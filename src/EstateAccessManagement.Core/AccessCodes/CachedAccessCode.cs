using EstateAccessManagement.Core.Enums;

namespace EstateAccessManagement.Core.AccessCodes
{
    public class CachedAccessCode
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public Guid ResidentId { get; set; }
        public string CodeHash { get; set; }
        public AccessCodeType CodeType { get; set; }
        public DateTime ExpiresAt { get; set; }
        public int? MaxUses { get; set; }
        public int CurrentUses { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeprecated { get; set; }
        public byte[] RowVersion { get; set; }
    }
}
