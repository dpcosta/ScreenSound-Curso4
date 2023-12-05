using System.Net.Http.Json;
using ScreenSound.Web.Response;

namespace ScreenSound.Web.Services;

public class AuthenticationService(IHttpClientFactory factory) : IAuthenticationService
{
    private readonly HttpClient httpClient = factory.CreateClient("API");
    public async Task<AuthResponse> LoginAsync(string email, string senha)
    {
        var response = await httpClient.PostAsJsonAsync("auth/login?useCookies=true", new
        {
            email = email,
            password = senha
        });

        if (response.IsSuccessStatusCode)
        {
            return new AuthResponse { Sucesso = true };
        }

        return new AuthResponse { Erros = ["Login/senha inválidos!"] };
    }
}