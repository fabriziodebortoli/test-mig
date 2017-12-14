#include "stdafx.h"

#include <TbFrameworkImages\GeneralFunctions.h>
#include <TbFrameworkImages\CommonImages.h>
#include <ExtensionsImages\CommonImages.h>

#include "TBRepositoryManager.h"
#include "CommonObjects.h"
#include "BDDMSRepository.h"
#include "UIDMSRepository.h"

#include "EasyAttachment\JsonForms\UIDMSRepository\IDD_DMSREPOSITORY_EXPLORER_TOOLBAR.hjson"
#include "EasyAttachment\JsonForms\UIDMSRepository\IDD_DMSREPOSITORY_BROWSER_TOOLBAR.hjson"
#include "EasyAttachment\JsonForms\UIDMSRepository\IDD_DMSREPOSITORY_EXPLORER.hjson"

//////////////////////////////////////////////////////////////////////////////
//			     class CFilesExtensionsItemSource implementation
//////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CFilesExtensionsItemSource, CItemSource)

//-----------------------------------------------------------------------------
CFilesExtensionsItemSource::CFilesExtensionsItemSource()
	:
	CItemSource			(),
	m_pExtensionsList	(NULL)
{
	// chiedo l'elenco estensioni utilizzate in EA
	m_pExtensionsList = AfxGetTbRepositoryManager()->GetAllExtensions();
}

//-----------------------------------------------------------------------------
CFilesExtensionsItemSource::~CFilesExtensionsItemSource()
{
	SAFE_DELETE(m_pExtensionsList);
}

//-----------------------------------------------------------------------------
void CFilesExtensionsItemSource::GetData(DataArray& values, CStringArray& descriptions, CString strCurrentValue)
{
	values.Add(new DataStr(szAllFiles));
	descriptions.Add(szAllFiles);

	if (!m_pExtensionsList)
		return;

	for (int i = 0; i <= m_pExtensionsList->GetUpperBound(); i++)
	{
		CString extension = m_pExtensionsList->GetAt(i);
		values.Add(new DataStr(extension));
		descriptions.Add(extension);
	}
}

//////////////////////////////////////////////////////////////////////////////
//			     class CCollectionsItemSource implementation
//////////////////////////////////////////////////////////////////////////////
//----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CCollectionsItemSource, CItemSource)

//-----------------------------------------------------------------------------
CCollectionsItemSource::CCollectionsItemSource()
	:
	CItemSource		(),
	m_pCollections	(NULL)
{
	//elenco dei collector da cui vengono poi estrapolate le collection
	m_pCollections = AfxGetTbRepositoryManager()->GetUsedCollections();
}

//-----------------------------------------------------------------------------
CCollectionsItemSource::~CCollectionsItemSource()
{
	SAFE_DELETE(m_pCollections);
}

//-----------------------------------------------------------------------------
void CCollectionsItemSource::GetData(DataArray& values, CStringArray& descriptions, CString strCurrentValue)
{
	if (!m_pCollections)
		return;

	CTBNamespace nsDoc;
	for (int i = 0; i < m_pCollections->GetSize(); i++)
	{
		DMSCollectionInfo* pCollectionInfo = m_pCollections->GetAt(i);
		if (pCollectionInfo)
		{
			nsDoc.SetNamespace(pCollectionInfo->m_DocNamespace);
			//Nel caso della security visualizzo la collection solo se l'utente ha i privilegi
			// se la security e la security light non sono attivate ed usate allora il metodo restituisce sempre TRUE
			if (AfxGetTbCmdManager()->CanUseNamespace(nsDoc, OSLType_Template, OSL_GRANT_EXECUTE, nullptr) == TRUE)
			{
				values.Add(new DataStr(pCollectionInfo->m_DocNamespace));
				descriptions.Add(pCollectionInfo->m_DocTitle.Str());
			}
		}
	}
		
}

//////////////////////////////////////////////////////////////////////////////
//			     class CWorkersListBox implementation
//////////////////////////////////////////////////////////////////////////////
//----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CWorkersListBox, CParsedCheckListBox /*CItemSource*/)

//-----------------------------------------------------------------------------
CWorkersListBox::CWorkersListBox()
	:
	CParsedCheckListBox	(),
	//CItemSource		(),
	m_pWorkersTblObj	(NULL),
	m_bAlsoDisabled		(FALSE)
{}

