// FIX tất cả lỗi namespace — KHÔNG dùng "using Android.*"
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using VinhKhanhMaui.Models;
using VinhKhanhMaui.Services;
using MauiLocation = Microsoft.Maui.Devices.Sensors.Location;
using MauiColors = Microsoft.Maui.Graphics.Colors;
using AppLocationArgs = VinhKhanhMaui.Services.LocationChangedEventArgs;

namespace VinhKhanhMaui.Pages;

public partial class MapPage : ContentPage
{
    // Dùng PoiRepository: thử API trước, nếu lỗi mạng → fallback seed data
    private readonly PoiRepository _repo = new();
    private readonly LocationTrackingService _gps = new();
    private readonly NarrationService _narration = new();
    private readonly MockGpsService _mockGps = new();
    private GeofenceService? _geofence;

    private List<PointOfInterest> _pois = new();
    private PointOfInterest? _pinnedPoi;
    private Pin? _simulatedLocationPin;
    private double _currentLat, _currentLon;
    private bool _useMockGps = false;
    private bool _initialized = false;
    private string _selectedCategory = "All";
    private string _searchText = "";

    private readonly (string Code, string Label)[] _categories = new[]
    {
        ("All",   "Tất cả"),
        ("Oc",    "🐌 Ốc"),
        ("Nuong", "🔥 Nướng"),
        ("Lau",   "🍲 Lẩu"),
        ("CaPhe", "☕ Cà phê"),
        ("Khac",  "🍜 Khác"),
    };

