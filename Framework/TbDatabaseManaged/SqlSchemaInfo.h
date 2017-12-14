#pragma once

#include <TbGeneric\Array.h>
#include <TbGeneric\dataobj.h>


//includere alla fine degli include del .H
#include "beginh.dex"


///////////////////////////////////////////////////////////////////////////////
//								SqlTablesItem
///////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT SqlTablesItem : public CObject
{
public:
	// Column info retrieved by SQL direct call
	CString		m_strQualifier;
	CString 	m_strOwner;
	CString 	m_strName;
	CString 	m_strType;
	CString 	m_strRemarks;

	// constructor	
public:
	SqlTablesItem();
	SqlTablesItem(const SqlTablesItem&);
};

///////////////////////////////////////////////////////////////////////////////
//								SqlColumnInfo
///////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT SqlColumnInfoObject : public CObject
{
public:
	// Column info retrieved by SQL direct call
	CString		m_strTableName;
	CString		m_strColumnName;
	CString		m_strCollatioName;
	CString		m_strColumnTitle;

	CString		m_strTableCatalog;
	CString		m_strTableSchema;
	CString		m_strTypeName;
	CString		m_strRemarks;

	SWORD		m_nSqlDataType;
	CString		m_strSqlDataType; //NEWDBLAYER
	long		m_lPrecision;
	long		m_lLength;
	int			m_nScale;
	int			m_nRadix;
	int			m_nDecimal;
	BOOL		m_bLoadedFromDB;
	BOOL		m_bNullable;
	volatile bool	m_bSpecial;		// Utilizzato per individuare univocamente la riga (o segmento di chiave primaria (se definita) o special column)

	// constructor	
public:
	SqlColumnInfoObject();
	SqlColumnInfoObject(const SqlColumnInfoObject&);
	SqlColumnInfoObject::SqlColumnInfoObject
	(
		const	CString&	strTableName,
		const	CString&	strColumnName,
		const	DataObj&	aDataObj	);

	virtual	BOOL IsEqual(const SqlColumnInfoObject& cf) const;
	// diagnostics
#ifdef _DEBUG
public:
	void Dump(CDumpContext& dc) const;
	void AssertValid() const { CObject::AssertValid(); }
#endif //_DEBUG
};



///////////////////////////////////////////////////////////////////////////////
//								SqlProcedureParamInfo
///////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT SqlProcedureParamInfo : public CObject
{
public:
	// Column info retrieved by SQL direct call
	CString		m_strProcCatalog;
	CString		m_strProcSchema;
	CString		m_strProcName;
	CString		m_strParamName;
	short       m_nOrdinalPosition;
	short	    m_nType;
	BOOL	    m_bHasDefault;
	CString     m_strDefault;
	BOOL		m_bIsNullable;
	short       m_nDataType;
	long		m_nMaxLength;
	long		m_nOctetLength;
	short		m_nPrecision;
	short       m_nScale;
	CString     m_strDescription;

	// constructor	
public:
	SqlProcedureParamInfo() {}
	SqlProcedureParamInfo(const SqlProcedureParamInfo&);

public:
	// Aggiorna i dati correlati al dataobj
	void UpdateDataObjInfo(DataObj* pDataObj);

	// diagnostics
#ifdef _DEBUG
public:
	void Dump(CDumpContext& dc) const;
	void AssertValid() const { CObject::AssertValid(); }
#endif //_DEBUG
};

///////////////////////////////////////////////////////////////////////////////
//								SqlProcedureParameters
///////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT SqlProcedureParameters :public ::Array
{
public:
	// accessing elements
	SqlProcedureParamInfo *		GetAt(int nIndex) const { return (SqlProcedureParamInfo *)Array::GetAt(nIndex); }
	SqlProcedureParamInfo *&	ElementAt(int nIndex) { return (SqlProcedureParamInfo *&)Array::ElementAt(nIndex); }

	// overloaded operator helpers
	SqlProcedureParamInfo *		operator[]	(int nIndex) const { return GetAt(nIndex); }
	SqlProcedureParamInfo *&	operator[]	(int nIndex) { return ElementAt(nIndex); }
};

#include "endh.dex"