#include "stdafx.h"

#include <TbNameSolver\Diagnostic.h>
#include <TbNameSolver\PathFinder.h>
#include <TbNameSolver\Chars.h>
#include <TbNameSolver\FileSystemFunctions.h>
#include <TbNameSolver\IFileSystemManager.h>
#include <TbWebServicesWrappers\LoginManagerInterface.h>
#include <TbGeneric\TBStrings.h>
#include <TbGeneric\Critical.h>
#include <TbGeneric\GeneralFunctions.h>
#include <TbGeneric\ReferenceObjectsInfo.h>
#include <TbGenlib\BaseApp.h>
#include <TbGes\ExtDocAbstract.h>

//Locals
#include "CSubscriptionData.h"

static TCHAR szXmlSubscriptions[]	= _T("Subscriptions");
static TCHAR szXmlSubscription[]	= _T("Subscription");
static TCHAR szXmlAttrNamespace[]	= _T("namespace");
static TCHAR szXmlAttrOperation[]	= _T("operation");
static TCHAR szXmlAttrMessage[]		= _T("message");
static TCHAR szXmlAttrProfile[]		= _T("profile");
static TCHAR szXmlAttrAction[]		= _T("action");
static TCHAR szXmlAttrWhen[]		= _T("when");
static TCHAR szXmlAttrWebServer[]	= _T("webServer");
static TCHAR szXmlAttrWebService[]	= _T("webService");
static TCHAR szXmlAttrWebNS[]		= _T("webNS");
static TCHAR szXmlAttrMethod[]		= _T("method");
static TCHAR szXmlAttrPort[]		= _T("port");
static TCHAR szXmlViewModes[]		= _T("viewModes");

static const TCHAR szEventNewCheck[]		= _T("NC");
static const TCHAR szEventEditCheck[]		= _T("EC");
static const TCHAR szEventDeleteCheck[]	= _T("DC");
static const TCHAR szEventNewBefore[]		= _T("NB");
static const TCHAR szEventEditBefore[]	= _T("EB");
static const TCHAR szEventDeleteBefore[]	= _T("DB");
static const TCHAR szEventNewAfter[]		= _T("NA");
static const TCHAR szEventEditAfter[]		= _T("EA");
static const TCHAR szEventDeleteAfter[]	= _T("DA");

static const TCHAR szOperationWarning[]		= _T("WARNING");
static const TCHAR szOperationDeny[]			= _T("DENY");
static TCHAR szViewModeForeground[]		= _T("Default");
static TCHAR szViewModeBackground[]		= _T("BackGround");

//-----------------------------------------------------------------------------
CSubscriptionInfo* AfxGetSubscriptionInfo()
{
	return AfxGetLoginContext()->GetObject<CSubscriptionInfo>();
}

///////////////////////////////////////////////////////////////////////////////
//							CSubscriptionData							     //
///////////////////////////////////////////////////////////////////////////////

IMPLEMENT_DYNAMIC(CSubscriptionData, CObject)

//-----------------------------------------------------------------------------
CSubscriptionData::CSubscriptionData
						(
							const CString& strExportProfile, 
							CPathFinder::PosType ePosType, 
							const CString& strWebServer, 
							const CString& strWebService, 
							const CString& strWebNS, 
							const CString& strWebMethod, 
							int dPort,
							const CString& strViewModes
						)
	:
	m_strExportProfile	(strExportProfile),
	m_ePosType			(ePosType),
	m_strWebServer		(strWebServer),
	m_strWebService		(strWebService),
	m_strWebNS			(strWebNS),
	m_strWebMethod		(strWebMethod),
	m_Port				(dPort),
	m_strViewModes		(strViewModes)
{
}

//-----------------------------------------------------------------------------
CSubscriptionData::CSubscriptionData(const CString& strOperation, const CString& strMessage, int dAction)
	:
	m_strOperation		(strOperation),
	m_strMessage		(strMessage),
	m_Action			(dAction)
{
}

