using EstateAccessManagement.Core.Entities;
using EstateAccessManagement.Core.Enums;
using EstateAccessManagement.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text;
using System.Text.Json;

namespace EstateAccessManagement.Infrastructure.Tests;
public class AccessCodeServiceTests
{
    private readonly ApplicationDbContext _dbContext;
    private readonly AccessCodeService _accessCodeService;
    private readonly Mock<ILogger<AccessCodeService>> _loggerMock;
    private readonly Mock<IDistributedCache> _cacheMock;

    public AccessCodeServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _dbContext = new ApplicationDbContext(options);
        _loggerMock = new Mock<ILogger<AccessCodeService>>();
        _cacheMock = new Mock<IDistributedCache>();

        _accessCodeService = new AccessCodeService(_loggerMock.Object, _dbContext, _cacheMock.Object);
    }

    private byte[] SerializeObject<T>(T obj)
    {
        var json = JsonSerializer.Serialize(obj);
        return Encoding.UTF8.GetBytes(json);
    }

    [Fact]
    public async Task GenerateAccessCodeAsync_ShouldCreateAndReturnCode_ForValidResidentIdAndType()
    {
        var residentId = Guid.NewGuid();
        var codeType = AccessCodeType.LongStayVisitor;

        _cacheMock.Setup(c => c.SetAsync(
            It.IsAny<string>(),
            It.IsAny<byte[]>(),
            It.IsAny<DistributedCacheEntryOptions>(),
            It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _accessCodeService.GenerateAccessCodeAsync(residentId, codeType);

        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(residentId, result.ResidentId);
        Assert.Equal(codeType, result.CodeType);
        Assert.Single(_dbContext.AccessCodes);
        var savedCode = await _dbContext.AccessCodes.FirstAsync();
        Assert.Equal(result.Id, savedCode.Id);

        _cacheMock.Verify(c => c.SetAsync(
            It.IsAny<string>(),
            It.IsAny<byte[]>(),
            It.IsAny<DistributedCacheEntryOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ValidateAccessCodeAsync_ShouldReturnFalse_ForExpiredCode()
    {
        var residentId = Guid.NewGuid();
        var rawCode = "EXPIREDCODE";
        var hashedCode = AccessCodeService.HashCode(rawCode);
        var accessCode = new AccessCode
        {
            Id = Guid.NewGuid(),
            ResidentId = residentId,
            CodeHash = hashedCode,
            CreatedAt = DateTime.UtcNow.AddHours(-2),
            ExpiresAt = DateTime.UtcNow.AddHours(-1),
            MaxUses = 1,
            CurrentUses = 0,
            IsActive = true,
            CodeType = AccessCodeType.TemporaryVisitor
        };
        _dbContext.AccessCodes.Add(accessCode);
        await _dbContext.SaveChangesAsync();

        _cacheMock.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                  .ReturnsAsync((byte[])null);

        var result = await _accessCodeService.ValidateAccessCodeAsync(rawCode);

        Assert.False(result.IsValid);
        Assert.Equal("Access code has expired.", result.Message);
        Assert.Equal(accessCode.Id, result.AccessCodeId);
        Assert.Equal(accessCode.ResidentId, result.ResidentId);

        var updatedCode = await _dbContext.AccessCodes.FindAsync(accessCode.Id);
        Assert.False(updatedCode!.IsActive);
    }

    [Fact]
    public async Task ValidateAccessCodeAsync_ShouldReturnFalse_ForExhaustedCode()
    {
        var residentId = Guid.NewGuid();
        var rawCode = "EXHAUSTEDCODE";
        var hashedCode = AccessCodeService.HashCode(rawCode);
        var accessCode = new AccessCode
        {
            Id = Guid.NewGuid(),
            ResidentId = residentId,
            CodeHash = hashedCode,
            CreatedAt = DateTime.UtcNow.AddHours(-2),
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            MaxUses = 1,
            CurrentUses = 1,
            IsActive = true,
            CodeType = AccessCodeType.TemporaryVisitor
        };
        _dbContext.AccessCodes.Add(accessCode);
        await _dbContext.SaveChangesAsync();

        _cacheMock.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                  .ReturnsAsync((byte[])null);

        var result = await _accessCodeService.ValidateAccessCodeAsync(rawCode);

        Assert.False(result.IsValid);
        Assert.Equal("Access code has reached its maximum number of uses.", result.Message);
        Assert.Equal(accessCode.Id, result.AccessCodeId);
        Assert.Equal(accessCode.ResidentId, result.ResidentId);

        var updatedCode = await _dbContext.AccessCodes.FindAsync(accessCode.Id);
        Assert.False(updatedCode!.IsActive);
    }

    [Fact]
    public async Task ValidateAccessCodeAsync_ShouldReturnFalse_ForNonexistentCode()
    {
        _cacheMock.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                  .ReturnsAsync((byte[])null);

        var result = await _accessCodeService.ValidateAccessCodeAsync("NONEXISTENT");

        Assert.False(result.IsValid);
        Assert.Equal("Access code not found or inactive.", result.Message);
        Assert.Null(result.AccessCodeId);
        Assert.Null(result.ResidentId);
    }

    [Fact]
    public async Task ValidateAccessCodeAsync_ShouldReturnFalse_ForInactiveCode()
    {
        var rawCode = "INACTIVECODE";
        var hashedCode = AccessCodeService.HashCode(rawCode);
        var accessCode = new AccessCode
        {
            Id = Guid.NewGuid(),
            ResidentId = Guid.NewGuid(),
            CodeHash = hashedCode,
            CreatedAt = DateTime.UtcNow.AddHours(-2),
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            MaxUses = null,
            CurrentUses = 0,
            IsActive = false,
            CodeType = AccessCodeType.LongStayVisitor
        };
        _dbContext.AccessCodes.Add(accessCode);
        await _dbContext.SaveChangesAsync();

        _cacheMock.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                  .ReturnsAsync((byte[])null);

        var result = await _accessCodeService.ValidateAccessCodeAsync(rawCode);

        Assert.False(result.IsValid);
        Assert.Equal("Access code not found or inactive.", result.Message);
        Assert.Null(result.AccessCodeId);
        Assert.Null(result.ResidentId);
    }
}
