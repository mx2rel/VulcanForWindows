using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using LiteDB.Async;
using VulcanForWindows.Vulcan.Grades.Final;
using Vulcanova.Core.Uonet;
using Vulcanova.Features.Auth;
using Vulcanova.Features.Auth.Accounts;
using Vulcanova.Features.Shared;
using Vulcanova.Uonet.Api.Grades;
using VulcanTest.Vulcan;

namespace Vulcanova.Features.Grades.Final;

public class FinalGrades : UonetResourceProvider
{

    public async Task<FinalGradesResponseEnvelope> GetPeriodGrades(Account account, int periodId,
        bool forceSync = false, bool waitForSync = false)
    {
        var resourceKey = GetGradesSummaryResourceKey(account, periodId);
        var v = new FinalGradesResponseEnvelope(this, account, periodId, resourceKey);
        
        v.Grades = new ObservableCollection<FinalGradesEntry>(await FinalGradesRepository.GetFinalGradesForPupilAsync(account.Id, account.Pupil.Id,
            periodId));

        if (ShouldSync(resourceKey) || forceSync)
        {
            if (waitForSync)
                await v.SyncAsync();
            else
                v.SyncAsync();
        }

        return v;
    }

    public async Task<FinalGradesEntry[]> FetchPeriodGradesAsync(Account account, int periodId)
    {
        var query = new GetGradesSummaryByPupilQuery(account.Unit.Id, account.Pupil.Id, periodId, 500);

        var client = await new ApiClientFactory().GetAuthenticatedAsync(account);

        var response = await client.GetAsync(GetGradesSummaryByPupilQuery.ApiEndpoint, query);

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<GradeMapperProfile>(); // Replace with your actual mapping profile class
        });

        IMapper mapper = mapperConfig.CreateMapper();

        var domainGrades = response.Envelope?.Select(mapper.Map<FinalGradesEntry>).ToArray();

        if (domainGrades == null)
        {
            return Array.Empty<FinalGradesEntry>();
        }

        foreach (var grade in domainGrades)
        {
            grade.AccountId = account.Id;
        }

        return domainGrades;
    }

    public async Task<IDictionary<Period, FinalGradesEntry[]>> FetchGradesFromAllPeriodsAsync(Account account)
    {
        IDictionary<Period, FinalGradesEntry[]> d = new Dictionary<Period, FinalGradesEntry[]>();
        //Console.WriteLine(JsonConvert.SerializeObject(account.Periods));
        foreach (var period in account.Periods)
        {
            d.Add(period, (await GetPeriodGrades(account, period.Id,false,true)).Grades.ToArray());
        }

        return d;
    }

    private static string GetGradesSummaryResourceKey(Account account, int periodId)
        => $"GradesSummary_{account.Id}_{account.Pupil.Id}_{periodId}";

    public override TimeSpan OfflineDataLifespan => TimeSpan.FromHours(1);
}

public static class FinalGradesRepository
{
    private static LiteDatabaseAsync _db => LiteDbManager.database;
    public static IDictionary<string, IEnumerable<FinalGradesEntry>> buffer = new Dictionary<string, IEnumerable<FinalGradesEntry>>();


    public static async Task<IEnumerable<FinalGradesEntry>> GetFinalGradesForPupilAsync(int accountId, int pupilId, int periodId)
    {
        string code = $"{accountId}.{pupilId}.{periodId}";

        if (buffer.TryGetValue(code, out var d))
        {
            return d;
        }
        var v = (await _db.GetCollection<FinalGradesEntry>()
                .FindAsync(g => g.PupilId == pupilId && g.AccountId == accountId && g.PeriodId == periodId))
            .OrderBy(g => g.Subject.Name);
        buffer[code] = v;

        return v;
    }

    public static async Task UpdatePupilFinalGradesAsync(IEnumerable<FinalGradesEntry> newGrades)
    {
        await _db.GetCollection<FinalGradesEntry>().UpsertAsync(newGrades);
    }
}