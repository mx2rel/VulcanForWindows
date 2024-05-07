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
using Vulcanova.Features.Attendance.Report;
using Vulcanova.Features.Auth;
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
    public sealed partial class AttendanceReportPage : Page, INotifyPropertyChanged
    {

        public float PresentPercent { get; set; }
        public int PresentCount { get; set; }
        public int AbsentCount { get; set; }
        public int LateCount { get; set; }
        public string PresentPercentDisplay { get => PresentPercent.ToString("0.00") + "%"; }

        public ObservableCollection<AttendanceReport> reports { get; set; }

        public AttendanceReportPage()
        {
            reports = new ObservableCollection<AttendanceReport>();
            this.InitializeComponent();
            Fetch();
        }

        async void Fetch()
        {
            var acc = new AccountRepository().GetActiveAccount();
            (PresentPercent, PresentCount, LateCount, AbsentCount) = await AttendanceReportService.GetPresenceInfo(acc);

            OnPropertyChanged(nameof(PresentPercent));
            OnPropertyChanged(nameof(PresentPercentDisplay));
            reports.ReplaceAll(await AttendanceReportService.GetReports(acc));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
