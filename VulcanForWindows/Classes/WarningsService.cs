using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VulcanForWindows.Classes;
using Vulcanova.Features.Auth;
using Vulcanova.Features.Auth.Accounts;
using Vulcanova.Features.Grades;
using Vulcanova.Features.Grades.Final;
using Vulcanova.Features.Grades.Summary;
using Vulcanova.Features.Shared;

namespace VulcanForWindows.Warnings
{
    public class WarningsService
    {
        static Account acc;
        public async Task<Warning[]> Generate()
        {
            acc = new AccountRepository().GetActiveAccountAsync();
            return await GenerateGradesWarning();
        }

        public async Task<Warning[]> GenerateGradesWarning()
        {
            var GradesResponse = await new GradesService().FetchGradesFromCurrentLevelAsync(acc);
            var FinalGradesResponse = await new FinalGrades().FetchPeriodGradesAsync(acc, acc.CurrentPeriod.Id);

            (Subject Key, double avg)[] failingGrades = GradesResponse.SelectMany(r => r.Value).GroupBy(r => r.Column.Subject)
                .Select(r => (r.Key, r.ToArray().CalculateAverage())).Where(r => r.Item2 < 1.75).ToArray();
            //(r.=Subject, float final)[] failingFinalGrades = FinalGradesResponse.Where(r=>!string.IsNullOrEmpty(r.FinalGrade))
            //    .Select(r => (r.Subject, float.Parse(r.FinalGrade))).Where(r => r.Item2 < 2).ToArray();

            return failingGrades.Select(r => new Warning($"Nie zdajesz z przedmiotu {r.Key.Name}!", $"Twoja średnia wynosi {r.avg}.", "", null, Warning.Severity.Critical)).ToArray();
        }
    }

    public class Warning
    {
        public enum Severity
        {
            Notice, Warning, Critical
        }

        public Warning(string title, string description, string moveToPage, object highlight, Severity warningSeverity)
        {
            Title = title;
            Description = description;
            MoveToPage = moveToPage;
            Highlight = highlight;
            WarningSeverity = warningSeverity;
        }

        public string Title { get; set; }
        public string Description { get; set; }
        public string MoveToPage { get; set; }
        public object Highlight { get; set; }
        public Severity WarningSeverity { get; set; }

    }
}
