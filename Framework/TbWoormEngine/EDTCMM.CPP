
#include "stdafx.h"

#include <ctype.h>

#include <TbNameSolver\TBNamespaces.h>

#include <TbGeneric\dib.h>
#include <TbGeneric\globals.h>
#include <TbGeneric\ParametersSections.h>

#include <TbGenlib\FunProto.h>
#include <TbGenlib\expr.h>
#include <TbGenlib\Baseapp.h>

#include <TbOledb\sqlcatalog.h>
#include <TbOledb\wclause.h>

#include "ActionsRepEngin.h"
#include "edtcmm.h"
#include "edtmng.h"
#include "eqnedit.h"
#include "disptbl.h"
#include "prgdata.h"
#include "ruledata.h"
#include "rpsymtbl.h"
#include "expdlg.h"

//................................. Resources
#include "edtcmm.hjson" //JSON AUTOMATIC UPDATE


//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif

static DOEXPRESSIONEDITORFUNCTION s_DoExpressionEditorFunctionPtr = NULL;

//----------------------------------------------------------------------------
void SetExpressionEditorFunction(DOEXPRESSIONEDITORFUNCTION ptr) 
{
	s_DoExpressionEditorFunctionPtr = ptr;
}

//----------------------------------------------------------------------------
void AddEventActionsKeywords(CStringArray& arKeywords)
{
	arKeywords.Add(cwsprintf(T_BEGIN));
	arKeywords.Add(cwsprintf(T_END));
	arKeywords.Add(cwsprintf(T_WHILE));
	arKeywords.Add(cwsprintf(T_DO));
	arKeywords.Add(cwsprintf(T_BREAK));
	arKeywords.Add(cwsprintf(T_CONTINUE));
	arKeywords.Add(cwsprintf(T_RETURN));
	arKeywords.Add(cwsprintf(T_QUIT));

	arKeywords.Add(cwsprintf(T_EVAL));
	arKeywords.Add(cwsprintf(T_RESET));

	arKeywords.Add(cwsprintf(T_DISPLAY));
	arKeywords.Add(cwsprintf(T_DISPLAY_TABLE_ROW));
	arKeywords.Add(cwsprintf(T_DISPLAY_FREE_FIELDS));

	arKeywords.Add(cwsprintf(T_INTERLINE));
	arKeywords.Add(cwsprintf(T_NEXTLINE));
	arKeywords.Add(cwsprintf(T_SPACELINE));
	arKeywords.Add(cwsprintf(T_FORMFEED));

	arKeywords.Add(cwsprintf(T_CALL));
	arKeywords.Add(cwsprintf(T_MESSAGE_BOX));
	arKeywords.Add(cwsprintf(T_ABORT));

	arKeywords.Add(cwsprintf(T_BREAKING));
	arKeywords.Add(cwsprintf(T_WHEN));

	arKeywords.Add(cwsprintf(T_BEFORE));
	arKeywords.Add(cwsprintf(T_AFTER));
}
//============================================================================
// 	Generic Functions
//============================================================================
//----------------------------------------------------------------------------
CString GenerateName(SymTable* pSymTable, LPCTSTR pszFromName, const CString& strPrefix)
{
	CString strTmpName(cwsprintf(strPrefix, pszFromName));
	CString	strNewName(strTmpName);
	int		i = 0;
	
	while (pSymTable->ExistField(strNewName))
	{                                                                 
		i++;
		strNewName = strTmpName + cwsprintf(_T("_%d"), pSymTable->GetCurId() + i);
    }                   
    
   	return strNewName;
}

//----------------------------------------------------------------------------
void ConvertCString(CString& str, ConversType conversType)
{
	if (conversType == LF_TO_CRLF)
	{
		AddCR(str);
	}
	else if (conversType == CRLF_TO_LF)
	{
		RemoveCR(str);
	}
	else 
		ASSERT (FALSE);
}

//----------------------------------------------------------------------------
BOOL IsNumericDataObj(const DataType& dataType)
{
	return dataType.IsNumeric();
}

