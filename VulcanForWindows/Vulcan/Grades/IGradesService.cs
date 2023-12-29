using System;
using System.Collections.Generic;
using Vulcanova.Features.Auth.Accounts;

namespace Vulcanova.Features.Grades;

public interface IGradesService
{
    IObservable<IEnumerable<Grade>> GetPeriodGrades(Account account, int periodId, bool forceSync = false);
}