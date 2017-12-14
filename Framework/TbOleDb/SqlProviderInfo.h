
#pragma once

#include <TbGeneric\DataObj.h>

#include "sqlobject.h"

//includere alla fine degli include del .H
#include "beginh.dex"

class OdbcDriverSection;
class SqlConnection;


// Default Precision e Scale per i tipi SQL_DECIMAL e SQL_NUMERIC
//-----------------------------------------------------------------------------
#define LONG_PRECISION	10
#define LONG_SCALE		0
#define INT_PRECISION	6
#define INT_SCALE		0
#define ENUM_PRECISION	10
#define ENUM_SCALE		0
#define BIT_PRECISION	1
#define BIT_SCALE		0

#define DBL_PRECISION	19
#define DBL_SCALE		4
#define MONEY_PRECISION	19
#define MONEY_SCALE		4
#define QTA_PRECISION	10
#define QTA_SCALE		3
#define PERC_PRECISION	7
#define PERC_SCALE		2

//in questo file sono contenute le dichiarazioni delle classi per la gestione
// delle informazioni relative al provider utilizzato
//
// SqlColumnTypeItem : informazioni relative al singolo tipo di dato gestito dal provider
// SqlColumnTypeArray : insieme dei tipi di dato gestiti dal provider
// SqlProviderInfo : gestore delle informazioni relative al provider
// SqlProviderInfoPool : insieme delle informazioni relative ai provider utilizzati dalle
//						 connessioni attive 



TB_EXPORT BOOL	CheckTypeCompatibility	(const DataType&, DBTYPE);


//////////////////////////////////////////////////////////////////////////////
//					SqlColumnTypeItem Definition
//////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
class TB_EXPORT SqlColumnTypeItem : public CObject
{
	friend class SqlColumnTypeArray;
	friend class SqlProviderInfo;
	friend class SqlStmt;
	
	DECLARE_DYNAMIC (SqlColumnTypeItem)
	
protected:
	DataType	m_DataObjType;
	SWORD		m_nSqlDataType;

public:
	SqlColumnTypeItem
		(
			const DataType&	aDataObjType,
			SWORD			nSqlDataType
		);
	virtual ~SqlColumnTypeItem () {}


	
// diagnostics
#ifdef _DEBUG
public:	
	void Dump(CDumpContext& dc) const {	ASSERT_VALID(this); AFX_DUMP0(dc, " SqlColumnTypeItem\n"); }
	void AssertValid() const{ CObject::AssertValid(); }
#endif //_DEBUG
};

//////////////////////////////////////////////////////////////////////////////
//					SqlColumnTypeArray definition
//////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
class TB_EXPORT SqlColumnTypeArray : public Array
{
	DECLARE_DYNAMIC (SqlColumnTypeArray)

public:
	SqlColumnTypeItem* 	GetAt		(int nIndex)const	{ return (SqlColumnTypeItem*) Array::GetAt(nIndex);	}
	SqlColumnTypeItem*&	ElementAt	(int nIndex)		{ return (SqlColumnTypeItem*&) Array::ElementAt(nIndex); }
	
	SqlColumnTypeItem* 	operator[](int nIndex)	const	{ return GetAt(nIndex);	}
	SqlColumnTypeItem*&	operator[](int nIndex)			{ return ElementAt(nIndex);	}
	
// diagnostics
#ifdef _DEBUG
public:	
	void Dump(CDumpContext& dc) const {	ASSERT_VALID(this); AFX_DUMP0(dc, " SqlColumnTypeArray\n"); }
	void AssertValid() const{ Array::AssertValid(); }
#endif //_DEBUG
};

// descrive le proprietà dipedenti dal provider
// due tipologie: quelle ricavibili dalle propriety del provider attraverso chiamate
// OLEDB e quelle parametrizzabili attraverso il SysAdmin dall'amministratore di 
// sistema
//////////////////////////////////////////////////////////////////////////////
//					COLEDBProviderProperties definition
//////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
class TB_EXPORT COLEDBProviderProperties 
{
public:	
	// queste sono richieste al'OLEDB Provider
	BOOL		m_bTxnCapable;
	BOOL		m_bMultiStorageObj;
	DWORD		m_dwTxnIsolationOptions;
	DWORD		m_dwCursorCommitBehavior;
	DWORD		m_dwCursorAbortBehavior;
	DWORD		m_dwIdentifierCase;
	TCHAR		m_chIDQuoteChar;
	CString		m_strQuote;

public:
	COLEDBProviderProperties();

public:
	BOOL GetProperties(SqlConnection*);
};


/////////////////////////////////////////////////////////////////////////////
//					CSysAdminProviderParams definition
//////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
class TB_EXPORT CSysAdminProviderParams 
{
public:	
	::DBMSType	m_eDBMSType;
	BOOL		m_bUseConstParameter;
	BOOL		m_bStripTrailingSpaces;
	BOOL		m_bUseUnicode;

public:
	CSysAdminProviderParams(::DBMSType);
	
public:
	BOOL ReadParams(const long& nProviderID);

private:
	void InitParams();

};

