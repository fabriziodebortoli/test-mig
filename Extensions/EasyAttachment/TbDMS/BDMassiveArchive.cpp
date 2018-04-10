#include "stdafx.h"

#include <TbGenlib\DMSAttachmentInfo.h>
#include <TbGenlib\TBLinearGauge.h>

#include <ExtensionsImages\CommonImages.h>

#include "TBRepositoryManager.h"
#include "BDMassiveArchive.h"
#include "UIMassiveArchive.h"
#include "TBDMSEnums.h"
#include "BDAcquisitionFromDevice.h"

#include "EasyAttachment\JsonForms\UIMassiveArchive\IDD_MASSIVEARCHIVE_WIZARD.hjson"
#include "EasyAttachment\JsonForms\UIMassiveArchive\IDD_RW_MASSIVEARCHIVE_FILESTOADD.hjson"
#include "EasyAttachment\JsonForms\UIAcquisitionFromDevice\IDD_ACQUISITION_FROM_DEVICE.hjson"

using namespace System;
using namespace System::Collections::Generic; 
using namespace Microarea::EasyAttachment;
using namespace Microarea::EasyAttachment::BusinessLogic;
using namespace Microarea::EasyAttachment::Components;
using namespace Microarea::TaskBuilderNet::Core::WebServicesWrapper;

///////////////////////////////////////////////////////////////////////////////
//						VFileToAttach declaration
///////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(VFileToAttach, SqlVirtualRecord)

//-----------------------------------------------------------------------------
VFileToAttach::VFileToAttach(BOOL bCallInit  /* = TRUE */)
	:
	SqlVirtualRecord		(_T("VFileToAttach")),
	l_IsSelected			(FALSE),
	m_pErpDocumentBarcodes	(NULL),
	attachInfoOtherData		(nullptr),
	m_pAttachmentInfo		(NULL)
{
	BindRecord();
	if (bCallInit) Init();
}

//-----------------------------------------------------------------------------
VFileToAttach::~VFileToAttach()
{
	SAFE_DELETE(m_pAttachmentInfo);
	SAFE_DELETE(m_pErpDocumentBarcodes);
}

//-----------------------------------------------------------------------------
void VFileToAttach::BindRecord()
{
	BEGIN_BIND_DATA();
		LOCAL_DATA(_NS_FLD("VIsSelected"),		l_IsSelected);
		LOCAL_KEY(_NS_FLD("VArchivedDocID"),	l_ArchivedDocID);
		LOCAL_KEY(_NS_FLD("VAttachmentID"),		l_AttachmentID);
		LOCAL_STR(_NS_FLD("VFileName"),			l_FileName,			256);
		LOCAL_STR(_NS_FLD("VMassiveAction"),	l_MassiveAction,	30);
		LOCAL_STR(_NS_FLD("VMassiveStatus"),	l_MassiveStatus,	30);
		LOCAL_STR(_NS_FLD("VMassiveResult"),	l_MassiveResult,	30);
		LOCAL_STR(_NS_FLD("VResultBmp"),		l_ResultBmp,		32);
		LOCAL_STR(_NS_FLD("VMassiveInfo"),		l_MassiveInfo,		200); 
	END_BIND_DATA();
}

//-----------------------------------------------------------------------------
LPCTSTR VFileToAttach::GetStaticName() { return _NS_TBL("VFileToAttach"); }

//-----------------------------------------------------------------------------
DMSAttachmentInfo* VFileToAttach::GetAttachmentInfo()
{
	SAFE_DELETE(m_pAttachmentInfo);

	if (attachInfoOtherData->Attachment != nullptr)
		m_pAttachmentInfo = CreateDMSAttachmentInfo(attachInfoOtherData->Attachment);

	return m_pAttachmentInfo;
}

