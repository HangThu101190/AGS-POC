namespace AGS.SmartShift.Domain.Services;

/// <summary>Ray-casting point-in-polygon with edge/vertex tolerance (prototype geofence).</summary>
public static class GeofenceService
{
    public const double Epsilon = 1e-9;

    public static bool Contains(double lat, double lng, IReadOnlyList<(double Lat, double Lng)> ring)
    {
        if (ring.Count < 3)
        {
            return false;
        }

        if (IsOnBoundary(lat, lng, ring))
        {
            return true;
        }

        var inside = false;
        for (var i = 0; i < ring.Count; i++)
        {
            var j = i == 0 ? ring.Count - 1 : i - 1;
            var (yi, xi) = ring[i];
            var (yj, xj) = ring[j];
            var intersect = yi > lat != yj > lat
                && lng < ((xj - xi) * (lat - yi) / (yj - yi + 0.0)) + xi;
            if (intersect)
            {
                inside = !inside;
            }
        }

        return inside;
    }

    private static bool IsOnBoundary(double lat, double lng, IReadOnlyList<(double Lat, double Lng)> ring)
    {
        for (var i = 0; i < ring.Count; i++)
        {
            var j = i == 0 ? ring.Count - 1 : i - 1;
            var a = ring[i];
            var b = ring[j];
            if (DistanceToVertex(lat, lng, a.Lat, a.Lng) <= Epsilon)
            {
                return true;
            }

            if (DistanceToSegment(lat, lng, a.Lat, a.Lng, b.Lat, b.Lng) <= Epsilon)
            {
                return true;
            }
        }

        return false;
    }

    private static double DistanceToVertex(double lat, double lng, double vLat, double vLng)
    {
        var dLat = lat - vLat;
        var dLng = lng - vLng;
        return Math.Sqrt(dLat * dLat + dLng * dLng);
    }

    private static double DistanceToSegment(
        double lat,
        double lng,
        double aLat,
        double aLng,
        double bLat,
        double bLng)
    {
        var dx = bLng - aLng;
        var dy = bLat - aLat;
        if (Math.Abs(dx) < Epsilon && Math.Abs(dy) < Epsilon)
        {
            return DistanceToVertex(lat, lng, aLat, aLng);
        }

        var t = ((lng - aLng) * dx + (lat - aLat) * dy) / (dx * dx + dy * dy);
        t = Math.Clamp(t, 0, 1);
        var projLat = aLat + t * dy;
        var projLng = aLng + t * dx;
        return DistanceToVertex(lat, lng, projLat, projLng);
    }
}
