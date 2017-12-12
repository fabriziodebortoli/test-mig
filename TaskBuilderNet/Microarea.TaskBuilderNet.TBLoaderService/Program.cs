using System;
using System.ServiceProcess;

namespace Microarea.TaskBuilderNet.TbLoaderService
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		static void Main()
		{
			if (Environment.UserInteractive)
			{
				TBLoader service1 = new TBLoader();
				service1.TestStartupAndStop(null);
			}
			else
			{
				ServiceBase[] ServicesToRun;
				ServicesToRun = new ServiceBase[]
				{
				new TBLoader()
				};
				ServiceBase.Run(ServicesToRun);
			}
		}
	}
}
