using System.Globalization;
using System.Text.RegularExpressions;

namespace VinhKhanhAdmin.Services;

public record GmapsParseResult(double Lat, double Lng, string Method);

/// <summary>
/// Parses various Google Maps URL formats into (lat, lng) coordinates.
/// Supports: @lat,lng,zoom | ?query=lat,lng | iframe !2d!3d | /place/.../lat,lng | ll= | raw "lat, lng".
/// Short links (maps.app.goo.gl) require HTTP redirect follow — caller's responsibility.
/// </summary>
public static class GmapsParser
{
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    public static GmapsParseResult? Parse(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return null;
        var s = input.Trim();

        // 1. Raw "lat, lng"
        var m = Regex.Match(s, @"^\s*(-?\d+(?:\.\d+)?)\s*,\s*(-?\d+(?:\.\d+)?)\s*$");
        if (m.Success && TryPoint(m, 1, 2, out var r)) return new(r.Lat, r.Lng, "tọa độ trực tiếp");

        // 2. @lat,lng,zoom (most common place URL)
        m = Regex.Match(s, @"@(-?\d+\.\d+),(-?\d+\.\d+)(?:,\d+(?:\.\d+)?z)?");
        if (m.Success && TryPoint(m, 1, 2, out r)) return new(r.Lat, r.Lng, "URL dạng @lat,lng");

        // 3. ?query=lat,lng
        m = Regex.Match(s, @"[?&]query=(-?\d+\.\d+)[,%2C]+(-?\d+\.\d+)");
        if (m.Success && TryPoint(m, 1, 2, out r)) return new(r.Lat, r.Lng, "param query");

        // 4. Iframe embed: !3d=lat, !2d=lng
        var d3 = Regex.Match(s, @"!3d(-?\d+\.\d+)");
        var d2 = Regex.Match(s, @"!2d(-?\d+\.\d+)");
        if (d3.Success && d2.Success &&
            double.TryParse(d3.Groups[1].Value, NumberStyles.Float, Inv, out var lat) &&
            double.TryParse(d2.Groups[1].Value, NumberStyles.Float, Inv, out var lng) &&
            Valid(lat, lng))
            return new(lat, lng, "iframe embed");

        // 5. ll=lat,lng
        m = Regex.Match(s, @"[?&]ll=(-?\d+\.\d+),(-?\d+\.\d+)");
        if (m.Success && TryPoint(m, 1, 2, out r)) return new(r.Lat, r.Lng, "param ll");

        // 6. /path/.../lat,lng
        m = Regex.Match(s, @"\/(-?\d+\.\d+),(-?\d+\.\d+)(?:\/|$|\?)");
        if (m.Success && TryPoint(m, 1, 2, out r)) return new(r.Lat, r.Lng, "URL path");

        return null;
    }

    private static bool TryPoint(Match m, int g1, int g2, out (double Lat, double Lng) r)
    {
        r = default;
        if (double.TryParse(m.Groups[g1].Value, NumberStyles.Float, Inv, out var lat) &&
            double.TryParse(m.Groups[g2].Value, NumberStyles.Float, Inv, out var lng) &&
            Valid(lat, lng))
        {
            r = (lat, lng);
            return true;
        }
        return false;
    }

    public static bool Valid(double lat, double lng) =>
        !double.IsNaN(lat) && !double.IsNaN(lng) &&
        lat is >= -90 and <= 90 && lng is >= -180 and <= 180;
}
