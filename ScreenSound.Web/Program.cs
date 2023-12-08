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

builder.Services.AddTransient<CookieHandler>();
builder.Services.AddAuthorizationCore();
builder.Services.AddTransient<AuthenticationStateProvider, AuthAPI>();
builder.Services.AddTransient<AuthAPI>(sp => (AuthAPI)sp.GetRequiredService<AuthenticationStateProvider>());
builder.Services.AddCascadingAuthenticationState();

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddTransient<ArtistaAPI>();
builder.Services.AddTransient<MusicaAPI>();


builder.Services.AddHttpClient("API",client => {
    client.BaseAddress = new Uri(builder.Configuration["APIServer:Url"]!);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
}).AddHttpMessageHandler<CookieHandler>();


await builder.Build().RunAsync();