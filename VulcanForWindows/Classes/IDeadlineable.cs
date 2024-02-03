using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vulcanova.Features.Exams;
using Vulcanova.Features.Shared;

namespace VulcanForWindows.Classes
{
    /// <summary>
    /// Interface for Exams and Homework
    /// </summary>
    public interface IDeadlineable
    {

        public enum DeadlineableType
        {
            ExamOrTest, Homework, Project
        }

        public DeadlineableType Type { get; }
        public string Content { get; set; }
        public string CreatorName { get; set; }
        public Subject Subject { get; set; }
        public DateTime Deadline { get; set; }
        public DateTime DateCreated { get; set; }
        public bool IsInPast { get; }

    }

    public class Deadlineable
    {
        public IDeadlineable createdFrom;
        public IDeadlineable.DeadlineableType Type { get; }
        public string Content { get; set; }
        public string CreatorName { get; set; }
        public Subject Subject { get; set; }
        public DateTime Deadline { get; set; }
        public DateTime DateCreated { get; set; }
        public bool IsInPast { get; }
        public string TypeText
        {
            get
            {
                switch (Type)
                {
                    case IDeadlineable.DeadlineableType.ExamOrTest:
                        return (createdFrom as Exam).Type;
                    case IDeadlineable.DeadlineableType.Homework:
                        return "Zadanie Domowe";
                    case IDeadlineable.DeadlineableType.Project:
                        return "Projekt";
                    default:
                        return "???";
                }
            }
        }
        public int DeadlineIn => (int)Math.Ceiling((Deadline - DateTime.Now).TotalDays);
        public Deadlineable(IDeadlineable d)
        {
            Type = d.Type;
            Content = d.Content;
            CreatorName = d.CreatorName;
            Subject = d.Subject;
            Deadline = d.Deadline;
            DateCreated = d.DateCreated;
            IsInPast = d.IsInPast;
            createdFrom = d;
        }
    }

    /// <summary>
    /// Extension methods for IDeadlineable interface
    /// </summary>
    public static class DeadlineableExtensions
    {
        public static bool IsInPast(this IDeadlineable deadlineable)
        {
            return deadlineable.Deadline < DateTime.Now;
        }
    }
}
