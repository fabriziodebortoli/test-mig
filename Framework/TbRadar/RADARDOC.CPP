
#include "stdafx.h"
#include <ctype.h>
#include <direct.h>

#include <TbGenlibManaged\MExport.h>
#include <TbNameSolver\PathFinder.h>

#include <TbClientCore\ClientObjects.h>
#include <TbWebServicesWrappers\LoginManagerInterface.h>

#include <TbNameSolver\IFileSystemManager.h>

#include <TbGeneric\GeneralFunctions.h>
#include <TbGeneric\globals.h>
#include <TbGeneric\schedule.h>
#include <TbGeneric\FontsTable.h>
#include <TbGeneric\tools.h>
#include <TbGeneric\DataObj.h>
#include <TbGeneric\FormatsTable.h>
#include <TbGeneric\WebServiceStateObjects.h>
#include <TbGeneric\EnumsTable.h>

#include <TbGenlib\messages.h>

#include <TbOledb\oledbmng.h>
#include <TbOledb\sqlrec.h>
#include <TbOledb\sqltable.h>			
#include <TbOledb\sqlaccessor.h>			
#include <TbOledb\sqlcatalog.h>
#include <TbOledb\wclause.h>
#include <TbWoormViewer\expexter.h>
#include <TbWoormViewer\export.h>

#include <TbGes\extdoc.h>
#include <TbGes\hotlink.h>
#include <TbGes\dbt.h>
#include <TbGes\barquery.h>

#include "radardoc.h"
#include "radarfrm.h"
#include "radarvw.h"
#include "radarbdy.h"
#include "radarbar.h"
#include "radardlg.h"

#include "radar.hrc"    //risorse non ancora gestite nel JSON
#include "radar.hjson" //JSON AUTOMATIC UPDATE
#include <TbGes\barquery.hjson> //JSON AUTOMATIC UPDATE
#include <TbGenlib\BaseFrm.hjson> //JSON AUTOMATIC UPDATE
#include <TbGenlib\commands.hrc>

class CExportData;
//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif

//-----------------------------------------------------------------------------
// Dimensione di default del buffer circolare
#define	DEF_BUFFER_SIZE 206//103	// magic number . Deve essere >= di 50 + righe visualizzate  - 3
// Se becco monti lo faccio nuovo.

//-----------------------------------------------------------------------------
// Definizione del file di inizializzazione dei radar presenti nell'applicativo
static const TCHAR szRadarDir [] = _T("radar");
static const TCHAR szRadarExt [] = _T("rad");

// Target del file di inizializzazione del radar
static const TCHAR szTagBuffer		[]	= _T("BufferSize");
static const TCHAR szTagFontStyle		[]	= _T("FontStyle");
static const TCHAR szTagRelease		[]	= _T("Release");
// Attuale release della 'grammatica' del radar
static const TCHAR szCurrentRelease	[]	= _T("0.0.0.5");
static const TCHAR szSTYLE_CELL_NUM	[]=   _T("CellaNumerica");
static const TCHAR szSTYLE_RADAR		[]=   _T("Radar");
static const TCHAR szRadarEndForward	[]=	 _T("RadarEndForward");
static const TCHAR szForms			[]=	 _T("Forms");

//-----------------------------------------------------------------------------
// Funzioni che operano un confronto fra data obj dello stesso tipo nel caso di
// ricerca stupida
static BOOL IsDataObjEqual			(DataObj* p1, DataObj* p2) { return p1->IsEqual				(*p2); }
static BOOL IsDataObjNotEqual		(DataObj* p1, DataObj* p2) { return !p1->IsEqual			(*p2); }
static BOOL IsDataObjGreater		(DataObj* p1, DataObj* p2) { return p1->IsGreaterThan		(*p2); }
static BOOL IsDataObjGreaterEqual	(DataObj* p1, DataObj* p2) { return p1->IsGreaterEqualThan	(*p2); }
static BOOL IsDataObjLess			(DataObj* p1, DataObj* p2) { return p1->IsLessThan			(*p2); }
static BOOL IsDataObjLessEqual		(DataObj* p1, DataObj* p2) { return p1->IsLessEqualThan		(*p2); }

#define SCHEDULER_CONDITIONED(func) if (m_pScheduler->IsStarted()) return func; else { SAFE_DELETE(m_pScheduler); return FALSE; }
///////////////////////////////////////////////////////////////////////////////
// Implementazione di RadarSqlRecCircleList
///////////////////////////////////////////////////////////////////////////////
//
//		Coda circolare per la gestione dei SqlRecord caricati dalla query
// corrente. Gestisce in maniera del tutto automatica il caricamente delle varie
// righe inserendole nelle posizioni corrette per la loro visualizzazione.
//		La struttura viene completamente svuotata ogni volta che si esegue una
// nuova query, una requery o quando si va sul primo o sull'ultimo record
// utilizzando le funzioni OnHome e OnEnd del document.
//		Il caricamento dei record avviene a blocchi di 50 record alla volta e
// si verifica quando l'utente vuole posizionarsi su un record non ancora
// presente in memoria.
//		E' una struttura locale utilizzata esclusivamente dela documento del
// radar (CRadarDoc).
//
//	m_pDoc			: documento di riferimento;
//
//	m_nNrMaxRecords	: numero massimo dei record contenuti nell'array circolare;
//						questo valore può essere modificato inizializzando un
//						opportuno valore nel file .INI dell'applicazione. Più
//						il valore è elevato più le prestazioni di accesso ai
//						dati aumentano, come pure la richiesta di memoria.
//
//	m_nStartIdx		: indica l'indice del primo record nell'array circolare;
//
//	ATTENZIONE	: si ricorda che le operazioni di lettura dei record avvengono
//					solo ed esclusivamente per i record estratti dall query
//					attualmente usata nella	tabella a cui il radar è collegato.
//
///////////////////////////////////////////////////////////////////////////////

//-----------------------------------------------------------------------------
RadarSqlRecCircleList::RadarSqlRecCircleList (CRadarDoc* pDoc)
	:
	m_pDoc			(pDoc),
	m_nNrMaxRecords	(DEF_BUFFER_SIZE),
	m_nStartIdx		(0)
{
}

// Clona il corrente SqlRecord
//-----------------------------------------------------------------------------
SqlRecord* RadarSqlRecCircleList::CloneCallerRec ()
{
	SqlRecord* pRec = (m_pDoc->m_pCallerSqlRecord->Create());
	ASSERT_VALID (pRec);
	pRec->SetConnection(m_pDoc->m_pCallerSqlRecord->GetConnection());

	return pRec;
}

// Ritorna NULL se sono posizionato sull'ultimo record, altrimenti legge e carica
// il nuovo record e ne ritorna il relativo contenuto informativo
//-----------------------------------------------------------------------------
SqlRecord* RadarSqlRecCircleList::ReadNext (int nNewIdx, BOOL bAdd)
{
	SqlRecord* pRec = (SqlRecord*) Array::GetAt(nNewIdx);
	ASSERT_VALID (pRec);

	// Legge dalla tabella in uso il record successivo
	if (MoveNext (pRec))
		return pRec;

    // Fine della tabella, se ero in fase di ADD di un nuovo record devo rimuoverlo,
    // altrimenti ricalcolo il nuovo indice di partenza dell'array circolare, 
	if (bAdd)
		RemoveAt (nNewIdx);
	else if (--m_nStartIdx < 0)
		m_nStartIdx = GetSize() - 1;

	return NULL;
}

// Ritorna NULL se sono posizionato sul primo record, altrimenti legge e carica
// il nuovo record e ne ritorna il relativo contenuto informativo
//-----------------------------------------------------------------------------
SqlRecord* RadarSqlRecCircleList::ReadPrev (int nNewIdx, BOOL bAdd)
{
	SqlRecord* pRec = (SqlRecord*) Array::GetAt(nNewIdx);
	ASSERT_VALID (pRec);

	// Legge dalla tabella in uso il record precedente
	if (MovePrev(pRec))
		return pRec;

    // Inizio della tabella, se ero in fase di ADD di un nuovo record devo
    // rimuoverlo, altrimenti ricalcolo il nuovo indice di partenza dell'array
    // circolare
	if (bAdd)
		RemoveAt (nNewIdx);
	else if (++m_nStartIdx >= GetSize())
		m_nStartIdx = 0;

	return NULL;
}

// Legge il record successivo
//-----------------------------------------------------------------------------
BOOL RadarSqlRecCircleList::MoveNext (SqlRecord* pSqlRec)
{	
	SqlTable* pTable = m_pDoc->m_pTable;
	ASSERT (pTable);
	ASSERT (pTable->IsOpen());

	// Controlla se la tabella è già posizionata sull'ultimo record
	if (pTable->IsEOF())
		return FALSE;

	return pTable->MoveNextCopy(pSqlRec);
}

// Legge il record precedente
//-----------------------------------------------------------------------------
BOOL RadarSqlRecCircleList::MovePrev (SqlRecord* pSqlRec)
{	
	SqlTable* pTable = m_pDoc->m_pTable;
	ASSERT (pTable);

	// Controlla se la tabella è già posizionata sul primo record
	if (pTable->IsBOF())
		return FALSE;

	return pTable->MovePrevCopy(pSqlRec);
}

// Dato la posizione del record e l'indice della colonna ne ritorna il DataObj
//-----------------------------------------------------------------------------
DataObj* RadarSqlRecCircleList::GetDataObj (int nIdxRec, int nIdxFld)
{
	return ((SqlRecord*) GetAt(nIdxRec))->GetDataObjAt(nIdxFld);
}

// Dato un indice ritorna il relativo SqlRecord.
//-----------------------------------------------------------------------------
SqlRecord* RadarSqlRecCircleList::GetAt (int nIdx)
{
	// Controlla se l'array circolare è attivo
	if (GetSize() == m_nNrMaxRecords)
	{
		// Rimappatura dell'indice
		nIdx += m_nStartIdx;
		if (nIdx >= GetSize())
			nIdx -= GetSize();
	}

	if (nIdx >= GetSize() || nIdx < 0)
		return NULL;

	return (SqlRecord*) Array::GetAt(nIdx);
}

// Inserisce in coda alla lista circolare il record letto dalla query corrente
// nella direnzione avanti (successivo - forward direction).
//-----------------------------------------------------------------------------
SqlRecord* RadarSqlRecCircleList::InsertNext ()
{
	// Controlla se l'array circolare è attivo
	if (GetSize() < m_nNrMaxRecords)
		return ReadNext (Add (CloneCallerRec()), TRUE);

	// Rimappatura degli indici
	int nNewIdx = m_nStartIdx;
	if ((++m_nStartIdx) >= GetSize())
		m_nStartIdx = 0;

	// Lettura e inserimento di un nuovo SqlRecord (forward direction)
	return ReadNext (nNewIdx, FALSE);
}

// Inserisce in testa alla lista circolare il record letto dalla query corrente
// nella direnzione indietro (precedente - backward direction)..
//-----------------------------------------------------------------------------
SqlRecord* RadarSqlRecCircleList::InsertPrev ()
{
	// Controlla se l'array circolare è attivo
	if (GetSize() < m_nNrMaxRecords)
	{
		// Lettura e aggiunta di un nuovo SqlRecord (backward direction)
		Array::InsertAt (0, (CObject*) CloneCallerRec (), 1);
		return ReadPrev (0, TRUE);
	}

	// Rimappatura degli indici
	if (--m_nStartIdx < 0)
		m_nStartIdx = GetUpperBound();

	// Lettura e inserimento di un nuovo SqlRecord (backward direction)
	return ReadPrev (m_nStartIdx, FALSE);
}

// Carica i record partendo dall'ultimo indietro (backward direction).
// Se il numero di record caricati è minore degli oggetti già allocati nell'array,
// quelli in eccedenza vengono cancellati. Altrimenti si allocano nuovi record.
//-----------------------------------------------------------------------------
BOOL RadarSqlRecCircleList::LoadFromBottom ()
{
	int				nLastIdx= GetUpperBound();
	SqlTable*	pTable	= m_pDoc->m_pTable;

	ASSERT (GetSize());
	ASSERT (pTable->IsOpen());

	m_nStartIdx = 0;

	// Si posiziona sull'ultimo e lo carica nell'array
	pTable->MoveLastCopy (GetAt (nLastIdx));
	if (pTable->IsEmpty())
		return FALSE;

	// Riempie gli oggetti gia allocati partendo dall'ultimo record della tabella
	nLastIdx--;
	int nIdx = 1;
	for (nIdx = 1; nIdx < GetSize(); nIdx++, nLastIdx--)
		if (MovePrev(GetAt (nLastIdx)) == NULL)
		{
			// Rimuove gli oggetti già allocati e non più utilizzati
			if (nLastIdx > 0)
				RemoveAt(0, nLastIdx);
			return TRUE;
		}

	// Legge e aggiunge nuovi record nell'array
	for (; nIdx < m_nNrMaxRecords; nIdx++)
		if (InsertPrev() == NULL)
			break;

	return TRUE;
}

// Carica i record partendo dal primo in avanti (forward direction).
// Se il numero di record caricati è minore degli oggetti già allocati nell'array,
// quelli in eccedenza vengono cancellati. Altrimenti si allocano nuovi record.
//-----------------------------------------------------------------------------
BOOL RadarSqlRecCircleList::LoadFromTop ()
{
	// Se la query precedente è vuota devo ricaricare l'array circolare
	if (GetSize() == 0)
		Add (CloneCallerRec());

	SqlTable* pTable = m_pDoc->m_pTable;
	ASSERT (pTable->IsOpen());

	// Si posiziona sul primo e lo carica nell'array
	m_nStartIdx = 0;
    pTable->MoveFirstCopy (GetAt (0));
	if (pTable->IsEmpty())
		return FALSE;

	// Riempie gli oggetti gia allocati nell'array
	int nIdx = 1;
	for (nIdx = 1; nIdx < GetSize(); nIdx++)
		if (MoveNext(GetAt (nIdx)) == NULL)
		{
			// Rimuove gli oggetti già allocati e non più utilizzati
			RemoveAt(nIdx, GetSize() - nIdx);
			return TRUE;
		}

	// Legge e alloca nuovi record
	for (; nIdx < m_nNrMaxRecords; nIdx++)
		if (InsertNext() == NULL)
			break;
	return TRUE;
}

// Sposta il cursore della query nella posizione corretta a fronte di un cambio
// di direzione di lerttura. ES: nella find stupida se scorro il file in avanti ma
// eseguo un ricerca all'indietro devo posizionare il cursore in corrispondenza del
// primo SqlRecord letto, e da qui continuare la ricerca sul RecordSet.
//-----------------------------------------------------------------------------
void RadarSqlRecCircleList::SkipRows (BOOL bNextDirection)
{
	SqlTable* pTable = m_pDoc->m_pTable;
	ASSERT (pTable);
	ASSERT (pTable->IsOpen());


	// problema nato quando i dati estratti sono esattamente della stessa
	// dimensione del buffer di appoggio della lista (m_nNrMaxRecords) (an.#10.323)
	int nSkipRows = GetSize();

	// il secondo parametro rappresenta il numero di righe di cui effettuare lo skip e non il numero
	// di righe da scorrere. 
	// Ad esempio se é 2 significa che devo skippare 2 righe e andare alla terza riga 
	if (bNextDirection)
	{
		if (nSkipRows != m_nNrMaxRecords)
			nSkipRows = pTable->IsBOF() ? GetSize()-1 : GetUpperBound()-2;

		pTable->Move(SqlTable::E_MOVE_NEXT, nSkipRows);
	}
	else
	{
		if (nSkipRows != m_nNrMaxRecords)
			nSkipRows = pTable->IsEOF() ? GetSize() : GetUpperBound()-1;

		pTable->Move(SqlTable::E_MOVE_PREV, nSkipRows);
	}
}

///////////////////////////////////////////////////////////////////////////////
// Diagnostics
#ifdef _DEBUG
void RadarSqlRecCircleList::Dump (CDumpContext& dc) const
{
	ASSERT_VALID (this);
	AFX_DUMP0(dc, " RadarSqlRecCircleList\n");
	Array::Dump(dc);
}
#endif // _DEBUG

///////////////////////////////////////////////////////////////////////////////
// class CRadarDoc
///////////////////////////////////////////////////////////////////////////////
//
//
//
///////////////////////////////////////////////////////////////////////////////

//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CRadarDoc, CAbstractDoc)

BEGIN_MESSAGE_MAP(CRadarDoc, CAbstractDoc)
	//{{AFX_MSG_MAP(CRadarDoc)
	ON_COMMAND (ID_RADAR_ORDER_ASCE,	OnOrderByAsc)
	ON_COMMAND (ID_RADAR_ORDER_DESC,	OnOrderByDesc)
	ON_COMMAND (ID_TOOLBAR_FIND_PREV,		OnFindPrev)
	ON_COMMAND (ID_TOOLBAR_FIND_NEXT, OnFindNext)
	ON_COMMAND (ID_RADAR_FIND_START,	OnFindStartDlg)
	ON_COMMAND (ID_RADAR_FIND_STOP,		OnFindStop)
	ON_COMMAND (ID_RADAR_EDIT,			OnEditRecord)
	ON_COMMAND (ID_RADAR_NEW,			OnNewRecord)
	ON_COMMAND (ID_RADAR_DELETE,		OnDeleteCols)
	ON_COMMAND (ID_RADAR_INSERT,		OnInsertCols)
	ON_COMMAND (ID_RADAR_SAVE,			OnSaveGrpAttrib)
	ON_COMMAND (ID_RADAR_SHIFT_LEFT,	OnShiftColLeft)
	ON_COMMAND (ID_RADAR_SHIFT_RIGHT,	OnShiftColRight)
	ON_COMMAND (ID_RADAR_RESIZE_COL,	OnResizeColumns)
	ON_COMMAND (ID_RADAR_HOME,			OnHome)
	ON_COMMAND (ID_RADAR_PAGE_UP,		OnPageUp)
	ON_COMMAND (ID_RADAR_ROW_UP,		OnRowUp)
	ON_COMMAND (ID_RADAR_ROW_DOWN,		OnRowDown)
	ON_COMMAND (ID_RADAR_PAGE_DOWN,		OnPageDown)
	ON_COMMAND (ID_RADAR_END,			OnEnd)
	ON_COMMAND (ID_RADAR_ESCAPE,		OnEscape)
	ON_COMMAND (ID_RADAR_EXEC_QUERY,	OnExecQuery)
	ON_COMMAND (ID_RADAR_EDIT_QUERY,	OnEditQuery)
	ON_COMMAND (ID_TOOLBAR_FIND_EDITBOX,	OnEditBoxQuery)
	
	ON_COMMAND			(ID_EXDOC_TAB_SWITCH, OnTabSwitch)
	ON_COMMAND_RANGE    (ID_EXDOC_TAB_SWITCH_MENU_START, (UINT)(ID_EXDOC_TAB_SWITCH_MENU_END), OnTabSwitchRange)

	ON_UPDATE_COMMAND_UI(ID_RADAR_FIND_START, OnUpdateFindStart)
	ON_UPDATE_COMMAND_UI (ID_RADAR_FIND_STOP,	OnUpdateFindStop)
	ON_UPDATE_COMMAND_UI (ID_RADAR_EDIT,		OnUpdateEditRecord)
	ON_UPDATE_COMMAND_UI (ID_RADAR_NEW,			OnUpdateNewRecord)
	ON_UPDATE_COMMAND_UI (ID_RADAR_HOME,		OnUpdateHome)
	ON_UPDATE_COMMAND_UI (ID_RADAR_PAGE_UP,		OnUpdatePageUp)
	ON_UPDATE_COMMAND_UI (ID_RADAR_ROW_UP,		OnUpdatePageUp)
	ON_UPDATE_COMMAND_UI (ID_RADAR_ROW_DOWN,	OnUpdateRowDown)
	ON_UPDATE_COMMAND_UI (ID_RADAR_PAGE_DOWN,	OnUpdatePageDown)
	ON_UPDATE_COMMAND_UI (ID_RADAR_END,			OnUpdateEnd)
	ON_UPDATE_COMMAND_UI (ID_RADAR_DELETE,		OnUpdateDeleteCols)
	ON_UPDATE_COMMAND_UI (ID_RADAR_INSERT,		OnUpdateInsertCols)
	ON_UPDATE_COMMAND_UI (ID_RADAR_SAVE,		OnUpdateSaveGrpAttrib)
	ON_UPDATE_COMMAND_UI (ID_RADAR_SHIFT_LEFT,	OnUpdateShiftColLeft)
	ON_UPDATE_COMMAND_UI (ID_RADAR_SHIFT_RIGHT,	OnUpdateShiftColRight)
	ON_UPDATE_COMMAND_UI (ID_RADAR_EXEC_QUERY,	OnUpdateExecQuery)
	ON_UPDATE_COMMAND_UI (ID_RADAR_EDIT_QUERY,	OnUpdateEditQuery)
	ON_UPDATE_COMMAND_UI (ID_RADAR_ORDER_ASCE,	OnUpdateOrderBy)
	ON_UPDATE_COMMAND_UI (ID_RADAR_ORDER_DESC,	OnUpdateOrderBy)

	ON_UPDATE_COMMAND_UI (ID_EXDOC_TAB_SWITCH, OnUpdateTabSwitch)

	ON_COMMAND (ID_RADAR_EXCEL,	ExportTableToExcel )
	ON_COMMAND (ID_RADAR_ODS,	ExportTableToODS)

	ON_UPDATE_COMMAND_UI (ID_RADAR_EXCEL,	OnUpdateExport)
	ON_UPDATE_COMMAND_UI (ID_RADAR_ODS,	OnUpdateExport)


	FIND_BAR_DROPDOWN_MENU_ON_UPDATE_COMMAND_UI(OnUpdateFindStart, OnUpdateNext, OnUpdatePrev, OnUpdateFindEdit)
	FIND_BAR_DROPDOWN_MENU_ON_UPDATE_COMMAND()

	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

