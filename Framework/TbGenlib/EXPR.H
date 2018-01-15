
#pragma once

#include <TbGeneric\DataObj.h>
#include <TbParser\Parser.h>

#include "expparse.h"

//includere alla fine degli include del .H
#include "beginh.dex"

//=============================================================================
class Parser;

//=============================================================================
typedef	DataType DATA_TYPE_OP_MAP[LAST_MAPPED_DATA_TYPE+1][LAST_MAPPED_DATA_TYPE+1];


//=============================================================================
//	Expression
//=============================================================================
class TB_EXPORT Expression : public CObject, public IDisposingSourceImpl
{
	DECLARE_DYNAMIC (Expression)

public:
	enum MessageID {
			EMPTY_MESSAGE = 0,
			PARAM,
			UNKNOW,
			EXPREMPTY,
			RETTYPE,
			NOSYMTABLE,
			UNKNOWN_FIELD,
			UNKNOWN_DB_COL_FIELD,
			CHECKTYPE,
			NULL_OPR,
			UNKNOWN_CALLER_DOC,
			UNKNOWN_EXTERNAL_FUNC,
			EXTERNAL_FUNC_ERROR,
			DIVISION_BY_ZERO,
			FORMAT_TYPEERR,
			FORMAT_UNKNOWN,
			SYNTAX_ERROR,
			EXTERNAL_FUNC_MISSING_ROUNDCLOSE,
			EXTERNAL_FUNC_MISSING_COMMA,
			EXTERNAL_FUNC_TOOMANY_PARAMETERS,
			EXTERNAL_FUNC_MISSING_PARAMETERS,
			EXTERNAL_FUNC_BAD_PARAMETER,
			FORBIDDEN_FIELD
	};

protected:
	CString		m_strExprString;
	CString		m_strAuditString;

	Stack		m_ExprStack;
	SymTable*	m_pSymTable;
	CStopTokens*	m_pStopTokens;

	int			m_nErrorPos;
	MessageID	m_nErrorID;
	int			m_nParseStartLine;
	CString		m_sErrorDetail;

	BOOL		m_bVrbCompiled;
	BOOL		m_bHasExternalFunctionCall;
	BOOL		m_bHasRuleFields;
	BOOL		m_bHasDynamicFragment;	//ContentOf(...), {EVAL <sub-expr> }

	// Operator compatibility maps.
	static	DATA_TYPE_OP_MAP	PlusMap;
	static	DATA_TYPE_OP_MAP	MinusMap;
	static	DATA_TYPE_OP_MAP	StarMap;
	static	DATA_TYPE_OP_MAP	SlashMap;
	static	DATA_TYPE_OP_MAP	PercMap;
	static	DATA_TYPE_OP_MAP	RelMap;

public:
	Expression(SymTable*);
	Expression(const Expression& aExp);
	virtual ~Expression() { SAFE_DELETE(m_pStopTokens); }

public:
	virtual	void	Reset			(BOOL bResetAll = TRUE);
	virtual	void	Empty			() { Reset(FALSE); }

	virtual BOOL	Eval			(DataObj&);
	virtual BOOL	Eval			(DataObj*& p);
	
	void		Assign				(const Expression&);
	void		Assign				(SymTable* pSymTable) { m_pSymTable = pSymTable; }
	void		AssignStr			(CString str) { m_strExprString = str; }

	SymTable*	GetSymTable			() const { return m_pSymTable; }
	void		SetSymTable			(SymTable* pSymTable) { m_pSymTable = pSymTable; }

	virtual BOOL		HasMember			(LPCTSTR sVarName) const;
	virtual void		GetParameters		(CStringArray& arParameters, BOOL bReverse = TRUE) const;

	virtual BOOL		Parse				(Parser&,		const DataType&, BOOL bKeepString = FALSE);
	virtual BOOL		Parse				(CString sExpr, const DataType&, BOOL bKeepString = FALSE);

	//BOOL		Unparse				(CString&, BOOL bUseAudit = FALSE);

	DataType	CompileOK			(Parser&, Stack&);
	ExpItemVal* EvalOK				(const Stack&);

	CString GetErrDescription		(BOOL bVerbouse = TRUE);
	const MessageID	GetErrId		() { return m_nErrorID; }

	BOOL		IsEmpty				() const  { return m_ExprStack.IsEmpty(); }
    BOOL		IsEqual				(const Expression&)		const;
	void		SetStopTokens		(Token, Token = T_NOTOKEN, Token = T_NOTOKEN, Token = T_NOTOKEN, Token = T_NOTOKEN);
	void		AddStopTokens		(Token st1);
	void		SetStopTokens		(Token arst[], int nSize);
	void		SetCompileVrb		(BOOL bFlag = TRUE) 	{ m_bVrbCompiled = bFlag; }

//	BOOL		Unparse				(CString&, BOOL bUseAudit = FALSE);
	virtual CString	ToString	(BOOL bUseAudit = TRUE);
	//operator LPCTSTR	() const;

