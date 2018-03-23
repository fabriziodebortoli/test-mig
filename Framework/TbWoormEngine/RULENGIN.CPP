
#include "stdafx.h"

#include <TbGeneric\LocalizableObjs.h>

#include <TbGenlib\funproto.h>
#include <TbGenlib\baseapp.h>
#include <TbGenlib\oslbaseinterface.h>

#include <TbOleDb\oledbmng.h>
#include <TbOleDb\sqlcatalog.h>
#include <TbOleDb\sqlrec.h>
#include <TbOleDb\sqltable.h>
#include <TbOleDb\wclause.h>

#include "ActionsRepEngin.h"
#include "qrywrkid.h"
#include "repfield.h"
#include "reptable.h"
#include "ruledata.h"
#include "repengin.h"
#include "rulengin.h"
#include "report.h"
#include "qryengin.h"
#include "ProcData.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif

//==============================================================================
//
//#define	VALID_FIELD		_T('1')
//#define	NULL_FIELD		_T('0')

// FieldStatusForTemporaryFile
// Poiche` e` ammessa l'estrazione di dati NULLI (cioe` non risolti dalla Select),
// nel caso di costruzione del temporaneo e` necessario aggiungere un campo fittizio
// costituito da uno stream di byte (tanti quanti sono i campi del data record del
// temporaneo) in cui l'i-esimo byte contiene '1' o '0' a seconda che l'i-esimo campo
// del data record sia VALIDO o NULLO.
// Questo permette in fase di retieving di rileggere i dati convalidandoli in maniera
// corretta ed inoltre raggruppandoli e/o sortandoli in maniera coerente.
//
//static const TCHAR BASED_CODE szFieldStatusName[]	= _T("FSFTF_G_T_");
//static const TCHAR BASED_CODE szTmpTableName[]	= _T("T_TMP_G_T_");	//@@TODO

//==============================================================================

IMPLEMENT_DYNAMIC(COrderColumn, CObject)

//==============================================================================
//
//			Class RuleEngine implementation
//
//==============================================================================
IMPLEMENT_DYNAMIC(RuleEngine, CObject);

//-----------------------------------------------------------------------------
RuleEngine::RuleEngine (WoormTable& aSymTable, EngineScheduler& aScheduler)
	:
	m_SymTable			(aSymTable),
	m_Scheduler			(aScheduler),
	m_EngineStatus		(8),
	m_pTmpRule			(NULL),
	m_SortedRules		(),
	m_UnsortedRules		(),
	m_pFieldsStatus		(NULL),
	m_nStartPos			(0),
	m_nStopPos			(0),
	m_nLastElementPos	(-1),
	m_nLastSubTreePos	(0),
	m_nTotOrderBySeg	(0),
	m_nTotSubTreeNodes	(0),
	m_nUnsRulesNumber	(0),
	m_nNullRulesNum		(0)
{
	//TODO verificare m_SortedRules.SetOwns(FALSE);
}

//-----------------------------------------------------------------------------
RuleEngine::~RuleEngine ()
{
	SAFE_DELETE (m_pFieldsStatus);
	SAFE_DELETE (m_pTupleFilterEngine);

	//int c1 = m_UnsortedRules.GetCountNotNull();
	//int c2 = m_SortedRules.GetCountNotNull();
	
	//m_UnsortedRules.RemoveAll();
	//m_SortedRules.RemoveAll();
}

//-----------------------------------------------------------------------------
BOOL RuleEngine::UserBreak ()
{
	return m_Scheduler.UserBreak();
}

//-----------------------------------------------------------------------------
BOOL RuleEngine::SetRTError (EngineScheduler::MessageID nErrCod, const CString& strErrExpl, LPCTSTR str)
{
	m_Scheduler.SetRTError(nErrCod, strErrExpl, str);
	return FALSE;
}

//-----------------------------------------------------------------------------
CString RuleEngine::FormatRTError ()
{
	return m_Scheduler.FormatRTError();
}

//-----------------------------------------------------------------------------
void RuleEngine::AddRule(RuleObj* pRule)
{
	int nPos = m_UnsortedRules.Add(pRule);
	
	pRule->m_nRuleTag = nPos;
}

//-----------------------------------------------------------------------------
BOOL RuleEngine::BuildTree (Parser& parser, GroupBy* pGroupByExp)
{
	ResetRTError();

	if ((m_nUnsRulesNumber = m_UnsortedRules.GetSize()) == 0)
		return TRUE;

	// + 1 is for eventual temporary rule
	m_SortedRules.SetSize(m_nUnsRulesNumber + 1);

	// makes the dependency rules graph
	BuildLinks();

	// checks for any recursion
	if (!CheckRecursion())
	{
		TraceRules ();
		return parser.SetError(EngineScheduler::FormatMessage(EngineScheduler::QUERY_RULES_RECURSIVE_ERR));
	}

	if (m_EngineStatus[GROUPBY])
	{
		CObArray joinedRules;

		if (pGroupByExp->IsTemporaryRequired(joinedRules))
			m_EngineStatus.On(TEMP);

		// the group criterion is the most important for tree building
		FindSubTree(joinedRules);

		if (m_EngineStatus[TEMP])
		{
			m_SymTable.GetDocument()->Message(
				_TB("The grouping criteria is incompatible to the rules order"),
				MB_ICONSTOP
			);
			TraceRules ();
			return FALSE;	//@@TODO
			//m_EngineStatus.On(GROUP_ON_TEMP);
		}
	}

	// it builds a sub-tree moving the rules from the unsorted array to the
	// sorted array
	// if FindSubTreeAndIndex was find a sub-tree the BuildSubTree returns
	// the last rule index of first tree to make temporary file
	//
	m_nLastSubTreePos = BuildSubTree();
	TraceRules ();

	// if it isn't necessary a temporary then building is finished
	if (!m_EngineStatus[TEMP]) 
		return TRUE;


	m_SymTable.GetDocument()->Message
		(
			_TB("The grouping criteria is incompatible to the rules order"),
			 MB_ICONSTOP
		);
	//TraceRules ();
	return FALSE;
}

//-----------------------------------------------------------------------------
void RuleEngine::TraceRules(CObArray& ar, CString title)
{
	TRACE((LPCTSTR) title);

	for (int kk = 0; kk < ar.GetSize(); kk++)
	{
		RuleObj* pR = (RuleObj*)ar[kk];
		if (!pR) continue;
		ASSERT_VALID(pR);
		TRACE((LPCTSTR)::cwsprintf(L"Rule (index:%d, tag:%d, orderpos:%d, id:%d, marked:%d, sorted:%d) %s\n",
			kk, pR->m_nRuleTag, pR->m_nOrderByPos, pR->m_nRuleId, pR->m_bMark, pR->m_bSorted, (LPCTSTR)pR->GetDescription()));

		if (pR->m_nParentsNum)
		{
			for (int i = 0; i < min(pR->m_nParentsNum, pR->m_Parents.GetSize()); i++)
			{
				RuleObj* parent = dynamic_cast<RuleObj*>(pR->m_Parents[i]);
				if (parent)
				{
					TRACE3("\tParent rule (index:%d, tag:%d) %s\n", i, parent->m_nRuleTag, (LPCTSTR)parent->GetDescription());
				}
			}
		}
		if (pR->m_Sons.GetSize())
		{
			for (int i = 0; i < pR->m_Sons.GetSize(); i++)
			{
				RuleObj* son = dynamic_cast<RuleObj*>(pR->m_Sons[i]);
				if (son)
				{
					TRACE3("\tSon rule (index:%d, tag:%d) %s\n", i, son->m_nRuleTag, (LPCTSTR)son->GetDescription());
				}
			}
		}
	}
}

void RuleEngine::TraceRules ()
{
#ifdef _DEBUG

	TraceRules(m_UnsortedRules, ::cwsprintf(L"\nUnsorted Rules (LastSubTreePos: %d)\n", m_nLastElementPos));

	TraceRules(m_SortedRules, ::cwsprintf(L"\nSorted Rules\n"));

#endif
}

//-----------------------------------------------------------------------------
// Build the dependency graph:
// every rules will have the list of own parents and own sons
//
void RuleEngine::BuildLinks ()
{
	// it's used to break the rule moving if an order-by criterion exists
	//
	m_nTotSubTreeNodes = 0;

	int p,s;

	// initial setting

	int nr = m_nUnsRulesNumber;
	for (p = 0; nr > 0; p++)
		if (m_UnsortedRules[p] != NULL)
		{
			nr--;
			RuleObj* pRule = (RuleObj*) m_UnsortedRules[p];
			pRule->m_Sons.RemoveAll();
			pRule->m_Parents.RemoveAll();
			
			pRule->m_nParentsNum	= 0;
			pRule->m_nRuleId		= p;
			pRule->m_bMark			= FALSE;
			pRule->m_nOrderByPos	= -1;
		}

	int np = m_nUnsRulesNumber;
	for (p = 0; np > 0; p++)
		if (m_UnsortedRules[p] != NULL)
		{
			np--;
			RuleObj* pParRule = (RuleObj*) m_UnsortedRules[p];

			int ns = m_nUnsRulesNumber;

			for (s = 0; ns > 0; s++)
				if (m_UnsortedRules[s] != NULL)
				{
					ns--;
					if (p != s)
					{
						RuleObj* pSonRule = (RuleObj*) m_UnsortedRules[s];

						if (pParRule->IsParentOf(*pSonRule))
						{
							pParRule->m_Sons.Add(pSonRule);
							pSonRule->m_Parents.Add(pParRule);
							pSonRule->m_nParentsNum++;
						}
					}
				}
		}
}

