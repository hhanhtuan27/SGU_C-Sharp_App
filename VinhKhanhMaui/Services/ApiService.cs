using System.Text.Json;
using VinhKhanhMaui.Models;

namespace VinhKhanhMaui.Services;

public class ApiService
{
    // ═══════════════════════════════════════════════
    // ĐỔI URL NÀY THÀNH DEV TUNNEL CỦA BẠN
    // ═══════════════════════════════════════════════
    public static readonly string BaseUrl = "https://07pcx3gw-5000.asse.devtunnels.ms";

    private readonly HttpClient _http;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ApiService()
    {
        _http = new HttpClient
        {
            BaseAddress = new Uri(BaseUrl),
            Timeout = TimeSpan.FromSeconds(10)
        };
        _http.DefaultRequestHeaders.Add("Cache-Control", "no-cache, no-store");
    }

    /// <summary>Load danh sách POI từ web admin API.</summary>
    public async Task<List<PointOfInterest>?> LoadPoisAsync()
    {
        try
        {
            var response = await _http.GetAsync("/api/pois");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<PointOfInterest>>(json, _jsonOptions);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>Ping server báo thiết bị đang online.</summary>
    public async Task PingAsync()
    {
        try
        {
            var deviceId = $"{DeviceInfo.Current.Name}-{DeviceInfo.Current.Platform}";
            var payload = new
            {
                deviceId,
                platform = DeviceInfo.Current.Platform.ToString(),
                appVersion = AppInfo.Current.VersionString
            };
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            await _http.PostAsync("/api/ping", content, cts.Token);
        }
        catch { }
    }
}