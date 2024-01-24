using AutoMapper;
using System;
using Vulcanova.Core.Data;
using Vulcanova.Uonet.Api.Common.Models;
using Vulcanova.Uonet.Api.Exams;

namespace Vulcanova.Features.Exams;

public class ExamsMapperProfile : Profile
{
    public ExamsMapperProfile()
    {
        CreateMap<ExamPayload, Exam>()
            .ForMember(e => e.Id, cfg => cfg.MapFrom(src => new AccountEntityId { VulcanId = src.Id }))
            .ForMember(e => e.CreatorName, cfg => cfg.MapFrom(src => src.Creator.DisplayName));

        CreateMap<Vulcanova.Uonet.Api.Common.Models.Subject, Vulcanova.Features.Shared.Subject>();

        CreateMap<DateTimeInfo, DateTime>()
            .ConvertUsing(d => DateTimeOffset.FromUnixTimeMilliseconds(d.Timestamp).LocalDateTime);
    }
}