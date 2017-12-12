#include "stdafx.h"

#include <atlimage.h>

#include <TbNameSolver\ThreadContext.h>
#include <TbNameSolver\PathFinder.h>
#include <TbNameSolver\TBNamespaces.h>

#include <TbClientCore\ClientObjects.h>

#include <TbGeneric\DataObj.h>
#include <TbGeneric\WebServiceStateObjects.h>
#include <TbGeneric\FontsTable.h>
#include <TbGeneric\FormatsTable.h>
#include <TbGeneric\ParametersSections.h>
#include <TbGeneric\SettingsTable.h>
#include <TbGeneric\GeneralFunctions.h>
#include <TbGeneric\FunctionObjectsInfo.h>
#include <TbGeneric\LocalizableObjs.h>
#include <TbGeneric\StatusBarMessages.h>
#include <TbGeneric\Pictures.h>
#include <TbGeneric\SettingsTable.h>
#include <TbGeneric\TBThemeManager.h>
#include <TbGeneric\JsonFormEngine.h>


#include <TbGenlib\PARSOBJ.H>
#include <TbGenlib\ParsCtrl.h>
#include <TbGenlib\Parsedt.h>
#include <TbGenlib\Messages.h>
#include <TbGenlib\ToolTipCtrl.h>
#include <TbGenlib\MicroareaVisualManager.h>
#include <TbGeneric\GeneralFunctions.h>

#include <TbParser\XmlFunctionObjectsParser.h>

#include <TbWebServicesWrappers\LoginManagerInterface.h>
#include <TbGes\ExtDocFrame.h>
#include <TbGes\tbges.hrc>
#include "oslbaseinterface.h"

#include "baseapp.h"

// Help entry
#include "basefrm.hjson" //JSON AUTOMATIC UPDATE

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"


#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

static const TCHAR* szTrue					= _T("true");
static const TCHAR* szDisableWindowCreation = _T("DisableWindowCreation");
 //=============================================================================
//				class CApplicationDateDialog definition
//=============================================================================
class CApplicationDateDialog : public CParsedDialog
{
	DECLARE_DYNAMIC (CApplicationDateDialog)

protected:
	DataDate	m_DataDate;
	CDateEdit	m_EditDate;

public:
	CApplicationDateDialog	(CWnd* = NULL);

protected:
	afx_msg virtual	BOOL	OnInitDialog	();
	afx_msg virtual	void	OnOK			();

	//{{AFX_MSG( CApplicationDateDialog )
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()
};

//=============================================================================
IMPLEMENT_DYNAMIC (CApplicationDateDialog, CParsedDialog)

BEGIN_MESSAGE_MAP(CApplicationDateDialog, CParsedDialog)
	//{{AFX_MSG_MAP( CApplicationDateDialog )
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//----------------------------------------------------------------------------
CApplicationDateDialog::CApplicationDateDialog(CWnd* pParent)
	:
	CParsedDialog	(IDD_APP_DATE, pParent, _NS_DLG("TbAppManager.ApplicationDate"))
{
	m_DataDate = DataDate
		(
			AfxGetApplicationDay(),
			AfxGetApplicationMonth(),
			AfxGetApplicationYear()
		);
}

//----------------------------------------------------------------------------
BOOL CApplicationDateDialog::OnInitDialog()
{
	CParsedDialog::OnInitDialog();

	m_EditDate.Attach (BTN_CALENDAR_ID);
	m_EditDate.SubclassEdit	(IDC_APP_DATE,	this);
	m_EditDate.SetValue (m_DataDate);

	return TRUE;
}

//----------------------------------------------------------------------------
void CApplicationDateDialog::OnOK()
{    
	if (!CParsedForm::CheckForm())
		return;
	m_EditDate.GetValue(m_DataDate);
	USES_UNATTENDED_DIAGNOSTIC();
	AfxSetApplicationDate(m_DataDate);
	if(AfxGetDiagnostic()->ErrorCodeFound(L"MLU-01"))
	{
		AfxMessageBox(AfxGetDiagnostic()->ToString());
		AfxGetDiagnostic()->ClearMessages();
	}
	
	EndDialog(IDOK);
}

//----------------------------------------------------------------------------
void ChangeOperationsDate()
{
	// OSL Codice di protezione
	CInfoOSL infoOSL (CTBNamespace(CTBNamespace::FUNCTION, _NS_WEB("Framework.TbGenlibUI.TbGenlibUI.SetApplicationDate")), OSLType_Function);

	AfxGetSecurityInterface()->GetObjectGrant (&infoOSL);
	if (! OSL_CAN_DO(&infoOSL, OSL_GRANT_EXECUTE))
	{
		AfxMessageBox(cwsprintf(OSLErrors::MISSING_GRANT(), *infoOSL.m_Namespace.ToString()));
		return; 
	}
	//----

	CApplicationDateDialog dlg(AfxGetMainWnd());
	dlg.DoModal();
}

//=============================================================================
//				class CThreadMessageCache definition
//=============================================================================
class CThreadMessageCache
{
public:
	DataObj*			m_pCtrlData = NULL;
	DataObj*			m_pOldCtrlData = NULL;
	CString				m_strError;
	CString				m_strWarning;
	BOOL				m_bNoCancOnWarning = FALSE;		// non visualizza il bottone di Cancel sui messaggi di Warning

	BOOL DataChanged() {
		return m_pOldCtrlData != NULL && !m_pOldCtrlData->IsEqual(*m_pCtrlData);
	}
};

DECLARE_THREAD_VARIABLE(CThreadMessageCache, aThreadMessageCache);



//=============================================================================
// Useful function for AddOn-Application management
//=============================================================================

//----------------------------------------------------------------------------
AddOnModule* AFXAPI AfxGetAddOnModule(CString strFilePath)
{
	strFilePath.MakeLower();
	// la prima applicazione è TB e poi ci sono le AddOnApplications
	AddOnApplication* pAddOnApp;
	for (int nApp = 0; nApp <= AfxGetAddOnAppsTable()->GetUpperBound(); nApp++)
	{
		pAddOnApp = AfxGetAddOnAppsTable()->GetAt(nApp);
		ASSERT(pAddOnApp);

		for (int nMod = 0; nMod <= pAddOnApp->m_pAddOnModules->GetUpperBound(); nMod++)
		{
			AddOnModule* pAddOnMod = pAddOnApp->m_pAddOnModules->GetAt(nMod);
			ASSERT(pAddOnMod);
			CString s = pAddOnMod->m_sModulePath;
			s.MakeLower();
			if  (strFilePath.Find(s) == 0)
				return pAddOnMod;
		}
	}

	return NULL;
}
//----------------------------------------------------------------------------
void AFXAPI AfxGetAddOnModules(HINSTANCE hDllInstance, AddOnModsArray &arModules)
{
	if (!hDllInstance) return;

	arModules.SetOwns(FALSE);

	AddOnDll* pAddOnDll = AfxGetAddOnDll(hDllInstance);

	if (!pAddOnDll)
		return;

	for (int i = 0; i < pAddOnDll->GetAddOnLibraries().GetCount(); i++)
	{
		AddOnLibrary* pLib = pAddOnDll->GetAddOnLibraries()[i];
		BOOL exists = FALSE;
		for (int i = 0; i < arModules.GetCount(); i++)
			if (arModules.GetAt(i) == pLib->m_pAddOnModule)
			{
				exists = TRUE;
				break;
			}
			if (!exists)
				arModules.Add(pLib->m_pAddOnModule);
	}
}
//----------------------------------------------------------------------------
AddOnDll* AFXAPI AfxGetAddOnDll (HINSTANCE hDllInstance)

