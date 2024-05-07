using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using VulcanForWindows.Classes;
using Vulcanova.Features.Auth;
using Vulcanova.Features.Grades;
using VulcanTest.Vulcan;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VulcanForWindows
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class FinalGradesAnalyzerPage : Page
    {


        public ObservableCollection<SubjectGradesAnalyzed> grades = new ObservableCollection<SubjectGradesAnalyzed>();

        public FinalGradesAnalyzerPage()
        {
            this.InitializeComponent();
            Init();
        }

        async void Init()
        {
            grades.ReplaceAll(await GetSubjectGrades());
            RecalculateAverage();
        }

        SubjectGradesAnalyzed[] cachedData = new SubjectGradesAnalyzed[0];
        DateTime cachedAt = DateTime.MinValue;

        public async Task<SubjectGradesAnalyzed[]> GetSubjectGrades()
        {
            if (DateTime.Now - cachedAt > new TimeSpan(0, 15, 0))
            {
                var acc = new AccountRepository().GetActiveAccountAsync();
                int currentPeriod = acc.Periods.Last().Id;
                var env = await new GradesService().GetPeriodGrades(acc, currentPeriod, true, true);
                cachedData =
                await SubjectGradesAnalyzed.GetSubjectGradesAnalyzed(env.Grades.ToArray(), currentPeriod);
                cachedAt = DateTime.Now;
            }
            return cachedData;
        }

        public void RecalculateAverage()
        {
            average = Math.Round(grades.Where(r => r.includeInCalculations).Select(r => float.Parse(r.displayGrade)).Average() * 100) / 100;
            averageDisplay.Text = average.ToString("0.00");
        }

        public double average;

        public bool ValidateTextBox(string input)
        {
            return
                int.TryParse(input, out int result);
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var context = (sender as FrameworkElement).DataContext as SubjectGradesAnalyzed;
            var textbox = (sender as TextBox);
            if (ValidateTextBox(textbox.Text))
            {
                context.finalGradeOverride = textbox.Text;
                RecalculateAverage();
            }
            else
            {
                textbox.Text = context.displayGrade;
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            var context = (sender as CheckBox).DataContext as SubjectGradesAnalyzed;
            if (context != null)
            {
                context.includeInCalculations = true;
                CheckedOrUnchecked(context);
            }
            RecalculateAverage();
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            var context = (sender as CheckBox).DataContext as SubjectGradesAnalyzed;
            if (context != null)
            {
                context.includeInCalculations = false;
                CheckedOrUnchecked(context);
            }
            RecalculateAverage();
        }

        void CheckedOrUnchecked(SubjectGradesAnalyzed s)
            => Preferences.Set<bool>($"Analyzer_{s.subject.Id}_Include", s.includeInCalculations);
    }
}