// class CRadarDoc construction/destruction
//-----------------------------------------------------------------------------
CRadarDoc::CRadarDoc()
	:
	m_bEnableSave	 	(FALSE),
	m_bLinkRadar		(FALSE),
	m_bNextDirection	(TRUE),
	m_bDocumentCalled	(FALSE),
	m_bFirstRun			(TRUE),
	m_nFirstIdxSqlRec	(0),
	m_nTimerID			(0),
	m_nFindTic			(0),
	m_nIdxFontStyle		(0),
	m_nRowCount			(0),
	m_pTable			(NULL),
	m_pQueryManager		(NULL),
	m_pScheduler		(NULL),
	m_pFindCond			(NULL),
	m_pRadarBody		(NULL),
	m_pSqlRecCircleList	(NULL),
	m_pDeletedColumns	(NULL),
	m_pColsToInsert		(NULL),
	m_strFontStyle		(""),
	m_strRadarFile		(""),
	m_bEditingQuery		(FALSE),
	m_bIsSaving			(FALSE),
	m_iFindCondition	(ECompareType::CMP_EQUAL)
{
	// gestisce le query del documento (predefinite e create da user)
	m_pQueryManager = new CQueryManager(this, ID_RADAR_EXEC_QUERY);
	SetDocAccel (IDR_RADAR_ACCELERATOR);
}

//-----------------------------------------------------------------------------
CRadarDoc::~CRadarDoc()
{
	ASSERT_VALID(this);
	ASSERT(m_pScheduler == NULL);
	ASSERT(m_pQueryManager);

	if (m_pTable)
	{
		ASSERT_VALID(m_pTable);
		// Se la tabella è ancora aperta viene chiusa
		if (m_pTable->IsOpen())
			m_pTable->Close ();
	
		// Se sono stato chiamato dall'HotLink non posso cancellare la tabella in
		// quanto è di sua proprietà.
		if (m_pCallerHotLink == NULL)
			SAFE_DELETE(m_pTable);
	}

	// elimina il gestore di query
	SAFE_DELETE(m_pQueryManager);

	// Cancellazione del SqlRecord creato nel metodo di Attach(pClass, pRecordClass)
	SAFE_DELETE(m_pDynamicSqlRecord);

	// Notifico al documento o all'HotLink collegato l'avvenuta morte
	OnRadarDied ();

	if (m_pColsToInsert		) delete m_pColsToInsert;
	if (m_pDeletedColumns	) delete m_pDeletedColumns;
	if (m_pFindCond			) delete m_pFindCond;
	if (m_pRadarBody		) delete m_pRadarBody;
	if (m_pSqlRecCircleList	) delete m_pSqlRecCircleList;

	ASSERT(_CrtCheckMemory());
}

//-----------------------------------------------------------------------------
void CRadarDoc::OnTabSwitch()
{
	CRadarFrame* pRadarFrame = GetRadarFrame();
	if (pRadarFrame)
	{
		CTBTabbedToolbar* pTabbed = pRadarFrame->GetTabbedToolBar();
		if (pTabbed)
		{
			pTabbed->MoveNextTab();
		}
	}
}

//-----------------------------------------------------------------------------
void CRadarDoc::OnTabSwitchRange(UINT nID)
{
	int nPos = nID - ID_EXDOC_TAB_SWITCH_MENU_START;
	CRadarFrame* pRadarFrame = GetRadarFrame();
	if (pRadarFrame)
	{
		CTBTabbedToolbar* pTabbed = pRadarFrame->GetTabbedToolBar();
		if (pTabbed)
		{
			pTabbed->SetActiveTabByPos(nPos);
		}
	}
}

// Ritorna TRUE se il percorso su cui salvare l'attuale configurazione
// grafica esite o è stato creato correttamente altrimenti FALSE. In tal caso
// il file di configurazione viene salvato nella directory principale
// dell'applicazione
//----------------------------------------------------------------------------
BOOL CRadarDoc::CheckRadarFullFileName(BOOL bFromSave /*FALSE*/)
{
	if (m_strRadarFile.IsEmpty() || bFromSave)
	{
		// Home Directory
		CTBNamespace aRadNamespace;

		if (m_pCallerDoc)
		{
			ASSERT_VALID(m_pCallerDoc);
			aRadNamespace = m_pCallerDoc->GetNamespace();
		}
		else if (m_pCallerHotLink)
		{
			ASSERT_VALID(m_pCallerHotLink);
			if (!m_pCallerHotLink->GetAddOnFlyNamespace().IsEmpty())
				aRadNamespace = m_pCallerHotLink->GetAddOnFly();
			else if (m_pCallerHotLink->GetAttachedDocument())
				aRadNamespace = m_pCallerHotLink->GetAttachedDocument()->GetNamespace();
			else
			{
				// se non ho un namespace, non salvo le personalizzazioni
				m_strRadarFile = _T("");
				return TRUE;
			}
		}
		else
		{
			// se non ho un namespace, non salvo le personalizzazioni
			m_strRadarFile = _T("");
			return TRUE;
		}

		if (aRadNamespace.IsValid() && aRadNamespace.GetType() == CTBNamespace::DOCUMENT)
		{
			m_strRadarFile = AfxGetPathFinder ()->GetDocumentRadarPath (aRadNamespace, CPathFinder::USERS, AfxGetLoginInfos()->m_strUserName, bFromSave);
		}
		else
		{
			m_strRadarFile = _T("");
			return TRUE;
		}

		if (bFromSave && !ExistPath (m_strRadarFile) && CreateDirectory (m_strRadarFile) != 0)
		{
			// Non è stato possibile creare la nuova directory
			AfxMessageBox (cwsprintf (_TB("Unable to create the directory:\r\n'{0-%s}'\r\nto save the radar configuration files.\r\nMake sure you have the required access rights."), (LPCTSTR)m_strRadarFile), MB_OK | MB_ICONSTOP);
			m_strRadarFile.Empty();
			return FALSE;
		}

		// Il path non è ancora stato creato
		ASSERT_VALID (m_pCallerSqlRecord);
		
		// Composizione del path completo
		m_strRadarFile = MakeFilePath (m_strRadarFile, m_pCallerSqlRecord->GetTableName(), szRadarExt);

		IFileSystemManager* pFileSystemManager = AfxGetFileSystemManager();
		if (pFileSystemManager && pFileSystemManager->IsManagedByAlternativeDriver(m_strRadarFile))
			return bFromSave ? TRUE : LoadFromVirtualDriver();		
		
		m_strTempRadarFile = m_strRadarFile;
	}

	return TRUE;
}

// Ritorna TRUE se la stringa letta della sezione e dal tag indicati, dal file
// m_strRadarFile (file di configurazione grafica) esiste, altrimenti FALSE.
// Il valore delle stessa viene ritornata come parametro.
//-----------------------------------------------------------------------------
BOOL CRadarDoc::GetRadarProfileString (LPCTSTR lpszSection, LPCTSTR lpszEntry, LPCTSTR lpszDefault, CString& strString) 
{
	if (!CheckRadarFullFileName() || m_strRadarFile.IsEmpty())
		return FALSE;

	if (m_strTempRadarFile.IsEmpty ())
		m_strTempRadarFile = GetTempName();

	TCHAR* pszBuffer	= strString.GetBuffer (4096);
	::GetPrivateProfileString (lpszSection, lpszEntry, lpszDefault, pszBuffer, 255, m_strTempRadarFile);
	strString.ReleaseBuffer ();	
	return !strString.IsEmpty();
}

// Ritorna il valore dell'intero letto della sezione e dal tag indicati, dal file
// m_strRadarFile (file di configurazione grafica).
//-----------------------------------------------------------------------------
int CRadarDoc::GetRadarProfileInt (LPCTSTR lpszSection, LPCTSTR lpszEntry, int nDefault) 
{
	return CheckRadarFullFileName()
			? ::GetPrivateProfileInt(lpszSection, lpszEntry, nDefault,	m_strTempRadarFile)
			: nDefault;
}

// Ricerca un DatoObj (AD ESCLUSIONE DELLE STRINGHE) nelle direzione AVANTI.
// Controlla se la direzione di ricerca coincide con quella di scansione,
// altrimenti la inverte, ricerca il DataObj all'interno della lista circolare
// e poi se non è stato trovato scandisce l'intera tabella.
//-----------------------------------------------------------------------------
BOOL CRadarDoc::FindDataObjNext ()
{
	// Ricerca all'interno dell'array circolare
	for (int nRec = m_pRadarBody->m_nActiveGrpRow + 1 + m_nFirstIdxSqlRec; nRec < GetSizeCircleList(); nRec++)
	{
		SqlRecord* pRec = TicGetSqlRecord (nRec);
		for (int nIdx = 0; nIdx < m_pFindCond->GetSizeSelCols(); nIdx++)
		{
			DataObj* pDataObj = pRec->GetDataObjAt(GetColumnIdxFindCond(nIdx));
			if (pfTestData(pDataObj, m_pFindCond->m_pDataObj) || !m_pScheduler->IsStarted() || m_pScheduler->IsAborted())
				SCHEDULER_CONDITIONED(FindNextRec(nRec))
		}
	}

	// Controllo della attuale direzione di scansione
	if (!m_bNextDirection)
		InvertCursorDirection(); // posizionamento del cursore

	// Ricerca all'interno della tabella
	for (SqlRecord* pRec = TicInsertNextSqlRec(); pRec != NULL; pRec = TicInsertNextSqlRec())
		for (int nIdx = 0; nIdx < m_pFindCond->GetSizeSelCols(); nIdx++)
		{
			DataObj* pDataObj = pRec->GetDataObjAt(GetColumnIdxFindCond(nIdx));
			if (pfTestData(pDataObj, m_pFindCond->m_pDataObj) || !m_pScheduler->IsStarted() || m_pScheduler->IsAborted())
				SCHEDULER_CONDITIONED(FindNextLoadGrpTable())
		}

	// DataObj non trovato mi posizione in fondo alla Tabella.
	GoEnd();
	return FALSE;
}

// Ricerca un DatoObj (AD ESCLUSIONE DELLE STRINGHE) nelle direzione INDIETRO.
// Controlla se la direzione di ricerca coincide con quella di scansione,
// altrimenti la inverte, ricerca il DataObj all'interno della lista circolare
// e poi se non è stato trovato scandisce l'intera tabella.
//-----------------------------------------------------------------------------
BOOL CRadarDoc::FindDataObjPrev ()
{
	// Ricerca all'interno dell'array circolare
	for (int nRec = m_pRadarBody->m_nActiveGrpRow - 1 + m_nFirstIdxSqlRec; nRec >= 0; nRec--)
	{
		SqlRecord* pRec = TicGetSqlRecord (nRec);
		for (int nIdx = 0; nIdx < m_pFindCond->GetSizeSelCols(); nIdx++)
		{
			DataObj* pDataObj = pRec->GetDataObjAt(GetColumnIdxFindCond(nIdx));
			if (pfTestData(pDataObj, m_pFindCond->m_pDataObj) || !m_pScheduler->IsStarted() || m_pScheduler->IsAborted())
				SCHEDULER_CONDITIONED(FindPrevRec (nRec))
		}
	}

	// Controllo della attuale direzione di scansione
	if (m_bNextDirection)
		InvertCursorDirection(FALSE);

	// Ricerca all'interno della tabella
	for (SqlRecord* pRec = TicInsertPrevSqlRec(); pRec != NULL; pRec = TicInsertPrevSqlRec())
		for (int nIdx = 0; nIdx < m_pFindCond->GetSizeSelCols(); nIdx++)
		{
			DataObj* pDataObj = pRec->GetDataObjAt(GetColumnIdxFindCond(nIdx));
			if (pfTestData(pDataObj, m_pFindCond->m_pDataObj) || !m_pScheduler->IsStarted() || m_pScheduler->IsAborted())
				SCHEDULER_CONDITIONED(FindPrevLoadGrpTable())
		}

	// DataObj non trovato mi posizione in testa alla Tabella.
	GoHome ();
	return FALSE;
}

// Il DatoObj (AD ESCLUSIONE DELLE STRINGHE) ricercato con la Find stupida è stato
// trovato all'interno dell'array circolare scandendolo in direzione avanti. Se il
// record è all'interno della tabella dei dati grafice aggiorno solo l'indice
// della riga attiva e della prima riga visualizzata, altrimenti aggiorno suo
// il contenuto con i record letti dall'array circolare.
//-----------------------------------------------------------------------------
BOOL CRadarDoc::FindNextRec(int nRec)
{
	// Aggionamento dei vari indici 
	int nVisibleRows = GetRadarView ()->GetVisibleRows ();
	if (nRec < (m_nFirstIdxSqlRec + m_pRadarBody->GetGrpTableNrRows()))
	{
		// Mi trovo all'interno della tabella dei dati grafici e aggiorno gli indici
		// della nuova riga attiva e della prima riga visualizzata
		m_pRadarBody->SetActiveAndFirstIdxGrpRow (FROM_FIND_NEXT, nVisibleRows, nRec - m_nFirstIdxSqlRec);
		return TRUE;
	}

	// Caricamento dei record nell'array circolare (aggiungendo in Coda)
	for (int nIdx = GetLastIdxSqlRec(); nIdx <= nRec; nIdx++)
	{
		AddRowTailGrpTable (nIdx);
		m_nFirstIdxSqlRec++;
	}

	// Aggiornamento degli indici all'interno della tabella dei dati grafici
	m_pRadarBody->SetActiveAndFirstIdxGrpRow (START_FROM_BOTTOM, nVisibleRows);
	return TRUE;
}

// Il DatoObj (AD ESCLUSIONE DELLE STRINGHE) ricercato con la Find stupida è stato
// trovato all'interno della tabella scandendola in direzione AVANTI. Carica i record
// nell'array circolare e formatta solo i record necessari alla visualizzazione.
//-----------------------------------------------------------------------------
BOOL CRadarDoc::FindNextLoadGrpTable()
{
	// Aggiornamento indice
	m_nFirstIdxSqlRec = GetSizeCircleList() >= m_pRadarBody->m_nNrMaxFormatRows
							? GetSizeCircleList() - m_pRadarBody->m_nNrMaxFormatRows
							: 0;
			
	// Caricamento dei record nell'array circolare (aggiungendo in testa) ed eventuale
	// loro formattazione
	for (int nIdx = m_nFirstIdxSqlRec; nIdx < GetSizeCircleList(); nIdx++)
		AddRowTailGrpTable (nIdx);

	// Aggiornamento degli indici all'interno della tabella dei dati grafici
	m_pRadarBody->SetActiveAndFirstIdxGrpRow (START_FROM_BOTTOM, GetRadarView ()->GetVisibleRows ());
	return TRUE;
}

// Il DatoObj (AD ESCLUSIONE DELLE STRINGHE) ricercato con la Find stupida è stato
// trovato all'interno dell'array circolare scandendolo in direzione INDIETRO. Se il
// record è all'interno della tabella dei dati grafice aggiorno solo l'indice
// della riga attiva e della prima riga visualizzata, altrimenti aggiorno suo
// il contenuto con i record letti dall'array circolare.
//-----------------------------------------------------------------------------
BOOL CRadarDoc::FindPrevRec(int nRec)
{
	int nVisibleRows = GetRadarView ()->GetVisibleRows ();
	// Aggionamento dei vari indici
	if (nRec >= m_nFirstIdxSqlRec)
	{
		// Mi trovo all'interno della tabella dei dati grafici e aggiorno gli indici
		// della nuova riga attiva e della prima riga visualizzata
		m_pRadarBody->SetActiveAndFirstIdxGrpRow (FROM_FIND_PREV, nVisibleRows, nRec - m_nFirstIdxSqlRec);
	 	return TRUE;
	}

	// Caricamento dei record nell'array circolare (aggiungendo in Testa) 
	for (int nIdx = m_nFirstIdxSqlRec - 1; nIdx >= nRec; nIdx--)
	{
		AddRowHeadGrpTable (nIdx);
		m_nFirstIdxSqlRec--;
	}

	// Aggiornamento degli indici all'interno della tabella dei dati grafici
	m_pRadarBody->SetActiveAndFirstIdxGrpRow (START_FROM_TOP, nVisibleRows);
	return TRUE;
}

// Il DatoObj (AD ESCLUSIONE DELLE STRINGHE) ricercato con la Find stupida è stato
// trovato all'interno della tabella scandendola in direzione INDIETRO. Carica i record
// nell'array circolare e formatta solo i record necessari alla visualizzazione.
//-----------------------------------------------------------------------------
BOOL CRadarDoc::FindPrevLoadGrpTable()
{
	// Aggiornamento indice
	m_nFirstIdxSqlRec = 0;

	// Caricamento dei record nell'array circolare (aggiungendo in testa) ed eventuale
	// loro formattazione
	int nLastRec = min (GetSizeCircleList(), m_pRadarBody->m_nNrMaxFormatRows);
	for (int nIdx = m_nFirstIdxSqlRec; nIdx < nLastRec; nIdx++)
		 AddRowTailGrpTable (nIdx);

	// Aggiornamento degli indici all'interno della tabella dei dati grafici
	m_pRadarBody->SetActiveAndFirstIdxGrpRow (START_FROM_TOP, GetRadarView ()->GetVisibleRows ());
	return TRUE;
}

