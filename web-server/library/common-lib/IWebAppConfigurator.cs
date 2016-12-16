using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
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
