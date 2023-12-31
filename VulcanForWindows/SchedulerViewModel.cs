using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Vulcanova.Features.Timetable;
using VulcanTest.Vulcan.Timetable;

namespace VulcanForWindows
{
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