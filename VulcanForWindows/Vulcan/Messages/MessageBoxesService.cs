using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using LiteDB.Async;
using VulcanForWindows.Vulcan;
using Vulcanova.Core.Uonet;
using Vulcanova.Features.Auth.Accounts;
using Vulcanova.Uonet.Api.MessageBox;
using VulcanTest.Vulcan;

namespace Vulcanova.Features.Messages;

public class MessageBoxesService : UonetResourceProvider
{

    public async Task<NewResponseEnvelope<MessageBox>> GetMessageBoxesByAccountId(Account account, bool forceSync = false, bool waitForSync = false)
    {
        var pupilId = account.Pupil.Id;
        var resourceKey = GetResourceKey(pupilId);

        var items = await MessageBoxesRepository.GetMessageBoxesForAccountAsync(pupilId);

        var v = new NewResponseEnvelope<MessageBox>(FetchMessageBoxesAsync(account), async delegate (object sender, IEnumerable<MessageBox> e)
        {
            SetJustSynced(resourceKey);
            await MessageBoxesRepository.UpdateMessageBoxesForAccountAsync(pupilId,e);

        });

        if (ShouldSync(resourceKey) || forceSync)
        {
            if (waitForSync)
                await v.Sync();
            else
                v.Sync();
        }

        return v;
    }

    public async Task MarkMessageBoxAsSelectedAsync(MessageBox box)
    {
        var boxes = (await MessageBoxesRepository.GetMessageBoxesForAccountAsync(box.Id.PupilId))
            .ToArray();

        foreach (var b in boxes)
        {
            b.IsSelected = b.Id == box.Id;
        }

        await MessageBoxesRepository.UpdateMessageBoxesForAccountAsync(box.Id.PupilId, boxes);
    }

    private async Task<IEnumerable<MessageBox>> FetchMessageBoxesAsync(Account account)
    {
        var query = new GetMessageBoxesQuery();

        var client = await new ApiClientFactory().GetAuthenticatedAsync(account);

        var response = await client.GetAsync(GetMessageBoxesQuery.ApiEndpoint, query);


        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MessageBoxMapperProfile>(); // Replace with your actual mapping profile class
        });

        IMapper mapper = mapperConfig.CreateMapper();


        var entries = response.Envelope.Select(mapper.Map<MessageBox>).ToArray();

        foreach (var entry in entries)
        {
            entry.Id.PupilId = account.Pupil.Id;
        }

        return entries;
    }

    private static string GetResourceKey(int pupilId)
        => $"MessageBoxes_{pupilId}";

    public override TimeSpan OfflineDataLifespan => TimeSpan.FromDays(7);
}

public class MessageBoxesRepository
{
    private static LiteDatabaseAsync _db => LiteDbManager.database;


    public static async Task<IEnumerable<MessageBox>> GetMessageBoxesForAccountAsync(int accountId)
    {
        return await _db.GetCollection<MessageBox>().FindAsync(b => b.Id.PupilId == accountId);
    }

    public static async Task<MessageBox> GetSelectedForAccountAsync(int accountId)
    {
        return await _db.GetCollection<MessageBox>()
            .FindOneAsync(b => b.Id.PupilId == accountId && b.IsSelected);
    }

    public static async Task UpdateMessageBoxesForAccountAsync(int accountId, IEnumerable<MessageBox> boxes)
    {
        await _db.GetCollection<MessageBox>().DeleteManyAsync(e => e.Id.PupilId == accountId);

        await _db.GetCollection<MessageBox>().UpsertAsync(boxes);
    }

    public static async Task UpdateMessageBoxAsync(MessageBox box)
    {
        await _db.GetCollection<MessageBox>().UpdateAsync(box);
    }

    public static async Task DeleteMessageBoxesForAccountAsync(int accountId)
    {
        await _db.GetCollection<MessageBox>().DeleteManyAsync(m => m.Id.PupilId == accountId);
    }
}