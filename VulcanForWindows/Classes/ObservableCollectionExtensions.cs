using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace VulcanTest.Vulcan
{
    public static class ObservableCollectionExtensions
    {
        public static void ReplaceAll<T>(this ObservableCollection<T> collection, IEnumerable<T> newItems)
        {
            if (collection.Count > 0)
                collection.Clear();

            foreach (var item in newItems.Where(r => r != null))
            {
                collection.Add(item);
            }

        }


        public static void Add<T>(this ObservableCollection<T> collection, IEnumerable<T> newItems)
        {

            foreach (var item in newItems.Where(r => r != null))
            {
                collection.Add(item);
            }

        }
    }
}
