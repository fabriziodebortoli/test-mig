
#include "stdafx.h"

#include <TbNameSolver\TbNamespaces.h>

#include <TbGeneric\EnumsTable.h>
#include <TbGeneric\DataObj.h>
#include <TbGeneric\FunctionCall.h>
#include <TbGeneric\CollateCultureFunctions.h>

#include <TbParser\TokensTable.h>
#include <TbParser\SymTable.h>

#include <TbGenlib\baseapp.h>
#include <TbGenlib\basedoc.h>
#include <TbGenlib\parsctrl.h>
#include <TbGenlib\expparse.h>
#include <TbGenlib\expr.h>

#include "inputmng.h"
#include "repfield.h"
#include "report.h"
#include "repengin.h"
#include "askdata.h"
#include "MultiLayout.h"
#include "procdata.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif

static const TCHAR BASED_CODE szOnAbend				[] = _T("OnAbend");

//===========================================================================
//              Class EventActions implementation
//===========================================================================
//---------------------------------------------------------------------------
EventActions::EventActions (RepEngine& OwnRepEngine)
	:
	m_OwnRepEngine (OwnRepEngine),
	m_pBeforeBlock  (new Block(NULL, NULL, &OwnRepEngine)),
	m_pAfterBlock   (new Block(NULL, NULL, &OwnRepEngine))
{}

EventActions::~EventActions()
{
	SAFE_DELETE(m_pBeforeBlock);
	SAFE_DELETE(m_pAfterBlock);
}

//---------------------------------------------------------------------------
BOOL EventActions::ParseEventActions(Parser& parser, BOOL bMustExist)
{
	BOOL bActionsFound = FALSE;

	if (parser.Matched(T_BEFORE))
		if (!m_pBeforeBlock->Parse(parser))
			return FALSE;
		else
			bActionsFound = TRUE;

	if (!parser.ErrorFound() && parser.Matched(T_AFTER))
		if (!m_pAfterBlock->Parse(parser))
			return FALSE;
		else
			bActionsFound = TRUE;

	if (!parser.ErrorFound() && bMustExist && !bActionsFound)
		return parser.SetError(_TB("Missing action"));

	return !parser.ErrorFound();
}

//===========================================================================
FormFeedEvents::FormFeedEvents(RepEngine& re)
	:
	EventActions(re)
{
	m_pBeforeBlock->m_strOwnerName = L"FormFeed.Before";
	m_pAfterBlock->m_strOwnerName  = L"FormFeed.After";
}
//===========================================================================

FillTableEvents::FillTableEvents(RepEngine& re, const CString& sTableName)
	:
	EventActions(re)
{
	m_pBeforeBlock->m_strOwnerName = L"Fill." + sTableName + L".Before";
	m_pAfterBlock->m_strOwnerName	= L"Fill." + sTableName + L".After";
}
//===========================================================================
//              Class ReportEvents implementation
//===========================================================================
//---------------------------------------------------------------------------
ReportEvents::ReportEvents (RepEngine& OwnRepEngine)
	:
	EventActions	(OwnRepEngine),
	m_pAlwaysBlock	(new Block(NULL, NULL, &OwnRepEngine)),
	m_pFinalizeBlock(new Block(NULL, NULL, &OwnRepEngine))
{
	m_pBeforeBlock->m_strOwnerName		= L"Report.Before";
	m_pAlwaysBlock->m_strOwnerName		= L"Report.Always";
	m_pAfterBlock->m_strOwnerName		= L"Report.After";
	m_pFinalizeBlock->m_strOwnerName	= L"Report.Finalize";
}

ReportEvents::~ReportEvents()
{
	SAFE_DELETE(m_pAlwaysBlock);
	SAFE_DELETE(m_pFinalizeBlock);
}

//---------------------------------------------------------------------------
BOOL ReportEvents::Parse(Parser& parser)
{
	BOOL bActionsFound = FALSE;

	if (parser.Matched(T_ALWAYS))
	{
		if (!m_pAlwaysBlock->Parse(parser))
			return FALSE;

		bActionsFound = TRUE;
		m_OwnRepEngine.m_bRepDispActionFound = m_OwnRepEngine.m_bDispActionFound;
	}

	BOOL bOk = !parser.ErrorFound() && ParseEventActions(parser, FALSE);

	if (bOk)
	{
		if (parser.Matched(T_FINALIZE))
		{
			BOOL bPrev = m_OwnRepEngine.m_bDispActionFound;
			m_OwnRepEngine.m_bDispActionFound = false;

			if (!m_pFinalizeBlock->Parse(parser))
				return FALSE;

			m_OwnRepEngine.m_bRepDispActionFound = m_OwnRepEngine.m_bRepDispActionFound || m_OwnRepEngine.m_bDispActionFound;
			m_OwnRepEngine.m_bDispActionFound = bPrev;
		}
	}
	return !parser.ErrorFound();
}

//===========================================================================
//              Class TriggeredEvent implementation
//===========================================================================
//---------------------------------------------------------------------------
TriggeredEvent::TriggeredEvent (RepEngine& OwnRepEngine, const CString& sName)
	:
	EventActions	(OwnRepEngine),
	m_bOccurred		(TRUE),
	m_BoolOperator	(T_NOTOKEN),
	m_WhenExpr		(&(OwnRepEngine.GetSymTable())),
	m_sName			(sName)
{
	m_pBeforeBlock->m_strOwnerName = L"Breaking." + sName + L".Before";
	m_pAfterBlock->m_strOwnerName  = L"Breaking." + sName + L".After";
}