//-----------------------------------------------------------------------------
::Array* VFileToAttach::GetErpDocumentBarcodes()
{
	if (!m_pErpDocumentBarcodes)
		m_pErpDocumentBarcodes = new ::Array();
	else
		m_pErpDocumentBarcodes->RemoveAll();

	if (attachInfoOtherData->Attachment != nullptr)
	{
		VAttachmentLink* pRec;
		for each (ERPDocumentBarcode^ doc in attachInfoOtherData->ERPDocumentsBarcode)
		{
			pRec = new VAttachmentLink();
			pRec->l_Image				= TBIcon(szIconDocument, TOOLBAR);
			pRec->l_TBDocNamespace		= doc->Namespace;
			pRec->l_TBPrimaryKey		= doc->PK;
			pRec->l_DocKeyDescription	= pRec->l_DocKeyDescription.IsEmpty() ? pRec->l_TBPrimaryKey : pRec->l_DocKeyDescription;

			// compongo la stringa con titolo documento e descrizione chiavi da visualizzare nel bodyedit su 2 righe
			String^ documentTitle = CUtility::GetDocumentTitle(gcnew String(pRec->l_TBDocNamespace.GetString()));
			pRec->l_DocumentDescription = documentTitle->ToString() + _T("\r\n") + pRec->l_DocKeyDescription;

			m_pErpDocumentBarcodes->Add(pRec);
		}
	}

	return m_pErpDocumentBarcodes;
}

//////////////////////////////////////////////////////////////////////////////
//				DBTFilesToArchive implementation
//////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(DBTFilesToArchive, DBTSlaveBuffered)

//-----------------------------------------------------------------------------	
DBTFilesToArchive::DBTFilesToArchive
	(
		CRuntimeClass*		pClass,
		CAbstractFormDoc*	pDocument,
		CString				sName // per disambiguare il nome del DBT nel JSON
	)
	:
	DBTSlaveBuffered(pClass, pDocument, sName, ALLOW_EMPTY_BODY, FALSE)
{
	SetAllowFilter(TRUE); // abilito il filtro in memoria
}

//-----------------------------------------------------------------------------
DataObj* DBTFilesToArchive::OnCheckPrimaryKey(int /*nRow*/, SqlRecord*)
{
	return NULL;
}

//-----------------------------------------------------------------------------
void DBTFilesToArchive::OnPreparePrimaryKey(int nRow, SqlRecord* pSqlRec)
{
	ASSERT(pSqlRec->IsKindOf(RUNTIME_CLASS(VFileToAttach)));
}

//-----------------------------------------------------------------------------
void DBTFilesToArchive::SetCurrentRow(int nRow)
{
	DBTSlaveBuffered::SetCurrentRow(nRow);
}

// aggiungo manualmente i SqlRecord al DBT
//-----------------------------------------------------------------------------	
BOOL DBTFilesToArchive::LocalFindData(BOOL bPrepareOld)
{
	RemoveAll();
	return TRUE;
}

//////////////////////////////////////////////////////////////////////////////
//				DBTDocumentLinks implementation
//////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(DBTDocumentLinks, DBTSlaveBuffered)

//-----------------------------------------------------------------------------	
DBTDocumentLinks::DBTDocumentLinks
	(
	CRuntimeClass*		pClass,
	CAbstractFormDoc*	pDocument
	)
	:
	DBTSlaveBuffered(pClass, pDocument, _NS_DBT("DBTDocumentLinks"), ALLOW_EMPTY_BODY, FALSE)
{
}

//-----------------------------------------------------------------------------
DataObj* DBTDocumentLinks::OnCheckPrimaryKey(int /*nRow*/, SqlRecord*)
{
	return NULL;
}

//-----------------------------------------------------------------------------
void DBTDocumentLinks::OnPreparePrimaryKey(int nRow, SqlRecord* pSqlRec)
{
	ASSERT(pSqlRec->IsKindOf(RUNTIME_CLASS(VAttachmentLink)));
}

//-----------------------------------------------------------------------------
void DBTDocumentLinks::SetCurrentRow(int nRow)
{
	DBTSlaveBuffered::SetCurrentRow(nRow);
}

