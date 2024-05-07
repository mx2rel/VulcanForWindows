using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using VulcanForWindows.Classes;
using VulcanForWindows.Vulcan;
using Vulcanova.Features.Auth;
using Vulcanova.Features.Exams;
using Vulcanova.Features.Homework;
using Vulcanova.Features.Shared;
using VulcanTest.Vulcan;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VulcanForWindows
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DeadlineablePage : Page, INotifyPropertyChanged
    {

        public IDictionary<DateTime, NewResponseEnvelope<Exam>> exams { get; set; } = new Dictionary<DateTime, NewResponseEnvelope<Exam>>();
        public IDictionary<DateTime, NewResponseEnvelope<Homework>> hws { get; set; } = new Dictionary<DateTime, NewResponseEnvelope<Homework>>();
        DateTime _from;
        public DateTime From
        {
            get => _from;
            set
            {
                _from = value.Date;
                _from = _from.AddDays(-_from.Day + 1);
            }
        }
        DateTime _to;
        public DateTime To
        {
            get => _to;
            set
            {
                _to = value.Date;
                _to = _to.AddDays(-_to.Day + 1);
                _to = _to.AddMonths(1);
            }
        }

        public Deadlineable selectedExam { get; set; }
        public ObservableCollection<MonthExamsPair> display { get; set; } = new ObservableCollection<MonthExamsPair>();
        public bool allowLoadButtons { get; set; } = true;
        public void LoadBefore()
        {
            var acc = new AccountRepository().GetActiveAccount();

            From = From.AddMonths(-1);
            (var start, var end) = acc.GetSchoolYearDuration();
            From = new DateTime(Math.Clamp(From.Ticks, start.Ticks, end.Ticks));
            UpdateDisplay();
        }
        public void LoadAfter()
        {
            var acc = new AccountRepository().GetActiveAccount();

            To = To.AddMonths(1);
            (var start, var end) = acc.GetSchoolYearDuration();
            From = new DateTime(Math.Clamp(From.Ticks, start.Ticks, end.Ticks));
            UpdateDisplay(true);
        }

        public async void UpdateDisplay(bool moveToEnd = false)
        {
            allowLoadButtons = false;
            OnPropertyChanged(nameof(allowLoadButtons));
            //start of month
            var v = await Load(From, To);
            var replaceW = v.GroupBy(r => r.Deadline.ToString("MM/yy"))
                .Where(r => r.ToList().Count > 0).
                Select(r => new MonthExamsPair(r.FirstOrDefault().Deadline, r.ToArray())).OrderBy(r => r.month);
            display.ReplaceAll(MonthExamsPair.AddMissingMonths(From, To, replaceW.ToArray()));

            allowLoadButtons = true;
            OnPropertyChanged(nameof(allowLoadButtons));
            if (selectedExam == null)
            {
                var avaible = display.SelectMany(r => r.exams).Where(r => !r.IsInPast).OrderBy(r => r.Deadline);
                if (avaible.Count() > 0)
                    selectedExam = avaible.First();
                OnPropertyChanged(nameof(selectedExam));
            }
            //if(moveToEnd) MoveToEnd(); TODO: execute after ui updates, so it actually moves to the end
        }
        public async Task<IDeadlineable[]> Load(DateTime from, DateTime to)
        {
            var acc = new AccountRepository().GetActiveAccount();
            var lexams = (await new ExamsService().GetExamsByDateRange(acc, from, to, true, true)).entries.ToArray();
            List<NewResponseEnvelope<Homework>> homeworkEnvelopes = new List<NewResponseEnvelope<Homework>>();
            foreach (var period in acc.PeriodsInRange(from, to))
                homeworkEnvelopes.Add(await new HomeworkService().GetHomework(acc, period.Id, true, true));
            return lexams.Select(r => r as IDeadlineable).Concat(homeworkEnvelopes.SelectMany(r => r.entries).Where(r => r.Deadline >= from && r.Deadline <= to).Select(r => r as IDeadlineable)).ToArray();

        }

        public DeadlineablePage()
        {
            this.InitializeComponent();
            From = DateTime.Now.AddMonths(-1);
            To = DateTime.Now.AddMonths(3);
            UpdateDisplay();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void BeforeButton(object sender, RoutedEventArgs e) => LoadBefore();
        private void AfterButton(object sender, RoutedEventArgs e)
        {
            LoadAfter();
        }

        private void MoveToEnd()
        {
            Panel contentPanel = scrollViewer.Content as Panel;

            if (contentPanel != null)
            {
                double maxHorizontalOffset = contentPanel.ActualWidth - scrollViewer.ActualWidth;

                if (maxHorizontalOffset > 0)
                {
                    scrollViewer.ChangeView(maxHorizontalOffset, 0, 1);
                }
            }
        }

        private void SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedExam = (sender as ListView).SelectedItem as Deadlineable;
            OnPropertyChanged(nameof(selectedExam));
        }
    }

    public class MonthExamsPair
    {
        public MonthExamsPair(DateTime m, IDeadlineable[] e)
        {
            month = m.AddDays(-m.Day + 1);

            exams = new ObservableCollection<Deadlineable>(e.OrderBy(r => r.Deadline).Select(r => r as IDeadlineable).Select(r => new Deadlineable(r)).ToArray());
        }
        public DateTime month { get; set; }
        public ObservableCollection<Deadlineable> exams { get; set; } = new ObservableCollection<Deadlineable>();

        public static IEnumerable<MonthExamsPair> AddMissingMonths(DateTime from, DateTime to, IEnumerable<MonthExamsPair> months)
        {
            var list = months.ToList();
            var foundM = list.GroupBy(r => r.month).Select(r => r.First().month).ToList();
            do
            {
                if (!foundM.Contains(from))
                    list.Add(new MonthExamsPair(from, new IDeadlineable[0]));
                from = from.AddMonths(1);

            } while (from < to);

            return list;
        }
    }
}
