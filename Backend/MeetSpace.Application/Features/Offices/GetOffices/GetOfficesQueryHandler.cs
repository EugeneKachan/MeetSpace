using MediatR;
using MeetSpace.Application.Common;
using MeetSpace.Application.Interfaces;

namespace MeetSpace.Application.Features.Offices.GetOffices;

public class GetOfficesQueryHandler : IRequestHandler<GetOfficesQuery, PagedResult<OfficeDto>>
{
    private readonly IOfficeRepository _repo;

    public GetOfficesQueryHandler(IOfficeRepository repo) => _repo = repo;

    public async Task<PagedResult<OfficeDto>> Handle(GetOfficesQuery request, CancellationToken ct)
    {
        var offices = string.IsNullOrEmpty(request.FilterByUserId)
            ? await _repo.GetAllAsync(ct)
            : await _repo.GetByAssignedUserAsync(request.FilterByUserId, ct);

        var dtos = offices.Select(o => new OfficeDto(
            o.Id,
            o.Name,
            o.Address,
            o.IsActive,
            o.Rooms.Select(r => new RoomDto(r.Id, r.Name, r.Capacity, r.Description, r.IsActive))
                   .ToList(),
            o.Assignments.Select(a => new ManagerDto(a.UserId, a.User.FirstName, a.User.LastName, a.User.Email ?? string.Empty))
                         .ToList()
        )).ToList();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            dtos = dtos.Where(o =>
                o.Name.ToLower().Contains(search) ||
                o.Address.ToLower().Contains(search)).ToList();
        }

        dtos = (request.SortBy?.ToLower(), request.SortDir?.ToLower()) switch
        {
            ("address",   "desc") => dtos.OrderByDescending(o => o.Address).ToList(),
            ("address",   _)     => dtos.OrderBy(o => o.Address).ToList(),
            ("roomcount", "desc") => dtos.OrderByDescending(o => o.Rooms.Count).ToList(),
            ("roomcount", _)     => dtos.OrderBy(o => o.Rooms.Count).ToList(),
            ("status",    "desc") => dtos.OrderByDescending(o => o.IsActive).ToList(),
            ("status",    _)     => dtos.OrderBy(o => o.IsActive).ToList(),
            (_,           "desc") => dtos.OrderByDescending(o => o.Name).ToList(),
            _                    => dtos.OrderBy(o => o.Name).ToList(),
        };

        var totalCount = dtos.Count;
        var paged = dtos
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        return new PagedResult<OfficeDto>(paged.AsReadOnly(), totalCount, request.Page, request.PageSize);
    }
}
