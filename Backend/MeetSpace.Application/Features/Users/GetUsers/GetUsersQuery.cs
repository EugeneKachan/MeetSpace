using MediatR;

namespace MeetSpace.Application.Features.Users.GetUsers;

/// <summary>
/// Query to retrieve all users. Admin only (FR-14).
/// </summary>
public record GetUsersQuery : IRequest<IReadOnlyList<UserDto>>;

public record UserDto(
    string Id,
    string FirstName,
    string LastName,
    string Email,
    string Role,
    bool IsActive,
    DateTime CreatedAt
);
