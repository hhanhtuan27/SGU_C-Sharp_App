-- =====================================================================
-- VinhKhanhGuide - Database schema & seed data
-- SQL Server 2019+
-- =====================================================================

IF DB_ID('VinhKhanhGuide') IS NULL
    CREATE DATABASE VinhKhanhGuide;
GO
USE VinhKhanhGuide;
GO

IF OBJECT_ID('dbo.NarrationLog', 'U') IS NOT NULL DROP TABLE dbo.NarrationLog;
IF OBJECT_ID('dbo.PointsOfInterest', 'U') IS NOT NULL DROP TABLE dbo.PointsOfInterest;
GO

CREATE TABLE dbo.PointsOfInterest (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    Name            NVARCHAR(150)  NOT NULL,
    Category        NVARCHAR(20)   NOT NULL,  -- Oc | Nuong | Lau | CaPhe
    Latitude        FLOAT          NOT NULL,
    Longitude       FLOAT          NOT NULL,
    RadiusMeters    FLOAT          NOT NULL DEFAULT 30,
    Priority        INT            NOT NULL DEFAULT 1,
    DescriptionVi   NVARCHAR(1000) NULL,
    DescriptionEn   NVARCHAR(1000) NULL,
    IsActive        BIT            NOT NULL DEFAULT 1,
    CreatedAt       DATETIME2      NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

CREATE TABLE dbo.NarrationLog (
    Id          BIGINT IDENTITY(1,1) PRIMARY KEY,
    PoiId       INT           NOT NULL,
    Language    NVARCHAR(5)   NOT NULL,
    PlayedAt    DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_NarrationLog_Poi FOREIGN KEY (PoiId) REFERENCES dbo.PointsOfInterest(Id)
);
GO

-- =====================================================================
-- Seed 22 POIs (Vĩnh Khánh, Quận 4)
-- =====================================================================
INSERT INTO dbo.PointsOfInterest (Name, Category, Latitude, Longitude, RadiusMeters, Priority, DescriptionVi, DescriptionEn) VALUES
(N'Ốc Oanh',                       N'Oc',    10.760719, 106.703297, 30, 5, N'Ốc Oanh là địa chỉ huyền thoại trên đường Vĩnh Khánh, nổi tiếng với ốc len xào dừa béo ngậy và sò huyết rang me đậm đà.', N'Oc Oanh is a legendary spot on Vinh Khanh Street, famous for its coconut-braised sea snails and tamarind blood cockles.'),
(N'Quán Ốc 662',                   N'Oc',    10.760836, 106.703505, 30, 3, N'Quán Ốc 662 phục vụ hơn 30 loại ốc tươi sống theo giá bình dân, phù hợp cho nhóm bạn sinh viên.', N'Oc 662 serves over 30 fresh shellfish varieties at student-friendly prices.'),
(N'Ốc Đào 2',                      N'Oc',    10.761137, 106.704979, 30, 3, N'Ốc Đào 2 thường đông khách buổi tối, chuyên các món ốc hương nướng muối ớt và ốc móng tay xào bơ tỏi.', N'Oc Dao 2 gets packed at night with grilled sweet snails and garlic-butter razor clams.'),
(N'Quán Ốc Bụi',                   N'Oc',    10.760597, 106.704217, 30, 2, N'Quán Ốc Bụi là quán vỉa hè gần gũi, nổi tiếng với ốc bươu nhồi thịt và nước chấm đặc biệt.', N'Oc Bui is a humble street stall known for stuffed apple snails and its signature dipping sauce.'),
(N'Ốc Hoa',                        N'Oc',    10.760713, 106.704217, 30, 2, N'Ốc Hoa chuyên ốc tươi vừa đánh bắt, chế biến theo kiểu Nam Bộ.', N'Oc Hoa specializes in freshly caught shellfish prepared Southern-style.'),
(N'Quán Ốc Sáu Nở',                N'Oc',    10.760964, 106.702942, 30, 3, N'Quán Ốc Sáu Nở có tuổi đời hơn 20 năm tại Vĩnh Khánh, nổi bật với sò điệp nướng mỡ hành.', N'Sau No has been on Vinh Khanh for over 20 years, celebrated for scallops grilled with scallion oil.'),
(N'Quán Ốc Vũ',                    N'Oc',    10.761403, 106.702705, 30, 2, N'Quán Ốc Vũ phục vụ hải sản theo mùa với giá phải chăng.', N'Oc Vu offers seasonal seafood at fair prices.'),
(N'Ốc Phát - Ốc Ngon Quận 4',      N'Oc',    10.761955, 106.702094, 30, 3, N'Ốc Phát nổi tiếng với tôm tích rang muối và ghẹ hấp bia.', N'Oc Phat is known for salt-roasted mantis shrimp and beer-steamed crab.'),
(N'Tiệm Nướng 10 Năm',             N'Nuong', 10.760537, 106.703528, 30, 3, N'Tiệm Nướng 10 Năm chuyên thịt ba chỉ nướng than hoa và lòng bò nướng sả ớt.', N'Tiem Nuong 10 Nam grills pork belly and lemongrass-chili beef tripe over charcoal.'),
(N'Hàu Nướng A Trung',             N'Nuong', 10.760669, 106.703673, 30, 3, N'Hàu Nướng A Trung đưa hàu tươi Long Sơn về phục vụ khách mỗi ngày.', N'Hau Nuong A Trung serves fresh Long Son oysters daily.'),
(N'Lẩu Mẹt Nướng 79k',             N'Nuong', 10.760806, 106.704310, 30, 2, N'Combo nướng 79k với đa dạng thịt, hải sản và rau củ đi kèm.', N'The 79k grill combo features a variety of meats, seafood and vegetables.'),
(N'Trạm Nướng BBQ',                N'Nuong', 10.760728, 106.704679, 30, 2, N'Trạm Nướng BBQ phong cách Hàn Quốc với thịt ướp sốt đặc trưng.', N'Tram Nuong BBQ offers Korean-style marinated grill.'),
(N'Thèm Nướng Yakiniku',           N'Nuong', 10.760778, 106.704739, 30, 3, N'Thèm Nướng phục vụ bò Mỹ và wagyu theo phong cách Yakiniku Nhật Bản.', N'Them Nuong serves US beef and wagyu Japanese yakiniku style.'),
(N'Thế Giới Bò - Nướng & Lẩu',     N'Nuong', 10.764036, 106.701278, 30, 2, N'Thế Giới Bò là điểm đến lý tưởng cho tín đồ thịt bò với cả lẩu lẫn nướng.', N'The Gioi Bo is a beef-lover destination with both hot pot and grill.'),
(N'Lẩu Bò Kỳ Kim',                 N'Lau',   10.761460, 106.702608, 30, 3, N'Lẩu Bò Kỳ Kim có nước dùng hầm xương bò 8 tiếng, ăn kèm bắp bò tươi.', N'Ky Kim beef hot pot simmers bone broth for 8 hours, served with fresh brisket.'),
(N'Lẩu gà lá é Con Gà Trống',      N'Lau',   10.760856, 106.706722, 30, 3, N'Lẩu gà lá é phong cách Phú Yên, hương vị thảo mộc đặc trưng.', N'Phu Yen-style chicken hot pot with distinctive herbs.'),
(N'Tiệm Cà Phê Lucky',             N'CaPhe', 10.760451, 106.707005, 30, 1, N'Tiệm Cà Phê Lucky là điểm dừng chân yên tĩnh sau bữa ăn đêm.', N'Lucky Cafe is a quiet stop after a late-night meal.'),
(N'Xù Phê',                        N'CaPhe', 10.761201, 106.706126, 30, 1, N'Xù Phê có không gian trẻ trung với đồ uống signature giá sinh viên.', N'Xu Phe offers a youthful vibe with signature drinks at student prices.'),
(N'Link Coffee & Tea',             N'CaPhe', 10.760846, 106.704983, 30, 1, N'Link Coffee & Tea phục vụ trà trái cây và cà phê pha máy.', N'Link Coffee & Tea serves fruit teas and machine-brewed coffee.'),
(N'Quán Nước SINZIEN',             N'CaPhe', 10.761756, 106.702283, 30, 1, N'SINZIEN là quán nước bình dân, điểm hẹn quen của dân Quận 4.', N'SINZIEN is a casual drink spot, a favorite of District 4 locals.');
GO

SELECT Id, Name, Category, Latitude, Longitude FROM dbo.PointsOfInterest ORDER BY Id;
GO
