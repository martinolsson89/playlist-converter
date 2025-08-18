namespace playlist_converter.Services.Spotify;

public interface ISpotifyService
{
    Task<List<string>> GetSpotifyPlaylistAsync(string playlistId, string accessToken);

}