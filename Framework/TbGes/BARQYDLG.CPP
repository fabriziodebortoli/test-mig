
#include "stdafx.h"

#include <TbGeneric\FontsTable.h>

#include <TbFrameworkImages\GeneralFunctions.h>
#include <TbFrameworkImages\CommonImages.h>

#include <TbGenlib\generic.h>
#include <TbGenlib\baseapp.h>
#include <TbParser\Parser.h>

#include <TbOledb\sqlcatalog.h>
#include <TbOledb\sqlconnect.h>
#include <TbOledb\wclause.h>

#include <TbWoormEngine\ActionsRepEngin.h>
#include <TbWoormEngine\edtcmm.h>
#include <TbWoormEngine\askdlg.h>
#include <TbWoormEngine\askdata.h>
#include <TbWoormEngine\prgdata.h>

#include <TbGes\extdoc.h>

#include "barqydlg.h"
#include "barquery.hjson" //JSON AUTOMATIC UPDATE

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif

///////////////////////////////////////////////////////////////////////////////
//
//	ATTENZIONE: le finestre di dialogo permettono di definire, modificare e
//					cancellare solo le query relative alla tabella correntemente
//					in uso.
//
///////////////////////////////////////////////////////////////////////////////

///////////////////////////////////////////////////////////////////////////////
// Implementazione di CBarQueryBaseDlg
///////////////////////////////////////////////////////////////////////////////
//
//	Classe base da cui derivano tutte le finestre di dialogo per l'editazione
// di ogni singola query. Gestisce completamente l'imputazione e l'assegnazione
// del nome di una query, controllando se il nome è in formato corretto. Il nome
// verrà quindi assegnato solo se soddisfa i seguenti vincoli:
// 	- non deve essere vuoto;
//	- non deve coincidere con quello della predefinita;
//	- non deve essere presente nella lista delle query in memoria;
//	in caso contrario l'errore sarà segnalato all'utente tramite opportuni
// messaggi di errore.
//	Inizialmente il bottone di OK sarà disabilitato e verrà abilitato solo
// quando l'utente avrà modificato uno o più control.
//
//	m_pQueriesList	: puntatore alla lista della query contenuta nella dialog
//						principale;
//
//	m_pAllQueries	: puntatore alla struttura delle query;
//
//	m_pStrEdtName	: string edit utilizzato per imputare o visualizzare il
//						nome della query nelle varie dialog;
//
///////////////////////////////////////////////////////////////////////////////
IMPLEMENT_DYNAMIC(CBarQueryBaseDlg, CParsedDialog)
BEGIN_MESSAGE_MAP(CBarQueryBaseDlg, CParsedDialog)
	//{{AFX_MSG_MAP(CBarQueryBaseDlg)
		ON_EN_CHANGE (IDC_BARQUERY_EDT_NAME, OnEdtChange)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CBarQueryBaseDlg::CBarQueryBaseDlg(UINT nIDD, CQueriesListBox* pList, AllQueries* pAllQueries)
	:
	CParsedDialog	(nIDD),
	m_pQueriesList	(pList),
	m_pAllQueries	(pAllQueries)
{
	m_pStrEdtName = new CStrEdit;
}

//-----------------------------------------------------------------------------
CBarQueryBaseDlg::~CBarQueryBaseDlg()
{
	ASSERT (m_pStrEdtName);
	delete m_pStrEdtName;
}

// Abilitazione/disabilitazione del bottone di Ok
//-----------------------------------------------------------------------------
void CBarQueryBaseDlg::BtnOkEnableWindow (BOOL bEnable /* = TRUE */)
{
	((CButton*) GetDlgItem (IDOK))->EnableWindow (bEnable);
}

//-----------------------------------------------------------------------------
BOOL CBarQueryBaseDlg::OnInitDialog()
{
	CParsedDialog::OnInitDialog();

	ASSERT (m_pStrEdtName);
	VERIFY (m_pStrEdtName->SubclassEdit (IDC_BARQUERY_EDT_NAME, this));

	// Disabilitazione del bottone di 'Ok'
	BtnOkEnableWindow (FALSE);

	CenterWindow();
	return TRUE;
}

// Ritorna TRUE se il controllo sul nome della query, inputato dall'utente, ha
// esito positivo altrimenti FALSE. Nel caso il nome non sia valido o già 
// esistente verranno visualizzati i relativi messaggi di errore.
//-----------------------------------------------------------------------------
BOOL CBarQueryBaseDlg::TestQueryName ()
{
	ASSERT (m_pStrEdtName);
	CString strQueryName = m_pStrEdtName->GetValue();
	if (strQueryName.IsEmpty ())
	{
		// Stringa vuota
		AfxMessageBox (_TB("Unable to save an unnamed query."), MB_OK|MB_ICONSTOP);
		return FALSE;
	}

	if (IsProgramQueryName (strQueryName))
	{
		// Il nome assegnato coincide con quello della query predefinita
		AfxMessageBox (cwsprintf (_TB("You cannot assign a generic query the same name of the query defined at program level ('{0-%s}')."), GetProgramQueryName()), MB_OK|MB_ICONSTOP);
		return FALSE;
	}
	if (m_pQueriesList && m_pQueriesList->FindQueryName (strQueryName))
	{
		// Il nome inputato è già presente nella lista
		AfxMessageBox (cwsprintf (_TB("The query named '{0-%s}' already exists in the query list.\r\nYou must enter a new name. "), (LPCTSTR)strQueryName), MB_OK | MB_ICONSTOP);
		return FALSE;
	}
	return TRUE;
}

