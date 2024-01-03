using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vulcanova.Features.Grades;

namespace VulcanForWindows.Classes
{
    public static class GradeExtensions
    {
        public static double CountAverage(this Grade[] grades)
        {
            decimal sum = 0;
            decimal weightSum = 0;

            foreach (var grade in grades)
            {
                if (grade.ContentRaw != null)
                    if (GetValue(grade.ContentRaw, out var g))
                    {
                        sum += g * grade.Column.Weight;
                        weightSum += grade.Column.Weight;
                    }
            }

            if (weightSum == 0)
                return 0;

            return (double)Math.Round(sum / weightSum * 100) / 100;
        }

        public static double CountAverage(this SubjectGrades grades, decimal newGrade, decimal newWeight)
        {
            decimal sum = 0;
            decimal weightSum = 0;

            foreach (var grade in grades.grades)
            {
                if (grade.ContentRaw != null)
                    if (GetValue(grade.ContentRaw, out var g))
                    {
                        sum += g * grade.Column.Weight;
                        weightSum += grade.Column.Weight;
                    }
            }

            sum += newGrade * newWeight;
            weightSum += newWeight;

            if (weightSum == 0)
                return 0;

            return (double)Math.Round(sum / weightSum * 100) / 100;
        }

        public static bool GetValue(string s, out decimal o)
        {
            var l = s.ToArray();
            if (int.TryParse(l[0] + "", out var full))
            {
                double second = (l.Length == 1) ? 0 : ((l[1] == '+') ? 0.5 : -0.25);

                o = (decimal)full + (decimal)second;
                return true;
            }
            else
                o = 0;
            return false;
        }
    }
}
