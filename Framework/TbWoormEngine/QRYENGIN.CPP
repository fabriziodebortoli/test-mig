
#include "stdafx.h"

#include <TBGenlib\baseapp.h>

#include "qrywrkid.h"
#include "qryengin.h"
#include "procdata.h"
#include "datafunc.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif

//===========================================================================
//
//              Class QueryEngine implementation
//
//===========================================================================
IMPLEMENT_DYNAMIC(QueryEngine, RuleEngine);

//---------------------------------------------------------------------------
QueryEngine::QueryEngine (WoormTable& aSymTable, EngineScheduler& aScheduler)
    :
    RuleEngine			(aSymTable, aScheduler)
{}

//---------------------------------------------------------------------------
QueryEngine::~QueryEngine ()
{
	SAFE_DELETE (m_pGroupingTupleEngine);
	SAFE_DELETE (m_pHavingTupleFilterEngine);
}

//---------------------------------------------------------------------------
//@@@ traverso - update dei campi solo del engine corrente non gestito
void QueryEngine::CopyData()
{
    for (int i = 0; i < m_SymTable.GetSize(); i++)
    {
        WoormField* pNM = (WoormField*) m_SymTable[i];

		ASSERT(pNM->IsKindOf(RUNTIME_CLASS(WoormField)));

        pNM->UpdateQryData();
    }
}

//---------------------------------------------------------------------------
BOOL QueryEngine::PutData()
{
    DataBool goodRecord(TRUE);

    if (m_pHavingTupleFilterEngine && !m_pHavingTupleFilterEngine->Eval(goodRecord))
	{
        return SetRTError(EngineScheduler::QUERY_EVAL_QUERY_FILTER, m_pHavingTupleFilterEngine->GetErrDescription());
	}

    if ((BOOL) goodRecord)
        if (!m_EngineStatus[TEMP])
        {
            m_Scheduler.PostMessage(WM_COMMAND, ID_RETRIEVE_TIC);
            if (!QueryProcessing()) 
				return FALSE;

            // reset current data level
            m_SymTable.SetDataLevel(QUERY_ENGINE);
        }
        else
        {
            return SetRTError(EngineScheduler::QUERY_EVAL_QUERY_FILTER, _TB("It cannot sort the report's rules"));
        }

    return TRUE;
}

//---------------------------------------------------------------------------
BOOL QueryEngine::GroupInMem()
{
    if (m_bFirstTuple)
    {
        m_bFirstTuple = FALSE;

        if (!m_pGroupingTupleEngine->Init(this))
			return FALSE;

        CopyData();

        if (!m_pGroupingTupleEngine->EvalFun(TRUE))
			return FALSE;
    }
    else if (m_pGroupingTupleEngine->IsChanged())
    {
        if (!PutData()) 
			return FALSE;

        CopyData();

 		m_pGroupingTupleEngine->ResetOccurrence();

       if (!m_pGroupingTupleEngine->EvalFun(TRUE))
			return FALSE;
    }
    else if (GetRTErrorCode() != EngineScheduler::EMPTY_MESSAGE)
        return FALSE;
	else if (!m_pGroupingTupleEngine->EvalFun())
		return FALSE;

	m_Scheduler.PostMessage(WM_COMMAND, ID_MEM_GROUP_TIC);

    return TRUE;
}

//---------------------------------------------------------------------------
BOOL QueryEngine::TupleProcessing()
{
    m_SymTable.SetDataLevel(QUERY_ENGINE);

    if (m_EngineStatus[GROUPBY])
	{
        if (m_EngineStatus[GROUP_ON_TEMP])
		{
			ASSERT(FALSE);
            return FALSE;
		}
        else
            if (!GroupInMem())
                return FALSE;
 	}
    else
    {
        CopyData();

        if (!PutData()) 
			return FALSE;
    }

  //@@@ traverso - update dei campi solo del engine corrente non gestito
    for (int i = 0; i < m_SymTable.GetSize(); i++)
    {
        WoormField* pNM = (WoormField*) m_SymTable[i];
		ASSERT(pNM->IsKindOf(RUNTIME_CLASS(WoormField)));

		if (pNM->IsARuleItem())
        	pNM->RuleDataProcessed();
    }

    return !UserBreak();
}

