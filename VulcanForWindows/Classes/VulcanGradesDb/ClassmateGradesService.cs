using AutoMapper;
using LiteDB.Async;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using VulcanForWindows.Classes.VulcanGradesDb.Models;
using Vulcanova.Core.Uonet;
using VulcanTest.Vulcan;

namespace VulcanForWindows.Classes.VulcanGradesDb
{
    public class ClassmateGradesService : UonetResourceProvider
    {
        private static LiteDatabaseAsync _db => LiteDbManager.database;

        static string GetResourceKey(int ColumnId) => $"ClassmateColumn_{ColumnId}";
        public static async Task<SingleClassmateColumn> GetSingleClassmateColumn(int ColumnId, bool forceSync = true)
        {
            if (new ClassmateGradesService().ShouldSync(GetResourceKey(ColumnId)) || forceSync)
            {
                var baseUrl = await CredentialsReader.ReadCredentialAsync<string>("ClassmatesGradesServerUrl");
                var str = await RetrieveData($"{baseUrl}/Get/{ColumnId}");

                if (str == "null") return null;

                if (JObject.Parse(str).TryGetValue("Data", out var o))
                {
                    var data = JsonConvert.DeserializeObject<SingleClassmateGrade[]>(o.ToObject<string>());
                    var response = new SingleClassmateColumn(ColumnId, data);
                    await _db.GetCollection<SingleClassmateColumn>().InsertAsync(response);
                    SetJustSynced(GetResourceKey(ColumnId));
                    return response;
                }

                return null;
            }
            else
            {
                var response = await _db.GetCollection<SingleClassmateColumn>().FindAsync(r => r.ColumnId == ColumnId);
                if (response.Count() == 0) return null;
                return response.First();
            }
        }

        public override TimeSpan OfflineDataLifespan => new TimeSpan(0, 10, 0);

        static async Task<string> RetrieveData(string url)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    // Make an HTTP GET request to the specified URL
                    HttpResponseMessage response = await client.GetAsync(url);

                    // Check if the response is successful (status code 200-299)
                    response.EnsureSuccessStatusCode();

                    // Optionally, you can read the response content if needed
                    // string responseBody = await response.Content.ReadAsStringAsync();
                    return await response.Content.ReadAsStringAsync();
                }
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Error visiting URL: {e.Message} \n {url}");
                return "";
            }
        }
    }
}
