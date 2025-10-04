using PaymentSystem.Application.Models.Authentication;
using System.Security.Claims;

namespace PaymentSystem.Application.Abstractions
{
    public interface IAuthService
    {
        Task<UserResponse> Login(UserLoginDto userLoginDto);

        Task<UserResponse> Register(UserRegisterDto userRegisterDto);

        Task<UserResponse> RefreshToken(RefreshTokenModel model);

        Task<UserResponse?> RevokeToken(ClaimsPrincipal user, string? userId);
    }
}