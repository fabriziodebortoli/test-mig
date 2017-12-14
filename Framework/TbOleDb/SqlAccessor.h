
#pragma once

#include <TbGeneric\DataObj.h>

#include "sqlobject.h"
#include "sqlrec.h"
#include "sqlcatalog.h"
#include "oledbmng.h"

//includere alla fine degli include del .H
#include "beginh.dex"

class CBindingBlob;

// contiene le classi necessarie per la gestione delle colonne di risultato (Bind columns)
// e per la gestione dei paramentri (Bind params)
// La classe SqlBindingElem serve per descrivere una singola colonna su cui effettuare
// il bind oppure un parametro.
// Il tipo dell'elemento specifica questa differenza e può essere:
//	DBPARAMIO_NOTPARAM : se è una colonna di bind
//	DBPARAMIO_INPUT: se è un parametro di input
//	DBPARAMIO_OUTPUT: se è un parametro di output
//	DBPARAMIO_INPUT | DBPARAMIO_OUTPUT: se è un parametro di input/output 
//	

extern const TB_EXPORT int nEmptySqlRecIdx;
//////////////////////////////////////////////////////////////////////////////
//							SqlBindingElem Definition
//////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
class TB_EXPORT SqlBindingElem : public CObject
{
	friend class SqlAccessor;
	friend class SqlBindingElemArray;
	friend class SqlTable;
	friend class SqlParamArray;
	friend class SqlColumnArray;
	friend class AuditingManager;
	friend class SqlTableStruct;

	DECLARE_DYNAMIC (SqlBindingElem)

private:
	CString		m_strBindName;	// nome della colonna o del parametro	
	DataObj*	m_pDataObj;     // Usato per prendere il data value da inserire
								// nel buffer di binding delle colonne
	DataObj*	m_pOldDataObj;  // Usato per minimizzare le requery con stessi parametri e le modifiche di un record

	int			m_nSqlRecIdx;	// Index of DataObj position into the related SqlRecord
	DBTYPE		m_nDBType;
	DWORD		m_dwStatus;		// lo stato di bind della colonna
	DBPARAMIO	m_eParamIO;		// il tipo di parametro 
	
	CBindingBlob* m_pBindingBlob; // struttura per effettuare il binding di un campo di tipo blob

	BOOL		m_bUpdatable;		//se il campo é presente nella clause di set della query di update nel caso di keyedupdate
	BOOL		m_bAutoIncrement;	//se é un campo autoincrement
	BOOL		m_bOwnData;			//se TRUE il dataobj é stato creato al volo e va cancellato nel distruttore altrimenti
									// é un dataobj appartenente ad un altro oggetto
	BOOL		m_bUseLength;		//deve utilizzare il campo lunghezza nel buffer di binding:
									// DataText in SqlServer ed Oracle
									// DataStr in Oracle
									// DataGuid in Oracle	
	BOOL		m_bReadOnly;
	CString		m_sLocalName;		//nome del local per i campi calcolati (Es: SqlTable::SelectSqlFunc)
	
public:
	SqlBindingElem
		(
		const CString&		strBindName, 
		const DBTYPE&		eDBType,
		DataObj*			pDataObj,
		const int&			nSqlRecIdx,
		const DBPARAMIO&	eType = DBPARAMIO_NOTPARAM,
		const CString&		strColumnName = _T("")// serve per i datatext
	);
	virtual ~SqlBindingElem();	

public:
	void		GetParamValue	(DataObj* pDataObj) const	{ pDataObj->Assign(*m_pDataObj); }
	DataObj*    GetDataObj		()					const	{ return m_pDataObj; }
	
	DataType	GetDataType		() const;

	void		ManageUnicodeInDB	(SqlRowSet* pRowSet);
	BOOL		IsUnicodeInDB		() const;
	int			GetSqlRecIdx		() const { return m_nSqlRecIdx; }
	
	virtual		int GetIndex		() const { return m_nSqlRecIdx; }
	