//---------------------------------------------------------------------------
BOOL TriggeredEvent::EvalWhen (BOOL bQry)
{
	DataBool    cond;
	BOOL        bRetVal;

	if (bQry)
		(m_OwnRepEngine.GetSymTable()).SetDataLevel(QUERY_ENGINE);

	if (!m_WhenExpr.Eval(cond))
		bRetVal = m_OwnRepEngine.SetRTError(EngineScheduler::REPORT_EVAL_WHEN_EXP, m_WhenExpr.GetErrDescription());
	else
		bRetVal = (int) cond;

	(m_OwnRepEngine.GetSymTable()).SetDataLevel(REPORT_ENGINE);

	return bRetVal;
}

//---------------------------------------------------------------------------
BOOL TriggeredEvent::Check (BOOL bQry /*= TRUE*/)
{        
	m_bOccurred = TRUE;
		
	if ((m_BreakList.GetSize() == 0) || (m_BoolOperator != T_NOTOKEN))
	{
		// if there is a WHEN condition then eval the when expression
		// for all rows excluding the last one
		// NEW try to use on last row too (row's fields has the last value)
		if (m_OwnRepEngine.GetEngineStatus() != RepEngine::RE_LAST_ROW || !m_WhenExpr.HasRuleFields())
		{
			m_bOccurred = EvalWhen(bQry);
		
			if (!m_bOccurred && 
				m_OwnRepEngine.GetRTErrorCode() != EngineScheduler::EMPTY_MESSAGE)
					return FALSE;
		}
		else
			m_bOccurred = FALSE;
	}

	if (m_BreakList.GetSize() != 0)
	{
		BOOL brk = FALSE;

		if (m_OwnRepEngine.GetEngineStatus() != RepEngine::RE_BODY)
			brk = TRUE;
		else
			for (int i = 0; i < m_BreakList.GetSize(); i++)
			{
				brk = ((WoormField*) m_BreakList[i])->IsQueryChanged();
				if (brk) break;
			}

		switch (m_BoolOperator)
		{
			case T_AND  	:	m_bOccurred = m_bOccurred && brk;   break;
			case T_OR   	:	m_bOccurred = m_bOccurred || brk;   break;
			case T_NOTOKEN	:	m_bOccurred = brk; break;
			default			:	m_bOccurred = FALSE; break;
		}
	}

	return TRUE;
}

//---------------------------------------------------------------------------
inline BOOL TriggeredEvent::DoBeforeActions ()
{
	return m_bOccurred ? m_pBeforeBlock->Exec() : TRUE;
}

//---------------------------------------------------------------------------
BOOL TriggeredEvent::DoAfterActions ()
{
  BOOL ok = TRUE;

  if (m_bOccurred) ok = m_pAfterBlock->Exec();

  m_bOccurred = FALSE;

  return ok;
}

//---------------------------------------------------------------------------
BOOL TriggeredEvent::CheckRuleItems(CString& sErr)
{
	for (int i= 0; i < m_BreakingFieldsToCheck.GetSize(); i++)
	{
		WoormField* pRepField = dynamic_cast<WoormField*>(m_BreakingFieldsToCheck.GetAt(i));
		if (!pRepField->IsARuleItem())
		{
			sErr = cwsprintf(_TB("The field {0-%s} cannot be used into breaking condition"), pRepField->GetName());
			return FALSE;
		}
	}
	return TRUE;
}

//---------------------------------------------------------------------------
BOOL TriggeredEvent::Parse(Parser& parser)
{
	m_BreakList.RemoveAll();
	m_BreakingFieldsToCheck.RemoveAll();

	BOOL bWhenFound = FALSE;

	if (parser.Matched(T_BREAKING))
	{
		CString strBuffer;

		do
		{
			if (!parser.ParseID(strBuffer)) return FALSE;

			WoormField* pRepField = (WoormField*) (m_OwnRepEngine.GetSymTable()).GetField(strBuffer);
			if (pRepField == NULL)
				return parser.SetError(Expression::FormatMessage(Expression::UNKNOWN_FIELD), strBuffer);

			if (!pRepField->IsARuleItem())
			{
				//return parser.SetError(_TB("The field cannot be used into breaking condition"), strBuffer);
				m_BreakingFieldsToCheck.Add(pRepField);
			}

			m_BreakList.Add(pRepField);
		}
		while (parser.Matched(T_COMMA));

		if (parser.ErrorFound()) return FALSE;

		switch (parser.LookAhead())
		{
			case T_AND  :   parser.SkipToken(); m_BoolOperator = T_AND; break;
			case T_OR   :   parser.SkipToken(); m_BoolOperator = T_OR; break;
			default     :   if (parser.ErrorFound())
								return FALSE;
							else
								m_BoolOperator = T_NOTOKEN;
		}

		if (m_BoolOperator != T_NOTOKEN)
		{
			parser.Matched(T_WHEN); // optional WHEN keyword after AND/OR operator
			bWhenFound = TRUE;           // anyway an expression comes after
		}
	}
	else
		bWhenFound = parser.ParseTag(T_WHEN);

	if (parser.ErrorFound()) return FALSE;

	if (bWhenFound)
	{
		m_WhenExpr.SetStopTokens(T_DO);
		if (!m_WhenExpr.Parse(parser, DATA_BOOL_TYPE))
			return FALSE;
	}

	return parser.ParseTag(T_DO) && ParseEventActions(parser);
}

//===========================================================================
//              Class RepEngine implementation
//===========================================================================
IMPLEMENT_DYNAMIC(RepEngine, QueryEngine);

