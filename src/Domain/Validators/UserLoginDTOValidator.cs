using Domain.Models.Read;
using FluentValidation;

namespace Domain.Validators;

public class UserLoginDTOValidator : AbstractValidator<UserLoginDTO>
{
    public UserLoginDTOValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required");
    }
}