// Ricerca una STRINGA nelle direzione AVANTI nei soli cmapi DataStr.
// Controlla se la direzione di ricerca coincide con quella di scansione,
// altrimenti la inverte, ricerca il DataObj all'interno della lista circolare
// e poi se non è stato trovato scandisce l'intera tabella.
//-----------------------------------------------------------------------------
BOOL CRadarDoc::FindStrNext (CString& strFind)
{
	// Ricerca all'interno dell'array circolare
	for (int nRec = m_pRadarBody->m_nActiveGrpRow + 1 + m_nFirstIdxSqlRec; nRec < GetSizeCircleList(); nRec++)
	{
		SqlRecord* pRec = TicGetSqlRecord (nRec);
		for (int nIdx = 0; nIdx < m_pFindCond->GetSizeSelCols(); nIdx++)
		{
			DataObj* pDataObj = pRec->GetDataObjAt(GetColumnIdxFindCond(nIdx));
			if (IsStringFind (pDataObj->Str(), strFind) || !m_pScheduler->IsStarted() || m_pScheduler->IsAborted())
				SCHEDULER_CONDITIONED(FindNextRec(nRec))
		}
	}

	// Controllo della attuale direzione di scansione
	if (!m_bNextDirection)
		InvertCursorDirection();

	// Ricerca all'interno della tabella
	for (SqlRecord* pRec = TicInsertNextSqlRec(); pRec != NULL; pRec = TicInsertNextSqlRec())
		for (int nIdx = 0; nIdx < m_pFindCond->GetSizeSelCols(); nIdx++)
		{
			DataObj* pDataObj = pRec->GetDataObjAt(GetColumnIdxFindCond(nIdx));
			if (IsStringFind (pDataObj->Str(), strFind) || !m_pScheduler->IsStarted() || m_pScheduler->IsAborted())
				SCHEDULER_CONDITIONED(FindNextLoadGrpTable())
		}
	// Stringa non trovata mi posizione in coda alla Tabella.
	GoEnd();
	return FALSE;
}

// Ricerca una STRINGA nelle direzione INDIETRO nei soli cmapi DataStr.
// Controlla se la direzione di ricerca coincide con quella di scansione,
// altrimenti la inverte, ricerca il DataObj all'interno della lista circolare
// e poi se non è stato trovato scandisce l'intera tabella.
//-----------------------------------------------------------------------------
BOOL CRadarDoc::FindStrPrev (CString& strFind)
{
	// Ricerca all'interno dell'array circolare
	for (int nRec = m_pRadarBody->m_nActiveGrpRow - 1 + m_nFirstIdxSqlRec; nRec >= 0; nRec--)
	{
		SqlRecord* pRec = TicGetSqlRecord (nRec);
		for (int nCol = 0; nCol < m_pFindCond->GetSizeSelCols(); nCol++)
		{
			DataObj* pDataObj = pRec->GetDataObjAt(GetColumnIdxFindCond(nCol));
			if (IsStringFind (pDataObj->Str(), strFind) || !m_pScheduler->IsStarted() || m_pScheduler->IsAborted())
				SCHEDULER_CONDITIONED(FindPrevRec (nRec));
		}
	}

	// Controllo della attuale direzione di scansione
	if (m_bNextDirection)
		InvertCursorDirection(FALSE);

	// Ricerca all'interno della tabella
	for (SqlRecord* pRec = TicInsertPrevSqlRec(); pRec != NULL; pRec = TicInsertPrevSqlRec())
		for (int nCol = 0; nCol < m_pFindCond->GetSizeSelCols(); nCol++)
		{
			DataObj* pDataObj = pRec->GetDataObjAt(GetColumnIdxFindCond(nCol));
			if (IsStringFind (pDataObj->Str(), strFind) || !m_pScheduler->IsStarted() ||  m_pScheduler->IsAborted())
				SCHEDULER_CONDITIONED(FindPrevLoadGrpTable ());
		}
	// Stringa non trovata mi posizione in testa alla Tabella.
	GoHome ();
	return FALSE;
}

// Uccide il timer attivato della find stupida
//-----------------------------------------------------------------------------
void CRadarDoc::KillFindTimer(BOOL bDeleteScheduler /*TRUE*/)
{
	if (!m_pScheduler)
		return;

	// Uccisione dello schedulatore e del timer
	m_pScheduler->Terminate();
	if (bDeleteScheduler)
	{
		delete m_pScheduler;
		m_pScheduler = NULL;
	}

	GetRadarFrame()->KillTimer(m_nTimerID);
	m_nTimerID = 0;
}

//-----------------------------------------------------------------------------
void CRadarDoc::UserBreakFind(BOOL bDeleteScheduler /*TRUE*/)
{
	KillFindTimer(bDeleteScheduler);
	AfxMessageBox(_TB("Search completed!\r\nInterrupted by the user."), MB_OK|MB_ICONSTOP);
}

// Controlla se la stringa strFind, indicata dall'utente, è contenuta e/o coincide
// con la string strFlg contenuta nel DataStr da esaminare.  La funnzione prende
// in considerazione i flag (Macth Case e Word Only) indicati dall'utente durante
// l'imputazione dei parametri della Find stupida.
//-----------------------------------------------------------------------------
BOOL CRadarDoc::IsStringFind (CString strFld, CString& strFind)
{
	// Controlla se è stata richiesta l'opzione di MacthCase. Se non attiva
	// si converte la stringa in caratteri minuscoli.
		strFld.MakeLower();

	// Ricerca della stringa
	int nIdxFirstChar = strFld.Find (strFind);

	if (
		m_pFindCond->m_nNumCond == ECompareType::CMP_NOT_CONTAINS ||
		(m_pFindCond->m_nNumCond == ECompareType::CMP_NOT_CONTAINS && nIdxFirstChar < 0)
	   )
	{
		return (nIdxFirstChar < 0);
	}
	
	if (nIdxFirstChar >= 0)
	{
		switch (m_pFindCond->m_nNumCond)
		{
		case ECompareType::CMP_CONTAINS : 
			return TRUE;
				break;

		case ECompareType::CMP_EQUAL :
		case ECompareType::CMP_NOT_EQUAL :
		// Controllo della corrispondenza esatta delle due stringhe
		if (nIdxFirstChar == 0 || !IsExtAlphaNumeric (strFld[nIdxFirstChar-1]))
		{
			int nIdxLastChar = nIdxFirstChar + strFind.GetLength();
			if (nIdxLastChar <= strFld.GetLength() && !IsExtAlphaNumeric (strFld[nIdxLastChar]))
					{
						if (m_pFindCond->m_nNumCond == ECompareType::CMP_NOT_EQUAL) return FALSE;
						return TRUE;
					}
				}
				if (m_pFindCond->m_nNumCond == ECompareType::CMP_NOT_EQUAL) return TRUE;
				break;

		case ECompareType::CMP_BEGIN_WITH:
				if (nIdxFirstChar == 0)
					return TRUE;
				break;

		case ECompareType::CMP_END_WITH:
				if (strFld.GetLength() ==  nIdxFirstChar + strFind.GetLength())
				return TRUE;
				break;

		}
	}
	
	return FALSE;
		}

//-----------------------------------------------------------------------------
BOOL CRadarDoc::RestartSearch()
{
	if (AfxMessageBox(_TB("The last row reached. Restart the search from the first ?"), MB_YESNO) == IDYES)
	{
		GoHome(FALSE);
		m_pRadarBody->m_nActiveGrpRow = m_pRadarBody->m_nActiveGrpRow - 1;
		StartFindStupid();
		return TRUE;
	}
	return FALSE;
}

// Predispone tutti i parametri e i flag per la ricerca stupida, seleziona la
// prodedura di test e di esecuzione della Find stupida.
//-----------------------------------------------------------------------------
void CRadarDoc::StartFindStupid()
{
	VERIFY(m_pTable->IsOpen());
	if ((m_pRadarBody->m_nActiveGrpRow + m_nFirstIdxSqlRec) == (GetSizeCircleList() - 1))
	{
		RestartSearch();
		return;
	}
	
	// Riempie l'array delle colonne selezionate
	ASSERT_VALID (m_pFindCond);
	m_pFindCond->RemoveAll ();
	for (int nIdx = 0; nIdx < m_pRadarBody->GetGrpTableNrCols(); nIdx++)
		if (m_pRadarBody->GetColumn(nIdx)->IsSelected())
			m_pFindCond->AddSelCols (nIdx);

	// Inizializzazione del timer
	m_nFindTic = 0;
	VERIFY ((m_nTimerID = GetRadarFrame()->SetTimer(IDT_RADAR, 400, NULL)));

 	// Esecuzione dello schedulatore
	m_pScheduler = new Scheduler;
	m_pScheduler->Start();

	// Inizializzazione dei messaggi nella status bar
	GetRadarFrame()->SetRunIndicator ();

	// Inizializzazione flag vari
	BOOL bIsFind = FALSE;

	// Selezione del tipo di ricerca in base alla colonna selezionata
	if (m_pFindCond->m_pDataObj->GetDataType() == DATA_STR_TYPE || m_pFindCond->m_pDataObj->GetDataType() == DATA_TXT_TYPE)
	{
		// Ricerca di un DataStr
		// Inizializzazione degli attributi di ricerca sulla stringa da ricercare
		CString strFind (m_pFindCond->m_pDataObj->Str());
			strFind = ((DataStr*)(m_pFindCond->m_pDataObj))->GetLowerCase();

		// Ricerca della stringa nella direzione indicata dall'utente
		bIsFind = m_pFindCond->m_bDown ? FindStrNext (strFind) : FindStrPrev (strFind);
	}
	else
	{
		// Inizializzazione delle procedure di test dei DatoObj in base alle scelte dell'utente
		// (ESCLUSA STRINGA - DATASTR)
		switch (m_pFindCond->m_nNumCond)
		{
			case ECompareType::CMP_EQUAL			: pfTestData = IsDataObjEqual;			break;
			case ECompareType::CMP_GREATER_THEN		: pfTestData = IsDataObjGreater;		break;
			case ECompareType::CMP_GREATER_OR_EQUAL	: pfTestData = IsDataObjGreaterEqual;	break;
			case ECompareType::CMP_LESSER_THEN		: pfTestData = IsDataObjLess;			break;
			case ECompareType::CMP_LESSER_OR_EQUAL	: pfTestData = IsDataObjLessEqual;		break;
			default									: pfTestData = IsDataObjNotEqual;
		}
		// Ricerca del DatoObj nella direzione indicata dall'utente
		bIsFind = m_pFindCond->m_bDown ? FindDataObjNext () : FindDataObjPrev ();
	}

	KillFindTimer();
	if (!bIsFind)
	{
		// Il DatoObj o la stringa imputata con i suoi attributi di ricerca non è stata
		// trovata all'interno della tabella.
		ASSERT_VALID (m_pFindCond);
		ASSERT_VALID (m_pFindCond->m_pDataObj);
		CString str;
		m_pRadarBody->GetColumn (m_pFindCond->GetSelCol(0))->FormatString (m_pFindCond->m_pDataObj, str);
		if (RestartSearch()) return;
	}
	// Se il DatoObj e la stringa sono stati trovati aggiorno eventualmente il
	// record nel documento collegato
	else 
	{
		// 22866 Radar ricerca con scrolling
		TopRow();
	}

	// La view deve essere aggiornata solo dopo aver aggiornato i vari indici
	UpdateRadarView	(UPDATE_VIEW);
}

// Aggiorna il contenuto grafico del radar.
//-----------------------------------------------------------------------------
void CRadarDoc::UpdateAllViewWithRec(int nNewActiveRec, WORD wCode)
{
	m_pRadarBody->m_nActiveGrpRow = nNewActiveRec;
	UpdateRadarView (wCode);
	// Aggiorno eventualmente il record nel documento collegato
	OnRadarRecordSelected();
}

//-----------------------------------------------------------------------------
void CRadarDoc::UpdateNrMultiLines (int nVisibleRows)
{
	ASSERT_VALID (m_pRadarBody);
	if (!m_pRadarBody->IsInsideVisibleRowsRange (m_nFirstIdxSqlRec, GetSizeCircleList() - 1, nVisibleRows, m_bNextDirection))
		m_pRadarBody->SetActiveAndFirstIdxGrpRow (MULTI_LINES, nVisibleRows);
	m_pRadarBody->UpdateMultiLinesFlag();
	UpdateAllViews (NULL, MAKELPARAM(0, UPDATE_VIEW));
}

//-----------------------------------------------------------------------------
void CRadarDoc::CloseRadar() 
{ 
	CRadarFrame* pRFrame = GetRadarFrame();
	//ASSERT(pRFrame);
	if (pRFrame)
		pRFrame->CloseRadar(); 
}

//-----------------------------------------------------------------------------
BOOL CRadarDoc::CanDoExit () 
{ 
	return IsFindNotActive	() && !IsStayAlive() && !m_bEditingQuery && !AfxIsThreadInModalState() && !IsWaiting();
}

// Visualizza e attiva il radar
//-----------------------------------------------------------------------------
void CRadarDoc::ActivateAndShowRadar(BOOL bShow)
{
	// Visualizza il radar.
	CRadarFrame* pFrame = GetRadarFrame();
	
	// Attivazione
	pFrame->CBCGPFrameWnd::ActivateFrame();

	ReadWindowPlacement(m_pCallerSqlRecord->GetTableName());

	UpdateRadarView (UPDATE_SCROLL_SIZE);

	if (bShow)
		pFrame->ShowWindow(SW_SHOW);

	// Visualizza gli eventuali errori durante l'inizializzazione del radar
	// La visualizzazione della finestra con gli errori DEVE AVVENIRE dopo
	// aver completatato il meccanismo standard di attivazione del radar.
	// NON SPOSTARE QUESTA RIGA
	AfxGetDiagnostic()->Show (TRUE);
}

// Crea il template del documento che è stato passato durante la fase di Attach
//-----------------------------------------------------------------------------
BOOL CRadarDoc::CreateCallerDoc()
{
	// Se il documento è già aperto non devo crearlo (DBT).
	if (m_pCallerDoc)
		return TRUE;

	// serve per inibire che il radar esegua piu` volte l'apertura del documento
	// di inserimento (esempio doppio click sul bottone della toolbar)
	m_bDocumentCalled = TRUE;
	if (m_CallerDocNamespace.IsValid())
		AfxGetTbCmdManager()->RunDocument(m_CallerDocNamespace.ToString(), szDefaultViewMode, TRUE, NULL, NULL, (CBaseDocument**) &m_pCallerDoc);

	if (m_pCallerDoc == NULL)
	{
		m_bDocumentCalled = FALSE;
		return FALSE;
	}

	m_pCallerSqlRecord = m_pCallerDoc->m_pDBTMaster->GetRecord();
	ASSERT(m_pCallerSqlRecord);

	// Collega il documento al radar
	m_pCallerDoc->m_pRadarDoc = this; 

	return TRUE;
}

//-----------------------------------------------------------------------------
int CRadarDoc::GetRadarColumnIndex (const CString& sColumnName)
{ 
	ASSERT_VALID(m_pRadarBody);
	return m_pRadarBody->FindColumn(sColumnName);; 
}

BOOL CRadarDoc::MoveRadarColumn (int nFromIndex, int nToIndex)
{ 
	ASSERT_VALID(m_pRadarBody);
	return m_pRadarBody->MoveColumn(nFromIndex, nToIndex);
}

BOOL CRadarDoc::SetColumnTitle (int nIdxCol, LPCTSTR szTitle)
{ 
	ASSERT_VALID(m_pRadarBody);
	return m_pRadarBody->SetColumnTitle(nIdxCol, szTitle);
}

BOOL CRadarDoc::SetColumnAlign (int nIdxCol, UINT align)
{ 
	ASSERT_VALID(m_pRadarBody);
	return m_pRadarBody->SetColumnAlign(nIdxCol, align);
}

BOOL CRadarDoc::RemoveRadarColumn (int nIndex)
{ 
	ASSERT_VALID(m_pRadarBody);
	return m_pRadarBody->RemoveColumn(nIndex);
}

void CRadarDoc::SetColumnScreenSize (int nIdxCol, int width)
{ 
	ASSERT_VALID(m_pRadarBody);
	m_pRadarBody->SetColumnToWidthChar(nIdxCol, width);
}

//-----------------------------------------------------------------------------
// Collega la view alla struttura contenente tutte le informaziomi per la
// rappresentazione grafica. Carica gli eventuali salvataggi delle precedenti
// configurazioni dal file .INI e i record dal data base
void CRadarDoc::StartRadar(SqlTable* pTable, BOOL bUsePredefined /*= FALSE */)
{
	// Disabilitazione dell'applicazione e visualizzazione del cursore a clessidra
	BeginWaitCursor();

	ASSERT_VALID(pTable);
	m_pTable = pTable;

	m_pRadarBody		= new RadarBody	(this);
	m_pSqlRecCircleList	= new RadarSqlRecCircleList (this);
	m_pDeletedColumns	= new RadarColumnsIdxArray;

	// Collega il radarbody alla view
	ASSERT_VALID(GetRadarView());
	GetRadarView ()->AttachBodyToView (m_pRadarBody);

	BOOL bLoadAttributesFromFile = FALSE;
	//carica il .RAD se esiste
	if (!LoadGrpAttribFromFile())
	{
		// Non esisteva un predecente salvataggio (.RAD) della configurazione grafica o
		// si è verificato un errore durante la fase di caricamento. In tal caso
		// tutte le informazioni lette sino a quel momento vengono azzerate
		m_pDeletedColumns->RemoveAll();

		// Caricamento della configurazione di default per ogni singola colonna
		// prelevandola direttamente dal data base
		AddColumnGrpTable ();
    }
	else
	{
		bLoadAttributesFromFile = TRUE;
	}

	//possibilità di personalizzazione programmativa
	if (m_pCallerHotLink)
	{
		m_pCallerHotLink->OnAfterLoadRadarColumns(this);
	}
	else if (m_pCallerDoc)
	{
		m_pCallerDoc->OnAfterLoadRadarColumns(this, m_pTable);
	}

	//Personalize the ToolBar
	if (GetRadarFrame())
	{
		if (m_pCallerHotLink)
		{
			m_pCallerHotLink->OnCustomizeRadarToolbar(GetRadarFrame()->GetTabbedToolBar());
		}
		else if (m_pCallerDoc)
		{
			m_pCallerDoc->OnCustomizeRadarToolbar(GetRadarFrame()->GetTabbedToolBar());
		}
    }
	
	// Aggiorna le coordinate di partenza e di arrivo di una singola colonna per
	// l'ottimizzazione della fase di paint.
	// NON SPOSTARE QUESTA RIGA (*)
	if (!bLoadAttributesFromFile)
	{
		m_pRadarBody->UpdateColumnsWidthOnFonts (this);
	}

	m_pRadarBody->UpdateColumnsStartEnd (0, TRUE);

	// Abilitazione dell'applicazione e visualizzazione del cursore precedente
	EndWaitCursor();

	// Collego la tool bar contenete la combo delle query alla tabella in uso
	bUsePredefined = bUsePredefined || (m_pCallerDoc && m_pCallerDoc->GetFormMode() == CBaseDocument::FIND);

	if (!(m_pCallerHotLink && m_pCallerHotLink->IsKindOf(RUNTIME_CLASS(DynamicHotKeyLink))))	//TODO DynamicHotKeyLink
		m_pQueryManager->Attach(m_pTable, bUsePredefined, FALSE, FALSE);	
                                    
	// Caricamento nel buffer circolare dei record
	if (!OpenTableAndCheckIsEmpty())
		GoHome (FALSE);

	ActivateAndShowRadar();
}

