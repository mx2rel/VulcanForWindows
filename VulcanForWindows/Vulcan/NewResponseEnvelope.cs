using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vulcanova.Features.Auth;
using Vulcanova.Features.Grades;
using VulcanTest.Vulcan;

namespace VulcanForWindows.Vulcan
{
    public class NewResponseEnvelope<T> : IResponseEnvelope<T>, INotifyPropertyChanged
    {
        public bool isLoadingOrUpdating { get; set; }
        public bool isInitiallyLoaded { get; set; }

        public void SendUpdate()
        {
            OnLoadingOrUpdatingFinished?.Invoke(this, entries);
            OnPropertyChanged(nameof(isInitiallyLoaded));
            OnPropertyChanged(nameof(isLoadingOrUpdating));
        }

        public event EventHandler<IEnumerable<T>> OnLoadingOrUpdatingFinished;

        public ObservableCollection<T> entries;

        public ObservableCollection<T> Entries
        {
            get { return entries; }
            set
            {
                entries = value;
                OnLoadingOrUpdatingFinished?.Invoke(this, entries);
            }
        }

        public void SetEntries(T[] values)
        {
            entries.ReplaceAll(values);
        }

        public Task<IEnumerable<T>> GetFunction;

        public EventHandler<IEnumerable<T>> RepoUpdate;

        public NewResponseEnvelope(Task<IEnumerable<T>> GetFunction, EventHandler<IEnumerable<T>> RepoUpdateFunc, EventHandler<IEnumerable<T>> OnUpdated)
        {
            entries = new ObservableCollection<T>();
            this.GetFunction = GetFunction;
            RepoUpdate += RepoUpdateFunc;
            OnLoadingOrUpdatingFinished += OnUpdated;
        }
        public NewResponseEnvelope(Task<IEnumerable<T>> GetFunction, EventHandler<IEnumerable<T>> RepoUpdateFunc)
        {
            entries = new ObservableCollection<T>();
            this.GetFunction = GetFunction;
            RepoUpdate += RepoUpdateFunc;
        }
        public NewResponseEnvelope(IEnumerable<T> initialEntries, Task<IEnumerable<T>> GetFunction,
            EventHandler<IEnumerable<T>> RepoUpdateFunc,
            EventHandler<IEnumerable<T>> OnUpdated, bool actAsIfSynced = true)
        {
            entries = new ObservableCollection<T>(initialEntries);
            isInitiallyLoaded = true;
            this.GetFunction = GetFunction;
            RepoUpdate += RepoUpdateFunc;
            OnLoadingOrUpdatingFinished += OnUpdated;
            if (actAsIfSynced) OnLoadingOrUpdatingFinished?.Invoke(this, entries);
        }

        public NewResponseEnvelope(IEnumerable<T> items)
        {
            entries = new ObservableCollection<T>(items);
        }

        public NewResponseEnvelope()
        {
            entries = new ObservableCollection<T>();
        }

        public async Task Sync()
        {
            isLoadingOrUpdating = true;

            OnPropertyChanged(nameof(isInitiallyLoaded));
            OnPropertyChanged(nameof(isLoadingOrUpdating));

            var onlineEntries = await GetFunction;
            entries.ReplaceAll((IEnumerable<T>)onlineEntries);

            RepoUpdate?.Invoke(this, onlineEntries);

            isLoadingOrUpdating = false;
            OnLoadingOrUpdatingFinished?.Invoke(this, entries);
            OnPropertyChanged(nameof(isInitiallyLoaded));
            OnPropertyChanged(nameof(isLoadingOrUpdating));

        }

        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
