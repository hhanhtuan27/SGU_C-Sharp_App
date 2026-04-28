-- ============================================================
-- VinhKhanhGuide — SeedData.sql
-- Runs 4th: 20 real POIs + narration demo logs + active devices
-- NOTE: Users (admin/demo) are seeded by C# DataSeeder on first run
--       using BCrypt.Net-Next to ensure hash compatibility.
-- ============================================================

USE VinhKhanhGuide;
GO

SET NOCOUNT ON;



-- ============================================================
-- POIs (20 real places from Vinh Khanh street food area)
-- ============================================================
DELETE FROM dbo.NarrationLog;
DELETE FROM dbo.PointsOfInterest;
DBCC CHECKIDENT ('dbo.PointsOfInterest', RESEED, 0);
GO

INSERT INTO dbo.PointsOfInterest
    (Name, Category, Latitude, Longitude, RadiusMeters, Priority,
     DescriptionVi, DescriptionEn, DescriptionJa, DescriptionKo, DescriptionZh,
     Address, OpeningHours, PriceRange, IsActive)
VALUES
-- === ỐC (8 quán) ===
(N'Ốc Oanh', N'Oc', 10.760719, 106.703297, 30, 5,
 N'Ốc Oanh là quán ốc huyền thoại của khu Vĩnh Khánh, nổi tiếng từ những năm 90. Nước chấm đặc trưng cay ngọt và ốc luôn tươi rói.',
 N'Oc Oanh is the legendary snail restaurant of the Vinh Khanh area, famous since the 90s. Known for its distinctive sweet and spicy dipping sauce and always-fresh snails.',
 N'オックオアインはヴィンカイン地区の伝説的な貝料理店です。90年代から有名で、独特の甘辛いタレと新鮮な貝が自慢です。',
 NULL, NULL,
 N'534 Vĩnh Khánh, Phường 8, Quận 4, TP.HCM', N'16:00 - 23:30', N'50k - 150k', 1),

(N'Quán Ốc 662', N'Oc', 10.760836, 106.703505, 25, 4,
 N'Quán Ốc 662 với menu đa dạng hơn 30 loại ốc các loại. Không gian rộng rãi, phục vụ nhanh, phù hợp cho nhóm đông.',
 N'Quan Oc 662 offers a diverse menu of over 30 snail varieties. Spacious setting, fast service, great for groups.',
 NULL, NULL, NULL,
 N'662 Vĩnh Khánh, Phường 8, Quận 4, TP.HCM', N'17:00 - 24:00', N'40k - 120k', 1),

(N'Ốc Đào 2', N'Oc', 10.761137, 106.704979, 25, 3,
 N'Ốc Đào 2 là chi nhánh mở rộng của Ốc Đào truyền thống. Đặc biệt với món ốc cháy tỏi thơm nức mũi và giá cả phải chăng.',
 N'Oc Dao 2 is the expanded branch of the classic Oc Dao. Famous for its fragrant garlic-fried snails at reasonable prices.',
 NULL, NULL, NULL,
 N'Vĩnh Khánh, Phường 8, Quận 4, TP.HCM', N'16:30 - 23:00', N'30k - 100k', 1),

(N'Quán Ốc Bụi', N'Oc', 10.760597, 106.704641, 20, 3,
 N'Quán ốc Bụi với phong cách bình dân, không gian vỉa hè đậm chất Sài Gòn. Món tủ là ốc móng tay xào bơ tỏi.',
 N'Quan Oc Bui offers street-style dining with an authentic Saigon sidewalk atmosphere. Signature dish: razor clams with garlic butter.',
 NULL, NULL, NULL,
 N'Vĩnh Khánh, Phường 8, Quận 4, TP.HCM', N'17:00 - 23:00', N'30k - 90k', 1),

