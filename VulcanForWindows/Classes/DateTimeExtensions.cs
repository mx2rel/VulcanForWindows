using System;

namespace VulcanTest.Vulcan
{
    public static class DateTimeExtensions
    {
        public static DateTime StartOfTheMonth(this DateTime dt)
        {
            return dt.Date.AddDays(-(dt.Day - 1));

        }
    }
}
