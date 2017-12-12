
#include "stdafx.h"
#include "afxpriv.h"

#include <stdio.h>

#include <TbNameSolver\TbNamespaces.h>

#include <TbParser\TokensTable.h>
#include <TbParser\Parser.h>

#include <TbGenlib\generic.h>
#include <TbGenlib\FunProto.h>

#include "edtmng.h"
#include "rpsymtbl.h"
#include "disptbl.h"
#include "procdata.h"
#include "procdlg.h"

//............................... Resources
#include "edtcmm.hjson" //JSON AUTOMATIC UPDATE
#include "procdlg.hjson" //JSON AUTOMATIC UPDATE

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif

//============================================================================
// class CProcedureDlg
//============================================================================
const UINT BASED_CODE proc_indicators[] =
{
	ID_PROC_SHOW_ACTION,
	ID_PROC_SHOW_MESSAGE,
	ID_EDTCMM_EQNEDIT_ROW_COL
};
IMPLEMENT_DYNAMIC(CProcedureDlg, CParsedDialog)
BEGIN_MESSAGE_MAP(CProcedureDlg, CParsedDialog)
	//{{AFX_MSG_MAP( CProcedureDlg )
	ON_BN_CLICKED		(IDC_PROC_REN,		BtnRenProcedure)
	ON_BN_CLICKED		(IDC_PROC_DEL,		BtnDelProcedure)
	ON_BN_CLICKED		(IDC_PROC_CONF,		BtnConfProcedure)
	ON_BN_CLICKED		(IDC_PROC_CANCEL,	BtnCancProcedure)

	ON_EN_SETFOCUS		(IDC_PROC_EDIT,		SetFocusProcEdit)

	ON_CBN_EDITCHANGE	(IDC_PROC_LIST,		EditChangeProcList)
	ON_CBN_SELCHANGE	(IDC_PROC_LIST,		SelChangeProcList)
	ON_CBN_DBLCLK		(IDC_PROC_LIST,		DblClkProcList)

	ON_WM_SIZE ()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//----------------------------------------------------------------------------
CProcedureDlg::CProcedureDlg
	(
		EditorManager*	pEditorManager,
		EventsData*		pEventsData,
		ObjectsData*		pProcData,
		WoormTable* pSymTable,
		RuleDataArray*		pRuleData,
		BOOL			bCanDelRen	/* = TRUE */
	)
	:
	CParsedDialog	(IDD_PROC),
	m_pEditorManager(pEditorManager),
    m_pEventsData	(pEventsData),
    m_pProcData		(pProcData),
    m_pSymTable		(pSymTable),
    m_edtProc		(pSymTable, pRuleData),
    m_bCanDelRen	(bCanDelRen)
{
	m_bEmptyEditFlag		= TRUE;
	m_bErrorFlag			= FALSE;
	m_bModifyFlag			= FALSE;
    m_bEditProcIsModified	= FALSE;

	// make an empty template
	m_strEmptyTemplate	= _T("Begin\r\n\r\nEnd\r\n");
	m_sTitle = _TB("Procedures");
}

//----------------------------------------------------------------------------
BOOL CProcedureDlg::OnInitDialog()
{
	CParsedDialog::OnInitDialog();
	SetWindowText(m_sTitle);
	
	m_cbProcs.	SubclassDlgItem	(IDC_PROC_LIST,		this);
	m_edtProc.	SubclassEdit	(IDC_PROC_EDIT,		this);
                    
	//m_btnCancel.	SubclassDlgItem	(IDC_PROC_CANCEL,	this);
	//m_btnConfirm.	SubclassDlgItem	(IDC_PROC_CONF,		this);
	//m_btnDelete.	SubclassDlgItem	(IDC_PROC_DEL,		this);
	//m_btnRename.	SubclassDlgItem	(IDC_PROC_REN,		this);

	// update the procedure list
	for (int i = 0; i < m_pProcData->GetSize(); i++)
		m_cbProcs.AddString (m_pProcData->GetName(i));

	// build the bCheck bar
	return InitStatusBar ();	
}

// Generic procedures
// 
//----------------------------------------------------------------------------
void CProcedureDlg::EnableBtnDelRen(BOOL bEnable)
{
	// bEnable the push buttom
	//m_btnRename.	EnableWindow (bEnable && m_bCanDelRen);
	//m_btnDelete.	EnableWindow (bEnable && m_bCanDelRen);
}

//----------------------------------------------------------------------------
ObjItem* CProcedureDlg::CreateItem()
{
	ProcedureObjItem* pNewProcObj = new ProcedureObjItem(m_pSymTable);
	return pNewProcObj;
}

