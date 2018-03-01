
#pragma once

#include <TbGeneric\Array.h>
#include <TbGeneric\dataobj.h>
#include <TbGeneric\ParametersSections.h>
#include <TbGenlib\BaseDoc.h>
#include <TbGenlib\TbCommandInterface.h>
#include <TbGeneric\SettingsTable.h>
#include <TbGeneric\ISqlRecord.h>

#include <TbNameSolver\TbNamespaces.h>

#include "sqlcatalog.h"
#include "lentbl.h"

//includere alla fine degli include del .H
#include "beginh.dex"

// vengono gestite le classi per la gestione della mappatura delle tabelle di db
// SqlRecordItem = mappa il singolo campo
// SqlRecord = classe base da cui derivare la classe per la gestione di una singola tabella
// SqlAddOnFieldsColumn e SqlNewFieldsArray = per la gestione dei campi aggiunti da terze parti
// SqlRecordView = per la gestione delle view
// RecordArray = array di sqlrecord
//============================================================================

class SqlTable;
class SqlLock;
class SqlLockTable;
class SqlNewFieldsArray;
class RecordArray;

/////////////////////////////////////////////////////////////////////////////
// Macro per costruire automaticamente la query di creazione di una TABLE
// o bindare le colonne ai dati. ATTENZIONE sono stati cablati i valori di
// lunghezza e numero di decimali per gli interi e i reali non usando delle
// defines per non avere la espansione multipla di macro. 
/////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------
#define GUID_COL_NAME		_T("TBGuid")
#define	CREATED_COL_NAME	_T("TBCreated")
#define MODIFIED_COL_NAME	_T("TBModified")
//for Company Resource Management. In this fields are stored the ResourceID has created or modified a single record	
#define CREATED_ID_COL_NAME	 _T("TBCreatedID") 
#define MODIFIED_ID_COL_NAME _T("TBModifiedID")

#define RETURN_VALUE _T("@RETURN_VALUE")

//-----------------------------------------------------------------------------
#define BEGIN_BIND_DATA()	{int nPos = GetSize();
#define END_BIND_DATA()		EndBindData(nPos);}

#define	BEGIN_BIND_ADDON_FIELDS(a)	{int nPos = a; 
#define	END_BIND_ADDON_FIELDS()	return nPos;}

//per la gestione delle view
#define BEGIN_BIND_VIEW_DATA()	{int nPos = GetSize();
#define END_BIND_VIEW_DATA()	}

//per la gestione di una stored procedure
//per i campi di result
#define BEGIN_RESULT_DATA()	{int nPos = GetSize();
#define END_RESULT_DATA()	}

//per i parametri di una stored procedure
#define BEGIN_BIND_PARAM_DATA()	{int nParam = GetSize();\
	if (m_nType != PROC_TYPE)\
	{ ASSERT(FALSE); return; }
#define END_BIND_PARAM_DATA()	}

//-----------------------------------------------------------------------------
#define LOCAL_STR(a,b,c) LOCAL_COLLATE_CULTURE_SENSITIVE_STR(a, b, c, TRUE);


//-----------------------------------------------------------------------------
#define LOCAL_KEY_STR(a,b,c) \
	LOCAL_COLLATE_CULTURE_SENSITIVE_STR(a, b, c, TRUE);\
	AddPrimaryKeyColumn(a); \
	const_cast<SqlColumnInfo*>(GetColumnInfo(a))->m_bSpecial = true;

//-----------------------------------------------------------------------------
#define LOCAL_COLLATE_CULTURE_SENSITIVE_STR(a,b,c,d)\
	ASSERT(b.GetDataType() == DATA_STR_TYPE);\
	BindLocalDataObj(nPos++, a, b, c, d);

//-----------------------------------------------------------------------------
#define LOCAL_DATA(a,b)\
	ASSERT(b.GetDataType() != DATA_STR_TYPE);\
	BindLocalDataObj(nPos++, a, b);

//-----------------------------------------------------------------------------
#define LOCAL_KEY(a,b)\
	ASSERT(b.GetDataType() != DATA_STR_TYPE);\
	BindLocalDataObj(nPos++, a, b);\
	AddPrimaryKeyColumn(a); \
	const_cast<SqlColumnInfo*>(GetColumnInfo(a))->m_bSpecial = true;	


// macro utilizzata per fare il binding di tutti i tipi di campo
//-----------------------------------------------------------------------------
#define BIND_DATA(a,b)\
	BindDataObj(nPos++, a, b);

//utilizzata per identificare un ipotetico campo chiave di una view
//-----------------------------------------------------------------------------
#define BIND_KEY(a,b)\
	BindDataObj(nPos++, a, b);\
    AddPrimaryKeyColumn(a);\
	const_cast<SqlColumnInfo*>(GetColumnInfo(a))->m_bSpecial = true;

// deve essere un long
//-----------------------------------------------------------------------------
#define BIND_AUTOINCREMENT(a,b)\
	ASSERT(b.GetDataType() == DATA_LNG_TYPE);\
	BindAutoIncrementDataObj(nPos++, a, b);

// macro utilizzata per fare il binding di un parametro di una stored procedure
//-----------------------------------------------------------------------------
#define BIND_PARAM(a,b)\
	BindParamDataObj(nParam++, a, b);

#define BIND_RESULT_VALUE(b)	\
	BindParamDataObj(nParam++, RETURN_VALUE, b);


// Bind del campo f_TBGuid che mappa il campo fisico della tabella TBGuid
// il valore viene assegnato in automatico in fase di inserimento del record
//-----------------------------------------------------------------------------
#define BIND_TB_GUID()\
	BindDataObj(nPos++, GUID_COL_NAME, f_TBGuid);\
	SetWithGUID();

//Improv. #5071
// Bind del campo f_TBClient che mappa il campo fisico della tabella TBClient (cliente/contribuente)
//-----------------------------------------------------------------------------
#define BIND_DATA_CONTEXT(a,b,c)\
	BindContextDataObj(nPos++, a, b, c);

// MACRO per effettuare il bind dei campi aggiunti
//-----------------------------------------------------------------------------
#define LOCAL_ADDON_STR(a,b,c) LOCAL_ADDON_COLLATE_CULTURE_SENSITIVE_STR(a,b,c,TRUE)

