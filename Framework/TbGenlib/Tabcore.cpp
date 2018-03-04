
#include "stdafx.h"
#include <TbGeneric\globals.h>
#include <TbGeneric\array.h>
#include <TbGeneric\VisualStylesXP.h>
#include <TbGeneric\LocalizableObjs.h>
#include <TbGeneric\TBThemeManager.h>
#include <TbGeneric\JsonFormEngine.h>
#include <TbGenlib\commands.hrc>

#include <TbFrameworkImages\CommonImages.h>
#include <TbFrameworkImages\GeneralFunctions.h>

#include "TBTabWnd.h"
#include "TabSelector.h"
#include "funproto.h"
#include "baseapp.h"
#include "oslbaseinterface.h"
#include "tabcore.h"
#include "BaseTileManager.h"


#include "parsres.hjson" //JSON AUTOMATIC UPDATE

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#define new DEBUG_NEW
#endif

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif

#define TAB_IMG_OFFSET _T("  ")

/////////////////////////////////////////////////////////////////////////////
//					class CBaseTabDialog implementation
/////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CBaseTabDialog, CParsedDialog)

//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CBaseTabDialog, CParsedDialog)
	//{{AFX_MSG_MAP(CBaseTabDialog)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CBaseTabDialog::CBaseTabDialog (const CString& sName, UINT nIDD/*=0*/)
	: 
	CParsedDialog	(),
	m_pDlgInfo		(NULL),
	m_bOwnsDlgInfo	(FALSE),
	m_pBkgnd		(NULL)
{
	m_sName	= sName;
	m_nID	= nIDD;
	m_bInOpenDlgCounter = FALSE;

	CString strName = sName;
	strName.Trim();
	if (strName.IsEmpty())
		strName = GetRuntimeClass()->m_lpszClassName;
	m_sName = strName;
}
//------------------------------------------------------------------------------
CBaseTabDialog::~CBaseTabDialog ()
{
	if (m_bOwnsDlgInfo && m_pDlgInfo)
		delete m_pDlgInfo;
}

//-----------------------------------------------------------------------------
BOOL CBaseTabDialog::Create (CBaseTabManager* pTabManager)
{
	m_pDlgInfo->m_pBaseTabDlg = this; 

	CParsedForm::SetBackgroundColor(pTabManager->GetBackColor());

	if (m_nID == 0)
	{
		ASSERT(FALSE);
		TRACE0("IDD in CBaseTabDialog. Create method failed, tab m_nID is not initialized\n");
		return FALSE;
    }
    
	if (!CParsedDialog::Create(m_nID, pTabManager, m_sName))
		return FALSE;
	
	RefreshBackgroundImage();

	return TRUE;
}

//------------------------------------------------------------------------------
void CBaseTabDialog::RefreshBackgroundImage()
{
	CParsedForm::SetBackgroundImage(m_pDlgInfo->m_strBkgndImage, this->m_locBackgroundImg, this->m_csOrigin);
}

//------------------------------------------------------------------------------
void CBaseTabDialog::PostNcDestroy()
{
	ASSERT(m_hWnd == NULL);
	m_pDlgInfo->m_pBaseTabDlg = NULL; 
	delete this;
}

//-----------------------------------------------------------------------------
void CBaseTabDialog::OnOK()
{
}

//-----------------------------------------------------------------------------
void CBaseTabDialog::OnCancel()
{
}

//--------------------------------------------------------------------------
BOOL CBaseTabDialog::PreProcessMessage(MSG* pMsg)
{
#ifndef _OLD_PTM

	//	Germano : la PreProcessMessage ora è attrezzata per ruotare i messaggi corettamente
	return CParsedForm::PreProcessMessage(pMsg);

#else

	//in design mode solo il form editor gestisce i messaggi
	if (GetDocument() && GetDocument()->IsInDesignMode() && pMsg->message >= WM_KEYFIRST && pMsg->message <= WM_KEYLAST)
		return FALSE;

	// since next statement will eat frame window accelerators,
	//   we call the ParsedForm::PreProcessMessage first
	if (CParsedForm::PreProcessMessage(pMsg))
		return TRUE;

	CWnd* pTabManger = GetParent();

	if (!pTabManger || pTabManger->PreTranslateMessage(pMsg))
		return TRUE;        // eaten by CTabManager

	// si ottiene il il "contenitore" del Tab
	CWnd* pContainer = pTabManger->GetParent();
	
	// don't translate dialog messages when in Shift+F1 help mode
	CFrameWnd* pFrameWnd = pContainer ? pContainer->GetTopLevelFrame() : NULL;
	if (pFrameWnd != NULL && pFrameWnd->m_bHelpMode)
		return FALSE;
	
	if (pContainer->IsKindOf(RUNTIME_CLASS(CBaseTabDialog)))
	{
		if (((CBaseTabDialog*)pContainer)->PreProcessMessage(pMsg))
			return TRUE;        // eaten by dialog accelerator
	}
	else
	{
		// since 'CBaseTabDialog::PreTranslateMessage' will eat frame window accelerators,
		//   we call the frame window's PreTranslateMessage first
		if ((pFrameWnd = pContainer->GetParentFrame()) != NULL)
		{
			if (pFrameWnd->PreTranslateMessage(pMsg))
				return TRUE;        // eaten by frame accelerator

			// check for parent of the frame in case of MDI
			if	(
					(pFrameWnd = pFrameWnd->GetParentFrame()) != NULL &&
					pFrameWnd->PreTranslateMessage(pMsg)
				)
				return TRUE;        // eaten by frame accelerator
		}
	}

	return FALSE;

#endif
}

//-----------------------------------------------------------------------------
void CBaseTabDialog::GetUsedRect(CRect &rectUsed)
{
	CRect aRectTabDialog;
	GetWindowRect(aRectTabDialog);
	CBaseTileGroup* pTileGroup = GetChildTileGroup();
	if (pTileGroup)
	{
		pTileGroup->GetUsedRect(rectUsed);
	}
	else  //potrebbe avere a sua volta un TabManager interno
	{
		CBaseTabManager* pBaseTabManager = GetChildTabManager();
		if (pBaseTabManager)
			pBaseTabManager->GetUsedRect(rectUsed);
	}
	rectUsed.UnionRect(aRectTabDialog, rectUsed);
}

/////////////////////////////////////////////////////////////////////////////
// CBaseTabDialog diagnostics

#ifdef _DEBUG
void CBaseTabDialog::AssertValid() const
{
	CParsedDialog::AssertValid();
}

void CBaseTabDialog::Dump(CDumpContext& dc) const
{
	CParsedDialog::Dump(dc);
	AFX_DUMP0(dc, "\nCTabDlg");
}
#endif //_DEBUG

//////////////////////////////////////////////////////////////////////////////
//							DlgInfoItem Implementation
//////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(DlgInfoItem, CObject)

//-----------------------------------------------------------------------------
DlgInfoItem::DlgInfoItem(CRuntimeClass*	pDialogClass, UINT nDialogID, int nOrdPos/*= -1*/)
	:
	IOSLObjectManager(OSLType_TabDialog),
	m_pDialogClass	(pDialogClass),
	m_nDialogID		(nDialogID),
	m_nLastFocusIDC	(0),
	m_pCellPos		(new CGridControlCellPos),
	m_nOrdPos		(nOrdPos),
	m_bEnabled		(TRUE),
	m_bVisible		(TRUE),
	m_nIconIndex	(-1),
	m_pBaseTabDlg	(NULL)
{
	m_hResourceModule = GetDllInstance(pDialogClass);

	InitJsonContext();
}	

//-----------------------------------------------------------------------------
DlgInfoItem::DlgInfoItem (
							CRuntimeClass*		pDialogClass, 
							UINT				nDialogID, 
							const CTBNamespace& sNamespace, 
							const CString&		sTitle, 
							int					nOrdPos /*= -1*/ )
	:
	IOSLObjectManager(OSLType_TabDialog),
	m_pDialogClass	(pDialogClass),
	m_nDialogID		(nDialogID),
	m_nLastFocusIDC	(0),
	m_pCellPos		(new CGridControlCellPos),
	m_nOrdPos		(nOrdPos),
	m_bEnabled		(TRUE),
	m_bVisible		(TRUE),
	m_nIconIndex	(-1),
	m_pBaseTabDlg	(NULL)
{
	m_strTitle = sTitle;
	GetInfoOSL()->m_Namespace = sNamespace;
	m_hResourceModule = GetDllInstance(pDialogClass);

	InitJsonContext();
}

