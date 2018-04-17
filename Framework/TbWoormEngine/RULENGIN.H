
#pragma once

#include <TbGeneric\bitset.h>
#include <TbGeneric\schedule.h>

#include <TbParser\Parser.h>
#include <TbGenlib\expr.h>

#include "QueryObject.h"
#include "reptable.h"
#include "report.h"
#include "reptable.h"
#include "ActionsRepEngin.h"
#include "ruledata.h"

//includere alla fine degli include del .H
#include "beginh.dex"

//============================================================================
class RuleObj;
class ExpRule;
class CondRule;
class WhileRule;
class QueryRule;
class DataTableRule;
class SqlTable;
class SqlTableInfo;
class SelectionMap;
class WClause;
class CTBContext;
class GroupBy;

//============================================================================
enum TB_EXPORT RuleReturn			{ RULE_ABORT, RULE_BACKTRACK, RULE_SUCCESS };
enum TB_EXPORT EngineStatusType		{ GROUPBY, TEMP, GROUP_ON_TEMP };

//============================================================================
class TB_EXPORT EngineStatus : public BitSet
{
public:
	EngineStatus (unsigned long nSize) : BitSet(nSize) {}

	void			On	(EngineStatusType bit) { BitSet::On(bit); }
	void			Off	(EngineStatusType bit) { BitSet::Off(bit); }
	int operator	[]	(EngineStatusType bit) const { return ((BitSet &) *this)[bit]; }
};

//============================================================================
class TB_EXPORT RuleEngine : public CObject, public IDisposingSourceImpl
{
	friend  ExpRule;
	friend  CondRule;
	friend  DataTableRule;
	friend  WhileRule;
	friend  QueryRule;
	friend  CRSTreeCtrl;

	DECLARE_DYNAMIC(RuleEngine);

protected:
	WoormTable&			m_SymTable;
	CString				m_strName;

	EngineStatus  		m_EngineStatus;

	// remember that m_pTmpRule will be deleted by Array object so it doesn't
	// delete in this class.
	TDisposablePtr<DataTableRule>		m_pTmpRule;

//private:
	Array		m_SortedRules;
	Array		m_UnsortedRules;
	WoormField*	m_pFieldsStatus;
	int			m_nStartPos;
	int			m_nStopPos;
	int			m_nLastElementPos;
	int			m_nLastSubTreePos;
	int			m_nTotSubTreeNodes;
	int			m_nUnsRulesNumber;
	int			m_nNullRulesNum;
	int			m_nTotOrderBySeg;

	ExpressionWithDebugBlocks*		m_pTupleFilterEngine = NULL;
public:
	EngineScheduler&	m_Scheduler;

public:
	RuleEngine (WoormTable&, EngineScheduler&);
	~RuleEngine ();

	WoormTable&	GetSymTable			() const	{ return m_SymTable; }
	BOOL		UserBreak			();
	BOOL		SetRTError			(EngineScheduler::MessageID, const CString& = _T(""), LPCTSTR = NULL);
	void		ResetRTError		()	const { m_Scheduler.ResetRTError(); }
	EngineScheduler::MessageID 		GetRTErrorCode () const { return m_Scheduler.GetRTErrorCode(); }
	CString		FormatRTError		();
	LPCTSTR		GetRTErrorFailedName()	const { return m_Scheduler.GetRTErrorFailedName(); }

	const CString&		GetName			() const					{ return m_strName;	}	//m_strPublicName
	void				SetName			(LPCTSTR pszName)			{ m_strName = pszName;	}	

protected:
	virtual BOOL		TupleProcessing	()					= 0;

			void		AddRule			(RuleObj*);
			BOOL		BuildTree		(Parser&, GroupBy* pGroupBy);
			BOOL		ApplyRules		(const Array* arOrderOnThisColumns = NULL, CString* pstrAuxWhereClause = NULL);
			BOOL		Retrieve		();

private:
	RuleReturn	ApplyFrom    		(int, const Array* arOrderOnThisColumns = NULL, CString* pstrAuxWhereClause = NULL);

