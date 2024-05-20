using System.Collections.Generic;
using System.Linq;

namespace VulcanForWindows.Preferences
{
    public static class PreferencesExtensions
    {
        public static IEnumerable<Preference> ToPreferences(this IDictionary<string, string> data)
        {
            return data.Select(r => new Preference(null, r.Key, r.Value));
        }
        public static IEnumerable<Preference> ToPreferences(this IDictionary<(string, string), string> data)
        {
            return data.Select(r => new Preference(r.Key.Item1, r.Key.Item2, r.Value));
        }
    }
}
