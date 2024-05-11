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
using Vulcanova.Features.Attendance;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VulcanForWindows.UserControls
{
    public sealed partial class JustificationRequestStatus : UserControl
    {

        public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register("VulcanValue", typeof(Vulcanova.Uonet.Api.Lessons.JustificationStatus?), typeof(JustificationRequestStatus), new PropertyMetadata(null, ValueChanged));

        public static readonly DependencyProperty ContextProperty =
        DependencyProperty.Register("Context", typeof(PresenceType), typeof(JustificationRequestStatus), new PropertyMetadata(null, ContextChanged));

        public static readonly DependencyProperty DisplayForAcceptedProperty =
        DependencyProperty.Register("DisplayForAccepted", typeof(bool), typeof(JustificationRequestStatus), new PropertyMetadata(null, DisplayForAcceptedChanged));

        public Vulcanova.Uonet.Api.Lessons.JustificationStatus? Value
        {
            get => (Vulcanova.Uonet.Api.Lessons.JustificationStatus?)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public bool DisplayForAccepted
        {
            get => (bool)GetValue(DisplayForAcceptedProperty);
            set => SetValue(DisplayForAcceptedProperty, value);
        }
        public PresenceType Context
        {
            get => (PresenceType)GetValue(ContextProperty);
            set => SetValue(ContextProperty, value);
        }
        private static void ContextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is JustificationRequestStatus s) s.Update();
        }
        private static void DisplayForAcceptedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is JustificationRequestStatus s) s.Update();
        }

        private static void ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is JustificationRequestStatus s) s.Update();
        }

        void Update()
        {
            //control.TitleText.Text = newValue;
            Requested.Visibility = (Value == Vulcanova.Uonet.Api.Lessons.JustificationStatus.Requested).ToVisibility();
            Rejected.Visibility = (Value == Vulcanova.Uonet.Api.Lessons.JustificationStatus.Rejected).ToVisibility();
            Accepted.Visibility = ((Value == Vulcanova.Uonet.Api.Lessons.JustificationStatus.Accepted || ( (Context == null) ? false : (Context.AbsenceJustified))) && DisplayForAccepted).ToVisibility();
        }


        public JustificationRequestStatus()
        {
            this.InitializeComponent();

            Requested.Visibility = (false).ToVisibility();
            Rejected.Visibility = (false).ToVisibility();
            Accepted.Visibility = (false).ToVisibility();
        }
    }
}