//----------------------------------------------------------------------------
// get identifier from a combo box
//----------------------------------------------------------------------------
//----------------------------------------------------------------------------
CString GetIdentifier(CComboBox& owner)
{
	CString strName;
	GetIdentifier(owner, strName);
	
	return strName;
}
	
//----------------------------------------------------------------------------
BOOL GetIdentifier(CComboBox& owner, CString& strName)
{
	owner.GetWindowText(strName);
	if (strName.IsEmpty()) return FALSE;
	
	BOOL ok = TRUE;
	int nPos = LOWORD(owner.GetEditSel()) - 1;
		           
	if (nPos >= 0)
	{
		TCHAR nChar = strName[nPos];
	
		// control if the new character is valid
		ok = (_istalnum(nChar) || (nChar == _T('_'))) && ((nPos != 0) || !_istdigit(nChar));
			
		if (!ok)	// delete new char
		{
			strName = strName.Left(nPos) + strName.Right(strName.GetLength() - nPos - 1);
			owner.SetWindowText(strName);
		}
	}
			                        
	if (ok && nPos != 0)
	{
		// check for first char (i.e. deleting some chars it became a digit)
		nPos = 0;
		ok = !_istdigit(strName[nPos]);
	}
		
	if (!ok)
	{
		owner.SetFocus();
		owner.SetEditSel(nPos, nPos);
		MessageBeep(MB_ICONHAND);
	}
		
	return ok;
}

//============================================================================
// CEqnEdit
//============================================================================
IMPLEMENT_DYNCREATE (CEqnEdit, CResizableStrEdit)

BEGIN_MESSAGE_MAP(CEqnEdit, CResizableStrEdit)
	//{{AFX_MSG_MAP( CEqnEdit )
	ON_WM_KEYDOWN		()
	ON_WM_KEYUP			()
	ON_WM_LBUTTONDOWN	()
	ON_WM_LBUTTONUP		()
	ON_WM_SETFOCUS		()
	ON_WM_LBUTTONDBLCLK	()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//----------------------------------------------------------------------------
CEqnEdit::CEqnEdit()
	:
	m_pSymTable			(NULL),
	m_pRuleData			(NULL),
	m_pTblRule			(NULL),
	m_dwPrevCaretPos	(0xFFFFFFFF),
	m_psbParentDialog	(NULL),
	m_pCheckExpression	(NULL),
	m_pCheckObject		(NULL)
{
}

//----------------------------------------------------------------------------
CEqnEdit::CEqnEdit(SymTable* pSymTable, RuleDataArray* pRuleData)
	:
	m_pSymTable			(pSymTable),
	m_pRuleData			(pRuleData),
	m_pTblRule			(NULL),
	m_dwPrevCaretPos	(0xFFFFFFFF),
	m_psbParentDialog	(NULL),
	m_pCheckExpression	(NULL),
	m_pCheckObject		(NULL)
{
	// Inibisce la formattazione di default dello CStrEdit
	m_nFormatIdx = NO_FORMAT;

	if (pRuleData && pSymTable == NULL)
	{
		ASSERT(FALSE);
	}
}

//----------------------------------------------------------------------------
CEqnEdit::~CEqnEdit()
{}

//------------------------------------------------------------------------------
BOOL CEqnEdit::OnInitCtrl()
{
	VERIFY(CResizableStrEdit::OnInitCtrl());

	SetTabStops(8);
	return TRUE;
}

