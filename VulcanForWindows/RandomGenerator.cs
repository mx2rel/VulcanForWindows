using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vulcanova.Features.Grades;

namespace VulcanForWindows
{
    public static class RandomGenerator
    {
        private static readonly Random _random = new Random();
        private static List<Column> GenerateRandomColumns(int count)
        {
            var columns = new List<Column>();

            for (int i = 0; i < count; i++)
            {
                var column = new Column
                {
                    Id = i + 1,
                    Key = Guid.NewGuid(),
                    PeriodId = _random.Next(1, 10), // Modify the range as needed
                    Name = $"Column_{i + 1}",
                    Code = $"Code_{i + 1}",
                    Group = $"Group_{i + 1}",
                    Number = i + 1,
                    Color = (uint)_random.Next(),
                    Weight = _random.Next(1, 5), // Modify the range as needed
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
                var g = Math.Round((decimal)(_random.NextDouble() * 5 + 1));
                var grade = new Grade
                {
                    Id = i + 1,
                    AccountId = _random.Next(1, 100), // Modify the range as needed
                    CreatorName = $"Creator_{i + 1}",
                    PupilId = _random.Next(1, 100), // Modify the range as needed
                    Content = $"Content_{i + 1}",
                    Comment = $"Comment_{i + 1}",
                    DateCreated = DateTime.Now.AddDays(-_random.Next(1, 100)), // Random date in the past
                    DateModify = DateTime.Now,
                    Value =g, // Modify the range as needed
                    Column = columns[_random.Next(columns.Count)], // Assign a random column
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
    }
}