// Ritorna il nome della query inputato nel relativo control
//-----------------------------------------------------------------------------
CString CBarQueryBaseDlg::GetNewName ()
{
	ASSERT_VALID (m_pStrEdtName);
	return m_pStrEdtName->GetValue();
}

///////////////////////////////////////////////////////////////////////////////
// Diagnostics
#ifdef _DEBUG
void CBarQueryBaseDlg::Dump (CDumpContext& dc) const
{
	ASSERT_VALID (this);
	TRACE0 (" CBarQueryBaseDlg\n");
	CParsedDialog::Dump(dc);
}
#endif // _DEBUG

///////////////////////////////////////////////////////////////////////////////
// Implementazione di CBarQueryRenameDlg
///////////////////////////////////////////////////////////////////////////////
//
//	Dialog che permette di rinominare una query. In alcuni casi il nome della
// query identifica anche la tipologia (temporanea o definitiva) per cui oltre
// al semplice cambiamento di nome gestisce anche tutte le trasformazioni
// possibili:
//		- da temp a temp;
//		- da temp a definitiva;
//		- da definitiva a temp;
//		- da definitiva a definitiva;
//	La scelta di utilizzare una finestra apposita per questa operazione è stata
// dettata dal fatto di semplificare il più possibile la gestione delle
// trasformazioni. 
//  L'operazione di rinomina avrà effetto solo se il nome inputato è corretto.
//
///////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
CBarQueryRenameDlg::CBarQueryRenameDlg(CQueriesListBox* pList, AllQueries* pAllQueries)
	:
	CBarQueryBaseDlg (IDD_BARQUERY_QUERY_RENAME, pList, pAllQueries)
{
}

//-----------------------------------------------------------------------------
BOOL CBarQueryRenameDlg::OnInitDialog()
{
	CBarQueryBaseDlg::OnInitDialog();

	// Visualizzo il vecchio valore in uno static
	((CStatic*) GetDlgItem (IDC_BARQUERY_ST_SHOW_NAME))->SetWindowText (GetCurSelQueryName ());

	CenterWindow();
	return TRUE;
}

//-----------------------------------------------------------------------------
void CBarQueryRenameDlg::OnOK ()
{
	// Controllo sul nome
	if (!TestQueryName ())
		return;

	if (GetAllQueries ()->RenameQuery (GetNewName(), GetCurSelQueryObj ()))
		GetQueriesList ()->RenameCurSelQuery ();

	EndDialog (IDOK);
}

///////////////////////////////////////////////////////////////////////////////
// Diagnostics
#ifdef _DEBUG
void CBarQueryRenameDlg::Dump (CDumpContext& dc) const
{
	ASSERT_VALID (this);
	TRACE0(" CBarQueryRenameDlg\n");
	CBarQueryBaseDlg::Dump(dc);
}
#endif // _DEBUG

///////////////////////////////////////////////////////////////////////////////
// CBarQueryBaseEditModeDlg
///////////////////////////////////////////////////////////////////////////////
//
//	Classe base da cui derivano tutte le finestre di dialogo per la modifica e
// per l'inserimento di una nuova query.
//	Gestisce il check box per impostare la corrente query come query di default.
// Esso sarà abilitato solo se il nome attualmente visualizzato nel relativo 
// edit identifica una query definitiva (per una descrizione più approfondita
// sulla sua abilitazione/disabilitazione rimandiamo alla dialog CBarQueryNewDlg
// e CBarQueryModifyDlg).
//	Attraverso questa dialog è possibile definire oltre alle normali query anche
// quelle parametriche (relativo bottone). 
//	Il bottone di 'Test' viene abilitato solo quando è stata apportata una
// qualsiasi modifica all'edit della Where Clause e dell'Order By, mentre quello
// di 'Ok' qualsiasi control presente sulla dialog.
//
//	m_pEdtExpWClause: control per l'editazione dell'espressione di Where Clause;
//						eseguendo un doppio click con il mouse su questo edit
//						compare la finestre per la composizione dell'espressione;
//
//	m_pEdtOrderBy	: control per l'editazione dell'espressione di Order By;
//						eseguendo un doppio click con il mouse su questo edit
//						compare la finestre per la composizione dell'espressione;
//
//	m_pPrgData		: puntatore alla struttura dei parametri; viene definito o è
//						presente solo nel caso di query parametrica;
//
//	m_bPrgDataOwns	: indica se il puntare precedente è di proprietà o meno della
//						dialog (serve per la cancellazione);
//
///////////////////////////////////////////////////////////////////////////////
BEGIN_MESSAGE_MAP(CBarQueryBaseEditModeDlg, CBarQueryBaseDlg)
	//{{AFX_MSG_MAP(CBarQueryBaseEditModeDlg)
		ON_BN_CLICKED (IDC_BARQUERY_BTN_TEST,		OnTest)
		ON_BN_CLICKED (IDC_BARQUERY_BTN_PARAMETERS,	ShowAskRules)

		ON_EN_UPDATE (IDC_BARQUERY_EDT_WCLAUSE,	OnEnableBtnTest)
		ON_EN_UPDATE (IDC_BARQUERY_EDT_ORDERBY,	OnEnableBtnTest)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CBarQueryBaseEditModeDlg::CBarQueryBaseEditModeDlg (UINT nIDD, SqlConnection* pSqlConnection, CQueriesListBox* pList, AllQueries* pAllQueries)
	:
	CBarQueryBaseDlg	(nIDD, pList, pAllQueries),
	m_pSqlConnection	(pSqlConnection),
	m_bPrgDataOwns		(FALSE),
	m_pPrgData			(NULL)
{
	m_pEdtExpWClause= new CExpEdit;
	m_pEdtOrderBy	= new CEqnEdit (NULL, NULL);
}

