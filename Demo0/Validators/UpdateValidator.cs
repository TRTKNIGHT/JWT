using Demo0.DTOs;
using FluentValidation;

namespace Demo0.Validators;

public class UpdateValidator : AbstractValidator<UpdateDto>
{
    public UpdateValidator()
    {
        RuleFor(u => u.Username)
        .MaximumLength(50).WithMessage("Username can't exceed 50 characters");

        RuleFor(u => u.Email)
        .EmailAddress().WithMessage("Dose't match email format")
        .MaximumLength(100).WithMessage("Username can't exceed 100 characters");

        RuleFor(u => u.Password)
        .Length(8, 100).WithMessage("Password must be between 8-100 characters");
    }
}
