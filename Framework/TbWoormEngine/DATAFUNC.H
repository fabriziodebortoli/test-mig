
#pragma once

#include <TbParser\SymTable.h>
#include <TbGenlib\expr.h>

//includere alla fine degli include del .H
#include "beginh.dex"

class WoormField;
class WoormTable;
class WoormTable;
class QueryEngine;
class Block;

//============================================================================
class TB_EXPORT FunctionData : public Expression
{
	DECLARE_DYNAMIC(FunctionData)

private:
	Token			m_FunctionType;
	CString			m_strPublicName;

public:
	FunctionData(SymTable*, Token = T_NOTOKEN);
	FunctionData(SymTable*, LPCTSTR pszExpression, const DataType&);

public:
	BOOL		Parse(Parser&, const DataType&);
	BOOL		ParseExpression(Parser&, const DataType&);
	BOOL		ParseFunction(Parser&, const DataType&);

	CString		GetPublicName() const { return m_strPublicName; }
	void		SetPublicName(LPCTSTR szName) { m_strPublicName = szName; }
	BOOL		IsAnExpression() const { return m_FunctionType == T_NOTOKEN; }
	Token		GetFunctionType() const { return m_FunctionType; }

	BOOL		CanDeleteField(LPCTSTR);
	void		DeleteField(LPCTSTR);
	BOOL		IsEmpty();
	void		Reset();
	void		Unparse(Unparser&);

	BOOL		CheckRuleItems(CString& sErr);

private:
	void	UnparseFunction(Unparser&);
	void	UnparseExpression(Unparser&);
	void	UnparsePublicName(Unparser&);
};

//============================================================================

class TB_EXPORT DataFunction : public Expression      // only for EVAL function
{
private:
    WoormField*		m_pFuncItem = NULL;
    long			m_nOccurrence = 0;         //per gestire AVG/Count/First

protected:
    Token			m_FunctionType;

public:
    DataFunction (SymTable*);
	DataFunction (const DataFunction&);

    BOOL        IsAnExpression  () const	{ return m_FunctionType == T_NOTOKEN; }
	Token		GetFunctionType	() const	{ return m_FunctionType; }
	void		SetFunctionType	(Token tk)	{ m_FunctionType = tk; }
    WoormField*	GetFuncItem     () const	{ return m_pFuncItem; }

    virtual DataObj&    GetData     ()				const   = 0;
    virtual DataObj&    GetAccData  ()				const   = 0;
    virtual BOOL        IsUpdated   (WoormField&)	const   = 0;
    virtual void        SetAccData  (DataObj&)				= 0;

            BOOL        Eval			();

			void		ResetOccurrence	() { m_nOccurrence = 0;}
			void		IncOccurrence	() { m_nOccurrence++; }

            BOOL        ParseFunction   (Parser&, DataObj* pAccData);
            BOOL        ParseExpression (Parser&, WoormField*);
};

//============================================================================
class TB_EXPORT EventFunction : public DataFunction
{
private:
	WoormField&	m_AccumulatorField;
	CString		m_strPublicName;

public:
	EventFunction (SymTable*, WoormField&);
	EventFunction (SymTable*, WoormField&, LPCTSTR pszExpression, const DataType&);
	EventFunction (const EventFunction&);

public:
	virtual DataObj&		GetData		() const;
	virtual DataObj&		GetAccData	() const;
	virtual BOOL			IsUpdated	(WoormField&)	const;
	virtual void			SetAccData	(DataObj&);

	virtual CString		ToString	(BOOL bUseAudit = TRUE);
	virtual	void		Reset		(BOOL bResetAll = TRUE);
			BOOL			Parse		(Parser&);

	CString		GetPublicName	() const			{ return m_strPublicName;	}
	void		SetPublicName	(LPCTSTR szName)	{ m_strPublicName = szName;	}

	BOOL		CanDeleteField	(LPCTSTR);
	void		DeleteField		(LPCTSTR);
	BOOL		IsEmpty			();

	void		Unparse			(Unparser&);
private:
	void	UnparseFunction		(Unparser&);
	void	UnparseExpression	(Unparser&);
	void	UnparsePublicName	(Unparser&);
};

//============================================================================
class TB_EXPORT GroupFunction : public DataFunction
{
public:
	GroupFunction (SymTable* pSymTable)
		:
		DataFunction	(pSymTable)
		{}

	virtual DataObj&	GetData		() const;

	virtual DataObj&	GetAccData	() const;

	virtual BOOL		IsUpdated	(WoormField& item) const;

	virtual void		SetAccData	(DataObj&);

			BOOL		Parse		(Parser&);

			BOOL		CheckRuleItems (CString& sErr);
};

//=============================================================================
//	ExpressionWithCheck
//=============================================================================
class TB_EXPORT ExpressionWithCheck : public Expression
{
	DECLARE_DYNAMIC(ExpressionWithCheck);

public:
	BOOL m_bSkipCheck = FALSE;

	ExpressionWithCheck(SymTable* pTable) : Expression(pTable) {}

	virtual BOOL Parse(Parser&, const DataType&, BOOL bKeepString = FALSE);

	BOOL CheckVariables(Parser& lex, const Stack* = NULL);

	ExpressionWithCheck& operator = (const ExpressionWithCheck& aExp) { Assign(aExp); return *this; }
};

//=============================================================================
//	Grouping Rule
//=============================================================================
class DebugBlock;

class TB_EXPORT GroupBy : public ExpressionWithCheck
{
	DECLARE_DYNAMIC(GroupBy);
private:
	QueryEngine*	m_pOwnerEngine = NULL;
	Array			m_GroupFunctions;			// array of  GroupFunction
	CString			m_strOldValue;
	BOOL			m_bForce = FALSE;			//it force to allows all fields
public:
	//only for breakpoint of RS-DEBUGGER
	DebugBlock*		m_pBefore = NULL;
	DebugBlock*		m_pAfter = NULL;

public:
	GroupBy (QueryEngine*);
	virtual ~GroupBy();

			BOOL		Init		(QueryEngine*);

			BOOL		IsChanged	();

			BOOL		EvalFun		(BOOL bOnlyExpr = FALSE);

			void		ResetOccurrence();

			BOOL		Parse		(Parser&);

			BOOL		IsTemporaryRequired	(CObArray&);

			BOOL		CheckRuleItems (CString& sErr);

private:
			BOOL		Eval				(CString&);
			
			BOOL		CheckRuleGraph		(CObArray&, const Stack& aStack);
};

//=============================================================================
//	Having tuples filter
//=============================================================================
class DebugBlock;

class TB_EXPORT ExpressionWithDebugBlocks : public Expression	//?? ExpressionWithCheck
{
	DECLARE_DYNAMIC(ExpressionWithDebugBlocks);
public:
	//only for breakpoint of RS-DEBUGGER
	DebugBlock*		m_pBefore = NULL;
	DebugBlock*		m_pAfter = NULL;
public:
	ExpressionWithDebugBlocks(QueryEngine*);
	virtual ~ExpressionWithDebugBlocks();

	virtual BOOL Eval(DataObj&);
};

//============================================================================
#include "endh.dex"