//////////////////////////////////////////////////////////////////////////////
//					SqlProviderInfo definition
//////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
class TB_EXPORT SqlProviderInfo : public SqlObject 
{
	friend class SqlProviderInfoPool;
	friend class SqlConnection;
	friend class COleDbParamsDlg;

	DECLARE_DYNAMIC(SqlProviderInfo)

public:
	long		m_nProviderId;
	CString		m_strProviderName;
    CString		m_strProviderVer;
	::DBMSType	m_eDbmsType; 	
	BOOL		m_bTransactions;
				

	COLEDBProviderProperties*	m_pProviderProperties;
	CSysAdminProviderParams*	m_pSysAdminParams;
	
protected:
	SqlColumnTypeArray*	m_pSqlColumnTypeArray; 

private:
	BOOL m_bInitialized;
	BOOL m_bUseUnicode;

public:
	SqlProviderInfo	(LPCTSTR pszProviderName, LPCTSTR pszProviderVersion, const long& nProviderId = -1);
	~SqlProviderInfo	();

private:
	void ReadProviderParameter();
	LPCTSTR GetNativeDateFormat(DBTYPEENUM eDBType) const;

protected:
	void BuildColumnTypeArray			();
	void BuildColumnTypeArrayOracle		(); 
	void BuildColumnTypeArraySqlServer	(); 
	
public:
	BOOL		LoadProviderInfo(SqlConnection*);
	CString		NativeConvert	(const DataObj*) const;
	CString		NativeConvert	(const DataObj* pDataObj, const BOOL& bUseUnicode) const;

	// assegna/ritorna il DbmsType
	void		AssignDbmsType	();
	virtual ::DBMSType GetDBMSType() const;

	// DataObj dependent function
	DBTYPE				GetSqlDataType			(const DataType&) const;
	SqlColumnTypeItem*	GetSqlColumnTypeItem	(const DataType&) const;

	// restituisce TRUE se dopo una commit/rollback i rowset sono ancora validi
	// controlla i parametri m_pProviderProperties->m_dwCursorCommitBehavior e
	// m_pProviderProperties->m_dwCursorAbortBehavior
	// a seconda del valore del parametro: se TRUE m_pProviderProperties->m_dwCursorCommitBehavior
	// se FALSE m_pProviderProperties->m_dwCursorAbortBehavior 
	BOOL CursorPreserveBehavior(BOOL bForCommit) const; 

public:
	//per la parametrizzazione
	DWORD	TxnIsolationOptions	() const { return m_pProviderProperties->m_dwTxnIsolationOptions;}
	DWORD	CursorCommitBehavior() const { return m_pProviderProperties->m_dwCursorCommitBehavior;}
	DWORD	CursorAbortBehavior	() const { return m_pProviderProperties->m_dwCursorAbortBehavior;}
	DWORD	IdentifierCase		() const { return m_pProviderProperties->m_dwIdentifierCase;}
	TCHAR	IDQuoteChar			() const { return m_pProviderProperties->m_chIDQuoteChar;}
	CString	StringQuote			() const { return m_pProviderProperties->m_strQuote;}
	BOOL	MultiStorageObjects	() const { return m_pProviderProperties->m_bMultiStorageObj;}
	

	BOOL	UseConstParameter	() const { return m_pSysAdminParams->m_bUseConstParameter; }
	BOOL	StripSpaces			() const { return m_pSysAdminParams->m_bStripTrailingSpaces; }
	BOOL	UseUnicode			() const { return m_pSysAdminParams->m_bUseUnicode;			}


	
#ifdef _DEBUG
public:	
	//@@OLE da sostituire con SqlObject
	void Dump(CDumpContext& dc) const {	ASSERT_VALID(this); AFX_DUMP0(dc, " SqlProviderInfo\n"); CObject::Dump(dc);}
	void AssertValid() const{ CObject::AssertValid(); } 
#endif //_DEBUG
};

//////////////////////////////////////////////////////////////////////////////
//						SqlProviderInfoPool
//////////////////////////////////////////////////////////////////////////////
//=============================================================================
class TB_EXPORT SqlProviderInfoPool : public Array
{
	DECLARE_DYNAMIC(SqlProviderInfoPool)

public:                                  
  	SqlProviderInfo* 	GetAt		(int nIndex) const	{ return (SqlProviderInfo*) Array::GetAt(nIndex);	}
	SqlProviderInfo*&	ElementAt	(int nIndex)		{ return (SqlProviderInfo*&) Array::ElementAt(nIndex); }
	
	SqlProviderInfo* 	operator[]	(int nIndex) const	{ return GetAt(nIndex);	}
	SqlProviderInfo*&	operator[]	(int nIndex)		{ return ElementAt(nIndex);	}

	int					Add(SqlProviderInfo*);

public:
	int					GetProviderInfoIdx	(const CString& strProviderName, BOOL bUseUnicode) const;
  	SqlProviderInfo* 	GetProviderInfo		(const CString& strProviderName, BOOL bUseUnicode) const;

// diagnostics
#ifdef _DEBUG
public:	
	void Dump(CDumpContext& dc) const {	ASSERT_VALID(this); AFX_DUMP0(dc, " SqlProviderInfoPool\n"); }
	void AssertValid() const{ Array::AssertValid(); }
#endif //_DEBUG
};


#include "endh.dex"

