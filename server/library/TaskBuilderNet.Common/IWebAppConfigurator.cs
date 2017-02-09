using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Routing;

namespace Microarea.Common
{
    public interface IWebAppConfigurator
    {
		void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory);
		void MapRoutes(IRouteBuilder routes);
	}
}
