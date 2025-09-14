using Microsoft.AspNetCore.Mvc;
using PaymentSystem.Application.Abstractions;
using PaymentSystem.Application.Models.Authentication;

namespace PaymentSystem.Web.Controllers
{
    [Route("accounts")]
    public class AccountsController(IAuthService authService) : ApiBaseController
    {
        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginDto userLoginDto)
        {
            var result = await authService.Login(userLoginDto);

            return Ok(result);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegisterDto userRegisterDto)
        {
            var result = await authService.Register(userRegisterDto);

            return Ok(result);
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken(RefreshTokenModel model)
        {
            var loginResult = await authService.RefreshToken(model);

            if (loginResult.IsLoggedIn)
            {
                return Ok(loginResult);
            }

            return Unauthorized();
        }
    }
}