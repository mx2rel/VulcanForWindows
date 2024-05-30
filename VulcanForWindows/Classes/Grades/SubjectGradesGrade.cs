using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vulcanova.Features.Grades;

namespace VulcanForWindows.Classes.Grades
{
    public class SubjectGradesGrade : Grade
    {
        bool _isBeingExcluded;

        public bool isBeingExcluded
        {
            get => _isBeingExcluded; 
            set
            {
                _isBeingExcluded = value;
                OnPropertyChanged(nameof(isBeingExcluded));
                OnPropertyChanged(nameof(displayExclude));
                OnPropertyChanged(nameof(displayInclude));
            }
        }

        public bool displayExclude => !IsHipothetic && !isBeingExcluded;
        public bool displayInclude => !IsHipothetic && isBeingExcluded;

        public static IEnumerable<SubjectGradesGrade> Get(IEnumerable<Grade> grades)
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Grade, SubjectGradesGrade>();
            });

            IMapper mapper = config.CreateMapper();
            var subjectGradesGrades = mapper.Map<IEnumerable<Grade>, IEnumerable<SubjectGradesGrade>>(grades);

            return subjectGradesGrades;
        }
        public static SubjectGradesGrade Get(Grade g)
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Grade, SubjectGradesGrade>();
            });

            IMapper mapper = config.CreateMapper();

            return mapper.Map<Grade, SubjectGradesGrade>(g);
        }
    }
}
