using MeetSpace.Domain.Entities;
using MeetSpace.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace MeetSpace.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IOpenIddictApplicationManager applicationManager,
        ILogger logger,
        string? adminPassword = null)
    {
        // --- Seed Identity roles ---
        string[] roles = [UserRoles.Employee, UserRoles.OfficeManager, UserRoles.Admin];
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
                logger.LogInformation("Role '{Role}' created.", role);
            }
        }

        // --- Seed default admin user ---
        const string adminEmail = "admin@MeetSpace.com";
        if (string.IsNullOrEmpty(adminPassword))
        {
            logger.LogInformation("Admin seed skipped: AdminSeed:Password is not configured.");
        }
        else if (await userManager.FindByEmailAsync(adminEmail) is null)
        {
            var admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "System",
                LastName = "Admin",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(admin, adminPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, UserRoles.Admin);
                logger.LogInformation("Default admin seeded: {Email}", adminEmail);
            }
            else
            {
                logger.LogError("Failed to seed admin: {Errors}",
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }

        // --- Seed OpenIddict application (Angular SPA client) ---
        if (await applicationManager.FindByClientIdAsync("MeetSpace-angular") is null)
        {
            await applicationManager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = "MeetSpace-angular",
                ClientType = ClientTypes.Public,
                DisplayName = "MeetSpace Angular App",
                Permissions =
                {
                    Permissions.Endpoints.Token,
                    Permissions.GrantTypes.Password,
                    Permissions.GrantTypes.RefreshToken,
                    Permissions.Scopes.Email,
                    Permissions.Scopes.Profile,
                    Permissions.Scopes.Roles,
                    Permissions.Prefixes.Scope + "api"
                }
            });
            logger.LogInformation("OpenIddict application 'MeetSpace-angular' registered.");
        }
    }
}
