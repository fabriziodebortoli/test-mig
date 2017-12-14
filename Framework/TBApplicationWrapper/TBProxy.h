#pragma once

using namespace Microarea::TaskBuilderNet::Interfaces;
using namespace Microarea::TaskBuilderNet::Core::DiagnosticManager;

class CTBApplicationWrapperApp;
class CDiagnostic;
class CLoginContext;


namespace Microarea { namespace Framework	{ namespace TBApplicationWrapper
{
	
	ref class TBCommandManagerProxy;
	//-----------------------------------------------------------------------------
	/// <summary>
	/// Internal Use
	/// </summary>
	public ref class TBApplicationProxy : ITBApplicationProxy
	{
		System::String^ authenticationToken;
		static CTBApplicationWrapperApp *m_pTBApplicationWrapperApp = NULL;
	//	IHttpDocumentHandler^	httpDocumentHandler;
		void Create(System::String^ tbApplicationPath, System::String^ arguments);
	
		Diagnostic^ GetDiagnostic(CDiagnostic* pDiagnostic, Diagnostic^ diagnostic);
		CLoginContext* GetLoginContext();
		static void OnApplicationExit(System::Object^ sender, System::EventArgs^ e);

	public:
		TBApplicationProxy(System::String^ tbApplicationPath, System::String^ arguments);
		~TBApplicationProxy(void);

		static ITBApplicationProxy^ AttachProxyToTbApplication();
		static void CloseTbApplication();

		property int SoapPort
		{
			virtual int get() ;
		}
		
		property int TcpPort
		{
			virtual int get() ;
		}

		property bool Valid
		{
			virtual bool get() ;
		}

		property bool Logged
		{
			virtual bool get() ;
		}
		property bool IsEasyBuilderDeveloper
		{
			virtual bool get() ;
		}
		property bool CanShowLockStructure
		{
			virtual bool get() ;
		}

		property cli::array<System::String^>^ DocumentHistory
		{
			virtual cli::array<System::String^>^ get() ;
		}
		virtual void Destroy(void);
		virtual bool HasExited(void);
		virtual void WaitForExit();
		
		virtual bool Login (System::String ^autheticationToken);
		virtual void CloseLogin ();
		virtual bool CloseAllDocuments ();
		virtual bool SilentCloseLoginDocuments();
		virtual void SetMenuWindowHandle(System::IntPtr hMenuWindow);
		virtual void SetDocked (System::Boolean isDocked);
		virtual void FireAction(int documentHandle, System::String^ action);
		virtual int RunDocument(System::String^ command, System::String^ arguments);
		virtual int RunReport(System::String^ command, System::String^ arguments);
		virtual bool RunFunction(System::String^ command, System::String^ arguments);
		virtual void RunFunctionInNewThread(System::String^ command, System::String^ arguments);
		virtual bool RunTextEditor(System::String^ command);
		virtual void ShowAboutFramework();
		virtual void ChangeOperationsDate();
		virtual void SetApplicationDate(System::DateTime date);
		virtual System::DateTime GetApplicationDate();
		virtual IDiagnostic^ GetGlobalDiagnostic(bool clear);
		virtual IDiagnostic^ GetApplicationContextDiagnostic(bool clear);
		virtual IDiagnostic^ GetLoginContextDiagnostic(bool clear);
		virtual void ClearCache();
		virtual bool OnBeforeCanCloseTB();
		virtual bool CanCloseTB();
		virtual bool CanCloseLogin();
		virtual bool CanChangeLogin(bool lockTB);
		virtual int GetOpenDocuments();
		virtual int GetOpenDocumentsInDesignMode();
		virtual int ChangeLogin(System::String^ oldAuthenticationToken, System::String^ newAuthenticationToken, bool unlock);
		virtual void ShowLockStructure();
		virtual void InitLockManager();
		virtual System::Windows::Forms::MenuStrip^ CreateMenuStrip(System::IntPtr Handle, System::IntPtr MenuHandle);
		virtual bool RunBatchInUnattendedMode(System::String^ documentNamespace, System::String^ xmlParams, int% documentHandle, cli::array<System::String^>^% messages);
		virtual bool RunReportInUnattendedMode(int woormInfo, System::String^ xmlParams, int% reportHandle, cli::array<System::String^>^% messages);
		virtual bool OpenEnumsViewer (System::String^ culture, System::String^ installation);
		virtual void SetUnattendedMode(bool set);
		static	bool CloseEnumsViewer();
	};
} } }
