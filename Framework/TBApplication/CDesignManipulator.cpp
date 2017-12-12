#include "stdafx.h"

#include "CDesignManipulator.h"

#ifdef _DEBUG
#undef THIS_FILE
static const char  THIS_FILE[] = __FILE__;
#endif


//////////////////////////////////////////////////////////////////////////////////	
//								CDesignModeLayoutManipolator class implementation
//////////////////////////////////////////////////////////////////////////////////	
CDesignModeLayoutManipulator::CDesignModeLayoutManipulator(CBaseDocument::DesignMode aDesignMode)
	:
	m_DesignMode(aDesignMode),
	m_pGroupBrush(NULL)
{
}

//-----------------------------------------------------------------------------
CDesignModeLayoutManipulator::~CDesignModeLayoutManipulator()
{
	if (m_pGroupBrush)
	{
		m_pGroupBrush->DeleteObject();
		delete m_pGroupBrush;
	}
}

//-----------------------------------------------------------------------------
CBaseDocument::DesignMode CDesignModeLayoutManipulator::GetDesignMode()
{
	return m_DesignMode;
}

//-----------------------------------------------------------------------------
void CDesignModeLayoutManipulator::OnAfterBuildDataControlLinks(CAbstractFormView* pView)
{
	if (pView->GetLayoutContainer())
		ChangeLayout(pView->GetLayoutContainer()->GetContainedElements());
}

//-----------------------------------------------------------------------------
void CDesignModeLayoutManipulator::OnAfterBuildDataControlLinks(CTabDialog* pTabDialog)
{
	if (pTabDialog->GetChildTileGroup())
	{
		if (!pTabDialog->GetChildTileGroup()->GetLayoutContainer())
			return;

		CDockableFrame* pFrame = dynamic_cast<CDockableFrame*>(pTabDialog->GetParentFrame());
		if (pFrame)
			pFrame->SuspendLayout();
		ChangeLayout(pTabDialog->GetChildTileGroup()->GetLayoutContainer()->GetContainedElements());
		if (pFrame)
			pFrame->ResumeLayout();
	}
}

//-----------------------------------------------------------------------------
BOOL CDesignModeLayoutManipulator::AreZeroSizesAllowed()
{
	return FALSE;
}

//-----------------------------------------------------------------------------
void CDesignModeLayoutManipulator::ChangeLayout(const LayoutElementArray* pElements)
{

	for (int i = 0; i < pElements->GetCount(); i++)
	{
		LayoutElement* pElement = pElements->GetAt(i);
		if (!pElement)
			continue;

		CLayoutContainer* pAsContainer = dynamic_cast<CLayoutContainer*>(pElement->GetParentElement());
		if (pAsContainer)
		{
			pAsContainer->SetLayoutAlign(CLayoutContainer::NO_ALIGN);
			pAsContainer->SetLayoutType(CLayoutContainer::VBOX);
		}

		pElement->SetFlex(0);
		Tile* pTile = pElement->AsATile();
		if (pTile)
			pTile->SetCollapsed(FALSE);

		if (pElement->GetContainedElements())
			ChangeLayout(pElement->GetContainedElements());
	}
}

//-----------------------------------------------------------------------------
int	CDesignModeLayoutManipulator::GetTileMaxHeightUnit() const
{
	int n = AfxGetThemeManager()->GetTileMaxHeightUnit();
	return n;
}
//-----------------------------------------------------------------------------
int	CDesignModeLayoutManipulator::GetTileAnchorSize() const
{
	return 0;
}

//-----------------------------------------------------------------------------
BOOL CDesignModeLayoutManipulator::IsInternalControlMovementEnabled()
{
	return FALSE;
}

//-----------------------------------------------------------------------------
BOOL CDesignModeLayoutManipulator::FreeResize()
{
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CDesignModeLayoutManipulator::HasToolbar()
{
	return FALSE;
}

//-----------------------------------------------------------------------------
BOOL CDesignModeLayoutManipulator::HasStatusBar() const
{
	return FALSE;
}

// da spostare nel tema alla prima occasione
//-----------------------------------------------------------------------------
BOOL CDesignModeLayoutManipulator::IsSameColor(COLORREF clr1, COLORREF clr2)
{
	return	GetRValue(clr1) == GetRValue(clr2) &&
		GetGValue(clr1) == GetGValue(clr2) &&
		GetBValue(clr1) == GetBValue(clr2);
}
//-----------------------------------------------------------------------------
CBrush* CDesignModeLayoutManipulator::GetTileGroupBkgColorBrush()
{
	if (GetDesignMode() == CBaseDocument::DM_RUNTIME)
	{
		COLORREF clrGroup = AfxGetThemeManager()->GetTileGroupBkgColor();
		COLORREF clrTitle = AfxGetThemeManager()->GetTileDialogTitleForeColor();

		// mi evito lo stesso colore
		if (IsSameColor(clrGroup, clrTitle))
			return GetBrushOf(AfxGetThemeManager()->GetTabSelectorHoveringForeColor());

		return GetBrushOf(AfxGetThemeManager()->GetTileDialogTitleForeColor());
	}

	return GetBrushOf(AfxGetThemeManager()->GetTileGroupBkgColor());
}

//-----------------------------------------------------------------------------
CBrush*	CDesignModeLayoutManipulator::GetBrushOf(COLORREF color)
{
	if (IsSameColor(m_GroupColor, color) && m_pGroupBrush != NULL)
		return m_pGroupBrush;

	if (m_pGroupBrush)
	{
		m_pGroupBrush->DeleteObject();
		delete m_pGroupBrush;
	}
	m_GroupColor = color;
	m_pGroupBrush = new CBrush(m_GroupColor);
	return m_pGroupBrush;
}

//////////////////////////////////////////////////////////////////////////////////	
//								CRestartDocumentInvocationInfo class implementation
//////////////////////////////////////////////////////////////////////////////////	

//-----------------------------------------------------------------------------
CRestartDocumentInvocationInfo::CRestartDocumentInvocationInfo(CBaseDocument::DesignMode aDesignMode)
	:
	CManagedDocComponentObj()
{
	m_pManipulator = new CDesignModeLayoutManipulator(aDesignMode);
}

//-----------------------------------------------------------------------------
CRestartDocumentInvocationInfo::~CRestartDocumentInvocationInfo()
{
	SAFE_DELETE(m_pManipulator);
}

//-----------------------------------------------------------------------------
void CRestartDocumentInvocationInfo::CreateNewDocumentOf(CBaseDocument* pDoc)
{
	pDoc->SetDesignMode(((CDesignModeLayoutManipulator*)m_pManipulator)->GetDesignMode());
}
