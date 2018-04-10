#include "stdafx.h"

#include <TbGenlib\DMSAttachmentInfo.h>

#include <ExtensionsImages\CommonImages.h>

#include "TBRepositoryManager.h"
#include "BDDMSRepository.h"
#include "UIDMSRepository.h"
#include "TBDMSEnums.h"
#include "CDDMS.h"
#include "BDAcquisitionFromDevice.h"

#include "UIAttachment.hjson"

#include "EasyAttachment\JsonForms\UIDMSRepository\IDD_DMSREPOSITORY_EXPLORER.hjson"
#include "EasyAttachment\JsonForms\UIDMSRepository\IDD_DMSREPOSITORY_EXPLORER_TOOLBAR.hjson"
#include "EasyAttachment\JsonForms\UIDMSRepository\IDD_DMSREPOSITORY_BROWSER_TOOLBAR.hjson"
#include "EasyAttachment\JsonForms\UIDMSRepository\IDD_DMSREPOSITORY_RESULT_ROWVIEW.hjson"
#include "EasyAttachment\JsonForms\UIAcquisitionFromDevice\IDD_ACQUISITION_FROM_DEVICE.hjson"

using namespace System;
using namespace Microarea::EasyAttachment; 
using namespace Microarea::EasyAttachment::BusinessLogic;
using namespace Microarea::EasyAttachment::Components;

//===========================================================================
//							DMSRepositoryEvents
// ref class that manages communication between Event Handlers and C++ CWnd
//===========================================================================
void DMSRepositoryEvents::InitializeEvents(BDDMSRepository* pDoc, DMSOrchestrator^ dmsOrchestrator)
{
	m_pDoc = pDoc;
	if (dmsOrchestrator)
		dmsOrchestrator->ArchiveCompleted += gcnew EventHandler<AttachmentInfoEventArgs^>(this, &DMSRepositoryEvents::OnArchiveDocCompleted);
}

//-----------------------------------------------------------------------------
void DMSRepositoryEvents::OnArchiveDocCompleted(System::Object^, Microarea::EasyAttachment::Components::AttachmentInfoEventArgs^ eventArg)
{
	if (m_pDoc)
	{
		AttachmentInfo^ currAtt = eventArg->CurrentAttachment;
		m_pDoc->OnArchiveDocCompleted(CreateDMSAttachmentInfo(currAtt, FALSE));
	}
}

///////////////////////////////////////////////////////////////////////////////
//					DBTBookmarksCategory definition
///////////////////////////////////////////////////////////////////////////////
//=============================================================================
IMPLEMENT_DYNAMIC(DBTCategoriesBookmarks, DBTBookmarks)

//----------------------------------------------------------------------------
DBTCategoriesBookmarks::DBTCategoriesBookmarks(CRuntimeClass* pClass, CAbstractFormDoc*	pDocument)
	: 
	DBTBookmarks(pClass, pDocument) 
{
}

//----------------------------------------------------------------------------
void DBTCategoriesBookmarks::OnPrepareRow(int /*nRow*/, SqlRecord* pRec)
{
	ASSERT(pRec->IsKindOf(RUNTIME_CLASS(VBookmark)));
	VBookmark* pBookmarkRec = (VBookmark*)pRec;
	pBookmarkRec->l_GroupType = E_BOOKMARK_CATEGORY;
	pBookmarkRec->l_GroupType.SetReadOnly(TRUE);
}

//////////////////////////////////////////////////////////////////////////////
//				DBTArchivedDocuments implementation
//////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(DBTArchivedDocuments, DBTSlaveBuffered)

//-----------------------------------------------------------------------------	
DBTArchivedDocuments::DBTArchivedDocuments
	(
		CRuntimeClass*		pClass,
		CAbstractFormDoc*	pDocument
	)
	:
	DBTSlaveBuffered(pClass, pDocument, _NS_DBT("DBTArchivedDocuments"), ALLOW_EMPTY_BODY, FALSE)
{
	SetAllowFilter(TRUE); // abilito il filtro in memoria
}

//-----------------------------------------------------------------------------
DataObj* DBTArchivedDocuments::OnCheckPrimaryKey(int /*nRow*/, SqlRecord*)
{
	return NULL;
}

//-----------------------------------------------------------------------------
void DBTArchivedDocuments::OnPreparePrimaryKey(int nRow, SqlRecord* pSqlRec)
{
	ASSERT(pSqlRec->IsKindOf(RUNTIME_CLASS(VArchivedDocument)));
}

//-----------------------------------------------------------------------------
void DBTArchivedDocuments::SetCurrentRow(int nRow)
{
	DBTSlaveBuffered::SetCurrentRow(nRow);
}

// aggiungo manualmente i SqlRecord al DBT
//-----------------------------------------------------------------------------	
BOOL DBTArchivedDocuments::LocalFindData(BOOL bPrepareOld)
{
	RemoveAll();

	DMSAttachmentsList* pAttList = GetDocument()->m_pAttachmentsList;

	DMSAttachmentInfo* attInfo;
	for (int i = 0; i <= pAttList->GetUpperBound(); i++)
	{
		attInfo = pAttList->GetAt(i);
		VArchivedDocument* pRec = (VArchivedDocument*)AddRecord();
		pRec->l_IsSelected			= FALSE;
		pRec->l_ArchivedDocId		= attInfo->m_ArchivedDocId;
		pRec->l_Name				= attInfo->m_Name;
		pRec->l_Description			= attInfo->m_Description;
		pRec->l_IsAttachment		= (attInfo->m_attachmentID > 0);
		pRec->l_IsAttachmentBmp		= pRec->l_IsAttachment ? ExtensionsGlyph(szGlyphAttachment) : _T("");
		pRec->l_IsWoormReport		= attInfo->m_IsWoormReport;
		pRec->l_IsWoormReportBmp	= pRec->l_IsWoormReport ? ExtensionsGlyph(szGlyphWoormReport) : _T("");
		pRec->l_Worker				= attInfo->m_ModifiedBy;
		pRec->l_CreationDate		= attInfo->m_ArchivedDate;
		pRec->l_ModifiedDate		= attInfo->m_ModifiedDate;
		pRec->l_CheckOutWorker		= attInfo->m_CurrentCheckOutWorker;
		pRec->l_CheckOutWorkerBmp	= (pRec->l_CheckOutWorker == _T("-1") || pRec->l_CheckOutWorker.IsEmpty()) ? _T("") : ExtensionsGlyph(szGlyphCheckOut);
	}

	return TRUE;
}

//////////////////////////////////////////////////////////////////////////////
//				DBTAttachmentLinks implementation
//////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(DBTAttachmentLinks, DBTSlaveBuffered)

//-----------------------------------------------------------------------------	
DBTAttachmentLinks::DBTAttachmentLinks
	(
	CRuntimeClass*		pClass,
	CAbstractFormDoc*	pDocument
	)
	:
	DBTSlaveBuffered(pClass, pDocument, _NS_DBT("DBTAttachmentLinks"), ALLOW_EMPTY_BODY, FALSE)
{
}

//-----------------------------------------------------------------------------
DataObj* DBTAttachmentLinks::OnCheckPrimaryKey(int /*nRow*/, SqlRecord*)
{
	return NULL;
}

//-----------------------------------------------------------------------------
void DBTAttachmentLinks::OnPreparePrimaryKey(int nRow, SqlRecord* pSqlRec)
{
	ASSERT(pSqlRec->IsKindOf(RUNTIME_CLASS(VAttachmentLink)));
	// VAttachmentLink* pRec = (VAttachmentLink*) pSqlRec;
}

//-----------------------------------------------------------------------------
void DBTAttachmentLinks::SetCurrentRow(int nRow)
{
	DBTSlaveBuffered::SetCurrentRow(nRow);
}

//-----------------------------------------------------------------------------	
BOOL DBTAttachmentLinks::LocalFindData(BOOL bPrepareOld)
{
	RemoveAll();

	VArchivedDocument* pRec = GetDocument()->m_pDBTArchivedDocuments->GetCurrent();
	::Array* pLinksArray = (pRec) ? pRec->GetAttachmentLinks() : NULL;
	if (!pLinksArray)
		return TRUE;
	for (int i = 0; i < pLinksArray->GetCount(); i++)
	{
		VAttachmentLink* pLink = (VAttachmentLink*)pLinksArray->GetAt(i);
		VAttachmentLink* pNewRec = (VAttachmentLink*)AddRecord();
		*pNewRec = *pLink;
	}
	
	return TRUE;
}

/////////////////////////////////////////////////////////////////////////////
//				class BDDMSRepository Implementation
/////////////////////////////////////////////////////////////////////////////
//----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(BDDMSRepository, CAbstractFormDoc)

