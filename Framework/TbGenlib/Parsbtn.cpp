
#include "stdafx.h"

#include <float.h>
#include <ctype.h>
#include <atlimage.h>

#include <TbFrameworkImages\GeneralFunctions.h>
#include <TbGeneric\globals.h>
#include <TbGeneric\EnumsTable.h>
#include <TbParser\SymTable.h>
#include <TbGeneric\FontsTable.h>
#include <TbGeneric\VisualStylesXp.h>
#include <TbGeneric\GeneralFunctions.h>
#include <TbWoormEngine\INPUTMNG.H>
#include <TbGeneric\TBThemeManager.h>

#include "messages.h"
#include "BaseTileDialog.h"
#include "baseapp.h"
#include "hlinkobj.h"
#include "parsobj.h"
#include "basedoc.h"
#include "MicroareaVisualManager.h"

#include "parsbtn.h"
#include "BaseTileDialog.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif

//=============================================================================
//			Class CExtButton implementation
//=============================================================================
IMPLEMENT_DYNCREATE (CExtButton, CButton)

BEGIN_MESSAGE_MAP(CExtButton, CButton)
	//{{AFX_MSG_MAP(CExtButton)
	ON_WM_SETFOCUS()
	ON_WM_KEYDOWN()
	ON_WM_CHAR()
	ON_WM_GETDLGCODE()
	ON_WM_WINDOWPOSCHANGING	()
	ON_MESSAGE		(UM_GET_CONTROL_DESCRIPTION,			OnGetControlDescription)
	ON_WM_CTLCOLOR_REFLECT	()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CExtButton::CExtButton()
	:
	CButton				(),
	CColoredControl		(this),
	CCustomFont			(this),
	IDisposingSourceImpl(this),


 	m_pBitmapUp			(NULL),
	m_pBitmapDown		(NULL),
	m_pBitmapDisabled	(NULL),

	m_pExtInfo			(NULL),

	m_nPngIDStdImage	(0),
	m_nPngIDAltImage	(0),
	m_nPngCurrImage		(0),

	m_bUseImageSize		(FALSE),
	m_bDrawFrame		(TRUE)
{
	m_sPngNSStdImage	= _T("");
	m_sPngNSAltImage	= _T("");
	m_sPngCurrImage		= _T("");
}

//-----------------------------------------------------------------------------
CExtButton::~CExtButton()
{
	if (m_pBitmapUp)
 		delete m_pBitmapUp;

	if (m_pBitmapDown)
		delete m_pBitmapDown;

	if (m_pBitmapDisabled)
		delete m_pBitmapDisabled;
	
	if (m_pExtInfo)
		delete m_pExtInfo;
}

//=============================================================================
// HELPER FUNCTIONS
//=============================================================================
// This function calculate rects and style needed to custom paint a checkbox or a radiobutton
// The aim is to paint the control with custom colors when disabled, overriding standard behavior (gray or sunken)
// The painting must match size, alignement and position of the standard commctrl behavior
// The control is painted in two parts: one is the check image, the other is the text
//-----------------------------------------------------------------------------
void CExtButton::CalculateCheckboxAndRadioRect
(
	const	CRect&		aResourceRect,
	CString&			strText,
	CRect&				rectCheck,
	DWORD&				dwCheckStyle,
	CRect&				rectText,
	DWORD&				dwTextStyle,
	CFont*				pFont
)
{
	// While documented otherwise, GetButtonStyle do not return all button styles, i.e. the BS_MULTILINE flag, must use GetWindowLong
	// UINT btnStyle = GetButtonStyle();
	LONG lStyles = ::GetWindowLong(m_hWnd, GWL_STYLE);

	// check if radio or checkbox
	BOOL bIsRadio = ((GetButtonStyle() & BS_RADIOBUTTON) == BS_RADIOBUTTON) ||
		((GetButtonStyle() & BS_AUTORADIOBUTTON) == BS_AUTORADIOBUTTON);

	// radio image is a little bit smaller
	int nWidth = bIsRadio ? 13 : 14;

	if (IsScale()) 
		nWidth = ScalePix(nWidth);

	// calculates rect for the check: it is a square (nWidth size) vertically centered respect to the .rc rect
	// and left-adjusted
	BOOL bTopAlign = ((lStyles & BS_TOP) == BS_TOP);// ((GetButtonStyle() & BS_TOP) == BS_TOP);
	DWORD horizontalTextAlign = DT_CENTER;
	if ((lStyles & BS_LEFT) == BS_LEFT)
		horizontalTextAlign = DT_LEFT;
	if ((lStyles & BS_RIGHT) == BS_RIGHT)
		horizontalTextAlign = DT_RIGHT;

	rectCheck.top = bTopAlign
		?	2
		:	aResourceRect.bottom > nWidth ? (aResourceRect.bottom - nWidth) / 2 : 0; // vertically centered
	rectCheck.bottom = rectCheck.top + nWidth;

	// set style: radio or check
	if (AfxGetThemeManager()->IsXpThemed())
		dwCheckStyle = (bIsRadio ? BP_RADIOBUTTON : BP_CHECKBOX);
	else
		dwCheckStyle = (bIsRadio ? DFCS_BUTTONRADIO : DFCS_BUTTONCHECK) | DFCS_INACTIVE;

	// initially the text rect has the same height of the resource one, it starts right of the checkbox one
	rectText.top = aResourceRect.top;
	rectText.bottom = aResourceRect.bottom;

	int rSpace = (AfxGetThemeManager()->IsXpThemed() ? 17 : 19);
	rSpace = ScalePix(rSpace);

	if ((GetStyle() & BS_LEFTTEXT) == BS_LEFTTEXT)
	{
		rectCheck.right = aResourceRect.right;
		rectCheck.left = aResourceRect.right - nWidth;
		rectText.left = aResourceRect.left;
		rectText.right = aResourceRect.right - rSpace;
	}
	else
	{
		rectCheck.left = 0;
		rectCheck.right = nWidth;
		rectText.left = aResourceRect.left + rSpace; // different displacement under Win2000 - just for fun
		rectText.right = aResourceRect.right;
	}

	//oops! radio in Win2000 are a pixel right and up (why? ask Bill!)
	if (bIsRadio && !AfxGetThemeManager()->IsXpThemed())
	{
		rectCheck.top -= 1;
		rectCheck.bottom -= 1;
		rectCheck.left += 1;
		rectCheck.right += 1;
	}

	BOOL bMultiLine = (lStyles & BS_MULTILINE) == BS_MULTILINE;
	// if the text is multiline, set automatic word break. Otherwise set single line.
	// vertically align top or center according to orginal rc settings 
	dwTextStyle = (bMultiLine ? DT_WORDBREAK : DT_SINGLELINE) | (bTopAlign ? DT_TOP : DT_VCENTER) | horizontalTextAlign;

	if (horizontalTextAlign != DT_RIGHT)
	{
		// Simulate drawing to adjust rect for the text
		CDC* pDC = GetDC();
		if (pDC)
		{
			if (pFont)
				pDC->SelectObject(pFont);

			pDC->DrawText(strText, &rectText, dwTextStyle | DT_CALCRECT);
			ReleaseDC(pDC);
		}
	}

	// even if the rect is calculated by DrawText, it needs some adjustments to match standard commctrl behavior
	// center respect to original rect when multiline but no top-align
	if (!bTopAlign && aResourceRect.bottom > rectText.bottom)
	{
		rectText.top += ((aResourceRect.bottom - rectText.bottom) / 2);
		rectText.bottom += ((aResourceRect.bottom - rectText.bottom) / 2);
	}
	else if (bMultiLine &&  !bIsRadio && !bTopAlign)
	{
		// Alline the checkBox to text 
		rectText.top += rectCheck.top;
		rectText.bottom += rectCheck.top;

		if (rectCheck.top % 2 != 0)
		{
			rectText.top -= 1;
			rectText.bottom -= 1;
		}
	}

	if (!bMultiLine && (rectText.bottom - aResourceRect.bottom) > 1)
	{
		int h = (rectText.Height() - aResourceRect.Height()) / 2;
		rectText.top -= h;
		rectText.bottom -= h;
	}
}


//-----------------------------------------------------------------------------
void CExtButton::OnKeyDown(UINT nChar, UINT nPreCnt, UINT nFlags)
{
	if ((GetStyle() & BS_OWNERDRAW) == BS_OWNERDRAW)
	{
		if (nChar == VK_RETURN ||
			nChar == VK_SPACE ||
			nChar == VK_TAB)
		{
			// lo gestisco nella OnChar()
			return;
		}
	}
	return __super::OnKeyDown(nChar, nPreCnt, nFlags);
}

//-----------------------------------------------------------------------------
void CExtButton::OnChar(UINT nChar, UINT nRepCnt, UINT nFlags)
{
	switch (nChar)
	{
	case VK_RETURN:
	case VK_SPACE:
		// gestisco ENTER e SPACEBAR. Questo perché se sono OwnerDraw, chissà perché i messaggi di tastiera non vengono inviati alla dialog
		GetParent()->SendMessage(
			WM_COMMAND,
			MAKEWPARAM(GetDlgCtrlID(), BN_CLICKED),
			(LPARAM)GetSafeHwnd());
		break;

	case VK_TAB:
		GetParent()->SendMessage(WM_NEXTDLGCTL, 0, 0);
		break;

	default:
		// Handle the rest.
		__super::OnChar(nChar, nRepCnt, nFlags);
	}

	return;
}

//-----------------------------------------------------------------------------
UINT CExtButton::OnGetDlgCode()
{
	if ((GetStyle() & BS_OWNERDRAW) == BS_OWNERDRAW)
		// Intercept all keyboard inputs. We don't get the option to just intercept the ENTER key. Thus,
		// in OnChar/OnKeyDown we are forced to handle all keyboard inputs that are relevant to the button.
		return __super::OnGetDlgCode() | DLGC_WANTMESSAGE;
	else
		return __super::OnGetDlgCode();
}
//-----------------------------------------------------------------------------
void CExtButton::OnWindowPosChanging(WINDOWPOS FAR* wndPos)
{
}

//-----------------------------------------------------------------------------
void CExtButton::OnSetFocus(CWnd* pOldWnd)
{
	CButton::OnSetFocus(pOldWnd);

	//La classe indicata derivava direttamente da CButton
	if (this->IsKindOf(RUNTIME_CLASS(CParsedGroupBtn)))
		return;
		
	GetParent()->SEND_WM_COMMAND(GetDlgCtrlID(), BN_SETFOCUS, m_hWnd);
}