//-----------------------------------------------------------------------------
CBarQueryBaseEditModeDlg::~CBarQueryBaseEditModeDlg ()
{
	if (m_bPrgDataOwns && m_pPrgData)
		delete m_pPrgData;

	ASSERT (m_pEdtExpWClause);
	ASSERT (m_pEdtOrderBy);

	delete m_pEdtExpWClause;
	delete m_pEdtOrderBy;
}

//-----------------------------------------------------------------------------
BOOL CBarQueryBaseEditModeDlg::OnInitDialog ()
{
	CBarQueryBaseDlg::OnInitDialog();

	// subclass dei controlli di libreria MicroArea
	VERIFY (m_pEdtExpWClause->	SubclassEdit (IDC_BARQUERY_EDT_WCLAUSE,	this));
	VERIFY (m_pEdtOrderBy->		SubclassEdit (IDC_BARQUERY_EDT_ORDERBY,	this));

	// Disabilitazione del pulsate di test
	BtnTestEnableWindow (FALSE);

	if (!m_pAllQueries->m_TableInfo.m_strSelect.IsEmpty())
		GetDlgItem(IDC_BARQUERY_SELECT)->SetWindowText(m_pAllQueries->m_TableInfo.m_strSelect);
	else
		GetDlgItem(IDC_BARQUERY_SELECT)->SetWindowText(_T("*"));

	if (!m_pAllQueries->m_TableInfo.m_strFrom.IsEmpty())
		GetDlgItem(IDC_BARQUERY_FROM)->SetWindowText(m_pAllQueries->m_TableInfo.m_strFrom);
	else
		GetDlgItem(IDC_BARQUERY_FROM)->SetWindowText(m_pAllQueries->GetTableName());

	// Inizializzazione dei parsed control
	ASSERT_VALID (m_pAllQueries);
	m_pEdtExpWClause->	SetTableInfo (*m_pAllQueries->m_TableInfo.m_pTableInfoArray);
	m_pEdtOrderBy->		SetTableInfo (*m_pAllQueries->m_TableInfo.m_pTableInfoArray);
	return TRUE;
}

// Ritorna TRUE se il controllo sintattico della query ha data esito positivo,
// altrimenti FALSE
//-----------------------------------------------------------------------------
BOOL CBarQueryBaseEditModeDlg::TestQuery (CString* pStrWClause /* NULL */)
{
	ASSERT (m_pEdtOrderBy);
	ASSERT (m_pEdtExpWClause);
	ASSERT_VALID (m_pAllQueries);

	// Controllo dl Where Clause
	if (!m_pEdtExpWClause->IsEmpty())
	{
		ASSERT_VALID(m_pAllQueries->m_TableInfo.m_pTableInfoArray);
	    WClause aWC (m_pSqlConnection, m_pPrgData ? m_pPrgData->GetSymTable() : NULL, *m_pAllQueries->m_TableInfo.m_pTableInfoArray);
		aWC.SetNative(IsNativeExpr());
		
		if (!m_pEdtExpWClause->CheckWC(aWC))
		{
			m_pEdtExpWClause->SetCtrlFocus(TRUE);
			return FALSE;
		}

		GetChkBoxNativeExpr()->SetCheck(aWC.IsNative());

		// Se il puntatore è diverso da NULL allora memorizzo la stringa di Filter
		if (pStrWClause)
			*pStrWClause = aWC.ToString(m_pAllQueries->m_pTable);
	}

	// Se la stringa di order by è vuota, estraggo secondo l'ordine della tabella
	if (!m_pEdtOrderBy->IsEmpty())
	{
		CString	strTmp (m_pEdtOrderBy->GetValue());
		ConvertCString (strTmp, CRLF_TO_LF);
		Parser lex (strTmp);

		// controllo sintattico dell'espressione
		if (!ParseOrderBy(lex, m_pPrgData ? m_pPrgData->GetSymTable() : NULL, 
							m_pAllQueries->m_TableInfo.m_pTableInfoArray, strTmp))
		{
			m_pEdtOrderBy->SetCtrlFocus(TRUE);
			return FALSE;
		}
	}

	BtnTestEnableWindow (FALSE);
	return TRUE;
}

// Abilitazione/disabilitazione del bottone di Test
//-----------------------------------------------------------------------------
void CBarQueryBaseEditModeDlg::BtnTestEnableWindow (BOOL bEnable /* TRUE */)
{
	GetDlgItem(IDC_BARQUERY_BTN_TEST)->EnableWindow (bEnable);
}

// Ritorna il Check Box per la gestione della query di default
//-----------------------------------------------------------------------------
CButton* CBarQueryBaseEditModeDlg::GetChkBoxDefault()
{
	return (CButton*)GetDlgItem(IDC_BARQUERY_CKB_SET_DEFAULT);
}

// Ritorna il Check Box per la gestione della espressione nativa SQL
//-----------------------------------------------------------------------------
CButton* CBarQueryBaseEditModeDlg::GetChkBoxNativeExpr()
{
	return (CButton*)GetDlgItem(IDC_BARQUERY_CKB_SET_NATIVE_SQL);
}

//-----------------------------------------------------------------------------
void CBarQueryBaseEditModeDlg::CovertStrOrderByInLF (CString& strNewOrderBy)
{
	strNewOrderBy = m_pEdtOrderBy->GetValue();
	ConvertCString (strNewOrderBy, CRLF_TO_LF);
}

