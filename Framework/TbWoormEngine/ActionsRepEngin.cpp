#include "stdafx.h"

#include <TBGenlib\baseapp.h>

#include <TbParser\Parser.h>
#include <TbGenlib\expparse.h>

#include "rpsymtbl.h"
#include "events.h"
#include "edtcmm.h"
#include "ExportSymbols.h"
#include "repfield.h"
#include "repengin.h"
#include "MultiLayout.h"
#include "procdata.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif

//===========================================================================
//              Class Block implementation
//===========================================================================
IMPLEMENT_DYNAMIC(ActionObj, CObject)

ActionObj::ActionObj (ActionObj* pParent, SymTable* pSymTable, RepEngine* pEngine, CFunctionDescription* pFun/* = NULL*/)
	:
	IDisposingSourceImpl(this),

	m_pRepEngine		(pEngine),
	m_pSymTable			(pSymTable),
	m_pFun				(pFun),

	m_ActionType		(ACT_NONE),
	m_ActionState		(STATE_NORMAL),

	m_pParent (pParent)
{
	ASSERT(pSymTable || pEngine);
	if (pSymTable) ASSERT_VALID(pSymTable);
	if (pEngine) ASSERT_VALID(pEngine);

	if (!pSymTable && pEngine)
		m_pSymTable = &pEngine->GetSymTable();

	ASSERT(!pEngine || m_pSymTable->IsKindOf(RUNTIME_CLASS(WoormTable)));
}

//---------------------------------------------------------------------------
BOOL ActionObj::Fail (LPCTSTR szError /*= NULL*/ )
{
	SetRTError(EngineScheduler::REPORT_ACTION_ERR, szError);
	return FALSE;
}

//---------------------------------------------------------------------------
BOOL ActionObj::SetRTError (int MessageID, const CString& err, LPCTSTR szErr)
{
	m_ActionState = STATE_ABORT;

	if (GetRepEngine())
		GetRepEngine()->SetRTError
								(
									(EngineScheduler::MessageID)MessageID,
									err,
									szErr
							   );
	return FALSE;
}

//---------------------------------------------------------------------------
Block* ActionObj::GetBlockParent() const 
{ return dynamic_cast<Block*>(GetRootParent()); }

//---------------------------------------------------------------------------
BOOL ActionObj::CheckBreakpoint()
{
	if (!GetRepEngine())
		return TRUE;

	CBreakpoint* pB = GetBreakpoint();
	if (!pB) 
		return TRUE;
	ASSERT_VALID(pB);

	if (pB->m_bEnabled)
	{
		BOOL bShowDebug = TRUE;

		if (pB->m_erprCondition && !pB->m_erprCondition->IsEmpty())
		{
			DataBool bHit(FALSE);
			BOOL bOk = pB->m_erprCondition->Eval(bHit);

			bShowDebug = bHit && bOk;
		}

		if (bShowDebug)
		{
			pB->m_nHitCount++;

			if ((pB->m_nActivateAfterHitCount - pB->m_nHitCount) > 0)
			{
				bShowDebug = FALSE;
			}
		}

		if (bShowDebug && pB->m_erprAction  && !pB->m_erprAction->IsEmpty())
		{
			DataStr sTrace;

			BOOL bOk = GetBreakpoint()->m_erprAction->Eval(sTrace);

			if (GetRepEngine() && !sTrace.IsEmpty() && bOk)
				GetRepEngine()->m_arTraceActions.Add(sTrace);

		}
		if (pB->m_bContinueExecution)
		{
			bShowDebug = FALSE;
		}

		if (bShowDebug)
		{
			if (GetRepEngine() && GetRepEngine()->GetCallerDoc())
			{
				if (GetRepEngine()->GetCallerDoc()->IsInUnattendedMode())
				{
					return SetRTError(EngineScheduler::REPORT_UNATTENDED);
				}

				GetRepEngine()->GetCallerDoc()->OpenDebugger(this);
			}
		}
	}
	return TRUE;
}

//---------------------------------------------------------------------------
ActionObj* ActionObj::GetRootParent() const
{
	if (m_pParent == NULL)
		return const_cast<ActionObj*>(this);

	return m_pParent->GetRootParent();
}

//---------------------------------------------------------------------------
ActionObj* ActionObj::GetNextAction() const
{
	if (m_pParent == NULL)
		return NULL;

	Block* pBlock = dynamic_cast<Block*>(m_pParent);
	if (pBlock)
	{
		int idx = pBlock->m_Actions.FindPtr(const_cast<ActionObj*>(this));
		if (idx < pBlock->m_Actions.GetUpperBound())
			return (ActionObj*) pBlock->m_Actions[idx + 1];
	}

		return m_pParent->GetNextAction();
}

//---------------------------------------------------------------------------
CString	ActionObj::Unparse()
{
	Unparser buff(TRUE);
	Unparse(buff, FALSE, TRUE);
	buff.Close();

	CString sBlock = buff.GetBufferString();

	if (!sBlock.IsEmpty())
		ConvertCString(sBlock, LF_TO_CRLF);

	return sBlock;
}

/////////////////////////////////////////////////////////////////////////////
// Diagnostics
#ifdef _DEBUG
void ActionObj::Dump(CDumpContext& dc) const
{
	ASSERT_VALID(this);
	AFX_DUMP1(dc, "\n\t(Tb)ActionObj = ", CString(this->GetRuntimeClass()->m_lpszClassName) + _T(", ") + cwsprintf(L"%d", this->m_ActionType));
}
#endif //_DEBUG

//===========================================================================
//              Class Block implementation
//===========================================================================
IMPLEMENT_DYNAMIC(Block, ActionObj)

//---------------------------------------------------------------------------
Block::Block (ActionObj* pParent, SymTable* pTable, RepEngine* pEngine, BOOL bRaiseEvents /*= TRUE*/, CFunctionDescription* pFun /*= NULL*/)
	:
	ActionObj			(pParent, pTable, pEngine, pFun),
	m_bRaiseEvents		(bRaiseEvents),
	m_bHasBeginEnd		(FALSE),
	m_pLocalSymTable	(NULL)
{
	m_nIndex = 0;
}

//---------------------------------------------------------------------------
Block::~Block ()
{
	if (m_pLocalSymTable)
	{
		m_pLocalSymTable->DeleteMeAsLocalScope();
		m_pLocalSymTable = NULL;
	}
}

//---------------------------------------------------------------------------
void Block::Empty()
{
	if (m_pLocalSymTable)
	{
		m_pLocalSymTable->DeleteMeAsLocalScope();
		m_pLocalSymTable = NULL;
	}
	m_Actions.RemoveAll();
}

//---------------------------------------------------------------------------
void Block::AddLocalField (WoormField* pLocal)
{
	if (!m_pLocalSymTable)
	{
		m_pLocalSymTable = GetSymTable()->CreateLocalScope();
	}
	m_pLocalSymTable->Add(pLocal);
}

//---------------------------------------------------------------------------
void Block::SetFun(CFunctionDescription* pF)
{  
	m_pFun = pF; 	
	if (pF) for (int i = 0; i < pF->GetParameters().GetSize(); i++)
	{
		CDataObjDescription* param = (CDataObjDescription*)(pF->GetParameters().GetAt(i));
		SymField* f = NULL;

		if (f = GetSymTable()->GetField(param->GetName()))
		{
			ASSERT(f->IsInput());
			if (param->GetDataType() == DataType::Array)
				ASSERT(f->IsArray() && f->GetDataType() == param->GetBaseDataType());
			else
				ASSERT(f->GetDataType() == param->GetDataType());
			continue;
		}

		WoormField* field = new WoormField(param->GetName(), WoormField::FIELD_INPUT);

		if (param->GetDataType() == DataType::Array)
			field->SetDataType(param->GetBaseDataType(), true);
		else
			field->SetDataType(param->GetDataType());

		field->SetDataPtr(param->GetValue(), false);

		AddLocalField(field);
	}
}

//---------------------------------------------------------------------------
BOOL Block::Exec()
{
	m_ActionState = STATE_NORMAL;
	CheckBreakpoint();

	for (int i = 0; i < m_Actions.GetSize(); i++)
	{
		ActionObj* pCmd = (ActionObj*) m_Actions[i];

		pCmd->CheckBreakpoint();

		if (!pCmd->Exec())
		{
			return FALSE;
		}

		if (pCmd->GetActionState() == STATE_ABORT)
		{
			return FALSE;
		}

		if (pCmd->GetActionState() == STATE_QUIT)
		{
			return FALSE;
		}
		
		if (GetRepEngine () && GetRepEngine ()->UserBreak())
		{
			m_ActionState = STATE_ABORT;
			return FALSE;
		}

		if (!pCmd->CanRun())
		{
			m_ActionState = pCmd->GetActionState();
			break;
		}
	}

	return TRUE;
}

//---------------------------------------------------------------------------
ActionObj* Block::ParseAction(Parser& parser)
{
	if (parser.ErrorFound()) 
		return NULL;

	ActionObj* pActionObj = NULL;

	Token actionToken = parser.LookAhead();

	CStringArray commentsBefore;
	parser.GetCommentTrace(commentsBefore);

	switch (actionToken)
	{
		case T_ID    :
		{
			pActionObj = new AssignAction(this, GetSymTable(), GetRepEngine ());
			break;
		}

		case T_IF :
		{
			pActionObj = new ConditionalAction(this, GetSymTable(), GetRepEngine (), GetFun());
			break;
		}

		case T_WHILE :
		{
			pActionObj = new WhileLoopAction(this, GetSymTable(), GetRepEngine (), GetFun());
			break;
		}

		case T_EVAL  :
		case T_RESET :
		{
			pActionObj = new EvalResetAction(this, GetWoormTable(), GetRepEngine ());
			break;
		}

		case T_DISPLAY :
		case T_DISPLAY_TABLE_ROW :
		case T_DISPLAY_FREE_FIELDS :
		{
			if (!m_bRaiseEvents)
			{
				parser.SetError(_TB("Action illegal in this context"));
				return NULL;
			}

			pActionObj = new DisplayFieldsAction(this, GetWoormTable(), GetRepEngine ());
			break;
		}

		case T_DISPLAY_CHART:
		{
			if (!m_bRaiseEvents)
			{
				parser.SetError(_TB("Action illegal in this context"));
				return NULL;
			}

			pActionObj = new DisplayChartAction(this, GetWoormTable(), GetRepEngine());
			break;
		}

		case T_FORMFEED :
		{
			if (!m_bRaiseEvents)
			{
				parser.SetError(_TB("Action illegal in this context: cannot raise events"));
				return NULL;
			}

			pActionObj = new FormFeedAction(this, GetWoormTable(), GetRepEngine ());
			break;
		}

		case T_INTERLINE    : 
		case T_SPACELINE    : 
		case T_NEXTLINE     : 
		case T_TITLELINE	:
		case T_SUBTITLELINE:
		{
			if (!m_bRaiseEvents)
			{
				parser.SetError(_TB("Action illegal in this context"));
				return NULL;
			}

			pActionObj = new DisplayTableAction(this, GetWoormTable(), GetRepEngine ());
			break;
		}

		case T_CALL :
		{
			pActionObj = new CallAction(this, GetWoormTable(), GetRepEngine ());
			break;
		}

		case T_ASK:
		{
			if (!m_bRaiseEvents)
			{
				parser.SetError(_TB("Action illegal in this context"));
				return NULL;
			}

			pActionObj = new AskDialogAction(this, GetWoormTable(), GetRepEngine ());
			break;
		}

		case T_ABORT:
			if (!m_bRaiseEvents)
			{
				parser.SetError(_TB("Action illegal in this context"));
				return NULL;
			}
			//MANCA il BREAK VOLUTAMENTE!
		case T_MESSAGE_BOX:
		case T_DEBUG:
			{

				pActionObj = new MessageBoxAction(this, GetSymTable(), GetRepEngine ());
				break;
			}

		case T_QUIT:
		case T_BREAK :
		case T_CONTINUE :
		{
			pActionObj = new QuitBreakContinueAction(this, GetSymTable(), GetRepEngine ());
			break;
		}

		case T_RETURN :
		{
			pActionObj = new ReturnAction(this, GetSymTable(), GetRepEngine (), m_pFun/*GetFun()*/);
			break;
		}

		case T_DO :
		{
			pActionObj = new DoExprAction(this, GetSymTable(), GetRepEngine ());
			break;
		}

		default :
		{
			pActionObj = new DeclareAction(this, GetSymTable(), GetRepEngine (), this);
			break;
		}
	}

	if (pActionObj->Parse(parser))
	{
		//BEFORE
		pActionObj->m_arCommentTraceBefore.Append(commentsBefore);
		//AFTER
		parser.GetCommentTrace(pActionObj->m_arCommentTraceAfter);

		return pActionObj;
	}

	// exit with error
	SAFE_DELETE (pActionObj);
	return NULL;
}

