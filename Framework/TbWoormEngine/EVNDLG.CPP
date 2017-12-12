
#include "stdafx.h"

#include <stdio.h>

#include <TbNameSolver\TBNamespaces.h>

#include <TbParser\TokensTable.h>
#include <TbParser\Parser.h>

#include <TbGenlib\generic.h>
#include <TbGenlib\FunProto.h>

#include "procdata.h"
#include "disptbl.h"
#include "edtmng.h"
#include "events.h"
#include "procdlg.h"
#include "rpsymdlg.h"
#include "rpsymtbl.h"
#include "evndlg.h"
#include "reptable.h"

#include "INPUTMNG.hjson" //JSON AUTOMATIC UPDATE
#include "edtcmm.hjson" //JSON AUTOMATIC UPDATE
#include "evndlg.hjson" //JSON AUTOMATIC UPDATE

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif

const UINT BASED_CODE EventIndicators[] =
{
	ID_EVNDLG_EVENT_MESSAGE,
	ID_EDTCMM_EQNEDIT_ROW_COL
};

enum EReportEvents
{
	JUMP_PAGE,
	REPORT,
	TABLE_EVENT
};

CString EventDescription(EReportEvents e)
{
	switch(e)
	{
		case TABLE_EVENT	: return _TB("Fill {0-%s}");
		case JUMP_PAGE		: return _TB("Form Feed");
		case REPORT			: return _TB("Report");
	}

	ASSERT(FALSE);
	return _T("");
}

//===========================================================================
// CEventDlg
//===========================================================================
IMPLEMENT_DYNAMIC(CEventDlg, CParsedDialog)
BEGIN_MESSAGE_MAP(CEventDlg, CParsedDialog)
	//{{AFX_MSG_MAP(CEventDlg)
	ON_LBN_SELCHANGE	(IDC_EVNDLG_CUSTOM_EVENT_LIST,	OnShowCustomEventActions)
	ON_LBN_SETFOCUS		(IDC_EVNDLG_CUSTOM_EVENT_LIST,	OnCustomEventSetFocus)
	ON_BN_CLICKED		(IDC_EVNDLG_CUSTOM_EVENT_UP,	OnCustomEventUp)
	ON_BN_CLICKED		(IDC_EVNDLG_CUSTOM_EVENT_DOWN,	OnCustomEventDown)
	ON_BN_CLICKED		(IDC_EVNDLG_ADD_CUSTOM_EVENT,	OnAddCustomEvent)
	ON_BN_CLICKED		(IDC_EVNDLG_DEL_CUSTOM_EVENT,	OnDelCustomEvent)

	ON_LBN_SELCHANGE	(IDC_EVNDLG_PREDEF_EVENT_LIST,	OnShowPredefEventActions)
	ON_LBN_SETFOCUS		(IDC_EVNDLG_PREDEF_EVENT_LIST,	OnPredefEventSetFocus)

	ON_BN_CLICKED		(IDC_EVNDLG_UNDO_EVENT,			OnShowCurrentEvent)
	ON_BN_CLICKED		(IDC_EVNDLG_CLEAR_ACTIONS,		OnClearActions)

	ON_BN_CLICKED		(IDC_EVNDLG_PROCEDURE,			OnCallProcedureDlg)
	ON_BN_CLICKED		(IDC_EVNDLG_NEW_FIELD,			OnCallAddFieldDlg)

	ON_EN_CHANGE		(IDC_EVNDLG_EVENT_EDIT,			OnCodeModified)

	ON_WM_SIZE ()

	ON_BN_CLICKED		(IDC_EVNDLG_RAD_CUSTOMIZE,			OnClickRadio)
	ON_BN_CLICKED		(IDC_EVNDLG_RAD_DEFAULT,			OnClickRadio)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//----------------------------------------------------------------------------
CEventDlg::CEventDlg
	(
		EditorManager*	pEditorManager,
		EventsData*	 	pEventData,
		ProcedureData*	pProcedureData,
		WoormTable* pSymTable,
		RuleDataArray*		pRuleData,
		DisplayTables*	pDispTable
	)
	:
	CParsedDialog		(IDD_EVNDLG_EVENT),
	m_pEditorManager	(pEditorManager),
	m_pEventData		(pEventData),
	m_pProcData			(pProcedureData),
	m_pSymTable			(pSymTable),
	m_pRuleData			(pRuleData),
	m_pDispTable		(pDispTable),
	m_CurEventType		(NONE_ACTION),
	m_bStatusNew		(FALSE),
	m_edtEventActions	(pSymTable, pRuleData),
	m_bModified			(FALSE),
	m_nOtherEventsCurSel(-1)
{}

//------------------------------------------------------------------------------
void CEventDlg::InitStatusBar()
{
	if	(
			!m_sbStatus.Create(this) || 
			!m_sbStatus.SetIndicators(EventIndicators, sizeof(EventIndicators) / sizeof(UINT))
		)
	{
		TRACE("Failed to create status bar\n");
		return;
	}

	UINT	nID, nStyle;
	int		cxWidth;
	
	m_sbStatus.GetPaneInfo(1, nID, nStyle, cxWidth);
	m_sbStatus.SetPaneInfo(1, nID, SBPS_NORMAL, cxWidth);

	m_sbStatus.GetPaneInfo(0, nID, nStyle, cxWidth);
	m_sbStatus.SetPaneInfo(0, nID, SBPS_STRETCH|SBPS_NORMAL, cxWidth);

	m_sbStatus.SetPaneText(0, NULL);
	m_sbStatus.SetPaneText(1, NULL);

	RecalcLayout	();
}

//----------------------------------------------------------------------------
void CEventDlg::RecalcLayout()
{
	RepositionBars(0, 0xFFFF, AFX_IDW_PANE_FIRST);
}

//------------------------------------------------------------------------------
void CEventDlg::OnSize (UINT nType, int cx, int cy)
{
	CParsedDialog::OnSize (nType, cx, cy); 
	
	RecalcLayout();
}

//----------------------------------------------------------------------------
CString	CEventDlg::GetCurrTableName	()
{
	int i = m_lbPredefEvents.GetCurSel();
	return m_pDispTable->GetNameFromIdx(HIWORD(m_lbPredefEvents.GetItemData(i)));
}

