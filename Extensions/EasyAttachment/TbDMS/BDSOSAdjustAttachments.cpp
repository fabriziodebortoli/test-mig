#include "stdafx.h"

#include <ExtensionsImages\CommonImages.h>

#include "EasyAttachment\JsonForms\UISOSAdjustAttachments\IDD_SOSADJUSTATTACH_WIZARD.hjson"

#include "TBRepositoryManager.h"
#include "BDSOSAdjustAttachments.h"
#include "UISOSAdjustAttachments.h"

using namespace System;
using namespace System::Data;
using namespace System::Collections::Generic; 
using namespace System::Collections::Specialized;
using namespace Microarea::EasyAttachment;
using namespace Microarea::EasyAttachment::BusinessLogic;
using namespace Microarea::EasyAttachment::Components;
using namespace Microarea::EasyAttachment::Core;
using namespace Microarea::TaskBuilderNet::Core::WebServicesWrapper;
using namespace Microarea::TaskBuilderNet::Interfaces;

//===========================================================================
//							DMSSOSAdjustAttachmentsEvents
// ref class that manages communication between Event Handlers and C++ CWnd
//===========================================================================
void DMSSOSAdjustAttachmentsEvents::InitializeEvents(BDSOSAdjustAttachments* pDoc, SOSManager^ sosManager)
{
	m_pDoc = pDoc;
	if (sosManager)
	{
		sosManager->SOSOperationCompleted += gcnew EventHandler<SOSEventArgs^>(this, &DMSSOSAdjustAttachmentsEvents::OnSOSOperationCompleted);
		sosManager->AdjustAttachmentsFinished += gcnew EventHandler(this, &DMSSOSAdjustAttachmentsEvents::OnAdjustAttachmentsFinished);
	}
}

// evento intercettato in esecuzione delle singole operazioni di invio
//-----------------------------------------------------------------------------
void DMSSOSAdjustAttachmentsEvents::OnSOSOperationCompleted(System::Object^, Microarea::EasyAttachment::Components::SOSEventArgs^ eventArg)
{
	if (m_pDoc)
	{
		// sono su un thread separato! 
		AfxInvokeThreadProcedure<BDSOSAdjustAttachments, CSOSEventArgs*>(m_pDoc->GetFrameHandle(), m_pDoc, &BDSOSAdjustAttachments::OnSOSOperationCompleted, CreateSOSEventArgs(eventArg));
	}
}

// evento intercettato al termine dell'elaborazione
//-----------------------------------------------------------------------------
void DMSSOSAdjustAttachmentsEvents::OnAdjustAttachmentsFinished(System::Object^, EventArgs^ eventArg)
{
	if (m_pDoc)
	{
		// sono su un thread separato! 
		AfxInvokeThreadProcedure<BDSOSAdjustAttachments>(m_pDoc->GetFrameHandle(), m_pDoc, &BDSOSAdjustAttachments::OnAdjustAttachmentsFinished);
	}
}

/////////////////////////////////////////////////////////////////////////////
//				class BDSOSAdjustAttachments Implementation
/////////////////////////////////////////////////////////////////////////////
//----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(BDSOSAdjustAttachments, CWizardFormDoc)

//----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(BDSOSAdjustAttachments, CWizardFormDoc)
	////{{AFX_MSG_MAP(BDSOSAdjustAttachments)
	ON_EN_VALUE_CHANGED	(IDC_SOSADJUSTATTACH_DOCCLASS_COMBO,	OnDocClassChanged)
	ON_EN_VALUE_CHANGED	(IDC_SOSADJUSTATTACH_DOCTYPE_COMBO,		OnDocTypeChanged)
	ON_EN_VALUE_CHANGED	(IDC_SOSADJUSTATTACH_FISCALYEAR_COMBO, OnFiscalYearChanged)

	ON_COMMAND			(ID_SOSADJUSTATTACH_BTN_SELDESEL,		OnSelDeselClicked)
	////}}AFX_MSG_MAP
