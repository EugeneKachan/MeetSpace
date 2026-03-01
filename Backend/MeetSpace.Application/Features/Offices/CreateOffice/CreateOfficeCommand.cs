using MediatR;

namespace MeetSpace.Application.Features.Offices.CreateOffice;

/// <summary>DTO for a room when creating an office.</summary>
public record CreateRoomRequest(
    string Name,
    int Capacity,
    string Description
);

/// <summary>Creates a new office, optionally with initial rooms (FR-3).</summary>
public record CreateOfficeCommand(
    string Name,
    string Address,
    List<CreateRoomRequest> Rooms
) : IRequest<Guid>;