//-----------------------------------------------------------------------------
void CRadarDoc::StartReady()
{
	if (!AfxGetThemeManager()->ShowRadarEdit())
	{
		UpdateAllViewWithRec(m_pRadarBody->m_nFirstIdxGrpRow, UPDATE_OPTIMIZATION);
	}
}

// Ritorna la frame del radar
//-----------------------------------------------------------------------------
CRadarFrame* CRadarDoc::GetRadarFrame()
{
	CRadarView* pRView = GetRadarView();
	return pRView ? pRView->GetRadarFrame() : NULL;
}

// Funzione richiamata dal meccanismo di MFC per la gestione del salvataggio
// dei dati. La funzione verifica se il documento può essere chiuso o meno. Nel
// caso sia attiva la find stupida chiedo conferma all'utente se vuole
// abbandonare la ricerca attualmente in corso. Se viene confermata questa
// richiesta il documento viene chiuso altrimenti rimane aperto
//-----------------------------------------------------------------------------
BOOL CRadarDoc::SaveModified()
{
	ASSERT_VALID(this);

	if (IsActiveFind())
	{
		// La Find stupida è attiva, prima di uscire devo ucciderla
		if (AfxMessageBox(_TB("The Search is still running.\r\nDo you want to abort it?"), MB_YESNO|MB_ICONEXCLAMATION) != IDYES)
			return FALSE;
		UserBreakFind ();
	}

	// Eventuale salvataggio della configurazione grafica
	SaveGraphicAttrib(TRUE);

	return TRUE;
}

// Creazione di un nuovo radar
//-----------------------------------------------------------------------------
BOOL CRadarDoc::OnNewDocument()
{
	// Disabilitazione dell'applicazione e visualizzazione del cursore a clessidra
	BeginWaitCursor ();

	BOOL bOk = CAbstractDoc::OnNewDocument();
	SetTitle (_TB("Radar"));

    // La partenza del documento avviene con il flag 'non modificato'
	if (bOk)
		SetModifiedFlag (FALSE);
	
	// Abilitazione dell'applicazione e visualizzazione del cursore precedente
	EndWaitCursor ();
	return bOk;
}

// Carica dalla tabella gli SqlRecord inserendoli nell'array circolare. La
// tabella viene letta nella direzione AVANTI e i record aggiunti in coda. Il
// numero massimo di SqlRecord letti è indicato nella variabile nNrRec.
//-----------------------------------------------------------------------------
BOOL CRadarDoc::LoadCircleListNext (int nNrRec, int& nRecEnd, BOOL bWaitCursor /*TRUE*/)
{
	// Disabilitazione dell'applicazione e visualizzazione del cursore a clessidra
	if (bWaitCursor)
	{
		BeginWaitCursor();
	}

	// Carica i dati della tabella nell'array circolare nella direzione AVANTI
	// (successivo)
	int nIdx = 0; 
	for (nIdx = 0; nIdx < nNrRec; nIdx++)
		if (!m_pSqlRecCircleList->InsertNext())
			break;

	if (nIdx)
	{
		// Ricalcola gli indice
		m_nFirstIdxSqlRec	-= nIdx;
		nRecEnd				-= nIdx;
	}
	// Abilitazione dell'applicazione e visualizzazione del cursore precedente
	if (bWaitCursor)
	{
		EndWaitCursor();
	}
	return nIdx;
}

// Carica dalla tabella gli SqlRecord inserendoli nell'array circolare. La
// tabella viene letta nella direzione INDIETRO e i record aggiunti in testa.
// Il numero massimo di SqlRecord letti è indicato nella variabile nNrRec.
//-----------------------------------------------------------------------------
BOOL CRadarDoc::LoadCircleListPrev (int nNrRec, int& nRecEnd)
{
	// Disabilitazione dell'applicazione e visualizzazione del cursore a clessidra
	BeginWaitCursor();

	// Carica i dati della tabella nell'array circolare nella direzione INDIETRO
	// (precedente)
	int nIdx = 0;
	for (nIdx = 0; nIdx < nNrRec; nIdx++)
		if (!m_pSqlRecCircleList->InsertPrev())
			break;

	if (nIdx)
	{
		// Ricalcola gli indice
		m_nFirstIdxSqlRec	+= nIdx;
		nRecEnd				+= nIdx;
		if (nRecEnd < 0)
			nRecEnd = 0;
	}
	// Abilitazione dell'applicazione e visualizzazione del cursore precedente
	EndWaitCursor();
	return nIdx;
}

// Carica dal file .INI gli attributi grafici precedentemente salvati.
//-----------------------------------------------------------------------------
BOOL CRadarDoc::LoadGrpAttribFromFile()
{
	// Costruisce la stringa di accesso alla sezione
	CString	strSection (m_pCallerSqlRecord->GetTableName());
	SetTitle (GetTableTitle());

	// Il salvataggio del radar fa riferimento ad una versione diversa per cui non lo carico
	CString strBuffer;
	if (!GetRadarProfileString (strSection, szTagRelease, _T(""), strBuffer) || strBuffer != szCurrentRelease)
	{
		// Creo il font di default
		GetRadarView()->CreateFontIndirect (LoadDefaultFontStyle ());
		// Aggiorno la combo box degli stili del carattere
		GetRadarFrame()->FillFontStyleComboBox (m_nIdxFontStyle);
		return (m_bEnableSave = FALSE);
	}

	// Deve essere eseguita prima del caricamento del font, in quanto la procedura che
	// crea il nuovo font imposta anche l'altezza della riga.
	ASSERT_VALID (m_pRadarBody);
	GetRadarView ()->ReadNoAskConfirm (strSection, m_strTempRadarFile);

	// Carica lo stile del font
	LoadFontStyle (strSection);

	// Carica la dimensione dell'array circolare per la memorizzazione dei SqlRecord
	SqlRecord* pRec = m_pTable ? m_pTable->GetRecord() : m_pCallerSqlRecord;
	// Caricamento degli attributi grafici di ogni singola colonna
	return m_pRadarBody->ReadColumnsConfig(strSection, m_strTempRadarFile, pRec, m_pDeletedColumns);
}

// Se il font è stato salvato su file prova a caricarlo, sde non esiste carica
// lo stile di default.
//-----------------------------------------------------------------------------
void CRadarDoc::LoadFontStyle (CString& szSection)
{
	CString strFontStyle, strFontStyle1;
	strFontStyle1 =szSTYLE_RADAR;

	// Controllo l'eventuale presenta di un precedente salvattagio dello stile
	if	(
			GetRadarProfileString	(szSection, szTagFontStyle, _T(""), strFontStyle) &&
			CheckExistFontStyle		(strFontStyle, strFontStyle1, _TB("The font style '{0-%s}' is wrong or has been deleted.\r\nThe font style '{1-%s}' will be used instead."))
		)
	{
		const FontStyle* pFont = AfxGetFontStyleTable()->GetFontStyle(m_nIdxFontStyle, &GetNamespace());
		GetRadarView ()->CreateFontIndirect (pFont->GetLogFont());
	}
	else
		GetRadarView ()->CreateFontIndirect (LoadDefaultFontStyle());

	// Aggiorno la combo box degli stili del carattere
	GetRadarFrame()->FillFontStyleComboBox (m_nIdxFontStyle);
}

// Carica il salvataggio della precedente posizione e dimensione della finestra
// del radar.
//-----------------------------------------------------------------------------
void CRadarDoc::ReadWindowPlacement(LPCTSTR lpszSection)
{
	WINDOWPLACEMENT wp;
	CheckRadarFullFileName();
	if (!m_strRadarFile.IsEmpty() && LoadWindowPlacement(lpszSection, wp, m_strTempRadarFile))
	{
		//wp.showCmd = SW_HIDE;
		GetRadarFrame()->SetWindowPlacement(&wp);
	}
}

// Scrive sul file RADAR_FILE_INI la corrente posizione e dimensione del radar.
//-----------------------------------------------------------------------------
void CRadarDoc::SaveWindowPlacement(CString& strSection)
{
	WINDOWPLACEMENT wp;
	wp.length = sizeof wp;

	CheckRadarFullFileName(TRUE);
	if (!m_strRadarFile.IsEmpty() && GetRadarFrame() && GetRadarFrame()->GetWindowPlacement(&wp))
		WriteWindowPlacement(strSection, wp, m_strTempRadarFile);
}

//-----------------------------------------------------------------------------
void CRadarDoc::AddDelColumnGrpTableFromMouse (const CPoint& LPMousePos)
{
	int nCol = m_pRadarBody->GetColumnIdxFromPoint (LPMousePos);
	if (nCol >= 0)
		AddDelColumnGrpTable (nCol);
	else
		AddDelColumnTailGrpTable ();
	delete m_pColsToInsert;
	m_pColsToInsert = NULL;
}

//-----------------------------------------------------------------------------
void CRadarDoc::AddDelColumnGrpTable (int nCol)
{
	ASSERT_VALID (m_pCallerSqlRecord);
	ASSERT_VALID (m_pColsToInsert);
	ASSERT_VALID (GetRadarView());
	ASSERT(::IsWindow(GetRadarView()->m_hWnd));

	CDC* pDC = GetRadarView()->GetDC();

	for (int nIdx = 0; nIdx < m_pColsToInsert->GetSize (); nIdx++)
	{
		int nIdxCol = m_pColsToInsert->GetAt (nIdx)->GetIdx();
		// Aggiunge o inserisce le carrateristiche grafiche e di visualizzazione della colonna
		// a secondo della scelte imposte dall'utente
		CString szQualifiedName = m_pCallerSqlRecord->GetQualifiedColumnName(nIdxCol);

		m_pRadarBody->InsertColumn(nCol, nIdxCol, m_pCallerSqlRecord->GetColumnInfo(nIdxCol), szQualifiedName);

		RadarColumn* pCol = m_pRadarBody->GetColumn(nCol);

		int nWidth = pCol->CalcScreenWidth(this, pDC);
		m_pRadarBody->SetColumnToWidthChar(nCol, nWidth, FALSE);

		// Aggiunge i dati da visualizzare
		for (
				int nIdxRow = m_pRadarBody->m_nFirstIdxGrpRow;
				nIdxRow < m_pRadarBody->m_nNrMaxFormatRows && nIdxRow < GetSizeCircleList();
				nIdxRow++
			)
			m_pRadarBody->AddDataToColumn (pCol, m_pSqlRecCircleList->GetDataObj(nIdxRow, nIdxCol));
	}

	GetRadarView()->ReleaseDC(pDC);
	
	m_bEnableSave = TRUE;
	
	// Ricalcola dalla colonna indicata le nuove dimensione delle colonne e aggiorna il contenuto della view
	m_pRadarBody->UpdateColumnsStartEnd (nCol ? nCol - 1 : nCol, TRUE);

	UpdateRadarView (UPDATE_VIEW_SCROLL_TITLEBAR);
}

// Aggiunge in coda alla tabella grafica le colonne che l'utente desidera
// nuovamente visualizzare
//-----------------------------------------------------------------------------
void CRadarDoc::AddDelColumnTailGrpTable ()
{
	ASSERT_VALID (m_pCallerSqlRecord);
	ASSERT_VALID (m_pColsToInsert);
	ASSERT(::IsWindow(GetRadarView()->m_hWnd));

	CDC* pDC = GetRadarView()->GetDC();

	SqlRecord* pRec = m_pTable ? m_pTable->GetRecord() : m_pCallerSqlRecord;

	int nStartCol = -1;
	for (int nIdx = 0; nIdx < m_pColsToInsert->GetSize (); nIdx++)
	{
		int nIdxCol	= m_pColsToInsert->GetAt (nIdx)->GetIdx();
		int nCol	= AddColumnGrpTable (nIdxCol, pRec);
		ASSERT (nCol >= 0);

		if (nStartCol < 0)
			nStartCol = nCol ? nCol - 1 : nCol;

		// Aggiunge i dati da visualizzare
		RadarColumn* pCol = m_pRadarBody->GetColumn (nCol);

		int nWidth = pCol->CalcScreenWidth(this, pDC);
		m_pRadarBody->SetColumnToWidthChar(nCol, nWidth, FALSE);

		for (
				int nIdxRow = m_pRadarBody->m_nFirstIdxGrpRow;
				nIdxRow < m_pRadarBody->m_nNrMaxFormatRows && nIdxRow < GetSizeCircleList();
				nIdxRow++
			)
			m_pRadarBody->AddDataToColumn (pCol, m_pSqlRecCircleList->GetDataObj(nIdxRow, nIdxCol));
	}

	GetRadarView()->ReleaseDC(pDC);

	// Ricalcola dalla colonna indicata le nuove dimensione delle colonne e aggiorna il contenuto della view
	m_bEnableSave = TRUE;

	m_pRadarBody->UpdateColumnsStartEnd (nStartCol, TRUE);

	UpdateRadarView (UPDATE_VIEW_SCROLL_TITLEBAR);
}

// Si posiziona sull'ultimo record della query corrente
//-----------------------------------------------------------------------------
void CRadarDoc::GoEnd ()
{
	// Disabilitazione dell'applicazione e visualizzazione del cursore a clessidra
	BeginWaitCursor();

	m_bNextDirection = FALSE;
	if (m_pSqlRecCircleList->LoadFromBottom ())
	{
		// Si posizione sul prima record libero
		m_nFirstIdxSqlRec = GetSizeCircleList() > m_pRadarBody->m_nNrMaxFormatRows
					? GetSizeCircleList() - m_pRadarBody->m_nNrMaxFormatRows
					: 0;

		// Rimuove tutti i le righe formattate
		m_pRadarBody->RemoveAllRows();

		// Aggiunge le nuove righe da visualizzare
		for (int nIdx = m_nFirstIdxSqlRec; nIdx < GetSizeCircleList(); nIdx++)
			AddRowGrpTable (nIdx);

		m_pRadarBody->SetFirstIdxGrpRowFromEnd(GetRadarView ()->GetVisibleRows());
		// Invio del messsaggio alla stautus bar
		UpdateAllViewWithRec (m_pRadarBody->GetGrpTableNrRows() - 1, UPDATE_VIEW);

		// Aggiorno eventualmente il record nel documento collegato
		OnRadarRecordSelected();
	}
	else
		// Rimuove tutti i le righe formattate
		m_pRadarBody->RemoveAllRows();

	// Abilitazione dell'applicazione e visualizzazione del cursore precedente
	EndWaitCursor();
}

// Si posiziona sul primo record della query corrente
//-----------------------------------------------------------------------------
void CRadarDoc::GoHome (BOOL bUpdate /* TRUE */)
{   
	// Disabilitazione dell'applicazione e visualizzazione del cursore a clessidra
	BeginWaitCursor();

	m_bNextDirection = TRUE;
	// Carica ui record partendo dalla cima della tabella in direzione giù
	if (m_pSqlRecCircleList->LoadFromTop())
	{
		// numero di righe da formattare
		int nNrFormatRec = min (GetSizeCircleList(), m_pRadarBody->m_nNrMaxFormatRows);

		m_nFirstIdxSqlRec = 0;
		// Rimuovo gli eventuali record presenti nella tabella grafica e carico i nuovi
		m_pRadarBody->RemoveAllRows();

		for (int nIdx = m_nFirstIdxSqlRec; nIdx < nNrFormatRec; nIdx++)
			AddRowGrpTable (nIdx);

		// Aggiornamento degli indici all'interno della tabella dei dati grafici
		m_pRadarBody->SetActiveAndFirstIdxGrpRow (START_FROM_TOP, GetRadarView ()->GetVisibleRows ());
		if (bUpdate)
			UpdateAllViewWithRec(0, UPDATE_VIEW);

		// Aggiorno eventualmente il record nel documento collegato
		OnRadarRecordSelected();
	}
	else
		// Rimuovo gli eventuali record presenti nella tabella grafica e carico i nuovi
		m_pRadarBody->RemoveAllRows();

	// Abilitazione dell'applicazione e visualizzazione del cursore precedente
	EndWaitCursor();
}

// Muove la corrente riga attiva, nella direzione di scansione dal basso verso 
// l'alto.
//-----------------------------------------------------------------------------
void CRadarDoc::PrevRow (int nNewActiveRec, WORD wCode)
{
	ASSERT (GetSizeCircleList());

	// Verifico se la nuova riga attiva è contenuta tra quelle attaulmente visibili
	if (nNewActiveRec >= m_pRadarBody->m_nFirstIdxGrpRow)
	{
		UpdateAllViewWithRec(nNewActiveRec, UPDATE_OPTIMIZATION);
		return;
	}

	// Se tale riga non è contenuta nella tabella della righe formattate
	// provare a caricare in questa tabella nuove righe.
	if (nNewActiveRec < 0 || (m_pRadarBody->m_nFirstIdxGrpRow - nNewActiveRec) < 0)
	{
		// Calcolo il nuovo indice dell'ipotetica riga richiesta. Se questo
		// valore supera la dimensione dell'array circolare provo a caricarne
		// di nuovi.
		int	nRecEnd = m_nFirstIdxSqlRec + nNewActiveRec;
		if (nRecEnd < 0)
		{
			// Devo caricare i nuovi record nell'array circolare
			ASSERT (m_pTable->IsOpen());
			if (m_pTable->IsBOF())
			{
				// Se sono posizionato sul primo verifico che la la riga attiva
				// coincida con il primo record dell'array circolare, in tal
				// caso sono proprio sulla prima riga della tabella. Non devo
				// più paintare e aggiornare niente
				if (!m_nFirstIdxSqlRec && !m_pRadarBody->m_nActiveGrpRow)
					return;
			}
			else
			{
				// Se non sono nella direzione corretta devo portare il cursore
				// della tabella nella posizione corretta per leggere i record
				// da caricare.
				if (m_bNextDirection)
					InvertCursorDirection(FALSE);

				// Prova a caricare nuovi record, se fallisco e sono posizionato
				// sul primo verifico che la la riga attiva coincida con il
				// primo record dell'array circolare, in tal caso sono proprio
				// sull'ultima riga della tabella. Non devo più paintare e
				// aggiornare niente
				if	(
						!LoadCircleListPrev(m_pRadarBody->m_nNrMaxFormatRows, nRecEnd)	&&
						!m_nFirstIdxSqlRec												&&
						!m_pRadarBody->m_nActiveGrpRow
					)
					return;
			}
			// Calcolo il numero di righe da caricare dall'array circolare
			// alla tabella grafica in modo da far coincidere l'attuale riga
			// attiva con la prima riga visualizzata
			nRecEnd = max (m_nFirstIdxSqlRec + nNewActiveRec, 0);
		}
		// Aggiunge i nuovi record nella tabelle delle righe formattate,
		// aggiungendo in testa
		for (int nIdx = m_nFirstIdxSqlRec - 1; nIdx >= nRecEnd; nIdx--)
		{
			AddRowHeadGrpTable (nIdx);
			m_nFirstIdxSqlRec--;
		}
		nNewActiveRec = 0;
		if (wCode == UPDATE_FROM_ROW_UP)
			GetRadarView ()->InvertEasyReadingCr ();
	}
	m_pRadarBody->m_nFirstIdxGrpRow = nNewActiveRec;
	UpdateAllViewWithRec(nNewActiveRec, wCode);
}

