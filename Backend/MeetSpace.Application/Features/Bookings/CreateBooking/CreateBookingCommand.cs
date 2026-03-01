using MediatR;

namespace MeetSpace.Application.Features.Bookings.CreateBooking;

public record CreateBookingCommand(
    Guid OfficeId,
    Guid RoomId,
    DateOnly Date,
    TimeOnly StartTime,
    TimeOnly EndTime,
    string Title,
    string UserId
) : IRequest<Guid>;
