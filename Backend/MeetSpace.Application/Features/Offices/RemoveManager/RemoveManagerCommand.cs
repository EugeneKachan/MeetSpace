using MediatR;

namespace MeetSpace.Application.Features.Offices.RemoveManager;

/// <summary>Removes an OfficeManager assignment from an Office. Admin only.</summary>
public record RemoveManagerCommand(Guid OfficeId, string UserId) : IRequest;