//-----------------------------------------------------------------------------	
BOOL DBTDocumentLinks::LocalFindData(BOOL bPrepareOld)
{
	RemoveAll();

	VFileToAttach* pRec = GetDocument()->m_pDBTFilesToAdd->GetCurrent();
	::Array* pLinksArray = (pRec) ? pRec->GetErpDocumentBarcodes() : NULL;
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

//===========================================================================
//							BarcodeManagerEvents
// ref class that manages communication between Event Handlers and C++ CWnd
//===========================================================================
void BarcodeManagerEvents::InitializeEvents(BDMassiveArchive* pDoc, Microarea::EasyAttachment::BusinessLogic::BarcodeManager^ barcodeManager)
{
	m_pDoc = pDoc;
	if (barcodeManager)
	{
		barcodeManager->MassiveObjectAdded += gcnew EventHandler<MassiveEventArgs^>(this, &BarcodeManagerEvents::OnMassiveRowAdded);
		barcodeManager->MassiveRowProcessed += gcnew EventHandler<MassiveEventArgs^>(this, &BarcodeManagerEvents::OnMassiveRowProcessed);
		barcodeManager->MassiveOperationCompleted += gcnew EventHandler<EventArgs^>(this, &BarcodeManagerEvents::OnMassiveOperationCompleted);		
	}
}

//-----------------------------------------------------------------------------
void BarcodeManagerEvents::OnMassiveRowAdded(System::Object^, Microarea::EasyAttachment::Components::MassiveEventArgs^ eventArg)
{
	if (m_pDoc && eventArg != nullptr)
	{
		VFileToAttach* pFileToAttach = new VFileToAttach();
		pFileToAttach->attachInfoOtherData = eventArg->aiod;

		if (eventArg->aiod != nullptr && eventArg->aiod->Attachment != nullptr)
		{
			pFileToAttach->l_ArchivedDocID	= eventArg->aiod->Attachment->ArchivedDocId;
			pFileToAttach->l_AttachmentID	= eventArg->aiod->Attachment->AttachmentId;
			pFileToAttach->l_FileName		= eventArg->aiod->Attachment->Name;
			pFileToAttach->l_FilePath		= eventArg->aiod->Attachment->OriginalPath;
			pFileToAttach->l_MassiveStatus	= BarcodeManager::GetStatusText(eventArg->aiod->BarCodeStatus);
			pFileToAttach->l_MassiveAction	= BarcodeManager::GetActionToDoText(eventArg->aiod->ActionToDo);
			pFileToAttach->l_MassiveInfo	= BarcodeManager::GetInfoText(eventArg->aiod->BarCodeStatus, eventArg->aiod->ActionToDo);

			// sono su un thread separato! 
			AfxInvokeThreadProcedure<BDMassiveArchive, VFileToAttach*>(m_pDoc->GetFrameHandle(), m_pDoc, &BDMassiveArchive::OnMassiveRowAdded, pFileToAttach);
		}		
	}
}
//-----------------------------------------------------------------------------
VFileToAttach* GetVFileToAttachThreadSafe()
{
	VFileToAttach* pFileToAttach = new VFileToAttach();
	return pFileToAttach;
}

//-----------------------------------------------------------------------------
void BarcodeManagerEvents::OnMassiveRowProcessed(System::Object^, Microarea::EasyAttachment::Components::MassiveEventArgs^ eventArg)
{
	if (m_pDoc && eventArg != nullptr)
	{
		VFileToAttach* pFileToAttach = AfxInvokeThreadGlobalFunction<VFileToAttach*>(m_pDoc->GetThreadId(), &GetVFileToAttachThreadSafe);
		pFileToAttach->attachInfoOtherData = eventArg->aiod;

		if (eventArg->aiod != nullptr && eventArg->aiod->Attachment != nullptr)
		{
			pFileToAttach->l_ArchivedDocID	= eventArg->aiod->Attachment->ArchivedDocId;
			pFileToAttach->l_AttachmentID	= eventArg->aiod->Attachment->AttachmentId;
			pFileToAttach->l_FileName		= eventArg->aiod->Attachment->Name;
			pFileToAttach->l_FilePath		= eventArg->aiod->Attachment->OriginalPath;
			pFileToAttach->l_MassiveStatus	= BarcodeManager::GetStatusText(eventArg->aiod->BarCodeStatus);
			pFileToAttach->l_MassiveAction	= BarcodeManager::GetResultText(eventArg->aiod->Result);

			switch (eventArg->aiod->Result)
			{
				case MassiveResult::Failed:
					pFileToAttach->l_ResultBmp = TBGlyph(szIconError);break;
				case MassiveResult::WithError:
					pFileToAttach->l_ResultBmp = TBGlyph(szIconInfo); break;
				case MassiveResult::Done:
				case MassiveResult::Ignored:
					pFileToAttach->l_ResultBmp = TBGlyph(szIconOk); break;
				default:
					pFileToAttach->l_ResultBmp = TBGlyph(szIconError); break;
			}
			// sono su un thread separato! 
			AfxInvokeThreadProcedure<BDMassiveArchive, VFileToAttach*>(m_pDoc->GetFrameHandle(), m_pDoc, &BDMassiveArchive::OnMassiveRowProcessed, pFileToAttach);
		}
	}
}

//-----------------------------------------------------------------------------
void BarcodeManagerEvents::OnMassiveOperationCompleted(System::Object^, System::EventArgs^ eventArg)
{
	if (m_pDoc)
	{
		// sono su un thread separato! 
		AfxInvokeThreadProcedure<BDMassiveArchive>(m_pDoc->GetFrameHandle(), m_pDoc, &BDMassiveArchive::OnMassiveOperationCompleted);
	}
}

/////////////////////////////////////////////////////////////////////////////
//				class BDMassiveArchive Implementation
/////////////////////////////////////////////////////////////////////////////
//----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(BDMassiveArchive, CWizardFormDoc)

//----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(BDMassiveArchive, CWizardFormDoc)
	////{{AFX_MSG_MAP(BDMassiveArchive)
	ON_CONTROL(BEN_ROW_CHANGED, IDC_MASSIVEARCHIVE_ADDFILES_BE_FILESTOADD, OnAddFileRowChanged)

	ON_COMMAND(ID_MASSIVEARCHIVE_BTN_ADD_FROM_FILESYSTEM,	OnAddFileFromFileSystem)
	ON_COMMAND(ID_MASSIVEARCHIVE_BTN_ADD_FROM_DEVICE,		OnAddFileFromDevice)
	ON_COMMAND(ID_MASSIVEARCHIVE_BTN_SELDESEL,				OnSelDeselClicked)
	ON_COMMAND(ID_SCANPROCESS_ENDED,						OnScanProcessEnded)

	//@@TODOMICHI: per disabilitare la delete row nella sola rowview: al momento non gestito
	ON_UPDATE_COMMAND_UI(ID_EXTDOC_DELETE_ROW,				OnUpdateDeleteRow) 
	////}}AFX_MSG_MAP
END_MESSAGE_MAP()

//----------------------------------------------------------------------------
BDMassiveArchive::BDMassiveArchive()
	:
	dmsOrchestrator				(nullptr),
	barcodeManagerEvents		(nullptr),
	attachmentInfoOtherDataList	(nullptr),
	m_pDBTFilesToAdd			(NULL),
	m_pDBTProcessedFiles		(NULL),
	m_pDBTDocumentLinks			(NULL),
	m_pCurrentAttInfo			(NULL),
	m_bSplitFile				(FALSE),
	m_bCanShowElaborationDlg	(FALSE),
	m_bCanClose					(TRUE),
	m_pGauge					(NULL),
	m_nCurrentElement			(0),
	m_GaugeRange				(100),
	m_Range						(0),
	m_pBEBtnSelDesel			(NULL),
	m_pBarcodeViewer			(NULL)
{
}

//----------------------------------------------------------------------------
BDMassiveArchive::~BDMassiveArchive()
{
	SAFE_DELETE(m_pDBTFilesToAdd);
	SAFE_DELETE(m_pDBTProcessedFiles);
	SAFE_DELETE(m_pDBTDocumentLinks);
	SAFE_DELETE(m_pCurrentAttInfo);
	
	if ((DMSOrchestrator^)dmsOrchestrator != nullptr)
	{
		delete dmsOrchestrator;
		dmsOrchestrator = nullptr;
	}
}

//-----------------------------------------------------------------------------
BOOL BDMassiveArchive::OnAttachData()
{
	__super::OnAttachData();	// serve per i titoli

	SetFormTitle(_TB("DMS Massive Attach"));
	SetFormName(_TB("DMS Massive Attach"));

	dmsOrchestrator = gcnew DMSOrchestrator();
	AfxGetTbRepositoryManager()->InitializeManager(this);
	dmsOrchestrator->InUnattendedMode = true;
	barcodeManager = dmsOrchestrator->BarcodeManager;

	barcodeManagerEvents = gcnew BarcodeManagerEvents();
	barcodeManagerEvents->InitializeEvents(this, barcodeManager);

	// istanzio l'AttachmentInfo corrente
	m_pCurrentAttInfo = new DMSAttachmentInfo();

	m_pDBTFilesToAdd		= new DBTFilesToArchive	(RUNTIME_CLASS(VFileToAttach), this, _T("DBTFilesToAdd"));
	m_pDBTProcessedFiles	= new DBTFilesToArchive	(RUNTIME_CLASS(VFileToAttach), this, _T("DBTArchivedFiles"));
	m_pDBTDocumentLinks		= new DBTDocumentLinks	(RUNTIME_CLASS(VAttachmentLink), this);

	attachmentInfoOtherDataList = gcnew List<AttachmentInfoOtherData^>();

	DECLARE_VAR_JSON(bSplitFile);
	DECLARE_VAR_JSON(ElaborationMessage);
	DECLARE_VAR_JSON(nCurrentElement);

	// variabili rowview
	DECLARE_VAR(_T("DocBarcodeValue"),			m_pCurrentAttInfo->m_BarcodeValue);
	DECLARE_VAR(_T("DocBarcodeValueViewer"),	m_pCurrentAttInfo->m_BarcodeValue);

	RegisterControl(IDC_MASSIVEARCHIVE_BE_DOC_LINKS, RUNTIME_CLASS(CAttachmentLinksBodyEdit));

	InitSelections();

	return TRUE;
}

//----------------------------------------------------------------------------
void BDMassiveArchive::OnParsedControlCreated(CParsedCtrl* pCtrl)
{
	__super::OnParsedControlCreated(pCtrl);

	UINT nIDC = pCtrl->GetCtrlID();

	if (nIDC == IDC_MASSIVEARCHIVE_RW_BARCODEVIEWER)
	{
		m_pBarcodeViewer = dynamic_cast<CTBDMSBarcodeViewerCtrl*>(pCtrl);
		if (!m_pBarcodeViewer) return;
		m_pBarcodeViewer->SetToolStripVisibility(FALSE);
		m_pBarcodeViewer->EnablePreviewNotAvailable(FALSE);
		m_pBarcodeViewer->SetSkipRecalcCtrlSize(TRUE);
		m_pBarcodeViewer->SetBarcode(m_pCurrentAttInfo->m_BarcodeValue, m_pCurrentAttInfo->m_BarcodeType); // serve per la visualizzazione del barcode
		return;
	}

	if (nIDC == IDC_MASSIVEARCHIVE_GAUGE)
	{
		m_pGauge = dynamic_cast<CTBLinearGaugeCtrl*>(pCtrl);
		if (m_pGauge)
		{
			// Questi reset del gauge servono a togliere il segnalino di avanzamento e la scala da 1 a 100 ecc.
			m_pGauge->SetMajorTickMarkSize(0);
			m_pGauge->SetMinorTickMarkSize(0);
			m_pGauge->SetMajorTickMarkStep(0);
			m_pGauge->RemovePointer(0);
			m_pGauge->SetBkgColor(AfxGetTileDialogStyleWizard()->GetBackgroundColor());
			m_pGauge->SetTextLabelFormat(_T(""));
			m_pGauge->RemoveAllColoredRanges();
		}
		return;
	}
}

//---------------------------------------------------------------------------
void BDMassiveArchive::OnWizardActivate(UINT nPageIDD)
{
	if (nPageIDD == IDD_WTD_MASSIVEARCHIVE_ADDFILES)
	{
		SetHeaderTitle(_TB(""));
		SetHeaderSubTitle(_TB("First of all add the files you want load in DMS."));

		CJsonWizardTabDialog* pWiz = (CJsonWizardTabDialog*)GetTabWizard()->GetActiveDlg();
		if (pWiz)
			pWiz->SetLast();

		return;
	}
}

//---------------------------------------------------------------------------
BOOL BDMassiveArchive::CanDoBatchExecute()
{
	//return TRUE;
	BOOL bEnable = GetTabWizard()->GetActiveDlg()->GetDlgInfoItem()->GetDialogID() == IDD_WTD_MASSIVEARCHIVE_ADDFILES;
	return bEnable;
}

//-----------------------------------------------------------------------------
void BDMassiveArchive::CustomizeBodyEdit(CBodyEdit* pBodyEdit)
{
	__super::CustomizeBodyEdit(pBodyEdit);

	if (pBodyEdit->GetNamespace().GetObjectName() == _NS_BE("BEMassiveArchiveFilesToAdd"))
	{
		CFilesToArchiveBodyEdit* pResultsBE = (CFilesToArchiveBodyEdit*)pBodyEdit;
		// a regime questo dovrebbe sparire!
		if (pResultsBE)
			m_pBEBtnSelDesel = pResultsBE->m_HeaderToolBar.FindButton(ID_MASSIVEARCHIVE_BTN_SELDESEL);
		return;
	}
}

//-----------------------------------------------------------------------------
BOOL BDMassiveArchive::CanRunDocument()
{
	if (!AfxGetOleDbMng()->DMSMassiveEnable())
	{
		AfxMessageBox(_TB("Impossible to open Massive Attach form!\r\nPlease, check in Administration Console if this company uses DMS and if DMS Advanced is activated."));
		return FALSE;
	}

	if (!AfxGetTbRepositoryManager()->BarcodeEnabled())
	{
		AfxMessageBox(_TB("In order to open Massive Attach form change your setting parameter and enable Barcode."));
		return FALSE;
	}

	return TRUE;
}

//-----------------------------------------------------------------------------
void BDMassiveArchive::OnPrepareAuxData(CTileDialog* pTile)
{
	UINT nIDC = (UINT)pTile->GetDialogID();
	// update della preview del barcode
	if (nIDC == IDD_TD_MASSIVEARCHIVE_RW_BARCODE)
		DoAttachmentInfoChanged();
}

//-----------------------------------------------------------------------------
void BDMassiveArchive::DoAttachmentInfoChanged()
{
	if (!m_pCurrentAttInfo)
		return;

	CView* pRowView = ViewAlreadyPresent(RUNTIME_CLASS(CJsonRowView));
	if (!pRowView)
		return;

	CJsonRowView* pJsonView = (CJsonRowView*)pRowView;
	if (!pJsonView)
		return;

	if (pJsonView->GetTileDialog(IDD_TD_MASSIVEARCHIVE_RW_BARCODE))
	{
		if (m_pBarcodeViewer)
			m_pBarcodeViewer->SetBarcode(m_pCurrentAttInfo->m_BarcodeValue, m_pCurrentAttInfo->m_BarcodeType);
	}
	UpdateDataView();
}

// compone la stringa da visualizzare nella caption della rowview
//-----------------------------------------------------------------------------
CString	BDMassiveArchive::OnGetCaption(CAbstractFormView* pView)
{
	CString sCaption = _T("");

	if (pView->GetNamespace().GetObjectName() == _T("MassiveArchiveRowView"))
	{
		VFileToAttach* pRec = (VFileToAttach*)m_pDBTFilesToAdd->GetCurrentRow();
		if (pRec)
			sCaption = _TB("File name: ") + pRec->l_FileName;
	}

	return sCaption;
}

//@@TODOMICHI: per disabilitare la delete row nella sola rowview: al momento non gestito
// da rivedere con la nuova gestione
//-----------------------------------------------------------------------------
void BDMassiveArchive::OnUpdateDeleteRow(CCmdUI* pCmdUI)
{
	// tolgo la possibilità di eliminare le righe dalla rowview
	pCmdUI->Enable(FALSE);
}

// evento sul cambio riga del bodyedit
//-----------------------------------------------------------------------------
void BDMassiveArchive::OnAddFileRowChanged()
{
	VFileToAttach* pRec = m_pDBTFilesToAdd->GetCurrent();
	if (!pRec) 
		return;

	DMSAttachmentInfo* pRecAttInfo = pRec->GetAttachmentInfo();
	if (!pRecAttInfo)
		m_pCurrentAttInfo->Clear();
	else
		*m_pCurrentAttInfo = *pRecAttInfo;

	m_pDBTDocumentLinks->LocalFindData(FALSE);

	DoAttachmentInfoChanged();

	UpdateDataView();
}

// gestione mancante per identificare se esiste gia' una riga con il documento
// appena aggiunto (nel caso venga aggiunto lo stesso file piu' volte)
//-----------------------------------------------------------------------------
void BDMassiveArchive::OnSingleAttachCompleted(DMSAttachmentInfo* attachmentInfo)
{
	/*if (!attachmentInfo) return;

	BOOL bRowAlreadyExists = FALSE;

	DMSAttachmentInfo* attachInfo = attachmentInfo;
	VArchivedDocument* pRec;
	for (int i = 0; i <= m_pDBTSOSDocuments->GetUpperBound(); i++)
	{
		pRec = (VArchivedDocument*)m_pDBTSOSDocuments->GetRow(i);
		if (pRec->l_ArchivedDocId == attachInfo->m_ArchivedDocId)
		{
			m_pDBTSOSDocuments->SetCurrentRow(i);
			bRowAlreadyExists = TRUE;
			break;
		}
	}

	if (!bRowAlreadyExists)
	{
		pRec = (VArchivedDocument*)m_pDBTSOSDocuments->InsertRecord(0);
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

		m_pDBTSOSDocuments->SetCurrentRow(0);
	}

	delete attachmentInfo;*/
}

//-----------------------------------------------------------------------------
void BDMassiveArchive::InitSelections()
{
	m_bSplitFile = FALSE;
	m_pDBTFilesToAdd->RemoveAll();
	m_pDBTProcessedFiles->RemoveAll();

	UpdateDataView();
}

//-----------------------------------------------------------------------------
void BDMassiveArchive::AddFiles(const CStringArray* pFilesToAdd)
{
	if (!pFilesToAdd || pFilesToAdd->GetSize() <= 0)
		return;
	
	List<String^>^ filesToArchive = gcnew List<String^>();
	
	for (int i = 0; i < pFilesToAdd->GetSize(); i++)
		filesToArchive->Add(gcnew String(pFilesToAdd->GetAt(i)));

	dmsOrchestrator->BarcodeManager->MassivePreProcess(filesToArchive, (m_bSplitFile == TRUE) ? true : false);
}

//-----------------------------------------------------------------------------
void BDMassiveArchive::OnAddFileFromFileSystem()
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

	AddFiles(pArray);
	SAFE_DELETE(pArray);
}