//----------------------------------------------------------------------------
void CEventDlg::ShowPrototype()
{
	CString	tColon, tDo, tBegin, tEnd;
	CString tBefore, tAfter, tAlways;

	tColon		= cwsprintf(T_COLON);
	tDo 		= cwsprintf(T_DO);
	tBegin 		= cwsprintf(T_BEGIN);
	tBegin		= _T("\t\t") + tBegin;
	tEnd		= cwsprintf(T_END);
	tEnd		= _T("\t\t") + tEnd;
	tBefore		= cwsprintf(T_BEFORE);
	tBefore		= _T("\t") + tBefore;
	tAfter		= cwsprintf(T_AFTER);
	tAfter		= _T("\t") + tAfter;
	tAlways		= cwsprintf(T_ALWAYS);
	tAlways		= _T("\t") + tAlways;

	CString str, msg;
	msg = _TB("/****** CHANGE THE EXAMPLE WHERE REQUIRED ******/");
	StrAppend(str, msg, FALSE, TRUE);

	switch (m_CurEventType)
	{
		case CUSTOM_EVENT:
		{
			CString	evnName, tBreaking, tWhen;
			evnName		= cwsprintf(_TB("NewEvent_{0-%d}"), m_lbCustomEvents.GetCount() + 1);
			msg = _T("\t") + _TB("/****** USE ONE OF THE FOLLOWING CRITERIA: ******/");
			tBreaking	= cwsprintf(T_BREAKING);
			tBreaking	+= _T(" <c1>,<c2>,...");
			tWhen		= cwsprintf(T_WHEN);
			tWhen		+= _T(" <expr>");
			StrAppend(str, evnName,		TRUE,	FALSE );
			StrAppend(str, tColon,		TRUE,	TRUE );
			StrAppend(str, msg,			TRUE,	TRUE );
			StrAppend(str, _T("\t// a)\t"),	TRUE,	FALSE );
			StrAppend(str, tBreaking,	TRUE, 	TRUE );
			StrAppend(str, _T("\t// b)\t"),	TRUE,	FALSE );
			StrAppend(str, tWhen,		TRUE,	TRUE );
			StrAppend(str, _T("\t// c)\t"),	TRUE,	FALSE );
			StrAppend(str, tBreaking,	TRUE, 	FALSE );
			StrAppend(str, _T("AND/OR"),	TRUE,	FALSE );
			StrAppend(str, tWhen,		TRUE,	TRUE );
			StrAppend(str, CString(_T("// ")) + _TB("Remarks:"), TRUE,	TRUE );
			StrAppend(str, CString(_T("// ")) +
				_TB("The rule's fields evaluated into \"<when-expr>\" contains the value of the new record"), TRUE,	TRUE);
			StrAppend(str, CString(_T("// ")) + 
				_TB("If the \"<when-expr>\" contains a rules' field, the 'Before' event was not raised on the last record"), TRUE, TRUE);

			StrAppend(str, tDo,			FALSE,	TRUE  );

			StrAppend(str, tBefore,		TRUE,	FALSE  );
			StrAppend(str, CString(_T("// ")) + _TB("The 'Before' event was not raised on the first record"), TRUE,	TRUE);
			StrAppend(str, tBegin,		FALSE,	TRUE  );
			StrAppend(str, tEnd,		FALSE,	TRUE  );

			StrAppend(str, _T(""),		FALSE,	TRUE  );

			StrAppend(str, tAfter,		TRUE,	FALSE  );
			StrAppend(str, CString(_T("// ")) + _TB("The 'After' event was not raised on the last record"), TRUE,	TRUE);
			StrAppend(str, tBegin,		FALSE,	TRUE  );
			StrAppend(str, tEnd,		FALSE,	TRUE  );

			break;
		}
		case REPORT_ACTION:
		{
			CString	tReport = cwsprintf(T_REPORT);
			StrAppend(str, tReport,		TRUE,	FALSE );
			StrAppend(str, tColon,		TRUE,	FALSE );
			StrAppend(str, tDo,			FALSE,	TRUE  );
	
			StrAppend(str, tAlways,		TRUE,	TRUE  );
			StrAppend(str, tBegin,		FALSE,	TRUE  );
			StrAppend(str, tEnd,		FALSE,	TRUE  );
			StrAppend(str, _T(""),		FALSE,	TRUE  );
			StrAppend(str, tBefore,		TRUE,	TRUE  );
			StrAppend(str, tBegin,		FALSE,	TRUE  );
			StrAppend(str, tEnd,		FALSE,	TRUE  );
			StrAppend(str, _T(""),		FALSE,	TRUE  );
			StrAppend(str, tAfter,		TRUE,	TRUE  );
			StrAppend(str, tBegin,		FALSE,	TRUE  );
			StrAppend(str, tEnd,		FALSE,	TRUE  );
			break;
		}
		case NEWPAGE_ACTION:
		{
			CString tFormfeed = cwsprintf(T_FORMFEED);
			StrAppend(str, tFormfeed,	TRUE,	FALSE );
			StrAppend(str, tColon,		TRUE,	FALSE );
			StrAppend(str, tDo,			FALSE,	TRUE  );
	
			StrAppend(str, tBefore,		TRUE,	TRUE  );
			StrAppend(str, tBegin,		FALSE,	TRUE  );
			StrAppend(str, tEnd,		FALSE,	TRUE  );
			StrAppend(str, _T(""),		FALSE,	TRUE  );
			StrAppend(str, tAfter,		TRUE,	TRUE  );
			StrAppend(str, tBegin,		FALSE,	TRUE  );
			StrAppend(str, tEnd,		FALSE,	TRUE  );
			break;
		}
		case DISPLAY_TABLE_ACTION:
		{
			CString tTable = cwsprintf(T_TABLE);
			StrAppend(str, tTable,				TRUE,	FALSE );
			StrAppend(str, GetCurrTableName(),	TRUE,	FALSE );
			StrAppend(str, tColon,				TRUE,	FALSE );
			StrAppend(str, tDo,					FALSE,	TRUE  );
	
			StrAppend(str, tBefore,		TRUE,	FALSE  );
			StrAppend(str, CString(_T("// ")) + _TB("Before the first page's row (excluding the first page)"), TRUE, TRUE);
			StrAppend(str, tBegin,		FALSE,	TRUE  );
			StrAppend(str, tEnd,		FALSE,	TRUE  );
			StrAppend(str, _T(""),		FALSE,	TRUE  );
			StrAppend(str, tAfter,		TRUE,	FALSE  );
			StrAppend(str, CString(_T("// ")) + _TB("After the last page's row"), TRUE, TRUE);
			StrAppend(str, tBegin,		FALSE,	TRUE  );
			StrAppend(str, tEnd,		FALSE,	TRUE  );
			break;
		}
	}

	ConvertCString(str, LF_TO_CRLF);

	m_edtEventActions.EnableWindow(TRUE);
	m_edtEventActions.SetWindowText(str);

	GetDlgItem(IDC_EVNDLG_ADD_CUSTOM_EVENT)->	EnableWindow(FALSE);
	GetDlgItem(IDC_EVNDLG_DEL_CUSTOM_EVENT)->	EnableWindow(FALSE);
	GetDlgItem(IDC_EVNDLG_CUSTOM_EVENT_UP)->	EnableWindow(FALSE);
	GetDlgItem(IDC_EVNDLG_CUSTOM_EVENT_DOWN)->	EnableWindow(FALSE);

	GetDlgItem(IDOK)->						EnableWindow(FALSE);
	GetDlgItem(IDC_EVNDLG_UNDO_EVENT)->		EnableWindow(m_CurEventType	== CUSTOM_EVENT);
	GetDlgItem(IDC_EVNDLG_CLEAR_ACTIONS)->	EnableWindow(FALSE);

	m_bStatusNew = TRUE;
}

//----------------------------------------------------------------------------
void CEventDlg::ShowEvent(LPCTSTR pszFileName)
{
	CString	strTmp;
	BOOL	bEmptyEvent = TRUE;

	if (pszFileName && *pszFileName)
	{
		CLineFile	iFile;
		TCHAR 		buffer[256];

		UINT flags = CFile::modeRead | CFile::shareDenyWrite | CFile::typeText;

		if (iFile.Open(pszFileName, flags))
		{
			while (iFile.ReadString(buffer, 255))
				strTmp += buffer;
		
			iFile.Close();
		}

		DeleteFile(pszFileName);

		if (!strTmp.IsEmpty())
			ConvertCString(strTmp, LF_TO_CRLF);

		bEmptyEvent = FALSE;
	}

	m_edtEventActions.EnableWindow(!strTmp.IsEmpty());

	if (m_CurEventType == CUSTOM_EVENT)
	{
		strTmp += CString(_T("//---- ")) + _TB("Remarks:")  + '\r' + '\n';
		strTmp += CString(_T("// ")) + _TB("The 'Before' event was not raised on the first record") + '\r' + '\n';
		strTmp += CString(_T("// ")) + _TB("The 'After' event was not raised on the last record");
		if (strTmp.Find(_T("When")) > 0 )
		{
			strTmp += CString(_T("\r\n")) + CString(_T("// ")) + _TB("The rule's fields evaluated into \"<when-expr>\" contains the value of the new record");
			strTmp += CString(_T("\r\n")) + CString(_T("// ")) + _TB("If the \"<when-expr>\" contains a rules' field, the 'Before' event was not raised on the last record");
		}
	}
	else if (m_CurEventType == DISPLAY_TABLE_ACTION)
	{
		strTmp += CString(_T("//---- ")) + _TB("Remarks:")  + '\r' + '\n';
		strTmp += CString(_T("// ")) + _TB("The 'Before' event was not raised on the first page");
	}

	m_edtEventActions.SetWindowText(strTmp);

	GetDlgItem(IDC_EVNDLG_ADD_CUSTOM_EVENT)->	EnableWindow(m_CurEventType	== CUSTOM_EVENT);
	GetDlgItem(IDC_EVNDLG_DEL_CUSTOM_EVENT)->	EnableWindow(!bEmptyEvent && m_CurEventType	== CUSTOM_EVENT);

	GetDlgItem(IDOK)->						EnableWindow(FALSE);
	GetDlgItem(IDC_EVNDLG_UNDO_EVENT)->		EnableWindow(FALSE);
	GetDlgItem(IDC_EVNDLG_CLEAR_ACTIONS)->	EnableWindow(!strTmp.IsEmpty());

	m_bStatusNew = FALSE;
}

