using EstateAccessManagement.Application.DTOs;
using MediatR;

namespace EstateAccessManagement.Application.Commands.Users
{
    public class LoginUserCommand : IRequest<LoginUserResponse>
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
