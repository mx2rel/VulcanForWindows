using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using Vulcanova.Features.Grades;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VulcanForWindows
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {

        public MainWindow()
        {
            this.InitializeComponent();
            rootFrame.Navigate(typeof(GradesPage));
        }

        private void NavigationChangedPage(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            //if (args.IsSettingsSelected)
            //{
            //    contentFrame.Navigate(typeof(SampleSettingsPage));
            //}
            //else
            //{
            var selectedItem = (Microsoft.UI.Xaml.Controls.NavigationViewItem)args.SelectedItem;
            if (selectedItem != null)
            {
                string selectedItemTag = ((string)selectedItem.Tag);
                //sender.Header = "Sample Page " + selectedItemTag.Substring(selectedItemTag.Length - 1);
                string pageName = selectedItemTag;
                Type pageType = Type.GetType("VulcanForWindows." +pageName);
                rootFrame.Navigate(pageType);
            }
            //}
        }
    }
}
