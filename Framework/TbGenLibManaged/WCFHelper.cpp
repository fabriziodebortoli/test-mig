#include "stdafx.h"

#include <TbNameSolver\LoginContext.h>
#include <TbNameSolver\ApplicationContext.h>
#include <TbGeneric\FunctionObjectsInfo.h>
#include <tbgeneric\generalfunctions.h>
#include <TbGeneric\WebServiceStateObjects.h>

#include "WCFHelper.h"

using namespace Microarea::TaskBuilderNet::Core::SoapCall;
using namespace System;
using namespace System::Reflection;
using namespace System::Collections::Generic;

class CGlobalObjects
{
public:
	CGlobalObjects() : m_pHost(NULL) {}
	 
	IHostApplication*	m_pHost;
	int					m_nTbLoaderSOAPPort;
	int					m_nTbLoaderTCPPort;
	TimeSpan			m_WebServicesTimeout;
} g_Globals;

//------------------------------------------------------------------
static CWCFHelper::CWCFHelper()
{
	freeMap = gcnew List<String^>();
	//funzioni che non necessitano di avere un authentication token valido
	freeMap->AddRange(gcnew array<String^>
	{
		_NS_WEB("Framework.TbGes.TbGes.GetLogins"),
		_NS_WEB("Framework.TbGes.TbGes.GetProcessID"),
		_NS_WEB("Framework.TbGes.TbGes.IsLoginValid"),
		_NS_WEB("Framework.TbGes.TbGes.CanCloseTB"),
		_NS_WEB("Framework.TbGes.TbGes.CloseTB"),
		_NS_WEB("Framework.TbGes.TbGes.DestroyTB"),
		_NS_WEB("Framework.TbGes.TbGes.UseRemoteInterface"),
		_NS_WEB("Framework.TbGes.TbGes.Login"),
		_NS_WEB("Framework.TbGes.TbGes.SetMenuHandle"),
		_NS_WEB("Framework.TbGes.TbGes.EnableSoapFunctionExecutionControl"),
		_NS_WEB("Framework.TbGes.TbGes.GetWindowInfosFromPoint"),
		_NS_WEB("Framework.TbGes.TbGes.ClearCache"),
		_NS_WEB("Framework.TbGes.TbGes.GetActiveThreads"),
		_NS_WEB("Framework.TbGes.TbGes.GetApplicationContextMessages")
	
	});

	freeMapForAdmin = gcnew List<String^>();
	//funzioni che non necessitano di avere un authentication token valido se si e' amministratori
	freeMapForAdmin->AddRange(gcnew array<String^>
	{
		_NS_WEB("Framework.TbGes.TbGes.KillThread"),
		_NS_WEB("Framework.TbGes.TbGes.StopThread"),
		_NS_WEB("Framework.TbGes.TbGes.CanStopThread"),
		_NS_WEB("Framework.TbGes.TbGes.GetActiveThreads"),
		_NS_WEB("Framework.TbGes.TbGes.DestroyTB")
	});

	nonResolvableAssemblies = gcnew List<String^>();
}
//------------------------------------------------------------------
void CWCFHelper::SetHost(IHostApplication* pHost, int nTbLoaderSOAPPort, int nTbLoaderTCPPort)
{						 
	g_Globals.m_pHost = pHost;
	g_Globals.m_nTbLoaderSOAPPort = nTbLoaderSOAPPort;
	g_Globals.m_nTbLoaderTCPPort = nTbLoaderTCPPort;
}
//------------------------------------------------------------------
void CWCFHelper::SetWebServicesTimeout(int nWebServicesTimeout)
{
	g_Globals.m_WebServicesTimeout = TimeSpan::FromMilliseconds(nWebServicesTimeout);
}

//------------------------------------------------------------------
int CWCFHelper::GetSOAPPort()
{
	return g_Globals.m_nTbLoaderSOAPPort;
}

