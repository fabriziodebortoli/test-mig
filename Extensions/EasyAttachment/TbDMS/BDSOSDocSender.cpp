#include "stdafx.h"

#include <ExtensionsImages\CommonImages.h>

#include "TBRepositoryManager.h"
#include "BDSOSDocSender.h"
#include "UISOSDocSender.h"

#include "EasyAttachment\JsonForms\UISOSDocSender\IDD_SOSDOCSENDER_WIZARD.hjson"

using namespace System;
using namespace System::Data;
using namespace System::Collections::Specialized;
using namespace System::Collections::Generic; 
using namespace Microarea::EasyAttachment;
using namespace Microarea::EasyAttachment::BusinessLogic;
using namespace Microarea::EasyAttachment::Components;
using namespace Microarea::TaskBuilderNet::Core::WebServicesWrapper;
using namespace Microarea::TaskBuilderNet::Interfaces;

//===========================================================================
//							DMSSOSDocSenderEvents
// ref class that manages communication between Event Handlers and C++ CWnd
//===========================================================================
void DMSSOSDocSenderEvents::InitializeEvents(BDSOSDocSender* pDoc, SOSManager^ sosManager)
{
	m_pDoc = pDoc;
	if (sosManager)
	{
		sosManager->SOSOperationCompleted += gcnew EventHandler<SOSEventArgs^>(this, &DMSSOSDocSenderEvents::OnSOSOperationCompleted);
		sosManager->SOSSendFinished += gcnew EventHandler(this, &DMSSOSDocSenderEvents::OnSOSOperationFinished);
	}
}

// evento intercettato in esecuzione delle singole operazioni di invio
//-----------------------------------------------------------------------------
void DMSSOSDocSenderEvents::OnSOSOperationCompleted(System::Object^, Microarea::EasyAttachment::Components::SOSEventArgs^ eventArg)
{
	if (m_pDoc)
	{
		// sono su un thread separato! 
		AfxInvokeThreadProcedure<BDSOSDocSender, CSOSEventArgs*>(m_pDoc->GetFrameHandle(), m_pDoc, &BDSOSDocSender::OnSOSOperationCompleted, CreateSOSEventArgs(eventArg));
	}
}

// evento intercettato al termine dell'elaborazione
//-----------------------------------------------------------------------------
void DMSSOSDocSenderEvents::OnSOSOperationFinished(System::Object^, EventArgs^ eventArg)
{
	if (m_pDoc)
	{
		// sono su un thread separato! 
		AfxInvokeThreadProcedure<BDSOSDocSender>(m_pDoc->GetFrameHandle(), m_pDoc, &BDSOSDocSender::OnSOSOperationFinished);
	}
}

/////////////////////////////////////////////////////////////////////////////
//				class BDSOSDocSender Implementation
/////////////////////////////////////////////////////////////////////////////
//----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(BDSOSDocSender, CWizardFormDoc)

//----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(BDSOSDocSender, CWizardFormDoc)
	////{{AFX_MSG_MAP(BDSOSDocSender)
	ON_EN_VALUE_CHANGED	(IDC_SOSDOCSENDER_DOCCLASS_COMBO,	OnDocClassChanged)
	ON_EN_VALUE_CHANGED	(IDC_SOSDOCSENDER_DOCTYPE_COMBO,	OnDocTypeChanged)
	ON_EN_VALUE_CHANGED	(IDC_SOSDOCSENDER_TAXJOURNAL_COMBO,	OnTaxJournalChanged)
	ON_EN_VALUE_CHANGED	(IDC_SOSDOCSENDER_FISCALYEAR_COMBO,	OnFiscalYearChanged)
	ON_EN_VALUE_CHANGED	(IDC_SOSDOCSENDER_STATUS_IDLE,		OnStatusIdleChanged)
	ON_EN_VALUE_CHANGED	(IDC_SOSDOCSENDER_STATUS_TORESEND,	OnStatusToResendChanged)
	
	ON_COMMAND			(ID_SOSDOCSENDER_BTN_SELDESEL,		OnSelDeselClicked)
	////}}AFX_MSG_MAP
END_MESSAGE_MAP()