	int			BuildSubTree 		();
	void		MoveRule     		(RuleObj&, BOOL&, BOOL&, int&);
	void		BuildLinks   		();
	BOOL		CheckRecursion		();
	BOOL		CheckDescendents 	(RuleObj&);
	void		FindSubTree			(CObArray&);
	void		MakeSubTreeNodes 	(RuleObj&);
	void		TraceRules			();
	void		TraceRules			(CObArray&, CString title);
};

//============================================================================
class TB_EXPORT DebugBlock : public Block
{
	//friend class WhileLoopAction;
	//friend class ConditionalAction;
	//friend class Procedure;

	DECLARE_DYNAMIC(DebugBlock);
public:
	DebugBlock(RepEngine* pEngine)
		:
		Block(NULL, NULL, pEngine, FALSE)
		{}

	virtual ~DebugBlock();

	BOOL	Exec(CObject* pOwner = NULL);
};

//============================================================================
class TB_EXPORT RuleObj : public CObject, public IDisposingSourceImpl
{
	friend  RuleEngine;
	friend  ExpRule;
	friend  CondRule;
	friend  DataTableRule;
	friend  GroupBy;
	
	DECLARE_DYNAMIC(RuleObj);
protected:
	int			m_nRuleTag = -1;

	RuleEngine&	m_OwnerEngine;

	CObArray	m_Sons;
	CObArray	m_Parents;
	int			m_nParentsNum;	// only used for sorting algorithm
	int			m_nRuleId;
	int			m_nOrderByPos;
	BOOL		m_bMark;
	BOOL		m_bSorted;
	BOOL		m_bFailed;
	BOOL		m_bNullSolution;
public:
	//used by breakpoint of RS-DEBUGGER - Iterative Rule For-While
	DebugBlock*		m_pBefore = NULL;
	DebugBlock*		m_pAfter = NULL;
public:
	RuleObj (RuleEngine&);
	virtual ~RuleObj();

	BOOL			IsSorted()		{ return m_bSorted; }
	WoormTable*		GetSymTable()	{ return &m_OwnerEngine.GetSymTable(); }

	virtual RuleType 	IsARule  () const = 0;

	virtual CString		GetDescription		() const = 0;

	//virtual BOOL	Parse(Parser&) = 0;
	virtual void	Unparse(Unparser&) {}
	virtual	CString	ToString();

protected:
	virtual BOOL		IsParentOf	 (RuleObj&) const;
	virtual BOOL		FindKeyName	 (LPCTSTR aFldName)							  = 0;

	//virtual BOOL		BuildTmpRule (Parser&, SqlSchema*, DataTableRule*)	const = 0;

	virtual RuleReturn	Apply		 (const Array* arOrderOnThisColumns = NULL, CString* pstrAuxWhereClause = NULL) = 0;
public:	
	virtual CString		GetSelFieldName		(int) const = 0;
	virtual int			GetSelFieldsNum		() const = 0;
	virtual void		GetSelFieldNames	(CStringArray&) const;

protected:
	BOOL	HasSon					(RuleObj&)	const;
	BOOL	IsSonOfNullRules		()			const;
	BOOL	IsParentOfFailedRules	()			const;
	BOOL	IsRoot					()			const;

	// diagnostics
#ifdef _DEBUG
public:
	virtual void Dump(CDumpContext&) const;
	//virtual void AssertValid() const;
#endif
};

//============================================================================
class TB_EXPORT LetStatement : public Expression
{
	friend ExpRule;
	friend CondRule;
	DECLARE_DYNAMIC(LetStatement);
private:
	WoormField*	m_pField;

public:
	LetStatement (SymTable*);

private:
	WoormField*	Parse			(Parser&);
	WoormField*	GetField		()	const { return m_pField; }
	LPCTSTR		GetFieldName	()	const { return m_pField->GetName(); }
	//LPCTSTR		GetUniqueName	()	const { return m_pField->GetUniqueName(); }
public:
	virtual CString		GetDescription() const;
};

//============================================================================
class TB_EXPORT ExpRule : public RuleObj
{
	DECLARE_DYNAMIC(ExpRule);
public:
	LetStatement	m_Expr;
		
