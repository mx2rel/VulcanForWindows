using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vulcanova.Features.Timetable;
using static Vulcanova.Features.Timetable.TimetableListEntry;

namespace VulcanForWindows.Classes
{
    public static class RandomGenerator
    {
        private static readonly Random random = new Random();

        #region grades
        private static List<Column> GenerateRandomColumns(int count)
        {
            var columns = new List<Column>();

            for (int i = 0; i < count; i++)
            {
                var column = new Column
                {
                    Id = i + 1,
                    Key = Guid.NewGuid(),
                    PeriodId = random.Next(1, 10), // Modify the range as needed
                    Name = $"Column_{i + 1}",
                    Code = $"Code_{i + 1}",
                    Group = $"Group_{i + 1}",
                    Number = i + 1,
                    Color = (uint)random.Next(),
                    Weight = random.Next(1, 5), // Modify the range as needed
                    subjectName = $"Subject_{i + 1}"
                };

                columns.Add(column);
            }

            return columns;
        }

        private static List<Grade> GenerateRandomGrades(int count, List<Column> columns)
        {
            var grades = new List<Grade>();

            for (int i = 0; i < count; i++)
            {
                var g = Math.Round((decimal)(random.NextDouble() * 5 + 1));
                var grade = new Grade
                {
                    Id = i + 1,
                    AccountId = random.Next(1, 100), // Modify the range as needed
                    CreatorName = $"Creator_{i + 1}",
                    PupilId = random.Next(1, 100), // Modify the range as needed
                    Content = $"Content_{i + 1}",
                    Comment = $"Comment_{i + 1}",
                    DateCreated = DateTime.Now.AddDays(-random.Next(1, 420)), // Random date in the past
                    DateModify = DateTime.Now,
                    Value = g, // Modify the range as needed
                    Column = columns[random.Next(columns.Count)], // Assign a random column
                    ContentRaw = $"{g}"

                };

                grades.Add(grade);
            }

            return grades;
        }

        public static List<Grade> GenerateRandomDataset()
        {
            return GenerateRandomGrades(30, GenerateRandomColumns(5));
        }
        #endregion

        #region timetable
        public static TimetableListEntry[] GenerateRandomTimetable()
        {
            List<TimetableListEntry> l = new List<TimetableListEntry>();
            for (int i = 0; i < 40; i++)
            {
                TimetableListEntry entry = new TimetableListEntry
                {
                    // Generate random values for properties
                    OriginalId = random.Next(1, 1000),
                    No = random.Next(1, 10),
                    SubjectName = GetRandomSubjectName(),
                    TeacherName = GetRandomTeacherName(),
                    Event = GetRandomEvent(),
                    Date = GetRandomDate(),
                    Start = GetRandomTime(),
                    End = GetRandomTime(),
                    RoomName = GetRandomRoomName(),
                    Change = GetRandomChangeDetails()
                };
                l.Add(entry);
            }

            return l.ToArray();
        }

        private static OverridableRefValue<string> GetRandomSubjectName()
        {
            // Replace with your own logic to get random subject names
            string[] subjectNames = { "Math", "Science", "History", "English", "Physics" };
            return subjectNames[random.Next(subjectNames.Length)];
        }

        private static OverridableRefValue<string> GetRandomTeacherName()
        {
            // Replace with your own logic to get random teacher names
            string[] teacherNames = { "Mr. Smith", "Mrs. Johnson", "Dr. Brown", "Ms. Davis", "Professor Lee" };
            return teacherNames[random.Next(teacherNames.Length)];
        }

        private static OverridableRefValue<string> GetRandomEvent()
        {
            // Replace with your own logic to get random events
            string[] events = { "Lecture", "Workshop", "Seminar", "Lab Session", "Discussion" };
            return events[random.Next(events.Length)];
        }

        private static OverridableValue<DateTime> GetRandomDate()
        {
            // Replace with your own logic to get random dates within a reasonable range
            DateTime startDate = DateTime.Today;
            DateTime endDate = startDate.AddDays(7); // One week range for example
            return new DateTime(random.Next(startDate.Year, endDate.Year + 1),
                                random.Next(1, 13),
                                random.Next(1, 29)); // Assuming maximum of 28 days for simplicity
        }

        private static OverridableValue<DateTime> GetRandomTime()
        {
            // Replace with your own logic to get random times
            return DateTime.Today.AddHours(random.Next(-192, 192)); // Assuming classes between 8 AM to 6 PM
        }

        private static OverridableRefValue<string> GetRandomRoomName()
        {
            // Replace with your own logic to get random room names
            string[] roomNames = { "Room A", "Room B", "Room C", "Auditorium", "Lab 1" };
            return roomNames[random.Next(roomNames.Length)];
        }

        private static TimetableListEntry.ChangeDetails GetRandomChangeDetails()
        {
            // Replace with your own logic to get random change details
            string[] changeTypes = { "Type A", "Type B", "Type C" };
            string[] changeNotes = { "Note 1", "Note 2", "Note 3" };

            return new TimetableListEntry.ChangeDetails
            {
                ChangeType = changeTypes[random.Next(changeTypes.Length)],
                RescheduleKind = (TimetableListEntry.RescheduleKind)random.Next(Enum.GetNames(typeof(TimetableListEntry.RescheduleKind)).Length),
                ChangeNote = changeNotes[random.Next(changeNotes.Length)]
            };
        }
        #endregion
    }
}
