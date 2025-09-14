using System.IdentityModel.Tokens.Jwt;
using System.Security.Authentication;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
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
                    Phone = user.PhoneNumber,
                    RefreshToken = GenerateRefreshTokenString(),
                    IsLoggedIn = true
                };
                
                user.RefreshToken = response.RefreshToken;
                user.RefreshTokenExpiry = DateTime.UtcNow.AddMinutes(
                    _authOptions.RefreshTokenJwtTokenExpiryIntervalMinutes);
                await userManager.UpdateAsync(user);

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

        #region access jwt token
        private UserResponse GenerateToken(UserResponse user)
        {
            var handler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_authOptions.JwtTokenPrivateKey);
            var credentials = new SigningCredentials(new SymmetricSecurityKey(key),
                                                     SecurityAlgorithms.HmacSha256Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = GenerateClaims(user),
                Expires = DateTime.UtcNow.AddMinutes(_authOptions.JwtTokenExpiryIntervalMinutes),
                SigningCredentials = credentials,
                Audience = _authOptions?.Audience,
                Issuer = _authOptions?.Issuer,
            };

            var token = handler.CreateToken(tokenDescriptor);
            user.JwtToken = handler.WriteToken(token);

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
        #endregion

        #region refresh token
        private string GenerateRefreshTokenString()
        {
            var randomNumber = new byte[64];
            using (var numberGenerator = RandomNumberGenerator.Create())
            {
                numberGenerator.GetBytes(randomNumber);
            }

            return Convert.ToBase64String(randomNumber);
        }

        public async Task<UserResponse> RefreshToken(RefreshTokenModel model)
        {
            var principal = GetTokenPrincipal(model.JwtToken, false);

            if (principal?.Identity?.Name is null)
            {
                throw new AuthenticationException("Invalid JWT token.");
            }

            var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                throw new AuthenticationException("JWT does not contain user identifier.");
            }

            var user = await userManager.FindByIdAsync(userId);

            if (user is null)
            {
                throw new EntityNotFoundException($"User with id={userId} not found.");
            }

            if (user.RefreshToken != model.RefreshToken || user.RefreshTokenExpiry <= DateTime.UtcNow)
            {
                throw new AuthenticationException("Invalid refresh token.");
            }

            var userRoles = await userManager.GetRolesAsync(user);
            var userResponse = new UserResponse
            {
                Id = user.Id,
                Email = user.Email,
                Roles = userRoles.ToArray(),
                Username = user.UserName,
                Phone = user.PhoneNumber,
                IsLoggedIn = true
            };

            userResponse = GenerateToken(userResponse);
            var newRefreshToken = GenerateRefreshTokenString();

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddMinutes(
                _authOptions.RefreshTokenJwtTokenExpiryIntervalMinutes);

            await userManager.UpdateAsync(user);

            userResponse.RefreshToken = newRefreshToken;
            return userResponse;
        }

        private ClaimsPrincipal? GetTokenPrincipal(string token, bool validateLifetime = true)
        {
            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_authOptions.JwtTokenPrivateKey));
            var validation = new TokenValidationParameters()
            {
                ValidateIssuer = true,
                ValidIssuer = _authOptions.Issuer,
                ValidateAudience = true,
                ValidAudience = _authOptions.Audience,
                ValidateLifetime = validateLifetime,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key
            };

            return new JwtSecurityTokenHandler().ValidateToken(token, validation, out _);
        }
        #endregion
    }
}