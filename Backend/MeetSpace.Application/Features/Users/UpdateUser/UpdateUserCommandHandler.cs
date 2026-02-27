using MediatR;
using MeetSpace.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace MeetSpace.Application.Features.Users.UpdateUser;

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, UpdateUserResponse>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UpdateUserCommandHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<UpdateUserResponse> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.Id)
            ?? throw new KeyNotFoundException($"User with ID '{request.Id}' was not found.");

        // Check if new email is already taken by a different user
        if (!string.Equals(user.Email, request.Email, StringComparison.OrdinalIgnoreCase))
        {
            var existing = await _userManager.FindByEmailAsync(request.Email);
            if (existing is not null)
                throw new InvalidOperationException($"Email '{request.Email}' is already in use.");
        }

        // Update fields
        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.Email = request.Email;
        user.UserName = request.Email;
        user.NormalizedEmail = request.Email.ToUpperInvariant();
        user.NormalizedUserName = request.Email.ToUpperInvariant();
        user.IsActive = request.IsActive;

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            var errors = string.Join("; ", updateResult.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to update user: {errors}");
        }

        // Update role if changed
        var currentRoles = await _userManager.GetRolesAsync(user);
        var currentRole = currentRoles.FirstOrDefault();

        if (currentRole != request.Role)
        {
            if (currentRole is not null)
                await _userManager.RemoveFromRoleAsync(user, currentRole);

            var roleResult = await _userManager.AddToRoleAsync(user, request.Role);
            if (!roleResult.Succeeded)
            {
                var errors = string.Join("; ", roleResult.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to assign role: {errors}");
            }
        }

        return new UpdateUserResponse(
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email!,
            request.Role,
            user.IsActive,
            user.CreatedAt
        );
    }
}