//----------------------------------------------------------------------------
void CEventDlg::ShowError(Parser& lex)
{
	// NB. lex.ShowErrors() azzera la posizione di errore ed il messaggio
	int nLine = lex.GetCurrentLine();
	int nCol = lex.GetCurrentPos();
	m_sbStatus.SetPaneText(0, lex.BuildErrMsg());
	m_sbStatus.SetPaneText(1, cwsprintf(ID_EDTCMM_EQNEDIT_ROW_COL, nLine, nCol));

	lex.ShowErrors();

	// Va fatta dopo la ShowErrors per evitare che quando il fuoco
	// ritorna all'edit tutto il testo venga selezionato
	m_edtEventActions.PositionCursor(nLine - 1, nCol - 1);
}

//----------------------------------------------------------------------------
void CEventDlg::UpdateEventList()
{
	int i = 0;
	for (i = 0; i < m_pEventData->m_TriggEvents.GetSize(); i++)
	{
		// la listbox NON DEVE AVERE lo stile LBS_SORT
		TriggEventData* pTriggEvent = (TriggEventData*) m_pEventData->m_TriggEvents[i];
		if (pTriggEvent)
			m_lbCustomEvents.AddString(pTriggEvent->GetEventName());
	}

	i = m_lbPredefEvents.	AddString(EventDescription(REPORT));
	m_lbPredefEvents.		SetItemData(i, REPORT);
	i = m_lbPredefEvents.	AddString(EventDescription(JUMP_PAGE));
	m_lbPredefEvents.		SetItemData(i, JUMP_PAGE);

	for (i = 0; i < m_pDispTable->GetCount(); i++)
	{
		int j = m_lbPredefEvents.	AddString(cwsprintf(EventDescription(TABLE_EVENT), m_pDispTable->GetNameFromIdx(i)));
		m_lbPredefEvents.			SetItemData(j, MAKELONG(TABLE_EVENT, i));
	}

	m_lbCustomEvents.EnableWindow(TRUE);
	m_lbPredefEvents.EnableWindow(TRUE);

	m_lbCustomEvents.SetFocus();
}

//----------------------------------------------------------------------------
void CEventDlg::MoveCustomEvent(int nFrom, int nTo)
{
	TriggEventData* pTriggEvent = (TriggEventData*) m_pEventData->m_TriggEvents[nFrom];
	
	// serve per non far cancellare l'oggetto dalla RemoveAt
	m_pEventData->m_TriggEvents.SetAt	(nFrom, NULL);
	m_pEventData->m_TriggEvents.RemoveAt(nFrom);
	m_lbCustomEvents.DeleteString		(nFrom);

	m_pEventData->m_TriggEvents.InsertAt(nTo, pTriggEvent);
	m_lbCustomEvents.InsertString		(nTo, pTriggEvent->GetEventName());
	m_lbCustomEvents.SetCurSel			(nTo);


	GetDlgItem(IDC_EVNDLG_CUSTOM_EVENT_UP)->	EnableWindow(nTo > 0);
	GetDlgItem(IDC_EVNDLG_CUSTOM_EVENT_DOWN)->	EnableWindow
		(
			nTo >= 0 && nTo < m_lbCustomEvents.GetCount() - 1
		);

	m_bModified = TRUE;
}

//----------------------------------------------------------------------------
void CEventDlg::ShowReportActions()
{
	m_CurEventType = REPORT_ACTION;

	if (m_pEventData->m_pReportActions == NULL || m_pEventData->m_pReportActions->IsEmpty())
	{
		ShowPrototype();
		return;
	}

	CString	strFileName = GetTempName();
	Unparser oFile(strFileName);

	m_pEventData->m_pReportActions->Unparse(oFile);
	oFile.Close();

	ShowEvent(strFileName);
}

//----------------------------------------------------------------------------
void CEventDlg::ShowNewPageActions()
{
	m_CurEventType = NEWPAGE_ACTION;

	if (m_pEventData->m_pNewPageActions == NULL || m_pEventData->m_pNewPageActions->IsEmpty())
	{
		ShowPrototype();
		return;
	}

	CString	strFileName = GetTempName();
	Unparser oFile(strFileName);

	m_pEventData->m_pNewPageActions->Unparse(oFile);
	oFile.Close();

	ShowEvent(strFileName);
}

//----------------------------------------------------------------------------
void CEventDlg::ShowTableActions()
{
	m_CurEventType	= DISPLAY_TABLE_ACTION;

	TableActionData* pTblEvent = m_pEventData->GetTableEvent(GetCurrTableName());

	// se non esiste un evento associato alla tabella corrente ci si predispone
	// per farne uno nuovo
	if (pTblEvent == NULL)
	{
		ShowPrototype();
		return;
	}

	CString strFileName = GetTempName();
	Unparser oFile(strFileName);

	pTblEvent->Unparse(oFile);
	oFile.Close();

	ShowEvent(strFileName);
}

//----------------------------------------------------------------------------
BOOL CEventDlg::DoSave(Parser& lex)
{
	m_sbStatus.SetPaneText(0, NULL);

	switch (m_CurEventType)
	{
		case CUSTOM_EVENT:
		{
			if (!SaveCustomEventActions(lex))	
				return FALSE;

			int nCurrSel = m_lbCustomEvents.GetCurSel();
			GetDlgItem(IDC_EVNDLG_ADD_CUSTOM_EVENT)->	EnableWindow(TRUE);
			GetDlgItem(IDC_EVNDLG_DEL_CUSTOM_EVENT)->	EnableWindow(TRUE);
			GetDlgItem(IDC_EVNDLG_CUSTOM_EVENT_UP)->	EnableWindow(nCurrSel > 0);
			GetDlgItem(IDC_EVNDLG_CUSTOM_EVENT_DOWN)->	EnableWindow
				(
					nCurrSel >= 0 && nCurrSel < m_lbCustomEvents.GetCount() - 1
				);

			m_lbCustomEvents.SetFocus();
			break;
		}
		
		case REPORT_ACTION:
			if (!SaveReportActions(lex))	
				return FALSE;
			m_lbPredefEvents.SetFocus();
			break;
			
		case NEWPAGE_ACTION:     
			if (!SaveNewPageActions(lex))	
				return FALSE;
			m_lbPredefEvents.SetFocus();
			break;
			
		case DISPLAY_TABLE_ACTION:
			if (!SaveTableActions(lex))	
				return FALSE;
			m_lbPredefEvents.SetFocus();
			break;
	}
	
	m_bStatusNew	= FALSE;
	m_bModified		= TRUE;

	m_lbCustomEvents.	EnableWindow(TRUE);
	m_lbPredefEvents.	EnableWindow(TRUE);

	GetDlgItem(IDOK)->						EnableWindow(FALSE);
	GetDlgItem(IDC_EVNDLG_UNDO_EVENT)->		EnableWindow(FALSE);
	GetDlgItem(IDC_EVNDLG_CLEAR_ACTIONS)->	EnableWindow(TRUE);

	return TRUE;
}

