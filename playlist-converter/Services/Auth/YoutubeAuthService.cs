using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.YouTube.v3;
using Microsoft.Extensions.Configuration;

namespace playlist_converter.Services.Auth;

public class YoutubeAuthService : IYoutubeAuthService
{

    private readonly ILogger<YoutubeAuthService> _logger;
    private readonly GoogleAuthorizationCodeFlow _flow;
    private readonly string _redirectUri;

    // A static user id placeholder – adapt to a real user context if you add accounts.
    private const string UserId = "default-user";

    public YoutubeAuthService(IConfiguration configuration, HttpClient httpClient, ILogger<YoutubeAuthService> logger)
    {
        _logger = logger;

        var clientId = Environment.GetEnvironmentVariable("YOUTUBE_CLIENT_ID");
        var clientSecret = Environment.GetEnvironmentVariable("YOUTUBE_CLIENT_SECRET");
        _redirectUri = Environment.GetEnvironmentVariable("YOUTUBE_REDIRECT_URI") ?? string.Empty;

        if (string.IsNullOrWhiteSpace(clientId) ||
            string.IsNullOrWhiteSpace(clientSecret) ||
            string.IsNullOrWhiteSpace(_redirectUri))
        {
            _logger.LogError("YouTube OAuth environment variables are missing (YOUTUBE_CLIENT_ID / YOUTUBE_CLIENT_SECRET / YOUTUBE_REDIRECT_URI).");
            throw new InvalidOperationException("YouTube OAuth configuration incomplete.");
        }

        _flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
        {
            ClientSecrets = new ClientSecrets
            {
                ClientId = clientId,
                ClientSecret = clientSecret
            },
            Scopes = new[]
            {
                YouTubeService.Scope.Youtube
            },
            Prompt = "consent"       // Force consent to guarantee refresh token on first auth.
        });
    }

    public string GetAuthorizationUrl()
    {
        try
        {
            // Builds a standards-compliant OAuth2 authorization URL via the Google library.
            var url = _flow.CreateAuthorizationCodeRequest(_redirectUri).Build();
            return url.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to build YouTube authorization URL.");
            throw;
        }
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