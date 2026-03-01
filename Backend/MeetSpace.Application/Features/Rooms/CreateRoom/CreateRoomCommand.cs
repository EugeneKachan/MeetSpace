using MediatR;

namespace MeetSpace.Application.Features.Rooms.CreateRoom;

/// <summary>Adds a new room to an existing office (FR-6).</summary>
public record CreateRoomCommand(
    Guid OfficeId,
    string Name,
    int Capacity,
    string Description
) : IRequest<Guid>;
