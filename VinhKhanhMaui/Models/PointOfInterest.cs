using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace VinhKhanhMaui.Models;

public class PointOfInterest : INotifyPropertyChanged
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Category { get; set; } = "";
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double RadiusMeters { get; set; } = 30;
    public string ImageUrl { get; set; } = "";

    public string DescriptionVi { get; set; } = "";
    public string DescriptionEn { get; set; } = "";
    public string DescriptionJa { get; set; } = "";
    public string DescriptionKo { get; set; } = "";
    public string DescriptionZh { get; set; } = "";

    private double _distanceMeters;
    public double DistanceMeters
    {
        get => _distanceMeters;
        set
        {
            if (Math.Abs(_distanceMeters - value) > 0.1)
            {
                _distanceMeters = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DistanceText));
            }
        }
    }

    public string DistanceText => _distanceMeters < 1000
        ? $"{_distanceMeters:F0} m"
        : $"{_distanceMeters / 1000:F1} km";

    public string CategoryIcon => Category switch
    {
        "Oc" => "🐌",
        "Nuong" => "🔥",
        "Lau" => "🍲",
        "CaPhe" => "☕",
        _ => "🍜"
    };

    /// <summary>Màu category dạng HEX string — dùng trong XAML binding.</summary>
    public string CategoryHex => Category switch
    {
        "Oc" => "#FF8C42",
        "Nuong" => "#DC3545",
        "Lau" => "#FFC107",
        "CaPhe" => "#0D6EFD",
        _ => "#6C757D"
    };

    /// <summary>Image source string — XAML tự convert.</summary>
    public string ImageDisplaySource
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(ImageUrl) &&
                ImageUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                return ImageUrl;
            return $"poi_{Id}.jpg";
        }
    }

    public string GetDescription(string lang)
    {
        string primary = lang switch
        {
            "en" => DescriptionEn,
            "ja" => DescriptionJa,
            "ko" => DescriptionKo,
            "zh" => DescriptionZh,
            _ => DescriptionVi
        };
        if (!string.IsNullOrWhiteSpace(primary)) return primary;
        if (!string.IsNullOrWhiteSpace(DescriptionEn)) return DescriptionEn;
        return DescriptionVi;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}