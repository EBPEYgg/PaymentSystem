using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PaymentSystem.Application.Abstractions;
using PaymentSystem.Application.Models.Authentication;
using PaymentSystem.Domain.Entities;
using PaymentSystem.Domain.Models;
using PaymentSystem.Domain.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Authentication;
using System.Security.Claims;
using System.Text;

namespace PaymentSystem.Application.Services
{
    public class AuthService(IOptions<AuthOptions> authOptions, 
        UserManager<IdentityUserEntity> userManager) : IAuthService
    {
        private readonly AuthOptions _authOptions = authOptions.Value;

        public async Task<UserResponse> Login(UserLoginDto userLoginDto)
        {
            var user = await userManager.FindByEmailAsync(userLoginDto.Email);
            var checkPasswordResult = await userManager.CheckPasswordAsync(user, userLoginDto.Password);

            if (checkPasswordResult)
            {
                var userRoles = await userManager.GetRolesAsync(user);
                var response = new UserResponse
                {
                    Id = user.Id,
                    Email = user.Email,
                    Roles = userRoles.ToArray(),
                    Username = user.UserName,
                    Phone = user.PhoneNumber
                };
                return GenerateToken(response);
            }

            throw new AuthenticationException();
        }

        public async Task<UserResponse> Register(UserRegisterDto userRegisterDto)
        {
            var createUserResult = await userManager.CreateAsync(new IdentityUserEntity
            {
                Email = userRegisterDto.Email,
                PhoneNumber = userRegisterDto.Phone,
                UserName = userRegisterDto.Username
            }, userRegisterDto.Password);

            if (createUserResult.Succeeded)
            {
                var user = await userManager.FindByEmailAsync(userRegisterDto.Email);
                var result = await userManager.AddToRoleAsync(user, RoleConstants.User);

                if (result.Succeeded)
                {
                    var response = new UserResponse
                    {
                        Id = user.Id,
                        Email = user.Email,
                        Roles = [RoleConstants.User],
                        Username = user.UserName,
                        Phone = user.PhoneNumber
                    };
                    return GenerateToken(response);
                }

                throw new Exception($"Errors: {string.Join(";", result.Errors
                    .Select(x => $"{x.Code} {x.Description}"))}");
            }

            throw new Exception($"Errors: {string.Join(";", createUserResult.Errors
                .Select(x => $"{x.Code} {x.Description}"))}");
        }

        public UserResponse GenerateToken(UserResponse user)
        {
            var handler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_authOptions.TokenPrivateKey);
            var credentials = new SigningCredentials(new SymmetricSecurityKey(key),
                                                     SecurityAlgorithms.HmacSha256Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = GenerateClaims(user),
                Expires = DateTime.UtcNow.AddMinutes(_authOptions.ExpireIntervalMinutes),
                SigningCredentials = credentials,
                Audience = _authOptions?.Audience,
                Issuer = _authOptions?.Issuer,
            };

            var token = handler.CreateToken(tokenDescriptor);
            user.Token = handler.WriteToken(token);

            return user;
        }

        private static ClaimsIdentity GenerateClaims(UserResponse user)
        {
            var claims = new ClaimsIdentity();
            claims.AddClaim(new Claim(ClaimTypes.Name, user.Email!));
            claims.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));

            foreach (var role in user.Roles!)
            {
                claims.AddClaim(new Claim(ClaimTypes.Role, role));
            }

            return claims;
        }
    }
}