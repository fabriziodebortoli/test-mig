using Microarea.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace Microarea.TbLoaderGate.Application
{
    public class WebAppConfigurator : IWebAppConfigurator
	{
		public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
		{
			app.Use(SocketDispatcher.Listen);
		}

		public void MapRoutes(IRouteBuilder routes)
		{
		}

	}
}
