
#include "stdafx.h"

#include <stdlib.h>
#include <afxpriv.h>
#include <direct.h>
#include <sys\stat.h>

#include <TbFrameworkImages\GeneralFunctions.h>
#include <TbFrameworkImages\CommonImages.h>

#include <TbNameSolver\FileSystemFunctions.h>
#include <TbNameSolver\PathFinder.h>

#include <TbGeneric\critical.h>
#include <TbGeneric\globals.h>

#include <TbGenlib\commands.hrc>

#include <TbParser\Parser.h>
#include <TbGenlib\generic.h>

#include <TbOledb\sqltable.h>
#include <TbOledb\sqlcatalog.h>
#include <TbOledb\wclause.h>
#include <TbOledb\sqlconnect.h>
 
#include <TbWoormEngine\ActionsRepEngin.h>
#include <TbWoormEngine\edtcmm.h>
#include <TbWoormEngine\prgdata.h>

#include "TbRadarInterface.h"
#include "hotlink.h"
#include "barquery.h"
#include "barqydlg.h"
#include "barquery.hjson" //JSON AUTOMATIC UPDATE
#include "extdoc.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

// Versioni delle grammatiche con cui sono salvati il file di transcodifica e
// quelli delle query. Se durante la fase di caricamento dei le versioni non
// coincido verrà segnalato all'utente e i vecchi file saranno soprascritti
//-----------------------------------------------------------------------------
static const int nQueriesFileRelease	= 1;
static const int nQueryFileRelease		= 1;


// Estensione del file di transcodifica
//-----------------------------------------------------------------------------
static const TCHAR BASED_CODE szQueryExt [] = _T("qry");

// Definizione dei prefissi per la composizione del nome di una query temporanea
// e per la sua identificazione
//-----------------------------------------------------------------------------
static const TCHAR BASED_CODE szTmpQueryName	[] = _T("Test_{0-%d}");
static const TCHAR BASED_CODE szTmpQueryNamePre[] = _T("Test_");

// Nome di identificazione delle query temporanee di ordinamento (ascendente e
// discendente) costruite automaticamente dalla BarQuery, quando è collegata ad
// un radar, su esplicita richiesta dell'utente ed in base alle colonne
// correntemente selezionate.
//-----------------------------------------------------------------------------
static const TCHAR BASED_CODE szTmpQueryNameDes[] = _T("Test_ZA");
static const TCHAR BASED_CODE szTmpQueryNameAsc[] = _T("Test_AZ");


///////////////////////////////////////////////////////////////////////////////
// Funzioni globali
///////////////////////////////////////////////////////////////////////////////

// Dato il percorso completo del file ritorna la data e l'ora dell'ultima
// modifica
//-----------------------------------------------------------------------------
BOOL GetFileTime (const CString& strFullName, CTime& mtime)
{
    CFileStatus	fileStatus;
	if (CLineFile::GetStatus(strFullName, fileStatus))
	{
		mtime = fileStatus.m_mtime;
		return TRUE;
	}
	return FALSE;
}

// Ritorna TRUE se il nome contiene il prefisso di temporanea, altrimenti FALSE
//-----------------------------------------------------------------------------
BOOL IsTempQueryName (const CString& strQueryName)
	{ return strQueryName.Find (szTmpQueryNamePre) == 0; }

// Ritorna TRUE se il nome coincide con quella della query predefinita,
// altrimenti FALSE
//-----------------------------------------------------------------------------
BOOL IsProgramQueryName (const CString& strQueryName)
	{ return strQueryName.CompareNoCase (GetProgramQueryName()) == 0; }

// Ritorna il nome della query predefinita
//-----------------------------------------------------------------------------
LPCTSTR GetProgramQueryName () { static CString strQueryName = _TB("Default"); return strQueryName;}

//-----------------------------------------------------------------------------
inline int BarQueryMessageBox (const CString& strMsg, LPCTSTR lpszMsg, UINT nStyle)
{
	return AfxMessageBox (cwsprintf (strMsg, lpszMsg), nStyle);
}

///////////////////////////////////////////////////////////////////////////////
// Implementazione di QueryInfo
///////////////////////////////////////////////////////////////////////////////
//
//	Contiene tutte le informazioni necessarie per costruire una query. E' in
// grado di salvare e leggere queste informazioni su file secondo una particolare
// grammatica (vedi metodo QueryInfo::Unparse). Definiremo questo tipo di file
// come 'file della query'. Il suo nome è composto dai primi otto caratteri del
// nome della tabella e per estensione ha un codice numerico univoco.
//	Su file verranno salvate ulteriori specifiche che permetto di eseguire un
// controllo incrociato con il file di transcodifica, in particolare verranno
// ripetuti i nomi della tabella e della query. Se questi nomi non coincidono
// la query è ritenuta non valida e quindi non caricata (errori di questo tipo
// si verifica quando l'utente manomette questi file). La query sarà considerata
// invalida anche nel caso di versioni della grammatica non coincidenti. In
// entrambi i casi l'errore occorso sarà segnalato all'utente.
// 	Questa classe oltre che a gestire query ordinarie, definite utilizzando
// m_strFilter e m_strFilter, gestisce query parametriche.
//	Tale tipologia di query, nel nostro caso, vengono definite quelle che prima
// dell'esecuzione richiedono all'utente di impostare i parametri di estrazione.
//	Esiste una opportuna finestra di dialogo che permette di definire il numero,
// il tipo di parametri che una query deve richiedere.
//
//	m_strFilter		: stringa di filtro;
//
//	m_strOrderBy	: stringa per l'ordinamento;
//
//	m_pPrgData		: questo campo assume significato solo nel caso in cui
//						si definisca una query parametrica.
//
///////////////////////////////////////////////////////////////////////////////

//-----------------------------------------------------------------------------
QueryInfo::QueryInfo
	(
		SqlConnection*		pSqlConnection, 
		BOOL				bNativeExpr, // = FALSE
		SqlTableStruct*		pTableInfo, // = NULL
		ProgramData*		pPrgData	// = NULL
	)
	:
	m_pPrgData			(NULL),
	m_bNativeExpr		(bNativeExpr),
	m_pSqlConnection	(pSqlConnection)
{
	if (pTableInfo)
		m_TableInfo.Assign(*pTableInfo);

	SetProgramData(pPrgData);
}

//-----------------------------------------------------------------------------
QueryInfo::QueryInfo (const QueryInfo& aQueryInfo)
{
	*this = aQueryInfo;
}

// costruisco le queryinfo da una stringa di query
//-----------------------------------------------------------------------------
QueryInfo::QueryInfo 
			(
				SqlConnection*			pSqlConnection,
				const	SqlTableInfoArray&	arTableInfo,					
				const	CString&		strQuery, 
				const	CString&		strTableName,
				const	CString&		strQueryName,
						LPCTSTR			szFromFileName/*=NULL*/
			)
	:
	m_pPrgData			(NULL),
	m_bNativeExpr		(FALSE),
	m_pSqlConnection	(pSqlConnection) 
{
	if (!strQuery.IsEmpty())
		ParseFromQueryString(strQuery, strTableName, strQueryName, arTableInfo, szFromFileName);	
}

//-----------------------------------------------------------------------------
QueryInfo::~QueryInfo ()
{
	ReleaseProgramData();
}

//-----------------------------------------------------------------------------
QueryInfo& QueryInfo::operator = (const QueryInfo& query)
{
	if (this != &query)
	{
		m_bNativeExpr	= query.m_bNativeExpr;
		m_TableInfo.Assign(query.m_TableInfo);
		m_pSqlConnection = query.m_pSqlConnection;
		SetProgramData(query.m_pPrgData);
	}
	return *this;
}

//------------------------------------------------------------------------------
BOOL QueryInfo::operator == (const QueryInfo& query) const
{
	if (this == &query)
		return TRUE;
	
	return
		(
			m_bNativeExpr	== query.m_bNativeExpr	&&
			m_TableInfo.m_strFilter.CompareNoCase(query.m_TableInfo.m_strFilter) == 0 &&
			m_TableInfo.m_strSort.CompareNoCase(query.m_TableInfo.m_strSort) == 0 &&
			m_pPrgData		== query.m_pPrgData	
		);
}

//------------------------------------------------------------------------------
BOOL QueryInfo::operator != (const QueryInfo& query) const
{
	return !(*this == query);
}


// Azzero i valori dei vari campi
//-----------------------------------------------------------------------------
void QueryInfo::ResetAllFields ()
{
	m_TableInfo.m_strSort.Empty();
	m_TableInfo.m_strFilter.Empty();

	ReleaseProgramData();
}

//-----------------------------------------------------------------------------
void QueryInfo::SetProgramData (ProgramData* pPrgData) 
{
	if (m_pPrgData == pPrgData)
		return;
	
	ReleaseProgramData();

	m_pPrgData = pPrgData; 
	
	if (m_pPrgData)
		m_pPrgData->m_nCounter++;
}

//-----------------------------------------------------------------------------
void QueryInfo::ReleaseProgramData ()
{
	if (m_pPrgData && !--m_pPrgData->m_nCounter)
		delete m_pPrgData;
	m_pPrgData = NULL;
}

// Una query viene salvata su file secondo la seguente grammatica:
//
//		RELEASE <versione grammatica>
//		TABLE <nome tabella> QUERY "nome query"
//			BEGIN
//				[blocco relativo alla descrizione della symbol table]
//
//				[WHERE		"Espressione filtro"]
//				[ORDER BY	"Espressione ordinamento"]
//
//				[blocco relativo alla descrizione della ask rule]
//			END
//
//	<versione grammatica>
//					versione della grammatica con cui è stato salvato il file
//					della query (è un valore intero);
//
//	<nome tabella>	identificata la tabella a cui si riferisce la query (case
//					insensitive);
//
//	"nome query"	nome pubblico della query (case insensitive); a differenza
//					del nome della tabella essendo una stringa racchiusa tra doppi
//					apici può contenere spazi bianchi, caratteri non alfanumerici,
//					ecc.;
//
//	[blocco relativo alla descrizione della symbol table]
//					nel caso di query parametrica viene descritta secondo una
//					particolare grammatica la symbol table;
//
//	[SELECT {"column list"}]
//					elenco opzionale colonne da estrarre (serve in caso di join o aggregazioni).
//	[FROM {"tables list"}]
//					opzionale, serve per le join.

//	[WHERE "Espressione Filtro"]
//					espressione della condizioni di estrazione (è presente solo
//					se non è vuoto il relativo campo).
//
//	"Espressione ordinamento"
//					espressione della condizioni di ordinamento (è presente solo
//					se non è vuoto il relativo campo).
//
//	[blocco relativo alla descrizione della ask rule]
//					nel caso di query parametrica viene descritta secondo una
//					particolare grammatica la ask rule;
//
//-----------------------------------------------------------------------------
void QueryInfo::GenericUnparse(Unparser& unparser, const CString& strTableName, const CString& strQueryName)
{
	// Salvataggio dell'attuale versione della grammatica
	unparser.UnparseTag	(T_RELEASE, FALSE);
	unparser.UnparseInt	(nQueryFileRelease);

	// Salvataggio del nome della tabella a della query
	unparser.UnparseTag		(T_TABLE,		FALSE);
	unparser.UnparseID		(strTableName,	FALSE);
	unparser.UnparseTag		(T_QUERY,		FALSE);
	unparser.UnparseString	(strQueryName);
	unparser.UnparseBegin	();

	if (m_pPrgData)
	{
		// E' una query parametrica devo unparsare la symbol table
		ASSERT (m_pPrgData->GetSymTable()->GetSize() >= 0);
		m_pPrgData->GetSymTable()->Unparse(unparser);
	}

	if (0 && !m_TableInfo.m_strSelect.IsEmpty())
	{
		unparser.UnparseTag	(T_SELECT,					FALSE);
		unparser.UnparseTag	(T_BRACEOPEN,				FALSE);
		unparser.Write		(m_TableInfo.m_strSelect,	FALSE); 
		unparser.UnparseBlank (FALSE);
		unparser.UnparseTag	(T_BRACECLOSE);
	}

	if (0 && !m_TableInfo.m_strFrom.IsEmpty())
	{
		unparser.UnparseTag	(T_FROM,					FALSE);
		unparser.UnparseTag	(T_BRACEOPEN,				FALSE);
		unparser.Write		(m_TableInfo.m_strFrom,		FALSE); 
		unparser.UnparseBlank (FALSE);
		unparser.UnparseTag	(T_BRACECLOSE);
	}

	// Se la stringa della Where Clause è vuota non scrivo niente
	if (!m_TableInfo.m_strFilter.IsEmpty())
	{
		unparser.UnparseTag	(T_WHERE,		FALSE);

		if (m_bNativeExpr)
		{
			m_TableInfo.m_strFilter.Trim();
			if (::FindWord(m_TableInfo.m_strFilter, cwsprintf(T_NATIVE)) < 0)
				unparser.UnparseTag	(T_NATIVE,	FALSE);	
		}	
		unparser.UnparseExpr(m_TableInfo.m_strFilter,	FALSE);

		unparser.UnparseSep	();
		unparser.UnparseCrLf();
	}

	// Se la stringa dell'Order è vuota non scrivo niente
	if (!m_TableInfo.m_strSort.IsEmpty())
	{
		unparser.UnparseTag	(T_ORDER,		FALSE);
		unparser.UnparseTag	(T_BY,			FALSE);
		unparser.UnparseExpr(m_TableInfo.m_strSort,	FALSE);
		unparser.UnparseSep	();
		unparser.UnparseCrLf();
	}

 	// E' una query parametrica devo unparsare la dialog di richiesta dei dati
 	if (m_pPrgData)
		m_pPrgData->GetAskRuleData()->Unparse(unparser);

	unparser.UnparseEnd ();	
}

