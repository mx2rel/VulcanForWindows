using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VulcanForWindows.Vulcan.Grades.Final
{
    public static class FinalGradeParser
    {
        public static bool TryGetValueFromDescriptiveForm(string s, out int value) =>
            (value = s switch
            {
                "cel" or "celujący" or "6" => 6,
                "bdb" or "bardzo dobry" or "5" => 5,
                "db" or "dobry" or "4" => 4,
                "dst" or "dostateczny" or "3" => 3,
                "dop" or "dopuszczający" or "2" => 2,
                "ndst" or "niedostateczny" or "1" => 1,
                _ => 0
            }) != 0;
    }
}