//----------------------------------------------------------------------------
BDSOSDocSender::BDSOSDocSender()
	:
	dmsOrchestrator				(nullptr),
	dmsSOSDocSenderEvents		(nullptr),
	m_pDBTSOSDocuments			(NULL),
	m_pDBTSOSElaboration		(NULL),
	m_pSOSSearchRules			(NULL),
	m_pSOSDocClassList			(NULL),
	m_pSOSDocTypeList			(NULL),
	m_pSOSTaxJournalList		(NULL),
	m_pSOSFiscalYearList		(NULL),
	m_bOnlyMainDoc				(FALSE),
	m_bDocIdle					(TRUE),
	m_bDocToResend				(TRUE),
	m_bCanShowElaborationDlg	(FALSE),
	m_bCanClose					(TRUE),
	m_pSOSDocumentArray			(NULL),
	m_pGauge					(NULL),
	m_nCurrentElement			(0),
	m_GaugeRange				(100),
	m_Range						(0),
	attachmentsToSendList		(nullptr),
	documentTypesList			(nullptr),
	m_bSendDocToSOS				(TRUE),
	m_bExcludeDocFromSOS		(FALSE),	
	m_bSelectDeselect			(TRUE),
	m_pSOSDocTypeItemSource		(NULL),
	m_pSOSTaxJournalItemSource	(NULL),
	m_pSOSFiscalYearItemSource	(NULL),
	m_pBEBtnSelDesel			(NULL)
{
}

//----------------------------------------------------------------------------
BDSOSDocSender::~BDSOSDocSender()
{
	SAFE_DELETE(m_pSOSSearchRules);
	SAFE_DELETE(m_pDBTSOSDocuments);
	SAFE_DELETE(m_pDBTSOSElaboration);
	SAFE_DELETE(m_pSOSDocClassList);
	SAFE_DELETE(m_pSOSDocTypeList);
	SAFE_DELETE(m_pSOSTaxJournalList);
	SAFE_DELETE(m_pSOSFiscalYearList);
	SAFE_DELETE(m_pSOSDocumentArray);
	
	if ((DMSOrchestrator^)dmsOrchestrator != nullptr)
	{
		delete dmsOrchestrator;
		dmsOrchestrator = nullptr;
	}
}

//-----------------------------------------------------------------------------
BOOL BDSOSDocSender::OnAttachData()
{
	__super::OnAttachData();	// serve per i titoli

	SetFormTitle(_TB("SOS Document sender"));
	SetFormName(_TB("SOS Document sender"));

	dmsOrchestrator = gcnew DMSOrchestrator();
	AfxGetTbRepositoryManager()->InitializeManager(this);

	dmsSOSDocSenderEvents = gcnew DMSSOSDocSenderEvents();
	dmsSOSDocSenderEvents->InitializeEvents(this, dmsOrchestrator->SosManager);

	// carico dal database DMS le classi documentali
	LoadSOSDocClasses();

	m_pDBTSOSDocuments	= new DBTSOSDocuments	(RUNTIME_CLASS(VSOSDocument), this);
	m_pDBTSOSElaboration= new DBTSOSElaboration	(RUNTIME_CLASS(VSOSElaboration), this);
	
	DECLARE_VAR_JSON(bSendDocToSOS);
	DECLARE_VAR_JSON(bExcludeDocFromSOS);
	DECLARE_VAR_JSON(DocumentClass);
	DECLARE_VAR_JSON(DocumentType);
	DECLARE_VAR_JSON(TaxJournal);
	DECLARE_VAR_JSON(FiscalYear);
	DECLARE_VAR_JSON(bOnlyMainDoc);
	DECLARE_VAR_JSON(bDocIdle);
	DECLARE_VAR_JSON(bDocToResend);
	DECLARE_VAR_JSON(ElaborationMessage);
	DECLARE_VAR_JSON(nCurrentElement);

	RegisterControl(IDC_SOSDOCSENDER_BE_RESULTS, RUNTIME_CLASS(CSOSDocSenderResultsBodyEdit));

	InitSelections();

	return TRUE;
}

