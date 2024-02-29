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
using System.ComponentModel;
using VulcanForWindows.UserControls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VulcanForWindows
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AttendancePage : Page, INotifyPropertyChanged
    {

        public NewResponseEnvelope<Lesson> env;

        public AttendanceDay[] entries { get; set; }
        public AttendanceDay[] filteredEntries { get; set; }
        public DateTime week;

        public AttendancePage()
        {
            env = new NewResponseEnvelope<Lesson>();
            filteredEntries = new AttendanceDay[0];
            week = GetStartOfTheWeek(DateTime.Now);
            entries = new AttendanceDay[0];
            this.InitializeComponent();
            GetEnvelope();
        }

        public DateTime GetStartOfTheWeek(DateTime t) => TimetableDayGrouper.GetStartOfWeek(t, true);

        public async void GetEnvelope()
        {
            env.Updated += Env_Updated;

            await new LessonsService().GetLessonsForSchoolYear(new AccountRepository().GetActiveAccountAsync(), env);
            LoadingBar.Visibility = Visibility.Collapsed;
            Spawn();
        }

        private void Env_Updated(object sender, IEnumerable<Lesson> e)
        {
            var newV = AttendanceDay.GetDays(env.entries.ToArray());
            var needsRespawn = JsonConvert.SerializeObject(ChangeWeek(false)) != JsonConvert.SerializeObject(GetFilteredEntries(newV));
            entries = newV;
            if (needsRespawn)
                Spawn();
        }

        public bool IsCurrentWeekSelected => GetStartOfTheWeek(week).Ticks == GetStartOfTheWeek(DateTime.Now).Ticks;
        public bool IsCurrentWeekButtonActive => !IsCurrentWeekSelected;

        private void Next(object sender, RoutedEventArgs e)
        {
            week = week.AddDays(7);
            Spawn();

            OnPropertyChanged(nameof(IsCurrentWeekSelected));
            OnPropertyChanged(nameof(IsCurrentWeekButtonActive));
        }
        private void Prev(object sender, RoutedEventArgs e)
        {
            week = week.AddDays(-7);
            Spawn();

            OnPropertyChanged(nameof(IsCurrentWeekSelected));
            OnPropertyChanged(nameof(IsCurrentWeekButtonActive));
        }
        private void CurrentWeek(object sender, RoutedEventArgs e)
        {
            if (IsCurrentWeekSelected) return;
            week = GetStartOfTheWeek(DateTime.Now);
            Spawn();

            OnPropertyChanged(nameof(IsCurrentWeekSelected));
            OnPropertyChanged(nameof(IsCurrentWeekButtonActive));
        }

        AttendanceDay[] ChangeWeek(bool set = true)
        {
            var v = entries.Where(r => r.date.Date >= week.Date && r.date.Date <= week.AddDays(4).Date).ToArray();
            if (set) filteredEntries = v;
            return v;
        }
        AttendanceDay[] GetFilteredEntries(AttendanceDay[] d)
        {
            var v = d.Where(r => r.date.Date >= week.Date && r.date.Date <= week.AddDays(4).Date).ToArray();
            return v;
        }

        private void Spawn()
        {
            ChangeWeek();
            sel.Text = GetStartOfTheWeek(week).ToString("dd/MM") + " - " + GetStartOfTheWeek(week).AddDays(6).ToString("dd/MM");


            St.Children.Remove(r);
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
                var t = new InfoDisplayControl();
                //t.Text = "Brak wpisów na ten okres.";
                //t.TextAlignment = TextAlignment.Center;
                //t.HorizontalAlignment = HorizontalAlignment.Center;
                //t.VerticalAlignment = VerticalAlignment.Center;
                if (week > DateTime.Now)
                {
                    t.Mood = InfoDisplayControl.Moods.Bored;
                    t.Header = "Okres jest w przyszłości";
                    t.Body = "Spróbuj ponownie, gdy lekcje faktycznie nastąpią...";
                }
                else
                {
                    t.Mood = InfoDisplayControl.Moods.Sad;
                    t.Header = "Brak wpisów na ten okres";
                    t.Body = "Spróbuj ponownie później lub wybierz inny okres";
                }
                St.Children.Add(t);
                r = t;
            }
        }
        FrameworkElement r;

        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
