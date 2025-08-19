using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using playlist_converter.Models.Spotify;

namespace playlist_converter.Services.Spotify;

public class SpotifyService : ISpotifyService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<SpotifyService> _logger;
    private readonly IMemoryCache _cache;
    private const string CACHE_KEY_PREFIX = "spotify_playlist_";
    private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(30);

    public SpotifyService(HttpClient httpClient, ILogger<SpotifyService> logger, IMemoryCache cache)
    {
        _httpClient = httpClient;
        _logger = logger;
        _cache = cache;
    }

    public async Task<List<string>> GetSpotifyPlaylistAsync(string playlistUrl, string accessToken)
    {
        var playlistId = ExtractPlaylistId(playlistUrl);

        string cacheKey = $"{CACHE_KEY_PREFIX}{playlistId}";

        // Try to get from cache first
        if (_cache.TryGetValue(cacheKey, out List<string> cachedPlaylist))
        {
            _logger.LogInformation($"Retrieved playlist {playlistId} from cache");
            return cachedPlaylist;
        }

        var request = new HttpRequestMessage(HttpMethod.Get, $"https://api.spotify.com/v1/playlists/{playlistId}?market=se");

        if (string.IsNullOrEmpty(accessToken))
        {
            _logger.LogError("Access token is null or empty.");
            throw new ArgumentException("Access token cannot be null or empty.", nameof(accessToken));
        }

        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        _logger.LogInformation($"Requesting Spotify playlist with ID: {playlistId}");

        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError($"Failed to retrieve playlist: {errorContent}");
            throw new HttpRequestException($"Failed to retrieve playlist: {response.StatusCode} - {errorContent}");
        }

        var content = await response.Content.ReadAsStringAsync();
        var playlistResponse = JsonConvert.DeserializeObject<SpotifyPlaylistResponse>(content);

        var result = new List<string> { playlistResponse.PlaylistName };

        foreach (var item in playlistResponse.Tracks.Items)
        {
            var trackName = item.Track.Name;
            var artistName = item.Track.Artists[0].Name; // Assuming there's at least one artist per track
            result.Add($"{artistName} - {trackName}");
        }

        // Cache the result
        _cache.Set(cacheKey, result, _cacheDuration);
        _logger.LogInformation($"Cached playlist {playlistId} for {_cacheDuration.TotalMinutes} minutes");

        return result;
    }

    // private method that extracts the playlist ID from the URL
    private string ExtractPlaylistId(string playlistUrl)
    {
        if (string.IsNullOrEmpty(playlistUrl))
        {
            _logger.LogError("Playlist URL is null or empty");
            throw new ArgumentException("Playlist URL cannot be null or empty", nameof(playlistUrl));
        }

        // First, URL-decode the input to handle cases where the URL is passed with encoding
        playlistUrl = Uri.UnescapeDataString(playlistUrl);

        // Check if the string is already just a playlist ID (no URL)
        if (!playlistUrl.Contains("/"))
        {
            return playlistUrl;
        }

        // Handle spotify URI format: spotify:playlist:4F1qNLynRpVGPqbBhD2HYK
        if (playlistUrl.StartsWith("spotify:playlist:"))
        {
            return playlistUrl.Split(':')[2];
        }

        // Extract ID from URL format like: https://open.spotify.com/playlist/4F1qNLynRpVGPqbBhD2HYK?si=7bad1081c7244753
        try
        {
            Uri uri = new Uri(playlistUrl);
            string path = uri.AbsolutePath;

            // Get segment after "playlist/"
            string[] segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            int playlistIndex = Array.FindIndex(segments, s => s.Equals("playlist", StringComparison.OrdinalIgnoreCase));

            if (playlistIndex >= 0 && playlistIndex < segments.Length - 1)
            {
                return segments[playlistIndex + 1];
            }

            _logger.LogError($"Could not find playlist ID in URL: {playlistUrl}");
            throw new ArgumentException($"Invalid Spotify playlist URL format: {playlistUrl}", nameof(playlistUrl));
        }
        catch (UriFormatException ex)
        {
            _logger.LogError(ex, $"Invalid URL format: {playlistUrl}");
            throw new ArgumentException($"Invalid URL format: {playlistUrl}", nameof(playlistUrl), ex);
        }
    }
}