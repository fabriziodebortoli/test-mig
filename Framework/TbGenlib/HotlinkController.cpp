#include "StdAfx.h"

#include <TbNameSolver\MacroToRedifine.h>
#include <TbGeneric\TBThemeManager.h>

#include "basedoc.h"
#include "parsobj.h"
#include "parsedt.h"
#include "hlinkobj.h"
#include "HotlinkController.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

static int szHSpacing = 3;
static int szVSpacing = 2;
//=============================================================================
//	CTBHotlinkManagerDecode class implementation
//=============================================================================
//-----------------------------------------------------------------------------
CHotLinkControllerDecode::CHotLinkControllerDecode(DataObj* pDataObj, CHotLinkControllerDecode::DecodePosition pos)
	:
	m_pDataObj	(pDataObj),
	m_Pos		(pos),
	m_pControl	(NULL)
{
}

//-----------------------------------------------------------------------------
CRuntimeClass* CHotLinkControllerDecode::GetControlClass()
{
	return CParsedStatic::GetDefaultClassFor(m_pDataObj);
}

//-----------------------------------------------------------------------------
BOOL CHotLinkControllerDecode::CreateControl(CParsedForm* pParent, CBaseDocument* pDocument, const CString& sName, CRect aRect, DWORD dwStyle /*0*/)
{
	UINT nIDC = AfxGetTBResourcesMap()->GetTbResourceID(sName, TbControls);

	DWORD defaultStyle = WS_VISIBLE | WS_CHILD;
	TBThemeManager* pTheme = AfxGetThemeManager();
	if (pTheme->GetControlsUseBorders())
		defaultStyle |= (pTheme->UseFlatStyle() ? WS_BORDER : WS_EX_CLIENTEDGE | SS_SUNKEN);

	m_pControl = ::CreateControl
		(
			sName,
			defaultStyle | dwStyle,
			aRect, 
			pParent->GetFormCWnd(),
			pDocument,
			pParent->GetControlLinks(),
			nIDC,
			NULL,
			m_pDataObj,
			GetControlClass()
		);


	return m_pControl != NULL;
}

//-----------------------------------------------------------------------------
CParsedCtrl* CHotLinkControllerDecode::GetControl()
{
	return m_pControl;
}

//-----------------------------------------------------------------------------
void CHotLinkControllerDecode::RepositionControl (CRect& rectOwner, CRect& rectOriginalPlaceHolder, CRect rectButton, UINT nFlags)
{
	CWnd* pWnd = m_pControl->GetCtrlCWnd();
	if (!pWnd)
	{
		ASSERT(FALSE);
		return;
	}
	
	CDC* pDC = pWnd->GetDC();
	CFont* pFont = pWnd->GetFont();

	CPoint currLocation;
	CSize aSize = GetEditSize(pDC, pFont, m_pControl->GetCtrlMaxLen());
	
	int nButtonWidth = rectButton.Width();
	if (nButtonWidth > 0)
		nButtonWidth += szHSpacing; 

	switch (m_Pos)
	{
	case CHotLinkControllerDecode::Fixed:
		pWnd->ReleaseDC(pDC);
		return;

	case CHotLinkControllerDecode::Right:
		currLocation.x = rectOwner.right + nButtonWidth + szHSpacing;
		currLocation.y = rectOwner.top;
		break;
	case CHotLinkControllerDecode::Left:
		currLocation.x = rectOwner.left - szHSpacing - aSize.cx;
		currLocation.y = rectOwner.top;
		break;

	case CHotLinkControllerDecode::Bottom:
		currLocation.x = rectOwner.left;
		currLocation.y = rectOwner.bottom + szVSpacing;
		break;
	case CHotLinkControllerDecode::Top:
		currLocation.x = rectOwner.left;
		currLocation.y = rectOwner.top - szVSpacing - aSize.cy;
		break;
	}
	
	int nDifference = rectOriginalPlaceHolder.right - (currLocation.x + aSize.cx);
	if (nDifference < 0)
		aSize.cx -= (-nDifference);

	pWnd->SetWindowPos(NULL, currLocation.x, currLocation.y, aSize.cx, aSize.cy, nFlags);
	pWnd->ReleaseDC(pDC);
}

