namespace MeetSpace.Domain.Entities;

/// <summary>
/// Represents a physical office location (FR-3, FR-4, FR-5).
/// </summary>
public class Office
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public ICollection<Room> Rooms { get; set; } = new List<Room>();
    public ICollection<OfficeAssignment> Assignments { get; set; } = new List<OfficeAssignment>();
}
