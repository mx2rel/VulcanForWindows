using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using LiteDB.Async;
using VulcanForWindows.Vulcan;
using Vulcanova.Core.Data;
using Vulcanova.Core.Uonet;
using Vulcanova.Features.Auth;
using Vulcanova.Features.Auth.Accounts;
using Vulcanova.Uonet.Api.MessageBox;
using VulcanTest.Vulcan;

namespace Vulcanova.Features.Messages;

public class MessagesService : UonetResourceProvider
{

    public static async Task<(NewResponseEnvelope<Message> Received, NewResponseEnvelope<Message> Sent, NewResponseEnvelope<Message> Deleted)> GetMessagesStack(Account account, Guid messageBoxId, bool forceSync = false, bool waitForSync =false)
    {
        var instance = new MessagesService();
        return (await
            instance.GetMessagesByBox(account, messageBoxId, MessageBoxFolder.Received, forceSync, waitForSync),
            await
            instance.GetMessagesByBox(account, messageBoxId, MessageBoxFolder.Sent, forceSync, waitForSync),
            await
            instance.GetMessagesByBox(account, messageBoxId, MessageBoxFolder.Deleted, forceSync, waitForSync));
    }

    public async Task<NewResponseEnvelope<Message>> GetMessagesByBox(
        Account account, Guid messageBoxId, MessageBoxFolder folder, bool forceSync = false, bool waitForSync = false)
    {
        var resourceKey = GetResourceKey(account.Id, messageBoxId, folder);

        var items = await MessagesRepository.GetMessagesByBoxAsync(messageBoxId, folder);

        var v = new NewResponseEnvelope<Message>(FetchMessagesByBoxAsync(account, messageBoxId, folder), async delegate (object sender, IEnumerable<Message> e)
        {
            SetJustSynced(resourceKey);
            await MessagesRepository.UpsertMessagesForBoxAsync(messageBoxId, e);

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

    public async Task MarkMessageAsReadAsync(Guid messageBoxId, AccountEntityId<Guid> messageId)
    {
        var account = new AccountRepository().GetByIdAsync(messageId.AccountId);

        var apiClient = await new ApiClientFactory().GetAuthenticatedAsync(account);

        await apiClient.PostAsync(ChangeMessageStatusRequest.ApiEndpoint,
            new ChangeMessageStatusRequest(messageBoxId, messageId.VulcanId, ChangeMessageStatusRequest.SetMessageStatus.Read));
        var message = await MessagesRepository.GetMessageAsync(messageBoxId, messageId.VulcanId);
        message.DateRead = DateTime.UtcNow;

        await MessagesRepository.UpdateMessageAsync(message);

        //MessageBus.Current.SendMessage(new MessageReadEvent(messageBoxId, messageId.VulcanId, message.DateRead.Value));
    }
    public async Task TrashMessage(Guid messageBoxId, AccountEntityId<Guid> messageId)
    {
        var account = new AccountRepository().GetByIdAsync(messageId.AccountId);

        var apiClient = await new ApiClientFactory().GetAuthenticatedAsync(account);

        await apiClient.PostAsync(ChangeMessageStatusRequest.ApiEndpoint,
            new ChangeMessageStatusRequest(messageBoxId, messageId.VulcanId, ChangeMessageStatusRequest.SetMessageStatus.Trash));

        //MessageBus.Current.SendMessage(new MessageReadEvent(messageBoxId, messageId.VulcanId, message.DateRead.Value));
    }

    private async Task<IEnumerable<Message>> FetchMessagesByBoxAsync(Account account, Guid messageBoxId, MessageBoxFolder folder)
    {
        var lastSync = GetLastSync(GetResourceKey(account.Id, messageBoxId, folder));

        var query = new GetMessagesByMessageBoxQuery(messageBoxId, folder, lastSync,
            PageSize: int.MaxValue);

        var apiClient = await new ApiClientFactory().GetAuthenticatedAsync(account);

        var response = await apiClient.GetAsync(GetMessagesByMessageBoxQuery.ApiEndpoint, query);


        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MessageBoxMapperProfile>(); // Replace with your actual mapping profile class
        });

        IMapper mapper = mapperConfig.CreateMapper();



        var messages = response.Envelope
            .Select(mapper.Map<Message>)
            .ToArray();

        foreach (var message in messages)
        {
            message.Id.AccountId = account.Id;
            message.MessageBoxId = messageBoxId;
            message.Folder = folder;
        }

        return messages;
    }

    private static string GetResourceKey(int accountId, Guid messageBoxId, MessageBoxFolder folder)
        => $"Messages_{accountId}_{messageBoxId}_{folder}";

    public override TimeSpan OfflineDataLifespan => TimeSpan.FromHours(1);
}

public static class MessagesRepository
{
    private static LiteDatabaseAsync _db => LiteDbManager.database;

    public static async Task<IEnumerable<Message>> GetMessagesByBoxAsync(Guid messageBoxId, MessageBoxFolder folder)
    {
        return (await _db.GetCollection<Message>()
            .FindAsync(m => m.MessageBoxId == messageBoxId && m.Folder == folder))
            .OrderByDescending(x => x.DateSent);
    }

    public static async Task UpsertMessagesForBoxAsync(Guid messageBoxId, IEnumerable<Message> messages)
    {
        await _db.GetCollection<Message>()
            .UpsertAsync(messages);
    }

    public static async Task<Message> GetMessageAsync(Guid messageBoxId, Guid messageId)
    {
        return await _db.GetCollection<Message>()
            .FindOneAsync(m => m.MessageBoxId == messageBoxId && m.Id.VulcanId == messageId);
    }

    public static async Task UpdateMessageAsync(Message message)
    {
        await _db.GetCollection<Message>().UpdateAsync(message);
    }

    public static async Task DeleteMessagesInBoxAsync(Guid messageBoxId)
    {
        await _db.GetCollection<Message>().DeleteManyAsync(m => m.MessageBoxId == messageBoxId);
    }
}