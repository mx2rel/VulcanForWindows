using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VulcanForWindows.Classes
{
    public class SubjectGrades
    {
        public SubjectGrades(string n, Grade[] g)
        {
            subName = n;
            grades = g;
        }
        public SubjectGrades(IGrouping<string, Grade[]> v)
        {
            subName = v.Key;
            grades = v.SelectMany(group => group).ToArray();
        }
        public string subName { get; set; }
        public Grade[] grades { get; set; }
        public Grade[] recentGrades => grades.OrderByDescending(r=>r.DateCreated).ToList().Take(10).ToArray();
        public string finalGrade { get; set; }
        public bool hasFinalGrade { get => !string.IsNullOrEmpty(finalGrade); }
        public Visibility desiredVisibility => hasFinalGrade ? Visibility.Visible : Visibility.Collapsed;
        public GradesCountChartData gradesCountChart => GradesCountChartData.Generate(grades);

        public double average
        {
            get
            {
                return grades.CountAverage();
            }
        }

        public string averageDisplay => average.ToString("0.00");

        public static SubjectGrades[] GetSubjectsGrades()
        {
            var r = RandomGenerator.GenerateRandomDataset()
                .GroupBy(r => r.Column.subjectName)
                .Select(g => new SubjectGrades(g.Key, g.ToArray()))
                .ToArray();

            return r;
        }
    }
}
