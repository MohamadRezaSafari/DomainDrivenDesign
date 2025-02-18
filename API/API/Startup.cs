using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System.Linq;
using Framework.AssemblyHelper;
using Framework.Core.DependencyInjection;
using Framework.Persistence;
using HR.Persistence;
using HR.ReadModel.Context.Models;
using Microsoft.EntityFrameworkCore;
using System;
using Microsoft.OpenApi.Any;
using System.Configuration;
using System.Globalization;
using HR.EmployeeContext.Facade.Contract;
using HR.EmployeeContext.Infrastructure.AntiCorruptionLayer.Shifts;

namespace API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }


        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers()
                .AddJsonOptions(options =>
                    options.JsonSerializerOptions.Converters.Add(new TimeSpanToStringConverter()));

            var assemblyDiscovery = new AssemblyDiscovery("HR*.dll");
            var registrars = assemblyDiscovery.DiscoverInstances<IRegistrar>("HR").ToList();
            foreach (var registrar in registrars)
            {
                registrar.Register(services, assemblyDiscovery);
            }

            services.AddDbContext<IDbContext, HRDbContext>(op =>
            {
                op.UseSqlServer(Configuration.GetConnectionString("HR_Developer"));
            });
            services.AddDbContext<HR_DeveloperContext>(op =>
            {
                op.UseSqlServer(Configuration.GetConnectionString("HR_Developer"));
            });


            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "API", Version = "v1" });
                c.MapType<TimeSpan>(() => new OpenApiSchema
                {
                    Type = "string",
                    Example = new OpenApiString("00:00:00")
                });
            });
        }


        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1"));
            }

            app.UseRouting();

            app.UseAuthorization();


            var cultureInfo = new CultureInfo("en-US");
            cultureInfo.NumberFormat.CurrencySymbol = "€";

            CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
            CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;


            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