(N'Ốc Hoa', N'Oc', 10.760713, 106.704217, 20, 3,
 N'Ốc Hoa nổi tiếng với món ốc len xào dừa béo ngậy. Không gian thoáng đãng, phục vụ thân thiện.',
 N'Oc Hoa is renowned for its creamy coconut-stewed sea snails. Airy space and friendly service.',
 NULL, NULL, NULL,
 N'Vĩnh Khánh, Phường 8, Quận 4, TP.HCM', N'16:00 - 23:00', N'35k - 110k', 1),

(N'Quán Ốc Sáu Nở', N'Oc', 10.760964, 106.702942, 25, 3,
 N'Quán Ốc Sáu Nở là quán gia đình truyền thống. Đặc trưng là món sò huyết nướng mỡ hành và ốc bươu nhồi thịt.',
 N'Quan Oc Sau No is a traditional family restaurant, specializing in grilled blood cockles with scallion oil and stuffed apple snails.',
 NULL, NULL, NULL,
 N'Vĩnh Khánh, Phường 8, Quận 4, TP.HCM', N'17:00 - 23:30', N'40k - 120k', 1),

(N'Quán Ốc Vũ', N'Oc', 10.761403, 106.702705, 20, 2,
 N'Quán Ốc Vũ phục vụ các món ốc biển tươi sống nhập về mỗi ngày từ Vũng Tàu. Giá cả trung bình khá.',
 N'Quan Oc Vu serves fresh sea snails imported daily from Vung Tau. Mid-range pricing.',
 NULL, NULL, NULL,
 N'Vĩnh Khánh, Phường 8, Quận 4, TP.HCM', N'16:00 - 23:00', N'50k - 150k', 1),

(N'Ốc Phát - Ốc Ngon Quận 4', N'Oc', 10.761955, 106.702094, 25, 3,
 N'Ốc Phát tự hào là ốc ngon quận 4 với công thức gia truyền. Nổi bật có món ốc hương nướng muối ớt.',
 N'Oc Phat takes pride in being the best snails of District 4 with heirloom recipes. Specialty: grilled sweet snails with salt and chili.',
 NULL, NULL, NULL,
 N'Vĩnh Khánh, Phường 8, Quận 4, TP.HCM', N'16:30 - 23:30', N'40k - 130k', 1),

-- === NƯỚNG (4 quán) ===
(N'Tiệm Nướng 10 Năm', N'Nuong', 10.760537, 106.703528, 30, 4,
 N'Tiệm Nướng 10 Năm - một thập kỷ chinh phục khẩu vị dân Sài Gòn. Đặc sản sườn nướng mật ong và bò cuộn phô mai.',
 N'Tiem Nuong 10 Nam — a decade of winning Saigon hearts. Signature: honey-grilled ribs and cheese-wrapped beef.',
 NULL, NULL, NULL,
 N'Vĩnh Khánh, Phường 8, Quận 4, TP.HCM', N'17:00 - 23:00', N'60k - 200k', 1),

(N'Hàu Nướng A Trung', N'Nuong', 10.760669, 106.703673, 20, 4,
 N'Hàu Nướng A Trung chuyên về hàu Pháp tươi sống, nướng mỡ hành, phô mai hoặc sốt bơ tỏi.',
 N'Hau Nuong A Trung specializes in fresh French oysters grilled with scallion oil, cheese, or garlic butter sauce.',
 NULL, NULL, NULL,
 N'Vĩnh Khánh, Phường 8, Quận 4, TP.HCM', N'17:00 - 23:30', N'80k - 250k', 1),

(N'Trạm Nướng BBQ', N'Nuong', 10.760728, 106.704679, 25, 3,
 N'Trạm Nướng BBQ với không gian industrial hiện đại, menu BBQ đa dạng kiểu Hàn, Mỹ, Việt.',
 N'Tram Nuong BBQ features modern industrial decor with Korean-American-Vietnamese BBQ fusion. Great for birthday parties.',
 NULL, NULL, NULL,
 N'Vĩnh Khánh, Phường 8, Quận 4, TP.HCM', N'17:00 - 22:30', N'100k - 300k', 1),