//-----------------------------------------------------------------------------
BOOL RuleEngine::CheckRecursion ()
{
	int nr = m_nUnsRulesNumber;
	for (int p = 0; nr > 0; p++)
		if (m_UnsortedRules[p] != NULL)
		{
			nr--;
			if (!CheckDescendents((RuleObj&) *m_UnsortedRules[p]))
				return FALSE;
		}

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL RuleEngine::CheckDescendents (RuleObj& aParentRule)
{
	// tests if aParentRule was already encountered
	//
	if (aParentRule.m_bMark) return FALSE;

	if (aParentRule.m_Sons.GetSize() != 0)
	{
		// this is a local use of m_bMark data member to test as above the node tracking
		//
		aParentRule.m_bMark = TRUE;

		for (int i = 0; i < aParentRule.m_Sons.GetSize(); i++)
			if (!CheckDescendents(*((RuleObj*) aParentRule.m_Sons[i])))
				return FALSE;

		aParentRule.m_bMark = FALSE;		// reset the initial value
	}

	return TRUE;
}

//-----------------------------------------------------------------------------
void RuleEngine::FindSubTree (CObArray& joinedRules)
{
	m_nTotOrderBySeg = joinedRules.GetSize();
	if (m_nTotOrderBySeg == 0) 
		return;		// empty order by

	for (int pos = 0; pos < m_nTotOrderBySeg; pos++)
	{
		RuleObj& aSegRule = *((RuleObj*) joinedRules[pos]);

		if (aSegRule.m_nOrderByPos == -1)
		{
			aSegRule.m_nOrderByPos = pos;
			MakeSubTreeNodes(aSegRule);
		}
		else
		{
			m_EngineStatus.On(TEMP);
		}
	}
}

//-----------------------------------------------------------------------------
void RuleEngine::MakeSubTreeNodes (RuleObj& aSonRule)
{
	if (!aSonRule.m_bMark)
	{
		// signal to MoveRule function that this rule is a sub-tree-node
		aSonRule.m_bMark = TRUE;
		m_nTotSubTreeNodes++;
	}

	for (int i = 0; i < aSonRule.m_Parents.GetSize(); i++)
	{
		RuleObj& aParentRule = *((RuleObj*) aSonRule.m_Parents[i]);
		MakeSubTreeNodes(aParentRule);
	}
}

//-----------------------------------------------------------------------------
// move the rules to an array that will contain the rules sorted according to
// the tree traversing
//
int RuleEngine::BuildSubTree ()
{
	int nr;

	BOOL	bContinueScans	= TRUE;
	BOOL	bRuleMoved	= TRUE;
	int		nCurrPos	= 0;

	for(;;)
	{
		while(((nr = m_nUnsRulesNumber) > 0) && bContinueScans && bRuleMoved)
		{
			bRuleMoved = FALSE;
			for (int p = 0; nr > 0 && bContinueScans && !bRuleMoved; p++)
				if (m_UnsortedRules[p] != NULL)
				{
					nr--;
					MoveRule(
							*((RuleObj*) m_UnsortedRules[p]),
							bContinueScans,
							bRuleMoved,
							nCurrPos
							);
				}
		}

		if ((nr > 0) && bContinueScans && !bRuleMoved && !m_EngineStatus[TEMP])
		{
			m_EngineStatus.On(TEMP);
		}
		else
			break;
	}

	return m_nLastElementPos;
}

//-----------------------------------------------------------------------------
// move avery rule according to eventual group-by/order-by criteria
//
void RuleEngine::MoveRule (RuleObj& aParentRule, BOOL& bContinueScans, BOOL& bRuleMoved, int& nCurrPos)
{
	if (aParentRule.m_nParentsNum != 0) 
		return;

	if (m_EngineStatus[GROUPBY])
	{
		if (!m_EngineStatus[TEMP] && nCurrPos < m_nTotOrderBySeg)
			if (aParentRule.m_nOrderByPos != nCurrPos)
				return;
			else
				nCurrPos++;

		if (!m_EngineStatus[GROUPBY] && m_nTotSubTreeNodes > 0)
			if (!aParentRule.m_bMark)
				return;
			else
				if ((--m_nTotSubTreeNodes == 0) && m_EngineStatus[TEMP]) 
					bContinueScans = FALSE;
	}

// At this point we must remove the actual aParentRule from unsorted array to insert it into
// sorted array, but to do that we leave a hole in the unsorted array, first because it
// is owner of stored objects and, second, if we need further pass on this array the index
// position stored in the others rules it's relative to the actual array state.

	m_UnsortedRules.SetAt(aParentRule.m_nRuleId, NULL);
	m_nUnsRulesNumber--;

	m_SortedRules.SetAt((aParentRule.m_nRuleId = ++m_nLastElementPos), &aParentRule);
	aParentRule.m_bSorted = TRUE;

	bRuleMoved = TRUE;

	for (int i = 0; i < aParentRule.m_Sons.GetSize(); i++)
	{
		RuleObj& aSonRule = *((RuleObj*) aParentRule.m_Sons[i]);
		aSonRule.m_nParentsNum--;
		if (bContinueScans)
			MoveRule(aSonRule, bContinueScans, bRuleMoved, nCurrPos);
	}
}

//-----------------------------------------------------------------------------
BOOL RuleEngine::ApplyRules (const Array* arOrderOnThisColumns /*=NULL*/, CString* pstrAuxWhereClause /*=NULL*/)
{
	ResetRTError();
	m_nNullRulesNum	= 0;

	m_nStartPos	= 0;
	m_SymTable.SetDataLevel(RULE_ENGINE);

	if (!m_EngineStatus[TEMP])
	{
		m_nStopPos = m_nLastElementPos;
		return ApplyFrom(m_nStartPos, arOrderOnThisColumns, pstrAuxWhereClause) == RULE_SUCCESS;
	}
	
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL RuleEngine::Retrieve ()
{
	ResetRTError();
	m_nNullRulesNum	= 0;

	if (!m_EngineStatus[TEMP]) return TRUE;

	BOOL ok = TRUE;

	m_SymTable.SetDataLevel(RULE_ENGINE);

	// save current engine status and switch to a normal data base inquiry
	//
	m_EngineStatus.Off(TEMP);

	BOOL gb		= m_EngineStatus[GROUPBY];
	BOOL gbot	= m_EngineStatus[GROUP_ON_TEMP];

	if (gb)
	{
		m_EngineStatus.Off(GROUPBY);
		if (gbot) m_EngineStatus.Off(GROUP_ON_TEMP);
	}

	// if there is an oredring status on temporary file then set appropriate
	// index for reading from it
	//     

//@@TODO	if (m_EngineStatus[ORDERBY])
//@@TODO		ok = m_pTmpRule->SetSortString(m_pOrderByExp->GetModifiedString());

	if (ok)
	{                                         
		m_nStartPos	= m_nLastSubTreePos + 1;
		m_nStopPos	= m_nLastElementPos;
	
		// read from temporary file applying tmpRule and all linked
		// remaining rules
		//
		ok = (ApplyFrom(m_nStartPos) == RULE_SUCCESS);
	}
	
	// close and destroy the temporary file
	//
	//DropTmpTable();

	// restore original engine status
	//
	if (gb)
	{
		m_EngineStatus.On(GROUPBY);
		if (gbot) m_EngineStatus.On(GROUP_ON_TEMP);
	}

	m_EngineStatus.On(TEMP);

	return ok;
}

//-----------------------------------------------------------------------------
RuleReturn RuleEngine::ApplyFrom (int fromRule, const Array* arOrderOnThisColumns /*=NULL*/, CString* pstrAuxWhereClause /*=NULL*/)
{
	if (UserBreak()) return RULE_ABORT;

	ResetRTError();

	if (fromRule > m_nStopPos)
	{
		DataBool filterTuple(TRUE);

		if (m_pTupleFilterEngine && !m_pTupleFilterEngine->Eval(filterTuple))
		{
			return RULE_ABORT;
		}
		if (!filterTuple)
			return RULE_SUCCESS;//skip tuple

		if (!TupleProcessing())
		{
			SetRTError(EngineScheduler::QUERY_RULES_TUPLE_PROC_ERR);
			return RULE_ABORT;
		}

		m_SymTable.SetDataLevel(RULE_ENGINE);

		return RULE_SUCCESS;
	}

	RuleObj* theRule = (RuleObj*) m_SortedRules[fromRule];

	if (theRule == NULL)
	{
		SetRTError(EngineScheduler::QUERY_RULES_NORULE_ERR);
		return RULE_ABORT;
	}

	return theRule->Apply(arOrderOnThisColumns, pstrAuxWhereClause);
}

//==============================================================================
//
//			Class RuleObj implementation
//
//==============================================================================
IMPLEMENT_DYNAMIC(RuleObj, CObject)

//-----------------------------------------------------------------------------
RuleObj::RuleObj (RuleEngine& aOwnerEngine)
	: 
	IDisposingSourceImpl(this),

	m_OwnerEngine	(aOwnerEngine),
	
	m_pBefore	(new DebugBlock(dynamic_cast<RepEngine*>(&aOwnerEngine))),
	m_pAfter	(new DebugBlock(dynamic_cast<RepEngine*>(&aOwnerEngine)))
{
	m_nOrderByPos	= -1;
	m_bSorted		= FALSE;
	m_bFailed		= FALSE;
	m_bNullSolution	= FALSE;
}

RuleObj::~RuleObj ()
{
	SAFE_DELETE(m_pBefore);
	SAFE_DELETE(m_pAfter);
}

//-----------------------------------------------------------------------------
BOOL RuleObj::HasSon (RuleObj& aRuleObj) const
{
	for (int i = 0; i < m_Sons.GetSize(); i++)
		if (((RuleObj*) m_Sons[i])->m_nRuleId == aRuleObj.m_nRuleId) 
			return TRUE;

	return FALSE;
}

//-----------------------------------------------------------------------------
BOOL RuleObj::IsSonOfNullRules () const
{
	for (int i = 0; i < m_Parents.GetSize(); i++)
		if (((RuleObj*) m_Parents[i])->m_bNullSolution)
			return TRUE;

	return FALSE;
}

//-----------------------------------------------------------------------------
BOOL RuleObj::IsParentOfFailedRules () const
{
	for (int i = 0; i < m_Sons.GetSize(); i++)
		if (((RuleObj*) m_Sons[i])->m_bFailed)
			return TRUE;

	return FALSE;
}

//-----------------------------------------------------------------------------
BOOL RuleObj::IsParentOf(RuleObj& aRuleObj) const
{
	if (aRuleObj.m_bSorted)
		return HasSon(aRuleObj);
	else
	{
		for (int i = 0; i < GetSelFieldsNum(); i++)
			if (aRuleObj.FindKeyName(GetSelFieldName(i)))
				return TRUE;

		return FALSE;
	}
}

//-----------------------------------------------------------------------------
BOOL RuleObj::IsRoot () const
{
	return m_Parents.GetSize() == 0;
}

//-----------------------------------------------------------------------------
CString RuleObj::ToString()
{
	Unparser buff(TRUE);
	Unparse(buff);
	buff.Close();

	CString sRule = buff.GetBufferString();
	return sRule;
}

//-----------------------------------------------------------------------------
void RuleObj::GetSelFieldNames (CStringArray& ar) const
{
	ar.RemoveAll();
	ar.SetSize(GetSelFieldsNum());
	for (int i = 0; i < GetSelFieldsNum(); i++)
		ar[i] = GetSelFieldName(i);
}

/////////////////////////////////////////////////////////////////////////////
// Diagnostics
#ifdef _DEBUG
void RuleObj::Dump(CDumpContext& dc) const
{
	ASSERT_VALID(this);
	AFX_DUMP1(dc, "\n\t(Tb)RuleObj = ", this->GetRuntimeClass()->m_lpszClassName);
}
#endif //_DEBUG

//==============================================================================
//
//			Class LetStatement implementation
//
//==============================================================================
IMPLEMENT_DYNAMIC(LetStatement, Expression)

//----------------------------------------------------------------------------
LetStatement::LetStatement (SymTable* pSymTab)
	:
	Expression	(pSymTab),
	m_pField	(NULL)
{}

//----------------------------------------------------------------------------
WoormField* LetStatement::Parse (Parser& parser)
{
	CString strFieldNameTo;

	if (parser.ParseID(strFieldNameTo))
	{
		m_pField = (WoormField*) m_pSymTable->GetField(strFieldNameTo);
		if (m_pField == NULL)
			parser.SetError(Expression::FormatMessage(Expression::UNKNOWN_FIELD), strFieldNameTo);
		else
			if (m_pField->IsReadOnly())
				parser.SetError(_TB("Read-only field"), strFieldNameTo);
			else
				if (m_pField->GetOwnerRule())
					parser.SetError(_TB("Field already used by another rule"), strFieldNameTo);
				else
					if (parser.ParseTag(T_ASSIGN))
						if (Expression::Parse(parser, m_pField->GetDataType()))
							return m_pField;
	}

	return NULL;
}

//-----------------------------------------------------------------------------
CString LetStatement::GetDescription() const
{
	return cwsprintf(L"Let %s = %s", GetFieldName(), const_cast<LetStatement*>(this)->ToString());
}

//==============================================================================
//
//			Class ExpRule implementation
//
//==============================================================================

IMPLEMENT_DYNAMIC(ExpRule, RuleObj)
//-----------------------------------------------------------------------------
ExpRule::ExpRule ( RuleEngine& aOwnerEngine)
	:
	RuleObj			(aOwnerEngine),
	m_Expr			(&(aOwnerEngine.GetSymTable())),
	m_ValidateWhereExpr(&(aOwnerEngine.GetSymTable()))
{
}

//-----------------------------------------------------------------------------
RuleType ExpRule::IsARule () const
{
	return RULE_EXPR;
}

//---------------------------------------------------------------------------
BOOL ExpRule::Parse (Parser& parser)
{
	m_Expr.SetStopTokens(T_WHERE, T_SEP);

	WoormField* pRI = m_Expr.Parse(parser);
	if (pRI)
	{
		pRI->SetOwnerRule(this);

		if (parser.Matched(T_WHERE) && !m_ValidateWhereExpr.Parse(parser, DATA_BOOL_TYPE))
			return FALSE;

		if (!parser.ErrorFound()) parser.ParseSep();
	}

	m_pBefore->m_strOwnerName = L"Rule." + GetDescription() + L".Before";
	m_pAfter->m_strOwnerName = L"Rule." + GetDescription() + L".After";

	return !parser.ErrorFound();
}

//----------------------------------------------------------------------------
void ExpRule::Unparse(Unparser& oFile)
{
	BOOL bUnparseWhereClause = TRUE;
	BOOL bUnparseId = FALSE;

	if (!m_Expr.IsEmpty())
	{
		if (bUnparseId)
		{
			oFile.UnparseID(m_Expr.m_pField->GetName(), FALSE);
			oFile.UnparseTag(T_ASSIGN, FALSE);
		}
		oFile.UnparseExpr(m_Expr.ToString(), FALSE);
	}

	if (bUnparseWhereClause && !m_ValidateWhereExpr.IsEmpty())
	{
		oFile.UnparseCrLf();
		oFile.UnparseTag(T_WHERE, FALSE);
		oFile.UnparseExpr(m_ValidateWhereExpr.ToString(), FALSE);
	}

	if (bUnparseId)
		oFile.UnparseSep(TRUE);
}

//-----------------------------------------------------------------------------
BOOL ExpRule::FindKeyName (LPCTSTR pszFieldName)
{
	if (m_Expr.HasMember(pszFieldName))
		return TRUE;
	if (m_ValidateWhereExpr.HasMember(pszFieldName))
		return TRUE;

	return FALSE;
}

//-----------------------------------------------------------------------------
CString ExpRule::GetDescription() const
{
	//return ::cwsprintf(L"%s = ...", m_Expr.GetFieldName());
	return m_Expr.GetFieldName();
}

//-----------------------------------------------------------------------------
RuleReturn ExpRule::Apply (const Array* arOrderOnThisColumns /*= NULL*/, CString* pstrAuxWhereClause /*= NULL*/)
{
	this->m_pBefore->Exec();	//for RS-DEBUGGER only

	WoormField* pRI  = (WoormField*) m_Expr.GetField();

	// try to eval the expression also if the ancestors are evaluated null because the dependency
	// could be due to the where-clause expression
	if ((m_OwnerEngine.m_nNullRulesNum == 0) || !IsSonOfNullRules() || !m_ValidateWhereExpr.IsEmpty())
	{
		if (!m_Expr.IsEmpty() &&  !m_Expr.Eval(*(pRI->GetRuleData())))
			if (m_Expr.GetErrId() != Expression::NULL_OPR)
			{
				m_OwnerEngine.SetRTError(EngineScheduler::QUERY_EXP_RULE_EVAL_EXP_ERR, m_Expr.GetErrDescription(),
										pRI->GetName());
				return RULE_ABORT;
			}
			else		// necessary if the field is operand in the where-clause expression
				pRI->RuleNullified();

		if (!m_ValidateWhereExpr.IsEmpty())
		{
			DataBool goodVal(FALSE);

			if (!m_ValidateWhereExpr.Eval(goodVal) &&
				(m_ValidateWhereExpr.GetErrId() != Expression::NULL_OPR))
			{
				m_OwnerEngine.SetRTError(EngineScheduler::QUERY_EXP_RULE_EVAL_FILT_ERR, m_ValidateWhereExpr.GetErrDescription(),
										pRI->GetName());
				return RULE_ABORT;
			}

			if (!((int) goodVal))
			{
				m_bFailed = TRUE;
				m_OwnerEngine.SetRTError(EngineScheduler::QUERY_EXP_RULE_BACKTRACK, _T(""), pRI->GetName());

				return RULE_BACKTRACK;
			}
		}

		if (!m_Expr.GetErrId())
		{
			m_bFailed = FALSE;

			if (m_bNullSolution)
			{
				m_bNullSolution = FALSE;
				m_OwnerEngine.m_nNullRulesNum--;
			}

			pRI->RuleUpdated();

			this->m_pAfter->Exec(this);	//for RS-DEBUGGER only

			return m_OwnerEngine.ApplyFrom(m_nRuleId + 1, arOrderOnThisColumns, pstrAuxWhereClause);
		}
	}

	m_bFailed = FALSE;

	if (!m_bNullSolution)
	{
		m_bNullSolution = TRUE;
		m_OwnerEngine.m_nNullRulesNum++;
	}

	pRI->RuleNullified();

	this->m_pAfter->Exec();	//for RS-DEBUGGER only

	return m_OwnerEngine.ApplyFrom(m_nRuleId + 1, arOrderOnThisColumns, pstrAuxWhereClause);
}


//==============================================================================
//
//			Class CondRule implementation
//
//==============================================================================
IMPLEMENT_DYNAMIC(CondRule, RuleObj)

//-----------------------------------------------------------------------------
CondRule::CondRule (RuleEngine& aOwnerEngine)
	:
	RuleObj			(aOwnerEngine),
	m_TestExpr		(&(aOwnerEngine.GetSymTable())),
	m_ThenExpr		(&(aOwnerEngine.GetSymTable())),
	m_ElseExpr		(&(aOwnerEngine.GetSymTable())),
	m_ValidateWhereExpr(&(aOwnerEngine.GetSymTable()))
{
}

//-----------------------------------------------------------------------------
RuleType CondRule::IsARule () const
{
	return RULE_COND_EXPR;
}

//---------------------------------------------------------------------------
BOOL CondRule::Parse(Parser& parser)
{
	BOOL bParseSep = TRUE;

	if (parser.ParseTag(T_IF))
	{
		m_TestExpr.SetStopTokens(T_THEN);

		if (!m_TestExpr.Parse(parser, DATA_BOOL_TYPE))
			return FALSE;

		if (parser.ParseTag(T_THEN))
		{
			m_ThenExpr.SetStopTokens(T_ELSE, T_WHERE, T_SEP);
			WoormField* pRIThen = m_ThenExpr.Parse(parser);
			if (pRIThen == NULL) return FALSE;

			bParseSep = !parser.Matched(T_SEP);

			if (parser.Matched(T_ELSE))
			{
				m_ElseExpr.SetStopTokens(T_WHERE, T_SEP);
				WoormField* pRIElse =  m_ElseExpr.Parse(parser);
				if (pRIElse == NULL) return FALSE;

				if (pRIThen != pRIElse)
				{
					CString strName(pRIThen->GetName());
					strName += ", ";
					strName += pRIElse->GetName();
					return parser.SetError(EngineScheduler::FormatMessage(EngineScheduler::QUERY_NOT_SAME_FIELD), strName);
				}

				bParseSep = !parser.Matched(T_SEP);
			}

			pRIThen->SetOwnerRule(this);

			if (!parser.ErrorFound() && parser.Matched(T_WHERE))
			{
				if (!m_ValidateWhereExpr.Parse(parser, DATA_BOOL_TYPE))
					return FALSE;

				bParseSep = TRUE;
			}

			if (!parser.ErrorFound() && bParseSep)
				parser.ParseSep();
		}
	}

	m_pBefore->m_strOwnerName = L"Rule." + GetDescription() + L".Before";
	m_pAfter->m_strOwnerName = L"Rule." + GetDescription() + L".After";

	return !parser.ErrorFound();
}

//----------------------------------------------------------------------------
void CondRule::Unparse(Unparser& oFile)
{
	BOOL bUnparseWhereClause = TRUE;
	BOOL bUnparseId = FALSE;

	oFile.UnparseTag(T_IF, FALSE);
	oFile.UnparseExpr(m_TestExpr.ToString(), TRUE);

	oFile.IncTab();
	oFile.UnparseTag(T_THEN, FALSE);
	if (bUnparseId)
	{
		oFile.UnparseID(m_ThenExpr.m_pField->GetName(), FALSE);
		oFile.UnparseTag(T_ASSIGN, FALSE);
	}
	oFile.UnparseExpr(m_ThenExpr.ToString(), FALSE);

	if (!m_ElseExpr.IsEmpty())
	{
		oFile.UnparseCrLf();
		oFile.UnparseTag(T_ELSE, FALSE);
		if (bUnparseId)
		{
			oFile.UnparseID(m_ElseExpr.m_pField->GetName(), FALSE);
			oFile.UnparseTag(T_ASSIGN, FALSE);
		}
		oFile.UnparseExpr(m_ElseExpr.ToString(), FALSE);
	}

	oFile.DecTab();

	if (bUnparseWhereClause && !m_ValidateWhereExpr.IsEmpty())
	{
		oFile.UnparseCrLf();
		oFile.UnparseTag(T_WHERE, FALSE);
		oFile.UnparseExpr(m_ValidateWhereExpr.ToString(), FALSE);
	}

	if (bUnparseId)
		oFile.UnparseSep(TRUE);
}

//-----------------------------------------------------------------------------
BOOL CondRule::FindKeyName (LPCTSTR pszFieldName)
{
	if (m_TestExpr.HasMember(pszFieldName)) 
		return TRUE;
	if (m_ThenExpr.HasMember(pszFieldName)) 
		return TRUE;
	if (m_ElseExpr.HasMember(pszFieldName)) 
		return TRUE;
	if (m_ValidateWhereExpr.HasMember(pszFieldName))
		return TRUE;

	return FALSE;
}

//-----------------------------------------------------------------------------
CString CondRule::GetDescription() const
{
	ASSERT_VALID(m_ThenExpr.GetField());
	//if (m_ElseExpr.IsEmpty())
	//	return cwsprintf(L"If-Then %s = ...", m_ThenExpr.GetFieldName());
	//return cwsprintf(L"If-Then-Else %s = ...", m_ThenExpr.GetFieldName());
	return m_ThenExpr.GetFieldName();
}

//-----------------------------------------------------------------------------
RuleReturn CondRule::Apply (const Array* arOrderOnThisColumns /*= NULL*/, CString* pstrAuxWhereClause /*= NULL*/)
{
	this->m_pBefore->Exec();	//for RS-DEBUGGER only

	WoormField* pRI = m_ThenExpr.GetField();

	DataBool cond;

	if (!m_TestExpr.Eval(cond))
	{
		if (m_TestExpr.GetErrId() != Expression::NULL_OPR)
		{
			m_OwnerEngine.SetRTError(
				EngineScheduler::QUERY_COND_RULE_EVAL_TEST_ERR, 
				m_TestExpr.GetErrDescription(),
				pRI->GetName()
				);
			return RULE_ABORT;
		}
	}
	else
	{
		if ((int) cond)
		{
			if (!m_ThenExpr.IsEmpty() && !m_ThenExpr.Eval(*(pRI->GetRuleData())))
			
				if (m_ThenExpr.GetErrId() != Expression::NULL_OPR)
				{
					m_OwnerEngine.SetRTError(EngineScheduler::QUERY_COND_RULE_EVAL_THEN_ERR, m_ThenExpr.GetErrDescription(),
											pRI->GetName());
					return RULE_ABORT;
				}
				else	// necessary if the field is operand in the where-clause expression
					pRI->RuleNullified();
		}
		else
		{
			if (!m_ElseExpr.IsEmpty() && !m_ElseExpr.Eval(*(pRI->GetRuleData())))
				if (m_ElseExpr.GetErrId() != Expression::NULL_OPR)
				{
					m_OwnerEngine.SetRTError(EngineScheduler::QUERY_COND_RULE_EVAL_ELSE_ERR, m_ElseExpr.GetErrDescription(),
											pRI->GetName());
					return RULE_ABORT;
				}
				else	// necessary if the field is operand in the where-clause expression
					pRI->RuleNullified();
		}

		if (!m_ValidateWhereExpr.IsEmpty())
		{
			DataBool goodVal(FALSE);

			if (!m_ValidateWhereExpr.Eval(goodVal) &&
				(m_ValidateWhereExpr.GetErrId() != Expression::NULL_OPR))
			{
				m_OwnerEngine.SetRTError(EngineScheduler::QUERY_COND_RULE_EVAL_FILT_ERR, m_ValidateWhereExpr.GetErrDescription(),
										pRI->GetName());
				return RULE_ABORT;
			}

			if (!((int) goodVal))
			{
				m_bFailed = TRUE;
				m_OwnerEngine.SetRTError(EngineScheduler::QUERY_COND_RULE_BACKTRACK, _T(""), pRI->GetName());

				return RULE_BACKTRACK;
			}
		}
	}

	m_bFailed = FALSE;

	if (!m_TestExpr.GetErrId() && ((int) cond ? !m_ThenExpr.GetErrId() : !m_ElseExpr.GetErrId()))
	{
		if (m_bNullSolution)
		{
			m_bNullSolution = FALSE;
			m_OwnerEngine.m_nNullRulesNum--;
		}

		pRI->RuleUpdated();

		this->m_pAfter->Exec(this);	//for RS-DEBUGGER only
	}
	else
	{
		if (!m_bNullSolution)
		{
			m_bNullSolution = TRUE;
			m_OwnerEngine.m_nNullRulesNum++;
		}

		pRI->RuleNullified();
	}

	return m_OwnerEngine.ApplyFrom(m_nRuleId + 1, arOrderOnThisColumns, pstrAuxWhereClause);
}

//==============================================================================
//
//			Class For-While Rule implementation
//
//==============================================================================

IMPLEMENT_DYNAMIC(WhileRule, RuleObj)
//-----------------------------------------------------------------------------
WhileRule::WhileRule (RuleEngine& aOwnerEngine)
	:
	RuleObj			(aOwnerEngine),
	m_CondExpr		(&(aOwnerEngine.GetSymTable())),
	m_pBody			(new Block(NULL, NULL, dynamic_cast<RepEngine*>(&aOwnerEngine), FALSE))
{
	m_pBody->m_strOwnerName = L"Rule." + GetDescription() + L".Body";

	m_pBefore->SetRuleScope();
	m_pBody->SetRuleScope();
	m_pAfter->SetRuleScope();

	m_pBefore->SetForceBeginEnd();
	m_pBody->SetForceBeginEnd();
	m_pAfter->SetForceBeginEnd();
}

//-----------------------------------------------------------------------------
RuleType WhileRule::IsARule () const
{
	return RULE_LOOP_WHILE;
}

//---------------------------------------------------------------------------
BOOL WhileRule::Parse(Parser& parser)
{
	m_arWSelFieldName.RemoveAll();
	CStringArray* pOld = this->m_OwnerEngine.GetSymTable().TraceFieldsModified(&this->m_arWSelFieldName);

	if (parser.Matched(T_FOR))
	{
		if (!m_pBefore->Parse(parser))
		{
			this->m_OwnerEngine.GetSymTable().TraceFieldsModified(pOld);
			return FALSE;
		}

		m_pBefore->m_strOwnerName = L"Rule." + GetDescription() + L".Init";
	}
		
	if (parser.Match(T_WHILE))
	{
		m_CondExpr.SetStopTokens(T_DO);
		if (!m_CondExpr.Parse(parser, DATA_BOOL_TYPE))
		{
			this->m_OwnerEngine.GetSymTable().TraceFieldsModified(pOld);
			return FALSE;
		}
	}

	if (parser.Match(T_DO))
	{
		if (!m_pBody->Parse(parser))
		{
			this->m_OwnerEngine.GetSymTable().TraceFieldsModified(pOld);
			return FALSE;
		}
	}

	if (parser.Matched(T_CONTINUE))
	{
		if (!m_pAfter->Parse(parser))
		{
			this->m_OwnerEngine.GetSymTable().TraceFieldsModified(pOld);
			return FALSE;
		}
		m_pAfter->m_strOwnerName = L"Rule." + GetDescription() + L".Repeat";
	}

	parser.Matched(T_SEP);

	if (pOld && m_arWSelFieldName.GetSize())
		pOld->Append(m_arWSelFieldName);

	this->m_OwnerEngine.GetSymTable().TraceFieldsModified(pOld);

	SetRuleFields(TRUE);

	return !parser.ErrorFound();
}

//----------------------------------------------------------------------------
void WhileRule::SetRuleFields(BOOL setRuleField)
{
	for (int i = 0; i < m_arWSelFieldName.GetSize(); i++)
	{
		WoormField* pF = m_OwnerEngine.GetSymTable().GetField(m_arWSelFieldName.GetAt(i));
		if (pF)
		{
			pF->SetTableRuleField(setRuleField);
			pF->SetOwnerRule(this);
		}
	}
}

//-----------------------------------------------------------------------------
WoormField* WhileRule::GetSelField(int i) const
{
	return m_OwnerEngine.GetSymTable().GetField(m_arWSelFieldName.GetAt(i));
}

//-----------------------------------------------------------------------------
BOOL WhileRule::FindKeyName (LPCTSTR pszFieldName)
{
	if (!m_CondExpr.IsEmpty() && m_CondExpr.HasMember(pszFieldName))
		return TRUE;
	if (m_pBody && m_pBody->HasMember(pszFieldName))
		return TRUE;
	if (m_pBefore && m_pBefore->HasMember(pszFieldName))
		return TRUE;
	if (m_pAfter && m_pAfter->HasMember(pszFieldName))
		return TRUE;

	return FALSE;
}

//-----------------------------------------------------------------------------
CString WhileRule::GetDescription() const
{
	//return L"For-While-Do ...";
	return L"Repeat";
}

//-----------------------------------------------------------------------------
RuleReturn WhileRule::Apply (const Array* arOrderOnThisColumns /*= NULL*/, CString* pstrAuxWhereClause /*= NULL*/)
{
	//Before/After are used as Init/Iterator blocks, Body is the rule
	BOOL bFirst = TRUE;
	m_bFailed = FALSE;

	m_pBefore->Exec();	//Init block

	for(;;) 
	{
		DataBool cond;
		if (!m_CondExpr.Eval(cond))
		{
			m_bFailed = TRUE;
			m_OwnerEngine.SetRTError(EngineScheduler::QUERY_COND_RULE_EVAL_TEST_ERR, m_CondExpr.GetErrDescription());
			return RULE_ABORT;
		}

		if (!cond) 
			break;	//esce dalla rule

		if (bFirst)
		{
			bFirst = FALSE;
			m_bNullSolution = FALSE;
			m_OwnerEngine.m_nNullRulesNum--;
		}

		m_pBody->Exec();		

		//itera sulle rules nidificate
		RuleReturn rr = m_OwnerEngine.ApplyFrom(m_nRuleId + 1, arOrderOnThisColumns, pstrAuxWhereClause);
		switch (rr)
		{
			case RULE_ABORT:
				return RULE_ABORT;

			case RULE_BACKTRACK:
			{
				if (!IsParentOfFailedRules())
					return RULE_BACKTRACK;
				break;
			}
		}

		m_pAfter->Exec();		//Iterator block
	}

	if (bFirst)
	{
		m_bNullSolution = TRUE;
		m_OwnerEngine.m_nNullRulesNum++;

		// make null all own fields
		for (int i = 0; i < GetSelFieldsNum(); i++)
			GetSelField(i)->RuleNullified();

		return m_OwnerEngine.ApplyFrom(m_nRuleId + 1, arOrderOnThisColumns, pstrAuxWhereClause);
	}

	return RULE_SUCCESS;
}

//==============================================================================
//
//			Class QueryRule implementation
//
//==============================================================================
IMPLEMENT_DYNAMIC(QueryRule, RuleObj)

//-----------------------------------------------------------------------------
QueryRule::QueryRule (RuleEngine& aOwnerEngine, CTBContext* pTBContext)
	:
	RuleObj		(aOwnerEngine),
	m_WhenExpr	(&(aOwnerEngine.GetSymTable())),
	m_pTBContext(pTBContext),
	m_pQuery	(NULL) //	(_T(""), &(aOwnerEngine.GetSymTable()), pTBContext->GetReadOnlySqlSession()) 
{
}

//-----------------------------------------------------------------------------
RuleType QueryRule::IsARule () const
{
	return RULE_NAMED_QUERY;
}

//---------------------------------------------------------------------------
BOOL QueryRule::Parse(Parser& parser)
{
	if (parser.ParseTag(T_QUERY))
	{
		parser.ParseID(m_strQueryName);

		//TROPPO presto, lo fa dopo, prima della Buildtree 
		//this->m_pQuery = dynamic_cast<QueryObject*>(this->m_OwnerEngine.GetSymTable().FindQuery(m_strQueryName));

		m_ConstraintMode = RuleSelectionMode::SEL_ALL;
		if (parser.Matched(T_NOT) && parser.ParseTag(T_NULL))
			m_ConstraintMode = RuleSelectionMode::SEL_NOT_NULL;
		else
			if (!parser.ErrorFound() && parser.Matched(T_NULL))
				m_ConstraintMode = RuleSelectionMode::SEL_NULL;
			else
				if (parser.ErrorFound()) 
					return FALSE;

		if (parser.Matched(T_WHEN))
		{
			m_WhenExpr.SetStopTokens(T_SEP);
			if (!m_WhenExpr.Parse(parser, DATA_BOOL_TYPE))
				return FALSE;
		}
		parser.Match(T_SEP);
	}

	m_pBefore->m_strOwnerName = L"Rule." + GetDescription() + L".Before";
	m_pAfter->m_strOwnerName = L"Rule." + GetDescription() + L".After";

	return !parser.ErrorFound();
}

//-----------------------------------------------------------------------------
BOOL QueryRule::FindKeyName (LPCTSTR pszFieldName)
{
	if (m_WhenExpr.HasMember(pszFieldName)) 
		return TRUE;
	if (m_pQuery && m_pQuery->HasMember(pszFieldName)) 
		return TRUE;

	return FALSE;
}

//-----------------------------------------------------------------------------
CString QueryRule::GetDescription() const
{
	return cwsprintf(L"Query %s", m_strQueryName);
}
//-----------------------------------------------------------------------------

RuleReturn QueryRule::ExecQuery()
{
	if (m_OwnerEngine.m_nNullRulesNum != 0 && IsSonOfNullRules())
		return RULE_BACKTRACK;

	//-----
	QueryObjItem* pQOM = m_OwnerEngine.GetSymTable().FindQuery(m_strQueryName);
	if (pQOM == NULL || pQOM->m_Query.GetSql().IsEmpty())
	{
		m_OwnerEngine.SetRTError(EngineScheduler::QUERY_NOT_YET_IMPLEMENTED, m_strQueryName);
		return RULE_ABORT;
	}
	m_pQuery = &(pQOM->m_Query);
	m_pQuery->SetQueryRule();

	//----
	this->m_pBefore->Exec();	//for RS-DEBUGGER only

	TRY
	{
		m_OwnerEngine.m_Scheduler.ExecuteSQLCommandRetBool(m_pQuery, (BOOL_FUNC) &QueryObject::Read);
	}
	CATCH(SqlException, e)	
	{
		CString strErr (L"Query rule " +  m_strQueryName + _T("\n") + e->m_strError);
		m_OwnerEngine.SetRTError(EngineScheduler::QUERY_DATTBL_RULE_READ_ERR, 0, strErr);
		return RULE_ABORT;
	}
	END_CATCH	
	
	this->m_pAfter->Exec(this);		//for RS-DEBUGGER only
	
	return RULE_SUCCESS;
}

//-----------------------------------------------------------------------------
RuleReturn QueryRule::ExecSubQuery(BOOL& bRecFoundForConstraint, const Array* arOrderOnThisColumns /*= NULL*/, CString* pstrAuxWhereClause /*= NULL*/)
{
	if (m_pQuery->IsEmpty())
		return RULE_SUCCESS;

	RuleReturn rr = RULE_SUCCESS;

	TRY {
		while (!m_pQuery->IsEof())
		{
			m_OwnerEngine.m_Scheduler.PostMessage(WM_COMMAND, ID_SEARCH_TIC);

			if (m_ConstraintMode == SEL_NULL)
			{
				bRecFoundForConstraint = TRUE;
				break;
			}

			m_bFailed = FALSE;

			if (m_bNullSolution)
			{
				m_bNullSolution = FALSE;
				m_OwnerEngine.m_nNullRulesNum--;
			}

			// validate all own fields
			ASSERT(GetSelFieldsNum() > 0);
			for (int i = 0; i < GetSelFieldsNum(); i++)
				GetSelField(i)->RuleUpdated();

			// this statement musts be in this place after fields updating
			rr = m_OwnerEngine.ApplyFrom(m_nRuleId + 1, arOrderOnThisColumns, pstrAuxWhereClause);

			switch (rr)
			{
				case RULE_ABORT:
					return RULE_ABORT;

				case RULE_BACKTRACK:
				{
					if (!IsParentOfFailedRules())
						return RULE_BACKTRACK;
					break;
				}

				case RULE_SUCCESS:
				{
					bRecFoundForConstraint = TRUE;
					break;
				}
			}

			m_pQuery->Read();	// -> MoveNext

			this->m_pAfter->Exec(this);	//for RS-DEBUGGER only
		}
	}
	CATCH(SqlException, e)
	{
		CString strErr (L"Query rule " + this->m_strQueryName + _T("\n") + e->m_strError);
		m_OwnerEngine.SetRTError(EngineScheduler::QUERY_DATTBL_RULE_READ_ERR, 0, strErr);
		return RULE_ABORT;
	}
	END_CATCH

	return RULE_SUCCESS;
}

//-----------------------------------------------------------------------------
RuleReturn QueryRule::Apply (const Array* arOrderOnThisColumns /*= NULL*/, CString* pstrAuxWhereClause /*= NULL*/)
{
	if (!m_WhenExpr.IsEmpty())
	{
		DataBool cond(TRUE);
		if (!m_WhenExpr.IsEmpty() && !m_WhenExpr.Eval(cond))
		{
			m_OwnerEngine.SetRTError(EngineScheduler::QUERY_COND_RULE_EVAL_TEST_ERR, m_WhenExpr.GetErrDescription());
			return RULE_ABORT;
		}

		if (!cond)
		{
			m_OwnerEngine.m_nNullRulesNum--;
			return m_OwnerEngine.ApplyFrom(m_nRuleId + 1, arOrderOnThisColumns, pstrAuxWhereClause);
		}
	}
	//----

	RuleReturn rr = ExecQuery();
	
	BOOL bRecFoundForConstraint = FALSE;
	switch (rr)
	{
		case RULE_ABORT		:
			 return RULE_ABORT;

		case RULE_SUCCESS	:
		{
			RuleReturn rr = ExecSubQuery(bRecFoundForConstraint, arOrderOnThisColumns, pstrAuxWhereClause);

			if (rr != RULE_SUCCESS)
				return rr;

			if (bRecFoundForConstraint && (m_ConstraintMode != SEL_NULL))
			{
				m_bFailed = FALSE;
				return RULE_SUCCESS;
			}
			
			break;
		}
	}

	if (
			(!bRecFoundForConstraint && (m_ConstraintMode == SEL_NOT_NULL)) ||
			(bRecFoundForConstraint && (m_ConstraintMode == SEL_NULL))
		)
	{
		m_bFailed = TRUE;
		m_OwnerEngine.SetRTError(EngineScheduler::QUERY_DATTBL_RULE_BACKTRACK, 0, this->m_strQueryName);

		return RULE_BACKTRACK;
	}

	m_bFailed = FALSE;

	if (!m_bNullSolution)
	{
		m_bNullSolution = TRUE;
		m_OwnerEngine.m_nNullRulesNum++;
	}

	// make null all own fields
	for (int i = 0; i < GetSelFieldsNum(); i++)
		GetSelField(i)->RuleNullified();

	return m_OwnerEngine.ApplyFrom(m_nRuleId + 1, arOrderOnThisColumns, pstrAuxWhereClause);
}

//-----------------------------------------------------------------------------
WoormField* QueryRule::GetSelField(int i) const
{
	ASSERT_VALID(m_pQuery);
	if (!m_pQuery) return NULL;
	ASSERT(i < GetSelFieldsNum());

	CString sName = m_pQuery->GetSelFieldName(i);
	WoormField* pF = this->m_OwnerEngine.GetSymTable().GetField(sName);
	ASSERT_VALID(pF);
	return pF;
}

//-----------------------------------------------------------------------------
CString QueryRule::GetSelFieldName(int i) const
{
	ASSERT_VALID(m_pQuery);
	if (!m_pQuery) return L"";
	ASSERT(i < GetSelFieldsNum());

	return m_pQuery->GetSelFieldName(i);
}

//-----------------------------------------------------------------------------
int QueryRule::GetSelFieldsNum() const
{
	ASSERT_VALID(m_pQuery);
	if (!m_pQuery) return 0;

	return m_pQuery->GetSelFieldsNum();
}

//----------------------------------------------------------------------------
void QueryRule::SetRuleFields(BOOL setRuleField)
{
	ASSERT_VALID(m_pQuery);

	if (m_pQuery)
	{
		for (int i = 0; i < m_pQuery->AllQueryColumns().GetSize(); i++)
		{
			WoormField* pF = m_OwnerEngine.GetSymTable().GetField(m_pQuery->AllQueryColumns().GetAt(i));
			if (pF)
			{
				pF->SetTableRuleField(setRuleField);
				pF->SetOwnerRule(this);
			}
		}
	}
}

//============================================================================
//
//			Class SelectionMap
//
//============================================================================
class SelectionMap : public CObject
{
	DECLARE_DYNAMIC(SelectionMap) 

	friend DataTableRule;
public:
	SqlTableJoinInfoArray	m_arSqlTableJoinInfoArray;

protected:
	RecordArray				m_arBindRecords;
	CArray<WoormField*>		m_SelFields;

public:
	SelectionMap () {}
	SelectionMap (const CString& strTableName);
	//virtual ~SelectionMap() {}

	int Add (const CString& strTableName);

	int Add(SqlRecord* pRec) { return m_arBindRecords.Add(pRec); }
	SqlRecord* GetAt(int idx) { return m_arBindRecords.GetAt(idx); }
	int GetSize() const { return m_arBindRecords.GetSize(); }

	CString	BindRuleItem	(WoormField*);

	void	ResetDataItem(WoormField* pr, int i);
	CString	GetTableNames();

};

//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(SelectionMap, CObject) 
			
//-----------------------------------------------------------------------------
SelectionMap::SelectionMap (const CString& strTableName)
{
	Add(strTableName);
}

//-----------------------------------------------------------------------------
int  SelectionMap::Add (const CString& strTableName)
{
	//chiama il costruttore di SqlRecord con il parametro m_bSkipDynamicDeclarations  a true,
	//in modo che il record non crei gia i sqlrecorditem dei campi aggiunti dinamicamente da EasyBuilder,
	//perche vengono poi creati da woorm in fase di parsing (BindRuleItem)  An.18780
	return Add (new SqlRecord(strTableName, NULL, TABLE_TYPE, true));
}

//-----------------------------------------------------------------------------
CString SelectionMap::BindRuleItem(WoormField* pRuleField)
{
	CString sErr = pRuleField->BindRuleItem(m_arBindRecords, m_arSqlTableJoinInfoArray);
	if (!sErr.IsEmpty())
		return sErr;

	m_SelFields.Add(pRuleField);
	return _T("");
}

//-----------------------------------------------------------------------------
void SelectionMap::ResetDataItem(WoormField* pri, int i)
{
	//TODO
	SqlRecordItem* pSqlRecordItem =	GetAt(0)->GetAt(i);
	pSqlRecordItem->SetDataObj(pri->GetRuleData());
}

//-----------------------------------------------------------------------------
CString	SelectionMap::GetTableNames()
{
	CString strNames = GetAt(0)->GetTableName();
	for(int i=1; i < GetSize(); i++)
		strNames += _T(",") + GetAt(i)->GetTableName();
	return strNames;
}

//==============================================================================
//
//			Class DataTableRule implementation
//
//==============================================================================

IMPLEMENT_DYNAMIC(DataTableRule, RuleObj)
//-----------------------------------------------------------------------------
DataTableRule::DataTableRule (RuleEngine& aOwnerEngine, CTBContext* pTBContext/*, BOOL bTemporary*/)
	:
	RuleObj			(aOwnerEngine),
	m_pTBContext	(pTBContext),
	//m_bTmpRule		(/*bTemporary*/FALSE),
	m_nDataLevel	(/*bTemporary ? QUERY_ENGINE :*/ RULE_ENGINE),
	m_ConstraintMode(SEL_ALL),
	m_pDataTable	(NULL),
	m_pSelectionMap	(NULL),
	m_pWhereClause	(NULL),
	m_pHavingClause (NULL),
	m_bDistinct		(FALSE),
	m_nTop			(0),
	m_WhenExpr		(&(aOwnerEngine.GetSymTable()))
{
}

//-----------------------------------------------------------------------------
DataTableRule::~DataTableRule ()
{
	CloseDataTable();
}

//-----------------------------------------------------------------------------
RuleType DataTableRule::IsARule () const
{
	return RULE_DATA_TABLE;
}

// Initial mapping for temporary rule is for DataObjs at QUERY_ENGINE data level
// (excluding FieldStatusForTemporaryFile defined only at RULE_ENGINE datalevel)
//-----------------------------------------------------------------------------
//void DataTableRule::AddSelFieldTmp (WoormField* pRuleItem, int nDataLevel)
//{   
//	ASSERT(pRuleItem);
//	ASSERT(m_bTmpRule);
//
//	//@@TODO prendere  il column info da sqlrecorditem
//	//int nColumnPos = m_pSelectionMap->m_SelFields.Add(pRuleItem);
//	//m_pSelectionMap->Add(new SqlRecordItem(pRuleItem->GetData(nDataLevel), pColumnInfo));
//}

//-----------------------------------------------------------------------------
//void DataTableRule::ResetDataMapping ()
//{
//	ASSERT(m_pSelectionMap);
//	ASSERT(m_bTmpRule);
//	ASSERT(m_nDataLevel == QUERY_ENGINE);
//	
//	m_nDataLevel = RULE_ENGINE;
//	
//	int nf = GetSelFieldsNum() - 1;
//
//	for (int i = 0; i < nf; i++)
//	{
//		m_pSelectionMap->ResetDataItem(GetSelField(i), i);
//	}
//}

//-----------------------------------------------------------------------------
void DataTableRule::CloseDataTable ()
{
	ASSERT_VALID(this);

	if (!m_pDataTable)
		return;
	ASSERT_VALID(m_pDataTable);

	if (m_pDataTable->IsOpen())
		m_pDataTable->Close();
			
	SAFE_DELETE(m_pDataTable);
	SAFE_DELETE(m_pSelectionMap);
	SAFE_DELETE(m_pWhereClause);
	SAFE_DELETE(m_pHavingClause);
}

//----------------------------------------------------------------------------
/*	Grammatica:
	From-Rule-Column ::= [ \{ <native-sql-expr> \} | <column-name> ] INTO <woorm-field 
	From-Rule ::= FROM [ <table-name> { , <table-name> } |
		<table-name> [Inner Join | Left Outer Join  | Right Outer Join] <table-name> [ ON [NATIVE <native-sql-expr> | <wc-expr>]]
		<table-name> [Inner Join | Left Outer Join  | Right Outer Join] <table-name> [ ON [NATIVE <native-sql-expr> | <wc-expr>]]
		SELECT [NULL | NOT NULL]] [DISTINCT] [TOP n]
		<From-Rule-Column> { , <From-Rule-Column> }
		[WHERE [NATIVE <native-expr> | WHERE <expr>]]
		[GROUP BY <native-groupby>]
		[HAVING [NATIVE <native-expr | HAVING <expr>]]
		[ORDER BY <native-orderby> ] 
		[WHEN <woorm-bool-expr> ] ;
*/
BOOL DataTableRule::Parse(Parser& parser)
{
	CString strPublicName, strPhysicalName, strIndexName;
	m_pDataTable = NULL;
	m_ExtraFilter.Empty();

	m_pSelectionMap = new SelectionMap();

	if (!parser.ParseTag(T_FROM))
		return FALSE;

	BOOL bNatural = TRUE;	//natural join: From t1, t2, t3
	BOOL bContinue = TRUE;
	do
	{
		CString	strDataTableName, strAliasTableName;

		SqlTableJoinInfoArray::EJoinType eJoinType = SqlTableJoinInfoArray::EJoinType::CROSS;

		if (m_pSelectionMap->GetSize() > 0 && !bNatural)
		{
			if (parser.Matched(T_INNER) && parser.Match(T_JOIN))
			{
				eJoinType = SqlTableJoinInfoArray::EJoinType::INNER;
			}
			else if (parser.Matched(T_CROSS) && parser.Match(T_JOIN))
			{
				eJoinType = SqlTableJoinInfoArray::EJoinType::CROSS;
			}
			else if (parser.Matched(T_LEFT) && parser.Match(T_OUTER) && parser.Match(T_JOIN))
			{
				eJoinType = SqlTableJoinInfoArray::EJoinType::LEFT_OUTER;
			}
			else if (parser.Matched(T_RIGHT) && parser.Match(T_OUTER) && parser.Match(T_JOIN))
			{
				eJoinType = SqlTableJoinInfoArray::EJoinType::LEFT_OUTER;
			}
			else if (parser.Matched(T_FULL) && parser.Match(T_OUTER) && parser.Match(T_JOIN))
			{
				eJoinType = SqlTableJoinInfoArray::EJoinType::FULL_OUTER;
			}
			if (parser.ErrorFound())
				return FALSE;
		}
		//----------------

		if (!parser.ParseSquaredIdent(strDataTableName))
			return FALSE;
				
		if (!m_pTBContext->GetSqlConnection()->ExistTable(strDataTableName))		
		{
			parser.SetError(_TB("DataBase table unknown") + _T(" (") + strDataTableName + ')');
			return FALSE;
		}

		const SqlCatalogEntry* pCatalogEntry = m_pTBContext->GetSqlConnection()->GetCatalogEntry(strDataTableName);		
		ASSERT_VALID(pCatalogEntry);
		if (pCatalogEntry)
		{
			SqlRecord* pRec = pCatalogEntry->CreateRecord();
			ASSERT_VALID(pRec);
			if (pRec)
			{
				pRec->EnableExtraFiltering(TRUE);
				pRec->OnExtraFiltering(m_ExtraFilter);
				m_arContextBagRecords.Add(pRec);
			}
		}

		if (parser.Matched(T_ALIAS) && !parser.ParseID(strAliasTableName))
			return FALSE;

		int pos = m_pSelectionMap->Add(strDataTableName);

		m_pSelectionMap->m_arSqlTableJoinInfoArray.Add(m_pTBContext->GetSqlConnection(), &m_OwnerEngine.GetSymTable(), m_pSelectionMap->GetAt(pos)->GetTableInfo(), strAliasTableName, eJoinType, NULL);

		ASSERT(m_pSelectionMap->GetSize());
		if (!m_pDataTable)
			m_pDataTable = new SqlTable(m_pSelectionMap->GetAt(0), m_pTBContext->GetReadOnlySqlSession());

		//----------------- 
		if (m_pSelectionMap->GetSize() > 1)
		{
			WClause* pJoinOn = m_pSelectionMap->m_arSqlTableJoinInfoArray.m_arJoinOn[pos];
			pJoinOn->SetSqlTable(m_pDataTable);

			if (eJoinType != SqlTableJoinInfoArray::EJoinType::CROSS)
			{
				if (!parser.Match(T_ON) || !pJoinOn->Parse(parser))
					return FALSE;
			}
		}

		//----------------- 
		bContinue = FALSE;
		if (bNatural && parser.Matched(T_COMMA))
		{
			bContinue = TRUE;
		}
		else
		{
			if (bNatural && m_pSelectionMap->GetSize() > 1)
				break;

			bNatural = FALSE;
			bContinue = parser.LookAhead(T_INNER) || parser.LookAhead(T_LEFT) || parser.LookAhead(T_RIGHT) || parser.LookAhead(T_FULL) || parser.LookAhead(T_CROSS);
		}

	} while (bContinue);

	if (!parser.ParseTag(T_SELECT))
		return FALSE;

	m_ConstraintMode = RuleSelectionMode::SEL_ALL;
	if (parser.Matched(T_NOT) && parser.ParseTag(T_NULL))
		m_ConstraintMode = RuleSelectionMode::SEL_NOT_NULL;
	else
		if (!parser.ErrorFound() && parser.Matched(T_NULL))
			m_ConstraintMode = RuleSelectionMode::SEL_NULL;
		else
			if (parser.ErrorFound()) return FALSE;

	m_bDistinct = parser.Matched(T_DISTINCT);
	if (parser.Matched(T_TOP))
	{
		if (!parser.ParseInt(m_nTop))
			return FALSE;
	}

	CStringArray m_arForbiddenPublicIdents;
	do
	{
		BOOL bNativeColumnExpr = FALSE;
		if (parser.LookAhead(T_BRACEOPEN))
		{
			if (!parser.ParseBracedContent(strPhysicalName))
				return FALSE;

			CString err;
			if (!::ParseCalculateColumn(strPhysicalName, &m_OwnerEngine.GetSymTable(), err))
				return parser.SetError(err);

			bNativeColumnExpr = TRUE;
		}
		else if (!parser.ParseSquaredCoupleIdent(strPhysicalName))
				return FALSE;

		if (!parser.ParseTag(T_INTO))          return FALSE;
		if (!parser.ParseID(strPublicName))    return FALSE;

		WoormField* pRI = (WoormField*) (m_OwnerEngine.GetSymTable().GetField(strPublicName));
		if (pRI == NULL)
			return parser.SetError(Expression::FormatMessage(Expression::UNKNOWN_FIELD), strPublicName);

		if (pRI->IsReadOnly())
			return parser.SetError(_TB("Read-only field"), strPublicName);
			
		if (pRI->GetOwnerRule())
			parser.SetError(_TB("Field already used by another rule"), strPublicName);

		pRI->SetPhysicalName(strPhysicalName);
		pRI->SetOwnerRule(this);

		if (bNativeColumnExpr)
			pRI->SetNativeColumnExpr(TRUE);

		CString strErr = m_pSelectionMap->BindRuleItem(pRI);
		if (!strErr.IsEmpty())
			return parser.SetError(strErr, strPhysicalName);

		m_arForbiddenPublicIdents.Add(strPublicName);
	}
	while (parser.Matched(T_COMMA));

	if (parser.ErrorFound())	
		return FALSE;

	int i = 0;
	if (m_pSelectionMap->m_arSqlTableJoinInfoArray.m_arstrAliasTableName.GetSize())
		for (i = 0; i < m_pSelectionMap->GetSize(); i++)
		{
			CString strAlias = m_pSelectionMap->m_arSqlTableJoinInfoArray.m_arstrAliasTableName.GetAt(i);
			m_pSelectionMap->GetAt(i)->SetQualifier(strAlias);
			m_pDataTable->FromTable(m_pSelectionMap->GetAt(i), strAlias.IsEmpty() ? NULL : &strAlias);
		}

	if (parser.Matched(T_WHERE))
	{
		m_pWhereClause = new WClause(m_pTBContext->GetSqlConnection(), &m_OwnerEngine.GetSymTable(), &m_pSelectionMap->m_arSqlTableJoinInfoArray, m_pDataTable);
		
		//ROLLBACK: funziona ma non piace - m_pWhereClause->SetForbiddenPublicIdents(m_arForbiddenPublicIdents);
		
		if (!m_pWhereClause->Parse(parser))
			return FALSE;
	}
	if (parser.ErrorFound())	
		return FALSE;

	if 	(parser.Matched(T_GROUP))
	{
		if	(
				!parser.ParseTag(T_BY)	
				||
				!ParseOrderBy(parser, &m_OwnerEngine.GetSymTable(), &m_pSelectionMap->m_arSqlTableJoinInfoArray, 
								m_pDataTable->m_strGroupBy, TRUE, NULL, &m_pSelectionMap->m_arSqlTableJoinInfoArray.m_arstrAliasTableName)
				||
				parser.ErrorFound()
			)
			return FALSE;
		
		CString s = m_pDataTable->m_strGroupBy; s.MakeLower();
		CString f = AfxGetTokensTable()->ToString(T_FCONTENTOF); f.MakeLower();
		if (s.Find(f) >= 0)
		{
			m_strGroupByDynamic = m_pDataTable->m_strGroupBy;
		}
	}

	if (parser.Matched(T_HAVING))
	{
		m_pHavingClause = new WClause(m_pTBContext->GetSqlConnection(), &m_OwnerEngine.GetSymTable(), &m_pSelectionMap->m_arSqlTableJoinInfoArray, m_pDataTable);
		m_pHavingClause->SetHavingClause();
			
		//Il controllo funziona ma non piace - m_pHavingClause->SetForbiddenPublicIdents(m_arForbiddenPublicIdents);	

		//TODO: si potrebbe aggiungere un altro controllo che le colonne siano quelle in group by

		if (!m_pHavingClause->Parse(parser))
			return FALSE;
	}
 
	if 	(parser.Matched(T_ORDER))
	{
		if	(
				!parser.ParseTag(T_BY)
				||
				!ParseOrderBy(parser, &m_OwnerEngine.GetSymTable(), &m_pSelectionMap->m_arSqlTableJoinInfoArray, 
								m_pDataTable->m_strSort, TRUE, NULL, &(m_pSelectionMap->m_arSqlTableJoinInfoArray.m_arstrAliasTableName))
				||
				parser.ErrorFound()
			)
			return FALSE;

		CString s = m_pDataTable->m_strSort; s.MakeLower();
		CString f = AfxGetTokensTable()->ToString(T_FCONTENTOF); f.MakeLower();
		if (s.Find(f) >= 0)
		{
			m_strOrderByDynamic = m_pDataTable->m_strSort;
		}
	}

	if (parser.Matched(T_WHEN))
	{
		m_WhenExpr.SetStopTokens(T_SEP);
		if (!m_WhenExpr.Parse(parser, DATA_BOOL_TYPE))
			return FALSE;
	}

	m_pBefore->m_strOwnerName = L"Rule." + GetDescription() + L".Before";
	m_pAfter->m_strOwnerName = L"Rule." + GetDescription() + L".After";

	return parser.ParseSep() && !parser.ErrorFound();
}

//----------------------------------------------------------------------------
int DataTableRule::GetTotFieldLinks() const
{
	int tot = m_pSelectionMap->m_SelFields.GetSize();
	return tot;
}

//----------------------------------------------------------------------------
void DataTableRule::Unparse(Unparser& oFile)
{
	if (!oFile.IsMemoryFile())
	{
		if (this->GetTotFieldLinks() == 0)
		{
			return;
		}
	}

	oFile.UnparseTag(T_FROM, FALSE);

	ASSERT_VALID(m_pSelectionMap);
	this->m_pSelectionMap->m_arSqlTableJoinInfoArray.Unparse(oFile);

	oFile.UnparseTag	(T_SELECT, FALSE);

	switch (m_ConstraintMode)
	{
		case SEL_ALL :
			break;

		case SEL_NOT_NULL :
			oFile.UnparseBlank  ();
			oFile.UnparseTag	(T_NOT,	 FALSE);
			oFile.UnparseTag    (T_NULL, FALSE);
			break;

        case SEL_NULL :
			oFile.UnparseBlank  ();
			oFile.UnparseTag    (T_NULL, FALSE);
			break;
    }

	if (m_bDistinct)
	{
		oFile.UnparseBlank  ();
		oFile.UnparseTag (T_DISTINCT,	 FALSE);
	}
	if (m_nTop)
	{
		oFile.UnparseBlank  ();
		oFile.UnparseTag (T_TOP,	 FALSE);
		oFile.UnparseInt (m_nTop,	 FALSE);
	}

	oFile.UnparseCrLf();
    oFile.IncTab();
    
	for (int j = 0; j < this->m_pSelectionMap->m_SelFields.GetSize(); j++)
	{
		WoormField* pF  = m_pSelectionMap->m_SelFields.GetAt(j);
		ASSERT_VALID(pF);

		if (j > 0)
			oFile.UnparseComma(TRUE);

		oFile.Write(cwsprintf(_T("%-20s"), pF->GetPhysicalName()), FALSE);

		oFile.UnparseBlank();
		oFile.UnparseTag(T_INTO, FALSE);
		oFile.UnparseID(pF->GetName(), FALSE);
	}

	if (m_pWhereClause && !m_pWhereClause->IsEmpty())
    {
        oFile.UnparseCrLf();
	    oFile.DecTab();
        oFile.UnparseTag    (T_WHERE,		FALSE);
		oFile.IncTab();

         oFile.UnparseExpr   (m_pWhereClause->ToString(),	FALSE);
    }
	oFile.DecTab();
    
	if (!m_strGroupByDynamic.IsEmpty())
    {
        oFile.UnparseCrLf();
		oFile.DecTab();

        oFile.UnparseTag    (T_GROUP,	FALSE);
        oFile.UnparseTag    (T_BY,		FALSE);
        oFile.UnparseExpr   (m_strGroupByDynamic,   FALSE);
    }

 	if (m_pHavingClause && !m_pHavingClause->IsEmpty())
	{
		oFile.UnparseCrLf();
		oFile.UnparseTag    (T_HAVING,		FALSE);

		oFile.UnparseExpr   (m_pHavingClause->ToString(),	FALSE);
	}

   if (!m_strOrderByDynamic.IsEmpty())
    {
        oFile.UnparseCrLf();
        oFile.UnparseTag    (T_ORDER,	FALSE);
        oFile.UnparseTag    (T_BY,		FALSE);
        oFile.UnparseExpr   (m_strOrderByDynamic,   FALSE);
    }

   	if (!m_WhenExpr.IsEmpty())
    {
		oFile.UnparseCrLf	();
		oFile.UnparseTag    (T_WHEN,      	FALSE);
		oFile.UnparseExpr   (m_WhenExpr.ToString(),	FALSE);
    }

	oFile.UnparseSep    (TRUE);
}

//-----------------------------------------------------------------------------
CString DataTableRule::ToSqlString()
{
	CString sql = GetDataTable()->ToString(TRUE, FALSE, FALSE);
	return sql;
}

//-----------------------------------------------------------------------------
BOOL DataTableRule::FindKeyName (LPCTSTR pszFieldName)
{
	//TODO FindKeyName call HasMember, ma NON verifica espressioni interne a ContentOf contenute in WClause, GroupBy/OrderBy, calculated column
	if (m_pWhereClause	&& m_pWhereClause->	HasMember(pszFieldName))
		return TRUE;
	if (m_pHavingClause	&& m_pHavingClause->HasMember(pszFieldName))
		return TRUE;
	if (!m_WhenExpr.IsEmpty() && m_WhenExpr.HasMember(pszFieldName))
		return TRUE;

	if (m_pSelectionMap)
		for (int i = 1; i < m_pSelectionMap->m_arSqlTableJoinInfoArray.m_arJoinOn.GetSize(); i++)
		{
			WClause* pJoinOn = m_pSelectionMap->m_arSqlTableJoinInfoArray.m_arJoinOn.GetAt(i);
			if (pJoinOn && pJoinOn->HasMember(pszFieldName)) 
				return TRUE;
		}

	return FALSE;
}

//-----------------------------------------------------------------------------
void DataTableRule::GetParameters(CStringArray& parameters) const
{
	parameters.RemoveAll();

	if (m_pSelectionMap)
		for (int i = 1; i < m_pSelectionMap->m_arSqlTableJoinInfoArray.m_arJoinOn.GetSize(); i++)
		{
			WClause* pJoinOn = m_pSelectionMap->m_arSqlTableJoinInfoArray.m_arJoinOn.GetAt(i);
			if (pJoinOn)
				pJoinOn->GetParameters(parameters);
		}

	if (m_pWhereClause)
		m_pWhereClause->GetParameters(parameters);
	if (m_pHavingClause)
		m_pHavingClause->GetParameters(parameters);
	//TODO da verificare se serve
	//if (!this->m_WhenExpr.IsEmpty())
	//	this->m_WhenExpr.GetParameters(parameters);
}

//-----------------------------------------------------------------------------
CString DataTableRule::GetDescription() const
{
	if (!m_pSelectionMap) 
		return L"<empty>";
	//return L"Select-From " + (m_pSelectionMap ? m_pSelectionMap->GetTableNames() : (m_pDataTable ? m_pDataTable->m_strFrom : L"")) ;
	if (m_pSelectionMap->m_arSqlTableJoinInfoArray.GetSize() == 0)
		return L"<empty>";

	CString sDescr;

	CString sTable = m_pSelectionMap->m_arSqlTableJoinInfoArray.GetAt(0)->GetTableName();
	int idx = sTable.Find('_');
	if (idx > -1 && idx < 4 && sTable.GetLength() > 7)
		sDescr = sTable.Mid(idx + 1);
	else sDescr = sTable;

	for (int i = 1; i < m_pSelectionMap->m_arSqlTableJoinInfoArray.GetSize(); i++)
	{
		sDescr += L" - ";

		sTable = m_pSelectionMap->m_arSqlTableJoinInfoArray.GetAt(i)->GetTableName();

		int idx = sTable.Find('_');
		if (idx > -1 && idx < 4 && sTable.GetLength() > 7)
			sDescr += sTable.Mid(idx + 1);
		else sDescr += sTable;
	}

	return sDescr;

}


//-----------------------------------------------------------------------------
RuleReturn DataTableRule::ExecQuery(const Array* arOrderOnThisColumns /*= NULL*/, CString* pstrAuxWhereClause /*=0*/)
{
	if (m_OwnerEngine.m_nNullRulesNum != 0 && IsSonOfNullRules())
		return RULE_BACKTRACK;

	//----
	for (int i = 0; i < m_pSelectionMap->GetSize(); i++)
	{
		const SqlCatalogEntry* pCatalogEntry = m_pTBContext->GetSqlConnection()->GetCatalogEntry(m_pSelectionMap->GetAt(i)->GetTableName());		
		ASSERT_VALID(pCatalogEntry);
		if (!pCatalogEntry) 
			continue;

		//OSL
		if (AfxGetSecurityInterface()->IsSecurityEnabled())
		{
			CInfoOSL oslInfo(*(const_cast<SqlCatalogEntry*>(pCatalogEntry)->GetInfoOSL()));
			AfxGetSecurityInterface()->GetObjectGrant(&oslInfo);
			if (!OSL_CAN_DO(&oslInfo, OSL_GRANT_EXECUTE))
			{
				CString strErr =  m_pSelectionMap->GetTableNames() + _T("\n") ;
				m_OwnerEngine.SetRTError(EngineScheduler::OSL_MISSING_GRANT, _T(""), strErr);
				return RULE_ABORT;
			}
		}
	}
	//----

	if (m_bDistinct)
		m_pDataTable->AddSelectKeyword(SqlTable::DISTINCT);

	if (m_nTop > 0 && m_pDataTable->m_pSqlConnection->GetDBMSType() != DBMS_ORACLE)
		m_pDataTable->AddSelectKeyword(SqlTable::TOP, m_nTop);

	if (!m_strGroupByDynamic.IsEmpty())
	{
		m_pDataTable->m_strGroupBy = m_strGroupByDynamic;
		if (!ExpandContentOfClause(m_pDataTable->m_strGroupBy, &(m_OwnerEngine.GetSymTable()), m_pDataTable->m_pSqlConnection))
			return RULE_ABORT;
	}
	if (!m_strOrderByDynamic.IsEmpty())
	{
		m_pDataTable->m_strSort = m_strOrderByDynamic;
		if (!ExpandContentOfClause(m_pDataTable->m_strSort, &(m_OwnerEngine.GetSymTable()), m_pDataTable->m_pSqlConnection))
			return RULE_ABORT;
	}

	m_pDataTable->Attach(&(m_OwnerEngine.GetSymTable()));

	BOOL bAppend = FALSE;
	for (int i = 1/*l'elemento 0 è sempre vuoto*/; i < m_pSelectionMap->m_arSqlTableJoinInfoArray.m_arJoinOn.GetSize(); i++)
	{
		WClause* pJoinOnClause = m_pSelectionMap->m_arSqlTableJoinInfoArray.m_arJoinOn[i];
		if (pJoinOnClause)
		{
			if (!pJoinOnClause->PrepareQuery(bAppend, i))
			{
				//if (pJoinOnClause->GetErrId() == Expression::EMPTY_MESSAGE)
				//	return RULE_BACKTRACK;

				m_OwnerEngine.SetRTError(EngineScheduler::QUERY_DATTBL_RULE_EVAL_JOINON_ERR, pJoinOnClause->GetErrDescription());
				return RULE_ABORT;
			}
			bAppend = TRUE;
		}
	}

	if (m_pWhereClause == NULL && m_pHavingClause == NULL && !bAppend)
	{
		// Se la tabella non e` ancora aperta occorre aprirla
		if (!m_pDataTable->IsOpen())
		{
			TRY
			{
				m_pDataTable->Open(FALSE, E_FAST_FORWARD_ONLY);
				m_pDataTable->SelectFromAllTable(); //SelectAll();
			}
			CATCH(SqlException, e)	
			{
				CString strErr =  m_pSelectionMap->GetTableNames() + _T("\n") + e->m_strError;
				m_OwnerEngine.SetRTError(EngineScheduler::QUERY_DATTBL_RULE_READ_ERR, 0, strErr);
				return RULE_ABORT;
			}
			END_CATCH
		}
	}
	else
	{
		// PrepareQuery di WClause esegue anche l'apertura della tabella e la select di tutte
		// le colonne del SqlRecord correntemente associato alla SqlTable connessa alla
		// WClause
		if (m_pWhereClause && !m_pWhereClause->PrepareQuery (bAppend))
		{
			if (m_pWhereClause->GetErrId() == Expression::EMPTY_MESSAGE)
				return RULE_BACKTRACK;

			m_OwnerEngine.SetRTError(EngineScheduler::QUERY_DATTBL_RULE_EVAL_WHERE_ERR, m_pWhereClause->GetErrDescription());
			return RULE_ABORT;
		}
	}

	// Add Context Bag Filters
	if (
		m_OwnerEngine.GetSymTable().GetDocument() && 
		!m_OwnerEngine.GetSymTable().GetDocument()->m_bSkipContext
		)
	{
		m_pDataTable->AddContextBagFilters(m_arContextBagRecords);
	}
	//-------------------------------------------------------------

	if (m_pHavingClause && !m_pHavingClause->PrepareQuery (bAppend || m_pWhereClause != NULL))
	{
		if (m_pHavingClause->GetErrId() == Expression::EMPTY_MESSAGE)
			return RULE_BACKTRACK;

		m_OwnerEngine.SetRTError(EngineScheduler::QUERY_DATTBL_RULE_EVAL_HAVING_ERR, m_pHavingClause->GetErrDescription());
		return RULE_ABORT;
	}

	if (pstrAuxWhereClause && ! pstrAuxWhereClause->IsEmpty())
	{
		CString strFilter, strOrderBy;
		CString strAux = *pstrAuxWhereClause;
		ASSERT(strAux.Left(5) == _T("FROM "));
		strAux = strAux.Mid(5);

		int nPos = strAux.Find(_T(" WHERE "));
		if(nPos < 0)
		{
			nPos = strAux.Find(_T(" ORDER BY "));
			if (nPos >= 0)
			{
				strOrderBy = strAux.Right(strAux.GetLength() - nPos - 10);
				strOrderBy.Trim();
			}
		}
		else
		{
			strFilter = strAux.Mid(nPos + 7);

			int nPosSort = strFilter.Find(_T(" ORDER BY "));
			if (nPosSort >= 0)
			{
				strOrderBy = strFilter.Right(strFilter.GetLength() - nPosSort - 10);
				strOrderBy.Trim();

				strFilter = strFilter.Left(nPosSort); strFilter.TrimRight();
			}
		}
		ASSERT(nPos >= 0);

		CString strTableName = strAux.Left(nPos); strTableName.TrimLeft(); strTableName.TrimRight();
		CString strWoormTable = m_pDataTable->GetAllTableName();
		if (strTableName.CompareNoCase(strWoormTable) == 0)
		{
			if (!strFilter.IsEmpty())
			{
				if (!m_pDataTable->m_strFilter.IsEmpty())
					m_pDataTable->m_strFilter = _T("(") + m_pDataTable->m_strFilter +_T(") AND ") + strFilter;
				else
					m_pDataTable->m_strFilter = strFilter;
			}
			if (!strOrderBy.IsEmpty())
			{
				if (m_pDataTable->m_strSort.IsEmpty())
					m_pDataTable->m_strSort = strOrderBy;
			}
		}
	}

	if (!m_ExtraFilter.IsEmpty() && m_pDataTable->m_strFilter.Find(m_ExtraFilter) == -1)
	{
		if (!m_pDataTable->m_strFilter.IsEmpty())
			m_pDataTable->m_strFilter = _T("(") + m_pDataTable->m_strFilter + _T(") AND ") + m_ExtraFilter;
		else
			m_pDataTable->m_strFilter = m_ExtraFilter;
	}

	m_pDataTable->StripBlankNearSquareBrackets();
	//----

#ifdef _DEBUG
/*	
	{
		CString sRuleParametersValue;
		CStringArray auxParamsArray;
		GetParameters(auxParamsArray);
		if (auxParamsArray.GetSize())
		{
			for (int i = 0; i < auxParamsArray.GetSize(); i++)
			{
				WoormField* pF = dynamic_cast<WoormField*>(m_OwnerEngine.GetSymTable().GetField(auxParamsArray[i]));
				ASSERT_VALID(pF);
				if (pF)
				{
					CString s = ::cwsprintf(_T("%s=%s (%d)\n"), (LPCTSTR)pF->GetName(), (LPCTSTR)(pF->GetRepData()->FormatData()), pF->GetLen());
					s.Remove('%');
					sRuleParametersValue += s;
				}
			}
			CString sTr; sTr.Format(L"Parameters of 'From-Table' rule %s\n%s", this->GetDescription(), sRuleParametersValue);
			//TRACE((LPCTSTR)sTr);
			//TRACE((LPCTSTR)this->ToSqlString());
		}
	}
*/
#endif

	this->m_pBefore->Exec();	//for RS-DEBUGGER only

	TRY
	{
		m_OwnerEngine.m_Scheduler.ExecuteSQLCommandRetVoid(m_pDataTable, (VOID_FUNC) &SqlTable::Query);
	}
	CATCH(SqlException, e)	
	{
		CString strErr (L"Table rule " + m_pSelectionMap->GetTableNames() + _T("\n") + e->m_strError);
		m_OwnerEngine.SetRTError(EngineScheduler::QUERY_DATTBL_RULE_READ_ERR, 0, strErr);
		return RULE_ABORT;
	}
	END_CATCH

	this->m_pAfter->Exec(this);		//for RS-DEBUGGER only
	
	return RULE_SUCCESS;
}

//-----------------------------------------------------------------------------
RuleReturn DataTableRule::ExecSubQuery(BOOL& bRecFoundForConstraint, const Array* arOrderOnThisColumns/* = NULL*/, CString* pstrAuxWhereClause /*=NULL*/)
{
	if (m_pDataTable->IsEmpty())
		return RULE_SUCCESS;
			
	RuleReturn rr = RULE_SUCCESS;

	TRY
	{
		int nCurrentRow = 0;
		while (!m_pDataTable->IsEOF())
		{
			nCurrentRow++;

			m_OwnerEngine.m_Scheduler.PostMessage(WM_COMMAND, ID_SEARCH_TIC);

			if (m_ConstraintMode == SEL_NULL)
			{
				bRecFoundForConstraint = TRUE;
				break;
			}
		
			//if (m_bTmpRule)
			//	UpdateDataStatus();
		
			m_bFailed = FALSE;
		
			if (m_bNullSolution)
			{
				m_bNullSolution = FALSE;
				m_OwnerEngine.m_nNullRulesNum--;
			}
		
			// validate all own fields
			ASSERT(GetSelFieldsNum() > 0);
			for (int i = 0; i < GetSelFieldsNum(); i++)
				GetSelField(i)->RuleUpdated();
		
			// this statement musts be in this place after fields updating
			rr = m_OwnerEngine.ApplyFrom(m_nRuleId + 1, arOrderOnThisColumns, pstrAuxWhereClause);
		
			switch (rr)
			{
				case RULE_ABORT		:
					return RULE_ABORT;
				
				case RULE_BACKTRACK	:
				{
					if (!IsParentOfFailedRules()) 
						return RULE_BACKTRACK;						
					break;
				}
										
				case RULE_SUCCESS	:
				{
					bRecFoundForConstraint = TRUE;
					break;
				}
			}
			
			if (m_nTop > 0 && nCurrentRow == m_nTop)
			{
				break;
			}

			m_pDataTable->MoveNext();

			this->m_pAfter->Exec(this);	//for RS-DEBUGGER only
		}
	}
	CATCH (SqlException, e)
	{
			CString strErr (L"Table rule " + m_pSelectionMap->GetTableNames() + _T("\n") + e->m_strError);
			m_OwnerEngine.SetRTError(EngineScheduler::QUERY_DATTBL_RULE_READ_ERR, 0, strErr);
			return RULE_ABORT;
	}
	END_CATCH
		
	return RULE_SUCCESS;
}


//-----------------------------------------------------------------------------
RuleReturn DataTableRule::Apply (const Array* arOrderOnThisColumns /*=NULL*/, CString* pstrAuxWhereClause /*=NULL*/)
{
	if (!m_WhenExpr.IsEmpty())
	{
		DataBool cond(TRUE);
		if (!m_WhenExpr.Eval(cond))
		{
			m_OwnerEngine.SetRTError(EngineScheduler::QUERY_EXP_RULE_EVAL_EXP_ERR, m_WhenExpr.GetErrDescription());
			return RULE_ABORT;
		}
		if (!cond) 
		{
			m_OwnerEngine.m_nNullRulesNum--;
			return m_OwnerEngine.ApplyFrom(m_nRuleId + 1, arOrderOnThisColumns, pstrAuxWhereClause);
		}
	}
	//----

	CString strSaveOrderBy;

	if (arOrderOnThisColumns && arOrderOnThisColumns->GetSize())
	{
		CString strNewOrderBy;

		for (int j = 0; j < arOrderOnThisColumns->GetSize(); j++)
		{
			COrderColumn* pCol = (COrderColumn*) arOrderOnThisColumns->GetAt(j);
			for (int i = 0; i < m_pSelectionMap->m_SelFields.GetSize(); i++)
			{
				WoormField* pRepField = m_pSelectionMap->m_SelFields.GetAt(i);

				if (pRepField->GetId() == pCol->m_nAliasSelectedColumn)
				{
					//preparo OrderBy 
					if (!strNewOrderBy.IsEmpty()) strNewOrderBy += ", ";
					strNewOrderBy += pRepField->GetPhysicalName();
					if (!pCol->m_bAscendingOrder) strNewOrderBy += " DESC";
				}
			}
		}

		if (!strNewOrderBy.IsEmpty()) 
		{
			strSaveOrderBy = m_pDataTable->m_strSort;
			m_pDataTable->m_strSort = strNewOrderBy;
		}
	}

	RuleReturn rr = ExecQuery(NULL, pstrAuxWhereClause);

	if (arOrderOnThisColumns && arOrderOnThisColumns->GetSize() && !strSaveOrderBy.IsEmpty())
	{
		m_pDataTable->m_strSort = strSaveOrderBy;
	}

	BOOL bRecFoundForConstraint = FALSE;
	switch (rr)
	{
		case RULE_ABORT		:
			 return RULE_ABORT;

		case RULE_SUCCESS	:
		{
			RuleReturn rr = ExecSubQuery(bRecFoundForConstraint, arOrderOnThisColumns, pstrAuxWhereClause);

			if (rr != RULE_SUCCESS)
				return rr;

			if (bRecFoundForConstraint && (m_ConstraintMode != SEL_NULL))
			{
				m_bFailed = FALSE;
				return RULE_SUCCESS;
			}
			
			break;
		}
	}
	
	if	(
			(!bRecFoundForConstraint && (m_ConstraintMode == SEL_NOT_NULL)) ||
			(bRecFoundForConstraint && (m_ConstraintMode == SEL_NULL))
		)
	{
		m_bFailed = TRUE;
		m_OwnerEngine.SetRTError(EngineScheduler::QUERY_DATTBL_RULE_BACKTRACK, 0, m_pSelectionMap->GetTableNames());

		return RULE_BACKTRACK;
	}

	m_bFailed = FALSE;

	if (!m_bNullSolution)
	{
		m_bNullSolution = TRUE;
		m_OwnerEngine.m_nNullRulesNum++;
	}

	// make null all own fields
	for (int i = 0; i < GetSelFieldsNum(); i++)
		GetSelField(i)->RuleNullified();

	return m_OwnerEngine.ApplyFrom(m_nRuleId + 1, arOrderOnThisColumns, pstrAuxWhereClause);
}

//-----------------------------------------------------------------------------
WoormField* DataTableRule::GetSelField (int i) const
{
	ASSERT(m_pSelectionMap);
	return m_pSelectionMap->m_SelFields[i];
}

//-----------------------------------------------------------------------------
CString DataTableRule::GetSelFieldName (int i) const
{
	return GetSelField(i)->GetName();
}

//-----------------------------------------------------------------------------
int DataTableRule::GetSelFieldsNum () const
{
	return m_pSelectionMap ? m_pSelectionMap->m_SelFields.GetSize() : 0;
}

//=============================================================================

IMPLEMENT_DYNAMIC(DebugBlock, Block);

BOOL DebugBlock::Exec(CObject* pOwner/* = NULL*/)
{
	if (pOwner && HasBreakpoint() && GetBreakpoint()->m_nTraceRows)
	{
		CBreakpoint* pB = GetBreakpoint();

		RuleObj* pRule = dynamic_cast<RuleObj*>(pOwner);
		if (pRule)
		{
			if (pB->m_arTracedNames.GetSize() == 0)
			{
				pRule->GetSelFieldNames(pB->m_arTracedNames);
			}

			CStringArray* pRow = new CStringArray();
			pRow->SetSize(pRule->GetSelFieldsNum());

			//------------------------
			{
				DataTableRule* pTblRule = dynamic_cast<DataTableRule*>(pRule);
				if (pTblRule)
				{
					SqlRecord* pRec = pTblRule->m_pDataTable->GetRecord();

					ASSERT(pRec->GetSizeEx() == pTblRule->GetSelFieldsNum());
					int s = min(pRec->GetSizeEx(), pTblRule->GetSelFieldsNum());

					for (int i = 0; i < s; i++)
					{
						SqlRecordItem* pItem = pRec->GetAt(i);
						ASSERT_VALID(pItem->GetDataObj());

						ASSERT(pItem->GetBindingName().CompareNoCase(pTblRule->GetSelFieldName(i)));

						pRow->SetAt(i, pItem->GetDataObj()->FormatData());
					}
					pB->AddTracedValues(pRow);
					//--------------
					goto super_exec;
				}
			}
			//------------------------
			{
				QueryRule* pQRule = dynamic_cast<QueryRule*>(pRule);
				if (pQRule)
				{
					SqlRecord* pRec = pQRule->GetQuery()->GetSqlTable()->GetRecord();

					ASSERT(pRec->GetSizeEx() == pQRule->GetSelFieldsNum());
					int s = min(pRec->GetSizeEx(), pQRule->GetSelFieldsNum());

					for (int i = 0; i < s; i++)
					{
						SqlRecordItem* pItem = pRec->GetAt(i);
						ASSERT_VALID(pItem->GetDataObj());

						ASSERT(pItem->GetBindingName().CompareNoCase(pQRule->GetSelFieldName(i)));

						pRow->SetAt(i, pItem->GetDataObj()->FormatData());
					}
					pB->AddTracedValues(pRow);
					//--------------
					goto super_exec;
				}
			}
			//------------------------
			{
				ExpRule* pExpRule = dynamic_cast<ExpRule*>(pRule);
				if (pExpRule)
				{
					WoormField* pF = pExpRule->GetSelField(0);

					pRow->SetAt(0, pF->GetData(0)->FormatData());

					pB->AddTracedValues(pRow);
					//--------------
					goto super_exec;
				}
			}
			//------------------------
			{
				CondRule* pCRule = dynamic_cast<CondRule*>(pRule);
				if (pCRule)
				{
					WoormField* pF = pCRule->GetSelField(0);

					pRow->SetAt(0, pF->GetData(0)->FormatData());

					pB->AddTracedValues(pRow);
					//--------------
					goto super_exec;
				}
			}
		}
		//------------------------

	}

super_exec:
	return __super::Exec();
}
//=============================================================================