//---------------------------------------------------------------------------
BOOL Block::Parse(Parser& parser)
{
	parser.GetCommentTrace(this->m_arCommentTraceBefore);

	ActionObj* pActionObj;

	if (parser.Matched(T_BEGIN))
	{   
		// also accepts empty "begin..end" block sections
		while (!parser.Matched(T_END))
		{
			if (parser.ErrorFound() || ((pActionObj = ParseAction(parser)) == NULL))
				return FALSE;
	
			AddAction(pActionObj);
/* TODO provoca cambio comportamento nei report esistenti
			if (pActionObj->GetActionType() == ActionType::ACT_SPACELINE)
			{	
				DisplayTableAction* pSpaceLine = dynamic_cast<DisplayTableAction*>(pActionObj);

				DisplayTableAction* pNextLine = new DisplayTableAction(this, GetWoormTable(), GetRepEngine());
				pNextLine->m_bAuto = TRUE;
				pNextLine->m_ActionType		= ActionType::ACT_NEXTLINE;

				pNextLine->m_RdeCommand		= pSpaceLine->m_RdeCommand;
				pNextLine->m_sTableName		= pSpaceLine->m_sTableName;
				pNextLine->m_pDisplayTable	= pSpaceLine->m_pDisplayTable;

				AddAction(pNextLine);
			}
*/
		}

		m_bHasBeginEnd = TRUE;
	}
	else
	{       
		// single action
		if (parser.ErrorFound() || ((pActionObj = ParseAction(parser)) == NULL))
			 return FALSE;
		
		AddAction(pActionObj);

		m_bHasBeginEnd = FALSE;
	}

	parser.GetCommentTrace(this->m_arCommentTraceAfter);

	return TRUE;
}

//----------------------------------------------------------------------------
BOOL Block::Parse(CString strBlock)
{
	ConvertCString(strBlock, CRLF_TO_LF);
	Parser lex(strBlock);

	return Parse(lex);
}

//----------------------------------------------------------------------------
void Block::AddAction(ActionType actionType, LPCTSTR pszString, int nPos)
{
	ActionObj* pActionObj = NULL;
	switch (actionType)
	{
		case ACT_DISPLAY:
			pActionObj = new DisplayFieldsAction(this, GetWoormTable(), GetRepEngine ());

			if (pszString && *pszString)
				((DisplayFieldsAction*)pActionObj)->AddDisplayName(pszString);
			break;

		case ACT_EVAL:
		case ACT_RESET:
			pActionObj = new EvalResetAction(this, GetWoormTable(), GetRepEngine ());
			pActionObj->SetActionType(actionType);

			if (pszString && *pszString)
				((EvalResetAction*)pActionObj)->SetFieldName(pszString);
			break;

		case ACT_NEXTLINE:
			pActionObj = new DisplayTableAction(this, GetWoormTable(), GetRepEngine ());
			pActionObj->SetActionType(ACT_NEXTLINE);

			//Controllo interno alla SetTableName per gestire anonymous //if (pszString && *pszString)
				((DisplayTableAction*)pActionObj)->SetTableName(pszString);
			break;

		case ACT_SPACELINE:
			pActionObj = new DisplayTableAction(this, GetWoormTable(), GetRepEngine());
			pActionObj->SetActionType(ACT_SPACELINE);

			//Controllo interno alla SetTableName per gestire anonymous //if (pszString && *pszString)
			((DisplayTableAction*)pActionObj)->SetTableName(pszString);
			break;


		default: 
			ASSERT(FALSE);	
			return;
	}

	if (nPos == -1)
		pActionObj->m_nIndex = m_Actions.Add(pActionObj);
	else
	{
		m_Actions.InsertAt(nPos, pActionObj);
		pActionObj->m_nIndex = nPos;
	}
}

//----------------------------------------------------------------------------
int Block::GetIdxAction(ActionType actionType) const
{
	int nNumActions = m_Actions.GetSize();
	for (int i = 0; i < nNumActions; i++)
	{
		ActionObj* pActionData = (ActionObj*) m_Actions[i];

		if (pActionData->GetActionType() == actionType)
			return i;
	}
    return -1; // not found
}

//----------------------------------------------------------------------------
ActionObj* Block::GetAction(int idx) const
{
	if (idx < 0 || idx >= GetCount())
	{
		ASSERT(FALSE);
		return NULL;
	}

	ActionObj* pActionData = (ActionObj*)m_Actions[idx];
	return pActionData;
}

//----------------------------------------------------------------------------
void Block::DispTableChanged(LPCTSTR pszOldName, LPCTSTR pszNewName)
{
	for (int i = 0; i < m_Actions.GetSize(); i++)
	{
		ActionObj* pActionData = (ActionObj*) m_Actions[i];

		pActionData->DispTableChanged(pszOldName, pszNewName);
	}
}

//----------------------------------------------------------------------------
void Block::GetSubtotalFields(CStringArray& arraySubtotals)
{
	for (int i = 0; i < m_Actions.GetSize(); i++)
	{
		ActionObj* pActionData = (ActionObj*) m_Actions[i];

		pActionData->GetSubtotalFields(arraySubtotals);
	}
}

//----------------------------------------------------------------------------
BOOL Block::CanDeleteField(LPCTSTR pszFieldName)
{
	for (int i = 0; i < m_Actions.GetSize(); i++)
	{
		ActionObj* pActionData = (ActionObj*) m_Actions[i];

		if (!pActionData->CanDeleteField(pszFieldName))
        	return FALSE;
	}
	return TRUE;
}

//----------------------------------------------------------------------------
void Block::DeleteField(LPCTSTR pszFieldName)
{
 	for (int nIdx = m_Actions.GetUpperBound(); nIdx >= 0; nIdx--)
	{
		ActionObj* pActionData = (ActionObj*) m_Actions[nIdx];

		pActionData->DeleteField(pszFieldName);

		if (pActionData->IsEmpty())	
			m_Actions.RemoveAt(nIdx);        	
	}
}

//----------------------------------------------------------------------------
void Block::DeleteTable(LPCTSTR pszDispTableName)
{
 	for (int nIdx = m_Actions.GetUpperBound(); nIdx >= 0; nIdx--)
	{
		ActionObj* pActionData = (ActionObj*) m_Actions[nIdx];

		pActionData->DeleteTable(pszDispTableName);

		if (pActionData->IsEmpty())	
			m_Actions.RemoveAt(nIdx);        	
	}
}

//----------------------------------------------------------------------------
BOOL Block::SearchNamedAction(LPCTSTR pszName, ActionType anAction)
{
	for (int nIdx = 0; nIdx < m_Actions.GetSize(); nIdx++)
	{
		ActionObj* pActionData = (ActionObj*) m_Actions[nIdx];
		
		if	(
				(pActionData->GetActionType() == anAction) && 
				(_tcsicmp(pszName, pActionData->GetAssociatedName()) == 0)
			)
			return TRUE;
	}
	return FALSE;
}

//----------------------------------------------------------------------------
void Block::DeleteNamedAction(LPCTSTR pszName, ActionType anAction)
{
	for (int nIdx = m_Actions.GetUpperBound(); nIdx >= 0 ; nIdx--)
	{
		ActionObj* pActionData = (ActionObj*) m_Actions[nIdx];
		CString str = pActionData->GetAssociatedName();
		if	(
				(pActionData->GetActionType() == anAction) && 
				(_tcsicmp(pszName, pActionData->GetAssociatedName()) == 0)
			)
			m_Actions.RemoveAt(nIdx); 
	}
}

//----------------------------------------------------------------------------
void Block::RenameNamedAction(LPCTSTR pszOldName, LPCTSTR pszNewName, ActionType anAction)
{
	CWordArray	nIdxToDelete;
    int			nIdx;

	for (nIdx = 0; nIdx < m_Actions.GetSize(); nIdx++)
	{
		ActionObj* pActionData = (ActionObj*) m_Actions[nIdx];
		
		if	(
				(pActionData->GetActionType() == anAction) && 
				(_tcsicmp(pszOldName, pActionData->GetAssociatedName()) == 0)
			)
			pActionData->Rename(pszOldName, pszNewName);
	}
}

//----------------------------------------------------------------------------
void Block::Unparse(Unparser& oFile, BOOL bNewLine, BOOL bSkipBeginEnd)
{
	oFile.UnparseComment(this->m_arCommentTraceBefore);

	m_nDebugUnparseRow = oFile.GetLine();
	if (!bSkipBeginEnd)
	{
		// to accept empty begin-end blocks
		if (m_bHasBeginEnd || m_Actions.GetSize() > 1)
		{
			oFile.UnparseCrLf();
			m_nDebugUnparseRow = oFile.GetLine();
			oFile.UnparseBegin(TRUE);
		}
	}
	//TRACE("block line %d\n", m_nDebugUnparseRow);

	for (int i=0; i < m_Actions.GetSize(); i++)
	{
		ActionObj* pActionData = (ActionObj*) m_Actions[i];
		if (pActionData->GetActionType() != ActionType::ACT_NONE && !pActionData->m_bAuto)
		{
			pActionData ->m_nDebugUnparseRow = oFile.GetLine();
			//TRACE("action line %d\n", pActionData->m_nDebugUnparseRow);
			pActionData->Unparse(oFile);
		}
    }

	if (!bSkipBeginEnd)
	{
		if (m_bHasBeginEnd || m_Actions.GetSize() > 1)
		{
			oFile.UnparseEnd(bNewLine);
		}
	}

	oFile.UnparseComment(this->m_arCommentTraceAfter);
}

//----------------------------------------------------------------------------

//===========================================================================
//              Class BoolBlock implementation
//===========================================================================
IMPLEMENT_DYNAMIC(BoolBlock, Block)

BoolBlock::BoolBlock (ActionObj* pParent, SymTable* pTable)
	:
	Block (pParent, pTable, NULL, FALSE, &m_Fun)
{
	m_Fun.SetReturnValueDescription(CDataObjDescription(L"", new DataBool(), TRUE));
}

//---------------------------------------------------------------------------
BOOL BoolBlock::Exec (DataBool& bRes)
{
	BOOL bOk = __super::Exec();
	if (!bOk)
		return FALSE;
	bRes = *dynamic_cast<DataBool*>(m_Fun.GetReturnValue());
	return TRUE;
}

//===========================================================================
//              Class ConputationalAction implementation
//===========================================================================
IMPLEMENT_DYNAMIC(EvalResetAction, ActionObj)

EvalResetAction::EvalResetAction (ActionObj* pParent, WoormTable* pTable, RepEngine* pEngine)
	:
	ActionObj       (pParent, pTable, pEngine),

	m_pField    	(NULL)
{
	m_ActionType = ACT_RESET;
}

//---------------------------------------------------------------------------
BOOL EvalResetAction::Exec()
{
	if (m_pField == NULL)
		return FALSE;

	m_ActionState = STATE_NORMAL;

	switch (m_ActionType)
	{
		case ACT_EVAL :
		{
			EventFunction* aFun = m_pField->GetEventFunction();
			if (aFun != NULL && !aFun->Eval())
				return
					SetRTError
							(
								EngineScheduler::REPORT_EVAL_EVENT_FUNC,
								aFun->GetErrDescription(),
								m_pField->GetName()
						   );
			if(aFun == NULL)
				SetRTError
				(
					EngineScheduler::REPORT_EVAL_EVENT_FUNC,
					_TB("The function is empty"),
					m_pField->GetName()
				);
			break;
		}
		case ACT_RESET :
		{
			Expression::MessageID err = m_pField->Init();
			if (err)
				return SetRTError
							(
								EngineScheduler::REPORT_EVAL_INIT_EXPR,
								Expression::FormatMessage (err),
								m_pField->GetName()
						   );
			break;
		}
		default : 
			return SetRTError(EngineScheduler::REPORT_UNKNOWN_ACTION);
	}
	return TRUE;
}

//---------------------------------------------------------------------------
BOOL EvalResetAction::Parse(Parser& parser)
{
	Token tk = parser.SkipToken();
	m_ActionType = (tk == T_EVAL ? ACT_EVAL : ACT_RESET);

	CString strFieldName;
	if (!parser.ParseID(strFieldName)) 
		return FALSE;

	if (!SetFieldName(strFieldName, &parser))
		return FALSE;

	return parser.ParseSep();
}

//---------------------------------------------------------------------------
BOOL EvalResetAction::SetFieldName(LPCTSTR strFieldName, Parser* parser)
{ 
	ASSERT(!m_pField);

	/*BOOL bEditing = TRUE;
	Block* parentBlock = GetBlockParent();
	if (parentBlock != NULL)
		bEditing = parentBlock->m_bOnEditing;*/

	m_pField = (WoormField*) GetSymTable()->GetField(strFieldName);
	if (m_pField == NULL)
		return parser ? parser->SetError(Expression::FormatMessage(Expression::UNKNOWN_FIELD), strFieldName) : FALSE;

	if ((m_pField->GetEventFunction() == NULL || m_pField->GetEventFunction()->ToString().IsEmpty()) && (m_ActionType == ACT_EVAL))
	{
		//m_ActionType = ACT_NONE;
		//m_pField = NULL;
		//if(bEditing)
		//An.EVAL FUNCTION - MessageBox di errore e apertura EditView  
		return parser ? parser->SetError(_TB("Function not defined for the field"), strFieldName) : FALSE;
		
		
	}
		
	return TRUE;
}

//----------------------------------------------------------------------------
void EvalResetAction::Unparse(Unparser& oFile, BOOL /*bNewLine = TRUE*/, BOOL /*bSkipBeginEnd = FALSE*/)
{
	oFile.UnparseComment(this->m_arCommentTraceBefore);

	switch (m_ActionType)
	{
		case ACT_NONE:
			oFile.UnparseComment(this->m_arCommentTraceAfter);
			return;
			
		case ACT_EVAL:
			ASSERT(m_pField);
			oFile.UnparseTag (T_EVAL, FALSE);
 			oFile.UnparseID	(m_pField->GetName(), FALSE);
			break;

		case ACT_RESET:
			ASSERT(m_pField);

			oFile.UnparseTag (T_RESET, FALSE);
 			oFile.UnparseID	(m_pField->GetName(), FALSE);
			break;

		default:
			ASSERT(FALSE);
	}
	oFile.UnparseSep(TRUE);
	oFile.UnparseComment(this->m_arCommentTraceAfter);
}

