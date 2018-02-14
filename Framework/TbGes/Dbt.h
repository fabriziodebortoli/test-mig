#pragma once

#include <TbGeneric\Array.h>
#include <TbNameSolver\JsonSerializer.h>

#include <TbParser\SymTable.h>
#include <TbOledb\sqlrec.h>
#include <TbOledb\sqltable.h>

#include "xmlgesinfo.h"

//includere alla fine degli include del .H
#include "beginh.dex"

//===========================================================================
class DBTObject;
	class DBTMaster;
	class DBTSlave;
		class DBTSlaveBuffered;

//==============================================================================
// usefuls class declaration
//==============================================================================
class Array;
class DBTArray;
class RecordArray;
class SqlException;
class SqlTable;
class SqlRecord;
class CAbstractFormDoc;
class DataObj;
class CBodyEdit;
class HotKeyLink;
class DBTSlave;
class CInfoOSL;
class CClientDocArray;
class CClientDoc;
class CXMLDocGenerator;
class CNumbererRequestParams;
class CNumbererBinder;
//==============================================================================
// usefuls defines
//==============================================================================
#define ALLOW_EMPTY_BODY	TRUE
#define CHECK_DUPLICATE_KEY	TRUE
#define DEFAULT_INDEX		NULL

// rappresenta la singola riga gestita dal DBTSlaveBuffered ed l'unica entitá gestita
// da un DBTSlave
////////////////////////////////////////////////////////////////////////////////
//				class DBTRow definition
////////////////////////////////////////////////////////////////////////////////
//
//===========================================================================
/*class TB_EXPORT DBTRow
{
	friend class DBTSlave;
	friend class DBTSlaveBuffered;

public:
	SqlRecord*			m_pRecord;
	DBTSlaveBuffered*	m_pDBTSlavable;

public:
	DBTRow(SqlRecord*); // il SqlRecord viene creato all'esterno cosí come
						// il DBTSlaveBuffered che viene agganciato quando necessario
	~DBTRow();

private:
	void	SetDBTSlavable(DBTSlaveBuffered*);
}; */



////////////////////////////////////////////////////////////////////////////////
//				class DBTArray definition
////////////////////////////////////////////////////////////////////////////////
//
// class array to avoid ripetitive cast
//===========================================================================
class TB_EXPORT DBTArray : public Array
{
public:
	// overloaded operator helpers
	DBTSlave* GetAt(int nIndex)	const		{ return (DBTSlave*) Array::GetAt(nIndex); }
	DBTSlave* GetBy(const CTBNamespace& aNs) const;
	DBTSlave*&	ElementAt	(int nIndex)	{ return (DBTSlave*&) Array::ElementAt(nIndex); }

	DBTSlave* operator[](int nIndex) const	{ return GetAt(nIndex); }
	DBTSlave*& operator[](int nIndex)		{ return ElementAt(nIndex); }

	void RemoveDBT (DBTSlave* pDBTSlave);
};



class DBTIterator;
typedef  DataObj* (__stdcall *DATAOBJ_ROW_FUNC) (SqlRecord*, int nRow);
//=============================================================================
class TB_EXPORT DBTObject : public CObject, public IDisposingSource, public IOSLObjectManager
{
	friend class CAbstractFormDoc;
	friend class CBodyEdit;
	friend class CWrmMaker;
	friend class CXMLDocGenerator;
	friend class CDocDescrMngPage;
	friend class DBTSlave;

	DECLARE_DYNAMIC(DBTObject)

protected:
	CAbstractFormDoc*	m_pDocument;

	SqlTable*	m_pTable;
	SqlRecord*	m_pRecord;		// owned and allocated
	SqlRecord*	m_pOldRecord;	// owned and allocated

	BOOL		m_bDBTErrorPending;  // evita di riassegnare l'errore per ogni riga sbagliata
	CString		m_strDBTError;
	BOOL		m_bNoDelete;
	BOOL		m_bDBTOnView; //è posto a TRUE se il SqlRecord su cui viene costruito il dbt è una view
	BOOL		m_bUpdated;
	BOOL		m_bRecordable;

	DATAOBJ_ROW_FUNC	m_pFnOnCheckPrimaryKey;

private:
	CXMLDBTInfo*		m_pXMLDBTInfo;
	CClientDoc*			m_pClientDocOwner;	//se è stato istanziato da un clientdoc
	CCallbackHandler	m_Handler;
	
protected:
	void Initialize(
			CRuntimeClass*		pClass,
			CAbstractFormDoc*	pDocument,
			const CString&		sName
		);
	DBTObject();
public:
	// constructors
	DBTObject
		(
			const CString&		sTableName,
			CAbstractFormDoc*	pDocument,
			const CString&		sName
		);
	DBTObject
		(
		SqlRecord*			pRecord,
		CAbstractFormDoc*	pDocument,
		const CString&		sName
		);
	DBTObject
		(
			CRuntimeClass*		pClass,
			CAbstractFormDoc*	pDocument,
			const CString&		sName
		);
	virtual ~DBTObject();
protected:

	// Display Error Message if enabled and always return FALSE
	BOOL	ErrorMessage(const CString &strMsg, SqlException* = NULL, UINT nIDP = 0);

	// Displays message if messaging is enabled, put the message
	// in document's m_pMessages otherwise
	void	ConditionalDisplayMessage(const CString &strMessage, UINT nID = MB_OK, UINT nIDHelp = (UINT)-1);

	// per gestire una diagnostica personalizzata di errore su chiave
	// primaria doppia (tornano sempre TRUE)
	BOOL	SetError	(const CString&);

	// common constructions
	void CreateAndInit (CAbstractFormDoc* pDocument, const CString& sName);

	DataObj* CheckPrimaryKey (SqlRecord* pRecord);

	void	SetFindable();

public:
	// base functions
	BOOL	Open		();
	void	Close		();

	void	SetDBTError (const CString&);

	CNumbererBinder*	GetNumbererBinder	();
	CNumbererRequest*	BindAutoincrement	(DataObj* pDataBinding, const CString& sEntity);
	CNumbererRequest*	BindAutonumber		(DataObj* pDataBinding, const CString& sEntity, DataDate* pDataDate = NULL);
	
	void SetOnCheckPrimaryKeyFunPtr(DATAOBJ_ROW_FUNC funPtr);
public:
	virtual BOOL	IsEmptyContent	(CString* = NULL)		{ return FALSE; }	//Implentare per controlli logici

	// DEVE essere implementata in DBTMaster, DBTSlave, DBTSlaveBuffered
	virtual	BOOL CheckTransaction	() = 0;
	virtual	BOOL GetCursorType		() = 0;

