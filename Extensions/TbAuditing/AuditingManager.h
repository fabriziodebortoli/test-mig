
#pragma once

#include <TbNameSolver\TBResourceLocker.h>
#include <TbOleDb\TbExtensionsInterface.h>

#include "beginh.dex"


//=============================================================================
class SqlConnection;
class SqlSession;
class SqlRecord;
class SqlTable;
class CTBNamespace;
class TNamespaces;
class TAuditingRecord;
class DBTMaster;
class CXMLFixedKeyArray;


static const TCHAR szAUDIT[] = _T("AU_");

// serve per gestire la mappa di lookup tra identificatore e namespace basandosi
// sulla tabella AUDIT_Namespaces che viene arricchita ogni volta che si utilizza
// un namespace non inserito
// La mappa viene inizializzata con il contenuto della tabella
// Attraverso il metodo GetID, dato un namespace viene restituito il proprio identificator
// se non é presente nella mappa, viene prima inserito nella tabella (se eventualmente giá presente
// non viene dato errore ma viene considerato il record presente) ed in contemporanea nella
// mappa in memoria.
///////////////////////////////////////////////////////////////////////////////
//								NamespacesLookupMng definition
///////////////////////////////////////////////////////////////////////////////
class TB_EXPORT NamespacesLookupMng : public CTBLockable
{
private:
	TNamespaces*	m_pNamespaceRec;
	SqlTable*		m_pTable;
	SqlSession*		m_pSqlSession;

	CMapStringToOb	m_LookupMap;

public:
	NamespacesLookupMng();
	~NamespacesLookupMng();

private:
	void InitMap();

public:
	virtual LPCSTR  GetObjectName() const { return "AuditingNamespacesLookupMng"; }
	DataLng GetID(CTBNamespace*);
};

///////////////////////////////////////////////////////////////////////////////
/* viene associata ad ogni singola tabella nel catalog entry
   m_pAuditRec: é il sqlrecord necessario x inserire le info di auditing. 
   Costruito a partire dalle tabelle AUDIT_TableName e TableName
   Per la query di inserimento non viene utilizzato nessun cursore, ma viene fatta una 
   query secca non preparata, utilizzando la sessione passata nel metodo TraceOperation
   in modo da inserire il comando di INSERT nell'eventuale transazione legata all'operatione
   da tracciare
*/
///////////////////////////////////////////////////////////////////////////////
//								AuditingManager definition
///////////////////////////////////////////////////////////////////////////////
class TB_EXPORT AuditingManager : public CAuditingMngObj, public CTBLockable
{
	friend class ReportGenerator;

private:
	TAuditingRecord*	m_pAuditRec;
	SqlTable*			m_pAuditTable; // for INSERT command

	//nel caso di una select parziale rischio di non avere a disposizione tutti i campi necessari 
	//per creare il record di tracciatura
	//nel caso effettuo una query per leggere tutto il record
	SqlRecord*	m_pAuxiliaryRec;
	SqlTable*	m_pAuxTable;
	BOOL		m_bAuxQuery;
	CString		m_strTableName;

	
	//utilizzate x la costruzione della query di inserimento nella tabella di auditing
	CStringArray m_ColumnNameArray;
	CString		m_strInsert;
	CString		m_strValues;
	CString		m_strCommandText; //stringa completa Non viene effettuata una query preparata
								// ma una query secca	
	BOOL		m_bValid;	
	

public:
	AuditingManager(LPCTSTR lpszTableName);
	virtual ~AuditingManager();

private:
	BOOL				CreateAuditingRecord	();
	TAuditingRecord*	GetAuditRec();

	DataObj* CreateDataObj			(const CString&, SqlRecord*, BOOL&);
	BOOL	BindRecord				();
	void	AssignValues			(int, SqlTable*);
	void	AddSelect				(SqlTable*, const CString&);
	DataObj* FindValueForColumnName	(const CString&, SqlTable* pTable);


public:
	virtual LPCSTR  GetObjectName() const { return "AuditingManager"; }

	BOOL IsValid() const { return m_bValid; }
	void BindTracedColumns(SqlTable*); //chiamato da SqlTable in fase di costruzione della query 
	
	//chiamate da XTech per l'esportazione
	void PrepareQuery(SqlTable*, DataDate&, DataDate&, int);
	BOOL PrepareDeletedQuery(SqlTable*, DBTMaster*, DataDate&, DataDate&);


	// per la creazione automatica di un report di woorm relativo ai dati di tracciatura del dbtmaster
	// del documento il cui namespace è passato come parametro. Inoltre sono passati l'array contenente
	// le eventuali fixedkey da usare nella rule di estrazione dei dati e il nome dell'utente-oppure allusers
	// per la memorizzazione del report
	CTBNamespace* CreateAuditingReport(CTBNamespace*, CXMLFixedKeyArray*, BOOL bAllUsers, const CString& strUser);

protected:
	virtual void TraceOperation (int, SqlTable*);
};


// gestisce la connessione al database primario utilizzata per le operazioni di 
// auditing e la mappa in memoria di lookup
///////////////////////////////////////////////////////////////////////////////
//								AuditingInterface definition
///////////////////////////////////////////////////////////////////////////////
class TB_EXPORT AuditingInterface : public CObject
{
	DECLARE_DYNCREATE(AuditingInterface)
private:
	SqlConnection*			m_pSqlConnection;	
	NamespacesLookupMng*	m_pNSLookupMng;

public:
	AuditingInterface();
	~AuditingInterface();

public:
	BOOL OpenAuditing();
	BOOL CloseAuditing();	

	SqlConnection*	GetSqlConnection() const { return m_pSqlConnection; }
	DataLng			GetNamespaceID(CTBNamespace* pNamespace) { return m_pNSLookupMng->GetID(pNamespace); }
};


// General Functions
//-----------------------------------------------------------------------------
TB_EXPORT AuditingInterface*	AFXAPI AfxGetAuditingInterface();
TB_EXPORT SqlConnection*		AFXAPI AfxGetAuditingSqlConnection();


#include "endh.dex"
