using System.Collections.Generic;
using Utility.ExDateTime.Entities;
using System.Linq;

namespace Utility.ExDateTime
{
    public static class EventAlgo
    {
        public record Result
        {
            public IEnumerable<(IEvent Start, IEvent End)> Sessions { get; set; } = null!;
            public IEnumerable<IEvent> UnMatchedEvents { get; set; } = null!;
        }

        /// <summary>
        /// Correlating two (2) IEvent records to create a (1) ISession record
        /// </summary>
        public static Result CreateSessions(this IEnumerable<IEvent> events, int correlIdKey)
        {
            var q = events
                // expecting a pair of IEvent to create a record of ISession
                .GroupBy(e => e.GetCorrelId(correlIdKey))
                .ToList();

            var err = q
                .Where(g => g.Count() != 2)
                .SelectMany(g => g.Select(v => v))
                .ToList();

            var sessions = q
                .Where(g => g.Count() == 2)
                .Select(g => g.CreateSession());

            return new Result
            {
                Sessions = sessions,
                UnMatchedEvents = err
            };
        }

        // expecting only 2 events in the collection
        static (IEvent Start, IEvent End) CreateSession(this IEnumerable<IEvent> events)
        {
            var startEvent = events.Where(e => e.EventDate == events.Min(e => e.EventDate)).First();
            var endEvent = events.Where(e => e.EventDate == events.Max(e => e.EventDate)).First();
            return (startEvent, endEvent);
        }
    }
}
