using System;

namespace UsageAnalysisLib.Entities
{
    public record UserUsage {
        public string LicenseServerName { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public DateTimeOffset? FromDate { get; set; }
        public DateTimeOffset? ToDate { get; set; }
        public int? TotalMinute { get; set; }
        public string? RefId { get; set; }
    }
}
