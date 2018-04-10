#include "stdafx.h" 

#include <TbNameSolver\ThreadContext.h>
#include <TbNamesolver\FileSystemFunctions.h>
#include <TbGeneric\globals.h>
#include <TbGeneric\dataobj.h>
#include <TbGenlib\baseapp.h>
#include <TbGes\DBT.h>
#include <TbGes\Tabber.h>
#include <TbWoormViewer\WoormDoc.h>
#include <TbGenlib\BarCode.h>
#include <TbFrameworkImages\CommonImages.h>
#include <ExtensionsImages\CommonImages.h>

#include "CDDMS.h"
#include "CDDMS.hjson" 
#include "ParsedControlWrapper.h"
#include "DMSSearchFilter.h"
#include "CommonObjects.h"
#include "SOSObjects.h"

#include "UIAttachment.h"
#include "UIAttachment.hjson" 
#include "BDDMSRepository.h"
#include "BDAcquisitionFromDevice.h"
#include "UIPaperyDlg.h"

#include "EasyAttachment\JsonForms\UIPaperyDlg\IDD_PAPERY.hjson"
#include "EasyAttachment\JsonForms\UIDMSRepository\IDD_DMSREPOSITORY_EXPLORER.hjson"
#include "EasyAttachment\JsonForms\UIAcquisitionFromDevice\IDD_ACQUISITION_FROM_DEVICE.hjson"

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

using namespace System;
using namespace System::Collections::Generic;
using namespace System::Data;
using namespace System::Diagnostics;
using namespace System::Globalization;
using namespace System::Threading;
using namespace System::Windows::Forms;

using namespace Microarea::Framework::TBApplicationWrapper;
using namespace Microarea::TaskBuilderNet::Interfaces::Model;
using namespace Microarea::EasyAttachment;
using namespace Microarea::EasyAttachment::UI;
using namespace Microarea::EasyAttachment::BusinessLogic;
using namespace Microarea::EasyAttachment::Components;

///////////////////////////////////////////////////////////////////////////////
//	class CDMSAttachmentManager
///////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CDMSAttachmentManager, CDMSAttachmentManagerObj)

//-----------------------------------------------------------------------------
CDMSAttachmentManager::CDMSAttachmentManager(CAbstractFormDoc* pDocument)
	:
	m_pDMS (NULL)
{
	AttachDocument(pDocument);
	m_pDocument->SetDMSAttachmentManager(this);
	m_pDMS = (CDDMS*)pDocument->GetClientDoc(_T("extensions.easyattachment.tbdms.CDDMS"));
}

//-----------------------------------------------------------------------------
::ArchiveResult	CDMSAttachmentManager::AttachReport(const CString& strPdfFileName, const CString& strReportTitle, const CString& strBarcode, int& nAttachmentId, CString& strMessage)
{
	strMessage.Empty();

	if (m_pDocument->IsInUnattendedMode())
		m_pDocument->GetMessages()->StartSession();
		
	::ArchiveResult archiveResult = m_pDMS->AttachFile(nAttachmentId, strPdfFileName, strReportTitle, true, true, strBarcode);
	
	if (archiveResult == ::ArchiveResult::TerminatedWithError)
	{
		if (m_pDocument->IsInUnattendedMode())
		{
			if (m_pDocument->GetMessages()->ErrorFound())
				strMessage = cwsprintf(_TB("The following errors occured during attaching process: {0-%s}"), m_pDocument->GetMessages()->ToString());
			m_pDocument->GetMessages()->ClearMessages();
			m_pDocument->GetMessages()->EndSession();
		}
	}

	return archiveResult;
}

//-----------------------------------------------------------------------------
::ArchiveResult CDMSAttachmentManager::AttachFile(const CString& strFileName, const CString& strDescription, int& nAttachmentID, CString& strMessage)
{
	strMessage.Empty();
	if (!m_pDocument->ValidCurrentRecord())
	{
		strMessage = _TB("The document has no valid record.");
		return ::ArchiveResult::TerminatedWithError;
	}

	if (m_pDocument->IsInUnattendedMode())
		m_pDocument->GetMessages()->StartSession();

	::ArchiveResult archiveResult = m_pDMS->AttachFile(nAttachmentID, strFileName, strDescription, false, false, _T(""));

	if (archiveResult == ::ArchiveResult::TerminatedWithError)	{
		if (m_pDocument->IsInUnattendedMode())
		{
			if (m_pDocument->GetMessages()->ErrorFound())
				strMessage = cwsprintf(_TB("The following errors occured during attaching process: {0-%s}"), m_pDocument->GetMessages()->ToString());
			m_pDocument->GetMessages()->ClearMessages();
			m_pDocument->GetMessages()->EndSession();
		}
	}

	return archiveResult;
}

//-----------------------------------------------------------------------------
::ArchiveResult CDMSAttachmentManager::AttachBinaryContent(const DataBlob& binaryContent, const CString& strFileName, const CString& strDescription, int& nAttachmentID, CString& strMessage)
{
	strMessage.Empty();
	if (!m_pDocument->ValidCurrentRecord())
	{
		strMessage = _TB("The document has no valid record.");
		return ::ArchiveResult::TerminatedWithError;
	}

	DataSize pDataSize;
	System::IntPtr intPointer = (System::IntPtr)((void*)binaryContent.GetRawData(&pDataSize));
	
	if (m_pDocument->IsInUnattendedMode())
		m_pDocument->GetMessages()->StartSession();

	::ArchiveResult archiveResult = m_pDMS->AttachBinaryContent(binaryContent, strFileName, strDescription, nAttachmentID);
	
	if (archiveResult == ::ArchiveResult::TerminatedWithError)
	{
		if (m_pDocument->IsInUnattendedMode())
		{
			if (m_pDocument->GetMessages()->ErrorFound())
				strMessage = cwsprintf(_TB("The following errors occured during attaching process: {0-%s}"), m_pDocument->GetMessages()->ToString());
			m_pDocument->GetMessages()->ClearMessages();
			m_pDocument->GetMessages()->EndSession();
		}
	}
	return archiveResult;
}

//-----------------------------------------------------------------------------
::ArchiveResult CDMSAttachmentManager::AttachArchivedDocument(int nArchivedDocID, int& nAttachmentID, CString& strMessage)
{
	strMessage.Empty();
	if (!m_pDocument->ValidCurrentRecord())
	{
		strMessage = _TB("The document has no valid record.");
		return ::ArchiveResult::TerminatedWithError;
	}

	if (m_pDocument->IsInUnattendedMode())
		m_pDocument->GetMessages()->StartSession();

	::ArchiveResult archiveResult = m_pDMS->AttachArchivedDocument(nArchivedDocID, nAttachmentID);

	if (archiveResult == ::ArchiveResult::TerminatedWithError)
	{
		if (m_pDocument->IsInUnattendedMode())
		{
			if (m_pDocument->GetMessages()->ErrorFound())
				strMessage = cwsprintf(_TB("The following errors occured during attaching process: {0-%s}"), m_pDocument->GetMessages()->ToString());
			m_pDocument->GetMessages()->ClearMessages();
			m_pDocument->GetMessages()->EndSession();
		}
	}
	return archiveResult;
}