// Visualizzazione delle dialog per la costruzione della query parametrica
//-----------------------------------------------------------------------------
void CBarQueryBaseEditModeDlg::ShowAskRules()
{
	//Se non è presente alloco il Program Data
	if (!m_pPrgData)
	{
		m_bPrgDataOwns	= TRUE;
		m_pPrgData		=  new ProgramData(NULL);
	}

	// Visualizzazione della finestra di dialogo per la definizione dei
	// parametri
	CAskRuleDlg	askRuleDlg (m_pPrgData, NULL);
	if (askRuleDlg.DoModal() != IDOK && m_bPrgDataOwns)
	{
		m_bPrgDataOwns = FALSE;
		delete m_pPrgData;
		m_pPrgData = NULL;
		return;
	}

	ASSERT (m_pEdtExpWClause);
	ASSERT (m_pPrgData);

	// Associo all'edit della Where Clause la tabella dei simboli appena
	// definita (gestisce i parametri)
	m_pEdtExpWClause->SetSymbolTable(m_pPrgData->GetSymTable());
	m_pEdtExpWClause->SetFocus		();
}

// Inizializzazione dei vari control partendo dalla corrente query selezionata
// nella lista presente nella finestra di dialog gestore della fase di editazione
//-----------------------------------------------------------------------------
void CBarQueryBaseEditModeDlg::InitForModify (QueryObj* pQueryObj)
{
	ASSERT_VALID (pQueryObj);
	QueryInfo* pQueryInfo = m_pAllQueries->GetQueryInfo (pQueryObj);
	if (!pQueryInfo)
	{
		ASSERT(FALSE);
		return;
	}
		
	m_pPrgData = pQueryInfo->m_pPrgData;

	GetChkBoxNativeExpr()->SetCheck(pQueryInfo->m_bNativeExpr);

	if (m_pPrgData)
		m_pEdtExpWClause->SetSymbolTable (m_pPrgData->GetSymTable());

	// Conversione delle stringhe per effettuare la corretta visualizzazione
	// a video
	CString strTmp (pQueryInfo->m_TableInfo.m_strFilter);
	ConvertCString(strTmp, LF_TO_CRLF);
	m_pEdtExpWClause-> SetValue (strTmp);

	// Conversione delle stringhe per effettuare la corretta visualizzazione
	// a video
	strTmp = pQueryInfo->m_TableInfo.m_strSort;
	ConvertCString(strTmp, LF_TO_CRLF);
	m_pEdtOrderBy-> SetValue (strTmp);
}

///////////////////////////////////////////////////////////////////////////////
// Diagnostics
#ifdef _DEBUG
void CBarQueryBaseEditModeDlg::Dump (CDumpContext& dc) const
{
	ASSERT_VALID (this);
	AFX_DUMP0(dc, " CBarQueryBaseEditModeDlg\n");
	CBarQueryBaseDlg::Dump(dc);
}
#endif // _DEBUG

///////////////////////////////////////////////////////////////////////////////
// CBarQueryNewDlg
///////////////////////////////////////////////////////////////////////////////
//
//	Permette di costruire una nuova query sia temporanea che definitiva. Ad ogni
// nuova query viene assegnato un nome temporaneo.
//	Il check box, per definire la corrente query come default, inizialmente è
// sempre disabilitato. Esso è agganciato in maniera molto stretta all'edit del
// nome, infatti sulla perdita del fuoco viene eseguito un controllo sul suo
// contenuto per verificare la presenza o meno prefisso di nome temporaneo. Se il
// controllo ha esito positivo il check viene disabilitato in caso contrario
// abilitato.
//	La nuova query viene assegnata automaticamente alle temporanee se l'utente
// non modifica il nome o assegna un nome con prefisso di query temporanea,
// altrimenti, viene inserita tra quelle definitive. In tal caso sulla chiusura
// della finestra (solo su bottone 'Ok') viene automaticamente salvata su file.
//	Il bottone di 'OK' viene abilitato alla prima modifica di un qualsiasi
// control della dialog.
//	Per visualizzare o per avere come base di partenza il contenuto della query
// predefinita, dato che non può essere editata, si è pensato di utilizzare
// questa dialog. Se la query selezionata coincide con quella predefinita e
// utilizzando il bottone di modifica si entra in fase di inserimento di una
// nuova query con l'eccezione che nei control di Order By e di Where Clause
// saranno visualizzati i valori contenuti nella query predefinita.
//	In questo modo salvando i dati della predefinita sono salvaguardati.
//
//	m_bFromModify	: flag utilizzato per indicare che si sta inerendo una
//						nuova query prelevando i valori da quella predefinita.
//
///////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CBarQueryNewDlg, CBarQueryBaseEditModeDlg)
	//{{AFX_MSG_MAP(CBarQueryNewDlg)
		ON_EN_KILLFOCUS (IDC_BARQUERY_EDT_NAME, OnQueryNameKillFocus)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CBarQueryNewDlg::CBarQueryNewDlg (SqlConnection* pSqlConnection, CQueriesListBox* pList, AllQueries* pAllQueries, BOOL bFromModify)
	:
	CBarQueryBaseEditModeDlg(IDD_BARQUERY_QUERY_MODIFY, pSqlConnection, pList, pAllQueries),
	m_bFromModify			(bFromModify)
{
}

