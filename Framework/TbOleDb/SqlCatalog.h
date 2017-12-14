 
#pragma once

#include <TbGeneric\Array.h>
#include <TbGeneric\dataobj.h>
#include <TbGenlib\addonmng.h>
#include <TbNameSolver\TBNamespaces.h>

#include "sqlproviderinfo.h"
#include "TbExtensionsInterface.h"

//includere alla fine degli include del .H
#include "beginh.dex"

class SqlTables;
class SqlCatalogEntry;
class SqlConnection;
class SqlUniqueColumns;
class SqlSession;
class SqlColumnArray;
class CXMLFixedKeyArray;
class SqlTableItem;


//in questo file sono presenti le classi per la gestione del catalog
//
// SqlColumnInfo = gestisce le proprietá di una singola colonna
// SqlTablesItem = singola tabella
// SqlTables = elenco delle tabelle presenti nel database
// CRTAddOnNewFields e CRTAddOnNewFieldsArray = per la gestione dei campi aggiunti da terze parti
// SqlTableInfo = informazioni di catalot legate ad una singola tabella
// SqlTableInfoArray = array contenente le informazioni di catalog di tutte le tabelle
// SqlIndexInfo = informazioni di un indice presente nel database
// SqlIndexesArray = informazioni di tutti gli indici presenti nel database
// SqlUniqueColumns = colonne che individuono univocamnete una riga di un set (o chiave primaria o specialcolumns)
// SqlCatalogEntry = elemento del catalog 
// SqlCatalog = elemento del catalog 
// SqlTypeInfoItem e SqlTypeInfo = per la gestione del typing
// SqlProcedureParamInfo =  per le info relative ad un parametro di una stored procedure
// per le colonne di result invece utilizzo le columninfo

//dato il tipo di tabella, view o procedure restituisce la stringa corrispondente
CString GetTypeString(int nType);

///////////////////////////////////////////////////////////////////////////////
//								SqlColumnInfo
///////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT SqlColumnInfo : public CObject, public CTBLockable
{
public:
	// Column info retrieved by SQL direct call
	CString		m_strTableName;
	CString		m_strColumnName;

	CString		m_strTableCatalog; 
	CString		m_strTableSchema;
	CString		m_strTypeName;		
	CString		m_strRemarks;

	DataType	m_DataObjType;	// DataObj Usato dal programmatore o scelto dall'utente (in woorm)
	BOOL		m_bUseCollationCulture;		
	
	SWORD		m_nSqlDataType;		
	long		m_lPrecision;
	long		m_lLength;
	int			m_nScale;
	int			m_nRadix;
	int			m_nDecimal;

	CRuntimeClass* m_RuntimeClass;
	
	BOOL		m_bNullable;
	BOOL		m_bVirtual;				// Indica che il dato non e' in tabella ma e' virtuale
	BOOL		m_bIndexed;				// é una colonna su cui é stato definito un indice
	BOOL		m_bNativeColumnExpr;	// é una espressione sql: count(*), Max(col), etc
	
	volatile bool m_bSpecial;		// Utilizzato per individuare univocamente la riga (o segmento di chiave primaria (se definita) o special column)
	volatile bool m_bAutoIncrement;	// é una colonna di tipi identity
	volatile bool m_bVisible;
	volatile bool m_bDataObjInfoUpdated;
	BOOL		  m_bLoadedFromDB;

#ifdef _DEBUG
	CRuntimeClass* m_pOwnerSqlRecordClass;
#endif
// constructor	
public:
	SqlColumnInfo();	
	SqlColumnInfo(const SqlColumnInfo&);	
	SqlColumnInfo::SqlColumnInfo
		(
			const	CString&	strTableName, 
			const	CString&	strColumnName,
			const	DataObj&	aDataObj
		);

	virtual LPCSTR  GetObjectName() const { return "SqlColumnInfo"; }

	BOOL IsEqual (const SqlColumnInfo& cf) const;
	
public:                    
	long			GetColumnLength	() const	{ return m_lLength; }
	int				GetColumnDecimal() const	{ return m_nDecimal; }
	DataType 		GetDataObjType() const;// { return m_DataObjType; }
	const CString&	GetTableName	() const	{ return m_strTableName; }
	const CString&	GetColumnName	() const	{ return m_strColumnName; }
	CString			GetQualifiedColumnName	() const	{ return m_strTableName + '.' + m_strColumnName; }
	CString			GetColumnTitle	() const;

	// Aggiorna i dati correlati al dataobj
	void SetDataObjInfo	(DataObj* pDataObj) const;
	void UpdateDataObjType	(DataObj* pDataObj); 
	void ForceUpdateDataObjType	(DataObj* pDataObj);
	BOOL GetDataObjTypes	(CWordArray& aDataObjTypes) const;

// diagnostics
#ifdef _DEBUG
public:	
	void Dump(CDumpContext& dc) const;
	void AssertValid() const{ CObject::AssertValid(); }
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
	void UpdateDataObjInfo	(DataObj* pDataObj);
	
// diagnostics
#ifdef _DEBUG
public:	
	void Dump(CDumpContext& dc) const;
	void AssertValid() const{ CObject::AssertValid(); }
#endif //_DEBUG
};