//---------------------------------------------------------------------------
RepEngine::RepEngine (ReportEngine* pOwnerReport)
	:
	QueryEngine         (*(pOwnerReport->m_pSymTable), *(pOwnerReport->m_pScheduler)),
	m_pOutChannel       (NULL),
	m_pOnFormFeedEvents(NULL),
	m_pReportEvents    (NULL),
	m_AskingRules		(*(pOwnerReport->m_pSymTable)),
	m_pOwnerReport		(pOwnerReport),
	m_pAutoFormFeed		(NULL),
	m_EngineStatus		(RE_INIT),
	m_pViewST			(NULL)
{}

//---------------------------------------------------------------------------
RepEngine::~RepEngine ()
{
	if (m_pOnFormFeedEvents) delete m_pOnFormFeedEvents;
	if (m_pReportEvents) delete m_pReportEvents;
	if (m_pAutoFormFeed) delete m_pAutoFormFeed;
}

//---------------------------------------------------------------------------
BOOL RepEngine::QueryProcessing()
{
	int i;

	m_SymTable.SetDataLevel(REPORT_ENGINE);

	for (i = 0; i < m_TriggeredEvents.GetSize(); i++)
		if (!((TriggeredEvent*) m_TriggeredEvents[i])->Check())
			return FALSE;

	if (GetEngineStatus() != RepEngine::RE_FIRST_ROW)
	{
		if (!m_pAutoFormFeed)
		{
			for (i = 0; i < m_TriggeredEvents.GetSize(); i++)
				if (!((TriggeredEvent*) m_TriggeredEvents[i])->DoBeforeActions())
					return FALSE;
		}
		else	
			if (!m_pAutoFormFeed->Exec())
				return FALSE;
	}

	m_EngineStatus = RE_BODY;

	SymField* pNameMember;

	//@@@ traverso - update dei campi solo del engine corrente non gestito
	for (i = 0; i < m_SymTable.GetSize(); i++)
	{
		pNameMember = (SymField*) m_SymTable[i];
		if (pNameMember->IsKindOf(RUNTIME_CLASS(WoormField)))
			((WoormField*) pNameMember)->UpdateRepData();
	}

	for (i = 0; i < m_TriggeredEvents.GetSize(); i++)
		if (!((TriggeredEvent*) m_TriggeredEvents[i])->DoAfterActions())
			return FALSE;

	if (m_pReportEvents && !m_pReportEvents->GetAlwaysActions().Exec())
		return FALSE;

	GetCallerDoc()->m_bIsFirstTuple = FALSE;

	//@@@ traverso - update dei campi solo del engine corrente non gestito
	for (i = 0; i < m_SymTable.GetSize(); i++)
	{
		SymField* pField = m_SymTable.GetAt(i);
		if (pField->IsKindOf(RUNTIME_CLASS(WoormField)) && ((WoormField*) pField)->IsARuleItem())
			((WoormField*) pField)->RepDataProcessed();
	}

	return !UserBreak();
}

//-----------------------------------------------------------------------------
BOOL RepEngine::FinalizeExec(COutParamBag* pBag)
{
	if (m_pReportEvents && !m_pReportEvents->GetFinalizeActions().Exec())
	{                                
		CString nl("\n");
		CString strError = EngineScheduler::FormatMessage(EngineScheduler::REPORT_ENGINE_ABORTED);
		UINT mb_icon = MB_ICONSTOP;
		EngineScheduler::MessageID	errcode = m_Scheduler.GetRTErrorCode();

		CString	strerrexpl	= m_Scheduler.GetRTErrorExpl();
		strError += nl + EngineScheduler::FormatMessage (errcode);
		if (!strerrexpl.IsEmpty ()) strError += nl + strerrexpl;
		strError += nl + m_Scheduler.GetRTErrorFailedName();
	
		ASSERT_VALID(m_pOwnerReport);
		if (m_pOwnerReport)
		{
			CWoormDoc* pWDoc = m_pOwnerReport->GetCallerDoc();
			ASSERT_VALID(pWDoc);
			if (pWDoc)
			{
				if (pWDoc->IsHideFrame())
					pWDoc->m_pWoormInfo ->m_strError = strError;
				else
					m_pOwnerReport->GetCallerDoc()->Message(strError, MB_OK | mb_icon);
			}
		}

		return FALSE;
	}
	ASSERT(pBag);
	if (pBag)
		pBag->Update();
	return TRUE;
}

