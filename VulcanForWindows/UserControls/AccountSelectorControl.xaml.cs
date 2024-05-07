using DevExpress.Data.Extensions;
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
using Vulcanova.Features.Auth;
using Vulcanova.Features.Auth.Accounts;
using VulcanTest.Vulcan;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VulcanForWindows.UserControls
{
    public sealed partial class AccountSelectorControl : UserControl
    {

        public ObservableCollection<Account> accounts = new ObservableCollection<Account>();
        public AccountSelectorControl()
        {
            this.InitializeComponent();
            UpdateAccounts();
        }

        private void AddAccount(object sender, RoutedEventArgs e)
        {
            var w = new Window();
            w.Content = new LoginPage();
            w.Title = "Dodaj nowe konto";
            w.Activate();
            w.Closed += W_Closed;
        }

        private void W_Closed(object sender, WindowEventArgs args)
        {
            UpdateAccounts();
        }

        private void SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ListView listView)
            {
                if (listView.SelectedItem != null)
                {
                    var selectedAccount = listView.SelectedItem as Account;
                    if (new AccountRepository().SetActiveByPupilId(selectedAccount.Pupil.Id))
                    {
                        UpdateAccounts();
                        MainWindow.Instance.LoadMainPage();
                    }
                }
            }
            
        }

        public void UpdateAccounts()
        {
            accounts.ReplaceAll(new AccountRepository().GetAccounts());

            list.SelectedIndex = accounts.FindIndex(r => r.IsActive);

        }
    }
}
