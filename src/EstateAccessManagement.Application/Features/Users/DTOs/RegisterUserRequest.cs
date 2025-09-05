using EstateAccessManagement.Core.Enums;

namespace EstateAccessManagement.Application.DTOs
{
    public class RegisterUserRequest
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public UserType UserType { get; set; }
    }
}