//------------------------------------------------------------------------------
void CEqnEdit::OnScrollCaret(UINT nChar)
{
	switch (nChar)
	{
		case VK_LBUTTON		:	case VK_RBUTTON		:	case VK_CANCEL		:
		case VK_MBUTTON		:	case VK_RETURN		:	case VK_SHIFT		:
		case VK_CONTROL		:	case VK_MENU		:	case VK_PAUSE		:
		case VK_CAPITAL		:	case VK_ESCAPE		:	case VK_SELECT		:
		case VK_PRINT		:	case VK_EXECUTE		:	case VK_SNAPSHOT	:
		case VK_INSERT		:	case VK_HELP		:	case VK_F1			:
		case VK_F2			:	case VK_F3			:	case VK_F4			:
		case VK_F5			:	case VK_F6			:	case VK_F7			:
		case VK_F8			:	case VK_F9			:	case VK_F10			:
		case VK_F11			:	case VK_F12			:	case VK_F13			:
		case VK_F14			:	case VK_F15			:	case VK_F16			:
		case VK_F17			:	case VK_F18			:	case VK_F19			:
		case VK_F20			:	case VK_F21			:	case VK_F22			:
		case VK_F23			:	case VK_F24			:	case VK_NUMLOCK		:
		case VK_SCROLL		:
			return;
	}

	DWORD dwPos = GetSel();
	if (m_dwPrevCaretPos == dwPos)
		return;

	int nCol;

	if (m_dwPrevCaretPos == 0xFFFFFFFF)
		nCol = HIWORD(dwPos);
	else
		if (LOWORD(dwPos) != LOWORD(m_dwPrevCaretPos))
			nCol = LOWORD(dwPos);
		else
			if (HIWORD(dwPos) != HIWORD(m_dwPrevCaretPos))
				nCol = HIWORD(dwPos);

	m_dwPrevCaretPos = dwPos;

	int nLine = LineFromChar(nCol);

	nCol -= LineIndex(nLine);

	if (m_psbParentDialog)
	{
		int nID = m_psbParentDialog->CommandToIndex(ID_EDTCMM_EQNEDIT_ROW_COL);
		if (nID >= 0)
		{
			CString msg = cwsprintf(ID_EDTCMM_EQNEDIT_ROW_COL, nLine + 1, nCol + 1);
			m_psbParentDialog->SetPaneText(nID, msg);
		}
	}
}

//-----------------------------------------------------------------------------
void CEqnEdit::OnLButtonDown(UINT nFlags, CPoint point)
{
	CResizableStrEdit::OnLButtonDown(nFlags, point);
	OnScrollCaret(0);
}

//-----------------------------------------------------------------------------
void CEqnEdit::OnLButtonUp(UINT nFlags, CPoint point)
{
	CResizableStrEdit::OnLButtonUp(nFlags, point);
	OnScrollCaret(0);
}

//-----------------------------------------------------------------------------
void CEqnEdit::OnKeyDown(UINT nChar, UINT nRepCnt, UINT nFlags)
{
	CResizableStrEdit::OnKeyDown(nChar, nRepCnt, nFlags);
	OnScrollCaret(nChar);
}

//-----------------------------------------------------------------------------
void CEqnEdit::OnKeyUp(UINT nChar, UINT nRepCnt, UINT nFlags)
{
	CResizableStrEdit::OnKeyUp(nChar, nRepCnt, nFlags);
	OnScrollCaret(nChar);
}

//-----------------------------------------------------------------------------
void CEqnEdit::OnSetFocus(CWnd* pWnd)
{
	CResizableStrEdit::OnSetFocus(pWnd);

	if (m_dwPrevCaretPos != 0xFFFFFFFF)
		SetSel(m_dwPrevCaretPos);
	else
		SetSel(0);

	OnScrollCaret(0);
}

//----------------------------------------------------------------------------
void CEqnEdit::OnLButtonDblClk(UINT, CPoint)
{
	CString strTmp;
	GetWindowText(strTmp);

	CString strOld(strTmp);

	CEqnEditDlg eqnEdit
		(
			GetParent(), 
			m_pSymTable, 
			strTmp, 
			GetSel(), 
			m_pRuleData, 
			m_pTblRule,
			m_arSqlTableInfo
		);

	//if (s_DoExpressionEditorFunctionPtr && s_DoExpressionEditorFunctionPtr(&m_arSqlTableInfo, m_pSymTable, strTmp, m_pCheckObject, m_pCheckExpression ) && strTmp != strOld)
	if (eqnEdit.DoModal() == IDOK && strTmp != strOld)
	{
		SetValue(strTmp);
		SetFocus();				//An.18707
		SetModifyFlag(TRUE);
		GetParent()->SEND_WM_COMMAND(GetDlgCtrlID(), EN_CHANGE, m_hWnd);
	}
}

