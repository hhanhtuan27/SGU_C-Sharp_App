-- ============================================================
-- VinhKhanhGuide — CreateDatabase.sql
-- Runs 1st: drop & create DB, 4 tables, indexes, trigger
-- ============================================================

USE master;
GO

IF DB_ID(N'VinhKhanhGuide') IS NOT NULL
BEGIN
    ALTER DATABASE VinhKhanhGuide SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE VinhKhanhGuide;
END
GO

CREATE DATABASE VinhKhanhGuide
COLLATE Vietnamese_CI_AS;
GO

USE VinhKhanhGuide;
GO

-- ============================================================
-- TABLE: Users
-- ============================================================
CREATE TABLE dbo.Users (
    Id              INT IDENTITY(1,1) NOT NULL,
    Username        NVARCHAR(50)      NOT NULL,
    PasswordHash    NVARCHAR(256)     NOT NULL,
    DisplayName     NVARCHAR(100)     NULL,
    Email           NVARCHAR(200)     NULL,
    Role            NVARCHAR(20)      NOT NULL DEFAULT N'user',
    CreatedAt       DATETIME2         NOT NULL DEFAULT SYSUTCDATETIME(),
    LastLoginAt     DATETIME2         NULL,
    IsActive        BIT               NOT NULL DEFAULT 1,
    CONSTRAINT PK_Users        PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_Users_Username UNIQUE (Username),
    CONSTRAINT CK_Users_Role   CHECK (Role IN (N'admin', N'user'))
);
GO

CREATE NONCLUSTERED INDEX IX_Users_Active_Role
    ON dbo.Users (IsActive, Role)
    INCLUDE (Username, DisplayName, LastLoginAt);
GO

-- ============================================================
-- TABLE: PointsOfInterest
-- ============================================================
CREATE TABLE dbo.PointsOfInterest (
    Id              INT IDENTITY(1,1) NOT NULL,
    Name            NVARCHAR(150)     NOT NULL,
    Category        NVARCHAR(20)      NOT NULL,
    Latitude        FLOAT             NOT NULL,
    Longitude       FLOAT             NOT NULL,
    RadiusMeters    FLOAT             NOT NULL DEFAULT 30,
    Priority        INT               NOT NULL DEFAULT 1,

    DescriptionVi   NVARCHAR(1000)    NOT NULL,
    DescriptionEn   NVARCHAR(1000)    NOT NULL,
    DescriptionJa   NVARCHAR(1000)    NULL,
    DescriptionKo   NVARCHAR(1000)    NULL,
    DescriptionZh   NVARCHAR(1000)    NULL,

    ImageUrl        NVARCHAR(500)     NULL,
    Address         NVARCHAR(300)     NULL,
    PhoneNumber     NVARCHAR(20)      NULL,
    OpeningHours    NVARCHAR(100)     NULL,
    PriceRange      NVARCHAR(50)      NULL,
    GoogleMapsLink  NVARCHAR(1000)    NULL,

    IsActive        BIT               NOT NULL DEFAULT 1,
    CreatedAt       DATETIME2         NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt       DATETIME2         NULL,

    CONSTRAINT PK_POI          PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT CK_POI_Category CHECK (Category IN (N'Oc', N'Nuong', N'Lau', N'CaPhe', N'Khac')),
    CONSTRAINT CK_POI_Lat      CHECK (Latitude  BETWEEN -90  AND 90),
    CONSTRAINT CK_POI_Lon      CHECK (Longitude BETWEEN -180 AND 180),
    CONSTRAINT CK_POI_Radius   CHECK (RadiusMeters > 0 AND RadiusMeters <= 500),
    CONSTRAINT CK_POI_Priority CHECK (Priority BETWEEN 1 AND 10)
);
GO

CREATE NONCLUSTERED INDEX IX_POI_Category_Active
    ON dbo.PointsOfInterest (Category, IsActive)
    INCLUDE (Name, Latitude, Longitude, RadiusMeters, Priority, ImageUrl);
GO

CREATE NONCLUSTERED INDEX IX_POI_Active_Priority
    ON dbo.PointsOfInterest (IsActive, Priority DESC, Id)
    INCLUDE (Name, Category, Latitude, Longitude);
GO

CREATE NONCLUSTERED INDEX IX_POI_UpdatedAt
    ON dbo.PointsOfInterest (UpdatedAt DESC)
    WHERE IsActive = 1;
GO

-- Trigger auto-update UpdatedAt
CREATE TRIGGER tr_POI_UpdateTimestamp
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

-- ============================================================
-- TABLE: ActiveDevices  (mobile heartbeat tracking)
-- ============================================================
CREATE TABLE dbo.ActiveDevices (
    DeviceId        NVARCHAR(100)     NOT NULL,
    UserId          INT               NULL,
    Platform        NVARCHAR(20)      NULL,
    AppVersion      NVARCHAR(20)      NULL,
    LastPingUtc     DATETIME2         NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_ActiveDevices PRIMARY KEY CLUSTERED (DeviceId),
    CONSTRAINT FK_ActiveDevices_User
        FOREIGN KEY (UserId) REFERENCES dbo.Users(Id)
        ON DELETE NO ACTION,
    CONSTRAINT CK_ActiveDevices_Platform
        CHECK (Platform IS NULL OR Platform IN (N'Android', N'iOS', N'Windows'))
);
GO

CREATE NONCLUSTERED INDEX IX_ActiveDevices_LastPing
    ON dbo.ActiveDevices (LastPingUtc DESC)
    INCLUDE (Platform, UserId);
GO

-- ============================================================
-- TABLE: NarrationLog  (analytics: which POI, which language, when)
-- ============================================================
CREATE TABLE dbo.NarrationLog (
    Id              BIGINT IDENTITY(1,1) NOT NULL,
    PoiId           INT               NOT NULL,
    DeviceId        NVARCHAR(100)     NULL,
    Language        NVARCHAR(20)      NOT NULL,
    PlayedAt        DATETIME2         NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_NarrationLog PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_NarrationLog_Poi
        FOREIGN KEY (PoiId) REFERENCES dbo.PointsOfInterest(Id)
        ON DELETE NO ACTION,
    CONSTRAINT CK_NarrationLog_Language
        CHECK (Language IN (N'Vietnamese', N'English', N'Japanese', N'Korean', N'Chinese'))
);
GO

CREATE NONCLUSTERED INDEX IX_NarrationLog_PoiId_PlayedAt
    ON dbo.NarrationLog (PoiId, PlayedAt DESC);
GO

CREATE NONCLUSTERED INDEX IX_NarrationLog_PlayedAt
    ON dbo.NarrationLog (PlayedAt DESC)
    INCLUDE (PoiId, Language);
GO

PRINT N'✓ CreateDatabase.sql completed: 4 tables, 7 indexes, 1 trigger';
