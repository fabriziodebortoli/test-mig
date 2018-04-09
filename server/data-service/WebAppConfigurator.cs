using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Microarea.Common;
using Microsoft.Extensions.DependencyInjection;

using Microsoft.Extensions.Configuration;

namespace Microarea.RSWeb
{
    public class WebAppConfigurator: IWebAppConfigurator
    {
		public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IConfiguration configuration)
		{
           
        }

        public void ConfigureServices(IConfiguration configuration, IServiceCollection services)
        {
           services.Configure<TbLoaderGateConfigParameters>(options => configuration.GetSection("RSConfigParameters").Bind(options));
        }

        public void MapRoutes(IRouteBuilder routes)
		{
		}
    }
}