// Draw the appropriate bitmap
//-----------------------------------------------------------------------------
void CExtButton::DrawItem(LPDRAWITEMSTRUCT lpDIS)
{
	if ((GetStyle() & BS_OWNERDRAW) != BS_OWNERDRAW)
	{
		CButton::DrawItem(lpDIS);
		return;
	}

	if (m_nPngIDStdImage || m_nPngIDAltImage || !m_sPngNSStdImage.IsEmpty() || !m_sPngNSAltImage.IsEmpty())
	{
		DrawPNGItem (lpDIS);
		
	
		if (lpDIS->itemState & ODS_FOCUS)
		{
			CDC* pDC = CDC::FromHandle(lpDIS->hDC);
			CRect rect(lpDIS->rcItem);
			rect.InflateRect(-4, -4);
			pDC->DrawFocusRect(rect);
		}
	}


	// se non esiste la bitmap esco (per EasyStudio)
	if (!m_pBitmapUp || !m_pBitmapDisabled)
		return;

	ASSERT(lpDIS != NULL);
	// must have at least the first bitmap loaded before calling DrawItem
	ASSERT(m_pBitmapUp->m_hObject != NULL);     // required

	// use the main bitmap for up, the selected bitmap for down
	CWalkBitmap* pBitmap = m_pBitmapUp;

	UINT state = lpDIS->itemState;
	if ((state & ODS_SELECTED) && m_pBitmapDown->m_hObject)
		pBitmap = m_pBitmapDown;

	if ((state & ODS_DISABLED) && m_pBitmapDisabled->m_hObject)
		pBitmap = m_pBitmapDisabled;   // image for disabled
    
	BITMAP bmInfo;
	if (pBitmap->GetObject(sizeof(bmInfo), &bmInfo) != sizeof(bmInfo))
		return;

	// draw the whole button
	CDC* pDC = CDC::FromHandle(lpDIS->hDC);
	CDC memDC;
	memDC.CreateCompatibleDC(pDC);
	CBitmap* pOld = memDC.SelectObject(pBitmap);
	if (pOld == NULL)
	{
		memDC.DeleteDC();
		return;     // destructors will clean up
	}

	CRect rect(lpDIS->rcItem);
	
	// draw the icon contents
	pDC->BitBlt
		(	
			rect.left, rect.top, rect.Width(), rect.Height(),
			&memDC, 0, 0, SRCCOPY
		);

	memDC.SelectObject(pOld);

	if (lpDIS->itemState & ODS_FOCUS)
	{
		rect.InflateRect(-4, -4);
		pDC->DrawFocusRect (rect);
	}
	memDC.DeleteDC();
}
                              
// LoadBitmaps will load in one, two, three or all four bitmaps
// returns TRUE if all specified images are loaded
//-----------------------------------------------------------------------------
BOOL CExtButton::LoadBitmaps(UINT nBitmapResourceID)
{
	if ((GetStyle() & BS_OWNERDRAW) != BS_OWNERDRAW)
	{
		TRACE1("CExtButton::LoadBitmaps : bitmap %d has not OWNERDRAW style \n", nBitmapResourceID);
		return FALSE;   // need this one image
	}

	if (!m_pBitmapUp)
		m_pBitmapUp = new CWalkBitmap;

	if (!m_pBitmapDown)
		m_pBitmapDown = new CWalkBitmap;

	if (!m_pBitmapDisabled)
		m_pBitmapDisabled = new CWalkBitmap;
		
	// delete old bitmaps (if present)
	m_pBitmapUp->DeleteObject();
	m_pBitmapDown->DeleteObject();
	m_pBitmapDisabled->DeleteObject();

	if (!m_pBitmapUp->LoadBitmap(nBitmapResourceID))
	{
		TRACE1("CExtButton::LoadBitmaps : failed to load bitmap %d for NORMAL image\n", nBitmapResourceID);
		return FALSE;   // need this one image
	}
	
	if (!m_pBitmapDown->LoadBitmap(nBitmapResourceID + 1))
	{
		TRACE1("CExtButton::LoadBitmaps : failed to load bitmap %d for DOWN image\n", nBitmapResourceID);
		m_pBitmapDown->LoadBitmap(nBitmapResourceID);
	}

	if (!m_pBitmapDisabled->LoadBitmap(nBitmapResourceID + 2))
	{
		TRACE1("CExtButton::LoadBitmaps : failed to load bitmap %d for DISABLED image\n", nBitmapResourceID);
		m_pBitmapDisabled->LoadBitmap(nBitmapResourceID);
	}

	CSize bitmapSize;
	BITMAP bmInfo;
	VERIFY(m_pBitmapUp->GetObject(sizeof(bmInfo), &bmInfo) == sizeof(bmInfo));

	VERIFY(SetWindowPos(NULL, -1, -1, bmInfo.bmWidth, bmInfo.bmHeight,
		SWP_NOMOVE|SWP_NOZORDER|SWP_NOREDRAW|SWP_NOACTIVATE));
		
	return TRUE;
}

// LoadBitmaps will load in one, two, three or all four bitmaps
// returns TRUE if all specified images are loaded
//-----------------------------------------------------------------------------
BOOL CExtButton::LoadBitmaps(const CString& sBitmapResourceName, const CString& sBitmapResourceDownName, const CString& sBitmapResourceDisabledName)
{
	if ((GetStyle() & BS_OWNERDRAW) != BS_OWNERDRAW)
	{
		TRACE1("CExtButton::LoadBitmaps : bitmap %s has not OWNERDRAW style \n", sBitmapResourceName);
		return FALSE;   // need this one image
	}

	if (!m_pBitmapUp)
		m_pBitmapUp = new CWalkBitmap;

	if (!m_pBitmapDown)
		m_pBitmapDown = new CWalkBitmap;

	if (!m_pBitmapDisabled)
		m_pBitmapDisabled = new CWalkBitmap;
		
	// delete old bitmaps (if present)
	m_pBitmapUp->DeleteObject();
	m_pBitmapDown->DeleteObject();
	m_pBitmapDisabled->DeleteObject();

	if (!m_pBitmapUp->LoadBitmap(sBitmapResourceName))
	{
		TRACE1("CExtButton::LoadBitmaps : failed to load bitmap %s for NORMAL image\n", sBitmapResourceName);
		return FALSE;   // need this one image
	}
	
	if (!sBitmapResourceDownName.IsEmpty () && !m_pBitmapDown->LoadBitmap(sBitmapResourceDownName))
	{
		TRACE1("CExtButton::LoadBitmaps : failed to load bitmap %s for DOWN image\n", sBitmapResourceDownName);
		m_pBitmapDown->LoadBitmap(sBitmapResourceDownName);
	}

	if (!sBitmapResourceDisabledName.IsEmpty () && !m_pBitmapDisabled->LoadBitmap(sBitmapResourceDisabledName))
	{
		TRACE1("CExtButton::LoadBitmaps : failed to load bitmap %d for DISABLED image\n", sBitmapResourceDisabledName);
		m_pBitmapDisabled->LoadBitmap(sBitmapResourceDisabledName);
	}

	CSize bitmapSize;
	BITMAP bmInfo;
	VERIFY(m_pBitmapUp->GetObject(sizeof(bmInfo), &bmInfo) == sizeof(bmInfo));
	VERIFY(SetWindowPos(NULL, -1, -1, bmInfo.bmWidth, bmInfo.bmHeight,
		SWP_NOMOVE|SWP_NOZORDER|SWP_NOREDRAW|SWP_NOACTIVATE));
		
	return TRUE;
}


//-----------------------------------------------------------------------------
LRESULT CExtButton::OnGetControlDescription(WPARAM wParam, LPARAM lParam)
{
	CWndObjDescriptionContainer* pContainer = (CWndObjDescriptionContainer*) wParam;
	CString strId = (LPCTSTR)lParam;
	CWndColoredObjDescription *pDesc = NULL;

	UINT style = GetButtonStyle();
	UINT typeStyle = BS_TYPEMASK & style;
	if (typeStyle == BS_CHECKBOX || typeStyle == BS_AUTOCHECKBOX 
		|| typeStyle == BS_3STATE || typeStyle == BS_AUTO3STATE
		|| typeStyle == BS_RADIOBUTTON || typeStyle == BS_AUTORADIOBUTTON
		||typeStyle == BS_GROUPBOX )
		{
			if (typeStyle == BS_GROUPBOX)
			{
				pDesc = (CWndColoredObjDescription*)(pContainer->GetWindowDescription(this, RUNTIME_CLASS(CWndColoredObjDescription), strId));
				pDesc->m_Type = CWndObjDescription::Group;
			}
			else
			{
				pDesc = (CWndCheckRadioDescription*)(pContainer->GetWindowDescription(this, RUNTIME_CLASS(CWndCheckRadioDescription), strId));
				
				pDesc->m_Type = (typeStyle == BS_RADIOBUTTON || typeStyle == BS_AUTORADIOBUTTON) ? CWndObjDescription::Radio : CWndObjDescription::Check;
				// verbose version of the line above.
				/*if (typeStyle == BS_RADIOBUTTON || typeStyle == BS_AUTORADIOBUTTON)
				{
					pDesc->m_Type = CWndObjDescription::Radio;
				}
				else if (typeStyle == BS_CHECKBOX || typeStyle == BS_AUTOCHECKBOX
					|| typeStyle == BS_3STATE || typeStyle == BS_AUTO3STATE)
				{
					pDesc->m_Type = CWndObjDescription::Check;
				}*/
			}
			if (pDesc)
			{
				pDesc->UpdateAttributes(this);
				PopulateFontDescription(pDesc);
				PopulateColorDescription(pDesc);
			}
			return (LRESULT) pDesc;
		}

		
	/*Il metodo GetWindowDescription crea da zero una descrizione(del tipo della runtimeclass passata) se non esisteva gia,
	o ripesca quella gia creata nei round trip precedenti se esisteva.
	Nella creazione assegna un id alla descrizione (m_strId), che servira' da chiave per recuperarle.
	Questo id viene creato in modo standard sulla base dell'handle di finestra.
	In alcuni casi pero finestre "differenti" hanno lo stesso id (es. parsedbitmap del bodyedit). 
	In questi casi si puo' creare un ID disambiguo e passarlo al metodo GetWindowDescription.
	*/
	CPushButtonDescription* pBtnDesc = (CPushButtonDescription*)pContainer->GetWindowDescription(this, RUNTIME_CLASS(CPushButtonDescription), strId);
	pBtnDesc->UpdateAttributes(this);
	 
	if (m_pBitmapUp)
	{
		CString sName = cwsprintf(_T("ebtn%ud.png"), (HBITMAP)*m_pBitmapUp);
		
		
		if (pBtnDesc->m_ImageBuffer.Assign( (HBITMAP)*m_pBitmapUp, sName, this))
			pBtnDesc->SetUpdated(&pBtnDesc->m_ImageBuffer);
	}

	return (LRESULT) pBtnDesc;
}

