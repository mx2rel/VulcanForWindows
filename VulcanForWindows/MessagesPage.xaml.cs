using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using VulcanForWindows.Vulcan;
using Vulcanova.Features.Auth;
using Vulcanova.Features.Messages;
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
    public sealed partial class MessagesPage : Page, INotifyPropertyChanged
    {
        public static MessagesPage instance;
        public MessagesPage()
        {
            this.InitializeComponent();
            instance = this;
            Fetch();
        }

        public ObservableCollection<MessageViewModel> Received { get; set; } = new ObservableCollection<MessageViewModel>();
        public ObservableCollection<MessageViewModel> Sent { get; set; } = new ObservableCollection<MessageViewModel>();
        public ObservableCollection<MessageViewModel> Trash { get; set; } = new ObservableCollection<MessageViewModel>();

        public ObservableCollection<MessageViewModel> GetCollection(int i)
        {
            switch (i)
            {
                case 0:
                    return Received;
                case 1:
                    return Sent;
                case 2:
                    return Trash;
            }

            return null;

        }
        public ObservableCollection<MessageViewModel> CurrentCollection
        {
            get => GetCollection(Pivot.SelectedIndex);
        }

        async void Fetch()
        {
            var acc = new AccountRepository().GetActiveAccountAsync();
            NewResponseEnvelope<MessageBox> v = await new MessageBoxesService().GetMessageBoxesByAccountId(acc, true, true);


            (NewResponseEnvelope<Message> r, NewResponseEnvelope<Message> s, NewResponseEnvelope<Message> t)
                = await MessagesService.GetMessagesStack(acc, v.entries.First().GlobalKey, false, true);

            Received.ReplaceAll(r.entries.Select(r => new MessageViewModel(r)).OrderByDescending(r => r.message.DateSent));
            Sent.ReplaceAll(s.entries.Select(r => new MessageViewModel(r)).OrderByDescending(r => r.message.DateSent));
            Trash.ReplaceAll(t.entries.Select(r => new MessageViewModel(r)).OrderByDescending(r => r.message.DateSent));


        }

        public bool MainCheckBoxChecked => CurrentCollection.ToArray().Where(r => r.IsSelected).Count() == CurrentCollection.Count && CurrentCollection.Count > 0;
        public bool EnableGroupActionsButtons => CurrentCollection.ToArray().Where(r => r.IsSelected).Count() > 0;

        private void MainChanged(object sender, RoutedEventArgs e)
        {
            if (checkbox.IsChecked.GetValueOrDefault())
                Select(SelectionCriteria.All);
            else
                Select(SelectionCriteria.None);

            //OnPropertyChanged(nameof(MainCheckBoxChecked));
        }
        private void MainUnchecked(object sender, RoutedEventArgs e)
        {
            Select(SelectionCriteria.None);
            //OnPropertyChanged(nameof(MainCheckBoxChecked));
        }

        public void SelectAll() => Select(SelectionCriteria.All);
        public void UnselectAll() => Select(SelectionCriteria.None);
        public void SelectRead() => Select(SelectionCriteria.Read);
        public void SelectUnread() => Select(SelectionCriteria.Unread);


        public void Select(SelectionCriteria criteria)
        {
           
            foreach (var v in CurrentCollection)
            {
                if (criteria == SelectionCriteria.None)
                    v.IsSelected = false;

                if (criteria == SelectionCriteria.All)
                    v.IsSelected = true;

                if (criteria == SelectionCriteria.Read && v.IsRead)
                    v.IsSelected = true;

                if (criteria == SelectionCriteria.Unread && !v.IsRead)
                    v.IsSelected = true;
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void SelectAll(object sender, RoutedEventArgs e) => SelectAll();

        private void UnselectAll(object sender, RoutedEventArgs e) => UnselectAll();

        private void SelectRead(object sender, RoutedEventArgs e) => SelectRead();

        private void SelectUnread(object sender, RoutedEventArgs e) => SelectUnread();

        private async void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var m = ((MessageViewModel)e.ClickedItem);
            ContentDialog dialog = new ContentDialog();
            dialog.XamlRoot = this.XamlRoot;
            var v = (Resources["MessagePopupContent"] as DataTemplate).LoadContent() as Grid;
            v.DataContext = m;
            dialog.Content = v;
            dialog.CloseButtonText = "Zamknij";
            var result = await dialog.ShowAsync();
        }

        int pivotPrevValue = 0;
        private void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(CurrentCollection));
            foreach (var v in GetCollection(pivotPrevValue))
                v.IsSelected = false;

            pivotPrevValue = Pivot.SelectedIndex;

        }

        private void ReadSelected(object sender, RoutedEventArgs e)
        {
            foreach (var v in GetCollection(pivotPrevValue).Where(r => r.IsSelected && !r.IsRead))
                v.MarkAsRead();
        }

        private void TrashSelected(object sender, RoutedEventArgs e)
        {
            foreach (var v in GetCollection(pivotPrevValue).Where(r => r.IsSelected))
                v.Trash();
        }
    }

    public class MessageViewModel : INotifyPropertyChanged
    {
        public MessageViewModel(Message m)
        {
            message = m;
        }
        public bool IsSelected
        {
            get => _IsSelected; set
            {
                _IsSelected = value;
                OnPropertyChanged(nameof(IsSelected));
                if (MessagesPage.instance == null) return;
                MessagesPage.instance.OnPropertyChanged(nameof(MessagesPage.MainCheckBoxChecked));
                MessagesPage.instance.OnPropertyChanged(nameof(MessagesPage.EnableGroupActionsButtons));
            }
        }

        public string PlainContent =>
    message?.Content?.Replace("\n", "")?.Replace("<.*?>", "") ?? string.Empty;

        public string DisplayContent =>
            ConvertHtmlToPlainText(message?.Content) ?? string.Empty;


        static string ConvertHtmlToPlainText(string html)
        {
            return html.Replace("</p></br>", "").Replace("</br></p>", "").Replace("<p>", "").Replace("</p>", "\n").Replace("<br>", "").Replace("</br>", "\n");
        }

        public string Receivers => string.Join(", ", message.Receiver.Select(r => r.Name));

        bool _IsSelected;
        public Message message;

        public async void Trash() => await new MessagesService().TrashMessage(message.MessageBoxId, message.Id);
        public async void MarkAsRead() => await new MessagesService().MarkMessageAsReadAsync(message.MessageBoxId, message.Id);

        public bool IsRead => message.DateRead != null;
        public bool Hover
        {
            get => _hover;
            set
            {
                _hover = value;
                OnPropertyChanged(nameof(Hover));
            }
        }
        bool _hover;


        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


    }

    public static class MessagesHelper
    {
        public static int NewCount(this IEnumerable<MessageViewModel> m) => NewCount(m.Select(r => r.message));
        public static int NewCount(this IEnumerable<Message> m) => m.Where(r => r.DateSent >= MainWindow.lastLaunch && r.DateRead == null).Count();
    }


    public enum SelectionCriteria
    {
        All, None, Read, Unread
    }
}