//----------------------------------------------------------------------------
void EvalResetAction::DeleteField(LPCTSTR pszFieldName)
{
	if (!m_pField)
		return;

	if (m_pField->GetName().CompareNoCase(pszFieldName) == 0)
	{
        m_pField = NULL;
		m_ActionType = ACT_NONE;
	}    	
}

//===========================================================================
//              Class AssignAction implementation
//===========================================================================
IMPLEMENT_DYNAMIC(AssignAction, ActionObj)

AssignAction::AssignAction (ActionObj* pParent, SymTable* pTable, RepEngine* pEngine)
	:
	ActionObj       (pParent, pTable, pEngine),

	m_pField    	(NULL),
	m_pExpr			(NULL),
	m_pIndexerExpr	(NULL)
{
	m_ActionType = ACT_ASSIGN;
}

//---------------------------------------------------------------------------
AssignAction::~AssignAction ()
{
  SAFE_DELETE(m_pExpr);
  SAFE_DELETE(m_pIndexerExpr);
}

//---------------------------------------------------------------------------
BOOL AssignAction::Exec()
{
	m_ActionState = STATE_NORMAL;

	if (!m_pExpr)
		return FALSE;

	if (m_pIndexerExpr)
	{
		DataLng idx;
		if (!m_pIndexerExpr->Eval(idx))
			return SetRTError
							(
								EngineScheduler::REPORT_EVAL_EVENT_EXPR,
								m_pIndexerExpr->GetErrDescription(),
								m_pField->GetName()
							);
		if (idx < 0)
		{
			return SetRTError
							(
								EngineScheduler::REPORT_EVAL_EVENT_EXPR,
								m_pIndexerExpr->GetErrDescription(),
								m_pField->GetName()
							);
		}

		if (m_pField->GetProvider())
		{
			DataObj* pElem = NULL;
			if (!m_pExpr->Eval(pElem))
				return SetRTError
							(
									EngineScheduler::REPORT_EVAL_EVENT_EXPR,
									m_pExpr->GetErrDescription(),
									m_pField->GetName()
							);
			m_pField->AssignIndexedData(idx, *pElem);
			SAFE_DELETE(pElem);
		}
		else if (m_pField->GetRepData()->IsKindOf(RUNTIME_CLASS(DataArray)))
		{
			DataArray* ar = (DataArray*)(m_pField->GetRepData());

			DataObj* elem = NULL;
			if (idx >= ar->GetSize())
			{
				elem = DataObj::DataObjCreate(ar->GetBaseDataType());
				ar->SetAtGrow(idx, elem);
			}
			else
				elem = ar->GetAt(idx);

			if (elem == NULL)
			{
				elem = DataObj::DataObjCreate(ar->GetBaseDataType());
				ar->SetAtGrow(idx, elem);;
			}

			if (!m_pExpr->Eval(*elem))
				return SetRTError
							(
									EngineScheduler::REPORT_EVAL_EVENT_EXPR,
									m_pExpr->GetErrDescription(),
									m_pField->GetName()
							);

		}
		else if (m_pField->GetRepData()->IsKindOf(RUNTIME_CLASS(DataStr)))
		{
			ASSERT(m_pField->GetRepData()->IsKindOf(RUNTIME_CLASS(DataStr)));
			DataStr* sField = (DataStr*)(m_pField->GetRepData());
			DataStr sVal;
			if (!m_pExpr->Eval(sVal))
				return SetRTError
							(
									EngineScheduler::REPORT_EVAL_EVENT_EXPR,
									m_pExpr->GetErrDescription(),
									m_pField->GetName()
							);
				
			CString newS = sField->GetString();
			idx -= 1; //in woorm le stringhe sono 1-based
			if (sVal.GetLen() > 0)
			{
				TCHAR c = (sVal.GetString().Left(1))[0];
				if (idx < newS.GetLength())
					newS.SetAt(idx, c);
				else
				{
					newS += c;
				}
				*sField = newS;
			}
		}
		else
			return SetRTError
							(
									EngineScheduler::REPORT_EVAL_EVENT_EXPR,
									m_pExpr->GetErrDescription(),
									m_pField->GetName()
							);
	}
	else
	{
		DataObj* pResult = NULL;
		if (!m_pExpr->Eval(pResult))
			return SetRTError
							(
								EngineScheduler::REPORT_EVAL_EVENT_EXPR,
								m_pExpr->GetErrDescription(),
								m_pField->GetName()
							);
		if (!pResult)
			return SetRTError
							(
								EngineScheduler::REPORT_EVAL_EVENT_EXPR,
								m_pExpr->GetErrDescription(),
								m_pField->GetName()
							);
		ASSERT_VALID(pResult);


		if (
				//PROTOTIPI
				m_pField->GetDataType() == DataType::Record &&
				!m_sRecordFieldName.IsEmpty()
			)
		{
			DataObj* pObj = m_pField->GetData();
			ASSERT_VALID(pObj);
			ASSERT_KINDOF(DataSqlRecord, pObj);

			DataSqlRecord* pDR = dynamic_cast<DataSqlRecord*>(pObj);
			ASSERT(pDR->GetIRecord());

			pObj = pDR->GetIRecord()->GetDataObjFromColumnName(this->m_sRecordFieldName);
			if (!pObj)
				return SetRTError
								(
									EngineScheduler::REPORT_EVAL_EVENT_EXPR,
									_TB("Record field name unknown"),
									m_pField->GetName() + '.' + m_sRecordFieldName
								);
			ASSERT_VALID(pObj);

			if (DataType::IsCompatible(pResult->GetDataType(), pObj->GetDataType()))
			{
				pObj->Assign(*pResult);
			}
			else
			{
				CString s(pResult->GetDataType().ToString());
				SAFE_DELETE(pResult);
				return SetRTError
								(
									EngineScheduler::REPORT_EVAL_EVENT_EXPR,
									_TB("Record field name has incompatible data type"),
									m_pField->GetName() + '.' + m_sRecordFieldName + ' ' +
									cwsprintf(_TB("(right value has {0} data type, instead record filed name has {1} data type)"), 
										s, pObj->GetDataType().ToString())
								);
			}

			if (GetRepEngine())
			{
				if (GetRepEngine()->GetEngineStatus() == RepEngine::RE_INIT)
					((WoormField*)m_pField)->InitAllDataLevel();
				else
					((WoormField*)m_pField)->ReportUpdated();
			}
			return TRUE;
		}


		if (GetBlockParent() && GetBlockParent()->IsRuleScope())
		{
			int lev = this->GetSymTable()->GetRoot()->GetDataLevel();
			m_pField->GetData(lev)->Assign(*pResult);

			m_pField->RuleUpdated();
		}
		else
		{
			m_pField->SetData(*pResult);

			SAFE_DELETE(pResult);
		
			if (GetRepEngine())
			{
				if (GetRepEngine ()->GetEngineStatus() == RepEngine::RE_INIT)
					((WoormField*)m_pField)->InitAllDataLevel();
				else
					((WoormField*)m_pField)->ReportUpdated();
			}
		}
	}
	return TRUE;
}

//---------------------------------------------------------------------------
BOOL AssignAction::Parse(Parser& parser)
{
	CString strName;
	if (!parser.ParseID(strName)) 
		return FALSE;

	m_pField = GetSymTable()->GetField(strName);
	if (m_pField == NULL)
	{
		int idx = strName.ReverseFind('.');
		if (idx > 0)
		{
			CString sRecName = strName.Left(idx);
			m_pField = GetSymTable()->GetField(sRecName);
			if (m_pField && m_pField->GetDataType() == DataType::Record)
			{
				this->m_sRecordFieldName = strName.Mid(idx + 1); //field name
				goto l_AssignAction_ok;
			}
		}

		return parser.SetError(Expression::FormatMessage(Expression::UNKNOWN_FIELD), strName);
	}
	this->GetSymTable()->TraceFieldModify(m_pField->GetName());

l_AssignAction_ok:

	if 
		(
			m_pField->GetDataType() == DataType::Array 
			|| 
			(
				parser.LookAhead(T_SQUAREOPEN) 
				&& 
				(
					m_pField->GetDataType() == DataType::String
					||
					m_pField->GetProvider()
				)
			)
		)
	{
		if (parser.Matched(T_SQUAREOPEN))
		{
			m_pIndexerExpr = new Expression(GetSymTable());
			m_pIndexerExpr->SetStopTokens(T_SQUARECLOSE);
			if (!m_pIndexerExpr->Parse(parser, DataType::Long))
			{
				return FALSE;
			}
			if (!parser.ParseTag(T_SQUARECLOSE))
				return parser.SetError(_TB("Computational action on array data type: fails parsing indexer"));
		}
		//else
		//	return parser.SetError(_TB("Computational action on array data type: missing indexer"));
	}

	if (!parser.ParseTag(T_ASSIGN)) 
		return FALSE;

	m_pExpr = new Expression(GetSymTable());
	DataType dt = 
		m_pIndexerExpr &&
		m_pField->GetRepData() &&
		m_pField->GetRepData()->GetDataType() == DataType::Array
		?
		((DataArray*)(m_pField->GetRepData()))->GetBaseDataType() 
		: 
		(m_pField->GetRepData()->GetDataType() == DataType::Record ? DataType::Variant :  m_pField->GetDataType() );

	if (!m_pExpr->Parse(parser, dt))
		return FALSE;
 
	return parser.ParseSep();
}

//----------------------------------------------------------------------------
void AssignAction::Unparse(Unparser& oFile, BOOL /*bNewLine = TRUE*/, BOOL /*bSkipBeginEnd = FALSE*/)
{
	oFile.UnparseComment(this->m_arCommentTraceBefore);

	if (m_pField && m_pExpr)
	{
		CString sName(m_pField->GetName());
		if (!m_sRecordFieldName.IsEmpty())
		{
			sName += ('.' + m_sRecordFieldName);
		}

		oFile.UnparseID		(sName, 	FALSE);

		if (m_pIndexerExpr)
		{
			oFile.UnparseTag	(T_SQUAREOPEN,			FALSE);
			oFile.UnparseExpr	(m_pIndexerExpr->ToString(),		FALSE);
			oFile.UnparseTag	(T_SQUARECLOSE,			FALSE);
		}

		oFile.UnparseTag	(T_ASSIGN,			FALSE);
        oFile.UnparseExpr	(m_pExpr->ToString(),		FALSE);
		oFile.UnparseSep	(TRUE);
	}

	oFile.UnparseComment(this->m_arCommentTraceAfter);
}
//----------------------------------------------------------------------------
BOOL AssignAction::CanDeleteField(LPCTSTR pszFieldName)
{
	if (!m_pField)
		return TRUE;

	if (m_pExpr && m_pExpr->HasMember(pszFieldName))
		return FALSE;
	if (m_pIndexerExpr && m_pIndexerExpr->HasMember(pszFieldName))
		return FALSE;

	return TRUE;
}

//----------------------------------------------------------------------------
void AssignAction::DeleteField(LPCTSTR pszFieldName)
{
	if (!m_pField)
		return;

	if (m_pField->GetName().CompareNoCase(pszFieldName) == 0)
	{
		SAFE_DELETE(m_pExpr); 
		SAFE_DELETE(m_pIndexerExpr); 
        m_pField = NULL;
		m_ActionType = ACT_NONE;
	}    	
}

//----------------------------------------------------------------------------
BOOL AssignAction::IsEmpty() const
{
	return (m_pExpr == NULL || m_pField == NULL);
}

//===========================================================================
//              Class ConditionalAction implementation
//===========================================================================
IMPLEMENT_DYNAMIC(ConditionalAction, ActionObj)

ConditionalAction::ConditionalAction (ActionObj* pParent, SymTable* pSymTable, RepEngine* pEngine, CFunctionDescription* pFun)
	:
	ActionObj		(pParent, pSymTable, pEngine, pFun),

	m_ConditionExpr	(GetSymTable()),

	m_ThenBlock		(this, pSymTable, pEngine, TRUE, pFun),
	m_ElseBlock		(this, pSymTable, pEngine, TRUE, pFun)
{
	m_ActionType = ACT_CONDITIONAL;
}

//---------------------------------------------------------------------------
BOOL ConditionalAction::Exec()
{
	m_ActionState = STATE_NORMAL;

	DataBool goodVal;
	if (!m_ConditionExpr.Eval(goodVal))
		return SetRTError(EngineScheduler::REPORT_EVAL_COND_EXPR, m_ConditionExpr.GetErrDescription());

	Block* pBlock =  
		(BOOL) goodVal 
			? &m_ThenBlock
			: (m_ElseBlock.IsEmpty() ? NULL : &m_ElseBlock);

	if (!pBlock)
		return TRUE;

	BOOL ok = pBlock->Exec();

	if (!ok || pBlock->m_ActionState == STATE_ABORT)
		return Fail();

	if (pBlock->m_ActionState == STATE_RETURN || pBlock->m_ActionState == STATE_BREAK || pBlock->m_ActionState == STATE_CONTINUE)
	{
		m_ActionState = pBlock->m_ActionState;
	}

	return TRUE;
}

