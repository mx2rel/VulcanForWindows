using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Vulcanova.Features.Timetable;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VulcanForWindows.UserControls
{
    public sealed partial class TimetableDayDisplayer : UserControl, INotifyPropertyChanged
    {

        public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register("Value", typeof(TimetableDay), typeof(TimetableDayDisplayer), new PropertyMetadata(null, ValueChanged));

        public static readonly DependencyProperty DisplayDateProperty =
        DependencyProperty.Register("Value", typeof(TimetableDay), typeof(bool), new PropertyMetadata(null, DisplayDateChanged));


        public TimetableDay Value
        {
            get => (TimetableDay)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public bool isEmpty => Value.entries.Value.Length == 0;

        public bool DisplayDate
        {
            get
            {
                try
                {
                    object displayDateValue = GetValue(DisplayDateProperty);

                    // Check if the retrieved value is not null and is of type bool
                    if (displayDateValue != null && displayDateValue is bool)
                    {
                        return (bool)displayDateValue;
                    }
                    else
                    {
                        // Handle the case where the retrieved value is null or not of type bool
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    // Handle other exceptions if needed
                    Console.WriteLine($"An error occurred: {ex.Message}");
                    return true;
                }
            }
            set => SetValue(DisplayDateProperty, value);
        }

        private static void ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TimetableDayDisplayer s)
            {
                s.OnPropertyChanged(nameof(Value));
                s.OnPropertyChanged(nameof(isEmpty));
            }

        }

        private static void DisplayDateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TimetableDayDisplayer s)
            {
                s.OnPropertyChanged(nameof(DisplayDate));
            }
        }

        public TimetableDayDisplayer()
        {
            OnPropertyChanged(nameof(Value));
            OnPropertyChanged(nameof(isEmpty));
            OnPropertyChanged(nameof(DisplayDate));
            this.InitializeComponent();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }



        private async void ShowLessonDetails(object sender, ItemClickEventArgs e)
        {

            ContentDialog dialog = new ContentDialog();
            dialog.XamlRoot = this.XamlRoot;
            var v = (Resources["LessonFullInfo"] as DataTemplate).LoadContent() as LessonFlyoutInfoContent;
            v.DataContext = e.ClickedItem as TimetableListEntry;
            dialog.Content = v;
            dialog.CloseButtonText = "Zamknij";
            var result = await dialog.ShowAsync();
        }
    }
}