(N'Thềm Nướng Yakiniku', N'Nuong', 10.760778, 106.704739, 25, 3,
 N'Thềm Nướng Yakiniku mang đến trải nghiệm BBQ Nhật chuẩn vị với thịt bò Wagyu và sốt tare nhà làm.',
 N'Them Nuong Yakiniku brings authentic Japanese BBQ with Wagyu beef and house-made tare sauce. Cozy izakaya atmosphere.',
 N'テムヌオン焼肉では和牛と自家製タレを使った本格的な日本の焼肉をお楽しみいただけます。',
 NULL, NULL,
 N'Vĩnh Khánh, Phường 8, Quận 4, TP.HCM', N'17:30 - 23:00', N'150k - 400k', 1),

-- === LẨU (3 quán) ===
(N'Lẩu Mẹt Nướng 79k', N'Lau', 10.760806, 106.704310, 30, 3,
 N'Lẩu Mẹt Nướng 79k - combo lẩu kèm nướng siêu tiết kiệm chỉ từ 79k. Phong cách phục vụ trên mẹt tre truyền thống.',
 N'Lau Met Nuong 79k offers amazing hotpot and grill combos from just 79k VND, served on traditional bamboo trays.',
 NULL, NULL, NULL,
 N'Vĩnh Khánh, Phường 8, Quận 4, TP.HCM', N'16:00 - 23:00', N'79k - 200k', 1),

(N'Lẩu Bò Kỳ Kim', N'Lau', 10.761460, 106.702608, 30, 3,
 N'Lẩu Bò Kỳ Kim nổi tiếng với nồi nước dùng hầm xương bò 12 tiếng, vị thanh ngọt đặc trưng và thịt bò nhập khẩu Úc.',
 N'Lau Bo Ky Kim is famous for its 12-hour bone broth with a distinctive clean-sweet flavor and imported Australian beef.',
 NULL, NULL, NULL,
 N'Vĩnh Khánh, Phường 8, Quận 4, TP.HCM', N'17:00 - 23:00', N'150k - 350k', 1),

(N'Lẩu gà lá é Con Gà Trống', N'Lau', 10.760856, 106.706722, 30, 4,
 N'Lẩu gà lá é Con Gà Trống mang đặc sản Phú Yên giữa lòng Sài Gòn. Gà thả vườn kết hợp lá é tạo hương vị khó quên.',
 N'Lau Ga La E Con Ga Trong brings Phu Yen specialty to Saigon. Free-range chicken with la e leaves creates unforgettable flavor.',
 NULL, NULL, NULL,
 N'Vĩnh Khánh, Phường 8, Quận 4, TP.HCM', N'16:30 - 22:30', N'120k - 280k', 1),

-- === CÀ PHÊ (3 quán) ===
(N'Tiệm cà phê Lucky', N'CaPhe', 10.760451, 106.707005, 20, 2,
 N'Tiệm cà phê Lucky với không gian vintage xinh xắn, phù hợp làm việc từ quán. Cà phê rang xay tại chỗ và có bánh ngọt handmade.',
 N'Lucky Cafe features a charming vintage space perfect for working. On-site roasted coffee and handmade pastries.',
 NULL, NULL, NULL,
 N'Vĩnh Khánh, Phường 8, Quận 4, TP.HCM', N'07:00 - 22:00', N'25k - 60k', 1),

(N'Xù Phê', N'CaPhe', 10.761201, 106.706756, 20, 2,
 N'Xù Phê là quán cà phê sân vườn yên tĩnh, có khu vực nuôi thú cưng. Menu đa dạng từ cà phê đặc sản đến trà trái cây.',
 N'Xu Phe is a quiet garden cafe with a pet corner, offering specialty coffee and fruit teas.',
 NULL, NULL, NULL,
 N'Vĩnh Khánh, Phường 8, Quận 4, TP.HCM', N'08:00 - 22:30', N'30k - 70k', 1),

