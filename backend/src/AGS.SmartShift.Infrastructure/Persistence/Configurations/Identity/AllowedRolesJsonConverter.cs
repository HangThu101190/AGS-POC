using System.Text.Json;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace AGS.SmartShift.Infrastructure.Persistence.Configurations.Identity;

internal static class AllowedRolesJsonConverter
{
    private static readonly JsonSerializerOptions Options = new();

    public static ValueConverter<List<string>, string> Create() =>
        new(
            v => JsonSerializer.Serialize(v, Options),
            v => Deserialize(v));

    public static ValueComparer<List<string>> CreateComparer() =>
        new(
            (a, b) => a.SequenceEqual(b),
            v => v.Aggregate(0, (hash, role) => HashCode.Combine(hash, role.GetHashCode(StringComparison.OrdinalIgnoreCase))),
            v => v.ToList());

    private static List<string> Deserialize(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new List<string>();
        }

        var trimmed = json.Trim();
        if (trimmed is "\"\"" or "null")
        {
            return new List<string>();
        }

        try
        {
            return JsonSerializer.Deserialize<List<string>>(trimmed, Options) ?? new List<string>();
        }
        catch (JsonException)
        {
            return new List<string>();
        }
    }
}
