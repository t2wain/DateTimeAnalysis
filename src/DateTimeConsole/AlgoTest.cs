using DateTimeConsole.Data;
using UsageAnalysisLib.Entities;
using Utility.ExDateTime;
using Utility.ExDateTime.Entities;
using UsageAnalysisLib;

namespace DateTimeConsole
{
    public class AlgoTest
    {
        private readonly UsageConfig _cfg;

        public AlgoTest(UsageConfig cfg)
        {
            this._cfg = cfg;
        }

        public void Run(int? testNo = null)
        {
            var t = testNo ?? 2;
            switch (t)
            {
                case 0:
                    ReadData();
                    break;
                case 1:
                    CreateSession(); 
                    break;
                case 2:
                    CalculatePeak();
                    break;
                case 3:
                    CalculateUserStat();
                    break;
            }
        }

        /// <summary>
        /// Read EventData from  a data file
        /// </summary>
        public IEnumerable<EventData> ReadData()
        {
            var data = DataFile.ReadEventData(_cfg.DataFile);
            return data;
        }

        public record SessionResult
        {
            public IEnumerable<ISession> Sessions { get; set; } = [];
            public IEnumerable<IEvent> UnmatchedEvent { get; set; } = [];
        }

        /// <summary>
        /// Read EventData from a data file
        /// then create a list of Sessions
        /// </summary>
        public SessionResult CreateSession()
        {
            // read EventData
            var data = ReadData();

            //// map EventData to Event using Marker as correlation id
            //var events = data.ToEvent();

            // create sessions based on 2 events
            // with matching correlation id.
            var result = data.CreateSessions(1);
            var sessions = result.Sessions
                .Select(i => new Session
                { 
                    Id = i.Start.Id,
                    DateRange = new(i.Start.EventDate, i.End.EventDate),
                    StartEvent = i.Start,
                    EndEvent = i.End,
                    SessionData = new SessionData
                    {
                        EventCorrelId = i.Start.GetCorrelId(1),
                        Server = ((IEventData)i.Start.EventData!).Server,
                        UserName = ((IEventData)i.Start.EventData!).UserName
                    }
                })
                .ToList();

            // re-map the remaing EventData to use 
            // CorrelationId2 as the correlation id
            var events2 = result
                .UnMatchedEvents;

            // create sessions based on 2 events
            // with matching correlation id.
            var result2 = events2.CreateSessions(2);
            var sessions2 = result2.Sessions
                .Select(i => new Session
                {
                    Id = i.Start.Id,
                    DateRange = new(i.Start.EventDate, i.End.EventDate),
                    StartEvent = i.Start,
                    EndEvent = i.End,
                    SessionData = new SessionData
                    {
                        EventCorrelId = i.Start.GetCorrelId(2),
                        Server = ((IEventData)i.Start.EventData!).Server,
                        UserName = ((IEventData)i.Start.EventData!).UserName
                    }
                })
                .ToList();

            var res = new SessionResult
            {
                Sessions = sessions.Concat(sessions2).ToList(),
                // note, the remaining unmatched events might 
                // be current sessions not yet completed
                UnmatchedEvent = result2.UnMatchedEvents
            };

            // active sessions without a matching end event
            var dt = res.UnmatchedEvent.Max(e => e.EventDate);
            var curUsers = res
                .UnmatchedEvent
                .GetCurrentUsers(7, 2)
                .ToList();

            return res;
        }

        /// <summary>
        /// Calculate the hourly peak levels for the recent 10 days
        /// </summary>
        public IEnumerable<(string Server, IEnumerable<PeakCount> Peaks)> CalculatePeak()
        {
            // read in the usage sessions
            var sessions = CreateSession().Sessions;

            // filter the data to the recent 10 days only
            var maxDate = sessions
                .Max(s => s.DateRange.FromDate)
                .AddDays(-10);

            var dservers = _cfg.AvailLicenses.ToDictionary(s => s.ServerName);

            // group the sessions by license servers
            var res = sessions
                .Where(s => s.DateRange.FromDate >= maxDate)
                .GroupBy(s => ((EventData)s.StartEvent.EventData!).Server)
                .Select(g => (Server: g.Key, Sessions: g.Select(s => s).ToList()) )
                .Select(o => (Server: o.Server, Peaks: o.Sessions.CalcPeak2Steps(
                    // by hourly period
                    TimeSpan.FromHours(1),
                    null,
                    // but only for periods of maximum peak level
                    // which is differ for each server. The peak level
                    // represent the max licenses available for use.
                    dservers[o.Server].Quantity
                )))
                .ToList();

            return res;
        }

        public void CalculateUserStat()
        {
            // read in the usage sessions
            var sessions = CreateSession().Sessions;

            // filter the data to the recent 10 days only
            var maxDate = sessions
                .Max(s => s.DateRange.FromDate)
                .AddDays(-10);

            var users = sessions
                .Where(s => s.DateRange.FromDate >= maxDate)
                .CalcActiveUsers(2, 3);
        }
    }
}
