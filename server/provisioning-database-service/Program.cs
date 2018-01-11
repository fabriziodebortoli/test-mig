using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace Microarea.ProvisioningDatabase
{
	public class Program
	{
		public static void Main(string[] args)
		{
			BuildWebHost(args).Run();
		}

		public static IWebHost BuildWebHost(string[] args)
		{
			//WebHost.CreateDefaultBuilder(args)
			//    .UseStartup<Startup>()
			//    .Build();

			var host = new WebHostBuilder()
				   .UseKestrel()
				   .UseContentRoot(Directory.GetCurrentDirectory())
				   .UseIISIntegration()
				   .UseStartup<Startup>()
				   .Build();
			return host;
		}
	}
}