	//aggiunge una callback da chiamare alla distruzione del documento
			void		AddDisposingHandler		(CObject* pListener, ON_DISPOSING_METHOD pHandler) { m_Handler.AddDisposingHandler(pListener, pHandler); }
			void		RemoveDisposingHandlers (CObject* pListener) { m_Handler.RemoveDisposingHandlers(pListener); }
	virtual BOOL		IsActive();
	virtual	BOOL		PrepareSymbolTable	(SymTable*);
	virtual DBTObject*	GetDBTObject		(SqlRecord*);

	virtual void		GetJson(BOOL bWithChildren, CJsonSerializer& jsonSerializer, BOOL bOnlyWebBound) { ASSERT(FALSE); }

public:
	// useful getting routines
	SqlTable*			GetTable		()	const { return m_pTable; }
	/*TBWebMethod*/SqlTable*			GetDBTTable		();

	SqlRecord*			GetRecord		()	const { return m_pRecord; }
	/*TBWebMethod*/SqlRecord*			GetDBTRecord	();

	SqlRecord*			GetOldRecord	()	const { return m_pOldRecord; }
	CAbstractFormDoc*	GetDocument		()	const { return m_pDocument; }
	BOOL				GetUpdated		()  const { return m_bUpdated; }

	CString				GetName			() const;
	CTBNamespace&		GetNamespace	() { return GetInfoOSL()->m_Namespace; }
	BOOL				IsDBTOnView		() { return m_bDBTOnView; }
	void				SetAsView		() { m_bDBTOnView = TRUE; } //si comporta come se il dbt fosse su una view (es. non viene effettuato edit nè il conseguente salvataggio)

	void				SetRecordable	(BOOL bRecordable) { m_bRecordable = bRecordable; }

public:
	void SetNoDelete	(BOOL bNoDelete = TRUE) { m_bNoDelete = bNoDelete; }
	BOOL GetNoDelete	() { return m_bNoDelete; }

public:

	virtual BOOL	FindData			(BOOL bPrepareOld = TRUE);
	virtual BOOL	AddNew				(BOOL bInit = TRUE);
	virtual BOOL	Edit				();
	virtual BOOL	Delete				();
	virtual BOOL	Update				();
	virtual void	SetReadOnly			(BOOL bReadOnly = TRUE); //agisce su i campi di m_pSqlRecord. Virtualizzato in DBTSlaveBuffered
	virtual void	SetAlwaysReadOnly	(BOOL bReadOnly = TRUE); //agisce su i campi di m_pSqlRecord. Virtualizzato in DBTSlaveBuffered
	virtual void	Disconnect			();

protected:
	virtual void 	Init		();

protected:
	// DEVONO essere implementate nel DBTMaster ma sono opzionali nei DBTSlaves
	virtual void	OnEnableControlsForFind		() = 0;
	virtual void	OnDisableControlsForEdit	() = 0;

	virtual	void	OnDefineQuery				() = 0;
	virtual	void	OnPrepareQuery				() = 0;

	// POSSONO  essere reimplementate nella classe finale
	virtual	BOOL	OnOkTransaction				();

	virtual void	OnDisableControlsForAddNew	();
	virtual void	OnDisableControlsAlways		();// è indipendente dallo stato del documento

	virtual	BOOL	OnCheckPrimaryKey			();
	virtual	void	OnPreparePrimaryKey			();

	virtual void	OnGoInBrowseMode			() {}
public:
	virtual CString	GetTitle					() { return GetXMLDBTInfo() ? GetXMLDBTInfo()->GetTitle() : GetNamespace().GetObjectName(); }

	CClientDocArray*	GetClientDocs() const;

	// chiamato dopo l'istanziazione del dbt da parte del clientdoc.
	// Serve per XTech (vale solo per gli slave/slavebuffer)
	void InstantiateFromClientDoc(CClientDoc* pClientDoc);
	CClientDoc* GetClientDocOwner() { return m_pClientDocOwner; }

	CXMLDBTInfo*		GetXMLDBTInfo()	const { return m_pXMLDBTInfo; }
	void				SetXMLDBTInfo(CXMLDBTInfo* pXMLDBTInfo) {m_pXMLDBTInfo = pXMLDBTInfo;}

	//virtual BOOL		Parse		(CXMLNode* pNode, BOOL bWithAttributes = TRUE, BOOL bParseLocal = FALSE)			{ ASSERT(FALSE); return FALSE; }
	//virtual BOOL		UnParse		(CXMLNode* pParentNode, BOOL bWithAttributes = TRUE, BOOL bUnParseLocal = FALSE, BOOL bUnParseSlavable = FALSE)	{ ASSERT(FALSE); return FALSE; }

protected:
	virtual BOOL LoadXMLDBTInfo();

// Diagnostics
#ifdef _DEBUG
public:
	void Dump		(CDumpContext&)	const;
	void AssertValid()				const;
#endif // _DEBUG
};

//=============================================================================
class TB_EXPORT DBTMaster : public DBTObject
{
	friend class CAbstractFormDoc;
	friend class SqlBrowser;
	friend class DBTSlave;
	friend class CRadarDoc;
	friend class DBTSlaveBuffered;
	friend class DBTMasterIterator;

	DECLARE_DYNAMIC(DBTMaster)

protected:
	DBTArray*				m_pDBTSlaves;

public:
	// constructors
	DBTMaster
		(
			const CString&		sTableName,
			CAbstractFormDoc*	pDocument,
			const CString&		sName
		);
	DBTMaster
		(
			CRuntimeClass*		pClass,
			CAbstractFormDoc*	pDocument,
			const CString&		sName
		);
	DBTMaster
		(
		SqlRecord*			pRecord,
		CAbstractFormDoc*	pDocument,
		const CString&		sName
		);
	virtual ~DBTMaster();

private:
	void PrepareBrowser		(SqlTable*);
	void PrepareFindQuery	(SqlTable*);
	void GoInBrowseMode		();	

public:
	// NON DEVONO essere reimplementate nelle classi istanziabili
	virtual	BOOL CheckTransaction	();
	virtual	BOOL GetCursorType		() { return E_NO_CURSOR; } // x ottimizzare le performance visto che é un rowset di una sola riga nn ha senso usare cursori

public:
	// base functions
	void	Attach	(DBTSlave*);

	BOOL	Open		();
	void	Close		();

public:
	CString	GetSortString		() const;
	void	SetNoMasterDelete	(BOOL bNoDelete = TRUE) { SetNoDelete(bNoDelete); } // mantenuta per compatibilita' con le versioni precedenti
	DBTArray* GetDBTSlaves		() const  { return m_pDBTSlaves; }

