using Microarea.Common;
using Microarea.TbfWebGate.Application;
using Microarea.TbfWebGate.Authorization;
using Microsoft.AspNetCore.Authorization;
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

namespace Microarea.TbfWebGate.AspNetCore
{
    public class TbfWebGateWebAppConfigurator : IWebAppConfigurator
    {
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IConfiguration configuration)
        {

        }

        public void ConfigureServices(IConfiguration configuration, IServiceCollection services)
        {
            services.AddMvc();
            //services.AddApiVersioning(o =>
            //{
            //    o.ReportApiVersions = true;
            //    o.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
            //    o.AssumeDefaultVersionWhenUnspecified = true;
            //});
            
            //DI
            //services.AddTransient<IOrchestratorService, OrchestratorService>();
            services.AddSingleton<IOrchestratorService, OrchestratorService>();

            //Authorization
            services.AddAuthorization(options =>
            {
                options.AddPolicy(
                    "LoggedIn", policy => policy.Requirements.Add(new TbfAuthorizationRequirement())
                    );
            });
            services.AddSingleton<IAuthorizationHandler, TbfAuthorizationHandler>();
        }

        public void MapRoutes(IRouteBuilder routes)
        {

        }
    }
}
