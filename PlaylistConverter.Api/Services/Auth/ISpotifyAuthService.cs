namespace PlaylistConverter.Api.Services.Auth;

public interface ISpotifyAuthService
{
    Task<string> GetAccessTokenAsync();
    Task<bool> RefreshTokenAsync();
}