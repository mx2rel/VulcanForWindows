using System;
using AutoMapper;
using Vulcanova.Uonet.Api.Common.Models;
using Vulcanova.Uonet.Api.Notes;

namespace Vulcanova.Features.Notes;

public class NoteMapperProfile : Profile
{
    public NoteMapperProfile()
    {
        CreateMap<NotesPayload, Note>()
            .ForMember(n => n.PupilId, cfg => cfg.MapFrom(src => src.IdPupil))
            .ForMember(n => n.CreatorName, cfg => cfg.MapFrom(src => src.Creator.DisplayName))
            .ForMember(h => h.CategoryName, cfg => cfg.MapFrom(src => src.Category.Name))
            .ForMember(h => h.CategoryType, cfg => cfg.MapFrom(src => src.Category.Type))
            .ForMember(h => h.DateModified,
                cfg => cfg.MapFrom(src => new DateTime(1970, 1, 1).AddMilliseconds(src.DateModify.Timestamp)));


        CreateMap<DateTimeInfo, DateTime>()
            .ConvertUsing(d => DateTimeOffset.FromUnixTimeMilliseconds(d.Timestamp).LocalDateTime);
    }
}