//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
BOOL QueryEngine::ExecQuery (const Array* arOrderOnThisColumns /*= NULL*/, CString* pstrAuxWhereClause /*=0*/)
{
    BOOL ok;

    m_bFirstTuple = TRUE;

    if (m_EngineStatus[GROUPBY])
        if (m_EngineStatus[GROUP_ON_TEMP])
            m_Scheduler.PostMessage(WM_COMMAND, ID_BEGIN_GROUP);
        else
            m_Scheduler.PostMessage(WM_COMMAND, ID_BEGIN_MEM_GROUP);

    ok = RuleEngine::ApplyRules(arOrderOnThisColumns, pstrAuxWhereClause);

    if (ok && m_EngineStatus[GROUPBY] && !m_EngineStatus[GROUP_ON_TEMP])
    {
        m_SymTable.SetDataLevel(QUERY_ENGINE);

        ok = PutData();         // process last grouped row in memory

        m_Scheduler.PostMessage(WM_COMMAND, ID_END_MEM_GROUP);
    }

    if (m_EngineStatus[GROUPBY])
        if (m_EngineStatus[GROUP_ON_TEMP])
            m_Scheduler.PostMessage(WM_COMMAND, ID_END_GROUP);
        else
            m_Scheduler.PostMessage(WM_COMMAND, ID_END_MEM_GROUP);

    if (m_EngineStatus[TEMP])
    {
		ASSERT(FALSE);
        //m_pTmpRule->ResetDataMapping();

        if (ok)
          ok = RuleEngine::Retrieve();
    }

    return ok;
}

//---------------------------------------------------------------------------
BOOL QueryEngine::ParseQueryRules(Parser& parser, CTBContext* pTBContext)
{
    if (parser.Matched(T_RULES))
	{
	    do
	    {
	        switch (parser.LookAhead())
	        {
	            case T_FROM :
	            {
	                DataTableRule* datTblRule = new DataTableRule(*this, pTBContext);
	                RuleEngine::AddRule(datTblRule);
	                if (!datTblRule->Parse(parser)) 
						return FALSE;
	                break;
	            }
	
	            case T_IF   :
	            {
	                CondRule* condRule = new CondRule(*this);
	                RuleEngine::AddRule(condRule);
	                if (!condRule->Parse(parser)) 
						return FALSE;
	                break;
	            }
		           
				case T_FOR   :
				case T_WHILE :
	            {
	                WhileRule* wRule = new WhileRule(*this);
	                RuleEngine::AddRule(wRule);
	                if (!wRule->Parse(parser)) 
						return FALSE;
	                break;
	            }
				
				case T_QUERY   :
	            {
	                QueryRule* qRule = new QueryRule(*this, pTBContext);
	                RuleEngine::AddRule(qRule);
	                if (!qRule->Parse(parser)) 
						return FALSE;
	                break;
	            }

	            case T_ID   :
	            {
	                ExpRule* expRule = new ExpRule(*this);
	                RuleEngine::AddRule(expRule);
	                if (!expRule->Parse(parser)) 
						return FALSE;
	                break;
	            }
	
	            default : return parser.SetError(_TB("Rule not found"));
	        }
	    }
	    while (!parser.Matched(T_END) && !parser.ErrorFound());
	}
    return !parser.ErrorFound();
}

//---------------------------------------------------------------------------
BOOL QueryEngine::ParseGroupBy(Parser& parser)
{
    if (m_EngineStatus[GROUPBY])
        return parser.SetError(_TB("GroupBy already defined"));

    if (!parser.ParseTag(T_BY))
    	return FALSE;

	m_pGroupingTupleEngine = new GroupBy(this);
	
    if (!m_pGroupingTupleEngine->Parse(parser))
        return FALSE;

    m_EngineStatus.On(GROUPBY);
    
    return TRUE;
}

