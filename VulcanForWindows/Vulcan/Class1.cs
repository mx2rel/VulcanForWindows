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
        private static string dataFilePath
            = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "data.dat");


        public static void Set<T>(string key, object value)
        {
            Dictionary<string, string> data = GetAllData();
            data[key] = JsonConvert.SerializeObject((T)value);

            SaveData(data);
        }

        public static T Get<T>(string key)
        {
            Dictionary<string, string> data = GetAllData();
            //if (!data.ContainsKey(key))
            //    Debug.Write($"\nCouldn't find key {key}\n{JsonConvert.SerializeObject(data)}\n");
            return data.ContainsKey(key) ? JsonConvert.DeserializeObject<T>(data[key]) : default(T);
        }


        public static bool TryGet<T>(string key, out T output)
        {
            Dictionary<string, string> data = GetAllData();

            //if (!data.ContainsKey(key))
            //    Debug.Write($"\nCouldn't find key {key}\n{JsonConvert.SerializeObject(data)}\n");
            try
            {
                output = data.ContainsKey(key) ? JsonConvert.DeserializeObject<T>(data[key]) : default(T);
            }
            catch
            {
                output = default(T);
                return false;
            }

            return data.ContainsKey(key);
        }

        private static Dictionary<string, string> GetAllData()
        {
            try
            {
                if (File.Exists(dataFilePath))
                {
                    string json = File.ReadAllText(dataFilePath);
                    return JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
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
        }

        private static void SaveData(Dictionary<string, string> data)
        {
            try
            {
                string json = JsonConvert.SerializeObject(data, Formatting.Indented);
                File.WriteAllText(dataFilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error saving data: " + ex.Message);
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