///////////////////////////////////////////////////////////////////////////////
//								SqlProcedureParameters
///////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT SqlProcedureParameters :public Array
{
public:
	// accessing elements
	SqlProcedureParamInfo *		GetAt			(int nIndex) const	{ return (SqlProcedureParamInfo *) Array::GetAt(nIndex); }
	SqlProcedureParamInfo *&	ElementAt		(int nIndex)		{ return (SqlProcedureParamInfo *&) Array::ElementAt(nIndex); }
	
	// overloaded operator helpers
	SqlProcedureParamInfo *		operator[]	(int nIndex) const	{ return GetAt(nIndex); }
	SqlProcedureParamInfo *&	operator[]	(int nIndex)		{ return ElementAt(nIndex); }
};


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
//								SqlTables
///////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT SqlTables : public Array
{
	DECLARE_DYNAMIC(SqlTables)
private:
	SqlConnection* m_pSqlConnection;

public:
	SqlTables(SqlConnection*);

private:
	BOOL IsSystemTable(TCHAR*, TCHAR*, ::DBMSType) const;
	BOOL GetInfo(BOOL bTable);

public:
	// accessing elements
	SqlTablesItem*	GetAt		(int nIndex) const	{ return (SqlTablesItem*) Array::GetAt(nIndex); }
	SqlTablesItem*&	ElementAt	(int nIndex)		{ return (SqlTablesItem*&) Array::ElementAt(nIndex); }
	
	// overloaded operator helpers
	SqlTablesItem*	operator[]	(int nIndex) const	{ return GetAt(nIndex); }
	SqlTablesItem*&	operator[]	(int nIndex)		{ return ElementAt(nIndex); }

private:
	BOOL GetSynonyms(const CString& strType);

public:
	BOOL GetTables()			{ return GetInfo(TRUE);  }
	BOOL GetViews()				{ return GetInfo(FALSE); }
	BOOL GetStoredProcedures();
	
// diagnostics
#ifdef _DEBUG
public:	
	void Dump(CDumpContext& dc) const {	ASSERT_VALID(this); AFX_DUMP0(dc, " SqlTables\n"); }
	void AssertValid() const{ Array::AssertValid(); }
#endif //_DEBUG
};

///////////////////////////////////////////////////////////////////////////////
//								CRTAddOnNewFields
///////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT CRTAddOnNewFields :public CObject
{
public:
	CRuntimeClass*	m_pRuntimeClass;	// puntatore alla runtime della classe che gestisce i nuovi campi aggiunti
	CString			m_strSignature;		// signature dell'AddOnApplication di proprietà della classe che gestisce i nuovi campi aggiunti
	CRuntimeClass*	m_pSqlRecordClass;
	CTBNamespace	m_nsOwnerLibrary;

public:
	CRTAddOnNewFields(CRuntimeClass*, const CString&, CRuntimeClass*, const CTBNamespace& nsOwnerLibrary);
	~CRTAddOnNewFields();
};


// contiene le Runtime e le signature delle classi che gestiscono i campi aggiunti. 
// Viene utilizzato per la creazione degli oggetti istanza

// Attenzione la RuntimeClass deve essere univoca. Non possiamo avere la stessa RuntimeClass
// per signature differenti
///////////////////////////////////////////////////////////////////////////////
//								CRTAddOnNewFieldsArray
///////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT CRTAddOnNewFieldsArray : public Array
{
public:
	~CRTAddOnNewFieldsArray();

	// accessing elements
	CRTAddOnNewFields *		GetAt			(int nIndex) const	{ return (CRTAddOnNewFields *) Array::GetAt(nIndex); }
	CRTAddOnNewFields *&	ElementAt		(int nIndex)		{ return (CRTAddOnNewFields *&) Array::ElementAt(nIndex); }
	CRuntimeClass*			GetRuntimeClass	(int nIndex) const	{ return GetAt(nIndex)->m_pRuntimeClass; }
	
	// overloaded operator helpers
	CRTAddOnNewFields *		operator[]	(int nIndex) const	{ return GetAt(nIndex); }
	CRTAddOnNewFields *&	operator[]	(int nIndex)		{ return ElementAt(nIndex); }

	// prima controlla l'esistenza
	void			Add(CRTAddOnNewFields*);         
	BOOL			Exist(CRTAddOnNewFields*)  const;
	LPCTSTR			GetSignature(CRuntimeClass*) const;
	CTBNamespace	GetNsOwnerLibrary(CRuntimeClass*) const;
};

///////////////////////////////////////////////////////////////////////////////
typedef CMap<int, int, int , int> CIntMap;

///////////////////////////////////////////////////////////////////////////////

