using System;

namespace Utility.ExDateTime.Entities
{    
     /// <summary>
     /// Represent an occurance at
     /// a point in time.
     /// </summary>
    public interface IEvent
    {
        int Id { get; }
        DateTimeOffset EventDate { get; }
        string EventType { get; }
        IData? EventData { get; }

        /// <summary>
        /// Mulitple event can be related by
        /// a correlation.
        /// </summary>
        string GetCorrelId(int key);
    }
}
