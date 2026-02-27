namespace MeetSpase.API.Authorization;

/// <summary>
/// Authorization policy names for role-based access control (FR-2).
/// Usage: [Authorize(Policy = Policies.AdminOnly)]
/// </summary>
public static class Policies
{
    /// <summary>Any authenticated user (Employee, OfficeManager, Admin).</summary>
    public const string EmployeeOrAbove = "EmployeeOrAbove";

    /// <summary>OfficeManager or Admin only.</summary>
    public const string ManagerOrAbove = "ManagerOrAbove";

    /// <summary>Admin only.</summary>
    public const string AdminOnly = "AdminOnly";
}