//----------------------------------------------------------------------------
BOOL CEventDlg::SaveCustomEventActions(Parser& lex)
{
	TriggEventData* newTriggEvent = new TriggEventData(*m_pSymTable);
	if (!newTriggEvent->Parse(lex))
	{
		ShowError(lex);

		delete newTriggEvent;
		return FALSE;
	}

	int nCurrSel = m_lbCustomEvents.GetCurSel();

	if (m_bStatusNew)
	{
		if (m_pEventData->GetEventIdx(newTriggEvent->GetEventName()) >= 0)
		{
			AfxMessageBox(_TB("Event name already exists."), MB_OK | MB_ICONEXCLAMATION);
			delete newTriggEvent;
			return FALSE;
		}
				
		m_pEventData->m_TriggEvents.InsertAt(nCurrSel + 1, newTriggEvent);

		m_lbCustomEvents.InsertString(nCurrSel + 1, newTriggEvent->GetEventName());
		m_lbCustomEvents.SetCurSel   (nCurrSel + 1);
		
		return TRUE;
	}

	CString	strEventName;
	m_lbCustomEvents.GetText(nCurrSel, strEventName);
	int	nEventIdx = m_pEventData->GetEventIdx(strEventName);

	if (nEventIdx >= 0)
	{
		m_pEventData->m_TriggEvents.RemoveAt(nEventIdx);
		m_pEventData->m_TriggEvents.InsertAt(nEventIdx, newTriggEvent);
	}

	if (strEventName.CompareNoCase(newTriggEvent->GetEventName()) != 0)
	{
		m_lbCustomEvents.DeleteString(nCurrSel);
		m_lbCustomEvents.InsertString(nCurrSel, newTriggEvent->GetEventName());
		m_lbCustomEvents.SetCurSel	 (nCurrSel);
	}
		
	return TRUE;
}

//----------------------------------------------------------------------------
BOOL CEventDlg::SaveReportActions(Parser& lex)
{
	ReportEventData* pNewReportEvent = new ReportEventData(*m_pSymTable);
	if (!pNewReportEvent->Parse(lex))
	{
		ShowError(lex);

		delete pNewReportEvent;
		return FALSE;
	}
	
	if (m_pEventData->m_pReportActions)
		delete m_pEventData->m_pReportActions;

	m_pEventData->m_pReportActions = pNewReportEvent;
		
	return TRUE;
}

//----------------------------------------------------------------------------
BOOL CEventDlg::SaveNewPageActions(Parser& lex)
{
	NewPageActionData* pNewPageAction = new NewPageActionData(*m_pSymTable);
	if (!pNewPageAction->Parse(lex))
	{
		ShowError(lex);

		delete pNewPageAction;
		return FALSE;
	}
	
	if (m_pEventData->m_pNewPageActions)
		delete m_pEventData->m_pNewPageActions;

	m_pEventData->m_pNewPageActions = pNewPageAction;
		
	return TRUE;
}

//----------------------------------------------------------------------------
BOOL CEventDlg::SaveTableActions(Parser& lex)
{
	TableActionData* pNewTableEvent = new TableActionData(*m_pSymTable);
	if (!pNewTableEvent->Parse(lex))
	{
		ShowError(lex);

		delete pNewTableEvent;
		return FALSE;
	}

	if (m_bStatusNew)
	{
		m_pEventData->m_TableActions.Add(pNewTableEvent);
		return TRUE;
	}

	CString strTableEventName = GetCurrTableName();
	int nEventIdx = m_pEventData->GetTableEventIdx(strTableEventName);
	if (nEventIdx >= 0)
	{
		m_pEventData->m_TableActions.RemoveAt(nEventIdx);
		m_pEventData->m_TableActions.InsertAt(nEventIdx, pNewTableEvent);
	}

	if (strTableEventName.CompareNoCase(pNewTableEvent->GetDisplayTableName()) != 0)
	{
		if (!pNewTableEvent->GetDisplayTableName().IsEmpty())
			AfxMessageBox(_TB("Invalid \"Table\" Name. Replaced by default name."), MB_OK | MB_ICONEXCLAMATION);
				
		pNewTableEvent->SetDisplayTableName(strTableEventName);
	}

	return TRUE;
}

//----------------------------------------------------------------------------
void CEventDlg::DelEvent()
{
	if (m_bStatusNew)
		return;

	int nCurrSel;
	int	nEventIdx;
	CString	strEventName;
	CString msg;
		
	switch (m_CurEventType)
	{
		case CUSTOM_EVENT:
			nCurrSel = m_lbCustomEvents.GetCurSel();
			m_lbCustomEvents.GetText(nCurrSel, strEventName);
			msg = cwsprintf(_TB("Are you sure you want to delete the {0-%s} event?"), (LPCTSTR) strEventName);
			if (AfxMessageBox(msg, MB_YESNO) != IDYES)
				return;

			nEventIdx = m_pEventData->GetEventIdx(strEventName);
			if (nEventIdx >= 0)
				m_pEventData->m_TriggEvents.RemoveAt(nEventIdx);

			m_lbCustomEvents.DeleteString(nCurrSel);
			if (nCurrSel > m_lbCustomEvents.GetCount() - 1)
				nCurrSel = m_lbCustomEvents.GetCount() - 1;

			m_lbCustomEvents.SetCurSel(nCurrSel);
			break;
	
		case REPORT_ACTION:
			msg = cwsprintf(_TB("Are you sure you want to delete the {0-%s} event?"), (LPCTSTR) cwsprintf(_TB("Report")));
			if (AfxMessageBox(msg, MB_YESNO) != IDYES)
				return;
		
			if (m_pEventData->m_pReportActions)
				delete m_pEventData->m_pReportActions;
		
			m_pEventData->m_pReportActions = NULL;
			break;
				
		case NEWPAGE_ACTION:     
			msg = cwsprintf(_TB("Are you sure you want to delete the {0-%s} event?"), (LPCTSTR) cwsprintf(_TB("Form Feed")));
			if (AfxMessageBox(msg, MB_YESNO) != IDYES)
				return;
		
			if (m_pEventData->m_pNewPageActions)
				delete m_pEventData->m_pNewPageActions;
		
			m_pEventData->m_pNewPageActions = NULL;
			break;
				
		case DISPLAY_TABLE_ACTION:
			strEventName = GetCurrTableName();
			nEventIdx = m_pEventData->GetTableEventIdx(strEventName);

			strEventName = cwsprintf(_TB("Fill {0-%s}"), strEventName);
			msg = cwsprintf(_TB("Are you sure you want to delete the {0-%s} event?"), (LPCTSTR) strEventName);
			if (AfxMessageBox(msg, MB_YESNO) != IDYES)
				return;
		
			if (nEventIdx >= 0)
				m_pEventData->m_TableActions.RemoveAt(nEventIdx);

			break;
	}

	m_bModified = TRUE;
	OnShowCurrentEvent();
}

//----------------------------------------------------------------------------
BOOL CEventDlg::OnInitDialog()
{
	CParsedDialog::OnInitDialog();
	
	m_lbCustomEvents.	SubclassDlgItem	(IDC_EVNDLG_CUSTOM_EVENT_LIST,	this);
	m_lbPredefEvents.	SubclassDlgItem	(IDC_EVNDLG_PREDEF_EVENT_LIST,	this);
	m_edtEventActions.	SubclassEdit	(IDC_EVNDLG_EVENT_EDIT,			this);

	UpdateEventList	();
	InitStatusBar	();	

	CheckRadioButton(IDC_EVNDLG_RAD_CUSTOMIZE, IDC_EVNDLG_RAD_DEFAULT, IDC_EVNDLG_RAD_DEFAULT); 
	OnClickRadio();

	return TRUE;
}

