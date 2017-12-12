
#pragma once

#include <TbGenlib\TBToolBar.h>
#include <TbGenlib\parsctrl.h>
#include <TbOledb\sqlcatalog.h>
#include <TbOledb\sqltable.h>

//includere alla fine degli include del .H
#include "beginh.dex"

class Parser;
class Unparser;

class ProgramData;
class CriticalArea;

class DataObj;
class SqlTable;
class SqlTableInfo;

class WClause;
class CExpEdit;
class CStrEdit;
class CEqnEdit;

class CQueriesListBox;
class CAbstractDoc;

// Classi definite all'interno del Modulo
//=============================================================================
class QueryInfo;
class QueryObj;
class Queries;
	class TempQueries;
	class AllQueries;

class CQueryManager;
class SqlConnection;

enum TB_EXPORT enumQueryType {Active, Hidden, HiddenSelect, Predefinite, Temp};
enum TB_EXPORT enumQueryExec	 {UserAbort, Ok, Failed};

// Definizione di QueryInfo
//=============================================================================
class TB_EXPORT QueryInfo : public CObject
{
public:
	ProgramData*	m_pPrgData;
	SqlConnection*	m_pSqlConnection;
	SqlTableStruct	m_TableInfo;

	BOOL			m_bNativeExpr;

public:
	QueryInfo (const QueryInfo&);
	
	QueryInfo 
	(
				SqlConnection*	pSqlConnection,
		const	SqlTableInfoArray&	aTableInfo,					
		const	CString&		strQuery, 
		const	CString&		strTableName,
		const	CString&		strQueryName,
				LPCTSTR			szFromFileName = NULL
	);
	
	QueryInfo
	(
				SqlConnection*	pSqlConnection,
				BOOL			bNativeExpr	/*= FALSE*/,
				SqlTableStruct*	= NULL,
				ProgramData*	= NULL
	);
	~QueryInfo();

private:
	BOOL GenericParse			(Parser&, const CString&, const CString&, const	SqlTableInfoArray& arTableInfo, LPCTSTR);
	void GenericUnparse			(Unparser&, const CString&, const CString&);

public:
	BOOL Parse				  (const CString& strFile, const CString& strTable, const CString& strQuery, const	SqlTableInfoArray& arTableInfo);
	BOOL ParseFromQueryString (LPCTSTR szQueryString,   const CString& strTable, const CString& strQuery, const	SqlTableInfoArray& arTableInfo, LPCTSTR szFromFileName = NULL);
	BOOL Unparse			  (const CString& strFile, const CString& strTable, const CString& strQuery);
	BOOL UnparseInString	  (CString& lpszQuery,	   const CString& strTableName, const CString& strQueryName);

	void ResetAllFields();

	void SetProgramData (ProgramData*);
	void ReleaseProgramData ();

public:
	QueryInfo&	operator =	(const QueryInfo&);
	BOOL		operator ==	(const QueryInfo&)	const;
	BOOL		operator !=	(const QueryInfo&)	const;

// Diagnostics
#ifdef _DEBUG
public:
	void Dump		(CDumpContext&)	const;
	void AssertValid()				const { CObject::AssertValid(); }
#endif // _DEBUG
};

// Definizione di QueryObj
//=============================================================================
class TB_EXPORT QueryObj : public CObject
{
public:
	int				m_nExtIndex;
	enumQueryType	m_enumType;

	CTime			m_mtime;
	CString			m_strTableName;
	CString			m_strQueryName;

	QueryInfo*		m_pQueryInfo;
	SqlConnection* 	m_pSqlConnection;

public:
	QueryObj (const CString& strTable, QueryInfo*);
	QueryObj (SqlConnection*, const CString& strTable, const CString& strQuery, int nExtIndex, QueryInfo* = NULL);
	QueryObj (SqlConnection*, const CString& strTable, const CString& strQuery, int nExtIndex, enumQueryType);
	QueryObj (QueryInfo*);
	~QueryObj();

public:
	BOOL IsPredefiniteQuery	();

