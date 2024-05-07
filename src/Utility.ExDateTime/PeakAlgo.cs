using System;
using System.Collections.Generic;
using System.Linq;
using Utility.ExDateTime.Entities;
using TZ = Utility.ExDateTime.TimeZoneExt;

namespace Utility.ExDateTime
{
    /// <summary>
    /// Perform standard usage analysis from 
    /// usage event data and usage session data.
    /// </summary>
    public static class PeakAlgo
    {

        #region Calculate peaks from usage sessions

        /// <summary>
        /// Calculate the peak level for each slice of time
        /// per license server
        /// </summary>
        public static IEnumerable<PeakCount> CalcPeak(this IEnumerable<ISession> sessions, 
            TimeSpan period)
        {
            // Determine the date range for this set of logs
            var fromDate = TZ.ExGetDateOnly(sessions.Min(l => l.DateRange.FromDate));
            var toDate = TZ.ExGetDateOnly(sessions.Max(l => l.DateRange.ToDate).AddDays(1));

            // Calculate the number of periods over the date range
            var steps = (toDate - fromDate).Divide(period) + 1;

            return Enumerable.Range(0, steps) // iterate through each period
                .Select(s => new
                {
                    // determine the date range for the period
                    FromDate = fromDate.Add(period.Multiply(s)),
                    ToDate = fromDate.Add(period.Multiply(s + 1)),
                    Sessions = sessions.Where(l => l.IsWithinDateRange(
                        // collect the logs within the date range step
                        fromDate.Add(period.Multiply(s)),
                        fromDate.Add(period.Multiply(s + 1))
                    ))
                    .ToList()
                })
                .Where(o => o.Sessions.Count > 0)
                // create a record for each period step
                .Select(o => new PeakCount 
                {
                    DateRange = new(o.FromDate, o.ToDate),
                    PeakDuration = period,
                    PeakLevel = o.Sessions.Count,
                    Sessions = o.Sessions.ToList()
                })
                .ToList();
        }


        /// <summary>
        /// Calculate the peak levels for each time period from sessions
        /// </summary>
        public static IEnumerable<PeakCount> CalcPeak2Steps(this IEnumerable<ISession> sessions,
            TimeSpan period, CoreHour? core, int? maxPeakLevel = null)
        {

            // calculation involve 2 steps
            // first step is to calculate how many sessions are active
            // for each minutes between the min/max date ranges of the sessions.
            // The smaller the time period step, the more accurate when calculating
            // the duration of the peak level.
            var peaks1 = sessions.CalcPeak(TimeSpan.FromMinutes(1));

            // second step is to sum up the sessions that are active for each
            // hourly period but only for the periods with maximum
            // peak level.
            var peaks2 = peaks1.CalcPeakDurationOverDatePeriod(
                // sum duration by hourly period
                period,
                core,
                // but only for periods of maximum peak level
                // which is differ for each server
                maxPeakLevel
            );

            return peaks2;
        }

        #endregion

        #region Calculate peak duration over a time period

        /// <summary>
        /// Calculate peak duration over a wider timespan than
        /// the previous calculation.
        /// </summary>
        public static IEnumerable<PeakCount> CalcPeakDurationOverDatePeriod(this IEnumerable<PeakCount> peaks, 
            TimeSpan datePeriod, CoreHour? core, int? maxPeakLevel = null)
        {

            // Determine the date range for this set of logs
            var fromDate = peaks.Min(p => p.DateRange.FromDate);
            var toDate = peaks.Max(p => p.DateRange.ToDate);
            if (core == null)
            {
                fromDate = TZ.ExGetDateOnly(fromDate);
                toDate = TZ.ExGetDateOnly(toDate).AddDays(1);
            }
            else
            {
                // the date are now adjust for local time zones.
                // Note, all dates in the data contains offset hour information,
                // and therefore, .NET will automatically perform the correct date calculation
                // while considering the hour offset differences of each data session as long
                // as all date maths and comparison are done using the DateTimeOffset data type.
                fromDate = TZ.ExGetDateOnly(TimeZoneInfo.ConvertTime(fromDate, core.TimeZone));
                toDate = TZ.ExGetDateOnly(TimeZoneInfo.ConvertTime(toDate, core.TimeZone).AddDays(1));
            }

            // Calculate the number of periods over the date range
            var steps = (toDate - fromDate).Divide(datePeriod) + 1;

            // iterate through each period
            var lst = Enumerable.Range(0, steps) 
                .Select(s => new
                {
                    // determine the date range for the period step
                    FromDate = fromDate.Add(datePeriod.Multiply(s)),
                    ToDate = fromDate.Add(datePeriod.Multiply(s + 1)),
                    Peaks = peaks.Where(p => p.IsWithinDateRange(
                        // collect the logs within the date range
                        fromDate.Add(datePeriod.Multiply(s)), 
                        fromDate.Add(datePeriod.Multiply(s + 1))
                    ) 
                    // and only if date range is overlap the core hour
                    && p.IsCoreHour(core) )
                    .ToList()
                })
                .Where(o => o.Peaks.Count > 0)
                // create a record for each period
                .Select(o => new PeakCount {
                    DateRange = new(o.FromDate, o.ToDate),
                    PeakLevel = o.Peaks.CalcMaxPeakLevel(maxPeakLevel), // peak
                    // include only logs where peak is GTE than
                    // the given thresshold max license per server
                    PeakDuration = o.Peaks.WhereMaxPeakLevel(maxPeakLevel)
                        // TODO: Should only sum the portion
                        // that is actually overlap with the date window
                        .Select(l => l.PeakDuration)
                        .SunTimeSpan(), // duration
                    Sessions = o.Peaks.SelectMany(p => p.Sessions).ToList()
                })
                .OrderByDescending(o => o.DateRange.FromDate)
                .ToList();

            return lst;
        }

