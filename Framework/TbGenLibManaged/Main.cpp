
#include "stdafx.h" 
#include <TbNameSolver\Diagnostic.h>
#include <TbNameSolver\LoginContext.h>
#include <TbNameSolver\ThreadContext.h>
#include <TbGeneric\FunctionObjectsInfo.h>
#include <TbGeneric\GeneralFunctions.h>

#include "Main.h"
#include "wcfservicegenerator.h"
#include "WCFHelper.h"
#include "StaticFunctions.h"

using namespace System;
using namespace System::CodeDom;
using namespace System::CodeDom::Compiler;
using namespace System::IO;
using namespace System::Reflection;
using namespace System::Threading;
using namespace System::Globalization;
using namespace System::Drawing;
using namespace System::Drawing::Imaging;
using namespace System::Runtime::InteropServices;
using namespace Microarea::TaskBuilderNet::Core::EasyBuilder;
using namespace Microarea::TaskBuilderNet::Core::Generic;
using namespace Microarea::TaskBuilderNet::Core::SoapCall;
using namespace Microarea::TaskBuilderNet::Core::NameSolver;
using namespace Microarea::TaskBuilderNet::Woorm::WoormController;
using namespace Microarea::TaskBuilderNet::Core::WebSockets;


ref class CGlobalClass
{
public:
	static ApplicationLockToken^ LockToken = nullptr;
};

//---------------------------------------------------------------------------------------
Assembly^ OnAssemblyResolve(Object^ sender, ResolveEventArgs^ args)
{
	Monitor::Enter(CWCFHelper::typeid);
	try
	{
		//controllo se ho gia' fallito in precedenza
		if (CWCFHelper::IsNonResolvableAssembly(args->Name))
			return nullptr;

		String^ sPath = Path::GetDirectoryName(Assembly::GetExecutingAssembly()->Location);
		AssemblyName ^an = gcnew AssemblyName(args->Name);
		String^ asmName = an->Name + ".dll";

		try
		{
			String ^file = Path::Combine(sPath, asmName);

			//se non lo trovo, lo cerco prima nella cartella degli assembly esterni custom di easy builder
			if (!File::Exists(file))
			{
				file = Path::Combine(gcnew String(AfxGetPathFinder()->GetEasyStudioReferencedAssembliesPath()), asmName);
				if (::ExistFile(CString(file)))
				{
					//Se esiste, lo carico da li nella stessa maniera in cui carico le dll delle
					//customizzazioni per non lock-are i file.
					//Inoltre evito il problema di cui a http://connect.microsoft.com/VisualStudio/feedback/details/545190/assemblyname-getassemblyname-followed-by-assembly-load-do-not-work-with-an-unc-name
					//in cui si incappava con Assembly::Load(an) di cui sotto
					Assembly^ a = AssembliesLoader::Load(file);
					if (a != nullptr)
						return a;
				}
			}
			else
			{
				AssemblyName^ an = AssemblyName::GetAssemblyName(file);
				Assembly^ a = Assembly::Load(an);
				if (a != nullptr)
					return a;
			}
		}
		catch (Exception ^ex)
		{
			AfxGetDiagnostic()->Add(ex->ToString());
			throw ex;
		}

		//ricordo che ho fallito per non fare piu accessi a file system in futuro
		CWCFHelper::AddNonResolvableAssembly(args->Name);
		return nullptr;
	}
	finally
	{
		Monitor::Exit(CWCFHelper::typeid);
	}
}

//---------------------------------------------------------------------------------------
void InitAssembly()
{
	AppDomain::CurrentDomain->AssemblyResolve += gcnew ResolveEventHandler(&OnAssemblyResolve);
	TBWCFService::GetThreadHwndFunctionPointer = gcnew TBWCFService::GetThreadHwndFunction(&CWCFHelper::GetThreadHwndFunction); 
	TBWCFService::LoadTBDllFunctionPointer = gcnew TBWCFService::LoadTBDllFunction(&CWCFHelper::LoadTBDllFunction); 
}

