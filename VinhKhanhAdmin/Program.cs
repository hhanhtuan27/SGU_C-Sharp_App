using System.Text;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using VinhKhanhAdmin.Models;
using VinhKhanhAdmin.Services;

var builder = WebApplication.CreateBuilder(args);

// ========== Services ==========

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ImageService>();
builder.Services.AddScoped<JwtService>();

// Both Cookie (MVC) and JWT (API) authentication — schemes don't interfere
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme          = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.LoginPath         = "/Auth/Login";
    options.LogoutPath        = "/Auth/Logout";
    options.AccessDeniedPath  = "/Auth/Login";
    options.ExpireTimeSpan    = TimeSpan.FromHours(12);
    options.SlidingExpiration = true;
    options.Cookie.Name       = "VinhKhanh.Auth";
    options.Cookie.HttpOnly   = true;
    options.Cookie.SameSite   = SameSiteMode.Lax;
})
.AddJwtBearer("Bearer", options =>
{
    var key      = builder.Configuration["Jwt:Key"] ?? "";
    var issuer   = builder.Configuration["Jwt:Issuer"] ?? "VinhKhanhAdmin";
    var audience = builder.Configuration["Jwt:Audience"] ?? "VinhKhanhApp";

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer           = true,
        ValidateAudience         = true,
        ValidateLifetime         = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer              = issuer,
        ValidAudience            = audience,
        IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
        ClockSkew                = TimeSpan.FromMinutes(1)
    };
});

builder.Services.AddAuthorization(opts =>
{
    opts.AddPolicy("AdminOnly", p => p.RequireRole("admin"));
    opts.AddPolicy("ApiJwt",   p =>
    {
        p.AuthenticationSchemes.Add("Bearer");
        p.RequireAuthenticatedUser();
    });
});

builder.Services.AddControllersWithViews()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

builder.Services.AddAntiforgery(opts => opts.HeaderName = "X-CSRF-TOKEN");

// CORS for mobile app to call API
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

// Increase upload limit
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(o =>
{
    o.MultipartBodyLengthLimit = 10 * 1024 * 1024;
});

var app = builder.Build();

// ========== Seed users on first run ==========
using (var scope = app.Services.CreateScope())
{
    var db     = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    await DataSeeder.SeedUsersAsync(db, logger);
}

// ========== Middleware ==========
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

// Default redirect to login
app.MapGet("/", ctx =>
{
    if (ctx.User.Identity?.IsAuthenticated == true)
        ctx.Response.Redirect("/Home");
    else
        ctx.Response.Redirect("/Auth/Login");
    return Task.CompletedTask;
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
