using System;
using Vulcanova.Features.Shared;

namespace VulcanTest.Vulcan.Timetable;

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
}