//-----------------------------------------------------------------------------
#define LOCAL_ADDON_COLLATE_CULTURE_SENSITIVE_STR(a,b,c,d)\
	ASSERT(b.GetDataType() == DATA_STR_TYPE);\
		m_pSqlRecParent->BindAddOnLocalDataObj(nPos++, a, b, c, d);

//-----------------------------------------------------------------------------
#define LOCAL_ADDON_DATA(a,b)\
	ASSERT(b.GetDataType() != DATA_STR_TYPE);\
		m_pSqlRecParent->BindAddOnLocalDataObj(nPos++, a, b);

//-----------------------------------------------------------------------------
#define BIND_ADDON_DATA(a,b)\
		m_arPhysicalColumnsNames.Add(CString(a));\
	    m_pSqlRecParent->BindAddOnDataObj(nPos++, a, b);

//-----------------------------------------------------------------------------
#define BIND_ADDON_AUTOINCREMENT(a,b)\
		ASSERT(b.GetDataType() == DATA_LNG_TYPE);\
		m_arPhysicalColumnsNames.Add(CString(a));\
		m_pSqlRecParent->BindAddnOnAutoIncrementDataObj(nPos++, a, b);

//-----------------------------------------------------------------------------
#define BIND_ADDON_DATA_CONTEXT(a,b,c)\
		m_arPhysicalColumnsNames.Add(CString(a));\
		m_pSqlRecParent->BindContextDataObj(nPos++, a, b, c);

//==============================================================================
#define BEGIN_REGISTER_TABLES()\
	if (pSqlConnection->GetAlias().CompareNoCase(_T("PRIMARY")))\
		return TRUE;\
	if (!pSqlConnection->IsValid())\
		return FALSE;\
	BOOL	bOk = TRUE;\
	AfxGetApp()->BeginWaitCursor();\

//registro le sole tabelle del database correlato avente alias = a
//==============================================================================
#define BEGIN_REGISTER_EXTERNAL_TABLES(a)\
	if (pSqlConnection->GetAlias().CompareNoCase(CString(a)))\
		return TRUE;\
	if (!pSqlConnection->IsValid())\
		return FALSE;\
	BOOL	bOk = TRUE;\
	AfxGetApp()->BeginWaitCursor();\
//-----------------------------------------------------------------------------
#define REGISTER_TABLE(a)\
	aTblNamespace.SetObjectName(CTBNamespace::TABLE, a::GetStaticName(), TRUE);\
	bOk = pSqlConnection->RegisterCatalogEntry(pszSignature, aTblNamespace, RUNTIME_CLASS(a), TABLE_TYPE) && bOk;

//-----------------------------------------------------------------------------
#define REGISTER_VIEW(a)\
	aTblNamespace.SetObjectName(CTBNamespace::TABLE, a::GetStaticName(), TRUE);\
	bOk = pSqlConnection->RegisterCatalogEntry(pszSignature, aTblNamespace, RUNTIME_CLASS(a), VIEW_TYPE) && bOk;

//-----------------------------------------------------------------------------
#define REGISTER_PROCEDURE(a)\
	aTblNamespace.SetObjectName(CTBNamespace::TABLE, a::GetStaticName(), TRUE);\
	pSqlConnection->RegisterCatalogEntry(pszSignature, aTblNamespace, RUNTIME_CLASS(a), PROC_TYPE);

//-----------------------------------------------------------------------------
#define REGISTER_VIRTUAL_TABLE(a)\
	aTblNamespace.SetObjectName(CTBNamespace::TABLE, a::GetStaticName(), TRUE);\
	pSqlConnection->RegisterCatalogEntry(pszSignature, aTblNamespace, RUNTIME_CLASS(a), VIRTUAL_TYPE);

//-----------------------------------------------------------------------------
#define END_REGISTER_TABLES()	AfxGetApp()->EndWaitCursor();\
	return bOk;

//-----------------------------------------------------------------------------
 #define FN(r,f)     (LPCTSTR)r.GetColumnName(&(r.f))

//-----------------------------------------------------------------------------
/////////////////////////////////////////////////////////////////////////////
//								SqlBindItem
//////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
class TB_EXPORT SqlBindItem : public CObject
{
	DECLARE_DYNAMIC (SqlBindItem)

protected:
	DataObj*		m_pDataObj;				// Usato come DataValue in BindParam                                    
	CString			m_strColumnName;		// mi serve memorizzarlo per la Rebind
	BOOL			m_bDynamicallyBound;	// it is an item created by dynamic sqlrecord
	BOOL			m_bIsAddOn;				// it is an item created using SqlAddOnFields

public:
	CString			m_strContextElementName; //è una colonna che nel caso viene utilizzato per effettuare a basso livello filtraggi legati al contesto (vedi contribuente, anno e attività per i commercialisti)

public:
	// costruttori
	SqlBindItem		(DataObj* pDataObj, const CString& strColumnName);
	SqlBindItem		(const SqlBindItem& bi);
	virtual ~SqlBindItem	();

protected:
	void Assign(const SqlBindItem& item);
	
public:
	// functions
	void	Init()	{ ASSERT(m_pDataObj); m_pDataObj->Clear(TRUE); }

	void			SetDataObj		(DataObj* pDataObj)	{ m_pDataObj = pDataObj; }
	DataObj*		GetDataObj		()	const			{ return m_pDataObj; }
	
	//Internal use only !!!!!
	void			ReplaceDataObj	(DataObj* pDataObj, BOOL deletePrev = FALSE)	{ if (m_pDataObj && deletePrev) delete m_pDataObj;  m_pDataObj = pDataObj; }
	//----

	virtual const CString&		GetColumnName	() const			{ return m_strColumnName; }

	BOOL	IsEqual		(const SqlBindItem&)	const;

	BOOL	IsDynamicallyBound	()	const 	{ return m_bDynamicallyBound; }
	BOOL	IsDirty				()	const 	{ ASSERT(m_pDataObj); return m_pDataObj->IsDirty(); }
	BOOL	IsModified			()	const 	{ ASSERT(m_pDataObj); return m_pDataObj->IsModified(); }
	BOOL	IsEmpty				()	const	{ ASSERT(m_pDataObj); return m_pDataObj->IsEmpty(); }
	
