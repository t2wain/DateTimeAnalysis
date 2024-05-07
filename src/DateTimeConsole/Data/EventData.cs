using UsageAnalysisLib.Entities;
using Utility.ExDateTime.Entities;

namespace DateTimeConsole.Data
{
    public record EventData : IEvent, IEventData
    {
        public int ID { get; set; }
        public int Index { get; set; }
        public DateTimeOffset Time { get; set; }
        public string Action { get; set; } = null!;
        public string UserID { get; set; } = null!;
        // Primary correlation id
        public string Marker { get; set; } = null!;
        public string Server { get; set; } = null!;
        // Secondary correlation id
        public string CorrelationId2 { get; set; } = null!;

        #region IEvent

        int IEvent.Id => ID;

        DateTimeOffset IEvent.EventDate => Time;

        string IEvent.EventType => Action;

        IData? IEvent.EventData => this;

        string IEvent.GetCorrelId(int key) => key switch
        {
            1 => Marker, // primary correlation id
            2 => CorrelationId2, // secondary correlation id
            _ => ""
        };

        #endregion

        #region IEventData

        string IEventData.UserName => UserID;

        string IEventData.Server => Server;

        #endregion
    }
}
