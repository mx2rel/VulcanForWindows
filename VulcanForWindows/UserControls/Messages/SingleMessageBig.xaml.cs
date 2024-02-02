﻿using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
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
    public sealed partial class SingleMessageBig : UserControl, INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        }

        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register("Message", typeof(MessageViewModel), typeof(SingleMessageBig), new PropertyMetadata(null, Message_Changed));

        public MessageViewModel Message
        {
            get => (MessageViewModel)GetValue(MessageProperty);
            set => SetValue(MessageProperty, value);
        }

        private static void Message_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //if (d is SingleMessageBig control && e.NewValue is MessageViewModel newValue)
            //{
            //    control.OnPropertyChanged(nameof(Message));
            //}
        }
        void HoverOn(object sender, PointerRoutedEventArgs e)
        {
            Message.Hover = true;

        }
        void HoverOff(object sender, PointerRoutedEventArgs e)
        {
            Message.Hover = false;
        }
        public SingleMessageBig()
        {
            OnPropertyChanged(nameof(Message));
            this.InitializeComponent();
            AddHandler(PointerEnteredEvent, new PointerEventHandler(HoverOn), true);
            AddHandler(PointerExitedEvent, new PointerEventHandler(HoverOff), true);
        }

        private async void Clicked(object sender, RoutedEventArgs e)
        {
            ContentDialog dialog = new ContentDialog();
            Message.message.DateRead = DateTime.Now;
            dialog.XamlRoot = this.XamlRoot;
            var v = new MessageControl(Message);
            v.DataContext = Message;
            dialog.Content = v;
            dialog.CloseButtonText = "Zamknij";
            dialog.MinWidth = 600;
            dialog.MinHeight = 600;
            var result = await dialog.ShowAsync();
            Message.MarkAsRead();
            Message.OnPropertyChanged(nameof(Message.IsRead));
            Message.OnPropertyChanged(nameof(Message.DisplayColor));

        }

        private void MarkAsRead(object sender, RoutedEventArgs e)
        {
            Message.MarkAsRead();
            Message.message.DateRead = DateTime.Now;
            Message.OnPropertyChanged(nameof(Message.IsRead));
            Message.OnPropertyChanged(nameof(Message.DisplayColor));
        }

        private void Trash(object sender, RoutedEventArgs e)
        {
            Message.Trash();
        }
    }
}