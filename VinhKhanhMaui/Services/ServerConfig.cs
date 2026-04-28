namespace VinhKhanhMaui;

/// <summary>
/// ĐỔI IP Ở ĐÂY khi chạy trên điện thoại thật qua WiFi.
/// Chỉ cần sửa 1 dòng duy nhất — toàn bộ app dùng giá trị này.
/// </summary>
public static class ServerConfig
{
    // ─────────────────────────────────────────────────────────
    //  BƯỚC 1: Tìm IP máy tính đang chạy web admin
    //    Windows: mở CMD → gõ "ipconfig" → tìm "IPv4 Address"
    //    VD: 192.168.1.15
    //
    //  BƯỚC 2: Đổi dòng BaseUrl bên dưới
    //
    //  BƯỚC 3: Build lại app (Ctrl+Shift+B) → cài lên điện thoại
    //    Sau đó không cần cắm dây nữa, app tự fetch qua WiFi
    // ─────────────────────────────────────────────────────────

#if DEBUG
    // Chạy trên emulator Android → 10.0.2.2 trỏ về localhost máy tính
    // Chạy trên điện thoại thật    → đổi thành IP LAN của máy tính
    public const string BaseUrl = "http://10.0.2.2:5000";
    // public const string BaseUrl = "http://192.168.1.15:5000";  // ← bỏ comment dòng này khi dùng điện thoại thật
#else
    // Production — đổi thành domain thật khi deploy lên server
    public const string BaseUrl = "https://your-domain.com";
#endif

    // Timeout mỗi request (giây)
    public const int TimeoutSeconds = 10;
}