	BOOL			operator ==			(const Expression& aExp)	const { return IsEqual(aExp); }
	BOOL			operator !=			(const Expression& aExp)	const { return !IsEqual(aExp); }
	Expression&		operator =			(const Expression& aExp)	{ Assign(aExp); return *this; }

	CStopTokens*	GetStopTokens() { return m_pStopTokens; }

	BOOL		HasExternalFunctionCall	() const { return m_bHasExternalFunctionCall; }
	BOOL		HasRuleFields			() const { return m_bHasRuleFields; }
	BOOL		HasDynamicFragment		() const { return m_bHasDynamicFragment; }

	const Stack* GetStack() const { return &m_ExprStack; }

	static BOOL	InitExpressionMaps	();
	static CString FormatMessage	(MessageID ID);

protected:
	virtual BOOL		Execute			(Stack&);
	virtual BOOL		Compile			(Parser&, Stack&, Stack&);
	virtual ExpItemVal* ResolveSymbol	(ExpItemVrb&);

	virtual DataType	GiveMeResultTypeForMathOpMap	(DATA_TYPE_OP_MAP&, ExpItemVal*, ExpItemVal*);
	virtual DataType	GiveMeResultTypeForBitwiseOp	(ExpItemVal*, ExpItemVal* = NULL);
	virtual DataType	GiveMeResultTypeForLogicalOp	(ExpItemVal*, ExpItemVal* = NULL);
	virtual DataType	GiveMeResultTypeForLikeOp		(ExpItemVal*, ExpItemVal*);

	virtual ExpItemVal*	OnApplyFunction					(ExpItemFun*, Stack&) { return NULL; }

protected:
	ExpItemVal*	GiveMeResult			(ExpItemOpe*, ExpItemVal*);
	ExpItemVal*	GiveMeResult			(ExpItemOpe*, ExpItemVal*, ExpItemVal*);
	ExpItemVal*	GiveMeResult			(ExpItemOpe*, ExpItemVal*, ExpItemVal*, ExpItemVal*);
	ExpItemVal*	ApplyFunction			(ExpItemFun*, Stack&);
	ExpItemVal*	ApplyExternalFunction	(ExpItemExternalFun*, Stack*);
	DataLng		ApplyExternalFunctionWrp	(DataLng dlItemFun, DataLng dlParamStack);
	DataBool	CallWebMethodWrp			(DataLng dlFun);

	BOOL		HasMember				(LPCTSTR sVarName, const Stack& stack) const;
	void		GetParameters			(CStringArray&, const Stack& stack, BOOL bReverse = TRUE) const;

	BOOL		ErrHandler(MessageID, ExpItem* = NULL, ExpItem* = NULL, ExpItem* = NULL);

	CString		CastStr			(ExpItemVal&);
	CString		CastTxt			(ExpItemVal&);
	BOOL 		CastBool		(ExpItemVal&);
	int			CastInt			(ExpItemVal&);
	long		CastLng			(ExpItemVal&);
	double		CastDbl			(ExpItemVal&);
	GUID		CastGuid		(ExpItemVal&);
	DataArray*	CastArray		(ExpItemVal&);
	DataRecord*		CastRecord		(ExpItemVal&);
	DataSqlRecord*	CastSqlRecord	(ExpItemVal&);
	SymField*	CastFieldProvider (ExpItemVal&);
	//IDataProvider* CastProvider (ExpItemVal&);

	void		AssignResult	(DataObj&, ExpItemVal&);
	ExpItemVal*	ReturnFunDataObj(int);

	ExpItemVal*	CheckType		(Parser&, ExpItemOpe*, ExpItemVal*, ExpItemVal* = NULL, ExpItemVal* = NULL);
	DataType	GiveMeResultType(ExpItemOpe*, ExpItemVal*, ExpItemVal*, ExpItemVal*);

	static BOOL		Compatible		(const DataType& dtFrom, const DataType& dtTo)
						{ return DataType::IsCompatible(dtFrom, dtTo); }

	static double 	Round			(double val, int nDec = 0);
	static void		Space			(CString&, int count);
	static void		Replicate		(CString&, int count, const CString& rep);

// diagnostics
#ifdef _DEBUG
public:	
	virtual void Dump(CDumpContext&) const;
	virtual void AssertValid() const;
#endif

};

//////////////////////////////////////////////////////////////////////////////
#include "endh.dex"
