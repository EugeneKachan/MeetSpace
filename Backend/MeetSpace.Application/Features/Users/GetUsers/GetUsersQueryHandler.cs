using MediatR;
using MeetSpace.Application.Common;
using MeetSpace.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace MeetSpace.Application.Features.Users.GetUsers;

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, PagedResult<UserDto>>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public GetUsersQueryHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<PagedResult<UserDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var query = _userManager.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(u =>
                u.FirstName.ToLower().Contains(search) ||
                u.LastName.ToLower().Contains(search) ||
                (u.Email != null && u.Email.ToLower().Contains(search)));
        }

        // Materialise before async role lookup
        var allUsers = query.ToList();

        var dtos = new List<UserDto>(allUsers.Count);
        foreach (var user in allUsers)
        {
            var roles = await _userManager.GetRolesAsync(user);
            dtos.Add(new UserDto(
                user.Id,
                user.FirstName,
                user.LastName,
                user.Email ?? string.Empty,
                roles.FirstOrDefault() ?? string.Empty,
                user.IsActive,
                user.CreatedAt
            ));
        }

        // Sort in memory (roles are only available after the loop above)
        dtos = (request.SortBy?.ToLower(), request.SortDir?.ToLower()) switch
        {
            ("firstname",  "desc") => dtos.OrderByDescending(u => u.FirstName).ToList(),
            ("firstname",  _)     => dtos.OrderBy(u => u.FirstName).ToList(),
            ("email",      "desc") => dtos.OrderByDescending(u => u.Email).ToList(),
            ("email",      _)     => dtos.OrderBy(u => u.Email).ToList(),
            ("role",       "desc") => dtos.OrderByDescending(u => u.Role).ToList(),
            ("role",       _)     => dtos.OrderBy(u => u.Role).ToList(),
            ("status",     "desc") => dtos.OrderByDescending(u => u.IsActive).ToList(),
            ("status",     _)     => dtos.OrderBy(u => u.IsActive).ToList(),
            ("createdat",  "desc") => dtos.OrderByDescending(u => u.CreatedAt).ToList(),
            ("createdat",  _)     => dtos.OrderBy(u => u.CreatedAt).ToList(),
            (_,            "desc") => dtos.OrderByDescending(u => u.LastName).ThenByDescending(u => u.FirstName).ToList(),
            _                     => dtos.OrderBy(u => u.LastName).ThenBy(u => u.FirstName).ToList(),
        };

        var totalCount = dtos.Count;
        var paged = dtos
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        return new PagedResult<UserDto>(paged.AsReadOnly(), totalCount, request.Page, request.PageSize);
    }
}
