using System;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.EasyAttachment.BusinessLogic
{
	//================================================================================
	public class BaseManager
	{
		protected string ManagerName = "BaseManager";

		//properties
		//--------------------------------------------------------------------------------
		public DMSOrchestrator DMSOrchestrator { get; set; }

		public event EventHandler<Microarea.EasyAttachment.Components.MessageEventArgs> ManagerErrorOccurred;

		//---------------------------------------------------------------------
		public BaseManager()
		{
			DMSOrchestrator = null;
		}

		///<summary>
		/// Creo un MessageEventArgs e poi lo passo con evento all'orchestrator
		///</summary>
		//--------------------------------------------------------------------------------
		public void SetMessage(string explain, Exception ex, string function, DiagnosticType dType = DiagnosticType.Error)
		{
			Microarea.EasyAttachment.Components.MessageEventArgs arg = new Microarea.EasyAttachment.Components.MessageEventArgs();
			arg.Explain = explain;
			arg.Function = function;
			arg.Library = ManagerName;
			arg.MessageType = dType;

			if (ex != null)
			{
				arg.Message = ex.Message;
				arg.Source = ex.Source;
				arg.StackTrace = ex.StackTrace;
			}

			if (ManagerErrorOccurred != null)
				ManagerErrorOccurred(this, arg);
		}

		///<summary>
		/// Passa il diagnostico all'orchestrator, che si occupa di mostrarlo
		///</summary>
		//--------------------------------------------------------------------------------
		public void SetDiagnostic(Diagnostic diagnostic)
		{
			if (ManagerErrorOccurred != null)
				ManagerErrorOccurred(diagnostic, null);
		}
	}
}
