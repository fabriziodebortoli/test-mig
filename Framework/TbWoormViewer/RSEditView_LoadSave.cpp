#include "stdafx.h"


#include <TbFrameworkImages\CommonImages.h>
#include <TbFrameworkImages\GeneralFunctions.h>

#include <TbGenlib/TBPropertyGrid.h>
#include <TbGenlib/BaseApp.h>

#include <TbOleDb/SqlCatalog.h>
#include <TbOleDb/SqlRec.h>
#include <TbOleDb/SqlAccessor.h>
#include <TbOleDb/SqlTable.h>
#include <TbGes/HotLink.h>
//#include <TbRadar/WrmRdrDoc.h>

#include <TbWoormEngine/rpsymtbl.h>
#include <TbWoormEngine/ActionsRepEngin.h>
#include <TbWoormEngine/ruledata.h>
#include <TbWoormEngine/events.h>
#include <TbWoormEngine/QueryObject.h>
#include <TbWoormEngine/prgdata.h>
#include <TbWoormEngine/edtmng.h>

#include "WoormDoc.h"
#include "WoormFrm.h"
#include "WoormDoc.h"

#include "RSEditView.h"
#include "RSEditorUI.h"

#include "RSEditorUI.hjson" //JSON AUTOMATIC UPDATE

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

///////////////////////////////////////////////////////////////////////////////

void CRSEditView::SetText(CString sText)
{
	sText.Trim();
	//sText.Replace(L"\n", L"\r\n");
	sText.Remove('\r');
	GetEditCtrl()->SetWindowText(sText);

	GetEditCtrl()->SetSel(0, 0);
	GetEditCtrl()->SetModified(FALSE);
}

CString CRSEditView::GetText()
{
	CString sText;
	GetEditCtrl()->GetWindowText(sText);
	sText.Trim();
	return sText;
}

//-----------------------------------------------------------------------------
void CRSEditView::LoadElement(CString* pText, BOOL* pbSaved/* = NULL*/)
{
	if(m_pEditTextRect)
		m_pEditTextRect->SetText(*pText);
	
	if (GetEditorFrame(FALSE))
	{
		GetEditorFrame(TRUE)->m_pToolTreeView->FillTreeForTextRect(this);

		SetLanguage(L"WRM", TRUE);

		ShowDiagnostic(L"");
	}
	//----------------------------

	ASSERT(pText);
	m_pbSaved = pbSaved; if (m_pbSaved) *m_pbSaved = FALSE;
	m_Context.SetPtrText(pText);

	SetText(*pText);
}

//-----------------------------------------------------------------------------
void CRSEditView::LoadElement(SymTable* pSymTable, Expression** ppExpr, DataType dtReturnType, BOOL bViewMode, BOOL* pbSaved/* = NULL*/, BOOL bAllowEmpty, CString descr)
{
	if (GetEditorFrame(FALSE))
	{
		SetLanguage(L"WRM", bViewMode);

		GetEditorFrame(TRUE)->m_pToolTreeView->FillTree(bViewMode, this);

		ShowDiagnostic(L"");
	}
	//----------------------------

	m_pbSaved = pbSaved; if (m_pbSaved) *m_pbSaved = FALSE;
	ASSERT(ppExpr);
	ASSERT_VALID(pSymTable);

	CString sExpr;
	BOOL bOwn = FALSE;
	if (*ppExpr)
	{
		ASSERT_VALID(*ppExpr);
		ASSERT_KINDOF(Expression, *ppExpr);
		ASSERT((*ppExpr)->GetSymTable()->GetRoot() == pSymTable->GetRoot());

		sExpr = (*ppExpr)->ToString();
	}
	else
	{
		bOwn = TRUE;
		*ppExpr = new Expression(pSymTable);
	}

	m_Context.SetExpr(pSymTable, ppExpr, dtReturnType, bViewMode);
	m_Context.m_bOwnObject = bOwn;
	m_Context.m_bAllowEmpty = bAllowEmpty;
	if (m_pEditTextRect) m_pEditTextRect->SetText(sExpr);

	if (GetEditorFrame(FALSE))
		GetEditorFrame(TRUE)->m_pDiagnosticView->m_edtErrors.SetWindowTextW(descr);

	SetText(sExpr);
}


//-----------------------------------------------------------------------------
void CRSEditView::LoadElement(WoormField* woormField, BOOL bViewMode, BOOL* pbSaved/* = NULL*/, BOOL bAllowEmpty, CString descr)
{
	if (GetEditorFrame(FALSE))
	{
		SetLanguage(L"WRM", bViewMode);

		GetEditorFrame(TRUE)->m_pToolTreeView->FillTree(bViewMode, this);

		ShowDiagnostic(L"");
	}
	//----------------------------
	SymTable* pSymTable = woormField->GetSymTable();
	EventFunction** ppEventFunc = &woormField->GetEventFunction();
	DataType dtReturnType = woormField->GetDataType();

	m_pbSaved = pbSaved; if (m_pbSaved) *m_pbSaved = FALSE;
	ASSERT(ppEventFunc);
	ASSERT_VALID(pSymTable);

	CString sExpr;
	BOOL bOwn = FALSE;
	if (*ppEventFunc)
	{
		ASSERT_VALID(*ppEventFunc);
		ASSERT_KINDOF(EventFunction, *ppEventFunc);
		ASSERT((*ppEventFunc)->GetSymTable()->GetRoot() == pSymTable->GetRoot());
		sExpr = (*ppEventFunc)->ToString();
	}
	else
	{
		bOwn = TRUE;
		*ppEventFunc = new EventFunction(pSymTable, *woormField);
	}

	m_Context.SetExpr(woormField, bViewMode);
	m_Context.m_bOwnObject = bOwn;
	m_Context.m_bAllowEmpty = bAllowEmpty;
	if (m_pEditTextRect) m_pEditTextRect->SetText(sExpr);

	if (GetEditorFrame(FALSE))
		GetEditorFrame(TRUE)->m_pDiagnosticView->m_edtErrors.SetWindowTextW(descr);

	SetText(sExpr);
}
//-----------------------------------------------------------------------------
void CRSEditView::LoadElementGroupingRule(SymTable* pSymTable, Expression** ppExpr, DataType dtReturnType, BOOL bViewMode, BOOL* pbSaved/* = NULL*/, BOOL bAllowEmpty, CString descr)
{
	if (GetEditorFrame(FALSE))
	{			
		SetLanguage(L"WRM", bViewMode);

		GetEditorFrame(TRUE)->m_pToolTreeView->FillTreeGroupingRule(this);

		ShowDiagnostic(L"");
	}
	//----------------------------

	m_pbSaved = pbSaved; if (m_pbSaved) *m_pbSaved = FALSE;
	ASSERT(ppExpr);
	ASSERT_VALID(pSymTable);

	CString sExpr;
	BOOL bOwn = FALSE;
	if (*ppExpr)
	{
		ASSERT_VALID(*ppExpr);
		ASSERT_KINDOF(Expression, *ppExpr);
		ASSERT((*ppExpr)->GetSymTable()->GetRoot() == pSymTable->GetRoot());
		sExpr = (*ppExpr)->ToString();
	}
	else
	{
		bOwn = TRUE;
		*ppExpr = new Expression(pSymTable);
	}

	m_Context.SetExpr(pSymTable, ppExpr, dtReturnType, bViewMode);
	m_Context.m_bOwnObject = bOwn;
	m_Context.m_bAllowEmpty = bAllowEmpty;

	if (GetEditorFrame(FALSE))
		GetEditorFrame(TRUE)->m_pDiagnosticView->m_edtErrors.SetWindowTextW(descr);

	SetText(sExpr);
}

//-----------------------------------------------------------------------------
void CRSEditView::LoadElement(SymTable* pSymTable, Block** ppBlock, BOOL bRaiseEvents, BOOL* pbSaved/* = NULL*/)
{
	if (GetEditorFrame(FALSE))
	{
		GetEditorFrame(TRUE)->m_pToolTreeView->FillTree(FALSE, this);

		SetLanguage(L"WRM", FALSE);

		ShowDiagnostic(L"");
	}
	//----------------------------

	m_pbSaved = pbSaved; if (m_pbSaved) *m_pbSaved = FALSE;
	ASSERT(ppBlock);
	ASSERT_VALID(pSymTable);

	BOOL bOwn = FALSE;
	if (*ppBlock)
	{
		ASSERT_VALID(*ppBlock);
		ASSERT_KINDOF(Block, *ppBlock);
		ASSERT((*ppBlock)->GetSymTable()->GetRoot() == pSymTable->GetRoot());
	}
	else
	{
		bOwn = TRUE;
		*ppBlock = new Block(NULL, dynamic_cast<WoormTable*>(pSymTable), NULL, bRaiseEvents);
		(*ppBlock)->SetForceBeginEnd();
	}

	m_Context.SetBlock(pSymTable, ppBlock, bRaiseEvents);
	m_Context.m_bOwnObject = bOwn;;

	CString sBlock;
	if (*ppBlock)
		sBlock = (*ppBlock)->Unparse();

	SetText(sBlock);
}

//-----------------------------------------------------------------------------
void CRSEditView::LoadElement(ExpRuleData* pRule, BOOL* pbSaved/* = NULL*/)
{
	if (GetEditorFrame(FALSE))
	{
		GetEditorFrame(TRUE)->m_pToolTreeView->FillTree(FALSE, this);

		SetLanguage(L"WRM", FALSE);

		ShowDiagnostic(L"");
	}
	//----------------------------

	m_pbSaved = pbSaved; if (m_pbSaved) *m_pbSaved = FALSE;
	ASSERT_VALID(pRule);
	ASSERT_VALID(pRule->GetSymTable());

	m_Context.SetExprRule(pRule->GetSymTable(), pRule);
	m_Context.m_bAllowEmpty = FALSE;
	
	CString sRule = pRule->Unparse(FALSE, FALSE);

	SetText(sRule);
}

