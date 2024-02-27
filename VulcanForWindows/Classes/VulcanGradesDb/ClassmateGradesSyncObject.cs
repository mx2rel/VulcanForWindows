using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VulcanForWindows.Classes.VulcanGradesDb
{
    public class ClassmateGradesSyncObject
    {
        public ClassmateGradesSyncObject(int columnId, DateTime synced)
        {
            ColumnId = columnId;
            Synced = synced;
        }

        public int ColumnId { get; set; }
        public DateTime Synced { get; set; }
    }
}