//----------------------------------------------------------------------------
void CEventDlg::OnClickRadio()
{
	if (GetCheckedRadioButton(IDC_EVNDLG_RAD_CUSTOMIZE, IDC_EVNDLG_RAD_DEFAULT) == IDC_EVNDLG_RAD_CUSTOMIZE)
	{
		m_lbPredefEvents.EnableWindow(FALSE);
		m_lbCustomEvents.EnableWindow(TRUE);
		m_lbCustomEvents.SetFocus();
	}
	else
	{
		m_lbCustomEvents.EnableWindow(FALSE);
		m_lbPredefEvents.EnableWindow(TRUE);
		m_lbPredefEvents.SetFocus();
	}
}                    

//----------------------------------------------------------------------------
void CEventDlg::OnCustomEventSetFocus()
{
	if (m_lbCustomEvents.GetCurSel() < 0)
		m_lbCustomEvents.SetCurSel(0);

	m_lbPredefEvents.SetCurSel(-1);

	OnShowCustomEventActions();
}

//----------------------------------------------------------------------------
void CEventDlg::OnPredefEventSetFocus()
{
	if (m_lbPredefEvents.GetCurSel() < 0)
		m_lbPredefEvents.SetCurSel(0);

	m_lbCustomEvents.SetCurSel(-1);

	OnShowPredefEventActions();
}

//---------------------------------------------------------------------------
void CEventDlg::OnShowCustomEventActions()
{
	m_CurEventType	= CUSTOM_EVENT;

	int	nCurrSel = m_lbCustomEvents.GetCurSel();

	GetDlgItem(IDC_EVNDLG_CUSTOM_EVENT_UP)->	EnableWindow(nCurrSel > 0);
	GetDlgItem(IDC_EVNDLG_CUSTOM_EVENT_DOWN)->	EnableWindow
		(
			nCurrSel >= 0 && nCurrSel < m_lbCustomEvents.GetCount() - 1
		);

	if (nCurrSel < 0)
	{
		ShowEvent(NULL);
		return;
	}

	CString		strEventName;
	CString		strFileName = GetTempName();

	m_lbCustomEvents.GetText(nCurrSel, strEventName);

	TriggEventData* pTriggEvent = m_pEventData->GetEvent(strEventName);
	if (!pTriggEvent)
	{
		m_lbCustomEvents.DeleteString(nCurrSel);
		ASSERT(FALSE);

		return;
	}

	Unparser oFile(strFileName);

	pTriggEvent->Unparse(oFile);
	oFile.Close();

	ShowEvent(strFileName);
}

//----------------------------------------------------------------------------
void CEventDlg::OnCustomEventUp()
{
	int nCurrSel = m_lbCustomEvents.GetCurSel();

	if (nCurrSel <= 0)	
		return;

	MoveCustomEvent(nCurrSel, nCurrSel - 1);
}

//----------------------------------------------------------------------------
void CEventDlg::OnCustomEventDown()
{
	int nCurrSel = m_lbCustomEvents.GetCurSel();

	if (nCurrSel < 0 || nCurrSel == (m_lbCustomEvents.GetCount() - 1))
		return;
	
	MoveCustomEvent(nCurrSel, nCurrSel + 1);
}

//----------------------------------------------------------------------------
void CEventDlg::OnAddCustomEvent()
{
	ShowPrototype();

	m_edtEventActions.SetFocus();
}

//----------------------------------------------------------------------------
void CEventDlg::OnDelCustomEvent()
{
	if (m_bStatusNew)
		return;

	if (m_CurEventType != CUSTOM_EVENT)
		return;

	DelEvent();
}

//----------------------------------------------------------------------------
void CEventDlg::OnShowPredefEventActions()
{
	GetDlgItem(IDC_EVNDLG_CUSTOM_EVENT_UP)->	EnableWindow(FALSE);
	GetDlgItem(IDC_EVNDLG_CUSTOM_EVENT_DOWN)->	EnableWindow(FALSE);

	int i = m_lbPredefEvents.GetCurSel();
	if (i < 0)
		return;

	switch (m_lbPredefEvents.GetItemData(i))
	{
		case REPORT:		ShowReportActions();	break;
		case JUMP_PAGE:		ShowNewPageActions();	break;
		default:
			if (LOWORD(m_lbPredefEvents.GetItemData(i)) == TABLE_EVENT)
								ShowTableActions();		break;
	}
}

//----------------------------------------------------------------------------
void CEventDlg::OnCodeModified()
{
	m_lbCustomEvents.	EnableWindow(FALSE);
	m_lbPredefEvents.	EnableWindow(FALSE);

	GetDlgItem(IDC_EVNDLG_ADD_CUSTOM_EVENT)->	EnableWindow(FALSE);
	GetDlgItem(IDC_EVNDLG_DEL_CUSTOM_EVENT)->	EnableWindow(FALSE);

	GetDlgItem(IDOK)->							EnableWindow(TRUE);
	GetDlgItem(IDC_EVNDLG_UNDO_EVENT)->			EnableWindow(TRUE);


	CString	strTmp;
	m_edtEventActions.GetWindowText(strTmp);
	GetDlgItem(IDC_EVNDLG_CLEAR_ACTIONS)->		EnableWindow(!strTmp.IsEmpty());
}

//----------------------------------------------------------------------------
void CEventDlg::OnShowCurrentEvent()
{
	switch (m_CurEventType)
	{
		case CUSTOM_EVENT:
			OnShowCustomEventActions();
			m_lbCustomEvents.SetFocus();
			break;
	
		case REPORT_ACTION:
			OnShowPredefEventActions();
			m_lbPredefEvents.SetFocus();
			break;

		case NEWPAGE_ACTION:     
			OnShowPredefEventActions();
			m_lbPredefEvents.SetFocus();
			break;

		case DISPLAY_TABLE_ACTION:
			OnShowPredefEventActions();
			m_lbPredefEvents.SetFocus();
			break;
	}

	m_lbCustomEvents.	EnableWindow(TRUE);
	m_lbPredefEvents.	EnableWindow(TRUE);

	m_sbStatus.SetPaneText(0, NULL);
}

//----------------------------------------------------------------------------
void CEventDlg::OnOK()
{
	CString	strTmp;

	m_edtEventActions.GetWindowText(strTmp);
	ConvertCString(strTmp, CRLF_TO_LF);

	Parser lex(strTmp);

	// the event is empty, so it must deleted
	if (lex.LookAhead(T_EOF))
	{
		if (m_bStatusNew)
			OnShowCurrentEvent();
		else
			DelEvent();
			
		return;
	}

	if (DoSave(lex))
		OnClickRadio();
}

// Gestisce il bottone CHIUDI e il tasto ESCAPE
//----------------------------------------------------------------------------
void CEventDlg::OnCancel()
{
	if (GetDlgItem(IDOK)->IsWindowEnabled())
	{
		CString	strTmp;

		m_edtEventActions.GetWindowText(strTmp);
		ConvertCString(strTmp, CRLF_TO_LF);

		Parser lex(strTmp);

		// the event is empty, so it must deleted
		if (lex.LookAhead(T_EOF))
		{
			if (!m_bStatusNew)
			{
				// la conferma di cancellazione verra` chiesta dalla DelEvent
				DelEvent();
			}
		}
		else
			if	(
					AfxMessageBox(_TB("Save last changes?"), MB_YESNO) == IDYES &&
					!DoSave(lex)
				)
				return;
	}

	// stacca la connessione alla status bar
	m_edtEventActions.SetDlgStatusBar(NULL);

	EndDialog(m_bModified ? IDOK : IDCANCEL);
}

