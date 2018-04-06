
#pragma once

#include <Tboledb\SqlTable.h>

//includere alla fine degli include del .H
#include "beginh.dex"

/*
Generico lettore di record. Utile quando si debbano leggere dei dati con accesso diretto,
ma sia eccessivo implementare un hot-link (ad. es.: dati merce, cli/for dell'articolo, ecc.)
È ottimizzato per la lettura di un record soltanto, rendendolo ideale per letture di dati mediante
chiave primaria (es.: decodifiche)

La classe va derivata per ogni tabella di cui serve un lettore, reimplementando la OnDefineQuery,
la OnPrepareQuery e la FindRecord, in cui si richiama la FindRecord del parent. La FindRecord
delle classi derivate avra` come parametro il codice (i codici) con cui fare accesso diretto.

Un TableReader puo` essere istanziato/usato/distrutto oppure mantenuto "vivo" per tutta la
sessione del documento, a scelta del programmatore. Per ottimizzare le prestazioni
e' meglio tenerlo vivo il piu' possibile in quanto la query si mantiene nel tempo su un
rowset aperto la prima volta che si usa e chiuso solo in distruzione dello tblread

*/

// useful forward class declaration
class SqlRecord;
class SqlCachedTable;
class CBaseDocument;
class SqlSession;

//===========================================================================
class TB_EXPORT TableReader : public CObject
{
	DECLARE_DYNAMIC(TableReader)
	
public:
	enum FindResult { NONE, FOUND, NOT_FOUND };

protected:
	// allocati e disallocati internamente
	SqlRecord*			m_pRecord;
	 //SqlCachedTable*		m_pTable;
	SqlTable*			m_pTable;
	CBaseDocument*		m_pDocument;
	FindResult 			m_PrevResult;
	BOOL	 			m_bForceQuery;
	
	SqlSession*			m_pSqlSession; // sessione di lavoro del TableReader
	BOOL				m_bOnlyOneRecord;

public:
	// constructors
	TableReader	(CRuntimeClass* pClass, CBaseDocument* pDocument);
	TableReader	(const CString& sTableName, CBaseDocument* pDocument);
	TableReader	(CRuntimeClass* pClass, SqlSession* pSqlSession);
	virtual ~TableReader() { Free(); }

public:
	// da usare per effettuare ricerche specifiche. Va reimplementato con i parametri
	// che permettono un accesso diretto (codice/codici di ricerca)
	virtual	FindResult	FindRecord	();
	void	AttachDocument	(CBaseDocument*	pDocument) { ASSERT(m_pDocument == NULL); m_pDocument = pDocument; }

public:
	void SetSqlSession	(SqlSession* pSession)  { m_pSqlSession = pSession; }
	void SetForceQuery	(BOOL bForce = TRUE )	{ m_bForceQuery = bForce;	}

protected:
	virtual void Free ();

	// DEVONO essere implementate dal programmatore
	virtual void OnDefineQuery		() = 0;
	virtual void OnPrepareQuery		() = 0;
	virtual BOOL IsEmptyQuery		() = 0;
	
// diagnostics
#ifdef _DEBUG
public:	
	void Dump(CDumpContext& dc) const {	ASSERT_VALID(this); AFX_DUMP0(dc, " TblReader\n"); }
	void AssertValid() const{ CObject::AssertValid(); }
#endif //_DEBUG
};

/*
	Generico lettore di un insieme di record. Utile per incapsulare lo scorrimento del risultato di una query complessa,
	e riutilizzarlo in più punti.
	È ottimizzato per lo scorrimento solo in avanti, quindi ammette soltanto i metodi IsEOF e MoveNext. Per questo motivo
	perde l'ottimizzazione in caso di ripetizione della stessa query, altrimenti resterebbe "in fondo" al rowset, ed essendo
	solo forward non è ammesso fare "MoveFirst"
*/
//===========================================================================
class TB_EXPORT RowsetReader : public TableReader
{
	DECLARE_DYNAMIC(RowsetReader)
public:
	// constructors
	RowsetReader	(CRuntimeClass* pClass, CBaseDocument* pDocument)
		:
		TableReader	(pClass, pDocument)
	{
		// dovendo estrarre un rowset forza la requery ogni volta, altrimenti a causa dell'ottimizzazione
		// rieseguendo una Find con gli stessi parametri resterebbe in fondo al rowset precedente. Il tipo
		// di cursore solo forward impedisce di fare MoveFirst
		SetForceQuery(TRUE);
		m_bOnlyOneRecord = FALSE; 
	}
	RowsetReader	(const CString& sTableName, CBaseDocument* pDocument)
		:
		TableReader	(sTableName, pDocument)
	{
		// dovendo estrarre un rowset forza la requery ogni volta, altrimenti a causa dell'ottimizzazione
		// rieseguendo una Find con gli stessi parametri resterebbe in fondo al rowset precedente. Il tipo
		// di cursore solo forward impedisce di fare MoveFirst
		SetForceQuery(TRUE);
		m_bOnlyOneRecord = FALSE;
	}


public:
	BOOL IsEOF()		{ return !m_pTable || m_pTable->IsEOF(); }
	void MoveNext()		{ if (m_pTable) m_pTable->MoveNext(); }
};

//===========================================================================
class TB_EXPORT RecordReader : public RowsetReader
{
	DECLARE_DYNAMIC(RecordReader)

	void BuildDirectAccessQuery();

	SqlRecord* m_pParamRec;
	BOOL		m_bMissingKeys;

public:
	// constructors
	RecordReader	(SqlRecord* pRec, CBaseDocument* pDocument)
		:
		RowsetReader	(pRec->GetRuntimeClass(), pDocument),
		m_bMissingKeys	(FALSE),
		m_pParamRec		(NULL)
	{
		ASSERT(pRec);
		m_pParamRec = pRec->Clone();
		
		//se il sqlrecord è dinamico, la createobject non mi valorizza il tablename!
		if (m_pRecord->GetTableName().IsEmpty())
			m_pRecord->SetTableName(pRec->GetTableName());
	}

	virtual ~RecordReader	() { SAFE_DELETE(m_pParamRec); }

	SqlRecord* GetRecord () const { return m_pRecord; }

protected:
	virtual void OnDefineQuery		();

	virtual void OnPrepareQuery		() {}
	virtual BOOL IsEmptyQuery		() { return FALSE; }
};

///////////////////////////////////////////////////////////////////////////////
#include "endh.dex"
