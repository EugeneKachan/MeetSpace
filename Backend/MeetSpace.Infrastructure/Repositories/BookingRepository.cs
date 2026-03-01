using MeetSpace.Application.Interfaces;
using MeetSpace.Domain.Entities;
using MeetSpace.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MeetSpace.Infrastructure.Repositories;

public class BookingRepository : IBookingRepository
{
    private readonly AppDbContext _db;

    public BookingRepository(AppDbContext db) => _db = db;

    public async Task<bool> HasConflictingAsync(Guid roomId, DateTime startUtc, DateTime endUtc, CancellationToken ct = default) =>
        await _db.Bookings.AnyAsync(
            b => b.RoomId == roomId && !b.IsCancelled
              && startUtc < b.EndTime && endUtc > b.StartTime, ct);

    public async Task AddAsync(Booking booking, CancellationToken ct = default)
    {
        await _db.Bookings.AddAsync(booking, ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<Booking?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _db.Bookings.FirstOrDefaultAsync(b => b.Id == id, ct);

    public async Task UpdateAsync(Booking booking, CancellationToken ct = default)
    {
        _db.Bookings.Update(booking);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<List<Booking>> GetByUserIdAsync(string userId, CancellationToken ct = default) =>
        await _db.Bookings
            .Include(b => b.Room)
                .ThenInclude(r => r!.Office)
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.StartTime)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Booking>> GetByOfficeAndDateAsync(
        Guid officeId, DateOnly date, CancellationToken ct = default) =>
        await _db.Bookings
            .Include(b => b.Room)
            .Where(b => b.Room!.OfficeId == officeId
                     && !b.IsCancelled
                     && DateOnly.FromDateTime(b.StartTime) == date)
            .ToListAsync(ct);
}
