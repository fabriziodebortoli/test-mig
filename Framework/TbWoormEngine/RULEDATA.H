
#pragma once

#include <TbGeneric\Array.h>
#include <TbGenlib\expr.h>

#include <TbOledb\wclause.h>	
#include <TbOledb\Sqlrec.h>	
#include <TbOledb\Sqltable.h>	

#include "ActionsRepEngin.h"

//includere alla fine degli include del .H
#include "beginh.dex"

//============================================================================
class Parser;
class Unparser;
class WoormTable;
class CTblRuleDlg;
class TblRuleData;
class ExpRuleDlg;
class SqlTableInfo;
class SqlConnection;
class QueryObject;
//============================================================================
//enum TB_EXPORT RuleDataType { TBLRULE_TYPE, EXPRULE_TYPE, QUERYRULE_TYPE, WHILERULE_TYPE };
enum TB_EXPORT RuleType { RULE_NULL, RULE_EXPR, RULE_COND_EXPR, RULE_DATA_TABLE, RULE_LOOP_WHILE, RULE_NAMED_QUERY };

//============================================================================
class TB_EXPORT RuleDataObj : public CObject
{
protected:
    WoormTable&		m_SymTable;
public:
	CStringArray	m_arCommentTraceBefore;
	CStringArray	m_arCommentTraceAfter;

public:
	RuleDataObj(WoormTable&);

	virtual RuleType	IsARule() = 0;

	virtual BOOL	Parse			(Parser&)	= 0;
	virtual void	Unparse			(Unparser&)	= 0;

	virtual BOOL	ExistPublicName	(LPCTSTR)	= 0;

	virtual BOOL	CanDeleteField	(LPCTSTR) const	= 0;
	virtual void	DeleteField		(LPCTSTR)	= 0;
	virtual void	RenameField		(LPCTSTR, LPCTSTR) {}
	virtual BOOL	IsEmpty			()			= 0;
	virtual CString		GetRuleDescription() const = 0;

	WoormTable*		GetSymTable() const { return &m_SymTable; }
// diagnostics
#ifdef _DEBUG
public:	
	virtual void Dump(CDumpContext&) const;
	virtual void AssertValid() const;
#endif
};

//============================================================================
class TB_EXPORT RuleDataArray : public Array
{
public:
	WoormTable&		m_SymTable;
	TblRuleData*	m_pLastTblRule = NULL;

	CStringArray	m_arCommentTraceBefore;
	CStringArray	m_arCommentTraceInner;
	CStringArray	m_arCommentTraceAfter;

public:
	RuleDataArray	(WoormTable&);
	~RuleDataArray	();

public:
	void	RemoveAt	(int nIndex, int nCount = 1);
	void	RemoveAll	();
	int		Add			(RuleDataObj*);
public:
	BOOL			Parse			(Parser&, SqlConnection*);
	void			Unparse			(Unparser&);

	RuleDataObj*	GetRuleData		(WORD id);
	RuleDataObj*	GetRuleData		(const CString& pszRuleName);
	RuleDataObj*	GetRuleData		(LPCTSTR pszRuleName, RuleType, int nReplic);
	int				GetRuleIndex	(LPCTSTR pszRuleName, RuleType, int nReplic);

	BOOL			IsEmpty			();
	BOOL			CanDeleteField	(LPCTSTR) const;
	void			DeleteField		(LPCTSTR);
	void			RenameField		(LPCTSTR pszOldFieldName, LPCTSTR pszNewFieldName);

	TblRuleData*	GetTblRuleData(const CString& sTableName);
	TblRuleData*	GetTblRuleDataFromField(const CString& sPublicName);

	int				GetQueryRuleDataIndex(QueryObjItem*);
};

//============================================================================
class TB_EXPORT TblRuleData : public RuleDataObj
{
	friend class	CTblRuleDlg;
	friend class	CModTblRuleDlg;
	friend class    CAddCalcColumn;
	friend class    CAddJoin;

	friend class	CTblRuleDlgNEW;

	friend class    CRSTreeCtrl;
	friend class	CRSEditView;

public:
	SqlTableJoinInfoArray	m_arSqlTableJoinInfoArray;

	RuleSelectionMode		m_ConstraintMode = RuleSelectionMode::SEL_ALL;	//Woorm language engine attribute
	BOOL					m_bDistinct;		//Sql language attribute
	int						m_nTop;				//Sql language attribute

	DataFieldLinkArray		m_CalcColumnLinks;	//Selected calculated "columns"

protected:
	WClause*				m_pWhereClause = NULL;		//Sql clauses
	CString					m_strGroupBy;

