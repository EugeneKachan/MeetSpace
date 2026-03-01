using MeetSpace.Domain.Entities;

namespace MeetSpace.Application.Interfaces;

public interface IRoomRepository
{
    Task<Room?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Room>> GetActiveByOfficeAsync(Guid officeId, int? minCapacity, CancellationToken ct = default);
    Task AddAsync(Room room, CancellationToken ct = default);
    Task UpdateAsync(Room room, CancellationToken ct = default);
}