//----------------------------------------------------------------------------
BOOL CProcedureDlg::OkProcedure()
{
	CString	strProcName;

	// extract the procedure's strName from combo box
	if (!GetIdentifier(m_cbProcs, strProcName))
		return FALSE;
	
	if (AfxGetTokensTable()->GetKeywordsToken(strProcName) != T_NOTOKEN)
	{
		AfxMessageBox(_TB("The name collides with a reserved word of TaskBuilder"));
		m_cbProcs.SetFocus();
		return FALSE;
	}

	// extract the procedure's body from the Edit
	CString	strTmp;
	
	m_edtProc.GetWindowText (strTmp);
	ConvertCString (strTmp, CRLF_TO_LF);

	Parser	lex(strTmp);

	// the strEvent is empty, so it must deleted
	if (lex.LookAhead(T_EOF) || lex.Bad())
    	return TRUE;

	//---- create a new procedure object and parse the body, inherit procedure's prototipe
	ObjItem* pPrevObj = m_pProcData->Get (strProcName);

	ObjItem* pNewProcObj = CreateItem();

	if (pPrevObj && pPrevObj->IsKindOf(RUNTIME_CLASS(ProcedureObjItem)))
	{
		ProcedureObjItem* p = (ProcedureObjItem*)pPrevObj;			ASSERT(p->m_pProcedure);
		CFunctionDescription* fp = p->m_pProcedure->GetFun();	
		if (fp)
		{
			ProcedureObjItem* n = (ProcedureObjItem*)pNewProcObj;		ASSERT(n->m_pProcedure);
			CFunctionDescription* f = new CFunctionDescription(*fp);
			n->m_pProcedure->SetFun(f);
		}
	}

	pNewProcObj->SetName(strProcName);

	if (!pNewProcObj->Parse(lex))
	{
    	ShowError (lex);
		delete pNewProcObj;
		return FALSE;
	}
	//----

	// has been modify the document
	m_bModifyFlag = TRUE;
	m_bEditProcIsModified = FALSE;

	if (pPrevObj)
	{
		int nProcIdx = m_pProcData->GetIndex(strProcName);
		//m_pProcData->Delete		(nProcIdx);
		m_pProcData->InsertAt	(nProcIdx, pNewProcObj);
	}
	else
	{
		// add new procedure
		m_pProcData->AddNew(pNewProcObj);
        m_cbProcs.AddString(pNewProcObj->GetName());
		
		// update the bCheck bar
		ShowActionStatusBar	(strProcName, _TB("Current: {0-%s}"));
    }

	ClearErrorStatusBar ();
    return TRUE;
}

// Status Bar procedures
// 
//------------------------------------------------------------------------------
BOOL CProcedureDlg::InitStatusBar()
{
	// create the bCheck bar
	if	(
			!m_sbStatus.Create (this) ||
			!m_sbStatus.SetIndicators(proc_indicators, sizeof(proc_indicators) / sizeof(UINT))
		)
	{
		TRACE("Failed to create bCheck bar\n");
		return FALSE;
	}

	UINT	nID, nStyle;
	int		cxWidth;
	
	// set the pane's paramenters
	m_sbStatus.GetPaneInfo	(2, nID, nStyle, cxWidth);
	m_sbStatus.SetPaneInfo	(2, nID, SBPS_NORMAL, cxWidth);
	
	m_sbStatus.GetPaneInfo	(1, nID, nStyle, cxWidth);
	m_sbStatus.SetPaneInfo	(1, nID, SBPS_STRETCH | SBPS_NORMAL, cxWidth);

	m_sbStatus.GetPaneInfo	(0, nID, nStyle, cxWidth);
	m_sbStatus.SetPaneInfo	(0, nID, SBPS_NORMAL, cxWidth);

	// clear the pane
	m_sbStatus.SetPaneText	(0, NULL);
	m_sbStatus.SetPaneText	(1, NULL);
	m_sbStatus.SetPaneText	(2, NULL);

	RecalcLayout			();
	return TRUE;
}

//----------------------------------------------------------------------------
void CProcedureDlg::ShowActionStatusBar(LPCTSTR pszProcName, const CString& strAction)
{
	CString str 		(cwsprintf (strAction, pszProcName));

	if (m_bEditProcIsModified)
		str += _T("*");		// to signal some modification made
			
	m_sbStatus.SetPaneText	(0, str);
}

//----------------------------------------------------------------------------
void CProcedureDlg::ClearErrorStatusBar()
{
	m_sbStatus.	SetPaneText	(1, NULL);
	m_sbStatus.	SetPaneText	(2, NULL);
}

