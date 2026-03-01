using FluentValidation;

namespace MeetSpace.Application.Features.Offices.CreateOffice;

public class CreateOfficeCommandValidator : AbstractValidator<CreateOfficeCommand>
{
    public CreateOfficeCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Office name is required.")
            .MaximumLength(200).WithMessage("Office name must not exceed 200 characters.");

        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("Address is required.")
            .MaximumLength(500).WithMessage("Address must not exceed 500 characters.");

        RuleForEach(x => x.Rooms).ChildRules(room =>
        {
            room.RuleFor(r => r.Name)
                .NotEmpty().WithMessage("Room name is required.")
                .MaximumLength(200).WithMessage("Room name must not exceed 200 characters.");

            room.RuleFor(r => r.Capacity)
                .GreaterThan(0).WithMessage("Room capacity must be greater than 0.");

            room.RuleFor(r => r.Description)
                .MaximumLength(1000).WithMessage("Room description must not exceed 1000 characters.");
        });
    }
}
