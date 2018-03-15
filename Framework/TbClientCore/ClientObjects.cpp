
#include "stdafx.h"

#include <TbNameSolver\FileSystemFunctions.h>
#include <TbNameSolver\PathFinder.h>
#include <TbNameSolver\TBNamespaces.h>
#include <TbNameSolver\IFileSystemManager.h>
#include <TbNameSolver\Diagnostic.h>

#include <TbGeneric\DataObj.h>
#include <TbGeneric\FunctionCall.h>
#include <TbGeneric\GeneralFunctions.h>
#include <TbGeneric\SettingsTable.h>
#include <TbGeneric\ParametersSections.h>

#include <TbClientCore\ServerConnectionInfo.h>
#include <TbGenlibManaged\main.h>
#include <TbWebServicesWrappers\TbServicesWrapper.h>
#include <TbWebServicesWrappers\LockManagerInterface.h>

#include "ClientObjects.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

//----------------------------------------------------------------------------
CClientObjects::CClientObjects()
:
	m_pTbServicesWrapper	(NULL),
	m_pServerConnectionInfo	(NULL),
	m_pLoginManagerInterface(NULL)
{
	
}

//----------------------------------------------------------------------------
CClientObjects::~CClientObjects()
{
	delete m_pServerConnectionInfo;
	delete m_pTbServicesWrapper;
	delete m_pLoginManagerInterface;
}

// inizializzazione degli oggetti base per la comunicazione socket
//----------------------------------------------------------------------------
BOOL CClientObjects::InitWebServicesConnections(const CString& strWebServer, const CString& strInstallation)
{ 
	CPathFinder* pPathFinder = AfxGetPathFinder();
	pPathFinder->SetWebServiceInstallation(strInstallation);

	// se deve essere tutto inizializzato chiudo la 
	// comunicazione con i server e la rigenero 
	if (m_pServerConnectionInfo)
		delete m_pServerConnectionInfo;

	m_pServerConnectionInfo	= new CServerConnectionInfo();
	CString sFileContent = AfxGetFileSystemManager()->GetServerConnectionConfig();
	if (!m_pServerConnectionInfo->Parse(sFileContent))
	{
		delete m_pServerConnectionInfo;
		m_pServerConnectionInfo = NULL;
		AfxGetDiagnostic()->Add (_TB("Error parsing Custom\\ServerConnection.config file."), CDiagnostic::Error);	
		return FALSE;
	}

	if (m_pTbServicesWrapper)
		delete m_pTbServicesWrapper;
	
	m_pTbServicesWrapper = new TbServicesWrapper
		(
		pPathFinder->GetTbServicesName(),
		_T("http://microarea.it/TbServices/"),
		strWebServer,
		m_pServerConnectionInfo->m_nWebServicesPort
		);

	if (m_pLoginManagerInterface)
		delete m_pLoginManagerInterface;

	m_pLoginManagerInterface = new CLoginManagerInterface(
			pPathFinder->GetLoginServiceName(),
			strWebServer,
			m_pServerConnectionInfo->m_nWebServicesPort
			);

	InitActivationInfos();
		
	return TRUE;
}

//----------------------------------------------------------------------------
void CClientObjects::GetActivationStateWarning ()
{
	// gli unici stati che devono comuncare degli warning
	if (AfxGetLoginManager()->GetActivationState() != CLoginManagerInterface::DemoWarning)
		return;

	AfxGetDiagnostic()->Add (_TB("This product is running in a Demo Version."), CDiagnostic::Warning);
}

//----------------------------------------------------------------------------
const CString CClientObjects::GetActivationStateInfo ()
{
	CString sMessage;

	switch (AfxGetLoginManager()->GetActivationState())
	{
		case CLoginManagerInterface::Activated:
			sMessage = _TB("Activated");
			break;
		case CLoginManagerInterface::Demo:
		case CLoginManagerInterface::DemoWarning:
			sMessage = _TB("Demo Version");
			break;
		case CLoginManagerInterface::SilentWarning:
				sMessage = _TB("Activation in Silent Warning mode");
			break;
		case CLoginManagerInterface::Warning:
			sMessage = _TB("Activation expiring");
			break;
		case CLoginManagerInterface::Disabled:
			sMessage = _TB("Disabled");
			break;
		default:
			sMessage = _TB("Not Activated");
			break;
	}

	// serial number type
	switch (AfxGetLoginManager()->GetSerialNumberType())
	{
		case CLoginManagerInterface::Development:
			sMessage += _TB(" For Developer Only ");
			break;
		case CLoginManagerInterface::Reseller:
			sMessage += _TB(" For Resellers Only ");
			break;
		case CLoginManagerInterface::Distributor:
			sMessage += _TB(" For Distributors Only ");
			break;
		default:
			break;
	}
	
	return sMessage;
}

