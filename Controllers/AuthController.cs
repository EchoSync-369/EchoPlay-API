using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using EchoPlayAPI.Services;

namespace EchoPlayAPI.Controllers
{
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly JWTService _jwtService;

        public AuthController(JWTService jwtService)
        {
            _jwtService = jwtService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> GetSpotifyUserProfile([FromBody] TokenRequest request)
        {
            if (string.IsNullOrEmpty(request?.Token))
                return BadRequest("Token is required");

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", request.Token);
            var response = await httpClient.GetAsync("https://api.spotify.com/v1/me");
            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());

            var content = await response.Content.ReadAsStringAsync();
            var json = JsonDocument.Parse(content).RootElement;

            // Extract email from the Spotify user profile response
            var email = json.GetProperty("email").GetString();
            
            if (string.IsNullOrEmpty(email))
                return BadRequest("User email not found in Spotify profile");
            
            // Generate JWT token with the user's email
            var jwtToken = _jwtService.GenerateToken(email);
            
            return Ok(new { profile = json, token = jwtToken });
        }
        
    }

    public class TokenRequest
    {
        public required string Token { get; set; }
    }
}