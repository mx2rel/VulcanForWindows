using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vulcanova.Features.Grades;

namespace VulcanForWindows.Viewmodels
{
    public class GradeViewmodel
    {

        public GradeViewmodel(Grade grade)
        {
            BasedOn = grade;
        }

        public Grade BasedOn { get; set; }


    }
}