// viene utilizzata per le TABLE, VIEW e PROCEDURE
///////////////////////////////////////////////////////////////////////////////
//								SqlTableInfo
class TB_EXPORT SqlTableInfo : public CObject
{
	friend class SqlCatalog;
	friend class SqlTable;
	friend class SqlUsedTables;	
	friend class SqlCatalogEntry;	
	
private:
	//use bool and not BOOL for threading reasons
	volatile bool			m_bCheckedSqlNewFieldRT;
	CRTAddOnNewFieldsArray*	m_pSqlNewFieldRT;

	DECLARE_LOCKABLE	(Array,		m_arVirtualColumns);
	DECLARE_LOCKABLE	(Array,		m_arPhisycalColumns);
	DECLARE_LOCKABLE	(CIntMap,	m_arPhisycalColumnMapping);

	CString					m_strTableName;

	SqlUniqueColumns*		m_pSqlUniqueColumns; 
	SqlProcedureParameters*	m_pProcParameters; //utilizzato solo per le Stored Procedure
	
	BOOL					m_bValid;	
	SqlCatalogEntry*		m_pSqlCatalogEntry;

	const SqlRecord*		m_pMasterRec;
	BOOL					m_bSortedWithRecord;

	const SqlColumnInfo*	m_pCreatedColumn;
	const SqlColumnInfo*	m_pModifiedColumn;
	const SqlColumnInfo*	m_pCreatedIDColumn;
	const SqlColumnInfo*	m_pModifiedIDColumn;
	const SqlColumnInfo*	m_pGuidColumn;

public:
	bool					ExistCreatedColumn		() const { return m_pCreatedColumn != NULL; }
	bool					ExistModifiedColumn		() const { return m_pModifiedColumn != NULL; }
	bool					ExistCreatedIDColumn	() const { return m_pCreatedIDColumn != NULL; }
	bool					ExistModifiedIDColumn	() const { return m_pModifiedIDColumn != NULL; }
	bool					ExistGuidColumn			() const { return m_pGuidColumn != NULL; }


	const SqlColumnInfo*	GetCreatedColumnInfo	() const { return m_pCreatedColumn; }
	const SqlColumnInfo*	GetModifiedColumnInfo	() const { return m_pModifiedColumn; }
	const SqlColumnInfo*	GetCreatedIDColumnInfo	() const { return m_pCreatedIDColumn; }
	const SqlColumnInfo*	GetModifiedIDColumnInfo	() const { return m_pModifiedIDColumn; }
	const SqlColumnInfo*	GetGuidColumnInfo		() const { return m_pGuidColumn; }

protected:
	void	LoadColumnsInfo (SqlConnection *pConnection);

	CString GetName(const CString& strColumnName) const; //serve per togliere l'eventuale qualifica
	CString GetName(const CString& strColumnName, int nDotIndex) const;

	BOOL	SortColumns(SqlRecord*);

public:
	const SqlRecord*		GetMasterRec() const { return m_pMasterRec; }
	BOOL					IsSortedWithRecord	() const {	return m_bSortedWithRecord; }
	const SqlColumnInfo*	GetPhisycalColumn			(int pos) const;
	const SqlColumnInfo*	GetPhisycalSortedColumn		(int pos) const;

	const SqlColumnInfo* AddVirtualColumnInfo (
								int			nPos,
						const	CString&	strColumnName,
						const	DataObj&	aDataObj,
								int			nLen,
								BOOL		bIsCollateCultureSensitive
								);
private:
	const SqlColumnInfo* AddDynamicColumnInfo	(
											const CString&	strColumnName,
											const DataObj&	aDataObj,
											int				nLen,
											BOOL			bIsCollateCultureSensitive,
											BOOL			bVirtual,
											BOOL			bSpecial
										);
	void RemoveDynamicColumnInfo	(const CString&	strColumnName);

public:
	SqlTableInfo (const CString& strTableName, int nType, SqlCatalogEntry* pCatalogEntry, SqlConnection *pConnection);
	virtual ~SqlTableInfo();
	
public:
	const Array* GetPhysicalColumns() const { return &m_arPhisycalColumns; }
	const CRTAddOnNewFieldsArray* GetCRTAddOnNewFields();
	
	int GetSizePhisycalColumns () const;
	int GetPreAllocSize () const;

	// accessing elements
	SqlColumnInfo*	GetAt		(int nIndex) const;
	SqlColumnInfo*&	ElementAt	(int nIndex);

	// overloaded operator helpers
	SqlColumnInfo*	operator[]	(int nIndex) const	{ return GetAt(nIndex); }
	SqlColumnInfo*&	operator[]	(int nIndex)		{ return ElementAt(nIndex); }

	DataType				GetColumnDataType	(const CString& strColumnName) const;

	const SqlColumnInfo*	GetColumnInfo		(const CString& strColumnName, int nPos, BOOL bVirtual);
	const SqlColumnInfo*	GetColumnInfo		(const CString& strColumnName) const;

