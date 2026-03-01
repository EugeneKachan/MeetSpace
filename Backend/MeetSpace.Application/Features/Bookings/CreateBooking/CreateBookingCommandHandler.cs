using MediatR;
using MeetSpace.Application.Interfaces;
using MeetSpace.Domain.Entities;

namespace MeetSpace.Application.Features.Bookings.CreateBooking;

public class CreateBookingCommandHandler : IRequestHandler<CreateBookingCommand, Guid>
{
    private readonly IOfficeRepository _officeRepo;
    private readonly IRoomRepository _roomRepo;
    private readonly IBookingRepository _bookingRepo;

    public CreateBookingCommandHandler(
        IOfficeRepository officeRepo,
        IRoomRepository roomRepo,
        IBookingRepository bookingRepo)
    {
        _officeRepo = officeRepo;
        _roomRepo = roomRepo;
        _bookingRepo = bookingRepo;
    }

    public async Task<Guid> Handle(CreateBookingCommand cmd, CancellationToken ct)
    {
        var office = await _officeRepo.GetByIdAsync(cmd.OfficeId, ct)
            ?? throw new InvalidOperationException("Office not found.");

        if (!office.IsActive)
            throw new InvalidOperationException("Office is inactive.");

        var room = await _roomRepo.GetByIdAsync(cmd.RoomId, ct)
            ?? throw new InvalidOperationException("Room not found.");

        if (!room.IsActive)
            throw new InvalidOperationException("Room is inactive.");

        if (room.OfficeId != cmd.OfficeId)
            throw new InvalidOperationException("Room does not belong to the specified office.");

        var startUtc = cmd.Date.ToDateTime(cmd.StartTime, DateTimeKind.Utc);
        var endUtc = cmd.Date.ToDateTime(cmd.EndTime, DateTimeKind.Utc);

        if (endUtc <= startUtc)
            throw new InvalidOperationException("End time must be after start time.");

        var hasConflict = await _bookingRepo.HasConflictingAsync(cmd.RoomId, startUtc, endUtc, ct);
        if (hasConflict)
            throw new InvalidOperationException("The room is already booked for the selected time.");

        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            RoomId = cmd.RoomId,
            UserId = cmd.UserId,
            StartTime = startUtc,
            EndTime = endUtc,
            Title = cmd.Title,
            CreatedAt = DateTime.UtcNow,
            IsCancelled = false
        };

        await _bookingRepo.AddAsync(booking, ct);
        return booking.Id;
    }
}
