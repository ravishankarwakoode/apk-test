using APKAnalyzerApp.Models;

namespace APKAnalyzerApp.Services
{
    public interface IReportGenerator
    {
        Task<string> GenerateAsync(ApkReport report);
    }
}
