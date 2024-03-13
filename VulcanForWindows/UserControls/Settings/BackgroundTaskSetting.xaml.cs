using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using VulcanTest.Vulcan;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VulcanForWindows.UserControls.Settings
{
    public sealed partial class BackgroundTaskSetting : UserControl
    {


        public static readonly DependencyProperty NumFieldVisibilityProperty =
            DependencyProperty.Register("NumFieldVisibility", typeof(Visibility), typeof(BackgroundTaskSetting), new PropertyMetadata(Visibility.Visible, NumFieldVisibility_Changed));

        public Visibility NumFieldVisibility
        {
            get => (Visibility)GetValue(NumFieldVisibilityProperty);
            set => SetValue(NumFieldVisibilityProperty, value);
        }

        private static void NumFieldVisibility_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is BackgroundTaskSetting control && e.NewValue is Visibility newValue)
            {
                control.numberbox.Visibility = newValue;
            }
        }


        public static readonly DependencyProperty PreferencesNameProperty =
            DependencyProperty.Register("PreferencesName", typeof(string), typeof(BackgroundTaskSetting), new PropertyMetadata(null, PreferencesName_Changed));

        public string PreferencesName
        {
            get => (string)GetValue(PreferencesNameProperty);
            set => SetValue(PreferencesNameProperty, value);
        }
        bool isDuringSetup = false;
        private static void PreferencesName_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is BackgroundTaskSetting control && e.NewValue is string newValue)
            {
                control.isDuringSetup = true;
                var value = Preferences.Get<int>(newValue, 10);
                control.numberbox.Value = (value != -1) ? value : 10;
                control.toggle.IsOn = value != -1;
                control.numberbox.IsEnabled = value != -1;
                control.isDuringSetup = false;

            }
        }


        public static readonly DependencyProperty DisplayNameProperty =
            DependencyProperty.Register("DisplayName", typeof(string), typeof(BackgroundTaskSetting), new PropertyMetadata(null, DisplayName_Changed));

        public string DisplayName
        {
            get => (string)GetValue(DisplayNameProperty);
            set => SetValue(DisplayNameProperty, value);
        }

        private static void DisplayName_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is BackgroundTaskSetting control && e.NewValue is string newValue)
            {
                control.DisplayNameText.Text = newValue;
            }
        }

        public BackgroundTaskSetting()
        {
            this.InitializeComponent();
        }

        private void ToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            numberbox.IsEnabled = toggle.IsOn;
            if (NumFieldVisibility == Visibility.Collapsed) numberbox.Value = 2;
            Set();
        }

        private void numberbox_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            numberbox.Value = Math.Round(numberbox.Value);
            Set();
        }

        void Set()
        {
            if (isDuringSetup) return;
            if (numberbox.Value == double.NaN || numberbox.Value == Math.Round( double.MinValue)) numberbox.Value = 10;
            var newValue = (int)((toggle.IsOn) ? (numberbox.Value) : -1);
            Preferences.Set<int>(PreferencesName, newValue);

            Sync(newValue);
        }

        public async void Sync(int value)
        {
            var client = new NamedPipeClientStream(".", "VulcanForWindowsInterAppSync", PipeDirection.Out);

            // Connect to the server asynchronously
            await client.ConnectAsync();

            // Write data to the server asynchronously
            var writer = new StreamWriter(client);
            await writer.WriteLineAsync($"{PreferencesName}|{value}");
            await writer.FlushAsync();
            Debug.WriteLine("Sync sent");
            client.Close();
        }
    }
}
