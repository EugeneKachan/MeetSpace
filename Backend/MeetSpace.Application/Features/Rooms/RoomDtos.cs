namespace MeetSpace.Application.Features.Rooms;

/// <summary>Returned in GET /api/rooms responses (FR-10).</summary>
public record RoomListDto(
    Guid Id,
    string Name,
    int Capacity,
    string Description
);
