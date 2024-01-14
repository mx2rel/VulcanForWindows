using System;
using System.Collections.Generic;

namespace Vulcanova.Core.Data;

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

public sealed class AccountEntityId : AccountEntityId<int>
{
}