//---------------------------------------------------------------------------
BOOL RepEngine::EndExec(COutParamBag* pBag, EngineScheduler::MessageID msg /*= EngineScheduler::EMPTY_MESSAGE*/, const CString& sErr /*= ""*/, LPCTSTR psz /*= NULL*/)
{
	BOOL bThereIsError = FALSE;

	if (msg != EngineScheduler::EMPTY_MESSAGE)
		SetRTError(msg, sErr, psz);

	SymField* pField = m_SymTable.GetField(SpecialReportField::NAME.STATUS);
	DataEnum* pRepStatus = (DataEnum*)(pField ? pField->GetData() : NULL);
	ASSERT(pRepStatus);
	if (pRepStatus)
		pRepStatus->Assign(TAG_REPORT_STATUS, TAG_REPORT_STATUS_SUCCESS);

	if (m_Scheduler.GetRTErrorCode() != EngineScheduler::EMPTY_MESSAGE)
	{
		CString strError;                      
		CString nl("\n");
		UINT mb_icon;
		//oltre a creare il messaggio devo valorizzare opportunamente il field ReportStatus 
		EngineScheduler::MessageID	errcode = m_Scheduler.GetRTErrorCode();

		switch (errcode)
		{       
			case	EngineScheduler::QUERY_EXP_RULE_BACKTRACK:
			case	EngineScheduler::QUERY_COND_RULE_BACKTRACK:
			case	EngineScheduler::QUERY_DATTBL_RULE_BACKTRACK:

				if (pRepStatus)
					pRepStatus->Assign(TAG_REPORT_STATUS, TAG_REPORT_STATUS_NO_DATA_FOUND);

				strError = EngineScheduler::FormatMessage(EngineScheduler::REPORT_ENGINE_TERMINATED);
				mb_icon = MB_ICONINFORMATION;
				break;

			case	EngineScheduler::QUERY_RULES_USER_BREAK:

				if (pRepStatus)
					pRepStatus->Assign(TAG_REPORT_STATUS, TAG_REPORT_STATUS_USER_ABORT);

				strError = EngineScheduler::FormatMessage(EngineScheduler::REPORT_ENGINE_ABORTED);
				mb_icon = MB_ICONWARNING;

				bThereIsError = TRUE;
				break;

			case	EngineScheduler::REPORT_DIALOG_ONABORT_MESSAGE:
			case	EngineScheduler::REPORT_ABORT_MESSAGE:

				if (pRepStatus)
					pRepStatus->Assign(TAG_REPORT_STATUS, TAG_REPORT_STATUS_SCRIPT_ABORT);

				strError = EngineScheduler::FormatMessage(EngineScheduler::REPORT_ENGINE_ABORTED);
				mb_icon = MB_ICONWARNING;

				bThereIsError = TRUE;
				break;

			case	EngineScheduler::REPORT_UNATTENDED:

				if (pRepStatus)
					pRepStatus->Assign(TAG_REPORT_STATUS, TAG_REPORT_STATUS_SCRIPT_ABORT);

				strError = EngineScheduler::FormatMessage(EngineScheduler::REPORT_UNATTENDED);
				mb_icon = MB_ICONWARNING;

				bThereIsError = TRUE;
				break;

			case	EngineScheduler::REPORT_QUIT:

				if (pRepStatus)
					pRepStatus->Assign(TAG_REPORT_STATUS, TAG_REPORT_STATUS_SCRIPT_QUIT);

				strError.Empty();
				mb_icon = MB_ICONINFORMATION;

				//bThereIsError = FALSE;
				break;

			default: 
				if (pRepStatus)
					pRepStatus->Assign(TAG_REPORT_STATUS, TAG_REPORT_STATUS_ABNORMAL_END);

				strError = EngineScheduler::FormatMessage(EngineScheduler::REPORT_ENGINE_ABORTED);
				mb_icon = MB_ICONSTOP;

				bThereIsError = TRUE;
		}

		CString	strerrexpl	= m_Scheduler.GetRTErrorExpl();
		strError += nl + EngineScheduler::FormatMessage (errcode);
		if (!strerrexpl.IsEmpty ()) strError += nl + strerrexpl;
		strError += nl + m_Scheduler.GetRTErrorFailedName();

		if (errcode != EngineScheduler::REPORT_QUIT && errcode != EngineScheduler::QUERY_RULES_USER_BREAK)
		{
			ASSERT_VALID(m_pOwnerReport);
			if (m_pOwnerReport)
			{
				CWoormDoc* pWDoc = m_pOwnerReport->GetCallerDoc();
				ASSERT_VALID(pWDoc);
				if (pWDoc)
				{
					if (pWDoc->IsHideFrame())
						pWDoc->m_pWoormInfo ->m_strError = strError;
					else
						m_pOwnerReport->GetCallerDoc()->Message(strError, MB_OK | mb_icon);
				}
			}
		}
		m_Scheduler.ResetRTError();
	
		ASSERT(m_pOwnerReport);
		if (m_pOwnerReport)
			m_pOwnerReport->ExecSpecialProc(szOnAbend);
	}

	BOOL bFinalizeOk = FinalizeExec(pBag);
	
	return !bThereIsError && bFinalizeOk;
}

//--------------------------------------------------------------------------
void RepEngine::UpdateViewSymbolTable(WoormTable* pST)
{
	if (pST && pST->GetSize())
	{
		ASSERT_VALID(pST);

		int i;
		for (i = 0; i < pST->GetSize(); i++)
		{
			SymField* pF = pST->GetAt(i);
			ASSERT_VALID(pF);

			SymField* pORF = m_SymTable.GetField(pF->GetName());
			if (pORF == NULL)
			{
				continue;
			}
			ASSERT_VALID(pORF);
			if (!pORF->IsKindOf(RUNTIME_CLASS(WoormField)))
			{
				TRACE(_T("Field %s unmatch field type\n"), pF->GetName());
				continue;
			}
			WoormField* pRF = (WoormField*) pORF;

			if (pRF->GetRepData() && DataType::IsCompatible(pRF->GetDataType(), pF->GetDataType()))
			{
				ASSERT_VALID(pRF->GetRepData());

				pF->AssignData(*(pRF->GetRepData()));
			}
			else
			{
				TRACE(_T("Field %s unmatch datatype\n"), pF->GetName());
			}
		}
	}
}

