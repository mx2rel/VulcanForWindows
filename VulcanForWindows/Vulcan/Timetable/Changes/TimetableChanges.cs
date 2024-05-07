using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using LiteDB.Async;
using Newtonsoft.Json;
using Vulcanova.Core.Uonet;
using Vulcanova.Features.Auth;
using Vulcanova.Features.Auth.Accounts;
using Vulcanova.Uonet.Api.Schedule;
using VulcanTest.Vulcan;
using VulcanTest.Vulcan.Timetable;
using VulcanTest.Vulcan.Timetable.Changes;

namespace Vulcanova.Features.Timetable.Changes;

public class TimetableChanges : UonetResourceProvider
{
    public async Task<TimetableChangeEntry[]> FetchEntriesForRange(Account account, DateTime from, DateTime to)
    {
        var query = new GetScheduleChangesEntriesByPupilQuery(account.Pupil.Id, from, to, DateTime.MinValue);

        var client = await new ApiClientFactory().GetAuthenticatedAsync(account);

        var response = await client.GetAsync(GetScheduleChangesEntriesByPupilQuery.ApiEndpoint, query);

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<TimetableMapperProfile>(); // Replace with your actual mapping profile class
        });

        IMapper mapper = mapperConfig.CreateMapper();

        var entries = response.Envelope.Select(mapper.Map<TimetableChangeEntry>).ToArray();

        foreach (var entry in entries)
        {
            entry.AccountId = account.Id;
            entry.PupilId = account.Pupil.Id;
        }

        return entries;
    }

    public async Task<TimetableChangeEntry[]> FetchEntriesForDay(Account account, DateTime day) =>
        await FetchEntriesForRange(account, new DateTime(day.Year, day.Month, day.Day),
            new DateTime(day.Year, day.Month, day.Day, 23, 59, 59));

    public async Task<TimetableChangeEntry[]> FetchEntriesForMonthAndYear(Account account, DateTime monthAndYear) =>
        await FetchEntriesForRange(account, new DateTime(monthAndYear.Year, monthAndYear.Month, 1),
            new DateTime(monthAndYear.Year, monthAndYear.Month, DateTime.DaysInMonth(monthAndYear.Year, monthAndYear.Month), 23, 59, 59));
    

    private static string GetTimetableResourceKey(Account account, DateTime monthAndYear)
        => $"TimetableChanges_{account.Pupil.Id}_{monthAndYear.Month}_{monthAndYear.Year}";

    public override TimeSpan OfflineDataLifespan => TimeSpan.FromHours(1);
}

public sealed record TimetableChangesUpdatedEvent(int AccountId) : UonetDataUpdatedEvent;

public static class TimetableChangesRepository
{
    private static LiteDatabaseAsync _db => LiteDbManager.database;

    public static async Task<IEnumerable<TimetableChangeEntry>> GetEntriesForPupilAsync(int accountId, int pupilId,
        DateTime monthAndYear)
    {
        return await _db.GetCollection<TimetableChangeEntry>()
            .FindAsync(g =>
                g.PupilId == pupilId && g.AccountId == accountId && g.LessonDate.Year == monthAndYear.Year &&
                (g.LessonDate.Month == monthAndYear.Month || (g.ChangeDate != null && g.ChangeDate.Value.Month == monthAndYear.Month)));
    }

    public static async Task UpsertEntriesAsync(IEnumerable<TimetableChangeEntry> entries, DateTime monthAndYear)
    {
        await _db.GetCollection<TimetableChangeEntry>()
            .DeleteManyAsync(g => g.LessonDate.Year == monthAndYear.Year &&
                                  g.LessonDate.Month == monthAndYear.Month);

        await _db.GetCollection<TimetableChangeEntry>().UpsertAsync(entries);
    }
}