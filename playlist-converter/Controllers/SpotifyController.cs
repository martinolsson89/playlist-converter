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

        [HttpGet("playlist/{playlistId}")]
        public async Task<IActionResult> Get()
        {
            var accessToken = await _spotifyAuthService.GetAccessTokenAsync();
            if (string.IsNullOrEmpty(accessToken))
            {
                _logger.LogError("Access token is null or empty.");
                return Unauthorized("Access token is required.");
            }

            try
            {
                var playlistId = HttpContext.Request.RouteValues["playlistId"]?.ToString();
                if (string.IsNullOrEmpty(playlistId))
                {
                    _logger.LogError("Playlist ID is null or empty.");
                    return BadRequest("Playlist ID is required.");
                }

                var tracks = _spotifyService.GetSpotifyPlaylistAsync(playlistId, accessToken).Result;
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
