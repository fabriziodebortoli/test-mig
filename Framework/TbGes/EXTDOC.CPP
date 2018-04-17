
#include "stdafx.h"
//#include "afxpriv.h"

#include <TbNameSolver\PathFinder.h>  
#include <TbNameSolver\TBNamespaces.h>
#include <TbNameSolver\TBWinThread.h>
#include <TbNameSolver\IFileSystemManager.h>

#include <TbClientCore\ClientObjects.h>
#include <TbWebServicesWrappers\LoginManagerInterface.h>

#include <TbGeneric\globals.h>
#include <TbGeneric\DataObj.h>
#include <TbGeneric\StatusBarMessages.h>
#include <TbGeneric\minmax.h>
#include <TbGeneric\ParametersSections.h>
#include <TbGeneric\LocalizableObjs.h>
#include <TbGeneric\WebServiceStateObjects.h>
#include <TbGeneric\JsonFormEngine.h>

#include <TbGenlib\reswalk.h> 
#include <TbGenlib\parsobj.h>
#include <TbGenlib\messages.h>
#include <TbGenlib\generic.h> 
#include <TbGenlib\ExternalControllerInfo.h>
#include <tbgenlib\HyperLink.h>
#include <TbGenlib\commands.hrc>
#include <TbGeneric\TBThemeManager.h>
#include <TbGenlib\AddressEdit.hjson> //JSON AUTOMATIC UPDATE
#include <TbGenlib\TBPropertyGrid.h>

#include <TbFrameworkImages\CommonImages.h>
#include <TbFrameworkImages\GeneralFunctions.h>

#include <TbGenlib\oslinfo.h>
#include <TbGenlib\Const.h>
#include <TbGenlibUI\SettingsTableManager.h>
#include <TbGenlib\oslbaseinterface.h>
#include <TbOledb\oledbmng.h>
#include <TbOledb\sqlrec.h>
#include <TbOledb\sqltable.h>			
#include <TbOledb\sqlcatalog.h>
#include <TbOledb\wclause.h>	
#include <TbOledb\SqlAccessor.h>	

#include <TbGenlibManaged\HelpManager.h>
#include <TbGenLibManaged\Main.h>
#include <TbGenLibManaged\MenuFunctions.h>
#include <TbGenLibManaged\GlobalFunctions.h>

#include <TbWoormEngine\report.h>

#include "TileManager.h"
#include "BEColumnInfo.H"
#include "Aliases.h"
#include "CAddressDlg.hjson" 
#include "browser.h"
#include "bodyedit.h"
#include "dbt.h"
#include "barquery.h"
#include "extdoc.h"
#include "TbRadarInterface.h"
#include "tabber.h"
#include "formmng.h"
#include "eventmng.h"
#include "xsltmng.h"
#include "NumbererService.h"
#include "IAbstractFormDocObject.h"
#include "SlaveViewContainer.h"
#include "TileDialog.h"
#include "HotFilterManager.h"
#include "JsonFormEngineEx.h"
#include "JsonFrame.h"
#include "ReportListDlg.h"
#include "UITileDialog.hjson"
#include "ControlBehaviour.h"
#include "BusinessServiceProvider.h"
#include "DocumentSession.h"
#include "CJsonModelGenerator.h"
//................................. resources
#include "extdoc.hjson" //JSON AUTOMATIC UPDATE
#include "formmng.hjson"
#include "barquery.hjson" //JSON AUTOMATIC UPDATE
#include "bodyedit.hjson" //JSON AUTOMATIC UPDATE

#include "JsonForms\TbGes\IDD_BROWSER.hjson"
#include "JsonForms\TbGes\IDD_EXTDOC_TOOLBAR.hjson"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

#ifdef _DEBUG
#define new DEBUG_NEW
#endif
//-----------------------------------------------------------------------------
#define IDT_UPDATEDATAVIEW	415	// id del timer attivato dall'UpdateDataView
//============================================================================
//	 BROWSE		NEW		EDIT	FIND	
//	===================================
//	{NEW,		---,	---,	---		}	//	F2 (Add new record)
//	{EDIT,		---,	---,	---		}	//	RETURN (Edit current Record)
//	{FIND,		---,	---,	---		}	//	F3 (Find record)
//	{BROWSE,	---,	---,	---		}	//	F4 (Query records)
//	{BROWSE,	---,	---,	---		}	//	F5 (Delete Record)
											//  F6 (Woorm Report)
//	{BROWSE,	---,	---,	---		}	//	F7 (Call Radar on Master)
//	{---,		NEW,	EDIT,	---		}	//	F8 (Inserimento al volo)
//	{---,		BROWSE,	BROWSE,	---		}	//	F10 (Save modified/new record)
//	{---,		NEW,	EDIT,	FIND	}	//	F11 (Radar da HotLink Upper)
//	{---,		NEW,	EDIT,	FIND	}	//	F12 (Radar da HotLink Lower)
//	{EXIT,		BROWSE,	BROWSE,	BROWSE	}	//	ESC (Abbandona stato)


// nome di default della query di ricerca		
//-----------------------------------------------------------------------------
static const TCHAR szDescending[] = _T("DESC");


//mi servono per la gestione delle performance di un documento
//-----------------------------------------------------------------------------
static const TCHAR szMoveFirst[] = _T("MoveFirst");
static const TCHAR szMoveLast[] = _T("MoveLast");
static const TCHAR szMoveNext[] = _T("MoveNext");
static const TCHAR szMovePrev[] = _T("MovePrev");

static const TCHAR szOpenAction[] = _T("Apertura documento");
static const TCHAR szFindAction[] = _T("Ricerca documento");
static const TCHAR szBrowseAction[] = _T("Browse documento");
static const TCHAR szEditAction[] = _T("Edit documento");
static const TCHAR szNewAction[] = _T("Nuovo documento");
static const TCHAR szSaveAction[] = _T("Salvataggio documento");
static const TCHAR szDeleteAction[] = _T("Cancellazione documento");

static const TCHAR szBatchAction[] = _T("Esecuzione Batch");

IMPLEMENT_DYNAMIC(DeprecatedSymField, SymField)

//-----------------------------------------------------------------------------
DataObj* GetField(SqlRecord* pRecord, const CString& sFieldName, bool& isVirtual)
{
	SqlRecordItem* pCol = pRecord->GetItemByColumnName(sFieldName);
	if (!pCol)
		pCol = pRecord->GetItemByColumnName(_T("l_") + sFieldName);//se � un campo local, la bind aggiunge l_
	if (pCol)
	{
		isVirtual = TRUE == pCol->GetColumnInfo()->m_bVirtual;
		return pCol->GetDataObj();
	}
	isVirtual = false;
	return NULL;
}

int GetRefreshDelay()
{
	static int nDelay = *((DataInt*)AfxGetSettingValue(snsTbGenlib, szFormsSection, szRefreshDelay, DataInt(100)));
	return nDelay;
}
/////////////////////////////////////////////////////////////////////////////
// CDMSAttachmentManagerObj
/////////////////////////////////////////////////////////////////////////////
IMPLEMENT_DYNAMIC(CDMSAttachmentManagerObj, CObject)

//==============================================================================
//	CAbstractDoc
//==============================================================================

IMPLEMENT_DYNAMIC(CAbstractDoc, CBaseDocument)

CAbstractDoc::~CAbstractDoc()
{
	if (m_hDocAccel)
	{
		DestroyAcceleratorTable(m_hDocAccel);
		m_hDocAccel = NULL;
	}
	delete m_pAccelDesc;
}

//------------------------------------------------------------------------------
CString CAbstractDoc::GetDocAccelText(WORD id)
{
	CString sDesc = GetAcceleratorText(m_hDocAccel, id);
	//CString sDesc = m_pAccelDesc ? m_pAccelDesc->GetDescription(id) : _T("");
	if (sDesc.IsEmpty())
	{
		CView*	pView = GetFirstView();
		if (pView)
		{
			CAbstractFrame* pFrame = dynamic_cast<CAbstractFrame*>(pView->GetParentFrame());
			sDesc = pFrame ? pFrame->GetDocAccelText(id) : _T("");
		}
	}

	return sDesc;
}
//------------------------------------------------------------------------------
void CAbstractDoc::LoadDocAccel()
{
	// se non e' definita la tabella non carica niente
	if (m_arDocAccelIDRs.IsEmpty() && m_arDocAccelNames.IsEmpty())
		return;
	AutoDeletePtr<CJsonContextObj> pContext;
	m_pAccelDesc = new CAcceleratorDescription;
	for (int i = 0; i < m_arDocAccelNames.GetCount(); i++)
	{
		DWORD dwAcc = REVERSEMAKEINTRESOURCE((LPCTSTR)m_arDocAccelNames[i]);
		pContext = TBLoadAcceleratorContext(dwAcc);
		if (pContext)
			m_pAccelDesc->Append(pContext->m_pDescription->m_pAccelerator);
	}

	for (int i = 0; i < m_arDocAccelIDRs.GetCount(); i++)
	{
		DWORD dwAcc = m_arDocAccelIDRs[i];
		pContext = TBLoadAcceleratorContext(dwAcc);
		if (pContext)
			m_pAccelDesc->Append(pContext->m_pDescription->m_pAccelerator);
	}
	if (pContext)
		((CJsonContext*)(CJsonContextObj*)pContext)->m_pDoc = dynamic_cast<CAbstractFormDoc*>(this);
	int nSize = 0;
	AutoDeletePtr<ACCEL> pAccel = m_pAccelDesc->ToACCEL(pContext, nSize);
	m_hDocAccel = CreateAcceleratorTable(pAccel, nSize);
}

// deve esser reimplementata perche' noi utilizziamo la OpenDocument con
// parametro <> NULL per fare lo stesso una OpenDocument(NULL) cio' OnFileNew
// ma passiamo come parametro il puntatore del documento chiamante per collegare
// tra loro piu' documenti. MFC non controlla il puntatore e usa questa routine
// che setta la path (a noi non interessa). Userebbe il puntatore ad un documento 
// come se fosse CString ??? (sic.)
//------------------------------------------------------------------------------
void CAbstractDoc::SetPathName(LPCTSTR lpszPathName, BOOL/* = TRUE*/)
{ /* do nothing */
}

//--------------------------------------------------------------------------
HINSTANCE CAbstractDoc::GetResourceModule()
{
	if (m_hResourceModule == NULL)
		m_hResourceModule = GetDllInstance(GetRuntimeClass());
	return m_hResourceModule;
}
//------------------------------------------------------------------------------
void CAbstractDoc::SetIconID(UINT nIcon)
{
	// non si sa mai.
	if (nIcon <= 0) return;

	// esistono documenti istanziati al volo ma senza view (vedi contabilita`)
	// in tal caso occorre uscire senza colpo ferire
	POSITION pos = GetFirstViewPosition();
	if (pos <= 0) return;

	CView*	pView = GetNextView(pos);
	ASSERT(pView); if (pView == NULL) return;

	CAbstractFrame* pFrame = dynamic_cast<CAbstractFrame*>(pView->GetParentFrame());
	if (pFrame == NULL)
	{
		ASSERT(FALSE);
		return;
	}

	pFrame->SetIconID(nIcon);

	// valido solo a32Bit :
	// se abilitata mette l'icona settata nel documento dal programmatore
	// ma disgraziatamente tutti i documenti di questo tipo si cuccano
	// la stessa icona nella frame (anche in stato dio iconizzato)
	//	pFrame->SetFrameIcon();  
}

//mi serve per intercettare i messaggi da non inviare ai client doc (anche auxiliary doc)
//-----------------------------------------------------------------------------
BOOL CAbstractDoc::IsBadCmdMsg(UINT nID)
{
	// ID_FILE_CLOSE: devo uscire per forza perche e' stata chiamata la OnCloseDocument che
	// cancella il documento corrente (un bel delete this) e pertanto non
	// posso piu' usare data member perche` sono spariti. (Itri enhancement)

	// ID_EXTDOC_EXIT: siccome alla fine viene "girato" in un ID_FILE_CLOSE, per lo stesso motivo (Rinaldi enhancement)

	// IDC_EXTDOC_BATCH_START_STOP, IDC_EXTDOC_BATCH_PAUSE_RESUME: non devo inviarli se sono nel
	// settaggio dei parametri del documento visto come task dello scheduler. Entrambi mi chiudono
	// il documento
	if (nID == ID_FILE_CLOSE || nID == ID_EXTDOC_EXIT)
		return TRUE;
	if (nID == IDC_EXTDOC_BATCH_START_STOP || nID == IDC_EXTDOC_BATCH_PAUSE_RESUME)
		return IsEditingParamsFromExternalController();
	return FALSE;
}

/////////////////////////////////////////////////////////////////////////////
// Diagnostics

#ifdef _DEBUG
void CAbstractDoc::Dump(CDumpContext& dc) const
{
	ASSERT_VALID(this);
	AFX_DUMP0(dc, " CAbstractDoc\n");
}

void CAbstractDoc::AssertValid() const
{
	CBaseDocument::AssertValid();
}
#endif //_DEBUG



/////////////////////////////////////////////////////////////////////////////
// 						CGenericEventMng
//
/////////////////////////////////////////////////////////////////////////////
// gestisce gli eventi di base a livello di tutti i documenti
// se il documento derivato implementa il proprio gestore di eventi, 
// il presente gestore viene "ereditato" (per contenimento) da quello 
// nuovo del documento derivato
//=============================================================================
class CGenericEventMng : public CEventManager
{
	DECLARE_TB_EVENT_MAP();

	DECLARE_DYNCREATE(CGenericEventMng);
};

//----------------------------------------------------------------------------
BEGIN_TB_EVENT_MAP(CGenericEventMng)
END_TB_EVENT_MAP

IMPLEMENT_DYNCREATE(CGenericEventMng, CEventManager);

//==============================================================================
//	CAbstractFormDoc
//==============================================================================

IMPLEMENT_DYNAMIC(CAbstractFormDoc, CAbstractDoc)

BEGIN_MESSAGE_MAP(CAbstractFormDoc, CAbstractDoc)

	// Status Bar event & Button
	ON_COMMAND(ID_EXTDOC_BACKTOMENU, OnBackMenu)
	ON_COMMAND(ID_EXTDOC_SWITCHTO, OnSwitchTo)
	ON_COMMAND(IDC_PROGBAR_BUTTON, OnProgBarButton)

	ON_COMMAND(ID_EXTDOC_NEW, OnNewRecord)
	ON_COMMAND(ID_EXTDOC_EDIT, OnEditRecord)
	ON_COMMAND(ID_EXTDOC_FIND, OnFindRecord)
	ON_COMMAND(ID_EXTDOC_DELETE, OnDeleteRecord)
	ON_COMMAND(ID_EXTDOC_SAVE, OnSaveRecord)

	ON_COMMAND(ID_ACTIONS_COPY, OnActionsCopy)
	ON_COMMAND(ID_EXTDOC_ESCAPE, OnEscape)

	ON_COMMAND(ID_EXTDOC_QUERY, OnQuery)
	ON_COMMAND(ID_EXTDOC_CUSTOMIZE, OnCustomize)
	ON_COMMAND(ID_EXTDOC_GEN_JSON_MODEL, OnGenerateJsonModel)
	ON_COMMAND(ID_EXTDOC_PROPERTIES, OnProperties)

	ON_COMMAND(ID_EXTDOC_EXEC_QUERY, OnExecQuery)
	ON_COMMAND(ID_EXTDOC_EXEC_POSTQUERY, OnExecPostQuery)
	ON_COMMAND(ID_EXTDOC_EDIT_QUERY, OnEditQuery)

	ON_COMMAND(ID_EXTDOC_FIRST, OnFirstRecord)
	ON_COMMAND(ID_EXTDOC_PREV, OnPrevRecord)
	ON_COMMAND(ID_EXTDOC_NEXT, OnNextRecord)
	ON_COMMAND(ID_EXTDOC_LAST, OnLastRecord)
	ON_COMMAND(ID_EXTDOC_HELP, OnFormHelp)
	ON_COMMAND(ID_EXTDOC_FAVORITES, OnFavorites)

	ON_COMMAND(ID_EXTDOC_CRASH, OnCrash)

	ON_UPDATE_COMMAND_UI(ID_EXTDOC_NEW, OnUpdateNewRecord)
	ON_UPDATE_COMMAND_UI(ID_EXTDOC_EDIT, OnUpdateEditRecord)
	ON_UPDATE_COMMAND_UI(ID_EXTDOC_FIND, OnUpdateFindRecord)
	ON_UPDATE_COMMAND_UI(ID_EXTDOC_DELETE, OnUpdateDeleteRecord)
	ON_UPDATE_COMMAND_UI(ID_EXTDOC_SAVE, OnUpdateSaveRecord)
	ON_UPDATE_COMMAND_UI(ID_EXDOC_TAB_SWITCH, OnUpdateTabSwitch)

	ON_UPDATE_COMMAND_UI(ID_EXTDOC_ESCAPE, OnUpdateEscape)
	ON_UPDATE_COMMAND_UI(ID_EXTDOC_QUERY, OnUpdateQuery)
	ON_UPDATE_COMMAND_UI(ID_EXTDOC_CUSTOMIZE, OnUpdateCustomize)
	ON_UPDATE_COMMAND_UI(ID_EXTDOC_GEN_JSON_MODEL, OnUpdateGenerateJsonModel)

	ON_UPDATE_COMMAND_UI(ID_EXTDOC_FIRST, OnUpdateFirstRecord)
	ON_UPDATE_COMMAND_UI(ID_EXTDOC_PREV, OnUpdatePrevRecord)
	ON_UPDATE_COMMAND_UI(ID_EXTDOC_NEXT, OnUpdateNextRecord)
	ON_UPDATE_COMMAND_UI(ID_EXTDOC_LAST, OnUpdateLastRecord)
	ON_UPDATE_COMMAND_UI(ID_EXTDOC_EXEC_QUERY, OnUpdateExecQuery)
	ON_UPDATE_COMMAND_UI(ID_EXTDOC_EDIT_QUERY, OnUpdateEditQuery)
	ON_UPDATE_COMMAND_UI(ID_EXTDOC_HELP, OnUpdateFormHelp)
	ON_UPDATE_COMMAND_UI(ID_EXTDOC_SWITCHTO, OnUpdateSwitchTo)
	ON_UPDATE_COMMAND_UI(IDC_PROGBAR_BUTTON, OnBarButton)

	ON_UPDATE_COMMAND_UI(ID_EXTDOC_MESSAGE_INDICATORS, OnUpdateMessageIndicator)
	ON_UPDATE_COMMAND_UI(ID_EXTDOC_FORM_MODE_INDICATORS, OnUpdateFormModeIndicator)
	ON_UPDATE_COMMAND_UI(ID_EXTDOC_TOTAL_RECORDS_INDICATORS, OnUpdateTotalRecordsIndicator)
	ON_UPDATE_COMMAND_UI(ID_EXTDOC_DELETED_RECORDS_INDICATORS, OnUpdateDeletedIndicator)
	ON_UPDATE_COMMAND_UI(ID_EXTDOC_ACTIVE_INDICATORS, OnUpdateDisactiveIndicator)
	ON_UPDATE_COMMAND_UI(ID_ACTIONS_COPY, OnUpdateActionsCopy)


	ON_BN_CLICKED(IDC_EXTDOC_BATCH_START_STOP, OnBatchStartStop)
	ON_BN_CLICKED(IDC_EXTDOC_BATCH_PAUSE_RESUME, OnBatchPauseResume)

	ON_COMMAND(ID_EXTDOC_BATCH_STARTSTOP_TOOLBTN, OnBatchStartStop)
	ON_UPDATE_COMMAND_UI(ID_EXTDOC_BATCH_STARTSTOP_TOOLBTN, OnUpdateBatchStartStop)
	ON_COMMAND(ID_EXTDOC_BATCH_PAUSERESUME_TOOLBTN, OnBatchPauseResume)
	ON_UPDATE_COMMAND_UI(ID_EXTDOC_BATCH_PAUSERESUME_TOOLBTN, OnUpdateBatchPauseResume)

	// comandi per la gestione delle query 
	ON_COMMAND_RANGE(ID_EXTDOC_DDTB_ENUMQUERY0, (UINT)(ID_EXTDOC_DDTB_ENUMQUERY0 + ID_EXTDOC_DDTB_ENUMQUERY_MAX), OnExecSelQuery)
	ON_COMMAND(ID_OTHER_QUERY, OnOtherQuery)
	ON_COMMAND(ID_EDIT_QUERY, OnEditQuery)

	// comandi per la gestione dei reports 
	ON_UPDATE_COMMAND_UI(ID_EXTDOC_REPORT, OnUpdateReport)
	ON_COMMAND(ID_EXTDOC_REPORT, OnReport)
	ON_COMMAND(ID_EXTDOC_LIST_REPORTS, OnListReports)
	ON_COMMAND(ID_EXTDOC_FORMREPORT, OnFormReport)
	ON_UPDATE_COMMAND_UI_RANGE(ID_EXTDOC_DDTB_ENUMREPORT0, (UINT)(ID_EXTDOC_DDTB_ENUMREPORT0 + ID_EXTDOC_DDTB_ENUMREPORT_UPPER), OnUpdateSelReport)
	ON_COMMAND_RANGE(ID_EXTDOC_DDTB_ENUMREPORT0, (UINT)(ID_EXTDOC_DDTB_ENUMREPORT0 + ID_EXTDOC_DDTB_ENUMREPORT_UPPER), OnSelReport)

	// comandi per la gestione dei radars
	ON_COMMAND(ID_EXTDOC_RADAR, OnRadar)
	ON_COMMAND(ID_EXTDOC_ALTRADAR, OnAltRadar)
	ON_COMMAND(ID_EXTDOC_NEWNRT, OnNewWrmRadar)
	ON_UPDATE_COMMAND_UI(ID_EXTDOC_RADAR, OnUpdateRadar)
	ON_COMMAND_RANGE(ID_EXTDOC_DDTB_ENUMRADAR0, (UINT)(ID_EXTDOC_DDTB_ENUMRADAR0 + ID_EXTDOC_DDTB_ENUMRADAR_MAX), OnFileWrmRadar)

	//----
	ON_COMMAND(ID_EXDOC_TAB_SWITCH, OnTabSwitch)
	ON_COMMAND_RANGE(ID_EXDOC_TAB_SWITCH_MENU_START, (UINT)(ID_EXDOC_TAB_SWITCH_MENU_END), OnTabSwitchRange)

	ON_COMMAND(ID_EXTDOC_REFRESH_ROWSET, OnRefreshRowset)
	ON_UPDATE_COMMAND_UI(ID_EXTDOC_REFRESH_ROWSET, OnUpdateRefreshRowset)

END_MESSAGE_MAP()

//------------------------------------------------------------------------------
CAbstractFormDoc::CAbstractFormDoc()
	:
	m_pDBTMaster(NULL),
	m_pHotKeyLinks(NULL),
	m_pClientDocs(NULL),
	m_pBSPs(NULL),
	m_pRadarDoc(NULL),
	m_pBrowser(NULL),
	m_nMessagesID(0),
	m_pHotLink(NULL),
	m_pHLRecord(NULL),
	m_bRepeatableNew(FALSE),
	m_bOnlyOneRecord(FALSE),
	m_bBrowseOnCreate(FALSE),
	m_bBrowseOnFirstRecord(FALSE),
	m_bNoFamilyClientDoc(FALSE),
	m_pCurrentRec(NULL),
	m_pFormManager(NULL),
	m_pQueryManager(NULL),
	m_bEnableDocumentale(TRUE),
	m_bEnableReportBarcodeWMS(FALSE),
	m_pXMLDocInfo(NULL),
	m_pBaseExportCriteria(NULL),
	m_pBaseImportCriteria(NULL),
	m_bDisableLoadXMLDocInfo(FALSE),
	m_pEventManager(NULL),
	m_pXSLTFunction(NULL),
	m_pVariablesArray(NULL),
	m_bOnAttachClient(FALSE),
	m_pWoormInfo(NULL),
	m_bCheckExistXMLDocInfo(FALSE),
	m_bExistXMLDocInfo(FALSE),
	m_bJsonDataSent(FALSE),
	m_bSingleUserMode(FALSE),
	m_bLockedDocument(FALSE),
	m_nUpdateDataViewLevel(0),
	m_bUpdatingDataView(false),
	m_bWaitingForUpdateDataView(false),
	m_nDisabled(0),
	m_bWarnWhenEmptyQuery(true),
	m_bFavoritesState(FALSE),
	m_pHeaderTitle(NULL),
	m_bUpdateDefaultReport(TRUE),
	m_pNumbererBinder(NULL),
	m_bClassicRunFromExternalController(TRUE)
{
	AfxGetDefaultSqlConnection()->SetAlwaysConnected(true);
	// Imposta il documento in modo BROWSE
	CBaseDocument::SetFormMode(BROWSE);

	// Di default usa la connessione globale

	//m_pMessages = new CMessages(m_nMessagesID);

	// istanzia il vettore degli hotlinks
	m_pHotKeyLinks = new Array;

	// istanzia il vettore Dei documenti ausiliari degli add-ons
	m_pClientDocs = new CClientDocArray;
	m_pBSPs = new CBSPArray;

	// abilita la getione delle form personalizzabili
	m_pFormManager = new CFormManager(this);

	// gestisce le query del documento (predefinite e create da user)
	m_pQueryManager = new CQueryManager(this, ID_EXTDOC_EXEC_POSTQUERY);

	m_pHeaderTitle = new DataStr();

	// permette di personalizzare il comportamento dopo NEW, cioe' rimanere
	// in NEW o tornare in BROWSE
	DataObj* pRepeatableNew = AfxGetSettingValue(snsTbGenlib, szPreferenceSection, szRepeatableNew, DataBool(m_bRepeatableNew), szTbDefaultSettingFileName);
	m_bRepeatableNew = pRepeatableNew ? *((DataBool*)pRepeatableNew) : FALSE;

	DataObj* pUseEasyBrowsing = AfxGetSettingValue(snsTbGenlib, szPreferenceSection, szUseEasyBrowsing, DataBool(m_bUseEasyBrowsing), szTbDefaultSettingFileName);
	m_bUseEasyBrowsing = pUseEasyBrowsing ? *((DataBool*)pUseEasyBrowsing) : FALSE;

	//permette di abilitare/disabilitare il salvataggio del report di default sulla scelta del report da eseguire nella dropdown del pilsate 'Stampa'
	DataObj* pUpdateDefaultReport = AfxGetSettingValue(snsTbGenlib, szReportSection, szUpdateDefaultReport, DataBool(m_bUpdateDefaultReport), szTbDefaultSettingFileName);
	m_bUpdateDefaultReport = pUpdateDefaultReport ? *((DataBool*)pUpdateDefaultReport) : TRUE;

	/*if (AfxIsRemoteInterface())
	{
		pUseEasyBrowsing = AfxGetSettingValue(snsTbGenlib, szPreferenceSection, szUseEasyBrowsingInEasyLookOnly, DataBool(m_bUseEasyBrowsing), szTbDefaultSettingFileName);
		if (pUseEasyBrowsing ? *((DataBool*)pUseEasyBrowsing) : FALSE)
			m_bUseEasyBrowsing = TRUE;
	}*/

	Attach(new CGenericEventMng());
	AfxGetThreadContext()->AddObject(this);
	SetContextClass(GetRuntimeClass());

}

//------------------------------------------------------------------------------
CAbstractFormDoc::~CAbstractFormDoc()
{
	//ASSERT(m_arReportDoc.GetSize() == 0);

	if (m_pNumbererBinder)
	{
		RemoveConsumer(m_pNumbererBinder);
		delete m_pNumbererBinder;
	}

	ASSERT(m_pDBTMaster == NULL);
	ASSERT(m_pBrowser == NULL);
	ASSERT(m_pClientDocs);
	ASSERT(m_pBSPs);
	ASSERT(m_pFormManager);
	ASSERT(m_pQueryManager);

	// eventuali lock accessori
	UnlockAll();

	if (m_bSingleUserMode)
		m_pTbContext->UnlockDocument();

	//SAFE_DELETE(m_pMessages);
	//li cancello uno a uno ed ogni volta ricontrollo la size: potrebbe infatti succedere che, in presenza di BSP,
	//la distruzione di un client document causi la distruzione di un altro
	while (m_pClientDocs->GetSize())
		m_pClientDocs->RemoveAt(m_pClientDocs->GetUpperBound());
	SAFE_DELETE(m_pClientDocs);
	while (m_pBSPs->GetSize())
		delete m_pBSPs->GetAt(0);//il distruttore si rimuover� dall'array

	SAFE_DELETE(m_pBSPs);
	SAFE_DELETE(m_pHotFilterManager);

	SAFE_DELETE(m_pFormManager);
	SAFE_DELETE(m_pQueryManager);
	SAFE_DELETE(m_pEventManager);
	SAFE_DELETE(m_pXSLTFunction);

	SAFE_DELETE(m_pXMLDocInfo);
	SAFE_DELETE(m_pBaseExportCriteria);
	SAFE_DELETE(m_pBaseImportCriteria);
	SAFE_DELETE(m_pVariablesArray);
	SAFE_DELETE(m_pHeaderTitle);


	if (GetMyAncestor() || GET_CONTEXT(m_pDocInvocationInfo))
	{
		if (m_bUnattendedMode)
			m_pMessages->EndSession();
		SAFE_DELETE(m_pTbContext);
	}
	else
	{// Il contesto deve essere l'utima cosa cancellata prima della diagnostica... onde evitare schiantamenti!
		SAFE_DELETE(m_pTbContext);
		SAFE_DELETE(m_pMessages);
	}

	delete m_pContextBag; //cancello sempre il contextbag visto che � sempre di propriet� del documento (o proprio oppure ereditato ma inizializzato con costruttore in copia)

	AfxGetThreadContext()->RemoveObject(this);
	m_pSqlConnection = NULL;

}

//=============================================================================
BOOL CAbstractFormDoc::CloseReportReady()
{
	for (int i = 0; i < m_arReportDoc.GetSize(); i++)
	{
		WoormDocPtr* pReportDoc = dynamic_cast<WoormDocPtr*>(m_arReportDoc.GetAt(i));
		if (!pReportDoc || !(*pReportDoc))
			continue;

		if (!AfxGetTbCmdManager()->CloseReportReady(*pReportDoc))
			return FALSE;
	}
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CAbstractFormDoc::CloseWoormReport()
{
	BOOL bClosed = TRUE;
	for (int i = 0; i < m_arReportDoc.GetSize(); i++)
	{
		WoormDocPtr* pReportDoc = dynamic_cast<WoormDocPtr*>(m_arReportDoc.GetAt(i));
		if (!pReportDoc || !(*pReportDoc))
			continue;

		if (!AfxGetTbCmdManager()->CloseWoormReport(*pReportDoc))
			bClosed = FALSE;
	}
	return bClosed;
}

//-----------------------------------------------------------------------------
BOOL CAbstractFormDoc::IsReportAlive(BOOL bActivateLast)
{
	for (int i = 0; i < m_arReportDoc.GetSize(); i++)
	{
		WoormDocPtr* pReportDoc = dynamic_cast<WoormDocPtr*>(m_arReportDoc.GetAt(i));
		if (!pReportDoc || !(*pReportDoc))
			continue;

		if (bActivateLast)
			(*pReportDoc)->Activate(NULL, TRUE);

		return TRUE;
	}
	return FALSE;
}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::WaitReportEnd()
{
	for (int i = 0; i < m_arReportDoc.GetSize(); i++)
	{
		WoormDocPtr* pReportDoc = dynamic_cast<WoormDocPtr*>(m_arReportDoc.GetAt(i));
		if (!pReportDoc || !(*pReportDoc))
			continue;

		AfxGetTbCmdManager()->WaitReportEnd(*pReportDoc);
	}
}

//-----------------------------------------------------------------------------
WoormDocPtr CAbstractFormDoc::RunWoormReport(CWoormInfo* pWoormInfo, BOOL bWaitEnd/* = FALSE*/)
{
	WoormDocPtr* pReportDoc = new WoormDocPtr(AfxGetTbCmdManager()->RunWoormReport(pWoormInfo, this, NULL, FALSE, !AfxIsRemoteInterface()));

	m_arReportDoc.Add(pReportDoc);

	if (bWaitEnd)
		AfxGetTbCmdManager()->WaitReportEnd(*pReportDoc);
	else
		AfxGetTbCmdManager()->WaitReportRunning(*pReportDoc);

	return *pReportDoc;
}

//=============================================================================
void CAbstractFormDoc::Disable(bool bDisable)
{
	bDisable ? m_nDisabled++ : m_nDisabled--;
	ASSERT_TRACE(m_nDisabled >= 0, "Disable(false) called more times than Disable(true)!");
	CWnd* pWnd = CWnd::FromHandle(m_hFrameHandle);
	if (pWnd && IsWindow(m_hFrameHandle))
		pWnd->EnableWindow(!m_nDisabled);
}

//=============================================================================
// Serve solo per effettuare la cancellazione automatica degli HotLink istanziati
// ESPLICITAMENTE nel documento
//-----------------------------------------------------------------------------	
void CAbstractFormDoc::Attach(HotKeyLink* pHKL, CString strName /*_T("")*/)
{
	ASSERT(pHKL);
	ASSERT(pHKL->IsKindOf(RUNTIME_CLASS(HotKeyLink)));

	pHKL->AttachDocument(this);
	if (!strName.IsEmpty())
		pHKL->SetName(strName);

	m_pHotKeyLinks->Add(pHKL);
}

//-----------------------------------------------------------------------------	
void CAbstractFormDoc::Attach(CEventManager* pEvMng)
{
	ASSERT(pEvMng);
	ASSERT(pEvMng->IsKindOf(RUNTIME_CLASS(CEventManager)));

	pEvMng->AttachDocument(this);
}

//-----------------------------------------------------------------------------	
void CAbstractFormDoc::Attach(CXSLTFunction* pXSLTFunc)
{
	ASSERT(pXSLTFunc);
	ASSERT(pXSLTFunc->IsKindOf(RUNTIME_CLASS(CXSLTFunction)));
	ASSERT(!pXSLTFunc->GetNamespaceURI().IsEmpty());
	pXSLTFunc->AttachDocument(this);

	SAFE_DELETE(m_pXSLTFunction);
	m_pXSLTFunction = pXSLTFunc;
}

//-----------------------------------------------------------------------------	
void CAbstractFormDoc::ResetJsonData(CJsonParser& json)
{
	if (json.BeginReadObject(_T("modelStructure")))
	{
		CJsonParser parser;
		CString sKey;
		CJsonIterator* pIterator = json.BeginIteration();
		while (pIterator->GetNext(sKey, parser))
		{
			if (sKey == _T("global"))
			{
				for (int j = 0; j < parser.GetCount(); j++)
				{
					CString sVar = parser.ReadString(j);
					DataObj* pDataObj = GetVariableValue(sVar);
					if (pDataObj)
					{
						pDataObj->SetWebBound();
					}
				}
			}
			else
			{
				SqlRecord* pRecord = NULL;
				DBTObject* pDBT = GetDBTByName(sKey);
				if (pDBT)
				{
					pRecord = pDBT->GetRecord();
				}
				else
				{
					HotKeyLink* pHKL = GetHotLink(sKey);
					if (pHKL)
						pRecord = pHKL->GetAttachedRecord();
				}
				if (pRecord)
					for (int j = 0; j < parser.GetCount(); j++)
					{
						CString sVar = parser.ReadString(j);
						DataObj* pDataObj = pRecord->GetDataObjFromColumnName(sVar);
						if (pDataObj)
						{
							pDataObj->SetWebBound();
						}
					}
			}
		}
	}

	m_bJsonDataSent = FALSE;
	for (int i = 0; i < m_arRegisteredDBTs.GetCount(); i++)
	{
		DBTObject* pDBT = m_arRegisteredDBTs.GetAt(i);
		if (pDBT->IsKindOf(RUNTIME_CLASS(DBTSlaveBuffered)))
			((DBTSlaveBuffered*)pDBT)->ResetJsonData();
	}
}
//-----------------------------------------------------------------------------	
void CAbstractFormDoc::AttachXMLExpCriteria(CXMLBaseAppCriteria* pBaseExportCriteria)
{
	ASSERT(pBaseExportCriteria);
	ASSERT(pBaseExportCriteria->IsKindOf(RUNTIME_CLASS(CXMLBaseAppCriteria)));

	m_pBaseExportCriteria = pBaseExportCriteria;
	m_pBaseExportCriteria->AttachDocument(this);
	m_pBaseExportCriteria->Customize();
}

//-----------------------------------------------------------------------------	
void CAbstractFormDoc::AttachXMLImpCriteria(CXMLBaseAppCriteria* pBaseImportCriteria)
{
	ASSERT(pBaseImportCriteria);
	ASSERT(pBaseImportCriteria->IsKindOf(RUNTIME_CLASS(CXMLBaseAppCriteria)));

	m_pBaseImportCriteria = pBaseImportCriteria;
	m_pBaseImportCriteria->AttachDocument(this);
	m_pBaseImportCriteria->Customize();
}

// Se viene chiamato Attraverso la CallLink da parte di un HotLink deve copiare
// il DataObj il piu' rapidamente possibile in quanto il Documento da cui e' stato
// lanciato potrebbe anche essere chiuso dall'utente
//-----------------------------------------------------------------------------
void CAbstractFormDoc::ConnectForm(HotKeyLink* pHotLink, SqlRecord* pHLRecord)
{
	ASSERT(pHotLink);
	ASSERT(pHLRecord);

	m_pHotLink = pHotLink;
	m_pHLRecord = pHLRecord;

	// si mette in condizioni di new se lo puo` fare
	if (NewRecord())
		SetModifiedFlag(TRUE);
}

// Aggancia una eventuale form esterna descritta in un file .tbf
//-----------------------------------------------------------------------------
void CAbstractFormDoc::SetFormName(const CString& strFormName)
{
	ASSERT(m_pFormManager);

	CString sFileName = GetName(strFormName);
	m_strFormName = sFileName + _T(".") + FileExtension::TBF_EXT();
}

//-----------------------------------------------------------------------------
const CString CAbstractFormDoc::GetCurrentQueryName()
{
	return m_pQueryManager->GetCurrentQueryName();
}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::SetCurrentQueryName(const CString& strQueryName)
{
	m_pQueryManager->SelectQuery(strQueryName);
	m_pQueryManager->SetQueryName();
}

// Chiamata dall'HotLink se esso muore prima che il documento gli ritorni il 
// record
//-----------------------------------------------------------------------------
void CAbstractFormDoc::OnHotLinkDied(HotKeyLink* pHotLink)
{
	ASSERT(m_pHotLink == pHotLink);
	ASSERT(m_pHotLink);

	if (m_pHotLink == pHotLink)
	{
		m_pHLRecord = NULL;
		m_pHotLink = NULL;
	}
}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::DisableAllView()
{
	POSITION pos = GetFirstViewPosition();
	while (pos != NULL)
	{
		CView* pView = GetNextView(pos);
		ASSERT_VALID(pView);
		if (pView != NULL && pView->IsKindOf(RUNTIME_CLASS(CAbstractFormView)))
			pView->GetParentFrame()->EnableWindow(FALSE);
	}
}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::EnableAllView()
{
	POSITION pos = GetFirstViewPosition();
	while (pos != NULL)
	{
		CView* pView = GetNextView(pos);
		ASSERT_VALID(pView);
		if (pView != NULL && pView->IsKindOf(RUNTIME_CLASS(CAbstractFormView)))
			pView->GetParentFrame()->EnableWindow(TRUE);
	}
}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::OnHotLinkRun()
{
	DisableAllView();
}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::OnHotLinkStop()
{
	EnableAllView();
}

// Se sono chiamato	dalla Call Link devo restituire i dati e poi chiudere
//-----------------------------------------------------------------------------
BOOL CAbstractFormDoc::ReturnDataToCallLink(BOOL bClose/* = TRUE*/, BOOL bCopyRecord/* = TRUE*/)
{
	if (m_pHotLink && m_pHLRecord)
	{
		if (m_pDBTMaster && bCopyRecord)
		{
			AssignMasterRecordToHkl();
			m_pHotLink->OnFormRecordAvailable();
		}

		// libera solamente la connessione riabilitando la chiamata CallLink
		m_pHotLink->OnFormDied();

		// pulische le variabile che hanno anche funzione di semaforo (flags)
		m_pHLRecord = NULL;
		m_pHotLink = NULL;

		// ammazza il documento dopo un inserimento al volo, sia che sia
		// andato a buon fine (salvato) che no
		if (bClose)
			CloseDocument();

		return TRUE;
	}
	return FALSE;
}

//-----------------------------------------------------------------------------
///<summary>
/// The function activate the Tab
///</summary>
//[TBWebMethod(name=CAbstractFormDoc_ActivateTab, thiscall_method=true, securityhidden=true)]
DataBool CAbstractFormDoc::TabDialogActivate(DataStr NsTab)
{
	POSITION pos = GetFirstViewPosition();
	while (pos != NULL)
	{
		CAbstractFormView* pView = dynamic_cast<CAbstractFormView*>(GetNextView(pos));
		ASSERT_VALID(pView);
		if (pView)
		{
			pView->PrepareTabDialogNamespaces();
			BOOL bRet = pView->TabDialogActivate(NsTab);
			if (bRet)
				return DataBool(TRUE);
		}
	}
	return DataBool(FALSE);
}

//-----------------------------------------------------------------------------
///<summary>
/// The function execs toolbar button's action
///</summary>
//[TBWebMethod(thiscall_method=true, securityhidden=true)]
DataBool CAbstractFormDoc::PressToolbarButton(DataStr buttonName)
{
	CMasterFrame* pMF = CAbstractFormDoc::GetMasterFrame();
	if (!pMF)
		return DataBool(FALSE);

	int nID = 0;

	CTBTabbedToolbar* pTabbedToolBar = pMF->GetTabbedToolBar();
	if (pTabbedToolBar)
	{
		nID = pTabbedToolBar->FindButton(buttonName.GetString());
	}

	if (nID >= 0)
	{
		pMF->PostMessage(WM_COMMAND, nID);
		return DataBool(TRUE);
	}
	return DataBool(FALSE);
}

//-----------------------------------------------------------------------------
BOOL CAbstractFormDoc::TabDialogActivate(UINT nTabIDC, UINT nIDD)
{
	POSITION pos = GetFirstViewPosition();
	while (pos != NULL)
	{
		CAbstractFormView* pView = dynamic_cast<CAbstractFormView*>(GetNextView(pos));
		ASSERT_VALID(pView);
		if (pView)
		{
			int nRet = pView->TabDialogActivate(nTabIDC, nIDD);
			if (nRet)
				return nRet > 0;
		}
	}
	return FALSE;
}

//-----------------------------------------------------------------------------
BOOL CAbstractFormDoc::TabDialogShow(UINT nTabIDC, UINT nIDD, BOOL bShow)
{
	POSITION pos = GetFirstViewPosition();
	while (pos != NULL)
	{
		CAbstractFormView* pView = dynamic_cast<CAbstractFormView*>(GetNextView(pos));
		ASSERT_VALID(pView);
		if (pView != NULL)
		{
			int nRet = pView->TabDialogShow(nTabIDC, nIDD, bShow);
			if (nRet)
				return nRet > 0;
		}
	}
	return FALSE;
}

//-----------------------------------------------------------------------------
BOOL CAbstractFormDoc::TabDialogEnable(UINT nTabIDC, UINT nIDD, BOOL bEnable /* = TRUE */)
{
	POSITION pos = GetFirstViewPosition();
	while (pos != NULL)
	{
		CAbstractFormView* pView = dynamic_cast<CAbstractFormView*>(GetNextView(pos));
		ASSERT_VALID(pView);
		if (pView)
		{
			int nRet = pView->TabDialogEnable(nTabIDC, nIDD, bEnable);
			if (nRet)
				return nRet > 0;
		}
	}
	return FALSE;
}

//-----------------------------------------------------------------------------
BOOL CAbstractFormDoc::TileDialogEnable(UINT nIDDTileGroup, UINT nIDD, BOOL bEnable)
{
	POSITION pos = GetFirstViewPosition();
	while (pos != NULL)
	{
		CAbstractFormView* pView = dynamic_cast<CAbstractFormView*>(GetNextView(pos));
		ASSERT_VALID(pView);
		if (pView)
		{
			int nRet = pView->TileDialogEnable(nIDDTileGroup, nIDD, bEnable);
			if (nRet)
				return nRet > 0;
		}
	}
	return FALSE;
}

//-----------------------------------------------------------------------------
BOOL CAbstractFormDoc::TilePanelEnable(UINT nIDDTileGroup, UINT nIDD, BOOL bEnable)
{
	POSITION pos = GetFirstViewPosition();
	while (pos != NULL)
	{
		CAbstractFormView* pView = dynamic_cast<CAbstractFormView*>(GetNextView(pos));
		ASSERT_VALID(pView);
		if (pView)
		{
			int nRet = pView->TilePanelEnable(nIDDTileGroup, nIDD, bEnable);
			if (nRet)
				return nRet > 0;
		}
	}
	return FALSE;
}

//-----------------------------------------------------------------------------
BOOL CAbstractFormDoc::TileGroupActivate(UINT nIDDTileManager, UINT nIDDTileGroup)
{
	POSITION pos = GetFirstViewPosition();
	while (pos != NULL)
	{
		CAbstractFormView* pView = dynamic_cast<CAbstractFormView*>(GetNextView(pos));
		ASSERT_VALID(pView);

		if (pView)
		{
			int nRet = pView->TileGroupActivate(nIDDTileManager, nIDDTileGroup);
			if (nRet)
				return nRet > 0;
		}
	}
	return FALSE;

}

//-----------------------------------------------------------------------------
CMasterFrame* CAbstractFormDoc::GetMasterFrame() const
{
	return dynamic_cast<CMasterFrame*>(GetFrame());
}

//-----------------------------------------------------------------------------
BOOL CAbstractFormDoc::HideButton(UINT nID, BOOL bHide /*= TRUE*/)
{
	CTBTabbedToolbar* pTabbedToolbar = GetMasterFrame()->GetTabbedToolBar();
	if (pTabbedToolbar)
	{
		return pTabbedToolbar->HideButton(nID, bHide);
	}
	return FALSE;
}

//-----------------------------------------------------------------------------
BOOL CAbstractFormDoc::DeleteButton(UINT nID)
{
	CTBTabbedToolbar* pTabbedToolbar = GetMasterFrame()->GetTabbedToolBar();
	if (pTabbedToolbar)
	{
		return pTabbedToolbar->RemoveButton(nID);
	}
	return FALSE;
}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::DoPinUnpin(UINT nDialogId, bool isPinned)
{
	OnPinUnpin(nDialogId, isPinned);
	m_pClientDocs->OnPinUnpin(nDialogId, isPinned);
	//if (GetHotFilterManager())
	//	GetHotFilterManager()->OnPinUnpin(pTileDialog);
}
//-----------------------------------------------------------------------------
void CAbstractFormDoc::DoPinUnpin(CBaseTileDialog* pTileDialog)
{
	OnPinUnpin(pTileDialog);
	m_pClientDocs->OnPinUnpin(pTileDialog);
	if (GetHotFilterManager())
		GetHotFilterManager()->OnPinUnpin(pTileDialog);
}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::OnPinUnpin(CBaseTileDialog* pTileDialog)
{
	OnPinUnpin(pTileDialog->GetDialogID(), TRUE == pTileDialog->IsPinned());
}

//-----------------------------------------------------------------------------
DBTObject* CAbstractFormDoc::GetDBTObject(const CRuntimeClass* pDBTClass) const
{
	if (!m_pDBTMaster) return NULL;

	if (m_pDBTMaster->IsKindOf(pDBTClass))
		return m_pDBTMaster;

	for (int i = 0; i <= m_pDBTMaster->m_pDBTSlaves->GetUpperBound(); i++)
	{
		if (
			m_pDBTMaster->m_pDBTSlaves->GetAt(i)->IsKindOf(pDBTClass))
			return m_pDBTMaster->m_pDBTSlaves->GetAt(i);
	}

	return GetRegisteredDBT(pDBTClass);
}

//-----------------------------------------------------------------------------
DBTObject* CAbstractFormDoc::GetDBTObject(const CString& sTableName) const
{
	if (!m_pDBTMaster) return NULL;

	if (m_pDBTMaster->GetRecord()->GetTableName().CompareNoCase(sTableName) == 0)
		return m_pDBTMaster;

	DBTObject* pDBTObject = NULL;
	for (int i = 0; i <= m_pDBTMaster->m_pDBTSlaves->GetUpperBound(); i++)
	{
		pDBTObject = m_pDBTMaster->m_pDBTSlaves->GetAt(i);

		if (pDBTObject->GetRecord()->GetTableName().CompareNoCase(sTableName) == 0)
			return pDBTObject;
	}

	return GetRegisteredDBT(sTableName);
}

//-----------------------------------------------------------------------------
DBTObject* CAbstractFormDoc::GetDBTByName(const CString& sDbtName) const
{
	if (m_pDBTMaster)
	{

		if (m_pDBTMaster->GetNamespace().GetObjectName().CompareNoCase(sDbtName) == 0)
			return m_pDBTMaster;

		DBTObject* pDBTObject = NULL;
		for (int i = 0; i <= m_pDBTMaster->m_pDBTSlaves->GetUpperBound(); i++)
		{
			pDBTObject = m_pDBTMaster->m_pDBTSlaves->GetAt(i);

			if (pDBTObject->GetNamespace().GetObjectName().CompareNoCase(sDbtName) == 0)
				return pDBTObject;
		}
	}
	return GetRegisteredDBTByName(sDbtName);
}


//-----------------------------------------------------------------------------
CDataSourceAlias* CAbstractFormDoc::RegisterDataSourceAlias(CString sAlias, const CString& sActual)
{
	if (sAlias.IsEmpty() || sActual.IsEmpty())
	{
		ASSERT(FALSE);
		return NULL;
	}
	if (sAlias[0] != ALIAS_IDENTIFIER)
		sAlias.Insert(0, ALIAS_IDENTIFIER);
	CDataSourceAlias* pAlias = new CDataSourceAlias(sAlias, sActual);
	m_arDataSourceAliases.Add(pAlias);
	return pAlias;
}
//-----------------------------------------------------------------------------
CFieldAlias* CAbstractFormDoc::RegisterFieldAlias(CString sAlias, const CString& sActual)
{
	if (sAlias.IsEmpty() || sActual.IsEmpty())
	{
		ASSERT(FALSE);
		return NULL;
	}
	if (sAlias[0] != ALIAS_IDENTIFIER)
		sAlias.Insert(0, ALIAS_IDENTIFIER);
	CFieldAlias* pAlias = new CFieldAlias(sAlias, sActual);
	m_arFieldAliases.Add(pAlias);
	return pAlias;
}
//-----------------------------------------------------------------------------
CGenericAlias* CAbstractFormDoc::RegisterGenericAlias(CString sAlias, const CString& sActual)
{
	if (sAlias.IsEmpty() || sActual.IsEmpty())
	{
		ASSERT(FALSE);
		return NULL;
	}
	if (sAlias[0] != ALIAS_IDENTIFIER)
		sAlias.Insert(0, ALIAS_IDENTIFIER);

	for (int i = 0; i < m_arGenericAliases.GetCount(); i++)
	{
		CGenericAlias* pAlias = (CGenericAlias*)m_arGenericAliases.GetAt(i);
		if (pAlias->m_sAlias == sAlias)
		{
			pAlias->m_sActual = sActual;
			return pAlias;
		}
	}
	CGenericAlias* pAlias = new CGenericAlias(sAlias, sActual);
	m_arGenericAliases.Add(pAlias);
	return pAlias;
}
//-----------------------------------------------------------------------------
BOOL CAbstractFormDoc::TranslateAlias(CString& sAlias)
{
	if (sAlias.IsEmpty() || sAlias[0] != ALIAS_IDENTIFIER)
		return FALSE;
	//sDataSource.TrimLeft(ALIAS_IDENTIFIER);
	for (int i = 0; i < m_arGenericAliases.GetCount(); i++)
	{
		CGenericAlias* pAlias = (CGenericAlias*)m_arGenericAliases.GetAt(i);
		if (pAlias->m_sAlias == sAlias)
		{
			sAlias = pAlias->m_sActual;
			return TRUE;
		}
	}
	return FALSE;
}


//-----------------------------------------------------------------------------
BOOL CAbstractFormDoc::TranslateDataSourceAlias(CString& sDataSource)
{
	if (sDataSource.IsEmpty() || sDataSource[0] != ALIAS_IDENTIFIER)
		return FALSE;
	//sDataSource.TrimLeft(ALIAS_IDENTIFIER);
	for (int i = 0; i < m_arDataSourceAliases.GetCount(); i++)
	{
		CDataSourceAlias* pAlias = (CDataSourceAlias*)m_arDataSourceAliases.GetAt(i);
		if (pAlias->m_sAlias == sDataSource)
		{
			sDataSource = pAlias->m_sActual;
			return TRUE;
		}
	}
	return FALSE;
}
//-----------------------------------------------------------------------------
BOOL CAbstractFormDoc::TranslateFieldAlias(const CString& sActualDataSource, CString& sField)
{
	if (sField.IsEmpty() || sField[0] != ALIAS_IDENTIFIER)
		return FALSE;

	//sField.TrimLeft(ALIAS_IDENTIFIER);
	if (sActualDataSource.IsEmpty())
	{
		for (int i = 0; i < m_arFieldAliases.GetCount(); i++)
		{
			CFieldAlias* pAlias = (CFieldAlias*)m_arFieldAliases.GetAt(i);
			if (pAlias->m_sAlias == sField)
			{
				sField = pAlias->m_sActual;
				return TRUE;
			}
		}
	}
	else
	{
		for (int i = 0; i < m_arDataSourceAliases.GetCount(); i++)
		{
			CDataSourceAlias* pAlias = (CDataSourceAlias*)m_arDataSourceAliases.GetAt(i);
			if (pAlias->m_sActual == sActualDataSource)
			{
				for (int j = 0; j < pAlias->m_arFields.GetCount(); j++)
				{
					CFieldAlias* pFieldAlias = (CFieldAlias*)pAlias->m_arFields.GetAt(j);
					if (pFieldAlias->m_sAlias == sField)
					{
						sField = pFieldAlias->m_sActual;
						return TRUE;
					}
				}
			}
		}
	}

	return FALSE;
}
//-----------------------------------------------------------------------------
void CAbstractFormDoc::GetDataSource(CString sDataManagerName, CString sFieldName, DBTObject*& pDbt, SqlRecord*& pRecord, DataObj*& pField, bool& isVirtual)
{
	pRecord = NULL;
	pDbt = NULL;
	if (sDataManagerName.IsEmpty())
	{
		//traduco l'eventuale alias nel nome effettivo; il primo argomento � nullo, si tratta di un alias di campo di documento
		TranslateFieldAlias(sDataManagerName, sFieldName);
		pField = GetVariableValue(sFieldName);
		return;
	}
	TranslateDataSourceAlias(sDataManagerName);
	TranslateFieldAlias(sDataManagerName, sFieldName);

	pDbt = GetDBTByName(sDataManagerName);
	if (pDbt)
	{
		pRecord = pDbt->GetRecord();
		pField = GetField(pRecord, sFieldName, isVirtual);
		return;
	}
	HotKeyLink* pHotLink = GetHotLink(sDataManagerName);
	if (pHotLink)
	{
		pRecord = pHotLink->GetAttachedRecord();
		pField = GetField(pRecord, sFieldName, isVirtual);
		return;
	}
	DataSourceGetterFunction pGetter = AfxGetLoginContext()->GetDataSourceGetter(sDataManagerName);
	if (pGetter)
	{
		pRecord = (SqlRecord*)pGetter();
		pField = GetField(pRecord, sFieldName, isVirtual);
		return;
	}
}

//-----------------------------------------------------------------------------
DBTObject* CAbstractFormDoc::GetDBTObject(const CTBNamespace&	aNs) const
{
	if (!m_pDBTMaster) return NULL;

	if (m_pDBTMaster->GetNamespace() == aNs)
		return m_pDBTMaster;

	DBTObject* pDBTObject = NULL;
	for (int i = 0; i <= m_pDBTMaster->m_pDBTSlaves->GetUpperBound(); i++)
	{
		pDBTObject = m_pDBTMaster->m_pDBTSlaves->GetAt(i);

		if (pDBTObject->GetNamespace() == aNs)
			return pDBTObject;
	}

	return GetRegisteredDBT(aNs);
}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::RegisterDBT(DBTObject* pDBT)
{
	DBTObject* pExistingDBT;
	for (int i = 0; i <= m_arRegisteredDBTs.GetUpperBound(); i++)
	{
		pExistingDBT = (DBTObject*)m_arRegisteredDBTs.GetAt(i);
		if (pExistingDBT)
		{
			if (pExistingDBT == pDBT)
			{
				ASSERT(FALSE);
				TRACE1("CAbstractFormDoc::RegisterDBT %s duplicate registration!", pExistingDBT->GetNamespace().ToUnparsedString());
				return;
			}

			if (pExistingDBT->GetNamespace() == pDBT->GetNamespace())
			{
//				ASSERT(FALSE);
				TRACE1("CAbstractFormDoc::RegisterDBT %s duplicate dbt namespace!", pExistingDBT->GetNamespace().ToUnparsedString());
				return;
			}
		}
	}

	m_arRegisteredDBTs.Add(pDBT);
}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::DeregisterDBT(DBTObject* pDBT)
{
	for (int i = m_arRegisteredDBTs.GetUpperBound(); i >= 0; i--)
	{
		if (((DBTObject*)m_arRegisteredDBTs.GetAt(i)) == pDBT)
		{
			m_arRegisteredDBTs.RemoveAt(i);
			break;
		}
	}
}

//-----------------------------------------------------------------------------
DBTObject* CAbstractFormDoc::GetRegisteredDBT(const CTBNamespace& aNs) const
{
	DBTObject* pDBT;
	for (int i = 0; i <= m_arRegisteredDBTs.GetUpperBound(); i++)
	{
		pDBT = (DBTObject*)m_arRegisteredDBTs.GetAt(i);
		if (pDBT && pDBT->GetNamespace() == aNs)
			return pDBT;
	}

	return NULL;
}

//-----------------------------------------------------------------------------
DBTObject* CAbstractFormDoc::GetRegisteredDBT(const CString& sTableName) const
{
	DBTObject* pDBT;
	for (int i = 0; i <= m_arRegisteredDBTs.GetUpperBound(); i++)
	{
		pDBT = (DBTObject*)m_arRegisteredDBTs.GetAt(i);
		if (pDBT && pDBT->GetRecord()->GetTableName().CompareNoCase(sTableName) == 0)
			return pDBT;
	}

	return NULL;
}
//-----------------------------------------------------------------------------
DBTObject* CAbstractFormDoc::GetRegisteredDBTByName(const CString& sDbtName) const
{
	DBTObject* pDBT;
	for (int i = 0; i <= m_arRegisteredDBTs.GetUpperBound(); i++)
	{
		pDBT = (DBTObject*)m_arRegisteredDBTs.GetAt(i);
		if (pDBT && pDBT->GetNamespace().GetObjectName().CompareNoCase(sDbtName) == 0)
			return pDBT;
	}

	return NULL;
}
//-----------------------------------------------------------------------------
DBTObject* CAbstractFormDoc::GetRegisteredDBT(const CRuntimeClass* pDBTClass) const
{
	DBTObject* pDBT;
	for (int i = 0; i <= m_arRegisteredDBTs.GetUpperBound(); i++)
	{
		pDBT = (DBTObject*)m_arRegisteredDBTs.GetAt(i);
		if (pDBT && pDBT->IsKindOf(pDBTClass))
			return pDBT;
	}

	return NULL;
}

//-----------------------------------------------------------------------------
DBTObject* CAbstractFormDoc::GetDBTObject(SqlRecord* pRec) const
{
	if (!m_pDBTMaster) return NULL;

	return m_pDBTMaster->GetDBTObject(pRec);
}

//-----------------------------------------------------------------------------
SqlRecord*	CAbstractFormDoc::GetCurrentRecord(const CRuntimeClass* pSqlRecordClass) const
{
	if (!m_pDBTMaster) return NULL;

	if (pSqlRecordClass == NULL)
		return m_pDBTMaster->GetRecord();


	if (
		m_pDBTMaster->GetRecord() &&
		m_pDBTMaster->GetRecord()->IsKindOf(pSqlRecordClass)
		)
		return m_pDBTMaster->GetRecord();


	for (int i = 0; i <= m_pDBTMaster->m_pDBTSlaves->GetUpperBound(); i++)
	{
		if (
			m_pDBTMaster->m_pDBTSlaves->GetAt(i) &&
			m_pDBTMaster->m_pDBTSlaves->GetAt(i)->GetRecord() &&
			m_pDBTMaster->m_pDBTSlaves->GetAt(i)->GetRecord()->IsKindOf(pSqlRecordClass)
			)
			return m_pDBTMaster->m_pDBTSlaves->GetAt(i)->GetRecord();
	}

	return NULL;
}


// ritorna in un array l'elenco dei namespaces dei client docs collegati
//-----------------------------------------------------------------------------
CTBNamespaceArray* CAbstractFormDoc::GetClientDocNamespaces() const
{
	if (m_pClientDocs->GetSize() == 0)
		return NULL;

	CTBNamespaceArray* pNamespaces = new CTBNamespaceArray();
	for (int i = 0; i <= m_pClientDocs->GetUpperBound(); i++)
		pNamespaces->Add(new CTBNamespace(m_pClientDocs->GetAt(i)->m_Namespace));

	return pNamespaces;
}

//-----------------------------------------------------------------------------
CClientDoc* CAbstractFormDoc::GetClientDoc(const CRuntimeClass* pClientDocClass) const
{
	for (int i = 0; i <= m_pClientDocs->GetUpperBound(); i++)
	{
		if (m_pClientDocs->GetAt(i)->IsKindOf(pClientDocClass))
			return m_pClientDocs->GetAt(i);
	}

	return NULL;
}

//-----------------------------------------------------------------------------
CClientDoc* CAbstractFormDoc::GetClientDoc(const CString& pNamespace) const
{
	for (int i = 0; i <= m_pClientDocs->GetUpperBound(); i++)
	{
		CClientDoc* pClient = m_pClientDocs->GetAt(i);
		if (pClient->NamespaceEquals(pNamespace))
			return pClient;
	}

	return NULL;
}

//-----------------------------------------------------------------------------
CBusinessServiceProviderObj* CAbstractFormDoc::GetBSP(const CString& pNamespace) const
{
	for (int i = 0; i <= m_pBSPs->GetUpperBound(); i++)
	{
		CBusinessServiceProviderObj* pBSP = m_pBSPs->GetAt(i);
		if (pBSP->GetstrNamespace().CompareNoCase(pNamespace) == 0)
			return pBSP;
	}

	return NULL;
}

//-----------------------------------------------------------------------------
SqlAddOnFieldsColumn* CAbstractFormDoc::GetAddOnFields(const CRuntimeClass* pSqlClass, const CRuntimeClass* pAddOnFieldsClass) const
{
	SqlRecord* pRec = GetCurrentRecord(pSqlClass);

	if (pRec)
		return pRec->GetAddOnFields(pAddOnFieldsClass);

	return NULL;
}


//-----------------------------------------------------------------------------
void CAbstractFormDoc::AddClientDocTabDlg(CTabManager* pTabMng)
{
	m_pClientDocs->CustomizeTabber(pTabMng);
}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::AddClientDocTileDialog(CTileGroup* pTileGroup)
{
	m_pClientDocs->CustomizeTileGroup(pTileGroup);
	pTileGroup->OnAfterCustomize();
}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::CallCustomizeBodyEdit(CBodyEdit* pBodyEdit)
{
	CustomizeBodyEdit(pBodyEdit);

	POSITION pos = GetFirstViewPosition();
	while (pos != NULL)
	{
		CView* pView = GetNextView(pos);
		ASSERT_VALID(pView);

		if (pView->IsKindOf(RUNTIME_CLASS(CAbstractFormView)))
			((CAbstractFormView*)pView)->CustomizeBodyEdit(pBodyEdit);
	}
	// permette l'aggancio di ulteriori colonne dai ClientDoc (appartenente allo stesso SQLRecord
	//del DBTSlaveBuffered agganciato->vedi colonne aggiunte)
	m_pClientDocs->CustomizeBodyEdit(pBodyEdit);
}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::CallCustomizeGridControl(CTBGridControl* pGridControl)
{
	m_pClientDocs->CustomizeGridControl(pGridControl);
}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::OnRadarDied(ITBRadar* pRadarDoc)
{
	ASSERT(pRadarDoc == m_pRadarDoc);
	ASSERT(m_pRadarDoc);

	if (pRadarDoc == m_pRadarDoc)
		m_pRadarDoc = NULL;

	if (pRadarDoc && pRadarDoc->GetDocument()->IsKindOf(RUNTIME_CLASS(CWoormDoc)))
		pRadarDoc->CloseRadar();
}

// Il documento tipo ricercatore (Finder) non puo` andare in new
//-----------------------------------------------------------------------------
BOOL CAbstractFormDoc::OnRadarRecordNew()
{
	// abortisco lo stato di NEW o EDIT (stato corrente) se possibile.
	// In quest'ultimo caso rigetto la richiesta di passaggio
	if (GetType() == VMT_FINDER || !Escape())
		return FALSE;

	Activate();

	// give to final implementation a chances to modify specific
	// auxiliary implementation dependent data
	return NewRecord();
}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::OnRadarRecordSelected(BOOL bMustActive /* = FALSE*/, BOOL bForceEditing /* = FALSE*/)
{
	SUSPEND_UPDATE_DATA_VIEW();
	// si salva il dato passato dal radar per poter eventualmente attivare
	// la gestione dell'accesso all'editing
	SaveCurrentRecord();

	// abortisce il corrente stato di new, edit o find (il controllo di editing
	// in corso e' controllato dal radar per cui qui e' pleonastico)
	if (GetFormMode() != BROWSE)
	{
		// informa tutte le view che il processo di edit e' abortito
		// e devono essere ignorati eventuali errori di control
		AbortAllViews();

		// non viene chiamata la GoInBrowseMode perche` come ultima azione
		// effettua la SetDefaultFocus() che in questa situazione rompe le p...
		// al radar
		SetFormMode(BROWSE);	// back to BROWSE mode
		SetModifiedFlag(FALSE);// back to unmodified
		UpdateDataView();
	}

	// Se il radar cede il controllo al documento prima devo attivarlo e 
	// poi entrare in modifica. Se sono un finder (ricercatore). non e` ammesso
	// l'editing e devo solo lanciare una bella browsata
	if (bMustActive)
	{
		Activate();
		// while importing must always edit, no matter of easybrowsing set
		if (GetType() != VMT_FINDER &&
			(
				bForceEditing ||
				(!m_bUseEasyBrowsing || IsImporting())
				)
			)
		{
			if (EditRecord())
				return;
		}
	}

	// Non testa se si puo` fare l'editing perche` l'ha gia` fatto il radar
	// ed inoltre inibisce la cessione del fuoco se il radar non lo permette
	BrowseRecord(bMustActive);
}

// Gestiscono la presentazione di default in Radar ed in WrmRadar
//-----------------------------------------------------------------------------
void CAbstractFormDoc::ClearVisible()
{
	m_pDBTMaster->m_pRecord->ClearVisible();
}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::SetVisible(const DataObj* pDataObj, BOOL bVisible)
{
	m_pDBTMaster->m_pRecord->SetVisible(pDataObj, bVisible);
}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::SetFocusOnField(DataObj* pField)
{
	BOOL bFound = FALSE;
	CAbstractFormView* pView = dynamic_cast<CAbstractFormView*>(GetFirstView());
	if (!pView)
		return;

	ControlLinks* pLinks = pView->m_pControlLinks;
	for (int i = 0; i < pLinks->GetSize(); i++)
	{
		CWnd* pWnd = pLinks->GetAt(i);
		ASSERT(pWnd);

		CParsedCtrl* pControl = GetParsedCtrl(pWnd);

		// Try for Parsed Control			
		if (pControl && (pControl->GetCtrlData() == pField))
		{
			if (pWnd)
				pWnd->SetFocus();
			bFound = TRUE;
			break;
		}
	}

	if (bFound)
		return;

	TabManagers* pTabManagers = pView->m_pTabManagers;
	for (int i = 0; i < pTabManagers->GetSize(); i++)
	{
		CTabManager* pTabManager = pTabManagers->GetAt(i);
		ASSERT(pTabManager);
		for (int j = 0; j < pTabManager->GetDlgInfoArray()->GetSize(); j++)
		{
			DlgInfoItem* pInfo = pTabManager->GetDlgInfoArray()->GetAt(j);
			pTabManager->TabDialogActivate(pTabManager->GetDlgCtrlID(), pInfo->GetDialogID());
			CBaseTabDialog* pTab = pTabManager->GetActiveDlg();

			CTileGroup* pTileGroup = pTabManager->GetActiveTileGroup();
			if (pTileGroup)
			{
				CParsedCtrl* pCtrl = pTileGroup->GetLinkedParsedCtrl(pField);
				if (pCtrl)
				{
					CWnd* pWndCtrl = pCtrl->GetCtrlCWnd();
					if (pWndCtrl)
						pWndCtrl->SetFocus();
					return;
				}
			}

			pLinks = pTab->GetControlLinks();
			for (int i = 0; i < pLinks->GetSize(); i++)
			{
				CWnd* pWnd = pLinks->GetAt(i);
				ASSERT(pWnd);
				CParsedCtrl* pCtrl = GetParsedCtrl(pWnd);
				if (pCtrl)
				{
					CWnd* pWndCtrl = pCtrl->GetCtrlCWnd();
					if (pWndCtrl)
						pWndCtrl->SetFocus();
					return;
				}
			}

		}
	}
}

//-----------------------------------------------------------------------------
BOOL CAbstractFormDoc::UseWoormRadar()
{
	DataObj* pSetting = AfxGetSettingValue(snsTbGenlib, szPreferenceSection, szUseWoormRadar, DataBool(FALSE), szTbDefaultSettingFileName);

	return  pSetting ? *((DataBool*)pSetting) : FALSE;
}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::OnRadar()
{
	Radar();
}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::OnAltRadar()
{
	AltRadar();
}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::OnWrmRadar()
{
	if (AfxIsRemoteInterface() || !CanDoRunWrmRadar())
		OnOldRadar();
	else
		OnFileWrmRadar(0);
}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::OnNewWrmRadar()
{
	NewWrmRadar();
}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::OnFileWrmRadar(UINT nID)
{
	if (!CanDoRadar()) return;

	// radar already attached
	if (m_pRadarDoc)
	{
		m_pRadarDoc->ActivateAndShowRadar();
		return;
	}

	// force Update for all controls view
	GetNotValidView(FALSE);

	// se nID == 0 utilizzza il report assegnato come default se esiste, altrimenti ne crea uno
	// altrimenti arriva dal menu dropdown del pulsante Radar della toolbar
	CString strReportName;
	if (nID != 1 && nID != 0)
	{
		strReportName = m_pFormManager->GetRadarName(nID - ID_EXTDOC_DDTB_ENUMRADAR0);
		if (!strReportName.IsEmpty())
			m_pFormManager->m_Radars.SetCurrentDefaultReport(strReportName);
	}
	else if (nID == 0)
		strReportName = m_pFormManager->GetRadarName();

	// costruisce un report sulla base del master record
	BOOL bTemporary = FALSE;
	if (strReportName.IsEmpty())
	{
		strReportName = AfxGetTBRadarFactory()->BuildWoormRadar(this);
		if (strReportName.IsEmpty())
			return;
		bTemporary = TRUE;
	}

	// istanzia il report come WoormRadar (bUseRadarFrame=TRUE)
	m_pRadarDoc = AfxCreateTBRadar(this, strReportName, bTemporary);

	if (m_pRadarDoc)
	{
		m_pRadarDoc->EnableStayAlive(TRUE, FALSE);

		// uso una tabella ed un record locali perche modifico la query in maniera libera.
		SqlRecord*	pSqlRecord = m_pDBTMaster->GetRecord()->Create();
		SqlTable*	pSqlTable = new SqlTable(pSqlRecord, m_pSqlConnection->GetDefaultSqlSession());

		// Carico la ORDER BY e la WHERE presente nel documento oppure quella costruita
		// per fare una find intelligente
		CString strAuxFilter;
		if (GetFormMode() == CBaseDocument::FIND)
		{
			PrepareFindQuery(pSqlTable);

			strAuxFilter = m_pRadarDoc->UpdateWhereClause(pSqlTable);
		}
		else
		{
			m_pBrowser->Query();	//PERCH� ? //@@TODO BAUZI da creare un metodo di clonazione della filter senza necessit� di eseguire la query

			if (!m_pBrowser->GetTable()->GetNativeFilter().IsEmpty())
				strAuxFilter = m_pRadarDoc->UpdateWhereClause(m_pBrowser->GetTable());
			else
			{
				m_pDBTMaster->PrepareBrowser(pSqlTable);

				strAuxFilter = m_pRadarDoc->UpdateWhereClause(pSqlTable);
			}
		}

		delete pSqlTable;
		delete pSqlRecord;

		m_pRadarDoc->Run(strAuxFilter);
	}
}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::OnOldRadar()
{
	OldRadar();
}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::OnCustomize()
{
	Customize();
}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::OnGenerateJsonModel()
{
	CJsonModelGenerator bsp(this);
	bsp.Generate();

}
// MagoXp equivalent behaviour: open report list
//-----------------------------------------------------------------------------
void CAbstractFormDoc::OnProperties()
{
	ASSERT(FALSE);
	//NMTOOLBAR pnmtb;
	//pnmtb.hdr.code		= TBN_DROPDOWN;
	//// TODO farsi ritornare il puntatore alla finestra della toolbar della tabbed toolbar che contiene l'ID
	//pnmtb.hdr.hwndFrom	= GetMasterFrame()->m_pToolBar ? GetMasterFrame()->m_pToolBar->m_hWnd : NULL;
	//pnmtb.hdr.idFrom	= AFX_IDW_TOOLBAR;
	//pnmtb.iItem			= ID_EXTDOC_REPORT;
	//
	//GetMasterFrame()->SendMessage(WM_NOTIFY, (WPARAM) pnmtb.hdr.idFrom, (LPARAM) &pnmtb);
}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::DoCustomize(BOOL bExecEnabled, LPCTSTR pszNewFile /*= NULL*/)
{
	if (!CanDoCustomize()) return;

	if (AfxGetPathFinder()->IsAllUsersPath(pszNewFile))
		m_pFormManager->m_Radars.m_bModAllUsers = TRUE;

	if ((m_pFormManager->EditForm(bExecEnabled, pszNewFile) == IDC_FORMMNG_REPORT_EXEC) && bExecEnabled)
		OnWrmRadar();
}

// Double and derived data types have problems with noise epsilons and input 
// formatted rounding. In many cases the input dataobj is formatted a little 
// different from the value stored originally into database, so if I want to 
// be sure to catch the exact value I have to apply the same rounding rules.
//-----------------------------------------------------------------------------
void CAbstractFormDoc::AdjustDoubleInWhereClause(
	const CString& strParam,
	const DataObj& aRadarDataObj,
	SqlTable* pTable,
	DataDbl* pDataObj,
	SqlRecord* pRec /*=NULL*/
)
{
	if (pRec)
		pTable->AddBetweenColumn(pRec, aRadarDataObj);
	else
		pTable->AddBetweenColumn(aRadarDataObj);

	pTable->AddParam(strParam + _T("_From"), *pDataObj);
	pTable->AddParam(strParam + _T("_To"), *pDataObj);

	CParsedCtrl* pInputCtrl = GetLinkedParsedCtrl(pDataObj);

	// the most suitable decimal used and the tollerance given by
	// all the values that could have been rounded to the value
	const  CDblFormatter* pFormatter = pInputCtrl ? (CDblFormatter*)pInputCtrl->GetCurrentFormatter() : NULL;
	double dRoundedValue = *pDataObj;
	double dTollerance = 0.5;

	// input formatter could use different rounding behaviours
	if (pFormatter)
	{
		dTollerance = pFormatter->GetQuantumFromDecNumber(pInputCtrl->GetCtrlNumDec());
		dRoundedValue = pFormatter->GetRoundedValue(dRoundedValue, dTollerance);
	}
	else
	{
		// by default I apply mathematical rounding and implicitly I include epsilon noise
		for (int i = 0; i < GetNumDecimals(pDataObj->GetEpsilon(), EPSILON_DECIMAL); i++)
			dTollerance = dTollerance / 10.0;
		FormatHelpers::Round(dRoundedValue, dTollerance, FormatHelpers::Signed);
	}

	pTable->SetParamValue(strParam + _T("_From"), DataDbl(dRoundedValue - dTollerance));
	pTable->SetParamValue(strParam + _T("_To"), DataDbl(dRoundedValue + dTollerance));
}

//-----------------------------------------------------------------------------
BOOL CAbstractFormDoc::PrepareFindQuery(int& nParamIdx, SqlTable* pTable, SqlRecord* pRec, BOOL bFromXTech /*=FALSE*/)
{
	CString strParam;
	BOOL bSubset = FALSE;
	for (int i = 0; i < pRec->GetSizeEx(); i++)
	{
		SqlRecordItem* pRecItem = pRec->GetAt(i);
		DataObj* pDataObj = pRecItem->GetDataObj();

		if (!pDataObj->IsFindable())
			continue;

		// Se l'utente forza il modificato viene tenuto presente anche il
		// campo vuoto (per tener conto dei valori di default per enum e set 
		// e i valori zero per i numeri)

		//IsValueLocked() serve nel caso in cui il metodo sia chiamato per smart Import\Export

		//A. 18528: in conseguenza dell'affermazuione precedente, perch� testare "!IsEmpty()" ? 
		//(implica che vengono gestiti ad esempio i campi logici che nascono a true e tutto cio che viene inizializzato 
		//nei metodi Init delle classi derivate da di DBT e SqlRecord)
		BOOL b2 = pDataObj->IsValueChanged(); //impostato dal menu di contesto, FALSE come default (non confondere con il "Modified")
		if (!pDataObj->IsEmpty() || b2 || (bFromXTech && pDataObj->IsValueLocked()))
		{
			bSubset = TRUE; // ho selezionato un subset dei dati
			//DataObj& aRadarDataObj = *(pTable->GetRecord()->GetDataObjAt(i));
			strParam = cwsprintf(_T("itri%d"), nParamIdx++);

			// Sulle stringe commuta automaticamente in like     	
			if (
				(pDataObj->GetDataType() == DATA_STR_TYPE || pDataObj->GetDataType() == DATA_TXT_TYPE)
				&&
				!pDataObj->IsEmpty()
				)
			{
				pTable->AddFilterLike(pRec->IsQualified() ? pRec->GetQualifiedColumnName(i, TRUE) : pRecItem->GetColumnName());

				if (pDataObj->GetDataType() == DATA_STR_TYPE)
					pTable->AddParam(strParam, *pDataObj);
				else
					pTable->AddDataTextParam(strParam, *pDataObj, pRecItem->GetColumnName());

				pTable->SetParamLike(strParam, *pDataObj);
			}
			else
			{
				// see comment on the top of AdjustDoubleInWhereClause method!
				if (
					pDataObj->GetDataType() == DATA_DBL_TYPE ||
					pDataObj->GetDataType() == DATA_MON_TYPE ||
					pDataObj->GetDataType() == DATA_PERC_TYPE ||
					pDataObj->GetDataType() == DATA_QTA_TYPE
					)
				{
					AdjustDoubleInWhereClause(strParam, *pDataObj, pTable, (DataDbl*)pDataObj, pRec);
				}
				else
				{
					pTable->AddFilterColumn(pRec->IsQualified() ? pRec->GetQualifiedColumnName(i, TRUE) : pRecItem->GetColumnName());

					pTable->AddParam(strParam, *pDataObj);
					pTable->SetParamValue(strParam, *pDataObj);
				}
			}
		}
	}
	return bSubset;
}

//-----------------------------------------------------------------------------
BOOL CAbstractFormDoc::CanFindOnSlave() const
{
	if (!m_pSqlConnection || m_pSqlConnection->GetDBMSType() == DBMS_ORACLE)
		return FALSE;

	DataObj* pB = AfxGetSettingValue(snsTbGenlib, szPreferenceSection, szEnableFindOnSlaveFields, DataBool(TRUE), szTbDefaultSettingFileName);
	BOOL bEnableFindSlave = (pB ? *((DataBool*)pB) : TRUE) && m_pFormManager->m_Properties.m_bFindSlave;
	return bEnableFindSlave;
}

// Usata Dal RadarDoc (nella routine Attach) per fare la query intelligente in 
// stato di FIND quando si preme F7
//-----------------------------------------------------------------------------
void CAbstractFormDoc::PrepareFindQuery(SqlTable* pTable, BOOL bFromXTech /*=FALSE*/)
{
	SelectRadarColumns(pTable);

	// Da un chance al programmatore di aggiungere altre condizioni di filtraggio 
	// o di sort (anche se il sort non serve quasi mai e rallenta le cose)
	m_pDBTMaster->PrepareFindQuery(pTable);

	SqlRecord* pMasterRec = m_pDBTMaster->GetRecord();

	// Aggiunge nella WHERE i filtri sui campi compilati
	int nParamIdx = 0;
	BOOL bSubset = PrepareFindQuery(nParamIdx, pTable, pMasterRec, bFromXTech);

	BOOL bEnableFindSlave = CanFindOnSlave();
	if (m_pDBTMaster->GetDBTSlaves() && !bFromXTech && bEnableFindSlave)
	{
		for (int d = 0; d < m_pDBTMaster->GetDBTSlaves()->GetCount(); d++)
		{
			DBTSlave* pDBT = m_pDBTMaster->GetDBTSlaves()->GetAt(d);
			if (!pDBT || pDBT->IsKindOf(RUNTIME_CLASS(DBTSlaveBuffered))) continue;

			int lenWhere = pTable->m_strFilter.GetLength();

			SqlRecord* pSlaveRec = pDBT->GetRecord();
			// enum Master and Slaves Foreign keys to simulate inner join on subquery 
			if (!pDBT->m_pFKReader)
			{
				pDBT->m_pFKReader = new SqlForeignKeysReader();
				pDBT->m_pFKReader->LoadForeignKeys
				(
					m_pDBTMaster->GetRecord()->GetTableName(),
					pDBT->GetRecord()->GetTableName(),
					GetReadOnlySqlSession()
				);
			}
			if (!pDBT->m_pFKReader->GetSize())
				continue;

			// Aggiunge nella WHERE i filtri sui campi compilati nelle tabelle slave
			BOOL bAdd = PrepareFindQuery(nParamIdx, pTable, pSlaveRec, FALSE);
			if (bAdd)
			{
				CString sSlaveTable, sSlaveCol, sMasterTable, sMasterCol, subSelect;
				for (int i = 0; i <= pDBT->m_pFKReader->GetUpperBound(); i++)
				{
					pDBT->m_pFKReader->GetForeignKey(i, sSlaveTable, sSlaveCol, sMasterTable, sMasterCol);

					DataObj* pMasterDataObj = pMasterRec->GetDataObjFromColumnName(sMasterCol);
					DataObj* pSlaveDataObj = pSlaveRec->GetDataObjFromColumnName(sSlaveCol);

					if (
						!pMasterDataObj || !pSlaveDataObj ||
						!DataType::IsCompatible(pMasterDataObj->GetDataType(), pSlaveDataObj->GetDataType()) ||
						pMasterRec->GetTableName().CompareNoCase(sMasterTable) ||
						pSlaveRec->GetTableName().CompareNoCase(sSlaveTable)
						)
						continue;

					pTable->m_strFilter += cwsprintf(_T(" AND %s.%s = %s.%s"), sMasterTable, sMasterCol, sSlaveTable, sSlaveCol);
				}

				//tolgo l' AND iniziale aggiunto dalla PrepareFindQuery (nParamIdx, pTable, pSlaveRec, FALSE);
				if (lenWhere)
					pTable->m_strFilter = pTable->m_strFilter.Left(lenWhere) + pTable->m_strFilter.Mid(lenWhere + 4);

				//inserisco il prologo della clausola di subquery
				CString s; s.Format(_T("%sEXISTS (SELECT * FROM %s WHERE "),
					(lenWhere ? _T(" AND ") : _T("")), pSlaveRec->GetTableName());
				pTable->m_strFilter.Insert(lenWhere, s);
				//chiusura della clausola di subquery
				pTable->m_strFilter += ')';
			}
		}
	}

	// mette la condizione di Order By
	switch (m_pFormManager->m_Properties.m_OrderByType)
	{
		// eseguo un OrderBy con tutti i campi inseriti nel filtro di ricerca, 
		// altrimenti lascio qella di default che mi arriva nella tabella
	case FormProperties::FINDABLE_QUERY:
	{
		// sicuramente il criterio di sort e` diverso e quindi pulisco.
		if (bSubset) pTable->m_strSort.Empty();
		for (int i = 0; i <= pTable->GetRecord()->GetUpperBound(); i++)
		{
			DataObj* pMasterDataObj = m_pDBTMaster->m_pRecord->GetDataObjAt(i);
			if (!pMasterDataObj->IsFindable())
				continue;

			if (!pMasterDataObj->IsEmpty() || pMasterDataObj->IsValueChanged())
				pTable->AddSortColumn(*(pTable->GetRecord()->GetDataObjAt(i)), m_pFormManager->m_Properties.m_bDescending);
		}
		break;
	}

	// � stato chiamato il radar in FIND ma senza toccare i campi di ricerca
	case FormProperties::BROWSER_QUERY:
	{
		// se non devo aggiungere il DESCENDING non tocco nulla ma uso il sort del browser
		if (!m_pFormManager->m_Properties.m_bDescending)
		{
			if (pTable->m_strSort.IsEmpty())
				pTable->m_strSort = m_pBrowser->m_pTable->m_strSort;
			break;
		}

		CString strSort = pTable->m_strSort;

		// eredita quella imposta dal programmatore nella OnPrepareFindQuery
		if (pTable->m_strSort.IsEmpty())
			strSort = m_pBrowser->m_pTable->m_strSort;
		else
			pTable->m_strSort.Empty();

		// aggiungo il descendig riparsando tutta la stringa di sort
		int i = 0;
		int start = 0;


		while (i >= 0)
		{
			i = strSort.Find(',', start);
			CString strColumn;
			if (i >= 0)
				strColumn = strSort.Mid(start, i - start);
			else
				strColumn = strSort.Right(strSort.GetLength() - start);

			start = i + 1;
			if (!strColumn.IsEmpty())
			{
				// controlla se per caso c'e` gia il descending nel qual caso lascia tutto come sta
				BOOL bDescending = strColumn.Find(szDescending) == -1
					? m_pFormManager->m_Properties.m_bDescending
					: FALSE;

				pTable->AddSortColumn(strColumn, bDescending);
			}
		}
		break;
	}

	default: break; // non mette niente
	}

	// rimette lo stato del documento a non modificato per evitare
	// inutili richieste di conferma da parte del radar per entrare in modifica
	SetModifiedFlag(FALSE);
}

// Usata Dal RadarDoc (nella routine Attach) per fare la query intelligente in 
// stato di FIND quando si preme F7
//-----------------------------------------------------------------------------
void CAbstractFormDoc::PrepareRadarQuery(SqlTable* pTable)
{
	if (GetFormMode() == CBaseDocument::FIND)
		PrepareFindQuery(pTable);
	else
		m_pDBTMaster->PrepareBrowser(pTable);
}

void CAbstractFormDoc::SelectRadarColumns(SqlTable* pTable)
{
	pTable->SelectAll();
}

//-----------------------------------------------------------------------------
CString CAbstractFormDoc::GetDefaultReport()
{
	CString s = GetDefaultForm();
	if (!s.IsEmpty())
		return s;

	ASSERT(m_pFormManager);
	return m_pFormManager->GetReportName();
}

//-----------------------------------------------------------------------------
CString CAbstractFormDoc::GetDefaultForm()
{
	return L"";
}
//-----------------------------------------------------------------------------
CString CAbstractFormDoc::GetDefaultTitleForm()
{
	return L"";
}

//-----------------------------------------------------------------------------
//metodo chiamato dal nuovo men� per avere la stringa descrittiva che rappresenta le informazioni utili per identificare il record corrente
CString CAbstractFormDoc::GetDefaultMenuDescription()
{
	if (!this->GetMaster() || !this->GetMaster()->GetRecord())
		return _T("");

	if (m_bOnlyOneRecord || this->GetMaster()->GetRecord()->IsPKEmpty())
		return _T("");

	CString strResult = GetDocumentObjectCaption();
	if (!strResult.IsEmpty())
		return strResult;

	return this->GetMaster()->GetRecord()->GetPrimaryKeyNameValue();
}


//-----------------------------------------------------------------------------
void  CAbstractFormDoc::GetDocumentObjectCaptionDataObjArray(DataObjArray& ar)
{
	const CDocumentDescription* pDocDescri = AfxGetDocumentDescription(GetNamespace());
	CString strResult = _T("");
	DataObj* pData = NULL;

	if (pDocDescri && pDocDescri->GetDescriptionKeys()->GetCount() < 0)
		return;

	for (int i = 0; i <= pDocDescri->GetDescriptionKeys()->GetUpperBound(); i++)
	{
		CString strSource = pDocDescri->GetDescriptionKeys()->GetAt(i);

		int nDot = strSource.Find('.');
		if (nDot < 0 && this->GetMaster() && this->GetMaster()->GetRecord())
		{
			pData = this->GetMaster()->GetRecord()->GetDataObjFromName(strSource);
			if (pData)
				ar.Add(pData);
		}
		else
		{
			CString strField = strSource.Mid(nDot + 1);
			CString strTable = strSource.Left(nDot);
			DBTObject* pDBt = this->GetDBTObject(strTable);
			if (pDBt != NULL && pDBt->GetRecord())
			{
				pData = pDBt->GetRecord()->GetDataObjFromName(strField);
				if (pData)
					ar.Add(pData);
			}
		}
	}
}

//-----------------------------------------------------------------------------
CString CAbstractFormDoc::GetDocumentObjectCaption()
{
	const CDocumentDescription* pDocDescri = AfxGetDocumentDescription(GetNamespace());
	CString strResult = _T("");
	DataObj* pData = NULL;
	CStringArray fields;

	if (pDocDescri && pDocDescri->GetDescriptionKeys()->GetCount() < 0)
		return _T("");

	if (this->GetMaster() && this->GetMaster()->GetRecord() && this->GetMaster()->GetRecord()->IsEmpty())
		return _T("");

	CString strFormatString = pDocDescri->GetCaption();

	for (int i = 0; i <= pDocDescri->GetDescriptionKeys()->GetUpperBound(); i++)
	{
		CString strSource = pDocDescri->GetDescriptionKeys()->GetAt(i);

		int nDot = strSource.Find('.');
		if (nDot < 0 && this->GetMaster() && this->GetMaster()->GetRecord())
		{
			pData = this->GetMaster()->GetRecord()->GetDataObjFromName(strSource);
			if (pData)
			{
				fields.Add(pData->FormatData());
				strResult += strSource + _T(": ") + pData->FormatData() + _T(" ");
			}
		}
		else
		{
			CString strField = strSource.Mid(nDot + 1);
			CString strTable = strSource.Left(nDot);
			DBTObject* pDBt = this->GetDBTObject(strTable);
			if (pDBt != NULL && pDBt->GetRecord())
			{
				pData = pDBt->GetRecord()->GetDataObjFromName(strField);
				if (pData)
				{
					fields.Add(pData->FormatData());
					strResult += strField + _T(": ") + pData->FormatData() + _T(" ");
				}
			}
		}
	}

	if (strFormatString.IsEmpty())
		return strResult;

	return StringFormat(strFormatString, fields);
}

//-----------------------------------------------------------------------------
CBaseTileDialog* CAbstractFormDoc::GetTileDialog(UINT nIDD)
{
	POSITION pos = GetFirstViewPosition();
	while (pos != NULL)
	{
		CView* pView = GetNextView(pos);
		ASSERT_VALID(pView);
		CAbstractFormView* pAbView = dynamic_cast<CAbstractFormView*>(pView);
		if (!pAbView)
			continue;

		CBaseTileDialog* pDlg = pAbView->GetTileDialog(nIDD);
		if (pDlg)
			return pDlg;
	}
	return NULL;

}

//-----------------------------------------------------------------------------
CString CAbstractFormDoc::GetDefaultTitleReport()
{
	ASSERT(m_pFormManager);
	CFunctionDescription* pInfo = m_pFormManager->GetReportDescription();
	if (pInfo != NULL)
		return pInfo->GetTitle();

	return _T("");
}

//-----------------------------------------------------------------------------
CReportManager& CAbstractFormDoc::GetReportManager()
{
	return m_pFormManager->m_Reports;
}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::OnListReports()
{
	if (!m_pFormManager) return;

	CReportMenuNode* pRoot = new CReportMenuNode;
	pRoot->SetNodeTag(_T("ROOT"));
	pRoot->SetVisible(FALSE);

	m_pFormManager->EnumReportAlias(pRoot);

	CListDocumentReportsDlg dlgDocumentReports(this, pRoot);
	dlgDocumentReports.DoModal();
	//pRoot will be deleted by CListDocumentReportsDlg distructor
}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::OnFormReport()
{
	FormReport();
}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::OnReport()
{
	OnFormReport();
}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::OnTabSwitch()
{
	CMasterFrame* pMasterFrame = GetMasterFrame();
	if (pMasterFrame)
	{
		CTBTabbedToolbar* pTabbed = pMasterFrame->GetTabbedToolBar();
		if (pTabbed)
		{
			pTabbed->MoveNextTab();
		}
	}
}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::OnTabSwitchRange(UINT nID)
{
	int nPos = nID - ID_EXDOC_TAB_SWITCH_MENU_START;
	CMasterFrame* pMasterFrame = GetMasterFrame();
	if (pMasterFrame)
	{
		CTBTabbedToolbar* pTabbed = pMasterFrame->GetTabbedToolBar();
		if (pTabbed)
		{
			pTabbed->SetActiveTabByPos(nPos);
		}
	}
}


//-----------------------------------------------------------------------------
void CAbstractFormDoc::OnSelReport(UINT nID)
{
	if (!CanDoReport())
		return;

	ASSERT(m_pFormManager);

	// se nID == 0 utilizza il report assegnato come default se esiste, altrimenti ne crea uno
	// altrimenti arriva dal menu dropdown del pulsante Radar della toolbar

	CFunctionDescription* fd =
		(nID ?
			fd = m_pFormManager->GetReportDescription(nID - ID_EXTDOC_DDTB_ENUMREPORT0)
			:
			fd = m_pFormManager->GetReportDescription()
			);

	if (fd)
	{
		CString strReportName = fd->GetNamespace().ToString();

		if (!ReportIsEnabled(strReportName))
			return;

		if (nID && m_bUpdateDefaultReport)
			m_pFormManager->m_Reports.SetCurrentDefaultReport(strReportName);

		DoRunReport(strReportName);
	}
	else
		AfxMessageBox(_TB("The default report selection is missing!"));
}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::DoRunReport(const CString& strReportName)
{
	CWoormInfo* pwi = new CWoormInfo(DataStr(strReportName));
	pwi->m_bOwnedByReport = TRUE;
	if (!DispatchRunReport(pwi))
	{
		delete pwi;
		return;
	}

	/* provoca un memory leak ma evita che chiudendo la finestra di uno dei report
	si schiantino gli altri in chiusura sulla delete del woorminfo condiviso.
	L'automatismo di chiusura del documento chiamante basato sul m_pReportDoc non copre
	le sucessive istanze di WoormDoc della sequenza.
	m_bOwnedByReport potrebbe diventare un references counter stile COM */
	if (pwi->m_ReportNames.GetSize() > 1)
		pwi->m_bOwnedByReport = FALSE;
	//----

	RunWoormReport(pwi);
}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::SelReport(UINT idxReport)
{
	OnSelReport(idxReport + ID_EXTDOC_DDTB_ENUMREPORT0);
}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::ChangeReports(const CString& strDocumentNamespace)
{
	CTBNamespace aNs;
	aNs.AutoCompleteNamespace(CTBNamespace::DOCUMENT, strDocumentNamespace, aNs);

	ASSERT(m_pFormManager);
	if (!m_pFormManager)
		return;

	BeginWaitCursor();

	m_pFormManager->m_Ns.SetNamespace(aNs);
	m_pFormManager->m_Reports.Clear();
	m_pFormManager->m_Reports.Parse(aNs);

	EndWaitCursor();
}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::SetSpecificReports(const CString& sForPrint, const CString& sForEmail)
{
	ASSERT(m_pFormManager);
	if (!m_pFormManager)
		return;

	m_pFormManager->m_Reports.m_NsSpecificReportForPrint.SetNamespace(sForPrint);
	m_pFormManager->m_Reports.m_NsSpecificReportForEmail.SetNamespace(sForEmail);

}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::OnQuery()
{
	Query();
}


// Esegue la query correntemente selezionata con il pulsante !  
//-----------------------------------------------------------------------------
void CAbstractFormDoc::OnExecQuery()
{
	ExecQuery();
}

// Esegue la query correntemente selezionata con il pulsante !  
//-----------------------------------------------------------------------------
void CAbstractFormDoc::FormHelp()
{
	ShowHelp(this->GetNamespace().ToString());
#ifdef _DEBUG
	//m_bUseEasyReading = !m_bUseEasyReading;
#endif
}

// Riaggiorna i dati (utilizzato in caso di cursore statico e keyset)
//--------------------------------------------------------------------------------
void CAbstractFormDoc::OnRefreshRowset()
{
	RefreshRowset();
}


// Esegue la query correntemente selezionata nella dialog del QueryManager 
//--------------------------------------------------------------------------------
void CAbstractFormDoc::OnExecPostQuery()
{
	if (!CanDoExecQuery())
		return;

	if (m_pBrowser->LastRecord())
	{
		BrowseRecord();
		return;
	}

	Message(_TB("No documents have been selected by the set query."));

	InitData();
	ClearCurrentRecord();
	GoInBrowseMode();
}

// la query viene solo eseguita
// @@TODO da ripristinare
//-----------------------------------------------------------------------------
void CAbstractFormDoc::OnExecSelQuery(UINT nID)
{
	if (!CanDoExecQuery())
		return;

	CString strQueryName = GetMasterFrame()->GetQueryName(nID);
	if (strQueryName.IsEmpty()) return;

	m_pQueryManager->SelectQuery(strQueryName);
}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::OnOtherQuery()
{
	OtherQuery();
}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::OnEditQuery()
{
	EditQuery();
}

// It's called on activation/focusing of CAbstractFormFrame/CAbstractFormView
// to ensure the document data validation
//--------------------------------------------------------------------------
CView* CAbstractFormDoc::GetNotValidView(BOOL bSignalError /* = FALSE */)
{
	POSITION pos = GetFirstViewPosition();
	while (pos != NULL)
	{
		CView* pView = GetNextView(pos);
		ASSERT_VALID(pView);
		if (pView != NULL && pView->IsKindOf(RUNTIME_CLASS(CAbstractFormView)))
			if (!((CAbstractFormView*)pView)->CheckForm(bSignalError && !IsInUnattendedMode()))
				return pView;
	}
	return NULL;
}

// Initialize flags to initial state for all views (i.e. error flags etc...)
//--------------------------------------------------------------------------
void CAbstractFormDoc::InitViewsFormFlags()
{
	POSITION pos = GetFirstViewPosition();
	while (pos != NULL)
	{
		CView* pView = GetNextView(pos);
		ASSERT_VALID(pView);
		if (pView != NULL && pView->IsKindOf(RUNTIME_CLASS(CAbstractFormView)))
			((CAbstractFormView*)pView)->InitFormFlags();
	}
}

//--------------------------------------------------------------------------
void CAbstractFormDoc::AbortAllViews()
{
	POSITION pos = GetFirstViewPosition();
	while (pos != NULL)
	{
		CView* pView = GetNextView(pos);
		ASSERT_VALID(pView);
		if (pView != NULL && pView->IsKindOf(RUNTIME_CLASS(CAbstractFormView)))
			((CAbstractFormView*)pView)->AbortForm();
	}
}

//------------------------------------------------------------------------------
void CAbstractFormDoc::SetViewFocus(CView* pView, BOOL bActivate /*TRUE*/)
{
	ASSERT_VALID(pView);
	CAbstractFormView* pBaseView = dynamic_cast<CAbstractFormView*>(pView);
	if (!pBaseView)
		return;

	if (bActivate)
		Activate(pView);

	pBaseView->SetDefaultFocus();
}

//------------------------------------------------------------------------------
void CAbstractFormDoc::SetDefaultFocus()
{
	// Può fare la SetDefaultFocus se il documento è ancora il documento correntemente attivo
	// (p.e. se è stato lanciato un report il fuoco deve rimanere al report)
	if (AfxGetMainWnd() != GetFrame())
		return;
	
	// set focus for all view without activate frame and view
	//
	// SILVANO e BRUNA: commentato perche in caso di piu View non si ferma nel momento in cui trova il primo controllo focusable, ma cicla su tutte le 
	// view, facendo scattare valuechanged
	//
	//POSITION pos = GetFirstViewPosition();
	//while (pos != NULL)
	//	SetViewFocus(GetNextView(pos), FALSE);

	if (m_bNeedsUpdateDataView)
		ExecuteUpdateDataView();
	
	// set focus for master view activating also frame and view
	POSITION first = GetFirstViewPosition();
	if (first != NULL)
		SetViewFocus(GetNextView(first));
}

//------------------------------------------------------------------------------
void CAbstractFormDoc::SetFormTitle(UINT nTitleID)
{
	CString strTitle;
	strTitle = AfxLoadTBString(nTitleID, GetDllInstance(GetRuntimeClass()));
	SetFormTitle(strTitle);
}

//------------------------------------------------------------------------------
void CAbstractFormDoc::SetFormTitle(const CString& strTitle)
{
	SetTitle(strTitle);
	DeclareVariable(_T("Title"), new DataStr(strTitle), TRUE, TRUE);

}

//-----------------------------------------------------------------------------
BOOL CAbstractFormDoc::SetCtrlFocus(UINT nIDC)
{
	if (AfxIsRemoteInterface())
		return FALSE;

	CWnd* pWnd = GetWndLinkedCtrl(nIDC);
	if (pWnd == NULL)
		return FALSE;

	pWnd->GetParent()->POST_WM_COMMAND(nIDC, PCN_SET_FOCUS, pWnd->m_hWnd);
	return TRUE;
}

// Questo metodo permette di risalire alla CWnd di un control IDC
//-----------------------------------------------------------------------------
CWnd* CAbstractFormDoc::GetWndLinkedCtrl(UINT nIDC)
{
	POSITION pos = GetFirstViewPosition();
	while (pos != NULL)
	{
		CView* pView = GetNextView(pos);
		ASSERT_VALID(pView);
		if (pView->IsKindOf(RUNTIME_CLASS(CAbstractFormView)))
		{
			CWnd* pWndCtrl = ((CAbstractFormView*)pView)->GetWndLinkedCtrl(nIDC);
			if (pWndCtrl)
				return pWndCtrl;
		}
	}
	return NULL;
}

// Questo metodo permette di risalire alla CWnd di un control utilizzando il namespace
//-----------------------------------------------------------------------------
CWnd* CAbstractFormDoc::GetWndLinkedCtrl(const CTBNamespace& aNS)
{
	POSITION pos = GetFirstViewPosition();
	while (pos != NULL)
	{
		CView* pView = GetNextView(pos);
		ASSERT_VALID(pView);
		if (pView->IsKindOf(RUNTIME_CLASS(CAbstractFormView)))
		{
			CWnd* pWndCtrl = ((CAbstractFormView*)pView)->GetWndLinkedCtrl(aNS);
			if (pWndCtrl)
				return pWndCtrl;
		}
	}
	return NULL;
}

// Questo metodo permette di risalire al parsed ctrl
//-----------------------------------------------------------------------------
CParsedCtrl* CAbstractFormDoc::GetLinkedParsedCtrl(UINT nIDC)
{
	POSITION pos = GetFirstViewPosition();
	while (pos != NULL)
	{
		CView* pView = GetNextView(pos);
		ASSERT_VALID(pView);
		if (pView->IsKindOf(RUNTIME_CLASS(CAbstractFormView)))
		{
			CParsedCtrl* pCtrl = ((CAbstractFormView*)pView)->GetLinkedParsedCtrl(nIDC);
			if (pCtrl)
				return pCtrl;
		}
	}
	return NULL;
}

// Questo metodo permette di risalire al parsed ctrl
//-----------------------------------------------------------------------------
CParsedCtrl* CAbstractFormDoc::GetLinkedParsedCtrl(DataObj* pDataObj)
{
	POSITION pos = GetFirstViewPosition();
	while (pos != NULL)
	{
		CView* pView = GetNextView(pos);
		ASSERT_VALID(pView);
		if (pView->IsKindOf(RUNTIME_CLASS(CAbstractFormView)))
		{
			CParsedCtrl* pCtrl = ((CAbstractFormView*)pView)->GetLinkedParsedCtrl(pDataObj);
			if (pCtrl)
				return pCtrl;
		}
	}
	return NULL;
}

// Questo metodo permette di risalire al parsed ctrl
//-----------------------------------------------------------------------------
CParsedCtrl* CAbstractFormDoc::GetLinkedParsedCtrl(const CTBNamespace& aNS)
{
	POSITION pos = GetFirstViewPosition();
	while (pos != NULL)
	{
		CView* pView = GetNextView(pos);
		ASSERT_VALID(pView);
		if (pView->IsKindOf(RUNTIME_CLASS(CAbstractFormView)))
		{
			CParsedCtrl* pCtrl = ((CAbstractFormView*)pView)->GetLinkedParsedCtrl(aNS);
			if (pCtrl)
				return pCtrl;
		}
	}
	return NULL;
}

// Questo metodo permette di risalire al bodyedit
//-----------------------------------------------------------------------------
CBodyEdit* CAbstractFormDoc::GetBodyEdits(CTBNamespace aNS)
{
	POSITION pos = GetFirstViewPosition();
	while (pos != NULL)
	{
		CView* pView = GetNextView(pos);
		ASSERT_VALID(pView);
		if (pView->IsKindOf(RUNTIME_CLASS(CAbstractFormView)))
		{
			CBodyEdit* pBodyedit = ((CAbstractFormView*)pView)->GetBodyEdits(aNS);
			if (pBodyedit)
				return pBodyedit;
		}
	}
	return NULL;
}

//-----------------------------------------------------------------------------
CStateCtrlObj* CAbstractFormDoc::GetLastStateCtrlChanged(UINT nIDC)
{
	CParsedCtrl* pParsedCtrl = GetLinkedParsedCtrl(nIDC);

	return pParsedCtrl ? pParsedCtrl->GetLastStateCtrlChanged() : NULL;
}

// Questo metodo permette di risalire alla CWnd di un control IDC non AddLinkato
//-----------------------------------------------------------------------------
CWnd* CAbstractFormDoc::GetWndCtrl(UINT nIDC)
{
	POSITION pos = GetFirstViewPosition();
	while (pos != NULL)
	{
		CView* pView = GetNextView(pos);
		ASSERT_VALID(pView);
		if (pView->IsKindOf(RUNTIME_CLASS(CAbstractFormView)))
		{
			CWnd* pWndCtrl = ((CAbstractFormView*)pView)->GetWndCtrl(nIDC);
			if (pWndCtrl)
				return pWndCtrl;
		}
	}
	return NULL;
}


// Standard behaviour for value changed message. In caso di FIND non esegue
// alcuna azione di default per permettere di passare da un control all'altro
// senza scatenare alcuna azione
//--------------------------------------------------------------------------
LRESULT CAbstractFormDoc::OnValueChanged(WPARAM, LPARAM)
{
	SUSPEND_UPDATE_DATA_VIEW();
	// Samethings data changed, so inform document
	SetModifiedFlag();
	UpdateDataView();

	return 1L;
}

//-----------------------------------------------------------------------------
CString CAbstractFormDoc::GetOpeningBanner()
{
	AddOnModule* pAddOnMod = AfxGetAddOnModule(GetNamespace());
	CString strTitle = GetNamespace().GetObjectName();
	if (pAddOnMod)
	{
		const CDocumentDescription* pDocDescri = AfxGetDocumentDescription(GetNamespace());
		if (pDocDescri)
			strTitle = pDocDescri->GetTitle();
	}
	return cwsprintf(_TB("Messages from the document {0-%s}"), strTitle);
}

//-----------------------------------------------------------------------------
CView* CAbstractFormDoc::CreateSlaveView
(
	const CRuntimeClass* pViewClass,
	UINT nSubTitleID,
	const CRuntimeClass* pClientClass /*= NULL*/,
	const CString& strFormName, /*_T("")*/
	CWnd* pParent /*=NULL*/,
	BOOL bModal /*= FALSE*/
)
{
	CString strSubTitle;

	strSubTitle = AfxLoadTBString(nSubTitleID, GetDllInstance(pViewClass));
	return CreateSlaveView(pViewClass, strSubTitle, pClientClass, strFormName, pParent, bModal);
}
//-----------------------------------------------------------------------------
CView* CAbstractFormDoc::CreateSlaveView(UINT nFrameId, CWnd* pParent /*=NULL*/, BOOL bModal /*= FALSE*/)
{
	return CreateSlaveView(RUNTIME_CLASS(CJsonFormView), _T(""), NULL, _T(""), pParent, 0, nFrameId, bModal);
}

// Create a Salve view attached to Master view.
// Slave view is like normal view, but all save,delete and read action are
// posponed to Master view
//-----------------------------------------------------------------------------
CView* CAbstractFormDoc::CreateSlaveView
(
	const CRuntimeClass* pViewClass,
	const CString& strSubTitle,
	const CRuntimeClass* pDocClass, /*= NULL*/
	const CString& strFormName,  /*_T("")*/
	CWnd* pParent, /*=NULL*/
	UINT nViewId /*=0*/,
	UINT nFrameId /*=0*/,
	BOOL bModal /*= FALSE*/
)
{
	// If there is a non valid view of current document we abort slave creation
	// notifying the error for not valid control when bad data is present in some
	// edit. This control has to be done only if the slave view is a popup (i.e. pParent is null)
	if (!pParent && GetNotValidView(TRUE))
		return NULL;

	// enable other view only if not already created
	CView* pView = nFrameId
		? ViewAlreadyPresent(nFrameId)
		: ViewAlreadyPresent(pViewClass, nViewId);

	if (pView)
	{
		Activate(pView);
		return pView;
	}

	ASSERT(GetDocTemplate());
	ASSERT(GetDocTemplate()->IsKindOf(RUNTIME_CLASS(CSingleExtDocTemplate)));

	// cerca il template che e' composta dalla view indicata e dal documento in oggetto (this)
	const CSingleExtDocTemplate* pOriginalTemplate = NULL;
	if (pDocClass)
		pOriginalTemplate = AfxGetTemplate(pViewClass, nViewId, pDocClass);
	else if (nFrameId)
		pOriginalTemplate = AfxGetBaseApp()->GetDocTemplate(nFrameId);
	else
		pOriginalTemplate = AfxGetTemplate(pViewClass, nViewId, GetRuntimeClass());

	if (!pOriginalTemplate)
	{
		TRACE("CAbstractFormDoc::CreateSlaveView: not registered template. View: %s. Check interface file\n", pViewClass->m_lpszClassName);
		return NULL;
	}
	ASSERT(pOriginalTemplate->IsKindOf(RUNTIME_CLASS(CSingleExtDocTemplate)));
	AutoDeletePtr<CSingleExtDocTemplate> pTemplate = new CSingleExtDocTemplate(pOriginalTemplate, NULL);
	if (nViewId != 0)
		pTemplate->m_nViewID = nViewId;
	//OSL
	if
		(
			pTemplate->GetFrameClass()->IsDerivedFrom(RUNTIME_CLASS(CSlaveFrame))
			||
			pTemplate->GetFrameClass()->IsDerivedFrom(RUNTIME_CLASS(CRowFormFrame))
			)
	{
		CInfoOSL infoOsl(pTemplate->GetNamespace(), pTemplate->GetInfoOSL()->GetType());
		AfxGetSecurityInterface()->GetObjectGrant(&infoOsl);
		if (!OSL_CAN_DO(&infoOsl, OSL_GRANT_EXECUTE))
		{
			Message(cwsprintf(OSLErrors::MISSING_GRANT(), GetInfoOSL()->m_Namespace.ToString()));
			return NULL;
		}
	}

	if (!pTemplate->GetFrameClass()->IsDerivedFrom(RUNTIME_CLASS(CRowFormFrame)))
		m_nsCurrentViewParent = GetNamespace();

	CFrameWnd* pNewFrame = pTemplate->CreateNewFrame(this, NULL);

	m_nsCurrentViewParent.Clear();

	if (pNewFrame)
	{
		ASSERT(pNewFrame->IsKindOf(RUNTIME_CLASS(CAbstractFormFrame)));
		((CAbstractFormFrame*)pNewFrame)->SetSubTitle(strSubTitle);

		if (pParent)
		{
			if (pParent->IsKindOf(RUNTIME_CLASS(CSlaveViewContainer)))
			{
				DWORD dwRemoveForDockStyle = WS_THICKFRAME | WS_CAPTION | WS_POPUP;
				DWORD dwAddForDockStyle = WS_CHILD;
				pNewFrame->ModifyStyle(dwRemoveForDockStyle, dwAddForDockStyle);
				pNewFrame->ShowWindow(SW_SHOW);
			}
			pNewFrame->SetParent(pParent);
		}
		//mi serve il puntatore alla View per aggiungerci una informazione strutturale
		pView = NULL;

		CWnd* pWnd = pNewFrame->GetDescendantWindow(AFX_IDW_PANE_FIRST, TRUE);
		pView = dynamic_cast<CView*>(pWnd);
		if (!pView)
		{
			ASSERT(FALSE);
			return NULL;
		}

		CAbstractFormView* pAV = dynamic_cast<CAbstractFormView*>(pView);

		//Prj. 6709 - tentativo di risolvere il flickering
		//gestione le view "normali" ma fatte "modale"
		//memorizzo lo stato "modal" per farle fare la strada di defailt, cio� senza HIDE/SHOW
		if (pAV)
			pAV->SetModal(bModal);

		pTemplate->InitialUpdateFrame(pNewFrame, this);

		if (pAV)
		{
			pAV->GetInfoOSL()->m_pParent = GetInfoOSL();

			//if (pParent)
			//	pView->ModifyStyle(0, WS_CLIPCHILDREN);
			//� stato aggiunto per problemi di disegno degli static con view embedded sovrapposte,
			//l'ho tolto perch� causava sovrapposizioni nel disegno, sostituito con una UpdateWindow
			//nel momento di visualizzazione della view embedded
		}
		if (AfxIsRemoteInterface())
			UpdateDataView();
		//se sono modale, ciclo finch� la finestra non viene chiusa
		if (bModal)
		{
			((CAbstractFormFrame*)pNewFrame)->DoModal();
			return NULL;
		}
		return pView;
	}

	ASSERT(pNewFrame);
	return NULL;
}

// Enable or Disable all dataobj connected with control Link (to avoid editing)
//------------------------------------------------------------------------------
void CAbstractFormDoc::EnableAllControlLinks(BOOL bEnable/* = TRUE*/)
{
	POSITION pos = GetFirstViewPosition();
	while (pos != NULL)
	{
		CView* pView = GetNextView(pos);
		ASSERT_VALID(pView);
		if (pView != NULL && pView->IsKindOf(RUNTIME_CLASS(CAbstractFormView)))
		{
			((CAbstractFormView*)pView)->EnableViewControlLinks(bEnable);
		}
	}
}

//------------------------------------------------------------------------------
void CAbstractFormDoc::InvalidateActiveTabDialog()
{
	CAbstractFormView*	pAbstractView = NULL;
	CTabDialog*			pActiveTabDlg = NULL;


	POSITION pos = GetFirstViewPosition();
	while (pos != NULL)
	{
		CView* pView = GetNextView(pos);
		ASSERT_VALID(pView);
		if (pView == NULL || !pView->IsKindOf(RUNTIME_CLASS(CAbstractFormView)))
			continue;

		pAbstractView = (CAbstractFormView*)pView;

		if (pAbstractView->m_pTabManagers && pAbstractView->m_pTabManagers->GetSize())
			for (int i = 0; i <= pAbstractView->m_pTabManagers->GetUpperBound(); i++)
			{
				pActiveTabDlg = pAbstractView->m_pTabManagers->GetActiveDlg(i);
				if (pActiveTabDlg)
					pActiveTabDlg->Invalidate();
			}

	}
}
//------------------------------------------------------------------------------
void CAbstractFormDoc::SuspendUpdateDataView()
{
	m_nUpdateDataViewLevel++;
}
//------------------------------------------------------------------------------
void CAbstractFormDoc::ResumeUpdateDataView()
{
	m_nUpdateDataViewLevel--;
	ASSERT(m_nUpdateDataViewLevel >= 0);
	if (m_nUpdateDataViewLevel == 0 && m_bNeedsUpdateDataView)
		UpdateDataView();
}
//-----------------------------------------------------------------------------
void CAbstractFormDoc::OnTimer(UINT nTimer)
{
	if (nTimer == IDT_UPDATEDATAVIEW)
	{
		//qualcuno potrebbe averla forzata nel frattempo
		if (m_bNeedsUpdateDataView)
			ExecuteUpdateDataView();

		m_bWaitingForUpdateDataView = false;
		CLocalizableFrame* pFrame = GetFrame();
		if (pFrame)
			pFrame->KillTimer(IDT_UPDATEDATAVIEW);

	}
}
//------------------------------------------------------------------------------
void CAbstractFormDoc::UpdateDataView(BOOL bForce /*= FALSE*/)
{
	if (m_bClosing || m_bUpdatingDataView)
		return;
	//ottimizzazione: se sono in un contesto che effettua più refresh nidificati, 
	//effettuo la UpdateDataView solo all'ultima chiamata
	//se sto aspettando il timer per effettuare l'update e non devo forzare il refresh, esco
	if (!bForce && (/*m_nUpdateDataViewLevel > 0 || PERASSO: non dovrebbe essere più necessario, adesso c'è il timer */
		m_bWaitingForUpdateDataView))
	{
		m_bNeedsUpdateDataView = true;
		return;
	}

	int delay = GetRefreshDelay();
	if (bForce || delay <= 0)
		ExecuteUpdateDataView();
	else
	{
		//faccio la prima dell'intervallo, poi faccio partire il timer, 
		//se nel frattempo ne verrano richieste altre, allo scadere del timer verrà effettuata
		ExecuteUpdateDataView();
		m_bWaitingForUpdateDataView = true;
		CLocalizableFrame* pFrame = GetFrame();
		if (pFrame)
			pFrame->SetTimer(IDT_UPDATEDATAVIEW, GetRefreshDelay(), NULL);
		else
			ExecuteUpdateDataView();
	}


}
//------------------------------------------------------------------------------
void CAbstractFormDoc::ExecuteUpdateDataView()
{
	CLocalChange<bool> _loc(m_bUpdatingDataView, true);
	m_bNeedsUpdateDataView = false;
	//soppressa ottimizzazione: veniva alterato l'ordine delle chiamate 
	//(UpdateDataView chiamata dopo della setDefaultFocus invece che prima, questo faceva assegnare il fuoco ad un controllo sbagliato)

	if (
		m_pXMLDataManager &&
		m_pXMLDataManager->GetStatus() == CXMLDataManagerObj::XML_MNG_IMPORTING_DATA &&
		!m_pXMLDataManager->UseOldXTechMode()
		)
	{
		return;
	}

	if (m_viewList.IsEmpty())
	{
		return;
	}

	POSITION pos = GetFirstViewPosition();
	while (pos != NULL)
	{
		CView* pView = GetNextView(pos);
		ASSERT_VALID(pView);

		if (pView->IsKindOf(RUNTIME_CLASS(CAbstractFormView)))
		{
			BOOL bIsViewVisible = TRUE;
			((CAbstractFormView*)pView)->OnFindHotLinks();
			((CAbstractFormView*)pView)->OnUpdateControls(bIsViewVisible);
		}
	}

	for (int i = 0; i < m_JsonDialogs.GetCount(); i++)
	{
		CJsonDialog* pDialog = m_JsonDialogs[i];
		BOOL bIsViewVisible = TRUE;
		pDialog->OnFindHotLinks();
		pDialog->OnUpdateControls(bIsViewVisible);
	}
	pos = GetFirstViewPosition();
	while (pos != NULL)
	{
		CView* pView = GetNextView(pos);
		ASSERT_VALID(pView);

		if (pView->IsKindOf(RUNTIME_CLASS(CAbstractFormView)))
			((CAbstractFormView*)pView)->OnResetDataObjs();
	}
	for (int i = 0; i < m_JsonDialogs.GetCount(); i++)
	{
		CJsonDialog* pDialog = m_JsonDialogs[i];
		pDialog->OnResetDataObjs();
	}
	CDocumentSession* pSession = (CDocumentSession*)AfxGetThreadContext()->m_pDocSession;
	if (pSession && AfxIsRemoteInterface())
	{
		pSession->PushDataToClients(this);
		pSession->PushButtonsStateToClients(GetFrame()->m_hWnd);
	}
}

// Itera nelle view (ed alle tabdialog al loro interno) per frammentare localmente
// la preparazione di dati ausiliari (tipicamente HotLink e simili).
// La prima chiamata fatta durante la costruzione  del documento non deve iterare nelle
// view e nelle tabdialog perche' provvederanno localmente le stesse in fase di 
// costruzione ed inizializzazione
//------------------------------------------------------------------------------
BOOL CAbstractFormDoc::DispatchPrepareAuxData(BOOL bOnlyDocument/* = FALSE*/)
{
	CPerformanceCrono aPerfCrono;
	aPerfCrono.Start();
	if (!(OnPrepareAuxData() && m_pClientDocs->OnPrepareAuxData()))
		return FALSE;
	aPerfCrono.Stop();
	//TRACE(cwsprintf(cwsprintf(_T("Document OnPrepareAuxData  %s \n\r"), aPerfCrono.GetFormattedElapsedTime())));
	for (int i = 0; i < m_arWebActiveContainers.GetCount(); i++)
	{
		DWORD id = m_arWebActiveContainers[i];
		OnPrepareAuxData(id);
		m_pClientDocs->OnPrepareAuxData(id);
		OnUpdateTitle(id);
	}
	FireBehaviour(bhe_OnPrepareAuxData, TRUE);

	if (bOnlyDocument)
		return TRUE;

	POSITION pos = GetFirstViewPosition();
	while (pos != NULL)
	{
		CView* pView = GetNextView(pos);
		ASSERT_VALID(pView);

		if (pView->IsKindOf(RUNTIME_CLASS(CAbstractFormView)))
		{
			if (!((CAbstractFormView*)pView)->DispatchPrepareAuxData())
				return FALSE;
		}
	}

	//Codice della prepareauxdata di tiledlg /grp che deve essere spostata a livello di documento per ottimizzazioni magic link
	if (GetImportExportParams() && GetImportExportParams()->IsOnlyBusinessObject())
		ImportExportPrepareAuxData();

	return TRUE;
}

//Questo metodo deve essere reimplementato solo per le prepareauxdata di oggetti grafici come tiledlg che non sono state completamente purgate da codice di documento.
//------------------------------------------------------------------------------
BOOL CAbstractFormDoc::ImportExportPrepareAuxData()
{
	return TRUE;
}

// replace standard call. Must ignore all parameter also in OnSaveDocument.
//------------------------------------------------------------------------------
BOOL CAbstractFormDoc::DoSave(LPCTSTR /*pszPathName*/, BOOL /*bReplace = TRUE*/)
{
	DispatchOnBeforeSave();
	BOOL bOk = OnSaveDocument(NULL);
	DispatchOnAfterSave();
	return bOk;
}

//-----------------------------------------------------------------------------
BOOL CAbstractFormDoc::CanLoadXMLDescription()
{
	if (!GetXmlDescription())
		return FALSE;

	//optimization, so the function reads the file system only the first time that called by.
	if (m_bCheckExistXMLDocInfo)
		return m_bExistXMLDocInfo;

	m_bCheckExistXMLDocInfo = TRUE;
	if (
		!m_bDisableLoadXMLDocInfo && GetType() != VMT_FINDER &&
		ExistFile(AfxGetPathFinder()->GetDocumentDocumentFullName(GetInfoOSL()->m_Namespace)) &&
		ExistFile(AfxGetPathFinder()->GetDocumentDbtsFullName(GetInfoOSL()->m_Namespace))
		)
		m_bExistXMLDocInfo = TRUE;

	return m_bExistXMLDocInfo;
}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::LoadXMLDescription()
{
	if (m_pXMLDocInfo && m_pXMLDocInfo->IsLoaded())
		return;

	if (!m_pXMLDocInfo)
		m_pXMLDocInfo = new CXMLDocInfo(GetNamespace());

	if (m_pXMLDocInfo && !m_pXMLDocInfo->LoadAllFiles())
	{
		TRACE("CAbstractFormDoc::InitDocument: error during document %s file xml loading\n", GetRuntimeClass()->m_lpszClassName);

		delete m_pXMLDocInfo;
		m_pXMLDocInfo = NULL;
		return;
	}

	//per il caricamento ONDEMAND (devo associare le XMLInfo ai DBT)
	if (m_pDBTMaster)
		m_pDBTMaster->LoadXMLDBTInfo();
}

// In caso di errori in fase di inizializzazione presenta gli  stessi attraverso
// l'apposito gestore e poi lo pulisce per renderlo riutilizzabile per altra 
// eventuale diagnostica
//
// Inoltre da una chance di controllare tutti i files preposti alla transazione
// (DBT, HotLink e altri archivi correlati alla transazione
//-----------------------------------------------------------------------------
BOOL CAbstractFormDoc::InitDocument()
{
	DeclareVariable(_T("_DEBUG"), new DataBool(
#ifdef DEBUG
		TRUE
#else
		FALSE
#endif
	), TRUE);

	DeclareVariable(_T("FormMode"), &m_FormMode);
	DeclareVariable(_T("BatchMode"), &m_bBatch);
	//sospendo la comunicazione al client finch� il documento non � pronto, per evitare messaggi inutili
	//dar� poi il via libera nella CAbstractFormDoc::OnFrameCreated()
	CDocumentSession* pSession = (CDocumentSession*)AfxGetThreadContext()->m_pDocSession;
	if (pSession && AfxIsRemoteInterface())
	{
		pSession->SuspendPushToClient();
		pSession->IgnoreModelChanges(true);//finch� non ho completato l'apertura del documento � inutile registrare i cambiamenti del model
		pSession->AddJsonModelProvider(this);
	}
	//permetto al programmatore di poter istanziare gli oggetti che hanno bisogno del contesto
	if (!OnInitDocument())
		return FALSE;

	if (GetType() == VMT_BATCH || GetType() == VMT_FINDER)
	{
		ASSERT(GetDocTemplate() && GetDocTemplate()->IsKindOf(RUNTIME_CLASS(CSingleExtDocTemplate)));
		GetInfoOSL()->SetType(GetType() == VMT_BATCH ? OSLType_BatchTemplate : OSLType_FinderDoc);
	}

	//load xml document description files only if the document is dynamic or the application runs in unattended mode (MagicLink, MagicDoc, Scheduler)
	//otherwise loading is ondemand
	if ((::AfxIsInUnattendedMode()) && CanLoadXMLDescription())
		LoadXMLDescription();

	// se il documento � un ADM devo controllare se alla sua parte sinistra di derivazione
	// (il documento e non l'ADMObj) sia agganciato qualche documento	
	AfxGetBaseApp()->AttachClientDocs(this, m_pClientDocs);

	//CScriptClientDoc::AttachAutoClientDoc (this);

	//se sono abilitato all'eventuale attach a delle family client doc
	int nSize = m_pClientDocs->GetSize();
	//se sono abilitato all'eventuale attach a delle family client doc
	if (!m_bNoFamilyClientDoc)
		AfxGetBaseApp()->AttachFamilyClientDocs(this, m_pClientDocs);
	else
		//tabullo per gestire i BSP (che non permettono l'aggancio di family ClientDoc) di ERP in TestManager
		if (AfxIsActivated(TESTMANAGER_APP, _NS_ACT("TBMacroRecorder")))
		{
			AfxGetBaseApp()->AttachFamilyClientDocs(this, m_pClientDocs);
			for (int i = m_pClientDocs->GetUpperBound(); i >= nSize; i--)
			{
				if (CString(m_pClientDocs->GetAt(i)->GetRuntimeClass()->m_lpszClassName).CompareNoCase(_T("CDMacroRecorder")))
					m_pClientDocs->RemoveAt(i);
			}
		}

	// il clientdoc del TestManager CDMacroRecord deve essere in prima posizione
	if (AfxIsActivated(TESTMANAGER_APP, _NS_ACT("TBMacroRecorder")))
	{
		for (int i = 0; i <= m_pClientDocs->GetUpperBound(); i++)
		{
			if (!CString(m_pClientDocs->GetAt(i)->GetRuntimeClass()->m_lpszClassName).CompareNoCase(_T("CDMacroRecorder")))
			{
				if (i != 0)
				{
					//BAUZI: trucco molto ma molto sporco per posizionare il clientdocument del TestManager in prima posizione
					//per intercettare prima degli altri clientdoc i messaggi provenienti dal documento server
					CClientDoc* pCD = m_pClientDocs->GetAt(i);
					m_pClientDocs->SetOwns(FALSE);
					m_pClientDocs->RemoveAt(i);
					m_pClientDocs->InsertAt(0, pCD);
					m_pClientDocs->SetOwns(TRUE);
					break;
				}
			}
		}
	}

	// Due clientdoc della TBExtensions devonoo essere in ultima posizione
	// EasyBuilder e CDNotification del TbDataSynchroClient
	// Se sono presenti entrambi allora EasyBuilder sar� in penultima posizione e CDNotification in ultima
	// questo perch� CDNotification prima di notificare al DataSynchronizer(notifica che avviene nella OnExtraTransaction) 
	// deve aspettare le eventauali modifiche effettuate nei dati del documento dalle personalizzazioni di EasyBuilder che possono avvenire nell'OnExtraTransaction
	CClientDoc* pEBClientDoc = NULL;
	CClientDoc* pDMSClientDoc = NULL;
	CClientDoc* pDSClientDoc = NULL;

	m_pClientDocs->SetOwns(FALSE);

	for (int i = m_pClientDocs->GetUpperBound(); i >= 0; i--)
	{
		CClientDoc* pClientDoc = m_pClientDocs->GetAt(i);

		// se trovo il DMS lo sposto dopo EasyStudio perche' si interfaccia con le strutture del documento
		// e deve lavorare dopo che i documento di EasyStudio si sono formati bene
		if (strcmp(pClientDoc->GetRuntimeClass()->m_lpszClassName, "CDDMS") == 0)
		{
			pDMSClientDoc = pClientDoc;
			m_pClientDocs->RemoveAt(i);
			continue;
		}

		if (strcmp(pClientDoc->GetRuntimeClass()->m_lpszClassName, "CDEasyBuilder") == 0)
		{
			pEBClientDoc = pClientDoc;
			m_pClientDocs->RemoveAt(i);
			continue;
		}

		if (strcmp(pClientDoc->GetRuntimeClass()->m_lpszClassName, "CDNotification") == 0)
		{
			pDSClientDoc = pClientDoc;
			m_pClientDocs->RemoveAt(i);
			continue;
		}
	}

	m_pClientDocs->SetOwns(TRUE);

	if (pEBClientDoc)
		m_pClientDocs->Add(pEBClientDoc);

	if (pDMSClientDoc)
		m_pClientDocs->Add(pDMSClientDoc);

	if (pDSClientDoc)
		m_pClientDocs->Add(pDSClientDoc);

	m_pClientDocs->Attach(this);
	//--------------------------

	m_pClientDocs->OnInitDocument();

	if (!
		(
			OnExistTables() && m_pClientDocs->OnExistTables() &&
			OnAttachData() && (m_bOnAttachClient || (!m_bOnAttachClient && m_pClientDocs->OnAttachData())) && // per le procedure batch in cui non viene fatto l'Attach del dbtmaster
			DispatchOnAfterAttachData() &&
			!m_pMessages->ErrorFound()
			)
		)
	{
		m_pMessages->Show(TRUE);
		return FALSE;
	}

	DECLARE_VAR(_T("HeaderStripTitle"), m_pHeaderTitle);

	//se il documento proviene da un contesto dinamico, allora il suo titolo deve essere preso dalla descrizione XML
	//e non dall'oggetto c++ (ho fatto 'save as' da easybuilder)
	if (GetNamespace().HasAFakeLibrary())
	{
		const CDocumentDescription* pDescri = GetXmlDescription();
		if (pDescri)
			SetFormTitle(pDescri->GetTitle());
	}

	CAbstractFormView* pView = dynamic_cast<CAbstractFormView*>(this->GetFirstView());
	if (pView)
		m_pHeaderTitle->Assign(pView->GetCaption());

	if (m_bSingleUserMode && !m_pTbContext->LockDocument())
	{
		m_bLockedDocument = TRUE;
		m_pMessages->Show(TRUE);
		return FALSE;
	}

	CMasterFrame* pMasterFrame = GetMasterFrame();

	if (pMasterFrame && !pMasterFrame->IsKindOf(RUNTIME_CLASS(ADMFrame)))
	{
		CTBTabbedToolbar* pTabbedToolBar = pMasterFrame->GetTabbedToolBar();
		BOOL bSuspend = FALSE;
		if (pTabbedToolBar)
			bSuspend = pTabbedToolBar->SuspendLayout();

		CustomizeClientDoc();

		if (pTabbedToolBar && bSuspend)
			pTabbedToolBar->ResumeLayout();
	}

	// Se il programmatore si dimenti di assegare un nome si forza l'utilizzo
	// della runtimeclass del documento istanziato. Viene pero` data una warning
	if (m_strFormName.IsEmpty())
	{
		//Lo vogliamo questo controllo ?
		//ASSERT_TRACE(FALSE, _T("Manca la definizione del nome della form: usare SetFormName()"));

		USES_CONVERSION;
		SetFormName(A2T((LPSTR)GetRuntimeClass()->m_lpszClassName));
	}

	if (m_pFormManager->m_Ns.IsEmpty())
		m_pFormManager->m_Ns.SetNamespace(m_pFormManager->m_pDocument->GetNamespace());

	// se � un ADM(non interattivo) leggo il meno possibile
	if (GetMasterFrame() && !GetMasterFrame()->IsKindOf(RUNTIME_CLASS(ADMFrame)))
	{
		m_pFormManager->Parse();
	}
	else
		m_pFormManager->m_Reports.Parse(m_pFormManager->m_Ns);	//GetDefaultReport()

	// Serve per inizializzare correttamente lo stato dei DataObj.
	if (!BatchEnableControls())
		EnableAllControlLinks(FALSE);

	//carica gli eventi di importazione associati al documento e ai suoi client docs
	if (m_pEventManager)
		m_pEventManager->Initialize();

#pragma warning( push )
#pragma warning( disable: 4996 )	
	//riempie l'array delle xmlvariables dichiate attraverso la declare_vars
	OnDeclareVariables();
#pragma warning( pop )

	AfxGetLoginContext()->CopyVariables(this);
	// Parsing operations are made on document thread diagnostic
	if (AfxGetDiagnostic()->MessageFound())
	{
		m_pMessages->Copy(AfxGetDiagnostic(), 1, _TB("Document Loading Messages"));
		AfxGetDiagnostic()->ClearMessages(TRUE);
	}

	m_pMessages->Show(TRUE);

	if (pMasterFrame)
		pMasterFrame->SetFrameIcon();


	return TRUE;
}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::OnFrameCreated()
{
	__super::OnFrameCreated();
	// Set Document tile in ToolBar - Toolbar in style Infinity
	if (AfxGetThemeManager()->IsToolbarInfinity())
	{
		CString docTitle = GetTitle();
		CMasterFrame* pFrame = GetMasterFrame();
		CTBTabbedToolbar* pTabbedBar = pFrame ? pFrame->GetTabbedToolBar() : NULL;
		if (pTabbedBar)
		{
			pTabbedBar->AddDocumentTitle(docTitle);
		}
	}
	//avevo fatto la suspend nella CAbstractFormDoc::InitDocument, adesso do il via libera
	CDocumentSession* pSession = (CDocumentSession*)AfxGetThreadContext()->m_pDocSession;
	if (pSession && AfxIsRemoteInterface())
	{
		pSession->IgnoreModelChanges(false);//da adesso in avanti posso registrare i cambiamenti del model
		pSession->ResumePushToClient();
	}
}

//-----------------------------------------------------------------------------
SqlConnection* CAbstractFormDoc::GetDefaultSqlConnection()
{
	return AfxGetDefaultSqlConnection();
}

// si comporta come una OnNewDocument senza chiamare il browser
//-----------------------------------------------------------------------------
BOOL CAbstractFormDoc::OnOpenDocument(LPCTSTR pObject)
{
	AfxGetDefaultSqlConnection()->SetAlwaysConnected(false);

	if (!CAbstractDoc::OnOpenDocument(pObject))
		return FALSE;

	// potrei non partire con NULL e ricevere le informazioni dal mondo
	// esterno. per compatibilita con MFC devo passare attraverso un LPCTSTR
	m_pDocInvocationInfo = (DocInvocationInfo*)pObject;
	m_pExternalControllerInfo = m_pDocInvocationInfo ? m_pDocInvocationInfo->m_pControllerInfo : NULL;

	if (GetMyAncestor())
	{
		m_pTbContext = new CTBContext(*GetMyAncestor()->m_pTbContext);
		TRACE_SQL(cwsprintf(_T("TBContext create a new CTBContext %lp for document %lp with runtimeclass %s using context %lp of the ancestor %lp with runtimeclass %s"),
			m_pTbContext, this, CString(GetRuntimeClass()->m_lpszClassName), GetMyAncestor()->m_pTbContext, GetMyAncestor(), CString(GetMyAncestor()->m_pTbContext->GetDocument()->GetRuntimeClass()->m_lpszClassName)),
			m_pTbContext->GetUpdatableSqlSession());
		//sicuramente e` una CMessages perche' istanziata dall'ancestor
		//(vedi altro ramo dell'if)
		m_pMessages = (CMessages*)m_pTbContext->GetDiagnostic();
	}
	else
	{
		if (GET_CONTEXT(m_pDocInvocationInfo))
		{
			m_pTbContext = new CTBContext(*GET_CONTEXT(m_pDocInvocationInfo));
			TRACE_SQL(cwsprintf(_T("TBContext create a new CTBContext %lp for document %lp with runtimeclass %s using context %lp from DocInvocationInfo %lp"),
				m_pTbContext, this, GetRuntimeClass()->m_lpszClassName, GET_CONTEXT(m_pDocInvocationInfo), m_pDocInvocationInfo),
				m_pTbContext->GetUpdatableSqlSession());

			m_pMessages = (CMessages*)m_pTbContext->GetDiagnostic();
		}
		else
		{
			//da istanziare prima del tbcontext, che lo utilizza
			//(prima era il tbcontext a crearlo)
			m_pMessages = new CMessages();
			m_pMessages->m_pDocument = this;
			m_pMessages->SetStartingBanner(GetOpeningBanner());
			m_pTbContext = new CTBContext(GetDefaultSqlConnection(), this);	// da mettere come connection di documento		
			TRACE_SQL(cwsprintf(_T("TBContext create a new CTBContext %lp for document %lp with runtimeclass %s"),
				m_pTbContext, this, CString(GetRuntimeClass()->m_lpszClassName)),
				m_pTbContext->GetUpdatableSqlSession());


			if (m_pTbContext->GetSqlConnection()->GetUsePerformanceMng())
			{
				SqlPerformanceDlg* pPerformanceDlg = new SqlPerformanceDlg(m_pTbContext);
			}
		}
	}

	//ContextBag
	//ne faccio un costruttore in copia. Questo per permettere a chi eredita il contextbag di modificarne i valori
	m_pContextBag = GET_CONTEXTBAG(m_pDocInvocationInfo) ? new CContextBag(*GET_CONTEXTBAG(m_pDocInvocationInfo)) : new CContextBag();

	m_pSqlConnection = m_pTbContext->GetSqlConnection();

	ConnectToDatabase(szOpenAction);

	//@@BAUZI
	if ((GetMyAncestor() || GET_CONTEXT(m_pDocInvocationInfo)) && m_bUnattendedMode)
		m_pTbContext->GetDiagnostic()->StartSession(GetOpeningBanner());

	// Suppone che tutto vada a buon fine.
	m_bAborted = FALSE;

	// time consuming funtion. Use waiting cursor
	BeginWaitCursor();

	// create a new document only if datasource is valid and tables exists
	// e ci sono abbastanza risorse GDI,Heap,System disponibili
	if (
		!m_pTbContext ||
		!m_pTbContext->IsValid() ||
		!ResourceAvailable()
		)
	{
		m_bAborted = TRUE;
		goto endopen;
	}

	// implementation dependent initialization
	if (!CAbstractDoc::OnNewDocument() || !InitDocument())
	{
		if (m_pMessages->IsUnattendedMode())
			m_pMessages->Add(cwsprintf(_TB("Unable to create the document {0-%s}."), GetTitle()));
		else
			AfxMessageBox(_TB("Unable to create the document."));
		m_bAborted = TRUE;
		goto endopen;
	}

	if (!m_bAborted)
	{
		// Carica la tabella di acceleratori specifica al documento
		// se e' stata definita dal programmatore con la SetDocAccel
		LoadDocAccel();

		// Crea il browser ma fa la query solo se l'opzione specifica di .ini
		// e` abilitata oppure se e` ammessa la creazione di un solo record
		// in modo da farlo vedere subito e non indurre in errore pensando che
		// il documento sia vuoto.
		m_bAborted = !CreateBrowser(pObject);
	}

	LoadSymbolTable();

	// start unmodified
	SetModifiedFlag(FALSE);

endopen:
	EndWaitCursor();

	DisconnectFromDatabase();
	return !m_bAborted;
}

//-----------------------------------------------------------------------------
BOOL CAbstractFormDoc::OnNewDocument()
{
	UserInteractionMode oldMode;
	CThreadContext* pThread = NULL;
	if (AfxIsRemoteInterface())
	{
		//in modalità web non devono esserci MessageBox in fase di apertura del documento
		pThread = AfxGetThreadContext();
		oldMode = pThread->GetUserInteractionMode();
		pThread->SetUserInteractionMode(UNATTENDED);
	
	}
	BOOL b = CanRunDocument();

	if (pThread)
	{
		pThread->SetUserInteractionMode(oldMode);
	}
	return b && CAbstractFormDoc::OnOpenDocument(NULL);
}

//-----------------------------------------------------------------------------
BOOL CAbstractFormDoc::CreateBrowser(LPCTSTR pObject/* = NULL*/)
{
	BOOL bOk = TRUE;
	ASSERT(m_pBrowser == NULL);
	if (m_pBrowser)	return bOk;

	// In Batch Mode (vedi differita) e nel caso di virtual record il browser non serve a nulla
	if (GetType() != VMT_BATCH && m_pDBTMaster && m_pDBTMaster->GetRecord()->GetType() != VIRTUAL_TYPE)
	{
		SUSPEND_UPDATE_DATA_VIEW();
		// permette al programmatore di valorizzare eventuali variabili di
		// documento che servono per definire la query predefinita del browser
		if (pObject) bOk = OnCreateBrowser(GET_AUXINFO(pObject));

		m_pBrowser = new SqlBrowser(m_pDBTMaster);

		BOOL bDirectBrowse = *(DataBool*)AfxGetSettingValue(snsTbGenlib, szFormsSection, szImmediateBrowsing, DataBool(FALSE), szTbDefaultSettingFileName);

		m_pQueryManager->Attach(m_pBrowser->m_pTable, FALSE, FALSE, FALSE);

		if (
			bOk &&
			(
				m_bBrowseOnCreate ||	// forzato dal programmatore
				m_bOnlyOneRecord ||	// faccio subito vedere
				bDirectBrowse
				) &&
			m_pBrowser && m_pBrowser->Query()
			)
		{
			BOOL bOKQuery = (m_bBrowseOnFirstRecord ? m_pBrowser->FirstRecord() : m_pBrowser->LastRecord());

			if (bOKQuery)
			{
				BrowseRecord(FALSE, TRUE);
				return bOk;
			}
		}

		InitData();
		ClearCurrentRecord();
		UpdateDataView();
	}

	return bOk;
}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::ReInitBrowser()
{
	// In Batch Mode (vedi differita) il browser non serve a nulla
	if (GetType() != VMT_BATCH && m_pDBTMaster && !m_pDBTMaster->GetRecord()->IsVirtual())
	{
		SUSPEND_UPDATE_DATA_VIEW();

		m_pBrowser->ReInit();

		m_pQueryManager->Attach(m_pBrowser->m_pTable, FALSE, FALSE);

		BOOL bDirectBrowse = *(DataBool*)AfxGetSettingValue(snsTbGenlib, szFormsSection, szImmediateBrowsing, DataBool(FALSE), szTbDefaultSettingFileName);

		if (
			(
				m_bBrowseOnCreate ||	// forzato dal programmatore
				m_bOnlyOneRecord ||	// faccio subito vedere
				bDirectBrowse
				) &&
			m_pBrowser && m_pBrowser->Query()
			)
		{
			BOOL bOKQuery = (m_bBrowseOnFirstRecord ? m_pBrowser->FirstRecord() : m_pBrowser->LastRecord());

			if (bOKQuery)
			{
				BrowseRecord(FALSE, TRUE);
				return;
			}
		}

		InitData();
		ClearCurrentRecord();
		UpdateDataView();
	}
}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::OnCloseDocument()
{
	GetNotValidView(FALSE);

	if (m_pClientDocs)
		m_pClientDocs->OnBeforeCloseDocument();

	if (m_pRadarDoc)
	{
		// MUST detach before close radar to avoid ACK-NACK comunication
		m_pRadarDoc->Detach();
		m_pRadarDoc->CloseRadar();

		// chiusura del radar woorm su chiusura del documento
		m_pRadarDoc->GetDocument()->CloseDocument();
	}

	CTBTabbedToolbar* pTabbedBar = GetMasterFrame() ? GetMasterFrame()->GetTabbedToolBar() : NULL;
	if (pTabbedBar)
	{
		pTabbedBar->ClosePopupMenu();
	}

	if (m_arReportDoc.GetSize())
	{
		BOOL bStop = FALSE;
		for (int i = 0; i < m_arReportDoc.GetSize(); i++)
		{
			WoormDocPtr* pReportDoc = dynamic_cast<WoormDocPtr*>(m_arReportDoc[i]);
			if (!pReportDoc || !(*pReportDoc))
				continue;

			if ((*pReportDoc)->SaveModified())
			{
				(*pReportDoc)->OnCloseDocument();
				//NO m_arReportDoc[i] = NULL;
			}
			else
				bStop = TRUE;
		}
		if (bStop)
			return;
		//NO m_arReportDoc.RemoveAll();
	}

	ReturnDataToCallLink(FALSE, FALSE);

	//-------------------------------------------------------------------
	//operazioni di pulizia del client doc PRIMA che venga distrutto tutto
	if (m_pClientDocs)
		m_pClientDocs->OnCloseServerDocument();
	CDocumentSession* pSession = (CDocumentSession*)AfxGetThreadContext()->m_pDocSession;
	if (pSession && AfxIsRemoteInterface())
		pSession->RemoveJsonModelProvider(this);

	//PERASSO: spostato qui dal distruttore, perch� il composed hotlink possiede il puntatore alla variabile selettore che, quando siamo nel distruttore del documento, potrebbe essere stata distrutta 
	//invece a me serve per poter effettuare il detach degli eventi di changed
	ASSERT(m_pHotKeyLinks);
	SAFE_DELETE(m_pHotKeyLinks);

	__super::OnCloseDocument();
	OnCloseBehaviours();
}

//------------------------------------------------------------------------------
BOOL CAbstractFormDoc::CloseDocument(BOOL bAsync)
{
	//questa prima non c'era, l'abbiamo aggiunta per verticali c#
	__super::CloseDocument(bAsync);

	return TRUE;
}

//------------------------------------------------------------------------------
void CAbstractFormDoc::SaveCurrentRecord()
{
	BOOL bEqual = FALSE;
	if (m_pDBTMaster)
	{
		// clona il record del master
		if (m_pCurrentRec == NULL)
			m_pCurrentRec = m_pDBTMaster->m_pRecord->Create();

		bEqual = m_pCurrentRec->IsEqual(*m_pDBTMaster->m_pRecord);
		// salva il dato per eventuali refetch
		*m_pCurrentRec = *(m_pDBTMaster->m_pRecord);
	}

	if (!bEqual) //avviso i clientdoc solo se � effettivamente cambiato il current record
		m_pClientDocs->OnSaveCurrentRecord();
}

//------------------------------------------------------------------------------
void CAbstractFormDoc::ClearCurrentRecord()
{
	SAFE_DELETE(m_pCurrentRec);
}

//------------------------------------------------------------------------------
BOOL CAbstractFormDoc::ValidCurrentRecord()
{
	return m_pCurrentRec != NULL;
}

//------------------------------------------------------------------------------
void CAbstractFormDoc::RefreshCurrentRecord()
{
	if (m_pCurrentRec)
	{
		BOOL bEqual = m_pCurrentRec->IsEqual(*m_pDBTMaster->m_pRecord);
		*(m_pDBTMaster->m_pRecord) = *m_pCurrentRec;

		if (!bEqual) //avviso i clientdoc solo se � effettivamente cambiato il current record
			m_pClientDocs->OnSaveCurrentRecord();
	}
}

// Mi aspetto solo una view
//------------------------------------------------------------------------------
BOOL CAbstractFormDoc::SetControlAutomaticExpression(UINT nID, const CString& strExp)
{
	if (GetType() != VMT_BATCH)
		return FALSE;

	POSITION pos = GetFirstViewPosition();
	ASSERT(pos);

	CView* pView = GetNextView(pos);
	ASSERT_VALID(pView);

	if (pView != NULL && pView->IsKindOf(RUNTIME_CLASS(CAbstractFormView)))
		return (((CAbstractFormView*)pView)->SetControlAutomaticExpression(nID, strExp));

	return FALSE;
}

// Mi aspetto solo una view
//------------------------------------------------------------------------------
BOOL CAbstractFormDoc::GetControlAutomaticExpression(UINT& nID, CString& strExp)
{
	if (GetType() != VMT_BATCH)
		return FALSE;

	POSITION pos = GetFirstViewPosition();
	ASSERT(pos);

	CView* pView = GetNextView(pos);
	ASSERT_VALID(pView);

	if (pView != NULL && pView->IsKindOf(RUNTIME_CLASS(CAbstractFormView)))
		return (((CAbstractFormView*)pView)->GetControlAutomaticExpression(nID, strExp));

	return FALSE;
}

//------------------------------------------------------------------------------
BOOL CAbstractFormDoc::DispatchFunctionCall(CFunctionDescription* pRDI)
{
	BOOL bOk = FireAction(pRDI->GetName(), pRDI) == CEventManager::FUNCTION_OK;
	if (bOk && !pRDI->GetReturnValue()->IsValid())
	{
		TRACE(_T("CAbstractFormDoc::DispatchFunctionCall: probably pRDI->SetReturnValue() couldn't exist for %s function\n"), pRDI->GetNamespace().ToString());
		ASSERT(FALSE);
	}

	return bOk;
}

//------------------------------------------------------------------------------
void CAbstractFormDoc::CustomizeClientDoc()
{
	// mi memorizzo il contesto del documento prima delle chiamate ai clientdoc
	for (int i = 0; i <= m_pClientDocs->GetUpperBound(); i++)
	{
		// � possibile che nella customize ci siano delle modifiche al contesto
		m_pClientDocs->GetAt(i)->Customize();
	}
	OnAfterCustomizeClientDocs();
}

////////////////////////////////////////////////////////////////////////////////
// Start of DBT related functions
//

void CAbstractFormDoc::AssignHklRecordToMaster()
{
	ASSERT_VALID(m_pHLRecord);
	ASSERT_VALID(m_pDBTMaster->m_pRecord);

	ASSERT(
		m_pDBTMaster->m_pRecord->GetRuntimeClass()->IsDerivedFrom(m_pHLRecord->GetRuntimeClass())
		||
		m_pHLRecord->GetRuntimeClass()->IsDerivedFrom(m_pDBTMaster->m_pRecord->GetRuntimeClass())
	);

	*(m_pDBTMaster->m_pRecord) = *m_pHLRecord;
}

//------------------------------------------------------------------------------
void CAbstractFormDoc::AssignMasterRecordToHkl()
{
	ASSERT_VALID(m_pHLRecord);
	ASSERT_VALID(m_pDBTMaster->m_pRecord);

	ASSERT(
		m_pDBTMaster->m_pRecord->GetRuntimeClass()->IsDerivedFrom(m_pHLRecord->GetRuntimeClass())
		||
		m_pHLRecord->GetRuntimeClass()->IsDerivedFrom(m_pDBTMaster->m_pRecord->GetRuntimeClass())
	);

	*m_pHLRecord = *(m_pDBTMaster->m_pRecord);
}

//------------------------------------------------------------------------------
void CAbstractFormDoc::InitData()
{
	SetAssignedCounter(NULL);

	if (m_pDBTMaster)
	{
		// initialize record(s)
		m_pDBTMaster->Init();

		// se sono una form connessa attraverso la ConnectForm il master record e' 
		// valorizzato da fuori e pertanto dopo la corretta inizializzazione lo
		// devo aggiornare con quanto passatomi dallo HotLink
		if (m_pHLRecord)
		{
			AssignHklRecordToMaster();
			SaveCurrentRecord();
		}
	}

	// Da alla implementazione finale la chance di initializzare dati ausiliari
	// inclusi eventuali documenti ausiliari degli add-on 
	OnInitAuxData() && m_pClientDocs->OnInitAuxData();
	FireBehaviour(bhe_OnInitAuxData, TRUE);

	InitViewsFormFlags();
}

//------------------------------------------------------------------------------
CBaseDocument::FormMode CAbstractFormDoc::SetFormMode(FormMode aFormMode)
{
	FormMode fmOldFormMode = CBaseDocument::SetFormMode(aFormMode);

	if (aFormMode == NEW || aFormMode == FIND)
		InitData();

	EnableControls();
	OnAfterSetFormMode(fmOldFormMode);
	m_pClientDocs->OnAfterSetFormMode(fmOldFormMode);

	return fmOldFormMode;
}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::OnAfterSetFormMode(FormMode oldFormMode)
{
	FireBehaviour(bhe_FormModeChanged);
}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::DeleteContents()
{
	if (m_pDBTMaster)
		m_pDBTMaster->Close();

	SAFE_DELETE(m_pDBTMaster);
	SAFE_DELETE(m_pBrowser);
	SAFE_DELETE(m_pCurrentRec);
	//E' compito del documento derivato che lo ha istanziato di deletarlo. Viene condiviso in catene di chiamata
	//SAFE_DELETE(m_pWoormInfo);
}

//-----------------------------------------------------------------------------
BOOL CAbstractFormDoc::LockMaster(BOOL bUseMessageBox)
{
	//@@OPTIMISTICLOCK
	if (AfxGetOleDbMng()->UseOptimisticLock())
		return TRUE;

	if (!m_pDBTMaster || !AfxIsValidAddress(m_pDBTMaster, sizeof(DBTMaster)))
		return FALSE;

	SqlTable* pTable = m_pDBTMaster->m_pTable;

	// prova a bloccare l'acceso al documento da editare (OCCHIO! solo su master)
	if (!pTable->LockCurrent(bUseMessageBox))
	{
		if (!m_bRetryingLock)
		{
			// in batch mode (vedi ADM) devo essere il piu` possibile silente
			if (!bUseMessageBox)
				m_pMessages->Add(pTable->GetLockMessage());

			GoInBrowseMode();
		}
		return FALSE;
	}
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CAbstractFormDoc::LockDocument(BOOL bDelete)
{
	while (TRUE)
	{
		LockStatus lck = ALL_LOCKED;
		// il progammatore fa il lock di tutti i dati ausiliari nel documento
		// server (il corrente) e in tutti gli eventuali client document, se esistono
		switch (GetFormMode())
		{
		case NEW:
			lck = FireBehaviour(bhe_LockDocumentForNew) ? ALL_LOCKED : LOCK_FAILED;
			if (lck == ALL_LOCKED)
			{
				lck = OnLockDocumentForNew();
				if (lck == ALL_LOCKED)
					lck = m_pClientDocs->OnLockDocumentForNew();
			}
			break;

		case EDIT:
			lck = bDelete
				? OnLockDocumentForDelete()
				: OnLockDocumentForEdit();

			if (lck == ALL_LOCKED)
				lck = bDelete
				? m_pClientDocs->OnLockDocumentForDelete()
				: m_pClientDocs->OnLockDocumentForEdit();
			break;

		default:
			ASSERT(bDelete);
			lck = OnLockDocumentForDelete();
			if (lck == ALL_LOCKED)
				lck = m_pClientDocs->OnLockDocumentForDelete();
			break;
		}

		// informo il documento che potrebbero esserci attivi dei lock di
		// transazione accessoria (in fase di new non viene loccata la testa)
		// e pertanto devo abilitare la UnlockAll() che per ottimizzazione
		// lo fa' solo se ci sono lock attivi
		//m_bLocked = TRUE;

		// Documento bloccato correttamente. Posso proseguire
		if (lck == ALL_LOCKED)
			return TRUE;

		// Problemi di condivisione. Rilascio tutti i lock (se sono proprietario del contesto)
		// e chiedo cosa fare
		UnlockAll();

		// Relock del master. Se fallisce e' proprio una sfiga, devo quindi uscire.
		// Mago3 e' bucato. Non fa' il relock del Master (booo!!)
		if ((GetFormMode() == EDIT || bDelete) && !LockMaster())
		{
			Message(_TB("Warning! While waiting, another user has taken ownership of the current document."));
			break;
		}

		// In caso di dato mancante per la transazione secondaria (qualcuno lo
		// ha cancellato da qualche altra stazione) non mi metto a loppare per 
		// ritentare il lock, ma segnalo la cosa.
		if (lck == NO_AUX_DATA)
			break;

		// comportamento di default in caso non si possano visualizzare MessageBox: uscire
		if (IsInUnattendedMode())
		{
			m_pMessages->Add(_TB("Busy resources"));
			break;
		}

		if (AfxMessageBox(_TB("Busy resources. Try again?"), MB_RETRYCANCEL) == IDCANCEL)
			break;

		//@@ TODO.. Se mai un giorno ci sara' in windows, mettere l'istruzione sotto:
		// sleep();
	}
	return FALSE;
}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::UnlockAll()
{
	// rilascia il lock di contesto solo se � un proprio contesto e non derivato
	if (m_pTbContext)
		m_pTbContext->UnlockAll();
}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::GoInBrowseMode()
{
	{ //non togliere la graffa: scope per la UpdateDataView
		SUSPEND_UPDATE_DATA_VIEW();
		if (m_pDBTMaster)
			m_pDBTMaster->GoInBrowseMode();

		// i derivati e i documenti client possono fare quello che vogliono 
		// prima di andare in stato di Browse
		OnGoInBrowseMode();
		m_pClientDocs->OnGoInBrowseMode();

		UnlockAll();

		SetFormMode(BROWSE);	// back to BROWSE mode
		SetModifiedFlag(FALSE);	// back to unmodified

		UpdateDataView();
	}

	SetDefaultFocus();
	if (::GetFocus() == NULL)
		GetFrame()->SetFocus();

	ToolBarButtonsHideGhost(1);
}

// Salvo qualcosa solo se esiste la catena dei DBT a partire dal DBTMaster 
// ad esempio alcune operazioni batch (vedi ABC) non gestiscono un DBT
//-----------------------------------------------------------------------------
BOOL CAbstractFormDoc::OnSaveDocument(LPCTSTR /*pszPathName*/)
{
	CStatusBarMsg msg(true, true, false);
	BOOL bOk = TRUE;
	BOOL bPrimary = TRUE;
	BOOL bSecondary = TRUE;

	TRY
	{
		//mi connetto al database
		//m_pTbContext->Connect();

		switch (GetFormMode())
		{
			case NEW:
				// nel caso il record esiste gia' da una chance all'utente 
				// di rimediare cambiando, se possibile, la chiave primaria.
				if (m_pDBTMaster && m_pDBTMaster->Exist())
				{
					Message(cwsprintf
								(
									_TB("The document {0-%s} already exists.\r\nChange the reference data in order to save it.\r\n"),
									(LPCTSTR)m_pDBTMaster->GetRecord()->GetPrimaryKeyDescription()
								)
							);

					m_pTbContext->Rollback();
					bOk = FALSE;
					break;
				}

				// primary transaction			
				m_pTbContext->StartTime(PRIMARY_TIME);
				m_pTbContext->EnableOptimisticLock();
				bPrimary = !m_pDBTMaster || m_pDBTMaster->Update();
				m_pTbContext->EnableOptimisticLock(false);
				m_pTbContext->StopTime(PRIMARY_TIME);

				// secondary transaction
				m_pTbContext->StartTime(SECONDARY_TIME);
				bSecondary = m_pClientDocs->OnBeforeNewTransaction() && FireBehaviour(bhe_NewTransaction) && OnNewTransaction() && m_pClientDocs->OnNewTransaction();

				if (!(bPrimary && bSecondary))
				{
					m_pTbContext->Rollback();
					m_pTbContext->StopTime(SECONDARY_TIME);
					m_pMessages->Show(TRUE);
					if (m_pDBTMaster)
						m_pDBTMaster->AddNew(FALSE);
					bOk = FALSE;
					break;
				}

				m_pTbContext->Commit();
				m_pTbContext->StopTime(SECONDARY_TIME);

				SaveCurrentRecord(); //the record is valid after the primary transaction yet

				//auxiliary transaction
				m_pTbContext->StartTime(AUXILIARY_TIME);
				OnExtraNewTransaction();
				m_pClientDocs->OnExtraNewTransaction();
				m_pTbContext->StopTime(AUXILIARY_TIME);

				m_pMessages->Show(TRUE);

				msg.Show(_TB("New document added"));
				msg.Dispose();//permetto la corretta assegnazione del fuoco alla finestra (che viene riabilitata)
				GoInBrowseMode();

				// Se chiamato da CallLink restituisce il dato e poi muore	
				if (ReturnDataToCallLink())
				{
					break;
				}
				// devo invalidare la query del browser (questo solo se il browser ha come cursore
				// il KEYSET_CURSOR che non mi permette di vedere gli inserimenti degli altri cursori
				if (m_pBrowser)
					m_pBrowser->SetInvalidQuery();

				// E' il programmatore che permette inserimenti ripetitivi solo
				// se possono essere inseriti piu` di un record, e non sono
				// chiamato da una CallLink
				//
				if (m_bRepeatableNew && !m_bOnlyOneRecord)
				{
					PostMessage(WM_COMMAND, ID_EXTDOC_NEW, NULL);
					break;
				}
				break;


			case EDIT:
				// primary transaction
				m_pTbContext->StartTime(PRIMARY_TIME);
				m_pTbContext->EnableOptimisticLock();
				bPrimary = !m_pDBTMaster || m_pDBTMaster->Update();
				m_pTbContext->EnableOptimisticLock(false);
				m_pTbContext->StopTime(PRIMARY_TIME);

				// secondary transaction
				m_pTbContext->StartTime(SECONDARY_TIME);
				bSecondary = m_pClientDocs->OnBeforeEditTransaction() && FireBehaviour(bhe_EditTransaction) && OnEditTransaction() && m_pClientDocs->OnEditTransaction();
				if (!(bPrimary && bSecondary))
				{
					m_pTbContext->Rollback();
					m_pTbContext->StopTime(SECONDARY_TIME);
					BrowseRecord();
					msg.Dispose();//permetto la corretta assegnazione del fuoco alla finestra (che viene riabilitata)
					GoInBrowseMode();
					m_pMessages->Show(TRUE);
					bOk = FALSE;
				}
				else
				{
					m_pTbContext->Commit();
					m_pTbContext->StopTime(SECONDARY_TIME);
					SaveCurrentRecord(); //the record is valid after the primary transaction yet

					//auxiliary transaction
					m_pTbContext->StartTime(AUXILIARY_TIME);
					OnExtraEditTransaction();
					m_pClientDocs->OnExtraEditTransaction();
					m_pTbContext->StopTime(AUXILIARY_TIME);
					msg.Dispose();//permetto la corretta assegnazione del fuoco alla finestra (che viene riabilitata)
					GoInBrowseMode();
					m_pMessages->Show(TRUE);
					msg.Show(_TB("Document changes saved"));
				}
				break;

			default:
				msg.Dispose();//permetto la corretta assegnazione del fuoco alla finestra (che viene riabilitata)
				GoInBrowseMode();
				break;
		}
	//mi disconnetto al database
	//m_pTbContext->Disconnect();

	}
		CATCH(SqlException, e)
	{
		m_pTbContext->StopTime(PRIMARY_TIME);
		m_pTbContext->StopTime(SECONDARY_TIME);
		m_pTbContext->StopTime(AUXILIARY_TIME);
		m_pMessages->Add(cwsprintf(_TB("Unable to save the document due to the following exception: %s"), e->m_strError));
		m_pMessages->Show(TRUE);
		m_pTbContext->Rollback();
		UnlockAll();
		return FALSE;
	}
	END_CATCH

		UnlockAll();
	return bOk;
}

//-----------------------------------------------------------------------------
BOOL CAbstractFormDoc::OnCmdMsg(UINT nID, int nCode, void* pExtra, AFX_CMDHANDLERINFO* pHandlerInfo)
{
	if (!AfxGetThreadContext()->IsValidObject(this))
		return FALSE;

	if (nCode == CN_UPDATE_COMMAND_UI && IsImporting())
		return TRUE;

	BOOL bHandled = FALSE;
	BOOL bBadCommand = IsBadCmdMsg(nID);
	CClientDoc* pClientDoc = NULL;
	//ruoto il messaggio prima
	if (!bBadCommand)
	{
		for (int i = 0; i <= m_pClientDocs->GetUpperBound(); i++)
		{
			pClientDoc = m_pClientDocs->GetAt(i);
			if (pClientDoc->GetMsgRoutingMode() == CClientDoc::CD_MSG_BOTH ||
				pClientDoc->GetMsgRoutingMode() == CClientDoc::CD_MSG_BEFORE)
			{
				pClientDoc->SetMsgState(CClientDoc::ON_BEFORE_MSG);
				bHandled = pClientDoc->OnCmdMsg(nID, nCode, pExtra, pHandlerInfo) || bHandled;
			}
		}
	}

	bHandled = __super::OnCmdMsg(nID, nCode, pExtra, pHandlerInfo) || bHandled;

	//ruoto il messaggio dopo
	if (!bBadCommand && AfxGetThreadContext()->IsValidObject(this))			//removed the ExistDocument check because of performance issues
	{
		ASSERT_VALID(this);
		for (int i = 0; i <= m_pClientDocs->GetUpperBound(); i++)
		{
			pClientDoc = m_pClientDocs->GetAt(i);
			if (pClientDoc->GetMsgRoutingMode() == CClientDoc::CD_MSG_BOTH ||
				pClientDoc->GetMsgRoutingMode() == CClientDoc::CD_MSG_AFTER)
			{
				pClientDoc->SetMsgState(CClientDoc::ON_AFTER_MSG);
				bHandled = pClientDoc->OnCmdMsg(nID, nCode, pExtra, pHandlerInfo) || bHandled;
			}
		}
	}

	return bHandled;
}

//------------------------------------------------------------------------------
BOOL CAbstractFormDoc::DispatchOnBeforeBatchExecute()
{
	BOOL bOK = OnBeforeBatchExecute();

	return bOK && m_pClientDocs->OnBeforeBatchExecute();
}

//----------------------------------------------------------------------------------
void CAbstractFormDoc::DispatchOnAfterBatchExecute()
{
	OnAfterBatchExecute();
	m_pClientDocs->OnAfterBatchExecute();
}

//----------------------------------------------------------------------------------
void CAbstractFormDoc::ActivateWebClientContainer(UINT nID, bool bActivate)
{
	for (int i = 0; i < m_arWebActiveContainers.GetCount(); i++)
	{
		if (m_arWebActiveContainers[i] == nID)
		{
			if (!bActivate)
				m_arWebActiveContainers.RemoveAt(i);
			return;
		}
	}
	if (bActivate)
	{
		m_arWebActiveContainers.Add(nID);
		OnUpdateTitle(nID);
		OnPrepareAuxData(nID); 
	}
}

// Reimplementabili per la fantasia bacata del programmatore (non si sa' mai)
//-----------------------------------------------------------------------------
void CAbstractFormDoc::DisableControlsForBatch()
{
	/* default do nothing */
}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::DisableControlsForAddNew()
{
	if (m_pDBTMaster)
		m_pDBTMaster->DisableControlsForAddNew();
}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::DisableControlsForEdit()
{
	if (m_pDBTMaster)
		m_pDBTMaster->DisableControlsForEdit();
}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::EnableControlsForFind()
{
	if (m_pDBTMaster) m_pDBTMaster->EnableControlsForFind();
}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::DisableControlsAlways()
{
	if (m_pDBTMaster)
		m_pDBTMaster->DisableControlsAlways();
}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::OnInitializeUI(const CTBNamespace& aFormNs)
{
}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::OnInitializeUI(CParsedForm* pForm)
{
}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::OnDestroyTabDialog(CTabDialog* pTabDlg)
{
	if (m_pClientDocs) m_pClientDocs->OnDestroyTabDialog(pTabDlg);
}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::OnBuildDataControlLinks(CTabDialog* pTabDlg)
{
	if (m_pClientDocs) m_pClientDocs->OnBuildDataControlLinks(pTabDlg);
	if (m_pHotFilterManager) m_pHotFilterManager->OnBuildDataControlLinks(pTabDlg);

	// to initialize other objects after BuildDataCtrlLinks code
	OnInitializeUI(pTabDlg->GetNamespace());
	OnInitializeUI(pTabDlg);
	m_pClientDocs->OnInitializeUI(pTabDlg->GetNamespace());
}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::OnBuildDataControlLinks(CAbstractFormView* pView)
{
	if (m_pClientDocs) m_pClientDocs->OnBuildDataControlLinks(pView);
	if (m_pHotFilterManager) m_pHotFilterManager->OnBuildDataControlLinks(pView);

	// to initialize other objects after BuildDataCtrlLinks code
	OnInitializeUI(pView->GetNamespace());
	OnInitializeUI(pView);
	if (m_pClientDocs) m_pClientDocs->OnInitializeUI(pView->GetNamespace());
}

//-----------------------------------------------------------------------------
HotFilterManager* CAbstractFormDoc::GetHotFilterManager()
{
	if (!m_pHotFilterManager)
		m_pHotFilterManager = new HotFilterManager(this, FALSE, 0, _T("Filters"));

	return m_pHotFilterManager;
}

//---------------------------------------------------------------------------------
void CAbstractFormDoc::CompleteQuery(const CString strName, SqlTable* pTable, SqlRecord* pRec, const DataObj& aColumn)
{
	if (!m_pHotFilterManager)
	{
		ASSERT(FALSE);
		return;
	}

	m_pHotFilterManager->CompleteQuery(strName, pTable, pRec, aColumn);
}

//---------------------------------------------------------------------------------
void CAbstractFormDoc::CompleteQuery(const CString strName, SqlTable* pTable, const DataObj& aColumn)
{
	if (!m_pHotFilterManager)
	{
		ASSERT(FALSE);
		return;
	}

	m_pHotFilterManager->CompleteQuery(strName, pTable, pTable->GetRecord(), aColumn);
}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::OnBuildDataControlLinks(CTileDialog* pTile)
{
	m_pClientDocs->OnBuildDataControlLinks(pTile);

	// to initialize other objects after BuildDataCtrlLinks code
	OnInitializeUI(pTile->GetNamespace());
	OnInitializeUI((CParsedForm*)pTile);

	m_pClientDocs->OnInitializeUI(pTile->GetNamespace());
}
//-----------------------------------------------------------------------------
void CAbstractFormDoc::OnUpdateTitle(CTileDialog* pTileDlg)
{
	OnUpdateTitle(pTileDlg->GetDialogID());
	if (pTileDlg->IsKindOf(RUNTIME_CLASS(CJsonTileDialog)))
	{
		CJsonTileDialog* pJsonTile = ((CJsonTileDialog*)pTileDlg);
		CWndTileDescription* pDesc = (CWndTileDescription*)pJsonTile->GetJsonContext()->m_pDescription;
		pDesc->EvaluateExpressions(pJsonTile->GetJsonContext(), false);
		CString s = AfxLoadJsonString(pDesc->m_strText, pDesc);
		pTileDlg->SetTitle(s);
		s = AfxLoadJsonString(pDesc->m_strCollapsedTitle, pDesc);
		pTileDlg->SetCollapsedDescription(s);
	}
}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::OnPrepareAuxData(CTabDialog* pTabDlg)
{
	m_pClientDocs->OnPrepareAuxData(pTabDlg);
	OnPrepareAuxData(pTabDlg->GetDialogID());
}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::OnPrepareAuxData(CAbstractFormView* pView)
{
	m_pClientDocs->OnPrepareAuxData(pView);
}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::OnPrepareAuxData(CTileDialog* pTileDlg)
{
	m_pClientDocs->OnPrepareAuxData(pTileDlg);
	OnPrepareAuxData(pTileDlg->GetDialogID());
}
//-----------------------------------------------------------------------------
void CAbstractFormDoc::OnPrepareAuxData(CTileGroup* pTileGroup)
{
	m_pClientDocs->OnPrepareAuxData(pTileGroup);
	OnPrepareAuxData(pTileGroup->GetDlgCtrlID());
}
// do related record action based on document mode	
//-----------------------------------------------------------------------------
BOOL CAbstractFormDoc::BatchEnableControls()
{
	if (GetType() != VMT_BATCH)
		return FALSE;

	EnableAllControlLinks(!m_bBatchRunning);
	if (!m_bBatchRunning)
	{
		DispatchDisableControlsForBatch();
	}

	SetBatchButtonState();

	return TRUE;
}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::DispatchDisableControlsForBatch()
{
	DisableControlsForBatch();
	m_pClientDocs->OnDisableControlsForBatch();
}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::DispatchDisableControlsForAddNew()
{
	DisableControlsForAddNew();
	m_pClientDocs->OnDisableControlsForAddNew();
}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::DispatchDisableControlsForEdit()
{
	DisableControlsForEdit();
	m_pClientDocs->OnDisableControlsForEdit();
}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::DispatchEnableControlsForFind()
{
	EnableControlsForFind();
	m_pClientDocs->OnEnableControlsForFind();
}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::DispatchDisableControlsAlways()
{
	DisableControlsAlways();
	m_pClientDocs->OnDisableControlsAlways();
}

//-----------------------------------------------------------------------------
BOOL CAbstractFormDoc::DispatchToolbarDropDown(int nID, CMenu& menu)
{
	BOOL bOk = FALSE;

	bOk = OnToolbarDropDown(nID, menu);
	bOk = m_pClientDocs->OnToolbarDropDown(nID, menu) || bOk;

	return bOk;
}

// do related record action based on document mode	
//-----------------------------------------------------------------------------
void CAbstractFormDoc::EnableControls()
{
	if (BatchEnableControls())
		return;

	// normal processing in interctive mode
	switch (GetFormMode())
	{
	case BROWSE:
		EnableAllControlLinks(FALSE);
		break;

	case NEW:
		EnableAllControlLinks();
		DispatchDisableControlsForAddNew();
		DispatchDisableControlsAlways();
		break;

	case EDIT:
		EnableAllControlLinks();
		DispatchDisableControlsForEdit();
		DispatchDisableControlsAlways();
		break;

	case FIND:
		EnableAllControlLinks(FALSE);
		DispatchEnableControlsForFind();
		break;
	}
}

//------------------------------------------------------------------------------
void CAbstractFormDoc::RemoveDestroying()
{
	CView*	pView = GetFirstView();
	CAbstractFrame* pFrame = NULL;
	if (pView)
		pFrame = dynamic_cast<CAbstractFrame*>(pView->GetParentFrame());
	if (pFrame)
		pFrame->RemoveDestroying();
}

//------------------------------------------------------------------------------
BOOL CAbstractFormDoc::SaveModified()
{
	if (!__super::SaveModified())
	{
		RemoveDestroying();
		return FALSE;
	}

	if (!m_pClientDocs->SaveModified())
	{
		RemoveDestroying();
		return FALSE;
	}

	if (GetType() == VMT_BATCH || GetType() == VMT_FINDER)
	{
		if (m_bBatchRunning)
		{
			Message(_TB("Processing...\r\nYou cannot quit without aborting it."));
			RemoveDestroying();
			return FALSE;
		}

		if (m_pFormManager && !m_pFormManager->SaveModified())
		{
			RemoveDestroying();
			return FALSE;       // don't continue
		}

		return TRUE;
	}

	if (m_pRadarDoc && !m_pRadarDoc->SaveModified())
	{
		RemoveDestroying();
		return FALSE;       // don't continue
	}

	if (m_pFormManager && !m_pFormManager->SaveModified())
	{
		RemoveDestroying();
		return FALSE;       // don't continue
	}

	// Can save only if NEW or EDIT, used by FinderDoc too
	if (!CanDoSaveRecord())
		return TRUE;

	//FinderDoc should not save 
	if (IsKindOf(RUNTIME_CLASS(CFinderDoc)))
		return TRUE;

	// force Update for all controls view
	GetNotValidView(FALSE);

	// don't need save; nothing modified		
	if (!IsModified())
		return TRUE;

	// comportamento di default in caso non si possano visualizzare MessageBox: uscire
	// permettendo la regolare prosecuzione (senza salvare)
	if (IsInUnattendedMode())
		return TRUE;


	BOOL bSave = TRUE;
	switch (AfxMessageBox(_TB("Changes not yet saved. Do you want to save them?"), MB_YESNOCANCEL))
	{
	case IDYES:
		// force Update and test for all controls view
		if (GetNotValidView(TRUE))
		{
			RemoveDestroying();
			return FALSE;
		}

		ConnectToDatabase(szSaveAction);

		m_pTbContext->StartTime(ONOK_TIME);
		// Bad Transaction (single data edit are ok bud trasaction not!)
		if (!(
			(!m_pDBTMaster || m_pDBTMaster->CheckTransaction()) &&
			OnBeforeOkTransaction() &&
			m_pClientDocs->OnBeforeOkTransaction() &&
			OnOkTransaction() &&
			m_pClientDocs->OnOkTransaction() &&
			LockDocument()
			))
		{
			m_pTbContext->StopTime(ONOK_TIME);
			DisconnectFromDatabase();
			m_pMessages->Show(TRUE);
			RemoveDestroying();
			return FALSE;
		}

		// faccio partire la transazione
		if (!m_pTbContext->StartTransaction())
		{
			Message(_TB("Unable to open a new transaction.\r\nDocument not saved"));
			m_pTbContext->StopTime(ONOK_TIME);
			DisconnectFromDatabase();
			RemoveDestroying();
			return FALSE;
		}
		m_pTbContext->StopTime(ONOK_TIME);

		bSave = DoSave(NULL);

		DisconnectFromDatabase();
		break;

	case IDCANCEL:
	{
		RemoveDestroying();
		return FALSE;       // don't continue
	}
	case IDNO: break;
	default: ASSERT(FALSE); break;
	}
	return bSave;    // keep going
}


//Metodo che dice se un Documento puo' essere chiuso in modo safe.
//imposta il thread in unattended mode all'inizio del metodo e ripristina lo stato attended alla fine 
//per evitare che vengano visualizzate messagebox
//-----------------------------------------------------------------------------
BOOL CAbstractFormDoc::CanCloseDocument()
{
	//Incremento il livello di messaggistica per "isolare" gli eventiali messaggi 
	//scatenati dalla CanCloseDocument e dalla SaveModified.
	//Se ci saranno messaggi, significa che era richesta un'interazione con l'utente, e quindi 
	//il documento non puo' essere chiuso
	AfxGetDiagnostic()->StartSession();

	if (!__super::CanCloseDocument())
		return FALSE;

	//setto il thread in unattended(se non lo e' gia') mode cosi non verranno mostrate messagebox 
	//ne da questo documento ne da i suoi figli
	BOOL bIsUnattended = AfxIsInUnattendedMode();
	if (!bIsUnattended)
		AfxGetThreadContext()->SetUserInteractionMode(UNATTENDED);

	BOOL bOk = CanDoEscape();

	BOOL bEdit = GetFormMode() == EDIT;
	bOk = bOk && !(IsModified() && (bEdit || GetFormMode() == NEW));
	bOk = bOk && DispatchOnBeforeEscape();

	//controllo se ci sono messaggi nel corrente livello, il documento non puo' essere chiuso	
	bOk = bOk && !AfxGetDiagnostic()->MessageFound(FALSE);
	//cancello gli eventuali messaggi di questo livello
	AfxGetDiagnostic()->ClearMessages(FALSE);
	//ripristino il livello precedente
	AfxGetDiagnostic()->EndSession();

	//Se non si era in UnattendedMode, ripristino la modalita di interazione Attended
	if (!bIsUnattended)
		AfxGetThreadContext()->SetUserInteractionMode(ATTENDED);

	return bOk;
}

//-----------------------------------------------------------------------------
BOOL CAbstractFormDoc::Escape(BOOL bMessage /*= TRUE*/)
{
	if (GetFormMode() == BROWSE)
		return TRUE;

	if (!CanDoEscape()) return FALSE;

	// In stato di edit conservo il current ammettendo di considerare lecita
	// la non sincronia della posizione del current record rispetto
	// alla corrente posizione del Browser.			
	BOOL bEdit = GetFormMode() == EDIT;

	// something modified, ask for abort changes (but only if messaging is enabled)
	if (
		bMessage &&
		IsModified() &&
		(bEdit || GetFormMode() == NEW) &&
		!IsInUnattendedMode() &&
		(AfxMessageBox(_TB("Are you sure you want to quit?"), MB_OKCANCEL) == IDCANCEL)
		)
		return FALSE;

	if (!DispatchOnBeforeEscape())
		return FALSE;

	//NEEDS_UPDATE_DATA_VIEW();  //rimossa per anomalia 18126, viene fatta dentro la go in browse mode
	// Signal to all view that process is terminating
	AbortAllViews();
	GoInBrowseMode();

	if (ReturnDataToCallLink(TRUE, FALSE))
		return TRUE;

	if (m_pDBTMaster)
	{
		// Reinizializza il record master con il corrente record 
		if (ValidCurrentRecord())
		{
			RefreshCurrentRecord();
			BrowseRecord();   //An: 20247, era BrowseRecord(FALSE)
		}
		else
		{
			InitData();
			UpdateDataView();
			SetDefaultFocus(); //An. 18126
		}
	}

	return TRUE;
}

// open master record with default key (key in 0 position)
//-----------------------------------------------------------------------------
BOOL CAbstractFormDoc::Attach(DBTMaster* pDBTMaster)
{
	ASSERT(pDBTMaster);

	m_pDBTMaster = pDBTMaster;

	if (!OnPostAttachData())
		return FALSE;

	// da un chance ai documenti accessori di attacare propri DBT, HotLink
	// e altri oggetti alla catena del DBTMaster e del documento
	m_bOnAttachClient = TRUE; //� stata chiamata l'OnAttachData dei clientdoc
	if (!m_pClientDocs->OnAttachData())
		return FALSE;

	//faccio il controllo sulla validit� dei dbt agganciati
	BOOL bOk = m_pDBTMaster->IsValidRecordsSchema(m_pMessages) && m_pDBTMaster->Open();

	if (bOk && m_pXMLDocInfo)
		m_pDBTMaster->LoadXMLDBTInfo();

	if (!m_pDBTMaster->m_bDBTOnView && !m_pDBTMaster->GetRecord()->HasGUID())
	{
		Message(cwsprintf(_TB("The table %s must have the attribute mastertable= \'true\'. \n\rPlease modify its table node description in the DatabaseObjects.xml of the module %s and then run the \'Create or upgrade company database structure\' procedure in Administration Console"), m_pDBTMaster->GetRecord()->GetNamespace()->ToString(), m_pDBTMaster->GetRecord()->GetNamespace()->GetModuleName()));
		return FALSE;
	}

	return bOk;
}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::ToolBarButtonsHideGhost(int nCase)
{
	int k = 0;
	//ToolBar draw
	CTBTabbedToolbar* pTabbedBar = GetMasterFrame() ? GetMasterFrame()->GetTabbedToolBar() : NULL;
	// l'ADM background non ha la toolbar, quello interattivo si
	if (!pTabbedBar)
		return;

	UINT HideIDs[10] = { 0 };
	UINT ShowIDs[10] = { 0 };
	UINT GhostIDs[10] = { 0 };

	switch (nCase)
	{
	case 0: // New - Edit
		ShowIDs[k] = ID_EXTDOC_SAVE; k++;
		ShowIDs[k] = ID_EXTDOC_ESCAPE; k++;
		ShowIDs[k] = 0; k = 0;

		HideIDs[k] = ID_EXTDOC_NEW; k++;
		HideIDs[k] = ID_EXTDOC_EDIT; k++;
		HideIDs[k] = 0; k = 0;

		GhostIDs[k] = ID_EXTDOC_DELETE; k++;
		GhostIDs[k] = 0; k = 0;
		break;

	case 1: // Browsing Document
		ShowIDs[k] = ID_EXTDOC_NEW; k++;
		ShowIDs[k] = ID_EXTDOC_EDIT; k++;
		ShowIDs[k] = ID_EXTDOC_DELETE; k++;
		ShowIDs[k] = 0; k = 0;

		HideIDs[k] = ID_EXTDOC_SAVE; k++;
		HideIDs[k] = ID_EXTDOC_ESCAPE; k++;
		HideIDs[k] = 0; k = 0;
		break;


	case 3: // Find
		ShowIDs[k] = ID_EXTDOC_ESCAPE; k++;
		ShowIDs[k] = 0; k = 0;

		HideIDs[k] = ID_EXTDOC_SAVE; k++;
		HideIDs[k] = ID_EXTDOC_NEW; k++;
		HideIDs[k] = 0; k = 0;

		GhostIDs[k] = ID_EXTDOC_EDIT; k++;
		GhostIDs[k] = ID_EXTDOC_DELETE; k++;
		GhostIDs[k] = 0; k = 0;
		break;

	default:
		ASSERT(FALSE);
		break;
	}

	// Hide Ghost Buttons
	pTabbedBar->HideGhostButtons(HideIDs, GhostIDs, ShowIDs);
}

//-----------------------------------------------------------------------------
BOOL CAbstractFormDoc::NewRecord()
{
	// il test necessario per evitare che gli acceleratori bypassino il CmdUI
	if (!DispatchCanDoNewRecord())
		return FALSE;
	if (!DispatchOnBeforeNewRecord())
		return FALSE;

	{ //non togliere la graffa: scope della NEEDS_UPDATE_DATA_VIEW()
		SUSPEND_UPDATE_DATA_VIEW()
			// Devo fare una query con il browser per vedere quanti record ci sono.
			// e stabilire se esiste gia` un record. Nel qual caso non abilito l'aggiunta
			// di nuovi record.
			ASSERT(m_pBrowser);

		ConnectToDatabase(szNewAction);
		if (
			m_bOnlyOneRecord &&
			m_pBrowser &&
			(
				!m_pBrowser->NoCurrent() ||
				m_pBrowser->Query() && !m_pBrowser->IsEmpty()
				)
			)
		{
			Message(_TB("It is possible to insert only one document!"));

			GoInBrowseMode();
			DisconnectFromDatabase();
			return FALSE;
		}

		if (!OnOkNewRecord() || !m_pClientDocs->OnOkNewRecord())
		{
			GoInBrowseMode();
			DisconnectFromDatabase();
			return FALSE;
		}

		AbortAllViews();

		if (m_pDBTMaster && !m_pDBTMaster->AddNew())
		{
			GoInBrowseMode();
			DisconnectFromDatabase();
			return FALSE;
		}

		// Seleziona il  modo NEW per aggiungere un nuovo record
		FormMode fmOldFormMode = CBaseDocument::SetFormMode(NEW);
		InitData();
		SetModifiedFlag(FALSE);

		OnAfterSetFormMode(fmOldFormMode);
		m_pClientDocs->OnAfterSetFormMode(fmOldFormMode);


		// give to final implementation a chances to modify specific
		// auxiliary implementation dependent data
		if (!DispatchPrepareAuxData())
		{
			InitData();
			ClearCurrentRecord();
			GoInBrowseMode();
			DisconnectFromDatabase();
			return FALSE;
		}

		// aggiorna tutte le views per inizializzare i controls che contengono i dataobj
		// e da il fuoco alla finestra primaria
		EnableControls();

		UpdateDataView();
	}

	DisconnectFromDatabase();

	ToolBarButtonsHideGhost(0);
	SetDefaultFocus();
	return TRUE;
}

//permette di effettuare il browse del documento partendo dal valore del GUID passato come argomento
// informato stringa oppure direttamente come DataGuid
//--------------------------------------------------------------------------
BOOL CAbstractFormDoc::BrowseRecordByTBGuid(const CString& strDataGuid)
{
	return BrowseRecordByTBGuid(DataGuid(strDataGuid));
}

static TCHAR szPTBGuid[] = _T("PTBGuid");

//--------------------------------------------------------------------------
BOOL CAbstractFormDoc::BrowseRecordByTBGuid(const DataGuid& dataGuid)
{
	SqlRecord* pMasteRec = m_pDBTMaster->GetRecord();

	SqlTable aTable(pMasteRec, GetReadOnlySqlSession());
	TRY
	{
		aTable.Open();
		aTable.SelectAll();
		aTable.AddParam(szPTBGuid, pMasteRec->f_TBGuid);
		aTable.AddFilterColumn(pMasteRec->f_TBGuid);
		aTable.SetParamValue(szPTBGuid, dataGuid);
		//pTable->SetOnlyOneRecord(); //@@TODO con ADO.NET
		aTable.Query();
		aTable.Close();
		return BrowseRecord();
	}
		CATCH(SqlException, e)
	{
		Message(cwsprintf(_TB("Document with TBGuid value {0-%s} not found!"), dataGuid.ToString()));
		if (aTable.IsOpen())
			aTable.Close();
		return FALSE;

	}
	END_CATCH

		return TRUE;
}

//--------------------------------------------------------------------------
BOOL CAbstractFormDoc::BrowseRecord(BOOL bFocus/*= TRUE*/, BOOL bCreating/* = FALSE*/)
{
	{ //non togliere la graffa, scope della NEEDS_UPDATE_DATA_VIEW();
		SUSPEND_UPDATE_DATA_VIEW();
		AbortAllViews();



		m_pClientDocs->OnBeforeBrowseRecord();

		ConnectToDatabase(szBrowseAction);
		CString strKeyDescri;
		if (m_pDBTMaster)
			strKeyDescri = m_pDBTMaster->GetRecord()->GetPrimaryKeyDescription();
		// read data input from User or passed from radar
		if (m_pDBTMaster && !m_pDBTMaster->FindData(FALSE))
		{
			// durante la creazione/cancellazione del documento non da messaggio di diagnostica
			// perche` potrei non aver inserito ancora alcun record o aver cancellato tutti i record
			if (!bCreating)
				Message(cwsprintf(_TB("Document {0-%s} not found!"), (LPCTSTR)strKeyDescri));

			InitData();
			ClearCurrentRecord();
			GoInBrowseMode();
			DisconnectFromDatabase();
			return FALSE;
		}

		SaveCurrentRecord();
		DispatchPrepareAuxData(bCreating);

		UpdateDataView();
	}

	m_pClientDocs->OnAfterBrowseRecord();
	DisconnectFromDatabase();

	if (bFocus)
		SetDefaultFocus();

	ToolBarButtonsHideGhost(1);
	return TRUE;
}

//--------------------------------------------------------------------------
void CAbstractFormDoc::ConnectToDatabase(LPCTSTR pszActionName/*= NULL*/)
{
	//mi connetto al database
	m_pTbContext->StartTime(TOTAL_TIME, pszActionName);
	m_pTbContext->Connect();
}

//--------------------------------------------------------------------------
void CAbstractFormDoc::DisconnectFromDatabase()
{
	//mi disconnetto al database
	m_pTbContext->Disconnect();
	m_pTbContext->StopTime(TOTAL_TIME);
}

//--------------------------------------------------------------------------
BOOL CAbstractFormDoc::EditRecord()
{
	// il test necessario per evitare che gli acceleratori bypassino il CmdUI
	if (!DispatchCanDoEditRecord())
		return FALSE;
	if (!DispatchOnBeforeEditRecord())
		return FALSE;
	{ //non togliere la graffa, scope della NEEDS_UPDATE_DATA_VIEW()
		SUSPEND_UPDATE_DATA_VIEW();
		//mi connetto al database
		ConnectToDatabase(szEditAction);
		GetNotValidView(TRUE);

		// Deve essere fatta prima della FindData per permettere al DBT di
		// fare le giuste reload anche in caso di EDIT_DELAYED o ALL_DELAYED
		FormMode fmOldFormMode = CBaseDocument::SetFormMode(EDIT);
		OnAfterSetFormMode(fmOldFormMode);
		m_pClientDocs->OnAfterSetFormMode(fmOldFormMode);


		CString strKeyDescri;
		if (m_pDBTMaster)
			strKeyDescri = m_pDBTMaster->GetRecord()->GetPrimaryKeyDescription();
		// Prova a bloccare l'accesso al documento da editare (OCCHIO! solo su master)
		// questo per prenotarmi il dato prima del refresh dei dati altrimenti rischio di avere dei dati vecchi
		BOOL bFind = !m_pDBTMaster || m_pDBTMaster->FindData();
		BOOL bLock = bFind && (!m_pDBTMaster || LockMaster(!IsInUnattendedMode())); //il lock lo faccio solo se il il DBTMaster esiste
		if (!bLock || !bFind)
		{
			if (!bFind)
			{
				Message(cwsprintf(_TB("Document {0-%s} not found!"), (LPCTSTR)strKeyDescri));
				ClearCurrentRecord();
			}
			else
				SaveCurrentRecord();

			GoInBrowseMode();
			//mi disconnetto al database
			DisconnectFromDatabase();
			return FALSE;
		}

		SaveCurrentRecord();

		if (!OnOkEdit() || !m_pClientDocs->OnOkEdit())
		{
			// serve a far ricaricare i DBT delayed
			DispatchPrepareAuxData();
			GoInBrowseMode();
			//mi disconnetto al database
			DisconnectFromDatabase();
			return FALSE;
		}

		if (m_pDBTMaster && !m_pDBTMaster->Edit())
		{
			// serve a far ricaricare i DBT delayed
			DispatchPrepareAuxData();
			GoInBrowseMode();
			//mi disconnetto al database
			DisconnectFromDatabase();
			return FALSE;
		}

		// deve essere fatta prima della PrepareAuxData perche` il programmatore
		// potrebbe decidere di mettersi in modalita "modificato" per forzare eventuali
		// salvataggi di dati precompilati nelle varie OnPrepareAuxData.
		SetModifiedFlag(FALSE);

		// Da una possibilita` al programmatore di personalizzare eventuali dati 
		// accessori che dipendono dalla logica della applicazione
		if (!DispatchPrepareAuxData())
		{
			GoInBrowseMode();
			//mi disconnetto dal database
			DisconnectFromDatabase();
			return FALSE;
		}

		// Updata tutte le views per inizia		izzare i controls che contengono i dataobj
		// e da il fuoco alla finestra primaria (non lo fa se chiamata dal radar)
		EnableControls();
		UpdateDataView();
	}
	SetDefaultFocus();

	//mi disconnetto al database
	DisconnectFromDatabase();

	ToolBarButtonsHideGhost(0);

	return TRUE;
}

//--------------------------------------------------------------------------
BOOL CAbstractFormDoc::FindRecord()
{
	// il test necessario per evitare che gli acceleratori bypassino il CmdUI
	if (!CanDoFindRecord())
		return FALSE;
	{//non togliere la graffa: scope per la UpdateDataView
		SUSPEND_UPDATE_DATA_VIEW();
		GetNotValidView(TRUE);

		m_pTbContext->StartTime(TOTAL_TIME, szFindAction);
		SetFormMode(FIND);
		ToolBarButtonsHideGhost(3);
		m_pTbContext->StopTime(TOTAL_TIME);

		UpdateDataView();
	}

	SetModifiedFlag(FALSE);
	SetDefaultFocus();

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CAbstractFormDoc::SaveRecord()
{
	// Can save only if NEW or EDIT
	if (!CanDoSaveRecord())
		return FALSE;

	// something is wrong in some dialog control edit
	// also force SetModifiedFlag to be set if some control is modified
	// force Update and test for all controls view
	if (GetNotValidView(TRUE))
		return FALSE;

	// don't need save; nothing modified so back to BROWSE mode
	if (!IsModified())
	{
		GoInBrowseMode();
		return FALSE;
	}

	ConnectToDatabase(szSaveAction);
	m_pTbContext->StartTime(ONOK_TIME);

	// Bad Transaction (single data edit are ok, trasaction no yet!)
	if (!(
		(!m_pDBTMaster || m_pDBTMaster->CheckTransaction()) &&
		OnBeforeOkTransaction() &&
		m_pClientDocs->OnBeforeOkTransaction() &&
		OnOkTransaction() &&
		m_pClientDocs->OnOkTransaction() &&
		LockDocument()
		))
	{
		m_pTbContext->StopTime(ONOK_TIME);
		DisconnectFromDatabase();

		if (m_pMessages && AfxIsValidAddress(m_pMessages, sizeof(CMessages)) && m_pMessages->ErrorFound())
			m_pMessages->Show(TRUE);
		return FALSE;
	}

	// faccio partire la transazione
	if (!m_pTbContext->StartTransaction())
	{
		Message(_TB("Unable to open a new transaction.\r\nDocument not saved"));
		m_pTbContext->StopTime(ONOK_TIME);
		DisconnectFromDatabase();
		return FALSE;
	}

	m_pTbContext->StopTime(ONOK_TIME);

	BOOL bOk = DoSave(NULL);

	DisconnectFromDatabase();

	//TODOLUCA  eliminata fino a quando non decidiamo il da farsi
	/*if (!IsInUnattendedMode())
	{
		if (AfxIsActivated(TBNET_APP, _T("MenuHistory")))
			AddToHistory(AfxGetLoginContext(), GetNamespace().ToString(), GetNamespace().GetTypeString(), m_pDBTMaster ? m_pDBTMaster->GetRecord()->GetPrimaryKeyNameValue() :  _T(""), this->GetTitle(), this->GetDefaultMenuDescription());
	}*/

	if (bOk)
		ToolBarButtonsHideGhost(1);

	Activate();
	return bOk;
}


// per il salvataggio del documento in import
//-----------------------------------------------------------------------------
BOOL CAbstractFormDoc::SaveImportDocument()
{
	SetModifiedFlag();

	ConnectToDatabase(szSaveAction);

	// Bad Transaction (single data edit are ok, trasaction no yet!)
	if (!(m_pDBTMaster->CheckTransaction() &&
		OnBeforeOkTransaction() &&
		m_pClientDocs->OnBeforeOkTransaction() &&
		OnOkTransaction() &&
		m_pClientDocs->OnOkTransaction() &&
		LockDocument()
		))
	{
		GoInBrowseMode();
		DisconnectFromDatabase();
		return FALSE;
	}

	if (!DoSave(NULL))
	{
		GoInBrowseMode();
		DisconnectFromDatabase();
		return FALSE;
	}

	DisconnectFromDatabase();

	return TRUE;
}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::ForcePreparePrimaryKey()
{
	if (m_pDBTMaster)
		m_pDBTMaster->ForcePreparePrimaryKey();

}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::ExecuteBatchFromExternalController()
{
	m_pMessages->StartSession();
	if (m_bClassicRunFromExternalController)
	{
		CAbstractFormView* pView = dynamic_cast<CAbstractFormView*>(GetFirstView());
		if (pView)
			pView->PerformBatchOperations();
	}
	else
		PerformBatchOperations();

	m_pMessages->EndSession();

}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::ValorizeControlData(ControlLinks *pLinks, BOOL bNotifyChange)
{
	if (!m_pExternalControllerInfo || !pLinks)
		return;
	{//non togliere la graffa: scope per la UpdateDataView
		SUSPEND_UPDATE_DATA_VIEW();
		for (int i = 0; i < pLinks->GetSize(); i++)
		{
			CWnd* pW = pLinks->GetAt(i);
			CParsedCtrl* pCtrl = GetParsedCtrl(pW);
			if (pCtrl)
			{
				m_pExternalControllerInfo->ValorizeControlData(pCtrl);
				if (bNotifyChange)
					pCtrl->NotifyToParent(EN_VALUE_CHANGED);
			}
			else if (pW->IsKindOf(RUNTIME_CLASS(CBodyEdit)))
				m_pExternalControllerInfo->ValorizeControlData((CBodyEdit*)pW);
			else if (pW->IsKindOf(RUNTIME_CLASS(CMultiSelectionListBox)))
				m_pExternalControllerInfo->ValorizeControlData((CMultiSelectionListBox*)pW);
		}
		UpdateDataView();
	}
	m_pExternalControllerInfo->UnlockValues();
}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::RetrieveControlData(ControlLinks *pLinks)
{
	if (!m_pExternalControllerInfo || !pLinks) return;

	for (int i = 0; i < pLinks->GetSize(); i++)
	{
		CWnd* pW = pLinks->GetAt(i);
		CParsedCtrl* pCtrl = GetParsedCtrl(pW);
		if (pCtrl)
		{
			if (!pCtrl->GetCtrlCWnd()->IsKindOf(RUNTIME_CLASS(CStatic)))
				m_pExternalControllerInfo->RetrieveControlData(pCtrl);
		}
		else if (pW->IsKindOf(RUNTIME_CLASS(CBodyEdit)))
			m_pExternalControllerInfo->RetrieveControlData((CBodyEdit*)pW);
		else if (pW->IsKindOf(RUNTIME_CLASS(CMultiSelectionListBox)))
			m_pExternalControllerInfo->RetrieveControlData((CMultiSelectionListBox*)pW);
	}
}

// per la cancellazione del documento importato
//-----------------------------------------------------------------------------
BOOL CAbstractFormDoc::DeleteImportDocument()
{
	BrowseRecord();
	return DeleteRecord();
}

// se riesce la cancellazione ritorna TRUE
//-----------------------------------------------------------------------------
BOOL CAbstractFormDoc::DeleteRecord()
{
	//mi connetto al database
	//m_pTbContext->Connect();


	if (
		!DispatchCanDoDeleteRecord() ||
		(!IsInUnattendedMode() && AfxMessageBox(_TB("Are you sure you want to delete it?"), MB_YESNO | MB_ICONQUESTION | MB_DEFBUTTON2) == IDNO) ||
		!DispatchOnBeforeDeleteRecord()
		)
	{
		//mi disconnetto dal database
		//m_pTbContext->Disconnect();
		return FALSE;
	}


	ConnectToDatabase(szDeleteAction);

	// In Stato di Browse devo rileggere l'intero documento per aggiornare il
	// contenuto dei buffer old onde poter stornare le transazioni in modo opportuno.
	// In Edit mode gli old buffers sono buoni, mentre se prima della DeleteRecord il
	// corrente documento e` stato modificato e salvato senza ulteriori movimenti del 
	// cursore gli old buffer contengo l'informazione vecchia.

	CString strKeyDescri;
	if (m_pDBTMaster)
		strKeyDescri = m_pDBTMaster->GetRecord()->GetPrimaryKeyDescription();

	if (GetFormMode() == BROWSE)
	{
		BOOL bFind = !m_pDBTMaster || m_pDBTMaster->FindData();
		BOOL bLook = bFind && LockMaster(!IsInUnattendedMode());
		if (!bFind || !bLook)
		{
			if (!bFind)
			{
				Message(cwsprintf(_TB("Document {0-%s} not found!"), (LPCTSTR)strKeyDescri));
				// se il database connesso � di tipo ORACLE devo invalidare la query del browser
			//(poich� oracle non supporta il KEYSET_CURSOR)
				if (m_pBrowser && m_pSqlConnection->GetDBMSType() == DBMS_ORACLE)
					m_pBrowser->SetInvalidQuery();
			}
			GoInBrowseMode();
			DisconnectFromDatabase();
			return FALSE;
		}
	}

	if (!(
		m_pClientDocs->OnBeforeOkDelete() &&
		OnOkDelete() && m_pClientDocs->OnOkDelete() &&
		LockDocument(TRUE)
		))
	{
		if (m_pMessages && AfxIsValidAddress(m_pMessages, sizeof(CMessages)) && m_pMessages->ErrorFound())
			m_pMessages->Show(TRUE);

		GoInBrowseMode();
		DisconnectFromDatabase();
		return FALSE;
	}

	if (!m_pTbContext->StartTransaction())
	{
		Message(_TB("Unable to open a new transaction.\r\nDocument not deleted"));
		GoInBrowseMode();
		DisconnectFromDatabase();
		return FALSE;
	}

	// ignora eventuali dati fasulli presenti nei controls
	AbortAllViews();

	CStatusBarMsg msg(TRUE, TRUE, FALSE);

	CString primaryRecordToDelete = NULL;
	if (m_pDBTMaster && m_pDBTMaster->GetRecord())
		primaryRecordToDelete = m_pDBTMaster->GetRecord()->GetPrimaryKeyNameValue();

	TRY
	{
		//messaggio a tutti i client doc (ma sopratutto il CDMacroRecorder per sapere quando inizia una transazione
		//per la cancellazione
		DispatchOnBeforeDelete();

		m_pTbContext->StartTime(PRIMARY_TIME);
		BOOL bPrimary = !m_pDBTMaster || m_pDBTMaster->Delete();
		m_pTbContext->StopTime(PRIMARY_TIME);

		// tempo della transazione primaria
		m_pTbContext->StartTime(SECONDARY_TIME);
		BOOL bSecondary = m_pClientDocs->OnBeforeDeleteTransaction() && FireBehaviour(bhe_DeleteTransaction) && OnDeleteTransaction() && m_pClientDocs->OnDeleteTransaction();

		if (!(bPrimary && bSecondary))
		{
			m_pTbContext->Rollback();
			m_pTbContext->StopTime(SECONDARY_TIME);

			// Reinizializza il record master con il corrente record 
			if (ValidCurrentRecord())
			{
				RefreshCurrentRecord();
				BrowseRecord(FALSE);
			}
			msg.Dispose();//permetto la corretta assegnazione del fuoco alla finestra (che viene riabilitata)
			GoInBrowseMode();

			m_pMessages->Show(TRUE);
			DispatchOnAfterDelete();
			DisconnectFromDatabase();
			return FALSE;
		}
		m_pTbContext->Commit();
		m_pTbContext->StopTime(SECONDARY_TIME);

		//auxiliary transaction
		m_pTbContext->StartTime(AUXILIARY_TIME);

		OnExtraDeleteTransaction();
		m_pClientDocs->OnExtraDeleteTransaction();

		m_pTbContext->StopTime(AUXILIARY_TIME);

		m_pMessages->Show(TRUE);

		InitData();
		ClearCurrentRecord();

		msg.Show(_TB("Document deleted"));
		msg.Dispose();//permetto la corretta assegnazione del fuoco alla finestra (che viene riabilitata)

		GoInBrowseMode();

		// se il database connesso � di tipo ORACLE devo invalidare la query del browser (poich� oracle non supporta il KEYSET_CURSOR)
		if (m_pBrowser && m_pSqlConnection->GetDBMSType() == DBMS_ORACLE)
			m_pBrowser->SetInvalidQuery();

		// anzich� lasciare il record indefinito effettuo un MoveNext
		if (CanDoNextRecord())
		{
			m_pTbContext->StartTime(TOTAL_TIME, szMoveNext);
			if (m_pBrowser && m_pBrowser->NextRecord() && !m_pBrowser->IsEmpty())
				BrowseRecord(FALSE);
			m_pTbContext->StopTime(TOTAL_TIME);
		}

		DispatchOnAfterDelete();
		DisconnectFromDatabase();
		return TRUE;
	}
		CATCH(SqlException, e)
	{
		if (m_pMessages && AfxIsValidAddress(m_pMessages, sizeof(CMessages)))
		{
			m_pMessages->Add(_TB("The document will not be deleted due to the following exception:  {0-%s} "), e->m_strError);
			m_pMessages->Show(TRUE);
		}
		m_pTbContext->Rollback();
		// Reinizializza il record master con il corrente record 
		if (ValidCurrentRecord())
		{
			RefreshCurrentRecord();
			BrowseRecord(FALSE);
		}
		msg.Dispose();//permetto la corretta assegnazione del fuoco alla finestra (che viene riabilitata)

		GoInBrowseMode();

		m_pTbContext->StopTime(PRIMARY_TIME);
		m_pTbContext->StopTime(SECONDARY_TIME);
		m_pTbContext->StopTime(AUXILIARY_TIME);
		DispatchOnAfterDelete();
		DisconnectFromDatabase();
		return FALSE;
	}
	END_CATCH;
}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::SetBatchButtonState()
{
	POSITION pos = GetFirstViewPosition();
	while (pos != NULL)
	{
		CView* pView = GetNextView(pos);
		ASSERT_VALID(pView);

		if (pView->IsKindOf(RUNTIME_CLASS(CAbstractFormView)))
			((CAbstractFormView*)pView)->SetBatchViewButtonState();
	}
	SwitchBatchRunButtonState();
}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::BatchStart()
{
	if (GetType() != VMT_BATCH) return;

	USES_DIAGNOSTIC();

	DWORD dwBeginTick = 0;
	DWORD dwStartTick = 0;
	DWORD dwStopTick = 0;
	DWORD dwEndTick = 0;

	// Utilizza una critical area per evitare problemi multitasking
	if (!m_BatchRunningArea.IsLocked())
	{
		if (GetNotValidView(TRUE))
		{
			m_BatchRunningArea.Unlock();
			return;
		}

		m_bBatchRunning = TRUE;

		EnableControls();
		UpdateDataView();

		m_BatchScheduler.Start();

		//ConnectToDatabase(szBatchAction);
		dwBeginTick = ::GetTickCount();

		m_pTbContext->Connect();

		// Dipende dalla implementazione del programmatore
		// ed e' il loop stretto gestito dallo schedule che implementa
		// la funzionalita' batch finale
		if (DispatchOnBeforeBatchExecute() && (!m_pHotFilterManager || m_pHotFilterManager->OnBeforeBatchExecute()))
		{
			CPushMessageLoopDepthMng __pushLoopDepth(BATCH);
			AfxGetThreadContext()->RaiseCallBreakEvent();
			try
			{
				AfxGetTBThread()->IncSuspendIdle();

				dwStartTick = ::GetTickCount();

				OnBatchExecute();

				dwStopTick = ::GetTickCount();

				AfxGetTBThread()->DecSuspendIdle();
			}
			catch (CThreadAbortedException* pException)
			{
				//DisconnectFromDatabase();
				m_pTbContext->Disconnect();
				AfxGetTBThread()->DecSuspendIdle();
				throw pException;
			}
#ifndef DEBUG
			catch (CException* pException)
			{
				AfxGetTBThread()->DecSuspendIdle();

				AfxGetDiagnostic()->Add(pException); //null pointer accepted
				if (pException)
					pException->Delete();
			}
#endif
			DispatchOnAfterBatchExecute();

			if (!IsInUnattendedMode())
				GetFirstView()->SendMessage(UM_EXTDOC_BATCH_COMPLETED, NULL, NULL);

			OnBatchCompleted();

			dwEndTick = ::GetTickCount();
		}

		UnlockAll(); //su richiesta dei gestionalisti
		m_pTbContext->Disconnect();

		if (m_bBatchRunning) // nel frattempo la batch potrbbe anche essere stata stoppata....
		{
			m_BatchScheduler.Terminate();
			m_bBatchRunning = FALSE;
			if (IsRunningFromExternalController())
			{
				m_pExternalControllerInfo->SetRunningTaskStatus(CExternalControllerInfo::TASK_SUCCESS);
				m_pExternalControllerInfo->m_Finished.Set();
			}
		}
		else
		{
			if (IsRunningFromExternalController())
			{
				m_pExternalControllerInfo->SetRunningTaskStatus(CExternalControllerInfo::TASK_USER_ABORT);
				m_pExternalControllerInfo->m_Finished.Set();
			}
		}

		OnManageAfterBatchExecute();

		DWORD spanFull = dwEndTick ? (dwEndTick - dwBeginTick) / 1000 : 0;
		if (spanFull > 0)
		{
			DWORD span = (dwStopTick - dwStartTick) / 1000;
			CString msg = ::cwsprintf(_TB("Elapsed time: {0-%d}:{1-%d} seconds"), span, spanFull);
			((CMasterFrame*)GetFrame())->SetStatusBarText(msg);
			TRACE(msg + '\n');
			//AfxMessageBox(msg);
		}

		EnableControls();
		UpdateDataView();

		m_BatchRunningArea.Unlock();
	}

	SetDefaultFocus();
	if (::GetFocus() == NULL && GetFrame())
		GetFrame()->SetFocus();
}

// permette al programmatore in caso di errore di interrompere l'elaborazione 
// e segnalarne l'errore all'utente
//-----------------------------------------------------------------------------
void CAbstractFormDoc::SetBatchAborted(LPCTSTR lpszErrorMessage)
{
	if (GetType() != VMT_BATCH) return;

	Message((!lpszErrorMessage)
		? _TB("Processing failed")
		: cwsprintf(_TB("Processing failed due to the following error:\r\n {0-%s}"), lpszErrorMessage)
	);

	//m_pTbContext->Disconnect();
	m_BatchScheduler.Terminate();
	m_bBatchRunning = FALSE;
	if (IsRunningFromExternalController())
	{
		m_pExternalControllerInfo->SetRunningTaskStatus(CExternalControllerInfo::TASK_FAILED);
		m_pExternalControllerInfo->m_Finished.Set();
	}
}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::BatchStop()
{
	if (GetType() != VMT_BATCH) return;

	if
		(
		(IsRunningFromExternalController())
			||
			(IsInUnattendedMode())
			||
			AfxMessageBox(_TB("Processing...\r\nAre you sure you want to abort it?"), MB_YESNO) == IDYES
			)

	{
		DisconnectFromDatabase();
		m_BatchScheduler.Terminate();
		m_bBatchRunning = FALSE;
		if (IsRunningFromExternalController())
		{
			m_pExternalControllerInfo->SetRunningTaskStatus(CExternalControllerInfo::TASK_USER_ABORT);
			m_pExternalControllerInfo->m_Finished.Set();
		}

	}
	//WM_CLOSE();
}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::SwitchBatchRunButtonState()
{
	if (GetType() != VMT_BATCH) return;
	CMasterFrame* pFrame = GetMasterFrame();
	if (pFrame)
		pFrame->SwitchBatchRunButtonState();
}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::OnBatchStartStop()
{
	if (GetType() != VMT_BATCH) return;

	if (IsEditingParamsFromExternalController())
	{
		if (!GetNotValidView(TRUE))
		{

			m_pExternalControllerInfo->SetRunningTaskStatus(CExternalControllerInfo::TASK_SAVE_PARAMS);
			m_pExternalControllerInfo->m_Finished.Set();
			//TODO FAI METODO CHE SALVA
			// parsing of the result document 
			((CAbstractFormView *)GetFirstView())->SyncExternalControllerInfo(TRUE);
			CXMLDocumentObject aDoc;
			aDoc.LoadXML(m_pExternalControllerInfo->GetXmlString());
			CString	sFileName = AfxGetPathFinder()->GetXmlParametersDirectory();
			sFileName = sFileName + SLASH_CHAR + m_pExternalControllerInfo->m_code + _T(".xml");
			aDoc.SaveXMLFile(sFileName, TRUE);
		}
		return;
	}

	// Se sta runnando il processo batch lo ferma altrimenti lo lancia
	if (m_bBatchRunning)
		BatchStop();
	else
	{
		if (!GetNotValidView(TRUE)) // per scatenare il check sui control della form!
			BatchStart();
	}

	if (m_bBatchCloseAfterExecution && GetMasterFrame())
		GetMasterFrame()->PostMessage(WM_CLOSE);
}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::OnBatchPauseResume()
{
	if (GetType() != VMT_BATCH)
		return;

	if (!m_bBatchRunning)
		return;

	if (m_BatchScheduler.IsPaused())
		m_BatchScheduler.Resume();
	else
		m_BatchScheduler.Pause();

	SetBatchButtonState();
}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::OnUpdateBatchStartStop(CCmdUI* pCmdUI)
{
	pCmdUI->Enable(CanDoBatchExecute());
}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::OnUpdateBatchPauseResume(CCmdUI* pCmdUI)
{
	pCmdUI->Enable(CanDoPauseResume());
}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::OnFirstRecord()
{
	FirstRecord();
}
//-----------------------------------------------------------------------------
void CAbstractFormDoc::FirstRecord()
{
	if (!CanDoFirstRecord())
		return;

	m_pTbContext->StartTime(TOTAL_TIME, szMoveFirst);
	if (m_pBrowser && m_pBrowser->FirstRecord())
		BrowseRecord(FALSE);
	m_pTbContext->StopTime(TOTAL_TIME);
}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::OnPrevRecord()
{
	PrevRecord();
}
//-----------------------------------------------------------------------------
void CAbstractFormDoc::PrevRecord()
{
	if (!CanDoPrevRecord())
		return;

	m_pTbContext->StartTime(TOTAL_TIME, szMovePrev);
	if (m_pBrowser && m_pBrowser->PrevRecord())
		BrowseRecord(FALSE);
	m_pTbContext->StopTime(TOTAL_TIME);
}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::OnNextRecord()
{
	NextRecord();
}
//-----------------------------------------------------------------------------
void CAbstractFormDoc::NextRecord()
{
	if (!CanDoNextRecord())
		return;

	m_pTbContext->StartTime(TOTAL_TIME, szMoveNext);
	if (m_pBrowser && m_pBrowser->NextRecord())
		BrowseRecord(FALSE);
	m_pTbContext->StopTime(TOTAL_TIME);
}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::OnLastRecord()
{
	LastRecord();
}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::LastRecord()
{
	if (!CanDoLastRecord())
		return;

	m_pTbContext->StartTime(TOTAL_TIME, szMoveLast);
	if (m_pBrowser && m_pBrowser->LastRecord())
		BrowseRecord(FALSE);

	m_pTbContext->StopTime(TOTAL_TIME);
}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::FormReport()
{
	GetNotValidView(TRUE);

	CString strReportName = GetDefaultForm();
	CTBNamespace nsDoubleClickRep;

	if (m_pFormManager->GetReports()->GetCurrentDefaultReport() && m_pFormManager->GetReports())
		nsDoubleClickRep = m_pFormManager->GetReports()->GetCurrentDefaultReport()->GetNamespace();

	CTBNamespace nsDefaultRep = AfxGetDocumentDefaultReport(GetNamespace().ToString());

	if ((nsDoubleClickRep != nsDefaultRep) || strReportName.IsEmpty())
	{
		OnSelReport(0);
		return;
	}
	DoRunReport(strReportName);
}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::Radar()
{
	if (AfxIsRemoteInterface())
	{
		//CDocumentSession* pSession = (CDocumentSession*)AfxGetThreadContext()->m_pDocSession;
		//if (!pSession)
		//{
		//	ASSERT(FALSE);
		//	return;
		//}

		//TODO manadare un boolean sul client
		//pSession->PushRadarInfoToClient(this);

		return;
	}

	if (UseWoormRadar())
		OnWrmRadar();
	else
		OnOldRadar();
}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::AltRadar()
{
	if (UseWoormRadar() || AfxIsRemoteInterface())
		OnOldRadar();
	else
		OnWrmRadar();
}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::OldRadar()
{
	if (!CanDoRadar()) return;

	// radar already attached
	if (m_pRadarDoc)
	{
		m_pRadarDoc->ActivateAndShowRadar();
		return;
	}

	m_pRadarDoc = AfxCreateTBRadar(this);
	if (AfxGetThemeManager()->ShowRadarFixed())
		m_pRadarDoc->EnableStayAlive(TRUE, FALSE);
	else
		m_pRadarDoc->EnableStayAlive(TRUE, TRUE);

	m_pRadarDoc->StartReady();
}

//-----------------------------------------------------------------------------
BOOL CAbstractFormDoc::AsQuery()
{
	//F4: query di ricerca specifica
	if (!CanDoQuery()) return FALSE;
	CString sSearchQuery(QueryNames::SEARCH_QUERYNAME());
	return m_pQueryManager->IsExistQuery(sSearchQuery);
}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::Query()
{
	//F4: query di ricerca specifica
	if (!CanDoQuery()) return;

	CString sSearchQuery(QueryNames::SEARCH_QUERYNAME());
	if (!AsQuery())
	{
		Message(cwsprintf(_TB("The query {0-%s} is not defined. You can create it using the required button on the toolbar. "), (LPCTSTR)sSearchQuery));
		return;
	}
	m_pQueryManager->SelectQuery(sSearchQuery);
}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::ExecQuery()
{
	if (!CanDoExecQuery())
		return;

	if (m_pQueryManager->ExecSelectedQuery(FALSE, FALSE) && m_pBrowser->LastRecord())
	{
		m_pBrowser->SetInvalidQuery(FALSE);
		BrowseRecord();
		return;
	}
	if (m_bWarnWhenEmptyQuery)
		Message(_TB("No documents have been selected by the set query."));

	InitData();
	ClearCurrentRecord();
	GoInBrowseMode();
}

// Riaggiorna i dati (utilizzato in caso di cursore statico e keyset)
//--------------------------------------------------------------------------------
void CAbstractFormDoc::RefreshRowset()
{
	if (!CanDoRefreshRowset())
		return;

	OnExecQuery();
}

// Riaggiorna i dati (utilizzato in caso di cursore statico e keyset)
//--------------------------------------------------------------------------------
void CAbstractFormDoc::Customize()
{
	//nel caso di macrorecorder attivo disabilita il recording per il tempo in cui � su il report 
	CApplicationContext::MacroRecorderStatus localStatus = AfxGetApplicationContext()->m_MacroRecorderStatus;
	AfxGetApplicationContext()->m_MacroRecorderStatus = CApplicationContext::IDLE;

	DoCustomize();

	//ripristina lo stato del macrorecorder
	AfxGetApplicationContext()->m_MacroRecorderStatus = localStatus;
}

//--------------------------------------------------------------------------------
void CAbstractFormDoc::NewWrmRadar()
{
	if (AfxIsRemoteInterface() || !CanDoRunWrmRadar())
		OnOldRadar();
	else
		OnFileWrmRadar(1);
}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::ExecSelQuery(CString strQuery)
{
	if (!CanDoExecQuery())
		return;

	if (strQuery.IsEmpty())
		return;

	m_pQueryManager->SelectQuery(strQuery);
}

//--------------------------------------------------------------------------------
void CAbstractFormDoc::EditQuery()
{
	if (!CanDoEditQuery())
		return;

	m_pQueryManager->EditQuery(!OSL_CAN_DO(GetInfoOSL(), OSL_GRANT_EDITQUERY));
}

//--------------------------------------------------------------------------------
void CAbstractFormDoc::OtherQuery()
{
	if (!OSL_CAN_DO(GetInfoOSL(), OSL_GRANT_BROWSE | OSL_GRANT_BROWSE_EXTENDED))
		return;

	m_pQueryManager->EditQuery(!OSL_CAN_DO(GetInfoOSL(), OSL_GRANT_EDITQUERY));
}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::OnBackMenu()
{
	if (!GetNotValidView(TRUE))
		::PostMessage(AfxGetMenuWindowHandle(), UM_ACTIVATE_MENU, (WPARAM)GetFrameHandle(), NULL);
}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::OnSwitchTo()
{
	if (!AfxGetThemeManager()->HasStatusBar()) return;

	CMenu menu;
	menu.CreatePopupMenu();
	GetMasterFrame()->MakeSwitchTomenu(&menu);
	CPoint point = GetMasterFrame()->GetPositionSwitchTo();
	CMasterFrame* pFrame = GetMasterFrame();
	if (pFrame == NULL) return;
	CSize  sizeX;
	sizeX.cx = 0; sizeX.cy = 22;
	// aproxymate position
	CONST UINT space = 10;

	int y1 = point.y - (menu.GetMenuItemCount() * sizeX.cy + space);
	menu.TrackPopupMenu(TPM_LEFTBUTTON, point.x, y1, pFrame);
}

// Open Dialog Log 
//-----------------------------------------------------------------------------
void CAbstractFormDoc::OnProgBarButton()
{
	CMasterFrame* pFrame = GetMasterFrame();
	if (pFrame == NULL) return;
	CTaskBuilderStatusBar* pStatusBar = pFrame->m_pStatusBar;
	if (!pStatusBar) return;
	int nID = ID_STATUS_PROGBAR_RANGE_START;
	CWnd* pWndCtrl = pStatusBar->GetDlgItem(nID);
	CRect rectButton;
	CRect	rectStatus;
	pStatusBar->GetWindowRect(rectStatus);
	pWndCtrl->GetWindowRect(rectButton);
	// Show the dialog 
	CWnd* pWndLogDialog = m_pMessages->ShowNoModal(rectButton.left, rectStatus.top);
	if (pWndLogDialog)
	{
		pWndCtrl = pStatusBar->GetDlgItem(IDC_PROGBAR_BUTTON);
		if (pWndCtrl) {
			CTBProgressBarButton* pButt = dynamic_cast<CTBProgressBarButton*>(pWndCtrl);
			if (pButt) {
				pButt->SetDialog(pWndLogDialog);
			}
		}
	}
}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::OnNewRecord() { NewRecord(); }
void CAbstractFormDoc::OnFindRecord() { FindRecord(); }
void CAbstractFormDoc::OnDeleteRecord() { DeleteRecord(); }
void CAbstractFormDoc::OnSaveRecord() { SaveRecord(); }

//-----------------------------------------------------------------------------
void CAbstractFormDoc::OnActionsCopy()
{
	GetNotValidView(TRUE);

	CString strKey;
	if (GetMaster() && GetMaster()->GetRecord() && !GetMaster()->GetRecord()->IsEmpty())
		strKey = GetMaster()->GetRecord()->GetPrimaryKeyNameValue();

	//per pura estetica rimuovo ultimo puntoevirgola;
	strKey.TrimRight(';');

	CString strLink = ::GetTBNavigateUrl(GetNamespace().ToString(), strKey);

	if (CopyTBLinkToClipboard(strLink))
		((CMasterFrame*)GetFrame())->SetStatusBarText(_TB("The document link has been succesfully copied to clipboard."));
	else
		((CMasterFrame*)GetFrame())->SetStatusBarText(_TB("There was an error copying the document link to clipboard."));

}

// Se parto da stato di browse il comportamento e' standard, cioe` faccio direttamente
// la query di ricerca perche' modifico quello che e` corrente. Se invece sono
// in stato di FIND (F3) controllo che i campi di chiave prinaria siano tutti valorizzati.
// Se lo sono faccio il comportamento standard altrimenti lancio la ricerca generica
// esattamente come se avessi schiacciato F7 al posto di CTRL-ENTER
//-----------------------------------------------------------------------------
void CAbstractFormDoc::OnEditRecord()
{
	BOOL bRadardFind = FALSE;
	if (GetFormMode() == FIND && m_pDBTMaster)
	{
		SqlRecord* pRec = m_pDBTMaster->GetRecord();
		if (pRec && pRec->GetSize() > 0)
		{
			for (int j = 0; j <= pRec->GetUpperBound(); j++)
			{
				const SqlColumnInfo* pColumnInfo = pRec->GetColumnInfo(j);
				if (!pColumnInfo->m_bSpecial)
					continue;

				// per fare la query diretta devo obbligatoriamente avere
				// in campi di chiave primaria editabili e findabili. Altrimenti
				// lancio il radar sulla base dei soli findable (user modified)
				DataObj* pDataObj = pRec->GetDataObjAt(j);
				if (!pDataObj->IsFindable())
				{
					bRadardFind = TRUE;
					break;
				}
			}
		}

	}

	ToolBarButtonsHideGhost(0);

	// se non tutti i campi di chiave primaria sono valorizzati chiamo il radar
	if (bRadardFind && CanDoRadar())
	{
		OnRadar();
		return;
	}

	if (
		GetFormMode() == FIND &&
		m_bUseEasyBrowsing
		)
	{
		GetNotValidView(TRUE);
		SetFormMode(BROWSE);
		BrowseRecord();
	}
	else
		EditRecord();
}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::OnEscape()
{
	if (GetType() != VMT_BATCH)
	{
		if (Escape())
		{
			ToolBarButtonsHideGhost(1);
		}
		return;
	}

	ToolBarButtonsHideGhost(1);

	if (m_bBatchRunning) BatchStop();
}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::UndoChanges()
{
	OnEscape();
}

//-----------------------------------------------------------------------------
BOOL CAbstractFormDoc::CanShowInOpenDocuments()
{
	return !IsBackgroundMode() && !IsInUnattendedMode();
}

//-----------------------------------------------------------------------------
BOOL CAbstractFormDoc::CanDoFirstRecord()
{
	BOOL bCanDo =
		GetType() != VMT_BATCH &&
		m_pDBTMaster			&&
		GetFormMode() == BROWSE &&
		m_pBrowser && m_pBrowser->CanDoFirstRecord();

	if (bCanDo && !OSL_CAN_DO(GetInfoOSL(), OSL_GRANT_BROWSE))
		return FALSE;

	return bCanDo;
}

//-----------------------------------------------------------------------------
BOOL CAbstractFormDoc::CanDoPrevRecord()
{
	BOOL bCanDo =
		GetType() != VMT_BATCH &&
		m_pDBTMaster			&&
		GetFormMode() == BROWSE &&
		m_pBrowser && m_pBrowser->CanDoPrevRecord();

	if (bCanDo && !OSL_CAN_DO(GetInfoOSL(), OSL_GRANT_BROWSE))
		return FALSE;

	return bCanDo;
}

//-----------------------------------------------------------------------------
BOOL CAbstractFormDoc::CanDoNextRecord()
{
	BOOL bCanDo =
		GetType() != VMT_BATCH &&
		m_pDBTMaster && AfxIsValidAddress(m_pDBTMaster, sizeof(DBTMaster)) &&
		GetFormMode() == BROWSE &&
		m_pBrowser && m_pBrowser->CanDoNextRecord();

	if (bCanDo && !OSL_CAN_DO(GetInfoOSL(), OSL_GRANT_BROWSE))
		return FALSE;

	return bCanDo;
}

//-----------------------------------------------------------------------------
BOOL CAbstractFormDoc::CanDoLastRecord()
{
	BOOL bCanDo =
		GetType() != VMT_BATCH &&
		m_pDBTMaster && AfxIsValidAddress(m_pDBTMaster, sizeof(DBTMaster)) &&
		GetFormMode() == BROWSE &&
		m_pBrowser && m_pBrowser->CanDoLastRecord();

	if (bCanDo && !OSL_CAN_DO(GetInfoOSL(), OSL_GRANT_BROWSE))
		return FALSE;

	return bCanDo;
}

//-----------------------------------------------------------------------------
BOOL CAbstractFormDoc::CanDoFindRecord()
{
	BOOL bCanDo = GetType() != VMT_BATCH && m_pDBTMaster && !m_bOnlyOneRecord && GetFormMode() == BROWSE;

	if (bCanDo && !OSL_CAN_DO(GetInfoOSL(), OSL_GRANT_BROWSE | OSL_GRANT_BROWSE_EXTENDED))
		return FALSE;

	return bCanDo;
}

//-----------------------------------------------------------------------------
BOOL CAbstractFormDoc::CanDoEscape()
{
	return !m_pTbContext->TransactionPending();
}

//-----------------------------------------------------------------------------
BOOL CAbstractFormDoc::CanDoReport()
{
	return
		!m_bOnlyOneRecord &&
		!m_pTbContext->TransactionPending() &&
		(!m_bBatchRunning || !m_bUnattendedMode)  /*&&
		m_pFormManager &&
		m_pFormManager->GetIndexReportDefault() > -1*/;
}

//-----------------------------------------------------------------------------
BOOL CAbstractFormDoc::CanDoRadar()
{
	return (
		GetType() != VMT_BATCH &&
		m_pDBTMaster &&
		!m_bOnlyOneRecord &&
		!m_pTbContext->TransactionPending() &&
		OSL_CAN_DO(GetInfoOSL(), OSL_GRANT_BROWSE | OSL_GRANT_BROWSE_EXTENDED)
		);
}

//-----------------------------------------------------------------------------
BOOL CAbstractFormDoc::CanDoExecQuery()
{
	BOOL bCanDo =
		GetType() != VMT_BATCH &&
		m_pDBTMaster && AfxIsValidAddress(m_pDBTMaster, sizeof(DBTMaster)) &&
		!m_bOnlyOneRecord &&
		m_pBrowser		&&
		GetFormMode() == BROWSE;

	if (bCanDo && !OSL_CAN_DO(GetInfoOSL(), OSL_GRANT_BROWSE))
		return FALSE;

	return bCanDo;
}

//-----------------------------------------------------------------------------
BOOL CAbstractFormDoc::CanDoEditQuery()
{
	BOOL bCanDo =
		GetType() != VMT_BATCH &&
		m_pDBTMaster && AfxIsValidAddress(m_pDBTMaster, sizeof(DBTMaster)) &&
		m_pBrowser &&
		!m_bOnlyOneRecord &&
		GetFormMode() == BROWSE;

	if
		(
			bCanDo
			&&
			!OSL_CAN_DO(GetInfoOSL(), OSL_GRANT_BROWSE | OSL_GRANT_BROWSE_EXTENDED | OSL_GRANT_EDITQUERY)
			)
		return FALSE;

	return bCanDo;
}

//-----------------------------------------------------------------------------
BOOL CAbstractFormDoc::CanDoQuery()
{
	BOOL bCanDo =
		m_pBrowser &&
		!m_bOnlyOneRecord				&&
		GetFormMode() == BROWSE &&
		!QueryNames::SEARCH_QUERYNAME().IsEmpty();

	if (bCanDo && !OSL_CAN_DO(GetInfoOSL(), OSL_GRANT_BROWSE))
		return FALSE;

	return bCanDo;
}

//-----------------------------------------------------------------------------
BOOL CAbstractFormDoc::CanDoRefreshRowset()
{
	return CanDoExecQuery();
}

//-----------------------------------------------------------------------------
BOOL CAbstractFormDoc::CanDoCustomize()
{
	BOOL bCanDo = m_pFormManager && !m_strFormName.IsEmpty() && !m_pTbContext->TransactionPending();
	if (bCanDo && !OSL_CAN_DO(GetInfoOSL(), OSL_GRANT_CUSTOMIZEFORM))
		return FALSE;

	return bCanDo;
}
//-----------------------------------------------------------------------------
BOOL CAbstractFormDoc::CanDoNewRecord()
{
	BOOL bCanDo =
		GetType() != VMT_BATCH &&
		m_pDBTMaster && AfxIsValidAddress(m_pDBTMaster, sizeof(DBTMaster)) &&
		m_pBrowser && m_pBrowser->IsValid() &&
		GetFormMode() == BROWSE;

	if (bCanDo && !OSL_CAN_DO(GetInfoOSL(), OSL_GRANT_NEW))
		return FALSE;

	return bCanDo;
}

//-----------------------------------------------------------------------------
BOOL CAbstractFormDoc::CanDoEditRecord()
{
	BOOL bCanDo =
		GetType() != VMT_BATCH &&
		m_pDBTMaster && AfxIsValidAddress(m_pDBTMaster, sizeof(DBTMaster)) &&
		(GetFormMode() == BROWSE || GetFormMode() == FIND) &&
		(ValidCurrentRecord() || GetFormMode() == FIND);

	if (bCanDo && !OSL_CAN_DO(GetInfoOSL(), OSL_GRANT_EDIT))
		return FALSE;

	return bCanDo;
}

//-----------------------------------------------------------------------------
BOOL CAbstractFormDoc::CanDoDeleteRecord()
{
	BOOL bCanDo =
		GetType() != VMT_BATCH &&
		m_pDBTMaster && AfxIsValidAddress(m_pDBTMaster, sizeof(DBTMaster)) &&
		GetFormMode() == BROWSE &&
		!m_pTbContext->TransactionPending() &&
		ValidCurrentRecord();

	if (bCanDo && !OSL_CAN_DO(GetInfoOSL(), OSL_GRANT_DELETE))
		return FALSE;

	return bCanDo;
}

//-----------------------------------------------------------------------------
BOOL CAbstractFormDoc::CanDoSaveRecord()
{
	return GetType() != VMT_BATCH && m_pDBTMaster && (GetFormMode() == NEW || GetFormMode() == EDIT) && !m_pTbContext->TransactionPending();
}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::OnUpdateNewRecord(CCmdUI* pCmdUI) { pCmdUI->Enable(DispatchCanDoNewRecord()); }
void CAbstractFormDoc::OnUpdateEditRecord(CCmdUI* pCmdUI) { pCmdUI->Enable(DispatchCanDoEditRecord()); }
void CAbstractFormDoc::OnUpdateFindRecord(CCmdUI* pCmdUI) { pCmdUI->Enable(CanDoFindRecord()); }
void CAbstractFormDoc::OnUpdateDeleteRecord(CCmdUI* pCmdUI) { pCmdUI->Enable(DispatchCanDoDeleteRecord()); }
void CAbstractFormDoc::OnUpdateReport(CCmdUI* pCmdUI) { pCmdUI->Enable(CanDoReport()); }
void CAbstractFormDoc::OnUpdateQuery(CCmdUI* pCmdUI) { pCmdUI->Enable(CanDoQuery()); }
void CAbstractFormDoc::OnUpdateSaveRecord(CCmdUI* pCmdUI) { pCmdUI->Enable(CanDoSaveRecord()); }
void CAbstractFormDoc::OnUpdateTabSwitch(CCmdUI* pCmdUI) { pCmdUI->Enable(TRUE); }
void CAbstractFormDoc::OnUpdateEscape(CCmdUI* pCmdUI)
{
	FormMode eFormMode = GetFormMode();
	BOOL bEnable = CanDoEscape() && (eFormMode == NEW || eFormMode == EDIT || eFormMode == FIND);
	pCmdUI->Enable(bEnable);
}

void CAbstractFormDoc::OnUpdateRadar(CCmdUI* pCmdUI)
{
	FormMode eFormMode = GetFormMode();
	BOOL bEnable = CanDoRadar() && eFormMode != EDIT && eFormMode != NEW;
	pCmdUI->Enable(bEnable);
}
void CAbstractFormDoc::OnUpdateGenerateJsonModel(CCmdUI* pCmdUI) { pCmdUI->Enable(GetFormMode() == CBaseDocument::BROWSE); }
void CAbstractFormDoc::OnUpdateCustomize(CCmdUI* pCmdUI) { pCmdUI->Enable(CanDoCustomize()); }
void CAbstractFormDoc::OnUpdateExecQuery(CCmdUI* pCmdUI) { pCmdUI->Enable(CanDoExecQuery()); }
void CAbstractFormDoc::OnUpdateEditQuery(CCmdUI* pCmdUI) { pCmdUI->Enable(CanDoEditQuery()); }
void CAbstractFormDoc::OnUpdateRefreshRowset(CCmdUI* pCmdUI) { pCmdUI->Enable(CanDoRefreshRowset()); }
void CAbstractFormDoc::OnUpdateFormHelp(CCmdUI* pCmdUI) { pCmdUI->Enable(TRUE); }
//void CAbstractFormDoc::OnUpdateSwitchTo		(CCmdUI* pCmdUI)	{ pCmdUI->Enable(TRUE); }
void CAbstractFormDoc::OnBarButton(CCmdUI* pCmdUI) { pCmdUI->Enable(TRUE); }

//-----------------------------------------------------------------------------
void CAbstractFormDoc::OnFormHelp()
{
	FormHelp();
}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::OnCrash()
{
	CAbstractFormDoc* pDoc = NULL;
	pDoc->Activate();
}
//-----------------------------------------------------------------------------
void CAbstractFormDoc::OnFavorites()
{
	CMasterFrame* pMF = CAbstractFormDoc::GetMasterFrame();
	if (!pMF)
		return;

	CTBTabbedToolbar* pTabbedToolBar = pMF->GetTabbedToolBar();
	if (pTabbedToolBar == NULL) return;
	CTBToolBar* pToolBar = pTabbedToolBar->FindToolBar(ID_EXTDOC_FAVORITES);
	if (pToolBar == NULL) return;

	m_bFavoritesState = !m_bFavoritesState;

	pToolBar->SetButtonInfo(ID_EXTDOC_FAVORITES, TBBS_BUTTON, m_bFavoritesState ? TBIcon(szIconFavoritesPin, TOOLBAR) : TBIcon(szIconFavorites, TOOLBAR));

}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::OnUpdateFirstRecord(CCmdUI* pCmdUI)
{
	BOOL bEnableOnlyOne = !m_bOnlyOneRecord || !m_pDBTMaster || !m_pDBTMaster->GetRecord();
	CMasterFrame* pMF = CAbstractFormDoc::GetMasterFrame();
	if (!pMF) return;
	CTBTabbedToolbar* pTabbedToolBar = pMF->GetTabbedToolBar();
	if (pTabbedToolBar->GetAutoHideToolBarButton() && GetFormMode() == BROWSE)
	{
		pCmdUI->Enable(bEnableOnlyOne);
		return;
	}
	pCmdUI->Enable(CanDoFirstRecord() && bEnableOnlyOne);
}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::OnUpdatePrevRecord(CCmdUI* pCmdUI)
{
	CMasterFrame* pMF = CAbstractFormDoc::GetMasterFrame();
	if (!pMF) return;
	CTBTabbedToolbar* pTabbedToolBar = pMF->GetTabbedToolBar();
	BOOL bEnableOnlyOne = !m_bOnlyOneRecord || !m_pDBTMaster || !m_pDBTMaster->GetRecord();
	if (pTabbedToolBar->GetAutoHideToolBarButton() && GetFormMode() == BROWSE)
	{
		pCmdUI->Enable(bEnableOnlyOne);
		return;
	}
	pCmdUI->Enable(CanDoPrevRecord() && bEnableOnlyOne);
}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::OnUpdateNextRecord(CCmdUI* pCmdUI)
{
	CMasterFrame* pMF = CAbstractFormDoc::GetMasterFrame();
	if (!pMF) return;
	CTBTabbedToolbar* pTabbedToolBar = pMF->GetTabbedToolBar();
	BOOL bEnableOnlyOne = !m_bOnlyOneRecord || !m_pDBTMaster || !m_pDBTMaster->GetRecord();
	if (pTabbedToolBar->GetAutoHideToolBarButton() && GetFormMode() == BROWSE)
	{
		pCmdUI->Enable(bEnableOnlyOne);
		return;
	}
	pCmdUI->Enable(CanDoNextRecord() && bEnableOnlyOne);
}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::OnUpdateLastRecord(CCmdUI* pCmdUI)
{
	CMasterFrame* pMF = CAbstractFormDoc::GetMasterFrame();
	if (!pMF) return;
	CTBTabbedToolbar* pTabbedToolBar = pMF->GetTabbedToolBar();
	BOOL bEnableOnlyOne = !m_bOnlyOneRecord || !m_pDBTMaster || !m_pDBTMaster->GetRecord();
	if (pTabbedToolBar->GetAutoHideToolBarButton() && GetFormMode() == BROWSE)
	{
		pCmdUI->Enable(bEnableOnlyOne);
		return;
	}
	pCmdUI->Enable(CanDoLastRecord() && bEnableOnlyOne);
}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::OnUpdateFormModeIndicator(CCmdUI* pCmdUI)
{
	pCmdUI->Enable();

	CString msg;
	switch (GetFormMode())
	{
	case BROWSE: msg = _TB("Ready");	break;
	case NEW: msg = _TB("New");		break;
	case EDIT: msg = _TB("Edit");	break;
	case FIND: msg = _TB("Find");	break;
	default: msg = _TB("Error");	break;
	}
	pCmdUI->SetText(msg);
}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::OnUpdateMessageIndicator(CCmdUI* pCmdUI)
{
	pCmdUI->Enable();

	CString msg;
	if (DispatchOnShowStatusBarMsg(msg))
		pCmdUI->SetText(msg);
}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::OnUpdateTotalRecordsIndicator(CCmdUI* pCmdUI)
{
	// Il numero di record non e' stato determinato
	if (!m_pBrowser || m_pBrowser->m_pTable->InvalidCounter())
	{
		pCmdUI->Enable(FALSE);
		return;
	}

	pCmdUI->Enable();
	pCmdUI->SetText(cwsprintf
	(
		_T("%05ld/%05ld"),
		m_pBrowser->m_pTable->GetCurrRecordNo(),
		m_pBrowser->m_pTable->GetRecordCount()
	));
}


//-----------------------------------------------------------------------------
void CAbstractFormDoc::OnUpdateDeletedIndicator(CCmdUI* pCmdUI)
{
	if (!m_pDBTMaster || !m_pDBTMaster->GetTable()->IsDeleted())
	{
		pCmdUI->Enable(FALSE);
		return;
	}

	pCmdUI->Enable();
	pCmdUI->SetText(cwsprintf(ID_EXTDOC_DELETED_RECORDS_INDICATORS));
}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::OnUpdateDisactiveIndicator(CCmdUI* pCmdUI)
{
	if (!OnDisactiveIndicator())
	{
		pCmdUI->Enable(FALSE);
		return;
	}

	pCmdUI->Enable();
	pCmdUI->SetText(cwsprintf(ID_EXTDOC_ACTIVE_INDICATORS));
}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::OnUpdateSelReport(CCmdUI* pCmdUI)
{
	ASSERT(m_pFormManager);

	// se nID == 0 utilizza il report assegnato come default se esiste, altrimenti ne crea uno
	// altrimenti arriva dal menu dropdown del pulsante Radar della toolbar

	CFunctionDescription* fd =
		(pCmdUI->m_nID ?
			fd = m_pFormManager->GetReportDescription(pCmdUI->m_nID - ID_EXTDOC_DDTB_ENUMREPORT0)
			:
			fd = m_pFormManager->GetReportDescription()
			);

	if (fd)
	{
		CString strReportName = fd->GetNamespace().ToString();
		pCmdUI->Enable(ReportIsEnabled(strReportName));
	}
	else
		pCmdUI->Enable(TRUE);
}

//-----------------------------------------------------------------------------
BOOL CAbstractFormDoc::GoInBrowserMode(const CString& sPK)
{
	SUSPEND_UPDATE_DATA_VIEW();

	SqlRecord* pMasterRec = m_pDBTMaster->GetRecord();
	pMasterRec->Init();

	LPCTSTR szFormModeParamName = _T("_FormMode");
	TCHAR* ps = ::StrStrIW(sPK, szFormModeParamName);
	if (!ps)
	{
		pMasterRec->SetPrimaryKeyNameValue(sPK); //valorizza le chiavi primarie a partire da una stringa con formato fieldName:FieldValue;fieldName1:fieldValue1

		OnRadarRecordSelected(FALSE);

		return TRUE;
	}

	int idx = _tcsclen(szFormModeParamName) + 1;
	ps += idx;
	TCHAR* f = _tcschr(ps, ';');
	CString sMode = sPK.Mid(idx, f - ps);

	if (sMode.CompareNoCase(L"NEW") == 0)
	{
		OnNewRecord();

		//la richiamo per valorizzare eventuali altre colonne diverse da quelle di chiave
		pMasterRec->SetColumnNameValue(sPK, this);

		UpdateDataView(); 
		return TRUE;
	}
	else if (sMode.CompareNoCase(L"EDIT") == 0)
	{
		pMasterRec->SetColumnNameValue(sPK);

		OnRadarRecordSelected(FALSE);

		OnEditRecord();

		//la richiamo per valorizzare eventuali altre colonne diverse da quelle di chiave
		pMasterRec->SetColumnNameValue(sPK, this);

		UpdateDataView();
		return TRUE;
	}
	return FALSE;
}

//-----------------------------------------------------------------------------
BOOL CAbstractFormDoc::GoInBrowserMode(CFunctionDescription* pFDI)
{
	if (!m_pDBTMaster || !pFDI || pFDI->GetParameters().GetSize() == 0)
	{
		return FALSE;
	}

	//TODO
	CDataObjDescription* pK = pFDI->GetParamDescription(_T("_DocumentPK"));
	if (pK && pK->GetValue())
	{
		return GoInBrowserMode(pK->GetValue()->Str());
	}
	//----

	SqlRecord* pMasterRec = m_pDBTMaster->GetRecord();
	pMasterRec->Init();

	for (int i = 0; i < pFDI->GetParameters().GetSize(); i++)
	{
		CDataObjDescription* par = pFDI->GetParamDescription(i);
		DataObj* pObj = pMasterRec->GetDataObjFromColumnName(par->GetName());
		if (pObj)
		{
			pObj->Assign(*par->GetValue());
		}
	}

	OnRadarRecordSelected(FALSE);
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CAbstractFormDoc::GoInBrowserMode(const SqlRecord* pRec)
{
	if (!m_pDBTMaster || pRec == NULL || m_pDBTMaster->GetRecord() == NULL)
	{
		return FALSE;
	}

	SqlRecord* pMasterRec = m_pDBTMaster->GetRecord();
	pMasterRec->Init();

	for (int i = 0; i < pMasterRec->GetNumberSpecialColumns(); i++)
	{
		DataObj* pTarget = pMasterRec->GetSpecialColumn(i)->GetDataObj();
		ASSERT(pTarget);

		//SqlRecordItem* pRecItem = pRec->GetSpecialColumn(i);
		//DataObj* pSrc = pRecItem ? pRecItem->GetDataObj() : NULL;
		DataObj* pSrc = i < pRec->GetNumberSpecialColumns() ? pRec->GetSpecialColumn(i)->GetDataObj() : NULL;
		if (pSrc == NULL)
		{
			CString sName = pMasterRec->GetSpecialColumn(i)->GetColumnName();
			pSrc = const_cast<SqlRecord*>(pRec)->GetDataObjFromColumnName(sName);
			if (pSrc == NULL)
			{
				TRACE(_TB("Cannot find primary key segment named %s\n"), pMasterRec->GetSpecialColumn(i)->GetColumnName());
				ASSERT(FALSE);
				break;
			}
		}
		pTarget->Assign(*pSrc);
	}

	OnRadarRecordSelected(FALSE);
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CAbstractFormDoc::SetXMLDocInfo(CXMLDocInfo* pXMLDocInfo)
{
	if (pXMLDocInfo)
	{
		if (m_pXMLDocInfo)
		{
			delete m_pXMLDocInfo;
			m_pXMLDocInfo = NULL;
		}

		m_pXMLDocInfo = pXMLDocInfo;
	}
	else
		return FALSE;

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CAbstractFormDoc::SetXMLDocInfo(const CXMLDocInfo& aXMLDocInfo)
{
	if (m_pXMLDocInfo)
		*m_pXMLDocInfo = aXMLDocInfo;

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CAbstractFormDoc::DispatchOnPostCreateClient(CBodyEdit* pBody)
{
	if (!OnPostCreateClient(pBody))
		return FALSE;

	return m_pClientDocs->OnPostCreateClient(pBody);
}

//------------------------------------------------------------------------------
BOOL CAbstractFormDoc::DispatchOnGetCustomColor(const CBodyEdit* pBody, CBodyEditRowSelected* pCurRow)
{
	// FALSE is OnGetCustomColor default value
	BOOL  bOk = OnGetCustomColor(pBody, pCurRow);
	return
		m_pClientDocs->OnGetCustomColor(pBody, pCurRow) || bOk;
}

//------------------------------------------------------------------------------
BOOL CAbstractFormDoc::DispatchOnDblClick
(
	const CBodyEdit* pBody,
	UINT nFlags,
	CBodyEditRowSelected* pCurRow
)
{
	// FALSE is OnDblClick default value
	BOOL  bOk = OnDblClick(pBody, nFlags, pCurRow);
	return m_pClientDocs->OnDblClick(pBody, nFlags, pCurRow) || bOk;
}

//------------------------------------------------------------------------------
BOOL CAbstractFormDoc::DispatchOnEnableTabSelChanging(UINT nTabber, UINT nFromIDD, UINT nToIDD)
{
	return
		OnEnableTabSelChanging(nTabber, nFromIDD, nToIDD) &&
		m_pClientDocs->OnEnableTabSelChanging(nTabber, nFromIDD, nToIDD);
}

//--------------------------------------------------------------------------------
BOOL CAbstractFormDoc::DispatchOnAfterAttachData()
{
	return OnAfterOnAttachData() && m_pClientDocs->OnAfterOnAttachData();
}

//------------------------------------------------------------------------------
void CAbstractFormDoc::DispatchOnTabSelChanged(UINT nTabber, UINT nTabIDD)
{
	if (GetType() == VMT_BATCH)
	{
		// Ad ogni commutazione di tabber riaggiorna lo stato dei bottoni batch
		// nel caso che siano integrati nelle singole dialog del tabber
		POSITION pos = GetFirstViewPosition();
		while (pos != NULL)
		{
			CView* pView = GetNextView(pos);
			ASSERT_VALID(pView);

			if (pView->IsKindOf(RUNTIME_CLASS(CAbstractFormView)))
			{
				((CAbstractFormView*)pView)->SetBatchViewButtonState();
			}
		}
	}

	OnTabSelChanged(nTabber, nTabIDD);
	m_pClientDocs->OnTabSelChanged(nTabber, nTabIDD);
}

//---------------------------------------------------------------------------
BOOL  CAbstractFormDoc::DispatchOnShowingBodyEditContextMenu(CBodyEdit* pBodyEdit, CMenu* pMenu, int nCol, int nRow, CPoint ptClient)
{
	return m_pClientDocs->OnShowingBodyEditContextMenu(pBodyEdit, pMenu, nCol, nRow, ptClient);
}

//---------------------------------------------------------------------------
void CAbstractFormDoc::DispatchOnModifyDBTDefineQuery(DBTObject* pDBTObj, SqlTable* pTable)
{
	OnModifyDBTDefineQuery(pDBTObj, pTable);
	m_pClientDocs->OnModifyDBTDefineQuery(pDBTObj, pTable);
}

//---------------------------------------------------------------------------
void CAbstractFormDoc::DispatchOnModifyDBTPrepareQuery(DBTObject* pDBTObj, SqlTable*pTable)
{
	OnModifyDBTPrepareQuery(pDBTObj, pTable);
	m_pClientDocs->OnModifyDBTPrepareQuery(pDBTObj, pTable);
}

//---------------------------------------------------------------------------
void CAbstractFormDoc::DispatchOnAfterCreateAndInitDBT(DBTObject* pDBTObj)
{
	OnAfterCreateAndInitDBT(pDBTObj);
	m_pClientDocs->OnAfterCreateAndInitDBT(pDBTObj);
}


//---------------------------------------------------------------------------
void CAbstractFormDoc::DispatchOnPrepareBrowser(SqlTable*pTable)
{
	OnModifyPrepareBrowser(pTable);
	m_pClientDocs->OnPrepareBrowser(pTable);
}

//---------------------------------------------------------------------------
void CAbstractFormDoc::DispatchOnPrepareFindQuery(SqlTable*pTable)
{
	OnModifyPrepareFindQuery(pTable);
	m_pClientDocs->OnPrepareFindQuery(pTable);
}

//---------------------------------------------------------------------------
void CAbstractFormDoc::DispatchOnDuringBatchExecute(SqlRecord* pCurrentRecord)
{
	m_pClientDocs->OnDuringBatchExecute(pCurrentRecord);
}

//---------------------------------------------------------------------------
void CAbstractFormDoc::DispatchOnLoadAttachedDocument(CFinderDoc* pFinderDoc)
{
	m_pClientDocs->OnLoadAttachedDocument(pFinderDoc);
}

//---------------------------------------------------------------------------
BOOL CAbstractFormDoc::DispatchOnShowingPopupMenu(UINT nIDC, CMenu* pMenu)
{
	if (pMenu)
		return m_pClientDocs->OnShowingPopupMenu(nIDC, pMenu);

	return TRUE;
}

//---------------------------------------------------------------------------
void CAbstractFormDoc::DispatchOnModifyHKLDefineQuery
(
	HotKeyLink* pHKL,
	SqlTable*   pTable,
	HotKeyLink::SelectionType nQuerySelection /* = DIRECT_ACCESS*/
)
{
	OnModifyHKLDefineQuery(pHKL, pTable, nQuerySelection);
	m_pClientDocs->OnModifyHKLDefineQuery(pHKL, pTable, nQuerySelection);

}

//---------------------------------------------------------------------------
void CAbstractFormDoc::DispatchOnModifyHKLPrepareQuery
(
	HotKeyLink* pHKL,
	SqlTable*	pTable,
	DataObj*	pData,
	HotKeyLink::SelectionType nQuerySelection /* = DIRECT_ACCESS*/
)
{
	OnModifyHKLPrepareQuery(pHKL, pTable, pData, nQuerySelection);
	m_pClientDocs->OnModifyHKLPrepareQuery(pHKL, pTable, pData, nQuerySelection);
}

//---------------------------------------------------------------------------
BOOL CAbstractFormDoc::DispatchOnHKLIsValid(HotKeyLink* pHKL)
{
	return m_pClientDocs->OnHKLIsValid(pHKL);

}
//---------------------------------------------------------------------------
void CAbstractFormDoc::OnPrepareForFind(HotKeyLinkObj* pHKL, SqlRecord* pRec)
{
	__super::OnPrepareForFind(pHKL, pRec);
	m_pClientDocs->OnPrepareForFind(pHKL, pRec);
	for (int i = 0; i < m_ControlBehaviours.GetCount(); i++)
	{
		CControlBehaviour* pBehaviour = m_ControlBehaviours[i];
		if (pBehaviour->GetHotLink() == pHKL)
		{
			pBehaviour->OnPrepareForFind(pRec);
			break;
		}
	}
}
//---------------------------------------------------------------------------
void CAbstractFormDoc::OnPrepareAuxData(HotKeyLinkObj* pHKL)
{
	__super::OnPrepareAuxData(pHKL);
	m_pClientDocs->OnPrepareAuxData(pHKL);
}

//---------------------------------------------------------------------------
int CAbstractFormDoc::DispatchOnModifyHKLSearchComboQueryData
(
	HotKeyLink* pHKL,
	const int& nMaxItems, DataObjArray& pKeyData, CStringArray& arDescriptions
)
{
	int nret = OnModifyHKLSearchComboQueryData(pHKL, nMaxItems, pKeyData, arDescriptions);
	nret = max(nret, m_pClientDocs->OnModifyHKLSearchComboQueryData(pHKL, nMaxItems, pKeyData, arDescriptions));
	return nret;
}

//---------------------------------------------------------------------------
void CAbstractFormDoc::DispatchBeforeCallLink(CParsedCtrl* pCtrl)
{
	m_pClientDocs->OnBeforeCallLink(pCtrl);
}

//---------------------------------------------------------------------------
void CAbstractFormDoc::DispatchOnHotLinkRun()
{
	OnHotLinkRun();
	m_pClientDocs->OnHotLinkRun();
}

//---------------------------------------------------------------------------
void CAbstractFormDoc::DispatchOnHotLinkStop()
{
	OnHotLinkStop();
	m_pClientDocs->OnHotLinkStop();
}

//---------------------------------------------------------------------------
BOOL CAbstractFormDoc::DispatchRunReport(CWoormInfo* pwi)
{
	ASSERT(pwi && pwi->m_ReportNames.GetSize());
	return OnRunReport(pwi) && m_pClientDocs->OnRunReport(pwi);
}
//---------------------------------------------------------------------------
BOOL CAbstractFormDoc::DispatchOnValidateRadarSelection(SqlRecord* pRec, HotKeyLink* pHotKeyLink /*NULL*/)
{
	return
		OnValidateRadarSelection(pRec, pHotKeyLink) &&
		m_pClientDocs->OnValidateRadarSelection(pRec, pHotKeyLink);
}

//---------------------------------------------------------------------------
BOOL CAbstractFormDoc::DispatchOnValidateRadarSelection(SqlRecord* pRec, CTBNamespace nsHotLinkNamespace, HotKeyLink* pHotKeyLink /*NULL*/)
{
	return
		OnValidateRadarSelection(pRec, nsHotLinkNamespace, pHotKeyLink) &&
		m_pClientDocs->OnValidateRadarSelection(pRec, nsHotLinkNamespace, pHotKeyLink);
}

//---------------------------------------------------------------------------
BOOL CAbstractFormDoc::DispatchOnShowStatusBarMsg(CString& sMsg)
{
	// FALSE is OnShowStatusBarMsg default value
	BOOL bOk = OnShowStatusBarMsg(sMsg);
	return m_pClientDocs->OnShowStatusBarMsg(sMsg) || bOk;
}

//---------------------------------------------------------------------------
void CAbstractFormDoc::DispatchOnBeforeSave()
{
	OnBeforeSave();
	return m_pClientDocs->OnBeforeSave();
}

//---------------------------------------------------------------------------
void CAbstractFormDoc::DispatchOnAfterSave()
{
	OnAfterSave();
	return m_pClientDocs->OnAfterSave();
}

//---------------------------------------------------------------------------
void CAbstractFormDoc::DispatchOnBeforeDelete()
{
	OnBeforeDelete();
	return m_pClientDocs->OnBeforeDelete();
}

//---------------------------------------------------------------------------
void CAbstractFormDoc::DispatchOnAfterDelete()
{
	OnAfterDelete();
	return m_pClientDocs->OnAfterDelete();
}

//---------------------------------------------------------------------------
CParsedCtrl* CAbstractFormDoc::DispatchOnCreateParsedCtrl(UINT nIDC, CRuntimeClass* pParsedCtrlClass)
{
	CParsedCtrl* pCtrl = NULL;
	if (m_pClientDocs)
		pCtrl = m_pClientDocs->OnCreateParsedCtrl(nIDC, pParsedCtrlClass);
	if (pCtrl == NULL)
		pCtrl = OnCreateParsedCtrl(nIDC, pParsedCtrlClass);
	return pCtrl;
}


//---------------------------------------------------------------------------
void CAbstractFormDoc::NotifyCommand(UINT nID)
{
	if (GetMasterFrame()) //ha senso oppure sarebbe meglio chiamare la OnCmdMsg della view??? @@BAUZI chiedere a Marco
		GetMasterFrame()->SendMessage(WM_COMMAND, nID);
	else
		OnCmdMsg(nID, CN_COMMAND, NULL, NULL);
}


// simple attaching
//---------------------------------------------------------------------------
BOOL CAbstractFormDoc::HasClientDocToAttach(const CString& sWhenServerName, const BOOL& bIsFamily)
{
	// old fashion attaching
	if (CString(GetRuntimeClass()->m_lpszClassName).CompareNoCase(sWhenServerName) == 0)
		return TRUE;

	if (GetXmlDescription())
	{
		if (GetNamespace().GetObjectName() == sWhenServerName)
			return TRUE;

		return GetXmlDescription()->IsMyClass(sWhenServerName, !bIsFamily);
	}

	return FALSE;
}

// family attaching
//---------------------------------------------------------------------------
BOOL CAbstractFormDoc::HasClientDocToAttach(const CRuntimeClass* pServerRuntimeClass)
{
	return IsKindOf(pServerRuntimeClass);
}

//---------------------------------------------------------------------------
int CAbstractFormDoc::FireAction(const CString& strActionName, CString* pstrInputOutput)
{
	if (strActionName.IsEmpty())
	{
		ASSERT(FALSE);
		return CEventManager::FUNCTION_NOT_FOUND;
	}

	return m_pEventManager->FireAction(strActionName, pstrInputOutput);
}

//---------------------------------------------------------------------------
int CAbstractFormDoc::FireAction(const CString& strActionName, void* pVoidInputOutput/*= NULL*/)
{
	if (strActionName.IsEmpty())
	{
		ASSERT(FALSE);
		return CEventManager::FUNCTION_NOT_FOUND;
	}

	return m_pEventManager->FireAction(strActionName, pVoidInputOutput);

}

//---------------------------------------------------------------------------
int CAbstractFormDoc::FireAction(const CString& strActionName)
{
	if (strActionName.IsEmpty())
	{
		ASSERT(FALSE);
		return CEventManager::FUNCTION_NOT_FOUND;
	}

	return m_pEventManager->FireAction(strActionName);
}

//---------------------------------------------------------------------------
int CAbstractFormDoc::FireAction(const CString& strActionName, CFunctionDescription* pRDI)
{
	if (strActionName.IsEmpty())
	{
		ASSERT(FALSE);
		return CEventManager::FUNCTION_NOT_FOUND;
	}

	return m_pEventManager->FireAction(strActionName, pRDI);

}

//---------------------------------------------------------------------------
BOOL CAbstractFormDoc::ExistAction(const CString& strActionName)
{
	if (strActionName.IsEmpty())
	{
		ASSERT(FALSE);
		return FALSE;
	}

	return m_pEventManager->ExistAction(strActionName);
}

//----------------------------------------------------------------------------
void CAbstractFormDoc::GetUniversalKeyList(CStringArray& aUKList)
{
	aUKList.RemoveAll();

	CXMLDBTInfo *pDBTInfo = m_pDBTMaster ? m_pDBTMaster->GetXMLDBTInfo() : NULL;
	if (!pDBTInfo)
		return;

	CXMLUniversalKeyGroup* pUniversalKeyGroup = pDBTInfo ? pDBTInfo->GetXMLUniversalKeyGroup() : NULL;
	if (!pUniversalKeyGroup)
		return;

	for (int i = 0; i < pUniversalKeyGroup->GetSize(); i++)
	{
		CXMLUniversalKey* pUnKey = pUniversalKeyGroup->GetAt(i);
		if (!pUnKey)
		{
			ASSERT(FALSE);
			continue;
		}

		aUKList.Add(pUnKey->GetName());
	}

	return;
}

//-----------------------------------------------------------------------------
BOOL CAbstractFormDoc::GetToolTipText(UINT nID, CString& strMessage)
{
	// super-override fatta da ExternalController: se sono in definizione dei parametri di lancio del batch,
	// il tooltip dell'F9 (esegui) si trasforma in quello di salvataggio dei parametri
	if (
		nID == ID_EXTDOC_BATCH_STARTSTOP_TOOLBTN &&
		IsEditingParamsFromExternalController()
		)
	{
		strMessage = _TB("Save procedure parameters (F9)\r\nSave Parameters (F9)");
		return TRUE;
	}
	// prima chiede ai ClientDocs se vogliono fornire un tooltip dinamico
	return m_pClientDocs->OnGetToolTipText(nID, strMessage) || OnGetToolTipText(nID, strMessage);
}

//-----------------------------------------------------------------------------
BOOL CAbstractFormDoc::OnGetToolTipText(UINT nId, CString& strMessage)
{
	return FALSE;
}

//-----------------------------------------------------------------------------
CSingleExtDocTemplate*	CAbstractFormDoc::GetDocTemplateFromSqlRecordRTC(CRuntimeClass* rtcSqlRecordMaster)
{
	CSingleExtDocTemplate* pTemplate = (CSingleExtDocTemplate*)GetDocTemplate();
	if
		(
			pTemplate && pTemplate->IsValid() &&
			GetDocTemplate()->GetRuntimeClass()->IsDerivedFrom(RUNTIME_CLASS(CSingleExtDocTemplate))
			)
	{
		if
			(
				GetType() != VMT_BATCH &&
				m_pDBTMaster &&
				m_pDBTMaster->GetRecord() &&
				m_pDBTMaster->GetRecord()->GetRuntimeClass() == rtcSqlRecordMaster
				)
			return pTemplate;
	}
	return NULL;
}

//-----------------------------------------------------------------------------
//[TBWebMethod(securityhidden=true, thiscall_method=true, inEasyBuilder_method=false)]
DBTMaster* CAbstractFormDoc::GetMaster()
{
	return m_pDBTMaster;
}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::GetJsonRadarInfos(CJsonSerializer& resp, const CString& name)
{
	resp.OpenObject(_T("radarInfo"));

	SqlRecord*  m_pCallerSqlRecord = m_pDBTMaster->GetRecord();
	SqlRecord*	m_pDynamicSqlRecord = m_pCallerSqlRecord->Create();

	// uso una tabella locale perche modifico la query in maniera libera. La tabella
	// viene chiusa e distrutta in ~CRadarDoc
	SqlTable* pTable = new SqlTable(m_pDynamicSqlRecord, m_pTbContext->GetReadOnlySqlSession());

	if (!name.IsEmpty() && name.CompareNoCase(L"default"))
	{
		if (!m_pQueryManager->SelectQuery(name))
			PrepareRadarQuery(pTable);
	}
	else
	{
		// Carico la ORDER BY e la WHERE presente nel documento oppure quella costruita
		// per fare una find intelligente
		PrepareRadarQuery(pTable);
	}

	pTable->Open();

	resp.OpenArray(_T("recordKeys"));
	for (int i = 0; i <= pTable->GetRecord()->GetPrimaryKeyIndexes().GetUpperBound(); i++)
	{
		resp.WriteString(i, pTable->GetRecord()->GetColumnName(i));
	}
	resp.CloseArray();

	CString strQuery = pTable->GetQuery();
	resp.WriteString(_T("query"), strQuery);

	pTable->Close();

	resp.CloseObject();

	SAFE_DELETE(pTable);
	SAFE_DELETE(m_pDynamicSqlRecord);
}

//-----------------------------------------------------------------------------
///<summary>
/// It returns the value of a specific field of an hotlink associated to a control.
/// Value is returned as xmllike string in order to bypass datatype problem
///</summary>
//[TBWebMethod(securityhidden=true, thiscall_method=true, inEasyBuilder_method=false)]
DataStr	CAbstractFormDoc::GetHotLinkValue(DataStr aCtrlNamespace, DataStr aHotLinkFieldName)
{
	if (aHotLinkFieldName.IsEmpty() || aCtrlNamespace.IsEmpty())
	{
		ASSERT(FALSE);
		CString strMsg = _TB("CAbstractFormDoc::GetHotLinkValue: called with empty parameters !");
		TRACE(strMsg);
		AfxGetDiagnostic()->Add(strMsg, CDiagnostic::Warning);
		return _T("NotFound");
	}

	CTBNamespace aNs;
	aNs.AutoCompleteNamespace(CTBNamespace::CONTROL, aCtrlNamespace.GetString(), GetNamespace());

	CString strMsg;
	CParsedCtrl* pParsedCtrl = GetLinkedParsedCtrl(aNs);
	if (!pParsedCtrl)
	{
		ASSERT(FALSE);
		strMsg = cwsprintf(_TB("CAbstractFormDoc::GetHotLinkValue: %s not found!"), aNs.ToString());
		TRACE(strMsg);
		AfxGetDiagnostic()->Add(strMsg, CDiagnostic::Warning);
		return _T("NotFound");
	}

	HotKeyLink* pHotKeyLink = (HotKeyLink*)pParsedCtrl->GetHotLink();
	if (!pHotKeyLink)
	{
		ASSERT(FALSE);
		strMsg = cwsprintf(_TB("CAbstractFormDoc::GetHotLinkValue: %s does not have an hotlink !"), aNs.ToString());
		TRACE(strMsg);
		AfxGetDiagnostic()->Add(strMsg, CDiagnostic::Warning);
		return _T("NotFound");
	}

	DataObj* pDataObj = pHotKeyLink->m_pRecord->GetDataObjFromColumnName(aHotLinkFieldName);
	if (!pDataObj)
	{
		ASSERT(FALSE);
		strMsg = cwsprintf(_TB("CAbstractFormDoc::GetHotLinkValue: %s is not a valid column name for the hotlink !"), aHotLinkFieldName);
		TRACE(strMsg);
		AfxGetDiagnostic()->Add(strMsg, CDiagnostic::Warning);
		return _T("NotFound");
	}

	return pDataObj->FormatDataForXML();
}

//-----------------------------------------------------------------------------
///<summary>
/// It returns the address data of a specific field of a DBT.
/// The function can accept a nRow number or -1 value means current row.
/// Value is returned as xmllike string in order to bypass datatype problem
///</summary>
//[TBWebMethod(thiscall_method=true, inEasyBuilder_method=false)]
DataObj* CAbstractFormDoc::GetFieldData(DataStr aDBTNamespace, DataStr aFieldName, DataInt nRow)
{
	CString strMsg;
	CTBNamespace aDBTNs;
	aDBTNs.AutoCompleteNamespace(CTBNamespace::DBT, aDBTNamespace, GetNamespace());

	DBTObject* pDBT = GetDBTObject(aDBTNs);
	if (!pDBT && m_pDBTMaster)
	{
		DBTObject* pDBTObject = m_pDBTMaster;
		if (pDBTObject->GetNamespace() == aDBTNs)
			pDBT = pDBTObject;
		else
		{
			for (int i = 0; i <= m_pDBTMaster->m_pDBTSlaves->GetUpperBound(); i++)
			{
				pDBTObject = m_pDBTMaster->m_pDBTSlaves->GetAt(i);

				if (pDBTObject->GetNamespace() == aDBTNs)
					pDBT = pDBTObject;
			}
		}
	}

	if (!pDBT)
	{
		ASSERT(FALSE);
		strMsg = cwsprintf(_TB("CAbstractFormDoc::GetFieldvalue: %s is not a valid DBT table name for this document!"), aDBTNamespace);
		TRACE(strMsg);
		AfxGetDiagnostic()->Add(strMsg, CDiagnostic::Warning);
		return NULL;
	}

	if (nRow > 0 && !pDBT->IsKindOf(RUNTIME_CLASS(DBTSlaveBuffered)))
	{
		ASSERT(FALSE);
		strMsg = cwsprintf(_TB("CAbstractFormDoc::GetDBTBufferedValue: %s is not a valid DBTSlaveBuffered table name for this document!"), aDBTNamespace);
		TRACE(strMsg);
		AfxGetDiagnostic()->Add(strMsg, CDiagnostic::Warning);
		return NULL;
	}

	SqlRecord* pRec = NULL;

	if (pDBT->IsKindOf(RUNTIME_CLASS(DBTSlaveBuffered)))
	{
		DBTSlaveBuffered* pBuffered = (DBTSlaveBuffered*)pDBT;

		if (nRow >= pBuffered->GetSize())
		{
			ASSERT(FALSE);
			strMsg = cwsprintf(_TB("CAbstractFormDoc::GetFieldValue: %d is not a valid row for DBTSlaveBuffered table name for this document!"), nRow);
			TRACE(strMsg);
			AfxGetDiagnostic()->Add(strMsg, CDiagnostic::Warning);
			return NULL;
		}

		pRec = nRow < 0 ? pBuffered->GetCurrentRow() : pBuffered->GetRow(nRow);
	}
	else
		pRec = pDBT->GetRecord();

	if (!pRec)
	{
		ASSERT(FALSE);
		strMsg = cwsprintf(_TB("CAbstractFormDoc::GetDBTBufferedValue: %s DBTSlaveBuffered has found a NULL record at row %d!"), aDBTNamespace, aFieldName);
		TRACE(strMsg);
		AfxGetDiagnostic()->Add(strMsg, CDiagnostic::Warning);
		return NULL;
	}

	DataObj* pDataObj = pRec->GetDataObjFromColumnName(aFieldName);
	if (!pDataObj)
	{
		ASSERT(FALSE);
		strMsg = cwsprintf(_TB("CAbstractFormDoc::GetFieldValue: %s is not a valid column name for the Table !"), aFieldName);
		TRACE(strMsg);
		AfxGetDiagnostic()->Add(strMsg, CDiagnostic::Warning);
		return NULL;
	}
	return pDataObj;
}

//-----------------------------------------------------------------------------
///<summary>
/// It returns the value of a specific field of a DBT.
/// The function can accept a nRow number or -1 value means current row.
/// Value is returned as xmllike string in order to bypass datatype problem
///</summary>
//[TBWebMethod(thiscall_method=true, inEasyBuilder_method=false)]
DataStr	CAbstractFormDoc::GetFieldValue(DataStr aDBTNamespace, DataStr aFieldName, DataInt nRow)
{
	DataObj* pDataObj = GetFieldData(aDBTNamespace, aFieldName, nRow);
	if (!pDataObj)
		return _T("");
	return pDataObj->FormatDataForXML();
}

//-----------------------------------------------------------------------------
///<summary>
/// It update the value of a specific field of a DBT.
/// The function can accept a nRow number or -1 value means current row.
/// Value is passed to xmllike string in order to bypass datatype problem
///</summary>
//[TBWebMethod(thiscall_method=true, inEasyBuilder_method=false)]
void CAbstractFormDoc::SetFieldValue(DataStr aDBTNamespace, DataStr aFieldName, DataInt nRow, DataStr aValue)
{
	DataObj* pDataObj = GetFieldData(aDBTNamespace, aFieldName, nRow);
	if (!pDataObj)
		return;
	pDataObj->AssignFromXMLString(aValue.GetString());
}

//-----------------------------------------------------------------------------
SqlSession* CAbstractFormDoc::GetReadOnlySqlSession()
{
	return m_pTbContext->GetReadOnlySqlSession();
}

//-----------------------------------------------------------------------------
///<summary>
/// Get document updatable connection
///</summary>
//[TBWebMethod(name=CAbstractFormDoc_GetConnection, thiscall_method=true, inEasyBuilder_method=false)]
SqlSession* CAbstractFormDoc::GetUpdatableSqlSession()
{
	return m_pTbContext->GetUpdatableSqlSession();
}

//-----------------------------------------------------------------------------
SqlConnection*	CAbstractFormDoc::GetSqlConnection()
{
	return m_pSqlConnection;
}

//-----------------------------------------------------------------------------
BOOL CAbstractFormDoc::DispatchOnPasteDBTRows(CTBEDataCoDecPastedRecord& pr)
{
	BOOL bOk = m_pClientDocs->OnPasteDBTRows(pr);

	return bOk ? OnPasteDBTRows(pr) : FALSE;
}

//-----------------------------------------------------------------------------
BOOL CAbstractFormDoc::DispatchOnValidatePasteDBTRows(RecordArray& arRows, CTBEDataCoDecRecordToValidate& vr)
{
	BOOL bOk = m_pClientDocs->OnValidatePasteDBTRows(arRows, vr);

	return bOk ? OnValidatePasteDBTRows(arRows, vr) : FALSE;
}

//-----------------------------------------------------------------------------
BOOL CAbstractFormDoc::DispatchOnValidatePasteDBTRows(SqlRecord* pRec, CTBEDataCoDecRecordToValidate& vr)
{
	BOOL bOk = m_pClientDocs->OnValidatePasteDBTRows(pRec, vr);

	return bOk ? OnValidatePasteDBTRows(pRec, vr) : FALSE;
}

//-----------------------------------------------------------------------------
CRuntimeClass* CAbstractFormDoc::DispatchOnModifySqlRecordClass(DBTObject* pDbt, const CString& sDBTName, CRuntimeClass* pSqlRecordClass)
{
	CRuntimeClass* pClass = m_pClientDocs->OnModifySqlRecordClass(pDbt, sDBTName, pSqlRecordClass);
	if (pClass && pClass != pSqlRecordClass)
		return pClass;
	return OnModifySqlRecordClass(pDbt, sDBTName, pSqlRecordClass);
}

//------------------------------------------------------------------------------
BOOL CAbstractFormDoc::DispatchCanDoEditRecord()
{
	return CanDoEditRecord() && m_pClientDocs->CanDoEditRecord();
}

//------------------------------------------------------------------------------
BOOL CAbstractFormDoc::DispatchCanDoDeleteRecord()
{
	return CanDoDeleteRecord() && m_pClientDocs->CanDoDeleteRecord();
}

//------------------------------------------------------------------------------
BOOL CAbstractFormDoc::DispatchCanDoNewRecord()
{
	return CanDoNewRecord() && m_pClientDocs->CanDoNewRecord();
}

//------------------------------------------------------------------------------
BOOL CAbstractFormDoc::DispatchOnBeforeEditRecord()
{
	return OnBeforeEditRecord() && m_pClientDocs->OnBeforeEditRecord();
}

//------------------------------------------------------------------------------
BOOL CAbstractFormDoc::DispatchOnBeforeNewRecord()
{
	return OnBeforeNewRecord() && m_pClientDocs->OnBeforeNewRecord();
}

//------------------------------------------------------------------------------
BOOL CAbstractFormDoc::DispatchOnBeforeDeleteRecord()
{
	return OnBeforeDeleteRecord() && m_pClientDocs->OnBeforeDeleteRecord();
}

//------------------------------------------------------------------------------
BOOL CAbstractFormDoc::DispatchOnBeforeEscape()
{
	return OnBeforeEscape() && FireBehaviour(bhe_BeforeEscape) && m_pClientDocs->OnBeforeEscape();
}

//------------------------------------------------------------------------------
void CAbstractFormDoc::DispatchOnBESelCell(CBodyEdit* pBody, SqlRecord* pCurRec, ColumnInfo* pCol)
{
	OnBESelCell(pBody, pCurRec, pCol);
	m_pClientDocs->OnBESelCell(pBody, pCurRec, pCol);
}

void CAbstractFormDoc::DispatchOnBEShowCtrl(CBodyEdit* pBody, SqlRecord* pCurRec, ColumnInfo* pCol)
{
	OnBEShowCtrl(pBody, pCurRec, pCol);
	m_pClientDocs->OnBEShowCtrl(pBody, pCurRec, pCol);
}

void CAbstractFormDoc::DispatchOnBEHideCtrl(CBodyEdit* pBody, SqlRecord* pCurRec, ColumnInfo* pCol)
{
	OnBEHideCtrl(pBody, pCurRec, pCol);
	m_pClientDocs->OnBEHideCtrl(pBody, pCurRec, pCol);
}

void CAbstractFormDoc::DispatchOnBEEnableButton(CBodyEdit* pBody, CBEButton* pBtn)
{
	OnBEEnableButton(pBody, pBtn);
	m_pClientDocs->OnBEEnableButton(pBody, pBtn);
}

//------------------------------------------------------------------------------

BOOL CAbstractFormDoc::DispatchOnBEBeginMultipleSel(CBodyEdit* pBody)
{
	BOOL b = m_pClientDocs->OnBEBeginMultipleSel(pBody);
	if (!b) OnBEBeginMultipleSel(pBody);
	return b;
}

BOOL CAbstractFormDoc::DispatchOnBEEndMultipleSel(CBodyEdit* pBody)
{
	BOOL b = m_pClientDocs->OnBEEndMultipleSel(pBody);
	if (!b) OnBEEndMultipleSel(pBody);
	return b;
}

BOOL CAbstractFormDoc::DispatchOnBECustomizeSelections(CBodyEdit* pBody, SelArray&	sel)
{
	BOOL b = m_pClientDocs->OnBECustomizeSelections(pBody, sel);
	if (!b) OnBECustomizeSelections(pBody, sel);
	return b;
}

//----------------------------------------------------------------------------------
BOOL CAbstractFormDoc::DispatchOnBECanDoDeleteRow(CBodyEdit* pBodyEdit)
{
	BOOL b = m_pClientDocs->OnBECandDoDeleteRow(pBodyEdit);

	return (!b ? FALSE : OnBECanDoDeleteRow(pBodyEdit));

}

//------------------------------------------------------------------------------
CBEButton* CAbstractFormDoc::GetBEButton(UINT idBody, UINT idBtn)
{
	CBodyEdit* pBody = dynamic_cast<CBodyEdit*>(GetWndCtrl(idBody));
	if (pBody)
	{
		CBEButton* pBtn = pBody->m_HeaderToolBar.FindButton(idBtn);
		if (!pBtn)
			pBtn = pBody->m_FooterToolBar.FindButton(idBtn);

		return pBtn;
	}
	return NULL;
}

//------------------------------------------------------------------------------
WebCommandType CAbstractFormDoc::OnGetWebCommandType(UINT commandID)
{
	WebCommandType type;
	//client docs can override default behaviour
	for (int i = 0; i <= m_pClientDocs->GetUpperBound(); i++)
		if ((type = m_pClientDocs->GetAt(i)->OnGetWebCommandType(commandID)) != WEB_UNDEFINED)
			return type;

	if (commandID == ID_EXTDOC_GOTO_MASTER 				// sposta fuoco su finestra master
		|| commandID == ID_EDIT_QUERY						// editazione query	
		|| commandID == ID_BROWSE_FILES 					// ricerca file su disco nel namespaceEdit
#ifndef TBWEB
		|| commandID == IDC_BE_SEARCH						// ricerca nel body edit
#endif
		)
		return WEB_UNSUPPORTED;
	if (commandID == ID_EXTDOC_HELP ||
		commandID == ID_ADDRESS_SHOW_MAP ||
		commandID == ID_ADDRESS_SELECTION_SHOW_MAP ||
		commandID == ID_GOOGLE_SHOW_MAP ||
		commandID == ID_GOOGLE_SHOW_SATELLITE
		)
		return WEB_LINK;

	return __super::OnGetWebCommandType(commandID);
}


//-----------------------------------------------------------------------------
void CAbstractFormDoc::DispatchOnActivate(CAbstractFormFrame* pFrame, UINT nState, CWnd* pWndOther, BOOL bMinimized)
{
	if (m_pClientDocs)
		m_pClientDocs->OnActivate(pFrame, nState, pWndOther, bMinimized);
}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::GetBindingInfo(const CString& sDataSource, const CString &sId, DBTObject*& pDBT, SqlRecord*& pRecord, DataObj*& pDataObj, CString& sBindingName, BOOL bMustExist)
{
	//se sono in design mode, non posso trovare i dataobj per il binding,
	//assegno un binding name di ufficio ed esco
	if (IsInStaticDesignMode())
	{
		if (sBindingName.IsEmpty())
			sBindingName = sDataSource;
		if (sBindingName.IsEmpty())
			sBindingName = sId;
		return;
	}
	//posso avere:

	//dbt.campo
	//hkl.campo
	//dbt
	//campo

	//retrieve SqlRecord and DataObj from data source
	if (!sDataSource.IsEmpty())
	{

		int startField = sDataSource.Find(_T("."));

		CString sDbtOrHKLOrFieldName, sFieldName;
		if (startField > -1)
		{
			sDbtOrHKLOrFieldName = sDataSource.Mid(0, startField);
			sDbtOrHKLOrFieldName.Trim();
			sFieldName = sDataSource.Mid(startField + 1);
			sFieldName.Trim();
		}
		else
		{
			sDbtOrHKLOrFieldName = sDataSource;
			sDbtOrHKLOrFieldName.Trim();
		}
		bool isVirtual = false;
		if (sFieldName.IsEmpty())
		{
			//ho solo un token: allora � un nome di campo o di dbt
			//provo a vedere se � un dbt
			GetDataSource(sDbtOrHKLOrFieldName, _T(""), pDBT, pRecord, pDataObj, isVirtual);
			if (pDBT && pRecord)
			{
				return;
			}
			//se non � un dbt, provo a vedere se � un campo
			GetDataSource(_T(""), sDbtOrHKLOrFieldName, pDBT, pRecord, pDataObj, isVirtual);

			if (pDataObj && sBindingName.IsEmpty())
				sBindingName = sDbtOrHKLOrFieldName;
#ifdef DEBUG
			if (!pDataObj && bMustExist)
			{
				TRACE2("Invalid data source: %s\n, %s\n", (LPCTSTR)sDataSource, (LPCTSTR)sId);
				ASSERT(FALSE);
			}
#endif
			return;
		}
		else
		{
			GetDataSource(sDbtOrHKLOrFieldName, sFieldName, pDBT, pRecord, pDataObj, isVirtual);
		}
		if (pRecord)
		{
			//macro RDC
			if (sBindingName.IsEmpty())
			{
				//potrebbe essere un alias
				CString sRealFieldName = sFieldName[0] == ALIAS_IDENTIFIER
					? pRecord->GetColumnInfo(pDataObj)->m_strColumnName
					: sFieldName;
				sBindingName = isVirtual ? sRealFieldName : pRecord->GetTableName() + _T("_") + sRealFieldName;
			}
		}
#ifdef DEBUG
		if (!pDataObj && bMustExist)
		{
			TRACE1("Invalid data source: %s\n", (LPCTSTR)sDataSource);
			ASSERT(FALSE);
		}
#endif
	}
}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::GetJson(CJsonSerializer& jsonSerializer, BOOL bOnlyWebBound)
{
	CString id = GetComponentId();
	if (id == _T("0"))
	{
		return;
	}
	CAbstractFormView* pView = dynamic_cast<CAbstractFormView*>(this->GetFirstView());
	if (pView)
		m_pHeaderTitle->Assign(pView->GetCaption());

	if (m_bJsonDataSent)
	{
		GetJsonPatch(jsonSerializer, bOnlyWebBound);
		if (!jsonSerializer.IsCurrentEmpty())
			jsonSerializer.WriteString(_T("id"), (LPCTSTR)id);
		return;
	}

	jsonSerializer.WriteString(_T("id"), (LPCTSTR)id);
	if (m_arDataSourceAliases.GetCount() || m_arFieldAliases.GetCount())
	{
		jsonSerializer.OpenObject(_T("aliases"));

		for (int i = 0; i < m_arFieldAliases.GetCount(); i++)
		{
			CFieldAlias* pAlias = (CFieldAlias*)m_arFieldAliases.GetAt(i);
			jsonSerializer.OpenObject(pAlias->m_sAlias);
			jsonSerializer.WriteString(_T("actual"), pAlias->m_sActual);
			jsonSerializer.CloseObject();
		}


		for (int i = 0; i < m_arDataSourceAliases.GetCount(); i++)
		{
			CDataSourceAlias* pAlias = (CDataSourceAlias*)m_arDataSourceAliases.GetAt(i);
			jsonSerializer.OpenObject(pAlias->m_sAlias);
			jsonSerializer.WriteString(_T("actual"), pAlias->m_sActual);
			if (pAlias->m_arFields.GetCount())
			{
				for (int j = 0; j < pAlias->m_arFields.GetCount(); j++)
				{
					CFieldAlias* pFieldAlias = (CFieldAlias*)pAlias->m_arFields.GetAt(j);
					jsonSerializer.OpenObject(pFieldAlias->m_sAlias);
					jsonSerializer.WriteString(_T("actual"), pFieldAlias->m_sActual);
					jsonSerializer.CloseObject();
				}
			}
			jsonSerializer.CloseObject();
		}

		jsonSerializer.CloseObject();
	}
	jsonSerializer.OpenObject(_T("data"));
	CXMLVariable* pVariable;
	if (m_pVariablesArray)
	{
		for (int i = 0; i < m_pVariablesArray->GetSize(); i++)
		{
			pVariable = m_pVariablesArray->GetAt(i);
			if (!bOnlyWebBound || pVariable->GetDataObj()->IsWebBound())
			{
				jsonSerializer.OpenObject(pVariable->GetName());
				pVariable->GetDataObj()->SerializeToJson(jsonSerializer, false);
				jsonSerializer.CloseObject();
			}
		}
	}

	for (int i = 0; i < m_arRegisteredDBTs.GetCount(); i++)
	{
		DBTObject* pDBT = m_arRegisteredDBTs.GetAt(i);
		pDBT->GetJson(jsonSerializer, bOnlyWebBound);
	}

	//cerco l'hotlink by name; se non lo trovo e mi viene passato un namespace valido, lo creo al volo
	for (int i = 0; i < m_pHotKeyLinks->GetSize(); i++)
	{
		HotKeyLink* pHotKeyLink = (HotKeyLink*)m_pHotKeyLinks->GetAt(i);
		pHotKeyLink->GetJson(jsonSerializer, bOnlyWebBound);
	}
	jsonSerializer.CloseObject();
	m_bJsonDataSent = TRUE;
}
//-----------------------------------------------------------------------------
void CAbstractFormDoc::GetJsonPatch(CJsonSerializer& jsonSerializer, BOOL bOnlyWebBound)
{
	jsonSerializer.OpenObject(_T("data"));
	CXMLVariable* pVariable;
	if (m_pVariablesArray)
	{
		for (int i = 0; i < m_pVariablesArray->GetSize(); i++)
		{
			pVariable = m_pVariablesArray->GetAt(i);
			if ((!bOnlyWebBound || pVariable->GetDataObj()->IsWebBound()) && pVariable->GetDataObj()->HasChangedForJson())
			{
				jsonSerializer.OpenObject(pVariable->GetName());
				pVariable->GetDataObj()->SerializeToJson(jsonSerializer, true);
				jsonSerializer.CloseObject();
			}
		}
	}

	//cerco l'hotlink by name; se non lo trovo e mi viene passato un namespace valido, lo creo al volo
	for (int i = 0; i < m_pHotKeyLinks->GetSize(); i++)
	{
		HotKeyLink* pHotKeyLink = (HotKeyLink*)m_pHotKeyLinks->GetAt(i);
		pHotKeyLink->GetJsonPatch(jsonSerializer, bOnlyWebBound);
	}
	for (int i = 0; i < m_arRegisteredDBTs.GetCount(); i++)
	{
		DBTObject* pDBT = m_arRegisteredDBTs.GetAt(i);
		pDBT->GetJsonPatch(jsonSerializer, bOnlyWebBound);
	}
	jsonSerializer.CloseObject(TRUE);

	m_ModifiedData.RemoveAll();
}

//-----------------------------------------------------------------------------
CString	CAbstractFormDoc::GetComponentId()
{
	return cwsprintf(_T("{0-%d}"), GetFrameHandle());
}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::SetJson(CJsonParser& jsonParser)
{
	if (jsonParser.BeginReadObject(_T("data")))
	{
		CJsonParser parser;
		CString sKey;
		CJsonIterator* pIterator = jsonParser.BeginIteration();
		while (pIterator->GetNext(sKey, parser))
		{
			if (parser.Has(_T("_value")))//� una variabile di documento
			{
				DataObj* pData = GetVariableValue(sKey);
				if (pData)
				{
					pData->AssignFromJson(parser);
					SetModifiedFlag();
				}
			}
			else
			{
				DBTObject* pDBT = GetDBTByName(sKey);
				if (pDBT && pDBT->SetJson(parser))
					SetModifiedFlag();
			}
		}

	}
}

//-----------------------------------------------------------------------------
CXMLVariable* CAbstractFormDoc::GetVariable(const CString& sName)
{
	if (!m_pVariablesArray)
		return NULL;

	CXMLVariable* pVariable;
	for (int i = 0; i <= m_pVariablesArray->GetUpperBound(); i++)
	{
		pVariable = m_pVariablesArray->GetAt(i);
		if (pVariable && sName.CompareNoCase(pVariable->GetName()) == 0)
			return pVariable;
	}

	return NULL;
}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::RemoveVariable(const CString& sName)
{
	if (!m_pVariablesArray)
		return;

	CXMLVariable* pVariable;
	for (int i = 0; i <= m_pVariablesArray->GetUpperBound(); i++)
	{
		pVariable = m_pVariablesArray->GetAt(i);
		if (pVariable && sName.CompareNoCase(pVariable->GetName()) == 0)
		{
			m_pVariablesArray->RemoveAt(i);
			break;
		}
	}
}


//-----------------------------------------------------------------------------
DataObj* CAbstractFormDoc::GetVariableValue(const CString& sName)
{
	CXMLVariable* pVariable = GetVariable(sName);

	return pVariable ? pVariable->GetDataObj() : NULL;
}

//-----------------------------------------------------------------------------
BOOL  CAbstractFormDoc::ValorizeVariable(const CString& sName, const CString& sValue)
{
	CXMLVariable* pVariable = GetVariable(sName);
	if (!pVariable)
		return FALSE;
	ASSERT_VALID(pVariable->GetDataObj());

	pVariable->GetDataObj()->Assign(sValue);
	return TRUE;
}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::BindVariable(const CString& sName, DataObj* pDataObj)
{
	CXMLVariable* pVariable = GetVariable(sName);
	if (pVariable)
		pVariable->BindExternalDataObj(pDataObj);
	else
		DeclareVariable(sName, pDataObj, FALSE);
}
//-----------------------------------------------------------------------------
void CAbstractFormDoc::DeclareVariable(const CString& sName, DataObj& aDataObj)
{
	DeclareVariable(sName, &aDataObj, FALSE, FALSE);
}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::DeclareVariable(const CString& sName, DataObj* pDataObj, BOOL bOwnsDataObj, /*FALSE*/ BOOL bReplaceExisting /*FALSE*/)
{
	CXMLVariable* pXMLVar = NULL;
	if (pXMLVar = GetVariable(sName))
	{
		if (bReplaceExisting)
		{
			ASSERT(pXMLVar->GetOwnsDataObj() == bOwnsDataObj);
			pXMLVar->SetDataObj(pDataObj);
			return;
		}
		ASSERT(FALSE);
		TRACE2("Document Variable %s has already been declared into %s document", sName, GetNamespace().ToUnparsedString());
		return;
	}

	if (!m_pVariablesArray)
		m_pVariablesArray = new CXMLVariableArray();

	pXMLVar = new CXMLVariable(sName, pDataObj, bOwnsDataObj);
	m_pVariablesArray->Add(pXMLVar);
}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::ReloadSymbolTable()
{
	SAFE_DELETE(m_pSymTable);
	m_pSymTable = new SymTable();

	LoadSymbolTable();
}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::AddToSymbolTable(SqlRecord* pRecord, const CString& strObjectName, BOOL bDeprecated)
{
	for (int i = 0; i < pRecord->GetSizeEx(); i++)
	{
		SqlRecordItem* pItem = pRecord->GetAt(i);
		if (!pItem || !pItem->GetDataObj())
			continue;

		SymField* pField = bDeprecated
			? new DeprecatedSymField
			(
				CDocFieldNames::DocumentDot +
				strObjectName +
				DOT_CHAR
				+ pRecord->GetQualifiedColumnName(i),
				pItem->GetDataObj()->GetDataType(),
				0,
				pItem->GetDataObj(),
				FALSE
			)
			: new SymField
			(
				CDocFieldNames::DocumentDot +
				strObjectName +
				DOT_CHAR
				+ pRecord->GetQualifiedColumnName(i),
				pItem->GetDataObj()->GetDataType(),
				0,
				pItem->GetDataObj(),
				FALSE
			);

		m_pSymTable->Add(pField);
	}
}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::LoadSymbolTable()
{
	// document context symbols
	DataLng me((long)this);
	me.SetAsHandle();
	SymField* pField = new SymField(CDocFieldNames::Document, DataType::Object, 0, &me);
	pField->AddMethods(RUNTIME_CLASS(CBaseDocument), AfxGetAddOnAppsTable()->GetMapWebClass());
	m_pSymTable->Add(pField);	pField->GetData()->SetValid(TRUE);

	CString aDocFullName = GetNamespace().ToUnparsedString();
	DataStr dsDocNs = aDocFullName;

	pField = new SymField(CDocFieldNames::Document_Namespace, DataType::String, 0, &dsDocNs);
	m_pSymTable->Add(pField);	pField->GetData()->SetValid(TRUE);

	DataStr dsDocName = GetNamespace().GetObjectName();
	pField = new SymField(CDocFieldNames::Document_Name, DataType::String, 0, &dsDocName);
	m_pSymTable->Add(pField);	pField->GetData()->SetValid(TRUE);

	//------------------------------------
	/*if (m_pVariablesArray) //TODO per la gestione dei loca di documento
		for (int i = 0; i <= m_pVariablesArray->GetUpperBound(); i++)
		{
			CXMLVariable* pVariable = (CXMLVariable*) m_pVariablesArray->GetAt(i);
			if (!pVariable || !pVariable->GetDataObj())
				continue;

			pField = new SymField
				(
					CDocFieldNames::DocumentDot + pVariable->GetName(),
					pVariable->GetDataObj()->GetDataType(),
					0,
					pVariable->GetDataObj(),
					FALSE
				);

			m_pSymTable->Add(pField);
			pField->GetData()->SetValid(TRUE);
		}*/

	if (!m_pDBTMaster)
		return;

	//queste le lascio per compatibilit� pregressa, ma sono da considerarsi deprecate, occorre valorizzarle
	//nell'MDocument con i nomi dei componenti!
	AddToSymbolTable(m_pDBTMaster->GetRecord(), m_pDBTMaster->GetRecord()->GetTableName(), TRUE);

	if (!m_pDBTMaster->m_pDBTSlaves)
		return;

	for (int d = 0; d <= m_pDBTMaster->m_pDBTSlaves->GetUpperBound(); d++)
	{
		DBTObject* pDBTObject = m_pDBTMaster->m_pDBTSlaves->GetAt(d);
		if (!pDBTObject)
			continue;

		SqlRecord* pRecord = NULL;
		if (pDBTObject->IsKindOf(RUNTIME_CLASS(DBTSlaveBuffered)))
		{
			//se ho la riga corrente, prendo quella, altrimenti la riga del prototipo
			//cos� ho sempre i campi del dbt in symbol table anche per scopi di editing della query
			DBTSlaveBuffered* pBuff = (DBTSlaveBuffered*)pDBTObject;
			pRecord = pBuff->GetCurrentRow();
			if (!pRecord)
				pRecord = pBuff->GetRecord();
		}
		else
		{
			pRecord = pDBTObject->GetRecord();
		}

		if (pRecord)
			AddToSymbolTable(pRecord, pRecord->GetTableName(), TRUE);
	}
}

//------------------------------------------------------------------------------
void CAbstractFormDoc::PrepareSymbolTable()
{
	SAFE_DELETE(m_pSymTable);
	m_pSymTable = new SymTable(); m_pSymTable->SetDocument(this);

	// document context symbols
	DataLng me((long)this);
	me.SetAsHandle();
	SymField* pField = new SymField(CDocFieldNames::Document, DataType::Object, SpecialReportField::NO_INTERNAL_ID, &me);
	pField->AddMethods(RUNTIME_CLASS(CBaseDocument), AfxGetAddOnAppsTable()->GetMapWebClass());
	m_pSymTable->Add(pField);	pField->GetData()->SetValid(TRUE);

	CString aDocFullName = GetNamespace().ToUnparsedString();
	DataStr dsDocNs = aDocFullName;

	pField = new SymField(CDocFieldNames::Document_Namespace, DataType::String, SpecialReportField::NO_INTERNAL_ID, &dsDocNs);
	m_pSymTable->Add(pField);	pField->GetData()->SetValid(TRUE);

	DataStr dsDocName = GetNamespace().GetObjectName();
	pField = new SymField(CDocFieldNames::Document_Name, DataType::String, SpecialReportField::NO_INTERNAL_ID, &dsDocName);
	m_pSymTable->Add(pField);	pField->GetData()->SetValid(TRUE);

	if (!m_pDBTMaster)
		return;

	m_pDBTMaster->PrepareSymbolTable(m_pSymTable);
}

//------------------------------------------------------------------------------
CString CAbstractFormDoc::GetKeyInXMLFormat()
{
	CString strKey;
	if (ValidCurrentRecord())
		m_pDBTMaster->GetRecord()->GetKeyInXMLFormat(strKey);

	return strKey;
}

//------------------------------------------------------------------------------------
void CAbstractFormDoc::DispatchDMSEvent(DMSEventTypeEnum eventType, int eventKey)
{
	OnDMSEvent(eventType, eventKey);
	if (m_pClientDocs)
		m_pClientDocs->OnDMSEvent(eventType, eventKey);
}

//------------------------------------------------------------------------------
CString CAbstractFormDoc::FormatRollbackLogMessage()
{
	if (!m_pDBTMaster)
		return _TB("Document without DBTs attached. Cannot log transaction data.");

	return m_pDBTMaster->GetRecord()->GetPrimaryKeyDescription();
}
//------------------------------------------------------------------------------
BOOL CAbstractFormDoc::CanCreateControl(UINT idc)
{
	return TRUE;
}
//------------------------------------------------------------------------------
void CAbstractFormDoc::OnParsedControlCreated(CParsedCtrl* pCtrl)
{

}
//------------------------------------------------------------------------------
void CAbstractFormDoc::OnColumnInfoCreated(ColumnInfo* pColInfo)
{
}

//------------------------------------------------------------------------------
void CAbstractFormDoc::OnPropertyCreated(CTBProperty * pProperty)
{
}

//------------------------------------------------------------------------------
BOOL CAbstractFormDoc::OnGetToolTipProperties(CBETooltipProperties* pTooltip)
{
	return FALSE;
}

//------------------------------------------------------------------------------
void CAbstractFormDoc::EnableBodyEditButtons(CBodyEdit* pBodyEdit)
{
}

//------------------------------------------------------------------------------
void CAbstractFormDoc::CustomizeGauge(CTBLinearGaugeCtrl* pBodyEdit)
{
}

//------------------------------------------------------------------------------
HotKeyLink* CAbstractFormDoc::GetHotLink(const CString& sName, CRuntimeClass* pClass)
{
	if (!m_pHotKeyLinks)
		return NULL;
	//cerco l'hotlink by name; se non lo trovo e mi viene passata una classe, lo creo al volo
	for (int i = 0; i <= m_pHotKeyLinks->GetUpperBound(); i++)
	{
		HotKeyLink* pHotKeyLink = (HotKeyLink*)m_pHotKeyLinks->GetAt(i);
		if (pHotKeyLink->GetName().CompareNoCase(sName) == 0)
		{
			if (!pClass || pHotKeyLink->IsKindOf(pClass))
				return pHotKeyLink;
			USES_CONVERSION;
			TRACE2("HotLink %s is not of type %s\n", sName, A2T(pClass->m_lpszClassName));
			ASSERT(FALSE);
			return NULL;
		}
	}

	HotKeyLink* pHotKeyLink = NULL;
	CString sVal;
	if (m_ForwardHotlinks.Lookup(sName, sVal))
	{
		pHotKeyLink = (HotKeyLink*)AfxGetTbCmdManager()->RunHotlink(CTBNamespace(CTBNamespace::HOTLINK, sVal));
		if (pHotKeyLink)
			Attach(pHotKeyLink, sName);

		if (!pClass || pHotKeyLink->IsKindOf(pClass))
			return pHotKeyLink;
		USES_CONVERSION;
		TRACE2("HotLink %s is not of type %s\n", sName, A2T(pClass->m_lpszClassName));
		ASSERT(FALSE);
		return NULL;
	}

	if (!pClass)
		return NULL;

	pHotKeyLink = (HotKeyLink*)pClass->CreateObject();
	Attach(pHotKeyLink, sName);

	return pHotKeyLink;
}
//------------------------------------------------------------------------------
void CAbstractFormDoc::RegisterForwardHotLink(const CString& sName, const CString& aNameSpace)
{
#ifdef DEBUG
	CString sVal;
	if (m_ForwardHotlinks.Lookup(sName, sVal))
	{
		ASSERT(sVal == aNameSpace);
	}

#endif // DEBUG

	if (sName.IsEmpty())
	{
		ASSERT_TRACE(FALSE, "Empty HotLink name");
		return;
	}
	m_ForwardHotlinks.SetAt(sName, aNameSpace);
}

//------------------------------------------------------------------------------
HotKeyLink* CAbstractFormDoc::GetHotLink(const CString& sName, const CTBNamespace& aNameSpace /*= CTBNamespace(_T(""))*/)
{
	if (!m_pHotKeyLinks)
		return NULL;
	//cerco l'hotlink by name; se non lo trovo e mi viene passato un namespace valido, lo creo al volo
	for (int i = 0; i <= m_pHotKeyLinks->GetUpperBound(); i++)
	{
		HotKeyLink* pHotKeyLink = (HotKeyLink*)m_pHotKeyLinks->GetAt(i);
		if (pHotKeyLink->GetName().CompareNoCase(sName) == 0)
		{
			if (!aNameSpace.IsEmpty() && pHotKeyLink->GetNamespace() != aNameSpace)
			{
				TRACE2("HotLink %s has not the namespace %s\n", sName, aNameSpace.ToString());
				ASSERT(FALSE);
				return NULL;
			}
			return pHotKeyLink;
		}
	}
	CTBNamespace ns = aNameSpace;
	HotKeyLink* pHotKeyLink = NULL;
	if (ns.IsEmpty())
	{
		CString sVal;
		if (m_ForwardHotlinks.Lookup(sName, sVal))
		{
			ns = CTBNamespace(CTBNamespace::HOTLINK, sVal);
		}
	}
	if (!ns.IsEmpty())
	{
		pHotKeyLink = (HotKeyLink*)AfxGetTbCmdManager()->RunHotlink(ns);
		if (pHotKeyLink)
			Attach(pHotKeyLink, sName);
	}

	return pHotKeyLink;
}

//------------------------------------------------------------------------------
CItemSource* CAbstractFormDoc::GetItemSource(const CString& sName, const CTBNamespace& aNameSpace, const CString& sParameter /*NULL*/, bool  bUseProductLanguage /*FALSE*/, bool  bAllowChanges/*FALSE*/)
{
	for (int i = 0; i < m_ItemSources.GetSize(); i++)
	{
		CItemSource* pDS = (CItemSource*)m_ItemSources.GetAt(i);
		if (pDS->IsKindOf(RUNTIME_CLASS(CItemSourceXml)))
		{
			CItemSourceXml* pDSXml = (CItemSourceXml*)pDS;
			if (pDSXml->GetName().CompareNoCase(sName) == 0 &&
				pDSXml->GetNamespace().CompareNoCase(aNameSpace.ToString()) == 0 &&
				pDSXml->GetParameter().CompareNoCase(sParameter) == 0)
				return pDSXml;
		}
		else
		{
			if (pDS->GetName().CompareNoCase(sName) == 0 && pDS->GetNamespace().CompareNoCase(aNameSpace.ToString()) == 0)
				return pDS;
		}
	}

	CItemSource* pDS = NULL;
	if (aNameSpace.IsValid())
	{
		pDS = (CItemSource*)AfxGetTbCmdManager()->CreateItemSource(aNameSpace);
		if (pDS)
		{
			pDS->SetName(sName);
			pDS->SetNamespace(aNameSpace.ToString());
			pDS->SetDocument(this);
			if (pDS->IsKindOf(RUNTIME_CLASS(CItemSourceXml)))
				((CItemSourceXml*)pDS)->Initialize(sParameter, bUseProductLanguage, bAllowChanges);
			m_ItemSources.Add(pDS);
		}
	}

	return pDS;
}

//------------------------------------------------------------------------------
CControlBehaviour* CAbstractFormDoc::GetControlBehaviour(const CString& sName, const CTBNamespace& aNameSpace)
{
	CControlBehaviour* pControlBehaviour = NULL;
	for (int i = 0; i < m_ControlBehaviours.GetSize(); i++)
	{
		pControlBehaviour = m_ControlBehaviours.GetAt(i);
		if (pControlBehaviour->GetName().CompareNoCase(sName) == 0 && pControlBehaviour->GetNamespace().CompareNoCase(aNameSpace.ToString()) == 0)
			return pControlBehaviour;
	}

	if (aNameSpace.IsValid())
	{
		pControlBehaviour = (CControlBehaviour*)AfxGetTbCmdManager()->CreateControlBehaviour(aNameSpace);
		if (pControlBehaviour)
		{
			pControlBehaviour->SetName(sName);
			pControlBehaviour->SetNamespace(aNameSpace.ToString());
			pControlBehaviour->SetDocument(this);
			OnControlBehaviourCreated(pControlBehaviour);
			m_ControlBehaviours.Add(pControlBehaviour);
		}
		else
		{
			ASSERT_TRACE2(FALSE, "Cannot create control behaviour %s with namespace %s", (LPCTSTR)sName, (LPCTSTR)aNameSpace.ToString());
		}
	}
	else
	{
		ASSERT_TRACE2(FALSE, "Invalid namespace for control behaviour %s: %s", (LPCTSTR)sName, (LPCTSTR)aNameSpace.ToString());
	}

	return pControlBehaviour;
}


//------------------------------------------------------------------------------
CDataAdapter* CAbstractFormDoc::GetDataAdapter(const CString& sName, const CTBNamespace& aNameSpace)
{
	for (int i = 0; i < m_pDataAdapters.GetSize(); i++)
	{
		CDataAdapter* pDA = (CDataAdapter*)m_pDataAdapters.GetAt(i);
		if (pDA->GetName().CompareNoCase(sName) == 0)
			return pDA;
	}

	CDataAdapter* pDA = NULL;
	if (aNameSpace.IsValid())
	{
		pDA = (CDataAdapter*)AfxGetTbCmdManager()->CreateDataAdapters(aNameSpace);
		if (pDA)
		{
			pDA->SetName(sName);
			pDA->SetDocument(this);
			m_pDataAdapters.Add(pDA);
		}
	}

	return pDA;
}
//------------------------------------------------------------------------------
CValidator* CAbstractFormDoc::GetValidator(const CString& sName, const CTBNamespace& aNameSpace)
{
	for (int i = 0; i < m_Validators.GetSize(); i++)
	{
		CValidator* pVal = (CValidator*)m_Validators.GetAt(i);
		if (pVal->GetName().CompareNoCase(sName) == 0)
			return pVal;
	}

	CValidator* pValidator = NULL;
	if (aNameSpace.IsValid())
	{
		pValidator = (CValidator*)AfxGetTbCmdManager()->CreateValidator(aNameSpace);
		if (pValidator)
		{
			pValidator->SetName(sName);
			pValidator->SetDocument(this);
			m_Validators.Add(pValidator);
		}
	}

	return pValidator;
}

//------------------------------------------------------------------------------
void CAbstractFormDoc::RemoveHotLink(HotKeyLink* pHotKeyLink)
{
	HotKeyLink* pHKL;
	if (m_pHotKeyLinks)
	{
		for (int i = m_pHotKeyLinks->GetUpperBound(); i >= 0; i--)
		{
			pHKL = (HotKeyLink*)m_pHotKeyLinks->GetAt(i);
			if (pHotKeyLink == pHKL)
			{
				m_pHotKeyLinks->RemoveAt(i);
				break;
			}
		}
	}
}

//------------------------------------------------------------------------------
CManagedDocComponentObj* CAbstractFormDoc::GetComponent(CString& sParentNamespace, CString& sName)
{
	return (m_pClientDocs) ? m_pClientDocs->GetComponent(sParentNamespace, sName) : NULL;
}

//------------------------------------------------------------------------------
void CAbstractFormDoc::GetComponents(CManagedDocComponentObj* pRequest, Array& returnedComponents)
{
	return (m_pClientDocs) ? m_pClientDocs->GetComponents(pRequest, returnedComponents) : NULL;
}

//------------------------------------------------------------------------------
void CAbstractFormDoc::DispatchBuildingSecurityTree(CTBTreeCtrl* pTree, Array* pInfoTreeItems)
{
	if (m_pClientDocs)
		m_pClientDocs->OnBuildingSecurityTree(pTree, pInfoTreeItems);
}

//------------------------------------------------------------------------------
void CAbstractFormDoc::DispatchDocumentCreated()
{
	OnDocumentCreated();
	if (m_pClientDocs)
		m_pClientDocs->OnDocumentCreated();
}

//------------------------------------------------------------------------------
void CAbstractFormDoc::DispatchFrameCreated()
{
	if (m_pClientDocs)
		m_pClientDocs->OnFrameCreated();
}


//------------------------------------------------------------------------------
int CAbstractFormDoc::GetAttachedReports(CReportMenuNode* pReportRootNode)
{
	CReportMenuNode* pSon = new CReportMenuNode;

	CString s = GetDefaultTitleForm();

	if (m_pFormManager)
		m_pFormManager->EnumReportAlias(pReportRootNode);

	int nDefaultIdx = m_pFormManager->GetIndexReportDefault();

	if (!s.IsEmpty())
	{
		pSon->SetNodeTag(s);
		pReportRootNode->InsertSonAt(0, pSon);
	}
	else
	{
		SAFE_DELETE(pSon);
	}

	return nDefaultIdx;
}

//------------------------------------------------------------------------------
int CAbstractFormDoc::GetAttachedQueries(CStringArray& arQueries)
{
	if (OSL_CAN_DO(GetInfoOSL(), OSL_GRANT_BROWSE | OSL_GRANT_BROWSE_EXTENDED))
	{
		if (!m_pQueryManager)
			return -1;

		m_pQueryManager->GetAllQueriesForMenu(arQueries);
	}
	return 0;
}

//------------------------------------------------------------------------------
BOOL CAbstractFormDoc::CanShowQueryManager()
{
	return OSL_CAN_DO(GetInfoOSL(), OSL_GRANT_EDITQUERY);
}

//------------------------------------------------------------------------------
void CAbstractFormDoc::SetAbstractFormDocObjectProxy(IAbstractFormDocObject* pObject)
{
}

//------------------------------------------------------------------------------
BOOL CAbstractFormDoc::OnPrepareAuxData()
{
	return TRUE;
}

//------------------------------------------------------------------------------
BOOL CAbstractFormDoc::OnAttachData()
{
	return TRUE;
}

//------------------------------------------------------------------------------
void CAbstractFormDoc::OnBeforeSave()
{
}

//------------------------------------------------------------------------------
void CAbstractFormDoc::OnAfterSave()
{
}

//------------------------------------------------------------------------------
BOOL CAbstractFormDoc::OnOkEdit()
{
	return TRUE;
}

//------------------------------------------------------------------------------
BOOL CAbstractFormDoc::OnOkNewRecord()
{
	return TRUE;
}

//------------------------------------------------------------------------------
BOOL CAbstractFormDoc::OnOkDelete()
{
	return TRUE;

}

//------------------------------------------------------------------------------
BOOL CAbstractFormDoc::OnOkTransaction()
{
	return TRUE;

}

//------------------------------------------------------------------------------
BOOL CAbstractFormDoc::OnBeforeOkTransaction()
{
	return TRUE;

}

//------------------------------------------------------------------------------
BOOL CAbstractFormDoc::OnNewTransaction()
{
	return TRUE;

}

//------------------------------------------------------------------------------
BOOL CAbstractFormDoc::OnEditTransaction()
{
	return TRUE;

}

//------------------------------------------------------------------------------
BOOL CAbstractFormDoc::OnDeleteTransaction()
{
	return TRUE;

}

//------------------------------------------------------------------------------
void CAbstractFormDoc::OnExtraNewTransaction()
{
}

//------------------------------------------------------------------------------
void CAbstractFormDoc::OnExtraEditTransaction()
{
}

//------------------------------------------------------------------------------
void CAbstractFormDoc::OnExtraDeleteTransaction()
{
}


//------------------------------------------------------------------------------
void CAbstractFormDoc::OnGoInBrowseMode()
{
}

//------------------------------------------------------------------------------
void CAbstractFormDoc::OnDocumentCreated()
{

}

//------------------------------------------------------------------------------
BOOL CAbstractFormDoc::OnInitDocument()
{
	return TRUE;
}


//------------------------------------------------------------------------------
BOOL CAbstractFormDoc::OnExistTables()
{
	return TRUE;
}

//------------------------------------------------------------------------------
BOOL CAbstractFormDoc::OnInitAuxData()
{
	for (int i = 0; i <= m_pHotKeyLinks->GetUpperBound(); i++)
	{
		HotKeyLink* pHotLink = (HotKeyLink*)m_pHotKeyLinks->GetAt(i);
		if (pHotLink && pHotLink->GetAttachedData() && pHotLink->GetAttachedData()->IsEmpty() && pHotLink->GetAttachedRecord())
		{
			pHotLink->GetAttachedRecord()->Init();
			pHotLink->m_PrevResult = HotKeyLink::NONE;
		}
	}

	return TRUE;
}

//------------------------------------------------------------------------------
BOOL CAbstractFormDoc::SetDataFromXMLString(CString strXML, const CString& strXSLTFileName)
{
	return (m_pXMLDataManager) ? m_pXMLDataManager->SetDataFromXMLString(strXML, strXSLTFileName) : FALSE;
}

//------------------------------------------------------------------------------
CString CAbstractFormDoc::GetDataToXMLString(const CString& strProfileName, const CString& strXSLTFileName)
{
	CString strXMLString = _T("");
	if (m_pXMLDataManager)
		strXMLString = m_pXMLDataManager->GetDataToXMLString(strProfileName, strXSLTFileName);

	return strXMLString;
}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::DumpDbts(const CString& sPath)
{
	if (!m_pDBTMaster && !GetSymTable())
		return;

	CXMLDocumentObject aXMLDbts;

	CXMLNode* pRoot = aXMLDbts.CreateRoot(L"Document");
	if (!pRoot)
	{
		ASSERT(FALSE);
		return;
	}

	if (m_pDBTMaster)
		m_pDBTMaster->UnParse(pRoot, TRUE, TRUE, TRUE);

	if (GetSymTable())
		GetSymTable()->UnParse(pRoot);

	aXMLDbts.SaveXMLFile(sPath, TRUE);
}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::DumpDbts()
{
	static int n = 1;
	if (1)
		DumpDbts(cwsprintf(L"c:\\dump_document.xml", n++));
	else
		DumpDbts(cwsprintf(L"c:\\dump_document_%d.xml", n++));
}

//-----------------------------------------------------------------------------
void CAbstractFormDoc::OnAddFormsOnDockPane(CTaskBuilderDockPane* pPane)
{
	__super::OnAddFormsOnDockPane(pPane);
	m_pClientDocs->OnAddFormsOnDockPane(pPane);
}


//-----------------------------------------------------------------------------
void CAbstractFormDoc::DispatchCommandToDocument(UINT nID)
{
	if (GetMasterFrame())
		GetMasterFrame()->SendMessage(WM_COMMAND, nID);
	else
		OnCmdMsg(nID, CN_COMMAND, NULL, NULL);
}


//-----------------------------------------------------------------------------
void CAbstractFormDoc::GetIDsUsedInMessageMap(CArray<int>& arIDs)
{
	arIDs.RemoveAll();
	PopulateIDsArrayFromMessageMap(GetMessageMap(), arIDs);
	m_pClientDocs->PopulateMessagesIDsArrayForPushToClients(arIDs);
}


// Attualmente vengono mandati gli IDs dei controlli che hanno un evento ValueChanged associato
// Gli altri eventi (es. bottoni della toolbar) non sono mandati, perche sono sempre Server-side 
// Via via che troviamo altri eventi che possono essere sia server-side che client-side, modifichiamo questo metodo perche li 
//estrapoli (eventualmente anche in array diversi)
//-----------------------------------------------------------------------------
void CAbstractFormDoc::PopulateIDsArrayFromMessageMap(const AFX_MSGMAP* pMsgMap, CArray<int>& arIDs)
{
	for (int e = 0; ; e++)
	{
		AFX_MSGMAP_ENTRY entry = pMsgMap->lpEntries[e];

		if (entry.nMessage == WM_COMMAND && entry.nCode == EN_VALUE_CHANGED)
		{
			arIDs.Add(entry.nID);
		}

		if (
			entry.nMessage == 0 && entry.nID == 0 &&
			entry.nCode == 0 && entry.nLastID == 0
			)
			break;
	}
	if (pMsgMap->pfnGetBaseMap != NULL)
	{
		PopulateIDsArrayFromMessageMap(pMsgMap->pfnGetBaseMap(), arIDs);
	}
}

//-----------------------------------------------------------------------------
CNumbererBinder* CAbstractFormDoc::GetNumbererBinder()
{
	if (!m_pNumbererBinder)
	{
		m_pNumbererBinder = new CNumbererBinder();
		AddConsumer(m_pNumbererBinder);
	}
	return m_pNumbererBinder;
}

//////////////////////////////////////////////////////////////////////////////
// Diagnostics
#ifdef _DEBUG
void CAbstractFormDoc::Dump(CDumpContext& dc) const
{
	ASSERT_VALID(this);
	AFX_DUMP0(dc, "CAbstractFormDoc\n");
}

void CAbstractFormDoc::AssertValid() const
{
	CAbstractDoc::AssertValid();
}

#endif //_DEBUG

//==============================================================================
//	CDynamicFormDoc
//==============================================================================
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CDynamicFormDoc, CAbstractFormDoc)


BOOL CDynamicFormDoc::OnAttachData()
{
	return TRUE;
}

//--------------------------------------------------------------------------------
BOOL CDynamicFormDoc::OnNewDocument()
{
	if (!__super::OnNewDocument())
		return false;
	const CDocumentDescription* pDescri = GetXmlDescription();
	if (pDescri)
		SetFormTitle(pDescri->GetTitle());
	return TRUE;
}

IMPLEMENT_DYNCREATE(CDynamicBatchFormDoc, CDynamicFormDoc)

//==============================================================================
//	CWizardFormDoc
//==============================================================================

//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CWizardFormDoc, CAbstractFormDoc)

//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CWizardFormDoc, CAbstractFormDoc)
	//{{AFX_MSG_MAP(CWizardFormDoc)
	ON_UPDATE_COMMAND_UI(IDC_WIZARD_NEXT, OnEnableWizardNext)
	ON_UPDATE_COMMAND_UI(IDC_WIZARD_BACK, OnEnableWizardBack)
	ON_UPDATE_COMMAND_UI(IDC_WIZARD_FINISH, OnEnableWizardFinish)
	ON_UPDATE_COMMAND_UI(ID_WIZARD_RESTART, OnEnableWizardRestart)
	ON_UPDATE_COMMAND_UI(IDCANCEL, OnEnableWizardCancel)

	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

CWizardFormDoc::CWizardFormDoc()
{
	m_pWaitingDoc = NULL;
	m_Type = VMT_BATCH;
}

//----------------------------------------------------------------------------
void CWizardFormDoc::WaitDocument()
{
	ASSERT_VALID(m_pWaitingDoc);

	if (m_pWaitingDoc)
		m_pWaitingDoc->DisableAllView();

	//adesso la frame del master doc � owner della mia frame, vengo disabilitato in automatico
	//quindi devo riabilitarmi!!!
	CWnd* pWnd = GetMasterFrame();
	if (pWnd)
		pWnd->EnableWindow();
	AfxGetTbCmdManager()->WaitDocumentEnd(this);
}

//-----------------------------------------------------------------------------
void CWizardFormDoc::OnCloseDocument()
{
	if (m_pWaitingDoc)
	{
		ASSERT_VALID(m_pWaitingDoc);
		m_pWaitingDoc->EnableAllView();
		m_pWaitingDoc->Activate();
	}

	m_pWaitingDoc = NULL;

	CAbstractFormDoc::OnCloseDocument();
}

//-----------------------------------------------------------------------------
BOOL CWizardFormDoc::OnAttachData()
{
	DECLARE_VAR_JSON(HeaderTitle);
	DECLARE_VAR_JSON(HeaderSubTitle);
	return TRUE;
}

//---------------------------------------------------------------------------------------
void CWizardFormDoc::OnParsedControlCreated(CParsedCtrl* pCtrl)
{
	UINT nIDC = (UINT)pCtrl->GetCtrlID();

	if (nIDC == IDC_TB_HEADER_TITLE)
	{
		CStrStatic* pHeaderTitle = dynamic_cast<CStrStatic*>(pCtrl);

		if (!pHeaderTitle)
			return;

		pHeaderTitle->SetOwnFont(m_bHeaderTitleBold, FALSE, FALSE);
		return;
	}

	if (nIDC == IDC_TB_HEADER_SUBTITLE)
	{
		CStrStatic* pHeaderSubTitle = dynamic_cast<CStrStatic*>(pCtrl);

		if (!pHeaderSubTitle)
			return;

		pHeaderSubTitle->SetBkgColor(AfxGetTileDialogStyleHeader()->GetBackgroundColor());
		pHeaderSubTitle->SetTextColor(AfxGetTileDialogStyleHeader()->GetTitleForeColor());
		pHeaderSubTitle->SetOwnFont(m_bHeaderSubTitleBold, FALSE, FALSE);
		return;
	}
}

//-----------------------------------------------------------------------------
void CWizardFormDoc::SetTitle(LPCTSTR lpszTitle)
{
	m_strWizardTitle = lpszTitle;
	CAbstractFormDoc::SetTitle(lpszTitle);
}
//-----------------------------------------------------------------------------
inline CWizardFormView* CWizardFormDoc::GetWizardView()
{
	return dynamic_cast<CWizardFormView*>(GetFirstView());
}

//-----------------------------------------------------------------------------
inline CTabWizard* CWizardFormDoc::GetTabWizard()
{
	CWizardFormView* pView = GetWizardView();
	return pView ? pView->GetTabManager() : NULL;
}

//---------------------------------------------------------------------------
LRESULT CWizardFormDoc::OnWizardNext(UINT /*IDD*/) { return WIZARD_DEFAULT_TAB; }
//---------------------------------------------------------------------------
LRESULT CWizardFormDoc::OnWizardBack(UINT /*IDD*/) { return WIZARD_DEFAULT_TAB; }
//---------------------------------------------------------------------------
LRESULT CWizardFormDoc::OnWizardCancel(UINT /*IDD*/) { return WIZARD_DEFAULT_TAB; }
//---------------------------------------------------------------------------
LRESULT	CWizardFormDoc::OnWizardFinish(UINT /*IDD*/) { return WIZARD_DEFAULT_TAB; }

//---------------------------------------------------------------------------
void CWizardFormDoc::OnEnableWizardNext(CCmdUI* pCmdUI)
{
	CWizardFormView* pView = GetWizardView();
	if (pView)
		pCmdUI->Enable(pView->IsToEnableWizardNext());
}
//---------------------------------------------------------------------------
void CWizardFormDoc::OnEnableWizardBack(CCmdUI* pCmdUI)
{
	CWizardFormView* pView = GetWizardView();
	if (pView)
		pCmdUI->Enable(pView->IsToEnableWizardBack());
}
//---------------------------------------------------------------------------
void CWizardFormDoc::OnEnableWizardFinish(CCmdUI* pCmdUI)
{
	CWizardFormView* pView = GetWizardView();
	if (pView)
		pCmdUI->Enable(pView->IsToEnableWizardFinish());
}

//---------------------------------------------------------------------------
void CWizardFormDoc::OnEnableWizardCancel(CCmdUI* pCmdUI)
{
	pCmdUI->Enable(TRUE);
}


//---------------------------------------------------------------------------
BOOL CWizardFormDoc::CanDoBatchExecute()
{
	CWizardFormView* pView = GetWizardView();
	return pView ? pView->IsToEnableWizardFinish() : __super::CanDoBatchExecute();
}
//---------------------------------------------------------------------------
void CWizardFormDoc::OnEnableWizardRestart(CCmdUI* pCmdUI)
{
	CWizardFormView* pView = GetWizardView();
	if (pView)
		pCmdUI->Enable(pView->IsToEnableWizardRestart());
}
//---------------------------------------------------------------------------
LRESULT CWizardFormDoc::DispatchWizardNext(UINT nIDD)
{
	LRESULT res = WIZARD_DEFAULT_TAB;
	if (m_pClientDocs)
		res = m_pClientDocs->OnWizardNext(nIDD);
	if (res == WIZARD_DEFAULT_TAB)
		res = OnWizardNext(nIDD);
	return res;
}

//---------------------------------------------------------------------------
LRESULT CWizardFormDoc::DispatchWizardBack(UINT nIDD)
{
	LRESULT res = WIZARD_DEFAULT_TAB;
	if (m_pClientDocs)
		res = m_pClientDocs->OnWizardBack(nIDD);
	if (res == WIZARD_DEFAULT_TAB)
		res = OnWizardBack(nIDD);
	return res;
}

//---------------------------------------------------------------------------
LRESULT CWizardFormDoc::DispatchWizardCancel(UINT nIDD)
{
	LRESULT res = WIZARD_DEFAULT_TAB;
	if (m_pClientDocs)
		res = m_pClientDocs->OnWizardCancel(nIDD);
	if (res == WIZARD_DEFAULT_TAB)
		res = OnWizardCancel(nIDD);
	return res;
}

//---------------------------------------------------------------------------
LRESULT CWizardFormDoc::DispatchWizardFinish(UINT nIDD)
{
	LRESULT res = WIZARD_DEFAULT_TAB;
	if (m_pClientDocs)
		res = m_pClientDocs->OnWizardFinish(nIDD);
	if (res == WIZARD_DEFAULT_TAB)
		res = OnWizardFinish(nIDD);
	return res;
}

//---------------------------------------------------------------------------
LRESULT CWizardFormDoc::DispatchOnBeforeWizardFinish(UINT nIDD)
{
	return (m_pClientDocs) ? m_pClientDocs->OnBeforeWizardFinish(nIDD) : WIZARD_DEFAULT_TAB;
}
//---------------------------------------------------------------------------
void CWizardFormDoc::DispatchUpdateWizardButtons(UINT nIDD)
{
	if (m_pClientDocs)
		m_pClientDocs->OnUpdateWizardButtons(nIDD);
}

//---------------------------------------------------------------------------
LRESULT CWizardFormDoc::DispatchGetBitmapID(UINT nIDD)
{
	return (m_pClientDocs) ? m_pClientDocs->OnGetBitmapID(nIDD) : 0;
}

//---------------------------------------------------------------------------
void CWizardFormDoc::DispatchDeactivate(UINT nIDD)
{
	OnWizardDeactivate(nIDD);
	if (m_pClientDocs)
		m_pClientDocs->OnWizardDeactivate(nIDD);
}
//---------------------------------------------------------------------------
void CWizardFormDoc::OnFrameCreated()
{
	__super::OnFrameCreated();
	CWizardFormView* pView = GetWizardView();
	if (pView)
		pView->EnableButtons(0);
}

//---------------------------------------------------------------------------
void CWizardFormDoc::DispatchActivate(UINT nIDD)
{
	OnWizardActivate(nIDD);
	if (m_pClientDocs)
		m_pClientDocs->OnWizardActivate(nIDD);
}

/////////////////////////////////////////////////////////////////////////////
// Diagnostics

#ifdef _DEBUG
void CWizardFormDoc::Dump(CDumpContext& dc) const
{
	ASSERT_VALID(this);
	AFX_DUMP0(dc, "CWizardFormDoc\n");
}

void CWizardFormDoc::AssertValid() const
{
	CAbstractFormDoc::AssertValid();
}
#endif //_DEBUG

IMPLEMENT_DYNCREATE(DBrowserDocument, CAbstractFormDoc)

//---------------------------------------------------------------------------
DBrowserDocument::DBrowserDocument()
{
	m_bBatch = TRUE;
}
//---------------------------------------------------------------------------
BOOL DBrowserDocument::OnOpenDocument(LPCTSTR lpszPathName)
{
	if (!CAbstractFormDoc::OnOpenDocument(lpszPathName))
		return FALSE;

	LPAUXINFO lpInfo = GET_AUXINFO(lpszPathName);
	if (lpInfo)
	{
		m_strUrl = (LPCTSTR)lpInfo;
	}

	return TRUE;
}

//---------------------------------------------------------------------------
BOOL DBrowserDocument::OnAttachData(void)
{
	DECLARE_VAR(_T("Url"), m_strUrl);
	return TRUE;
}

//---------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CBrowserDocumentView, CJsonFormView)

//---------------------------------------------------------------------------
CBrowserDocumentView::CBrowserDocumentView() : CJsonFormView(IDD_BROWSER)
{
}