	WClause*				m_pHavingClause = NULL;
	CString					m_strOrderBy;

	Expression*				m_pWhenExpr = NULL;		//Woorm language enable rule condition

	BOOL					m_bTemporaryJoin; //true: join just inserted

	SqlConnection*			m_pSqlConnection = NULL;

public:
	class SqlTest
	{
	public:

		SqlTable*			m_pDataTable = NULL;
		RecordArray			m_arBindRecords;
		CString				m_ExtraFilter;
		RecordArray			m_arContextBagRecords;

		SqlTest() : m_pDataTable(NULL) {}
		~SqlTest() { if (m_pDataTable) m_pDataTable->Close();  SAFE_DELETE(m_pDataTable); }

		void Clear() { if (m_pDataTable) m_pDataTable->Close(); SAFE_DELETE(m_pDataTable); m_ExtraFilter.Empty(); m_arContextBagRecords.RemoveAll(); m_arBindRecords.RemoveAll(); if (m_pDataTable) m_pDataTable->ClearQuery(); }
	};
	
	SqlTest m_SqlTest;

public:
	TblRuleData(WoormTable& aSymTable, SqlConnection* pSqlConnection, LPCTSTR tblName = NULL);
	~TblRuleData();

	RuleType		IsARule			();
    BOOL			IsEmpty			();

	virtual CString		GetRuleDescription() const;

	CString				GetSelectionModeDescr() const;
	static CString		GetSelectionModeDescr(RuleSelectionMode);

	void				Empty();	//Remove all
	SqlConnection*		GetConnection() const { return m_pSqlConnection; }

public:
	int				AddJoinTable	(SqlTableInfo* tableInfo);
    void			AddTableInfo	(SqlTableInfo* tableInfo, const CString& sAlias = CString());
	int				GetArSqlTableInfoSize ();

	void			GetTableNames	(CStringArray &arTableNames) const;
	CString			GetTableNames	() const ;

	BOOL			IsDistinct		()	const	{ return m_bDistinct; }
	void			SetDistinct		(BOOL d)	{ m_bDistinct = d; }
	BOOL			GetTop			()	const	{ return m_nTop; }
	void			SetTop			(int t)		{ m_nTop = t; }

	DataFieldLink*	AddLink			(LPCTSTR pszPhysicName, LPCTSTR pszPublicName, BOOL bNativeExpr/* = FALSE*/, DataType type = DataType::Null, int nTable = -1); //aggiunto nTable
	BOOL			ExistLink		(LPCTSTR pszPhysicName, LPCTSTR pszPublicName);
	BOOL			GetLink			(int nIndex, CString& strPhysicName, CString& strPublicName, int nTable = -1); //aggiunto nTable
	int				GetNumTableLinks (int nTable = -1) const ; 
	int				GetTotFieldLinks () const; 

	BOOL			ExistPhysicalName			(LPCTSTR pszName);
	BOOL			ExistPublicName				(LPCTSTR pszName);
	BOOL			ExistPublicNameInTableInfo	(LPCTSTR pszPublicName, int idxTableInfo);

	DataFieldLink*	GetCalcColumn				(LPCTSTR pszPublicName);
	BOOL			IsNativeExpr				(LPCTSTR pszPublicName);
	BOOL			IsNativeQuery				();

	CString			GetPublicNameOf		(LPCTSTR pszPhysicName);
	CString			GetPhysicalNameOf	(LPCTSTR pszPublicName);
	DataType		GetTypeOf			(LPCTSTR pszPublicName);
	void			GetCalculatedColumns(CStringArray &arCalculatedColumns);
	int				GetTableIndex		(const CString& sTable);

	BOOL			RemoveLink		(LPCTSTR pszPublicName, BOOL bDeleteField);
	void			RemoveAllLinks	(BOOL bDeleteFields);
	BOOL			CanDeleteRule	(CString& sUsedFieldName, CString& sLog) const;
	virtual BOOL	CanDeleteField	(LPCTSTR) const;
	virtual void	DeleteField		(LPCTSTR pszPublicName) { RemoveLink(pszPublicName, TRUE); }
	virtual void	RenameField		(LPCTSTR pszOldName, LPCTSTR pszNewName);

	void			AddWhereClause		();
	WClause*		GetWhereClause		() const;
	void			AssignWhereClause	(const WClause&);

	void			AddHavingClause		();
	WClause*		GetHavingClause		() const;
	void			AssignHavingClause	(const WClause&);

