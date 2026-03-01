using MeetSpace.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MeetSpace.Infrastructure.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Office> Offices => Set<Office>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<OfficeAssignment> OfficeAssignments => Set<OfficeAssignment>();
    public DbSet<Booking> Bookings => Set<Booking>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
        });

        modelBuilder.Entity<Office>(entity =>
        {
            entity.HasKey(o => o.Id);
            entity.Property(o => o.Name).IsRequired().HasMaxLength(200);
            entity.Property(o => o.Address).IsRequired().HasMaxLength(500);
        });

        modelBuilder.Entity<Room>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Name).IsRequired().HasMaxLength(200);
            entity.Property(r => r.Description).HasMaxLength(1000);
            entity.HasOne(r => r.Office)
                  .WithMany(o => o.Rooms)
                  .HasForeignKey(r => r.OfficeId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OfficeAssignment>(entity =>
        {
            entity.HasKey(a => new { a.OfficeId, a.UserId });
            entity.HasOne(a => a.Office)
                  .WithMany(o => o.Assignments)
                  .HasForeignKey(a => a.OfficeId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(a => a.User)
                  .WithMany()
                  .HasForeignKey(a => a.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(b => b.Id);
            entity.Property(b => b.Title).HasMaxLength(200);
            entity.Property(b => b.CreatedAt).IsRequired();
            entity.HasIndex(b => new { b.RoomId, b.StartTime, b.EndTime })
                  .HasDatabaseName("IX_Bookings_Room_Start_End");
            entity.HasOne(b => b.Room)
                  .WithMany()
                  .HasForeignKey(b => b.RoomId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