//----------------------------------------------------------------------------
void BDSOSDocSender::OnParsedControlCreated(CParsedCtrl* pCtrl)
{
	__super::OnParsedControlCreated(pCtrl);

	UINT nIDC = pCtrl->GetCtrlID();

	if (nIDC == IDC_SOSDOCSENDER_DOCCLASS_COMBO)
	{
		CSOSDocClassesItemSource* pDocClassesItemSource = (CSOSDocClassesItemSource*)pCtrl->GetItemSource();
		pDocClassesItemSource->SetDocClassList(m_pSOSDocClassList);
		return;
	}

	if (nIDC == IDC_SOSDOCSENDER_DOCTYPE_COMBO)
	{
		m_pSOSDocTypeItemSource = (CSOSDocTypeItemSource*)pCtrl->GetItemSource();
		return;
	}

	if (nIDC == IDC_SOSDOCSENDER_TAXJOURNAL_COMBO)
	{
		m_pSOSTaxJournalItemSource = (CSOSTaxJournalItemSource*)pCtrl->GetItemSource();
		return;
	}

	if (nIDC == IDC_SOSDOCSENDER_FISCALYEAR_COMBO)
	{
		m_pSOSFiscalYearItemSource = (CSOSFiscalYearItemSource*)pCtrl->GetItemSource();
		return;
	}

	if (nIDC == IDC_SOSDOCSENDER_GAUGE)
	{
		if (!m_pGauge)
			m_pGauge = dynamic_cast<CTBLinearGaugeCtrl*>(pCtrl);

		if (!m_pGauge)
			return;

		// Questi reset del gauge servono a togliere il segnalino di avanzamento e la scala da 1 a 100 ecc.
		m_pGauge->SetMajorTickMarkSize(0);
		m_pGauge->SetMinorTickMarkSize(0);
		m_pGauge->SetMajorTickMarkStep(0);
		m_pGauge->RemovePointer(0);
		m_pGauge->SetBkgColor(AfxGetTileDialogStyleWizard()->GetBackgroundColor());
		m_pGauge->SetTextLabelFormat(_T(""));
		m_pGauge->RemoveAllColoredRanges();
		m_pGauge->AddColoredRange(0, m_nCurrentElement, AfxGetThemeManager()->GetTileDialogTitleForeColor());
	}
}

//-----------------------------------------------------------------------------
void BDSOSDocSender::CustomizeBodyEdit(CBodyEdit* pBodyEdit)
{
	__super::CustomizeBodyEdit(pBodyEdit);

	if (pBodyEdit->GetNamespace().GetObjectName() == _NS_BE("BESOSDocuments"))
	{
		CSOSDocSenderResultsBodyEdit* pResultsBE = (CSOSDocSenderResultsBodyEdit*)pBodyEdit;
		// a regime questo dovrebbe sparire!
		if (pResultsBE)
			m_pBEBtnSelDesel = pResultsBE->m_HeaderToolBar.FindButton(ID_SOSDOCSENDER_BTN_SELDESEL);
		return;
	}
}

//-----------------------------------------------------------------------------
LRESULT BDSOSDocSender::OnWizardNext(UINT nIDD)
{
	if (nIDD == IDD_WTD_SOSDOCSENDER_FILTERS)
	{
		// se le selezioni minime sono mancanti o se non ho estratto documenti non procedo
		if (!ExtractSOSDocuments())
			return WIZARD_SAME_TAB;
	}

	return WIZARD_DEFAULT_TAB;
}

//---------------------------------------------------------------------------
void BDSOSDocSender::OnEnableWizardNext(CCmdUI* pCmdUI)
{
	BOOL bEnable = GetTabWizard()->GetActiveDlg()->GetDlgInfoItem()->GetDialogID() == IDD_WTD_SOSDOCSENDER_FILTERS;
	pCmdUI->Enable(bEnable);
}

//---------------------------------------------------------------------------
void BDSOSDocSender::OnEnableWizardBack(CCmdUI* pCmdUI)
{
	BOOL bEnable = GetTabWizard()->GetActiveDlg()->GetDlgInfoItem()->GetDialogID() != IDD_WTD_SOSDOCSENDER_ELABORATION; 
	pCmdUI->Enable(bEnable);
}

//---------------------------------------------------------------------------
void BDSOSDocSender::OnWizardActivate(UINT nPageIDD)
{
	if (nPageIDD == IDD_WTD_SOSDOCSENDER_FILTERS)
	{
		SetHeaderTitle(_TB(""));
		SetHeaderSubTitle(_TB("First of all set filters in order to extract the documents you want to send or exclude to SOStitutiva."));
		return;
	}

	if (nPageIDD == IDD_WTD_SOSDOCSENDER_DOCSELECTIONS)
	{
		SetHeaderTitle(_TB(""));
		SetHeaderSubTitle(_TB("Here is the list of documents extracted according to the filters set. Please select the documents you want to consider for the elaboration."));

		// imposto questa tabdialog come ultima, cosi da poter far comparire il pulsante Esegui in automatico
		CJsonWizardTabDialog* pWiz = (CJsonWizardTabDialog*)GetTabWizard()->GetActiveDlg();
		if (pWiz)
			pWiz->SetLast();

		return;
	}
}

//---------------------------------------------------------------------------
BOOL BDSOSDocSender::CanDoBatchExecute()
{
	// E' abilitato sulla seconda tab
	BOOL bEnable = GetTabWizard()->GetActiveDlg()->GetDlgInfoItem()->GetDialogID() == IDD_WTD_SOSDOCSENDER_DOCSELECTIONS; 
	return bEnable;
}

