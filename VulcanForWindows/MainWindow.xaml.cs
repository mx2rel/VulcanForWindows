using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Linq;
using Vulcanova.Features.Auth;
using VulcanTest.Vulcan;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VulcanForWindows
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {

        public static MainWindow Instance;

        public MainWindow()
        {
            this.InitializeComponent();
            new AccountSyncService().SyncAccountsIfRequiredAsync();
            rootFrame.Navigate(typeof(MainPanelPage));
            Instance = this;
        }

        public static void NavigateTo(string tag)
        {
            Type t = Type.GetType("VulcanForWindows." + tag);
            if (Instance.rootFrame.CurrentSourcePageType != t)
            {
                Instance.rootFrame.Navigate(t);
                var d = Instance.nvSample.MenuItems.Where(r => (r as FrameworkElement).Tag as string == tag).ElementAt(0);
                if (d != null)
                    Instance.nvSample.SelectedItem = d;
            }
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
                Type pageType = Type.GetType("VulcanForWindows." + pageName);

                if (rootFrame.CurrentSourcePageType != pageType)
                    rootFrame.Navigate(pageType);
            }
            //}
        }
    }
}
