using VinhKhanhMaui.Models;

namespace VinhKhanhMaui.Services;

public class GeofenceEnteredEventArgs : EventArgs
{
    public PointOfInterest Poi { get; set; } = null!;
    public double Distance { get; set; }
}

/// <summary>
/// Xử lý logic geofence với debounce + cooldown + priority-by-distance.
///  - Debounce 3s: user phải ở trong vùng 3 giây liên tục mới trigger
///  - Cooldown 5 phút: sau khi narrate xong, quán đó không trigger lại trong 5p
///  - Nhiều POI cùng vùng: chọn POI gần nhất
/// </summary>
public class GeofenceService
{
    private readonly List<PointOfInterest> _pois;
    private readonly Dictionary<int, DateTime> _insideSince = new();
    private readonly Dictionary<int, DateTime> _lastFiredUtc = new();
    private readonly HashSet<int> _currentlyInside = new();

    private readonly TimeSpan _debounceTime = TimeSpan.FromSeconds(3);
    private readonly TimeSpan _cooldownTime = TimeSpan.FromMinutes(5);

    public event EventHandler<GeofenceEnteredEventArgs>? GeofenceEntered;

    public GeofenceService(List<PointOfInterest> pois)
    {
        _pois = pois;
    }

    public void UpdateLocation(double userLat, double userLon)
    {
        var now = DateTime.UtcNow;
        PointOfInterest? bestPoi = null;
        double bestDistance = double.MaxValue;

        foreach (var poi in _pois)
        {
            double d = Haversine(userLat, userLon, poi.Latitude, poi.Longitude);
            poi.DistanceMeters = d;
            bool inside = d <= poi.RadiusMeters;

            if (inside)
            {
                if (!_insideSince.ContainsKey(poi.Id))
                    _insideSince[poi.Id] = now;

                if (now - _insideSince[poi.Id] >= _debounceTime)
                {
                    if (_currentlyInside.Contains(poi.Id)) continue;
                    if (_lastFiredUtc.TryGetValue(poi.Id, out var last)
                        && now - last < _cooldownTime) continue;

                    if (d < bestDistance)
                    {
                        bestDistance = d;
                        bestPoi = poi;
                    }
                }
            }
            else
            {
                _insideSince.Remove(poi.Id);
                _currentlyInside.Remove(poi.Id);
            }
        }

        if (bestPoi != null)
        {
            _currentlyInside.Add(bestPoi.Id);
            _lastFiredUtc[bestPoi.Id] = now;
            GeofenceEntered?.Invoke(this, new GeofenceEnteredEventArgs
            {
                Poi = bestPoi,
                Distance = bestDistance
            });
        }
    }

    public static double Haversine(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371000;
        double dLat = (lat2 - lat1) * Math.PI / 180;
        double dLon = (lon2 - lon1) * Math.PI / 180;
        double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                   Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180) *
                   Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }
}