{
	if (!hDllInstance) return NULL;

	// la prima applicazione è TB e poi ci sono le AddOnApplications
	AddOnApplication* pAddOnApp;
	for (int nApp = 0; nApp <= AfxGetAddOnAppsTable()->GetUpperBound(); nApp++)
	{
		pAddOnApp = AfxGetAddOnAppsTable()->GetAt(nApp);
		ASSERT(pAddOnApp);

		AddOnDll* pDll = pAddOnApp->GetAddOnDll(hDllInstance);
		if (pDll)
			return pDll;
	}

	return NULL;
}
//----------------------------------------------------------------------------
AddOnModule* AFXAPI AfxGetAddOnModule (const CTBNamespace& aNamespace)
{
	CString sAppName = aNamespace.GetApplicationName();
	CString sModName = aNamespace.GetObjectName(CTBNamespace::MODULE);

	// la prima applicazione è TB e poi ci sono le AddOnApplications
	AddOnApplication* pAddOnApp = AfxGetAddOnApp(sAppName);
	if (!pAddOnApp)
		return NULL;

	for (int nMod = 0; nMod <= pAddOnApp->m_pAddOnModules->GetUpperBound(); nMod++)
	{
		AddOnModule* pAddOnMod = pAddOnApp->m_pAddOnModules->GetAt(nMod);
		ASSERT(pAddOnMod);
		if (_tcsicmp(pAddOnMod->GetModuleName(), sModName) == 0)
			return pAddOnMod;
	}

	return NULL;
}

//----------------------------------------------------------------------------
AddOnLibrary* AFXAPI AfxGetAddOnLibrary (const CTBNamespace& aNamespace)
{
	AddOnModule* pAddOnMod = AfxGetAddOnModule(aNamespace);
	if (!pAddOnMod)
		return NULL;

	return pAddOnMod->GetLibraryFromName(aNamespace.GetObjectName(CTBNamespace::LIBRARY));
}

//----------------------------------------------------------------------------
AddOnApplication* AFXAPI AfxGetAddOnApp	 (const CString& strAppName)
{
	if (strAppName.IsEmpty())
		return NULL;

	for (int i = 0; i <= AfxGetAddOnAppsTable()->GetUpperBound(); i++)
	{
		AddOnApplication* pAddOnApp = AfxGetAddOnAppsTable()->GetAt(i);
		ASSERT(pAddOnApp);

		if (strAppName.CollateNoCase(pAddOnApp->m_strAddOnAppName) == 0)
			return pAddOnApp;
	}

	return NULL;
}

//----------------------------------------------------------------------------
AddOnApplication* AFXAPI AfxGetAddOnAppFromSignature(LPCTSTR pszSignature)
{
	if (!pszSignature)
		return NULL;

	for (int i = 0; i <= AfxGetAddOnAppsTable()->GetUpperBound(); i++)
	{
		AddOnApplication* pAddOnApp = AfxGetAddOnAppsTable()->GetAt(i);
		ASSERT(pAddOnApp);

		if (!_tcsicmp(pAddOnApp->GetSignature(),pszSignature))
			return pAddOnApp;
	}

	return NULL;
}

//----------------------------------------------------------------------------
AddOnModule* AFXAPI AfxGetOwnerAddOnModule (CRuntimeClass* pObjectClass)
{
	for (int nApp = 0; nApp <= AfxGetAddOnAppsTable()->GetUpperBound(); nApp++)
	{
		AddOnApplication* pAddOnApp = AfxGetAddOnAppsTable()->GetAt(nApp);
		ASSERT(pAddOnApp);

		for (int nMod = 0; nMod <= pAddOnApp->m_pAddOnModules->GetUpperBound(); nMod++)
		{
			AddOnModule* pAddOnMod = pAddOnApp->m_pAddOnModules->GetAt(nMod);
			ASSERT(pAddOnMod);

			if (pAddOnMod->GetOwnerLibrary(pObjectClass))
				return pAddOnMod;
		}
	}

	return NULL;
}

//----------------------------------------------------------------------------
AddOnDll* AFXAPI AfxGetAddOnDll (const CTBNamespace& aDllNamespace)
{
	AddOnApplication* pAddOnApp = AfxGetAddOnApp(aDllNamespace.GetApplicationName());
	if (!pAddOnApp)
		return NULL;
	
	return pAddOnApp->GetAddOnDll(aDllNamespace);
}

//----------------------------------------------------------------------------
AddOnLibrary* AFXAPI AfxGetOwnerAddOnLibrary (const CTBNamespace* pNamespace)
{
	if (!pNamespace)
		return NULL;

	AddOnModule* pAddOnMod = AfxGetAddOnModule(*pNamespace);
	if (!pAddOnMod)
		return NULL;

	AddOnLibrary* pLibrary = pAddOnMod->GetOwnerLibrary(pNamespace);
	if (pLibrary)
		return pLibrary;

	return NULL;
}

// ritorna la libreria proprietaria dell'oggetto dal nome della classe
//----------------------------------------------------------------------------
AddOnLibrary* AFXAPI AfxGetOwnerAddOnLibrary (const CTBNamespace::NSObjectType aType, const CString& sRuntimeClassName)
{
	for (int nApp = 0; nApp <= AfxGetAddOnAppsTable()->GetUpperBound(); nApp++)
	{
		AddOnApplication* pAddOnApp = AfxGetAddOnAppsTable()->GetAt(nApp);
		ASSERT(pAddOnApp);

		for (int nMod = 0; nMod <= pAddOnApp->m_pAddOnModules->GetUpperBound(); nMod++)
		{
			AddOnModule* pAddOnMod = pAddOnApp->m_pAddOnModules->GetAt(nMod);
			ASSERT(pAddOnMod);

			AddOnLibrary* pLibrary = pAddOnMod->GetOwnerLibrary(aType, sRuntimeClassName);
			if (pLibrary)
				return pLibrary;
		}
	}
	return NULL;
}

//------------------------------------------------------------------------------
BOOL AFXAPI AfxIsCurrentlyInUnattendedMode() 
{ 
	return AfxIsInUnattendedMode() || AfxGetTbCmdManager()->IsDLLLoading();  
}


//-----------------------------------------------------------------------------
BOOL AFXAPI AfxThreadLockTraceEnabled()
{
	DataBool* aParam = (DataBool*) AfxGetSettingValue(CTBNamespace(szTbGenlibNamespace), szEnvironment, szEnableLockTrace, DataBool(FALSE));
	return aParam && *aParam;
}


