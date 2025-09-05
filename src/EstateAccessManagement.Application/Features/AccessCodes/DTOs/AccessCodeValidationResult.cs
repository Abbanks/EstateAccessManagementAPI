namespace EstateAccessManagement.Application.Features.AccessCodes.DTOs
{
    public class AccessCodeValidationResult
    {
        public bool IsValid { get; set; }
        public string Message { get; set; } = string.Empty;
        public Guid? ResidentId { get; set; }
        public Guid? AccessCodeId { get; set; }
    }
}
