using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using playlist_converter.Services.Auth;
using playlist_converter.Services.Spotify;

namespace playlist_converter.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SpotifyController : ControllerBase
    {
        private readonly ISpotifyService _spotifyService;
        private readonly ISpotifyAuthService _spotifyAuthService;
        private readonly ILogger<SpotifyController> _logger;

        public SpotifyController(ISpotifyService spotifyService, ISpotifyAuthService spotifyAuthService,
            ILogger<SpotifyController> logger)
        {
            _spotifyService = spotifyService;
            _spotifyAuthService = spotifyAuthService;
            _logger = logger;
        }

        [HttpGet("playlist")]
        public async Task<IActionResult> Get([FromQuery] string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                _logger.LogError("Playlist URL is null or empty.");
                return BadRequest("Playlist URL is required.");
            }

            var accessToken = await _spotifyAuthService.GetAccessTokenAsync();
            if (string.IsNullOrEmpty(accessToken))
            {
                _logger.LogError("Access token is null or empty.");
                return Unauthorized("Access token is required.");
            }

            try
            {
                var tracks = await _spotifyService.GetSpotifyPlaylistAsync(url, accessToken);
                return Ok(tracks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving the Spotify playlist.");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
