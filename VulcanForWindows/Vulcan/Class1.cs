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

namespace VulcanTest.Vulcan
{
    public static class LiteDbManager
    {
        private static string defaultFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "main.txt");
        public static LiteDatabaseAsync database = new LiteDatabaseAsync("Filename=main.db;Connection=shared;");

    }

    public static class Preferences
    {
        private static string folder = Path.Combine( Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "VulcanForWindows");
        private static string dataFilePath
            => Path.Combine(folder, "data.dat");

        public static Windows.Storage.StorageFolder roamingFolder =
    Windows.Storage.ApplicationData.Current.RoamingFolder;


        public static void Set<T>(string key, T value)
        {
            Dictionary<string, string> data = GetAllData();
            data[key] = JsonConvert.SerializeObject((T)value);

            SaveData(data);
        }

        public static T Get<T>(string key)
        {
            Dictionary<string, string> data = GetAllData();
            return data.ContainsKey(key) ? JsonConvert.DeserializeObject<T>(data[key]) : default(T);
        }
        public static T Get<T>(string key, T defaultVal)
        {
            Dictionary<string, string> data = GetAllData();
            return data.ContainsKey(key) ? JsonConvert.DeserializeObject<T>(data[key]) : defaultVal;
        }


        public static bool TryGet<T>(string key, out T output)
        {
            Dictionary<string, string> data = GetAllData();

            if(data.ContainsKey(key))
            {
                output = JsonConvert.DeserializeObject<T>(data[key]);
                return true;
            }
            output = default(T);
            return false;

        }

        public static void Clear()
        {
            SaveData(new Dictionary<string, string>());
        }

        private static Dictionary<string, string> GetAllData()
        {
            try
            {
                if (File.Exists(dataFilePath))
                {
                    string json = File.ReadAllText(dataFilePath);
                    var v= JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
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

        private static void SaveData(Dictionary<string, string> data)
        {
            if (!File.Exists(dataFilePath))
            {
                var s = File.Create(dataFilePath);
                Debug.WriteLine($"Created {s.Name}");
            }
            try
            {
                string json = JsonConvert.SerializeObject(data, Formatting.Indented);
                File.WriteAllText(dataFilePath, json);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("\nError saving data: " + ex.Message);
            }
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
            return dt.Date.AddDays(-(dt.Day-1));

        }
    }
}
