using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Microarea.Common;
using Microarea.RSWeb.Models;
using System;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Microarea.RSWeb
{
    public class WebAppConfigurator: IWebAppConfigurator
    {
		public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IConfiguration configuration)
		{
            app.Use(RSSocketHandler.Listen);
		}

        public void ConfigureServices(IConfiguration configuration, IServiceCollection services)
        {
        }

        public void MapRoutes(IRouteBuilder routes)
		{
		}
    }
}
