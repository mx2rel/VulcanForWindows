using System;
using Vulcanova.Features.Shared;

namespace VulcanTest.Vulcan.Timetable
{

    public class TimetableEntry
    {
        public int Id { get; set; }
        public TimetableTimeSlot TimeSlot { get; set; }
        public int PupilId { get; set; }
        public int AccountId { get; set; }
        public string RoomName { get; set; }
        public string TeacherName { get; set; }
        public DateTime Date { get; set; }
        public Subject Subject { get; set; }
        public string Event { get; set; }
        public bool Visible { get; set; }
        public int PeriodId { get; set; }

        public DateTime start => new DateTime(Date.Year,Date.Month,Date.Day,TimeSlot.Start.Hour, TimeSlot.Start.Minute,0);
        public DateTime end => new DateTime(Date.Year,Date.Month,Date.Day,TimeSlot.End.Hour, TimeSlot.End.Minute,0);

        public string displayTime => $"{start.Hour.ToString("00")}:{start.Minute.ToString("00")}-{end.Hour.ToString("00")}:{end.Minute.ToString("00")}";
        public string subName => Subject.Name;
    }

}