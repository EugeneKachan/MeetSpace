using MediatR;

namespace MeetSpace.Application.Features.Offices.AssignManager;

/// <summary>Assigns an OfficeManager user to an Office. Admin only (FR-4).</summary>
public record AssignManagerCommand(Guid OfficeId, string UserId) : IRequest;
