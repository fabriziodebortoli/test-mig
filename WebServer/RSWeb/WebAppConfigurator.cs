using Microarea.Common;
using Microarea.RSWeb.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microarea.RSWeb
{
	namespace Microarea.TbLoaderGate.Application
	{
		public class WebAppConfigurator : IWebAppConfigurator
		{
			public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
			{
				SocketHandler handler = new SocketHandler();
				app.Use(handler.Listen);
			}

		}
	}
}
