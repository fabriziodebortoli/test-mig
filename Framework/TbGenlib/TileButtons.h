#pragma once

#include <BCGCBPro\BCGPButton.h>

#include "beginh.dex"

class Tile;

/////////////////////////////////////////////////////////////////////////////
//					class CCollapseButton definition
/////////////////////////////////////////////////////////////////////////////
class TB_EXPORT CCollapseButton : public CBCGPButton
{
	DECLARE_DYNAMIC(CCollapseButton)

	HBITMAP m_hBmpCollapse;
	HBITMAP m_hBmpExpand;

	BOOL m_bCollapsed;

	CCollapseButton();
	~CCollapseButton();

public:
	void ChangeIcon	(BOOL bCollapsed);
	void SetColor(COLORREF crNewColor, BOOL bTransparent = TRUE);

	virtual BOOL Create(LPCTSTR caption, DWORD dwStyle, const RECT&  rect, Tile* pOwner, UINT  nID, BOOL bCollapsed);
	void DetachImages();
private:
	Tile*	m_pOwner;

protected:
	virtual void OnFillBackground(CDC* pDC, const CRect& rectClient);
	

protected:
	//{{AFX_MSG(CCollapseButton)
	afx_msg BOOL    OnClicked     ();
	afx_msg	LRESULT OnGetControlDescription(WPARAM wParam, LPARAM lParam);
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

/////////////////////////////////////////////////////////////////////////////
//					class CPinButton definition
/////////////////////////////////////////////////////////////////////////////
class CPinButton : public CBCGPButton
{
	DECLARE_DYNAMIC(CPinButton)

	HBITMAP m_hBmpPin;
	HBITMAP m_hBmpUnpin;

	BOOL m_bPinned;

	CPinButton();
	~CPinButton();

public:
	void ChangeIcon	(BOOL bPinned);
	void SetColor	(COLORREF crNewColor, BOOL bTransparent = FALSE);

	virtual BOOL Create(LPCTSTR caption, DWORD dwStyle, const RECT&  rect, Tile* pOwner, UINT  nID, BOOL bPinned);
	void DetachImages();

private:
	Tile*	m_pOwner;

protected:
	virtual void OnFillBackground(CDC* pDC, const CRect& rectClient);
	virtual void OnDraw(CDC* pDC, const CRect& rect, UINT uiState);
	virtual void DoDrawItem(CDC* pDCPaint, CRect rectClient, UINT itemState);

protected:
	//{{AFX_MSG(CPinButton)
	afx_msg BOOL    OnClicked     ();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

const int TILE_COLLAPSE_BUTTON_OFFSET = 2;
const int TILE_COLLAPSE_BUTTON_SIZE = 20;

#include "endh.dex"
