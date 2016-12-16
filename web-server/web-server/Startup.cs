using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microarea.Common;
using Microsoft.AspNetCore.Http;

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
				.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

			

			if (env.IsDevelopment())
			{
				// For more details on using the user secret store see https://go.microsoft.com/fwlink/?LinkID=532709
				builder.AddUserSecrets();
			}

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
						if (typeof(IWebAppConfigurator).IsAssignableFrom(t))
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

			services.AddSession();

			// Add service and create Policy with options
			services.AddCors(options =>
			{
				options.AddPolicy("CorsPolicy",
					builder => builder.AllowAnyOrigin()
					.AllowAnyMethod()
					.AllowAnyHeader()
					.AllowCredentials());
			});

			// Assembly asm = Assembly.Load(new AssemblyName("ControllerLib"));
			IMvcBuilder mvcBuilder = services.AddMvc();//.AddApplicationPart(asm);
			foreach (Assembly asm in modules)
				mvcBuilder.AddApplicationPart(asm);
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
		{
			loggerFactory.AddConsole(Configuration.GetSection("Logging"));
			loggerFactory.AddDebug();
			//logger = loggerFactory.CreateLogger("WebServer");

			

			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
				app.UseDatabaseErrorPage();
				app.UseBrowserLink();
			}
			else
			{
				app.UseExceptionHandler("/Home/Error");
			}
			app.UseWebSockets();
			//aggiungo gli handler di chiamata, mettere prima della UseFileServer()
			foreach (var configurator in configurators)
				configurator.Configure(app, env, loggerFactory);
			//ATTENZIONE: se questa chiamata è messa prima di aggiungere degli handler di chiamata, questi non vengono chiamati!
			app.UseFileServer();
			app.UseCors("CorsPolicy");
			// Add external authentication middleware below. To configure them please see https://go.microsoft.com/fwlink/?LinkID=532715
			app.UseSession();

			app.UseMvc(routes =>
			{
				foreach (var configurator in configurators)
					configurator.MapRoutes(routes);
			});
		}
		
	}
}
