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
using Vulcanova.Uonet.Api.MessageBox;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VulcanForWindows.UserControls
{
    public sealed partial class AttachmentsIndicator : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        }

        public static readonly DependencyProperty AttachmentsProperty =
            DependencyProperty.Register("Attachments", typeof(List<Attachment>), typeof(AttachmentsIndicator), new PropertyMetadata(new List<Attachment>(), Attachments_Changed));

        public List<Attachment> Attachments
        {
            get => (List<Attachment>)GetValue(AttachmentsProperty);
            set
            {
                SetValue(AttachmentsProperty, value);

                OnPropertyChanged(nameof(Attachments));
                OnPropertyChanged(nameof(HasAttachments));
                OnPropertyChanged(nameof(AttachmentsText));
                OnPropertyChanged(nameof(AttachmentsTooltip));
            }
        }

        public bool HasAttachments => Attachments.Count > 0;
        public string AttachmentsText => $"{Attachments.Count} {(Attachments.Count == 1 ? "załącznik" : ((Attachments.Count > 4) ? "załączników" : "załączniki"))}";
        public string AttachmentsTooltip => string.Join(", ", Attachments.Select(r => r.Name));

        private static void Attachments_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is AttachmentsIndicator control && e.NewValue is string newValue)
            {
            }
        }

        public AttachmentsIndicator()
        {
            this.InitializeComponent();
        }
    }
}
