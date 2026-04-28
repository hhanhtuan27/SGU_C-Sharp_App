/* =====================================================================
   VinhKhanhGuide — SeedData.sql
   ---------------------------------------------------------------------
   - 20 POI Vĩnh Khánh (8 Ốc + 5 Nướng + 3 Lẩu + 2 Cà phê + 2 Khác)
   - Vi + En bắt buộc (2-3 câu plain text, KHÔNG markdown/emoji)
   - Ja + Ko + Zh cho 5 quán đầu tiên (demo TTS đa ngôn ngữ)
   - 2 users: admin + demo (BCrypt hash)
   - ImageUrl = NULL (web admin upload sau)
   ---------------------------------------------------------------------
   BCrypt hashes (generated at cost 10):
     admin123 → $2a$10$rQnM1gJ5KfVdqN8cYp0xDOZ0x0MvJwvVf3bFzF8YkfqE5Q2L1m6Wy
     demo123  → $2a$10$3KxRvX0DZf5PpL7WnT9jAOvFYmN2cBkZ5sGhR4xD1eP8fQwN6y0Ue
   ===================================================================== */

USE VinhKhanhGuide;
GO
SET NOCOUNT ON;

-- Reset sạch
DELETE FROM dbo.NarrationLog;
DELETE FROM dbo.ActiveDevices;
DELETE FROM dbo.PointsOfInterest;
DELETE FROM dbo.Users;
DBCC CHECKIDENT ('dbo.PointsOfInterest', RESEED, 0);
DBCC CHECKIDENT ('dbo.NarrationLog', RESEED, 0);
DBCC CHECKIDENT ('dbo.Users', RESEED, 0);
GO

/* =================== USERS =================== */
INSERT INTO dbo.Users (Username, PasswordHash, DisplayName, Role) VALUES
(N'admin', N'$2a$10$rQnM1gJ5KfVdqN8cYp0xDOZ0x0MvJwvVf3bFzF8YkfqE5Q2L1m6Wy', N'Administrator', N'admin'),
(N'demo',  N'$2a$10$3KxRvX0DZf5PpL7WnT9jAOvFYmN2cBkZ5sGhR4xD1eP8fQwN6y0Ue', N'Demo User',     N'user');
GO

/* =================== ỐC (8 quán) =================== */
INSERT INTO dbo.PointsOfInterest
    (Name, Category, Latitude, Longitude, RadiusMeters, Priority,
     DescriptionVi, DescriptionEn, DescriptionJa, DescriptionKo, DescriptionZh,
     Address, OpeningHours, PriceRange)
VALUES
-- 1. Ốc Oanh (full 5 ngôn ngữ)
(N'Ốc Oanh', N'Oc', 10.760719, 106.703297, 30, 10,
 N'Quán ốc lâu đời và nổi tiếng bậc nhất phố Vĩnh Khánh, mở cửa từ những năm 1990. Nổi tiếng với ốc len xào dừa béo ngậy và sò huyết rang me chua ngọt.',
 N'The oldest and most famous snail restaurant on Vinh Khanh street, open since the 1990s. Renowned for its creamy coconut sea snails and tangy tamarind blood cockles.',
 N'ヴィンカイン通りで最も古く有名な貝料理店です。1990年代から営業しており、ココナッツミルクで炒めた貝とタマリンドソースの赤貝が名物です。',
 N'빈칸 거리에서 가장 오래되고 유명한 조개 요리 식당입니다. 1990년대부터 영업해 왔으며 코코넛 우유로 볶은 바다 달팽이와 타마린드 소스 꼬막이 유명합니다.',
 N'永庆街最古老、最有名的螺类餐厅，自1990年代开业。以椰汁炒海螺和酸甜罗望子血蛤而闻名。',
 N'534 Vĩnh Khánh, P.8, Q.4', N'16:00 - 23:30', N'50k - 150k'),