//-----------------------------------------------------------------------------
DlgInfoItem::~DlgInfoItem()
{
	ASSERT(m_pCellPos);
	
	delete m_pCellPos;
	delete m_pJsonContext;
}

//-----------------------------------------------------------------------------
void DlgInfoItem::InitJsonContext()
{
	CJsonResource res = AfxGetTBResourcesMap()->DecodeID(TbResources, m_nDialogID);
	m_pJsonContext = res.IsEmpty()
		? NULL
		: CJsonFormEngineObj::GetInstance()->CreateContext(res);
	if (m_pJsonContext && m_strTitle.IsEmpty())
		m_strTitle = AfxLoadJsonString(m_pJsonContext->m_pDescription->m_strText, m_pJsonContext->m_pDescription);
}
//-----------------------------------------------------------------------------
void DlgInfoItem::SetResourceModule(HINSTANCE hInstance) 
{
	if (m_hResourceModule == hInstance)
		return;

	m_hResourceModule = hInstance; 
	delete m_pJsonContext;
	InitJsonContext();
}
//-----------------------------------------------------------------------------
void DlgInfoItem::SetDialogID (UINT nID)
{ 
	m_nDialogID = nID; 
}

//-----------------------------------------------------------------------------
void DlgInfoItem::SetLastFocusIDC(UINT nIDC)
{ 
	m_nLastFocusIDC = nIDC; 
}

//-----------------------------------------------------------------------------
void DlgInfoItem::SetCellPos (const CGridControlCellPos& cp)
{ 
	if (m_pCellPos)
		*m_pCellPos = cp;
	else
		m_pCellPos = new CGridControlCellPos(cp);
}

//-----------------------------------------------------------------------------
void DlgInfoItem::SetCellPos (CGridControlCellPos* pCellPos)
{ 
	SAFE_DELETE(m_pCellPos);
	m_pCellPos = pCellPos; 
}

//-----------------------------------------------------------------------------
void DlgInfoItem::SetOrdPos(const int& nOrdPos)
{
	m_nOrdPos = nOrdPos;
}

//-----------------------------------------------------------------------------
void DlgInfoItem::SetSelectorImage	(const CString& nsSelectorImage)
{
	m_nsSelectorImage = nsSelectorImage;
}

//-----------------------------------------------------------------------------
void DlgInfoItem::SetSelectorTooltip (const CString& sSelectorTooltip)
{
	m_sSelectorTooltip = sSelectorTooltip;
}

/////////////////////////////////////////////////////////////////////////////
// Diagnostics

#ifdef _DEBUG
void DlgInfoItem::Dump(CDumpContext& dc) const
{
	ASSERT_VALID(this);
	AFX_DUMP0(dc, " DlgInfoItem\n");
	CObject::Dump(dc);
}

void DlgInfoItem::AssertValid() const
{
	CObject::AssertValid();
}
#endif //_DEBUG

//////////////////////////////////////////////////////////////////////////////
//					DlgInfoArray implementation
//////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(DlgInfoArray, Array)

int	DlgInfoArray::Find (const CString& sName) const
{
	for (int nTab = 0; nTab <= GetUpperBound(); nTab++)
	{
		DlgInfoItem* pTabDlgInf	= GetAt(nTab);

		if (sName.CompareNoCase(pTabDlgInf->GetNamespace().GetObjectName()) == 0)
			return nTab;
	}
	return -1;
}

/////////////////////////////////////////////////////////////////////////////
// Diagnostics

#ifdef _DEBUG
void DlgInfoArray::Dump(CDumpContext& dc) const
{
	ASSERT_VALID(this);
	AFX_DUMP0(dc, " DlgInfoArray\n");
	Array::Dump(dc);
}

void DlgInfoArray::AssertValid() const
{
	Array::AssertValid();
}
#endif //_DEBUG

/////////////////////////////////////////////////////////////////////////////
// 							CBaseTabManager
/////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CBaseTabManager, CTabSelector)

//-----------------------------------------------------------------------------
// Define our offsets for drawing
#define TCEX_SELECTED_XOFFSET	7
#define TCEX_SELECTED_YOFFSET	0
#define TCEX_UNSELECTED_XOFFSET	4
#define TCEX_UNSELECTED_YOFFSET	2
#define CXBUTTONMARGIN			2
#define CYBUTTONMARGIN			3

//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CBaseTabManager, CTabSelector)

	ON_NOTIFY_REFLECT_EX(TCN_SELCHANGE, OnTabSelChange)
	ON_NOTIFY_REFLECT_EX(TCN_SELCHANGING, OnTabSelChanging)
	ON_WM_LBUTTONDOWN				()

	ON_MESSAGE(UM_CTRL_FOCUSED,		OnCtrlFocused)
	ON_MESSAGE(UM_VALUE_CHANGED,	OnValueChanged)
	ON_MESSAGE(UM_RECALC_CTRL_SIZE,	OnRecalcCtrlSize)
	ON_MESSAGE(UM_ACTIVATE_TAB_PAGE,OnActivateTabPage)

	ON_WM_SETFOCUS					()
	ON_WM_WINDOWPOSCHANGED			()
	ON_WM_MOUSEMOVE()
	ON_WM_SIZE				()

END_MESSAGE_MAP()
    
//-----------------------------------------------------------------------------
CBaseTabManager::CBaseTabManager()
	:
	CTabSelector				(),
	IDisposingSourceImpl		(this),
	IOSLObjectManager			(OSLType_Tabber),
	m_pParent					(NULL),
	m_pActiveDlg				(NULL),
	m_pBkgColorBrush			(NULL),
	m_crBkgColor				(AfxGetThemeManager()->GetBackgroundColor()),
	m_bKeepTabDlgAlive			(AfxIsRemoteInterface()),
	m_bDefaultSequence			(TRUE),
	m_pDocument					(NULL)
{
	SetBackColor(m_crBkgColor);
	m_pDlgInfoAr = new DlgInfoArray();

	if (AfxIsInUnattendedMode())
		return;

	CString asPaths[2];
	asPaths[0] = TBGlyph(szIconDisabled0);
	asPaths[1] = TBGlyph(szIconDisabled1);

	for (size_t i = 0; i < 2; i++)
	{
		HICON hIcon = TBLoadImage(asPaths[i]);
		if (hIcon == NULL)
			continue;

		if (i == 0)
		{
			CSize iconSize = GetHiconSize(hIcon);
			m_imgList.Create(iconSize.cx, iconSize.cy, ILC_COLOR32, 16, 16);
			m_imgList.SetBkColor(AfxGetThemeManager()->GetTransBmpTransparentDefaultColor());
		}

		m_imgList.Add(hIcon);
		::DestroyIcon(hIcon);
	}

	if (AfxGetApplicationContext()->IsActiveAccessibilityEnabled())
		EnableActiveAccessibility();
}

//-----------------------------------------------------------------------------
CBaseTabManager::CBaseTabManager(DWORD dwStyle, DWORD dwStyleEx)
	:
	CTabSelector				(),
	IDisposingSourceImpl		(this),
	IOSLObjectManager			(OSLType_Tabber),
	m_pParent					(NULL),
	m_pActiveDlg				(NULL),
	m_pBkgColorBrush			(NULL),
	m_crBkgColor				(AfxGetThemeManager()->GetBackgroundColor()),
	m_bKeepTabDlgAlive			(AfxIsRemoteInterface())
{
	SetBackColor(m_crBkgColor);
	m_pDlgInfoAr = new DlgInfoArray();

	if (AfxIsInUnattendedMode())
		return;

	CString asPaths[2];
	asPaths[0] = TBGlyph(szIconDisabled0);
	asPaths[1] = TBGlyph(szIconDisabled1);
	for (size_t i = 0; i < 2; i++)
	{
		HICON hIcon = TBLoadImage(asPaths[i]);
		if (i == 0)
		{
			CSize iconSize = GetHiconSize(hIcon);
			m_imgList.Create(iconSize.cx, iconSize.cy, ILC_COLOR32, 16, 16);
			m_imgList.SetBkColor(AfxGetThemeManager()->GetTransBmpTransparentDefaultColor());
		}

		m_imgList.Add(hIcon);
		::DestroyIcon(hIcon);
	}

	if (AfxGetApplicationContext()->IsActiveAccessibilityEnabled())
		EnableActiveAccessibility();
}

