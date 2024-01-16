using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
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
using VulcanForWindows.Vulcan.Grades;
using Vulcanova.Features.Attendance;
using Vulcanova.Features.Attendance.Report;
using Vulcanova.Features.Auth;
using Vulcanova.Features.Auth.Accounts;
using Vulcanova.Features.Grades;
using Vulcanova.Features.Messages;
using Vulcanova.Features.Timetable;
using VulcanTest.Vulcan;
using VulcanTest.Vulcan.Timetable;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VulcanForWindows
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPanelPage : Page, INotifyPropertyChanged
    {
        public GradesResponseEnvelope env { get; set; }
        public float PresentPercent { get; set; }
        public string PresentPercentDisplay { get => PresentPercent.ToString("0.00") + "%"; }

        public ObservableCollection<SubjectGrades> sg { get; set; }
        public NewResponseEnvelope<Lesson> att { get; set; }

        public IReadOnlyDictionary<DateTime, IReadOnlyCollection<TimetableListEntry>> lessons;
        public TimetableDay displayDay { get; set; }
        public bool displayDayLoaded { get; set; }
        public bool displayDayLoading { get => !displayDayLoaded; }

        public int UnjustifiedCount => (att != null) ? ((att.Entries.Count == 0) ? -1 :
            (att.Entries.Where(r => r.PresenceType != null).Where(r => r.PresenceType.Absence && (!r.PresenceType.AbsenceJustified && !r.PresenceType.LegalAbsence)).Count())) : -1;
        public int InProgressCount => (att != null) ? ((att.Entries.Count == 0) ? -1 :
            (att.Entries.Where(r => r.PresenceType != null).Where(r => r.PresenceType.Absence).Where(r => r.JustificationStatus != null)
            .Where(r => r.JustificationStatus == Vulcanova.Uonet.Api.Lessons.JustificationStatus.Requested).Count())) : -1;
        public ObservableCollection<Lesson> lastNieusprawiedliwione;
        public MainPanelPage()
        {
            att = new NewResponseEnvelope<Lesson>();
            lastNieusprawiedliwione = new ObservableCollection<Lesson>();
            sg = new ObservableCollection<SubjectGrades>();
            messages = new ObservableCollection<MessageViewModel>();
            Fetch();
            this.InitializeComponent();
        }

        public void Fetch()
        {
            var acc = new AccountRepository().GetActiveAccountAsync();
            FetchAttendance(acc);
            FetchGrades(acc);
            FetchTimetable(acc);
            FetchMessages(acc);
        }

        public NewResponseEnvelope<Message> messagesEnvelope { get; set; } = new NewResponseEnvelope<Message>();

        public ObservableCollection<MessageViewModel> messages { get; set; }
        public int MessagesCount => messages.Count;
        public int NewMessagesCount => messages.ToArray().NewCount();
        public bool messagesLoaded { get; set; }

        private async Task FetchMessages(Account acc)
        {
            NewResponseEnvelope<MessageBox> v = await new MessageBoxesService().GetMessageBoxesByAccountId(acc, true, true);
            messagesEnvelope = await
                new MessagesService().GetMessagesByBox(acc, v.entries.First().GlobalKey, Vulcanova.Uonet.Api.MessageBox.MessageBoxFolder.Received, true, true);
            messages.ReplaceAll(messagesEnvelope.entries.GroupBy(r => (r.DateRead == null) ? 0 : 1).Select(r=>r.OrderByDescending(r=>r.DateSent)).SelectMany(r=>r.Select(g=> new MessageViewModel(g))));
            messagesLoaded = true;
            OnPropertyChanged(nameof(messagesLoaded));
            OnPropertyChanged(nameof(MessagesCount));
            OnPropertyChanged(nameof(NewMessagesCount));
        }
        private async Task FetchGrades(Account acc)
        {
            env = await new GradesService().GetPeriodGrades(acc, acc.CurrentPeriod.Id);
            env.Updated += Env_Updated;
            Env_Updated(null, null);
        }

        private async Task FetchTimetable(Account acc)
        {
            Debug.WriteLine("\nLoading\n");

            var to = DateTime.Today.AddDays(1);
            if ((int)DateTime.Today.DayOfWeek >= 5)
                to = to.AddDays(2);

            lessons = await Timetable.FetchEntriesForRange(acc, DateTime.Today.AddDays(-1), to);
            DateTime dayToDisplay;

            //day to display logic
            if (lessons.TryGetValue(DateTime.Today, out var today))
            {
                if (today.OrderByDescending(r => r.Start).ElementAt(0).End < DateTime.Now)
                {
                    dayToDisplay = DateTime.Today.AddDays(1);
                }
                else
                    dayToDisplay = DateTime.Today;
            }
            else if (lessons.TryGetValue(DateTime.Today.AddDays(1), out var tommorow))
                dayToDisplay = DateTime.Today.AddDays(1);
            else dayToDisplay = DateTime.Today;

            if ((dayToDisplay == DateTime.Today.AddDays(1) && (int)DateTime.Today.DayOfWeek == 5) || (int)DateTime.Today.DayOfWeek >= 6)
                dayToDisplay = TimetableDayGrouper.GetStartOfWeek(DateTime.Today).AddDays(7);


            TimetableListEntry[] e = (lessons.Keys.Contains(dayToDisplay)) ? (lessons[dayToDisplay].ToArray()) : (new TimetableListEntry[0]);

            displayDay = (new TimetableDay(
                new KeyValuePair<DateTime, TimetableListEntry[]>(dayToDisplay, e)));


            displayDayLoaded = true;
            Debug.WriteLine("\nLoaded\n");
            OnPropertyChanged(nameof(displayDay));
            OnPropertyChanged(nameof(displayDayLoaded));
            OnPropertyChanged(nameof(displayDayLoading));
        }

        private async Task FetchAttendance(Account acc)
        {
            PresentPercent = await AttendanceReportService.GetPresencePercentage(acc);
            OnPropertyChanged(nameof(PresentPercent));
            OnPropertyChanged(nameof(PresentPercentDisplay));

            att.Updated += Att_Updated;
            await new LessonsService().GetLessonsForRange(acc, DateTime.Now.AddDays(-14), DateTime.Now, att);
            Att_Updated(null, null);
        }

        private void Att_Updated(object sender, IEnumerable<Lesson> e)
        {
            lastNieusprawiedliwione.ReplaceAll(
                att.Entries
                .Where(r => r.Date >= TimetableDayGrouper.GetStartOfWeek(DateTime.Now.AddDays(-7), false))
                .Where(r => r.PresenceType != null).Where(r => r.PresenceType.Absence).
                GroupBy(r => r.CanBeJustified).OrderBy(r => r.Key ? 0 : 1).Select(r => r.OrderByDescending(h => h.Date))
                .SelectMany(r => r));
            OnPropertyChanged(nameof(UnjustifiedCount));
            OnPropertyChanged(nameof(InProgressCount));
        }

        private void Env_Updated(object sender, IEnumerable<Grade> e)
        {
            sg.ReplaceAll(SubjectGrades.CreateRecent(env));
        }

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            //(e.ClickedItem as SubjectGrades)
            //TODO: LOAD SUBJECT
        }

        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void LoadPage(object sender, TappedRoutedEventArgs e)
        {
            string pageName = (sender as FrameworkElement).Tag as string;
            MainWindow.NavigateTo(pageName);
        }
    }
}