//-----------------------------------------------------------------------------
BOOL BDSOSDocSender::CanRunDocument()
{
	if (!AfxGetOleDbMng()->DMSSOSEnable())
	{
		AfxMessageBox(_TB("Impossible to open SOS Document Sender form!\r\nPlease, check in Administration Console if this company uses DMS and if DMS Advanced is activated."));
		return FALSE;
	}

	if (!AfxGetTbRepositoryManager()->GetDMSSettings()->GetSettings()->l_EnableSOS)
	{
		AfxMessageBox(_TB("You are not allowed to open SOS Document Sender form! In order to open it change your setting parameter and enable SOS."));
		return FALSE;
	}

	return TRUE;
}

//-----------------------------------------------------------------------------
void BDSOSDocSender::DisableControlsForBatch()
{
	m_DocumentType.SetReadOnly(m_DocumentType.IsEmpty());
	m_TaxJournal.SetReadOnly(m_TaxJournal.IsEmpty());
	m_FiscalYear.SetReadOnly(m_FiscalYear.IsEmpty());

	UpdateDataView();
}

// eseguo un check preventivo prima di eseguire l'elaborazione vera e propria
//-----------------------------------------------------------------------------
BOOL BDSOSDocSender::CheckFilters()
{
	// eseguo un check preventivo in caso di spedizione via FTP: controllo che il folder abbia i permessi di scrittura
	if (dmsOrchestrator->SosManager->SOSConfigurationState->SOSConfiguration->FTPSend)
	{
		if (!CheckDirectoryAccess(dmsOrchestrator->SosManager->SOSConfigurationState->SOSConfiguration->FTPSharedFolder))
		{
			Message(_TB("The specified folder for FTP does not have the Read/Write permissions. Unable to proceed!"), MB_ICONSTOP);
			return FALSE;
		}
	}

	// pulisco la lista degli allegati da inviare
	attachmentsToSendList->Clear();

	// mi tengo da parte gli AttachmentInfo delle righe selezionate e nel frattempo le conto
	int nSelRowsCount = 0;
	for (int i = 0; i < m_pDBTSOSDocuments->GetSize(); i++)
	{
		VSOSDocument* pRec = (VSOSDocument*)m_pDBTSOSDocuments->GetRow(i);
		if (pRec->l_IsSelected)
		{
			attachmentsToSendList->Add(pRec->attachInfo);
			nSelRowsCount++;
		}
	}

	if (nSelRowsCount == 0)
	{
		Message(_TB("You have to select at least one document! Unable to proceed!"), MB_ICONSTOP);
		return FALSE;
	}

	// se ho selezionato un numero di documenti superiore a quello indicato nei parametri non procedo
	if (nSelRowsCount > dmsOrchestrator->SettingsManager->UsersSettingState->Options->SOSOptionsState->MaxElementsInEnvelope)
	{
		Message
			(
				cwsprintf(_TB("The number of documents you have selected is higher than the value set in your setting parameter. The maximum allowed value is {0-%d}."),
				dmsOrchestrator->SettingsManager->UsersSettingState->Options->SOSOptionsState->MaxElementsInEnvelope), 
				MB_ICONSTOP
			);
		return FALSE;
	}

	if (AfxMessageBox(cwsprintf(_TB("{0-%d} documents selected and ready to send.\r\nDo you want to send to SOS the selected documents?"), nSelRowsCount), MB_YESNO | MB_ICONQUESTION | MB_DEFBUTTON2) == IDNO)
		return FALSE;

	return TRUE;
}

//-----------------------------------------------------------------------------
void BDSOSDocSender::OnBatchExecute()
{
	// prima controllo che i filtri siano corretti
	m_bCanShowElaborationDlg = CheckFilters();

	if (!m_bCanShowElaborationDlg)
		return;

	BeginWaitCursor();
	m_bCanClose = FALSE;
	m_bBatchRunning = TRUE;

	// in questo metodo parte un thread separato di elaborazione lato C#
	if (m_bSendDocToSOS)
		dmsOrchestrator->SosManager->SendAttachmentsToSos(attachmentsToSendList);
	else
		dmsOrchestrator->SosManager->RemoveAttachmentsFromSOS(attachmentsToSendList);

	m_bBatchRunning = FALSE;
	
	UpdateDataView();
}

//----------------------------------------------------------------------------
void BDSOSDocSender::OnBatchCompleted()
{
	__super::OnBatchCompleted();

	if (m_bCanShowElaborationDlg) // mi posiziono sull'ultima pagina solo se le selezioni sono ok
	{
		((CSOSDocSenderWizardFormView*)GetFirstView())->OnWizardNext();
		// faccio partire il timer solo se e' partita l'elaborazione
		StartTimer();
	}
}

