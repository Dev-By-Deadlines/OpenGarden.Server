using FluentValidation;
using OpenGarden.Server.Dtos;

namespace OpenGarden.Server.Validators;

public class RegisterValidator : AbstractValidator<RegisterDto>
{
    public RegisterValidator()
    {
        RuleFor(x => x.UserName)
            .NotEmpty()
            .MinimumLength(2)
            .MaximumLength(20);

        RuleFor(x => x.Password)
            .MinimumLength(8);

        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.Password)
            .WithMessage("Passwords do not match.");
    }
}