	void SaveQuery	(const CString& strQueryDir, const CString& strQueryFile);
	int  RemoveFile	(const CString& strQueryDir, const CString& strQueryFile);

	QueryInfo* GetQueryInfo (const CString& strQueryDir, const CString& strFileName, const SqlTableInfoArray& arTableInfo);
	void Unparse (Unparser&, BOOL bSelect);

// Diagnostics
#ifdef _DEBUG
public:
	void Dump		(CDumpContext&)	const;
	void AssertValid()				const { CObject::AssertValid(); }
#endif // _DEBUG
};

//	Definizione di AllQueries
//=============================================================================
class TB_EXPORT Queries : public Array
{
public:
	QueryObj* GetAt (int);

	int		Add		(QueryObj*);
	void	InsertAt(int, QueryObj*);

// Diagnostics
#ifdef _DEBUG
public:
	void Dump		(CDumpContext&) const;
	void AssertValid()				const { Array::AssertValid(); }
#endif // _DEBUG
};

//	Definizione di AllQueries
//=============================================================================
class TB_EXPORT TempQueries : public Queries
{
private:
	int m_nLastIdxTmpQuery;

public:
	TempQueries	();

public:
	int			IsExistQuery	(const CString&);
	void		DeleteQuery		(QueryObj*, BOOL bOwn = TRUE);
	void		FillList		(CQueriesListBox*);
	CString		GetTempQueryName();
	QueryObj*	AddQuery		(const CString& strQueryName, QueryInfo*);

#ifdef _DEBUG
public:
	void Dump		(CDumpContext&) const;
	void AssertValid()				const { Queries::AssertValid(); }
#endif // _DEBUG
};

//	Definizione di AllQueries
//=============================================================================
class TB_EXPORT AllQueries : public Queries
{
	friend class CQueryMenu;

protected:
	TempQueries*	m_pTempQueries;

	QueryObj*	m_pPredefiniteQuery;
	QueryObj*	m_pSortAsc;
	QueryObj*	m_pSortDesc;
	QueryObj*	m_pDefaultQuery;
	
	CTime		m_mtime;

private:
	CTBNamespace	m_nsCustomize;

public:
	SqlConnection*		m_pSqlConnection;
	QueryObj*			m_pSelectedQuery;
	SqlTable*			m_pTable;
	
	SqlTableStruct		m_TableInfo; //SqlTableInfoArray	m_TableInfo; //const SqlTableInfo*	m_pTableInfo;

public:
	AllQueries	();
	~AllQueries	();

	BOOL LoadQueries();
	BOOL Attach		(SqlTable*, BOOL bUsePredefined);

	QueryObj*	GetQueryObj		(int nIdx);
	QueryInfo*	GetQueryInfo	(QueryObj*);
	
	// namespace per il salvataggio delle customizzazioni
			void			SetCustomizeNamespace	(const CTBNamespace& aNs);
	const	CTBNamespace&	GetCustomizeNamespace	()							{ return m_nsCustomize; }

			CString		GetTempQueryName		();
	const	CString&	GetPredefiniteQueryName	();
	const	CString&	GetDefaultQueryName		();
	const	CString&	GetSelectedQueryName	();
			void		RefreshPredefiniteQuery	(SqlTable*);

public:
	BOOL ExecSelectedQuery (BOOL bFromAttach, CWnd* pParent, BOOL bDoQuery = TRUE);

	BOOL IsDefaultQuery		(QueryObj*);
	void FillList			(CQueriesListBox*);
	BOOL LoadChangedQueries ();

	int	Add			(QueryObj*);
	int	Add			(const CString& strTable, const CString& strQuery, int nExtIndex, enumQueryType);
	int AddToTemp	(QueryObj*);

	QueryObj* AddNewQuery		(const CString& strQuery, QueryInfo*, BOOL bAsDefault = FALSE);
	QueryObj* AddNewTempQuery	(const CString& strQuery, QueryInfo*);

