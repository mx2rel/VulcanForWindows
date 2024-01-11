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
        public bool isLoading { get; set; }
        public bool isLoaded
        {
            get => !isLoading;
            set => isLoaded = !value;
        }

        public event EventHandler<IEnumerable<T>> Updated;

        public ObservableCollection<T> entries;

        public ObservableCollection<T> Entries
        {
            get { return entries; }
            set
            {
                entries = value;
                Updated?.Invoke(this, entries);
            }
        }

        public void SetEntries(T[] values)
        {
            entries.ReplaceAll(values);
        }

        public Task<IEnumerable<T>> GetFunction;

        public EventHandler<IEnumerable<T>> RepoUpdate;

        public NewResponseEnvelope(Task<IEnumerable<T>> GetFunction, EventHandler<IEnumerable<T>> RepoUpdateFunc)
        {
            entries = new ObservableCollection<T>();
            this.GetFunction = GetFunction;
            RepoUpdate += RepoUpdateFunc;
        }

        public async Task Sync()
        {
            isLoading = true;

            OnPropertyChanged(nameof(isLoaded));
            OnPropertyChanged(nameof(isLoading));

            var onlineEntries = await GetFunction;
            Debug.WriteLine(JsonConvert.SerializeObject(onlineEntries));
            entries.ReplaceAll((IEnumerable<T>)onlineEntries);

            RepoUpdate?.Invoke(this, onlineEntries);

            isLoading = false;
            Updated?.Invoke(this, entries);
            OnPropertyChanged(nameof(isLoaded));
            OnPropertyChanged(nameof(isLoading));

        }

        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
