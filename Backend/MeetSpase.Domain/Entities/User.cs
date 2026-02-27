using Microsoft.AspNetCore.Identity;

namespace MeetSpase.Domain.Entities;

/// <summary>
/// Application user — extends ASP.NET Core IdentityUser with project-specific fields.
/// </summary>
public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