//-----------------------------------------------------------------------------
::ArchiveResult CDMSAttachmentManager::AttachFromTable(CString& strMessage)
{
	strMessage.Empty();
	if (!m_pDocument->ValidCurrentRecord())
	{
		strMessage = _TB("The document has no valid record.");
		return ::ArchiveResult::TerminatedWithError;
	}

	if (m_pDocument->IsInUnattendedMode())
		m_pDocument->GetMessages()->StartSession();

	::ArchiveResult archiveResult = m_pDMS->AttachFromTable();
	if (archiveResult == ::ArchiveResult::TerminatedWithError)
	{
		if (m_pDocument->IsInUnattendedMode())
		{
			if (m_pDocument->GetMessages()->ErrorFound())
				strMessage = cwsprintf(_TB("The following errors occured during attaching process: {0-%s}"), m_pDocument->GetMessages()->ToString());
			m_pDocument->GetMessages()->ClearMessages();
			m_pDocument->GetMessages()->EndSession();
		}
	}
	return archiveResult;
}

//-----------------------------------------------------------------------------
::ArchiveResult CDMSAttachmentManager::AttachPapery(const CString& strBarcode, const CString& strDescription, const CString& strReportName, CString& strMessage)
{
	if (!m_pDocument->ValidCurrentRecord())
	{
		strMessage = _TB("The document has no valid record.");
		return ::ArchiveResult::TerminatedWithError;
	}

	if (m_pDocument->IsInUnattendedMode())
		m_pDocument->GetMessages()->StartSession();

	::ArchiveResult archiveResult = m_pDMS->AttachPapery(strBarcode, strDescription, strReportName);
	if (archiveResult == ::ArchiveResult::TerminatedWithError)
	{
		if (m_pDocument->IsInUnattendedMode())
		{
			if (m_pDocument->GetMessages()->ErrorFound())
				strMessage = cwsprintf(_TB("The following errors occured during attaching process: {0-%s}"), m_pDocument->GetMessages()->ToString());
			m_pDocument->GetMessages()->ClearMessages();
			m_pDocument->GetMessages()->EndSession();
		}
	}
	return archiveResult;
}

//-----------------------------------------------------------------------------
CAttachmentsArray* CDMSAttachmentManager::GetAttachments(AttachmentFilterTypeEnum filterType)
{
	if (!m_pDocument->ValidCurrentRecord())
		return NULL;

	return AfxGetIDMSRepositoryManager()->GetAttachments(m_pDocument->GetNamespace().ToString(), m_pDocument->m_pDBTMaster->GetRecord()->GetPrimaryKeyNameValue(), filterType);
}

//-----------------------------------------------------------------------------
CAttachmentInfo* CDMSAttachmentManager::GetAttachmentInfo(int attachmentID)
{
	return AfxGetTbRepositoryManager()->GetAttachmentInfo(attachmentID);
}

//-----------------------------------------------------------------------------
void CDMSAttachmentManager::OpenAttachmentsListForm(CUIntArray* pSelectedAttachments, bool onlyForMail)
{
	if (!m_pDocument->ValidCurrentRecord())
		return;

	m_pDMS->OpenAttachmentsListForm(pSelectedAttachments, onlyForMail);
}

//-----------------------------------------------------------------------------
bool CDMSAttachmentManager::CreateNewSosDocument(int nAttachmentID, CString& strMessage)
{
	strMessage.Empty();
	if (!m_pDocument->ValidCurrentRecord())
	{
		strMessage = _TB("The document has no valid record.");
		return false;
	}

	if (m_pDocument->IsInUnattendedMode())
		m_pDocument->GetMessages()->StartSession();

	bool ok = m_pDMS->CreateNewSosDocument(nAttachmentID);
	if (!ok)
	{
		if (m_pDocument->IsInUnattendedMode())
		{
			if (m_pDocument->GetMessages()->ErrorFound())
				strMessage = cwsprintf(_TB("The following errors occured during create new SOS document process: {0-%s}"), m_pDocument->GetMessages()->ToString());
			m_pDocument->GetMessages()->ClearMessages();
			m_pDocument->GetMessages()->EndSession();
		}
	}
	return ok;
}

//-----------------------------------------------------------------------------
bool CDMSAttachmentManager::IsDocumentInSOS()
{
	return m_pDMS->IsDocumentInSOS();
}

//===========================================================================
//							ManagedEventsHandler
// ref class that manages communication between Event Handlers and C++ CWnd
//===========================================================================
void MDMSDocOrchestratorEvents::InitializeEvents(CDDMS* pCDDMS, DMSDocOrchestrator^ dmsOrchestrator)
{
	cdDMS = pCDDMS;
	if (dmsOrchestrator)
	{
		dmsOrchestrator->AttachCompleted += gcnew EventHandler<AttachmentInfoEventArgs^>(this, &MDMSDocOrchestratorEvents::OnAttachCompleted);
		dmsOrchestrator->AttachmentDeleted += gcnew EventHandler<AttachmentEventArgs^>(this, &MDMSDocOrchestratorEvents::OnAttachmentDeleted);
		dmsOrchestrator->UpdateCollectionCompleted += gcnew EventHandler<Microarea::EasyAttachment::Components::CollectionEventArgs^>(this, &MDMSDocOrchestratorEvents::OnUpdateCollectionCompleted);
		dmsOrchestrator->SyncronizationIndexesFinished += gcnew EventHandler<System::EventArgs^>(this, &MDMSDocOrchestratorEvents::OnSyncronizationIndexesFinished);
	}
}

//-----------------------------------------------------------------------------
void MDMSDocOrchestratorEvents::OnAttachCompleted(System::Object^, Microarea::EasyAttachment::Components::AttachmentInfoEventArgs^ eventArg)
{
	if (cdDMS)
	{
		AttachmentInfo^ currAtt = eventArg->CurrentAttachment;
		cdDMS->OnAttachCompleted(CreateDMSAttachmentInfo(currAtt, FALSE));
		cdDMS->GetServerDoc()->DispatchDMSEvent(DMSEventTypeEnum::NewDMSAttachment, currAtt->AttachmentId);
	}
}

//-----------------------------------------------------------------------------
void MDMSDocOrchestratorEvents::OnAttachmentDeleted(System::Object^, Microarea::EasyAttachment::Components::AttachmentEventArgs^ eventArg)
{
	if (cdDMS)
		cdDMS->GetServerDoc()->DispatchDMSEvent(DMSEventTypeEnum::DeleteDMSAttachment, eventArg->AttachmentID);
}

//-----------------------------------------------------------------------------
void MDMSDocOrchestratorEvents::OnSyncronizationIndexesFinished(System::Object^, System::EventArgs^ eventArg)
{
	if (cdDMS)
		cdDMS->OnSyncronizationIndexesFinished();
	}

//-----------------------------------------------------------------------------
void MDMSDocOrchestratorEvents::OnUpdateCollectionCompleted(System::Object^, Microarea::EasyAttachment::Components::CollectionEventArgs^ eventArg)
{
	if (cdDMS)
	{
		cdDMS->OnUpdateCollectionCompleted();
		cdDMS->GetServerDoc()->DispatchDMSEvent(DMSEventTypeEnum::UpdateDMSCollection, eventArg->CollectionID);
	}
}

