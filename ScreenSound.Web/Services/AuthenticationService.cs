using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using ScreenSound.Web.Response;

namespace ScreenSound.Web.Services;

public class AuthenticationService(IHttpClientFactory factory) : AuthenticationStateProvider, IAuthenticationService
{
    private readonly HttpClient httpClient = factory.CreateClient("API");
    private bool autenticado = false;

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        autenticado = false;
        ClaimsPrincipal usuario = new(new ClaimsIdentity());
        var resposta = await httpClient.GetAsync("auth/manage/info");

        if (resposta.IsSuccessStatusCode)
        {
            var info = await resposta.Content.ReadFromJsonAsync<InfoPessoaUsuaria>();
            if (info is not null)
            {
                List<Claim> direitos = 
                [
                    new(ClaimTypes.Name, info.Email!),
                    new(ClaimTypes.Email, info.Email!)
                ];
                var id = new ClaimsIdentity(direitos, nameof(AuthenticationService));
                usuario = new ClaimsPrincipal(id);
                autenticado = true;
            }
        }

        return new AuthenticationState(usuario);
    }

    public async Task<AuthResponse> LoginAsync(string email, string senha)
    {
        var response = await httpClient.PostAsJsonAsync("auth/login?useCookies=true", new
        {
            email,
            password = senha
        });

        if (response.IsSuccessStatusCode)
        {
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
            return new AuthResponse { Sucesso = true };
        }

        return new AuthResponse { Erros = ["Login/senha inválidos!"] };
    }
}