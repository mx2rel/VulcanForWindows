using AutoMapper;
using System;
using Vulcanova.Features.Attendance;
using Vulcanova.Uonet.Api.Lessons;
using VulcanTest.Vulcan.Timetable;

namespace VulcanTest.Vulcan.Attendance.Report;

public class AttendanceMapperProfile : Profile
{
    public AttendanceMapperProfile()
    {
        CreateMap<LessonPayload, Lesson>()
            .ForMember(e => e.Id, cfg => cfg.MapFrom(src => new AccountEntityId { VulcanId = src.Id }))
            .ForMember(dest => dest.TeacherName, cfg => cfg.MapFrom(src => src.TeacherPrimary.DisplayName))
            .ForMember(dest => dest.Date, cfg => cfg.MapFrom(src => src.Day))
            .ForMember(dest => dest.No, cfg => cfg.MapFrom(src => src.TimeSlot.Position))
            .ForMember(dest => dest.Start,
                cfg => cfg.ConvertUsing(TimeZoneAwareTimeConverter.Instance, src => src.TimeSlot.Start))
            .ForMember(dest => dest.End,
                cfg => cfg.ConvertUsing(TimeZoneAwareTimeConverter.Instance, src => src.TimeSlot.End));

        CreateMap<Vulcanova.Uonet.Api.Lessons.PresenceType, Vulcanova.Features.Attendance.PresenceType >();
        CreateMap<Vulcanova.Uonet.Api.Common.Models.Subject, Vulcanova.Features.Shared.Subject>();
        CreateMap<Vulcanova.Uonet.Api.Common.Models.DateTimeInfo, DateTime>()
            .ConvertUsing(d => DateTimeOffset.FromUnixTimeMilliseconds(d.Timestamp).LocalDateTime);
    }
}