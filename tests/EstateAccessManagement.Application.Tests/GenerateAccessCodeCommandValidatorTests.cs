using EstateAccessManagement.Application.Features.AccessCodes.Commands;
using EstateAccessManagement.Application.Features.AccessCodes.Validators;
using EstateAccessManagement.Common.Enums;
using FluentValidation.TestHelper;

namespace EstateAccessManagement.Application.Tests;
public class GenerateAccessCodeCommandValidatorTests
{
    private readonly GenerateAccessCodeCommandValidator _validator;

    public GenerateAccessCodeCommandValidatorTests()
    {
        _validator = new GenerateAccessCodeCommandValidator();
    }

    [Theory]
    [InlineData(AccessCodeType.None)]
    [InlineData((AccessCodeType)999)]
    public void Should_HaveError_When_CodeTypeInvalid(AccessCodeType codeType)
    {
        var model = new GenerateAccessCodeCommand { CodeType = codeType };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.CodeType);
    }

    [Theory]
    [InlineData(AccessCodeType.TemporaryVisitor)]
    [InlineData(AccessCodeType.LongStayVisitor)]
    public void Should_NotHaveError_When_CodeTypeValid(AccessCodeType codeType)
    {
        var model = new GenerateAccessCodeCommand { CodeType = codeType };
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.CodeType);
    }
}