//-----------------------------------------------------------------------------
BOOL CExtButton::ForceUpdateCtrlView()	
{ 
	return m_bColored || m_bCustomDraw; 
}

//-----------------------------------------------------------------------------
void CExtButton::SetPngImages (UINT nIDStdImage, UINT nIDAltImage /*0*/)
{ 
	m_nPngIDStdImage = nIDStdImage; 
	m_nPngIDAltImage = nIDAltImage; 
	m_nPngCurrImage = nIDStdImage;
}

//-----------------------------------------------------------------------------
void CExtButton::SetPngImages(CString sNSStdImage, CString sNSAltImage /*= _T("")*/)
{
	m_sPngNSStdImage = sNSStdImage;
	m_sPngNSAltImage = sNSAltImage;
	m_sPngCurrImage = sNSStdImage;
}

//-----------------------------------------------------------------------------
void CExtButton::ShowAltImage(BOOL bIsAlt /*= TRUE*/)
{

	if (!m_sPngNSStdImage.IsEmpty())
	{ 
		// PNG with name sapce
		if (bIsAlt)
		{
			m_sPngCurrImage = m_sPngNSAltImage;
		}
		else
			m_sPngCurrImage = m_sPngNSStdImage;
	}
	else
	{
		// PNG from ID
		if (bIsAlt && m_nPngIDAltImage != 0)
		{
			m_nPngCurrImage = m_nPngIDAltImage;
		}
		else
			m_nPngCurrImage = m_nPngIDStdImage;
	}
	Invalidate();
} 

//-----------------------------------------------------------------------------
void CExtButton::DrawPNGItem(LPDRAWITEMSTRUCT lpDIS) 
{
	CDC* pDC = GetDC();
	BOOL bIsPressed = (lpDIS->itemState & ODS_SELECTED);
	CRect aBtnRect;
	GetClientRect(&aBtnRect);

	if (m_bDrawFrame)
	{
		// if themed (XP or Vista), draws a themed button face
		if (AfxGetThemeManager()->IsXpThemed())
		{
			CMicroareaVisualManager* pManager = dynamic_cast<CMicroareaVisualManager*>(CBCGPVisualManager::GetInstance());
			if (pManager)
				pManager->DrawThemedButton(m_hWnd, lpDIS->hDC, BP_PUSHBUTTON, (bIsPressed ? PBS_PRESSED : PBS_NORMAL), &aBtnRect, FALSE);
			else
				AfxGetThemeManager()->DrawOldXPButton(m_hWnd, lpDIS->hDC, BP_PUSHBUTTON, (bIsPressed ? PBS_PRESSED : PBS_NORMAL), &aBtnRect);
		}
		else
		{
			// if non-themed, draws a standard button frame
			pDC->DrawFrameControl
				(
					&aBtnRect,
					DFC_BUTTON ,
					DFCS_BUTTONPUSH | (bIsPressed ? DFCS_PUSHED : 0)
				);
		}
	}

	Gdiplus::Bitmap* pBitmap = NULL;
	if (!m_sPngCurrImage.IsEmpty())
	{
		CTBNamespace nsImage(CTBNamespace::IMAGE, m_sPngCurrImage);
		if (AfxGetThemeManager()->IsToAddMoreColor() && this->IsWindowEnabled())
		{
			CString sName = nsImage.GetObjectName();
			CString sNewNamespace = nsImage.ToString();
			sNewNamespace = sNewNamespace.Left(sNewNamespace.GetLength() - sName.GetLength());
			CTBNamespace ns(sNewNamespace + GetName(sName) + _T("_c") + GetExtension(sName));

			if (ExistFile(AfxGetPathFinder()->GetFileNameFromNamespace(ns, AfxGetLoginInfos()->m_strUserName)))
				nsImage = ns;
		}
		
		pBitmap = LoadGdiplusBitmapOrPng(nsImage.ToString());
	}
	else
	{
		pBitmap = LoadPNG(m_nPngCurrImage);
	}
	if (!pBitmap) return;

	if (!m_bUseImageSize)
	{
		// erase previous Image, clean Zone if not present a frame
		if (!m_bDrawFrame)
		{
			CWnd* pParent = GetParent();
			if (IsWindow(this->m_hWnd) && this->IsWindowVisible())
			{
				CBaseFormView* pView = dynamic_cast<CBaseFormView*>(pParent);
				if (pView && pView->GetBackgroundBrush())
					pDC->FillSolidRect(&aBtnRect, pView->GetBackgroundColor());

				CParsedDialog* pTab = dynamic_cast<CParsedDialog*>(pParent);
				if (pTab && pTab->GetBackgroundBrush())
					pDC->FillSolidRect(&aBtnRect, pTab->GetBackgroundColor());
			}
		}
		// End: erase previous Image, clean Zone 

		// PNG is supposed square
		// resize it on the minimum size of the button area
		int imgWidth = min(aBtnRect.Height(), aBtnRect.Width()) - 4; // 2 pixels border	
		if (pBitmap)
		{
			Gdiplus::Graphics graphics(lpDIS->hDC);
			int deltaX = (aBtnRect.Width() - imgWidth) / 2;
			int deltaY = (aBtnRect.Height() - imgWidth) / 2;
			// shifts 1 pixel right if pressed
			graphics.DrawImage(pBitmap, deltaX + (bIsPressed ? 1 : 0), deltaY + (bIsPressed ? 1 : 0), imgWidth, imgWidth);
			graphics.Flush();
			delete pBitmap;
		}
	}
	else
	{
		if (pBitmap)
		{
			Gdiplus::Graphics graphics(lpDIS->hDC);
			int deltaX = (aBtnRect.Width() - pBitmap->GetWidth()) / 2;
			int deltaY = (aBtnRect.Height() - pBitmap->GetHeight()) / 2;
			// shifts 1 pixel right if pressed
			graphics.DrawImage(pBitmap, deltaX + (bIsPressed ? 1 : 0), deltaY + (bIsPressed ? 1 : 0), pBitmap->GetWidth(), pBitmap->GetHeight());
			graphics.Flush();
			delete pBitmap;
		}
	}
	ReleaseDC(pDC);
}

/////////////////////////////////////////////////////////////////////////////
// CFrameToolTip

BEGIN_MESSAGE_MAP(CFrameToolTip, CBCGPToolTipCtrl)
	ON_NOTIFY_REFLECT(TTN_POP, OnPop)
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CFrameToolTip::CFrameToolTip()
{

}

//-----------------------------------------------------------------------------
void CFrameToolTip::SetToolTip(CWnd* pWnd, CString strText)
{
	int nPos = strText.Find(L"\n");
	if (nPos > 0) {
		m_strDescription = strText.Mid(nPos + 1);
		AddTool(pWnd, strText.Mid(0, nPos - 1));
		return;
	}
	AddTool(pWnd, strText);
}

//-----------------------------------------------------------------------------
void CFrameToolTip::OnPop(NMHDR* pNMHDR, LRESULT* pResult)
{
	CString sTmpDesc = m_strDescription;
	__super::OnPop(pNMHDR, pResult);
	m_strDescription = sTmpDesc;
}

//=============================================================================
//			Class CPaneButton	- Button for status bar
//=============================================================================
IMPLEMENT_DYNAMIC(CPaneButton, CExtButton)

BEGIN_MESSAGE_MAP(CPaneButton, CExtButton)
	//{{AFX_MSG_MAP(CParsedButton)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CPaneButton::CPaneButton() :
	m_pBitmap (NULL)
{
}

//-----------------------------------------------------------------------------
CPaneButton::~CPaneButton()
{
	if (m_pBitmap)
	{
		delete m_pBitmap;
	}
}

//-----------------------------------------------------------------------------
void CPaneButton::LoadButtonImage()
{
	if (m_pBitmap == NULL)
	{
		Gdiplus::Bitmap* pBmp;

		if (!m_sPngCurrImage.IsEmpty())	
			pBmp = LoadGdiplusBitmapOrPng(m_sPngCurrImage);
			
		else 
			pBmp = LoadPNG(m_nPngCurrImage);

		UINT w = ScalePix(pBmp->GetWidth());
		UINT h = ScalePix(pBmp->GetHeight());

		m_pBitmap = (Gdiplus::Bitmap*) pBmp->GetThumbnailImage(w, h);


		delete pBmp;
	}
}

//-----------------------------------------------------------------------------
void CPaneButton::DrawPNGItem(LPDRAWITEMSTRUCT lpDIS)
{
	CDC* pDC = CDC::FromHandle(lpDIS->hDC);
	BOOL bIsPressed = (lpDIS->itemState & ODS_SELECTED);
	RECT aBtnRect;
	GetClientRect(&aBtnRect);
	
	LoadButtonImage();
	if (!m_pBitmap) return;

	Gdiplus::Graphics graphics(lpDIS->hDC);

	
	/*graphics.FillRectangle(AfxGetThemeManager()->GetToolbarBkgBrush(), 
		aBtnRect.left, aBtnRect.top, aBtnRect.right - aBtnRect.left, aBtnRect.top - aBtnRect.bottom);*/

	graphics.DrawImage(m_pBitmap, 2, 2, m_pBitmap->GetWidth(), m_pBitmap->GetHeight());
	graphics.Flush();
}

//-----------------------------------------------------------------------------
void CPaneButton::SetToolTip(CString strText)
{
	//non creo i tooltip, non vengono visualizzati se l'iterfaccia è web
	if (AfxIsRemoteInterface())
		return;

	if (!m_ToolTip.Create(this))
		return;
	
	m_ToolTip.SetToolTip(this, strText);
}

//-----------------------------------------------------------------------------
BOOL CPaneButton::PreTranslateMessage(MSG* pMsg)
{
	if (m_ToolTip.m_hWnd)
		m_ToolTip.RelayEvent(pMsg);

	return __super::PreTranslateMessage(pMsg);
}

