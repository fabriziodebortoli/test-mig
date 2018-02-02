
#include "stdafx.h"

#include <TbGeneric\globals.h>
#include <TbGeneric\FontsTable.h>
#include <TbGeneric\VisualStylesXP.h>

#include "baseapp.h"
#include "parscbx.h"
#include "parsedt.h"
#include "hlinkobj.h"
#include "tbcommandinterface.h"
#include <TbGeneric\TBThemeManager.h>

#include "extres.hjson" //JSON AUTOMATIC UPDATE
#include "parsres.hjson" //JSON AUTOMATIC UPDATE

#include "HyperLink.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif


//=============================================================================
//			Class CHyperLink implementation
//=============================================================================
IMPLEMENT_DYNAMIC (CHyperLink, CBCGPURLLinkButton)
BEGIN_MESSAGE_MAP(CHyperLink, CBCGPURLLinkButton)
	//{{AFX_MSG_MAP(CHyperLink)
	ON_WM_LBUTTONUP		()
	ON_WM_RBUTTONUP		()
	ON_WM_MOUSEMOVE		()
	ON_COMMAND			(ID_FOLLOW_HYPERLINK,			OnFollowHyperlink)
	ON_MESSAGE			(UM_GET_CONTROL_DESCRIPTION,	OnGetControlDescription)
	ON_WM_SHOWWINDOW	()

	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CHyperLink::CHyperLink(CParsedCtrl* pCtrl) 
	:
	m_pOwner			(pCtrl),
	m_bVisible			(TRUE),	// "visibile" in questo caso vuol dire che il suo control di riferimento
								// è visibile, quindi se serve lui si deve mostrare
	m_bAlwaysHidden		(FALSE),
	m_pLinkedDocument	(NULL),
	m_hOldCursor		(NULL)
{
	ASSERT(pCtrl);
	
	m_bMouseInside = FALSE;

	m_pFont	= AfxGetThemeManager()->GetHyperlinkFont();
}

//-----------------------------------------------------------------------------
CHyperLink::~CHyperLink() 
{
	if (m_pFont && m_pFont != AfxGetThemeManager()->GetHyperlinkFont())
	{
		BOOL ok;
		//if (m_pFont->Detach())
			ok = m_pFont->DeleteObject();

		delete m_pFont;
	}
}

//-----------------------------------------------------------------------------
LRESULT CHyperLink::OnGetControlDescription(WPARAM wParam, LPARAM lParam)
{
	//does nothing
	return (LRESULT)CWndObjDescription::GetDummyDescription();
}

//-----------------------------------------------------------------------------
BOOL CHyperLink::Create(CWnd* pParentWnd)
{
    CRect rectE(0, 0, 0, 0);
	UINT nIDC = AfxGetTBResourcesMap()->GetTbResourceID(_T("HyperLink"), TbResourceType::TbControls);
	return __super::Create(NULL, BS_OWNERDRAW , rectE, pParentWnd, nIDC);
}

//-----------------------------------------------------------------------------
void CHyperLink::Init()
{
	HINSTANCE hInst = AfxFindResourceHandle(MAKEINTRESOURCE(IDC_TB_HAND), RT_GROUP_CURSOR);
	m_hHandCursor = ::LoadCursor(hInst,MAKEINTRESOURCE(IDC_TB_HAND));
	OverlapToControl();
}

//-----------------------------------------------------------------------------
void CHyperLink::OverlapToControl()
{
	CRect ownerRect;
	// determina dove sta l'owner control rispetto al parent (che è la dialog),
	// per posizionarcisi sopra
	m_pOwner->GetCtrlCWnd()->GetWindowRect(&ownerRect);
	m_pOwner->GetCtrlCWnd()->GetParent()->ScreenToClient(&ownerRect);

	OverlapToControl(ownerRect, SWP_NOREDRAW | SWP_NOACTIVATE);
}

