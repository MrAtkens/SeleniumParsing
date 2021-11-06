
namespace Models.System
{
    public class SimpleTask : BaseTask
    { 
        public int AuthorId { get; set; }
        public string TaskType { get; set; }
        public bool TaskStatus { get; set; }
        public double Price { get; set; }
        public string Url { get; set; }
        public string AuthorName { get; set; }
        public bool Status { get; set; }
        public int WorkTime { get; set; } = 0;
        public int CheckTime { get; set; } = 0;
        public string WorkCount { get; set; }
    }
}