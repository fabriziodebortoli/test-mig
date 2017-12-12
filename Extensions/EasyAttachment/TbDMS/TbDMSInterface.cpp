#include "stdafx.h" 

#include <TBNameSolver\Diagnostic.h>
#include <TbNameSolver\CompanyContext.h>
#include <TbGenlib\baseapp.h>
#include <TBOledb\OleDbMng.h>
#include <TBOledb\sqlmark.h>
#include <TBOledb\sqlobject.h>
#include <TbGes\extdoc.h>
#include <TbGes\JsonFrame.h>
#include <TbGes\InterfaceMacros.h>
#include <TbGenlib\InterfaceMacros.h>

#include "TbRepositoryManager.h"
#include "TbDMSInterface.h"
#include "TbDMSFunctions.h"
#include "CommonObjects.h"
#include "SOSObjects.h"
#include "CDDMS.h"
#include "DMSSearchFilter.h"

#include "UIAttachment.h"
#include "DDMSSettings.h"
#include "UIDMSSettings.h"
#include "DDMSCategories.h"
#include "DSOSConfiguration.h"
#include "BDSOSDocSender.h"
#include "UISOSDocSender.h"
#include "BDSOSAdjustAttachments.h"
#include "UISOSAdjustAttachments.h"
#include "BDAcquisitionFromDevice.h"
#include "UIAcquisitionFromDevice.h"
#include "BDMassiveArchive.h"
#include "UIMassiveArchive.h"
#include "BDDMSRepository.h"
#include "UIDMSRepository.h"

#include "EasyAttachment\JsonForms\UIDMSRepository\IDD_DMSREPOSITORY_EXPLORER.hjson"
#include "EasyAttachment\JsonForms\UIDMSRepository\IDD_DMSREPOSITORY_BROWSER.hjson"
#include "EasyAttachment\JsonForms\UIDMSSettings\IDD_DMS_SETTINGS.hjson"
#include "EasyAttachment\JsonForms\UISOSConfiguration\IDD_SOS_CONFIGURATION.hjson"
#include "EasyAttachment\JsonForms\UIDMSCategories\IDD_DMSCATEGORIES.hjson"
#include "EasyAttachment\JsonForms\UISOSDocSender\IDD_SOSDOCSENDER_WIZARD.hjson"
#include "EasyAttachment\JsonForms\UISOSAdjustAttachments\IDD_SOSADJUSTATTACH_WIZARD.hjson"
#include "EasyAttachment\JsonForms\UIMassiveArchive\IDD_MASSIVEARCHIVE_WIZARD.hjson"
#include "EasyAttachment\JsonForms\UIAcquisitionFromDevice\IDD_ACQUISITION_FROM_DEVICE.hjson"
//#include "EasyAttachment\JsonForms\UIPaperyDlg\IDD_PAPERY.hjson"

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

#define DBRELEASE 403 //cambiare qui il database release di EasyAttachment

////////////////////////////////////////////////////////////////////////////
//				INIZIO definizione della interfaccia di Add-On
/////////////////////////////////////////////////////////////////////////////

// check esistenza valori vuoti nella colonna TBGuid e relativo aggiornamento
//-----------------------------------------------------------------------------
void CheckAndUpdateTBGuidInERPDocument()
{
	if (TbRepositoryManager::ExistEmptyTBGuidValuesInERPDocument())
		AfxGetTbRepositoryManager()->UpdateTBGuidInERPDocument();

	AfxPostQuitMessage(0); // chiudo il thread
}

