using MediatR;

namespace MeetSpace.Application.Features.Bookings.GetUserBookings;

public record BookingSummaryDto(
    Guid Id,
    Guid RoomId,
    string RoomName,
    string OfficeName,
    string Date,        // YYYY-MM-DD
    string StartTime,   // HH:mm
    string EndTime,     // HH:mm
    string Title,
    bool IsCancelled
);

public record GetUserBookingsQuery(string UserId) : IRequest<List<BookingSummaryDto>>;
