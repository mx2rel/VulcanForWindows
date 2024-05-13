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
using VulcanForWindows.Classes;
using VulcanForWindows.UserControls.ClassmatesGrades;
using Vulcanova.Features.Grades;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VulcanForWindows.UserControls
{
    public sealed partial class RecentGrade : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public static readonly DependencyProperty GradesPoolProperty =
            DependencyProperty.Register("GradesPool", typeof(Grade[]), typeof(RecentGrade), new PropertyMetadata(null, GradesPool_Changed));

        public Grade[] GradesPool
        {
            get => (Grade[])GetValue(GradesPoolProperty);
            set => SetValue(GradesPoolProperty, value);
        }

        private static void GradesPool_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is RecentGrade control && e.NewValue is Grade[] newValue)
            {
                if (newValue.Length == 0) return;
                var iterateThrough = newValue.GetLatestGrades().Where(r => r.Column.Weight > 0).Where(r => r.VulcanValue.HasValue);
                bool onlyNewOnes = iterateThrough.Where(r => r.IsRecent).Count() > 0;
                var listOfGrades = new List<Grade>();
                foreach (var grade in iterateThrough.Where(r => (r.IsRecent || !onlyNewOnes)))
                {
                    for (int i = 0; i <= Math.Max(Math.Pow(grade.Column.Weight, 1d / 3d), 1); i++)
                        listOfGrades.Add(grade);
                }
                if (listOfGrades.Count > 0)
                {
                    Random random = new Random();
                    int randomIndex = random.Next(0, listOfGrades.Count);

                    var selected = listOfGrades[randomIndex];
                    control.Grade = selected;
                }
            }
        }


        public static readonly DependencyProperty DisplayClassmatesGradesProperty =
            DependencyProperty.Register("DisplayClassmatesGrades", typeof(bool), typeof(RecentGrade), new PropertyMetadata(true, DisplayClassmatesGrades_Changed));

        public bool DisplayClassmatesGrades
        {   
            get => (bool)GetValue(DisplayClassmatesGradesProperty);
            set => SetValue(DisplayClassmatesGradesProperty, value);
        }

        private static void DisplayClassmatesGrades_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is RecentGrade control && e.NewValue is bool newValue)
            {
                //control.ClassmateGradesContainer.Visibility = newValue ? Visibility.Visible : Visibility.Collapsed;
                //if (newValue == false)
                //{
                //    (control.ClassmateGradesContainer.Parent as Grid).Children.Remove(control.ClassmateGradesContainer); //TODO: MAKE IT RIGHT WAY, SOMETHING CRASHES APP WHEN ACTUALLY HIDDEN, NOT REMOVED
                //}
            }
        }



        public static readonly DependencyProperty GradeProperty =
            DependencyProperty.Register("Grade", typeof(Grade), typeof(RecentGrade), new PropertyMetadata(null, Grade_Changed));

        public Grade Grade
        {
            get => (Grade)GetValue(GradeProperty);
            set => SetValue(GradeProperty, value);
        }

        public bool isGradeNull { get => Grade == null; }

        private static void Grade_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is RecentGrade control && e.NewValue is Grade newValue)
            {
                control.OnPropertyChanged(nameof(Grade));
                control.OnPropertyChanged(nameof(isGradeNull));
                if (control.DisplayClassmatesGrades)
                    control.SpawnClassmateGrades();
            }
        }

        void SpawnClassmateGrades()
        {
            if (!DisplayClassmatesGrades) return;

            var c = new SingleClassmateGrades();
            c.ErrorMessageOrientation = Orientation.Horizontal;
            c.ColumnId = Grade.Column.Id;
            c.UserGrade = Grade;
            c.IsCompact = true;
            ClassmateGradesContainer.Children.Add(c);
        }

        public RecentGrade()
        {
            this.InitializeComponent();
        }
    }
}
