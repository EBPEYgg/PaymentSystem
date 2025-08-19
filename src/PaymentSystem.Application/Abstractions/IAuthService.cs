using PaymentSystem.Application.Models.Authentication;

namespace PaymentSystem.Application.Abstractions
{
    public interface IAuthService
    {
        Task<UserResponse> Register(UserRegisterDto userRegisterDto);

        Task<UserResponse> Login(UserLoginDto userLoginDto);
    }
}