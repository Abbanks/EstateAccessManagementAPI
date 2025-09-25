using EstateAccessManagement.Application.Features.AccessCodes.Queries;
using EstateAccessManagement.Application.Features.AccessCodes.Validators;
using FluentValidation.TestHelper;

namespace EstateAccessManagement.Application.Tests
{
    public class GetAccessCodeQueryValidatorTests
    {
        private readonly GetAccessCodeQueryValidator _validator;

        public GetAccessCodeQueryValidatorTests()
        {
            _validator = new GetAccessCodeQueryValidator();
        }

        [Fact]
        public void Should_HaveError_When_IdIsEmpty()
        {
            var model = new GetAccessCodeQuery { Id = Guid.Empty };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Id)
                .WithErrorMessage("Id must be provided.");
        }

        [Fact]
        public void Should_NotHaveError_When_IdIsValid()
        {
            var model = new GetAccessCodeQuery { Id = Guid.NewGuid() };
            var result = _validator.TestValidate(model);
            result.ShouldNotHaveValidationErrorFor(x => x.Id);
        }
    }

}
