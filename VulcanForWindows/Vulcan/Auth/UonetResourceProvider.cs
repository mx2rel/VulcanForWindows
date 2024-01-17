using System;
using System.Diagnostics;
using VulcanTest.Vulcan;

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
            if (Preferences.TryGet<DateTime>($"LastSync_{resourceKey}", out var output))
                return output;

            return DateTime.MinValue;
        }

        public static void SetJustSynced(string resourceKey)
        {
            Preferences.Set<DateTime>($"LastSync_{resourceKey}", DateTime.Now);
        }
    }
}
