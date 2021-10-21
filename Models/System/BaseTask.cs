using System;
using Models.Base;

namespace Models.System
{
    public class BaseTask : Entity, IDisposable
    {
        public int TaskId { get; set; }
        public int AuthorId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string TaskType { get; set; }
        public string Price { get; set; }
        public string Url { get; set; }
        public string AuthorName { get; set; }
        public bool Status { get; set; }
        public int WorkTime { get; set; } = 0;
        public int CheckTime { get; set; } = 0;
        public string WorkCount { get; set; }
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}