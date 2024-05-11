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
using Vulcanova.Features.Auth;
using Vulcanova.Features.Grades;
using Vulcanova.Features.Grades.Final;
using Vulcanova.Features.Shared;

namespace VulcanForWindows.Classes
{
    public class SubjectGrades : INotifyPropertyChanged
    {
        public SubjectGrades() { }
        public SubjectGrades(Subject subject, GradesResponseEnvelope env, int periodId, string fGrade = "", GradesResponseEnvelope prevPeriod = null)
        {
            this.subject = subject;
            this.env = env;
            grades = new ObservableCollection<Grade>(env.Grades.Where(r => r.Column.Subject.Id == subject.Id));
            this.periodId = periodId;
            finalGrade = fGrade;
            if (prevPeriod != null)
                prevPeriodGrades = prevPeriod.Grades.ToArray();
            GetYearlyAverage();
            foreach (var v in grades) v.CalculateClassAverage();

            GetFinalGrade();

        }
        public SubjectGrades(Subject subject, Grade[] g, int periodId, string fGrade = "", int trim = 0)
        {
            this.subject = subject;
            grades = new ObservableCollection<Grade>(g.Where(r => r.Column.Subject.Id == subject.Id));
            finalGrade = fGrade;
            this.periodId = periodId;
            GetYearlyAverage();
            if (trim > 0) grades = new ObservableCollection<Grade>(grades.ToArray().Take(trim).ToArray());
            foreach (var v in grades) v.CalculateClassAverage();

            GetFinalGrade();
        }

       public async Task<string> GetFinalGrade()
        {
            var finalGrades =
                await new FinalGrades().GetPeriodGrades(new AccountRepository().GetActiveAccount(),
                periodId, false, true);
            var g = finalGrades.Grades.Where(r => r.Subject.Id == subject.Id);
            if (g.Count() > 0)
            {
                finalGrade = g.First().FinalGrade;
                //if (string.IsNullOrEmpty(finalGrade)) finalGrade = g.First().PredictedGrade;
                OnPropertyChanged(nameof(finalGrade));
                OnPropertyChanged(nameof(hasFinalGrade));
            }
            return finalGrade;
        }

        public int periodId;
        public Subject subject { get; set; }
        public GradesResponseEnvelope env;
        public ObservableCollection<Grade> grades
        {
            get; set;
        }

        public Grade[] prevPeriodGrades = new Grade[0];

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
            GetYearlyAverage(true);
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
            GetYearlyAverage(true);
        }

        public Grade[] recentGrades => grades.OrderBy(r => r.DateCreated).ToList().Take(10).ToArray();
        public string finalGrade { get; set; }
        public bool hasFinalGrade { get => !string.IsNullOrEmpty(finalGrade); }
        [JsonIgnore]
        public GradesCountChartData gradesCountChart => GradesCountChartData.Generate(grades.ToArray());