//---------------------------------------------------------------------------------------
void InitTimer()
{
	TBWCFService::StartTimer();
}

//---------------------------------------------------------------------------------------
void FreeAssemblyObjects()
{
	AppDomain::CurrentDomain->AssemblyResolve -= gcnew ResolveEventHandler(&OnAssemblyResolve);
	TBWCFService::GetThreadHwndFunctionPointer = nullptr;
	TBWCFService::LoadTBDllFunctionPointer = nullptr; 
}

//-----------------------------------------------------------------------------
void GenerateEasyBuilderEnumsDllAsync()
{
	try
	{
	System::Threading::ThreadStart^ threadStart = gcnew System::Threading::ThreadStart(
		StaticFunctions::GenerateEasyBuilderEnumsDllIfNecessary
			);
		System::Threading::Thread^ asyncThread = gcnew System::Threading::Thread(
			threadStart
			);
		asyncThread->Start();
	}
	catch (Exception^ exc)
	{
		AfxGetDiagnostic()->Add(exc->ToString(), CDiagnostic::Error);
	}
}

//---------------------------------------------------------------------------------------
void StartWCFServices(Assembly^ asmb, int soapPort, int tcpPort)
{
	if (soapPort == 0 && tcpPort == 0)
		return;

	TBWCFService::SoapPort = soapPort;
	TBWCFService::TcpPort = tcpPort;
	try
	{
		for each (Type^ t in asmb->GetTypes())
		{
			if (t->BaseType != TBWCFService::typeid)
				continue;
			try
			{
				TBWCFService^ service = (TBWCFService^) asmb->CreateInstance(t->FullName);

				ServiceCache::AddService(service);
			}
			catch (Exception^ ex)
			{
				AfxGetDiagnostic()->Add(ex->ToString(), CDiagnostic::Warning);
			}
		}

		ServiceCache::StartRestService();
	}
	catch (Exception^ ex)
	{
		AfxGetDiagnostic()->Add(ex->ToString(), CDiagnostic::Warning);
	}
}

//---------------------------------------------------------------------------------------
void CreateWCFServices(const CBaseDescriptionArray &functions, int soapPort, int tcpPort, const CString& strUserForRegisteringNamespaces, int startPortForRegisteringNamespaces, IHostApplication* pHost)
{
	CWCFHelper::SetHost(pHost, soapPort, tcpPort);

	CServiceGenerator^ generator = gcnew CServiceGenerator();
	generator->AppendFunctions(functions);

	Assembly^ asmb = generator->Compile();
	if (asmb == nullptr)
	{
		AfxGetDiagnostic()->Add(_TB("Cannot compile WCF Services assembly, WCF services will not be available"), CDiagnostic::Warning);
		return;	
	}

	if (!strUserForRegisteringNamespaces.IsEmpty())
		generator->RegisterNamespaces(strUserForRegisteringNamespaces, startPortForRegisteringNamespaces);

	StartWCFServices(asmb, soapPort, tcpPort);

}

//---------------------------------------------------------------------------------------
void StartWCFServices(int soapPort, int tcpPort, IHostApplication* pHost)
{
	CWCFHelper::SetHost(pHost, soapPort, tcpPort);

	Assembly^ asmb = ServiceClientCache::GetServicesAssembly();
	if (asmb == nullptr)
	{
		AfxGetDiagnostic()->Add(_TB("Cannot load WCF Services assembly, WCF services will not be available"), CDiagnostic::Warning);
		return;	
	}

	StartWCFServices(asmb, soapPort, tcpPort);

}

//---------------------------------------------------------------------------------------
void StopWCFServices()
{
	ServiceCache::StopRestService();
	ServiceCache::Clear();
}

//---------------------------------------------------------------------------------------
bool EnableSoapExecutionControl(bool bEnable)
{
	return CWCFHelper::EnableSoapExecutionControl(bEnable);
}

