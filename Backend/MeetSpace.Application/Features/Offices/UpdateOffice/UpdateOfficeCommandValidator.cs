using FluentValidation;

namespace MeetSpace.Application.Features.Offices.UpdateOffice;

public class UpdateOfficeCommandValidator : AbstractValidator<UpdateOfficeCommand>
{
    public UpdateOfficeCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Office ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Office name is required.")
            .MaximumLength(200).WithMessage("Office name must not exceed 200 characters.");

        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("Address is required.")
            .MaximumLength(500).WithMessage("Address must not exceed 500 characters.");
    }
}
