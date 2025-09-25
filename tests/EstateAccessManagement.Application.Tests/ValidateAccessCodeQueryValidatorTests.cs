using EstateAccessManagement.Application.Features.AccessCodes.Queries;
using EstateAccessManagement.Application.Features.AccessCodes.Validators;
using FluentValidation.TestHelper;

namespace EstateAccessManagement.Application.Tests
{
    public class ValidateAccessCodeQueryValidatorTests
    {
        private readonly ValidateAccessCodeQueryValidator _validator;

        public ValidateAccessCodeQueryValidatorTests()
        {
            _validator = new ValidateAccessCodeQueryValidator();
        }

        [Fact]
        public void Should_HaveError_When_CodeIsEmpty()
        {
            var model = new ValidateAccessCodeQuery { Code = string.Empty };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Code)
                .WithErrorMessage("Access code is required.");
        }

        [Theory]
        [InlineData("TV784")]
        [InlineData("TV78407")]
        public void Should_HaveError_When_CodeLengthInvalid(string code)
        {
            var model = new ValidateAccessCodeQuery { Code = code };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Code)
                .WithErrorMessage("Access code must be 6 characters long.");
        }

        [Fact]
        public void Should_NotHaveError_When_CodeIsValidLength()
        {
            var model = new ValidateAccessCodeQuery { Code = "TV7840" };
            var result = _validator.TestValidate(model);
            result.ShouldNotHaveValidationErrorFor(x => x.Code);
        }
    }
}
