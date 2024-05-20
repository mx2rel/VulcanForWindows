using System;
using System.Diagnostics;
using VulcanForWindows.Preferences;

namespace Vulcanova.Core.Uonet
{
    public abstract class UonetResourceProvider
    {
        public abstract TimeSpan OfflineDataLifespan { get; }

        public bool ShouldSync(string resourceKey)
        {
            var lastSync = GetLastSync(resourceKey);

            //Debug.Write($"\n{resourceKey}\n:Last sync: {lastSync}\n{DateTime.UtcNow - lastSync} ago\n{DateTime.UtcNow - lastSync > OfflineDataLifespan}\n");

            return DateTime.UtcNow - lastSync > OfflineDataLifespan;
        }

        public static DateTime GetLastSync(string resourceKey)
        {
            return PreferencesManager.Get<DateTime>($"LastSync_{resourceKey}", DateTime.MinValue);
        }

        public static void SetJustSynced(string resourceKey)
        {
            PreferencesManager.Set<DateTime>($"LastSync_{resourceKey}", DateTime.Now);
        }
    }
}