//---------------------------------------------------------------------------------------
void DeleteWCFServicesAssembly()
{
	try
	{
		String^ path = ServiceClientCache::GetServicesAssemblyPath();

		if (!File::Exists(path))
			return;

		File::Delete(path);
	} 
	catch (Exception^ ex)
	{
		AfxGetDiagnostic()->Add(ex->ToString(), CDiagnostic::Warning);
	}
}

//---------------------------------------------------------------------------------------
bool ApplicationLock()
{

	String^ path = gcnew String(AfxGetPathFinder()->GetSemaphoreFilePath());

	try
	{
		CGlobalClass::LockToken = ApplicationSemaphore::Lock(path);
	}
	catch (Exception ^ ex)
	{
		AfxGetDiagnostic()->Add(ex->Message);
		return false;
	}
	return true;
}


//---------------------------------------------------------------------------------------
bool WCFServicesCreationNeeded()
{
	String^ path = ServiceClientCache::GetServicesAssemblyPath();

	FileInfo^ asmFileInfo = gcnew FileInfo(path);

	if (!asmFileInfo->Exists)
		return true;

	return asmFileInfo->LastWriteTime < InstallationData::CacheDate;
}

//---------------------------------------------------------------------------------------
void ThrowParamError(String^ name)
{
	throw gcnew ApplicationException(gcnew String(cwsprintf(_TB("Parameter not defined in local function prototype: {0-%s}"), CString(name))));
}

//---------------------------------------------------------------------------------------
void SetWebServicesTimeout(int nTimeout)
{
	CWCFHelper::SetWebServicesTimeout(nTimeout);
}

//---------------------------------------------------------------------------------------
///Funzione che chiama una funzione WEB esterna a TB (Crea una proxy C# usando l'infrastruttura WCF, quindi fa la chiamata via reflection)
bool InvokeWCFExternalFunction(CFunctionDescription* pFunctionDescription, bool forceAssemblyCreation)
{
	DynamicProxy^ proxy = nullptr;
	try 
	{ 
		//costruisco la classe proxy a partire dal wsdl
		proxy = ServiceClientCache::GetClientProxy(gcnew String(pFunctionDescription->GetUrl()) + "?wsdl", CWCFHelper::GetWebServicesTimeout(), forceAssemblyCreation);

		//recupero il metodo da invocare
		MethodInfo^ mi =  proxy->GetMethod(gcnew String(pFunctionDescription->GetName()));
		if (mi == nullptr)
		{
			pFunctionDescription->m_strError += cwsprintf(_TB("Cannot find method '{0-%s}' in WCF class '{1-%s}'\r\n"), pFunctionDescription->GetName(), CString(proxy->ProxyType->ToString()));
			return false;
		}
		//confronto il nomero di parametri ricevuti con quelli attesi
		int paramCount = pFunctionDescription->GetParameters().GetCount();
		cli::array<ParameterInfo^>^ methodParams = mi->GetParameters();
		if (methodParams->Length != paramCount)
		{
			pFunctionDescription->m_strError += cwsprintf(_TB("Parameter number mismatch: expected {0-%d} parameters, received {1-%d} parameters\r\n"), methodParams->Length, paramCount);
			return false; 
		}

		cli::array<Object^>^ params = gcnew cli::array<Object^>(paramCount);
		//travaso i parametri della function description nell'array di parametri da passare nell'invocazione dinamica
		for (int i = 0; i < paramCount; i++)
		{
			ParameterInfo ^param = methodParams[i];
			DataObj *pValue = pFunctionDescription->GetParamValue(param->Name);
			//parametro non definito nel prototipo del client ma presente in quello del server
			if (!pValue)
				ThrowParamError(param->Name);
			params[i] = CWCFHelper::DataObjToObject(pValue, param->ParameterType);
		}

		//effettuo la chiamata via reflection
		Object^ ret = proxy->CallMethod(mi, params);

		/*per tutti i parametri che sono out o in-out, travaso il valore indietro nella function description*/

		for (int i = 0; i < paramCount; i++)
		{
			ParameterInfo ^param = methodParams[i];

			CDataObjDescription* par = pFunctionDescription->GetParamDescription(param->Name);

			//parametro non definito nel prototipo del client ma presente in quello del server
			if (!par)
				ThrowParamError(param->Name);

			if (par->IsPassedModeIn())
				continue;

			CWCFHelper::AssignObjectToDataObj(params[i], par->GetValue());
		}

		//assegno il valore di ritorno
		CWCFHelper::AssignObjectToDataObj(ret, pFunctionDescription->GetReturnValue());

	}
	catch (Exception^ ex)
	{
		Exception^ currEx = ex;
		while (currEx != nullptr)
		{
			pFunctionDescription->m_strError += CString(currEx->Message) + _T("\r\n");
			currEx = currEx->InnerException;
		}
		return false;
	}
	finally
	{
	}
	return true;
}

