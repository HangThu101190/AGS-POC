using System.Text.Json;
using AGS.SmartShift.Domain.Common;

namespace AGS.SmartShift.Application.Contracts.Common;

/// <summary>Query string contract for list endpoints backing DataTable (page, sort, AG Grid filters JSON).</summary>
public sealed class TableListRequest
{
    public int Page { get; init; }
    public int PageSize { get; init; } = TableListCriteria.DefaultPageSize;
    public string? SortBy { get; init; }
    public string? SortDir { get; init; }

    /// <summary>AG Grid filterModel serialized as JSON (optional).</summary>
    public string? Filters { get; init; }

    public TableListCriteria ToCriteria() =>
        new()
        {
            Page = Math.Max(0, Page),
            PageSize = PageSize,
            SortBy = string.IsNullOrWhiteSpace(SortBy) ? null : SortBy.Trim(),
            SortDescending = string.Equals(SortDir, "desc", StringComparison.OrdinalIgnoreCase),
            Filters = ParseAgGridFilters(Filters),
        };

    private static IReadOnlyDictionary<string, string>? ParseAgGridFilters(string? filtersJson)
    {
        if (string.IsNullOrWhiteSpace(filtersJson))
        {
            return null;
        }

        try
        {
            using var doc = JsonDocument.Parse(filtersJson);
            if (doc.RootElement.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var prop in doc.RootElement.EnumerateObject())
            {
                var text = ExtractFilterText(prop.Value);
                if (!string.IsNullOrWhiteSpace(text))
                {
                    map[prop.Name] = text.Trim();
                }
            }

            return map.Count > 0 ? map : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static string? ExtractFilterText(JsonElement model)
    {
        if (model.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        if (model.TryGetProperty("filter", out var filter) &&
            filter.ValueKind == JsonValueKind.String)
        {
            return filter.GetString();
        }

        if (model.TryGetProperty("values", out var values) &&
            values.ValueKind == JsonValueKind.Array &&
            values.GetArrayLength() > 0)
        {
            return values[0].GetString();
        }

        return null;
    }
}
