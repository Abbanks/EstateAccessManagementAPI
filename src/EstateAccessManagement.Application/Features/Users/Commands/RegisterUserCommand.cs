using EstateAccessManagement.Application.DTOs;
using EstateAccessManagement.Common.Enums;
using MediatR;

namespace EstateAccessManagement.Application.Features.Users.Commands
{
    public class RegisterUserCommand : IRequest<RegisterUserResponse>
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public UserType UserType { get; set; }
    }
}
