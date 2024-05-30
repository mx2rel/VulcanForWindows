using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vulcanova.Features.Auth;
using Vulcanova.Features.Shared;

namespace VulcanForWindows.Extensions
{
    public static class PeriodExtenstions
    {
        /// <summary>
        /// Returns Id of first term of level
        /// </summary>
        public static int GetSchoolYearId(this Period period)
        {
            var id = period.Id;
            return id - ((id % 2 == 0) ? 0 : 1);
        }

        public static Period GetFirstPeriodOfLevel(this Period period)
        {

            var id = period.Id;
            return new AccountRepository().GetActiveAccount().Periods.Where(r => r.Level == period.Level).FirstOrDefault();
        }
        public static IEnumerable<Period> GetAllPeriodsOfLevel(this Period period)
        {

            var id = period.Id;
            return new AccountRepository().GetActiveAccount().Periods.Where(r => r.Level == period.Level);
        }

        public static Period GetPeriodFromId(int id)
        {

            var p = new AccountRepository().GetActiveAccount().Periods.Find(r => r.Id == id);
            return p;
        }

        public static int GetSchoolYearId(int id)
            => id - ((id % 2 == 0) ? 0 : 1);
        public static IEnumerable<int> GetSchoolYearAllIds(int id)
            => GetAllPeriodsOfLevel(GetPeriodFromId(id)).Select(r => r.Id);
    }
}
