using LiteDB.Async;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vulcanova.Core.Uonet;
using Vulcanova.Features.Auth;
using Vulcanova.Features.Auth.Accounts;
using Vulcanova.Features.Shared;
using VulcanTest.Vulcan;

namespace Vulcanova.Features.Attendance.Report;

public class AttendanceReportService : UonetResourceProvider
{

    private static string GetReportResourceKey(Account account)
        => $"AttendanceReport_{account.Id}";

    public override TimeSpan OfflineDataLifespan => TimeSpan.FromDays(1);

    public static async Task UpdateIfNeeded(Account account) => await new AttendanceReportService().UpdateIfNeededPrivate(account);
    public static async Task ForceUpdate(Account account) => await new AttendanceReportService().InvalidateReportsAsync(account);

    async Task UpdateIfNeededPrivate(Account account)
    {
        var r = GetReportResourceKey(account);
        if (ShouldSync(r))
            await InvalidateReportsAsync(account);
    }

    async Task InvalidateReportsAsync(Account account)
    {
        int accountId = account.Id;
        var (yearStart, yearEnd) = account.GetSchoolYearDuration();

        var entries = (await LessonsRepository.GetLessonsBetweenAsync(accountId, yearStart, yearEnd))
            .Where(l => l.PresenceType != null && l.CalculatePresence && l.Subject != null)
            .ToArray();

        var entriesBySubject = entries
            .GroupBy(e => e.Subject.Id);

        var generationDate = DateTime.UtcNow;

        var reports = entriesBySubject.Select(g =>
        {
            var subject = g.First().Subject;

            return new AttendanceReport
            {
                Id = $"{accountId}/{subject.Id}",
                AccountId = accountId,
                DateGenerated = generationDate,
                Absence = g.Count(x => x.PresenceType.Absence),
                Late = g.Count(x => x.PresenceType.Late),
                Presence = g.Count(x => x.PresenceType.Presence && !x.PresenceType.Late),
                Subject = subject
            };
        });

        var reportsArray = reports.ToArray();

        await AttendanceReportRepository.UpdateAttendanceReportsAsync(accountId, reportsArray);
        var r = GetReportResourceKey(account);
        SetJustSynced(r);
    }
}

public static class AttendanceReportRepository
{
    private static LiteDatabaseAsync _db => LiteDbManager.database;

    public static async Task<IEnumerable<AttendanceReport>> GetAttendanceReportsAsync(Account account)
    {
        try
        {
            var maxDate = await _db.GetCollection<AttendanceReport>()
                .MaxAsync(r => r.DateGenerated);

            return await _db.GetCollection<AttendanceReport>()
                .FindAsync(r => r.AccountId == account.Id && r.DateGenerated == maxDate);
        }
        catch (LiteAsyncException e) when (e.InnerException is InvalidOperationException { Message: "Sequence contains no elements" })
        {
            return Array.Empty<AttendanceReport>();
        }
    }

    public static async Task UpdateAttendanceReportsAsync(int accountId, ICollection<AttendanceReport> reports)
    {
        await _db.GetCollection<AttendanceReport>()
            .UpsertAsync(reports);

        await _db.GetCollection<AttendanceReport>()
            .DeleteManyAsync(x => x.AccountId == accountId && !reports.Select(r => r.Id).Contains(x.Id));
    }
}

public record AttendanceReportUpdatedEvent(int AccountId, float OverallAttendancePercentage) : UonetDataUpdatedEvent;