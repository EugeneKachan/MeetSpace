using MediatR;
using MeetSpace.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace MeetSpace.Application.Features.Users.GetUsers;

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, IReadOnlyList<UserDto>>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public GetUsersQueryHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<IReadOnlyList<UserDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var users = _userManager.Users
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .ToList();

        var result = new List<UserDto>(users.Count);
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            result.Add(new UserDto(
                user.Id,
                user.FirstName,
                user.LastName,
                user.Email ?? string.Empty,
                roles.FirstOrDefault() ?? string.Empty,
                user.IsActive,
                user.CreatedAt
            ));
        }

        return result.AsReadOnly();
    }
}