	void 	SetDirty 	(BOOL bDirty = TRUE)	{ ASSERT(m_pDataObj); m_pDataObj->SetDirty(bDirty); }
	void 	SetModified	(BOOL bModified= TRUE)	{ ASSERT(m_pDataObj); m_pDataObj->SetModified(bModified); }

public:
	// operators
	BOOL	operator ==	(const SqlBindItem& item)	const { return IsEqual (item); }
	BOOL	operator !=	(const SqlBindItem& item)	const { return !IsEqual (item); }
	void	operator =	(const SqlBindItem&);
	
// diagnostics
#ifdef _DEBUG
public:	
	void Dump(CDumpContext& dc) const {	ASSERT_VALID(this); AFX_DUMP0(dc, " SqlRecordItem "); CObject::Dump(dc);}
	void AssertValid() const{ CObject::AssertValid(); }
#endif //_DEBUG
};
			
//////////////////////////////////////////////////////////////////////////////
//								SqlRecordItem
//////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
class TB_EXPORT SqlRecordItem : public SqlBindItem
{
	friend class SqlRecord;
	friend class SqlRecordLocals;
	friend class SqlColumnArray;
	friend class SqlLockMng;
	friend class SqlTable;
	friend class SqlAddOnFieldsColumn;
	friend class SqlTableInfo;

	DECLARE_DYNAMIC (SqlRecordItem)

protected:
	const SqlColumnInfo*	m_pColumnInfo;

public:
	BOOL					m_bOwnColumnInfo;
	int						m_lLength;

public:
	// costruttori
	SqlRecordItem (DataObj* pDataObj, const CString& strColumnName, const SqlColumnInfo* pColumnInfo = NULL, BOOL bDynamicallyBound = FALSE);
	SqlRecordItem (const SqlRecordItem& ri);
	virtual ~SqlRecordItem ();

public:
	BOOL	IsEqual		(const SqlRecordItem&)	const;
	BOOL	IsSpecial	()	const	{ return m_pColumnInfo ? m_pColumnInfo->m_bSpecial : FALSE; }
	BOOL	IsMandatory ()	const;
	BOOL	IsVirtual	()	const { return m_pColumnInfo && m_pColumnInfo->m_bVirtual; }

	const SqlColumnInfo*	GetColumnInfo() const { return m_pColumnInfo; }
	void SetColumnInfo(const SqlColumnInfo*);

	void Parse		(CXMLNode* pNode, BOOL bWithAttributes = TRUE);
	void UnParse	(CXMLNode* pNode, BOOL bWithAttributes = TRUE, BOOL bSoapType = TRUE);
		
	virtual const CString&		GetBindingName			() const { return m_strColumnName; } //torna il nome della colonna bindato al record e non il nome della colonna
	virtual const CString&		GetColumnName			() const { return m_pColumnInfo ? m_pColumnInfo->GetColumnName() : m_strColumnName; }
	virtual CString				GetQualifiedColumnName	() const { return m_pColumnInfo ? m_pColumnInfo->GetQualifiedColumnName() : m_strColumnName; }

	void SetColumnName	(const CString& strColumnName) 				{ if (m_pColumnInfo) (const_cast<SqlColumnInfo*>(m_pColumnInfo))->m_strColumnName = strColumnName; m_strColumnName = strColumnName; }

	long			GetColumnLength () const { return m_pColumnInfo ? m_pColumnInfo->GetColumnLength() : m_lLength; }
	CString			GetColumnTitle (const CString& sTable) const;
	DataType		GetDataType () const { return m_pDataObj ? m_pDataObj->GetDataType() : DataType::Null; }

protected:
		// collate culture info
	void UpdateCollateCultureStatus	();

public:
	// operators
	BOOL	operator ==	(const SqlRecordItem& item)	const { return IsEqual (item); }
	BOOL	operator !=	(const SqlRecordItem& item)	const { return !IsEqual (item); }
	void	operator =	(const SqlRecordItem&);
	
// diagnostics
#ifdef _DEBUG
public:	
	void Dump(CDumpContext& dc) const {	ASSERT_VALID(this); AFX_DUMP0(dc, " SqlRecordItem "); SqlBindItem::Dump(dc);}
	void AssertValid() const{ CObject::AssertValid(); }
#endif //_DEBUG
};

//////////////////////////////////////////////////////////////////////////////
//								SqlProcParamItem
//////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
class TB_EXPORT SqlProcParamItem : public SqlBindItem
{
	friend class SqlRecordProcedure;
	friend class SqlColumnArray;
	friend class SqlTable;
	
	DECLARE_DYNAMIC (SqlProcParamItem)


protected:
	SqlProcedureParamInfo*	m_pParameterInfo;

public:
	// costruttori
	SqlProcParamItem (DataObj* pDataObj, const CString& strColumnName, SqlProcedureParamInfo* pParamInfo = NULL);
	~SqlProcParamItem();

public:
	BOOL	IsEqual		(const SqlProcParamItem&)	const;

	SqlProcedureParamInfo*	GetParameterInfo() const { return m_pParameterInfo; }
	void SetParameterInfo(SqlProcedureParamInfo*);

public:
	// operators
	BOOL	operator ==	(const SqlProcParamItem& item)	const { return IsEqual (item); }
	BOOL	operator !=	(const SqlProcParamItem& item)	const { return !IsEqual (item); }
	void	operator =	(const SqlProcParamItem&);
	
// diagnostics
#ifdef _DEBUG
public:	
	void Dump(CDumpContext& dc) const {	ASSERT_VALID(this); AFX_DUMP0(dc, " SqlProcParamItem "); SqlBindItem::Dump(dc);}
	void AssertValid() const{ CObject::AssertValid(); }
#endif //_DEBUG
};

// le proprietà global/lobal servono per avere la visibilità dell'informazione
// contenuta nel record dall'esterno o meno (in fase di esportazione dei dati)
// il record non può essere in contemporanea locale e globale. Questo controllo
// viene fatto all'interno del metodo SetLocalGlobal() di SqlRecord
//#define LOCAL_PROPERTY			0x0001 // record locale
//#define GLOBAL_PROPERTY			0x0002 // record globale

#define TB_BIND_GUID_USED			0x00000001 // se è stata effettuata la BIND_GUID

