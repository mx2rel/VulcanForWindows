using DevExpress.WinUI.Core.Internal;
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
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VulcanForWindows.UserControls
{
    public sealed partial class SummaryAverageGradeDisplayer : UserControl
    {
        public SummaryAverageGradeDisplayer()
        {
            this.InitializeComponent();
        }

        public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register("Title", typeof(string), typeof(UserControl1), new PropertyMetadata(null, TitleChanged));

        public static readonly DependencyProperty AverageProperty =
        DependencyProperty.Register("Average", typeof(string), typeof(UserControl1), new PropertyMetadata(null, AverageChanged));

        public static readonly DependencyProperty IsLoadingProperty =
        DependencyProperty.Register("IsLoading", typeof(bool), typeof(UserControl1), new PropertyMetadata(null, LoadingChanged));

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }
        public string Average
        {
            get => (string)GetValue(AverageProperty);
            set => SetValue(AverageProperty, value);
        }
        public bool IsLoading
        {
            get => (bool)GetValue(IsLoadingProperty);
            set => SetValue(IsLoadingProperty, value);
        }

        private static void TitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SummaryAverageGradeDisplayer control && e.NewValue is string newValue)
                control.TitleText.Text = newValue;
        }
        private static void AverageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SummaryAverageGradeDisplayer control && e.NewValue is string newValue)
            {
                control.AverageText.Text = newValue;
                bool b = newValue == "0";
                control.Skeleton.Visibility = b.ToVisibility();
                control.AverageText.Visibility = (!b).ToVisibility();

            }
        }
        private static void LoadingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SummaryAverageGradeDisplayer control && e.NewValue is bool newValue)
            {
                control.Skeleton.Visibility = newValue.ToVisibility();
                control.AverageText.Visibility = (!newValue).ToVisibility();
            }
        }
    }
}
