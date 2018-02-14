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

	::Array		m_arColumnsInfo; //array contenente le columnsInfo
	::Array		m_arProcedureParams; //array contenente le paramsinfo x le stored procedure

	// constructor	
public:
	SqlTablesItem();
	//SqlTablesItem(const SqlTablesItem&);
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

	CRuntimeClass*	m_RuntimeClass;
	BOOL			m_bVirtual;				// Indica che il dato non e' in tabella ma e' virtuale
	BOOL			m_bIndexed;				// é una colonna su cui é stato definito un indice
	BOOL			m_bNativeColumnExpr;	// é una espressione sql: count(*), Max(col), etc
	volatile bool	m_bAutoIncrement;	// é una colonna di tipi identity
	volatile bool	m_bDataObjInfoUpdated;
	volatile bool	m_bVisible;
	volatile bool	m_bSpecial;		// Utilizzato per individuare univocamente la riga (o segmento di chiave primaria (se definita) o special column)


	DataType	m_DataObjType;	// DataObj Usato dal programmatore o scelto dall'utente (in woorm)
	BOOL		m_bUseCollationCulture;
	
#ifdef _DEBUG
	CRuntimeClass* m_pOwnerSqlRecordClass;
#endif
	// constructor	

	// constructor	
public:
	SqlColumnInfoObject();
	SqlColumnInfoObject(const SqlColumnInfoObject&);
	SqlColumnInfoObject
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
	SWORD		m_nSqlDataType;
	CString		m_strSqlDataType; //NEWDBLAYER

	long		m_lLength;
	long		m_lPrecision;
	short       m_nScale;
	CString     m_strDescription;

private:
	bool m_bDataObjInfoUpdated;

	// constructor	
public:
	SqlProcedureParamInfo();
	SqlProcedureParamInfo(const SqlProcedureParamInfo&);

public:
	void UpdateResultValueType(DataObj* pResDataObj);
	// Aggiorna i dati correlati al dataobj
	void UpdateDataObjInfo(DataObj* pDataObj);

	// diagnostics
#ifdef _DEBUG
public:
	void Dump(CDumpContext& dc) const;
	void AssertValid() const { CObject::AssertValid(); }
#endif //_DEBUG
};


#include "endh.dex"