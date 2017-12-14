
#pragma once

#include <TbWoormViewer\woormvw.h>

//includere alla fine degli include del .H
#include "beginh.dex"

class CWrmRadarDoc;
class CWrmRadarFrame;
// Definizione di CWrmRadarView
//=============================================================================
class TB_EXPORT CWrmRadarView : public CWoormView
{
	DECLARE_DYNCREATE(CWrmRadarView)
	DECLARE_MESSAGE_MAP()

public:
	CWrmRadarView	();
	virtual ~CWrmRadarView	();

public:
	CWrmRadarDoc*		GetDocument		()	{ ASSERT (m_pDocument); return (CWrmRadarDoc*) m_pDocument; }
	CWrmRadarFrame*	GetWrmRadarFrame	()	{ CWrmRadarFrame *pFrame = (CWrmRadarFrame*) GetParentFrame(); ASSERT (pFrame); return pFrame; }

protected:
	virtual void OnLinkSelected		();

	// Generated message map functions
	//{{AFX_MSG(CWrmRadarView)
	afx_msg void OnLButtonDblClk(UINT nFlags, CPoint point);
	afx_msg void OnLButtonDown(UINT nFlags, CPoint point);
	afx_msg	void OnVKUp();
	afx_msg	void OnVKDown();

	BOOL OnMouseWheel(UINT nFlags, short zDelta, CPoint pt);

// Diagnostics
#ifdef _DEBUG
public:
	void Dump		(CDumpContext&)	const;
	void AssertValid()				const { CWoormView::AssertValid(); }
#endif // _DEBUG
};

#include "endh.dex"