//----------------------------------------------------------------------------
void CEqnEdit::SetDlgStatusBar(CStatusBar* psbParentDialog)
{
	m_psbParentDialog = psbParentDialog;
}

//----------------------------------------------------------------------------
void CEqnEdit::SetTableInfo(SqlTableInfo* pTableInfo, TblRuleData* pTblRule /* = NULL */)
{
	if (pTblRule && (pTableInfo == NULL || m_pSymTable == NULL))
	{
		ASSERT(FALSE);
		return;
	}

	m_arSqlTableInfo.RemoveAll();
	if (pTableInfo != NULL)
	   m_arSqlTableInfo.Add(pTableInfo);
	
	m_pTblRule		= pTblRule;
}

//----------------------------------------------------------------------------
void CEqnEdit::SetTableInfo(SqlTableInfoArray arTableInfo, TblRuleData* pTblRule /* = NULL */)
{
	if (pTblRule && (arTableInfo.GetSize() == 0 || m_pSymTable == NULL))
	{
		ASSERT(FALSE);
		return;
	}
	m_arSqlTableInfo.RemoveAll();	
	m_arSqlTableInfo.Append(arTableInfo);
	m_pTblRule		= pTblRule;
}


//----------------------------------------------------------------------------
BOOL CEqnEdit::IsEmpty()
{
	CString strTmp;
	GetWindowText(strTmp);
	strTmp.Trim();
	return strTmp.IsEmpty();
}


//============================================================================
// CExpEdit
//============================================================================
//----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE (CExpEdit, CEqnEdit)

//----------------------------------------------------------------------------
CExpEdit::CExpEdit()
	:
	CEqnEdit ()
{}

//----------------------------------------------------------------------------
CExpEdit::CExpEdit(SymTable* pSymTable, RuleDataArray* pRuleData /* = NULL */)
	:
	CEqnEdit (pSymTable, pRuleData)
{}

//----------------------------------------------------------------------------
CExpEdit::~CExpEdit()
{}

//-----------------------------------------------------------------------------
void CExpEdit::InsertInCString(CString& strTarget, LPCTSTR pszStr, int nPos, int nShift)
{
	CString	s;
	
	if (nPos >= 0)
	{
		s = strTarget.Left(nPos);
		s += pszStr;
		s += strTarget.Right( strTarget.GetLength() - nPos - nShift);
		strTarget = s;
	}
}

