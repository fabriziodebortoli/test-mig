
#include "stdafx.h"

#include <TbParser\Parser.h>
#include <TBGenlib\baseapp.h>

#include "rpsymtbl.h"
#include "events.h"
#include "edtcmm.h"

#include "ExportSymbols.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif

//============================================================================
// OnActionsData
//============================================================================
IMPLEMENT_DYNAMIC(OnActionsData, CObject)
//----------------------------------------------------------------------------
OnActionsData::OnActionsData(WoormTable& aSymTable)
	:
	m_SymTable			(aSymTable),
	m_pBeforeBlock		(new Block(NULL, &aSymTable, NULL)),
	m_pAfterBlock		(new Block(NULL, &aSymTable, NULL))
{
	m_pBeforeBlock->SetForceBeginEnd();	
	m_pAfterBlock->SetForceBeginEnd();
}

//----------------------------------------------------------------------------
OnActionsData::~OnActionsData()
{
	SAFE_DELETE(m_pBeforeBlock);
	SAFE_DELETE(m_pAfterBlock);
}

//----------------------------------------------------------------------------
void OnActionsData::DispTableChanged(LPCTSTR pszOldName, LPCTSTR pszNewName)
{
	m_pBeforeBlock->DispTableChanged(pszOldName, pszNewName);
	m_pAfterBlock->DispTableChanged(pszOldName, pszNewName);
}

//----------------------------------------------------------------------------
void OnActionsData::GetSubtotalFields(CStringArray& arraySubtotals)
{
	m_pBeforeBlock->GetSubtotalFields(arraySubtotals);
	m_pAfterBlock->GetSubtotalFields(arraySubtotals);
}

//----------------------------------------------------------------------------
BOOL OnActionsData::CanDeleteField(LPCTSTR pszFieldName)
{
	return
		m_pBeforeBlock->CanDeleteField(pszFieldName) && 
		m_pAfterBlock->CanDeleteField(pszFieldName);
}

//----------------------------------------------------------------------------
void OnActionsData::DeleteField(LPCTSTR pszFieldName)
{
	m_pBeforeBlock->DeleteField(pszFieldName);
	m_pAfterBlock->DeleteField(pszFieldName);
}

//----------------------------------------------------------------------------
void OnActionsData::DeleteTable(LPCTSTR pszDispTableName)
{
	m_pBeforeBlock->DeleteTable(pszDispTableName);
	m_pAfterBlock->DeleteTable(pszDispTableName);
}

//----------------------------------------------------------------------------
BOOL OnActionsData::SearchNamedAction(LPCTSTR pszName, ActionType anAction)
{
	return 
		m_pBeforeBlock->SearchNamedAction(pszName, anAction) ||
		m_pAfterBlock->SearchNamedAction(pszName, anAction);
}

//----------------------------------------------------------------------------
void OnActionsData::DeleteNamedAction(LPCTSTR pszName, ActionType anAction)
{
	m_pBeforeBlock->DeleteNamedAction(pszName, anAction);
	m_pAfterBlock->DeleteNamedAction(pszName, anAction);
}

//----------------------------------------------------------------------------
void OnActionsData::RenameNamedAction(LPCTSTR pszOldName, LPCTSTR pszNewName, ActionType anAction)
{
	m_pBeforeBlock->RenameNamedAction(pszOldName, pszNewName, anAction);
	m_pAfterBlock->RenameNamedAction(pszOldName, pszNewName, anAction);
}

//----------------------------------------------------------------------------
BOOL OnActionsData::IsEmpty()
{
	if (
			m_pBeforeBlock->GetCount() ||
			m_pAfterBlock->GetCount() ||

			m_pBeforeBlock->m_arCommentTraceBefore.GetSize() ||
			m_pBeforeBlock->m_arCommentTraceAfter.GetSize() ||

			m_pAfterBlock->m_arCommentTraceBefore.GetSize() ||
			m_pAfterBlock->m_arCommentTraceAfter.GetSize() 
		)
		return FALSE;

	return TRUE;
}

//----------------------------------------------------------------------------
void OnActionsData::Empty()
{
	if (m_pBeforeBlock) m_pBeforeBlock->Empty();
	if (m_pAfterBlock) m_pAfterBlock->Empty();
}

//----------------------------------------------------------------------------
void OnActionsData::AddAction
	(
		ActionType	actionType,
		LPCTSTR 	pszString,
		BOOL 		bInBeforeSection,
		int			nPos
	)
{
	if (bInBeforeSection)
    	m_pBeforeBlock->AddAction(actionType, pszString, nPos);
	else
		m_pAfterBlock->AddAction(actionType, pszString, nPos);
}

//----------------------------------------------------------------------------
int OnActionsData::GetIdxAction(ActionType actionType, BOOL bInBeforeSection) const
{
	if (bInBeforeSection)
		return m_pBeforeBlock->GetIdxAction(actionType);
	else
		return m_pAfterBlock->GetIdxAction(actionType);
}

//----------------------------------------------------------------------------
BOOL OnActionsData::Parse(Parser& lex)
{
	if (lex.LookAhead(T_BEFORE))
	{
		lex.SkipToken();
		if (!m_pBeforeBlock->Parse(lex))
			return FALSE;

		if (lex.LookAhead(T_AFTER))
		{
			lex.SkipToken();
        	if (!m_pAfterBlock->Parse(lex))
        		return FALSE;
		}        	
	}
	else
    {
		if (!lex.ParseTag(T_AFTER))		
			return lex.SetError(_TB("Missing action"));
			
		if (!m_pAfterBlock->Parse(lex))	
			return FALSE;
	}

    return TRUE;
}

//----------------------------------------------------------------------------
void OnActionsData::Unparse(Unparser& oFile)
{
	if (!m_pBeforeBlock->IsEmpty())
	{
		oFile.IncTab();
		oFile.UnparseTag(T_BEFORE, FALSE);

        m_pBeforeBlock->Unparse(oFile);
		oFile.DecTab();
    }

	if (!m_pAfterBlock->IsEmpty())
	{
		oFile.IncTab();
		oFile.UnparseTag(T_AFTER, FALSE);

		m_pAfterBlock->Unparse(oFile);
		oFile.DecTab();
	}
}

