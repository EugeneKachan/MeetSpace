using MediatR;

namespace MeetSpace.Application.Features.Offices.DeactivateOffice;

/// <summary>Soft-deletes an office by setting IsActive = false (FR-5).</summary>
public record DeactivateOfficeCommand(Guid Id) : IRequest;
