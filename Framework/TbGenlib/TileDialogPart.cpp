#include "stdafx.h"

#include "BaseTileDialog.h"
#include "parsbtn.h"

#include "TileDialogPart.h"

#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

//////////////////////////////////////////////////////////////////////////////
//							CTileDialogPart								//
//////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CTileDialogPart, CObject)

//------------------------------------------------------------------------------
CTileDialogPart::CTileDialogPart
	(
		CBaseTileDialog*	pParent,
		CRect&				rectStatic
	)
	:
	m_rectOriginal		(0,0,0,0),
	m_rectActual		(0,0,0,0),
	m_rectStatic		(rectStatic),
	m_pParent			(pParent),
	m_RightmostColumn	(0),
	m_BottomLine		(0),
	m_nAutoMinWidth		(0),
	m_nAutoMaxWidth		(0)
{
}

//------------------------------------------------------------------------------
CTileDialogPart::~CTileDialogPart()
{
}

//------------------------------------------------------------------------------
void CTileDialogPart::SetOriginalRect(CRect &rectAvail, CTileDialogPart* pPartAfter /*= NULL*/)
{
	m_rectOriginal.left		= m_rectStatic.left;
	m_rectOriginal.right	= pPartAfter ? pPartAfter->m_rectStatic.left - 1 : rectAvail.right;
	m_rectOriginal.top		= m_rectStatic.top;
	m_rectOriginal.bottom	= m_rectStatic.bottom;

	m_rectActual = m_rectOriginal;
}

//------------------------------------------------------------------------------
void CTileDialogPart::SetStaticAreaExtent(int nLeft, int nRight)
{
	m_rectStatic.right	= nRight; 
	m_rectStatic.left	= nLeft;
}

//------------------------------------------------------------------------------
void CTileDialogPart::ResizeStaticAreaHeight(int nHeight)
{
	m_rectStatic.bottom = m_rectStatic.top + nHeight;
	m_rectActual.bottom = m_rectActual.top + nHeight;
}

//-----------------------------------------------------------------------------
BOOL CTileDialogPart::IsInStaticArea(const CRect& rectCtrl)
{
	return CRect().IntersectRect(m_rectStatic, rectCtrl);
}

//-----------------------------------------------------------------------------
BOOL CTileDialogPart::Contains(const CRect& rectCtrl)
{
	return CRect().IntersectRect(m_rectActual, rectCtrl);
}

//-----------------------------------------------------------------------------
BOOL CTileDialogPart::Contains(CWnd* pWnd)
{
	for (int c = 0; c <= m_Controls.GetUpperBound(); c++)
	{
		if (m_Controls.GetAt(c) == pWnd->m_hWnd)
			return TRUE;
	}
	
	return FALSE;
}

//-----------------------------------------------------------------------------
CRect CTileDialogPart::Intersect(const CRect& rectCtrl)
{
	CRect rect;
	rect.IntersectRect(m_rectActual, rectCtrl);
	return rect;
}
 

//------------------------------------------------------------------------------
void CTileDialogPart::AddControl(CWnd* pCtrl, const CRect& rectCtrl, BOOL bAdd /*TRUE*/)
{
	if (bAdd)
		m_Controls.Add(pCtrl->GetSafeHwnd());
	// look for controls in the anchor area
	// if anchor size is not specified in the theme, anchor is not managed
	int anchorSize = AfxGetThemeManager()->GetTileAnchorSize();
	if (
		m_pParent->IsStretchableControl(pCtrl) &&  //anchor  only edit, combo and boolbutton
		anchorSize != 0 &&
		rectCtrl.right >= m_rectActual.right - anchorSize
		)
	{
		if (bAdd)
			m_Anchored.Add(pCtrl->GetSafeHwnd());
	}
	else
		// keep track of the rightmost non-anchored control
		m_RightmostColumn = max(m_RightmostColumn, rectCtrl.right);

	// keeps track of the lowest non-hidden control
	if (rectCtrl.bottom > m_BottomLine)
		m_BottomLine = rectCtrl.bottom;
}

