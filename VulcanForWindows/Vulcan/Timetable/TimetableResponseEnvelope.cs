using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vulcanova.Features.Auth.Accounts;
using VulcanTest.Vulcan;
using VulcanTest.Vulcan.Timetable;

namespace VulcanForWindows.Vulcan.Timetable
{
    public class TimetableResponseEnvelope : IResponseEnvelope<TimetableEntry>
    {
        public bool isLoading;
        public event EventHandler<IEnumerable<TimetableEntry>> OnLoadingOrUpdatingFinished;

        private ObservableCollection<TimetableEntry> entries;
        public ObservableCollection<TimetableEntry> Entries
        {
            get { return entries; }
            set
            {
                entries = value;
                OnLoadingOrUpdatingFinished?.Invoke(this, entries);
            }
        }
        OgTimetable g; Account account; DateTime monthAndYear; string resourceKey;
        public TimetableResponseEnvelope(OgTimetable g, Account account, DateTime monthAndYear, string resourceKey)
        {
            entries = new ObservableCollection<TimetableEntry>();
            this.g = g;
            this.account = account;
            this.monthAndYear = monthAndYear;
            this.resourceKey = resourceKey;
        }
        public TimetableResponseEnvelope() { }
        public async Task SyncAsync()
        {

            isLoading = true;

            var onlineEntries = await g.FetchEntriesForMonthAndYear(account, monthAndYear);
            //Debug.Write(JsonConvert.SerializeObject(onlineEntries));
            await TimetableRepository.UpdatePupilEntriesAsync(onlineEntries, monthAndYear);

            OgTimetable.SetJustSynced(resourceKey);

            Entries.ReplaceAll<TimetableEntry>(onlineEntries);

            OnLoadingOrUpdatingFinished?.Invoke(this, entries);

        }
    }
}