        IDictionary<Period, Grade[]> _yearGrades;
        public static IDictionary<string, ((double average, int count, int weightSum) data, DateTime generatedAt)> YearlyAverages = new Dictionary<string, ((double average, int count, int weightSum) data, DateTime generatedAt)>();
        public async Task<(double average, int count, int weightSum)> GetYearlyAverage(bool force= false)
        {
            if (YearlyAverages.TryGetValue(GetYearlyAverageId(), out var o) && !force)
            {
                if (DateTime.Now - o.generatedAt < new TimeSpan(0, 15, 0))
                {

                    yearlyAverage = o.data.average;
                    yearGradesCount = $"{o.data.count}";

                    OnPropertyChanged(nameof(yearlyAverage));
                    OnPropertyChanged(nameof(averageDisplay));
                    OnPropertyChanged(nameof(yearGradesCount));
                    return o.data;
                }
            }
            if (_yearGrades == null) _yearGrades =
                (await (new GradesService()).FetchLevelGradesWithPeriodAsync(new AccountRepository().GetActiveAccount(), periodId));

            if (_yearGrades != null)
            {
                var gradesOnly = _yearGrades.SelectMany(r => r.Value).Where(r => r.Column.Subject.Id == subject.Id);
                yearlyGrades = gradesOnly.Concat(addedGrades).ToArray();

                yearlyAverage = yearlyGrades.CalculateAverage();
                yearGradesCount = $"{yearlyGrades.Count()}";

                OnPropertyChanged(nameof(yearlyAverage));
                OnPropertyChanged(nameof(averageDisplay));
                OnPropertyChanged(nameof(yearGradesCount));
                var result = ((yearlyAverage, yearlyGrades.Count(), yearlyGrades.Select(r => r.Column.Weight).Sum()), DateTime.Now);
                YearlyAverages[GetYearlyAverageId()] = result;
                return result.Item1;
            }
            return (0,0,0);
        }
        string GetYearlyAverageId()
        {
            return $"{((periodId % 2 == 0) ? periodId : (periodId - 1))}_{subject.Id}";
        }
        public async Task<(double average, int count, int weightSum)> CalculateYearlyAverage(Grade excludeGrade = null, bool forceSlowMethod = false)
            => await CalculateYearlyAverage(((excludeGrade == null) ? null : new Grade[] { excludeGrade }), forceSlowMethod);
        public async Task<(double average, int count, int weightSum)> CalculateYearlyAverage(Grade[] excludeGrades = null, bool forceSlowMethod = false)
        {
            var YearlyAveragesEntryKey = GetYearlyAverageId();
            if (!forceSlowMethod && (YearlyAverages.TryGetValue(YearlyAveragesEntryKey, out var YearlyAveragesData)))
            {
                var sum = YearlyAveragesData.data.average * (double)YearlyAveragesData.data.weightSum;
                excludeGrades = excludeGrades.Where(r => r.VulcanValue.HasValue && r.Column.Weight != 0).ToArray();
                foreach (var excludedGrade in excludeGrades)
                    sum -= (double)excludedGrade.VulcanValue.Value * (double)excludedGrade.Column.Weight;
                var weightSum = YearlyAveragesData.data.weightSum - excludeGrades.Select(r => r.Column.Weight).Sum();
                return (sum / weightSum, YearlyAveragesData.data.count - excludeGrades.Length, weightSum);
            }
            else
            {
                var excludeIds = excludeGrades.Select(r => r.Id).ToList();
                if (YearlyAverages.TryGetValue(GetYearlyAverageId(), out var o))
                {
                    if (DateTime.Now - o.generatedAt < new TimeSpan(0, 10, 0))
                        return o.data;
                }
                if (_yearGrades == null) _yearGrades =
                    (await (new GradesService()).FetchLevelGradesWithPeriodAsync(new AccountRepository().GetActiveAccount(), periodId));

                var gradesOnly = _yearGrades.SelectMany(r => r.Value).Where(r => r.Column.Subject.Id == subject.Id);
                var yearlyGrades = gradesOnly.Concat(addedGrades).ToArray();

                var yearlyAverage = yearlyGrades.Where(r => ((excludeGrades == null) ? true : (!excludeIds.Contains(r.Id)))).ToArray().CalculateAverage();

                if (excludeGrades == null)
                    return (yearlyAverage, yearlyGrades.Count(), yearlyGrades.Select(r => r.Column.Weight).Sum());

                return (yearlyAverage, yearlyGrades.Count(), yearlyGrades.Select(r => r.Column.Weight).Sum());
            }

        }

        public Grade[] yearlyGrades = new Grade[0];

        public double yearlyAverage { get; set; }
        public string yearGradesCount { get; set; }

        public string averageDisplay => yearlyAverage.ToString("0.00");

        public static SubjectGrades[] GetSubjectsGrades(Grade[] g, int periodId, int trim = 0)
        {
            var r = g.GroupBy(r => r.Column.Subject.Id).Select(r => new SubjectGrades(r.First().Column.Subject, r.ToArray(), periodId, "", trim)).ToArray();

            return r;
        }
        public static SubjectGrades[] GetSubjectsGrades(GradesResponseEnvelope env, FinalGradesResponseEnvelope fenv, int periodId, GradesResponseEnvelope otherPeriod = null)
        {
            var r = env.Grades.Select(r => r.Column.Subject).GroupBy(r => r.Name).Select(r => r.First()).Select(r => new SubjectGrades(r, env, periodId,
                ((fenv.Grades.Where(g => g.Subject.Id == r.Id).ToArray().Length > 0) ?
                (fenv.Grades.Where(g => g.Subject.Id == r.Id).ToArray()[0].FastDisplayGrade) : ""), otherPeriod
            )).ToArray();

            return r;
        }

        public static SubjectGrades[] CreateRecent(GradesResponseEnvelope e)
        {
            var gradesDates = e.Grades.GroupBy(r => r.Column.Subject.Name).Select(r => r.OrderByDescending(d => d.DateCreated).Take(5)).OrderByDescending(r => r.FirstOrDefault().DateCreated).SelectMany(r => r).ToList();

            var limit = (gradesDates[(gradesDates.Count > 25) ? 25 : (gradesDates.Count - 1)]).DateCreated;

            var grades = e.Grades.Where(r => r.DateCreated.GetValueOrDefault() >= limit).ToArray();


            return GetSubjectsGrades(grades, 0, 5).OrderByDescending(r => r.grades.Last().DateCreated).Take(5).ToArray();
        }
        public static SubjectGrades[] CreateRecent(Grade[] e)
        {
            var gradesDates = e.GroupBy(r => r.Column.Subject.Name).Select(r => r.OrderByDescending(d => d.DateCreated).Take(5)).OrderByDescending(r => r.FirstOrDefault().DateCreated).SelectMany(r => r).ToList();

            var limit = (gradesDates[(gradesDates.Count > 25) ? 25 : (gradesDates.Count - 1)]).DateCreated;

            var grades = e.Where(r => r.DateCreated.GetValueOrDefault() >= limit).ToArray();


            return GetSubjectsGrades(grades, 0, 5).OrderByDescending(r => r.grades.Last().DateCreated).Take(5).ToArray();
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
