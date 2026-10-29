namespace APKAnalyzerApp.Models
{
    public class ApkReport
    {
        public string FileName { get; set; } = string.Empty;
        public string PackageName { get; set; } = string.Empty;
        public string VersionName { get; set; } = string.Empty;
        public string VersionCode { get; set; } = string.Empty;
        public List<string> Permissions { get; set; } = new();
        public List<string> Findings { get; set; } = new();
        public int RiskScore { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
