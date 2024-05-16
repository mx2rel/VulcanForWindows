using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using VulcanoidServerClient.Models;
using System.Collections.ObjectModel;
using VulcanoidServerClient.Services.Announcements;
using VulcanTest.Vulcan;
using VulcanForWindows.Classes;
using System.Diagnostics;
using Newtonsoft.Json;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VulcanForWindows.UserControls.Widgets
{
    public sealed partial class AnnouncementsWidget : UserControl
    {

        ObservableCollection<Announcement> Announcements = new ObservableCollection<Announcement>();
        public AnnouncementsWidget()
        {
            this.InitializeComponent();
            Load();
        }

        async void Load()
        {
            var all = await AnnouncementsService.GetAll();
            Debug.WriteLine(JsonConvert.SerializeObject(all));
            Announcements = new ObservableCollection<Announcement>(all);
            flipView.ItemsSource = Announcements;
        }

        private void ElementTapped(object sender, TappedRoutedEventArgs e)
        {
            if(sender is FrameworkElement fe)
                if(fe.DataContext is Announcement announcement)
                {
                    AnnouncementsManager.GetContentDialog(announcement, fe);
                }
        }
    }
}
