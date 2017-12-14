
#pragma once

#include <TbWoormViewer\woormdoc.h>
#include <TbWoormViewer\woormfrm.h>
#include <TbGes\extdoc.h>

//includere alla fine degli include del .H
#include "beginh.dex"

class CWrmRadarDoc;
class CWrmRadarView;

// Definizione della classe CWrmRadarFrame
//=============================================================================
class TB_EXPORT CWrmRadarFrame : public CWoormFrame
{
	DECLARE_DYNCREATE(CWrmRadarFrame)

public:
	CWrmRadarFrame();
	virtual ~CWrmRadarFrame();

public:
	CWrmRadarView*		GetWrmRadarView();
	CWrmRadarDoc*		GetWrmRadarDocument();

public:
   	void	UpdateButtonFixedCheck	(BOOL bFixedCheck);

	virtual BOOL	CreateTools(CWoormDocMng* = NULL);

protected:
	void OnActivateHandler(BOOL bActivate, CWnd* pActivateWnd);
	virtual BOOL PreCreateWindow(CREATESTRUCT& cs);
	virtual BOOL IsRadarFrame() const { return TRUE; }
	virtual BOOL Create(LPCTSTR lpszClassName, LPCTSTR lpszWindowName, DWORD dwStyle, const RECT& rect, CWnd* pParentWnd, LPCTSTR lpszMenuName, DWORD dwExStyle, CCreateContext* pContext);

	// Generated message map functions
	//{{AFX_MSG(CWrmRadarFrame)
	afx_msg void OnActivate(UINT nState, CWnd* pWndOther, BOOL bMinimized);
	afx_msg	void OnFixed		();
	afx_msg	void OnUpdateFixed	(CCmdUI*);
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()        

// Diagnostics
#ifdef _DEBUG
public:
	void Dump		(CDumpContext&)	const;
	void AssertValid()				const { CWoormFrame::AssertValid(); }
#endif // _DEBUG
};
#include "endh.dex"