//////////////////////////////////////////////////////////////////////////////
//								SqlRecord
//////////////////////////////////////////////////////////////////////////////
//	SqlRecord contains an Array of SqlRecordItem. Remember that this array
//	can have same empty elements, this particular condition is necessary
//	in Woorm application where are not used all fields of a particular
//	DataTable.
//
TB_EXPORT DataStr GetExtraFiltering(DataArray /*[string]*/ aTablesNames, DataStr aWC);

//=============================================================================
class TB_EXPORT SqlRecord : public ISqlRecord, public IDisposingSource
{
	friend class SqlTable;
	friend class DBTSlaveBuffered;
	friend class DBTObject;
	friend class AuditingManager;
	friend class SqlCatalogEntry;

	DECLARE_DYNCREATE(SqlRecord)
public:
	DataGuid			f_TBGuid;
	//these fields are always created in all the tables of the applications
	DataDate			f_TBModified;
	DataDate			f_TBCreated;	
	DataLng				f_TBCreatedID;
	DataLng				f_TBModifiedID;
	
protected:
	CString				m_strTableName;
	CString				m_strQualifier;

	SqlConnection*		m_pSqlConnection;
	SqlTableInfo*		m_pTableInfo;
	
	SqlNewFieldsArray*	m_pSqlNewFieldsTable;
	RecordArray*		m_arExtensions;
	
	CUIntArray			m_aSqlPrimaryKeyIndexes;//indice, nel sql record, delle  chiavi primarie della tabella

	WORD				m_wRecStatus;	//bitvector di RecordStatus

	WORD				m_wTBBindMap;	// mi dice quale bind di basso livello (BIND_TAG, BIND_PROPERTY, BIND_GUID) è stata fatta
	
	short				m_nType;		//TABLE_TYPE, VIEW_TYPE, PROC_TYPE, VIRTUAL_TYPE

	bool				m_bAutoIncrement; //esiste una colonna autoincrementale
	bool				m_bPrepareBanner;
	bool				m_bBindingDynamically;
	bool				m_bEndBind;
	bool				m_bInsertedByUI;
	bool				m_bCreatedAsEmpty;
	CCallbackHandler	m_Handler;
	short				m_nLocalsCount;
	short				m_nDummyCount;//campi aggiunti al volo per la decodifica degli hotlink
	
	CObArray	m_arContextBagElements; //gestione contextbag

public:	
	enum RecordStatus { VALID = 0x0001, MODIFIED = 0x0002, STORABLE = 0x0004, NEVER_STORABLE = 0x0008 };

public:                            
	SqlRecord();			// default constructor
	SqlRecord(LPCTSTR szTableName, SqlConnection* pConn = NULL, short nType = TABLE_TYPE, bool bbCreatedAsEmpty = false);
	SqlRecord(const SqlRecord& record); // copy constructor empty
	virtual ~SqlRecord();

public:                                  
	virtual					SqlRecord*	GetSqlRecord	() { return this; }
	//virtual					ISqlRecord*	GetISqlRecord	() { return (ISqlRecord*) this; }
	virtual					SqlRecord*	Create			() const;
	virtual	/*TBWebMethod*/ SqlRecord*	Clone			();
	virtual /*TBWebMethod*/	void		CopyRecord		(SqlRecord*);
	virtual					ISqlRecord*	IClone			() { return Clone ()->GetSqlRecord(); }
	virtual					void		Assign			(ISqlRecord* pIRec) { CopyRecord (pIRec->GetSqlRecord()); }
	virtual					void		Dispose			() { delete this; }

	// operators
	virtual SqlRecord&	operator =	(const SqlRecord& record);
	virtual BOOL	operator ==	(const SqlRecord& record) const	{ return IsEqual(record); }
	virtual BOOL	operator != (const SqlRecord& record) const	{ return !IsEqual(record); }
	
	//aggiunge una callback da chiamare alla distruzione del documento
	void AddDisposingHandler (CObject* pListener, ON_DISPOSING_METHOD pHandler) { m_Handler.AddDisposingHandler(pListener, pHandler); }
	void RemoveDisposingHandlers (CObject* pListener) { m_Handler.RemoveDisposingHandlers(pListener); }
	
	BOOL CompareFieldBy(int nFieldIndex, ECompareType cmp, const CStringArray& arFilterValues, DataObj* pPreAllocated = NULL, BOOL bCompareNoCase = TRUE, CParsedCtrl* = NULL);
	BOOL CompareFieldBy(int nFieldIndex, ECompareType cmp, DataObj* pFilterData = NULL, BOOL bCompareNoCase = TRUE);

public:
	// Usato internamente da DBTSlaveBuffered per gestire l'algoritmo 
	// ottimizzato di salvataggio (vedi dbt.cpp).
	BOOL	IsModified	()	const;
	//loop sui field
	BOOL	IsDataModified() const;

	SqlConnection*	GetConnection() const  {return m_pSqlConnection; }
	int				GetSizeEx () const;	//extensions

	void	SetBindingDynamically	(bool bValue = true);
	virtual void	BindDynamicDeclarations (int& nStartPos);
	void	RefreshDynamicFields ();

	void	SetInsertedByUI	(bool b = true) { m_bInsertedByUI = b; }
	bool	IsInsertedByUI() const  { return m_bInsertedByUI; }

	void    SetColumnAsNativeExpression(DataObj* pDataObj);

private:
	void	Initialize		();
	void	SetStatus	(BOOL bValid, RecordStatus aStatusFlag);

	//imposta solo il flag globale del record
	void	SetModified	(BOOL bModified = TRUE);
	//loop sui fields
	void	SetDataModified(BOOL bModified = TRUE);

	// viene utilizzata per il cambio di connessione per agganciare le informazioni delle columninfo presenti nel 
	// tableinfo presente nel catalog della nuova connessione
	void	RebindingColumns		();
	BOOL	BindRecordItem			(SqlRecordItem*, int nPos, BOOL bAutoIncrement = FALSE);
	void	BindMandatoryFields		(int& nStartPos);
	void	BindAddOnFields			(int& nStartPos); 
	
protected:
	void	EndBindData					(int& nStartPos);
	void	PrepareMessageBanner		();
	
	// collate culture info
	void	UpdateCollateCultureStatus	();

	virtual void OnCreating (SqlRecord* pRec) const;
	virtual void OnCreated	(SqlRecord* pRec) const;

public:                                  
	// overridables
	virtual BOOL	IsEmpty			() 					const;
	virtual BOOL	IsEqual			(const SqlRecord&)	const;
	virtual BOOL	IIsEqual		(const ISqlRecord& irec)	const { return IsEqual(*(const_cast<ISqlRecord*>(&irec)->GetSqlRecord())); }

