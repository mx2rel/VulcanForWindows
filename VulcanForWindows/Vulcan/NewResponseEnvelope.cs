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
    /// <summary>
    /// Represents a generic response envelope for handling data loading and updating.
    /// </summary>
    /// <typeparam name="T">The type of data stored in the envelope.</typeparam>
    public class NewResponseEnvelope<T> : IResponseEnvelope<T>, INotifyPropertyChanged
    {
        /// <summary>
        /// Value indicating whether the envelope is currently loading or updating data.
        /// </summary>
        public bool isLoadingOrUpdating { get; set; }

        /// <summary>
        /// Value indicating whether the envelope finished initial loaded.
        /// </summary>
        public bool isInitialLoadDone { get; set; }

        /// <summary>
        /// Sends an update notification indicating the completion of loading or updating data.
        /// </summary>
        public void SendUpdate()
        {
            OnLoadingOrUpdatingFinished?.Invoke(this, entries);
            OnPropertyChanged(nameof(isInitialLoadDone));
            OnPropertyChanged(nameof(isLoadingOrUpdating));
        }

        /// <summary>
        /// Occurs when loading or updating of data is finished.
        /// </summary>
        public event EventHandler<IEnumerable<T>> OnLoadingOrUpdatingFinished;


        /// <summary>
        /// Occurs when loading or updating of data starts.
        /// </summary>
        public event EventHandler OnLoadingOrUpdatingStarted;

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

        /// <summary>
        /// Replaces the existing entries with the specified values.
        /// </summary>
        /// <param name="values">The values to set as entries.</param>
        public void SetEntries(T[] values)
        {
            entries.ReplaceAll(values);
        }

        /// <summary>
        /// Represents the asynchronous function to retrieve data.
        /// </summary>
        public Task<IEnumerable<T>> GetFunction;

        /// <summary>
        /// Initializes a new instance of the NewResponseEnvelope class.
        /// </summary>
        /// <param name="GetFunction">The function to retrieve data asynchronously.</param>
        /// <param name="RepoUpdateFunc">The event handler for repository updates.</param>
        /// <param name="OnUpdated">The event handler for updates.</param>
        public NewResponseEnvelope(Task<IEnumerable<T>> GetFunction, EventHandler<IEnumerable<T>> RepoUpdateFunc, EventHandler<IEnumerable<T>> OnUpdated)
        {
            entries = new ObservableCollection<T>();
            this.GetFunction = GetFunction;
            OnLoadingOrUpdatingFinished += RepoUpdateFunc;
            OnLoadingOrUpdatingFinished += OnUpdated;
        }

        /// <summary>
        /// Initializes a new instance of the NewResponseEnvelope class.
        /// </summary>
        /// <param name="GetFunction">The function to retrieve data asynchronously.</param>
        /// <param name="RepoUpdateFunc">The event handler for repository updates.</param>
        public NewResponseEnvelope(Task<IEnumerable<T>> GetFunction, EventHandler<IEnumerable<T>> RepoUpdateFunc)
        {
            entries = new ObservableCollection<T>();
            this.GetFunction = GetFunction;
            OnLoadingOrUpdatingFinished += RepoUpdateFunc;
        }

        /// <summary>
        /// Initializes a new instance of the NewResponseEnvelope class.
        /// </summary>
        /// <param name="initialEntries">The initial entries to populate the envelope.</param>
        /// <param name="GetFunction">The function to retrieve data asynchronously.</param>
        /// <param name="RepoUpdateFunc">The event handler for repository updates.</param>
        /// <param name="OnUpdated">The event handler for updates.</param>
        /// <param name="callOnLoad">Flag indicating if should call <see cref="OnLoadingOrUpdatingFinished"/>.</param>
        public NewResponseEnvelope(IEnumerable<T> initialEntries, Task<IEnumerable<T>> GetFunction, EventHandler<IEnumerable<T>> RepoUpdateFunc, EventHandler<IEnumerable<T>> OnUpdated, bool callOnLoad = true)
        {
            entries = new ObservableCollection<T>(initialEntries);
            isInitialLoadDone = true;
            this.GetFunction = GetFunction;
            OnLoadingOrUpdatingFinished += RepoUpdateFunc;
            OnLoadingOrUpdatingFinished += OnUpdated;
            if (callOnLoad) OnLoadingOrUpdatingFinished?.Invoke(this, entries);
        }

        /// <summary>
        /// Initializes a new instance of the NewResponseEnvelope class.
        /// </summary>
        public NewResponseEnvelope()
        {
            entries = new ObservableCollection<T>();
        }

        /// <summary>
        /// Asynchronously synchronizes or loads the data in the envelope.
        /// </summary>
        public async Task Sync()
        {
            isLoadingOrUpdating = true;
            OnLoadingOrUpdatingStarted?.Invoke(this, null);

            OnPropertyChanged(nameof(isInitialLoadDone));
            OnPropertyChanged(nameof(isLoadingOrUpdating));

            var onlineEntries = await GetFunction;
            entries.Add((IEnumerable<T>)onlineEntries);


            isLoadingOrUpdating = false;
            OnLoadingOrUpdatingFinished?.Invoke(this, onlineEntries);
            OnPropertyChanged(nameof(isInitialLoadDone));
            OnPropertyChanged(nameof(isLoadingOrUpdating));
        }


        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
