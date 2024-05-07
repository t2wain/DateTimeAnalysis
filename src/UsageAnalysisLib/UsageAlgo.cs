using System;
using System.Collections.Generic;
using System.Linq;
using UsageAnalysisLib.Entities;
using Utility.ExDateTime;
using Utility.ExDateTime.Entities;
using TZ = Utility.ExDateTime.TimeZoneExt;

namespace UsageAnalysisLib
{
    public static class UsageAlgo
    {

        #region User Analysis

        /// <summary>
        /// Calculate statistics on usage level of each users
        /// </summary>
        /// <param name="hoursThreshold">hours per day threshold to be considered as an active day</param>
        /// <param name="daysThreshold">number of active day threshold to be considered as an active user</param>
        /// <returns></returns>
        public static IEnumerable<ActiveUser> CalcActiveUsers(this IEnumerable<ISession> sessions, 
            int hoursThreshold, int daysThreshold) =>
            sessions
                .Select(s => new 
                { 
                    Data = (ISessionData)s.SessionData!,
                    Session = s,
                    MinuteDuration = s.DateRange.TimeDuration().TotalMinutes
                })
                // analyze per user
                .GroupBy(s => s.Data.UserName)
                .Select(g => new
                {
                    UserName = g.Key,
                    TotalDays = g.GroupBy(s => s.Session.DateRange.FromDate.Date).Count(),
                    ActiveDays = g.GroupBy(s => s.Session.DateRange.FromDate.Date)
                        .Where(o => o.Sum(s => s.MinuteDuration) >= hoursThreshold)
                        .Count(),
                    TotalMinutes = g.Sum(s => s.MinuteDuration),
                    NumberOfSessions = g.Count(),
                    MaxSessionDuration = g.Max(l => l.MinuteDuration),
                    ExtendedSessionCount = g.Count(o => o.MinuteDuration > TimeSpan.FromHours(12).TotalMinutes),
                    // concurrent license usages
                    Peaks = g.Select(o => o.Session)
                        .CalcPeak(TimeSpan.FromMinutes(1))
                        .Where(p => p.PeakLevel > 1)
                })
                .Select(o => new ActiveUser
                {
                    UserName = o.UserName,
                    TotalDays = o.TotalDays,
                    ActiveDays = o.ActiveDays,
                    TotalMinutes = o.TotalMinutes,
                    NumberOfSessions = o.NumberOfSessions,
                    MaxSessionDuration = o.MaxSessionDuration,
                    ConcurrentSessionCount = o.Peaks
                        .SelectMany(p => p.Sessions
                            .Select(s => ((ISessionData)s.SessionData!).EventCorrelId))
                        .Distinct()
                        .Count(),
                    // each peak is 1 min duration
                    ExtendedSessionCount = o.ExtendedSessionCount,
                    ConcurrentSessionMinutes = o.Peaks.Count(), // becasue peak period is 1 minute
                    MaxConcurrentPeak = o.Peaks.Count() switch
                    {
                        0 => null,
                        _ => o.Peaks.Max(p => p.PeakLevel)
                    },
                })
                .Select(i => i with
                {
                    UserClass = i.ActiveDays >= daysThreshold ? "Active" : null
                })
            .ToList();


        /// <summary>
        /// Calculate the total durations of usage for the user
        /// for the day when licenses were CHECKOUT
        /// </summary>
        public static IEnumerable<UserUsage> CalcUserUsageLevelSum(this IEnumerable<ISession> sessions) =>
            sessions
                .Select(s => new { 
                    Session = s, 
                    Data = (ISessionData)s.SessionData!, 
                    MinuteDuration = Convert.ToInt32((s.DateRange.ToDate - s.DateRange.FromDate).TotalMinutes)
                })
                .GroupBy(l => new { l.Data.Server, l.Data.UserName })
                .Select(g => new UserUsage {
                    LicenseServerName = g.Key.Server,
                    UserName = g.Key.UserName,
                    FromDate = TZ.ExGetDateOnly(g.Min(i => i.Session.DateRange.FromDate)),
                    ToDate = TZ.ExGetDateOnly(g.Max(i => i.Session.DateRange.ToDate)),
                    TotalMinute = g.Sum(s => s.MinuteDuration),
                    RefId = null
                })
                .OrderByDescending(o => o.TotalMinute)
                .ToList();


        /// <summary>
        /// Calculate the duration of usage each time user
        /// CHECKOUT a license
        /// </summary>
        public static IEnumerable<UserUsage> CalcUserUsageLevel(this IEnumerable<ISession> sessions) =>
            sessions
                .Select(s => new {
                    Session = s,
                    Data = (ISessionData)s.SessionData!,
                    MinuteDuration = Convert.ToInt32((s.DateRange.ToDate - s.DateRange.FromDate).TotalMinutes)
                })
                .Select(s => new UserUsage 
                {
                    LicenseServerName = s.Data.Server,
                    UserName = s.Data.UserName, 
                    FromDate = s.Session.DateRange.FromDate, 
                    ToDate = s.Session.DateRange.ToDate, 
                    TotalMinute = s.MinuteDuration, 
                    RefId = s.Data.EventCorrelId 
                })
                .OrderByDescending(o => o.ToDate)
                .ToList();


        /// <summary>
        /// Calculate those events without a matching correlation id.
        /// </summary>
        /// <param name="eventTypes">The event types to correlate (CHECKOUT, CHECKIN)</param>
        /// <param name="correlKey">An event may have more than one correlation ids</param>
        /// <returns></returns>
        public static IEnumerable<IEvent> GetUnCorrelatedEvent(this IEnumerable<IEvent> events, int correlKey) =>
            events
                .Select(e => new {
                    Event = e,
                    Data = (IEventData)e.EventData!
                })
                .GroupBy(e => new { e.Data.Server, CorrelId = e.Event.GetCorrelId(correlKey) })
                .Where(g => g.Count() < 2)
                .SelectMany(g => g.Select(e => e.Event))
                .ToList();


        /// <summary>
        /// Current users are those events (ex. checked-out) 
        /// without a corresponding event pair (ex. checked-in). 
        /// </summary>
        /// <param name="prevDays">Only consider those unpaired events 
        /// within recent days as current users</param>
        public static IEnumerable<UserUsage> GetCurrentUsers(this IEnumerable<IEvent> events, int prevDays, int correlKey) =>
            events
                .Select(e => new {
                    Event = e,
                    Data = (IEventData)e.EventData!
                })
                .GroupBy(e => new { e.Data.Server, CorrelId = e.Event.GetCorrelId(correlKey) })
                .Where(g => g.Count() != 2)
                .SelectMany(g => g.Select(e => e))
                .Where(e => e.Event.EventDate > DateTimeOffset.Now.AddDays(prevDays * -1))
                .OrderBy(e => e.Data.Server)
                .ThenByDescending(e => e.Event.EventDate)
                .Select(l => new UserUsage {
                    LicenseServerName = l.Data.Server,
                    UserName = l.Data.UserName,
                    FromDate = l.Event.EventDate,
                    ToDate = new DateTimeOffset(DateTime.UtcNow, TimeSpan.Zero),
                    TotalMinute = Convert.ToInt32((DateTimeOffset.Now - l.Event.EventDate).TotalMinutes),
                    RefId = l.Event.GetCorrelId(correlKey)
                })
                .ToList();

        #endregion

    }
}
