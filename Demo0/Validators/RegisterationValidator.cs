using Demo0.DTOs;
using FluentValidation;

namespace Demo0.Validators;

public class RegisterationValidator : AbstractValidator<RegisterDto>
{
    public RegisterationValidator()
    {
        RuleFor(r => r.Username)
        .NotNull().WithMessage("Username is required")
        .NotEmpty().WithMessage("Username is requires")
        .MaximumLength(50).WithMessage("Username can't exceed 50 characters");

        RuleFor(r => r.Email)
        .NotNull().WithMessage("Email is required")
        .NotEmpty().WithMessage("Email is required")
        .EmailAddress().WithMessage("Dose't match email format")
        .MaximumLength(100).WithMessage("Username can't exceed 100 characters");

        RuleFor(r => r.Password)
        .NotNull().WithMessage("Password is required")
        .NotEmpty().WithMessage("Password is required")
        .Length(8, 100).WithMessage("Password must be between 8-100 characters");
    }
}
