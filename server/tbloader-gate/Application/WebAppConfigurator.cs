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

namespace Microarea.TbLoaderGate.Application
{
    public class WebAppConfigurator : IWebAppConfigurator
	{
		public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IConfiguration configuration)
		{
            TBLoaderConnectionParameters options = new TBLoaderConnectionParameters();
            configuration.GetSection("TBLoaderConnectionParameters").Bind(options);
            SocketDispatcher dispatcher = new SocketDispatcher(env, options);
            app.Use(dispatcher.Listen);
        }


        public void ConfigureServices(IConfiguration configuration, IServiceCollection services)
        {
            services.Configure<TBLoaderConnectionParameters>(options => configuration.GetSection("TBLoaderConnectionParameters").Bind(options));
        }

        public void MapRoutes(IRouteBuilder routes)
		{
		}
    }
}
