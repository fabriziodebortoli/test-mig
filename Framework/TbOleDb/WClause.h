
#pragma once

#include <TbGenlib\expr.h>
#include "sqlcatalog.h"

//includere alla fine degli include del .H
#include "beginh.dex"

//=========================================================================
class Parser;
class SqlTableInfo;
class SqlTable;
class ParamsArray;

//============================================================================

TB_EXPORT BOOL	IsPhysicalName	(const CString& strName, const SqlTableInfo* pSqlTableInfo);
TB_EXPORT BOOL	IsPhysicalName	(const CString& strName, const SqlTableInfoArray* parSqlTableInfo, CStringArray* parstrAliasTableName = NULL);

TB_EXPORT CString	GetPhysicalName	(const CString& strFromName, const SqlTableInfo* pSqlTableInfo = NULL);
TB_EXPORT CString	GetPhysicalName	(const CString& strFromName, const SqlTableInfoArray* parSqlTableInfo);

TB_EXPORT BOOL	ParseOrderBy	
						(
							Parser& parser, SymTable* pSymTable,
							const SqlTableInfoArray* parSqlTableInfo,
							CString& strOrderBy,
							BOOL bQualified = FALSE,
							CStringArray* items = NULL,
							CStringArray* parstrAliasTableName = NULL
						);
TB_EXPORT BOOL ExpandContentOfClause (CString& strSql, SymTable* pSymTable, SqlConnection* pSqlConnection);
TB_EXPORT BOOL ExpandContentOfClause (CString& strSql, SymTable* pSymTable, int nStartPos/* = 0*/);

TB_EXPORT BOOL ParseCalculateColumn (CString str, SymTable* pSymTable, CString& err);

//===========================================================================
//		Local ExpItemValWC class
//===========================================================================
class ExpItemValWC : public ExpItemVal
{
	DECLARE_DYNAMIC(ExpItemValWC)

public:
	CWordArray*	m_pCompatibleDataTypes;

public:
	ExpItemValWC(DataObj*, int pos = -1, BOOL bToBeDeleted = FALSE, BOOL bOwnsData = TRUE, const CWordArray* = NULL);
	virtual ~ExpItemValWC();

public:
	virtual ExpItem*	Clone();
};

//=========================================================================
class TB_EXPORT WClauseExpr : public Expression
{
	DECLARE_DYNAMIC(WClauseExpr)

	friend class WClause;
public:
	enum EClauseType { WHERE, HAVING, JOIN_ON };

protected:
	SqlConnection*		m_pSqlConnection;
	SqlTableJoinInfoArray	m_arSqlTableInfo;
	SqlTable*			m_pSqlTable;
	ParamsArray*		m_pParamsArray;
	Token				m_EmptyWhere;
	BOOL				m_bNative;

	EClauseType			m_eClauseType;
	int					m_nPrepareTableIndex;	//usata dalla PrepareQuery per le clauses di Join On

	CStringArray		m_arForbiddenPublicIdents;

public:
	CStringArray	m_arCommentTraceBefore;
	CStringArray	m_arCommentTraceAfter;

public:
	WClauseExpr(SqlConnection*, SymTable*, const SqlTableJoinInfoArray* = NULL, SqlTable* = NULL);
	WClauseExpr	(const WClauseExpr&);
	~WClauseExpr();

public:
	virtual	WClauseExpr*	Clone			();
	virtual	void			Reset			(BOOL bResetAll = TRUE);

	virtual CString			ToString		(SqlTable* = NULL);

	virtual BOOL			IsEmpty			();

	virtual void			SetTableInfo	(const SqlTableInfo* pSqlTableInfo, SqlConnection* pConnection, SymTable* pSymTable);
	virtual void			SetTableInfo	(const SqlTableInfoArray* pSqlTableInfoAr)		{ m_arSqlTableInfo = *pSqlTableInfoAr; }
	virtual void			SetTableInfo	(const SqlTableJoinInfoArray* pSqlTableInfoAr)	{ m_arSqlTableInfo = *pSqlTableInfoAr; }

	virtual BOOL			HasMember		(LPCTSTR pszVarName) const;
	virtual void			GetParameters	(CStringArray& arParameters, BOOL bReverse = TRUE) const;

public:
	void			operator =	(const WClauseExpr&);
	
	BOOL			Parse			(Parser&);

	void			SetNative		(BOOL bNative = TRUE)			{ m_bNative = bNative; }
	BOOL			IsNative		()	const						{ return m_bNative; }

	EClauseType		GetClauseType	()	const			{ return m_eClauseType; }
	void			SetClauseType	(EClauseType ct)	{ m_eClauseType = ct; }
	void			SetHavingClause	()					{ m_eClauseType = EClauseType::HAVING; }
	void			SetJoinOnClause	()					{ m_eClauseType = EClauseType::JOIN_ON; }

