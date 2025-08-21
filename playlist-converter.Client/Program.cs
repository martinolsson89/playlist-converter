using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.Authorization;
using PlaylistConverter.Client;
using PlaylistConverter.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Config-based API base
var apiBaseSetting = builder.Configuration["ApiBaseUrl"];
var apiBase = string.IsNullOrWhiteSpace(apiBaseSetting)
    ? builder.HostEnvironment.BaseAddress
    : (apiBaseSetting.EndsWith('/') ? apiBaseSetting : apiBaseSetting + "/");

builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(apiBase) });

builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<AuthenticationStateProvider, ApiAuthenticationStateProvider>();
builder.Services.AddScoped<PlaylistConverterService>();

await builder.Build().RunAsync();
