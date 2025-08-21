using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace PlaylistConverter.Client.Services;

public class ApiAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly TokenService _tokens;

    public ApiAuthenticationStateProvider(TokenService tokens)
    {
        _tokens = tokens;
        _tokens.TokenChanged += () => NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        if (string.IsNullOrEmpty(_tokens.YoutubeToken))
            return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));

        var claims = new List<Claim>
        {
            new("provider", "youtube"),
            new("token_present", "true")
        };
        var identity = new ClaimsIdentity(claims, "youtube");
        return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity)));
    }
}