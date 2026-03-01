using MediatR;

namespace MeetSpace.Application.Features.Offices.UpdateOffice;

/// <summary>Updates an existing office's name and address (FR-4).</summary>
public record UpdateOfficeCommand(
    Guid Id,
    string Name,
    string Address
) : IRequest;
