using Microsoft.AspNetCore.Mvc;
using EchoPlayAPI.Services;

namespace EchoPlayAPI.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        private readonly SpotifyService _spotifyService;
        private readonly IConfiguration _configuration;

        public AuthController(SpotifyService spotifyService, IConfiguration configuration)
        {
            _spotifyService = spotifyService;
            _configuration = configuration;
        }

        [HttpGet("spotify")]
        public IActionResult InitiateSpotifyAuth()
        {
            var authUrl = _spotifyService.GetAuthorizationUrl();
            return Redirect(authUrl);
        }

        [HttpGet("spotify/callback")]
        public async Task<IActionResult> SpotifyCallback([FromQuery] string code, [FromQuery] string? error, [FromQuery] string? state)
        {
            var frontendUrl = _configuration["Frontend:BaseUrl"];

            if (!string.IsNullOrEmpty(error))
            {
                return Redirect($"{frontendUrl}/auth/error?error={error}");
            }

            if (string.IsNullOrEmpty(code))
            {
                return Redirect($"{frontendUrl}/auth/error?error=no_code");
            }

            try
            {
                // Exchange code for tokens
                var tokenResponse = await _spotifyService.ExchangeCodeForTokenAsync(code);
                if (tokenResponse == null)
                {
                    return Redirect($"{frontendUrl}/auth/error?error=token_exchange_failed");
                }

                // Get user profile
                var userProfile = await _spotifyService.GetUserProfileAsync(tokenResponse.access_token);
                if (userProfile == null)
                {
                    return Redirect($"{frontendUrl}/auth/error?error=profile_fetch_failed");
                }

                return Redirect($"{frontendUrl}/auth/success?user={userProfile.display_name}");
            }
            catch (Exception ex)
            {
                return Redirect($"{frontendUrl}/auth/error?error=server_error");
            }
        }
    }
}