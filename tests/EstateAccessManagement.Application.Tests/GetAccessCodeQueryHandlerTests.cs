using EstateAccessManagement.Application.Features.AccessCodes.DTOs;
using EstateAccessManagement.Application.Features.AccessCodes.Queries;
using EstateAccessManagement.Application.Interfaces;
using EstateAccessManagement.Common.Enums;
using Microsoft.Extensions.Logging;
using Moq;

namespace EstateAccessManagement.Application.Tests
{
    public class GetAccessCodeQueryHandlerTests
    {
        private readonly Mock<IAccessCodeService> _accessCodeServiceMock;
        private readonly Mock<ILogger<GetAccessCodeQueryHandler>> _loggerMock;
        private readonly GetAccessCodeQueryHandler _handler;

        public GetAccessCodeQueryHandlerTests()
        {
            _accessCodeServiceMock = new Mock<IAccessCodeService>();
            _loggerMock = new Mock<ILogger<GetAccessCodeQueryHandler>>();
            _handler = new GetAccessCodeQueryHandler(_loggerMock.Object, _accessCodeServiceMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnResult_WhenAccessCodeExists()
        {
            var accessCodeId = Guid.NewGuid();
            var accessCode = new GetAccessCodeResult
            {
                Id = accessCodeId,
                CodeType = AccessCodeType.LongStayVisitor,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                MaxUses = null,
                CurrentUses = 1
            };

            _accessCodeServiceMock.Setup(s => s.GetAccessCodeByIdAsync(accessCodeId))
                .ReturnsAsync(accessCode);

            var request = new GetAccessCodeQuery { Id = accessCodeId };

            var result = await _handler.Handle(request, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(accessCodeId, result.Id);
            Assert.True(result.IsActive);
        }

        [Fact]
        public async Task Handle_ShouldReturnNull_WhenAccessCodeDoesNotExist()
        {
            var accessCodeId = Guid.NewGuid();
            _accessCodeServiceMock.Setup(s => s.GetAccessCodeByIdAsync(accessCodeId))
                .ReturnsAsync((GetAccessCodeResult)null);

            var request = new GetAccessCodeQuery { Id = accessCodeId };
            var result = await _handler.Handle(request, CancellationToken.None);

            Assert.Null(result);
        }
    }

}
