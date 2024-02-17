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
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using VulcanForWindows.Classes;
using Vulcanova.Features.Auth;
using Vulcanova.Features.Grades;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using VulcanTest.Vulcan;
using Newtonsoft.Json;
using System.Diagnostics;
using VulcanForWindows.Vulcan.Grades;
using Vulcanova.Features.Grades.Final;
using VulcanForWindows.Vulcan.Grades.Final;
using System.Threading.Tasks;

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


            grades = new ObservableCollection<SubjectGrades>();
            selectedPeriod = new AccountRepository().GetActiveAccountAsync().Periods.Last();
            PeriodSelector.SelectedIndex = avaiblePeriods.Length - 1;
            PeriodSelector.UpdateLayout();
            AssignGrades();
            cd = new MonthChartData();
            timer = new DispatcherTimer();
            timer.Tick += Timer_Tick;
            timer.Interval = TimeSpan.FromSeconds(0.7);
        }
        IDictionary<int, GradesResponseEnvelope> envelopes = new Dictionary<int, GradesResponseEnvelope>();
        IDictionary<int, FinalGradesResponseEnvelope> fEnvelopes = new Dictionary<int, FinalGradesResponseEnvelope>();
        public Vulcanova.Features.Shared.Period selectedPeriod;
        public int selectedPeriodId => selectedPeriod.Id;
        public Vulcanova.Features.Shared.Period[] avaiblePeriods => new AccountRepository().GetActiveAccountAsync().Periods/*.Select(r => r.Id)*/.ToArray();
        public string[] displayPeriods => avaiblePeriods.Select(r => $"Klasa {r.Level}, Semestr {r.Number}").ToArray();
        private void ChangedPeriod(object sender, SelectionChangedEventArgs e)
        {
            var p = avaiblePeriods[(sender as ComboBox).SelectedIndex] as Vulcanova.Features.Shared.Period;
            if (selectedPeriod.Id == p.Id) return;
            selectedPeriod = p;
            LoadingBar.Visibility = Visibility.Visible;

            AssignGrades();
        }

        GradesResponseEnvelope env
        {
            get => envelopes[selectedPeriodId];
            set => envelopes[selectedPeriodId] = value;
        }
        async void AssignGrades()
        {
            var acc = new AccountRepository().GetActiveAccountAsync();

            if (!fEnvelopes.ContainsKey(selectedPeriodId))
                fEnvelopes[selectedPeriodId] = await new FinalGrades().GetPeriodGrades(acc, selectedPeriodId,false,true);

            if (!envelopes.ContainsKey(selectedPeriodId))
            {
                env = await new GradesService().GetPeriodGrades(acc, selectedPeriodId, true, false);
                env.Updated += HandleGradesUpdated;
            }
            else
            {
                HandleGradesUpdated(env, new Grade[0]);
                //Debug.Write(JsonConvert.SerializeObject(cd));
            }
            //TopLevel.UpdateLayout();
        }

        async Task<FinalGradesResponseEnvelope> GetFenvelope(int id)
        {
            if (!fEnvelopes.ContainsKey(id))
                fEnvelopes[id] = await new FinalGrades().GetPeriodGrades(new AccountRepository().GetActiveAccountAsync(), selectedPeriodId, false, true);
            return fEnvelopes[id];
        }

        async void HandleGradesUpdated(object sender, IEnumerable<Grade> updatedGrades)
        {
            grades.ReplaceAll(SubjectGrades.GetSubjectsGrades(sender as GradesResponseEnvelope, await GetFenvelope(selectedPeriodId)));
            LoadingBar.Visibility = isLoading ? Visibility.Visible : Visibility.Collapsed;

            cd = MonthChartData.Generate(grades.SelectMany(r=>r.grades).ToArray());
            chartAndTableGrid.DataContext = cd;
            chartAndTableGrid.UpdateLayout();
        }
        public bool isLoading => ((grades.ToArray().Length > 0) ? env.isLoading : true);
        public ObservableCollection<SubjectGrades> grades { get; set; }


        //private void myButton_Click(object sender, RoutedEventArgs e)
        //{
        //    myButton.Content = "Clicked";
        //    LoadingBar.Visibility = Visibility.Collapsed;

        //}

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
            if (gradeOver != null)
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

        private async void DisplayHipotheticGrade(object sender, RoutedEventArgs e)
        {
            ContentDialog dialog = new ContentDialog();
            dialog.XamlRoot = this.XamlRoot;
            var v = (Resources["HipotheticGradeCheckContent"] as DataTemplate).LoadContent() as StackPanel;
            v.DataContext = (sender as Button).DataContext;
            dialog.Content = v;
            dialog.CloseButtonText = "Cancel";

            var result = await dialog.ShowAsync();

        }
        SubjectGrades eSg;
        Expander eSgu;
        private async void AddHipotheticGrade(object sender, RoutedEventArgs e)
        {
            ContentDialog dialog = new ContentDialog();
            dialog.XamlRoot = this.XamlRoot;
            var v = (Resources["HipotheticGradeAddContent"] as DataTemplate).LoadContent() as StackPanel;
            v.DataContext = (sender as Button).DataContext;
            eSg = ((sender as Button).DataContext as SubjectGrades);
            eSgu = FindParentOfType<Expander>(sender as DependencyObject);
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

        public T FindParentOfType<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parent = VisualTreeHelper.GetParent(child);

            while (parent != null)
            {
                if (parent is T typedParent)
                {
                    return typedParent;
                }
                parent = VisualTreeHelper.GetParent(parent);
            }

            return null;
        }

        public List<T> FindEvenDeepChildrenOfType<T>(DependencyObject parent) where T : DependencyObject
        {
            var resultList = new List<T>();

            if (parent == null) return resultList;

            var count = VisualTreeHelper.GetChildrenCount(parent);

            for (var i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                // Check if the current child is of the specified type and is even
                if (child is T typedChild && i % 2 == 0)
                {
                    resultList.Add(typedChild);
                }

                // Recursively search for even deep children
                resultList.AddRange(FindEvenDeepChildrenOfType<T>(child));
            }

            return resultList;
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

            eSg.AddGrade(new Grade
            {
                Content = grade.ToString(),
                ContentRaw = grade.ToString(),
                Column = new Column
                {
                    Name = "Hipotetyczna ocena",
                    Weight = weight
                },
                Value=grade,
                IsHipothetic = true
            });
            eSgu.DataContext = eSg;
            eSgu.UpdateLayout();
            //FindEvenDeepChildrenOfType<TextBlock>(eSgu.Header as ).Where(r => r.Name == "average").ToArray()[0].UpdateLayout();
            //FindObjectsByName()
            //grades.ReplaceAll(grades.ToArray());
            //var s = eSgu.DataContext;
            //eSgu.DataContext = null;
            //eSgu.DataContext = s;
            eSgu.GetBindingExpression(FrameworkElement.DataContextProperty)?.UpdateSource();
            TopLevel.UpdateLayout();
            var lv = ((eSgu.Content as StackPanel).Children[0] as ListView);
            lv.ScrollIntoView(lv.Items.Last());
        }


        private void ChangedWeight(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            if (sender.Parent != null)
                foreach (var v in (sender.Parent as StackPanel).Children)
                {
                    if ((v as FrameworkElement).Tag != null)
                        if (v is TextBlock t)
                        {
                            t.Text = $"{t.Tag}: {(sender.DataContext as SubjectGrades).CountAverage(int.Parse(t.Tag.ToString()), (int)sender.Value)}";
                        }
                }
        }

        private async void ViewGradeDetails(object sender, TappedRoutedEventArgs e)
        {
                ContentDialog dialog = new ContentDialog();
            dialog.XamlRoot = this.XamlRoot;
            var v = (Resources["GradeFullInfo"] as DataTemplate).LoadContent() as StackPanel;
            v.DataContext = (sender as ListView).DataContext;
            dialog.Content = v;
            dialog.CloseButtonText = "Ok";
            var result = await dialog.ShowAsync();
        }
    }
}