END_MESSAGE_MAP()

//----------------------------------------------------------------------------
BDSOSAdjustAttachments::BDSOSAdjustAttachments()
	:
	dmsOrchestrator				(nullptr),
	dmsSOSAdjustAttachEvents	(nullptr),
	m_pDBTSOSDocuments			(NULL),
	m_pDBTSOSElaboration		(NULL),
	m_pSOSSearchRules			(NULL),
	m_pSOSDocClassList			(NULL),
	m_pSOSDocTypeList			(NULL),
	m_pSOSFiscalYearList		(NULL),
	m_pSOSDocClassInfo			(NULL),
	m_bCanShowElaborationDlg	(FALSE),
	m_bCanClose					(TRUE),
	m_pSOSDocumentArray			(NULL),
	m_pGauge					(NULL),
	m_nCurrentElement			(0),
	m_GaugeRange				(100),
	m_Range						(0),
	attachmentsList				(nullptr),
	m_bSelectDeselect			(TRUE),
	m_pSOSDocTypeItemSource		(NULL),
	m_pSOSFiscalYearItemSource	(NULL),
	m_pBEBtnSelDesel			(NULL)
{
}

//----------------------------------------------------------------------------
BDSOSAdjustAttachments::~BDSOSAdjustAttachments()
{
	SAFE_DELETE(m_pSOSSearchRules);
	SAFE_DELETE(m_pDBTSOSDocuments);
	SAFE_DELETE(m_pDBTSOSElaboration);
	SAFE_DELETE(m_pSOSDocClassList);
	SAFE_DELETE(m_pSOSDocTypeList);
	SAFE_DELETE(m_pSOSFiscalYearList);
	SAFE_DELETE(m_pSOSDocumentArray);
	
	if ((DMSOrchestrator^)dmsOrchestrator != nullptr)
	{
		delete dmsOrchestrator;
		dmsOrchestrator = nullptr;
	}
}

//-----------------------------------------------------------------------------
BOOL BDSOSAdjustAttachments::OnAttachData()
{
	__super::OnAttachData();	// serve per i titoli

	SetFormTitle(_TB("SOS Adjust Attachments"));
	SetFormName(_TB("SOS Adjust Attachments"));

	dmsOrchestrator = gcnew DMSOrchestrator();
	AfxGetTbRepositoryManager()->InitializeManager(this);

	dmsSOSAdjustAttachEvents = gcnew DMSSOSAdjustAttachmentsEvents();
	dmsSOSAdjustAttachEvents->InitializeEvents(this, dmsOrchestrator->SosManager);

	// carico dal database le informazioni per popolare i controls
	LoadDMSInformation();

	m_pDBTSOSDocuments		= new DBTSOSDocuments	(RUNTIME_CLASS(VSOSDocument), this);
	m_pDBTSOSElaboration	= new DBTSOSElaboration	(RUNTIME_CLASS(VSOSElaboration), this);
	
	DECLARE_VAR_JSON(DocumentClass);
	DECLARE_VAR_JSON(DocumentType);
	DECLARE_VAR_JSON(FiscalYear);
	DECLARE_VAR_JSON(ElaborationMessage);
	DECLARE_VAR_JSON(nCurrentElement);

	RegisterControl(IDC_SOSADJUSTATTACH_BE_RESULTS,	RUNTIME_CLASS(CSOSAdjustAttachmentsResultsBodyEdit));

	InitSelections();

	return TRUE;
}

