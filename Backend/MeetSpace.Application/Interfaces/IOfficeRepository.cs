using MeetSpace.Domain.Entities;

namespace MeetSpace.Application.Interfaces;

public interface IOfficeRepository
{
    Task<IReadOnlyList<Office>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Office>> GetActiveAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Office>> GetByAssignedUserAsync(string userId, CancellationToken ct = default);
    Task<Office?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Office office, CancellationToken ct = default);
    Task UpdateAsync(Office office, CancellationToken ct = default);
    Task<bool> AssignmentExistsAsync(Guid officeId, string userId, CancellationToken ct = default);
    Task AddAssignmentAsync(OfficeAssignment assignment, CancellationToken ct = default);
    Task RemoveAssignmentAsync(Guid officeId, string userId, CancellationToken ct = default);
}