//----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(BDDMSRepository, CAbstractFormDoc)
	////{{AFX_MSG_MAP(BDDMSRepository)
	ON_CONTROL(BEN_ROW_CHANGED, IDC_DMSREPOSITORY_BE_RESULTS,					OnArchiveDocRowChanged)
	ON_CONTROL(BEN_ROW_CHANGED, IDC_DMSREPOSITORY_BE_SEARCHFIELDS_CONDITION,	OnSearchFieldRowChanged)
	ON_CONTROL(BEN_ROW_CHANGED, IDC_DMSREPOSITORY_DOC_BE_CATEGORIES,			OnBECategoriesRowChanged)

	ON_UPDATE_COMMAND_UI(ID_EXTDOC_PREV_ROW,							OnUpdateMoveToPrevRow) 
	ON_UPDATE_COMMAND_UI(ID_EXTDOC_NEXT_ROW,							OnUpdateMoveToNextRow) 
	ON_UPDATE_COMMAND_UI(ID_EXTDOC_FIRST_ROW,							OnUpdateMoveToFirstRow)
	ON_UPDATE_COMMAND_UI(ID_EXTDOC_LAST_ROW,							OnUpdateMoveToLastRow)

	ON_COMMAND			(ID_ARCHDOC_BTN_NEW_FROM_FILESYSTEM,			OnNewDocumentFromFileSystem)
	ON_COMMAND			(ID_ARCHDOC_BTN_NEW_FROM_DEVICE,				OnNewDocumentFromDevice)
	ON_COMMAND			(ID_SCANPROCESS_ENDED,							OnScanProcessEnded)

	ON_COMMAND			(ID_DMSREPOSITORY_TOOLBTN_EXTRACT_DOCUMENTS,	OnToolbarExtractDocuments)
	ON_UPDATE_COMMAND_UI(ID_DMSREPOSITORY_TOOLBTN_EXTRACT_DOCUMENTS,	OnUpdateToolbarExtractDocuments)
	ON_COMMAND			(ID_DMSREPOSITORY_TOOLBTN_UNDO_EXTRACTION,		OnToolbarUndoExtractDocuments)
	ON_UPDATE_COMMAND_UI(ID_DMSREPOSITORY_TOOLBTN_UNDO_EXTRACTION,		OnUpdateToolbarUndoExtractDocuments)

	ON_EN_VALUE_CHANGED	(IDC_DMSREPOSITORY_ALL_DOCUMENTS,				OnAllDocumentsChanged)			
	ON_EN_VALUE_CHANGED	(IDC_DMSREPOSITORY_WORKERS_SELECTWORKERS,		OnSelectWorkersChanged)			
	ON_EN_VALUE_CHANGED	(IDC_DMSREPOSITORY_WORKERS_SHOWDISABLED,		OnShowDisabledWorkersChanged)	
	ON_EN_VALUE_CHANGED	(IDC_DMSREPOSITORY_ADVSEL_ALLDOCUMENTS,			OnSelCollectionChanged)			
	ON_EN_VALUE_CHANGED	(IDC_DMSREPOSITORY_ADVSEL_COLLECTIONS,			OnCollectionChanged)			
	ON_EN_VALUE_CHANGED	(IDC_DMSREPOSITORY_DOC_BARCODE,					OnBarcodeChanged)				
	////}}AFX_MSG_MAP
END_MESSAGE_MAP()

//----------------------------------------------------------------------------
BDDMSRepository::BDDMSRepository()
	:
	dmsOrchestrator				(nullptr),
	dmsRepositoryEvents			(nullptr),
	m_bEditMode					(FALSE),
	m_bSelectWorkers			(FALSE),
	m_bShowDisabledWorkers		(FALSE),
	m_bFileNameAndDescription	(TRUE),
	m_bTags						(TRUE),
	m_bBarcode					(TRUE),
	m_bBookmarks				(TRUE),
	m_bDocumentContent			(TRUE),
	m_pAttachmentsList			(NULL),
	m_pDBTArchivedDocuments		(NULL),
	m_pSearchFilter				(NULL),
	m_pCurrentAttInfo			(NULL),
	m_pDBTCategoriesBookmarks	(NULL),
	m_pDBTAttachmentLinks		(NULL),
	m_pDBTSearchFieldsConditions(NULL),
	m_bOpenAsBrowser			(FALSE),
	m_pArchivedDocIdsArray		(NULL),
	m_bAllExtractedDoc			(FALSE),
	m_bFirstExtractedDoc		(TRUE),
	m_nTopNrDocuments			(50),
	m_bAllRepository			(TRUE),	
	m_bOnlyCollection			(FALSE),
	m_pCollectionList			(NULL),
	m_CollectionID				(-1),
	m_nIsAttachmentColumnIdx	(-1),
	m_nIsWoormReportColumnIdx	(-1),
	m_pCachedSearchFilter		(NULL),
	m_bEnableBarcode			(FALSE),
	m_pWorkersListBox			(NULL),
	m_pBarcodeViewer			(NULL),
	m_pBookmarkBE				(NULL)
{
	// inizializzo le date degli allegati dal primo giorno della settimana corrente alla data odierna
	m_FromDate.SetFullDate(); //devo considerare anche i minuti 
	m_ToDate.SetFullDate();

	m_FromDate = AfxGetApplicationDate();
	m_FromDate.SetWeekStartDate();
	m_ToDate = AfxGetApplicationDate();
}

//----------------------------------------------------------------------------
BDDMSRepository::~BDDMSRepository()
{
	SAFE_DELETE(m_pAttachmentsList);
	SAFE_DELETE(m_pSearchFilter);
	SAFE_DELETE(m_pCurrentAttInfo);
	SAFE_DELETE(m_pDBTArchivedDocuments);
	SAFE_DELETE(m_pDBTCategoriesBookmarks);
	SAFE_DELETE(m_pDBTAttachmentLinks);
	SAFE_DELETE(m_pCollectionList);
	SAFE_DELETE(m_pDBTSearchFieldsConditions);

	// faccio la delete solo se sono un repoexplorer, altrimenti lo cancella il clientdoc che me lo passa
	if (!m_bOpenAsBrowser)
		SAFE_DELETE(m_pCachedSearchFilter);

	if ((DMSOrchestrator^)dmsOrchestrator != nullptr)
	{
		delete dmsOrchestrator;
		dmsOrchestrator = nullptr;
	}
}

//-----------------------------------------------------------------------------
BOOL BDDMSRepository::OnAttachData()
{
	SetFormTitle(m_bOpenAsBrowser ? _TB("Repository Browser") : _TB("Repository Explorer"));
	SetFormName(m_bOpenAsBrowser ? _TB("Repository Browser") : _TB("Repository Explorer"));

	dmsOrchestrator = gcnew DMSOrchestrator();
	AfxGetTbRepositoryManager()->InitializeManager(this);
	
	dmsRepositoryEvents = gcnew DMSRepositoryEvents();
	dmsRepositoryEvents->InitializeEvents(this, dmsOrchestrator);

	// istanzio l'AttachmentInfo corrente
	m_pCurrentAttInfo = new DMSAttachmentInfo();
	// per avere l'informazione anche a livello di file JSON per show/hide
	m_bEnableBarcode = (AfxGetTbRepositoryManager()->GetDMSSettings()->GetSettings()->l_EnableBarcode);

	// elenco dei collector da cui vengono poi estrapolate le collection
	m_pCollectionList = AfxGetTbRepositoryManager()->GetUsedCollections();

	GetHotLink<HKLDMSFields>(L"DMSFields")->FilterForCollectionID(&m_CollectionID);

	// aggancio i DBT
	m_pDBTSearchFieldsConditions	= new DBTSearchFieldsConditions	(RUNTIME_CLASS(VSearchFieldCondition), this);
	m_pDBTArchivedDocuments			= new DBTArchivedDocuments		(RUNTIME_CLASS(VArchivedDocument), this);
	m_pDBTCategoriesBookmarks		= new DBTCategoriesBookmarks	(RUNTIME_CLASS(VBookmark), this);
	m_pDBTAttachmentLinks			= new DBTAttachmentLinks		(RUNTIME_CLASS(VAttachmentLink), this);

	if (!m_pCachedSearchFilter)
		m_pCachedSearchFilter = new CSearchFilter();
	m_pCachedSearchFilter->SetSearchFieldsConditions(m_pDBTSearchFieldsConditions);
	
	InitSelections();

	// inizializzazione variabili e bodyedit JSON
	InitJSONVariables();

	return TRUE;
}