//-----------------------------------------------------------------------------
void CWorkersListBox::LoadWorkers(BOOL bAlsoDisabled /*= FALSE*/)
{
	m_pWorkersTblObj = AfxGetWorkersTable();
	m_bAlsoDisabled = bAlsoDisabled;
	OnFillListBox();
}

//-----------------------------------------------------------------------------
void CWorkersListBox::OnFillListBox()
{
	__super::OnFillListBox();

	if (!m_pWorkersTblObj)
		return;
	CWorker* worker = NULL;
	BOOL bDisabled = FALSE;

	for (int i = 0; i <= m_pWorkersTblObj->GetWorkersCount(); i++)
	{
		worker = m_pWorkersTblObj->GetWorkerAt(i);
		if (worker == NULL) continue;

		bDisabled = worker->GetDisabled();

		// se non voglio visualizzare le matricole disabilitate le skippo
		if (!m_bAlsoDisabled && bDisabled)
			continue;

		CString aFullName(worker->GetName() + " " + worker->GetLastName());
		DataStr aID = cwsprintf(_T("%d"), worker->GetWorkerID());
		AddAssociation(aFullName, aID);
	}
}

//==============================================================================
//						CDMSRepositoryView
//==============================================================================
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CDMSRepositoryView, CJsonFormView)

//-----------------------------------------------------------------------------
CDMSRepositoryView::CDMSRepositoryView()
{
}

/////////////////////////////////////////////////////////////////////////////
//			class CSearchFieldsConditionsBodyEdit implementation
/////////////////////////////////////////////////////////////////////////////
//=============================================================================
IMPLEMENT_DYNCREATE(CSearchFieldsConditionsBodyEdit, CJsonBodyEdit)

//-----------------------------------------------------------------------------	
void CSearchFieldsConditionsBodyEdit::Customize()
{
	__super::Customize();
}

//-----------------------------------------------------------------------------
BOOL CSearchFieldsConditionsBodyEdit::OnCommand(WPARAM wParam, LPARAM lParam)
{
	DECLARE_WM_COMMAND_PARAMS(wParam, lParam, nID, nCode, hWndCtrl);

	if (nID == 0 || hWndCtrl == NULL)
		return CBodyEdit::OnCommand(wParam, lParam);

	if (nCode == EN_VALUE_CHANGED)
	{
		if (nID == IDC_DMSREPOSITORY_SEARCH_CONDITION_DESCRI)
			GetDocument()->DoSearchFieldChanged(); 

		if (nID == IDC_DMSREPOSITORY_SEARCH_CONDITION_VALUE)
			GetDocument()->DoSearchValueChanged();
	}

	return CBodyEdit::OnCommand(wParam, lParam);
}

/////////////////////////////////////////////////////////////////////////////
//			class CArchivedDocumentsBodyEdit Implementation
/////////////////////////////////////////////////////////////////////////////
//=============================================================================
IMPLEMENT_DYNCREATE(CArchivedDocumentsBodyEdit, CJsonBodyEdit)

//-----------------------------------------------------------------------------	
BEGIN_MESSAGE_MAP(CArchivedDocumentsBodyEdit, CJsonBodyEdit)
	ON_COMMAND(ID_DOCUMENT_EXTRACTION_ENDED,			OnExtractDocumentsEnded)

	ON_COMMAND(ID_ARCHDOC_BTN_VIEW,						OnShowDocument)
	ON_COMMAND(ID_ARCHDOC_BTN_DELETE,					OnDeleteDocument)
	ON_COMMAND(ID_ARCHDOC_BTN_SEND,						OnSendDocument)

	ON_COMMAND(ID_ARCHDOC_BTN_CHECKOUT,					OnCheckInOutClicked)
	ON_COMMAND(ID_ARCHDOC_BTN_UNDO_CHECKOUT,			OnUndoCheckOutClicked)
	ON_COMMAND(ID_ARCHDOC_BTN_EDIT,						OnEditSaveClicked)
	ON_COMMAND(ID_ARCHDOC_BTN_UNDO_CHANGES,				OnUndoArchivedDocChanges)

	ON_COMMAND(ID_ARCHDOC_BTN_SELECT_DESELECT,			OnSelectDeselectClicked)
	ON_COMMAND(ID_ARCHDOC_BTN_COPY_IN,					OnCopyInClicked)

	ON_COMMAND(ID_ARCHDOC_BTN_SHOW_ONLY_WOORM_REPORT,	OnShowReportClicked)
	ON_COMMAND(ID_ARCHDOC_BTN_SHOW_ONLY_ATTACHMENTS,	OnShowAttachmentsClicked)
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------	
CArchivedDocumentsBodyEdit::CArchivedDocumentsBodyEdit()
	: 
	m_pBEBtnEditSave			(NULL),
	m_pBEBtnCheckInOut			(NULL),
	m_pBEBtnUndoCheckInOut		(NULL),
	m_pBEBtnSelectDeselect		(NULL),
	m_pBEBtnMemoryFilters		(NULL),
	m_bSelectDeselect			(TRUE),
	m_bFilterWoormReport		(FALSE),
	m_bFilterAttachments		(FALSE),
	m_bIsActiveMultipleSelection(FALSE)
{
}

