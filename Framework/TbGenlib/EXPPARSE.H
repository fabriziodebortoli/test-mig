
#pragma once

#include <TbGeneric\DataObj.h>
#include <TbGeneric\stack.h>

#include <TbParser\Parser.h>
#include <TbParser\SymTable.h>
#include <TbParser\TokensTable.h>

//includere alla fine degli include del .H
#include "beginh.dex"

class CFunctionDescription;
class Expression;

//============================================================================
enum TB_EXPORT ExpItemType
	{
		EXP_ITEM_OPE_CLASS,
		EXP_ITEM_VRB_CLASS,
		EXP_ITEM_FUN_CLASS,
		EXP_ITEM_EXTERNAL_FUN_CLASS,
		EXP_ITEM_VAL_CLASS
	};

enum TB_EXPORT PrivateTokens
	{
		TT_UNMINUS		= T_STARTTOKEN,
		TT_ESCAPED_LIKE,
		TT_IS_NULL,
		TT_IS_NOT_NULL,
		TT_NULL
	};

enum TB_EXPORT OperatorType
	{ 
		UNARY_OPR, 	BINARY_OPR, TERNARY_OPR, LOGICAL_OPR
	};

//============================================================================
class TB_EXPORT ExpItem : public CObject
{
	DECLARE_DYNAMIC(ExpItem)
	
public:
	int		m_nPosInStr;

public:
	ExpItem (int p)	{ m_nPosInStr = p; }
	virtual ~ExpItem() {}

public:
	virtual ExpItemType	IsA		() const			= 0;
	virtual ExpItem* 	Clone	()					= 0;
	virtual BOOL		IsEqual	(const ExpItem&)	= 0;
	virtual ExpItem* 	Expand	()					{ return Clone(); }

// diagnostics
#ifdef _DEBUG
public:	
	virtual void Dump(CDumpContext&) const;
	virtual void AssertValid() const;
#endif
};

//============================================================================
class TB_EXPORT ExpItemOpe : public ExpItem
{
	DECLARE_DYNAMIC(ExpItemOpe)
	
public:
	int			m_nOpe;
	DataType	m_ResultType;
	Stack		m_frstOpStack;
	Stack		m_scndOpStack;

public:
	ExpItemOpe(int op, int p = -1, const DataType& = DataType::Null);

public:
	virtual ExpItemType     IsA		() const;
	virtual ExpItem*		Clone	();
	virtual ExpItem*		Expand	();
	virtual BOOL			IsEqual	(const ExpItem&);

public:
	OperatorType	GetType	() const;
};

//============================================================================
class TB_EXPORT ExpItemFun : public ExpItem
{
	DECLARE_DYNAMIC(ExpItemFun)
	
public:
	Token m_nFun;
	int	m_nNumParam;	// it is the number of parameters

public:
	ExpItemFun(Token tf, int nrParam, int p = -1);

public:
	virtual ExpItemType	IsA		() const;
	virtual ExpItem*	Clone	();
	virtual BOOL		IsEqual	(const ExpItem&);
};

//============================================================================
class TB_EXPORT ExpItemExternalFun : public ExpItem
{
	DECLARE_DYNAMIC(ExpItemExternalFun)
	
public:
	CFunctionDescription*	m_pFunctionDescription;
	int						m_nActualParameters; //comprensivo di eventuali OPTIONALs
	BOOL					m_bLateBinding;
	BOOL					m_bIsProcedure;
	CString					m_sLateBindingName;

public:
	ExpItemExternalFun(CFunctionDescription*, int p = -1);
	virtual ~ExpItemExternalFun();

public:
	virtual ExpItemType	IsA		() const;
	virtual ExpItem*	Clone	();
	virtual BOOL		IsEqual	(const ExpItem&);	

	CFunctionDescription* GetFuncPrototype() { return m_pFunctionDescription; }
};

//============================================================================
class TB_EXPORT ExpItemContentOfFun : public ExpItemFun
{
	DECLARE_DYNAMIC(ExpItemContentOfFun)
	
public:
	Stack*		m_pExpr;	// it is the espression
	SymTable*	m_pSymTable;

public:
	ExpItemContentOfFun (SymTable*	pSymTable, Stack* pExpr, int p = -1);
	virtual ~ExpItemContentOfFun();

public:
	virtual ExpItem*	Clone	();
	virtual ExpItem*	Expand	();

	virtual BOOL		IsEqual	(const ExpItem&);
};

