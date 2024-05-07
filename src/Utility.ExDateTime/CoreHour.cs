using System;

namespace Utility.ExDateTime
{
    /// <summary>
    /// Define the core business hours
    /// </summary>
    public record CoreHour
    {
        public int? FromHour { get; set; }
        public int? ToHour { get; set; }
        //public DayOfWeek FromDay { get; set; } = DayOfWeek.Monday;
        //public DayOfWeek ToDay { get; set; } = DayOfWeek.Thursday;
        public TimeZoneInfo TimeZone { get; set; } = TimeZoneInfo.Utc;
    }
}
