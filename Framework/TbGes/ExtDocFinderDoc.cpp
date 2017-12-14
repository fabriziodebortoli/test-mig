
#include "stdafx.h"
#include <TbFrameworkImages\CommonImages.h>
#include <TbFrameworkImages\GeneralFunctions.h>

#include <TbGeneric\minmax.h>
#include <TbGeneric\globals.h>
#include <TbGeneric\DataObj.h>
#include <TbGeneric\StatusBarMessages.h>
#include <TbGeneric\LocalizableObjs.h>

#include <TbGenlib\reswalk.h>
#include <TbGenlib\parsobj.h>
#include <TbGenlib\messages.h>
#include <TbGenlib\generic.h>

#include <TbOledb\oledbmng.h>
#include <TbOledb\sqlrec.h>
#include <TbOledb\sqltable.h>			
#include <TbOledb\sqlcatalog.h>	

#include <TbWoormEngine\report.h>

#include "browser.h"
#include "bodyedit.h"
#include "hotlink.h"
#include "dbt.h"
#include "barquery.h"
#include "extdoc.h"
#include "tabber.h"
#include "formmng.h"
#include "JsonForms\TbGes\IDD_EXTDOC_FINDER_TOOLBAR.hjson"

//................................. resources
#include "extdoc.hjson" //JSON AUTOMATIC UPDATE
#include "barquery.hjson" //JSON AUTOMATIC UPDATE


//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

//==============================================================================
//						CFinderDoc
//==============================================================================

// ToolBar Rimappata                   
//-----------------------------------------------------------------------------
#define TOOLBAR_BTN_STAYALIVE_ON 13
#define TOOLBAR_BTN_STAYALIVE_OFF 0

#define STANDARD_COMMAND_PREFIX _T("ToolbarButton.Framework.TbGes.TbGes.AbstractFormDoc.")

//-----------------------------------------------------------------------------
CString FinderNamespace(const CString& aName)
{
	return CString(STANDARD_COMMAND_PREFIX) + aName;
}

//
//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CFinderDoc, CAbstractFormDoc)

//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CFinderDoc, CAbstractFormDoc)
	ON_COMMAND	(ID_EXTDOC_SAVE, 		OnSaveRecord)
	ON_COMMAND	(ID_EXTDOC_EXEC_QUERY,	OnRequery)
	ON_COMMAND	(ID_FINDER_STAYALIVE,	OnStayAlive)

	// Abilitazione della combo box per la query intelligente
	ON_UPDATE_COMMAND_UI(ID_EXTDOC_EXEC_QUERY,	OnUpdateExecQuery)
	ON_UPDATE_COMMAND_UI(ID_EXTDOC_SAVE, 		OnUpdateSaveRecord)
	ON_UPDATE_COMMAND_UI(ID_FINDER_STAYALIVE, 	OnUpdateStayAlive)
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CFinderDoc::CFinderDoc()
	:
	m_bStayAlive					(FALSE),
	m_pAttachedDocument				(NULL),
	m_bAttachedDocumentManagement	(AUXINFO)
{                                   
	m_bBrowseOnCreate	= TRUE;
	m_Type				= VMT_FINDER; 

	// non si possono abilitare le query non primarie.
	ASSERT(m_pQueryManager);
	m_pQueryManager->DisableQuery();
}

// I ricercatori (Finder document) non possono partire senza avere un documento
// attaccato
//-----------------------------------------------------------------------------
BOOL CFinderDoc::OnOpenDocument(LPCTSTR pParam)
{
	if (!pParam)
		return FALSE;

	BOOL bOk = TRUE;
	switch (m_bAttachedDocumentManagement)
	{
		case NONE:
		case AUXINFO:
			m_pAttachedDocument = (CAbstractFormDoc*)GET_AUXINFO(pParam);
			break;
		case ANCESTOR:
			m_pAttachedDocument = (CAbstractFormDoc*)GET_ANCESTOR(pParam);
			break;
	}

	ASSERT(m_pAttachedDocument->IsKindOf(RUNTIME_CLASS(CAbstractFormDoc)));

	return CAbstractFormDoc::OnOpenDocument(pParam);
}

// Non puo` essere usato. Bisogna per forza usare OnOpenDocument con un documento
// attaccato.
//-----------------------------------------------------------------------------
BOOL CFinderDoc::OnNewDocument()
{
	ASSERT(FALSE);
	return FALSE;
}

// Il Finder document non si salva mai. Il comando di Salva serve per eseguire
// particolari azioni sul documento attaccato.
//-----------------------------------------------------------------------------
BOOL CFinderDoc::CanDoSaveRecord()
{ 
	return GetType() != VMT_BATCH && m_pDBTMaster && GetFormMode() == BROWSE; 
}

// Permette di cambiare lo stato di 'stay alive'. 
// Se è attivo viene disattivato altrimenti attivato. 
//-----------------------------------------------------------------------------
BOOL CFinderDoc::ChangeStayAlive()
{
	m_bStayAlive = !m_bStayAlive;
	return m_bStayAlive;
}

