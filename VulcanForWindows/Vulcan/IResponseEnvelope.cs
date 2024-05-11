using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vulcanova.Features.Auth.Accounts;

namespace VulcanForWindows.Vulcan
{
    public interface IResponseEnvelope<T>
    {
        public event EventHandler<IEnumerable<T>> OnLoadingOrUpdatingFinished;

        //public static async Task<G> GetEnvelopeForType<G>(
        //    int periodId,
        //    Account acc,
        //    Func<Account, int, bool, bool, Task<G>> getOperation,
        //    Action OnChange, IDictionary<int, G> dictionary) where G : IResponseEnvelope<T>
        //{
        //    if (!dictionary.ContainsKey(periodId))
        //    {
        //        dictionary[periodId].OnLoadingOrUpdatingFinished += (sender, args) => OnChange(); // OnLoadingOrUpdatingFinished this line
        //        dictionary[periodId] = await getOperation(acc, periodId, false, false);
        //    }
        //    else
        //    {
        //        // LoadingBar.Visibility = isLoadingOrUpdating ? Visibility.Visible : Visibility.Collapsed;
        //        // TODO: LOADING BAR
        //        // Debug.Write(JsonConvert.SerializeObject(cd));
        //    }

        //    return dictionary[periodId];
        //}
    }
}