//-----------------------------------------------------------------------------
BOOL CBarQueryNewDlg::OnInitDialog ()
{
	CBarQueryBaseEditModeDlg::OnInitDialog();

	// Ricerco il primo nome di query temporanea non ancora assegnato (questo
	// è necessario per evitare di andare a sovrascrivere a query temporanee
	// definite dell'utente con nome uguale a quelle definite al livello di
	// programma)
	CString strQueryName = m_pAllQueries ? m_pAllQueries->GetTempQueryName() : _T("");
	CQueriesListBox* pList = GetQueriesList ();

	while (pList && pList->FindQueryName(strQueryName))
		strQueryName = m_pAllQueries->GetTempQueryName ();

	// Visualizzo il nome della query temporanea e disabilito il check box di
	// query di default (se temporanea non può essere definita come default)
	m_pStrEdtName->SetValue (strQueryName);
	GetChkBoxDefault()->EnableWindow (FALSE);

	if (pList && m_bFromModify)
	{
		// Visualizzazione dei valori della query predefinita
		ASSERT (pList->GetCurSelQueryObj()->IsPredefiniteQuery());
		InitForModify (pList->GetCurSelQueryObj());
	}
	return TRUE;
}

// Abilitazione/disabilitazione del check per definire una query di default.
// Il check viene abilitato solo se il nome della query non contiene il prefisso
// di temporanea.
//-----------------------------------------------------------------------------
void CBarQueryNewDlg::OnQueryNameKillFocus ()
{
	GetChkBoxDefault()->EnableWindow (!IsTempQueryName (GetNewName ()));
}

//-----------------------------------------------------------------------------
void CBarQueryNewDlg::OnOK ()
{
	// Controlla il nome delle query
	if (!TestQueryName ())
		return;

	// Controllo la correttezza sintattica della query
	CString strWClause;
	if (!TestQuery (&strWClause))
		return;
    
    CString strOrderByCov;
	CovertStrOrderByInLF (strOrderByCov);

	SqlTableStruct sts(m_pAllQueries->m_TableInfo);
	sts.m_strFilter = strWClause;
	sts.m_strSort = strOrderByCov;

	// Occorre settare il flag per evitare di cancellare la program data
	m_bPrgDataOwns = FALSE;
	QueryInfo* pQueryInfo = new QueryInfo (m_pSqlConnection, IsNativeExpr(), &sts, m_pPrgData);

	// Aggiungo alla struttura in memoria la nuova query
	QueryObj* pQueryObj = m_pAllQueries->AddNewQuery (GetNewName(), pQueryInfo, IsDefaultQuery());

	// Aggiungo e seleziono la nuova query
	CQueriesListBox* pList = GetQueriesList();
	if (pList)
		pList->SetCurSel (pList->AddQuery (pQueryObj));
	
	EndDialog(IDOK);
}

///////////////////////////////////////////////////////////////////////////////
// Diagnostics
#ifdef _DEBUG
void CBarQueryNewDlg::Dump (CDumpContext& dc) const
{
	ASSERT_VALID (this);
	AFX_DUMP0(dc, " CBarQueryNewDlg\n");
	CBarQueryBaseEditModeDlg::Dump(dc);
}
#endif // _DEBUG

///////////////////////////////////////////////////////////////////////////////
// CBarQueryModifyDlg
///////////////////////////////////////////////////////////////////////////////
//
//	Permette di modificare una query presente in memoria sia temporanea che
// definitiva.
//	Si ricorda che questa dialog non permette di modificare il nome della query,
// per eseguire l'operazione occorre usare la relativa finestra di dialogo.
//	Le query temporanee non potranno mai essere impostate come query di default.
//	In base alla tipologia della query viene abilitato o disabilitato il 
// check box di query di default e sarà attivato solo se la query correntemente
// selezionata è la query di default.
//	Il bottone di 'OK' viene abilitato alla prima modifica di un qualsiasi
// control della dialog.
//	
///////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
CBarQueryModifyDlg::CBarQueryModifyDlg (SqlConnection* pSqlConnection, CQueriesListBox* pList, AllQueries* pAllQueries)
	:
	CBarQueryBaseEditModeDlg (IDD_BARQUERY_QUERY_MODIFY, pSqlConnection, pList, pAllQueries)
{
}

//-----------------------------------------------------------------------------
BOOL CBarQueryModifyDlg::OnInitDialog ()
{
	CBarQueryBaseEditModeDlg::OnInitDialog();

	// Visualizzo i valori della query attualmente selezionata e disabilito
	// l'edit del nome della query (sono in modifica)
	QueryObj* pQueryObj = GetCurSelQueryObj ();
	ASSERT_VALID (pQueryObj);
	ASSERT_VALID (m_pAllQueries);
	m_pStrEdtName->SetValue (pQueryObj->m_strQueryName);
	m_pStrEdtName->EnableWindow (FALSE);

	InitForModify (pQueryObj);

	// Se il nome della query identifica una query temporanea disabilito il
	// check box, altrimenti verifico se coincide con quella impostata come
	// default dall'utente e abilito il check
	if (pQueryObj->m_enumType == Temp)
		GetChkBoxDefault()->EnableWindow (FALSE);
	else
		GetChkBoxDefault()->SetCheck(m_pAllQueries->IsDefaultQuery (pQueryObj));

	// Passo il fuoco al primo edit
	m_pEdtExpWClause->SetFocus ();
	return TRUE;
}

//-----------------------------------------------------------------------------
void CBarQueryModifyDlg::OnOK ()
{
	// Non eseguo il controllo del nome della query in quando è sicuramente
	// corretto visto che non posso editarlo

	// Controllo la correttezza sintattica della query
	CString strWClause;
	if (!TestQuery (&strWClause))
		return;
    
    CString strOrderByCov;
	CovertStrOrderByInLF (strOrderByCov);

	// Aggiorno la struttura in memoria con i nuovi dati della query
	m_pAllQueries->UpdateQuery
		(
			GetCurSelQueryObj (),
			IsNativeExpr(),
			strWClause,
			strOrderByCov,
			IsDefaultQuery(),
			m_pPrgData
		);
	EndDialog(IDOK);
}