	Expression		m_ValidateWhereExpr;

public:
	ExpRule (RuleEngine& OwnerEngine);

public:
	BOOL	Parse (Parser&);
	virtual void	Unparse(Unparser&); 

protected:
	virtual RuleType 	IsARule		 () const;
	virtual RuleReturn	Apply		 (const Array* arOrderOnThisColumns = NULL, CString* pstrAuxWhereClause = NULL);
	virtual CString		GetDescription() const;

	virtual BOOL		FindKeyName	 (LPCTSTR pszFldName);
public:
	virtual CString		GetSelFieldName		(int)	const { return m_Expr.GetFieldName(); }
	virtual int			GetSelFieldsNum		()		const { return 1; }
			WoormField* GetSelField			(int)	const { return m_Expr.GetField(); }
};

//============================================================================
class TB_EXPORT CondRule : public RuleObj
{
	DECLARE_DYNAMIC(CondRule);
public:
	Expression		m_TestExpr;
		LetStatement	m_ThenExpr;
		LetStatement	m_ElseExpr;

	Expression		m_ValidateWhereExpr;

public:
	CondRule  (RuleEngine& OwnerEngine);

public:
	virtual BOOL	Parse	(Parser&);
	virtual void	Unparse(Unparser&);

protected:
	virtual RuleType 	IsARule			 ()	const;
	virtual RuleReturn	Apply		 (const Array* arOrderOnThisColumns = NULL, CString* pstrAuxWhereClause = NULL);
	virtual CString		GetDescription() const;

	virtual BOOL		FindKeyName			(LPCTSTR pszFldName);
public:
	virtual CString		GetSelFieldName		(int)	const { return m_ThenExpr.GetFieldName(); }
	virtual int			GetSelFieldsNum		()		const { return 1; }
			WoormField* GetSelField			(int)	const { return m_ThenExpr.GetField(); }
};

//============================================================================
class TB_EXPORT WhileRule : public RuleObj
{
	DECLARE_DYNAMIC(WhileRule);

	Expression		m_CondExpr;
	Block*			m_pBody = NULL;

	CStringArray	m_arWSelFieldName;

public:
	Block*			GetInitBlock		() { return m_pBefore; }
	Block*			GetBodyBlock		() { return m_pBody; }
	Block*			GetIteratorBlock	() { return m_pAfter; }

public:
	WhileRule  (RuleEngine& OwnerEngine);

public:
	BOOL	Parse	(Parser&);

protected:
	virtual RuleType 	IsARule					()	const;
	virtual RuleReturn	Apply				(const Array* arOrderOnThisColumns = NULL, CString* pstrAuxWhereClause = NULL);
	virtual CString		GetDescription		() const;

	virtual BOOL		FindKeyName			(LPCTSTR pszFldName);

			void		SetRuleFields(BOOL setRuleField);
public:
	virtual CString		GetSelFieldName		(int i)	const { return m_arWSelFieldName.GetAt(i); }
	virtual int			GetSelFieldsNum		()		const  { return m_arWSelFieldName.GetSize(); }
			WoormField* GetSelField			(int i) const;
};

//============================================================================
class TB_EXPORT QueryRule : public RuleObj
{
	friend class QueryEngine;

	DECLARE_DYNAMIC(QueryRule);
private:
	CString			m_strQueryName;
	QueryObject*	m_pQuery = NULL;

	RuleSelectionMode	m_ConstraintMode = RuleSelectionMode::SEL_ALL;

	Expression		m_WhenExpr;
	CTBContext*		m_pTBContext = NULL;

public:
	QueryRule  (RuleEngine& OwnerEngine, CTBContext* pTBContext);

	BOOL	Parse	(Parser&);

	QueryObject*	GetQuery() const { ASSERT_VALID(m_pQuery); return m_pQuery; }

	virtual CString		GetDescription() const;
protected:
	virtual RuleType 	IsARule			 ()	const;
	virtual RuleReturn	Apply		 (const Array* arOrderOnThisColumns = NULL, CString* pstrAuxWhereClause = NULL);

