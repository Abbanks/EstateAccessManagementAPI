using EstateAccessManagement.Application.Features.AccessCodes.DTOs;
using EstateAccessManagement.Application.Interfaces;
using EstateAccessManagement.Core.Entities;
using EstateAccessManagement.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace EstateAccessManagement.Infrastructure.Services
{
    public class AccessCodeService(
        ILogger<AccessCodeService> logger, 
        ApplicationDbContext db,
        IDistributedCache cache) : IAccessCodeService
    {
        private const string AccessCodeCacheKeyPrefix = "access_code:";
        public async Task<GenerateAccessCodeResult> GenerateAccessCodeAsync(Guid residentId, AccessCodeType type)
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

            db.AccessCodes.Add(accessCode);
            await db.SaveChangesAsync();

            var cacheKey = $"{AccessCodeCacheKeyPrefix}{codeHash}";
            var cachedData = JsonSerializer.Serialize(new CachedAccessCode
            {
                Id = accessCode.Id,
                ResidentId = accessCode.ResidentId,
                CodeHash = accessCode.CodeHash,
                CodeType = accessCode.CodeType,
                ExpiresAt = accessCode.ExpiresAt,
                MaxUses = accessCode.MaxUses,
                CurrentUses = accessCode.CurrentUses,
                IsActive = true
            });

            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpiration = expiresAt.AddHours(1)
            };

            await cache.SetStringAsync(cacheKey, cachedData, cacheOptions);

            return new GenerateAccessCodeResult
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
            var cacheKey = $"{AccessCodeCacheKeyPrefix}{codeHash}";

            var cachedData = await cache.GetStringAsync(cacheKey);
            CachedAccessCode cachedCode = null;

            if (!string.IsNullOrEmpty(cachedData))
            {
                cachedCode = JsonSerializer.Deserialize<CachedAccessCode>(cachedData);
                if (cachedCode != null)
                {
                    if (!cachedCode.IsActive || cachedCode.ExpiresAt < DateTime.UtcNow)
                    {
                        await cache.RemoveAsync(cacheKey);
                        var dbEntry = await db.AccessCodes.FirstOrDefaultAsync(ac => ac.Id == cachedCode.Id);
                        if (dbEntry != null && dbEntry.IsActive)
                        {
                            dbEntry.IsActive = false;
                            await db.SaveChangesAsync();
                        }
                        return new AccessCodeValidationResult
                        {
                            IsValid = false,
                            Message = "Access code has expired or is inactive."
                        };
                    }

                    if (cachedCode.MaxUses.HasValue && cachedCode.CurrentUses >= cachedCode.MaxUses.Value)
                    {
                        await cache.RemoveAsync(cacheKey);
                        var dbEntry = await db.AccessCodes.FirstOrDefaultAsync(ac => ac.Id == cachedCode.Id);
                        if (dbEntry != null && dbEntry.IsActive)
                        {
                            dbEntry.IsActive = false;
                            await db.SaveChangesAsync();
                        }
                        return new AccessCodeValidationResult
                        {
                            IsValid = false,
                            Message = "Access code has reached its maximum number of uses."
                        };
                    }

                    cachedCode.CurrentUses++;
                    if (cachedCode.MaxUses.HasValue && cachedCode.CurrentUses >= cachedCode.MaxUses.Value)
                    {
                        cachedCode.IsActive = false;
                    }

                    var updatedCacheValue = JsonSerializer.Serialize(cachedCode);
                    await cache.SetStringAsync(cacheKey, updatedCacheValue, new DistributedCacheEntryOptions
                    {
                        AbsoluteExpiration = cachedCode.ExpiresAt.AddHours(1)
                    });

                    var dbAccessCode = await db.AccessCodes.FirstOrDefaultAsync(ac => ac.Id == cachedCode.Id);
                    if (dbAccessCode != null)
                    {
                        dbAccessCode.CurrentUses = cachedCode.CurrentUses;
                        dbAccessCode.IsActive = cachedCode.IsActive;
                        await db.SaveChangesAsync();
                    }

                    return new AccessCodeValidationResult
                    {
                        IsValid = true,
                        Message = "Access code is valid.",
                        ResidentId = cachedCode.ResidentId,
                        AccessCodeId = cachedCode.Id
                    };
                }
            }

            // Cache miss fallback
            var accessCode = await db.AccessCodes
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
                await db.SaveChangesAsync();
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
                await db.SaveChangesAsync();
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

            await db.SaveChangesAsync();

            var newCacheValue = JsonSerializer.Serialize(new CachedAccessCode
            {
                Id = accessCode.Id,
                ResidentId = accessCode.ResidentId,
                CodeHash = accessCode.CodeHash,
                CodeType = accessCode.CodeType,
                ExpiresAt = accessCode.ExpiresAt,
                MaxUses = accessCode.MaxUses,
                CurrentUses = accessCode.CurrentUses,
                IsActive = accessCode.IsActive
            });

            await cache.SetStringAsync(cacheKey, newCacheValue, new DistributedCacheEntryOptions
            {
                AbsoluteExpiration = accessCode.ExpiresAt.AddHours(1)
            });

            return new AccessCodeValidationResult
            {
                IsValid = true,
                Message = "Access code is valid.",
                ResidentId = accessCode.ResidentId,
                AccessCodeId = accessCode.Id
            };
        }

        public async Task<GetAccessCodeResult> GetAccessCodeByIdAsync(Guid id)
        {
            var accessCode = await db.AccessCodes.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);

            if (accessCode == null)
            {
                return null;
            }

            return new GetAccessCodeResult
            {
                Id = accessCode.Id,
                ResidentId = accessCode.ResidentId,
                CodeType = accessCode.CodeType,
                ExpiresAt = accessCode.ExpiresAt,
                MaxUses = accessCode.MaxUses,
                CurrentUses = accessCode.CurrentUses,
                IsActive = accessCode.IsActive,
                CreatedAt = accessCode.CreatedAt
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

        private class CachedAccessCode
        {
            public Guid Id { get; set; }
            public Guid ResidentId { get; set; }
            public string CodeHash { get; set; }
            public AccessCodeType CodeType { get; set; }
            public DateTime ExpiresAt { get; set; }
            public int? MaxUses { get; set; }
            public int CurrentUses { get; set; }
            public bool IsActive { get; set; }
        }
    }
}
