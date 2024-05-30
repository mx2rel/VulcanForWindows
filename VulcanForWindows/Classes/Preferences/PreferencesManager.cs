using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace VulcanForWindows.Preferences
{
    public static class PreferencesManager
    {
        //C:\Users\Marcel\AppData\Local\Packages\mx2rel.VulcanDlaWindows_rnfa7nf5b04yc
        static bool createdFolderPath = false;
        static bool createdPrefFolderPath = false;
        static List<string> createdCategories = new List<string>();
        public static string folder
        {
            get
            {
                var path = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "Vulcanoid");
                if (!createdFolderPath)
                {
                    if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                    createdFolderPath = true;
                }
                return path;
            }
        }
        private static string prefFolder
        {
            get
            {
                var path = Path.Combine(folder, "Preferences");
                if (!createdPrefFolderPath)
                {
                    if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                    createdPrefFolderPath = true;
                }
                return path;
            }
        }

        private static string GetCategoryPath(string category)
        {
            category = (string.IsNullOrEmpty(category) ? ("main") : category);
            var path = Path.Combine(prefFolder, $"{category}.txt");
            if (!createdCategories.Contains(category))
            {
                if (!File.Exists(path))
                {
                    var sw = File.CreateText(path);
                    sw.Close();
                }
                createdCategories.Add(category);
            }
            return path;
        }


        public static void Set<T>(string key, T value)
            => Set(null, key, value);

        public static void Set<T>(string category, string key, T value)
        {
            Dictionary<string, string> data = GetAllData(category);
            data[key] = JsonConvert.SerializeObject(value);

            SaveData(category, data);
        }

        public static T Get<T>(string key)
            => Get<T>(null, key);
        public static T Get<T>(string category, string key)
        {
            Dictionary<string, string> data = GetAllData(category);
            return data.ContainsKey(key) ? JsonConvert.DeserializeObject<T>(data[key]) : default;
        }

        public static T Get<T>(string key, T defaultVal)
            => Get(null, key, defaultVal);

        public static T Get<T>(string category, string key, T defaultVal)
        {
            Dictionary<string, string> data = GetAllData(category);
            return data.ContainsKey(key) ? JsonConvert.DeserializeObject<T>(data[key]) : defaultVal;
        }


        public static bool TryGet<T>(string key, out T output)
            => TryGet(null, key, out output);

        public static bool TryGet<T>(string category, string key, out T output)
        {
            Dictionary<string, string> data = GetAllData(category);

            if (data.ContainsKey(key))
            {
                output = JsonConvert.DeserializeObject<T>(data[key]);
                return true;
            }
            output = default;
            return false;

        }

        public static void Clear()
            => Clear(null);

        public static void Clear(string category)
            => SaveData(category, new Dictionary<string, string>());

        public static void WipeFolder()
        {
            if (Directory.Exists(folder))
                Directory.Delete(folder, true);
        }

        public static Dictionary<(string category, string key), string> GetAllData()
        {
            IEnumerable<(string category, Dictionary<string, string> dict)> v = GetAllCategories().Select(r => (r, GetAllData(r)));

            var s = v
                .Select(category => category.dict.Select(e => ((category.category, e.Key), e.Value))).SelectMany(r => r)
                .ToDictionary(d => d.Item1, d => d.Value);

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
                Debug.WriteLine($"Created {filePath}");
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
}