//----------------------------------------------------------------------------
void BDDMSRepository::InitJSONVariables()
{
	DECLARE_VAR_JSON(bAllExtractedDoc);
	DECLARE_VAR_JSON(bFirstExtractedDoc);
	DECLARE_VAR_JSON(nTopNrDocuments);
	DECLARE_VAR_JSON(FromDate);
	DECLARE_VAR_JSON(ToDate);
	DECLARE_VAR_JSON(sFileExtension);

	DECLARE_VAR_JSON(bSelectWorkers);
	DECLARE_VAR_JSON(bShowDisabledWorkers);
	DECLARE_VAR_JSON(Workers);

	DECLARE_VAR_JSON(FreeText);
	DECLARE_VAR_JSON(bFileNameAndDescription);
	DECLARE_VAR_JSON(bTags);
	DECLARE_VAR_JSON(bBarcode);
	DECLARE_VAR_JSON(bBookmarks);
	DECLARE_VAR_JSON(bDocumentContent);

	DECLARE_VAR_JSON(bAllRepository);
	DECLARE_VAR_JSON(bOnlyCollection);
	DECLARE_VAR_JSON(DocNamespace);
	
	DECLARE_VAR_JSON(bOpenAsBrowser);
	DECLARE_VAR_JSON(bEnableBarcode);

	// variabili rowview
	DECLARE_VAR(_T("DocDescription"),		m_pCurrentAttInfo->m_Description);
	DECLARE_VAR(_T("DocOriginalPath"),		m_pCurrentAttInfo->m_OriginalPath);
	DECLARE_VAR(_T("DocSize"),				m_pCurrentAttInfo->m_Size);
	DECLARE_VAR(_T("DocArchivedDate"),		m_pCurrentAttInfo->m_ArchivedDate);
	DECLARE_VAR(_T("DocCreatedBy"),			m_pCurrentAttInfo->m_CreatedBy);
	DECLARE_VAR(_T("DocModifiedBy"),		m_pCurrentAttInfo->m_ModifiedBy);
	DECLARE_VAR(_T("DocTemporaryPathFile"), m_pCurrentAttInfo->m_TemporaryPathFile);
	DECLARE_VAR(_T("DocFreeTag"),			m_pCurrentAttInfo->m_FreeTag);
	DECLARE_VAR(_T("DocBarcodeValue"),		m_pCurrentAttInfo->m_BarcodeValue);
	DECLARE_VAR(_T("DocBarcodeValueViewer"),m_pCurrentAttInfo->m_BarcodeValue); 
	//
	// devo registrare esplicitamente i BodyEdit custom!!!!!
	RegisterControl(IDC_DMSREPOSITORY_BE_SEARCHFIELDS_CONDITION,	RUNTIME_CLASS(CSearchFieldsConditionsBodyEdit));
	RegisterControl(IDC_DMSREPOSITORY_BE_RESULTS,					RUNTIME_CLASS(CArchivedDocumentsBodyEdit));
	RegisterControl(IDC_DMSREPOSITORY_BE_LINKS,						RUNTIME_CLASS(CAttachmentLinksBodyEdit));
	RegisterControl(IDC_DMSREPOSITORY_DOC_BE_CATEGORIES,			RUNTIME_CLASS(CBookmarksBodyEdit));
	//
}

//----------------------------------------------------------------------------
void BDDMSRepository::OnParsedControlCreated(CParsedCtrl* pCtrl)
{
	__super::OnParsedControlCreated(pCtrl);

	UINT nIDC = pCtrl->GetCtrlID();

	if (nIDC == IDC_DMSREPOSITORY_WORKERS_LISTBOX)
	{
		//m_pWorkersListBox = (CWorkersListBox*)((CParsedListBox*)pCtrl)->GetItemSource();
		m_pWorkersListBox = (CWorkersListBox*)pCtrl;
		if (m_pWorkersListBox)
			m_pWorkersListBox->LoadWorkers();
		return;
	}

	if (nIDC == IDC_DMSREPOSITORY_DOC_BARCODEVIEWER)
	{
		m_pBarcodeViewer = dynamic_cast<CTBDMSBarcodeViewerCtrl*>(pCtrl);
		if (!m_pBarcodeViewer) return;
		m_pBarcodeViewer->SetToolStripVisibility(FALSE);
		m_pBarcodeViewer->EnablePreviewNotAvailable(FALSE);
		m_pBarcodeViewer->SetSkipRecalcCtrlSize(TRUE);
		m_pBarcodeViewer->SetBarcode(m_pCurrentAttInfo->m_BarcodeValue, m_pCurrentAttInfo->m_BarcodeType); // serve per la visualizzazione del barcode
		return;
	}
}

//-----------------------------------------------------------------------------
void BDDMSRepository::OnBECategoriesRowChanged()
{
	if (m_pBookmarkBE)
		m_pBookmarkBE->OnBookmarkRowChanged();
}

//-----------------------------------------------------------------------------
void BDDMSRepository::OnColumnInfoCreated(ColumnInfo* pColInfo)
{
	__super::OnColumnInfoCreated(pColInfo);
	if (!pColInfo)
		return;
	
	UINT nIDC = pColInfo->GetCtrlID();

	if (nIDC == IDC_ATT_BE_BOOKMARK_DESCRI)
	{
		m_pSearchFieldsItemSource = (CSearchFieldsItemSource*)pColInfo->GetParsedCtrl()->GetItemSource();
		m_pSearchFieldsItemSource->SetFilterCategory();
		return;
	}

	if (nIDC == IDC_ATT_BE_BOOKMARK_VALUE)
	{
		m_pSearchFieldValuesItemSource = (CSearchFieldValuesItemSource*)pColInfo->GetParsedCtrl()->GetItemSource();
		m_pSearchFieldsItemSource->SetSearchFieldList(((DBTBookmarks*)m_pDBTCategoriesBookmarks)->m_pSearchFields);
		return;
	}

	// gestione filtermemory nel BodyEdit
	if (nIDC == IDC_DMSREPOSITORY_BE_ARCHDOC_IS_ATTACHMENT)
	{
		m_nIsAttachmentColumnIdx = pColInfo->GetDataInfoIdx();
		return;
	}
	if (nIDC == IDC_DMSREPOSITORY_BE_ARCHDOC_IS_WOORM_REPORT)
	{
		m_nIsWoormReportColumnIdx = pColInfo->GetDataInfoIdx();
		return;
	}
	//
}

//-----------------------------------------------------------------------------
void BDDMSRepository::CustomizeBodyEdit(CBodyEdit* pBodyEdit)
{
	__super::CustomizeBodyEdit(pBodyEdit);

	if (pBodyEdit->GetNamespace().GetObjectName() == _NS_BE("Bookmarks"))
	{
		m_pBookmarkBE = (CBookmarksBodyEdit*)pBodyEdit;
		if (m_pBookmarkBE)
		{
			m_pBookmarkBE->m_pSearchFieldsItemSource		= m_pSearchFieldsItemSource;
			m_pBookmarkBE->m_pSearchFieldValuesItemSource	= m_pSearchFieldValuesItemSource;
		}
		return;
	}

	if (pBodyEdit->GetNamespace().GetObjectName() == _NS_BE("BEArchivedDocuments"))
	{
		CArchivedDocumentsBodyEdit* pArchivedDocsBE = (CArchivedDocumentsBodyEdit*)pBodyEdit;
		if (pArchivedDocsBE)
		{
			//@@TODOMICHI: a regime questo deve sparire!
			pArchivedDocsBE->m_pBEBtnEditSave		= pArchivedDocsBE->m_HeaderToolBar.FindButton(ID_ARCHDOC_BTN_EDIT);
			pArchivedDocsBE->m_pBEBtnSelectDeselect = pArchivedDocsBE->m_HeaderToolBar.FindButton(ID_ARCHDOC_BTN_SELECT_DESELECT);
			pArchivedDocsBE->m_pBEBtnCheckInOut		= pArchivedDocsBE->m_HeaderToolBar.FindButton(ID_ARCHDOC_BTN_CHECKOUT);
			pArchivedDocsBE->m_pBEBtnUndoCheckInOut = pArchivedDocsBE->m_HeaderToolBar.FindButton(ID_ARCHDOC_BTN_UNDO_CHECKOUT);
			pArchivedDocsBE->m_pBEBtnMemoryFilters	= pArchivedDocsBE->m_HeaderToolBar.FindButton(ID_ARCHDOC_BTN_FILTERS);
		}
		return;
	}
}

//-----------------------------------------------------------------------------
CTilePanel* BDDMSRepository::GetTilePanel(UINT nIDD)
{
	CMasterFormView* pView = (CMasterFormView*)GetFirstView();
	if (!pView)
		return NULL;

	CTileGroup* pGroup = pView->GetTileGroup(IDC_TG_DMSREPOSITORY);
	if (!pGroup)
		return NULL;

	CTilePanel* pTile = pGroup->GetTilePanel(nIDD); 
	if (pTile)
		return pTile;

	return NULL;
}

//-----------------------------------------------------------------------------
void BDDMSRepository::OnPrepareAuxData(CTileDialog* pTile)
{
	UINT nIDC = (UINT)pTile->GetDialogID();
	// update della preview del documento o del barcode
	if (nIDC == IDD_TD_RW_DMSREPOSITORY_DOC_PREVIEW || nIDC == IDD_TD_RW_DMSREPOSITORY_DOC_BARCODE)
		DoAttachmentInfoChanged();
}

// compone la stringa da visualizzare nella caption della rowview
//-----------------------------------------------------------------------------
CString	BDDMSRepository::OnGetCaption(CAbstractFormView* pView)
{
	CString sCaption = _T("");

	if (pView->GetNamespace().GetObjectName() == _T("DMSRepositoryRowView"))
	{
		VArchivedDocument* pRec = (VArchivedDocument*)m_pDBTArchivedDocuments->GetCurrentRow();
		if (pRec)
			sCaption = _TB("File name: ") + pRec->l_Name;
	}

	return sCaption;
}

//-----------------------------------------------------------------------------
void BDDMSRepository::OnUpdateMoveToPrevRow(CCmdUI* pCmdUI)
{
	pCmdUI->Enable(!m_bEditMode);
}

//-----------------------------------------------------------------------------
void BDDMSRepository::OnUpdateMoveToNextRow(CCmdUI* pCmdUI)
{
	pCmdUI->Enable(!m_bEditMode);
}

//-----------------------------------------------------------------------------
void BDDMSRepository::OnUpdateMoveToFirstRow(CCmdUI* pCmdUI)
{
	pCmdUI->Enable(!m_bEditMode);
}

