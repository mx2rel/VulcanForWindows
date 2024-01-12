using Microsoft.UI.Xaml;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VulcanForWindows.Vulcan.Grades;
using VulcanForWindows.Vulcan.Grades.Final;
using Vulcanova.Features.Grades;
using Vulcanova.Features.Grades.Final;
using Vulcanova.Features.Shared;

namespace VulcanForWindows.Classes
{
    public class SubjectGrades : INotifyPropertyChanged
    {
        public SubjectGrades() { }
        public SubjectGrades(Subject subject, GradesResponseEnvelope env, string fGrade = "")
        {
            this.subject = subject;
            this.env = env;
            grades = new ObservableCollection<Grade>(env.Grades.Where(r => r.Column.Subject.Id == subject.Id));
            finalGrade = fGrade;
        }
        public SubjectGrades(Subject subject, Grade[] g, string fGrade = "", int trim = 0)
        {
            this.subject = subject;
            grades = new ObservableCollection<Grade>(g.Where(r => r.Column.Subject.Id == subject.Id));
            finalGrade = fGrade;

            if (trim > 0) grades = new ObservableCollection<Grade>(grades.ToArray().Take(trim).ToArray());
        }

        public FinalGradesResponseEnvelope finalEnvelope;

        public Subject subject { get; set; }
        public GradesResponseEnvelope env;
        public ObservableCollection<Grade> grades
        {
            get; set;
        }

        public List<Grade> addedGrades = new List<Grade>();

        public void AddGrade(Grade grade)
        {
            grades.Add(grade);
            addedGrades.Add(grade);
            OnPropertyChanged("averageDisplay");
            OnPropertyChanged("AverageColor");
            OnPropertyChanged("AverageText");
            OnPropertyChanged("removeButtonVisibility");

            //OnPropertyChanged(nameof(grades));

        }

        public Visibility removeButtonVisibility => (addedGrades.Count == 0) ? Visibility.Collapsed : Visibility.Visible;


        public void removeAddedGrades()
        {
            foreach (var v in addedGrades)
                grades.Remove(v);

            addedGrades.Clear();

            OnPropertyChanged("averageDisplay");
            OnPropertyChanged("AverageColor");
            OnPropertyChanged("AverageText");
            OnPropertyChanged("removeButtonVisibility");
        }

        public Grade[] recentGrades => grades.OrderByDescending(r => r.DateCreated).ToList().Take(10).ToArray();
        public string finalGrade { get; set; }
        public bool hasFinalGrade { get => !string.IsNullOrEmpty(finalGrade); }
        public Visibility desiredVisibility => hasFinalGrade ? Visibility.Visible : Visibility.Collapsed;
        [JsonIgnore]
        public GradesCountChartData gradesCountChart => GradesCountChartData.Generate(grades.ToArray());

        public double average
        {
            get
            {
                return grades.ToArray().CalculateAverage();
            }
        }

        public string averageDisplay => average.ToString("0.00");

        public static SubjectGrades[] GetSubjectsGrades(GradesResponseEnvelope env)
        {
            var r = env.Grades.Select(r => r.Column.Subject).GroupBy(r => r.Name).Select(r => r.First()).Select(r => new SubjectGrades(r, env)).ToArray();

            return r;
        }
        public static SubjectGrades[] GetSubjectsGrades(Grade[] g, int trim = 0)
        {
            var r = g.Select(r => r.Column.Subject).GroupBy(r => r.Name).Select(r => r.First()).Select(r => new SubjectGrades(r, g, "", trim)).ToArray();

            return r;
        }
        public static SubjectGrades[] GetSubjectsGrades(GradesResponseEnvelope env, FinalGradesResponseEnvelope fenv)
        {
            var r = env.Grades.Select(r => r.Column.Subject).GroupBy(r => r.Name).Select(r => r.First()).Select(r => new SubjectGrades(r, env,
                ((fenv.Grades.Where(g => g.Subject.Id == r.Id).ToArray().Length > 0) ? 
                (fenv.Grades.Where(g => g.Subject.Id == r.Id).ToArray()[0].FastDisplayGrade) : "")            
            )).ToArray();

            return r;
        }

        public static SubjectGrades[] CreateRecent(GradesResponseEnvelope e)
        {
            var gradesDates = e.Grades.GroupBy(r=>r.Column.Subject.Name).Select(r => r.OrderByDescending(d=>d.DateCreated).Take(5)).OrderByDescending(r => r.FirstOrDefault().DateCreated).SelectMany(r=>r).ToList();

            var limit = (gradesDates[(gradesDates.Count > 25) ? 25 : (gradesDates.Count - 1)]).DateCreated;

            var grades = e.Grades.Where(r => r.DateCreated.GetValueOrDefault() >= limit).ToArray();


            return GetSubjectsGrades(grades, 5).OrderByDescending(r => r.grades.Last().DateCreated).Take(5).ToArray();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public string AverageColor => (addedGrades.Count == 0) ? "LightGray" : "#ffc400";
        public string AverageText => (addedGrades.Count == 0) ? "Średnia:" : "Hip. Średnia:";

    }
}
