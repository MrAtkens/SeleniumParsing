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
    public class EntityTaskProvider : EntityProvider<ApplicationContext, SimpleTask, Guid>, ITaskProvider
    {
        private readonly ApplicationContext _context;
        public EntityTaskProvider(ApplicationContext context) : base(context)
        {
            _context = context;
        }

        public async Task CheckTasks(List<SimpleTask> newTasks)
        {
            foreach (var task in newTasks)
            {
                var exists = await FirstOrDefault(x => x.Title.Equals(task.Title) && x.TaskId.Equals(task.TaskId));
                if (exists == null)
                {
                    newTasks.Remove(task);
                }
            }
            await AddRange(newTasks);
        }

        public async Task<bool> CheckByTaskId(int id, int siteId)
        {
            return await _context.Tasks.AnyAsync(task => task.TaskId == id && task.SiteId == siteId);
        }

        public async Task<List<SimpleTask>> GetAllExtensionsNull()
        {
            return await _context.Tasks.Where(t => t.Description == null &&
                                                   t.CreationDate == null &&
                                                   t.WorkTime == 0 &&
                                                   t.CheckTime == 0).ToListAsync();
        }

        public async Task<List<int>> GetAllTaskId()
        {
            return await _context.Tasks.Select(t => t.TaskId).ToListAsync();
        }

        public async Task<List<SimpleTask>> GetAllBySiteId(int siteId)
        {
            return await _context.Tasks.Where(t => t.SiteId == siteId).ToListAsync();
        }

        public async Task<List<SimpleTask>> GetAllNew()
        {
            return await _context.Tasks.Where(t => t.TaskStatus).ToListAsync();
        }

        public async Task<SimpleTask> GetByTaskId(int id, int siteId)
        {
            return await _context.Tasks.FirstOrDefaultAsync(t => t.TaskId.Equals(id) && t.SiteId.Equals(siteId));
        }

        public async Task<int> GetCountSiteId(int siteId)
        {
            return await _context.Tasks.Where(t => t.SiteId == siteId).CountAsync();
        }
    }
}
