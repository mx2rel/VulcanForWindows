using Microsoft.UI.Xaml;
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
using Vulcanova.Features.Messages;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VulcanForWindows.UserControls
{
    public sealed partial class MessageControl : UserControl, INotifyPropertyChanged
    {


        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register("Message", typeof(MessageViewModel), typeof(MessageControl), new PropertyMetadata(null, Message_Changed));

        public MessageViewModel Message
        {
            get => (MessageViewModel)GetValue(MessageProperty);
            set => SetValue(MessageProperty, value);
        }

        private static void Message_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MessageControl control && e.NewValue is MessageViewModel newValue)
            {
                control.OnPropertyChanged(nameof(Message));
            }
        }

        public MessageControl(MessageViewModel m)
        {
            Message= m;
            this.InitializeComponent();
        }
        public MessageControl()
        {
            this.InitializeComponent();
        }


        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