// Devo fare un controllo del database release di EasyAttachment.
// Non posso utilizzare il meccanismo standard poichè il database da controllare è parallelo a quello aziendale
// registro solo la SqlDBMark per permettere questo controllo
//-----------------------------------------------------------------------------
BOOL InitDMSManager(FunctionDataInterface* pRDI)
{
	if (AfxIsRemoteInterface())
		return TRUE;

	DMSStatusEnum dmsStatus = DbInvalid;
	if (
		AfxIsActivated(TBEXT_APP, _NS_ACT("EasyAttachment")) &&
		!AfxGetOleDbMng()->GetDMSConnectionString().IsEmpty()
		)
	{
		CString strMsg;
		dmsStatus = TbRepositoryManager::CheckDMSStatus(DBRELEASE, strMsg);
		AfxGetOleDbMng()->SetDMSStatus(dmsStatus);
		if (!strMsg.IsEmpty())
			AfxGetDiagnostic()->Add(strMsg, CDiagnostic::Warning);
	}

	CLoginContext* pContext = AfxGetLoginContext();
	if (pContext && !pContext->GetDMSRepositoryManager())
	{
		TbRepositoryManager* pDMSRepositoryMng = new TbRepositoryManager();
		pContext->AttachDMSRepositoryManager(pDMSRepositoryMng);
	}

	if (dmsStatus == Valid)
	{
		// creo un thread separato per eseguire la procedura di aggiornamento 
		// dei valori della colonna TBGuid nella tabella DMS_ErpDocument
		CWinThread* pThread = AfxGetTbCmdManager()->CreateDocumentThread();
		AfxInvokeAsyncThreadGlobalProcedure(pThread->m_nThreadID, &CheckAndUpdateTBGuidInERPDocument);
	}

	return TRUE;
}

// Metodo con i controlli preventivi per decidere se agganciare il pannello degli allegati
//-----------------------------------------------------------------------------
BOOL CanAttachClientDoc(CAbstractFormDoc* pDoc)
{
	// check preliminare
	BOOL bCanAttach = AfxGetOleDbMng()->EasyAttachmentEnable() && pDoc && !pDoc->IsKindOf(RUNTIME_CLASS(CFinderDoc));
	if (!bCanAttach)
		return bCanAttach;
	
	// verifico se il namespace della batch e' abilitato alla SOS
	BOOL bIsDocNsInSOS = AfxGetTbRepositoryManager()->IsDocumentNamespaceInSOS(pDoc->GetNamespace().ToString());

	// check in caso di frame interamente in JSON 
	CJsonFrame* pJsonFrame = dynamic_cast<CJsonFrame*>(pDoc->GetFrame());
	if (pJsonFrame)
	{
		ViewCategory viewCategory = pJsonFrame->GetFrameDescription()->GetViewCategory();
		switch (viewCategory)
		{
		case VIEW_DATA_ENTRY:
			return TRUE; // i dataentry sono sempre abilitati
			break;
		case VIEW_BATCH:
		case VIEW_WIZARD:
			return bIsDocNsInSOS; // sono abilitati solo quelli riconosciuti
			break;
		case VIEW_ACTIVITY:
		case VIEW_FINDER:
		case VIEW_PARAMETER:
		default:
			return FALSE; // il resto non deve avere il clientdoc
		}
	}

	// check in caso di masterframe vecchia maniera
	bCanAttach = (!pDoc->GetMasterFrame() ||
		(pDoc->GetMasterFrame() && (!pDoc->GetMasterFrame()->IsKindOf(RUNTIME_CLASS(CBatchFrame)) || bIsDocNsInSOS)));
	
	return bCanAttach;
}

static AFX_EXTENSION_MODULE TbDMSDLL = { NULL, NULL };

