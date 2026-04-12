using System;
using System.Collections.Generic;
using System.Linq;
using VinhKhanhGuide.Models;

namespace VinhKhanhGuide.Services
{
    public class GeofenceEnteredEventArgs : EventArgs
    {
        public PointOfInterest Poi { get; set; }
        public double DistanceMeters { get; set; }
    }

    /// <summary>
    /// Compares the current GPS position against every POI and fires
    /// <see cref="GeofenceEntered"/> the first time the user crosses
    /// a POI's radius. A per-POI cooldown prevents re-firing when the
    /// user lingers at the edge of a zone.
    /// </summary>
    public class GeofenceService
    {
        private readonly List<PointOfInterest> _pois;
        private readonly Dictionary<int, DateTime> _lastFiredUtc = new Dictionary<int, DateTime>();
        private readonly HashSet<int> _currentlyInside = new HashSet<int>();

        /// <summary>How long a POI stays muted after it has been narrated.</summary>
        public TimeSpan Cooldown { get; set; } = TimeSpan.FromMinutes(5);

        public event EventHandler<GeofenceEnteredEventArgs> GeofenceEntered;

        public GeofenceService(IEnumerable<PointOfInterest> pois)
        {
            _pois = pois?.ToList() ?? new List<PointOfInterest>();
        }

        /// <summary>Feed a new user location into the engine.</summary>
        public void UpdateLocation(double userLat, double userLon)
        {
            // Sort by priority so that when two POIs fire in the same tick
            // the higher-priority one wins.
            var ordered = _pois.OrderByDescending(p => p.Priority);

            foreach (var poi in ordered)
            {
                double d = HaversineCalculator.DistanceMeters(
                    userLat, userLon, poi.Latitude, poi.Longitude);

                bool inside = d <= poi.RadiusMeters;

                if (inside)
                {
                    // Only fire on the *entry* transition — not every tick.
                    if (!_currentlyInside.Contains(poi.Id) && NotOnCooldown(poi.Id))
                    {
                        _currentlyInside.Add(poi.Id);
                        _lastFiredUtc[poi.Id] = DateTime.UtcNow;
                        GeofenceEntered?.Invoke(this, new GeofenceEnteredEventArgs
                        {
                            Poi = poi,
                            DistanceMeters = d
                        });
                        return; // one narration per tick
                    }
                }
                else
                {
                    _currentlyInside.Remove(poi.Id);
                }
            }
        }

        private bool NotOnCooldown(int poiId)
        {
            if (!_lastFiredUtc.TryGetValue(poiId, out var last)) return true;
            return DateTime.UtcNow - last >= Cooldown;
        }

        public void Reset()
        {
            _lastFiredUtc.Clear();
            _currentlyInside.Clear();
        }
    }
}
