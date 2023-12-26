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
using Vulcanova.Features.Grades;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VulcanForWindows
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class GradesPage : Page
    {
        public GradesPage()
        {
            this.InitializeComponent();
            grades = SubjectGrades.GetSubjectsGrades();

        }
        public SubjectGrades[] grades { get; set; }


        private void myButton_Click(object sender, RoutedEventArgs e)
        {
            myButton.Content = "Clicked";
        }

        private void Grade_ShowInfo(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            var g = (sender as ListView).DataContext as Grade;
            var content = (Resources["GradeInfoFlyout"] as DataTemplate);
            ContentControl contentControl = new ContentControl
            {
                ContentTemplate = content,
                Content = g
            };

            var flyout = new Flyout();
            flyout.Content = contentControl;
            flyout.ShowAt(sender as FrameworkElement);
        }
    }
}
