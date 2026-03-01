using MediatR;

namespace MeetSpace.Application.Features.Offices.GetOffices;

/// <summary>
/// Returns offices with their rooms and assigned managers.
/// When <see cref="FilterByUserId"/> is set the result is limited to offices
/// the given user is assigned to (used for OfficeManager role).
/// </summary>
public record GetOfficesQuery(
    string? FilterByUserId = null
) : IRequest<IReadOnlyList<OfficeDto>>;
