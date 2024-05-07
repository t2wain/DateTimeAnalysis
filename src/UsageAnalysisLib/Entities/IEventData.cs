using System;
using Utility.ExDateTime.Entities;

namespace UsageAnalysisLib.Entities
{
    public interface IEventData : IData
    {
        string UserName { get; }
        string Server { get; }
    }
}
