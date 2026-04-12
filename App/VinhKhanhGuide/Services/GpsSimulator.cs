using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace VinhKhanhGuide.Services
{
    public class LocationChangedEventArgs : EventArgs
    {
        public double Latitude  { get; set; }
        public double Longitude { get; set; }
    }

    /// <summary>
    /// Fake GPS for the demo. You can either:
    ///   • set a single coordinate via <see cref="SetLocation"/>, or
    ///   • call <see cref="StartRoute"/> with a list of waypoints and let
    ///     it walk through them on a timer so the app can demonstrate
    ///     geofencing without leaving the classroom.
    /// </summary>
    public class GpsSimulator
    {
        private readonly Timer _routeTimer = new Timer { Interval = 1500 };
        private List<(double lat, double lon)> _route;
        private int _routeIndex;

        public double CurrentLatitude  { get; private set; } = 10.760719;
        public double CurrentLongitude { get; private set; } = 106.703297;

        public event EventHandler<LocationChangedEventArgs> LocationChanged;

        public GpsSimulator()
        {
            _routeTimer.Tick += RouteTimer_Tick;
        }

        public void SetLocation(double lat, double lon)
        {
            CurrentLatitude  = lat;
            CurrentLongitude = lon;
            LocationChanged?.Invoke(this,
                new LocationChangedEventArgs { Latitude = lat, Longitude = lon });
        }

        public void StartRoute(IEnumerable<(double lat, double lon)> waypoints, int intervalMs = 1500)
        {
            _route = new List<(double, double)>(waypoints);
            _routeIndex = 0;
            _routeTimer.Interval = intervalMs;
            _routeTimer.Start();
        }

        public void StopRoute() => _routeTimer.Stop();

        private void RouteTimer_Tick(object sender, EventArgs e)
        {
            if (_route == null || _routeIndex >= _route.Count)
            {
                _routeTimer.Stop();
                return;
            }
            var p = _route[_routeIndex++];
            SetLocation(p.lat, p.lon);
        }
    }
}
