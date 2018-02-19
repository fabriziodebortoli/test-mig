#pragma once

//includere alla fine degli include del .H
#include "beginh.dex"
#include <afxstatusbar.h>

/////////////////////////////////////////////////////////////////////////////
// CTaskBuilderStatusBar window
class TB_EXPORT CTaskBuilderStatusBar : public CBCGPStatusBar
{
// Construction
public:
	CTaskBuilderStatusBar();

protected:
	BOOL m_bSuspendUpdateCmdUI = FALSE;
public:
	void SetSuspendUpdateCmdUI(BOOL bSuspend = TRUE) { m_bSuspendUpdateCmdUI = bSuspend; }
	BOOL IsSuspendedUpdateCmdUI() { return m_bSuspendUpdateCmdUI; }
	virtual void OnUpdateCmdUI(CFrameWnd* pTarget, BOOL bDisableIfNoHndler);

	// Operations
public:
	
	int GetPanesCount() const;
	void SetPaneWidth(int nIndex, int nWidth);
	BOOL AddPane(UINT nID, int nIndex );
	BOOL RemovePane(UINT nID);
	BOOL AddPaneControl(CWnd* pWnd, UINT nID, BOOL bAutoDestroy = TRUE);
	void DisableControl( int nIndex, BOOL bDisable=TRUE);
	void SetPaneInfo(int nIndex, UINT nID, UINT nStyle, int cxWidth);
	void SetPaneStyle(int nIndex, UINT nStyle);
	BOOL SetIndicators(const UINT* lpIDArray, int nIDCount);

	void SetPaneWithProges(BOOL bProgres = TRUE) { m_bPaneWithProges = bProgres; }
	void SetPaneWithProgesTextZone(int nWidth){ m_nTextwidthProges = nWidth; }

// Implementation
public:
	virtual ~CTaskBuilderStatusBar();
	virtual CSize CalcFixedLayout(BOOL bStretch, BOOL bHorz);

protected:
	BOOL m_bPaneWithProges;
	int  m_nTextwidthProges;

protected:
	virtual void OnDrawPane(CDC* pDC, CBCGStatusBarPaneInfo* pPane);

protected:

	struct _STATUSBAR_PANE_
	{
		_STATUSBAR_PANE_(){
			nID = cxText = nStyle = nFlags = 0;
		}
		
		UINT    nID;        // IDC of indicator: 0 => normal text area
		int     cxText;     // width of string area in pixels
		//   on both sides there is a 3 pixel gap and
		//   a one pixel border, making a pane 6 pixels wider
		UINT    nStyle;     // style flags (SBPS_*)
		UINT    nFlags;     // state flags (SBPF_*)
		CString strText;    // text in the pane
	};
	
	struct _STATUSBAR_PANE_CTRL_
	{
		CWnd* pWnd;
		UINT nID;
		BOOL bAutoDestroy;		
	};
	
	CArray < _STATUSBAR_PANE_CTRL_*, _STATUSBAR_PANE_CTRL_* > m_arrPaneControls; 
	
	_STATUSBAR_PANE_* GetPanePtr(int nIndex) const;
	
	BOOL PaneInfoGet(int nIndex, _STATUSBAR_PANE_* pPane);
	BOOL PaneInfoSet(int nIndex, _STATUSBAR_PANE_* pPane);
	
	void RepositionControls();
	
	// Generated message map functions
protected:
	DECLARE_MESSAGE_MAP()
	afx_msg BOOL OnEraseBkgnd(CDC* pDC);
	afx_msg void OnSize(UINT nType, int cx, int cy);

	virtual LRESULT WindowProc(UINT message, WPARAM wParam, LPARAM lParam);
};

#include "endh.dex"


