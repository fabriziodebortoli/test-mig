
#include "stdafx.h"

#include <TbNameSolver\TbNamespaces.h>
#include <TbParser\Parser.h>
#include <TbGenlib\FunProto.h>

#include "ActionsRepEngin.h"
#include "rpsymtbl.h"
#include "qrydata.h"

//............................... Resources
#include "edtcmm.hjson" //JSON AUTOMATIC UPDATE


//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif

//============================================================================
// GroupByAction
//============================================================================
//----------------------------------------------------------------------------
GroupByAction::GroupByAction()
	:
	m_pFunction	(NULL)
{
}

//----------------------------------------------------------------------------
GroupByAction::~GroupByAction()
{
	if (m_pFunction)	
	{
		delete m_pFunction;
		m_pFunction = NULL;
	}
}

//----------------------------------------------------------------------------
BOOL GroupByAction::CanDeleteField(LPCTSTR pszFieldName)
{
	if (m_pFunction)
	{
		if (!m_pFunction->CanDeleteField(pszFieldName))
        	return FALSE;
	}

	return TRUE;
}

//----------------------------------------------------------------------------
void GroupByAction::DeleteField(LPCTSTR pszFieldName)
{
	if (m_pFunction)
    {
		m_pFunction->DeleteField(pszFieldName);
		if (m_pFunction->IsEmpty())
		{
			delete m_pFunction;
			m_pFunction = NULL;		// must be set to NULL
        }
    }
}

//----------------------------------------------------------------------------
BOOL GroupByAction::IsEmpty()
{
	return m_pFunction == NULL;
}

//----------------------------------------------------------------------------
BOOL GroupByAction::Parse(Parser& lex, WoormTable* aSymTable)
{
	DataType dataType = DATA_NULL_TYPE;

	if (lex.LookAhead(T_ID))
	{
		 if (!lex.ParseID(m_strPublicName))
		 	return FALSE;
		 	
		 SymField* pF = aSymTable->GetField(m_strPublicName);
		 if (!pF)
		 	return lex.SetError(Expression::FormatMessage(Expression::UNKNOWN_FIELD), m_strPublicName);

		 if (!lex.ParseTag(T_ASSIGN))
		 	return FALSE;

		 dataType = pF->GetDataType();
	}

	m_pFunction = new FunctionData(aSymTable);
	if (!m_pFunction->Parse(lex, dataType))
	{
		delete m_pFunction;
        m_pFunction = NULL;
		return FALSE;
	}

    return TRUE;
}

//----------------------------------------------------------------------------
void GroupByAction::Unparse(Unparser& oFile)
{
	if (!m_strPublicName.IsEmpty())		// only for expression Function
	{
		oFile.UnparseID	(m_strPublicName,	FALSE);
		oFile.UnparseTag(T_ASSIGN, 		FALSE);
	}

	m_pFunction->Unparse(oFile);
}

//============================================================================
// GroupByData
//============================================================================
//----------------------------------------------------------------------------
GroupByData::GroupByData(WoormTable& aSymTable)
	:
	m_SymTable				(aSymTable),
	m_pTupleFilter			(new Expression(&aSymTable)),
	m_pGroupingTuple		(new ExpressionWithCheck(&aSymTable)),
	m_pHavingTupleFilter	(new Expression(&aSymTable))
{}

GroupByData::~GroupByData()
{
	SAFE_DELETE(m_pTupleFilter);
	SAFE_DELETE(m_pGroupingTuple);
	SAFE_DELETE(m_pHavingTupleFilter);
}

//----------------------------------------------------------------------------
BOOL GroupByData::CanConvertFieldToInput(LPCTSTR pszFieldName, CString& sLog) const
{
	if (m_pGroupingTuple->HasMember(pszFieldName))
	{
		sLog = _TB("The field is used by Grouping rules");
		return FALSE;
	}
	return TRUE;
}

//----------------------------------------------------------------------------
BOOL GroupByData::CanDeleteField(LPCTSTR pszFieldName)
{
	if (m_pTupleFilter && m_pTupleFilter->HasMember(pszFieldName))
		return FALSE;
	if (m_pGroupingTuple->HasMember(pszFieldName))
		return FALSE;
	if (m_pHavingTupleFilter && m_pHavingTupleFilter->HasMember(pszFieldName))
		return FALSE;

	for (int idx = 0; idx < m_ActionsArray.GetSize(); idx++)
	{
		GroupByAction* groupByAction = (GroupByAction*) m_ActionsArray[idx];

		if (!groupByAction->CanDeleteField(pszFieldName))
			return FALSE;
	}

    return TRUE;
}