//-----------------------------------------------------------------------------
BEGIN_ADDON_INTERFACE()

	//viene chiamato subito dopo il caricamento della DLL
	//-----------------------------------------------------------------------------
	virtual void AOI_InitDLL()
	{
		HINSTANCE hInstance = NULL;
		GET_DLL_HINSTANCE(hInstance);
		// Inizializzazione unica DLL di estensione serve per il walking delle risorse
		if (AfxInitExtensionModule(TbDMSDLL, hInstance))
			new CDynLinkLibrary(TbDMSDLL);
	}  

	//-----------------------------------------------------------------------------
	BEGIN_FAMILY_CLIENT_DOC()
		EXCLUDE_DOC_FROM_ATTACH_WHEN(!CanAttachClientDoc(pDoc))
		WHEN_FAMILY_SERVER_DOC(CAbstractFormDoc)
			ATTACH_FAMILY_CLIENT_DOC(CDDMS,	_NS_CD("CDDMS"))
		END_FAMILY_SERVER_DOC()
	END_FAMILY_CLIENT_DOC()

	BEGIN_TEMPLATE()
		//REGISTER_SLAVE_JSON_TEMPLATE(IDD_PAPERY)

		BEGIN_DOCUMENT(_NS_DOC("DDMSSettings"), TPL_NO_PROTECTION)
			REGISTER_MASTER_JSON_TEMPLATE(szDefaultViewMode, DDMSSettings, IDD_DMS_SETTINGS)
		END_DOCUMENT()

		BEGIN_DOCUMENT(_NS_DOC("DDMSCategories"), TPL_NO_PROTECTION)
			REGISTER_MASTER_JSON_TEMPLATE(szDefaultViewMode, DDMSCategories, IDD_DMSCATEGORIES)
		END_DOCUMENT()

		BEGIN_DOCUMENT(_NS_DOC("DMSRepositoryExplorer"), TPL_NO_PROTECTION)
			REGISTER_MASTER_JSON_TEMPLATE(szDefaultViewMode, BDDMSRepository, IDD_DMSREPOSITORY_EXPLORER)
			REGISTER_BKGROUND_TEMPLATE(szBackgroundViewMode, BDDMSRepository)
		END_DOCUMENT()	

		BEGIN_DOCUMENT(_NS_DOC("DMSRepositoryBrowser"), TPL_NO_PROTECTION)
			REGISTER_MASTER_JSON_TEMPLATE(szDefaultViewMode, BDDMSRepositoryBrowser, IDD_DMSREPOSITORY_BROWSER)
			REGISTER_BKGROUND_TEMPLATE(szBackgroundViewMode, BDDMSRepositoryBrowser)
		END_DOCUMENT()

		BEGIN_DOCUMENT(_NS_DOC("DSOSConfiguration"), TPL_NO_PROTECTION)
			REGISTER_MASTER_JSON_TEMPLATE(szDefaultViewMode, DSOSConfiguration, IDD_SOS_CONFIGURATION)
		END_DOCUMENT()

		BEGIN_DOCUMENT(_NS_DOC("SOSDocSender"), TPL_NO_PROTECTION)
			REGISTER_MASTER_JSON_TEMPLATE(szDefaultViewMode, BDSOSDocSender, IDD_SOSDOCSENDER_WIZARD)
			REGISTER_BKGROUND_TEMPLATE(szBackgroundViewMode, BDSOSDocSender)
		END_DOCUMENT()

		BEGIN_DOCUMENT(_NS_DOC("SOSAdjustAttachments"), TPL_NO_PROTECTION)
			REGISTER_MASTER_JSON_TEMPLATE(szDefaultViewMode, BDSOSAdjustAttachments, IDD_SOSADJUSTATTACH_WIZARD)
			REGISTER_BKGROUND_TEMPLATE(szBackgroundViewMode, BDSOSAdjustAttachments)
		END_DOCUMENT()
		
		BEGIN_DOCUMENT(_NS_DOC("MassiveArchive"), TPL_NO_PROTECTION)
			REGISTER_MASTER_JSON_TEMPLATE(szDefaultViewMode, BDMassiveArchive, IDD_MASSIVEARCHIVE_WIZARD)
			REGISTER_BKGROUND_TEMPLATE(szBackgroundViewMode, BDMassiveArchive)
		END_DOCUMENT()

		BEGIN_DOCUMENT(_NS_DOC("AcquisitionFromDevice"), TPL_NO_PROTECTION)
			REGISTER_MASTER_JSON_TEMPLATE(szDefaultViewMode, BDAcquisitionFromDevice, IDD_ACQUISITION_FROM_DEVICE)
		END_DOCUMENT()

	END_TEMPLATE()
	
	//-----------------------------------------------------------------------------
	BEGIN_TABLES()
		BEGIN_REGISTER_TABLES()
			REGISTER_VIRTUAL_TABLE(VBookmark)
			REGISTER_VIRTUAL_TABLE(VSettings)
			REGISTER_VIRTUAL_TABLE(VExtensionMaxSize)
			REGISTER_VIRTUAL_TABLE(VCategoryValues)
			REGISTER_VIRTUAL_TABLE(VArchivedDocument)
			REGISTER_VIRTUAL_TABLE(VAttachmentLink)
			REGISTER_VIRTUAL_TABLE(VSearchFieldCondition)		
			REGISTER_VIRTUAL_TABLE(VSOSConfiguration)
			REGISTER_VIRTUAL_TABLE(VSOSDocClass)
			REGISTER_VIRTUAL_TABLE(VSOSDocument)
			REGISTER_VIRTUAL_TABLE(VFileToAttach)
			REGISTER_VIRTUAL_TABLE(VSOSElaboration)
		END_REGISTER_TABLES()
	END_TABLES()

	//-----------------------------------------------------------------------------
	BEGIN_HOTLINK()
		DECLARE_HOTLINK(HKLDMSFields,			_NS_HKL("DMSFields"))
		DECLARE_HOTLINK(HKLSearchFieldIndexes,	_NS_HKL("DMSSearchFieldIndexes"))
	END_HOTLINK()

	//-----------------------------------------------------------------------------
	BEGIN_ITEMSOURCE()
		DECLARE_ITEMSOURCE(CAttachOptionForDocItemSource,	_NS_IS("AttachOptionForDocItemSource"))
		DECLARE_ITEMSOURCE(CAttachOptionForBatchItemSource,	_NS_IS("AttachOptionForBatchItemSource"))
		DECLARE_ITEMSOURCE(CBCOptionForDocItemSource,		_NS_IS("BCOptionForDocItemSource"))
		DECLARE_ITEMSOURCE(CBCOptionForBatchItemSource,		_NS_IS("BCOptionForBatchItemSource"))
		DECLARE_ITEMSOURCE(CBCTypeItemSource,				_NS_IS("BCTypeItemSource"))
		DECLARE_ITEMSOURCE(CFilesExtensionsItemSource,		_NS_IS("FilesExtensionsItemSource"))
		DECLARE_ITEMSOURCE(CCollectionsItemSource,			_NS_IS("CollectionsItemSource"))
		DECLARE_ITEMSOURCE(CSearchFieldsItemSource,			_NS_IS("SearchFieldsItemSource"))
		DECLARE_ITEMSOURCE(CSearchFieldValuesItemSource,	_NS_IS("SearchFieldValuesItemSource"))
		DECLARE_ITEMSOURCE(CBookmarkTypeItemSource,			_NS_IS("BookmarkTypeItemSource"))
		//DECLARE_ITEMSOURCE(CWorkersListBox,				_NS_IS("WorkersListBox")) //@@TODOMICHI: ripristinare la CStrCheckedListBox (da vedere con Luca)
		DECLARE_ITEMSOURCE(CSOSDocClassesItemSource,		_NS_IS("SOSDocClassesItemSource"))
		DECLARE_ITEMSOURCE(CSOSDocTypeItemSource,			_NS_IS("SOSDocTypeItemSource"))
		DECLARE_ITEMSOURCE(CSOSTaxJournalItemSource,		_NS_IS("SOSTaxJournalItemSource"))
		DECLARE_ITEMSOURCE(CSOSFiscalYearItemSource,		_NS_IS("SOSFiscalYearItemSource"))
		DECLARE_ITEMSOURCE(CExtensionsToScanItemSource,		_NS_IS("ExtensionsToScanItemSource"))
	END_ITEMSOURCE()

	//-----------------------------------------------------------------------------
	BEGIN_REGISTER_CONTROLS()
		BEGIN_CONTROLS_TYPE(DataType::Long)
			REGISTER_LISTBOX_CONTROL(AttachmentsListBox, _T("CAttachments ListBox"), CAttachmentsListBox, FALSE)
		END_CONTROLS_TYPE()

		BEGIN_CONTROLS_TYPE(DataType::Array) 
			REGISTER_LISTBOX_CONTROL(WorkersListBox, _T("Workers CheckListbox"), CWorkersListBox, FALSE)
		END_CONTROLS_TYPE()

		BEGIN_CONTROLS_TYPE(DataType::Null)
			REGISTER_CONTROL		(szMTreeView, DMSCategoriesTreeView, _T("DMSCategories TreeView"), CDMSCategoriesTreeViewAdv, 0, 0, 0, 0, TRUE)
			REGISTER_STATIC_CONTROL	(TBDMSViewer,			_T("TBPicture DMS Viewer"),			CTBDMSViewerCtrl,			FALSE)
			REGISTER_STATIC_CONTROL	(TBDMSBarcodeViewer,	_T("TBPicture DMS Barcode Viewer"), CTBDMSBarcodeViewerCtrl,	FALSE)
		END_CONTROLS_TYPE()
	END_REGISTER_CONTROLS()

	//-----------------------------------------------------------------------------
	BEGIN_FUNCTIONS()
		REGISTER_EVENT_HANDLER(_NS_EH("OnDSNChanged"), InitDMSManager)
	END_FUNCTIONS()

END_ADDON_INTERFACE()