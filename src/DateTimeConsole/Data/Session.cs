using Utility.ExDateTime;
using Utility.ExDateTime.Entities;

namespace DateTimeConsole.Data
{
    /// <summary>
    /// Represent an occurrence over
    /// a period of time marked by
    /// a starting event and an ending event.
    /// </summary>
    public record Session : ISession
    {
        public int Id { get; set; }
        public DateRange DateRange { get; set; } = null!;
        public IEvent StartEvent { get; set; } = null!;
        public IEvent EndEvent { get; set; } = null!;
        public IData? SessionData { get; set; }
    }
}