//-----------------------------------------------------------------------------
void BDMassiveArchive::OnAddFileFromDevice()
{
	// apro la dialog per l'acquisizione da device
	m_pAcquisitionFromDevice =
		(BDAcquisitionFromDevice*)AfxGetTbCmdManager()->RunDocument(_NS_DOC("Extensions.EasyAttachment.TbDMS.AcquisitionFromDevice"), szDefaultViewMode, NULL, NULL, this);

	GetMasterFrame()->EnableWindow(FALSE);
	AfxGetTbCmdManager()->WaitDocumentEnd(m_pAcquisitionFromDevice);
	GetMasterFrame()->EnableWindow(TRUE);
}

//-----------------------------------------------------------------------------	
void BDMassiveArchive::OnScanProcessEnded()
{
	if (m_pAcquisitionFromDevice)
		AddFiles(m_pAcquisitionFromDevice->m_pAcquiredFiles);
}

//-----------------------------------------------------------------------------	
void BDMassiveArchive::OnSelDeselClicked()
{
	m_bSelectDeselect = !m_bSelectDeselect;

	VFileToAttach* pRec;
	for (int i = 0; i <= m_pDBTFilesToAdd->GetUpperBound(); i++)
	{
		pRec = (VFileToAttach*)m_pDBTFilesToAdd->GetRow(i);
		pRec->l_IsSelected = !m_bSelectDeselect;
	}

	m_pBEBtnSelDesel->SetText(m_bSelectDeselect ? _TB("Select") : _TB("Deselect"));

	UpdateDataView();
}

