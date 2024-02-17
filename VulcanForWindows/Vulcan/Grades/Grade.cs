using System;

namespace Vulcanova.Features.Grades;

public class Grade
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public string CreatorName { get; set; }
    public int PupilId { get; set; }
    public string ContentRaw { get; set; }
    public string Content { get; set; }
    public string Comment { get; set; }
    public DateTime? DateCreated { get; set; }
    public DateTime DateModify { get; set; }
    public bool IsModified => DateCreated != DateModify;
    public decimal? Value { get; set; }
    public Column Column { get; set; }
    public string BgColor => (Column.Name == "Hipotetyczna ocena") ? "#7a420a" : "Green";
    public bool IsHipothetic { get; set; }
    public bool IsRecent { get => DateModify.Date >= DateTime.Today.AddDays(-2); }

    public bool IsHipotheticOrRecent => (IsRecent || IsHipothetic);
}