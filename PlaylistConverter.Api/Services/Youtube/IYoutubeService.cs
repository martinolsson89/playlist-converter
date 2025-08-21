namespace PlaylistConverter.Api.Services.Youtube;

public interface IYoutubeService
{
    Task<string> CreateYoutubePlaylistAsync(string title, string privacyStatus, string accessToken);
    Task<string> SearchVideo(string query);
    Task AddToPlaylist(string videoId, string playlistId, string accessToken);
}