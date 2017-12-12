using System;
using System.Windows.Forms;

namespace Microarea.TaskBuilderNet.Interfaces
{
	public interface ITBApplicationProxy
	{
		int SoapPort { get; }
		int TcpPort { get; }
		bool Valid { get; }
		bool Logged { get; }
		bool CanShowLockStructure { get; }
		string[] DocumentHistory { get; }
        
		bool HasExited();

		void CloseLogin();

		void FireAction(int documentHandle, string action);

		int RunDocument(string command, string arguments);

		bool RunFunction(string command, string arguments);

		int RunReport(string command, string arguments);

		bool RunTextEditor(string command);

		IDiagnostic GetLoginContextDiagnostic(bool clear);

		IDiagnostic GetApplicationContextDiagnostic(bool clear);

		IDiagnostic GetGlobalDiagnostic(bool clear);

		void SetApplicationDate(DateTime date);
		void ChangeOperationsDate ();
        
        bool OpenEnumsViewer(string culture, string installation);
        
		void ShowAboutFramework();

		void WaitForExit();

		bool CanChangeLogin(bool lockTbLoader);

		int GetOpenDocuments();

		int GetOpenDocumentsInDesignMode();

		void ClearCache();

		DateTime GetApplicationDate();

		void Destroy();

		void SetMenuWindowHandle(IntPtr menuHandle);

		void SetDocked(bool isDocked);

		void ShowLockStructure();

		bool Login(string AuthenticationToken);

		bool CanCloseLogin();

		bool CanCloseTB();

		int ChangeLogin(string oldAuthenticationToken, string newAuthenticationToken, bool unlock);
		MenuStrip CreateMenuStrip(IntPtr Handle, IntPtr MenuHandle);

		bool CloseAllDocuments();

		bool RunBatchInUnattendedMode(string documentNamespace, string xmlParams, ref int documentHandle, ref string[] messages);

		bool RunReportInUnattendedMode(int woormInfo, string xmlParams, ref int reportHandle, ref string[] messages);

        bool SilentCloseLoginDocuments();

		void RunFunctionInNewThread (string command, string arguments);

		void InitLockManager();

		bool IsEasyBuilderDeveloper { get; }

		void SetUnattendedMode(bool unattendedMode);

        bool OnBeforeCanCloseTB();
	}
}
