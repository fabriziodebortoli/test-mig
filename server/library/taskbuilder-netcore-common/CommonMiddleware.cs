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

        public async Task Invoke(HttpContext context)
        {
            this.BeginInvoke(context);
            await this.next.Invoke(context);
            this.EndInvoke(context);
        }

        private void BeginInvoke(HttpContext context)
        {
            string c;
            if (context.Request.Cookies.TryGetValue(culture_cookie, out c))
            {
                Thread.CurrentThread.CurrentUICulture = CultureInfo.CreateSpecificCulture(c);
            }

        }

        private void EndInvoke(HttpContext context)
        {
            // Do custom work after controller execution
        }
    }

}
