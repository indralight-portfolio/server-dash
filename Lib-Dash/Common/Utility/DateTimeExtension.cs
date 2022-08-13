using System;

namespace Common.Utility
{
    public static class DateTimeExtension
    {
        public static DateTime Truncate(this DateTime dateTime, long resolution = TimeSpan.TicksPerMillisecond)
        {
            return new DateTime(dateTime.Ticks - (dateTime.Ticks % resolution), dateTime.Kind);
        }
        public static long ToUnixTimestamp(this DateTime dateTime)
        {
            return (int)(dateTime.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        }

        public static DateTime FromUnixTimestamp(long unixTimestamp)
        {
            System.DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            epoch = epoch.AddMilliseconds(unixTimestamp);
            return epoch;
        }

        public static DateTime ToKST(this DateTime dateTime)
        {
            return dateTime.AddHours(9);
        }

        public static string ToString_DateTime(this DateTime? datetime)
        {
            return datetime == null ? string.Empty : ((DateTime)datetime).ToString_DateTime();
        }
        public static string ToString_DateTime(this DateTime datetime)
        {
            return datetime.ToString("yyyy-MM-dd HH:mm:ss");
        }
        public static string ToString_Date(this DateTime datetime)
        {
            return datetime.ToString("yyyy-MM-dd");
        }

        public static string ToString_ISO(this DateTime dateTime)
        {
            return dateTime.ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'");
        }

        public static string ToFileName(this DateTime dateTime)
        {
            return dateTime.ToString("yyyy-MM-dd-HH-mm-ss");
        }

        public static DateTime ToLocalTime(this DateTime dateTime, sbyte offset)
        {
            return dateTime.AddHours(offset);
        }
        public static DateTime ToLocalTime(this DateTime dateTime, TimeSpan offset)
        {
            return dateTime.Add(offset);
        }
        public static DateTime ToUtcTime(this DateTime dateTime, TimeSpan offset)
        {
            return dateTime.Add(-offset);
        }

        public static DateTime FirstDayOfYear(this DateTime dateTime)
        {
            return dateTime.AddDays(1 - dateTime.DayOfYear);
        }

        public static TimeSpan TuncateDay(this TimeSpan timeSpan)
        {
            if (timeSpan.Days > 0)
                return timeSpan - TimeSpan.FromDays(timeSpan.Days);
            else if (timeSpan.Hours < 0)
                return timeSpan + TimeSpan.FromHours(24);
            else
                return timeSpan;
        }
    }

    public class LocalTimeComparer
    {
        public enum ComparerType
        {
            Daily,
            Weekly,
            Monthly,
        }

        public TimeSpan Offset;
        public ComparerType ResetType;
        public byte ResetHour;
        public DayOfWeek ResetDayOfWeek;
        public int ResetDayOfMonth;

        //public LocalTimeComparer(sbyte offset, byte resetHour) : this(TimeSpan.FromHours(offset), resetHour) { }
        public LocalTimeComparer(TimeSpan offset, byte resetHour)
        {
            Offset = offset;
            ResetType = ComparerType.Daily;
            ResetHour = resetHour;
        }

        public LocalTimeComparer(TimeSpan offset, DayOfWeek resetDayOfWeek, byte resetHour)
        {
            Offset = offset;
            ResetType = ComparerType.Weekly;
            ResetDayOfWeek = resetDayOfWeek;
            ResetHour = resetHour;
        }

        public LocalTimeComparer(TimeSpan offset, byte resetDayOfMonth, byte resetHour)
        {
            Offset = offset;
            ResetType = ComparerType.Monthly;
            ResetDayOfMonth = resetDayOfMonth;
            ResetHour = resetHour;
        }

        public bool IsAfterResetTime(DateTime utcNow, DateTime lastTime)
        {
            DateTime localNow = utcNow.ToLocalTime(Offset);
            DateTime resetLocalTime = GetNextResetLocalTime(lastTime);
            return localNow > resetLocalTime;
        }
        public DateTime GetNextResetTime(DateTime lastTime)
        {
            var resetLocalTime = GetNextResetLocalTime(lastTime);
            return resetLocalTime.ToUtcTime(Offset);
        }

        public DateTime GetNextResetLocalTime(DateTime lastTime)
        {
            DateTime localLast = lastTime.ToLocalTime(Offset);
            DateTime resetTime = localLast.Date;
            if (ResetType == ComparerType.Daily)
            {
                if (localLast.Hour >= ResetHour)
                {
                    resetTime = localLast.Date.AddDays(1).AddHours(ResetHour);
                }
                else
                {
                    resetTime = localLast.Date.AddHours(ResetHour);
                }
            }
            else if (ResetType == ComparerType.Weekly)
            {
                if (localLast.DayOfWeek > ResetDayOfWeek)
                {
                    resetTime = localLast.Date.AddDays(7 + (int)ResetDayOfWeek - (int)localLast.DayOfWeek).AddHours(ResetHour);
                }
                else if (localLast.DayOfWeek < ResetDayOfWeek)
                {
                    resetTime = localLast.Date.AddDays((int)ResetDayOfWeek - (int)localLast.DayOfWeek).AddHours(ResetHour);
                }
                else
                {
                    if (localLast.Hour >= ResetHour)
                    {
                        resetTime = localLast.Date.AddDays(7).AddHours(ResetHour);
                    }
                    else
                    {
                        resetTime = localLast.Date.AddHours(ResetHour);
                    }
                }
            }
            else if (ResetType == ComparerType.Monthly)
            {
                if (localLast.Day > ResetDayOfMonth)
                {
                    resetTime = localLast.Date.AddMonths(1).AddDays(ResetDayOfMonth - localLast.Day).AddHours(ResetHour);
                }
                else if (localLast.Day < ResetDayOfMonth)
                {
                    resetTime = localLast.Date.AddDays(ResetDayOfMonth - localLast.Day).AddHours(ResetHour);
                }
                else
                {
                    if (localLast.Hour >= ResetHour)
                    {
                        resetTime = localLast.Date.AddMonths(1).AddHours(ResetHour);
                    }
                    else
                    {
                        resetTime = localLast.Date.AddHours(ResetHour);
                    }
                }
            }
            return resetTime;
        }
    }
}
