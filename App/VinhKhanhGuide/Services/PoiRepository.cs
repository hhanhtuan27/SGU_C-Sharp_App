using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using VinhKhanhGuide.Models;

namespace VinhKhanhGuide.Services
{
    /// <summary>
    /// Loads points of interest from SQL Server. If the DB is unreachable
    /// we fall back to an embedded seed list so the app still runs on the
    /// demo machine (useful when you need to present without network).
    /// </summary>
    public class PoiRepository
    {
        private readonly string _connectionString;

        public PoiRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<PointOfInterest> LoadAll()
        {
            try
            {
                return LoadFromDatabase();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "[PoiRepository] DB load failed, using seed. " + ex.Message);
                return GetSeedData();
            }
        }

        private List<PointOfInterest> LoadFromDatabase()
        {
            var list = new List<PointOfInterest>();
            const string sql =
                @"SELECT Id, Name, Category, Latitude, Longitude, RadiusMeters,
                         Priority, DescriptionVi, DescriptionEn, IsActive
                  FROM dbo.PointsOfInterest WHERE IsActive = 1 ORDER BY Priority DESC, Id";

            using (var conn = new SqlConnection(_connectionString))
            using (var cmd  = new SqlCommand(sql, conn))
            {
                conn.Open();
                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        list.Add(new PointOfInterest
                        {
                            Id            = rd.GetInt32(0),
                            Name          = rd.GetString(1),
                            Category      = PoiCategoryExtensions.Parse(rd.GetString(2)),
                            Latitude      = rd.GetDouble(3),
                            Longitude     = rd.GetDouble(4),
                            RadiusMeters  = rd.GetDouble(5),
                            Priority      = rd.GetInt32(6),
                            DescriptionVi = rd.IsDBNull(7) ? null : rd.GetString(7),
                            DescriptionEn = rd.IsDBNull(8) ? null : rd.GetString(8),
                            IsActive      = rd.GetBoolean(9)
                        });
                    }
                }
            }
            return list;
        }

        /// <summary>Offline fallback — mirrors the SQL seed.</summary>
        public static List<PointOfInterest> GetSeedData()
        {
            return new List<PointOfInterest>
            {
                New(1,  "Ốc Oanh",                       PoiCategory.Oc,    10.760719, 106.703297, 5),
                New(2,  "Quán Ốc 662",                   PoiCategory.Oc,    10.760836, 106.703505, 3),
                New(3,  "Ốc Đào 2",                      PoiCategory.Oc,    10.761137, 106.704979, 3),
                New(4,  "Quán Ốc Bụi",                   PoiCategory.Oc,    10.760597, 106.704217, 2),
                New(5,  "Ốc Hoa",                        PoiCategory.Oc,    10.760713, 106.704217, 2),
                New(6,  "Quán Ốc Sáu Nở",                PoiCategory.Oc,    10.760964, 106.702942, 3),
                New(7,  "Quán Ốc Vũ",                    PoiCategory.Oc,    10.761403, 106.702705, 2),
                New(8,  "Ốc Phát - Ốc Ngon Quận 4",      PoiCategory.Oc,    10.761955, 106.702094, 3),
                New(9,  "Tiệm Nướng 10 Năm",             PoiCategory.Nuong, 10.760537, 106.703528, 3),
                New(10, "Hàu Nướng A Trung",             PoiCategory.Nuong, 10.760669, 106.703673, 3),
                New(11, "Lẩu Mẹt Nướng 79k",             PoiCategory.Nuong, 10.760806, 106.704310, 2),
                New(12, "Trạm Nướng BBQ",                PoiCategory.Nuong, 10.760728, 106.704679, 2),
                New(13, "Thèm Nướng Yakiniku",           PoiCategory.Nuong, 10.760778, 106.704739, 3),
                New(14, "Thế Giới Bò - Nướng & Lẩu",     PoiCategory.Nuong, 10.764036, 106.701278, 2),
                New(15, "Lẩu Bò Kỳ Kim",                 PoiCategory.Lau,   10.761460, 106.702608, 3),
                New(16, "Lẩu gà lá é Con Gà Trống",      PoiCategory.Lau,   10.760856, 106.706722, 3),
                New(17, "Tiệm Cà Phê Lucky",             PoiCategory.CaPhe, 10.760451, 106.707005, 1),
                New(18, "Xù Phê",                        PoiCategory.CaPhe, 10.761201, 106.706126, 1),
                New(19, "Link Coffee & Tea",             PoiCategory.CaPhe, 10.760846, 106.704983, 1),
                New(20, "Quán Nước SINZIEN",             PoiCategory.CaPhe, 10.761756, 106.702283, 1),
            };
        }

        private static PointOfInterest New(int id, string name, PoiCategory cat,
                                           double lat, double lon, int prio)
        {
            return new PointOfInterest
            {
                Id            = id,
                Name          = name,
                Category      = cat,
                Latitude      = lat,
                Longitude     = lon,
                RadiusMeters  = 30,
                Priority      = prio,
                DescriptionVi = name + " là một địa điểm nổi bật của phố ẩm thực Vĩnh Khánh, Quận 4.",
                DescriptionEn = name + " is a popular spot on the Vinh Khanh food street in District 4."
            };
        }
    }
}