//////////////////////////////////////////////////////////////////////////////
//					     Class CDDMS definition
//////////////////////////////////////////////////////////////////////////////
//----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CDDMS, CClientDoc)

//----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CDDMS, CClientDoc)
	//{{AFX_MSG_MAP(CClientDoc)
	ON_COMMAND(ID_DMSREPOSITORYBROWSER_ATTACH_COMPLETED,OnRepositoryBrowserAttachCompleted)
	ON_COMMAND(ID_SCANPROCESS_ENDED,					OnScanProcessEnded)

	ON_COMMAND(ID_DMS_NEW_ATT_FILESYSTEM,				OnNewAttachmentFromFileSystem)
	ON_COMMAND(ID_DMS_NEW_ATT_REPOSITORY,				OnNewAttachmentFromRepository)
	ON_COMMAND(ID_DMS_NEW_ATT_DEVICE,					OnNewAttachmentFromDevice)
	ON_COMMAND(ID_DMS_NEW_ATT_PAPERY,					OnNewAttachmentFromPapery)
	ON_COMMAND(ID_DMS_RUN_PAPERY_REPORT,				OnRunCreatePaperyReport)
	ON_COMMAND(ID_DMS_UNDOCHECKOUT_ATTACHMENT,			OnUndoCheckOutAttachment)	
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//----------------------------------------------------------------------------
CDDMS::CDDMS()
:
	view						(nullptr),
	document					(nullptr),
	dmsDocOrchestrator			(nullptr),
	dmsOrchestratorEventsMng	(nullptr),
	m_pObserverContext			(NULL),
	hInstance					(NULL),
	m_bObserving				(false)	,	
	m_pShowAsDescriptionFields	(NULL),  
	m_pReportDoc				(NULL),
	m_AttachCount				(0),
	m_pAttachments				(NULL),
	m_pDBTBookmarks				(NULL),
	m_pCurrAttachmentInfo		(NULL),
	m_pAttachmentPane			(NULL),	
	m_bEditMode					(FALSE),
	m_pRepositoryBrowser		(NULL),
	m_pAcquisitionFromDevice	(NULL),
	m_pDMSAttachmentManager		(NULL),
	m_SOSDocumentStatus			(0),
	m_SOSAttachmentID			(-1),
	m_pCachedSearchFilter		(NULL),
	m_pPaperyBarcodeViewer		(NULL)
{
	m_pCurrAttachmentInfo	= new DMSAttachmentInfo();
	m_pAttachments			= new DMSAttachmentsList();
	m_pCachedSearchFilter	= new CSearchFilter();

	m_bMailActivated = AfxIsActivated(TBEXT_APP, MAILCONNECTOR_ACT);

	m_nMaxDocNrToShow = AfxGetTbRepositoryManager()->GetDMSSettings()->GetSettings()->l_MaxDocNumber;
}

//----------------------------------------------------------------------------
CDDMS::~CDDMS()
{
	SAFE_DELETE(m_pShowAsDescriptionFields);
	SAFE_DELETE(m_pDMSAttachmentManager);
}

//----------------------------------------------------------------------------
CAbstractFormDoc* CDDMS::GetServerDoc()
{
	CBaseDocument*	pServerDoc = GetMasterDocument();
	ASSERT(pServerDoc->IsKindOf(RUNTIME_CLASS(CAbstractFormDoc)));
	return (CAbstractFormDoc*)pServerDoc;
}

//----------------------------------------------------------------------------
BOOL CDDMS::OnInitDocument()
{
	m_pDMSAttachmentManager = new CDMSAttachmentManager(m_pServerDocument);
	return TRUE;
}

//----------------------------------------------------------------------------
BOOL CDDMS::OnAttachData() 
{
	Thread::CurrentThread->CurrentUICulture = gcnew CultureInfo(gcnew String(AfxGetCulture()));
	
	dmsDocOrchestrator = gcnew DMSDocOrchestrator(MDocument::Create<MDocument^>((IntPtr) m_pServerDocument));

	//alcune informazioni sono comuni a tutta i DMSOrchestrator	:
	// Settings
	// WorkersTable
	// plugins enabled (security, mailconnector, barcode...)
	// Model datacontext
	AfxGetTbRepositoryManager()->InitializeManager(this);
	
	dmsOrchestratorEventsMng = gcnew MDMSDocOrchestratorEvents();
	dmsOrchestratorEventsMng->InitializeEvents(this, dmsDocOrchestrator);

	m_pDBTBookmarks = new DBTBookmarks(RUNTIME_CLASS(VBookmark), m_pServerDocument);
	m_pDBTBookmarks->InstantiateFromClientDoc(this);

	// inizializzo le variabili per il JSON
	InitJSONVariables();

	m_nMaxDocNrToShow.SetAlwaysEditable();

	return TRUE;
}

// dichiarazione variabili del pannello Allegati per i controls descritti in JSON 
//----------------------------------------------------------------------------
void CDDMS::InitJSONVariables()
{
	m_bEnableBarcode				= AfxGetTbRepositoryManager()->GetDMSSettings()->GetSettings()->l_EnableBarcode;
	m_bAutomaticBarcodeDetection	= AfxGetTbRepositoryManager()->GetDMSSettings()->GetSettings()->l_AutomaticBarcodeDetection;
	// AddPapery slaveview
	DECLARE_VAR(_T("sPaperyDescripion"),		m_sPaperyDescripion);
	DECLARE_VAR(_T("sPaperyBarcodeValue"),	m_sPaperyBarcodeValue);
	DECLARE_VAR(_T("sPaperyNotes"),			m_sPaperyNotes);
	DECLARE_VAR(_T("PaperyBarcodeViewer"),	m_sPaperyBarcodeValue);
	m_sPaperyBarcodeType = CBarCodeTypes::BarCodeDescription(CBarCodeTypes::BarCodeType(MAKELONG((int)AfxGetTbRepositoryManager()->GetDMSSettings()->GetSettings()->l_BarcodeType.GetItemValue(), E_BARCODE_TYPE)));
	m_sPaperyDescripion = cwsprintf(_TB("Barcode type: {0-%s} - Prefix: {1-%s}"), m_sPaperyBarcodeType, AfxGetTbRepositoryManager()->GetDMSSettings()->GetSettings()->l_BarcodePrefix.Str());
	//
}

//-----------------------------------------------------------------------------
void CDDMS::OnParsedControlCreated(CParsedCtrl* pCtrl)
{
	__super::OnParsedControlCreated(pCtrl);

	UINT nIDC = pCtrl->GetCtrlID();
	if (nIDC == IDC_PAPERY_BARCODEVIEWER)
	{
		m_pPaperyBarcodeViewer = dynamic_cast<CTBDMSBarcodeViewerCtrl*>(pCtrl);
		if (!m_pPaperyBarcodeViewer) return;
		m_pPaperyBarcodeViewer->SetToolStripVisibility(FALSE);
		m_pPaperyBarcodeViewer->EnablePreviewNotAvailable(FALSE);
		m_pPaperyBarcodeViewer->SetSkipRecalcCtrlSize(TRUE);
		m_pPaperyBarcodeViewer->SetBarcode(m_sPaperyBarcodeValue, m_sPaperyBarcodeType); // serve per la visualizzazione del barcode
		return;
	}
}