//----------------------------------------------------------------------------
void CProcedureDlg::ShowError(Parser& lex)
{
	// NB. lex.ShowErrors() azzera la posizione di errore ed il messaggio
	int nLine = lex.GetCurrentLine();
	int nCol = lex.GetCurrentPos();
	m_sbStatus.SetPaneText(1, lex.BuildErrMsg());
	m_sbStatus.SetPaneText(2, cwsprintf(ID_EDTCMM_EQNEDIT_ROW_COL, nLine, nCol));

	lex.ShowErrors();

	// Va fatta dopo la ShowErrors per evitare che quando il fuoco
	// ritorna all'edit tutto il testo venga selezionato
	m_edtProc.PositionCursor(nLine - 1, nCol - 1);

	m_bErrorFlag = TRUE;
}

//----------------------------------------------------------------------------
void CProcedureDlg::RecalcLayout()
{
	RepositionBars(0, 0xFFFF, AFX_IDW_PANE_FIRST);
}

//------------------------------------------------------------------------------
void CProcedureDlg::OnSize (UINT nType, int cx, int cy)
{
	CParsedDialog::OnSize (nType, cx, cy); 
	
	RecalcLayout();
}

// Edit box procedures IDC_EDIT_PROC
//
//----------------------------------------------------------------------------
void CProcedureDlg::ChangeProcEdit()
{
	if (!m_bEditProcIsModified)
	{
		CString strProcName;
		if (!GetIdentifier(m_cbProcs, strProcName)) return;
		
		// disable the combo box
		m_cbProcs.EnableWindow (FALSE);
		
	    EnableBtnDelRen	(FALSE);
	
		// bEnable the Confirm push buttom
		//m_btnConfirm.	EnableWindow ();
		//m_btnCancel.	EnableWindow ();
		
		m_bEditProcIsModified = TRUE;
		ShowActionStatusBar
			(
				strProcName,
				(m_pProcData->GetIndex (strProcName) < 0) 
					? _TB("New: {0-%s}") 
					: _TB("Current: {0-%s}")
			);
	}
}

//----------------------------------------------------------------------------
void CProcedureDlg::SetFocusProcEdit()
{
	CString strProcName;   

	if (!GetIdentifier (m_cbProcs, strProcName)) return;

	if (!m_pProcData->Exist (strProcName) && !m_bEditProcIsModified)
    {
		m_edtProc.SetWindowText (m_strEmptyTemplate);
		ShowActionStatusBar	(strProcName, _TB("New: {0-%s}"));
	}
	
	ChangeProcEdit();                      
}

//----------------------------------------------------------------------------
void CProcedureDlg::ShowProcedure (LPCTSTR pszProcName)
{
	CString	strFileName = GetTempName();
	ObjItem* pProc = m_pProcData->Get(pszProcName);

	Unparser oFile	(TRUE);
	pProc->Unparse	(oFile, TRUE);
	oFile.Close		();

	m_strLastProcEdit = oFile.GetBufferString();

	ConvertCString(m_strLastProcEdit, LF_TO_CRLF);

	m_edtProc.SetWindowText(m_strLastProcEdit);

	ShowActionStatusBar	(pszProcName, _TB("Current: {0-%s}"));
}

// Buttom procedures IDC_PROC_CANCEL, IDC_PROC_CONF, IDC_PROC_DEL, IDC_PROC_REN 
//
//----------------------------------------------------------------------------
void CProcedureDlg::BtnCancProcedure()
{
	CString strProcName;
	
	m_cbProcs.EnableWindow	();

    EnableBtnDelRen	(TRUE);

	//m_btnConfirm.	EnableWindow	(FALSE);
	//m_btnCancel.	EnableWindow	(FALSE);

	m_cbProcs.GetWindowText	(strProcName);                           
	
	BOOL oldProc = m_pProcData->Exist (strProcName);
	
	m_edtProc.SetWindowText(oldProc ? m_strLastProcEdit : m_strEmptyTemplate);	

    m_bEditProcIsModified = FALSE;
	ShowActionStatusBar	(strProcName, (oldProc ? _TB("Current: {0-%s}") : _TB("New: {0-%s}")));
	ClearErrorStatusBar	();
}

//----------------------------------------------------------------------------
void CProcedureDlg::BtnConfProcedure()
{
	// check and add the procedure's body
	if (OkProcedure ())
	{
		// bEnable the Proc List and the realted buttom
		m_cbProcs.	EnableWindow();

	    EnableBtnDelRen	(TRUE);

		// disable the Confirm push buttom
		//m_btnConfirm.	EnableWindow	(FALSE);
		//m_btnCancel.	EnableWindow	(FALSE);
	}
}