//=============================================================================
//			Class CParsedButton implementation
//=============================================================================
IMPLEMENT_DYNAMIC (CParsedButton, CExtButton)

BEGIN_MESSAGE_MAP(CParsedButton, CExtButton)
	//{{AFX_MSG_MAP(CParsedButton)
	ON_WM_KILLFOCUS		()
	ON_WM_ENABLE		()
	ON_WM_LBUTTONDOWN	()
	ON_WM_CONTEXTMENU	()
	ON_WM_CANCELMODE()
	ON_WM_MOUSEMOVE()

	//}}AFX_MSG_MAP
END_MESSAGE_MAP()
   
//-----------------------------------------------------------------------------
CParsedButton::CParsedButton()
	:
	CExtButton		(),
	CParsedCtrl		()

{
	CParsedCtrl::Attach(this);
	CParsedCtrl::AttachCustomFont(this);
	
}

//-----------------------------------------------------------------------------
CParsedButton::CParsedButton(DataBool* pData)
	:
	CExtButton		(),
	CParsedCtrl(pData)
{
	CParsedCtrl::Attach(this);
	CParsedCtrl::AttachCustomFont(this);
}

//-----------------------------------------------------------------------------
void CParsedButton::OnCancelMode()
{
	CButton::OnCancelMode();
	RemoveHovering();
}

//-----------------------------------------------------------------------------
void CParsedButton::OnMouseMove(UINT nFlags, CPoint point)
{
	__super::OnMouseMove(nFlags, point);

	CRect rectClient;
	GetClientRect(rectClient);

	CPoint ptScreen = point;
	ClientToScreen(&ptScreen);
	BOOL bRedraw = FALSE;
	BOOL bInRect = rectClient.PtInRect(point) && WindowFromPoint(ptScreen)->GetSafeHwnd() == GetSafeHwnd();
	if (bInRect && !m_bIsHovering)
	{
		bRedraw = TRUE;
		m_bIsHovering = TRUE;
		SetCapture();
	}
	else if (!bInRect && m_bIsHovering)
	{
		bRedraw = TRUE;
		m_bIsHovering = FALSE;
		ReleaseCapture();
	}

	if (bRedraw)
	{
		Invalidate();
		UpdateWindow();
	}
}

// Create, also, associated button (don't need declaration of button in resource file)
//-----------------------------------------------------------------------------
BOOL CParsedButton::Create(DWORD dwStyle, const RECT& rect, CWnd* pParentWnd, UINT nID)
{                 
	if	(
			!CheckControl(nID, pParentWnd)	||
			!__super::Create(_T(""), dwStyle, rect, pParentWnd, nID)
		)
		return FALSE;
	
	CParsedCtrl::AttachCustomFont(this);

	SetDefaultFontControl(this, this);	

	return InitCtrl();
}

// Create, also, associated button (don't need declaration of button in resource file)
//-----------------------------------------------------------------------------
BOOL CParsedButton::SubclassEdit(UINT nID, CWnd* pParentWnd, const CString& strName)
{
	if	(
			!CheckControl(nID, pParentWnd, _T("BUTTON"))	||
			!SubclassDlgItem(nID, pParentWnd)
		)
		return FALSE;
		
	// deve essere fatta come prima cosa (per gli altri controls e` fatta dalla
	// CParsedCtrl::CreateAssociatedButton che qui non ha senso chiamare
	//
	//SetCtrlFont(pParentWnd->GetFont(), FALSE);
	SetDefaultFontControl(this, this);	

	SetNamespace(strName);
	
	return InitCtrl();
}

//-----------------------------------------------------------------------------
void CParsedButton::RemoveHovering()
{
	if (!m_bIsHovering)
		return;

	m_bIsHovering = FALSE;
	ReleaseCapture();
	Invalidate();
	UpdateWindow();
}

//-----------------------------------------------------------------------------
void CParsedButton::OnKillFocus (CWnd* pWnd)
{
	DoKillFocus(pWnd);
	RemoveHovering();
	// standard action
	CButton::OnKillFocus(pWnd);
}

//-----------------------------------------------------------------------------
BOOL CParsedButton::OnCommand(WPARAM wParam, LPARAM lParam)
{
	CParsedCtrl::DoCommand(wParam, lParam);

	BOOL bDone = CButton::OnCommand(wParam, lParam);

	//guardo se è un comando di menu o un accelleratore
	CWnd* pParent = GetCtrlParent();
	CBaseDocument* pDoc  = GetDocument ();
	if (!pDoc && pParent && pParent->IsKindOf(RUNTIME_CLASS(CParsedDialog)))
		pDoc = ((CParsedDialog*)pParent)->GetDocument();

	if (!bDone && lParam == 0 && ((wParam & 0xFFFE0000) == 0) && pDoc)
	{
		//lo ruoto al documento
		POSITION pos = pDoc->GetFirstViewPosition();
		if (pos != NULL)
			pDoc->GetNextView(pos)->PostMessage (WM_COMMAND, wParam, 0);
	}
	return bDone;
}
                                     
//-----------------------------------------------------------------------------
void CParsedButton::OnEnable(BOOL bEnable)
{
	__super::OnEnable(bEnable);
	
	DoEnable(bEnable);
}

//-----------------------------------------------------------------------------
void CParsedButton::OnLButtonDown(UINT nFlag, CPoint mousePos)
{
	SetFocus();
	
	if (IsWindowEnabled())
		__super::OnLButtonDown (nFlag, mousePos);
}

//-----------------------------------------------------------------------------
void CParsedButton::OnContextMenu(CWnd* pWnd, CPoint mousePos)
{
	if (!CParsedCtrl::DoContextMenu(pWnd, mousePos))
		__super::OnContextMenu(pWnd, mousePos);
}

//=============================================================================
//			Class CBoolButton implementation
//=============================================================================
IMPLEMENT_DYNCREATE (CBoolButton, CParsedButton)

BEGIN_MESSAGE_MAP(CBoolButton, CParsedButton)
	//{{AFX_MSG_MAP(CBoolButton)
	ON_MESSAGE				(BM_SETCHECK,	OnBtnSetCheck)
	ON_MESSAGE				(WM_SETTEXT,	OnSetText)
	ON_WM_PAINT				()
	ON_WM_NCPAINT			()
	ON_WM_ERASEBKGND		()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()
            
//-----------------------------------------------------------------------------
CBoolButton::CBoolButton()
	:
	CParsedButton		(),
	m_bManualChecking	(TRUE),
	m_bFirstPaint		(TRUE)
{}
                        
//-----------------------------------------------------------------------------
CBoolButton::CBoolButton(DataBool* pData)
	:
	CParsedButton		(pData),
	m_bManualChecking	(TRUE),
	m_bFirstPaint(TRUE)

{}
                        
//-----------------------------------------------------------------------------
LRESULT CBoolButton::OnSetText(WPARAM wParam, LPARAM lParam)
{
	return DefWindowProc(WM_SETTEXT, wParam, lParam);
}

//-----------------------------------------------------------------------------
LRESULT CBoolButton::OnBtnSetCheck(WPARAM wParam, LPARAM lParam)
{
	BOOL bUpdateData =	GetCheck() != (int) wParam;
	
	DefWindowProc(BM_SETCHECK, wParam, lParam);

	if (bUpdateData && m_bManualChecking)
	{
		SetModifyFlag(TRUE);
		CParsedCtrl::UpdateCtrlData(TRUE);
	}

	return 0L;
}

//-----------------------------------------------------------------------------
void CBoolButton::DoEnable(BOOL bEnable)
{
	CRect entireRect;
	GetWindowRect(entireRect);
	GetParent()->ScreenToClient(entireRect);
	GetParent()->RedrawWindow(entireRect);

	__super::DoEnable(bEnable);
}

//-----------------------------------------------------------------------------
BOOL CBoolButton::OnInitCtrl()
{
	VERIFY(CParsedButton::OnInitCtrl());
	
	// questo control e` valido solo per CheckBox e RadioButton (vedere windows.h)
	//
	DWORD dwStyle = GetButtonStyle();
	if	(
			((dwStyle & BS_CHECKBOX)		!= BS_CHECKBOX)		&&
			((dwStyle & BS_AUTOCHECKBOX)	!= BS_AUTOCHECKBOX)	&&
			((dwStyle & BS_RADIOBUTTON)		!= BS_RADIOBUTTON)	&&
			((dwStyle & BS_AUTORADIOBUTTON)	!= BS_AUTORADIOBUTTON)
		)
	{
		ASSERT(FALSE);
		return FALSE;
	}

	// To default the text align to left
	SetWindowLong(m_hWnd, GWL_STYLE, ::GetWindowLong(m_hWnd, GWL_STYLE) | BS_LEFT);
	
	return TRUE;
}

//-----------------------------------------------------------------------------
void CBoolButton::SetValue(const DataObj& aValue)
{
	DataObj& pData = const_cast<DataObj&>(aValue);
	ASSERT(CheckDataObjType(&pData));
	SetValue((BOOL) ((DataBool&)pData));
}

//-----------------------------------------------------------------------------
void CBoolButton::GetValue(DataObj& aValue)
{
	ASSERT(CheckDataObjType(&aValue));

	((DataBool&) aValue).Assign(GetValue());
}

//-----------------------------------------------------------------------------
CSize CBoolButton::AdaptNewSize(UINT nCols, UINT /* nRows */, BOOL /* bButtonsIncluded */)
{
	if (nCols == 0)
		nCols = 1;

	CDC* pDC = GetDC();
	CSize cs = GetEditSize(pDC, GetFont(), nCols, 1);
	ReleaseDC(pDC);

	cs.cx += nCols > 0 ? 23 : 10;	// per tener conto dello spazio occupato dal check
	
	return cs;
}

//-----------------------------------------------------------------------------
void CBoolButton::SetValue(BOOL bValue)
{
	m_bManualChecking = FALSE;
	if (m_hWnd)
		SetCheck ( bValue ? 1 : 0 );
	m_bManualChecking = TRUE;
}

//-----------------------------------------------------------------------------
BOOL CBoolButton::GetValue()
{
	return GetCheck();
}

//-----------------------------------------------------------------------------
BOOL CBoolButton::NeedCustomPaint()
{
	return	m_bColored ||
			(
			!IsWindowEnabled() &&
			m_pDocument && m_pDocument->UseEasyReading()
			);
}

