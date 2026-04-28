/* =====================================================================
   VinhKhanhGuide — StoredProcedures.sql (7 SPs)
   ---------------------------------------------------------------------
   POI:   sp_InsertPoi, sp_UpdatePoi, sp_SoftDeletePoi, sp_RestorePoi
   User:  sp_RegisterUser, sp_LoginUser
   Device: sp_UpsertPing
   ===================================================================== */

USE VinhKhanhGuide;
GO

/* =====================================================================
   1. sp_InsertPoi — thêm quán mới (web admin gọi)
   ===================================================================== */
IF OBJECT_ID('dbo.sp_InsertPoi', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_InsertPoi;
GO
CREATE PROCEDURE dbo.sp_InsertPoi
    @Name           NVARCHAR(150),
    @Category       NVARCHAR(20),
    @Latitude       FLOAT,
    @Longitude      FLOAT,
    @DescriptionVi  NVARCHAR(1000),
    @DescriptionEn  NVARCHAR(1000),
    @RadiusMeters   FLOAT          = 30,
    @Priority       INT            = 1,
    @DescriptionJa  NVARCHAR(1000) = NULL,
    @DescriptionKo  NVARCHAR(1000) = NULL,
    @DescriptionZh  NVARCHAR(1000) = NULL,
    @ImageUrl       NVARCHAR(500)  = NULL,
    @Address        NVARCHAR(300)  = NULL,
    @PhoneNumber    NVARCHAR(20)   = NULL,
    @OpeningHours   NVARCHAR(100)  = NULL,
    @PriceRange     NVARCHAR(50)   = NULL,
    @GoogleMapsLink NVARCHAR(1000) = NULL,
    @NewId          INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    -- Validate bắt buộc
    IF @Name IS NULL OR LEN(LTRIM(RTRIM(@Name))) = 0
    BEGIN RAISERROR(N'Name is required.', 16, 1); RETURN; END

    IF @Category NOT IN (N'Oc', N'Nuong', N'Lau', N'CaPhe', N'Khac')
    BEGIN RAISERROR(N'Invalid Category. Must be: Oc, Nuong, Lau, CaPhe, Khac.', 16, 1); RETURN; END

    IF @DescriptionVi IS NULL OR LEN(LTRIM(RTRIM(@DescriptionVi))) = 0
    BEGIN RAISERROR(N'DescriptionVi is required.', 16, 1); RETURN; END

    IF @DescriptionEn IS NULL OR LEN(LTRIM(RTRIM(@DescriptionEn))) = 0
    BEGIN RAISERROR(N'DescriptionEn is required.', 16, 1); RETURN; END

    INSERT INTO dbo.PointsOfInterest
        (Name, Category, Latitude, Longitude, RadiusMeters, Priority,
         DescriptionVi, DescriptionEn, DescriptionJa, DescriptionKo, DescriptionZh,
         ImageUrl, Address, PhoneNumber, OpeningHours, PriceRange, GoogleMapsLink)
    VALUES
        (@Name, @Category, @Latitude, @Longitude, @RadiusMeters, @Priority,
         @DescriptionVi, @DescriptionEn, @DescriptionJa, @DescriptionKo, @DescriptionZh,
         @ImageUrl, @Address, @PhoneNumber, @OpeningHours, @PriceRange, @GoogleMapsLink);

    SET @NewId = SCOPE_IDENTITY();
END
GO

/* =====================================================================
   2. sp_UpdatePoi — sửa quán (NULL = không đổi field đó)
   ===================================================================== */
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
    @GoogleMapsLink NVARCHAR(1000) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM dbo.PointsOfInterest WHERE Id = @Id)
    BEGIN RAISERROR(N'POI Id %d not found.', 16, 1, @Id); RETURN; END

    UPDATE dbo.PointsOfInterest
    SET
        Name           = COALESCE(@Name,           Name),
        Category       = COALESCE(@Category,       Category),
        Latitude       = COALESCE(@Latitude,       Latitude),
        Longitude      = COALESCE(@Longitude,      Longitude),
        RadiusMeters   = COALESCE(@RadiusMeters,   RadiusMeters),
        Priority       = COALESCE(@Priority,       Priority),
        DescriptionVi  = COALESCE(@DescriptionVi,  DescriptionVi),
        DescriptionEn  = COALESCE(@DescriptionEn,  DescriptionEn),
        DescriptionJa  = COALESCE(@DescriptionJa,  DescriptionJa),
        DescriptionKo  = COALESCE(@DescriptionKo,  DescriptionKo),
        DescriptionZh  = COALESCE(@DescriptionZh,  DescriptionZh),
        ImageUrl       = COALESCE(@ImageUrl,       ImageUrl),
        Address        = COALESCE(@Address,        Address),
        PhoneNumber    = COALESCE(@PhoneNumber,    PhoneNumber),
        OpeningHours   = COALESCE(@OpeningHours,   OpeningHours),
        PriceRange     = COALESCE(@PriceRange,     PriceRange),
        GoogleMapsLink = COALESCE(@GoogleMapsLink, GoogleMapsLink)
    WHERE Id = @Id;
    -- trigger tr_UpdateTimestamp sẽ tự set UpdatedAt