//----------------------------------------------------------------------------
void CProcedureDlg::BtnDelProcedure()
{
	CString	strProcName;
	m_cbProcs.GetWindowText (strProcName);
	
	int nProcIdx = m_cbProcs.FindString(-1, strProcName);
	m_cbProcs.GetLBText (nProcIdx, strProcName);

	CDeleteProcedureDlg	delProcDlg (m_pEventsData, m_pProcData, strProcName);
	if (delProcDlg.DoModal () == IDOK)
	{
		m_cbProcs.DeleteString 	(nProcIdx);
		m_cbProcs.SetWindowText	(_T(""));
		m_cbProcs.SetFocus			();

		m_edtProc.SetWindowText	(_T(""));

	    EnableBtnDelRen	(FALSE);
		
		m_sbStatus.SetPaneText(0, NULL);
		ClearErrorStatusBar();
		
		if (m_pEditorManager)
			m_pEditorManager->SetModifiedFlag ();
	}
}

//----------------------------------------------------------------------------
void CProcedureDlg::BtnRenProcedure()
{
	CString strOldName;
	CString	strNewName;
	
	m_cbProcs.GetWindowText(strOldName);
	
	int nProcIdx = m_cbProcs.FindString(-1, strOldName);
	m_cbProcs.GetLBText(nProcIdx, strOldName);

	RenameProcedureDlg	renProcDlg (m_pEventsData, m_pProcData, strOldName, &strNewName);
	if (renProcDlg.DoModal () == IDOK)
	{
		m_cbProcs.DeleteString	(nProcIdx);
		m_cbProcs.AddString 	(strNewName);
		m_cbProcs.SetWindowText(strNewName);

		ShowActionStatusBar(strNewName, _TB("Current: {0-%s}"));
		
		if (m_pEditorManager)
			m_pEditorManager->SetModifiedFlag ();
	}
}

// Combo Box procedures IDC_PROC_LIST
//
//----------------------------------------------------------------------------
void CProcedureDlg::DblClkProcList()
{
	if (m_cbProcs.GetCurSel() == CB_ERR)
		return;

	CString strProcName;

	m_cbProcs.GetLBText (m_cbProcs.GetCurSel(), strProcName);

	CShowProcEventsDlg showProcEventsDlg	(m_pEventsData, strProcName);
	showProcEventsDlg.DoModal			();
}

//----------------------------------------------------------------------------
void CProcedureDlg::EditChangeProcList()
{
	CString strName = GetIdentifier(m_cbProcs);
	   
	int	nCurrSel;

	if ((nCurrSel = m_cbProcs.FindString(-1, strName)) != CB_ERR)
	{
		m_cbProcs.GetLBText(nCurrSel, strName);
	    ShowProcedure		(strName);
	    EnableBtnDelRen		(TRUE);
		ShowActionStatusBar (strName, _TB("Current: {0-%s}"));
		
	    m_bEmptyEditFlag = FALSE;
	    return;
	}

	ShowActionStatusBar (strName, _TB("No selection"));
	
    if (!m_bEmptyEditFlag)
    {
    	EnableBtnDelRen	(FALSE);
		m_edtProc.SetWindowText(_T(""));
		
		m_bEmptyEditFlag = TRUE;
	}
}

//----------------------------------------------------------------------------
void CProcedureDlg::SelChangeProcList()
{
	if (m_cbProcs.GetCurSel() == CB_ERR)
		return;

	CString strProcName;

	m_cbProcs.GetLBText(m_cbProcs.GetCurSel(), strProcName);
	ShowProcedure		(strProcName);
	EnableBtnDelRen		(TRUE);

    m_bEmptyEditFlag = FALSE;
}

// AFX_MSG procedures
//
//----------------------------------------------------------------------------
void CProcedureDlg::OnOK()
{
	// the procedure's body is modified, must save the new body
	if (!m_cbProcs.IsWindowEnabled() && !OkProcedure ())
		return;

	if (m_bModifyFlag && m_pEditorManager)
		m_pEditorManager->SetModifiedFlag();

	m_edtProc.SetDlgStatusBar(NULL);
	EndDialog (IDOK);
}

//----------------------------------------------------------------------------
void CProcedureDlg::OnCancel()
{
	m_edtProc.SetDlgStatusBar(NULL);
	EndDialog (IDCANCEL);
}

