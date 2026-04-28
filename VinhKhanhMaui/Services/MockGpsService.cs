namespace VinhKhanhMaui.Services;

/// <summary>
/// Giả lập GPS đi bộ dọc Vĩnh Khánh. Có 2 chế độ:
///   - Auto walk: đi tự động qua 20 quán, mỗi quán dừng 5s
///   - Manual tap: click trên map để teleport
/// </summary>
public class MockGpsService
{
    private CancellationTokenSource? _cts;
    public event EventHandler<LocationChangedEventArgs>? LocationChanged;
    public bool IsRunning { get; private set; }

    // Lộ trình dọc Vĩnh Khánh từ đầu đường đến cuối
    private static readonly (double Lat, double Lon)[] Route = new[]
    {
        (10.761955, 106.702094), // Ốc Phát
        (10.761756, 106.702283), // SINZIEN
        (10.761460, 106.702608), // Lẩu Bò Kỳ Kim
        (10.761403, 106.702705), // Ốc Vũ
        (10.761137, 106.704979), // Ốc Đào 2
        (10.760964, 106.702942), // Ốc Sáu Nở
        (10.760836, 106.703505), // Ốc 662
        (10.760806, 106.704310), // Lẩu Mẹt Nướng
        (10.760778, 106.704739), // Yakiniku
        (10.760728, 106.704679), // BBQ
        (10.760719, 106.703297), // Ốc Oanh (trung tâm)
        (10.760713, 106.704217), // Ốc Hoa
        (10.760669, 106.703673), // Hàu Nướng A Trung
        (10.760597, 106.704455), // Ốc Bụi
        (10.760537, 106.703528), // Tiệm Nướng 10 Năm
        (10.760846, 106.704983), // Link Coffee
        (10.760856, 106.706722), // Lẩu gà lá é
        (10.761201, 106.706134), // Xù Phê
        (10.760451, 106.707005), // Lucky café
    };

    /// <summary>Bắt đầu đi bộ tự động dọc lộ trình.</summary>
    public void StartAutoWalk()
    {
        Stop();
        _cts = new CancellationTokenSource();
        IsRunning = true;

        _ = Task.Run(async () =>
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                for (int i = 0; i < Route.Length; i++)
                {
                    if (_cts.Token.IsCancellationRequested) break;

                    var (lat, lon) = Route[i];

                    // Interpolate 10 bước giữa 2 điểm (giả lập di chuyển mượt)
                    int nextIdx = (i + 1) % Route.Length;
                    var (nextLat, nextLon) = Route[nextIdx];
                    int steps = 10;

                    for (int s = 0; s <= steps; s++)
                    {
                        if (_cts.Token.IsCancellationRequested) break;
                        double t = (double)s / steps;
                        double curLat = lat + (nextLat - lat) * t;
                        double curLon = lon + (nextLon - lon) * t;

                        LocationChanged?.Invoke(this, new LocationChangedEventArgs
                        {
                            Latitude = curLat,
                            Longitude = curLon,
                            Accuracy = 5
                        });

                        await Task.Delay(1000, _cts.Token);
                    }

                    // Dừng 5s ở mỗi điểm (đủ debounce 3s)
                    LocationChanged?.Invoke(this, new LocationChangedEventArgs
                    {
                        Latitude = nextLat,
                        Longitude = nextLon,
                        Accuracy = 5
                    });
                    await Task.Delay(5000, _cts.Token);
                }
            }
        });
    }

    /// <summary>Teleport tới 1 vị trí cụ thể (manual mode).</summary>
    public void TeleportTo(double lat, double lon)
    {
        LocationChanged?.Invoke(this, new LocationChangedEventArgs
        {
            Latitude = lat,
            Longitude = lon,
            Accuracy = 5
        });
    }

    public void Stop()
    {
        _cts?.Cancel();
        IsRunning = false;
    }
}