// sul doppio click del bodyedit apro il file temporaneo con il programma predefinito
//-----------------------------------------------------------------------------
void BDMassiveArchive::ShowDocument(SqlRecord* pCurrentRow)
{
	if (!pCurrentRow)
		return;

	VFileToAttach* pRec = (VFileToAttach*)pCurrentRow;
	if (!pRec->l_FilePath.IsEmpty())
		AfxGetTbRepositoryManager()->OpenDocument(pRec->l_FilePath + _T("\\") + pRec->l_FileName);
}

//-----------------------------------------------------------------------------
BOOL BDMassiveArchive::CheckSelections()
{
	// prima di elaborare verifico che ci siano dei file selezionati
	attachmentInfoOtherDataList->Clear();

	// mi tengo da parte gli AttachmentInfo delle righe selezionate e nel frattempo le conto
	int nSelRowsCount = 0;
	for (int i = 0; i < m_pDBTFilesToAdd->GetSize(); i++)
	{
		VFileToAttach* pRec = (VFileToAttach*)m_pDBTFilesToAdd->GetRow(i);
		if (pRec->l_IsSelected)
		{
			attachmentInfoOtherDataList->Add(pRec->attachInfoOtherData);
			nSelRowsCount++;
		}
	}

	if (nSelRowsCount == 0)
	{
		Message(_TB("You have to select at least one file! Unable to proceed!"), MB_ICONSTOP);
		m_bCanShowElaborationDlg = FALSE;
		return FALSE;
	}

	return TRUE;
}

