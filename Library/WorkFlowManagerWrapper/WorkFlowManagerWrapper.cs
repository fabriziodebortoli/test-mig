using System;

namespace Microarea.Library.WorkFlowManagerWrapper
{
	/// <summary>
	/// 
	/// </summary>
	//---------------------------------------------------------------------------
	public class WorkFlowManager
	{
		private localhost.WorkFlowManager workFlowManager = new Microarea.Library.WorkFlowManagerWrapper.localhost.WorkFlowManager();

		//---------------------------------------------------------------------------
		public WorkFlowManager(string url)
		{
			workFlowManager.Url = url;
		}

		//---------------------------------------------------------------------------
		public int Init()
		{
			return workFlowManager.Init();
		}

		//----------------------------------------------------------------------
		public bool IsActivated(string application, string functionality)
		{
			return workFlowManager.IsActivated(application, functionality);
		}

		//----------------------------------------------------------------------
		public int GetActivationState()
		{
			return workFlowManager.GetActivationState();
		}

		//----------------------------------------------------------------------
		public bool Login()
		{
			return workFlowManager.Login();
		}

		//----------------------------------------------------------------------
		public void CheckActivationTimes()
		{
			workFlowManager.CheckActivationTimes();
		}

		//----------------------------------------------------------------------
		public void MetodoDaTogliere(int a, int b, bool c)
		{
			workFlowManager.MetodoDaTogliere(a, b, c);
		}
	}
}
