using MeetSpase.Domain.Entities;
using MeetSpase.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MeetSpase.Infrastructure.Repositories;

/// <summary>
/// Retained for potential future queries. For authentication use UserManager&lt;ApplicationUser&gt; directly.
/// </summary>
public class UserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ApplicationUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        => await _context.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

    public async Task<ApplicationUser?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
        => await _context.Users.FindAsync(new object[] { id }, cancellationToken);
}
