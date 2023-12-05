using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using ScreenSound.Web;
using ScreenSound.Web.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// serviços necessários para autenticação/autorização
builder.Services.AddScoped<CookieHandler>();
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, AuthenticationService>();

// serviços que consomem a API do Screensound
builder.Services.AddScoped(
    sp => (IAuthenticationService)sp.GetRequiredService<AuthenticationStateProvider>());
builder.Services.AddScoped<ArtistaAPI>();
builder.Services.AddScoped<MusicaAPI>();

builder.Services.AddHttpClient("API",client => {
    client.BaseAddress = new Uri(builder.Configuration["APIServer:Url"]!);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
}).AddHttpMessageHandler<CookieHandler>();

builder.Services.AddMudServices();

await builder.Build().RunAsync();
