﻿using Microsoft.UI.Xaml;
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
using Vulcanova.Features.Exams;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VulcanForWindows.UserControls
{
    public sealed partial class SingleExam : UserControl, INotifyPropertyChanged
    {


        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        }


        public static readonly DependencyProperty ExamProperty =
            DependencyProperty.Register("Exam", typeof(Exam), typeof(SingleExam), new PropertyMetadata(null, Exam_Changed));

        public Exam Exam
        {
            get => (Exam)GetValue(ExamProperty);
            set => SetValue(ExamProperty, value);
        }

        private static void Exam_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SingleExam control && e.NewValue is string newValue)
            {
                // TODO: Implement your logic here
            }
        }

        public SingleExam()
        {
            OnPropertyChanged(nameof(Exam));
            this.InitializeComponent();
        }
    }
}