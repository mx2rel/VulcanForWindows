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

        if (yearStart.Day == 31 && yearStart.Month == 8) yearStart = yearStart.AddDays(1); //some schools have this set up wrong

        return (yearStart, yearEnd);
    }
}