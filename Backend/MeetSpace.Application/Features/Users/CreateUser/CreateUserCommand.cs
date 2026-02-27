using MediatR;

namespace MeetSpace.Application.Features.Users.CreateUser;

/// <summary>
/// Command to create a new user (Admin only — FR-14).
/// </summary>
public record CreateUserCommand(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string Role,
    bool IsActive = true
) : IRequest<CreateUserResponse>;

/// <summary>
/// Response returned after a successful user creation.
/// </summary>
public record CreateUserResponse(
    string Id,
    string FirstName,
    string LastName,
    string Email,
    string Role,
    bool IsActive,
    DateTime CreatedAt
);
