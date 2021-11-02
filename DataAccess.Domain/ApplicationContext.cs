using System;
using Microsoft.EntityFrameworkCore;
using Models.System;

namespace DataSource
{
    public sealed class ApplicationContext : DbContext
    {
        public ApplicationContext(DbContextOptions options) : base(options)
        {
            try
            {
                // It should throw exception when migrations are not available,
                // for example in a tests
                Database.EnsureCreated();
            }
            catch (Exception e)
            {
                Database.EnsureCreated();
            }
                
        }

        public DbSet<SimpleTask> Tasks { get; set; }
        public DbSet<WorkingTask> WorkingTasks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SimpleTask>()
                .HasIndex(a => a.TaskId);
        }
    }
}
