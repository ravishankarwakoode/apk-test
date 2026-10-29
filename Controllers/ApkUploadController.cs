using APKAnalyzerApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace APKAnalyzerApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ApkUploadController : ControllerBase
    {
        private readonly IApkAnalyzerService _apkAnalyzerService;
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _configuration;

        public ApkUploadController(IApkAnalyzerService apkAnalyzerService, IWebHostEnvironment environment, IConfiguration configuration)
        {
            _apkAnalyzerService = apkAnalyzerService;
            _environment = environment;
            _configuration = configuration;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No APK file uploaded.");
            }

            var maxFileSizeInMb = _configuration["AppSettings:MaxApkFileSizeInMb"] ?? "100";
            var maxFileSizeBytes = long.Parse(maxFileSizeInMb) * 1024 * 1024;

            if (file.Length > maxFileSizeBytes)
            {
                return BadRequest($"APK file size exceeds the allowed limit of {maxFileSizeInMb} MB.");
            }

            if (!file.FileName.EndsWith(".apk", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("Only APK files are allowed.");
            }

            var uploadFolder = Path.Combine(_environment.ContentRootPath, "Uploads");
            Directory.CreateDirectory(uploadFolder);

            var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
            var filePath = Path.Combine(uploadFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var result = await _apkAnalyzerService.AnalyzeAsync(filePath);

            return Ok(new
            {
                fileName = result.FileName,
                packageName = result.PackageName,
                versionName = result.VersionName,
                versionCode = result.VersionCode,
                permissions = result.Permissions,
                findings = result.Findings,
                riskScore = result.RiskScore,
                reportPath = result.ReportPath
            });
        }
    }
}
