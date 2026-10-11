using AlarmSystem.Configuration;
using AlarmSystem.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;

namespace AlarmSystem.Services
{
    public class StarlinkAuthService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMemoryCache _cache;
        private readonly StarlinkOptions _options;
        private const string CacheKey = "starlink_access_token";

        public StarlinkAuthService(IHttpClientFactory httpClientFactory, IMemoryCache cache, IOptions<StarlinkOptions> options)
        {
            _httpClientFactory = httpClientFactory;
            _cache = cache;
            _options = options.Value;
        }

        public bool HasAuthConfig()
        {
            var clientId = Environment.GetEnvironmentVariable("STARLINK_CLIENT_ID") ?? _options.ClientId;
            var clientSecret = Environment.GetEnvironmentVariable("STARLINK_CLIENT_SECRET") ?? _options.ClientSecret;
            var authUrl = GetAuthUrl();

            return !string.IsNullOrWhiteSpace(clientId)
                && !string.IsNullOrWhiteSpace(clientSecret)
                && !string.IsNullOrWhiteSpace(authUrl);
        }

        private string GetAuthUrl()
        {
            var configuredAuthUrl = Environment.GetEnvironmentVariable("STARLINK_AUTH_URL") ?? _options.AuthUrl;
            if (!string.IsNullOrWhiteSpace(configuredAuthUrl))
                return configuredAuthUrl;

            var apiBaseUrl = Environment.GetEnvironmentVariable("STARLINK_API_BASE_URL") ?? _options.ApiBaseUrl;
            if (string.IsNullOrWhiteSpace(apiBaseUrl))
                return string.Empty;

            try
            {
                var baseUri = new Uri(apiBaseUrl);
                var authUri = new Uri(baseUri, "/api/auth/connect/token");
                return authUri.ToString();
            }
            catch
            {
                return string.Empty;
            }
        }

        public async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default)
        {
            if (_cache.TryGetValue<string>(CacheKey, out var token) && token is not null)
                return token;

            var clientId = Environment.GetEnvironmentVariable("STARLINK_CLIENT_ID") ?? _options.ClientId;
            var clientSecret = Environment.GetEnvironmentVariable("STARLINK_CLIENT_SECRET") ?? _options.ClientSecret;
            var authUrl = GetAuthUrl();

            if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret) || string.IsNullOrWhiteSpace(authUrl))
                throw new InvalidOperationException("Starlink credentials/auth URL are not configured.");

            var client = _httpClientFactory.CreateClient();
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string,string>("grant_type","client_credentials"),
                new KeyValuePair<string,string>("client_id", clientId),
                new KeyValuePair<string,string>("client_secret", clientSecret),
            });

            using var req = new HttpRequestMessage(HttpMethod.Post, authUrl)
            {
                Content = content
            };

            using var resp = await client.SendAsync(req, cancellationToken);
            var text = await resp.Content.ReadAsStringAsync(cancellationToken);
            if (!resp.IsSuccessStatusCode)
                throw new HttpRequestException($"Token request failed: {resp.StatusCode} {text}");

            var tokenResp = System.Text.Json.JsonSerializer.Deserialize<TokenResponse>(text);
            if (tokenResp is null || string.IsNullOrWhiteSpace(tokenResp.access_token))
                throw new InvalidOperationException("Invalid token response from Starlink.");

            var expires = TimeSpan.FromSeconds(Math.Max(60, tokenResp.expires_in - 30));
            _cache.Set(CacheKey, tokenResp.access_token, DateTimeOffset.UtcNow.Add(expires));

            return tokenResp.access_token;
        }

        public void ClearToken()
        {
            _cache.Remove(CacheKey);
        }
    }
}
