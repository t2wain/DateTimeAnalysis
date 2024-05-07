namespace Utility.ExDateTime.Entities
{
    /// <summary>
    /// Represent an occurance over
    /// a period of time marked by
    /// a starting event and an endind event.
    /// </summary>
    public interface ISession
    {
        int Id { get; }
        DateRange DateRange { get; }
        IEvent StartEvent { get; }
        IEvent EndEvent { get; }
        IData? SessionData { get; }
    }
}
