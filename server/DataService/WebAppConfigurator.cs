
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

using Microarea.Common;
using Microarea.DataService.Models;

namespace Microarea.RSWeb
{
    public class WebAppConfigurator: IWebAppConfigurator
    {
		public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
		{
			DSSocketHandler handler = new DSSocketHandler();
			app.Use(handler.Listen);
		}
		public void MapRoutes(IRouteBuilder routes)
		{
		}

	}
}
