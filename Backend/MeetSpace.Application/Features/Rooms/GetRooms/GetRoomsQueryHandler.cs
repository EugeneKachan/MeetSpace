using MediatR;
using MeetSpace.Application.Interfaces;

namespace MeetSpace.Application.Features.Rooms.GetRooms;

public class GetRoomsQueryHandler : IRequestHandler<GetRoomsQuery, IReadOnlyList<RoomListDto>>
{
    private readonly IRoomRepository _repo;
    private readonly IBookingRepository _bookingRepo;

    public GetRoomsQueryHandler(IRoomRepository repo, IBookingRepository bookingRepo)
    {
        _repo = repo;
        _bookingRepo = bookingRepo;
    }

    public async Task<IReadOnlyList<RoomListDto>> Handle(GetRoomsQuery request, CancellationToken ct)
    {
        var rooms = await _repo.GetActiveByOfficeAsync(request.OfficeId, request.MinCapacity, ct);

        // If a specific date and time range is requested, filter out rooms that are already booked
        if (request.Date is not null && request.StartTime is not null && request.EndTime is not null)
        {
            var roomIds = rooms.Select(r => r.Id).ToList();

            // Retrieve bookings for the office on the requested date
            var bookings = await _bookingRepo.GetByOfficeAndDateAsync(
                request.OfficeId,
                request.Date.Value,
                ct);

            var unavailableRoomIds = bookings
                .Where(b => roomIds.Contains(b.RoomId) &&
                            TimeRangesOverlap(
                                request.StartTime.Value,
                                request.EndTime.Value,
                                b.StartTime,
                                b.EndTime))
                .Select(b => b.RoomId)
                .Distinct()
                .ToHashSet();

            rooms = rooms
                .Where(r => !unavailableRoomIds.Contains(r.Id))
                .ToList();
        }

        return rooms
            .Select(r => new RoomListDto(r.Id, r.Name, r.Capacity, r.Description))
            .ToList();
    }

    private static bool TimeRangesOverlap(TimeOnly requestedStart, TimeOnly requestedEnd, TimeOnly bookingStart, TimeOnly bookingEnd)
    {
        // Two intervals [a, b) and [c, d) overlap if a < d and c < b
        return requestedStart < bookingEnd && bookingStart < requestedEnd;
    }
}
