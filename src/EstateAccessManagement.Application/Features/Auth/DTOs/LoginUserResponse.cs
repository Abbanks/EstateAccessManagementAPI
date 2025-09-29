namespace EstateAccessManagement.Application.DTOs
{
    public class LoginUserResponse
    {
        public string Token { get; set; } = string.Empty;
        public string UserType { get; set; } = string.Empty;
        public Guid UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
    }
}
