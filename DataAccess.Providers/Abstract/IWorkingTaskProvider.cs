using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DataAccess.Providers.Abstract.Base;
using Models.System;

namespace DataAccess.Providers.Abstract
{
    public interface IWorkingTaskProvider : IProvider<WorkingTask, Guid>
    {
        Task<WorkingTask> GetByTaskId(int id, int siteId);
        Task<List<WorkingTask>> GetAllBySiteId(int siteId);
        Task<bool> CheckByTaskId(int id, int siteId);
    }
}