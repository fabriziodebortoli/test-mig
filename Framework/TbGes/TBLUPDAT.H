
#pragma once

#include <TbGeneric\DataObj.h>

#include "tblread.h"
                        
//includere alla fine degli include del .H
#include "beginh.dex"

/*
	Generico lettore e scrittore di singoli record di tabelle, utile  quando e` 
	necessario leggere modificare e scrivere record specifici in transazioni
	accessorie
*/

//===========================================================================
class TB_EXPORT TableUpdater : public CObject
{
	DECLARE_DYNAMIC(TableUpdater)
	
public:
	enum FindResult {NONE, FOUND, NOT_FOUND, LOCKED };

private:	
	// Puntatore alla CMessage esterna alla quale possono essere accodati 
	// eventuali messaggi durante le letture e le scritture del record 
	// nella tabella
	CDiagnostic*	m_pDiagnostic;
	FindResult		m_Status;
	BOOL			m_bForceAutocommit;
 	SqlSession*		m_pSqlSession; // sessione di lavoro del TableUpdater
   
protected:		
	// allocati e disallocati internamente
	SqlRecord*		m_pRecord;
	SqlRecord*		m_pOldRecord;
	SqlTable*		m_pTable;
	CBaseDocument*	m_pDocument;
	BOOL			m_bOnlyOneRecord;
	
public:
	// pClass è il puntatore alla CRuntimeClass del record che viene gestito 
	// dall'TableUpdater viene passato dalle classi derivate nel loro 
	// costruttore poiche` solo in esse si conosce la classe del record da 
	// gestire. pDocument è il puntatore al documento dal quale possono essere
	// effettuate le transazioni accessorie. 
	TableUpdater
		(
			CRuntimeClass*	pClass, 
			CBaseDocument* 	pDocument = NULL, 
			CDiagnostic* 	pDiagnostic = NULL
		);
	TableUpdater
		(
			const CString&	sTableName, 
			CBaseDocument* 	pDocument = NULL, 
			CDiagnostic* 	pDiagnostic = NULL
		);
	 
	virtual ~TableUpdater() { Free(); }

public:	
	// LOCK support
	BOOL	LockCurrent		(BOOL bUseMessageBox = TRUE, LockRetriesMng* pRetriesMng = NULL);
	BOOL	IsCurrentLocked	()	{ return m_pTable && m_pTable->IsCurrentLocked();}
	CString	GetLockMessage	()	{ return (m_pTable) ? m_pTable->GetLockMessage() : _T("");}

	void	UnlockCurrent	()	{ if (m_pTable) m_pTable->UnlockCurrent (); }
	void	UnlockAll		()	{ if (m_pTable) m_pTable->UnlockAll (); }

	void	EnableLocksCache		(const BOOL bValue = TRUE) { if (m_pTable && m_pTable->m_pContext) m_pTable->m_pContext->EnableLocksCache (m_pTable, bValue); }
	void	ClearLocksCache			(SqlTable* pTable, const CString sLockContextKey = _T("")) { if (m_pTable && m_pTable->m_pContext) m_pTable->m_pContext->ClearLocksCache (m_pTable, m_pTable->GetTableName()); }
	BOOL	LockTableKey			(SqlRecord* pRec, LockRetriesMng* pRetriesMng = NULL);
	BOOL	UnlockTableKey			(SqlRecord* pRec) { return m_pTable->m_pContext && m_pTable->m_pContext->UnlockTableKey(m_pTable, pRec); }
	BOOL	UnlockAllLockContextKeys(const CString& sLockContextKey) { return m_pTable->m_pContext && m_pTable->m_pContext->UnlockAllLockContextKeys(sLockContextKey, m_pTable); }

	// obbligo il cursore (associato al tableupdater) in modifica a utilizzare l'autocommit anche se sono in una
	// connessione che prevede l'utilizzo delle transazioni
	// tale istruzione deve essere utilizzata con molta ATTENZIONE									
	void	SetAutocommit(); 

	void	AttachDocument	(CBaseDocument*	pDocument)	{ ASSERT(m_pDocument == NULL); m_pDocument = pDocument; }
	void	SetSqlSession	(SqlSession* pSession);

public:	
	virtual	FindResult	FindRecord		(BOOL bLock = FALSE, LockRetriesMng* pRetriesMng = NULL);
	virtual BOOL		UpdateRecord(BOOL bForceTBModified = FALSE);
	virtual BOOL		DeleteRecord	();
	
protected:	
	virtual void Free ();

	// DEVONO essere implementate dal programmatore
	virtual void OnDefineQuery		() = 0;
	virtual void OnPrepareQuery		() = 0;
	virtual BOOL IsEmptyQuery		() = 0;
	
private:
	BOOL TableCreated();
// diagnostics
#ifdef _DEBUG
public:	
	void Dump(CDumpContext& dc) const {	ASSERT_VALID(this); AFX_DUMP0(dc, " TblUpdater\n"); }
	void AssertValid() const{ CObject::AssertValid(); }
#endif //_DEBUG
};

/*
	Generico lettore e scrittore di gruppi di record di tabelle, utile quando e` 
	necessario leggere modificare e aggiornare insiemi di record risultato di una query
	complessa in elaborazioni varie (es.: batch)
*/
//===========================================================================
class TB_EXPORT RowsetUpdater : public TableUpdater
{
	DECLARE_DYNAMIC(RowsetUpdater)

public:
	RowsetUpdater
		(
			CRuntimeClass*	pClass, 
			CBaseDocument* 	pDocument, 
			CDiagnostic* 	pDiagnostic = NULL
		)
		:
		TableUpdater(pClass, pDocument, pDiagnostic)
	{
		m_bOnlyOneRecord = FALSE;
	}

	RowsetUpdater
		(
			const CString&	sTableName, 
			CBaseDocument* 	pDocument, 
			CDiagnostic* 	pDiagnostic = NULL
		)
		:
		TableUpdater(sTableName, pDocument, pDiagnostic)
	{
		m_bOnlyOneRecord = FALSE;
	}

	//@@TODO Bauzi + Rinaldi cambiare il tipo di cursore per ottimizzazioni performance

public:
	BOOL IsEOF()		{ return !m_pTable || m_pTable->IsEOF(); }
	void MoveNext()		{ if (m_pTable) m_pTable->MoveNext(); *m_pOldRecord = *m_pRecord; }
	void Disconnect()	{ if (m_pTable) m_pTable->Disconnect(); }
};

#include "endh.dex"
                       
