using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Microarea.Common;
using Microarea.RSWeb.Models;
using System;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Microarea.RSWeb
{
    public class WebAppConfigurator: IWebAppConfigurator
    {
		public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
		{
            app.Use(RSSocketHandler.Listen);
		}
		public void MapRoutes(IRouteBuilder routes)
		{
		}
    }
}
