using Models.Base;
using System;

namespace Models.Models
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
        public int WorkTime { get; set; }
        public int CheckTime { get; set; }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}