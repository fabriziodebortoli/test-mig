
#include "stdafx.h"

#include <TbGeneric\FontsTable.h>
#include <TbGeneric\globals.h>
#include <TbGeneric\WndObjDescription.h>

#include "TBToolBar.h"
#include "barcombo.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif

// Creazione del font della combo box
//-----------------------------------------------------------------------------
CFont* CreateToolBarFont ()
{
	LOGFONT logFont; 
	CFont* pFont = new CFont;

	if	(
			!AfxGetThemeManager()->GetFormFont()->GetObject(sizeof(LOGFONT), (LPTSTR) &logFont) ||
			!pFont->CreateFontIndirect(&logFont)
		)
	{
		TRACE0("BarCombo-CreateToolBarFont\n\tFailed to create the ComboBox font, so it will use the system font.\n");
		pFont->FromHandle((HFONT)::GetStockObject(SYSTEM_FONT));
	}
	return pFont;
}

///////////////////////////////////////////////////////////////////////////////
//	Implementazione di CBarComboBox
///////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////
BEGIN_MESSAGE_MAP(CBarComboBox, CBCGPComboBox)
	//{{AFX_MSG_MAP(CBarComboBox)
		ON_WM_KEYDOWN ()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
BOOL CBarComboBox::Create (const RECT& rect, CWnd* pParentWnd, UINT nID)
{
	m_nID = nID;
	return CBCGPComboBox::Create(CBS_DROPDOWNLIST|CBS_SORT|WS_VISIBLE|WS_VSCROLL|WS_CHILD|WS_CLIPSIBLINGS|WS_BORDER, rect, pParentWnd, nID);
}

// Acceleratori per le funzionalità della combo
//-----------------------------------------------------------------------------
void CBarComboBox::OnKeyDown(UINT nChar, UINT nRepCnt, UINT nFlags)
{
	switch (nChar)
	{
		case VK_ESCAPE	:
			GetParent ()->SEND_WM_COMMAND(m_nID, CBN_SELENDCANCEL, m_hWnd);
			ShowDropDown(FALSE);
			return;
		case VK_RETURN	:
		case VK_SPACE	:
			GetParent ()->SEND_WM_COMMAND(m_nID, CBN_SELENDOK, m_hWnd);
			ShowDropDown(FALSE);
			return;

		default : CBCGPComboBox::OnKeyDown (nChar, nRepCnt, nFlags);
	}
}

///////////////////////////////////////////////////////////////////////////////
// Diagnostics
#ifdef _DEBUG
void CBarComboBox::Dump (CDumpContext& dc) const
{
	ASSERT_VALID (this);
	AFX_DUMP0(dc, " CBarComboBox\n");
	CBCGPComboBox::Dump(dc);
}
#endif // _DEBUG
