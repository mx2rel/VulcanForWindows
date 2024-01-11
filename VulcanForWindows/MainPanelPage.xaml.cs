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
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using VulcanForWindows.Classes;
using VulcanForWindows.Vulcan;
using VulcanForWindows.Vulcan.Grades;
using Vulcanova.Features.Attendance;
using Vulcanova.Features.Auth;
using Vulcanova.Features.Auth.Accounts;
using Vulcanova.Features.Grades;
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
    public sealed partial class MainPanelPage : Page, INotifyPropertyChanged
    {
        public GradesResponseEnvelope env { get; set; }

        public ObservableCollection<SubjectGrades> sg { get; set; }
        public NewResponseEnvelope<Lesson> att { get; set; }

        public int UnjustifiedCount => (att != null) ? ((att.Entries.Count == 0) ? -1 :
            (att.Entries.Where(r => r.PresenceType != null).Where(r => r.PresenceType.Absence && (!r.PresenceType.AbsenceJustified && !r.PresenceType.LegalAbsence)).Count())) : -1;
        public int InProgressCount => (att != null) ? ((att.Entries.Count == 0) ? -1 :
            (att.Entries.Where(r => r.PresenceType != null).Where(r => r.PresenceType.Absence).Where(r => r.JustificationStatus != null)
            .Where(r=>r.JustificationStatus == Vulcanova.Uonet.Api.Lessons.JustificationStatus.Requested).Count())) : -1;
        public ObservableCollection<Lesson> lastNieusprawiedliwione;
        public MainPanelPage()
        {
            lastNieusprawiedliwione = new ObservableCollection<Lesson>();
            sg = new ObservableCollection<SubjectGrades>();
            Fetch();
            this.InitializeComponent();
        }

        public void Fetch()
        {
            var acc = new AccountRepository().GetActiveAccountAsync();
            FetchGrades(acc);
            FetchAttendance(acc);
        }

        private async Task FetchGrades(Account acc)
        {
            env = await new GradesService().GetPeriodGrades(acc, acc.CurrentPeriod.Id);
            env.Updated += Env_Updated;
        }

        private async Task FetchAttendance(Account acc)
        {
            att = await new LessonsService().GetLessonsByMonth(acc, DateTime.Now);
            att.Updated += Att_Updated;
        }

        private void Att_Updated(object sender, IEnumerable<Lesson> e)
        {
            lastNieusprawiedliwione.ReplaceAll(att.Entries.Where(r => r.PresenceType != null).Where(r => r.PresenceType.Absence).
                GroupBy(r => r.CanBeJustified).Select(r => r.OrderByDescending(h => h.Date)).SelectMany(r => r).Where(r => r.Date >= TimetableDayGrouper.GetStartOfWeek(DateTime.Now.AddDays(-7), false)));
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
    }
}