(N'Link Coffee and Tea', N'CaPhe', 10.760846, 106.704983, 20, 2,
 N'Link Coffee và Tea với phong cách Hàn Quốc hiện đại, view sống ảo cực chất. Signature là latte nghệ thuật và matcha đá xay.',
 N'Link Coffee and Tea features modern Korean style with Instagram-worthy views. Signature: latte art and matcha frappe.',
 NULL, NULL, NULL,
 N'Vĩnh Khánh, Phường 8, Quận 4, TP.HCM', N'07:30 - 22:00', N'35k - 75k', 1),

-- === KHÁC (2 quán) ===
(N'Bánh Flan Ngọc Nga', N'Khac', 10.760892, 106.702450, 15, 3,
 N'Bánh Flan Ngọc Nga là món tráng miệng gia truyền hơn 30 năm. Flan caramel mềm mịn, không quá ngọt, kết cấu hoàn hảo.',
 N'Banh Flan Ngoc Nga is a heirloom dessert with over 30 years of tradition. Smooth caramel flan, not too sweet, with perfect texture.',
 NULL, NULL, NULL,
 N'Vĩnh Khánh, Phường 8, Quận 4, TP.HCM', N'10:00 - 22:00', N'15k - 35k', 1),

(N'Chè Khánh Vy', N'Khac', 10.760300, 106.703800, 15, 2,
 N'Chè Khánh Vy với hơn 40 loại chè truyền thống Việt Nam. Đặc biệt có chè thái kiểu Huế và chè Campuchia độc đáo.',
 N'Che Khanh Vy offers over 40 traditional Vietnamese sweet soups, including unique Hue-style and Cambodian varieties.',
 NULL, NULL, NULL,
 N'Vĩnh Khánh, Phường 8, Quận 4, TP.HCM', N'14:00 - 23:00', N'20k - 45k', 1);

GO

-- ============================================================
-- NarrationLog demo data (for dashboard charts)
-- ============================================================
DECLARE @i INT = 0;
WHILE @i < 200
BEGIN
    INSERT INTO dbo.NarrationLog (PoiId, DeviceId, Language, PlayedAt)
    VALUES (
        ((ABS(CHECKSUM(NEWID())) % 20) + 1),
        N'seed-device-' + CAST((ABS(CHECKSUM(NEWID())) % 8) + 1 AS NVARCHAR(10)),
        (CASE ABS(CHECKSUM(NEWID())) % 10
            WHEN 0 THEN N'English'
            WHEN 1 THEN N'English'
            WHEN 2 THEN N'English'
            WHEN 3 THEN N'Japanese'
            WHEN 4 THEN N'Korean'
            WHEN 5 THEN N'Chinese'
            ELSE N'Vietnamese'
         END),
        DATEADD(HOUR, -(ABS(CHECKSUM(NEWID())) % 720), SYSUTCDATETIME())
    );
    SET @i = @i + 1;
END
GO

-- ============================================================
-- Heartbeat demo
-- ============================================================
INSERT INTO dbo.ActiveDevices (DeviceId, UserId, Platform, AppVersion, LastPingUtc) VALUES
(N'seed-device-1', NULL, N'Android', N'1.0.0', DATEADD(SECOND, -30,  SYSUTCDATETIME())),
(N'seed-device-2', NULL, N'iOS',     N'1.0.0', DATEADD(SECOND, -90,  SYSUTCDATETIME())),
(N'seed-device-3', NULL, N'Android', N'1.0.1', DATEADD(SECOND, -200, SYSUTCDATETIME()));
GO

PRINT N'✓ SeedData.sql completed: 20 POIs, 200 narration logs, 3 devices';
PRINT N'  → Users (admin/demo) will be seeded by C# DataSeeder on first app run.';

-- Verify
SELECT N'Users'        AS TableName, COUNT(*) AS Rows FROM dbo.Users
UNION ALL SELECT N'POIs',             COUNT(*) FROM dbo.PointsOfInterest
UNION ALL SELECT N'NarrationLog',     COUNT(*) FROM dbo.NarrationLog
UNION ALL SELECT N'ActiveDevices',    COUNT(*) FROM dbo.ActiveDevices;
