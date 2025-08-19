namespace playlist_converter.Models.Auth;

public class AddFromSpotifyRequest
{
    public string SpotifyPlaylistId { get; set; }
    public string YouTubePlaylistId { get; set; }
    public string YouTubeAccessToken { get; set; }
}