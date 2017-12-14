using System;

namespace Microarea.TaskBuilderNet.TbHermesBL
{
	public class ServiceStatus
	{
		public bool InternetConnectionAvailable { get; set; }
		public DateTime LastMailUpdate { get; set; }

		public ServiceStatus()
		{
			this.InternetConnectionAvailable = true;
		}
	}
}
