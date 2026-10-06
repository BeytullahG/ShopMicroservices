using FluentValidation;

using Shop.Identity.Api.Contracts;

namespace Shop.Identity.Api.Validators;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor (x => x.Email)
            .NotEmpty();
        RuleFor (x => x.Password)
            .NotEmpty();
    }
}