//============================================================================
// TableActionData
//============================================================================
//----------------------------------------------------------------------------
TableActionData::TableActionData(WoormTable& aSymTable)
	:
    OnActionsData	(aSymTable)
{}

//----------------------------------------------------------------------------
CString TableActionData::GetDisplayTableName()
{
	return m_strDisplayTableName;
}

//----------------------------------------------------------------------------
void TableActionData::SetDisplayTableName(LPCTSTR pszNewName)
{
	m_strDisplayTableName = pszNewName;
	ASSERT(m_strDisplayTableName.GetLength());
}

//----------------------------------------------------------------------------
void TableActionData::DispTableChanged(LPCTSTR pszOldName, LPCTSTR pszNewName)
{
	if (m_strDisplayTableName.CompareNoCase(pszOldName) == 0)
    	m_strDisplayTableName	= pszNewName;

	ASSERT(m_strDisplayTableName.GetLength());

	OnActionsData::DispTableChanged(pszOldName, pszNewName);
}

//----------------------------------------------------------------------------
BOOL TableActionData::Parse(Parser& lex)
{
	if (!lex.ParseTag(T_TABLE))
		return FALSE;

	if (lex.LookAhead(T_ID))
		lex.ParseID(m_strDisplayTableName);

	if (!lex.ParseTag(T_COLON) || !lex.ParseTag(T_DO))
		return FALSE;

	return OnActionsData::Parse(lex);
}

//----------------------------------------------------------------------------
void TableActionData::Unparse(Unparser& oFile)
{
	if (IsEmpty())	
		return;

	ASSERT(m_strDisplayTableName.GetLength());

	oFile.UnparseTag	(T_TABLE,				FALSE);
	oFile.UnparseID		(m_strDisplayTableName,	FALSE);
	oFile.UnparseTag	(T_COLON,				FALSE);
	oFile.UnparseTag	(T_DO,					TRUE);

	OnActionsData::Unparse(oFile);

	oFile.UnparseCrLf();
}

//============================================================================
// NewPageActionData
//============================================================================

NewPageActionData::NewPageActionData(WoormTable& aSymTable)
	:
    OnActionsData	(aSymTable)
{}

//----------------------------------------------------------------------------
BOOL NewPageActionData::Parse(Parser& lex)
{
	if	(
			!lex.ParseTag(T_FORMFEED)	||
	        !lex.ParseTag(T_COLON)		||
	        !lex.ParseTag(T_DO)
       )
    	return FALSE;

	return OnActionsData::Parse(lex);
}

//----------------------------------------------------------------------------
void NewPageActionData::Unparse(Unparser& oFile)
{
	if (IsEmpty())
		return;

	oFile.UnparseTag(T_FORMFEED,	FALSE);
	oFile.UnparseTag(T_COLON,		FALSE);
	oFile.UnparseTag(T_DO,			TRUE);

	OnActionsData::Unparse(oFile);
	oFile.UnparseCrLf();
}

//============================================================================
// ReportEventData
//============================================================================
//----------------------------------------------------------------------------
ReportEventData::ReportEventData(WoormTable& aSymTable)
	:
    OnActionsData	(aSymTable),
	m_pAlwaysBlock	(new Block(NULL, &aSymTable, NULL)),
	m_pFinalizeBlock(new Block(NULL, &aSymTable, NULL))
{
	m_pAlwaysBlock->SetForceBeginEnd();
	m_pFinalizeBlock->SetForceBeginEnd();
}

//----------------------------------------------------------------------------
ReportEventData::~ReportEventData()
{
	SAFE_DELETE(m_pAlwaysBlock);
	SAFE_DELETE(m_pFinalizeBlock);
}

//----------------------------------------------------------------------------
BOOL ReportEventData::CanDeleteField(LPCTSTR pszFieldName)
{
	if (!m_pAlwaysBlock->CanDeleteField(pszFieldName) || !m_pFinalizeBlock->CanDeleteField(pszFieldName))
		return FALSE;

	return OnActionsData::CanDeleteField(pszFieldName);
}

//----------------------------------------------------------------------------
void ReportEventData::DeleteField(LPCTSTR pszFieldName) 
{
	m_pAlwaysBlock->DeleteField(pszFieldName);
	m_pFinalizeBlock->DeleteField(pszFieldName);

	OnActionsData::DeleteField(pszFieldName);
}

//----------------------------------------------------------------------------
void ReportEventData::DeleteTable(LPCTSTR pszDispTableName)
{
	m_pAlwaysBlock->DeleteTable(pszDispTableName);
	m_pFinalizeBlock->DeleteTable(pszDispTableName);

	OnActionsData::DeleteTable(pszDispTableName);
}

//----------------------------------------------------------------------------
BOOL ReportEventData::SearchNamedAction(LPCTSTR pszName, ActionType anAction)
{
	return
		m_pAlwaysBlock->SearchNamedAction(pszName, anAction) ||
		m_pFinalizeBlock->SearchNamedAction(pszName, anAction) ||
		OnActionsData::SearchNamedAction(pszName, anAction);
}

//----------------------------------------------------------------------------
void ReportEventData::DeleteNamedAction(LPCTSTR pszName, ActionType anAction)
{
	m_pAlwaysBlock->DeleteNamedAction(pszName, anAction);
	m_pFinalizeBlock->DeleteNamedAction(pszName, anAction);

	OnActionsData::DeleteNamedAction(pszName, anAction);
}

//----------------------------------------------------------------------------
void ReportEventData::RenameNamedAction(LPCTSTR pszOldName, LPCTSTR pszNewName, ActionType anAction)
{
	m_pAlwaysBlock->RenameNamedAction(pszOldName, pszNewName, anAction);
	m_pFinalizeBlock->RenameNamedAction(pszOldName, pszNewName, anAction);

	OnActionsData::RenameNamedAction(pszOldName, pszNewName, anAction);
}