	// per disabilitare il caricamento parziale degli slave buffered
	void	SetNoPreloadStep	();

		// useful getting routines
	BOOL IsValidRecordsSchema(CDiagnostic*);

public:
	virtual BOOL	Exist		();
	virtual BOOL	FindData	(BOOL bPrepareOld = TRUE);
	virtual BOOL	AddNew		(BOOL bInit = TRUE);
	virtual BOOL	Edit		();
	virtual BOOL	Delete		();
	virtual BOOL	Update		();
	virtual void 	Init		();
	virtual void	Disconnect();

	virtual void	OnEnableControlsForFind		();
	virtual void	OnDisableControlsForEdit	();

	virtual BOOL	LoadXMLDBTInfo();
	
	virtual	BOOL	PrepareSymbolTable         (SymTable*);

protected:
	// overridable
	virtual void 	InitSlaves	();

protected:
	void	EnableControlsForFind		();
	void	DisableControlsForAddNew	();
	void	DisableControlsForEdit		();
	void	DisableControlsAlways		();

protected:
	// DEVONO essere implementate nella classe finale
	virtual	void OnDefineQuery			();
	virtual	void OnPrepareQuery			();

public:
	// DEVE ESSERE Reimplementata se esiste una WHERE clause o un criterio di SORT
	virtual	void OnPrepareBrowser	(SqlTable*);
	virtual void OnPrepareFindQuery	(SqlTable*) {/* default do nothing*/}

	//DEVE essere reimplementata per l'import/export se si vuole aggiungere una
	//where clause (può essere uguale alla query implementata nella OnPrepareBrowser)
	// l'implementazione base fa solo una selectall.
	virtual	void OnPrepareForXImportExport(SqlTable*);

protected:
	//reprepare DBTSlave\DBTSlaveBuffere key segment
	virtual void ForcePreparePrimaryKey();

public:
	virtual void 	OnBeforeXMLExport	();
	virtual void 	OnAfterXMLExport	();
	virtual BOOL 	OnOkXMLExport		();

	virtual void 	OnBeforeXMLImport	();
	virtual void 	OnAfterXMLImport	();
	virtual BOOL 	OnOkXMLImport		();

	virtual DBTObject* GetDBTObject (SqlRecord*);

	virtual void	GetJson(BOOL bWithChildren, CJsonSerializer& jsonSerializer, BOOL bOnlyWebBound);
	virtual void	SetJson(BOOL bWithChildren, CJsonParser& jsonParser);

	//virtual BOOL		Parse		(CXMLNode* pNode, BOOL bWithAttributes = TRUE, BOOL bParseLocal = FALSE);
	virtual BOOL		UnParse		(CXMLNode* pParentNode, BOOL bWithAttributes = TRUE, BOOL bUnParseLocal = FALSE, BOOL bUnParseSlavable = FALSE);

// Diagnostics
#ifdef _DEBUG
public:
	void Dump		(CDumpContext&)	const;
	void AssertValid()				const;
#endif // _DEBUG
};

//=============================================================================
class TB_EXPORT DBTSlave : public DBTObject
{
	friend DBTMaster;
	friend class CXMLDataManager;
	friend class DBTSlaveBuffered;
	friend class CAbstractFormDoc;

	DECLARE_DYNCREATE(DBTSlave)

public:
	enum ReadType { NO_DELAYED, BROWSE_DELAYED, EDIT_DELAYED, ALL_DELAYED };

protected:
	BOOL		m_bAllowEmpty;
	BOOL		m_bEmpty;
	BOOL		m_bOnlyDelete;
	ReadType	m_ReadType;
	bool		m_bOnlyForRead;
// Il flag è stato reso pubblico per permettere, in fase di edit di poterlo forzare
// a TRUE (Gestione molti a molti).
// In questo caso infatti, con il documento in stato di edit è possibile fare la new di
// un nuovo DBT (inserimento di una nuova riga). Il flag, che abilita la modifyData viene
// automaticamente impostato dalla LocalFindData e dall' AddNew
public:
	BOOL		m_bLoaded;

protected:
	DBTObject*				m_pDBTMaster;
	SqlRecord*				m_pMasterRecord;
	SqlForeignKeysReader*	m_pFKReader; //serve per la find x permettere anche ai campi dei reali dbtslaves (quelli con una relazione di FK sul db) di diventare findable


	DBTSlave  ();
public:
	// constructors
	DBTSlave
		(
			CRuntimeClass*		pClass,
			CAbstractFormDoc*	pDocument,
			const CString&		sName,
			BOOL				bAllowEmpty = !ALLOW_EMPTY_BODY
		);
	DBTSlave
		(
			const CString&		sTableName,
			CAbstractFormDoc*	pDocument,
			const CString&		sName,
			BOOL				bAllowEmpty = !ALLOW_EMPTY_BODY
		);
	DBTSlave
		(
		SqlRecord*			pRecord,
		CAbstractFormDoc*	pDocument,
		const CString&		sName,
		BOOL				bAllowEmpty = !ALLOW_EMPTY_BODY
		);

	virtual ~DBTSlave();

public:
	// abilita solo le funzioni delete per gestire delle persistenze ridotte
	// al solo caso di deletazione per ottimizzare le prestazioni
	BOOL GetOnlyDelete  ()							{ return m_bOnlyDelete; }
	void SetOnlyDelete	(BOOL bOnlyDelete = TRUE)	{ m_bOnlyDelete = bOnlyDelete; }

	void SetDelayedRead	(ReadType aReadType = BROWSE_DELAYED)	{ m_ReadType = aReadType; }
	ReadType GetDelayedReadType	()	{ return m_ReadType; }

	// il paramentro a TRUE consente di ignorare il corrente valore di massimo
	// numero di righe caricabili : vale solo per il DBTSlaveBuffered
	BOOL Reload		(BOOL bIgnorePreloadStep = FALSE);

	BOOL GetAllowEmpty () { return m_bAllowEmpty; }
	void SetAllowEmpty (const BOOL& bValue) { m_bAllowEmpty = bValue; }
	
	const DBTObject*	GetMaster() const	{ return m_pDBTMaster; }
	SqlRecord*	GetMasterRecord() { if (m_pMasterRecord) return m_pMasterRecord;  ASSERT_VALID(GetMaster());  return (GetMaster() ? GetMaster()->GetRecord() : NULL); }

	bool	IsOnlyForRead () { return m_bOnlyForRead; }
	void	SetOnlyForRead(bool bSet) { m_bOnlyForRead = bSet; }
public:
	virtual	void	SetPreloadStep		(int)		{}
	virtual	int		GetPreloadStep		()	const	{ return -1; }