//---------------------------------------------------------------------------
BOOL ConditionalAction::Parse(Parser& parser)
{
	parser.ParseTag(T_IF);

	m_ConditionExpr.SetStopTokens(T_THEN);
	if (!m_ConditionExpr.Parse(parser, DATA_BOOL_TYPE)) return FALSE;

	if (!parser.ParseTag(T_THEN))	return FALSE;
	if (!m_ThenBlock.Parse(parser))	return FALSE;
	
	if (parser.Matched(T_ELSE))
	{
		if (!m_ElseBlock.Parse(parser))	return FALSE;
		
		return m_ElseBlock.m_bHasBeginEnd ? parser.ParseSep() : TRUE;
	}
	
	// lexan error ?
	if (parser.ErrorFound())	return FALSE;
	
	// ELSE not found
	return m_ThenBlock.m_bHasBeginEnd ? parser.ParseSep() : TRUE;
}

//----------------------------------------------------------------------------
void ConditionalAction::Unparse(Unparser& oFile, BOOL /*bNewLine = TRUE*/, BOOL /*bSkipBeginEnd = FALSE*/)
{
	oFile.UnparseComment(this->m_arCommentTraceBefore);

	if (m_ThenBlock.IsEmpty() && m_ElseBlock.IsEmpty()) 
	{
		oFile.UnparseComment(this->m_arCommentTraceAfter);
		return;
	}

	oFile.UnparseTag	(T_IF,				FALSE);
	oFile.UnparseExpr	(m_ConditionExpr.ToString(),	TRUE);
		
	oFile.IncTab();
	oFile.UnparseTag(T_THEN, FALSE);
        
    // if block has Begin-End syntax no CrLf is wrote to append a separator
    // if there isn't any ELSE statement, otherwise after a simple action
    // is always appended a separator plus a CrLf
	m_ThenBlock.Unparse(oFile, FALSE);

	if (!m_ElseBlock.IsEmpty())
	{                                            
		if (m_ElseBlock.m_bHasBeginEnd)
			oFile.UnparseCrLf();	// it need a cr-lf pair but not a separator
				
		oFile.UnparseTag(T_ELSE, FALSE);

        // if block has Begin-End syntax no CrLf is wrote to append a separator,
        // otherwise after a simple action is always appended a separator plus
        // a CrLf
		m_ElseBlock.Unparse(oFile, FALSE);

		if (m_ElseBlock.m_bHasBeginEnd)
			oFile.UnparseSep(TRUE);	// it need a separator and a cr-lf pair
	}                                                          
	else // ELSE not present
		if (m_ThenBlock.m_bHasBeginEnd)
			oFile.UnparseSep(TRUE);	// it need a separator and a cr-lf pair

	oFile.DecTab();
	oFile.UnparseComment(this->m_arCommentTraceAfter);
}

//----------------------------------------------------------------------------
BOOL ConditionalAction::IsEmpty() const
{
	return m_ThenBlock.IsEmpty() && m_ElseBlock.IsEmpty();
}

//----------------------------------------------------------------------------
BOOL ConditionalAction::CanDeleteField(LPCTSTR pszFieldName)
{
	if (m_ConditionExpr.HasMember(pszFieldName))
		return FALSE;

	if (!m_ThenBlock.IsEmpty() && !m_ThenBlock.CanDeleteField(pszFieldName))
		return FALSE;

	if (!m_ElseBlock.IsEmpty() && !m_ElseBlock.CanDeleteField(pszFieldName))
		return FALSE;

	return TRUE;
}

//----------------------------------------------------------------------------
void ConditionalAction::DeleteField(LPCTSTR pszFieldName)
{
	if (!m_ThenBlock.IsEmpty()) m_ThenBlock.DeleteField(pszFieldName);
	if (!m_ElseBlock.IsEmpty()) m_ElseBlock.DeleteField(pszFieldName);
}

//----------------------------------------------------------------------------
void ConditionalAction::DeleteTable(LPCTSTR pszDispTableName)
{
    if (!m_ThenBlock.IsEmpty()) m_ThenBlock.DeleteTable(pszDispTableName);
	if (!m_ElseBlock.IsEmpty()) m_ElseBlock.DeleteTable(pszDispTableName);
}

//----------------------------------------------------------------------------
void ConditionalAction::DispTableChanged(LPCTSTR pszOldName, LPCTSTR pszNewName)
{
	if (!m_ThenBlock.IsEmpty()) m_ThenBlock.DispTableChanged(pszOldName, pszNewName);
	if (!m_ElseBlock.IsEmpty()) m_ElseBlock.DispTableChanged(pszOldName, pszNewName);
}

//===========================================================================
//              Class WhileLoopAction implementation
//===========================================================================
IMPLEMENT_DYNAMIC(WhileLoopAction, ActionObj)

WhileLoopAction::WhileLoopAction (ActionObj* pParent, SymTable* pTable, RepEngine* pEngine, CFunctionDescription* pFun)
	:
	ActionObj		(pParent, pTable, pEngine, pFun),

	m_ConditionExpr	(GetSymTable()),
	m_Block			(this, pTable, pEngine, TRUE, pFun)
{
	m_ActionType	= ACT_WHILE;	
}

//---------------------------------------------------------------------------
BOOL WhileLoopAction::Exec()
{
	m_ActionState = STATE_NORMAL;

	DataBool goodVal;
	for (;;)
	{
		if (GetRepEngine () && GetRepEngine ()->UserBreak())
			break;

		if (!m_ConditionExpr.Eval(goodVal))
			return Fail(m_ConditionExpr.GetErrDescription());	

		if (!((BOOL)goodVal))
			return TRUE;

		BOOL ok = m_Block.Exec();

		if (!ok || m_Block.m_ActionState == STATE_ABORT)
			return Fail();

		if (m_Block.m_ActionState == STATE_RETURN)
		{
			m_ActionState = STATE_RETURN;
			return TRUE;
		}
		if (m_Block.m_ActionState == STATE_BREAK)
		{
			return TRUE;
		}
	}
	return FALSE;
}

//---------------------------------------------------------------------------
BOOL WhileLoopAction::Parse(Parser& parser)
{
	parser.ParseTag(T_WHILE);

	m_ConditionExpr.SetStopTokens(T_DO);
	if (!m_ConditionExpr.Parse(parser, DATA_BOOL_TYPE)) 
		return FALSE;

	if (!parser.ParseTag(T_DO))	return FALSE;
	if (!m_Block.Parse(parser))	return FALSE;
	
	return m_Block.m_bHasBeginEnd ? parser.ParseSep() : TRUE;
}

//----------------------------------------------------------------------------
void WhileLoopAction::Unparse(Unparser& oFile, BOOL /*bNewLine = TRUE*/, BOOL /*bSkipBeginEnd = FALSE*/)
{
	oFile.UnparseComment(this->m_arCommentTraceBefore);

	if (m_Block.IsEmpty()) 
	{
		oFile.UnparseComment(this->m_arCommentTraceAfter);
		return;
	}

	oFile.UnparseTag	(T_WHILE,			FALSE);
	oFile.UnparseExpr	(m_ConditionExpr.ToString(),	TRUE);
		
	oFile.IncTab();
	oFile.UnparseTag(T_DO, FALSE);
        
    // if block has Begin-End syntax no CrLf is wrote to append a separator
	m_Block.Unparse(oFile, FALSE);

	if (m_Block.m_bHasBeginEnd)
		oFile.UnparseSep(TRUE);	// it need a separator and a cr-lf pair

	oFile.DecTab();
	oFile.UnparseComment(this->m_arCommentTraceAfter);
}

//----------------------------------------------------------------------------
BOOL WhileLoopAction::IsEmpty() const
{
	return m_Block.IsEmpty();
}

//----------------------------------------------------------------------------
BOOL WhileLoopAction::CanDeleteField(LPCTSTR pszFieldName)
{
	if (m_ConditionExpr.HasMember(pszFieldName))
		return FALSE;

	if (!m_Block.IsEmpty() && !m_Block.CanDeleteField(pszFieldName))
		return FALSE;

	return TRUE;
}

//----------------------------------------------------------------------------
void WhileLoopAction::DeleteField(LPCTSTR pszFieldName)
{
    if (!m_Block.IsEmpty())	
		m_Block.DeleteField(pszFieldName);
}

//----------------------------------------------------------------------------
void WhileLoopAction::DeleteTable(LPCTSTR pszDispTableName)
{
    if (!m_Block.IsEmpty())	
		m_Block.DeleteTable(pszDispTableName);
}

//----------------------------------------------------------------------------
void WhileLoopAction::DispTableChanged(LPCTSTR pszOldName, LPCTSTR pszNewName)
{
	if (!m_Block.IsEmpty())
		m_Block.DispTableChanged(pszOldName, pszNewName);
}

//===========================================================================
//              Class DisplayFieldsAction implementation
//===========================================================================
IMPLEMENT_DYNAMIC(DisplayFieldsAction, ActionObj)

DisplayFieldsAction::DisplayFieldsAction (ActionObj* pParent, WoormTable* pTable, RepEngine* pEngine)
	:
	ActionObj       (pParent, pTable, pEngine)
{
	m_ActionType = ACT_DISPLAY;
}

//---------------------------------------------------------------------------
BOOL DisplayFieldsAction::Exec()
{
	m_ActionState = STATE_NORMAL;

	if (!GetRepEngine ())
	{
		return FALSE;
	}

	for(int i = 0; i < m_Fields.GetSize(); i++)
	   if (!((WoormField*) m_Fields[i])->Display(*GetRepEngine ()))
			return FALSE;

	return TRUE;
}

//---------------------------------------------------------------------------
BOOL DisplayFieldsAction::Parse(Parser& parser)
{
	Token actionToken = parser.SkipToken();

	switch (actionToken)
	{
		case T_DISPLAY :
		{
			m_ActionType = ACT_DISPLAY;

			CString strName;
			do
			{
				if (!parser.ParseID(strName)) 
					break;

				WoormField* pRepField = GetWoormTable()->GetField(strName);
				if (pRepField == NULL)
					return parser.SetError(Expression::FormatMessage(Expression::UNKNOWN_FIELD), strName);

				if (GetRepEngine() && pRepField->GetDataType() != DataType::Array)
				{
					if	(
							pRepField->IsColumn() && 
							!pRepField->IsColTotal() &&
							GetRepEngine () && GetRepEngine()->m_bOnFormFeedAction
						)
						return parser.SetError(_TB("Unable to display column on form feed event"), strName);

					if (!pRepField->IsHidden())
					{
						pRepField->SetDisplayed();
						m_Fields.Add(pRepField);
					}
					// else ignore action
				}
				else 
				{
					m_Fields.Add(pRepField);
				}
			}
			while (parser.Matched(T_COMMA));

			break;
		}
		case T_DISPLAY_TABLE_ROW :
		{
			m_ActionType = ACT_DISPLAY_TABLE_ROW;

			BOOL bUnnamed = TRUE;
			DisplayTableEntry* pDisplayTable = GetWoormTable()->MatchDisplayTable(parser, bUnnamed);
			if (pDisplayTable == NULL) 
				return NULL;    // error set by MatchDisplayTable
			if (!bUnnamed)
				m_sTableName = pDisplayTable->GetTableName();

			if (GetRepEngine () && GetRepEngine ()->m_bOnFormFeedAction)
				return parser.SetError(_TB("Unable to display line on form feed event"), pDisplayTable->GetTableName());

			if (pDisplayTable->GetColumns())
			{
				ASSERT(pDisplayTable->IsKindOf(RUNTIME_CLASS(DisplayTableEntryEngine)));
				for (int i = 0; i <= pDisplayTable->GetColumns()->GetUpperBound(); i++)
				{
					WoormField* pRepField = (WoormField*) pDisplayTable->GetColumns()->GetAt(i);
					if (!pRepField->IsHidden() && !pRepField->IsColTotal() && !pRepField->IsSubTotal())
					{
						pRepField->SetDisplayed();
						m_Fields.Add(pRepField);
					}
				}
			}
			else ASSERT(pDisplayTable->IsKindOf(RUNTIME_CLASS(DisplayTableEntry)));


			break;
		}
		case T_DISPLAY_FREE_FIELDS :
		{
			m_ActionType = ACT_DISPLAY_FREE_FIELDS;

			WoormTable* pWTable = (WoormTable*) (GetSymTable()->GetRoot());

			for (int i = 0; i <= pWTable->GetUpperBound(); i++)
			{
				WoormField* pRepField = pWTable->GetAt(i);

				if (!pRepField->IsKindOf(RUNTIME_CLASS(WoormField))) 
				{
					ASSERT(FALSE);
					continue;
				}
				// vengono visualizzati solo i campi liberi (i totali di colonna no)
				if (
						!pRepField->IsHidden() &&
						!pRepField->IsColumn() &&
						!pRepField->IsSubTotal()  &&
						!pRepField->IsColTotal() &&
						pRepField->GetId() < SpecialReportField::REPORT_LOWER_SPECIAL_ID
					)
				{
					pRepField->SetDisplayed();
					m_Fields.Add(pRepField);
				}
			}
			break;
		}
	}

	if (parser.ErrorFound())  
		return FALSE;

	if (GetRepEngine ()) 
		GetRepEngine ()->m_bDispActionFound = TRUE;

	return parser.ParseSep();
}

