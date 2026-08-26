using ConferenceHub.Application.DTOs.Auth;
using FluentValidation;

namespace ConferenceHub.Application.Validators.Auth;

public class RegisterDtoValidator : AbstractValidator<RegisterDto>
{
    public RegisterDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email must be a valid email address")
            .MaximumLength(256);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters long");

        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage("UserName is required")
            .MinimumLength(2).WithMessage("UserName must be at least 6 characters long")
            .MaximumLength(64);
    }
}