//-----------------------------------------------------------------------------
void BDDMSRepository::OnUpdateMoveToLastRow(CCmdUI* pCmdUI)
{
	pCmdUI->Enable(!m_bEditMode);
}

//-----------------------------------------------------------------------------
void BDDMSRepository::OnAllDocumentsChanged()
{
	m_nTopNrDocuments.SetReadOnly(m_bAllExtractedDoc);
	UpdateDataView();
}

//-----------------------------------------------------------------------------
void BDDMSRepository::OnSelectWorkersChanged()
{
	m_Workers.SetReadOnly(!m_bSelectWorkers);
	m_bShowDisabledWorkers.SetReadOnly(!m_bSelectWorkers);
	m_Workers.Clear();
	UpdateDataView();
}

//-----------------------------------------------------------------------------
void BDDMSRepository::OnShowDisabledWorkersChanged()
{
	if (m_pWorkersListBox)
		m_pWorkersListBox->LoadWorkers(m_bShowDisabledWorkers);
	m_Workers.Clear();
	UpdateDataView();
}

//-----------------------------------------------------------------------------
void BDDMSRepository::OnSelCollectionChanged()
{
	DoSelCollectionChanged();
}

//-----------------------------------------------------------------------------
void BDDMSRepository::OnCollectionChanged()
{
	DoCollectionChanged();
}

//-----------------------------------------------------------------------------
void BDDMSRepository::OnBarcodeChanged()
{
	if (!AfxGetTbRepositoryManager()->IsValidEABarcodeValue(m_pCurrentAttInfo->m_BarcodeValue))
	{
		m_pCurrentAttInfo->m_BarcodeValue.Clear();
		UpdateDataView();
	}
}

//-----------------------------------------------------------------------------
void BDDMSRepository::DoAttachmentInfoChanged()
{
	if (!m_pCurrentAttInfo)
		return;

	CView* pRowView = ViewAlreadyPresent(RUNTIME_CLASS(CJsonRowView));
	if (pRowView)
	{
		CJsonRowView* pJsonView = (CJsonRowView*)pRowView;
		if (!pJsonView)
			return;

		if (pJsonView->GetTileDialog(IDD_TD_RW_DMSREPOSITORY_DOC_PREVIEW))
		{
			// se il file temporaneo non esiste lo creo e poi lo visualizzo
			if (m_pCurrentAttInfo->m_TemporaryPathFile.IsEmpty())
				m_pCurrentAttInfo->m_TemporaryPathFile = AfxGetTbRepositoryManager()->GetArchivedDocTempFile(m_pCurrentAttInfo->m_ArchivedDocId);
		}

		if (pJsonView->GetTileDialog(IDD_TD_RW_DMSREPOSITORY_DOC_BARCODE))
		{
			if (m_pBarcodeViewer)
				m_pBarcodeViewer->SetBarcode(m_pCurrentAttInfo->m_BarcodeValue, m_pCurrentAttInfo->m_BarcodeType);
		}
		UpdateDataView();
	}
}

//-----------------------------------------------------------------------------
BOOL BDDMSRepository::CanRunDocument()
{
	if (!AfxGetOleDbMng()->EasyAttachmentEnable())
	{
		AfxMessageBox(_TB("Impossible to open Repository Explorer!\r\nPlease, check in Administration Console if this company uses DMS."));
		return FALSE;
	}

	return TRUE;
}

//-----------------------------------------------------------------------------
void BDDMSRepository::OnArchiveDocCompleted(DMSAttachmentInfo* attachmentInfo)
{
	if (!attachmentInfo) return;

	BOOL bRowAlreadyExists = FALSE;

	DMSAttachmentInfo* attachInfo = attachmentInfo;
	VArchivedDocument* pRec;
	for (int i = 0; i <= m_pDBTArchivedDocuments->GetUpperBound(); i++)
	{
		pRec = (VArchivedDocument*)m_pDBTArchivedDocuments->GetRow(i);
		if (pRec->l_ArchivedDocId == attachInfo->m_ArchivedDocId)
		{
			m_pDBTArchivedDocuments->SetCurrentRow(i);
			bRowAlreadyExists = TRUE;
			break;
		}
	}

	if (!bRowAlreadyExists)
	{
		pRec = (VArchivedDocument*)m_pDBTArchivedDocuments->InsertRecord(0);
		pRec->l_IsSelected = FALSE;
		pRec->l_ArchivedDocId = attachInfo->m_ArchivedDocId;
		pRec->l_Name = attachInfo->m_Name;
		pRec->l_Description = attachInfo->m_Description;
		pRec->l_IsAttachment = (attachInfo->m_attachmentID > 0);
		pRec->l_IsAttachmentBmp = pRec->l_IsAttachment ? ExtensionsGlyph(szGlyphAttachment) : _T("");
		pRec->l_IsWoormReport = attachInfo->m_IsWoormReport;
		pRec->l_IsWoormReportBmp = pRec->l_IsWoormReport ? ExtensionsGlyph(szGlyphWoormReport) : _T("");
		pRec->l_Worker = attachInfo->m_ModifiedBy;
		pRec->l_CreationDate = attachInfo->m_ArchivedDate;
		pRec->l_ModifiedDate = attachInfo->m_ModifiedDate;
		pRec->l_CheckOutWorker = attachInfo->m_CurrentCheckOutWorker;
		pRec->l_CheckOutWorkerBmp = (pRec->l_CheckOutWorker == _T("-1") || pRec->l_CheckOutWorker.IsEmpty()) ? _T("") : ExtensionsGlyph(szGlyphCheckOut);

		m_pDBTArchivedDocuments->SetCurrentRow(0);
	}

	delete attachmentInfo;
	OnArchiveDocRowChanged();
}

//-----------------------------------------------------------------------------
void BDDMSRepository::DisableControlsForBatch()
{
	if (AfxGetTbRepositoryManager()->GetDMSSettings()->GetSettings()->l_ShowOnlyMyArchivedDocs && !AfxGetLoginInfos()->m_bAdmin)
	{
		m_bSelectWorkers = TRUE;
		m_bShowDisabledWorkers = TRUE;
		m_Workers = cwsprintf(_T("%d"), AfxGetWorkerId());
		m_Workers.SetReadOnly();
		m_Workers.SetReadOnly();
		m_bShowDisabledWorkers.SetReadOnly();
		m_bSelectWorkers.SetReadOnly();
	}
	else
	{
		m_Workers.SetReadOnly(!m_bSelectWorkers);
		m_bShowDisabledWorkers.SetReadOnly(!m_bSelectWorkers);
	}

	if (!AfxGetTbRepositoryManager()->GetDMSSettings()->GetSettings()->l_EnableBarcode)
	{
		m_bBarcode = FALSE;
		m_bBarcode.SetReadOnly();
	}

	if (!AfxGetTbRepositoryManager()->GetDMSSettings()->GetSettings()->l_EnableFTS)
	{
		m_bDocumentContent = FALSE;
		m_bDocumentContent.SetReadOnly();
	}

	m_DocNamespace.SetReadOnly(m_bAllRepository);
	EnableCurrentAttachControls();
}

//-----------------------------------------------------------------------------
void BDDMSRepository::EnableCurrentAttachControls()
{
	m_pCurrentAttInfo->m_Description.SetReadOnly(!m_bEditMode);
	m_pCurrentAttInfo->m_FreeTag.SetReadOnly(!m_bEditMode);
	m_pCurrentAttInfo->m_BarcodeValue.SetReadOnly(!m_bEditMode);
	m_pDBTCategoriesBookmarks->SetReadOnly(!m_bEditMode);
	m_pDBTArchivedDocuments->SetReadOnly(m_bEditMode);
	UpdateDataView();
}

//-----------------------------------------------------------------------------
void BDDMSRepository::ExtractArchivedDocuments()
{
	if (GetNotValidView(TRUE))
		return;

	if (m_FromDate.IsEmpty() || m_ToDate.IsEmpty())
	{
		Message(_TB("The date period is not complete, specify from and to dates."));
		return;
	}

	if (m_FromDate > m_ToDate)
	{
		Message(_TB("The date period is not consistent, check from and to dates."));
		return;
	}

	m_FromDate.SetTime(0, 0, 0);
	m_ToDate.SetTime(23, 59, 59);

	// riempio la struttura con tutti i filtri impostati dall'utente
	CreateCacheSearchFilters();

	// richiamo il metodo esposto dal dmsorchestrator per avviare la ricerca
	SAFE_DELETE(m_pAttachmentsList);
	m_pAttachmentsList = AfxGetTbRepositoryManager()->GetArchivedDocuments(m_pCachedSearchFilter);

	// se non ho estratto nulla visualizzo un msg e faccio la Clear di tutti i controlli
	if (!m_pAttachmentsList || m_pAttachmentsList->GetCount() == 0)
	{
		Message(_TB("No documents match your search filters"));
		m_pDBTArchivedDocuments->RemoveAll();
		m_pCurrentAttInfo->Clear();
		m_pDBTAttachmentLinks->RemoveAll();
		return;
	}

	// forzo il riempimento del DBT con elenco documenti archiviati estratti dalla ricerca
	m_pDBTArchivedDocuments->LocalFindData(FALSE);

	DoExtractDocumentsEnded();
}

