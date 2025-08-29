using EstateAccessManagement.Application.DTOs;
using FluentValidation;

namespace EstateAccessManagement.Application.Validators
{
    public class RegisterUserRequestValidator : AbstractValidator<RegisterUserRequest>
    {
        public RegisterUserRequestValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
            RuleFor(x => x.FirstName).NotEmpty().MaximumLength(30);
            RuleFor(x => x.LastName).NotEmpty().MaximumLength(30);
            RuleFor(x => x.UserType).IsInEnum();
        }
    }
}
