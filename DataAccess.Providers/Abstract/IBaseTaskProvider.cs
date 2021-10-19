using System;
using System.Collections.Generic;
using DataAccess.Providers.Abstract.Base;
using System.Threading.Tasks;
using Models.Models;

namespace DataAccess.Providers.Abstract
{
    public interface IBaseTaskProvider : IProvider<BaseTask, Guid>
    {
        Task CheckTasks(List<BaseTask> newTasks);
    }
}
