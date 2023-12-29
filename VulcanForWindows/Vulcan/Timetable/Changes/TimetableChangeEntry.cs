using System;
using Vulcanova.Uonet.Api.Schedule;

namespace VulcanTest.Vulcan.Timetable.Changes;

public class TimetableChangeEntry
{
    public int Id { get; set; }
    public int TimetableEntryId { get; set; }
    public int UnitId { get; set; }
    public int PupilId { get; set; }
    public int AccountId { get; set; }
    public Vulcanova.Features.Shared.Subject Subject { get; set; }
    public DateTime LessonDate { get; set; }
    public DateTime? ChangeDate { get; set; }
    public TimetableTimeSlot TimeSlot { get; set; }
    public string Note { get; set; }
    public string Event { get; set; }
    public string Reason { get; set; }
    public string TeacherName { get; set; }
    public string RoomName { get; set; }
    public Change Change { get; set; }
}