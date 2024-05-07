namespace UsageAnalysisLib.Entities
{
    public record ActiveUser
    {
        public string UserName { get; set; } = null!;
        public string? Location { get; set; }
        public int TotalDays { get; set; }
        public int ActiveDays { get; set; }
        public double TotalMinutes { get; set; }
        public int NumberOfSessions { get; set; }
        public double MaxSessionDuration { get; set; }
        public int ExtendedSessionCount { get; set; }
        public int? ConcurrentSessionCount { get; set; }
        public int? ConcurrentSessionMinutes { get; set; }
        public int? MaxConcurrentPeak { get; set; }
        public string? UserClass { get; set; }
    }
}