	virtual BOOL	IsPhisicalEqual	(const SqlRecord&)	const; // controllo l'uguaglianza solo per i campi fisici qundi non per i local. Serve per la memorizzazione 
			BOOL	IsRegistered	()					const { return m_pTableInfo && m_pTableInfo->GetSqlCatalogEntry()->HasBeenRegistered(); }
	virtual void	Init			();

	virtual void	SetConnection(SqlConnection* = NULL); // aggancia il sqlrecord ad una determinata connection

	virtual		  long			GetColumnLength	(const DataObj*) const;

	virtual void EnableExtraFiltering	(BOOL) {}					// per implementare filtraggi custom
	virtual void OnExtraFiltering		(CString& /*strFilter*/) {}	// per implementare filtraggi custom

	// gestione del dirty flag per ottimizzare il trasferimento da/per il database
	BOOL			IsDirty		() const;
	void 			SetDirty 	(BOOL bDirty = TRUE);
	void 			SetFlags	(BOOL bDirty = TRUE, BOOL bModified = TRUE);

	BOOL			IsPresent		(const DataObj* pDataObj, int& nIdx) const;
	BOOL			IsPKEmpty() 	const;
public:                                  
	virtual		int			GetIndexFromColumnName				(const CString&) const;	
				int			GetIndexFromDataObj					(const DataObj* pDataObj) const;

				int			Lookup		(const CString&)	const;
				int			Lookup		(const DataObj*)	const;

				virtual	DataObj*	GetDataObjFromColumnName	(const CString&);	
/*TBWebMethod*/	virtual DataObj*	GetDataObjFromName			(DataStr);	

	virtual		DataObj* 	GetDataObjAt					(int nIndex) const;	

				DataObj*	GetContextDataObj				(const CString& strContextElementName);

	SqlRecordItem* 			GetAt				(int nIndex) const;	
	SqlRecordItem*			GetItemByColumnName	(const CString&	strColumnName);
	SqlRecordItem*			GetItemByDataObj	(const DataObj* pDataObj);//it has to exists

	const SqlColumnInfo*	GetColumnInfo		(int idx)  const;	
	const SqlColumnInfo*	GetColumnInfo		(const DataObj* pDataObj) const;
	const SqlColumnInfo*	GetColumnInfo		(const CString& str) const;

	const CString&			GetColumnName				(int idx) const	;
	virtual const CString&	GetColumnName				(const DataObj* pDataObj)	const	{ return GetColumnName(Lookup(pDataObj)); }

	CString					GetQualifiedColumnName		(const SqlColumnInfo*, BOOL bFindExtensions = TRUE, BOOL bMandatory = FALSE) const;
	CString					GetQualifiedColumnName		(const DataObj* pDataObj, BOOL bMandatory = FALSE) const;
	CString					GetQualifiedColumnName		(int idx, BOOL bMandatory = FALSE) const;

	CString					GetColumnTitle				(int idx)	 const	{ return AfxLoadDatabaseString(m_strTableName, GetColumnName(idx)); }
	
	// functions called into BindRecord
	// restituisce il SqlRecordItem creato
	SqlRecordItem* BindGenericDataObj		(int nPos, const CString& strColumnName, DataObj& pDataObj, BOOL bAutoIncrement = FALSE, const SqlColumnInfo* = NULL);
	SqlRecordItem* BindDataObj				(int nPos, const CString& strColumnName, DataObj& pDataObj);
	SqlRecordItem* BindLocalDataObj			(int nPos, const CString& strColumnName, DataObj& aDataObj, int nLen = 0, BOOL bIsCollateCultureSensitive = TRUE);
	SqlRecordItem* BindAutoIncrementDataObj	(int nPos, const CString& strColumnName, DataObj& aDataObj);
	SqlRecordItem* BindAddOnDataObj			(int nPos, const CString& strColumnName, DataObj& aDataObj);
	SqlRecordItem* BindAddOnLocalDataObj	(int nPos, const CString& strColumnName, DataObj& aDataObj, int nLen = 0, BOOL bIsCollateCultureSensitive = TRUE);
	SqlRecordItem* BindAddnOnAutoIncrementDataObj(int nPos, const CString& strColumnName, DataObj& aDataObj);
	SqlRecordItem* BindContextDataObj		(int nPos, const CString& strColumnName, DataObj& aDataObj, const CString& strContextElementName);
	SqlRecordItem* BindDynamicDataObj		(const CString& strColumnName, DataObj& aDataObj, int nLen);
	const SqlTableInfo* GetTableInfo	();
	void GetTableInfo	(SqlTableInfoArray&, BOOL bClear = TRUE, BOOL bGetExtensions = FALSE);

	virtual const CString&	GetTableName		() const { return m_strTableName; }

	CString	GetTableTitle		() const { return AfxLoadDatabaseString(m_strTableName, m_strTableName); }
	
	const CString&	GetQualifier		() const { return m_strQualifier; }
	BOOL			IsQualified			() const { return !m_strQualifier.IsEmpty(); }

	void			SetTableName		(const CString& sName) { m_strTableName = sName; }

	// per la gestione del GUID
	void			SetWithGUID()			{ m_wTBBindMap |= TB_BIND_GUID_USED; }
	BOOL			HasGUID()		const	{ return (m_wTBBindMap & TB_BIND_GUID_USED) == TB_BIND_GUID_USED; }

	BOOL						IsSpecial				(const DataObj* pDataObj) const;
	BOOL						IsSpecial				(int nIdx)	const;
	SqlRecordItem*				GetSpecialColumn		(int nSegment) const;
	int							GetNumberSpecialColumns () const;
	const CUIntArray&			GetPrimaryKeyIndexes	() const { return m_aSqlPrimaryKeyIndexes; }
	void						GetCopyPrimaryKeyIndexes(CUIntArray* pPrimaryKeyIndexes) const;
	void						AddPrimaryKeyColumn		(const CString& aColumnName);

	// restituisce i dati di chiave primaria in forma leggibile dall'utente
	CString			GetPrimaryKeyDescription() const;
	virtual CString GetRecordDescription() const;
	//restituisce i valori dei campi passati come argomento in forma leggibile dall'utente
	CString			GetFieldsValueDescription(CStringArray* pFields);
	virtual CString	ToString() const ;

