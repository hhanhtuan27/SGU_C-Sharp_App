/* =====================================================================
   VinhKhanhGuide — Views.sql (5 views cho web admin dashboard)
   ===================================================================== */

USE VinhKhanhGuide;
GO

/* ---------------------------------------------------------------------
   vw_OnlineUsers — thiết bị online (ping trong 2 phút gần nhất)
   Tách theo platform (Android/iOS/Windows).
   Dashboard hiển thị: "5 devices online (3 Android, 2 iOS)"
   --------------------------------------------------------------------- */
IF OBJECT_ID('dbo.vw_OnlineUsers', 'V') IS NOT NULL DROP VIEW dbo.vw_OnlineUsers;
GO
CREATE VIEW dbo.vw_OnlineUsers
AS
SELECT
    d.DeviceId,
    d.UserId,
    u.Username,
    u.DisplayName,
    d.Platform,
    d.AppVersion,
    d.LastPingUtc,
    DATEDIFF(SECOND, d.LastPingUtc, SYSUTCDATETIME()) AS SecondsSinceLastPing
FROM dbo.ActiveDevices d
LEFT JOIN dbo.Users u ON d.UserId = u.Id
WHERE d.LastPingUtc >= DATEADD(MINUTE, -2, SYSUTCDATETIME());
GO

/* ---------------------------------------------------------------------
   vw_TopPlayedPois — top 10 quán nghe nhiều nhất 90 ngày qua
   Dashboard bảng "Hot spots".
   --------------------------------------------------------------------- */
IF OBJECT_ID('dbo.vw_TopPlayedPois', 'V') IS NOT NULL DROP VIEW dbo.vw_TopPlayedPois;
GO
CREATE VIEW dbo.vw_TopPlayedPois
AS
SELECT TOP 10
    p.Id,
    p.Name,
    p.Category,
    p.ImageUrl,
    COUNT(n.Id)                AS TotalPlays,
    COUNT(DISTINCT n.DeviceId) AS UniqueDevices,
    COUNT(DISTINCT n.Language) AS LanguagesUsed,
    MAX(n.PlayedAt)            AS LastPlayedAt
FROM dbo.PointsOfInterest p
INNER JOIN dbo.NarrationLog n ON p.Id = n.PoiId
WHERE n.PlayedAt >= DATEADD(DAY, -90, SYSUTCDATETIME())
  AND p.IsActive = 1
GROUP BY p.Id, p.Name, p.Category, p.ImageUrl
ORDER BY TotalPlays DESC, p.Name;
GO

/* ---------------------------------------------------------------------
   vw_LanguageStats — phân bố lượt nghe theo ngôn ngữ TTS
   Dashboard pie chart.
   --------------------------------------------------------------------- */
IF OBJECT_ID('dbo.vw_LanguageStats', 'V') IS NOT NULL DROP VIEW dbo.vw_LanguageStats;
GO
CREATE VIEW dbo.vw_LanguageStats
AS
WITH Total AS (
    SELECT CAST(COUNT(*) AS FLOAT) AS Cnt FROM dbo.NarrationLog
)
SELECT
    n.Language,
    COUNT(*)                                            AS PlayCount,
    CAST(COUNT(*) * 100.0 / NULLIF((SELECT Cnt FROM Total), 0)
        AS DECIMAL(5,2))                                AS Percentage,
    COUNT(DISTINCT n.PoiId)                             AS UniquePois,
    COUNT(DISTINCT n.DeviceId)                          AS UniqueDevices,
    MIN(n.PlayedAt)                                     AS FirstPlayedAt,
    MAX(n.PlayedAt)                                     AS LastPlayedAt
FROM dbo.NarrationLog n
GROUP BY n.Language;
GO

/* ---------------------------------------------------------------------
   vw_DailyPlays — số lượt phát theo ngày, 30 ngày gần nhất
   Dashboard line chart.
   --------------------------------------------------------------------- */
IF OBJECT_ID('dbo.vw_DailyPlays', 'V') IS NOT NULL DROP VIEW dbo.vw_DailyPlays;
GO
CREATE VIEW dbo.vw_DailyPlays
AS
SELECT
    CAST(n.PlayedAt AS DATE)    AS PlayDate,
    COUNT(*)                    AS TotalPlays,
    COUNT(DISTINCT n.PoiId)     AS UniquePois,
    COUNT(DISTINCT n.DeviceId)  AS UniqueDevices,
    COUNT(DISTINCT n.Language)  AS LanguagesUsed
FROM dbo.NarrationLog n
WHERE n.PlayedAt >= DATEADD(DAY, -30, SYSUTCDATETIME())
GROUP BY CAST(n.PlayedAt AS DATE);
GO

/* ---------------------------------------------------------------------
   vw_UserStats — thống kê user cho dashboard overview
   Tổng user | mới 7 ngày | active 30 ngày | admin count
   --------------------------------------------------------------------- */
IF OBJECT_ID('dbo.vw_UserStats', 'V') IS NOT NULL DROP VIEW dbo.vw_UserStats;
GO
CREATE VIEW dbo.vw_UserStats
AS
SELECT
    COUNT(*)                                                              AS TotalUsers,
    SUM(CASE WHEN IsActive = 1 THEN 1 ELSE 0 END)                        AS ActiveUsers,
    SUM(CASE WHEN CreatedAt >= DATEADD(DAY, -7, SYSUTCDATETIME())
             THEN 1 ELSE 0 END)                                          AS NewLast7Days,
    SUM(CASE WHEN LastLoginAt >= DATEADD(DAY, -30, SYSUTCDATETIME())
             THEN 1 ELSE 0 END)                                          AS ActiveLast30Days,
    SUM(CASE WHEN Role = N'admin' AND IsActive = 1 THEN 1 ELSE 0 END)    AS AdminCount,
    SUM(CASE WHEN Role = N'user'  AND IsActive = 1 THEN 1 ELSE 0 END)    AS UserCount
FROM dbo.Users;
GO

PRINT N'[OK] Views.sql done. 5 views created.';
GO
