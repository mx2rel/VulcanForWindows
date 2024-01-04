using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vulcanova.Features.Auth.Accounts;
using Vulcanova.Features.Grades.Final;
using VulcanTest.Vulcan;

namespace VulcanForWindows.Vulcan.Grades.Final
{

    public class FinalGradesResponseEnvelope : IResponseEnvelope<FinalGradesEntry>
    {
        public bool isLoading;
        public event EventHandler<IEnumerable<FinalGradesEntry>> Updated;

        private ObservableCollection<FinalGradesEntry> grades;
        public ObservableCollection<FinalGradesEntry> Grades
        {
            get { return grades; }
            set
            {
                grades = value;
                Updated?.Invoke(this, grades);
            }
        }
        FinalGrades g; Account account; int periodId; string finalGradesResourceKey;
        public FinalGradesResponseEnvelope(FinalGrades g, Account account, int periodId, string finalGradesResourceKey)
        {
            grades = new ObservableCollection<FinalGradesEntry>();
            this.g = g;
            this.account = account;
            this.periodId = periodId;
            this.finalGradesResourceKey = finalGradesResourceKey;
        }
        public FinalGradesResponseEnvelope() { }
        public async Task SyncAsync()
        {
            isLoading = true;
            var onlineGrades = await g.FetchPeriodGradesAsync(account, periodId);

            await FinalGradesRepository.UpdatePupilFinalGradesAsync(onlineGrades);

            FinalGrades.SetJustSynced(finalGradesResourceKey);

            Grades.ReplaceAll(await FinalGradesRepository.GetFinalGradesForPupilAsync(account.Id, account.Pupil.Id,
                periodId));
            isLoading = false;

            Updated?.Invoke(this, grades);

        }
    }
}
