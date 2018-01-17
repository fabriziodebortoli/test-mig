#pragma once

#include <TbGeneric\RDEProtocol.h>
#include <TbGenlib\Command.h>

#include "RepTable.h"

//includere alla fine degli include del .H
#include "beginh.dex"
//============================================================================
class RepEngine;
class Procedure;
class ProcedureObjItem;
class ActionObj;
class Block;

//============================================================================
enum TB_EXPORT ActionType	{
					ACT_NONE,
					ACT_ASSIGN,
					ACT_CONDITIONAL,	ACT_WHILE,
					ACT_CALL,			
					ACT_RESET,			ACT_EVAL,				
					ACT_DISPLAY, ACT_DISPLAY_CHART, ACT_DISPLAY_TABLE_ROW,	ACT_DISPLAY_FREE_FIELDS,
					ACT_FORMFEED,	 	
					ACT_NEXTLINE, ACT_SPACELINE, ACT_INTERLINE, ACT_TITLELINE, ACT_CUSTOM_TITLELINE,
					ACT_ASK,			
					ACT_MESSAGE_BOX, ACT_ABORT, ACT_DEBUG, 
					ACT_QUIT, ACT_BREAK, ACT_CONTINUE, 
					ACT_RETURN, ACT_DO_EXPR,
					ACT_REM,
					ACT_DECLARE
				};

//============================================================================
class CBreakpoint: public CObject, public IDisposingSourceImpl
{
public:
	ActionObj*			m_pAction = NULL;				//The statment that owns the breakpoint

	BOOL				m_bStepOverBreakpoint = FALSE;	//F10 - step one line; It forces to remove breakpoint after fire it
	BOOL				m_bEnabled = TRUE;
	
	Expression*			m_erprCondition = NULL;

	int					m_nHitCount = 0;
	int					m_nActivateAfterHitCount = 0;

	Expression*			m_erprAction = NULL;
	BOOL				m_bContinueExecution = FALSE;

	int						m_nTraceRows = 0;
	CStringArray			m_arTracedNames;
	CArray<CStringArray*>	m_arTracedValues;


	CBreakpoint(ActionObj* pAction) : IDisposingSourceImpl(this), m_pAction (pAction) {}

	virtual ~CBreakpoint() 
		{ 
			SAFE_DELETE(m_erprCondition); SAFE_DELETE(m_erprAction); 

			for (int i = 0; i < m_arTracedValues.GetSize(); i++) 
			{
				CStringArray* pRow = m_arTracedValues[i];
				delete pRow;
			}
		}

	void AddTracedValues(CStringArray* pRow)
	{
		if (m_arTracedValues.GetSize() == m_nTraceRows)
		{
			delete m_arTracedValues[0];
			m_arTracedValues.RemoveAt(0);
		}
		m_arTracedValues.Add(pRow);
	}
};

//============================================================================
class TB_EXPORT ActionObj : public TBScript, public IDisposingSourceImpl
{
	friend class Block;

	DECLARE_DYNAMIC(ActionObj)
public:
	enum ActionState {STATE_NORMAL, STATE_RETURN, STATE_BREAK, STATE_CONTINUE, STATE_ABORT, STATE_QUIT};
private:
	SymTable*		m_pSymTable = NULL;
	RepEngine*		m_pRepEngine = NULL;
	ActionObj*		m_pParent = NULL;	//Parent actionobj (block, if, while, ...)
	int				m_nIndex = -1;		//Index in Block's actions
protected:
	ActionType		m_ActionType = ActionType::ACT_NONE;
	ActionState		m_ActionState = ActionState::STATE_NORMAL;
	CFunctionDescription*	m_pFun = NULL;
public:
	CStringArray	m_arCommentTraceBefore;
	CStringArray	m_arCommentTraceAfter;
	BOOL			m_bAuto = FALSE;	//aggiunto da codice, non va salvato
	
public:
	ActionObj (ActionObj* m_pParent, SymTable* pSymTable, RepEngine* pOwnRepEngine, CFunctionDescription* pFun = NULL);
	virtual ~ActionObj () { ASSERT_VALID(this); RemoveBreakpoint(); }

	CFunctionDescription*	GetFun	() const					{ return m_pFun; }

