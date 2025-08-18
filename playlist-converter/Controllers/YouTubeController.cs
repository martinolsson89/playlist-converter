using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using playlist_converter.Services.Auth;
using playlist_converter.Services.Youtube;

namespace playlist_converter.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class YouTubeController : ControllerBase
    {
        private readonly IYoutubeAuthService _authService;
        private readonly IYoutubeService _youtubeService;
        private readonly ILogger<YouTubeController> _logger;

        public YouTubeController(IYoutubeAuthService authService, IYoutubeService youtubeService, ILogger<YouTubeController> logger)
        {
            _authService = authService;
            _youtubeService = youtubeService;
            _logger = logger;
        }

        // Returns the Google OAuth authorization URL (better for Swagger / SPA)
        [HttpGet("AuthUrl")]
        [ProducesResponseType(typeof(AuthUrlResponse), StatusCodes.Status200OK)]
        public IActionResult GetAuthUrl()
        {
            try
            {
                var authUrl = _authService.GetAuthorizationUrl();
                return Ok(new AuthUrlResponse { AuthorizationUrl = authUrl });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating YouTube authorization URL.");
                return StatusCode(500, "Failed to generate authorization URL.");
            }
        }

        //GET: api/YouTube Login
        [HttpGet("Login")]
        public IActionResult Login()
        {
            try
            {
                string authUrl = _authService.GetAuthorizationUrl();
                return Redirect(authUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during YouTube login");
                return StatusCode(500, "Error during authentication process");
            }
        }

        [HttpGet("Callback")]
        public async Task<IActionResult> Callback([FromQuery] string code, [FromQuery] string error = "")
        {
            if (!string.IsNullOrEmpty(error))
            {
                _logger.LogError("YouTube OAuth error: {Error}", error);
                return BadRequest($"OAuth error: {error}");
            }

            if (string.IsNullOrWhiteSpace(code))
            {
                return BadRequest("Authorization code is missing.");
            }

            try
            {
                var accessToken = await _authService.GetAccessTokenAsync(code);
                return Ok(new { AccessToken = accessToken });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during YouTube callback processing.");
                return StatusCode(500, "Error during authentication callback process");
            }
        }

        [HttpPost("CreatePlaylist")]
        public async Task<IActionResult> CreatePlaylist([FromBody] CreatePlaylistRequest request)
        {
            if (string.IsNullOrEmpty(request.Title) || string.IsNullOrEmpty(request.AccessToken))
            {
                return BadRequest("Title and access token are required");
            }

            try
            {
                var privacyStatus = string.IsNullOrEmpty(request.PrivacyStatus) ? "private" : request.PrivacyStatus;
                var playlistId = await _youtubeService.CreateYoutubePlaylistAsync(
                    request.Title,
                    privacyStatus,
                    request.AccessToken);

                return Ok(new { PlaylistId = playlistId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating YouTube playlist");
                return StatusCode(500, "Error creating playlist");
            }
        }

        public class CreatePlaylistRequest
        {
            public string Title { get; set; } = string.Empty;
            public string PrivacyStatus { get; set; } = string.Empty;
            public string AccessToken { get; set; } = string.Empty;
        }
        public class AuthUrlResponse
        {
            public string AuthorizationUrl { get; set; } = string.Empty;
        }
    }
}

