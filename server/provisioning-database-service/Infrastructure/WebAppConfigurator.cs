using Microarea.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microarea.ProvisioningDatabase.Controllers.Helpers;

namespace Microarea.ProvisioningDatabase.Infrastructure
{
    public class WebAppConfigurator : IWebAppConfigurator
	{
		public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IConfiguration configuration)
		{
        }

        public void ConfigureServices(IConfiguration configuration, IServiceCollection services)
        {
			services.AddTransient<IJsonHelper, JsonHelper>();
			services.AddTransient<IHttpHelper, HttpHelper>();
		}

        public void MapRoutes(IRouteBuilder routes)
		{
		}
    }
}
