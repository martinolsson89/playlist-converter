namespace PlaylistConverter.Client.Services;

public class TokenService
{
    private string? _youtubeToken;
    public event Action? TokenChanged;

    public string? YoutubeToken
    {
        get => _youtubeToken;
        set
        {
            if (_youtubeToken == value) return;
            _youtubeToken = value;
            TokenChanged?.Invoke();
        }
    }

    public bool IsAuthenticated => !string.IsNullOrEmpty(_youtubeToken);
}