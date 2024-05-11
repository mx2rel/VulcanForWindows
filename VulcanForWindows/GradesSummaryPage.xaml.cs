using DevExpress.WinUI.Core.Internal;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using VulcanForWindows.Classes;
using VulcanForWindows.Vulcan;
using VulcanForWindows.Vulcan.Grades;
using VulcanForWindows.Vulcan.Grades.Final;
using Vulcanova.Features.Auth;
using Vulcanova.Features.Auth.Accounts;
using Vulcanova.Features.Grades;
using Vulcanova.Features.Grades.Final;
using Vulcanova.Features.Shared;
using VulcanTest.Vulcan;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VulcanForWindows
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class GradesSummaryPage : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }


        public GradesSummaryPage()
        {
            PeriodAverage = -1;
            YearAverage = -1;
            FinalAverage = -1;
            periodFinalGrades = new ObservableCollection<PeriodFinalGradeViewModel>();
            this.InitializeComponent();
            selectedPeriod = new AccountRepository().GetActiveAccount().CurrentPeriod;
            PeriodSelector.SelectedIndex = avaiblePeriods.Length - 1;
            LoadAllGrades();

        }


        public Vulcanova.Features.Shared.Period selectedPeriod;
        public int selectedPeriodId => selectedPeriod.Id;
        public Vulcanova.Features.Shared.Period[] avaiblePeriods => new AccountRepository().GetActiveAccount().Periods/*.Select(r => r.Id)*/.ToArray();
        public string[] displayPeriods => avaiblePeriods.Select(r => $"Klasa {r.Level}, Semestr {r.Number}").ToArray();
        public bool Loaded;
        private void ChangedPeriod(object sender, SelectionChangedEventArgs e)
        {
            selectedPeriod = avaiblePeriods[(sender as ComboBox).SelectedIndex] as Vulcanova.Features.Shared.Period;
            UpdateAverages(selectedPeriod);
        }
        IDictionary<Vulcanova.Features.Shared.Period, Grade[]> allGrades;
        IDictionary<Vulcanova.Features.Shared.Period, FinalGradesEntry[]> allFinalGrades;
        private async void LoadAllGrades()
        {
            allGrades = await new GradesService().FetchGradesFromAllPeriodsAsync(new AccountRepository().GetActiveAccount());
            allFinalGrades = await new FinalGrades().FetchGradesFromAllPeriodsAsync(new AccountRepository().GetActiveAccount());

            Loaded = true;
            LoadingBar.Visibility = Visibility.Collapsed;

            UpdateAverages(selectedPeriod);
        }

        //    IDictionary<int, GradesResponseEnvelope> gradesEnvelopes = new Dictionary<int, GradesResponseEnvelope>();
        //    IDictionary<int, FinalGradesResponseEnvelope> finalGradesEnvelopes = new Dictionary<int, FinalGradesResponseEnvelope>();

        //    GradesResponseEnvelope gradesEnvelope
        //    {
        //        get => gradesEnvelopes[selectedPeriodId];
        //        set => gradesEnvelopes[selectedPeriodId] = value;
        //    }
        //    FinalGradesResponseEnvelope finalGradesEnvelope
        //    {
        //        get => finalGradesEnvelopes[selectedPeriodId];
        //        set => finalGradesEnvelopes[selectedPeriodId] = value;
        //    }

        //    //TODO: MAKE GLOBAL
        //    public async Task<G> GetEnvelope<G, T>(
        //int periodId,
        //Account acc,
        //Func<Account, int, bool, bool, Task<G>> getOperation,
        //Action OnChange, IDictionary<int, G> dictionary) where G : IResponseEnvelope<T>
        //    {
        //        if (!dictionary.ContainsKey(periodId))
        //        {
        //            dictionary[periodId] = await getOperation(acc, periodId, false, false);
        //            dictionary[periodId].OnLoadingOrUpdatingFinished += (sender, args) => OnChange(); // OnLoadingOrUpdatingFinished this line
        //        }
        //        else
        //        {
        //            // LoadingBar.Visibility = isLoadingOrUpdating ? Visibility.Visible : Visibility.Collapsed;
        //            // TODO: LOADING BAR
        //            // Debug.Write(JsonConvert.SerializeObject(cd));
        //        }

        //        return dictionary[periodId];
        //    }

        //    async void AssignGrades()
        //    {
        //        var acc = new AccountRepository().GetActiveAccountAsync();
        //        GetEnvelope<GradesResponseEnvelope, Grade>(
        //            selectedPeriodId,
        //            acc,
        //            new GradesService().GetPeriodGrades,
        //            RecalculateAverage,
        //            gradesEnvelopes);
        //        GetEnvelope<FinalGradesResponseEnvelope, FinalGradesEntry>(
        //            selectedPeriodId,
        //            acc,
        //            new FinalGrades().GetPeriodGrades,
        //            ReclaculateFinalAverage,
        //            finalGradesEnvelopes);


        //        int changeBy = GetAnotherPeriodFromYear();

        //        if (changeBy != 0)
        //        {
        //            GetEnvelope<GradesResponseEnvelope, Grade>(
        //            selectedPeriodId + changeBy,
        //            acc,
        //            new GradesService().GetPeriodGrades,
        //            RecalculateAverage,
        //            gradesEnvelopes);
        //            GetEnvelope<FinalGradesResponseEnvelope, FinalGradesEntry>(
        //                selectedPeriodId + changeBy,
        //                acc,
        //                new FinalGrades().GetPeriodGrades,
        //                ReclaculateFinalAverage,
        //                finalGradesEnvelopes);
        //        }
        //        //TopLevel.UpdateLayout();
        //    }

        //    private int GetAnotherPeriodFromYear()
        //    {
        //        // get all periods from year
        //        int changeBy = (selectedPeriodId % 2 == 0) ? 1 : -1;
        //        //check if period exists (is not from the future)
        //        if ((changeBy == 1 && (new AccountRepository().GetActiveAccountAsync()).Periods.Where(r => r.Id == selectedPeriodId + changeBy).ToArray().Length == 0))
        //            changeBy = 0;
        //        return changeBy;
        //    }

        public float PeriodAverage { get; set; }
        public float YearAverage { get; set; }
        public float FinalAverage { get; set; }

        public bool isSecondNumber => (selectedPeriod.Number == 2);

        public ObservableCollection<PeriodFinalGradeViewModel> periodFinalGrades { get; set; }

        private void UpdateAverages(Vulcanova.Features.Shared.Period period)
        {
            if (!Loaded) return;
            //Debug.Write(JsonConvert.SerializeObject(allGrades));
            PeriodAverage = (float)Math.Round(allGrades.Where(r => r.Key.Id == period.Id).ToArray()[0].Value.CalculateAverage(), 2);
            YearAverage = (float)Math.Round(allGrades.Where(r => r.Key.Level == period.Level).SelectMany(r => r.Value).ToArray().CalculateAverage(), 2);
            FinalAverage = (float)Math.Round(allFinalGrades.Where(r => r.Key.Id == period.Id).ToArray()[0].Value.CalculateAverage(), 2);
            RaisePropertyChanged(nameof(PeriodAverage));
            RaisePropertyChanged(nameof(YearAverage));
            RaisePropertyChanged(nameof(FinalAverage));
            sp.UpdateLayout();

            periodFinalGrades.ReplaceAll(new ObservableCollection<PeriodFinalGradeViewModel>(allFinalGrades.Where(r => r.Key.Id == period.Id).ElementAt(0).Value.Select(t =>
            new PeriodFinalGradeViewModel
            {
                fg = t,
                PeriodAverage = (float)allGrades.Where(r => r.Key.Id == period.Id).ToArray()[0].Value.Where(r => r.Column.Subject.Id == t.Subject.Id).ToArray().CalculateAverage(),
                YearAverage = (float)allGrades.Where(r => r.Key.Level == period.Level).SelectMany(r => r.Value).Where(r => r.Column.Subject.Id == t.Subject.Id).ToArray().CalculateAverage(),
                Period = period
            }).ToArray()));
        }
    }

    public class PeriodFinalGradeViewModel
    {
        public FinalGradesEntry fg { get; set; }
        public float PeriodAverage;
        public float YearAverage;
        public Period Period;

        public string PeriodAverageDisplay => PeriodAverage.ToString("0.00");
        public string YearAverageDisplay => YearAverage.ToString("0.00");

        /// <summary>
        /// Displays period average if first semester, and year average if second semester
        /// </summary>
        /// 
        public string ToolTipText
        {
            get
              => (Period.Number == 2) ? "Średnia roczna" : "Średnia okresu";
        }
        public string AverageDisplay
        {
            get
              => (Period.Number == 2) ? YearAverageDisplay : PeriodAverageDisplay;
        }
    }
}
