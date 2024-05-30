using Microsoft.UI.Xaml;
using System;
using System.ComponentModel;
using System.Linq;
using VulcanForWindows;
using VulcanForWindows.Classes.VulcanGradesDb;
using Vulcanova.Features.Grades.Summary;

namespace Vulcanova.Features.Grades;

public class Grade : INotifyPropertyChanged
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
    public bool HasBeenModifiedByTeacher => DateCreated != DateModify;
    public decimal? ActualValue
    {
        get
        {
            if (VulcanValue == null) return null;
            if (AverageCalculator.TryGetValueFromContentRaw(ContentRaw, out var d))
                return d;
            return VulcanValue;
        }
    }
    public decimal? VulcanValue { get; set; }
    public Column Column { get; set; }
    public bool IsHipothetic { get; set; }
    public bool IsRecent { get => DateModify.Date >= DateTime.Today.AddDays(-1) || DateModify >= MainWindow.lastLaunch; }
    public float ClassAverage { get; private set; } = -1;
    public string ClassAverageDisplay => $"Œrednia klasy: {ClassAverage.ToString("0.00")}";
    public Visibility ClassAverageVibility => (ClassAverage == -1) ? Visibility.Collapsed : Visibility.Visible;
    public bool IsHipotheticOrRecent => (IsRecent || IsHipothetic);

    public async void CalculateClassAverage()
    {
        try
        {

            var classRequest = (await ClassmateGradesService.GetSingleClassmateColumn(Column.Id));
            if (classRequest == null) return;
            var classGrades = classRequest.Grades.Select(r => r.Value);
            if (classGrades.Count() > 4)
                ClassAverage = classGrades.Sum() / classGrades.Count();

            OnPropertyChanged(nameof(ClassAverageDisplay));
            OnPropertyChanged(nameof(ClassAverageVibility));
        }
        catch (System.Net.Http.HttpRequestException e)
        {
            return;
        }
    }


    public event PropertyChangedEventHandler PropertyChanged;

    public virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}