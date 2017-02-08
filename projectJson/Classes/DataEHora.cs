using System;
//using System.Data;
//using System.Configuration;
//using System.Web;
//using System.Web.Security;
//using System.Web.UI;
//using System.Web.UI.WebControls;
//using System.Web.UI.WebControls.WebParts;
//using System.Web.UI.HtmlControls;
//using System.Collections.Generic;   
//using System.Text;

namespace Classes
{
    public static class DataEHora
    {
        public static bool IsWorkday(DateTime date)
        {
            // Weekend
            if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                return false;

            // Could add holiday logic here.

            return true;
        }

        public static DateTime StartOfBusiness(DateTime date)
        {
            return new DateTime(date.Year, date.Month, date.Day, 9, 0, 0);
        }

        public static DateTime CloseOfBusiness(DateTime date)
        {
            return new DateTime(date.Year, date.Month, date.Day, 18, 0, 0);
        }

        public static TimeSpan BusinessTimeDelta(DateTime start, DateTime stop)
        {
            if (start == stop)
                return TimeSpan.Zero;

            if (start > stop)
            {
                DateTime temp = start;
                start = stop;
                stop = temp;
            }

            // First we are going to truncate these DateTimes so that they are within the business day.

            // How much time from the beginning til the end of the day?
            DateTime startFloor = StartOfBusiness(start);
            DateTime startCeil = CloseOfBusiness(start);
            if (start < startFloor) start = startFloor;
            if (start > startCeil) start = startCeil;

            TimeSpan firstDayTime = startCeil - start;
            bool workday = true; // Saves doublechecking later
            if (!IsWorkday(start))
            {
                workday = false;
                firstDayTime = TimeSpan.Zero;
            }

            // How much time from the start of the last day til the end?
            DateTime stopFloor = StartOfBusiness(stop);
            DateTime stopCeil = CloseOfBusiness(stop);
            if (stop < stopFloor) stop = stopFloor;
            if (stop > stopCeil) stop = stopCeil;

            TimeSpan lastDayTime = stop - stopFloor;
            if (!IsWorkday(stop))
                lastDayTime = TimeSpan.Zero;

            // At this point all dates are snipped to within business hours.

            if (start.Date == stop.Date)
            {
                if (!workday) // Precomputed value from earlier
                    return TimeSpan.Zero;

                return stop - start;
            }

            // At this point we know they occur on different dates, so we can use
            // the offset from SOB and COB.

            TimeSpan timeInBetween = TimeSpan.Zero;
            TimeSpan hoursInAWorkday = (startCeil - startFloor);

            // I tried cool math stuff instead of a for-loop, but that leaves no clean way to count holidays.
            for (DateTime itr = startFloor.AddDays(1); itr < stopFloor; itr = itr.AddDays(1))
            {
                if (!IsWorkday(itr))
                    continue;

                // Otherwise, it's a workday!
                timeInBetween += hoursInAWorkday;
            }

            return firstDayTime + lastDayTime + timeInBetween;
        }

        public enum DateInterval
        {
            Day,
            DayOfYear,
            Hour,
            Minute,
            Month,
            Quarter,
            Second,
            Weekday,
            WeekOfYear,
            Year
        }

        public static long DateDiff(DateInterval interval, DateTime dt1, DateTime dt2)
        {
            return DateDiff(interval, dt1, dt2, System.Globalization.DateTimeFormatInfo.CurrentInfo.FirstDayOfWeek);
        }

        public static long DateDiff(DateInterval interval, DateTime dt1, DateTime dt2, DayOfWeek eFirstDayOfWeek)
        {
            if (interval == DateInterval.Year)
                return dt2.Year - dt1.Year;

            if (interval == DateInterval.Month)
                return (dt2.Month - dt1.Month) + (12 * (dt2.Year - dt1.Year));

            TimeSpan ts = dt2 - dt1;

            if (interval == DateInterval.Day || interval == DateInterval.DayOfYear)
                return Round(ts.TotalDays);

            if (interval == DateInterval.Hour)
                return Round(ts.TotalHours);

            if (interval == DateInterval.Minute)
                return Round(ts.TotalMinutes);

            if (interval == DateInterval.Second)
                return Round(ts.TotalSeconds);

            if (interval == DateInterval.Weekday)
            {
                return Round(ts.TotalDays / 7.0);
            }

            if (interval == DateInterval.WeekOfYear)
            {
                while (dt2.DayOfWeek != eFirstDayOfWeek)
                    dt2 = dt2.AddDays(-1);
                while (dt1.DayOfWeek != eFirstDayOfWeek)
                    dt1 = dt1.AddDays(-1);
                ts = dt2 - dt1;
                return Round(ts.TotalDays / 7.0);
            }

            if (interval == DateInterval.Quarter)
            {
                double d1Quarter = GetQuarter(dt1.Month);
                double d2Quarter = GetQuarter(dt2.Month);
                double d1 = d2Quarter - d1Quarter;
                double d2 = (4 * (dt2.Year - dt1.Year));
                return Round(d1 + d2);
            }

            return 0;
        }

        public static bool ValidaData(string DataString)
        {
            bool Retorno = true;
            try
            {
                DateTime testar = Convert.ToDateTime(DataString);
            }
            catch (FormatException)
            {
                Retorno = false;
            }
            return Retorno;
        }

        private static int GetQuarter(int nMonth)
        {
            if (nMonth <= 3)
                return 1;
            if (nMonth <= 6)
                return 2;
            if (nMonth <= 9)
                return 3;
            return 4;
        }
        
        private static long Round(double dVal)
        {
            if (dVal >= 0)
                return (long)Math.Floor(dVal);
            return (long)Math.Ceiling(dVal);
        }
    }
}