//-----------------------------------------------------------------------------
void CDDMS::OnCloseServerDocument()
{
	// controlla se esiste gia' e nel caso lo chiude
	if (m_pReportDoc)
	{
		if (m_pReportDoc->SaveModified())
			m_pReportDoc->OnCloseDocument();
	}
	
	// se il RepoBrowser e' aperto forzo la sua chiusura
	if (m_pRepositoryBrowser)
		m_pRepositoryBrowser->OnCloseDocument();

	if (m_bObserving && m_pObserverContext)
		m_pObserverContext->EndObserving(ON_CHANGING);

	for (int i = 0; i <= m_arEventToDetach.GetUpperBound(); i++)
	{
		CBookmarkFieldEvents* bookmarkEvent = (CBookmarkFieldEvents*)m_arEventToDetach.GetAt(i);
		if (bookmarkEvent && bookmarkEvent->BookmarkToObserve)
		{
			MDataObj^ dummy = bookmarkEvent->BookmarkToObserve->DataObj;
			DataObj* dataObj = (DataObj*)dummy->DataObjPtr.ToInt64();
			if (dataObj)
				dataObj->DetachEvents(bookmarkEvent);
		}
	}
	m_arEventToDetach.RemoveAll();

	if (m_pObserverContext != nullptr)
	{
		delete m_pObserverContext;
		m_pObserverContext = nullptr;
	}

	if (m_pAttachments)
		delete m_pAttachments;

	//è il il DMSAttachmentInfo iniziale vuoto
	if (m_pCurrAttachmentInfo)
		delete m_pCurrAttachmentInfo;

	if (m_pDBTBookmarks)
		delete m_pDBTBookmarks;

	if (m_pCachedSearchFilter)
		delete m_pCachedSearchFilter;

	if ((DMSOrchestrator^)dmsDocOrchestrator != nullptr)
	{
		delete dmsDocOrchestrator;
		dmsDocOrchestrator = nullptr;
	}
}

//-----------------------------------------------------------------------------
BOOL CDDMS::OnToolbarDropDown(UINT nID, CMenu& menu)
{
	/*if (nID == ID_OPEN_DMS && GetServerDoc()->ValidCurrentRecord())
	{
		menu.AppendMenu(MF_STRING, ID_RUN_CREATEPAPERY_REPORT, (LPTSTR)(LPCTSTR)_TB("Run papery barcode report") );
		return TRUE;
	}*/
	return FALSE;
}

#define TBDMS_COMMAND_PREFIX _T("ToolbarButton.Extensions.TbDBS.TbDBS.Attachment.")

//-----------------------------------------------------------------------------
CString StdNamespace(const CString& aName)
{
	return CString(TBDMS_COMMAND_PREFIX) + aName;
}

//----------------------------------------------------------------------------
void CDDMS::Customize()
{
	if (m_pServerDocument && !m_pServerDocument->IsInUnattendedMode())
	{
		// aggiungo il DockPane degli Attachments alla frame del documento chiamante
		m_pAttachmentPane = new CAttachmentPane(this);
		
		CMasterFrame* pFrame = dynamic_cast<CMasterFrame*>(m_pServerDocument->GetFrame());
		if (pFrame && (CAttachmentPane*)pFrame->CreateDockingPane(m_pAttachmentPane, IDD_ATTACHMENT, _T("AttachmentPane"), _TB("Attachments"), CBRS_ALIGN_RIGHT, CSize(500, 800)))
			m_pAttachmentPane->SetAutoHideMode(TRUE, CBRS_ALIGN_RIGHT | CBRS_HIDE_INPLACE); // per non aprire in automatico il pane

		if (GetServerDoc()->IsABatchDocument())
			LoadAttachments();
	}
}

//-----------------------------------------------------------------------------
void CDDMS::LoadAttachments()
{
	if (m_pAttachments)
		m_pAttachments->RemoveAll();

	// se il barcode e' abilitato carico tutti gli allegati, altrimenti escludo i papery
	List<AttachmentInfo^>^ attachmentList = dmsDocOrchestrator->GetAttachments
		(
			AfxGetTbRepositoryManager()->GetDMSSettings()->GetSettings()->l_EnableBarcode ? AttachmentFilterType::Both : AttachmentFilterType::OnlyAttachment
		);

	for each (AttachmentInfo^ attachment in attachmentList)
	{
		DMSAttachmentInfo* attInfo = CreateDMSAttachmentInfo(attachment, FALSE);
		if (attInfo)
			m_pAttachments->Add(attInfo);
	}

	m_nCurrAttachment = m_pAttachments && m_pAttachments->GetSize() > 0 && m_pAttachments->GetAt(m_pAttachments->GetUpperBound()) ? m_pAttachments->GetUpperBound() : -1;
	
	UpdateAttachmentsCounter();

	if (m_pAttachmentPane)
		m_pAttachmentPane->OnAfterLoadAttachments();
}

//Gestione attach
//-----------------------------------------------------------------------------
void CDDMS::OnAttachCompleted(DMSAttachmentInfo* attachmentInfo)
{
	if (m_pAttachments)
	{
		DMSAttachmentInfo* newAttachInfo = attachmentInfo;
		int nNewAttach = m_pAttachments->GetAttachmentIdx(attachmentInfo);
		if (nNewAttach == -1)
			nNewAttach = m_pAttachments->Add(newAttachInfo);
		else
		{
			newAttachInfo = m_pAttachments->GetAt(nNewAttach);
			delete attachmentInfo; //vuoi dire che sto agganciando un allegato già presente per cui non considero quello che mi viene passato dal DMSOrchestrator
		}

		m_nCurrAttachment = nNewAttach;

		UpdateAttachmentsCounter();
	}
}

//-----------------------------------------------------------------------------
void CDDMS::UpdateAttachmentsCounter()
{
	int count = dmsDocOrchestrator->GetAttachmentsCount(AttachmentFilterType::Both);
	m_sAttachmentsCounter = cwsprintf(_TB("Nr. elements: %d / %d"), m_pAttachments->GetSize(), count);
}

//-----------------------------------------------------------------------------
::ArchiveResult CDDMS::AttachFiles(CStringArray* pFilesToArchvie)
{
	if (!pFilesToArchvie || pFilesToArchvie->GetSize() <= 0)
		return TerminatedSuccess;

	int nAttachmentID;
	::ArchiveResult archiveResult = TerminatedSuccess;

	for (int i = 0; i < pFilesToArchvie->GetSize(); i++)
	{
		if (AttachFile(nAttachmentID, pFilesToArchvie->GetAt(i), _T(""), false) == TerminatedWithError)
			archiveResult = TerminatedWithError;
	}
			
	SetCurrentAttachment();

	if (m_pAttachmentPane)
		m_pAttachmentPane->OnNewAttachCompleted();
	
	return archiveResult;
}

