using EstateAccessManagement.Core.Enums;

namespace EstateAccessManagement.Application.Features.AccessCodes.DTOs
{
    public class GetAccessCodeResult
    {
        public Guid Id { get; set; }
        public Guid ResidentId { get; set; }
        public string Code { get; set; }
        public AccessCodeType CodeType { get; set; }
        public string? ExpiresAt { get; set; }
        public int? MaxUses { get; set; }
        public int CurrentUses { get; set; }
        public bool IsActive { get; set; }
        public string? CreatedAt { get; set; }
    }
}