//-----------------------------------------------------------------------------
void CTileDialogPart::SetSize()
{
	// tile part height is initially equal to those of its parent tile
	int nOriginalHeight = m_pParent->m_rectOriginal.Height();

	// In case of AUTO or FREE MinHeight adjusts height so that it includes the lowest non-hidden controls
	// Usually controls hidden due to localization or configuration are placed at the bottom
	if (m_pParent->m_nMinHeight == AUTO || m_pParent->m_nMinHeight == FREE)
	{
		int bottomPadding = m_pParent->GetTileStyle()->GetAspect() == TileStyle::EDGE ? 4 : 2; // some bottom margin
		nOriginalHeight = min(nOriginalHeight, m_BottomLine + m_pParent->m_nTitleHeight + bottomPadding); 
	}

	m_rectStatic	.bottom	= m_rectStatic.top		+ nOriginalHeight; 
	m_rectOriginal	.bottom	= m_rectOriginal.top	+ nOriginalHeight;
	
	// lower the top of the part's area by the title height
	m_rectStatic	.top += m_pParent->m_nTitleHeight;
	m_rectOriginal	.top += m_pParent->m_nTitleHeight;

	//@@@TODO vale la pena di avere un parametro per la flessibilita` di resize?
	m_nAutoMinWidth = max((int)(m_rectOriginal.Width() * 0.8), m_RightmostColumn - m_rectOriginal.left);
	m_nAutoMaxWidth = (int)(m_rectOriginal.Width() * 1.2);

	m_rectActual = m_rectOriginal;
}

//-----------------------------------------------------------------------------
int CTileDialogPart::ResizeStaticAreaWidth(int nOffset, int nNewWidth)
{
	int delta = nNewWidth - m_rectStatic.Width();

	TBThemeManager* pThemeManager = AfxGetThemeManager();
	int leftPadding = m_pParent->GetTileStyle()->GetAspect() == TileStyle::EDGE ? pThemeManager->GetTileStaticAreaInnerLeftPadding() + 1 : pThemeManager->GetTileStaticAreaInnerLeftPadding();
	int rightPadding = pThemeManager->GetTileStaticAreaInnerRightPadding();

	m_rectStatic.OffsetRect(nOffset, 0);
	m_rectActual.OffsetRect(nOffset, 0);
	m_rectOriginal.OffsetRect(nOffset, 0);
	
	for (int c = 0; c <= m_Controls.GetUpperBound(); c++)
	{
		CWnd* pCtrl = CWnd::FromHandle(m_Controls[c]);
		if (!::IsWindow(pCtrl->m_hWnd))
			continue;

		CRect r;

		pCtrl->GetWindowRect(&r);
		m_pParent->ScreenToClient(&r);

		r.OffsetRect(nOffset, 0);

		if (!IsInStaticArea(r))
			// just shifted
			pCtrl->MoveWindow(r.left + delta + rightPadding + pThemeManager->GetTileInnerLeftPadding(), r.top, r.Width(), r.Height());
		else
		{
			// shifted and resized
			if (!pCtrl->IsKindOf(RUNTIME_CLASS(CBoolButton)))
				pCtrl->MoveWindow(m_rectActual.left + leftPadding, r.top, r.Width() + (r.left - (m_rectActual.left + leftPadding)) + delta - leftPadding, r.Height());
			else
			{
				CBoolButton* pBoolButton = (CBoolButton*)pCtrl;
				pBoolButton->RecalculatePaintInfo();
				pCtrl->MoveWindow(m_rectActual.left + leftPadding, r.top, r.Width() + (r.left - (m_rectActual.left + leftPadding)) + delta - leftPadding + rightPadding, r.Height());
				pCtrl->ModifyStyle(WS_BORDER, NULL);
			}
		}
	}

	SetStaticAreaExtent(m_rectActual.left, m_rectActual.left + nNewWidth + rightPadding); 

	m_nAutoMinWidth			+= delta + rightPadding;
	m_nAutoMaxWidth			+= delta + rightPadding;

	return delta + rightPadding;
}

