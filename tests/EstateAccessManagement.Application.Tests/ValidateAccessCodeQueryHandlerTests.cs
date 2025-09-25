using EstateAccessManagement.Application.Features.AccessCodes.DTOs;
using EstateAccessManagement.Application.Features.AccessCodes.Queries;
using EstateAccessManagement.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;

namespace EstateAccessManagement.Application.Tests
{
    public class ValidateAccessCodeQueryHandlerTests
    {
        private readonly Mock<IAccessCodeService> _accessCodeServiceMock;
        private readonly Mock<ILogger<ValidateAccessCodeQueryHandler>> _loggerMock;
        private readonly ValidateAccessCodeQueryHandler _handler;

        public ValidateAccessCodeQueryHandlerTests()
        {
            _accessCodeServiceMock = new Mock<IAccessCodeService>();
            _loggerMock = new Mock<ILogger<ValidateAccessCodeQueryHandler>>();
            _handler = new ValidateAccessCodeQueryHandler(_loggerMock.Object, _accessCodeServiceMock.Object);
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
                AccessCodeId = Guid.NewGuid()
            };

            _accessCodeServiceMock.Setup(s => s.ValidateAccessCodeAsync(code))
                .ReturnsAsync(validationResult);

            var request = new ValidateAccessCodeQuery { Code = code };

            var result = await _handler.Handle(request, CancellationToken.None);

            Assert.NotNull(result);
            Assert.True(result.IsValid);
            Assert.Equal(validationResult.Message, result.Message);
        }
    }

}
