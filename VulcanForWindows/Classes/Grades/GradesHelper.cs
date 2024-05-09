using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vulcanova.Features.Grades;

namespace VulcanForWindows.Classes.Grades
{
    public static class GradesHelper
    {

        /// <summary>
        /// Generates array of <see cref="SubjectGrades"/> based on <paramref name="grades"/> array.
        /// </summary>
        /// <param name="grades">Array to generate <see cref="SubjectGrades"/> from</param>
        /// <returns></returns>
        public static SubjectGrades[] GenerateSubjectGrades(this Grade[] grades, bool loadFinalGrade = true)
        {

            var r = grades.GroupBy(r => r.Column.Subject.Id).Select(r => new SubjectGrades(r.First().Column.Subject, r.ToArray(), loadFinalGrade: loadFinalGrade)).ToArray();

            return r;
        }

        public static async Task<SubjectGradesAnalyzed[]> GenerateSubjectGradesAnalyzed(this Grade[] g)
        {
            var SubjectGradesAnalyzed = g.GroupBy(r => r.Column.Subject.Id).Select(r => new SubjectGradesAnalyzed(r.First().Column.Subject, r.ToArray(), true)).ToArray();
            foreach (var element in SubjectGradesAnalyzed) await element.FetchYearlyAverage();
            return SubjectGradesAnalyzed;
        }

    }
}