//-----------------------------------------------------------------------------
BOOL CSubscriptionData::IsActiveInForegroundMode()
{
	return m_strViewModes.Find(szViewModeForeground) >= 0;
}

//-----------------------------------------------------------------------------
BOOL CSubscriptionData::IsActiveInBackgroundMode()
{
	return m_strViewModes.Find(szViewModeBackground) >= 0;
}

///////////////////////////////////////////////////////////////////////////////
//							CSubscriptionInfo							     //
///////////////////////////////////////////////////////////////////////////////

IMPLEMENT_DYNCREATE(CSubscriptionInfo, CObject)

//-----------------------------------------------------------------------------
CSubscriptionInfo::CSubscriptionInfo()
	:
	m_pSubscriptionList (NULL),
	m_pBONEList			(NULL)
{
	m_pSubscriptionList = new CSubscritpionsList();
	m_pBONEList			= new CSubscritpionsList();
	Load();
}

//-----------------------------------------------------------------------------
CSubscriptionInfo::~CSubscriptionInfo()
{
	Clear();
	delete m_pSubscriptionList;
	delete m_pBONEList;
}

//-----------------------------------------------------------------------------
BOOL CSubscriptionInfo::ExistDocumentTrigger(CAbstractFormDoc* pDoc)
{
	CDocumentSubscritpionData* pDocSubscrData = NULL;

	TB_LOCK_FOR_READ();

	CString aNs = pDoc->GetNamespace().ToString();
	m_pSubscriptionList->Lookup(aNs.MakeLower(), pDocSubscrData);
	if (pDocSubscrData != NULL)
		return TRUE;

	return FALSE;
}

//-----------------------------------------------------------------------------
BOOL CSubscriptionInfo::ExistDocumentBONE(CAbstractFormDoc* pDoc)
{
	CDocumentSubscritpionData* pDocSubscrData = NULL;

	TB_LOCK_FOR_READ();
	
	CString aNs = pDoc->GetNamespace().ToString();
	m_pBONEList->Lookup(aNs.MakeLower(), pDocSubscrData);
	if (pDocSubscrData != NULL)
		return TRUE;

	return FALSE;
}

//-----------------------------------------------------------------------------
CDocumentSubscritpionData* CSubscriptionInfo::GetDocumentSubscritpionData (CAbstractFormDoc* pDoc, BOOL bIsBONE)
{
	CDocumentSubscritpionData* pDocSubscrData = NULL;

	TB_LOCK_FOR_READ();
	CString aNs = pDoc->GetNamespace().ToString();
	
	if (bIsBONE)
		m_pBONEList->Lookup(aNs.MakeLower(), pDocSubscrData);
	else
		m_pSubscriptionList->Lookup(aNs.MakeLower(), pDocSubscrData);
	
	return pDocSubscrData;
}

//-----------------------------------------------------------------------------
CEventManagementData* CSubscriptionInfo::GetEventManagementData(CAbstractFormDoc* pDoc, int dAction, CString aWhen, BOOL bIsBONE)
{
	CString strEvent;
	if (dAction == 1 && aWhen == "BEFORE")	strEvent = szEventNewBefore;
	if (dAction == 1 && aWhen == "AFTER")	strEvent = szEventNewAfter;
	if (dAction == 1 && aWhen == "CHECK")	strEvent = szEventNewCheck;
	if (dAction == 2 && aWhen == "BEFORE")	strEvent = szEventEditBefore;
	if (dAction == 2 && aWhen == "AFTER")	strEvent = szEventEditAfter;
	if (dAction == 2 && aWhen == "CHECK")	strEvent = szEventEditCheck;
	if (dAction == 3 && aWhen == "BEFORE")	strEvent = szEventDeleteBefore;
	if (dAction == 3 && aWhen == "AFTER")	strEvent = szEventDeleteAfter;
	if (dAction == 3 && aWhen == "CHECK")	strEvent = szEventDeleteCheck;

	CDocumentSubscritpionData* pDocSubscrData = GetDocumentSubscritpionData(pDoc, bIsBONE);

	if (!pDocSubscrData)
		return NULL;

	CEventManagementData* pEMData = NULL;
	pDocSubscrData->Lookup(strEvent.MakeLower(), pEMData);

	return pEMData;
}