// richiamata sul changed di riga
//-----------------------------------------------------------------------------	
void CArchivedDocumentsBodyEdit::EnableButtons()
{
	BOOL bEnable = GetDocument()->m_pDBTArchivedDocuments->GetUpperBound() >= 0 && GetDocument()->m_pCurrentAttInfo->m_ArchivedDocId > -1;

	EnableButton(ID_ARCHDOC_BTN_NEW, !GetDocument()->m_bEditMode); // il pulsante di New e' disabilitato solo in Edit mode
	EnableButton(ID_ARCHDOC_BTN_EDIT, bEnable && !m_bIsActiveMultipleSelection);  // se sono in multiselection NON posso andare in Edit!
	EnableButton(ID_ARCHDOC_BTN_VIEW, bEnable);
	EnableButton(ID_ARCHDOC_BTN_COPY_IN, bEnable);
	EnableButton(ID_ARCHDOC_BTN_CHECKOUT, bEnable && !GetDocument()->m_bEditMode);
	EnableButton(ID_ARCHDOC_BTN_DELETE, bEnable && !GetDocument()->m_bEditMode);
	EnableButton(ID_ARCHDOC_BTN_SEND, bEnable);
	// i pulsanti con i filtri sono disabilitati solo in Edit Mode
	EnableButton(ID_ARCHDOC_BTN_FILTERS, !GetDocument()->m_bEditMode);
	EnableButton(ID_ARCHDOC_BTN_SHOW_ONLY_WOORM_REPORT, !GetDocument()->m_bEditMode);
	EnableButton(ID_ARCHDOC_BTN_SHOW_ONLY_ATTACHMENTS, !GetDocument()->m_bEditMode);

	UpdateCheckInCheckOutToolbarBtn();

	if (GetDocument()->m_bOpenAsBrowser)
	{
		EnableButton(ID_ARCHDOC_BTN_SELECT_DESELECT, bEnable);
		UpdateSelectDeselectToolbarBtn();
	}
}

// ritorna un array con gli indici delle righe selezionate con la multiselezione
//-----------------------------------------------------------------------------	
CUIntArray* CArchivedDocumentsBodyEdit::GetSelectedRowsIndexes()
{
	CUIntArray* pIndexes = new CUIntArray();

	for (int i = 0; i <= GetSelRowsUpperBound(); i++)
	{
		SelStatus status = GetSelRowsStatus(i);
		if (status == SelStatus::SELECTED)
			pIndexes->Add(i);
	}

	return pIndexes;
}

//-----------------------------------------------------------------------------	
void CArchivedDocumentsBodyEdit::OnShowDocument()
{
	if (m_bIsActiveMultipleSelection && CountSelectedRows() > 0)
	{
		CUIntArray* pIndexes = GetSelectedRowsIndexes();
		GetDocument()->ShowDocuments(pIndexes);
		SAFE_DELETE(pIndexes);
	}
	else
		GetDocument()->ShowDocuments();
}

// cancellazione singola o multipla
//-----------------------------------------------------------------------------	
void CArchivedDocumentsBodyEdit::OnDeleteDocument()
{
	if (m_bIsActiveMultipleSelection && CountSelectedRows() > 0)
	{
		CUIntArray* pIndexes = GetSelectedRowsIndexes();
		GetDocument()->DeleteDocuments(pIndexes);
		SAFE_DELETE(pIndexes);
	}
	else
		GetDocument()->DeleteDocuments();
}

