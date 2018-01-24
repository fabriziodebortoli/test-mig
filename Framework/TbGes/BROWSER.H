
#pragma once

#include <TbOleDb\Sqltable.h>
//includere alla fine degli include del .H
#include "beginh.dex"


class SqlSession;
class SqlTable;
class SqlRecord;
class DBTMaster;

//@@Da parlare con Enrico
// gestione del Browser disconnesso
// Effettuo la query e il rowset lo memorizzo in un array che utilizzo al posto dello rowset stesso
// una volta riempito l'array chiudo il cursore 
// Ogni elemento dell'array é un DataObjArray contenente il valore della chiave del record all i-esima
// posizione
//=============================================================================
/*class CRecordKeySet : DataObjArrayOfArray
{
public:
	enum DirectionType {FORWARDS, BACKWARDS};
	int				m_nCurrIndex;
	DirectionType	m_eDirection;

};*/

//=============================================================================
class TB_EXPORT SqlBrowser : public CObject
{         
	friend class CAbstractFormDoc;

	DECLARE_DYNAMIC(SqlBrowser)

private:
	SqlSession*	m_pSqlSession; //slego il browser dal documento. Avrà una sua propria sessione

	DBTMaster*	m_pDBTMaster;
	SqlTable*	m_pTable;
	SqlRecord*	m_pRecord;
	BOOL		m_bInvalidQuery;
	BOOL		m_bFirstRecord;
	BOOL		m_bLastRecord;
	BOOL		m_bNoRecord;
	BOOL		m_bPingPong;		// Abilita il processo di abilitazione dei bottoni
	BOOL		m_bValid;

	// mi serve quando lavoro con il browse disconnesso
	//CRecordKeySet* m_pRecordKeySet;

private:
	void Initialize			(BOOL bCopyBuffer = TRUE);
	void SetNoRecord		();
	void CheckFirstRecord	();
	void CheckLastRecord	();
	//void LoadRecordKeySet	();

public:
	// constructors
	SqlBrowser (DBTMaster*);
	virtual ~SqlBrowser();

public:
	BOOL CanDoFirstRecord	() const;
	BOOL CanDoPrevRecord	() const;
	BOOL CanDoNextRecord	() const;
	BOOL CanDoLastRecord	() const;

	BOOL FirstRecord	();
	BOOL PrevRecord		();
	BOOL NextRecord		();
	BOOL LastRecord		();

	void SetInvalidQuery	(const BOOL bValidQuery = TRUE);
	BOOL IsEmpty			() const;
	BOOL NoCurrent			() const;
	BOOL Query				(MoveType moveType = E_MOVE_NEXT);

	BOOL		IsValid		() const { return m_bValid; }
	SqlTable*	GetTable	() const { return m_pTable; }
	
	void ReInit (BOOL bCopyBuffer = TRUE);

// diagnostics
#ifdef _DEBUG
public:	
	void Dump(CDumpContext& dc) const {	ASSERT_VALID(this); AFX_DUMP0(dc, " SqlBrowser\n"); }
	void AssertValid() const{ CObject::AssertValid(); }
#endif //_DEBUG
};

// start inlines
//
/////////////////////////////////////////////////////////////////////////////
//
// stop inlines
/////////////////////////////////////////////////////////////////////////////

#include "endh.dex"