///////////////////////////////////////////////////////////////////////////////
// Diagnostics
#ifdef _DEBUG
void CBarQueryModifyDlg::Dump (CDumpContext& dc) const
{
	ASSERT_VALID (this);
	AFX_DUMP0(dc, " CBarQueryModifyDlg\n");
	CBarQueryBaseEditModeDlg::Dump(dc);
}
#endif // _DEBUG

///////////////////////////////////////////////////////////////////////////////
// Implementazione di CBarQueriesDlg
///////////////////////////////////////////////////////////////////////////////
//
//	Gestisce tutte le operazioni che possono essere eseguite su di una query.
// Visualizza in una lista tutte le query attualmente definite per la tabella 
// corrente e ad ogni query associa un bitmap in base al tipo e allo stato in
// cui si trovano. Solo una query alla volta può essere la query di default.
//	Una volta eseguita l'operazione richiesta dall'utente (modificare,
// aggiungere, cancellare e rinominare) verrà settato un flag per memorizzare
// lo stato di struttura dati modificata. In maniera del tutto automatica e
// simmetrica ogni cambiamento effettuato avviene sia in memoria che a video
// (aggiornando la selezione nella lista) e se necessario anche su file.
//	Per visualizzare o per avere come base di partenza il contenuto della query
// predefinita, dato che non può essere editata, occorre utilizzare il bottone
// di modifica. In questo modo si entra in fase di inserimento di una nuova
// query però con i valori inizializzati con quelli presenti nella predefinita.
//	L'esecuzione di una query avviene o tramite il relativo bottone o il doppio
// click sulla lista.
//
//	m_bModified		: flag dello stato di modifica della struttura in memoria;
//
//	m_pAllQueries	: struttura delle query;
//
//	m_pLBQueries	: lista di visualizzazione delle query;
//
///////////////////////////////////////////////////////////////////////////////
IMPLEMENT_DYNAMIC(CBarQueriesDlg, CParsedDialog)
BEGIN_MESSAGE_MAP(CBarQueriesDlg, CParsedDialog)
	//{{AFX_MSG_MAP(CBarQueriesDlg)
		ON_LBN_DBLCLK	(IDC_BARQUERY_LB_QUERIES,	OnExec)
		ON_LBN_SELCHANGE(IDC_BARQUERY_LB_QUERIES,	OnQuerySelChange)

		ON_BN_CLICKED (IDC_BARQUERY_BTN_MODIFY,	OnModify)
		ON_BN_CLICKED (IDC_BARQUERY_BTN_EXEC,	OnExec)
		ON_BN_CLICKED (IDC_BARQUERY_BTN_RENAME,	OnRename)
		ON_BN_CLICKED (IDC_BARQUERY_BTN_NEW,	OnNew)
		ON_BN_CLICKED (IDC_BARQUERY_BTN_DELETE,	OnDelete)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CBarQueriesDlg::CBarQueriesDlg (AllQueries* pAllQueries, BOOL bOnlyExec /*=FALSE*/)
	:
	CParsedDialog	(IDD_BARQUERY_QUERY_MANAGE),
	m_pSqlConnection(NULL),
	m_bModified		(FALSE),
	m_pAllQueries	(pAllQueries),
	m_bOnlyExec		(bOnlyExec)
{
	m_pSqlConnection = pAllQueries->m_pSqlConnection;
	m_pLBQueries = new CQueriesListBox ();
}

//-----------------------------------------------------------------------------
CBarQueriesDlg::~CBarQueriesDlg()
{
	ASSERT (m_pLBQueries);
	delete m_pLBQueries;
} 

//-----------------------------------------------------------------------------
BOOL CBarQueriesDlg::EnableModify	(BOOL bEnable /*= TRUE*/)
{
	return EnableButton(IDC_BARQUERY_BTN_MODIFY, bEnable);
}

//-----------------------------------------------------------------------------
BOOL CBarQueriesDlg::EnableDelete	(BOOL bEnable /*= TRUE*/)
{
	return EnableButton(IDC_BARQUERY_BTN_DELETE, bEnable);
}

//-----------------------------------------------------------------------------
BOOL CBarQueriesDlg::EnableRename	(BOOL bEnable /*= TRUE*/)
{
	return EnableButton(IDC_BARQUERY_BTN_RENAME, bEnable);
}

//-----------------------------------------------------------------------------
BOOL CBarQueriesDlg::EnableNew		(BOOL bEnable/*= TRUE*/)
{
	return EnableButton(IDC_BARQUERY_BTN_NEW, bEnable);
}

//-----------------------------------------------------------------------------
BOOL CBarQueriesDlg::EnableButton (int nIDC, BOOL bEnable /*= TRUE*/)
{
	return GetDlgItem(nIDC)->EnableWindow (bEnable && !m_bOnlyExec);
}

//-----------------------------------------------------------------------------
BOOL CBarQueriesDlg::OnInitDialog()
{
	CParsedDialog::OnInitDialog();

	ASSERT_VALID (m_pLBQueries);
	ASSERT_VALID (m_pAllQueries);

	// Subclass dei control
	VERIFY (m_pLBQueries->SubclassDlgItem (IDC_BARQUERY_LB_QUERIES, this));

	EnableRename ();
	EnableDelete ();
	EnableNew ();
	EnableModify ();

	m_pAllQueries->FillList (m_pLBQueries);
	// Seleziono delle query selezionata
	m_pLBQueries->SelectQueryName (m_pAllQueries->GetSelectedQueryName());
	OnQuerySelChange();
	CenterWindow ();
	return TRUE;
}

