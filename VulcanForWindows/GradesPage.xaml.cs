using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using VulcanForWindows.Classes;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VulcanForWindows
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class GradesPage : Page
    {
        public ChartData cd { get; set; }
        public GradesPage()
        {
            this.InitializeComponent();
            grades = SubjectGrades.GetSubjectsGrades();
            cd = ChartData.Generate(grades.SelectMany(r => r.grades).ToArray());
        }
        public SubjectGrades[] grades { get; set; }


        private void myButton_Click(object sender, RoutedEventArgs e)
        {
            myButton.Content = "Clicked";
        }

        private void Grade_ShowInfo(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            var g = (sender as ListView).DataContext as Grade;
            var content = (Resources["GradeInfoFlyout"] as DataTemplate);
            ContentControl contentControl = new ContentControl
            {
                ContentTemplate = content,
                Content = g
            };

            var flyout = new Flyout();
            flyout.Content = contentControl;
            flyout.ShowAt(sender as FrameworkElement);
        }

        private void ChangeChartAndTable(object sender, RoutedEventArgs e)
        {
            var og = table.Visibility;
            table.Visibility = chart.Visibility;
            chart.Visibility = og;
        }
    }
    public class TableRow
    {
        public string name { get; set; }
        public string value { get; set; }
    }
    public class ChartData
    {

        public ISeries[] Series { get; set; }

        public List<Axis> XAxes { get; set; }
        public Dictionary<DateTime, Grade[]> data { get; private set; }
        public TableRow[] tableData
        {
            get
            {
                return data.Select(r => new TableRow
                {
                    name = r.Key.ToString("MMMM/yyyy").Replace(".", " "),
                    value = CountAverage(r.Value).ToString("0.00") + $" ({r.Value.Length} {OcenToQuantity(r.Value.Length)})"
                }).ToArray();
            }
        }
        public static ChartData Generate(Grade[] grades)
        {
            var chartData = new ChartData();
            chartData.data = GetData(grades);

            chartData.Series = new ISeries[]
            {
                new LineSeries<double>
            {
                Values = chartData.data.Select(r=>CountAverage(r.Value)).ToArray(),
                Fill = null,
                XToolTipLabelFormatter =
        (chartPoint) => $"{chartData.data.Keys.ElementAt(chartPoint.Index).ToString("MMMM/yy").Replace(".", " ")}: {chartPoint.PrimaryValue}" +
        $"{Environment.NewLine} ({chartData.data.ToArray()[chartPoint.Index].Value.Length} {OcenToQuantity(chartData.data.ToArray()[chartPoint.Index].Value.Length)})"
            }

            };

            chartData.XAxes = new List<Axis>
            {
                new Axis
                {
                    Labels = chartData.data.Select(r=>r.Key.ToString("MMM/yy").Replace(".", " '")).ToArray()
                }
            };

            return chartData;
        }

        static string OcenToQuantity(int ilosc)
        {
            // Ustawiamy początkowe formy słowa
            string formaPodstawowa = "ocena";
            string formaMnoga = "oceny";
            string formaLiczba = "ocen";

            // Sprawdzamy i modyfikujemy formy słowa w zależności od ilości
            if (ilosc == 1)
            {
                return formaPodstawowa;
            }
            else if (ilosc % 10 >= 2 && ilosc % 10 <= 4 && (ilosc % 100 < 10 || ilosc % 100 >= 20))
            {
                return formaMnoga;
            }
            else
            {
                return formaLiczba;
            }
        }

        public static double CountAverage(Grade[] grades)
        {
            decimal sum = 0;
            decimal weightSum = 0;

            foreach (var grade in grades)
            {
                if (grade.Value != null)
                {
                    sum += grade.Value.GetValueOrDefault() * grade.Column.Weight;
                    weightSum += grade.Column.Weight;
                }
            }
            if (weightSum == 0) return 0;

            return (double)Math.Round(sum / weightSum, 2);
        }

        ///<summary>Groups an array of Grade objects by month and year, returning a Dictionary with DateTime keys and Grade arrays as values.</summary>
        ///<param name="grades">Array of Grade objects to be grouped</param>
        ///<returns>A Dictionary<DateTime, Grade[]> containing grouped data</returns>
        public static Dictionary<DateTime, Grade[]> GetData(Grade[] grades)
        {
            Dictionary<DateTime, Grade[]> grouped = grades.GroupBy(r => new DateTime(r.DateCreated.Value.Year, r.DateCreated.Value.Month, 1))
                .Select(r => new KeyValuePair<DateTime, Grade[]>(r.Key, r.ToArray())).ToArray().ToDictionary(pair => pair.Key, pair => pair.Value);

            DateTime newest = grouped.Keys.ElementAt(0);
            foreach (var date in grouped.Keys.ToArray())
                if (date > newest)
                    newest = date;

            //fill all missing months
            for (int i = 0; i < grouped.Keys.Count; i++)
            {
                var date = grouped.Keys.ElementAt(i);
                if (!grouped.ContainsKey(date.AddMonths(1)))
                    if (date.AddMonths(1) <= DateTime.Today)
                        if (newest > date)
                            grouped.Add(date.AddMonths(1), new Grade[0]);
            }

            //order from oldest to newest
            grouped = grouped.ToArray().OrderBy(r => r.Key).ToDictionary(pair => pair.Key, pair => pair.Value);
            return grouped;
        }
    }
}
