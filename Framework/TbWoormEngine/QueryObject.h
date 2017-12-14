
#pragma once

#include <TbOleDb\SqlTable.h>

//includere alla fine degli include del .H
#include "beginh.dex"
//==========================================================================
class SymTable;
class Parser;
class SqlConnection;
class TagLink;

//------------------------------------------------------------------------------
class SqlRecordProcedureQuery : public SqlRecordProcedure
{
	DECLARE_DYNCREATE(SqlRecordProcedureQuery) 

public:
	DataLng		f_Ret_Result;

public:
	SqlRecordProcedureQuery();

public:
	virtual void	BindRecord	();	
};


///////////////////////////////////////////////////////////////////////////////
class TB_EXPORT QueryObject : public QueryObjectBase
{
	DECLARE_DYNAMIC(QueryObject)

public:
	enum Direction { _IN, _OUT, _INOUT, _COL, _EXPAND, _INCLUDE, _EVAL };

protected:
	CString			m_strQueryName;
	CString			m_strQueryTemplate; //originale, completa dei tag

	CString			m_strSql;	//senza i tag, con i segnaposto dei parametri

	long			m_nQueryHandle;
	
	SymTable*		m_pSymbolTable;

	SqlSession*		m_pSqlSession;
	SqlTable*		m_poSqlTable;
	SqlRecord*		m_poSqlRecord;

	Array			m_TagLinks;
	QueryObject*	m_pParent;

	CMessages		m_msg;
public:
	BOOL			m_bValorizeAll = TRUE;

private:
	Array			m_ObjectBag;
	RecordArray		m_RecordBag;

	BOOL			m_bUseCursor;
	CursorType		m_CursorType;
	BOOL			m_bCursorUpdatable;
	BOOL			m_bSensibility;

	BOOL			m_bIsQueryRule;

	CStringArray	m_arAllSelectedField;
	CStringArray	m_arAllParameters;
	CStringArray	m_arExternalParameters;

	CStringArray	m_arSelFieldName;
	CStringArray	m_arParametersName;

public:
	QueryObject (SymTable* pSymbolTable, SqlSession* pSession);
	QueryObject (const CString& sName, SymTable* pSymbolTable, SqlSession* pSession, QueryObject* pParent = NULL);
	virtual ~QueryObject ();

	virtual BOOL	Parse		(Parser& parser);
	BOOL			Unparse		(Unparser& unparser, BOOL bSkipHeader = FALSE, BOOL bSkipBeginEnd = FALSE) const;
	CString			Unparse		() const;

 	void	SetQueryTemplate	(const CString& sQueryTemplate)		{ m_strQueryTemplate = sQueryTemplate; }
 	CString	GetQueryTemplate	() const							{ return m_strQueryTemplate; }

	CString GetName			() const							{ return m_strQueryName; }
	void	SetName			(const CString& sName)				{ m_strQueryName = sName; }

	SymTable* GetSymTable() const { return m_pSymbolTable ; }

	long	GetHandle		() const							{ return m_nQueryHandle; }
	void	SetHandle		(long h)							{ m_nQueryHandle = h; }
	
	void	SetSqlSession(SqlSession* pSqlSession)				{ m_pSqlSession = pSqlSession; }

	void	SetQueryRule		(BOOL set = TRUE)								{ m_bIsQueryRule = set; }

	DataBool	Define			(DataStr sSql);
	DataBool	Open			();
	DataBool	Read			();
	DataBool	Close			();
	DataBool	Execute			();
	DataBool	ReadOne			();

	DataBool	Call			();

	DataStr		GetColumnName	(DataInt col);
	DataObj*	GetData			(DataStr sName);
	DataStr		GetValue		(DataStr sName);

	BOOL	IsEof			() const;
	BOOL	IsBof			() const;
	BOOL	IsEmpty			() const;

	BOOL	IsOpen			() const;

	CString GetSql			() const	{ return m_strSql; }
	CString	GetError		() 			{ return m_msg.ToString(); }
	BOOL	IsFailed		()			{ return !((m_msg.ToString()).IsEmpty()); }