//-----------------------------------------------------------------------------
BOOL CRSEditView::OpenExprRuleEditor(CNodeTree* pNode)
{
	WoormField*  pF = dynamic_cast<WoormField*>(pNode->m_pItemData);
	ASSERT_VALID(pF);
	if (!pF) return FALSE;

	ExpRuleData* pExprRule = dynamic_cast<ExpRuleData*>(GetDocument()->m_pEditorManager->GetRuleData()->GetRuleData(pF->GetName()));
	ASSERT_VALID(pExprRule);
	if (!pExprRule) return FALSE;

	if (GetEditorFrame(FALSE))
	{
		GetEditorFrame(TRUE)->m_pToolTreeView->FillTree(FALSE, this);

		SetLanguage(L"WRM", FALSE);
	}

	//NO, altrimenti perde il nodo! m_Context.SetExprRule(pExprRule->GetSymTable(), pExprRule);
	m_Context.m_pSymTable = pExprRule->GetSymTable();
	m_Context.m_pRuleExpr = pExprRule;

	m_Context.m_bAllowEmpty = FALSE;

	CString sRule = pExprRule->Unparse(FALSE, FALSE);

	SetText(sRule);
	return TRUE;
}

// ---------------------------------------------------------------------------- -
BOOL CRSEditView::LoadElementFromTree(CNodeTree* pNode, BOOL* pbSaved/* = NULL*/)
{
	ASSERT_VALID(pNode);

	SetText(L"");																										

	if (GetEditorFrame(FALSE))
		ShowDiagnostic(L"");

	m_pbSaved = pbSaved; if (m_pbSaved) *m_pbSaved = FALSE;
	m_Context.SetNodeTree(pNode);

	switch (pNode->m_NodeType)
	{
		case CNodeTree::ENodeType::NT_PROCEDURE:
		{
			OpenProcedureEditor(pNode);
			return TRUE;
		}
		case CNodeTree::ENodeType::NT_RULE_LOOP:
		{
			OpenRuleLoopEditor(pNode);
			return TRUE;
		}
		case CNodeTree::ENodeType::NT_RULE_NAMED_QUERY:
		{
			OpenRuleQueryEditor(pNode);
			return TRUE;
		}
		case CNodeTree::ENodeType::NT_NAMED_QUERY:
		{
			OpenQueryEditor(pNode);
			return TRUE;
		}
		//-------------------------------------------
		case CNodeTree::ENodeType::NT_RULE_QUERY_WHERE:
		case CNodeTree::ENodeType::NT_RULE_QUERY_HAVING:
		case CNodeTree::ENodeType::NT_RULE_QUERY_JOIN_ON:
		{
			OpenWhereClauseEditor(pNode);
			return TRUE;
		}

		case CNodeTree::ENodeType::NT_RULE_QUERY_GROUPBY:
		case CNodeTree::ENodeType::NT_RULE_QUERY_ORDERBY:
		{
			OpenOrderByClauseEditor(pNode);
			return TRUE;
		}

		case CNodeTree::ENodeType::NT_VARIABLE:
		{
			WoormField*  pF = dynamic_cast<WoormField*>(pNode->m_pItemData);
			ASSERT_VALID(pF);
			if (!pF) return FALSE;

			if (pF->IsTableRuleField() && pF->IsNativeColumnExpr())
			{
				return OpenCalcColumnEditor(pNode);
			}
			else if (pF->IsExprRuleField())
			{
				return OpenExprRuleEditor(pNode);
			}
			return FALSE;
		}

		case CNodeTree::ENodeType::NT_RULE_QUERY_FULL_TABLE:
		{
			OpenFullTableRuleEditor(pNode);
			return TRUE;
		}

		case CNodeTree::ENodeType::NT_TRIGGER_EVENT:
		{
			OpenBreakingEventEditor(pNode);
			return TRUE;
		}

		//------------------------------------------
		case CNodeTree::ENodeType::NT_TUPLE_GROUPING_ACTIONS:
		{
			OpenGroupActionsEditor(pNode);
			return TRUE;
		}
		case CNodeTree::ENodeType::NT_ROOT_TUPLE_RULES:
		{
			OpenGroupingRuleEditor(pNode);
			return TRUE;
		}

		//------------------------------------------
	
		case CNodeTree::ENodeType::NT_EXPR:
		{
			LoadElement(pNode->m_pSymTable, pNode->m_ppExpr, pNode->m_ReturnType, pNode->m_bViewMode);
			return TRUE;
		}

		case CNodeTree::ENodeType::NT_TUPLE_FILTER:
		case CNodeTree::ENodeType::NT_TUPLE_HAVING_FILTER:
		case CNodeTree::ENodeType::NT_TUPLE_GROUPING:
		{
			LoadElementGroupingRule(pNode->m_pSymTable, pNode->m_ppExpr, pNode->m_ReturnType, pNode->m_bViewMode);
			return TRUE;
		}
		
		case CNodeTree::ENodeType::NT_BLOCK:
		{
			LoadElement(pNode->m_pSymTable, pNode->m_ppBlock, pNode->m_bRaiseEvents);
			return TRUE;
		}
		default:
			;
	}
	return FALSE;
}

//-----------------------------------------------------------------------------
void CRSEditView::LoadFullReport1(CString sFileName, BOOL* pbSaved/* = NULL*/)
{
	if (!GetEditorFrame(FALSE)) return;

	//TODO verificare se manca set Context EM_FULL_REPORT
	m_pbSaved = pbSaved; if (m_pbSaved) *m_pbSaved = FALSE;
	m_Context.SetFullReport(sFileName);

	CString sText;
	if (!::LoadLineTextFile(sFileName, sText))
	{
		ASSERT(FALSE);
		GetParent()->PostMessage(WM_CLOSE);
		return ;
	}
	GetEditorFrame(TRUE)->SetWindowText(sFileName);

	GetEditorFrame(TRUE)->m_pToolTreeView->FillFullTextTree(this);

	SetLanguage(L"WRM", FALSE);

	SetText(sText);

	SetFocus();
	GetEditCtrl()->SetSel(1, 1);
}

void CRSEditView::LoadFullReport(CString sFileName, BOOL* pbSaved, CString sError, int nLine, int nPos)
{
	LoadFullReport1(sFileName, pbSaved);

	if (!sError.IsEmpty())
	{
		sError = _TB("The report has syntax errors. Now, you could try to correct them.\r\n") + sError;

		ShowDiagnostic(sError, nLine, nPos);
	}
	else ShowDiagnostic(_TB("Ready"));
}

//-----------------------------------------------------------------------------
void CRSEditView ::EnableStringPreview(BOOL bEnabled /* = TRUE */ )
{
	m_bStringPreviewEnabled = bEnabled;
	if (m_bStringPreviewEnabled && !m_pEditTextRect)
		m_pEditTextRect = new TextRect(CPoint(0, 0), GetDocument());

	if (GetEditorFrame(FALSE))
	{
		GetEditorFrame(TRUE)->EnablePreviewPanel();
	}
}

//-----------------------------------------------------------------------------
void CRSEditView::OpenProcedureEditor(CNodeTree* pNode)
{
	if (GetEditorFrame(FALSE))
	{
		GetEditorFrame(TRUE)->m_pToolTreeView->FillTree(FALSE, this);

		SetLanguage(L"WRM", FALSE);
	}
	//----------------------------

	//ProcedureData* pProcedures = (ProcedureData*)pNode->m_pParentItemData;
	//ASSERT_VALID(pProcedures);
	//ASSERT_KINDOF(ProcedureData, pProcedures);

	ProcedureObjItem* pProc = dynamic_cast<ProcedureObjItem*>(pNode->m_pItemData);
	ASSERT_VALID(pProc);
	if (!pProc) return;

	CString sProc = pProc->Unparse();
	//ConvertCString(sProc, LF_TO_CRLF);

	SetText(sProc);
}

//-----------------------------------------------------------------------------
void CRSEditView::OpenQueryEditor(CNodeTree* pNode)
{
	if (GetEditorFrame(FALSE))
	{
		GetEditorFrame(TRUE)->m_pToolTreeView->FillTreeForSql(NULL,this);

		SetLanguage(L"SQL", FALSE);

		GetEditorFrame(TRUE)->m_pMainToolBar->HideButton(ID_EDIT_CHECK, FALSE);
		GetEditorFrame(TRUE)->m_pMainToolBar->AdjustLayout();
	}
	//-----------------------

	//QueryObjectData* pQueries = (QueryObjectData*)pNode->m_pParentItemData;
	//ASSERT_VALID(pQueries);
	//ASSERT_KINDOF(QueryObjectData, pQueries);

	QueryObjItem* pQuery = dynamic_cast<QueryObjItem*>(pNode->m_pItemData);
	ASSERT_VALID(pQuery);
	if (!pQuery) return;

	CString sQry = pQuery->Unparse();

	SetText(sQry);
}

//-----------------------------------------------------------------------------
void CRSEditView::OpenRuleQueryEditor(CNodeTree* pNode)
{
	if (GetEditorFrame(FALSE))
	{
		GetEditorFrame(TRUE)->m_pToolTreeView->FillTreeForSql(NULL, this);

		SetLanguage(L"SQL", FALSE);

		GetEditorFrame(TRUE)->m_pMainToolBar->HideButton(ID_EDIT_CHECK, FALSE);
		GetEditorFrame(TRUE)->m_pMainToolBar->AdjustLayout();
	}
	//-----------------------

	//QueryObjectData* pQueries = (QueryObjectData*)pNode->m_pParentItemData;
	//ASSERT_VALID(pQueries);
	//ASSERT_KINDOF(QueryObjectData, pQueries);

	QueryRuleData* pRQuery = dynamic_cast<QueryRuleData*>(pNode->m_pItemData);
	ASSERT_VALID(pRQuery);
	if (!pRQuery) return;

	QueryObjItem* pQuery = pRQuery->GetQueryItem();
	ASSERT_VALID(pQuery);
	if (!pQuery) return;

	CString sQry = pQuery->Unparse();

	SetText(sQry);
}