//------------------------------------------------------------------
int CWCFHelper::GetTCPPort()
{
	return g_Globals.m_nTbLoaderTCPPort;
}
//------------------------------------------------------------------
TimeSpan CWCFHelper::GetWebServicesTimeout()
{
	return g_Globals.m_WebServicesTimeout;
}

//------------------------------------------------------------------
bool CWCFHelper::EnableSoapExecutionControl(bool bEnable)
{
	return g_Globals.m_pHost->EnableSoapExecutionControl(bEnable == TRUE) == TRUE;
}
//------------------------------------------------------------------
Type^ CWCFHelper::ManagedTypeFromDataType(DataType type)
{
	WORD wType = type.m_wType;
	if (DataType::Null == wType) { return nullptr; }
	if (DataType::Void == wType) { return nullptr; }
	if (DataType::String == wType) { return String::typeid; }
	if (DataType::Integer == wType) { return int::typeid; }
	if (DataType::Long == wType) { return int::typeid; }
	if (DataType::Object == wType) { return int::typeid; }
	if (DataType::Double == wType) { return double::typeid; }
	if (DataType::Money == wType) { return double::typeid; }
	if (DataType::Quantity == wType) { return double::typeid; }
	if (DataType::Percent == wType) { return double::typeid; }
	if (DataType::Date == wType) { return String::typeid; }
	if (DataType::DateTime == wType) { return String::typeid; }
	if (DataType::Time == wType) { return String::typeid; }
	if (DataType::ElapsedTime == wType) { return long::typeid; }
	if (DataType::Bool == wType) { return bool::typeid; }
	if (DataType::Enum == wType) { return int::typeid; }
	if (DataType::Guid == wType) { return String::typeid; }
	if (DataType::Text == wType) { return String::typeid; }
	if (DataType::Blob == wType) { return Byte::typeid; }

	throw gcnew Exception(String::Format("Unsupported type: {0}", wType));

}

//------------------------------------------------------------------
bool CWCFHelper::IsArrayType(CDataObjDescription* pParam)
{
	return pParam->IsArray() || pParam->GetBaseDataType().m_wType == DataType::Blob.m_wType;
}
//------------------------------------------------------------------
String^ CWCFHelper::ManagedTypeFromDataType(CDataObjDescription* pParam)
{
	DataType dataType = pParam->GetBaseDataType();
	Type^ t = ManagedTypeFromDataType(dataType);
	
	String^ ret = t == nullptr ? "void" : t->FullName;
	
	if (IsArrayType(pParam))
		ret = ret + "[]";

	return ret;
}