//-----------------------------------------------------------------------------
void CBoolButton::DrawFocusRectForChildWindows()
{
	__super::OnPaint();	

	if (!this->GetParentFrame())
		return;

	//Se la finestra in cui mi trovo è Tabbed (e quindi ha lo stile WS_CHILD)  il focus rect delle
	//checkbox non viene disegnato, per cui lo ridisegno forzatamente
	//unico effetto collaterale è che se la finestra viene stabbata, si da il focus ad una checkbox
	//e si riattacca la finestra, vengono mostrati entrambi i focus rect
	DWORD style = this->GetParentFrame()->GetStyle();
	if ((style & (WS_CHILD)) != WS_CHILD)
		return;

	RECT rectText;
	this->GetClientRect(&rectText);

	rectText.top		= rectText.top; 
	rectText.bottom		= rectText.bottom; 
	if ((GetStyle() & BS_LEFTTEXT) != BS_LEFTTEXT)
		rectText.left = rectText.left + (AfxGetThemeManager()->IsXpThemed() ? 15 : 17); // different displacement under Win2000 - just for fun
	rectText.right		= rectText.right;

	CDC* pDC = this->GetDC();
	if (pDC)
	{
		pDC->DrawFocusRect(&rectText);
		this->ReleaseDC(pDC);
	}
}

//-----------------------------------------------------------------------------
BOOL CBoolButton::OnEraseBkgnd(CDC* pDC)
{
	CRect aRect;
	this->GetClientRect(aRect);

	CParsedForm* pParsedForm = ::GetParsedForm(this->GetParent());

	if (pParsedForm)
		pDC->FillRect(aRect, pParsedForm->GetBackgroundBrush());

	return TRUE;
}

//-----------------------------------------------------------------------------
void CBoolButton::OnNcPaint()
{
	__super::OnNcPaint();
}

//-----------------------------------------------------------------------------
void CBoolButton::OnPaint( )
{
	CPaintDC dc(this);

	CBrush* pBrush = NULL;
	CParsedForm* pParsedForm = ::GetParsedForm(this->GetParent());

	int nIdx;
	BOOL bIsInStaticArea = FALSE;
	CBaseTileDialog* pTileDialog = NULL;
	BOOL bAllowCheckBoxInStaticArea = FALSE;
	if (pParsedForm &&  pParsedForm->GetFormCWnd()->IsKindOf(RUNTIME_CLASS(CBaseTileDialog)))
	{
		// Color brush static area color
		pTileDialog = (CBaseTileDialog*)pParsedForm->GetFormCWnd();
		bIsInStaticArea = pTileDialog->IsInStaticArea(this, &nIdx);
		bAllowCheckBoxInStaticArea = pTileDialog->GetAllowCheckBoxInStaticArea();
	}
	
	TBThemeManager* pThemeManager = AfxGetThemeManager();
	//---- custom background color
	COLORREF crBkgnd = pThemeManager->GetBackgroundColor();
	if (pParsedForm)	
	{ 
		pBrush = const_cast<CBrush*>(pParsedForm->GetBackgroundBrush());	
	}

	if (!pBrush)
	{ 
		pBrush = const_cast<CBrush*>(pThemeManager->GetBackgroundColorBrush()); 
	}

	if (bAllowCheckBoxInStaticArea && bIsInStaticArea)
		pBrush = pThemeManager->GetTileDialogStaticAreaBkgColorBrush();

	ASSERT(pBrush);
	BOOL bIsTransparent = pParsedForm && pParsedForm->IsTransparent();
	if (bIsTransparent)
	{
		crBkgnd = pParsedForm->GetBackgroundColor(m_hWnd);
		pBrush = pParsedForm->GetBackgroundBrush(m_hWnd);
	}

	CDC* pDC = &dc;
	if (pDC)
	{
		CMicroareaVisualManager* pManager = dynamic_cast<CMicroareaVisualManager*>(CBCGPVisualManager::GetInstance());

		pDC->SetBkMode(TRANSPARENT);
		BOOL bIsEnable = pManager && pManager->IsToUseEnabledBorderColor(this, IsWindowEnabled());
		BOOL bHasFocus = HasFocus();

		CRect r;
		GetClientRect(&r);

		CString strText;
		GetWindowText(strText);
		
		pDC->SetTextColor
		(bIsEnable ?
			this->m_crText :
			(bIsInStaticArea ?
				AfxGetThemeManager()->GetDisabledInStaticAreaControlForeColor() :
				AfxGetThemeManager()->GetDisabledControlForeColor()
				)
		);
		HGDIOBJ old = pDC->SelectObject(GetPreferredFont());
		
		if (m_bFirstPaint)
		{
			CalculateCheckboxAndRadioRect(r, strText, m_rectCheck, m_dwCheckStyle, m_rectText, m_dwTextStyle, GetPreferredFont());
			if (bIsInStaticArea && !bAllowCheckBoxInStaticArea)
			{
				m_rectCheck.MoveToX(pTileDialog->GetStaticAreaRect(nIdx).Width() + (AfxGetThemeManager()->GetTileInnerLeftPadding() + 1));
				m_rectText = CRect(m_rectText.left, m_rectText.top, pTileDialog->GetStaticAreaRect(nIdx).Width() - (AfxGetThemeManager()->GetTileInnerLeftPadding() + 2), m_rectText.bottom);
			}

			//In alcuni casi al primo paint le label arrivano vuote (BCG ?)
			if (IsWindowVisible() && !strText.IsEmpty())
				m_bFirstPaint = FALSE;

			pDC->FillRect(&r, pBrush);
		}

		if (pThemeManager->IsXpThemed())
		{
			// putroppo ci sono casi (come l'abilitazione disabilitazione) che lasciano il disagno fatto da MFC : forzo un filrect dell'area (dicesi tapullo)
			CRect cRect(m_rectCheck);
			cRect.InflateRect(2,2);
			pDC->FillRect(&cRect, pBrush);

			if (pManager)
				pManager->DrawThemedButton
				(
					m_hWnd,
					pDC->m_hDC,
					m_dwCheckStyle,
					(
					(m_dwCheckStyle & BP_RADIOBUTTON) == BP_RADIOBUTTON
						?
						(GetCheck() ? (bIsEnable ? (bHasFocus ? RBS_CHECKEDHOT : RBS_CHECKEDNORMAL) : RBS_CHECKEDDISABLED) : (bIsEnable ? (bHasFocus ? RBS_UNCHECKEDHOT : RBS_UNCHECKEDNORMAL) : RBS_UNCHECKEDDISABLED))
						:
						(GetCheck() ? (bIsEnable ? (bHasFocus ? CBS_CHECKEDHOT : CBS_CHECKEDNORMAL) : CBS_CHECKEDDISABLED) : (bIsEnable ? (bHasFocus ? CBS_UNCHECKEDHOT : CBS_UNCHECKEDNORMAL) : CBS_UNCHECKEDDISABLED))
						),
					&m_rectCheck,
					m_bIsHovering
				);
			else
				pThemeManager->DrawOldXPButton
				(
					m_hWnd,
					pDC->m_hDC,
					m_dwCheckStyle,
					(
					(m_dwCheckStyle & BP_RADIOBUTTON) == BP_RADIOBUTTON
						?
						(GetCheck() ? (bIsEnable ? (bHasFocus ? RBS_CHECKEDHOT : RBS_CHECKEDNORMAL) : RBS_CHECKEDDISABLED) : (bIsEnable ? (bHasFocus ? RBS_UNCHECKEDHOT : RBS_UNCHECKEDNORMAL) : RBS_UNCHECKEDDISABLED))
						:
						(GetCheck() ? (bIsEnable ? (bHasFocus ? CBS_CHECKEDHOT : CBS_CHECKEDNORMAL) : CBS_CHECKEDDISABLED) : (bIsEnable ? (bHasFocus ? CBS_UNCHECKEDHOT : CBS_UNCHECKEDNORMAL) : CBS_UNCHECKEDDISABLED))
						),
					&m_rectCheck
				);
		}
		else
		{
			pDC->FillRect(&m_rectCheck, pBrush); 
			pDC->DrawFrameControl
				(
					&m_rectCheck,
					DFC_BUTTON,
					m_dwCheckStyle | (GetCheck() ? DFCS_CHECKED : 0) 
				);
		}

		COLORREF oldBkgColor;

		if (!bIsTransparent)
		{
			oldBkgColor = pDC->SetBkColor(crBkgnd);
			CRect rectEntireRectText = m_rectText;
			if (bIsInStaticArea)
			{ 
				pBrush = pTileDialog->GetStaticAreaBrush();
				oldBkgColor = pDC->SetBkColor(pThemeManager->GetTileDialogStaticAreaBkgColor());
				if (!bAllowCheckBoxInStaticArea)
					rectEntireRectText = CRect(m_rectText.left, m_rectText.top, pTileDialog->GetStaticAreaRect(nIdx).Width(), m_rectText.bottom);
			}
		
			pDC->FillRect(&rectEntireRectText, pBrush);
		}

		pDC->DrawText(strText, &m_rectText, m_dwTextStyle);

		if (bHasFocus && !strText.IsEmpty())
			pDC->DrawFocusRect(&m_rectText);
		
		if (!bIsTransparent)
			pDC->SetBkColor(oldBkgColor);

		pDC->SelectObject(old);
		ReleaseDC(pDC);
	}
}

//=============================================================================
//			Class CBoolButtonStatic implementation
//=============================================================================
IMPLEMENT_DYNCREATE(CBoolButtonStatic, CBoolButton)

//-----------------------------------------------------------------------------
CBoolButtonStatic::CBoolButtonStatic()
:
CBoolButton()
{}

//-----------------------------------------------------------------------------
void CBoolButtonStatic::Attach(DataObj* pDataObj)
{
	CParsedCtrl::Attach(pDataObj);
	pDataObj->SetAlwaysReadOnly();
}

//=============================================================================
//			Class CPushButton implementation
//=============================================================================
IMPLEMENT_DYNCREATE (CPushButton, CParsedButton)

BEGIN_MESSAGE_MAP(CPushButton, CParsedButton)
	//{{AFX_MSG_MAP(CPushButton)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()
            
//-----------------------------------------------------------------------------
CPushButton::CPushButton()
	:
	CParsedButton	()
{}
                        
//-----------------------------------------------------------------------------
CPushButton::CPushButton(DataBool* pData)
	:
	CParsedButton	(pData)
{}
                        
