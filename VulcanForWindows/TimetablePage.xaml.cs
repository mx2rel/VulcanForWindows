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
using VulcanForWindows.Classes;
using Windows.Foundation;
using Newtonsoft.Json;
using Windows.Foundation.Collections;
using System.Collections.ObjectModel;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VulcanForWindows
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class TimetablePage : Page
    {
        public DateTime Start { get; set; }

        ObservableCollection<TimetableEntry> appointments = new ObservableCollection<TimetableEntry>();
        public IEnumerable<TimetableEntry> Appointments { get { return appointments; } }

        public TimetablePage()
        {
            this.InitializeComponent();
            //System.Diagnostics.Debug.Write( JsonConvert.SerializeObject(SchedulerViewModel.Appointments)); 
            Start = DateTime.Today;
            appointments = new ObservableCollection<TimetableEntry>(TimetableEntry.Generate(RandomGenerator.GenerateRandomTimetable()));
        }
    }
}