	void			GetKeyInXMLFormat(CString& strKey, BOOL bEnumAsString = FALSE) const;
	void			GetKeyStream (DataObjArray& arPkSegments, BOOL bClone = TRUE) const;

	CString			GetPrimaryKeyNameValue() const; //restituisce i valori non formattati(vedi enum) come stringa con il seguente formato  fieldName:FieldValue;fieldName1:fieldValue1
	void			SetPrimaryKeyNameValue(const CString& keyDescri); //valorizza le chiavi primarie a partire da una stringa con formato fieldName:FieldValue;fieldName1:fieldValue1
	//simile alla precedente, ma se non rova la colonna la cerca nelle variabili del documento (usata da OFM-Commitment)
	void			SetColumnNameValue(const CString& keyDescri, CBaseDocument* = NULL); //valorizza le chiavi primarie a partire da una stringa con formato fieldName:FieldValue;fieldName1:fieldValue1
	
	// restituisce i campi che sono locckati
	void			GetLockedFields(DataObjArray& arLockedFields, BOOL bOnlySpecialFields =FALSE, BOOL bClearArray =TRUE ) const;
	
	//Dimensione massima dell'oggetto comprensiva della lunghezza di tutte le colonne DataStr
	int				GetAllocSize() const;
	int				GetRecordSize()	const; 
	
	// restituisce il puntatore all'oggetto istanza della RuntimeClass della classe derivata da
	// SqlAddOnFieldsColumn passata come parametro 
	SqlAddOnFieldsColumn*	GetAddOnFields(const CRuntimeClass*) const;
	BOOL					HasAddOnNewFields() const { return (m_pSqlNewFieldsTable != NULL); }
	void					ModifyAddOnFieldsValue(SqlRecord*);

	const SqlNewFieldsArray*	GetSqlNewFieldsArray() const { return m_pSqlNewFieldsTable; }

	void	Clear		(BOOL bValid);
	BOOL	IsVirtual	();

	BOOL	IsVirtual	(int nIdx)	const { return GetColumnInfo(nIdx)->m_bVirtual; }

	BOOL	IsAutoIncrement(int nIdx)					const { return GetColumnInfo(nIdx)->m_bAutoIncrement;}
	BOOL	IsAutoIncrement(const DataObj* pDataObj)	const { return GetColumnInfo(pDataObj)->m_bAutoIncrement;}
	
	// Indica che si conosce la struttura sul database (SqlTableInfo caricata correttamente)
	BOOL	IsValid		()	const;
	void	SetValid	(BOOL bValid = TRUE);

	// Gestisce la presentazione di default in Radar ed in WrmRadar
	void	ClearVisible();
	BOOL	IsVisible	(int nIdx)	const									{ return GetColumnInfo(nIdx)->m_bVisible; }
	void	SetVisible	(int nIdx,					BOOL bVisible = TRUE);
	void	SetVisible	(const DataObj* pDataObj,	BOOL bVisible = TRUE)	{ SetVisible(Lookup(pDataObj), bVisible); }

	// Indica che la riga contiene dati che il programmatore vuole che siano salvati.
	// Usato principalmente in DBTSlaveBuffered per gestire le righe vuote
	BOOL	IsStorable	()	const;
	void	SetStorable	(BOOL bStorable = TRUE);

	// Indica che la riga contiene dati che sono utili solo in memoria
	// Usato principalmente in DBTTree per gestire delle righe di raggruppamento
	BOOL	IsNeverStorable	()	const;
	void	SetNeverStorable (BOOL b = TRUE);

	// per dare l'opportunità al programmatore di decidere se qualificare o meno i campi della tabella
	// in fase di costruzione di query (soprattutto JOIN)
	// se la stringa è vuota utilizzo il nome della tabella altrimenti la stringa che rappresenta 
	// l'alias
	void	SetQualifier(const CString& strQualifier = _T(""))  { m_strQualifier = (strQualifier.IsEmpty()) ? GetTableName() : strQualifier;}
	void	DisableQualifier()								{ m_strQualifier = _T(""); }

	BOOL	IsAView()	const { return m_nType == VIEW_TYPE; }
	
	const CInfoOSL* GetOSLTableInfo ();
	const CTBNamespace* GetNamespace () { const CInfoOSL* p = GetOSLTableInfo (); return p ? &(p->m_Namespace) : NULL; };
	void  SetNamespace (const CTBNamespace& aNamespace);

	const short&	GetType		() const { return m_nType; }

	CString	GetAutoIncrementColumn() const; //restituisce l'unica colonna autoincrement del record

	BOOL Parse		(CXMLNode* pNode, BOOL bWithAttributes = TRUE, BOOL bParseLocal = FALSE);
	BOOL UnParse	(CXMLNode* pNode, BOOL bWithAttributes = TRUE, BOOL bUnParseLocal = FALSE, BOOL bSoapType = TRUE);

	void SetDataOSLReadOnly		(BOOL bReadOnly = TRUE, int idx = -1);
	void SetReadOnly			(BOOL bReadOnly = TRUE); //mette in READONLY tutti i field del SqlRecord (fisici e virtuali)
	void SetDataHideAndReadOnly (BOOL b = TRUE, int idxSkipIt = -1);

	void CopyAttribute (SqlRecord*);

	//Record Extensions to support join by DBTSlaveBuffered/Bodyedit/RadarDoc
	void				RemoveExtension	(SqlRecord*);
	void				AddExtension	(SqlRecord*);
	const RecordArray*	GetExtensions	() const;
	const SqlRecord*	GetExtension	(CRuntimeClass* rtc) const;
	const SqlRecord*	LookupExtensionFromColumnIndex (int& nIndex) const ; //it is NOT a RecordArray's index, it modify its index param
	int					GetExtensionIndex	(int nIndex) const;
	
	void GetJson(CJsonSerializer& jsonSerializer, BOOL bOnlyWebBound); 
	void GetJsonPatch(CJsonSerializer& jsonSerializer, SqlRecord* pOld);
	bool SetJson(CJsonParser& jsonParser);
	void SetWebBound(CJsonParser& jsonParser);
	/*TBWebMethod*/DataStr	GetFieldValue(DataStr FieldName);
	/*TBWebMethod*/void		SetFieldValue(DataStr FieldName, DataStr Value);

