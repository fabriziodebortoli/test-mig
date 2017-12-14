
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Core.WebServicesWrapper;

namespace Microarea.EasyBuilder.BackendCommunication
{
	/// <summary>
	/// ActivatorWrapper.
	/// </summary>
	//=========================================================================
	internal class ActivatorWrapper : BaseWsWrapper
	{
		//---------------------------------------------------------------------
		public ActivatorWrapper
			(
			ProxySettings	proxySettings
			) : base(proxySettings)
		{
		}

		//---------------------------------------------------------------------
		public string Register()
		{
			LoginManager loginManager = new LoginManager(BasePathFinder.BasePathFinderInstance.LoginManagerUrl, 1000000);

			return loginManager.Ping();
		}
	}
}