//============================================================================
// CQueriesDlg
//----------------------------------------------------------------------------
CQueriesDlg::CQueriesDlg
	(
		EditorManager*	pEditorManager,
		EventsData*		pEventsData,
		ObjectsData*	pProcData,
		WoormTable* pSymTable,
		RuleDataArray*		pRuleData,
		BOOL			bCanDelRen/* = TRUE*/
	)
	: 
	CProcedureDlg(pEditorManager, pEventsData, pProcData, pSymTable, pRuleData, bCanDelRen)
{
	m_sTitle = _TB("Queries");
	m_strEmptyTemplate	= _T("Begin\r\n\t{\r\n\t\tSELECT column1 {COL w_field} FROM table WHERE column2 = {IN w_filter}\r\n\t}\r\nEnd\r\n");
}

//----------------------------------------------------------------------------
ObjItem* CQueriesDlg::CreateItem()
{
	QueryObjItem* pNewProcObj = new QueryObjItem(m_pSymTable, L"");
	return pNewProcObj;
}

//============================================================================
// CProcedureEventDlg
IMPLEMENT_DYNAMIC(CProcedureEventDlg, CParsedDialog)
//----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CProcedureEventDlg, CParsedDialog)
	//{{AFX_MSG_MAP( CProcedureEventDlg )
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

CProcedureEventDlg::CProcedureEventDlg
	(
		int				nDlgIndex,
		EventsData*		pEventsData,
		LPCTSTR			pszProcName
	)
	:
	CParsedDialog		(nDlgIndex),
    m_pEventsData		(pEventsData),
	m_strCurrProcName	(pszProcName)
{}

//----------------------------------------------------------------------------
BOOL CProcedureEventDlg::OnInitDialog()
{
	CParsedDialog::OnInitDialog();
	
	SetDlgItemText(IDC_PROC_CURRENT_NAME, m_strCurrProcName);

	//m_lbTriggEvents.		SubclassDlgItem	(IDC_PROC_TRIGGEVN_LIST,		this);
	//m_lbTableActions.		SubclassDlgItem	(IDC_PROC_TABLEACT_LIST,		this);
	//m_btnReport.			SubclassDlgItem	(IDC_PROC_EVENT_REPORT,			this);
	//m_btnNewPage.			SubclassDlgItem	(IDC_PROC_EVENT_NEWPAGE,		this);

	UpdateTriggEventsList	();
	UpdateTableActionsList	();
	UpdatePredefineEvent	();

	EnableAllItemDlg		();
	
	return TRUE;
}

//----------------------------------------------------------------------------
void CProcedureEventDlg::UpdateTriggEventsList()
{
	for (int nIdx = 0; nIdx < m_pEventsData->GetTriggEventsSize(); nIdx++)
	{
		CString strEvent;
		if (m_pEventsData->SearchTriggEventsNamedAction (nIdx, m_strCurrProcName, strEvent, ACT_CALL))
			m_lbTriggEvents.AddString (strEvent);
	}
}

//----------------------------------------------------------------------------
void CProcedureEventDlg::UpdateTableActionsList()
{
	for (int nIdx = 0; nIdx < m_pEventsData->GetTableActionsSize(); nIdx++)
	{
		CString strEvent;
		if (m_pEventsData->SearchTableActionsNamedAction (nIdx, m_strCurrProcName, strEvent, ACT_CALL))
			m_lbTableActions.AddString (strEvent);
	}
}

//----------------------------------------------------------------------------
void CProcedureEventDlg::UpdatePredefineEvent()
{
	m_bIsEventNewPage	= m_pEventsData->SearchNewPageNamedAction	(m_strCurrProcName, ACT_CALL);
	m_bIsEventReport	= m_pEventsData->SearchReportNamedAction	(m_strCurrProcName, ACT_CALL);
}

//----------------------------------------------------------------------------
void CProcedureEventDlg::SetStdEventCheckBox(BOOL bCheck)
{
	if (m_bIsEventReport)
		m_btnReport.SetCheck(bCheck);
		
	if (m_bIsEventNewPage)
		m_btnNewPage.SetCheck(bCheck);
}

//----------------------------------------------------------------------------
void CProcedureEventDlg::EnableAllItemDlg(BOOL bEnable)
{
	m_lbTriggEvents.	EnableWindow(m_lbTriggEvents.GetCount() > 0 && bEnable);
	m_lbTableActions.	EnableWindow(m_lbTableActions.GetCount() > 0 && bEnable);
	m_btnNewPage.		EnableWindow(m_bIsEventNewPage && bEnable);
	m_btnReport.		EnableWindow(m_bIsEventReport && bEnable);
}

//============================================================================
// class CShowProcEventsDlg implementation
//============================================================================