//----------------------------------------------------------------------------
BOOL ReportEventData::IsEmpty()
{
	if (!OnActionsData::IsEmpty())
		return FALSE;

	if (
		m_pAlwaysBlock->GetCount() ||
		m_pFinalizeBlock->GetCount() ||

		m_pAlwaysBlock->m_arCommentTraceBefore.GetSize() ||
		m_pAlwaysBlock->m_arCommentTraceAfter.GetSize() ||

		m_pFinalizeBlock->m_arCommentTraceBefore.GetSize() ||
		m_pFinalizeBlock->m_arCommentTraceAfter.GetSize()
		)
		return FALSE;

	return TRUE;
}

//----------------------------------------------------------------------------
void ReportEventData::AddActionInAlways(ActionType actionType, LPCTSTR pszString)
{
	m_pAlwaysBlock->AddAction(actionType, pszString, -1 /* add at the end*/);
}

//----------------------------------------------------------------------------
void ReportEventData::DispTableChanged(LPCTSTR pszOldName, LPCTSTR pszNewName)
{
	m_pAlwaysBlock->DispTableChanged(pszOldName, pszNewName);
	m_pFinalizeBlock->DispTableChanged(pszOldName, pszNewName);

	OnActionsData::DispTableChanged(pszOldName, pszNewName);
}

//----------------------------------------------------------------------------
BOOL ReportEventData::Parse(Parser& lex)
{
	BOOL bBlockExist = FALSE;
	lex.RemoveCommentTrace(); //remove previous comments

	if	(
			!lex.ParseTag(T_REPORT) ||
			!lex.ParseTag(T_COLON)	||
			!lex.ParseTag(T_DO)
	   )
		return FALSE;

	if (lex.LookAhead(T_ALWAYS))
	{
		lex.SkipToken();
		if (!m_pAlwaysBlock->Parse(lex))
			return FALSE;
        bBlockExist = TRUE;
	}

    if (lex.LookAhead(T_BEFORE) || lex.LookAhead(T_AFTER))
	{
		if (!OnActionsData::Parse(lex))
			return FALSE;
		bBlockExist = TRUE;
	}

	if (lex.LookAhead(T_FINALIZE))
	{
		lex.SkipToken();
		if (!m_pFinalizeBlock->Parse(lex))
			return FALSE;
		bBlockExist = TRUE;
 	}

	if (!bBlockExist)			
		return lex.SetError(_TB("Missing action"));

	return TRUE;
}

//----------------------------------------------------------------------------
void ReportEventData::Unparse(Unparser& oFile)
{
	if (IsEmpty())
		return;

	oFile.UnparseTag(T_REPORT,	FALSE);
	oFile.UnparseTag(T_COLON,	FALSE);
	oFile.UnparseTag(T_DO,		TRUE);

	if (!m_pAlwaysBlock->IsEmpty())
	{
		oFile.IncTab();
		oFile.UnparseTag(T_ALWAYS, FALSE);

		m_pAlwaysBlock->Unparse(oFile);
		oFile.DecTab();
	}

    OnActionsData::Unparse(oFile);

	if (!m_pFinalizeBlock->IsEmpty())
	{
		oFile.IncTab();
		oFile.UnparseTag(T_FINALIZE, FALSE);

		m_pFinalizeBlock->Unparse(oFile);
		oFile.DecTab();
	}
}

//============================================================================
// TriggEventData
//============================================================================
//----------------------------------------------------------------------------
TriggEventData::TriggEventData(WoormTable& aSymTable)
	:
	OnActionsData	(aSymTable),
	m_pWhenExpr		(new Expression(&aSymTable)),
	m_bMustTrueTogether (TRUE)
{}

//----------------------------------------------------------------------------
TriggEventData::TriggEventData
	(
	WoormTable& aSymTable,
	LPCTSTR			pszName,
    CStringArray&	arrayFields
   )
	:
	OnActionsData	(aSymTable),
	m_pWhenExpr(new Expression(&aSymTable)),
	m_bMustTrueTogether(TRUE),
	m_strEventName	(pszName)
{
	for (int i = 0; i < arrayFields.GetSize(); i++)
		m_BreakList.Add(arrayFields[i]);
}

//----------------------------------------------------------------------------
TriggEventData::~TriggEventData()
{
	SAFE_DELETE(m_pWhenExpr);
}
//----------------------------------------------------------------------------
CString TriggEventData::GetEventName()
{
	return m_strEventName;
}

//----------------------------------------------------------------------------
void TriggEventData::SetEventName(LPCTSTR pszName)
{
	m_strEventName = pszName;
}

//----------------------------------------------------------------------------
BOOL TriggEventData::ParseBreakList(Parser& lex)
{
	m_BreakList.RemoveAll();

	CString pszFieldName;
	if (!lex.ParseID(pszFieldName))		
		return FALSE;

	WoormField* pRepField = m_SymTable.GetField(pszFieldName);

	if (pRepField == NULL)
		return lex.SetError(Expression::FormatMessage(Expression::UNKNOWN_FIELD), pszFieldName);
		
	if (pRepField->IsInput())
		return lex.SetError(_TB("Read-only field"), pszFieldName);

	m_BreakList.Add(pszFieldName);

	while (lex.LookAhead(T_COMMA))
	{
		lex.SkipToken();
		if (!lex.ParseID(pszFieldName))
			return FALSE;

		if (pRepField == NULL)
			return lex.SetError(Expression::FormatMessage(Expression::UNKNOWN_FIELD), pszFieldName);
			
		if (pRepField->IsInput())
    		return lex.SetError(_TB("Read-only field"), pszFieldName);

		m_BreakList.Add(pszFieldName);
	}

    return TRUE;
}

//----------------------------------------------------------------------------
BOOL TriggEventData::ParseWhenExpr(Parser& lex)
{
	if (!m_pWhenExpr)
		m_pWhenExpr = new Expression(&this->m_SymTable);
	else
		m_pWhenExpr->Reset(FALSE);

	if (!lex.ParseTag(T_WHEN))
		return FALSE;

	m_pWhenExpr->SetStopTokens(T_DO);
	if (!m_pWhenExpr->Parse(lex, DATA_BOOL_TYPE, TRUE))
		return FALSE;

    return TRUE;
}

