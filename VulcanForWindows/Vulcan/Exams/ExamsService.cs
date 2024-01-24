using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using LiteDB.Async;
using VulcanForWindows.Vulcan;
using Vulcanova.Core.Uonet;
using Vulcanova.Features.Auth;
using Vulcanova.Features.Auth.Accounts;
using Vulcanova.Uonet.Api.Exams;
using VulcanTest.Vulcan;

namespace Vulcanova.Features.Exams;

public class ExamsService : UonetResourceProvider
{

    public async Task<NewResponseEnvelope<Exam>> GetExamsByDateRange(Account acc, DateTime from, DateTime to,
        bool forceSync = false, bool waitForSync = false)
    {

        var resourceKey = GetExamsResourceKey(acc, from, to);
        var items = await ExamsRepository.GetExamsForPupilAsync(acc.Id, from, to);


        var v = new NewResponseEnvelope<Exam>(FetchExamsAsync(acc,from,to), async delegate (object sender, IEnumerable<Exam> e)
        {
            SetJustSynced(resourceKey);
            await ExamsRepository.UpdateExamsForPupilAsync(acc.Id, e);

        });

        v.Entries.ReplaceAll(items);

        if (ShouldSync(resourceKey) || forceSync)
        {
            if (waitForSync)
                await v.Sync();
            else
                v.Sync();
        }
        return v;
    }

    private async Task<IEnumerable<Exam>> FetchExamsAsync(Account account, DateTime from, DateTime to)
    {
        var query = new GetExamsByPupilQuery(account.Unit.Id, account.Pupil.Id, from.AddDays(-60), 500);

        var client =  await new ApiClientFactory().GetAuthenticatedAsync(account);

        var response = await client.GetAsync(GetExamsByPupilQuery.ApiEndpoint, query);

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<ExamsMapperProfile>(); // Replace with your actual mapping profile class
        });

        IMapper mapper = mapperConfig.CreateMapper();


        var entries = response.Envelope.Select(mapper.Map<Exam>).ToArray();

        foreach (var entry in entries)
        {
            entry.Id.AccountId = account.Id;
        }

        return entries.Where(r=>r.Deadline >= from && r.Deadline <= to);
    }

    private static string GetExamsResourceKey(Account account, DateTime from, DateTime to)
        => $"Timetable_{account.Id}_{from.ToShortDateString()}_{to.ToLongDateString()}";

    public override TimeSpan OfflineDataLifespan => TimeSpan.FromHours(1);
}

public class ExamsRepository
{
    private static LiteDatabaseAsync _db => LiteDbManager.database;

    public static async Task<IEnumerable<Exam>> GetExamsForPupilAsync(int accountId, DateTime from, DateTime to)
    {
        return await _db.GetCollection<Exam>()
            .FindAsync(e => e.Id.AccountId == accountId
                            && e.Deadline >= from
                            && e.Deadline <= to);
    }

    public static async Task UpdateExamsForPupilAsync(int accountId, IEnumerable<Exam> entries)
    {
        await _db.GetCollection<Exam>().DeleteManyAsync(e => e.Id.AccountId == accountId);

        await _db.GetCollection<Exam>().UpsertAsync(entries);
    }
}