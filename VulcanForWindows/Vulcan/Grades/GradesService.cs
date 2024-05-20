using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using LiteDB.Async;
using Newtonsoft.Json;
using VulcanForWindows.Classes;
using VulcanForWindows.Extensions;
using VulcanForWindows.Vulcan;
using VulcanForWindows.Vulcan.Grades;
using Vulcanova.Core.Uonet;
using Vulcanova.Features.Attendance;
using Vulcanova.Features.Auth;
using Vulcanova.Features.Auth.Accounts;
using Vulcanova.Features.Shared;
using Vulcanova.Uonet.Api;
using Vulcanova.Uonet.Api.Grades;
using VulcanTest.Vulcan;

namespace Vulcanova.Features.Grades;

public class GradesService : UonetResourceProvider
{

    public async Task<GradesResponseEnvelope> GetPeriodGrades(Account account, int periodId, bool forceSync = false, bool waitForSync = false)
    {
        var normalGradesResourceKey = GetGradesResourceKey(account, periodId);
        var behaviourGradesResourceKey = GetBehaviourGradesResourceKey(account, periodId);

        var v = new GradesResponseEnvelope(this, account, periodId, normalGradesResourceKey, behaviourGradesResourceKey);


        v.Grades = new ObservableCollection<Grade>(await GradesRepository.GetPeriodGradesForPupilAsync(account.Pupil.Id,
            periodId));


        if (ShouldSync(normalGradesResourceKey) || ShouldSync(behaviourGradesResourceKey) || forceSync)
        {
            if (waitForSync)
                await v.SyncAsync();
            else
                v.SyncAsync();

        }
        else
        {
            v.isLoaded = true;
        }
        return v;
    }

    public async Task<IEnumerable<Grade>> GetPeriodGradesV2(Account account, int periodId, bool forceSync = false)
    {
        var normalGradesResourceKey = GetGradesResourceKey(account, periodId);
        var behaviourGradesResourceKey = GetBehaviourGradesResourceKey(account, periodId);

        if (ShouldSync(normalGradesResourceKey) || ShouldSync(behaviourGradesResourceKey) || forceSync)
        {
            var onlineGrades = await FetchPeriodGradesAsync(account, periodId);

            await GradesRepository.UpdatePupilGradesAsync(onlineGrades);

            GradesService.SetJustSynced(normalGradesResourceKey);
            GradesService.SetJustSynced(behaviourGradesResourceKey);
            return onlineGrades;
        }

        return await GradesRepository.GetPeriodGradesForPupilAsync(account.Pupil.Id, periodId);

    }

    public async Task<NewResponseEnvelope<Grade>> GetPeriodGradesV3(Account account, int periodId, EventHandler<IEnumerable<Grade>> OnUpdated = null, bool forceSync = false, bool waitForSync = false)
    {
        var normalGradesResourceKey = GetGradesResourceKey(account, periodId);
        var behaviourGradesResourceKey = GetBehaviourGradesResourceKey(account, periodId);

        var rEnvelope = new NewResponseEnvelope<Grade>(await GradesRepository.GetPeriodGradesForPupilAsync(account.Pupil.Id,
            periodId), FetchPeriodGradesAsync(account, periodId), async delegate (object sender, IEnumerable<Grade> g)
        {
            SetJustSynced(normalGradesResourceKey);
            SetJustSynced(behaviourGradesResourceKey);
            await GradesRepository.UpdatePupilGradesAsync(g);

        }, OnUpdated, true);


        if (ShouldSync(normalGradesResourceKey) || ShouldSync(behaviourGradesResourceKey) || forceSync)
        {
            if (waitForSync)
                await rEnvelope.Sync();
            else
                rEnvelope.Sync();

        }

        return rEnvelope;
    }
    public async Task<NewResponseEnvelope<Grade>> GetYearGradesV3(Account account, int yearId, EventHandler<IEnumerable<Grade>> OnUpdated = null, bool forceSync = false, bool waitForSync = false)
    {
        var normalGradesResourceKey = GetGradesResourceKey(account, yearId, "year");
        var behaviourGradesResourceKey = GetBehaviourGradesResourceKey(account, yearId, "year");

        var rEnvelope = new NewResponseEnvelope<Grade>(await GradesRepository.GetYearGradesForPupilAsync(account.Pupil.Id,
            yearId), FetchGradesFromLevelOneIEnumerableAsync(account,yearId), async delegate (object sender, IEnumerable<Grade> g)
        {
            SetJustSynced(normalGradesResourceKey);
            SetJustSynced(behaviourGradesResourceKey);
            await GradesRepository.UpdatePupilGradesAsync(g);

        }, OnUpdated, true);


        if (ShouldSync(normalGradesResourceKey) || ShouldSync(behaviourGradesResourceKey) || forceSync)
        {
            if (waitForSync)
                await rEnvelope.Sync();
            else
                rEnvelope.Sync();

        }

        return rEnvelope;
    }

    public async Task<IDictionary<Period, Grade[]>> FetchGradesFromAllPeriodsAsync(Account account)
    {
        IDictionary<Period, Grade[]> d = new Dictionary<Period, Grade[]>();
        //Console.WriteLine(JsonConvert.SerializeObject(account.Periods));
        foreach (var period in account.Periods)
        {
            d.Add(period, (await GetPeriodGrades(account, period.Id, false, true)).Grades.ToArray());
        }

        return d;
    }