//-----------------------------------------------------------------------------
void BDSOSDocSender::OnSOSOperationCompleted(CSOSEventArgs* sosArgs)
{
	VSOSElaboration* pRec;

	int idx = sosArgs->m_nIdx;
	
	if (m_pGauge)
		m_pGauge->UpdateCtrlView();

	// se l'idx e' -1 significa che devo aggiornare lo static generale
	if (sosArgs->m_nIdx == -1)
		m_ElaborationMessage = sosArgs->m_sMessage;
	else
	{
		// altrimenti aggiorno le righe del bodyedit
		int nSize = m_pDBTSOSElaboration->GetSize();
		if (nSize == idx)
			pRec = (VSOSElaboration*)m_pDBTSOSElaboration->AddRecord();
		else
		{
			pRec = (VSOSElaboration*)m_pDBTSOSElaboration->GetRow(sosArgs->m_nIdx);
			if (!pRec) // x pararci se non trovassimo la riga
				pRec = (VSOSElaboration*)m_pDBTSOSElaboration->AddRecord();
		}

		switch (sosArgs->m_MessageType)
		{
		case DiagnosticType::Information:
			pRec->l_MsgBmp = TBGlyph(szGlyphOk);
			break;

		case DiagnosticType::Error:
			pRec->l_MsgBmp = TBGlyph(szIconError);
			break;

		case DiagnosticType::None:
		default:
			pRec->l_MsgBmp = _T("");
			break;
		}

		pRec->l_Message = sosArgs->m_sMessage;
	}

	delete sosArgs;

	UpdateDataView();
}

//-----------------------------------------------------------------------------
void BDSOSDocSender::OnSOSOperationFinished()
{
	m_bCanClose = TRUE;
	EndWaitCursor();

	EndTimer();

	m_nCurrentElement = 100;
	SetGaugeColors();

	UpdateDataView();
}

// generica inizializzazione dei dataobj assegnati ai controls
// ed altre variabili di documento
//-----------------------------------------------------------------------------
void BDSOSDocSender::InitSelections()
{
	if (m_pSOSSearchRules)
		m_pSOSSearchRules->Clear();

	m_pDBTSOSDocuments->RemoveAll();

	m_bOnlyMainDoc	= FALSE;
	m_bDocIdle		= TRUE;
	m_bDocToResend	= TRUE;

	attachmentsToSendList = gcnew List<AttachmentInfo^>();

	//inizializzo anche gli array di appoggio per le combobox
	m_pSOSDocTypeList		= new CStringArray();
	m_pSOSTaxJournalList	= new CStringArray();
	m_pSOSFiscalYearList	= new CStringArray();

	// array con elenco tipi documento tornati dal C#
	documentTypesList = gcnew List<String^>();

	// array con gli stati documento pilotati dalle checkbox
	docStatusList = gcnew List<StatoDocumento>();
	if (m_bDocIdle)
		docStatusList->Add(StatoDocumento::IDLE);
	if (m_bDocToResend)
		docStatusList->Add(StatoDocumento::TORESEND);

	UpdateDataView();
}

// changed sulla combobox delle classi documentali della SOS
// carico i tipi documento associati alla classe doc selezionata
//-----------------------------------------------------------------------------
void BDSOSDocSender::OnDocClassChanged()
{
	if (m_DocumentClass.IsEmpty())
		return;

	SOSDocClassInfo* pInfo = m_pSOSDocClassList->GetSOSDocClassInfoByCode(m_DocumentClass);
	if (!pInfo)
		return;

	if (m_pSOSDocTypeList)
		m_pSOSDocTypeList->RemoveAll();

	documentTypesList->Clear();
	documentTypesList = dmsOrchestrator->SosManager->GetAllDocumentType(pInfo->docClass->GetSOSDocumentTypes());

	if (documentTypesList->Count < 1)
	{
		m_pSOSDocTypeList->Add(_TB("No document types"));
		m_DocumentType.SetReadOnly(TRUE);
	}
	else
	{
		for each (String^ docType in ((List<String^>^)(documentTypesList)))
			m_pSOSDocTypeList->Add(docType);

		m_DocumentType.SetReadOnly(FALSE);
	}

	if (m_pSOSDocTypeList->GetSize() > 0)
		m_DocumentType = m_pSOSDocTypeList->GetAt(0);

	// faccio la set nell'itemsource successivo
	m_pSOSDocTypeItemSource->SetDocTypesList(m_pSOSDocTypeList);

	FireAction(_T("OnDocClassChanged"));
	UpdateDataView();
	
	OnDocTypeChanged();
}

