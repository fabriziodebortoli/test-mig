
#include "stdafx.h"

// local declarations
#include <TbGeneric\GeneralFunctions.h>

#include "batchdlg.h"
#include "baseapp.h"
                     
// resources                     
#include "extres.hjson" //JSON AUTOMATIC UPDATE


//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif

/////////////////////////////////////////////////////////////////////////////
// 				class CBatchDialog Implementation
/////////////////////////////////////////////////////////////////////////////
//----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CBatchDialog, CParsedDialog)
//----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CBatchDialog, CParsedDialog)
	//{{AFX_MSG_MAP( CBatchDialog )
	ON_BN_CLICKED		(IDC_BATCHDLG_START_STOP,	OnBatchStartStop)
	ON_BN_CLICKED		(IDC_BATCHDLG_PAUSE_RESUME,	OnBatchPauseResume)
	ON_BN_CLICKED		(IDC_BATCHDLG_CLOSE,		OnCancel)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//----------------------------------------------------------------------------
CBatchDialog::CBatchDialog(UINT nIdd, CWnd* pWndParent/* = NULL*/)
	:
	CParsedDialog(nIdd,pWndParent)
{
	m_bBatchRunning = FALSE;
}

//----------------------------------------------------------------------------
BOOL CBatchDialog::OnInitDialog()
{
	CParsedDialog::OnInitDialog();

    SetBatchButtonState();
    
	return FALSE;
}

//-----------------------------------------------------------------------------
void CBatchDialog::SetBatchButtonState()
{
	CWnd* pStartStop = GetDlgItem(IDC_BATCHDLG_START_STOP);
	if (pStartStop)	
		pStartStop->SetWindowText(cwsprintf(m_bBatchRunning ? _TB("&Interrupt\r\n") : _TB("&Start")));
		
	CWnd* pPauseResume = GetDlgItem(IDC_BATCHDLG_PAUSE_RESUME);
	if (pPauseResume)	
	{
		pPauseResume->EnableWindow(m_bBatchRunning);
		pPauseResume->SetWindowText(cwsprintf(m_BatchScheduler.IsPaused() ? _TB("&Continue") : _TB("&Pause")));
	}

	CWnd* pClose = GetDlgItem(IDC_BATCHDLG_CLOSE);
	if (pClose)	
		pClose->EnableWindow(!m_bBatchRunning);
}

//----------------------------------------------------------------------------
void CBatchDialog::EnableControls()
{
	SetBatchButtonState();

	CWnd* pStartStop = GetDlgItem(IDC_BATCHDLG_START_STOP);
	if (pStartStop)	
		pStartStop->SetFocus();
}

//-----------------------------------------------------------------------------
void CBatchDialog::BatchStart()
{
    // Utilizza una critical area per evitare problemi multitasking
	if (!m_BatchRunningArea.IsLocked())
	{	
		m_bBatchRunning = TRUE;
		EnableControls();
		m_BatchScheduler.Start();
		
		// Implementation dependent code
		OnBatchExecute();

		m_BatchScheduler.Terminate();
		m_bBatchRunning = FALSE;
		EnableControls();
		
		m_BatchRunningArea.Unlock();
	}
}

//-----------------------------------------------------------------------------
void CBatchDialog::BatchStop ()
{   
	if (!m_bBatchRunning) 
		return;

	if (!m_BatchScheduler.IsPaused())
		if (m_BatchScheduler.IsAborted()) return;
	
	if (AfxMessageBox(_TB("Processing...\r\nAre you sure you want to abort it?"), MB_YESNO | MB_DEFBUTTON2) == IDYES)	
	{
		m_BatchScheduler.Terminate();
		m_bBatchRunning = FALSE;
	}
}

//-----------------------------------------------------------------------------
void CBatchDialog::OnBatchStartStop ()
{
    // Se sta runnando il processo batch lo ferma altrimenti lo lancia
	if (m_bBatchRunning) 
		BatchStop();
	else
		BatchStart();
}

//-----------------------------------------------------------------------------
void CBatchDialog::OnBatchPauseResume ()
{
	if (!m_bBatchRunning) 
		return;

	if (m_BatchScheduler.IsPaused()) 
		m_BatchScheduler.Resume();
	else
		m_BatchScheduler.Pause();
	
	SetBatchButtonState();
}

//------------------------------------------------------------------------------
void CBatchDialog::OnOK()
{
	OnBatchStartStop();
}

//------------------------------------------------------------------------------
void CBatchDialog::OnCancel()
{
	if (m_bBatchRunning)
		AfxMessageBox(_TB("Processing...\r\nYou cannot quit without aborting it."));
	else
	{
		OnCloseDialog();
		EndDialog(IDCANCEL);
	}
}

//--------------------------------------------------------------------------
BOOL CBatchDialog::PreTranslateMessage(MSG* pMsg)
{
	if	(pMsg->wParam == VK_ESCAPE)
	{
		BatchStop();
		return TRUE;
	}

	return CParsedDialog::PreTranslateMessage(pMsg);
}

