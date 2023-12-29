using System;
using System.Globalization;
using AutoMapper;

namespace VulcanTest.Vulcan.Timetable
{
    public class TimeZoneAwareTimeConverter : IValueConverter<string, DateTime>
    {
        private static readonly TimeZoneInfo Tz = TimeZoneInfo.FindSystemTimeZoneById("Europe/Warsaw");

        public DateTime Convert(string sourceMember, ResolutionContext context) =>
           TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(TimeZoneInfo.ConvertTimeToUtc(
                DateTime.ParseExact(sourceMember, "HH:mm", CultureInfo.InvariantCulture), Tz), DateTimeKind.Utc), TimeZoneInfo.Local);

        public static readonly TimeZoneAwareTimeConverter Instance = new();
    }
}