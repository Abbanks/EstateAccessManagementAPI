using EstateAccessManagement.Common.Enums;
using System.ComponentModel.DataAnnotations;

namespace EstateAccessManagement.Core.Entities
{
    public class AccessCode
    {
        public Guid Id { get; set; }

        [Required]
        [MaxLength(8)]
        public string CodeHash { get; set; }

        [Required]
        public Guid ResidentId { get; set; }

        [Required]
        public AccessCodeType CodeType { get; set; }

        [Required]
        public DateTime ExpiresAt { get; set; }

        public DateTime CreatedAt { get; set; }
        public bool IsDeprecated { get; set; }

        public bool IsActive { get; set; }

        public int? MaxUses { get; set; }

        public int CurrentUses { get; set; }

        public AppUser Resident { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }
    }
}
