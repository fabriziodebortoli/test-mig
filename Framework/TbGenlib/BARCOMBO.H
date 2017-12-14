
#pragma once

#include <TbGenlib\TBToolBar.h>
//includere alla fine degli include del .H
#include "beginh.dex"

//	Classi definite all'interno del modulo
//=============================================================================
class CBarComboBox;

class CToolBarWithControl;
	class CToolBarWithCombo;

class CWndObjDescriptionContainer;
class CWndObjDescription;

//	Definizione di CBarComboBox
//=============================================================================
class TB_EXPORT CBarComboBox : public CBCGPComboBox
{
protected:
	UINT m_nID;

public:
	CBarComboBox() { m_nID = 0; }
	BOOL Create	(const RECT& rect, CWnd* pParentWnd, UINT nID);

	void GetCurSelText (CString&) const;

protected:
	// Generated message map functions
	//{{AFX_MSG(CBarComboBox)
		afx_msg void OnKeyDown (UINT, UINT, UINT);
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

// Diagnostics
#ifdef _DEBUG
public:
	void Dump		(CDumpContext&)	const;
	void AssertValid()				const { CBCGPComboBox::AssertValid(); }
#endif // _DEBUG
};

///////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////
//
// Funzioni Inline
//
///////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
inline void CBarComboBox::GetCurSelText (CString& str) const
	{ GetLBText(GetCurSel (), str); }

#include "endh.dex"