//----------------------------------------------------------------------------
BOOL TriggEventData::CanConvertFieldToInput(LPCTSTR pszFieldName, CString& sLog) const
{
	for (int i = 0; i < m_BreakList.GetSize(); i++)
	{
		if (m_BreakList[i].CompareNoCase(pszFieldName) == 0)
		{
			sLog = _TB("The field is used into a trigger event") + L" - " + this->m_strEventName;
			return FALSE;
		}
	}

	return TRUE;
}

//----------------------------------------------------------------------------
BOOL TriggEventData::CanDeleteField(LPCTSTR pszFieldName)
{
	for (int i = 0; i < m_BreakList.GetSize(); i++)
	{
		if (m_BreakList[i].CompareNoCase(pszFieldName) == 0)
        	return FALSE;
	}

	if (m_pWhenExpr && m_pWhenExpr->HasMember(pszFieldName))
		return FALSE; 

    return OnActionsData::CanDeleteField(pszFieldName);
}

//----------------------------------------------------------------------------
void TriggEventData::DeleteField(LPCTSTR pszFieldName)
{
	OnActionsData::DeleteField(pszFieldName);
}

//----------------------------------------------------------------------------
BOOL TriggEventData::Parse(Parser& lex)
{
	CString strTmp;
	Token	tk;

	if (!lex.ParseID(m_strEventName))	
		return FALSE;
	if (!lex.ParseTag(T_COLON))			
		return FALSE;

	if (lex.Matched(T_BREAKING))
    {
		if (!ParseBreakList(lex))			
			return FALSE;

		tk = lex.LookAhead();
		if (tk == T_AND || tk == T_OR)
		{
			m_bMustTrueTogether = (tk == T_AND);
			lex.SkipToken();
			if (!ParseWhenExpr(lex))
				return FALSE;
		}
	}
	else
	{
    	if (!ParseWhenExpr(lex))
    		return FALSE;
    }

	if (!lex.ParseTag(T_DO))
		return FALSE;

    return OnActionsData::Parse(lex);
}

//----------------------------------------------------------------------------
void TriggEventData::UnparseBreakList(Unparser& oFile)
{
	if (m_BreakList.GetSize() == 0)
		return;

	oFile.UnparseID(m_BreakList[0], FALSE);

	for (int i = 1; i < m_BreakList.GetSize(); i++)
	{
		oFile.UnparseComma	(					FALSE);
		oFile.UnparseBlank	();
		oFile.UnparseID		(m_BreakList[i],	FALSE);
    }
}

CString TriggEventData::UnparseBreakList()
{
	Unparser oFile(TRUE);
	UnparseBreakList(oFile);
	oFile.Close();

	return oFile.GetBufferString();
}

//----------------------------------------------------------------------------
void TriggEventData::GetBreakingFields(CStringArray& ar)
{
	CString s = UnparseBreakList();
	CStringArray_Split(ar, s, L",");
}

//----------------------------------------------------------------------------
void TriggEventData::UnparseWhenExpr(Unparser& oFile)
{
	if (m_pWhenExpr && !m_pWhenExpr->IsEmpty())
	{
		oFile.UnparseTag	(T_WHEN,		FALSE);
		oFile.UnparseExpr	(m_pWhenExpr->ToString(),	FALSE);
	}
}

//----------------------------------------------------------------------------
BOOL TriggEventData::IsEmpty()
{	
	if (m_BreakList.GetSize() == 0 && (!m_pWhenExpr || m_pWhenExpr->IsEmpty()))
		return TRUE;

	BOOL b = __super::IsEmpty(); //butterebbe via l'evento se non ci sono istruzioni
	return FALSE;
}

//----------------------------------------------------------------------------
void TriggEventData::Empty()
{
	__super::Empty();

	m_strEventName.Empty();
	m_BreakList.RemoveAll();
	m_bMustTrueTogether = TRUE;
	if (m_pWhenExpr) m_pWhenExpr->Reset(FALSE);
}

//----------------------------------------------------------------------------
void TriggEventData::Unparse(Unparser& oFile)
{
	if (IsEmpty())
		return;

	oFile.UnparseID		(m_strEventName,FALSE);
	oFile.UnparseTag	(T_COLON,		FALSE);

	if (m_BreakList.GetSize() > 0)
	{
		oFile.UnparseTag(T_BREAKING, FALSE);

		UnparseBreakList(oFile);

		if (m_pWhenExpr && !m_pWhenExpr->IsEmpty())
		{
			oFile.UnparseTag((m_bMustTrueTogether ? T_AND : T_OR), FALSE);

            UnparseWhenExpr(oFile);
		}
	}
	else
	{
		UnparseWhenExpr(oFile);
	}

    oFile.UnparseBlank	();
	oFile.UnparseTag	(T_DO, TRUE);

	OnActionsData::Unparse(oFile);

	oFile.UnparseCrLf();
}

CString TriggEventData::Unparse()
{
	Unparser oFile(TRUE);
	Unparse(oFile);
	oFile.Close();

	return oFile.GetBufferString();
}

//-----------------------------------------------------------------------------
CString TriggEventData::MakeSubTotalName(CString strNewName)
{
	if (strNewName.GetLength() > 1)
	{
		if (strNewName.Left(2).CompareNoCase(L"w_") == 0)
			strNewName = strNewName.Mid(2);
	}
	strNewName = L"SubTotal_" + strNewName;

	CString sSubTotalName = strNewName;
	int		i = 0;
	while (m_SymTable.ExistField(sSubTotalName))
	{
		i++;
		sSubTotalName = strNewName + cwsprintf(_T("_%d"), i);
	}
	return sSubTotalName;
}