	virtual BOOL	Update				();
	
protected:
	virtual BOOL DelayedRead	(BOOL bPrepareOld);

protected:
	// NON DEVE essere reimplementate nelle classi istanziabili
	virtual	BOOL CheckTransaction	();
	virtual	BOOL GetCursorType		() { return E_NO_CURSOR; } // x ottimizzare le performance visto che é un rowset di una sola riga nn ha senso usare cursori

protected:	// member
	virtual BOOL	AddNewData	();
	virtual BOOL	ModifyData	();

	// don't implement AddNew, use base class
	virtual BOOL LocalFindData	(BOOL bPrepareOld = TRUE);
	virtual BOOL FindData		(BOOL bPrepareOld = TRUE);

	virtual BOOL Edit	();
	virtual BOOL Delete	();

	// Implementare nelle derivate solo se e` necassario
	virtual void	OnEnableControlsForFind		() {}
	virtual void	OnDisableControlsForEdit	() {}

	//reprepare primarykey segments
	virtual void ForcePreparePrimaryKey();

	// DEVE essere implementata nella classe finale se si accettano
	// DBTSlave vuoti (ALLOW_EMPTY_BODY)
	virtual BOOL	IsEmptyData	();
public:
	//virtual BOOL	IsEmptyContent	(CString* = NULL)		{ return FALSE; }	//Implentare per controlli logici
	// DEVONO essere implementate nella classe finale
	virtual	void OnDefineQuery			();
	virtual	void OnPrepareQuery			();
	virtual	void OnPreparePrimaryKey	();

	virtual void 	OnBeforeXMLExport	() {}
	virtual void 	OnAfterXMLExport	() {}
	virtual BOOL 	OnOkXMLExport		() { return TRUE; }

	virtual void 	OnBeforeXMLImport	() {}
	virtual void 	OnAfterXMLImport	() {}
	virtual BOOL 	OnOkXMLImport		() { return TRUE; }

	//virtual BOOL	Parse		(CXMLNode* pNode, BOOL bWithAttributes = TRUE, BOOL bParseLocal = FALSE);
	virtual BOOL	UnParse		(CXMLNode* pParentNode, BOOL bWithAttributes = TRUE, BOOL bUnParseLocal = FALSE, BOOL /*bUnParseSlavable*/ = FALSE);

	virtual void	GetJson		(BOOL bWithChildren, CJsonSerializer& jsonSerializer, BOOL bOnlyWebBound);
	virtual void	SetJson		(BOOL bWithChildren, CJsonParser& jsonParser);

// Diagnostics
#ifdef _DEBUG
public:
	void Dump		(CDumpContext&)	const;
	void AssertValid()				const;
#endif // _DEBUG
};

//////////////////////////////////////////////////////////////////////////////
// Container of Smart Pointers to BodyEdit
// I use a list because a DBT can be data source for more than one grid
// when grid is destroyed, the corresponding pointer of the list is nulled
///////////////////////////////////////////////////////////////////////////////
class CBodyEditPointers : public CArray<TDisposablePtr<CBodyEdit>*>
{
public:
	~CBodyEditPointers();

	CBodyEdit* GetPointerAt(int i);
	void AddPointer(CBodyEdit* pEdit);
};

///////////////////////////////////////////////////////////////////////////////
class DBTSlaveMap
{
public:
	DBTSlaveMap() : m_pDBTSlavePrototype(NULL)
	{
	}
	~DBTSlaveMap()
	{
		SAFE_DELETE(m_pDBTSlavePrototype);
	}
	TDisposablePtr<DBTSlave> m_pCurrentDBTSlave;
	DBTSlave* m_pDBTSlavePrototype;
	CMap<SqlRecord*, SqlRecord*, DBTSlave*, DBTSlave*> m_Slaves;

	//nel caso di più livelli di nesting, esistono più prototipi (uno per ogni riga del parent, più quello del prototipo del parent)
	//a me interessa quello del prototipo del parent
	DBTSlave* GetSiblingPrototype();
};
#define GET_DBTSLAVE_DEPRECATED __declspec(deprecated("\nWARNING! This function is deprecated. You should use the overload that receives the DBT name as first parameter, or the template that uses the DBT class."))

typedef  void (__stdcall *ROW_FUNC) (SqlRecord*);

struct HKLDescriptionInfo
{
	CString m_DescriptionField;
	HotKeyLink* m_pHKL;
	HKLDescriptionInfo* Clone();
};
struct HKLKeyInfo
{
	CString m_strKeyField;
	HotKeyLink* m_pHKL;
	HKLKeyInfo* Clone();
};
DataObj* GetDataObjFromName(SqlRecord* pRec, const CString& sFieldName);
//=============================================================================
class TB_EXPORT DBTSlaveBuffered : public DBTSlave, public IDataProvider
{
	friend DBTMaster;
	friend class CXMLDataManager;
	friend class DBTSlaveBufferedIterator;
	friend class DBTSlaveMap;
	friend class CBodyEdit;
	friend class CTreeBodyEdit;
	friend class CRowFormView;
	friend class CSlaveViewContainer;
	friend class CDBTTreeEdit;

	DECLARE_DYNCREATE(DBTSlaveBuffered)

	enum CompareStatus { NEW_ROW, MODIFIED, EQUAL };

private:
	CJsonWrapper		m_JsonData;//per i delta dei dati json
	DATAOBJ_ROW_FUNC	m_pFnDuplicateKey;
	CBodyEditPointers	m_arBodyPtr;
	TArray<HKLDescriptionInfo>m_arHKLDescriptionInfos; //array degli hotlink a cui sono associati campi di decodifica dinamici
	TArray<HKLKeyInfo>m_arHKLKeyInfos; //array degli hotlink a cui sono associati campi chiave per la decodifica dinamica
protected:	// data
	RecordArray*	m_pRecords;				// buffered body

public:
	RecordArray*	m_pOldRecords;			// buffered old body
	DataObj*		m_pDefaultDuplicateKeyDataObj;
protected:	// data
	BOOL			m_bCheckDuplicateKey;
	int				m_nCurrentRow;			// current row pointed from BodyEdit
	int				m_nPreloadStep;			// numero di record letti in modo browse 
	// Servono solo al BodyEdit per gestire la disabiltazione di tutte le 
	// colonne senza dover disabilitare tutti i dataobj
	BOOL			m_bAlwaysReadOnly;
	BOOL			m_bReadOnly;
	BOOL			m_bModified;

	// servono per poter conoscere dall'esterno quali se un record è stato inserito
	// modificato o cancellato. Questo può essere fatto attraverso i metodi
	// IsNewRow, IsModifiedRow e IsDeletedRow.
	CDWordArray*			m_pwNewRows;
	CDWordArray*			m_pwModifiedRows;
	CDWordArray*			m_pwDeletedRows;