	int						GetColumnInfoPos	(const CString& strColumnName, int nPos, BOOL bVirtual, const SqlColumnInfo** = NULL);
	int						GetColumnInfoPos	(const CString& strColumnName, const SqlColumnInfo** = NULL) const;

	BOOL					ExistColumn			(const CString& strColumnName) const;
	
	const CString&			GetTableName		() 	const	{ return m_strTableName; }
	const CString			GetTableTitle		() 	const	{ return AfxLoadDatabaseString(m_strTableName, m_strTableName); }
	BOOL					IsValid				()	const	{ return m_bValid; }
	const SqlCatalogEntry*	GetSqlCatalogEntry	()	const	{ return m_pSqlCatalogEntry; }

	//Impr. 3936
	BOOL					IsMasterTable() const;

	//carica le info relative alle PK e alle SpecialColumns solo la prima volta che viene chiamata
	const SqlUniqueColumns*	GetSqlUniqueColumns() const  { return m_pSqlUniqueColumns; }

	//per il controllo dell'esistenza degli indici. 
	// Serve x l'Order By nel caso di utilizzo di cursori dinamici e SqlServer
	void SetExistIndex	(const CString& szColumnName);
	BOOL ExistIndex		(const CString& szColumnName) const;	
	BOOL ExistIndex		(const SqlColumnInfo*) const;	
	
	void LoadIndexInfo			(SqlConnection *pConnection);
	void LoadPrimaryKeyInfo		(SqlConnection *pConnection);

	//per la parte relativa alle StoredProcedure
	void LoadProcParametersInfo	(SqlConnection *pConnection);
	int	 GetParamInfoPos(const CString& strParamName, int nPos = -1) const;
	SqlProcedureParamInfo* GetParamAt(int nIdx) const { return (m_pProcParameters) ? m_pProcParameters->GetAt(nIdx) : NULL; }

	void SetNamespace	(const CTBNamespace& aNamespace);

	const CInfoOSL* GetOSLTableInfo ();

// diagnostics
#ifdef _DEBUG
public:	
	void Dump(CDumpContext& dc) const {	ASSERT_VALID(this); AFX_DUMP0(dc, " SqlTableInfo\n"); }
	void AssertValid() const{ __super::AssertValid(); }
#endif //_DEBUG
};

////////////////////////////////////////////////////////////////////////////////
//				class SqlTableInfoArray definition
////////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT SqlTableInfoArray : public CArray<const SqlTableInfo*>
{
	DECLARE_DYNAMIC(SqlTableInfoArray) 

public:
	CStringArray			m_arstrAliasTableName;

	int Add(const SqlTableInfo* pTI, const CString& sAlias = CString())
	{
		m_arstrAliasTableName.Add(sAlias);
		return __super::Add(pTI);
	}

	SqlTableInfoArray () {}

	SqlTableInfoArray(const SqlTableInfo* pTI, const CString& sAlias = CString()) { Add(pTI, sAlias); }

	SqlTableInfoArray (const SqlTableInfoArray&); //duplica solo l'array e non gli elementi

	SqlTableInfoArray& operator=	(const SqlTableInfoArray&); //duplica solo l'array e non gli elementi

	const SqlColumnInfo* GetColumnInfo(const CString& strName) const ;

	virtual void	Unparse(Unparser& oFile);
	virtual CString	Unparse();

	virtual void RemoveAll();

	int Find(const SqlTableInfo* pTI);
	int Find(const CString& sTableName);

	const SqlTableInfo* GetTableInfo(const CString& sTableName);
};                                        

////////////////////////////////////////////////////////////////////////////////
//				class SqlTableJoinInfo/SqlTableJoinInfoArray definition
////////////////////////////////////////////////////////////////////////////////
//
//============================================================================
class TB_EXPORT DataFieldLink : public CObject
{
	DECLARE_DYNAMIC(DataFieldLink)
public:
	CString		m_strPhysicalName;
	CString		m_strPublicName;
	BOOL		m_bHidden;
	DataType	m_type;

public:
	DataFieldLink(LPCTSTR pszPhysicName, LPCTSTR pszPublicName, BOOL bHiddenLink = FALSE/*, BOOL bNativeExpr = FALSE*/, DataType type = NULL);
	DataFieldLink(const DataFieldLink&);
};

//============================================================================
class TB_EXPORT DataFieldLinkArray : public Array
{
	DECLARE_DYNAMIC(DataFieldLinkArray)
public:
	DataFieldLinkArray() {}
	DataFieldLinkArray(const DataFieldLinkArray& arSrc) { Copy(arSrc); }

	// accessing elements
	DataFieldLink*		GetAt(int nIndex) const { return (DataFieldLink*)Array::GetAt(nIndex); }
	DataFieldLink*&		ElementAt(int nIndex) { return (DataFieldLink*&)Array::ElementAt(nIndex); }

	// overloaded operator helpers
	DataFieldLink*		operator[]	(int nIndex) const { return GetAt(nIndex); }
	DataFieldLink*&		operator[]	(int nIndex) { return ElementAt(nIndex); }

