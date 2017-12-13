using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microarea.Common;
using Microarea.AccountManager.Interfaces;
using Microarea.AccountManager.Library;
using Microarea.TbLoaderGate;
using System.Net;
using Microarea.DataService;

namespace WebApplication
{
    public class Startup
    {
        List<Assembly> modules = new List<Assembly>();
        List<IWebAppConfigurator> configurators = new List<IWebAppConfigurator>();
        private ILogger logger;

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddJsonFile("tbloader.config.json", optional: true);

            //if (env.IsDevelopment())
            //{
            //	// For more details on using the user secret store see https://go.microsoft.com/fwlink/?LinkID=532709
            //	builder.AddUserSecrets();
            //}

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();

            ReadModules();
        }

        private void ReadModules()
        {
            IConfigurationSection section = Configuration.GetSection("Modules");
            foreach (var module in section.GetChildren())
            {
                try
                {
                    AssemblyName an = new AssemblyName(module.Value);
                    var assembly = Assembly.Load(an);
                    foreach (Type t in assembly.GetTypes())
                        if (t.IsClass && typeof(IWebAppConfigurator).IsAssignableFrom(t))
                        {
                            configurators.Add((IWebAppConfigurator)Activator.CreateInstance(t));
                        }
                    modules.Add(assembly);
                }
                catch (Exception ex)
                {
                    if (logger != null)
                        logger.LogError(ex.Message);
                }
            }
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            //services.AddDbContext<ApplicationDbContext>(options =>
            //   options.UseSqlite(Configuration.GetConnectionString("DefaultConnection")));

            //  services.AddIdentity<ApplicationUser, IdentityRole>()
            //     .AddEntityFrameworkStores<ApplicationDbContext>()
            //     .AddDefaultTokenProviders();

            // Add service and create Policy with options
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials());
            });

            services.AddTransient<IAccountManagerProvider, AccountManagerProvider>();

            // Assembly asm = Assembly.Load(new AssemblyName("ControllerLib"));
            IMvcBuilder mvcBuilder = services.AddMvc();

            foreach (Assembly asm in modules)
                mvcBuilder.AddApplicationPart(asm);
            services.AddResponseCaching();
            services.AddMemoryCache();
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromSeconds(20 * 60);
                options.Cookie.HttpOnly = true;
            });

            services.AddDataServiceDependencies();

            
            foreach (var configurator in configurators)
                configurator.ConfigureServices(Configuration, services);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            app.UseCors("CorsPolicy");
            
            app.UseSession();

            app.UseResponseCaching();

            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            logger = loggerFactory.CreateLogger("WebServer");
            
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }
           
            app.UseWebSockets();
            //aggiungo gli handler di chiamata, mettere prima della UseFileServer()
            foreach (var configurator in configurators)
                configurator.Configure(app, env, loggerFactory, Configuration);

            //ATTENZIONE: se questa chiamata � messa prima di aggiungere degli handler di chiamata, questi non vengono chiamati!
            app.UseFileServer();

            app.UseMvc(routes =>
            {
                routes.MapRoute("default", "{controller=Home}/{action=Index}/{id?}");
                foreach (var configurator in configurators)
                    configurator.MapRoutes(routes);
            });
            // Add external authentication middleware below. 
            //To configure them please see https://go.microsoft.com/fwlink/?LinkID=532715
        }
    }
}

