using LiteDB.Async;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;

namespace VulcanTest.Vulcan
{
    public static class LiteDbManager
    {
        public static LiteDatabaseAsync database = new LiteDatabaseAsync($"Filename={Path.Combine(Preferences.folder, ("main.db"))};Connection=shared;");
    }

    public static class Preferences
    {
        //C:\Users\Marcel\AppData\Local\Packages
        public static string folder = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "VulcanForWindows");
        private static string prefFolder = Path.Combine(folder, "Preferences");
        private static string GetCategoryPath(string category)
            => Path.Combine(folder, $"{(category ?? "main")}.dat");

        public static Windows.Storage.StorageFolder roamingFolder =
    Windows.Storage.ApplicationData.Current.RoamingFolder;


        public static void Set<T>(string key, T value)
            => Set(null, key, value);

        public static void Set<T>(string category, string key, T value)
        {
            Dictionary<string, string> data = GetAllData(category);
            data[key] = JsonConvert.SerializeObject((T)value);

            SaveData(category, data);
        }

        public static T Get<T>(string key)
            => Get<T>(null, key);
        public static T Get<T>(string category, string key)
        {
            Dictionary<string, string> data = GetAllData(category);
            return data.ContainsKey(key) ? JsonConvert.DeserializeObject<T>(data[key]) : default(T);
        }

        public static T Get<T>(string key, T defaultVal)
            => Get(null, key, defaultVal);

        public static T Get<T>(string category, string key, T defaultVal)
        {
            Dictionary<string, string> data = GetAllData(category);
            return data.ContainsKey(key) ? JsonConvert.DeserializeObject<T>(data[key]) : defaultVal;
        }


        public static bool TryGet<T>(string key, out T output)
            => TryGet<T>(null, key, out output);

        public static bool TryGet<T>(string category, string key, out T output)
        {
            Dictionary<string, string> data = GetAllData(category);

            if (data.ContainsKey(key))
            {
                output = JsonConvert.DeserializeObject<T>(data[key]);
                return true;
            }
            output = default(T);
            return false;

        }

        public static void Clear()
            => Clear(null);

        public static void Clear(string category)
        {
            SaveData(category, new Dictionary<string, string>());
        }

        public static Dictionary<(string category, string key), string> GetAllData()
        {
            IEnumerable<(string category, Dictionary<string, string> dict)> v = GetAllCategories().Select(r => (r, GetAllData(r)));

            var s = v
                .Select(category => category.dict.Select(e => ((category.category, e.Key), e.Value))).SelectMany(r => r)
                .ToDictionary(d => d.Item1, d=>d.Value);

            return s;
        }

        public static Dictionary<string, string> GetAllData(string category)
        {

            var filePath = GetCategoryPath(category);

            try
            {
                if (File.Exists(filePath))
                {
                    string json = File.ReadAllText(filePath);
                    var v = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                    if (v != null) return v;
                }
                else
                {
                    return new Dictionary<string, string>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error reading data: " + ex.Message);
                return new Dictionary<string, string>();
            }
            return new Dictionary<string, string>();
        }

        private static void SaveData(string category, Dictionary<string, string> data)
        {
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            if (!Directory.Exists(prefFolder)) Directory.CreateDirectory(prefFolder);

            var filePath = GetCategoryPath(category);

            if (!File.Exists(filePath))
            {
                var s = File.Create(filePath);
                Debug.WriteLine($"Created {s.Name}");
                s.Close();
            }
            try
            {
                string json = JsonConvert.SerializeObject(data, Formatting.Indented);
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("\nError saving data: " + ex.Message);
            }
        }

        public static string[] GetAllCategories()
            => Directory.GetFileSystemEntries(prefFolder);
    }

    public static class AppWide
    {
        public static string AppVersion => string.Format("{0}.{1}.{2}.{3}",
                    Package.Current.Id.Version.Major,
                    Package.Current.Id.Version.Minor,
                    Package.Current.Id.Version.Build,
                    Package.Current.Id.Version.Revision);
    }

    public class Preference
    {
        public string Category { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }

        public Preference(string _category,string _name, string _value)
        {
            Category = _category;
            Name = _name; 
            Value = _value;
        }
    }

    public static class PreferencesHelpers
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

    public static class ObservableCollectionExtensions
    {
        public static void ReplaceAll<T>(this ObservableCollection<T> collection, IEnumerable<T> newItems)
        {
            if (collection.Count > 0)
                collection.Clear();

            foreach (var item in newItems.Where(r => r != null))
            {
                collection.Add(item);
            }

        }
    }
    public static class DateTimeHelper
    {
        public static DateTime StartOfTheMonth(this DateTime dt)
        {
            return dt.Date.AddDays(-(dt.Day - 1));

        }
    }
}