	BOOL			Parse			(Parser& lex, BOOL bCheckUnique, BOOL bAllowUnknownVar);
	BOOL			Parse			(Parser& lex) { return Parse(lex, TRUE, FALSE); }
	void			Unparse			(Unparser&);
	CString			Unparse			();

	CString			ToNamedQuery	();
	CString			ToNamedQuerySelect();

	void			GetColumns		(CStringArray& columns) const;
	void			GetParameters	(CStringArray& parameters) const;

	BOOL			ExecQuery		(CString& sErr);
	BOOL			BuildQuery		(CString& sErr);

	BOOL			IsTemporaryJoin		()  const { return m_bTemporaryJoin; }
	void			SetTemporaryJoin	(BOOL t)	 { m_bTemporaryJoin = t; }
};

//=============================================================================
class TB_EXPORT ExpRuleData : public RuleDataObj
{
	friend class	CExpRuleDlg;

public:
	CString			m_strPublicName;

	Expression		m_TestExpr;
		Expression		m_ThenExpr;
		Expression		m_ElseExpr;

	Expression*		m_pValidateWhereExpr;

	BOOL m_bDisableUniqueConstraint;

public:
	ExpRuleData(WoormTable& aSymTable);
	virtual ~ExpRuleData();

public:
	RuleType		IsARule			();
	CString			GetPublicName	();
	void			SetPublicName(CString name){ m_strPublicName = name; }
	BOOL			ExistPublicName	(LPCTSTR pszName);

	BOOL			CanDeleteField	(LPCTSTR) const;
	void			DeleteField		(LPCTSTR);
	void			RenameField		(LPCTSTR pszOldFieldName, LPCTSTR pszNewFieldName);
	BOOL			IsEmpty			();

	BOOL			ParseId			(Parser&);
	BOOL			Parse			(Parser&, BOOL bParseWhereClause);
	BOOL			Parse			(Parser& lex) { return Parse(lex, TRUE); }

	void			Unparse			(Unparser&, BOOL bUnparseWhereClause, BOOL bUnparseId);
	void			Unparse			(Unparser& ofile)  { return Unparse(ofile, TRUE, TRUE); }
	CString			Unparse			(BOOL bUnparseWhereClause = TRUE, BOOL bUnparseId = TRUE);

	virtual CString		GetRuleDescription() const;

	void			Empty();
};

//=============================================================================
class TB_EXPORT WhileRuleData : public RuleDataObj
{
public:
	Expression*		m_pCondExpr;
	Block*			m_pBefore;
	Block*			m_pAfter;
	Block*			m_pBody;

	CStringArray	m_arWSelFieldName;

public:
	WhileRuleData(WoormTable& aSymTable);
	virtual ~WhileRuleData();

public:
	RuleType		IsARule			();
	CString			GetPublicName	();

	BOOL			ExistPublicName	(LPCTSTR pszName);

	BOOL			CanDeleteField	(LPCTSTR) const;
	void			DeleteField		(LPCTSTR);
	BOOL			IsEmpty			();

	BOOL			Parse			(Parser&);
	void			Unparse			(Unparser&);

	virtual CString		GetRuleDescription() const;
	void SetRuleFields(BOOL setRuleField);
};

//=============================================================================
class TB_EXPORT QueryRuleData : public RuleDataObj
{
	friend class CRSNamedQueryRuleProp;
	friend class CRS_ObjectPropertyView;

protected:
	CString			m_strQueryName;
	QueryObjItem*	m_pObjQuery = NULL;
	RuleSelectionMode m_ConstraintMode = RuleSelectionMode::SEL_ALL;
	Expression*		m_pWhenExpr = NULL;

public:
	QueryRuleData(WoormTable& aSymTable);

	CString			GetQueryName	() { return m_strQueryName; }
	QueryObjItem*	GetQueryItem	();
	void 			SetQueryItem	(QueryObjItem* pQ);

	RuleType		IsARule			();
	CString			GetPublicName	();

	BOOL			ExistPublicName	(LPCTSTR pszName);
	virtual BOOL	CanDeleteField	(LPCTSTR) const;
	virtual void	DeleteField		(LPCTSTR);
	virtual void	RenameField		(LPCTSTR pszOldName, LPCTSTR pszNewName);

	BOOL			IsEmpty			();

	BOOL			Parse			(Parser&);
	void			Unparse			(Unparser&);

	virtual CString		GetRuleDescription() const;

	CString GetSelectionModeDescr() const
		{ return TblRuleData::GetSelectionModeDescr(m_ConstraintMode); }

	void SetRuleFields(BOOL setRuleField);
};

#include "endh.dex"
