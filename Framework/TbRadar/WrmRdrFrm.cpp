
#include "stdafx.h"

#include <TbFrameworkImages\CommonImages.h>
#include <TbFrameworkImages\GeneralFunctions.h>

#include <TbGeneric\globals.h>
#include <TbGenlib\baseapp.h>

#include <TbWoormEngine\inputmng.h>
#include <TbWoormViewer\WoormDoc.hjson> //JSON AUTOMATIC UPDATE
#include <TbWoormViewer\export.h>

#include "WrmRdrdoc.h"
#include "WrmRdrfrm.h"
#include "WrmRdrvw.h"
#include "WrmRdrFrm.h"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif

//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CWrmRadarFrame, CWoormFrame)

BEGIN_MESSAGE_MAP(CWrmRadarFrame, CWoormFrame)
	//{{AFX_MSG_MAP(CWrmRadarFrame)
	ON_WM_ACTIVATE()
	ON_COMMAND(ID_WRMRADAR_FIXED,	OnFixed)
	ON_UPDATE_COMMAND_UI (ID_WRMRADAR_FIXED,	OnUpdateFixed)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()  

//-----------------------------------------------------------------------------
CWrmRadarFrame::CWrmRadarFrame()
{
	SetDockable(FALSE);
}

//-----------------------------------------------------------------------------
CWrmRadarFrame::~CWrmRadarFrame()
{
}

//-----------------------------------------------------------------------------
BOOL CWrmRadarFrame::CreateTools(CWoormDocMng* pDoc/* = NULL*/)
{
	if	(!__super::CreateTools(pDoc))
	{
		TRACE("Failed to create WrmRadarbar\n");
		return FALSE;      // fail to create
	}

	return TRUE;
}

//-----------------------------------------------------------------------------
void CWrmRadarFrame::OnActivate(UINT nState, CWnd* pWndOther, BOOL bMinimized)
{
	__super::OnActivate(nState, pWndOther, bMinimized);

	OnActivateHandler(pWndOther && nState != WA_INACTIVE, pWndOther);
}

//-----------------------------------------------------------------------------
BOOL CWrmRadarFrame::PreCreateWindow(CREATESTRUCT& cs)
{
	if (!__super::PreCreateWindow(cs)) return FALSE;
   
	cs.hwndParent = GetValidOwner();
		
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CWrmRadarFrame::Create(LPCTSTR lpszClassName, LPCTSTR lpszWindowName, DWORD dwStyle, const RECT& rect, CWnd* pParentWnd, LPCTSTR lpszMenuName, DWORD dwExStyle, CCreateContext* pContext)
{
	BOOL bRet = __super::Create(lpszClassName, lpszWindowName, dwStyle, rect, pParentWnd, lpszMenuName, dwExStyle, pContext);

	return bRet;
}

//-----------------------------------------------------------------------------
void CWrmRadarFrame::OnActivateHandler(BOOL bActive, CWnd* pActiveWnd)
{
	CWrmRadarView* pView = (CWrmRadarView*) GetActiveView();
	if (pView == NULL)
		return;

	CWrmRadarDoc* pDoc	= pView->GetDocument();
	if (pDoc == NULL)
		return;

	// se perde il fuoco e non deve rimanere attaccata si autouccide
	// e prima di morire deve scollegarsi dal documento chiamante.
	// Attenzione se perde il fuoco per una richiesta di limiti che e` fatta
	// non con dialog ma con MDI allora non deve morire.
	//
	if 
		(
			!bActive && !InModalState() && pDoc->CanDoExit()  && IsWindowEnabled()  
			&& 
			(pActiveWnd ? !pActiveWnd->IsKindOf(RUNTIME_CLASS(CFindWordDlg)) : TRUE)	//TODO piu' logico FALSE ?
		)
	{
			PostMessage	(WM_CLOSE);
			if (pDoc->m_FindDlg)
				pDoc->m_FindDlg->DestroyWindow();
	}
}

// Cambia la stato del bottone di stay alive in accordo con lo stato attuale.
// Automaticamente modifica lo stato del bottone di Link WrmRadar
//-----------------------------------------------------------------------------
void CWrmRadarFrame::OnFixed()
{
	GetWrmRadarDocument()->ChangeStayAlive();
	UpdateButtonFixedCheck(GetWrmRadarDocument()->IsStayAlive());
}

// Funzioni che abilitano / disabilitano i vari bottoni presenti nella toolbar
// in base allo stato attuale del WrmRadar
//-----------------------------------------------------------------------------
void CWrmRadarFrame::OnUpdateFixed(CCmdUI* pCmdUI)
{
	// se non c'e` ancora il documento porto via le scatole
	if (GetActiveDocument() == NULL)
		return;

	BOOL bEnable = GetWrmRadarDocument()->IsEnableStayAlive();
	pCmdUI->Enable(bEnable);

	if (bEnable)
		pCmdUI->SetCheck (GetWrmRadarDocument()->IsStayAlive());
}

// Aggiorna lo stato del bottone di pin (spillatura)
//-----------------------------------------------------------------------------
void CWrmRadarFrame::UpdateButtonFixedCheck(BOOL bFixedCheck)
{
	if (GetMainToolBar()) 
		GetMainToolBar()->SetButtonInfo (ID_WRMRADAR_FIXED, TBBS_BUTTON,
			bFixedCheck ? 
			TBIcon(szIconPinned, TOOLBAR) :
			TBIcon(szIconPin, TOOLBAR)
			);
	}

//-----------------------------------------------------------------------------
CWrmRadarDoc* CWrmRadarFrame::GetWrmRadarDocument()
{
	CWrmRadarDoc* pDoc = (CWrmRadarDoc*) GetActiveDocument();
	ASSERT_VALID(pDoc);
	return pDoc;
}

//-----------------------------------------------------------------------------
CWrmRadarView* CWrmRadarFrame::GetWrmRadarView()
{ 
	CWrmRadarView* pView = GetWrmRadarDocument()->GetWrmRadarView();
	ASSERT(pView);
	return pView;
}
///////////////////////////////////////////////////////////////////////////////
// Diagnostics
#ifdef _DEBUG
void CWrmRadarFrame::Dump (CDumpContext& dc) const
{
	ASSERT_VALID (this);
	AFX_DUMP0(dc, " CWrmRadarFarme\n");
	CWoormFrame::Dump(dc);
}
#endif // _DEBUG
