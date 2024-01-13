using System;
using LiteDB;
using Vulcanova.Features.Shared;

namespace Vulcanova.Features.Attendance.Report;

public class AttendanceReport
{
    public string Id { get; set; }
    public DateTime DateGenerated { get; set; }
    public int AccountId { get; set; }
    public Subject Subject { get; set; }
    public int Presence { get; set; }
    public int Absence { get; set; }
    public int Late { get; set; }

    [BsonIgnore]
    public int PresenceAndLate => Presence + Late;

    [BsonIgnore]
    public float PresencePercentage => (float) (Presence + Late) / (Presence + Absence + Late) * 100;
    [BsonIgnore]
    public string PresencePercentageDisplay => PresencePercentage.ToString("0.00")+"%";
}