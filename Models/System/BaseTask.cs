using System;
using Models.Base;

namespace Models.System
{
    public abstract class BaseTask : Entity, IDisposable
    {
        public int TaskId { get; set; }
        public int SiteId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}