	void			SetForbiddenPublicIdents	(const CStringArray& parForbiddenPublicIdents);

protected:
	virtual BOOL		Eval			(DataObj&)	{ ASSERT(FALSE); return FALSE; }
	// il secondo parametro mi permette di aggiungere ulteriori condizioni di filtraggio
	// ad una query già esistente
	virtual	BOOL		PrepareQuery(WClauseExpr*& pCurrWClause, BOOL bAppend = FALSE, int nTableIndex = 0);

	virtual ExpItemVal* ResolveSymbol	(ExpItemVrb&);
	
	virtual DataType	GiveMeResultTypeForMathOpMap	(DATA_TYPE_OP_MAP&, ExpItemVal*, ExpItemVal*);
	virtual DataType	GiveMeResultTypeForBitwiseOp	(ExpItemVal*, ExpItemVal* = NULL);
	virtual DataType	GiveMeResultTypeForLogicalOp	(ExpItemVal*, ExpItemVal* = NULL);
	virtual DataType	GiveMeResultTypeForLikeOp		(ExpItemVal*, ExpItemVal*);
	
	virtual ExpItemVal*	OnApplyFunction					(ExpItemFun*, Stack&);
public:
	BOOL	ParseNative				(Parser&);
	void	SetSqlTable				(SqlTable* pSqlTable) { m_pSqlTable = pSqlTable;  }
protected:
	BOOL	ParseVariableOrConstForNativeWhere	(Parser&);
	BOOL	ConvertToNative			(const CString& sSource, CString& sWhere);
	BOOL	BindParamsNative		(SqlTable* = NULL, BOOL bAppend = FALSE); //bAppend vedi sopra
	BOOL	ModifyVariableOrConst	(Stack&);
	BOOL	BindParams				(SqlTable* = NULL, BOOL bAppend = FALSE); //bAppend vedi sopra
	BOOL	OpenTable				(BOOL bAppend = FALSE); // bAppend vedi sopra
	void	AddSqlParam				(const CString& sParamName, DataObj* pAssociatedData);
	CString GetPrefixParamName		(CString name = L"Param") const ;
public:
	static BOOL ConvertToNative
	(
		const CString& sSource, CString& sWhere,
		SymTable* pSymTable,
		SqlConnection* pSqlConnection,
		CStringArray* parForbiddenPublicIdents
	);
};

//=========================================================================
// class name c#/core: IfWhereClause : WhereClauseExpr
class TB_EXPORT WClause : public WClauseExpr
{
	DECLARE_DYNAMIC(WClause)

protected:
	Expression*		m_pCondExpr;
	WClauseExpr*	m_pThenWClause;	//could be instance of class WClause too
	WClauseExpr*	m_pElseWClause; //could be instance of class WClause too

	WClauseExpr*	m_pCurrWClause;

public:
	WClause(SqlConnection*, SymTable*, const SqlTableJoinInfoArray* = NULL	, SqlTable* = NULL);
	WClause(SqlConnection*, SymTable*, const SqlTableInfoArray&				, SqlTable* = NULL, CStringArray* parstrTableAlias = NULL);

	WClause	(const WClause&);
	~WClause();

public:
	virtual	WClauseExpr*	Clone			();
	virtual	void			Reset			(BOOL bResetAll = TRUE);

	virtual void			SetTableInfo	(const SqlTableInfo* pSqlTableInfo);
	virtual void			SetTableInfo	(const SqlTableInfoArray* parSqlTableInfo);
	virtual void			SetTableInfo	(const SqlTableJoinInfoArray* parSqlTableInfo);

	virtual CString			ToString		(SqlTable* = NULL);

	virtual BOOL			IsEmpty			();

	virtual BOOL			HasMember		(LPCTSTR sVarName) const;
	virtual void			GetParameters	(CStringArray&, BOOL bReverse = TRUE) const;

public:
	void		operator =		(const WClause&);

	BOOL		Parse			(Parser&);
	void		SetNative		(BOOL bNative = TRUE);
	BOOL		PrepareQuery	(BOOL bAppend = FALSE, int nTableIndex = 0);

protected:
	// il secondo parametro mi permette di aggiungere ulteriori condizioni di filtraggio
	// ad una query già esistente
	virtual	BOOL	PrepareQuery(WClauseExpr*& pCurrWClause, BOOL bAppend = FALSE, int nTableIndex = 0);

protected:
			BOOL	ParseCondWhere	(Parser&);
			void	Dispose			();
};
#include "endh.dex"
