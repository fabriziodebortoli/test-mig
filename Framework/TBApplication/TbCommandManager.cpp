#include "stdafx.h"

#include <TbGeneric\Globals.h>
#include <TbGeneric\DataFileInfo.h>
#include <TbGeneric\webservicestateobjects.h>

#include <TbNameSolver\PathFinder.h>
#include <TbNameSolver\Templates.h>
#include <TbNameSolver\ThreadContext.h>
#include <TbNameSolver\LoginContext.h>
#include <TbNameSolver\IFileSystemManager.h>

#include <TbClientCore\ClientObjects.h>
#include <TbClientCore\GlobalFunctions.h>

#include <TbWebServicesWrappers\LoginManagerInterface.h>

#include <TbGenlib\Baseapp.h>
#include <TbGenlib\AddOnMng.h>
#include <TbGenlibUI\paddoc.h>
#include <TbGenlib\oslbaseinterface.h>
#include <TbGenlib\DiagnosticManager.h>
#include <TbGenlib\command.h>

#include <TbParser\XmlDataFileParser.h>

#include <TbOleDb\SqlRec.h>
#include <TbOleDb\SqlTable.h>
#include <TbOleDb\SqlAccessor.h>

#include <TbWoormEngine\ActionsRepEngin.h>

#include <TbWoormViewer\WoormDoc.h>
#include <TbWoormViewer\WoormFrm.h>
#include <TbWoormViewer\WoormVw.h>
#include <TbWoormViewer\WoormDoc.hjson> //JSON AUTOMATIC UPDATE

#include <TbGes\extdoc.h>
#include <TbGes\hotlink.h>
 
#include <TbRadar\WrmRdrVw.h>

#include "TaskBuilderApp.h"
#include "DocumentThread.h"
#include "LibrariesLoader.h"
#include "TbCommandManager.h"
#include "LoginThread.h"


#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif

static const TCHAR szXmlValueAttribute[]		= _T("value");
static const TCHAR szXmlBaseTypeAttribute[]		= _T("basetype");
static const TCHAR szXmlTypeAttribute[]			= _T("type");
static const TCHAR szXmlControlTag[]			= _T("ControlData");
static const TCHAR szXmlQueryTag[]				= _T("Query");
static const TCHAR szXmlParamTag[]				= _T("Param");
static const TCHAR szOnBeforeRunReport[]		= _T("OnBeforeRunReport");
static const TCHAR szOnBeforeRunDocument[]		= _T("OnBeforeRunDocument");
static const TCHAR szEnterpriseEdition[]		= _T("Enterprise");

/*//-----------------------------------------------------------------------------
class CNeedUpdateMenuBrowser
{
public:
	CNeedUpdateMenuBrowser()

	{
	}
	~CNeedUpdateMenuBrowser()
	{
		CLoginContext *pContext = AfxGetLoginContext();
		if (pContext)
			PushToClients(pContext->GetName(), _T("DocumentListUpdated"), _T(""));
	}
};*/



BEGIN_TB_STRING_MAP(CTbCommandManagerStrings)
	TB_LOCALIZED(ActivationDisabled, "The activation for the current product has been precautionary suspended, please send an email with the problem description to {0-%s}. You will be contacted as soon as possible.")
	TB_LOCALIZED(NoActivated,		 "The configuration requested for this function is disabled and/or current user is not allowed.")
	TB_LOCALIZED(NoUserLicense,		 "There are no CAL available for the selected document or report. Please contact application administrator.")
	TB_LOCALIZED(ActivationFailed,	 "The product could not authenticate the user license credentials through the proper Microarea Internet services.You are required to confirm your credentials by using the WebUpdater Admin command \"Register\"; an active Internet connection is required at the time of the confirmation. If further problems persists, preventing you to confirm your credentials, check if the Internet connection status is active and in case problems are still present send an email containing your information and a short issue description to {0-%s}. We will contact you as soon as possible.")
END_TB_STRING_MAP()

/////////////////////////////////////////////////////////////////////////////
//							CTbCommandManager
/////////////////////////////////////////////////////////////////////////////

//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CTbCommandManager, CTbCommandInterface)

//-----------------------------------------------------------------------------
CTbCommandManager::CTbCommandManager ()
 : m_pEBCommands(NULL)
{
}

//-----------------------------------------------------------------------------
CTbCommandManager::~CTbCommandManager()
{
	delete m_pEBCommands;
}

//-----------------------------------------------------------------------------
BOOL CTbCommandManager::IsDLLLoading ()
{
	return m_LibrariesLoader.IsDLLLoading();
}

//-----------------------------------------------------------------------------
const CSingleExtDocTemplate* CTbCommandManager::GetDocTemplate
	(
		LPCTSTR				pszDocNamespace, 
		FailedInvokeCode&	invokeCode,
		LPCTSTR				pszViewMode	 /*szDefaultViewMode*/,
		BOOL				bFromFunction /*FALSE*/,
		BOOL				bUseDiagnostic	/*TRUE*/
	)
{
	const CDocumentDescription* pXmlDocInfo = AfxGetDocumentDescription(CTBNamespace(CTBNamespace::DOCUMENT, pszDocNamespace));
	if (!pXmlDocInfo)
	{
		invokeCode = InvkUnknownTemplate;
		if (bUseDiagnostic)
			AfxGetDiagnostic()->Add(cwsprintf(_TB("DocumentObjects.xml description not found for the document {0-%s}"), pszDocNamespace), CDiagnostic::Error);
		return NULL;
	}

	return GetDocTemplate(pXmlDocInfo, invokeCode, pszViewMode, bFromFunction, bUseDiagnostic);
}

//-----------------------------------------------------------------------------
FailedInvokeCode CTbCommandManager::CanDoRunDocument
									(
										LPCTSTR		pszDocNamespace, 
										LPCTSTR		pszViewMode, 
										BOOL		bFromFunction,
										BOOL		bUseDiagnostic
									)
{
	FailedInvokeCode invokeCode;
	/*const CSingleExtDocTemplate* pTemplate =*/ 
		GetDocTemplate
				(
					pszDocNamespace, 
					invokeCode,
					pszViewMode,
					bFromFunction,
					bUseDiagnostic
				);
	return invokeCode;
}

//-----------------------------------------------------------------------------
BOOL CTbCommandManager::CanUseNamespace(CTBNamespace tbNamespace, OSLTypeObject oslT, DWORD grant, CDiagnostic* pDiagnostic, BOOL* pbProtected /*= NULL*/)
{
	CInfoOSL infoOsl (tbNamespace, oslT);
	AfxGetSecurityInterface()->GetObjectGrant (&infoOsl);
	if (!OSL_CAN_DO(&infoOsl, grant)) 
	{
		if (pbProtected) *pbProtected = OSL_IS_PROTECTED(&infoOsl);
		
		if (pDiagnostic)
			pDiagnostic->Add(_TB("User hasn't permission to execute document. Please contact application administrator.") + _T(".\r\n ") + tbNamespace.ToString(), CDiagnostic::Error);
		return FALSE;
	}
	if (pbProtected) *pbProtected = OSL_IS_PROTECTED(&infoOsl);

	//security light
	if (!AfxGetLoginContext()->CanUseNamespace(tbNamespace.ToString(), grant))
	{
		if (pDiagnostic)
			pDiagnostic->Add(_TB("User hasn't permission to execute document. Please contact application administrator.") + _T(".\r\n ") + tbNamespace.ToString(), CDiagnostic::Error);
		return FALSE;
	}
	
	return TRUE;
}

