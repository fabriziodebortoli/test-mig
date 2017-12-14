
#pragma once

#include "ActionsRepEngin.h"

//includere alla fine degli include del .H
#include "beginh.dex"

//============================================================================
class WoormTable;
class Parser;
class Unparser;
class Expression;
class Block;
class CEventDlg;
class CSubTotalDlg;
class EventsData;

//============================================================================
class TB_EXPORT OnActionsData : public CObject
{
	DECLARE_DYNAMIC(OnActionsData)
public:
	WoormTable&		m_SymTable;

	Block*	m_pBeforeBlock;
    Block*	m_pAfterBlock;

public:
	OnActionsData(WoormTable&);
	virtual ~OnActionsData();

public:
	BOOL	Parse				(Parser&);
	void	Unparse				(Unparser&);

	void	DispTableChanged	(LPCTSTR pszOldName, LPCTSTR pszNewName);
	void	GetSubtotalFields	(CStringArray&);

	BOOL	CanDeleteField		(LPCTSTR);
	void	DeleteField			(LPCTSTR);
	void	DeleteTable			(LPCTSTR);

	virtual BOOL	IsEmpty		();
	virtual void	Empty		();

	BOOL	SearchNamedAction	(LPCTSTR, ActionType);
	void	DeleteNamedAction	(LPCTSTR, ActionType);
	void	RenameNamedAction	(LPCTSTR pszOld, LPCTSTR pszNew, ActionType);

	// the BOOL parameter means TRUE = before actions, FALSE = after actions
	// the last parameter means the position to add, default (-1) append at the end
	//
	void		AddAction			(ActionType, LPCTSTR, BOOL, int = -1);
	//ActionObj*	GetAction			(ActionType, BOOL);
    int			GetIdxAction		(ActionType, BOOL) const;
};

//============================================================================
class TB_EXPORT ReportEventData : public OnActionsData
{
public:
	Block*			m_pAlwaysBlock;
	Block*			m_pFinalizeBlock;

public:
	ReportEventData	(WoormTable&);
	virtual ~ReportEventData();

public:
	BOOL	Parse				(Parser&);
	void	Unparse				(Unparser&);
	void	DispTableChanged	(LPCTSTR pszOldName, LPCTSTR pszNewName);

	BOOL	CanDeleteField		(LPCTSTR);
	void	DeleteField			(LPCTSTR);
	void	DeleteTable			(LPCTSTR);
	virtual BOOL	IsEmpty				();

	BOOL	SearchNamedAction	(LPCTSTR, ActionType);
	void	DeleteNamedAction	(LPCTSTR, ActionType);
	void	RenameNamedAction	(LPCTSTR pszOld, LPCTSTR pszNew, ActionType);
	
	void	AddActionInAlways	(ActionType, LPCTSTR);
};

//
//	m_bMustTrueTogether :  it is set when The-Break-List and When-Expression must
//						be TRUE together. (it can AND/OR condition)
//
//============================================================================
class TB_EXPORT TriggEventData : public OnActionsData
{
public:
	CString				m_strEventName;
	CStringArray		m_BreakList;
	BOOL				m_bMustTrueTogether;	//<breaking field list> AND/OR <when expr>
	Expression*			m_pWhenExpr;

public:
	TriggEventData(WoormTable&);
	TriggEventData(WoormTable&, LPCTSTR, CStringArray&);
	virtual ~TriggEventData();

public:
	virtual BOOL	IsEmpty();
	virtual void	Empty();

	BOOL		Parse			(Parser&);
	void		Unparse			(Unparser&);
	CString		Unparse			();

	CString		GetEventName		();
	void		SetEventName		(LPCTSTR);

	BOOL		CanConvertFieldToInput (LPCTSTR pszFieldName, CString& sLog) const;

	BOOL		CanDeleteField		(LPCTSTR);
	void		DeleteField			(LPCTSTR);

	BOOL		ParseBreakList	(Parser&);
	BOOL		ParseWhenExpr	(Parser&);

	void		UnparseBreakList(Unparser&);
	void		UnparseWhenExpr	(Unparser&);

	CString		UnparseBreakList();
	void		GetBreakingFields(CStringArray&);