	void	SetReadOnly	(BOOL bSet = TRUE)	{ m_bReadOnly = bSet; }
	BOOL	IsReadOnly	()					{ return m_bReadOnly; }

	CString GetBindName(BOOL bQualified = FALSE);
	
	void	SetUpdatable(BOOL bSet = TRUE)	{ m_bUpdatable = bSet; }
	BOOL	IsUpdatable	()					{ return m_bUpdatable; }

	const CString& GetLocalName() const { return m_sLocalName; }

private:
	void	AssignOldDataObj(const DataObj&);
	void	ClearOldDataObj();

	BOOL	SameValue	()	const			{ return m_pOldDataObj->IsEqual(*m_pDataObj); }

	void	SetParamValue	(const DataObj& aDataObj);

	void	ReadBlobValue	(BYTE* pBuffer);
	long	WriteBlobValue	(BYTE* pBuffer);

// diagnostics
#ifdef _DEBUG
public:	
	void Dump(CDumpContext& dc) const {	ASSERT_VALID(this); AFX_DUMP0(dc, " SqlColumnItem\n"); }
	void AssertValid() const{ CObject::AssertValid(); }
#endif //_DEBUG
};

class TB_EXPORT SqlBindingElem2 : public SqlBindingElem
{
public:
		int m_nIdx;

		SqlBindingElem2
			(
				int nIdx,
				const CString&		strBindName, 
				const DBTYPE&		eDBType,
				DataObj*			pDataObj,
				const int&			nSqlRecIdx,
				const DBPARAMIO&	eType = DBPARAMIO_NOTPARAM,
				const CString&		strColumnName = _T("")// serve per i datatext
			)
				:
				m_nIdx	(nIdx),
				SqlBindingElem
					(
						strBindName, 
						eDBType,
						pDataObj,
						nSqlRecIdx,
						eType,
						strColumnName
					)
				{}

		virtual		int GetIndex			() const { return m_nIdx; }
};

//////////////////////////////////////////////////////////////////////////////
//					SqlAccessor definition
//////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT SqlBindingElemArray : public Array
{
	DECLARE_DYNAMIC (SqlBindingElemArray)

public:
	SqlBindingElemArray() {}

	SqlBindingElem* 	GetAt		(int nIdx)const	{ return (SqlBindingElem*) Array::GetAt(nIdx);	}
	SqlBindingElem*&	ElementAt	(int nIdx)		{ return (SqlBindingElem*&) Array::ElementAt(nIdx); }

	SqlBindingElem* 	operator[]	(int nIdx)const	{ return GetAt(nIdx);	}
	SqlBindingElem*&	operator[]	(int nIdx)		{ return ElementAt(nIdx);	}

	SqlBindingElem* GetParamByName	(const CString& strColumnName);
	SqlBindingElem* GetElemByDataObj(const DataObj* pDataObj);
	DataObj* 		GetDataObj		(const CString& strColumnName);
	DataObj* 		GetOldDataObj	(const CString& strColumnName);

	DataObj* 		GetDataObjAt	(int nIdx);
	const CString&	GetParamName	(int nIdx);

	BOOL			SameValues		() const;	
};