//-----------------------------------------------------------------------------
void BDMassiveArchive::OnBatchExecute()
{
	// prima di elaborare verifico che ci siano dei file selezionati
	m_bCanShowElaborationDlg = CheckSelections();

	if (!m_bCanShowElaborationDlg)
		return;

	BeginWaitCursor();
	m_bCanClose = FALSE;
	m_bBatchRunning = TRUE;

	// in questo metodo parte un thread separato di elaborazione lato C#
	barcodeManager->MassiveProcessThread(attachmentInfoOtherDataList);

	m_bBatchRunning = FALSE;

	UpdateDataView();
}

//----------------------------------------------------------------------------
void BDMassiveArchive::OnBatchCompleted()
{
	__super::OnBatchCompleted();

	if (m_bCanShowElaborationDlg) // mi posiziono sull'ultima pagina solo se le selezioni sono ok
	{
		((CMassiveArchiveWizardFormView*)GetFirstView())->OnWizardNext();
		
		// faccio partire il timer solo se e' partita l'elaborazione
		StartTimer();
	}
}

//---------------------------------------------------------------------------
void BDMassiveArchive::OnCloseDocument()
{
	if (!m_bCanClose)
	{
		AfxMessageBox(_TB("Unable to close the form because the process is running!\r\nPlease try again in a few seconds..."));
		return;
	}

	CWizardFormDoc::OnCloseDocument();
}