//--------------------------------------------------------------------------
// OnRun : Main engine loop
BOOL RepEngine::ExecReport (RDEManager* pOutChannel, Array* arOrderOnThisColumns /*= NULL*/, CString* pstrAuxWhereClause /*=0*/, CStaticFieldArrayCache* pStaticFields/*Use pre static input , else NULL*/, BOOL keepValues, CFunctionDescription* pFD, WoormTable* pST/*= NULL*/)
{
	GetCallerDoc()->m_dsCurrentLayoutView = REPORT_DEFAULT_LAYOUT_NAME;
	GetCallerDoc()->m_bIsFirstTuple = FALSE;
	GetCallerDoc()->m_bIsLastTuple = FALSE;

	m_pOutChannel = pOutChannel;
	m_parOrderOnThisColumns = arOrderOnThisColumns;
	m_pstrAuxWhereClause = pstrAuxWhereClause;
	
	m_pViewST = pST;

	m_SymTable.SetDataLevel(REPORT_ENGINE);
	m_EngineStatus = RE_INIT;

	Expression::MessageID err = Expression::EMPTY_MESSAGE;

	COutParamBag  bag (
						pFD, 
						m_pOwnerReport->GetCallerDoc()->m_bUpdateOutputParametersEvenIfReportDoesNotFetchRecords,
						pStaticFields,
						&m_SymTable
					);

	for (int i = 0; i < m_SymTable.GetSize(); i++)
	{
		WoormField* pRF = dynamic_cast<WoormField*>(m_SymTable[i]);
		ASSERT_VALID(pRF);
		if (pRF)
		{
			CString strName = pRF->GetName();
			CDataObjDescription* par = pFD && pFD->GetParameters().GetSize() ? pFD->GetParamDescription(strName) : NULL;

			if (par)
			{
				if
					(
						par->GetPassedMode() == CDataObjDescription::_INOUT
						||
						par->GetPassedMode() == CDataObjDescription::_OUT
						)
				{
					bag.m_arIndexOutParam.Add(pFD->GetParamIndex(strName));
					bag.m_arOutField.Add(pRF);
				}

				if (pRF->GetRepData())
				{
					if
						(
							par->GetPassedMode() == CDataObjDescription::_INOUT ||
							par->GetPassedMode() == CDataObjDescription::_IN
							)
					{
						pRF->Init(par->GetValue());

						bag.m_arInField.Add(pRF);
					}
					continue;
				}
			}
			if (pRF->IsStatic()/*never reinitialize*/)
			{
				if (bag.m_parStaticFields && bag.m_parStaticFields->RetrieveStaticValue(pRF))
				{
					continue;
				}
			}
			if (pRF->IsReInit()/*always reinitialize*/ || !keepValues || !bag.m_parStaticFields || !bag.m_parStaticFields->RetrieveStaticValue(pRF))
			{
				err = pRF->Init();

				if (err)
					return EndExec
								(
									&bag,
									EngineScheduler::REPORT_EVAL_INIT_EXPR,
									Expression::FormatMessage(err),
									pRF->GetName()
								);
			}
		}
	}
			
	BOOL bEditingFromExternalController = m_pOwnerReport->IsEditingParamsFromExternalController();

	GetCallerDoc()->m_dsCurrentLayoutEngine = GetCallerDoc()->m_dsCurrentLayoutView;
	m_SymTable.ReattachDisplayTable(GetCallerDoc()->m_dsCurrentLayoutEngine);
	GetCallerDoc()->UpdateLayout();

	if (GetCallerDoc()->m_bDebugging)
	{
		GetCallerDoc()->DoOpenDebugger(NULL);
	}

	CString strErr;
	bag.m_arFieldsInitialized.RemoveAll();

	if (m_AskingRules.GetCount() > 0)
	{
		AskDialogInputMng aAskDialogInputMng(&m_AskingRules, &m_SymTable, GetCallerDoc());

		if (!aAskDialogInputMng.ExecAskRules(GetCallerDoc()->GetFrame(), m_pOwnerReport, FALSE, &bag))
		{
			EndExec(&bag);
			return FALSE;
		}
	}
	
	if (bEditingFromExternalController && !m_pOwnerReport->m_pCallerDoc->m_pExternalControllerInfo->m_bEditAndRun) //é una uscita per SUCCESSO
		return (m_pReportEvents ? m_pReportEvents->GetFinalizeActions().Exec() : TRUE); 

	if (GetCallerDoc()->m_dsCurrentLayoutEngine != GetCallerDoc()->m_dsCurrentLayoutView)
	{
		GetCallerDoc()->m_dsCurrentLayoutView = GetCallerDoc()->m_dsCurrentLayoutEngine;
		m_SymTable.ReattachDisplayTable(GetCallerDoc()->m_dsCurrentLayoutEngine);
		GetCallerDoc()->UpdateLayout();
	}

	UpdateViewSymbolTable(pST);
		
	m_pOutChannel->WriteInitialSpecialField();

	if (!RunEngine())
	{
		strErr.Format(_TB("Error running Report Engine"));
		EndExec(&bag, EngineScheduler::REPORT_ENGINE_ABORTED);
		return FALSE;
	}

	if (m_AsyncEngineRunningStatus == 1)
	{
		bag.m_bForceUpdateOutputParameters = TRUE;

		return FinalizeExec(&bag);
	}
	else
		return EndExec(&bag);
}

//-----------------------------------------------------------------------------
UINT __cdecl AsyncRun (LPVOID pParam)
{
    RepEngine* pEngine = (RepEngine*)pParam;

    if (pEngine == NULL || !pEngine->IsKindOf(RUNTIME_CLASS(RepEngine)))
		return 1;

	InitThreadContext();

	pEngine->ExecEngine();

    return 0;  
}