//---------------------------------------------------------------------------------------
///Funzione che chiama via reflection una funzione esterna (inutile fare la chiamata tramite WCF visto che siamo
///nello stesso processo...)
bool InvokeWCFInternalFunction(CFunctionDescription* pFunctionDescription)
{
#ifndef	DEBUG 
	try 
#endif
	{
		//recupero l'assembly dove si trova la classe WCF per invocare la funzione
		Assembly^ asmb = ServiceClientCache::GetServicesAssembly();
		if (asmb == nullptr)
		{
			pFunctionDescription->m_strError += _TB("Cannot load WCF service assembly\r\n");
			return false;
		}	

		//costruisco dinamicamente il tipo della classe che mi permette di fare la chiamata
		String^ typeName = String::Concat(ServiceClientCache::AssemblyNamespace, ".",  CServiceGenerator::GetServiceName(pFunctionDescription));
		Type ^ classType = asmb->GetType(typeName);
		if (classType == nullptr)
		{
			pFunctionDescription->m_strError += cwsprintf(_TB("Cannot find class '{0-%s}' in WCF service assembly\r\n"), CString(typeName));
			return false;
		}

		//costruisco dimanicamente il nome del metodo da chiamare
		String^ name = "____" + CServiceGenerator::GetFunctionName(pFunctionDescription);
		String^ nameSpace = CServiceGenerator::GetFunctionNamespace(pFunctionDescription);

		//recupero il metodo by reflection
		MethodInfo^ mi = classType->GetMethod(name);
		if (mi == nullptr)
		{
			pFunctionDescription->m_strError += cwsprintf(_TB("Cannot find method '{0-%s}' in WCF class '{1-%s}'\r\n"), CString(name), CString(typeName));
			return false;
		}

		//confronto il numero dei parametri del metodo da chiamare con quelli passati nella function description
		int paramCount = pFunctionDescription->GetParameters().GetCount();
		cli::array<ParameterInfo^>^ methodParams = mi->GetParameters();
		if (methodParams->Length != paramCount + 1) //i parametri passati hanno in piu` l'handle della finestra per trovare il thread giusto
		{
			pFunctionDescription->m_strError += cwsprintf(_TB("Parameter number mismatch: expected {0-%d} parameters, received {1-%d} parameters\r\n"), methodParams->Length - 1, paramCount);
			return false; 
		}

		CLoginContext* pContext = AfxGetLoginContext();

		//il primo parametro e' l'handle della finestra che mi permette di individuare il thread giusto su cui fare la chiamata
		cli::array<Object^>^ params = gcnew cli::array<Object^>(paramCount + 1);
		params[0] = CWCFHelper::GetThreadHwndFunction(
			pContext ? gcnew String(pContext->GetName()) : nullptr,
			pFunctionDescription->GetContextHandle(),
			nameSpace,
			false
			);

		//travaso i parametri della function description nell'array di parametri da passare nell'invocazione dinamica
		for (int i = 0; i < paramCount; i++)
		{
			short idx = i + 1; //il primo argomento e' l'handle della finestra
			ParameterInfo ^param = methodParams[idx]; 
			params[idx] = CWCFHelper::DataObjToObject(pFunctionDescription->GetParamValue(i), param->ParameterType);
		}

		/*effettuo la chiamata via reflection*/

		Object^ ret = mi->Invoke(nullptr, params);

		/*per tutti i parametri che sono out o in-out, travaso il valore indietro nella function description*/

		for (int i = 0; i < paramCount; i++)
		{
			if (pFunctionDescription->GetParamDescription(i)->IsPassedModeIn())
				continue;

			CWCFHelper::AssignObjectToDataObj(params[i+1], pFunctionDescription->GetParamValue(i));
		}

		//assegno il valore di ritorno
		CWCFHelper::AssignObjectToDataObj(ret, pFunctionDescription->GetReturnValue());
	}
#ifndef	DEBUG //in debug lascio schiantare il programma dove si verifica l'errore per facilitarne l'individuazione
	catch (Exception^ ex)
	{
		pFunctionDescription->m_strError += CString(ex->ToString()) + _T("\r\n");
		return false;
	}
#endif
	return true;
}