//-----------------------------------------------------------------------------
void CExpEdit::ChangePublicNames(CString& strWClause, BOOL bNative)
{   
	CString	strPhysicalName;
	CString strFieldName;

	Parser	lex(strWClause);
	int		nCount = 0;
	long	nCurrLine = 1;

	while (!lex.Bad() && !lex.LookAhead(T_EOF))
	{
		bNative = bNative || lex.LookAhead(T_NATIVE);
	
		if (nCurrLine != lex.GetCurrentLine())
		{
			nCurrLine = lex.GetCurrentLine();
			LPCTSTR pszTmp = (LPCTSTR) strWClause;
			for (long i = 1; i < nCurrLine; i++)
			{
				pszTmp = _tcschr(pszTmp, LF_CHAR);
				pszTmp++;
			}
			
			nCount = (int) (pszTmp - (LPCTSTR) strWClause);
		}

		if (!lex.LookAhead(T_ID))
		{
			// se inizia una nuova sotto-espressione si ritorna in modo NON nativo
			
			if (lex.LookAhead(T_ELSE) || lex.LookAhead(T_THEN))
				bNative = FALSE;
	
			lex.SkipToken();
			continue;
		}

		lex.ParseID(strFieldName);
		int nFieldNameLength = strFieldName.GetLength();

		// si verifica se e` il nome di una colonna della tabella corrente
		//
		if (!IsPhysicalName(strFieldName, &m_arSqlTableInfo))
		{
			// se non lo e` potrebbe essere il nome pubblico dato ad una colonna
			// selezionata dalla corrente rule (se esiste)
			//
			if (!m_pTblRule || !m_pTblRule->ExistPublicName(strFieldName))
				continue;

			// in tal caso si sostituisce il nome pubblico con il nome fisico
			//
			strFieldName = m_pTblRule->GetPhysicalNameOf(strFieldName);
		
			BOOL bExists = FALSE;
			for (int i = 0; i < m_arSqlTableInfo.GetSize(); i++)
				if (m_arSqlTableInfo.GetAt(i)->ExistColumn(strFieldName))
				{
					bExists = TRUE;
					break;
				}
			ASSERT (bExists);
		}
		else
		{
			// e` il nome di una colonna, ma se, come potrebbe (ad esempio e` scritto
			// senza qualificatore di tabella, ma la IsPhysicalName() ha trovato una
			// colonna con quel nome), e` anche il nome pubblico di un campo, si da`
			// priorita` a quest'ultimo e si lascia tutto com'e`
			//
			if (m_pSymTable && m_pSymTable->ExistField(strFieldName))
				continue;
		}

		// si decide se il nome debba essere sostituito con quello qualificato
		//
		if (bNative)
			strPhysicalName = strFieldName;
		else
			strPhysicalName = GetPhysicalName(strFieldName, &m_arSqlTableInfo);
		
		InsertInCString
		(
			strWClause,
			strPhysicalName,
			lex.GetCurrentPos() + nCount, 
			nFieldNameLength//strFieldName.GetLength()
		);
		
		nCount += strPhysicalName.GetLength() - strFieldName.GetLength();
	}
	
}

//----------------------------------------------------------------------------
BOOL CExpEdit::CheckWC(WClause&	aWClause)
{
	if (m_arSqlTableInfo.GetSize() == 0)
		return FALSE;
	
	CString	strWClause;
	GetWindowText(strWClause);

	ConvertCString(strWClause, CRLF_TO_LF);
	
	ChangePublicNames(strWClause, aWClause.IsNative());

	CString strTmp(strWClause);

	ConvertCString(strTmp, LF_TO_CRLF);
	SetWindowText(strTmp);
		
	Parser lex(strWClause);

	if (lex.LookAhead(T_EOF))
	{
		aWClause.Reset();
        return TRUE;
    }

	return aWClause.Parse(lex);
}

//----------------------------------------------------------------------------
BOOL CExpEdit::CheckExp(Expression& expr, const DataType& typeExpr, BOOL bCanBeEmpty)
{
	CString	strTmp;
	GetWindowText(strTmp);

	ConvertCString(strTmp, CRLF_TO_LF);

	return CheckExp(Parser(strTmp), expr, typeExpr, bCanBeEmpty);
}	

//----------------------------------------------------------------------------
BOOL CExpEdit::CheckExp(Parser& lex, Expression& expr, const DataType& typeExpr, BOOL bCanBeEmpty)
{
	if (lex.LookAhead(T_EOF))
		if (bCanBeEmpty)
		{
			expr.Reset();
	        return TRUE;
	    }
	    else
	    {
			AfxMessageBox(_TB("Empty expression not allowed"), MB_OK | MB_ICONEXCLAMATION);
	    	return FALSE;
	    }

	return expr.Parse(lex, typeExpr, TRUE);
}

//----------------------------------------------------------------------------
BOOL CExpEdit::CheckExp(const DataType& typeExpr)
{
	CString strExpr;
    GetWindowText(strExpr);
    ConvertCString(strExpr, CRLF_TO_LF);
    Parser lex(strExpr);
             
    Expression e(m_pSymTable);
    if (!CheckExp(lex, e, typeExpr))
    {
        SetCtrlFocus(TRUE);
        return FALSE;
    }
    
	DataObj* pObj = DataObj::DataObjCreate(typeExpr);

    if (!e.Eval(*pObj))
		return FALSE;

	return TRUE;
}

//=============================================================================
//			Class DispTableInfo implementation
//=============================================================================
//----------------------------------------------------------------------------
DispTableInfo::DispTableInfo	(EditorManager* pEditorManager)
	:
	m_pEditorManager	(pEditorManager)
{}