    public async Task<IDictionary<Period, Grade[]>> FetchGradesFromLevelAsync(Account account, int periodId)
    {
        IDictionary<Period, Grade[]> d = new Dictionary<Period, Grade[]>();
        //Console.WriteLine(JsonConvert.SerializeObject(account.Periods));
        foreach (var period in account.Periods.Where(r => r.GetFirstPeriodOfLevel().Id == PeriodExtenstions.GetSchoolYearId(periodId)))
        {
            d.Add(period, (await GetPeriodGrades(account, period.Id, false, true)).Grades.ToArray());
        }

        return d;
    }
    public async Task<IEnumerable< Grade>> FetchGradesFromLevelOneIEnumerableAsync(Account account, int periodId)
    {
        return (await FetchGradesFromLevelAsync(account, periodId)).Select(r => r.Value).SelectMany(r => r);
    }

    public async Task<IDictionary<Period, Grade[]>> FetchGradesFromCurrentLevelAsync(Account account)
    {
        return await FetchGradesFromLevelAsync(account, account.CurrentPeriod.Id);
    }

    public async Task<IEnumerable<Grade>> FetchGradesFromCurrentPeriodAsync(Account account)
        => await FetchPeriodGradesAsync(account, account.CurrentPeriod.Id);

    public async Task<IEnumerable<Grade>> FetchPeriodGradesAsync(Account account, int periodId)
    {
        var client = await new ApiClientFactory().GetAuthenticatedAsync(account);

        var normalGradesLastSync = GetLastSync(GetGradesResourceKey(account, periodId));

        var normalGradesQuery =
            new GetGradesByPupilQuery(account.Unit.Id, account.Pupil.Id, periodId, normalGradesLastSync, 500);

        var normalGrades = client.GetAllAsync(GetGradesByPupilQuery.ApiEndpoint,
            normalGradesQuery);

        var behaviourGradesLastSync = GetLastSync(GetGradesResourceKey(account, periodId));

        var behaviourGradesQuery =
            new GetBehaviourGradesByPupilQuery(account.Unit.Id, account.Pupil.Id, periodId, behaviourGradesLastSync,
                500);

        var behaviourGrades = client.GetAllAsync(GetBehaviourGradesByPupilQuery.ApiEndpoint,
            behaviourGradesQuery);
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<GradeMapperProfile>(); // Replace with your actual mapping profile class
        });

        IMapper mapper = mapperConfig.CreateMapper();


        var domainGrades = await normalGrades
            .Concat(behaviourGrades)
            .Select(mapper.Map<Grade>)
            .ToArrayAsync();

        foreach (var grade in domainGrades)
        {
            grade.AccountId = account.Id;
        }

        return domainGrades;
    }

    private static string GetGradesResourceKey(Account account, int periodId, string add = "")
        => $"Grades_{account.Id}_{account.Pupil.Id}{(string.IsNullOrEmpty(add) ? "" : $"_{add}")}_{periodId}";

    private static string GetBehaviourGradesResourceKey(Account account, int periodId, string add = "")
        => $"BehaviourGrades_{account.Id}_{account.Pupil.Id}{(string.IsNullOrEmpty(add) ? "" : $"_{add}")}_{periodId}";

    public override TimeSpan OfflineDataLifespan => TimeSpan.FromMinutes(15);
}

public static class GradesRepository
{
    private static LiteDatabaseAsync _db => LiteDbManager.database;

    public static IDictionary<string, IEnumerable<Grade>> buffer = new Dictionary<string, IEnumerable<Grade>>();

    public static async Task<IEnumerable<Grade>> GetPeriodGradesForPupilAsync(int pupilId, int periodId)
    {
        string code = $"{pupilId}.{periodId}";

        if (buffer.TryGetValue(code, out var d))
        {
            return d;
        }

        var v = (await LiteDbManager.database.GetCollection<Grade>()
                .FindAsync(g => g.PupilId == pupilId && g.Column.PeriodId == periodId))
            .OrderBy(g => g.Column.Subject.Name)
            .ThenBy(g => g.DateCreated);

        buffer[code] = v;

        return v;
    }
    public static async Task<IEnumerable<Grade>> GetYearGradesForPupilAsync(int pupilId, int periodId)
    {
        periodId = PeriodExtenstions.GetSchoolYearId(periodId);
        var allIds = PeriodExtenstions.GetSchoolYearAllIds(periodId);
        string code = $"{pupilId}.{periodId}";

        if (buffer.TryGetValue(code, out var d))
        {
            return d;
        }

        var v = (await LiteDbManager.database.GetCollection<Grade>()
                .FindAsync(g => g.PupilId == pupilId && allIds.Contains(g.Column.PeriodId)))
            .OrderBy(g => g.Column.Subject.Name)
            .ThenBy(g => g.DateCreated);

        buffer[code] = v;

        return v;
    }

    public static async Task UpdatePupilGradesAsync(IEnumerable<Grade> newGrades)
    {
        await LiteDbManager.database.GetCollection<Grade>().UpsertAsync(newGrades);
    }
}