//------------------------------------------------------------------
Object^ CWCFHelper::DataObjToObject(DataObj* pDataObj, System::Type^ destType)
{
	Type^ typeToCreate = destType;
	bool isDestTypeArray = false;
	if (destType->IsByRef)
	{
		//se fosse un reference type, avrei un tipo reference che non riesco ad istanziare (nel nome ha un &), 
		//allora devo trimmare il carattere di reference alla fine ed ottenere il corrispondente
		//tipo NON reference
		String^ typeName =  destType->FullName->TrimEnd('&');

		//se e` un array, devo crearlo con la classe Array, che passa
		//il numero di elementi al costruttore
		if (typeName->EndsWith("[]"))
		{
			typeName = typeName->Substring(0, typeName->LastIndexOf("[]"));
			isDestTypeArray = true;
		}
		typeToCreate = destType->Assembly->GetType(typeName);
	}

	DataType dt = pDataObj->GetDataType();

	if (dt == DataType::String)
	{
		Object^ o = gcnew String((LPCTSTR)pDataObj->GetRawData());
		if (destType->IsEnum && !o->GetType()->IsEnum)
			o = Enum::Parse(destType, o->ToString());
		return o;
	}
	else if (dt == DataType::Integer)
		return Convert::ChangeType((*((short*)pDataObj->GetRawData())), typeToCreate);//occhio: il dataint al suo interno e' uno short
	else if (dt == DataType::Bool)
		return Convert::ChangeType((*((bool*)pDataObj->GetRawData())), typeToCreate);

	else if (dt == DataType::Money)
		return Convert::ChangeType((*((double*)pDataObj->GetRawData())), typeToCreate);
	else if (dt == DataType::Quantity)
		return Convert::ChangeType((*((double*)pDataObj->GetRawData())), typeToCreate);
	else if (dt == DataType::Percent)
		return Convert::ChangeType((*((double*)pDataObj->GetRawData())), typeToCreate);
	else if (dt == DataType::Double)
		return Convert::ChangeType((*((double*)pDataObj->GetRawData())), typeToCreate);

	else if (dt.m_wType == DATA_DATE_TYPE /*dt == DataType::Date  || dt == DataType::DateTime || dt == DataType::Time*/)
	{
		BSTR bstr = ((DataDate*)pDataObj)->GetSoapValue();
		String^ ret = gcnew String(bstr);
		::SysFreeString(bstr);
		return Convert::ChangeType(ret, typeToCreate);
	}

	else if (dt.m_wType == DATA_ENUM_TYPE) //DataType::Enum has m_wTag == 0
	{
		Object^ o = (((DataEnum*)pDataObj)->GetSoapValue());
		//l'enumerativo mi potrebbe arrivare come numero o come stringa
		if (destType->IsEnum && !o->GetType()->IsEnum)
			o = Enum::Parse(destType, o->ToString());
		return Convert::ChangeType(o, typeToCreate);
	}
	else if (dt == DataType::Long)
		return Convert::ChangeType((*((long*)pDataObj->GetRawData())), typeToCreate);
	else if (dt == DataType::ElapsedTime)
		return Convert::ChangeType((*((long*)pDataObj->GetRawData())), typeToCreate);
	else if (dt == DataType::Object)
		return Convert::ChangeType((*((long*)pDataObj->GetRawData())), typeToCreate);

	else if (dt == DataType::Guid)
		return Convert::ChangeType(gcnew String((LPCTSTR)pDataObj->GetRawData()), typeToCreate);
	else if (dt == DataType::Text)
		return Convert::ChangeType(gcnew String((LPCTSTR)pDataObj->GetRawData()), typeToCreate);

	else if (dt == DataType::Blob)
	{
		int count = ((DataBlob*)pDataObj)->GetLen();
		array<Byte>^ ar = gcnew array<Byte>(count);
		for (int i = 0; i < count; i++)
			ar[i] = ((BYTE*)((DataBlob*)pDataObj)->GetRawData())[i];
		return Convert::ChangeType(ar, typeToCreate);
	}
	else if (dt == DataType::Array)
	{
		DataArray* pArray = (DataArray*) pDataObj;
		
		//il tipo atteso potrebbe essere un array, oppure un derivato di List<>
		//per fortuna entrambi implementano IList
		System::Collections::IList^ list = nullptr;
		if (destType->IsArray) //caso dell'array
			list = gcnew ArrayList();
		else
		{
			//caso di List<> o []
			if (isDestTypeArray)	
				//creo dinamicamente il tipo []
				list = System::Array::CreateInstance(typeToCreate, pArray->GetSize());
			else
				//creo dinamicamente il tipo List<>
				list = (System::Collections::IList^) Activator::CreateInstance(typeToCreate);
		}

		//calcolo il tipo managed contenuto nell'array
		Type^ baseManagedType = ManagedTypeFromDataType(pArray->GetBaseDataType());
		
		ASSERT(baseManagedType != nullptr);

		//travaso i dati unmanaged in quelli managed
		for (int i = 0; i < pArray->GetSize(); i++)
		{
			DataObj* pObj = pArray->GetAt(i);
			ASSERT_VALID(pObj);
			Object^ pO = DataObjToObject(pObj, baseManagedType);
			ASSERT(pO != nullptr);
			if (list->IsFixedSize)
				list[i] = pO;
			else
				list->Add(pO);	
		}
		
		//ritorno, a seconda del tipo atteso, l'array oppure la List<>
		return destType->IsArray
			? ((ArrayList^)list)->ToArray(baseManagedType)
			: list;
	}

	return nullptr;
}
//------------------------------------------------------------------
void CWCFHelper::AssignObjectToDataObj(Object^ obj, DataObj* pDataObj)
{
	if (obj == nullptr)
		return;

	DataType type = pDataObj->GetDataType();

	if (type == DataType::String)
		pDataObj->Assign(DataStr(obj->ToString()));
	else if (type == DataType::Integer)
		pDataObj->Assign(DataInt(Convert::ToInt32(obj)));
	else if (type == DataType::Bool)
		pDataObj->Assign(DataBool(Convert::ToBoolean(obj)));

	else if (type == DataType::Money)
		pDataObj->Assign(DataDbl(Convert::ToDouble(obj)));
	else if (type == DataType::Quantity)
		pDataObj->Assign(DataDbl(Convert::ToDouble(obj)));
	else if (type == DataType::Percent)
		pDataObj->Assign(DataDbl(Convert::ToDouble(obj)));
	else if (type == DataType::Double)
		pDataObj->Assign(DataDbl(Convert::ToDouble(obj)));

	else if (type == DataType::Date)
		pDataObj->AssignFromXMLString(CString(obj->ToString()));
	else if (type == DataType::DateTime)
		pDataObj->AssignFromXMLString(CString(obj->ToString()));
	else if (type == DataType::Time)
		pDataObj->AssignFromXMLString(CString(obj->ToString()));

	else if (type == DataType::Long)
		pDataObj->Assign(DataLng(Convert::ToInt32(obj)));
	else if (type == DataType::ElapsedTime)
		pDataObj->Assign(DataLng(Convert::ToInt32(obj)));
	else if (type == DataType::Object)
		pDataObj->Assign(DataLng(Convert::ToInt32(obj)));

	else if (type.m_wType == DataType::Enum.m_wType)	//DataType::Enum has m_wTag == 0
		pDataObj->Assign(DataEnum(Convert::ToInt32(obj)));

	else if (type == DataType::Guid)
		pDataObj->Assign(CString(obj->ToString()));
	else if (type == DataType::Text)
		pDataObj->Assign(CString(obj->ToString()));

	else if (type == DataType::Blob)
	{
		DataBlob* pBlob = (DataBlob*)pDataObj;
		array<Byte>^ ar = (array<Byte>^)obj;
		int count = ((array<Byte>^)obj)->Length;
		pBlob->SetAllocSize(count);
		pBlob->SetUsedLen(count);
		for (int i = 0; i < count; i++)
			((BYTE*)pBlob->GetRawData())[i] = ar[i];
	}
	else if (type == DataType::Array)
	{
		DataArray* pArray = (DataArray*) pDataObj;
		pArray->Clear();
		if (System::Collections::IList::typeid->IsInstanceOfType(obj))
		{
			System::Collections::IList ^ar = (System::Collections::IList^) obj;
			for (int i = 0; i < ar->Count; i++)
			{
				DataObj *pNewDataObj = DataObj::DataObjCreate(pArray->GetBaseDataType());
				AssignObjectToDataObj(ar[i], pNewDataObj);
				pArray->Add(pNewDataObj);
			}
		}
		else //compatibilità pregressa: trasformo un object nell'array di properties
		{
			for each (PropertyInfo^ pi in obj->GetType()->GetProperties())
			{
				DataStr* pValue = new DataStr(pi->GetValue(obj, nullptr)->ToString());
				pArray->Add(pValue);
			}
		}
	}
}