//-----------------------------------------------------------------------------
CBaseTabManager::~CBaseTabManager()
{
	delete m_pDlgInfoAr;
	m_pDlgInfoAr = NULL;

	if (m_pBkgColorBrush)
	{
		m_pBkgColorBrush->DeleteObject();
		delete m_pBkgColorBrush;
	}
	
	m_imgList.DeleteImageList();

	if (m_pProxy != NULL)
	{
		//force disconnect accessibility clients
		::CoDisconnectObject((IAccessible*)m_pProxy, NULL);
		m_pProxy = NULL;
	}
}

//-----------------------------------------------------------------------------
void CBaseTabManager::OnMouseMove(UINT nFlags, CPoint point) 
{
	TCHITTESTINFO hitTest;
	hitTest.pt = point;

	int iItem = HitTest(&hitTest);
	if (iItem != -1)
	{
		int n = GetDlgItemPos(iItem);
		ASSERT(n >=0);
		BOOL bIsEnabled = m_pDlgInfoAr->GetAt(n)->m_bEnabled;
		if (!bIsEnabled) return;
	}

	__super::OnMouseMove(nFlags, point);
}

//-----------------------------------------------------------------------------
int CBaseTabManager::GetDlgItemPos(int nVisiblePos)
{
	int pos = -1;
	if (nVisiblePos >= 0 && nVisiblePos < m_pDlgInfoAr->GetSize())
	{
		for (int i = 0; i < m_pDlgInfoAr->GetCount(); i++)
			if (m_pDlgInfoAr->GetAt(i)->m_bVisible)
			{
				nVisiblePos--; 
				pos = i;
				if (nVisiblePos < 0)
					break;
			}
	}
	return pos;
}

//-----------------------------------------------------------------------------
int CBaseTabManager::GetFirstTab(int nStartPos)
{
	if (nStartPos >= 0 && nStartPos < m_pDlgInfoAr->GetSize())
	{
		for (int i = nStartPos; i < m_pDlgInfoAr->GetCount(); i++)
		{
			DlgInfoItem* pItem = m_pDlgInfoAr->GetAt(i);
			if (pItem->m_bVisible && pItem->m_bEnabled)
				return i;
		}
	}
	return nStartPos == 0 ? -1 : GetFirstTab(0);
}

//-----------------------------------------------------------------------------
int CBaseTabManager::GetDlgItemNextPos(int nPos, BOOL OnlyEnabled /*=FALSE*/)
{
	int i= 0;
	for (i=nPos+1; i < m_pDlgInfoAr->GetSize(); i++)
	{
		if (OnlyEnabled)
		{
			if (m_pDlgInfoAr->GetAt(i)->m_bEnabled)
				break;
		}
		else
			if (m_pDlgInfoAr->GetAt(i)->m_bVisible && m_pDlgInfoAr->GetAt(i)->m_bEnabled)
				break;
	}
	return i < m_pDlgInfoAr->GetSize() ? i : -1;
}

//-----------------------------------------------------------------------------
int CBaseTabManager::GetDlgItemPrevPos(int nPos, BOOL OnlyEnabled /*=FALSE*/)
{
	int i = 0;
	for (i=nPos-1; i > -1; i--)
	{
		if (OnlyEnabled)
		{
			if (m_pDlgInfoAr->GetAt(i)->m_bEnabled)
				break;
		}
		else
			if (m_pDlgInfoAr->GetAt(i)->m_bVisible && m_pDlgInfoAr->GetAt(i)->m_bEnabled)
				break;
	}
	return i > -1 ? i : -1;
}

//-----------------------------------------------------------------------------
int CBaseTabManager::GetTabIndexFromItemPos(int nPos)
{
	int newPos = nPos;
	for (int i = 0; i <= nPos; i++)
	{
		if (!m_pDlgInfoAr->GetAt(i)->m_bVisible)
			newPos--;
	}
	return newPos;
}

//-----------------------------------------------------------------------------
int CBaseTabManager::GetRequiredHeight(CRect &rectAvail)
{ 
	CBaseTabDialog* pDialog = GetActiveDlg();
	if (pDialog && pDialog->GetChildTileGroup())
		return pDialog->GetChildTileGroup()->GetRequiredHeight(rectAvail) + (GetNormalTabber() ? GetNormalTabber()->GetTabsHeight() : 0);
	
	return rectAvail.Height(); 
}

//------------------------------------------------------------------------------
int CBaseTabManager::GetMinHeight(CRect& rect /*= CRect(0, 0, 0, 0)*/)
{ 
	switch (m_nMinHeight)
	{
		case ORIGINAL	:
		case FREE		:
		case AUTO		: 
		{
			CBaseTabDialog* pDialog = GetActiveDlg();
			
			
			if (pDialog)
			{
				int nHeight;
				if (pDialog->GetChildTileGroup())
				{
					//il rect che arriva è quello del tabber nella sua interezza, ma la dimensione che deve arrivare alla min height è a meno della larghezza della bottoniera
					if (m_ShowMode == ShowMode::VERTICAL_TILE)  // oppure, pDialog->GetClientRect(rect);
						rect.DeflateRect(m_nSelectorWidth, 0);

					nHeight = pDialog->GetChildTileGroup()->GetMinHeight(rect);
				}
				else if (pDialog && pDialog->GetChildTabManager())
					nHeight = pDialog->GetChildTabManager()->GetMinHeight(rect);
				else
				{
					CRect rectDlg;
					pDialog->GetWindowRect(rectDlg);
					nHeight = rectDlg.Height();
				}
				return nHeight + (GetNormalTabber() ? GetNormalTabber()->GetTabsHeight() : 0);
			}

			// Codizione accettata perchè potrebbe essere indotta dalla protezione sotto security di tutte le TabDialog o dell'intero TabManager
			return m_nMinHeight;
		}
		default: return m_nMinHeight;
	}
}

//-----------------------------------------------------------------------------
int CBaseTabManager::GetRequiredWidth(CRect &rectAvail)
{ 
	CBaseTabDialog* pDialog = GetActiveDlg();
	if (pDialog && pDialog->GetChildTileGroup())
		return pDialog->GetChildTileGroup()->GetRequiredWidth(rectAvail);
	
	if (pDialog && pDialog->GetChildTabManager())
		return pDialog->GetChildTabManager()->GetRequiredWidth(rectAvail);

	return rectAvail.Width(); 
}

//-----------------------------------------------------------------------------
void CBaseTabManager::GetAvailableRect(CRect &rectAvail)
{
	GetClientRect(rectAvail);
}
	
//-----------------------------------------------------------------------------
void CBaseTabManager::Relayout(CRect &rectNew, HDWP hDWP/*= NULL*/)
{
	CDockableFrame* pFrame = dynamic_cast<CDockableFrame*>(this->GetParentFrame());
	if (pFrame && pFrame->IsLayoutSuspended())
		return;

	// Avoid calling MoveWindow if just internal repositioning due to collapsing
	CRect rectWnd;
	GetWindowRect(rectWnd);
	GetParent()->ScreenToClient(rectWnd);
	
	CBaseTabDialog* pDialog = GetActiveDlg();

	if (rectNew != rectWnd)
		MoveWindow(rectNew);
	else
	{
		if (pDialog && pDialog->GetChildTileGroup())
		{
			CRect rectClient;
			GetClientRect(rectClient);

			// arrange the rectangle to accomodate the selector
			if (m_ShowMode == NORMAL)
			{
				AdjustCurrentDlgRect(&rectClient);
				rectClient.MoveToXY(0, 0);
			}
			else if (m_ShowMode == VERTICAL_TILE)
				rectClient.right -= GetSelectorWidth();

			pDialog->GetChildTileGroup()->Relayout(rectClient);
		}

		if (pDialog && pDialog->GetChildTabManager())
		{
			CRect rectClient;
			pDialog->GetClientRect(rectClient);
			pDialog->GetChildTabManager()->Relayout(rectClient);

			CBaseTabDialog* pSubTabDialog = pDialog->GetChildTabManager()->GetActiveDlg();
			if (pSubTabDialog)
			{
				CRect subRectClient;
				pSubTabDialog->GetClientRect(subRectClient);

				if (pSubTabDialog->GetLayoutContainer())
					pSubTabDialog->GetLayoutContainer()->Relayout(subRectClient);
			}
		}
	}
}

