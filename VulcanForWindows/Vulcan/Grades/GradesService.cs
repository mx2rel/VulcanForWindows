using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using LiteDB.Async;
using Newtonsoft.Json;
using VulcanForWindows.Vulcan.Grades;
using Vulcanova.Core.Uonet;
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


        v.Grades = new ObservableCollection<Grade>(await GradesRepository.GetGradesForPupilAsync(account.Id, account.Pupil.Id,
            periodId));

        if (ShouldSync(normalGradesResourceKey) || ShouldSync(behaviourGradesResourceKey) || forceSync)
        {
            if (waitForSync)
                await v.SyncAsync();
            else
                v.SyncAsync();

        }
        return v;
    }

    public async Task<IDictionary<int, Grade[]>> FetchGradesFromAllPeriodsAsync(Account account)
    {
        IDictionary<int, Grade[]> d = new Dictionary<int, Grade[]>();
        //Console.WriteLine(JsonConvert.SerializeObject(account.Periods));
        foreach (var period in account.Periods)
        {
            d.Add(period.Id, await FetchPeriodGradesAsync(account, period.Id));
        }

        return d;
    }

    public async Task<Grade[]> FetchGradesFromCurrentPeriodAsync(Account account)
        => await FetchPeriodGradesAsync(account, account.CurrentPeriod.Id);

    public async Task<Grade[]> FetchPeriodGradesAsync(Account account, int periodId)
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

    private static string GetGradesResourceKey(Account account, int periodId)
        => $"Grades_{account.Id}_{account.Pupil.Id}_{periodId}";

    private static string GetBehaviourGradesResourceKey(Account account, int periodId)
        => $"BehaviourGrades_{account.Id}_{account.Pupil.Id}_{periodId}";

    public override TimeSpan OfflineDataLifespan => TimeSpan.FromMinutes(15);
}

public static class GradesRepository
{
    private static LiteDatabaseAsync _db => LiteDbManager.database;

    public static async Task<IEnumerable<Grade>> GetGradesForPupilAsync(int accountId, int pupilId, int periodId)
    {
        return (await _db.GetCollection<Grade>()
                .FindAsync(g => g.PupilId == pupilId && g.AccountId == accountId && g.Column.PeriodId == periodId))
            .OrderBy(g => g.Column.Subject.Name)
            .ThenBy(g => g.DateCreated);
    }

    public static async Task UpdatePupilGradesAsync(IEnumerable<Grade> newGrades)
    {
        await _db.GetCollection<Grade>().UpsertAsync(newGrades);
    }
}