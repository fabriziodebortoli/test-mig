
#pragma once

#include "ruledata.h"

//includere alla fine degli include del .H
#include "beginh.dex"

//============================================================================
class Parser;
class Unparser;
class CGroupByDlg;
class WoormTable;
class FunctionData;
class SqlConnection;

//============================================================================
class TB_EXPORT GroupByAction : public CObject
{
protected:
	CString			m_strPublicName;
    FunctionData*	m_pFunction;

public:
	GroupByAction();
	~GroupByAction();

public:
	BOOL	Parse		(Parser&, WoormTable*);
	void	Unparse		(Unparser&);

	BOOL	CanDeleteField	(LPCTSTR);
	void	DeleteField		(LPCTSTR);
	BOOL	IsEmpty			();

	BOOL		CheckRuleItems(CString& sErr) { return m_pFunction ? m_pFunction->CheckRuleItems(sErr) : TRUE; }
};

//============================================================================
class TB_EXPORT GroupByData: public CObject
{
	friend	class CGroupByDlg;

public:
	WoormTable&					m_SymTable;

	Expression*					m_pTupleFilter;
	ExpressionWithCheck*		m_pGroupingTuple;
	BOOL						m_bForce = FALSE;	//it force to allows all fields
	Array						m_ActionsArray;
	Expression*					m_pHavingTupleFilter;
public:
	GroupByData(WoormTable&);
	~GroupByData();

public:
	BOOL	Parse					(Parser&);
		BOOL	ParseActionsBlock	(Parser&);
		BOOL	ParseAction			(Parser&);

	void	Unparse			(Unparser&);
		CString  Unparse();
	void	UnparseActions	(Unparser&);
		CString  UnparseActions ();

	void	Empty	();
	BOOL	IsEmpty	();

	BOOL	CanDeleteField	(LPCTSTR);
	void	DeleteField		(LPCTSTR);

	BOOL	CanConvertFieldToInput(LPCTSTR pszFieldName, CString& sLog) const;

	BOOL	CheckRuleItems(CString& sErr);
};

//============================================================================
class TB_EXPORT QueryData : public CObject
{
	friend class	CGroupByDlg;

public:
	WoormTable&	m_SymTable;

	RuleDataArray	m_RuleData;

	GroupByData*	m_pGroupBy;

public:
	QueryData(WoormTable&);
	~QueryData();

public:
	RuleDataObj*	GetRule		(int index);
	int				GetNumRules	();
	RuleDataArray*		GetRuleData	();

	void 			SetGroupBy	(GroupByData* bNewGroupBy);

	BOOL	IsEmpty			();
	BOOL	CanDeleteField	(LPCTSTR) const ;
	void	DeleteField		(LPCTSTR);
	void	RenameField		(LPCTSTR, LPCTSTR);

	BOOL	CanConvertFieldToInput (LPCTSTR pszFieldName, CString& sLog) const;

	BOOL	Parse		(Parser&, SqlConnection*);
	void	Unparse		(Unparser&);
};
#include "endh.dex"
