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
using Vulcanova.Features.Timetable;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VulcanForWindows.UserControls
{
    public sealed partial class LessonFlyoutInfoContent : UserControl, INotifyPropertyChanged
    {

        public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register("VulcanValue", typeof(TimetableListEntry), typeof(LessonFlyoutInfoContent), new PropertyMetadata(null, ValueChanged));

        public TimetableListEntry Value
        {
            get => (TimetableListEntry)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }
        private static void ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is LessonFlyoutInfoContent s) s.OnPropertyChanged(nameof(Value));
        }

        public LessonFlyoutInfoContent()
        {
            this.InitializeComponent();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
