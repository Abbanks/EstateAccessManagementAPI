using EstateAccessManagement.Application.Features.AccessCodes.Queries;
using FluentValidation;

namespace EstateAccessManagement.Application.Features.AccessCodes.Validators
{
    public class ValidateAccessCodeQueryValidator : AbstractValidator<ValidateAccessCodeQuery>
    {
        public ValidateAccessCodeQueryValidator()
        {
            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("Access code is required.")
                .Length(6).WithMessage("Access code must be 8 characters long."); 
        }
    }
}
