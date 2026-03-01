using MediatR;

namespace MeetSpace.Application.Features.Offices.GetActiveOffices;

/// <summary>
/// Returns all active offices available for booking discovery.
/// Accessible by all authenticated users (FR-9).
/// </summary>
public record GetActiveOfficesQuery : IRequest<IReadOnlyList<ActiveOfficeDto>>;