//@@OLE chiedere per la gestione dei stripblank
//////////////////////////////////////////////////////////////////////////////
//					SqlAccessor definition
//////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT SqlAccessor : public SqlBindingElemArray
{
	DECLARE_DYNAMIC (SqlAccessor)

protected:
	DBLENGTH		m_lBufferSize;  // indica la dimensione del buffer di binding. 
									// Viene incrementato ad ogni colonna inserita nella select o ad ogni parametro
									// aggiunto. A seconda che sia un buffer utilizzato per le bind column oppure 
									// utilizzato per la bind dei parametri

	int				m_nOutNumb;		// numero dei parametri con tipo output oppure DBPARAMIO_NOTPARAM
									// mi serve per inserire nel buffer anche lo stato

	int				m_nUseLengthNumb;	// in SQLSERVER per la gestione dei blob ed in ORACLE per le stringhe
									// serve anche la gestione della lunghezza

	BYTE*			m_pBindBuffer; 
	SqlRowSet*		m_pSqlRowSet;
    
private:
	BOOL			m_bBlobAlreadyBinded;

public:
	SqlAccessor(SqlRowSet*);
	~SqlAccessor();

public:

	int	Add
		(
			const CString&	 strName, 
				   DataObj*	 pDataObj, 
			const DBTYPE&	 eDBType,
			const DBPARAMIO& eType,
			const int&		 nSqlRecIdx,
				  BOOL		 bAutoIncrement = FALSE,
			const CString&	strColumnName =_T(""),//serve nel caso di binding di parametro su un filtro di tipo Text
				  int		nInsertPos = -1 //se valorizzato il parametro viene inserito nell'nInsertPos posizione dell'array

		);

	int Add
		(
			const CString&	strName, 
			const DataType& nDataType, 
			const DBTYPE&	eDBType,
			const DBLENGTH& nLen, 
			const int& nSqlRecIdx,
			const DBPARAMIO& eType = DBPARAMIO_INPUT,
			const CString&	strColumnName =_T(""), //serve nel caso di binding di parametro su un filtro di tipo Text
				  int	nInsertPos = -1 //se valorizzato il parametro viene inserito nell'nInsertPos posizione dell'array
		);
	
	
private:
	CString		GetStatusError	(int nIdx);
	BOOL		CanAddStorageObject() const;

protected:
	DBLENGTH 	GetBindLen		(SqlBindingElem*) const;

public:
	BOOL	FixupBindElements	(BOOL bOnlyAutoInc =FALSE); // prende risultato dal buffer dopo la lettura da database e 
								// lo sposta nei dataobj bindati // questo lo posso fare anche per i soli campoi di tipo AUTOINCREMENT
	// prende i valore contenuto nei dataobj bindati e lo copia nel buffer di binding	
	BOOL	FixupBuffer		(BOOL bForced); 	
	BOOL	InitBuffer		(); //pulisce il buffer di appoggio quando la query non estrae non ha estratto alcun dato
	BOOL	CheckStatus		(CDiagnostic* pDiagnostic);
	void	Clear ();
	
// diagnostics
#ifdef _DEBUG
public:	
	void Dump(CDumpContext& dc) const {	ASSERT_VALID(this); AFX_DUMP0(dc, " SqlAccessor\n"); }
	void AssertValid() const{ Array::AssertValid(); }
#endif //_DEBUG
};

//////////////////////////////////////////////////////////////////////////////
//					SqlColumnArray definition
//////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT SqlColumnArray : public SqlAccessor
{
	DECLARE_DYNAMIC (SqlColumnArray)

public:
	SqlColumnArray(SqlRowSet*);

public:
	int Add(SqlRecord* pRecord, int nIdx, int nInsertPos = -1);
	int Add(SqlRecord* pRecord, DataObj* pDataObj, int nInsertPos = -1);
	int	Add(const CString& strName, DataObj* pDataObj, const DBTYPE& eDBType, const int& nSqlRecIdx, BOOL bAutoIncrement = FALSE, int nInsertPos = -1);

public:
	BOOL		BindColumns		(); // predispone buffer di binding e struttura di binding
	BOOL		RibindColumns	(); // ribinding delle colonne di tipo blob per le query preparate
	BOOL		FixupColumns	();		// prende risultato dal buffer dopo la lettura da database e 
										// lo sposta nei dataobj bindati
	BOOL		FixupAutoIncColumns();	//solo per i campi autoincrement

// diagnostics
#ifdef _DEBUG
public:	
	void Dump(CDumpContext& dc) const {	ASSERT_VALID(this); AFX_DUMP0(dc, " SqlColumnArray\n"); }
	void AssertValid() const{ SqlAccessor::AssertValid(); }
#endif //_DEBUG
};

