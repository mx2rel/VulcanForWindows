using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VulcanForWindows.Classes.VulcanGradesDb.Models
{
    public class SingleClassmateColumn
    {
        public int ColumnId { get; set; }
        public SingleClassmateGrade[] Grades { get; set; }
    }
}