	//Filtraggio
	RecordArray*			m_pAllRecords;
	BOOL					m_bAllowFilter;
	//Free list optimization
	RecordArray*			m_pFreeList;

	CArray<DBTSlaveMap*>	m_DBTSlaveData;

	bool					m_bFindDataCalled;
	CArray<DBTSlave*>		m_DeletedSlaves;
	DWORD					m_dwLatestModifyTime;
	DBTSlaveBuffered  ();
public:
	// constructors
	DBTSlaveBuffered
		(
			CRuntimeClass*		pClass,
			CAbstractFormDoc*	pDocument,
			const CString&		sName,
			BOOL				bAllowEmpty = ALLOW_EMPTY_BODY,
			BOOL				bCheckDuplicateKey = !CHECK_DUPLICATE_KEY
		);
	DBTSlaveBuffered
		(
			const CString&		sTableName,
			CAbstractFormDoc*	pDocument,
			const CString&		sName,
			BOOL				bAllowEmpty = ALLOW_EMPTY_BODY,
			BOOL				bCheckDuplicateKey = !CHECK_DUPLICATE_KEY
		);
	DBTSlaveBuffered
		(
		SqlRecord*			pRecord,
		CAbstractFormDoc*	pDocument,
		const CString&		sName,
		BOOL				bAllowEmpty = ALLOW_EMPTY_BODY,
		BOOL				bCheckDuplicateKey = !CHECK_DUPLICATE_KEY
		);
	virtual ~DBTSlaveBuffered();

protected:
	BOOL CheckSlaveTransaction ();
	// NON DEVE essere reimplementate nelle classi istanziabili
	virtual	BOOL CheckTransaction ();
	virtual	BOOL GetCursorType	  () { return E_FAST_FORWARD_ONLY; } // x ottimizzare le performance visto che é un rowset di + righe
	
protected:	// member
	SqlRecord*		InsertNewRecord		(int nRow) ;
    CompareStatus	CompareRow			(int nRow, int& nFoundedRow);
	BOOL			AddNewRow			(int nRow);
	BOOL			UpdateRows			();
	BOOL			DeleteUnusedRows	();

	// called by BodyEdit
	DataObj*		CheckRow		(int nRow, BOOL bCheckAll = TRUE, BOOL bFromBody = TRUE);
	DataObj*		CheckRecords	(int& nRow, BOOL bFromBody = TRUE);
	BOOL			MoreRecords		()	const;

protected:	// member
	virtual void	LoadNextRecords	();
		// base functions
private:
	void		 SetCurrentRow	 (int nRow, BOOL bAlignCurrentSlave);
	void		 AttachDataEventsProxy(SqlRecord* pAddedRec);

	DBTSlaveMap* GetDBTSlaveData(const CString& strDBTName);
	DBTSlaveMap* GetDBTSlaveData(const CRuntimeClass* pClass);
	void		RemoveAllRecords();
	CBodyEditPointers* GetBodyEdits();
	void AddHotLinkDescriptionField(HotKeyLink* pHKL, const CString& sDescriField);
public:
	void PrepareDynamicColumns(BOOL bUpdateDescriptions);
	void PrepareDynamicColumns(SqlRecord *pRec, BOOL bUpdateDescriptions);
	void AddHotLinkKeyField(HotKeyLink* pHKL, const CString& sKeyField);
	DataObj* GetBindingData(const CString& strParentDataSource, const CString& strDataSource, CString& sFieldName, CString& sBindingName, bool &fromHKL);
	DBTSlave*	GetMainPrototype();
	void	Attach	(DBTSlave*);
	
	void GetSlavesDBTS(CStringArray& arNames);

	void	SetDuplicateKeyFunPtr(DATAOBJ_ROW_FUNC funPtr);

	void	SuspendObservables();
	void	ResumeObservables();

	void	ResetJsonData();
	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//deprecated method
	GET_DBTSLAVE_DEPRECATED DBTSlave* GetCurrentDBTSlave();
	//use one of the following instead:
	DBTSlave* GetCurrentDBTSlave(const CString& strDBTName);
	template<class T> T* GetCurrentDBTSlave()
	{
		DBTSlaveMap* pData = GetDBTSlaveData(RUNTIME_CLASS(T));
		return (T*) (pData ? pData->m_pCurrentDBTSlave.operator DBTSlave *() : NULL);	
	}
	DBTSlave* GetCurrentDBTSlave(CRuntimeClass* pDBTClass)
	{
		DBTSlaveMap* pData = GetDBTSlaveData(pDBTClass);
		return (DBTSlave*) (pData ? pData->m_pCurrentDBTSlave.operator DBTSlave *() : NULL);	
	}
	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//deprecated method
	GET_DBTSLAVE_DEPRECATED DBTSlave*	GetDBTSlave(int nRow, BOOL bForceCreate = FALSE);
	//use one of the following instead:
	DBTSlave*	GetDBTSlave(const CString& strDBTName, int nRow, BOOL bForceCreate = FALSE);
	template<class T> T* GetDBTSlave(int nRow, BOOL bForceCreate = FALSE)
	{
		DBTSlaveMap* pData = GetDBTSlaveData(RUNTIME_CLASS(T));
		return (T*) (pData ? GetDBTSlave(nRow, pData, bForceCreate): NULL);
	}
	DBTSlave* GetDBTSlave(CRuntimeClass* pDBTClass, int nRow, BOOL bForceCreate = FALSE)
	{
		DBTSlaveMap* pData = GetDBTSlaveData(pDBTClass);
		return (pData ? GetDBTSlave(nRow, pData, bForceCreate): NULL);	
	}
	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//deprecated method
	GET_DBTSLAVE_DEPRECATED DBTSlave*	GetDBTSlave(SqlRecord* pRecordMaster, BOOL bForceCreate = FALSE);
	//use one of the following instead:
	DBTSlave*	GetDBTSlave(const CString& strDBTName, SqlRecord* pRecordMaster, BOOL bForceCreate = FALSE);
	template<class T> T* GetDBTSlave(SqlRecord* pRecordMaster, BOOL bForceCreate = FALSE)
	{
		DBTSlaveMap* pData = GetDBTSlaveData(RUNTIME_CLASS(T));
		return (T*) (pData ? GetDBTSlave(pRecordMaster, pData, bForceCreate) : NULL);
	}

	SqlRecord*	AddNewRecord	(BOOL bCopyCurrent);
	
