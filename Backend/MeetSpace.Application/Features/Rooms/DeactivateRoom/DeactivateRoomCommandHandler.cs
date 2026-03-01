using MediatR;
using MeetSpace.Application.Interfaces;

namespace MeetSpace.Application.Features.Rooms.DeactivateRoom;

public class DeactivateRoomCommandHandler : IRequestHandler<DeactivateRoomCommand>
{
    private readonly IRoomRepository _repo;

    public DeactivateRoomCommandHandler(IRoomRepository repo) => _repo = repo;

    public async Task Handle(DeactivateRoomCommand command, CancellationToken ct)
    {
        var room = await _repo.GetByIdAsync(command.Id, ct)
            ?? throw new KeyNotFoundException($"Room '{command.Id}' not found.");

        room.IsActive = false;

        await _repo.UpdateAsync(room, ct);
    }
}
