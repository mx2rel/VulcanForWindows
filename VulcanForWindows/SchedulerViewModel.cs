using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Vulcanova.Features.Timetable;

namespace VulcanForWindows
{
    public class TimetableEntry
    {
        public long Id { get; set; }
        public bool AllDay { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public int LabelId { get; set; }
        public string Subject { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public string RecurrenceInfo { get; set; }

        public TimetableEntry(TimetableListEntry e)
        {
            // Initialize properties using values from TimetableListEntry 'e'
            this.AllDay = false; // Assuming default value for AllDay

            // Using overridden or original values from TimetableListEntry
            this.Start = e.Start;
            this.End = e.End;
            this.Subject = e.SubjectName; // Assuming implicit conversion works for SubjectName

            // Assigning default values or handling overrides where applicable
            this.LabelId = 0; // Assign an appropriate LabelId
            this.Description = string.Empty; // Or assign a meaningful description
            this.Location = e.RoomName + " | " + e.TeacherName; // Assuming implicit conversion works for RoomName
            this.RecurrenceInfo = string.Empty; // Or provide recurrence information if available
        }

        public static TimetableEntry[] Generate(TimetableListEntry[] e)
        {
            //appointments = new ObservableCollection<TimetableListEntry>(e);
            return (e.Select(r => new TimetableEntry(r)).ToArray());
        }
    }
    public class SchedulerViewModel
    {
        public virtual DateTime Start { get; set; }

        ObservableCollection<TimetableEntry> appointments = new ObservableCollection<TimetableEntry>();
        public IEnumerable<TimetableEntry> Appointments { get { return appointments; } }



        public SchedulerViewModel()
        {
            Init();
        }

        void Init()
        {
            Start = DateTime.Today;
        }
    }
}