## Peak level Analysis

Usage data are often available as either event or session. If only event data are available, then a session must be created from 2 correlating events, example are checked-out and checked-in events.

- CreateSessions ( EventAlgo )

Given a set of session data where each session represents a license currently being used for a time duration, the peak license usage can be calculated for various time periods, such as for hourly or daily time period. The peak level can be plotted on a time line to visualize the availability of licenses.

- CalcPeak2Steps ( PeakAlgo )

Note, peak level is not the same as peak duration. The peak duration for an hourly period can varies between 1 and 60 minutes. In general, a wider time period calculation (like daily), the less detail is known about the peak duration. Also, peak duration within the peak time period is not necessary a continuous duration. The peak level might be fluctuating within the time period.

## Date Time Analysis

Usage data are dependent on date time. Actual data may be generated on different servers at various locations in different time zones. Users are also may be located in various locations in different time zones. Therefore, the date time of data from different time zones must be align to the same time scale for the analysis.

Also, sometime only usage statistic during core business hours are relevant. However, core business hours will be different for each locations in different time zones and also varied according to local laws.

- CoreHour is a parameter to specify that result from calculation is to be adjusted for the local time zone and/or core business hours

All dates in the usage data are stored as DateTimeOffset which contains the proper hour offset for various time zones. Therefore, .NET will automatically perform the correct date calculation while considering various different hour offsets of dates in the data.