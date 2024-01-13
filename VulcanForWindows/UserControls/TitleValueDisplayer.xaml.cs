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
    public sealed partial class TitleValueDisplayer : UserControl, INotifyPropertyChanged
    {
        public enum DisplayStyles
        {
            Vertical, Horizontal, HorizontalWithIcon
        }


        public static readonly DependencyProperty DisplayStyleProperty =
            DependencyProperty.Register("DisplayStyle", typeof(DisplayStyles), typeof(TitleValueDisplayer), new PropertyMetadata(null, DisplayStyle_Changed));

        public DisplayStyles? DisplayStyle
        {
            get => (DisplayStyles?)GetValue(DisplayStyleProperty);
            set
            {
                if (DisplayStyle != value)
                {
                    SetValue(DisplayStyleProperty, value);
                    DisplayStyleChanged();
                }
            }
        }

        private static void DisplayStyle_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TitleValueDisplayer control && e.NewValue is DisplayStyles newValue)
            {
                control.DisplayStyleChanged();
            }
        }

        void DisplayStyleChanged()
        {
            spVertical.Visibility = (DisplayStyle.Value == DisplayStyles.Vertical).ToVisibility();
            spHorizontal.Visibility = (DisplayStyle.Value == DisplayStyles.Horizontal).ToVisibility();
            spHorizontalWithIcon.Visibility = (DisplayStyle.Value == DisplayStyles.HorizontalWithIcon).ToVisibility();
        }


            public TitleValueDisplayer()
        {
            this.InitializeComponent();

            if (StartAsLoading)
                IsLoading = true;

            if (DisplayStyle == null)
                if (Title != null)
                    DisplayStyle = DisplayStyles.Vertical;
                else if (ImageSource != null) DisplayStyle = DisplayStyles.HorizontalWithIcon;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register("Title", typeof(string), typeof(SummaryAverageGradeDisplayer), new PropertyMetadata(null, TitleChanged));

        public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register("Value", typeof(string), typeof(SummaryAverageGradeDisplayer), new PropertyMetadata("-", ValueChanged));

        public static readonly DependencyProperty StartAsLoadingProperty =
        DependencyProperty.Register("StartAsLoading", typeof(bool), typeof(SummaryAverageGradeDisplayer), new PropertyMetadata(false, LoadingChanged));



        public static readonly DependencyProperty ImageSourceProperty =
            DependencyProperty.Register("ImageSource", typeof(string), typeof(TitleValueDisplayer), new PropertyMetadata(null, ImageSource_Changed));

        public string ImageSource
        {
            get => (string)GetValue(ImageSourceProperty);
            set => SetValue(ImageSourceProperty, value);
        }

        private static void ImageSource_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TitleValueDisplayer control && e.NewValue is string newValue)
            {
                // TODO: Implement your logic here
                control.OnPropertyChanged(nameof(ImageSource));

                var bitmapImage = new BitmapImage(new Uri(newValue));
                control.img.Source = bitmapImage;
            }
        }


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
            if (d is TitleValueDisplayer control && e.NewValue is string newValue)
                control.OnPropertyChanged(nameof(Title));
        }
        private static void ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TitleValueDisplayer control && e.NewValue is string newValue)
            {
                control.IsLoading = false;
                control.OnPropertyChanged(nameof(IsLoading));
                control.OnPropertyChanged(nameof(Value));

            }
        }
        private static void LoadingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TitleValueDisplayer control && e.NewValue is bool newValue)
            {
            }
        }
    }
}