//----------------------------------------------------------------------------
BOOL CClientObjects::IsActivated (const CString& strApplication, const CString& strModuleOrFunctionality)
{
	CTBNamespace aModNamespace;
	aModNamespace.SetType (CTBNamespace::MODULE);
	aModNamespace.SetApplicationName (strApplication);
	aModNamespace.SetObjectName (strModuleOrFunctionality);

	return IsActivated (aModNamespace);
}

//----------------------------------------------------------------------------
void CClientObjects::ReinitActivationInfos()
{
	CMapStringToOb* pInfos = const_cast<CMapStringToOb*>(&m_ActivationInfos);

	pInfos->RemoveAll();
	InitActivationInfos();
}

//----------------------------------------------------------------------------
void CClientObjects::InitActivationInfos()
{
	CStringArray arModules;
	m_pLoginManagerInterface->GetActivatedModules  (arModules);
	AddInActivationInfo (arModules);
}

//----------------------------------------------------------------------------
BOOL CClientObjects::IsActivated(const CTBNamespace& aNamespace)
{
	DataObj* pDataObj = AfxGetSettingValue(snsTbGenlib, szDevelopmentSection, szAllActive, DataBool(FALSE), szTbDefaultSettingFileName);
	BOOL bByPass = pDataObj && pDataObj->GetDataType() == DATA_BOOL_TYPE ? *((DataBool*)pDataObj) : FALSE;
		
	CString strTemp = aNamespace.ToString();

	if (bByPass)
	{
		if (aNamespace.GetApplicationName().CompareNoCase(_T("ERP")) == 0 &&
				(
					aNamespace.GetModuleName().CompareNoCase(_T("MasterData_BR")) == 0 
				)
			)
			return FALSE;

		return TRUE;
	}

	LPCTSTR pszKey = NULL;
	return m_ActivationInfos.LookupKey(strTemp.MakeLower(), pszKey);
}

//----------------------------------------------------------------------------
void CClientObjects::AddInActivationInfo (const CString& sModule)
{
	CMapStringToOb* pInfos = const_cast<CMapStringToOb*>(&m_ActivationInfos);

	CString strNs, strType, strApplication, strModuleOrFunctionality, strAddOnLibrary;
	int nModulePos, nLibraryPos;
	strNs = sModule;
	// the string has a namespace format
	nModulePos = sModule.Find (CTBNamespace::GetSeparator());
	if (nModulePos < 0)
		return;

	nLibraryPos	= strNs.Find (CTBNamespace::GetSeparator(), nModulePos + 1);

	// application.module oppure application.module.library
	strApplication = strNs.Left (nModulePos).Trim(); 
	strAddOnLibrary = nLibraryPos > 0 ? strNs.Mid (nLibraryPos + 1).Trim() : _T("");
		
	if (strAddOnLibrary.IsEmpty())
		strModuleOrFunctionality = strNs.Mid (nModulePos + 1).Trim(); 
	else
		strModuleOrFunctionality = strNs.Mid (nModulePos + 1, nLibraryPos - nModulePos - 1).Trim(); 

	if (strApplication.IsEmpty() || strModuleOrFunctionality.IsEmpty())
		return;

	CTBNamespace aNs (CTBNamespace::MODULE, strApplication + CTBNamespace::GetSeparator () + strModuleOrFunctionality);

	if (nLibraryPos > 0)
	{
		aNs.SetType(CTBNamespace::LIBRARY);
		aNs.SetObjectName(strAddOnLibrary);
	}

	strNs = aNs.ToString();
	pInfos->SetAt(strNs.MakeLower(), NULL);
}
//----------------------------------------------------------------------------
void CClientObjects::AddInActivationInfo (const CStringArray& arModules)
{
	for (int i = 0; i <= arModules.GetUpperBound(); i++)
	{
		AddInActivationInfo(arModules.GetAt (i));
	}
}

