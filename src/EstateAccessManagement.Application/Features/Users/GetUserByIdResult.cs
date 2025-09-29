using EstateAccessManagement.Core.Enums;

namespace EstateAccessManagement.Application.Features.Users
{
    public class GetUserByIdResult
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public UserType UserType { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsDeprecated { get; set; }
    }
}
