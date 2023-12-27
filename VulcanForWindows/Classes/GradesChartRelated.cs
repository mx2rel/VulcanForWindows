using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;

namespace VulcanForWindows.Classes
{
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
                    value = r.Value.CountAverage().ToString("0.00") + $" ({r.Value.Length} {OcenToQuantity(r.Value.Length)})"
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
}
