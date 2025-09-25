using EstateAccessManagement.Application.Features.AccessCodes.Commands;
using EstateAccessManagement.Application.Features.AccessCodes.DTOs;
using EstateAccessManagement.Application.Interfaces;
using EstateAccessManagement.Common.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;

namespace EstateAccessManagement.Application.Tests
{
    public class GenerateAccessCodeCommandHandlerTests
    {
        private readonly Mock<IAccessCodeService> _accessCodeServiceMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Mock<ILogger<GenerateAccessCodeCommandHandler>> _loggerMock;
        private readonly GenerateAccessCodeCommandHandler _handler;

        public GenerateAccessCodeCommandHandlerTests()
        {
            _accessCodeServiceMock = new Mock<IAccessCodeService>();
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _loggerMock = new Mock<ILogger<GenerateAccessCodeCommandHandler>>();

            var httpContext = new DefaultHttpContext();
            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
            }));

            _httpContextAccessorMock.Setup(a => a.HttpContext).Returns(httpContext);

            _handler = new GenerateAccessCodeCommandHandler(_loggerMock.Object, _accessCodeServiceMock.Object, _httpContextAccessorMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnAccessCodeResult_WhenValid()
        {
            var residentId = Guid.NewGuid();
            var command = new GenerateAccessCodeCommand { CodeType = AccessCodeType.TemporaryVisitor };

            var accessCodeResult = new GenerateAccessCodeResult
            {
                Id = Guid.NewGuid(),
                ResidentId = residentId,
                CodeType = command.CodeType,
                Code = "TV7840",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddHours(1),
                MaxUses = 1,
                CurrentUses = 0
            };

            _accessCodeServiceMock.Setup(s => s.GenerateAccessCodeAsync(It.IsAny<Guid>(), command.CodeType))
                .ReturnsAsync(accessCodeResult);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(accessCodeResult.Id, result.Id);
            Assert.Equal(accessCodeResult.Code, result.Code);
            Assert.Equal(accessCodeResult.CodeType, result.CodeType);
        }

        [Fact]
        public async Task Handle_ShouldThrowUnauthorizedAccessException_WhenNoUserId()
        {
            var emptyContext = new DefaultHttpContext();
            emptyContext.User = new ClaimsPrincipal();

            _httpContextAccessorMock.Setup(a => a.HttpContext).Returns(emptyContext);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _handler.Handle(new GenerateAccessCodeCommand(), CancellationToken.None));
        }
    }
}
