using System;
using System.Diagnostics;
using System.Reflection;
using System.Security.Principal;
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
            if (!IsRunAsAdministrator())
            {
                // It is not possible to launch a ClickOnce app as administrator directly, so instead we launch the
                // app as administrator in a new process.
                var processInfo = new ProcessStartInfo(Assembly.GetExecutingAssembly().CodeBase);

                // The following properties run the new process as administrator
                processInfo.UseShellExecute = true;
                processInfo.Verb = "runas";

                // Start the new process
                try
                {
                    Process.Start(processInfo);
                }
                catch (Exception)
                {
                    // The user did not allow the application to run as administrator
                    Console.WriteLine("Sorry, this application must be run as Administrator.");
                }

                // Shut down the current process
                return;
            }

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

        //------------------------------------------------------------------------------------------------
        private static bool IsRunAsAdministrator()
        {
            var wi = WindowsIdentity.GetCurrent();
            var wp = new WindowsPrincipal(wi);

            return wp.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}