//-----------------------------------------------------------------------------
BOOL AFXAPI AfxAPIHookingEnabled()
{
	DataBool* aParam = (DataBool*)AfxGetSettingValue(CTBNamespace(szTbGenlibNamespace), szEnvironment, szDisableWindowCreation, DataBool(FALSE));
	return aParam && *aParam;
}

//-----------------------------------------------------------------------------
BOOL AFXAPI AfxCenterControlsEnabled()
{
	DataBool* aParam = (DataBool*) AfxGetSettingValue(CTBNamespace(szTbGenlibNamespace), szFormsSection, szEnableCenterControls, DataBool(FALSE));
	return aParam && *aParam;
}

//-----------------------------------------------------------------------------
void AFXAPI AfxSetCenterControlsEnabled(BOOL bEnabled)
{
	AfxSetSettingValue(CTBNamespace(szTbGenlibNamespace), szFormsSection, szEnableCenterControls, DataBool(bEnabled));
}

//-----------------------------------------------------------------------------
BOOL AfxIsCustomObject(const CTBNamespace* pNs)
{
	AddOnModule* pMod = AfxGetAddOnModule(*pNs);
	return pMod ? pMod->m_bIsCustom : FALSE;
}

//------------------------------------------------------------------------------
void AFXAPI AfxGetClientDocs (const CTBNamespace& aServerNs, CObArray& arClientDocs)
{
	AddOnModule* pAddOnMod = AfxGetAddOnModule(aServerNs);

	if (!pAddOnMod)
		return;

	const CDocumentDescription* pDocInfo = AfxGetDocumentDescription(aServerNs);

	if (!pDocInfo)
		return;


	// i family potrebbero essere rappresentati da più di un when server
	for (int i=0; i <= AfxGetClientDocsTable()->GetUpperBound(); i++)
	{
		CServerDocDescription* pServerInfo = AfxGetClientDocsTable()->GetAt(i); 

		if (
				!pServerInfo ||
				// clientdoc non family
				(pServerInfo->GetNamespace() != aServerNs &&
				// clientdoc family
				(pDocInfo->IsExcludedFromFamily() || !pServerInfo->IsHierarchyOf(pDocInfo->GetClassHierarchy())))
			)
			continue;

		for (int n=0; n <= pServerInfo->GetClientDocs().GetUpperBound(); n++)
			arClientDocs.Add (pServerInfo->GetClientDocs().GetAt(n));
	}
}

// Ritorna la descrizione Xml di un oggetto di database sulla base del suo nome
//----------------------------------------------------------------------------
const CDbObjectDescription* AFXAPI AfxGetDbObjectDescription (const CString& sName)
{
	return	AfxGetDatabaseObjectsTable() ? 
			AfxGetDatabaseObjectsTable()->GetDescription(sName) :
			NULL;
		;
}

//----------------------------------------------------------------------------
CString FailedInvokeCodeDescription(FailedInvokeCode err)
{
	CString strDescr;

	switch(err)
	{
		case InvkNoError:
			break;

		case InvkOslForbidden:
			strDescr = OSLErrors::MISSING_GRANT();
			break;

		case InvkTooManyWindows:
			strDescr = _TB("There are too many documents open");
			break;

		case InvkApmCmdIdUnknown:
			strDescr = _TB("Menu command not found");
			break;

		case InvkApmCmdType:
			strDescr = _TB("Wrong menu command type\r\n");
			break;

		case InvkApmCmdInterface:
			strDescr = _TB("Wrong menu command interface");
			break;

		case InvkKindOfTemplate:
			strDescr = _TB("Wrong type of document");
			break;

		case InvkKindOfAdm:
			strDescr = _TB("Wrong ADM type");
			break;

		case InvkUnknownTemplate:
			strDescr = _TB("Unknown document");
			break;

		case InvkUnknownConstraint:
			strDescr = _TB("Unknown object\r\n");
			break;
		
		case InvkCfgForbidden:
			strDescr = _TB("The requested function is not available in the purchased configuration.");
			break;


		case InvkUnknownError:
			strDescr = _TB("General error\t\r\n");

			break;

		case InvkOperationDateError:
			strDescr = _TB("Invalid operation Date\t\r\n");

			break;
		default:;
	}

	return strDescr;
}

//-----------------------------------------------------------------------------
CBaseApp* AFXAPI AfxGetBaseApp()
{ 
	ASSERT(AfxGetApp()->IsKindOf(RUNTIME_CLASS(CBaseApp)));

	return (CBaseApp*)AfxGetApp(); 
}              

//-----------------------------------------------------------------------------
const AddOnAppsArray* AFXAPI AfxGetAddOnAppsTable() 
{ 
	return AfxGetApplicationContext()->GetObject<const AddOnAppsArray>(&CApplicationContext::GetAddOnApps);
}              


//-----------------------------------------------------------------------------
const CSingleExtDocTemplate* AFXAPI AfxGetTemplate(const CTBNamespace& aDocumentNameSpace)
{ 
	ASSERT((CBaseApp*)AfxGetBaseApp());
	return AfxGetBaseApp()->GetDocTemplate(aDocumentNameSpace); 
}

//-----------------------------------------------------------------------------
const CSingleExtDocTemplate* AFXAPI AfxGetTemplate(const CRuntimeClass* pViewClass, UINT nViewId, const CRuntimeClass* pDocClass /*NULL*/)
{ 
	ASSERT((CBaseApp*)AfxGetBaseApp());

	const CSingleExtDocTemplate* pTemplate = AfxGetBaseApp()->GetDocTemplate(pViewClass, nViewId, 0, pDocClass, TRUE);

	return (pTemplate && pTemplate->IsValid() ? pTemplate : NULL); 
}

//-----------------------------------------------------------------------------
CString AFXAPI AfxGetXEngineSiteCode()
{
	CTBNamespace aFunctionNamespace(_NS_WEB("Function.Extensions.XEngine.TbXmlTransfer.GetSiteCode"));

	if (!AfxIsActivated(TBEXT_APP, XENGINE_ACT))
		return _T("");

	CFunctionDescription aFunctionDescription;
	if (!AfxGetTbCmdManager()->GetFunctionDescription(aFunctionNamespace, aFunctionDescription))
	{
		ASSERT(FALSE);
		return _T("");
	}

	DataStr strSiteCode;
	if	(AfxGetTbCmdManager()->RunFunction(&aFunctionDescription, NULL))
	{
		strSiteCode = *((DataStr*)aFunctionDescription.GetReturnValue());
	}

	return strSiteCode;
}

// Poichè non esiste il concetto di applicazione nella gestione dell'attivazione,
// per conoscere se un'applicazione è attiva è necessario derivarlo dai moduli
// che la compongono.
//-----------------------------------------------------------------------------
BOOL AFXAPI AfxIsAppActivated (const CString& sApplication)
{
	AddOnApplication* pAddOnApp = AfxGetAddOnApp(sApplication);
	
	if (!pAddOnApp)
		return FALSE;

	AddOnModule* pAddOnMod; 
	for (int i=0; i <= pAddOnApp->m_pAddOnModules->GetUpperBound(); i++)
	{
		pAddOnMod = pAddOnApp->m_pAddOnModules->GetAt(i);
		if (AfxIsActivated(pAddOnMod->GetApplicationName(), pAddOnMod->GetModuleName()))
			return TRUE;
	}

	return FALSE;
}