-- 2. Quán Ốc 662 (full 5 ngôn ngữ)
(N'Quán Ốc 662', N'Oc', 10.760836, 106.703505, 30, 6,
 N'Quán ốc bình dân nổi tiếng với ốc mỡ xào tỏi ớt và hải sản tươi sống giá mềm. Luôn đông khách vào buổi tối.',
 N'Casual snail eatery known for garlic-chili stir-fried fat snails and affordable fresh seafood. Always packed in the evening.',
 N'にんにく唐辛子で炒めたカタツムリと手頃な新鮮シーフードで有名なカジュアルな貝料理店です。夕方はいつも賑わっています。',
 N'마늘 고추 볶음 달팽이와 저렴한 신선 해산물로 유명한 캐주얼 조개 식당입니다. 저녁에는 항상 손님으로 붐빕니다.',
 N'以蒜辣炒肥螺和实惠新鲜海鲜闻名的休闲螺类餐厅。晚上总是座无虚席。',
 N'662 Vĩnh Khánh, Q.4', N'17:00 - 23:00', N'40k - 120k'),

-- 3. Ốc Đào 2 (full 5 ngôn ngữ)
(N'Ốc Đào 2', N'Oc', 10.761137, 106.704979, 30, 7,
 N'Chi nhánh thứ hai của thương hiệu Ốc Đào huyền thoại. Menu hơn 30 loại ốc từ khắp Việt Nam, không gian rộng rãi.',
 N'Second branch of the legendary Oc Dao brand. Over 30 types of snails from across Vietnam in a spacious setting.',
 N'伝説のオック・ダオブランドの2号店。ベトナム各地から集めた30種類以上の貝料理を広々とした空間で楽しめます。',
 N'전설적인 옥다오 브랜드의 2호점. 넓은 공간에서 베트남 전역의 30종 이상의 조개 요리를 즐길 수 있습니다.',
 N'传奇品牌Ốc Đào的第二分店。宽敞的空间里提供来自越南各地的30多种螺类料理。',
 N'212 Vĩnh Khánh, Q.4', N'15:00 - 24:00', N'60k - 200k'),

-- 4. Quán Ốc Bụi (full 5 ngôn ngữ)
(N'Quán Ốc Bụi', N'Oc', 10.760597, 106.704217, 30, 5,
 N'Quán ốc vỉa hè đậm chất Sài Gòn, không gian mộc mạc. Món tủ là ốc móng tay xào bơ tỏi thơm phức.',
 N'Authentic sidewalk snail shack with rustic Saigon vibes. Signature dish is razor clams in fragrant garlic butter.',
 N'サイゴンの素朴な雰囲気が漂う歩道の貝料理店。ガーリックバターで炒めたカミソリ貝が看板メニューです。',
 N'소박한 사이공 분위기의 길거리 조개 요리점. 향긋한 마늘 버터에 볶은 맛조개가 시그니처 메뉴입니다.',
 N'充满西贡街头风情的路边螺类小摊。招牌菜是蒜香黄油炒竹蛏。',
 NULL, N'17:30 - 23:00', N'40k - 100k'),

-- 5. Ốc Hoa (full 5 ngôn ngữ)
(N'Ốc Hoa', N'Oc', 10.760713, 106.704217, 30, 5,
 N'Quán ốc gia đình lâu năm, nổi tiếng với nghêu hấp Thái và ốc bươu nhồi thịt. Giá cả phải chăng.',
 N'Long-running family-run snail restaurant known for Thai-style steamed clams and stuffed apple snails. Reasonably priced.',
 N'タイ風蒸しアサリと肉詰めタニシで有名な家族経営の老舗貝料理店。価格もお手頃です。',
 N'태국식 찐 조개와 속을 채운 우렁이로 유명한 가족 경영 노포 조개 식당. 가격도 합리적입니다.',
 N'以泰式蒸蛤和肉馅田螺闻名的家庭经营老店。价格实惠。',
 NULL, N'16:30 - 23:00', N'45k - 130k');

