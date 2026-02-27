using MediatR;

namespace MeetSpace.Application.Features.Users.UpdateUser;

/// <summary>
/// Command to update an existing user (Admin only — FR-15).
/// Password is excluded — a separate change-password flow handles that.
/// </summary>
public record UpdateUserCommand(
    string Id,
    string FirstName,
    string LastName,
    string Email,
    string Role,
    bool IsActive
) : IRequest<UpdateUserResponse>;

public record UpdateUserResponse(
    string Id,
    string FirstName,
    string LastName,
    string Email,
    string Role,
    bool IsActive,
    DateTime CreatedAt
);
