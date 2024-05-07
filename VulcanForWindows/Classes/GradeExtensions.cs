using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
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
            return grades.CalculateAverageRaw().avg;
        }
        public static (double avg, double sum, int weights) CalculateAverageRaw(this Grade[] grades)
        {
            decimal sum = 0;
            decimal weightSum = 0;

            foreach (var grade in grades.Where(r => r.Value.HasValue))
            {
                sum += grade.Value.Value * grade.Column.Weight;
                weightSum += grade.Column.Weight;
            }

            if (weightSum == 0)
                return (0, 0, 0);

            return ((double)Math.Round(sum / weightSum * 100) / 100, (double)sum, (int)weightSum);
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

        public static double CountAverage(this SubjectGrades grades, double newGrade, int newWeight)
        {
            var raw = grades.yearlyGrades.CalculateAverageRaw();
            var newSum = raw.sum + newGrade * newWeight;
            double newWeightsSum = raw.weights + newWeight;
            return Math.Round(newSum / newWeightsSum, 2);
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

        public static Grade[] GetLatestGrades(this Grade[] g)
        {
            if (g.Length == 0) return new Grade[0];
            var startDate = g.OrderByDescending(r => r.DateModify).First().DateModify.AddDays(-7).Date;
            return g.Where(r => r.DateModify.Date >= startDate).ToArray();
        }
    }
}
