namespace VinhKhanhMaui.Services;

public class LocationChangedEventArgs : EventArgs
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double Accuracy { get; set; }
}

public class LocationTrackingService
{
    private CancellationTokenSource? _cts;
    public event EventHandler<LocationChangedEventArgs>? LocationChanged;
    public event EventHandler<string>? StatusChanged;
    public bool IsTracking { get; private set; }

    public async Task<bool> StartAsync()
    {
        try
        {
            var status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted)
            {
                StatusChanged?.Invoke(this, "⚠ Cần cấp quyền vị trí trong Settings");
                return false;
            }

            _cts = new CancellationTokenSource();
            IsTracking = true;

            _ = Task.Run(async () =>
            {
                while (!_cts.Token.IsCancellationRequested)
                {
                    try
                    {
                        var request = new GeolocationRequest(
                            GeolocationAccuracy.Best, TimeSpan.FromSeconds(5));
                        var loc = await Geolocation.Default.GetLocationAsync(
                            request, _cts.Token);

                        if (loc != null)
                        {
                            LocationChanged?.Invoke(this, new LocationChangedEventArgs
                            {
                                Latitude = loc.Latitude,
                                Longitude = loc.Longitude,
                                Accuracy = loc.Accuracy ?? 0
                            });
                        }
                    }
                    catch { }

                    await Task.Delay(2000, _cts.Token);
                }
            });

            StatusChanged?.Invoke(this, "📡 GPS đang chạy");
            return true;
        }
        catch (Exception ex)
        {
            StatusChanged?.Invoke(this, "❌ " + ex.Message);
            return false;
        }
    }

    public void Stop()
    {
        _cts?.Cancel();
        IsTracking = false;
    }
}