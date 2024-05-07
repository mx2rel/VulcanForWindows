using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VulcanForWindows.UserControls
{
    public sealed partial class InfoDisplayControl : UserControl, INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public static readonly DependencyProperty MoodProperty =
            DependencyProperty.Register("Mood", typeof(Moods), typeof(InfoDisplayControl), new PropertyMetadata(null, Mood_Changed));

        public Moods Mood
        {
            get => (Moods)GetValue(MoodProperty);
            set => SetValue(MoodProperty, value);
        }

        private static void Mood_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is InfoDisplayControl control && e.NewValue is Moods newValue)
            {
                control.MoodImage.Source = new BitmapImage(new Uri($"ms-appx:///Assets/Pupil/{newValue}.png"));
            }
        }


        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register("Header", typeof(string), typeof(InfoDisplayControl), new PropertyMetadata(null, Header_Changed));

        public string Header
        {
            get => (string)GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        private static void Header_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is InfoDisplayControl control && e.NewValue is string newValue)
            {
                control.HeaderText.Text = newValue;
            }
        }


        public static readonly DependencyProperty BodyProperty =
            DependencyProperty.Register("Body", typeof(string), typeof(InfoDisplayControl), new PropertyMetadata(null, Body_Changed));

        public string Body
        {
            get => (string)GetValue(BodyProperty);
            set => SetValue(BodyProperty, value);
        }

        private static void Body_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is InfoDisplayControl control && e.NewValue is string newValue)
            {
                control.BodyText.Visibility = Visibility.Visible;
                control.BodyText.Text = newValue;
                control.OnPropertyChanged(nameof(ShowBody));
                if(control.FrameworkBody != null)
                {
                    control.bodyContainer.Children.Remove(control.FrameworkBody);
                    control.FrameworkBody = null;
                }
            }
        }


        public static readonly DependencyProperty TeachingTipTextProperty =
            DependencyProperty.Register("TeachingTipText", typeof(string), typeof(InfoDisplayControl), new PropertyMetadata(null, TeachingTipText_Changed));

        public string TeachingTipText
        {
            get => (string)GetValue(TeachingTipTextProperty);
            set => SetValue(TeachingTipTextProperty, value);
        }

        private static void TeachingTipText_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is InfoDisplayControl control && e.NewValue is string newValue)
            {
                control.OnPropertyChanged(nameof(DisplayInfoButton));
                control.TeachingTip.Subtitle = newValue;
            }
        }


        public static readonly DependencyProperty TeachingTipTitleProperty =
            DependencyProperty.Register("TeachingTipTitle", typeof(string), typeof(InfoDisplayControl), new PropertyMetadata(null, TeachingTipTitle_Changed));

        public string TeachingTipTitle
        {
            get => (string)GetValue(TeachingTipTitleProperty);
            set => SetValue(TeachingTipTitleProperty, value);
        }

        private static void TeachingTipTitle_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is InfoDisplayControl control && e.NewValue is string newValue)
            {

                control.OnPropertyChanged(nameof(DisplayInfoButton));
                control.TeachingTip.Title = newValue;
            }
        }


        public static readonly DependencyProperty FrameworkBodyProperty =
            DependencyProperty.Register("FrameworkBody", typeof(FrameworkElement), typeof(InfoDisplayControl), new PropertyMetadata(null, FrameworkBody_Changed));

        public FrameworkElement FrameworkBody
        {
            get => (FrameworkElement)GetValue(FrameworkBodyProperty);
            set => SetValue(FrameworkBodyProperty, value);
        }

        private static void FrameworkBody_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is InfoDisplayControl control && e.NewValue is FrameworkElement newValue)
            {
                control.BodyText.Visibility = Visibility.Collapsed;
                if(newValue is TextBlock textBlock)
                {
                    textBlock.TextAlignment = TextAlignment.Center;
                    textBlock.TextWrapping = TextWrapping.WrapWholeWords;
                }
                control.bodyContainer.Children.Add(newValue);
            }
        }



        public bool DisplayInfoButton => !string.IsNullOrEmpty(TeachingTipText) || !string.IsNullOrEmpty(TeachingTipTitle);
        public bool ShowBody => !string.IsNullOrEmpty(Body);


        private void FontIcon_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            TeachingTip.IsOpen = true;
        }

        private void FontIcon_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            TeachingTip.IsOpen = false;

        }


        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register("Orientation", typeof(Orientation), typeof(InfoDisplayControl), new PropertyMetadata(Orientation.Vertical, Orientation_Changed));

        public Orientation Orientation
        {
            get => (Orientation)GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }

        private static void Orientation_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is InfoDisplayControl control && e.NewValue is Orientation newValue)
            {
                control.MainSp.Orientation = newValue;
            }
        }


        public enum Moods
        {
            Happy, Sad, Excited, Normal, Bored
        }

        public InfoDisplayControl()
        {
            this.InitializeComponent();
        }
    }
}