//-----------------------------------------------------------------------------
WoormField* TriggEventData::CreateSubTotalField(const CString& sColumnName, EventsData* pEventsData)
{
	WoormField* pColumnField = m_SymTable.GetField(sColumnName);

	if (pColumnField)
	{
		CString sSubTotalName = MakeSubTotalName(sColumnName);
		
		WoormField* pSubTotalField = new WoormField
											(
												sSubTotalName,
												WoormField::FIELD_SUBTOTAL
											);

		pSubTotalField->SetLen(pColumnField->GetLen());
		pSubTotalField->SetNumDec(pColumnField->GetNumDec());
		pSubTotalField->SetHidden(pColumnField->IsHidden());
		// Anyway the column is visible (see CSubTotalDlg::ShowColumnField())

		if (IsNumericDataObj(pColumnField->GetDataType()))
		{
			pSubTotalField->SetDataType(pColumnField->GetDataType());
			pSubTotalField->SetEventFunction(&m_SymTable, sColumnName, T_CSUM);
		}
		else if (pColumnField->GetDataType() == DataType::String)
		{
			// "SubTotali" descrittivi
			pSubTotalField->SetDataType(DATA_STR_TYPE);
			CString strTmp(_TB("\"SubTotal\""));
			pSubTotalField->SetEventFunction(&m_SymTable, sColumnName, strTmp, DATA_STR_TYPE);
		}
		else
		{
			// casi particolari
			pSubTotalField->SetDataType(pColumnField->GetDataType());
			CString strTmp(sColumnName);
			pSubTotalField->SetEventFunction(&m_SymTable, sColumnName, strTmp, pColumnField->GetDataType());
		}

		m_SymTable.Add(pSubTotalField);
		//---- -----
		int nPosNextlineAction = this->GetIdxAction(ACT_NEXTLINE, TRUE /* in before section */);

		pEventsData->AddActionInAlways(ACT_EVAL, pSubTotalField->GetName());

		this->AddAction
				(
				ACT_DISPLAY,
				pSubTotalField->GetName(),
				TRUE, // it is always in before section */
				nPosNextlineAction
				);

		this->AddAction
				(
				ACT_RESET,
				pSubTotalField->GetName(),
				TRUE, // it is always in before section */
				(nPosNextlineAction == -1 ? -1 : nPosNextlineAction + 1)
				);

		if (nPosNextlineAction == -1)
			this->AddAction(ACT_NEXTLINE, NULL, TRUE);

		return pSubTotalField;
	}
	return NULL;
}

//----------------------------------------------------------------------------
CString TriggEventData::GetEventPrototype()
{
	CString	tColon, tDo, tBegin, tEnd;
	CString tBefore, tAfter, tAlways;
	CString str, msg;
	CString	evnName, tBreaking, tWhen;

	tColon = cwsprintf(T_COLON);
	tDo = cwsprintf(T_DO);
	tBegin = cwsprintf(T_BEGIN);
	tBegin = _T("\t\t") + tBegin;
	tEnd = cwsprintf(T_END);
	tEnd = _T("\t\t") + tEnd;
	tBefore = cwsprintf(T_BEFORE);
	tBefore = _T("\t") + tBefore;
	tAfter = cwsprintf(T_AFTER);
	tAfter = _T("\t") + tAfter;
	tAlways = cwsprintf(T_ALWAYS);
	tAlways = _T("\t") + tAlways;

	msg = _TB("/****** CHANGE THE EXAMPLE WHERE REQUIRED ******/");
	::StrAppend(str, msg, FALSE, TRUE);

	evnName = _TB("<NewEvent_name>");
	msg = _T("\t") + _TB("/****** USE ONE OF THE FOLLOWING CRITERIA: ******/");
	tBreaking = cwsprintf(T_BREAKING);
	tBreaking += _T(" <c1>,<c2>,...");
	tWhen = cwsprintf(T_WHEN);
	tWhen += _T(" <expr>");
	StrAppend(str, evnName, TRUE, FALSE);
	StrAppend(str, tColon, TRUE, TRUE);
	StrAppend(str, msg, TRUE, TRUE);
	StrAppend(str, _T("\t// a)\t"), TRUE, FALSE);
	StrAppend(str, tBreaking, TRUE, TRUE);
	StrAppend(str, _T("\t// b)\t"), TRUE, FALSE);
	StrAppend(str, tWhen, TRUE, TRUE);
	StrAppend(str, _T("\t// c)\t"), TRUE, FALSE);
	StrAppend(str, tBreaking, TRUE, FALSE);
	StrAppend(str, _T("AND/OR"), TRUE, FALSE);
	StrAppend(str, tWhen, TRUE, TRUE);
	StrAppend(str, CString(_T("// ")) + _TB("Remarks:"), TRUE, TRUE);
	StrAppend(str, CString(_T("// ")) +
		_TB("The rule's fields evaluated into \"<when-expr>\" contains the value of the new record"), TRUE, TRUE);
	StrAppend(str, CString(_T("// ")) +
		_TB("If the \"<when-expr>\" contains a rules' field, the 'Before' event was not raised on the last record"), TRUE, TRUE);

	StrAppend(str, tDo, FALSE, TRUE);

	StrAppend(str, tBefore, TRUE, FALSE);
	StrAppend(str, CString(_T("// ")) + _TB("The 'Before' event was not raised on the first record"), TRUE, TRUE);
	StrAppend(str, tBegin, FALSE, TRUE);
	StrAppend(str, tEnd, FALSE, TRUE);

	StrAppend(str, _T(""), FALSE, TRUE);

	StrAppend(str, tAfter, TRUE, FALSE);
	StrAppend(str, CString(_T("// ")) + _TB("The 'After' event was not raised on the last record"), TRUE, TRUE);
	StrAppend(str, tBegin, FALSE, TRUE);
	StrAppend(str, tEnd, FALSE, TRUE);

	return str;
}