//-----------------------------------------------------------------------------
BOOL QueryInfo::Unparse(const CString& strFullName, const CString& strTableName, const CString& strQueryName)
{
	Unparser unparser;
	if (!unparser.Open(strFullName))
		return FALSE;
	unparser.SetFormat(CLineFile::UTF8);

	GenericUnparse(unparser, strTableName, strQueryName);

	unparser.Close();
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL QueryInfo::UnparseInString(CString& strQuery, const CString& strTableName, const CString& strQueryName)
{
	// sto utilizzando un memfile
	Unparser unparser(TRUE);

	GenericUnparse(unparser, strTableName, strQueryName);
	strQuery = unparser.GetBufferString();
	
	unparser.Close();
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL QueryInfo::GenericParse
	(
				Parser&			lex,
		const	CString&		strTableName,
		const	CString&		strQueryName,
		const	SqlTableInfoArray& arTableInfo,
				LPCTSTR			szFromFileName
	)
{
	int nRelease;
	if (!lex.Match (T_RELEASE) || !lex.ParseInt (nRelease))
		return FALSE;

	// Controllo dell'attuale versione della grammatica
	if (nRelease != nQueryFileRelease)
		return FALSE;

	// Lettura e controllo della corrispondenza tra il contenuto del file di
	// transcodifica e il file della query. Il controllo viene effettuato sul
	// nome della tabella e della query
	CString strBuffer;
	if (!lex.Match (T_TABLE) || !lex.ParseID (strBuffer))
		return FALSE;

	if (strBuffer.CompareNoCase (strTableName))
	{
		// Il nome della tabella non coincidono
		CString str = cwsprintf (
									_TB("Unable to load the query '{0-%s}'.\r\nThe table names in the file '{1-%s}' are not aligned with the transcoding file, respectively {2-%s} and {3-%s}."),
									(LPCTSTR) strQueryName,
									szFromFileName,
									(LPCTSTR) strBuffer,
									(LPCTSTR) strTableName
								);
		AfxMessageBox (str, MB_OK | MB_ICONSTOP);
		return FALSE;
	}

	if (!lex.Match (T_QUERY) || !lex.ParseString (strBuffer))
		return FALSE;

	if (strBuffer.CompareNoCase (strQueryName))
	{
		// Il nome della query non coincidono
		CString str = cwsprintf (
									_TB("Unable to load the query '{0-%s}'.\r\nThe file '{1-%s}' is not aligned with the transcoding file, as regards query names, and is named: {2-%s}."),
									(LPCTSTR) strQueryName,
									szFromFileName,
									(LPCTSTR) strBuffer
								);
		AfxMessageBox (str, MB_OK | MB_ICONSTOP);
		return FALSE;
	}

	if (!lex.ParseBegin ())
		return FALSE;

	// Verifico se è una query parametrica
	if (lex.LookAhead(T_VAR))
	{
		ReleaseProgramData();
		// Carico le informazioni relative al blocco della symbol table
		ProgramData* pPrgData = new ProgramData(NULL);
		if (!pPrgData->GetSymTable()->Parse(lex))
		{
			delete pPrgData;
			return FALSE;
		}
		SetProgramData(pPrgData);
	}

	if (lex.Matched(T_SELECT))
	{
		lex.Match(T_BRACEOPEN);

		lex.EnableAuditString();

		lex.SkipToToken(T_BRACECLOSE);

		m_TableInfo.m_strSelect = lex.GetAuditString();
		lex.EnableAuditString(FALSE);

		lex.Match(T_BRACECLOSE);
	}
	if (lex.Matched(T_FROM))
	{
		lex.Match(T_BRACEOPEN);

		lex.EnableAuditString();

		lex.SkipToToken(T_BRACECLOSE);

		m_TableInfo.m_strFrom = lex.GetAuditString();
		lex.EnableAuditString(FALSE);

		lex.Match(T_BRACECLOSE);
	}

	ASSERT (arTableInfo.GetSize());
	WClause tmpWClause(m_pSqlConnection, m_pPrgData ? m_pPrgData->GetSymTable() : NULL, arTableInfo);

	// Parse della stringa di where clause
	if (lex.LookAhead(T_WHERE))
	{
		lex.SkipToken ();
		if (!tmpWClause.Parse(lex) || !lex.ParseSep())
			return FALSE;

		m_TableInfo.m_strFilter = tmpWClause.ToString ();
	}

	m_bNativeExpr = tmpWClause.IsNative();

	// Parse della stringa di filter
	if (lex.LookAhead(T_ORDER))
	{
		lex.SkipToken ();

		if (!lex.ParseTag(T_BY) ||
			 !ParseOrderBy(lex, m_pPrgData ? m_pPrgData->GetSymTable() : NULL,
							 &arTableInfo, m_TableInfo.m_strSort) || !lex.ParseSep())
			return FALSE;
	}
 	// E' una query parametrica devo unparsare la dialog di richiesta dei dati
	return m_pPrgData && !m_pPrgData->GetAskRuleData()->Parse(lex, NULL) ? FALSE : lex.ParseEnd ();
}

// Parse della query. A questo livello non si conosce il nome del file su cui
// è memorizzata la query ed infatti viene passato dall'esterno. Il nome della
// tabella e della query servono per realizzare il controllo sopra descritto
// mentre la struttura SqlTableInfo è necessaria per eseguire il controllo
// sintattico e di congruenza della query con la tabella in uso.
//-----------------------------------------------------------------------------
BOOL QueryInfo::Parse
	(
		const	CString&		strFullName,
		const	CString&		strTableName,
		const	CString&		strQueryName,
		const SqlTableInfoArray& arTableInfo
	)
{
	Parser lex;
	if (!lex.Open (strFullName))
		return FALSE;

	return GenericParse(lex, strTableName, strQueryName, arTableInfo, (LPCTSTR)strFullName);

}

// devo consentire anche di parsare la query a partire da una stringa
//-----------------------------------------------------------------------------
BOOL QueryInfo::ParseFromQueryString
	(
		LPCTSTR	szQueryString,
		const	CString&		strTableName,
		const	CString&		strQueryName,
		const	SqlTableInfoArray&	arTableInfo,
		LPCTSTR	szFromFileName	/*=NULL*/
	)
{
	Parser lex(szQueryString);
	return GenericParse(lex, strTableName, strQueryName, arTableInfo, szFromFileName);
}


///////////////////////////////////////////////////////////////////////////////
// Diagnostics
#ifdef _DEBUG
void QueryInfo::Dump (CDumpContext& dc) const
{
	ASSERT_VALID (this);
	dc << " QueryInfo\n\tOrderBy: " << m_TableInfo.m_strSort << ";\n\tWClause: " <<  m_TableInfo.m_strFilter << ";\n";
	CObject::Dump(dc);
}
#endif // _DEBUG

///////////////////////////////////////////////////////////////////////////////
// Implementazione di QueryObj
///////////////////////////////////////////////////////////////////////////////
//
//	Si occupa della memorizzazione, della gestione e della rappresentazione a
// video di una singola query. E' in grado di salvare la query sul relativo file
// ed aggiorna il file di transcodifica.
//	Il file di transcodifica non è altro che una tabella di corrispondenze tra
// il nome della query e il file dove è contenuta, inoltre viene anche indicato
// il nome dalla tabella a cui fa riferimento. Questo ulteriore indicazione è
// necessaria per risolvere il seguente problema.
//	Il nome di un file in ambiente DOS e ridotto a 8 caratteri, ma i nomi delle
// tabelle sono molto più lunghi, per cui eseguendo un troncamento di questo
// tipo alcuni nomi possono collidere. Per evitare di imporre nuovi nomi univoci
// alle tabelle nei primi otto caratteri si è deciso di far coesistere su uno
// stesso file query appartenenti a tabelle differenti (vedi ?????).
//	In memoria saranno quindi necessariamente presenti anche query che non
// appartengono alla tabella corrente in modo da potere ricostruire in maniera
// esatta e senza nessuna perdita di informazioni il file di transcodifica.
//	Per meglio distinguere le varie tipologie delle query è stato introdotto un
// data member m_enumStatus, di tipo enumerativo, che può assumere i seguenti
// valori:
//
//		Hidden		: non appartiene alla tabella in uso (non è possibile
//						svolgere su di essa alcun tipo di operazione) ed è
//						del tutto nascosta all'utente;
//		HiddenSelect: come sopra, ma è la query di default la tabella indicata);
//		Active      : fanno riferimento alla tabella corrente e possono essere
//						eseguite e/o modificate e passate da un documento ad
//						un altro (tramite i file);
//		Temp        : come sopra, ma temporanea; esse sono locali e presenti
//						solo nella corrente sessione di lavoro, saranno
//						definitivamente perse a chiusura del documento;
//		Predefinite	: come sopra, ma è definita dal programmatore (cablata) e non
//						è possibile modificarla);
//
//	Le query Hidden e HiddenSelect verranno riscritti solo i valori de file di
// transcodifica ogni volta che sarà necessario il suo aggiornamento. Le query
// Active possono essere salvate sul file delle query e se necessario aggiornare
// quello di transcodifica. Le ultime due tipologie di query non verranno mai
// salvate su file.
//	Visto che le query possono essere modificate da più documenti per avere
// subito a disposizione e/o per essere aggiornati con le nuove informazioni,
// ogni volta che si cerca di accedere ad una query si controlla la data e l'ora
// dell'ultima modifica sia del file di transcodifica che quella della query.
// Se non coincido si provvederà al caricamento dei nuovi valori.
//	Il file della query, come quello di transcodifica, ha il nome composto da i
// primi 8 caratteri della tabella mentre come estensione ha un codice numerico
// univoco ed in genere anche progressivo. Tale indice viene utilizzato per
// ordinare in memoria le varie query, in questo modo risulta più agevole
// trovare gli eventuali buchi nella numerazione ed assegnarli nuovamente.
//	Come vedremo in seguito le informazioni per l'esecuzione delle query non
// vengono caricate tutte all'inizio, ma solo quando servono, per cui inizialmente
// il m_pQueryInfo sarà inizializzato per tutte le query a NULL eccetto per
// quella predefinita e per quella di default (se è diversa da quella
// predefinita).
//
//	NOTA: per motivi di tempo e di semplicità di implementazione ho deciso
//			di non avere una classe per ogni singola tipologia di query
//			(scelta forse più logica).
//
//	m_enumType		: identifica la tipologia della query;
//
//	m_strTableName	: nome della tabella a cui appartiene;
//
//	m_strQueryName	: nome con cui viene identificata la query; è utilizzato
//						per la sua rappresentazione a video;
//
//	m_nExtIndex		: numero che identifica l'estensione del file;
//
//	m_pQueryInfo	: informazioni relative all'esecuzione della query;
//
//	m_mtime			: data del file della query all'ultima lettura eseguita;
//						è utilizzata solo ed esclusivamente per le query di
//						tipo Active;
//
///////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
QueryObj::QueryObj
	(
				SqlConnection*	pConnection,
		const	CString&		strTableName,
		const	CString&		strQueryName,
				int				nExtIdx,
				enumQueryType	enumStatus
	)
	:
	m_pSqlConnection(pConnection),
	m_enumType		(enumStatus),
	m_strTableName	(strTableName),
	m_strQueryName	(strQueryName),
	m_nExtIndex		(nExtIdx),
	m_pQueryInfo	(NULL)
{
	m_mtime = CTime (1980, 1, 1, 0, 0, 0);
}

//-----------------------------------------------------------------------------
QueryObj::QueryObj
	(
			SqlConnection*	pConnection,	
		const	CString&	strTableName,
		const	CString&	strQueryName,
				int			nExtIdx,
				QueryInfo*	pQueryInfo	// = NULL
	)
	:
	m_pSqlConnection(pConnection),
	m_enumType		(Active),
	m_strTableName	(strTableName),
	m_strQueryName	(strQueryName),
	m_nExtIndex		(nExtIdx),
	m_pQueryInfo	(pQueryInfo)
{
	m_mtime = CTime (1980, 1, 1, 0, 0, 0);
}

// Costruttore usato solo per definire la query predefinita
//-----------------------------------------------------------------------------
QueryObj::QueryObj(QueryInfo* pQueryInfo)
	:
	m_enumType		(Predefinite),
	m_nExtIndex		(-1),
	m_strTableName	(""),
	m_strQueryName	(GetProgramQueryName()),
	m_pQueryInfo	(pQueryInfo),
	m_pSqlConnection(NULL)
{
	ASSERT_VALID (m_pQueryInfo);
	m_pSqlConnection = m_pQueryInfo->m_pSqlConnection;
	m_mtime = CTime (1980, 1, 1, 0, 0, 0);
}

// Costruttore usato solo per definire le query temporanee
//-----------------------------------------------------------------------------
QueryObj::QueryObj (const CString& strQueryName, QueryInfo* pQueryInfo)
	:
	m_enumType		(Temp),
	m_nExtIndex		(-1),
	m_strTableName	(""),
	m_strQueryName	(strQueryName),
	m_pQueryInfo	(pQueryInfo),
	m_pSqlConnection(NULL)
{
	ASSERT_VALID (m_pQueryInfo);
	m_pSqlConnection = m_pQueryInfo->m_pSqlConnection;
	m_mtime = CTime (1980, 1, 1, 0, 0, 0);
}

//-----------------------------------------------------------------------------
QueryObj::~QueryObj ()
{
	if (m_pQueryInfo) delete m_pQueryInfo;
}

// Ritorna la struttura contenente le informazione per l'esecuzione della query,
// mentre in caso di errore NULL.
// A secondo della tipologia della query si ha:
//		- temp o predefinita: le informazioni sono presenti solo in memoria
//			e per come sono state implementate sono sempre presenti e non
//			possono essere aggiornati da altri documenti;
//		- da file: la query è già presente nella struttura in memoria allora
//			controllo la data e l'ora del file e se i due valori non coincidono
//			carico le nuove informazioni, altrimenti carico direttamente il
//			file.
//-----------------------------------------------------------------------------
QueryInfo* QueryObj::GetQueryInfo (const CString& strQueryDir, const CString& strFileName, const SqlTableInfoArray& arTableInfo)
{
	ASSERT (m_enumType != Hidden && m_enumType != HiddenSelect);
	if (m_enumType == Predefinite || m_enumType == Temp)
	{
		// Query predefinita o temporanea esiste sicuramente in memoria
		ASSERT_VALID (m_pQueryInfo);
		return m_pQueryInfo;
	}

	// Verifico esistenza del file relativo alla query
	//CString strFullName = MakeFilePath (strQueryDir, arTableInfo.GetAt(0)->GetTableName().Left (8), cwsprintf (_T("%d"), m_nExtIndex));
	CString strFullName = MakeFilePath (strQueryDir, strFileName, cwsprintf (_T("%d"), m_nExtIndex));
	if (!ExistFile (strFullName))
		return NULL;

	// Salvo la data e l'ora dell'ultima modifica del file in esame
	CTime mtime; GetFileTime (strFullName, mtime);
	if (!m_pQueryInfo)
	{
		ASSERT (m_enumType != Predefinite && m_enumType != Temp);
		m_pQueryInfo = new QueryInfo (m_pSqlConnection, FALSE);
	}
	else if (m_mtime >= mtime)
		return m_pQueryInfo; // il file non è stato aggiornato da nessuno

	// Se non riesco a caricare le caratteristiche della query devo cancellare
	// l'oggetto creato
	if (!m_pQueryInfo->Parse (strFullName, m_strTableName, m_strQueryName, arTableInfo))
	{
		delete m_pQueryInfo;
		m_pQueryInfo = NULL;
	}

	m_mtime = mtime;
	return m_pQueryInfo;
}

// Unparse di una singola riga del file di transcodifica. Ogni riga del file
// identifica in maniera univoca una query.
//-----------------------------------------------------------------------------
void QueryObj::Unparse(Unparser& unparser, BOOL bSel)
{
	unparser.UnparseTag		(T_TABLE,		FALSE);
	unparser.UnparseID		(m_strTableName,FALSE);
	unparser.UnparseTag		(T_QUERY,		FALSE);
	unparser.UnparseString	(m_strQueryName,FALSE);
	unparser.UnparseTag		(T_EXTENSION,	FALSE);
	unparser.UnparseInt		(m_nExtIndex,	FALSE);
	unparser.UnparseBlank	(FALSE);

	// In base allo stato devo:
	// - appartiene alla tabella in esame:
	//		devo controllare se è se selezionata;.
	// - non appartiene alla tabella in esame:
	//		a) se selezionata		=>	salvare la selezione;
	//		b) se non selezionata	=>	niente;
	switch (m_enumType)
	{
		case Active			: if (!bSel) break;
		case HiddenSelect	: unparser.UnparseTag (T_SELECT); return;
	}
	unparser.UnparseCrLf ();
}

// Salva le informazioni relative alla query
//-----------------------------------------------------------------------------
void QueryObj::SaveQuery (const CString& strQueryDir, const CString& strQueryFile)
{
	CString strFile (MakeFilePath (strQueryDir, strQueryFile, cwsprintf (_T("%d"), m_nExtIndex)));
	ASSERT (!strFile.IsEmpty ());
	m_pQueryInfo->Unparse (strFile, m_strTableName, m_strQueryName);
	// Aggiornamento della data e ora di ultima modifica
	GetFileTime (strFile, m_mtime);
}

// Rimuove il file su cui è memorizzata la query
//-----------------------------------------------------------------------------
int QueryObj::RemoveFile (const CString& strQueryDir, const CString& strQueryFile)
{
	return DeleteFile (MakeFilePath (strQueryDir, strQueryFile, cwsprintf (_T("%d"), m_nExtIndex)));
}

///////////////////////////////////////////////////////////////////////////////
// Diagnostics
#ifdef _DEBUG
void QueryObj::Dump (CDumpContext& dc) const
{
	ASSERT_VALID (this);
	dc << " QueryObj\n\tNome tabella: " << m_strTableName << "\n\tQuery: " << m_strQueryName << " \n";
	CObject::Dump(dc);
}
#endif // _DEBUG

///////////////////////////////////////////////////////////////////////////////
// Implementazione di Queries
///////////////////////////////////////////////////////////////////////////////

///////////////////////////////////////////////////////////////////////////////
// Diagnostics
#ifdef _DEBUG
void Queries::Dump (CDumpContext& dc) const
{
	ASSERT_VALID (this);
	TRACE0 (" Queries\n");
	Array::Dump(dc);
}
#endif // _DEBUG

///////////////////////////////////////////////////////////////////////////////
// Implementazione di TempQueries
///////////////////////////////////////////////////////////////////////////////
//
//	La memorizzazione delle query temporanee avviene in una struttura del tutto
// autonoma rispetto a quelle provenienti da file. Le motivazioni che hanno
// indotto a optare per questa scelta sono state:
//
//		- se il file di transcodifica è stato modificato occorre ricaricare
//			nuovamente le query, in questo modo si svuota solo la relativa
//			struttura lasciando inalterata quella delle temporanee;
//		- quando si trasforma una query in definitiva o quando se ne inserisce
///			una nuova inserita occorre assegnare il valore dell'estensione che
//			identifica in maniera univoca il file; per semplicità di gestione
//			l'array delle query definitive è ordinato in base a questo valore 
//			(in questo modo non si hanno duplicazioni estensioni ed è facile
//			individuare i buchi nella numerazione). Se in tale struttura sono
//			presenti query temporanee il meccanismo si complica;
//		- le query sono sempre presenti in memoria sino a quando rimane aperto
//			il documento a cui sono collegate;
//
//	E' possibile trasformare un query temporanea in definitiva e viceversa. In
// entrambi i casi, a parte inizializzare i campi opportuni per accogliere il
// nuovo stato, l'oggetto query sarà spostato da una struttura a l'altra
// semplicemente eliminando il relativo entry.
//	Ad ogni nuova query che l'utente crea viene assegnato un nome temporaneo,
// univoco all'interno della corrente sessione di lavoro, composto dal prefisso
// szTmpQueryName concatenato con un valore numerico progressivo. Se l'utente non
// modifica questo nome la query viene aggiunta come query temporanea.
//	Ne consegue che tutte le query aggiunte senza aver modificato il nome assegnato
// per default non saranno assolutamente salvate su file.
// 
// m_nLastIdxTmpQuery	: indica l'indice da concatenare al prefisso per
//							comporre un nome da assegnare ad una nuova query;
//
///////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
TempQueries::TempQueries ()
	:
	m_nLastIdxTmpQuery (0)
{
}

// Rimuove la query identificata dal relativo puntatore. E' possibile indicare
// se l'oggetto deve essere anche cancellato fisicamente. La sola cancellazione
// dell'entry viene effettuata quando trasformo una query temporanea in
// definitiva
//-----------------------------------------------------------------------------
void TempQueries::DeleteQuery (QueryObj* pQueryObj, BOOL bOwn /* TRUE */)
{
	ASSERT_VALID (pQueryObj);
	for (int nIdx = 0; nIdx < GetSize(); nIdx++)
		if (GetAt (nIdx) == pQueryObj)
		{
			SetOwns (bOwn);
			RemoveAt(nIdx);
			SetOwns (TRUE);
			break;
		}
}

// Ritorna il nome da assegnare ad un nuova query
//-----------------------------------------------------------------------------
CString TempQueries::GetTempQueryName ()
{
	return cwsprintf (szTmpQueryName, m_nLastIdxTmpQuery++);
}

// Riempie la lista con le query attualmente memorizzate
//-----------------------------------------------------------------------------
void TempQueries::FillList (CQueriesListBox* pList)
{
	ASSERT_VALID (pList);
	// Aggiungo tutte le query della tabella 
	for (int nIdx = 0; nIdx < GetSize(); nIdx++)
		pList->AddQuery (GetAt (nIdx));
}

// Se la query identificata dal nome passato come parametro esiste, ritorna la
// sua posizione altrimenti -1
//-----------------------------------------------------------------------------
int TempQueries::IsExistQuery (const CString& strQuery)
{
	for (int nIdx = 0; nIdx < GetSize(); nIdx++)
		if (strQuery.CompareNoCase (GetAt (nIdx)->m_strQueryName) == 0)
			return nIdx;
	return -1;
}

///////////////////////////////////////////////////////////////////////////////
// Diagnostics
#ifdef _DEBUG
void TempQueries::Dump (CDumpContext& dc) const
{
	ASSERT_VALID (this);
	TRACE (" TempQueries\n");
	Queries::Dump(dc);
}
#endif // _DEBUG

///////////////////////////////////////////////////////////////////////////////
// Implementazione di AllQueries
///////////////////////////////////////////////////////////////////////////////
//
//	Classe in grado di memorizzare e gestire tutte le query utilizzate dalla
// tabella corrente.
//	La struttura viene attivata ed inizializzata quando la BarQuery viene
// collegata al documento (vedi CBarQuery::Attach e CQueryManager::Attach) in 
// questa fase si costruisce la query predefinita, prelevando le informazioni
// necessarie dalla tabella correntemente in uso, si carica il file di
// transcodifica e se è definita anche il file della query di default. La
// query predefinita è memorizzata in una struttura completamente indipendente
// sia dalle temporanee che dalle definitive, è sempre presente ed è sicuramente
// corretta ed eseguibile.
//	Il caricamento delle altre query presenti nel file di transcodifica avviene
// quando sono eseguite oppure quando si entra in modifica. Se sono già presenti
// in memoria viene eseguito un controllo sulla data e l'ora dei file per
// l'eventuale caricamento di quelle modificate.
//	In base al meccanismo che chiunque può cancellare e rinominare una query
// può succedere di utilizzare una query non più esistente. In tal caso appena
// si cerca di eseguire una qualsiasi azione su di essa viene segnalato all'utente
// la nuova situazione venutasi a creare e la nuova query selezionata diventa
// quella di default. Il problema si può nuovamente presentare in quanto quella
// di default potrebbe essere anch'essa non più valida, per cui quella
// selezionata diventa quella predefinita (solo nel caso query di default
// diversa da quella predefinita).
//	Come già scritto in precedenza, in memoria sono contenute anche query che
// con la tabella corrente non hanno niente a che fare, per cui è eseguito un
// filtraggio in modo da far vedere al mondo esterno solo le tabelle che
// interessano.
//	Gestisce l'algoritmo per l'assegnazione delle estensioni numeriche ai file
// delle nuove query, cercando di trovare i buchi, se presenti, nella loro
// numerazione. E' per questo motivo che la struttura delle query definitive
// è ordinata secondo questi indici. Se non esiste nessun buco si utilizza il
// primo indice disponibile (si aggiunge in coda).
//	Se la BarQuery è collegata ad un radar è in grado di costruire in maniera
// del tutto automatica le query di ordinamento ascendente e discendente in base
// al nome delle colonne attualmente selezionate. Nella BarQuery del radar
// esistono due pulsanti, attivati solo quando esiste almeno una colonna è
// selezionata, preposti a questo scopo. Le query cosi create vengono inserite
// tra quelle temporanee ed aggiornate ad ogni cambiamento di selezione. Esse
// possono essere modificate dall'utente e se l'utente non utilizza un nome
// diverso vengono sovrascritte appena si esegue un'operazione di ordinamento.
//	Nel caso in cui si utilizzi come query di default una query parametrica,
// quando il documento viene aperto non richiede l'imputazione di nessun
// parametro ma utilizza quelli di default impostati in fase di costruzione 
// della stessa (si verifica solo alla partenza).
//	Oltre al concetti di query predefinita e di default viene definita con il
// termine di query correntemente selezionata e memorizzata nel relativo data
// member, la query il cui nome viene visualizzato nel bottone della bar query.
//
//	m_pTempQueries		: array delle query temporanee; viene costruito in fase
//							di Attach ed è sempre presente sino alla chiusura
//							del documento;
//
//	m_pTable			: tabella su cui occorre eseguire le query;
//
//	m_pTableInfo		: informazioni sulla tabelle per la composizione della
//							query;
//
//	m_pPredefiniteQuery	: struttura di memorizzazione della query predefinita;
//							è sempre presente ed eseguibile, ma non può essere
//							assolutamente modificata o cancellata;
//
//	m_pDefaultQuery		: puntatore alla struttura contenente le informazioni
//							della query di default; se l'utente non ne definisce
//							una coincide con quella predefinita; la struttura a
//							cui punta non è assolutamente di suo proprietà;
//
//	m_pSelectedQuery	: puntatore alla struttura contenente le informazioni
//							della query attualmente selezionata il cui nome
//							viene visualizzato nel bottone; la struttura a cui
//							punta non è assolutamente di suo proprietà;
//
//	m_pSortAsc			: puntatore alla struttura contenente le informazioni
//							relative alla query di ordinamento ascendente in
//							base alle colonne selezionate nel Radar;
//							USATO SOLO DAL RADAR;
//							
//	m_pSortDesc			: come sopra ma con ordinamento discendente;
//
//	m_mtime				: data e ora del file di transcodifica all'ultima lettura
//							eseguita;
//
///////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
AllQueries::AllQueries ()
	:
	m_pTempQueries		(NULL),
	m_pTable			(NULL),
	m_pPredefiniteQuery	(NULL),
	m_pSortAsc			(NULL),
	m_pSortDesc			(NULL),
	m_pSelectedQuery	(NULL),
	m_pDefaultQuery		(NULL),
	m_pSqlConnection	(NULL)
{
	// Inizializzazione del tempo
	m_mtime = CTime (1980, 1, 1, 0, 0, 0);
}

//-----------------------------------------------------------------------------
AllQueries::~AllQueries ()
{
	SAFE_DELETE(m_pTempQueries);
	SAFE_DELETE(m_pPredefiniteQuery);
	SAFE_DELETE(m_pSortAsc);
	SAFE_DELETE(m_pSortDesc);
}


//-----------------------------------------------------------------------------
void AllQueries::SetCustomizeNamespace (const CTBNamespace& aNs)
{ 
	m_nsCustomize = aNs; 
}

//	Legge dal file di transcodifica tutte le query in esso presenti, inserendole
// nella struttura in memoria e settando il relativo tipo. La sintassi con cui
// tale file viene parsato è il seguente:
//
//		RELEASE <versione grammatica>
//		BEGIN
//			TABLE <nome tabella> QUERY "nome query" EXTENSION <Numero Query> [SELECT]
//					.						.
//					.						.
//		END
//	
//	Analizzando più in dettaglio ogni singolo campo:
//	<nome tabella>	identificata la tabella a cui si riferisce la query (case
//					insensitive);
//
//	"nome query"	nome pubblico della query (case insensitive); a differenza
//					del nome della tabella essendo una stringa racchiusa tra doppi
//					apici può contenere spazi bianchi, caratteri non alfanumerici,
//					ecc.;
//
//	<Numero Query>	estensione numerica del file su cui è memorizzata la query;
//
//	SELECT			indica la query di default; se nessuna query, relativa alla
//					tabella corrente, non contiene questo token	allora si
//					utilizza come query di default quella predefinita;
//
//-----------------------------------------------------------------------------
BOOL AllQueries::LoadQueries ()
{
	// Controllo l'esistenza del file di transcodifica, se non esiste => non ci
	// sono query predefinite per la tabella
	CString strQueryFile = GetQueryFileFullPathName ();
	if (!ExistFile (strQueryFile))
		return TRUE;

	// Memorizzo la data e l'ora del file per potere tenere conto di eventuali
	// modifiche
	VERIFY (GetFileTime (strQueryFile, m_mtime));

	// Rimuovo tutte le query definitive
	RemoveAll ();

	// Verifica della versione del file di transcodifica
	Parser lex;
	int nRelease;
	if (!lex.Open (strQueryFile) || !lex.Match (T_RELEASE) || !lex.ParseInt (nRelease))
		return FALSE;

	if (nRelease != nQueriesFileRelease)
		return FALSE;

	if (!lex.Match (T_BEGIN))
		return FALSE;

	CString strQueryAlias;
	CString strTableName;
	CString strFileExt;
	CString strTable (GetTableName ());
	while (!lex.LookAhead(T_EOF) && !lex.LookAhead(T_END) && !lex.Eof())
	{
		 if (lex.Bad ())
		 	return FALSE;

		// Tabella a cui appartiene la query
		if (!lex.Match (T_TABLE) || !lex.ParseID (strTableName))
			return FALSE;

		// Nome della query
		if (!lex.Match (T_QUERY) || !lex.ParseString (strQueryAlias))
			return FALSE;

		// Indice dell'estensione del file
		int nExtIndex;
		if (!lex.Match (T_EXTENSION) || !lex.ParseInt (nExtIndex))
			return FALSE;

		// Controllo se per caso l'utente ha impostato una query di default
		// diversa da quella predefinita. Se trovo una query selezionata ma
		// non relativa alla tabella in esame devo memorizzare questo stato
		// per ripristinarlo nella fase di unparse
		enumQueryType	status	= strTable.CompareNoCase (strTableName) == 0 ? Active : Hidden;
		BOOL			bSelect	= lex.LookAhead(T_SELECT);
		if (bSelect)
		{
			lex.SkipToken ();
			if (status == Hidden)
				status = HiddenSelect;
        }
		// Costruzione dell'array ordinato in base all'estensione del file
		int nIdxToInsert = GetNewPos (0, GetUpperBound (), nExtIndex);

		// Se l'indice è già presente nell'array lo ignoro e passo alla query
		// successiva
		if (nIdxToInsert < 0)
			continue;

		InsertAt (nIdxToInsert, strTableName, strQueryAlias, nExtIndex, status);
		// Selezione della query di default per questa tabella
		if (bSelect && status == Active)
			m_pDefaultQuery = GetAt (nIdxToInsert);
	}
	return TRUE;
}

// Unparse del file di transcodifica
//-----------------------------------------------------------------------------
BOOL AllQueries::Unparse ()
{
	Unparser unparser;
	if (!unparser.Open(GetQueryFileFullPathName (TRUE)))
		return TRUE;

	unparser.SetFormat(CLineFile::UTF8);

	// Salvataggio dell'attuale versione del file di transcodifica
	unparser.UnparseTag		(T_RELEASE, FALSE);
	unparser.UnparseInt		(nQueriesFileRelease);
	unparser.UnparseBegin	();

	for (int nIdx = 0; nIdx < GetSize (); nIdx++)
	{
		QueryObj* pQueryObj = GetAt (nIdx);
		pQueryObj->Unparse (unparser, pQueryObj == m_pDefaultQuery);
	}

	unparser.UnparseEnd ();
	return TRUE;
}

// Salva l'attuale configurazione delle query nel file di transcodifica aggiornando
// la data e l'ora dell'ultima modifica
//-----------------------------------------------------------------------------
void AllQueries::SaveQueriesFile ()
{
	// Aggiornamento della data e ora di ultima modifica
	if (Unparse ())
		GetFileTime (GetQueryFileFullPathName (), m_mtime);
}

// Inizializza la struttura per la memorizzazione delle query. Ritorna TRUE se
// la query di default non coincide con quella predefinita, altrimenti FALSE.
// L'esecuzione della query è necessario in quanto la tabella viene inizializzata
// e aperta con quella predefinita
//-----------------------------------------------------------------------------
BOOL AllQueries::Attach (SqlTable* pTable, BOOL bUsePredefined)
{
	if (pTable == NULL)
	{
		ASSERT (FALSE);
		return FALSE;
	}

	SAFE_DELETE(m_pTempQueries);
	SAFE_DELETE(m_pPredefiniteQuery);
	SAFE_DELETE(m_pSortAsc);
	SAFE_DELETE(m_pSortDesc);

	// Allocazione dell'array delle query temporanee
	m_pTempQueries = new TempQueries;

	// Assegnazione dei data member utilizzati per il controllo e la preparazione
	// della query
	m_pTable	= pTable;
	m_pSqlConnection = m_pTable->m_pSqlConnection;

	m_TableInfo.Assign(*pTable);

	if (!m_TableInfo.m_pTableInfoArray || m_TableInfo.m_pTableInfoArray->GetSize() == 0)
	{
		ASSERT (FALSE);
		return FALSE;
	}

	QueryInfo* pQi = new QueryInfo (m_pSqlConnection, TRUE, &m_TableInfo);
	// Creazione della query predefinita
	m_pPredefiniteQuery = new QueryObj (pQi);

	// Inizialmente la query di default coincide con quella predefinita
	m_pDefaultQuery = m_pPredefiniteQuery;

	// Se non si deve usare la predefinita carica le query definite su file
	if (!bUsePredefined && !LoadQueries ())
		BarQueryMessageBox (_TB("An error occurred while uploading the transcoding file:\r\n\t{0-%s}\r\nQueries may have been not properly loaded."), GetQueryFileFullPathName (), MB_OK);

	// Selezione della query di default
	m_pSelectedQuery = m_pDefaultQuery;

	// Se la query differisce da quella predefinita eseguiro` la query impostata
	return m_pDefaultQuery != m_pPredefiniteQuery;
}

//	Ritorna TRUE se l'esecuzione della query attualmente selezionata è andata a
// buon fine altrimenti FALSE. Gli errori che si possono verificare in questa
// fase sono:
//		- nel caricamento; le possibili cause vanno	ricercate:
//				- nella cancellazione del file della query;
//				- la query è stata rinominata;
//				- errore in fase di parse;
//		- nella preparazione;
//
//	in entrambi i casi viene segnalato all'utente il tipo di errore occorso, e
// si sostituisce a quella selezionata quella di default. Se quest'ultima coincide
// con quella predefinita non ho alcun problema, altrimenti mi potrei trovare
// nuovamente nella situazione di partenza e a questo punto seleziono quella
// predefinita. Si ricorda che il caricamento da file (ogni volta che eseguo una
// query) è stato introdotto per 'sentire' le eventuali modifiche apportate
//-----------------------------------------------------------------------------
BOOL AllQueries::ExecSelectedQuery (BOOL bFromAttach, CWnd* pParent/* = NULL*/, BOOL bDoQuery /*= TRUE*/)
{
	enumQueryExec error = ExecQuery (bFromAttach, pParent, bDoQuery);
	while (error != Ok && m_pSelectedQuery != m_pPredefiniteQuery)
	{
		// In base al tipo di errore visualizza il relativo messaggio
		switch (error)
		{
			case Failed:
				BarQueryMessageBox (_TB("An error occurred while checking the query:\r\n{0-%s}\r\nThe preferred query will be used instead."), m_pSelectedQuery->m_strQueryName, MB_OK | MB_ICONSTOP);
				break;
			case UserAbort:
				BarQueryMessageBox (_TB("Error uploading query:\r\n\t{0-%s}\r\nThe query cannot be processed, the preferred one will be used instead.\r\nPossible causes:\r\n- the query has been deleted;\r\n- the query is not valid."), m_pSelectedQuery->m_strQueryName, MB_OK | MB_ICONSTOP);
				break;
			default: ASSERT (FALSE);
		}
		if (m_pSelectedQuery == m_pDefaultQuery)
			m_pSelectedQuery = m_pPredefiniteQuery;
		else
		{
			// Provo a caricare le informazioni per eseguire la query
			// preferenziale, se fallisce utilizzo quella predefinita
			QueryInfo* pQueryInfo = GetQueryInfo (m_pDefaultQuery);
			m_pSelectedQuery = !pQueryInfo || pQueryInfo->m_pPrgData
									? m_pPredefiniteQuery
									: m_pDefaultQuery;
		}
		// Riprovo ad eseguire la query
		error = ExecQuery (bFromAttach, pParent, bDoQuery); 
	}
	return error == Ok;
}

// Ritorna Ok se la query è stata eseguita con successo altrimenti un codice
// di errore
//-----------------------------------------------------------------------------
enumQueryExec AllQueries::ExecQuery (BOOL bFromAttach, CWnd* pParent/* = NULL*/, BOOL bDoQuery)
{
	ASSERT_VALID (m_pSelectedQuery);
	CWaitCursor curW;

	// La query non esiste più
	QueryInfo* pQuery = GetQueryInfo (m_pSelectedQuery);
	if (!pQuery)
		return UserAbort;

	ASSERT (m_pTable);
	ASSERT_VALID (pQuery);
	
	enumQueryExec nResult = Failed;

	TRY
	{
		// pulisce i parametri di select per evitare di portarsi dietro la
		// eventuale select fatta dalla OnPrepareBrowser
		m_pTable->m_strFilter.	Empty();
		m_pTable->m_strSort.	Empty();

		// Controllo sintattico della query assegnazione dei relativi parametri 
		// alla tabella
		if (pQuery->m_TableInfo.m_strFilter.IsEmpty ())
		{
			if (pQuery->m_pPrgData && !pQuery->m_pPrgData->ExecAskRules (bFromAttach, pParent))
				return UserAbort;

			// Se la tabella e` aperta occorre necessariamente chiuderla
			if (m_pTable->IsOpen())	
				m_pTable->Close();
			
			m_pTable->Open(FALSE, E_KEYSET_CURSOR, FALSE); // utilizzo cursore scrollabile
			m_pTable->EnableRemoveDeletedRow(); //cancellazione dei record (vedi anomalia: 18257)

			m_pTable->SelectAll();
		}
		else
		{
			if (!m_pTable->IsOpen())
				m_pTable->Open(FALSE, E_KEYSET_CURSOR, FALSE);
			m_pTable->EnableRemoveDeletedRow(); //cancellazione dei record (vedi anomalia: 18257)

			ASSERT_VALID(m_TableInfo.m_pTableInfoArray);
			WClause aWC (m_pSqlConnection, pQuery->m_pPrgData ? pQuery->m_pPrgData->GetSymTable() : NULL, *m_TableInfo.m_pTableInfoArray, m_pTable);
			aWC.SetNative(m_pSelectedQuery->IsPredefiniteQuery() || pQuery->m_bNativeExpr);

			Parser lex(pQuery->m_TableInfo.m_strFilter);

			// Impostazione della Where Clause
			if (!aWC.Parse(lex))
			{
				TRACE0 ("AllQueries::ExecQuery: WClause::Parse failed.\n");
				return Failed;
			}

			// Se la query è una query parametrica richiede l'imputazione dei
			// parametri. Se l'utente abortisce in questa fase i parametri della
			// tabella contengono valori non validi.
			if (pQuery->m_pPrgData && !pQuery->m_pPrgData->ExecAskRules (bFromAttach, pParent))
				return UserAbort;
		
			// PrepareQuery di WClause esegue anche l'apertura della tabella la select di tutte
			// le colonne del SqlRecord correntemente associato alla SqlTable connessa alla
			// WClause
			if (!aWC.PrepareQuery())
			{
				if (aWC.GetErrId())
				{
					AfxMessageBox(aWC.GetErrId());
					return Failed;
				}

				//@@TODO tapullo per gestire il possibile caso in cui la PrepareQuery
				// ritorni FALSE per BREAK (vedere odbc\wclause.cpp): cioe` per qualche
				// condizione strana l'utente non vuole estrarre nulla.
				// Non succedera` mai, ma l'ho gestito per amor di completezza.
				//											Germano Traverso
				DataBool aTRUE	(TRUE);
				DataBool aFALSE (FALSE);
				
				m_pTable->m_strFilter = cwsprintf
				(
					_T("%s = %s"),
					(LPCTSTR)(m_pTable->m_pSqlConnection->NativeConvert(&aFALSE)),
					(LPCTSTR)(m_pTable->m_pSqlConnection->NativeConvert(&aTRUE))					
				);                 
			}
		}

		//---- Impostazione delle condizioni di sort e di Select
		m_pTable->m_strSort = pQuery->m_TableInfo.m_strSort;

		if (!ExpandContentOfClause(m_pTable->m_strSort, (pQuery->m_pPrgData ? pQuery->m_pPrgData->GetSymTable() : NULL), m_pTable->m_pSqlConnection))
			return Failed;

		if (!/*pQuery->*/m_TableInfo.m_strFrom.IsEmpty())
			m_pTable->m_strFrom = /*pQuery->*/m_TableInfo.m_strFrom;
		else if (m_TableInfo.m_pTableInfoArray && m_TableInfo.m_pTableInfoArray->GetCount() > 1)
			m_pTable->m_strFrom = m_TableInfo.m_pTableInfoArray->Unparse();

		if (!m_TableInfo.m_strSelect.IsEmpty())
		{
			m_pTable->m_strSelect = m_TableInfo.m_strSelect;

			m_pTable->SelectColumns(m_TableInfo);
		}

		//----
		if (bDoQuery)
			m_pTable->Query();
			
		// All works fine
		nResult = Ok;
	}
	CATCH(SqlException, e)	
	{
		AfxMessageBox (e->m_strError);
		if (m_pTable->IsOpen())	m_pTable->Close();
	}
	END_CATCH

	// Esecuzione della query
	return nResult;
}

// Ritorna il percorso completo del file di transcodifica su cui salvare le
// informazioni delle query e genera l'albero di directory se sta per salvare
//-----------------------------------------------------------------------------
CString AllQueries::GetQueryFileFullPathName (BOOL bFromSave /*FALSE*/)
{
	if (m_nsCustomize.IsEmpty() || !m_nsCustomize.IsValid())
		return _T("");

	CString strQueryFilePath = AfxGetPathFinder ()->GetDocumentQueryPath (m_nsCustomize, CPathFinder::ALL_USERS, _T(""), bFromSave);

	// lettura || scrittura
	if (!bFromSave || ExistPath (strQueryFilePath))
		return MakeFilePath (strQueryFilePath, GetQueryFileName (), szQueryExt);

	BarQueryMessageBox (_TB("Unable to create the directory:\r\n'{0-%s}'\r\nto save the query configuration files.\r\nMake sure you have the required access rights."), strQueryFilePath, MB_OK | MB_ICONSTOP);
	return _T("");
}

// Ritorna NULL se la query non deve essere cancellata, altrimenti il nome
// della nuova query selezionata. Elimina dalla struttura in memoria la
// query passata come parametro eseguendo tutti i controlli del caso. I vicoli
// imposti all'utente sono:
// 	- non può cancellare la query predefinita;
// 	- se viene cancellata la query attualmente selezionata, seleziono quella
//		di default;
// 	- se viene cancellata la query di default viene selezionata quella
//		predefinita;
//	- la cancellazione avviene su esplicita richiesta di conferma.
//-----------------------------------------------------------------------------
BOOL AllQueries::DeleteQuery (QueryObj* pQueryObj)
{
	ASSERT_VALID (pQueryObj);
	ASSERT (!pQueryObj->IsPredefiniteQuery());

	// Chiedo la conferma all'utente
	if (BarQueryMessageBox (_TB("Do you want to delete the query: '{0-%s}'? "), pQueryObj->m_strQueryName, MB_YESNO | MB_ICONSTOP) != IDYES)
		return FALSE;

	// Se una query temporanea la rimuovo dal relativo array;
	if (pQueryObj->m_enumType == Temp)
	{
		ASSERT_VALID (m_pTempQueries);
		// Se l'attuale query da cancellare è quella selezionata, la nuova
		// query di selezionata diventa quella default
		if (pQueryObj == m_pSelectedQuery)
			m_pSelectedQuery = m_pDefaultQuery;

		m_pTempQueries->DeleteQuery (pQueryObj);
		return TRUE;
	}

	// Se la query da cancellare coincide con quella di default, la nuova
	// query di default diventa quella predefinita
	if (pQueryObj == m_pDefaultQuery)
		m_pDefaultQuery = m_pPredefiniteQuery;

	// Se l'attuale query da cancellare è quella selezionata, la nuova query
	// selezionata diventa quella default
	if (pQueryObj == m_pSelectedQuery)
		m_pSelectedQuery = m_pDefaultQuery;

	// Rimozione del file associato alla query
	pQueryObj->RemoveFile (GetQueryDir(), GetQueryFileName ());

	// Ricerca e cancellazione della query
	RemoveAt (FindPos (0, GetUpperBound(), pQueryObj->m_nExtIndex));
	SaveQueriesFile ();
	return TRUE;
}

// Esegue ricorsivamente una ricerca dicotomica nell'array delle query. Ritorna
// l'indice in cui dovrà essere inserito il nuovo elemento, nel caso si verifichi
// un errore ritorna -1;
//-----------------------------------------------------------------------------
int AllQueries::GetNewPos(int nStart, int nEnd, int nIdxToFind)
{
	if (GetSize () == 0)
		return 0;

	// nel caso la distanza sia dispari si arrotonda per eccesso
	int nMiddleIdx	= (nStart + nEnd + 1) / 2;
	int nDist		= nEnd - nStart;

	int nExtIndex = GetAt (nMiddleIdx)->m_nExtIndex;

	// L'errore si può verificare solo ed esclusivamente se l'utente edita in
	// maniera non corretta il file di transcodifica
	if (nExtIndex == nIdxToFind)
		return -1;

	if (nExtIndex > nIdxToFind)
		// se il numero la distanza è 0 vuol dire che senza alcun dubbio
		// la nuova posizione è quella del corrente indice centrale.
		// Se la distanza è unitaria si è in pratica confrontato con il
		// secondo estremo, di conseguenza visto che ciò è stato fatto in
		// maniera arbitraria è necessario forzare il confronto con il primo
		// estremo prima di prendere la decisione
		return nDist == 0 ?  nMiddleIdx : GetNewPos(nStart, nDist == 1 ? nStart : nMiddleIdx, nIdxToFind);

	// se l'indice è maggiore di quello nella posizione centrale
	// e la distanza tra gli estremi e` minore o uguale a 1 senzaltro
	// il nuova indice deve essere posizionato dopo la posizione centrale
	return nDist <= 1 ? nMiddleIdx + 1 : GetNewPos(nMiddleIdx, nEnd, nIdxToFind);
}

// Esegue ricorsivamente una ricerca dicotomica nell'array delle query.
// Viene richiamata esclusivamente in fase di cancellazione di una query
// definitiva. Per come è stato implementato il meccanismo di assegnazione
// dell'estensioni l'indice esiste e non è duplicato.
//-----------------------------------------------------------------------------
int AllQueries::FindPos (int nStart, int nEnd, int nExtIdxToFind)
{
	// nel caso la distanza sia dispari si arrotonda per eccesso
	int nMiddleIdx	= (nStart + nEnd + 1) / 2;
	int nDist		= nEnd - nStart;
	int nExtIndex	= GetAt (nMiddleIdx)->m_nExtIndex;

	if (nExtIndex == nExtIdxToFind)
		return nMiddleIdx;

	return nExtIndex > nExtIdxToFind
				? FindPos(nStart, nDist == 1 ? nStart : nMiddleIdx, nExtIdxToFind)
				: FindPos(nMiddleIdx, nEnd, nExtIdxToFind);
}

// Ritorna le informazioni alla query se questa fa riferimento alla tabella
// corrente altrimenti NULL (filtraggio delle query).
//-----------------------------------------------------------------------------
QueryObj* AllQueries::GetQueryObj (int nIdx)
{
	QueryObj* pQuery = GetAt (nIdx);
	return	pQuery->m_enumType == Active ? pQuery : NULL;
}

// Aggiorna il contenuto di una query sia essa temporanea che definitiva. Per
// quelle definitive si occupa anche dell'eventuale salvataggio dei file. Nel
// caso le modifiche riguardano solo i parametri per l'esecuzione della query
// viene salvato solo il file relativo mentre nel caso venga selezionata come
// query di default occorre salvare il file di transcodifica.
//-----------------------------------------------------------------------------
void AllQueries::UpdateQuery
	(
				QueryObj*		pQueryObj,
				BOOL			bNativeExpr,
		const	CString&		strWClause,
		const	CString&		strOrderBy, 
				BOOL			bAsDefault,
				ProgramData*	pPrgData
	)
{
	ASSERT_VALID (pQueryObj);
	ASSERT_VALID (m_pTempQueries);

	if (pQueryObj->m_enumType == Temp)
	{
		ASSERT_VALID (pQueryObj->m_pQueryInfo);

		// La query è temporanea, aggiorno direttamente i dati
		pQueryObj->m_pQueryInfo->m_bNativeExpr	= bNativeExpr;
		pQueryObj->m_pQueryInfo->m_TableInfo.m_strFilter	= strWClause;
		pQueryObj->m_pQueryInfo->m_TableInfo.m_strSort		= strOrderBy;
		pQueryObj->m_pQueryInfo->SetProgramData(pPrgData);
		return;
	}

	ASSERT (pQueryObj->m_enumType == Active);
	QueryInfo* pQueryInfo = GetQueryInfo (pQueryObj);

	// Se la funzione ritorna un valore nullo non bisogna considerarlo un errore
	// visto che andrà comunque a sovrascrivere gli eventuali valori.
	//@@ WHY PECCHE`?? (Germano)
	if (!pQueryInfo)
	{
		ASSERT(FALSE);
		pQueryInfo = new QueryInfo (m_pSqlConnection, FALSE);
		pQueryObj->m_pQueryInfo = pQueryInfo;
	}

	pQueryInfo->m_bNativeExpr	= bNativeExpr;
	pQueryInfo->m_TableInfo.m_strFilter	= strWClause;
	pQueryInfo->m_TableInfo.m_strSort	= strOrderBy;
	pQueryInfo->SetProgramData(pPrgData);

	if (!((m_pDefaultQuery == pQueryObj && bAsDefault)|| (m_pDefaultQuery != pQueryObj && !bAsDefault)))
	{
		// Devo salvare anche il file di transcodifica
		if (m_pDefaultQuery == pQueryObj && !bAsDefault)
			m_pDefaultQuery = m_pPredefiniteQuery;
		else if (bAsDefault)
			m_pDefaultQuery = pQueryObj;

		// Salvataggio del file di transcodifica
		SaveQueriesFile ();
	}

	// Salvataggio del file della query
	pQueryObj->SaveQuery(GetQueryDir(), GetQueryFileName ());
}

// Ritorna TRUE se l'operazione di rename ha successo, altrimenti FALSE.
// Questa funzione oltre che ad aggiornare i valori esegue anche le eventuali
// traformazione necessarie salvandole su file. Le trasformazioni possibili
// sono:
//		- da temp a temp: aggiorno solo il contenuto in memoria;
//
//		Per riguarda le trasformazioni che seguono, tutte aggiornano il file
//		di transcodifica e della query.
//		- da temp a definitiva: trasforma la query da temporanea a definitiva
//			aggiornando le strutture in memoria;
//		- da definitiva a temp : trasforma la query da definitiva a temporanea
//			aggiornando le strutture in memoria;
//		- da definitiva a definitiva : aggiorno il contenuto in memoria;
//
//	La traformazione viene eseguita automaticamente in base allo stato attuale
// della query e al nuovo nome assegnatole.
//-----------------------------------------------------------------------------
BOOL AllQueries::RenameQuery (const CString& strQueryName, QueryObj* pQueryObj)
{
	ASSERT_VALID (pQueryObj);
	ASSERT_VALID (m_pTempQueries);

	BOOL bTempName = IsTempQueryName(strQueryName);
	if (pQueryObj->m_enumType == Temp)
	{
		if (bTempName)
		{
			// La tipologia della query non è stata modificata, assegno quindi
			// il nuovo nome
			pQueryObj->m_strQueryName = strQueryName;
			return TRUE;
		}
		// Il nome identifica una query non più temporanea, devo trasformarla
		InsertAtFromTemp (GetSize () ? FindHole () : 0, pQueryObj);
	}
	else if (bTempName)
	{
		// Assegno il nuovo nome alla query
		pQueryObj->m_strQueryName = strQueryName;
		// La query viene trasformata da definitiva a temporanea. Devo solo
		// salvare il file di transcodifica
		AddToTemp (pQueryObj);

		// Salvataggio del file di transcodifica
		SaveQueriesFile ();
		return TRUE;
	}

	if	(
			!pQueryObj->m_pQueryInfo && 
			!pQueryObj->GetQueryInfo (GetQueryDir(), GetQueryFileName(), *m_TableInfo.m_pTableInfoArray)
		)
	{
		AfxMessageBox (cwsprintf (_TB("Unable to rename the query:\r\n{0-%s}\r\nin the query:\r\n{1-%s}\r\nPossible causes are to be found in the query deletion."), (LPCTSTR) pQueryObj->m_strQueryName, (LPCTSTR) strQueryName), MB_OK | MB_ICONSTOP);
		return FALSE;
	}

	// Assegno il nuovo nome alla query
	pQueryObj->m_strQueryName = strQueryName;

	// Salvataggio del file di transcodifica
	SaveQueriesFile ();

	// Salva il file della query
	pQueryObj->SaveQuery(GetQueryDir(), GetQueryFileName ());
	return TRUE;
}

// Trasforma una query definitiva in una query temporanea, provvedendo a spostare
// la query da una struttura all'altra
//-----------------------------------------------------------------------------
int AllQueries::AddToTemp (QueryObj* pQueryObj)
{
	ASSERT_VALID (pQueryObj);
	ASSERT_VALID (m_pTempQueries);

	// Se coincide con quella di default, la nuova query diventa quella predefinita
	if (pQueryObj == m_pDefaultQuery)
		m_pDefaultQuery = m_pPredefiniteQuery;

	// Rimozione del file di memorizzazione della query
	pQueryObj->RemoveFile (GetQueryDir(), GetQueryFileName ());

	// La query è stata trasformata in query temporanea, cancello solo
	// l'entry e non l'oggetto dalla struttura delle query definitive
	SetOwns (FALSE);
	RemoveAt(FindPos (0, GetUpperBound(), pQueryObj->m_nExtIndex));
	SetOwns	(TRUE);

	// Aggiornamento dei valori per definire una query temporanea
	pQueryObj->m_enumType		= Temp;
	pQueryObj->m_nExtIndex		= -1;
	pQueryObj->m_strTableName	= "";

	// Rimuovere il corrente file della query
	return m_pTempQueries->Add (pQueryObj);
}

// Trasforma una query temporanea in una query definitiva, provvedendo a spostare
// la query da una struttura all'altro ed inserendo la nuova query nella
// posizione indicata.
//-----------------------------------------------------------------------------
void AllQueries::InsertAtFromTemp (int nIdx, QueryObj* pQueryObj)
{
	ASSERT_VALID (pQueryObj);
	ASSERT_VALID (m_pTempQueries);

	// La query è stata trasformata in query definitiva, cancello solo
	// l'entry, e non l'oggetto, dalla struttura delle query temporanee
	m_pTempQueries->DeleteQuery (pQueryObj, FALSE);

	// Aggiornamento dei valori per definire una query definitiva
	pQueryObj->m_enumType		= Active;
	pQueryObj->m_nExtIndex		= nIdx;
	pQueryObj->m_strTableName	= GetTableName ();

	InsertAt (nIdx, pQueryObj);
}

// Aggiunge una nuova query. Se è una query temporanea aggiorno solo la
// struttura in memoria, altrimenti provvederà ad assegnare un'estensione
// univoca al file su cui dovrà essere memorizzata. L'assegnazione del codice
// dell'estensione viene eseguita secondo un particolare algoritmo in grado di
// trovare gli eventuali buchi di numerazione, se presenti, altrimenti si
// utilizza il primo indice libero (si ricorda che la struttura è ordinata
// proprio in base a questo codice).
//	Provvederà anche al salvataggio sia del file di transcodifica che quello
// relativo alla query.
//-----------------------------------------------------------------------------
QueryObj* AllQueries::AddNewQuery(const CString& strQueryName, QueryInfo* pQueryInfo, BOOL bAsDefault)
{
	// Il nome della query contiene il prefisso delle query temporanee?
	if (IsTempQueryName(strQueryName))
		return AddNewTempQuery (strQueryName, pQueryInfo);

    // Ricerca della posizione in cui inserire la nuova query, se non
    // sono presenti buchi aggiungo in coda
	int nIdx;
	QueryObj* pQueryObj;
	if (GetSize() == 0 || GetUpperBound() == GetAt (GetUpperBound())->m_nExtIndex)
		nIdx = Add (new QueryObj (m_pSqlConnection, GetTableName(), strQueryName, GetSize(), pQueryInfo));
	else
	{
		// Esiste sicuramente un buco nella numerazione
		nIdx = FindHole ();
		InsertAt (nIdx, new QueryObj (m_pSqlConnection, GetTableName(), strQueryName, nIdx, pQueryInfo));
	}
	pQueryObj = GetAt (nIdx);
	if (bAsDefault)
		m_pDefaultQuery	= pQueryObj;

	// Salvataggio dell'attuale configurazione
	SaveQueriesFile ();
    pQueryObj->SaveQuery (GetQueryDir(), GetQueryFileName ());
	return pQueryObj;
}

// Aggiunge una nuova query temporanea
//-----------------------------------------------------------------------------
QueryObj* AllQueries::AddNewTempQuery(const CString& strQueryName, QueryInfo* pQueryInfo)
{
	ASSERT_VALID (m_pTempQueries);
	return m_pTempQueries->AddQuery (strQueryName, pQueryInfo);
}

// Ricerca gli eventuali buchi nella numerazione delle estensioni ritornando
// l'indice corretto
//-----------------------------------------------------------------------------
int AllQueries::FindHole ()
{
	ASSERT (GetSize () > 0);

	if (GetUpperBound() != GetAt (GetUpperBound())->m_nExtIndex)
		for (int nIdx = 0; nIdx < GetSize(); nIdx++)
			if (GetAt (nIdx)->m_nExtIndex > nIdx)
				return nIdx;

	return GetSize();
}

// Ritorna il path completo della directory in cui sono salvate le informazioni
// relative alle query
//-----------------------------------------------------------------------------
CString AllQueries::GetQueryDir()
{
	if (m_nsCustomize.IsEmpty() || !m_nsCustomize.IsValid())
		return _T("");
	return AfxGetPathFinder()->GetDocumentQueryPath (m_nsCustomize, CPathFinder::ALL_USERS);
}

// nome di file compatibile.
//-----------------------------------------------------------------------------
CString AllQueries::GetQueryFileName ()
{
	return GetTableName().Left(8);
}

//	Confronta i valori correnti della data e dell'ora del file di transcodifica
// con quelli presenti in memoria, se coincidono nessuno ha aggiornato il file.
// In caso contrario occorre procedere al caricamento dei nuovi valori, prima
// di effettuare questa operazione, che richiede la cancellazione completa della
// struttura delle query definitive, è necessario controllare il tipo di query
// attualmente selezionato e se coincide con una di quelle definitive occorre
// salvare il suo nome per poterla poi ripristinare ad operazione terminata. Non
// c'è alcun problema invece se si tratta di query temporanea o definitiva in
// quando risiedono sempre in memoria.
//	A caricamento avvenuto, in caso di precedente query selezionata di tipo
// definitivo, verifico l'esistenza della query, se ha esito negativo seleziono
// in maniera automatica quella di default e la eseguo.
// 	Ritorna TRUE se deve essere aggiornato il valore del bottone presente nella
// BarQuery (la query selezionata è stata modificata) altrimenti FALSE.
//	Viene richiamata ogni volta che si esegue una query o quando si entra nella
// dialog per la gestione delle query.
//-----------------------------------------------------------------------------
BOOL AllQueries::LoadChangedQueries ()
{
	CTime mtime;
	// Verifico se il file di transcodifica è stato modificato
	if (!GetFileTime (GetQueryFileFullPathName (), mtime) || mtime <= m_mtime)
		return FALSE;

	// Se la query attualmente selezionata è una query che viene salvata su file
	// devo memorizzare il suo nome e mettere a NULL il relativo puntatore, in
	// quanto l'attuale struttura in memoria sarà cancellata e ricaricata.
	// Il puntatore rimane valido se la query selezionata coincide con quella
	// predefinita o con una temporanea
	ASSERT_VALID (m_pSelectedQuery);
	CString strQueryNameSelect (m_pSelectedQuery->m_strQueryName);
	if (m_pSelectedQuery->m_enumType == Active)
		m_pSelectedQuery = NULL;

	// Inizialmente la query di default coincide con quella predefinita
	m_pDefaultQuery = m_pPredefiniteQuery;

	if (!LoadQueries ())
		BarQueryMessageBox (_TB("An error occurred while uploading the transcoding file:\r\n\t{0-%s}\r\nQueries may have been not properly loaded."), GetQueryFileFullPathName (), MB_OK);

	if (m_pSelectedQuery || SelectQuery (strQueryNameSelect))
		return FALSE;

	// la query precedentemente selezionata non è più presente, segnalo l'errore
	// e seleziona come nuova query quella di default
	BarQueryMessageBox (_TB("Error uploading query:\r\n\t{0-%s}\r\nThe query cannot be processed, the preferred one will be used instead.\r\nPossible causes:\r\n- the query has been deleted;\r\n- the query is not valid."), strQueryNameSelect, MB_OK | MB_ICONSTOP);
	m_pSelectedQuery = m_pDefaultQuery;
	return TRUE;
}

// Ritorna il nome della completo tabella
//-----------------------------------------------------------------------------
CString AllQueries::GetTableName () const
{
	if (!m_TableInfo.m_pTableInfoArray || m_TableInfo.m_pTableInfoArray->GetSize() == 0)
	{
		ASSERT(FALSE);
		return _T("");
	}

	return m_TableInfo.m_pTableInfoArray->GetAt(0)->GetTableName();
}

//Ritorna le informazioni per la preparazione ed esecuzione della query
//-----------------------------------------------------------------------------
QueryInfo* AllQueries::GetQueryInfo (QueryObj* pQueryObj)
{
	ASSERT (m_TableInfo.m_pTableInfoArray && m_TableInfo.m_pTableInfoArray->GetSize());
	ASSERT_VALID (pQueryObj);

	return pQueryObj->GetQueryInfo (GetQueryDir(), GetQueryFileName(), *m_TableInfo.m_pTableInfoArray);
}

// Riempie la lista con le query attualmente disponibili per la tabella
//-----------------------------------------------------------------------------
void AllQueries::FillList (CQueriesListBox* pList)
{
	ASSERT_VALID (pList);
	// Aggiungo la query definita dal programmatore (predefinita)
	pList->AddQuery (m_pPredefiniteQuery);

	// Aggiungo tutte le query della tabella 
	for (int nIdx = 0; nIdx < GetSize(); nIdx++)
	{
		// Ritorna solo le query relative alla tabella in uso
		QueryObj* pQueryObj = GetQueryObj (nIdx);
		if (pQueryObj)
			pList->AddQuery (pQueryObj);
	}
	// Inserisco le eventuali query temporanee
	ASSERT_VALID (m_pTempQueries);
	m_pTempQueries->FillList (pList);
}

// Ritorna TRUE se la query esiste nell'array, altrimenti FALSE. La ricerca
// viene effettuata solo per le query definitive
//-----------------------------------------------------------------------------
int AllQueries::IsExistQuery (const CString& strQuery)
{
	for (int nIdx = 0; nIdx < GetSize(); nIdx++)
	{
		// Controllo se l'elemento appartiene alla tabella corrente
		QueryObj* pQueryObj = GetQueryObj (nIdx);
		if (!pQueryObj)
			continue;

		if (strQuery.CompareNoCase (pQueryObj->m_strQueryName) == 0)
			return nIdx;
	}
	return -1;
}

// Seleziona la query indicata come parametro. Ritorna TRUE se l'operazione
// viene eseguita con successo, altrimenti FALSE. 
//-----------------------------------------------------------------------------
BOOL AllQueries::SelectQuery (const CString& strQuery)
{
	int nIdx = IsExistQuery (strQuery);

	if (nIdx >= 0) 
		m_pSelectedQuery = GetAt (nIdx);
	else
	{
		// può essere quella predefinita
		if (m_pPredefiniteQuery->m_strQueryName.CollateNoCase(strQuery) == 0)
			m_pSelectedQuery = m_pPredefiniteQuery;
		else
			return FALSE;
	}

	// A questo punto posso accedere direttamente all'elemento della tabella
	// senza controllare se appartiene alla tabella in esame. Tale controllo
	// è stato eseguito dalla funzione precedente	
	ASSERT_VALID (m_pSelectedQuery);
	return TRUE;
}

// Esegue una ricerca nell'array delle query temporanea per trovare la query
// indicata. Se la query viene trovata vengono azzerati i valori altrimenti
// ne crea una nuova. In entrambi i casi ritorna il puntatore alla struttura
// del dato
//-----------------------------------------------------------------------------
QueryObj* AllQueries::GetAndResetTempQuery (const CString& strQueryName, ProgramData* pPrgData/*=NULL*/)
{
	ASSERT_VALID (m_pTempQueries);
	int nIdxQuery = m_pTempQueries->IsExistQuery (strQueryName);
	if (nIdxQuery < 0)
	{
		QueryObj* pQueryObj = AddNewTempQuery (strQueryName, new QueryInfo(m_pSqlConnection, FALSE));
		if (pPrgData && pQueryObj->m_pQueryInfo)
		{
			pQueryObj->m_pQueryInfo->m_pPrgData = pPrgData;
			pPrgData->m_nCounter++;
		}
		return pQueryObj;
	}
	// La query era già presente nell'array delle query temporanee, devo
	// resettare i valori
	QueryObj* pQueryObj = m_pTempQueries->GetAt (nIdxQuery);
	pQueryObj->m_pQueryInfo->ResetAllFields(); //chiama la ReleaseProgramData()
	if (pPrgData)
	{
		pQueryObj->m_pQueryInfo->m_pPrgData = pPrgData;
		pPrgData->m_nCounter++;
	}
	return pQueryObj;
}

// E' richiamata esclusivamente dal radar per comporre le query di ordinamento
// ascendente e discendente in base alle colonne selezionate e passate nel parametro
// CStringArray. Costruisce, seleziona ed esegue la query indicata
//-----------------------------------------------------------------------------
BOOL AllQueries::MakeQueryOrderBy (const CStringArray& fieldArray, BOOL bDesc)
{
    // salvo la query corrente...
    // la estrapolo della query selezionata (se si è già passati
    // da qui la query e' già presente nell'array delle query temporaneee, e quindi 
    // si deve salvare la struttura della query prima di ripulirne i valori con 
    // GetAndResetTempQuery ) 
	ASSERT(m_pSelectedQuery && m_pSelectedQuery->m_pQueryInfo);
	SqlTableStruct sts (m_pSelectedQuery->m_pQueryInfo->m_TableInfo);

	BOOL bNative = m_pSelectedQuery->m_pQueryInfo->m_bNativeExpr || m_pSelectedQuery->IsPredefiniteQuery();
	
	m_pSelectedQuery = GetAndResetTempQuery (bDesc ? szTmpQueryNameDes : szTmpQueryNameAsc, m_pSelectedQuery->m_pQueryInfo->m_pPrgData);
	
	m_pSelectedQuery->m_pQueryInfo->m_bNativeExpr = bNative;

    // ripristino le clausola SQL corrente compromessa dalla GetAndResetTempQuery...
	m_pSelectedQuery->m_pQueryInfo->m_TableInfo.Assign(sts);

	//customizzo l'ordinamento
	m_pSelectedQuery->m_pQueryInfo->m_TableInfo.m_strSort.Empty();

	// Composizione della stringa di order by attraverso le colonne selezionate
	int nSize = fieldArray.GetSize();
	for (int nIdx = 0; nIdx < nSize; nIdx++)
	{
		if (nIdx > 0)
			m_pSelectedQuery->m_pQueryInfo->m_TableInfo.m_strSort += _T(", ");

		m_pSelectedQuery->m_pQueryInfo->m_TableInfo.m_strSort += fieldArray.GetAt (nIdx);
		m_pSelectedQuery->m_pQueryInfo->m_TableInfo.m_strSort += bDesc ? _T(" Desc") : _T(" Asc");
	}
    
	BOOL bRet = ExecQuery(FALSE, NULL, TRUE) == Ok;

	//ripristino l'ordinamento
	m_pSelectedQuery->m_pQueryInfo->m_TableInfo.m_strSort = sts.m_strSort;
	return bRet;
}

//-----------------------------------------------------------------------------
void AllQueries::RefreshPredefiniteQuery(SqlTable* pTable)
{
	m_pPredefiniteQuery->m_pQueryInfo->m_TableInfo.Assign(*pTable);
}


///////////////////////////////////////////////////////////////////////////////
// Diagnostics
#ifdef _DEBUG
void AllQueries::Dump (CDumpContext& dc) const
{
	ASSERT_VALID (this);
	dc << " AllQueries\n\tQueries per la tabella: " << GetTableName() << "\n";
	Queries::Dump(dc);
}
#endif // _DEBUG

///////////////////////////////////////////////////////////////////////////////
//						CQueryManager
///////////////////////////////////////////////////////////////////////////////
//
//	Permette di collegare la struttura delle query alla tabella e si occupa
// di eseguire la query correntemente selezionata.
//
//	ATTENZIONE: se il CQueryManager è collegato ad un radar lanciato da HOTLINK,
//					alla partenza, NON DEVE ASSOLUTAMENTE USARE LA QUERY DI
//					DEFAULT ma utilizzare quella predefinita.
// 
//	m_pAllQueries		: struttura delle query;
//
//	m_pProcessing	: semaforo per la protezione dell'area critica
//
//	m_nID			: ID del bottone di esecuzione della query nella relativa toolbar
//
///////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
CQueryManager::CQueryManager(CAbstractDoc* pDocument, UINT nID)
	:
	m_pAllQueries	(NULL),
	m_pProcessing	(NULL),
	m_bDisableQuery	(FALSE),
	m_pDocument		(pDocument),
	m_nID			(nID)
{
}

//-----------------------------------------------------------------------------
CQueryManager::~CQueryManager()
{
	SAFE_DELETE(m_pAllQueries);
	SAFE_DELETE(m_pProcessing);
}

// Collega il manager con la struttura delle query. Se esiste una query di default
// diversa da quella predefinita e non è collegata ad un
// radar proveniente da HotLink la eseguo.
//-----------------------------------------------------------------------------
void CQueryManager::Attach (SqlTable* pTable, BOOL bUsePredefined /* FALSE */, BOOL bPostMsg /*= FALSE*/, BOOL bExecQuery /*= TRUE*/)
{
	ASSERT (pTable);
	m_pProcessing	= new CriticalArea;
	m_pAllQueries	= new AllQueries;

	CTBNamespace sQryNamespace;
	if (m_pDocument->IsKindOf(RUNTIME_CLASS(CAbstractFormDoc)))
		sQryNamespace = m_pDocument->GetNamespace();
	else if (m_pDocument->IsKindOf(RUNTIME_CLASS(CAbstractDoc)))
	{
		ITBRadar* pRadarDoc = m_pDocument->GetRadarInterface ();
		if (pRadarDoc && pRadarDoc->GetCallerDoc())
			sQryNamespace = pRadarDoc->GetCallerDoc()->GetNamespace();
		else if (pRadarDoc && pRadarDoc->GetCallerHotLink() && !pRadarDoc->GetCallerHotLink()->GetAddOnFlyNamespace().IsEmpty())
			sQryNamespace = pRadarDoc->GetCallerHotLink()->GetAddOnFly();
		else if (pRadarDoc && pRadarDoc->GetCallerHotLink() && pRadarDoc->GetCallerHotLink()->GetAttachedDocument())
			sQryNamespace = pRadarDoc->GetCallerHotLink()->GetAttachedDocument()->GetNamespace();
		else 
			m_bDisableQuery = TRUE;
	}

	if (sQryNamespace.IsValid() && sQryNamespace.GetType() == CTBNamespace::DOCUMENT)
		m_pAllQueries->SetCustomizeNamespace (sQryNamespace);
	else
		m_bDisableQuery = TRUE;

	// Collego la tabella all'array delle query, che provvederà a caricarle
	// e ritornare la query attualmente selezionata (predefinita o default)
	if (m_pAllQueries->Attach (pTable, m_bDisableQuery || bUsePredefined) && !m_bDisableQuery )
		ExecSelectedQuery(TRUE, bPostMsg, bExecQuery);
	
	// Visualizzazione della query nel bottone
	SetQueryName ();

	ASSERT_VALID (m_pAllQueries->m_pSelectedQuery);
}

//-----------------------------------------------------------------------------
void CQueryManager::Detach ()
{
	SAFE_DELETE(m_pAllQueries);
	SAFE_DELETE(m_pProcessing);

	m_bDisableQuery	= FALSE;
}

// Visualizza il nome delle query selezionata insieme al suo prefisso
//-----------------------------------------------------------------------------
void CQueryManager::SetQueryName ()
{
	if (!CheckAllQueries())
		return;
	//CheckAllQueries effettua già la validazione di m_pAllQueries 
	//ASSERT_VALID(m_pAllQueries);
	//if (!m_pAllQueries) return;
	CString strQuery = cwsprintf(_TB("Query: {0-%s}"), m_pAllQueries->GetSelectedQueryName ());

	// forza l'esecuzione al documento ospite
	ASSERT_VALID(m_pDocument);
	if (!m_pDocument) return;
	POSITION pos = m_pDocument->GetFirstViewPosition();
	CView* pView = m_pDocument->GetNextView(pos);

	CAbstractFrame* pFrame = dynamic_cast<CAbstractFrame*>(pView->GetParentFrame());
	if (!pFrame) return;
	if (pFrame->GetAddLoginDataInTitle() || m_pAllQueries->GetSelectedQueryName() != m_pAllQueries->GetDefaultQueryName())
		pFrame->SetQueryName(strQuery);

	pFrame->OnUpdateFrameTitle(TRUE);
}

// 	Permette di entrare in editazione delle query presenti in memoria e che
// fanno riferimento all'attuale tabella. Prima di aprire la dialog devo
// verificare se si è verificato qualche cambiamenti nel file di
// transcodifica o delle varie query. Se così è stato provvederò a caricare
// i nuovi valori.
//-----------------------------------------------------------------------------
void CQueryManager::EditQuery (BOOL bExecOnly /*=FALSE*/)
{
	if (m_bDisableQuery)
		return;
	
	if (!CheckAllQueries())
		return ;

	BOOL bForceExec = m_pAllQueries->LoadChangedQueries ();

	// Visualizza la dialog per la modifica o l'inserimento delle query
	CBarQueriesDlg dlg (m_pAllQueries, bExecOnly);

	// Salvataggio delle query
    if (dlg.DoModal () == IDC_BARQUERY_BTN_EXEC || dlg.m_bModified || bForceExec)
		ExecSelectedQuery ();
}

// Esegue la query attualmente selezionata. Se la preparazione e l'esecuzione
// viene terminata con successo nel bottone è visualizzato il suo nome ed è
// segnalato al documento di aggiornare il proprio contenuto
//-----------------------------------------------------------------------------
BOOL CQueryManager::ExecSelectedQuery (BOOL bFromAttach /* FALSE */, BOOL bPostMsg /*= TRUE*/, BOOL bDoQuery /*= TRUE*/)
{
	if (!CheckAllQueries())
		return FALSE;

	// Controlla se l'area critica non sia già bloccata
	if (!m_pProcessing || m_pProcessing->IsLocked())
		return FALSE;

	if (m_bDisableQuery)
	{
		CWaitCursor curW;
		m_pAllQueries->m_pTable->Query();
	}
	else if (!m_pAllQueries->ExecSelectedQuery (bFromAttach, GetFrame(), bDoQuery))
	{
		m_pProcessing->Unlock();
		return FALSE;
	}

	// Selezione della query appena eseguita
	SetQueryName ();

	if (bPostMsg)
	{
		// forza l'esecuzione al documento ospite
		CFrameWnd* pFrame = GetFrame();
		if (pFrame)
		{
			pFrame->PostMessage(WM_COMMAND, (m_nID));
			pFrame->SetFocus();
		}
	}
	m_pProcessing->Unlock();

	return TRUE;
}

//-----------------------------------------------------------------------------
CFrameWnd* CQueryManager::GetFrame() const
{
	if (m_pDocument == NULL) 
		return NULL;
	POSITION pos = m_pDocument->GetFirstViewPosition();
	CView* pView = m_pDocument->GetNextView(pos);
	if (pView == NULL) 
		return NULL;
	CFrameWnd* pFrame = pView->GetParentFrame();
	return pFrame;
}

// Seleziona, se esiste,la query identificata dal parametro e la esegue
//-----------------------------------------------------------------------------
void CQueryManager::SelectQuery (const CString& strQuery)
{
	if (!CheckAllQueries())
		return;
	if (m_pAllQueries->SelectQuery (strQuery))
		ExecSelectedQuery ();
}

//-----------------------------------------------------------------------------
BOOL CQueryManager::CheckAllQueries()
{
	if (!m_pAllQueries)
	{	
		ITBRadar* pRad = m_pDocument->GetRadarInterface ();
		HotKeyLink* pHKL = NULL;
		if (pRad && (pHKL = pRad->GetCallerHotLink()))
		{
			m_pAllQueries = new AllQueries();
			m_pAllQueries->Attach(pHKL->GetSqlTable(), TRUE);
		}
	}
	ASSERT_VALID (m_pAllQueries);
	return m_pAllQueries ? TRUE : FALSE;
}

//	Costruisce le query di ordinamento ascendente e discendente in base alle
// colonne selezionate (passate come parametro) ed esegue la query richiesta.
// Questa funzione viene chiamata solo ed esclusivamente dal RADAR.
//-----------------------------------------------------------------------------
BOOL CQueryManager::MakeQueryOrderBy (const CStringArray& fieldArray, BOOL bDesc /* FALSE */)
{
	if (!CheckAllQueries())
		return FALSE;

	// Creazione ed esecuzione delle query di order by
	if (!m_pAllQueries->MakeQueryOrderBy (fieldArray, bDesc))
		return FALSE;

	// Setto all'interno del bottone l'attuale query in uso
	SetQueryName ();
	return TRUE;
}

//-----------------------------------------------------------------------------
void CQueryManager::RefreshPredefiniteQuery(SqlTable* pTable)
{
	m_pAllQueries->RefreshPredefiniteQuery(pTable);
}

//-----------------------------------------------------------------------------
void CQueryManager::GetAllQueriesForMenu(CStringArray& arQueries)
{
	if (GetDefaultQueryName())
		arQueries.Add(GetDefaultQueryName()); //_T("BARQUERY_DEFAULT_MENU")
	
	if (GetDefaultQueryName() != GetPredefiniteQueryName())
		arQueries.Add(GetPredefiniteQueryName());  //BARQUERY_PREDEFINITE_MENU
		
	// se ci sono carico anche le altre query
	if (m_pAllQueries->GetSize() > 0) 		
	{
		int i = 0;
		for (i = 0; i < m_pAllQueries->GetSize() ; i++)
		{
			QueryObj* pTempQuery =  m_pAllQueries->GetAt(i);

			if 
				(
					!pTempQuery ||
					pTempQuery->m_strQueryName == m_pAllQueries->GetDefaultQueryName() ||
					pTempQuery->m_strQueryName == m_pAllQueries->GetPredefiniteQueryName()
				) 
				continue;

			LPCTSTR pszName = (LPCTSTR)m_pAllQueries->GetAt(i)->m_strQueryName;
			arQueries.Add(pszName); 
		}
	}
}

///////////////////////////////////////////////////////////////////////////////
// Diagnostics
#ifdef _DEBUG
void CQueryManager::Dump (CDumpContext& dc) const
{
	ASSERT_VALID (this);
	AFX_DUMP0(dc, " CQueryManager\n");
	CObject::Dump(dc);
}
#endif // _DEBUG


// per la gestione del popup delle query
/////////////////////////////////////////////////////////////////////////////
// CQueryMenu
//----------------------------------------------------------------------------
CQueryMenu::CQueryMenu(CQueryManager* pQueryManager, CIdStringArray* pIDQueryArray, BOOL bShowQueryManager /*=FALSE*/)
	:
	m_pQueryManager(pQueryManager)
{
	ASSERT(pQueryManager);

	VERIFY(CreatePopupMenu());
	ASSERT(GetMenuItemCount()==0);

	AllQueries* pAllQueries = pQueryManager->m_pAllQueries;
	UINT nLastUI = ID_EXTDOC_DDTB_ENUMQUERY0;
	int nMaxVisibleQueryName = ID_EXTDOC_DDTB_ENUMQUERY_MAX; //massimo numero di query visibili nel menù a tendina

	// inserisco prima le query preferenziale e se diversa da quella predefinita, anche la
	// predefinita
	if (pAllQueries)
	{
		UINT nID = 0L;
		if (pAllQueries->m_pDefaultQuery)
		{
			nID = nLastUI++;
			VERIFY(AppendMenu(MF_STRING, nID, (LPCTSTR)pQueryManager->GetDefaultQueryName()));
			VERIFY(m_DefBitmap.Attach(LoadBitmapOrPng(TBIcon(szQuery_Defa, TOOLBAR), FALSE)));

			SetMenuItemBitmaps(ID_EXTDOC_DDTB_ENUMQUERY0, &m_DefBitmap, &m_DefBitmap);
			pIDQueryArray->Add(new CIdStringElement(nID, pQueryManager->GetDefaultQueryName()));
		}

		if (pAllQueries->m_pDefaultQuery != pAllQueries->m_pPredefiniteQuery)
		{
			nID = nLastUI++;
			VERIFY(AppendMenu(MF_STRING, nID, (LPCTSTR)pQueryManager->GetPredefiniteQueryName()));
			VERIFY (m_PredBitmap.Attach	(LoadBitmapOrPng(TBIcon(szQuery_Pred, TOOLBAR), FALSE)));
			SetMenuItemBitmaps(ID_EXTDOC_DDTB_ENUMQUERY0 + 1, &m_PredBitmap, &m_PredBitmap);
			pIDQueryArray->Add(new CIdStringElement(nID, pQueryManager->GetPredefiniteQueryName()));
		}
		
		// se ci sono carico anche le altre query
		if (pAllQueries->GetSize() > 0) 		
		{
			int i = 0;
			for (i = 0; i <= pAllQueries->GetUpperBound() ; i++)
			{
				if (i >= nMaxVisibleQueryName)
					break;

				if 
					(
						!pAllQueries->GetAt(i) ||
						pAllQueries->GetAt(i) == pAllQueries->m_pDefaultQuery ||
						pAllQueries->GetAt(i) == pAllQueries->m_pPredefiniteQuery
					) 
					continue;

				nID = nLastUI++;

				LPCTSTR pszName = (LPCTSTR)pQueryManager->m_pAllQueries->GetAt(i)->m_strQueryName;
				VERIFY(AppendMenu(MF_STRING, nID, pszName));
				if (_tcsicmp(_T("Ricerca"), pszName)==0)
				{
					VERIFY (m_FindBitmap.Attach	(LoadBitmapOrPng(TBIcon(szQuery_FindMN, TOOLBAR), FALSE)));
					SetMenuItemBitmaps(nID, &m_FindBitmap, &m_FindBitmap);			
				}
				pIDQueryArray->Add(new CIdStringElement(nID, pszName));
			}
			
			if (i >= nMaxVisibleQueryName)
				VERIFY(AppendMenu(MF_STRING, ID_OTHER_QUERY, (LPCTSTR)_T("...")));			
		}

		if (bShowQueryManager)
		{
			VERIFY(AppendMenu(MF_STRING, ID_EDIT_QUERY, _TB("Query Manager (Ctrl+E)...")));
			VERIFY (m_EditBitmap.Attach	(LoadBitmapOrPng(TBIcon(szQuery_EditMN, TOOLBAR), FALSE)));
			SetMenuItemBitmaps(ID_EDIT_QUERY, &m_EditBitmap, &m_EditBitmap);			
		}
	}
}

//----------------------------------------------------------------------------
CQueryMenu::~CQueryMenu()
{
	if (m_DefBitmap.m_hObject)
		m_DefBitmap.DeleteObject();

	if (m_PredBitmap.m_hObject)
		m_PredBitmap.DeleteObject();
	
	if (m_EditBitmap.m_hObject)
		m_EditBitmap.DeleteObject();
}