//-----------------------------------------------------------------------------
BOOL CPushButton::OnInitCtrl()
{
	VERIFY(CParsedButton::OnInitCtrl());
	
	// questo control e` valido solo per PushButton (vedere windows.h)
	//
	DWORD dwStyle = (GetButtonStyle() & 0x0000000F) & ~BS_OWNERDRAW;
	if	(
			dwStyle != BS_PUSHBUTTON	&&
			dwStyle != BS_DEFPUSHBUTTON
		)
	{
		ASSERT(FALSE);
		return FALSE;
	}

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CPushButton::ShowCtrl(int nCmdShow)
{
	//if (IsDataOSLHide())
	//{
	//	nCmdShow = SW_HIDE;
	//}
	return __super::ShowCtrl(nCmdShow);
}

//-----------------------------------------------------------------------------
BOOL CPushButton::EnableCtrl(BOOL bEnable /* = TRUE */)
{
	//if (IsDataOSLReadOnly())
	//{
	//	bEnable = FALSE;
	//}
	return __super::EnableCtrl(bEnable);
}

//-----------------------------------------------------------------------------
void CPushButton::SetValue(const DataObj& aValue)
{
	ASSERT(CheckDataObjType(&aValue));
	BOOL bVal = (BOOL) ((DataBool&) aValue);
	if ((m_dwCtrlStyle & BTN_STYLE_INVERTED) == BTN_STYLE_INVERTED)
		bVal = !bVal;

	SetValue(bVal);
}

//-----------------------------------------------------------------------------
void CPushButton::GetValue(DataObj& aValue)
{
	ASSERT(CheckDataObjType(&aValue));

	((DataBool&) aValue).Assign(*GetCtrlData());
}

//-----------------------------------------------------------------------------
void CPushButton::SetValue(BOOL bValue)
{
	if (IsDataOSLReadOnly() || IsDataOSLHide())
		bValue = FALSE;
	EnableWindow(bValue);
}

//-----------------------------------------------------------------------------
BOOL CPushButton::GetValue()
{
	return (BOOL) ((DataBool&) *GetCtrlData());
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
//			Class CChildButton implementation
//=============================================================================
IMPLEMENT_DYNAMIC (CChildButton, CParsedButton)

BEGIN_MESSAGE_MAP(CChildButton, CParsedButton)
	//{{AFX_MSG_MAP(CChildButton)
	ON_WM_KILLFOCUS			()
	ON_WM_PAINT				()
	ON_MESSAGE				(BM_SETCHECK,	OnBtnSetCheck)
	ON_WM_ENABLE			()
	ON_WM_CONTEXTMENU		()
	ON_COMMAND				(ID_CTRL_BEHAVIOR, OnFind)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()
            
//-----------------------------------------------------------------------------
CChildButton::CChildButton(CParsedGroupBtn* pOwnerGroup, WORD wItem, UINT nID)
	:
	CParsedButton(),
	m_bAttached		(FALSE),
	m_pOwnerGroupBtn(pOwnerGroup),
	m_wEnumItem		(wItem),
	m_bFirstPaint	(TRUE),
	m_nID			(nID)
{
	ASSERT_VALID(pOwnerGroup);
}
                        
//-----------------------------------------------------------------------------
CChildButton::~CChildButton()
{
}

//-----------------------------------------------------------------------------
BOOL CChildButton::Create(DWORD dwStyle, const RECT& rect, CWnd* pParentWnd, UINT nID)
{                 
	ASSERT(!m_bAttached);   
	
	m_bAttached = TRUE;

	return __super::Create(dwStyle, rect, pParentWnd, nID);

	//return __super::Create(_T(""), dwStyle, rect, pParentWnd, nID);
}

// Create, also, associated button (don't need declaration of button in resource file)
//-----------------------------------------------------------------------------
BOOL CChildButton::SubclassEdit(UINT nID, CWnd* pParentWnd)
{
	ASSERT(!m_bAttached);   
	
	m_bAttached = TRUE;

	return SubclassDlgItem(nID, pParentWnd);
}

//-----------------------------------------------------------------------------
void CChildButton::OnKillFocus (CWnd* pWnd)
{
    GetOwnerGroupBtn()->DoKillFocus(pWnd);
    
    CExtButton::OnKillFocus(pWnd);
}
                                     
//-----------------------------------------------------------------------------
LRESULT CChildButton::OnBtnSetCheck(WPARAM wParam, LPARAM lParam)
{
	BOOL bUpdateData =	GetCheck() != (int) wParam;
	
	DefWindowProc(BM_SETCHECK, wParam, lParam);

	if (bUpdateData && GetOwnerGroupBtn()->m_bManualChecking)
	{
		GetOwnerGroupBtn()->SetModifyFlag(TRUE);
		if ((GetButtonStyle() & BS_AUTORADIOBUTTON) == BS_AUTORADIOBUTTON  && wParam == 0)
			GetOwnerGroupBtn()->UpdateCtrlData(TRUE);
	}

	return 0L;
}

//-----------------------------------------------------------------------------
BOOL CChildButton::NeedCustomPaint()
{
	return		m_bColored 
			|| 
				(GetOwnerGroupBtn()->m_bColored && GetOwnerGroupBtn()->m_crBkgnd != AfxGetThemeManager()->GetBackgroundColor())
			||
			(
				!IsWindowEnabled() &&
				GetOwnerGroupBtn()->m_pDocument && GetOwnerGroupBtn()->m_pDocument->UseEasyReading()
			);
}

//-----------------------------------------------------------------------------
void CChildButton::OnPaint( )
{
	CPaintDC dc(this);

	CBrush* pBrush = NULL;
	CParsedForm* pParsedForm = ::GetParsedForm(this->GetParent());

	int nIdx;
	BOOL bIsInStaticArea = FALSE;
	CBaseTileDialog* pTileDialog = NULL;
	if (pParsedForm &&  pParsedForm->GetFormCWnd()->IsKindOf(RUNTIME_CLASS(CBaseTileDialog)))
	{
		// Color brush static area color
		pTileDialog = (CBaseTileDialog*)pParsedForm->GetFormCWnd();
		bIsInStaticArea = pTileDialog->IsInStaticArea(this, &nIdx);
	}

	TBThemeManager* pThemeManager = AfxGetThemeManager();
	//---- custom background color
	COLORREF crBkgnd = pThemeManager->GetBackgroundColor();
	if (pParsedForm)
	{
		pBrush = const_cast<CBrush*>(pParsedForm->GetBackgroundBrush());
	}

	if (!pBrush)
	{
		pBrush = const_cast<CBrush*>(pThemeManager->GetBackgroundColorBrush());
	}

	ASSERT(pBrush);
	BOOL bIsTransparent = pParsedForm && pParsedForm->IsTransparent();
	if (bIsTransparent)
	{
		crBkgnd = pParsedForm->GetBackgroundColor(m_hWnd);
		pBrush = pParsedForm->GetBackgroundBrush(m_hWnd);
	}

	CDC* pDC = &dc;
	if (pDC)
	{
		CMicroareaVisualManager* pManager = dynamic_cast<CMicroareaVisualManager*>(CBCGPVisualManager::GetInstance());
		pDC->SetBkMode(TRANSPARENT);
		BOOL bIsEnable = pManager && pManager->IsToUseEnabledBorderColor(this, IsWindowEnabled());
		BOOL bHasFocus = this == CWnd::GetFocus();

		CRect r;
		GetClientRect(&r);

		CString strText;
		GetWindowText(strText);

		pDC->SetTextColor
		(bIsEnable ?
			this->m_crText :
			(
				bIsInStaticArea ?
				AfxGetThemeManager()->GetDisabledInStaticAreaControlForeColor() :
				AfxGetThemeManager()->GetDisabledControlForeColor()
				)
		);
		HGDIOBJ old = pDC->SelectObject(GetWndPreferredFont());

		if (m_bFirstPaint)
		{
			CalculateCheckboxAndRadioRect(r, strText, m_rectCheck, m_dwCheckStyle, m_rectText, m_dwTextStyle, GetWndPreferredFont());
			if (bIsInStaticArea)
			{
				m_rectCheck.MoveToX(pTileDialog->GetStaticAreaRect(nIdx).Width() + (AfxGetThemeManager()->GetTileInnerLeftPadding() + 1));
				m_rectText = CRect(m_rectText.left, m_rectText.top, pTileDialog->GetStaticAreaRect(nIdx).Width() - (AfxGetThemeManager()->GetTileInnerLeftPadding() + 2), m_rectText.bottom);
			}

			//In alcuni casi al primo paint le label arrivano vuote (BCG ?)
			if (IsWindowVisible() && !strText.IsEmpty())
				m_bFirstPaint = FALSE;

			pDC->FillRect(&r, pBrush);
		}

		if (pThemeManager->IsXpThemed())
		{
			if (pManager)
				pManager->DrawThemedButton
				(
					m_hWnd,
					pDC->m_hDC,
					m_dwCheckStyle,
					(
					(m_dwCheckStyle & BP_RADIOBUTTON) == BP_RADIOBUTTON
						?
						(GetCheck() ? (bIsEnable ? (bHasFocus ? RBS_CHECKEDHOT : RBS_CHECKEDNORMAL) : RBS_CHECKEDDISABLED) : (bIsEnable ? (bHasFocus ? RBS_UNCHECKEDHOT : RBS_UNCHECKEDNORMAL) : RBS_UNCHECKEDDISABLED))
						:
						(GetCheck() ? (bIsEnable ? (bHasFocus ? CBS_CHECKEDHOT : CBS_CHECKEDNORMAL) : CBS_CHECKEDDISABLED) : (bIsEnable ? (bHasFocus ? CBS_UNCHECKEDHOT : CBS_UNCHECKEDNORMAL) : CBS_UNCHECKEDDISABLED))
						),
					&m_rectCheck,
					FALSE
				);
			else
				AfxGetThemeManager()->DrawOldXPButton
				(
					m_hWnd,
					pDC->m_hDC,
					m_dwCheckStyle,
					(
					(m_dwCheckStyle & BP_RADIOBUTTON) == BP_RADIOBUTTON
						?
						(GetCheck() ? (bIsEnable ? (bHasFocus ? RBS_CHECKEDHOT : RBS_CHECKEDNORMAL) : RBS_CHECKEDDISABLED) : (bIsEnable ? (bHasFocus ? RBS_UNCHECKEDHOT : RBS_UNCHECKEDNORMAL) : RBS_UNCHECKEDDISABLED))
						:
						(GetCheck() ? (bIsEnable ? (bHasFocus ? CBS_CHECKEDHOT : CBS_CHECKEDNORMAL) : CBS_CHECKEDDISABLED) : (bIsEnable ? (bHasFocus ? CBS_UNCHECKEDHOT : CBS_UNCHECKEDNORMAL) : CBS_UNCHECKEDDISABLED))
						),
					&m_rectCheck

				);
		}
		else
		{
			pDC->FillRect(&m_rectCheck, pBrush);
			pDC->DrawFrameControl
				(
					&m_rectCheck,
					DFC_BUTTON,
					m_dwCheckStyle | (GetCheck() ? DFCS_CHECKED : 0)
					);
		}

		COLORREF oldBkgColor;

		if (!bIsTransparent)
		{
			oldBkgColor = pDC->SetBkColor(crBkgnd);
			CRect rectEntireRectText = m_rectText;
			if (bIsInStaticArea)
			{
				pBrush = pTileDialog->GetStaticAreaBrush();
				oldBkgColor = pDC->SetBkColor(pThemeManager->GetTileDialogStaticAreaBkgColor());
				rectEntireRectText = CRect(m_rectText.left, m_rectText.top, pTileDialog->GetStaticAreaRect(nIdx).Width(), m_rectText.bottom);
			}

			pDC->FillRect(&rectEntireRectText, pBrush);
		}

		pDC->DrawText(strText, &m_rectText, m_dwTextStyle);

		if (bHasFocus && !strText.IsEmpty())
			pDC->DrawFocusRect(&m_rectText);

		if (!bIsTransparent)
			pDC->SetBkColor(oldBkgColor);

		pDC->SelectObject(old);
		ReleaseDC(pDC);
	}
}

//-----------------------------------------------------------------------------
void CChildButton::OnEnable(BOOL bEnable)
{
	if (
		(m_pOwnerGroupBtn ? m_pOwnerGroupBtn->UseEasyReading() : AfxGetThemeManager()->UseEasyReading()) &&
		!bEnable
		)
		Invalidate();
	__super::OnEnable(bEnable);
}

//-----------------------------------------------------------------------------
void CChildButton::DoEnable(BOOL bEnable)
{
	CRect entireRect;
	GetWindowRect(entireRect);
	GetParent()->ScreenToClient(entireRect);
	GetParent()->RedrawWindow(entireRect);
	
	__super::DoEnable(bEnable);
}

//-----------------------------------------------------------------------------
void CChildButton::OnFind()
{
	if 
		(
			m_pOwnerGroupBtn && m_pOwnerGroupBtn->m_pData && 
			m_pOwnerGroupBtn->GetDocument() && 
			m_pOwnerGroupBtn->GetDocument()->GetFormMode() == CBaseDocument::FIND
		)
	{
		BOOL bSet = !m_pOwnerGroupBtn->m_pData->IsValueChanged();
		m_pOwnerGroupBtn->m_pData->Clear(); // mette il valuechanged a false
		m_pOwnerGroupBtn->m_pData->SetValueChanged(bSet);	
	}
}

//-----------------------------------------------------------------------------
void CChildButton::OnContextMenu (CWnd* pWnd, CPoint ptMousePos)
{
	if (
			m_pOwnerGroupBtn && m_pOwnerGroupBtn->m_pData &&
			m_pOwnerGroupBtn->GetDocument() &&
			m_pOwnerGroupBtn->GetDocument()->GetFormMode() == CBaseDocument::FIND
		)
	{
		CMenu   menu;	
		menu.CreatePopupMenu();

		if (m_pOwnerGroupBtn->m_pData->IsValueChanged() || !m_pOwnerGroupBtn->m_pData->IsEmpty())
			menu.AppendMenu(MF_STRING, ID_CTRL_BEHAVIOR, _TB("Remove from search"));
		else
			menu.AppendMenu(MF_STRING, ID_CTRL_BEHAVIOR, _TB("Insert in search"));
		
		AfxGetThreadContext()->RaiseCallBreakEvent();
	
		menu.TrackPopupMenu (TPM_LEFTBUTTON, ptMousePos.x, ptMousePos.y, this);
	}
}

//=============================================================================
//			Class CGroupBoxBtn implementation
//=============================================================================
IMPLEMENT_DYNAMIC (CGroupBoxBtn, CExtButton)

BEGIN_MESSAGE_MAP(CGroupBoxBtn, CExtButton)
	//{{AFX_MSG_MAP(CGroupBoxBtn)
	ON_WM_PAINT				()
	ON_WM_ERASEBKGND		()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()                  

//-----------------------------------------------------------------------------
CGroupBoxBtn::CGroupBoxBtn()
	:
	CExtButton ()
{
}

//-----------------------------------------------------------------------------
BOOL CGroupBoxBtn::OnEraseBkgnd (CDC* pDC)
{ 
	if (m_bColored)
		return TRUE; 
	else
		return __super::OnEraseBkgnd(pDC);
}

//-----------------------------------------------------------------------------
void CGroupBoxBtn::OnPaint()
{
	if (m_bColored)
	{
		PaintGroupBox();

		//::RedrawInnerControls(this);
	}
	else
		__super::OnPaint();
}

//-----------------------------------------------------------------------------
void CGroupBoxBtn::SetZOrderInnerControls (CWnd* pParentWnd /*= NULL*/)
{
	CWnd* pParent = pParentWnd ? pParentWnd : GetParent();
	CBaseFormView* pView = dynamic_cast<CBaseFormView*>(pParent);
	if (pView)
		pView->SetZOrderInnerControls (this, pParent);
	else
	{
		CParsedDialog* pDialog = dynamic_cast<CParsedDialog*>(pParent);
		if (pDialog)
			pDialog->SetZOrderInnerControls (this, pParent);
	}
}

//-----------------------------------------------------------------------------
void CGroupBoxBtn::PaintGroupBox()
{
	CPaintDC dc(this); // device context for painting

	// Get the rectangle
	CRect clientRect;
	this->GetClientRect(&clientRect);

	dc.SetBkMode(TRANSPARENT);
	HBRUSH hBrush = GetBkgBrushHandle();
	COLORREF crBkgnd = m_crBkgnd;

	if (m_bColored)
	{
		dc.SetTextColor(m_crText);

		ASSERT(m_pBrush);
		dc.SetBkColor(m_crBkgnd);
		dc.SelectObject(GetBkgBrush());

		//spariscono alcuni controlli 
		//dc.FillSolidRect(&clientRect, crBkgnd);
	}
	else
	{
		CWnd* pParent = this->GetParent();
		if (pParent->IsKindOf(RUNTIME_CLASS(CBaseFormView)))
		{
			CBaseFormView* pView = (CBaseFormView*) pParent;
			if (pView->GetBackgroundBrush())
			{
				dc.SetBkColor(pView->GetBackgroundColor()); crBkgnd = pView->GetBackgroundColor();
				dc.SelectObject(pView->GetBackgroundBrush()); hBrush =  (HBRUSH) pView->GetBackgroundBrush()->GetSafeHandle();
			}
		}
		else if (pParent->IsKindOf(RUNTIME_CLASS(CParsedDialog)))
		{
			CParsedDialog* pTab = (CParsedDialog*) pParent;
			if (pTab->GetBackgroundBrush())
			{
				dc.SetBkColor(pTab->GetBackgroundColor()); crBkgnd = pTab->GetBackgroundColor();
				dc.SelectObject(pTab->GetBackgroundBrush()); hBrush =  (HBRUSH) pTab->GetBackgroundBrush()->GetSafeHandle();
			}
		}
	}

	const int nSep = 2;

	// Get the text
	CString text;
	this->GetWindowText(text);

	// Style is set using the "Align Text" property in the dialog editor, or as a parameter to CreateWindow or CreateWindowEx
	const DWORD windowStyle(this->GetStyle());

	ASSERT (this->IsKindOf(RUNTIME_CLASS(CGroupBoxBtn)));
	ASSERT(windowStyle & BS_GROUPBOX);

	dc.SelectObject(((CGroupBoxBtn*)this)->GetWndPreferredFont());

	const int offsetSize(3/*textSize.cy / 2*/); // offset the start of the line a little bit from the edge of the text
	
	CRect textRect(clientRect);

	CSize textSize(dc.GetTextExtent(text));

	const DWORD textDrawingFlags(DT_SINGLELINE | DT_NOCLIP | DT_LEFT);

	int nTopLineY = textSize.cy / 2;
	
	CPoint textLocation(0,0); // have to calculate, since DrawState does not have a center flag (is there a DSS_CENTER, and I just missed it ???)

	// Draw The Line(s)
	int hdir;
	if ((BS_CENTER & windowStyle) == BS_CENTER)	// centered text	
	{ 	
		hdir = 1;
	}
	else if (BS_RIGHT & windowStyle)		// right-aligned text
	{ 
		hdir = 2;
	}
	else 	// left-aligned text
	{ 
		//ASSERT(BS_LEFT & windowStyle);
		hdir = 0;
	}

	switch (hdir)
	{
		case 0:	//LEFT	- left-aligned text
			{
				textRect.left += offsetSize + 7;
				textRect.right = textRect.left + textSize.cx;
				break;
			}
		case 1:	//CENTER
			{
				const int eachSideWidth = (clientRect.Width() - textSize.cx) / 2;
				textRect.left = eachSideWidth;
				textRect.right = textRect.left + textSize.cx;
				break;
			}
		case 2:	//RIGHT
			{
				textRect.right -= (offsetSize + 7);
				textRect.left = textRect.right - textSize.cx;
				break;
			}
	}
	DrawHLine(dc, nTopLineY, clientRect.left, textRect.left - offsetSize, m_crLine, m_nSizeLinePen == 1);
	DrawHLine(dc, nTopLineY, textRect.right + offsetSize, clientRect.right, m_crLine, m_nSizeLinePen == 1);

	if (GetBkgColor() == AfxGetThemeManager()->GetBackgroundColor())
	{
		clientRect.top = nTopLineY;
	}

	CPen pen(PS_SOLID, m_nSizeLinePen, m_crLine);
	dc.SelectObject(&pen);

	//-----
	DrawVLine(dc, clientRect.left,						nTopLineY, clientRect.bottom, m_crLine, m_nSizeLinePen == 1, TRUE);
	DrawVLine(dc, clientRect.right - m_nSizeLinePen,	nTopLineY, clientRect.bottom, m_crLine, m_nSizeLinePen == 1, FALSE);

	DrawHLine(dc, clientRect.bottom - m_nSizeLinePen,	clientRect.left, clientRect.right, m_crLine, m_nSizeLinePen == 1);

	dc.FillSolidRect
				(
					textRect.left - offsetSize, 
					nTopLineY - m_nSizeLinePen,
					textRect.Width() + 2 * offsetSize, 
					2 * m_nSizeLinePen, 
					crBkgnd
				);

	// Draw the text
	
	//const UINT drawStateFlags(this->IsWindowEnabled() ? DSS_NORMAL : DSS_DISABLED);
	const UINT drawStateFlags(DSS_NORMAL);

	if (!text.IsEmpty())
		dc.DrawState
			(
				CPoint(textRect.left, textRect.top),
				CSize(0,0), // 0,0 says to calculate it (per DrawState SDK docs)
				text, 
				drawStateFlags,
				TRUE, 0, 
				hBrush
			);
}

//=============================================================================
//			Class CParsedGroupBtn implementation
//=============================================================================
IMPLEMENT_DYNAMIC (CParsedGroupBtn, CGroupBoxBtn)

BEGIN_MESSAGE_MAP(CParsedGroupBtn, CGroupBoxBtn)
	//{{AFX_MSG_MAP(CParsedGroupBtn)
	ON_WM_ENABLE			()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()                  

//-----------------------------------------------------------------------------
CParsedGroupBtn::CParsedGroupBtn()
	:
	CGroupBoxBtn		(),
	m_bManualChecking	(TRUE)
{
	CParsedCtrl::Attach(this);
	CParsedCtrl::AttachCustomFont(this);
}
                        
//-----------------------------------------------------------------------------
BOOL CParsedGroupBtn::Create(DWORD dwStyle, const RECT& rect, CWnd* pParentWnd, UINT nID)
{                 
	return
		CheckControl(nID, pParentWnd) &&
		__super::Create(_T(""), dwStyle, rect, pParentWnd, nID) &&
		CParsedCtrl::AttachCustomFont(this) &&
		InitCtrl();
}

//-----------------------------------------------------------------------------
BOOL CParsedGroupBtn::SubclassEdit(UINT nID, CWnd* pParentWnd, const CString& strName)
{
	BOOL bOk =
		CheckControl(nID, pParentWnd, _T("BUTTON"))	&&
		SubclassDlgItem(nID, pParentWnd)		&&
		InitCtrl();

	if (bOk)
		SetNamespace(strName);
	
	return bOk;
}

//-----------------------------------------------------------------------------
void CParsedGroupBtn::OnEnable(BOOL bEnable)
{
	CButton::OnEnable(bEnable);
	
	DoEnable(bEnable);

	CChildButton*	pChildButton;
	
	int numItems = m_ButtonAssociations.GetSize();
	for (int i = 0; i < numItems; i++)
	{
		pChildButton = (CChildButton*) m_ButtonAssociations[i];
		
		if (pChildButton)
			pChildButton->EnableWindow(bEnable);
	}
}

//-----------------------------------------------------------------------------
void CParsedGroupBtn::DoEnable(BOOL bEnable)
{
	if (UseEasyReading() && !bEnable)
	{
		CChildButton*	pChildButton;
		
		int numItems = m_ButtonAssociations.GetSize();
		for (int i = 0; i < numItems; i++)
		{
			pChildButton = (CChildButton*) m_ButtonAssociations[i];
			
			if (pChildButton)
				pChildButton->Invalidate();
		}
	}
	__super::DoEnable(bEnable);
}

//-----------------------------------------------------------------------------
//	Add an Association between a dataObj and a Button ID
CChildButton* CParsedGroupBtn::AddAssociation (UINT nID, DWORD dwEnum)
{          
	DataObj* pData = NULL;
	const EnumItemArray* pItems = NULL;

	ASSERT(HIWORD(dwEnum) == m_wTag);
		 
	switch (GetDataType().m_wType)
	{
		case DATA_ENUM_TYPE :
		{
			pItems = AfxGetEnumsTable()->GetEnumItems(m_wTag);
		
			if (pItems == NULL)
			{
				TRACE1("ENUM %d Undefined for CEnumButton\n", m_wTag);
				ASSERT(FALSE);
				
				return NULL;
			}
			break;
		}
		default :
			 ASSERT(FALSE);
			 return NULL;
	}
	
	CChildButton* pChildButton = new CChildButton(this, LOWORD(dwEnum), nID);
	pChildButton->AttachDocument(this->GetDocument());
	
	if (pChildButton->SubclassEdit(nID, GetParent()))
	{
		// check if it an acceptable button
		UINT nBtnStyle = pChildButton->GetButtonStyle();
		
		ASSERT	(
					(nBtnStyle & BS_AUTOCHECKBOX)	== BS_AUTOCHECKBOX		||
					(nBtnStyle & BS_AUTORADIOBUTTON)== BS_AUTORADIOBUTTON
				);

		m_ButtonAssociations.Add(pChildButton);

		pChildButton->SetWindowText(pItems->GetTitle(LOWORD(dwEnum)));

		return pChildButton;
	}

	if (pChildButton)
		delete pChildButton;
		
	return NULL;
}

//-----------------------------------------------------------------------------
CChildButton* CParsedGroupBtn::GetAssociatedButton (UINT nID)
{
	for (int i = 0; i < m_ButtonAssociations.GetSize(); i++)
	{
		CChildButton* pB = (CChildButton*) m_ButtonAssociations.GetAt(i);
		if (pB->m_nID == nID)
			return pB;
	}
	return NULL;
}

//-----------------------------------------------------------------------------
CWnd* CParsedGroupBtn::SetCtrlFocus(BOOL /* = FALSE, here ignored */)
{
	CChildButton* pFirstButton = NULL;
	
	if (m_ButtonAssociations.GetSize() > 0)
	{
		// get first button association
		pFirstButton = (CChildButton*) m_ButtonAssociations[0];
	}
	
	if (pFirstButton)
		pFirstButton->SetFocus();

	return pFirstButton;
}

//-----------------------------------------------------------------------------
void CParsedGroupBtn::SetCtrlFont(CFont* pFont, BOOL bRedraw /* TRUE */)
{
	CChildButton*	pChildButton;
	
	int numItems = m_ButtonAssociations.GetSize();
	for (int i = 0; i < numItems; i++)
	{
		pChildButton = (CChildButton*) m_ButtonAssociations[i];
		
		if (pChildButton)
			pChildButton->SetFont(pFont, bRedraw);
	}
}

//-----------------------------------------------------------------------------
BOOL CParsedGroupBtn::SetCtrlPos(const CWnd*, int, int, int, int, UINT)
{
	// do nothing because can't move each buttons
	//
	return FALSE;
}

//-----------------------------------------------------------------------------
BOOL CParsedGroupBtn::ShowCtrl(int nCmdShow)
{
	CChildButton*	pChildButton;
	BOOL			rc = TRUE;
	
	int numItems = m_ButtonAssociations.GetSize();
	for (int i = 0; i < numItems; i++)
	{
		pChildButton = (CChildButton*) m_ButtonAssociations[i];
		
		if (pChildButton)
			rc = pChildButton->ShowWindow(nCmdShow) && rc;
	}
	
	return rc;
}

//-----------------------------------------------------------------------------
void CParsedGroupBtn::DoKillFocus(CWnd* pWnd)
{
	int numItems = m_ButtonAssociations.GetSize();
	for (int i = 0; i < numItems; i++)
		if (pWnd == m_ButtonAssociations[i])
			return;
			
	CParsedCtrl::DoKillFocus(pWnd);
}

//=============================================================================
//			Class CEnumButton implementation
//=============================================================================
IMPLEMENT_DYNCREATE (CEnumButton, CParsedGroupBtn)

BEGIN_MESSAGE_MAP(CEnumButton, CParsedGroupBtn)
	//{{AFX_MSG_MAP(CEnumButton)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()    

//-----------------------------------------------------------------------------
CEnumButton::CEnumButton()
	:
	CParsedGroupBtn()
{}
                        
//-----------------------------------------------------------------------------
CEnumButton::CEnumButton(DataEnum* pData)
	:
	CParsedGroupBtn()
{
	Attach(pData);
}
                        
//-----------------------------------------------------------------------------
void CEnumButton::Attach(DataObj* pDataObj)
{
	if (pDataObj)
	{
		ASSERT(pDataObj->GetDataType().m_wType == GetDataType().m_wType);
		SetTagValue(((DataEnum*)pDataObj)->GetTagValue());
	}
	CParsedCtrl::Attach(pDataObj);
}

//-----------------------------------------------------------------------------
void CEnumButton::SetValue(const DataObj& aValue)
{
	ASSERT(CheckDataObjType(&aValue));
	ASSERT(aValue.IsKindOf(RUNTIME_CLASS(DataEnum)));

	m_bManualChecking = FALSE;

	DataEnum& aDataEnum = (DataEnum&) aValue;
	ASSERT(aDataEnum.GetTagValue() == m_wTag);

	m_pData->Assign(aValue);
	
	int numItems = m_ButtonAssociations.GetSize();
	for (int i = 0; i < numItems; i++)
	{
		CChildButton* pChildButton = (CChildButton*) m_ButtonAssociations[i];
		ASSERT(pChildButton);

		int nCheck = aDataEnum.IsEqual(DataEnum(m_wTag, pChildButton->m_wEnumItem)) ? 1 : 0;

		pChildButton->SetCheck(nCheck);
	}

	m_bManualChecking = TRUE;
}

//-----------------------------------------------------------------------------
void CEnumButton::GetValue(DataObj& aValue)
{
	ASSERT(CheckDataObjType(&aValue));
	
	DataEnum& aDataEnum = (DataEnum&) aValue;
	ASSERT(aDataEnum.GetTagValue() == m_wTag);
	
	int numItems = m_ButtonAssociations.GetSize();
	for (int i = 0; i < numItems; i++)
	{
		CChildButton* pChildButton = (CChildButton*) m_ButtonAssociations[i];
		ASSERT(pChildButton);

		if (pChildButton->GetCheck())
		{
			aValue.Assign(DataEnum(m_wTag, pChildButton->m_wEnumItem));
			return;
		}
	}
}

//=============================================================================
