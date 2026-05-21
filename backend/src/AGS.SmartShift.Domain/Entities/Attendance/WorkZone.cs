using System.Text.Json;
using AGS.SmartShift.Domain.Common;

namespace AGS.SmartShift.Domain.Entities.Attendance;

/// <summary>Versioned geofence polygon for a site (CXR work area).</summary>
public sealed class WorkZone : AuditableEntity<Guid>
{
    public Guid SiteId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public int Version { get; private set; }
    public bool IsActive { get; private set; }
    public string PolygonJson { get; private set; } = "[]";

    private WorkZone()
    {
    }

    public static WorkZone Create(
        Guid id,
        Guid siteId,
        string name,
        int version,
        IReadOnlyList<(double Lat, double Lng)> polygon,
        DateTime utcNow)
    {
        var zone = new WorkZone
        {
            Id = id,
            SiteId = siteId,
            Name = name.Trim(),
            Version = version,
            IsActive = true,
            PolygonJson = SerializePolygon(polygon),
        };
        zone.MarkCreated(utcNow);
        return zone;
    }

    public void UpdatePolygon(IReadOnlyList<(double Lat, double Lng)> polygon, DateTime utcNow)
    {
        PolygonJson = SerializePolygon(polygon);
        Version += 1;
        MarkUpdated(utcNow);
    }

    public IReadOnlyList<(double Lat, double Lng)> GetPolygon() => ParsePolygon(PolygonJson);

    public static string SerializePolygon(IReadOnlyList<(double Lat, double Lng)> ring)
    {
        if (ring.Count < 3)
        {
            throw new DomainException("invalid_polygon", "Work zone polygon requires at least 3 vertices.");
        }

        var arr = ring.Select(p => new[] { p.Lat, p.Lng }).ToArray();
        return JsonSerializer.Serialize(arr);
    }

    public static IReadOnlyList<(double Lat, double Lng)> ParsePolygon(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return Array.Empty<(double, double)>();
        }

        try
        {
            var parsed = JsonSerializer.Deserialize<double[][]>(json);
            if (parsed is null || parsed.Length < 3)
            {
                return Array.Empty<(double, double)>();
            }

            var ring = new List<(double Lat, double Lng)>(parsed.Length);
            foreach (var pair in parsed)
            {
                if (pair.Length < 2)
                {
                    return Array.Empty<(double, double)>();
                }

                ring.Add((pair[0], pair[1]));
            }

            return ring;
        }
        catch (JsonException)
        {
            return Array.Empty<(double, double)>();
        }
    }

    /// <summary>Default CXR polygon — mirrors prototype / FE `checkinZone.ts`.</summary>
    public static IReadOnlyList<(double Lat, double Lng)> CxrDefaultPolygon() =>
    [
        (12.0148, 109.2175),
        (11.9953, 109.2112),
        (11.9918, 109.2137),
        (11.9856, 109.2122),
        (11.9837, 109.2142),
        (11.9801, 109.2161),
        (11.9798, 109.2189),
        (12.0091, 109.2291),
        (12.0148, 109.2176),
    ];
}