//-----------------------------------------------------------------------------
void CRadarDoc::GoToEndRow ()
{
	BeginWaitCursor ();

	int nNewActiveRec = 0;
	do
	{
		nNewActiveRec = m_pRadarBody->GetIdxGrpRowNextPage(GetRadarView()->GetVisibleRows());
		ASSERT (GetSizeCircleList());

		// Verifico se la nuova riga attiva è contenuta tra quelle attaulmente visibili
		if (nNewActiveRec < GetLastIdxGrpRow ())
		{
			UpdateAllViewWithRec(nNewActiveRec, UPDATE_OPTIMIZATION);
			continue;
		}

		// Se tale riga non è contenuta nella tabella della righe formattate
		// provare a caricare in questa tabella nuove righe.
		if (nNewActiveRec >= m_pRadarBody->GetGrpTableNrRows())
		{
			// Calcolo il nuovo indice dell'ipotetica riga richiesta. Se questo
			// valore supera la dimensione dell'array circolare provo a caricarne
			// di nuovi.
			int	nRecEnd = m_nFirstIdxSqlRec + nNewActiveRec;
			int nSizeCircle = GetSizeCircleList();
			if (nRecEnd >= (nSizeCircle - 1))
			{
				// Devo caricare i nuovi record nell'array circolare
				ASSERT (m_pTable->IsOpen());
				if (m_pTable->IsEOF())
				{
					// Se sono posizionato sull'ultimo verifico che la la riga attiva
					// coincida con l'ultimo record dell'array circolare, in tal caso
					// sono proprio sull'ultima riga della tabella. Non devo più
					// paintare e aggiornare niente
					if ((m_pRadarBody->m_nActiveGrpRow + m_nFirstIdxSqlRec) == (GetSizeCircleList() - 1))
						continue;
				}
				else
				{
					// Se non sono nella direzione corretta devo portare il cursore
					// della tabella nella posizione corretta per leggere i record
					// da caricare.
					if (!m_bNextDirection)
						InvertCursorDirection();	

					// Prova a caricare nuovi record, se fallisco e sono posizionato
					// sull'ultimo verifico che la la riga attiva coincida con
					// l'ultimo record dell'array circolare, in tal caso sono proprio
					// sull'ultima riga della tabella.
					// Non devo più paintare e aggiornare niente
					if	(
							!LoadCircleListNext(m_pRadarBody->m_nNrMaxFormatRows, nRecEnd, FALSE) &&
							(m_pRadarBody->m_nActiveGrpRow + m_nFirstIdxSqlRec) == (GetSizeCircleList() - 1)
						)
						continue;
				}
				// Calcolo il numero di righe da caricare dall'array circolare alla
				// tabella grafica in modo da far coincidere l'attuale riga attiva
				// con l'ultima riga visualizzata
				int nCfr = GetLastIdxSqlRec();
				nCfr = m_pRadarBody->GetGrpTableNrRows(); 
				nCfr = nNewActiveRec + m_nFirstIdxSqlRec;
				nCfr = nNewActiveRec -m_pRadarBody->GetGrpTableNrRows() + GetLastIdxSqlRec();
				nRecEnd = min (GetSizeCircleList() - 1,  nCfr);
			}
			// Aggiunge i nuovi record nella tabelle delle righe formattate, aggiungendo in coda
			for (int nIdx = GetLastIdxSqlRec(); nIdx <= nRecEnd; nIdx++)
			{
				AddRowTailGrpTable (nIdx);
				m_nFirstIdxSqlRec++;
			}
			nNewActiveRec = m_pRadarBody->GetGrpTableNrRows() - 1;
		}
		ASSERT (nNewActiveRec >= 0);
		m_pRadarBody->m_nFirstIdxGrpRow = nNewActiveRec - GetRadarView ()->GetAvailableVisibleRows() + 1;
	}
	while (!m_pTable->IsEOF());

	EndWaitCursor ();

	UpdateAllViewWithRec(nNewActiveRec, UPDATE_FROM_PAGE_DOWN);
	OnPageDown ();
}

// Muove la corrente riga attiva, nella direzione di scansione dall'alto verso
// il basso
//-----------------------------------------------------------------------------
void CRadarDoc::NextRow (int nNewActiveRec, WORD wCode)
{
	ASSERT (GetSizeCircleList());

	// Verifico se la nuova riga attiva è contenuta tra quelle attaulmente visibili
	if (nNewActiveRec < GetLastIdxGrpRow ())
	{
		UpdateAllViewWithRec(nNewActiveRec, UPDATE_OPTIMIZATION);
		return;
	}

	// Se tale riga non è contenuta nella tabella della righe formattate
	// provare a caricare in questa tabella nuove righe.
	if (nNewActiveRec >= m_pRadarBody->GetGrpTableNrRows())
	{
		// Calcolo il nuovo indice dell'ipotetica riga richiesta. Se questo
		// valore supera la dimensione dell'array circolare provo a caricarne
		// di nuovi.
		int	nRecEnd = m_nFirstIdxSqlRec + nNewActiveRec;
		int nSizeCircle = GetSizeCircleList();
		if (nRecEnd >= (nSizeCircle - 1))
		{
			// Devo caricare i nuovi record nell'array circolare
			ASSERT (m_pTable->IsOpen());
			if (m_pTable->IsEOF())
			{
				// Se sono posizionato sull'ultimo verifico che la la riga attiva
				// coincida con l'ultimo record dell'array circolare, in tal caso
				// sono proprio sull'ultima riga della tabella. Non devo più
				// paintare e aggiornare niente
				if ((m_pRadarBody->m_nActiveGrpRow + m_nFirstIdxSqlRec) == (GetSizeCircleList() - 1))
					return;
			}
			else
			{
				// Se non sono nella direzione corretta devo portare il cursore
				// della tabella nella posizione corretta per leggere i record
				// da caricare.
				if (!m_bNextDirection)
					InvertCursorDirection();	

				// Prova a caricare nuovi record, se fallisco e sono posizionato
				// sull'ultimo verifico che la la riga attiva coincida con
				// l'ultimo record dell'array circolare, in tal caso sono proprio
				// sull'ultima riga della tabella.
				// Non devo più paintare e aggiornare niente
				if	(
						!LoadCircleListNext(m_pRadarBody->m_nNrMaxFormatRows, nRecEnd) &&
						(m_pRadarBody->m_nActiveGrpRow + m_nFirstIdxSqlRec) == (GetSizeCircleList() - 1)
					)
					return;
			}
			// Calcolo il numero di righe da caricare dall'array circolare alla
			// tabella grafica in modo da far coincidere l'attuale riga attiva
			// con l'ultima riga visualizzata
			int nCfr = GetLastIdxSqlRec();
			nCfr = m_pRadarBody->GetGrpTableNrRows(); 
			nCfr = nNewActiveRec + m_nFirstIdxSqlRec;
			nCfr = nNewActiveRec -m_pRadarBody->GetGrpTableNrRows() + GetLastIdxSqlRec();
			nRecEnd = min (GetSizeCircleList() - 1,  nCfr);
		}
		// Aggiunge i nuovi record nella tabelle delle righe formattate, aggiungendo in coda
		for (int nIdx = GetLastIdxSqlRec(); nIdx <= nRecEnd; nIdx++)
		{
			AddRowTailGrpTable (nIdx);
			m_nFirstIdxSqlRec++;
		}
		nNewActiveRec = m_pRadarBody->GetGrpTableNrRows() - 1;
		if (wCode == UPDATE_FROM_ROW_DOWN)
			GetRadarView ()->InvertEasyReadingCr ();
	}
	ASSERT (nNewActiveRec >= 0);
	m_pRadarBody->m_nFirstIdxGrpRow = nNewActiveRec - GetRadarView ()->GetAvailableVisibleRows() + 1;

	UpdateAllViewWithRec(nNewActiveRec, wCode);
}

// seleziona il record e lo evidenzia come prima line in lista se è possibile
//-----------------------------------------------------------------------------
void CRadarDoc::TopRow()
{
	INT nFirstIdxGrpRow = m_pRadarBody->GetFirstIdxGrpRow();
	INT nActiveGrpRow = m_pRadarBody->GetActiveGrpRow();
	INT nLastIdxGrpRow = GetLastIdxGrpRow();

	if (nActiveGrpRow <= nFirstIdxGrpRow)
	{
		OnRadarRecordSelected();
		return;
	}

	// Nuva riga dove spostarsi
	INT nNewActiveRec = nLastIdxGrpRow + (nActiveGrpRow - nFirstIdxGrpRow - 1);

	// Se tale riga non è contenuta nella tabella della righe formattate
	// provare a caricare in questa tabella nuove righe.
	if (nNewActiveRec >= m_pRadarBody->GetGrpTableNrRows())
	{
		// Calcolo il nuovo indice dell'ipotetica riga richiesta. Se questo
		// valore supera la dimensione dell'array circolare provo a caricarne
		// di nuovi.
		int	nRecEnd = m_nFirstIdxSqlRec + nNewActiveRec;
		int nSizeCircle = GetSizeCircleList();
		if (nRecEnd >= (nSizeCircle - 1))
		{
			// Devo caricare i nuovi record nell'array circolare
			ASSERT(m_pTable->IsOpen());
			if (m_pTable->IsEOF())
			{
				// Se sono posizionato sull'ultimo verifico che la la riga attiva
				// coincida con l'ultimo record dell'array circolare, in tal caso
				// sono proprio sull'ultima riga della tabella. Non devo più
				// paintare e aggiornare niente
				if ((m_pRadarBody->m_nActiveGrpRow + m_nFirstIdxSqlRec) == (GetSizeCircleList() - 1))
					return;
			}
			else
			{
				// Se non sono nella direzione corretta devo portare il cursore
				// della tabella nella posizione corretta per leggere i record
				// da caricare.
				if (!m_bNextDirection)
					InvertCursorDirection();

				// Prova a caricare nuovi record, se fallisco e sono posizionato
				// sull'ultimo verifico che la la riga attiva coincida con
				// l'ultimo record dell'array circolare, in tal caso sono proprio
				// sull'ultima riga della tabella.
				// Non devo più paintare e aggiornare niente
				if (
					!LoadCircleListNext(m_pRadarBody->m_nNrMaxFormatRows, nRecEnd) &&
					(m_pRadarBody->m_nActiveGrpRow + m_nFirstIdxSqlRec) == (GetSizeCircleList() - 1)
					)
					return;
			}
			// Calcolo il numero di righe da caricare dall'array circolare alla
			// tabella grafica in modo da far coincidere l'attuale riga attiva
			// con l'ultima riga visualizzata
			int nCfr = GetLastIdxSqlRec();
			nCfr = m_pRadarBody->GetGrpTableNrRows();
			nCfr = nNewActiveRec + m_nFirstIdxSqlRec;
			nCfr = nNewActiveRec - m_pRadarBody->GetGrpTableNrRows() + GetLastIdxSqlRec();
			nRecEnd = min(GetSizeCircleList() - 1, nCfr);
		}
		// Aggiunge i nuovi record nella tabelle delle righe formattate, aggiungendo in coda
		for (int nIdx = GetLastIdxSqlRec(); nIdx <= nRecEnd; nIdx++)
		{
			AddRowTailGrpTable(nIdx);
			m_nFirstIdxSqlRec++;
			nActiveGrpRow--;
		}
		nNewActiveRec = m_pRadarBody->GetGrpTableNrRows() - 1;
		GetRadarView()->InvertEasyReadingCr();
	}
	ASSERT(nNewActiveRec >= 0);
	m_pRadarBody->m_nFirstIdxGrpRow = nNewActiveRec - GetRadarView()->GetAvailableVisibleRows() + 1;
	m_pRadarBody->m_nActiveGrpRow = nActiveGrpRow;

	UpdateRadarView(UPDATE_FROM_ROW_DOWN);
	// Aggiorno eventualmente il record nel documento collegato
	OnRadarRecordSelected();
}

// Viene richiamata quando l'utente utilizza il DBLClick del mouse.
//-----------------------------------------------------------------------------
BOOL CRadarDoc::DblClkRecordSelect (int nRec)
{
	if (!AfxGetThemeManager()->ShowRadarEdit() && IsEnableStayAlive())
	{
		GetRadarFrame()->CloseRadar();
		return FALSE;
	}

	if (!CanDoRecordSelect())
		return FALSE;

	// Calcolo della riga attualmente selezionata e controllo che sia all'interno del
	// rango dei record formattati.
	nRec += m_pRadarBody->m_nFirstIdxGrpRow;
	if (nRec < GetLastIdxGrpRow ())
	{
		// Se è stay alive aggiorno il radar
		if (IsStayAlive())
			UpdateAllViewWithRec(nRec, UPDATE_OPTIMIZATION);

		if (CanDoEditRecord())
			AfxInvokeAsyncThreadProcedure<CRadarDoc, BOOL>(::GetCurrentThreadId(), this, &CRadarDoc::RadarRecordSelected, TRUE, FALSE);
			
		return TRUE;
    }
	return FALSE;
}
//-----------------------------------------------------------------------------
void CRadarDoc::ChangeSelVisibleRow (int nRec, BOOL bIsJustActivated)
{
	if (!CanDoChangeRowSel())
		return;

	ASSERT (GetSizeCircleList());
	// Sposta la selezione 
	nRec += m_pRadarBody->m_nFirstIdxGrpRow;
	if ( 
		(nRec == m_pRadarBody->m_nActiveGrpRow || nRec >= GetLastIdxGrpRow ()) 
		&&
		!m_bFirstRun
	   )
		return;

	m_bFirstRun = FALSE;

	if (bIsJustActivated)
		UpdateAllViewWithRec(nRec, UPDATE_VIEW);
	else
		UpdateAllViewWithRec(nRec, UPDATE_OPTIMIZATION);
}

// Inverte la corrente direzione di scansione della tabella, provvedendo a
// spostare il cursore nella posizione corretta e a caricare i relativi
// Sql record nell'array circolare.
//-----------------------------------------------------------------------------
void CRadarDoc::InvertCursorDirection(BOOL bNextDirection)
{
	// Disabilitazione dell'applicazione e visualizzazione del cursore a clessidra
	BeginWaitCursor();

	// Sposta il cursore della tabella nella posizione corretta in accordo con la
	// nuova direzione di scansione
	m_pSqlRecCircleList->SkipRows (bNextDirection);
	m_bNextDirection = bNextDirection;

	// Abilitazione dell'applicazione e visualizzazione del cursore precedente
	EndWaitCursor();
}

// Controlla se è possibile mantenere ancora attiva la condizione di ricerca corrente
// o disabilitare la ricerca stupida
//-----------------------------------------------------------------------------
void CRadarDoc::FindStupidReset (const DataType& aDataType)
{
	if (!CanDoFindStart() || (IsFindCondActive () && aDataType != m_pFindCond->m_pDataObj->GetDataType()))
		FindStupidDisable ();

	// set default search mode in select the collum
	if (aDataType.m_wType == DATA_STR_TYPE || aDataType.m_wType == DATA_TXT_TYPE ) 
		m_iFindCondition = ECompareType::CMP_CONTAINS;
	else
		m_iFindCondition = ECompareType::CMP_EQUAL;
}

// Disabilita la ricerca stupida resettando le condizioni di ricerca
//-----------------------------------------------------------------------------
void CRadarDoc::FindStupidDisable ()
{
	if (m_pFindCond)
	{
		delete m_pFindCond;
		m_pFindCond = NULL;
	}
}

//-----------------------------------------------------------------------------
void CRadarDoc::SetWidthSelColumn (int nWidth)
{
	m_pRadarBody->SetWidthSelColumn (nWidth);
	m_bEnableSave = TRUE;
	UpdateRadarView (UPDATE_VIEW_SCROLL_TITLEBAR);
}

// Gestisce la corretta visualizzazione dei SqlRecord, ad ogni variazione delle
// dimensioni della finestra, dei font e della tabella (SqlTable).
//-----------------------------------------------------------------------------
void CRadarDoc::ShowCorrectRows ()
{
	// Controlla se la nuova riga attiva è all'interno della range di righe visibili
	int nVisibleRows = GetRadarView()->GetVisibleRows();
	if (m_pRadarBody->IsInsideVisibleRowsRange (m_nFirstIdxSqlRec, GetSizeCircleList() - 1, nVisibleRows, m_bNextDirection))
		return;

	int nRecEnd = m_nFirstIdxSqlRec + m_pRadarBody->m_nFirstIdxGrpRow + nVisibleRows;
	if	(
			nRecEnd >= GetSizeCircleList()									&&
			!LoadCircleListNext(m_pRadarBody->m_nNrMaxFormatRows, nRecEnd)	&&
			m_pRadarBody->GetGrpTableNrRows() <= nVisibleRows
		)
	{
		m_pRadarBody->m_nFirstIdxGrpRow = 0;
		return;
	}

	// adds the new record obj to format array
	for (int nIdx = GetLastIdxSqlRec(); nIdx < nRecEnd; nIdx++)
	{
		AddRowTailGrpTable (nIdx);
		m_nFirstIdxSqlRec++;
	}
	m_pRadarBody->SetActiveAndFirstIdxGrpRow (FROM_SHOW_ROW, nVisibleRows);
}

// Permette di cambiare lo stato di 'stay alive'. Se è attivo viene disattivato 
// altrimenti attivato. Il collegamento automatico tra record selezionato e sua
// visualizzazione all'interno del documento sara abilitato solo se è attivo
// lo stato di 'stay alive'.
//-----------------------------------------------------------------------------
BOOL CRadarDoc::ChangeStayAlive()
{
	m_bStayAlive = !m_bStayAlive;
	m_bLinkRadar = m_bStayAlive;
	return m_bStayAlive;
}

// Connette il radar al documento passato come parametro (chiamata da DBT).
// Si assume una frame e un solo documento
//-----------------------------------------------------------------------------
void CRadarDoc::Attach (CAbstractFormDoc* pCallerDoc)
{
	ASSERT_VALID (pCallerDoc);

	ASSERT (m_pCallerDoc		== NULL);
	ASSERT (m_pCallerHotLink	== NULL);
	ASSERT (m_pCallerSqlRecord	== NULL);
	ASSERT (m_pDynamicSqlRecord == NULL);
	ASSERT (m_pTable			== NULL);

	m_pCallerDoc		= pCallerDoc;
	m_pCallerSqlRecord	= pCallerDoc->m_pDBTMaster->GetRecord();
	m_pDynamicSqlRecord	= m_pCallerSqlRecord->Create();

	// uso una tabella locale perche modifico la query in maniera libera. La tabella
	// viene chiusa e distrutta in ~CRadarDoc
	SqlTable* pTable = new SqlTable (m_pDynamicSqlRecord, pCallerDoc->m_pTbContext->GetReadOnlySqlSession());
	
	// Carico la ORDER BY e la WHERE presente nel documento oppure quella costruita
	// per fare una find intelligente
	m_pCallerDoc->PrepareRadarQuery(pTable);

	// inizializzo il namespace del radar come il documento che lo gestisce
	GetNamespace() = m_pCallerDoc->GetNamespace();

	StartRadar (pTable);
}

// Il radar parte da solo e permette di lanciare il documento ......
//-----------------------------------------------------------------------------
void CRadarDoc::Attach (CTBNamespace aCallerNamespace, CRuntimeClass* pRecordClass)
{                                                                          
	ASSERT (pRecordClass);

	ASSERT (aCallerNamespace.IsValid() && aCallerNamespace.GetType() == CTBNamespace::DOCUMENT);
	ASSERT (m_pCallerDoc		== NULL);
	ASSERT (m_pCallerHotLink	== NULL);
	ASSERT (m_pCallerSqlRecord	== NULL);
	ASSERT (m_pDynamicSqlRecord == NULL);
	ASSERT (m_pTable			== NULL);

	m_CallerDocNamespace= aCallerNamespace;
	m_pDynamicSqlRecord = (SqlRecord*) pRecordClass->CreateObject();
	m_pCallerSqlRecord	= m_pDynamicSqlRecord;

	ASSERT (!m_pCallerSqlRecord->GetTableName().IsEmpty());
	StartRadar	(new SqlTable(m_pCallerSqlRecord, m_pCallerDoc->m_pSqlConnection->GetDefaultSqlSession()));
	
	EnableStayAlive	(TRUE, TRUE);
	GetRadarFrame()->SetFixedCheck();
} 	

