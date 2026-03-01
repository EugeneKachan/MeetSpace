using MediatR;
using MeetSpace.Application.Interfaces;

namespace MeetSpace.Application.Features.Bookings.CancelBooking;

public class CancelBookingCommandHandler : IRequestHandler<CancelBookingCommand, Unit>
{
    private readonly IBookingRepository _bookingRepo;

    public CancelBookingCommandHandler(IBookingRepository bookingRepo) =>
        _bookingRepo = bookingRepo;

    public async Task<Unit> Handle(CancelBookingCommand cmd, CancellationToken ct)
    {
        var booking = await _bookingRepo.GetByIdAsync(cmd.Id, ct)
            ?? throw new InvalidOperationException("Booking not found.");

        if (!cmd.IsManagerOrAdmin && booking.UserId != cmd.RequestingUserId)
            throw new InvalidOperationException("Not authorized to cancel this booking.");

        booking.IsCancelled = true;
        await _bookingRepo.UpdateAsync(booking, ct);

        return Unit.Value;
    }
}
