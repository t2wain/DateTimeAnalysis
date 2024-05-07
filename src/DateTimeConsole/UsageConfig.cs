namespace DateTimeConsole
{
    public record Server
    {
        public string ServerName { get; set; } = null!;
        public int Quantity { get; set; }
        public string TimeZone { get; set; } = null!;
    }

    public record UsageConfig
    {
        public string DataFile { get; set; } = null!;
        public IEnumerable<Server> AvailLicenses { get; set; } = [];
    }
}