// Connette il radar all'hotlink passato come parametro. La tabella è di
// proprietà dell'hotlink e il radar deve solo aprirla, data che la query è
// già stata preperata. La tabella deve essere richiusa quando il radar muore.
//-----------------------------------------------------------------------------
void CRadarDoc::Attach (HotKeyLink* pCallerHotLink, SqlTable* pTable, SqlRecord* pCallerRec)
{
	ASSERT_VALID (pCallerHotLink);
	ASSERT_VALID (pTable);
	ASSERT_VALID (pCallerRec);

	ASSERT (m_pCallerDoc		== NULL);
	ASSERT (m_pCallerHotLink	== NULL);
	ASSERT (m_pCallerSqlRecord	== NULL);
	ASSERT (m_pDynamicSqlRecord == NULL);
	ASSERT (m_pTable			== NULL);

	m_pCallerHotLink	= pCallerHotLink;
	m_pCallerSqlRecord	= pCallerRec;

	// La tabella non deve essere creata ma si deve utilizzare quella dell'HotLink.
	StartRadar(pTable, TRUE);
}

// Sconnette il radar dal document o dall'hotlink a cui era collegato.
//-----------------------------------------------------------------------------
void CRadarDoc::Detach ()
{
	// detach class
	if (m_CallerDocNamespace.IsValid())
	{
		ASSERT (m_pCallerHotLink == NULL);
		m_pCallerDoc	= NULL;
		m_CallerDocNamespace = CTBNamespace();
		// delete SqlRecord evetually created from attach(pClass, pRecordClass)
		if (m_pDynamicSqlRecord)
		{
			delete m_pDynamicSqlRecord;
			m_pDynamicSqlRecord = NULL;
		}
		return;
	}
	// Scollega il documento (chiamata da DBT)
	if (m_pCallerDoc)
	{
		ASSERT (m_pCallerHotLink == NULL);
		m_pCallerDoc = NULL;
		return;
	}
	// Scollega l'HotLink
	if (m_pCallerHotLink)
	{	
		ASSERT (m_pCallerDoc == NULL);
		m_pCallerHotLink = NULL;
	}
}

//-----------------------------------------------------------------------------
void CRadarDoc::OnRadarRecordSelected ()
{
	if (CanDoLinkRecord())
		RadarRecordSelected();
}

// Ritorna all'oggetto attualmente collegato al radar il record selezionato
// dall'utente (con DblClick).
//-----------------------------------------------------------------------------
void CRadarDoc::RadarRecordSelected(BOOL bActivateDoc /* FALSE */, BOOL bForceEditing)
{
	// Selezione del record collegato con il Documento DBT
	if (m_pCallerDoc) 
	{
		ASSERT (m_pCallerHotLink == NULL);
		// Abilitazione dell'applicazione e visualizzazione del cursore precedente
		if (
				m_pCallerDoc->IsModified() &&
				m_pCallerDoc->GetFormMode() != CBaseDocument::BROWSE &&
				AfxMessageBox(_TB("The document has been changed.\r\nIgnore changes and continue?"), MB_YESNO) != IDYES
			)
		{
			CRadarView *pView = GetRadarView();
			pView->ResetActionAndCursor();
			pView->ResetMouse();	
			return;
		}

		int nRec = m_pRadarBody->m_nActiveGrpRow + m_nFirstIdxSqlRec;
		SqlRecord* pRec = m_pSqlRecCircleList->GetAt (nRec);
		if (!pRec || !m_pCallerDoc->DispatchOnValidateRadarSelection(pRec, m_pCallerHotLink))
		{
			CRadarView *pView = GetRadarView();
			pView->ResetActionAndCursor();
			pView->ResetMouse();
			return;
		}

		*m_pCallerSqlRecord = *pRec;
		m_pCallerDoc->OnRadarRecordSelected(!IsStayAlive() || !IsActiveLinkRadar() || bActivateDoc, bForceEditing);
		return;
	}
	// Selezione del record collegato con l'HotLink
	if (m_pCallerHotLink)
	{	
		ASSERT_VALID (m_pTable);
		ASSERT (m_pTable->IsOpen());
		ASSERT (m_pCallerDoc == NULL);

		int nRec = m_pRadarBody->m_nActiveGrpRow + m_nFirstIdxSqlRec;
		SqlRecord* pRec = m_pSqlRecCircleList->GetAt (nRec);
		if (
				! m_pCallerHotLink->OnValidateRadarSelection(pRec) 
			|| 
				(
					m_pCallerHotLink->GetAttachedDocument() && m_pCallerHotLink->GetAttachedDocument()->IsKindOf(RUNTIME_CLASS(CAbstractFormDoc)) 
					? 
					! ((CAbstractFormDoc*)(m_pCallerHotLink->GetAttachedDocument()))->DispatchOnValidateRadarSelection(pRec, m_pCallerHotLink)
					:
					FALSE
				)
			)
		{
			Message(m_pCallerHotLink->GetErrorString(TRUE));

			CRadarView *pView = GetRadarView();
			pView->ResetActionAndCursor();
			pView->ResetMouse();
			return;
		}
		else if (!m_pCallerHotLink->GetWarningString(FALSE).IsEmpty())
		{
			Message(m_pCallerHotLink->GetWarningString(TRUE),0,0,0,CMessages::MSG_WARNING);
		}
			
		if (	
			AfxIsActivated(TESTMANAGER_APP, _NS_ACT("TBMacroRecorder")) &&
			AfxGetApplicationContext()->m_MacroRecorderStatus == CApplicationContext::RECORDING && 
			m_pCallerHotLink->GetAttachedDocument() &&
			m_pCallerHotLink->GetAttachedDocument()->IsKindOf(RUNTIME_CLASS(CAbstractFormDoc)) && 
			!m_pCallerHotLink->IsHotLinkFromControl()
			)
		{
			//ruoto l'informazione solo al MacroRercorder (TRUCCO by BAUZI e BRUNI)
			CAbstractFormDoc* pDoc = (CAbstractFormDoc*)(m_pCallerHotLink->GetAttachedDocument());
			CClientDoc* pMacroRec = pDoc->GetClientDoc(_T("TestManager.TBMacroRecorder.TBMacroRecorder.CDMacroRecorder"));
			if (pMacroRec)
				pMacroRec->OnValidateRadarSelection(pRec, m_pCallerHotLink->GetNamespace());
		}

		*m_pCallerSqlRecord = *pRec;
		// La chiusura della tabella deve essere NECESSARIAMENTE ESEGUITA PRIMA
		// della chiamata OnRadarRecordSelected dell'HotLink
		m_pTable->Close ();
		m_pCallerHotLink->OnRadarRecordAvailable();
	}
}

// Aggiorna l'indice dello stile dei caratteri e il font utilizzato nel radar.
//-----------------------------------------------------------------------------
void CRadarDoc::FontStyleUpdate (CString& strNewFontStyle)
{
	// Se lo stile del carattere è uguale a quello attualmente selezionato non si
	// deve fare niente
	int nIdx = AfxGetFontStyleTable()->GetFontIdx(strNewFontStyle);
	if (nIdx == m_nIdxFontStyle)
		return;

	m_nIdxFontStyle	= nIdx;
	m_strFontStyle	= strNewFontStyle;
	m_bEnableSave	= TRUE;

	const FontStyle* pFont = AfxGetFontStyleTable()->GetFontStyle(m_nIdxFontStyle, &GetNamespace());
	if (pFont)
		GetRadarView ()->FontChanged (pFont->GetLogFont());
	UpdateRadarView (UPDATE_SCROLL_SIZE_FORCE_REPAINT);
}

// Controlla se il font indicato esiste nella tabella dei stili del carattere dell'
// applicazione. Se non esiste viene emmesso un messaggio nella finestra dei messaggi
//-----------------------------------------------------------------------------
BOOL CRadarDoc::CheckExistFontStyle (CString& strFontStyle, CString& strNewStyle, const CString& strErr)
{
	// Controllo se lo stile esiste nella tabella degli stili
	m_nIdxFontStyle = (int) AfxGetFontStyleTable()->GetFontIdx(strFontStyle);
	if (m_nIdxFontStyle != AfxGetFontStyleTable()->GetFontIdx(FNT_DEFAULT))
	{
		m_strFontStyle = strFontStyle;
		return TRUE;
	}
	// Stile non trovato, emetto relativo messaggio di errore nella CMessage
	CString	msg = cwsprintf (strErr, (LPCTSTR) strFontStyle, (LPCTSTR) strNewStyle);
	AfxGetDiagnostic()->Add (msg);
	return FALSE;
}

// Viene richiamata ogni volta che viene alterata la tabella degli stili
// dell'applicazione e la prima volta che carico il font.
//-----------------------------------------------------------------------------
LOGFONT CRadarDoc::LoadDefaultFontStyle ()
{
	// Caricamento delle stringhe
	CString strFontStyle1, strFontStyle2;
	strFontStyle1 = szSTYLE_RADAR;
	strFontStyle2 = szSTYLE_CELL_NUM;

	if (!CheckExistFontStyle (strFontStyle1, strFontStyle2, _TB("The font style '{0-%s}' is wrong or has been deleted.\r\nThe font style '{1-%s}' will be used instead.")))
	{
		strFontStyle1 = FNT_STANDARD;
		if (!CheckExistFontStyle (strFontStyle2, strFontStyle1, _TB("The font style '{0-%s}' is wrong or has been deleted.\r\nThe font style '{1-%s}' will be used instead.")))
		{
			m_nIdxFontStyle = (int) AfxGetFontStyleTable()->GetFontIdx(strFontStyle1);
			m_strFontStyle	= strFontStyle1;
		}
	}
	const FontStyle* pFont = AfxGetFontStyleTable()->GetFontStyle(m_nIdxFontStyle, &GetNamespace());

	return pFont->GetLogFont();
}

// Viene richiamata dalla frame ogni volta che viene modificatoi lo stile del
// carattere.
//-----------------------------------------------------------------------------
void CRadarDoc::OnFontStyleChanged ()
{
	CString strFontStyle1;
	strFontStyle1 = szSTYLE_RADAR;

	const FontStyle* pFont = AfxGetFontStyleTable()->GetFontStyle(m_nIdxFontStyle, &GetNamespace());

	if (CheckExistFontStyle (m_strFontStyle, strFontStyle1, _TB("The font style '{0-%s}' is wrong or has been deleted.\r\nThe font style '{1-%s}' will be used instead.")))
		GetRadarView ()->FontChanged (pFont->GetLogFont());
	else
		GetRadarView ()->FontChanged (LoadDefaultFontStyle());

	// Aggiorno la combo box degli stili del carattere
	GetRadarFrame()->FillFontStyleComboBox (m_nIdxFontStyle);

	// Visualizza gli eventuali errori durante l'inizializzazione del radar
	AfxGetDiagnostic()->Show (TRUE);
	m_bEnableSave = TRUE;
	UpdateRadarView(UPDATE_SCROLL_SIZE);
}

//-----------------------------------------------------------------------------
void CRadarDoc::FormatStyleChanged ()
{
	// @@ TODO AGGIORNAMENTO DELLO STILE Di ogni singola COLONNA se modificato o cancellato
}

// Camcella le colonne selezionate
//-----------------------------------------------------------------------------
void CRadarDoc::OnOrderByAsc()
{
	if (CanDoOrderBy())
		MakeOrderBy ();
}

// Camcella le colonne selezionate
//-----------------------------------------------------------------------------
void CRadarDoc::OnOrderByDesc()
{
	if (CanDoOrderBy())
		MakeOrderBy (TRUE);
}

//-----------------------------------------------------------------------------
void CRadarDoc::MakeOrderBy (BOOL bDesc /* FALSE */)
{
	ASSERT_VALID (m_pRadarBody);
	CStringArray fieldArray;
	m_pRadarBody->GelAllSelColumnsName (fieldArray);
	if (m_pQueryManager->MakeQueryOrderBy (fieldArray, bDesc))
		OnExecQuery ();
}

// Cancella le colonne selezionate
//-----------------------------------------------------------------------------
void CRadarDoc::OnDeleteCols()
{
	if (!CanDoDeleteCols())
		return;

	m_pRadarBody->RemoveSelectedColumns(m_pDeletedColumns);

	// Dopo la cancellazione delle colonne disabilito la find stupida
	FindStupidDisable();
	m_bEnableSave = TRUE;
	UpdateRadarView (UPDATE_VIEW_SCROLL_TITLEBAR);
}

// Ricerca il successivo dato obj selezionato nelle condizioni di ricerca.
//-----------------------------------------------------------------------------
void CRadarDoc::OnFindNext ()
{
	ExecuteEditBoxQuery();
}

// Ricerca il precedente dato obj selezionato nelle condizioni di ricerca.
//-----------------------------------------------------------------------------
void CRadarDoc::OnFindPrev ()
{
	if (!CanDoFindNextPrev())
		return;

	m_pFindCond->m_nNumCond = m_iFindCondition;
	m_pFindCond->m_bDown = FALSE;
	if (!DataObjAssign()) return;

	StartFindStupid();
}

// Richiede l'imputazione dei dati per la ricerca stupida in base al tipo di dato
// presente nelle colonne selezionate attraverso finestre di dialogo differenti
//-----------------------------------------------------------------------------
void CRadarDoc::OnFindStart()
{
	if (!CanDoFindStart())
		return;

	// Controllo se era già attiva un precedente ricerca per quel tipo di dato
	if (m_pFindCond == NULL)
		m_pFindCond = new RadarFindCondition();

	// Non sono state inizializzate le condizioni di find stupida
	if (m_pFindCond->m_pDataObj == NULL)
	{
		RadarColumn* pCol = NULL;
		// Ricerco la prima colonna selezionata, dato che questa funzione è abilitata solo quanto
		// le colonne sono dello stesso tipo di dato, per impostare le condizioni di find in accordo
		// con il dato Obj in essa contenuto.
		for (int nIdx = 0; nIdx < m_pRadarBody->GetGrpTableNrCols(); nIdx++)
		{
			pCol = m_pRadarBody->GetColumn(nIdx);
			if (pCol->IsSelected())
				break;
	    }
        ASSERT_VALID (pCol);
		m_pFindCond->m_pDataObj = ((m_pCallerSqlRecord->GetDataObjAt(pCol->GetIdx()))->DataObjClone());
		m_pFindCond->m_pDataObj->Clear();
	}
}

//-----------------------------------------------------------------------------
BOOL CRadarDoc::DataObjAssign()
{
	CTBTabbedToolbar* pTabbedBar = GetRadarFrame()->GetTabbedToolBar();
	if (!pTabbedBar) return FALSE;
	CTBToolBar* pToolBar = pTabbedBar->FindToolBar(ID_TOOLBAR_FIND_EDITBOX);
	if (!pToolBar) return FALSE;
	CString sSearch = pToolBar->GetTextContent(ID_TOOLBAR_FIND_EDITBOX);
	if (sSearch.IsEmpty())	return FALSE;
		
	DataObj* pDataObj = m_pFindCond->m_pDataObj;

	if (pDataObj->GetDataType().m_wType == DATA_ENUM_TYPE)
	{
		((DataEnum*) pDataObj)->AssignByTitle(sSearch);
	}
	else
	{
		pDataObj->Assign(sSearch);
	}

	return TRUE;
}

//-----------------------------------------------------------------------------
void CRadarDoc::ExecuteEditBoxQuery()
{
	OnFindStart();

	if (m_pFindCond == NULL)
		return;

	m_pFindCond->m_bDown = TRUE;
	m_pFindCond->m_nNumCond = m_iFindCondition;

	if (!DataObjAssign()) return;

	DataObj* pDataObj = m_pFindCond->m_pDataObj;

	if (pDataObj->IsValid())
		StartFindStupid();
	else
	{
		int nIdx = 0;
		for (nIdx = 0; nIdx < m_pRadarBody->GetGrpTableNrCols(); nIdx++)
			if (m_pRadarBody->GetColumn(nIdx)->IsSelected())
				break;
		AfxMessageBox(
			cwsprintf(_TB("The column '{0-%s}' contains a type of data not handled by the search box."), (LPCTSTR)(m_pRadarBody->GetColumn(nIdx)->GetName())),
			MB_OK | MB_ICONSTOP
			);
		FindStupidDisable();
	}
}

//-----------------------------------------------------------------------------
void CRadarDoc::OnEditBoxQuery ()
{
	ExecuteEditBoxQuery();

	CTBTabbedToolbar* pTabbedBar = GetRadarFrame()->GetTabbedToolBar(); if (!pTabbedBar) return;
	CTBToolBar* pToolBar = pTabbedBar->FindToolBar(ID_TOOLBAR_FIND_EDITBOX); if (!pToolBar) return;
	pToolBar->SetTextFocus(ID_TOOLBAR_FIND_EDITBOX);
}

//-----------------------------------------------------------------------------
LRESULT CRadarDoc::FillAutoComple(WPARAM wp, LPARAM lp)
{
	int nCtrlID = (int)wp;
	if (nCtrlID != ID_TOOLBAR_FIND_EDITBOX)
	{
		return 0;
	}

	CTBTabbedToolbar* pTabbedBar = GetRadarFrame()->GetTabbedToolBar();
	if (!pTabbedBar) return 0;
	CTBToolBar* pToolBar = pTabbedBar->FindToolBar(ID_TOOLBAR_FIND_EDITBOX);
	if (!pToolBar) return 0;
	CString sSearch = pToolBar->GetTextContent(ID_TOOLBAR_FIND_EDITBOX);

	RadarColumn* pCol = NULL;
	for (int nIdx = 0; nIdx < m_pRadarBody->GetGrpTableNrCols(); nIdx++)
	{
		pCol = m_pRadarBody->GetColumn(nIdx);
		if (pCol->IsSelected())
			break;
	}
    ASSERT_VALID (pCol);
	DataObj* pDataObj = ((m_pCallerSqlRecord->GetDataObjAt(pCol->GetIdx()))->DataObjClone());
	if (pDataObj && pDataObj->GetDataType().m_wType == DATA_ENUM_TYPE)
	{
		CStringList* pList = (CStringList*)lp;
		ASSERT(pList != NULL);
		DataEnum* pDataEnum = ((DataEnum*)pDataObj);
		const EnumItemArray* pEnumItemArray = pDataEnum->GetEnumItems();
		for (int i = 0; i <= pEnumItemArray->GetUpperBound(); i++)
		{
			EnumItem* pItem = pEnumItemArray->GetAt(i);
			if (sSearch.Trim().IsEmpty())
			{
				pList->AddTail(pItem->GetTitle());
			}
			else
			{
				CString sItem = pItem->GetTitle();
				if (sItem.MakeUpper().Find(sSearch.MakeUpper()) >= 0)
				{
					pList->AddTail(pItem->GetItemName());
				}
			}
		}		
	}
	return 0;
}

