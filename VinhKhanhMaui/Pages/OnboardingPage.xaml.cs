namespace VinhKhanhMaui.Pages;

public partial class OnboardingPage : ContentPage
{
    public OnboardingPage()
    {
        InitializeComponent();
        Carousel.ItemsSource = new[]
        {
            new { Icon = "🗺️", Title = "Khám phá Vĩnh Khánh",
                  Description = "Phố ẩm thực nổi tiếng nhất Quận 4 với hơn 20 quán ốc, nướng, lẩu, cà phê." },
            new { Icon = "📍", Title = "GPS Tự động",
                  Description = "Bật quyền vị trí để app phát hiện khi bạn đến gần quán." },
            new { Icon = "🔊", Title = "Thuyết minh đa ngôn ngữ",
                  Description = "TTS 5 ngôn ngữ: VN, EN, 日本語, 한국어, 中文. Khi vào vùng 30m quán, app tự đọc." },
            new { Icon = "⚡", Title = "Thông minh chống nhiễu",
                  Description = "Debounce 3s, cooldown 5 phút, hàng đợi ưu tiên." },
        };
        BtnStart.Clicked += OnStart;
    }

    private void OnStart(object? sender, EventArgs e)
    {
        Preferences.Set("onboarding_done", true);
        Application.Current!.Windows[0].Page = new LoginPage();
    }
}