	void		DestroyDBTSlave(DBTSlave* pSlave);
	DBTSlave*	GetDBTSlave(int nRow, DBTSlaveMap* pData, BOOL bForceCreate = FALSE);
	DBTSlave*	GetDBTSlave(SqlRecord* pRecordMaster, DBTSlaveMap* pData, BOOL bForceCreate = FALSE);
	DBTSlave*	CreateDBTSlave(DBTSlave*pPrototype);
	
	DBTObject*	GetDBTObject(const CRuntimeClass* pDBTClass) const;
	DBTObject*	GetDBTObject(const CString& sTableName) const;
	DBTObject*	GetDBTObject(const CTBNamespace&	aNs) const;
	DBTObject*	GetDBTByName(const CString& sDbtName) const;
	virtual DBTObject*	GetDBTObject (SqlRecord*);
	
	virtual void		GetJson(BOOL bWithChildren, CJsonSerializer& jsonSerializer, BOOL bOnlyWebBound);
	virtual void		SetJson(BOOL bWithChildren, CJsonParser& jsonParser);
	
	virtual void		GetJsonForSingleDBT(CJsonSerializer& jsonSerializer, BOOL bOnlyWebBound);

	SqlRecord*	GetCurrentMasterRecord(const CString& strDBTName = _T(""));

	// useful getting routines
	RecordArray* GetRecords		 ()	const			{ ASSERT(m_pRecords); return m_pRecords; }
	SqlRecord*	 GetRow			 (int nRow) const;
	virtual void SetCurrentRow	 (int nRow);
	SqlRecord*	 GetCurrentRow	 () const 			{ return (m_nCurrentRow < 0 || m_nCurrentRow > m_pRecords->GetUpperBound()) ? NULL : GetRow(m_nCurrentRow); }
	void		 SetCurrentRowForValueChanged	    (int nRow);
	int			 GetCurrentRowIdx() const 			{ return m_nCurrentRow; }
	int			 GetUpperBound	 () const			{ ASSERT(m_pRecords); return m_pRecords->GetUpperBound(); }
	int			 GetSize		 () const			{ ASSERT(m_pRecords); return m_pRecords->GetSize(); }
 	virtual int	 GetRowCount	 () const			{ return GetSize(); }
	int			 GetTotalRowCount() const			{ ASSERT(m_pTable); return m_pTable->GetRowSetCount(); }

    BOOL		 IsEmpty		 () const			{ ASSERT(m_pRecords); return m_pRecords->GetSize() == 0; }

	BOOL		IsCheckingDuplicateKey		() const							{ return m_bCheckDuplicateKey; }
	void		SetCheckDuplicateKey		(BOOL bCheckDuplicateKey = TRUE)	{ m_bCheckDuplicateKey = bCheckDuplicateKey; }
	int			FindRecordIndex				(SqlRecord* pRec);

    // Gestione  del ReadOnly utile per il BodyEdit
	BOOL		IsReadOnly		() const				{ return m_bReadOnly; }
	virtual void		SetReadOnly		(BOOL bReadOnly = TRUE, BOOL bRecursive = FALSE);
	void		SetAlwaysReadOnly		(BOOL bReadOnly = TRUE) { if (bReadOnly) m_bReadOnly = bReadOnly; m_bAlwaysReadOnly = bReadOnly; }
	void		SetReadOnlyCurrentRow (BOOL bReadOnly = TRUE);
	void		SetReadOnlyRow		  (int nRow, BOOL bReadOnly = TRUE);

	void		SetDataOSLReadOnly	  (BOOL bReadOnly = TRUE, int idx = -1,  BOOL onlyLoaded = TRUE, int startRowIndex = 0);

	// posso sapere dopo il salvataggio di un documento in EDIT se la riga nRow è stata aggiunta
	// modificata o deletata
	BOOL		IsNewRow		(int nRow) const;
	BOOL		IsModifiedRow	(int nRow, int& nOldRow) const; // in nOldRow ho l'informazione del numero di riga nell'oldrecord
	BOOL		IsDeletedRow	(int nRow) const;

	// useful getting routines for old body
	SqlRecord*	GetOldRow			(int nRow) const{ ASSERT(m_pOldRecords); return (SqlRecord*) m_pOldRecords->GetAt(nRow); }
	int			GetOldUpperBound	() const		{ ASSERT(m_pOldRecords); return m_pOldRecords->GetUpperBound(); }
	int			GetOldSize			() const		{ ASSERT(m_pOldRecords); return m_pOldRecords->GetSize(); }
	BOOL		IsOldEmpty			() const		{ ASSERT(m_pOldRecords); return m_pOldRecords->GetSize() == 0; }

private:
	void		AddBodyEdit		(CBodyEdit* pBody);
	void		AlignBodyEdits(BOOL bRefreshBody);
	void		InternalSetModified(BOOL bModified = TRUE);
protected:
	// controlla che due righe non abbiano la stessa chiave primaria
	BOOL IsDuplicateKey(SqlRecord*, SqlRecord*);
	CompareStatus CompareWithOldRow(int nRow, int& nFoundRow);

	// Utile al programmatore per rafforzare il concetto di riga
	// duplicata su campi che non fanno parte di chiave primaria.
	// L'algoritmo di update non viene inficiato.
	virtual BOOL UserIsDuplicateKey(SqlRecord*, SqlRecord*) { return FALSE; }
	 
public:
	virtual BOOL IsActive();
	virtual	SqlRecord*	AddOldRecord	();
	/*TBWebMethod*/virtual	SqlRecord*	AddRecord		();

	virtual	SqlRecord*	InsertRecord	(int nRow);
	virtual	BOOL		DeleteRecord	(int nRow);
	virtual	void		RemoveAll		(BOOL bRemoveOld = FALSE);

	virtual BOOL		AddNewData		();
	virtual BOOL		ModifyData		();
	virtual BOOL		Delete			();
	virtual BOOL		Update			();

	virtual BOOL		LocalFindData	(BOOL bPrepareOld = TRUE);
	virtual BOOL		FindData		(BOOL bPrepareOld = TRUE);
	virtual void		OnFindData		() {}

	// se negativo significa disattivare la limitazione di caricamento righe
	virtual	void		SetPreloadStep	(int n)		{ m_nPreloadStep = n; }
	virtual	int			GetPreloadStep	()	const	{ return m_nPreloadStep; }

	virtual BOOL		IsModified		() const;
	virtual void		SetModified	(BOOL bModified = TRUE);

	//reprepare primarykey segments
	virtual void ForcePreparePrimaryKey();

protected:
	// overridable
	virtual void 	Init		();

	// don't implement Edit and AddNew, use DBTSlave class method
	virtual BOOL	AddNew		(BOOL bInit = TRUE);
	virtual BOOL	Edit		();