//-----------------------------------------------------------------------------
void CHyperLink::OverlapToControl(CRect& rectEdit, UINT nFlags)
{
	if (m_bAlwaysHidden)
	{
		EnableWindow(FALSE);
		ShowWindow(SW_HIDE);
		m_bVisible = FALSE;
		return;
	}

	int nSave = 0;
	int nBorder = ScalePix(3);
	int nBottomRight = ScalePix(5);
	if (m_pOwner->GetCtrlCWnd()->IsKindOf(RUNTIME_CLASS(CParsedStatic)))
	{
		nBorder = ScalePix(1);
		nBottomRight = ScalePix(2);
	}
	if (m_pOwner->GetCtrlCWnd()->IsKindOf(RUNTIME_CLASS(CParsedCombo)))
		nSave = ScalePix(16);

	SetWindowPos
		(
			m_pOwner->GetCtrlCWnd(), // se imposta lo z-order, sta sempre sopra il suo control
			rectEdit.left + nBorder, 
			rectEdit.top + nBorder, 
			rectEdit.right - rectEdit.left - nBottomRight - nSave - 1, 
			rectEdit.bottom - rectEdit.top - nBottomRight,
			nFlags
		);
}

//--------------------------------------------------------------------------
void CHyperLink::OnShowWindow(BOOL bShow, UINT nStatus)
{
	if (bShow)
	{
		OverlapToControl();
	}
}

//-----------------------------------------------------------------------------
void CHyperLink::SetHLinkFont(CFont* pFont)
{
	CFont* pDefault = AfxGetThemeManager()->GetHyperlinkFont();

	// se prima c'e' un font differente dal default, lo cancella
	if (m_pFont && m_pFont != pDefault)
	{
		m_pFont->DeleteObject();
		delete m_pFont;
		m_pFont = NULL;
	}

	if (pFont && pFont != pDefault)
	{
		LOGFONT logFont;
		pFont->GetLogFont(&logFont);
		logFont.lfUnderline = TRUE;

		m_pFont = new CFont();
		m_pFont->CreateFontIndirect(&logFont);
	}
	else
	{
		m_pFont = AfxGetThemeManager()->GetHyperlinkFont();
	}

	if (this->m_hWnd)
		SetFont(m_pFont);
}

//-----------------------------------------------------------------------------
void CHyperLink::DrawItem(LPDRAWITEMSTRUCT lpDIS)
{
	CString strText;
	m_pOwner->GetCtrlCWnd()->GetWindowText(strText);

	::FillRect(lpDIS->hDC, &lpDIS->rcItem, (HBRUSH)AfxGetThemeManager()->GetBackgroundColorBrush()->GetSafeHandle());

	COLORREF crOldTextColor = ::SetTextColor(lpDIS->hDC, AfxGetThemeManager()->GetHyperLinkForeColor());
	COLORREF crOldBkColor = ::SetBkColor(lpDIS->hDC, AfxGetThemeManager()->GetBackgroundColor());
	RECT aRect = lpDIS->rcItem;
	aRect.left += 3;
	
	HGDIOBJ oldFont = NULL;

	if (m_pFont)
		oldFont = ::SelectObject(lpDIS->hDC, m_pFont->m_hObject);

	::DrawText (lpDIS->hDC, strText, strText.GetLength(), &aRect, DT_SINGLELINE|DT_NOPREFIX);
   
	if (oldFont)
		::SelectObject(lpDIS->hDC, oldFont);

	::SetTextColor(lpDIS->hDC, crOldTextColor);
	::SetBkColor(lpDIS->hDC, crOldBkColor);
}
                              
//-----------------------------------------------------------------------------
void CHyperLink::OnLButtonUp (UINT, CPoint ptMousePos)
{	
	m_bMouseInside = FALSE;
	// attiva l'apertura del documento autoinviandosi un messaggio, in modo da fare finire le attività
	// legate alla visualizzazione (cambio cursore, ecc.) per non rischiare di inceppare la perdita del fuoco 
	PostMessage(WM_COMMAND, ID_FOLLOW_HYPERLINK);
	ReleaseCapture();
}

//-----------------------------------------------------------------------------
void CHyperLink::OnRButtonUp (UINT nFlag, CPoint ptMousePos)
{
	CMenu  menu;	
	menu.CreatePopupMenu();

	if (m_pOwner->GetDocument() && 
		m_pOwner->GetDocument()->ShowingPopupMenu(m_pOwner->GetCtrlID(), &menu) &&
		menu.GetMenuItemCount() > 0
		)	
	{
		CRect ItemRect;
		m_pOwner->m_pOwnerWnd->GetWindowRect(ItemRect);
		CPoint ptPoint = ItemRect.TopLeft();
		ptPoint += ptMousePos;			
		menu.TrackPopupMenu (TPM_LEFTBUTTON, ptPoint.x, ptPoint.y, m_pOwner->m_pOwnerWnd);
	}
}

