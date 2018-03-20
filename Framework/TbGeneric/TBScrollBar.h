#pragma once
#include "beginh.dex"

///////////////////////////////////////////////////////////////////////////
class TB_EXPORT CTBScrollBar : public CBCGPScrollBar
{
	DECLARE_DYNCREATE(CTBScrollBar)

public:
	CTBScrollBar();
	~CTBScrollBar();

private:
	BOOL		m_bVisible;
	int			m_nWidth;
	COLORREF	m_BackGroundColor;
	COLORREF	m_BkgButtonNoPressedColor;
	COLORREF	m_BkgButtonPressedColor;
	BOOL		m_bThumbVisible;

public:
	BOOL isVisible();
	void SetVisible	(BOOL bVisible = TRUE);
	void SetHidden  (BOOL bHidden = TRUE);
	int GetWidth();
	int GetHeight();
	void SetBackGroundColor(COLORREF color) { m_BackGroundColor = color; }
	COLORREF GetBackGroundColor() { return m_BackGroundColor; }
	void SetBkgButtonNoPressedColor(COLORREF color) { m_BkgButtonNoPressedColor = color; }
	COLORREF GetBkgButtonNoPressedColor() { return m_BkgButtonNoPressedColor; }
	void SetBkgButtonPressedColor(COLORREF color) { m_BkgButtonPressedColor = color; }
	COLORREF GetBkgButtonPressedColor() { return m_BkgButtonPressedColor; }
	void SetThumbVisible(BOOL bSet) { m_bThumbVisible = bSet; }
	BOOL GetThumbVisible() { return m_bThumbVisible; }

protected:
	afx_msg void OnPaint();
	DECLARE_MESSAGE_MAP()
};