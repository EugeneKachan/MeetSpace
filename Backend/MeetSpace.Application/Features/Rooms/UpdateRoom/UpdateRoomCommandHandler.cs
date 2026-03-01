using MediatR;
using MeetSpace.Application.Interfaces;

namespace MeetSpace.Application.Features.Rooms.UpdateRoom;

public class UpdateRoomCommandHandler : IRequestHandler<UpdateRoomCommand>
{
    private readonly IRoomRepository _repo;

    public UpdateRoomCommandHandler(IRoomRepository repo) => _repo = repo;

    public async Task Handle(UpdateRoomCommand command, CancellationToken ct)
    {
        var room = await _repo.GetByIdAsync(command.Id, ct)
            ?? throw new KeyNotFoundException($"Room '{command.Id}' not found.");

        room.Name = command.Name;
        room.Capacity = command.Capacity;
        room.Description = command.Description;

        await _repo.UpdateAsync(room, ct);
    }
}
