﻿using Microsoft.UI.Xaml.Controls;
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

namespace VulcanForWindows.Classes
{
    public static class AnnouncementsManager
    {
        static ContentDialog prev = null;

        public static async void Update(FrameworkElement root)
        {
            var relevantAnnouncement = await AnnouncementsService.GetAllRelevant(AppWide.AppVersion);
            DisplayNewPopups(relevantAnnouncement, root);
        }

        public async static void DisplayNewPopups(IEnumerable<Announcement> relevantAnnouncements,FrameworkElement root)
        {
            await Task.Delay(1000);
            var announcementsToShow = relevantAnnouncements.Where(r => r.ShowAsPopup).Where(r=>!r.PopupOnlyOnce || PreferencesManager.Get<bool>("announcements", $"{r.ID}_viewed", false) == false).ToList();

            announcementsToShow = announcementsToShow
                .GroupBy(r => r.Priority).OrderByDescending(r => r.Key).Select(r => r.OrderBy(r => r.SentOn)).SelectMany(r => r).ToList();

            if (announcementsToShow.Count == 0) return;

            for (int i = announcementsToShow.Count - 1; i >= 0; i--)
            {
                var dialog = await GetContentDialog(announcementsToShow[i], root, false);
                var lAnn = announcementsToShow[i];
                var lPrev = prev;
                if (prev != null)
                    dialog.Closed += async delegate 
                    { 
                        
                        await lPrev.ShowAsync();

                        if(lAnn.PopupOnlyOnce)
                            PreferencesManager.Set<bool>("announcements", $"{lAnn.ID}_viewed", true);
                    };

                prev = dialog;
            }

            await prev.ShowAsync();

            prev = null;

        }


        public async static Task<ContentDialog> GetContentDialog(Announcement announcement, FrameworkElement fe, bool showInstantly = true)
        {
            ContentDialog dialog = new ContentDialog();

            // XamlRoot must be set in the case of a ContentDialog running in a Desktop app
            dialog.XamlRoot = fe.XamlRoot;
            dialog.Title = announcement.Title;
            dialog.CloseButtonText = "Zamknij";
            var sp = new StackPanel();
            dialog.Content = sp;

            if (announcement.HasHeaderImg)
            {
                var headerImg = new Image();
                headerImg.Source = new BitmapImage(new Uri(announcement.HeaderImg));
                sp.Children.Add(headerImg);
            }

            sp.Children.Add(MdConverter.ConvertMarkdownToStackPanel(announcement.MdContent));

            if (showInstantly)
                await dialog.ShowAsync();

            return dialog;
        }

    }
}