//-----------------------------------------------------------------------------
const CSingleExtDocTemplate* CTbCommandManager::GetDocTemplate
						(
							const CDocumentDescription*	pDocDescri, 
							FailedInvokeCode&			invokeCode,
							LPCTSTR						pszViewMode,
							BOOL						bFromFunction,
							BOOL						bUseDiagnostic
						)
{
	ASSERT(pDocDescri);
	if (!pDocDescri)
		return NULL;

	invokeCode = InvkNoError;
	const CSingleExtDocTemplate* pTemplate = NULL;


	CLoginContext* pLoginContext = AfxGetLoginContext();
	if(pLoginContext && !pLoginContext ->m_bMluValid)
	{
		invokeCode = InvkOperationDateError;
		AfxGetDiagnostic()->Add(cwsprintf( _TB( "Sadly your M.L.U. service is no longer active.\r\nThe operation date provided follows the M.L.U. expiry date: please select an operation date prior to the M.L.U. expiry date.\r\nRestore the full efficiency of you Mago.net!\r\nFind out how by visiting the website: {0-%s}") , AfxGetLoginManager()->GetBrandedKey(_T("MLURenewalShortLink")), CDiagnostic::Error));
		goto end;
	}



	const CTBNamespace *pNamespace = pDocDescri->GetTemplateNamespace() 
			? pDocDescri->GetTemplateNamespace()
			: &pDocDescri->GetNamespace();

	if (AfxIsRemoteInterface())
	{
		CViewModeDescription* pViewMode = pDocDescri->GetViewMode(pszViewMode);
		if (pViewMode && pViewMode->GetNoWeb())
		{
			AfxGetDiagnostic()->Add(cwsprintf(_TB("Document {0-%s} is not available in Web Mode"), pNamespace->ToString()), CDiagnostic::Info);
			invokeCode = InvkCfgForbidden;
			goto end;
		}
	}

	// non ho un namespace valido
	if (!pNamespace->IsValid() || pNamespace->GetType() != CTBNamespace::DOCUMENT)
	{
		if (bUseDiagnostic)
			AfxGetDiagnostic()->Add(_TB("Wrong document namespace.") + pNamespace->ToString(), CDiagnostic::Error);
		invokeCode = InvkUnknownTemplate;
		goto end;
	}
	
	// verifica e carica le DLL necessarie  (per i documenti dinamici è sufficiente caricare EasyBuilder
	if (!m_LibrariesLoader.LoadNeededLibraries(*pNamespace))
	{
		if (bUseDiagnostic)
			AfxGetDiagnostic()->Add(_TB("Document cannot be processed.") + pNamespace->ToString(), CDiagnostic::Error);
		invokeCode = InvkUnknownTemplate;
		goto end;
	}

	if (pDocDescri->IsDynamic())
	{
		pTemplate = AfxGetBaseApp()->GetDocTemplate
			(
				(_tcsicmp(pszViewMode, szBackgroundViewMode)) == 0 
					? RUNTIME_CLASS(ADMView) 
					: RUNTIME_CLASS(CDynamicFormView), 
				0,
				0,
				(pDocDescri->GetViewMode(pszViewMode) && pDocDescri->GetViewMode(pszViewMode)->GetType() == VMT_BATCH)
					? RUNTIME_CLASS(CDynamicBatchFormDoc)
					: RUNTIME_CLASS(CDynamicFormDoc)
			);
	}
	else
	{
		// non posso ottimizzare sui template perchè devo sempre verificare le DLL di dichirazione 
		// degli eventuali ClientDocs che possono non essere state caricate prima
		
		pTemplate = (CSingleExtDocTemplate*) AfxGetBaseApp()->GetDocTemplate(pNamespace->ToString(), pszViewMode);
		if (!pTemplate || !pTemplate->IsKindOf(RUNTIME_CLASS(CSingleExtDocTemplate)) )
		{
			if (bUseDiagnostic)
			{
				AfxGetDiagnostic()->Add(cwsprintf(_TB("Template that correspond to namespace '{0-%s}' does not exist."), pNamespace->ToString()), CDiagnostic::Error);
				AfxGetDiagnostic()->Add(_TB("Verify exactness of indicated namespace and right to module access."), CDiagnostic::Info);
			}
			invokeCode = InvkKindOfTemplate;
			goto end;
		}
	}
	// controlla le protezioni di template
	if	(
			((pTemplate->m_dwProtections & TPL_ADMIN_PROTECTION) == TPL_ADMIN_PROTECTION && !AfxGetLoginInfos()->m_bAdmin) ||
			((pTemplate->m_dwProtections & TPL_FUNCTION_PROTECTION) == TPL_FUNCTION_PROTECTION && !bFromFunction)
		)
	{	
		if (bUseDiagnostic)
		{
			AfxGetDiagnostic()->Add(_TB("The document is available only to the system administrator or is protected."), CDiagnostic::Error);
			AfxGetDiagnostic()->Add(cwsprintf(_TB("Reserved document has namespace '{0-%s}'."), pNamespace->ToString()), CDiagnostic::Error);
		}
		invokeCode = InvkCfgForbidden;
		goto end;
	}
	
	BOOL bIsBackground = IsBackground(pszViewMode);

	//Security check
	OSLTypeObject oslT = OSLType_Template;

	if (pDocDescri->GetViewMode(pszViewMode))
		switch (pDocDescri->GetViewMode(pszViewMode)->GetType())
		{
			case VMT_FINDER:
				oslT = OSLType_FinderDoc;
				break;
			case VMT_BATCH:
				oslT = OSLType_BatchTemplate;
				break;
			//default: oslT = OSLType_Template;
		}

	BOOL bProtected = FALSE;
	if (!CanUseNamespace
			(
				*pNamespace, 
				oslT, 
				(bIsBackground ? OSL_GRANT_SILENTEXECUTE : OSL_GRANT_EXECUTE),
				(bUseDiagnostic ? AfxGetDiagnostic() : NULL),
				&bProtected
			)
		)
	{
		invokeCode = InvkOslForbidden;
		goto end;
	}

	if	(
			((pTemplate->m_dwProtections & TPL_SECURITY_PROTECTION) == TPL_SECURITY_PROTECTION) &&
			!AfxGetLoginInfos()->m_bAdmin &&
			!bProtected	
		)
	{	
		if (bUseDiagnostic)
		{
			AfxGetDiagnostic()->Add(_TB("The document is available only to the system administrator or when it is protected by Security and user has grants"), CDiagnostic::Error);
			AfxGetDiagnostic()->Add(cwsprintf(_TB("Reserved document has namespace '{0-%s}'."), pNamespace->ToString()), CDiagnostic::Error);
		}
		invokeCode = InvkCfgForbidden;
		goto end;
	}

	// it burns only Enterprise CAL when: edition is enterprise, application is !Unattended, viewmode is !Unattended
	if (
			!bIsBackground &&
			!AfxIsInUnattendedMode() &&  
			_tcsicmp(AfxGetLoginManager()->GetEdition(), szEnterpriseEdition) == 0 &&
			!AfxIsCustomObject(pNamespace) && //gli oggetti di moduli custom non sono soggetti ad attivazione
			!AfxIsCalAvailable(pNamespace->GetApplicationName(), pNamespace->GetObjectName(CTBNamespace::MODULE))
		)
	{
		if (bUseDiagnostic)
		{
			CLoginManagerInterface::ActivationState as = AfxGetLoginManager()->GetActivationState();
			if (as == CLoginManagerInterface::Disabled)
					AfxGetDiagnostic()->Add(cwsprintf(CTbCommandManagerStrings::ActivationDisabled(), AfxGetLoginManager()->GetBrandedKey(_T("SuspendedMail"))), CDiagnostic::Error);
			else
				if (as == CLoginManagerInterface::NoActivated)
					AfxGetDiagnostic()->Add(cwsprintf(CTbCommandManagerStrings::ActivationFailed(), AfxGetLoginManager()->GetBrandedKey(_T("AuthenticationMail"))), CDiagnostic::Error);
				else
					AfxGetDiagnostic()->Add(CTbCommandManagerStrings::NoUserLicense(), CDiagnostic::Error);
		}
		invokeCode = InvkOslForbidden;
		goto end;
	}

	if	((!pDocDescri->IsDynamic() || pDocDescri->LiveInStandard()) && !AfxIsActivated(pNamespace->GetApplicationName(), pNamespace->GetObjectName(CTBNamespace::MODULE)))
	{	
		if (bUseDiagnostic)
		{
			CLoginManagerInterface::ActivationState as = AfxGetLoginManager()->GetActivationState();
			if (as == CLoginManagerInterface::Disabled)
				AfxGetDiagnostic()->Add(cwsprintf(CTbCommandManagerStrings::ActivationDisabled(), AfxGetLoginManager()->GetBrandedKey(_T("SuspendedMail"))), CDiagnostic::Error);
			else
				if (as == CLoginManagerInterface::NoActivated)
					AfxGetDiagnostic()->Add(cwsprintf(CTbCommandManagerStrings::ActivationFailed(), AfxGetLoginManager()->GetBrandedKey(_T("AuthenticationMail"))), CDiagnostic::Error);
				else
					AfxGetDiagnostic()->Add(CTbCommandManagerStrings::NoActivated(), CDiagnostic::Error);
		}
		invokeCode = InvkOslForbidden;
		goto end;
	}

	if	(
			!AfxIsInUnattendedMode() &&  
			_tcsicmp(AfxGetLoginManager()->GetEdition(), szEnterpriseEdition) == 0 &&
			!AfxIsCalAvailable(_T("ClientNet"), _NS_ACT("LicenzaUtente"))
		)
	{		
		if (bUseDiagnostic)
		{
			CLoginManagerInterface::ActivationState as = AfxGetLoginManager()->GetActivationState();
			if (as == CLoginManagerInterface::Disabled)
				AfxGetDiagnostic()->Add(cwsprintf(CTbCommandManagerStrings::ActivationDisabled(), AfxGetLoginManager()->GetBrandedKey(_T("SuspendedMail"))), CDiagnostic::Error);
			else
				if (as == CLoginManagerInterface::NoActivated)
					AfxGetDiagnostic()->Add(cwsprintf(CTbCommandManagerStrings::ActivationFailed(), AfxGetLoginManager()->GetBrandedKey(_T("AuthenticationMail"))), CDiagnostic::Error);
				else
					AfxGetDiagnostic()->Add(CTbCommandManagerStrings::NoUserLicense(), CDiagnostic::Error);
		}		
		invokeCode = InvkOslForbidden;
		goto end;
	}

end:
	return pTemplate;
}

//-----------------------------------------------------------------------------
BOOL CTbCommandManager::IsBackground(LPCTSTR pszViewMode)
{
	return _tcsicmp(pszViewMode, szBackgroundViewMode) == 0 || 
		_tcsicmp(pszViewMode, szUnattendedViewMode) == 0 || 
		_tcsicmp(pszViewMode, szNoInterface) == 0;
}
#include <TbGenlibUI\BrowserDlg.h>

//-----------------------------------------------------------------------------
CBaseDocument* CTbCommandManager::RunDocument 
	(
		LPCTSTR					pszDocNamespace, 
		LPCTSTR					pszViewMode		/*szDefaultViewMode*/, 
		BOOL					bFromFunction	/*FALSE*/,
		CBaseDocument*			pAncestor		/*NULL*/, 
		LPAUXINFO				lpAuxInfo		/*NULL*/, 
		CBaseDocument**			ppExistingDoc	/*NULL*/, 
		FailedInvokeCode*		pFailedCode	/*NULL*/,
		CExternalControllerInfo*pControllerInfo /*NULL*/,
		BOOL					bIsRunningAsADM /*FALSE*/,
		CTBContext*				pTBContext    /*NULL*/,
		CManagedDocComponentObj* pManagedParameters/*NULL*/,
		CContextBag*			 pContextBag  /*NULL*/
	)
{
	if (pAncestor && pAncestor->InvokeRequired())
	{
		return AfxInvokeThreadFunction
			<
			CBaseDocument*,
			CTbCommandManager,
			LPCTSTR,
			LPCTSTR,					
			BOOL,					
			CBaseDocument*,			
			LPAUXINFO,				
			CBaseDocument**,			
			FailedInvokeCode*,		
			CExternalControllerInfo*,	
			BOOL, 
			CTBContext*,	
			CManagedDocComponentObj*,
			CContextBag*
			>
			(
				pAncestor->GetThreadId(),
				this, 
				&CTbCommandManager::RunDocument,	
				pszDocNamespace, 
				pszViewMode,
				bFromFunction,
				pAncestor, 
				lpAuxInfo, 
				ppExistingDoc, 
				pFailedCode,
				pControllerInfo,
				bIsRunningAsADM,
				pTBContext,
				pManagedParameters,
				pContextBag
			);
	}

	USES_DIAGNOSTIC();
	//CNeedUpdateMenuBrowser needUpdate;

	// login context locked
	if (IsTBLocked())
	{
		if (pFailedCode) *pFailedCode = InvkLoginLocked;
		AfxGetDiagnostic()->Add(Strings::INVALID_COMPANY(), CDiagnostic::Error);
		return NULL;
	}

	// considero il tipo non obbligatorio
	CTBNamespace aNs(CTBNamespace::DOCUMENT, pszDocNamespace);
	
	if (!aNs.IsValid())
	{
		if (AfxGetBaseApp()->IsDevelopment())
			AfxGetDiagnostic()->Add(_TB("The namespace passed to RunDocument is not complete!") + pszDocNamespace, CDiagnostic::Error);
		else
			AfxGetDiagnostic()->Add(_TB("Unable to run document!") + aNs.ToString (), CDiagnostic::Error);

		if (pFailedCode) *pFailedCode = InvkUnknownTemplate;
		return NULL;
	}

	const CDocumentDescription* pXmlDocInfo = NULL;
	
	//non necessito di controllo sul module info
	pXmlDocInfo = AfxGetDocumentDescription(aNs);

	if (pControllerInfo && pControllerInfo->IsRunning() && pXmlDocInfo)
	{
		CViewModeDescription* pViewMode = pXmlDocInfo->GetViewMode(pszViewMode);
		if (pViewMode && !pViewMode->GetSchedulable())
		{
			AfxGetDiagnostic()->Add(cwsprintf(_TB("The document {0-%s} is not a schedulable object. It cannot be executed!"), pszDocNamespace), CDiagnostic::Error);
			if (pFailedCode) *pFailedCode = InvkCfgForbidden;
			return NULL;
		}
	}

	//serve controllo se siamo in stato di recording, avviso al macro recoder che c'è stata una rundocument
	CFunctionDescription fd;
	if (
			AfxIsActivated(TESTMANAGER_APP, _NS_ACT("TBMacroRecorder")) &&
			AfxGetApplicationContext()->m_MacroRecorderStatus != CApplicationContext::IDLE &&
			!IsBackground(pszViewMode)
		)
	{
		if (GetFunctionDescription(_NS_WEB("TestManager.TBMacroRecorder.TBMacroRecorder.RecordOpenDocument"), fd))
		{
			fd.SetParamValue(_T("nameSpace"), DataStr(aNs.ToString()));
			RunFunction(&fd, 0);
		}
	}
	
	FunctionDataInterface fdi;
	fdi.AddStrParam(_T("namespace"), aNs.ToString());
	DataStr message = _T("");
	fdi.AddOutParam(_T("message"), &message);
	fdi.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::Bool, CDataObjDescription::_OUT));
	fdi.SetReturnValue(DataBool(TRUE));
	FireEvent(szOnBeforeRunDocument, &fdi);
	DataBool* res = (DataBool*)fdi.GetReturnValue();
	if (!res || !(*res))
	{
		AfxGetDiagnostic()->Add(message.Str());
		return NULL;
	}

	// se deve usarne uno già esistente, lo riattiva
	if	(ppExistingDoc && *ppExistingDoc)
	{
	    (*ppExistingDoc)->Activate(NULL, TRUE);
		if (pFailedCode) *pFailedCode = InvkNoError;
		{
			return *ppExistingDoc;
		}
	}
	
	CBaseDocument* pDocument = NULL;
	DocInvocationParams* pParams = NULL;

	FailedInvokeCode aFailedCode;
	const CSingleExtDocTemplate* pTemplate = GetDocTemplate(aNs.ToString(), aFailedCode, pszViewMode, bFromFunction);
	if (aFailedCode != InvkNoError)
	{
		if (pFailedCode) *pFailedCode = aFailedCode;
		return NULL;
	}

	if (pTemplate == NULL || !pTemplate->IsValid())
	{
		AfxGetDiagnostic()->Add(_TB("Document not found."), CDiagnostic::Error);
		if (pFailedCode) *pFailedCode = InvkUnknownTemplate;
		return NULL;
	}

	// controlla che in caso di CFinderDoc, venga passato il pAuxInfo
	if (!lpAuxInfo)
	{		
		if (pXmlDocInfo && pXmlDocInfo->GetViewMode(pszViewMode) && pXmlDocInfo->GetViewMode(pszViewMode)->GetType() == VMT_FINDER)
		{
			AfxGetDiagnostic()->Add(_TB("Search document available only if linked to another document!"), CDiagnostic::Error);
			if (pFailedCode) *pFailedCode = InvkApmCmdInterface;
				return NULL;
		}
	}

	// istanzio il docinvocation e lo inizializzo con i parametri solo se necessario
	// il puntatore viene deletato dal documento che ne diventa il proprietario
	if (pAncestor || lpAuxInfo || pControllerInfo || pXmlDocInfo || bIsRunningAsADM || pTBContext || pManagedParameters || pContextBag) 
	{
		pParams = new DocInvocationParams ();
		if (pAncestor || lpAuxInfo || pControllerInfo || pTBContext || pContextBag)
			pParams->m_pDocInfo	= new DocInvocationInfo(pAncestor, pszDocNamespace, lpAuxInfo, pControllerInfo, pTBContext, pContextBag);
		pParams->m_pDocDescri		= pXmlDocInfo;
		pParams->m_bIsRunningAsADM	= bIsRunningAsADM;
		pParams->m_pManagedParameters = pManagedParameters;
	}


	
	// eccezione generica che scatta quando il documento si 
	// pianta violentemente durante la UpdateInitialFrame(). 
