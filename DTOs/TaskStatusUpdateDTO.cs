using Models.Enums;

namespace DTOs
{
    public class TaskStatusUpdateDTO
    {
        public string ParsingQuery { get; set; }
        public Status TaskStatus { get; set; }

        public TaskStatusUpdateDTO(string parsingQuery, Status taskStatus)
        {
            ParsingQuery = parsingQuery;
            TaskStatus = taskStatus;
        }
    }
}