////////////////////////////////////////////////////////////////////////////////
//			class CSingleExtDocTemplate
////////////////////////////////////////////////////////////////////////////////

//------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CSingleExtDocTemplate, CSingleDocTemplate)

// reimplementati costruttore e distruttore per gestire il caricamento di un solo 
// menu` a fronte dell'utilizzo dello stesso IDR_...
//
// ATTENZIONE: questa asserzione e` pesante e va capita profondamente.
// Possono esserci casi in cui questo non e` vero, allora create il template dalla
// classe CSingleDocTemplate standard di MFC (si perde pero` la gestione di CloseAll
// e di ricerca dei templates. Attenzione! CBaseApp vuole derivati da 
// CSingleExtDocTemplate.
//----------------------------------------------------------------------------
CSingleExtDocTemplate::CSingleExtDocTemplate
	(
		UINT nIDResource, CRuntimeClass* pDocClass,
		CRuntimeClass* pFrameClass, CRuntimeClass* pViewClass
	)
	: 
	CSingleDocTemplate
	(
	IDR_DUMMY_TEMPLATE/*uso questo perché il costruttore cerca di caricare la stringa ma se l'id del template è dinamico non la troverebbe*/,
	pDocClass,
	pFrameClass, 
	pViewClass),
	IOSLObjectManager(OSLType_Template),
	m_hDllInstance	(0),
	m_dwProtections	(0),
	m_bValid		(TRUE),
	m_pOriginalTemplate (NULL),
	m_pOpeningDoc	(NULL),
	m_pOpeningFrame	(NULL),
	m_pDocInvocationParams(NULL),
	m_nViewID (0),
	m_pJsonContext(NULL)
{
	//solo dopo che il costruttore base (quello che chiama la LoadTemplate e cerca la stringa) è stato chiamato, metto l'id reale
	m_nIDResource = nIDResource;
}
//----------------------------------------------------------------------------
void CSingleExtDocTemplate::LoadTemplate()
{
	//inibisco la chiamata al parent, non devo fare nulla, tutte le risorse sono json
}
//----------------------------------------------------------------------------
CSingleExtDocTemplate::CSingleExtDocTemplate(const CSingleExtDocTemplate* pTemplate, DocInvocationParams* pParams)
	:
	CSingleDocTemplate(
	IDR_DUMMY_TEMPLATE/*uso questo perché il costruttore cerca di caricare la stringa ma se l'id del template è dinamico non la troverebbe*/,
	pTemplate->GetDocClass(),
	pTemplate->GetFrameClass(),
	pTemplate->GetViewClass()
	),
	IOSLObjectManager(*(const_cast<CSingleExtDocTemplate*>(pTemplate)->GetInfoOSL())),
	m_hDllInstance(pTemplate->m_hDllInstance),
	m_dwProtections(pTemplate->m_dwProtections),
	m_bValid(pTemplate->m_bValid),
	m_sConfig(pTemplate->m_sConfig),
	m_sViewMode(pTemplate->m_sViewMode),
	m_pOriginalTemplate(pTemplate),
	m_pOpeningDoc(NULL),
	m_pOpeningFrame(NULL),
	m_pDocInvocationParams(NULL),
	m_nViewID(pTemplate->m_nViewID),
	m_pJsonContext(NULL)
{
	//solo dopo che il costruttore base (quello che chiama la LoadTemplate e cerca la stringa) è stato chiamato, metto l'id reale
	m_nIDResource = pTemplate->GetIDResource();
	if (pParams)
	{
		m_pDocInvocationParams = pParams;
		if (pParams->GetDocumentDescription())
			SetNamespace(pParams->GetDocumentDescription()->GetNamespace());
	}
}

//-----------------------------------------------------------------------------
void CSingleExtDocTemplate::AdjustMenu()
{
	CBaseApp *pBaseApp = AfxGetBaseApp();
	// sto aggiungendo il primo template con questo menu'
	if (pBaseApp->TemplateNo(m_nIDResource) == 0)
		return;

	// Esiste almeno un template con lo stesso IDR_...
	const CSingleExtDocTemplate* pExistingTemplate = pBaseApp->GetDocTemplate(m_nIDResource);

	// cancella il menu` appena caricato
	if (m_hMenuInPlace != NULL)
		::DestroyMenu(m_hMenuInPlace);


 	// usa il menu del primo template analogo
	ASSERT(pExistingTemplate);
	if (pExistingTemplate)
		m_hMenuInPlace = pExistingTemplate->m_hMenuInPlace;

}

// Setta a NULL il menu per evitare che il distruttore della classe padre lo cancelli.
// Solo l'ultimo template dello stesso tipo documento puo` distruggerlo.
// ATTENZIONE siamo gia` nei distruttori e quindi non possiamo chiamare AfxGetBaseApp()
// pena beccarsi un assert. Inoltre siccome siamo in piena distruzione della lista
// dei templates, MFC prima rimuove dalla lista e poi cancella l'elemento pertanto
// nel caso di un solo elemento con un particolare IDR_ lo stesso non viene contato
// dalla routine TemplateNo da qui il controllo non con 1 ma con 0
//-----------------------------------------------------------------------------
CSingleExtDocTemplate::~CSingleExtDocTemplate()
{
	int nTemplate = AfxGetBaseApp()->TemplateNo(m_nIDResource);
	
	if (m_hMenuInPlace != NULL && nTemplate >= 1)
		m_hMenuInPlace = NULL;
}

//-----------------------------------------------------------------------------
void CSingleExtDocTemplate::RemoveDocument(CDocument* pDoc)
{
	__super::RemoveDocument(pDoc);
	delete this;
}
	
//-----------------------------------------------------------------------------
BOOL CSingleExtDocTemplate::SameResourceID(UINT nIDResource) const
{
	return m_nIDResource == nIDResource;
}

//-----------------------------------------------------------------------------
int	CSingleExtDocTemplate::GetDocCount() const
{
	CWebServiceStateObjectsPtr pObjects = AfxGetWebServiceStateObjects(FALSE);
	CObArray arObjects;
	pObjects->GetObjects(RUNTIME_CLASS(CBaseDocument), arObjects);

	int nCount = 0;
	CBaseDocument* pDoc;
	for (int i = 0; i < arObjects.GetCount(); i++)
	{
		pDoc = dynamic_cast<CBaseDocument*>(arObjects[i]);
		if (pDoc && pDoc->m_pTemplate->GetNamespace() == const_cast<CSingleExtDocTemplate*>(this)->GetNamespace())
			nCount ++;
	}
	return nCount;
}

//-----------------------------------------------------------------------------
BOOL CSingleExtDocTemplate::OwnView(const CRuntimeClass* pViewClass) const
{ 
	ASSERT(pViewClass);

	// sono ammessi template con view NULL (vedi apmgr)	
	return 
		m_pViewClass && 
		m_pViewClass->m_lpszClassName == pViewClass->m_lpszClassName;
}

