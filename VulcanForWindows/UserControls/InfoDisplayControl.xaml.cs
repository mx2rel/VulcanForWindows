﻿using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VulcanForWindows.UserControls
{
    public sealed partial class InfoDisplayControl : UserControl
    {


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
                control.BodyText.Text = newValue;
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
