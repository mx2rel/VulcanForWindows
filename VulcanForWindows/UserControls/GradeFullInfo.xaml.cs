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

namespace VulcanForWindows.UserControls
{
    public sealed partial class GradeFullInfo : UserControl
    {


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
    }
}
