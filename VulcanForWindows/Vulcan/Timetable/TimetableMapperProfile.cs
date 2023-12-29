using AutoMapper;
using System;
using Vulcanova.Uonet.Api.Common.Models;
using Vulcanova.Uonet.Api.Schedule;
using VulcanTest.Vulcan.Timetable.Changes;

namespace VulcanTest.Vulcan.Timetable;

public class TimetableMapperProfile : Profile
{
    public TimetableMapperProfile()
    {
        CreateMap<TimeSlot, TimetableTimeSlot>()
            .ForMember(dest => dest.Start, cfg => cfg.ConvertUsing(TimeZoneAwareTimeConverter.Instance, src => src.Start))
            .ForMember(dest => dest.End, cfg => cfg.ConvertUsing(TimeZoneAwareTimeConverter.Instance, src => src.End));

        CreateMap<ScheduleEntryPayload, TimetableEntry>()
            .ForMember(dest => dest.RoomName, cfg => cfg.MapFrom(src => src.Room.Code))
            .ForMember(dest => dest.TeacherName, cfg => cfg.MapFrom(src => src.TeacherPrimary.DisplayName))
            .ForMember(dest => dest.Date, cfg => cfg.MapFrom(src => ConvertDateTimeInfoToDateTime(src.Date)))
            .ForMember(dest => dest.Subject, cfg => cfg.MapFrom(src => ConvertSub(src.Subject)));

        CreateMap<ScheduleChangeEntryPayload, TimetableChangeEntry>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.TimetableEntryId, opt => opt.MapFrom(src => src.ScheduleId))
            .ForMember(dest => dest.UnitId, opt => opt.MapFrom(src => src.UnitId))
            .ForMember(dest => dest.PupilId, opt => opt.MapFrom(src => 0)) // Set as needed
            .ForMember(dest => dest.AccountId, opt => opt.MapFrom(src => 0)) // Set as needed
            .ForMember(dest => dest.Subject, cfg => cfg.MapFrom(src => ConvertSub(src.Subject)))
            .ForMember(dest => dest.LessonDate, opt => opt.MapFrom(src => src.LessonDate.Date))
            .ForMember(dest => dest.ChangeDate, opt => opt.MapFrom(src => (DateTime?)src.ChangeDate))
            .ForMember(dest => dest.TimeSlot, opt => opt.MapFrom(src => src.TimeSlot))
            .ForMember(dest => dest.Note, opt => opt.MapFrom(src => src.Note))
            .ForMember(dest => dest.Event, opt => opt.MapFrom(src => src.Event != null ? src.Event.ToString() : null))
            .ForMember(dest => dest.Reason, opt => opt.MapFrom(src => src.Reason))
            .ForMember(dest => dest.TeacherName, opt => opt.MapFrom(src => src.TeacherPrimary.Name)) // Assuming 'Name' property in Teacher
            .ForMember(dest => dest.RoomName, opt => opt.MapFrom(src => src.Room)) // Assuming 'Name' property in Room
            .ForMember(dest => dest.Change, opt => opt.MapFrom(src => src.Change));

    }

    private DateTime? ConvertDateTimeInfoToDateTime(DateTimeInfo dateTimeInfo)
    {
        // You need to implement the conversion logic from DateTimeInfo to DateTime here
        // This depends on the structure of your DateTimeInfo class

        // Example: Assuming DateTimeInfo has a DateTime property
        if (dateTimeInfo == null) return null;
        return dateTimeInfo.Time;
    }
    private Vulcanova.Features.Shared.Subject ConvertSub(Subject sub)
    {
        var s = new Vulcanova.Features.Shared.Subject();
        if (sub != null) { 
        s.Name = sub.Name;
        s.Position = sub.Position;
        s.Kod = sub.Kod;
        s.Id = sub.Id;
            s.Key = sub.Key;
        }
        return s;
    }
}