END
GO

/* =====================================================================
   3. sp_SoftDeletePoi — soft delete (giữ FK log)
   ===================================================================== */
IF OBJECT_ID('dbo.sp_SoftDeletePoi', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_SoftDeletePoi;
GO
CREATE PROCEDURE dbo.sp_SoftDeletePoi @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM dbo.PointsOfInterest WHERE Id = @Id)
    BEGIN RAISERROR(N'POI Id %d not found.', 16, 1, @Id); RETURN; END
    UPDATE dbo.PointsOfInterest SET IsActive = 0 WHERE Id = @Id;
END
GO

/* =====================================================================
   4. sp_RestorePoi — khôi phục
   ===================================================================== */
IF OBJECT_ID('dbo.sp_RestorePoi', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_RestorePoi;
GO
CREATE PROCEDURE dbo.sp_RestorePoi @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.PointsOfInterest SET IsActive = 1 WHERE Id = @Id;
END
GO

/* =====================================================================
   5. sp_RegisterUser — tạo tài khoản mới
   Web admin tạo user, hoặc app đăng ký.
   Password hash PHẢI được tạo ở tầng application (BCrypt).
   SP chỉ nhận hash đã sẵn — KHÔNG hash trong SQL.
   ===================================================================== */
IF OBJECT_ID('dbo.sp_RegisterUser', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_RegisterUser;
GO
CREATE PROCEDURE dbo.sp_RegisterUser
    @Username     NVARCHAR(50),
    @PasswordHash NVARCHAR(256),
    @DisplayName  NVARCHAR(100) = NULL,
    @Email        NVARCHAR(200) = NULL,
    @Role         NVARCHAR(20)  = N'user',
    @NewId        INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM dbo.Users WHERE Username = @Username)
    BEGIN RAISERROR(N'Username "%s" already exists.', 16, 1, @Username); RETURN; END

    IF @Role NOT IN (N'admin', N'user')
    BEGIN RAISERROR(N'Role must be "admin" or "user".', 16, 1); RETURN; END

    INSERT INTO dbo.Users (Username, PasswordHash, DisplayName, Email, Role)
    VALUES (@Username, @PasswordHash, @DisplayName, @Email, @Role);

    SET @NewId = SCOPE_IDENTITY();
END
GO

/* =====================================================================
   6. sp_LoginUser — verify login (trả về user info nếu đúng)
   So sánh password PHẢI ở tầng application (BCrypt.Verify).
   SP chỉ lookup username + trả hash để app so sánh.
   Nếu tìm thấy → update LastLoginAt.
   ===================================================================== */
IF OBJECT_ID('dbo.sp_LoginUser', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_LoginUser;
GO
CREATE PROCEDURE dbo.sp_LoginUser
    @Username NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @UserId INT;

    SELECT @UserId = Id
    FROM dbo.Users
    WHERE Username = @Username AND IsActive = 1;

    IF @UserId IS NULL
    BEGIN RAISERROR(N'Invalid username or account disabled.', 16, 1); RETURN; END

    -- Update last login
    UPDATE dbo.Users SET LastLoginAt = SYSUTCDATETIME() WHERE Id = @UserId;

    -- Return user info (app so sánh PasswordHash bằng BCrypt.Verify)
    SELECT Id, Username, PasswordHash, DisplayName, Email, Role, LastLoginAt
    FROM dbo.Users
    WHERE Id = @UserId;
END
GO

/* =====================================================================
   7. sp_UpsertPing — heartbeat từ app MAUI
   MERGE: insert mới nếu DeviceId chưa có, update nếu đã có.
   App gọi mỗi 30-60 giây.
   ===================================================================== */
IF OBJECT_ID('dbo.sp_UpsertPing', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_UpsertPing;
GO
CREATE PROCEDURE dbo.sp_UpsertPing
    @DeviceId   NVARCHAR(100),
    @UserId     INT            = NULL,
    @Platform   NVARCHAR(20)   = NULL,
    @AppVersion NVARCHAR(20)   = NULL
AS
BEGIN
    SET NOCOUNT ON;

    MERGE dbo.ActiveDevices AS target
    USING (SELECT @DeviceId AS DeviceId) AS source
    ON target.DeviceId = source.DeviceId
    WHEN MATCHED THEN
        UPDATE SET
            UserId      = COALESCE(@UserId, target.UserId),
            Platform    = COALESCE(@Platform, target.Platform),
            AppVersion  = COALESCE(@AppVersion, target.AppVersion),
            LastPingUtc = SYSUTCDATETIME()
    WHEN NOT MATCHED THEN
        INSERT (DeviceId, UserId, Platform, AppVersion, LastPingUtc)
        VALUES (@DeviceId, @UserId, @Platform, @AppVersion, SYSUTCDATETIME());
END
GO

PRINT N'[OK] StoredProcedures.sql done. 7 procedures created.';
GO
