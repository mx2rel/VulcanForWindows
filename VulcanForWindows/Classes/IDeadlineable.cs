using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vulcanova.Features.Shared;

namespace VulcanForWindows.Classes
{
    /// <summary>
    /// Interface for Exams and Homework
    /// </summary>
    public interface IDeadlineable
    {

        public enum Type
        {
            Exam, Homework, Project
        }

        public Type type { get; }
        public string Content { get; set; }
        public string CreatorName { get; set; }
        public Subject Subject { get; set; }
        public DateTime Deadline { get; set; }
        public DateTime DateCreated { get; set; }

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
