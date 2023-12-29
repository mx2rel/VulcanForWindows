using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using LiteDB.Async;
using Newtonsoft.Json;
using Vulcanova.Core.Uonet;
using Vulcanova.Features.Attendance.Report;
using Vulcanova.Features.Auth;
using Vulcanova.Features.Auth.Accounts;
using Vulcanova.Features.Shared;
using Vulcanova.Uonet.Api;
using Vulcanova.Uonet.Api.Lessons;
using Vulcanova.Uonet.Api.Presence;
using VulcanTest.Vulcan;
using VulcanTest.Vulcan.Attendance.Report;

namespace Vulcanova.Features.Attendance;

public class LessonsService : UonetResourceProvider
{

    public IObservable<IEnumerable<Lesson>> GetLessonsByMonth(Account account, DateTime monthAndYear, bool forceSync = false)
    {
        return Observable.Create<IEnumerable<Lesson>>(async observer =>
        {

            var resourceKey = GetTimetableResourceKey(account, monthAndYear);

            var items = await LessonsRepository.GetLessonsForAccountAsync(account.Id, monthAndYear);

            observer.OnNext(items);

            if (ShouldSync(resourceKey) || forceSync)
            {
                DateTime from;
                DateTime to;

                var hasPerformedFullSyncKey = $"Lessons_{account.Id}_HasPerformedFullSync";

                var succes = Preferences.TryGet<bool>(hasPerformedFullSyncKey, out var hasPerformedFullSync);

                if (!hasPerformedFullSync || !succes)
                {
                    (from, to) = account.GetSchoolYearDuration();
                }
                else
                {
                    from = new DateTime(monthAndYear.Year, monthAndYear.Month, 1);
                    to = new DateTime(monthAndYear.Year, monthAndYear.Month,
                        DateTime.DaysInMonth(from.Year, from.Month));
                }

                var onlineEntries = await FetchEntriesForMonthAndYear(account, from, to);

                await LessonsRepository.UpsertLessonsForAccountAsync(onlineEntries, account.Id, monthAndYear);

                if (!hasPerformedFullSync)
                {
                    Preferences.Set<bool>(hasPerformedFullSyncKey, true);
                }

                SetJustSynced(resourceKey);

                items = await LessonsRepository.GetLessonsForAccountAsync(account.Id, monthAndYear);

                observer.OnNext(items);

                await AttendanceReportService.InvalidateReportsAsync(account);
            }

            observer.OnCompleted();
        });
    }

    public static async Task<Lesson[]> FetchEntriesForMonthAndYear(Account account, DateTime from, DateTime to)
    {
        var query = new GetLessonsByPupilQuery(account.Pupil.Id, from, to, DateTime.MinValue);

        var client = await new ApiClientFactory().GetAuthenticatedAsync(account);

        var response = await client.GetAsync(GetLessonsByPupilQuery.ApiEndpoint, query);

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<AttendanceMapperProfile>(); // Replace with your actual mapping profile class
        });

        IMapper mapper = mapperConfig.CreateMapper();

        var lessons = response.Envelope.Select(mapper.Map<Lesson>).ToArray();

        foreach (var lesson in lessons)
        {
            lesson.Id.AccountId = account.Id;
        }

        return lessons;
    }

    public static async Task SubmitAbsenceJustification(Account account, int lessonClassId, string message)
    {
        //var request = new JustifyLessonRequest(message, lessonClassId, account.Pupil.Id, account.Login.Id);

        //var client = await new ApiClientFactory().GetAuthenticatedAsync(account);

        //await client.PostAsync(JustifyLessonRequest.ApiEndpoint, request);
    }

    private static string GetTimetableResourceKey(Account account, DateTime monthAndYear)
        => $"Lessons_{account.Id}_{monthAndYear.Month}_{monthAndYear.Year}";

    public override TimeSpan OfflineDataLifespan => TimeSpan.FromHours(1);
}

public static class LessonsRepository
{
    private static LiteDatabaseAsync _db => LiteDbManager.database;

    public static async Task<IEnumerable<Lesson>> GetLessonsForAccountAsync(int accountId, DateTime monthAndYear)
    {
        return await _db.GetCollection<Lesson>()
            .FindAsync(g =>
                g.Id.AccountId == accountId && g.Date.Year == monthAndYear.Year && g.Date.Month == monthAndYear.Month);
    }

    public static async Task<IEnumerable<Lesson>> GetLessonsBetweenAsync(int accountId, DateTime start, DateTime end)
    {
        return await _db.GetCollection<Lesson>()
            .FindAsync(g => g.Id.AccountId == accountId && g.Date >= start && g.Date <= end);
    }

    public static async Task UpsertLessonsForAccountAsync(IEnumerable<Lesson> entries, int accountId, DateTime monthAndYear)
    {
        await _db.GetCollection<Lesson>()
            .DeleteManyAsync(g =>
                g.Date.Year == monthAndYear.Year && g.Date.Month == monthAndYear.Month && g.Id.AccountId == accountId);

        await _db.GetCollection<Lesson>().UpsertAsync(entries);
    }
}