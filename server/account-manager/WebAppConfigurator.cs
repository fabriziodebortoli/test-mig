using Microarea.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microarea.AccountManager;

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
            ProvisioningParameters provisioningOptions = new ProvisioningParameters();
            configuration.GetSection("ProvisioningParameters").Bind(provisioningOptions);
        }

		//---------------------------------------------------------------------
		public void ConfigureServices(IConfiguration configuration, IServiceCollection services)
        {
            services.Configure<ProvisioningParameters>(provisioningOptions => configuration.GetSection("ProvisioningParameters").Bind(provisioningOptions));
		}

		//---------------------------------------------------------------------
		public void MapRoutes(IRouteBuilder routes)
		{
		}
    }
}
