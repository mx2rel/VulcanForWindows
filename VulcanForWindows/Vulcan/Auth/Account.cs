using System.Collections.Generic;
using System.Linq;
using Vulcanova.Uonet.Api.Auth;

namespace Vulcanova.Features.Auth.Accounts;

public class Account
{
    public int Id { get; set; }
    public Pupil Pupil { get; set; }
    public Unit Unit { get; set; }
    public ConstituentUnit ConstituentUnit { get; set; }
    public bool IsActive { get; set; }
    public List<Vulcanova.Features.Shared.Period> Periods { get; set; }
    public string IdentityThumbprint { get; set; }
    public Login Login { get; set; }
    public string[] Capabilities { get; set; }
    public SenderEntry SenderEntry { get; set; }
    public string ClassDisplay { get; set; }
    public string Context { get; set; }
    public string Partition { get; set; }

    public Shared.Period CurrentPeriod => Periods.Where(r => r.Current).ToArray()[0];
}