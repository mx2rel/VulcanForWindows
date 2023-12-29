using System;
using System.Linq;
using Vulcanova.Features.Auth.Accounts;

namespace Vulcanova.Features.Shared;

public static class AccountExtensions
{
    public static (DateTime Start, DateTime End) GetSchoolYearDuration(this Account account)
    {
        var currentPeriod = account.Periods.Single(x => x.Current);
        var allPeriodsInYear = account.Periods.Where(x => x.Level == currentPeriod.Level)
            .ToArray();

        var yearStart = allPeriodsInYear.First().Start;
        var yearEnd = allPeriodsInYear.Last().End;

        return (yearStart, yearEnd);
    }
}