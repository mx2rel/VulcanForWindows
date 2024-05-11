using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vulcanova.Features.Auth.Accounts;
using Vulcanova.Features.Grades;
using VulcanTest.Vulcan;

namespace VulcanForWindows.Vulcan.Grades
{
    public class GradesResponseEnvelope : IResponseEnvelope<Grade>
    {
        public bool isLoading;
        public bool isLoaded;
        public event EventHandler<IEnumerable<Grade>> OnLoadingOrUpdatingFinished;

        private ObservableCollection<Grade> grades;
        public ObservableCollection<Grade> Grades
        {
            get { return grades; }
            set
            {
                grades = value;
                OnLoadingOrUpdatingFinished?.Invoke(this, grades);
            }
        }
        GradesService g; Account account; int periodId; string normalGradesResourceKey; string behaviourGradesResourceKey;
        public GradesResponseEnvelope(GradesService g, Account account, int periodId, string normalGradesResourceKey, string behaviourGradesResourceKey)
        {
            grades = new ObservableCollection<Grade>();
            this.g = g;
            this.account = account;
            this.periodId = periodId;
            this.normalGradesResourceKey = normalGradesResourceKey;
            this.behaviourGradesResourceKey = behaviourGradesResourceKey;
        }
        public GradesResponseEnvelope() { }
        public async Task SyncAsync()
        {
            isLoading = true;
            var onlineGrades = await g.FetchPeriodGradesAsync(account, periodId);

            await GradesRepository.UpdatePupilGradesAsync(onlineGrades);

            GradesService.SetJustSynced(normalGradesResourceKey);
            GradesService.SetJustSynced(behaviourGradesResourceKey);

            Grades.ReplaceAll( await GradesRepository.GetGradesForPupilAsync(account.Pupil.Id,
                periodId));
            isLoading = false;

            isLoaded = true;
            OnLoadingOrUpdatingFinished?.Invoke(this, grades);

        }
    }
}
