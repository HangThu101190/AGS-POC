namespace AGS.SmartShift.Application.Common.Models;

/// <summary>API response envelope for server-driven tables (AG Grid infinite / lazy load).</summary>
public sealed class PagedList<T>
{
    public required IReadOnlyList<T> Items { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }

    public static PagedList<T> From(
        IReadOnlyList<T> items,
        int page,
        int pageSize,
        int totalCount) =>
        new()
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
        };
}
