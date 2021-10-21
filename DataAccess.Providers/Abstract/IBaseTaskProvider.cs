using System;
using System.Collections.Generic;
using DataAccess.Providers.Abstract.Base;
using System.Threading.Tasks;
using Models.System;

namespace DataAccess.Providers.Abstract
{
    public interface IBaseTaskProvider : IProvider<BaseTask, Guid>
    {
        Task CheckTasks(List<BaseTask> newTasks);
        Task<bool> CheckByTaskId(int id);

        Task<List<BaseTask>> GetAllExtensionsNull();
    }
}