//-----------------------------------------------------------------------------
void CHyperLink::DoFollowHyperlink(DataObj* pData /* = NULL */, BOOL bActivate /* = TRUE*/)
{
	if	(
			!m_pOwner->m_pHotKeyLink ||
			!m_pOwner->m_pHotKeyLink->IsHotLinkEnabled() ||
			!m_pOwner->m_pHotKeyLink->CanDoSearchOnLink()
		)
		return;
	
	//Gira il messaggio al documento
	NotifyMessage(ID_FOLLOW_HYPERLINK);

	BOOL bpDataDoDelete = FALSE;
	if (!pData)
	{
		ASSERT(m_pOwner->m_pData);
		bpDataDoDelete = TRUE;
		pData = m_pOwner->m_pData->DataObjClone();
		m_pOwner->GetValue(*pData);
	}
	
	// se il document ha cambiato namespace non posso usare quello già istanziato, ma devo chiudere e riaprire il linked document
	if	(
			m_pLinkedDocument && 
			(
				m_pLinkedDocument->GetNamespace() != m_pOwner->m_pHotKeyLink->GetAddOnFlyNamespace() ||
				m_pOwner->m_pHotKeyLink->NeedToDestroyLinkedDocument(m_pLinkedDocument)
			)
		)
		AfxGetTbCmdManager()->DestroyDocument(m_pLinkedDocument);

	m_pLinkedDocument = (CBaseDocument*)m_pOwner->m_pHotKeyLink->BrowserLink(pData, m_pLinkedDocument, NULL, bActivate);

	if (bpDataDoDelete)
		delete pData;
}

//-----------------------------------------------------------------------------
void CHyperLink::NotifyMessage(UINT message)
{
	CParsedCtrl* pCtrl = NULL;
	if (m_pOwner && m_pOwner->GetCtrlCWnd())
		pCtrl = GetParsedCtrl(m_pOwner->GetCtrlCWnd());

	//Gira il messaggio ID_FOLLOW_HYPERLINK al documento
	if (pCtrl && pCtrl->GetCtrlCWnd() && pCtrl->GetCtrlCWnd()->GetParent())
	{
		//Nel caso del bodyedit il parent del pCtrl è il bodyedit stesso, per ruotare il messaggio
		//abbiamo bisogno del parent del bodyedit
		if (
			pCtrl->GetNamespace().GetType() == CTBNamespace::GRIDCOLUMN && 
			pCtrl->GetCtrlCWnd()->GetParent()->GetParent()
			)
		{
			pCtrl->GetCtrlCWnd()->GetParent()->GetParent()->SendMessage
													(
														WM_COMMAND,
														(WPARAM)MAKELONG(pCtrl->GetCtrlID(), message),
														(LPARAM)pCtrl->GetCtrlCWnd()->GetParent()->GetParent()->m_hWnd
													);
			return;
		}
		else
		{
			pCtrl->GetCtrlCWnd()->GetParent()->SendMessage
													(
														WM_COMMAND,
														(WPARAM)MAKELONG(pCtrl->GetCtrlID(), message),
														(LPARAM)pCtrl->GetCtrlCWnd()->GetParent()->m_hWnd
													);
			return;
		}
	}
}

//-----------------------------------------------------------------------------
void CHyperLink::RefreshHyperlink(DataObj* pData /* = NULL */)
{
	// se il documento collegato è aperto ne rinfresca il contenuto
	if	(m_pLinkedDocument)
		DoFollowHyperlink(pData,FALSE); // non porta in primo piano
}

//-----------------------------------------------------------------------------
void CHyperLink::OnFollowHyperlink()
{
	DoFollowHyperlink();
}

