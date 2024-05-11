using LiveChartsCore;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Vulcanova.Features.Auth;
using Vulcanova.Features.Grades;
using Vulcanova.Features.Shared;
using Windows.Foundation;
using Windows.Foundation.Collections;

using LiveChartsCore;
using LiveChartsCore.Kernel;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Drawing;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.VisualElements;
using SkiaSharp;
using System.Globalization;
using Vulcanova.Features.Grades.Summary;
using VulcanForWindows.Classes;
using System.Threading.Tasks;
using System.Diagnostics;
using LiveChartsCore.Defaults;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VulcanForWindows.UserControls.GradesCharts
{
    public sealed partial class SubjectMonthGrades : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        Grade[] Grades;

        public string[] SubjectNames { get; set; } = new string[0];
        public Subject[] Subjects { get; set; } = new Subject[0];

        public SubjectMonthGrades()
        {
            this.InitializeComponent();
            Init();
        }

        async void Init()
        {
            await LoadData();
            GenerateData();
        }

        async Task LoadData()
        {
            Grades = (await new GradesService().FetchGradesFromCurrentLevelAsync(new AccountRepository().GetActiveAccount()))
                .SelectMany(r => r.Value).ToArray();
            Subjects = Grades.GroupBy(r => r.Column.Subject.Id).Select(r => r.FirstOrDefault().Column.Subject).ToArray();
            SubjectNames = (new string[] { "Wszyskie przedmioty" }).Concat(Grades.GroupBy(r => r.Column.Subject.Id).Select(r => r.FirstOrDefault().Column.Subject.Name)).ToArray();
            OnPropertyChanged(nameof(Subjects));
            OnPropertyChanged(nameof(SubjectNames));
            TypeBox.SelectedIndex = 1;

        }

        public ISeries[] Series { get; set; } = new ISeries[0];

        public List<Axis> XAxes { get; set; } = new List<Axis>();

        void GenerateData()
        {
            bool allSubjects = SelectedSubjects.Length == 0;
            var SubjectFilter = allSubjects ? (new List<int>()) : SelectedSubjects.Select(r => r.Id).ToList();
            var FilteredGrades = Grades.Where(r => SubjectFilter.Contains(r.Column.Subject.Id) || allSubjects).ToArray();
            IEnumerable<(string subject, IOrderedEnumerable<IGrouping<string, Grade>> Data)> GroupedByMonthGrade =
                FilteredGrades.GroupBy(r => allSubjects ? "Ogół" : (r.Column.Subject.Name))
                .Select(r => (r.Key, r.ToArray().GroupBy(r => r.DateModify.ToString("dd/MM/yy"))
                .OrderBy(r => DateTime.ParseExact(/*"01." +*/ r.Key, "dd.MM.yy", CultureInfo.CurrentCulture))));
            List<(string subject, List<(DateTime day, double Value)> data)> ActualValues = new List<(string subject, List<(DateTime day, double Value)> data)>();

            (Subject Subject, Grade[] Grades)[] SubjectGrades = FilteredGrades.OrderBy(r=>r.DateModify).GroupBy(r => r.Column.Subject.Id).Select(r => (r.First().Column.Subject, r.ToArray())).ToArray();

            if (TypeBox.SelectedIndex == 1)
            {


                foreach ((string subject, IOrderedEnumerable<IGrouping<string, Grade>> Data) element in GroupedByMonthGrade.ToArray())
                {
                    int sumOfWeights = 0;
                    double sum = 0;
                    List<(DateTime day, double value)> l = new List<(DateTime day, double value)>();

                    for (int i = 0; i < element.Data.Count(); i++)
                    {
                        var data = element.Data.ToArray()[i];
                        var v = data.ToArray().CalculateAverageRaw();
                        if (v.avg == 0) continue;
                        sumOfWeights += v.weights;
                        sum += v.sum;
                        l.Add((DateTime.ParseExact(/*"01." + */data.Key, "dd/MM/yy", CultureInfo.CurrentCulture),
                            Math.Round(sum / sumOfWeights, 2)));
                        //ActualValues = FillMissingDays(ActualValues);

                    }
                    ActualValues.Add((element.subject, l));
                }
            }
            else
            {
                foreach ((string subject, IOrderedEnumerable<IGrouping<string, Grade>> Data) element in GroupedByMonthGrade.ToArray())
                {
                    List<(DateTime day, double value)> days = new List<(DateTime day, double value)>();

                    for (int i = 0; i < element.Data.Count(); i++)
                    {
                        var data = element.Data.ToArray()[i];

                        var v = data.ToArray().CalculateAverage();
                        if (v == 0) continue;

                        days.Add((DateTime.ParseExact(/*"01."+*/data.Key, "dd/MM/yy", CultureInfo.CurrentCulture), v));
                    }
                    ActualValues.Add((element.subject, days));
                }
            }

            Series = ActualValues.Select(r =>
                new LineSeries<DateTimePoint>
                {
                    Values = r.data.Select(r => new DateTimePoint(
                        r.day, (double)r.Value)).ToArray(),
                    Name = r.subject,
                    IsVisibleAtLegend = true,
                    XToolTipLabelFormatter = (chartPoint) =>
                    {
                        var selectedDate = r.data.ElementAt(chartPoint.Index).day;
                        var tooltipContent = $"{selectedDate.ToString("dd MMM yy", CultureInfo.CurrentCulture)}{Environment.NewLine}";
                        return tooltipContent;
                    },
                    Fill = null
                    //    Fill = new LinearGradientPaint(
                    //new[] { new SKColor(0, 255, 40, 150), new SKColor(0, 255, 40, 0) },
                    //new SKPoint(0.5f, 0),
                    //new SKPoint(0.5f, 1)),
                    //    DataPadding = new LiveChartsCore.Drawing.LvcPoint(0, 1),
                    //    LineSmoothness = 0.5f,

                    //    Stroke = new SolidColorPaint(new SKColor(0, 230, 50))
                    //    {
                    //        StrokeThickness = 3
                    //    },
                    //GeometryFill = new SolidColorPaint(new SKColor(255, 255, 255)),
                    //GeometryStroke = new SolidColorPaint(new SKColor(0, 230, 50))
                    //{
                    //    StrokeThickness = 3
                    //},
                }
                ).ToArray();
            //Series = Series.Concat(

            //        SubjectGrades.Select(r => new LineSeries<DateTimePoint>
            //        {
            //            Values = r.Grades.Where(g => g.VulcanValue.HasValue && g.Column.Weight > 0).Select(g => new DateTimePoint(g.DateModify.Date, (double)g.VulcanValue.GetValueOrDefault())),
            //            Fill= null,
            //            Stroke = null,
            //            Name = r.Subject.Name

            //        }).ToArray()).ToArray();


            XAxes = new List<Axis>
            {
                new DateTimeAxis(TimeSpan.FromDays(1), date => date.ToString("dd MMM yy"))

            };
            OnPropertyChanged(nameof(Series));
            OnPropertyChanged(nameof(XAxes));
        }

        private void ConfigChanged(object sender, SelectionChangedEventArgs e)
        {
            GenerateData();
        }

        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            popup.IsOpen = !popup.IsOpen;
        }

        private void ToggleButton_LostFocus(object sender, RoutedEventArgs e)
        {
            UIElement focusedElement = FocusManager.GetFocusedElement() as UIElement;

            if (focusedElement != null && !IsDescendantOf(focusedElement, popup))
            {
                popup.IsOpen = false;
            }
        }

        static bool IsDescendantOf(DependencyObject element, DependencyObject parent)
        {
            if (element == null || parent == null)
            {
                return false;
            }

            // Get the parent of the element
            DependencyObject currentParent = VisualTreeHelper.GetParent(element);

            // Traverse up the visual tree until the parent is found or we reach the root
            while (currentParent != null)
            {
                if (currentParent == parent)
                {
                    // The element is a descendant of the parent
                    return true;
                }

                // Move up to the next parent in the visual tree
                currentParent = VisualTreeHelper.GetParent(currentParent);
            }

            // The element is not a descendant of the parent
            return false;
        }

        private void Popup_Closed(object sender, object e)
        {
            toggleButton.IsChecked = false;
        }

        private void listview_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedSubjects = listview.SelectedItems.Select(r => (Subject)r).ToArray();
            GenerateData();

            toggleButton.Content = new TextBlock
            {
                TextAlignment = TextAlignment.Center,
                Text = (SelectedSubjects.Length == 0) ? ("Wszystkie przedmioty") :
                ((SelectedSubjects.Length <= 2) ? (string.Join(", ", SelectedSubjects.Select(r => r.Name))) :
                (SelectedSubjects.Length + " " + ((SelectedSubjects.Length > 4) ? "przedmiotów" : "przedmioty")))
            };
        }

        public Subject[] SelectedSubjects = new Subject[0];
    }
}
