using LiveChartsCore;
using LiveChartsCore.Kernel;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Drawing;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using LiveChartsCore.SkiaSharpView.VisualElements;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Vulcanova.Features.Grades;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VulcanForWindows.UserControls.ClassmatesGrades
{
    public sealed partial class SingleClassmateGrades : UserControl, INotifyPropertyChanged
    {


        public static readonly DependencyProperty LoadOnSetProperty =
            DependencyProperty.Register("LoadOnSet", typeof(bool), typeof(SingleClassmateGrades), new PropertyMetadata(true, LoadOnSet_Changed));

        public bool LoadOnSet
        {
            get => (bool)GetValue(LoadOnSetProperty);
            set => SetValue(LoadOnSetProperty, value);
        }

        private static void LoadOnSet_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SingleClassmateGrades control && e.NewValue is bool newValue)
            {
                // TODO: Implement your logic here
            }
        }


        public static readonly DependencyProperty IsCompactProperty =
            DependencyProperty.Register("IsCompact", typeof(bool), typeof(SingleClassmateGrades), new PropertyMetadata(false, IsCompact_Changed));

        public bool IsCompact
        {
            get => (bool)GetValue(IsCompactProperty);
            set => SetValue(IsCompactProperty, value);
        }

        private static void IsCompact_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SingleClassmateGrades control && e.NewValue is bool newValue)
            {
                control.OnPropertyChanged(nameof(IsCompact));
            }
        }


        public static readonly DependencyProperty ErrorMessageOrientationProperty =
            DependencyProperty.Register("ErrorMessageOrientation", typeof(Orientation), typeof(SingleClassmateGrades), new PropertyMetadata(Orientation.Vertical, ErrorMessageOrientation_Changed));

        public Orientation ErrorMessageOrientation
        {
            get => (Orientation)GetValue(ErrorMessageOrientationProperty);
            set => SetValue(ErrorMessageOrientationProperty, value);
        }

        private static void ErrorMessageOrientation_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SingleClassmateGrades control && e.NewValue is Orientation newValue)
            {
                control.OnPropertyChanged(nameof(ErrorMessageOrientation));
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public static readonly DependencyProperty UserGradeProperty =
            DependencyProperty.Register("UserGrade", typeof(Grade), typeof(SingleClassmateGrades), new PropertyMetadata(null, Grade_Changed));

        public Grade UserGrade
        {
            get => (Grade)GetValue(UserGradeProperty);
            set => SetValue(UserGradeProperty, value);
        }


        public static readonly DependencyProperty ColumnIdProperty =
            DependencyProperty.Register("ColumnId", typeof(int), typeof(SingleClassmateGrades), new PropertyMetadata(null, ColumnId_Changed));

        private static void ColumnId_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SingleClassmateGrades control && e.NewValue is int newValue)
            {

                //control.GenerateChart();
                //Debug.WriteLine("CID:" + newValue);

            }
        }
        private static void Grade_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SingleClassmateGrades control && e.NewValue is Grade newValue)
            {
                if (control.LoadOnSet)
                    control.GenerateChart(newValue.Column.Id, newValue.ActualValue);

            }
        }

        public int ColumnId
        {
            get => (int)GetValue(ColumnIdProperty);
            set => SetValue(ColumnIdProperty, value);
        }
        public SingleClassmateGrades()
        {
            this.InitializeComponent();
            //GenerateChart();
        }
        double highestGrade = 0;
        float betterThanPercentile = 0;
        public bool FailedToLoad { get; set; }
        public bool DisplayLoadingIndicator { get; set; } = true;
        string betterThanDisplay => (betterThanPercentile == -1) ? "" : $"Lepiej niż {betterThanPercentile.ToString("0.00")}% klasy";
        string highestGradeDisplay => $"Najwyższa ocena w klasie to {highestGrade}";

        public bool ShouldDisplayChart => GradesAvaible >= MinGradesAvaibleToShowChart;
        public bool TooLittleGrades => GradesAvaible < MinGradesAvaibleToShowChart && !FailedToLoad;

        int GradesAvaible { get; set; }
        int MinGradesAvaibleToShowChart { get; set; } = 4;
        public async void GenerateChart(int columnId, decimal? userGrade)
        {
            GenerateChart(columnId, userGrade.HasValue ? (double)userGrade.Value : -1);
        }

        public async void GenerateChart(int columnId, double userGrade = 0)
        {
            FailedToLoad = false;
            DisplayLoadingIndicator = true;
            OnPropertyChanged(nameof(DisplayLoadingIndicator));


            await Task.Delay(10);
            if (ColumnId == 0) return;
            try
            {
                var classmatesGrades = await Classes.VulcanGradesDb.ClassmateGradesService.GetSingleClassmateColumn(columnId);
                if (classmatesGrades == null) return;

                foreach (var v in classmatesGrades.Grades)
                    if (v.Value > highestGrade) highestGrade = v.Value;

                GradesAvaible = classmatesGrades.Grades.Length;
                var groupped = classmatesGrades.Grades
        .GroupBy(r => Math.Round(r.Value - 0.01))
        .ToDictionary(
            r => r.Key,
            r => r.ToArray()
        );
                var list = new List<int>();

                double maxgrade = 6;

                int betterGradesCount = 0;
                int worseOrEqalGradesCount = 0;

                foreach (var grade in groupped)
                {
                    if (grade.Key > maxgrade) maxgrade = Math.Ceiling(grade.Key);

                    if (userGrade != -1)
                    {
                        if (grade.Key > userGrade)
                            betterGradesCount += grade.Value.Length;
                        if (grade.Key <= userGrade) worseOrEqalGradesCount += grade.Value.Length;
                    }
                }
                if (userGrade != -1)
                    betterThanPercentile = (float)worseOrEqalGradesCount / (float)(betterGradesCount + worseOrEqalGradesCount) * 100f;
                else betterThanPercentile = -1;

                List<float> avaibleGrades = new List<float>();
                for (int i = 1; i <= maxgrade; i++) avaibleGrades.Add(i);


                for (int i = 1; i <= maxgrade; i++)
                {
                    if (groupped.TryGetValue(i, out var value))
                        list.Add(value.Length);
                    else
                        list.Add(0);
                }
                var lArray = list.ToArray();
                var labels = avaibleGrades.Select(r => r.ToString()).Where(r => !string.IsNullOrEmpty(r)).ToArray();
                Series = new ISeries[]
                {
                new LineSeries<int>
                {
                Values = lArray,
                XToolTipLabelFormatter = (chartPoint) =>
                    {
                        var tooltipContent = $"Ocena {labels[chartPoint.Index]}";
                        return tooltipContent;
                    },
                }
                };

                XAxes = new List<Axis>
            {
                new Axis()
                {
                    Labels =labels,

                }

            };

                grid.DataContext = this;
                OnPropertyChanged(nameof(Series));
                OnPropertyChanged(nameof(XAxes));
                OnPropertyChanged(nameof(betterThanDisplay));
                OnPropertyChanged(nameof(highestGradeDisplay));
                OnPropertyChanged(nameof(GradesAvaible));
                OnPropertyChanged(nameof(ShouldDisplayChart));
            }
            catch (System.Net.Http.HttpRequestException e)
            {
                FailedToLoad = true;
            }


            DisplayLoadingIndicator = false;
            OnPropertyChanged(nameof(FailedToLoad));
            OnPropertyChanged(nameof(DisplayLoadingIndicator));
            OnPropertyChanged(nameof(TooLittleGrades));
        }
        public ISeries[] Series { get; set; } = new ISeries[0];

        public List<Axis> XAxes { get; set; } = new List<Axis>();

        private void FontIcon_PointerEntered2(object sender, PointerRoutedEventArgs e)
        {
            TeachingTipClassmateGrades2.IsOpen = true;
        }

        private void FontIcon_PointerExited2(object sender, PointerRoutedEventArgs e)
        {
            TeachingTipClassmateGrades2.IsOpen = false;

        }

        private void TryAgain(object sender, RoutedEventArgs e)
        {
            GenerateChart(UserGrade.Column.Id, UserGrade.VulcanValue);
        }
    }
}