//----------------------------------------------------------------------------
BOOL TriggEventData::CheckRuleItems(CString& sErr)
{
	for (int i = 0; i < m_BreakList.GetSize(); i++)
	{
		CString sName = m_BreakList[i];

		WoormField* pFuncItem = m_SymTable.GetField(sName);

		if (pFuncItem && !pFuncItem->IsARuleItem())
		{
			sErr = cwsprintf(_TB("Field {0-%s} has to valorized by rule"), sName);
			return FALSE;
		}
	}

	return TRUE;
}

//============================================================================
// EventsData
//============================================================================
//----------------------------------------------------------------------------
EventsData::EventsData(WoormTable& aSymTable)
	:
	m_SymTable			(aSymTable),
	m_pReportActions	(NULL),
	m_pNewPageActions	(NULL)
{
	m_pReportActions = new ReportEventData(aSymTable);
	m_pNewPageActions = new NewPageActionData(aSymTable);
}

//----------------------------------------------------------------------------
EventsData::~EventsData()
{
	if (m_pReportActions)	delete m_pReportActions;
	if (m_pNewPageActions)	delete m_pNewPageActions;
}

//----------------------------------------------------------------------------
int EventsData::GetTableEventIdx(LPCTSTR pszTableEventName)
{
	for (int i = 0; i < m_TableActions.GetSize(); i++)
	{
		TableActionData* pTblEvent = (TableActionData*) m_TableActions[i];
		if	(
				pTblEvent->GetDisplayTableName().IsEmpty()			||
				pTblEvent->GetDisplayTableName().CompareNoCase(pszTableEventName) == 0
			)
			return i;
	}
	return -1;
}

//----------------------------------------------------------------------------
int EventsData::GetEventIdx(LPCTSTR m_strEventName)
{
	for (int i = 0; i < m_TriggEvents.GetSize(); i++)
	{
		TriggEventData* pTrgEvent = (TriggEventData*) m_TriggEvents[i];
		if (_tcsicmp(m_strEventName, pTrgEvent->GetEventName()) == 0)
			return i;
	}
	return -1;
}

//----------------------------------------------------------------------------
TableActionData* EventsData::GetTableEvent(LPCTSTR pszTableEventName)
{
	int nIdx = GetTableEventIdx(pszTableEventName);

	return (nIdx == -1 ? NULL : (TableActionData*) m_TableActions[nIdx]);
}

//----------------------------------------------------------------------------
TriggEventData*	EventsData::GetEvent(LPCTSTR pszEventName)
{
	int nIdx = GetEventIdx(pszEventName);

	return (nIdx == -1 ? NULL : (TriggEventData*) m_TriggEvents[nIdx]);
}

//----------------------------------------------------------------------------
void EventsData::DispTableChanged(LPCTSTR pszOldName, LPCTSTR pszNewName)
{
	int nNumItem, i;

	if (m_pReportActions)
		m_pReportActions->DispTableChanged(pszOldName, pszNewName);

	if (m_pNewPageActions)
		m_pNewPageActions->DispTableChanged(pszOldName, pszNewName);

	nNumItem = m_TableActions.GetSize();
	for (i = 0; i < nNumItem; i++)
	{
		TableActionData* pTblAction = (TableActionData*) m_TableActions[i];
		pTblAction->DispTableChanged(pszOldName, pszNewName);
	}

    nNumItem = m_TriggEvents.GetSize();
	for (i=0; i<nNumItem; i++)
	{
		TriggEventData*	pTrigEventData = (TriggEventData*) m_TriggEvents[i];
		pTrigEventData->DispTableChanged(pszOldName, pszNewName);
	}
}

//----------------------------------------------------------------------------
BOOL EventsData::CanConvertFieldToInput(LPCTSTR pszFieldName, CString& sLog) const
{
	for (int nIdx = 0; nIdx < m_TriggEvents.GetSize(); nIdx++)
	{
		TriggEventData*	pTrigEventData = (TriggEventData*)m_TriggEvents[nIdx];

		if (!pTrigEventData->CanConvertFieldToInput(pszFieldName, sLog))
			return FALSE;
	}

	return TRUE;
}

//----------------------------------------------------------------------------
BOOL EventsData::CanDeleteField(LPCTSTR pszFieldName) const
{
	int nIdx;

	if (m_pReportActions && !m_pReportActions->CanDeleteField(pszFieldName))
		return FALSE;

	if (m_pNewPageActions && !m_pNewPageActions->CanDeleteField(pszFieldName))
		return FALSE;

	for (nIdx = 0; nIdx < m_TableActions.GetSize(); nIdx++)
	{
		TableActionData* pTblAction = (TableActionData*) m_TableActions[nIdx];

		if (!pTblAction->CanDeleteField(pszFieldName))
			return FALSE;
	}

	for (nIdx = 0; nIdx < m_TriggEvents.GetSize(); nIdx++)
	{
		TriggEventData*	pTrigEventData = (TriggEventData*) m_TriggEvents[nIdx];

		if (!pTrigEventData->CanDeleteField(pszFieldName))
			return FALSE;
	}

	return TRUE;
}

//----------------------------------------------------------------------------
void EventsData::DeleteField(LPCTSTR pszFieldName)
{
	CWordArray	idxToDelete1;
	CWordArray	idxToDelete2;
	int nIdx;

	if (m_pReportActions)
		m_pReportActions->DeleteField(pszFieldName);

	if (m_pNewPageActions)
		m_pNewPageActions->DeleteField(pszFieldName);

	for (nIdx = 0; nIdx < m_TableActions.GetSize(); nIdx++)
	{
		TableActionData* pTblAction = (TableActionData*) m_TableActions[nIdx];

		pTblAction->DeleteField(pszFieldName);

		if (pTblAction->IsEmpty())
			idxToDelete1.Add((WORD)nIdx);
	}

	for (nIdx = 0; nIdx < m_TriggEvents.GetSize(); nIdx++)
	{
		TriggEventData*	pTrigEventData = (TriggEventData*) m_TriggEvents[nIdx];

		pTrigEventData->DeleteField(pszFieldName);

		if (pTrigEventData->IsEmpty())
			idxToDelete2.Add((WORD)nIdx);
	}

	// now mest be deleted all empty object
	//
	for (nIdx = idxToDelete1.GetUpperBound(); nIdx >= 0; nIdx--)
		m_TableActions.RemoveAt(idxToDelete1[ nIdx ]);

	for (nIdx = idxToDelete2.GetUpperBound(); nIdx >= 0; nIdx--)
		m_TriggEvents.RemoveAt(idxToDelete2[ nIdx ]);
}

