using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NLog;
using PaymentSystem.Application.Abstractions;
using PaymentSystem.Application.Models.Authentication;
using PaymentSystem.Domain.Entities;
using PaymentSystem.Domain.Exceptions;
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

        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public async Task<UserResponse> Login(UserLoginDto userLoginDto)
        {
            _logger.Debug("Login attempt for Email={Email}.", userLoginDto.Email);
            var user = await userManager.FindByEmailAsync(userLoginDto.Email);

            if (user == null)
            {
                throw new EntityNotFoundException($"User with email={userLoginDto.Email} not found.");
            }

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
                _logger.Info("User logged in successfully. Email={Email}, UserId={UserId}.", user.Email, user.Id);
                return GenerateToken(response);
            }

            _logger.Error("Login attempt failed for Email={Email}.", userLoginDto.Email);
            throw new AuthenticationException("Incorrect password.");
        }

        public async Task<UserResponse> Register(UserRegisterDto userRegisterDto)
        {
            _logger.Debug("Register attempt for Email={Email}.", userRegisterDto.Email);

            if (await userManager.FindByEmailAsync(userRegisterDto.Email) != null)
            {
                throw new DuplicateEntityException($"User with Email={userRegisterDto.Email} already exists.");
            }

            var createUserResult = await userManager.CreateAsync(new IdentityUserEntity
            {
                Email = userRegisterDto.Email,
                PhoneNumber = userRegisterDto.Phone,
                UserName = userRegisterDto.Username
            }, userRegisterDto.Password);

            if (createUserResult.Succeeded)
            {
                var user = await userManager.FindByEmailAsync(userRegisterDto.Email);

                if (user == null)
                {
                    throw new EntityNotFoundException($"User with email={userRegisterDto.Email} not registered.");
                }

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
                    _logger.Info("User registered successfully. Email={Email}, UserId={UserId}.", user.Email, user.Id);
                    return GenerateToken(response);
                }

                _logger.Error("Unsuccessful attempt to add a role to a user with an email={Email}.", userRegisterDto.Email);
                throw new Exception($"Errors: {string.Join(";", result.Errors
                    .Select(x => $"{x.Code} {x.Description}"))}");
            }

            _logger.Error("Registration attempt failed for Email={Email}.", userRegisterDto.Email);
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