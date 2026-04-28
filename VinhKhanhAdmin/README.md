# VinhKhanh Admin — Web Panel

ASP.NET Core 8 MVC + EF Core + SQL Server — admin panel cho đồ án **"Smart Audio Guide phố ẩm thực Vĩnh Khánh"** (Quận 4, HCMC).

Web admin này là **kênh duy nhất** để CRUD POI (Points of Interest) và quản lý user. Đồng thời cung cấp REST API cho app .NET MAUI mobile để đăng nhập, lấy danh sách POI, gửi heartbeat, và log lượt nghe TTS.

![Stack](https://img.shields.io/badge/.NET-8.0-blueviolet) ![SQL](https://img.shields.io/badge/SQL%20Server-2019+-red) ![EF](https://img.shields.io/badge/EF%20Core-8.0-green) ![License](https://img.shields.io/badge/License-MIT-yellow)

---

## 🏗️ Kiến trúc tổng thể

```
┌──────────────────┐      HTTP REST       ┌──────────────────┐      EF Core / SP      ┌──────────────────┐
│   App .NET MAUI  │ ──────────────────▶  │   Web Admin      │ ─────────────────────▶ │  SQL Server DB   │
│   (Mobile)       │                      │   (ASP.NET Core) │                        │  VinhKhanhGuide  │
│                  │                      │                  │                        │                  │
│  • GPS tracking  │                      │  • CRUD POI      │                        │  • 4 tables      │
│  • TTS narration │                      │  • Upload ảnh    │                        │  • 5 views       │
│  • Heartbeat     │                      │  • Dashboard     │                        │  • 7 SPs         │
│  • User login    │                      │  • User mgmt     │                        │  • 1 trigger     │
└──────────────────┘                      │  • JWT API       │                        └──────────────────┘
                                          └──────────────────┘
```

**Web** dùng Cookie authentication cho trang admin, JWT Bearer cho mobile API — 2 scheme chạy song song không xung đột.

---

## ⚡ Cài đặt nhanh (5 phút)

### Bước 1 — Chuẩn bị SQL Server

Cài SQL Server 2019+ (Developer / Express / LocalDB đều được). Sửa connection string trong `appsettings.json` nếu cần:

```json
"ConnectionStrings": {
    "DefaultConnection": "Server=(local);Database=VinhKhanhGuide;Integrated Security=True;TrustServerCertificate=True;"
}
```

### Bước 2 — Chạy SQL scripts

Mở **SQL Server Management Studio (SSMS)** hoặc **Azure Data Studio**, kết nối tới server, chạy tuần tự **4 file** trong folder `Database/`:

| Thứ tự | File | Tác dụng |
|---|---|---|
| 1 | `CreateDatabase.sql` | DROP DB cũ (nếu có) và tạo 4 tables + 7 indexes + trigger |
| 2 | `Views.sql` | 5 views dashboard |
| 3 | `StoredProcedures.sql` | 7 SPs (CRUD POI + User + Ping) |
| 4 | `SeedData.sql` | 20 POI + 200 narration logs demo |

Verify:

```sql
USE VinhKhanhGuide;
SELECT COUNT(*) FROM dbo.PointsOfInterest;    -- mong đợi: 20
SELECT COUNT(*) FROM dbo.NarrationLog;        -- mong đợi: 200
```

> **Lưu ý**: User `admin`/`demo` **chưa có** sau khi chạy SQL — sẽ được **C# DataSeeder** tự tạo lúc start app (với BCrypt hash chuẩn).

### Bước 3 — Chạy web

```bash
cd VinhKhanhAdmin
dotnet restore
dotnet run
```

Mở trình duyệt: **http://localhost:5000** → đăng nhập:

| Username | Password | Role |
|---|---|---|
| `admin` | `admin123` | admin |
| `demo` | `demo123` | user (không login được web admin, chỉ cho app mobile) |

---

## 🎨 Tính năng Web Admin

### 🏠 Dashboard (`/Home`)

- **4 stat cards**: tổng POI / user / online realtime / lượt nghe hôm nay
- **Biểu đồ lượt nghe 30 ngày** (Chart.js line chart với gradient cam)
- **Doughnut chart** phân bố theo category (Ốc/Nướng/Lẩu/Cà phê/Khác)
- **Top 10 quán hot** với ranking vàng/bạc/đồng
- **Bar chart ngôn ngữ TTS** (Việt/Anh/Nhật/Hàn/Trung)
- **Online devices**: auto-refresh mỗi 30 giây qua AJAX

### 📍 Quản lý POI (`/Pois`)

**Danh sách**:
- Bảng có thumbnail, filter chip theo category, search by name, phân trang 15/page
- Tab "POI đã xóa" + nút khôi phục (soft delete)
- Actions inline: Chỉnh sửa / Xóa / Khôi phục

**Form thêm/sửa** (trang quan trọng nhất):
- **3 chế độ nhập tọa độ đồng thời** — sync 2 chiều realtime:
  - **A**. Dán link Google Maps → **server-side regex parser** qua AJAX endpoint `/Pois/ParseGmaps`. Hỗ trợ:
    - `@lat,lng,zoom` (URL place)
    - `?query=lat,lng` (search URL)
    - Iframe embed `!3d=lat!2d=lng`
    - URL path `/lat,lng/`
    - `?ll=lat,lng`
    - Tọa độ thuần `10.76, 106.70`
  - **B**. Nhập tay 2 ô Lat/Lng (validate range `[-90,90]` / `[-180,180]`)
  - **C**. Kéo-thả marker trên mini map Leaflet (hoặc click để đặt điểm mới)
- **Mini map Leaflet** (free, không cần API key):
  - Marker cam gradient match theme
  - Vòng tròn bán kính geofence update theo slider
- **Upload ảnh**:
  - JPG/PNG/WEBP, max 3MB, auto-resize về 800x800 bằng **ImageSharp**
  - Lưu vào `/wwwroot/uploads/pois/{guid}.jpg`
  - DB lưu **URL tuyệt đối**: `http://localhost:5000/uploads/pois/xxx.jpg` ← app MAUI dùng trực tiếp
  - Hoặc paste URL ảnh ngoài
- **5 tab ngôn ngữ** (🇻🇳 🇬🇧 🇯🇵 🇰🇷 🇨🇳) với indicator chấm xanh khi tab đã điền
- **Priority slider 1-10**, **Radius slider 10-100m** với preview visual
- **Toggle IsActive** với label đổi màu

### 👥 Quản lý User (`/Users`, admin only)

- CRUD user: username, display name, email, role (admin/user)
- Tạo user mới → password tự hash bằng BCrypt (work factor 11)
- Edit → để trống password nếu không đổi
- Toggle active / inactive (không vô hiệu hóa được user `admin` gốc)

---

## 🔌 REST API cho App MAUI Mobile

Base URL: `http://your-server:5000/api` · CORS đã enable cho tất cả origin.

### `POST /api/auth/login`

```json
// Request
{ "username": "demo", "password": "demo123", "deviceId": "android-abc123", "platform": "Android" }

// Response 200
{
  "token": "eyJhbGci...",
  "user": { "id": 2, "username": "demo", "displayName": "Demo User", "role": "user" }
}
```

Server tự upsert device heartbeat khi login có `deviceId`.

### `POST /api/auth/register`

```json
// Request
{ "username": "newuser", "password": "123456", "displayName": "New User", "email": "a@b.com" }
// Response: giống login
```

### `GET /api/pois` · `GET /api/pois?category=Oc`

Trả về list POI đang active, sort by priority desc. Bao gồm `DescriptionVi/En/Ja/Ko/Zh` và `ImageUrl` tuyệt đối.

### `GET /api/pois/{id}`

Chi tiết 1 POI.

### `POST /api/ping`

```json
{ "deviceId": "android-abc123", "userId": 2, "platform": "Android", "appVersion": "1.0.0" }
```

Call mỗi 60-120s từ mobile để cập nhật `LastPingUtc`. Dashboard coi device là "online" nếu ping trong 5 phút qua.

### `POST /api/narration/log`

```json
{ "poiId": 1, "deviceId": "android-abc123", "language": "Vietnamese" }
```

Gọi sau khi TTS play xong. `language` phải là 1 trong 5: `Vietnamese` | `English` | `Japanese` | `Korean` | `Chinese`.

---

## 📦 Schema Database

Xem `Database/CreateDatabase.sql` và ERD trong file `ERD.md`.

**4 tables**:

| Table | Purpose |
|---|---|
| `Users` | Admin + mobile users với BCrypt hash |
| `PointsOfInterest` | POI với 5 ngôn ngữ, tọa độ, bán kính, image URL |
| `ActiveDevices` | Heartbeat từ mobile |
| `NarrationLog` | Analytics: POI × language × device × time |

**5 views** (pre-aggregated cho dashboard): `vw_OnlineUsers`, `vw_TopPlayedPois`, `vw_LanguageStats`, `vw_DailyPlays`, `vw_UserStats`

**7 SPs**: `sp_InsertPoi`, `sp_UpdatePoi`, `sp_SoftDeletePoi`, `sp_RestorePoi`, `sp_RegisterUser`, `sp_LoginUser`, `sp_UpsertPing`

**⚠ Ràng buộc quan trọng**:
- `Category` CHỈ nhận 5 giá trị: `Oc` | `Nuong` | `Lau` | `CaPhe` | `Khac` (CHECK constraint)
- `Language` CHỈ nhận 5 giá trị: `Vietnamese` | `English` | `Japanese` | `Korean` | `Chinese`
- `DescriptionVi` và `DescriptionEn` **NOT NULL** (app MAUI fallback chọn→En→Vi)
- `UpdatedAt` tự động cập nhật qua trigger `tr_POI_UpdateTimestamp`

---

## 🧪 Test tích hợp end-to-end

**Flow chính**: web admin thêm POI → app MAUI refresh → POI xuất hiện + có ảnh.

1. Đăng nhập web `admin`/`admin123`
2. Menu "Quản lý POI" → "Thêm POI"
3. Điền tên, chọn category, dán link Google Maps (ví dụ: `https://www.google.com/maps/place/10.760719,106.703297`) → click "Trích xuất" → tọa độ tự điền
4. Kéo marker trên bản đồ để tinh chỉnh
5. Upload ảnh JPG bất kỳ (web tự resize)
6. Viết mô tả tiếng Việt (bắt buộc), tiếng Anh (bắt buộc)
7. Lưu POI

**Kiểm tra DB**:

```sql
SELECT Id, Name, Category, ImageUrl, Latitude, Longitude FROM PointsOfInterest
ORDER BY Id DESC;
-- ImageUrl phải là URL tuyệt đối: http://localhost:5000/uploads/pois/{guid}.jpg
```

**Từ app MAUI** gọi:

```csharp
var http = new HttpClient();
var pois = await http.GetFromJsonAsync<List<PoiDto>>("http://localhost:5000/api/pois");
// → 21 POIs (20 seed + 1 mới tạo)
// pois[20].ImageUrl → tải ảnh trực tiếp qua URL
```

---

## 🗂️ Cấu trúc project

```
VinhKhanhAdmin/
├── Controllers/
│   ├── AuthController.cs           (login/logout cookie)
│   ├── HomeController.cs           (dashboard)
│   ├── PoisController.cs           (CRUD + parse gmaps)
│   ├── UsersController.cs          (user management)
│   └── Api/
│       ├── ApiAuthController.cs    (JWT login/register)
│       ├── ApiPoisController.cs    (GET for mobile)
│       └── ApiPingNarrationController.cs
├── Models/
│   ├── AppDbContext.cs             (EF Core DbContext)
│   ├── PointOfInterest.cs
│   ├── User.cs, ActiveDevice.cs, NarrationLog.cs
│   └── ViewModels/ViewModels.cs
├── Services/
│   ├── DataSeeder.cs               (seeds admin/demo users)
│   ├── GmapsParser.cs              (6 URL format regex)
│   ├── ImageService.cs             (upload + resize)
│   └── JwtService.cs
├── Views/
│   ├── Auth/Login.cshtml
│   ├── Home/Index.cshtml, Error.cshtml
│   ├── Pois/Index.cshtml, Form.cshtml
│   ├── Users/Index.cshtml, Form.cshtml
│   └── Shared/_Layout.cshtml
├── wwwroot/
│   ├── css/site.css                (dark navy + orange theme)
│   ├── js/site.js
│   └── uploads/pois/               (ảnh upload)
├── Database/
│   ├── CreateDatabase.sql          (chạy 1st)
│   ├── Views.sql                   (chạy 2nd)
│   ├── StoredProcedures.sql        (chạy 3rd)
│   └── SeedData.sql                (chạy 4th)
├── Properties/launchSettings.json
├── VinhKhanhAdmin.csproj
├── Program.cs
├── appsettings.json
└── README.md  (file này)
```

---

## 🛠️ Công nghệ & NuGet packages

| Package | Phiên bản | Mục đích |
|---|---|---|
| `Microsoft.EntityFrameworkCore.SqlServer` | 8.0.0 | ORM + SQL Server provider |
| `BCrypt.Net-Next` | 4.0.3 | Password hashing |
| `SixLabors.ImageSharp` | 3.1.5 | Image resize (cross-platform, thay cho System.Drawing) |
| `Microsoft.AspNetCore.Authentication.JwtBearer` | 8.0.0 | JWT cho mobile API |

Frontend:
- **Bootstrap** không dùng — viết CSS thuần để match vibe app WinForms
- **Leaflet.js 1.9.4** — bản đồ free (OpenStreetMap tiles)
- **Chart.js 4.4.0** — dashboard charts

---

## 🔐 Bảo mật

- Password: BCrypt work factor 11 (chậm vừa đủ để chống brute force)
- JWT: HS256, default expiry 30 days. **Đổi `Jwt:Key` trong `appsettings.json`** trước khi deploy production
- Cookie admin: HttpOnly, SameSite=Lax, hết hạn 12h sliding
- Anti-forgery token trên tất cả POST form + AJAX
- CORS: `AllowAnyOrigin` (demo); production nên whitelist domain mobile

---

## 🐛 Troubleshooting

**Lỗi "Cannot connect to database"**
→ Check SQL Server service đang chạy, verify connection string, mở Windows Firewall port 1433 nếu dùng TCP.

**Upload ảnh 413 Payload Too Large**
→ Kestrel default limit là 30MB, đã được config. Nếu dùng IIS reverse proxy, thêm `<requestLimits maxAllowedContentLength="10485760" />` trong `web.config`.

**App MAUI không load được ảnh**
→ Check `ImageUrl` trong DB phải là URL **tuyệt đối** với scheme `http://` hoặc `https://`. Nếu deploy lên server khác, update `Upload:PublicBaseUrl` trong `appsettings.json`.

**Short link `maps.app.goo.gl` không parse được**
→ Đúng spec — short link cần follow HTTP redirect (server-side). Thêm HttpClient follow 3xx trong `GmapsParser` nếu cần (ngoài scope MVP).

---

## 📄 License

MIT — đồ án học thuật. Tự do sử dụng, sửa đổi, không trách nhiệm.

---

**Tác giả**: Đồ án C# — Phố ẩm thực Vĩnh Khánh, Quận 4, TP.HCM
**Ngày cập nhật**: 2026