//----------------------------------------------------------------------------
void GroupByData::DeleteField(LPCTSTR pszFieldName)
{
	// it is an array of index at GroupByAction that must be deleted.
    //
	CWordArray	idxToDelete;
	int			idx;

	for (idx = 0; idx < m_ActionsArray.GetSize(); idx++)
	{
		GroupByAction* groupByAction = (GroupByAction*) m_ActionsArray[idx];

		groupByAction->DeleteField(pszFieldName);

		// If the groupByAction is empty then it is added to the deleted list and after
		// it will be removed from the array.
		// This is done because it is not possible remove an object
		// while it run over the array.
		//
		if (groupByAction->IsEmpty())	
			idxToDelete.Add((WORD)idx);
	}

	// the run over the "idxToDelete" must be done from bottom to the top
	// so the absolute position not change
    //
	for (idx = idxToDelete.GetUpperBound(); idx >= 0; idx--)
	{
    	m_ActionsArray.RemoveAt(idxToDelete[ idx ]);
    }
}

//----------------------------------------------------------------------------
void GroupByData::Empty()
{
	m_pTupleFilter->Reset(FALSE);
	m_pGroupingTuple->Reset(FALSE);
	m_ActionsArray.RemoveAll();
	m_pHavingTupleFilter->Reset(FALSE);
}

//----------------------------------------------------------------------------
BOOL GroupByData::IsEmpty()
{
	return (m_pHavingTupleFilter == NULL || m_pHavingTupleFilter->IsEmpty())
			&&
			(m_pTupleFilter == NULL || m_pTupleFilter->IsEmpty())
			&&
			(m_pGroupingTuple == NULL || m_pGroupingTuple->IsEmpty() || m_ActionsArray.GetSize() == 0);
}

//----------------------------------------------------------------------------
BOOL GroupByData::ParseAction(Parser& lex)
{
	GroupByAction* groupAction = new GroupByAction;

	if (!groupAction->Parse(lex, &m_SymTable))
	{
		delete groupAction;
		return FALSE;
	}
	m_ActionsArray.Add(groupAction);

	if (!lex.ParseSep()) 
		return FALSE;
	return TRUE;
}

//----------------------------------------------------------------------------
BOOL GroupByData::ParseActionsBlock(Parser& lex)
{
	m_ActionsArray.RemoveAll();

	if (!lex.ParseBegin())
		return FALSE;

	do {
		if (!ParseAction(lex))
			return FALSE;
	} while (!lex.LookAhead(T_END) && !lex.Bad());

	if (!lex.ParseEnd()) 
		return FALSE;
	return TRUE;
}

//----------------------------------------------------------------------------
BOOL GroupByData::Parse(Parser& lex)
{
	CString pszFieldName;

	if (lex.LookAhead(T_WHERE))
	{
		SAFE_DELETE(m_pTupleFilter);
		m_pTupleFilter = new Expression(&m_SymTable);

		lex.SkipToken();
		if (!m_pTupleFilter->Parse(lex, DATA_BOOL_TYPE, TRUE))
			return FALSE;
		if (!lex.ParseSep())
			return FALSE;
	}

	if (lex.Matched(T_GROUP))
	{
		if (!lex.ParseTag(T_BY))								
			return FALSE;

		m_bForce = lex.Matched(T_FORCE);	//it force to allows all fields

		m_pGroupingTuple->SetStopTokens(T_DO);
		m_pGroupingTuple->m_bSkipCheck = m_bForce;
		if (!m_pGroupingTuple->Parse(lex, DATA_STR_TYPE, TRUE))
			return FALSE;

		if (!lex.ParseTag(T_DO))								
			return FALSE;

		if (lex.LookAhead(T_BEGIN))
		{
			if (!ParseActionsBlock(lex))
				return FALSE;
		}
		else
		{
			m_ActionsArray.RemoveAll();

			if (!ParseAction(lex))
				return FALSE;
		}
	}

	if (lex.LookAhead(T_HAVING))
	{
		SAFE_DELETE(m_pHavingTupleFilter);
		m_pHavingTupleFilter = new Expression(&m_SymTable);

		lex.SkipToken();
		if (!m_pHavingTupleFilter->Parse(lex, DATA_BOOL_TYPE, TRUE))
			return FALSE;
		if (!lex.ParseSep())
			return FALSE;
	}

    return TRUE;
}

//----------------------------------------------------------------------------
void GroupByData::Unparse(Unparser& oFile)
{
	ASSERT_VALID(m_pGroupingTuple);
	if (IsEmpty())
		return;

	if (m_pTupleFilter && !m_pTupleFilter->IsEmpty())
	{
		oFile.IncTab();
		oFile.UnparseTag(T_WHERE, FALSE);
		oFile.UnparseExpr(m_pTupleFilter->ToString(), FALSE);
		oFile.UnparseSep(TRUE);
		oFile.DecTab();
	}

	if (!m_pGroupingTuple->IsEmpty() && m_ActionsArray.GetSize() > 0)
	{
		oFile.IncTab();
		oFile.UnparseTag	(T_GROUP,		FALSE);
		oFile.UnparseTag	(T_BY, 			FALSE);

		if (m_bForce)
			oFile.UnparseTag(T_FORCE,		FALSE);

		oFile.UnparseExpr	(m_pGroupingTuple->ToString(), FALSE);
		oFile.UnparseTag	(T_DO, 			FALSE);

		if (m_ActionsArray.GetSize() > 1)
		{
			oFile.UnparseCrLf	();
			oFile.UnparseBegin	();
		}

		UnparseActions(oFile);

		if (m_ActionsArray.GetSize() > 1)
		{
			oFile.UnparseCrLf();
            oFile.UnparseEnd();
		}
		oFile.UnparseCrLf();
		oFile.DecTab();
	}

	if (m_pHavingTupleFilter && !m_pHavingTupleFilter->IsEmpty())
	{
		oFile.IncTab();
		oFile.UnparseTag(T_HAVING, FALSE);
		oFile.UnparseExpr(m_pHavingTupleFilter->ToString(), FALSE);
		oFile.UnparseSep(TRUE);
		oFile.DecTab();
	}
}