//----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CShowProcEventsDlg, CProcedureEventDlg)
	//{{AFX_MSG_MAP( CShowProcEventsDlg )
	ON_BN_CLICKED	(IDC_PROC_EVENT_REPORT,		OnStdEventClicked)
	ON_BN_CLICKED	(IDC_PROC_EVENT_NEWPAGE,	OnStdEventClicked)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//----------------------------------------------------------------------------
CShowProcEventsDlg::CShowProcEventsDlg
	(
		EventsData*		pEventsData,
		LPCTSTR	pszProcName
	)
	:
	CProcedureEventDlg	(IDD_PROC_SHOW_EVENTS, pEventsData, pszProcName)
{}

//----------------------------------------------------------------------------
BOOL CShowProcEventsDlg::OnInitDialog ()
{
	CProcedureEventDlg::OnInitDialog ();
	
	SetStdEventCheckBox();
	
	return TRUE;
}

//----------------------------------------------------------------------------
void CShowProcEventsDlg::OnOK ()
{
	EndDialog (IDOK);
}

//----------------------------------------------------------------------------
void CShowProcEventsDlg::OnStdEventClicked ()
{
	SetStdEventCheckBox();
}

//============================================================================
// ModifyProcedureDlg
//----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(ModifyProcedureDlg, CProcedureEventDlg)
	//{{AFX_MSG_MAP( ModifyProcedureDlg )
	ON_BN_CLICKED	(IDC_PROC_TRIGGEVN_SEL_ALL,		SelAllTriggEvn)
	ON_BN_CLICKED	(IDC_PROC_TRIGGEVN_DESEL_ALL,	DeselAllTriggEvn)
	ON_LBN_DBLCLK	(IDC_PROC_TRIGGEVN_LIST,		DblClkTriggEvn)

	ON_BN_CLICKED	(IDC_PROC_TABLEACT_SEL_ALL,		SelAllTableAct)
	ON_BN_CLICKED	(IDC_PROC_TABLEACT_DESEL_ALL,	DeselAllTableAct)
	ON_LBN_DBLCLK	(IDC_PROC_TABLEACT_LIST,		DblClkTableAct)

	ON_BN_CLICKED	(IDC_PROC_SEL_ALL,				SelectAll)
	ON_BN_CLICKED	(IDC_PROC_DESEL_ALL,			DeselectAll)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//----------------------------------------------------------------------------
ModifyProcedureDlg::ModifyProcedureDlg
	(
		int				nDlgIndex,
		ObjectsData*	pProcData,
		EventsData*		pEventsData,
		LPCTSTR			pszProcName
	)
	:
	CProcedureEventDlg	(nDlgIndex, pEventsData, pszProcName),
	m_pProcData			(pProcData)
{}

//----------------------------------------------------------------------------
BOOL ModifyProcedureDlg::OnInitDialog()
{
	CProcedureEventDlg::OnInitDialog();
	
	m_btnSelAllTriggEvn.	SubclassDlgItem	(IDC_PROC_TRIGGEVN_SEL_ALL,		this);
	m_btnDeselAllTriggEvn.	SubclassDlgItem	(IDC_PROC_TRIGGEVN_DESEL_ALL,	this);

	m_btnSelectAll.			SubclassDlgItem	(IDC_PROC_SEL_ALL,				this);
	m_btnDeselectAll.		SubclassDlgItem	(IDC_PROC_DESEL_ALL,			this);

	m_btnSelAllTableAct.	SubclassDlgItem	(IDC_PROC_TABLEACT_SEL_ALL,		this);
	m_btnDeselAllTableAct.	SubclassDlgItem	(IDC_PROC_TABLEACT_DESEL_ALL,	this);

	return TRUE;
}

//----------------------------------------------------------------------------
void ModifyProcedureDlg::SelectAllList(CListBox& listBox, BOOL bSelectAll)
{
	for (int i = 0; i < listBox.GetCount(); i++)
		listBox.SetSel (i, bSelectAll);
}

//----------------------------------------------------------------------------
void ModifyProcedureDlg::SelAllTableAct()
{
	SelectAllList (m_lbTableActions);
}

//----------------------------------------------------------------------------
void ModifyProcedureDlg::DeselAllTableAct()
{
	SelectAllList (m_lbTableActions, FALSE);
}

//----------------------------------------------------------------------------
void ModifyProcedureDlg::DblClkTableAct()
{
	m_lbTableActions.SetSel(m_lbTableActions.GetCaretIndex(), FALSE);
}

//----------------------------------------------------------------------------
void ModifyProcedureDlg::SelAllTriggEvn()
{
	SelectAllList (m_lbTriggEvents);
}

