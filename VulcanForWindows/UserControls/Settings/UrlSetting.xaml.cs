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
using VulcanForWindows.Preferences;
using System.Diagnostics;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VulcanForWindows.UserControls.Settings
{
    public sealed partial class UrlSetting : UserControl
    {


        public static readonly DependencyProperty DisplayWarningProperty =
            DependencyProperty.Register("DisplayWarning", typeof(bool), typeof(UrlSetting), new PropertyMetadata(false, DisplayWarning_Changed));

        public bool DisplayWarning
        {
            get => (bool)GetValue(DisplayWarningProperty);
            set => SetValue(DisplayWarningProperty, value);
        }

        private static void DisplayWarning_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UrlSetting control && e.NewValue is bool newValue)
            {
                control.InfoIcon.Visibility = newValue.ToVisibility();
            }
        }


        public static readonly DependencyProperty DisplayProperty =
            DependencyProperty.Register("Display", typeof(string), typeof(UrlSetting), new PropertyMetadata(null, Display_Changed));

        public string Display
        {
            get => (string)GetValue(DisplayProperty);
            set => SetValue(DisplayProperty, value);
        }

        private static void Display_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UrlSetting control && e.NewValue is string newValue)
            {
                // TODO: Implement your logic here
            }
        }


        public static readonly DependencyProperty SaveIdProperty =
            DependencyProperty.Register("SaveId", typeof(string), typeof(UrlSetting), new PropertyMetadata(null, SaveId_Changed));

        public string SaveId
        {
            get => (string)GetValue(SaveIdProperty);
            set => SetValue(SaveIdProperty, value);
        }

        private static void SaveId_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UrlSetting control && e.NewValue is string newValue)
            {
                control.UrlInput.Text = PreferencesManager.Get<string>("Settings", newValue, string.Empty);
            }
        }



        public UrlSetting()
        {
            this.InitializeComponent();
        }

        private void UrlUpdated(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(UrlInput.Text) || IsValidHttpsUrl(UrlInput.Text))
                PreferencesManager.Set("Settings", SaveId, UrlInput.Text);
        }

        static bool IsValidHttpsUrl(string url)
        {
            if (Uri.TryCreate(url, UriKind.Absolute, out Uri result))
            {
                return result.Scheme == Uri.UriSchemeHttps || result.Scheme == Uri.UriSchemeHttp;
            }

            return false;
        }

        private void FontIcon_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            TeachingTip.IsOpen = true;
        }

        private void FontIcon_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            TeachingTip.IsOpen = false;

        }
    }
}
