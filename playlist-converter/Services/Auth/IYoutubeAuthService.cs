namespace playlist_converter.Services.Auth;

public interface IYoutubeAuthService
{
    string GetAuthorizationUrl();
    Task<string> GetAccessTokenAsync(string authorizationCode);
    Task<bool> RefreshTokenAsync(string refreshToken);
}