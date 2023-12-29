using System;
using VulcanTest.Vulcan;

namespace Vulcanova.Core.Uonet
{
    public abstract class UonetResourceProvider
    {
        public abstract TimeSpan OfflineDataLifespan { get; }

        public bool ShouldSync(string resourceKey)
        {
            var lastSync = GetLastSync(resourceKey);
            return DateTime.UtcNow - lastSync > OfflineDataLifespan;
        }

        public static DateTime GetLastSync(string resourceKey)
        {

            if (Preferences.TryGet<DateTime>(resourceKey, out var output))
                return output;

            return DateTime.MinValue;
        }

        public static void SetJustSynced(string resourceKey)
        {
            Preferences.Set<DateTime>($"LastSync_{resourceKey}", DateTime.Now);
        }
    }
}
