using System;
using System.Collections.Generic;
using DataAccess.Providers.Abstract.Base;
using System.Threading.Tasks;
using Models.System;

namespace DataAccess.Providers.Abstract
{
    public interface ITaskProvider : IProvider<SimpleTask, Guid>
    {
        Task CheckTasks(List<SimpleTask> newTasks);
        Task<bool> CheckByTaskId(int id, int siteId);
        Task<List<SimpleTask>> GetAllExtensionsNull();
        Task<List<int>> GetAllTaskId();
        Task<SimpleTask> GetByTaskId(int id, int siteId);
        Task<List<SimpleTask>> GetAllBySiteId(int siteId);
        Task<List<SimpleTask>> GetAllNew();
        Task<int> GetCountSiteId(int siteId);
    }
}
