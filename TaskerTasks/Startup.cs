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
using TaskerTasks.Swagger;

namespace TaskerTasks
{
    public class Startup
    {
        private IConfiguration Configuration { get; }
        public Startup()
        {
            Configuration = new ConfigurationBuilder()
           .AddJsonFile("appsettings.json")
           .AddJsonFile("appsettings.CoreConfigurations.json")
           .Build();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            //Data base connectionW
            services.AddDbContext<ApplicationContext>(
            options => options.UseNpgsql(
                Configuration.GetConnectionString("DevConnection")));
            //DataAccess Providers
            services.AddScoped<ITaskProvider, EntityTaskProvider>();

            //Services
            services.AddTransient<AvisoService>();
            services.AddTransient<AdvigoService>();

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo {Title = "TaskerTasks", Version = "v1"});
                c.OperationFilter<SwaggerFileFilter>();
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                
                app.UseSwaggerUI(c => {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "TaskerTasks v1");
                });
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}