//----------------------------------------------------------------------------
void ModifyProcedureDlg::DeselAllTriggEvn()
{
	SelectAllList (m_lbTriggEvents, FALSE);
}

//----------------------------------------------------------------------------
void ModifyProcedureDlg::DblClkTriggEvn()
{
	m_lbTriggEvents.SetSel (m_lbTriggEvents.GetCaretIndex (), FALSE);
}

//----------------------------------------------------------------------------
void ModifyProcedureDlg::DeselectAll ()
{
	SelectAllList		(m_lbTriggEvents,		FALSE);
	SelectAllList		(m_lbTableActions,	FALSE);
	SetStdEventCheckBox	(FALSE);
}

//----------------------------------------------------------------------------
void ModifyProcedureDlg::SelectAll ()
{
	SelectAllList		(m_lbTriggEvents);
	SelectAllList		(m_lbTableActions);
	SetStdEventCheckBox	();
}

//----------------------------------------------------------------------------
BOOL ModifyProcedureDlg::GetItemSelect (CStringArray& itemArray, CListBox& listBox)
{
	CString strEvent;
	int*	pIndexSel;
	
	int nSelItems = m_lbTriggEvents.GetSelCount();
   	if (nSelItems <= 0)
   		return FALSE;

	pIndexSel = new int[nSelItems];
	listBox.GetSelItems (nSelItems, pIndexSel);

	for (int i = nSelItems - 1; i >= 0; i--)
	{
		CString strName;

		listBox.GetText	(pIndexSel[i], strName);
		itemArray.Add	(strName);
	}

	delete [] pIndexSel;
	return TRUE;
}

//----------------------------------------------------------------------------
BOOL ModifyProcedureDlg::GetTriggEventsSelect (CStringArray& aTriggEventArray)
{
	return GetItemSelect (aTriggEventArray, m_lbTriggEvents);
}

//----------------------------------------------------------------------------
BOOL ModifyProcedureDlg::GetTableActionsSelect (CStringArray& aTableActionsArray)
{
	return GetItemSelect (aTableActionsArray, m_lbTableActions);
}

//----------------------------------------------------------------------------
void ModifyProcedureDlg::EnableAllItemDlg(BOOL bEnable)
{
	CProcedureEventDlg::EnableAllItemDlg(bEnable);
	
	if (m_lbTriggEvents.GetCount() > 0)
	{
		m_btnSelAllTriggEvn.	EnableWindow (bEnable);
		m_btnDeselAllTriggEvn.	EnableWindow (bEnable);
	}
	
	if (m_lbTableActions.GetCount() > 0)
	{
		m_btnSelAllTableAct.	EnableWindow (bEnable);
		m_btnDeselAllTableAct.	EnableWindow (bEnable);
	}
	BOOL bAllEnabled =
			m_lbTriggEvents.GetCount ()	||  
			m_lbTableActions.GetCount ()	||
			m_bIsEventNewPage				||	
			m_bIsEventReport;

	if (bAllEnabled)
	{
		m_btnSelectAll.		EnableWindow (bEnable);
		m_btnDeselectAll.	EnableWindow (bEnable);
	}	
}

//============================================================================
// RenameProcedureDlg
//============================================================================
BEGIN_MESSAGE_MAP(RenameProcedureDlg, ModifyProcedureDlg)
	//{{AFX_MSG_MAP( RenameProcedureDlg )
	ON_EN_CHANGE (IDC_PROC_NEW_NAME, ModifyNewName)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//----------------------------------------------------------------------------
RenameProcedureDlg::RenameProcedureDlg
	(
		EventsData*		pEventsData,
		ObjectsData*	pProcData,
		LPCTSTR			pszProcName,
		CString*		pstrNewName
	)
	:
	ModifyProcedureDlg	(IDD_PROC_RENAME, pProcData, pEventsData, pszProcName),
	m_pNewName			(pstrNewName),
	m_bItemsEnabled		(FALSE),
	m_edtNewName		(NULL)
{}

//----------------------------------------------------------------------------
BOOL RenameProcedureDlg::OnInitDialog()
{
	ModifyProcedureDlg::OnInitDialog();
	
	EnableAllItemDlg (FALSE);
	
	m_edtNewName.SubclassEdit(IDC_PROC_NEW_NAME, this);
	
	return TRUE;
}

//----------------------------------------------------------------------------
void RenameProcedureDlg::ModifyNewName ()
{
	if (!m_bItemsEnabled)
	{
		EnableAllItemDlg (TRUE);
		m_bItemsEnabled = TRUE;
	}
}

