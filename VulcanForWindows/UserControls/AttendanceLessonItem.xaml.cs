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
using Vulcanova.Features.Attendance;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VulcanForWindows.UserControls
{
    public sealed partial class AttendanceLessonItem : UserControl, INotifyPropertyChanged
    {

        public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register("Value", typeof(Lesson), typeof(AttendanceLessonItem), new PropertyMetadata(null, ValueChanged));
        public static readonly DependencyProperty DisplayDateProperty =
        DependencyProperty.Register("DisplayDate", typeof(bool), typeof(AttendanceLessonItem), new PropertyMetadata(null, DisplayDateChanged));


        public Lesson Value
        {
            get => (Lesson)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }
        public bool DisplayDate
        {
            get => (bool)GetValue(DisplayDateProperty);
            set => SetValue(DisplayDateProperty, value);
        }

        private static void ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is AttendanceLessonItem s) s.OnPropertyChanged(nameof(Value));
        }
        private static void DisplayDateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is AttendanceLessonItem s) s.dateDisplayer.Visibility = s.DisplayDate.ToVisibility();
        }

        public AttendanceLessonItem()
        {
            this.InitializeComponent();
            OnPropertyChanged(nameof(Value));
            OnPropertyChanged(nameof(DisplayDate));
            dateDisplayer.Visibility = DisplayDate.ToVisibility();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
