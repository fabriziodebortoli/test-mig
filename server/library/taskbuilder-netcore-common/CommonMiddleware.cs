using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Microarea.Common
{
    public class CommonMiddleware
    {
        public const string culture_cookie = "ui_culture";
        private readonly RequestDelegate next;

        public CommonMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public Task Invoke(HttpContext context)
        {
            string c;

            if (context.Request.Cookies.TryGetValue(culture_cookie, out c))
            {
                try
                { 
                    var culture = new CultureInfo(c);
                    CultureInfo.CurrentUICulture = culture;
                }
                catch
                {
                    //in caso di cookie errato... non dovrebbe mai passare di qui...
                }
            }

            return this.next(context);
        }

    }

    public class WebAppConfigurator : IWebAppConfigurator
    {
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IConfiguration configuration)
        {
            app.UseMiddleware<CommonMiddleware>();
        }

        public void MapRoutes(IRouteBuilder routes)
        {
        }
    }
}
