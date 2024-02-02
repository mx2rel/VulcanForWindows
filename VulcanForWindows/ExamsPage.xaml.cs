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
    public sealed partial class ExamsPage : Page, INotifyPropertyChanged
    {

        public IDictionary<DateTime, NewResponseEnvelope<Exam>> exams { get; set; } = new Dictionary<DateTime, NewResponseEnvelope<Exam>>();
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

        public Exam selectedExam { get; set; }
        public ObservableCollection<MonthExamsPair> display { get; set; } = new ObservableCollection<MonthExamsPair>();
        public bool allowLoadButtons { get; set; } = true;
        public void LoadBefore()
        {
            var acc = new AccountRepository().GetActiveAccountAsync();

            From = From.AddMonths(-1);
            (var start, var end) = acc.GetSchoolYearDuration();
            From = new DateTime( Math.Clamp(From.Ticks, start.Ticks, end.Ticks));
            UpdateDisplay();
        }
        public void LoadAfter()
        {
            var acc = new AccountRepository().GetActiveAccountAsync();

            To = To.AddMonths(1);
            (var start, var end) = acc.GetSchoolYearDuration();
            From = new DateTime(Math.Clamp(From.Ticks, start.Ticks, end.Ticks));
            UpdateDisplay();
        }

        public async void UpdateDisplay()
        {
            allowLoadButtons = false;
            OnPropertyChanged(nameof(allowLoadButtons));
            //start of month
            var v = await Load(From, To);

            display.ReplaceAll(v.GroupBy(r => r.Deadline.ToString("MM/yy"))
                .Where(r => r.ToList().Count > 0).
                Select(r => new MonthExamsPair(r.FirstOrDefault().Deadline, r.ToArray())).OrderBy(r => r.month));

            allowLoadButtons = true;
            OnPropertyChanged(nameof(allowLoadButtons));
            if (selectedExam == null)
            {
                selectedExam = display.SelectMany(r => r.exams).Where(r => !r.IsInPast()).OrderBy(r => r.Deadline).First();
                OnPropertyChanged(nameof(selectedExam));
            }
        }
        public async Task<Exam[]> Load(DateTime from, DateTime to)
        {
            var acc = new AccountRepository().GetActiveAccountAsync();
            return (await new ExamsService().GetExamsByDateRange(acc, from, to, true, true)).entries.ToArray();

        }

        public ExamsPage()
        {
            this.InitializeComponent();
            From = DateTime.Now;
            To = DateTime.Now.AddMonths(1);
            UpdateDisplay();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void BeforeButton(object sender, RoutedEventArgs e) => LoadBefore();
        private void AfterButton(object sender, RoutedEventArgs e) => LoadAfter();

        private void SelectionChanged(ItemsView sender, ItemsViewSelectionChangedEventArgs args)
        {
            selectedExam = sender.SelectedItem as Exam;
            OnPropertyChanged(nameof(selectedExam));
        }
    }

    public class MonthExamsPair
    {
        public MonthExamsPair(DateTime m, Exam[] e)
        {
            exams = new ObservableCollection<Exam>(e.OrderBy(r => r.Deadline).ToArray());
            month = m;
        }
        public DateTime month { get; set; }
        public ObservableCollection<Exam> exams { get; set; } = new ObservableCollection<Exam>();
    }
}
