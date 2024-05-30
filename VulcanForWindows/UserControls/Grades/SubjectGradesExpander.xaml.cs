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
using Vulcanova.Features.Grades;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VulcanForWindows.UserControls.Grades
{
    public sealed partial class SubjectGradesExpander : UserControl
    {


        public static readonly DependencyProperty SubjectGradesProperty =
            DependencyProperty.Register("SubjectGrades", typeof(SubjectGrades), typeof(SubjectGradesExpander), new PropertyMetadata(null, SubjectGrades_Changed));

        public SubjectGrades SubjectGrades
        {
            get => (SubjectGrades)GetValue(SubjectGradesProperty);
            set => SetValue(SubjectGradesProperty, value);
        }

        private static void SubjectGrades_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SubjectGradesExpander control && e.NewValue is SubjectGrades newValue)
            {
                // TODO: Implement your logic here
            }
        }

        public SubjectGradesExpander()
        {
            this.InitializeComponent();
        }

        private void Expanding(Expander sender, ExpanderExpandingEventArgs args)
        {
            RightInfo.Opacity = 0;
        }
        private void Collapsed(Expander sender, ExpanderCollapsedEventArgs args)
        {
            RightInfo.Opacity = 1;
        }

        private void RemoveHipotheticGrade(object sender, RoutedEventArgs e)
        {
            var grade = ((sender as MenuFlyoutItem).DataContext as Grade);
            SubjectGrades.removeAddedGrade(grade);
        }

        private void ViewGradeDetails(object sender, TappedRoutedEventArgs e)
        {
            if (sender is FrameworkElement fe)
            {
                var grade = (fe.Parent as FrameworkElement).DataContext as Grade;
                if (!grade.IsHipothetic)
                    GradeFullInfo.OpenGradeFullInfo(grade, SubjectGrades);
            }
        }


        private async void AddHipotheticGrade(object sender, RoutedEventArgs e)
        {
            ContentDialog dialog = new ContentDialog();
            dialog.XamlRoot = this.XamlRoot;
            var v = (Resources["HipotheticGradeAddContent"] as DataTemplate).LoadContent() as StackPanel;
            v.DataContext = (sender as Button).DataContext;
            dialog.Content = v;
            dialog.CloseButtonText = "Anuluj";
            dialog.PrimaryButtonText = "Dodaj";
            dialog.DefaultButton = ContentDialogButton.Primary;

            dialog.PrimaryButtonClick += AddHip;
            var result = await dialog.ShowAsync();

        }
        private void RemoveHipotheticGrades(object sender, RoutedEventArgs e)
        {
            ((sender as Button).DataContext as SubjectGrades).removeAddedGrades();
        }

        private void AddHip(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            IEnumerable<NumberBox> numberBoxes = (sender.Content as StackPanel).Children.OfType<NumberBox>();
            int grade = 0;
            int weight = 0;
            foreach (var v in numberBoxes)
                if (v.Tag != null)
                    if (v.Tag.ToString() == "grade")
                        grade = (int)Math.Round(v.Value);
                    else
                        if (v.Tag.ToString() == "weight")
                        weight = (int)Math.Round(v.Value);

            SubjectGrades.AddGrade(new Grade
            {
                Content = grade.ToString(),
                ContentRaw = grade.ToString(),
                Column = new Column
                {
                    Name = "Hipotetyczna ocena",
                    Weight = weight,
                    Subject = new Vulcanova.Features.Shared.Subject()
                    {
                        Id = SubjectGrades.grades[0].Column.Subject.Id
                    },
                    Color = 16747520
                },
                VulcanValue = grade,
                IsHipothetic = true,
                Id = (int)DateTime.Now.Ticks,
            });
            MainExpander.DataContext = SubjectGrades;
            MainExpander.UpdateLayout();
            //FindEvenDeepChildrenOfType<TextBlock>(eSgu.Header as ).Where(r => r.Name == "average").ToArray()[0].UpdateLayout();
            //FindObjectsByName()
            //grades.ReplaceAll(grades.ToArray());
            //var s = eSgu.DataContext;
            //eSgu.DataContext = null;
            //eSgu.DataContext = s;
            MainExpander.GetBindingExpression(FrameworkElement.DataContextProperty)?.UpdateSource();
            //TopLevel.UpdateLayout();
            var lv = ((MainExpander.Content as Grid).Children[1] as ListView);
            lv.ScrollIntoView(lv.Items.Last());
        }
    }
}