//-----------------------------------------------------------------------------
void CTileDialogPart::ResizeRightmostControls(const CRect& rectNew)
{
	if (m_Anchored.GetSize() == 0)
		return; // no controls anchored on the right

	int nNewWidth = min(max(rectNew.Width(), GetMinWidth()), GetMaxWidth());
	
	if (m_rectActual.Width() == nNewWidth)
		return; // same width, no bothering to resize

	for (int c = 0; c <= m_Anchored.GetUpperBound(); c++)
	{
		CWnd* pCtrl = CWnd::FromHandle(m_Anchored.GetAt(c));
		if (!pCtrl || !::IsWindow(pCtrl->m_hWnd))
			continue;

		CRect rectCtrl;
		pCtrl->GetWindowRect(rectCtrl);
		
		m_pParent->ScreenToClient(rectCtrl);

		// IMPORTANT! at this moment, the control is already moved in its new position inside rectNew
		int ctrlWidth = rectNew.left + nNewWidth - AfxGetThemeManager()->GetTileRightPadding() - rectCtrl.left;

		pCtrl->SetWindowPos(NULL, -1, -1, ctrlWidth, rectCtrl.Height(), SWP_NOMOVE | SWP_NOZORDER);
		if (pCtrl->IsKindOf(RUNTIME_CLASS(CBoolButton)))
			pCtrl->ModifyStyle(WS_BORDER, NULL);

		//riposizionamento bottoni
		CParsedCtrl* pParsedCtrl = ::GetParsedCtrl(pCtrl);
		if (pParsedCtrl)
			pParsedCtrl->AdjustButtonsVisualization();
	}

}

//------------------------------------------------------------------------------
void CTileDialogPart::AdjustLocationNonDockedRightMostControls(const CRect& rectNew)
{
	for (int c = 0; c <= m_Controls.GetUpperBound(); c++)
	{
		CWnd* pCtrl = CWnd::FromHandle(m_Controls.GetAt(c));
		if (!pCtrl || !::IsWindow(pCtrl->m_hWnd))
			continue;

		if (pCtrl->IsKindOf(RUNTIME_CLASS(CLinkButton)) || pCtrl->IsKindOf(RUNTIME_CLASS(CStateButton)))
			continue;

		CRect rectCtrl;
		pCtrl->GetWindowRect(rectCtrl);
		m_pParent->ScreenToClient(rectCtrl);

		if (rectCtrl.right < rectNew.right)
			continue;
		BOOL bIsAnchored = FALSE;

		for (int a = 0; a <= m_Anchored.GetUpperBound(); a++)
		{
			HWND hwdAnchored = m_Anchored.GetAt(a);
			if (pCtrl->m_hWnd == hwdAnchored)
			{
				bIsAnchored = TRUE;
				break;
			}
		}

		if (!bIsAnchored)
		{
			rectCtrl.right = rectNew.right - 2;
			pCtrl->SetWindowPos(NULL, 0, 0, rectCtrl.Width(), rectCtrl.Height(), SWP_NOMOVE | SWP_NOACTIVATE | SWP_NOZORDER);
		}
	}
}

//------------------------------------------------------------------------------
int CTileDialogPart::GetMinWidth()
{ 
	switch (m_pParent->m_nMinWidth)
	{
		case ORIGINAL	: return m_rectOriginal.Width();
		case FREE		:
		case AUTO		: return m_nAutoMinWidth;
		default			: ASSERT_TRACE(FALSE,"Fixed width not managed on tiles with tile parts"); return m_nMinWidth;
	}
}

//------------------------------------------------------------------------------
int CTileDialogPart::GetMinHeight(CRect& rect /*= CRect(0, 0, 0, 0)*/)
{ 
	switch (m_pParent->m_nMinHeight)
	{
		case ORIGINAL	:
		case FREE		:
		case AUTO		: return m_rectOriginal.Height();
		default			: ASSERT_TRACE(FALSE,"Fixed height not managed on tiles with tile parts"); return m_nMinHeight; 
	}
}

//------------------------------------------------------------------------------
int CTileDialogPart::GetMaxWidth()
{ 
	switch (m_pParent->m_nMaxWidth)
	{
		case ORIGINAL	: return m_rectOriginal.Width();
		case FREE		: return INT_MAX;
		case AUTO		: return m_nAutoMaxWidth;
		default			: ASSERT_TRACE(FALSE,"Fixed width not managed on tiles with tile parts"); return m_nMaxWidth; 
	}
}

