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
using Vulcanova.Features.Attendance;
using Vulcanova.Features.Auth;
using Vulcanova.Features.Shared;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VulcanForWindows.UserControls
{
    public sealed partial class YearAttendanceControl : UserControl
    {
        public YearAttendanceControl()
        {
            this.InitializeComponent();
            SpawnYear();
            SpawnEntries();
        }


        public void SpawnYear()
        {
            var yearDuration = new AccountRepository().GetActiveAccountAsync().GetSchoolYearDuration();

            for (int i = 0; i < 7; i++)
            {
                var column = new ColumnDefinition();
                column.Width = new GridLength(35);
                Container.ColumnDefinitions.Add(column);
            }

            var mColumn = new ColumnDefinition();
            mColumn.Width = new GridLength(100);
            Container.ColumnDefinitions.Add(mColumn);
            DateTime endOfTheMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(1).AddDays(-1);

            for (int i = 0; i < Math.Ceiling((endOfTheMonth - yearDuration.Start).TotalDays / 7d); i++)
            {
                var row = new RowDefinition();
                row.Height = new GridLength(35);
                Container.RowDefinitions.Add(row);
            }

            for (DateTime date = yearDuration.Start; date <= endOfTheMonth; date = date.AddDays(1))
            {
                var position = GetPosForDate(date, yearDuration.Start);
                var text = new TextBlock();
                text.VerticalAlignment = VerticalAlignment.Center;
                text.Text = date.Day.ToString();
                text.TextAlignment = TextAlignment.Center;
                if (date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday && date.Date <= DateTime.Now.Date)
                {
                    text.FontWeight = new Windows.UI.Text.FontWeight() { Weight = 500 };
                }
                else text.Foreground = new SolidColorBrush(Microsoft.UI.Colors.LightGray);

                if (date.Date > DateTime.Now.Date) text.FontStyle = Windows.UI.Text.FontStyle.Italic;

                Container.Children.Add(text);
                Grid.SetColumn(text, position.Column);
                Grid.SetRow(text, position.Row);

                if (date.Day == 1)
                {
                    var monthText = new TextBlock();
                    monthText.VerticalAlignment = VerticalAlignment.Center;
                    monthText.Text = date.ToString("MMM yy");
                    monthText.FontWeight = new Windows.UI.Text.FontWeight() { Weight = 700 };

                    Container.Children.Add(monthText);

                    Grid.SetColumn(monthText, 8);
                    Grid.SetRow(monthText, position.Row);
                }
            }
        }


        public (int Column, int Row) GetPosForDate(DateTime date, DateTime start)
        {
            int dayFromStart = (int)Math.Floor((date - start).TotalDays);
            return (GetDayOfWeek(date), (int)(Math.Floor((GetDayOfWeek(start) + dayFromStart) / 7d)));
        }

        public async void SpawnEntries()
        {
            var yearDuration = new AccountRepository().GetActiveAccountAsync().GetSchoolYearDuration();
            Vulcan.NewResponseEnvelope<Lesson> l = new Vulcan.NewResponseEnvelope<Lesson>();
            await new LessonsService().GetLessonsForRange(new AccountRepository().GetActiveAccountAsync(), yearDuration.Start, DateTime.Today, l, false, true);

            IEnumerable<(DateTime Key, int LateCount, int JustifiedLateCount, int AbsenceCount, int JustifiedAbsenceCount)> entriesCount = l.Entries.Where(r => r.PresenceType != null).GroupBy(r => r.Date).Select(r => (r.Key,
            r.ToArray().Count(r => r.PresenceType.Late && !r.PresenceType.AbsenceJustified),
            r.ToArray().Count(r => r.PresenceType.Late && r.PresenceType.AbsenceJustified),
            r.ToArray().Count(r => r.PresenceType.Absence && !r.PresenceType.AbsenceJustified && !r.PresenceType.LegalAbsence),
            r.ToArray().Count(r => r.PresenceType.AbsenceJustified || r.PresenceType.LegalAbsence)
            ));

            // Create another var with bools
            IEnumerable<(DateTime Key, bool Late, bool JustifiedLate, bool Absence, bool JustifiedAbsence)> entries = entriesCount.Select(entry => (entry.Key,
                        entry.LateCount > 0,
                        entry.JustifiedLateCount > 0,
                        entry.AbsenceCount > 0,
                        entry.JustifiedAbsenceCount > 0
                        ));

            DateTime endOfTheMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(1).AddDays(-1);

            for (DateTime date = yearDuration.Start.AddDays(1); date <= endOfTheMonth; date = date.AddMonths(1))
            {
                var position = GetPosForDate(date, yearDuration.Start);
                var desiredMonthId = date.ToString("MM yy");
                var e = l.entries.Where(r => r.Date.ToString("MM yy") == desiredMonthId).Where(r => r.CalculatePresence);
                var percent = ((float)e.Where(r => r.PresenceType != null).Where(r => !r.PresenceType.Absence).Count() / (float)e.Count()) * 100;

                var monthText = new TextBlock();
                monthText.VerticalAlignment = VerticalAlignment.Top;
                monthText.Text = percent.ToString("0.00") + "%";
                monthText.FontWeight = new Windows.UI.Text.FontWeight() { Weight = 600 };
                monthText.Foreground = new SolidColorBrush(Microsoft.UI.Colors.Gray);

                Container.Children.Add(monthText);

                Grid.SetColumn(monthText, 8);
                Grid.SetRow(monthText, position.Row + 1);
            }

            foreach (var v in entries)
            {
                var firstDayOfMonth = new DateTime(v.Key.Year, v.Key.Month, 1);
                var gr = Container;
                var g = new Grid();
                g.CornerRadius = new CornerRadius(3);
                g.Height = 30;
                g.Width = 30;
                int rowsMet = (v.Late ? 1 : 0) + (v.JustifiedLate ? 1 : 0) + (v.JustifiedAbsence ? 1 : 0) + (v.Absence ? 1 : 0);
                if (rowsMet == 0) continue;
                for (int i = 0; i < 4; i++)
                {
                    var row = new RowDefinition();
                    if ((i == 0 && v.Late) || (i == 1 && v.JustifiedLate) || (i == 2 && v.Absence) || (i == 3 && v.JustifiedAbsence))
                        row.Height = new GridLength(30 / rowsMet);
                    else
                        row.Height = new GridLength(0);
                    g.RowDefinitions.Add(row);
                }
                var position = GetPosForDate(v.Key, yearDuration.Start);
                Grid.SetRow(g, position.Row);
                Grid.SetColumn(g, position.Column);

                List<string> s = new List<string>();

                var counts = entriesCount.Where(r => r.Key == v.Key).First();
                if (v.Late)
                {
                    var n = new Border();
                    n.VerticalAlignment = VerticalAlignment.Stretch;
                    n.Background = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb((byte)255, (byte)255, (byte)145, (byte)0));

                    s.Add($"({counts.LateCount}) Spóźnienie");
                    g.Children.Add(n);

                }
                if (v.JustifiedLate)
                {
                    var n = new Border();
                    n.VerticalAlignment = VerticalAlignment.Stretch;
                    n.Background = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb((byte)255, (byte)255, (byte)171, (byte)62));
                    Grid.SetRow(n, 1);

                    s.Add($"({counts.JustifiedLateCount}) Spóźnienie uspr.");
                    g.Children.Add(n);
                }
                if (v.Absence)
                {
                    var n = new Border();
                    n.VerticalAlignment = VerticalAlignment.Stretch;
                    n.Background = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb((byte)255, (byte)165, (byte)0, (byte)0));
                    Grid.SetRow(n, 2);

                    s.Add($"({counts.AbsenceCount}) Nieobecność");
                    g.Children.Add(n);

                }
                if (v.JustifiedAbsence)
                {
                    var n = new Border();
                    n.VerticalAlignment = VerticalAlignment.Stretch;
                    n.Background = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb((byte)255, (byte)210, (byte)73, (byte)0));
                    Grid.SetRow(n, 3);

                    s.Add($"({counts.JustifiedAbsenceCount}) Nieobecność uspr.");
                    g.Children.Add(n);
                }

                string show = string.Join("\n", s);

                foreach (var ch in gr.Children.Where(r => Grid.GetRow(r as FrameworkElement) == position.Row).Where(r => Grid.GetColumn(r as FrameworkElement) == GetDayOfWeek(v.Key)))
                {
                    ToolTipService.SetToolTip(ch, show);
                }

                gr.Children.Insert(0, g);
            }

        }

        int GetDayOfWeek(DateTime t)
        {
            var i = (int)t.DayOfWeek;
            if (i == 0) i = 6;
            else i--;
            return i;
        }
    }
}
