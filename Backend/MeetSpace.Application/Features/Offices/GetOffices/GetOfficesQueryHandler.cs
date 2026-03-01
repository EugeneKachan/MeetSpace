using MediatR;
using MeetSpace.Application.Interfaces;

namespace MeetSpace.Application.Features.Offices.GetOffices;

public class GetOfficesQueryHandler : IRequestHandler<GetOfficesQuery, IReadOnlyList<OfficeDto>>
{
    private readonly IOfficeRepository _repo;

    public GetOfficesQueryHandler(IOfficeRepository repo) => _repo = repo;

    public async Task<IReadOnlyList<OfficeDto>> Handle(GetOfficesQuery request, CancellationToken ct)
    {
        var offices = string.IsNullOrEmpty(request.FilterByUserId)
            ? await _repo.GetAllAsync(ct)
            : await _repo.GetByAssignedUserAsync(request.FilterByUserId, ct);

        return offices.Select(o => new OfficeDto(
            o.Id,
            o.Name,
            o.Address,
            o.IsActive,
            o.Rooms.Select(r => new RoomDto(r.Id, r.Name, r.Capacity, r.Description, r.IsActive))
                   .ToList(),
            o.Assignments.Select(a => new ManagerDto(a.UserId, a.User.FirstName, a.User.LastName, a.User.Email ?? string.Empty))
                         .ToList()
        )).ToList();
    }
}