// riempie la classe dei filtri cachati in base alle selezioni effettuate nella form
// dopo che ho eseguito la prima estrazione dati
//-----------------------------------------------------------------------------
void BDDMSRepository::CreateCacheSearchFilters()
{
	// se sto aprendo il repoexplorer e l'oggetto non e' ancora valido allora lo creo
	if (!m_bOpenAsBrowser && !m_pCachedSearchFilter)
	{
		m_pCachedSearchFilter = new CSearchFilter();
		m_pCachedSearchFilter->SetSearchFieldsConditions(m_pDBTSearchFieldsConditions);
	}

	// filtri sulle date
	m_pCachedSearchFilter->m_StartDate	= m_FromDate;
	m_pCachedSearchFilter->m_EndDate	= m_ToDate;

	// filtri nr documenti
	if (m_bFirstExtractedDoc && !m_nTopNrDocuments.IsEmpty() && m_nTopNrDocuments > 0)
		m_pCachedSearchFilter->m_TopDocsNumber = m_nTopNrDocuments;
	else
		m_pCachedSearchFilter->m_TopDocsNumber = 0;

	// filtri sulle estensioni
	m_pCachedSearchFilter->m_DocExtensionType = m_sFileExtension;
	// filtri per il testo libero
	m_pCachedSearchFilter->m_FreeTag = m_FreeText;
	// filtri sulla collection
	m_pCachedSearchFilter->m_CollectionID = m_CollectionID;

	// filtri per singoli bookmark
	m_pCachedSearchFilter->CreateSearchFieldsConditions();

	// prima pulisco i vari filtri e poi assegno in base alle scelte dell'utente
	m_pCachedSearchFilter->SetSearchNone();
	// se tutti i flag sono a TRUE imposto All
	if (m_bFileNameAndDescription && m_bTags && m_bBarcode && m_bBookmarks && m_bDocumentContent)
		m_pCachedSearchFilter->SetSearchAll();
	else // se tutti i flag sono a FALSE imposto None e metto ad empty il free text
		if (!m_bFileNameAndDescription && !m_bTags && !m_bBarcode && !m_bBookmarks && !m_bDocumentContent)
		{
			m_pCachedSearchFilter->SetSearchNone();
			m_pCachedSearchFilter->m_FreeTag.Clear();
		}
		else
		{
			// altrimenti imposto i singoli flag
			m_pCachedSearchFilter->SetSearchLocation(m_bFileNameAndDescription, m_pCachedSearchFilter->SearchLocation::NameAndDescription);
			m_pCachedSearchFilter->SetSearchLocation(m_bTags, m_pCachedSearchFilter->SearchLocation::Tags);
			m_pCachedSearchFilter->SetSearchLocation(m_bBarcode, m_pCachedSearchFilter->SearchLocation::Barcode);
			m_pCachedSearchFilter->SetSearchLocation(m_bBookmarks, m_pCachedSearchFilter->SearchLocation::AllBookmarks);
			m_pCachedSearchFilter->SetSearchLocation(m_bDocumentContent, m_pCachedSearchFilter->SearchLocation::Content);
		}

	// filtri sui workers
	m_pCachedSearchFilter->m_arWorkers.RemoveAll(); //pulisco le vecchie selezioni

	if (!m_Workers.IsEmpty()) // se ho selezionato dei worker
	{
		// la variabile m_Workers contiene una lista di workerid intervallati da ;
		int nTokenPos = 0;
		CString sToken = m_Workers.Str().Tokenize(_T(";"), nTokenPos);
		while ((nTokenPos - 1) <= m_Workers.Str().GetLength() && !sToken.Trim().IsEmpty())
		{
			m_pCachedSearchFilter->m_arWorkers.Add(sToken);
			sToken = m_Workers.Str().Tokenize(_T(";"), nTokenPos);
		}
	}
}

// alla fine dell'estrazione dati chiudo tutti i panel con le selezioni
//-----------------------------------------------------------------------------
void BDDMSRepository::DoExtractDocumentsEnded()
{
	CTilePanel* pPanel = GetTilePanel(IDC_TP_DMSREPOSITORY_QUICKSEARCH);
	if (pPanel)
		pPanel->SetCollapsed(TRUE);

	pPanel = GetTilePanel(IDC_TP_DMSREPOSITORY_ADVANCEDSEARCH);
	if (pPanel)
		pPanel->SetCollapsed(TRUE);

	// propago un evento al bodyedit dei risultati per rimuovere i filtri in memoria
	GetFrame()->SendMessageToDescendants(WM_COMMAND, ID_DOCUMENT_EXTRACTION_ENDED);
}

// se eseguo operazioni di edit sul documento disabilito i pannelli con le selezioni
//-----------------------------------------------------------------------------
void BDDMSRepository::DoEditDocumentChanged()
{
	CTilePanel* pPanel = GetTilePanel(IDC_TP_DMSREPOSITORY_QUICKSEARCH);
	if (pPanel)
		pPanel->Enable(!m_bEditMode);

	pPanel = GetTilePanel(IDC_TP_DMSREPOSITORY_ADVANCEDSEARCH);
	if (pPanel)
		pPanel->Enable(!m_bEditMode);
}

//-----------------------------------------------------------------------------
void BDDMSRepository::OnNewDocumentFromFileSystem()
{
	TCHAR szBuff[8128];
	szBuff[0] = '\0';

	CFileDialog fileDialog(TRUE, NULL, NULL, OFN_ALLOWMULTISELECT, NULL);
	fileDialog.m_ofn.lpstrFile = szBuff;
	fileDialog.m_ofn.nMaxFile = 4096;

	if (fileDialog.DoModal() != IDOK)
		return;

	CStringArray* pArray = new CStringArray();

	POSITION p = fileDialog.GetStartPosition();
	while (p != NULL)
	{
		CString strFile = fileDialog.GetNextPathName(p);
		pArray->Add(strFile);
	}

	ArchiveFiles(pArray);
	SAFE_DELETE(pArray);
}

//-----------------------------------------------------------------------------
void BDDMSRepository::OnNewDocumentFromDevice()
{
	m_pAcquisitionFromDevice =
		(BDAcquisitionFromDevice*)AfxGetTbCmdManager()->RunDocument(_NS_DOC("Extensions.EasyAttachment.TbDMS.AcquisitionFromDevice"), szDefaultViewMode, NULL, NULL, this);

	GetMasterFrame()->EnableWindow(FALSE);
	AfxGetTbCmdManager()->WaitDocumentEnd(m_pAcquisitionFromDevice);
	GetMasterFrame()->EnableWindow(TRUE);
}

//-----------------------------------------------------------------------------	
void BDDMSRepository::OnScanProcessEnded()
{
	if (m_pAcquisitionFromDevice)
		ArchiveFiles(m_pAcquisitionFromDevice->m_pAcquiredFiles);
}

//-----------------------------------------------------------------------------
void BDDMSRepository::ArchiveFiles(const CStringArray* pDroppedFiles)
{
	if (!pDroppedFiles || pDroppedFiles->GetSize() <= 0)
		return;

	for (int i = 0; i < pDroppedFiles->GetSize(); i++)
		dmsOrchestrator->ArchiveFile(gcnew String(pDroppedFiles->GetAt(i)), _T(""), false, false, _T(""));		
}

// intercetto il click sul pulsante di estrazione dati
//----------------------------------------------------------------------------
void BDDMSRepository::OnToolbarExtractDocuments()
{
	// avendo cambiato pulsante nella toolbar devo chiamare esplicitamente
	// i metodi GetNotValidView e UpdateDataView
	GetNotValidView(TRUE);
	ExtractArchivedDocuments();
	UpdateDataView();
}

//----------------------------------------------------------------------------
void BDDMSRepository::OnUpdateToolbarExtractDocuments(CCmdUI* pCmdUI)
{
	pCmdUI->Enable(!m_bEditMode);
}

// intercetto il click sul pulsante di annulla estrazione dati
//----------------------------------------------------------------------------
void BDDMSRepository::OnToolbarUndoExtractDocuments()
{
	GetNotValidView(TRUE);
	InitSelections(TRUE); // reinizializzo le selezioni
	UpdateDataView();
}

//----------------------------------------------------------------------------
void BDDMSRepository::OnUpdateToolbarUndoExtractDocuments(CCmdUI* pCmdUI)
{
	pCmdUI->Enable(!m_bEditMode);
}

