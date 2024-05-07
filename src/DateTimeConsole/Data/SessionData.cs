using UsageAnalysisLib.Entities;

namespace DateTimeConsole.Data
{
    public record SessionData : ISessionData
    {
        public string EventCorrelId { get; set; } = null!;

        public string UserName { get; set; } = null!;

        public string Server { get; set; } = null!;
    }
}
