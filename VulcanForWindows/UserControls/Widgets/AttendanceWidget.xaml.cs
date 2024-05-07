using LiveChartsCore;
using LiveChartsCore.SkiaSharpView.Extensions;
using LiveChartsCore.SkiaSharpView.Painting;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using VulcanForWindows.Vulcan;
using Vulcanova.Features.Attendance;
using Vulcanova.Features.Attendance.Report;
using Vulcanova.Features.Auth;
using Vulcanova.Features.Auth.Accounts;
using VulcanTest.Vulcan;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VulcanForWindows.UserControls.Widgets
{
    public sealed partial class AttendanceWidget : UserControl, INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;
        public SolidColorPaint LegendTextPaint { get; set; } =
        new SolidColorPaint
        {
            Color = new SKColor(255, 255, 255)
        };
        void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public NewResponseEnvelope<Lesson> att { get; set; } = new NewResponseEnvelope<Lesson>();
        public int UnjustifiedCount => (att != null) ? ((!att.isLoaded) ? -1 :
            (att.Entries.Where(r => r.PresenceType != null).Where(r => r.PresenceType.Absence && (!r.PresenceType.AbsenceJustified && !r.PresenceType.LegalAbsence)).Count())) : -1;
        public int InProgressCount => (att != null) ? ((!att.isLoaded) ? -1 :
            (att.Entries.Where(r => r.PresenceType != null).Where(r => r.PresenceType.Absence).Where(r => r.JustificationStatus != null)
            .Where(r => r.JustificationStatus == Vulcanova.Uonet.Api.Lessons.JustificationStatus.Requested).Count())) : -1;
        public ObservableCollection<Lesson> lastNieusprawiedliwione = new ObservableCollection<Lesson>();

        public IEnumerable<ISeries> seriesRadial { get; set; } = new ObservableCollection<ISeries>();

        public float PresentPercent { get; set; }
        public string PresentPercentDisplay { get => PresentPercent.ToString("0.00") + "%"; }
        public bool IsLoading { get; set; } = true;
        public AttendanceWidget()
        {
            this.InitializeComponent();
            FetchAttendance(new AccountRepository().GetActiveAccountAsync());
            MonthsAttendance.Month = (DateTime.Now.AddDays(-3));

            InitializeTimer();
            SetRandomIndex();
        }


        private DispatcherTimer _timer;
        private void InitializeTimer()
        {
            _timer = new DispatcherTimer();
            _timer.Tick += Timer_Tick;
            _timer.Interval = TimeSpan.FromSeconds(20); // Adjust interval as needed
            _timer.Start();
        }
        private void SetRandomIndex()
        {
            var _random = new Random();
            flipView.SelectedIndex = _random.Next(0, flipView.Items.Count);
        }
        private void Timer_Tick(object sender, object e)
        {
            // Move to the next item in FlipView
            if (flipView.SelectedIndex < flipView.Items.Count - 1)
                flipView.SelectedIndex++;
            else
                flipView.SelectedIndex = 0;
        }

        private async void FetchAttendance(Account acc)
        {
            PresentPercent = await AttendanceReportService.GetPresencePercentage(acc);
            OnPropertyChanged(nameof(PresentPercent));
            OnPropertyChanged(nameof(PresentPercentDisplay));

            att.Updated += Att_Updated;
            await new LessonsService().GetLessonsForSchoolYear(acc, att);
            Att_Updated(null, null);
            IsLoading = false;
            OnPropertyChanged(nameof(IsLoading));

        }

        private void GenerateChart()
        {
            var wCalc = att.entries.Where(r => r.CalculatePresence).Where(r=>r.PresenceType!=null);
            int present = wCalc.Where(r => r.PresenceType.Presence).Count();
            int absnet = wCalc.Where(r => r.PresenceType.Absence && !r.PresenceType.AbsenceJustified && !r.PresenceType.LegalAbsence).Count();
            int legalAbsnet = wCalc.Where(r => r.PresenceType.AbsenceJustified || r.PresenceType.LegalAbsence).Count();
            int late = wCalc.Where(r => r.PresenceType.Late).Count();
            int _index = 0;
            var names = new string[] { "Obecność", "Spóźnienie", "Nieobecność uspr.", "Nieobeność nieuspr" };
            var colors = new SKColor[] { new SKColor(2, 209, 33), new SKColor(227, 223, 9), new SKColor(227, 103, 9), new SKColor(227, 31, 9) };
            seriesRadial = new int[]
            {
                present,late,legalAbsnet,absnet
            }.AsPieSeries((value, series) =>
            {
                series.Fill = new SolidColorPaint(colors[_index]);
                series.Name = names[_index++ % names.Length];
                series.MaxRadialColumnWidth = 60;
                series.ToolTipLabelFormatter = point => $"{point.StackedValue!.Share:P2}";
            });
        }

        private void Att_Updated(object sender, IEnumerable<Lesson> e)
        {
            GenerateChart();
            lastNieusprawiedliwione.ReplaceAll(
                att.Entries
                .Where(r => r.Date >= TimetableDayGrouper.GetStartOfWeek(DateTime.Now.AddDays(-7), false))
                .Where(r => r.PresenceType != null).Where(r => r.PresenceType.Absence).
                GroupBy(r => r.CanBeJustified).OrderBy(r => r.Key ? 0 : 1).Select(r => r.OrderByDescending(h => h.Date.AddMinutes(-h.No)))
                .SelectMany(r => r));

            OnPropertyChanged(nameof(seriesRadial));
            OnPropertyChanged(nameof(UnjustifiedCount));
            OnPropertyChanged(nameof(InProgressCount));
        }
        private void flipView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_timer == null) return;
            _timer.Stop();
            _timer.Start();
        }
    }
}