//------------------------------------------------------------------------------
int CTileDialogPart::GetRequiredHeight(CRect &rectAvail)
{
	return m_rectOriginal.Height();
}

//------------------------------------------------------------------------------
int CTileDialogPart::GetRequiredWidth(CRect &rectAvail)
{
	return m_rectOriginal.Width(); 
}

//------------------------------------------------------------------------------
void CTileDialogPart::GetAvailableRect(CRect &rectAvail)
{
	rectAvail = m_rectOriginal;
}

//------------------------------------------------------------------------------
void CTileDialogPart::Relayout(CRect& rectNew, HDWP hDWP /*= NULL*/)
{
	CDockableFrame* pFrame = dynamic_cast<CDockableFrame*>(m_pParent->GetParentFrame());
	if (pFrame && pFrame->IsLayoutSuspended())
		return;
	
	if (rectNew.Height() < GetMinHeight())
		rectNew.bottom = rectNew.top + GetMinHeight();

	if(rectNew.Width() < GetMinWidth())
		rectNew.right = rectNew.left + GetMinWidth();

	m_rectStatic.bottom = m_rectStatic.top + rectNew.Height();

	int displX = rectNew.left - m_rectActual.left;
	int displY = rectNew.top - m_rectActual.top;

	// position was not changed, no bother to move
	if (displX != 0 || displY != 0)
	{
		m_rectStatic.OffsetRect(displX, displY);

		for (int c = 0; c <= m_Controls.GetUpperBound(); c++)
		{
			CWnd* pCtrl = CWnd::FromHandle(m_Controls[c]);
			if (!::IsWindow(pCtrl->m_hWnd))
				continue;

			CRect r;
			pCtrl->GetWindowRect(&r);
			m_pParent->ScreenToClient(&r);

			pCtrl->SetWindowPos(NULL, r.left + displX,  r.top + displY, -1, -1, SWP_NOSIZE | SWP_NOZORDER);

			//riposizionamento bottoni (tolto corina)
			//AdjustStateControlsPosition(pCtrl, displX, displY);
		}
	}

	ResizeRightmostControls(rectNew);
	ResizeStaticAreaHeight(rectNew.Height());
	AdjustLocationNonDockedRightMostControls(rectNew);
	m_rectActual = rectNew;
	// avvisa EasyStudio che si e' rimosso tutto (un inseguimento di control continuo)
	m_pParent->SendMessage(UM_TILEPART_AFTER_RELAYOUT, NULL, (LPARAM)this);
}

////------------------------------------------------------------------------------
//void CTileDialogPart::AdjustStateControlsPosition(CWnd* pCtrl, int displX, int displY)
//{
//	CParsedCtrl* pParsedCtrl = ::GetParsedCtrl(pCtrl);
//	if (!pParsedCtrl)
//		return;
//
//	CRect buttonRect;
//	for (int c = 0 ; c <= pParsedCtrl->GetStateCtrlsArray().GetUpperBound(); c++)
//	{
//		CStateCtrlObj* pStateCtrl = (CStateCtrlObj*) pParsedCtrl->GetStateCtrlsArray().GetAt(c);
//		CStateButton* pStateButton = pStateCtrl->GetButton();
//		if (!pStateCtrl || !pStateButton)
//			continue;
//		
//		pStateButton->GetWindowRect(&buttonRect); 	
//		m_pParent->ScreenToClient(buttonRect);
//
//		pStateButton->SetWindowPos(NULL, buttonRect.left + displX, buttonRect.top + displY, -1, -1, SWP_NOSIZE | SWP_NOZORDER);
//	}
//}



//------------------------------------------------------------------------------
void CTileDialogPart::GetUsedRect(CRect &rectUsed)
{
	rectUsed = m_rectActual;
}

