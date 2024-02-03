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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using VulcanForWindows.Classes;
using VulcanForWindows.UserControls.Deadlinables;
using Vulcanova.Features.Exams;
using Vulcanova.Features.Homework;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VulcanForWindows.UserControls
{
    public sealed partial class SingleDeadlineable : UserControl, INotifyPropertyChanged
    {


        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        }


        public static readonly DependencyProperty ExamProperty =
            DependencyProperty.Register("ExamOrTest", typeof(Deadlineable), typeof(SingleDeadlineable), new PropertyMetadata(null, Exam_Changed));

        public Deadlineable Deadlinable
        {
            get => (Deadlineable)GetValue(ExamProperty);
            set => SetValue(ExamProperty, value);
        }

        private static void Exam_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SingleDeadlineable control && e.NewValue is string newValue)
            {
                // TODO: Implement your logic here
            }
        }


        public static readonly DependencyProperty AllowClickProperty =
            DependencyProperty.Register("AllowClick", typeof(bool), typeof(SingleDeadlineable), new PropertyMetadata(true, AllowClick_Changed));

        public bool AllowClick
        {
            get => (bool)GetValue(AllowClickProperty);
            set => SetValue(AllowClickProperty, value);
        }

        private static void AllowClick_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SingleDeadlineable control && e.NewValue is bool newValue)
            {
                // TODO: Implement your logic here
            }
        }


        public SingleDeadlineable()
        {
            OnPropertyChanged(nameof(Deadlinable));
            this.InitializeComponent();
        }

        private async void Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (!AllowClick) return;
            object content = null;
            if (Deadlinable.createdFrom is Exam)
            {
                content = new ExamPopup(Deadlinable.createdFrom as Exam);
            }
            if (Deadlinable.createdFrom is Homework)
            { }

            if (content == null) return;

            ContentDialog dialog = new ContentDialog();
            dialog.XamlRoot = this.XamlRoot;
            dialog.Content = content;
            dialog.CloseButtonText = "Zamknij";
            dialog.MinWidth = 600;
            dialog.MinHeight = 600;
            var result = await dialog.ShowAsync();

        }
    }
}