//----------------------------------------------------------------------------
void DisplayFieldsAction::Unparse(Unparser& oFile, BOOL /*bNewLine = TRUE*/, BOOL /*bSkipBeginEnd = FALSE*/)
{
	oFile.UnparseComment(this->m_arCommentTraceBefore);

	if (m_ActionType == ACT_NONE)
	{
		oFile.UnparseComment(this->m_arCommentTraceAfter);
		return;
	}

	if (m_ActionType == ACT_DISPLAY_TABLE_ROW)
	{
    	oFile.UnparseTag (T_DISPLAY_TABLE_ROW,		FALSE);

		if (!m_sTableName.IsEmpty())
			oFile.UnparseID (m_sTableName,		FALSE);

		oFile.UnparseSep(TRUE);
		oFile.UnparseComment(this->m_arCommentTraceAfter);
		return;
	}

	if (m_ActionType == ACT_DISPLAY_FREE_FIELDS)
	{
    	oFile.UnparseTag (T_DISPLAY_FREE_FIELDS, FALSE);
		oFile.UnparseSep(TRUE);
		oFile.UnparseComment(this->m_arCommentTraceAfter);
		return;
	}

	if (m_Fields.GetSize() > 0)
	{
    	oFile.UnparseTag (T_DISPLAY,	FALSE);
		BOOL bFirst = TRUE;
		for (int i = 0; i < m_Fields.GetSize(); i++)
		{
			WoormField*	pField = dynamic_cast<WoormField*>(m_Fields[i]);
			if (!pField) continue;

			ASSERT_VALID(pField);
			try {
				pField->GetName();
			}
			catch (...)
			{
				return;
			}

			oFile.UnparseID	(pField->GetName(), FALSE);

			if (i < m_Fields.GetUpperBound() && !pField->GetName().IsEmpty())
            	oFile.UnparseComma(FALSE);
		}
		oFile.UnparseSep(TRUE);
    }
	oFile.UnparseComment(this->m_arCommentTraceAfter);
}

//---------------------------------------------------------------------------
BOOL DisplayFieldsAction::ExistColumnOf	(DisplayTableEntryEngine* pDT)
{
	for (int i = 0; i < m_Fields.GetSize(); i++)
	{
		WoormField* pField = (WoormField*) m_Fields[i];
		ASSERT_VALID(pField);

		DisplayTableEntry* pOwnDT = pField->GetDisplayTable();
		if (pOwnDT && pOwnDT->GetId() == pDT->GetId())
			return TRUE;
	}
	return FALSE;
}

//----------------------------------------------------------------------------
void DisplayFieldsAction::GetSubtotalFields(CStringArray& arraySubtotals)
{
	for (int i = 0; i < m_Fields.GetSize(); i++)
	{
		WoormField* pField = (WoormField*) m_Fields[i];
		ASSERT(pField); ASSERT_VALID(pField);

		if (pField->IsSubTotal())
		{
			EventFunction* funcData = pField->GetEventFunction();
            if (funcData)
				arraySubtotals.Add(funcData->GetPublicName());
		}
    }
}

//----------------------------------------------------------------------------
BOOL DisplayFieldsAction::CanDeleteField(LPCTSTR)
{
	// it is always possible the deleting field
	return TRUE;
}

//----------------------------------------------------------------------------
void DisplayFieldsAction::DeleteField(LPCTSTR pszFieldName)
{
	if (m_ActionType == ACT_DISPLAY_TABLE_ROW)
	{
		return;
	}

	if (m_ActionType == ACT_DISPLAY_FREE_FIELDS)
	{
		if (! ((WoormTable*)(GetSymTable()->GetRoot()))->MoreVisibleFreeFields(pszFieldName))
		{
			m_ActionType = ACT_NONE;
		}
		return;
	}

	ASSERT(m_ActionType == ACT_DISPLAY);

	for (int i = m_Fields.GetUpperBound(); i >=0; i--)
	{
		WoormField* pField = (WoormField*) m_Fields[i];
		ASSERT(pField); ASSERT_VALID(pField);

		if (pField->GetName().CompareNoCase(pszFieldName) == 0)
		{
			m_Fields.RemoveAt(i);
           // return;
        }
	}
}

//----------------------------------------------------------------------------
void DisplayFieldsAction::DeleteTable(LPCTSTR pszDispTableName)
{
	if (m_ActionType == ACT_DISPLAY_TABLE_ROW)
	{
		if (	
			m_sTableName.IsEmpty() ||
			m_sTableName.CompareNoCase(pszDispTableName) == 0
		)
			{
				m_sTableName.Empty();
				m_ActionType = ACT_NONE;
				m_Fields.RemoveAll();
			}
	}
}

//----------------------------------------------------------------------------
void DisplayFieldsAction::AddDisplayName(LPCTSTR pszFieldName)
{
	WoormField* pField = GetWoormTable()->GetField(pszFieldName);
	ASSERT(pField);
	if (pField)
		m_Fields.Add(pField);
}

//----------------------------------------------------------------------------
BOOL DisplayFieldsAction::IsEmpty() const
{
	return m_ActionType == ACT_DISPLAY ? m_Fields.GetSize() == 0 : FALSE;
}

//===========================================================================
//              Class DisplayTableAction implementation
//===========================================================================
IMPLEMENT_DYNAMIC(DisplayTableAction, ActionObj)

DisplayTableAction::DisplayTableAction(ActionObj* pParent, WoormTable* pTable, RepEngine* pEngine)
	:
	ActionObj		(pParent, pTable, pEngine),

	m_RdeCommand	(RDEManager::NONE),
	m_pDisplayTable	(NULL)
{
}

DisplayTableAction::DisplayTableAction(ActionObj* pParent, RepEngine* pEngine, IRDEManager::Command cmd, DisplayTableEntryEngine* pDT)
	:
	ActionObj		(pParent, NULL, pEngine),

	m_RdeCommand	(cmd),
	m_pDisplayTable	(pDT)
{
	switch(cmd)
	{
	case RDEManager::INTER_LINE:
			m_ActionType = ACT_INTERLINE;
			break;
	case RDEManager::NEXT_LINE:
			m_ActionType = ACT_NEXTLINE;
			break;
	case RDEManager::TITLE_LINE:
			m_ActionType = ACT_TITLELINE;
			break;
	case RDEManager::CUSTOM_TITLE_LINE:
			m_ActionType = ACT_CUSTOM_TITLELINE;
			break;
	default:
		ASSERT(FALSE);
		return;
	}
	ASSERT(m_pDisplayTable);

	m_sTableName = m_pDisplayTable->GetTableName();
}

//---------------------------------------------------------------------------
BOOL DisplayTableAction::Exec()
{
	m_ActionState = STATE_NORMAL;

	if (!GetRepEngine ())
	{
		return FALSE;
	}

	ASSERT(!GetRepEngine ()->GetCallerDoc()->m_dsCurrentLayoutEngine.GetString().IsEmpty());

	CString sCurrL(GetRepEngine ()->GetCallerDoc()->m_dsCurrentLayoutEngine.GetString());

	if (m_pDisplayTable->GetLayoutName().CompareNoCase(sCurrL))
		return TRUE;

	if (m_ActionType == ACT_CUSTOM_TITLELINE)
	{
		ASSERT_VALID(m_pCustomTitle);
		if (!m_pCustomTitle)
			return SetRTError(EngineScheduler::REPORT_EVAL_EVENT_EXPR);
		DataStr strCustomTitle;
		if (!m_pCustomTitle->Eval(strCustomTitle))
			return SetRTError(EngineScheduler::REPORT_EVAL_EVENT_EXPR, m_pCustomTitle->GetErrDescription());

		RDEData* pRDEData = new RDEData((WORD)m_RdeCommand, strCustomTitle);

		m_pDisplayTable->SetDataDisplayed();

		BOOL ok = m_pDisplayTable->WriteLine(*GetRepEngine(), (WORD)m_RdeCommand, pRDEData);

		delete pRDEData;
		return ok;
	}

	if (m_ActionType == ACT_TITLELINE)
		m_pDisplayTable->SetDataDisplayed();

	return m_pDisplayTable->WriteLine(*GetRepEngine (), (WORD)m_RdeCommand, NULL);
}

//---------------------------------------------------------------------------
BOOL DisplayTableAction::Parse(Parser& parser)
{
	Token tk = parser.SkipToken();
	switch(tk)
	{
		case T_INTERLINE:
				m_RdeCommand = RDEManager::INTER_LINE;
				m_ActionType = ACT_INTERLINE;
				break;

		case T_NEXTLINE:
				m_RdeCommand = RDEManager::NEXT_LINE;
				m_ActionType = ACT_NEXTLINE;
				break;
		case T_SPACELINE:
				m_RdeCommand = RDEManager::NEXT_LINE;
				m_ActionType = ACT_SPACELINE;
				break;

		case T_TITLELINE:
				m_RdeCommand = RDEManager::TITLE_LINE;
				m_ActionType = ACT_TITLELINE;
				break;

		case T_SUBTITLELINE:
			m_RdeCommand = RDEManager::CUSTOM_TITLE_LINE;
			m_ActionType = ACT_CUSTOM_TITLELINE;

			m_pCustomTitle = new Expression(GetSymTable());
			m_pCustomTitle->SetStopTokens(T_OF, T_SEP);
			if (!m_pCustomTitle->Parse(parser, DATA_STR_TYPE, TRUE))
				return FALSE;
 			break;

		default: 
				ASSERT(FALSE);
				return FALSE;
	}

	BOOL bUnnamed = TRUE;
	m_pDisplayTable = GetWoormTable()->MatchDisplayTable(parser, bUnnamed);
	if (m_pDisplayTable == NULL) 
		return NULL;    // error set by MatchDisplayTable

	if (!bUnnamed)
		m_sTableName = m_pDisplayTable->GetTableName();

	if (GetRepEngine ()) 
		GetRepEngine ()->m_bDispActionFound = TRUE;

	return parser.ParseSep();
}

//----------------------------------------------------------------------------
void DisplayTableAction::Unparse(Unparser& oFile, BOOL /*bNewLine = TRUE*/, BOOL /*bSkipBeginEnd = FALSE*/)
{
	oFile.UnparseComment(this->m_arCommentTraceBefore);
	
	WoormTable* pWSymTable = GetWoormTable();
	if (pWSymTable)
	{
		if (pWSymTable->GetDisplayTablesNum() == 0)
			m_ActionType = ACT_NONE;
		if (!m_sTableName.IsEmpty() && !pWSymTable->ExistsDisplayTable(m_sTableName))
			m_ActionType = ACT_NONE;
	}

	switch (m_ActionType)
	{
		case ACT_NONE:
			oFile.UnparseComment(this->m_arCommentTraceAfter);
			return;
			
		case ACT_INTERLINE:
			oFile.UnparseTag (T_INTERLINE, FALSE);
			if (!m_sTableName.IsEmpty())
 				oFile.UnparseID	(m_sTableName, FALSE);
			break;

		case ACT_NEXTLINE:
			oFile.UnparseTag (T_NEXTLINE, FALSE);
			if (!m_sTableName.IsEmpty())
 				oFile.UnparseID	(m_sTableName, FALSE);
			break;
		case ACT_SPACELINE:
			oFile.UnparseTag(T_SPACELINE, FALSE);
			if (!m_sTableName.IsEmpty())
				oFile.UnparseID(m_sTableName, FALSE);
			break;

		case ACT_TITLELINE:
			oFile.UnparseTag (T_TITLELINE, FALSE);
			if (!m_sTableName.IsEmpty())
 				oFile.UnparseID	(m_sTableName, FALSE);
			break;

		case ACT_CUSTOM_TITLELINE:
			oFile.UnparseTag(T_SUBTITLELINE, FALSE);

			if (m_pCustomTitle && !m_pCustomTitle->IsEmpty())
				oFile.UnparseExpr(m_pCustomTitle->ToString(), FALSE);

			if (!m_sTableName.IsEmpty())
			{
				oFile.UnparseTag(T_OF);
				oFile.UnparseID(m_sTableName, FALSE);
			}

			break;
		default:
			ASSERT(FALSE);
	}
	oFile.UnparseSep(TRUE);
	oFile.UnparseComment(this->m_arCommentTraceAfter);
}

//---------------------------------------------------------------------------
void DisplayTableAction::SetTableName(LPCTSTR pszTableName)
{ 
	if (pszTableName == NULL || *pszTableName == '\0')
	{
		m_pDisplayTable = GetWoormTable()->FindSingleDisplayTable();
		if (!m_pDisplayTable)
		{
			ASSERT(FALSE);
			m_sTableName.Empty();
			m_ActionType = ACT_NONE;
		}
		return;
	}

	Parser parser(pszTableName);
	BOOL bUnnamed = TRUE;
	m_pDisplayTable = GetWoormTable()->MatchDisplayTable(parser, bUnnamed);
	if (!m_pDisplayTable)
	{
		ASSERT(FALSE);
		m_sTableName.Empty();
		m_ActionType = ACT_NONE;
		return;
	}

	if (!bUnnamed)
		m_sTableName = pszTableName;
}

//----------------------------------------------------------------------------
void DisplayTableAction::DeleteTable(LPCTSTR pszDispTableName)
{
	switch (m_ActionType)
	{
		case ACT_INTERLINE	:
		case ACT_NEXTLINE	:
		case ACT_SPACELINE:
		case ACT_TITLELINE	:
		case ACT_CUSTOM_TITLELINE:
		{
			if	(
					m_sTableName.IsEmpty() ||
					m_sTableName.CompareNoCase(pszDispTableName) == 0
				)
			{
				m_sTableName.Empty();
				m_ActionType = ACT_NONE;
				m_pDisplayTable = NULL;
			}
		}
	}
}

