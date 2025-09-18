using EstateAccessManagement.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace EstateAccessManagement.Infrastructure;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
: IdentityDbContext<AppUser, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<AccessCode> AccessCodes { get; set; }
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<AppUser>(entity =>
        {
            entity.Property(u => u.FirstName).HasMaxLength(30).IsRequired();
            entity.Property(u => u.LastName).HasMaxLength(30).IsRequired();
            entity.Property(u => u.UserType).IsRequired();
            entity.Property(u => u.CreatedAt).IsRequired();
            entity.Property(u => u.ModifiedAt).IsRequired();
            entity.Property(u => u.IsDeprecated).IsRequired();
        });

        builder.Entity<AccessCode>(entity =>
        {
            entity.HasKey(ac => ac.Id);

            entity.Property(ac => ac.CodeHash)
                .IsRequired()
                .HasMaxLength(64);

            entity.Property(ac => ac.ResidentId)
                .IsRequired();

            entity.Property(ac => ac.CodeType)
                .IsRequired();

            entity.Property(ac => ac.ExpiresAt)
                .IsRequired();

            entity.Property(ac => ac.CreatedAt)
                .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");

            entity.Property(ac => ac.IsActive)
                .HasDefaultValue(true);

            entity.Property(ac => ac.IsDeprecated)
                .HasDefaultValue(false);

            entity.Property(ac => ac.CurrentUses)
                .HasDefaultValue(0);

            entity.HasIndex(ac => ac.CodeHash);
            entity.HasIndex(ac => ac.ResidentId);
            entity.HasIndex(ac => ac.IsActive);
            entity.HasIndex(ac => ac.ExpiresAt);

            entity.HasOne(ac => ac.Resident)
                .WithMany() 
                .HasForeignKey(ac => ac.ResidentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(ac => ac.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();
        });
    }
}