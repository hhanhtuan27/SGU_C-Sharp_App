using VinhKhanhMaui.Models;

namespace VinhKhanhMaui.Services;

public class PoiRepository
{
    private readonly ApiService _api = new();

    /// <summary>
    /// Load POI: ưu tiên API web → fallback seed data offline.
    /// </summary>
    public async Task<List<PointOfInterest>> LoadAllAsync()
    {
        try
        {
            var pois = await _api.LoadPoisAsync();
            if (pois != null && pois.Count > 0)
            {
                System.Diagnostics.Debug.WriteLine($"✓ API: {pois.Count} POIs from server");
                return pois;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"✗ API failed: {ex.Message}");
        }

        System.Diagnostics.Debug.WriteLine("⚠ Using SEED data (offline)");
        return GetSeedData();
    }
    public static List<PointOfInterest> GetSeedData()
    {
        return new List<PointOfInterest>
        {
            New(1,  "Ốc Oanh",              "Oc",    10.760719, 106.703297, 5),
            New(2,  "Quán Ốc 662",          "Oc",    10.760836, 106.703505, 3),
            New(3,  "Ốc Đào 2",             "Oc",    10.761137, 106.704979, 3),
            New(4,  "Quán Ốc Bụi",          "Oc",    10.760597, 106.704455, 2),
            New(5,  "Ốc Hoa",               "Oc",    10.760713, 106.704217, 2),
            New(6,  "Quán Ốc Sáu Nở",       "Oc",    10.760964, 106.702942, 3),
            New(7,  "Quán Ốc Vũ",           "Oc",    10.761403, 106.702705, 2),
            New(8,  "Ốc Phát - Ngon Q4",    "Oc",    10.761955, 106.702094, 3),
            New(9,  "Tiệm Nướng 10 Năm",    "Nuong", 10.760537, 106.703528, 3),
            New(10, "Hàu Nướng A Trung",    "Nuong", 10.760669, 106.703673, 3),
            New(11, "Lẩu Mẹt Nướng 79k",    "Nuong", 10.760806, 106.704310, 2),
            New(12, "Trạm Nướng BBQ",       "Nuong", 10.760728, 106.704679, 2),
            New(13, "Thèm Nướng Yakiniku",  "Nuong", 10.760778, 106.704739, 3),
            New(14, "Thế Giới Bò - Nướng",  "Nuong", 10.764036, 106.701278, 2),
            New(15, "Lẩu Bò Kỳ Kim",        "Lau",   10.761460, 106.702608, 3),
            New(16, "Lẩu gà lá é",          "Lau",   10.760856, 106.706722, 3),
            New(17, "Tiệm cà phê Lucky",    "CaPhe", 10.760451, 106.707005, 1),
            New(18, "Xù Phê",               "CaPhe", 10.761201, 106.706126, 1),
            New(19, "Link Coffee & Tea",    "CaPhe", 10.760846, 106.704983, 1),
            New(20, "Quán Nước SINZIEN",    "CaPhe", 10.761756, 106.702283, 1),
        };
    }
    public async Task PingServerAsync()
    {
        var api = new ApiService();
        await api.PingAsync();
    }
    private static PointOfInterest New(int id, string name, string cat,
        double lat, double lon, int priority)
    {
        return new PointOfInterest
        {
            Id = id,
            Name = name,
            Category = cat,
            Latitude = lat,
            Longitude = lon,
            RadiusMeters = 30,
            // ImageUrl trống → app dùng local file poi_X.jpg
            // Khi web admin upload ảnh → ImageUrl = "http://server/uploads/xxx.jpg"
            // → app tự hiển thị ảnh từ URL
            ImageUrl = "",
            DescriptionVi = $"{name} là điểm ẩm thực nổi bật của phố Vĩnh Khánh, Quận 4, Hồ Chí Minh. Nổi tiếng với hương vị đậm đà, không gian phố đêm sôi động và giá cả hợp lý.",
            DescriptionEn = $"{name} is a famous food spot on Vinh Khanh street, District 4, Ho Chi Minh City. Known for rich authentic flavors, vibrant night street atmosphere and reasonable prices.",
            DescriptionJa = $"{name}は、ホーチミン市4区のヴィンカイン通りにある有名な屋台です。本格的な味、活気あるナイトストリートの雰囲気、お手頃な価格で知られています。",
            DescriptionKo = $"{name}은(는) 호치민시 4군 빈칸 거리의 유명한 먹거리 가게입니다. 정통 맛, 활기 넘치는 야시장 분위기, 합리적인 가격으로 유명합니다.",
            DescriptionZh = $"{name} 是胡志明市第4郡永庆美食街上的著名小吃店。以正宗的风味、充满活力的夜市氛围和合理的价格而闻名。",
        };
    }
}