//-----------------------------------------------------------------------------
::ArchiveResult CDDMS::AttachFile(int& nAttachmentId, const CString& strFileName, const CString& description, bool report /*=false*/, bool skipDetectBarcode /*=false*/, const CString& barcode /*=_T("")*/)
{
	::ArchiveResult archResult = ::ArchiveResult::Cancel;
	//mi viene chiamato dal woorm
	if (report)	
	{
		archResult = (::ArchiveResult)dmsDocOrchestrator->AttachReport(nAttachmentId, gcnew String(strFileName), gcnew String(description), gcnew String(barcode));

		SetCurrentAttachment();

		if (m_pAttachmentPane)
			m_pAttachmentPane->OnNewAttachCompleted();
	}
	else
		archResult = (::ArchiveResult)dmsDocOrchestrator->AttachFile(nAttachmentId, gcnew String(strFileName), gcnew String(description));
	
	return archResult;
}

//-----------------------------------------------------------------------------
::ArchiveResult CDDMS::AttachArchivedDocument(int nArchivedDocId, int& nAttachmentId)
{
	return (::ArchiveResult)(int)dmsDocOrchestrator->AttachArchivedDocument(nArchivedDocId, nAttachmentId);
}

//---------------------------------------------------------------------------------------------------------------
::ArchiveResult CDDMS::AttachPapery(const CString& strBarcode, const CString& strDescription, const CString& strFileName)
{
	::ArchiveResult archiveResult = (::ArchiveResult)dmsDocOrchestrator->AttachPapery(gcnew String(strBarcode), gcnew String(strDescription), gcnew String(strFileName));

	SetCurrentAttachment();

	if (m_pAttachmentPane)
		m_pAttachmentPane->OnNewAttachCompleted();

	return archiveResult;
}

//---------------------------------------------------------------------------------------------------------------
::ArchiveResult CDDMS::AttachBinaryContent(const DataBlob & binaryContent, const CString & strFileName, const CString & strDescription, int & nAttachmentID)
{
	DataSize pDataSize;
	System::IntPtr intPointer = (System::IntPtr)((void*)binaryContent.GetRawData(&pDataSize));

	::ArchiveResult archiveResult = (::ArchiveResult)dmsDocOrchestrator->AttachBinaryContent(intPointer, (int)pDataSize, gcnew String(strFileName), gcnew String(strDescription), nAttachmentID);
	
	SetCurrentAttachment();

	if (m_pAttachmentPane)
		m_pAttachmentPane->OnNewAttachCompleted();

	return archiveResult;
}

//---------------------------------------------------------------------------------------------------------------
::ArchiveResult CDDMS::AttachFromTable()
{
	::ArchiveResult archiveResult = (::ArchiveResult)dmsDocOrchestrator->AttachFromTable();
	GetServerDoc()->OnAfterAttachProcess();

	return archiveResult;
}

//-----------------------------------------------------------------------------
bool CDDMS::CreateNewSosDocument(int nAttachmentId)
{
	return dmsDocOrchestrator->CreateNewSosDocument(nAttachmentId);
}

//-----------------------------------------------------------------------------
bool CDDMS::IsDocumentNamespaceInSOS()
{
	return dmsDocOrchestrator->DocumentNamespaceInSos;
}

//-----------------------------------------------------------------------------
bool CDDMS::IsDocumentInSOS()
{
	return m_SOSDocumentStatus != 0 && m_SOSDocumentStatus != 1 && m_SOSDocumentStatus != 5; //vedi stati StatoDocumento in SOSProxyWrapper (m_SOSDocumentStatus ! IDLE && m_SOSDocumentStatus != TORESEND)
}

//---------------------------------------------------------------------------------------------------------------
void CDDMS::OpenAttachmentsListForm(CUIntArray * pSelectedAttachments, bool onlyForMail)
{
	List<int>^  attachmentList = gcnew List<int>();
	dmsDocOrchestrator->OpenAttachmentsListForm(attachmentList, onlyForMail);
	if (pSelectedAttachments)
		pSelectedAttachments->RemoveAll();
	else
		pSelectedAttachments = new CUIntArray();

	for each (int attachemntID in attachmentList)
		pSelectedAttachments->Add(attachemntID);
}

// messaggio inviato dal RepositoryBrowser sul click del pulsante Attach
//---------------------------------------------------------------------------------------------------------------
void CDDMS::OnRepositoryBrowserAttachCompleted()
{
	if (!m_pRepositoryBrowser || !m_pRepositoryBrowser->m_pArchivedDocIdsArray)
		return;

	int nAttachmentId;
	int nArchivedDocId;
	for (int i = 0; i < m_pRepositoryBrowser->m_pArchivedDocIdsArray->GetCount(); i++)
	{
		nArchivedDocId = m_pRepositoryBrowser->m_pArchivedDocIdsArray->GetAt(i);
		AttachArchivedDocument(nArchivedDocId, nAttachmentId);
	}

	m_pRepositoryBrowser->m_pArchivedDocIdsArray->RemoveAll();
	SAFE_DELETE(m_pRepositoryBrowser->m_pArchivedDocIdsArray);

	SetCurrentAttachment();
	if (m_pAttachmentPane)
		m_pAttachmentPane->OnNewAttachCompleted();
}

//--------------------------------------------------------------------
// Gestione singolo allegato
//----------------------------------------------------------------------
//-----------------------------------------------------------------------------
void CDDMS::SetCurrentAttachment()
{
	if (m_pCurrAttachmentInfo && (!m_pAttachments || (m_nCurrAttachment <  0 || m_nCurrAttachment > m_pAttachments->GetUpperBound())))
	{
		m_pCurrAttachmentInfo->Clear();
		m_pDBTBookmarks->RemoveAll();
	}
	else
	{
		DMSAttachmentInfo* pTempAtt = m_pAttachments->GetAt(m_nCurrAttachment);
		if (!pTempAtt->m_IsAPapery)
		{
			Microarea::EasyAttachment::Components::AttachmentInfo^ attachment = dmsDocOrchestrator->GetCompletedAttachmentInfoFromAttachmentId(pTempAtt->m_attachmentID);
			if (attachment == nullptr)
				return;
			DMSAttachmentInfo* attInfo = CreateDMSAttachmentInfo(attachment);
			*m_pCurrAttachmentInfo = *attInfo;
			delete attInfo;
		}
		else
			*m_pCurrAttachmentInfo = *pTempAtt;

		if (!m_pCurrAttachmentInfo->m_IsAPapery && m_pCurrAttachmentInfo->m_TemporaryPathFile.IsEmpty())
			m_pCurrAttachmentInfo->m_TemporaryPathFile = dmsDocOrchestrator->GetAttachmentTempFile(m_pCurrAttachmentInfo->m_attachmentID);

		//carico i bookmark nel m_pDBTBookmarks
		m_pDBTBookmarks->LoadFromBookmarkDT(m_pCurrAttachmentInfo);
	}
}

