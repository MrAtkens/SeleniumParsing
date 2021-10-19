using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace DataSource
{
    // This class is a manager, which is makes migrations and different else database manipulations
    // Please, do not use it in a solution
    public class ApplicationContextFactory : IDesignTimeDbContextFactory<ApplicationContext>
    {
        public ApplicationContext CreateDbContext(string[] args)
        {
            // Build config
            IConfiguration config = new ConfigurationBuilder()
                 .AddJsonFile("appsettings.json")
                 .Build();

            var connectionString = config.GetConnectionString("DevConnection");

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new ApplicationContext(optionsBuilder.Options);
        }
    }
}