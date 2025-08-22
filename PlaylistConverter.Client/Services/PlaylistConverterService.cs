using System.Net.Http.Json;
using PlaylistConverter.Shared.Models.Auth;
using PlaylistConverter.Shared.Models.Youtube;

namespace PlaylistConverter.Client.Services;

public class PlaylistConverterService
{
    private readonly HttpClient _http;

    public PlaylistConverterService(HttpClient http) => _http = http;

    public Task<string> GetYoutubeAuthUrlAsync(string redirectAbsoluteUrl) =>
        GetYoutubeAuthUrlInternalAsync(redirectAbsoluteUrl);

    private async Task<string> GetYoutubeAuthUrlInternalAsync(string? redirect)
    {
        var url = "api/YouTube/AuthUrl";
        if (!string.IsNullOrEmpty(redirect))
            url += $"?redirect={Uri.EscapeDataString(redirect)}";
        var resp = await _http.GetFromJsonAsync<AuthUrlResponse>(url);
        return resp?.AuthorizationUrl ?? throw new InvalidOperationException("Auth URL missing");
    }

    public Task<List<string>?> GetSpotifyPlaylistAsync(string playlistIdOrUrl) =>
        _http.GetFromJsonAsync<List<string>>($"api/Spotify/playlist?url={Uri.EscapeDataString(playlistIdOrUrl)}");

    public async Task<string> CreateYoutubePlaylistAsync(CreatePlaylistRequest request)
    {
        var resp = await _http.PostAsJsonAsync("api/YouTube/CreatePlaylist", request);
        resp.EnsureSuccessStatusCode();
        var dict = await resp.Content.ReadFromJsonAsync<Dictionary<string, string>>();
        return dict?["playlistId"] ?? throw new InvalidOperationException("playlistId missing");
    }

    public Task<Dictionary<string, object>?> AddFromSpotifyAsync(AddFromSpotifyRequest request) =>
        Post<AddFromSpotifyRequest, Dictionary<string, object>>("api/YouTube/AddFromSpotify", request);

    private async Task<TOut?> Post<TIn, TOut>(string url, TIn body)
    {
        var resp = await _http.PostAsJsonAsync(url, body);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync<TOut>();
    }

    private class AuthUrlResponse
    {
        public string AuthorizationUrl { get; set; } = "";
    }
}