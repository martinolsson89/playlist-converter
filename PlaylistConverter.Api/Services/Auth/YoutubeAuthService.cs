using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.YouTube.v3;

namespace PlaylistConverter.Api.Services.Auth;

public class YoutubeAuthService : IYoutubeAuthService
{

    private readonly ILogger<YoutubeAuthService> _logger;
    private readonly GoogleAuthorizationCodeFlow _flow;
    private readonly string _redirectUri;
    private readonly string _clientId;
    private readonly string _clientSecret;
    private readonly string[] _scopes;

    // A static user id placeholder – adapt to a real user context if you add accounts.
    private const string UserId = "default-user";

    public YoutubeAuthService(ILogger<YoutubeAuthService> logger)
    {
        _logger = logger;

        _clientId = Environment.GetEnvironmentVariable("YOUTUBE_CLIENT_ID") ?? string.Empty;
        _clientSecret = Environment.GetEnvironmentVariable("YOUTUBE_CLIENT_SECRET") ?? string.Empty;
        _redirectUri = Environment.GetEnvironmentVariable("YOUTUBE_REDIRECT_URI") ?? string.Empty;
        _scopes = new[] { YouTubeService.Scope.Youtube };

        if (string.IsNullOrWhiteSpace(_clientId) ||
            string.IsNullOrWhiteSpace(_clientSecret) ||
            string.IsNullOrWhiteSpace(_redirectUri))
        {
            _logger.LogError("YouTube OAuth environment variables are missing (YOUTUBE_CLIENT_ID / YOUTUBE_CLIENT_SECRET / YOUTUBE_REDIRECT_URI).");
            throw new InvalidOperationException("YouTube OAuth configuration incomplete.");
        }

        _flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
        {
            ClientSecrets = new ClientSecrets
            {
                ClientId = _clientId,
                ClientSecret = _clientSecret
            },
            Scopes = _scopes,
            Prompt = "consent"       // Force consent to guarantee refresh token on first auth.
        });
    }

    public string GetAuthorizationUrl(string state = "")
    {
        var scopeUrl = string.Join(" ", _scopes);
        var url = $"https://accounts.google.com/o/oauth2/auth" +
                  $"?client_id={_clientId}" +
                  $"&redirect_uri={Uri.EscapeDataString(_redirectUri)}" +
                  $"&scope={Uri.EscapeDataString(scopeUrl)}" +
                  $"&response_type=code&access_type=offline&prompt=consent";
        if (!string.IsNullOrEmpty(state))
            url += $"&state={Uri.EscapeDataString(state)}";
        return url;
    }

    public async Task<string> GetAccessTokenAsync(string authorizationCode)
    {
        if (string.IsNullOrWhiteSpace(authorizationCode))
        {
            _logger.LogError("Authorization code was null or empty when exchanging for token.");
            throw new ArgumentException("Authorization code is required.", nameof(authorizationCode));
        }

        try
        {
            TokenResponse tokenResponse = await _flow.ExchangeCodeForTokenAsync(
                UserId,
                authorizationCode,
                _redirectUri,
                CancellationToken.None);

            if (string.IsNullOrWhiteSpace(tokenResponse.AccessToken))
            {
                _logger.LogError("Received empty access token from Google token exchange.");
                throw new InvalidOperationException("Empty access token received.");
            }

            // (Optional) Persist refresh token (tokenResponse.RefreshToken) somewhere secure for the user.
            return tokenResponse.AccessToken;
        }
        catch (TokenResponseException tre)
        {
            _logger.LogError(tre, "Google token exchange failed: {Error}", tre.Error?.ErrorDescription);
            throw new InvalidOperationException($"Token exchange failed: {tre.Error?.ErrorDescription}", tre);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during Google token exchange.");
            throw;
        }
    }

    public async Task<bool> RefreshTokenAsync(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            _logger.LogError("Refresh token was null or empty.");
            throw new ArgumentException("Refresh token is required.", nameof(refreshToken));
        }

        try
        {
            var refreshed = await _flow.RefreshTokenAsync(UserId, refreshToken, CancellationToken.None);

            if (refreshed == null || string.IsNullOrWhiteSpace(refreshed.AccessToken))
            {
                _logger.LogWarning("Refresh token call returned no access token.");
                return false;
            }

            return true;
        }
        catch (TokenResponseException tre)
        {
            _logger.LogError(tre, "Failed to refresh YouTube access token: {Error}", tre.Error?.ErrorDescription);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error refreshing YouTube access token.");
            return false;
        }
    }
}