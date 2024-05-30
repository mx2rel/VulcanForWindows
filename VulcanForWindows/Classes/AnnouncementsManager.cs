using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VulcanoidServerClient.Models;
using VulcanoidServerClient.Services.Announcements;
using VulcanTest.Vulcan;
using System.Diagnostics;
using Microsoft.UI.Xaml.Media.Imaging;
using VulcanoidServerClient;
using MarkdownToWinUi3.MdToWinUi;
using VulcanForWindows.Preferences;
using VulcanoidServerClient.Api;

namespace VulcanForWindows.Classes
{
    public static class AnnouncementsManager
    {
        static ContentDialog prev = null;

        public static async Task Update(FrameworkElement root, StackPanel infosPanel)
        {
            EndpointsHandler.overrideServer = PreferencesManager.Get("Settings", "AnnouncementsServerUrl", "");
            Debug.WriteLine("OVERRIDE:" + EndpointsHandler.server);
            var relevantAnnouncement = await AnnouncementsService.GetAllRelevant(AppWide.AppVersion);

            DisplayInfos(relevantAnnouncement, infosPanel);
            DisplayNewPopups(relevantAnnouncement, root);
        }

        public static void DisplayInfos(IEnumerable<Announcement> relevantAnnouncements, StackPanel infosPanel)
        {

            relevantAnnouncements = relevantAnnouncements.Where(r =>
            r.Type == VulcanoidServerClient.Api.Payloads.Announcements.AnnouncementType.Info
            || r.Type == VulcanoidServerClient.Api.Payloads.Announcements.AnnouncementType.Warning
            || r.Type == VulcanoidServerClient.Api.Payloads.Announcements.AnnouncementType.Error).OrderBy(r => r.Priority);

            relevantAnnouncements =
                relevantAnnouncements.Where(r => (r.InfoBarOnlyOnce && !PreferencesManager.Get<bool>("announcements", $"{r.ID}_viewed", false)) || !r.InfoBarOnlyOnce);

            foreach (var announcement in relevantAnnouncements)
            {
                var i = new InfoBar();
                switch (announcement.Type)
                {
                    case VulcanoidServerClient.Api.Payloads.Announcements.AnnouncementType.Info:
                        i.Severity = InfoBarSeverity.Informational; break;

                    case VulcanoidServerClient.Api.Payloads.Announcements.AnnouncementType.Warning:
                        i.Severity = InfoBarSeverity.Warning; break;

                    case VulcanoidServerClient.Api.Payloads.Announcements.AnnouncementType.Error:
                        i.Severity = InfoBarSeverity.Error; break;
                }
                i.Title = announcement.Title;
                if (!string.IsNullOrEmpty(announcement.MdContent))
                    i.Content = MdConverter.ConvertMarkdownToStackPanel(announcement.MdContent);
                i.IsOpen = true;
                if (!string.IsNullOrEmpty(announcement.ButtonText))
                {
                    i.ActionButton = new HyperlinkButton { Content = announcement.ButtonText, NavigateUri = new Uri(announcement.ButtonUrl) };
                }
                infosPanel.Children.Add(i);
                if (announcement.InfoBarOnlyOnce)
                    PreferencesManager.Set<bool>("announcements", $"{announcement.ID}_viewed", true);
            }
        }

        public async static void DisplayNewPopups(IEnumerable<Announcement> relevantAnnouncements, FrameworkElement root)
        {

            await Task.Delay(500);

            var announcementsToShow = relevantAnnouncements.Where(r => r.ShowAsPopup).Where(r => (r.PopupOnlyOnce && !PreferencesManager.Get<bool>("announcements", $"{r.ID}_popup_viewed", false)) || !r.PopupOnlyOnce).ToList();

            announcementsToShow = announcementsToShow
                .GroupBy(r => r.Priority).OrderByDescending(r => r.Key).Select(r => r.OrderBy(r => r.SentOn)).SelectMany(r => r).ToList();

            if (announcementsToShow.Count == 0) return;
            Announcement last = null;
            for (int i = announcementsToShow.Count - 1; i >= 0; i--)
            {
                var dialog = await GetContentDialog(announcementsToShow[i], root, false);
                var lAnn = announcementsToShow[i];
                var lPrev = prev;
                if (prev != null)
                    dialog.Closed += async delegate
                    {
                        if (lAnn.PopupOnlyOnce)
                            PreferencesManager.Set<bool>("announcements", $"{lAnn.ID}_popup_viewed", true);

                        await lPrev.ShowAsync();
                    };
                last = lAnn;
                prev = dialog;
            }

            await prev.ShowAsync();
            if (last != null)
                if (last.PopupOnlyOnce)
                    PreferencesManager.Set<bool>("announcements", $"{last.ID}_popup_viewed", true);
            prev = null;
            return;
        }


        public async static Task<ContentDialog> GetContentDialog(Announcement announcement, FrameworkElement fe, bool showInstantly = true)
        {
            ContentDialog dialog = new ContentDialog();

            // XamlRoot must be set in the case of a ContentDialog running in a Desktop app
            dialog.XamlRoot = fe.XamlRoot;
            dialog.Title = announcement.Title;
            dialog.CloseButtonText = "Zamknij";
            var sv = new ScrollView();
            var sp = new StackPanel();
            sv.Content = sp;
            dialog.Content = sv;

            if (announcement.HasHeaderImg)
            {
                var headerImg = new Image();
                headerImg.Source = new BitmapImage(new Uri(announcement.HeaderImg));
                sp.Children.Add(headerImg);
            }

            sp.Children.Add(MdConverter.ConvertMarkdownToStackPanel(announcement.MdContent));

            var bsp = new GridView();
            bsp.SelectionMode = ListViewSelectionMode.None;
            sp.Children.Add(bsp);

            foreach (var b in announcement.Buttons)
            {
                var btn = new Button();
                btn.Margin = new Thickness(0,0,3,3);
                btn.Content = b.Display;
                btn.Click += (object sender, RoutedEventArgs e) =>
                {
                    MainWindow.NavigateTo(b.Path);

                    if (announcement.PopupOnlyOnce)
                        PreferencesManager.Set<bool>("announcements", $"{announcement.ID}_popup_viewed", false);

                    dialog.Hide();
                };
                bsp.Items.Add(btn);
            }

            if (showInstantly)
                await dialog.ShowAsync();

            return dialog;
        }
    }
}