	DataFieldLink*		Find(const CString& sName) const;

	void Copy(const DataFieldLinkArray&);

	void SetQualified(const CString& sTableName);
};

//============================================================================
class TB_EXPORT DataFieldLinkArrays : public Array
{
	DECLARE_DYNAMIC(DataFieldLinkArrays)
public:
	// accessing elements
	DataFieldLinkArray*		GetAt(int nIndex) const { return (DataFieldLinkArray*)Array::GetAt(nIndex); }
	DataFieldLinkArray*&	ElementAt(int nIndex) { return (DataFieldLinkArray*&)Array::ElementAt(nIndex); }

	// overloaded operator helpers
	DataFieldLinkArray*		operator[]	(int nIndex) const { return GetAt(nIndex); }
	DataFieldLinkArray*&	operator[]	(int nIndex) { return ElementAt(nIndex); }

	DataFieldLink*		Find(const CString& sName) const;

	void Copy (const DataFieldLinkArrays&);

	int GetFieldLinkCount() const;
};

//============================================================================

class WClause;

class TB_EXPORT SqlTableJoinInfoArray : public SqlTableInfoArray
{
	DECLARE_DYNAMIC(SqlTableJoinInfoArray)
public:
	enum EJoinType { INNER, LEFT_OUTER, RIGHT_OUTER, FULL_OUTER, CROSS };

	CArray<EJoinType>			m_arJoinType;
	CArray<WClause*>			m_arJoinOn;
	BOOL						m_OwnJoinExpressions;
	DataFieldLinkArrays			m_arFieldLinks;

	int Add(SqlConnection*		pConnection, SymTable*			pSymTable, const SqlTableInfo* pTI, const CString& sAlias = CString(), EJoinType eJoinType = EJoinType::CROSS, WClause* pJoinOn = NULL);
	int Add(SqlConnection*		pConnection, SymTable*			pSymTable, const CString& sTableName);

	SqlTableJoinInfoArray() : m_OwnJoinExpressions (TRUE) {}
	SqlTableJoinInfoArray(const SqlTableJoinInfoArray& ar);
	SqlTableJoinInfoArray(const SqlTableInfoArray&);	
	SqlTableJoinInfoArray(const CString& sTableName, SqlConnection*		pConnection, SymTable*			pSymTable);

	virtual ~SqlTableJoinInfoArray ();

	SqlTableJoinInfoArray& operator= (const SqlTableJoinInfoArray&); //duplica solo l'array e non gli elementi

	virtual void Unparse(Unparser& oFile);
	virtual CString Unparse() { return __super::Unparse(); }

	void RemoveAll();

	void QualifiedLinks(int idx);
	void QualifiedLinks();
};


///////////////////////////////////////////////////////////////////////////////
//								SqlCatalogEntry
///////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT SqlCatalogEntry : public CObject, public CTBLockable, public IOSLObjectManager 
{
friend class SqlCatalog;
friend class SqlTableInfo;

	DECLARE_DYNAMIC(SqlCatalogEntry)
private:
	CAuditingMngObj*		m_pAuditingMng; //x la gestione dell'auditing sulla singola tabella
	CRuntimeClass*			m_pSqlRecordClass;
	bool					m_bTraceChecked;
	bool					m_bVirtual;		//indica che la tabella non è realmente nel database
	CMapStringToString		m_ColumnsCollations;

	CTableRowSecurityMngObj* m_pTableRowSecurityMng; //indica che la tabella è sotto protezione (ROWSECURITY LAYER)
	const CDbObjectDescription*	m_pDBObjectDescription;

public:
	CString 				m_strTableName;
	CString					m_strSignature;
	BOOL					m_bExist;
	int						m_nType;

	SqlTableInfo*			m_pTableInfo;

// constructor	
public:
	SqlCatalogEntry
		(
			const CString&	strTableName,
			int				m_nType = TABLE_TYPE, // puo' trattarsi di una "TABLE",  di una "VIEW" oppure di una "PROCEDURE"
			LPCTSTR			pszSignature = _T(""),
			CRuntimeClass*	pSqlRecordClass = NULL,
			BOOL			bExist = TRUE,
			BOOL			bVirtual = FALSE
		);
	virtual ~SqlCatalogEntry();

protected:
	void SetNamespace(const CTBNamespace& strNamespace);

	BOOL SortTableInfoColumns();

private:
	void SetSqlRecordClass (CRuntimeClass* prc) { ASSERT(!m_pSqlRecordClass); m_pSqlRecordClass = prc; }
	void SetTraced();
	void SetDbDescription(CDbObjectDescription* pNewDescription);

	void RemoveDynamicColumnInfo(const CString& strColumnName);

public:
	bool IsVirtual() const { return m_bVirtual; }
	virtual LPCSTR  GetObjectName() const { return "SqlCatalogEntry"; }
	const CTBNamespace& GetNamespace() const { return const_cast<SqlCatalogEntry*>(this)->GetInfoOSL()->m_Namespace; }
	CRTAddOnNewFieldsArray* CreateSqlNewFieldRT() const;
	SqlRecord* CreateRecord () const;
	CRuntimeClass* GetSqlRecordClass () const { return m_pSqlRecordClass; }
	BOOL HasBeenRegistered() const;

	void	SetProtected(CTableRowSecurityMngObj*); //TBRowSecurityLayer	
	BOOL	IsProtected() const { return m_pTableRowSecurityMng != NULL; }
	void	AddRowSecurityFilters(SqlTable*, SqlTableItem*) const;	
	void	ValorizeRowSecurityParameters(SqlTable*) const;
	CString GetSelectGrantString(SqlTable*) const;	
	BOOL	CanCurrentWorkerUsesRecord(SqlRecord*, SqlTable*) const;
	void	HideProtectedFields(SqlRecord*) const;

	//Impr. 3936
	BOOL	IsMasterTable() const { return (m_pDBObjectDescription) ? m_pDBObjectDescription->IsMasterTable() : FALSE; }

// gestione auditing
public:
	BOOL				IsTraced() const { return m_pAuditingMng != NULL && m_pAuditingMng->IsValid(); }
	void				TraceOperation(int eType, SqlTable* pTable	) const;
	void				BindTracedColumns(SqlTable* pTable) const;
	
	//chiamate da XTech per l'esportazione
	void PrepareQuery(SqlTable* pTable, DataDate& aFrom, DataDate& aTo, int eType) const;
	BOOL PrepareDeletedQuery(SqlTable* pTable, DBTMaster* pDBTMaster, DataDate& aFrom, DataDate& aTo) const;

	//per creare al volo un report di woorm sui dati di tracciatura
	// con gli eventuali valori di fixedkey dipendenti dal documento
	//viene restituito il namespace del report creato
	CTBNamespace* CreateAuditingReport(CTBNamespace* pNamespace, CXMLFixedKeyArray* pFixedArray, BOOL bAllUsers, const CString& strUser) const;

// diagnostics
#ifdef _DEBUG
public:	
	void Dump(CDumpContext& dc) const {	ASSERT_VALID(this); AFX_DUMP0(dc, " SqlCatalogEntry\n"); }
	void AssertValid() const{ CObject::AssertValid(); }
#endif //_DEBUG
};
  
