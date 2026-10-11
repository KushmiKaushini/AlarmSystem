using AlarmSystem.Configuration;
using AlarmSystem.Models;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text.Json;

namespace AlarmSystem.Services
{
    public class StarlinkApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly StarlinkAuthService _authService;
        private readonly StarlinkOptions _options;

        public StarlinkApiService(IHttpClientFactory httpClientFactory, StarlinkAuthService authService, IOptions<StarlinkOptions> options)
        {
            _httpClientFactory = httpClientFactory;
            _authService = authService;
            _options = options.Value;
        }

        public async Task<ApiTestResponse> CallEndpointAsync(string endpoint, string method = "GET", string bearerToken = "", string requestBody = "", CancellationToken cancellationToken = default)
        {
            var useManualBearer = !string.IsNullOrWhiteSpace(bearerToken);
            string? token = null;

            if (useManualBearer)
            {
                token = bearerToken;
            }
            else if (_authService.HasAuthConfig())
            {
                token = await _authService.GetAccessTokenAsync(cancellationToken);
            }

            var apiBaseUrl = Environment.GetEnvironmentVariable("STARLINK_API_BASE_URL") ?? _options.ApiBaseUrl;
            if (string.IsNullOrWhiteSpace(apiBaseUrl))
                throw new InvalidOperationException("Starlink API base URL is not configured.");

            var client = _httpClientFactory.CreateClient("starlink");
            client.BaseAddress = new Uri(apiBaseUrl.TrimEnd('/') + "/");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.UserAgent.ParseAdd("AlarmSystem/1.0");
            if (!string.IsNullOrWhiteSpace(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var requestUri = endpoint?.TrimStart('/') ?? string.Empty;
            var sw = Stopwatch.StartNew();
            using var req = new HttpRequestMessage(new HttpMethod(method), requestUri);
            if (!string.IsNullOrWhiteSpace(requestBody) && (method ?? "GET").ToUpperInvariant() != "GET")
            {
                req.Content = new StringContent(requestBody, System.Text.Encoding.UTF8, "application/json");
            }
            using var resp = await client.SendAsync(req, cancellationToken);
            sw.Stop();

            var text = await resp.Content.ReadAsStringAsync(cancellationToken);
            var apiResp = new ApiTestResponse
            {
                Status = (int)resp.StatusCode,
                StatusText = resp.ReasonPhrase ?? string.Empty,
                ResponseTime = sw.ElapsedMilliseconds,
                Timestamp = DateTime.UtcNow
            };
            apiResp.RawBody = text ?? string.Empty;

            try
            {
                if (!string.IsNullOrWhiteSpace(text))
                {
                    apiResp.Data = JsonSerializer.Deserialize<JsonElement>(text);
                }
            }
            catch
            {
                apiResp.Data = null;
            }

            if (!useManualBearer && resp.StatusCode == System.Net.HttpStatusCode.Unauthorized && _authService.HasAuthConfig())
            {
                // Try refresh token and retry once with a fresh request message.
                _authService.ClearToken();
                token = await _authService.GetAccessTokenAsync(cancellationToken);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                sw.Restart();
                using var req2 = new HttpRequestMessage(new HttpMethod(method ?? "GET"), requestUri);
                if (!string.IsNullOrWhiteSpace(requestBody) && (method ?? "GET").ToUpperInvariant() != "GET")
                {
                    req2.Content = new StringContent(requestBody, System.Text.Encoding.UTF8, "application/json");
                }
                using var resp2 = await client.SendAsync(req2, cancellationToken);
                sw.Stop();
                var text2 = await resp2.Content.ReadAsStringAsync(cancellationToken);

                apiResp.Status = (int)resp2.StatusCode;
                apiResp.StatusText = resp2.ReasonPhrase ?? string.Empty;
                apiResp.ResponseTime = sw.ElapsedMilliseconds;
                apiResp.Timestamp = DateTime.UtcNow;
                apiResp.RawBody = text2 ?? string.Empty;
                try { apiResp.Data = JsonSerializer.Deserialize<JsonElement>(text2 ?? string.Empty); } catch { apiResp.Data = null; }
            }

            return apiResp;
        }
    }
}
