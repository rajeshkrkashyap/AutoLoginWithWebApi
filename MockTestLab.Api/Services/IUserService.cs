using MockTestLab.Api.Models;
using MockTestLab.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Cryptography;
using MockTestLab.Shared.Entities;

namespace MockTestLab.Api.Services
{
    public interface IUserService
    {
        Task<MainResponse> RegisterUserAsync(RegisterViewModel model);
        Task<MainResponse> LoginUserAsync(LoginViewModel model);
        Task<MainResponse> ConfirmEmailAsync(string userId, string token);
        Task<MainResponse> ForgetPasswordAsync(string email);
        Task<MainResponse> ResetPasswordAsync(ResetPasswordViewModel model);
        Task<MainResponse> RefreshTokenAsync(AuthenticationResponse model);
    }

    public class UserService : IUserService
    {

        private UserManager<AppUser> _userManger;
        private IConfiguration _configuration;
        private IMailService _mailService;
        public UserService(UserManager<AppUser> userManager, IConfiguration configuration, IMailService mailService)
        {
            _userManger = userManager;
            _configuration = configuration;
            _mailService = mailService;
        }

        public async Task<MainResponse> RegisterUserAsync(RegisterViewModel model)
        {
            if (model == null)
                throw new NullReferenceException("Reigster Model is null");

            if (model.Password != model.ConfirmPassword)
                return new MainResponse
                {
                    IsSuccess = false,
                    ErrorMessage = "Confirm password doesn't match the password"
                };

            var AppUser = new AppUser
            {
                Name = model.Name,
                Email = model.Email,
                UserName = model.Email,
            };
            try
            {
                var result = await _userManger.CreateAsync(AppUser, model.Password);

                if (result.Succeeded)
                {
                    var confirmEmailToken = await _userManger.GenerateEmailConfirmationTokenAsync(AppUser);

                    var encodedEmailToken = Encoding.UTF8.GetBytes(confirmEmailToken);
                    var validEmailToken = WebEncoders.Base64UrlEncode(encodedEmailToken);

                    string url = $"{_configuration["AppUrl"]}/confirmemail?userid={AppUser.Id}&token={validEmailToken}";

                    await _mailService.SendEmailAsync(AppUser.Email, "Confirm your email", $"<h1>Welcome to CoonetTo.Ai </h1>" +
                        $"<p>Please confirm your email by <a href='{url}'>Clicking here</a></p>");

                    return new MainResponse
                    {
                        IsSuccess = true,
                        Content = "User created successfully!"
                    };
                }
                return new MainResponse
                {
                    IsSuccess = false,
                    ErrorMessage = "User did not create",
                };
            }
            catch (Exception ex)
            {
                return new MainResponse
                {
                    IsSuccess = false,
                    ErrorMessage = ex.Message,
                };
            }

        }