	virtual SymTable*		GetSymTable() const				    { return m_pSymTable; }	//overridata da Block
	virtual WoormTable*		GetWoormTable () const				{ ASSERT_KINDOF(WoormTable, GetSymTable	()); return (WoormTable*)GetSymTable (); }

	virtual RepEngine*		GetRepEngine () const				{ return m_pRepEngine; }
	
	ActionType				GetActionType () const				{ return m_ActionType; }
	void					SetActionType (ActionType at) 		{ m_ActionType = at; }

	void					SetParent(ActionObj* pParent)		{ m_pParent = pParent; }
	ActionObj*				GetParent() const					{ return  m_pParent; }
	ActionObj*				GetRootParent() const;
	ActionObj*				GetNextAction() const;
	Block*					GetBlockParent() const;

	BOOL					CanRun() const						{ return m_ActionState == STATE_NORMAL; }

	ActionState				GetActionState() const				{ return m_ActionState; }
		
public:
	virtual BOOL	Exec	()													= 0;
	virtual BOOL	CheckBreakpoint ();
	virtual BOOL IsEmpty () const												{ return m_ActionType == ACT_NONE; }

	virtual void DispTableChanged	(LPCTSTR /*pszOld*/, LPCTSTR /*pszNew*/)	{}
	virtual void GetSubtotalFields	(CStringArray& /*arraySubtotals*/)			{}

	virtual	BOOL CanDeleteField		(LPCTSTR)									{ return TRUE; }
	virtual BOOL HasMember			(LPCTSTR sName)	const						{ return ! const_cast<ActionObj*>(this)->CanDeleteField(sName); }
	virtual void DeleteField		(LPCTSTR pszFieldName)						{}
	virtual void DeleteTable		(LPCTSTR pszDispTableName)					{}

	virtual	BOOL Parse				(Parser&)									= 0;
	virtual void Unparse(Unparser&, BOOL /*bNewLine*/ = TRUE, BOOL /*bSkipBeginEnd*/ = FALSE) = 0;
	virtual CString	Unparse();

	virtual LPCTSTR	GetAssociatedName	()										{ return _T(""); }
	virtual void	Rename				(LPCTSTR pszOld, LPCTSTR pszNew)		{ ASSERT(FALSE);}

	virtual ActionObj*	GetActionByRow		(int nRow);	//for UI debug purpouse (nRow is about to the DebugEditView line number stored in m_nDebugUnparseRow)
	virtual BOOL		GetBreakpointRows	(CArray<int>& arRows) const;	//for UI debug purpouse (nRow is about to the DebugEditView line number stored in m_nDebugUnparseRow)

protected:
	BOOL	Fail	(LPCTSTR szError = NULL);
	BOOL	SetRTError (int MessageID, const CString& = _T(""), LPCTSTR = NULL);

	//RS-DEBUGGER
private:
	CBreakpoint*	m_pBreakpoint = NULL;
public:
	int				m_nDebugUnparseRow = -1;	//for UI debug purpouse, it is the DebugEditView line number

	BOOL			HasBreakpoint	() const { return m_pBreakpoint != NULL; }
	CBreakpoint*	GetBreakpoint	() { return m_pBreakpoint; }
	void			AddBreakpoint	(BOOL bStepOver = FALSE);
	void			RemoveBreakpoint();
	void			DeletePtrBreakpoint() { SAFE_DELETE(m_pBreakpoint); }

// diagnostics
#ifdef _DEBUG
public:
	virtual void Dump(CDumpContext&) const;
#endif
};

//============================================================================
class TB_EXPORT Block: public ActionObj
{
	friend class WhileLoopAction;
	friend class ConditionalAction;
	friend class Procedure;

	DECLARE_DYNAMIC(Block)
protected:
	SymTable*	m_pLocalSymTable = NULL;
	BOOL		m_bIsRuleScope = FALSE;

	BOOL		m_bHasBeginEnd = TRUE;
	BOOL		m_bRaiseEvents = FALSE;
public:
	Array		m_Actions;			// Array of Actions
	CString		m_strOwnerName;
public:
	Block (ActionObj* pParent, SymTable*, RepEngine*, BOOL bRaiseEvents = TRUE, CFunctionDescription* pFun = NULL);
	virtual ~Block ();

