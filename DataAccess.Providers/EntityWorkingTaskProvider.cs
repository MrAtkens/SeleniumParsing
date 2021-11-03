using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAccess.Providers.Abstract;
using DataAccess.Providers.Abstract.Base;
using DataSource;
using Microsoft.EntityFrameworkCore;
using Models.System;

namespace DataAccess.Providers
{
    public class EntityWorkingTaskProvider : EntityProvider<ApplicationContext, WorkingTask, Guid>, IWorkingTaskProvider
    {
        private readonly ApplicationContext _context;

        public EntityWorkingTaskProvider(ApplicationContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<WorkingTask>> GetAllBySiteId(int siteId)
        {
            return await _context.WorkingTasks.Where(t => t.SiteId == siteId).ToListAsync();
        }

        public async Task<WorkingTask> GetByTaskId(int id, int siteId)
        {
            return await _context.WorkingTasks.FirstOrDefaultAsync(t => t.TaskId.Equals(id) && t.SiteId.Equals(siteId));
        }
        
        public async Task<bool> CheckByTaskId(int id, int siteId)
        {
            return await _context.WorkingTasks.AnyAsync(task => task.TaskId == id && task.SiteId == siteId);
        }
    }
}