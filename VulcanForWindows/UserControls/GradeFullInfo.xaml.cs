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

namespace VulcanForWindows.UserControls
{
    public sealed partial class GradeFullInfo : UserControl
    {


        public static readonly DependencyProperty SubjectGradesProperty =
            DependencyProperty.Register("SubjectGrades", typeof(SubjectGrades), typeof(GradeFullInfo), new PropertyMetadata(null, SubjectGrades_Changed));

        public SubjectGrades SubjectGrades
        {
            get => (SubjectGrades)GetValue(SubjectGradesProperty);
            set => SetValue(SubjectGradesProperty, value);
        }

        private async static void SubjectGrades_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is GradeFullInfo control && e.NewValue is SubjectGrades newValue)
            {
                control.avgChange.Visibility = (newValue == null) ? Visibility.Collapsed : Visibility.Visible;
                if (newValue == null) return;
                var change = Math.Round(newValue.yearActualAverage - ((await newValue.CalculateYearlyAverage(new Grade[1] { control.Grade }, includeAddedGrades: false)).average), 2);
                control.avgChange.Text = $"Średnia: {((change > 0) ? "+" : "")}{change}";
            }
        }


        public static readonly DependencyProperty GradeProperty =
            DependencyProperty.Register("Grade", typeof(Grade), typeof(GradeFullInfo), new PropertyMetadata(null, Grade_Changed));

        public Grade Grade
        {
            get => (Grade)GetValue(GradeProperty);
            set => SetValue(GradeProperty, value);
        }

        private static void Grade_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is GradeFullInfo control && e.NewValue is Grade newValue)
            {
                // TODO: Implement your logic here
            }
        }


        public GradeFullInfo()
        {
            this.InitializeComponent();
        }

        public static Window OpenGradeFullInfo(Grade grade, SubjectGrades context, bool IsPriorityWindow = true, TypedEventHandler<object, WindowEventArgs> OnClosed = null)
        {
            Window w;

            if (IsPriorityWindow)
                MainWindow.Instance.IsOtherWindowOpen = true;

            w = new Window();
            var v = new GradeFullInfo();
            v.Grade = grade;
            v.DataContext = grade;
            v.SubjectGrades = context;
            w.Content = v;
            w.SystemBackdrop = new MicaBackdrop();
            int newX = (MainWindow.Instance.AppWindow.Size.Width - 800) / 2 + MainWindow.Instance.AppWindow.Position.X;
            int newY = (MainWindow.Instance.AppWindow.Size.Height - 450) / 2 + MainWindow.Instance.AppWindow.Position.Y;
            w.AppWindow.MoveAndResize(new Windows.Graphics.RectInt32(newX, newY, 800, 450));
            w.ExtendsContentIntoTitleBar = true;
            if (IsPriorityWindow)
                w.Activated += W_Activated;
            if (IsPriorityWindow)
                w.SizeChanged += W_SizeChanged;
            if (IsPriorityWindow)
                w.Closed += W_Closed;
            if (OnClosed != null)
                w.Closed += OnClosed;
            w.Activate();

            return w;
        }

        private static void W_Closed(object sender, WindowEventArgs args)
        {
            MainWindow.Instance.IsOtherWindowOpen = false;
        }

        private static void W_SizeChanged(object sender, Microsoft.UI.Xaml.WindowSizeChangedEventArgs args)
        {
            if (sender is Window w)
            {
                w.Close();
                MainWindow.Instance.IsOtherWindowOpen = false;
            }

        }

        private static void W_Activated(object sender, Microsoft.UI.Xaml.WindowActivatedEventArgs e)
        {
            if (sender is Window w)
            {
                if (e.WindowActivationState == WindowActivationState.Deactivated)
                {
                    w.Close();
                    MainWindow.Instance.IsOtherWindowOpen = false;
                }
            }
        }
    }
}