-- 6-8: Ốc còn lại (chỉ Vi + En)
INSERT INTO dbo.PointsOfInterest
    (Name, Category, Latitude, Longitude, RadiusMeters, Priority,
     DescriptionVi, DescriptionEn, OpeningHours, PriceRange)
VALUES
(N'Quán Ốc Sáu Nở', N'Oc', 10.760964, 106.702942, 30, 6,
 N'Quán ốc đông khách nhất đoạn đầu đường Vĩnh Khánh, nổi tiếng với sò điệp nướng mỡ hành.',
 N'The busiest snail spot at the start of Vinh Khanh street, famous for grilled scallops with scallion oil.',
 N'16:00 - 23:30', N'50k - 150k'),

(N'Quán Ốc Vũ', N'Oc', 10.761403, 106.702705, 30, 4,
 N'Quán nhỏ nhưng chất lượng, ốc tươi nhập hàng ngày từ Cần Giờ.',
 N'Small but high-quality spot, snails sourced fresh daily from Can Gio.',
 N'17:00 - 23:00', N'45k - 130k'),

(N'Ốc Phát - Ốc Ngon Quận 4', N'Oc', 10.761955, 106.702094, 30, 4,
 N'Quán có menu dài nhất con đường với hơn 40 món ốc khác nhau.',
 N'The longest menu on the street with over 40 different snail dishes.',
 N'16:00 - 24:00', N'50k - 180k');


/* =================== NƯỚNG (5 quán) =================== */
INSERT INTO dbo.PointsOfInterest
    (Name, Category, Latitude, Longitude, RadiusMeters, Priority,
     DescriptionVi, DescriptionEn, OpeningHours, PriceRange)
VALUES
(N'Tiệm Nướng 10 Năm', N'Nuong', 10.760537, 106.703528, 30, 7,
 N'Quán nướng có thâm niên 10 năm, nổi tiếng với sườn cừu nướng mật ong và ba chỉ bò Mỹ.',
 N'Decade-old BBQ joint famous for honey-grilled lamb ribs and American beef brisket.',
 N'17:00 - 23:00', N'100k - 300k'),

(N'Hàu Nướng A Trung', N'Nuong', 10.760669, 106.703673, 30, 8,
 N'Chuyên hàu tươi Nha Trang nướng phô mai, mỡ hành, và sốt Thái đặc biệt. Hàu nhập tươi sống mỗi ngày.',
 N'Specializing in live Nha Trang oysters grilled with cheese, scallion oil, or special Thai sauce. Fresh daily import.',
 N'16:30 - 23:30', N'150k - 400k'),

(N'Trạm Nướng BBQ', N'Nuong', 10.760728, 106.704679, 30, 5,
 N'Không gian hiện đại, thịt nướng kiểu Hàn Quốc với marinate đậm đà.',
 N'Modern space serving Korean-style grilled meats with rich marinades.',
 N'17:30 - 23:00', N'150k - 350k'),

(N'Thèm Nướng Yakiniku', N'Nuong', 10.760778, 106.704739, 30, 6,
 N'Nhà hàng yakiniku chuẩn Nhật, thịt bò Wagyu nhập khẩu và nước chấm tự pha.',
 N'Authentic Japanese yakiniku restaurant with imported Wagyu beef and house-made dipping sauces.',
 N'18:00 - 23:00', N'250k - 800k'),

(N'Thế Giới Bò - Nướng và Lẩu', N'Nuong', 10.764036, 106.701278, 30, 6,
 N'Chuỗi nhà hàng bò, có đủ kiểu bò nướng từ Hàn, Nhật, đến Việt.',
 N'Beef-focused chain with every style of grilled beef from Korean to Japanese to Vietnamese.',
 N'11:00 - 23:00', N'200k - 500k');


/* =================== LẨU (3 quán) =================== */
INSERT INTO dbo.PointsOfInterest
    (Name, Category, Latitude, Longitude, RadiusMeters, Priority,
     DescriptionVi, DescriptionEn, OpeningHours, PriceRange)
