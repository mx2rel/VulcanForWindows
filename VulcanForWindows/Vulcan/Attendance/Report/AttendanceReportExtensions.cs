using System.Collections.Generic;
using System.Linq;

namespace Vulcanova.Features.Attendance.Report;

public static class AttendanceReportExtensions
{
    public static float CalculateOverallAttendance(this IReadOnlyCollection<AttendanceReport> reports)
    {
        var allPresences = reports.Sum(x => x.Presence + x.Late);
        var allNonPresence = reports.Sum(x => x.Absence);

        var percentage = (float)allPresences / (allPresences + allNonPresence) * 100;

        return percentage;
    }
}