//--------------------------------------------------------------------------
BOOL RepEngine::RunEngine()
{
	CPushMessageLoopDepthMng __pushLoopDepth(WOORM_REPORT);
	AfxGetThreadContext()->RaiseCallBreakEvent();

	m_AsyncEngineRunningStatus = -1;
	if (m_Scheduler.m_bUseMultithreading)
	{
		GetCallerDoc()->m_bRunningInAThread = TRUE;

		if (!AfxBeginTBThread(AsyncRun, this, AfxGetLoginContext(), THREAD_PRIORITY_NORMAL, 0,0, NULL, "report_engine"))
			return FALSE;

		m_Scheduler.LoopUntil(m_AsyncEngineRunningStatus, TRUE);
	}
	else
	{
		GetCallerDoc()->m_bRunningInAThread = FALSE;

		ExecEngine ();
	}

	GetCallerDoc()->m_bRunningInAThread = FALSE;

	return TRUE;
}

//--------------------------------------------------------------------------
void RepEngine::ExecEngine ()
{
	m_EngineStatus = RE_BEFORE;

	GetCallerDoc()->m_pTbContext->Connect();
	// report starts then it must do de report before actions
	if (m_pReportEvents && !m_pReportEvents->GetBeforeActions().Exec())
		goto endExec;

	UpdateViewSymbolTable (this->m_pViewST);

	m_EngineStatus = RE_FIRST_ROW;
	GetCallerDoc()->m_bIsFirstTuple = TRUE;

	if (!QueryEngine::ExecQuery(m_parOrderOnThisColumns, m_pstrAuxWhereClause)) 
		goto endExec;

	m_EngineStatus = RE_LAST_ROW;
	GetCallerDoc()->m_bIsLastTuple = TRUE;

	// at the last tuple it must do the triggered before actions
	for (int i = 0; i < m_TriggeredEvents.GetSize(); i++)
		if (!((TriggeredEvent*) m_TriggeredEvents[i])->Check(FALSE))
			goto endExec;

	m_SymTable.SetDataLevel(REPORT_ENGINE);

	for (int i = 0; i < m_TriggeredEvents.GetSize(); i++)
		if (!((TriggeredEvent*) m_TriggeredEvents[i])->DoBeforeActions())
			goto endExec;

	m_EngineStatus = RE_BODY;	// the after actions are to be considered as part of a body report (TODO: RE_AFTER);
	GetCallerDoc()->m_bIsLastTuple = FALSE;

	if (m_pReportEvents ? !m_pReportEvents->GetAfterActions().Exec() : FALSE)
		goto endExec;

	m_AsyncEngineRunningStatus = 1;
	GetCallerDoc()->m_pTbContext->Disconnect();
	return;

endExec:
	m_AsyncEngineRunningStatus = 0;
	GetCallerDoc()->m_pTbContext->Disconnect();
}

//---------------------------------------------------------------------------
/*DataBool RepEngine::ShowAskDialog(DataStr strDialogName)
{
	AskDialogData* pAskDialogData = m_AskingRules.GetAskDialog(strDialogName);
	if(!pAskDialogData)
		return SetRTError(EngineScheduler::REPORT_EMPTY_DIALOG, 0, strDialogName.GetString());

	AskDialogInputMng aAskDialogInputMng(&m_AskingRules, &m_SymTable, GetCallerDoc());

	if(!aAskDialogInputMng.ExecAsk(GetCallerDoc()->GetFrame(), pAskDialogData, m_pOwnerReport))
		return SetRTError(EngineScheduler::QUERY_RULES_USER_BREAK);

	return TRUE;
}*/

//------------------------------------------------------------------------------------
DataBool RepEngine::ShowAllAskDialogs(const CStringArray& allDialogs)
{
	for (int i = 0; i < allDialogs.GetCount(); i++)
	{
		AskDialogData* pAskDialogData = m_AskingRules.GetAskDialog(allDialogs.GetAt(i));
		if (!pAskDialogData)
			return SetRTError(EngineScheduler::REPORT_EMPTY_DIALOG, 0, allDialogs.GetAt(i).GetString());
	}

	AskDialogInputMng aAskDialogInputMng(&m_AskingRules, &m_SymTable, GetCallerDoc());

	aAskDialogInputMng.SetAllActions(allDialogs);

	if (!aAskDialogInputMng.ExecAllActions(GetCallerDoc()->GetFrame(), m_pOwnerReport))
		return SetRTError(EngineScheduler::QUERY_RULES_USER_BREAK);

	return TRUE;
}

//---------------------------------------------------------------------------
BOOL RepEngine::ParseAskingRules(Parser& parser)
{
	if (parser.LookAhead(T_DIALOGS))
		return m_AskingRules.Parse(parser, m_pOwnerReport);

	return TRUE;
}

