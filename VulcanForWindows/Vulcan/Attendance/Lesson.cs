using System;
using System.Collections.Generic;
using LiteDB;
using Vulcanova.Features.Shared;
using Vulcanova.Uonet.Api.Lessons;

namespace Vulcanova.Features.Attendance;

public class Lesson
{
    public AccountEntityId Id { get; set; }
    public int No { get; set; }
    public bool CalculatePresence { get; set; }
    public DateTime Date { get; set; }
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public string Topic { get; set; }
    public bool Visible { get; set; }
    public string TeacherName { get; set; }
    public Subject Subject { get; set; }
    public PresenceType PresenceType { get; set; }
    public JustificationStatus? JustificationStatus { get; set; }
    public int LessonClassId { get; set; }
    
    [BsonIgnore]
    public bool CanBeJustified => PresenceType != null 
                                  && (PresenceType.Late || PresenceType.Absence)
                                  && !PresenceType.AbsenceJustified
                                  && JustificationStatus == null;
}
    
public class PresenceType
{
    public bool Absence { get; set; }
    public bool AbsenceJustified { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; }
    public int Id { get; set; }
    public bool Late { get; set; }
    public bool LegalAbsence { get; set; }
    public string Name { get; set; }
    public int Position { get; set; }
    public bool Presence { get; set; }
    public bool Removed { get; set; }
    public string Symbol { get; set; }
}

public sealed class AccountEntityId : AccountEntityId<int>
{
}

public class AccountEntityId<T>
{
    public T VulcanId { get; set; }
    public int AccountId { get; set; }

    protected bool Equals(AccountEntityId<T> other)
    {
        return EqualityComparer<T>.Default.Equals(VulcanId, other.VulcanId) && AccountId == other.AccountId;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((AccountEntityId<T>)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(VulcanId, AccountId);
    }

    public static bool operator ==(AccountEntityId<T> left, AccountEntityId<T> right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(AccountEntityId<T> left, AccountEntityId<T> right)
    {
        return !Equals(left, right);
    }
}