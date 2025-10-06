using System;
using System.Globalization;

namespace WebApplication1.Extensions
{
    public static class DateTimeExtensions
    {
        public static int GetWeekOfYear(this DateTime date)
        {
            var culture = CultureInfo.CurrentCulture;
            return culture.Calendar.GetWeekOfYear(date,
                CalendarWeekRule.FirstFourDayWeek,
                DayOfWeek.Monday);
        }
    }
}