//------------------------------------------------------------------
void LoadTBDllFunction(const CString& strDllName)
{
	::AfxLoadLibrary(strDllName);
}

//------------------------------------------------------------------
void CWCFHelper::LoadTBDllFunction(String^ dllName)
{
	//faccio caricare esplicitamente la dll dal thread principale, perche' quello corrente potrebbe non avere
	//MFC inizializzato
	AfxInvokeThreadGlobalProcedure<const CString& >(AfxGetApplicationContext()->GetAppMainWnd(), &::LoadTBDllFunction, CString(dllName));
}

//------------------------------------------------------------------
///restituisce l'handle di finestra per mandare la chiamata WCF al thread giusto
IntPtr CWCFHelper::GetThreadHwndFunction(String^ authenticationToken, int contextHandle, String^ functionNamespace, bool checkAuthToken)
{
	//prima recupero il login context associato all'authentication token
	CLoginContext* pContext = AfxGetLoginContext(authenticationToken);
	 
	//se devo controllarne la validita` e non ho un login context, allora l'authentication token non e` buono, lancio eccezione
	//(fanno eccezione alcune funzioni libere quali ad es GetProcessId)
	if (checkAuthToken && !pContext && !IsFreeWcfFunction(functionNamespace, authenticationToken) )
		throw gcnew TBSoapException(gcnew String(_TB("Access denied: wrong authentication token")));
	
	//chiedo all'applicazione se posso eseguire il metodo richiesto 
	CString strError, strNamespace(functionNamespace);
	if (!g_Globals.m_pHost->CanExecuteSoapMethod(pContext, strNamespace, strError))
			throw gcnew TBSoapException(gcnew String(strError));

	//se mi viene passato un handle di contesto, devo trovare il thread che ha creato quel contesto
	if (contextHandle != 0)
	{
		CObject* pObject = (CObject*)contextHandle;
		try
		{
			HWND hwnd = NULL;
			
			//esiste una mappa che associa ad ogni oggetto di contesto la sua finestra di thread
			if (AfxGetWebServiceStateObjectThreadWnd(pObject, hwnd))
				return (IntPtr)hwnd;
		}
		catch(...)
		{
			//invalid object reference
		}

		CString message;
		message.Format(_TB("Invalid object reference:") + _T(" %p"), contextHandle);
		throw gcnew TBSoapException(gcnew String(message));
	}

	CApplicationContext *pAppContext = AfxGetApplicationContext();
	if (!pAppContext->IsMultiThreadedDocument())
		return (IntPtr)pAppContext->GetAppMainWnd(); 
	//se invece non ho contesto, allora recupero il thread corrente (se la chiamata mi arriva
	//dal framework WCF, il thread corrente non sara` di tipo CTBWinThread, quindi il valore di ritorno
	//sara` NULL
	CWinThread *pTBThread = AfxGetTBThread();
	if (pTBThread)
		return (IntPtr)pTBThread->m_pMainWnd->m_hWnd;

	//se il thread corrente non e' di tipo CTBWinThread, allora mando la chiamata al thread 
	//del LoginContext
	if (pContext)
		return (IntPtr)pContext->m_pMainWnd->m_hWnd;
	
	//se non esiste LoginContext, la mando al thread principale
	return (IntPtr)pAppContext->GetAppMainWnd(); 
}

