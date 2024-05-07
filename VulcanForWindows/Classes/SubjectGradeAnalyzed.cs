using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VulcanForWindows.Vulcan.Grades;
using Vulcanova.Features.Grades;
using Vulcanova.Features.Shared;
using VulcanTest.Vulcan;

namespace VulcanForWindows.Classes
{
    public class SubjectGradesAnalyzed : SubjectGrades
    {

        public SubjectGradesAnalyzed(Subject subject, GradesResponseEnvelope env, int periodId, string fGrade = "", GradesResponseEnvelope prevPeriod = null)
        {
            this.subject = subject;
            this.env = env;
            grades = new ObservableCollection<Grade>(env.Grades.Where(r => r.Column.Subject.Id == subject.Id));
            this.periodId = periodId;
            finalGrade = fGrade;
            if (prevPeriod != null)
                prevPeriodGrades = prevPeriod.Grades.ToArray();
            FetchYearlyAverage();
            foreach (var v in grades) v.CalculateClassAverage();


        }
        public SubjectGradesAnalyzed(Subject subject, Grade[] g, int periodId, string fGrade = "", int trim = 0, bool FetchAverages = true)
        {
            this.subject = subject;
            grades = new ObservableCollection<Grade>(g.Where(r => r.Column.Subject.Id == subject.Id));
            finalGrade = fGrade;
            this.periodId = periodId;
            if (FetchAverages)
                FetchYearlyAverage();
            if (trim > 0) grades = new ObservableCollection<Grade>(grades.ToArray().Take(trim).ToArray());
            foreach (var v in grades) v.CalculateClassAverage();

        }

        async Task<bool> FetchYearlyAverage()
        {
            var avg = await GetYearlyAverage();
            var fG = await GetFinalGrade();

            if (fG == null)
            {
                if (avg.count > 0)
                    finalGradePredicted = Math.Round(avg.average).ToString();
                OnPropertyChanged(nameof(displayGrade));
                OnPropertyChanged(nameof(defaultGrade));
                OnPropertyChanged(nameof(allowEdits));
            }
            return true;

        }

        public string displayGrade => (finalGrade == null) ? ((finalGradeOverride == null) ? finalGradePredicted : finalGradeOverride) : finalGrade;
        public string defaultGrade => (finalGrade == null) ? finalGradePredicted : finalGrade;
        public string finalGradeOverride
        {
            get => _finalGradeOverride;
            set
            {
                _finalGradeOverride = value;
                OnPropertyChanged(nameof(displayGrade));
                OnPropertyChanged(nameof(defaultGrade));
            }
        }
        string _finalGradeOverride = null;

        string finalGradePredicted;

        public bool allowEdits => finalGrade == null && includeInCalculations;
        public bool includeInCalculations
        {
            get
            {
                if (_includeInCalculations == null)
                    _includeInCalculations = Preferences.Get<bool>($"Analyzer_{subject.Id}_Include", true);
                return _includeInCalculations.Value;
            }
            set
            {
                _includeInCalculations = value;
                OnPropertyChanged(nameof(allowEdits));
                OnPropertyChanged(nameof(includeInCalculations));
            }
        }
        bool? _includeInCalculations;

        public async static Task<SubjectGradesAnalyzed[]> GetSubjectGradesAnalyzed(Grade[] g, int periodId, int trim = 0)
        {
            var r = g.GroupBy(r => r.Column.Subject.Id).Select(r => new SubjectGradesAnalyzed(r.First().Column.Subject, r.ToArray(), periodId, "", trim, false)).ToArray();
            foreach (var v in r) await v.FetchYearlyAverage();
            return r;
        }
    }
}
