using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    public sealed partial class MonthsAttendanceControl : UserControl
    {
        public MonthsAttendanceControl()
        {
            this.InitializeComponent();
            var l = new AccountRepository().GetActiveAccountAsync().GetSchoolYearDuration();
            from = new DateTime(2024, 1, 1);
            to = new DateTime(2024, 3, 31);
            for (int i = 1; i <= 3; i++)
            {
                calendars.Add(new DateTime(2024,i,1), RenderCalendar(i, 2024));
            }
            SpawnEntries();
        }

        DateTime from;
        DateTime to;
        IDictionary<DateTime, Grid> calendars = new Dictionary<DateTime, Grid>();

        Grid RenderCalendar(int month, int year = 2024)
        {
            DateTime firstDayOfMonth = new DateTime(year, month, 1);

            var sp = new StackPanel();
            sp.VerticalAlignment = VerticalAlignment.Center;
            sp.Width = 40 * 7;
            sp.Spacing = 10;
            var monthText = new TextBlock();
            monthText.Text = firstDayOfMonth.ToString("MMMM yy");
            monthText.TextAlignment = TextAlignment.Center;
            monthText.FontWeight = new Windows.UI.Text.FontWeight() { Weight = 500 };
            monthText.FontSize = 22;
            sp.Children.Add(monthText);
            var grid = new Grid();
            for (int i = 0; i < 7; i++)
            {
                var column = new ColumnDefinition();
                column.Width = new GridLength(40);
                grid.ColumnDefinitions.Add(column);
            }
            for (int i = 0; i < 6; i++)
            {
                var row = new RowDefinition();
                row.Height = new GridLength(40);
                grid.RowDefinitions.Add(row);
            }

            DateTime lastDayOfMonth = new DateTime(year, month, DateTime.DaysInMonth(2024, month));

            for (DateTime i = firstDayOfMonth.AddDays(-GetDayOfWeek(firstDayOfMonth));
                (i.Month <= firstDayOfMonth.Month || (i.Month == month + 1 && (7 - GetDayOfWeek(lastDayOfMonth)) > i.Day))
                ; i = i.AddDays(1))
            {
                var v = new Grid();
                var text = new TextBlock();
                text.VerticalAlignment = VerticalAlignment.Center;
                text.Text = i.Day.ToString();
                text.TextAlignment = TextAlignment.Center;
                if(i.Month == month)
                {
                    text.FontWeight = new Windows.UI.Text.FontWeight() { Weight = 500 };
                }
                else
                {
                    text.Foreground = new SolidColorBrush(Microsoft.UI.Colors.Gray);
                }
                v.Children.Add(text);
                Grid.SetColumn(v, GetDayOfWeek(i));
                Grid.SetRow(v, (i.Month == month) ? ((int)Math.Floor((i.Day + GetDayOfWeek(firstDayOfMonth) - 1) / 7f)) : ((i.Month == month - 1) ? 0 : ((int)Math.Floor((lastDayOfMonth.Day + GetDayOfWeek(firstDayOfMonth) - 1) / 7f))));
                grid.Children.Add(v);
            }

            sp.Children.Add(grid);
            container.Items.Add(sp);
            return grid;
        }

        int GetDayOfWeek(DateTime t)
        {
            var i = (int)t.DayOfWeek;
            if (i == 0) i = 7;
            else i--;
            return i;
        }

        public async void SpawnEntries()
        {
            Vulcan.NewResponseEnvelope<Lesson> l = new Vulcan.NewResponseEnvelope<Lesson>();
            await new LessonsService().GetLessonsForRange(new AccountRepository().GetActiveAccountAsync(), from, to, l, false, true);

            IEnumerable<(DateTime Key, bool Late, bool JustifiedLate, bool Absence, bool JustifiedAbsence)> entries = l.Entries.GroupBy(r => r.Date).Select(r => (r.Key,
            r.ToArray().Where(r => r.PresenceType.Late && !r.PresenceType.AbsenceJustified).Count() > 0,
            r.ToArray().Where(r => r.PresenceType.Late && r.PresenceType.AbsenceJustified).Count() > 0,
            r.ToArray().Where(r => r.PresenceType.Absence && !r.PresenceType.AbsenceJustified && !r.PresenceType.LegalAbsence).Count() > 0,
            r.ToArray().Where(r => r.PresenceType.AbsenceJustified || r.PresenceType.LegalAbsence).Count() > 0
            ));
            foreach(var v in entries)
            {
                var firstDayOfMonth = new DateTime(v.Key.Year, v.Key.Month, 1);
                if (calendars.TryGetValue(firstDayOfMonth, out var gr))
                {
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
                            row.Height = new GridLength(30/rowsMet);
                        else
                            row.Height = new GridLength(0);
                        g.RowDefinitions.Add(row);
                    }
                    Grid.SetRow(g, (int)Math.Floor((v.Key.Day + GetDayOfWeek(firstDayOfMonth) - 1) / 7f));
                    Grid.SetColumn(g,GetDayOfWeek(v.Key));

                    if (v.Late)
                    {
                        var n = new Border();
                        n.VerticalAlignment = VerticalAlignment.Stretch;
                        n.Background = new SolidColorBrush(Microsoft.UI.Colors.Yellow);
                        ToolTipService.SetToolTip(n, "Spóźnienie");

                        g.Children.Add(n);

                    }
                    if (v.JustifiedLate)
                    {
                        var n = new Border();
                        n.VerticalAlignment = VerticalAlignment.Stretch;
                        n.Background = new SolidColorBrush(Microsoft.UI.Colors.DarkBlue);
                        Grid.SetRow(n, 1);

                        ToolTipService.SetToolTip(n, "Spóźnienie usprawiedliwione");
                        g.Children.Add(n);
                    }
                    if (v.Absence)
                    {
                        var n = new Border();
                        n.VerticalAlignment = VerticalAlignment.Stretch;
                        n.Background = new SolidColorBrush(Microsoft.UI.Colors.Red);
                        Grid.SetRow(n, 2);
                        ToolTipService.SetToolTip(n, "Nieobecność");
                        g.Children.Add(n);

                    }
                    if (v.JustifiedAbsence)
                    {
                        var n = new Border();
                        n.VerticalAlignment = VerticalAlignment.Stretch;
                        n.Background = new SolidColorBrush(Microsoft.UI.Colors.MediumPurple);
                        Grid.SetRow(n, 3);
                        ToolTipService.SetToolTip(n, "Nieobecność usprawiedliwiona");
                        g.Children.Add(n);
                    }

                    gr.Children.Insert(0,g);
                }
            }

        }
    }
}
