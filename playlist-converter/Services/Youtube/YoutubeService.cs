using Microsoft.AspNetCore.DataProtection.KeyManagement;
using playlist_converter.Models.Youtube;
using System;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace playlist_converter.Services.Youtube;

public class YoutubeService : IYoutubeService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<YoutubeService> _logger;

    public YoutubeService(HttpClient httpClient, ILogger<YoutubeService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    // Create Youtube playlist
    public async Task<string> CreateYoutubePlaylistAsync(string title, string privacyStatus, string accessToken)
    {
        ValidateParameters(title, privacyStatus, accessToken);

        var content = CreateRequestContent(title, privacyStatus.ToLower());
        ConfigureAuthorizationHeader(accessToken);

        var url = "https://youtube.googleapis.com/youtube/v3/playlists?part=id%2Csnippet&key=";

        var response = await SendApiRequestAsync(content, url);
        return ExtractPlaylistIdFromResponse(response);
    }

    public async Task<string> SearchVideo(string query)
    {
        var url = $"https://youtube.googleapis.com/youtube/v3/search?part=snippet&maxResults=1&q={query}&type=video&key=";

        var response = await SendApiRequestAsync(null, url);

        var responseData = JsonSerializer.Deserialize<YoutubeSearchVideo>(response);

        string videoId = null;
        foreach (var item in responseData.items)
        {
            videoId = item.id.videoId;
            // Use videoId as needed
        }

        return videoId;
    }

    public async Task AddToPlaylist(string videoId, string playlistId, string accessToken)
    {
        string APIkey = "YOUR_API_KEY";

        // Prepare the request body
        var requestBody = new
        {
            snippet = new
            {
                playlistId = playlistId,
                resourceId = new
                {
                    kind = "youtube#video",
                    videoId = videoId
                }
            }
        };

        var json = JsonSerializer.Serialize(requestBody);
        ConfigureAuthorizationHeader(accessToken);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var url = "https://www.googleapis.com/youtube/v3/playlistItems?part=snippet";

        var response = await _httpClient.PostAsync("https://www.googleapis.com/youtube/v3/playlistItems?part=snippet", content);

        if (!response.IsSuccessStatusCode)
        {
            var errorResponse = await response.Content.ReadAsStringAsync();
            // Log the errorResponse or throw a detailed exception
            throw new HttpRequestException($"YouTube API call failed: {response.StatusCode}\n{errorResponse}");
        }

        response.EnsureSuccessStatusCode();

    }

    private void ValidateParameters(string title, string privacyStatus, string accessToken)
    {
        if (string.IsNullOrEmpty(accessToken))
        {
            _logger.LogError("Access token is null or empty.");
            throw new ArgumentException("Access token cannot be null or empty.", nameof(accessToken));
        }

        if (string.IsNullOrEmpty(title))
        {
            _logger.LogError("Title is null or empty.");
            throw new ArgumentException("Title cannot be null or empty.", nameof(title));
        }

        var validPrivacyStatuses = new[] { "public", "private", "unlisted" };
        if (!validPrivacyStatuses.Contains(privacyStatus.ToLower()))
        {
            _logger.LogError("Invalid privacy status provided.");
            throw new ArgumentException("Privacy status must be 'public', 'private', or 'unlisted'.", nameof(privacyStatus));
        }
    }

    private StringContent CreateRequestContent(string title, string privacyStatus)
    {
        var requestBody = new
        {
            snippet = new
            {
                title,
                description = "Playlist created from Spotify data.",
                privacyStatus
            }
        };

        var json = JsonSerializer.Serialize(requestBody);
        return new StringContent(json, Encoding.UTF8, "application/json");
    }

    private void ConfigureAuthorizationHeader(string accessToken)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    }

    private async Task<string> SendApiRequestAsync(StringContent? content, string url)
    {
        var apiKey = Environment.GetEnvironmentVariable("YOUTUBE_API_KEY");


        if (content is null)
        {
           var response = await _httpClient.GetAsync($"{url}{apiKey}");

           if (!response.IsSuccessStatusCode)
           {
               var errorContent = await response.Content.ReadAsStringAsync();
               _logger.LogError($"Failed find video: {errorContent}");
               throw new HttpRequestException($"Failed to find video: {response.StatusCode} - {errorContent}");
           }
           return await response.Content.ReadAsStringAsync();
        }
        else
        {
           var response = await _httpClient.PostAsync(
                $"{url}{apiKey}",
                content);

           if (!response.IsSuccessStatusCode)
           {
               var errorContent = await response.Content.ReadAsStringAsync();
               _logger.LogError($"Failed to create playlist: {errorContent}");
               throw new HttpRequestException($"Failed to create playlist: {response.StatusCode} - {errorContent}");
           }
           return await response.Content.ReadAsStringAsync();
        }
    }

    private string ExtractPlaylistIdFromResponse(string responseContent)
    {
        var responseData = JsonSerializer.Deserialize<YoutubePlaylistResponse>(responseContent);
        return responseData.id;
    }

}