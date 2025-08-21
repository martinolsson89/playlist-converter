namespace PlaylistConverter.Api.Services.Auth;

public interface IYoutubeAuthService
{
    string GetAuthorizationUrl(string state = "");
    Task<string> GetAccessTokenAsync(string authorizationCode);
    Task<bool> RefreshTokenAsync(string refreshToken);
}