//============================================================================
class TB_EXPORT ExpItemVrb : public ExpItem
{
	DECLARE_DYNAMIC(ExpItemVrb)
	
public:
	DataObj*	m_pData;
	CString		m_strNameVrb;

public:
	ExpItemVrb(LPCTSTR pszName, int p = -1, DataObj* = NULL);
	ExpItemVrb(const ExpItemVrb& v) 
		: 
		ExpItem		(v.m_nPosInStr),
		m_strNameVrb(v.m_strNameVrb),
		m_pData		(v.m_pData)
		{}

public:
	virtual ExpItemType	IsA		() const;
	virtual ExpItem*	Clone	();
	virtual BOOL		IsEqual	(const ExpItem&);
};

//============================================================================
class TB_EXPORT ExpItemVal : public ExpItem
{
	DECLARE_DYNAMIC(ExpItemVal)
	
public:
	DataObj*	m_pVal;
	BOOL		m_bToBeDeleted;
	BOOL		m_bOwnsData;
	BOOL		m_bVoid;
	BOOL		m_bVariant;
public:
	ExpItemVal(DataObj* pData, int pos = -1, BOOL bToBeDeleted = FALSE, BOOL bOwnsData = TRUE, BOOL bVoid = FALSE, BOOL bVariant = FALSE);
	virtual ~ExpItemVal();

public:
	virtual ExpItemType	IsA		() const;
	virtual ExpItem*	Clone	();
	virtual BOOL		IsEqual	(const ExpItem&);

public:
	DataType	GetDataType		() const;
};

//============================================================================
class TB_EXPORT ExpItemValFromVar : public ExpItemVal
{
	DECLARE_DYNAMIC(ExpItemValFromVar)
	
public:
	CString		m_strNameVrb;

	ExpItemValFromVar(const CString& strNameVrb, DataObj* pData, int pos = -1, BOOL bToBeDeleted = FALSE, BOOL bOwnsData = TRUE)
		:
		ExpItemVal		(pData, pos, bToBeDeleted, bOwnsData),
		m_strNameVrb	(strNameVrb)
		{}

public:
	virtual ExpItem*	Clone	();
};

//============================================================================
class TB_EXPORT ExpItemContentOfVal : public ExpItemVal
{
	DECLARE_DYNAMIC(ExpItemContentOfVal)
	
public:
	ExpItemContentOfVal(DataObj* pData, int pos = -1, BOOL bToBeDeleted = FALSE, BOOL bOwnsData = TRUE)
		:
		ExpItemVal(pData, pos, bToBeDeleted, bOwnsData)
		{}

public:
	virtual ExpItem*	Clone	();
};

//============================================================================
class TB_EXPORT ExpItemContentOfParamVal : public ExpItemVal
{
	DECLARE_DYNAMIC(ExpItemContentOfParamVal)
	
public:
	ExpItemContentOfParamVal(DataObj* pData, int pos = -1, BOOL bToBeDeleted = FALSE, BOOL bOwnsData = TRUE)
		:
		ExpItemVal(pData, pos, bToBeDeleted, bOwnsData)
		{}

public:
	virtual ExpItem*	Clone	();
};

//============================================================================
class TB_EXPORT CStopTokens : public CWordArray
{
public:
	BOOL m_bSkipInnerBlock;
	BOOL m_bSkipInnerRoundBrackets;
	BOOL m_bSkipInnerSquareBrackets;
	BOOL m_bSkipInnerBraceBrackets;
protected:
	BOOL m_nCountInnerBlock;
	BOOL m_nCountInnerRoundBrackets;
	BOOL m_nCountInnerSquareBrackets;
	BOOL m_nCountInnerBraceBrackets;
public:
	CStopTokens() 
		: 
		m_bSkipInnerBlock			(FALSE),
		m_bSkipInnerRoundBrackets	(FALSE),
		m_bSkipInnerSquareBrackets	(FALSE),
		m_bSkipInnerBraceBrackets	(FALSE),

		m_nCountInnerBlock			(0),
		m_nCountInnerRoundBrackets	(0),
		m_nCountInnerSquareBrackets	(0),
		m_nCountInnerBraceBrackets	(0)
	{}

	CStopTokens (const CStopTokens& a) { Assign(a); }
	void Assign (const CStopTokens& a);

	BOOL IsStopParse (Token tk);
};

