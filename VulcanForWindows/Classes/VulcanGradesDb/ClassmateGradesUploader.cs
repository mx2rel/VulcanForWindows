using LiteDB.Async;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Vulcanova.Core.Uonet;
using Vulcanova.Features.Auth;
using Vulcanova.Features.Grades;
using VulcanTest.Vulcan;

namespace VulcanForWindows.Classes.VulcanGradesDb
{
    public static class ClassmateGradesUploader
    {
        static int userid;
        static ILiteCollectionAsync<ClassmateGradesSyncObject> DbEntries;
        private static LiteDatabaseAsync _db => LiteDbManager.database;

        public static DateTime GetLastSync(int PeriodId)
        {
            return Preferences.Get<DateTime>($"LastSync_ClassmatesGrades_{PeriodId}", DateTime.MinValue);
        }

        public static void SetJustSynced(int PeriodId)
        {
            Preferences.Set<DateTime>($"LastSync_ClassmatesGrades_{PeriodId}", DateTime.Now);
        }

        public async static void UpsyncGrades(Grade[] grades, int PeriodId)
        {
            //if (DateTime.Now - GetLastSync(PeriodId) < new TimeSpan(0, 2, 0)) return;
            Debug.WriteLine("Upsyncing grades");
            SetJustSynced(PeriodId);
            userid = (new AccountRepository().GetActiveAccountAsync()).Pupil.Id;
            DbEntries = LiteDbManager.database.GetCollection<ClassmateGradesSyncObject>();
            var syncObjects = (await DbEntries.FindAllAsync())
                .ToDictionary(r => r.ColumnId, r => r.Synced);
            foreach (var grade in grades.Where(r => r.Column.Weight > 0).Where(r => r.Value.HasValue))
            {
                if (syncObjects.TryGetValue(grade.Column.Id, out var synced))
                {

                    if (synced <= grade.DateModify) SyncGrade(grade);
                }
                else
                {
                    SyncGrade(grade);
                }
            }

        }

        public async static void SyncGrade(Grade grade)
        {
            //string url = $"http://localhost:3205/UploadGrade/{grade.Column.Id}/{grade.Value}/{userid}";
            var baseUrl = Properties.Resources.ClassmatesGradesServerUrl;
            Debug.WriteLine(baseUrl);

            string url = $"{baseUrl}/UploadGrade/{grade.Column.Id}/{grade.Value}/{userid}";
            var succes = await VisitUrlInBackground(url);
            if (succes) await DbEntries.InsertAsync(new ClassmateGradesSyncObject(grade.Column.Id, DateTime.Now));
        }


        static async Task<bool> VisitUrlInBackground(string url)
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
                    return true;
                }
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Error visiting URL: {e.Message} \n {url}");
                return false;
            }
        }


    }
}
