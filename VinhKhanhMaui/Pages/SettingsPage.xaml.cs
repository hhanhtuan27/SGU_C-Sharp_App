using VinhKhanhMaui.Models;
using VinhKhanhMaui.Services;

namespace VinhKhanhMaui.Pages;

public partial class SettingsPage : ContentPage
{
    private readonly MockGpsService _mockGps = new();

    public SettingsPage()
    {
        InitializeComponent();
        PickerLang.ItemsSource = AppLanguage.All;

        int savedIdx = AppLanguage.All.FindIndex(
            l => l.Code == Preferences.Get("lang", "vi"));
        PickerLang.SelectedIndex = Math.Max(0, savedIdx);

        _mockGps.LocationChanged += (s, e) =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                LblMockStatus.Text = $"📍 {e.Latitude:F5}, {e.Longitude:F5}";
                EventBus.SendMockLocation(e);
            });
        };
    }

    private void BtnMockStart_Clicked(object sender, EventArgs e)
    {
        _mockGps.StartAutoWalk();
        BtnMockStart.IsEnabled = false;
        BtnMockStop.IsEnabled = true;
        BtnMockStop.TextColor = Colors.White;
        LblMockStatus.Text = "🚶 Đang đi bộ dọc Vĩnh Khánh...";
        EventBus.SendMockGpsToggle(true);
    }

    private void BtnMockStop_Clicked(object sender, EventArgs e)
    {
        _mockGps.Stop();
        BtnMockStart.IsEnabled = true;
        BtnMockStop.IsEnabled = false;
        BtnMockStop.TextColor = Color.FromArgb("#6B7280");
        LblMockStatus.Text = "⏹ Đã dừng";
        EventBus.SendMockGpsToggle(false);
    }

    private void PickerLang_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (PickerLang.SelectedItem is AppLanguage lang)
        {
            Preferences.Set("lang", lang.Code);
            EventBus.SendLanguageChange(lang.Code);
        }
    }

    private async void BtnResetOnboarding_Clicked(object sender, EventArgs e)
    {
        Preferences.Set("onboarding_done", false);
        await Shell.Current.GoToAsync("onboarding");
    }

    private async void BtnLogout_Clicked(object sender, EventArgs e)
    {
        bool confirm = await this.DisplayAlert(
            "Đăng xuất", "Bạn có chắc muốn đăng xuất?", "Đăng xuất", "Hủy");
        if (!confirm) return;

        Preferences.Remove("logged_in");
        Preferences.Remove("username");

        // Reset toàn bộ app thay vì navigate (tránh stack rối)
        Application.Current!.MainPage = new AppShell();
    }
}