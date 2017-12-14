using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Core.tbLoaderLauncher;

namespace Microarea.TaskBuilderNet.Core.WebServicesWrapper
{
	public class TBLoaderLauncherWrapper
	{
		#region variabili private
		private TBLoaderLauncher tbLoaderLauncher = new TBLoaderLauncher();
		#endregion

		#region costruttori
		//---------------------------------------------------------------------------
		public TBLoaderLauncherWrapper()
			: this(BasePathFinder.BasePathFinderInstance.TbLoaderLauncherUrl, InstallationData.ServerConnectionInfo.WebServicesTimeOut)
		{

		}
		//---------------------------------------------------------------------------
		public TBLoaderLauncherWrapper(string TbLoaderLauncherUrl, int timeout)
		{
			tbLoaderLauncher.Url = TbLoaderLauncherUrl;
			tbLoaderLauncher.Timeout = timeout;
		}
		#endregion

		#region metodi per l'istanziazione dei tb
		//---------------------------------------------------------------------------
		public int ExecuteUsingPsExec(string psexecPath, string arguments, string remoteComputerName, string user, string password)
		{
			return tbLoaderLauncher.ExecuteUsingPsExec(psexecPath, arguments, remoteComputerName, user, password);
		}
		#endregion
	}
}