//////////////////////////////////////////////////////////////////////////////
//					SqlParamArray definition
//////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT SqlParamArray : public SqlAccessor
{
	DECLARE_DYNAMIC (SqlParamArray)

private:
	DBPARAMBINDINFO*	m_pParamInfo;
	ULONG*				m_pOrdinals;

public:
	SqlParamArray(SqlRowSet*);
	~SqlParamArray();

public:
	BOOL	BindParameters	();
	BOOL	FixupParameters	();
	BOOL	FixupOutParams	();

	HRESULT SetParameterInfo();

public:
	int			GetParamPosition(const CString& strParamName) const;
	BOOL 		ExistParam		(const CString& strParamName) const { return GetParamPosition(strParamName) != -1; }
	void 		SetParamValue	(const CString& strParamName, const DataObj& aDataObj);
	
	void		GetParamValue	(const CString& strParamName, DataObj* pDataObj) const;
	DataType	GetParamDataType(const CString& strParamName) const;

	void		GetParamValue	(int nPos, DataObj* pDataObj) const;
	DataType	GetParamDataType(int nPos) const;

// diagnostics
#ifdef _DEBUG
public:	
	void Dump(CDumpContext& dc) const {	ASSERT_VALID(this); AFX_DUMP0(dc, " SqlParamArray\n"); }
	void AssertValid() const{ SqlAccessor::AssertValid(); }
#endif //_DEBUG
};

//classe di ausilio per la gestione degli stream
//////////////////////////////////////////////////////////////////////////////
//					CISSHelper definition
//////////////////////////////////////////////////////////////////////////////
//
class CISSHelper : public ISequentialStream  
{
public:

	// Constructor/destructor.
	CISSHelper();
	virtual ~CISSHelper();

	// Helper function to clean up memory.
	virtual void Clear();

	// ISequentialStream interface implementation.
	STDMETHODIMP_(ULONG)	AddRef(void);
	STDMETHODIMP_(ULONG)	Release(void);
	STDMETHODIMP QueryInterface(REFIID riid, LPVOID *ppv);
    STDMETHODIMP Read( 
            /* [out] */ void __RPC_FAR *pv,
            /* [in] */ ULONG cb,
            /* [out] */ ULONG __RPC_FAR *pcbRead);
    STDMETHODIMP Write( 
            /* [in] */ const void __RPC_FAR *pv,
            /* [in] */ ULONG cb,
            /* [out] */ ULONG __RPC_FAR *pcbWritten);

public:

	void*       m_pBuffer;		// Buffer
	ULONG       m_ulLength;     // Total buffer size.
	BOOL		m_bInUnicode;

private:

	ULONG		m_cRef;			// Reference count (not used).
	ULONG       m_iReadPos;     // Current index position for reading from the buffer.
	ULONG       m_iWritePos;    // Current index position for writing to the buffer.

};

//classe di ausilio per effettuare il binding di un blob
//////////////////////////////////////////////////////////////////////////////
//					CBindingBlob definition
//////////////////////////////////////////////////////////////////////////////
//
class CBindingBlob
{
public:
	DataObj*			m_pDataObj; // E'il puntatore al dataobj di tipo blob
	CString				m_strColumnName; //nome colonna Text
	CISSHelper*			m_pISSHelper;
	IUnknown**			m_pUnk;
	BYTE*				m_pBuffer; 
	ULONG				m_ulBytesRead;
	//long				m_iReadPos;     // Current index position for reading from the buffer.
	//long				m_iWritePos;    // Current index position for writing to the buffer.

public:
	CBindingBlob(DataObj* pDataObj, const CString& strColumnName);
	~CBindingBlob();

public:
	void SetUnicodeInDB	(const BOOL& bValue);
	BOOL IsUnicodeInDB	() const;

	void Clear	();
	void Read	(BYTE* pBuffer);
	long Write	(BYTE* pBuffer);
};

#include "endh.dex"
