using EstateAccessManagement.Application.Features.AccessCodes.DTOs;
using EstateAccessManagement.Application.Interfaces;
using EstateAccessManagement.Core.Entities;
using EstateAccessManagement.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace EstateAccessManagement.Infrastructure.Services
{
    public class AccessCodeService(ILogger<AccessCodeService> logger, ApplicationDbContext _db) : IAccessCodeService
    {
        public async Task<CreateAccessCodeResult> GenerateAccessCodeAsync(Guid residentId, AccessCodeType type)
        {
            var rawCode = GenerateShortCode(type);
            var codeHash = HashCode(rawCode);
            DateTime expiresAt;
            int? maxUses = null;

            switch (type)
            {
                case AccessCodeType.TemporaryVisitor:
                    expiresAt = DateTime.UtcNow.AddHours(24);
                    maxUses = 1; 
                    break;

                case AccessCodeType.LongStayVisitor:
                    expiresAt = DateTime.UtcNow.AddDays(7);
                    maxUses = null; 
                    break;

                default:
                    expiresAt = DateTime.UtcNow.AddDays(30); 
                    break;
            }

            var accessCode = new AccessCode
            {
                ResidentId = residentId,
                CodeType = type,
                CodeHash = codeHash,
                ExpiresAt = expiresAt,
                MaxUses = maxUses,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _db.AccessCodes.Add(accessCode);
            await _db.SaveChangesAsync();

            accessCode.CodeHash = rawCode;
            return new CreateAccessCodeResult
            {
                Id = accessCode.Id,
                ResidentId = accessCode.ResidentId,
                Code = rawCode,
                CodeType = accessCode.CodeType,
                ExpiresAt = accessCode.ExpiresAt,
                MaxUses = accessCode.MaxUses,
                CurrentUses = accessCode.CurrentUses,
                IsActive = accessCode.IsActive,
                CreatedAt = accessCode.CreatedAt
            };
        }

        public async Task<AccessCodeValidationResult> ValidateAccessCodeAsync(string code)
        {
            var codeHash = HashCode(code);

            var accessCode = await _db.AccessCodes
                .Where(ac => ac.IsActive)
                .FirstOrDefaultAsync(ac => ac.CodeHash == codeHash);

            if (accessCode == null)
            {
                return new AccessCodeValidationResult
                {
                    IsValid = false,
                    Message = "Access code not found or inactive."
                };
            }

            if (accessCode.ExpiresAt < DateTime.UtcNow)
            {
                accessCode.IsActive = false;
                await _db.SaveChangesAsync();

                return new AccessCodeValidationResult
                {
                    IsValid = false,
                    Message = "Access code has expired.",
                    ResidentId = accessCode.ResidentId,
                    AccessCodeId = accessCode.Id
                };
            }

            if (accessCode.MaxUses.HasValue && accessCode.CurrentUses >= accessCode.MaxUses.Value)
            {
                accessCode.IsActive = false;
                await _db.SaveChangesAsync();

                return new AccessCodeValidationResult
                {
                    IsValid = false,
                    Message = "Access code has reached its maximum number of uses.",
                    ResidentId = accessCode.ResidentId,
                    AccessCodeId = accessCode.Id
                };
            }

            accessCode.CurrentUses++;

            if (accessCode.MaxUses.HasValue && accessCode.CurrentUses >= accessCode.MaxUses.Value)
            {
                accessCode.IsActive = false;
            }

            await _db.SaveChangesAsync();

            return new AccessCodeValidationResult
            {
                IsValid = true,
                Message = "Access code is valid.",
                ResidentId = accessCode.ResidentId,
                AccessCodeId = accessCode.Id
            };
        }

        private static string GenerateShortCode(AccessCodeType type)
        {
            var prefix = type switch
            {
                AccessCodeType.TemporaryVisitor => "TV",
                AccessCodeType.LongStayVisitor => "LV",
                _ => "AC"
            };

            var randomDigits = RandomNumberGenerator.GetInt32(1000, 9999);
            return $"{prefix}{randomDigits}";
        }

        public static string HashCode(string code)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(code);
            var hashBytes = sha256.ComputeHash(bytes);
            return Convert.ToHexString(hashBytes).Substring(0, 8);
        }
    }
}
