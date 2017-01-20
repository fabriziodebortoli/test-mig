
using Microarea.RSWeb.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Microarea.Common;

namespace Microarea.RSWeb
{
    public class WebAppConfigurator: IWebAppConfigurator
    {
		public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
		{
			SocketHandler handler = new SocketHandler();
			app.Use(handler.Listen);
		}
		public void MapRoutes(IRouteBuilder routes)
		{
		}

	}
}