// changed sulla combobox dei tipi documento 
// carico i Tax Journal associati al tipo documento selezionato
//-----------------------------------------------------------------------------
void BDSOSDocSender::OnDocTypeChanged()
{
	if (m_pSOSTaxJournalList)
		m_pSOSTaxJournalList->RemoveAll();

	if (m_DocumentType.IsEmpty() || documentTypesList->Count == 0)
	{
		m_pSOSTaxJournalList->Add(_TB("No tax journals"));
		m_TaxJournal.SetReadOnly(TRUE);
	}
	else
	{
		List<String^>^ taxJournalList = dmsOrchestrator->SosManager->GetAllTaxJournals(gcnew String(m_DocumentType.Str()), docStatusList);

		if (!taxJournalList || taxJournalList->Count == 0)
		{
			m_pSOSTaxJournalList->Add(_TB("No tax journals"));
			m_TaxJournal.SetReadOnly(TRUE);
		}
		else
		{
			for each (String^ taxJournal in taxJournalList)
			{
				if (!System::String::IsNullOrWhiteSpace(taxJournal))
					m_pSOSTaxJournalList->Add(taxJournal);
			}

			if (m_pSOSTaxJournalList->GetSize() == 0)
			{
				m_pSOSTaxJournalList->Add(_TB("No tax journals"));
				m_TaxJournal.SetReadOnly(TRUE);
			}
			else
				m_TaxJournal.SetReadOnly(FALSE);
		}
	}
	
	if (m_pSOSTaxJournalList->GetSize() > 0)
		m_TaxJournal = m_pSOSTaxJournalList->GetAt(0);

	// faccio la set nell'itemsource successivo
	m_pSOSTaxJournalItemSource->SetTaxJournalsList(m_pSOSTaxJournalList);

	FireAction(_T("OnDocTypeChanged"));
	UpdateDataView();

	OnTaxJournalChanged();
}

// changed sulla combobox dei Tax Journal
// carico i Fiscal Year disponibili
//-----------------------------------------------------------------------------
void BDSOSDocSender::OnTaxJournalChanged()
{
	if (m_pSOSFiscalYearList)
		m_pSOSFiscalYearList->RemoveAll();

	if (m_TaxJournal.IsEmpty())
	{
		m_pSOSFiscalYearList->Add(_TB("No fiscal years"));
		m_FiscalYear.SetReadOnly(TRUE);
	}
	else
	{
		CString taxJ = (m_TaxJournal.IsEmpty() || m_TaxJournal.CompareNoCase(_TB("No tax journals")) == 0) ? _TB("") : m_TaxJournal;

		List<String^>^ fiscalYearList = dmsOrchestrator->SosManager->GetAllFiscalYears(gcnew String(m_DocumentType.Str()), gcnew String(taxJ), docStatusList);

		if (!fiscalYearList || fiscalYearList->Count == 0)
		{
			m_pSOSFiscalYearList->Add(_TB("No fiscal years"));
			m_FiscalYear.SetReadOnly(TRUE);
		}
		else
		{
			for each (String^ fYear in fiscalYearList)
				m_pSOSFiscalYearList->Add(fYear);
			m_FiscalYear.SetReadOnly(FALSE);
		}
	}
	
	if (m_pSOSFiscalYearList->GetSize() > 0)
	{
		m_FiscalYear = m_pSOSFiscalYearList->GetAt(0);
		FireAction(_T("OnFiscalYearChanged"));

		m_pSOSFiscalYearItemSource->SetFiscalYearsList(m_pSOSFiscalYearList);
	}

	FireAction(_T("OnTaxJournalChanged"));
	UpdateDataView();
}

// changed sulla combobox dei Fiscal Year
// sparo un evento a chi e' in ascolto
//-----------------------------------------------------------------------------
void BDSOSDocSender::OnFiscalYearChanged()
{
	FireAction(_T("OnFiscalYearChanged"));
	UpdateDataView();
}

//-----------------------------------------------------------------------------
void BDSOSDocSender::OnStatusIdleChanged()
{
	if (m_bDocIdle)
	{
		if (!docStatusList->Contains(StatoDocumento::IDLE))
			docStatusList->Add(StatoDocumento::IDLE);
	}
	else
	{
		if (docStatusList->Contains(StatoDocumento::IDLE))
			docStatusList->Remove(StatoDocumento::IDLE);
	}
	OnDocTypeChanged();
}

//-----------------------------------------------------------------------------
void BDSOSDocSender::OnStatusToResendChanged()
{
	if (m_bDocToResend)
	{
		if (!docStatusList->Contains(StatoDocumento::TORESEND))
			docStatusList->Add(StatoDocumento::TORESEND);
	}
	else
	{
		if (docStatusList->Contains(StatoDocumento::TORESEND))
			docStatusList->Remove(StatoDocumento::TORESEND);
	}
	OnDocTypeChanged();
}