//-----------------------------------------------------------------------------
void CBaseTabManager::GetUsedRect(CRect &rectUsed)
{
	GetWindowRect(rectUsed);
	if (GetShowMode() == NORMAL)
	{
		CRect aTabsRect;
		GetTabsWindowRect(aTabsRect);
		rectUsed.top = aTabsRect.top;
	}

	//// mi faccio dare l'area del tabber per comprendere le linguette
	//CRect aRectTabber;
	//GetWindowRect(aRectTabber);
	//CBaseTabDialog* pDialog = GetActiveDlg();
	//if (pDialog)
	//{   
	//	pDialog->GetUsedRect(rectUsed);
	//}
	//rectUsed.UnionRect(aRectTabber, rectUsed);
}

//------------------------------------------------------------------------------
BOOL CBaseTabManager::CanDoLastFlex(FlexDim  fd)
{
	CBaseTabDialog* pDialog = GetActiveDlg();
	if (pDialog && pDialog->GetChildTileGroup())
		return pDialog->GetChildTileGroup()->CanDoLastFlex(fd);

	return FALSE;
}

//-----------------------------------------------------------------------------
void CBaseTabManager::OnWindowPosChanged(WINDOWPOS FAR* wndPos)
{
	__super::OnWindowPosChanged(wndPos);

	if (m_pActiveDlg)
		RepositionCurrentDlg();
}

//------------------------------------------------------------------------------
LRESULT CBaseTabManager::OnRecalcCtrlSize(WPARAM, LPARAM)
{
	DoRecalcCtrlSize();
	return 0L;
}

//--------------------------------------------------------------------------
void CBaseTabManager::OnSize(UINT nType, int cx, int cy)
{
	__super::OnSize(nType, cx, cy);

	CBaseTabDialog* pDialog = GetActiveDlg();
	if (pDialog)
		pDialog->DoResize();
}


//------------------------------------------------------------------------------
LRESULT CBaseTabManager::OnActivateTabPage(WPARAM wParam, LPARAM)
{
	//wParam nIDD della TabDialog
	int nPos = GetTabDialogPos(wParam);	
	if (nPos < 0 || nPos > m_pDlgInfoAr->GetUpperBound()) 
		return 0L;

	//se la tab non e' abilitata (quelle marcate con la X rossa, oppure con il 
	//lucchetto che indica protezione via security) esco
	//senza tentare di attivarla
	DlgInfoItem* pDlgInfo = GetDlgInfoArray()->GetAt(nPos);
	if ( pDlgInfo && !pDlgInfo->m_bEnabled)
		return 0L;

	TabDialogActivate(wParam);
	return 1L;
}
//devono matchare i valori in resource.h del TbMacroRecorder
#define ID_MACRORECORDER_VALIDATION 2302
#define ID_MACRORECORDER_TABDLG_VALIDATION 2306
// Standard behaviour to manage message from owned controls
//------------------------------------------------------------------------------
BOOL CBaseTabManager::OnCommand(WPARAM wParam, LPARAM lParam)
{
	DECLARE_WM_COMMAND_PARAMS(wParam, lParam, nID, nCode, hWndCtrl);

	//sono messaggi che devono arrivare al taber, non li devo mandare al parent!
	if (nID >= ID_MT_DROPDOWN_BUTTON && nID <= ID_MT_MENU_END)
		return __super::OnCommand(wParam, lParam);

	if (__super::OnCommand(wParam, lParam))
		return TRUE;

	CWnd* pParent = GetParent();
	return pParent ? pParent->SendMessage(WM_COMMAND, wParam, lParam) : FALSE;

/*
	//messaggio personalizzato per validazione di controlli e tab readonly
	if (
		(nID == ID_MACRORECORDER_VALIDATION || nID == ID_MACRORECORDER_TABDLG_VALIDATION)
		&&  pParent && pParent->SendMessage(WM_COMMAND, wParam, lParam) != 0
		)
		return TRUE;
*/
}

//--------------------------------------------------------------------------
BOOL CBaseTabManager::PreTranslateMessage(MSG* pMsg)
{
#ifndef _OLD_PTM

	return __super::PreTranslateMessage(pMsg);

#else

	ASSERT(pMsg != NULL);
	ASSERT_VALID(this);
	
	if (pMsg->message != WM_SYSKEYDOWN)
		return FALSE;

	int nActiveTab = -1;
	DlgInfoItem* pDlg = NULL;

	switch ((UINT) pMsg->wParam)
	{
		case VK_PRIOR:
		{
			nActiveTab = GetActiveTab();
			do
			{
			 	if (nActiveTab == 0)
			 		return FALSE;
			
		 		nActiveTab--;

				// se si passa per una dialog disabilitata si skippa
				pDlg = m_pDlgInfoAr ? m_pDlgInfoAr->GetAt(nActiveTab) : NULL;
			}
			while (!pDlg->m_bEnabled || !pDlg->m_bVisible);

	 		break;
		}
		case VK_NEXT:
		{
			nActiveTab = GetActiveTab();
			do
			{
			 	if (nActiveTab == m_pDlgInfoAr->GetUpperBound())
			 		return FALSE;
		
		 		nActiveTab++;

				// se si passa per una dialog disabilitata si skippa
				
				pDlg = m_pDlgInfoAr->GetAt(nActiveTab);
			}
			while (!pDlg->m_bEnabled || !pDlg->m_bVisible);

	 		break;
		}
		case VK_F11:
		{
			CWnd* pForm = GetParent();
			if (!pForm)
				return FALSE;

			CWnd* pWnd = pForm->GetNextDlgTabItem(this, FALSE);

			while (pWnd && pWnd->m_hWnd != m_hWnd)
			{
				if (pWnd->IsWindowEnabled()	&& !pWnd->IsKindOf(RUNTIME_CLASS(CStatic)))
				{
					pWnd->SetFocus();
					return TRUE;
				}

				pWnd = pForm->GetNextDlgTabItem(pWnd, FALSE);
			}
				
			return FALSE;
		}
	 	default:
	 	{
			CString strPattern("&");
			strPattern += (TCHAR) pMsg->wParam;
					
			for (int i = 0; i <= m_pDlgInfoAr->GetUpperBound(); i++)
			{
				DlgInfoItem* pDlgInfoItem = m_pDlgInfoAr->GetAt(i);
				CString strCaption = pDlgInfoItem->m_strTitle;
				strCaption.MakeUpper();
				if (strCaption.Find(strPattern) != -1)
				{
					nActiveTab = i;
					if (nActiveTab == GetActiveTab())
						return TRUE;
			
					// se si richiama una dialog disabilitata si da errore
					if (!pDlgInfoItem->m_bEnabled || !pDlgInfoItem->m_bVisible)
					{
						MessageBeep(MB_ICONHAND);
						return TRUE;
					}
					break;
				}
			}
		}
	}
	if (nActiveTab >= 0)
	{
		return TabDialogActivate(GetTabDialogIDD(nActiveTab)) > 0;
	}

	return FALSE;

#endif
}

//-----------------------------------------------------------------------------
LRESULT	CBaseTabManager::OnCtrlFocused	(WPARAM wParam, LPARAM lParam)
{
	return GetParent() ? GetParent()->SendMessage(UM_CTRL_FOCUSED, wParam, lParam) : 0L;
}

//-----------------------------------------------------------------------------
LRESULT CBaseTabManager::OnValueChanged	(WPARAM wParam, LPARAM lParam)
{
	return GetParent() ? GetParent()->SendMessage(UM_VALUE_CHANGED, wParam, lParam) : 0L;
}

//-----------------------------------------------------------------------------
void CBaseTabManager::OnCustomize()
{}

//-----------------------------------------------------------------------------
int CBaseTabManager::GetTabDialogPos(const CTBNamespace& aNs)
{
	if (!m_pDlgInfoAr)
		return -1;
	
	DlgInfoItem* pInfo = NULL;
	for (int i = 0; i <= m_pDlgInfoAr->GetUpperBound(); i++)
	{
		pInfo = m_pDlgInfoAr->GetAt(i);
		
		if (pInfo && aNs == pInfo->GetNamespace())
			return i;
	}
	
	return -1;
}