//----------------------------------------------------------------------------
void CEventDlg::OnClearActions()
{
	int nCurrSel;
	CString	strEventName;
	CString msg;
		
	switch (m_CurEventType)
	{
		case CUSTOM_EVENT:
			nCurrSel = m_lbCustomEvents.GetCurSel();
			if (nCurrSel >= 0)
			{
				m_lbCustomEvents.GetText(nCurrSel, strEventName);
				msg = cwsprintf(_TB("Changes made on {0-%s} event actions will be lost.\r\n\rContinue?"), (LPCTSTR) strEventName);
			}
			break;
	
		case REPORT_ACTION:
			msg = cwsprintf(_TB("Changes made on {0-%s} event actions will be lost.\r\n\rContinue?"), (LPCTSTR) cwsprintf(_TB("Report")));
			break;
				
		case NEWPAGE_ACTION:     
			msg = cwsprintf(_TB("Changes made on {0-%s} event actions will be lost.\r\n\rContinue?"), (LPCTSTR) cwsprintf(_TB("Form Feed")));
			break;
				
		case DISPLAY_TABLE_ACTION:
			nCurrSel = m_lbPredefEvents.GetCurSel();
			strEventName = cwsprintf(_TB("Fill {0-%s}"), GetCurrTableName());
			msg = cwsprintf(_TB("Changes made on {0-%s} event actions will be lost.\r\n\rContinue?"), (LPCTSTR) strEventName);
			break;
	}

	if (m_edtEventActions.GetModifyFlag() && AfxMessageBox(msg, MB_YESNO) != IDYES)
		return;

	m_edtEventActions.SetModifyFlag(TRUE);
	m_edtEventActions.SetWindowText(_T(""));

	OnCodeModified();
}

//----------------------------------------------------------------------------
void CEventDlg::OnCallProcedureDlg()
{                      
	CProcedureDlg procDlg
		(
			m_pEditorManager,
			m_pEventData,
			m_pProcData,
			m_pSymTable,
			m_pRuleData,
			FALSE			/* can't delete/rename procedures */
		);

	procDlg.DoModal ();
}

//----------------------------------------------------------------------------
void CEventDlg::OnCallAddFieldDlg()
{
	WoormField* pRepFieldData = new WoormField(_T(""));

	// il nuovo campo viene forzato hidden
	pRepFieldData->SetHidden(TRUE);

	CVarFieldDlg varFieldDlg(m_pSymTable, m_pRuleData, pRepFieldData, NULL, TRUE);

	if (varFieldDlg.DoModal() != IDOK)
		return;

	m_pEditorManager->SetModifiedFlag();
}


//===========================================================================
//===========================================================================
//===========================================================================
//===========================================================================
//===========================================================================
// SubtotalObject
//===========================================================================

class SubtotalObject : public CObject
{
public:
	CString		m_strColumnName;
	CString		m_strSubtotalName;
	//BOOL		m_bInBeforeSection;

public:
	SubtotalObject(WoormTable*, LPCTSTR/*, BOOL*/);
};

//----------------------------------------------------------------------------
SubtotalObject::SubtotalObject
	(        
		WoormTable*	m_pSymTable,
		LPCTSTR			pszColumnName/*,
		BOOL			bBeforeSection*/
	)
{
	m_strColumnName		= pszColumnName;
	//m_bInBeforeSection	= bBeforeSection;
	m_strSubtotalName	= GenerateName(m_pSymTable, pszColumnName, _T("SubTot{0-%s}"));
}


//===========================================================================
//	This class is used to input the break-information to generate
//	the event that display the subtotals fields.
//	NB. "nEventNumber" parameter is a default number to suggest an 
//		unique eventName.
//
//==========================================================================//
class CBrkCondDlg : public CParsedDialog
{
	DECLARE_DYNAMIC(CBrkCondDlg)
public:
	WoormTable*	m_pSymTable;
	CStringArray*	m_pBreakFieldList;
	CString*		m_pNameBreakCond;
	int				m_nEventNumber;		
	EventsData*		m_pEventData;

	CMultiListBox	m_mlbFields;
	CIdentifierEdit	m_edtName;

public:
	CBrkCondDlg
		(
			WoormTable* pSymTable,
			CStringArray*	pBreakFieldList,
			CString*		pstrName,
			int				nEventNumber,
			EventsData*		pEventData
		);

public:
	void	AddFieldInBreakList	(LPCTSTR);
	void	DelFieldInBreakList	(LPCTSTR);

protected:
	virtual BOOL	OnInitDialog		();
	virtual	void	OnOK				();
	virtual	void	OnCancel			();

	//{{AFX_MSG( CBrkCondDlg )
	afx_msg	void	ShowFieldList		();
	afx_msg	void	OnDblClkFieldList	();
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()
};


//===========================================================================
// CBrkCondDlg
//===========================================================================
IMPLEMENT_DYNAMIC(CBrkCondDlg, CParsedDialog)
BEGIN_MESSAGE_MAP(CBrkCondDlg, CParsedDialog)
	//{{AFX_MSG_MAP(CBrkCondDlg)
	ON_BN_CLICKED		(IDC_EVNDLG_BRK_SHOW_STR,	ShowFieldList)
	ON_BN_CLICKED		(IDC_EVNDLG_BRK_SHOW_DATE,	ShowFieldList)
	ON_BN_CLICKED		(IDC_EVNDLG_BRK_SHOW_NUM,	ShowFieldList)
	ON_LBN_DBLCLK		(IDC_EVNDLG_BRK_FIELD_LIST,	OnDblClkFieldList)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//----------------------------------------------------------------------------
CBrkCondDlg::CBrkCondDlg
	(
		WoormTable* pSymTable,
		CStringArray*	pBreakFieldList,
		CString*		pstrName,
		int				nEventNumber,
		EventsData*		pEventData
	)
	:
	CParsedDialog		(IDD_EVNDLG_BREAKCOND),
	m_pSymTable			(pSymTable),
	m_pBreakFieldList	(pBreakFieldList),
	m_pNameBreakCond	(pstrName),
	m_nEventNumber		(nEventNumber),
	m_edtName			(NULL),
	m_mlbFields			(IDB_CHECK),
	m_pEventData		(pEventData)
{}

//----------------------------------------------------------------------------
BOOL CBrkCondDlg::OnInitDialog()
{
	CParsedDialog::OnInitDialog();
	
	m_mlbFields.SubclassDlgItem	(IDC_EVNDLG_BRK_FIELD_LIST, this);
	m_mlbFields.SetStyle		(CMultiListBox::SORT_ON_FIRST_STRING);

	m_edtName.	SubclassEdit	(IDC_EVNDLG_BRK_NAME, this);
	
	CheckDlgButton(IDC_EVNDLG_BRK_SHOW_STR,	TRUE);
	CheckDlgButton(IDC_EVNDLG_BRK_SHOW_DATE,	FALSE);
	CheckDlgButton(IDC_EVNDLG_BRK_SHOW_NUM,	FALSE);

	ShowFieldList();

	CString strDefaultName = cwsprintf(_T("Event_%d"), m_nEventNumber);
	m_edtName.SetValue(strDefaultName);

	return TRUE;
}