//-----------------------------------------------------------------------------
BOOL CSingleExtDocTemplate::OwnDocument(const CRuntimeClass* pDocClass, BOOL bIsDerived /*= FALSE*/) const
{ 
	if (!m_pDocClass)
		return pDocClass == NULL;
	
	return m_pDocClass->m_lpszClassName == pDocClass->m_lpszClassName ||
			(bIsDerived && m_pDocClass->IsDerivedFrom(pDocClass))     || 
			(bIsDerived && pDocClass->IsDerivedFrom(m_pDocClass)) ;
}

//-----------------------------------------------------------------------------
CDocument* CSingleExtDocTemplate::CreateNewDocument()
{
	m_pOpeningDoc = __super::CreateNewDocument();

	CBaseDocument* pDoc = dynamic_cast<CBaseDocument*>(m_pOpeningDoc);
	if (pDoc)
	{
		*pDoc->GetInfoOSL() = *GetInfoOSL();
		pDoc->m_pTemplate = this;
		if (pDoc->m_pDocInvocationParams)
			SAFE_DELETE(pDoc->m_pDocInvocationParams);
		pDoc->m_pDocInvocationParams = m_pDocInvocationParams;

		// parametri di invocation che partono dal codice managed (vd. EasyStudio)
		if (pDoc->GetManagedParameters())
			pDoc->GetManagedParameters()->CreateNewDocumentOf(pDoc);
	}
	return m_pOpeningDoc;
}

//-----------------------------------------------------------------------------
CFrameWnd* CSingleExtDocTemplate::CreateNewFrame(CDocument* pDoc, CFrameWnd* pOther)
{
	m_pOpeningFrame = __super::CreateNewFrame(pDoc, pOther);

	return m_pOpeningFrame;
}

//-----------------------------------------------------------------------------
void CSingleExtDocTemplate::InitialUpdateFrame(CFrameWnd* pFrame, CDocument* pDoc, BOOL bMakeVisible /*= TRUE*/)
{
	//if (AfxIsRemoteInterface())
	//	bMakeVisible = FALSE;

	CDockableFrame* pDockableFrame = (CDockableFrame*)(pFrame);
	if (pDockableFrame)
		pDockableFrame->SetFrameVisible(bMakeVisible==TRUE);
	
	__super::InitialUpdateFrame(pFrame, pDoc, bMakeVisible);

	HWND hwnd = pFrame->m_hWnd;//mi tengo da parte l'handle di finestra
	if (pDockableFrame)
	{
		pDockableFrame->OnFrameCreated();
	}
	
	//siccome la OnFrameCreated rilascia la coda dei messaggi, potrebbe nel frattempo essersi 
	//gia` chiusa la frame con tutto il documento

	if (!::IsWindow(hwnd))
		return;

	ASSERT_KINDOF(CBaseDocument, pDoc);
	CBaseDocument* pBaseDoc = (CBaseDocument*)pDoc;
	if (pBaseDoc->m_hFrameHandle == NULL) //prevents row view overrides
	{
		pBaseDoc->m_hFrameHandle = hwnd;
		pBaseDoc->OnFrameCreated();
	}		
	pBaseDoc->UpdateFrameCounts();//dopo la pDockableFrame->OnFrameCreated() la frame diventa visibile, devo richiamare la UpdateFrameCounts perché tiene conto delle frame visibili

	pBaseDoc->DispatchFrameCreated();

	if (pDockableFrame)
		pDockableFrame->m_bAfterOnFrameCreated = TRUE;
}

CCriticalSection g_OpenDocumentSection;
//-----------------------------------------------------------------------------
CDocument* CSingleExtDocTemplate::OpenDocumentFile
	( 
		LPCTSTR lpszPathName, 
		BOOL bMakeVisible /*= TRUE*/ 
	) 
{
	//non posso lanciare contemporaneamente due opendocumentfile per problemi legati al multithreading
	AfxGetThreadContext()->AllowDockableFrame(bMakeVisible==TRUE);

	// OSL Codice di protezione
	CInfoOSL infoOSL (GetInfoOSL()->m_Namespace, GetInfoOSL()->GetType());

	AfxGetSecurityInterface()->GetObjectGrant (&infoOSL);
	if ( 
		! OSL_CAN_DO(&infoOSL, OSL_GRANT_EXECUTE) 
		&&
		! OSL_CAN_DO(&infoOSL, OSL_GRANT_SILENTEXECUTE)
			)
	{
		AfxMessageBox(cwsprintf(OSLErrors::MISSING_GRANT(), GetInfoOSL()->m_Namespace.ToString()));
		return NULL; 
	}

	/////////////////////////////////////////////////////////////////////////
	LPCTSTR lpszDocInfo = lpszPathName ?
		(LPCTSTR) ((DocInvocationParams*) lpszPathName)->m_pDocInfo
		: NULL;

	CSingleLock l (&g_OpenDocumentSection, TRUE);
	CDocument* pDoc = CSingleDocTemplate::OpenDocumentFile(lpszDocInfo, bMakeVisible);

	// aggiungo il puntatore alla lista degli oggetti accessibili
	// dalle funzioni esterne chiamate via SOAP (la rimozione viene fatta dalla OnCloseDocument del documento)
	if (pDoc)
	{
		AfxAddWebServiceStateObject(pDoc);
		AfxGetThreadContext()->AddDocument(pDoc);
	}

	return pDoc;
}




///////////////////////////////////////////////////////////////////////////////
//			class CBaseApp implementation
///////////////////////////////////////////////////////////////////////////////

//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CBaseApp, CBCGPWinApp)

//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CBaseApp, CBCGPWinApp)

	ON_COMMAND(ID_WINDOW_CLOSE_ALL, 			OnCloseAll)

	ON_UPDATE_COMMAND_UI(ID_FILE_PRINT_SETUP,		OnUpdatePrintSetup)

	// Standard print setup command support
	ON_COMMAND(ID_FILE_PRINT_SETUP, __super::OnFilePrintSetup)

END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CBaseApp::CBaseApp()
{
}

//------------------------------------------------------------------------------
CBaseApp::~CBaseApp ()
{
}
//---------------------------------------------------
BOOL CBaseApp::InitInstance ()	
{
	SetAfxIsCurrentlyInUnattendedModePtr(&AfxIsCurrentlyInUnattendedMode);
	
	BOOL bOk = CBCGPWinApp::InitInstance();

	// ---- BCG Theme ----
	SetRegistryKey(_T("Microarea\\MagoNet"));

	CBCGPBaseControlBar::m_bMultiThreaded = TRUE;
	CBCGPToolBarImages::m_bMultiThreaded = TRUE;

	// Clear BCG key in windows register
	this->m_bSaveState = FALSE;
	CleanState();

	((CBCGPWinApp*) AfxGetApp())->SetVisualTheme(CBCGPWinApp::BCGP_VISUAL_THEME_CUSTOM);
	CBCGPVisualManager::SetDefaultManager(RUNTIME_CLASS(CMicroareaVisualManager));

	CBCGPToolTipParams toolTipParamsBalloon;
	toolTipParamsBalloon.m_bVislManagerTheme = TRUE;

	GetTooltipManager()->SetTooltipParams(BCGP_TOOLTIP_TYPE_TOOLBAR, RUNTIME_CLASS (CTBToolTipCtrl), &toolTipParamsBalloon);

	//--------------
	return bOk;
}
//---------------------------------------------------
int CBaseApp::ExitInstance ()	
{
	AfxGetThreadContext()->PerformExitinstanceOperations();

	int nRet = __super::ExitInstance ();

	BCGCBProCleanUp();

	// free doc manager
	if (m_pDocManager != NULL)
	{
		delete m_pDocManager;
		m_pDocManager = NULL;
	}

	return nRet;
}