//-----------------------------------------------------------------------------
int CBaseTabManager::GetTabDialogPos(UINT nIDD)
{
	DlgInfoItem* pInfo = NULL;
	for (int i = 0; i <= m_pDlgInfoAr->GetUpperBound(); i++)
	{
		pInfo = m_pDlgInfoAr->GetAt(i);
		if (pInfo && nIDD == pInfo->m_nDialogID)
			return i;
	}

	return -1;
}
//-----------------------------------------------------------------------------
UINT CBaseTabManager::GetTabDialogIDD(int nPos)
{
	if (nPos < 0 || nPos > m_pDlgInfoAr->GetUpperBound())
	{
		ASSERT(FALSE);	
		return 0;
	}
	return m_pDlgInfoAr->GetAt(nPos)->m_nDialogID;
}

//-----------------------------------------------------------------------------
//distrugge e ricrea la tab attiva per rinfrescare i controlli
//in pratica simula un change di tab in cui però la tab attivata è 
//quella corrente
int CBaseTabManager::TabDialogRefresh()
{
	//simulo il clic sulla tab corrente
	m_nPosClickedTab = GetCurSel();
	//verifico se ci sono errori da segnalare (per iniire perdite di fuoco inopportune)
	if (DoTabSelChanging() == TABCTRL_ABORT_SELCHANGING) 
		return -1;
	//distruggo e ricreo la tab
	DoTabSelChange();
	return 1;
}

//-----------------------------------------------------------------------------
int CBaseTabManager::TabDialogActivate(UINT nIDD)
{
	int nPos = GetTabDialogPos(nIDD);	
	if (nPos < 0) 
		return 0;

	// non fa nulla, e torna OK, se gia` visualizzata
	if (m_pActiveDlg && nIDD == m_pActiveDlg->m_pDlgInfo->GetDialogID())
		return 1;

	DlgInfoItem* pDlgInfo = GetDlgInfoArray()->GetAt(nPos);

	//if (!pDlgInfo->m_bEnabled)
	//	return -1;

	if (! OSL_CAN_DO(pDlgInfo->GetInfoOSL(), OSL_GRANT_EXECUTE))
		return -1;

	m_nPosClickedTab = nPos;

	if (DoTabSelChanging() == TABCTRL_ABORT_SELCHANGING) 
		return -1;

	SetCurSel(GetTabIndexFromItemPos(nPos));

	if (GetCurSel() >= 0)
	{
		DoTabSelChange();
		return 1;
	}
	return -1;
}

//-----------------------------------------------------------------------------
int CBaseTabManager::TabDialogShow(UINT nIDD, BOOL bShow)
{
	ASSERT(bShow == TRUE || bShow == FALSE);

	int nPos = GetTabDialogPos(nIDD);
	if (nPos < 0) return 0;

	if (bShow)
		ShowTab(nPos, TRUE);
	else
	{
		if (m_pActiveDlg && m_pActiveDlg->m_nID == nIDD)
		{
			if (!m_pActiveDlg->CheckForm(TRUE)) 
				return -1;

			if (m_bDefaultSequence)
			{
				int nOtherPos = GetDlgItemPrevPos(nPos);
				if (nOtherPos < 0)
					nOtherPos = GetDlgItemNextPos(nPos);
				if (nOtherPos >= 0)
					TabDialogActivate(m_pDlgInfoAr->GetAt(nOtherPos)->GetDialogID());
			}
		}

		ShowTab(nPos, FALSE);
	}
	return 1;
}

//-----------------------------------------------------------------------------
int CBaseTabManager::TabDialogEnable(UINT nIDD, BOOL bEnable)
{
	ASSERT(bEnable == TRUE || bEnable == FALSE);

	int nPos = GetTabDialogPos(nIDD);
	if (nPos < 0) return 0;

	DlgInfoItem* pDlg = m_pDlgInfoAr->GetAt(nPos);
	pDlg->m_bEnabled = bEnable;
	if (!bEnable && m_pActiveDlg && pDlg == m_pActiveDlg->GetDlgInfoItem())
	{
		int nOtherPos = GetDlgItemPrevPos(nPos);
		if (nOtherPos < 0)
			nOtherPos = GetDlgItemNextPos(nPos);
		if (nOtherPos >= 0)
			TabDialogActivate(m_pDlgInfoAr->GetAt(nOtherPos)->GetDialogID());
	}

	int nTabIndex = GetTabIndexFromItemPos(nPos);
	if (GetImageList())
	{
		TCITEM tcitm;
		tcitm.mask = TCIF_IMAGE | TCIF_TEXT;

		tcitm.iImage = bEnable ? pDlg->m_nIconIndex : 0; 
		
		CString sTitle = pDlg->m_strTitle;
		if (tcitm.iImage > -1)
			sTitle = TAB_IMG_OFFSET + sTitle; //faccio spazio per l'icona
		tcitm.pszText = sTitle.GetBuffer(); 

		BOOL bOk = __super::SetItem(nTabIndex, &tcitm);

		sTitle.ReleaseBuffer();
	}

	return 1;
}
//-----------------------------------------------------------------------------
void CBaseTabManager::ChangeTabTitle(UINT nIDDTab,  const CString& sTitle, int nIconIndex /*= -1*/)
{
	int nPos = GetTabDialogPos(nIDDTab);
	if (nPos < 0)
		return;

	DlgInfoItem* pDlg = m_pDlgInfoAr->GetAt(nPos);

	int nTabIndex = GetTabIndexFromItemPos(nPos);

	pDlg->m_strTitle = sTitle;

	TCITEM tcitm;
	tcitm.mask = TCIF_TEXT;
	if (pDlg->m_nIconIndex != nIconIndex && GetImageList())
		tcitm.mask |= TCIF_IMAGE;

	CString strTitle = pDlg->m_strTitle;
	if (nIconIndex > -1)
		strTitle = TAB_IMG_OFFSET + strTitle; //faccio spazio per l'icona
	tcitm.pszText = strTitle.GetBuffer(); 
	tcitm.iImage = nIconIndex; 

	BOOL bOk = __super::SetItem(nTabIndex, &tcitm);
	strTitle.ReleaseBuffer();
	pDlg->m_nIconIndex = nIconIndex;
}

//-----------------------------------------------------------------------------
DlgInfoItem* CBaseTabManager::CreateDlgInfoItem(CRuntimeClass*	pDialogClass, UINT nDialogID, int nOrdPos/*= -1*/)
{
	return new DlgInfoItem (pDialogClass, nDialogID, nOrdPos);
}

//-----------------------------------------------------------------------------
int CBaseTabManager::AddDialog(CRuntimeClass* pDialogClass, UINT nIDTitle, int nOrdPos/*= -1*/, const CString nsSelectorImage /*_T("")*/, const CString sSelectorTooltip /*_T("")*/)
{
	if (! (nOrdPos >= -1 && nOrdPos <= m_pDlgInfoAr->GetSize()))
	{
		TRACE("TabDialog %s is wrong positioned (position %d of %d): appended on the end", pDialogClass->m_lpszClassName, nOrdPos,  m_pDlgInfoAr->GetSize());
	}
	if (nOrdPos < -1 || nOrdPos > m_pDlgInfoAr->GetSize())
		nOrdPos = -1;

	DlgInfoItem* pDlgInfo = CreateDlgInfoItem(pDialogClass, nIDTitle, nOrdPos);

	int nPos;
	if (nOrdPos > -1)
	{
		m_pDlgInfoAr->InsertAt(nOrdPos, pDlgInfo);
		nPos = nOrdPos;
		DlgInfoItem* pNextDlgInfo = NULL;
		// devo aggiornare la posizione delle tabdialog successive a quella inserita
		for (int i = nOrdPos + 1; i < m_pDlgInfoAr->GetSize(); i++)
		{
			pNextDlgInfo = m_pDlgInfoAr->GetAt(i);
			if (pNextDlgInfo)
				pNextDlgInfo->m_nOrdPos = i;			
		}
	}
	else
		nPos = m_pDlgInfoAr->Add(pDlgInfo); 

	pDlgInfo->SetSelectorImage(nsSelectorImage);
	pDlgInfo->SetSelectorTooltip(sSelectorTooltip);
	return nPos;
}

