using MediatR;
using MeetSpace.Application.Interfaces;

namespace MeetSpace.Application.Features.Offices.GetActiveOffices;

public class GetActiveOfficesQueryHandler : IRequestHandler<GetActiveOfficesQuery, IReadOnlyList<ActiveOfficeDto>>
{
    private readonly IOfficeRepository _repo;

    public GetActiveOfficesQueryHandler(IOfficeRepository repo) => _repo = repo;

    public async Task<IReadOnlyList<ActiveOfficeDto>> Handle(GetActiveOfficesQuery request, CancellationToken ct)
    {
        var offices = await _repo.GetActiveAsync(ct);
        return offices
            .Select(o => new ActiveOfficeDto(o.Id, o.Name, o.Address))
            .ToList();
    }
}
