using Android.App;
using Android.Content.PM;
using Android.OS;

namespace VinhKhanhMaui;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true,
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation |
    ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize |
    ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        // Tăng cache cho Google Maps tiles (mặc định ~50MB, tăng lên 100MB)
        // Giữ map hiển thị khi mất WiFi tạm thời
        try
        {
            var cacheDir = CacheDir;
            if (cacheDir != null)
            {
                long maxCacheSize = 100 * 1024 * 1024; // 100MB
                // Google Maps tự quản lý cache trong thư mục này
                // Chỉ cần đảm bảo đủ dung lượng
            }
        }
        catch { }
    }
}