// invio di allegati via email
//-----------------------------------------------------------------------------	
void CArchivedDocumentsBodyEdit::OnSendDocument()
{
	if (m_bIsActiveMultipleSelection && CountSelectedRows() > 0)
	{
		CUIntArray* pIndexes = GetSelectedRowsIndexes();
		GetDocument()->SendDocuments(pIndexes);
		SAFE_DELETE(pIndexes);
	}
	else
		GetDocument()->SendDocuments();
}

//-----------------------------------------------------------------------------	
void CArchivedDocumentsBodyEdit::UpdateCheckInCheckOutToolbarBtn()
{
	// se sono nel RepoBrowser non procedo
	if (GetDocument()->m_bOpenAsBrowser)
		return;

	VArchivedDocument* pRec = GetDocument()->m_pDBTArchivedDocuments->GetCurrent();
	if (!pRec) return;

	//Gestione checkIn-CheckOut
	// un file puo' essere preso in out se e' nessuno lo ha in out 
	BOOL bDocCanCheckOut = (pRec->l_CheckOutWorker == _T("-1") || pRec->l_CheckOutWorker.IsEmpty());
	if (!bDocCanCheckOut)
	{
		// NON posso eliminare un file che e' in checkout ma solo se non e' attiva la multiselezione
		if (!m_bIsActiveMultipleSelection)
			EnableButton(ID_ARCHDOC_BTN_DELETE, FALSE);

		// se non posso eseguire il checkout devo cmq controllare che il file 
		// non sia in out da un workerid diverso dal corrente,
		// se cosi fosse faccio sparire tutti i bottoni e torno subito
		if (pRec->l_CheckOutWorker != DataStr(((CString)AfxGetWorkerId().ToString())))
		{
			EnableButton(ID_ARCHDOC_BTN_CHECKOUT, FALSE);
			EnableButton(ID_ARCHDOC_BTN_UNDO_CHECKOUT, FALSE);
			return;
		}
		else
			EnableButton(ID_ARCHDOC_BTN_CHECKOUT, TRUE);
	}

	m_pBEBtnCheckInOut->SetText(bDocCanCheckOut ? _TB("Check Out") : _TB("Check In"));
	m_pBEBtnCheckInOut->SetTooltip(bDocCanCheckOut ? _TB("Check Out") : _TB("Check In"));
	m_pBEBtnCheckInOut->SetImage(bDocCanCheckOut ? TBLoadImage(ExtensionsIcon(szIconCheckOut, TOOLBAR)) : TBLoadImage(ExtensionsIcon(szIconCheckIn, TOOLBAR)));

	EnableButton(ID_ARCHDOC_BTN_UNDO_CHECKOUT, !bDocCanCheckOut);
}

//-----------------------------------------------------------------------------	
void CArchivedDocumentsBodyEdit::UpdateEditSaveToolbarBtn()
{
	// disabilito la selezione multipla
	EnableMultipleSel(!GetDocument()->m_bEditMode);

	m_pBEBtnEditSave->SetText(GetDocument()->m_bEditMode ? _TB("Save") : _TB("Edit"));
	m_pBEBtnEditSave->SetImage(GetDocument()->m_bEditMode ? TBLoadImage(TBIcon(szIconSave, TOOLBAR)) : TBLoadImage(TBIcon(szIconEdit, TOOLBAR)));
	m_pBEBtnEditSave->SetTooltip(GetDocument()->m_bEditMode ? _TB("Save") : _TB("Edit"));
	m_pBEBtnEditSave->EnableMenuItem(ID_ARCHDOC_BTN_UNDO_CHANGES, GetDocument()->m_bEditMode);	
}

//-----------------------------------------------------------------------------	
BOOL CArchivedDocumentsBodyEdit::OnCanLeaveCurrPos(int nNewCurrRec)
{
	return !GetDocument()->m_bEditMode;
}

//-----------------------------------------------------------------------------	
void CArchivedDocumentsBodyEdit::UpdateSelectDeselectToolbarBtn()
{
	m_pBEBtnSelectDeselect->SetText(m_bSelectDeselect ? _TB("Select") : _TB("Deselect"));
	m_pBEBtnSelectDeselect->SetTooltip(m_bSelectDeselect ? _TB("Select") : _TB("Deselect"));
}

