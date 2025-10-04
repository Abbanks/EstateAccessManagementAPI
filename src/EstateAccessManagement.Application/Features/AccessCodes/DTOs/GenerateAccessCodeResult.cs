using EstateAccessManagement.Core.Enums;

namespace EstateAccessManagement.Application.Features.AccessCodes.DTOs
{
    public class GenerateAccessCodeResult
    {
        public Guid Id { get; set; }
        public Guid ResidentId { get; set; }
        public string Code { get; set; }
        public AccessCodeType CodeType { get; set; }
        public int? MaxUses { get; set; }
        public int CurrentUses { get; set; }
        public bool IsActive { get; set; }
        public string? ValidFrom { get; set; }
        public string? ExpiresAt { get; set; }
    }
}