// permette di caricare o manipolare l'attached document
//-----------------------------------------------------------------------------
void CFinderDoc::OnSaveRecord()
{                                                
	// serve per bypassare l'acceleratore!
	if (!CanDoSaveRecord())
		return;
		
	// something is wrong in some dialog control edit
	// also force SetModifiedFlag to be set if some control is modified
	// force Update and test for all controls view
	if (GetNotValidView(TRUE))
		return; 

	if (m_bAttachedDocumentManagement != NONE)
	{
		// Paramento di culo: non dovrebbe nemmeno abilitarsi il bottone di applica, 
		// ma non si sa mai.
		ASSERT(m_pAttachedDocument);
		if (!m_pAttachedDocument)
			return;
	}
	
	// force Update for all controls view
	GetNotValidView(FALSE);

	//Prj. 6709 - improve performance 
	//inibisco la UpdateDataView durante il processo di travaso dei dati per velocizzare il processo 
	//di carico + calcolo dei DataObj

	if (m_pAttachedDocument)
		m_pAttachedDocument->SuspendUpdateDataView();

	OnLoadAttachedDocument();

	if (m_pAttachedDocument)
		m_pAttachedDocument->DispatchOnLoadAttachedDocument(this);

	if (m_pAttachedDocument)
		m_pAttachedDocument->ResumeUpdateDataView();
	
	if (!IsStayAlive())
		CloseDocument();
	else
		m_pAttachedDocument->Activate();
}


// si occupa di riallineare il browser ai nuovi filtri di query. E necessario
// cancellare il Browser perche` monticelli soli durante la creazione dello
// stesso (attach del browser al bottone) parsa la query predefinita.
//-----------------------------------------------------------------------------
void CFinderDoc::OnRequery()
{
	// garbage collection delle strutture di monti. MUST BE DONE!
	ASSERT(m_pQueryManager);
	m_pQueryManager->Detach();

	// ridisabilita il bottone destro (query aggiuntive) che viene
	// di default abilitato dalla DetachToolbar
	m_pQueryManager->DisableQuery();
	
	ASSERT(m_pBrowser);
	SAFE_DELETE(m_pBrowser);

	// Ricrea il browser simulando la creazione fatta dalla OnOpenDocument
	// iniziale
	CreateBrowser((LPCTSTR)m_pDocInvocationInfo);
}

// Cambia la stato del bottone di stay alive in accordo con lo stato attuale.
//-----------------------------------------------------------------------------
void CFinderDoc::OnStayAlive()
{
	// Controlla se è stay alive, e selezione l'id del bitmap da visualizzare
	UINT nIdx = ChangeStayAlive() ? TOOLBAR_BTN_STAYALIVE_ON : TOOLBAR_BTN_STAYALIVE_OFF;
	CMasterFrame* pFrame = GetMasterFrame();
	if (pFrame)
	{
		CTBTabbedToolbar* pTabToolBar = pFrame->GetTabbedToolBar();
		if (pTabToolBar)
		{
			if (nIdx == TOOLBAR_BTN_STAYALIVE_ON)
				pTabToolBar->SetButtonInfo(ID_FINDER_STAYALIVE, TBBS_BUTTON, TBIcon(szIconPinned, TOOLBAR), _TB("Unpin"));
			else 
				pTabToolBar->SetButtonInfo(ID_FINDER_STAYALIVE, TBBS_BUTTON, TBIcon(szIconPin, TOOLBAR), _TB("Pin")); 

			pFrame->SetOwner(IsStayAlive() ? pFrame->GetValidOwner() : NULL);
		}
	}
}

// Funzione che gestisce il tasto di fissa
//-----------------------------------------------------------------------------
void CFinderDoc::OnUpdateStayAlive(CCmdUI* pCmdUI)
{
	pCmdUI->SetCheck (IsStayAlive());
}


/////////////////////////////////////////////////////////////////////////////
// Diagnostics

#ifdef _DEBUG
void CFinderDoc::Dump(CDumpContext& dc) const
{
	ASSERT_VALID(this);
	AFX_DUMP0(dc, " CFinderDoc\n");
}

void CFinderDoc::AssertValid() const
{
	CAbstractFormDoc::AssertValid();
}
#endif //_DEBUG

//////////////////////////////////////////////////////////////////////////////
//							CFinderFrame
//////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CFinderFrame, CMasterFrame)

BEGIN_MESSAGE_MAP(CFinderFrame, CMasterFrame)
	ON_REGISTERED_MESSAGE(BCGM_CHANGEVISUALMANAGER, OnChangeVisualManager)
END_MESSAGE_MAP()  

//-----------------------------------------------------------------------------
CFinderFrame::CFinderFrame ()
	:
	CMasterFrame()
{
	SetDockable(FALSE);	
}

//-----------------------------------------------------------------------------------
LRESULT CFinderFrame::OnChangeVisualManager(WPARAM wParam, LPARAM lParam)
{
	return 0L;
}

//-----------------------------------------------------------------------------
BOOL CFinderFrame::PreCreateWindow(CREATESTRUCT& cs)
{
	BOOL b = __super::PreCreateWindow(cs);
	cs.hwndParent = NULL;//il finder non ha parent, a meno che sia pinnato
	return b;
}

//-----------------------------------------------------------------------------
BOOL CFinderFrame::OnCustomizeJsonToolBar()
{
	return CreateJsonToolbar(IDD_EXTDOC_FINDER_TOOLBAR);
}

//-----------------------------------------------------------------------------
BOOL CFinderFrame::OnCustomizeTabbedToolBar	(CTBTabbedToolbar* pTabbedBar)
{
	return TRUE;

}

//-----------------------------------------------------------------------------
BOOL CFinderFrame::Create(LPCTSTR lpszClassName, LPCTSTR lpszWindowName, DWORD dwStyle, const RECT& rect, CWnd* pParentWnd, LPCTSTR lpszMenuName, DWORD dwExStyle, CCreateContext* pContext)
{
	return __super::Create(lpszClassName, lpszWindowName, dwStyle, rect, pParentWnd, NULL/*nessun menu!*/, dwExStyle, pContext);
}

//=====================================================================================