//-----------------------------------------------------------------------------	
void CArchivedDocumentsBodyEdit::OnExtractDocumentsEnded()
{
	// alla fine dell'estrazione dati rimuovo d'ufficio tutti i filtri applicati al bodyedit
	m_bFilterWoormReport = m_bFilterAttachments = FALSE;

	m_pBEBtnMemoryFilters->CheckMenuItem(ID_ARCHDOC_BTN_SHOW_ONLY_WOORM_REPORT, m_bFilterWoormReport);
	m_pBEBtnMemoryFilters->CheckMenuItem(ID_ARCHDOC_BTN_SHOW_ONLY_ATTACHMENTS, m_bFilterAttachments);

	GetDocument()->m_pDBTArchivedDocuments->RemoveMemoryFilter();
}

//-----------------------------------------------------------------------------	
void CArchivedDocumentsBodyEdit::OnSelectDeselectClicked()
{
	m_bSelectDeselect = !m_bSelectDeselect;

	VArchivedDocument* pRec;
	for (int i = 0; i <= GetDocument()->m_pDBTArchivedDocuments->GetUpperBound(); i++)
	{
		pRec = (VArchivedDocument*)GetDocument()->m_pDBTArchivedDocuments->GetRow(i);
		pRec->l_IsSelected = !m_bSelectDeselect;
	}
	
	GetDocument()->UpdateDataView();

	UpdateSelectDeselectToolbarBtn();
}

//-----------------------------------------------------------------------------	
void CArchivedDocumentsBodyEdit::OnCopyInClicked()
{
	if (m_bIsActiveMultipleSelection && CountSelectedRows() > 0)
	{
		CUIntArray* pIndexes = GetSelectedRowsIndexes();
		GetDocument()->SaveArchiveDocFileInFolder(pIndexes);
		SAFE_DELETE(pIndexes);
	}
	else
		GetDocument()->SaveArchiveDocFileInFolder();
}

//-----------------------------------------------------------------------------	
void CArchivedDocumentsBodyEdit::OnShowReportClicked()
{
	m_bFilterWoormReport = !m_bFilterWoormReport;

	m_pBEBtnMemoryFilters->CheckMenuItem(ID_ARCHDOC_BTN_SHOW_ONLY_WOORM_REPORT, m_bFilterWoormReport);

	DataStr sReportImage = m_bFilterWoormReport ? _T("") : ExtensionsGlyph(szGlyphWoormReport);

	if (m_bFilterWoormReport)
		GetDocument()->m_pDBTArchivedDocuments->MemoryFilter(_T("VIsWoormReportBmp"), &DataStr(sReportImage), TRUE, DBTSlaveBuffered::ERemoveMemFilter::REMOVE_FILTER_NONE);
	else
		GetDocument()->m_pDBTArchivedDocuments->RemoveMemoryFilter(TRUE, DBTSlaveBuffered::ERemoveMemFilter::REMOVE_FILTER_NOT_UI, GetDocument()->m_nIsWoormReportColumnIdx);
}

//-----------------------------------------------------------------------------	
void CArchivedDocumentsBodyEdit::OnShowAttachmentsClicked()
{
	m_bFilterAttachments = !m_bFilterAttachments;

	m_pBEBtnMemoryFilters->CheckMenuItem(ID_ARCHDOC_BTN_SHOW_ONLY_ATTACHMENTS, m_bFilterAttachments);

	DataStr sAttachmentImage = m_bFilterAttachments ? _T("") : ExtensionsGlyph(szGlyphAttachment);

	if (m_bFilterAttachments)
		GetDocument()->m_pDBTArchivedDocuments->MemoryFilter(_T("VIsAttachmentBmp"), &DataStr(sAttachmentImage), TRUE, DBTSlaveBuffered::ERemoveMemFilter::REMOVE_FILTER_NONE);
	else
		GetDocument()->m_pDBTArchivedDocuments->RemoveMemoryFilter(TRUE, DBTSlaveBuffered::ERemoveMemFilter::REMOVE_FILTER_NOT_UI, GetDocument()->m_nIsAttachmentColumnIdx);
}