	void InsertAtFromTemp	(int, QueryObj*);
	void InsertAt			(int, QueryObj*);
	void InsertAt			(int, const CString& strTable, const CString& strQuery, int nExtIndex, enumQueryType);

	BOOL DeleteQuery (QueryObj*);
	BOOL RenameQuery (const CString& strQueryName, QueryObj*);
	void UpdateQuery (
								QueryObj*,
								BOOL			bNativeExpr,
						const	CString&		strWClause,
						const	CString&		strOrderBy, 
								BOOL			bAsDefault,
								ProgramData*
					);

	BOOL MakeQueryOrderBy (const CStringArray& fieldArray, BOOL bDesc);

public:
	virtual CString GetQueryDir				();
	virtual CString	GetQueryFileName		();
	virtual CString GetQueryFileFullPathName(BOOL bFromSave = FALSE);

	CString GetTableName () const;

	BOOL SelectQuery	(const CString& strQuery);
	int IsExistQuery	(const CString& strQuery);
	int IsExistTempQuery(const CString& strQuery);

private:
	int FindHole	();
	int FindPos		(int nStart, int nEnd, int nExtIdxToFind);
	int GetNewPos	(int nStart, int nEnd, int nExtIdxToFind);

	enumQueryExec ExecQuery (BOOL bFromAttach, CWnd* pParent, BOOL bDoQuery);

	BOOL Unparse ();
	void SaveQueriesFile();

	QueryObj* GetAndResetTempQuery (const CString& szQueryName, ProgramData* = NULL);

// Diagnostics
#ifdef _DEBUG
public:
	void Dump		(CDumpContext&) const;
	void AssertValid()				const { Queries::AssertValid(); }
#endif // _DEBUG
};

/////////////////////////////////////////////////////////////////////////////
// Class used for Managed UI integration
/////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT CQueryMenuItem
{
public:
	CString m_strkey;
	CString m_strValue;
	
	CQueryMenuItem() {}
	CQueryMenuItem(CString key, CString value);
};

//	Definizione di CQueryManager
//=============================================================================
class TB_EXPORT CQueryManager : public CObject
{
	friend class CQueryMenu;

protected:
	AllQueries*		m_pAllQueries;
	CriticalArea*	m_pProcessing;
	BOOL			m_bDisableQuery;
	CAbstractDoc*	m_pDocument;
	UINT			m_nID;

public:
	CQueryManager(CAbstractDoc*, UINT nID);
	virtual ~CQueryManager();

public:
	void Attach (SqlTable*, BOOL bUsePredefined = FALSE, BOOL bPostMsg = TRUE, BOOL bExecQuery = TRUE);
	void Detach ();

	const CString GetCurrentQueryName() const { ASSERT_VALID (m_pAllQueries); return m_pAllQueries->GetSelectedQueryName (); }
	void SetQueryName	();
	void EditQuery		(BOOL bExecOnly = FALSE);
	void SelectQuery	(const CString& strQuery);
	void DisableQuery	(BOOL bDisable = TRUE)		{ m_bDisableQuery = bDisable; }
	BOOL IsExistQuery	(const CString& strQuery)	{ ASSERT_VALID (m_pAllQueries); return m_pAllQueries->IsExistQuery (strQuery) >= 0; }

	BOOL ExecSelectedQuery			(BOOL bFromAttach = FALSE, BOOL bPostMsg = TRUE, BOOL bDoQuery = TRUE);

	// usate solo dal radar
	BOOL MakeQueryOrderBy	(const CStringArray& fieldArray, BOOL bDesc = FALSE);
	BOOL CheckAllQueries	();

	const	CString&	GetDefaultQueryName		()	{ ASSERT_VALID (m_pAllQueries); return m_pAllQueries->GetDefaultQueryName(); }
	const	CString&	GetPredefiniteQueryName	()	{ ASSERT_VALID (m_pAllQueries); return m_pAllQueries->GetPredefiniteQueryName(); }

	void GetAllQueriesForMenu(CStringArray& arQueries);

