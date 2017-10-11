
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

using Microarea.Common;
using Microarea.DataService.Models;
using System;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Microarea.DataService
{
    public class WebAppConfigurator: IWebAppConfigurator
    {
		public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IConfiguration configuration)
		{
            loggerFactory.AddDebug();

            app.UseStaticFiles();
            app.UseMvc();

        }
        public void MapRoutes(IRouteBuilder routes)
		{
		}
    }
}