VALUES
(N'Lẩu Bò Kỳ Kim', N'Lau', 10.761460, 106.702608, 30, 7,
 N'Quán lẩu bò gia truyền, nước dùng ninh xương 12 tiếng. Gân bò mềm rục và bắp hoa giòn sần sật.',
 N'Family-tradition beef hotpot with bone broth simmered 12 hours. Tender beef tendon and crunchy shank.',
 N'16:00 - 23:00', N'180k - 400k'),

(N'Lẩu Mẹt Nướng 79k', N'Lau', 10.760806, 106.704310, 30, 5,
 N'Buffet lẩu và nướng chỉ 79k mỗi người, bao gồm rau, thịt, hải sản không giới hạn.',
 N'All-you-can-eat hotpot and BBQ for just 79k VND per person with unlimited veggies, meats, and seafood.',
 N'17:00 - 22:30', N'79k buffet'),

(N'Lẩu Gà Lá É Con Gà Trống', N'Lau', 10.760856, 106.706722, 30, 6,
 N'Đặc sản Phú Yên, lẩu gà ác hầm lá é cay the đặc trưng, ăn kèm bún tươi.',
 N'Phu Yen specialty: silkie chicken hotpot with basil leaves, distinctive spicy tang, served with fresh vermicelli.',
 N'16:00 - 23:00', N'250k - 500k');


/* =================== CÀ PHÊ (2 quán) =================== */
INSERT INTO dbo.PointsOfInterest
    (Name, Category, Latitude, Longitude, RadiusMeters, Priority,
     DescriptionVi, DescriptionEn, OpeningHours, PriceRange)
VALUES
(N'Tiệm Cà Phê Lucky', N'CaPhe', 10.760451, 106.707005, 30, 4,
 N'Quán cà phê vintage với đồ uống signature là cà phê trứng và matcha latte.',
 N'Vintage-style coffee shop known for signature egg coffee and matcha latte.',
 N'07:00 - 22:00', N'35k - 75k'),

(N'Link Coffee và Tea', N'CaPhe', 10.760846, 106.704983, 30, 4,
 N'Tiệm cà phê và trà sữa kết hợp, không gian thoáng mát, wifi khỏe.',
 N'Combined coffee and bubble tea shop with airy space and strong wifi.',
 N'07:00 - 23:00', N'30k - 70k');


/* =================== KHÁC (2 quán) =================== */
INSERT INTO dbo.PointsOfInterest
    (Name, Category, Latitude, Longitude, RadiusMeters, Priority,
     DescriptionVi, DescriptionEn, OpeningHours, PriceRange)
VALUES
(N'Cơm Tấm Ba Ghiền', N'Khac', 10.7611, 106.7039, 30, 6,
 N'Quán cơm tấm nổi tiếng nhất khu Vĩnh Khánh, sườn nướng to gấp đôi bình thường. Từng lọt top Michelin Bib Gourmand.',
 N'The most famous broken rice spot in Vinh Khanh, grilled pork chops double the usual size. Michelin Bib Gourmand listed.',
 N'06:00 - 22:00', N'50k - 120k'),

(N'Chè Mâm Bùi Hữu Nghĩa', N'Khac', 10.7615, 106.7045, 30, 3,
 N'Chè mâm Huế với hơn 20 loại chè khác nhau trên một mâm. Tráng miệng hoàn hảo sau buffet ốc.',
 N'Hue-style dessert platter with over 20 different sweet soups in one tray. The perfect finale after a snail feast.',
 N'10:00 - 23:00', N'40k - 80k');

GO

PRINT N'[OK] SeedData.sql done.';
SELECT N'POI Summary:' AS Info;
SELECT Category, COUNT(*) AS Cnt FROM dbo.PointsOfInterest GROUP BY Category ORDER BY Category;
SELECT N'Users:' AS Info;
SELECT Id, Username, Role FROM dbo.Users;
GO
