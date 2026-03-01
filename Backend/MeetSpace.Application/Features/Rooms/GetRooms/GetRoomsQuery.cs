using MediatR;

namespace MeetSpace.Application.Features.Rooms.GetRooms;

/// <summary>
/// Returns active rooms for the specified office with optional filters (FR-10).
/// </summary>
public record GetRoomsQuery(
    Guid OfficeId,
    int? MinCapacity = null,
    DateOnly? Date = null,
    TimeOnly? StartTime = null,
    TimeOnly? EndTime = null
) : IRequest<IReadOnlyList<RoomListDto>>;
