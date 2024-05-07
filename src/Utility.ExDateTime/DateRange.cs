using System;

namespace Utility.ExDateTime
{
    public record DateRange
    {
        public DateRange(DateTimeOffset fromDate, DateTimeOffset toDate)
        {
            FromDate = fromDate;
            ToDate = toDate;
        }
        public DateTimeOffset FromDate { get; }
        public DateTimeOffset ToDate { get; }
    }
}
