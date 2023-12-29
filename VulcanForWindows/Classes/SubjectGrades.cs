using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VulcanForWindows.Vulcan.Grades;
using Vulcanova.Features.Grades;
using Vulcanova.Features.Shared;

namespace VulcanForWindows.Classes
{
    public class SubjectGrades
    {
        public SubjectGrades() { }
        public SubjectGrades(Subject subject, GradesResponseEnvelope env)
        {
            this.subject = subject;
            this.env = env;
        }

        public Subject subject { get; set; }
        public GradesResponseEnvelope env;
        public Grade[] grades
        {
            get => env.Grades.Where(r => r.Column.Subject.Id == subject.Id).ToArray();
        }
        public Grade[] recentGrades => grades.OrderByDescending(r => r.DateCreated).ToList().Take(10).ToArray();
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

        public static SubjectGrades[] GetSubjectsGrades(GradesResponseEnvelope env)
        {
            var r = env.Grades.Select(r => r.Column.Subject).GroupBy(r => r.Name).Select(r => r.First()).Select(r => new SubjectGrades(r, env)).ToArray();

            return r;
        }
    }
}
