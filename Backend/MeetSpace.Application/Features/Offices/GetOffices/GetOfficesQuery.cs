using MediatR;
using MeetSpace.Application.Common;

namespace MeetSpace.Application.Features.Offices.GetOffices;

/// <summary>
/// Returns offices with their rooms and assigned managers.
/// When <see cref="FilterByUserId"/> is set the result is limited to offices
/// the given user is assigned to (used for OfficeManager role).
/// </summary>
public record GetOfficesQuery(
    string? FilterByUserId = null,
    int Page = 1,
    int PageSize = 10,
    string? Search = null,
    string SortBy = "name",
    string SortDir = "asc"
) : IRequest<PagedResult<OfficeDto>>;
