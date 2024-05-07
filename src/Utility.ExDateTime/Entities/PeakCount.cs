using System;
using System.Collections.Generic;

namespace Utility.ExDateTime.Entities
{
    public record PeakCount
    {
        public DateRange DateRange { get; set; } = null!;
        public int PeakLevel { get; set; }
        public TimeSpan PeakDuration { get; set; } = TimeSpan.Zero;
        public IEnumerable<ISession> Sessions { get; set; } = [];
        public IEnumerable<IEvent> Events { get; set; } = [];
    }
}
