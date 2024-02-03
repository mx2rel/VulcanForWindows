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
using Windows.Foundation;
using Microsoft.UI;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VulcanForWindows.UserControls
{
    public sealed partial class DeadlineDisplayer : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        }

        public static readonly DependencyProperty DeadlineInProperty =
            DependencyProperty.Register("DeadlineIn", typeof(int), typeof(DeadlineDisplayer), new PropertyMetadata(int.MaxValue, DeadlineIn_Changed));

        public int DeadlineIn
        {
            get => (int)GetValue(DeadlineInProperty);
            set => SetValue(DeadlineInProperty, value);
        }

        private static void DeadlineIn_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DeadlineDisplayer control && e.NewValue is int newValue)
            {
                // TODO: Implement your logic here
            }
        }


        public static readonly DependencyProperty MaxValueProperty =
            DependencyProperty.Register("MaxValue", typeof(int), typeof(DeadlineDisplayer), new PropertyMetadata(7, MaxValue_Changed));

        public int MaxValue
        {
            get => (int)GetValue(MaxValueProperty);
            set => SetValue(MaxValueProperty, value);
        }

        private static void MaxValue_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DeadlineDisplayer control && e.NewValue is int newValue)
            {
                // TODO: Implement your logic here
            }
        }


        public static readonly DependencyProperty InfoLevelProperty =
            DependencyProperty.Register("InfoLevel", typeof(int), typeof(DeadlineDisplayer), new PropertyMetadata(6, InfoLevel_Changed));

        public int InfoLevel
        {
            get => (int)GetValue(InfoLevelProperty);
            set => SetValue(InfoLevelProperty, value);
        }

        private static void InfoLevel_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DeadlineDisplayer control && e.NewValue is int newValue)
            {
                // TODO: Implement your logic here
            }
        }


        public static readonly DependencyProperty WarningLevelProperty =
            DependencyProperty.Register("WarningLevel", typeof(int), typeof(DeadlineDisplayer), new PropertyMetadata(4, WarningLevel_Changed));

        public int WarningLevel
        {
            get => (int)GetValue(WarningLevelProperty);
            set => SetValue(WarningLevelProperty, value);
        }

        private static void WarningLevel_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DeadlineDisplayer control && e.NewValue is int newValue)
            {
                // TODO: Implement your logic here
            }
        }


        public static readonly DependencyProperty ErrorLevelProperty =
            DependencyProperty.Register("ErrorLevel", typeof(int), typeof(DeadlineDisplayer), new PropertyMetadata(2, ErrorLevel_Changed));

        public int ErrorLevel
        {
            get => (int)GetValue(ErrorLevelProperty);
            set => SetValue(ErrorLevelProperty, value);
        }

        private static void ErrorLevel_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DeadlineDisplayer control && e.NewValue is int newValue)
            {
                // TODO: Implement your logic here
            }
        }

        public int Level
        {
            get
            {
                if (ErrorLevel > DeadlineIn)
                    return 3;
                if (WarningLevel > DeadlineIn)
                    return 2;
                if (InfoLevel > DeadlineIn)
                    return 1;
                return 0;
            }
        }

        public Microsoft.UI.Xaml.Media.
                        SolidColorBrush backgroundColor => new Microsoft.UI.Xaml.Media.
                        SolidColorBrush(colors[Level].bg);

        public Microsoft.UI.Xaml.Media.
                        SolidColorBrush textColor => new Microsoft.UI.Xaml.Media.
                        SolidColorBrush(colors[Level].fg);

        public (Windows.UI.Color bg, Windows.UI.Color fg)[] colors = new (Windows.UI.Color bg, Windows.UI.Color fg)[]
        {
            (Colors.White, Microsoft.UI.Colors.Black),
            (Colors.Yellow, Microsoft.UI.Colors.Black),
            (Colors.Orange, Microsoft.UI.Colors.Black),
            (Colors.Red, Microsoft.UI.Colors.Black)
        };

        public Visibility ShouldShow => (DeadlineIn <= MaxValue && DeadlineIn >= 0).ToVisibility();

        public DeadlineDisplayer()
        {
            UpdateProperties();
            this.InitializeComponent();
        }

        private void UpdateProperties()
        {
            OnPropertyChanged(nameof(Level));
            OnPropertyChanged(nameof(backgroundColor));
            OnPropertyChanged(nameof(textColor));
        }
    }
}