//----------------------------------------------------------------------------
void EventsData::DeleteTable(LPCTSTR pszDispTableName)
{
	if (m_pReportActions)
		m_pReportActions->DeleteTable(pszDispTableName);

	if (m_pNewPageActions)
		m_pNewPageActions->DeleteTable(pszDispTableName);

	int nIdx = GetTableEventIdx(pszDispTableName);
	if (nIdx >= 0)
		m_TableActions.RemoveAt(nIdx);

	CWordArray	idxToDelete;

	for (nIdx = 0; nIdx < m_TriggEvents.GetSize(); nIdx++)
	{
		TriggEventData*	pTrigEventData = (TriggEventData*) m_TriggEvents[nIdx];

		pTrigEventData->DeleteTable(pszDispTableName);

		if (pTrigEventData->IsEmpty())
			idxToDelete.Add((WORD)nIdx);
	}

	// now delete all empty object
	//
	for (nIdx = idxToDelete.GetUpperBound(); nIdx >= 0; nIdx--)
		m_TriggEvents.RemoveAt(idxToDelete[ nIdx ]);
}

//----------------------------------------------------------------------------
BOOL EventsData::IsEmpty()
{
	return
		(m_TableActions.GetSize() == 0)	&&
		(m_TriggEvents.GetSize() == 0)	&&
		(!m_pReportActions || m_pReportActions->IsEmpty())	&&
		(!m_pNewPageActions || m_pNewPageActions->IsEmpty());
}

//----------------------------------------------------------------------------
void EventsData::AddActionInNewPage(ActionType actionType, LPCTSTR pszString, BOOL bInBeforeSection)
{
   	if (!m_pNewPageActions)
		m_pNewPageActions = new NewPageActionData(m_SymTable);

	m_pNewPageActions->AddAction(actionType, pszString, bInBeforeSection);
}

//----------------------------------------------------------------------------
void EventsData::AddActionInReport(ActionType actionType, LPCTSTR pszString, BOOL bInBeforeSection)
{
   	if (!m_pReportActions)
		m_pReportActions = new ReportEventData(m_SymTable);

	m_pReportActions->AddAction(actionType, pszString, bInBeforeSection);
}

//----------------------------------------------------------------------------
void EventsData::AddActionInAlways(ActionType actionType, LPCTSTR pszString)
{
   	if (!m_pReportActions)
		m_pReportActions = new ReportEventData(m_SymTable);

	m_pReportActions->AddActionInAlways(actionType, pszString);
}

//----------------------------------------------------------------------------
BOOL EventsData::Parse(Parser& lex)
{
	//lex.RemoveCommentTrace();
	lex.GetCommentTrace(this->m_arCommentTraceBefore);

	if (!lex.Matched(T_EVENTS))		
		return TRUE;

	if (lex.LookAhead(T_FORMFEED))
	{
    	SAFE_DELETE(m_pNewPageActions);
    	
		m_pNewPageActions = new NewPageActionData(m_SymTable);
    	if (!m_pNewPageActions->Parse(lex))
    	{
    		delete m_pNewPageActions;
    		m_pNewPageActions = NULL;
    		return FALSE;
    	}
    }

	if (lex.LookAhead(T_REPORT))
	{
    	SAFE_DELETE (m_pReportActions) ;
    	
		m_pReportActions = new ReportEventData(m_SymTable);
		if (!m_pReportActions->Parse(lex))
    	{
    		delete m_pReportActions;
    		m_pReportActions = NULL;
    		return FALSE;
    	}
    }

	while (lex.LookAhead(T_TABLE))
	{
		TableActionData* pTblAction = new TableActionData(m_SymTable);
		if (!pTblAction->Parse(lex))
		{
			delete pTblAction;
            return FALSE;
		}

		if (GetTableEvent(pTblAction->GetDisplayTableName()))
		{
			lex.SetError(_TB("Action already defined for the table"), pTblAction->GetDisplayTableName());
			delete pTblAction;
            return FALSE;
		}

        m_TableActions.Add(pTblAction);
    }

	while (lex.LookAhead(T_ID))
	{
		TriggEventData*	pTrigEventData = new TriggEventData(m_SymTable);
		if (!pTrigEventData->Parse(lex))
		{
			delete pTrigEventData;
            return FALSE;
		}

		if (GetEvent(pTrigEventData->GetEventName()))
		{
			lex.SetError(_TB("Event already defined"), pTrigEventData->GetEventName());
			delete pTrigEventData;
            return FALSE;
		}                     

		
		if (AfxGetExportSymbols()->IsActivated())
			AfxGetExportSymbols()->AddItemOnLastFile(pTrigEventData->GetEventName(), EXP_SYMB_EVENT);


		m_TriggEvents.Add(pTrigEventData);
	}

	lex.GetCommentTrace(this->m_arCommentTraceInner);

	BOOL bOk = (!lex.Bad() && lex.ParseEnd());
		
	lex.GetCommentTrace(this->m_arCommentTraceAfter);

	return bOk;
}

//----------------------------------------------------------------------------
void EventsData::Unparse(Unparser& oFile)
{
 	oFile.UnparseComment(this->m_arCommentTraceBefore);
	if (IsEmpty())
    	return;

    oFile.IncTab();

    oFile.UnparseCrLf	();
	oFile.UnparseTag	(T_EVENTS, TRUE);

    oFile.IncTab();

   	if (m_pNewPageActions)
		m_pNewPageActions->Unparse(oFile);

	if (m_pReportActions)
		m_pReportActions->Unparse(oFile);

	int i;
	int nNumItem = m_TableActions.GetSize();
	for (i = 0; i < nNumItem; i++)
	{
		TableActionData* pTblAction = (TableActionData*) m_TableActions[i];
		ASSERT_VALID(pTblAction);
		pTblAction->Unparse(oFile);
	}

    nNumItem = m_TriggEvents.GetSize();
	for (i = 0; i < nNumItem; i++)
	{
		TriggEventData*	pTrigEventData = (TriggEventData*) m_TriggEvents[i];
		ASSERT_VALID(pTrigEventData);
		pTrigEventData->Unparse(oFile);
	}
		
	oFile.UnparseComment(this->m_arCommentTraceInner);

	oFile.UnparseEnd(TRUE);
		
	oFile.UnparseComment(this->m_arCommentTraceAfter);
}