//----------------------------------------------------------------------------
void CBarQueriesDlg::OnModify()
{
	ASSERT_VALID (m_pLBQueries);
	if (!CanDoModify())
		return;

	if (m_pLBQueries->GetCurSelQueryObj()->IsPredefiniteQuery())
	{
		// La query selezionata è quella predefinita. Siccome la query 
		// predefinita non può essere modificata, per visualizzare ed editare
		// il suo contenuto, entro in fase di inserimento di una nuova query.
		CBarQueryNewDlg dlg (m_pSqlConnection, m_pLBQueries, m_pAllQueries, TRUE);
		if (dlg.DoModal () != IDOK)
			return;

		OnQuerySelChange();
	}
	else
	{
		// Visualizzazione della dialog per la modifica
		CBarQueryModifyDlg dlg (m_pSqlConnection, m_pLBQueries, m_pAllQueries);
		if (dlg.DoModal () != IDOK)
			return;
	}

	m_pLBQueries->Invalidate();
	m_bModified = TRUE;
}

// Abilitazione/disabilitazione dei bottoni di rinomina e cancella 
// quando la query correntemente selezionata diventa quelle predefinita (che
// può essere solo ed esclusivamente eseguita)
//----------------------------------------------------------------------------
void CBarQueriesDlg::OnQuerySelChange()
{
	ASSERT_VALID (m_pLBQueries);
	BOOL bEnable = !m_pLBQueries->GetCurSelQueryObj()->IsPredefiniteQuery();
	EnableRename (bEnable);
	EnableDelete (bEnable);
}

// Esecuzione della corrente query
//----------------------------------------------------------------------------
void CBarQueriesDlg::OnExec()
{
	ASSERT_VALID (m_pLBQueries);
	ASSERT_VALID (m_pAllQueries);
	m_pAllQueries->m_pSelectedQuery = m_pLBQueries->GetCurSelQueryObj();
	EndDialog (IDC_BARQUERY_BTN_EXEC);
}

// Inserimento di una nuova query
//----------------------------------------------------------------------------
void CBarQueriesDlg::OnNew()
{
	if (!CanDoNew ())
		return;

	// Visualizzazione della dialog per la modifica
	ASSERT_VALID (m_pLBQueries);
	ASSERT_VALID (m_pAllQueries);
	CBarQueryNewDlg dlg (m_pSqlConnection, m_pLBQueries, m_pAllQueries);
	if (dlg.DoModal () != IDOK)
		return;

	OnQuerySelChange();
	m_bModified = TRUE;
}

// Rinomina la query attualmente selezionata
//----------------------------------------------------------------------------
void CBarQueriesDlg::OnRename()
{
	ASSERT_VALID (m_pLBQueries);
	if (!CanDoRename() || m_pLBQueries->GetCurSelQueryObj()->IsPredefiniteQuery())
		return;

	// Visualizzazione della dialog per il rename
	CBarQueryRenameDlg dlg (m_pLBQueries, m_pAllQueries);
	if (dlg.DoModal () == IDOK)
		m_bModified = TRUE;
}

// Cancella la query attualmente selezionata
//----------------------------------------------------------------------------
void CBarQueriesDlg::OnDelete()
{
	ASSERT_VALID (m_pLBQueries);
	if (!CanDoDelete() || m_pLBQueries->GetCurSelQueryObj()->IsPredefiniteQuery())
		return;

	ASSERT_VALID (m_pAllQueries);
	// Cancellazione della query dalla memoria
	if (!m_pAllQueries->DeleteQuery (m_pLBQueries->GetCurSelQueryObj ()))
		return;

	m_pLBQueries->DeleteCurSelQuery ();
	OnQuerySelChange();
	m_bModified = TRUE;
}

//----------------------------------------------------------------------------
BOOL CBarQueriesDlg::IsQuerySelect()
{
	ASSERT_VALID (m_pLBQueries);
	return m_pLBQueries->GetCount() > 0 && m_pLBQueries->GetCurSel () >= 0;
}

///////////////////////////////////////////////////////////////////////////////
// Diagnostics
#ifdef _DEBUG
void CBarQueriesDlg::Dump (CDumpContext& dc) const
{
	ASSERT_VALID(this);
	AFX_DUMP0	(dc, " CBarQueriesDlg\n");
	CParsedDialog::Dump(dc);
}
#endif // _DEBUG

///////////////////////////////////////////////////////////////////////////////
// Implementazione di CQueriesListBox
///////////////////////////////////////////////////////////////////////////////
//
//	List box personalizzata nella quale vengono visualizzate i nomi delle query
// e i bitmap associati che ne identifica il tipo e lo stato.
//	La ricerca e il caricamento dei bitmap, come la creazione ed assegnazione
// del font avviene, nella funzione CQueriesListBox::SubclassDlgItem.
//
//	m_nBmpHeight	:
//	m_nBmpWidth		: altezza e larghezza in pixel dei bitmap da visualizzare;
//
//	m_pFont			: font con cui è rappresentata la parte testo;

//	m_bmpTemp		: bitmap per query temporanee;
//	m_bmpDefault	: bitmap per query di default;
//	m_bmpPredefinite: bitmap per query predefinita;
//	m_bmpOnFile		: bitmap per query definitive;
//
///////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
CQueriesListBox::CQueriesListBox()
	:
	m_nBmpHeight(0),
	m_nBmpWidth	(0),
	m_pFont		(NULL)
{
}