CString GroupByData::Unparse()
{
	Unparser buff(TRUE);
	Unparse(buff);
	buff.Close();

	CString sActionsList = buff.GetBufferString();
	return sActionsList;
}

//----------------------------------------------------------------------------
void GroupByData::UnparseActions(Unparser& oFile)
{
	for (int i=0; i < m_ActionsArray.GetSize(); i++)
	{
		GroupByAction* groupAction = (GroupByAction*) m_ActionsArray[i];
		groupAction->Unparse(oFile);
		oFile.UnparseSep (FALSE);

		if (i < m_ActionsArray.GetUpperBound())
			oFile.UnparseCrLf();
	}
}

CString GroupByData::UnparseActions()
{
	Unparser buff(TRUE);
	UnparseActions(buff);
	buff.Close();

	CString sActionsList = buff.GetBufferString();
	return sActionsList;
}

//----------------------------------------------------------------------------
BOOL GroupByData::CheckRuleItems(CString& sErr)
{
	for (int i = 0; i < m_ActionsArray.GetSize(); i++)
	{
		GroupByAction* groupAction = (GroupByAction*)m_ActionsArray[i];
		
		if (!groupAction->CheckRuleItems(sErr))
			return FALSE;
	}
	return TRUE;
}

//============================================================================
// QueryData
//============================================================================
//----------------------------------------------------------------------------
QueryData::QueryData(WoormTable& aSymTable)
	:
	m_SymTable			(aSymTable),
	m_RuleData			(aSymTable),
	m_pGroupBy			(new GroupByData(aSymTable))
{
}

//----------------------------------------------------------------------------
QueryData::~QueryData()
{
    SAFE_DELETE (m_pGroupBy);
}

//----------------------------------------------------------------------------
RuleDataObj* QueryData::GetRule(int index)
{
	return (RuleDataObj*) m_RuleData[index];
}

//----------------------------------------------------------------------------
int QueryData::GetNumRules()
{
	return m_RuleData.GetSize();
}

//----------------------------------------------------------------------------
RuleDataArray* QueryData::GetRuleData()
{
	return &m_RuleData;
}

//----------------------------------------------------------------------------
BOOL QueryData::IsEmpty()
{
	return m_RuleData.IsEmpty();
}

//----------------------------------------------------------------------------
BOOL QueryData::CanConvertFieldToInput(LPCTSTR pszFieldName, CString& sLog) const
{
	if (m_pGroupBy && !m_pGroupBy->CanConvertFieldToInput(pszFieldName, sLog))
		return FALSE;

	return TRUE;
}

//----------------------------------------------------------------------------
BOOL QueryData::CanDeleteField(LPCTSTR pszFieldName) const
{
	if (!m_RuleData.CanDeleteField(pszFieldName))					
		return FALSE;
	
	if (m_pGroupBy && !m_pGroupBy->CanDeleteField(pszFieldName))	
		return FALSE;

    return TRUE;
}

//----------------------------------------------------------------------------
void QueryData::DeleteField(LPCTSTR pszFieldName)
{
	m_RuleData.DeleteField(pszFieldName);

	if (m_pGroupBy)	
		m_pGroupBy->DeleteField(pszFieldName);
}

//----------------------------------------------------------------------------
void QueryData::RenameField(LPCTSTR pszOldFieldName, LPCTSTR pszNewFieldName)
{
	m_RuleData.RenameField(pszOldFieldName, pszNewFieldName);

	//TODO if (m_pGroupBy) m_pGroupBy->RenameField(pszOldFieldName, pszNewFieldName);
}

//----------------------------------------------------------------------------
void QueryData::SetGroupBy(GroupByData* pNewGroupBy)
{
	if (pNewGroupBy)
	{
		SAFE_DELETE (m_pGroupBy);
		m_pGroupBy = pNewGroupBy;
	}
}

//----------------------------------------------------------------------------
BOOL QueryData::Parse(Parser& lex, SqlConnection* pSqlConnection)
{
	if (!m_RuleData.Parse(lex, pSqlConnection))
		return FALSE;

	if (!m_pGroupBy->Parse(lex))
		return FALSE;

	return TRUE;
}

//----------------------------------------------------------------------------
void QueryData::Unparse(Unparser& oFile)
{
	m_RuleData.Unparse(oFile);

	m_pGroupBy->Unparse(oFile);
}
