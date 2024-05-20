using AutoMapper;
using LiteDB.Async;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VulcanForWindows;
using VulcanForWindows.Classes;
using VulcanForWindows.Preferences;
using VulcanForWindows.Vulcan;
using Vulcanova.Core.Uonet;
using Vulcanova.Features.Auth.Accounts;
using Vulcanova.Features.Shared;
using Vulcanova.Uonet.Api.Lessons;
using VulcanTest.Vulcan;
using VulcanTest.Vulcan.Attendance.Report;

namespace Vulcanova.Features.Attendance;

public class LessonsService : UonetResourceProvider
{
    public async Task GetLessonsForSchoolYear(Account acc, NewResponseEnvelope<Lesson> l)
        => await GetLessonsForRange(acc, acc.GetSchoolYearDuration().Start, acc.GetSchoolYearDuration().End, l, true, true);
    public async Task GetLessonsForRange(Account acc, DateTime from, DateTime to, NewResponseEnvelope<Lesson> l, bool startFromMostRecent = true, bool updateAsap = true, bool forceSync = false, bool waitForSync = false)
    {
        l.isLoadingOrUpdating = true;
        var total = new List<NewResponseEnvelope<Lesson>>();
        var lessons = new List<Lesson>();
        for (DateTime i = (startFromMostRecent ? to : from).StartOfTheMonth();
     (startFromMostRecent ? (i.Date.StartOfTheMonth() >= from.Date.StartOfTheMonth()) : (i.Date.StartOfTheMonth() <= to.Date.StartOfTheMonth()));
     i = i.AddMonths((startFromMostRecent ? (-1) : (1))))
        {
            var dt = i;
            var resourceKey = GetTimetableResourceKey(acc, dt);

            if (ShouldSync(resourceKey) || forceSync)
                total.Add(await GetLessonsByMonth(acc, dt, false, true));
            else
            {
                var c = await LessonsRepository.GetLessonsForAccountAsync(acc.Pupil.Id, dt);
                lessons = lessons.Concat(c).ToList();
            }

            Update();
        }

        var result = total.SelectMany(r => r.Entries).Concat(lessons);
        l.entries.ReplaceAll(result);
        l.isLoadingOrUpdating = false;
        l.SendUpdate();

        void Update()
        {
            var result = total.SelectMany(r => r.Entries).Concat(lessons);
            l.entries.ReplaceAll(result);
        l.SendUpdate();
        }
    }

    public async Task<NewResponseEnvelope<Lesson>> GetLessonsByMonth(Account account, DateTime monthAndYear, bool forceSync = false, bool waitForSync = false)
    {

        var resourceKey = GetTimetableResourceKey(account, monthAndYear);

        DateTime from;
        DateTime to;

        var hasPerformedFullSyncKey = $"Lessons_{account.Pupil.Id}_HasPerformedFullSync";

        var succes = PreferencesManager.TryGet<bool>(hasPerformedFullSyncKey, out var hasPerformedFullSync);

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

        var v = new NewResponseEnvelope<Lesson>(FetchEntriesForMonthAndYear(account, from, to), async delegate (object sender, IEnumerable<Lesson> e)
        {
            SetJustSynced(resourceKey);
            await LessonsRepository.UpsertLessonsForAccountAsync(e, account.Pupil.Id, monthAndYear);

        });

        var items = await LessonsRepository.GetLessonsForAccountAsync(account.Pupil.Id, monthAndYear);
        v.Entries.ReplaceAll(items);

        if (ShouldSync(resourceKey) || forceSync)
        {
            if (waitForSync)
                await v.Sync();
            else
                v.Sync();
        }

        PreferencesManager.Set<bool>(hasPerformedFullSyncKey, true);

        return v;
    }

    public static async Task<IEnumerable<Lesson>> FetchEntriesForMonthAndYear(Account account, DateTime from, DateTime to)
    {
        var query = new GetLessonsByPupilQuery(account.Pupil.Id, from, to, DateTime.MinValue, -2147483648, 2000);

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
            lesson.Id.PupilId = account.Pupil.Id;
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
        => $"Lessons_{account.Pupil.Id}_{monthAndYear.Month}_{monthAndYear.Year}";

    public override TimeSpan OfflineDataLifespan => TimeSpan.FromHours(1);
}

public static class LessonsRepository
{
    private static LiteDatabaseAsync _db => LiteDbManager.database;

    public static async Task<IEnumerable<Lesson>> GetLessonsForAccountAsync(int pupilId)
    {
        var v = await _db.GetCollection<Lesson>()
            .FindAsync(g =>
                g.Id.PupilId == pupilId);


        return v;
    }
    public static async Task<IEnumerable<Lesson>> GetLessonsForAccountAsync(int pupilid, DateTime monthAndYear)
    {

        //var s = DateTime.Now.Second + DateTime.Now.Millisecond * 0.01;

        var v = await _db.GetCollection<Lesson>()
            .FindAsync(g =>
                g.Id.PupilId == pupilid && g.Date.Year == monthAndYear.Year && g.Date.Month == monthAndYear.Month);

        //var t = DateTime.Now.Second + DateTime.Now.Millisecond * 0.01;

        //Debug.Write($"\n{t - s}\n");

        return v;
    }

    public static async Task<IEnumerable<Lesson>> GetLessonsBetweenAsync(int pupilId, DateTime start, DateTime end)
    {
        return await _db.GetCollection<Lesson>()
            .FindAsync(g => g.Id.PupilId == pupilId && g.Date >= start && g.Date <= end);
    }

    public static async Task UpsertLessonsForAccountAsync(IEnumerable<Lesson> entries, int pupilId, DateTime monthAndYear)
    {
        await _db.GetCollection<Lesson>()
            .DeleteManyAsync(g =>
                g.Date.Year == monthAndYear.Year && g.Date.Month == monthAndYear.Month && g.Id.PupilId == pupilId);

        await _db.GetCollection<Lesson>().UpsertAsync(entries);
    }
}