//---------------------------------------------------------------------------
BOOL RepEngine::ParseStdActions(Parser& parser)
{
	if (parser.Matched(T_FORMFEED))
	{                             
		if (!parser.ParseTag(T_COLON) || !parser.ParseTag(T_DO)) return FALSE;
	
		m_bDispActionFound = FALSE;
		m_bOnFormFeedAction = TRUE;

		m_pOnFormFeedEvents = new FormFeedEvents(*this);
		if (!m_pOnFormFeedEvents->ParseEventActions(parser))
			return FALSE;

		m_bOnFormFeedAction  = FALSE;
	}

	if  (parser.Matched(T_REPORT))
	{
		if (!parser.ParseTag(T_COLON) || !parser.ParseTag(T_DO)) return FALSE;
	
		m_bDispActionFound = FALSE;

		m_pReportEvents   = new ReportEvents(*this);
		if (!m_pReportEvents->Parse(parser))
			return FALSE;
	}

	while (parser.Matched(T_TABLE))
	{
		m_bDispActionFound	= FALSE;
		m_bOnTableAction	= TRUE;
		
		BOOL bUnnamed;
		DisplayTableEntry* pDispTable = m_SymTable.MatchDisplayTable(parser, bUnnamed);
		if (pDispTable == NULL) 
		{
			ASSERT(FALSE);
			return FALSE;
		}
		if (!pDispTable->IsKindOf(RUNTIME_CLASS(DisplayTableEntryEngine))) 
		{
			ASSERT(FALSE);
			return FALSE;
		}
		DisplayTableEntryEngine* pDisplayTable = (DisplayTableEntryEngine*) pDispTable;

		if (pDisplayTable->TableActions() != NULL)
			return parser.SetError(_TB("Action already defined for the table"), pDisplayTable->GetTableName());
	
		if (!parser.ParseTag(T_COLON) || !parser.ParseTag(T_DO)) 
			return FALSE;
	
		pDisplayTable->TableActions() = new FillTableEvents(*this, pDisplayTable->GetTableName());
		if (!pDisplayTable->TableActions()->ParseEventActions(parser)) 
			return FALSE;

		m_bOnTableAction	= FALSE;	
	}

	return !parser.ErrorFound();
}

//---------------------------------------------------------------------------
BOOL RepEngine::ParseTriggeredEvents(Parser& parser)
{
	m_bDispActionFound = FALSE;
	CString sName;
	while (parser.LookAhead(T_ID))   
	{
		if (!parser.ParseID(sName) || !parser.ParseTag(T_COLON)) 
			return FALSE;

		TriggeredEvent* pTrigEvent  = new TriggeredEvent(*this, sName);
		m_TriggeredEvents.Add(pTrigEvent);

		if (!pTrigEvent->Parse(parser)) 
			return FALSE;
	}

	return !parser.ErrorFound();
}

//---------------------------------------------------------------------------
BOOL RepEngine::ParseEvents(Parser& parser)
{
	if (parser.Matched(T_EVENTS))
		if 
			(
				ParseStdActions(parser) &&
				ParseTriggeredEvents(parser)
			)
			return parser.ParseEnd();  // EVENTS END
		else
			return FALSE;
	else
		return !parser.ErrorFound();
}

//---------------------------------------------------------------------------
BOOL RepEngine::MakeAutoDisplayActions(Parser& parser)
{
	// insert eventually display field actions
	DisplayFieldsAction* pAlwaysDFA = NULL;
	DisplayFieldsAction* pOnFormFeedDFA = NULL;
	DisplayFieldsAction* pOnEndOfReportDFA = NULL;

	int i = 0;
	for (i = m_nItemStart; i < m_SymTable.GetSize(); i++)
	{
		if (!((SymField*) m_SymTable[i])->IsKindOf(RUNTIME_CLASS(WoormField))) 
			continue;
 
		WoormField* pRepField = (WoormField*) m_SymTable[i];
		
		if (pRepField->IsDisplayed()	||
			pRepField->IsHidden()		||
			pRepField->IsSubTotal()		) 
			continue;
		
		if (pRepField->GetId() >= SpecialReportField::REPORT_LOWER_SPECIAL_ID) 
			continue;
													   
		// ColTotals have column attribute!!
		if (!pRepField->IsColumn() || pRepField->IsColTotal())
		{
			// build Report event if it don't exist yet
			if (m_pReportEvents == NULL)
				m_pReportEvents = new ReportEvents(*this);

			// build Form Feed event if it don't exist yet
			if (m_pOnFormFeedEvents == NULL)
				m_pOnFormFeedEvents    = new FormFeedEvents(*this);

			// add actions as below:
			//
			//	FormFeed : Do
			//		Before Begin
			//			Display field_not_col_1, field_not_col_2, .. field_not_col_N;
			//			... USER ACTIONS ...
			//		End
			//
			//	Report : Do
			//		After Begin
			//			... USER ACTIONS ...
			//			Display field_not_col_1, field_not_col_2, .. field_not_col_N;
			//		End
			//
			if (pOnFormFeedDFA == NULL && pOnEndOfReportDFA == NULL)
			{
				// insert as first action the automatic field display actions
				//
				pOnFormFeedDFA = new DisplayFieldsAction(NULL, NULL, this);
				m_pOnFormFeedEvents->GetBeforeActions().InsertActionAt(0, pOnFormFeedDFA);

				// insert as last action the automatic field display actions
				//
				pOnEndOfReportDFA = new DisplayFieldsAction(NULL, NULL, this);
				m_pReportEvents->GetAfterActions().AddAction(pOnEndOfReportDFA);
			}

			pOnFormFeedDFA->AddField(pRepField);
			pOnEndOfReportDFA->AddField(pRepField);
		}
		else
		{
			if (m_bRepDispActionFound)
				// display actions in Report_Event->Always_Section explicity inserted!
				return parser.SetError(EngineScheduler::FormatMessage(EngineScheduler::REPORT_AUTO_DISPLAY), pRepField->GetName());
	
			// build Report event if it don't exist yet
			if (m_pReportEvents == NULL)
				m_pReportEvents = new ReportEvents(*this);
	
			// add actions as below:
			//
			//	Report : Do
			//		Always Display field_1, field_2, .. field_N;
			//
			if (pAlwaysDFA == NULL)
			{
				pAlwaysDFA = new DisplayFieldsAction(NULL, NULL, this);
				// insert as first action the automatic field display actions
				m_pReportEvents->GetAlwaysActions().InsertActionAt(0, pAlwaysDFA);
			}
	
			pAlwaysDFA->AddField(pRepField);
		}

		pRepField->SetDisplayed();
	}

	// add a NEXTLINE action for evry Display Table that has a field automatically diaplayed
	// and eventually insert a table overflow FORMFEED action
	//
	int numTable = m_SymTable.GetDisplayTablesNum();

	if	(
			numTable == 0 &&
			(m_pReportEvents == NULL || m_pReportEvents->GetAlwaysActions().IsEmptyCommands()) &&
			m_TriggeredEvents.GetSize() == 0
		)
	{
		m_pAutoFormFeed = new FormFeedAction(NULL, NULL, this);
		return TRUE;
	}

	if (numTable) for (i = 0; i < m_SymTable.GetDisplayTables()->m_DispTables.GetSize(); i++)
	{
		DisplayTableEntryEngine* pDisplayTable = (DisplayTableEntryEngine*) m_SymTable.GetDisplayTables()->m_DispTables[i];

		if (pAlwaysDFA && pAlwaysDFA->ExistColumnOf(pDisplayTable))
		{
			DisplayTableAction* pDisplayTableAction;
			pDisplayTableAction = new DisplayTableAction(NULL, this, RDEManager::NEXT_LINE, pDisplayTable);
			m_pReportEvents->GetAlwaysActions().AddAction(pDisplayTableAction);
		}

		if (pDisplayTable->TableActions() == NULL)
			pDisplayTable->TableActions() = new FillTableEvents(*this, pDisplayTable->GetTableName());

		// at least a FormFeedAction on table overflow (BEFORE actions) must exists
		if (pDisplayTable->TableActions()->GetBeforeActions().IsEmptyCommands())
		{
			m_bOnTableAction	= TRUE;

			pDisplayTable->TableActions()->GetBeforeActions().AddAction(new FormFeedAction(NULL, NULL, this));

			m_bOnTableAction	= FALSE;
		}
	}

	return TRUE;
}

