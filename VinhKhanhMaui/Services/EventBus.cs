using VinhKhanhMaui.Models;

namespace VinhKhanhMaui.Services;

public static class EventBus
{
    public static event Action<bool>? MockGpsToggled;
    public static event Action<LocationChangedEventArgs>? MockLocationReceived;
    public static event Action<string>? LanguageChanged;
    public static event Action<PointOfInterest>? PoiSelected;

    public static void SendMockGpsToggle(bool useMock)
        => MockGpsToggled?.Invoke(useMock);

    public static void SendMockLocation(LocationChangedEventArgs e)
        => MockLocationReceived?.Invoke(e);

    public static void SendLanguageChange(string langCode)
        => LanguageChanged?.Invoke(langCode);

    public static void SendPoiSelected(PointOfInterest poi)
        => PoiSelected?.Invoke(poi);

    public static event Action<string>? MenuActionReceived;

    public static void SendMenuAction(string action)
        => MenuActionReceived?.Invoke(action);
}