	/*TBWebMethod*/DataBool	TbScriptIsStorable	(); 
	/*TBWebMethod*/void		TbScriptSetStorable	(DataBool bStorable); 
	
	void ValorizeDBObjectDescription(CDbObjectDescription*);

// diagnostics
#ifdef _DEBUG
public:	
	void Dump(CDumpContext& dc) const ;
#endif //_DEBUG
};


//=========================================================================
class TB_EXPORT DynamicSqlRecord : public SqlRecord
{
	DECLARE_DYNCREATE(DynamicSqlRecord) 

public:
	DynamicSqlRecord();
	DynamicSqlRecord (LPCTSTR szTableName, SqlConnection* pConn = NULL, short nType = TABLE_TYPE, bool bSkipDynamicDeclarations = false);

	virtual SqlRecord&	operator =	(const SqlRecord& record);

protected:
	virtual void OnCreated	(SqlRecord* pRec) const;

private:
	void BindDynamic();
};

//////////////////////////////////////////////////////////////////////////////

class TB_EXPORT UnregisteredSqlRecord : public SqlRecord
{
	DECLARE_DYNAMIC(UnregisteredSqlRecord) 
public:
	UnregisteredSqlRecord(LPCTSTR szTableName);

private:
	void	CreateDynamicDeclarations ();
	DataType GetFirstCompatibleDataType(SqlColumnInfo* pSqlColumnInfo);

public:
	virtual					SqlRecord*	Create() const;
};

//////////////////////////////////////////////////////////////////////////////
//								SqlRecordDynamic
//////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT SqlRecordDynamic : public SqlRecord
{
	DECLARE_DYNCREATE(SqlRecordDynamic) 

public:
	SqlRecordDynamic();

public:
	virtual void BindRecord	();	
	virtual void SetConnection(SqlConnection*);
	virtual SqlRecord* Create () const;

public:
	static LPCTSTR GetStaticName();
};


// classe utilizzata per la gestione dei campi aggiunti
//////////////////////////////////////////////////////////////////////////////
//								SqlAddOnFieldsColumn
//////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
class TB_EXPORT SqlAddOnFieldsColumn: public CObject
{
	friend class SqlRecord;
	
	DECLARE_DYNAMIC(SqlAddOnFieldsColumn)

protected:
	SqlRecord*		m_pSqlRecParent;
	CStringArray	m_arPhysicalColumnsNames;

public:    
	SqlAddOnFieldsColumn();
	virtual ~SqlAddOnFieldsColumn() {};

public:                                  
	// overridables
	virtual void	InitAddOnFields			() {}
	virtual int		BindAddOnFields			(int nStartPos =0) { return nStartPos;}
	virtual	void	ModifyAddOnFieldsValue	(SqlRecord*)	{}
	
	virtual	const CString&	GetTableName	() const		{ return m_pSqlRecParent->GetTableName();}

public:                                  
	void					AttachRecParent	(SqlRecord* pRecParent)	{ m_pSqlRecParent = pRecParent; }
	SqlRecord*				GetRecParent() const { return m_pSqlRecParent; }

	const CStringArray&		GetPhysicalColumnsNames	() const { return m_arPhysicalColumnsNames; }
	const BOOL				HasColumn				(const CString& sColName) const;

	BOOL					IsEqual	(const SqlAddOnFieldsColumn&)	const;

public:
	// operators
	virtual SqlAddOnFieldsColumn&	operator =	(const SqlAddOnFieldsColumn& addOnFields);
	virtual BOOL	operator ==	(const SqlAddOnFieldsColumn& addOnFields) const	{ return IsEqual(addOnFields); }
	virtual BOOL	operator != (const SqlAddOnFieldsColumn& addOnFields) const	{ return !IsEqual(addOnFields); }


// diagnostics
#ifdef _DEBUG
public:	
	void Dump(CDumpContext& dc) const;
#endif //_DEBUG
};

///////////////////////////////////////////////////////////////////////////////
//								SqlNewFieldsArray
///////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT SqlNewFieldsArray :public Array
{
	friend class SqlRecord;

public:
	SqlNewFieldsArray() 
		: 
		Array()
	{}
public:
	// accessing elements
	SqlAddOnFieldsColumn *		GetAt		(int nIndex) const	{ return (SqlAddOnFieldsColumn *) Array::GetAt(nIndex); }
	SqlAddOnFieldsColumn *&		ElementAt	(int nIndex)		{ return (SqlAddOnFieldsColumn *&) Array::ElementAt(nIndex); }
	
	// overloaded operator helpers
	SqlAddOnFieldsColumn *		operator[]	(int nIndex) const	{ return GetAt(nIndex); }
	SqlAddOnFieldsColumn *&		operator[]	(int nIndex)		{ return ElementAt(nIndex); }

};

// Record with all virtual fields. This record doesn't map any database table or view or storedprocedure
///////////////////////////////////////////////////////////////////////////////
//								SqlVirtualRecord
///////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT SqlVirtualRecord : public SqlRecord
{
	DECLARE_DYNAMIC(SqlVirtualRecord)

public:
	// default constructor
	SqlVirtualRecord(LPCTSTR szTableName, SqlConnection* pConn = NULL);
	SqlVirtualRecord(const SqlVirtualRecord& record);
};


// per la gestione delle view
///////////////////////////////////////////////////////////////////////////////
//								SqlRecordView
///////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT SqlRecordView : public SqlRecord
{
	DECLARE_DYNAMIC(SqlRecordView)

public:
	// default constructor
	SqlRecordView(LPCTSTR szTableName, SqlConnection* pConn = NULL);
	SqlRecordView(const SqlRecordView& record);
};


// per la gestione delle Stored Procedure
///////////////////////////////////////////////////////////////////////////////
//								SqlProcedureParamList
///////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT SqlProcedureParamList :public Array
{
	friend class SqlRecordProcedure;

private:
	SqlProcParamItem*	GetParamItemFromParamInfo	(SqlProcedureParamInfo*) const;
	SqlProcParamItem*	GetParamItemFromDataObj		(const DataObj*) const;
	DataObj*			GetDataObjFromParamName		(const CString&) const;

public:
	// accessing elements
	SqlProcParamItem *		GetAt			(int nIndex) const	{ return (SqlProcParamItem *) Array::GetAt(nIndex); }
	SqlProcParamItem *&		ElementAt		(int nIndex)		{ return (SqlProcParamItem *&) Array::ElementAt(nIndex); }
	
	// overloaded operator helpers
	SqlProcParamItem *		operator[]	(int nIndex) const	{ return GetAt(nIndex); }
	SqlProcParamItem *&		operator[]	(int nIndex)		{ return ElementAt(nIndex); }
};


