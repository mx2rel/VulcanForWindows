using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using AutoMapper;
using LiteDB.Async;
using Vulcanova.Core.Uonet;
using Vulcanova.Features.Auth;
using Vulcanova.Features.Auth.Accounts;
using Vulcanova.Uonet.Api.Grades;
using VulcanTest.Vulcan;

namespace Vulcanova.Features.Grades.Final;

public class FinalGrades : UonetResourceProvider
{

    public IObservable<IEnumerable<FinalGradesEntry>> GetPeriodGrades(Account account, int periodId,
        bool forceSync = false)
    {
        return Observable.Create<IEnumerable<FinalGradesEntry>>(async observer =>
        {

            var resourceKey = GetGradesSummaryResourceKey(account, periodId);

            var items = await FinalGradesRepository.GetFinalGradesForPupilAsync(account.Id, account.Pupil.Id,
                periodId);

            observer.OnNext(items);

            if (ShouldSync(resourceKey) || forceSync)
            {
                var onlineGrades = await FetchPeriodGradesAsync(account, periodId);

                await FinalGradesRepository.UpdatePupilFinalGradesAsync(onlineGrades);

                SetJustSynced(resourceKey);

                items = await FinalGradesRepository.GetFinalGradesForPupilAsync(account.Id, account.Pupil.Id,
                    periodId);

                observer.OnNext(items);
            }

            observer.OnCompleted();
        });
    }
    public async Task<FinalGradesEntry[]> FetchGradesAsync(Account account) => await FetchPeriodGradesAsync(account, account.CurrentPeriod.Id);

    public async Task<FinalGradesEntry[]> FetchPeriodGradesAsync(Account account, int periodId)
    {
        var query = new GetGradesSummaryByPupilQuery(account.Unit.Id, account.Pupil.Id, periodId, 500);

        var client =  await new ApiClientFactory().GetAuthenticatedAsync(account);

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

    private static string GetGradesSummaryResourceKey(Account account, int periodId)
        => $"GradesSummary_{account.Id}_{account.Pupil.Id}_{periodId}";

    public override TimeSpan OfflineDataLifespan => TimeSpan.FromHours(1);
}

public static class FinalGradesRepository
{
    private static LiteDatabaseAsync _db => LiteDbManager.database;


    public static async Task<IEnumerable<FinalGradesEntry>> GetFinalGradesForPupilAsync(int accountId, int pupilId, int periodId)
    {
        return (await _db.GetCollection<FinalGradesEntry>()
                .FindAsync(g => g.PupilId == pupilId && g.AccountId == accountId && g.PeriodId == periodId))
            .OrderBy(g => g.Subject.Name);
    }

    public static async Task UpdatePupilFinalGradesAsync(IEnumerable<FinalGradesEntry> newGrades)
    {
        await _db.GetCollection<FinalGradesEntry>().UpsertAsync(newGrades);
    }
}