using EstateAccessManagement.Application.Commands.Users;
using EstateAccessManagement.Application.DTOs;
using EstateAccessManagement.Application.Queries.Users;
using EstateAccessManagement.Core.Entities;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EstateAccessManagement.Application.Handlers
{
    public class AuthHandler(UserManager<AppUser> userManager, IConfiguration config, ILogger<AuthHandler> logger)
        : IRequestHandler<RegisterUserCommand, RegisterUserResponse>,
          IRequestHandler<LoginUserQuery, LoginUserResponse>
    {
        public async Task<RegisterUserResponse> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            logger.LogInformation("User registration started for {Email}", request.Email);

            var existingUser = await userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                logger.LogWarning("User registration failed. Email {Email} is already in use.", request.Email);
                throw new ValidationException("Email is already in use.");
            }

            var user = new AppUser
            {
                Email = request.Email,
                UserName = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                UserType = request.UserType,
                CreatedAt = DateTime.UtcNow,
                ModifiedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                logger.LogError("User registration failed for {Email}: {Errors}", request.Email, string.Join(", ", result.Errors.Select(e => e.Description)));
                throw new ApplicationException("User registration failed: " + string.Join(", ", result.Errors.Select(e => e.Description)));
            }

            logger.LogInformation("User registered successfully with ID {UserId}", user.Id);

            return new RegisterUserResponse
            {
                Message = "User registered successfully",
                UserId = user.Id
            };
        }
        public async Task<LoginUserResponse> Handle(LoginUserQuery request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Login attempt for {Email}", request.Email);

            var user = await userManager.FindByEmailAsync(request.Email);
            if (user == null || !await userManager.CheckPasswordAsync(user, request.Password))
            {
                logger.LogWarning("Invalid login attempt for {Email}", request.Email);
                throw new ApplicationException("Invalid credentials.");
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.UserType.ToString())
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(config["Jwt:Key"]);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(7),
                Issuer = config["Jwt:Issuer"],
                Audience = config["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);

            logger.LogInformation("User {Email} logged in successfully", user.Email);

            return new LoginUserResponse
            {
                Token = tokenHandler.WriteToken(token),
                UserType = user.UserType.ToString(),
                UserId = user.Id,
                FullName = $"{user.FirstName} {user.LastName}"
            };
        }
    }
}
