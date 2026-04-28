-- ============================================================
-- VinhKhanhGuide — StoredProcedures.sql
-- Runs 3rd: 7 SPs (CRUD POI + User + Ping)
-- ============================================================

USE VinhKhanhGuide;
GO

-- ============================================================
-- sp_InsertPoi  — create new POI, returns new Id
-- ============================================================
IF OBJECT_ID('dbo.sp_InsertPoi', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_InsertPoi;
GO
CREATE PROCEDURE dbo.sp_InsertPoi
    @Name           NVARCHAR(150),
    @Category       NVARCHAR(20),
    @Latitude       FLOAT,
    @Longitude      FLOAT,
    @DescriptionVi  NVARCHAR(1000),
    @DescriptionEn  NVARCHAR(1000),
    @DescriptionJa  NVARCHAR(1000) = NULL,
    @DescriptionKo  NVARCHAR(1000) = NULL,
    @DescriptionZh  NVARCHAR(1000) = NULL,
    @RadiusMeters   FLOAT          = 30,
    @Priority       INT            = 1,
    @ImageUrl       NVARCHAR(500)  = NULL,
    @Address        NVARCHAR(300)  = NULL,
    @PhoneNumber    NVARCHAR(20)   = NULL,
    @OpeningHours   NVARCHAR(100)  = NULL,
    @PriceRange     NVARCHAR(50)   = NULL,
    @GoogleMapsLink NVARCHAR(1000) = NULL,
    @IsActive       BIT            = 1,
    @NewId          INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.PointsOfInterest
        (Name, Category, Latitude, Longitude, RadiusMeters, Priority,
         DescriptionVi, DescriptionEn, DescriptionJa, DescriptionKo, DescriptionZh,
         ImageUrl, Address, PhoneNumber, OpeningHours, PriceRange, GoogleMapsLink,
         IsActive, CreatedAt, UpdatedAt)
    VALUES
        (@Name, @Category, @Latitude, @Longitude, @RadiusMeters, @Priority,
         @DescriptionVi, @DescriptionEn, @DescriptionJa, @DescriptionKo, @DescriptionZh,
         @ImageUrl, @Address, @PhoneNumber, @OpeningHours, @PriceRange, @GoogleMapsLink,
         @IsActive, SYSUTCDATETIME(), SYSUTCDATETIME());

    SET @NewId = SCOPE_IDENTITY();
END
GO

-- ============================================================
-- sp_UpdatePoi  — update POI (NULL param = skip field)
-- ============================================================
IF OBJECT_ID('dbo.sp_UpdatePoi', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_UpdatePoi;
GO
CREATE PROCEDURE dbo.sp_UpdatePoi
    @Id             INT,
    @Name           NVARCHAR(150)  = NULL,
    @Category       NVARCHAR(20)   = NULL,
    @Latitude       FLOAT          = NULL,
    @Longitude      FLOAT          = NULL,
    @RadiusMeters   FLOAT          = NULL,
    @Priority       INT            = NULL,
    @DescriptionVi  NVARCHAR(1000) = NULL,
    @DescriptionEn  NVARCHAR(1000) = NULL,
    @DescriptionJa  NVARCHAR(1000) = NULL,
    @DescriptionKo  NVARCHAR(1000) = NULL,
    @DescriptionZh  NVARCHAR(1000) = NULL,
    @ImageUrl       NVARCHAR(500)  = NULL,
    @Address        NVARCHAR(300)  = NULL,
    @PhoneNumber    NVARCHAR(20)   = NULL,
    @OpeningHours   NVARCHAR(100)  = NULL,
    @PriceRange     NVARCHAR(50)   = NULL,
    @GoogleMapsLink NVARCHAR(1000) = NULL,
    @IsActive       BIT            = NULL
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.PointsOfInterest
    SET
        Name           = ISNULL(@Name,           Name),
        Category       = ISNULL(@Category,       Category),
        Latitude       = ISNULL(@Latitude,       Latitude),
        Longitude      = ISNULL(@Longitude,      Longitude),
        RadiusMeters   = ISNULL(@RadiusMeters,   RadiusMeters),
        Priority       = ISNULL(@Priority,       Priority),
        DescriptionVi  = ISNULL(@DescriptionVi,  DescriptionVi),
        DescriptionEn  = ISNULL(@DescriptionEn,  DescriptionEn),
        DescriptionJa  = ISNULL(@DescriptionJa,  DescriptionJa),
        DescriptionKo  = ISNULL(@DescriptionKo,  DescriptionKo),
        DescriptionZh  = ISNULL(@DescriptionZh,  DescriptionZh),
        ImageUrl       = ISNULL(@ImageUrl,       ImageUrl),
        Address        = ISNULL(@Address,        Address),
        PhoneNumber    = ISNULL(@PhoneNumber,    PhoneNumber),
        OpeningHours   = ISNULL(@OpeningHours,   OpeningHours),
        PriceRange     = ISNULL(@PriceRange,     PriceRange),
        GoogleMapsLink = ISNULL(@GoogleMapsLink, GoogleMapsLink),
        IsActive       = ISNULL(@IsActive,       IsActive)
    WHERE Id = @Id;

    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

-- ============================================================
-- sp_SoftDeletePoi
-- ============================================================
IF OBJECT_ID('dbo.sp_SoftDeletePoi', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_SoftDeletePoi;
GO
CREATE PROCEDURE dbo.sp_SoftDeletePoi
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.PointsOfInterest
    SET IsActive = 0
    WHERE Id = @Id;
    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

-- ============================================================
-- sp_RestorePoi
-- ============================================================
IF OBJECT_ID('dbo.sp_RestorePoi', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_RestorePoi;
GO
CREATE PROCEDURE dbo.sp_RestorePoi
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.PointsOfInterest
    SET IsActive = 1
    WHERE Id = @Id;
    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

-- ============================================================
-- sp_RegisterUser
-- ============================================================
IF OBJECT_ID('dbo.sp_RegisterUser', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_RegisterUser;
GO
CREATE PROCEDURE dbo.sp_RegisterUser
    @Username     NVARCHAR(50),
    @PasswordHash NVARCHAR(256),
    @Role         NVARCHAR(20)  = N'user',
    @DisplayName  NVARCHAR(100) = NULL,
    @Email        NVARCHAR(200) = NULL,
    @NewId        INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM dbo.Users WHERE Username = @Username)
    BEGIN
        SET @NewId = -1;  -- duplicate
        RETURN;
    END

    INSERT INTO dbo.Users (Username, PasswordHash, Role, DisplayName, Email, CreatedAt, IsActive)
    VALUES (@Username, @PasswordHash, @Role, @DisplayName, @Email, SYSUTCDATETIME(), 1);

    SET @NewId = SCOPE_IDENTITY();
END
GO

-- ============================================================
-- sp_LoginUser  — returns user row (with PasswordHash) — app verifies via BCrypt
-- ============================================================
IF OBJECT_ID('dbo.sp_LoginUser', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_LoginUser;
GO
CREATE PROCEDURE dbo.sp_LoginUser
    @Username NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT Id, Username, PasswordHash, DisplayName, Email, Role, IsActive, LastLoginAt
    FROM dbo.Users
    WHERE Username = @Username AND IsActive = 1;
END
GO

-- ============================================================
-- sp_UpsertPing  — upsert heartbeat from mobile device
-- ============================================================
IF OBJECT_ID('dbo.sp_UpsertPing', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_UpsertPing;
GO
CREATE PROCEDURE dbo.sp_UpsertPing
    @DeviceId   NVARCHAR(100),
    @UserId     INT           = NULL,
    @Platform   NVARCHAR(20)  = NULL,
    @AppVersion NVARCHAR(20)  = NULL
AS
BEGIN
    SET NOCOUNT ON;

    MERGE dbo.ActiveDevices AS t
    USING (SELECT @DeviceId AS DeviceId) AS s
    ON (t.DeviceId = s.DeviceId)
    WHEN MATCHED THEN
        UPDATE SET
            UserId = ISNULL(@UserId, t.UserId),
            Platform = ISNULL(@Platform, t.Platform),
            AppVersion = ISNULL(@AppVersion, t.AppVersion),
            LastPingUtc = SYSUTCDATETIME()
    WHEN NOT MATCHED THEN
        INSERT (DeviceId, UserId, Platform, AppVersion, LastPingUtc)
        VALUES (@DeviceId, @UserId, @Platform, @AppVersion, SYSUTCDATETIME());
END
GO

PRINT N'✓ StoredProcedures.sql completed: 7 SPs created';
