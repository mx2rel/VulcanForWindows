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
using Vulcanova.Uonet.Api;
using Vulcanova.Uonet.Api.Grades;
using VulcanTest.Vulcan;

namespace Vulcanova.Features.Grades;

public class AverageGrades : UonetResourceProvider
{
    public IObservable<IEnumerable<AverageGrade>> GetAverageGrades(Account account, int periodId, bool forceSync = false)
    {
        return Observable.Create<IEnumerable<AverageGrade>>(async observer =>
        {

            var normalGradesResourceKey = GetAverageGradesResourceKey(account, periodId);

            var items = await AverageGradesRepository.GetAverageGradesForPupilAsync(account.Id, account.Pupil.Id,
                periodId);

            observer.OnNext(items);

            if (ShouldSync(normalGradesResourceKey) || forceSync)
            {
                var onlineGrades = await FetchAverageGradesAsync(account, periodId);

                await AverageGradesRepository.UpdatePupilAverageGradesAsync(onlineGrades);

                SetJustSynced(normalGradesResourceKey);

                items = await AverageGradesRepository.GetAverageGradesForPupilAsync(account.Id, account.Pupil.Id,
                    periodId);

                observer.OnNext(items);
            }

            observer.OnCompleted();
        });
    }
    public async Task<AverageGrade[]> FetchAverageGradesAsync(Account account) => await FetchAverageGradesAsync(account, account.CurrentPeriod.Id);
    public async Task<AverageGrade[]> FetchAverageGradesAsync(Account account, int periodId)
    {
        var client = await new ApiClientFactory().GetAuthenticatedAsync(account);

        var averageGradesQuery =
            new GetAverageGradesByPupilQuery(account.Unit.Id, account.Pupil.Id, periodId, 500);

        var averageGrades = client.GetAllAsync(GetAverageGradesByPupilQuery.ApiEndpoint,
            averageGradesQuery);

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<GradeMapperProfile>(); // Replace with your actual mapping profile class
        });

        IMapper mapper = mapperConfig.CreateMapper();

        var domainGrades = await averageGrades
            .Select(mapper.Map<AverageGrade>)
            .ToArrayAsync();

        foreach (var grade in domainGrades)
        {
            grade.AccountId = account.Id;
        }

        return domainGrades;
    }

    private static string GetAverageGradesResourceKey(Account account, int periodId)
        => $"AverageGrades_{account.Id}_{account.Pupil.Id}_{periodId}";

    public override TimeSpan OfflineDataLifespan => TimeSpan.FromMinutes(15);
}

public static class AverageGradesRepository
{
    private static LiteDatabaseAsync _db => LiteDbManager.database;

    public static async Task<IEnumerable<AverageGrade>> GetAverageGradesForPupilAsync(int accountId, int pupilId, int periodId)
    {
        return await _db.GetCollection<AverageGrade>()
            .FindAsync(g => g.PupilId == pupilId && g.AccountId == accountId && g.PeriodId == periodId);
    }

    public static async Task UpdatePupilAverageGradesAsync(IEnumerable<AverageGrade> newGrades)
    {
        await _db.GetCollection<AverageGrade>().UpsertAsync(newGrades);
    }
}