using DevExpress.Pdf.Drawing.DirectX;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
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
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using VulcanForWindows.Classes;
using VulcanForWindows.Vulcan.Grades;
using Vulcanova.Features.Auth;
using Vulcanova.Features.Auth.Accounts;
using Vulcanova.Features.Grades;
using Vulcanova.Features.Shared;
using VulcanTest.Vulcan;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VulcanForWindows.UserControls.Widgets
{
    public sealed partial class GradesWidget : UserControl, INotifyPropertyChanged
    {
        private IDictionary<Period, Grade[]> res;

        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public ObservableCollection<SubjectGrades> sg { get; set; } = new ObservableCollection<SubjectGrades>();

        public ISeries[] Series { get; set; } = new ISeries[0];

        public List<Axis> XAxes { get; set; } = new List<Axis>();

        public GradesWidget()
        {
            this.InitializeComponent();
            Load();
            InitializeTimer();
            SetRandomIndex();
        }

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            //(e.ClickedItem as SubjectGrades)
            //TODO: LOAD SUBJECT
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

        async void Load()
        {
            await FetchGrades(new AccountRepository().GetActiveAccount());
            LoadChartData(res.SelectMany(r => r.Value).ToArray());

        }
        Grade[] thisPeriodGrades { get; set; }
        private async Task FetchGrades(Account acc)
        {
            res = await new GradesService().FetchGradesFromCurrentLevelAsync(acc);
            thisPeriodGrades = res.OrderBy(r => r.Key.Id).Last().Value;
            sg.ReplaceAll(SubjectGrades.CreateRecent(res.SelectMany(r => r.Value).ToArray()));
            OnPropertyChanged(nameof(thisPeriodGrades));
        }
        static DateTime GetWeekStartDate(DateTime date)
        {
            DayOfWeek firstDayOfWeek = CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek;
            int diff = date.DayOfWeek - firstDayOfWeek;
            if (diff < 0)
                diff += 7;
            return date.AddDays(-diff).Date;
        }
        void LoadChartData(Grade[] g, DateTime? startDate = null)
        {
            var v = CalculateChart(g, startDate);
            Series = v.Series;
            XAxes = v.XAxes;
            OnPropertyChanged(nameof(Series));
            OnPropertyChanged(nameof(XAxes));
        }

        public static (ISeries[] Series, List<Axis> XAxes) CalculateChart(Grade[] g, DateTime? startDate = null)
        {
            ISeries[] Series = new ISeries[0];

            List<Axis> XAxes = new List<Axis>();

            if (!startDate.HasValue)
                startDate = GetWeekStartDate(DateTime.Now).AddDays(-7 * 6);

            (DateTime month, Grade[] grades)[] grouped =
            g.GroupBy(r => GetWeekStartDate(r.DateModify))
            .Select(group =>
            {
                DateTime firstDayOfWeek = group.Key;
                Grade[] gradesInGroup = group.Select(item => item).ToArray();
                return (firstDayOfWeek, gradesInGroup);
            })
            .OrderBy(group => group.firstDayOfWeek).ToArray()
            .ToArray();


            double sumOfWeights = 0;
            double sum = 0;

            List<(DateTime firstDayOfWeek, double avg)> values = new List<(DateTime firstDayOfWeek, double avg)>();

            for (int i = 0; i < grouped.Length; i++)
            {
                var output = grouped[i].grades.CalculateAverageRaw();
                sumOfWeights += output.weights;
                sum += output.sum;
                values.Add((grouped[i].month, Math.Round(sum / sumOfWeights, 2)));
            }

            Series = new ISeries[] {
                new LineSeries<DateTimePoint>
                {
                    Values = values.Where(r => r.firstDayOfWeek >= startDate).Select(r => new DateTimePoint(
                        r.firstDayOfWeek, r.avg)).ToArray(),
                    Fill = new LinearGradientPaint(
                new [] { new SKColor(0, 255, 40, 150), new SKColor(0, 255, 40, 0) },
                new SKPoint(0.5f, 0),
                new SKPoint(0.5f, 1)),
                    DataPadding = new LiveChartsCore.Drawing.LvcPoint(0, 1),
                    LineSmoothness = 1,
                    Stroke = new SolidColorPaint(new SKColor(0,230,50)) {
                    StrokeThickness=3},
                    GeometryFill = new SolidColorPaint(new SKColor(255,255,255)),
                    GeometryStroke = new SolidColorPaint(new SKColor(0,230,50)){
                    StrokeThickness=3},
                }
            };


            XAxes = new List<Axis>
            {
                new DateTimeAxis(TimeSpan.FromDays(7), date => date.ToString("dd/MM/yy"))

            };

            return (Series, XAxes);
        }

        private void flipView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_timer == null) return;
            _timer.Stop();
            _timer.Start();
        }
    }
}