//----------------------------------------------------------------------------
void DisplayTableAction::DispTableChanged(LPCTSTR pszOldName, LPCTSTR pszNewName)
{
	if (m_sTableName.CompareNoCase(pszOldName) == 0)
		m_sTableName = pszNewName;
}


//===========================================================================
//              Class DisplayChartAction implementation
//===========================================================================
IMPLEMENT_DYNAMIC(DisplayChartAction, ActionObj)

DisplayChartAction::DisplayChartAction(ActionObj* pParent, WoormTable* pSymTable, RepEngine* pEngine)
	:
	ActionObj(pParent, pSymTable, pEngine)
{
	m_ActionType = ACT_DISPLAY_CHART;
}

//---------------------------------------------------------------------------
BOOL DisplayChartAction::Exec()
{
	m_ActionState = STATE_NORMAL;

	if (!GetRepEngine())
	{
		return FALSE;
	}

	ASSERT(!GetRepEngine()->GetCallerDoc()->m_dsCurrentLayoutEngine.GetString().IsEmpty());

	CString sCurrL(GetRepEngine()->GetCallerDoc()->m_dsCurrentLayoutEngine.GetString());

	SymTable* pSymTable = GetSymTable()->GetRoot();
	for (int i = 0; i < pSymTable->GetSize(); i++)
	{
		WoormField* pF = dynamic_cast<WoormField*>(pSymTable->GetAt(i));
		if (!pF) continue;
		if (pF->GetDataType() != DataType::Array) continue;

		pF->WriteArray(*GetRepEngine());
	}

	return TRUE;
}

//---------------------------------------------------------------------------
BOOL DisplayChartAction::Parse(Parser& parser)
{
	Token tk = parser.SkipToken();
	switch (tk)
	{
	case T_DISPLAY_CHART:
		
		m_ActionType = ACT_DISPLAY_CHART;
		break;

	default:
		ASSERT(FALSE);
		return FALSE;
	}

	CString strTableName;
	if (!parser.ParseID(this->m_sChartName))
		return FALSE;

	return parser.ParseSep();
}

//----------------------------------------------------------------------------
void DisplayChartAction::Unparse(Unparser& oFile, BOOL /*bNewLine = TRUE*/, BOOL /*bSkipBeginEnd = FALSE*/)
{
	oFile.UnparseComment(this->m_arCommentTraceBefore);

	switch (m_ActionType)
	{
	case ACT_NONE:
		oFile.UnparseComment(this->m_arCommentTraceAfter);
		return;

	case ACT_DISPLAY_CHART:
		oFile.UnparseTag(T_DISPLAY_CHART, FALSE);
		if (!m_sChartName.IsEmpty())
			oFile.UnparseID(m_sChartName, FALSE);
		break;

	default:
		ASSERT(FALSE);
	}
	oFile.UnparseSep(TRUE);
	oFile.UnparseComment(this->m_arCommentTraceAfter);
}

//---------------------------------------------------------------------------
void DisplayChartAction::SetChartName(LPCTSTR pszChartName)
{
	m_sChartName = pszChartName;
}

//----------------------------------------------------------------------------
void DisplayChartAction::DeleteTable(LPCTSTR pszChartName)
{
	m_sChartName.Empty();
	m_ActionType = ACT_NONE;
}

//===========================================================================
//              Class FormFeedAction implementation
//===========================================================================
IMPLEMENT_DYNAMIC(FormFeedAction, ActionObj)

FormFeedAction::FormFeedAction(ActionObj* pParent, WoormTable* pSymTable, RepEngine* pEngine)
	:
	ActionObj				(pParent, pSymTable, pEngine),

	m_bCalledFromOnTable	(pEngine ? pEngine->m_bOnTableAction : FALSE),
	m_bForced				(FALSE)
{
	m_ActionType = ACT_FORMFEED;
}

//---------------------------------------------------------------------------
BOOL FormFeedAction::Exec()
{
	m_ActionState = STATE_NORMAL;

	if (!GetRepEngine ())
	{
		return FALSE;
	}

	if 
		(
			GetRepEngine()->GetEngineStatus() != RepEngine::RE_BODY &&
			GetRepEngine()->GetEngineStatus() != RepEngine::RE_BEFORE &&
			//No! appende pagine vuote GetRepEngine()->GetEngineStatus() != RepEngine::RE_LAST_ROW &&
			!m_bCalledFromOnTable &&
			!m_bForced
		)
	{
		if (GetRepEngine()->GetEngineStatus() == RepEngine::RE_LAST_ROW && !m_sLayoutName.IsEmpty())
		{
			GetRepEngine()->GetCallerDoc()->m_dsCurrentLayoutEngine = m_sLayoutName;
		}
		return TRUE;
	}

	if (GetRepEngine()->GetOutChannel() == NULL)
		return FALSE;

	EventActions* pOnFFActions = GetRepEngine()->GetOnFFActions();

	if (pOnFFActions && !pOnFFActions->GetBeforeActions().Exec())
		return FALSE;

	GetRepEngine()->GetCallerDoc()->m_dlCurrentPageEngine += 1;
	if (m_sLayoutName.IsEmpty())
	{
		if (!GetRepEngine()->GetOutChannel()->Write(0, RDEManager::NEW_PAGE, 0))
			return SetRTError(EngineScheduler::REPORT_RDE_WRITE_NEWPAGE);
	}
	else
	{
		GetRepEngine()->GetCallerDoc()->m_dsCurrentLayoutEngine = m_sLayoutName;

		if (!GetRepEngine()->GetOutChannel()->WriteNewPage(m_sLayoutName))
			return SetRTError(EngineScheduler::REPORT_RDE_WRITE_NEWPAGE);

		GetWoormTable()->ReattachDisplayTable(m_sLayoutName);
	}

	// we must reset all Display Tables
	GetRepEngine()->GetSymTable().ResetRowsCounter();

	return !pOnFFActions || pOnFFActions->GetAfterActions().Exec();
}

//---------------------------------------------------------------------------
BOOL FormFeedAction::Parse(Parser& parser)
{
	if (!parser.ParseTag(T_FORMFEED))
		return FALSE;

	m_bForced = parser.Matched(T_FORCE);

	if (parser.LookAhead(T_STR))
	{
		parser.ParseString(m_sLayoutName);
	}
	
	if (GetRepEngine())
	{
		if (GetRepEngine()->m_bOnFormFeedAction && m_sLayoutName.IsEmpty())
		{
			return parser.SetError(_TB("Illegal nested FormFeed action"));
		}
	 
		GetRepEngine()->m_bDispActionFound = TRUE;
	}

	return parser.ParseSep();
}

//----------------------------------------------------------------------------
void FormFeedAction::Unparse(Unparser& oFile, BOOL /*bNewLine = TRUE*/, BOOL /*bSkipBeginEnd = FALSE*/)
{
	oFile.UnparseComment(this->m_arCommentTraceBefore);

	oFile.UnparseTag (T_FORMFEED,	FALSE);

	if (m_bForced)
		oFile.UnparseTag (T_FORCE,	FALSE);

	if (!m_sLayoutName.IsEmpty())
		oFile.UnparseString	(m_sLayoutName,		FALSE);

	oFile.UnparseSep(TRUE);
	oFile.UnparseComment(this->m_arCommentTraceAfter);
}

//===========================================================================
//              Class CallAction implementation
//===========================================================================
IMPLEMENT_DYNAMIC(CallAction, ActionObj)

CallAction::CallAction (ActionObj* pParent, WoormTable* pSymTable, RepEngine* pEngine)
	:
	ActionObj	(pParent, pSymTable, pEngine)
{
	m_ActionType = ACT_CALL;
}

//---------------------------------------------------------------------------
LPCTSTR	CallAction::GetAssociatedName	()
{ 
	return m_sName; 
}

//---------------------------------------------------------------------------
BOOL CallAction::Exec()
{
	m_ActionState = STATE_NORMAL;

	ProcedureObjItem* pItemProc =  GetWoormTable()->FindProcedure(m_sName);

	if (!pItemProc || !pItemProc->m_pProcedure)
		return SetRTError(EngineScheduler::UNKNOWN_PROCEDURE, m_sName);

	return pItemProc->m_pProcedure->Exec();
}

//---------------------------------------------------------------------------
BOOL CallAction::Parse(Parser& parser)
{
	if (!parser.ParseTag(T_CALL)) 
		return FALSE;

	//m_bForce = parser.Matched(T_FORCE);

	if (!parser.ParseID(m_sName)) 
		return FALSE;
/*
	CWoormDoc* pWDoc = dynamic_cast<CWoormDoc*>(GetWoormTable()->GetDocument());
	if (!m_bForce && pWDoc && pWDoc->m_bAllowEditing)
	{
		ProcedureObjItem* pProcObj = GetWoormTable()->FindProcedure(m_sName);
		if (pProcObj == NULL)
		{
			parser.SetError(_TB("Unknown called procedure"), m_sName);
			return FALSE;
		}
	}
*/
	return parser.ParseSep();
}

//----------------------------------------------------------------------------
void CallAction::Unparse(Unparser& oFile, BOOL /*bNewLine = TRUE*/, BOOL /*bSkipBeginEnd = FALSE*/)
{
	oFile.UnparseComment(this->m_arCommentTraceBefore);

	if (m_ActionType == ACT_NONE)
	{
		oFile.UnparseComment(this->m_arCommentTraceAfter);
		return;
	}

	ProcedureObjItem* pProcObj = GetWoormTable()->FindProcedure(m_sName);
	if (!pProcObj)
		return;

	oFile.UnparseTag (T_CALL,	FALSE);

	//if (m_bForce)
	//	oFile.UnparseTag(T_FORCE, FALSE);

	oFile.UnparseID	(m_sName,	FALSE);
	oFile.UnparseSep(TRUE);

	oFile.UnparseComment(this->m_arCommentTraceAfter);
}

//===========================================================================
//              Class AskDialogAction implementation
//===========================================================================
IMPLEMENT_DYNAMIC(AskDialogAction, ActionObj)

AskDialogAction::AskDialogAction(ActionObj* pParent, WoormTable* pTable, RepEngine* pEngine)
	:
	ActionObj (pParent, pTable, pEngine)
{
	m_ActionType = ACT_ASK;
}

//---------------------------------------------------------------------------
BOOL AskDialogAction::Exec()
{
	m_ActionState = STATE_NORMAL;

	m_allDialogName.RemoveAll();
	m_allDialogName.Add(m_strDialogName);

	if (!GetRepEngine() || !GetRepEngine()->GetCallerDoc())
		return FALSE;

	if (GetRepEngine()->GetCallerDoc()->IsInUnattendedMode())
	{
		return SetRTError(EngineScheduler::REPORT_UNATTENDED);
	}

	AskRuleData* pAskDialogs = &GetRepEngine()->GetAskingRules();
	ASSERT_VALID(pAskDialogs);
	if (pAskDialogs->GetCount() == 0)
	{
		return SetRTError(EngineScheduler::DIALOG_MISSING);;
	}

	AskDialogData*	pDlg = pAskDialogs->GetAskDialog(m_strDialogName);
	if (!pDlg)
	{
		return SetRTError(EngineScheduler::DIALOG_UNKNOWN);;
	}
	if (!pDlg->IsOnAsk())
	{
		return SetRTError(EngineScheduler::DIALOG_NOT_ON_ASK);;
	}

	if (GetRepEngine()->GetCallerDoc()->IsAWoormRunningMultithread())
	{
		HWND hwndThread = GetRepEngine()->GetCallerDoc()->GetFrameHandle();
		return AfxInvokeThreadFunction<DataBool, RepEngine, const CStringArray&>(hwndThread, GetRepEngine(), &RepEngine::ShowAllAskDialogs, m_allDialogName);
	}
	else
		return GetRepEngine()->ShowAllAskDialogs(m_allDialogName);
}

//---------------------------------------------------------------------------
BOOL AskDialogAction::Parse(Parser& parser)
{
	BOOL bOK = TRUE;
	
	while (parser.Matched(T_ASK))
	{
		bOK = bOK && parser.ParseID(m_strDialogName);

		if (bOK)
			m_allDialogName.Add(m_strDialogName);

		while (parser.Matched(T_COMMA))
		{
			bOK = bOK && parser.ParseID(m_strDialogName);
			if (bOK)
				m_allDialogName.Add(m_strDialogName);
		}

		bOK = bOK && parser.ParseSep();
	}
	
	//return parser.ParseTag(T_ASK) && parser.ParseID(m_strDialogName) && parser.ParseSep();

	return bOK;
}

//----------------------------------------------------------------------------
void AskDialogAction::Unparse(Unparser& oFile, BOOL /*bNewLine = TRUE*/, BOOL /*bSkipBeginEnd = FALSE*/)
{
	oFile.UnparseComment(this->m_arCommentTraceBefore);
	if (m_ActionType == ACT_NONE)
	{
		oFile.UnparseComment(this->m_arCommentTraceAfter);
		return;
	}

	oFile.UnparseTag (T_ASK,	FALSE);
	//oFile.UnparseID(m_strDialogName, FALSE);
	for (int i = 0; i < m_allDialogName.GetCount(); i++)
	{
		oFile.UnparseID(m_strDialogName, FALSE);

		if (i != m_allDialogName.GetCount() - 1)
			oFile.UnparseComma(FALSE);
	}

	oFile.UnparseSep(TRUE);
	oFile.UnparseComment(this->m_arCommentTraceAfter);
}