//-----------------------------------------------------------------------------
void CRadarDoc::SetDefaultSelect(const CString& sQualifiedName)
{
	// Find the colum
	int nCol = m_pRadarBody->FindColumn(sQualifiedName);
	if (nCol < 0) return;
	// Select the colum
	int nXStart, nXEnd;
	DataType dataType = m_pRadarBody->SelectColumn	(nCol, nXStart, nXEnd);

	if (dataType != DATA_NULL_TYPE)
	{
		FindStupidReset	(dataType);
	}

	if (GetRadarFrame () && GetRadarFrame ()->GetTitleBar())
		GetRadarFrame ()->GetTitleBar()->InvalidateUpdateRect	(nXStart, nXEnd);

	CTBTabbedToolbar* pTabbedBar = GetRadarFrame()->GetTabbedToolBar();
	if (!pTabbedBar) return;
	CTBToolBar* pToolBar = pTabbedBar->FindToolBar(ID_TOOLBAR_FIND_EDITBOX);
	if (!pToolBar) return;
	if (!pTabbedBar->SetActiveTab(pToolBar->GetName())) return;
	pToolBar->SetTextFocus(ID_TOOLBAR_FIND_EDITBOX);
}

//-----------------------------------------------------------------------------
BOOL CRadarDoc::OnPopulatedDropDown    (UINT nIdCommand)
{
	CTBTabbedToolbar* pTabbedBar = GetRadarFrame()->GetTabbedToolBar();
	if (!pTabbedBar) return FALSE;

	pTabbedBar->OnPopulatedDropDown(nIdCommand);
	
	CTBToolBar* pToolBar = pTabbedBar->FindToolBar(ID_TOOLBAR_FIND_NEXT);

	RadarColumn* pCol = NULL;
	// Ricerco la prima colonna selezionata, dato che questa funzione è abilitata solo quanto
	// le colonne sono dello stesso tipo di dato, per impostare le condizioni di find in accordo
	// con il dato Obj in essa contenuto.
	for (int nIdx = 0; nIdx < m_pRadarBody->GetGrpTableNrCols(); nIdx++)
	{
		pCol = m_pRadarBody->GetColumn(nIdx);
		if (pCol->IsSelected())
			break;
	}
    ASSERT_VALID (pCol);


	if (nIdCommand == ID_TOOLBAR_FIND_NEXT)
	{
		DataObj* pDataObj = ((m_pCallerSqlRecord->GetDataObjAt(pCol->GetIdx()))->DataObjClone());
		DROPDOWN_MENU_POPOLATE(pToolBar, pDataObj, m_iFindCondition)
	}
	return TRUE;
}

//-----------------------------------------------------------------------------
void CRadarDoc::OnFindStartDlg()
{
	// init find cound object
	OnFindStart();

	// Selezione della finestra di dialogo per l'imputazione dei dati in base al tipo
	CRadarFindDlg* pDlg;
	switch (m_pFindCond->m_pDataObj->GetDataType().m_wType)
	{
		case DATA_ENUM_TYPE	: pDlg = new CRadarFindEnumDlg	(m_pFindCond); break;
		case DATA_STR_TYPE	: 
		case DATA_TXT_TYPE	: 
			pDlg = new CRadarFindStrDlg	(m_pFindCond); break;
		case DATA_INT_TYPE	: pDlg = new CRadarFindIntDlg	(m_pFindCond); break;
		case DATA_LNG_TYPE	: pDlg = new CRadarFindLngDlg	(m_pFindCond); break;
		case DATA_DBL_TYPE	: pDlg = new CRadarFindDblDlg	(m_pFindCond); break;
		case DATA_DATE_TYPE	: pDlg = new CRadarFindDateDlg	(m_pFindCond); break;
		case DATA_BOOL_TYPE	: pDlg = new CRadarFindBoolDlg	(m_pFindCond); break;
		case DATA_MON_TYPE	: pDlg = new CRadarFindMonDlg	(m_pFindCond); break;
		case DATA_PERC_TYPE	: pDlg = new CRadarFindPercDlg	(m_pFindCond); break;
		case DATA_QTA_TYPE	: pDlg = new CRadarFindQtaDlg	(m_pFindCond); break;
		default:
			
			int nIdx = 0;
			for (nIdx = 0; nIdx < m_pRadarBody->GetGrpTableNrCols(); nIdx++)
			 if (m_pRadarBody->GetColumn(nIdx)->IsSelected())
				 break;
			  AfxMessageBox	(
			  					cwsprintf (_TB("The column '{0-%s}' contains a type of data not handled by the radar."), (LPCTSTR) (m_pRadarBody->GetColumn(nIdx)->GetName())),
			  					MB_OK | MB_ICONSTOP
			  				);
			  FindStupidDisable ();
			  return;
	}
	ASSERT_VALID (pDlg);
	if (pDlg->DoModal() == IDOK)
		StartFindStupid ();
	else if (m_pFindCond->m_pDataObj->IsEmpty())
		FindStupidDisable ();

	delete pDlg;
}

//-----------------------------------------------------------------------------
void CRadarDoc::OnFindStop ()
{
	if (CanDoFindStop())
		// non devo cancellare lo scheduler perchè sono
		// durante il loop di IsAborted
		UserBreakFind(FALSE);
}

// Ricerca della prima colonna selezionata
//-----------------------------------------------------------------------------
int CRadarDoc::GetIdxFirstSelCol ()
{
	// Ricerca della colonna selezionata
	for (int nCol = 0; nCol < m_pRadarBody->GetGrpTableNrCols(); nCol++)
		if (m_pRadarBody->GetColumn(nCol)->IsSelected())
			return nCol;
	return -1;
}

// Inserisce una o più le colonne precedentemente cancellate
//-----------------------------------------------------------------------------
void CRadarDoc::OnInsertCols()
{
	if (!CanDoInsertCols ())
		return;

	// Utilizzo un array temporaneo per memorizzare gli indici e il titolo delle
	// colonne da inserire nuovamente nel radar
	ASSERT (m_pColsToInsert == NULL);
	m_pColsToInsert = new RadarColumnsIdxArray;
	CRadarInsertColumnsDlg dlg	(
									this,
									m_pDeletedColumns,
									m_pColsToInsert,
									m_pRadarBody->GetNrSelColumns () == 1
								);
	int res = dlg.DoModal();
	if (res == IDC_RADAR_INS_CKB_MOUSE)
	{
		GetRadarView()->InsertColumnWithMouse();		return;
	}
	else if (res == IDC_RADAR_INS_RB_TAIL)
	{
		AddDelColumnTailGrpTable();
	}
	else if (res == IDC_RADAR_INS_RB_HEAD)
	{
		AddDelColumnGrpTable(0);
	}
	else if (res == IDC_RADAR_INS_RB_PREV_COL)
	{
		AddDelColumnGrpTable(GetIdxFirstSelCol());
	}
	else if (res == IDC_RADAR_INS_RB_NEXT_COL)
	{
		AddDelColumnGrpTable(GetIdxFirstSelCol() + 1);
	}
	delete m_pColsToInsert;
	m_pColsToInsert = NULL;
}

// Spostano la colonna di una posizione verso sinistra
//-----------------------------------------------------------------------------
void CRadarDoc::OnShiftColLeft()
{
	if (!CanDoShiftColLeft())
		return;

	int nXStart, nXEnd;
	m_pRadarBody->ShiftColumnLeft (nXStart, nXEnd);
	GetRadarFrame ()->GetTitleBar()->InvalidateUpdateRect (nXStart, nXEnd);

	m_bEnableSave = TRUE;
	UpdateAllViews (NULL);

	if (nXStart <= GetRadarView()->GetScrollPosition().x)
		GetRadarView()->PostMessage(WM_COMMAND, ID_RADAR_PAGE_LEFT);
}

// Spostano la colonna di una posizione verso destra
//-----------------------------------------------------------------------------
void CRadarDoc::OnShiftColRight()
{
	if (!CanDoShiftColRight())
		return;

	int nXStart, nXEnd;
	m_pRadarBody->ShiftColumnRight (nXStart, nXEnd);
	GetRadarFrame ()->GetTitleBar()->InvalidateUpdateRect (nXStart, nXEnd);
	
	m_bEnableSave = TRUE;
	UpdateAllViews (NULL);

	CRect rect; GetRadarView()->GetClientRect(rect);
	
	if (nXEnd >= GetRadarView()->GetScrollPosition().x + rect.Width())
		GetRadarView()->PostMessage(WM_COMMAND, ID_RADAR_PAGE_RIGHT);
}

// Ridimensionano una o più colonne
//-----------------------------------------------------------------------------
void CRadarDoc::OnResizeColumns()
{
	// Calcolo la lunghezza di defaul del campo
	RadarIntArray widthColsArray;
	for (int nCol = 0; nCol < m_pRadarBody->GetGrpTableNrCols(); nCol++)
	{
		const SqlColumnInfo* pSqlColInfo = m_pCallerSqlRecord->GetColumnInfo(m_pRadarBody->GetColumn(nCol)->GetIdx());

		int nWidth	= max((int)_tcslen (pSqlColInfo->GetColumnTitle()), pSqlColInfo->GetColumnLength());
		if (nWidth < COLUMN_CHAR_WIDTH_MIN_VALUE)
			nWidth = COLUMN_CHAR_WIDTH_MIN_VALUE;
		else if (nWidth > COLUMN_CHAR_WIDTH_MAX_VALUE)
			nWidth = COLUMN_CHAR_WIDTH_MAX_VALUE;
		widthColsArray.AddInt(nWidth);
    }

	CRadarResizeColumnsDlg dlg (m_pRadarBody, &widthColsArray);
	if (dlg.DoModal() != IDOK)
		return;

	m_bEnableSave = TRUE;
	UpdateRadarView (UPDATE_VIEW_SCROLL_TITLEBAR);
}

// Esegue la query correntemente selezionata nella combo box contenuta nella tool bar
//-----------------------------------------------------------------------------
void CRadarDoc::OnExecQuery ()
{
	if (!CanDoExecQuery ())
		return;

	if (OpenTableAndCheckIsEmpty ())
	{
		// La tabella è vuota  
		// Rimozione delle righe nella lista circolare e nella tabella grafica
		m_pSqlRecCircleList->	RemoveAll		();
		m_pRadarBody->			RemoveAllRows	();

		UpdateAllViewWithRec(-1);
		return;
	}
	GoHome();
}

//-----------------------------------------------------------------------------
void CRadarDoc::OnEditQuery ()
{
	if (!CanDoEditQuery())
		return;

	m_bEditingQuery = TRUE;
	m_pQueryManager->EditQuery();
// Attivazione
	CRadarFrame* pFrame = GetRadarFrame();

	pFrame->CBCGPFrameWnd::ActivateFrame();

	m_bEditingQuery = FALSE;


}

// Esegue la query correntemente selezionata nella combo box contenuta nella tool bar
//-----------------------------------------------------------------------------
BOOL CRadarDoc::OpenTableAndCheckIsEmpty ()
{
	// Se la tabella è già aperta posso fare una requery, altrimenti devo riaprirla
	// per visualizzare il contenuto della nuova query impostata dall'utente attraverso
	// BarQuery.
	ASSERT_VALID (m_pTable);
	BOOL bEmpty = FALSE;

	TRY
	{
		if (!m_pTable->IsOpen())
		{
			m_pTable->Open(FALSE, E_KEYSET_CURSOR, m_pCallerHotLink == NULL);

			if (m_pCallerHotLink)
			{
				if (m_pTable->m_pColumnArray->GetSize() == 0)
					m_pCallerHotLink->SelectColumns(m_pTable);
			}
			else if (m_pCallerDoc)
			{
				if (m_pTable->m_pColumnArray->GetSize() == 0)
					m_pCallerDoc->SelectRadarColumns(m_pTable);
			}
			else
				m_pTable->SelectAll();
		}
		else
			m_pTable->Invalidate();

		m_pTable->Query();
		bEmpty = m_pTable->IsEmpty();
		m_nRowCount = (bEmpty) ? 0 : m_pTable->GetExtractedRows();
	}
	CATCH(SqlException, e)	
	{
		AfxMessageBox (e->m_strError);
		THROW_LAST();
	}
	END_CATCH

	return bEmpty;
}

// Ritorna la stringa da visualizzare sul corpo del radar, a secondo se la tabella
// è effettivamente vuota oppure solo la query attualmente utilizzata
//-----------------------------------------------------------------------------
CString CRadarDoc::GetEmptyTableCaption ()
{
	return m_pTable->m_strFilter.IsEmpty () 
		?_TB("The table:") 
		:_TB("The query currently used for the table:");
}

//-----------------------------------------------------------------------------
LPCTSTR CRadarDoc::GetTableName ()
{
	ASSERT (m_pCallerSqlRecord);
	return m_pCallerSqlRecord->GetTableName();
}

//-----------------------------------------------------------------------------
CString CRadarDoc::GetTableTitle ()
{
	ASSERT_VALID (m_pCallerSqlRecord);
	return m_pCallerSqlRecord->GetTableTitle();
}

// Salvattaggio degli attributi grafici
//-----------------------------------------------------------------------------
void CRadarDoc::SaveGraphicAttrib (BOOL bAskConfirm /* FALSE */)
{
	if (m_bIsSaving)
		return;
	m_bIsSaving = TRUE;

	// Devo controllare se sia stato inizializzato il file su cui scrivere le informazioni di configurazione
	if (!CheckRadarFullFileName(TRUE))
	{
		m_bIsSaving = FALSE;
		return;
	}

	// se non ho un namespace, non salvo le personalizzazioni
	if (m_strRadarFile.IsEmpty())
	{
		if (CanDoSaveGrpAttrib())
			AfxMessageBox (_TB("Unable to save the customizations of this radar."), MB_OK | MB_ICONSTOP);
		m_bIsSaving = FALSE;
		return;
	}

	CString	strSection(m_pCallerSqlRecord->GetTableName());
	// Salva l'attuale dimensione e posizione del radar
	SaveWindowPlacement	(strSection);

	// Salvataggio dello release corrente
	WriteRadarProfileString (strSection, szTagRelease, szCurrentRelease);

	GetRadarView ()->WriteNoAskConfirm (strSection, m_strTempRadarFile);

	if (!CanDoSaveGrpAttrib())
	{
		m_bIsSaving = FALSE;
		return;
	}

	// Il flag va sempre resettato a false, in quando se l'utente decide non volere salvare
	// la configurazione grafica non devo richiederlo di nuovo.
	m_bEnableSave = FALSE;

	if (bAskConfirm && AfxMessageBox(_TB("The radar graphic attributes have been changed.\r\nDo you want to save them before exiting?")
		//+'\n' + m_strRadarFile
		, MB_YESNO|MB_ICONQUESTION) != IDYES)
	{
		m_bIsSaving = FALSE;
		return;
	}

	ASSERT (m_strFontStyle.IsEmpty() == FALSE);
	WriteRadarProfileString (strSection, szTagFontStyle, m_strFontStyle);

	// Rimuove gli eventuali precedenti valori delle colonne
	for (int i = 0; i < m_pDeletedColumns->GetSize(); i++)
	{
		int idx = m_pDeletedColumns->GetAt(i)->GetIdx();
		const SqlColumnInfo* pCol = m_pCallerSqlRecord->GetColumnInfo(idx);
		if (pCol)
			WriteRadarProfileString	(strSection, idx < m_pCallerSqlRecord->GetSize() ? pCol->GetColumnName() : pCol->GetQualifiedColumnName(), NULL);
	}

    // Salva le informazione relative alle colonne
	m_pRadarBody->WriteColumnsConfig (strSection, m_strTempRadarFile);

	IFileSystemManager* pFileSystemManager = AfxGetFileSystemManager();
	if (pFileSystemManager && pFileSystemManager->IsManagedByAlternativeDriver(m_strRadarFile))
		SaveFromVirtualDriver();		
}

// Procedura eseguita ogni volta che premo il tasto Escape
//-----------------------------------------------------------------------------
void CRadarDoc::OnEscape()
{
	// Se sto inserendo delle colonne cancellate con il mouse, non posso chiudere
	// il radar ma deve annullare l'inserimento.
	if (GetRadarView() && GetRadarView()->ResetInsertDeleteColumns())
	{
		ASSERT_VALID (m_pColsToInsert);
		// Gli oggetti da inserire vengono travasati nell'array originale.
		// Il distrutture dell'array degli oggetti da inserire non deve cancellare
		// gli oggetti per cui devo settare il relativo flag
		m_pColsToInsert->SetOwns (FALSE);
		for (int nIdx = 0; nIdx < m_pColsToInsert->GetSize (); nIdx++)
			m_pDeletedColumns->Add (m_pColsToInsert->GetAt (nIdx));

		delete m_pColsToInsert;
		m_pColsToInsert = NULL;
	}
	// Se è attiva la ricerca stupida, non posso chiudere il radar, ma deve annullare la ricerca.
	else if (IsActiveFind())
		UserBreakFind ();
	else if (GetRadarFrame())
		GetRadarFrame()->CloseRadar();
}

//-----------------------------------------------------------------------------
void CRadarDoc::GoRow (int nRow)
{
	GoHome(FALSE);
	// Verifico se la nuova riga attiva è contenuta tra quelle attaulmente visibili
	if (nRow < GetLastIdxGrpRow())
	{
		UpdateAllViewWithRec(nRow, UPDATE_VIEW);
		return;
	}
	NextRow (nRow, UPDATE_FROM_PAGE_DOWN);
}

//-----------------------------------------------------------------------------
void CRadarDoc::RowDown	()
{
	if (CanDoMoveDown ())
		NextRow (m_pRadarBody->GetActiveGrpRow() + 1, UPDATE_FROM_ROW_DOWN);
}
//-----------------------------------------------------------------------------
void CRadarDoc::RowUp ()
{
	if (CanDoMoveUp ())
		PrevRow (m_pRadarBody->GetActiveGrpRow() - 1, UPDATE_FROM_ROW_UP);
}
//-----------------------------------------------------------------------------
void CRadarDoc::PageDown ()
{
	if (CanDoMoveDown())
		NextRow (m_pRadarBody->GetIdxGrpRowNextPage(GetRadarView()->GetVisibleRows()), UPDATE_FROM_PAGE_DOWN);
}

//-----------------------------------------------------------------------------
void CRadarDoc::PageUp	()
{
	if (CanDoMoveUp())
		PrevRow (m_pRadarBody->GetIdxGrpRowPrevPage(GetRadarView()->GetVisibleRows()), UPDATE_FROM_PAGE_UP);
}

//-----------------------------------------------------------------------------
void CRadarDoc::PageEnd	()
{
	if (CanDoMoveDown())
	{
		DataBool* bNewEnd = (DataBool*) AfxGetSettingValue(snsTbGenlib, szForms, szRadarEndForward, DataBool(FALSE), szTbDefaultSettingFileName);
		if (bNewEnd && *bNewEnd)
			GoToEndRow ();
		else 
			GoEnd();
	}
}

//-----------------------------------------------------------------------------
void CRadarDoc::PageHome ()
{
	if (CanDoMoveUp())
		GoHome();
}

// Spostamento del cursore di una pagina l'alto
//-----------------------------------------------------------------------------
void CRadarDoc::OnPageUp ()
{
	PageUp();
	GetRadarView()->OnKeyScrollPageUp();
}

// Spostamento del cursore di una pagina verso il basso
//-----------------------------------------------------------------------------
void CRadarDoc::OnPageDown ()
{
	PageDown();
	GetRadarView()->OnKeyScrollPageDown();
}

// Spostamento in fondo alla tabella
//-----------------------------------------------------------------------------
void CRadarDoc::OnEnd ()
{
	PageEnd	();
	GetRadarView()->OnKeyScrollPageEnd();
}

// Spostamento in testa alla tabella
//-----------------------------------------------------------------------------
void CRadarDoc::OnHome()
{
	PageHome();
	GetRadarView()->OnKeyScrollPageHome();
}