///////////////////////////////////////////////////////////////////////////////
//								SqlForeignKeys
///////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT SqlForeignKeysReader : public CStringArray
{
	CString	m_sLastFromTableName;
	CString	m_sLastToTableName;

public:
	void	LoadForeignKeys	(CString sFromTableName, CString sToTableName, SqlSession* pSqlSession, BOOL bLoadAllToTables = FALSE);

	void	GetForeignKey	(const int& nIdx, CString& sFromTable, CString& sFromCol, CString& sToTable, CString& sToCol);
	CString GetForeignKeyOf	(const CString& sTable, const CString& sCol, const CString& sOnTable, SqlSession* pSqlSession);
};

// restituisce le colonne che individuano univocamente una riga di un set
// o come chiave primaria oppure come specialcolumns
///////////////////////////////////////////////////////////////////////////////
//								SqlUniqueColumns
///////////////////////////////////////////////////////////////////////////////
class TB_EXPORT SqlUniqueColumns : public CObject
{
protected:
	CStringArray	m_aSqlPrimaryKeys;		// chiavi primarie della tabella
	CStringArray	m_aSqlSpecialColumns;	// colonne che identificano in modo univoco un record


public:
	int		GetUpperBound() const	{ return m_aSqlPrimaryKeys.GetSize() ? m_aSqlPrimaryKeys.GetUpperBound(): m_aSqlSpecialColumns.GetUpperBound(); }
	int		GetSize() const			{ return m_aSqlPrimaryKeys.GetSize() ? m_aSqlPrimaryKeys.GetSize()		: m_aSqlSpecialColumns.GetSize(); }
	CString	GetAt(int nIndex) const	{ return m_aSqlPrimaryKeys.GetSize() ? m_aSqlPrimaryKeys.GetAt(nIndex)	: m_aSqlSpecialColumns.GetAt(nIndex); }

