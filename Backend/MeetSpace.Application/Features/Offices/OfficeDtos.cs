namespace MeetSpace.Application.Features.Offices;

/// <summary>Lightweight office summary returned in GET /api/offices/active (FR-9).</summary>
public record ActiveOfficeDto(Guid Id, string Name, string Address);

/// <summary>Returned in GET /api/offices responses.</summary>
public record RoomDto(
    Guid Id,
    string Name,
    int Capacity,
    string Description,
    bool IsActive
);

/// <summary>Summary of an assigned office manager.</summary>
public record ManagerDto(
    string Id,
    string FirstName,
    string LastName,
    string Email
);

/// <summary>Returned in GET /api/offices responses.</summary>
public record OfficeDto(
    Guid Id,
    string Name,
    string Address,
    bool IsActive,
    IReadOnlyList<RoomDto> Rooms,
    IReadOnlyList<ManagerDto> Managers
);
