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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using VulcanForWindows.Vulcan;
using Vulcanova.Features.Auth;
using Vulcanova.Features.Exams;
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
    public sealed partial class ExamsPage : Page
    {

        public IDictionary<DateTime, NewResponseEnvelope<Exam>> exams { get; set; } = new Dictionary<DateTime, NewResponseEnvelope<Exam>>();
        DateTime _currentMonth;
        public DateTime currentMonth
        {
            get => _currentMonth;
            set
            {
                _currentMonth = value.Date;
                _currentMonth = _currentMonth.AddDays(-_currentMonth.Day + 1);
            }
        }

        public ObservableCollection<Exam> display { get; set; } = new ObservableCollection<Exam>();
        public async void ChangeMonth(DateTime month, bool loadAround = true)
        {
            //start of month
            month = month.Date;
            month = month.AddDays(-month.Day + 1);
            var v = await GetMonth(month, loadAround);
            if (exams.ContainsKey(currentMonth))
                exams[currentMonth].Updated -= ExamsPage_Updated;

            currentMonth = month;
            v.Updated += ExamsPage_Updated;
            display.ReplaceAll(v.Entries);
            Debug.WriteLine($"EPSet\n{JsonConvert.SerializeObject(v.Entries)}");

        }

        private void ExamsPage_Updated(object sender, IEnumerable<Exam> e)
        {
            Debug.WriteLine("EPUp");
            display.ReplaceAll(e);
        }

        public async Task<NewResponseEnvelope<Exam>> GetMonth(DateTime month, bool loadAround = true)
        {
            //start of month
            month = month.Date;
            month = month.AddDays(-month.Day + 1);
            if (!exams.ContainsKey(month))
            {
                var acc = new AccountRepository().GetActiveAccountAsync();
                exams.Add(month, await new ExamsService().GetExamsByDateRange(acc, month, month.AddMonths(1),false,true));
            }


            if (loadAround) LoadAround(month);
            return exams[month];
        }

        public async void LoadAround(DateTime month)
        {
            var acc = new AccountRepository().GetActiveAccountAsync();
            if (!exams.ContainsKey(month.AddMonths(-1)))
                exams.Add(month.AddMonths(-1), await new ExamsService().GetExamsByDateRange(acc, month.AddMonths(-1), month));
            if (!exams.ContainsKey(month.AddMonths(1)))
                exams.Add(month.AddMonths(1), await new ExamsService().GetExamsByDateRange(acc, month.AddMonths(1), month));
        }

        public ExamsPage()
        {
            this.InitializeComponent();
            ChangeMonth(DateTime.Now);
        }
    }
}