//-----------------------------------------------------------------------------	
void BDSOSDocSender::OnSelDeselClicked()
{
	m_bSelectDeselect = !m_bSelectDeselect;

	VSOSDocument* pRec;
	for (int i = 0; i <= m_pDBTSOSDocuments->GetUpperBound(); i++)
	{
		pRec = (VSOSDocument*)m_pDBTSOSDocuments->GetRow(i);
		pRec->l_IsSelected = !m_bSelectDeselect;
	}

	m_pBEBtnSelDesel->SetText(m_bSelectDeselect ? _TB("Select") : _TB("Deselect"));

	UpdateDataView();
}

// metodo richiamato sull'OnWizardNext della prima pagina
// per caricare i SOSDocument che soddisfano i filtri dell'utente
//-----------------------------------------------------------------------------
BOOL BDSOSDocSender::ExtractSOSDocuments()
{
	// attenzione che devo skippare le stringhe generiche in caso di combo vuote (tranne quella del tax journal)
	if (m_DocumentClass.IsReadOnly() || m_DocumentType.IsReadOnly() || m_FiscalYear.IsReadOnly())
	{
		Message(_TB("Some selections are missing!\r\nUnable to proceed."));
		return FALSE;
	}

	if (!m_bDocIdle && !m_bDocToResend)
	{
		Message(_TB("Please select at least one document status!"));
		return FALSE;
	}
	
	// riempio la struttura con tutti i filtri impostati dall'utente
	CreateSearchFilters();

	// richiamo il metodo esposto dal dmsorchestrator per avviare la ricerca
	if (m_pSOSDocumentArray)
		m_pSOSDocumentArray->RemoveAll();
	else
		m_pSOSDocumentArray = new RecordArray();

	SosSearchRules^ sosSearchRules = gcnew SosSearchRules();

	// passo l'elenco dei tipi documento da un CStringArray ad un List<String> per assegnarlo poi ai filtri C#
	List<String^>^ sosDocumentTypes = gcnew List<String^>();
	for (int i = 0; i < m_pSOSSearchRules->m_arSosDocumentTypes.GetSize(); i++)
	{
		CString doctype = m_pSOSSearchRules->m_arSosDocumentTypes.GetAt(i);
		sosDocumentTypes->Add(gcnew String(doctype.GetString()));
	}
	sosSearchRules->SosDocumentTypes = sosDocumentTypes;
	//

	sosSearchRules->SosTaxJournal = gcnew String(m_pSOSSearchRules->m_SosTaxJournal.GetString());
	sosSearchRules->FiscalYear = gcnew String(m_pSOSSearchRules->m_SosFiscalYear.GetString());

	List<StatoDocumento>^ sosDocumentStatusList = gcnew List<StatoDocumento>();
	if (m_pSOSSearchRules->m_bDocIdle)
		sosDocumentStatusList->Add(StatoDocumento::IDLE);
	if (m_pSOSSearchRules->m_bDocToResend)
		sosDocumentStatusList->Add(StatoDocumento::TORESEND);
	sosSearchRules->SosDocumentStatus = sosDocumentStatusList;

	sosSearchRules->ERPFieldsRuleList = m_pSOSSearchRules->erpFieldRules;
	//	

	SearchResultDataTable^ searchResultDT = dmsOrchestrator->GetSOSDocuments(sosSearchRules);

	if (searchResultDT != nullptr)
	{
		for each (DataRow^ row in searchResultDT->Rows)
		{
			VSOSDocument* pRec = new VSOSDocument();
			pRec->l_FileName = row[CommonStrings::Name]->ToString();
			pRec->l_AttachmentID = DataLng((int)row[CommonStrings::AttachmentID]);
			pRec->l_DescriptionKeys = row[CommonStrings::DocKeyDescription]->ToString();
			pRec->l_DocumentType = row[CommonStrings::DocNamespace]->ToString();
			pRec->l_DocumentStatus = ((int)row[CommonStrings::DocStatus] == 5) ? _TB("To resend") : _TB("To send");
			pRec->attachInfo = (Microarea::EasyAttachment::Components::AttachmentInfo^)row[CommonStrings::AttachmentInfo];
			m_pSOSDocumentArray->Add(pRec);
		}
	}

	if (!m_pSOSDocumentArray || m_pSOSDocumentArray->GetCount() == 0)
	{
		Message(_TB("No documents match your search filters"));
		m_pDBTSOSDocuments->RemoveAll();
		return FALSE;
	}

	// forzo il riempimento del DBT con elenco dei SOSDocument estratti dalla ricerca
	m_pDBTSOSDocuments->LoadSOSDocuments(m_pSOSDocumentArray);

	return TRUE;
}

