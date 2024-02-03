using AutoMapper;
using System;
using Vulcanova.Core.Data;
using Vulcanova.Features.Shared;
using Vulcanova.Uonet.Api.Common.Models;
using Vulcanova.Uonet.Api.Homework;

namespace Vulcanova.Features.Homework;

public class HomeworkMapperProfile : Profile
{
    public HomeworkMapperProfile()
    {
        CreateMap<Uonet.Api.Common.Models.Subject, Vulcanova.Features.Shared.Subject>();
        CreateMap<HomeworkPayload, Homework>()
            .ForMember(e => e.Id, cfg => cfg.MapFrom(src => new AccountEntityId { VulcanId = src.Id }))
            .ForMember(h => h.PupilId, cfg => cfg.MapFrom(src => src.IdPupil))
            .ForMember(h => h.HomeworkId, cfg => cfg.MapFrom(src => src.IdHomework))
            .ForMember(h => h.CreatorName, cfg => cfg.MapFrom(src => src.Creator.DisplayName));

        CreateMap<DateTimeInfo, DateTime>()
            .ConvertUsing(d => DateTimeOffset.FromUnixTimeMilliseconds(d.Timestamp).LocalDateTime);
    }
}