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
	BOOL	m_bVisible;
	int		m_nWidth;
	
public:
	BOOL isVisible();
	void SetVisible	(BOOL bVisible = TRUE);
	void SetHidden  (BOOL bHidden = TRUE);
	int GetWidth();
	int GetHeight();

protected:
	afx_msg void OnPaint();
	DECLARE_MESSAGE_MAP()
};