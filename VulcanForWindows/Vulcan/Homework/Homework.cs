using System;
using VulcanForWindows.Classes;
using Vulcanova.Core.Data;
using Vulcanova.Features.Shared;

namespace Vulcanova.Features.Homework;

public class Homework : IDeadlineable
{
    public AccountEntityId Id { get; set; }
    public Guid Key { get; set; }
    public int PupilId { get; set; }
    public int HomeworkId { get; set; }
    public string Content { get; set; }
    public bool IsAnswerRequired { get; set; }
    public DateTime DateCreated { get; set; }
    public DateTime Deadline { get; set; }
    public DateTime? AnswerDate { get; set; }
    public DateTime? AnswerDeadline { get; set; }
    public string CreatorName { get; set; }
    public Subject Subject { get; set; }

    public IDeadlineable.Type type 
    { get => (((DateCreated - Deadline).TotalDays > 12) ? IDeadlineable.Type.Project : IDeadlineable.Type.Homework); }
}