	virtual	void OnDefineQuery	();
	virtual	void OnPrepareQuery	();

protected:
	// Implementare nelle derivate solo se e` necessario
	virtual void	OnEnableControlsForFind		() {}
	virtual void	OnDisableControlsForEdit	() {}

	// NON DEVE essere reimplementata nella classe finale, e non deve essere mai
	// chiamata. Bisogna implementare l'equivalente routine che lavora sulle righe
	virtual	BOOL OnCheckPrimaryKey		() { ASSERT(FALSE); return FALSE; }
	virtual	void OnPreparePrimaryKey	() { ASSERT(FALSE); }

protected:
	// DEVONO essere implementate nella classe finale
	virtual DataObj*	OnCheckPrimaryKey	(int /*nRow*/, SqlRecord*);
	virtual void		OnPreparePrimaryKey	(int /*nRow*/, SqlRecord*);

	virtual void		OnPrepareDynamicColumns	(SqlRecord*);
	virtual void		OnPrepareAuxColumns		(SqlRecord*);
	virtual void		OnPrepareOldAuxColumns	(SqlRecord*);

	virtual void		OnRecordAdded		(SqlRecord* pRec, int row);

	virtual	void		OnSetCurrentRow		();
	virtual void		OnPrepareRow		(int /*nRow*/, SqlRecord*);

	virtual BOOL		OnBeforeAddRow		(int /*nRow*/);
	virtual void		OnAfterAddRow		(int /*nRow*/, SqlRecord*);

	virtual BOOL		OnBeforeInsertRow	(int /*nRow*/);
	virtual void		OnAfterInsertRow	(int /*nRow*/, SqlRecord*);

	virtual BOOL		OnBeforeDeleteRow	(int /*nRow*/);
	virtual void		OnAfterDeleteRow	(int /*nRow*/);

	virtual DataObj*	OnCheckUserData		(int /*nRow*/)				{ return NULL; }
	virtual DataObj*	OnCheckUserRecords	(int& /*nRow*/)				{ return NULL; }

	// Torna il data obj su qui riposizionare il fuoco (NULL = NON RIPOSIZIONA)
	virtual	DataObj*	GetDuplicateKeyPos	(SqlRecord*);

	// Messaggio di errore specializzato da parte del programmatore
	virtual	CString		GetDuplicateKeyMsg	(SqlRecord*);

public:
	virtual void 	OnBeforeXMLExport	(int /*nRow*/) {}
	virtual void 	OnAfterXMLExport	(int /*nRow*/) {}
	virtual BOOL 	OnOkXMLExport		(int /*nRow*/) { return TRUE; }

	virtual void 	OnBeforeXMLImport	(int /*nRow*/) {}
	virtual void 	OnAfterXMLImport	(int /*nRow*/) {}
	virtual BOOL 	OnOkXMLImport		(int /*nRow*/) { return TRUE; }

			void	LoadAllRecords		();
			BOOL	LoadMoreRows		(int preloadStep);
			BOOL	ManualLoad			(); //for unattached DBTs

	virtual	BOOL	MemorySort			(LPCTSTR szOrderBy);
	virtual	BOOL	MemorySort			();
	virtual	void	OnMemorySorted		() {}

	virtual	int			FindRecordIndex	(const CString& sColumnName, const DataObj* aVal, int nStartPos = 0) const;
			SqlRecord*	FindRecord		(const CString& sColumnName, DataObj* aVal, int nStartPos = 0);
			SqlRecord*	FindRecord		(const CStringArray& arColumnName, const DataObjArray& arValues, int nStartPos = 0);

	virtual	BOOL		CalcSum			(int nIndex, DataObj& aSum) const;
	virtual	BOOL		CalcSum			(const CString& sColumnName, DataObj& aSum) const ;

	virtual DataObj*	GetMinElem		(const CString& /*sColumnName*/);
	virtual DataObj*	GetMaxElem		(const CString& /*sColumnName*/);

	virtual	BOOL		PrepareSymbolTable	(SymTable*);
	
			BOOL		GetMatchRecords (RecordArray& ar, const CString& sExpr, SymTable* parent/*=NULL*/);

//Filtraggio righe/Tree ----------------------------------------------
private:
	class MemFilter: public CObject
	{
	public:
		enum EMemFilterType { FILTER_NONE, FILTER_DATAOBJS, FILTER_STRINGS, FILTER_EXPR, FILTER_METHOD, FILTER_TOP, FILTER_COLUMNDATA };

		DBTSlaveBuffered*	m_pDBT = NULL;

		EMemFilterType		m_eFilterType;
		BOOL				m_bUI;

		CArray<int>			m_arColumnsDataIndex;
		//CStringArray		m_arColumnsName;

		DataObjArray		m_arFilterValues;
		CStringArray		m_arFilterStrValues;

		BOOL				m_bAnd_Or;

		ECompareType		m_eCmp;
		CParsedCtrl*		m_pCtrl = NULL;
		DataObj*			m_pCmpObj = NULL;

		SymTable*			m_pSymTable = NULL;
		::Expression*		m_pExpr = NULL;

		int					m_nTop = 0;

		MemFilter (DBTSlaveBuffered* pDBT, const CArray<int>& arColumnDataIndex,	const DataObjArray& arFilterValue,		BOOL bAnd_Or);
		MemFilter (DBTSlaveBuffered* pDBT, int nColumnDataIndex, DataObj* pFilterValue, ECompareType);

		MemFilter (DBTSlaveBuffered* pDBT, int nColumnDataIndex, const CStringArray& arFilterStrValues,	ECompareType, CParsedCtrl*, DataObj*);
		MemFilter (DBTSlaveBuffered* pDBT, ::Expression*, SymTable*);
		MemFilter (DBTSlaveBuffered* pDBT);	//call virtual methd CheckFilterMethod
		MemFilter (DBTSlaveBuffered* pDBT, int nTop);	//call virtual methd CheckFilterMethod

		virtual ~MemFilter();

		BOOL	Check			(SqlRecord* pRec);
			BOOL	CheckDataObjs	(SqlRecord* pRec);
			BOOL	CheckStrings	(SqlRecord* pRec);
			BOOL	CheckExpr		(SqlRecord* pRec);
			BOOL	CheckMethod		(SqlRecord* pRec);
			BOOL	CheckColumnData	(SqlRecord* pRec);

	};

	Array m_arMemFilters;