	void	SetFun(CFunctionDescription* f);

	virtual SymTable* GetSymTable	() const { return m_pLocalSymTable ? m_pLocalSymTable : __super::GetSymTable(); }

	virtual BOOL	Exec ();
	
	BOOL	IsEmpty			()	const						{ return m_Actions.GetSize() == 0 && !m_bHasBeginEnd; }
	int		GetCount		() const						{ return m_Actions.GetSize(); }

	ActionObj*	GetAction	(int idx) const;
	void		AddAction		(ActionObj* pAction)			{ int pos = m_Actions.Add(pAction);	pAction->m_nIndex = pos; pAction->SetParent(this); }
	void		InsertActionAt	(int pos, ActionObj* pAction)	{ m_Actions.InsertAt(pos, pAction);	pAction->m_nIndex = pos; pAction->SetParent(this); }

	void	SetForceBeginEnd	(BOOL b = TRUE)				{ m_bHasBeginEnd = b; }
	BOOL	GetForceBeginEnd    ()	 const					{ return m_bHasBeginEnd ; }
	BOOL	Parse(Parser&);
	BOOL	IsEmptyCommands	()	const				{ return m_Actions.GetSize() == 0 ; }
	void	Empty();

private:
	//ActionObj*	ParseDisplayTableAction	(Parser&, IRDEManager::Command);
	ActionObj*	ParseAction				(Parser&);

public:
	BOOL	Parse				(CString strBlock);
	virtual void	Unparse(Unparser&, BOOL bNewLine = TRUE, BOOL bSkipBeginEnd = FALSE);
	virtual CString	Unparse() { return __super::Unparse(); }

	virtual ActionObj*			GetActionByRow		(int nRow);
	virtual BOOL				GetBreakpointRows	(CArray<int>& arRows) const;
			BOOL				ExistsBreakpoint	() const;	//Exists at least one breakpoint

	void	DispTableChanged	(LPCTSTR pszOldName, LPCTSTR pszNewName);
	void	GetSubtotalFields	(CStringArray&);

	BOOL	CanDeleteField		(LPCTSTR);
	void	DeleteField			(LPCTSTR);
	void	DeleteTable			(LPCTSTR);

	BOOL	SearchNamedAction	(LPCTSTR, ActionType);
	void	DeleteNamedAction	(LPCTSTR, ActionType);
	void	RenameNamedAction	(LPCTSTR pszOld, LPCTSTR pszNew, ActionType);
	
	// the last parameter means the position to add, default (-1) append at the end
	void		AddAction		(ActionType, LPCTSTR, int pos);
	int			GetIdxAction	(ActionType) const ;
	//----
	void		AddLocalField	(WoormField*);
	//----
	BOOL		SetRuleScope(BOOL set = TRUE) { BOOL old = m_bIsRuleScope; m_bIsRuleScope = set; return old; }
	BOOL		IsRuleScope() const { return m_bIsRuleScope; }
};

//============================================================================
class TB_EXPORT BoolBlock: public Block
{
	DECLARE_DYNAMIC(BoolBlock)
private:
	CFunctionDescription		m_Fun;
public:
	BoolBlock (ActionObj* m_pParent, SymTable*);
	//virtual ~BoolBlock ();
	virtual BOOL	Exec (DataBool&);
};

//============================================================================
class TB_EXPORT AssignAction : public ActionObj
{
	DECLARE_DYNAMIC(AssignAction)
private:
	SymField*		m_pField;

	Expression*		m_pExpr;
	Expression*		m_pIndexerExpr;

	CString			m_sRecordFieldName;

public:
	AssignAction (ActionObj* m_pParent, SymTable*, RepEngine*);
	~AssignAction ();

public:
	virtual LPCTSTR	GetAssociatedName	() { return m_pField ? m_pField->GetName() : _T(""); }

	BOOL	Exec	();
	BOOL	Parse	(Parser&);
	void	Unparse(Unparser&, BOOL bNewLine = TRUE, BOOL bSkipBeginEnd = FALSE);

    BOOL	IsEmpty				() const ;

	BOOL	CanDeleteField		(LPCTSTR);
	void	DeleteField			(LPCTSTR);
};