#ifndef _DEBUG
try
#endif
	{
		CPerformanceCrono aCrono;
		aCrono.Start();

		if (AfxMultiThreadedDocument()) 
		{
			CWinThread *pThread = AfxGetThread();
			if (pThread->IsKindOf(RUNTIME_CLASS(CDocumentThread))) // single threaded
			{
				pDocument = (CBaseDocument*)((CDocumentThread*) pThread)->OpenDocumentOnCurrentThread
					(
					pTemplate, 
					(LPCTSTR)pParams,
					_tcsicmp(pszViewMode, szNoInterface) != 0
					);
			}
			else
			{
			/*	SqlConnection* pSqlConnection = AfxGetOleDbMng()->GetNewConnection(_T("Server = USR-GRILLOLARA1; Database = ERP_ANNA; User ID = sa; Password = Microarea.; Connect Timeout = 30; MultipleActiveResultSets = true"), TRUE);
				if (!pParams)
					pParams = new DocInvocationParams();
				pParams->m_pSqlConnection = pSqlConnection;
			*/	CDocumentThread* pNewThread = (CDocumentThread*)RUNTIME_CLASS(CDocumentThread)->CreateObject();
				pDocument = (CBaseDocument*)pNewThread->OpenDocumentOnNewThread
					(
					pTemplate, 
					pParams, 
					AfxGetThreadContext()->GetLoginContextName(),
					_tcsicmp(pszViewMode, szNoInterface) != 0
					);
			}
		}
		else
		{
			pDocument = (CBaseDocument*)AfxOpenDocumentOnCurrentThread
						(
						pTemplate,
						(LPCTSTR)pParams,
						_tcsicmp(pszViewMode, szNoInterface) != 0
						);
		}
		aCrono.Stop();

		TRACE(_T("Running document in: %s\n\r"), (LPCTSTR)aCrono.GetFormattedElapsedTime());
	}
#ifndef _DEBUG
	catch (CException* e)
	{
		CleanBadDocument(pDocument, pTemplate);
		pDocument = NULL;

		TCHAR szError[1024];
		e->GetErrorMessage(szError, 1024);
		AfxGetDiagnostic()->Add(_TB("Unable to open document owing to following error:"), CDiagnostic::Error);
		AfxGetDiagnostic()->Add(szError, CDiagnostic::Error);
	}
#endif
	
	if (pDocument == NULL ) 
	{
		if (pFailedCode) *pFailedCode = InvkOslForbidden;
		return NULL;
	}
		
	if (!pDocument->IsKindOf(RUNTIME_CLASS(CBaseDocument)))
	{
		AfxGetDiagnostic()->Add(_TB("Unable to launch this type of document."), CDiagnostic::Error);
		if (pFailedCode) *pFailedCode = InvkKindOfTemplate;
		SAFE_DELETE(pParams);
		return NULL;
	}

	if (pFailedCode) *pFailedCode = InvkNoError;

	// verifico se devono essere forniti warning di attivazione
	AfxGetCommonClientObjects()->GetActivationStateWarning();

	// se si tratta di un documento con interfaccia ADM la inizializzo
	if (pDocument->IsKindOf(RUNTIME_CLASS(CAbstractFormDoc)))
	{
		CAbstractFormDoc* pDoc = (CAbstractFormDoc*) pDocument;
		if (pDoc->GetADM())
			pDoc->GetADM()->ADMAttach(pDoc);
	}

	return pDocument;
}
//-------------------------------------------------------------------
CWinThread* CTbCommandManager::CreateDocumentThread(bool bManagedMessagePump/*= false*/, ThreadHooksState hookingState /*= ThreadHooksState::DEFAULT*/)
{
	if (!AfxMultiThreadedDocument())
		return AfxGetThread();
	CDocumentThread* pThread = (CDocumentThread*)RUNTIME_CLASS(CDocumentThread)->CreateObject();
	pThread->m_bManagedMessagePump = bManagedMessagePump;
	pThread->m_nThreadHooksState = hookingState;
	pThread->Start(AfxGetThreadContext()->GetLoginContextName());	
	return pThread;

}

// La eventuale diagnostica deve essere fatta fuori dal programmatore
//-----------------------------------------------------------------------------
ADMObj*	CTbCommandManager::RunDocument
	(
		ADMClass*			pszADMClass, 
		LPCTSTR				pszViewMode	 /*szDefaultViewMode*/, 
		LPCTSTR				pszNamespace /* NULL*/,
		CBaseDocument*		pAncestor	 /*NULL*/, 
		LPAUXINFO			lpAuxInfo	 /*NULL*/, 
		ADMObj**			ppExistingADM /*NULL*/, 
		FailedInvokeCode*	pFailedCode	 /*NULL*/,
		CTBContext*			pTBContext    /*NULL*/,
		CContextBag*		pContextBag  /*NULL*/
	)
{ 
	USES_DIAGNOSTIC();

	if (IsTBLocked())
	{
		if (pFailedCode) *pFailedCode = InvkLoginLocked;
		AfxGetDiagnostic()->Add(Strings::INVALID_COMPANY(), CDiagnostic::Error);
		return NULL;
	}

	if (!pszADMClass || !pszViewMode)
	{
		if (pFailedCode) *pFailedCode = InvkKindOfAdm;
		AfxGetDiagnostic()->Add (_TB("The ADM interface is not specified. Unable to run document."));
		return NULL;
	}

	if (ppExistingADM && *ppExistingADM)
	{
		CBaseDocument *pDoc = (*ppExistingADM)->GetDocument();
		if (pDoc)
			pDoc->Activate();
		return *ppExistingADM;
	}

	// se mi è stato fornito un namespace, lo privilegio, dopo
	// l'istanza verificherò che abbia l'ADMCLASS indicatami.
	CTBNamespace aNamespace;
	if (pszNamespace)
		aNamespace.AutoCompleteNamespace(CTBNamespace::DOCUMENT, pszNamespace, aNamespace);
	
	// se non mi è stato fornito di namespace, provo a cercare il 
	// documento attraverso la dichiarazione della sua interfaccia
	if (!aNamespace.IsValid())
	{
		if (pszNamespace && AfxGetBaseApp()->IsDevelopment())
			AfxGetDiagnostic()->Add (_TB("The namespace entered for RunDocument is not complete! The first document found with the specified ADM interface will be instatiated."), CDiagnostic::Warning);

		aNamespace = GetDocNamespace(pszADMClass);
		if (!aNamespace.IsValid())
		{
			if (pFailedCode) *pFailedCode = InvkKindOfAdm;
			AfxGetDiagnostic()->Add (_TB("Specified interface does not correspond to any document! Unable to run document."));
			return *ppExistingADM;
		}
	}

	// provo ad istanziarlo. Nel caso fallisca ritorno il codice di errore
	CBaseDocument* pDoc = RunDocument(aNamespace.ToString(), pszViewMode, TRUE, pAncestor, lpAuxInfo, NULL, pFailedCode, NULL, TRUE, pTBContext, NULL,pContextBag);
	if (!pDoc || (pFailedCode && *pFailedCode != InvkNoError) || !pDoc->IsKindOf(RUNTIME_CLASS(CAbstractFormDoc)))
	{
		if (pDoc)
			DestroyDocument(pDoc);	
		return NULL;
	}

	// controlla di aver istanziato l'ADM giusto 
	CAbstractFormDoc* pADMDoc = (CAbstractFormDoc*) pDoc;
	ADMObj* pADMObj = pADMDoc->GetADM();
	if	(!(pADMObj && pADMObj->IsADMClass(pszADMClass)))
	{
		DestroyDocument(pDoc);	
		if (pFailedCode) *pFailedCode = InvkKindOfAdm;
		AfxGetDiagnostic()->Add (_TB("Document does not have the interface specified in the XML declaration! Unable to run document."));
		return NULL;
	}
	return pADMObj;
}
#pragma warning(disable: 4996) 
//-----------------------------------------------------------------------------
BOOL CTbCommandManager::ExistDocument(CBaseDocument* pDocument)
{
	HWND hwndFrame = NULL;
	return ExistDocument(pDocument, hwndFrame);
}
#pragma warning(default: 4996)

//-----------------------------------------------------------------------------
BOOL CTbCommandManager::ExistDocument (CBaseDocument* pDocument, HWND& hwndFrame)
{
	if (!pDocument) 
		return FALSE;

	//acquire READ lock on object map to prevent document destruction
	CWebServiceStateObjectsPtr ptr = AfxGetWebServiceStateObjects();
	if (!ptr->ExistObject(pDocument))
	{
		hwndFrame = NULL;
		return FALSE;
	}

	//ASSERT_VALID(pDocument);
	//even if document thread is destroying document, this variable is still valid 
	//because document thread is waiting for CWebServiceStateObjectsPtr
	hwndFrame = pDocument->m_hFrameHandle; //importante! non usare il metodo, ma la variabile! Se pDocument è morto, col metodo potrebbe schiantarsi
	return hwndFrame == NULL || ::IsWindow(hwndFrame);//additional check: the document frame has to exist!
	//READ lock on object map is released,
}
#pragma warning(disable: 4996) 
//-----------------------------------------------------------------------------
BOOL CTbCommandManager::ExistDocument (ADMObj* pAdm)
{
	HWND hwndFrame = NULL;
	return ExistDocument(pAdm, hwndFrame);
}
#pragma warning(default: 4996)

//-----------------------------------------------------------------------------
BOOL CTbCommandManager::ExistDocument(ADMObj* pAdm, HWND& hwndFrame)
{
	if (!pAdm) return FALSE;

	//Acquire lock on object map, to prevent object destruction
	CWebServiceStateObjectsPtr ptr = AfxGetWebServiceStateObjects();	
	LongArray arObjects;
	ptr->GetObjectHandles(RUNTIME_CLASS(CAbstractFormDoc), arObjects);

	for (int i=0; i < arObjects.GetSize(); i++)
	{
		CAbstractFormDoc* pDoc = (CAbstractFormDoc *)arObjects[i];
		if (pAdm == pDoc->GetADM())
		{
			hwndFrame = pDoc->GetFrameHandle();
			return ::IsWindow(hwndFrame);
		}
	}
	hwndFrame = NULL;
	return FALSE;
}