//---------------------------------------------------------------------------
BOOL RepEngine::ParseProcedures(Parser& parser)
{
	m_bOnFormFeedAction	= FALSE;
	m_bOnTableAction	= FALSE;

	BOOL bHaveProcedures = parser.Matched(T_PROCEDURES);
	if (bHaveProcedures && !parser.Match(T_BEGIN)) return FALSE;
	
	CString		strProcName;
	ProcedureObjItem*  pProcObj;
	while (parser.Matched(T_PROCEDURE))
	{
		if (!parser.ParseID(strProcName)) return FALSE;

		pProcObj = m_SymTable.FindProcedure(strProcName);
		if (pProcObj == NULL)
		{
			pProcObj = new ProcedureObjItem(this, strProcName);

			m_SymTable.AddProcedure(pProcObj);
		}
		else
			if (!pProcObj->IsEmpty())
				return parser.SetError(_TB("Procedure already defined"), strProcName);

		if (!pProcObj->Parse(parser)) 
			return FALSE;
	}

	if (bHaveProcedures && !parser.Match(T_END)) 
		return FALSE;
	return !parser.ErrorFound();
}

//---------------------------------------------------------------------------
BOOL RepEngine::ParseQueries(Parser& parser, CTBContext* pTBContext)
{
	BOOL bHaveQueries = parser.Matched(T_QUERIES);
	if (bHaveQueries && !parser.Match(T_BEGIN)) return FALSE;
	QueryObjItem*  pQuery;

	while (parser.LookAhead(T_QUERY))
	{
		pQuery = new QueryObjItem(&m_SymTable, L"", pTBContext);

		if (!pQuery->Parse(parser))
		{
			delete pQuery;
			break;
		}

		if (m_SymTable.FindQuery(pQuery->GetName()) != NULL)
		{
			parser.SetError(_TB("Query already defined"), pQuery->GetName());
			delete pQuery;
			break;
		}

		m_SymTable.AddQuery(pQuery);
	}

	if (bHaveQueries && !parser.Match(T_END))
	{
		parser.SetError(_TB("Missing END token"));
		return FALSE;
	}

	return TRUE; //!parser.ErrorFound();
}

//---------------------------------------------------------------------------
BOOL RepEngine::Parse(Parser& parser, CTBContext* pTBContext)
{
	m_bRepDispActionFound	= FALSE;
	m_bDispActionFound		= FALSE;
	m_bOnFormFeedAction		= FALSE;
	m_bOnTableAction		= FALSE;
	m_nItemStart			= m_SymTable.GetSize();
	
	CString sName;
	if (!parser.ParseID(sName))
		return FALSE;

	SetName(sName);
		
	if (!m_SymTable.Parse(parser))
		return FALSE;
			
	if (!QueryEngine::Compile(parser, pTBContext))
		return FALSE;
	
	if	(
			!ParseEvents		(parser)				||
			!ParseProcedures	(parser)				||
			!ParseQueries		(parser, pTBContext)	||
			!ParseAskingRules	(parser)				||
			!parser.ParseEnd	()
		)
		return FALSE;

	CString sErr;
	if (!ResolveQueries(sErr))
	{
		parser.SetError(sErr);
		return FALSE;
	}

	if (!QueryEngine::BuildTree(parser))
		return FALSE;

	return MakeAutoDisplayActions(parser);
}

