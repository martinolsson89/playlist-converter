namespace PlaylistConverter.Api.Services.Spotify;

public interface ISpotifyService
{
    Task<List<string>> GetSpotifyPlaylistAsync(string playlistId, string accessToken);
    Task<List<string>> GetSpotifyPlaylistAsync(string playlistUrl);

}