//----------------------------------------------------------------------------
void CBrkCondDlg::ShowFieldList()
{
	BOOL bShowStringsEnums	= IsDlgButtonChecked(IDC_EVNDLG_BRK_SHOW_STR);
	BOOL bShowDatesBools 	= IsDlgButtonChecked(IDC_EVNDLG_BRK_SHOW_DATE);
	BOOL bShowNumerics		= IsDlgButtonChecked(IDC_EVNDLG_BRK_SHOW_NUM);

	m_mlbFields.SetRedraw(FALSE);
	m_mlbFields.ResetContent();
	for (int i = 0; i < m_pSymTable->GetSize(); i++)
	{
		WoormField* pRepField = (WoormField*) m_pSymTable->GetAt(i);

		if	(
				pRepField->GetFieldType() != WoormField::FIELD_COLUMN &&
				pRepField->GetFieldType() != WoormField::FIELD_NORMAL
			)	
			continue;

		if	(
				(
					bShowStringsEnums	&&
					(
						pRepField->GetDataType() == DATA_STR_TYPE	||
						pRepField->GetDataType() == DATA_ENUM_TYPE
					)
				)	||
				(
					bShowDatesBools		&&
					(
						pRepField->GetDataType() == DATA_DATE_TYPE	||
						pRepField->GetDataType() == DATA_BOOL_TYPE
					)
				)	||
				(	bShowNumerics	&& IsNumericDataObj(pRepField->GetDataType()))
			)		 
			m_mlbFields.AddString(pRepField->GetName(), _T(""));
	}                 
	
	m_mlbFields.SetRedraw(TRUE);
	m_mlbFields.Invalidate(FALSE);
}

//----------------------------------------------------------------------------
void CBrkCondDlg::AddFieldInBreakList(LPCTSTR pszFieldName)
{
	int nNumItem = m_pBreakFieldList->GetSize();
	for (int i = 0; i < nNumItem; i++)
	{
		if (m_pBreakFieldList->GetAt(i).CompareNoCase(pszFieldName) == 0)
			return;
	}
	
	m_pBreakFieldList->Add(pszFieldName);
}

//----------------------------------------------------------------------------
void CBrkCondDlg::DelFieldInBreakList(LPCTSTR pszFieldName)
{
	int nNumItem = m_pBreakFieldList->GetSize();
	for (int i = 0; i < nNumItem; i++)
	{
		if (_tcsicmp(pszFieldName, m_pBreakFieldList->GetAt(i)) == 0)
		{
			m_pBreakFieldList->RemoveAt(i);
			return;
		}	
	}
}

//----------------------------------------------------------------------------
void CBrkCondDlg::OnDblClkFieldList()
{
	CString	strTmp;
	int		nCurrIdx = m_mlbFields.GetCurSel();
	
	if (nCurrIdx < 0) return;
	
	if (m_mlbFields.GetFlag(nCurrIdx))
	{
		m_mlbFields.SetFlag(nCurrIdx, CMultiListBox::UNCHECKED);
		strTmp = m_mlbFields.GetString1(nCurrIdx);
		DelFieldInBreakList(strTmp);
	}
	else
	{
		m_mlbFields.SetFlag(nCurrIdx, CMultiListBox::CHECK_ONE);
		strTmp = m_mlbFields.GetString1(nCurrIdx);
		AddFieldInBreakList(strTmp);
	}
}

//----------------------------------------------------------------------------
void CBrkCondDlg::OnOK()
{
	m_edtName.GetValue(*m_pNameBreakCond);
	
	if (m_pNameBreakCond->IsEmpty())
	{
		AfxMessageBox(_TB("No breaking event name."));
		return;
	}

	if (AfxGetTokensTable()->GetKeywordsToken(*m_pNameBreakCond) != T_NOTOKEN)
	{
		AfxMessageBox(_TB("The name collides with a reserved word of TaskBuilder"));
		m_edtName.SetFocus();
		return;
	}

	if (m_pEventData->GetEventIdx(*m_pNameBreakCond) >= 0)
	{
		AfxMessageBox(_TB("Event name already exists."));
		return;
	}

	if (m_pBreakFieldList->GetSize() <= 0)
	{
		AfxMessageBox(_TB("No breaking field defined."), MB_ICONSTOP);
		return;
	}

	EndDialog(IDOK);
}

//----------------------------------------------------------------------------
void CBrkCondDlg::OnCancel()
{
	m_pBreakFieldList->RemoveAll();
	EndDialog(IDCANCEL);
}

//===========================================================================
// CSubTotalDlg
//===========================================================================
IMPLEMENT_DYNAMIC(CSubTotalDlg, CParsedDialog)
BEGIN_MESSAGE_MAP(CSubTotalDlg, CParsedDialog)
	//{{AFX_MSG_MAP(CSubTotalDlg)
	ON_LBN_DBLCLK		(IDC_EVNDLG_SBT_FIELD_LIST,		AddSubtotalAction)
	ON_LBN_SELCHANGE	(IDC_EVNDLG_SBT_EVENT_LIST,		SelectEventAction)

	ON_BN_CLICKED		(IDC_EVNDLG_SBT_NUMERIC_FIELD,	SubTotalButtonAction)
	ON_BN_CLICKED		(IDC_EVNDLG_SBT_STRING_FIELD,	SubTotalButtonAction)
	ON_BN_CLICKED		(IDC_EVNDLG_SBT_OTHER_FIELD,	SubTotalButtonAction)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//----------------------------------------------------------------------------
CSubTotalDlg::CSubTotalDlg
	(
		EventsData*	 	pEventData,
		WoormTable* pSymTable
	)
	:
	CParsedDialog		(IDD_EVNDLG_SUBTOTAL),
	m_pEventData		(pEventData),
	m_pSymTable			(pSymTable),
	m_pCurTriggEvent	(NULL),
	m_mlbColumnField	(IDB_CHECK)
{}

//----------------------------------------------------------------------------
BOOL CSubTotalDlg::OnInitDialog()
{
	CParsedDialog::OnInitDialog();
	
	m_mlbColumnField.	SubclassDlgItem(IDC_EVNDLG_SBT_FIELD_LIST,		this);
	m_mlbColumnField.	SetStyle(CMultiListBox::SORT_ON_FIRST_STRING);

	m_lbCustomEvents.	SubclassDlgItem(IDC_EVNDLG_SBT_EVENT_LIST,		this);

	GetDlgItem(IDOK)->EnableWindow(FALSE);

	CheckDlgButton(IDC_EVNDLG_SBT_NUMERIC_FIELD,	TRUE);
	CheckDlgButton(IDC_EVNDLG_SBT_STRING_FIELD, 	FALSE);
	CheckDlgButton(IDC_EVNDLG_SBT_OTHER_FIELD, 		FALSE);

	// prima devono essere inseriti gli eventi e poi campi
	ShowEventList		();
	ShowColumnField		();

	return TRUE;
}

//----------------------------------------------------------------------------
void CSubTotalDlg::ShowEventList()
{
	CString s; 
	s = _TB("NEW EVENT");
	m_lbCustomEvents.AddString(s);

	for (int i = 0; i < m_pEventData->m_TriggEvents.GetSize(); i++)
	{
		TriggEventData*	pTriggEvent = (TriggEventData*) m_pEventData->m_TriggEvents[i];

		m_lbCustomEvents.AddString(pTriggEvent->GetEventName());
	}

	m_lbCustomEvents.SetCurSel(0);
}

//----------------------------------------------------------------------------
void CSubTotalDlg::ShowColumnField()
{
	BOOL numericFields = IsDlgButtonChecked(IDC_EVNDLG_SBT_NUMERIC_FIELD);
	BOOL stringFields  = IsDlgButtonChecked(IDC_EVNDLG_SBT_STRING_FIELD);
	BOOL otherFields   = IsDlgButtonChecked(IDC_EVNDLG_SBT_OTHER_FIELD);
	BOOL markFlag;

	for (int i = 0; i < m_pSymTable->GetSize(); i++)
	{
		WoormField* pRepField = (WoormField*) m_pSymTable->GetAt(i);

		if (!pRepField->IsColumn() || pRepField->IsHidden()) 
			continue;
		
		BOOL bNumericField = IsNumericDataObj(pRepField->GetDataType());
		if (!numericFields && bNumericField) 
			continue;

		if (!stringFields && pRepField->GetDataType() == DATA_STR_TYPE) 
			continue;

		//@@ TODO sarebbe bello mettere "SubTotali" descrittivi anche per le altre tipologie
		//   pero` bisogna rivedere la logica del wrmeng (vedi anche edtmng::AddAutoColTotal)
		if (!otherFields && !bNumericField && pRepField->GetDataType() != DATA_STR_TYPE) 
			continue;

		markFlag = ExistInTriggEvent(pRepField->GetName());

		m_mlbColumnField.AddString
				(
					pRepField->GetName(), pRepField->GetDataType().ToString(),
					markFlag ? CMultiListBox::CHECK_ONE : CMultiListBox::UNCHECKED
				);
	}
}