//-----------------------------------------------------------------------------
int CBaseTabManager::AddDialog(CRuntimeClass* pDialogClass, UINT nDialogID, const CTBNamespace& aNs, const CString& sTitle, int nOrdPos /*-1*/, const CString nsSelectorImage /*_T("")*/, const CString sSelectorTooltip)
{
	if (! (nOrdPos >= -1 && nOrdPos <= m_pDlgInfoAr->GetSize()))
	{
		TRACE("TabDialog %s is wrong positioned (position %d of %d): appended on the end", pDialogClass->m_lpszClassName, nOrdPos,  m_pDlgInfoAr->GetSize());
	}
	if (nOrdPos < -1 || nOrdPos > m_pDlgInfoAr->GetSize())
		nOrdPos = -1;

	DlgInfoItem* pDlgInfo; 
	// double attach of the same tab dialog returns the same object
	for (int i=0; i <= m_pDlgInfoAr->GetUpperBound(); i++)
	{
		pDlgInfo = m_pDlgInfoAr->GetAt(i);
		if (!pDlgInfo)
			continue;

		if (pDlgInfo->GetNamespace() == aNs)
			return i;
	}

	pDlgInfo = CreateDlgInfoItem(pDialogClass, nDialogID, nOrdPos);
	if (pDlgInfo)
	{
		pDlgInfo->m_strTitle = sTitle;
		pDlgInfo->GetInfoOSL()->m_Namespace = aNs;
		pDlgInfo->SetSelectorImage(nsSelectorImage);
		pDlgInfo->SetSelectorTooltip(sSelectorTooltip);
	}

	int nPos;
	if( nOrdPos > -1 )
	{
		m_pDlgInfoAr->InsertAt(nOrdPos, pDlgInfo);
		nPos = nOrdPos;
		DlgInfoItem* pNextDlgInfo = NULL;
		// devo aggiornare la posizione delle tabdialog successive a quella inserita
		for (int i = nOrdPos + 1; i < m_pDlgInfoAr->GetSize(); i++)
		{
			pNextDlgInfo = m_pDlgInfoAr->GetAt(i);
			if (pNextDlgInfo)
				pNextDlgInfo->m_nOrdPos = i;			
		}
	}
	else
		nPos = m_pDlgInfoAr->Add(pDlgInfo); 

	return nPos;
}

//-----------------------------------------------------------------------------
void CBaseTabManager::SetTabDialogImage(UINT nDialogID, const CString nsSelectorImage)
{
	int pos = GetTabDialogPos(nDialogID);
	this->ChangeImage(pos, nsSelectorImage);
}

