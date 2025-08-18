using Newtonsoft.Json;
using playlist_converter.Models.Spotify;

namespace playlist_converter.Services.Spotify;

public class SpotifyService : ISpotifyService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<SpotifyService> _logger;

    public SpotifyService(HttpClient httpClient, ILogger<SpotifyService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<List<string>> GetSpotifyPlaylistAsync(string playlistId, string accessToken)
    {
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

        return result;
    }
}