//---------------------------------------------------
CString CBaseApp::GetAppTitle() const	
{
	return AfxGetApplicationContext()->GetAppTitle(); 
}

//-----------------------------------------------------------------------------
AddOnApplication* CBaseApp::GetTaskBuilderAddOnApp ()	const 
{ 
	return AfxGetAddOnApp(szTaskBuilderApp);
}

//-----------------------------------------------------------------------------
AddOnApplication* CBaseApp::GetMasterAddOnApp()	const
{
	return AfxGetAddOnAppsTable()->GetMasterAddOnApp(); 
}

//-----------------------------------------------------------------------------
AddOnApplication* CBaseApp::GetExtensionsAddOnApp () const
{
	return AfxGetAddOnApp(szExtensionsApp);
}
//-----------------------------------------------------------------------------
BOOL CBaseApp::IsInUnattendedMode()const	
{
	return AfxGetApplicationContext()->IsInUnattendedMode(); 
}

//-----------------------------------------------------------------------------
BOOL CBaseApp::CanUseReportEditor () 
{
	return AfxGetApplicationContext()->CanUseReportEditor(); 
} 

// Controllo delle risorse di sistema necessarie alla applicazione
//-----------------------------------------------------------------------------
BOOL CBaseApp::ResourceAvailable()
{
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CBaseApp::OnIdle(LONG lCount)
{
	AfxGetThreadContext()->ClearOldObjects();

	BOOL bContinueIdle = AfxGetThreadContext()->OnIdle(lCount);

	return __super::OnIdle(lCount) || bContinueIdle;
}

//-----------------------------------------------------------------------------
BOOL CBaseApp::OtherDocumentPresent(BOOL bDoDiagnostic /*= FALSE*/) const
{
	BOOL bOpenDocs = GetNrOpenDocuments() > 0;
	BOOL bOtherDocs = bOpenDocs || AfxGetLoginContext()->GetOpenDialogs() > 0;
	if (bOtherDocs && bDoDiagnostic)
		AfxMessageBox(_TB("The application cannot be closed.\r\nClose all the open documents."));
		
	return bOtherDocs;
}

//-----------------------------------------------------------------------------
int CBaseApp::GetNrOpenDocuments () const
{
	LongArray handles;
	AfxGetWebServiceStateObjectsHandles(handles, RUNTIME_CLASS(CBaseDocument));
	return handles.GetCount();
}


                                                  
/////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////
// Already implemented in APPDLG.CPP servono solo per derivare un nuovo template
/////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////

//------------------------------------------------------------------------------
void CBaseApp::OnCloseAll()
{
	DoCloseAll();
}

//------------------------------------------------------------------------------
BOOL CBaseApp::DoCloseAll()
{
	CLoginContext* pContext = AfxGetLoginContext();
	return pContext ? pContext->CloseAllDocuments() : FALSE;
}

//------------------------------------------------------------------------------
void CBaseApp::InvalidateTemplates()
{
	TB_LOCK_FOR_WRITE();
	POSITION pos = GET_FIRST_DOC_TEMPLATE_POSITION();

	while (pos != NULL)
	{
		CSingleExtDocTemplate* pTemplate = GET_NEXT_DOC_TEMPLATE(pos);
		if (!pTemplate || !pTemplate->IsKindOf(RUNTIME_CLASS(CSingleExtDocTemplate)))
			continue;
		
		pTemplate->SetValid(FALSE);
	}
}

//------------------------------------------------------------------------------
void CBaseApp::ValidateTemplates(const CTBNamespace& aModuleNs)
{
	TB_LOCK_FOR_WRITE();
	POSITION pos = GET_FIRST_DOC_TEMPLATE_POSITION();

	while (pos != NULL)
	{
		CSingleExtDocTemplate* pTemplate = GET_NEXT_DOC_TEMPLATE(pos);
		if (!pTemplate || !pTemplate->IsKindOf(RUNTIME_CLASS(CSingleExtDocTemplate)))
			continue;

		const CTBNamespace* pNs = &(pTemplate->GetNamespace());
		
		if	(
				_tcsicmp(pNs->GetApplicationName(), aModuleNs.GetApplicationName()) == 0 &&
				_tcsicmp(pNs->GetObjectName(CTBNamespace::MODULE), aModuleNs.GetObjectName(CTBNamespace::MODULE)) == 0
			)
			pTemplate->SetValid(TRUE);
	}
}

//------------------------------------------------------------------------------
const CSingleExtDocTemplate* CBaseApp::GetDocTemplate
	(
		const	CRuntimeClass*	pViewClass, 
				UINT			nViewId, 
				UINT			nFrameId,
		const	CRuntimeClass*	pDocClass,			/*= NULL*/
				BOOL			bIsDerived,			/*FALSE*/
				BOOL			bCheckMasterFrame	/*FALSE*/
	) 
	const
{
	TB_LOCK_FOR_READ();
	POSITION pos = GET_FIRST_DOC_TEMPLATE_POSITION();

	while (pos != NULL)
	{
		CSingleExtDocTemplate* pTemplate = GET_NEXT_DOC_TEMPLATE(pos);
		if (!pTemplate)
			continue;

		if (!pTemplate->IsKindOf(RUNTIME_CLASS(CSingleExtDocTemplate)))
			continue;
		
		BOOL bSameDoc  = !pDocClass || pTemplate->OwnDocument(pDocClass, bIsDerived);
		BOOL bSameView = !pViewClass || pTemplate->OwnView(pViewClass);
		
		//se non vengono specificati gli id, è sufficiente il test sulla runtime class
		BOOL bSameId = TRUE;
		//se il template specifica un id, allora è una vista json con view sempre uguale, 
		//quindi devo matchare anche l'id
		if (pTemplate->m_nViewID)
			bSameId = pTemplate->m_nViewID == nViewId;
		//se il template non ha un view id, ma mi viene passato comunque un id, allora si tratta di frame id, 
		//quindi deve matchare con l'IDResource del template (id della frame)
		else if (nFrameId)
			bSameId = pTemplate->GetIDResource() == nFrameId;

		if (bSameDoc && bSameView && bSameId && pTemplate->IsValid())
			if (!bCheckMasterFrame || AfxGetBaseApp()->IsMasterFrame(pTemplate->GetFrameClass()))
				return pTemplate;
	}

	return NULL;
}

//----------------------------------------------------------------------------
const CSingleExtDocTemplate* CBaseApp::GetDocTemplate(UINT nIDResource) const
{                                                     
	TB_LOCK_FOR_READ();

	POSITION pos = GET_FIRST_DOC_TEMPLATE_POSITION();

	while (pos != NULL)
	{
		CSingleExtDocTemplate* pTemplate = GET_NEXT_DOC_TEMPLATE(pos);
		if (!pTemplate)
			continue;

		if (!pTemplate->IsKindOf(RUNTIME_CLASS(CSingleExtDocTemplate)))
			continue;

		// ATTENZIONE si ferma al primo template che ha l'IDR_... cercato.
		if (pTemplate->SameResourceID(nIDResource) && pTemplate->IsValid())
			return pTemplate;
	}

	ASSERT(FALSE);
	TRACE1 ("No template of resource %d found.\n",  nIDResource);
	return NULL;
}

//----------------------------------------------------------------------------
const CSingleExtDocTemplate* CBaseApp::GetDocTemplate(const CString& sDocumentName, const CString& sViewMode)
{           
	CSingleExtDocTemplate* pFoundTemplate = NULL;
	CString strKey = cwsprintf(_T("%s - %s"), sDocumentName, sViewMode);
	
	BEGIN_TB_LOCK_FOR_READ();

	if (m_TemplateCache.Lookup(strKey, pFoundTemplate))
		return pFoundTemplate;

	POSITION pos = GET_FIRST_DOC_TEMPLATE_POSITION();

	while (pos != NULL)
	{
		CSingleExtDocTemplate* pTemplate = GET_NEXT_DOC_TEMPLATE(pos);
		if (!pTemplate)
			continue;

		if (!pTemplate->IsKindOf(RUNTIME_CLASS(CSingleExtDocTemplate)))
			continue;
		
		if (
				pTemplate->IsValid() &&
				_tcsicmp(pTemplate->GetNamespace().ToString(), sDocumentName) == 0  && 
				(sViewMode == szNoInterface || _tcsicmp(pTemplate->m_sViewMode, sViewMode) == 0)			
			) 
		{
			pFoundTemplate = pTemplate;
			break;
		}
	}

	END_TB_LOCK_FOR_READ();

	if (pFoundTemplate == NULL)
	{
		TRACE1 ("No template of document %s found.\n",  (LPCTSTR) sDocumentName);
	}
	TB_OBJECT_LOCK(&m_TemplateCache);
	m_TemplateCache.SetAt(strKey, pFoundTemplate);
	return pFoundTemplate;
}

//----------------------------------------------------------------------------
const CSingleExtDocTemplate* CBaseApp::GetDocTemplate(const CTBNamespace& aDocumentNameSpace)
{
	return GetDocTemplate(aDocumentNameSpace.ToString(), szDefaultViewMode);
}

//----------------------------------------------------------------------------
int CBaseApp::TemplateNo(UINT nIDResource) const
{                                                     
	TB_LOCK_FOR_READ();
	int nTemplateNo = 0;
	POSITION pos = GET_FIRST_DOC_TEMPLATE_POSITION();

	while (pos != NULL)
	{
		CSingleExtDocTemplate* pTemplate = GET_NEXT_DOC_TEMPLATE(pos);
		if (!pTemplate)
			continue;

		if (!pTemplate->IsKindOf(RUNTIME_CLASS(CSingleExtDocTemplate)))
			continue;
		
		if (pTemplate->SameResourceID(nIDResource))
			nTemplateNo++;
	}
	return nTemplateNo;
}

//----------------------------------------------------------------------------------
CSingleExtDocTemplate* CBaseApp::RegisterTemplate
	(
		UINT				nIDResource,
		CRuntimeClass*		pDocClass,
		CRuntimeClass*		pFrameClass,
		CRuntimeClass*		pViewClass,
		UINT				nViewId,
		UINT				nFrameId,
		CTBNamespace&		sDocumentNamespace,
		AddOnInterfaceObj*	pParent/* = NULL*/,
		CString				sConfig,/*NULL*/
		DWORD				dwProtections /*TPL_NO_PROTECTION*/,
		CString				sViewMode /*szDefaultViewMode*/
	)
{
	TB_LOCK_FOR_WRITE();
	// se il template e' gia` registrato allora evita la riregistrazione
	// Nota: a causa della protezione della lista nel DocManager non e` possibile
	// eliminare un template gia` registrato, per cui non rimane altro che
	// lasciarlo nella lista ma non registrarlo due volte
	CSingleExtDocTemplate* pTemplate = const_cast<CSingleExtDocTemplate*>(GetDocTemplate (pViewClass, nViewId, nFrameId, pDocClass));
	if (pTemplate == NULL)
	{
		pTemplate = new CSingleExtDocTemplate
			(
				nIDResource,
				pDocClass,
				pFrameClass,
				pViewClass
			);
		AddDocTemplate (pTemplate);
		if (pParent) 
			pParent->m_arDocTemplates.Add(pTemplate);
	}
	else 
	{
		// template doppio
		if (pTemplate->IsValid())
		{
			CString sNs = sDocumentNamespace.ToString() + _T(": ") + sViewMode;
			ASSERT(FALSE);
			TRACE1("Duplicate template registration call for namespace %s .", (LPCTSTR) sNs);
		}
		else
			pTemplate->SetValid(TRUE);
	}

	// se esistono carico i dati dalla descrizione xml
	const CDocumentDescription* pXmlDocInfo = NULL;
	if (pParent)
	{
		AddOnModule* pAddOnMod = AfxGetAddOnModule(sDocumentNamespace);
		if (pAddOnMod)
			pXmlDocInfo = AfxGetDocumentDescription(sDocumentNamespace);
	}

	if (!pXmlDocInfo && sDocumentNamespace.GetType() == CTBNamespace::DOCUMENT)
	{
		AfxGetDiagnostic()->Add (cwsprintf(_TB("The Xml description for the document %s is missing. It should be declared into the DocumentObjects.xml file in the ModuleObjects directory."), sDocumentNamespace.ToString()), CDiagnostic::Warning);
		AfxGetDiagnostic()->Add (cwsprintf(_TB("The document will be available with the following behaviour:"), sDocumentNamespace), CDiagnostic::Warning);
		AfxGetDiagnostic()->Add (cwsprintf(_TB("- the client documents libraries loading will be disabled;"), sDocumentNamespace), CDiagnostic::Warning);
		AfxGetDiagnostic()->Add (cwsprintf(_TB("- the \"batch document\" attribute could be unavailable;"), sDocumentNamespace), CDiagnostic::Warning);
		AfxGetDiagnostic()->Add (cwsprintf(_TB("- the \"unattended mode\" attribute could be unavailable;"), sDocumentNamespace), CDiagnostic::Warning);
		AfxGetDiagnostic()->Add (cwsprintf(_TB("Please, close the application and correct the problem."), sDocumentNamespace), CDiagnostic::Warning);
		AfxGetDiagnostic()->Add (_T("") , CDiagnostic::Warning);
	}

	CViewModeDescription* pViewModeInfo = pXmlDocInfo ? pXmlDocInfo->GetViewMode(sViewMode) : NULL;

	//pTemplate->GetInfoOSL().m_pParent = pParent ? pParent->GetInfoOSL() : NULL;
	pTemplate->SetNamespace(sDocumentNamespace);

	pTemplate->m_sConfig	= sConfig;
	pTemplate->m_sViewMode	= sViewMode;	
	pTemplate->m_dwProtections = dwProtections;

	return pTemplate;
}	

//----------------------------------------------------------------------------
BOOL CBaseApp::CanCloseApplication (BOOL bWithMsgBox /*FALSE*/)
{
	return AfxGetApplicationContext()->CanClose();
}

//-----------------------------------------------------------------------------
void CBaseApp::OnUpdatePrintSetup (CCmdUI* pCmdUI) 
{
	DataBool* aParam = (DataBool*) AfxGetSettingValue(snsTbGenlib, szEnvironment, szShowPrintSetup, DataBool(FALSE), szTbDefaultSettingFileName);
	if (!aParam || *aParam == FALSE)
	{
		if (pCmdUI->m_pMenu)
			pCmdUI->m_pMenu->DeleteMenu(pCmdUI->m_nID, MF_BYCOMMAND);
		else
			pCmdUI->SetText(_T(""));
	}
}

//-----------------------------------------------------------------------------
void CBaseApp::AttachClientDocs(CAbstractFormDoc* pServerDoc, CClientDocArray* pArray)
{
	AfxGetAddOnAppsTable()->AttachClientDocs(pServerDoc, pArray);
	AfxGetAddOnAppsTable()->AttachScriptClientDocs(pServerDoc);
}

//-----------------------------------------------------------------------------
void CBaseApp::AttachFamilyClientDocs(CAbstractFormDoc* pServerDoc, CClientDocArray* pArray)
{
	AfxGetAddOnAppsTable()->AttachFamilyClientDocs(pServerDoc, pArray);
}

//-----------------------------------------------------------------------------
int CBaseApp::DoMessageBox(LPCTSTR lpszPrompt, UINT nType, UINT nIDPrompt)
{
	return AfxTBMessageBox(lpszPrompt, nType, nIDPrompt);
}

// verifica la tipologia di attivazione e se esiste un utente connesso deve avere il flag di sviluppatore TBS 
// (abilitabile in conosole solo se è attivata la funzionalità EasyBuilderDesigner)
//-----------------------------------------------------------------------------
BOOL CBaseApp::IsDevelopment() const
{ 
	return	AfxGetLoginManager()->IsActivationForDevelopment() && 
			(AfxGetLoginInfos() == NULL || AfxGetLoginInfos()->m_bEasyBuilderDeveloper );
}

//------------------------------------------------------------------------------
BOOL CBaseApp::IsNoCancOnWarning() const	
{
	GET_THREAD_VARIABLE(CThreadMessageCache, aThreadMessageCache);
	return aThreadMessageCache.m_bNoCancOnWarning; 
}

//------------------------------------------------------------------------------
const CString& CBaseApp::GetError() const	
{
	GET_THREAD_VARIABLE(CThreadMessageCache, aThreadMessageCache);
	return aThreadMessageCache.m_strError; 
}	

//------------------------------------------------------------------------------
const CString& CBaseApp::GetWarning() const	
{
	GET_THREAD_VARIABLE(CThreadMessageCache, aThreadMessageCache);
	return aThreadMessageCache.m_strWarning; 
}	

//------------------------------------------------------------------------------
const DataObj& CBaseApp::GetChangingCtrlData() const
{
	GET_THREAD_VARIABLE(CThreadMessageCache, aThreadMessageCache);
	return *aThreadMessageCache.m_pCtrlData; 
}
//------------------------------------------------------------------------------
const DataObj& CBaseApp::GetOldCtrlData() const
{
	GET_THREAD_VARIABLE(CThreadMessageCache, aThreadMessageCache);
	return *aThreadMessageCache.m_pOldCtrlData;
}
//------------------------------------------------------------------------------
BOOL CBaseApp::SetOldCtrlData (DataObj* pData, DataObj* pOldData)	
{
	GET_THREAD_VARIABLE(CThreadMessageCache, aThreadMessageCache);
	aThreadMessageCache.m_pCtrlData = pData;
	aThreadMessageCache.m_pOldCtrlData = pOldData; 
	return TRUE;
	//il valore di ritorno stabilisce se mandare o meno l'EN_VALUE_CHANGED
	//sarebbe bello mandarlo solo se il dato è cambiato, ma purtroppo il codice gestionale considera questo messaggio
	//alla stregua di una focus lost piuttosto che di value changed
	//soluzione di compromesso: spetta al codice gestionale, nel metodo che intercetta il messaggio, testare 
	// se il dato è cambiato usando AfxGetBaseApp()->IsCtrlDataChanged()
	//return aThreadMessageCache.m_bDataChanged;
	
}
//------------------------------------------------------------------------------
BOOL CBaseApp::IsValidOldCtrlData	()				
{
	GET_THREAD_VARIABLE(CThreadMessageCache, aThreadMessageCache);
	return aThreadMessageCache.m_pOldCtrlData != NULL; 
}
//------------------------------------------------------------------------------
BOOL CBaseApp::IsCtrlDataChanged()
{
	GET_THREAD_VARIABLE(CThreadMessageCache, aThreadMessageCache);
	return aThreadMessageCache.DataChanged();
}
//-----------------------------------------------------------------------------
void CBaseApp::SetError (const CString& strMess)
{
	GET_THREAD_VARIABLE(CThreadMessageCache, aThreadMessageCache);
	aThreadMessageCache.m_strError = strMess;
}

//-----------------------------------------------------------------------------
void CBaseApp::SetWarning(const CString& strMess, BOOL bNoCanc /*= FALSE*/)
{
	GET_THREAD_VARIABLE(CThreadMessageCache, aThreadMessageCache);
	aThreadMessageCache.m_strWarning = strMess;
	aThreadMessageCache.m_bNoCancOnWarning = bNoCanc;
}

//--------------------------------------------------------------------------------
CDocument* CBaseApp::OpenDocumentOnCurrentThread (
	const CSingleDocTemplate* pTemplate, 
	LPCTSTR pInfo, 
	BOOL bMakeVisible /* = TRUE*/)
{
	ASSERT_KINDOF(CSingleExtDocTemplate, pTemplate);
	CSingleExtDocTemplate *pNewTemplate = new CSingleExtDocTemplate((CSingleExtDocTemplate*)pTemplate, (DocInvocationParams*)pInfo);
	return pNewTemplate->OpenDocumentFile(pInfo, bMakeVisible);//il documento farà poi la delete
}
//=============================================================================
CTbCommandInterface* AFXAPI AfxGetTbCmdManager()
{
	return AfxGetApplicationContext()->GetObject<CTbCommandInterface>(&CApplicationContext::GetCommandManager);
}

//-----------------------------------------------------------------------------
CDocument* AfxOpenDocumentOnCurrentThread(const CSingleDocTemplate* pTemplate, LPCTSTR pInfo, BOOL bMakeVisible /*= TRUE*/)
{
	CTBWinThread* pThread = AfxGetTBThread();
	return pThread 
		? pThread->OpenDocumentOnCurrentThread(pTemplate, pInfo, bMakeVisible)	
		: AfxGetBaseApp()->OpenDocumentOnCurrentThread(pTemplate, pInfo, bMakeVisible);
}