//-----------------------------------------------------------------------------
CQueriesListBox::~CQueriesListBox()
{
	if (m_pFont)
	{
		m_pFont->DeleteObject ();
		delete m_pFont;
	}
	m_bmpTemp.			DeleteObject ();
	m_bmpDefault.		DeleteObject ();
	m_bmpPredefinite.	DeleteObject ();
	m_bmpOnFile.		DeleteObject ();
	m_bmpCerca.		DeleteObject ();
}                                               

//-----------------------------------------------------------------------------
BOOL CQueriesListBox::SubclassDlgItem (UINT nIDC, CWnd* pParent)
{
	if (!CBCGPListBox::SubclassDlgItem (nIDC, pParent))
		return FALSE;

	VERIFY(m_bmpTemp.Attach(LoadBitmapOrPng(TBIcon(szQuery_Temp, TOOLBAR))));
	VERIFY(m_bmpDefault.Attach(LoadBitmapOrPng(TBIcon(szQuery_Defa, TOOLBAR))));
	VERIFY(m_bmpPredefinite.Attach(LoadBitmapOrPng(TBIcon(szQuery_Pred, TOOLBAR))));
	VERIFY(m_bmpOnFile.Attach(LoadBitmapOrPng(TBIcon(szQuery_File, TOOLBAR))));
	VERIFY(m_bmpCerca.Attach(LoadBitmapOrPng(TBIcon(szQuery_Find, TOOLBAR))));

	BITMAP bmInfo;
	VERIFY (m_bmpTemp.GetObject(sizeof(bmInfo), &bmInfo) > 0);
	m_nBmpHeight= bmInfo.bmHeight;
    m_nBmpWidth	= bmInfo.bmWidth;

	// Ottiene dall'applicazione il font con cui rappresentare la parte testo
	LOGFONT lf;
	if (!AfxGetThemeManager()->GetFormFont()->GetObject(sizeof(LOGFONT), (LPTSTR) &lf))
		return FALSE;

	m_pFont = new CFont();
	m_pFont->CreateFontIndirect (&lf);
	SetFont (m_pFont);
	return TRUE;
}

// In base al tipo e allo stato della query ritorna il puntatore al relativo
// bitmap da visualizzare nella lista
//-----------------------------------------------------------------------------
CBitmap* CQueriesListBox::GetQueryObjBitmap (QueryObj* pQueryObj)
{
	AllQueries* pAllQueries = GetAllQueries ();
	ASSERT_VALID (pAllQueries);
	switch (pQueryObj->m_enumType)
	{
		case Temp		: return &m_bmpTemp;
		case Predefinite: 
			return pAllQueries->IsDefaultQuery (pQueryObj) ? &m_bmpDefault : &m_bmpPredefinite;
		default			: 
			if (pQueryObj->m_strQueryName.CompareNoCase(CAbstractFormDoc::QueryNames::SEARCH_QUERYNAME()) == 0)
				return &m_bmpCerca;
			return pAllQueries->IsDefaultQuery (pQueryObj) ? &m_bmpDefault : &m_bmpOnFile;
	}
}

// Aggiunge una nuova query nella lista. Per scelta implementativa una nuova
// query non è mai selezionata
//-----------------------------------------------------------------------------
int CQueriesListBox::AddQuery (QueryObj* pQueryObj)
{
	ASSERT_VALID (pQueryObj);

	int nIdxCB = AddString (pQueryObj->m_strQueryName);
	SetItemDataPtr (nIdxCB, pQueryObj);
	return nIdxCB;
}

// Rimuove la query selezionata dalla lista e seleziona la query di default
//-----------------------------------------------------------------------------
void CQueriesListBox::DeleteCurSelQuery ()
{
	// Cancellazione della query dalla lista
	DeleteString	(GetCurSel ());
	SelectQueryName (GetAllQueries ()->GetDefaultQueryName ());
}

// Rinomina la corrente query selezionata cancellando e poi inserendo il relativo
// entry nella lista
//-----------------------------------------------------------------------------
void CQueriesListBox::RenameCurSelQuery ()
{
	ASSERT (GetCurSel() >= 0);
	
	// Per mantenere la lista ordinata
	QueryObj* pQueryObj = GetCurSelQueryObj ();
	DeleteString(GetCurSel ());
	SetCurSel	(AddQuery (pQueryObj));
}

//-----------------------------------------------------------------------------
void CQueriesListBox::OnDrawItemContent(CDC* pDC, CRect rect, int nIndex)
{
	// Set incon in item
	QueryObj*	pQueryObj = GetQueryObj(nIndex);
	ASSERT(pQueryObj);
	if (pQueryObj)
	{
		CBitmap* pBmp = GetQueryObjBitmap(pQueryObj);
		HICON hIcon = CBitmapToHICON(pBmp);
		SetItemIcon(nIndex, hIcon);
	}

	__super::OnDrawItemContent(pDC, rect, nIndex);
}

//-----------------------------------------------------------------------------
void CQueriesListBox::MeasureItem(LPMEASUREITEMSTRUCT lpMeasureItemStruct)
{
	ASSERT (m_nBmpHeight > 0 && m_nBmpWidth > 0);
	lpMeasureItemStruct->itemHeight = m_nBmpHeight + 2;
}

///////////////////////////////////////////////////////////////////////////////
// Diagnostics
#ifdef _DEBUG
void CQueriesListBox::Dump (CDumpContext& dc) const
{
	ASSERT_VALID(this);
	AFX_DUMP0	(dc, " CQueriesListBox\n");
	CBCGPListBox::Dump(dc);
}
#endif // _DEBUG

