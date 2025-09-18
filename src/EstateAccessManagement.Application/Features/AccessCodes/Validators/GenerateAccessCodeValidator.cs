using EstateAccessManagement.Application.Features.AccessCodes.Commands;
using EstateAccessManagement.Core.Enums;
using FluentValidation;

namespace EstateAccessManagement.Application.Features.AccessCodes.Validators
{
    public class GenerateAccessCodeCommandValidator : AbstractValidator<GenerateAccessCodeCommand>
    {
        public GenerateAccessCodeCommandValidator()
        {
            RuleFor(x => x.CodeType)
                .NotEmpty().WithMessage("CodeType is required.")
                .IsInEnum().WithMessage("The selected CodeType is not valid.")
                .NotEqual(AccessCodeType.None).WithMessage("Please select a code type");
        }
    }
}
