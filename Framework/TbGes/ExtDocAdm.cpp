
#include "stdafx.h"
#include <TbFrameworkImages\CommonImages.h>
#include <TbFrameworkImages\GeneralFunctions.h>

#include <TbGeneric\globals.h>
#include <TbGeneric\DataObj.h>
#include <TbGeneric\StatusBarMessages.h>
#include <TbGeneric\minmax.h>
#include <TbGeneric\LocalizableObjs.h>

#include <TbGenlib\reswalk.h>
#include <TbGenlib\parsobj.h>
#include <TbGenlib\messages.h>
#include <TbGenlib\generic.h>

#include <TbOledb\oledbmng.h>
#include <TbOledb\sqlrec.h>
#include <TbOledb\sqltable.h>			
#include <TbOledb\sqlcatalog.h>

#include "browser.h"
#include "bodyedit.h"
#include "hotlink.h"
#include "dbt.h"
#include "barquery.h"
#include "extdoc.h"
#include "tabber.h"
#include "formmng.h"
#include "IAbstractFormDocObject.h"
//................................. resources
#include "extdoc.hjson" //JSON AUTOMATIC UPDATE
#include "barquery.hjson" //JSON AUTOMATIC UPDATE

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"
//============================================================================

#define IDT_ADM				100		// id of timer fo ADM
#define DELAY_ADM_MS		500		// Delay of refresh of ADM in ms
#define PROGRESS_BAR_MAX	50



static const TCHAR szOpenAction[] = _T("Apertura documento");
static const TCHAR szFindAction[] = _T("Ricerca documento");
static const TCHAR szBrowseAction[] = _T("Browse documento");
static const TCHAR szEditAction[] = _T("Edit documento");
static const TCHAR szNewAction[] = _T("Nuovo documento");
static const TCHAR szSaveAction[] = _T("Salvataggio documento");
static const TCHAR szDeleteAction[] = _T("Cancellazione documento");

IMPLEMENT_ADMCLASS(ADMObj)

//-----------------------------------------------------------------------------
ADMObj::ADMObj()
{
	m_pDocument 			= NULL;
	m_pOriginalDocMessages	= NULL;
}

//-----------------------------------------------------------------------------
ADMObj::~ADMObj()
{
	ADMRestoreMessages();
}

//-----------------------------------------------------------------------------
SqlRecord*	ADMObj::GetMasterRecord	()	
{
	ASSERT(m_pDocument); 
	return m_pDocument->m_pDBTMaster->GetRecord();
}

// dirotta la messaggistica del documento ADM sullo stesso CMessages del
// documento chiamante, in modo che questo possa gestirne la visualizzazione
// nel momento opportuno (fasi batch, ecc.). Mantiene il puntatore al messages
// originale per ripristinarlo in fase di chiusura del documento
//-----------------------------------------------------------------------------
void ADMObj::ADMSetMessages(CMessages* pMessages)
{
	ASSERT(m_pDocument);
	m_pOriginalDocMessages		= m_pDocument->m_pMessages;
	m_pDocument->m_pMessages	= pMessages;
}


// eventualmente non venisse chiamata la ADMSaveDocument il programmatore
// può ripristinare il puntatore originale
//-----------------------------------------------------------------------------
void ADMObj::ADMRestoreMessages()
{   
	if (m_pOriginalDocMessages && m_pDocument)
		m_pDocument->m_pMessages = m_pOriginalDocMessages;

	m_pOriginalDocMessages = NULL;
}

//@@ADM @@TODO
//
// Le routines di ADMNewDocument .... Save ... Edit... Delete...
//
// non dovrebbero tornare BOOL ma il motivo del'errore (tipo HotLink etc..)
//
// LOCKED, SUCCESS, ERROR etc....