//-----------------------------------------------------------------------------
BOOL CDDMS::SaveCurrentAttachment()
{
	//per prima cosa devo portare le modifiche fatte sul DBTBookmarks
	//riporto le modifiche fatte sul DBT 
	m_pCurrAttachmentInfo->ModifyBookmarks(m_pDBTBookmarks);

	//il metodo UpdateAttachment controllo se è necessario o meno effettuare le modifiche andando a confrontare con lo stato precedente 
	if (dmsDocOrchestrator->UpdateAttachment
		(
		(AttachmentInfo^)m_pCurrAttachmentInfo->attachmentInfo,
		gcnew String(m_pCurrAttachmentInfo->m_Description.Str()),
		gcnew String(m_pCurrAttachmentInfo->m_FreeTag.Str()),
		gcnew String(m_pCurrAttachmentInfo->m_BarcodeValue.Str()),
		((BOOL)m_pCurrAttachmentInfo->m_IsMainDoc == TRUE),
		((BOOL)m_pCurrAttachmentInfo->m_IsForMail == TRUE)
		))
	{
		m_bEditMode = FALSE;
		if (m_pCurrAttachmentInfo->m_IsAPapery)
		{
			//aggiorno il campo m_Description (l'unico che l'utente può modificare nel caso di Papery)
			DMSAttachmentInfo* pTempAtt = m_pAttachments->GetAt(m_nCurrAttachment);
			pTempAtt->m_Description = m_pCurrAttachmentInfo->m_Description;
		}
		return TRUE;
	}

	return FALSE;
}

//-----------------------------------------------------------------------------
BOOL CDDMS::DeleteCurrentAttachment()
{
	if (!IsCurrAttachmentValid())
		return FALSE;

	int delAttachID = m_nCurrAttachment;

	bool bResult = (m_pCurrAttachmentInfo->m_IsAPapery)
		? dmsDocOrchestrator->DeletePapery(gcnew String(m_pCurrAttachmentInfo->m_BarcodeValue.Str()))
		: DeleteAttachment(m_pCurrAttachmentInfo->m_attachmentID, m_pCurrAttachmentInfo->m_ArchivedDocId);

	if (bResult)
	{
		m_pAttachments->RemoveAt(delAttachID);

		int nextAttachID = -1;
		int size = m_pAttachments->GetSize();
		if (size > 1)
			nextAttachID = (delAttachID - 1 > 0) ? delAttachID - 1 : delAttachID + 1;
		else
			if (size == 1)
				nextAttachID = 0;

		m_nCurrAttachment = nextAttachID;
		SetCurrentAttachment();

		if (m_pAttachmentPane)
			m_pAttachmentPane->OnNewAttachCompleted();
	}

	return bResult;
}

//-----------------------------------------------------------------------------
void CDDMS::SendCurrentAttachment()
{
	if (!IsCurrAttachmentValid())
		return;

	CStringArray arAttachementsFiles;
	CStringArray arAttachmentsTitles;

	arAttachementsFiles.Add(m_pCurrAttachmentInfo->m_TemporaryPathFile);
	arAttachmentsTitles.Add(m_pCurrAttachmentInfo->m_Name);
	CMessages messages;
	if (!AfxGetIMailConnector()->SendAsAttachments(arAttachementsFiles, arAttachmentsTitles, &messages))
		messages.Show();
}

//-----------------------------------------------------------------------------
void CDDMS::ViewCurrentAttachment()
{
	if (!IsCurrAttachmentValid() || m_pCurrAttachmentInfo->m_IsAPapery)
		return;

	Process::Start(gcnew String(m_pCurrAttachmentInfo->m_TemporaryPathFile.Str()));
}

//-----------------------------------------------------------------------------
void CDDMS::CheckOutCurrentAttachment()
{
	if (!m_pCurrAttachmentInfo)
		return;

	AttachmentInfo^ aInfo = m_pCurrAttachmentInfo->attachmentInfo;
	//il metodo UpdateAttachment controllo se è necessario o meno effettuare le modifiche andando a confrontare con lo stato precedente 
	if (dmsDocOrchestrator->CheckOut(aInfo))
		ViewCurrentAttachment();
}

//-----------------------------------------------------------------------------
void CDDMS::CheckInCurrentAttachment()
{
	if (!m_pCurrAttachmentInfo)
		return;

	AttachmentInfo^ aInfo = m_pCurrAttachmentInfo->attachmentInfo;
	//il metodo UpdateAttachment controllo se è necessario o meno effettuare le modifiche andando a confrontare con lo stato precedente 
	dmsDocOrchestrator->CheckIn(aInfo);
}

//-----------------------------------------------------------------------------
void CDDMS::OnUndoCheckOutAttachment()
{
	if (!m_pCurrAttachmentInfo)
		return;

	AttachmentInfo^ aInfo = m_pCurrAttachmentInfo->attachmentInfo;
	//il metodo UpdateAttachment controllo se è necessario o meno effettuare le modifiche andando a confrontare con lo stato precedente 
	dmsDocOrchestrator->Undo(aInfo);
}

//-----------------------------------------------------------------------------
void CDDMS::UndoChangesAttachment()
{
	m_bEditMode = FALSE;
	SetCurrentAttachment();
}

//-----------------------------------------------------------------------------
BOOL CDDMS::IsCurrAttachmentValid()
{
	return m_pCurrAttachmentInfo && m_pCurrAttachmentInfo->IsValid();
}

//Gestione stato del documento
//-----------------------------------------------------------------------------
 void CDDMS::OnAfterSetFormMode(CBaseDocument::FormMode oldFormMode)
 {
	 dmsDocOrchestrator->AfterSetFormMode((FormModeType)((int)oldFormMode));

	 if (dmsDocOrchestrator->DocumentNamespaceInSos && GetServerDoc()->GetFormMode() == CBaseDocument::FIND)
	 {
		 m_SOSDocumentStatus = 0; //se sono in stato di find devo azzerare lo stato
		 m_SOSDocumentStatusToolTip.Empty();
	 }

	 if (m_pServerDocument->GetFormMode() == CAbstractFormDoc::NEW ||
		 m_pServerDocument->GetFormMode() == CAbstractFormDoc::FIND)
	 {
		 if (m_bEditMode)
		 {
			 if (!GetServerDoc()->IsInUnattendedMode() && AfxMessageBox(_TB("The current attachment is in edit mode. Would you like to save it?"), MB_OKCANCEL) == IDOK)
				 SaveCurrentAttachment();
			 else		 
				 UndoChangesAttachment();
		}

		 if (m_pAttachments)
			 m_pAttachments->RemoveAll();

		 m_nCurrAttachment = -1;		

		 if (m_pAttachmentPane)
		 {
			 m_pAttachmentPane->OnAfterLoadAttachments();
			 m_pAttachmentPane->SetAutoHideMode(TRUE, CBRS_RIGHT | CBRS_HIDE_INPLACE);
		 }
	 }
 }
	
 //-----------------------------------------------------------------------------
 void CDDMS::OnGoInBrowseMode()
 {
	 EndObserving();
 }

//-----------------------------------------------------------------------------	
BOOL CDDMS::OnBeforeEscape() 
{ 
	EndObserving();
	return TRUE;
}

