/* =====================================================================
   VinhKhanhGuide — CreateDatabase.sql (MAUI + Web Admin)
   ---------------------------------------------------------------------
   Kiến trúc:
   [App MAUI] ──HTTP──> [Web Admin API] ──SQL──> [SQL Server DB]
                         [Web Admin UI]  ──SQL──> [SQL Server DB]

   4 bảng: PointsOfInterest, Users, ActiveDevices, NarrationLog
   + CHECK constraints + indexes + trigger UpdatedAt
   ---------------------------------------------------------------------
   Chạy: SSMS → kết nối (local) → mở file → F5
   ===================================================================== */

USE master;
GO

IF DB_ID('VinhKhanhGuide') IS NOT NULL
BEGIN
    ALTER DATABASE VinhKhanhGuide SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE VinhKhanhGuide;
END
GO

CREATE DATABASE VinhKhanhGuide COLLATE Vietnamese_CI_AS;
GO
USE VinhKhanhGuide;
GO

/* =====================================================================
   1. PointsOfInterest — bảng quán ăn
   ===================================================================== */
CREATE TABLE dbo.PointsOfInterest (
    Id              INT IDENTITY(1,1)   NOT NULL,
    Name            NVARCHAR(150)       NOT NULL,
    Category        NVARCHAR(20)        NOT NULL,
    Latitude        FLOAT               NOT NULL,
    Longitude       FLOAT               NOT NULL,
    RadiusMeters    FLOAT               NOT NULL DEFAULT 30,
    Priority        INT                 NOT NULL DEFAULT 1,

    -- 5 ngôn ngữ (TTS đọc trực tiếp — plain text, KHÔNG markdown/HTML/emoji)
    DescriptionVi   NVARCHAR(1000)      NOT NULL,
    DescriptionEn   NVARCHAR(1000)      NOT NULL,
    DescriptionJa   NVARCHAR(1000)      NULL,
    DescriptionKo   NVARCHAR(1000)      NULL,
    DescriptionZh   NVARCHAR(1000)      NULL,

    -- Thông tin bổ sung (web admin nhập)
    ImageUrl        NVARCHAR(500)       NULL,   -- URL tuyệt đối từ web upload
    Address         NVARCHAR(300)       NULL,
    PhoneNumber     NVARCHAR(20)        NULL,
    OpeningHours    NVARCHAR(100)       NULL,
    PriceRange      NVARCHAR(50)        NULL,
    GoogleMapsLink  NVARCHAR(1000)      NULL,

    IsActive        BIT                 NOT NULL DEFAULT 1,
    CreatedAt       DATETIME2           NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt       DATETIME2           NULL,

    CONSTRAINT PK_PointsOfInterest PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT CK_Category  CHECK (Category IN (N'Oc', N'Nuong', N'Lau', N'CaPhe', N'Khac')),
    CONSTRAINT CK_Lat       CHECK (Latitude BETWEEN -90 AND 90),
    CONSTRAINT CK_Lon       CHECK (Longitude BETWEEN -180 AND 180),
    CONSTRAINT CK_Radius    CHECK (RadiusMeters > 0 AND RadiusMeters <= 500),
    CONSTRAINT CK_Priority  CHECK (Priority BETWEEN 1 AND 10)
);
GO

/* =====================================================================
   2. Users — tài khoản đăng nhập app + web admin
   ===================================================================== */
CREATE TABLE dbo.Users (
    Id              INT IDENTITY(1,1)   NOT NULL,
    Username        NVARCHAR(50)        NOT NULL,
    PasswordHash    NVARCHAR(256)       NOT NULL,
    DisplayName     NVARCHAR(100)       NULL,
    Email           NVARCHAR(200)       NULL,
    Role            NVARCHAR(20)        NOT NULL DEFAULT N'user',
    CreatedAt       DATETIME2           NOT NULL DEFAULT SYSUTCDATETIME(),
    LastLoginAt     DATETIME2           NULL,
    IsActive        BIT                 NOT NULL DEFAULT 1,

    CONSTRAINT PK_Users PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_Users_Username UNIQUE (Username),
    CONSTRAINT CK_Role CHECK (Role IN (N'admin', N'user'))
);
GO