//--------------------------------------------------------------------------------
bool InvokeWCFFunction(CFunctionDescription* pFunctionDescription, BOOL bInternalCall)
{

	if (!(bInternalCall 
		? InvokeWCFInternalFunction(pFunctionDescription) 

		: (InvokeWCFExternalFunction(pFunctionDescription, false) 
#ifdef _DEBUG		
		//per le funzioni esterne: l'assembly proxy potrebbe non essere più valido, 
		//per cui se fallisce una chiamata con l'assembly corrente provo una seconda chiamata rigenerandolo
		|| InvokeWCFExternalFunction(pFunctionDescription, true)
#endif
		)))
	{
		//	AfxGetDiagnostic()->Add(cwsprintf(_TB("Error invoking function {0-%s}"), pFunctionDescription->GetName()));
		//	if (!pFunctionDescription->m_strError.IsEmpty())
		//		AfxGetDiagnostic()->Add(pFunctionDescription->m_strError);
		return false;
	}
	return true;
}

//--------------------------------------------------------------------------------
void GetServerInstallationInfo(CString& strFileServer, CString& strWebServer, CString &strInstallation, CString &strMasterSolutionName)
{
	strFileServer = InstallationData::FileSystemServerName;
	strWebServer = InstallationData::WebServerName;
	strInstallation = InstallationData::InstallationName;
	strMasterSolutionName = InstallationData::ServerConnectionInfo->MasterSolutionName;
}

//----------------------------------------------------------------------------
int GetTbLoaderSOAPPort()
{
	return CWCFHelper::GetSOAPPort();
}
//----------------------------------------------------------------------------
int GetTbLoaderTCPPort()
{
	return CWCFHelper::GetTCPPort();
}

//----------------------------------------------------------------------------
void SetAdminAuthenticationToken(const CString& strToken)
{
	CWCFHelper::SetAdminToken(gcnew String(strToken));
}

//----------------------------------------------------------------------------
bool SetUseExpect100ContinueInWCFCalls(bool bSet)
{
	bool bOld = System::Net::ServicePointManager::Expect100Continue;
	System::Net::ServicePointManager::Expect100Continue = bSet;
	return bOld;
}

//----------------------------------------------------------------------------
DataDate GetInstallationDate()
{
	DateTime date = Microarea::TaskBuilderNet::Core::Generic::InstallationData::InstallationDate;

	return DataDate(date.Day, date.Month, date.Year, date.Hour, date.Minute, date.Second);

}

//----------------------------------------------------------------------------
void InitThreadCulture()
{
	Thread::CurrentThread->CurrentUICulture = gcnew CultureInfo(gcnew String(AfxGetCulture()));
}


//----------------------------------------------------------------------------
bool CopyTBLinkToClipboard(const CString& tblink)
{
	return TBLinkHelper::CopyTBLinkToClipboard(gcnew String(tblink));
}