//-----------------------------------------------------------------------------
void BDDMSRepository::InitSelections(BOOL bClearSelections /*= FALSE*/)
{
	if (!bClearSelections)
	{
		if (m_bOpenAsBrowser && m_pCachedSearchFilter)
		{
			// nr documenti
			if (!m_pCachedSearchFilter->m_TopDocsNumber.IsEmpty() && m_pCachedSearchFilter->m_TopDocsNumber > 0)
			{
				m_bAllExtractedDoc = FALSE;
				m_bFirstExtractedDoc = TRUE;
				m_nTopNrDocuments = m_pCachedSearchFilter->m_TopDocsNumber;
			}
			else
			{
				m_bAllExtractedDoc = TRUE;
				m_bFirstExtractedDoc = FALSE;
				m_nTopNrDocuments = 0;
			}

			// date
			m_FromDate = m_pCachedSearchFilter->m_StartDate;
			m_ToDate = m_pCachedSearchFilter->m_EndDate;
			// estensione file
			m_sFileExtension = m_pCachedSearchFilter->m_DocExtensionType;

			// free text
			m_FreeText = m_pCachedSearchFilter->m_FreeTag;
			if (m_pCachedSearchFilter->m_wSearchLocation == All)
				m_bFileNameAndDescription = m_bTags = m_bBarcode = m_bBookmarks = m_bDocumentContent = TRUE;
			else
				if (m_pCachedSearchFilter->m_wSearchLocation == None)
					m_bFileNameAndDescription = m_bTags = m_bBarcode = m_bBookmarks = m_bDocumentContent = FALSE;
				else
				{
					m_bFileNameAndDescription = (((SearchLocation)m_pCachedSearchFilter->m_wSearchLocation & SearchLocation::NameAndDescription) == SearchLocation::NameAndDescription);
					m_bTags = (((SearchLocation)m_pCachedSearchFilter->m_wSearchLocation & SearchLocation::Tags) == SearchLocation::Tags);
					m_bBarcode = (((SearchLocation)m_pCachedSearchFilter->m_wSearchLocation & SearchLocation::Barcode) == SearchLocation::Barcode);
					m_bBookmarks = (((SearchLocation)m_pCachedSearchFilter->m_wSearchLocation & SearchLocation::AllBookmarks) == SearchLocation::AllBookmarks);
					m_bDocumentContent = (((SearchLocation)m_pCachedSearchFilter->m_wSearchLocation & SearchLocation::Content) == SearchLocation::Content);
				}

			// worker
			if (AfxGetTbRepositoryManager()->GetDMSSettings()->GetSettings()->l_ShowOnlyMyArchivedDocs && !AfxGetLoginInfos()->m_bAdmin)
			{
				m_bSelectWorkers.SetReadOnly();
				m_bSelectWorkers = FALSE;
				m_bShowDisabledWorkers = FALSE;
				m_Workers = cwsprintf(_T("%d"), AfxGetWorkerId());
			}
			else
			{
				if (m_pCachedSearchFilter->m_arWorkers.IsEmpty())
				{
					m_bSelectWorkers = FALSE;
					m_bShowDisabledWorkers = FALSE;
					m_bShowDisabledWorkers.SetReadOnly();
					m_Workers.Clear();
					m_Workers.SetReadOnly();
				}
				else
				{
					m_bSelectWorkers = TRUE;
					for (int i = 0; i < m_pCachedSearchFilter->m_arWorkers.GetCount(); i++)
					{
						CString worker = m_pCachedSearchFilter->m_arWorkers.GetAt(i);
						m_Workers += (worker + ";"); // compongo la stringa dei workerid selezionati ed intervallati dal ;
					}
				}
			}

			// categorie e bookmarks
			m_CollectionID = m_pCachedSearchFilter->m_CollectionID;
			if (m_CollectionID > 0)
			{
				m_bAllRepository = FALSE;
				m_bOnlyCollection = TRUE;
				if (m_pCollectionList)
				{
					DMSCollectionInfo* pCollInfo = m_pCollectionList->GetCollectionInfoByCollectionID(m_CollectionID);
					m_DocNamespace = (pCollInfo) ? pCollInfo->m_DocNamespace : _T("");
				}
			}
			else
			{
				m_bAllRepository = TRUE;
				m_bOnlyCollection = FALSE;
				m_DocNamespace = _T("");
			}
			m_DocNamespace.SetReadOnly(m_bAllRepository);

			// se ho dei filtri per i bookmarks li carico nel DBT
			if (m_pCachedSearchFilter->m_pCachedSearchFieldsArray && m_pCachedSearchFilter->m_pCachedSearchFieldsArray->GetSize() > 0)
			{
				m_pCachedSearchFilter->RestoreFilters();
				GetHotLink<HKLDMSFields>(L"DMSFields")->FilterForCollectionID(&m_CollectionID);
			}

			return;
		}
	}

	// questo codice viene eseguito se:
	// 1. ho cliccato sul pulsante Undo della toolbar e voglio quindi impostare i filtri con i valori di default
	// 2. sto aprendo il RepoExplorer
	// 3. sto aprendo il RepoBrowser e il puntatore all'oggetto con la cache dei filtri non e' valido

	// inizializzo le date degli allegati dal primo giorno della settimana corrente alla data odierna
	m_FromDate = AfxGetApplicationDate();
	m_FromDate.SetWeekStartDate();
	m_ToDate = AfxGetApplicationDate();

	m_bSelectWorkers			= FALSE;
	m_bShowDisabledWorkers		= FALSE;
	m_bShowDisabledWorkers.		SetReadOnly();
	m_Workers.					Clear(); 
	m_Workers.					SetReadOnly();
	m_bFileNameAndDescription	= TRUE;
	m_bTags						= TRUE;
	m_bBookmarks				= TRUE;
	m_bAllExtractedDoc			= FALSE;
	m_bFirstExtractedDoc		= TRUE;
	m_nTopNrDocuments			= 50;
	
	//Collection
	m_bAllRepository			= TRUE;
	m_bOnlyCollection			= FALSE;
	m_CollectionID				= -1;
	m_DocNamespace				= _T("");
	m_DocNamespace.SetReadOnly(TRUE);

	m_sFileExtension				= szAllFiles;
	m_FreeText.Clear();

	if (!AfxGetTbRepositoryManager()->GetDMSSettings()->GetSettings()->l_EnableBarcode)
	{
		m_bBarcode = FALSE;
		m_bBarcode.SetReadOnly();
	}
	if (!AfxGetTbRepositoryManager()->GetDMSSettings()->GetSettings()->l_EnableFTS)
	{
		m_bDocumentContent = FALSE;
		m_bDocumentContent.SetReadOnly();
	}

	m_pDBTArchivedDocuments->RemoveAll();
	m_pDBTAttachmentLinks->RemoveAll();
	m_pCurrentAttInfo->Clear();

	if (AfxGetTbRepositoryManager()->GetDMSSettings()->GetSettings()->l_ShowOnlyMyArchivedDocs && !AfxGetLoginInfos()->m_bAdmin)
	{
		m_bSelectWorkers.SetReadOnly();
		m_bSelectWorkers = FALSE;
		m_bShowDisabledWorkers = FALSE;
		m_Workers = cwsprintf(_T("%d"), AfxGetWorkerId());
	}

	UpdateDataView();
}

// apertura file temporaneo con il programma predefinito
//-----------------------------------------------------------------------------
void BDDMSRepository::ShowDocuments(CUIntArray* pIndexes /*= NULL*/)
{
	if (!pIndexes)
	{
		// se non esiste il file temporaneo lo creo al volo	
		if (m_pCurrentAttInfo->m_TemporaryPathFile.IsEmpty())
			m_pCurrentAttInfo->m_TemporaryPathFile = AfxGetTbRepositoryManager()->GetArchivedDocTempFile(m_pCurrentAttInfo->m_ArchivedDocId);

		AfxGetTbRepositoryManager()->OpenDocument(m_pCurrentAttInfo->m_TemporaryPathFile.Str());
	}
	else
	{
		int idx = -1;
		VArchivedDocument* pRec;
		DMSAttachmentInfo* pAttInfo;
		for (int i = 0; i < pIndexes->GetCount(); i++)
		{
			idx = pIndexes->GetAt(i);
			pRec = (VArchivedDocument*)m_pDBTArchivedDocuments->GetRow(idx);
			pAttInfo = pRec->GetAttachmentInfo();
			if (pAttInfo)
			{
				if (pAttInfo->m_TemporaryPathFile.IsEmpty())
					pAttInfo->m_TemporaryPathFile = AfxGetTbRepositoryManager()->GetArchivedDocTempFile(pAttInfo->m_ArchivedDocId);
				AfxGetTbRepositoryManager()->OpenDocument(pAttInfo->m_TemporaryPathFile.Str());
			}
		}
	}
}

//-----------------------------------------------------------------------------
void BDDMSRepository::EditArchivedDoc()
{
	m_bEditMode = TRUE;
	EnableCurrentAttachControls();
	DoEditDocumentChanged();
}

//-----------------------------------------------------------------------------
void BDDMSRepository::SaveArchivedDoc()
{
	//per prima cosa devo portare le modifiche fatte sul DBTBookmarks
	m_pCurrentAttInfo->ModifyBookmarks(m_pDBTCategoriesBookmarks);

	if (AfxGetTbRepositoryManager()->UpdateArchivedDoc(m_pCurrentAttInfo, m_pCurrentAttInfo->m_Description, m_pCurrentAttInfo->m_FreeTag, m_pCurrentAttInfo->m_BarcodeValue))
		m_bEditMode = FALSE;
	
	EnableCurrentAttachControls();
	DoEditDocumentChanged();
}

//-----------------------------------------------------------------------------
void BDDMSRepository::UndoArchivedDocChanges()
{
	OnArchiveDocRowChanged();
	m_bEditMode = FALSE;

	EnableCurrentAttachControls();
	DoEditDocumentChanged();
}