/* =====================================================================
   3. ActiveDevices — heartbeat từ app MAUI (online tracking)
   ===================================================================== */
CREATE TABLE dbo.ActiveDevices (
    DeviceId        NVARCHAR(100)       NOT NULL,
    UserId          INT                 NULL,
    Platform        NVARCHAR(20)        NULL,   -- 'Android', 'iOS', 'Windows'
    AppVersion      NVARCHAR(20)        NULL,
    LastPingUtc     DATETIME2           NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT PK_ActiveDevices PRIMARY KEY CLUSTERED (DeviceId),
    CONSTRAINT FK_ActiveDevices_User
        FOREIGN KEY (UserId) REFERENCES dbo.Users(Id)
);
GO

/* =====================================================================
   4. NarrationLog — log mỗi lần TTS phát
   ===================================================================== */
CREATE TABLE dbo.NarrationLog (
    Id              BIGINT IDENTITY(1,1) NOT NULL,
    PoiId           INT                  NOT NULL,
    DeviceId        NVARCHAR(100)        NULL,
    Language        NVARCHAR(20)         NOT NULL,
    PlayedAt        DATETIME2            NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT PK_NarrationLog PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_NarrationLog_Poi
        FOREIGN KEY (PoiId) REFERENCES dbo.PointsOfInterest(Id),
    CONSTRAINT CK_NarrationLog_Language
        CHECK (Language IN (N'Vietnamese', N'English', N'Japanese', N'Korean', N'Chinese'))
);
GO

/* =====================================================================
   5. INDEXES
   ===================================================================== */

-- POI: filter category + active (app lọc liên tục)
CREATE NONCLUSTERED INDEX IX_POI_Category_Active
    ON dbo.PointsOfInterest (Category, IsActive)
    INCLUDE (Name, Latitude, Longitude, RadiusMeters, Priority);
GO

-- POI: sort default (Priority DESC)
CREATE NONCLUSTERED INDEX IX_POI_Active_Priority
    ON dbo.PointsOfInterest (IsActive, Priority DESC, Id);
GO

-- POI: geofence range scan
CREATE NONCLUSTERED INDEX IX_POI_Location
    ON dbo.PointsOfInterest (Latitude, Longitude)
    WHERE IsActive = 1;
GO

-- Log: top-N per POI
CREATE NONCLUSTERED INDEX IX_Log_PoiId_PlayedAt
    ON dbo.NarrationLog (PoiId, PlayedAt DESC);
GO

-- Log: time-range (daily plays chart)
CREATE NONCLUSTERED INDEX IX_Log_PlayedAt
    ON dbo.NarrationLog (PlayedAt DESC)
    INCLUDE (PoiId, Language, DeviceId);
GO

-- ActiveDevices: tìm online (LastPingUtc gần đây)
CREATE NONCLUSTERED INDEX IX_ActiveDevices_LastPing
    ON dbo.ActiveDevices (LastPingUtc DESC)
    INCLUDE (UserId, Platform);
GO

-- Users: login lookup
CREATE NONCLUSTERED INDEX IX_Users_Username_Active
    ON dbo.Users (Username, IsActive)
    INCLUDE (PasswordHash, Role, DisplayName);
GO

/* =====================================================================
   6. TRIGGER — auto UpdatedAt khi UPDATE PointsOfInterest
   ===================================================================== */
CREATE TRIGGER dbo.tr_UpdateTimestamp
ON dbo.PointsOfInterest
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT UPDATE(UpdatedAt)
    BEGIN
        UPDATE p
        SET UpdatedAt = SYSUTCDATETIME()
        FROM dbo.PointsOfInterest p
        INNER JOIN inserted i ON p.Id = i.Id;
    END
END
GO

PRINT N'[OK] CreateDatabase.sql done.';
PRINT N'     4 tables + 7 indexes + 1 trigger created.';
PRINT N'     Next: Views.sql -> StoredProcedures.sql -> SeedData.sql';
GO
