using System.Text;
using Newtonsoft.Json;
using PlaylistConverter.Shared.Models.Spotify;

namespace PlaylistConverter.Api.Services.Auth;

public class SpotifyAuthService : ISpotifyAuthService
{
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private readonly ILogger<SpotifyAuthService> _logger;

    public SpotifyAuthService(IConfiguration configuration, HttpClient httpClient, ILogger<SpotifyAuthService> logger)
    {
        _configuration = configuration;
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<string> GetAccessTokenAsync()
    {
        var clientId = Environment.GetEnvironmentVariable("SPOTIFY_CLIENT_ID");
        var clientSecret = Environment.GetEnvironmentVariable("SPOTIFY_CLIENT_SECRET");

        if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
        {
            _logger.LogError("Spotify ClientId or ClientSecret is not configured.");
            throw new InvalidOperationException("Spotify ClientId or ClientSecret is not configured.");
        }

        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));

        var tokenRequest = new HttpRequestMessage(HttpMethod.Post, "https://accounts.spotify.com/api/token")
        {
            Headers = {
                { "Authorization", $"Basic {credentials}" }
            },
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "grant_type", "client_credentials" }
            })
        };

        var response = await _httpClient.SendAsync(tokenRequest);

        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError($"Failed to retrieve access token: {content}");
            throw new HttpRequestException($"Failed to retrieve access token: {content}");
        }

        var tokenResponse = JsonConvert.DeserializeObject<SpotifyToken>(content);

        return tokenResponse!.Access_token;
    }

    public async Task<bool> RefreshTokenAsync()
    {
        // Implement token refresh logic
        throw new NotImplementedException();
    }
}