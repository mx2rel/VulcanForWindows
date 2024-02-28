using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    public sealed partial class MainWindow : Window, INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        public static MainWindow Instance;
        public static DateTime lastLaunch;
        public bool IsOtherWindowOpen
        {
            get => IsOtherWindowOpenVisibility == Visibility.Visible;
            set
            {
                IsOtherWindowOpenVisibility = value ? Visibility.Visible : Visibility.Collapsed;
                OnPropertyChanged(nameof(IsOtherWindowOpenVisibility));
            }
        }
        public Visibility IsOtherWindowOpenVisibility = Visibility.Collapsed;
        public MainWindow()
        {
            this.InitializeComponent();
            Instance = this;
            isLoggedIn = (new AccountRepository().GetActiveAccountAsync() != null);
            if (!isLoggedIn)
            {
                nvSample.Visibility = Visibility.Collapsed;
                rootFrame.Navigate(typeof(LoginPage));

            }
            else
            {
                LoadMainPage();
            }
            Preferences.TryGet<DateTime>("lastLaunch", out lastLaunch);
            Preferences.Set<DateTime>("lastLaunch", DateTime.Now);


            
            Preferences.Set<int>("timesLaunched", Preferences.Get<int>("timesLaunched", 0)+1);
        }
        bool isLoggedIn = false;
        public void Logout()
        {
            isLoggedIn = false;
            nvSample.Visibility = Visibility.Collapsed;
            rootFrame.Navigate(typeof(LoginPage));
        }
        public void LoadMainPage()
        {
            nvSample.Visibility = Visibility.Visible;
            history = new List<Type>();
            history.Add(typeof(MainWindow));
            new AccountSyncService().SyncAccountsIfRequiredAsync();
            rootFrame.Navigate(typeof(MainPanelPage));
        }

        public static void NavigateTo(string tag)
        {
            Type t = Type.GetType("VulcanForWindows." + tag);
            if (Instance.rootFrame.CurrentSourcePageType != t)
            {
                Instance.NavigateTo(t);
            }
        }

        private void NavigationChangedPage(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.IsSettingsSelected)
            {

                if (rootFrame.CurrentSourcePageType != typeof(SettingsPage))
                    NavigateTo(typeof(SettingsPage));
            }
            else
            {
                var selectedItem = (Microsoft.UI.Xaml.Controls.NavigationViewItem)args.SelectedItem;
                if (selectedItem != null)
                {
                    string selectedItemTag = ((string)selectedItem.Tag);
                    //sender.Header = "Sample Page " + selectedItemTag.Substring(selectedItemTag.Length - 1);
                    string pageName = selectedItemTag;
                    Type pageType = Type.GetType("VulcanForWindows." + pageName);

                    if (rootFrame.CurrentSourcePageType != pageType)
                        NavigateTo(pageType);
                }
            }
        }

        public void MoveBack()
        {
            history.RemoveAt(history.Count - 1);
            NavigateTo(history.Last(), false);
        }

        public List<Type> history = new List<Type>();

        private void NavigateTo(Type pageType, bool addToHistory = true)
        {
            rootFrame.Navigate(pageType);
            if (addToHistory)
                history.Add(pageType);
            UpdateBackButton();

            var d = Instance.nvSample.MenuItems.Where(r => (r as FrameworkElement).Tag as string == pageType.Name);
            if (d.Count() > 0)
                Instance.nvSample.SelectedItem = d.ElementAt(0);
        }

        void UpdateBackButton() =>
            nvSample.IsBackEnabled = history.Count > 2;

        private void BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            MoveBack();
            UpdateBackButton();
        }
    }
}