        public async Task<MainResponse> LoginUserAsync(LoginViewModel model)
        {
            var user = await _userManger.FindByEmailAsync(model.Email);

            if (user == null)
            {
                return new MainResponse
                {
                    IsSuccess = false,
                    ErrorMessage = "There is no user with that Email address"
                };
            }

            var isemailConfirmed = await _userManger.IsEmailConfirmedAsync(user);

            if (!isemailConfirmed)
            {
                return new MainResponse
                {
                    IsSuccess = false,
                    ErrorMessage = "Email not Confirmed!",
                };
            }
            var result = await _userManger.CheckPasswordAsync(user, model.Password);

            if (!result)
                return new MainResponse
                {
                    IsSuccess = false,
                    ErrorMessage = "Invalid password",
                };

            var claims = new[]
            {
                new Claim("Email", model.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
            };

            string accessToken = GenerateAccessToken(user);
            var refreshToken = GenerateRefreshToken();
            user.RefreshToken = refreshToken;
            await _userManger.UpdateAsync(user);

            var response = new MainResponse
            {
                Content = new AuthenticationResponse
                {
                    RefreshToken = refreshToken,
                    AccessToken = accessToken
                },
                IsSuccess = true,
                ErrorMessage = ""
            };

            return response;

        }

        public async Task<MainResponse> ConfirmEmailAsync(string userId, string token)
        {
            var user = await _userManger.FindByIdAsync(userId);
            if (user == null)
                return new MainResponse
                {
                    IsSuccess = false,
                    ErrorMessage = "User not found"
                };

            var decodedToken = WebEncoders.Base64UrlDecode(token);
            string normalToken = Encoding.UTF8.GetString(decodedToken);

            var result = await _userManger.ConfirmEmailAsync(user, normalToken);

            if (result.Succeeded)
                return new MainResponse
                {
                    IsSuccess = true,
                    Content = "Email confirmed successfully!",
                };

            return new MainResponse
            {
                IsSuccess = false,
                ErrorMessage = "Email did not confirm",
                Content = result.Errors.Select(e => e.Description),
            };
        }

        public async Task<MainResponse> ForgetPasswordAsync(string email)
        {
            var user = await _userManger.FindByEmailAsync(email);
            if (user == null)
                return new MainResponse
                {
                    IsSuccess = false,
                    ErrorMessage = "No user associated with email",
                };

            var token = await _userManger.GeneratePasswordResetTokenAsync(user);
            var encodedToken = Encoding.UTF8.GetBytes(token);
            var validToken = WebEncoders.Base64UrlEncode(encodedToken);

            string url = $"{_configuration["AppUrl"]}/ResetPassword?email={email}&token={validToken}";

            await _mailService.SendEmailAsync(email, "Reset Password", "<h1>Follow the instructions to reset your password</h1>" +
                $"<p>To reset your password <a href='{url}'>Click here</a></p>");

            return new MainResponse
            {
                IsSuccess = true,
                Content = "Reset password URL has been sent to the email successfully!"
            };
        }

        public async Task<MainResponse> ResetPasswordAsync(ResetPasswordViewModel model)
        {
            var user = await _userManger.FindByEmailAsync(model.Email);
            if (user == null)
                return new MainResponse
                {
                    IsSuccess = false,
                    ErrorMessage = "No user associated with email",
                };


            if (model.NewPassword != model.ConfirmPassword)
                return new MainResponse
                {
                    IsSuccess = false,
                    ErrorMessage = "Password doesn't match its confirmation",
                };

            var decodedToken = WebEncoders.Base64UrlDecode(model.Token);
            string normalToken = Encoding.UTF8.GetString(decodedToken);

            var result = await _userManger.ResetPasswordAsync(user, normalToken, model.NewPassword);

            if (result.Succeeded)
                return new MainResponse
                {
                    IsSuccess = true,
                    Content = "Password has been reset successfully!",
                };

            return new MainResponse
            {
                IsSuccess = false,
                Content = result.Errors.Select(e => e.Description)
            };
        }

        public async Task<MainResponse> RefreshTokenAsync(AuthenticationResponse refreshTokenRequest)
        {
            var principal = GetPrincipalFromExpiredToken(refreshTokenRequest.AccessToken);

            if (principal != null)
            {
                var email = principal.Claims.FirstOrDefault(f => f.Type == ClaimTypes.Email);

                var user = await _userManger.FindByEmailAsync(email?.Value);

                if (user is null || user.RefreshToken != refreshTokenRequest.RefreshToken)
                {
                    return new MainResponse
                    {
                        IsSuccess = false,
                        ErrorMessage = "Invalid Request",
                    };
                }

                string newAccessToken = GenerateAccessToken(user);
                string refreshToken = GenerateRefreshToken();

                user.RefreshToken = refreshToken;
                await _userManger.UpdateAsync(user);

                return new MainResponse
                {
                    IsSuccess = true,
                    Content = new AuthenticationResponse
                    {
                        RefreshToken = refreshToken,
                        AccessToken = newAccessToken
                    }
                };
            }
            else
            {
                return new MainResponse
                {
                    IsSuccess = true,
                    ErrorMessage = "Invalid Token Found"
                };
            }
        }
        private string GenerateAccessToken(AppUser user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            var keyDetail = Encoding.UTF8.GetBytes(_configuration["JWT:Key"]);

            var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(ClaimTypes.Name, $"{user.Name}"),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim("UserAvatar", $"{user.UserAvatar}"),

            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Audience = _configuration["JWT:Audience"],
                Issuer = _configuration["JWT:Issuer"],
                Expires = DateTime.UtcNow.AddMinutes(60),
                Subject = new ClaimsIdentity(claims),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(keyDetail), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            var keyDetail = Encoding.UTF8.GetBytes(_configuration["JWT:Key"]);
            var tokenValidationParameter = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = false,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _configuration["JWT:Issuer"],
                ValidAudience = _configuration["JWT:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(keyDetail),
            };

            SecurityToken securityToken;
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameter, out securityToken);
            var jwtSecurityToken = securityToken as JwtSecurityToken;
            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");
            return principal;
        }
        private string GenerateRefreshToken()
        {

            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }
    }

}