//-----------------------------------------------------------------------------
void CSubscriptionInfo::Clear()
{
	TB_LOCK_FOR_WRITE();
	
	CString aKey;
	CDocumentSubscritpionData* pItem;
	POSITION aPos = m_pSubscriptionList->GetStartPosition();
	while (aPos != NULL)
	{
		m_pSubscriptionList->GetNextAssoc(aPos,aKey,pItem);
		ClearDocumentSubscritpionData(pItem);
		delete pItem;
	}
	m_pSubscriptionList->RemoveAll();

	ClearBONEList();

	m_Producers.RemoveAll();
}

//-----------------------------------------------------------------------------
void CSubscriptionInfo::ClearDocumentSubscritpionData(CDocumentSubscritpionData* pDocumentSubscritpionData)
{
	CString aKey;
	CEventManagementData* pItem;
	POSITION aPos = pDocumentSubscritpionData->GetStartPosition();
	while (aPos != NULL)
	{
		pDocumentSubscritpionData->GetNextAssoc(aPos,aKey,pItem);
		ClearEventManagementData(pItem);
		delete pItem;
	}
}

//-----------------------------------------------------------------------------
void CSubscriptionInfo::ClearEventManagementData(CEventManagementData* pEventManagementData)
{
	CString aKey;
	CSubscriptionData* pItem;
	POSITION aPos = pEventManagementData->GetStartPosition();
	while (aPos != NULL)
	{
		pEventManagementData->GetNextAssoc(aPos,aKey,pItem);
		ClearSubscriptionData(pItem);
		delete pItem;
	}
}

//-----------------------------------------------------------------------------
void CSubscriptionInfo::ClearSubscriptionData(CSubscriptionData* pSubscriptionData)
{
	pSubscriptionData->m_tmpExportResult.RemoveAll();
}

//-----------------------------------------------------------------------------
void CSubscriptionInfo::ClearBONEList()
{
	CString aKey;
	CDocumentSubscritpionData* pItem;
	POSITION aPos = m_pBONEList->GetStartPosition();
	while (aPos != NULL)
	{
		m_pBONEList->GetNextAssoc(aPos,aKey,pItem);
		ClearDocumentSubscritpionData(pItem);
		delete pItem;
	}
	m_pBONEList->RemoveAll();
}

//-----------------------------------------------------------------------------
void CSubscriptionInfo::Load()
{
	//Prima cerco per company corrente
	Load(AfxGetPathFinder()->GetActionSubscriptionsFolderPath());

	//Poi per all companies
	Load(AfxGetPathFinder()->GetActionSubscriptionsFolderPath(CPathFinder::ALL_COMPANIES));
}

//-----------------------------------------------------------------------------
void CSubscriptionInfo::Load(CString strSubscriptionPath)
{
	CXMLDocumentObject aDocument;

	CStringArray arFiles;
	AfxGetFileSystemManager ()->GetFiles (strSubscriptionPath, _T("*.xml"), &arFiles);
	BOOL bFound = FALSE;
	for (int i=0; i <= arFiles.GetUpperBound(); i++)
	{     
		CString sFileName = GetPath (arFiles.GetAt(i));
		CString sProducer = GetNameWithExtension (arFiles.GetAt(i));

		if (CheckProducer(sProducer))
			if (CheckFile(sFileName + CString(SLASH_CHAR) + sProducer, aDocument))
				ParseFile(sProducer, aDocument);
	}
}

