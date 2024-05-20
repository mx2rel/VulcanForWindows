using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VulcanForWindows.Preferences;
using VulcanForWindows.Vulcan.Grades;
using Vulcanova.Features.Grades;
using Vulcanova.Features.Shared;

namespace VulcanForWindows.Classes
{
    public class SubjectGradesAnalyzed : SubjectGrades
    {
        public SubjectGradesAnalyzed(Subject subject, Grade[] g, bool FetchAverages = true)
        {
            this.subject = subject;
            grades = new ObservableCollection<Grade>(g.Where(r => r.Column.Subject.Id == subject.Id));

            if (FetchAverages)
                FetchYearlyAverage();

            foreach (var v in grades) v.CalculateClassAverage();

        }

        public async Task<bool> FetchYearlyAverage()
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
                    _includeInCalculations = PreferencesManager.Get<bool>($"Analyzer_{subject.Id}_Include", true);
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
    }
}
