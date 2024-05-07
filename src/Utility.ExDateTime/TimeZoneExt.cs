using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Utility.ExDateTime
{
    public static class TimeZoneExt
    {

        #region Timezones

        public const string HOUSTON_TZ = "Central Standard Time";
        public const string UK_TZ = "GMT Standard Time";
        public const string INDIA_TZ = "India Standard Time";
        public const string SINGAPORE_TZ = "Singapore Standard Time";
        public const string VN_TZ = "SE Asia Standard Time";
        public const string SAUDI_TZ = "Arab Standard Time";

        public static TimeSpan HOUSTON_UTC_OFFSET => TimeSpan.FromHours(-6);
        public static TimeSpan UK_UTC_OFFSET = TimeSpan.FromHours(0);
        public static TimeSpan INDIA_UTC_OFFSET = new TimeSpan(5, 30, 0);
        public static TimeSpan SINGAPORE_UTC_OFFSET = TimeSpan.FromHours(8);
        public static TimeSpan VN_UTC_OFFSET = TimeSpan.FromHours(7);
        public static TimeSpan SAUDI_UTC_OFFSET = TimeSpan.FromHours(4);


        /// <summary>
        /// Get equivalent TimeZoneInfo.
        /// Also consider daylight saving time.
        /// </summary>
        public static List<TimeZoneInfo> ExGetMatchingTimeZones(this DateTimeOffset time) =>
            TimeZoneInfo.GetSystemTimeZones()
                // tz.GetUtcOffset(time) also consider DST
                .Where(tz => tz.GetUtcOffset(time) == time.Offset)
                .ToList();


        /// <summary>
        /// Get equivalent TimeZoneInfo.
        /// Does not consider daylight saving time.
        /// </summary>
        /// <param name="utcOffset">Search the web for standard UTC offset</param>
        public static List<TimeZoneInfo> ExGetStandardTimeZones(TimeSpan utcOffset) =>
            TimeZoneInfo.GetSystemTimeZones()
                // tz.BaseUtcOffset is the standard UTC.
                // Does not consider DST.
                .Where(tz => tz.BaseUtcOffset == utcOffset)
                .ToList();

        #endregion

        #region CultureInfo

        public static string HOUSTON_CU = "en-US";
        public static string UK_CU = "en-GB";
        public static string INDIA_CU = "en-IN";
        public static string SINGAPORE_CU = "en-SG";
        public static string VN_CU = "vi-VN";


        public static IEnumerable<CultureInfo> GetAllCultureInfo() =>
            CultureInfo.GetCultures(CultureTypes.AllCultures);

        #endregion

        #region Core business hours

        /// <summary>
        /// Check if the date range data is overlap
        /// with the core business hours.
        /// </summary>
        /// <param name="core">Refence core business hours</param>
        /// <param name="range">Date range data</param>
        /// <returns></returns>
        public static bool ExIsCoreHour(this CoreHour core, DateRange range)
        {
            if (core.FromHour == null || core.ToHour == null)
                return true;

            // convert data range data to the same time zone 
            // of the reference core business hours
            var tzRange = new DateRange(
                TimeZoneInfo.ConvertTime(range.FromDate, core.TimeZone),
                TimeZoneInfo.ConvertTime(range.ToDate, core.TimeZone)
            );

            // calculate the reference core business hour date window
            var coredate = ExGetDateOnly(tzRange.FromDate);
            var coreWindow = new DateRange(coredate.AddHours(core.FromHour.Value), coredate.AddHours(core.ToHour.Value));

            // calculate if there is overlap
            var ol = tzRange.ExIsOverlapWindow(coreWindow);
            if (!ol)
            {
                // date range may cross the next day
                // TODO: date range may span across more than 1 day
                coredate = ExGetDateOnly(tzRange.ToDate);
                coreWindow = new DateRange(coredate.AddHours(core.FromHour.Value), coredate.AddHours(core.ToHour.Value));
                ol = tzRange.ExIsOverlapWindow(coreWindow);
            }

            return ol;
        }

        #endregion

        public static DateTimeOffset ExAddTimeZone(DateTime time, TimeZoneInfo timezone)
        {
            var dt = DateTime.SpecifyKind(time, DateTimeKind.Unspecified);
            return new DateTimeOffset(dt, timezone.GetUtcOffset(dt));
        }

        public static DateTimeOffset ExGetDateOnly(DateTimeOffset dateTime)
        {
            var dateOnly = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day,
                0, 0, 0, DateTimeKind.Unspecified);
            return new DateTimeOffset(dateOnly, dateTime.Offset);
        }

        #region Date overlap

        /// <summary>
        /// Given a date range data and a reference date windown, check
        /// if the date range is overlap with the date window.
        /// </summary>
        /// <param name="range">Date range data</param>
        /// <param name="dateWindow">Reference date window</param>
        public static bool ExIsOverlap(this DateRange range, DateRange dateWindow) =>
            // OverlapWithinWindow + OverlapRightOfWindow
            (range.FromDate >= dateWindow.FromDate && range.FromDate < dateWindow.ToDate) ||
            // OverlapWithinWindow + OverlapLeftOfWindow
            (range.ToDate >= dateWindow.FromDate && range.ToDate < dateWindow.ToDate) ||
            // OverlapWithinWindow
            (range.FromDate >= dateWindow.FromDate && range.ToDate < dateWindow.ToDate) ||
            // OverlapOverWindow
            (range.FromDate <= dateWindow.FromDate && range.ToDate > dateWindow.ToDate);


        /// <summary>
        /// Given a date range data and a reference date windown, check
        /// if the date range is overlap with the date window.
        /// </summary>
        /// <param name="range">Date range data</param>
        /// <param name="dateWindow">Reference date window</param>
        public static bool ExIsOverlapWindow(this DateRange range, DateRange dateWindow) =>
            range.ExIsOverlapLeftOfWindow(dateWindow)
                || range.ExIsOverlapRightOfWindow(dateWindow)
                || range.ExIsOverlapWithinWindow(dateWindow)
                || range.ExIsOverlapOverWindow(dateWindow);


        /// <summary>
        /// Given a date range data and a reference date windown, calculate
        /// the time that the date range is overlap with the date window.
        /// </summary>
        /// <param name="range">Date range data</param>
        /// <param name="dateWindow">Reference date window</param>
        public static TimeSpan ExOverLapTimeSpan(this DateRange range, DateRange dateWindow)
        {
            if (range.ExIsOverlapLeftOfWindow(dateWindow))
                return range.ToDate - dateWindow.FromDate;
            else if (range.ExIsOverlapRightOfWindow(dateWindow))
                return dateWindow.ToDate - range.FromDate;
            else if (range.ExIsOverlapWithinWindow(dateWindow))
                return range.ToDate - range.FromDate;
            else if (range.ExIsOverlapOverWindow(dateWindow))
                return dateWindow.ToDate - dateWindow.FromDate;
            else return TimeSpan.Zero;
        }


        /// <summary>
        /// Given a date range data and a reference date windown, check
        /// if the date range is overlap with the FromDate of the date window.
        /// </summary>
        /// <param name="range">Date range data</param>
        /// <param name="dateWindow">Reference date window</param>
        public static bool ExIsOverlapLeftOfWindow(this DateRange range, DateRange dateWindow) =>
            range.FromDate < dateWindow.FromDate 
                && range.ToDate > dateWindow.FromDate 
                && range.ToDate <= dateWindow.ToDate;


        /// <summary>
        /// Given a date range data and a reference date windown, check
        /// if the date range is overlap with the ToDate of the date window.
        /// </summary>
        /// <param name="range">Date range data</param>
        /// <param name="dateWindow">Reference date window</param>
        public static bool ExIsOverlapRightOfWindow(this DateRange range, DateRange dateWindow) =>
            range.FromDate >= dateWindow.FromDate
                && range.FromDate < dateWindow.ToDate
                && range.ToDate > dateWindow.ToDate;


        /// <summary>
        /// Given a date range data and a reference date windown, check
        /// if the date range is overlap entirely within the date window.
        /// </summary>
        /// <param name="range">Date range data</param>
        /// <param name="dateWindow">Reference date window</param>
        public static bool ExIsOverlapWithinWindow(this DateRange range, DateRange dateWindow) =>
            range.FromDate >= dateWindow.FromDate
                && range.ToDate <= dateWindow.ToDate;


        /// <summary>
        /// Given a date range data and a reference date windown, check
        /// if the date range is overlap entirely over the date window.
        /// </summary>
        /// <param name="range">Date range data</param>
        /// <param name="dateWindow">Reference date window</param>
        public static bool ExIsOverlapOverWindow(this DateRange range, DateRange dateWindow) =>
            range.FromDate < dateWindow.FromDate
                && range.ToDate > dateWindow.ToDate;

        #endregion

        public static TimeSpan TimeDuration(this DateRange dr) =>
            (dr.ToDate - dr.FromDate);
    }
}