//----------------------------------------------------------------------------
void DispTableInfo::OnInitDialog(CDialog* pParent, BOOL bShow /*= TRUE*/)
{
	ASSERT_VALID(pParent);

	// nome di default della tabella grafica
	DisplayTables* pDispTable = m_pEditorManager->GetPrgData()->GetSymTable()->GetDisplayTables();
	ASSERT(pDispTable);
	m_strDispTableName = _TB("Table");
	m_strDispTableName += cwsprintf(_T("_%d"), pDispTable->GetCount() + 1);
	
	DataObj* pSetting = AfxGetSettingValue(snsTbWoormViewer, szWoormGeneralOptions, szDefaultTableRows, DataInt(m_nRowsNum), szTbWoormViewerSettingFileName);
	m_nRowsNum = pSetting ? *((DataInt*) pSetting) : m_nRowsNum;

	BOOL bOk = m_edtDispTableName.SubclassEdit(IDC_DISPTBL_DLG_TABLE_NAME, pParent);
	if (bOk)
	{
	m_edtDispTableName.SetCtrlStyle(m_edtDispTableName.GetCtrlStyle() | STR_STYLE_NO_EMPTY);
	m_edtDispTableName.SetValue	(m_strDispTableName);
	}

	bOk = m_edtRowsNum.SubclassEdit(IDC_DISPTBL_DLG_TABLE_ROWSNO, pParent);
	if (bOk)
	{
		m_edtRowsNum.SetRange(1, SHRT_MAX);
		m_edtRowsNum.SetValue	(m_nRowsNum);
	}
	ShowControls(pParent, bShow);
}

//----------------------------------------------------------------------------
void DispTableInfo::ShowControls(CDialog* pParent, BOOL bShow /*= TRUE*/)
{
	ASSERT_VALID(pParent);

	int nCmd = bShow ? SW_SHOW : SW_HIDE;

	pParent->GetDlgItem(IDC_DISPTBL_DLG_TABLE_NAME)->			ShowWindow(nCmd);
	pParent->GetDlgItem(IDC_DISPTBL_DLG_TABLE_ROWSNO)->			ShowWindow(nCmd);
	pParent->GetDlgItem(IDC_DISPTBL_DLG_TABLE_ROWSNO_LABEL)->	ShowWindow(nCmd);
	pParent->GetDlgItem(IDC_DISPTBL_DLG_TABLE_NAME_LABEL)->		ShowWindow(nCmd);
	pParent->GetDlgItem(IDC_DISPTBL_DLG_TABLE_GROUP)->			ShowWindow(nCmd);
}

//----------------------------------------------------------------------------
BOOL DispTableInfo::AreControlsVisible()
{
	return IsTBWindowVisible(&m_edtDispTableName);
}

//----------------------------------------------------------------------------
void DispTableInfo::SetDispTableName(LPCTSTR pszDispTableName)
{
	m_strDispTableName = pszDispTableName;
	m_edtDispTableName.SetValue	(m_strDispTableName);
}

//----------------------------------------------------------------------------
CString DispTableInfo::GetEditDispTableName()
{
	return m_edtDispTableName.GetValue();
}

//----------------------------------------------------------------------------
BOOL DispTableInfo::CheckControls()
{
	if (!m_edtDispTableName.IsValid() || !m_edtRowsNum.IsValid())
		return FALSE;

	CString strDispTableName = m_edtDispTableName.GetValue();

	DisplayTables* pDispTable = m_pEditorManager->GetPrgData()->GetSymTable()->GetDisplayTables();
	ASSERT(pDispTable);
	if (pDispTable && pDispTable->Exist(strDispTableName, DisplayTables::CURRENT_LAYOUT))
	{
		AfxMessageBox(_TB("Table already defined"));
		m_edtDispTableName.SetFocus();
		return FALSE;
	}

	m_strDispTableName = strDispTableName;
	m_nRowsNum = m_edtRowsNum.GetValue();

	return TRUE;
}
