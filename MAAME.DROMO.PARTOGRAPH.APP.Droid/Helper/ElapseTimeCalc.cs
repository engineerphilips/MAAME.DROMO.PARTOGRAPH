using EMPEROR.PERIOD;
using EMPEROR.PLURALIZE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Helper
{
    public class ElapseTimeCalc
    {
        public static string PeriodElapseTime(DateTime date)
        {
            var endDate = DateTime.Now;
            var startDate = date;
            var period = new Period(startDate, endDate);

            if (period.Years > 0)
            {
                return period.Months > 0
                    ? $"{period.Years} {new Pluralizer().Format("YR", period.Years)}, {period.Months} {new Pluralizer().Format("MON", period.Months)} AGO"
                    : $"{period.Years} {new Pluralizer().Format("YR", period.Years)} AGO";
            }
            else if (period.Months > 0)
            {
                return period.Days > 0
                    ? $"{period.Months} {new Pluralizer().Format("MON", period.Months)}, {period.Days} {new Pluralizer().Format("D", period.Days)} AGO"
                    : $"{period.Months} {new Pluralizer().Format("MON", period.Months)} AGO";
            }
            else if (period.Days > 0)
            {
                return period.Hours > 0
                    ? $"{period.Days} {new Pluralizer().Format("D", period.Days)}, {period.Hours} {new Pluralizer().Format("HR", period.Hours)} AGO"
                    : $"{period.Days} {new Pluralizer().Format("D", period.Days)} AGO";
            }
            else if (period.Hours > 0)
            {
                return period.Minutes > 0
                    ? $"{period.Hours} {new Pluralizer().Format("HR", period.Hours)}, {period.Minutes} {new Pluralizer().Format("MIN", period.Minutes)} AGO"
                    : $"{period.Hours} {new Pluralizer().Format("HR", period.Hours)} AGO";
            }
            else if (period.Minutes > 0)
            {
                return period.Seconds > 0
                    ? $"{period.Minutes} {new Pluralizer().Format("MIN", period.Minutes)}, {period.Seconds} {new Pluralizer().Format("SEC", period.Seconds)} AGO"
                    : $"{period.Minutes} {new Pluralizer().Format("MIN", period.Minutes)} AGO";
            }
            else
                return "MOMENT AGO";
        }

        public static string PeriodElapseTime(DateTime startdate, DateTime enddate)
        {
            var endDate = enddate;
            var startDate = startdate;
            var period = new Period(startDate, endDate);

            if (period.Years > 0)
            {
                return period.Months > 0
                    ? $"{period.Years} {new Pluralizer().Format("YR", period.Years)}, {period.Months} {new Pluralizer().Format("MON", period.Months)} AGO"
                    : $"{period.Years} {new Pluralizer().Format("YR", period.Years)} AGO";
            }
            else if (period.Months > 0)
            {
                return period.Days > 0
                    ? $"{period.Months} {new Pluralizer().Format("MON", period.Months)}, {period.Days} {new Pluralizer().Format("D", period.Days)} AGO"
                    : $"{period.Months} {new Pluralizer().Format("MON", period.Months)} AGO";
            }
            else if (period.Days > 0)
            {
                return $"{period.Days} {new Pluralizer().Format("D", period.Days)} AGO";
            }
            else if (period.Hours > 0)
            {
                return $"{period.Hours} {new Pluralizer().Format("HR", period.Hours)} AGO";
            }
            else if (period.Minutes > 0)
            {
                return $"{period.Minutes} {new Pluralizer().Format("MIN", period.Minutes)} AGO";
            }
            else
                return "MOMENT AGO";
        }

        public static string PeriodElapseTimeLower(DateTime startdate, DateTime enddate)
        {
            var endDate = enddate;
            var startDate = startdate;
            var period = new Period(startDate, endDate);

            if (period.Years > 0)
            {
                return period.Months > 0
                    ? $"{period.Years} {new Pluralizer().Format("year", period.Years)}, {period.Months} {new Pluralizer().Format("month", period.Months)} ago"
                    : $"{period.Years} {new Pluralizer().Format("year", period.Years)} ago";
            }
            else if (period.Months > 0)
            {
                return period.Days > 0
                    ? $"{period.Months} {new Pluralizer().Format("month", period.Months)}, {period.Days} {new Pluralizer().Format("day", period.Days)} ago"
                    : $"{period.Months} {new Pluralizer().Format("month", period.Months)} ago";
            }
            else if (period.Days > 0)
            {
                return period.Hours > 0
                    ? $"{period.Days} {new Pluralizer().Format("day", period.Days)}, {period.Hours} {new Pluralizer().Format("hour", period.Hours)} ago"
                    : $"{period.Days} {new Pluralizer().Format("day", period.Days)} ago";
            }
            else if (period.Hours > 0)
            {
                return period.Minutes > 0
                    ? $"{period.Hours} {new Pluralizer().Format("hour", period.Hours)}, {period.Minutes} {new Pluralizer().Format("minute", period.Minutes)} ago"
                    : $"{period.Hours} {new Pluralizer().Format("hour", period.Hours)} ago";
            }
            else if (period.Minutes > 0)
            {
                return $"{period.Minutes} {new Pluralizer().Format("minute", period.Minutes)} ago";
            }
            else
                return "now";
        }

        public static string PeriodElapseTimeLowerTrim(DateTime startdate, DateTime enddate)
        {
            var endDate = enddate;
            var startDate = startdate;
            var period = new Period(startDate, endDate);

            if (period.Years > 0)
            {
                return period.Months > 0
                    ? $"{period.Years} {new Pluralizer().Format("year", period.Years)}, {period.Months} {new Pluralizer().Format("month", period.Months)}"
                    : $"{period.Years} {new Pluralizer().Format("year", period.Years)}";
            }
            else if (period.Months > 0)
            {
                return period.Days > 0
                    ? $"{period.Months} {new Pluralizer().Format("month", period.Months)}, {period.Days} {new Pluralizer().Format("day", period.Days)}"
                    : $"{period.Months} {new Pluralizer().Format("month", period.Months)}";
            }
            else if (period.Days > 0)
            {
                return period.Hours > 0
                    ? $"{period.Days} {new Pluralizer().Format("day", period.Days)}, {period.Hours} {new Pluralizer().Format("hour", period.Hours)}"
                    : $"{period.Days} {new Pluralizer().Format("day", period.Days)}";
            }
            else if (period.Hours > 0)
            {
                return period.Minutes > 0
                    ? $"{period.Hours} {new Pluralizer().Format("hour", period.Hours)}, {period.Minutes} {new Pluralizer().Format("minute", period.Minutes)}"
                    : $"{period.Hours} {new Pluralizer().Format("hour", period.Hours)}";
            }
            else if (period.Minutes > 0)
            {
                return $"{period.Minutes} {new Pluralizer().Format("minute", period.Minutes)}";
            }
            else
                return "now";
        }

        public static string PeriodLeft(DateTime startdate, DateTime enddate)
        {
            var endDate = enddate;
            var startDate = startdate;
            var period = new Period(startDate, endDate);

            if (period.Years > 0)
            {
                return period.Months > 0
                    ? $"{period.Years} {new Pluralizer().Format("YR", period.Years)}, {period.Months} {new Pluralizer().Format("MON", period.Months)} LEFT"
                    : $"{period.Years} {new Pluralizer().Format("YR", period.Years)} LEFT";
            }
            else if (period.Months > 0)
            {
                return period.Days > 0
                    ? $"{period.Months} {new Pluralizer().Format("MON", period.Months)}, {period.Days} {new Pluralizer().Format("D", period.Days)} LEFT"
                    : $"{period.Months} {new Pluralizer().Format("MON", period.Months)} LEFT";
            }
            else if (period.Days > 0)
            {
                return $"{period.Days} {new Pluralizer().Format("D", period.Days)} LEFT";
            }
            else if (period.Hours > 0)
            {
                return $"{period.Hours} {new Pluralizer().Format("HR", period.Hours)} LEFT";
            }
            else if (period.Minutes > 0)
            {
                return $"{period.Minutes} {new Pluralizer().Format("MIN", period.Minutes)} LEFT";
            }
            else
                return "MOMENT LEFT";
        }
    }
}
