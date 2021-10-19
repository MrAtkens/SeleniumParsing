using DataAccess.Providers.Abstract;
using DataAccess.Providers.Abstract.Base;
using DataSource;
using Models.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataAccess.Providers
{
    public class EntityBaseTaskProvider : EntityProvider<ApplicationContext, BaseTask, Guid>, IBaseTaskProvider
    {
        private readonly ApplicationContext _context;
        public EntityBaseTaskProvider(ApplicationContext context) : base(context)
        {
            _context = context;
        }

        public async Task CheckTasks(List<BaseTask> newTasks)
        {
            foreach (BaseTask task in newTasks)
            {
                var exists = await FirstOrDefault(x => x.Title.Equals(task.Title) && x.TaskId.Equals(task.TaskId));
                if (exists == null)
                {
                    newTasks.Remove(task);
                }
            }
            await AddRange(newTasks);
        }
    }
}
