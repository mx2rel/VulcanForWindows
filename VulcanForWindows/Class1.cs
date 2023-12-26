using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vulcanova.Features.Grades;

namespace VulcanForWindows
{
    public class SubjectGrades
    {
        public SubjectGrades(string n, Grade[] g)
        {
            subName = n;
            grades = g;
        }
        public SubjectGrades(IGrouping<string, Grade[]> v)
        {
            subName = v.Key;
            grades = v.SelectMany(group => group).ToArray();
        }
        public string subName { get; set; }
        public Grade[] grades { get; set; }

        public static SubjectGrades[] GetSubjectsGrades()
        {
            var r = RandomGenerator.GenerateRandomDataset()
                .GroupBy(r => r.Column.subjectName)
                .Select(g => new SubjectGrades(g.Key, g.ToArray()))
                .ToArray();

            return r;
        }
    }
}