//---------------------------------------------------------------------------------------------
void BDMassiveArchive::StartTimer()
{
	// scatta ogni secondo
	GetFirstView()->SetTimer(CHECK_MASSIVEARCHIVE_TIMER, 1000, NULL);
	SetProgressRange(100);
	if (m_pGauge)
		m_pGauge->UpdateCtrlView();
}

//-----------------------------------------------------------------------------------------------
void BDMassiveArchive::EndTimer()
{
	GetFirstView()->KillTimer(CHECK_MASSIVEARCHIVE_TIMER);
}

//------------------------------------------------------------------------------
void BDMassiveArchive::DoOnTimer()
{
	StepProgressBar();
	UpdateDataView();
}

//-----------------------------------------------------------------------------
void BDMassiveArchive::StepProgressBar()
{
	if (!m_pGauge)
		return;

	m_nCurrentElement += 1;
	SetGaugeColors();

	m_pGauge->UpdateCtrlView();
	UpdateDataView();
}

//------------------------------------------------------------------------------------
void BDMassiveArchive::SetGaugeColors()
{
	if (!m_pGauge)
		return;
	COLORREF cBlue = AfxGetTileDialogStyleNormal()->GetTitleSeparatorColor();

	m_pGauge->RemoveAllColoredRanges();

	DataDbl nPos = (m_GaugeRange * m_nCurrentElement) / m_Range;
	if (nPos > m_Range) // se ho raggiunto il max riazzero il CurrentElement
		m_nCurrentElement = 0;
	m_pGauge->AddColoredRange(0, nPos, cBlue);
}