//------------------------------------------------------------------------------
void CTileDialogPart::GetControlStructure(CWndObjDescriptionContainer* pContainer, CString sId)
{
	if (!pContainer || !m_pParent)
		return;

	
	CWndObjDescription*  pTilePartDesc = pContainer->GetWindowDescription(NULL, RUNTIME_CLASS(CWndObjDescription), sId);
	pTilePartDesc->m_Type = CWndObjDescription::TilePart;
	CRect r;
	GetUsedRect(r);
	m_pParent->ClientToScreen(r);
	pTilePartDesc->SetRect(r, FALSE);
	//Static Part description
	CWndColoredObjDescription*  pTileStaticDesc = (CWndColoredObjDescription*)pTilePartDesc->m_Children.GetWindowDescription(NULL, RUNTIME_CLASS(CWndColoredObjDescription), sId + _T("_static"));
	pTileStaticDesc->m_Type = CWndObjDescription::TilePartStatic;
	pTileStaticDesc->m_crBkgColor = m_pParent->GetStaticAreaColor();
	r = GetStaticAreaRect();
	m_pParent->ClientToScreen(r);
	pTileStaticDesc->SetRect(r, FALSE);
		//Content Part description
	CWndObjDescription*  pTileContentDesc = pTilePartDesc->m_Children.GetWindowDescription(NULL, RUNTIME_CLASS(CWndObjDescription), sId + _T("_content"));
	pTileContentDesc->m_Type = CWndObjDescription::TilePartContent;

	CRect oDiffRect;
	oDiffRect.SubtractRect(pTilePartDesc->GetRect(), pTileStaticDesc->GetRect());
	// TODO
	pTileContentDesc->SetRect(oDiffRect, FALSE);

	CString strRadioName;
	for (int iCount = 0; iCount < m_Controls.GetCount(); iCount++)
	{
		CWnd* pCtrl = CWnd::FromHandle(m_Controls[iCount]);
		if (!::IsWindow(pCtrl->m_hWnd))
			continue;

		CRect r;
		pCtrl->GetWindowRect(&r);
		m_pParent->ScreenToClient(&r);

		if (IsInStaticArea(r))
		{
			CWndObjDescription* pTmp = pTileStaticDesc->AddChildWindow(pCtrl);
			
			//Check RadioBtn: if the window is a radioBtn, add a name to the corresponding WndObjDescription to identify the radio group
			if (pTmp && pTmp->m_Type == CWndObjDescription::Radio && pTmp->IsKindOf(RUNTIME_CLASS(CWndCheckRadioDescription)))
			{

				// TODO: remove these lines below.
				DWORD uiStyle = pCtrl->GetStyle();
				DWORD uiMaskedStyle = uiStyle & WS_GROUP;
				if (uiMaskedStyle == 0)
				{
					int i = 0;
					i++;
				}
				if ((pCtrl->GetStyle() & WS_GROUP) == WS_GROUP)
				{
					strRadioName = cwsprintf(_T("radioGroup_%d"), pCtrl->m_hWnd);
					((CWndCheckRadioDescription*)pTmp)->m_strGroupName = strRadioName;
				}
				else
				{
					((CWndCheckRadioDescription*)pTmp)->m_strGroupName = strRadioName;
				}
			}
		}
		else
		{			
			CWndObjDescription* pTmp =  pTileContentDesc->AddChildWindow(pCtrl);

			//Check RadioBtn: if the window is a radioBtn, add a name to the corresponding WndObjDescription to identify the radio group
			if (pTmp && pTmp->m_Type == CWndObjDescription::Radio && pTmp->IsKindOf(RUNTIME_CLASS(CWndCheckRadioDescription)))
			{

				// TODO: remove these lines below.
				DWORD uiStyle = pCtrl->GetStyle();
				DWORD uiMaskedStyle = uiStyle & WS_GROUP;
				if (uiMaskedStyle == 0)
				{
					int i = 0;
					i++;
				}
				if ((pCtrl->GetStyle() & WS_GROUP) == WS_GROUP)
				{
					strRadioName = cwsprintf(_T("radioGroup_%d"), pCtrl->m_hWnd);
					((CWndCheckRadioDescription*)pTmp)->m_strGroupName = strRadioName;
				}
				else
				{
					((CWndCheckRadioDescription*)pTmp)->m_strGroupName = strRadioName;
				}
			}
		}
	}

}
