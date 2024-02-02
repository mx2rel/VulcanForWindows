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
using Vulcanova.Uonet.Api;
using Vulcanova.Uonet.Api.Homework;
using VulcanTest.Vulcan;

namespace Vulcanova.Features.Homework
{
    public class HomeworkService : UonetResourceProvider
    {
        public async Task<NewResponseEnvelope<Homework>> GetHomework(Account acc, int periodId,
            bool forceSync = false, bool waitForSync = false)
        {
            var resourceKey = GetHomeworkResourceKey(acc, periodId);
            var items = await HomeworksRepository.GetHomeworkForPupilAsync(acc.Id);

            var v = new NewResponseEnvelope<Homework>(FetchHomework(acc, periodId, acc.Id), async delegate (object sender, IEnumerable<Homework> e)
            {
                SetJustSynced(resourceKey);
                await HomeworksRepository.UpdateHomeworkEntriesAsync(e, acc.Id);
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

        private async Task<IEnumerable<Homework>> FetchHomework(Account account, int periodId, int accountId)
        {
            var query = new GetHomeworkByPupilQuery(account.Pupil.Id, periodId, DateTime.MinValue);

            var client = await new ApiClientFactory().GetAuthenticatedAsync(account);

            var response = client.GetAllAsync(GetHomeworkByPupilQuery.ApiEndpoint, query);

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<HomeworkMapperProfile>(); // Replace with your actual mapping profile class
            });

            IMapper mapper = mapperConfig.CreateMapper();

            var entries = await response.Select(mapper.Map<Homework>).ToArrayAsync();

            foreach (var entry in entries)
            {
                entry.Id.AccountId = accountId;
            }

            return entries;
        }

        private static string GetHomeworkResourceKey(Account account, int periodId)
            => $"Homeworks_{account.Id}_{periodId}";

        public override TimeSpan OfflineDataLifespan => TimeSpan.FromHours(1);
    }

    public class HomeworksRepository
    {
        private static LiteDatabaseAsync _db => LiteDbManager.database;

        public static async Task<IEnumerable<Homework>> GetHomeworkForPupilAsync(int accountId)
        {
            return await _db.GetCollection<Homework>()
                .FindAsync(e => e.Id.AccountId == accountId);
        }

        public static async Task UpdateHomeworkEntriesAsync(IEnumerable<Homework> entries, int accountId)
        {
            await _db.GetCollection<Homework>().DeleteManyAsync(e => e.Id.AccountId == accountId);

            await _db.GetCollection<Homework>().UpsertAsync(entries);
        }
    }
}