//----------------------------------------------------------------------------
int EventsData::GetTriggEventsSize() 
{
	return m_TriggEvents.GetSize();
}

//----------------------------------------------------------------------------
int EventsData::GetTableActionsSize() 
{
	return m_TableActions.GetSize();
}


//----------------------------------------------------------------------------
void EventsData::DeleteNewPageNamedAction (LPCTSTR pszName, ActionType anAction)
{
   	if (m_pNewPageActions)
		m_pNewPageActions->DeleteNamedAction(pszName, anAction);
}

//----------------------------------------------------------------------------
void EventsData::DeleteReportNamedAction (LPCTSTR pszName, ActionType anAction)
{
	if (m_pReportActions)
		m_pReportActions->DeleteNamedAction(pszName, anAction);
}

//----------------------------------------------------------------------------
void EventsData::DeleteTableActionsNamedAction
	(
		LPCTSTR pszEventsName,
		LPCTSTR pszName,
		ActionType anAction
	)
{
	int nNumItem = m_TableActions.GetSize();
	for (int i = 0; i < nNumItem; i++)
	{
		TableActionData* pTblAction = (TableActionData*) m_TableActions[i];
		if (_tcsicmp(pTblAction->GetDisplayTableName(), pszEventsName) == 0)
			pTblAction->DeleteNamedAction(pszName, anAction);
	}
}

//----------------------------------------------------------------------------
void EventsData::DeleteTriggEventsNamedAction
	(
		LPCTSTR pszEventsName,
		LPCTSTR pszName,
		ActionType anAction
	)
{
    int nNumItem = m_TriggEvents.GetSize();
	for (int i=0; i<nNumItem; i++)
	{
		TriggEventData*	pTrigEventData = (TriggEventData*) m_TriggEvents[i];
		
		if (_tcsicmp(pTrigEventData->GetEventName(), pszEventsName) == 0)
			pTrigEventData->DeleteNamedAction(pszName, anAction);
	}
}

//----------------------------------------------------------------------------
void EventsData::RenameNewPageNamedAction
	(
		LPCTSTR pszOldName,
		LPCTSTR pszNewName,
		ActionType anAction
	)
{
   	if (m_pNewPageActions)
		m_pNewPageActions->RenameNamedAction(pszOldName, pszNewName, anAction);
}

//----------------------------------------------------------------------------
void EventsData::RenameReportNamedAction
	(
		LPCTSTR pszOldName,
		LPCTSTR pszNewName,
		ActionType anAction
	)
{
	if (m_pReportActions)
		m_pReportActions->RenameNamedAction(pszOldName, pszNewName, anAction);
}

//----------------------------------------------------------------------------
void EventsData::RenameTableActionsNamedAction
	(
		LPCTSTR pszEventsName,
		LPCTSTR pszOldName,
		LPCTSTR pszNewName,
		ActionType anAction
	)
{
	int nNumItem = m_TableActions.GetSize();
	for (int i = 0; i < nNumItem; i++)
	{
		TableActionData* pTblAction = (TableActionData*) m_TableActions[i];
		if (_tcsicmp(pTblAction->GetDisplayTableName(), pszEventsName) == 0)
			pTblAction->RenameNamedAction(pszOldName, pszNewName, anAction);
	}
}

//----------------------------------------------------------------------------
void EventsData::RenameTriggEventsNamedAction
	(
		LPCTSTR pszEventsName,
		LPCTSTR pszOldName,
		LPCTSTR pszNewName,
		ActionType anAction
	)
{
    int nNumItem = m_TriggEvents.GetSize();
	for (int i = 0; i < nNumItem; i++)
	{
		TriggEventData*	pTrigEventData = (TriggEventData*) m_TriggEvents[i];
		
		if (_tcsicmp(pTrigEventData->GetEventName(), pszEventsName) == 0)
			pTrigEventData->RenameNamedAction(pszOldName, pszNewName, anAction);
	}
}

//----------------------------------------------------------------------------
BOOL EventsData::SearchNewPageNamedAction (LPCTSTR pszName, ActionType anAction)
{
	return m_pNewPageActions && m_pNewPageActions->SearchNamedAction(pszName, anAction);
}

//----------------------------------------------------------------------------
BOOL EventsData::SearchReportNamedAction (LPCTSTR pszName, ActionType anAction)
{
	return 	m_pReportActions && m_pReportActions->SearchNamedAction(pszName, anAction);
}

//----------------------------------------------------------------------------
BOOL EventsData::SearchTableActionsNamedAction
	(
		int nIndex,
		LPCTSTR pszName,
		CString& strEvent,
		ActionType anAction
	)
{
	TableActionData* pTblAction = (TableActionData*) m_TableActions[nIndex];

	if (pTblAction->SearchNamedAction(pszName, anAction))
	{
		strEvent = pTblAction->GetDisplayTableName();
		return TRUE;
	}
	
	return FALSE;
}

//----------------------------------------------------------------------------
BOOL EventsData::SearchTriggEventsNamedAction
	(
		int nIndex,
		LPCTSTR pszName,
		CString& strEvent,
		ActionType anAction
	)
{
	TriggEventData* pTriggEvents = (TriggEventData*) m_TriggEvents[nIndex];

	if (pTriggEvents->SearchNamedAction(pszName, anAction))
	{
		strEvent = pTriggEvents->GetEventName();
		return TRUE;
	}
	
	return FALSE;
}
