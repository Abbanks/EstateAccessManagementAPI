using EstateAccessManagement.Application.DTOs;
using EstateAccessManagement.Common.Extensions;
using EstateAccessManagement.Core.Entities;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace EstateAccessManagement.Application.Features.Users.Commands
{
    public class RegisterUserHandler(UserManager<AppUser> userManager, ILogger<RegisterUserHandler> logger)
    : IRequestHandler<RegisterUserCommand, RegisterUserResponse>
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
                throw new ArgumentException("User registration failed: " + string.Join(", ", result.Errors.Select(e => e.Description)));
            }

            var roleResult = await userManager.AddToRoleAsync(user, request.UserType.ToString());

            if (!roleResult.Succeeded)
            {
                logger.LogError("Failed to add user {Email} to role {Role}: {Errors}", request.Email, request.UserType.ToString(), string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                throw new ApplicationException("Failed to assign role to user.");
            }

            logger.LogInformation("User registered successfully with ID {UserId}", user.Id);

            return new RegisterUserResponse
            {
                Message = "User registered successfully",
                UserId = user.Id,
                UserType = request.UserType.GetDescription()
            };
        }
    }
}