//-----------------------------------------------------------------------------
void BDMassiveArchive::SetProgressRange(int nRange)
{
	if (nRange <= 0)
		return;

	m_Range = nRange;

	m_GaugeRange = 100;
	m_nCurrentElement = 0;

	m_pGauge->SetGaugeRange(0, m_GaugeRange);
	m_pGauge->RemoveAllColoredRanges();

	UpdateDataView();
}

//-----------------------------------------------------------------------------
void BDMassiveArchive::OnMassiveRowAdded(VFileToAttach* pFileToAttach)
{
	VFileToAttach* pNewRec = (VFileToAttach*)m_pDBTFilesToAdd->AddRecord();
	*pNewRec = *pFileToAttach;
	pNewRec->attachInfoOtherData = pFileToAttach->attachInfoOtherData;
	delete pFileToAttach;
	UpdateDataView();
}

//-----------------------------------------------------------------------------
void BDMassiveArchive::OnMassiveRowProcessed(VFileToAttach* pFileToAttach)
{
	VFileToAttach* pNewRec = (VFileToAttach*)m_pDBTProcessedFiles->AddRecord();
	*pNewRec = *pFileToAttach;
	pNewRec->attachInfoOtherData = pFileToAttach->attachInfoOtherData;
	
	m_ElaborationMessage = cwsprintf(_TB("Elaboration file {0-%s} in progress..."), pNewRec->l_FileName.Str());

	delete pFileToAttach;
	UpdateDataView();
}

//-----------------------------------------------------------------------------
void BDMassiveArchive::OnMassiveOperationCompleted()
{
	m_bCanClose = TRUE;
	EndWaitCursor();

	EndTimer();

	m_nCurrentElement = 100;
	SetGaugeColors();

	m_ElaborationMessage = _TB("Elaboration has been completed.");

	UpdateDataView();
}