//----------------------------------------------------------------------------
void RenameProcedureDlg::OnOK()
{                      
	if (!CheckForm()) 
		return;
	
	CStringArray	triggEvnArray;
	CStringArray	tableActArray;

	CString strNewName;
	m_edtNewName.GetValue (strNewName);
	
	if (AfxGetTokensTable()->GetKeywordsToken(strNewName) != T_NOTOKEN)
	{
		AfxMessageBox(_TB("The name collides with a reserved word of TaskBuilder"));
		m_edtNewName.SetFocus();
		return;
	}

	if (m_pProcData->Exist(strNewName))
	{
		// the procedure's strName is just exist 
		AfxMessageBox
		(
			cwsprintf(_TB("The \"{0-%s}\" procedure already exists. \r\nType a new name."), (LPCTSTR) strNewName), 
			MB_OK | MB_ICONINFORMATION
		);
		m_edtNewName.SetFocus();
		return;
    }	

	// rename the procedure with new strName
	m_pProcData->Rename(m_strCurrProcName, strNewName);

	// settare il nome all'interno dell listbox
	if (GetTriggEventsSelect(triggEvnArray))
	{
		// update all strEvent selectes
		for (int nIdx=0; nIdx < triggEvnArray.GetSize(); nIdx++)
			m_pEventsData->RenameTriggEventsNamedAction
				(
					triggEvnArray[nIdx],
					m_strCurrProcName,
					strNewName,
					ACT_CALL
				);
	}	

	if (GetTableActionsSelect(tableActArray))
	{
		// update all strEvent selectes
		for (int nIdx = 0; nIdx < tableActArray.GetSize(); nIdx++)
			m_pEventsData->RenameTableActionsNamedAction
				(
					tableActArray[nIdx],
					m_strCurrProcName,
					strNewName,
					ACT_CALL
				);
	}	
	if (CheckBtnNewPage ())
		m_pEventsData->RenameNewPageNamedAction(m_strCurrProcName, strNewName, ACT_CALL);

	if (CheckBtnReport ())
		m_pEventsData->RenameReportNamedAction(m_strCurrProcName, strNewName, ACT_CALL);

	*m_pNewName = strNewName;
	EndDialog (IDOK);
}

//============================================================================
// class CDeleteProcedureDlg
//============================================================================

//----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CDeleteProcedureDlg, ModifyProcedureDlg)
	//{{AFX_MSG_MAP( CDeleteProcedureDlg )
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//----------------------------------------------------------------------------
CDeleteProcedureDlg::CDeleteProcedureDlg
	(
		EventsData*		pEventsData,
		ObjectsData*	pProcData,
		LPCTSTR			pszProcName
	)
	:
	ModifyProcedureDlg	(IDD_PROC_DELETE, pProcData, pEventsData, pszProcName)
{}

//----------------------------------------------------------------------------
BOOL CDeleteProcedureDlg::OnInitDialog ()
{
	ModifyProcedureDlg::OnInitDialog ();
	
	EnableAllItemDlg (TRUE);
	
	return TRUE;
}

//----------------------------------------------------------------------------
void CDeleteProcedureDlg::OnOK()
{
    CString msg = cwsprintf(_TB("Do you want delete to the '{0-%s}' procedure?"), (LPCTSTR) m_strCurrProcName);
	if (AfxMessageBox(msg, MB_YESNO | MB_ICONEXCLAMATION) != IDYES)
		return;

	// delete the procedure
	m_pProcData->Delete (m_strCurrProcName);
	
	CStringArray	triggEvnArray;
	CStringArray	tableActArray;
	
	// settare il nome all'interno dell listbox
	if (GetTriggEventsSelect(triggEvnArray))
	{
		// update all strEvent selectes
		for (int nIdx = 0; nIdx < triggEvnArray.GetSize(); nIdx++)
			m_pEventsData->DeleteTriggEventsNamedAction
				(
					triggEvnArray[nIdx],
					m_strCurrProcName,
					ACT_CALL
				);
	}	
	if (GetTableActionsSelect(tableActArray))
	{
		// update all strEvent selectes
		for (int nIdx = 0; nIdx < tableActArray.GetSize(); nIdx++)
			m_pEventsData->DeleteTableActionsNamedAction
				(
					tableActArray[nIdx],
					m_strCurrProcName,
					ACT_CALL
				);
	}	
	if (CheckBtnNewPage ())
		m_pEventsData->DeleteNewPageNamedAction (m_strCurrProcName, ACT_CALL);

	if (CheckBtnReport ())
		m_pEventsData->DeleteReportNamedAction (m_strCurrProcName, ACT_CALL);
		
	EndDialog (IDOK);
}

