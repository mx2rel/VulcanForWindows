using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using LiteDB.Async;
using VulcanForWindows.Classes;
using VulcanForWindows.Vulcan;
using Vulcanova.Core.Uonet;
using Vulcanova.Features.Auth;
using Vulcanova.Features.Auth.Accounts;
using Vulcanova.Uonet.Api;
using Vulcanova.Uonet.Api.Notes;
using VulcanTest.Vulcan;

namespace Vulcanova.Features.Notes
{
    public class NotesService : UonetResourceProvider
    {
        public async Task<NewResponseEnvelope<Note>> GetNotesByDateRange(Account account,
            bool forceSync = false, bool waitForSync = false)
        {
            var resourceKey = GetNotesResourceKey(account);
            var items = await NotesRepository.GetNotesByPupilAsync(account.Id, account.Pupil.Id);

            var v = new NewResponseEnvelope<Note>(FetchNotesAsync(account), async delegate (object sender, IEnumerable<Note> e)
            {
                await NotesRepository.UpdateNoteEntriesAsync(e, account.Id, account.Pupil.Id);
                SetJustSynced(resourceKey);
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

        private async Task<IEnumerable<Note>> FetchNotesAsync(Account account)
        {
            var query = new GetNotesByPupilQuery(account.Pupil.Id, DateTime.MinValue);
            var client = await new ApiClientFactory().GetAuthenticatedAsync(account);
            var response = client.GetAllAsync(GetNotesByPupilQuery.ApiEndpoint, query);

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<NoteMapperProfile>(); // Replace with your actual mapping profile class
            });

            IMapper mapper = mapperConfig.CreateMapper();

            var entries = await response.Select(mapper.Map<Note>).ToArrayAsync();

            foreach (var entry in entries)
            {
                entry.AccountId = account.Id;
            }

            return entries;
        }

        private static string GetNotesResourceKey(Account account)
        {
            return $"Notes_{account.Id}";
        }

        public override TimeSpan OfflineDataLifespan => TimeSpan.FromHours(1);
    }

    public class NotesRepository
    {
        private static LiteDatabaseAsync _db => LiteDbManager.database;

        public static async Task<IEnumerable<Note>> GetNotesByPupilAsync(int accountId, int pupilId)
        {
            return await _db.GetCollection<Note>()
                .FindAsync(e => e.AccountId == accountId && e.PupilId == pupilId);
        }

        public static async Task UpdateNoteEntriesAsync(IEnumerable<Note> entries, int accountId, int pupilId)
        {
            await _db.GetCollection<Note>().DeleteManyAsync(e => e.AccountId == accountId && e.PupilId == pupilId);
            await _db.GetCollection<Note>().UpsertAsync(entries);
        }
    }
}