//------------------------------------------------------------------
bool CWCFHelper::IsFreeWcfFunction(String^ functionNamespace, String^ authenticationToken)
{
	return freeMap->Contains(functionNamespace) ||
		(authenticationToken == adminToken && freeMapForAdmin->Contains(functionNamespace));
}

//------------------------------------------------------------------
void CWCFHelper::AddNonResolvableAssembly(String^ asmName)
{
	nonResolvableAssemblies->Add(asmName);
}
//------------------------------------------------------------------
bool CWCFHelper::IsNonResolvableAssembly(String^ asmName)
{
	return nonResolvableAssemblies->Contains(asmName);
}

//------------------------------------------------------------------
IntPtr CWCFHelper::GetThreadMainWnd()
{
	return (IntPtr)(int)::GetThreadMainWnd();
}

//------------------------------------------------------------------
gcroot<System::Object^> __Invoke(gcroot<Delegate^> d)
{
	gcroot<System::Object^> ret = d->DynamicInvoke();
	return ret;
}
//------------------------------------------------------------------
System::Object^ CWCFHelper::Invoke(IntPtr hwnd, Delegate^ method)
{
	System::Object^ ret = AfxInvokeThreadGlobalFunction<gcroot<System::Object^>, gcroot<Delegate^>>((HWND)(int)hwnd, &__Invoke, method);
	return ret;
}