//===========================================================================
//              Class MessageBoxAction implementation
//===========================================================================
IMPLEMENT_DYNAMIC(MessageBoxAction, ActionObj)

MessageBoxAction::MessageBoxAction(ActionObj* pParent, SymTable* pTable, RepEngine* pEngine)
	:
	ActionObj	(pParent, pTable, pEngine),

	m_Message	(GetSymTable())
{
	m_ActionType = ACT_MESSAGE_BOX;	//ACT_MESSAGE_BOX, ACT_ABORT, ACT_DEBUG
}

//---------------------------------------------------------------------------
BOOL MessageBoxAction::Exec()
{
	m_ActionState = STATE_NORMAL;

	DataStr strMessage;
	if (!m_Message.Eval(strMessage))
		return SetRTError(EngineScheduler::REPORT_EVAL_EVENT_EXPR, m_Message.GetErrDescription());

	if (m_ActionType == ACT_ABORT)
	{
		return SetRTError(EngineScheduler::REPORT_ABORT_MESSAGE, strMessage.GetString());
	}

	else if (m_ActionType == ACT_MESSAGE_BOX)
	{
		if (GetRepEngine())
		{
			if (!m_sIcon.IsEmpty())
				strMessage = strMessage + _T("|") + m_sIcon;
			RDEData* pRDEData = new RDEData(RDEManager::MESSAGE_BOX, strMessage);

			BOOL ok = GetRepEngine()->GetOutChannel()->Write(0, *pRDEData);

			delete pRDEData;
			return ok;
		}

		int nIcon = MB_ICONINFORMATION;
		if (!m_sIcon.IsEmpty())
		{
			if (m_sIcon.CompareNoCase(L"error") == 0)
				nIcon = MB_ICONERROR;
			else if (m_sIcon.CompareNoCase(L"warning") == 0)
				nIcon = MB_ICONWARNING;
			else if (m_sIcon.CompareNoCase(L"info") == 0)
				nIcon = MB_ICONINFORMATION;
			else if (m_sIcon.CompareNoCase(L"question") == 0)
				nIcon = MB_ICONQUESTION;
		}

		AfxMessageBox(strMessage.GetString(), MB_OK | nIcon);
		return TRUE;
	}

/*	if (m_ActionType == ACT_DEBUG && GetRepEngine() && GetRepEngine()->GetCallerDoc() && GetRepEngine()->GetCallerDoc()->m_bAllowEditing)
	{
		if (GetRepEngine()->GetCallerDoc()->IsInUnattendedMode())
		{
			return SetRTError(EngineScheduler::REPORT_UNATTENDED);
		}

		GetRepEngine()->GetCallerDoc()->OpenDebugger(this);

		return TRUE;
	}
	else */if (m_ActionType == ACT_DEBUG)
	{
		int nIcon = MB_ICONINFORMATION;
		if (!m_sIcon.IsEmpty())
		{
			if (m_sIcon.CompareNoCase(L"error") == 0)
				nIcon = MB_ICONERROR;
			else if (m_sIcon.CompareNoCase(L"warning") == 0)
				nIcon = MB_ICONWARNING;
			else if (m_sIcon.CompareNoCase(L"info") == 0)
				nIcon = MB_ICONINFORMATION;
			else if (m_sIcon.CompareNoCase(L"question") == 0)
				nIcon = MB_ICONQUESTION;
		}
		AfxMessageBox(strMessage.GetString(), MB_OK | nIcon);
		return TRUE;
	}
	else
		AfxMessageBox(strMessage.GetString(), MB_OK);

	return TRUE;
}

//---------------------------------------------------------------------------
BOOL MessageBoxAction::Parse(Parser& parser)
{
	Token actionToken = parser.SkipToken();
	switch(actionToken)
	{
		case T_MESSAGE_BOX:
			m_ActionType = ACT_MESSAGE_BOX;
			break;

		case T_ABORT:
			m_ActionType = ACT_ABORT;
			break;

		case T_DEBUG:
			m_ActionType = ACT_DEBUG;
			break;
	}

	m_Message.SetStopTokens(Token::T_SEP, Token::T_COMMA);
	BOOL bOk = m_Message.Parse(parser, DATA_STR_TYPE, TRUE) ;
	if (!bOk) 
		return FALSE;
	if (parser.Matched(Token::T_COMMA))
	{
		bOk = parser.ParseString(m_sIcon);
	}

	return bOk && parser.ParseSep();
}

//----------------------------------------------------------------------------
void MessageBoxAction::Unparse(Unparser& oFile, BOOL /*bNewLine = TRUE*/, BOOL /*bSkipBeginEnd = FALSE*/)
{
	oFile.UnparseComment(this->m_arCommentTraceBefore);

	switch (m_ActionType)
	{
		case ACT_MESSAGE_BOX:
			if (!m_Message.IsEmpty())
			{
				oFile.UnparseTag	(T_MESSAGE_BOX,	FALSE);

				oFile.UnparseExpr	(m_Message.ToString(),		FALSE);

				if (!m_sIcon.IsEmpty())
				{
					oFile.UnparseTag(T_COMMA, FALSE);
					oFile.UnparseString(m_sIcon, FALSE);
				}
			}
			break;

		case ACT_ABORT:
			if (!m_Message.IsEmpty())
			{
				oFile.UnparseTag	(T_ABORT,	FALSE);

				oFile.UnparseExpr	(m_Message.ToString(),		FALSE);
			}
			break;

		case ACT_QUIT:
			oFile.UnparseTag	(T_QUIT,	FALSE);
			break;

		case ACT_DEBUG:
			if (!m_Message.IsEmpty())
			{
				oFile.UnparseTag	(T_DEBUG,	FALSE);

				oFile.UnparseExpr	(m_Message.ToString(),		FALSE);
			}
			break;
	}
	oFile.UnparseSep(TRUE);
	oFile.UnparseComment(this->m_arCommentTraceAfter);
}

//===========================================================================
//              Class QuitBreakContinueAction implementation
//===========================================================================
IMPLEMENT_DYNAMIC(QuitBreakContinueAction, ActionObj)

QuitBreakContinueAction::QuitBreakContinueAction (ActionObj* pParent, SymTable* pTable, RepEngine* pEngine)
	:
	ActionObj (pParent, pTable, pEngine)
{
}

//---------------------------------------------------------------------------
BOOL QuitBreakContinueAction::Exec()
{
	m_ActionState = STATE_NORMAL;

	ASSERT(m_ActionType != ACT_NONE);

	if (m_ActionType == ACT_QUIT)
	{
		m_ActionState = STATE_QUIT;
		return SetRTError(EngineScheduler::REPORT_QUIT, _TB("<REPORT ACTION QUIT>"));
	}
	
	m_ActionState = (m_ActionType == ACT_BREAK ? STATE_BREAK : STATE_CONTINUE);

	return TRUE;
}

//---------------------------------------------------------------------------
BOOL QuitBreakContinueAction::Parse(Parser& parser)
{
	Token tk = parser.SkipToken();
	switch(tk)
	{
		case T_QUIT:
			m_ActionType = ACT_QUIT;
			break;
		case T_BREAK:
			m_ActionType = ACT_BREAK;
			break;
		case T_CONTINUE:
			m_ActionType = ACT_CONTINUE;
			break;
		default:
			ASSERT(FALSE);
			return FALSE;
	}
	return parser.ParseSep();
}

//----------------------------------------------------------------------------
void QuitBreakContinueAction::Unparse(Unparser& oFile, BOOL /*bNewLine = TRUE*/, BOOL /*bSkipBeginEnd = FALSE*/)
{
	oFile.UnparseComment(this->m_arCommentTraceBefore);

	switch (m_ActionType)
	{
		case ACT_NONE:
			oFile.UnparseComment(this->m_arCommentTraceAfter);
			return;
			
		case ACT_BREAK:
			oFile.UnparseTag	(T_BREAK,		FALSE);
			break;
		case ACT_CONTINUE:
			oFile.UnparseTag	(T_CONTINUE,	FALSE);
			break;
		case ACT_QUIT:
			oFile.UnparseTag	(T_QUIT,		FALSE);
			break;
	}

	oFile.UnparseSep(TRUE);
	oFile.UnparseComment(this->m_arCommentTraceAfter);
}

//===========================================================================
//              Class ReturnAction implementation
//===========================================================================
IMPLEMENT_DYNAMIC(ReturnAction, ActionObj)

ReturnAction::ReturnAction (ActionObj* pParent, SymTable* pTable, RepEngine* pEngine, CFunctionDescription* pFun)
	:
	ActionObj		(pParent, pTable, pEngine, pFun),

	m_ReturnExpr	(GetSymTable())
{
	m_ActionType = ACT_RETURN;
}

//---------------------------------------------------------------------------
BOOL ReturnAction::Exec()
{
	m_ActionState = STATE_NORMAL;

	if (!m_ReturnExpr.IsEmpty())
	{
		ASSERT_VALID(GetFun());
		ASSERT_VALID(GetFun()->GetReturnValue());
		if (GetFun() && !m_ReturnExpr.Eval(*GetFun()->GetReturnValue()))
			return Fail(m_ReturnExpr.GetErrDescription());
	}

	m_ActionState = STATE_RETURN;

	return TRUE;
}

//---------------------------------------------------------------------------
BOOL ReturnAction::Parse(Parser& parser)
{
	if (!parser.ParseTag(T_RETURN))
		return FALSE;

	if (GetFun() && GetFun()->GetReturnValueDataType() != DataType::Void)
	{
		if (!m_ReturnExpr.Parse(parser, GetFun()->GetReturnValueDataType())) 
			return FALSE;
	}
	return parser.ParseSep();
}

//----------------------------------------------------------------------------
void ReturnAction::Unparse(Unparser& oFile, BOOL /*bNewLine = TRUE*/, BOOL /*bSkipBeginEnd = FALSE*/)
{
	oFile.UnparseComment(this->m_arCommentTraceBefore);

	oFile.UnparseTag	(T_RETURN,	FALSE);

	if (!m_ReturnExpr.IsEmpty())
		oFile.UnparseExpr	(m_ReturnExpr.ToString(),		FALSE);

	oFile.UnparseSep(TRUE);
	oFile.UnparseComment(this->m_arCommentTraceAfter);
}

//===========================================================================
//              Class DoExprAction implementation
//===========================================================================
IMPLEMENT_DYNAMIC(DoExprAction, ActionObj)

DoExprAction::DoExprAction(ActionObj* pParent, SymTable* pTable, RepEngine* pEngine)
	:
	ActionObj	(pParent, pTable, pEngine),

	m_Expr		(GetSymTable())
{
	m_ActionType = ACT_DO_EXPR;
}

//---------------------------------------------------------------------------
BOOL DoExprAction::Exec()
{
	m_ActionState = STATE_NORMAL;

	DataObj* pDummy = NULL;
	if (!m_Expr.Eval(pDummy))
	{
		SAFE_DELETE(pDummy);
		return Fail(m_Expr.GetErrDescription());
	}
	SAFE_DELETE(pDummy);
	return TRUE;
}

//---------------------------------------------------------------------------
BOOL DoExprAction::Parse(Parser& parser)
{
	if (!parser.ParseTag(T_DO))
		return FALSE;

	if (!m_Expr.Parse(parser, DataType::Variant)) 
		return FALSE;

	return parser.ParseSep();
}

//----------------------------------------------------------------------------
void DoExprAction::Unparse(Unparser& oFile, BOOL /*bNewLine = TRUE*/, BOOL /*bSkipBeginEnd = FALSE*/)
{
	oFile.UnparseComment(this->m_arCommentTraceBefore);

	oFile.UnparseTag	(T_DO,	FALSE);

	oFile.UnparseExpr	(m_Expr.ToString(), FALSE);

	oFile.UnparseSep(TRUE);
	oFile.UnparseComment(this->m_arCommentTraceAfter);
}

//===========================================================================
//              Class DeclareAction implementation
//===========================================================================
IMPLEMENT_DYNAMIC(DeclareAction, ActionObj)

//---------------------------------------------------------------------------
DeclareAction::DeclareAction (ActionObj* pParent, SymTable* pTable, RepEngine* pEngine, Block* pScopeBlock)
	:
	ActionObj		(pParent, pTable, pEngine),

	m_InitExpr		(GetSymTable()),
	m_pLocalField	(NULL),
	m_pScopeBlock	(pScopeBlock)
{
	m_ActionType = ACT_DECLARE;
}

//---------------------------------------------------------------------------
BOOL DeclareAction::Exec()
{
	m_ActionState = STATE_NORMAL;

	ASSERT(m_pLocalField);

	if (!m_InitExpr.IsEmpty())
	{
		if (!m_InitExpr.Eval(*m_pLocalField->GetData()))
			return Fail(m_InitExpr.GetErrDescription());
	}
	return TRUE;
}