//-----------------------------------------------------------------------------
BOOL CSubscriptionInfo::CheckProducer(CString strProducer)
{
	TB_LOCK_FOR_WRITE();
	
	for (int i=0; i <= m_Producers.GetUpperBound(); i++)
		if (m_Producers.GetAt(i) == strProducer)
			return FALSE;

	m_Producers.Add(strProducer);
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CSubscriptionInfo::CheckFile(	CString				strFile,
									CXMLDocumentObject&	aDocument
								 )
{
	if (!ExistFile(strFile))
	{
		AfxGetDiagnostic()->Add(cwsprintf(_TB("The file {0-%s} is missing"), strFile));
		return FALSE;
	}

	// Controllo se l'xml ha un formato valido
	if (!aDocument.LoadXMLFile(strFile))
	{
		AfxGetDiagnostic()->Add (cwsprintf(_TB("Format error in xml file: {0-%s}"), strFile));
		return FALSE;
	}

	// Controllo se l'xml ha un tag radice "Subscriptions"
	CXMLNode* pRoot = aDocument.GetRoot();
	CString sRootName;
	if (!pRoot || !pRoot->GetName(sRootName) || sRootName.CompareNoCase (szXmlSubscriptions) != 0) 
	{
		AfxGetDiagnostic()->Add (cwsprintf(_TB("Missing root element in file: {0-%s}"), strFile));
		return FALSE;
	}

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CSubscriptionInfo::ParseFile(	CString				strProducer,
									CXMLDocumentObject&	aDocument
								)
{
	BOOL bOk = TRUE;
	// Mi leggo la lista di Subscription
	CXMLNodeChildsList* pSubscriptionNodes = aDocument.SelectNodes (_T("/") + CString(szXmlSubscriptions) + _T("/") + szXmlSubscription);

	// Se non ci sono Subscription non ne carico ma non ritorno errori
	if (!pSubscriptionNodes || !pSubscriptionNodes->GetSize())
		return TRUE;

	// anche se un TAG è errato non mi fermo a caricare il file
	// così fornisco una diagnostica completa di tutto
	for (int i=0; i <= pSubscriptionNodes->GetUpperBound(); i++)
		if (pSubscriptionNodes->GetAt(i) && !ParseTagSubscription(strProducer, pSubscriptionNodes->GetAt(i)))
			bOk = FALSE;

	delete pSubscriptionNodes;

	return bOk;
}

//-----------------------------------------------------------------------------
BOOL CSubscriptionInfo::ParseTagSubscription(CString strProducer, CXMLNode* pTagNode)
{
	CString	strTagNS;
	CString	strTagOperation;
	CString	strTagAction;
	CString strEvent;
	CString	strTagMessage;
	CString	strTagProfile;
	CString	strTagWhen;
	CString	strTagWebServer;
	CString	strTagWebService;
	CString	strTagWebNS;
	CString	strTagMethod;
	CString	strTagPort;
	CString	strTagViewModes = szViewModeForeground;
    int dPort = 80;
	CPathFinder::PosType ePosType = CPathFinder::STANDARD;
	
	// leggo l'attributo namespace del TAG
	if (!pTagNode->GetAttribute(szXmlAttrNamespace, strTagNS) || strTagNS.IsEmpty())
	{
		AfxGetDiagnostic()->Add (_T("Elemento <Subscription> privo di attributo namespace! "));
		return FALSE;
	}

	CTBNamespace aNS(CTBNamespace::DOCUMENT, strTagNS);
	strTagNS = aNS.ToString();

	// leggo l'attributo operation del TAG
	if (!pTagNode->GetAttribute(szXmlAttrOperation, strTagOperation) || strTagOperation.IsEmpty())
	{
		AfxGetDiagnostic()->Add (_T("Elemento <Subscription> privo di attributo operation! "));
		return FALSE;
	}

	// leggo l'attributo action del TAG
	if (!pTagNode->GetAttribute(szXmlAttrAction, strTagAction) || strTagAction.IsEmpty())
	{
		AfxGetDiagnostic()->Add (_T("Elemento <Subscription> privo di attributo action! "));
		return FALSE;
	}
	int dAction = atoi((CStringA)strTagAction);

	BOOL bIsBONE = FALSE;

	if (strTagOperation == szOperationWarning || strTagOperation == szOperationDeny)
	{
		bIsBONE = TRUE;

		if (dAction == 1)	strEvent = szEventNewCheck;
		if (dAction == 2)	strEvent = szEventEditCheck;
		if (dAction == 3)	strEvent = szEventDeleteCheck;

		// leggo l'attributo message del TAG
		if (!pTagNode->GetAttribute(szXmlAttrMessage, strTagMessage) || strTagMessage.IsEmpty())
		{
			AfxGetDiagnostic()->Add (_T("Elemento <Subscription> privo di attributo message! "));
			return FALSE;
		}
	}
	else
	{
		// leggo l'attributo profile del TAG
		if (!pTagNode->GetAttribute(szXmlAttrProfile, strTagProfile) || strTagProfile.IsEmpty())
		{
			AfxGetDiagnostic()->Add (_T("Elemento <Subscription> privo di attributo profile! "));
			return FALSE;
		}

		// leggo l'attributo when del TAG
		if (!pTagNode->GetAttribute(szXmlAttrWhen, strTagWhen) || strTagWhen.IsEmpty())
		{
			AfxGetDiagnostic()->Add (_T("Elemento <Subscription> privo di attributo when! "));
			return FALSE;
		}

		if (dAction == 1 && strTagWhen == "BEFORE")	strEvent = szEventNewBefore;
		if (dAction == 1 && strTagWhen == "AFTER")		strEvent = szEventNewAfter;
		if (dAction == 2 && strTagWhen == "BEFORE")	strEvent = szEventEditBefore;
		if (dAction == 2 && strTagWhen == "AFTER")	strEvent = szEventEditAfter;
		if (dAction == 3 && strTagWhen == "BEFORE")	strEvent = szEventDeleteBefore;
		if (dAction == 3 && strTagWhen == "AFTER")	strEvent = szEventDeleteAfter;

		// leggo l'attributo webServer del TAG
		if (!pTagNode->GetAttribute(szXmlAttrWebServer, strTagWebServer) || strTagWebServer.IsEmpty())
		{
			AfxGetDiagnostic()->Add (_T("Elemento <Subscription> privo di attributo webServer! "));
			return FALSE;
		}

		// leggo l'attributo webService del TAG
		if (!pTagNode->GetAttribute(szXmlAttrWebService, strTagWebService) || strTagWebService.IsEmpty())
		{
			AfxGetDiagnostic()->Add (_T("Elemento <Subscription> privo di attributo webService! "));
			return FALSE;
		}

		// leggo l'attributo webNSdel TAG
		if (!pTagNode->GetAttribute(szXmlAttrWebNS, strTagWebNS) || strTagWebNS.IsEmpty())
		{
			AfxGetDiagnostic()->Add (_T("Elemento <Subscription> privo di attributo webNS! "));
			return FALSE;
		}

		// leggo l'attributo method del TAG
		if (!pTagNode->GetAttribute(szXmlAttrMethod, strTagMethod) || strTagMethod.IsEmpty())
		{
			AfxGetDiagnostic()->Add (_T("Elemento <Subscription> privo di attributo method! "));
			return FALSE;
		}

		// leggo l'attributo port del TAG
		if (pTagNode->GetAttribute(szXmlAttrPort, strTagPort) && !strTagPort.IsEmpty())
			dPort = atoi((CStringA)strTagPort);

		// leggo l'attributo viewModes del TAG
		if (!pTagNode->GetAttribute(szXmlViewModes, strTagViewModes) || strTagViewModes.IsEmpty())
			strTagViewModes = szViewModeForeground;

		// Controllo l'esistenza del profilo per il documento indicato
		// nella directory custom COMPANY\USER\userName
		// nella directory custom COMPANY\ALL_USER
		// se non esiste nella STANDARD 
		//check for existing profile for user
		CString strProfilePath = AfxGetPathFinder()->GetExportProfilePath(strTagNS, strTagProfile, CPathFinder::USERS, AfxGetLoginInfos()->m_strUserName, FALSE);
		if (ExistPath(strProfilePath))
			ePosType = CPathFinder::USERS;
		else
		{
			strProfilePath = AfxGetPathFinder()->GetExportProfilePath(strTagNS, strTagProfile, CPathFinder::ALL_USERS);
			if (ExistPath(strProfilePath))
				ePosType = CPathFinder::ALL_USERS;
			else
			{
				ePosType = CPathFinder::STANDARD;
				strProfilePath = AfxGetPathFinder()->GetExportProfilePath(strTagNS, strTagProfile, CPathFinder::STANDARD);
				if (!ExistPath(strProfilePath))
				{
					AfxGetDiagnostic()->Add(cwsprintf(_TB("The profile {0-%s} doesn't exist for the document {1-%s}"), strTagProfile, strTagNS));
					return FALSE;
				}
			}
		}
	}

	// Creo la sottoscrizione
	CSubscriptionData* pSubscriptionData = NULL;
	if (bIsBONE)
		pSubscriptionData = new CSubscriptionData(strTagOperation, strTagMessage, dAction);
	else
		pSubscriptionData = new CSubscriptionData(strTagProfile, ePosType, strTagWebServer, strTagWebService, strTagWebNS, strTagMethod, dPort, strTagViewModes);
	
	CDocumentSubscritpionData* pDocSubscrData = NULL;

	//Modificate le lookup in modo che lavorino sempre su namespace MakeLower altrimenti la minima differenza
	//di casing rompe il funzionamento del trigger
	if (!Lookup(strTagNS, pDocSubscrData, bIsBONE))
	{
		CEventManagementData* pEMData = new CEventManagementData();
		pEMData->SetAt(strProducer.MakeLower(), pSubscriptionData);

		pDocSubscrData = new CDocumentSubscritpionData();
		pDocSubscrData->SetAt(strEvent.MakeLower(), pEMData);

		SetAt(strTagNS, pDocSubscrData, bIsBONE);

		return TRUE;
	}

	CEventManagementData* pEMData = NULL;
	if (!pDocSubscrData->Lookup(strEvent.MakeLower(), pEMData))
	{
		CEventManagementData* pEMData = new CEventManagementData();
		pEMData->SetAt(strProducer.MakeLower(), pSubscriptionData);

		pDocSubscrData->SetAt(strEvent.MakeLower(), pEMData);

		return TRUE;
	}

	CSubscriptionData* pObj;
	if (pEMData->Lookup(strProducer.MakeLower(), pObj))
	{
		AfxGetDiagnostic()->Add (cwsprintf(_TB("Double definition of the event {0-%s} {1-%s} for the producer {2-%s}"), strTagAction, strTagWhen, strProducer));
		return FALSE;
	}
	else
		pEMData->SetAt(strProducer.MakeLower(), pSubscriptionData);

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CSubscriptionInfo::Lookup(CString strTagNS, CDocumentSubscritpionData*& pDocSubscrData, BOOL bIsBONE)
{
	TB_LOCK_FOR_READ();
	if (bIsBONE)
		return m_pBONEList->Lookup(strTagNS.MakeLower(), pDocSubscrData);
	else
		return m_pSubscriptionList->Lookup(strTagNS.MakeLower(), pDocSubscrData);
}

//-----------------------------------------------------------------------------
void CSubscriptionInfo::SetAt(CString strTagNS, CDocumentSubscritpionData* pDocSubscrData, BOOL bIsBONE)
{
	TB_LOCK_FOR_WRITE();
	
	if (bIsBONE)
		m_pBONEList->SetAt(strTagNS.MakeLower(), pDocSubscrData);
	else
		m_pSubscriptionList->SetAt(strTagNS.MakeLower(), pDocSubscrData);
}
