using EstateAccessManagement.Application.Features.AccessCodes.Queries;
using FluentValidation;

namespace EstateAccessManagement.Application.Features.AccessCodes.Validators
{
    public class GetAccessCodeQueryValidator : AbstractValidator<GetAccessCodeQuery>
    {
        public GetAccessCodeQueryValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Id must be provided.");
        }
    }
}