//============================================================================
class TB_EXPORT ConditionalAction : public ActionObj
{
	DECLARE_DYNAMIC(ConditionalAction)
private:
	Expression	m_ConditionExpr;
	Block		m_ThenBlock;
	Block		m_ElseBlock;

public:
	ConditionalAction (ActionObj* m_pParent, SymTable*, RepEngine*, CFunctionDescription* pFun);

public:
	BOOL	Exec	();
	BOOL	Parse	(Parser&);
	void	Unparse(Unparser&, BOOL bNewLine = TRUE, BOOL bSkipBeginEnd = FALSE);

    BOOL	IsEmpty	() const ;

	void	DispTableChanged	(LPCTSTR pszOldName, LPCTSTR pszNewName);

	BOOL	CanDeleteField		(LPCTSTR);
	void	DeleteField			(LPCTSTR);
	void	DeleteTable			(LPCTSTR);

	virtual ActionObj*		GetActionByRow		(int nRow);
	virtual BOOL			GetBreakpointRows	(CArray<int>& arRows) const;
};

//============================================================================
class TB_EXPORT WhileLoopAction : public ActionObj
{
	DECLARE_DYNAMIC(WhileLoopAction)
private:
	Expression	m_ConditionExpr;
	Block		m_Block;

public:
	WhileLoopAction (ActionObj* m_pParent, SymTable*, RepEngine*, CFunctionDescription* pFun);

public:
	BOOL	Exec	();
	BOOL	Parse	(Parser&);
	void	Unparse(Unparser&, BOOL bNewLine = TRUE, BOOL bSkipBeginEnd = FALSE);

	BOOL	IsEmpty				() const;

	void	DispTableChanged	(LPCTSTR pszOldName, LPCTSTR pszNewName);

	BOOL	CanDeleteField		(LPCTSTR);
	void	DeleteField			(LPCTSTR);
	void	DeleteTable			(LPCTSTR);

	virtual ActionObj*	GetActionByRow		(int nRow);
	virtual BOOL		GetBreakpointRows	(CArray<int>& arRows) const;
};

//============================================================================
//DISPLAY, DISPLAY_FREE_FIELDS, DISPLAY_TABLE_ROW
class TB_EXPORT DisplayFieldsAction : public ActionObj
{
	DECLARE_DYNAMIC(DisplayFieldsAction)
private:
	CObArray    m_Fields;	// array of WoormField* property of WoormTable
  	CString		m_sTableName;

public:
	DisplayFieldsAction (ActionObj* m_pParent, WoormTable*, RepEngine*);

public:
	virtual LPCTSTR	GetAssociatedName	() { return m_sTableName; }

	BOOL	Exec	();
	BOOL	Parse	(Parser& parser);
	void	Unparse	(Unparser&, BOOL bNewLine = TRUE, BOOL bSkipBeginEnd = FALSE);

    BOOL	IsEmpty				() const;
	BOOL	CanDeleteField		(LPCTSTR);
	void	DeleteField			(LPCTSTR);
	void	DeleteTable			(LPCTSTR pszDispTableName);

	//BOOL	Parse	(Token, Parser&);

	void	AddField		(/*WoormField*/CObject* pDF) { m_Fields.Add(pDF); }
	BOOL	ExistColumnOf	(DisplayTableEntryEngine*);

    void	AddDisplayName		(LPCTSTR);
	void	GetSubtotalFields	(CStringArray&);
};

//============================================================================
//INTERLINE, NEXTLINE, SPACELINE
class TB_EXPORT DisplayTableAction : public ActionObj
{
	DECLARE_DYNAMIC(DisplayTableAction)
public:
	IRDEManager::Command		m_RdeCommand; //INTER_LINE, NEXT_LINE, TITLE_LINE, CUSTOM_TITLE_LINE
	DisplayTableEntry*			m_pDisplayTable = NULL;
	CString						m_sTableName;
	Expression*					m_pCustomTitle = NULL;
public:
	DisplayTableAction (ActionObj* m_pParent, WoormTable*, RepEngine*);
	DisplayTableAction (ActionObj* m_pParent, RepEngine*, IRDEManager::Command, DisplayTableEntryEngine*);

public:
	virtual LPCTSTR	GetAssociatedName	() { return m_sTableName; }

