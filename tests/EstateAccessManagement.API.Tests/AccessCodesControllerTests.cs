using EstateAccessManagement.API.Controllers;
using EstateAccessManagement.Application.Features.AccessCodes.Commands;
using EstateAccessManagement.Application.Features.AccessCodes.DTOs;
using EstateAccessManagement.Application.Features.AccessCodes.Queries;
using EstateAccessManagement.Common.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace EstateAccessManagement.API.Tests
{
    public class AccessCodesControllerTests
    {
        private readonly Mock<IMediator> _mediatorMock;
        private readonly AccessCodesController _controller;

        public AccessCodesControllerTests()
        {
            _mediatorMock = new Mock<IMediator>();
            _controller = new AccessCodesController(_mediatorMock.Object);
        }

        [Fact]
        public async Task GenerateAccessCode_ReturnsCreatedResult_WithValidResult()
        {
            var command = new GenerateAccessCodeCommand { CodeType = AccessCodeType.TemporaryVisitor };
            var result = new GenerateAccessCodeResult { Id = Guid.NewGuid(), CodeType = command.CodeType };

            _mediatorMock.Setup(m => m.Send(command, It.IsAny<CancellationToken>())).ReturnsAsync(result);

            var actionResult = await _controller.GenerateAccessCode(command);

            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(actionResult);
            Assert.Equal(nameof(_controller.GenerateAccessCode), createdAtActionResult.ActionName);
            Assert.Equal(result, createdAtActionResult.Value);
        }

        [Fact]
        public async Task GetAccessCode_ReturnsOk_WhenCodeFound()
        {
            var id = Guid.NewGuid();
            var expectedResult = new GetAccessCodeResult { Id = id, IsActive = true };

            _mediatorMock.Setup(m => m.Send(It.Is<GetAccessCodeQuery>(q => q.Id == id), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResult);

            var result = await _controller.GetAccessCode(id);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<GetAccessCodeResult>(okResult.Value);
            Assert.Equal(expectedResult.Id, value.Id);
        }

        [Fact]
        public async Task GetAccessCode_ReturnsNotFound_WhenCodeNotFound()
        {
            var id = Guid.NewGuid();

            _mediatorMock.Setup(m => m.Send(It.IsAny<GetAccessCodeQuery>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync((GetAccessCodeResult?)null);

            var result = await _controller.GetAccessCode(id);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task ValidateAccessCode_ReturnsOk_WhenCodeValid()
        {
            var command = new ValidateAccessCodeQuery { Code = "TV7840" };
            var validationResult = new AccessCodeValidationResult
            {
                IsValid = true,
                Message = "Valid code",
                AccessCodeId = Guid.NewGuid(),
                ResidentId = Guid.NewGuid()
            };

            _mediatorMock.Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);

            var result = await _controller.ValidateAccessCode(command);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<AccessCodeValidationResult>(okResult.Value);
            Assert.True(value.IsValid);
        }

        [Fact]
        public async Task ValidateAccessCode_ReturnsBadRequest_WhenCodeInvalid()
        {
            var command = new ValidateAccessCodeQuery { Code = "LV7840" };
            var validationResult = new AccessCodeValidationResult
            {
                IsValid = false,
                Message = "Invalid code"
            };

            _mediatorMock.Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);

            var result = await _controller.ValidateAccessCode(command);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var value = Assert.IsType<AccessCodeValidationResult>(badRequestResult.Value);
            Assert.False(value.IsValid);
            Assert.Equal("Invalid code", value.Message);
        }
    }
}