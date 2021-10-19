using DataAccess.Providers;
using DataAccess.Providers.Abstract;
using DataSource;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Services.Business;

namespace TaskerTasks
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public Startup()
        {
            Configuration = new ConfigurationBuilder()
           .AddJsonFile("appsettings.json")
           .AddJsonFile("appsettings.CoreConfigurations.json")
           .Build();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            //Data base connection
            services.AddDbContext<ApplicationContext>(
            options => options.UseSqlServer(
                Configuration.GetConnectionString("DevConnection")));
            //DataAccess Providers
            services.AddScoped<IBaseTaskProvider, EntityBaseTaskProvider>();


            //Services
            services.AddTransient<AvisoService>();

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo {Title = "TaskerTasks", Version = "v1"});
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "TaskerTasks v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}