	int		AddSpecialColumn(const CString& strColumnName) { return m_aSqlSpecialColumns.Add(strColumnName); }
	void	LoadPrimaryKey	(const CString& strTableName, SqlSession* pSqlSession);

// diagnostics
#ifdef _DEBUG
public:	
	void Dump(CDumpContext& dc) const {	ASSERT_VALID(this); AFX_DUMP0(dc, " SqlUniqueColumns\n"); }
	void AssertValid() const{ CObject::AssertValid(); }
#endif //_DEBUG
};  

  
///////////////////////////////////////////////////////////////////////////////
//								SqlCatalog
///////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT SqlCatalog : public CMapStringToOb, public CTBLockable                             
{
	friend class SqlConnection;
	friend class COleDbManager;
	friend class DataTableRule;
protected:
	BOOL				m_bValid;
	BOOL				m_bLoaded;
	CString				m_sDatabaseCollation;

public:
	SqlCatalog();
	virtual ~SqlCatalog();

private:
	BOOL	RegisterCatalogEntry
				(
					SqlConnection*		pConnection,		
					LPCTSTR				pszSignature, 
					const CTBNamespace&	aNamespace,
					CRuntimeClass*		pSqlRecordClass, 
					int					nType,
					CDbObjectDescription* pDbDescription = NULL
				);

public:	
	void Load(SqlConnection *pConnection);

	CRuntimeClass*		GetSqlRecordClass			(const CString& strTableName) const;
	CRuntimeClass*		GetSqlRecordClass			(const CTBNamespace& ns) const;
	
	CString				GetTableSignature			(const CString& strTableName) const ;
	const CTBNamespace*	GetNamespace				(const CString& strTableName) const ;

	BOOL				ExistTable					(const CString& strTableName) const;

	BOOL	IsValid 		()	const						{ return m_bValid; }

	void	GetRegisteredTableNames(CStringArray& arTableNames) const;
	virtual LPCSTR  GetObjectName() const { return "SqlCatalog"; }
	const	CString& GetDatabaseCollation () const;
	CString GetColumnCollationName (const CString& sTableName, const CString& sColumnName) const;
	BOOL	IsCollationCultureSensitive (SqlColumnInfo* pColumnInfo, SqlConnection *pConnection) const;
	BOOL	AddDynamicCatalogEntry(SqlConnection* pConnection, const CTBNamespace& aNamespace, const int& nType, bool isVirtual, CDbObjectDescription* pDbDescription);
	void	RemoveDynamicCatalogEntry(const CString& strTableName);

	SqlCatalogEntry*		GetEntry				(const CString& strTableName) const;
	SqlCatalogEntry*		GetEntry				(const CTBNamespace&) const;
	SqlTableInfo*			GetTableInfo			(const CString& strTableName, SqlConnection* pConnection);
	BOOL					DatabaseEmpty			()	const;

protected:
	void 					SetEntry				(const CString& strTableName, SqlCatalogEntry*);
	
private:
	void					RefreshTraces			();
	BOOL					LoadColumnCollations	(SqlConnection *pConnection);
	CString					ReadDatabaseCollationName(SqlConnection *pConnection);
	CString					ReadServerCollationName (SqlConnection *pConnection);

public:
	const	SqlColumnInfo*	AddDynamicColumnInfo		(
															const CString&	strTableName,
															const CString&	strColumnName,
															const	DataObj&	aDataObj,
															int				nLen,
															BOOL			bIsCollateCultureSensitive,
															BOOL			bVirtual,
															BOOL			bSpecial
														);
	void RemoveDynamicColumnInfo						(
															const CString& strTableName, 
															const CString&	strColumnName
														);
	const SqlColumnInfo*	AddVirtualColumnInfo
												(	
													const	CString&	strTableName,
													const	CString&	strColumnName,
													const	DataObj&	aDataObj,
															int			nLen,
															BOOL bIsCollateCultureSensitive
												);
	
	BOOL ReloadTableInfo(const CString&	strTableName, SqlConnection* sqlConnection);

	void SortTableInfoColumns();

// diagnostics
#ifdef _DEBUG
public:	
	void Dump(CDumpContext& dc) const {	ASSERT_VALID(this); AFX_DUMP0(dc, " SqlCatalog\n"); }
	void AssertValid() const{ CMapStringToOb::AssertValid(); }
#endif //_DEBUG
	DECLARE_DYNCREATE(SqlCatalog);
};

DECLARE_CONST_SMART_LOCK_PTR(SqlCatalog);

///////////////////////////////////////////////////////////////////////////////
class TB_EXPORT CGroupTableObArray : public CObArray, public IOSLObjectManager 
{
	DECLARE_DYNAMIC(CGroupTableObArray)

public:
	CString		m_strSignature;

	virtual ~CGroupTableObArray() {}
};

///////////////////////////////////////////////////////////////////////////////
//								SqlTypeInfo
///////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT SqlTypeInfoItem : public CObject
{   
public:
	SqlTypeInfoItem();
	SqlTypeInfoItem(const SqlTypeInfoItem& src) { *this = src; }

public:
	void operator= (const SqlTypeInfoItem&);
	
public:
	CString	m_strTypeName;
	SWORD	m_nSqlDataType;
	long	m_lPrecision;
	CString	m_strPrefix;
	CString	m_strSuffix;
	CString	m_strCreateParams;
	BOOL	m_bNullable;
	BOOL	m_bCaseSensitive;
	int		m_nSearchable;
	BOOL	m_bUnsignedAttribute;
	int		m_nMoney;
	BOOL	m_bAutoIncrement;
	CString	m_strLocalTypeName;
	
// diagnostics
#ifdef _DEBUG
public:	
	void Dump(CDumpContext& dc) const {	ASSERT_VALID(this); AFX_DUMP0(dc, " SqlTypeInfoItem\n"); }
	void AssertValid() const{ CObject::AssertValid(); }
#endif //_DEBUG
};