//-----------------------------------------------------------------------------	
BOOL CDDMS::SaveModified()
{
	if (m_bEditMode)
	{
		if (!GetServerDoc()->IsInUnattendedMode() && AfxMessageBox(_TB("The current attachment is in edit mode. Would you like to save it?"), MB_OKCANCEL) == IDOK)
			SaveCurrentAttachment();
		else
			UndoChangesAttachment();
	}

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CDDMS::OnOkNewRecord()
{
	dmsDocOrchestrator->AfterNewErpDocument();
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CDDMS::OnOkEdit()
{
	StartObserving();
	return TRUE;
}

//-----------------------------------------------------------------------------
void CDDMS::OnSaveCurrentRecord()
{
	if (GetServerDoc()->GetFormMode() != CAbstractFormDoc::EDIT && GetServerDoc()->GetFormMode() != CAbstractFormDoc::NEW)
	{
		if (m_bEditMode)
		{
			if (!GetServerDoc()->IsInUnattendedMode() && AfxMessageBox(_TB("The current attachment is in edit mode. Would you like to save it?"), MB_OKCANCEL) == IDOK)
				SaveCurrentAttachment();
			else
				UndoChangesAttachment();
		}

		dmsDocOrchestrator->AfterBrowseErpDocument();
		AfxGetTbRepositoryManager()->SetAttachmentPanelOptions(m_nMaxDocNrToShow);
		LoadAttachments();
	}
}

//-----------------------------------------------------------------------------
BOOL CDDMS::OnPrepareAuxData()
{
	if (dmsDocOrchestrator->DocumentNamespaceInSos)
	{
		m_SOSDocumentStatus = 0; 
		m_SOSAttachmentID = -1;
		m_SOSDocumentStatusToolTip.Empty();
		String^ sosStatusMsg = gcnew String("");
		if (GetServerDoc()->ValidCurrentRecord())
		{
			dmsDocOrchestrator->GetDocumentSOSInfo(m_SOSDocumentStatus, m_SOSAttachmentID, sosStatusMsg);
			m_SOSDocumentStatusToolTip = sosStatusMsg;
		}
	}

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CDDMS::OnExtraEditTransaction()
{
	EndObserving();
	return TRUE;
}

//-----------------------------------------------------------------------------
void CDDMS::OnAfterDelete()
{
	if (!GetServerDoc()->ValidCurrentRecord())
		dmsDocOrchestrator->AfterBrowseErpDocument();	//serve nel caso in cui stiamo cancellando l'unico record presente nel dataentry
}

//-----------------------------------------------------------------------------
BOOL CDDMS::OnExtraDeleteTransaction()
{
	dmsDocOrchestrator->AfterDeleteErpDocument();
	return TRUE;
}

//Gestione observable
// I observe the dataobj that are bookmark of this erp document attachments, i order to save in the table
//DMS_IndexesSyncronization the changed values. The the webservice EasyAttachmentSyncService syncronizes the bookmarks search value
// considering the changed values

//-----------------------------------------------------------------------------
void CDDMS::OnUpdateCollectionCompleted()
{
	if (GetServerDoc()->GetFormMode() != CBaseDocument::EDIT)
		return;

	EndObserving();
	StartObserving();
}

//-----------------------------------------------------------------------------
void CDDMS::StartObserving()
{
 	if (GetServerDoc()->GetFormMode() != CBaseDocument::EDIT || m_bObserving)
		return;

	if (!m_pObserverContext)
		m_pObserverContext = new CObserverContext();
	
	m_arEventToDetach.RemoveAll();
	dmsDocOrchestrator->StartBookmarkObserving();

	BookmarkToObserve^ bookmarkToObserve = nullptr;
	for (int i = 0; i < dmsDocOrchestrator->BookmarksToObserve->Count; i++)
	{
		bookmarkToObserve = dmsDocOrchestrator->BookmarksToObserve[i];
		if (bookmarkToObserve != nullptr)
		{
			MDataObj^ dummy = bookmarkToObserve->DataObj;
			DataObj* dataObj = (DataObj*)dummy->DataObjPtr.ToInt64();
			if (dataObj)
			{				
				CBookmarkFieldEvents* bookmarkEvent = new CBookmarkFieldEvents(m_pObserverContext, bookmarkToObserve);
				dataObj->AttachEvents(bookmarkEvent);	
				m_arEventToDetach.Add(bookmarkEvent);
			}
		}
	}

	m_bObserving = true;	
	m_pObserverContext->StartObserving(ON_CHANGING);				
}

//-----------------------------------------------------------------------------
void CDDMS::EndObserving()
{
	if (!m_pObserverContext || dmsDocOrchestrator->BookmarksToObserve->Count <= 0 || !m_bObserving)
	{
		if (m_pObserverContext && m_bObserving)
			m_pObserverContext->EndObserving(ON_CHANGING);
		m_bObserving = false;
		return;
	}
			
	m_bObserving = false;
	ObservableMap* pObservableMap = m_pObserverContext->GetObservableMap(ON_CHANGING);

	if (!pObservableMap)
		return;

	POSITION pos = pObservableMap->GetStartPosition();
	CObservable* pObservable;
	CBookmarkFieldEvents* pDataEvents = NULL;
	CDataEventsObj* pDataEventsObj = NULL;
	while (pos)
	{
		pObservableMap->GetNextAssoc(pos, pObservable, pDataEventsObj);
		if (pDataEventsObj)
		{
			pDataEvents = (CBookmarkFieldEvents*)pDataEventsObj;
			if (pDataEvents->BookmarkToObserve)
				pDataEvents->BookmarkToObserve->Changed = true;			
		}
	}

	//faccio il detach degli eventi sui dataobj
	for (int i =0; i <= m_arEventToDetach.GetUpperBound(); i++)
	{
		CBookmarkFieldEvents* bookmarkEvent = (CBookmarkFieldEvents*)m_arEventToDetach.GetAt(i);
		if (bookmarkEvent && bookmarkEvent->BookmarkToObserve)
		{
			MDataObj^ dummy = bookmarkEvent->BookmarkToObserve->DataObj;
			DataObj* dataObj = (DataObj*)dummy->DataObjPtr.ToInt64();
			if (dataObj)
				dataObj->DetachEvents(bookmarkEvent);
		}
	}
	m_arEventToDetach.RemoveAll();
	
	if (pObservableMap->GetSize() > 0)
	{
		dmsDocOrchestrator->EndBookmarkObserving();
		if (m_pCurrAttachmentInfo && (AttachmentInfo^)m_pCurrAttachmentInfo->attachmentInfo != nullptr)
		{
			dmsDocOrchestrator->ReloadBookmarksValues((AttachmentInfo^)m_pCurrAttachmentInfo->attachmentInfo);
			m_pDBTBookmarks->LoadFromBookmarkDT(m_pCurrAttachmentInfo);
		}

		if (m_pAttachmentPane)
			m_pAttachmentPane->OnUpdateControls();
	}

	m_pObserverContext->EndObserving(ON_CHANGING);		
}

//-----------------------------------------------------------------------------
void CDDMS::OnSyncronizationIndexesFinished()
{
}

//-----------------------------------------------------------------------------
bool CDDMS::DeleteAttachment(int nAttachmentId, int nArchivedDocId)
{
	return dmsDocOrchestrator->DeleteAttachment(nAttachmentId, nArchivedDocId);
}

//Filtra i comandi non consentiti quando si e' in esecuzione da MagoWeb
//-----------------------------------------------------------------------------
WebCommandType CDDMS::OnGetWebCommandType(UINT commandID)
{
	return __super::OnGetWebCommandType(commandID);
}

// creo uno o piu' attachment selezionando i documenti da filesystem
//---------------------------------------------------------------------------------------------------------------
void CDDMS::OnNewAttachmentFromFileSystem()
{
	if (!m_pAttachmentPane)
		return;

	TCHAR szBuff[8128];
	szBuff[0] = '\0';

	CFileDialog fileDialog(TRUE, NULL, NULL, OFN_ALLOWMULTISELECT, NULL, m_pAttachmentPane);
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

	AttachFiles(pArray);
	SAFE_DELETE(pArray);
}

// apro il RepositoryBrowser per selezionare i documenti archiviati da allegare
//---------------------------------------------------------------------------------------------------------------
void CDDMS::OnNewAttachmentFromRepository()
{
	m_pRepositoryBrowser = (BDDMSRepositoryBrowser*)AfxGetTbCmdManager()->RunDocument(_NS_DOC("Extensions.EasyAttachment.TbDMS.DMSRepositoryBrowser"), szDefaultViewMode, NULL, NULL, m_pServerDocument);
}

// creo uno o piu' attachment caricandoli da un device esterno
//---------------------------------------------------------------------------------------------------------------
void CDDMS::OnNewAttachmentFromDevice()
{
	if (!m_pAttachmentPane)
		return;
	
	m_pAcquisitionFromDevice =
		(BDAcquisitionFromDevice*)AfxGetTbCmdManager()->RunDocument(_NS_DOC("Extensions.EasyAttachment.TbDMS.AcquisitionFromDevice"), szDefaultViewMode, NULL, NULL, m_pServerDocument);

	m_pServerDocument->GetMasterFrame()->EnableWindow(FALSE);
	AfxGetTbCmdManager()->WaitDocumentEnd(m_pAcquisitionFromDevice);
	m_pServerDocument->GetMasterFrame()->EnableWindow(TRUE);
}

//-----------------------------------------------------------------------------	
void CDDMS::OnScanProcessEnded()
{
	if (m_pAcquisitionFromDevice)
		AttachFiles(m_pAcquisitionFromDevice->m_pAcquiredFiles);
}

// creo un papery
//---------------------------------------------------------------------------------------------------------------
void CDDMS::OnNewAttachmentFromPapery()
{
	if (!m_pAttachmentPane)
		return;

	//CreateSlaveView(IDD_PAPERY, NULL, 1);
	
	CPaperyParsedDlg dlg(m_pAttachmentPane, this);
	if (dlg.DoModal() == IDOK)
		AttachPapery(m_sPaperyBarcodeValue, m_sPaperyNotes, _T(""));
}

//-----------------------------------------------------------------------------
void CDDMS::RunCreatePaperyReport()
{
	/*if (AfxMessageBox(_TB(""), MB_OKCANCEL | MB_ICONQUESTION) == MB_OKCANCEL)
	return;*/
	if (m_pReportDoc)
	{
		m_pReportDoc->Activate();
		return;
	}
	CWoormInfo* pWoormInfo = new CWoormInfo(DataStr(_T("report.extensions.easyattachment.EA_DocBarcodeLabelsRep")));
	pWoormInfo->m_bOwnedByReport = TRUE;

	//I get the description of the document used the ShowAsDescription fields presents in SeachBookmark fields in dbts.xml  file
	if (!m_pShowAsDescriptionFields)
	{
		if (GetServerDoc() && GetServerDoc()->m_pDBTMaster && GetServerDoc()->CanLoadXMLDescription())
		{
			GetServerDoc()->LoadXMLDescription();
			CXMLDBTInfo* pXMLDBTInfo = GetServerDoc()->m_pDBTMaster->GetXMLDBTInfo();
			if (pXMLDBTInfo && pXMLDBTInfo->m_pXMLSearchBookmarkArray)
				m_pShowAsDescriptionFields = pXMLDBTInfo->m_pXMLSearchBookmarkArray->GetShowAsDescriptionFields();
		}
	}
	CString docDescription = (m_pShowAsDescriptionFields) ? GetServerDoc()->m_pDBTMaster->GetRecord()->GetFieldsValueDescription(m_pShowAsDescriptionFields) : GetServerDoc()->m_pDBTMaster->GetRecord()->GetPrimaryKeyDescription();

	pWoormInfo->AddParam(_NS_WRMVAR("w_DocumentTitle"), &DataStr(GetServerDoc()->GetTitle()));
	pWoormInfo->AddParam(_NS_WRMVAR("w_DocumentDescription"), &DataStr(docDescription));

	m_pReportDoc = AfxGetTbCmdManager()->RunWoormReport(pWoormInfo, m_pServerDocument);
}

//-----------------------------------------------------------------------------
void CDDMS::OnRunCreatePaperyReport()
{
	RunCreatePaperyReport();
}

// esegue il detect del barcode nell'AttachmentInfo corrente
//-----------------------------------------------------------------------------
void CDDMS::DoDetectBarcodeForCurrentAttachInfo()
{
	Microarea::EasyAttachment::BusinessLogic::TypedBarcode^ tb = dmsDocOrchestrator->AttachManager->DetectBarcode(m_pCurrAttachmentInfo->attachmentInfo);
	CTypedBarcode ctb(tb->Value, tb->TypeDescription);
	m_pCurrAttachmentInfo->m_BarcodeValue = ctb.m_strBarcodeValue;
	m_pCurrAttachmentInfo->m_BarcodeType = ctb.m_strBarcodeType;
}

// utilizzato per la visualizzazione delle info della SOS nell'apposita tile nel pannello Allegati
//-----------------------------------------------------------------------------
::CanBeSentToSOSType CDDMS::CanBeSentToSOS(CString& strMsg)
{
	String^ outMsg = gcnew String("");
	Microarea::EasyAttachment::BusinessLogic::CanBeSentToSOSType canBeSent = dmsDocOrchestrator->SosManager->CanBeSentToSOS((AttachmentInfo^)m_pCurrAttachmentInfo->attachmentInfo, outMsg);
	strMsg = outMsg;
	return (::CanBeSentToSOSType)canBeSent;
}

// per creare un SOSDocument con l'apposito button nella tile della SOS nel pannello Allegati
//-----------------------------------------------------------------------------
BOOL CDDMS::CreateNewSosDocument()
{
	return dmsDocOrchestrator->SosManager->CreateNewSosDocument((AttachmentInfo^)m_pCurrAttachmentInfo->attachmentInfo);
}

//=============================================================================
//			Class CBookmarkFieldEvents Declaration
//=============================================================================
IMPLEMENT_DYNAMIC(CBookmarkFieldEvents, CObject)

//-----------------------------------------------------------------------------
CBookmarkFieldEvents::CBookmarkFieldEvents(CObserverContext* pContext, gcroot<Microarea::EasyAttachment::Components::BookmarkToObserve^> bookmark)
	:
	m_pObsContext	(pContext)
	{
		m_bOwned = true;
		BookmarkToObserve = bookmark;
	}
