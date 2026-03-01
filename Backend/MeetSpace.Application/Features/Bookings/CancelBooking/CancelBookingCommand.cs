using MediatR;

namespace MeetSpace.Application.Features.Bookings.CancelBooking;

public record CancelBookingCommand(
    Guid Id,
    string RequestingUserId,
    bool IsManagerOrAdmin
) : IRequest<Unit>;