//-----------------------------------------------------------------------------
void CRSEditView::OpenRuleLoopEditor(CNodeTree* pNode)
{
//TODO
 //ASSERT(FALSE);
}
//-----------------------------------------------------------------------------
void CRSEditView::OpenWhereClauseEditor(CNodeTree* pNode)
{
	TblRuleData* pTblRule = (TblRuleData*)pNode->m_pParentItemData;
	ASSERT_VALID(pTblRule);
	ASSERT_KINDOF(TblRuleData, pTblRule);

	if (GetEditorFrame(FALSE))
	{
		GetEditorFrame(TRUE)->m_pToolTreeView->FillTreeForSql(pTblRule,this);
		SetLanguage(L"SQL", FALSE);
	}

	WClause* pWClause = (WClause*)pNode->m_pItemData;
	ASSERT_VALID(pWClause);
	ASSERT_KINDOF(WClause, pWClause);

	CString sQry;
	if (pWClause && !pWClause->IsEmpty())
		sQry = pWClause->ToString();

	SetText(sQry);
}

//-----------------------------------------------------------------------------
void CRSEditView::OpenOrderByClauseEditor(CNodeTree* pNode)
{
	TblRuleData* pTblRule = (TblRuleData*)pNode->m_pItemData;
	ASSERT_VALID(pTblRule);
	ASSERT_KINDOF(TblRuleData, pTblRule);

	if (GetEditorFrame(FALSE))
	{
		GetEditorFrame(TRUE)->m_pToolTreeView->FillTreeForSql(pTblRule,this);

		SetLanguage(L"SQL", FALSE);
	}

	CString sColumnList;
	if (pNode->m_NodeType == CNodeTree::NT_RULE_QUERY_ORDERBY)
		sColumnList = pTblRule->m_strOrderBy;
	else if (pNode->m_NodeType == CNodeTree::NT_RULE_QUERY_GROUPBY)
		sColumnList = pTblRule->m_strGroupBy;
	else
	{
		ASSERT(FALSE);
		//GetParent()->PostMessage(WM_CLOSE);
		return;
	}

	SetText(sColumnList);
}

//-----------------------------------------------------------------------------
BOOL CRSEditView::OpenCalcColumnEditor(CNodeTree* pNode)
{
	WoormField*  pF = dynamic_cast<WoormField*>(pNode->m_pItemData);
	ASSERT_VALID(pF);
	if (!pF) return FALSE;

	TblRuleData* pTblRule = dynamic_cast<TblRuleData*>(pNode->m_pAncestorItemData);
	if (!pTblRule)
	{
		//sto selezionando la variabile passando dall'oggetto di layout e allora recupero il parent nelle rules dell'engine
		HTREEITEM engineHt = GetDocument()->FindRSTreeItemData(ERefreshEditor::Rules, pF);//GetDocument()->GetRSTree(ERefreshEditor::Rules)->GetParentItem(GetDocument()->FindRSTreeItemData(ERefreshEditor::Rules, wrmField));
		CNodeTree*pEngineNode = (CNodeTree*)GetDocument()->GetRSTree(ERefreshEditor::Rules)->GetItemData(engineHt);
		pTblRule = dynamic_cast<TblRuleData*>(pEngineNode->m_pAncestorItemData);
	
	}
	ASSERT_VALID(pTblRule);
	if (!pTblRule) return FALSE;

	DataFieldLink*  pObjLink = pTblRule->GetCalcColumn(pF->GetName());
	ASSERT_VALID(pObjLink);
	if (!pObjLink) return FALSE;

	if (GetEditorFrame(FALSE))
	{
		GetEditorFrame(TRUE)->m_pToolTreeView->FillTreeForSql(pTblRule, this);

		SetLanguage(L"SQL", FALSE);
	}

	SetText(pObjLink->m_strPhysicalName);
	return TRUE;
}

//-----------------------------------------------------------------------------
void CRSEditView::OpenFullTableRuleEditor(CNodeTree* pNode)
{
	TblRuleData* pTblRule = dynamic_cast<TblRuleData*>(pNode->m_pItemData);
	ASSERT_VALID(pTblRule);

	if (GetEditorFrame(FALSE))
	{
		GetEditorFrame(TRUE)->m_pToolTreeView->FillTreeForSql(pTblRule, this);

		SetLanguage(L"SQL", FALSE);

		GetEditorFrame(TRUE)->m_pDiagnosticView->UpdateWindow();

		GetEditorFrame(TRUE)->m_pMainToolBar->HideButton(ID_EDIT_CHECK, FALSE);
		GetEditorFrame(TRUE)->m_pMainToolBar->AdjustLayout();
	}

	CString sRule = pTblRule->Unparse();

	SetText(sRule);
}

//-----------------------------------------------------------------------------
//void CRSEditView::OpenEventBreakingListEditor(CNodeTree* pNode)
//{
//if (GetEditorFrame(FALSE))
//{
//	GetEditorFrame(TRUE)->m_pToolTreeView->FillTree(FALSE);
//
//	SetLanguage(L"WRM");
//}
//-----------------------
//
//	TriggEventData* pTriggerEvent = dynamic_cast<TriggEventData*>(pNode->m_pItemData);
//	ASSERT_VALID(pTriggerEvent);
//
//	CString sColumnList;
//	//ASSERT(pTriggerEvent->m_BreakList.GetSize());
//	if (pTriggerEvent->m_BreakList.GetSize())
//		sColumnList = pTriggerEvent->UnparseBreakList();
//
//	SetText(sColumnList);
//}

//-----------------------------------------------------------------------------
void CRSEditView::OpenBreakingEventEditor(CNodeTree* pNode)
{
	if (GetEditorFrame(FALSE))
	{
		GetEditorFrame(TRUE)->m_pToolTreeView->FillTree(FALSE,this);

		SetLanguage(L"WRM", FALSE);
	}
	//-----------------------

	TriggEventData* pTriggerEvent = dynamic_cast<TriggEventData*>(pNode->m_pItemData);
	ASSERT_VALID(pTriggerEvent);

	CString sEvent;
	sEvent = pTriggerEvent->Unparse();

	SetText(sEvent);

	CString sHelper = TriggEventData::GetEventPrototype();
	
	ShowDiagnostic(::AddCR(sHelper));
}

//-----------------------------------------------------------------------------
void CRSEditView::OpenGroupActionsEditor(CNodeTree* pNode)
{
	if (GetEditorFrame(FALSE))
	{
		GetEditorFrame(TRUE)->m_pToolTreeView->FillTreeForGroup(this);

		SetLanguage(L"WRM", FALSE);
	}
	//-----------------------

	GroupByData* pGroup = dynamic_cast<GroupByData*>(pNode->m_pItemData);
	ASSERT_VALID(pGroup);

	CString sActionsList = pGroup->UnparseActions();

	SetText(sActionsList);
}

//-----------------------------------------------------------------------------
void CRSEditView::OpenGroupingRuleEditor(CNodeTree* pNode)
{
	if (GetEditorFrame(FALSE))
	{
		GetEditorFrame(TRUE)->m_pToolTreeView->FillTreeForGroup(this);

		SetLanguage(L"WRM", FALSE);
	}
	//-----------------------

	GroupByData* pGroup = dynamic_cast<GroupByData*>(pNode->m_pItemData);
	ASSERT_VALID(pGroup);

	CString sGroupingRule = pGroup->Unparse();

	SetText(sGroupingRule);
}

//=============================================================================
void CRSEditView::ShowErrorText(CString sError)
{
	//AfxMessageBox(sErr);
	ASSERT_VALID(GetEditorFrame(TRUE)->m_pDiagnosticView);

	GetEditorFrame(TRUE)->m_pDiagnosticView->m_edtErrors.SetWindowText(sError);
}

// -----------------------------------------------------------------------------
void CRSEditView::ShowDiagnostic(Parser& lex, const CString& sAux, BOOL bSkipLine/* = FALSE*/)
{
	int nLine = lex.GetCurrentLine();
	int nCol = lex.GetCurrentPos();
	CString sError = lex.BuildErrMsg(TRUE);

	lex.ClearError();

	if (!sAux.IsEmpty())
		sError =  sAux + L"\r\n" + sError;

	ShowDiagnostic(sError, bSkipLine ? -1 : nLine, nCol);
}

void CRSEditView::ShowDiagnostic(CString sError, int line, int col)
{
	if (line > -1)
	{
		//sError += cwsprintf(L"\r\nline: %d - %d", line, col);
		sError += cwsprintf(L"\r\nline: %d", line);
	}

	ShowErrorText(sError);

	if (line > 1)
	{
		if (col < 0) col = 0;
		col += ::FindOccurence(GetEditCtrl()->GetText(), L"\n", line - 1, 0) + 1;

		GetEditCtrl()->SetSel(col, col);
		GetEditCtrl()->SelectLine(line);
	}
	else
	{
		GetEditCtrl()->SetSel(0, 0);
		//GetEditCtrl()->SelectLine(1);
	}
}

