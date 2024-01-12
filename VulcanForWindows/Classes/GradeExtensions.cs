using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vulcanova.Features.Grades;
using Vulcanova.Features.Grades.Final;

namespace VulcanForWindows.Classes
{
    public static class GradeExtensions
    {
        public static double CalculateAverage(this Grade[] grades)
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


        public static double CalculateAverage(this FinalGradesEntry[] grades)
        {
            decimal sum = 0;
            decimal weightSum = 0;

            foreach (var grade in grades.Where(r => r.FinalGrade != null || r.PredictedGrade != null))
            {
                if (decimal.TryParse(grade.FastDisplayGrade, out var r))
                {
                    sum += r;
                    weightSum++;
                }
                else
                    Debug.Write($"\nFailed to parse {grade.FastDisplayGrade}\n");
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
            o = 0;
            var l = s.ToArray();
            if (l.Length == 0) return false;
            if (int.TryParse(l[0] + "", out var full))
            {
                double second = (l.Length == 1) ? 0 : ((l[1] == '+') ? 0.5 : -0.25);

                o = (decimal)full + (decimal)second;
                return true;
            }
            return false;
        }
    }
}
