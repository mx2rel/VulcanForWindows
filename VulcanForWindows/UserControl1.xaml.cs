using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using static Vulcanova.Features.Timetable.TimetableListEntry;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VulcanForWindows
{
    public sealed partial class UserControl1 : UserControl
    {

        public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register("Value", typeof(OverridableRefValue<string>), typeof(UserControl1), new PropertyMetadata(null, OnValueChanged));
        public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register("Title", typeof(string), typeof(UserControl1), new PropertyMetadata(null, TitleChanged));


        public OverridableRefValue<string> Value
        {
            get => (OverridableRefValue<string>)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }
        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public UserControl1()
        {
            this.InitializeComponent();
        }
        private static void TitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UserControl1 control && e.NewValue is string newValue)
                control.TitleText.Text = newValue;
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UserControl1 control && e.NewValue is OverridableRefValue<string> newValue)
            {
                // Update the ValueText
                if (string.IsNullOrEmpty(newValue.Override))
                {
                    control.ValueText.Foreground = new SolidColorBrush(Colors.White);
                }
                else
                {
                    control.ValueText.Foreground = new SolidColorBrush(ConvertHexToColor("#ffc400"));

                }
                string val = GetValue(newValue);
                if (val != null)
                    control.ValueText.Text = GetValue(newValue);
                control.Panel.Visibility = (val == null) ? Visibility.Collapsed : Visibility.Visible;
            }
        }
        public static Color ConvertHexToColor(string hex)
        {
            hex = hex.Remove(0, 1);
            byte a = hex.Length == 8 ? Byte.Parse(hex.Substring(0, 2), NumberStyles.HexNumber) : (byte)255;
            byte r = Byte.Parse(hex.Substring(hex.Length - 6, 2), NumberStyles.HexNumber);
            byte g = Byte.Parse(hex.Substring(hex.Length - 4, 2), NumberStyles.HexNumber);
            byte b = Byte.Parse(hex.Substring(hex.Length - 2), NumberStyles.HexNumber);
            return Color.FromArgb(a, r, g, b);
        }


        public static string GetValue(OverridableRefValue<string> v)
        {
            if (v == null) return null;
            return v.Value;
        }
    }
}
