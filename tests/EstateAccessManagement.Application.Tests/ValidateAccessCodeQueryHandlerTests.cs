using EstateAccessManagement.Application.Features.AccessCodes.DTOs;
using EstateAccessManagement.Application.Features.AccessCodes.Queries;
using EstateAccessManagement.Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;

namespace EstateAccessManagement.Application.Tests
{
    public class ValidateAccessCodeQueryHandlerTests
    {
        private readonly Mock<IAccessCodeService> _accessCodeServiceMock;
        private readonly Mock<ILogger<ValidateAccessCodeQueryHandler>> _loggerMock;
        private readonly ValidateAccessCodeQueryHandler _handler;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;

        public ValidateAccessCodeQueryHandlerTests()
        {
            _accessCodeServiceMock = new Mock<IAccessCodeService>();
            _loggerMock = new Mock<ILogger<ValidateAccessCodeQueryHandler>>();
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();

            var httpContext = new DefaultHttpContext();
            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
            }));

            _httpContextAccessorMock.Setup(a => a.HttpContext).Returns(httpContext);
            _handler = new ValidateAccessCodeQueryHandler(_loggerMock.Object, _accessCodeServiceMock.Object,
                   _httpContextAccessorMock.Object);
        }

        [Fact]
        public async Task Handle_ReturnsValidationResult_FromService()
        {
            var code = "123456";
            var validationResult = new AccessCodeValidationResult
            {
                IsValid = true,
                Message = "Valid code",
                ResidentId = Guid.NewGuid(),
                AccessCodeId = Guid.NewGuid(),
                VerifiedBy = "Security Name"
            };

            _accessCodeServiceMock.Setup(s => s.ValidateAccessCodeAsync(code, It.IsAny<Guid>()))
                .ReturnsAsync(validationResult);

            var request = new ValidateAccessCodeQuery { Code = code };

            var result = await _handler.Handle(request, CancellationToken.None);

            Assert.NotNull(result);
            Assert.True(result.IsValid);
            Assert.Equal(validationResult.Message, result.Message);
        }
    }

}
