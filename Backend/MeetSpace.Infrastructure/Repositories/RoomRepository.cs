using MeetSpace.Application.Interfaces;
using MeetSpace.Domain.Entities;
using MeetSpace.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MeetSpace.Infrastructure.Repositories;

public class RoomRepository : IRoomRepository
{
    private readonly AppDbContext _db;

    public RoomRepository(AppDbContext db) => _db = db;

    public async Task<Room?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _db.Rooms.FirstOrDefaultAsync(r => r.Id == id, ct);

    public async Task<IReadOnlyList<Room>> GetActiveByOfficeAsync(
        Guid officeId, int? minCapacity, CancellationToken ct = default)
    {
        var query = _db.Rooms
            .Where(r => r.OfficeId == officeId && r.IsActive);

        if (minCapacity.HasValue)
            query = query.Where(r => r.Capacity >= minCapacity.Value);

        return await query.OrderBy(r => r.Name).ToListAsync(ct);
    }

    public async Task AddAsync(Room room, CancellationToken ct = default)
    {
        await _db.Rooms.AddAsync(room, ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Room room, CancellationToken ct = default)
    {
        _db.Rooms.Update(room);
        await _db.SaveChangesAsync(ct);
    }
}
