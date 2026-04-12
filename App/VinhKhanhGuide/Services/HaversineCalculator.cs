using System;

namespace VinhKhanhGuide.Services
{
    /// <summary>
    /// Great-circle distance between two GPS coordinates using the
    /// Haversine formula:
    ///
    ///     d = 2r · arcsin( sqrt( sin²(Δφ/2) + cos(φ₁)·cos(φ₂)·sin²(Δλ/2) ) )
    ///
    /// Earth radius is treated as the mean radius (6,371,000 m) which is
    /// accurate to ~0.5% — more than enough for a 30 m geofence.
    /// </summary>
    public static class HaversineCalculator
    {
        private const double EarthRadiusMeters = 6_371_000.0;

        public static double DistanceMeters(
            double lat1, double lon1,
            double lat2, double lon2)
        {
            double phi1   = DegToRad(lat1);
            double phi2   = DegToRad(lat2);
            double dPhi   = DegToRad(lat2 - lat1);
            double dLamb  = DegToRad(lon2 - lon1);

            double a = Math.Sin(dPhi / 2) * Math.Sin(dPhi / 2) +
                       Math.Cos(phi1) * Math.Cos(phi2) *
                       Math.Sin(dLamb / 2) * Math.Sin(dLamb / 2);

            double c = 2 * Math.Asin(Math.Min(1.0, Math.Sqrt(a)));
            return EarthRadiusMeters * c;
        }

        private static double DegToRad(double deg) => deg * Math.PI / 180.0;
    }
}