///////////////////////////////////////////////////////////////////////////////
//								SqlTypeInfo
///////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT SqlTypeInfo : public Array
{
	DECLARE_DYNAMIC(SqlTypeInfo)
	
public:
	SqlTypeInfo(SqlSession* pSqlSession);

public:
	// accessing elements
	SqlTypeInfoItem*		GetAt		(int nIndex) const	{ return (SqlTypeInfoItem*) Array::GetAt(nIndex); }
	SqlTypeInfoItem*&	ElementAt	(int nIndex)		{ return (SqlTypeInfoItem*&) Array::ElementAt(nIndex); }
	
	// overloaded operator helpers
	SqlTypeInfoItem*		operator[]	(int nIndex) const	{ return GetAt(nIndex); }
	SqlTypeInfoItem*&	operator[]	(int nIndex)		{ return ElementAt(nIndex); }
	
// diagnostics
#ifdef _DEBUG
public:	
	void Dump(CDumpContext& dc) const {	ASSERT_VALID(this); AFX_DUMP0(dc, " SqlTypeInfo\n"); }
	void AssertValid() const{ Array::AssertValid(); }
#endif //_DEBUG
};

///////////////////////////////////////////////////////////////////////////////
//							CHelperSqlCatalog
///////////////////////////////////////////////////////////////////////////////
class TB_EXPORT CHelperSqlCatalog : public CObject
{
	DECLARE_DYNAMIC(CHelperSqlCatalog)

public:
	//-------------------------------------------------------------------------
	class CModuleTables: public CObject
	{
		public:
			AddOnModule* m_pModule;
			CString m_sTitle;
			Array m_arModTables;

			CModuleTables(AddOnModule* pModule, CString sTitle);
	};

	//-------------------------------------------------------------------------
	class CTableForeignTables;

	class CTableColumns : public CObject
	{
	public:
		const SqlCatalogEntry* m_pCatalogEntry;
		Array m_arSortedColumns;

		
		CTableColumns(const SqlCatalogEntry* pCatalogEntry);
		~CTableColumns()
		{
			m_arSortedColumns.RemoveAll();
			m_arForeignTables.RemoveAll();
			m_mapForeignTables.RemoveAll();
		}

		Array& GetForeignKeys();// { if (!m_bForeignTablesLoaded) { LoadForeignKeys(); m_bForeignTablesLoaded = TRUE; } return m_arForeignTables; }
	protected:
		BOOL m_bForeignTablesLoaded = FALSE;
		Array m_arForeignTables;			//array di CTableForeignTables
		CMapStringToOb m_mapForeignTables;	//mappa per array m_arForeignTables

		BOOL LoadForeignKeys();//carica le foreignkey della tabella di partenza
	};

	//-------------------------------------------------------------------------
	class CTableForeignTables : public CObject
	{
	public:
		CString	m_sForeignTableName;	// nome tabella foreign
		Array	m_arForeignKeys;		//array di CTableForeignTablesKeys

		CTableForeignTables(const CString& sForeignTableName)
		{
			m_sForeignTableName	= sForeignTableName;
			m_arForeignKeys.SetOwns(TRUE);
		}
		~CTableForeignTables()
		{
			m_arForeignKeys.RemoveAll();
		}

	};

	//-------------------------------------------------------------------------
	class CTableForeignTablesKeys : public CObject
	{
	public:
		CTableForeignTables*	m_pParent = NULL;
		CString m_sColumnName;			// nome colonna chiave nella tabella di partenza
		CString m_sForeignColumnName;	// nome colonna nella tabella foreign

		CTableForeignTablesKeys(CTableForeignTables* pParent, const CString& sColumnName, const CString& sForeignColumnName)
		{
			m_sColumnName			= sColumnName;
			m_sForeignColumnName	= sForeignColumnName;
			m_pParent				= pParent;
		}
	};

public:
	Array	m_arModules;
	Array	m_arAllTables;
	Array	m_arExternalTables;

public:
	CHelperSqlCatalog();

	void Load();

	CTableColumns* FindEntryByName(const SqlCatalogEntry* pCatalogEntry);
	CTableColumns* FindEntryByName(const CString& sTableName);
	CModuleTables* FindModuleByTitle(AddOnModule* pMod);

	

	Array& GetForeignKeys(CTableColumns*);
	Array* GetForeignKeys(const CString& sTableName);
protected:
	void FillTable(const SqlCatalogEntry* pCatalogEntry, CModuleTables* pModT);

	
};

//-----------------------------------------------------------------------------
TB_EXPORT SqlCatalog* AfxGetSqlCatalog(SqlConnection* pConnection);

// ----------------------------------------------------------------------------
TB_EXPORT int CompareSqlColumnInfo(CObject* po1, CObject* po2);
TB_EXPORT int CompareSqlCatalogEntry(CObject* po1, CObject* po2);

//=============================================================================
#include "endh.dex"

	
