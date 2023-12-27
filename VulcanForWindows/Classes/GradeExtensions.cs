using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                if (grade.Value != null)
                {
                    sum += grade.Value.GetValueOrDefault() * grade.Column.Weight;
                    weightSum += grade.Column.Weight;
                }
            }

            if (weightSum == 0)
                return 0;

            return (double)Math.Round(sum / weightSum * 100) / 100;
        }
    }
}
