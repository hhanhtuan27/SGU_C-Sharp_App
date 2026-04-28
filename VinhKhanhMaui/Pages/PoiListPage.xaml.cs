using VinhKhanhMaui.Models;
using VinhKhanhMaui.Services;

namespace VinhKhanhMaui.Pages;

public partial class PoiListPage : ContentPage
{
    private List<PointOfInterest> _allPois = new();
    private string _selectedCategory = "All";

    // Dùng PoiRepository — đã có sẵn fallback: thử API trước, nếu lỗi dùng seed data
    // KHÔNG thay bằng ApiService trực tiếp vì sẽ mất dữ liệu offline
    private readonly PoiRepository _repo = new();

    private readonly (string Code, string Label, Color Color)[] _categories = new[]
    {
        ("All",   "Tất cả",  Color.FromArgb("#EBEDF0")),
        ("Oc",    "🐌 Ốc",   Color.FromArgb("#FF8C42")),
        ("Nuong", "🔥 Nướng", Color.FromArgb("#DC3545")),
        ("Lau",   "🍲 Lẩu",  Color.FromArgb("#FFC107")),
        ("CaPhe", "☕ Cà phê",Color.FromArgb("#0D6EFD")),
        ("Khac",  "🍜 Khác",  Color.FromArgb("#6C757D")),
    };

    public PoiListPage()
    {
        InitializeComponent();
        BuildFilterChips();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // FIX: Luôn gọi LoadAllAsync() mỗi lần vào trang — KHÔNG có điều kiện "if count == 0"
        // PoiRepository tự xử lý: thử API trước → nếu lỗi mạng → trả về seed data
        // Kết quả: app online thì lấy data mới nhất, offline thì vẫn có 20 quán seed
        _allPois = await _repo.LoadAllAsync();

        ApplyFilter();
    }

    private void BuildFilterChips()
    {
        FilterChips.Children.Clear();
        foreach (var (code, label, color) in _categories)
        {
            var btn = new Button
            {
                Text = label,
                FontSize = 13,
                HeightRequest = 36,
                CornerRadius = 18,
                Padding = new Thickness(14, 0),
                BackgroundColor = code == "All" ? Color.FromArgb("#FF8C42") : Color.FromArgb("#2A3545"),
                TextColor = code == "All" ? Colors.Black : Colors.White,
                FontAttributes = FontAttributes.Bold
            };
            string cat = code;
            btn.Clicked += (s, e) =>
            {
                _selectedCategory = cat;
                foreach (var child in FilterChips.Children)
                    if (child is Button b)
                    {
                        b.BackgroundColor = Color.FromArgb("#2A3545");
                        b.TextColor = Colors.White;
                    }
                btn.BackgroundColor = Color.FromArgb("#FF8C42");
                btn.TextColor = Colors.Black;
                ApplyFilter();
            };
            FilterChips.Children.Add(btn);
        }
    }

    private void ApplyFilter()
    {
        string search = TxtSearch.Text?.Trim().ToLower() ?? "";
        var filtered = _allPois
            .Where(p => _selectedCategory == "All" || p.Category == _selectedCategory)
            .Where(p => string.IsNullOrEmpty(search) || p.Name.ToLower().Contains(search))
            .OrderBy(p => p.DistanceMeters)
            .ToList();

        PoiListView.ItemsSource = null;
        PoiListView.ItemsSource = filtered;
    }

    private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        => ApplyFilter();

    private async void PoiListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is PointOfInterest poi)
        {
            ((CollectionView)sender).SelectedItem = null;
            EventBus.SendPoiSelected(poi);
            await Shell.Current.GoToAsync("//map");
        }
    }
}
