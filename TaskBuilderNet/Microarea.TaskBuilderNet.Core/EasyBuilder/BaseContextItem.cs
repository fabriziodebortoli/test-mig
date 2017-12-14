using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microarea.TaskBuilderNet.Core.EasyBuilder
{
	//=========================================================================
	public class BaseContextItem
	{
		private string moduleName;
		private string applicationName;

		public virtual ApplicationType ApplicationType
		{
			get { return ApplicationType.Customization; }
		}


		public IBaseModuleInfo ModuleInfo
		{
			get
			{
				return BasePathFinder.BasePathFinderInstance.GetModuleInfoByName(ApplicationName, ModuleName); 
			}

		}
		public string ModuleName
		{
			get
			{
				return moduleName;
			}

			set
			{
				moduleName = value;
			}
		}

		public string ApplicationName
		{
			get
			{
				return applicationName;
			}

			set
			{
				applicationName = value;
			}
		}

		public BaseContextItem(string application, string module)
		{
			this.applicationName = application;
			this.moduleName = module;
		}
	}
}
