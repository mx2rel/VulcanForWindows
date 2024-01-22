using AutoMapper;
using Vulcanova.Core.Data;
using Vulcanova.Uonet.Api.Exams;

namespace Vulcanova.Features.Exams;

public class ExamsMapperProfile : Profile
{
    public ExamsMapperProfile()
    {
        CreateMap<ExamPayload, Exam>()
            .ForMember(e => e.Id, cfg => cfg.MapFrom(src => new AccountEntityId { VulcanId = src.Id }))
            .ForMember(e => e.CreatorName, cfg => cfg.MapFrom(src => src.Creator.DisplayName));
    }
}