using EstateAccessManagement.Application.DTOs;
using FluentValidation;

namespace EstateAccessManagement.Application.Features.Users.Validators
{
    public class LoginValidator : AbstractValidator<LoginRequest>
    {
        public LoginValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Password).NotEmpty();
        }
    }
}