//----------------------------------------------------------------------------
const CServerConnectionInfo* CClientObjects::GetServerConnectionInfo () const
{ 
	return m_pServerConnectionInfo;
}

//============================================================================
// Global Functions 
//=============================================================================
//----------------------------------------------------------------------------
void AFXAPI AfxReinitActivationInfos()
{
	CClientObjects* pCClientObjects = AfxGetCommonClientObjects();

	if (pCClientObjects)
		pCClientObjects->ReinitActivationInfos(); 
}

//----------------------------------------------------------------------------
CClientObjects* AFXAPI AfxGetCommonClientObjects()
{
	return AfxGetApplicationContext()->GetObject<CClientObjects>(&CApplicationContext::GetClientObjects); 
}

//----------------------------------------------------------------------------
TbServicesWrapper* AFXAPI AfxGetTbServices()
{
	return AfxGetCommonClientObjects()->GetTbServices();
}


//----------------------------------------------------------------------------
CLoginManagerInterface*	AFXAPI AfxGetLoginManager ()
{
	return AfxGetCommonClientObjects()->GetLoginManager();
}




//=============================================================================
// verifica la configurazione attuale del programma

//chiede a login manager se un modulo o funzionalità di un'app è stato comprato
//----------------------------------------------------------------------------
BOOL AFXAPI AfxIsActivated (const CString& strApplication, const CString& strModuleOrFunctionality, BOOL bForceCall /*FALSE*/)
{
	// force call to web method
	if (bForceCall && AfxGetLoginManager())
		return AfxGetLoginManager()->IsActivated(strApplication, strModuleOrFunctionality);
	
	CClientObjects*	pClientObjects = AfxGetCommonClientObjects();

	if (!pClientObjects)
		return FALSE;

	return pClientObjects->IsActivated (strApplication, strModuleOrFunctionality);
}

// add-on library checking overload
//----------------------------------------------------------------------------
BOOL AFXAPI AfxIsActivated	(const CTBNamespace& sLibraryNamespace)
{
	CClientObjects* pClientObjects = AfxGetCommonClientObjects();

	if (!pClientObjects)
		return FALSE;

	return pClientObjects->IsActivated (sLibraryNamespace);
}

//----------------------------------------------------------------------------
BOOL AFXAPI AfxIsCalAvailable (const CString& strApplication, const CString& strModuleOrFunctionality)
{
	CLoginManagerInterface* pLoginManagerInterface = AfxGetLoginManager();
	if (!pLoginManagerInterface)
		return FALSE;


	return pLoginManagerInterface->IsCalAvailable(strApplication, strModuleOrFunctionality);
}

//----------------------------------------------------------------------------
BOOL AFXAPI AfxFeUsed()
{
	CLoginManagerInterface* pLoginManagerInterface = AfxGetLoginManager();
	if (!pLoginManagerInterface)
		return FALSE;


	return pLoginManagerInterface->FeUsed();
}

//----------------------------------------------------------------------------
BOOL AFXAPI AfxIsValidDate (const DataDate operationDate, DataDate& maxDate)
{
	CLoginManagerInterface* pLoginManagerInterface = AfxGetLoginManager();
	if (!pLoginManagerInterface)
		return FALSE;
	
	CLoginContext* pLoginContext = AfxGetLoginContext();
	pLoginContext ->m_bMluValid = pLoginManagerInterface->IsValidDate(operationDate, maxDate);
	return pLoginContext ->m_bMluValid;
}

//----------------------------------------------------------------------------
BOOL AFXAPI AfxSetCompanyInfo (const CString& strName, const CString& strValue)
{
	CLoginManagerInterface* pLoginManagerInterface = AfxGetLoginManager();
	if (!pLoginManagerInterface)
		return FALSE;

	return pLoginManagerInterface->SetCompanyInfo(strName, strValue);
}