//-----------------------------------------------------------------------------
BOOL CRSEditView::DoClose()
{
	switch (this->m_Context.m_eType)
	{
		case CRSEditViewParameters::EditorMode::EM_EXPR:
		{
			if (m_Context.m_bAllowEmpty && m_Context.m_bOwnObject && (*m_Context.m_ppExpr) && (*m_Context.m_ppExpr)->IsEmpty())
				SAFE_DELETE((*m_Context.m_ppExpr));
			break;
		}

		case CRSEditViewParameters::EditorMode::EM_FUNC_EXPR:
		{
			if (m_Context.m_bAllowEmpty && m_Context.m_bOwnObject && (*m_Context.m_ppEventFunc) && (*m_Context.m_ppEventFunc)->IsEmpty())
				SAFE_DELETE((*m_Context.m_ppEventFunc));
			break;
		}

		case CRSEditViewParameters::EditorMode::EM_BLOCK:
		{
			if (m_Context.m_bAllowEmpty && m_Context.m_bOwnObject && (*m_Context.m_ppBlock) && (*m_Context.m_ppBlock)->IsEmpty())
				SAFE_DELETE((*m_Context.m_ppBlock));
			break;
		}
		case CRSEditViewParameters::EditorMode::EM_NODE_TREE:
		{
			ASSERT_VALID(m_Context.m_pNode);
			switch (m_Context.m_pNode->m_NodeType)
			{
				case CNodeTree::ENodeType::NT_RULE_QUERY_FULL_TABLE:
				{
					if (m_pbSaved && *m_pbSaved)
					{
						GetDocument()->RefreshRSTree(ERefreshEditor::Variables);
						GetDocument()->RefreshRSTree(ERefreshEditor::Rules, m_Context.m_pNode->m_pItemData);
					}
				}
			}
		}
	}
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CRSEditView::DoSave()
{
	switch (this->m_Context.m_eType)
	{
	case CRSEditViewParameters::EditorMode::EM_NODE_TREE:
		{
			ASSERT_VALID(m_Context.m_pNode);

			switch (m_Context.m_pNode->m_NodeType)
			{
				case CNodeTree::ENodeType::NT_PROCEDURE:
				{
					if (!SaveProcedure())
						return FALSE;
					break;
				}

				case CNodeTree::ENodeType::NT_NAMED_QUERY:
				{
					if (!SaveQuery())
						return FALSE;
					break;
				}

				case CNodeTree::ENodeType::NT_RULE_NAMED_QUERY:
				{
					if (!SaveRuleQuery())
						return FALSE;
					break;
				}
				case CNodeTree::ENodeType::NT_RULE_LOOP:
				{
					if (!SaveRuleLoop())
						return FALSE;
					break;
				}

				case CNodeTree::ENodeType::NT_TRIGGER_EVENT:
				{
					if (!SaveBreakingEvent())
						return FALSE;
					break;
				}
				//case CNodeTree::ENodeType::NT_EVENT_BREAKING_LIST:
				//{
				//	if (!SaveEventBreakingList())
				//		return FALSE;
				//	break;
				//}

				case CNodeTree::ENodeType::NT_TUPLE_GROUPING_ACTIONS:
				{
					if (!SaveGroupActionsList())
						return FALSE;
					break;
				}
				case CNodeTree::ENodeType::NT_ROOT_TUPLE_RULES:
				{
					if (!SaveGroupingRule())
						return FALSE;
					break;
				}

				case CNodeTree::ENodeType::NT_RULE_QUERY_GROUPBY:
				case CNodeTree::ENodeType::NT_RULE_QUERY_ORDERBY:
				{
					if (!SaveOrderByClause())
						return FALSE;
					break;
				}
				case CNodeTree::ENodeType::NT_VARIABLE:
				{
					WoormField*  pF = dynamic_cast<WoormField*>(m_Context.m_pNode->m_pItemData);
					ASSERT_VALID(pF);
					if (!pF) return FALSE;

					if (pF->IsTableRuleField() && pF->IsNativeColumnExpr())
					{
						TblRuleData* pTblRule = dynamic_cast<TblRuleData*>(m_Context.m_pNode->m_pAncestorItemData);
						if (!pTblRule)
						{
							//sto selezionando la variabile passando dall'oggetto di layout e allora recupero il parent nelle rules dell'engine
							HTREEITEM engineHt = GetDocument()->FindRSTreeItemData(ERefreshEditor::Rules, pF);//GetDocument()->GetRSTree(ERefreshEditor::Rules)->GetParentItem(GetDocument()->FindRSTreeItemData(ERefreshEditor::Rules, wrmField));
							CNodeTree*pEngineNode = (CNodeTree*)GetDocument()->GetRSTree(ERefreshEditor::Rules)->GetItemData(engineHt);
							pTblRule = dynamic_cast<TblRuleData*>(pEngineNode->m_pAncestorItemData);

						}
						ASSERT_VALID(pTblRule);
						if (!pTblRule) return FALSE;

						DataFieldLink*  pObjLink = pTblRule->GetCalcColumn(pF->GetName());
						ASSERT_VALID(pObjLink);
						if (!pObjLink) return FALSE;

						if (!SaveCalcColumn())
							return FALSE;
					}
					else if (pF->IsExprRuleField())
					{
						if (!SaveRuleExpression())
							return FALSE;
					}
					break;
				}

				case CNodeTree::ENodeType::NT_RULE_QUERY_FULL_TABLE:
				{
					if (!SaveFullTableRule())
						return FALSE;
					break;
				}

				case CNodeTree::ENodeType::NT_RULE_QUERY_WHERE:
				case CNodeTree::ENodeType::NT_RULE_QUERY_HAVING:
				case CNodeTree::ENodeType::NT_RULE_QUERY_JOIN_ON:
				{
					if (!SaveWClause())
						return FALSE;
					break;
				}

				default:
				{
					ASSERT(FALSE);
					return FALSE;
				}
			}

			ASSERT_VALID(m_Context.m_pNode);

			if (GetDocument()->GetWoormFrame()->m_pEditorDockedView)
				GetDocument()->GetWoormFrame()->m_pEditorDockedView->LoadElementFromTree(m_Context.m_pNode);

			break;
		}

		case CRSEditViewParameters::EditorMode::EM_TEXT:
		{
			ASSERT(m_Context.m_pText);
			*m_Context.m_pText = GetEditCtrl()->GetText();

			ShowDiagnostic(_TB("Text was saved"));
			break;
		}

		case CRSEditViewParameters::EditorMode::EM_FULL_REPORT:
		{
			if (!SaveFullReport())
				return FALSE;
			break;
		}

		case CRSEditViewParameters::EditorMode::EM_EXPR:
		{
			if (!SaveExpression())
				return FALSE;
			break;
		}

		case CRSEditViewParameters::EditorMode::EM_FUNC_EXPR:
		{
			if (!SaveFunction())
				return FALSE;
			break;
		}
		case CRSEditViewParameters::EditorMode::EM_BLOCK:
		{
			if (!SaveBlock())
				return FALSE;
			break;
		}

		case CRSEditViewParameters::EditorMode::EM_RULE_EXPR:
		{
			if (!SaveRuleExpression())
				return FALSE;
			break;
		}
		
		default:
		{
			ASSERT(FALSE);
			return FALSE;
		}
	}
	
	if (m_pbSaved)
		*m_pbSaved = TRUE;

	GetEditCtrl()->SetModified(FALSE);
	GetDocument()->SetModifiedFlag();
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CRSEditView::SaveExpression()
{
	ASSERT(m_Context.m_ppExpr);

	CString sExpr = GetEditCtrl()->GetText();
	sExpr.Trim();
	if (sExpr.IsEmpty())
	{
		if (m_Context.m_bAllowEmpty)
		{
			if (m_Context.m_bOwnObject)
			{ 
				SAFE_DELETE((*m_Context.m_ppExpr));
			}
			else
			{
				(*m_Context.m_ppExpr)->Empty();
			}
			ShowDiagnostic(_TB("Ok, empty expression allowed"));
			return TRUE;
		}
		else
		{
			ShowDiagnostic(_TB("Error, empty expression is not allowed"));
			return FALSE;
		}
	}

	Expression* pTempExpr = NULL;
	if (*m_Context.m_ppExpr && (*m_Context.m_ppExpr)->IsKindOf(RUNTIME_CLASS(ExpressionWithCheck)))
	{
		pTempExpr = new ExpressionWithCheck(m_Context.m_pSymTable);
		((ExpressionWithCheck*)pTempExpr)->m_bSkipCheck = ((ExpressionWithCheck*)*m_Context.m_ppExpr)->m_bSkipCheck;
	}
	else
		pTempExpr = new Expression(m_Context.m_pSymTable);

	Parser lex(sExpr);
	if (pTempExpr->Parse(lex, m_Context.m_eReturnType, TRUE))
	{
		if (*m_Context.m_ppExpr)
		{
			(*m_Context.m_ppExpr)->Empty();
			VERIFY((*m_Context.m_ppExpr)->Parse(sExpr, m_Context.m_eReturnType, TRUE));

			SAFE_DELETE(pTempExpr);
		}
		else
		{
			*m_Context.m_ppExpr = pTempExpr;
		}

		ShowDiagnostic(_TB("Expression was saved"));
		return TRUE;
	}
	else
	{
		ShowDiagnostic(lex, pTempExpr->GetErrDescription(FALSE), TRUE);

		SAFE_DELETE(pTempExpr);
	}
	return FALSE;
}

//-----------------------------------------------------------------------------
BOOL CRSEditView::SaveBlock()
{
	ASSERT(m_Context.m_ppBlock);

	CString sBlock = GetEditCtrl()->GetText();
	sBlock.Trim();
	if (sBlock.IsEmpty())
	{
		if (m_Context.m_bAllowEmpty)
		{
			if (m_Context.m_bOwnObject)
			{
				SAFE_DELETE(*m_Context.m_ppBlock);
			}
			else
				(*m_Context.m_ppBlock)->Empty();

			ShowDiagnostic(_TB("Empty command block was saved"));
			return TRUE;
		}
		else
		{
			ShowDiagnostic(_TB("Empty command block is not allowed"));
			return FALSE;
		}
	}

	Block* pTempBlock = new Block(NULL, m_Context.m_pSymTable, NULL, m_Context.m_bRaiseEvents);
	pTempBlock->SetForceBeginEnd((*m_Context.m_ppBlock)->GetForceBeginEnd());
	//An.EVAL FUNCTION - MessageBox di errore e apertura EditView 
	//pTempBlock->m_bOnEditing = TRUE;

	sBlock = L"Begin\n" + sBlock + L"\nEnd";
	Parser lex(sBlock);
	if (pTempBlock->Parse(lex))
	{
		if (*m_Context.m_ppBlock)
		{
			(*m_Context.m_ppBlock)->Empty();
			VERIFY((*m_Context.m_ppBlock)->Parse(sBlock));
		}
		else
		{
			*m_Context.m_ppBlock = pTempBlock;
		}

		ShowDiagnostic(_TB("Command block was saved"));
		return TRUE;
	}
	else
	{
		ShowDiagnostic(lex);

		SAFE_DELETE(pTempBlock);
	}
	return FALSE;
}

//-----------------------------------------------------------------------------
BOOL CRSEditView::SaveFunction()
{
	ASSERT(m_Context.m_ppEventFunc);

	CString sFunc = GetEditCtrl()->GetText();
	sFunc.Trim();
	if (sFunc.IsEmpty())
	{
		if (m_Context.m_bAllowEmpty)
		{
			if (m_Context.m_bOwnObject)
			{
				SAFE_DELETE((*m_Context.m_ppEventFunc));
			}
			else
			{
				(*m_Context.m_ppEventFunc)->Empty();
			}
			ShowDiagnostic(_TB("Ok, empty expression allowed"));
			return TRUE;
		}
		else
		{
			ShowDiagnostic(_TB("Error, empty expression is not allowed"));
			return FALSE;
		}
	}

	EventFunction* pTempFunc = NULL;
	pTempFunc = new EventFunction(m_Context.m_pSymTable, *m_Context.m_pWoormField);
	pTempFunc->SetPublicName(m_Context.m_pWoormField->GetName());
	Parser lex(sFunc);
	if (pTempFunc->Parse(lex))
	{

		(*m_Context.m_ppEventFunc)->Empty();
		(*m_Context.m_ppEventFunc)->SetPublicName(pTempFunc->GetPublicName());
		Parser lex2(sFunc);
		VERIFY((*m_Context.m_ppEventFunc)->Parse(lex2));

		SAFE_DELETE(pTempFunc);
		ShowDiagnostic(_TB("Function expression was saved"));

		if (
			m_Context.m_eType == CRSEditViewParameters::EditorMode::EM_NODE_TREE &&
			GetDocument() && GetDocument()->GetWoormFrame() &&
			::IsWindow(GetDocument()->GetWoormFrame()->m_hWnd) &&
			GetDocument()->GetWoormFrame()->m_pEditorDockedView
			)
		{
			GetDocument()->GetWoormFrame()->m_pEditorDockedView->LoadElementFromTree(m_Context.m_pNode);
		}
		return TRUE;
	}
	else
	{
		ShowDiagnostic(lex);

		SAFE_DELETE(pTempFunc);
	}
	return FALSE;
}

//-----------------------------------------------------------------------------
BOOL CRSEditView::SaveRuleExpression()
{
	ASSERT_VALID(m_Context.m_pRuleExpr);
	ASSERT_VALID(m_Context.m_pSymTable);

	CString sRule = GetEditCtrl()->GetText();
	sRule.Trim();
	if (sRule.IsEmpty())
	{
		if (m_Context.m_bAllowEmpty)
		{
			//TODO remove  m_Context.m_pRuleExpr and change WoormField
			return TRUE;
		}
		else
		{
			ShowDiagnostic(_TB("Empty rule expression is not allowed"));
			return FALSE;
		}
	}

	ExpRuleData* pTempRule = new ExpRuleData(*dynamic_cast<WoormTable*>(m_Context.m_pSymTable));
	pTempRule->m_bDisableUniqueConstraint = TRUE;
	pTempRule->m_strPublicName = m_Context.m_pRuleExpr->m_strPublicName;

	Parser lex(sRule);
	if (pTempRule->Parse(lex, FALSE))
	{
		if (pTempRule->GetPublicName() != m_Context.m_pRuleExpr->GetPublicName())
		{
			ShowDiagnostic(_TB("Changing the field name is not allowed"));

			SAFE_DELETE(pTempRule);
			return FALSE;
		}

		m_Context.m_pRuleExpr->Empty();
		m_Context.m_pRuleExpr->m_bDisableUniqueConstraint = TRUE;
		m_Context.m_pRuleExpr->m_strPublicName = pTempRule->m_strPublicName;
		Parser lex2(sRule);
		VERIFY(m_Context.m_pRuleExpr->Parse(lex2, FALSE));
		m_Context.m_pRuleExpr->m_bDisableUniqueConstraint = FALSE;

		SAFE_DELETE(pTempRule);
		ShowDiagnostic(_TB("Rule expression was saved"));

		if (
			m_Context.m_eType == CRSEditViewParameters::EditorMode::EM_NODE_TREE &&
			GetDocument() && GetDocument()->GetWoormFrame() &&
			::IsWindow(GetDocument()->GetWoormFrame()->m_hWnd) &&
			GetDocument()->GetWoormFrame()->m_pEditorDockedView
			)
		{
			GetDocument()->GetWoormFrame()->m_pEditorDockedView->LoadElementFromTree(m_Context.m_pNode);
		}
		return TRUE;
	}
	else
	{
		ShowDiagnostic(lex);

		SAFE_DELETE(pTempRule);
	}
	return FALSE;
}

//-----------------------------------------------------------------------------
BOOL CRSEditView::SaveFullReport()
{
	//Vedi WoormDocMng::DoSave2
	CString wrmText = GetEditCtrl()->GetText();

	CString filepath = GetDocument()->GetFilePath();

	if (!GetDocument()->PrepareFileNameForSave(filepath))
		return FALSE;

	CLineFile file;
	file.Open(filepath, CFile::modeCreate | CFile::modeWrite | CFile::shareDenyRead | CFile::typeText);
	file.WriteString(wrmText);
	file.Close();

	if (m_pbSaved)
		*m_pbSaved = TRUE;

	ShowDiagnostic(cwsprintf(_TB("The report {0-%s} was saved succesfully"), (LPCTSTR)filepath));
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CRSEditView::SaveProcedure()
{
	CString sProc = GetEditCtrl()->GetText();
	sProc.Trim();
	sProc = L"Begin\n" + sProc + L"\nEnd";
	Parser	lex(sProc);

	ASSERT_VALID(m_Context.m_pNode);
	ProcedureObjItem* pOldProc = dynamic_cast<ProcedureObjItem*>(m_Context.m_pNode->m_pItemData);
	ASSERT_VALID(pOldProc);
	ProcedureObjItem* pNewProcObj = new ProcedureObjItem(dynamic_cast<WoormTable*>(pOldProc->GetSymTable()));

	CFunctionDescription* fp = pOldProc->m_pProcedure->GetFun();
	if (fp)
	{
		CFunctionDescription* f = new CFunctionDescription(*fp);
		pNewProcObj->m_pProcedure->SetFun(f);
	}
	pNewProcObj->SetName(pOldProc->GetName());

	if (!pNewProcObj->Parse(lex))
	{
		ShowDiagnostic(lex);

		delete pNewProcObj;
		return FALSE;
	}

	ProcedureData*	pProcedures = dynamic_cast<ProcedureData*>(m_Context.m_pNode->m_pParentItemData);
	ASSERT_VALID(pProcedures);

	int nProcIdx = pProcedures->GetIndex(pOldProc->GetName());
	pProcedures->Delete(nProcIdx);
	m_Context.m_pNode->m_pItemData = pNewProcObj;
	pProcedures->InsertAt(nProcIdx, pNewProcObj);

	GetDocument()->SetModifiedFlag();

	ShowDiagnostic(_TB("Procedure was saved"));
    return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CRSEditView::SaveQuery()
{
	CString sQry = GetEditCtrl()->GetText();
	sQry.Trim();
	BOOL bBeginWithSelect = ::FindWord(sQry, L"SELECT") == 0;

	QueryObjItem* pOldQry = dynamic_cast<QueryObjItem*>(m_Context.m_pNode->m_pItemData);
	ASSERT_VALID(pOldQry);
	ASSERT_VALID(pOldQry->GetSymTable());

	QueryObjItem* pNewQry = new QueryObjItem(dynamic_cast<WoormTable*>(pOldQry->GetSymTable()), pOldQry->GetName());

	if (!pNewQry->Parse(sQry))
	{
		ShowDiagnostic(pNewQry->GetError());

		delete pNewQry;
		return FALSE;
	}

	QueryObjectData* pQueries = dynamic_cast<QueryObjectData*>(m_Context.m_pNode->m_pParentItemData);
	ASSERT_VALID(pQueries);
	if (!pQueries) return FALSE;

	int nIdx = pQueries->GetIndex(pOldQry->GetName());

	CRSTreeCtrl* tree = GetDocument()->GetRSTree(ERefreshEditor::Rules);
	CNodeTree* node = tree->FindNode(pOldQry, tree->m_htRules);
	if (node)
	{
		node->m_pItemData = pNewQry->GetQueryObject();
	}
	m_Context.m_pNode->m_pItemData = pNewQry;

	pQueries->Delete(nIdx);
	pQueries->InsertAt(nIdx, pNewQry);

	GetDocument()->SetModifiedFlag();

	CString msg;
	if (bBeginWithSelect && pNewQry->m_Query.AllQueryColumns().GetSize() == 0)
	{
		msg = _TB("Named query with SELECT sql clause has to be binded to report variables by tag { COL var-name }");
		msg += L"\r\n";
	}

	msg += _TB("The named query looks correct, but it contains native parts that will be validated on run by the database.");
	msg += L"\r\n";
	msg += _TB("Named query was saved");

	ShowDiagnostic(msg);
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CRSEditView::SaveRuleQuery()
{
	CString sQry = GetEditCtrl()->GetText();
	sQry.Trim();
	BOOL bBeginWithSelect = ::FindWord(sQry, L"SELECT") == 0;
	sQry = L"Begin\n{\n" + sQry + L"\n}\nEnd";
	Parser	lex(sQry);

	QueryRuleData* pRQry = dynamic_cast<QueryRuleData*>(m_Context.m_pNode->m_pItemData);
	ASSERT_VALID(pRQry);
	ASSERT_VALID(pRQry->GetSymTable());

	QueryObjItem* pNewQry = new QueryObjItem(dynamic_cast<WoormTable*>(pRQry->GetSymTable()), pRQry->GetQueryName());

	if (!pNewQry->Parse(lex))
	{
		ShowDiagnostic(lex);

		delete pNewQry;
		return FALSE;
	}

	QueryObjectData* pQueries = GetDocument()->m_pEditorManager->GetPrgData()->GetQueryObjectData();
	ASSERT_VALID(pQueries);
	if (!pQueries) return FALSE;

	int nIdx = pQueries->GetIndex(pRQry->GetQueryName());

	CRSTreeCtrl* tree = GetDocument()->GetRSTree(ERefreshEditor::Queries);
	CNodeTree* node = tree->FindNode(pRQry->GetQueryItem(), tree->m_htQueries);
	if (node)
	{
		node->m_pItemData = pNewQry;
	}
	pRQry->SetQueryItem(pNewQry);

	pQueries->Delete(nIdx);
	pQueries->InsertAt(nIdx, pNewQry);

	GetDocument()->SetModifiedFlag();

	CString msg;
	if (bBeginWithSelect && pNewQry->m_Query.AllQueryColumns().GetSize() == 0)
	{
		msg = _TB("Named query with SELECT sql clause has to be binded to report variables by tag { COL var-name }");
		msg += L"\r\n";
	}

	msg += _TB("The named query looks correct, but it contains native parts that will be validated on run by the database.");
	msg += L"\r\n";
	msg += _TB("Named query was saved");

	ShowDiagnostic(msg);
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CRSEditView::SaveRuleLoop()
{
//TODO
	ASSERT(FALSE);
	return FALSE;
}
//-----------------------------------------------------------------------------
//BOOL CRSEditView::SaveEventBreakingList()
//{
//	CString sFieldList = GetEditCtrl()->GetText();
//	sFieldList.Trim();
//	Parser lex(sFieldList);
//
//	TriggEventData* pTriggerEvent = dynamic_cast<TriggEventData*>(m_Context.m_pNode->m_pItemData);
//	ASSERT_VALID(pTriggerEvent);
//
//	TriggEventData t (pTriggerEvent->m_SymTable);
//	if (!t.ParseBreakList(lex))
//	{
//		int nLine = lex.GetCurrentLine();
//		int nCol = lex.GetCurrentPos();
//		CString sError = lex.BuildErrMsg(TRUE);
//
//		ShowDiagnostic(sError, nLine, nCol);
//		return FALSE;
//	}
//
//	Parser	lex1(sFieldList);
//	VERIFY(pTriggerEvent->ParseBreakList(lex1));
//
//	GetDocument()->SetModifiedFlag();
//	ShowDiagnostic(_TB("Breaking fields list was saved"));
//	return TRUE;
//}

//-----------------------------------------------------------------------------
BOOL CRSEditView::SaveBreakingEvent()
{
	CString sEvent = GetEditCtrl()->GetText();
	sEvent.Trim();
	Parser lex(sEvent);

	TriggEventData* pTriggerEvent = dynamic_cast<TriggEventData*>(m_Context.m_pNode->m_pItemData);
	ASSERT_VALID(pTriggerEvent);

	TriggEventData t (pTriggerEvent->m_SymTable);
	if (!t.Parse(lex))
	{
		ShowDiagnostic(lex);
		return FALSE;
	}

	pTriggerEvent->Empty();
	Parser	lex1(sEvent);
	VERIFY(pTriggerEvent->Parse(lex1));

	GetDocument()->SetModifiedFlag();
	ShowDiagnostic(_TB("Breaking event was saved"));

	//TODO aggiornare solo il tree edge dell'evento corrente
		//EventsData* pEventsData = dynamic_cast<EventsData*>(m_Context.m_pNode->m_pParentItemData);
		//ASSERT_VALID(pEventsData);
		//CRSTreeCtrl* pTreeCtrl = GetDocument()->GetRSTree(ERefreshEditor::Events);
		//pTreeCtrl->FillEvents(pEventsData, pTriggerEvent, TRUE);
	GetDocument()->RefreshRSTree(ERefreshEditor::Events, pTriggerEvent);

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CRSEditView::SaveGroupActionsList()
{
	CString sGroupListActions = GetEditCtrl()->GetText();
	sGroupListActions.Trim();

	GroupByData* pGroup = dynamic_cast<GroupByData*>(m_Context.m_pNode->m_pItemData);
	ASSERT_VALID(pGroup);

	if (!sGroupListActions.IsEmpty())
	{
		sGroupListActions = L"Begin\n" + sGroupListActions + L"\nEnd";
		Parser lex(sGroupListActions);

		GroupByData tempGroup(pGroup->m_SymTable);
		if (!tempGroup.ParseActionsBlock(lex))
		{
			ShowDiagnostic(lex);
			return FALSE;
		}

		Parser	lex1(sGroupListActions);
		VERIFY(pGroup->ParseActionsBlock(lex1));
	}
	else
		pGroup->m_ActionsArray.RemoveAll();

	GetDocument()->SetModifiedFlag();
	ShowDiagnostic(_TB("Group actions list was saved"));

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CRSEditView::SaveGroupingRule()
{
	CString sGroup = GetEditCtrl()->GetText();
	sGroup.Trim();
	Parser lex(sGroup);

	GroupByData* pGroup = dynamic_cast<GroupByData*>(m_Context.m_pNode->m_pItemData);
	ASSERT_VALID(pGroup);

	GroupByData tempGroup(pGroup->m_SymTable);
	if (!tempGroup.Parse(lex))
	{
		ShowDiagnostic(lex);
		return FALSE;
	}

	pGroup->Empty();

	Parser	lex1(sGroup);
	VERIFY(pGroup->Parse(lex1));

	GetDocument()->SetModifiedFlag();
	ShowDiagnostic(_TB("Grouping event was saved"));

	GetDocument()->RefreshRSTree(ERefreshEditor::RuleGroup, pGroup);
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CRSEditView::SaveFullTableRule()
{
//TODO check same columns

	CString sRule = GetEditCtrl()->GetText();
	sRule.Trim();
	Parser lex(sRule);

	TblRuleData* pTblRule = (TblRuleData*) m_Context.m_pNode->m_pItemData;
	ASSERT_VALID(pTblRule);
	ASSERT_KINDOF(TblRuleData, pTblRule);

	TblRuleData newRule(pTblRule->m_SymTable, pTblRule->m_pSqlConnection);
	if (!newRule.Parse(lex, FALSE, TRUE))
	{
		ShowDiagnostic(lex);
		return FALSE;
	}

	//------------------------------------------
	// Gestione cambio colonne selezionate
	CStringArray arOldColumns;
	pTblRule->GetColumns(arOldColumns);

	CStringArray arNewColumns;
	newRule.GetColumns(arNewColumns);

	if (arNewColumns.GetSize() == 0)
	{
		ShowDiagnostic(_TB("Query Rule has to be select at least one column"));
		return FALSE;
	}

	for (int oi = arOldColumns.GetUpperBound(); oi >= 0 && arNewColumns.GetSize() > 0; oi--)
	{
		CString sCol = arOldColumns[oi];
		int ni = CStringArray_Find(arNewColumns, sCol);
		if (ni >= 0)
		{
			arNewColumns.RemoveAt(ni);
			arOldColumns.RemoveAt(oi);
		}
	}
	//in arOldColumns ci sono delle colonne non più estratte dalla query
	for (int oi = 0; oi  < arOldColumns.GetSize(); oi++)
	{
		CString sCol = arOldColumns[oi];

		WoormField* pF = pTblRule->GetSymTable()->GetField(sCol);
		ASSERT_VALID(pF);
		if (!pF) continue;

		pF->SetTableRuleField(FALSE);
	}
	//in arNewColumns ci sono delle nuove colonne estratte dalla query
	for (int ni = 0; ni < arNewColumns.GetSize(); ni++)
	{
		CString sCol = arNewColumns[ni];

		DataType dt = newRule.GetTypeOf(sCol);

		WoormField* pF = new WoormField(sCol, WoormField::FIELD_NORMAL, dt);
			pF->SetHidden(TRUE);
			pF->SetTableRuleField(TRUE);

		pTblRule->GetSymTable()->Add(pF);
	}
	//------------------------------------------

	pTblRule->Empty();
	Parser	lex1(sRule);
	VERIFY(pTblRule->Parse(lex1, FALSE, FALSE));

	if (arOldColumns.GetSize() || arNewColumns.GetSize())
		GetDocument()->RefreshRSTree(ERefreshEditor::Variables);

	//TODO aggiornare solo il tree edge della table rule corrente
	GetDocument()->RefreshRSTree(ERefreshEditor::Rules, pTblRule);

	GetDocument()->SetModifiedFlag();

	CString msg;
	if (pTblRule->IsNativeQuery() || ::FindWord(sRule, L"ContentOf") > -1)
		msg = _TB("The query looks correct, but it contains native parts that will be validated on run by the database.");
	else
		msg = _TB("The query is correct.");

	msg += L"\r\n" + _TB("Query Rule was saved");

	ShowDiagnostic(msg);
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CRSEditView::SaveOrderByClause()
{
	CString sOrderByClause = GetEditCtrl()->GetText();
	sOrderByClause.Trim();

	TblRuleData* pTblRule = (TblRuleData*)m_Context.m_pNode->m_pItemData;
	ASSERT_VALID(pTblRule);
	ASSERT_KINDOF(TblRuleData, pTblRule);
	
	CString sColumnList;
	if (!sOrderByClause.IsEmpty())
	{
		Parser lex(sOrderByClause);
		if (!::ParseOrderBy(lex, pTblRule->GetSymTable(), &pTblRule->m_arSqlTableJoinInfoArray, sColumnList))
		{
			ShowDiagnostic(lex);
			return FALSE;
		}
	}

	CString msg2;

	if (m_Context.m_pNode->m_NodeType == CNodeTree::ENodeType::NT_RULE_QUERY_ORDERBY)
	{
		pTblRule->m_strOrderBy = sColumnList;

		msg2 = _TB("Order by sql clause was saved");
	}
	else if (m_Context.m_pNode->m_NodeType == CNodeTree::ENodeType::NT_RULE_QUERY_GROUPBY)
	{
		for (int i = 0; i < pTblRule->m_arSqlTableJoinInfoArray.m_arFieldLinks.GetSize(); i++)
		{
			DataFieldLinkArray* parLinks = pTblRule->m_arSqlTableJoinInfoArray.m_arFieldLinks[i];
			for (int j = 0; j < parLinks->GetSize(); j++)
			{
				DataFieldLink* pLink = (*parLinks)[j];

				CString columnSelected = pLink->m_strPhysicalName;

				if (::FindWord(sColumnList, columnSelected) == -1)
				{
					ShowDiagnostic(_TB("Selected columns must be in Group By clause or in an aggregate function: ") + columnSelected);
					return FALSE;
				}
			}
		}

		pTblRule->m_strGroupBy = sColumnList;
		
		msg2 = _TB("Group by sql clause was saved");
	}
	else
	{
		ASSERT(FALSE);
		return FALSE;
	}

	CString msg;
	if (::FindWord(sColumnList, L"ContentOf") > -1)
	{
		msg = _TB("The sql clause looks correct, but it contains native parts that will be validated on run by the database.");
		msg += L"\r\n";
	}

	ShowDiagnostic(msg + msg2);

	GetDocument()->SetModifiedFlag();
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CRSEditView::SaveWClause()
{
	CString sWClause = GetEditCtrl()->GetText();
	sWClause.Trim();

	TblRuleData* pTblRule = (TblRuleData*)m_Context.m_pNode->m_pParentItemData;
	ASSERT_VALID(pTblRule);
	ASSERT_KINDOF(TblRuleData, pTblRule);

	WClause* pWClause = (WClause*)m_Context.m_pNode->m_pItemData;
	ASSERT_VALID(pWClause);
	ASSERT_KINDOF(WClause, pWClause);

	if (sWClause.IsEmpty())
	{
		pWClause->Reset(FALSE); //pWClause->SetTableInfo(&pTblRule->m_arSqlTableJoinInfoArray); pWClause->SetSymTable(&pTblRule->m_SymTable);
		GetDocument()->SetModifiedFlag();
		return TRUE;
	}

	WClause w(pTblRule->m_pSqlConnection, &pTblRule->m_SymTable, &pTblRule->m_arSqlTableJoinInfoArray);
	w.SetClauseType(pWClause->GetClauseType());
	
	Parser lex(sWClause);
	if (!w.Parse(lex))
	{
		ShowDiagnostic(lex);
		return FALSE;
	}

	pWClause->Reset(FALSE); 
	Parser	lex1(sWClause);
	VERIFY(pWClause->Parse(lex1));

	GetDocument()->SetModifiedFlag();

	CString msg;
	if (pWClause->IsNative() || ::FindWord(sWClause, L"ContentOf") > -1)
	{
		msg = _TB("The sql clause looks correct, but it contains native parts that will be validated on run by the database.");
		msg += L"\r\n" ;
	}

	msg += _TB("Sql clause was saved");

	ShowDiagnostic(msg);
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CRSEditView::SaveCalcColumn()
{
	WoormField*  pF = dynamic_cast<WoormField*>(m_Context.m_pNode->m_pItemData);
	ASSERT_VALID(pF);
	ASSERT(pF->IsTableRuleField());
	ASSERT(pF->IsNativeColumnExpr());

	TblRuleData* pTblRule = dynamic_cast<TblRuleData*>(m_Context.m_pNode->m_pAncestorItemData);
	ASSERT_VALID(pTblRule);

	DataFieldLink*  pObjLink = pTblRule->GetCalcColumn(pF->GetName());
	ASSERT_VALID(pObjLink);

	CString sCalcColumnExpr = GetEditCtrl()->GetText();
	sCalcColumnExpr.Trim();

	pObjLink->m_strPhysicalName = sCalcColumnExpr;

	GetDocument()->SetModifiedFlag();

	CString msg;
	msg = _TB("Calculated column syntax will be validated on run by the database.");
	msg += L"\r\n" + _TB("Calculated column sql clause was saved");	

	ShowDiagnostic(msg);

	if (
		m_Context.m_eType == CRSEditViewParameters::EditorMode::EM_NODE_TREE &&
		GetDocument() && GetDocument()->GetWoormFrame() &&
		::IsWindow(GetDocument()->GetWoormFrame()->m_hWnd) &&
		GetDocument()->GetWoormFrame()->m_pEditorDockedView
		)
	{
		GetDocument()->GetWoormFrame()->m_pEditorDockedView->LoadElementFromTree(m_Context.m_pNode);
	}

	return TRUE;
}

// ----------------------------------------------------------------------------
void CRSEditView::CheckQuery_AddParameters(const CStringArray& arParameters, SymTable* pSymTable, UINT& nLastUI)
{
	for (int i = 0; i < arParameters.GetSize(); i++)
	{
		CString sFName = arParameters[i];

		WoormField* pF = dynamic_cast<WoormField*>(pSymTable->GetField(sFName));
		ASSERT_VALID(pF); if (!pF) continue;
		CString sDescr;
		sDescr += _T("Data-type: ") + pF->GetDataType().ToString() + L"; ";
		sDescr += cwsprintf(_T("ID: %d"), pF->GetId()) + L"; ";
		sDescr += pF->GetSourceDescription() + L"; ";

		const CParsedCtrlFamily* pFamily = AfxGetParsedControlsRegistry()->GetDefaultFamilyInfo(pF->GetData()->GetDataType());
		ASSERT_VALID(pFamily); if (!pFamily) continue;
		CRegisteredParsedCtrl* ctrl = pFamily->GetRegisteredControl(pF->GetData()->GetDataType());
		ASSERT_VALID(ctrl); if (!ctrl) continue;

		DWORD style = 0;
		if (pF->GetData()->GetDataType() == DataType::Bool)
			style = BS_AUTOCHECKBOX;
		else if (pF->GetData()->GetDataType().m_wType == DATA_ENUM_TYPE)
			style = CBS_DROPDOWNLIST;

		HotKeyLink* pHKL = NULL;
		if (pF->IsAsk())
		{
			AskFieldData* askField = GetDocument()->GetEditorManager()->GetPrgData()->GetAskRuleData()->GetAskField(pF->GetName());

			BOOL isDynamic, isXml;
			FunctionDataInterface* pDescri = AfxGetTbCmdManager()->GetHotlinkDescription(askField->m_nsHotLink, isDynamic, isXml);

			if (pDescri)
			{
				pHKL = (HotKeyLink*)pDescri->m_pComponentClass->CreateObject();
				pHKL->EnableAddOnFly(FALSE);
				pHKL->MustExistData(FALSE);

				sDescr += _TB("Namespace hotlink: ") + askField->m_nsHotLink.ToUnparsedString();
			}
		}

		CTBProperty* prop = ((CRSEditorParametersView*)GetEditorFrame(TRUE)->m_pParametersView)->m_pPropGridParams->
							AddProperty	(sFName, sFName, L"", pF->GetData(), nLastUI, style, ctrl->GetClass(), pHKL);

		prop->SetDescription(sDescr);

		if (pF->GetData()->GetDataType().IsNumeric())
		{
			int numDec = pF->GetNumDec();
			CParsedCtrl * ctrl = prop->GetControl();
			ctrl->SetCtrlNumDec(numDec);
		}

		nLastUI++;
	}
}

void CRSEditView::CheckNamedQuery()
{
	if (!GetEditorFrame(FALSE)) return;

	ASSERT_VALID(GetEditorFrame(TRUE)->m_pEditView);
	QueryObjItem* pOldQuery = (QueryObjItem*)GetEditorFrame(TRUE)->m_pEditView->m_Context.m_pNode->m_pItemData;
	ASSERT_VALID(pOldQuery);
	ASSERT_KINDOF(QueryObjItem, pOldQuery);

	ASSERT_VALID(GetEditorFrame(TRUE)->m_pParametersView);

	((CRSEditorParametersView*)GetEditorFrame(TRUE)->m_pParametersView)->m_pPropGridParams->RemoveAllComponents();
	((CRSEditorParametersView*)GetEditorFrame(TRUE)->m_pParametersView)->m_pPropGridParams->AdjustLayout();

	CString strQuery = GetEditorFrame(TRUE)->m_pEditView->GetEditCtrl()->GetText();
	strQuery.Trim();

	GetEditorFrame(TRUE)->m_pMainToolBar->HideButton(ID_EDIT_EXEC, FALSE);
	GetEditorFrame(TRUE)->m_pMainToolBar->AdjustLayout();

	QueryObject* pQuery = new QueryObject(pOldQuery->GetSymTable(), AfxGetDefaultSqlSession());

	BOOL bOk = pQuery->Define(DataStr(strQuery));
	if (!bOk)
	{
		CString sErr = pQuery->GetError();

		ShowDiagnostic(sErr);

		delete pQuery;
		return;
	}

	UINT nLastUI = ID_PARAM0;
	const CStringArray& arParameters = pQuery->AllQueryParameters();
	CheckQuery_AddParameters(arParameters, pQuery->GetSymTable(), nLastUI);

	const CStringArray& arExternalParameters = pQuery->ExternalQueryParameters();
	CheckQuery_AddParameters(arExternalParameters, pQuery->GetSymTable(), nLastUI);

	CString msg;
	if (::FindWord(strQuery, L"SELECT") == 0 && pQuery->AllQueryColumns().GetSize() == 0)
	{
		msg = _TB("Named query with SELECT sql clause has to be binded to report variables by tag { COL var-name }");
		msg += L"\r\n";
	}

	msg += _TB("The named query looks correct, but it contains native parts that will be validated on run by the database.");
	msg += L"\r\n";
	msg += _TB("Insert parameters to test the query and then click 'Execute'");

	ShowDiagnostic(msg);

	delete pQuery;
}

//-----------------------------------------------------------------------------
void CRSEditView::ExecuteNamedQuery()
{
	if (!GetEditorFrame(FALSE)) return;

	ASSERT_VALID(GetEditorFrame(TRUE)->m_pEditView);
	QueryObjItem* pOldQuery = (QueryObjItem*)GetEditorFrame(TRUE)->m_pEditView->m_Context.m_pNode->m_pItemData;
	ASSERT_VALID(pOldQuery);
	ASSERT_KINDOF(QueryObjItem, pOldQuery);

	ASSERT_VALID(GetEditorFrame(TRUE)->m_pGridView);
	ASSERT_VALID(GetEditorFrame(TRUE)->m_pGridView->m_pGrdTable);
	ASSERT_VALID(GetEditorFrame(TRUE)->m_pParametersView);

	if (!GetEditorFrame(TRUE)->m_pParametersView->CheckForm(TRUE))
		return;

	CString strQuery = GetEditorFrame(TRUE)->m_pEditView->GetEditCtrl()->GetText();
	strQuery.Trim();

	QueryObject* pQuery = new QueryObject(pOldQuery->GetSymTable(), AfxGetDefaultSqlSession());
	BOOL bOk = pQuery->Define(DataStr(strQuery));
	if (!bOk)
	{
		CString sErr = pQuery->GetError();

		ShowDiagnostic(sErr);
		delete pQuery;
		return;
	}

	CTBGridControlResizable*  pGrdTable = GetEditorFrame(TRUE)->m_pGridView->m_pGrdTable;
	pGrdTable->RemoveAll();
	pGrdTable->DeleteAllColumns();
	pGrdTable->AdjustLayout();

	//---------------
	if (strQuery.Find(L"SELECT") == 0 && pQuery->AllQueryColumns().GetSize() == 0)
	{
		ShowDiagnostic(_TB("Named query with SELECT sql clause has to be binded to report variables by tag { COL var-name }"));
	}

	BOOL ok = pQuery->Open();
	if (!ok)
	{
		ShowDiagnostic(pQuery->GetError());
		delete pQuery;
		return;
	}

	CString query = pQuery->ToSqlString();

	CArray<WoormField*> arColumnFields;
	const CStringArray& arColumns = pQuery->GetCurrentQueryColumns();
	for (int i = 0; i < arColumns.GetSize(); i++)
	{
		CString sFName = arColumns[i];

		WoormField* pF = dynamic_cast<WoormField*>(pQuery->GetSymTable()->GetField(sFName));
		arColumnFields.Add(pF);

		pGrdTable->InsertColumn(i, sFName, 100);
	}

	while (pQuery->Read())
	{
		CBCGPGridRow* pRow = pGrdTable->CreateRow(arColumns.GetSize());

		for (int i = 0; i < arColumnFields.GetSize(); i++)
		{
			DataObj* pObj = arColumnFields[i]->GetData();
			CString sValue = pObj->FormatData();

			pRow->GetItem(i)->SetValue((LPCTSTR)sValue);
		}
		pGrdTable->AddRow(pRow, FALSE /* Don't recal. layout */);
	}
	pGrdTable->AdjustLayout();
	//------------------------------------

	delete pQuery;

	GetEditorFrame(TRUE)->m_pDiagnosticView->m_edtErrors.SetWindowText(query);
	GetEditorFrame(TRUE)->UpdateWindow();

	//-------------------------------------------
	//RESIZE

	pGrdTable->OnRecalcCtrlSize(WM_USER, WM_KEYDOWN);
	GetEditorFrame(TRUE)->m_pDiagnosticView->m_edtErrors.OnRecalcCtrlSize(WM_USER, WM_KEYDOWN);	
}

//-----------------------------------------------------------------------------
void CRSEditView::CheckRuleQuery()
{
	if (!GetEditorFrame(FALSE)) return;

	ASSERT_VALID(GetEditorFrame(TRUE)->m_pEditView);
	TblRuleData* pTblRule = (TblRuleData*)GetEditorFrame(TRUE)->m_pEditView->m_Context.m_pNode->m_pItemData;
	ASSERT_VALID(pTblRule);
	ASSERT_KINDOF(TblRuleData, pTblRule);

	ASSERT_VALID(GetEditorFrame(TRUE)->m_pParametersView);

	((CRSEditorParametersView*)GetEditorFrame(TRUE)->m_pParametersView)->m_pPropGridParams->RemoveAllComponents();
	((CRSEditorParametersView*)GetEditorFrame(TRUE)->m_pParametersView)->m_pPropGridParams->AdjustLayout();

	CString strQuery = GetEditorFrame(TRUE)->m_pEditView->GetEditCtrl()->GetText();
	strQuery.Trim();

	GetEditorFrame(TRUE)->m_pMainToolBar->HideButton(ID_EDIT_EXEC, FALSE);
	GetEditorFrame(TRUE)->m_pMainToolBar->AdjustLayout();

	Parser lex(strQuery);

	TblRuleData aTblRule(*pTblRule->GetSymTable(), pTblRule->GetConnection());

	CStringArray arExternalParameters;
	aTblRule.GetSymTable()->TraceFieldsUsed(&arExternalParameters);

	if (!aTblRule.Parse(lex, FALSE, FALSE))
	{
		aTblRule.GetSymTable()->TraceFieldsUsed(NULL);
		ShowDiagnostic(lex);
		return;
	}
	aTblRule.GetSymTable()->TraceFieldsUsed(NULL);

	CStringArray arParameters;
	aTblRule.GetParameters(arParameters);

	CStringArray_AppendUnique(arExternalParameters, arParameters);

	UINT nLastUI = ID_PARAM0;
	CheckQuery_AddParameters(arExternalParameters, aTblRule.GetSymTable(), nLastUI);

	CString msg;
	if (aTblRule.IsNativeQuery() || ::FindWord(strQuery, L"ContentOf") > -1)
		msg = _TB("The query looks correct, but it contains native parts that will be validated on run by the database.");
	else
		msg = _TB("The query is correct.");

	msg += L"\r\n" + _TB("Insert parameters to test the query and then click 'Execute'");

	ShowDiagnostic(msg);
}

// ---------------------------------------------------------------------------- -
void CRSEditView::ExecuteRuleQuery()
{
	if (!GetEditorFrame(FALSE)) return;

	ASSERT_VALID(GetEditorFrame(TRUE)->m_pEditView);
	TblRuleData* pTblRule = (TblRuleData*)GetEditorFrame(TRUE)->m_pEditView->m_Context.m_pNode->m_pItemData;
	ASSERT_VALID(pTblRule);
	ASSERT_KINDOF(TblRuleData, pTblRule);

	ASSERT_VALID(GetEditorFrame(TRUE)->m_pGridView);
	ASSERT_VALID(GetEditorFrame(TRUE)->m_pGridView->m_pGrdTable);
	ASSERT_VALID(GetEditorFrame(TRUE)->m_pParametersView);

	if (!GetEditorFrame(TRUE)->m_pParametersView->CheckForm(TRUE))
		return;

	CTBGridControlResizable*  pGrdTable = GetEditorFrame(TRUE)->m_pGridView->m_pGrdTable;
	pGrdTable->RemoveAll();
	pGrdTable->DeleteAllColumns();
	pGrdTable->AdjustLayout();

	CString strQuery = GetEditorFrame(TRUE)->m_pEditView->GetEditCtrl()->GetText();

	Parser lex(strQuery);

	TblRuleData aTblRule(*pTblRule->GetSymTable(), pTblRule->GetConnection());

	if (!aTblRule.Parse(lex, FALSE, FALSE))
	{
		ShowDiagnostic(lex);
		return;
	}

	//---------------
	CArray<WoormField*> arColumnFields;
	CStringArray arColumns;
	aTblRule.GetColumns(arColumns);

	for (int i = 0; i < arColumns.GetSize(); i++)
	{
		CString sFName = arColumns[i];

		WoormField* pF = dynamic_cast<WoormField*>(aTblRule.GetSymTable()->GetField(sFName));
		arColumnFields.Add(pF);

		pGrdTable->InsertColumn(i, sFName, 100);
	}

	CString query;
	CString sError;
	BOOL bOk = aTblRule.ExecQuery(sError);

	//-----------------------------------
	SqlTable* pSqlTable = aTblRule.m_SqlTest.m_pDataTable;
	ASSERT_VALID(pSqlTable);

	if (pSqlTable)
	{
		query = pSqlTable->ToString(TRUE, FALSE, FALSE);

		CString sRuleParametersValue;
		CStringArray auxParamsArray;
		aTblRule.GetParameters(auxParamsArray);

		int start = 0;
		for (int i = 0; i < auxParamsArray.GetSize(); i++)
		{
			WoormField* pF = dynamic_cast<WoormField*>(aTblRule.GetSymTable()->GetField(auxParamsArray[i]));
			ASSERT_VALID(pF);
			if (pF)
				sRuleParametersValue += ::cwsprintf(_T("(%s=%s) "), (LPCTSTR)pF->GetName(), (LPCTSTR)(pF->GetRepData()->FormatData()));
		}
		if (!sRuleParametersValue.IsEmpty())
			query += L"\r\n\r\nRule parameters value:\r\n" + sRuleParametersValue;
	}

	//-----------------------------------
	if (!bOk)
	{
		GetEditorFrame(TRUE)->m_pDiagnosticView->m_edtErrors.SetWindowText(sError + L"\r\n" + query);
		return;
	}
	GetEditorFrame(TRUE)->m_pDiagnosticView->m_edtErrors.SetWindowText(query);

	while (!aTblRule.m_SqlTest.m_pDataTable->IsEOF())
	{
		CBCGPGridRow* pRow = pGrdTable->CreateRow(arColumns.GetSize());

		for (int i = 0; i < arColumnFields.GetSize(); i++)
		{
			DataObj* pObj = arColumnFields[i]->GetData();
			CString sValue = pObj->FormatData();

			pRow->GetItem(i)->SetValue((LPCTSTR)sValue);
		}
		// Add row to grid:
		pGrdTable->AddRow(pRow, FALSE /* Don't recal. layout */);

		aTblRule.m_SqlTest.m_pDataTable->MoveNext();
	}
	pGrdTable->AdjustLayout();

	//---------------------------------------

	aTblRule.m_SqlTest.m_pDataTable->Close();

	//---------------------------------------
	GetEditorFrame(TRUE)->UpdateWindow();

	//---------------------------------------
	//RESIZE
	pGrdTable->OnRecalcCtrlSize(WM_USER, WM_KEYDOWN);
	GetEditorFrame(TRUE)->m_pDiagnosticView->m_edtErrors.OnRecalcCtrlSize(WM_USER, WM_KEYDOWN);
}

// ---------------------------------------------------------------------------- -
void CRSEditView::CheckFullReport()
{
	if (!GetEditorFrame(FALSE)) return;

	ASSERT_VALID(GetDocument());

	CString wrmText = GetEditorFrame(TRUE)->m_pEditView->GetEditCtrl()->GetText();
	if (wrmText.IsEmpty())
		return;

	CWoormInfo info;
	BOOL compileOK = GetDocument()->ParseReport(&info, wrmText);
	if (!compileOK)
	{
		CString sErr;
		if (info.m_arErrors.GetCount())
		{
			CStringArray_Concat(info.m_arErrors, sErr, L"\r\n");
		}
		else sErr = _TB("Unknown error on parse report script");

		ShowDiagnostic(sErr, info.m_nLine, info.m_nCol);
	}
	else 
	{
		ASSERT(info.m_arErrors.GetCount() == 0);
		ShowDiagnostic(_TB("The Report is correct!"));
	}
}