//---------------------------------------------------------------------------
BOOL QueryEngine::ParseHavingFilter(Parser& parser)
{
    if (m_pHavingTupleFilterEngine)
	    return parser.SetError(_TB("Having group filter already defined"));
	    
	m_pHavingTupleFilterEngine = new ExpressionWithDebugBlocks(this);
	m_pHavingTupleFilterEngine->m_pBefore->m_strOwnerName = L"Tuple.Having.Before";
	m_pHavingTupleFilterEngine->m_pAfter->m_strOwnerName = L"Tuple.Having.After";

    if (!m_pHavingTupleFilterEngine->Parse(parser, DATA_BOOL_TYPE))
        return FALSE;

	return parser.ParseSep();
}
//---------------------------------------------------------------------------
BOOL QueryEngine::ParseTupleFilter(Parser& parser)
{
	if (m_pTupleFilterEngine)
		return parser.SetError(_TB("Tuple filter already defined"));

	m_pTupleFilterEngine = new ExpressionWithDebugBlocks(this);
	m_pTupleFilterEngine->m_pBefore->m_strOwnerName = L"Tuple.Filter.Before";
	m_pTupleFilterEngine->m_pAfter->m_strOwnerName = L"Tuple.Filter.After";

	if (!m_pTupleFilterEngine->Parse(parser, DATA_BOOL_TYPE))
		return FALSE;

	return parser.ParseSep();
}

//---------------------------------------------------------------------------
BOOL QueryEngine::Compile(Parser& parser, CTBContext* pTBContext)
{
    if	(!ParseQueryRules(parser, pTBContext))
        return FALSE;

	if (parser.Matched(T_WHERE) && !ParseTupleFilter(parser))
		return FALSE;

	if (parser.Matched(T_GROUP) && !ParseGroupBy(parser))
		return FALSE;

	if (parser.Matched(T_HAVING) && !ParseHavingFilter(parser))
		return FALSE;

	if (parser.ErrorFound())
		return FALSE;

	return TRUE;
	//posticipato a dopo il parsing del resto del report (per riconoscere le queries)
	//RuleEngine::BuildTree(parser, m_pGroupingTupleEngine);
}

//---------------------------------------------------------------------------
BOOL QueryEngine::BuildTree(Parser& parser)
{
	return RuleEngine::BuildTree(parser, m_pGroupingTupleEngine);
}

//---------------------------------------------------------------------------
BOOL QueryEngine::ResolveQueries(CString& sErr)
{
	for (int i = 0; i <  m_UnsortedRules.GetSize(); i++)
	{
		QueryRule* pQR = dynamic_cast<QueryRule*>(m_UnsortedRules.GetAt(i));
		if (!pQR) continue;	//skip delle altre rule
		
		QueryObjItem* pQI = dynamic_cast<QueryObjItem*>(this->GetSymTable().FindQuery(pQR->m_strQueryName));
		if (!pQI) 
		{
			sErr = cwsprintf(_TB("The query {0-%s} was not resolved"), pQR->m_strQueryName);
			return FALSE;
		}

		pQR->m_pQuery = &pQI->m_Query;

		pQR->SetRuleFields(TRUE);
	}

	//------------

	if (m_pGroupingTupleEngine)
	{
		if (!m_pGroupingTupleEngine->CheckRuleItems(sErr))
			return FALSE;
	}

	return TRUE;
}

//---------------------------------------------------------------------------
BOOL RepEngine::ResolveQueries(CString& sErr)
{
	if (!__super::ResolveQueries(sErr))
		return FALSE;

	for (int i = 0; i < this->m_TriggeredEvents.GetSize(); i++)
	{
		TriggeredEvent* pEvent = dynamic_cast<TriggeredEvent*>(m_TriggeredEvents.GetAt(i));
		if (pEvent && !pEvent->CheckRuleItems(sErr))
			return FALSE;		
	}

	return TRUE;
}
//---------------------------------------------------------------------------
