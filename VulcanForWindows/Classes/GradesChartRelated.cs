using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiveChartsCore;
using LiveChartsCore.Kernel;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Drawing;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.VisualElements;
using SkiaSharp;

namespace VulcanForWindows.Classes
{
    public class TableRow
    {
        public string name { get; set; }
        public string value { get; set; }
    }
    public class MonthChartData
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
                    value = r.Value.CountAverage().ToString("0.00") + $" ({r.Value.Length} {OcenToQuantity(r.Value.Length)})"
                }).ToArray();
            }
        }

        public static MonthChartData Generate(Grade[] grades)
        {
            var chartData = new MonthChartData();
            chartData.data = GetData(grades);

            chartData.Series = new ISeries[]
            {
                new LineSeries<double>
            {
                Values = chartData.data.Select(r=>r.Value.CountAverage()).ToArray(),
                Fill = null,
                XToolTipLabelFormatter =
        (chartPoint) => $"{chartData.data.Keys.ElementAt(chartPoint.Index).ToString("MMMM/yy").Replace(".", " ")}: {chartPoint.PrimaryValue.ToString("0.00")}" +
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

    public class GradesCountChartData
    {
        public List<ChartElement<SkiaSharpDrawingContext>> VisualElements { get; set; }

        public ISeries[] Series { get; set; }

        public List<Axis> YAxes { get; set; }
        public Dictionary<float, int> data { get; private set; }
        //public TableRow[] tableData
        //{
        //    get
        //    {
        //        return data.Select(r => new TableRow
        //        {
        //            name = r.Key.ToString("MMMM/yyyy").Replace(".", " "),
        //            value = r.Value.CountAverage().ToString("0.00") + $" ({r.Value.Length} {OcenToQuantity(r.Value.Length)})"
        //        }).ToArray();
        //    }
        //}

        public static GradesCountChartData Generate(Grade[] grades)
        {
            var chartData = new GradesCountChartData();
            var v = grades.GroupBy(r => r.Value).ToArray();
            chartData.data = v.Where(r => r.Key != null).OrderBy(r => r.Key).ToDictionary(group => (float)group.Key.GetValueOrDefault(), group => group.ToArray().Length);

            for (int i = 1; i <= 6; i++)
            {
                if (!chartData.data.ContainsKey(i))
                    chartData.data.Add(i, 0);
            }

            chartData.data = chartData.data.ToArray().OrderBy(r => r.Key).ToDictionary(group => (float)group.Key, group => group.Value);

            var visuals = new List<ChartElement<SkiaSharpDrawingContext>>();

            var rectangleVisual = new GeometryVisual<RectangleGeometry>
            {
                X = 0,
                Y = grades.CountAverage() - 1,
                Width = 100,
                LocationUnit = LiveChartsCore.Measure.MeasureUnit.ChartValues,
                SizeUnit = LiveChartsCore.Measure.MeasureUnit.ChartValues,
                Height = 0.02,
                Fill = new SolidColorPaint(new SKColor(239, 83, 80, 220)) { ZIndex = 10 },
                Stroke = new SolidColorPaint(new SKColor(239, 83, 80)) { ZIndex = 10, StrokeThickness = 1.5f },
                Label = $"Średnia: {grades.CountAverage()}",
                LabelPaint = new SolidColorPaint(new SKColor(239, 83, 80)) { ZIndex = 11 },
                LabelSize = 12
            };

            visuals.Add(rectangleVisual);
            chartData.VisualElements = visuals;

            chartData.Series = new ISeries[]
            {
                new RowSeries<int>
            {
                Values = chartData.data.Select(r=>r.Value).ToArray(),
                DataPadding= new LiveChartsCore.Drawing.LvcPoint(0,0),
                XToolTipLabelFormatter =
        (chartPoint) => $"{chartData.data.Keys.ElementAt(chartPoint.Index)}: {chartData.data.ToArray()[chartPoint.Index].Value} {OcenToQuantity(chartData.data.ToArray()[chartPoint.Index].Value)}"
            }

            };

            chartData.YAxes = new List<Axis>
            {
                new Axis
                {
                    Labels = chartData.data.Select(r=>r.Key.ToString("0.00")).ToArray(),
                    MinLimit=0
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
    }
}
