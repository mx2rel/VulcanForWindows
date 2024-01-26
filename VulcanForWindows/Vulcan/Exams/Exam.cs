using System;
using Vulcanova.Core.Data;
using Vulcanova.Features.Shared;

namespace Vulcanova.Features.Exams;

public class Exam
{
    public AccountEntityId Id { get; set; }
    public string Key { get; set; }
    public string Type { get; set; }
    public string Content { get; set; }
    public DateTime DateCreated { get; set; }
    public DateTime DateModify { get; set; }
    public DateTime Deadline { get; set; }
    public string CreatorName { get; set; }
    public Subject Subject { get; set; }
    public int PupilId { get; set; }

    public bool IsInPast { get => Deadline < DateTime.Now; }
}