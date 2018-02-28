using Microarea.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskBuilderNetCore.EasyStudio.Interfaces;
using TaskBuilderNetCore.EasyStudio;

namespace Microarea.EasyStudio.AspNetCore
{
    public class EasyStudioWebAppConfigurator : IWebAppConfigurator
    {
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IConfiguration configuration)
        {

        }

        public void ConfigureServices(IConfiguration configuration, IServiceCollection services)
        {
            services.AddMvc();
            services.AddSingleton<IServiceManager, ServicesManager>();
        }

        public void MapRoutes(IRouteBuilder routes)
        {

        }
    }
}
