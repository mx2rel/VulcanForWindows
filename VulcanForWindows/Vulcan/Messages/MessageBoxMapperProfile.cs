using System;
using AutoMapper;
using Vulcanova.Core.Data;
using Vulcanova.Uonet.Api.MessageBox;

namespace Vulcanova.Features.Messages;

public class MessageBoxMapperProfile : Profile
{
    public MessageBoxMapperProfile()
    {
        CreateMap<MessageBoxPayload, MessageBox>()
            .ForMember(e => e.Id, cfg => cfg.MapFrom(src => new AccountEntityId { VulcanId = src.Id }));

        CreateMap<MessagePayload, Message>()
            .ForMember(e => e.Id, cfg => cfg.MapFrom(src => new AccountEntityId<Guid> { VulcanId = src.Id }));
    }
}