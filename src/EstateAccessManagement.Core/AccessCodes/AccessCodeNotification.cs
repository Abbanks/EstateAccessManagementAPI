namespace EstateAccessManagement.Core.AccessCodes
{
    public class AccessCodeNotification
    {
        public Guid ResidentId { get; set; }
        public string? ResidentEmail { get; set; }
        public string? AccessCode { get; set; }
        public string? CodeType { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
