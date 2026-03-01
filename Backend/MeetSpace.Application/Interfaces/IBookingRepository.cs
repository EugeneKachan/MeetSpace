using MeetSpace.Domain.Entities;

namespace MeetSpace.Application.Interfaces;

public interface IBookingRepository
{
    /// <summary>
    /// Returns true if there is a non-cancelled booking for the same room
    /// that overlaps [startUtc, endUtc).
    /// </summary>
    Task<bool> HasConflictingAsync(Guid roomId, DateTime startUtc, DateTime endUtc, CancellationToken ct = default);

    Task AddAsync(Booking booking, CancellationToken ct = default);
    Task<Booking?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task UpdateAsync(Booking booking, CancellationToken ct = default);

    /// <summary>Returns all non-cancelled bookings belonging to the given user, newest first.</summary>
    Task<List<Booking>> GetByUserIdAsync(string userId, CancellationToken ct = default);

    /// <summary>
    /// Returns all non-cancelled bookings for every room in the specified office on the given date (UTC).
    /// Used to determine room availability when filtering by date/time.
    /// </summary>
    Task<IReadOnlyList<Booking>> GetByOfficeAndDateAsync(Guid officeId, DateOnly date, CancellationToken ct = default);
}