	virtual BOOL		FindKeyName			(LPCTSTR pszFldName);

			void		SetRuleFields(BOOL setRuleField);
public:
	int			GetSelFieldsNum	() const;
	CString		GetSelFieldName	(int i) const;
	WoormField* GetSelField		(int i) const;

private:
	RuleReturn	ExecQuery();
	RuleReturn	ExecSubQuery(BOOL& bRecFoundForConstraint, const Array* arOrderOnThisColumns = NULL, CString* pstrAuxWhereClause = NULL);
};

//============================================================================
class TB_EXPORT DataTableRule : public RuleObj
{
	friend RuleEngine;
	friend DebugBlock;

	DECLARE_DYNAMIC(DataTableRule);

private:
	RuleSelectionMode	m_ConstraintMode = RuleSelectionMode::SEL_ALL;
	SelectionMap*	m_pSelectionMap = NULL;
	BOOL			m_bDistinct = FALSE;
	int				m_nTop;
	WClause*		m_pWhereClause = NULL;
	CString			m_strGroupByDynamic;
	WClause*		m_pHavingClause = NULL;
	CString			m_strOrderByDynamic;

	Expression		m_WhenExpr;

	SqlTable*		m_pDataTable = NULL;
	int				m_nDataLevel = RULE_ENGINE;
	CString			m_ExtraFilter;
	RecordArray		m_arContextBagRecords;
	CTBContext*		m_pTBContext = NULL;

	//BOOL			m_bTmpRule = FALSE;
public:
	DataTableRule (RuleEngine& OwnerEngine, CTBContext* pTBContext/*, BOOL bTemporary = FALSE*/);
	~DataTableRule ();

public:
	//BOOL		ReadFromTmp		();
	//BOOL		WriteOnTmp		();
	//BOOL		UpdateOnTmp		();

	//void		AddSelFieldTmp  (WoormField*, int = QUERY_ENGINE);
	//void		ResetDataMapping();

	void		CloseDataTable	();
	SqlTable*	GetDataTable	()		{ return m_pDataTable; }

	BOOL			Parse		  	(Parser&);
	virtual void	Unparse			(Unparser&);
	CString			ToSqlString		();

	void		GetParameters	(CStringArray& parameters) const;
	int			GetTotFieldLinks() const;
protected:
	//virtual BOOL		BuildTmpRule	(Parser&, SqlSchema*, DataTableRule*)const;

	virtual RuleType	IsARule				()			const;
	virtual RuleReturn  Apply			(const Array* arOrderOnThisColumns /*=NULL*/, CString* pstrAuxWhereClause = NULL);
	virtual CString		GetDescription() const;

	//virtual BOOL		IsParentOf		(RuleObj&)	const;
	virtual BOOL		FindKeyName		(LPCTSTR pszFldName);
	virtual int			GetSelFieldsNum	() const;
	virtual CString		GetSelFieldName	(int) const;
	virtual WoormField*	GetSelField		(int) const;

private:
	RuleReturn	ExecQuery			(const Array* arOrderOnThisColumns = NULL, CString* pstrAuxWhereClause = NULL);
	RuleReturn	ExecSubQuery		(BOOL& bRecFoundForConstraint, const Array* arOrderOnThisColumns = NULL, CString* pstrAuxWhereClause = NULL);
	//void		UpdateDataStatus	();
	//void		UpdateStatusFlags	();
};

//============================================================================
//	RuleEngine auxiliary class
//============================================================================

class TB_EXPORT COrderColumn: public CObject
{
	DECLARE_DYNAMIC(COrderColumn);
public:
	WORD m_nAliasSelectedColumn;
	BOOL m_bAscendingOrder;

	COrderColumn () 
		: m_nAliasSelectedColumn (0), m_bAscendingOrder (TRUE) {};
	COrderColumn (WORD  nAliasSelectedColumn, BOOL bAscendingOrder = TRUE) 
		: m_nAliasSelectedColumn (nAliasSelectedColumn), m_bAscendingOrder (bAscendingOrder) {};
};

#include "endh.dex"
