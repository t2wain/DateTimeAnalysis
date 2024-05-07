namespace UsageAnalysisLib.Entities
{
    public interface ISessionData : IEventData
    {
        string EventCorrelId { get; }
    }
}