// elimina un documento oppure una selezione multipla di documenti
//-----------------------------------------------------------------------------
void BDDMSRepository::DeleteDocuments(CUIntArray* pIndexes /*= NULL*/)
{
	if (!pIndexes)
	{
		VArchivedDocument* pRec = m_pDBTArchivedDocuments->GetCurrent();
		if (!pRec) return;

		if (pRec->l_CheckOutWorker != _T("-1") && !pRec->l_CheckOutWorker.IsEmpty())
		{
			AfxMessageBox(_TB("It is not possible to delete an archived document in check-out"));
			return;
		}
			
		if (
			pRec->l_IsAttachment &&
			AfxMessageBox(_TB("Selected document is used as attachment in ERP documents. Do you want continue to delete archived document and all related attachments?"), MB_OKCANCEL) == IDCANCEL
			)
			return;

		AfxGetTbRepositoryManager()->DeleteArchiveDocInCascade(m_pCurrentAttInfo);
	}
	else
	{
		if (AfxMessageBox(_TB("Do you want continue to delete all selected archived documents and the related attachments?\r\n(Remember: the files in check-out cannot be deleted)"), MB_OKCANCEL) == IDCANCEL)
			return;

		int idx = -1;
		VArchivedDocument* pRec;
		DMSAttachmentInfo* pAttInfo;
		for (int i = 0; i < pIndexes->GetCount(); i++)
		{
			idx = pIndexes->GetAt(i);
			pRec = (VArchivedDocument*)m_pDBTArchivedDocuments->GetRow(idx);
			
			if (pRec->l_CheckOutWorker != _T("-1") && !pRec->l_CheckOutWorker.IsEmpty())
				continue; // i file in check-out NON possono essere eliminati

			pAttInfo = pRec->GetAttachmentInfo();
			if (pAttInfo)
				AfxGetTbRepositoryManager()->DeleteArchiveDocInCascade(pAttInfo);
		}
	}

	// ricarico il DBT
	OnToolbarExtractDocuments();
}

// invio via email l'allegato
//-----------------------------------------------------------------------------
void BDDMSRepository::SendDocuments(CUIntArray* pIndexes)
{
	DMSAttachmentsList* pAttachmentsList = new DMSAttachmentsList();
	// per non eliminare il contenuto dell'array, che sono puntatori che verranno eliminati dopo nel distruttore del SqlVirtualRecord
	pAttachmentsList->SetOwns(FALSE); 

	if (!pIndexes)
		pAttachmentsList->Add(m_pCurrentAttInfo);
	else
	{
		int idx = -1;
		VArchivedDocument* pRec;
		DMSAttachmentInfo* pAttInfo;
		for (int i = 0; i < pIndexes->GetCount(); i++)
		{
			idx = pIndexes->GetAt(i);
			pRec = (VArchivedDocument*)m_pDBTArchivedDocuments->GetRow(idx);

			pAttInfo = pRec->GetAttachmentInfo();
			if (pAttInfo)
				pAttachmentsList->Add(pAttInfo);
		}
	}

	if (pAttachmentsList->GetCount() > 0)
		AfxGetTbRepositoryManager()->SendAsAttachments(pAttachmentsList);

	SAFE_DELETE(pAttachmentsList);
}

// sulla selezione della riga del DBT vado ad assegnare l'AttachmentInfo corrente
//-----------------------------------------------------------------------------
void BDDMSRepository::OnArchiveDocRowChanged()
{
	VArchivedDocument* pRec = m_pDBTArchivedDocuments->GetCurrent();
	if (!pRec) return;

	DMSAttachmentInfo* pRecAttInfo = pRec->GetAttachmentInfo();
	if (!pRecAttInfo)
		m_pCurrentAttInfo->Clear();
	else
		*m_pCurrentAttInfo = *pRecAttInfo;
	
	// carico le informazioni dei singoli allegati correlati al documento archiviato corrente
	m_pDBTAttachmentLinks->LocalFindData(FALSE);
	// carico i bookmarks delle categorie
	m_pDBTCategoriesBookmarks->LoadFromBookmarkDT(m_pCurrentAttInfo);

	DoAttachmentInfoChanged();

	UpdateDataView();
}

// esegue il check-in/checkout di uno o piu' documenti archiviati
//-----------------------------------------------------------------------------
void BDDMSRepository::CheckOutFile(CUIntArray* pIndexes /*= NULL*/)
{
	if (!pIndexes)
	{
		VArchivedDocument* pRec = m_pDBTArchivedDocuments->GetCurrent();
		if (!pRec) return;
		BOOL m_bCheckOut = (pRec->l_CheckOutWorker == _T("-1") || pRec->l_CheckOutWorker.IsEmpty());

		// eseguo il checkin o il checkout e se e' andato a buon fine devo aggiornare il localfield del pRec
		// su cui mi baso per eseguire l'update dell'interfaccia grafica
		if ((m_bCheckOut) ? AfxGetTbRepositoryManager()->CheckOut(m_pCurrentAttInfo) : AfxGetTbRepositoryManager()->CheckIn(m_pCurrentAttInfo))
		{
			pRec->l_CheckOutWorker = m_pCurrentAttInfo->m_CurrentCheckOutWorker;
			pRec->l_CheckOutWorkerBmp = (pRec->l_CheckOutWorker == _T("-1") || pRec->l_CheckOutWorker.IsEmpty()) ? _T("") : ExtensionsGlyph(szGlyphCheckOut);
			m_bCheckOut = !m_bCheckOut;
		}
	}
	else
	{
		int idx = -1;
		VArchivedDocument* pRec;
		DMSAttachmentInfo* pAttInfo;
		for (int i = 0; i < pIndexes->GetCount(); i++)
		{
			idx = pIndexes->GetAt(i);
			pRec = (VArchivedDocument*)m_pDBTArchivedDocuments->GetRow(idx);

			pAttInfo = pRec->GetAttachmentInfo();
			if (!pAttInfo) continue;

			BOOL m_bCheckOut = (pRec->l_CheckOutWorker == _T("-1") || pRec->l_CheckOutWorker.IsEmpty());

			// eseguo il checkin o il checkout e se e' andato a buon fine devo aggiornare il localfield del pRec
			// su cui mi baso per eseguire l'update dell'interfaccia grafica
			if ((m_bCheckOut) ? AfxGetTbRepositoryManager()->CheckOut(pAttInfo) : AfxGetTbRepositoryManager()->CheckIn(pAttInfo))
			{
				pRec->l_CheckOutWorker = pAttInfo->m_CurrentCheckOutWorker;
				pRec->l_CheckOutWorkerBmp = (pRec->l_CheckOutWorker == _T("-1") || pRec->l_CheckOutWorker.IsEmpty()) ? _T("") : ExtensionsGlyph(szGlyphCheckOut);
				m_bCheckOut = !m_bCheckOut;
			}
		}
	}

	UpdateDataView();
}

// esegue l'undo checkout di uno o piu' documenti archiviati
//-----------------------------------------------------------------------------
void BDDMSRepository::UndoCheckOutFile(CUIntArray* pIndexes /*= NULL*/)
{
	if (!pIndexes)
	{
		VArchivedDocument* pRec = m_pDBTArchivedDocuments->GetCurrent();
		if (!pRec) return;

		if (AfxGetTbRepositoryManager()->Undo(m_pCurrentAttInfo))
		{
			pRec->l_CheckOutWorker = m_pCurrentAttInfo->m_CurrentCheckOutWorker;
			pRec->l_CheckOutWorkerBmp = (pRec->l_CheckOutWorker == _T("-1") || pRec->l_CheckOutWorker.IsEmpty()) ? _T("") : ExtensionsGlyph(szGlyphCheckOut);
		}
	}
	else
	{
		int idx = -1;
		VArchivedDocument* pRec;
		DMSAttachmentInfo* pAttInfo;
		for (int i = 0; i < pIndexes->GetCount(); i++)
		{
			idx = pIndexes->GetAt(i);
			pRec = (VArchivedDocument*)m_pDBTArchivedDocuments->GetRow(idx);
			// non posso fare l'undo di file che non sono in out oppure sono stati presi in out da un worker diverso dal correntemente loginato
			// (questo controllo viene fatto a monte in caso di selezione singola, ma con la multipla devo gestirlo qui nel ciclo)
			if (pRec->l_CheckOutWorker == _T("-1") || pRec->l_CheckOutWorker.IsEmpty() || (pRec->l_CheckOutWorker.CompareNoCase(DataStr(((CString)AfxGetWorkerId().ToString())))))
				continue;

			pAttInfo = pRec->GetAttachmentInfo();
			if (!pAttInfo) continue;
		
			if (AfxGetTbRepositoryManager()->Undo(pAttInfo))
			{
				pRec->l_CheckOutWorker = pAttInfo->m_CurrentCheckOutWorker;
				pRec->l_CheckOutWorkerBmp = (pRec->l_CheckOutWorker == _T("-1") || pRec->l_CheckOutWorker.IsEmpty()) ? _T("") : ExtensionsGlyph(szGlyphCheckOut);
			}
		}
	}

	UpdateDataView();
}

