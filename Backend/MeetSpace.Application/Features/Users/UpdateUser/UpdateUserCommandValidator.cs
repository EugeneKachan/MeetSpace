using FluentValidation;
using MeetSpace.Domain.Enums;

namespace MeetSpace.Application.Features.Users.UpdateUser;

public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    private static readonly string[] AllowedRoles =
        [UserRoles.Employee, UserRoles.OfficeManager, UserRoles.Admin];

    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(100).WithMessage("First name must not exceed 100 characters.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(100).WithMessage("Last name must not exceed 100 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.")
            .MaximumLength(256).WithMessage("Email must not exceed 256 characters.");

        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("Role is required.")
            .Must(r => AllowedRoles.Contains(r))
            .WithMessage($"Role must be one of: {string.Join(", ", [UserRoles.Employee, UserRoles.OfficeManager, UserRoles.Admin])}.");
    }
}
