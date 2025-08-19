using EstateAccessManagement.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace EstateAccessManagement.Core.Entities
{
    public class AccessCode
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(6)]
        public string Code { get; set; } = string.Empty;

        [Required]
        public Guid ResidentId { get; set; }

        [Required]
        public AccessCodeType CodeType { get; set; }

        [Required]
        public DateTime ExpiresAt { get; private set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsDeprecated { get; set; }

        public bool IsActive { get; set; } = true;

        public int? MaxUses { get; private set; }

        public int CurrentUses { get; set; } = 0;

        public AppUser Resident { get; set; } = null!;
    }
}
