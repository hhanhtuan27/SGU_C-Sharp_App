namespace VinhKhanhGuide.Models
{
    /// <summary>
    /// A single food-street location that the app can narrate.
    /// Mirrors the dbo.PointsOfInterest table.
    /// </summary>
    public class PointOfInterest
    {
        public int    Id            { get; set; }
        public string Name          { get; set; }
        public PoiCategory Category { get; set; }
        public double Latitude      { get; set; }
        public double Longitude     { get; set; }
        public double RadiusMeters  { get; set; } = 30;
        public int    Priority      { get; set; } = 1;
        public string DescriptionVi { get; set; }
        public string DescriptionEn { get; set; }
        public bool   IsActive      { get; set; } = true;

        /// <summary>Pick description in the requested language, fall back gracefully.</summary>
        public string GetDescription(string language)
        {
            bool vn = string.Equals(language, "VN", System.StringComparison.OrdinalIgnoreCase);
            var primary   = vn ? DescriptionVi : DescriptionEn;
            var secondary = vn ? DescriptionEn : DescriptionVi;
            return !string.IsNullOrWhiteSpace(primary) ? primary : secondary ?? Name;
        }

        public override string ToString() => $"{Name} ({Category})";
    }
}