// Permette di schedulare un report e rimanere in attesa del suo completamento
// con successiva uscita dallo stesso. Attenzione se il report non si chiude in
// automatico il loop e` INFINITO (pirloni se capita)
//-----------------------------------------------------------------------------
void CTbCommandManager::WaitReportEnd (CWoormDoc* pWoormDoc)
{
	WaitDocumentEnd(pWoormDoc);
}
	
//-----------------------------------------------------------------------------
void CTbCommandManager::WaitDocumentEnd (CBaseDocument* pDocument)
{
	if (!pDocument) return;
	TBEventDisposablePtr<CBaseDocument> doc = pDocument;

	CPushMessageLoopDepthMng __pushLoopDepth(MODAL_STATE);
	AfxGetThreadContext()->RaiseCallBreakEvent();
	CTBWinThread::LoopUntil(&doc.m_Disposed);
}


//-----------------------------------------------------------------------------
void CTbCommandManager::WaitReportRunning (CWoormDoc* pWoormDoc)
{
	if (!pWoormDoc) return;
	TDisposablePtr<CWoormDoc> doc = pWoormDoc;

	CPushMessageLoopDepthMng __pushLoopDepth(MODAL_STATE);
	AfxGetThreadContext()->RaiseCallBreakEvent();

	while (doc && doc->IsReportRunning() && CTBWinThread::PumpThreadMessages())
		Sleep(1);
}

//-----------------------------------------------------------------------------

#pragma warning(disable: 4996) 

