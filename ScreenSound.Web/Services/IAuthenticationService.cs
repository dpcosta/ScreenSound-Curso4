
using ScreenSound.Web.Response;

namespace ScreenSound.Web.Services;

public interface IAuthenticationService
{
    Task<AuthResponse> LoginAsync(string email, string senha);
}