    public MapPage()
    {
        InitializeComponent();
        PickerLanguage.ItemsSource = AppLanguage.All;
        int idx = AppLanguage.All.FindIndex(l => l.Code == Preferences.Get("lang", "vi"));
        PickerLanguage.SelectedIndex = Math.Max(0, idx);

        _mockGps.LocationChanged += (s, e) => { if (_useMockGps) OnLocationUpdate(e); };

        EventBus.MenuActionReceived += async action =>
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                switch (action)
                {
                    case "poilist": await ShowPoiListPopup(); break;
                    case "nearby": await ShowNearbyPopup(); break;
                    case "simulator": ToggleSimulator(); break;
                    case "refresh": await RefreshDataAsync(); break;
                    case "help": await ShowHelpPopup(); break;
                }
            });
        };
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        // Ping server NGAY để web dashboard hiện thiết bị
        _ = Task.Run(async () =>
        {
            try { await _repo.PingServerAsync(); }
            catch { }
        });
        // FIX: Luôn gọi LoadAllAsync() mỗi lần vào trang
        // PoiRepository tự lo: API có → data mới | API lỗi → seed data offline
        _pois = await _repo.LoadAllAsync();

        if (_geofence != null) _geofence.GeofenceEntered -= OnGeofenceEntered;
        _geofence = new GeofenceService(_pois);
        _geofence.GeofenceEntered += OnGeofenceEntered;
        RenderMarkers();

        if (!_initialized)
        {
            BuildFilterChips();
            MapView.MapClicked += OnMapClicked;
            _gps.LocationChanged += (s, e) => { if (!_useMockGps) OnLocationUpdate(e); };
            _gps.StatusChanged += (s, msg) =>
                MainThread.BeginInvokeOnMainThread(() => { if (!_useMockGps) LblStatus.Text = msg; });
            _narration.SpeakingStarted += (s, poi) =>
                MainThread.BeginInvokeOnMainThread(() =>
                { LblNarrating.Text = $"{poi.Name} ({poi.DistanceMeters:F0}m)"; NarratingBanner.IsVisible = true; });
            _narration.SpeakingCompleted += (s, e) =>
                MainThread.BeginInvokeOnMainThread(() => NarratingBanner.IsVisible = false);
            _initialized = true;
            // LUÔN load data mới (không điều kiện)
            _pois = await _repo.LoadAllAsync();
            _geofence = new GeofenceService(_pois);
            _geofence.GeofenceEntered -= OnGeofenceEntered; // tránh duplicate
            _geofence.GeofenceEntered += OnGeofenceEntered;
            FilterMarkers();
        }

        MapView.MoveToRegion(MapSpan.FromCenterAndRadius(
            new MauiLocation(10.7609, 106.7035), Distance.FromMeters(400)));
        if (!_useMockGps) await _gps.StartAsync();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        if (!_useMockGps) _gps.Stop();
    }

    private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
    {
        _searchText = e.NewTextValue?.Trim().ToLower() ?? "";
        BtnClearSearch.IsVisible = !string.IsNullOrEmpty(_searchText);
        if (string.IsNullOrEmpty(_searchText)) { FilterMarkers(); return; }
        var matched = _pois.Where(p => p.Name.ToLower().Contains(_searchText)).ToList();
        MapView.Pins.Clear();
        if (_simulatedLocationPin != null) MapView.Pins.Add(_simulatedLocationPin);
        foreach (var poi in matched) MapView.Pins.Add(CreatePin(poi));
        if (matched.Count == 1) { OnPoiTapped(matched[0]); TxtSearch.Unfocus(); }
    }

    private void BtnClearSearch_Clicked(object sender, EventArgs e)
    {
        TxtSearch.Text = ""; _searchText = "";
        BtnClearSearch.IsVisible = false;
        FilterMarkers(); TxtSearch.Unfocus();
    }

    private void BtnMenu_Clicked(object sender, EventArgs e) => Shell.Current.FlyoutIsPresented = true;

    private void BuildFilterChips()
    {
        FilterChips.Children.Clear();
        foreach (var (code, label) in _categories)
        {
            var btn = new Button
            {
                Text = label,
                FontSize = 12,
                HeightRequest = 32,
                CornerRadius = 16,
                Padding = new Thickness(12, 0),
                BackgroundColor = code == "All" ? Color.FromArgb("#FF8C42") : Color.FromArgb("#2A3545"),
                TextColor = code == "All" ? MauiColors.Black : MauiColors.White,
                FontAttributes = FontAttributes.Bold
            };
            string cat = code;
            btn.Clicked += (s, e) =>
            {
                _selectedCategory = cat;
                foreach (var child in FilterChips.Children)
                    if (child is Button b) { b.BackgroundColor = Color.FromArgb("#2A3545"); b.TextColor = MauiColors.White; }
                btn.BackgroundColor = Color.FromArgb("#FF8C42"); btn.TextColor = MauiColors.Black;
                FilterMarkers();
            };
            FilterChips.Children.Add(btn);
        }
    }

    private void FilterMarkers()
    {
        MapView.Pins.Clear();
        if (_simulatedLocationPin != null) MapView.Pins.Add(_simulatedLocationPin);
        var filtered = _selectedCategory == "All" ? _pois : _pois.Where(p => p.Category == _selectedCategory).ToList();
        foreach (var poi in filtered) MapView.Pins.Add(CreatePin(poi));
        UpdateNearbyStrip();
    }

    private Pin CreatePin(PointOfInterest poi)
    {
        var pin = new Pin
        {
            Label = poi.Name,
            Address = poi.CategoryIcon,
            Type = PinType.Place,
            Location = new MauiLocation(poi.Latitude, poi.Longitude)
        };
        pin.MarkerClicked += (s, e) => { e.HideInfoWindow = false; OnPoiTapped(poi); };
        return pin;
    }

    private void RenderMarkers()
    {
        MapView.Pins.Clear();
        if (_simulatedLocationPin != null) MapView.Pins.Add(_simulatedLocationPin);
        foreach (var poi in _pois) MapView.Pins.Add(CreatePin(poi));
        UpdateNearbyStrip();
    }

    private void OnMapClicked(object? sender, MapClickedEventArgs e)
    {
        if (_useMockGps) TeleportSimulator(e.Location.Latitude, e.Location.Longitude);
        else { _narration.StopAll(); NarratingBanner.IsVisible = false; }
    }

    private void TeleportSimulator(double lat, double lon)
    {
        if (_simulatedLocationPin != null) MapView.Pins.Remove(_simulatedLocationPin);
        _simulatedLocationPin = new Pin
        {
            Label = "📍 Vị trí giả lập",
            Type = PinType.Generic,
            Location = new MauiLocation(lat, lon)
        };
        MapView.Pins.Add(_simulatedLocationPin);
        _mockGps.TeleportTo(lat, lon);
        LblSimStatus.Text = $"📍 {lat:F5}, {lon:F5}";
    }

    private void OnAnywhereTapped(object sender, TappedEventArgs e)
    { _narration.StopAll(); NarratingBanner.IsVisible = false; }

    private async void ToggleSimulator()
    {
        _useMockGps = !_useMockGps;
        SimulatorBar.IsVisible = _useMockGps;
        if (_useMockGps)
        {
            _gps.Stop(); MapView.IsShowingUser = false;
            LblSimStatus.Text = "👆 Tap bản đồ để chọn vị trí"; LblStatus.Text = "🎮 GPS Giả lập";
            await this.DisplayAlert("GPS Giả lập",
                "👆 Tap vào bản đồ để đặt vị trí giả lập.\n\nNếu vị trí gần quán (30m), app tự động thuyết minh.\n\nBấm ✕ Tắt trên thanh xanh để quay lại GPS thật.", "Đã hiểu");
        }
        else
        {
            _mockGps.Stop(); MapView.IsShowingUser = true;
            if (_simulatedLocationPin != null) { MapView.Pins.Remove(_simulatedLocationPin); _simulatedLocationPin = null; }
            _ = _gps.StartAsync(); LblStatus.Text = "📡 GPS thật";
        }
    }

    private void BtnSimClose_Clicked(object sender, EventArgs e) { if (_useMockGps) ToggleSimulator(); }

    private void OnLocationUpdate(AppLocationArgs e)
    {
        _currentLat = e.Latitude; _currentLon = e.Longitude;
        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (_useMockGps) LblSimStatus.Text = $"📍 {e.Latitude:F5}, {e.Longitude:F5}";
            else LblStatus.Text = $"📍 {e.Latitude:F5}, {e.Longitude:F5}  (±{e.Accuracy:F0}m)";
            foreach (var p in _pois)
                p.DistanceMeters = GeofenceService.Haversine(e.Latitude, e.Longitude, p.Latitude, p.Longitude);
            if (e.Accuracy <= 50) _geofence?.UpdateLocation(e.Latitude, e.Longitude);
            UpdateNearbyStrip();
        });
    }

    private void OnGeofenceEntered(object? sender, GeofenceEnteredEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() => { _pinnedPoi = null; UpdateNearbyStrip(); _narration.EnqueueSpeak(e.Poi); });
    }

    private void UpdateNearbyStrip()
    {
        var source = _selectedCategory == "All" ? _pois : _pois.Where(p => p.Category == _selectedCategory).ToList();
        var nearby = source.Where(p => p.DistanceMeters <= 50 && p.DistanceMeters > 0).OrderBy(p => p.DistanceMeters).ToList();
        var finalList = new List<PointOfInterest>();
        if (_pinnedPoi != null)
        {
            if (nearby.Any(p => p.Id == _pinnedPoi.Id)) _pinnedPoi = null;
            else finalList.Add(_pinnedPoi);
        }
        finalList.AddRange(nearby);
        NearbyCollectionView.ItemsSource = null;
        NearbyCollectionView.ItemsSource = finalList;
        LblNearbyCount.Text = finalList.Count > 0 ? $"{finalList.Count} quán" : "";
    }

    private void OnPoiTapped(PointOfInterest poi)
    {
        MapView.MoveToRegion(MapSpan.FromCenterAndRadius(new MauiLocation(poi.Latitude, poi.Longitude), Distance.FromMeters(150)));
        if (_currentLat != 0)
            poi.DistanceMeters = GeofenceService.Haversine(_currentLat, _currentLon, poi.Latitude, poi.Longitude);
        if (poi.DistanceMeters > 50) { _pinnedPoi = poi; UpdateNearbyStrip(); }
        _narration.StopAll(); _narration.EnqueueSpeak(poi);
    }

    private void NearbyStrip_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is PointOfInterest poi)
        { OnPoiTapped(poi); ((CollectionView)sender).SelectedItem = null; }
    }

    private void PickerLanguage_Changed(object sender, EventArgs e)
    {
        if (PickerLanguage.SelectedItem is AppLanguage lang)
        { _narration.CurrentLanguage = lang.Code; Preferences.Set("lang", lang.Code); _narration.StopAll(); }
    }

    private void BtnStopNarration_Clicked(object sender, EventArgs e)
    { _narration.StopAll(); NarratingBanner.IsVisible = false; }

    private async Task ShowPoiListPopup()
    {
        if (_currentLat != 0)
            foreach (var p in _pois)
                p.DistanceMeters = GeofenceService.Haversine(_currentLat, _currentLon, p.Latitude, p.Longitude);
        var sorted = _pois.OrderBy(p => p.DistanceMeters).ToList();
        var items = sorted.Select(p => $"{p.CategoryIcon} {p.Name}  —  📍 {(_currentLat == 0 ? "?" : p.DistanceText)}").ToArray();
        string? chosen = await DisplayActionSheet("📋 Danh sách quán", "Đóng", null, items);
        if (chosen != null && chosen != "Đóng") { int i = Array.IndexOf(items, chosen); if (i >= 0) OnPoiTapped(sorted[i]); }
    }

    private async Task ShowNearbyPopup()
    {
        if (_currentLat == 0) { await this.DisplayAlert("GPS", "Chưa có vị trí.", "OK"); return; }
        var nearby = _pois.Where(p => p.DistanceMeters <= 500).OrderBy(p => p.DistanceMeters).ToList();
        if (nearby.Count == 0) { await this.DisplayAlert("Nearby", "Không có quán trong 500m.", "OK"); return; }
        var items = nearby.Select(p => $"{p.CategoryIcon} {p.Name}  —  📍 {p.DistanceText}").ToArray();
        string? chosen = await DisplayActionSheet($"📍 {nearby.Count} quán gần bạn", "Đóng", null, items);
        if (chosen != null && chosen != "Đóng") { int i = Array.IndexOf(items, chosen); if (i >= 0) OnPoiTapped(nearby[i]); }
    }

    private async Task ShowHelpPopup()
    {
        string lang = _narration.CurrentLanguage;
        string content = lang switch
        {
            "en" => "1. Allow GPS permission\n2. Walk near a restaurant (30m)\n3. App auto-narrates\n4. Tap pin to hear description\n5. Use search bar to find\n6. ☰ menu for more\n\nSupport: 0707289072",
            "ja" => "1. GPS権限を許可\n2. 店の近く(30m)を歩く\n3. 自動案内\n4. ピンをタップ\n5. 検索バーで検索\n6. ☰メニュー\n\nサポート: 0707289072",
            "ko" => "1. GPS 권한 허용\n2. 식당 근처(30m) 걷기\n3. 자동 안내\n4. 핀 탭\n5. 검색\n6. ☰ 메뉴\n\n지원: 0707289072",
            "zh" => "1. 允许GPS权限\n2. 走到店铺附近(30m)\n3. 自动播报\n4. 点击标记\n5. 搜索\n6. ☰ 菜单\n\n客服: 0707289072",
            _ => "1. Cho phép GPS\n2. Đi gần quán (30m)\n3. App tự thuyết minh\n4. Tap pin để nghe\n5. Thanh tìm kiếm\n6. ☰ menu thêm\n\n• Debounce 3s • Cooldown 5 phút\n\nHỗ trợ: 0707289072"
        };
        await this.DisplayAlert("📖 Hướng dẫn", content, "OK");
    }

    private async Task RefreshDataAsync()
    {
        LblStatus.Text = "🔄 Đang tải...";
        _pois = await _repo.LoadAllAsync();
        if (_geofence != null) _geofence.GeofenceEntered -= OnGeofenceEntered;
        _geofence = new GeofenceService(_pois);
        _geofence.GeofenceEntered += OnGeofenceEntered;
        FilterMarkers();
        LblStatus.Text = $"✅ {_pois.Count} quán";
        await this.DisplayAlert("✅", $"Đã tải {_pois.Count} quán", "OK");
    }
}
