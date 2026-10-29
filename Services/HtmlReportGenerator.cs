using APKAnalyzerApp.Models;

namespace APKAnalyzerApp.Services
{
    public class HtmlReportGenerator : IReportGenerator
    {
        public Task<string> GenerateAsync(ApkReport report)
        {
            var reportsDir = "Reports";
            Directory.CreateDirectory(reportsDir);

            var fileName = $"{Path.GetFileNameWithoutExtension(report.FileName)}_{DateTime.Now:yyyyMMddHHmmss}.html";
            var filePath = Path.Combine(reportsDir, fileName);

            var html = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8' />
    <title>APK Security Report</title>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 30px; color: #222; }}
        h1 {{ color: #1d3557; }}
        .card {{ border: 1px solid #ddd; padding: 16px; margin-bottom: 18px; border-radius: 8px; }}
        .risk {{ color: red; font-weight: bold; }}
        ul {{ line-height: 1.8; }}
        .header {{ background: #f1f5f9; padding: 20px; border-radius: 8px; }}
    </style>
</head>
<body>
    <div class='header'>
        <h1>APK Security Report</h1>
        <p><strong>File:</strong> {report.FileName}</p>
        <p><strong>Package:</strong> {report.PackageName}</p>
        <p><strong>Version Name:</strong> {report.VersionName}</p>
        <p><strong>Version Code:</strong> {report.VersionCode}</p>
        <p><strong>Created At:</strong> {report.CreatedAt}</p>
        <p class='risk'><strong>Risk Score:</strong> {report.RiskScore}/100</p>
    </div>

    <div class='card'>
        <h2>Permissions</h2>
        <ul>
            {GetHtmlList(report.Permissions)}
        </ul>
    </div>

    <div class='card'>
        <h2>Findings</h2>
        <ul>
            {GetHtmlList(report.Findings)}
        </ul>
    </div>
</body>
</html>";

            File.WriteAllText(filePath, html);
            return Task.FromResult(filePath);
        }

        private static string GetHtmlList(List<string> items)
        {
            if (items.Count == 0)
            {
                return "<li>No data found.</li>";
            }

            return string.Join("", items.Select(item => $"<li>{item}</li>"));
        }
    }
}
