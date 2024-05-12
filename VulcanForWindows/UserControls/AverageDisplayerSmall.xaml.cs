using DevExpress.WinUI.Core.Internal;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VulcanForWindows.UserControls
{
    public sealed partial class AverageDisplayerSmall : UserControl, INotifyPropertyChanged
    {

        public AverageDisplayerSmall()
        {
            this.InitializeComponent();

            if (StartAsLoading)
                IsLoading = true;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register("Title", typeof(string), typeof(AverageDisplayerSmall), new PropertyMetadata(null, TitleChanged));

        public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register("Value", typeof(string), typeof(AverageDisplayerSmall), new PropertyMetadata("-", ValueChanged));

        public static readonly DependencyProperty StartAsLoadingProperty =
        DependencyProperty.Register("StartAsLoading", typeof(bool), typeof(AverageDisplayerSmall), new PropertyMetadata(false, LoadingChanged));



        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }
        public string Value
        {
            get => (string)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }
        public bool StartAsLoading
        {
            get => (bool)GetValue(StartAsLoadingProperty);
            set => SetValue(StartAsLoadingProperty, value);
        }
        public bool IsLoading { get; set; }


        private static void TitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is AverageDisplayerSmall control && e.NewValue is string newValue)
                control.OnPropertyChanged(nameof(Title));
        }
        private static void ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is AverageDisplayerSmall control && e.NewValue is string newValue)
            {
                control.IsLoading = false;
                control.OnPropertyChanged(nameof(IsLoading));
                control.OnPropertyChanged(nameof(Value));

            }
        }
        private static void LoadingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is AverageDisplayerSmall control && e.NewValue is bool newValue)
            {
                control.OnPropertyChanged(nameof(IsLoading));
            }
        }
    }
}
