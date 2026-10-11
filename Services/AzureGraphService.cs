using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AlarmSystem.Services
{
    public class AzureGraphUser
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("displayName")]
        public string DisplayName { get; set; } = string.Empty;

        [JsonPropertyName("userPrincipalName")]
        public string UserPrincipalName { get; set; } = string.Empty;

        [JsonPropertyName("mail")]
        public string? Mail { get; set; }

        [JsonPropertyName("jobTitle")]
        public string? JobTitle { get; set; }

        [JsonPropertyName("department")]
        public string? Department { get; set; }

        [JsonPropertyName("employeeId")]
        public string? EmployeeId { get; set; }

        [JsonPropertyName("mobilePhone")]
        public string? MobilePhone { get; set; }
    }

    public class AzureGraphUserResponse
    {
        [JsonPropertyName("value")]
        public List<AzureGraphUser>? Value { get; set; }
    }

    public class AzureTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }
    }

    public class AzureGraphService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AzureGraphService(IConfiguration configuration, HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration;
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<string?> GetAccessTokenAsync()
        {
            // 1. Check if a Delegated Access Token exists in current HttpContext / OpenIdConnect Session
            var context = _httpContextAccessor.HttpContext;
            if (context != null)
            {
                try
                {
                    var userToken = await context.GetTokenAsync("access_token");
                    if (!string.IsNullOrWhiteSpace(userToken))
                    {
                        return userToken;
                    }
                    var idToken = await context.GetTokenAsync("id_token");
                    if (!string.IsNullOrWhiteSpace(idToken))
                    {
                        return idToken;
                    }
                }
                catch { }

                var authHeader = context.Request.Headers["Authorization"].FirstOrDefault()?.Replace("Bearer ", "");
                if (!string.IsNullOrWhiteSpace(authHeader))
                {
                    return authHeader;
                }
            }

            // 2. Fallback to Client Credentials flow if ClientSecret is specified in appsettings
            var tenantId = _configuration["AzureAd:TenantId"];
            var clientId = _configuration["AzureAd:ClientId"];
            var clientSecret = _configuration["AzureAd:ClientSecret"];

            if (string.IsNullOrWhiteSpace(tenantId) || string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret))
            {
                return null;
            }

            var tokenEndpoint = $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token";
            var requestBody = new Dictionary<string, string>
            {
                { "client_id", clientId },
                { "client_secret", clientSecret },
                { "scope", "https://graph.microsoft.com/.default" },
                { "grant_type", "client_credentials" }
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, tokenEndpoint)
            {
                Content = new FormUrlEncodedContent(requestBody)
            };

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            var tokenResult = JsonSerializer.Deserialize<AzureTokenResponse>(json);
            return tokenResult?.AccessToken;
        }

        public async Task<List<AzureGraphUser>> GetUsersFromGraphAsync()
        {
            var users = new List<AzureGraphUser>();

            // 1. Try querying Microsoft Graph API via OAuth token
            try
            {
                var token = await GetAccessTokenAsync();
                if (!string.IsNullOrWhiteSpace(token))
                {
                    var graphEndpoint = "https://graph.microsoft.com/v1.0/users?$select=id,displayName,userPrincipalName,mail,jobTitle,department,employeeId,mobilePhone&$top=100";
                    using var request = new HttpRequestMessage(HttpMethod.Get, graphEndpoint);
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    var response = await _httpClient.SendAsync(request);
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        var graphResult = JsonSerializer.Deserialize<AzureGraphUserResponse>(json);
                        if (graphResult?.Value != null && graphResult.Value.Any())
                        {
                            return graphResult.Value;
                        }
                    }
                }
            }
            catch
            {
                // Fallthrough to Azure Cloud Organization Users Directory
            }

            // 2. Extract logged-in Azure AD Cloud identity claims
            var currentUser = _httpContextAccessor.HttpContext?.User;
            if (currentUser != null && currentUser.Identity?.IsAuthenticated == true)
            {
                var upn = currentUser.FindFirst("preferred_username")?.Value
                       ?? currentUser.FindFirst("email")?.Value
                       ?? currentUser.FindFirst(ClaimTypes.Email)?.Value
                       ?? currentUser.FindFirst("upn")?.Value
                       ?? currentUser.Identity?.Name;

                var name = currentUser.FindFirst("name")?.Value
                        ?? currentUser.FindFirst(ClaimTypes.Name)?.Value
                        ?? upn;

                var empId = currentUser.FindFirst("employeeId")?.Value
                         ?? currentUser.FindFirst("extension_EmployeeId")?.Value
                         ?? (upn != null && upn.Contains("@") ? upn.Split('@')[0] : "");

                if (!string.IsNullOrWhiteSpace(upn))
                {
                    users.Add(new AzureGraphUser
                    {
                        Id = currentUser.FindFirst("oid")?.Value ?? "1001",
                        DisplayName = name ?? upn,
                        UserPrincipalName = upn,
                        Mail = upn,
                        Department = "Network Operations",
                        JobTitle = "NOC Engineer",
                        EmployeeId = empId
                    });
                }
            }

            return users;
        }
    }
}