//-----------------------------------------------------------------------------
void BDSOSDocSender::AddERPFilter(const CString& name, DataObj* pFromData, DataObj* pToData)
{
	if (m_pSOSSearchRules)
		m_pSOSSearchRules->AddERPFilter(name, pFromData, pToData);
}

// riempie la classe dei filtri in base alle selezioni effettuate nella form
//-----------------------------------------------------------------------------
void BDSOSDocSender::CreateSearchFilters()
{
	if (!m_pSOSSearchRules)
		m_pSOSSearchRules = new CSOSSearchRules();
	m_pSOSSearchRules->Clear();

	m_pSOSSearchRules->m_arSosDocumentTypes.Add(m_DocumentType);

	m_pSOSSearchRules->m_SosFiscalYear = m_FiscalYear;

	// devo epurare la stringa No tax journals
	CString taxJ = (m_TaxJournal.CompareNoCase(_TB("No tax journals")) == 0) ? _TB("") : m_TaxJournal;
	m_pSOSSearchRules->m_SosTaxJournal = taxJ;
	
	m_pSOSSearchRules->m_bOnlyMainDoc = m_bOnlyMainDoc;

	m_pSOSSearchRules->m_bDocIdle		= m_bDocIdle;
	m_pSOSSearchRules->m_bDocToResend	= m_bDocToResend;
	FireAction(_T("OnCreateSOSSearchFilter"));
}

// riempio le strutture locali con le info lette dal C# 
//-----------------------------------------------------------------------------
void BDSOSDocSender::LoadSOSDocClasses()
{
	// mi faccio ritornare dal C# l'elenco delle classi documentali
	RecordArray* arSosDoc = AfxGetTbRepositoryManager()->GetSOSConfiguration()->m_pSOSDocClassesArray;

	if (arSosDoc)
	{
		m_pSOSDocClassList = new SOSDocClassList();
 		VSOSDocClass* pRec;
		for (int i = 0; i < arSosDoc->GetSize(); i++)
		{
			pRec = (VSOSDocClass*)arSosDoc->GetAt(i);
			SOSDocClassInfo* pInfo = new SOSDocClassInfo();
			pInfo->m_Code			= pRec->l_Code;
			pInfo->m_Description	= pRec->l_Description;
			pInfo->docClass			= pRec->docClass; //mi tengo da parte anche il puntatore alla DocClass C#
			m_pSOSDocClassList->Add(pInfo);
		}
	}
}

//---------------------------------------------------------------------------
void BDSOSDocSender::OnCloseDocument()
{
	if (!m_bCanClose)
	{
		AfxMessageBox(_TB("Unable to close the form because the process is running!\r\nPlease try again in a few seconds..."));
		return;
	}

	CWizardFormDoc::OnCloseDocument();
}

//---------------------------------------------------------------------------------------------
void BDSOSDocSender::StartTimer()
{
	// scatta ogni secondo
	GetFirstView()->SetTimer(CHECK_SOSDOCSENDER_TIMER, 1000, NULL);
	SetProgressRange(100);
	if (m_pGauge)
		m_pGauge->UpdateCtrlView();
}

//-----------------------------------------------------------------------------------------------
void BDSOSDocSender::EndTimer()
{
	GetFirstView()->KillTimer(CHECK_SOSDOCSENDER_TIMER);
}

//------------------------------------------------------------------------------
void BDSOSDocSender::DoOnTimer()
{
	StepProgressBar();
	UpdateDataView();
}

//-----------------------------------------------------------------------------
void BDSOSDocSender::StepProgressBar()
{
	if (!m_pGauge)
		return;

	m_nCurrentElement += 1;
	SetGaugeColors();

	m_pGauge->UpdateCtrlView();
	UpdateDataView();
}

//------------------------------------------------------------------------------------
void BDSOSDocSender::SetGaugeColors()
{
	if (!m_pGauge)
		return;

	DataDbl nPos = (m_GaugeRange * m_nCurrentElement) / m_Range;
	if (nPos > m_Range) // se ho raggiunto il max riazzero il CurrentElement
		m_nCurrentElement = 0;

	m_pGauge->ModifyRange(0, 0, nPos);
}

//-----------------------------------------------------------------------------
void BDSOSDocSender::SetProgressRange(int nRange)
{
	if (nRange <= 0 || !m_pGauge)
		return;

	m_Range = nRange;

	m_GaugeRange = 100;
	m_nCurrentElement = 0;

	m_pGauge->SetGaugeRange(0, m_GaugeRange);
	m_pGauge->ModifyRange(0, 0, 0);

	UpdateDataView();
}
