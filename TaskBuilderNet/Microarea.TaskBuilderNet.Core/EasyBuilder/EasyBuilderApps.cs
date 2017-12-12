using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.TaskBuilderNet.Core.EasyBuilder
{
	//=========================================================================
	public struct EasyBuilderAppDetails
	{
		public string Application;
		public string Module;
		public ApplicationType ApplicationType;

		public EasyBuilderAppDetails(string application, string module, ApplicationType applicationType)
		{
			this.Application = application;
			this.Module = module;
			this.ApplicationType = applicationType;
		}
	}
}
