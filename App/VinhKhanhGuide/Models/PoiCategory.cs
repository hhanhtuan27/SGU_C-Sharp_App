using System.Drawing;

namespace VinhKhanhGuide.Models
{
    /// <summary>
    /// Category of a Point of Interest. Each category has its own accent
    /// color used for both the list badge and the map marker tint.
    /// </summary>
    public enum PoiCategory
    {
        Oc = 0,
        Nuong = 1,
        Lau = 2,
        CaPhe = 3
    }

    public static class PoiCategoryExtensions
    {
        public static PoiCategory Parse(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return PoiCategory.Oc;
            switch (raw.Trim().ToLowerInvariant())
            {
                case "oc":    return PoiCategory.Oc;
                case "nuong": return PoiCategory.Nuong;
                case "lau":   return PoiCategory.Lau;
                case "caphe": return PoiCategory.CaPhe;
                default:       return PoiCategory.Oc;
            }
        }

        public static string DisplayName(this PoiCategory c, string language)
        {
            bool vn = string.Equals(language, "VN", System.StringComparison.OrdinalIgnoreCase);
            switch (c)
            {
                case PoiCategory.Oc:    return vn ? "Ốc"        : "Shellfish";
                case PoiCategory.Nuong: return vn ? "Nướng"     : "Grill";
                case PoiCategory.Lau:   return vn ? "Lẩu"       : "Hot Pot";
                case PoiCategory.CaPhe: return vn ? "Cà phê"    : "Cafe";
                default:                 return c.ToString();
            }
        }

        /// <summary>
        /// Accent color per category — kept saturated so badges pop against
        /// the dark background of the main form.
        /// </summary>
        public static Color AccentColor(this PoiCategory c)
        {
            switch (c)
            {
                case PoiCategory.Oc:    return Color.FromArgb(255, 140,  66); // warm orange
                case PoiCategory.Nuong: return Color.FromArgb(230,  57,  70); // grill red
                case PoiCategory.Lau:   return Color.FromArgb(244, 162,  97); // amber
                case PoiCategory.CaPhe: return Color.FromArgb(111, 168, 220); // cafe blue
                default:                 return Color.Gainsboro;
            }
        }
    }
}