//=============================================================================
//	CHotLinkController class implementation
//=============================================================================

IMPLEMENT_DYNAMIC(CHotLinkController, CObject)

//-----------------------------------------------------------------------------
CHotLinkController::CHotLinkController(CParsedCtrl* pOwner)
	:
	m_pOwner(pOwner),
	m_bPlaceHolderShrinked(false)
{
	CWnd* pCodeWnd = m_pOwner ? m_pOwner->GetCtrlCWnd() : NULL;
	if (pCodeWnd) 
	{
		pCodeWnd->GetWindowRect(&m_OriginalPlaceHolder);	
		pCodeWnd->GetParent()->ScreenToClient(&m_OriginalPlaceHolder);
		ResizePlaceHolder(true);
	}
}

//-----------------------------------------------------------------------------
CHotLinkControllerDecode* CHotLinkController::Add
	(
		DataObj* pDataObj, 
		const CString& sName, 
		CHotLinkControllerDecode::DecodePosition pos,
		BOOL bUsePlaceHolder /*TRUE*/,
		int nChars /*-1*/ 
	)
{
	CWnd* pCodeWnd = m_pOwner->GetCtrlCWnd();
	CWnd* pParent = pCodeWnd ? pCodeWnd->GetParent() : NULL;
	if (!pParent)
	{
		ASSERT_TRACE(FALSE, "CHotLinkController::Add NULL parent window, cannot create decode control!");
		return NULL;
	}

	CParsedForm* pForm = pParent ? GetParsedForm(pParent) : NULL;

	if (!pForm)
	{
		ASSERT_TRACE(FALSE, "CHotLinkController::Add parent window is NOT a CParsedForm, cannot create decode control!");
		return NULL;
	}

	CHotLinkControllerDecode* pDecode = new CHotLinkControllerDecode(pDataObj, pos);
	CRect rect (1,1,1,1);
	
	if (!pForm || !pDecode->CreateControl(pForm, m_pOwner->GetDocument(), sName, rect))
	{
		delete pDecode;
		ASSERT_TRACE(FALSE, "CHotLinkController::Add cannot create control!");
		return NULL;
	}

	if (nChars > 0)
		pDecode->GetControl()->SetCtrlMaxLen(nChars, TRUE);
	
	m_Decodes.Add(pDecode);
	
	CRect aOwnerRect;
	pCodeWnd->GetWindowRect(aOwnerRect);
	pParent->ScreenToClient(aOwnerRect);
	pDecode->RepositionControl(aOwnerRect, m_OriginalPlaceHolder, GetButtonRect(), SWP_NOZORDER);

	return pDecode;
}

//-----------------------------------------------------------------------------
CRect CHotLinkController::GetButtonRect	()
{
	CRect aButtonRect;
	if (m_pOwner->GetButton())
	{
		m_pOwner->GetButton()->GetWindowRect(aButtonRect);
		m_pOwner->GetCtrlParent()->ScreenToClient(aButtonRect);
	}
	return aButtonRect;
}

//-----------------------------------------------------------------------------
void CHotLinkController::AddDefaultDecode(const CString& sDecodeNs)
{
	HotKeyLinkObj* pHotLink = GetHotLink();
	DataObj* pDataObj = pHotLink->GetDescriptionDataObj();
	if (pDataObj)
		Add(pDataObj, sDecodeNs);
	else
		ASSERT_TRACE(FALSE,"CHotLinkController::AddDefaultDecode() DataObj of default description field not found!");
}

