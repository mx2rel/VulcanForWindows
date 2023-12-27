using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
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
using VulcanForWindows.Classes;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VulcanForWindows
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class GradesPage : Page
    {
        public MonthChartData cd { get; set; }
        public GradesPage()
        {
            this.InitializeComponent();
            grades = SubjectGrades.GetSubjectsGrades();
            cd = MonthChartData.Generate(grades.SelectMany(r => r.grades).ToArray());

            timer = new DispatcherTimer();
            timer.Tick += Timer_Tick;
            timer.Interval = TimeSpan.FromSeconds(0.7);
        }
        public SubjectGrades[] grades { get; set; }


        private void myButton_Click(object sender, RoutedEventArgs e)
        {
            myButton.Content = "Clicked";
        }

        Flyout gradeFlyout;
        Flyout subjectFlyout;
        private void Grade_ShowInfo(object sender)
        {
            var g = (sender as ListView).DataContext as Grade;
            var content = (Resources["GradeInfoFlyout"] as DataTemplate);
            ContentControl contentControl = new ContentControl
            {
                ContentTemplate = content,
                Content = g
            };

            gradeFlyout = new Flyout();
            gradeFlyout.OverlayInputPassThroughElement = sender as DependencyObject;
            //PointerMoved += PointerMovedF;
            gradeFlyout.Content = contentControl;
            gradeFlyout.ShowAt(sender as FrameworkElement);
        }

        private void Subject_ShowInfo(object sender)
        {
            //var resD = new ResourceDictionary { Source = new Uri("ms-appx:///ResourceDictionary1.xaml") };
            var g = (sender as Grid).DataContext as SubjectGrades;

            var content = (/*resD*/Resources["SubjectGradesInfoFlyout"] as DataTemplate);
            ContentControl contentControl = new ContentControl
            {
                ContentTemplate = content,
                Content = g
            };

            subjectFlyout = new Flyout();
            subjectFlyout.OverlayInputPassThroughElement = sender as DependencyObject;
            subjectFlyout.Placement = FlyoutPlacementMode.Right;
            //PointerMoved += PointerMovedF;
            subjectFlyout.Content = contentControl;
            subjectFlyout.ShowAt(sender as FrameworkElement);
        }

        private void ChangeChartAndTable(object sender, RoutedEventArgs e)
        {
            var og = table.Visibility;
            table.Visibility = chart.Visibility;
            chart.Visibility = og;
        }

        object gradeOver;
        object subjectOver;
        private DispatcherTimer timer;

        private void Timer_Tick(object sender, object e)
        {
            timer.Stop();
            // Function to be executed after 1 second
            // Example: DoSomethingAfterHover();
            if(gradeOver != null)
            Grade_ShowInfo(gradeOver);
            if (subjectOver != null)
                Subject_ShowInfo(subjectOver);
        }

        private void ListView_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            gradeOver = sender;
            timer.Start();
        }

        private void ListView_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            timer.Stop();
            if (gradeFlyout != null)
            {
                gradeFlyout.Hide();
                gradeOver = null;
            }
        }

        private void Expander_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            subjectOver = sender;
            timer.Start();
        }

        private void Expander_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            timer.Stop();
            if (subjectFlyout != null)
            {
                subjectFlyout.Hide();
                subjectOver = null;
            }
        }
    }
}
