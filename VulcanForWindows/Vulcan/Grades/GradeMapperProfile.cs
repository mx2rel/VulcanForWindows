using System;
using System.Globalization;
using AutoMapper;
using Vulcanova.Features.Grades.Final;
using Vulcanova.Uonet.Api.Common.Models;
using Vulcanova.Uonet.Api.Grades;
using Subject = Vulcanova.Features.Shared.Subject;

namespace Vulcanova.Features.Grades;

public class GradeMapperProfile : Profile
{
    public GradeMapperProfile()
    {
        CreateMap<GradePayload, Grade>()
            .ForMember(g => g.CreatorName, cfg => cfg.MapFrom(src => src.Creator.DisplayName))
            .ForMember(g => g.VulcanValue, cfg => cfg.MapFrom(src => src.Value));

        CreateMap<Uonet.Api.Grades.Column, Column>();

        CreateMap<Uonet.Api.Common.Models.Subject, Subject>();

        CreateMap<DateTimeInfo, DateTime>()
            .ConvertUsing(d => DateTimeOffset.FromUnixTimeMilliseconds(d.Timestamp).LocalDateTime);

        CreateMap<GradesSummaryEntryPayload, FinalGradesEntry>()
            .ForMember(f => f.Id, cfg => cfg.MapFrom(src => $"{src.PeriodId}_{src.Id}"))
            .ForMember(f => f.PredictedGrade, cfg => cfg.MapFrom(src => src.Entry1))
            .ForMember(f => f.FinalGrade, cfg => cfg.MapFrom(src => src.Entry2));

        CreateMap<AverageGradePayload, AverageGrade>()
            .ForMember(a => a.SubjectId, cfg => cfg.MapFrom(src => src.Subject.Id))
            .ForMember(a => a.Average,
                cfg => cfg.MapFrom(src =>
                    decimal.Parse(src.Average, NumberStyles.Number, CultureInfo.CreateSpecificCulture("pl-PL"))));
    }
}