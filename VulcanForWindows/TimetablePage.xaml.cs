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
using Newtonsoft.Json;
using Windows.Foundation.Collections;
using System.Collections.ObjectModel;
using VulcanTest.Vulcan.Timetable;
using VulcanForWindows.Vulcan.Timetable;
using Vulcanova.Features.Auth;
using VulcanTest.Vulcan;
using System.Diagnostics;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VulcanForWindows
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class TimetablePage : Page
    {

        public TimetableResponseEnvelope env;
        TimetableDayGrouper tdg;

        public Day[] display
        {
            get => (tdg.GetWeek(week));
        }

        public DateTime week;

        public TimetablePage()
        {
            this.InitializeComponent();
            week = new DateTime(2023, 12, 10);

            AssignTimetable();
            //appointments = new ObservableCollection<TimetableEntry>(TimetableEntry.Generate(RandomGenerator.GenerateRandomTimetable()));

        }

        public async void AssignTimetable()
        {
            var acc = new AccountRepository().GetActiveAccountAsync();

            env = await new OgTimetable().GetPeriodEntriesByMonth(acc, DateTime.Today, false, false);
            env.EntriesUpdated += HandleEntriesUpdated;
            tdg = new TimetableDayGrouper(env.Entries);

        }
        void HandleEntriesUpdated(object sender, IEnumerable<TimetableEntry> updatedGrades)
        {
            tdg = new TimetableDayGrouper(env.Entries);
            lv.ItemsSource = display;
            lv.UpdateLayout();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            (sender as Button).Content = tdg.entries.Length;
            lv.ItemsSource = display;
            lv.UpdateLayout();
        }
    }

    public class TimetableDayGrouper
    {
        public TimetableDayGrouper(IEnumerable<TimetableEntry> e)
        {
            var v = new Dictionary<DateTime, List<TimetableEntry>>();
            foreach (var entry in e.Where(r => r.Visible))
            {
                if (!v.ContainsKey(entry.Date))
                    v.Add(entry.Date, new List<TimetableEntry>());

                v[entry.Date].Add(entry);
            }
            Debug.Write(JsonConvert.SerializeObject(v) + "\n\n\np\n" + JsonConvert.SerializeObject(e));
            entries = v.Select(r => new KeyValuePair<DateTime, TimetableEntry[]>(r.Key, r.Value.ToArray().OrderBy(d => d.start).ToArray())).OrderBy(r => r.Key).ToArray();
        }

        public KeyValuePair<DateTime, TimetableEntry[]>[] entries { get; set; }

        public Day[] GetWeek(DateTime date)
        {
            date.AddDays(1);
            DateTime startOfWeek = date.AddDays(-(int)date.DayOfWeek + (int)DayOfWeek.Monday);
            DateTime endOfWeek = startOfWeek.AddDays(4); // Friday is 4 days after Monday

            return entries.Where(r => r.Key.Date >= startOfWeek.Date && r.Key.Date <= endOfWeek.Date).Select(r => new Day(r)).ToArray();
        }
    }

    public class Day
    {

        public Day(KeyValuePair<DateTime, TimetableEntry[]> e)
        {
            entries = e;
        }

        public KeyValuePair<DateTime, TimetableEntry[]> entries { get; set; }

        public TimetableEntry[] e => entries.Value;

        public string day => entries.Key.ToString();
    }
}
