namespace Vulcanova.Features.Grades;

public class AverageGrade
{
    public int Id { get; set; }
    public int PeriodId { get; set; }
    public int PupilId { get; set; }
    public int AccountId { get; set; }
    public int SubjectId { get; set; }
    public decimal Average { get; set; }
}