//-----------------------------------------------------------------------------
void CHyperLink::DoEnable(BOOL bEnable)
{
	if (!m_bVisible || m_bAlwaysHidden)
		return;

	if (m_pOwner->m_pHotKeyLink && !m_pOwner->m_pHotKeyLink->IsHyperLinkEnabled ())
	{
		ShowWindow(SW_HIDE);
		Invalidate();
		return;
	}

	if (m_pOwner->GetCtrlCWnd()->IsKindOf(RUNTIME_CLASS(CParsedStatic)))
	{
		if (m_pOwner->GetCtrlCWnd()->GetWindowTextLength() == 0)
			ShowWindow(SW_HIDE);
		else
			ShowWindow(SW_SHOW);

		Invalidate();
		return;
	}

	EnableWindow(!bEnable);

	if (bEnable)
	{
		ShowWindow(SW_HIDE);
	}
	else
	{
		if (m_pOwner->GetCtrlCWnd()->GetWindowTextLength() == 0)
		{
			if (m_pOwner->GetCtrlCWnd()->IsKindOf(RUNTIME_CLASS(CDescriptionCombo)))
			{
				CDescriptionCombo* pDesCbx = (CDescriptionCombo*) m_pOwner->GetCtrlCWnd();
				ASSERT_VALID(pDesCbx->GetCtrlData());

				CString str;
				if (pDesCbx->GetDescription(pDesCbx->GetCtrlData(), str) && str.GetLength() > 0)
				{
					ASSERT(!str.IsEmpty());
					pDesCbx->m_sReadOnlyText = str; 
					pDesCbx->SetWindowText(str);

					ShowWindow(SW_SHOW);
				}
				else
				{
					//ASSERT(pDesCbx->GetCtrlData()->IsEmpty());

					ShowWindow(SW_HIDE);
				}
			}
			else
				ShowWindow(SW_HIDE);
		}
		else
		{
			if (
					(m_pOwner->m_dwCtrlStyle & CTRL_STYLE_SHOW_FIRST)
					|| 
					(m_pOwner->m_dwCtrlStyle & CTRL_STYLE_SHOW_LAST)
				)
				ShowWindow(SW_HIDE);
			else
				ShowWindow(SW_SHOW);
		}
	}
#ifdef _DEBUG
	if (IsWindowVisible())
	{
		//ASSERT(m_pOwner->GetCtrlCWnd()->GetWindowTextLength());
		CString s;
		m_pOwner->GetCtrlCWnd()->GetWindowText(s);
		//ASSERT(s.GetLength());
	}
#endif
	Invalidate();
}

//-----------------------------------------------------------------------------
void CHyperLink::UpdateCtrlView()
{
	if (!IsWindowEnabled() || !m_bVisible || m_bAlwaysHidden)
		return;

	CWnd* pWnd = m_pOwner->GetCtrlCWnd();
	if	(
			pWnd->GetWindowTextLength() == 0 || 
			(
				!IsTBWindowVisible(pWnd) && 
				pWnd->GetParent() && 
				IsTBWindowVisible(pWnd->GetParent())
			)
		)
	{
		ShowWindow(SW_HIDE);
		return;
	}

	if (m_pOwner->m_pHotKeyLink && !m_pOwner->m_pHotKeyLink->IsHyperLinkEnabled ())
	{
		ShowWindow(SW_HIDE);
		return;
	}

	if (m_pOwner->m_dwCtrlStyle & (CTRL_STYLE_SHOW_FIRST | CTRL_STYLE_SHOW_LAST))
	{
		ShowWindow(SW_HIDE);
		return;
	}

	Invalidate();
	ShowWindow(SW_SHOW);
	
	RefreshHyperlink();
}

//-----------------------------------------------------------------------------
void CHyperLink::ShowCtrl(int nCmdShow)
{
	// se non deve mai essere visto (es.: bodyedit), non fa nulla
	if (m_bAlwaysHidden)
		return;

	// l'attributo di visibile / invisibile viene comunque aggiornato
	m_bVisible = nCmdShow != SW_HIDE;

	if (!m_bVisible)
		ShowWindow(SW_HIDE);
	else 
		UpdateCtrlView();
}

//-----------------------------------------------------------------------------
void CHyperLink::OnMouseMove( UINT nFlags, CPoint point )
{
	CRect rect;
	GetClientRect(rect);

	BOOL bInside = rect.PtInRect(point);
	if (m_bMouseInside == bInside)
		return;

	m_bMouseInside = bInside;

	if (m_bMouseInside)
	{
		m_hOldCursor = ::SetCursor(m_hHandCursor);
		SetCapture();
	}
	else
		ReleaseCapture();
}

//----------------------------------------------------------------------------------------------
BOOL CHyperLink::ReleaseCapture()
{
	if (::GetCapture() != GetSafeHwnd())
		return FALSE;

	if (m_hOldCursor)
		::SetCursor(m_hOldCursor);

	m_hOldCursor = NULL;
	return ::ReleaseCapture();
}

