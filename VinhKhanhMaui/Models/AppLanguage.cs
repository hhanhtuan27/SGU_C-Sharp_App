namespace VinhKhanhMaui.Models;

public class AppLanguage
{
    public string Code { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public string Locale { get; set; } = "";

    public static List<AppLanguage> All => new()
    {
        new() { Code = "vi", DisplayName = "🇻🇳 Tiếng Việt", Locale = "vi-VN" },
        new() { Code = "en", DisplayName = "🇬🇧 English",     Locale = "en-US" },
        new() { Code = "ja", DisplayName = "🇯🇵 日本語",       Locale = "ja-JP" },
        new() { Code = "ko", DisplayName = "🇰🇷 한국어",       Locale = "ko-KR" },
        new() { Code = "zh", DisplayName = "🇨🇳 中文",         Locale = "zh-CN" },
    };
}