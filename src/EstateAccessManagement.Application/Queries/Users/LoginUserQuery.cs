using EstateAccessManagement.Application.DTOs;
using MediatR;

namespace EstateAccessManagement.Application.Queries.Users
{
    public class LoginUserQuery : IRequest<LoginUserResponse>
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
