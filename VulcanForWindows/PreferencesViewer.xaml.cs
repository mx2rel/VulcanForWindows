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
using VulcanForWindows.Preferences;
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
    public sealed partial class PreferencesViewer : Page
    {

        public void Clear()
        {
            VulcanForWindows.Preferences.PreferencesManager.Clear();
        }

        public ObservableCollection<Preference> Preferences { get; set; } = new ObservableCollection<Preference>();

        public PreferencesViewer()
        {
            Preferences.ReplaceAll(VulcanForWindows.Preferences.PreferencesManager.GetAllData().ToPreferences());

            this.InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e) => Clear();
    }


}
