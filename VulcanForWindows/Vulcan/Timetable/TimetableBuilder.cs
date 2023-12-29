using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Vulcanova.Features.Timetable;
using Vulcanova.Features.Timetable.Changes;
using Vulcanova.Uonet.Api.Schedule;
using VulcanTest.Vulcan.Timetable.Changes;

namespace VulcanTest.Vulcan.Timetable;

public static class TimetableBuilder
{
    public static IReadOnlyDictionary<DateTime, IReadOnlyCollection<TimetableListEntry>> BuildTimetable(
        ICollection<TimetableEntry> lessons, ICollection<TimetableChangeEntry> changes)
    {
        var timetable = lessons.Where(l => l.Visible)
            .Select(lesson => new TimetableListEntry
            {
                OriginalId = lesson.Id,
                Date = lesson.Date.Date,
                No = lesson.TimeSlot.Position,
                Start = lesson.TimeSlot.Start,
                End = lesson.TimeSlot.End,
                SubjectName = lesson.Subject?.Name,
                RoomName = lesson.RoomName,
                TeacherName = lesson.TeacherName,
                Event = lesson.Event
            })
            .ToList();

        foreach (var change in changes)
        {
            var lessonToUpdate = timetable.SingleOrDefault(l => l.OriginalId == change.TimetableEntryId);

            if (lessonToUpdate != null)
            {
                lessonToUpdate.SubjectName.Override = change.Subject?.Name;
                lessonToUpdate.RoomName.Override = change.RoomName;
                lessonToUpdate.TeacherName.Override = change.TeacherName;
                lessonToUpdate.Event.Override = change.Event;

                lessonToUpdate.Change = new TimetableListEntry.ChangeDetails
                {
                    ChangeNote = change.Note ?? change.Reason,
                    ChangeType = change.Change.Type,
                };

                if (change.Change.Type is ChangeType.Rescheduled)
                {
                    lessonToUpdate.Change.RescheduleKind = TimetableListEntry.RescheduleKind.Removed;
                }
            }


            if (change.Change.Type is ChangeType.Rescheduled
                // for now a hack to not display "ghost" lessons
                && (change.Subject != null || lessonToUpdate != null))
            {
                timetable.Add(new TimetableListEntry
                {
                    No = new TimetableListEntry.OverridableValue<int>
                    {
                        Override = change.TimeSlot?.Position,
                        OriginalValue = lessonToUpdate?.No ?? 0
                    },
                    Start = new TimetableListEntry.OverridableValue<DateTime>
                    {
                        Override = change.TimeSlot?.Start,
                        OriginalValue = lessonToUpdate?.Start ?? DateTime.MaxValue
                    },
                    End = new TimetableListEntry.OverridableValue<DateTime>
                    {
                        Override = change.TimeSlot?.End,
                        OriginalValue = lessonToUpdate?.End ?? DateTime.MaxValue
                    },
                    Date = new TimetableListEntry.OverridableValue<DateTime>
                    {
                        Override = change.ChangeDate?.Date,
                        OriginalValue = change.LessonDate.Date
                    },
                    SubjectName = new TimetableListEntry.OverridableRefValue<string>
                    {
                        Override = change.Subject?.Name,
                        OriginalValue = lessonToUpdate?.SubjectName
                    },
                    RoomName = new TimetableListEntry.OverridableRefValue<string>
                    {
                        Override = change.RoomName,
                        OriginalValue = lessonToUpdate?.RoomName
                    },
                    TeacherName = new TimetableListEntry.OverridableRefValue<string>
                    {
                        Override = change.TeacherName,
                        OriginalValue = lessonToUpdate?.TeacherName,
                    },
                    Change = new TimetableListEntry.ChangeDetails
                    {
                        ChangeNote = change.Note ?? change.Reason,
                        ChangeType = change.Change.Type,
                        RescheduleKind = TimetableListEntry.RescheduleKind.Added
                    },
                    Event = new TimetableListEntry.OverridableRefValue<string>
                    {
                        Override = change.Event,
                        OriginalValue = lessonToUpdate?.Event
                    }
                });
            }
        }

        var result = timetable
            .GroupBy(l => l.Date.Value)
            .ToDictionary(l => l.Key,
                l => (IReadOnlyCollection<TimetableListEntry>)l.OrderBy(x => x.No)
                    .ThenByDescending(x => x.Change != null).ToList().AsReadOnly());

        return new ReadOnlyDictionary<DateTime, IReadOnlyCollection<TimetableListEntry>>(result);
    }
}