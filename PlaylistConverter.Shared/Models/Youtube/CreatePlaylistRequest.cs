namespace PlaylistConverter.Shared.Models.Youtube;

public class CreatePlaylistRequest
{
    public string Title { get; set; } = string.Empty;
    public string PrivacyStatus { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
}