//---------------------------------------------------------------------------
BOOL DeclareAction::Parse(Parser& parser)
{
	DataType aType, aBaseType; CString sRecName;
	if (!parser.ParseDataType(aType, aBaseType, sRecName))
	{
		return FALSE;
	}

	CString strFieldName;
	if (!parser.ParseID(strFieldName)) 
		return FALSE;

	SymField* pExistField = GetSymTable()->GetField(strFieldName);
	if (pExistField)	//alternativa ammettere se livello di scope � superiore, ma forse � fonte di errore
		return parser.SetError(_TB("Duplicate identificator name"), strFieldName);

	m_pLocalField = new WoormField(strFieldName, WoormField::FIELD_INPUT, aType);

	if (aType == DataType::Array)
	{
		m_pLocalField->SetDataType(aBaseType, TRUE);
	}
	
	if (aType == DataType::Record)
	{
		if (sRecName.IsEmpty())
		{
			delete m_pLocalField;
			return parser.SetError(_TB("Missing record table name"), strFieldName);
		}

		const SqlCatalogEntry* pEntry = AfxGetDefaultSqlConnection()->GetCatalogEntry(sRecName);
		if (!pEntry)
		{
			delete m_pLocalField;
			return parser.SetError(_TB("Unkonwn table name"), sRecName);
		}
			
		SqlRecord* pRec = pEntry->CreateRecord();
		if (!pRec)
		{
			delete m_pLocalField;
			return parser.SetError(_TB("wrong table name"), sRecName);
		}

		DataObj* pObj = m_pLocalField->GetData();
		ASSERT_VALID(pObj);
		ASSERT_KINDOF(DataSqlRecord, pObj);

		DataSqlRecord* pDRec = dynamic_cast<DataSqlRecord*> (pObj);
		pDRec->SetIRecord(pRec, TRUE);
	}

	if (parser.Matched(T_ASSIGN))
	{
		m_pLocalField->AddMethodList(parser);

		if (!m_InitExpr.Parse(parser, aType))
			return FALSE;
	}

	m_pScopeBlock->AddLocalField(m_pLocalField); //create or update scope block action's local symbol table

	return parser.ParseSep();
}

//----------------------------------------------------------------------------
void DeclareAction::Unparse(Unparser& oFile, BOOL /*bNewLine = TRUE*/, BOOL /*bSkipBeginEnd = FALSE*/)
{
	ASSERT(m_pLocalField);
	oFile.UnparseComment(this->m_arCommentTraceBefore);

	m_pLocalField->UnparseDataType	(oFile,	FALSE);
	
	oFile.UnparseID(m_pLocalField->GetName(), FALSE);

	if (!m_InitExpr.IsEmpty())
	{
		oFile.UnparseTag	(T_ASSIGN,	FALSE);
		oFile.UnparseExpr	(m_InitExpr.ToString(), FALSE);
	}

	oFile.UnparseSep(TRUE);
	oFile.UnparseComment(this->m_arCommentTraceAfter);
}

//===========================================================================
//              Class Procedure implementation
//===========================================================================
IMPLEMENT_DYNAMIC(Procedure, CObject)
//---------------------------------------------------------------------------
Procedure::Procedure
				(
					const CString&	strProcName,
					WoormTable*		pTable,
 					RepEngine*		pEngine
			   )
	:
	m_pBlock     (new Block(NULL, pTable, pEngine, TRUE, NULL)),

	m_strName   (strProcName),
	m_bParsed	(FALSE),
	m_pFun		(NULL)
{
	m_pBlock->SetForceBeginEnd();
	m_pBlock->m_strOwnerName = L"Procedure." + strProcName;

	m_bCalledFromOnFormFeed	= pEngine ? pEngine->m_bOnFormFeedAction : FALSE;
	m_bCalledFromOnTable	= pEngine ? pEngine->m_bOnTableAction : FALSE;
}

//---------------------------------------------------------------------------
Procedure::~Procedure ()
{ 
	SAFE_DELETE(m_pFun);
	SAFE_DELETE(m_pBlock);
}

//---------------------------------------------------------------------------
void Procedure::SetFun (CFunctionDescription* pF)
{
	//ASSERT(m_pBlock->GetFun() == NULL);
	m_pBlock->SetFun(pF);
	if (pF)
	{
		ASSERT(m_strName == pF->GetName());
		m_pBlock->m_strOwnerName = L"Procedure." + pF->GetName();
	}
	else 
		m_pBlock->m_strOwnerName = L"Procedure." + m_strName;;
}

void Procedure::SetName(const CString& name) 
{ 
	 m_strName = name; 
	 m_pBlock->m_strOwnerName = L"Procedure." + name;
}

//---------------------------------------------------------------------------
BOOL Procedure::Exec ()
{
	//TODO If m_pFun->GetReturnType != void && "there is not return value" then m_Block.SetRTError(EngineScheduler::MISSING_RET_VALUE
	return
		m_pBlock->IsEmpty()
		? (m_bParsed ? TRUE : m_pBlock->SetRTError(EngineScheduler::REPORT_EMPTY_PROC, 0, GetName()))
		: m_pBlock->Exec();
}

//---------------------------------------------------------------------------
BOOL Procedure::Parse (Parser& parser)
{
	if (parser.Matched(T_PROCEDURE))
	{
		CString sName;
		if (!parser.ParseID(sName))
			return FALSE;
		SetName(sName);
	}
	//-----
	DataType type, baseType;
	if (parser.Matched(T_ROUNDOPEN))
	{
		CString paramName;
		m_pFun = new CFunctionDescription (GetName());
		do
		{
			CDataObjDescription* param = new CDataObjDescription();
			if (!parser.ParseDataType(type, baseType))
			{
				return FALSE;
			}
			if (type == DataType::Array)
				param->SetArrayType(baseType);
			else
				param->SetDataType(type);

			if (!parser.ParseID(paramName))
				return FALSE;

			param->SetName(paramName);

			m_pFun->AddParam(param);
				
			if (!parser.LookAhead(T_ROUNDCLOSE))
				parser.Match(T_COMMA);
		}
		while (!parser.Matched(T_ROUNDCLOSE));
	}

	CDataObjDescription aRetVal;
	if (parser.Matched(T_AS))
	{
		if (!m_pFun)
			m_pFun = new CFunctionDescription (GetName());

		if (!parser.ParseDataType(type, baseType))
		{
			return FALSE;
		}
		if (type == DataType::Array)
			aRetVal.SetArrayType(baseType);
		else
		{
			aRetVal.SetDataType(type);
		}
	}
	else
	{
		aRetVal.SetDataType(DataType::Void);
	}

	if (m_pFun)
	{
		m_pFun->SetNsType(CTBNamespace::FUNCTION);
		m_pFun->SetReturnValueDescription(aRetVal);
		
		SetFun(m_pFun);
	}
	//----

	if (m_pBlock->GetRepEngine())
	{
		// for checking the procedure call from a FormFeed event associated action                                      
		m_pBlock->GetRepEngine()->m_bOnFormFeedAction = m_bCalledFromOnFormFeed;
		// for checking the procedure call from a Table event associated action
		m_pBlock->GetRepEngine()->m_bOnTableAction = m_bCalledFromOnTable;
	}

	BOOL ok = m_pBlock->Parse(parser);
	m_bParsed = TRUE;
	
	if (m_pBlock->GetRepEngine())
	{
		m_pBlock->GetRepEngine()->m_bOnFormFeedAction = FALSE;
		m_pBlock->GetRepEngine()->m_bOnTableAction = FALSE;
	}
	return ok;
}

//----------------------------------------------------------------------------
void Procedure::Unparse(Unparser& oFile, BOOL bSkipHeader/* = FALSE*/, BOOL bSkipBeginEnd /*= FALSE*/)
{
	//TODO oFile.UnparseComment(this->m_arCommentTraceBefore);

	if (!bSkipHeader)
	{
		oFile.		UnparseTag		(T_PROCEDURE, FALSE);
		oFile.		UnparseBlank	();
		oFile.		UnparseID		(GetName(), FALSE);

		CFunctionDescription* fd = GetFun();
		if (fd)
		{
			CBaseDescriptionArray& ar = fd->GetParameters();
			if (ar.GetCount() > 0)
			{
				oFile.UnparseTag (T_ROUNDOPEN, FALSE);
				for (int i = 0; i < ar.GetCount(); i++)
				{
					CDataObjDescription* param = (CDataObjDescription*) ar.GetAt(i);
					if (i > 0) oFile.UnparseTag (T_COMMA, FALSE);

					oFile.UnparseDataType		(param->GetDataType(), param->GetBaseDataType(), FALSE);
					oFile.UnparseID				(param->GetName(), FALSE);
				}
				oFile.UnparseTag (T_ROUNDCLOSE, FALSE);
			}
			if (fd->GetReturnValueDataType() != DataType::Void)
			{
				oFile.UnparseTag		(T_AS, FALSE);

				oFile.UnparseDataType	(fd->GetReturnValueDataType(), fd->GetReturnValueDescription().GetBaseDataType(), FALSE);
			}
		}
	}

	m_pBlock->Unparse(oFile, TRUE, bSkipBeginEnd);

	oFile.UnparseCrLf		();
}

/////////////////////////////////////////////////////////////////////////////
// Diagnostics

#ifdef _DEBUG
void Procedure::Dump(CDumpContext& dc) const
{
	ASSERT_VALID(this);
	AFX_DUMP1(dc, "\nSymField = ", GetName());
}

void Procedure::AssertValid() const
{
	CObject::AssertValid();
}
#endif //_DEBUG

//----------------------------------------------------------------------------

//////////////////////////////////////////////////////////////////////////////
// DEBUG

ActionObj* ActionObj::GetActionByRow(int nRow)
{
	return (m_nDebugUnparseRow == nRow) ? const_cast<ActionObj*>(this) : NULL;
}

//----------------------------------------------------------------------------
ActionObj* Block::GetActionByRow(int nRow)
{
	ActionObj* pActionChild = __super::GetActionByRow(nRow);
	if (pActionChild)
		return pActionChild;

	for (int i = 0; i < m_Actions.GetSize(); i++)
	{
		ActionObj* pActionData = (ActionObj*)m_Actions[i];

		if (pActionData->m_nDebugUnparseRow > nRow)
			return NULL;

		ActionObj* pActionChild = pActionData->GetActionByRow(nRow);
		if (pActionChild)
			return pActionChild;
	}
	return NULL;
}

//----------------------------------------------------------------------------
ActionObj* ConditionalAction::GetActionByRow(int nRow)
{
	ActionObj* pActionChild = __super::GetActionByRow(nRow);
	if (pActionChild)
		return pActionChild;

	if (!m_ThenBlock.IsEmpty())
	{
		pActionChild = m_ThenBlock.GetActionByRow(nRow);
		if (pActionChild)
			return pActionChild;
	}

	if (!m_ElseBlock.IsEmpty())
	{
		pActionChild = m_ElseBlock.GetActionByRow(nRow);
		if (pActionChild)
			return pActionChild;
	}

	return NULL;
}

//----------------------------------------------------------------------------
ActionObj* WhileLoopAction::GetActionByRow(int nRow)
{
	ActionObj* pActionChild = __super::GetActionByRow(nRow);
	if (pActionChild)
		return pActionChild;

	if (!m_Block.IsEmpty())
	{
		pActionChild = m_Block.GetActionByRow(nRow);
		if (pActionChild)
			return pActionChild;
	}
	return NULL;
}

//=============================================================================
void ActionObj::RemoveBreakpoint() 
{ 
	ASSERT_VALID(this); 
	if (m_pBreakpoint && this->m_pRepEngine)
	{
		ASSERT_VALID(m_pBreakpoint);
		ASSERT_VALID(m_pRepEngine);
		for (int i = this->m_pRepEngine->m_arBreakpoints.GetCount() - 1; i >= 0; i--)
		{
			CBreakpoint* pB = this->m_pRepEngine->m_arBreakpoints.GetAt(i);
			ASSERT_VALID(pB);
			if (m_pBreakpoint == pB)
			{	
				SAFE_DELETE(m_pBreakpoint); 
				this->m_pRepEngine->m_arBreakpoints.RemoveAt(i);
				break;
			}
		}
	}
}

void ActionObj::AddBreakpoint(BOOL bStepOver/* = FALSE*/)
{
	if (!m_pBreakpoint)
	{
		m_pBreakpoint = new CBreakpoint(this);

		if (this->m_pRepEngine)
		{
			this->m_pRepEngine->m_arBreakpoints.Add(m_pBreakpoint);
		}
	}
	m_pBreakpoint->m_bStepOverBreakpoint = bStepOver;
}

//----------------------------------------------------------------------------
BOOL ActionObj::GetBreakpointRows(CArray<int>& arRows) const
{
	if (HasBreakpoint())
		arRows.Add(m_nDebugUnparseRow);
	return arRows.GetSize();
}

//----------------------------------------------------------------------------
BOOL Block::GetBreakpointRows(CArray<int>& arRows) const
{
	for (int i = 0; i < m_Actions.GetSize(); i++)
	{
		ActionObj* pActionData = (ActionObj*)m_Actions[i];

		pActionData->GetBreakpointRows(arRows);;
	}
	return __super::GetBreakpointRows(arRows);
}

BOOL Block::ExistsBreakpoint() const
{
	CArray<int> arRows;
	return GetBreakpointRows(arRows);
}

//----------------------------------------------------------------------------
BOOL ConditionalAction::GetBreakpointRows(CArray<int>& arRows) const
{
	if (!m_ThenBlock.IsEmpty())
	{
		m_ThenBlock.GetBreakpointRows(arRows);
	}

	if (!m_ElseBlock.IsEmpty())
	{
		m_ElseBlock.GetBreakpointRows(arRows);
	}

	return __super::GetBreakpointRows(arRows);
}

//----------------------------------------------------------------------------
BOOL WhileLoopAction::GetBreakpointRows(CArray<int>& arRows) const
{
	if (!m_Block.IsEmpty())
	{
		m_Block.GetBreakpointRows(arRows);
	}
	return __super::GetBreakpointRows(arRows);
}

//////////////////////////////////////////////////////////////////////////////