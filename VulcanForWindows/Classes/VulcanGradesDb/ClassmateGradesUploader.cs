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
        private static LiteDatabaseAsync _db => LiteDbManager.database;

        public static DateTime GetGeneralLastSent(int PeriodId)
        {
            return Preferences.Get<DateTime>($"LastSync_ClassmatesGrades_{PeriodId}", DateTime.MinValue);
        }

        public static void SetGeneralJustSynced(int PeriodId)
        {
            Preferences.Set<DateTime>($"LastSync_ClassmatesGrades_{PeriodId}", DateTime.Now);
        }


        public static DateTime GetGradeLastSent(int GradeId)
        {
            return Preferences.Get<DateTime>($"LastSync_ClassmatesGrades_Grade_Sent_{GradeId}", DateTime.MinValue);
        }

        public static void SetGradeJustSent(int GradeId)
        {
            Preferences.Set<DateTime>($"LastSync_ClassmatesGrades_Grade_Sent_{GradeId}", DateTime.Now);
        }

        public async static void UpsyncGrades(Grade[] grades, int PeriodId)
        {
            if (DateTime.Now - GetGeneralLastSent(PeriodId) < new TimeSpan(0, 2, 0)) return;
            Debug.WriteLine("Upsyncing grades");
            SetGeneralJustSynced(PeriodId);
            userid = (new AccountRepository().GetActiveAccount()).Pupil.Id;
            foreach (var grade in grades.Where(r => r.Column.Weight > 0).Where(r => r.ActualValue.HasValue))
            {
                UpsyncGrade(grade);
            }

        }

        public async static void UpsyncGrade(Grade grade)
        {

            if (GetGradeLastSent(grade.Id) > grade.DateModify) return;

            var baseUrl = Properties.Resources.ClassmatesGradesServerUrl;

            if (grade.ActualValue == null || grade.Column.Weight == 0) return;

            string url = $"{baseUrl}/UploadGrade/{grade.Column.Id}/{grade.ActualValue}/{userid}";
            var succes = await VisitUrlInBackground(url);
            if (succes)
                SetGradeJustSent(grade.Id);
        }


        static async Task<bool> VisitUrlInBackground(string url)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();
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
