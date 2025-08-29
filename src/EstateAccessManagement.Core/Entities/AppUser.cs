using EstateAccessManagement.Core.Enums;
using Microsoft.AspNetCore.Identity;

namespace EstateAccessManagement.Core.Entities;

public class AppUser : IdentityUser<Guid>
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public UserType UserType { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
    public bool IsDeprecated { get; set; }
    public DateTime? DeletedOn { get; set; }
}
