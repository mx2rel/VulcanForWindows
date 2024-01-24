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

        public ObservableCollection<Exam> display { get; set; } = new ObservableCollection<Exam>();
        public bool allowLoadButtons { get; set; } = true;
        public void LoadBefore()
        {
            From = From.AddMonths(-1);
            LoadMonth(From, true, true);
        }
        public void LoadAfter()
        {
            To = To.AddMonths(1);
            LoadMonth(To, true, true);
        }

        public async void LoadMonth(DateTime month, bool loadAround = true, bool insertAtStart = false)
        {
            allowLoadButtons = false;
            OnPropertyChanged(nameof(allowLoadButtons));
            //start of month
            month = month.Date;
            var v = await GetMonth(month, loadAround);

            if (insertAtStart) display.ReplaceAll(v.entries.Concat(display.ToList()));
            else display.ReplaceAll(display.ToList().Concat(v.entries));
            allowLoadButtons = true;    
            OnPropertyChanged(nameof(allowLoadButtons));
        }

        public async Task<NewResponseEnvelope<Exam>> GetMonth(DateTime month, bool loadAround = true)
        {
            //start of month
            month = month.Date;
            if (!exams.ContainsKey(month))
            {
                var acc = new AccountRepository().GetActiveAccountAsync();
                exams.Add(month, await new ExamsService().GetExamsByDateRange(acc, month, month.AddMonths(1),true,true));
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
            From = DateTime.Now;
            To = DateTime.Now;
            LoadMonth(From,true,true);
        }
        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void BeforeButton(object sender, RoutedEventArgs e) => LoadBefore();
        private void AfterButton(object sender, RoutedEventArgs e) => LoadAfter();
    }
}
