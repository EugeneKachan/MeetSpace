namespace MeetSpace.Domain.Entities;

/// <summary>
/// Join entity — assigns an OfficeManager user to an Office.
/// </summary>
public class OfficeAssignment
{
    public Guid OfficeId { get; set; }
    public Office Office { get; set; } = null!;

    /// <summary>FK to AspNetUsers (ApplicationUser.Id — string GUID).</summary>
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;
}