// Spostamento del cursore di una riga verso l'alto
//-----------------------------------------------------------------------------
void CRadarDoc::OnRowUp ()
{
	RowUp();
	GetRadarView()->OnKeyScrollRowUp();
}

// Spostamento del cursore di una riga verso il basso
//-----------------------------------------------------------------------------
void CRadarDoc::OnRowDown ()
{
	RowDown();
	GetRadarView()->OnKeyScrollRowDown();
}

// Creazione di un nuovo record associato al radar
//-----------------------------------------------------------------------------
void CRadarDoc::OnNewRecord ()
{
	if (!CanDoNewRecord())
		return;

	if (m_pCallerHotLink == NULL)
	{
		if (!CreateCallerDoc ())
			return;

		ASSERT (m_pCallerDoc);
		m_pCallerDoc->OnRadarRecordNew();
		return;
	}

	if (!m_pCallerHotLink->IsEnabledAddOnFly())
		return;

	// serve per inibire che il radar esegua piu` volte l'apertura del documento
	// di inserimento (esempio doppio click sul bottone della toolbar)
	m_bDocumentCalled = TRUE;
    
	//PERASSO: an. #13012 disabilito la possibilita` di chiusura del radar,
	//che potrebbe essere innescata dalla CallLink in un momento in cui
	//ho ancora bisogno del radar "vivo";
	BOOL bWasEnableStayAlive = m_bEnableStayAlive;
	BOOL bWasStayAlive = m_bStayAlive;
	EnableStayAlive(TRUE, TRUE);
	
	BOOL bDiableDoCallLink = m_pCallerHotLink->IsDoCallLinkDisable();
	m_pCallerHotLink->SetAddOnFlyRunning (TRUE);

	m_pCallerHotLink->SetDoCallLinkDisable(TRUE);
	m_pCallerHotLink->DoCallLink(FALSE);
	m_pCallerHotLink->SetDoCallLinkDisable(bDiableDoCallLink);
	//@@ TODO rivedere logiche di new e modify per hotlink (bisogna chiamare
	// il documento giusto attraverso qualcosa di simile alla CallLink
	m_pCallerHotLink->CallLink();
	m_pCallerHotLink->SetAddOnFlyRunning (FALSE);
	
	//PERASSO: an. #13012 Scateno la chiusura del radar, se necessario, previa ripristino
	//dello stato di abilitazione della chiusura
	EnableStayAlive(bWasEnableStayAlive, bWasStayAlive);
	GetRadarFrame()->PostMessage(WM_ACTIVATE);
}

// Modifica di un record associato al radar
//-----------------------------------------------------------------------------
void CRadarDoc::OnEditRecord ()
{
	//asincrono, altrimenti la routing frame risulta essere quella del radar e una eventuale messagebox di retry lock tenterebbe
	//di prenderla come parent anche se sta morendo!
	if (!AfxGetThemeManager()->ShowRadarEdit() && IsEnableStayAlive())
		return;
	
	if (CanDoEditRecord())
		AfxInvokeAsyncThreadProcedure<CRadarDoc, BOOL>(::GetCurrentThreadId(), this, &CRadarDoc::RadarRecordSelected, TRUE, FALSE);
}

//-----------------------------------------------------------------------------
CRadarView*	CRadarDoc::GetRadarView ()
{
	POSITION pos = GetFirstViewPosition();
	CRadarView* pView = (CRadarView*) GetNextView(pos);
	//ASSERT_VALID (pView);
	return pView;
}

//-----------------------------------------------------------------------------
LONG CRadarDoc::GetSqlRowActive()
{
	if (m_pRadarBody)
		return m_pRadarBody->GetActiveGrpRow() + m_nFirstIdxSqlRec;

	return 0;
}

int			CRadarDoc::GetSizeCircleList()	{ return m_pSqlRecCircleList->GetSize(); }
RadarBody*	CRadarDoc::GetRadarBody		()	{ return m_pRadarBody;	}

//-----------------------------------------------------------------------------
BOOL CRadarDoc::WriteRadarProfileString (LPCTSTR lpszSection, LPCTSTR lpszEntry, LPCTSTR lpszString)
{
	if (m_strTempRadarFile.IsEmpty ())
		m_strTempRadarFile = GetTempName ();

	return ::WritePrivateProfileString (lpszSection, lpszEntry, lpszString, m_strTempRadarFile);
}

//-----------------------------------------------------------------------------
BOOL CRadarDoc::WriteRadarProfileInt (LPCTSTR lpszSection, LPCTSTR lpszEntry, int nValue) 
{ 
	if (m_strTempRadarFile.IsEmpty ())
		m_strTempRadarFile = GetTempName ();

	return ::WritePrivateProfileString (lpszSection, lpszEntry, cwsprintf (_T("%d"), nValue), m_strTempRadarFile);
}

//-----------------------------------------------------------------------------
BOOL CRadarDoc::CanDoInsertCols ()
	{ return m_pRadarBody && m_pDeletedColumns->GetSize() && IsFindNotActive (); }

//-----------------------------------------------------------------------------
BOOL CRadarDoc::CanDoOrderBy ()
	{ return CanDoExecQuery () && m_pRadarBody && m_pRadarBody->GetNrSelColumns () > 0; }

//-----------------------------------------------------------------------------
BOOL CRadarDoc::CanDoExecQuery ()
	{ return IsFindNotActive (); }

//-----------------------------------------------------------------------------
BOOL CRadarDoc::CanDoEditQuery ()
{ 
	BOOL bOk = IsFindNotActive ();
	if (!bOk) return FALSE;
	
	if (m_pCallerDoc)
	{
		bOk = OSL_CAN_DO(m_pCallerDoc->GetInfoOSL(), OSL_GRANT_EDITQUERY); 
	}
	else if (m_pCallerHotLink )
	{
		bOk = OSL_CAN_DO(m_pCallerHotLink->GetInfoOSL(), OSL_GRANT_EDITQUERY); 
	}
	return bOk;
}

//-----------------------------------------------------------------------------
BOOL CRadarDoc::IsActiveLinkRadar () 
{ 
	if (IsEnableStayAlive() && !AfxGetThemeManager()->ShowRadarFixed())
	{
		return TRUE;
	}

	return m_bLinkRadar;
}

//-----------------------------------------------------------------------------
BOOL CRadarDoc::CanDoEnable	() 
{ 
	return 
		m_pRadarBody && 
		CanDoExecQuery () && 
		IsFindNotActive() && 
		m_pSqlRecCircleList->GetSize();
}

//-----------------------------------------------------------------------------
BOOL CRadarDoc::CanDoFindEdit()
{
	return m_pRadarBody &&	m_pRadarBody->CanDoEnableFindStupid();
}

//-----------------------------------------------------------------------------
BOOL CRadarDoc::CanDoFindStart ()
	{ return 
		CanDoExecQuery ()						&&
		m_pRadarBody							&&
		m_pRadarBody->CanDoEnableFindStupid ()	&&
		m_pSqlRecCircleList->GetSize();
	}

//-----------------------------------------------------------------------------
BOOL CRadarDoc::CanDoNewRecord()
	{ return
		m_pRadarBody		&&
		IsFindNotActive()	&&
		!m_bDocumentCalled	&&
		(!m_pCallerHotLink || m_pCallerHotLink->IsEnabledAddOnFly());
	}

//-----------------------------------------------------------------------------
// Caricamento della configurazione di default per ogni singola colonna
// prelevandola direttamente dal data base

void CRadarDoc::AddColumnGrpTable ()
{ 
	SqlRecord* pRec = m_pTable ? m_pTable->GetRecord() : m_pCallerSqlRecord;

	int sz = pRec->GetSizeEx();
	for (int nIdx = 0; nIdx < sz; nIdx++)
		AddColumnGrpTable (nIdx, pRec);
}

//-----------------------------------------------------------------------------
int CRadarDoc::AddColumnGrpTable(int nIdx, SqlRecord* pRec)
{ 
	ASSERT_VALID(m_pTable);
	//SqlRecord* pRec = m_pTable ? m_pTable->GetRecord() : m_pCallerSqlRecord;

	const SqlColumnInfo* pSqlCol = pRec->GetColumnInfo(nIdx);

	CString sColName = pRec->GetQualifiedColumnName(nIdx);

	if (pSqlCol->m_bVirtual)
	{
		if (pSqlCol->m_bNativeColumnExpr && m_pTable && m_pTable->m_pColumnArray)
		{
			for (int i = 0; i < m_pTable->m_pColumnArray->GetSize(); i++)
			{
				SqlBindingElem* pSbi = m_pTable->m_pColumnArray->GetAt(i);
				ASSERT_VALID(pSbi);

				if (pSbi->GetLocalName().CompareNoCase(sColName) == 0)
				{
					return m_pRadarBody->AddColumn(nIdx, pSqlCol, sColName);
				}
			}
		}
		return -1;
	}

	if (m_pTable && m_pTable->m_pColumnArray->GetSize())
	{
		SqlBindingElem* pAcc = m_pTable->m_pColumnArray->GetParamByName(sColName);
		if (!pAcc)
			return -1;
	}
	return m_pRadarBody->AddColumn (nIdx, pSqlCol, sColName);
}

// Ritorna l'indice della colonna della tabella su cui si vuole eseguire la Find Stupida.
//-----------------------------------------------------------------------------
int CRadarDoc::GetColumnIdxFindCond (int nIdx)
	{ return m_pRadarBody->GetColumn(m_pFindCond->GetSelCol(nIdx))->GetIdx(); }

// Ritorna l'indice dell'ultimo record presente nell'array di record formattati,
// all'interno dell'array circolare.
//-----------------------------------------------------------------------------
int CRadarDoc::GetLastIdxSqlRec() {return m_nFirstIdxSqlRec + m_pRadarBody->GetGrpTableNrRows (); }

// Aggiunge in testa all'array di record formattati un nuovo record, prelevandolo dal buffer
// circolare ed inserendolo in testa.
//-----------------------------------------------------------------------------
void CRadarDoc::AddRowHeadGrpTable(int nRec)
{
	ASSERT_VALID (m_pRadarBody);
	m_pRadarBody->AddRowHeadGrpTable (m_pSqlRecCircleList->GetAt (nRec));
}

// Aggiunge in coda all'array di record formattati un nuovo record, prelevandolo dal buffer
// circolare ed inserendolo in coda.
//-----------------------------------------------------------------------------
void CRadarDoc::AddRowTailGrpTable (int nRec)
{
	if (nRec < 0)
		return;

	ASSERT_VALID (m_pRadarBody);
	m_pRadarBody->AddRowTailGrpTable (m_pSqlRecCircleList->GetAt (nRec));
}

// Aggiunge in coda all'array di record formattati un nuovo record, prelevandolo dal buffer circolare
//-----------------------------------------------------------------------------
void CRadarDoc::AddRowGrpTable (int nRec)
{
	ASSERT_VALID (m_pRadarBody);
	m_pRadarBody->AddRowGrpTable (m_pSqlRecCircleList->GetAt (nRec));
}
//-----------------------------------------------------------------------------
int CRadarDoc::GetLastIdxGrpRow ()
{
	ASSERT_VALID (m_pRadarBody);
	return m_pRadarBody->GetLastIdxGrpRow(GetRadarView ()->GetVisibleRows ());
}

// Ritorna un SqlRecord contenuto nell'array circolare aggiornando il contattore dei
// record attualmenti letti, visualizzato nella status bar.
//-----------------------------------------------------------------------------
SqlRecord* CRadarDoc::TicGetSqlRecord(int nRec) { m_nFindTic++; return m_pSqlRecCircleList->GetAt (nRec); }

// Ritorna un SqlRecord, letto dalla tabella nella direzione AVANTI/INDIETRO e inserito
// nell'array circolare, aggiornando il contattore dei record attualmenti letti,
// visualizzato nella status bar.
//-----------------------------------------------------------------------------
SqlRecord* CRadarDoc::TicInsertNextSqlRec() { m_nFindTic++; return m_pSqlRecCircleList->InsertNext(); }
SqlRecord* CRadarDoc::TicInsertPrevSqlRec() { m_nFindTic++; return m_pSqlRecCircleList->InsertPrev(); }

BOOL CRadarDoc::IsFindCondActive		() { return m_pFindCond && m_pFindCond->m_pDataObj; }

//-----------------------------------------------------------------------------
BOOL CRadarDoc::CanDoDeleteCols ()
	{ return m_pRadarBody && m_pRadarBody->CanDoEnableDeleteColumns() && IsFindNotActive ();}
//-----------------------------------------------------------------------------
BOOL CRadarDoc::CanDoShiftColLeft ()
	{ return m_pRadarBody && m_pRadarBody->CanDoEnableLeftShift() && IsFindNotActive (); }

//-----------------------------------------------------------------------------
BOOL CRadarDoc::CanDoShiftColRight ()
	{ return m_pRadarBody && m_pRadarBody->CanDoEnableRightShift () && IsFindNotActive (); }

///////////////////////////////////////////////////////////////////////////////
// Diagnostics
#ifdef _DEBUG
void CRadarDoc::Dump (CDumpContext& dc) const
{
	ASSERT_VALID (this);
	AFX_DUMP0(dc, " CRadarDoc\n");
	CAbstractDoc::Dump(dc);
}
#endif // _DEBUG

//-----------------------------------------------------------------------------
void CRadarDoc::ExportTableToODS ()
{
	if (!CanDoExport())
		return;

	CExportInfo oExpInfo;
	oExpInfo.SetExportType(TypeOfExport::EXPORT_OPENOFFICE_ODS_TYPE);

	ExportTable(&oExpInfo.m_nExportType);
}

//-------------------------------------------------------------------------
void CRadarDoc::ExportTableToExcel ()
{
	if (!CanDoExport())
		return;
	
	CExportInfo oExpInfo; 
	oExpInfo.SetExportType(TypeOfExport::EXPORT_EXCELNET_TYPE);

	ExportTable(&oExpInfo.m_nExportType);
}

//-------------------------------------------------------------------------
void CRadarDoc::ExportTable(TypeOfExport* pExp)
{
	if (!CanDoExport())
		return;

	BeginWaitCursor();

	SqlTable* pTable = m_pTable;
	SqlRecord* pRow = m_pCallerSqlRecord;
	CExportDataNet  m_ExpData;
	DataObj* pCol;	 
	CString strCell;
	int nRows;

	try 
	{	
		if ( ! pTable -> IsEmpty () )
		{
			nRows = 0;
			pTable -> MoveFirstCopy ( pRow );

			int nNumColumns = 0;
			int nCountCols = 0;
			nNumColumns = m_pRadarBody -> GetNrSelColumns ();
			if (nNumColumns == 0) 
				nNumColumns = m_pRadarBody -> GetGrpTableNrCols ();

			if (nNumColumns)
			{
				int* paIndCols = new int [nNumColumns];
				for (int nCol = 0; (nCol < m_pRadarBody -> GetGrpTableNrCols ()) && (nCountCols < nNumColumns); nCol++)
				{
					if ( m_pRadarBody -> GetNrSelColumns () &&
						 ! m_pRadarBody -> GetColumn (nCol) -> IsSelected () ) 
							continue;
					
					strCell = m_pRadarBody -> GetColumn (nCol) -> GetTitle ();
					paIndCols[nCountCols++] = pRow->Lookup( m_pRadarBody -> GetColumn (nCol) -> GetName () );
					m_ExpData.m_Table.m_arColumnTitles.Add(strCell);
				}

				DataObjArray* row;

				do
				{
					row = new DataObjArray;
				
					for (int nCol = 0; nCol < nNumColumns; nCol++)
					{
						pCol = pRow -> GetDataObjAt (paIndCols[nCol]);

						if (pCol->GetDataType() == DataType::Bool)
						{
							DataBool db;
							db.Assign(*pCol);

							DataType oDataType = DataType::String;
							DataObj* pDataValue = DataObj::DataObjCreate(oDataType);
							ASSERT_VALID(pDataValue);

							pDataValue->Assign(db.FormatData());
							row->Add(pDataValue);
						}
						else
							row->Add(pCol->Clone());
					}
					nRows++;
					m_ExpData.m_Table.m_Cells.Add(row);

				} while (pTable -> MoveNextCopy (pRow)) ;
				delete [] paIndCols;
			}
		}

		MExportNet Exp;
		Exp.Export(&(m_ExpData), *pExp);
	}
	catch ( _com_error & e ) {
		AfxMessageBox( DecodeComException ( &e ) );
	} 

	EndWaitCursor( );

	GoHome();
}

//-----------------------------------------------------------------------------
CString CRadarDoc::FormatDataForExport(DataObj* pCol)
{
	DataType pFormatter = pCol->GetDataType();
	DataStr* myData = (DataStr*)pCol;
	
	
//	(*((TbGeneric.dll!DataStr*)(pCol))).m_strText

	CString strCell = pCol->ToString();// = (*((DataStr*)(pCol)))->m_strText;
	//pFormatter->FormatDataObj(*this, strCell);

	return strCell;
}

//-----------------------------------------------------------------------------
BOOL CRadarDoc::CanDoExport ()
{
	BOOL bOk = TRUE;
	
	if (m_pCallerDoc)
	{
		bOk = OSL_CAN_DO(m_pCallerDoc->GetInfoOSL(), OSL_GRANT_EXPORT); 
	}
	else if (m_pCallerHotLink )
	{
		bOk = OSL_CAN_DO(m_pCallerHotLink->GetInfoOSL(), OSL_GRANT_EXPORT); 
	}

	return (bOk);
}

//-----------------------------------------------------------------------------
void CRadarDoc::OnUpdateExport (CCmdUI* pCmdUI)
{
	BOOL bOk = CanDoExport ();

	pCmdUI->Enable (bOk);
}

//------------------------------------------------------------------------------
BOOL CRadarDoc::LoadFromVirtualDriver ()
{
	m_strTempRadarFile = GetTempName ();

	CString sContent = AfxGetFileSystemManager()->GetTextFile (m_strRadarFile);

	if (sContent.IsEmpty())
	{
		return TRUE;
	}

	CFileException exc;
	CLineFile aTempFile;
	if (aTempFile.Open (m_strTempRadarFile, CFile::typeText | CFile::modeCreate | CFile::modeWrite, &exc))
	{
		aTempFile.Write(sContent, sContent.GetLength());
		aTempFile.Close ();
	}

	return TRUE;
}

//------------------------------------------------------------------------------
BOOL CRadarDoc::SaveFromVirtualDriver ()
{
	if (m_strTempRadarFile.IsEmpty())
		return TRUE;

	CFileException exc;
	CLineFile aTempFile;
	if (aTempFile.Open (m_strTempRadarFile, CFile::typeText | CFile::modeRead), &exc)
	{
		int nFileLen = aTempFile.GetLength();

		TCHAR* buff = new TCHAR[nFileLen + 1];
		aTempFile.Read (buff, nFileLen);
		for (int i= nFileLen; i >=0; i--)
		{
			if (buff[i] < 50000)
			{
				buff[i+1] = '\0';
				break;
			}
		}
		CString sContent (buff);
		aTempFile.Close ();
		delete buff;
	
		return AfxGetFileSystemManager()->SetTextFile (m_strRadarFile, sContent);
	}

	return FALSE;
}

//------------------------------------------------------------------------------
WebCommandType CRadarDoc::OnGetWebCommandType(UINT commandID)
{
	if (commandID == ID_RADAR_EXCEL || commandID == ID_RADAR_ODS)
		return WEB_UNSUPPORTED;

	return __super::OnGetWebCommandType(commandID);
}
