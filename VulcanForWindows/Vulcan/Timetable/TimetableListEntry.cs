using System;
using Vulcanova.Uonet.Api.Schedule;

namespace Vulcanova.Features.Timetable
{

    public class TimetableListEntry
    {
        public int? OriginalId { get; set; }
        public OverridableValue<int> No { get; set; }
        public OverridableRefValue<string> SubjectName { get; set; }
        public OverridableRefValue<string> TeacherName { get; set; }
        public OverridableRefValue<string> Event { get; set; }
        public OverridableValue<DateTime> Date { get; set; }
        public OverridableValue<DateTime> Start { get; set; }
        public OverridableValue<DateTime> End { get; set; }
        public OverridableRefValue<string> RoomName { get; set; }
        public ChangeDetails Change { get; set; }

        public class ChangeDetails
        {
            public ChangeType ChangeType { get; set; }
            public RescheduleKind? RescheduleKind { get; set; }
            public string ChangeNote { get; set; }
        }


        public string textColor => (Change == null) ? White : (Change.ChangeType == ChangeType.Substitution ? White : Red);
        public string textDecor => (Change == null) ? "None" : (Change.ChangeType == ChangeType.Substitution ? "None" : "Strikethrough");


        public string subName => SubjectName?.Value ?? Event.Value ;
        public string displayTime => $"{Start.Value.Hour.ToString("00")}:{Start.Value.Minute.ToString("00")}\n{End.Value.Hour.ToString("00")}:{End.Value.Minute.ToString("00")}";

        string Red = "#d15045";
        string Yellow = "#ffc400";
        string White = "#FFFFFF";

        public string SubNameColor => (textColor == Red) ? Red : ((SubjectName.Override == null) ? White : Yellow);
        public string TeacherColor => (textColor == Red) ? Red : ((TeacherName.Override == null) ? White : Yellow);
        public string RoomColor => (textColor == Red) ? Red : ((RoomName.Override == null) ? White : Yellow);

        public string room => RoomName.Value;
        public string teacher => TeacherName.Value;

        public enum RescheduleKind
        {
            Removed,
            Added
        }

        public abstract class BaseOverridableValue<T> : IComparable<BaseOverridableValue<T>>, IFormattable
            where T : IComparable
        {
            public abstract T Value { get; }

            public int CompareTo(BaseOverridableValue<T> other)
            {
                if (ReferenceEquals(this, other)) return 0;
                if (ReferenceEquals(null, other)) return 1;
                return Value.CompareTo(other.Value);
            }

            public string ToString(string format, IFormatProvider formatProvider)
            {
                if (Value is IFormattable formattable)
                {
                    return formattable.ToString(format, formatProvider);
                }

                return ToString();
            }

            public override string ToString()
            {
                return Value?.ToString();
            }
        }

        public class OverridableRefValue<T> : BaseOverridableValue<T>
            where T : IComparable
        {
            public T OriginalValue { get; init; }
            public T Override { get; set; }

            public static implicit operator OverridableRefValue<T>(T value)
            {
                return new OverridableRefValue<T>
                {
                    OriginalValue = value
                };
            }

            public static implicit operator T(OverridableRefValue<T> obj)
            {
                return obj.Override ?? obj.OriginalValue;
            }

            public override T Value => Override ?? OriginalValue;
        }

        public class OverridableValue<T> : BaseOverridableValue<T>
            where T : struct, IComparable
        {
            public T OriginalValue { get; init; }
            public T? Override { get; set; }

            public static implicit operator OverridableValue<T>(T value)
            {
                return new OverridableValue<T>
                {
                    OriginalValue = value
                };
            }

            public static implicit operator T(OverridableValue<T> obj)
            {
                return obj.Override ?? obj.OriginalValue;
            }

            public override T Value => Override.GetValueOrDefault(OriginalValue);
        }
    }
}