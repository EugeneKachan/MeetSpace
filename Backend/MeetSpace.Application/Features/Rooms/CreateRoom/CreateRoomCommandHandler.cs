using MediatR;
using MeetSpace.Application.Interfaces;
using MeetSpace.Domain.Entities;

namespace MeetSpace.Application.Features.Rooms.CreateRoom;

public class CreateRoomCommandHandler : IRequestHandler<CreateRoomCommand, Guid>
{
    private readonly IOfficeRepository _officeRepo;
    private readonly IRoomRepository _roomRepo;

    public CreateRoomCommandHandler(IOfficeRepository officeRepo, IRoomRepository roomRepo)
    {
        _officeRepo = officeRepo;
        _roomRepo = roomRepo;
    }

    public async Task<Guid> Handle(CreateRoomCommand command, CancellationToken ct)
    {
        var office = await _officeRepo.GetByIdAsync(command.OfficeId, ct)
            ?? throw new KeyNotFoundException($"Office '{command.OfficeId}' not found.");

        var room = new Room
        {
            Id = Guid.NewGuid(),
            OfficeId = office.Id,
            Name = command.Name,
            Capacity = command.Capacity,
            Description = command.Description,
            IsActive = true
        };

        await _roomRepo.AddAsync(room, ct);
        return room.Id;
    }
}