	CString			MakeSubTotalName		(CString strNewName);
	WoormField*		CreateSubTotalField		(const CString& sColumnName, EventsData*);

	BOOL CheckRuleItems (CString& sErr);

	static CString GetEventPrototype();
};

//============================================================================
class TB_EXPORT TableActionData : public OnActionsData
{
public:
	CString				m_strDisplayTableName;

public:
	TableActionData	(WoormTable&);

public:
	BOOL	Parse				(Parser&);
	void	Unparse				(Unparser&);
	void	DispTableChanged	(LPCTSTR pszOldName, LPCTSTR pszNewName);

    CString	GetDisplayTableName		();
	void	SetDisplayTableName		(LPCTSTR);
};

//============================================================================
class TB_EXPORT NewPageActionData : public OnActionsData
{
public:
	NewPageActionData(WoormTable&);

public:
	BOOL	Parse	(Parser&);
	void	Unparse	(Unparser&);
};

//============================================================================
class TB_EXPORT EventsData: public CObject
{
	friend class CEventDlg;
	friend class CSubTotalDlg;

public:
	WoormTable&			m_SymTable;

	ReportEventData*	m_pReportActions;
	NewPageActionData*	m_pNewPageActions;
	Array				m_TableActions;
	Array				m_TriggEvents;

protected:
	CStringArray	m_arCommentTraceBefore;
	CStringArray	m_arCommentTraceInner;
	CStringArray	m_arCommentTraceAfter;

public:
	EventsData(WoormTable&);
	~EventsData();

public:
	BOOL	Parse	(Parser&);
	void	Unparse	(Unparser&);

	TriggEventData*		GetEvent			(LPCTSTR pszEventName);
	int					GetEventIdx			(LPCTSTR pszEventName);
	TableActionData*	GetTableEvent		(LPCTSTR pszTableEventName);
	int					GetTableEventIdx	(LPCTSTR pszTableEventName);

	void	DispTableChanged	(LPCTSTR pszOldName, LPCTSTR pszNewName);

	void	AddActionInAlways	(ActionType, LPCTSTR);
	void	AddActionInNewPage	(ActionType, LPCTSTR, BOOL bInBeforeSection);
	void	AddActionInReport	(ActionType, LPCTSTR, BOOL bInBeforeSection);

	BOOL	CanConvertFieldToInput (LPCTSTR pszFieldName, CString& sLog) const;

	BOOL	CanDeleteField		(LPCTSTR) const;
	void	DeleteField			(LPCTSTR);
	void	DeleteTable			(LPCTSTR);
	BOOL	IsEmpty				();

	int		GetTriggEventsSize	();
	int		GetTableActionsSize	();
	
	void	DeleteNewPageNamedAction		(LPCTSTR pszName, ActionType);
	void	DeleteReportNamedAction			(LPCTSTR pszName, ActionType);
	void	DeleteTableActionsNamedAction	(LPCTSTR pszEventsName, LPCTSTR pszName, ActionType);
	void	DeleteTriggEventsNamedAction	(LPCTSTR pszEventsName, LPCTSTR pszName, ActionType);
	void	RenameNewPageNamedAction		(LPCTSTR pszOldName, LPCTSTR pszNewName, ActionType);
	void	RenameReportNamedAction			(LPCTSTR pszOldName, LPCTSTR pszNewName, ActionType);
	void	RenameTableActionsNamedAction	(LPCTSTR pszEventsName, LPCTSTR pszOldName, LPCTSTR pszNewName, ActionType);
	void	RenameTriggEventsNamedAction	(LPCTSTR pszEventsName, LPCTSTR pszOldName, LPCTSTR pszNewName, ActionType);
	BOOL	SearchReportNamedAction			(LPCTSTR pszName, ActionType);
	BOOL	SearchNewPageNamedAction		(LPCTSTR pszName, ActionType);
	BOOL	SearchTableActionsNamedAction	(int nIndex, LPCTSTR pszName, CString& strEvent, ActionType);
	BOOL	SearchTriggEventsNamedAction	(int nIndex, LPCTSTR pszName, CString& strEvent, ActionType);
};

#include "endh.dex"
