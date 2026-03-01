using MediatR;
using MeetSpace.Application.Interfaces;

namespace MeetSpace.Application.Features.Bookings.GetUserBookings;

public class GetUserBookingsQueryHandler : IRequestHandler<GetUserBookingsQuery, List<BookingSummaryDto>>
{
    private readonly IBookingRepository _bookings;

    public GetUserBookingsQueryHandler(IBookingRepository bookings) => _bookings = bookings;

    public async Task<List<BookingSummaryDto>> Handle(GetUserBookingsQuery request, CancellationToken ct)
    {
        var bookings = await _bookings.GetByUserIdAsync(request.UserId, ct);

        return bookings
            .Select(b => new BookingSummaryDto(
                b.Id,
                b.RoomId,
                b.Room?.Name ?? string.Empty,
                b.Room?.Office?.Name ?? string.Empty,
                b.StartTime.ToString("yyyy-MM-dd"),
                b.StartTime.ToString("HH:mm"),
                b.EndTime.ToString("HH:mm"),
                b.Title,
                b.IsCancelled
            ))
            .ToList();
    }
}
