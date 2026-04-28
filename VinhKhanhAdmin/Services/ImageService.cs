using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace VinhKhanhAdmin.Services;

public class ImageService
{
    private readonly IWebHostEnvironment _env;
    private readonly IConfiguration _config;
    private readonly ILogger<ImageService> _logger;

    public ImageService(IWebHostEnvironment env, IConfiguration config, ILogger<ImageService> logger)
    {
        _env = env;
        _config = config;
        _logger = logger;
    }

    /// <summary>
    /// Uploads and resizes image; returns absolute URL like http://server:5000/uploads/pois/{guid}.jpg
    /// </summary>
    public async Task<string?> UploadPoiImageAsync(IFormFile file, HttpRequest request)
    {
        if (file == null || file.Length == 0) return null;

        var maxSize = _config.GetValue<long>("Upload:MaxSizeBytes", 3_145_728);
        var maxDim  = _config.GetValue<int>("Upload:MaxDimension", 800);

        if (file.Length > maxSize)
            throw new InvalidOperationException($"File quá lớn (tối đa {maxSize / 1024 / 1024} MB)");

        var allowed = new[] { "image/jpeg", "image/png", "image/webp" };
        if (!allowed.Contains(file.ContentType.ToLowerInvariant()))
            throw new InvalidOperationException("Chỉ chấp nhận JPG, PNG, WEBP");

        var uploadDir = Path.Combine(_env.WebRootPath, "uploads", "pois");
        Directory.CreateDirectory(uploadDir);

        var fileName = $"{Guid.NewGuid():N}.jpg";
        var filePath = Path.Combine(uploadDir, fileName);

        using (var stream = file.OpenReadStream())
        using (var image = await Image.LoadAsync(stream))
        {
            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Mode = ResizeMode.Max,
                Size = new Size(maxDim, maxDim)
            }));

            await image.SaveAsJpegAsync(filePath, new JpegEncoder { Quality = 85 });
        }

        // Build absolute URL (config override → request scheme/host fallback)
        var baseUrl = _config["Upload:PublicBaseUrl"];
        if (string.IsNullOrWhiteSpace(baseUrl))
            baseUrl = $"{request.Scheme}://{request.Host}";

        return $"{baseUrl.TrimEnd('/')}/uploads/pois/{fileName}";
    }

    public void DeletePoiImage(string? imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl)) return;
        try
        {
            var fileName = Path.GetFileName(new Uri(imageUrl).LocalPath);
            if (string.IsNullOrWhiteSpace(fileName)) return;

            var path = Path.Combine(_env.WebRootPath, "uploads", "pois", fileName);
            if (File.Exists(path)) File.Delete(path);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to delete image {Url}", imageUrl);
        }
    }
}