//-----------------------------------------------------------------------------
//ex LRESULT CBaseTabManager::OnTabActivate(WPARAM wParam, LPARAM lParam)
BOOL CBaseTabManager::OnTabSelChanging(NMHDR* pNMHDR, LRESULT* pResult)
{
	ASSERT(m_pActiveDlg);
	if (!m_pActiveDlg || pNMHDR->hwndFrom != m_hWnd) 
		*pResult = TABCTRL_ABORT_SELCHANGING;	//changing aborted
	else
		*pResult = DoTabSelChanging();
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CBaseTabManager::DoTabSelChanging()
{ 
	if (!m_pActiveDlg)
		return TABCTRL_CONFIRM_SELCHANGING;

	if (m_nPosClickedTab < 0)
		return TABCTRL_ABORT_SELCHANGING;

	int nActiveTab = GetDlgItemPos(GetCurSel());
	if (nActiveTab < 0)
		return TABCTRL_ABORT_SELCHANGING;

	BOOL bEmitError = m_nPosClickedTab != nActiveTab;

	UINT nNexdTabIDD = GetTabDialogIDD(m_nPosClickedTab);

	if (!m_pActiveDlg->CheckForm(bEmitError)) 
		return TABCTRL_ABORT_SELCHANGING;

	CWnd* pWnd = m_pActiveDlg->GetLastFocusedCtrl();
	
	if (pWnd && ::IsWindow(pWnd->m_hWnd) && m_pActiveDlg->IsChild(pWnd))
	{

		CWnd* pGridWnd = dynamic_cast<CWnd*>(CGridControlObj::FromChild(pWnd, m_pActiveDlg));
		if (pGridWnd)
		{
			// Si simula un losing focus per il control del Grid che ha correntemente il fuoco.
			// In risposta al messaggio di losing focus si puo` ritornare
			// MAKELRESULT(ID, CTRL_FATAL_ERROR) oppure MAKELRESULT(ID, CTRL_WARNING_ERROR)
			// Vedere anche CBodyEdit e ParsedCtrl
			//
			LRESULT res = pGridWnd->SendMessage(UM_LOSING_FOCUS, 0, RELATIVE_FOCUSED);

			if (res)
			{
				// E` stato rigettata la perdita di fuoco
				pGridWnd->PostMessage
					(
						UM_BAD_VALUE, LOWORD(res),
						HIWORD(res) | CTRL_IMMEDIATE_NOTIFY | CTRL_FOCUS_LOSE_REJECTED
					);

				return TABCTRL_ABORT_SELCHANGING;
			}

			if (nActiveTab != -1 && pGridWnd->IsKindOf(RUNTIME_CLASS(CGridControl)))
				((CGridControl*) pGridWnd)->GetCurrCellPos(*(m_pDlgInfoAr->GetAt(nActiveTab)->m_pCellPos));

			pWnd = pGridWnd;
		}

		if (!DispatchOnEnableTabSelChanging(m_pActiveDlg->m_nID, nNexdTabIDD)) 
			return TABCTRL_ABORT_SELCHANGING;
	}
	else if (!DispatchOnEnableTabSelChanging(m_pActiveDlg->m_nID, nNexdTabIDD)) 
		return TABCTRL_ABORT_SELCHANGING;

	//ATTENZIONE: chiamata spostata all'inizio della OnTabSelChange (ex OnTabShown)
	// CreateTabDialog(nNewTab);
	return TABCTRL_CONFIRM_SELCHANGING;
}

//-----------------------------------------------------------------------------
//ex LRESULT CBaseTabManager::OnTabShown(WPARAM wParam, LPARAM lParam)
BOOL CBaseTabManager::OnTabSelChange(NMHDR* pNMHDR, LRESULT* pResult) 
{
	DoTabSelChange();
	return TRUE;
}

//-----------------------------------------------------------------------------
void CBaseTabManager::DoTabSelChange() 
{
	//spostata qui la distruzione della tab, in quanto se la si distrugge
	//nella OnChanging come in passato, potrebbe esserci un messaggio 
	//fra l'OnChanging e l'OnChanged che utilizza un oggetto distrutto e quindi RANDOM CRASH!!!
	// destroy/hide the previous dialog after creating/showing the new one, to minimize flickering
	CBaseTabDialog* pPreviousDlg = m_pActiveDlg;
	m_pActiveDlg = NULL;

	CDockableFrame* pFrame = dynamic_cast<CDockableFrame*>(this->GetParentFrame());
	if (pFrame)
		pFrame->SuspendLayout();

	int nPos = GetDlgItemPos(GetCurSel());
	if (nPos >= 0)
		CreateTabDialog(nPos); //dalla changing

	//In certi casi la create, per motivi di security o di abilitazione, ritorna la prima tab disponibile (viene chiesto di attivare la tab i, non è attivabile quindi viene restituita la i-1)
	//in questo caso previous e active sono le stesse, non deve fare niente (e di certo non distruggere la active, altrimenti non funziona più niente)
	if (pPreviousDlg == m_pActiveDlg)
		return;

	if (m_pActiveDlg)
	{
		m_pActiveDlg->ShowWindow(SW_HIDE);
		
		// refresh controls in the tab and 
		// updates their "enable" state.
		if (m_bKeepTabDlgAlive)
			m_pActiveDlg->EnableTabControls();
	}

	//XTECH optimization
	if (pPreviousDlg)
	{
		if (m_bKeepTabDlgAlive)//@@BAUZI
			pPreviousDlg->ShowWindow(SW_HIDE);
		else
		{
			// Viene disabilitata la dialog poiche in certi casi morendo da` il fuoco
			// al suo primo control provocando degli InvalidHandle
			pPreviousDlg->EnableWindow(FALSE);
			pPreviousDlg->DestroyWindow(); 
		}
	}
	
	if (m_pActiveDlg)
		DispatchOnAfterTabSelChanged(m_pActiveDlg->m_nID);

	SendMessageToDescendants(UM_INITIALIZE_TILE_LAYOUT, (WPARAM)NULL, (LPARAM)this);

	if (pFrame)
		pFrame->ResumeLayout();

	if (GetParentElement())
		RequestRelayout();
	// nel caso di tabber vecchi che hanno tilegroups
	// ma che non hanno il layout abilitato (vd wizard
	// di xtech deve invocare il layout sull'elemento
	else if (!GetParentElement() && m_pActiveDlg->GetLayoutContainer())
		m_pActiveDlg->GetLayoutContainer()->RequestRelayout();

	m_pActiveDlg->ShowWindow(SW_SHOW);

	SetCtrlFocus(GetCurSel());
}

//-----------------------------------------------------------------------------
void CBaseTabManager::OnLButtonDown (UINT nFlags, CPoint point)
{
	TCHITTESTINFO HitTestInfo;
	HitTestInfo.pt = point;
	HitTestInfo.flags = TCHT_ONITEM;

	m_nPosClickedTab = -1;
	int nPos = HitTest(&HitTestInfo);

	if (nPos < 0 || nPos > m_pDlgInfoAr->GetUpperBound() || m_pDlgInfoAr->GetUpperBound() < 0)
		 return;

	m_nPosClickedTab = nPos;
	DlgInfoItem* pInfoItem = NULL;
	for (int i=0; i <= (int)nPos; i++)
	{
		pInfoItem = m_pDlgInfoAr->GetAt(i);
		if (pInfoItem && !pInfoItem->m_bVisible) 
		{
			m_nPosClickedTab++;
		}
	}

	if (
			m_nPosClickedTab > m_pDlgInfoAr->GetUpperBound()
			||
			(
				m_pDlgInfoAr->GetAt(m_nPosClickedTab) &&
				!(m_pDlgInfoAr->GetAt(m_nPosClickedTab)->m_bEnabled)
			)
		)
	{
		m_nPosClickedTab = -1;
		return;
	}

	__super::OnLButtonDown(nFlags, point);
}

//-----------------------------------------------------------------------------
void CBaseTabManager::ShowTab(int nPos, BOOL bVisible)
{
	DlgInfoItem* pDlg = m_pDlgInfoAr->GetAt(nPos);
	ASSERT(pDlg);
	if (pDlg==NULL) return;

	int nSkip = 0;
	for (int i = 0; i < nPos; i++)
		if (!m_pDlgInfoAr->GetAt(i)->m_bVisible)
			nSkip++;

	if (pDlg->m_bVisible && !bVisible)
	{
		DeleteItem(nPos - nSkip);
	}

	pDlg->m_bVisible = bVisible;

	if (bVisible)
	{
		InsertDlgInfoItem(nPos - nSkip, pDlg);
	}
}

// Serve per gestire il fuoco dato ai "cavalieri" e passarlo alla dialog correntemente
// visualizzata.
//-----------------------------------------------------------------------------
void CBaseTabManager::OnSetFocus(CWnd* pOldCWnd)
{
	__super::OnSetFocus(pOldCWnd);

	if (!m_pActiveDlg || !::IsWindow(m_pActiveDlg->m_hWnd))
		return;
	
	CWnd* pDlgCtrl = m_pActiveDlg->GetLastFocusedCtrl();
	
	if (pDlgCtrl && ::IsWindow(pDlgCtrl->m_hWnd) && m_pActiveDlg->IsChild(pDlgCtrl))
		pDlgCtrl->SetFocus();
	else		
		m_pActiveDlg->SetDefaultFocus();
}

//-----------------------------------------------------------------------------
BOOL CBaseTabManager::SetCtrlFocus(int nActiveTab)
{
	if (nActiveTab < 0)
		return FALSE;

	if (!m_pActiveDlg || !::IsWindow(m_pActiveDlg->m_hWnd))
	{
		ASSERT(FALSE);
		return FALSE;
	}

	UINT nIDC = m_pDlgInfoAr->GetAt(nActiveTab)->m_nLastFocusIDC;
	if (nIDC)
	{
		CWnd* pWnd = CParsedForm::GetChildCtrlWndFromID(m_pActiveDlg, nIDC);
		if (pWnd)
		{
			if (pWnd->IsWindowEnabled())
			{
				// Se e` un GridControl viene riposizionata la cella
				if (pWnd->IsKindOf(RUNTIME_CLASS(CGridControl)))
					((CGridControl*)pWnd)->SetCurrCellPos(*(m_pDlgInfoAr->GetAt(nActiveTab)->m_pCellPos));

				pWnd->SetFocus();
				return TRUE;
			}
		}
	}

	if ((m_pActiveDlg->m_hWnd && ::IsWindow(m_pActiveDlg->m_hWnd)) || m_pActiveDlg->m_pCtrlSite != NULL)
		return m_pActiveDlg->SetDefaultFocus();

	return FALSE;
}

//-----------------------------------------------------------------------------
DlgInfoItem* CBaseTabManager::CreateDialogObj(UINT nPos)
{
	DlgInfoItem* pDlgInfo = m_pDlgInfoAr->GetAt(nPos);

	if (pDlgInfo->m_pBaseTabDlg)
		m_pActiveDlg = pDlgInfo->m_pBaseTabDlg;
	else
	{
		m_pActiveDlg = (CBaseTabDialog*)pDlgInfo->m_pDialogClass->CreateObject();
		ASSERT_KINDOF(CBaseTabDialog, m_pActiveDlg);

		m_pActiveDlg->SetResourceModule(pDlgInfo->GetResourceModule());
		m_pActiveDlg->GetInfoOSL()->m_Namespace = pDlgInfo->GetInfoOSL()->m_Namespace;

		CString strName = pDlgInfo->GetNamespace().GetObjectName();
		if (!strName.IsEmpty())
			m_pActiveDlg->m_sName = strName;

		m_pActiveDlg->m_nCurrTabPos = nPos;
		m_pActiveDlg->m_pDlgInfo = pDlgInfo;

		OnAttachParents(m_pActiveDlg);
	}

	return pDlgInfo;
}

//-----------------------------------------------------------------------------
BOOL CBaseTabManager::CreateTabDialog(int nPos)
{
	if (nPos < 0)
		return FALSE;

	//controllo di security
	if (m_pDlgInfoAr->GetAt(nPos)->m_bEnabled == FALSE)
	{
		int i = 0;
		for (i = GetCurSel() - 1; i >= 0; i--)
		{
			if (m_pDlgInfoAr->GetAt(i)->m_bVisible && m_pDlgInfoAr->GetAt(i)->m_bEnabled) 
				break;
		}
		if (i < 0)
		{
			for (i = GetCurSel() + 1; i < m_pDlgInfoAr->GetSize(); i++)
			{
				if (m_pDlgInfoAr->GetAt(i)->m_bVisible && m_pDlgInfoAr->GetAt(i)->m_bEnabled) 
					break;
			}
		}

		if (i >= 0 && i < m_pDlgInfoAr->GetSize())
			nPos = i;
		else
			return FALSE;
	}

	DlgInfoItem* pDlgInfo =  CreateDialogObj(nPos);

	if (!pDlgInfo->m_pBaseTabDlg)
	{
		CBaseDocument* pDoc = m_pActiveDlg->GetDocument();
		if (!m_pActiveDlg->Create(this))
		{
			m_pActiveDlg = NULL;
			pDlgInfo->m_pBaseTabDlg = NULL;
			if (pDoc)
				pDoc->m_pMessages->Add(_TB("Error during TabDialog creation"));

			return FALSE;
		}
	}

	return TRUE;
}

//-----------------------------------------------------------------------------
void CBaseTabManager::AdjustCurrentDlgRect(CRect* pRect)
{
	// arrange the rectangle to accomodate the selector tab, plus some margins
	pRect->top += (GetNormalTabber() ? GetNormalTabber()->GetTabsHeight() : 0);
}

//-----------------------------------------------------------------------------
void CBaseTabManager::RepositionCurrentDlg()
{
    ASSERT(m_pActiveDlg);
	if (!m_pActiveDlg || GetShowMode() == NONE)
		return;

	CRect rc;
	GetClientRect(&rc);

	if (GetShowMode() == NORMAL)
	{
		AdjustCurrentDlgRect(&rc);
		m_pActiveDlg->MoveWindow(rc.left, rc.top, rc.Width(), rc.Height(), TRUE);
	}
	else
	{
		m_pActiveDlg->MoveWindow(rc.left + m_nSelectorWidth, rc.top, rc.Width() - m_nSelectorWidth, rc.Height(), TRUE);
	}
}

//-----------------------------------------------------------------------------
int CBaseTabManager::InsertDlgInfoItem(int i, DlgInfoItem* pdlginfo)
{
	return __super::InsertDlgInfoItem(i, pdlginfo);
}

//-----------------------------------------------------------------------------
void CBaseTabManager::InitTabNamespaces()
{
	//finché non viene creato l'oggetto tab, non si conosce
	//il suo nome, pertanto non è possibile calcolarne il namespace.
	//Questo metodo istanzia tutte le tab (senza creare l'oggetto grafico)
	//per conoscerne il nome ed inizializzare tutti i namespace del tabber
	for (int i = 0; i < m_pDlgInfoAr->GetCount(); i++)
	{
		DlgInfoItem* pItem = m_pDlgInfoAr->GetAt(i);
		if (pItem->GetNamespace().IsEmpty())
		{
			CBaseTabDialog* pDialog = (CBaseTabDialog*) pItem->GetDialogClass()->CreateObject();
			ASSERT_TRACE( 
				pDialog->m_sName.IsEmpty() || !m_pDlgInfoAr->Exists(pDialog->m_sName),
				cwsprintf(_T("TabDialog with duplicate namespace: %s, %s, %d\n"),
				pDialog->m_sName, pItem->GetDialogClass()->m_lpszClassName, pItem->m_nDialogID)
				);
			pItem->GetNamespace().SetChildNamespace(CTBNamespace::TABDLG, pDialog->m_sName, GetNamespace());
			delete pDialog;
		}
		ASSERT(pItem->GetInfoOSL()->m_pParent == NULL || pItem->GetInfoOSL()->m_pParent == GetInfoOSL());
		if (!pItem->GetInfoOSL()->m_pParent)
			pItem->GetInfoOSL()->m_pParent = GetInfoOSL(); //x EB
	}
}


//-----------------------------------------------------------------------------
void CBaseTabManager::OnInitialUpdate(UINT nIDC, CWnd* pParentWnd, BOOL bCallCustomize /*TRUE*/)
{		
	m_pParent = pParentWnd;

	CWnd* pWnd = pParentWnd->GetDlgItem(nIDC);
	CRect rectWnd;
	if (pWnd)
	{
		pWnd->GetWindowRect(&rectWnd);
		pParentWnd->ScreenToClient(rectWnd);

		pWnd->DestroyWindow();
	}
	DWORD dwTabManagerStyle = WS_TABSTOP | WS_CHILD | WS_VISIBLE;
	CreateEx(TCS_MULTILINE|TCS_BUTTONS|TCS_FLATBUTTONS, dwTabManagerStyle, rectWnd, pParentWnd, nIDC);

	SetFont(pParentWnd->GetFont());
	SetImageList(&m_imgList);

	if (AfxGetThemeManager()->UseFlatStyle())
		AfxGetThemeManager()->MakeFlat(this);

	InitSizeInfo(this);

	//@@@TODO LAYOUT RESIZABLE CTRL
	CParsedForm* pForm = GetParsedForm(pParentWnd);
	if (pForm && pForm->GetLayoutContainer())
	{
		SetAutoSizeCtrl(0);
		SetResizableCurSize(0,0);
	}

	if (bCallCustomize)
	{
		Customize();
		CustomizeExternal();
	}

	COLORREF  clrTabBkgnd = m_crBkgColor = AfxGetThemeManager()->GetBackgroundColor();
	SetBackColor(clrTabBkgnd);
    
	int nDlgs  = m_pDlgInfoAr->GetSize();

	DlgInfoItem* pDlg;
	for (int i = 0; i < nDlgs ; i++)
	{
		pDlg = m_pDlgInfoAr->GetAt(i);
		if (pDlg->m_strTitle.IsEmpty() && bCallCustomize)
			pDlg->m_strTitle = AfxLoadTBString(pDlg->GetDialogID(), pDlg->GetResourceModule());

		if (pDlg->m_bVisible)
			InsertDlgInfoItem(i, pDlg);
	}

	// da una chance al programmatore di personalizzare il Tabber
	if (bCallCustomize)
		OnCustomize();
	
	if (m_pDlgInfoAr->GetSize())
	{
		for (int i = 0; i <= m_pDlgInfoAr->GetUpperBound(); i++)
		{
			DlgInfoItem* pDlgInfo = CreateDialogObj(i);

			//controllo di security
			if (OSL_CAN_DO(m_pActiveDlg->GetInfoOSL(), OSL_GRANT_EXECUTE) == 0)
			{
				m_pActiveDlg->m_pDlgInfo->m_bEnabled = FALSE;

				if (m_pActiveDlg->m_pDlgInfo->m_strTitle.IsEmpty())
					m_pActiveDlg->m_pDlgInfo->m_strTitle = cwsprintf(m_pActiveDlg->m_nID);

				ChangeTabTitle(m_pActiveDlg->m_pDlgInfo->GetDialogID(), m_pActiveDlg->m_pDlgInfo->m_strTitle, 1);
			}

			delete m_pActiveDlg;
			m_pActiveDlg = NULL;
			pDlgInfo->m_pBaseTabDlg = NULL;
		}

		if (CreateTabDialog(GetFirstTab(0))) // crea la prima TabDialog
			SetCurSel(m_pActiveDlg->m_nCurrTabPos);
		else
			SetCurSel(-1);
	}

	AdjustTabManager();
}

//-----------------------------------------------------------------------------
void CBaseTabManager::AdjustTabManager()
{
	if (GetShowMode() == VERTICAL_TILE)
	{
		CalculateSelectorWidth();
	}
	OnUpdateTabStates();

	if (m_pActiveDlg)
		m_pActiveDlg->ShowWindow(SW_SHOW);
	
	CRect rect;
	GetWindowRect(rect);
	SetWindowPos(NULL, 0, 0, rect.Width() + 1, rect.Height(), SWP_NOMOVE | SWP_NOZORDER | SWP_FRAMECHANGED);  //TODOLUCA il +1 forse non serve più
}

//-----------------------------------------------------------------------------
COLORREF CBaseTabManager::GetBackColor() { return m_crBkgColor; }
void CBaseTabManager::SetBackColor(COLORREF cr) 
{ 
	if (m_crBkgColor == cr)
	{
		//ASSERT(m_pBkgColorBrush);
		//return;
	}
	m_crBkgColor = cr; 
	if (m_pBkgColorBrush)
	{
		m_pBkgColorBrush->DeleteObject();
		delete m_pBkgColorBrush;
	}
	m_pBkgColorBrush = new CBrush(cr);
}

//-----------------------------------------------------------------------------
UINT CBaseTabManager::GetActiveTabID()
{
	CBaseTabDialog* pDlg = GetActiveDlg();
	if (pDlg && pDlg->m_pDlgInfo)
	{
		return pDlg->m_pDlgInfo->GetDialogID();
	}
	return -1;
}

//-----------------------------------------------------------------------------
UINT CBaseTabManager::GetActiveTab() { return GetCurSel(); }

//-----------------------------------------------------------------------------
void CBaseTabManager::SetActiveTab(int nActiveTab) { SetCurSel(nActiveTab); }

//-----------------------------------------------------------------------------
int CBaseTabManager::GetTabCount() { return GetItemCount(); }

//-----------------------------------------------------------------------------
CString CBaseTabManager::GetTabIconSource(UINT iDialogID)
{
	int index = GetTabDialogPos(iDialogID);
	if (index < m_arSelectors.GetCount() && index >= 0)
	{
		CSelectorButton* pSelectorButton = m_arSelectors.GetAt(index);
		return pSelectorButton->GetIconSource();
	}

	return _T("");
}

//for XTech optimization
//-----------------------------------------------------------------------------
void  CBaseTabManager::SetKeepTabDlgAlive(BOOL bSet)
{
	m_bKeepTabDlgAlive =  bSet; 
}

//--------------------------------------------------------------------------
CString  CBaseTabManager::GetRanorexNamespace()
{
	return cwsprintf(_T("{0-%s}{1-%s}"), GetNamespace().GetObjectName(), GetNamespace().GetTypeString());
}

/////////////////////////////////////////////////////////////////////////////
// CBaseTabManager diagnostics

#ifdef _DEBUG
void CBaseTabManager::AssertValid() const
{
	__super::AssertValid();
}

void CBaseTabManager::Dump(CDumpContext& dc) const
{
	__super::Dump(dc);
	AFX_DUMP0(dc, "\nCTabManager");
}
#endif //_DEBUG
//=============================================================================
