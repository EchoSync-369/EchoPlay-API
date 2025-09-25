using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using EchoPlayAPI.Services;
using EchoPlayAPI.Data;
using EchoPlayAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace EchoPlayAPI.Controllers
{
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly JWTService _jwtService;
        private readonly AppDbContext _context;

        public AuthController(JWTService jwtService, AppDbContext context)
        {
            _jwtService = jwtService;
            _context = context;
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

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                // Create new user
                user = new User
                {
                    Email = email,
                    IsAdmin = false // Set default value for new users
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }

            // Generate JWT token with the user's email
            var jwtToken = _jwtService.GenerateToken(email);

            return Ok(new { IsAdmin = user.IsAdmin, profile = json, token = jwtToken });
        }
    }

    public class TokenRequest
    {
        public required string Token { get; set; }
    }
}