using MediatR;
using MeetSpace.Application.Interfaces;

namespace MeetSpace.Application.Features.Rooms.GetRooms;

public class GetRoomsQueryHandler : IRequestHandler<GetRoomsQuery, IReadOnlyList<RoomListDto>>
{
    private readonly IRoomRepository _repo;

    public GetRoomsQueryHandler(IRoomRepository repo) => _repo = repo;

    public async Task<IReadOnlyList<RoomListDto>> Handle(GetRoomsQuery request, CancellationToken ct)
    {
        var rooms = await _repo.GetActiveByOfficeAsync(request.OfficeId, request.MinCapacity, ct);
        return rooms
            .Select(r => new RoomListDto(r.Id, r.Name, r.Capacity, r.Description))
            .ToList();
    }
}
