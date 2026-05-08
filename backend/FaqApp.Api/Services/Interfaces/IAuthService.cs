using FaqApp.Api.Dtos.Auth;

namespace FaqApp.Api.Services.Interfaces;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request);
}