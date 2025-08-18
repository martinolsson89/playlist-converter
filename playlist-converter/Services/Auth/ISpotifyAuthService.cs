namespace playlist_converter.Services.Auth;

public interface ISpotifyAuthService
{
    Task<string> GetAccessTokenAsync();
    Task<bool> RefreshTokenAsync();
}