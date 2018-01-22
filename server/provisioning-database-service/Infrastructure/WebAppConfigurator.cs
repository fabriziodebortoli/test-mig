using Microarea.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microarea.ProvisioningDatabase.Controllers.Helpers;

namespace Microarea.ProvisioningDatabase.Infrastructure
{
	/// <summary>
	/// Every service needs this class to load itself in web-server
	/// </summary>
	//============================================================================
    public class WebAppConfigurator : IWebAppConfigurator
	{
		//---------------------------------------------------------------------
		public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IConfiguration configuration)
		{
        }

		//---------------------------------------------------------------------
		public void ConfigureServices(IConfiguration configuration, IServiceCollection services)
        {
			services.AddTransient<IJsonHelper, JsonHelper>();
			services.AddTransient<IHttpHelper, HttpHelper>();
		}

		//---------------------------------------------------------------------
		public void MapRoutes(IRouteBuilder routes)
		{
		}
    }
}