	BOOL	Exec	();
	BOOL	Parse	(Parser& parser);
	void	Unparse(Unparser&, BOOL bNewLine = TRUE, BOOL bSkipBeginEnd = FALSE);

	void DeleteTable (LPCTSTR pszDispTableName);

	virtual void SetTableName(LPCTSTR pszNewName);
};

//============================================================================
//DISPLAY_CHART

class TB_EXPORT DisplayChartAction : public ActionObj
{
	DECLARE_DYNAMIC(DisplayChartAction)
private:
	CString						m_sChartName;
public:
	DisplayChartAction(ActionObj* m_pParent, WoormTable*, RepEngine*);

public:
	virtual LPCTSTR	GetAssociatedName() { return m_sChartName; }

	BOOL	Exec();
	BOOL	Parse(Parser& parser);
	void	Unparse(Unparser&, BOOL bNewLine = TRUE, BOOL bSkipBeginEnd = FALSE);

	void DeleteTable(LPCTSTR pszChartName);

	virtual void SetChartName(LPCTSTR pszNewName);
};

//============================================================================
class TB_EXPORT FormFeedAction : public ActionObj
{
	DECLARE_DYNAMIC(FormFeedAction)

private:
	BOOL	m_bCalledFromOnTable;
	BOOL	m_bForced;
public:
	CString	m_sLayoutName;

public:
	FormFeedAction (ActionObj* m_pParent, WoormTable*, RepEngine*);

	BOOL	Exec	();
	BOOL	Parse	(Parser&);
	void	Unparse(Unparser&, BOOL bNewLine = TRUE, BOOL bSkipBeginEnd = FALSE);
};

//============================================================================
class TB_EXPORT EvalResetAction : public ActionObj
{
	DECLARE_DYNAMIC(EvalResetAction)
private:
	WoormField*		m_pField;

public:
	EvalResetAction (ActionObj* m_pParent, WoormTable*, RepEngine*);

public:
	virtual LPCTSTR	GetAssociatedName	() { return m_pField ? m_pField->GetName() : _T(""); }

	BOOL	Exec	();
	BOOL	Parse	(Parser&);
	void	Unparse(Unparser&, BOOL bNewLine = TRUE, BOOL bSkipBeginEnd = FALSE);

	void DeleteField	(LPCTSTR pszFieldName);

	BOOL SetFieldName	(LPCTSTR strFieldName, Parser* parser = NULL);
};

//============================================================================
class TB_EXPORT CallAction : public ActionObj
{
	DECLARE_DYNAMIC(CallAction)
private:
	//ProcedureObjItem*	m_pProcedure;	// element of WoormTable::m_Procedures
	CString		m_sName;
	//BOOL		m_bForce = FALSE; //allows forward declaration
public:
	CallAction (ActionObj* m_pParent, WoormTable*, RepEngine*);

public:
	virtual LPCTSTR	GetAssociatedName	(); // { return m_pProcedure ? m_pProcedure->GetName() : _T(""); }
	BOOL	Exec	();
	BOOL	Parse	(Parser&);
	void	Unparse(Unparser&, BOOL bNewLine = TRUE, BOOL bSkipBeginEnd = FALSE);
};

//============================================================================
class TB_EXPORT AskDialogAction : public ActionObj
{
	DECLARE_DYNAMIC(AskDialogAction)
private:
	CString			m_strDialogName;
	CStringArray	m_allDialogName;

public:
	AskDialogAction (ActionObj* m_pParent, WoormTable*, RepEngine*);

public:
	virtual LPCTSTR	GetAssociatedName	() { return m_strDialogName; }
	BOOL	Exec	();
	BOOL	Parse	(Parser&);
	void	Unparse(Unparser&, BOOL bNewLine = TRUE, BOOL bSkipBeginEnd = FALSE);
};

//============================================================================
class TB_EXPORT MessageBoxAction : public ActionObj
{
	DECLARE_DYNAMIC(MessageBoxAction)
private:
	Expression		m_Message;
	CString			m_sIcon;

public:
	MessageBoxAction (ActionObj* m_pParent, SymTable*, RepEngine*);

public:
	BOOL	Exec	();
	BOOL	Parse	(Parser&);
	void	Unparse(Unparser&, BOOL bNewLine = TRUE, BOOL bSkipBeginEnd = FALSE);