BOOL CTbCommandManager::CanCloseDocument(CBaseDocument* pDocument)
{
	USES_DIAGNOSTIC();

	HWND hwndFrame = NULL;
	BOOL bExist = ExistDocument(pDocument, hwndFrame);
	if	(pDocument == NULL || !bExist)
		return TRUE;
	try
	{
		if (!AfxInvokeThreadFunction<BOOL, CBaseDocument>(GetWindowThreadProcessId(hwndFrame, NULL), pDocument, &CBaseDocument::CanCloseDocument))
			return FALSE;
	}
	catch (CThreadCallFailedException* e)
	{
		e->Delete();
		return FALSE;
	}
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CTbCommandManager::CloseDocument(CBaseDocument*& pDocument, BOOL bAsync /*= FALSE*/)
{
	HWND hwndFrame = NULL;
	if	(pDocument == NULL || !ExistDocument(pDocument, hwndFrame))
		return TRUE;
	
	USES_DIAGNOSTIC();

	try
	{
		if (!AfxInvokeThreadFunction<BOOL, CBaseDocument, BOOL>(GetWindowThreadProcessId(hwndFrame, NULL), pDocument, &CBaseDocument::CloseDocument, bAsync))
			return FALSE;
	}
	catch (CThreadCallFailedException* e)
	{
		e->Delete();
		return FALSE;
	}
	pDocument = NULL; 
	return TRUE;
}
 
//-----------------------------------------------------------------------------
BOOL CTbCommandManager::DestroyDocument(CBaseDocument*& pDocument)
{
	if	(pDocument == NULL || !ExistDocument(pDocument))
		return TRUE;
	 
	USES_DIAGNOSTIC();

	try
	{
		AfxInvokeThreadProcedure<CBaseDocument>(pDocument->GetFrameHandle(), pDocument, &CBaseDocument::DestroyDocument);
	}
	catch (CThreadCallFailedException* e)
	{
		e->Delete();
		return FALSE;
	}

	pDocument = NULL;

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CTbCommandManager::DestroyDocument (ADMObj*& pADMObj)
{
	if	(pADMObj == NULL || !ExistDocument(pADMObj))
		return TRUE;
	
	USES_DIAGNOSTIC();

	CBaseDocument* pDoc = pADMObj->GetDocument();
	DestroyDocument(pDoc);

	pADMObj = NULL;

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CTbCommandManager::CloseDocument (const CBaseDocument* pDocument, BOOL bAsync /*= FALSE*/)
{
	CBaseDocument* p = (CBaseDocument*)pDocument;
	return CloseDocument((CBaseDocument*&)p, bAsync);
}
//-----------------------------------------------------------------------------
BOOL CTbCommandManager::DestroyDocument	(const ADMObj* pADM)
{
	ADMObj* p = (ADMObj*)pADM;
	return DestroyDocument((ADMObj*&)p);
}
//-----------------------------------------------------------------------------
BOOL CTbCommandManager::DestroyDocument	(const CBaseDocument* pDocument)
{
	CBaseDocument* p = (CBaseDocument*)pDocument;
	return DestroyDocument((CBaseDocument*&)p);
}
	
#pragma warning(default: 4996)

//-----------------------------------------------------------------------------
void CTbCommandManager::CleanBadDocument (CBaseDocument* pDocument, const CSingleExtDocTemplate* pTemplate)
{
	// se ne ho il puntatore in mano lo chiudo
	if (pDocument)
	{
		TRACE (pDocument->GetNamespace().ToString() + _TB("Error during OpenDocumentFile of document. Document will be closed."));
		DestroyDocument(pDocument);
	}
}

// Si occupa di recuperare il documento sulla base dell' interfaccia ADM
//-----------------------------------------------------------------------------
const CTBNamespace CTbCommandManager::GetDocNamespace (ADMClass* pADMClass)
{
	if (!pADMClass)
		return CTBNamespace();

	
	CString sADMClass = (LPCTSTR) *pADMClass;

	if (sADMClass.IsEmpty())
		return CTBNamespace();

	AddOnApplication*	pAddOnApp = NULL;
	AddOnModule*		pAddOnMod = NULL;
	
	CBaseDescriptionArray* pArrDescri;
	const CDocumentDescription* pDocInfo = NULL;
	for (int i=0; i <= AfxGetAddOnAppsTable()->GetUpperBound(); i++)
	{
		pAddOnApp = AfxGetAddOnAppsTable()->GetAt(i);

		if (!pAddOnApp->m_pAddOnModules)
			continue;

		for (int n=0; n <= pAddOnApp->m_pAddOnModules->GetUpperBound(); n++)
		{
			pAddOnMod = pAddOnApp->m_pAddOnModules->GetAt(n);
			if (!pAddOnMod)
				continue;
		
			pArrDescri = AfxGetDocumentDescriptionsOf(pAddOnMod->m_Namespace);

			pDocInfo = NULL;
			const CDocumentDescription* pDescri = NULL;

			for (int d=0; d <= pArrDescri->GetUpperBound(); d++)
			{
				pDescri = (const CDocumentDescription*) pArrDescri->GetAt(d);
				if (pDescri && _tcsicmp(pDescri->GetInterfaceClass(), sADMClass) == 0)
				{
					pDocInfo = pDescri;
					break;
				}
			}

			delete pArrDescri;
			if (pDocInfo)
				return pDocInfo->GetNamespace();
		}
	}

	return CTBNamespace();
}

//-----------------------------------------------------------------------------
CPadDoc* CTbCommandManager::RunEditor(CString sName /*_T("")*/)
{   
	USES_DIAGNOSTIC();

	// login context locked
	if (IsTBLocked())
	{
		AfxGetDiagnostic()->Add(Strings::INVALID_COMPANY(), CDiagnostic::Error);
		return NULL;
	}

	const CSingleExtDocTemplate* pTemplate = AfxGetTemplate(RUNTIME_CLASS(CPadView), 0);

	if (!pTemplate)
	{   
		AfxGetDiagnostic()->Add (_TB("The text editor has not been registered! Unable to run document."), CDiagnostic::Error);
		return NULL;
	}

	CString sFileName; 

	// se è un namespace lo gestisco come tale, altrimenti assumo sia un filename
	if (!sName.IsEmpty())
	{
		if (IsDosName(sName, TRUE))
			sFileName = sName;
		else
		{
			CTBNamespace aNamespace(CTBNamespace::TEXT, sName);

			// se ho il namespace lo espando in filename
			if (aNamespace.IsValid() && aNamespace.GetType() == CTBNamespace::TEXT)
				sFileName = AfxGetPathFinder()->GetFileNameFromNamespace(aNamespace, AfxGetLoginInfos()->m_strUserName);
			else
				sFileName = sName;
		}
	}
	else 
		sFileName = sName;

	// controllo se esiste
	if (!sFileName.IsEmpty() && !ExistFile(sFileName))
	{
		AfxGetDiagnostic()->Add(_TB("Text file not found."), CDiagnostic::Error);
		return NULL;
	}

	DocInvocationParams* pParams = new DocInvocationParams ();
	pParams->m_pDocInfo	= new DocInvocationInfo(NULL, sName);

	CDocumentThread* pNewThread = (CDocumentThread*)RUNTIME_CLASS(CDocumentThread)->CreateObject();
	CPadDoc* pDoc = (CPadDoc*) pNewThread->OpenDocumentOnNewThread
		(
			pTemplate, 
			pParams, 
			AfxGetThreadContext()->GetLoginContextName()
		);

	return pDoc;
}
#pragma warning(disable: 4996)
//-----------------------------------------------------------------------------
BOOL CTbCommandManager::EditorRunning (CPadDoc* pPadDoc)
{
	return ExistDocument(pPadDoc);
}
#pragma warning(default: 4996)
//-----------------------------------------------------------------------------
BOOL CTbCommandManager::FireEvent (const CString& sEvent, FunctionDataInterface* pParams, FailedInvokeCode* pFailedCode /*= NULL*/)
{
	USES_DIAGNOSTIC();
	if (sEvent.IsEmpty())
	{
		AfxGetDiagnostic()->Add(cwsprintf(_TB("Unnamed event \"{0-%s}\"."), (LPCTSTR) sEvent), CDiagnostic::Error);
		return FALSE;
	}

	FunctionDataInterface* pFunction = NULL;
	const CFunctionDescription* pXmlDescri = NULL;
	CBaseDescriptionArray* pXmlDescris = NULL;

	// itero su tutte le AddOnApplication per scoprire si è registrato all'evento
	for (int i = 0; i <= AfxGetAddOnAppsTable()->GetUpperBound(); i++)
	{
		AddOnApplication* pAddOnApp = AfxGetAddOnAppsTable()->GetAt(i);

		for (int j = 0; j <= pAddOnApp->m_pAddOnModules->GetUpperBound(); j++) 
		{
			AddOnModule* pAddOnMod = pAddOnApp->m_pAddOnModules->GetAt(j);

			pXmlDescris = (CBaseDescriptionArray*) pAddOnMod->m_XmlDescription.GetEventHandlersInfo().GetFunctionsByName(sEvent);
			if (!pXmlDescris)
				continue;
			
			// all functions descriptions into the same module
			for (int f = 0; f <= pXmlDescris->GetUpperBound(); f++) 
			{
				pXmlDescri = (CFunctionDescription*) pXmlDescris->GetAt(f);

				// full activation event handler
				if (pXmlDescri->IsFullExecutePolicy() && !AfxIsActivated(pAddOnMod->GetApplicationName(), pAddOnMod->GetModuleName()))
					continue;

				// addon activation event handler
				if (pXmlDescri->IsAddOnExecutePolicy())
				{
					CTBNamespace aLibraryNs (CTBNamespace::LIBRARY, pXmlDescri->GetNamespace().Left(CTBNamespace::LIBRARY, FALSE)); 
					if (!AfxIsActivated(aLibraryNs))
					continue;
				}

				// se la libreria dichiara di voler ricevere sempre l'evento
				// la devo caricare in memoria
				if (pXmlDescri->AlwaysCalledIfEvent())
					LoadNeededLibraries(pXmlDescri->GetNamespace());

				for (int n = 0; n <= pAddOnMod->m_pAddOnLibs->GetUpperBound(); n++) 
				{
					AddOnLibrary* pAddOnLib = pAddOnMod->m_pAddOnLibs->GetAt(n);
					if (pAddOnLib)
						pFunction = (FunctionDataInterface*) pAddOnLib->GetRegisteredObjectInfo(pXmlDescri->GetNamespace());

					// se non la trovo passo alla successiva
					if (!pFunction || !pFunction->m_pfFunction)
						continue;

					// idem se non ho i diritti di accesso
					CBaseSecurityInterface* pSecurity = AfxGetSecurityInterface();
					if (pSecurity)
					{
						CInfoOSL infoOsl(pFunction->GetInfoOSL()->m_Namespace, pFunction->GetInfoOSL()->GetType());
						pSecurity->GetObjectGrant (&infoOsl);
						if (!OSL_CAN_DO(&infoOsl, OSL_GRANT_EXECUTE))
							continue;
					}

					try
					{
						pFunction->m_pfFunction (pParams);
					}
					catch  (...)
					{
						ASSERT(FALSE);
					}
				}
			} 
			delete pXmlDescris;
		}
	}

	return TRUE;
}

//-----------------------------------------------------------------------------
CWoormDoc* CTbCommandManager::RunWoormReport 
									(
										LPCTSTR pszReportNamespace, 
										CBaseDocument* pCallerDoc, 
										BOOL bUseRadarFrame /*FALSE*/,
										BOOL bRunReport /*TRUE*/
									)
{   
	if (pCallerDoc && pCallerDoc->InvokeRequired())
	{
		CWinThread* pThread = CreateDocumentThread();
		return AfxInvokeThreadFunction
				<
				CWoormDoc*,
				CTbCommandManager,
				LPCTSTR,
				CBaseDocument*,
				BOOL,
				BOOL
				>
				(
				pCallerDoc->GetThreadId(),
				this, 
				&CTbCommandManager::RunWoormReport,	
				pszReportNamespace,
				pCallerDoc,
				bUseRadarFrame,
				bRunReport
				);
		}
	CWoormInfo*	pWoormInfo = new CWoormInfo(DataStr(pszReportNamespace));
	pWoormInfo->m_bOwnedByReport = TRUE;	

	return RunWoormReport(pWoormInfo, pCallerDoc, NULL, bUseRadarFrame, bRunReport);
}

//-----------------------------------------------------------------------------
CWoormDoc* CTbCommandManager::RunWoormReport 
									(
										CWoormInfo* pWoormInfo, 
										CBaseDocument* pCallerDoc, 
										CExternalControllerInfo*pControllerInfo /*NULL*/,
										BOOL bUseRadarFrame /*FALSE*/,
										BOOL bRunReport /*TRUE*/
									)
{   
	ASSERT_VALID(pWoormInfo);
	if (!pWoormInfo)
		return NULL;

	if (pCallerDoc)
	{
		ASSERT_VALID(pCallerDoc);
		if (pCallerDoc->InvokeRequired())
		{
			return AfxInvokeThreadFunction
						<
							CWoormDoc*,
							CTbCommandManager,
							CWoormInfo*,
							CBaseDocument*,
							CExternalControllerInfo*,
							BOOL,
							BOOL
						>
						(
							pCallerDoc->GetThreadId(),
							this, 
							&CTbCommandManager::RunWoormReport,	
							pWoormInfo,
							pCallerDoc,
							pControllerInfo,
							bUseRadarFrame,
							bRunReport
						);
		}
	}

	USES_DIAGNOSTIC();

	CLoginContext* pLoginContext = AfxGetLoginContext();
	if(!pLoginContext ->m_bMluValid)
	{
		AfxGetDiagnostic()->Add(cwsprintf( _TB( "Sadly your M.L.U. service is no longer active.\r\nThe operation date provided follows the M.L.U. expiry date: please select an operation date prior to the M.L.U. expiry date.\r\nRestore the full efficiency of you Mago.net!\r\nFind out how by visiting the website: {0-%s}") , AfxGetLoginManager()->GetBrandedKey(_T("MLURenewalShortLink")), CDiagnostic::Error));
		return NULL;
	}

	// login context locked
	if (IsTBLocked())
	{
		AfxGetDiagnostic()->Add(Strings::INVALID_COMPANY(), CDiagnostic::Error);
		return NULL;
	}

	if (pWoormInfo->m_nNextReport > pWoormInfo->m_ReportNames.GetUpperBound())
		return NULL;

	if (
			AfxGetApplicationContext()->m_MacroRecorderStatus == CApplicationContext::PLAYING &&
			!(pWoormInfo->m_bAutoPrint && pWoormInfo->m_bCloseOnEndPrint)
		)  
		return NULL;
	
	// considero il tipo non obbligatorio
	CString sReportNamespace = pWoormInfo->m_ReportNames.GetAt(pWoormInfo->m_nNextReport++);
	if (
			!AfxIsInUnattendedMode() &&  
			_tcsicmp(AfxGetLoginManager()->GetEdition(), szEnterpriseEdition) == 0 &&
			!AfxIsCalAvailable(_T("ClientNet"), _NS_ACT("LicenzaUtente"))
		)
	{		
		AfxGetDiagnostic()->Add(CTbCommandManagerStrings::NoUserLicense(), CDiagnostic::Error);
		AfxGetDiagnostic()->Add(sReportNamespace, CDiagnostic::Error);
		return NULL;
	}

	CTBNamespace aNamespace (CTBNamespace::REPORT);
	if (!IsDosName(sReportNamespace, TRUE))
	{
		aNamespace.AutoCompleteNamespace(CTBNamespace::REPORT, sReportNamespace, aNamespace);

		if (aNamespace.IsValid())
		{
			//TODO BAUZI - file wrm da provider 
			if (0)
			{
				//load metadato

				//strReport = ...

				pWoormInfo->m_bIsReportString = TRUE;
			}
		}
	}

	CString strReport (sReportNamespace);

	if (bUseRadarFrame && IsDosName(sReportNamespace, TRUE)) 
	{
		;//temporary Radar-woorm 
	}
	else if (/*bUseRadarFrame && */pWoormInfo->m_bIsReportString)
	{
		goto l_reportString;//temporary Radar-woorm 
	}
	else
	{
		// se non è un namespace valido provo direttamente con il filename
		if (aNamespace.IsValid())
			strReport = AfxGetPathFinder()->GetReportFullName(aNamespace, AfxGetThreadContext()->GetSessionLoginName());
		else
			aNamespace = AfxGetPathFinder()->GetNamespaceFromPath(sReportNamespace);

	if	(
			!AfxIsInUnattendedMode() &&  
			_tcsicmp(AfxGetLoginManager()->GetEdition(), szEnterpriseEdition) == 0 &&
			!pWoormInfo->m_bAutoPrint && !pWoormInfo->m_bHideFrame &&
			!AfxIsCustomObject(&aNamespace) && //gli oggetti di moduli custom non sono soggetti ad attivazione
			!AfxIsCalAvailable(aNamespace.GetApplicationName(), aNamespace.GetObjectName(CTBNamespace::MODULE))
		)
		{
			AfxGetDiagnostic()->Add(CTbCommandManagerStrings::NoUserLicense(), CDiagnostic::Error);
			AfxGetDiagnostic()->Add(sReportNamespace, CDiagnostic::Error);
			return NULL;
		}
		// verifica che il modulo sia attivato
		if (!AfxIsActivated(aNamespace.GetApplicationName(), aNamespace.GetObjectName(CTBNamespace::MODULE)))
		{
			AfxGetDiagnostic()->Add(_TB("The configuration requested for this function is disabled: please contact your system administrator.") + _T("\r\n") + sReportNamespace, CDiagnostic::Error);
			return NULL;
		}

		if (!AfxGetLoginContext()->CanUseNamespace(aNamespace.ToString(), OSL_GRANT_EXECUTE))
		{
			AfxGetDiagnostic()->Add(cwsprintf(_TB("User hasn't permission to  execute list \"{0-%s}\". Please contact application administrator for obtain it."), (LPCTSTR) strReport), CDiagnostic::Error);
			return NULL;
		}
	}

	if (strReport.IsEmpty() || !ExistFile(strReport))
	{
		AfxGetDiagnostic()->Add(cwsprintf(_TB("List \"{0-%s}\" not found."), (LPCTSTR) strReport), CDiagnostic::Error);
		return NULL;
	}

	{
		CInfoOSL infoOSL(aNamespace, OSLType_Report);
		AfxGetSecurityInterface()->GetObjectGrant (&infoOSL);
		if (!OSL_CAN_DO( &(infoOSL), OSL_GRANT_EXECUTE))
		{
			AfxGetDiagnostic()->Add(cwsprintf(_TB("User hasn't permission to  execute list \"{0-%s}\". Please contact application administrator for obtain it."), (LPCTSTR) strReport), CDiagnostic::Error);
			return NULL;
		}
	}

	{
		FunctionDataInterface fdi;
		fdi.AddStrParam(_T("namespace"), aNamespace.ToString());
		DataStr message = _T("");
		fdi.AddOutParam(_T("message"), &message);
		DataLng controllerHandle((pControllerInfo) ? ((long)pControllerInfo) : 0);
		fdi.AddInOutParam(_T("controllerHandle"), &controllerHandle);
		fdi.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::Bool, CDataObjDescription::_OUT));
		fdi.SetReturnValue(DataBool(TRUE));
		FireEvent(szOnBeforeRunReport, &fdi);
		DataBool* res = (DataBool*)fdi.GetReturnValue();
		if (!res || !(*res))
		{
			AfxGetDiagnostic()->Add(message.Str());
			return NULL;
		}
		else
			if (!pControllerInfo && controllerHandle > 0)
			{
				CObject* pObj = ((CObject*)((long)controllerHandle));
				if (pObj->IsKindOf(RUNTIME_CLASS(CExternalControllerInfo)))
					pControllerInfo = (CExternalControllerInfo*)pObj;
			}
	}

l_reportString:	//il goto serve per saltare tutti i controlli sul file
	//----

	CRuntimeClass* rcView = RUNTIME_CLASS(CWoormView); 
	/* TODO Riccardo e Marco e Silvano */
	if (AfxIsRemoteInterface())
		rcView = RUNTIME_CLASS(CAbstractWoormView);
	else  if (bUseRadarFrame)
		rcView = RUNTIME_CLASS(CWrmRadarView);

	const CSingleExtDocTemplate* pTemplate = AfxGetTemplate(rcView,	0);

	// istanzio il docinvocation e lo inizializzo con i parametri solo se necessario
	DocInvocationParams* pParams = new DocInvocationParams ();
	pParams->m_pDocInfo			 = new DocInvocationInfo(pCallerDoc, strReport, pWoormInfo, pControllerInfo);

	// try to execute woorm report
	pWoormInfo->SetNamespace(aNamespace);
	pWoormInfo->m_bRunReport = bRunReport;

	CBaseDocument* pDoc = NULL;
	CWinThread *pThread = AfxGetThread();
	if (pThread->IsKindOf(RUNTIME_CLASS(CDocumentThread))) //single threaded
	{
		pDoc = (CBaseDocument*)((CDocumentThread*) pThread)->OpenDocumentOnCurrentThread
			(
			pTemplate, 
			(LPCTSTR)pParams, 
			!(pWoormInfo->m_bIconized || pWoormInfo->m_bHideFrame)
			);
	}
	else
	{
		if (AfxMultiThreadedDocument()) //sono in modalità multithreading
		{
			CWinThread *pThread = AfxGetThread();
			if (pThread->IsKindOf(RUNTIME_CLASS(CDocumentThread))) //se sono in un thread di documento, gli altri documenti devo aprirli in questo thread
			{
				pDoc = (CBaseDocument*)((CDocumentThread*) pThread)->OpenDocumentOnCurrentThread
				(
					pTemplate, 
					(LPCTSTR)pParams, 
					!(pWoormInfo->m_bIconized || pWoormInfo->m_bHideFrame)
				);
			}
			else
			{
				//altrimenti creo un nuovo thread di documento
				CDocumentThread* pNewThread = (CDocumentThread*)RUNTIME_CLASS(CDocumentThread)->CreateObject();
				pDoc = (CBaseDocument*)pNewThread->OpenDocumentOnNewThread
						(
						pTemplate, 
						pParams, 
						AfxGetThreadContext()->GetLoginContextName(),
						!(pWoormInfo->m_bIconized || pWoormInfo->m_bHideFrame)
						);
			}
		}
		else //se non sono in modalità multithread, apro il documento nel thread corrente
		{
			pDoc = (CBaseDocument*)AfxOpenDocumentOnCurrentThread
					(
						pTemplate, 
						(LPCTSTR)pParams, 
						!(pWoormInfo->m_bIconized || pWoormInfo->m_bHideFrame)
					);
		}
	}
	//metto da parte il puntatore nello smart pointer perché l'oggetto potrebbe morire durante la show del diagnostic
	WoormDocPtr ptr = (CWoormDoc*) pDoc;
	SHOW_DIAGNOSTIC()
	return ptr;
}
#pragma warning(disable: 4996)
//-----------------------------------------------------------------------------
BOOL CTbCommandManager::ReportRunning (CWoormDoc* pWoormDoc)
{
	return ExistDocument(pWoormDoc);
}
#pragma warning(default: 4996)
//-----------------------------------------------------------------------------
BOOL CTbCommandManager::CloseWoormReport(CWoormDoc* pWoormDoc)
{
	return CloseDocument((CBaseDocument*&) pWoormDoc);
}
#pragma warning(disable: 4996)
//-----------------------------------------------------------------------------
BOOL CTbCommandManager::CloseReportReady (CWoormDoc* pWoormDoc)
{
	if (ReportRunning(pWoormDoc))
	{
		if (pWoormDoc->IsReportRunning())
		{
			pWoormDoc->Activate(NULL, TRUE);
			return FALSE;
		}
		else
			return CloseWoormReport(pWoormDoc);
	}
	return TRUE;
}
#pragma warning(default: 4996)

//-----------------------------------------------------------------------------
BOOL CTbCommandManager::RunFunction (CFunctionDescription* pFunction, FailedInvokeCode* pFailedCode /*= NULL*/)
{
	USES_DIAGNOSTIC();

	// login context locked (TODOBRUNA definizione delle funzioni che devono passare
/*	if (IsTBLocked())
	{
		if (pFailedCode) *pFailedCode = InvkLoginLocked;
		return NULL;
	}*/

	// le functions che passano prima dell'operazione di Login, 
	// vanno lasciate passare. La CanUseNamepsace ritornerebbe false
	if (
		!AfxGetAuthenticationToken().IsEmpty() &&
			!AfxGetLoginContext()->CanUseNamespace(pFunction->GetNamespace().ToString(), OSL_GRANT_EXECUTE)
		)
	{
		AfxGetDiagnostic()->Add(_TB("User hasn't permission to execute function. Please contact application administrator.") + _T(".\r\n ") + pFunction->GetNamespace().ToString(), CDiagnostic::Error);
		return FALSE;
	}

	CBaseSecurityInterface* pSecurity = AfxGetSecurityInterface();
	if (pSecurity && pSecurity->IsSecurityEnabled())
	{
		CInfoOSL infoOSL(pFunction->GetNamespace(), OSLType_Function);
		AfxGetSecurityInterface()->GetObjectGrant (&infoOSL);
		if (!OSL_CAN_DO(&(infoOSL), OSL_GRANT_EXECUTE))
		{
			AfxGetDiagnostic()->Add(cwsprintf(_TB("User hasn't permission to execute function \"{0-%s}\". Please contact application administrator for obtain it."), pFunction->GetNamespace().ToUnparsedString()), CDiagnostic::Error);
			return FALSE;
		}
	}
	
	CString sTbScript = pFunction->GetTBScript();
	if (!sTbScript.IsEmpty())
	{
		SymTable symTable;
		SymTable* symTablePtr = &symTable;

		if (pFunction->IsThisCallMethods())
		{
			DataLng* pHandle = (DataLng*) pFunction->GetParamValue(_T("handle"));
			if (!pHandle || !pHandle->IsKindOf(RUNTIME_CLASS(DataLng)) || !pHandle->IsHandle())
			{
				AfxGetDiagnostic()->Add(cwsprintf(_TB("Missing handle parameter \"{0-%s}\""), pFunction->GetNamespace().ToUnparsedString()), CDiagnostic::Error);
				return FALSE;
			}
			CObject* pObj =  (CObject*)((long)*pHandle);
			if (!pObj)
			{
				AfxGetDiagnostic()->Add(cwsprintf(_TB("Handle address is null\"{0-%s}\""), pFunction->GetNamespace().ToUnparsedString()), CDiagnostic::Error);
				return FALSE;
			}

			CBaseDocument* pDoc = NULL;
			if (pObj->IsKindOf(RUNTIME_CLASS(CBaseDocument)))
			{
				pDoc = (CBaseDocument*)pObj;
			}
			else if (pObj->IsKindOf(RUNTIME_CLASS(CClientDoc)))
			{
				pDoc = ((CClientDoc*)pObj)->GetMasterDocument();
			}

			if (!pDoc)
			{
				AfxGetDiagnostic()->Add(cwsprintf(_TB("Unknown handle address \"{0-%s}\""), pFunction->GetNamespace().ToUnparsedString()), CDiagnostic::Error);
				return FALSE;
			}
#pragma warning(disable: 4996)
			if (!ExistDocument(pDoc))
			{
				AfxGetDiagnostic()->Add(cwsprintf(_TB("Document was not found \"{0-%s}\""), pFunction->GetNamespace().ToUnparsedString()), CDiagnostic::Error);
				return FALSE;
			}
#pragma warning(default: 4996)
			symTablePtr = pObj->IsKindOf(RUNTIME_CLASS(CClientDoc)) ?
							((CClientDoc*)pObj)->GetSymTable() : 
							pDoc->GetSymTable();
		}

		Parser lex(sTbScript);
		TBScript* pScript = CreateTbScript(pFunction, symTablePtr);
		if (!pScript)
			return FALSE;
		if (!pScript->Parse(lex))
		{
			AfxGetDiagnostic()->Add(cwsprintf(_TB("Error on parsing script method \"{0-%s}\""), pFunction->GetNamespace().ToUnparsedString()), CDiagnostic::Error);
			delete pScript;
			return FALSE;
		}
		if (!pScript->Exec())
		{
			AfxGetDiagnostic()->Add(cwsprintf(_TB("Error on executing script method \"{0-%s}\""), pFunction->GetNamespace().ToUnparsedString()), CDiagnostic::Error);
			delete pScript;
			return FALSE;
		}
		delete pScript;
		return TRUE;
	}

	return AfxInvokeSoapMethod(pFunction);
}

//-----------------------------------------------------------------------------
BOOL CTbCommandManager::RunFunction (LPCTSTR lpcszNamespace, FailedInvokeCode* pFailedCode)
{
	USES_DIAGNOSTIC();
	CFunctionDescription aFunction;
	if (!GetFunctionDescription(lpcszNamespace, aFunction))
		return FALSE;

	return RunFunction(&aFunction, pFailedCode);
}

//-----------------------------------------------------------------------------
BOOL CTbCommandManager::GetFunctionDescription(LPCTSTR lpcszNamespace, CFunctionDescription &aFunctionDescription)
{
	CTBNamespace aNamespace(CTBNamespace::FUNCTION, lpcszNamespace);

	return GetFunctionDescription(aNamespace, aFunctionDescription);
}

//-----------------------------------------------------------------------------
BOOL CTbCommandManager::GetFunctionDescription(const CTBNamespace& aNs, CFunctionDescription &aFunctionDescription, BOOL bEmitDiagnostic/* = TRUE*/)
{
	const CFunctionDescription *pFunction = GetFunctionDescription(aNs, bEmitDiagnostic);
	if (!pFunction) return FALSE;

	aFunctionDescription = *pFunction;
	return TRUE;
}

//-----------------------------------------------------------------------------
const CFunctionDescription* CTbCommandManager::GetFunctionDescription(const CTBNamespace& aNs, BOOL bEmitDiagnostic /*= TRUE*/)
{
	if (!aNs.IsValid() || aNs.GetType() != CTBNamespace::FUNCTION)
	{
		if (bEmitDiagnostic)
			AfxGetDiagnostic()->Add(cwsprintf(_TB("Function \"{0-%s}\" of invalid type."), (LPCTSTR) aNs.ToString()), CDiagnostic::Error);
		return NULL;
	}
	
	AddOnModule* pAddOnMod = AfxGetAddOnModule(aNs);
	if(!pAddOnMod)
	{
		if (bEmitDiagnostic)
			AfxGetDiagnostic()->Add(cwsprintf(_TB("The searched function \"{0-%s} is not found in any installed module."), (LPCTSTR) aNs.ToString()), CDiagnostic::Info);
		return NULL;
	}

	const CBaseDescriptionArray &functions = pAddOnMod->m_XmlDescription.GetFunctionsInfo().m_arFunctions;

	for (int nF2 = 0; nF2 <= functions.GetUpperBound(); nF2++)
	{
		CFunctionDescription* pF = (CFunctionDescription*) functions.GetAt(nF2);
		if (pF->GetNamespace() == aNs)
			return pF;
	}
	
	return NULL;
}

//-----------------------------------------------------------------------------
BOOL CTbCommandManager::SaveDataFileInfo(CDataFileInfo* pDFI)
{
	//salvo le modifiche effettuate dall'utente sul file di custom presente in ALL_USER
	CXMLDataFileParser parser;
	parser.SaveDataFile(pDFI);
	//devo invalidare anche il DFI presente nella cache in modo che i dati siano ricaricati
	CDataFileInfo* pCachedDFI = AfxGetDataFilesManager()->GetDataFile(pDFI->m_Namespace.ToString());
	if (pCachedDFI)
		pCachedDFI->m_bInvalid = TRUE;

	return TRUE;
}

//-----------------------------------------------------------------------------
CDataFileInfo*  CTbCommandManager::GetDataFileInfo (LPCTSTR lpcszNamespace, BOOL bAllowChanges /*=FALSE*/, BOOL bUseProductLanguage /*= FALSE*/)
{
	USES_DIAGNOSTIC();
	CDataFileInfo* pdfi = NULL;

	if (!bAllowChanges)
		pdfi = AfxGetDataFilesManager()->GetDataFile(lpcszNamespace);		

	if (pdfi == NULL)
	{
		CTBNamespace aNs(lpcszNamespace);
		if (! aNs.IsValid() || aNs.GetType() != CTBNamespace::DATAFILE )
		{
			ASSERT(FALSE);
			AfxGetDiagnostic()->Add(cwsprintf(_TB("Invalid Namespace for DataFile \"{0-%s}\"."), (LPCTSTR) aNs.ToString()), CDiagnostic::Error);
			return NULL;
		}

		CXMLDataFileParser parser;
		pdfi = new CDataFileInfo(aNs, bAllowChanges, bUseProductLanguage);
		if (!parser.LoadDataFile(pdfi))
		{
			TRACE(_T("Failed loading data file ") + aNs.ToString() + _T(".\r\n"));

			delete pdfi;
			return NULL;
		}
		
		if (!bAllowChanges) // non lo aggiungo alla cache, quelli con AllowChanges = TRUE sono a perdere e sarà cura dell'XMLCombo che lo ha richiesto di deletarlo
			AfxGetDataFilesManager()->Add(pdfi); 
	}
	return pdfi;
}

//-----------------------------------------------------------------------------
BOOL CTbCommandManager::ExistFunction(const CTBNamespace& aNs) 
{
	return GetFunctionDescription(aNs) != NULL;
}

//-----------------------------------------------------------------------------
AddOnLibrary* CTbCommandManager::GetAddOnLibrary(const CTBNamespace& aNs)
{
	// login context locked
	if (IsTBLocked())
	{
		AfxGetDiagnostic()->Add(Strings::INVALID_COMPANY(), CDiagnostic::Error);
		return NULL;
	}

	AddOnLibrary* pAddOnLib = AfxGetAddOnLibrary(aNs);
	if (!pAddOnLib)
	{
		// carico le DLL necessarie	
		m_LibrariesLoader.LoadNeededLibraries(aNs);

		pAddOnLib = AfxGetAddOnLibrary(aNs);
		if (!pAddOnLib)
		{
			AfxGetDiagnostic()->Add(aNs.ToString() + _TB(": object not registered."));
			return NULL;
		}
	}

	return pAddOnLib;
}

//-----------------------------------------------------------------------------
CItemSource* CTbCommandManager::CreateItemSource(const CTBNamespace& aNs)
{
	USES_DIAGNOSTIC();

	AddOnLibrary* pAddOnLib = GetAddOnLibrary(aNs);
	if (!pAddOnLib)
		return NULL;


	CRuntimeClass* pClass = pAddOnLib->GetItemSource(aNs);
	if (!pClass)
	{
		AfxGetDiagnostic()->Add(aNs.ToString() + _TB(": item source not registered."));
		return NULL;
	}
	return (CItemSource*)pClass->CreateObject();
}


//-----------------------------------------------------------------------------
CControlBehaviour* CTbCommandManager::CreateControlBehaviour(const CTBNamespace& aNs)
{
	USES_DIAGNOSTIC();

	AddOnLibrary* pAddOnLib = GetAddOnLibrary(aNs);
	if (!pAddOnLib)
		return NULL;

	CRuntimeClass* pClass = pAddOnLib->GetControlBehaviour(aNs);
	if (!pClass)
	{
		AfxGetDiagnostic()->Add(aNs.ToString() + _TB(": control behaviour not registered."));
		return NULL;
	}
	CControlBehaviour* pObj = (CControlBehaviour*)pClass->CreateObject();

	ASSERT_TRACE1(pObj, "CreateObject cannot create control behaviour class: does the class %s use correctly IMPLEMENT_DYNCREATE macro?", pClass->m_lpszClassName);
	return pObj;
}

//-----------------------------------------------------------------------------
CDataAdapter* CTbCommandManager::CreateDataAdapters(const CTBNamespace& aNs)
{
	USES_DIAGNOSTIC();

	AddOnLibrary* pAddOnLib = GetAddOnLibrary(aNs);
	if (!pAddOnLib)
		return NULL;

	CRuntimeClass* pClass = pAddOnLib->GetDataAdapter(aNs);
	if (!pClass)
	{
		AfxGetDiagnostic()->Add(aNs.ToString() + _TB(": data adapter not registered."));
		return NULL;
	}
	return (CDataAdapter*)pClass->CreateObject();
}


//-----------------------------------------------------------------------------
CValidator* CTbCommandManager::CreateValidator(const CTBNamespace& aNs)
{
	USES_DIAGNOSTIC();

	AddOnLibrary* pAddOnLib = GetAddOnLibrary(aNs);
	if (!pAddOnLib)
		return NULL;

	CRuntimeClass* pClass = pAddOnLib->GetValidator(aNs);
	if (!pClass)
	{
		AfxGetDiagnostic()->Add(aNs.ToString() + _TB(": validator not registered."));
		return NULL;
	}
	return (CValidator*)pClass->CreateObject();
}

//-----------------------------------------------------------------------------
FunctionDataInterface* CTbCommandManager::GetHotlinkDescription (const CTBNamespace& aHKLNs, BOOL& bIsHKLDynamic, BOOL& bIsHKLXml)
{
	USES_DIAGNOSTIC();

	// login context locked
	if (IsTBLocked())
	{
		AfxGetDiagnostic()->Add(Strings::INVALID_COMPANY(), CDiagnostic::Error);
		return NULL;
	}

	AddOnModule* pAddOnMod = AfxGetAddOnModule(aHKLNs);
	if (!pAddOnMod)
		return NULL;

	CFunctionDescription* pXmlHklDescri = pAddOnMod->m_XmlDescription.GetParamObjectInfo(aHKLNs);
	if (pXmlHklDescri && pXmlHklDescri->IsKindOf(RUNTIME_CLASS(CHotlinkDescription)))
	{
		if (
			((CHotlinkDescription*)pXmlHklDescri)->IsDynamic() &&
			((CHotlinkDescription*)pXmlHklDescri)->GetClassName().IsEmpty()
			)
		{
			bIsHKLDynamic = TRUE;
			return NULL;
		}
		if (((CHotlinkDescription*)pXmlHklDescri)->IsXml())
		{
			bIsHKLXml = TRUE;
			return NULL;
		}
	}

	AddOnLibrary* pAddOnLib = AfxGetOwnerAddOnLibrary(&aHKLNs);
	if (!pAddOnLib)
	{
		// carico le DLL necessarie	
		m_LibrariesLoader.LoadNeededLibraries(aHKLNs);

		pAddOnLib = AfxGetOwnerAddOnLibrary(&aHKLNs);
		if (!pAddOnLib)
		{
			AfxGetDiagnostic()->Add (aHKLNs.ToString () + _TB(": hotlink not registered."));
			return NULL;
		}
	}

	FunctionDataInterface* pDescri = (FunctionDataInterface*) pAddOnLib->GetRegisteredObjectInfo(aHKLNs);
	return pDescri;
}

//-----------------------------------------------------------------------------
HotKeyLinkObj* CTbCommandManager::RunHotlink (const CTBNamespace& aHKLNs, FailedInvokeCode*, CRuntimeClass** ppControlClass/* = NULL*/)
{
	USES_DIAGNOSTIC();

	BOOL bHKLDynamic = FALSE;
	BOOL bHKLXml = FALSE;
	if (aHKLNs.HasAFakeLibrary())
	{
		AddOnModule* pAddOnMod = AfxGetAddOnModule(aHKLNs);
		if (!pAddOnMod)
		{
			ASSERT(FALSE);
			return NULL;
		}
		if (!m_pEBCommands)
		{
			CTBNamespace ebNs(_T("Extensions.EasyBuilder.TbEasyBuilder"));
			LoadNeededLibraries(ebNs);
			ASSERT(m_pEBCommands);
		}

		FunctionDataInterface* pDescri = GetHotlinkDescription(aHKLNs, bHKLDynamic, bHKLXml);
		if (bHKLDynamic)
		{
			//se ho applicato dei cambiamenti al catalog in memoria nella sessione precedente
			//e li ho utilizzati nell'object model, uscendo e rientrando potrebbe non funzionare più
			//quindi devo per prima cosa applicare gli stessi cambiamenti (che mi ero salvato su file system)
			if (AfxIsActivated(szExtensionsApp, EASYSTUDIO_MODULE_NAME))
				RunFunction(_T("Extensions.EasyBuilder.TbEasyBuilder.UpdateCatalogIfNeeded"));
			return new DynamicHotKeyLink(aHKLNs.ToString());
		}
		if (bHKLXml)
		{
			return new XmlHotKeyLink(aHKLNs.ToString());
		}
	}

	FunctionDataInterface* pDescri = GetHotlinkDescription(aHKLNs, bHKLDynamic, bHKLXml);
	if (bHKLDynamic)
	{
		//se ho applicato dei cambiamenti al catalog in memoria nella sessione precedente
		//e li ho utilizzati nell'object model, uscendo e rientrando potrebbe non funzionare più
		//quindi devo per prima cosa applicare gli stessi cambiamenti (che mi ero salvato su file system)
		if (AfxIsActivated(szExtensionsApp, EASYSTUDIO_MODULE_NAME))
			RunFunction(_T("Extensions.EasyBuilder.TbEasyBuilder.UpdateCatalogIfNeeded"));
		return new DynamicHotKeyLink(aHKLNs.ToString());
	}
	if (bHKLXml)
	{
		return new XmlHotKeyLink(aHKLNs.ToString());
	}

	if (!pDescri || !pDescri->m_pComponentClass)
	{
		AfxGetDiagnostic()->Add (aHKLNs.ToString () + _TB(": hotlink not registered."));
		return NULL;
	}

	HotKeyLinkObj* pHKL = NULL;

	pHKL = (HotKeyLinkObj*) pDescri->m_pComponentClass->CreateObject();
	if (pHKL)
		pHKL->InitNamespace();

	if (ppControlClass) 
		*ppControlClass = pDescri->m_pControlClass;

	return pHKL;
}

// istanzia un hotlink rappresentato dal suo namespace e ritorna al  chiamante
// la query richiesta preparata con i parametri indicati. Il protocollo usato  
// per comunicare è l'XML sia in richiesta che risposta:
// ---- RICHIESTA ----
// <HotKeyLink>	
//    <ControlData value="">  valore contenuto nel control
//	  <Param value="">        i parametri dell'Hotlink nell'ordine incontrato
// </HotKeyLink>
// 
// ---- RISPOSTA ----
// <HotKeyLink>	
//		<Query value="sqlquery">	stringa sql della query 
//	    <Param value=""  type="" basetype=""/> parametri
// </HotKeyLink>
//-----------------------------------------------------------------------------
CString CTbCommandManager::GetHotlinkQuery(
												const CString&	sNamespace, 
												const CString&	sParams,
												const int&		nAction,
												const CString	sFilter/*= _T("")*/,
												HotKeyLink* pHotlink /*= NULL*/
											)
{
	// login context locked
	if (IsTBLocked())
	{
		AfxGetDiagnostic()->Add(Strings::INVALID_COMPANY(), CDiagnostic::Error);
		return _T("");
	}

	if (sNamespace.IsEmpty())
		return _T("");

	// costriuisco il namespace e lo verifico
	CTBNamespace aNamespace(CTBNamespace::HOTLINK, sNamespace);	
	if (!aNamespace.IsValid())
		return _T("");

	// istanzio l'hotlink attraverso il suo namespace
	HotKeyLink* pHKL = pHotlink ? pHotlink : (HotKeyLink*) RunHotlink(aNamespace);
	if (!pHKL)
		return _T("");

	// senza il DataObj primario non posso nemmeno validare
	// la OnPrepareQuery, quindi esco 
	DataObj* pDataObj = pHKL->GetDataObj();
	if (!pDataObj)
	{
		delete pHKL;
		return _T("");
	}

	// mi sono stati passati dei parametri
	if (!sParams.IsEmpty())
	{
		CString sTemp;
		CXMLDocumentObject aRequest;
		try { aRequest.LoadXML(sParams); }	catch (CException*) {};

		// nodo <ControlData>
		CXMLNodeChildsList* pNodes = aRequest.GetNodeListByTagName(szXmlControlTag);

		if (pNodes && pNodes->GetSize() && pNodes->GetAt(0)->GetAttribute(szXmlValueAttribute, sTemp))
			pDataObj->AssignFromXMLString(sTemp);
		
		SAFE_DELETE(pNodes);
		
		// <Param>
		pNodes = aRequest.GetNodeListByTagName(szXmlParamTag);

		DataObjArray arParams;
		if (PrepareParamsToCustomize(aNamespace, pNodes, arParams))
			pHKL->Customize(arParams);
		
		SAFE_DELETE(pNodes);
	}

	HotKeyLink::SelectionType aSelType;
	switch (nAction)
	{
		case 0:	aSelType = HotKeyLink::UPPER_BUTTON;		break;
		case 1:	aSelType = HotKeyLink::LOWER_BUTTON;		break;
		case 2:	aSelType = HotKeyLink::COMBO_ACCESS;		break;
		case 3:	aSelType = HotKeyLink::DIRECT_ACCESS;		break;
	};

	CString sQuery;
	SqlParamArray* pParams = pHKL->GetQuery(aSelType, sQuery, sFilter);
		
	pHKL->CloseTable();
	delete pHKL;
	
	return sQuery;
}

//-----------------------------------------------------------------------------
BOOL CTbCommandManager::PrepareParamsToCustomize(
													const CTBNamespace& aNamespace, 
													CXMLNodeChildsList*	pNodes, 
													DataObjArray&		arParams
												)
{
	// non mi sono stati passati
	if (!pNodes)
		return TRUE;
	
	// cerco la libreria di appartenenza dell'oggetto
	AddOnLibrary* pAddOnLib = AfxGetAddOnLibrary(aNamespace);
	if (!pAddOnLib)
		return FALSE;

	// mi faccio dare il prototipo del metodo
	CFunctionDescription* pPrototype = (CFunctionDescription*) pAddOnLib->GetRegisteredObjectInfo(aNamespace);
	if (!pPrototype)
		return FALSE;

	// il numero di parametri passati può essere minore ma non maggiore
	if (pNodes->GetSize() > pPrototype->GetParameters().GetSize())
		return FALSE;

	// verifico la congruenza dei parametri secondo il prototipo
	CDataObjDescription* pParam;
	CXMLNode* pNode;

	for (int i=0; i <= pPrototype->GetParameters().GetUpperBound(); i++)
	{
		pParam = (CDataObjDescription*) pPrototype->GetParameters().GetAt(i);
		if (!pParam || i > pNodes->GetUpperBound())
			continue;
		
		pNode = pNodes->GetAt(i);
		CString sValue;

		if (!pNode || !pNode->GetAttribute(szXmlValueAttribute, sValue))
			continue;

		// può anche essere vuoto
		sValue.Trim(); 

		// creo il dataobj necessario
		DataObj* pDataObj = DataObj::DataObjCreate(pParam->GetDataType());
		pDataObj->AssignFromXMLString(sValue);
		arParams.Add (pDataObj);
	}

	return TRUE;
}

// DLL on demand
//-----------------------------------------------------------------------------
BOOL CTbCommandManager::AutoRegisterLibrary (const CString& sLibraryNamespace, HINSTANCE hLib)
{
	USES_UNATTENDED_DIAGNOSTIC ();

	return m_LibrariesLoader.AutoRegisterLibrary(sLibraryNamespace, hLib);
}

//-----------------------------------------------------------------------------
BOOL CTbCommandManager::LoadNeededLibraries (
												const CTBNamespace& aComponentNamespace,
												CTBNamespaceArray*	pLibDependencies /*= NULL*/, 
												LoadLibrariesMode	aMode /*LoadNeeded*/
											)
{
	USES_UNATTENDED_DIAGNOSTIC();

	BOOL bOk = m_LibrariesLoader.LoadNeededLibraries
				(
					aComponentNamespace, 
					pLibDependencies, 
					aMode
				);
	return bOk;
}

//-----------------------------------------------------------------------------
BOOL CTbCommandManager::Login(const CString& sAuthenticationToken)
{
	CLoginContext* pContext = AfxGetLoginContext(sAuthenticationToken);
	// if already logged, does nothing
	if (pContext)
	{
		((CLoginThread *)pContext)->WaitForStartupReady();
		return TRUE;
	}

	if (!AfxMultiThreadedLogin() && AfxGetApplicationContext()->GetLoginContextNumber() >= 1) 
	{
		AfxGetDiagnostic()->Add(_TB("Multiple logins are not available when multithreading is disabled"));
		return FALSE;
	}

	CLoginThread *pLoginThread = (CLoginThread *)RUNTIME_CLASS(CLoginThread)->CreateObject();
	
	return pLoginThread->Login(sAuthenticationToken, FALSE) && pLoginThread->IsValid();
}

//-------------------------------------------------------------------
DWORD CTbCommandManager::GetProcessID ()
{
	//ritorno della funzione (il process ID di TBLoader che serve a MenuManager)
	return  ::GetCurrentProcessId();
}

//-------------------------------------------------------------------
BOOL CTbCommandManager::GetCurrentUser(CString& strUser, CString& strCompany)
{
	if (!AfxGetLoginManager())
		return FALSE;

	strUser		= AfxGetLoginInfos()->m_strUserName;
	strCompany	= AfxGetLoginInfos()->m_strCompanyName;

	return  TRUE;
}


//-------------------------------------------------------------------
BOOL CTbCommandManager::CanCloseTB (BOOL bWithMsgBox /* = FALSE*/)
{
	return AfxGetApplicationContext()->CanClose();
}

//-------------------------------------------------------------------
void CTbCommandManager::CloseTB(BOOL bWithMsgBox /* = FALSE*/)
{
	if (!CanCloseTB(bWithMsgBox))
		return;

	::PostThreadMessage(AfxGetApp()->m_nThreadID, WM_QUIT, 0, NULL);
}

//-------------------------------------------------------------------
BOOL CTbCommandManager::CanChangeLogin (BOOL bLock /*FALSE*/)
{
	CLoginContext *pContext = AfxGetLoginContext();
	if (!pContext) return FALSE;

	if (!pContext->CanClose())
		return FALSE;

	// Task Builder lock status
	if (bLock)
		pContext->Lock ();
	
	return TRUE;
}
// 0=Success
// 1=Different oldToken
// 2=Error during closing connections
// 3=Error retrieving newToken informations in LoginManger
//-------------------------------------------------------------------
int CTbCommandManager::ChangeLogin (const CString&	sOldAuthenticationToken, const CString&	sNewAuthenticationToken, BOOL bUnLock /*FALSE*/)
{
	USES_DIAGNOSTIC();
	CLoginContext* pOldContext = AfxGetLoginContext(sOldAuthenticationToken);
	
	// different authentication token
	if (!pOldContext || _tcsicmp(pOldContext->GetName(), sOldAuthenticationToken))
	{
		AfxGetDiagnostic()->Add(_TB("The request to change the login data cannot be processed since Task Builder is currently logged in with another authentication token. Change Login cancelled."), CDiagnostic::Error);
		return 1;
	}
	
	if (!pOldContext->CanClose())
	{
		// Change Login failed due to disconnections
		AfxGetDiagnostic()->Add(_TB("The closing of all the available database connections is not achievable at this point. Change Login cancelled."), CDiagnostic::Error);
		return 2;
	}
	
	// First of all I disconnect the previous
	pOldContext->Close();

	// then I reconnect the new login
	if (!Login(sNewAuthenticationToken))
	{
		AfxGetDiagnostic()->Add(_TB("Error occurred during the call to Login Manager for retrieving information about the new user. Change Login cancelled."), CDiagnostic::Error);
		return 3;
	}
	
	AfxGetApplicationContext()->CloseAllLogins(TRUE);

	return 0;
}

//-------------------------------------------------------------------
BOOL CTbCommandManager::IsTBLocked	()
{
	return AfxGetLoginContext() && AfxGetLoginContext()->IsLocked ();
}

//-------------------------------------------------------------------
BOOL CTbCommandManager::LockTB (const CString& sAuthenticationToken)
{
	if (_tcsicmp(sAuthenticationToken, AfxGetAuthenticationToken()) == 0)
	{
		AfxGetLoginContext()->Lock();
		return TRUE;
	}

	return FALSE;
}

//-------------------------------------------------------------------
BOOL CTbCommandManager::UnLockTB (const CString& sAuthenticationToken)
{
	COleDbManager *pOleDBMng = AfxGetOleDbMng();
	if (
			_tcsicmp(sAuthenticationToken, AfxGetAuthenticationToken()) == 0
			&& pOleDBMng
			&& pOleDBMng->IsValid()
		)
	{
		AfxGetLoginContext()->UnLock();
		return TRUE;
	}

	return FALSE;
}

//-------------------------------------------------------------------
BOOL CTbCommandManager::TableExists (LPCTSTR pszSqlTableName, LPCTSTR pszSqlColumnName /*= NULL*/)
{
	SqlTableInfo* pTable = AfxGetOleDbMng()->GetPrimaryConnection()->GetTableInfo(pszSqlTableName);
	
	if (pTable == NULL)
		return FALSE;
	if (pszSqlColumnName) 
		return pTable->ExistColumn(pszSqlColumnName);
	return TRUE;
}

// Return Values:
// 0=Success
// 1=Invalid Authentication Token
// 2=Cannot Close Connections
//-------------------------------------------------------------------
int CTbCommandManager::DisconnectCompany (const CString& sAuthenticationToken)
{
	CLoginContext*	pLoginContext = AfxGetLoginContext();

	if (!pLoginContext || _tcsicmp(pLoginContext->GetName(), sAuthenticationToken))
	{
		AfxGetDiagnostic()->Add(_TB("The request to change the login data cannot be processed since Task Builder is currently logged in with another authentication token. Disconnect Company request cancelled."), CDiagnostic::Error);
		return 1;
	}

	COleDbManager* pOleDbMng = AfxGetOleDbMng();
	// I check if I can close connections to databases
	if (pOleDbMng && !pOleDbMng->CanCloseAllConnections ())
	{
		AfxGetDiagnostic()->Add(_TB("The closing of all the available database connections is not achievable at this point. Disconnect Company request cancelled."), CDiagnostic::Error);
		return 2;
	}

	// Auditing Connection CleanUp
	if (AfxGetLoginInfos()->m_bAuditing && AfxIsActivated(szExtensionsApp, TBAUDITING_ACT))
		RunFunction(_NS_WEB("Extensions.TbAuditing.TbAuditing.CloseAuditing"), NULL);

	pLoginContext->AttachOleDbMng (NULL);
	pLoginContext->Lock ();
	return 0;
}

//-------------------------------------------------------------------
int CTbCommandManager::ReconnectCompany (const CString& sAuthenticationToken)
{
	//Questo metodo in 3.0 ha cambiato radicalmente aspetto: in 2.x si occupava di ricrearea la sqlconnection
	//in 3.0 viene fatto tutto tramite Logoff e login del logincontext: l'unica cosa che rimaneva 
	//da fare era settare il Docked a true per ripristinare il comportamento
	CLoginContext* pLoginContext = AfxGetLoginContext(sAuthenticationToken);
	pLoginContext->SetDocked(TRUE);
	return 0; //Success
}	

//-------------------------------------------------------------------
void CTbCommandManager::InitOnDemandEnabled ()
{
	m_LibrariesLoader.InitOnDemandEnabled();
}

//-------------------------------------------------------------------
CString	CTbCommandManager::NativeConvert(const DataObj* pData)
{
	return AfxGetDefaultSqlConnection()->NativeConvert(pData);
}

//-------------------------------------------------------------------
CBehavioursRegistryService*	CTbCommandManager::GetBehaviourService (const CString& sEntity)
{
	CBehavioursRegistryEntity* pEntity = AfxGetBehavioursRegistry()->GetEntity(sEntity);
	if (!pEntity || pEntity->GetService().IsEmpty())
	{
		AfxGetDiagnostic()->Add(cwsprintf(_TB("Cannot find class for behaviour service for entity {0-%s}. Request discarded!"), sEntity), CDiagnostic::Error);
		return NULL;
	}
	
	CBehavioursRegistryService* pService = AfxGetBehavioursRegistry()->GetBehaviour(pEntity->GetService());
	if (!pService)
	{
		AfxGetDiagnostic()->Add(cwsprintf(_TB("Cannot find class for behaviour service for entity {0-%s}. Request discarded!"), sEntity), CDiagnostic::Error);
		return NULL;
	}

	// caricamento on demand
	if (!pService->GetClass()) 
	{
		CTBNamespace aNs(CTBNamespace::BEHAVIOUR,pEntity->GetService());
		LoadNeededLibraries(aNs);
		pService = AfxGetBehavioursRegistry()->GetBehaviour(pEntity->GetService());
	}

	if (!pService || !pService->GetClass())
		AfxGetDiagnostic()->Add(cwsprintf(_TB("Cannot find class for behaviour service for entity {0-%s}. Request discarded!"), sEntity), CDiagnostic::Error);

	return pService;
}

//-------------------------------------------------------------------
TBScript* CTbCommandManager::CreateTbScript (CFunctionDescription* pFun, SymTable* pSymTable)
{
	Block* pScript = new Block (NULL, pSymTable, NULL, FALSE, pFun);
	pScript->SetFun(pFun);
	return pScript;
}

//-------------------------------------------------------------------
BOOL CTbCommandManager::GetSchemaReportVariables(CTBNamespace& nsReport, DataTypeNamedArray& arReportColumns, DataTypeNamedArray& arReportAskFields)
{
	return CWoormDocMng::GetSchemaReportVariables(nsReport, arReportColumns, arReportAskFields);
}
