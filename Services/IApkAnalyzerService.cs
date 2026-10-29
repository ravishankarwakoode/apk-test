using APKAnalyzerApp.Models;

namespace APKAnalyzerApp.Services
{
    public interface IApkAnalyzerService
    {
        Task<ApkAnalysisResult> AnalyzeAsync(string apkFilePath);
    }
}