	// Permette al programmatore di aggiornare la query predefinita che il QueryManager ha memorizzato insieme alle eventuali
	// altre personalizzate. Questo serve nel caso, ad es., per la query predefinita di browse cambino dei parametri
	// Il SqlTable* passato deve avere la query preparata, cosi come normalmente viene fatto in OnPrepareBrowser
	void RefreshPredefiniteQuery(SqlTable*);

	CFrameWnd* GetFrame() const;

// Diagnostics
#ifdef _DEBUG
public:
	void Dump		(CDumpContext&)	const;
	void AssertValid()				const { CObject::AssertValid(); }
#endif // _DEBUG
};


/////////////////////////////////////////////////////////////////////////////
// Menu for query
/////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT CQueryMenu : public CTBToolBarMenu
{
private:
	CQueryManager* m_pQueryManager;
	CBitmap	m_DefBitmap;  // per la query di default
	CBitmap	m_PredBitmap; // per la query predefinita	
	CBitmap	m_EditBitmap; // edit delle query
	CBitmap m_FindBitmap; // per la query di ricerca

public:
	CQueryMenu(CQueryManager*, CIdStringArray*, BOOL bShowQueryManager = FALSE);
	~CQueryMenu();
};

///////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////
//
// Funzioni Inline
//
///////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////
inline BOOL QueryObj::IsPredefiniteQuery () { return m_enumType == Predefinite; }

//-----------------------------------------------------------------------------
inline QueryObj* Queries::GetAt(int nIdx) { return (QueryObj*) Array::GetAt (nIdx); }

inline int	Queries::Add		(QueryObj* pObj)			{ return Array::Add (pObj);}
inline void Queries::InsertAt	(int nIdx, QueryObj* pObj)	{ Array::InsertAt (nIdx, pObj);}

//-----------------------------------------------------------------------------
inline QueryObj* TempQueries::AddQuery (const CString& strQuery, QueryInfo* pObj)
	{ return GetAt (Add (new QueryObj (strQuery, pObj))); }

//-----------------------------------------------------------------------------
inline const CString& AllQueries::GetPredefiniteQueryName ()
	{ ASSERT_VALID (m_pPredefiniteQuery); return m_pPredefiniteQuery->m_strQueryName; }
inline const CString& AllQueries::GetSelectedQueryName ()
	{ ASSERT_VALID (m_pSelectedQuery); return m_pSelectedQuery->m_strQueryName; }
inline const CString& AllQueries::GetDefaultQueryName ()
	{ ASSERT_VALID (m_pDefaultQuery); return m_pDefaultQuery->m_strQueryName; }
inline CString AllQueries::GetTempQueryName ()
	{ ASSERT_VALID (m_pTempQueries); return m_pTempQueries->GetTempQueryName (); }

inline int AllQueries::Add (QueryObj* pObj) { return Queries::Add (pObj);}
inline int AllQueries::Add (const CString& strTable, const CString& strQuery, int nExtIdx, enumQueryType status)
	{ return Add (new QueryObj (m_pSqlConnection, strTable, strQuery, nExtIdx, status));}

inline void AllQueries::InsertAt (int nIdx, QueryObj* pObj) { Queries::InsertAt (nIdx, pObj);}
inline void AllQueries::InsertAt (int nIdx, const CString& strTable, const CString& strQuery, int nExtIdx, enumQueryType status)
	{ InsertAt (nIdx, new QueryObj (m_pSqlConnection, strTable, strQuery, nExtIdx, status));}

inline BOOL AllQueries::IsDefaultQuery (QueryObj* pObj) { return pObj == m_pDefaultQuery; }


///////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////
//
// Funzioni di utilità generale usate in altri moduli
//
///////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////
TB_EXPORT BOOL IsTempQueryName	(const CString& strQueryName);
TB_EXPORT BOOL IsProgramQueryName	(const CString& strQueryName);

TB_EXPORT LPCTSTR GetProgramQueryName ();

#include "endh.dex"
