﻿using AutoMapper;
using LiteDB.Async;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Vulcanova.Core.Uonet;
using Vulcanova.Features.Auth.Accounts;
using Vulcanova.Uonet.Api.Schedule;
using VulcanTest.Vulcan;
using VulcanTest.Vulcan.Timetable;

namespace VulcanTest.Vulcan.Timetable
{
    public class OgTimetable : UonetResourceProvider
    {
        public override TimeSpan OfflineDataLifespan => TimeSpan.FromMinutes(10);

        public IObservable<IEnumerable<TimetableEntry>> GetPeriodEntriesByMonth(Account account, DateTime monthAndYear,
       bool forceSync = false)
        {
            return Observable.Create<IEnumerable<TimetableEntry>>(async observer =>
            {

                var resourceKey = GetTimetableResourceKey(account, monthAndYear);

                var items = await TimetableRepository.GetEntriesForPupilAsync(account.Id, account.Pupil.Id,
                    monthAndYear);

                observer.OnNext(items);

                if (ShouldSync(resourceKey) || forceSync)
                {
                    var onlineEntries = await FetchEntriesForMonthAndYear(account, monthAndYear);

                    await TimetableRepository.UpdatePupilEntriesAsync(onlineEntries, monthAndYear);

                    SetJustSynced(resourceKey);

                    items = await TimetableRepository.GetEntriesForPupilAsync(account.Id, account.Pupil.Id,
                        monthAndYear);

                    observer.OnNext(items);
                }

                observer.OnCompleted();
            });
        }

        public async Task<TimetableEntry[]> FetchEntriesForRange(Account account, DateTime from, DateTime to)
        {
            var query = new GetScheduleEntriesByPupilQuery(account.Pupil.Id, from, to, DateTime.MinValue);

            var client = await new ApiClientFactory().GetAuthenticatedAsync(account);

            var response = await client.GetAsync(GetScheduleEntriesByPupilQuery.ApiEndpoint, query);

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<TimetableMapperProfile>(); // Replace with your actual mapping profile class
            });

            IMapper mapper = mapperConfig.CreateMapper();

            var entries = response.Envelope.Select(mapper.Map<TimetableEntry>).ToArray();

            foreach (var entry in entries)
            {
                entry.AccountId = account.Id;
                entry.PupilId = account.Pupil.Id;
            }

            return entries;
        }
        public async Task<TimetableEntry[]> FetchEntriesForDay(Account account, DateTime day) =>
        await FetchEntriesForRange(account, new DateTime(day.Year, day.Month, day.Day),
            new DateTime(day.Year, day.Month, day.Day, 23, 59, 59));

        public async Task<TimetableEntry[]> FetchEntriesForMonthAndYear(Account account, DateTime monthAndYear) =>
            await FetchEntriesForRange(account, new DateTime(monthAndYear.Year, monthAndYear.Month, 1),
                new DateTime(monthAndYear.Year, monthAndYear.Month, DateTime.DaysInMonth(monthAndYear.Year, monthAndYear.Month), 23, 59, 59));
        private static string GetTimetableResourceKey(Account account, DateTime monthAndYear)
        => $"Timetable_{account.Id}_{account.Pupil.Id}_{monthAndYear.Month}_{monthAndYear.Year}";

    }

    public static class TimetableRepository
    {
        private static LiteDatabaseAsync _db => LiteDbManager.database;

        public static async Task<IEnumerable<TimetableEntry>> GetEntriesForPupilAsync(int accountId, int pupilId,
            DateTime monthAndYear)
        {
            return await _db.GetCollection<TimetableEntry>()
                .FindAsync(g =>
                    g.PupilId == pupilId && g.AccountId == accountId && g.Date.Year == monthAndYear.Year &&
                    g.Date.Month == monthAndYear.Month);
        }

        public static async Task UpdatePupilEntriesAsync(IEnumerable<TimetableEntry> entries, DateTime monthAndYear)
        {
            await _db.GetCollection<TimetableEntry>()
                .DeleteManyAsync(g => g.Date.Year == monthAndYear.Year &&
                                      g.Date.Month == monthAndYear.Month);

            await _db.GetCollection<TimetableEntry>().UpsertAsync(entries);
        }
    }

}