//-----------------------------------------------------------------------------
HotKeyLinkObj* CHotLinkController::GetHotLink()
{
	if (!m_pOwner)
	{
		ASSERT_TRACE(FALSE,"CHotLinkController::GetHotLink() not dealing with a CParsedCtrl!");
		return NULL;
	}
	HotKeyLinkObj* pHotLink = m_pOwner->GetHotLink();
	if (!pHotLink)
		ASSERT_TRACE(FALSE,"CHotLinkController::AddDefaultDecode() CParsedCtrl without hotkeylink attached!");
	
	return pHotLink;
}

//-----------------------------------------------------------------------------
void CHotLinkController::ShowButton (const BOOL bShow /*TRUE*/)
{
	if (!m_pOwner || !m_pOwner->GetHotLink())
	{
		ASSERT_TRACE(FALSE,"CHotLinkController::ShowButton CParsedCtrl without hotlink!");
		return;
	}

	CButton* pButton = m_pOwner->GetButton();
	if (pButton)
		pButton->ShowWindow(bShow ? SW_SHOW : SW_HIDE); 
}

//-----------------------------------------------------------------------------
void CHotLinkController::OnAfterFindRecord	()
{
	for (int i=0; i < m_Decodes.GetSize(); i++)
	{
		CHotLinkControllerDecode* pDecode = (CHotLinkControllerDecode*) m_Decodes.GetAt(i);
		CParsedCtrl* pCtrl = pDecode->GetControl();
		if (pCtrl && pCtrl->GetCtrlCWnd() && pCtrl->GetCtrlCWnd()->IsWindowVisible())
		{
			pCtrl->UpdateCtrlData(TRUE);
			pCtrl->UpdateCtrlView();
		}
	}
}

//-----------------------------------------------------------------------------
void CHotLinkController::OnPrepareAuxData ()
{
	ASSERT(m_pOwner);
	CButton* pButton = m_pOwner->GetButton();
	CBaseDocument* pDoc = m_pOwner->GetDocument();

	if (pButton && pDoc)
		pButton->ShowWindow(pDoc->GetFormMode() == CBaseDocument::BROWSE ? SW_HIDE : SW_SHOW); 
}

//-----------------------------------------------------------------------------
void CHotLinkController::RepositionControls	(CRect& rectOwner, UINT nFlags)
{
	CRect aButtonRect = GetButtonRect();
	for (int i=0; i < m_Decodes.GetSize(); i++)
	{
		CHotLinkControllerDecode* pDecode =  (CHotLinkControllerDecode*) m_Decodes.GetAt(i);
		pDecode->RepositionControl(rectOwner, m_OriginalPlaceHolder, aButtonRect, nFlags);
	}
}

//-----------------------------------------------------------------------------
void CHotLinkController::DoCellPosChanging	(CRect& rectOwner, UINT nFlags)
{
	RepositionControls(rectOwner, nFlags);
}

//-----------------------------------------------------------------------------
void CHotLinkController::ResizePlaceHolder	(bool bShrink)
{
	CWnd* pCodeWnd = m_pOwner->GetCtrlCWnd();
	CWnd* pParent = pCodeWnd ? pCodeWnd->GetParent() : NULL;

	if (!pCodeWnd || !pParent)
	{
		ASSERT_TRACE(FALSE, _T("Place holder of hotlink code field not found or window not created!"));
		return;
	}

	CRect newRect(m_OriginalPlaceHolder);
	// calculate code spacing on the base of control length
	if (bShrink)
	{
		// if place holder is equal or smaller i don't touch it
		CSize aCodeSize = m_pOwner->AdaptNewSize	(m_pOwner->GetCtrlMaxLen(), 1, TRUE);
		if (aCodeSize.cx >  m_OriginalPlaceHolder.Width())
			return;
		
		newRect.right = newRect.left + aCodeSize.cx;
		newRect.bottom = newRect.top + aCodeSize.cy;
	}

	pCodeWnd->SetWindowPos(NULL, newRect.left, newRect.top, newRect.Width(), newRect.Height(), SWP_NOZORDER);
	//m_pOwner->DoCellPosChanging(newRect, SWP_NOZORDER);
}
