using MediatR;
using MeetSpace.Application.Common;

namespace MeetSpace.Application.Features.Users.GetUsers;

/// <summary>
/// Query to retrieve a paginated, filtered and sorted page of users. Admin only (FR-14).
/// </summary>
public record GetUsersQuery(
    int Page = 1,
    int PageSize = 10,
    string? Search = null,
    string SortBy = "lastName",
    string SortDir = "asc"
) : IRequest<PagedResult<UserDto>>;

public record UserDto(
    string Id,
    string FirstName,
    string LastName,
    string Email,
    string Role,
    bool IsActive,
    DateTime CreatedAt
);