//-----------------------------------------------------------------------------	
void CArchivedDocumentsBodyEdit::OnUndoArchivedDocChanges()
{
	GetDocument()->UndoArchivedDocChanges();
	UpdateEditSaveToolbarBtn();
}

//-----------------------------------------------------------------------------	
void CArchivedDocumentsBodyEdit::OnEditSaveClicked()
{
	if (!GetDocument()->m_bEditMode)
	{
		// vado in edit e apro in automatico la vista di riga
		GetDocument()->EditArchivedDoc();
		CallDialog();
	}
	else
	{
		GetDocument()->SaveArchivedDoc();
		// dopo il salvataggio vado ad aggiornare le info nella riga del DBT
		VArchivedDocument* pRec = GetDocument()->m_pDBTArchivedDocuments->GetCurrent();
		if (pRec)
		{
			// aggiorno la colonna Descrizione solo e' cambiata (e relativa data modifica)
			DataStr sDescri = GetDocument()->m_pCurrentAttInfo->m_Description;
			if (pRec->l_Description.CompareNoCase(sDescri) != 0)
			{
				pRec->l_Description = sDescri;
				pRec->l_ModifiedDate = GetDocument()->m_pCurrentAttInfo->m_ModifiedDate;
				GetDocument()->UpdateDataView();
			}
		}
	}

	UpdateEditSaveToolbarBtn();
}

// evento sul click del bottone della toolbar Undo
//-----------------------------------------------------------------------------	
void CArchivedDocumentsBodyEdit::OnUndoCheckOutClicked()
{
	if (m_bIsActiveMultipleSelection && CountSelectedRows() > 0)
	{
		CUIntArray* pIndexes = GetSelectedRowsIndexes();
		GetDocument()->UndoCheckOutFile(pIndexes);
		SAFE_DELETE(pIndexes);
	}
	else
		GetDocument()->UndoCheckOutFile();
	
	UpdateCheckInCheckOutToolbarBtn();
}

// evento sul click del bottone della toolbar Checkin/Checkout
//-----------------------------------------------------------------------------	
void CArchivedDocumentsBodyEdit::OnCheckInOutClicked()
{
	if (m_bIsActiveMultipleSelection && CountSelectedRows() > 0)
	{
		CUIntArray* pIndexes = GetSelectedRowsIndexes();
		GetDocument()->CheckOutFile(pIndexes);
		SAFE_DELETE(pIndexes);
	}
	else
		GetDocument()->CheckOutFile();

	UpdateCheckInCheckOutToolbarBtn();
}

//-----------------------------------------------------------------------------	
void CArchivedDocumentsBodyEdit::OnBeginMultipleSel()
{
	if (GetDocument()->m_bEditMode) return;

	m_bIsActiveMultipleSelection = TRUE;
	EnableButtons(); // forzo la riabilitazione dei pulsanti della toolbar
}

//-----------------------------------------------------------------------------	
void CArchivedDocumentsBodyEdit::OnEndMultipleSel()
{
	m_bIsActiveMultipleSelection = FALSE;
	EnableButtons(); // forzo la riabilitazione dei pulsanti della toolbar
}

//-----------------------------------------------------------------------------	
BOOL CArchivedDocumentsBodyEdit::OnDblClick(UINT nFlags, CBodyEditRowSelected* pCurrentRow)
{
	GetDocument()->ShowDocuments();
	return TRUE;
}

//-----------------------------------------------------------------------------	
BOOL CArchivedDocumentsBodyEdit::OnPostCreateClient()
{
	// dopo che ho creato il bodyedit lo abilito ad accettare il drag dei files
	ModifyStyleEx(0, WS_EX_ACCEPTFILES);
	return TRUE; 
}

//-----------------------------------------------------------------------------
BOOL CArchivedDocumentsBodyEdit::OnSubFolderFound()
{
	return (AfxMessageBox(_TB("Archive also files in subfolders?"), MB_YESNO | MB_ICONQUESTION) == IDYES);
}

//-----------------------------------------------------------------------------
void CArchivedDocumentsBodyEdit::OnDropFiles(const CStringArray& arDroppedFiles)
{
	GetDocument()->ArchiveFiles(&arDroppedFiles);
}

//-----------------------------------------------------------------------------	
void CArchivedDocumentsBodyEdit::Customize()
{
	__super::Customize();
}