// salva il documento archiviato sul file system (oppure un gruppo di documenti in un folder)
//-----------------------------------------------------------------------------
void BDDMSRepository::SaveArchiveDocFileInFolder(CUIntArray* pIndexes /*= NULL*/)
{
	if (!pIndexes)
	{
		VArchivedDocument* pRec = m_pDBTArchivedDocuments->GetCurrent();
		if (!pRec || pRec->l_ArchivedDocId <= 0)
			return;

		AfxGetTbRepositoryManager()->SaveArchiveDocFileInFolder(pRec->l_ArchivedDocId);
	}
	else
	{
		int idx = -1;
		VArchivedDocument* pRec;
		CUIntArray* pArchiveDocIdsArray = new CUIntArray();

		for (int i = 0; i < pIndexes->GetCount(); i++)
		{
			idx = pIndexes->GetAt(i);
			pRec = (VArchivedDocument*)m_pDBTArchivedDocuments->GetRow(idx);
			pArchiveDocIdsArray->Add((int)pRec->l_ArchivedDocId);
		}

		AfxGetTbRepositoryManager()->SaveMultipleArchiveDocFileInFolder(pArchiveDocIdsArray);
		SAFE_DELETE(pArchiveDocIdsArray);
	}
}

//-----------------------------------------------------------------------------
DMSCollectionInfo* BDDMSRepository::GetFistAvailableCollection()
{
	if (!m_pCollectionList || m_pCollectionList->GetSize() == 0)
		return NULL;

	CTBNamespace nsDoc;
	for (int i = 0; i < m_pCollectionList->GetSize(); i++)
	{
		DMSCollectionInfo* pCollectionInfo = m_pCollectionList->GetAt(i);
		if (pCollectionInfo)
		{
			nsDoc.SetNamespace(pCollectionInfo->m_DocNamespace);
			//Nel caso della security visualizzo la collection solo se l'utente ha i privilegi
			// se la security e la security light non sono attivate ed usate allora il metodo restituisce sempre TRUE
			if (AfxGetTbCmdManager()->CanUseNamespace(nsDoc, OSLType_Template, OSL_GRANT_EXECUTE, nullptr) == TRUE)
				return pCollectionInfo;
		}
	}

	return NULL;
}

//gestione dei filtri advanced
//-----------------------------------------------------------------------------
void BDDMSRepository::DoSelCollectionChanged()
{
	m_DocNamespace.SetReadOnly(m_bAllRepository);
	if (m_bAllRepository)
	{
		m_pCachedSearchFilter->m_CollectionID = m_CollectionID = -1;
		m_DocNamespace = _T("");
	}
	else
	{
		DMSCollectionInfo* pCollInfo = GetFistAvailableCollection();
		m_DocNamespace = (pCollInfo) ? pCollInfo->m_DocNamespace : _T("");
		m_pCachedSearchFilter->m_CollectionID = m_CollectionID = (pCollInfo) ? pCollInfo->m_CollectionID : -1;
	}

	if (m_pCachedSearchFilter->m_pCachedSearchFieldsArray)
		m_pCachedSearchFilter->m_pCachedSearchFieldsArray->RemoveAll();

	m_pDBTSearchFieldsConditions->RemoveAll();
	UpdateDataView();
}

//-----------------------------------------------------------------------------
void BDDMSRepository::DoCollectionChanged()
{
	DMSCollectionInfo* pCollInfo = (!m_DocNamespace.IsEmpty()) ? m_pCollectionList->GetCollectionInfoByDocNamespace(m_DocNamespace.Str()) : NULL;
	m_pCachedSearchFilter->m_CollectionID = m_CollectionID = (pCollInfo) ? pCollInfo->m_CollectionID : -1;

	if (m_pCachedSearchFilter->m_pCachedSearchFieldsArray)
		m_pCachedSearchFilter->m_pCachedSearchFieldsArray->RemoveAll();
		
	m_pDBTSearchFieldsConditions->RemoveAll();
	UpdateDataView();
}

//-----------------------------------------------------------------------------
void BDDMSRepository::DoSearchFieldChanged()
{
	VSearchFieldCondition* searchFieldCondition = m_pDBTSearchFieldsConditions->GetCurrent();
	searchFieldCondition->l_FieldName = GetHotLink<HKLDMSFields>(L"DMSFields")->GetRecord()->f_FieldName;//m_pHKLDMSFields->GetRecord()->f_FieldName;
	searchFieldCondition->l_ValueType = GetHotLink<HKLDMSFields>(L"DMSFields")->GetRecord()->f_ValueType;// m_pHKLDMSFields->GetRecord()->f_ValueType;
	GetHotLink<HKLSearchFieldIndexes>(L"SearchFieldIndexes")->SetFieldName(searchFieldCondition->l_FieldName);
	//m_pHKLSearchFieldIndexes->SetFieldName(searchFieldCondition->l_FieldName);
	if (searchFieldCondition->l_FieldDescription.IsEmpty())
		searchFieldCondition->l_FormattedValue.Clear();
}

//-----------------------------------------------------------------------------
void BDDMSRepository::DoSearchValueChanged()
{
	VSearchFieldCondition* searchFieldCondition = m_pDBTSearchFieldsConditions->GetCurrent();
	searchFieldCondition->l_SearchFieldID = GetHotLink<HKLSearchFieldIndexes>(L"SearchFieldIndexes")->GetRecord()->f_SearchIndexID;//m_pHKLSearchFieldIndexes->GetRecord()->f_SearchIndexID;
	searchFieldCondition->l_FieldValue = GetHotLink<HKLSearchFieldIndexes>(L"SearchFieldIndexes")->GetRecord()->f_FieldValue; //m_pHKLSearchFieldIndexes->GetRecord()->f_FieldValue;
}

//-----------------------------------------------------------------------------
void BDDMSRepository::OnSearchFieldRowChanged()
{
	VSearchFieldCondition* searchFieldCondition = m_pDBTSearchFieldsConditions->GetCurrent();
	GetHotLink<HKLSearchFieldIndexes>(L"SearchFieldIndexes")->SetFieldName(searchFieldCondition->l_FieldName);
	//m_pHKLSearchFieldIndexes->SetFieldName(searchFieldCondition->l_FieldName);
}

/////////////////////////////////////////////////////////////////////////////
//				class BDDMSRepositoryBrowser Implementation
/////////////////////////////////////////////////////////////////////////////
//----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(BDDMSRepositoryBrowser, BDDMSRepository)

//----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(BDDMSRepositoryBrowser, BDDMSRepository)
	////{{AFX_MSG_MAP(BDDMSRepositoryBrowser)
	ON_COMMAND			(ID_DMSREPOSITORY_TOOLBTN_ATTACH, OnToolbarAttach)
	ON_UPDATE_COMMAND_UI(ID_DMSREPOSITORY_TOOLBTN_ATTACH, OnUpdateToolbarAttach)
	////}}AFX_MSG_MAP
END_MESSAGE_MAP()

//----------------------------------------------------------------------------
BDDMSRepositoryBrowser::BDDMSRepositoryBrowser()
{
	// imposto il flag per pilotare l'apertura del documento come browser
	m_bOpenAsBrowser = TRUE;
}

//----------------------------------------------------------------------------
BOOL BDDMSRepositoryBrowser::OnOpenDocument(LPCTSTR pParam)
{
	// puntatore al documento chiamante (sarebbe il serverdocument del client doc)
	if (pParam)
		m_pCallingDoc = (CAbstractFormDoc*)GET_AUXINFO(pParam);

	// assegno al puntatore locale quello istanziato dal clientdoc (per la cache dei filtri)
	CDDMS* pCDDMS = (CDDMS*)m_pCallingDoc->GetClientDoc(RUNTIME_CLASS(CDDMS));
	if (pCDDMS && pCDDMS->m_pCachedSearchFilter)
		m_pCachedSearchFilter = pCDDMS->m_pCachedSearchFilter;

	return CAbstractFormDoc::OnOpenDocument(pParam);
}

// intercetto il click sul button della toolbar per l'Attach
// riempio un array con l'elenco degli id dei documenti archiviati selezionati e
// invio una sendmessage al client doc che si occupera' di creare gli allegati
//----------------------------------------------------------------------------
void BDDMSRepositoryBrowser::OnToolbarAttach()
{
	if (m_pDBTArchivedDocuments->IsEmpty() || !m_pCallingDoc) 
		return;

	if (m_pArchivedDocIdsArray && m_pArchivedDocIdsArray->GetCount() > 0)
		m_pArchivedDocIdsArray->RemoveAll();
	else
		m_pArchivedDocIdsArray = new CUIntArray();

	VArchivedDocument* pRec;
	for (int i = 0; i <= m_pDBTArchivedDocuments->GetUpperBound(); i++)
	{
		pRec = (VArchivedDocument*)m_pDBTArchivedDocuments->GetRow(i);
		if (!pRec->l_IsSelected) continue;

		m_pArchivedDocIdsArray->Add((int)pRec->l_ArchivedDocId);
	}
	
	// intercettato dal CDDMS
	m_pCallingDoc->GetMasterFrame()->SendMessage(WM_COMMAND, ID_DMSREPOSITORYBROWSER_ATTACH_COMPLETED);

	GetMasterFrame()->PostMessage(WM_CLOSE);
}

//----------------------------------------------------------------------------
void BDDMSRepositoryBrowser::OnUpdateToolbarAttach(CCmdUI* pCmdUI)
{
	BOOL bRecordIsSelected = FALSE;

	VArchivedDocument* pRec;
	for (int i = 0; i <= m_pDBTArchivedDocuments->GetUpperBound(); i++)
	{
		pRec = (VArchivedDocument*)m_pDBTArchivedDocuments->GetRow(i);
		if (pRec->l_IsSelected)
		{
			bRecordIsSelected = TRUE;
			break;
		}
	}

	pCmdUI->Enable(bRecordIsSelected);
}