//----------------------------------------------------------------------------
void CSubTotalDlg::SubTotalButtonAction()
{
	m_mlbColumnField.SetRedraw(FALSE);
	m_mlbColumnField.ResetContent();

	ShowColumnField();

	m_mlbColumnField.SetRedraw(TRUE);
	m_mlbColumnField.Invalidate(FALSE);
}

//----------------------------------------------------------------------------
void CSubTotalDlg::AddSubtotalAction()
{
	CString m_strColumnName;

	int nCurrIdx = m_mlbColumnField.GetCurSel();
	m_strColumnName = m_mlbColumnField.GetString1(nCurrIdx);

	if (m_strColumnName.IsEmpty()) 			return;
	if (m_mlbColumnField.GetFlag(nCurrIdx))	return;

	m_NewSubtotals.Add(new SubtotalObject(m_pSymTable, m_strColumnName/*, TRUE  in before section )*/));

	m_mlbColumnField.SetFlag(nCurrIdx, CMultiListBox::CHECK_ONE);

	GetDlgItem(IDOK)->EnableWindow(TRUE);
	GetDlgItem(IDC_EVNDLG_SBT_NUMERIC_FIELD)->EnableWindow(FALSE);
	GetDlgItem(IDC_EVNDLG_SBT_STRING_FIELD)->EnableWindow(FALSE);
}

//----------------------------------------------------------------------------
BOOL CSubTotalDlg::ExistInTriggEvent(LPCTSTR pszFieldName)
{
	CString	strCurrEventName;

	int nCurrIdx = m_lbCustomEvents.GetCurSel();
	if (nCurrIdx == 0)
		return FALSE;

	m_lbCustomEvents.GetText(nCurrIdx, strCurrEventName);

	if (!strCurrEventName.IsEmpty())
	{
		m_pCurTriggEvent = m_pEventData->GetEvent(strCurrEventName);

		if (m_pCurTriggEvent)
		{
			CStringArray arraySubtotals;
			m_pCurTriggEvent->GetSubtotalFields(arraySubtotals);

			for (int i = 0; i < arraySubtotals.GetSize(); i++)
				if (_tcsicmp(pszFieldName, arraySubtotals[i]) == 0)
					return TRUE;
		}
	}
	
	return FALSE;
}

//----------------------------------------------------------------------------
void CSubTotalDlg::SelectEventAction()
{
	m_mlbColumnField.SetRedraw(FALSE);
	m_mlbColumnField.ResetContent();
	ShowColumnField();
	m_mlbColumnField.SetRedraw(TRUE);
	m_mlbColumnField.Invalidate(FALSE);

	m_NewSubtotals.RemoveAll();
}

//----------------------------------------------------------------------------
void CSubTotalDlg::AddSubtotalField()
{
	WoormField*	pColumnField;
	WoormField*	pSubtotalField;
	ASSERT_VALID(m_pCurTriggEvent);
	if (!m_pCurTriggEvent) 
		return;

	for (int i = 0; i < m_NewSubtotals.GetSize(); i++)
	{
		SubtotalObject* pSubtotal = (SubtotalObject*) m_NewSubtotals[i];

		pColumnField = m_pSymTable->GetField(pSubtotal->m_strColumnName);

		if (pColumnField)
		{
			pSubtotalField = new WoormField
				(
					pSubtotal->m_strSubtotalName,
					WoormField::FIELD_SUBTOTAL
				);

			pSubtotalField->SetLen		(pColumnField->GetLen());
			pSubtotalField->SetNumDec	(pColumnField->GetNumDec());
			pSubtotalField->SetHidden	(pColumnField->IsHidden());
			// Anyway the column is visible (see CSubTotalDlg::ShowColumnField())
			
			if (IsNumericDataObj(pColumnField->GetDataType()))
			{
				pSubtotalField->SetDataType	(pColumnField->GetDataType());
				pSubtotalField->SetEventFunction(m_pSymTable, pSubtotal->m_strColumnName, T_CSUM);
			}
			else if (pColumnField->GetDataType() == DataType::String)
			{
				// "SubTotali" descrittivi
				pSubtotalField->SetDataType	(DATA_STR_TYPE);
				CString strTmp(_TB("\"SubTotal\""));
				pSubtotalField->SetEventFunction(m_pSymTable, pSubtotal->m_strColumnName, strTmp, DATA_STR_TYPE);
			}				
			else 
			{
				// casi particolari
				pSubtotalField->SetDataType	(pColumnField->GetDataType());
				CString strTmp(pSubtotal->m_strColumnName);
				pSubtotalField->SetEventFunction(m_pSymTable, pSubtotal->m_strColumnName, strTmp, pColumnField->GetDataType());
			}

			m_pSymTable->Add (pSubtotalField);
		}
	}
}

//	Must be add two kind of actions :
//		the first that evaluate the pSubtotal value in each row;	
//		the second that display the pSubtotal and done a NextLine action;
//
//	Attention! if there are just a nextline action it isn't added.
//----------------------------------------------------------------------------
void CSubTotalDlg::AddDefaultAction()
{
	if (m_pCurTriggEvent == NULL) return;

	int nPosNextlineAction = m_pCurTriggEvent->GetIdxAction(ACT_NEXTLINE, TRUE /* in before section */);

	for (int i = 0; i < m_NewSubtotals.GetSize(); i++)
	{
		SubtotalObject* pSubtotal = (SubtotalObject*) m_NewSubtotals[i];

		m_pEventData->AddActionInAlways(ACT_EVAL, pSubtotal->m_strSubtotalName);

		m_pCurTriggEvent->AddAction
			(
				ACT_DISPLAY,
				pSubtotal->m_strSubtotalName,
				TRUE, // it is always in before section */
				nPosNextlineAction
			);

		m_pCurTriggEvent->AddAction
			(
				ACT_RESET,
				pSubtotal->m_strSubtotalName,
				TRUE, // it is always in before section */
				(nPosNextlineAction == -1 ? -1 : nPosNextlineAction + 1)
			);
	}

	if (nPosNextlineAction == -1 && m_NewSubtotals.GetSize() > 0)
		m_pCurTriggEvent->AddAction(ACT_NEXTLINE, NULL, TRUE /* in after section */);
}

//----------------------------------------------------------------------------
void CSubTotalDlg::OnOK()
{
	if (m_pCurTriggEvent == NULL)
	{
		CStringArray	strFieldsArray;
		CString			strNameBreak;
		int				nEventNumber = m_pEventData->m_TriggEvents.GetSize() + 1;

		CBrkCondDlg	brkDlg(m_pSymTable, &strFieldsArray, &strNameBreak, nEventNumber, m_pEventData);

		if (brkDlg.DoModal() != IDOK)
			return;

		m_pCurTriggEvent = new TriggEventData(*m_pSymTable, strNameBreak, strFieldsArray);
		m_pEventData->m_TriggEvents.Add(m_pCurTriggEvent); 
	}
	 
	AddSubtotalField();
	AddDefaultAction();
	EndDialog(IDOK);
}

//----------------------------------------------------------------------------
void CSubTotalDlg::OnCancel()
{
	EndDialog(IDCANCEL);
}