///////////////////////////////////////////////////////////////////////////////
//								SqlRecordProcedure
///////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT SqlRecordProcedure : public SqlRecord
{
	DECLARE_DYNCREATE(SqlRecordProcedure)

public:
	SqlProcedureParamList*	m_pProcedureParamList; // eventuale lista di parametri

public:
	// default constructor
	SqlRecordProcedure(LPCTSTR szProcedureName, SqlConnection* pConn = NULL);
	SqlRecordProcedure(const SqlRecordProcedure& record);
	~SqlRecordProcedure();

private:
	void RebindingParams		();
	void BindDynamicParameters	();

	BOOL BindParamItem			(SqlProcParamItem* pParamItem, int nPos);
public:
	void BindParamDataObj(int nPos, const CString& strParamName, DataObj& pDataObj);

	SqlProcParamItem*	GetParamItemFromName		(const CString& sName);	
	SqlProcParamItem*	GetParamItemFromParamInfo	(SqlProcedureParamInfo*) const;	
	const CString&		GetParamName(DataObj*) const;
	
	virtual const CString&				GetColumnName				(const DataObj*)	const;
	virtual		  DataObj*				GetDataObjFromColumnName	(const CString&);
   	virtual		  long					GetColumnLength				(const DataObj*)	const;

	virtual		  SqlRecordProcedure*	Create						();

	virtual void SetConnection(SqlConnection*);
	virtual BOOL IsEmpty() const;
	virtual BOOL IsEqual(const SqlRecordProcedure& record) const;
};

////////////////////////////////////////////////////////////////////////////////
//				class RecordArray
////////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT RecordArray : public Array
{
	friend class DBTSlaveBuffered;
	DECLARE_DYNAMIC(RecordArray) 

	BOOL m_bObservablesEnabled;

public:
	enum ESortFunctionStatus { sfsNone, sfsTodo, sfsOk };

protected:
	ESortFunctionStatus		m_sfs;
	CString					m_strOrderBy;
	CArray <int>			m_arSortedColumIndex;
	CArray <BOOL>			m_arAscSort;
	CArray <CParsedCtrl*>	m_arFormatCtrls;

	RecordArray*	m_pFreeList;

public:
	RecordArray();

public:
	// can be reimplemented in the derived classes to return the prototype of contained records
	virtual SqlRecord*	GetPrototype()	{ return NULL; }

	void EnableObservables(BOOL bValue = TRUE);

	BOOL SetOrderBy (LPCTSTR szOrderBy);
	BOOL SetOrderIndex();

	virtual int	Compare (CObject* po1, CObject* po2) const; 
	virtual BOOL LessThen (CObject* po1, CObject* po2) const {return (Compare(po1,po2) < 0);}

	// overloaded operator helpers
	SqlRecord* GetAt		(int nIndex) const	{ return (SqlRecord*) Array::GetAt(nIndex); }
	SqlRecord*&	ElementAt	(int nIndex)		{ return (SqlRecord*&) Array::ElementAt(nIndex); }

	SqlRecord* operator[]	(int nIndex) const	{ return GetAt(nIndex); }
	SqlRecord*& operator[]	(int nIndex)		{ return ElementAt(nIndex); }
	
	//gestione free list
	void		Attach		(RecordArray* pFreeList) { m_pFreeList = pFreeList; }
	void		RemoveAt	(int nIndex, int nCount = 1);
	void		RemoveAll	();
	SqlRecord*	RemoveLast	();	//ritorna l'ultimo elemento e lo elimina dall'array

	BOOL		Parse		(CRuntimeClass* pClassRec, const CString& filename, BOOL bWithAttributes = TRUE, BOOL bParseLocal = FALSE);
	BOOL		Parse		(CRuntimeClass* pClassRec, CXMLNode* pNode, BOOL bWithAttributes = TRUE, BOOL bParseLocal = FALSE);
	BOOL		UnParse		(CXMLNode* pParent, BOOL bWithAttributes = TRUE, BOOL bUnParseLocal = FALSE, BOOL bSoapType = TRUE);

	int			FindRecordIndex (const CString& sColumnName, const DataObj* aVal, int nStartPos = 0) const;
	SqlRecord*	FindRecord (const CString& sColumnName, DataObj* aVal, int nStartPos = 0);
	SqlRecord*	FindRecord (const CStringArray& arColumnName, const DataObjArray& arValues, int nStartPos = 0);
	//TODO int	FindRecordIndexBy (const CString& sColumnName, ECompareType cmp, const CStringArray& arCmpValues, int nStartPos = 0, BOOL bCompareNoCase = TRUE) const;
	
	SqlRecord*	FindRecordByTableName(const CString& sTableName);

	template <class T>	void CalcSum(int nIndex, DataObj& aSum) const;
						BOOL CalcSum(int nIndex, DataObj& aSum) const;
						BOOL CalcSum(const CString& sColumnName, DataObj& aSum) const;

	virtual DataObj*	GetMinElem			(const CString& /*sColumnName*/) const;
	virtual DataObj*	GetMaxElem			(const CString& /*sColumnName*/) const;
			DataObj*	GetMinElem			(int nIndex) const;
			DataObj*	GetMaxElem			(int nIndex) const;

};                                        
                 
////////////////////////////////////////////////////////////////////////////////
//				class SqlRecordLocals
////////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT SqlRecordLocals : public SqlRecord
{
	CString		m_strTableName;
	Array		m_ColumnInfos;
public:
	SqlRecordLocals(const CString& strTableName);
	virtual ~SqlRecordLocals(void);
	SqlRecordItem* AddLocalField(::DataType type, const CString& strName, int lLength = 0);
	SqlRecordItem* AddLocalField(::DataObj* pDataObj, const CString& strName, int lLength = 0);
	virtual SqlRecord*	Create	() const;
};

#include "endh.dex"

