using MediatR;

namespace MeetSpace.Application.Features.Rooms.UpdateRoom;

/// <summary>Updates an existing room's details (FR-7).</summary>
public record UpdateRoomCommand(
    Guid Id,
    string Name,
    int Capacity,
    string Description
) : IRequest;
