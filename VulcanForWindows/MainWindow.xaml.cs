using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using VulcanForWindows.Preferences;
using VulcanForWindows.UserControls;
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

        public static bool REMOVE_ALL_SAVED_FILES_WITH_UPDATE = true;

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
            var f = new Flyout();
            f.Content = new AccountSelectorControl();
            changeAccountButton.Flyout = f;
            Instance = this;

            Logger.Log("Launch");

            if (REMOVE_ALL_SAVED_FILES_WITH_UPDATE)
            {
                var lastVer = PreferencesManager.Get<string>("lastVerLaunched");
                if (lastVer != AppWide.AppVersion && !string.IsNullOrEmpty(lastVer) && PreferencesManager.Get<int>("timesLaunched") > 0)
                {
                    PreferencesManager.WipeFolder();
                    ShowLogoutPopup();
                }
            }

            isLoggedIn = (new AccountRepository().GetActiveAccount() != null);
            if (!isLoggedIn)
            {
                nvSample.Visibility = Visibility.Collapsed;
                rootFrame.Navigate(typeof(LoginPage));

            }
            else
            {
                LoadMainPage();
            }



            PreferencesManager.TryGet<DateTime>("lastLaunch", out lastLaunch);
            PreferencesManager.Set<DateTime>("lastLaunch", DateTime.Now);
            PreferencesManager.Set<string>("lastVerLaunched", AppWide.AppVersion);

            PreferencesManager.Set<int>("timesLaunched", PreferencesManager.Get<int>("timesLaunched", 0) + 1);
        }

        private async void ShowLogoutPopup()
        {
            await Task.Delay(500);

            var popup = new ContentDialog();
            popup.Title = "Ta aktualizacja wymagała zresetowania zapisanych danych.";
            popup.Content = "Ze względów technicznych, wylogowaliśmy Cię ze wszystkich kont. Zaloguj się ponownie";
            popup.PrimaryButtonText = "Okej";
            //popup.XamlRoot = root.XamlRoot;
            //await popup.ShowAsync();
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
            nvSample.SelectedItem = null;
            nvSample.Visibility = Visibility.Visible;
            history = new List<Type>();
            history.Add(typeof(MainWindow));
            new AccountSyncService().SyncAccountsIfRequiredAsync();
            rootFrame.Navigate(typeof(MainPanelPage));
        }

        public static void NavigateTo(string tag)
        {
            if (string.IsNullOrEmpty(tag)) return;
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
                    if (selectedItemTag == null) return;
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
