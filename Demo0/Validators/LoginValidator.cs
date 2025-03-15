using Demo0.DTOs;
using FluentValidation;

namespace Demo0.Validators;

public class LoginValidator : AbstractValidator<LoginDto>
{
    public LoginValidator()
    {
        RuleFor(l => l.Email)
        .NotNull().WithMessage("Email is required")
        .NotEmpty().WithMessage("Email is required")
        .EmailAddress().WithMessage("Dose't match email format")
        .MaximumLength(100).WithMessage("Username can't exceed 100 characters");

        RuleFor(l => l.Password)
        .NotNull().WithMessage("Password is required")
        .NotEmpty().WithMessage("Password is required")
        .Length(8, 100).WithMessage("Password must be between 8-100 characters");

        RuleFor(l => l.ClientId)
        .NotNull().WithMessage("ClientId is required")
        .NotEmpty().WithMessage("ClientId is required");
    }
}
