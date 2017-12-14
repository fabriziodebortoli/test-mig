using System;
using System.Globalization;
using System.ServiceProcess;

namespace Microarea.Library.SystemServices
{
	/// <summary>
	/// Summary description for ServiceManager.
	/// </summary>
	public class ServiceManager
	{
		private ServiceManager(){} // no instance wanted
	
		//---------------------------------------------------------------------
		public static ServiceController GetServiceController(string machineName, string serviceName)
		{
			ServiceController[] scs = ServiceController.GetServices(machineName);
			foreach (ServiceController sc in scs)
				if (string.Compare(sc.ServiceName, serviceName, true, CultureInfo.InvariantCulture) == 0)
					return sc;
			return null;
		}
	}
}
