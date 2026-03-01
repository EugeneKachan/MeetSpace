using MeetSpace.Application.Interfaces;
using MeetSpace.Domain.Entities;
using MeetSpace.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MeetSpace.Infrastructure.Repositories;

public class OfficeRepository : IOfficeRepository
{
    private readonly AppDbContext _db;

    public OfficeRepository(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<Office>> GetAllAsync(CancellationToken ct = default) =>
        await _db.Offices
                 .Include(o => o.Rooms)
                 .Include(o => o.Assignments).ThenInclude(a => a.User)
                 .OrderBy(o => o.Name)
                 .ToListAsync(ct);

    public async Task<IReadOnlyList<Office>> GetByAssignedUserAsync(string userId, CancellationToken ct = default) =>
        await _db.Offices
                 .Include(o => o.Rooms)
                 .Include(o => o.Assignments).ThenInclude(a => a.User)
                 .Where(o => o.Assignments.Any(a => a.UserId == userId))
                 .OrderBy(o => o.Name)
                 .ToListAsync(ct);

    public async Task<Office?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _db.Offices
                 .Include(o => o.Rooms)
                 .Include(o => o.Assignments).ThenInclude(a => a.User)
                 .FirstOrDefaultAsync(o => o.Id == id, ct);

    public async Task AddAsync(Office office, CancellationToken ct = default)
    {
        await _db.Offices.AddAsync(office, ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Office office, CancellationToken ct = default)
    {
        _db.Offices.Update(office);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<bool> AssignmentExistsAsync(Guid officeId, string userId, CancellationToken ct = default) =>
        await _db.OfficeAssignments.AnyAsync(a => a.OfficeId == officeId && a.UserId == userId, ct);

    public async Task AddAssignmentAsync(OfficeAssignment assignment, CancellationToken ct = default)
    {
        await _db.OfficeAssignments.AddAsync(assignment, ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task RemoveAssignmentAsync(Guid officeId, string userId, CancellationToken ct = default)
    {
        var assignment = await _db.OfficeAssignments
            .FirstOrDefaultAsync(a => a.OfficeId == officeId && a.UserId == userId, ct);
        if (assignment is not null)
        {
            _db.OfficeAssignments.Remove(assignment);
            await _db.SaveChangesAsync(ct);
        }
    }
}