        #endregion

        #region Events Analysis

        public static IEnumerable<PeakCount> GroupByPeriod(this IEnumerable<IEvent> events,
            TimeSpan period)
        {
            // Determine the date range for this set of logs
            var fromDate = TZ.ExGetDateOnly(events.Min(e => e.EventDate));
            var toDate = TZ.ExGetDateOnly(events.Max(e => e.EventDate).AddDays(1));

            // Calculate the number of periods over the date range
            var steps = Convert.ToInt32((toDate - fromDate).Divide(period) + 1);

            return Enumerable.Range(0, steps) // iterate through each period
                .Select(s => new
                {
                    // determine the date range for the period
                    FromDate = fromDate.Add(period.Multiply(s)),
                    ToDate = fromDate.Add(period.Multiply(s + 1)),
                    Events = events.Where(e => e.IsWithinDateRange(
                        // collect the logs within the date range step
                        fromDate.Add(period.Multiply(s)),
                        fromDate.Add(period.Multiply(s + 1))
                    )).ToList()
                })
                .Where(o => o.Events.Count() > 0)
                .Select(o => new PeakCount
                {
                    DateRange = new(o.FromDate, o.ToDate),
                    PeakLevel = o.Events.Count(),
                    PeakDuration = period,
                    Events = o.Events
                })
                .ToList();
        }

        #endregion

        #region Utilities

        public static TimeSpan Multiply(this TimeSpan span, int step) => 
            TimeSpan.FromSeconds(span.TotalSeconds * step);

        public static int Divide(this TimeSpan dividend, TimeSpan divisor) =>
            Convert.ToInt32(dividend.TotalSeconds / divisor.TotalSeconds);

        static int CalcMaxPeakLevel(this IEnumerable<PeakCount> peaks, int? maxPeakLevel) =>
            maxPeakLevel switch
            {
                null => peaks.Max(p => p.PeakLevel),
                _ => Math.Min(peaks.Max(p => p.PeakLevel), maxPeakLevel.Value)
            };

        static IEnumerable<PeakCount> WhereMaxPeakLevel(this IEnumerable<PeakCount> peaks, int? maxPeakLevel)
        {
            var max = peaks.CalcMaxPeakLevel(maxPeakLevel);
            return peaks.Where(p => p.PeakLevel >= max).ToList();
        }

        static TimeSpan SunTimeSpan(this IEnumerable<TimeSpan> times) =>
            times.Aggregate((agg, t) => agg + t); 

        static bool IsWithinDateRange(this ISession log, DateTimeOffset fromDate, DateTimeOffset toDate) =>
            new DateRange(log.DateRange.FromDate, log.DateRange.ToDate).ExIsOverlapWindow(new(fromDate, toDate));

        public static bool IsWithinDateRange(this IEvent evt, DateTimeOffset fromDate, DateTimeOffset toDate) =>
            new DateRange(evt.EventDate, evt.EventDate).ExIsOverlapWindow(new(fromDate, toDate));

        static bool IsWithinDateRange(this PeakCount peak, DateTimeOffset fromDate, DateTimeOffset toDate) =>
            new DateRange(peak.DateRange.FromDate, peak.DateRange.ToDate).ExIsOverlapWindow(new(fromDate, toDate));

        static bool IsCoreHour(this ISession session, CoreHour core) =>
            core.ExIsCoreHour(new DateRange(session.DateRange.FromDate, session.DateRange.ToDate));

        static bool IsCoreHour(this PeakCount peak, CoreHour? core) =>
            core switch
            {
                null => true,
                _ => core.ExIsCoreHour(new DateRange(peak.DateRange.FromDate, peak.DateRange.ToDate))
            };

        #endregion
    }
}
