using EstateAccessManagement.Application.Features.AccessCodes.DTOs;
using EstateAccessManagement.Application.Interfaces.Email;
using EstateAccessManagement.Application.Interfaces.Messaging;
using EstateAccessManagement.Application.Interfaces.Services;
using EstateAccessManagement.Core.AccessCodes;
using EstateAccessManagement.Core.Entities;
using EstateAccessManagement.Core.Enums;
using EstateAccessManagement.Core.Extensions;
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
        IDistributedCache cache,
        IUserService userService,
        IMessageQueueClient messageQueueClient,
        IEmailService emailSender
        ) : IAccessCodeService
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
                Code = rawCode,
                CodeType = type,
                CodeHash = codeHash,
                ExpiresAt = expiresAt,
                MaxUses = maxUses,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                RowVersion = new byte[] { 0 },
                IsDeprecated = false,
            };

            db.AccessCodes.Add(accessCode);
            await db.SaveChangesAsync();

            var cacheKey = $"{AccessCodeCacheKeyPrefix}{codeHash}";
            var cachedData = JsonSerializer.Serialize(new CachedAccessCode
            {
                Id = accessCode.Id,
                Code = rawCode,
                ResidentId = accessCode.ResidentId,
                CodeHash = accessCode.CodeHash,
                CodeType = accessCode.CodeType,
                ExpiresAt = accessCode.ExpiresAt,
                MaxUses = accessCode.MaxUses,
                CurrentUses = accessCode.CurrentUses,
                IsActive = true,
                RowVersion = new byte[] { 0 },
                IsDeprecated = accessCode.IsDeprecated,
            });

            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpiration = expiresAt.AddHours(1)
            };

            await cache.SetStringAsync(cacheKey, cachedData, cacheOptions);

            var residentInfo = await userService.GetUserById(residentId);
            var notificationEvent = new AccessCodeNotification
            {
                ResidentId = accessCode.ResidentId,
                ResidentEmail = residentInfo.Email,
                AccessCode = rawCode,
                CodeType = accessCode.CodeType.GetDescription(),
                ExpiresAt = accessCode.ExpiresAt,
            };
            string message = JsonSerializer.Serialize(notificationEvent);
            await messageQueueClient.PublishAsync("AccessCodeNotifications", message);

            return new GenerateAccessCodeResult
            {
                Id = accessCode.Id,
                ResidentId = accessCode.ResidentId,
                Code = rawCode,
                CodeType = accessCode.CodeType,
                ExpiresAt = accessCode.ExpiresAt.ToString("f"),
                MaxUses = accessCode.MaxUses,
                CurrentUses = accessCode.CurrentUses,
                IsActive = accessCode.IsActive,
                CreatedAt = accessCode.CreatedAt.ToString("f"),
            };
        }

        public async Task<AccessCodeValidationResult> ValidateAccessCodeAsync(string code)
        {
            var codeHash = HashCode(code);
            var cacheKey = $"{AccessCodeCacheKeyPrefix}{codeHash}";
            var cachedData = await cache.GetStringAsync(cacheKey);
            CachedAccessCode? cachedCode = !string.IsNullOrEmpty(cachedData) ? JsonSerializer.Deserialize<CachedAccessCode>(cachedData) : null;

            if (cachedCode != null)
            {
                if (cachedCode.IsDeprecated || !cachedCode.IsActive || cachedCode.ExpiresAt < DateTime.UtcNow)
                {
                    await InvalidateCodeAsync(cacheKey, cachedCode.Id);
                    return new AccessCodeValidationResult { IsValid = false, Message = "Access code is invalid." };
                }

                if (cachedCode.MaxUses.HasValue && cachedCode.CurrentUses >= cachedCode.MaxUses.Value)
                {
                    await InvalidateCodeAsync(cacheKey, cachedCode.Id);
                    return new AccessCodeValidationResult { IsValid = false, Message = "Access code has reached its maximum number of uses." };
                }

                cachedCode.CurrentUses++;
                if (cachedCode.MaxUses.HasValue && cachedCode.CurrentUses >= cachedCode.MaxUses.Value)
                {
                    cachedCode.IsActive = false;
                }

                await cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(cachedCode),
                    new DistributedCacheEntryOptions { AbsoluteExpiration = cachedCode.ExpiresAt.AddHours(1) });

                for (int attempt = 0; attempt < 3; attempt++)
                {
                    try
                    {
                        var dbCode = await db.AccessCodes.FirstOrDefaultAsync(c => c.Id == cachedCode.Id);
                        if (dbCode != null)
                        {
                            dbCode.CurrentUses = cachedCode.CurrentUses;
                            dbCode.IsActive = cachedCode.IsActive;
                            await db.SaveChangesAsync();
                        }
                        break;
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (attempt == 2) throw;
                    }
                }

                return new AccessCodeValidationResult
                {
                    IsValid = true,
                    Message = "Access code is valid.",
                    ResidentId = cachedCode.ResidentId,
                    AccessCodeId = cachedCode.Id
                };
            }

            // On cache miss, validate from DB as fallback
            var accessCode = await db.AccessCodes.FirstOrDefaultAsync(ac => ac.Code == code && ac.IsActive);
            if (accessCode == null || accessCode.ExpiresAt < DateTime.UtcNow)
            {
                if (accessCode != null)
                {
                    accessCode.IsActive = false;
                    await db.SaveChangesAsync();
                }
                return new AccessCodeValidationResult
                {
                    IsValid = false,
                    Message = "Access code invalid.",
                    ResidentId = accessCode?.ResidentId,
                    AccessCodeId = accessCode?.Id
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

            var newCache = new CachedAccessCode
            {
                Id = accessCode.Id,
                Code = accessCode.Code,
                ResidentId = accessCode.ResidentId,
                CodeHash = accessCode.CodeHash,
                CodeType = accessCode.CodeType,
                ExpiresAt = accessCode.ExpiresAt,
                MaxUses = accessCode.MaxUses,
                CurrentUses = accessCode.CurrentUses,
                IsActive = accessCode.IsActive,
                RowVersion = accessCode.RowVersion,
                IsDeprecated = accessCode.IsDeprecated
            };
            await cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(newCache),
                new DistributedCacheEntryOptions { AbsoluteExpiration = accessCode.ExpiresAt.AddHours(1) });

            return new AccessCodeValidationResult
            {
                IsValid = true,
                Message = "Access code is valid.",
                ResidentId = accessCode.ResidentId,
                AccessCodeId = accessCode.Id
            };
        }

        public async Task<GetAccessCodeResult?> GetAccessCodeByIdAsync(Guid id)
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
                Code = accessCode.Code,
                CodeType = accessCode.CodeType,
                ExpiresAt = accessCode.ExpiresAt.ToString("f"),
                MaxUses = accessCode.MaxUses,
                CurrentUses = accessCode.CurrentUses,
                IsActive = accessCode.IsActive,
                CreatedAt = accessCode.CreatedAt.ToString("f")
            };
        }

        public async Task<List<GetAccessCodeResult>?> GetAccessCodes(Guid id)
        {
            var accessCodes = await db.AccessCodes
                    .AsNoTracking()
                    .Where(c => c.ResidentId == id)
                    .ToListAsync();

            return accessCodes.Select(ac => new GetAccessCodeResult
            {
                Id = ac.Id,
                ResidentId = ac.ResidentId,
                Code = ac.Code,
                CodeType = ac.CodeType,
                ExpiresAt = ac.ExpiresAt.ToString("f"),
                MaxUses = ac.MaxUses,
                CurrentUses = ac.CurrentUses,
                IsActive = ac.IsActive,
                CreatedAt = ac.CreatedAt.ToString("f"),
            }).ToList();
        }

        public async Task<bool> DeleteAccessCodeAsync(Guid id)
        {
            var code = await db.AccessCodes.FirstOrDefaultAsync(c => c.Id == id);
            if (code == null || code.IsDeprecated)
            {
                return false;
            }

            code.IsDeprecated = true;
            code.IsActive = false;
            await db.SaveChangesAsync();

            var cacheKey = $"{AccessCodeCacheKeyPrefix}{code.CodeHash}";
            await cache.RemoveAsync(cacheKey);

            return true;
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

        private async Task InvalidateCodeAsync(string cacheKey, Guid codeId)
        {
            await cache.RemoveAsync(cacheKey);
            var dbEntry = await db.AccessCodes.FirstOrDefaultAsync(ac => ac.Id == codeId);
            if (dbEntry != null && dbEntry.IsActive)
            {
                dbEntry.IsActive = false;
                await db.SaveChangesAsync();
            }
        }
    }
}
