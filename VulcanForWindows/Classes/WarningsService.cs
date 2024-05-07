using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VulcanForWindows.Classes;
using Vulcanova.Features.Attendance.Report;
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
            acc = new AccountRepository().GetActiveAccount();
            return (new Warning[] { await GenerateGradesWarning(), await GenerateAttendanceWarning() }).Where(r=>r!=null).ToArray();
        }

        public async Task<Warning> GenerateGradesWarning()
        {
            var GradesResponse = await new GradesService().FetchGradesFromCurrentLevelAsync(acc);
            var FinalGradesResponse = await new FinalGrades().FetchPeriodGradesAsync(acc, acc.CurrentPeriod.Id);

            (Subject Key, double avg)[] failingGrades = GradesResponse.SelectMany(r => r.Value).GroupBy(r => r.Column.Subject)
                .Select(r => (r.Key, r.ToArray().CalculateAverage())).Where(r => r.Item2 < 1.75).ToArray();
            (Subject Key, int final)[] failingFinalGrades = FinalGradesResponse.Where(r => !string.IsNullOrEmpty(r.FinalGrade)).Where(r => int.TryParse(r.FinalGrade, out int _))
               .Select(r => (r.Subject, int.Parse(r.FinalGrade))).Where(r => r.Item2 < 2).ToArray();

            if (failingGrades.Length == 0 && failingFinalGrades.Length == 0) return null;

            (Subject s, bool isFinalFail)[] grouped = failingGrades.Where(r => !failingFinalGrades.Select(j => j.Key).ToList().Contains(r.Key)).Select(r => (r.Key, false)).ToArray()
            .Concat(failingFinalGrades.Select(r => (r.Key, true))).ToArray();

            // return failingGrades.GroupBy(r=>r.isFinalFail).Select(r=> new Severity(r.Key ? $"Nie zdajesz z "))
            return new Warning($"Nie zdajesz z {grouped.Length} przedmiotu/ów!",
            $"Twoja średnia lub oceny końcowe z: {string.Join(", ", grouped.Select(r => r.s.Name))} są zbyt niskie.",
            "GradesPage", null, Warning.Severity.Critical);
        }

        public async Task<Warning> GenerateAttendanceWarning()
        {
            var AttendanceResponse = await AttendanceReportService.GetReports(acc);
            var FailingAt = AttendanceResponse.Where(r => r.PresencePercentage < 50).ToArray();

            if (FailingAt.Length == 0) return null;

            return new Warning($"Masz za niską frekwencję z {FailingAt.Length} przedmiotów!",
                $"Twoja frekwencja jest niższa niż 50% z {string.Join(",", FailingAt.Select(r => r.Subject.Name))}.", "AttendanceReportPage", null, Warning.Severity.Critical);
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