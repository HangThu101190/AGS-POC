namespace AGS.SmartShift.Domain.Common;

/// <summary>Server-side table query — paging, sort, column filters (field → contains text).</summary>
public sealed class TableListCriteria
{
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 200;

    public int Page { get; init; }
    public int PageSize { get; init; } = DefaultPageSize;
    public string? SortBy { get; init; }
    public bool SortDescending { get; init; }
    public IReadOnlyDictionary<string, string>? Filters { get; init; }

    /// <summary>When set, list queries are restricted to this department (Sup/Staff scope).</summary>
    public Guid? ScopeDepartmentId { get; init; }

    public int Skip => Page * EffectivePageSize;
    public int Take => EffectivePageSize;

    public int EffectivePageSize =>
        PageSize < 1 ? DefaultPageSize : Math.Min(PageSize, MaxPageSize);
}