//----------------------------------------------------------------------------
void BDSOSAdjustAttachments::OnParsedControlCreated(CParsedCtrl* pCtrl)
{
	__super::OnParsedControlCreated(pCtrl);

	UINT nIDC = pCtrl->GetCtrlID();

	if (nIDC == IDC_SOSADJUSTATTACH_DOCCLASS_COMBO)
	{
		CSOSDocClassesItemSource* pDocClassesItemSource = (CSOSDocClassesItemSource*)pCtrl->GetItemSource();
		pDocClassesItemSource->SetDocClassList(m_pSOSDocClassList);
		return;
	}

	if (nIDC == IDC_SOSADJUSTATTACH_DOCTYPE_COMBO)
	{
		m_pSOSDocTypeItemSource = (CSOSDocTypeItemSource*)pCtrl->GetItemSource();
		return;
	}

	if (nIDC == IDC_SOSADJUSTATTACH_FISCALYEAR_COMBO)
	{
		m_pSOSFiscalYearItemSource = (CSOSFiscalYearItemSource*)pCtrl->GetItemSource();
		return;
	}

	if (nIDC == IDC_SOSADJUSTATTACH_GAUGE)
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
void BDSOSAdjustAttachments::CustomizeBodyEdit(CBodyEdit* pBodyEdit)
{
	__super::CustomizeBodyEdit(pBodyEdit);

	if (pBodyEdit->GetNamespace().GetObjectName() == _NS_BE("BESOSAdjustAttach"))
	{
		CSOSAdjustAttachmentsResultsBodyEdit* pResultsBE = (CSOSAdjustAttachmentsResultsBodyEdit*)pBodyEdit;
		// a regime questo dovrebbe sparire!
		if (pResultsBE)
			m_pBEBtnSelDesel = pResultsBE->m_HeaderToolBar.FindButton(ID_SOSADJUSTATTACH_BTN_SELDESEL);
		return;
	}
}

// riempio le strutture locali con le info lette dal C# 
//-----------------------------------------------------------------------------
void BDSOSAdjustAttachments::LoadDMSInformation()
{
	// mi faccio ritornare dal C# l'elenco delle classi documentali
	RecordArray* arSosDoc = AfxGetTbRepositoryManager()->GetSOSConfiguration()->m_pSOSDocClassesArray;

	if (arSosDoc)
	{
		m_pSOSDocClassList = new SOSDocClassList();
		VSOSDocClass* pRec;
		CTBNamespace docNamespace;
		bool toAtt;
		for (int i = 0; i < arSosDoc->GetSize(); i++)
		{
			pRec = (VSOSDocClass*)arSosDoc->GetAt(i);

			try
			{
				// skippo la classi documentali non gestite dalla SOS (ad esempio quelle di Fatel)
				if (!pRec || pRec->docClass->InternalDocClass == "" || pRec->docClass->ERPDocNamespaces->Count == 0)
					continue;
			}
			catch (System::NullReferenceException^)
			{
				// per pararci :)
				continue;
			}

			//non devo considerare la classi documentali che si riferiscono a soli documenti di tipo batch (vedi stampe fiscali)
			for each (ERPSOSDocumentType^ docType in pRec->docClass->ERPDocNamespaces)
			{
				docNamespace.SetNamespace(CString(docType->DocNamespace));
				toAtt = false;
				const CDocumentDescription* pDocDescri = docNamespace.IsValid() ? AfxGetDocumentDescription(docNamespace) : NULL;
				if (pDocDescri && pDocDescri->GetFirstViewMode() && pDocDescri->GetFirstViewMode()->GetType() == VMT_DATAENTRY)
				{
					toAtt = true;
					break;
				}
			}
			if (toAtt)
			{
				SOSDocClassInfo* pInfo = new SOSDocClassInfo();
				pInfo->m_Code = pRec->l_Code;
				pInfo->m_Description = pRec->l_Description;
				pInfo->docClass = pRec->docClass; //mi tengo da parte anche il puntatore alla DocClass C#
				m_pSOSDocClassList->Add(pInfo);
			}
		}
	}
}

// generica inizializzazione dei dataobj assegnati ai controls
// ed altre variabili di documento
//-----------------------------------------------------------------------------
void BDSOSAdjustAttachments::InitSelections()
{
	if (m_pSOSSearchRules)
		m_pSOSSearchRules->Clear();

	m_pDBTSOSDocuments->RemoveAll();

	attachmentsList = gcnew List<AttachmentInfo^>();

	//inizializzo anche gli array di appoggio per le combobox
	m_pSOSDocTypeList = new CStringArray();
	m_pSOSFiscalYearList = new CStringArray();

	// nella combo carico gli ultimi 5 anni
	int lastYear = AfxGetApplicationYear();
	for (int i = lastYear; i >= lastYear - 5; i--)
		m_pSOSFiscalYearList->Add(i.ToString());
	if (m_pSOSFiscalYearList->GetCount() > 0)
		m_FiscalYear = m_pSOSFiscalYearList->GetAt(0);

	UpdateDataView();
}

//---------------------------------------------------------------------------
void BDSOSAdjustAttachments::OnWizardActivate(UINT nPageIDD)
{
	if (nPageIDD == IDD_WTD_SOSADJUSTATTACH_FILTERS)
	{
		SetHeaderTitle(_TB(""));
		SetHeaderSubTitle(_TB("First of all set filters in order to extract the attachments you want to update and make available for sending to SOStitutiva."));
		return;
	}

	if (nPageIDD == IDD_WTD_SOSADJUSTATTACH_DOCSELECTIONS)
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

//-----------------------------------------------------------------------------
LRESULT BDSOSAdjustAttachments::OnWizardNext(UINT nIDD)
{
	if (nIDD == IDD_WTD_SOSADJUSTATTACH_FILTERS)
	{
		// se le selezioni minime sono mancanti o se non ho estratto documenti non procedo
		if (!ExtractAttachmentsToAdjust())
			return WIZARD_SAME_TAB;
	}

	return WIZARD_DEFAULT_TAB;
}

//---------------------------------------------------------------------------
void BDSOSAdjustAttachments::OnEnableWizardNext(CCmdUI* pCmdUI)
{
	BOOL bEnable = GetTabWizard()->GetActiveDlg()->GetDlgInfoItem()->GetDialogID() == IDD_WTD_SOSADJUSTATTACH_FILTERS;
	pCmdUI->Enable(bEnable);
}

//---------------------------------------------------------------------------
void BDSOSAdjustAttachments::OnEnableWizardBack(CCmdUI* pCmdUI)
{
	BOOL bEnable = GetTabWizard()->GetActiveDlg()->GetDlgInfoItem()->GetDialogID() != IDD_WTD_SOSADJUSTATTACH_ELABORATION;
	pCmdUI->Enable(bEnable);
}

//---------------------------------------------------------------------------
BOOL BDSOSAdjustAttachments::CanDoBatchExecute()
{
	// E' abilitato sulla seconda tab
	BOOL bEnable = GetTabWizard()->GetActiveDlg()->GetDlgInfoItem()->GetDialogID() == IDD_WTD_SOSADJUSTATTACH_DOCSELECTIONS;
	return bEnable;
}

//-----------------------------------------------------------------------------
BOOL BDSOSAdjustAttachments::CanRunDocument()
{
	if (!AfxGetOleDbMng()->DMSSOSEnable())
	{
		AfxMessageBox(_TB("Impossible to open SOS Adjust Attachments form!\r\nPlease, check in Administration Console if this company uses DMS and if DMS Advanced is activated."));
		return FALSE;
	}

	if (!AfxGetTbRepositoryManager()->GetDMSSettings()->GetSettings()->l_EnableSOS)
	{
		AfxMessageBox(_TB("You are not allowed to open SOS Adjust Attachments form! In order to open it change your setting parameter and enable SOS."));
		return FALSE;
	}

	return TRUE;
}

//-----------------------------------------------------------------------------
void BDSOSAdjustAttachments::DisableControlsForBatch()
{
	m_DocumentType.SetReadOnly(m_DocumentType.IsEmpty());
	m_FiscalYear.SetReadOnly(m_FiscalYear.IsEmpty());

	UpdateDataView();
}

//-----------------------------------------------------------------------------
void BDSOSAdjustAttachments::OnBatchExecute()
{
	// prima controllo che i filtri siano corretti
	m_bCanShowElaborationDlg = CheckFilters();

	if (!m_bCanShowElaborationDlg)
		return;

	BeginWaitCursor();
	m_bCanClose = FALSE;
	m_bBatchRunning = TRUE;

	// in questo metodo parte un thread separato di elaborazione lato C#
	dmsOrchestrator->SosManager->AdjustAttachmentsForSOS
		(
			attachmentsList,
			gcnew ERPSOSDocumentType(gcnew String(m_ERPSOSDocumentType.m_DocNamespace.Str()), gcnew String(m_ERPSOSDocumentType.m_DocType.Str()))
		);

	m_bBatchRunning = FALSE;
	
	UpdateDataView();
}

// eseguo un check preventivo prima di eseguire l'elaborazione vera e propria
//-----------------------------------------------------------------------------
BOOL BDSOSAdjustAttachments::CheckFilters()
{
	// pulisco la lista degli allegati da inviare
	attachmentsList->Clear();

	// mi tengo da parte gli AttachmentInfo delle righe selezionate e nel frattempo le conto
	int nSelRowsCount = 0;
	for (int i = 0; i < m_pDBTSOSDocuments->GetSize(); i++)
	{
		VSOSDocument* pRec = (VSOSDocument*)m_pDBTSOSDocuments->GetRow(i);
		if (pRec->l_IsSelected)
		{
			attachmentsList->Add(pRec->attachInfo);
			nSelRowsCount++;
		}
	}

	if (nSelRowsCount == 0)
	{
		Message(_TB("You have to select at least one attachment! Unable to proceed!"), MB_ICONSTOP);
		return FALSE;
	}

	if (AfxMessageBox(cwsprintf(_TB("{0-%d} attachments selected and ready to create new SOS documents.\r\nDo you want to proceed with elaboration?"), nSelRowsCount), MB_YESNO | MB_ICONQUESTION | MB_DEFBUTTON2) == IDNO)
		return FALSE;

	return TRUE;
}

//----------------------------------------------------------------------------
void BDSOSAdjustAttachments::OnBatchCompleted()
{
	__super::OnBatchCompleted();

	if (m_bCanShowElaborationDlg) // mi posiziono sull'ultima pagina solo se le selezioni sono ok
	{
		((CSOSAdjustAttachmentsWizardFormView*)GetFirstView())->OnWizardNext();
		// faccio partire il timer solo se e' partita l'elaborazione
		StartTimer();
	}
}

//-----------------------------------------------------------------------------
void BDSOSAdjustAttachments::OnSOSOperationCompleted(CSOSEventArgs* sosArgs)
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

// evento intercettato alla fine dell'elaborazione
//-----------------------------------------------------------------------------
void BDSOSAdjustAttachments::OnAdjustAttachmentsFinished()
{
	m_bCanClose = TRUE;
	EndWaitCursor();

	EndTimer();

	m_nCurrentElement = 100;
	SetGaugeColors();

	UpdateDataView();
}

// changed sulla combobox delle classi documentali della SOS
// carico tutti i tipi documento associati alla classe doc selezionata
//-----------------------------------------------------------------------------
void BDSOSAdjustAttachments::OnDocClassChanged()
{
	if (m_DocumentClass.IsEmpty())
		return;

	m_pSOSDocClassInfo = m_pSOSDocClassList->GetSOSDocClassInfoByCode(m_DocumentClass);
	if (!m_pSOSDocClassInfo)
		return;

	if (m_pSOSDocTypeList)
		m_pSOSDocTypeList->RemoveAll();

	CTBNamespace docNamespace;
	// carico tutti i tipi documento (non batch) previsti per la classe documentale
	for each (ERPSOSDocumentType^ docType in m_pSOSDocClassInfo->docClass->ERPDocNamespaces)
	{
		docNamespace.SetNamespace(CString(docType->DocNamespace));
		const CDocumentDescription* pDocDescri = docNamespace.IsValid() ? AfxGetDocumentDescription(docNamespace) : NULL;
		if (pDocDescri && pDocDescri->GetFirstViewMode() && pDocDescri->GetFirstViewMode()->GetType() == VMT_DATAENTRY)
			m_pSOSDocTypeList->Add(docType->DocType->ToString());
	}

	m_DocumentType.SetReadOnly(m_pSOSDocTypeList->GetCount() < 1);

	if (m_pSOSDocTypeList->GetSize() > 0)
		m_DocumentType = m_pSOSDocTypeList->GetAt(0);

	m_pSOSDocTypeItemSource->SetDocTypesList(m_pSOSDocTypeList);

	FireAction(_T("OnDocClassChanged"));
	OnDocTypeChanged();
	
	if (m_pSOSFiscalYearList->GetCount() > 0)
		m_FiscalYear = m_pSOSFiscalYearList->GetAt(0);

	m_pSOSFiscalYearItemSource->SetFiscalYearsList(m_pSOSFiscalYearList);

	OnFiscalYearChanged();
}

// changed sulla combobox del tipo documento
// mi tengo da parte anche il namespace del documento
//-----------------------------------------------------------------------------
void BDSOSAdjustAttachments::OnDocTypeChanged()
{
	if (!m_pSOSDocClassInfo)
		return;
	
	for each (ERPSOSDocumentType^ docType in m_pSOSDocClassInfo->docClass->ERPDocNamespaces)
	{
		if (String::Compare(docType->DocType, gcnew String(m_DocumentType.Str()), StringComparison::InvariantCultureIgnoreCase) == 0)
		{
			m_ERPSOSDocumentType.m_DocType = docType->DocType->ToString();
			m_ERPSOSDocumentType.m_DocNamespace = docType->DocNamespace->ToString();
		}
	}

	FireAction(_T("OnDocTypeChanged"));
	UpdateDataView();
}

//-----------------------------------------------------------------------------
void BDSOSAdjustAttachments::OnFiscalYearChanged()
{
	FireAction(_T("OnFiscalYearChanged"));
	UpdateDataView();
}

//-----------------------------------------------------------------------------
void BDSOSAdjustAttachments::OnSelDeselClicked()
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

// aggiungo i filtri gestionali come da evento del ClientDoc CDSOSFilter
//-----------------------------------------------------------------------------
void BDSOSAdjustAttachments::AddERPFilter(const CString& name, DataObj* pFromData, DataObj* pToData)
{
	if (m_pSOSSearchRules)
		m_pSOSSearchRules->AddERPFilter(name, pFromData, pToData);
}

// metodo richiamato sull'OnWizardNext della prima pagina
// per caricare i SOSDocument che soddisfano i filtri dell'utente
//-----------------------------------------------------------------------------
BOOL BDSOSAdjustAttachments::ExtractAttachmentsToAdjust()
{
	// attenzione che devo skippare le stringhe generiche in caso di combo vuote
	if (m_DocumentClass.IsReadOnly() || m_DocumentType.IsReadOnly() || m_FiscalYear.IsReadOnly())
	{
		Message(_TB("Some selections are missing!\r\nUnable to proceed."));
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
	sosSearchRules->SosDocumentTypes	= sosDocumentTypes;
	sosSearchRules->ERPSosDocumentType	= gcnew ERPSOSDocumentType(gcnew String(m_pSOSSearchRules->m_ERPSosDocumentType.m_DocNamespace.Str()), gcnew String(m_pSOSSearchRules->m_ERPSosDocumentType.m_DocType.Str()));
	sosSearchRules->FiscalYear			= gcnew String(m_pSOSSearchRules->m_SosFiscalYear.GetString());
	sosSearchRules->ERPFieldsRuleList	= m_pSOSSearchRules->erpFieldRules;

	SearchResultDataTable^ searchResultDT = dmsOrchestrator->GetAttachmentsToAdjustForSOS(sosSearchRules);

	if (searchResultDT != nullptr)
	{
		for each (DataRow^ row in searchResultDT->Rows)
		{
			VSOSDocument* pRec = new VSOSDocument();
			pRec->l_FileName = row[CommonStrings::Name]->ToString();
			pRec->l_AttachmentID = DataLng((int)row[CommonStrings::AttachmentID]);
			pRec->l_DescriptionKeys = row[CommonStrings::DocKeyDescription]->ToString();
			pRec->l_DocumentType = row[CommonStrings::DocNamespace]->ToString();
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

// riempie la classe dei filtri in base alle selezioni effettuate nella form
//-----------------------------------------------------------------------------
void BDSOSAdjustAttachments::CreateSearchFilters()
{
	if (!m_pSOSSearchRules)
		m_pSOSSearchRules = new CSOSSearchRules();
	
	m_pSOSSearchRules->Clear();
	m_pSOSSearchRules->m_arSosDocumentTypes.Add(m_DocumentType);	
	m_pSOSSearchRules->m_ERPSosDocumentType = m_ERPSOSDocumentType;	
	m_pSOSSearchRules->m_SosFiscalYear = m_FiscalYear;

	FireAction(_T("OnCreateSOSSearchFilterForAdjust"));
}

//---------------------------------------------------------------------------
void BDSOSAdjustAttachments::OnCloseDocument()
{
	if (!m_bCanClose)
	{
		AfxMessageBox(_TB("Unable to close the form because the process is running!\r\nPlease try again in a few seconds..."));
		return;
	}

	CWizardFormDoc::OnCloseDocument();
}

//---------------------------------------------------------------------------------------------
void BDSOSAdjustAttachments::StartTimer()
{
	// scatta ogni secondo
	GetFirstView()->SetTimer(CHECK_SOSADJUSTATTACH_TIMER, 1000, NULL);
	SetProgressRange(100);
	if (m_pGauge)
		m_pGauge->UpdateCtrlView();
}

//-----------------------------------------------------------------------------------------------
void BDSOSAdjustAttachments::EndTimer()
{
	GetFirstView()->KillTimer(CHECK_SOSADJUSTATTACH_TIMER);
}

//------------------------------------------------------------------------------
void BDSOSAdjustAttachments::DoOnTimer()
{
	StepProgressBar();
	UpdateDataView();
}

//-----------------------------------------------------------------------------
void BDSOSAdjustAttachments::StepProgressBar()
{
	if (!m_pGauge)
		return;

	m_nCurrentElement += 1;
	SetGaugeColors();

	m_pGauge->UpdateCtrlView();
	UpdateDataView();
}

//------------------------------------------------------------------------------------
void BDSOSAdjustAttachments::SetGaugeColors()
{
	if (!m_pGauge)
		return;

	DataDbl nPos = (m_GaugeRange * m_nCurrentElement) / m_Range;
	if (nPos > m_Range) // se ho raggiunto il max riazzero il CurrentElement
		m_nCurrentElement = 0;

	m_pGauge->ModifyRange(0, 0, nPos);
}

//-----------------------------------------------------------------------------
void BDSOSAdjustAttachments::SetProgressRange(int nRange)
{
	if (nRange <= 0)
		return;

	m_Range = nRange;

	m_GaugeRange = 100;
	m_nCurrentElement = 0;

	m_pGauge->SetGaugeRange(0, m_GaugeRange);
	m_pGauge->ModifyRange(0, 0, 0);

	UpdateDataView();
}