	void	ApplyMemoryFilters	();
public:
	enum ERemoveMemFilter { REMOVE_FILTER_NONE, REMOVE_FILTER_ALL, REMOVE_FILTER_UI, REMOVE_FILTER_NOT_UI };
private:
	void	RemoveMemoryFilterAux(BOOL bRefreshBody = TRUE, ERemoveMemFilter eRemoveFilters = REMOVE_FILTER_ALL, int nColumnDataIdx = -1);
public:
	BOOL	RemoveMemoryFilter(BOOL bRefreshBody = TRUE, ERemoveMemFilter eRemoveFilters = REMOVE_FILTER_ALL, int nColumnDataIdx = -1);
	virtual BOOL CheckFilterMethod(SqlRecord*) { return TRUE; }

	//filtra le righe: sono usate dal Bodyedit
	BOOL	MemoryFilter(BOOL bRefreshBody = TRUE, ERemoveMemFilter eRemoveFilters = REMOVE_FILTER_ALL, int nTop = 0);
	BOOL	MemoryFilter(int nColumnDataIndex, const CStringArray& arFilterValues, ECompareType = CMP_EQUAL, CParsedCtrl* = NULL, BOOL bRefreshBody = TRUE, ERemoveMemFilter eRemovePrevFilters = REMOVE_FILTER_NONE);
	BOOL	MemoryFilter(int nColumnDataIndex, CString sFilterValue, ECompareType = CMP_EQUAL, CParsedCtrl* = NULL, BOOL bRefreshBody = TRUE, ERemoveMemFilter eRemovePrevFilters = REMOVE_FILTER_NONE);
	//filtri programmativi - implementa le successive
	BOOL	MemoryFilter(const CArray<int>& arColumnDataIndex, const DataObjArray& arFilterValue, BOOL bAnd_Or = TRUE, BOOL bRefreshBody = TRUE, ERemoveMemFilter eRemoveFilters = REMOVE_FILTER_ALL);
	//filtra le righe sulle colonna in AND o in OR dei valori passati
	BOOL	MemoryFilter(const CStringArray& arColumnName, const DataObjArray& arFilterValue, BOOL bAnd_Or = TRUE, BOOL bRefreshBody = TRUE, ERemoveMemFilter eRemoveFilters = REMOVE_FILTER_ALL);
	//filtra le righe sulla colonna in OR dei valori passati (sottocaso della precedente)
	BOOL	MemoryFilter(const CString& sColumnName, const DataObjArray& arFilterValue, BOOL bRefreshBody = TRUE, ERemoveMemFilter eRemoveFilters = REMOVE_FILTER_ALL);
	//filtra le righe sulla colonna (sottocaso della precedente)
	BOOL	MemoryFilter(const CString& sColumnName, DataObj* filterValue, BOOL bRefreshBody = TRUE, ERemoveMemFilter eRemoveFilters = REMOVE_FILTER_ALL);

	BOOL	MemoryFilter(const CString& sColumnName, DataObj* filterValue, ECompareType, BOOL bRefreshBody = TRUE, ERemoveMemFilter eRemoveFilters = REMOVE_FILTER_ALL);

	//filtra le righe che soddisfano l'espressione in sintassi Woorm-like, si devono utilizzare i nomi delle colonne della tabella (i campi local hanno 'l_' come prefisso)
	BOOL	MemoryFilter(const CString& sExpression, SymTable* parent = NULL, BOOL bRefreshBody = TRUE, ERemoveMemFilter eRemoveFilters = REMOVE_FILTER_ALL);
																																
	BOOL	IsAllowFilter		();
	void	SetAllowFilter		(BOOL bAllow);

	BOOL	IsMemoryFiltered		() const;
	BOOL	IsMemoryFilterActive	() const;
	BOOL	IsMemoryUIFilterActive	(int nColumnDataIdx = -1) const;
			
	int		RemapIndexF2A	(const int nRow);
	int		RemapIndexA2F	(const int nRow);

	// iterazione dbt completo
	int			GetUnfilteredUpperBound ()			const { ASSERT_VALID(m_pAllRecords); return m_pAllRecords->GetUpperBound(); }
	SqlRecord*	GetUnfilteredRow		(int nRow)	const { ASSERT_VALID(m_pAllRecords); return m_pAllRecords->GetAt(nRow); }
	BOOL		DeleteUnfilteredRecord	(int nRow);
	// END Filtraggio/Tree -------------------------------------------------

	virtual void SetReadOnlyFields	() {}

	virtual	void PrepareAllPrimaryKeys (int startRow = 0);

			void ReNumberColumn (DataObj*, int startRow = 0);
			void ReNumberColumn (int nDataIdx, int startRow = 0);

			BOOL IsUniqueColumn (DataObj*, int startRow = 0, int* pIdxR1 = NULL, int* pIdxR2 = NULL) const;
			BOOL IsUniqueColumn (int nDataIdx, int startRow = 0, int* pIdxR1 = NULL, int* pIdxR2 = NULL) const;

	/*TBWebMethod*/virtual SqlRecord* CreateRecord();

	/*TBWebMethod*/SqlRecord*	TbScriptGetRow				(DataLng nRow);
	/*TBWebMethod*/SqlRecord*	TbScriptGetCurrentRow		();
	/*TBWebMethod*/void			TbScriptSetCurrentRow		(DataLng nRow);
	/*TBWebMethod*/DataLng		TbScriptGetCurrentRowIdx	();
	/*TBWebMethod*/DataLng		TbScriptGetSize				();
	/*TBWebMethod*/SqlRecord*	TbScriptInsertRecord	(DataLng nRow);
	/*TBWebMethod*/void			TbScriptDeleteRecord	(DataLng nRow);
	/*TBWebMethod*/void			TbScriptRemoveAll		();

	virtual DataObj*	GetData (const CString& /*sName*/, int /*nRow*/ = -1);

protected:
	void	ClearSlaveDBTs();
	
private:
	DBTSlaveBuffered*	GetActiveSibling			();
	void				SetCurrentDBTSlave			(DBTSlaveMap* pData, DBTSlave* pSlave);
	void				AlignDBTSlaveToCurrentRow	();

public:
			BOOL		Parse		(const CString& filename, BOOL bWithAttributes = TRUE, BOOL bParseLocal = FALSE);
	virtual BOOL		Parse		(CXMLNode* pParentNode, BOOL bWithAttributes = TRUE, BOOL bParseLocal = FALSE);
	virtual BOOL		UnParse		(CXMLNode* pParentNode, BOOL bWithAttributes = TRUE, BOOL bUnParseLocal = FALSE, BOOL bUnParseSlavable = FALSE);
	DWORD	GetLatestModifyTime();
// Diagnostics
#ifdef _DEBUG
public:
	void Dump		(CDumpContext&)	const;
	void AssertValid()				const;
#endif // _DEBUG
};

#include "endh.dex"
