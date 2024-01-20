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
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using VulcanTest.Vulcan;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VulcanForWindows.UserControls.Settings
{
    public sealed partial class ReminderSettings : UserControl, INotifyPropertyChanged
    {
        public bool allowNewEntries => Entries.Count <= 6;
        public ObservableCollection<ReminderSettingEntry> Entries { get; set; } = new ObservableCollection<ReminderSettingEntry>();

        public ReminderSettings()
        {
            Entries.ReplaceAll(Preferences.Get<ReminderSettingEntry[]>("ReminderSettings", new ReminderSettingEntry[]
            {
                new ReminderSettingEntry(new TimeSpan(16,0,0), 1),
                new ReminderSettingEntry(new TimeSpan(18,0,0), 3),
            }));
            this.InitializeComponent();
            PropertyChanged += ReminderSettingEntryChanged;
        }

        public void Save()
        {
            Preferences.Set<ReminderSettingEntry[]>("ReminderSettings", Entries.OrderBy(r=>r.daysPrior + r.hour.TotalDays).ToArray());
        }

        private void NewEntry(object sender, RoutedEventArgs e)
        {
            Entries.Add(new ReminderSettingEntry(1));
            Entries.Last().PropertyChanged += ReminderSettingEntryChanged;
                OnPropertyChanged(nameof(allowNewEntries));
        }

        private void RemoveEntry(object sender, RoutedEventArgs e)
        {
            ReminderSettingEntry entryToRemove = (sender as FrameworkElement)?.DataContext as ReminderSettingEntry;

            if (entryToRemove != null)
            {
                Entries.Remove(entryToRemove);
                OnPropertyChanged(nameof(allowNewEntries));
            }
        }
        private void ReminderSettingEntryChanged(object sender, PropertyChangedEventArgs e) => Save();

        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ReminderSettingEntry : INotifyPropertyChanged
    {
        public ReminderSettingEntry()
        {
            _hour = new TimeSpan(18, 0, 0);
            _daysPrior = 1;
        }
        public ReminderSettingEntry(int daysPrior)
        {
            _hour = new TimeSpan(18,0,0);
            _daysPrior = daysPrior;
        }
        public ReminderSettingEntry(TimeSpan hour, int daysPrior)
        {
            _hour = hour;
            _daysPrior = daysPrior;
        }

        public TimeSpan hour
        {
            get => _hour;
            set
            {
                if (_hour == value) return;

                _hour = value;
                OnPropertyChanged(nameof(hour));
            }
        }
        TimeSpan _hour;
        public int daysPrior
        {
            get => _daysPrior;
            set
            {
                if (_daysPrior == value) return;

                _daysPrior = value;
                OnPropertyChanged(nameof(daysPrior));
                OnPropertyChanged(nameof(daysText));
            }
        }
        int _daysPrior;

        [JsonIgnore]
        public string daysText { get => ((daysPrior == 1) ? "dzień" : "dni"); }

        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
