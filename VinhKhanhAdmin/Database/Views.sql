-- ============================================================
-- VinhKhanhGuide — Views.sql
-- Runs 2nd: 5 dashboard views
-- ============================================================

USE VinhKhanhGuide;
GO

-- ============================================================
-- vw_OnlineUsers  — devices pinging in last 5 minutes
-- ============================================================
IF OBJECT_ID('dbo.vw_OnlineUsers', 'V') IS NOT NULL DROP VIEW dbo.vw_OnlineUsers;
GO
CREATE VIEW dbo.vw_OnlineUsers AS
SELECT
    d.DeviceId,
    d.UserId,
    u.Username,
    u.DisplayName,
    d.Platform,
    d.AppVersion,
    d.LastPingUtc,
    DATEDIFF(SECOND, d.LastPingUtc, SYSUTCDATETIME()) AS SecondsSincePing
FROM dbo.ActiveDevices d
LEFT JOIN dbo.Users u ON d.UserId = u.Id
WHERE d.LastPingUtc >= DATEADD(MINUTE, -5, SYSUTCDATETIME());
GO

-- ============================================================
-- vw_TopPlayedPois  — most played POIs in last 90 days
-- ============================================================
IF OBJECT_ID('dbo.vw_TopPlayedPois', 'V') IS NOT NULL DROP VIEW dbo.vw_TopPlayedPois;
GO
CREATE VIEW dbo.vw_TopPlayedPois AS
SELECT TOP 100
    p.Id,
    p.Name,
    p.Category,
    p.ImageUrl,
    p.Latitude,
    p.Longitude,
    COUNT(n.Id) AS PlayCount,
    COUNT(DISTINCT n.DeviceId) AS UniqueDevices,
    MAX(n.PlayedAt) AS LastPlayedAt
FROM dbo.PointsOfInterest p
LEFT JOIN dbo.NarrationLog n
    ON n.PoiId = p.Id
    AND n.PlayedAt >= DATEADD(DAY, -90, SYSUTCDATETIME())
WHERE p.IsActive = 1
GROUP BY p.Id, p.Name, p.Category, p.ImageUrl, p.Latitude, p.Longitude
ORDER BY PlayCount DESC, p.Id;
GO

-- ============================================================
-- vw_LanguageStats  — TTS language distribution (30 days)
-- ============================================================
IF OBJECT_ID('dbo.vw_LanguageStats', 'V') IS NOT NULL DROP VIEW dbo.vw_LanguageStats;
GO
CREATE VIEW dbo.vw_LanguageStats AS
SELECT
    Language,
    COUNT(*) AS PlayCount,
    COUNT(DISTINCT DeviceId) AS UniqueDevices,
    COUNT(DISTINCT PoiId) AS UniquePois
FROM dbo.NarrationLog
WHERE PlayedAt >= DATEADD(DAY, -30, SYSUTCDATETIME())
GROUP BY Language;
GO

-- ============================================================
-- vw_DailyPlays  — plays per day (last 30 days)
-- ============================================================
IF OBJECT_ID('dbo.vw_DailyPlays', 'V') IS NOT NULL DROP VIEW dbo.vw_DailyPlays;
GO
CREATE VIEW dbo.vw_DailyPlays AS
SELECT
    CAST(PlayedAt AS DATE) AS PlayDate,
    COUNT(*) AS PlayCount,
    COUNT(DISTINCT DeviceId) AS UniqueDevices,
    COUNT(DISTINCT PoiId) AS UniquePois
FROM dbo.NarrationLog
WHERE PlayedAt >= DATEADD(DAY, -30, SYSUTCDATETIME())
GROUP BY CAST(PlayedAt AS DATE);
GO

-- ============================================================
-- vw_UserStats  — user overview
-- ============================================================
IF OBJECT_ID('dbo.vw_UserStats', 'V') IS NOT NULL DROP VIEW dbo.vw_UserStats;
GO
CREATE VIEW dbo.vw_UserStats AS
SELECT
    COUNT(*) AS TotalUsers,
    SUM(CASE WHEN IsActive = 1 THEN 1 ELSE 0 END) AS ActiveUsers,
    SUM(CASE WHEN Role = N'admin' THEN 1 ELSE 0 END) AS AdminUsers,
    SUM(CASE WHEN CreatedAt >= DATEADD(DAY, -7, SYSUTCDATETIME()) THEN 1 ELSE 0 END) AS NewUsers7d,
    SUM(CASE WHEN LastLoginAt >= DATEADD(DAY, -30, SYSUTCDATETIME()) THEN 1 ELSE 0 END) AS ActiveUsers30d
FROM dbo.Users;
GO

PRINT N'✓ Views.sql completed: 5 views created';
