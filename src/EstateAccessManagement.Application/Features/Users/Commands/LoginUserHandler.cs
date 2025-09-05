using EstateAccessManagement.Application.DTOs;
using EstateAccessManagement.Application.Interfaces;
using EstateAccessManagement.Core.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace EstateAccessManagement.Application.Features.Users.Commands
{
    public class LoginUserHandler(UserManager<AppUser> userManager, ILogger<LoginUserHandler> logger, IAuthService authService)
    : IRequestHandler<LoginUserCommand, LoginUserResponse>
    {
        public async Task<LoginUserResponse> Handle(LoginUserCommand request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Login attempt for {Email}", request.Email);

            var user = await userManager.FindByEmailAsync(request.Email);
            if (user == null || !await userManager.CheckPasswordAsync(user, request.Password))
            {
                logger.LogWarning("Invalid login attempt for {Email}", request.Email);
                throw new ApplicationException("Invalid credentials.");
            }

            var token = await authService.GenerateJwtToken(user);

            logger.LogInformation("User {Email} logged in successfully", user.Email);

            return new LoginUserResponse
            {
                Token = token,
                UserType = user.UserType.ToString(),
                UserId = user.Id,
                FullName = $"{user.FirstName} {user.LastName}"
            };
        }
    }
}