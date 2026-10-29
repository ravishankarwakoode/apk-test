using APKAnalyzerApp.Models;
using APKAnalyzerApp.Utilities;
using System.Text.RegularExpressions;

namespace APKAnalyzerApp.Services
{
    public class ApkAnalyzerService : IApkAnalyzerService
    {
        private readonly IReportGenerator _reportGenerator;

        public ApkAnalyzerService(IReportGenerator reportGenerator)
        {
            _reportGenerator = reportGenerator;
            EnsureDirectoriesExist();
        }

        private void EnsureDirectoriesExist()
        {
            Directory.CreateDirectory("Uploads");
            Directory.CreateDirectory("Reports");
        }

        public async Task<ApkAnalysisResult> AnalyzeAsync(string apkFilePath)
        {
            var fileName = Path.GetFileName(apkFilePath);

            if (!File.Exists(apkFilePath))
            {
                throw new FileNotFoundException("APK file not found.", apkFilePath);
            }

            var packageName = string.Empty;
            var versionName = string.Empty;
            var versionCode = string.Empty;
            var permissions = new List<string>();
            var findings = new List<string>();

            try
            {
                var aaptPath = GetToolPath("aapt");
                if (!string.IsNullOrEmpty(aaptPath))
                {
                    var output = CommandRunner.Run(aaptPath, $"dump badging \"{apkFilePath}\"");
                    packageName = ExtractValue(output, "package: name='", "'");
                    versionName = ExtractValue(output, "versionName='", "'");
                    versionCode = ExtractValue(output, "versionCode='", "'");

                    var permissionMatches = Regex.Matches(output, @"uses-permission: name='([^']+)' ");
                    foreach (Match match in permissionMatches)
                    {
                        permissions.Add(match.Groups[1].Value);
                    }
                }
                else
                {
                    packageName = "Unknown";
                    versionName = "Unknown";
                    versionCode = "Unknown";
                    permissions.Add("aapt not found");
                }
            }
            catch (Exception ex)
            {
                findings.Add("APK metadata read failed: " + ex.Message);
            }

            if (permissions.Any(p => p.Contains("READ_SMS") || p.Contains("SEND_SMS") || p.Contains("CALL_PHONE")))
            {
                findings.Add("Suspicious SMS or phone permissions detected.");
            }

            if (permissions.Any(p => p.Contains("CAMERA") || p.Contains("RECORD_AUDIO")))
            {
                findings.Add("Camera or microphone access is present.");
            }

            if (permissions.Any(p => p.Contains("ACCESS_FINE_LOCATION") || p.Contains("ACCESS_COARSE_LOCATION")))
            {
                findings.Add("Location permission detected.");
            }

            if (permissions.Any(p => p.Contains("INTERNET")))
            {
                findings.Add("Internet permission detected; verify network security.");
            }

            var riskScore = CalculateRiskScore(findings, permissions.Count);

            var report = new ApkReport
            {
                FileName = fileName,
                PackageName = packageName,
                VersionName = versionName,
                VersionCode = versionCode,
                Permissions = permissions,
                Findings = findings,
                RiskScore = riskScore,
                CreatedAt = DateTime.Now
            };

            var reportFilePath = await _reportGenerator.GenerateAsync(report);

            return new ApkAnalysisResult
            {
                FileName = fileName,
                PackageName = packageName,
                VersionName = versionName,
                VersionCode = versionCode,
                FilePath = apkFilePath,
                Permissions = permissions,
                Findings = findings,
                RiskScore = riskScore,
                ReportPath = reportFilePath,
                CreatedAt = DateTime.Now
            };
        }

        private static string ExtractValue(string content, string startText, string endText)
        {
            var index = content.IndexOf(startText, StringComparison.Ordinal);
            if (index < 0) return string.Empty;

            var start = index + startText.Length;
            var end = content.IndexOf(endText, start, StringComparison.Ordinal);

            if (end < 0) return string.Empty;

            return content.Substring(start, end - start);
        }

        private static string GetToolPath(string toolName)
        {
            var paths = new[]
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Android", "Sdk", "platform-tools", toolName + ".exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Android", "Sdk", "build-tools", toolName + ".exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Android", "Sdk", "platform-tools", toolName + ".exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Android", "Sdk", "platform-tools", toolName),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Android", "Sdk", "platform-tools", toolName)
            };

            foreach (var path in paths)
            {
                if (File.Exists(path))
                    return path;
            }

            return string.Empty;
        }

        private static int CalculateRiskScore(List<string> findings, int permissionCount)
        {
            int score = 0;

            score += findings.Count * 10;
            score += permissionCount * 2;

            if (score > 100) score = 100;
            return score;
        }
    }
}
