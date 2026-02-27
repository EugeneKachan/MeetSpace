using MediatR;
using MeetSpace.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace MeetSpace.Application.Features.Users.CreateUser;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, CreateUserResponse>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public CreateUserCommandHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<CreateUserResponse> Handle(CreateUserCommand command, CancellationToken cancellationToken)
    {
        // Check for duplicate email
        var existing = await _userManager.FindByEmailAsync(command.Email);
        if (existing is not null)
            throw new InvalidOperationException($"A user with email '{command.Email}' already exists.");

        var user = new ApplicationUser
        {
            UserName = command.Email,
            Email = command.Email,
            FirstName = command.FirstName,
            LastName = command.LastName,
            IsActive = command.IsActive,
            CreatedAt = DateTime.UtcNow,
            EmailConfirmed = true   // skip email confirmation for admin-created users
        };

        var createResult = await _userManager.CreateAsync(user, command.Password);
        if (!createResult.Succeeded)
        {
            var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to create user: {errors}");
        }

        var roleResult = await _userManager.AddToRoleAsync(user, command.Role);
        if (!roleResult.Succeeded)
        {
            // Roll back — delete the user if role assignment fails
            await _userManager.DeleteAsync(user);
            var errors = string.Join("; ", roleResult.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to assign role: {errors}");
        }

        return new CreateUserResponse(
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email!,
            command.Role,
            user.IsActive,
            user.CreatedAt
        );
    }
}
