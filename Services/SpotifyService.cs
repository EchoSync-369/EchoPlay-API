using EchoPlayAPI.Models;
using System.Text;
using System.Text.Json;

namespace EchoPlayAPI.Services
{
    public class SpotifyService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public SpotifyService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<SpotifyTokenResponse?> ExchangeCodeForTokenAsync(string code)
        {
            var clientId = _configuration["Spotify:ClientId"];
            var clientSecret = _configuration["Spotify:ClientSecret"];
            var redirectUri = _configuration["Spotify:RedirectUri"];

            var tokenRequest = new Dictionary<string, string>
            {
                {"grant_type", "authorization_code"},
                {"code", code},
                {"redirect_uri", redirectUri},
                {"client_id", clientId},
                {"client_secret", clientSecret}
            };

            var content = new FormUrlEncodedContent(tokenRequest);
            var response = await _httpClient.PostAsync("https://accounts.spotify.com/api/token", content);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<SpotifyTokenResponse>(json);
            }

            return null;
        }

        public async Task<SpotifyUser?> GetUserProfileAsync(string accessToken)
        {
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

            var response = await _httpClient.GetAsync("https://api.spotify.com/v1/me");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<SpotifyUser>(json);
            }

            return null;
        }

        public string GetAuthorizationUrl()
        {
            var clientId = _configuration["Spotify:ClientId"];
            var redirectUri = _configuration["Spotify:RedirectUri"];
            var scopes = "user-read-email user-read-private streaming user-read-playback-state user-modify-playback-state";

            var queryParams = new Dictionary<string, string>
            {
                {"client_id", clientId},
                {"response_type", "code"},
                {"redirect_uri", redirectUri},
                {"scope", scopes},
                {"state", Guid.NewGuid().ToString()} // For security
            };

            var queryString = string.Join("&", queryParams.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));
            return $"https://accounts.spotify.com/authorize?{queryString}";
        }
    }
}