//----------------------------------------------------------------------------
void ApplicationDoEvents()
{
	System::Windows::Forms::Application::DoEvents();
}
//----------------------------------------------------------------------------
void ApplicationRaiseIdle()
{
	System::Windows::Forms::Application::RaiseIdle(EventArgs::Empty);
}
//----------------------------------------------------------------------------
// TBUnmanagedFilter class
//----------------------------------------------------------------------------
ref class TBUnmanagedFilter : public System::Windows::Forms::IMessageFilter
{
	int m_lIdleCount;
	CWinThread* m_pThread;
public:
	TBUnmanagedFilter(CWinThread* pThread) : m_pThread(pThread), m_lIdleCount(0)
	{
	}
	virtual bool PreFilterMessage(System::Windows::Forms::Message% m)
	{
		m_lIdleCount = 0;

		if( (m.Msg >= WM_KEYFIRST && m.Msg <= WM_KEYLAST))
		{
			MSG msg;
			msg.message = m.Msg;
			msg.hwnd = (HWND)(int)m.HWnd;
			msg.lParam = (LPARAM)(int)m.LParam;
			msg.wParam = (WPARAM)(int)m.WParam;
			if (AfxGetThread()->PreTranslateMessage(&msg))
			{
				return true;
			}
		}
		return false;
	}
	void OnIdle(Object^sender, EventArgs^ args)
	{
		m_pThread->OnIdle(m_lIdleCount++);
	}
};

//----------------------------------------------------------------------------
void ApplicationRun(CWinThread* pThread)
{
	TBUnmanagedFilter^ handler = gcnew TBUnmanagedFilter(pThread);
	System::Windows::Forms::Application::Idle += gcnew EventHandler(handler, &TBUnmanagedFilter::OnIdle);
	System::Windows::Forms::Application::AddMessageFilter(handler);
	System::Windows::Forms::Application::Run();

	System::Windows::Forms::Application::Idle -= gcnew EventHandler(handler, &TBUnmanagedFilter::OnIdle);
}

//----------------------------------------------------------------------------
bool ApplicationFilterMessage(LPMSG lpMsg)
{
	System::Windows::Forms::Message msg;
	msg.Msg = lpMsg->message;
	msg.HWnd = (IntPtr)(int)lpMsg->hwnd;
	msg.LParam = (IntPtr)(int)lpMsg->lParam;
	msg.WParam = (IntPtr)(int)lpMsg->wParam;
	if (System::Windows::Forms::Application::FilterMessage(msg))
	{
		lpMsg->message = msg.Msg;
		lpMsg->hwnd = (HWND)(int)msg.HWnd;
		lpMsg->lParam = (LPARAM)(int)msg.LParam;
		lpMsg->wParam = (WPARAM)(int)msg.WParam;
		return true;
	}
	return false;
}
//----------------------------------------------------------------------------
///
void PushToClients(const CString& sClientId, const CString& sMessage)
{
	ServerWebSocketConnector::PushToClients(gcnew String(sClientId), gcnew String(sMessage));
}

//----------------------------------------------------------------------------
int GetWebSocketsConnectorPort()
{
	return ServerWebSocketConnector::Port;
}

//----------------------------------------------------------------------------
void MakeTransparent(const BYTE* sourceBuffer, int nSourceSize, COLORREF transparentColor, BYTE*& buffer, int &nSize)
{
	cli::array<Byte>^ sourceAr = gcnew cli::array<Byte>(nSourceSize);
	Marshal::Copy((IntPtr)(void*)sourceBuffer, sourceAr, 0, nSourceSize); 
	
	MemoryStream^ sourceStream = gcnew MemoryStream(sourceAr);
	Bitmap^ bmp = (Bitmap^) Bitmap::FromStream(sourceStream);
	bmp->MakeTransparent(Color::FromArgb(transparentColor));
	MemoryStream^ ms = gcnew MemoryStream();
	bmp->Save(ms, ImageFormat::Png);
	cli::array<Byte>^ ar = ms->ToArray();
	nSize = ar->Length;
	buffer = new BYTE[nSize];
	Marshal::Copy(ar, 0, (IntPtr)buffer, nSize); 
	delete bmp;
}

void MengleActivationString(CString& sActivation)
{
	sActivation = Functions::GetSafeActivationString(gcnew String(sActivation));
}