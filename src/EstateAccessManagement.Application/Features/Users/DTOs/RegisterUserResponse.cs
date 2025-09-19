namespace EstateAccessManagement.Application.DTOs
{
    public class RegisterUserResponse
    {
        public string Message { get; set; } = string.Empty;
        public Guid UserId { get; set; }
        public string UserType { get; set; }
    }
}