//============================================================================
class TB_EXPORT ExpParse
{
private:
	CStopTokens*	m_pStopTokens = NULL;
	SymTable*		m_pSymTable = NULL;
	CString			m_strStringParsed;

	BOOL			m_bHasFields = FALSE;
	BOOL			m_bHasRuleFields = FALSE;
	BOOL			m_bHasInputFields = FALSE;
	BOOL			m_bHasAskFields = FALSE;

	BOOL			m_bHasExternalFunctionCall = FALSE;
	BOOL			m_bHasDynamicFragment = FALSE;	//ContentOf(...), {EVAL <sub-expr> }
public:
	BOOL			m_bWClause = FALSE;
public:
	ExpParse(SymTable* pSymTable = NULL, CStopTokens* pStopTokens = NULL);

	BOOL		Parse		(Parser&, Stack&, BOOL bKeepString = FALSE);
	CString		GetString	();

	BOOL		HasFields			() const { return m_bHasFields; }
	BOOL		HasRuleFields		() const { return m_bHasRuleFields; }
	BOOL		HasInputFields		() const { return m_bHasInputFields; }
	BOOL		HasAskFields		() const { return m_bHasAskFields; }
	BOOL		HasExternalFunctionCall	() const { return m_bHasExternalFunctionCall; }
	BOOL		HasDynamicFragment		() const { return m_bHasDynamicFragment; }

	static BOOL	CompareStack(const Stack&, const Stack&);
	static void	DupStack	(const Stack&, Stack&);
	static void	ExpandStack	(const Stack&, Stack&);
	static void	MoveStack	(Stack&, Stack&, int nItems = -1);

private:
	void		Expression		(Parser&,Stack&);
	void		Disjunctive		(Parser&,Stack&);
	void		Conjunctive		(Parser&,Stack&);
	void		Formula			(Parser&,Stack&);
	void		Term			(Parser&,Stack&);
	void		Factor			(Parser&,Stack&);
	BOOL		StopParse		(Parser&);

	BOOL		ParseExternalFunc		(CString& strName, Parser& lex, Stack& exprStack);
	BOOL		ParseInternalFunc		(Parser& lex, Stack& exprStack);
	BOOL		ParseOptionalParameters	(ExpItemFun*, Parser& lex, Stack& exprStack, int nH = 0);
	BOOL		ParseArrayIndexer		(CString& sArrayName, Parser& lex, Stack& exprStack);
	BOOL		ParseArrayMethods		(const CString& sArrayName, const CString& sMethodName, Parser& lex, Stack& exprStack, LPCTSTR szPrefix/*=L"Array_"*/);
	BOOL		ParseArrayCreate		(Parser& lex, Stack& exprStack);

public:
	BOOL		ParseContentOfFunc		(Parser& lex, Stack& exprParentStack, BOOL bNative = FALSE);

static	BOOL	IsVariantArgumentsFunction	(CString strFuncName);
static	BOOL	AllowLateBinding			(CString strFuncName, CString& sHandleName, SymTable* = NULL);
};

//============================================================================
class TB_EXPORT ExpUnparse
{
public:
	static	CString	UnparseData		(const DataObj&);
	
public:
	BOOL	Unparse			(CString&,	Stack&);

protected:
	BOOL	UnparseVariable	(CString&,	ExpItemVrb*);
	BOOL	UnparseValue	(CString&,	ExpItemVal*);
	BOOL	UnparseOpe		(CString&,	int);

private:
	BOOL	Expression		(CStringArray&,	Stack&);
};

//////////////////////////////////////////////////////////////////////////////
//                 N O N    C L A S S    F U N C T I O N         			//
//////////////////////////////////////////////////////////////////////////////
//----------------------------------------------------------------------------
TB_EXPORT int		parameters_of		(int nFun);
TB_EXPORT int		opt_parameters_of	(int nFun);
TB_EXPORT BOOL		return_func_type	(int nFun, DataType&);
TB_EXPORT DataType	param_fun			(int nFun, int wIdParam);
TB_EXPORT DataType	opt_param_fun		(int nFun, int wIdParam);

enum TB_EXPORT EGroupFunction
{
	G_PRIVATE,
	G_DATETIME,
	G_NUMERIC,
	G_STRING,
	G_ARRAY,
	G_SYSTEM,
	G_OTHER
};

TB_EXPORT CString			return_func_group_description	(EGroupFunction eGroup);
TB_EXPORT EGroupFunction	return_func_group				(int nFun);


//---------------------------------------------------------------------------
#include "endh.dex"
