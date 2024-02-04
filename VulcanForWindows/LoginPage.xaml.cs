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
using Vulcanova.Core.Uonet;
using Vulcanova.Features.Auth;
using Vulcanova.Uonet.Api.Common;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VulcanForWindows
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LoginPage : Page
    {
        public LoginPage()
        {
            this.InitializeComponent();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LoadingBar.Visibility = Visibility.Visible;
                var apiclientfact = new ApiClientFactory();
                var authserv = new AuthenticationService(apiclientfact);
                var ip = new FebeInstanceUrlProviderDecorator(new InstanceUrlProvider());
                var instanceUrl = await ip.GetInstanceUrlAsync(token.Text, symbol.Text);
                var accounts = await authserv.AuthenticateAsync(token.Text, pin.Text, instanceUrl);
                accounts[0].IsActive = true;
                new AccountRepository().AddAccountsAsync(accounts);
                LoadingBar.Visibility = Visibility.Collapsed;
                MainWindow.Instance.LoadMainPage();
            }
            catch
            {
                LoadingBar.Visibility = Visibility.Collapsed;
                errorBar.IsOpen = true;
            }
        }
    }
}