	BOOL	IsEmpty () const { return m_Message.IsEmpty(); }
};

//============================================================================
class TB_EXPORT QuitBreakContinueAction : public ActionObj
{
	DECLARE_DYNAMIC(QuitBreakContinueAction)
public:
	QuitBreakContinueAction (ActionObj* m_pParent, SymTable*, RepEngine*);

public:
	BOOL	Exec	();
	BOOL	Parse	(Parser&);
	void	Unparse(Unparser&, BOOL bNewLine = TRUE, BOOL bSkipBeginEnd = FALSE);
};

//============================================================================
class TB_EXPORT RemAction : public ActionObj
{
	DECLARE_DYNAMIC(RemAction)
public:
	RemAction (ActionObj* m_pParent, SymTable*, RepEngine*);

	CString		m_sRemarkText;
public:
	BOOL	Exec	();
	BOOL	Parse	(Parser&);
	void	Unparse(Unparser&, BOOL bNewLine = TRUE, BOOL bSkipBeginEnd = FALSE);
};

//============================================================================
class TB_EXPORT DoExprAction : public ActionObj
{
	DECLARE_DYNAMIC(DoExprAction)
private:
	Expression	m_Expr;
public:
	DoExprAction (ActionObj* m_pParent, SymTable*, RepEngine*);

public:
	BOOL	Exec		();
	BOOL	Parse		(Parser&);
	void	Unparse(Unparser&, BOOL bNewLine = TRUE, BOOL bSkipBeginEnd = FALSE);
	BOOL	IsEmpty() const { return m_Expr.IsEmpty(); }
};

//============================================================================
class TB_EXPORT ReturnAction : public ActionObj
{
	DECLARE_DYNAMIC(ReturnAction)
private:
	Expression	m_ReturnExpr;

public:
	ReturnAction (ActionObj* m_pParent, SymTable*, RepEngine*, CFunctionDescription* pFun);

public:
	BOOL	Exec	();
	BOOL	Parse	(Parser&);
	void	Unparse(Unparser&, BOOL bNewLine = TRUE, BOOL bSkipBeginEnd = FALSE);
};

//============================================================================
class TB_EXPORT DeclareAction : public ActionObj
{
	DECLARE_DYNAMIC(DeclareAction)
private:
	Block*			m_pScopeBlock;
	WoormField*		m_pLocalField;
	Expression		m_InitExpr;

public:
	DeclareAction (ActionObj* m_pParent, SymTable*, RepEngine*, Block*);

public:
	BOOL	Exec	();
	BOOL	Parse	(Parser&);
	void	Unparse(Unparser&, BOOL bNewLine = TRUE, BOOL bSkipBeginEnd = FALSE);
};

//============================================================================
class TB_EXPORT Procedure : public CObject
{
	friend class ProcedureObjItem;

	DECLARE_DYNAMIC(Procedure)
public:
	CString					m_strName;
	Block*					m_pBlock;
protected:
	BOOL					m_bParsed;
	CFunctionDescription*	m_pFun;

public:
	Procedure (const CString&, WoormTable*, RepEngine* = NULL);
	virtual ~Procedure ();

	CString GetName () const { return m_strName; }
	void		SetName (const CString& name);

	BOOL	IsEmpty() const { return m_pBlock->IsEmpty(); }
	SymTable*	GetSymTable() const { return m_pBlock->GetSymTable(); }

	CFunctionDescription* GetFun() const { return m_pBlock->GetFun(); }
	void SetFun (CFunctionDescription* pF);

	BOOL	Exec	();                 
	BOOL	Parse	(Parser&);
	void	Unparse (Unparser& oFile, BOOL bSkipHeader = FALSE, BOOL bSkipBeginEnd = FALSE);

private:
	BOOL	m_bCalledFromOnFormFeed;
	BOOL	m_bCalledFromOnTable;

#ifdef _DEBUG
public:
	void AssertValid() const;
	void Dump(CDumpContext& dc) const;
#endif //_DEBUG
};

//============================================================================
#include "endh.dex"
