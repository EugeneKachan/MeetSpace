using MediatR;

namespace MeetSpace.Application.Features.Rooms.DeactivateRoom;

/// <summary>Soft-deletes a room by setting IsActive = false (FR-8).</summary>
public record DeactivateRoomCommand(Guid Id) : IRequest;
