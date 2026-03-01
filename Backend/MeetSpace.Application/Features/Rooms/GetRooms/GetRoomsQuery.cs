using MediatR;

namespace MeetSpace.Application.Features.Rooms.GetRooms;

/// <summary>
/// Returns active rooms for the specified office with optional filters (FR-10).
/// Date/time availability filtering will be applied once the Booking entity is available (Task 015-016).
/// </summary>
public record GetRoomsQuery(
    Guid OfficeId,
    int? MinCapacity = null,
    DateOnly? Date = null,
    TimeOnly? StartTime = null,
    TimeOnly? EndTime = null
) : IRequest<IReadOnlyList<RoomListDto>>;