//-----------------------------------------------------------------------------
BOOL ADMObj::ADMNewDocument()
{    
	ASSERT(m_pDocument);
	ASSERT(m_pDocument->m_pDBTMaster);
    
	CUpdateDataViewLevel __upd(m_pDocument);
    
	// e` come se il documento stesse girando con un processo batch
	if (m_pDocument->m_BatchRunningArea.IsLocked())
		return FALSE;

    m_pDocument->m_bBatchRunning = TRUE;
	m_pDocument->m_BatchScheduler.Start();

	m_pDocument->ConnectToDatabase(szNewAction);

	if (!m_pDocument->OnOkNewRecord() || !m_pDocument->m_pClientDocs->OnOkNewRecord())
	{
		ADMGoInBrowseMode();
		m_pDocument->DisconnectFromDatabase();
		return FALSE;
	}


	if (!m_pDocument->m_pDBTMaster->AddNew())
	{
		ADMGoInBrowseMode();
		m_pDocument->DisconnectFromDatabase(); 
		return FALSE;
	}

	// Seleziona il  modo NEW per aggiungere un nuovo record
	m_pDocument->SetFormMode(CBaseDocument::NEW);
	
	if (!m_pDocument->DispatchPrepareAuxData(TRUE))
	{
		ADMGoInBrowseMode();
		m_pDocument->DisconnectFromDatabase(); 
		return FALSE;
	}
		
	m_pDocument->DisconnectFromDatabase();

	m_pDocument->EnableControls();
	m_pDocument->ToolBarButtonsHideGhost(0);

	//m_pDocument->UpdateDataView(); sostituita da CUpdateDataViewLevel __upd(m_pDocument);
	
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL ADMObj::ADMEditDocument()
{    
	ASSERT(m_pDocument);
	ASSERT(m_pDocument->m_pDBTMaster);

    // e` come se il documento stesse girando con un processo batch
	if (m_pDocument->m_BatchRunningArea.IsLocked())
		return FALSE;

	CUpdateDataViewLevel __upd(m_pDocument);

    m_pDocument->m_bBatchRunning = TRUE;
	m_pDocument->m_BatchScheduler.Start();
   	
	m_pDocument->ConnectToDatabase(szEditAction);

    // mi metto in stato di Browse e forzo (TRUE in FindData) il caricamento
	// di tutti i DBT anche per quelli DELAYED (EDIT e ALL) 
	//
	m_pDocument->CBaseDocument::SetFormMode(CAbstractFormDoc::BROWSE);
	
    CString strKeyDescri = m_pDocument->m_pDBTMaster->GetRecord()->GetPrimaryKeyDescription();

	if (!m_pDocument->m_pDBTMaster->FindData())
	{
		m_pDocument->m_pMessages->Add(cwsprintf
				(
					_TB("Document {0-%s} not found!"), 
					(LPCTSTR)strKeyDescri
				)
			);

		m_pDocument->ClearCurrentRecord();
		ADMGoInBrowseMode();
		m_pDocument->DisconnectFromDatabase();
		return FALSE;
    }

    // Prova a bloccare l'accesso al documento da editare (OCCHIO! solo su master)
	if (!m_pDocument->LockMaster(!m_pDocument->IsInUnattendedMode()))
	{
		ADMGoInBrowseMode();
		m_pDocument->DisconnectFromDatabase();
		return FALSE;
	}
	
	m_pDocument->SaveCurrentRecord();

	if (!m_pDocument->OnOkEdit() || !m_pDocument->m_pClientDocs->OnOkEdit())
	{
		// serve a far ricaricare i DBT delayed
		m_pDocument->DispatchPrepareAuxData();
		ADMGoInBrowseMode();	
		m_pDocument->DisconnectFromDatabase();
		return FALSE;
    }

	if (!m_pDocument->m_pDBTMaster->Edit())
	{
		ADMGoInBrowseMode();
		m_pDocument->DisconnectFromDatabase();
		return FALSE;
    }

	// mi metto in stato di edit per il successivo update e updato i controls
	m_pDocument->SetFormMode(CAbstractFormDoc::EDIT);

	// deve essere fatta prima della PrepareAuxData perche` il programmatore
	// potrebbe decidere di mettersi in modalita "modificato" per forzare eventuali
	// salvataggi di dati precompilati nelle varie OnPrepareAuxData.
	m_pDocument->SetModifiedFlag(FALSE);

	// Da una possibilita` al programmatore di personalizzare eventuali dati 
	// accessori che dipendono dalla logica della applicazione
	if (!m_pDocument->DispatchPrepareAuxData())
	{
		ADMGoInBrowseMode();
		m_pDocument->DisconnectFromDatabase();
		return FALSE;
    }

	m_pDocument->DisconnectFromDatabase();

	m_pDocument->EnableControls();
	m_pDocument->ToolBarButtonsHideGhost(0);
	//m_pDocument->UpdateDataView(); sostituita da CUpdateDataViewLevel __upd(m_pDocument);

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL ADMObj::ADMDeleteDocument()
{    
	ASSERT(m_pDocument);
	ASSERT(m_pDocument->m_pDBTMaster);

    // e` come se il documento stesse girando con un processo batch
	if (m_pDocument->m_BatchRunningArea.IsLocked())
		return FALSE;

    m_pDocument->m_bBatchRunning = TRUE;
	m_pDocument->m_BatchScheduler.Start();

	m_pDocument->ConnectToDatabase(szDeleteAction);

	if	(m_pDocument->GetFormMode() == CBaseDocument::BROWSE && !m_pDocument->m_pDBTMaster->FindData())
	{
		m_pDocument->m_pMessages->Add(cwsprintf
			(
				_TB("Document {0-%s} not found. Change not made. "), 
				(LPCTSTR)m_pDocument->m_pDBTMaster->GetRecord()->GetPrimaryKeyDescription())
			);
		ADMGoInBrowseMode();
		m_pDocument->DisconnectFromDatabase();
		return FALSE;
    }

	if	(!
			(
				m_pDocument->m_pClientDocs->OnBeforeOkDelete() &&
				m_pDocument->OnOkDelete() && m_pDocument->m_pClientDocs->OnOkDelete() && 
				m_pDocument->LockMaster(FALSE) && m_pDocument->LockDocument(TRUE)
			)
		)
	{
		ADMGoInBrowseMode();
		m_pDocument->DisconnectFromDatabase(); 
		return FALSE;
	}

	if (!m_pDocument->m_pTbContext->StartTransaction())
	{
		m_pDocument->m_pMessages->Add(_TB("Unable to open a new transaction.\r\nDocument not deleted"));
		ADMGoInBrowseMode();
		m_pDocument->DisconnectFromDatabase(); 
		return FALSE;
	}

	if (!
			(
				m_pDocument->m_pDBTMaster->Delete() && 
				m_pDocument->m_pClientDocs->OnBeforeDeleteTransaction() &&
				m_pDocument->FireBehaviour(bhe_DeleteTransaction) &&
				m_pDocument->OnDeleteTransaction() &&
				m_pDocument->m_pClientDocs->OnDeleteTransaction()
			 )
		)
	{
		m_pDocument->m_pTbContext->Rollback();	
		ADMGoInBrowseMode();
		m_pDocument->DisconnectFromDatabase(); 
		return FALSE;
	}

	m_pDocument->m_pTbContext->Commit();
	//transazione accessorie
	m_pDocument->OnExtraDeleteTransaction();
	m_pDocument->m_pClientDocs->OnExtraDeleteTransaction();
	m_pDocument->InitData();
	ADMGoInBrowseMode();
	m_pDocument->DisconnectFromDatabase();
	return TRUE;
}

// simmetria con il comportamento del documento non ADM o ADM interattivo
//-----------------------------------------------------------------------------
void ADMObj::ADMGoInBrowseMode()
{    
	// chance al programmatore di fare qualche cosa prima
	ADMOnGoInBrowseMode();

	m_pDocument->UnlockAll();
	m_pDocument->SetFormMode(CBaseDocument::BROWSE);	// back to BROWSE mode
	m_pDocument->SetModifiedFlag(FALSE);	// back to unmodified

	m_pDocument->m_BatchScheduler.Terminate();

	m_pDocument->m_bBatchRunning = FALSE;
	m_pDocument->m_BatchRunningArea.Unlock();
	m_pDocument->ToolBarButtonsHideGhost(1);
}



// non gestisce la transazione, in quanto si resta dentro a quella del
// documento chiamante
//-----------------------------------------------------------------------------
BOOL ADMObj::ADMSaveDocument()
{
	ASSERT(m_pDocument);
	ASSERT(m_pDocument->m_pDBTMaster);

	m_pDocument->DispatchOnBeforeSave();

	
	m_pDocument->ConnectToDatabase(szSaveAction);
	m_pDocument->m_pTbContext->StartTime(ONOK_TIME);
	if (
			!(
				!ADMIsAborted() &&
				ADMOnSaveDocument() &&
				m_pDocument->m_pDBTMaster->CheckTransaction() &&
				m_pDocument->m_pClientDocs->OnBeforeOkTransaction() &&
				m_pDocument->OnOkTransaction() &&
				m_pDocument->m_pClientDocs->OnOkTransaction() &&
				m_pDocument->LockDocument()
			 )
		)
	{
		// ripristina lo stato d partenza
		m_pDocument->m_pTbContext->StopTime(ONOK_TIME);
		m_pDocument->DispatchOnAfterSave();
		ADMGoInBrowseMode();
		m_pDocument->DisconnectFromDatabase();
		return FALSE;
	}

	// faccio partire la transazione
	if (!m_pDocument->m_pTbContext->StartTransaction())
	{
		m_pDocument->m_pTbContext->StopTime(ONOK_TIME);
		m_pDocument->m_pMessages->Add(_TB("Unable to open a new transaction.\r\nDocument not saved"));
		// ripristina lo stato d partenza
		m_pDocument->DispatchOnAfterSave();
		ADMGoInBrowseMode();
		m_pDocument->DisconnectFromDatabase();
		return FALSE;
	}

	m_pDocument->m_pTbContext->StopTime(ONOK_TIME);

	BOOL bOk = TRUE;
	BOOL bPrimary = TRUE;
	BOOL bSecondary = TRUE;
	TRY
	{
		switch (m_pDocument->GetFormMode())
		{
			case CBaseDocument::NEW:
			{
				// qualcuno ha gia` inserito il dato
				if (m_pDocument->m_pDBTMaster->Exist())
				{
					m_pDocument->m_pMessages->Add(cwsprintf
						(
							_TB("The document {0-%s} already exists. Insertion failed."),
							(LPCTSTR)m_pDocument->m_pDBTMaster->GetRecord()->GetPrimaryKeyDescription())
						);
					m_pDocument->m_pTbContext->Rollback();
					bOk = FALSE;
					break;
				}

				// primary transaction
				m_pDocument->m_pTbContext->StartTime(PRIMARY_TIME);
				bPrimary = m_pDocument->m_pDBTMaster->Update();
				m_pDocument->m_pTbContext->StopTime(PRIMARY_TIME);
				// secondary transaction
				m_pDocument->m_pTbContext->StartTime(SECONDARY_TIME);
				bSecondary = m_pDocument->m_pClientDocs->OnBeforeNewTransaction() &&
							 m_pDocument->FireBehaviour(bhe_NewTransaction) &&
							 m_pDocument->OnNewTransaction() &&
							 m_pDocument->m_pClientDocs->OnNewTransaction();

				if (!(bPrimary && bSecondary))
				{
					m_pDocument->m_pTbContext->Rollback();
					m_pDocument->m_pTbContext->StopTime(SECONDARY_TIME);
					bOk = FALSE;
					break;
				}

				m_pDocument->m_pTbContext->Commit();
				m_pDocument->m_pTbContext->StopTime(SECONDARY_TIME);

				m_pDocument->SaveCurrentRecord();
				//auxiliary transaction
				m_pDocument->m_pTbContext->StartTime(AUXILIARY_TIME);
				m_pDocument->OnExtraNewTransaction();
				m_pDocument->m_pClientDocs->OnExtraNewTransaction();
				m_pDocument->m_pTbContext->StopTime(AUXILIARY_TIME);
				break;
			}

			case CBaseDocument::EDIT:
			{
				m_pDocument->m_pTbContext->StartTime(PRIMARY_TIME);
				bPrimary = m_pDocument->m_pDBTMaster->Update();
				m_pDocument->m_pTbContext->StopTime(PRIMARY_TIME);

				// secondary transaction
				m_pDocument->m_pTbContext->StartTime(SECONDARY_TIME);
				bSecondary = m_pDocument->m_pClientDocs->OnBeforeEditTransaction() &&
							 m_pDocument->FireBehaviour(bhe_EditTransaction) &&
							 m_pDocument->OnEditTransaction() &&
							 m_pDocument->m_pClientDocs->OnEditTransaction();


				if (!(bPrimary && bSecondary))
				{
					m_pDocument->m_pTbContext->Rollback();
					m_pDocument->m_pTbContext->StopTime(SECONDARY_TIME);
					bOk = FALSE;
					break;
				}

				m_pDocument->m_pTbContext->Commit();
				m_pDocument->m_pTbContext->StopTime(SECONDARY_TIME);

				//auxiliary transaction
				m_pDocument->m_pTbContext->StartTime(AUXILIARY_TIME);
				m_pDocument->OnExtraEditTransaction();
				m_pDocument->m_pClientDocs->OnExtraEditTransaction();
				m_pDocument->m_pTbContext->StopTime(AUXILIARY_TIME);
				break;
			}

			default:
				ASSERT(FALSE);
				break;
		}

		m_pDocument->DispatchOnAfterSave();
	}
	CATCH(SqlException, e)
	{
		m_pDocument->m_pTbContext->StopTime(PRIMARY_TIME);
		m_pDocument->m_pTbContext->StopTime(SECONDARY_TIME);
		m_pDocument->m_pTbContext->StopTime(AUXILIARY_TIME);
		m_pDocument->m_pMessages->Add(cwsprintf(_TB("Unable to save the document due to the following exception: %s"), e->m_strError));
		m_pDocument->m_pTbContext->Rollback();
		ADMGoInBrowseMode();
		m_pDocument->DisconnectFromDatabase();
		m_pDocument->ToolBarButtonsHideGhost(1);
		return FALSE;
	}
	END_CATCH

	m_pDocument->m_pTbContext->StopTime(TOTAL_TIME);
	// ripristina lo stato d partenza
	ADMGoInBrowseMode();
	m_pDocument->DisconnectFromDatabase();
	m_pDocument->ToolBarButtonsHideGhost(1);
	return bOk;
}

//-----------------------------------------------------------------------------
BOOL ADMObj::ADMBrowseDocument()
{
	return ADMFetchDocument(FALSE);
}

// Ci si aspetta che il record del DBTMaster abbia i campi di chiame primaria
// goia` valorizzati dal chiamante per poter effettuare la corretta FindData().
// Il documento caricato con questa funzione NON deve essere salvato in alcun
// modo. Se cio` e` necessario usare l'opportuno metodo ADMEditDocument
//-----------------------------------------------------------------------------
BOOL ADMObj::ADMFetchDocument(BOOL bForEdit /*TRUE*/)
{
	ASSERT(m_pDocument);
	ASSERT(m_pDocument->m_pDBTMaster);

    // mi metto in stato di Browse e forzo il caricamento di tutti i
	// DBT anche per quelli DELAYED (EDIT e ALL)
	//
	m_pDocument->CBaseDocument::SetFormMode(CAbstractFormDoc::BROWSE);

	m_pDocument->ConnectToDatabase(szBrowseAction);
	if (!m_pDocument->m_pDBTMaster->FindData())
	{
		m_pDocument->m_pMessages->Add(_TB("Document not found!"));
		m_pDocument->DisconnectFromDatabase(); 
		return FALSE;
	}

	m_pDocument->SaveCurrentRecord();
	// Seleziona il modo EDIT per permettere alla
	// successiva OnPrepareAuxData di lavorare correttamente
	//
	if (bForEdit)
		m_pDocument->SetFormMode(CAbstractFormDoc::EDIT);
	
	m_pDocument->OnPrepareAuxData() && 
	m_pDocument->m_pClientDocs->OnPrepareAuxData();		
	
	
	
	if (bForEdit)
		m_pDocument->CBaseDocument::SetFormMode(CAbstractFormDoc::BROWSE);
	
	m_pDocument->DisconnectFromDatabase();
	
	return TRUE;
} 
	

/////////////////////////////////////////////////////////////////////////////
//								ADMFrame
/////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(ADMFrame,CMasterFrame)

//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(ADMFrame,CMasterFrame)
	//{{AFX_MSG_MAP(ADMFrame)
	ON_WM_SYSCOMMAND ()
	ON_WM_SIZE ()
	ON_WM_ACTIVATE()
	ON_WM_DESTROY()
	ON_WM_TIMER()
	ON_MESSAGE			(UM_GET_CONTROL_DESCRIPTION,	OnGetControlDescription)
	ON_MESSAGE			(UM_IS_UNATTENDED_WINDOW,		OnIsUnattendedWindow)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()


//-----------------------------------------------------------------------------
ADMFrame::ADMFrame () :
	m_pProgressBar			(NULL),
	m_pStatusBar			(NULL),
	m_bAutoProgressBar		(FALSE),
	m_pProgressBarButton	(NULL),
	m_nTimerPtr				(0),
	m_bLogButton			(FALSE)
{
	SetDockable(FALSE);	
	m_bHasToolbar = FALSE;
	//il costruttore del parent mi ha aggiunto alla catena, ma siccome io diventerò child, non deve entrare a far parte di queste logiche di finestre attive
	AfxGetThreadContext()->RemoveActiveWnd(this);
}

//-----------------------------------------------------------------------------
ADMFrame::~ADMFrame()
{
	if (m_pProgressBar) {
		SAFE_DELETE(m_pProgressBar);
	}

	if (m_pProgressBarButton) {
		SAFE_DELETE(m_pProgressBarButton);
	}
	
	if (m_pStatusBar) {
		m_pStatusBar->RedrawWindow();
	}
}

//-----------------------------------------------------------------------------
void ADMFrame::OnActivate(UINT nState, CWnd* pWndOther, BOOL bMinimized)
{
	//non devo entrare nel ciclo di attivazione della DockableFrame, perché sono child della view del documento chiamante
	//quindi non chiamo la Activate del parent
}

//-----------------------------------------------------------------------------
BOOL ADMFrame::CreateStatusBar()
{
	ASSERT(m_pStatusBar == NULL);
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL ADMFrame::PreCreateWindow(CREATESTRUCT& cs)
{
	cs.style = WS_DISABLED | WS_BORDER;
	return __super::PreCreateWindow(cs);
}

//-----------------------------------------------------------------------------
void ADMFrame::SetFrameSize(CSize csDialogSize)
{
	m_Size = csDialogSize;
}

//-----------------------------------------------------------------------------
void ADMFrame::OnSysCommand( UINT nID, LPARAM lParam )
{
}

//-----------------------------------------------------------------------------
void ADMFrame::OnDestroy()
{
	__super::OnDestroy();
	if (!m_pProgressBar) return;

	INT nMin = 0; 
	INT nMax = 0;
	m_pProgressBar->GetRange(nMin, nMax);
	m_pProgressBar->SetPos(nMax);
}

//-----------------------------------------------------------------------------
void ADMFrame::RemoveProgressBar()
{
	m_bAutoProgressBar = FALSE;
	KillTimer(m_nTimerPtr);

	if (m_pProgressBar) {
		SAFE_DELETE(m_pProgressBar);
	}

	if (m_pStatusBar) {
		m_pStatusBar->RedrawWindow();
	}
}

//-----------------------------------------------------------------------------
void ADMFrame::DisableAutoProgressBar()
{ 
	m_bAutoProgressBar = FALSE;
	KillTimer(m_nTimerPtr);
}

//-----------------------------------------------------------------------------
void ADMFrame::EnableAutoProgressBar()
{
	m_bAutoProgressBar = TRUE;
	m_nTimerPtr = SetTimer(IDT_ADM, DELAY_ADM_MS, NULL);
}

//-----------------------------------------------------------------------------
void ADMFrame::OnTimer(UINT nIDEvent)
{
	if (nIDEvent != IDT_ADM || !m_bAutoProgressBar) return;

	INT nMin = 0;
	INT nMax = 0;
	m_pProgressBar->GetRange(nMin, nMax);
	INT nStep = m_pProgressBar->GetPos();
	if (nStep < PROGRESS_BAR_MAX)
	{
		m_pProgressBar->StepIt();
	}
	else
	{
		m_pProgressBar->SetPos(nMin);
	}
	// Sleep for timing of refresh
}

//-----------------------------------------------------------------------------
void ADMFrame::SetRangeProgressBar(INT nMin, INT nMax) 
{ 
	if (m_pProgressBar == NULL) return;
	m_pProgressBar->SetRange(nMin, nMax); 
}
//-----------------------------------------------------------------------------
void ADMFrame::SetPosProgressBar(INT nPos) 
{ 
	if (m_pProgressBar == NULL) return;
	m_pProgressBar->SetPos(nPos); 
}

//-----------------------------------------------------------------------------
INT  ADMFrame::GetPosProgressBar() 
{ 
	if (m_pProgressBar == NULL) return -1;
	return m_pProgressBar->GetPos(); 
}

//-----------------------------------------------------------------------------
void ADMFrame::SetTextProgressBar(const CString& strText)
{ 
	if (m_pProgressBar == NULL) return;
	m_pProgressBar->SetText(strText); 
}

//-----------------------------------------------------------------------------
void ADMFrame::AddLogButton() 
{ 
	m_bLogButton = TRUE; 
	LogButtonAppend();
}

//-----------------------------------------------------------------------------
void ADMFrame::LogButtonAppend()
{
	if (m_bLogButton && m_pStatusBar)
	{
		// Button for Log is present ?
		CWnd* pWndCtrl = m_pStatusBar->GetDlgItem(IDC_PROGBAR_BUTTON);
		if (pWndCtrl == NULL && m_pProgressBarButton == NULL) {
			// Append the button if not present
			m_pProgressBarButton = new CTBProgressBarButton(m_pStatusBar, 0);
		}

		if (pWndCtrl)
		{
			CTBProgressBarButton* pButt = dynamic_cast<CTBProgressBarButton*>(pWndCtrl);
			if (pButt) {
				pButt->ResizeAndPosition();
			}
		}
	}
}

//-----------------------------------------------------------------------------
void ADMFrame::OnFrameCreated()
{
	__super::OnFrameCreated();

	CDocument* pActiveDocument = GetActiveDocument();
	if (pActiveDocument)
	{
		CString sTitle = pActiveDocument->GetTitle();

		CAbstractFormDoc* pAncestorDoc = ((CAbstractFormDoc*)pActiveDocument)->GetAncestor();
		CWnd* pView = NULL;
		if (pAncestorDoc && pAncestorDoc != pActiveDocument && pAncestorDoc->GetFirstView())
			pView = pAncestorDoc->GetFirstView();
		else
		{
			CWnd *pWnd = AfxGetThreadContext()->GetActiveDockableWnd();
			if (pWnd && pWnd->IsKindOf(RUNTIME_CLASS(CDockableFrame)))
				pView = ((CDockableFrame*)pWnd)->GetActiveView();
		}
		if (pView)
		{
			CFrameWnd* pFrame = pView->GetParentFrame();
			if (pFrame->IsKindOf(RUNTIME_CLASS(CAbstractFormFrame)))
			{
				// Get status Bar 
				m_pStatusBar = ((CAbstractFormFrame*)pFrame)->GetStatusBar();
				
				if (m_pStatusBar && m_pProgressBar == NULL)
				{
					// Insert ProgressBar in Pain 0
					m_pProgressBar = new  CTBStatusBarProgressBar(sTitle, PROGRESS_BAR_MAX, PROGRESS_BAR_MAX, FALSE, 0, m_pStatusBar);
					EnableAutoProgressBar();
					// Log button append to tooolBar
					// AddLogButton();
				}
			}

			CRect rcParent;
			pView->GetWindowRect(rcParent);
			SetWindowPos
				(
				NULL, 30, rcParent.Height() - m_Size.cy - 30, m_Size.cx, m_Size.cy,
				SWP_NOACTIVATE | SWP_NOZORDER | SWP_SHOWWINDOW
				);

			SetParent(pView);

		}
		else
			ShowWindow(SW_HIDE);
	}
}

//-----------------------------------------------------------------------------
BOOL ADMFrame::OnCreateClient(LPCREATESTRUCT lpcs, CCreateContext* pContext)
{
	ModifyStyle(WS_CAPTION, 0);
	return __super::OnCreateClient(lpcs, pContext);
}

//-----------------------------------------------------------------------------
LRESULT ADMFrame::OnGetControlDescription(WPARAM wParam, LPARAM lParam)
{
	//does nothing
	return (LRESULT)CWndObjDescription::GetDummyDescription();
}
//-----------------------------------------------------------------------------
LRESULT ADMFrame::OnIsUnattendedWindow(WPARAM wParam, LPARAM lParam)
{
	return (LRESULT)TRUE;
}

/////////////////////////////////////////////////////////////////////////////
//								ADMView
/////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(ADMView, CMasterFormView)

BEGIN_MESSAGE_MAP(ADMView, CMasterFormView)
	//{{AFX_MSG_MAP(ADMView)
	ON_WM_SIZE		()
	ON_WM_ERASEBKGND()
	ON_WM_CTLCOLOR	()
	ON_WM_DESTROY	()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
ADMView::ADMView()
	:
	CMasterFormView			(_T("adm"), IDD_ADM_VIEW)
{       
	SetCenterControls(FALSE);
}

	
//----------------------------------------------------------------------------
ADMView::~ADMView()
{
}

//----------------------------------------------------------------------------
void ADMView::OnInitialUpdate()
{
	ShowWindow(SW_HIDE);
}

//==============================================================================
//							AuxDataFrame (Frame senza titolo)
//==============================================================================
IMPLEMENT_DYNCREATE(AuxDataFrame, CSlaveFrame)

// Aggiunge solo il subtitle e non anche il titolo del documento
//-			----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
AuxDataFrame::AuxDataFrame()
{
	m_bHasToolbar = FALSE;
}

//-----------------------------------------------------------------------------
void AuxDataFrame::OnUpdateFrameTitle(BOOL bAddToTitle)
{
	DoUpdateFrameTitle(bAddToTitle, TRUE);
}

//-----------------------------------------------------------------------------
BOOL AuxDataFrame::CreateStatusBar()
{
	ASSERT(m_pStatusBar == NULL);
	return TRUE;
}

// Called by the framework before the creation of the Windows window attached to this CWnd object.
//-----------------------------------------------------------------------------
BOOL AuxDataFrame::PreCreateWindow(CREATESTRUCT& cs)
{
	//cs.style &= ~WS_MAXIMIZEBOX & ~WS_THICKFRAME & ~WS_VISIBLE;
	cs.style &= ~WS_MAXIMIZEBOX & ~WS_VISIBLE;
	return CSlaveFrame::PreCreateWindow(cs);
}

//==============================================================================
//	AuxDataFrameResizable
//==============================================================================

IMPLEMENT_DYNCREATE(AuxDataFrameResizable, AuxDataFrame)

//-----------------------------------------------------------------------------
BOOL AuxDataFrameResizable::PreCreateWindow(CREATESTRUCT& cs) 
{
	BOOL bOk = __super::PreCreateWindow(cs);

	cs.style		|= (WS_MAXIMIZEBOX   | WS_MINIMIZEBOX   | WS_THICKFRAME);

	return bOk;
}

//==============================================================================
//	AuxDataFrameWithBottomToolbar
//==============================================================================
IMPLEMENT_DYNCREATE(AuxDataFrameWithBottomToolbar, AuxDataFrame)

BEGIN_MESSAGE_MAP(AuxDataFrameWithBottomToolbar, AuxDataFrame)
	//{{AFX_MSG_MAP(AuxDataFrameWithBottomToolbar)
	ON_WM_SIZE()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
AuxDataFrameWithBottomToolbar::AuxDataFrameWithBottomToolbar()
	: 
	AuxDataFrame		(),
	m_pBottomToolbar	(NULL)
{
	m_bHasToolbar = TRUE;
}

//-----------------------------------------------------------------------------
AuxDataFrameWithBottomToolbar::~AuxDataFrameWithBottomToolbar()
{
	SAFE_DELETE(m_pTabbedToolBar);
}

//-----------------------------------------------------------------------------
void AuxDataFrameWithBottomToolbar::OnSize(UINT nType, int cx, int cy)
{
	__super::OnSize(nType, cx, cy);

	if (m_pTabbedToolBar)
		m_pTabbedToolBar->SetWindowPos(NULL, 0, 0, cx, m_pTabbedToolBar->CalcMaxButtonHeight(), SWP_NOMOVE | SWP_NOZORDER);
}


//------------------------------------------------------------------
void AuxDataFrameWithBottomToolbar::AdjustTabbedToolBar()
{
	if (!m_pTabbedToolBar)
		return;

	m_pTabbedToolBar->AdjustLayoutActiveTab();
}

//------------------------------------------------------------------
BOOL AuxDataFrameWithBottomToolbar::OnCustomizeJsonToolBar()
{
	return TRUE;
}

//------------------------------------------------------------------
BOOL AuxDataFrameWithBottomToolbar::OnCustomizeTabbedToolBar(CTBTabbedToolbar* pTabbedBar)
{	
	pTabbedBar->EnableDocking(this, CBRS_ALIGN_BOTTOM);

	ASSERT(m_pBottomToolbar == NULL);


	// Add New ToolBar
	m_pBottomToolbar = new CTBToolBar();
	if (!m_pBottomToolbar->CreateEmptyTabbedToolbar(this, szToolbarNameMain, _TB("Bottom")))
	{
		TRACE("Failed to create the main toolBar.\n");
		ASSERT(FALSE);
		return -1;
	}

	m_pBottomToolbar->SetBkgColor(AfxGetThemeManager()->GetDialogToolbarBkgColor());
	m_pBottomToolbar->SetForeColor(AfxGetThemeManager()->GetDialogToolbarForeColor());
	m_pBottomToolbar->SetTextColor(AfxGetThemeManager()->GetDialogToolbarTextColor());
	m_pBottomToolbar->SetTextColorHighlighted(AfxGetThemeManager()->GetDialogToolbarTextHighlightedColor());
	m_pBottomToolbar->SetHighlightedColor(AfxGetThemeManager()->GetDialogToolbarHighlightedColor());

	AddCustomButtons();
	m_pBottomToolbar->AddButtonToRight(IDCANCEL, _NS_TOOLBARBTN("Cancel"), TBIcon(szIconEscape, TOOLBAR), _TB("Cancel"));

	// aggiunge l'acceleratore di default per IDCANCEL <=> VK_ESCAPE
	m_pBottomToolbar->RemoveAccelerator(m_hAccelTable, ID_EXTDOC_ESCAPE);
	m_pBottomToolbar->AppendAccelerator(m_hAccelTable, IDCANCEL, FVIRTKEY, VK_ESCAPE);

	pTabbedBar->AddTab(m_pBottomToolbar);
	return TRUE;
}

//------------------------------------------------------------------
BOOL AuxDataFrameWithBottomToolbar::CreateAuxObjects(CCreateContext* pCreateContext)
{
	
	
	return TRUE;
}

//===========================================================================================

/////////////////////////////////////////////////////////////////////////////
//				class CAuxDataSearchFrame Implementation
/////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
IMPLEMENT_DYNCREATE(CAuxDataSearchFrame, AuxDataFrameWithBottomToolbar)

//------------------------------------------------------------------
void CAuxDataSearchFrame::AddCustomButtons()
{
	m_pBottomToolbar->AddButtonToRight(IDOK, _NS_TOOLBARBTN("Search"), TBIcon(szIconSearch, TOOLBAR), _TB("Search"));
	m_pBottomToolbar->SetDefaultAction(IDOK);

	// Impedisco alla Toolbar di essere spostata dall'utente (a causa di EnableDocking)
	m_pBottomToolbar->SetBarStyle(m_pBottomToolbar->GetBarStyle() & ~CBRS_GRIPPER);
}

//-----------------------------------------------------------------------------
void CAuxDataSearchFrame::OnAdjustFrameSize(CSize& size)
{
	size = POPUP_SIZE_ONE_MINI;
}

/////////////////////////////////////////////////////////////////////////////
//				class CAuxDataOkFrame Implementation
/////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
IMPLEMENT_DYNCREATE(CAuxDataOkFrame, AuxDataFrameWithBottomToolbar)

//------------------------------------------------------------------
void CAuxDataOkFrame::AddCustomButtons()
{
	m_pBottomToolbar->AddButtonToRight(IDOK, _NS_TOOLBARBTN("Ok"), TBIcon(szIconOk, TOOLBAR), _TB("Ok"));
	m_pBottomToolbar->SetDefaultAction(IDOK);
}

//-----------------------------------------------------------------------------
void CAuxDataOkFrame::OnAdjustFrameSize(CSize& size)
{
	size = POPUP_SIZE_ONE_MINI;
}

/////////////////////////////////////////////////////////////////////////////
//				class CAuxDataOkDoubleFrame Implementation
/////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
IMPLEMENT_DYNCREATE(CAuxDataOkDoubleFrame, CAuxDataOkFrame)

//-----------------------------------------------------------------------------
void CAuxDataOkDoubleFrame::OnAdjustFrameSize(CSize& size)
{
	size = POPUP_SIZE_TWO_MINI;
}

/////////////////////////////////////////////////////////////////////////////
//				class CAuxDataSearchDoubleFrame Implementation
/////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
IMPLEMENT_DYNCREATE(CAuxDataSearchDoubleFrame, CAuxDataSearchFrame)

//-----------------------------------------------------------------------------
void CAuxDataSearchDoubleFrame::OnAdjustFrameSize(CSize& size)
{
	size = POPUP_SIZE_TWO_MINI;
}
