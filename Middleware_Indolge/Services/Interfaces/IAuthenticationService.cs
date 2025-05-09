using Middleware_Indolge.Models;

namespace Middleware_Indolge.Services.Interfaces
{
    public interface IAuthenticationService
    {
        Task<LoginResponse> Login(LoginModel request);
    }
}
