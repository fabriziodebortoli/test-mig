using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Microarea.Common;
using Microarea.RSWeb.Models;
using System;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microarea.RSWeb.Controllers;
using Microsoft.Extensions.Configuration;

namespace Microarea.RSWeb
{
    public class WebAppConfigurator: IWebAppConfigurator
    {
		public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IConfiguration configuration)
		{
            RSConfigParameters options = new RSConfigParameters();
            configuration.GetSection("RSConfigParameters").Bind(options);
            RSSocketHandler.ConfigParameters = options;

            app.Use(RSSocketHandler.Listen);
            
        }

        public void ConfigureServices(IConfiguration configuration, IServiceCollection services)
        {
          //  services.Configure<RSConfigParameters>(options => configuration.GetSection("RSConfigParameters").Bind(options));
        }

        public void MapRoutes(IRouteBuilder routes)
		{
		}
    }
}
