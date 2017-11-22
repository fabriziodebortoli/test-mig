using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microarea.AdminServer.Controllers.Helpers;
using Microarea.AdminServer.Controllers.Helpers.All;

namespace Microarea.AdminServer
{
	//============================================================================
	public class Startup
    {
		//---------------------------------------------------------------------
		public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		//---------------------------------------------------------------------
		public void ConfigureServices(IServiceCollection services)
        {
            // Add service and create Policy with options
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials());
            });

            services.AddMvc();
            services.Configure<AppOptions>(options => Configuration.GetSection("App").Bind(options));
			services.AddTransient<IJsonHelper, JsonHelper>();
			services.AddTransient<IHttpHelper, HttpHelper>();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		//---------------------------------------------------------------------
		public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();
            app.UseStaticFiles();

            app.UseCors("CorsPolicy");
           
            app.UseMvc(routes =>
            {
				routes.MapRoute("security", "tokens", defaults: new { controller = "Security", action = "Tokens" });
				routes.MapRoute("tbfs", "tbfs", defaults: new { controller = "TBFS", action = "init" });
				routes.MapRoute("database", "database", defaults: new { controller = "Database", action = "Database" });
				routes.MapRoute("default", "{controller=Admin}/{action=Index}/{id?}");
            });
        }
    }
}
