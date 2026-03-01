namespace MeetSpace.Application.Common;

/// <summary>
/// Generic wrapper for paginated API responses.
/// </summary>
public record PagedResult<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    int Page,
    int PageSize
);