	BOOL	SetError		(LPCTSTR szErr, LPCTSTR szAuxErr = NULL);
	BOOL	ShowError		();
	CString	GetQueryName	();

	SqlTable* GetSqlTable	() { return m_poSqlTable;} 

	void SetCursorType(CursorType cursorType, BOOL bCursorUpdatable = FALSE , BOOL bSensibility = TRUE)
		{ m_bUseCursor = TRUE; m_CursorType = cursorType; m_bCursorUpdatable = bCursorUpdatable; m_bSensibility = bSensibility; }

	CString ToSqlString() const ;
	
	//PARSED - all tags - no duplicate
	const CStringArray& AllQueryColumns()			const { return m_arAllSelectedField; }
	const CStringArray& AllQueryParameters()		const { return m_arAllParameters; }
	const CStringArray& ExternalQueryParameters()	const { return m_arExternalParameters; }	//when and eval expression

	BOOL	HasMember		(LPCTSTR pszName) const;	//check field used by when and eval expression too
	BOOL	HasColumn		(LPCTSTR pszName) const;

	//post BUILD - only expanded tags
	const CStringArray& GetCurrentQueryColumns() const { return m_arSelFieldName; }
	const CStringArray& GetCurrentQueryParameters() const { return m_arParametersName; }

	virtual CString		GetSelFieldName(int i)	const { return m_arSelFieldName.GetAt(i); }
	virtual int			GetSelFieldsNum()		const { return m_arSelFieldName.GetSize(); }

	void RenameField	(LPCTSTR pszOldName, LPCTSTR pszNewName);
	void DeleteField	(LPCTSTR pszName);
protected:
	BOOL	IsSubQuery		() const { return m_strQueryName.IsEmpty(); }
	BOOL	ParseInternal	(Parser& parser);
	BOOL	ParseTag		(Parser& parser);
	QueryObject* ParseSubQuery (Parser& parser);

	int 	AddLink			(LPCTSTR pszName, Direction direction, DataObj* pData, int nLen = 0, Expression* pWhenExpr = NULL, QueryObject* pExpandClause = NULL);
	TagLink* GetColumn		(const CString& name);
	
	void	Clear			();
	BOOL	Build			();
	BOOL	ExpandTemplate (CString& strSql); //resolve Expand/Include
	BOOL	BindColumn		(SqlTable* pSqlTable, SqlRecord* pSqlRecord, int& nBind);
	BOOL	BindParameter	(SqlTable* pSqlTable, SqlRecord* pSqlRecord, int& nBind, BOOL bIsProcedure = FALSE);
	BOOL	ValorizeColumns (BOOL bFetched = TRUE, SqlTable* paramTable =NULL);
	BOOL	ReplaceInputParameters (CString& sSql, int& nStartQuestionMarkPos);

private:
	int		AddAllColumn		(LPCTSTR pszName);
	int		AddAllParameters	(LPCTSTR pszName);
	int		AddCurrentColumn	(LPCTSTR pszName);
	int		AddCurrentParameter (LPCTSTR pszName);
	void	SetCurrentQueryColumns();
	void	SetCurrentQueryParameters();

// Diagnostics
#ifdef _DEBUG
public:
	void Dump (CDumpContext& dc) const	
		{ ASSERT_VALID(this); AFX_DUMP0(dc, " QueryObject\n");}
#endif // _DEBUG
};

///////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT TagLink : public CObject
{
public:
	CString	m_strName;
	CString	m_strSqlName;
	CString	m_strTitle;

	QueryObject::Direction m_Direction;

	Expression*	m_pWhenExpr = NULL;

	QueryObject* m_pExpandClause = NULL;
	QueryObject* m_pElseClause = NULL;

	int m_nLen = 0;
	DataObj* m_pData = NULL;
	BOOL m_bWhen;

public:
	TagLink(LPCTSTR pszPublicName, QueryObject::Direction direction, DataObj*, int, Expression* pWhenExpr = NULL, QueryObject* pExpandClause = NULL);
	~TagLink();
};


//=============================================================================
#include "endh.dex"
