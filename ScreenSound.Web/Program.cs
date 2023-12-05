using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using ScreenSound.Web;
using ScreenSound.Web.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddMudServices();

// serviços necessários para autenticação/autorização
builder.Services.AddAuthorizationCore();
builder.Services.AddTransient<CookieHandler>();
builder.Services.AddTransient<AuthenticationStateProvider, AuthenticationService>();

// serviços que consomem a API do Screensound
builder.Services.AddTransient<IAuthenticationService, AuthenticationService>();
builder.Services.AddTransient<ArtistaAPI>();
builder.Services.AddTransient<MusicaAPI>();

builder.Services.AddHttpClient("API",client => {
    client.BaseAddress = new Uri(builder.Configuration["APIServer:Url"]!);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
}).AddHttpMessageHandler<CookieHandler>();


await builder.Build().RunAsync();
