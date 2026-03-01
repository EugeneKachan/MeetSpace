namespace MeetSpace.Domain.Entities;

/// <summary>
/// Represents a bookable meeting room belonging to an office (FR-6, FR-7, FR-8).
/// </summary>
public class Room
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public Guid OfficeId { get; set; }
    public Office Office { get; set; } = null!;
}
