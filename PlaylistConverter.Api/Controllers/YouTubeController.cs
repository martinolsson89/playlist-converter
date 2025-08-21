using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PlaylistConverter.Api.Services.Auth;
using PlaylistConverter.Api.Services.Spotify;
using PlaylistConverter.Api.Services.Youtube;
using PlaylistConverter.Shared.Models.Auth;
using PlaylistConverter.Shared.Models.Youtube;

namespace PlaylistConverter.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class YouTubeController : ControllerBase
    {
        private readonly IYoutubeAuthService _authService;
        private readonly IYoutubeService _youtubeService;
        private readonly ISpotifyService _spotifyService;
        private readonly ISpotifyAuthService _spotifyAuthService;
        private readonly ILogger<YouTubeController> _logger;

        public YouTubeController(
            IYoutubeAuthService authService,
            IYoutubeService youtubeService,
            ISpotifyService spotifyService,
            ISpotifyAuthService spotifyAuthService,
            ILogger<YouTubeController> logger)
        {
            _authService = authService;
            _youtubeService = youtubeService;
            _spotifyService = spotifyService;
            _spotifyAuthService = spotifyAuthService;
            _logger = logger;
        }

        // Returns the Google OAuth authorization URL (better for Swagger / SPA)
        [HttpGet("AuthUrl")]
        [ProducesResponseType(typeof(AuthUrlResponse), StatusCodes.Status200OK)]
        public IActionResult GetAuthUrl([FromQuery] string? redirect = null)
        {
            try
            {
                string state = string.IsNullOrEmpty(redirect) ? "" : $"redirect:{redirect}";
                var authUrl = _authService.GetAuthorizationUrl(state);
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
        public async Task<IActionResult> Callback([FromQuery] string code, [FromQuery] string error = "", [FromQuery] string? state = null)
        {
            if (!string.IsNullOrEmpty(error))
            {
                _logger.LogError("YouTube OAuth error: {Error}", error);
                return BadRequest($"OAuth error: {error}");
            }
            if (string.IsNullOrWhiteSpace(code))
                return BadRequest("Authorization code is missing.");

            try
            {
                var accessToken = await _authService.GetAccessTokenAsync(code);

                if (!string.IsNullOrEmpty(state) && state.StartsWith("redirect:"))
                {
                    var redirectUrl = state["redirect:".Length..];
                    // Place token in URL fragment (not sent to server hosting client)
                    return Redirect($"{redirectUrl}#access_token={Uri.EscapeDataString(accessToken)}");
                }

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

        // POST: api/YouTube/AddFromSpotify
        [HttpPost("AddFromSpotify")]
        public async Task<IActionResult> AddTracksFromSpotifyPlaylist([FromBody] AddFromSpotifyRequest request)
        {
            if (string.IsNullOrEmpty(request.SpotifyPlaylistId) ||
                string.IsNullOrEmpty(request.YouTubePlaylistId) ||
                string.IsNullOrEmpty(request.YouTubeAccessToken))
            {
                return BadRequest("SpotifyPlaylistId, YouTubePlaylistId, and YouTubeAccessToken are required");
            }

            try
            {
                // Get tracks from Spotify playlist
                var tracks = await _spotifyService.GetSpotifyPlaylistAsync(request.SpotifyPlaylistId);

                // Skip the first item as it's the playlist name
                var tracksList = tracks.Skip(1).ToList();
                var results = new List<object>();
                int successCount = 0;

                // For each track, search YouTube and add to playlist
                foreach (var track in tracksList)
                {
                    try
                    {
                        // Search for the video on YouTube
                        var videoId = await _youtubeService.SearchVideo(track);

                        if (!string.IsNullOrEmpty(videoId))
                        {
                            // Add the video to the YouTube playlist
                            await _youtubeService.AddToPlaylist(videoId, request.YouTubePlaylistId, request.YouTubeAccessToken);

                            results.Add(new
                            {
                                Track = track,
                                VideoId = videoId,
                                Status = "Added"
                            });

                            successCount++;
                        }
                        else
                        {
                            results.Add(new
                            {
                                Track = track,
                                VideoId = (string)null,
                                Status = "Not found"
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error adding track '{track}' to YouTube playlist");
                        results.Add(new
                        {
                            Track = track,
                            Error = ex.Message,
                            Status = "Error"
                        });
                    }

                    // Add a small delay to avoid rate limiting
                    await Task.Delay(200);
                }

                return Ok(new
                {
                    TotalTracks = tracksList.Count,
                    SuccessfullyAdded = successCount,
                    Results = results
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding tracks from Spotify playlist to YouTube playlist");
                return StatusCode(500, "Error processing the request: " + ex.Message);
            }
        }
    }
}

