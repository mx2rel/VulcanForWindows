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
using VulcanForWindows.Vulcan;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Vulcanova.Features.Attendance;
using Vulcanova.Features.Auth;
using System.Collections.ObjectModel;
using VulcanTest.Vulcan;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Globalization;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VulcanForWindows
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AttendancePage : Page
    {

        public NewResponseEnvelope<Lesson> env;

        public AttendanceDay[] entries { get; set; }
        public AttendanceDay[] filteredEntries { get; set; }
        public DateTime week;

        public AttendancePage()
        {
            filteredEntries = new AttendanceDay[0];
            week = GetStartOfTheWeek(DateTime.Now);
            Debug.WriteLine(week);
            entries = new AttendanceDay[0];
            Debug.WriteLine(JsonConvert.SerializeObject(entries));
            this.InitializeComponent();
            GetEnvelope();
        }

        public DateTime GetStartOfTheWeek(DateTime t) => TimetableDayGrouper.GetStartOfWeek(t, true);

        public async void GetEnvelope()
        {
            env = await new LessonsService().GetLessonsByMonth(new AccountRepository().GetActiveAccountAsync(), DateTime.Now);
            LoadingBar.Visibility = Visibility.Collapsed;
            env.Updated += Env_Updated;
            Spawn();
        }

        private void Env_Updated(object sender, IEnumerable<Lesson> e)
        {
            entries = (AttendanceDay.GetDays(env.entries.ToArray()));
            Spawn();
        }

        private void Next(object sender, RoutedEventArgs e)
        {
            week = week.AddDays(7);
            Spawn();
        }
        private void Prev(object sender, RoutedEventArgs e)
        {
            week = week.AddDays(-7);
            Spawn();
        }
        private void CurrentWeek(object sender, RoutedEventArgs e)
        {
            week = GetStartOfTheWeek(DateTime.Now);
            Spawn();
        }

        void ChangeWeek()
        {
            filteredEntries = entries.Where(r => r.date.Date >= week.Date && r.date.Date <= week.AddDays(4).Date).ToArray();
        }

        private void Spawn()
        {
            ChangeWeek();
            sel.Text = GetStartOfTheWeek(week).ToString("dd/MM") + " - " + GetStartOfTheWeek(week).AddDays(6).ToString("dd/MM");


            gr.Children.Clear();

            for (int i = 0; i < filteredEntries.Length; i++)
            {
                var sp = (this.Resources["AttendanceDayDisplay"] as DataTemplate).LoadContent() as StackPanel;

                gr.Children.Add(sp);

                sp.DataContext = filteredEntries[i];
                Grid.SetColumn(sp, i);
            }
            if (filteredEntries.Length == 0)
            {
                var r = new TextBlock();
                r.Text = "Brak wpisów na ten okres.";
                r.TextAlignment = TextAlignment.Center;
                r.HorizontalAlignment = HorizontalAlignment.Center;
                r.VerticalAlignment = VerticalAlignment.Center;
                gr.Children.Add(r);
            }
        }
    }

    public class AttendanceDay
    {
        public DateTime date;
        public Lesson[] lessons;

        public string day => new CultureInfo("en-US", false).TextInfo.ToTitleCase(date.ToString("dddd, dd/MM"));

        public AttendanceDay(DateTime d, Lesson[] l)
        {
            d.AddHours(-d.Hour);
            d.AddMinutes(-d.Minute);
            d.AddSeconds(-d.Second);
            d.AddMilliseconds(-d.Millisecond);
            date = d;
            lessons = l.OrderBy(r => r.Start).ToArray();
        }

        public static AttendanceDay[] GetDays(Lesson[] l)
        {
            return l.GroupBy(r => r.Date).OrderBy(r => r.Key).Select(r => new AttendanceDay(r.Key, r.ToArray())).ToArray();
        }
    }

}
