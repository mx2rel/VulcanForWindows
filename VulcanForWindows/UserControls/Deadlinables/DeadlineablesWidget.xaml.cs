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
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using VulcanForWindows.Classes;
using VulcanForWindows.Vulcan;
using Vulcanova.Features.Auth;
using Vulcanova.Features.Exams;
using Vulcanova.Features.Homework;
using VulcanTest.Vulcan;
using Windows.Foundation;
using Windows.Foundation.Collections;
using System.Linq;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VulcanForWindows.UserControls.Deadlinables
{
    public sealed partial class DeadlineablesWidget : UserControl
    {
        public DeadlineablesWidget()
        {
            this.InitializeComponent();
            LoadDisplay();
        }
        async void LoadDisplay()
        {
            display.ReplaceAll((await Load(DateTime.Today, DateTime.Today.AddDays(28))).OrderBy(r=>r.Deadline).Select(r=>new Deadlineable(r)));

        }
        public ObservableCollection<Deadlineable> display { get; set; } = new ObservableCollection<Deadlineable>();

        public async Task<IDeadlineable[]> Load(DateTime from, DateTime to)
        {
            var acc = new AccountRepository().GetActiveAccount();
            var lexams = (await new ExamsService().GetExamsByDateRange(acc, from, to, true, true)).entries.ToArray();
            List<NewResponseEnvelope<Homework>> homeworkEnvelopes = new List<NewResponseEnvelope<Homework>>();
            foreach (var period in acc.PeriodsInRange(from, to))
                homeworkEnvelopes.Add(await new HomeworkService().GetHomework(acc, period.Id, true, true));
            return lexams.Select(r => r as IDeadlineable).Concat(homeworkEnvelopes.SelectMany(r => r.entries).Where(r => r.Deadline >= from && r.Deadline <= to).Select(r => r as IDeadlineable)).ToArray();

        }
    }
}
