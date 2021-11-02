using System;
using Models.Base;

namespace Models.System
{
    public class WorkingTask : BaseTask
    {
        public WorkingTask(BaseTask task)
        {
            SiteId = task.SiteId;
            TaskId = task.TaskId;
            Title = task.Title;
            Description = task.Description;
        }
    }
}