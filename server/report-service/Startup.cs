using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Microarea.RSWeb.Models;
using System;

namespace Microarea.RSWeb
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            //if (env.IsEnvironment("Development"))
            //{
                // This will push telemetry data through Application Insights pipeline faster, allowing you to view results immediately.
                //builder.AddApplicationInsightsSettings(developerMode: true);
            //}

            builder.AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            //services.AddApplicationInsightsTelemetry(Configuration);

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
            services.AddResponseCaching();
            services.AddMemoryCache();  // AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromSeconds(20 * 60);
                /*
                * https://docs.microsoft.com/en-us/aspnet/core/fundamentals/app-state
                  Session uses a cookie to track and identify requests from a single browser. By default, 
                  this cookie is named ".AspNet.Session", and it uses a path of "/". 
                  Because the cookie default does not specify a domain, it is not made available to the client-side script
                  on the page (because CookieHttpOnly defaults to true).
                 */
                //options.CookieName = ".AdventureWorks.Session";
                //options.CookieHttpOnly = true;
                options.Cookie.HttpOnly = true;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
           app.UseCors("CorsPolicy");

           app.UseSession();

           app.UseResponseCaching();

           app.UseStaticFiles();

           app.UseWebSockets();

           loggerFactory.AddConsole(Configuration.GetSection("Logging"));
           loggerFactory.AddDebug();

            //app.UseApplicationInsightsRequestTelemetry();
            //app.UseApplicationInsightsExceptionTelemetry();

            //new WebAppConfigurator().Configure(app, env, loggerFactory);

            app.Use(RSSocketHandler.Listen);

           app.UseMvc();
        }
    }
}
