using DataAccess.Providers.Abstract;
using DataAccess.Providers.Abstract.Base;
using DataSource;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Models.System;

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

        public async Task<bool> CheckByTaskId(int id)
        {
            return await _context.Tasks.AnyAsync(task => task.TaskId == id);
        }

        public async Task<List<BaseTask>> GetAllExtensionsNull()
        {
            return await _context.Tasks.Where(t => t.Description == null &&
                                                   t.CreationDate == null &&
                                                   t.WorkTime == 0 &&
                                                   t.CheckTime == 0).ToListAsync();
        }
    }
}
