
#pragma once

#include <TbGes\extdoc.h>
#include "radardef.h"

//includere alla fine degli include del .H
#include "beginh.dex"

class SqlTable;

class CToolBarWithCombo;
class CRadarDoc;
class CRadarView;
class RadarBody;
class CRadarTitleBar;

// Classi definite all'interno del modulo
//=============================================================================
class CRadarFrame;
#define NUMBER_CURSORS	6

// Definizione della classe CRadarFrame
//=============================================================================
class TB_EXPORT CRadarFrame : public CAbstractFrame
{
	DECLARE_DYNCREATE(CRadarFrame)
private:
	HCURSOR	m_rghCursors [NUMBER_CURSORS];

	int				m_nCurrentMsg;
	CString			m_strStatusFormat;
	CStringArray*	m_pMsgStatusBar;
protected:
	CTaskBuilderStatusBar  m_wndStatusBar;
	
	//CTBTabbedToolbar*	m_pWndTabbedToolbar;
	CTBToolBar*			m_pWndMainToolBar;
	CTBToolBar*			m_pWndExportToolBar;
	CTBToolBar*			m_pWndGrpToolBar;

	CRadarTitleBar*	m_pWndTitleBar;

public:
	BOOL	m_bIsClosing;

	CRadarFrame	();
	~CRadarFrame();

public:
	void CloseRadar ();

public:
	CRadarTitleBar*		GetTitleBar	()			{ ASSERT (m_pWndTitleBar);	return m_pWndTitleBar;}
	virtual CTBTabbedToolbar* GetTabbedToolBar() { return m_pTabbedToolBar; }
	
	virtual BOOL OnPopulatedDropDown    (UINT nIdCommand);
	
	CTBToolBar* GetQueryBar	()	{ ASSERT(m_pWndMainToolBar); return m_pWndMainToolBar;}

	CRadarView*		GetRadarView();
	CRadarDoc*		GetRadarDocument();

	// Visualizzano gli stati attuali del radar nella status bar
	void SetCursorAndMsgStatusBar	(SelectCursor = ARROW);
	void SetRunIndicator			();
   	void SetFixedCheck				();

	void AttachRadarBodyToToolBar (RadarBody*);

	// Controllano la combo box degli stili del carattere.
	void FillFontStyleComboBox (int nIdxCurrentFontStyle);

	void UpdateButtonEasyReading (BOOL bEasyReading);
	LPTSTR GetMenuCaption(UINT id, CString& strBuffer);

	virtual void AdjustTabbedToolBar();
	void RemoveTabInToolbar(LPCTSTR lpszText);
	
protected:
	void ChangeBtnLinkRadar (BOOL bEnable);
	void LoadResources		();
	virtual BOOL LoadFrame(UINT nIDResource, DWORD dwDefaultStyle = WS_OVERLAPPEDWINDOW | FWS_ADDTOTITLE, CWnd* pParentWnd = NULL, CCreateContext* pContext = NULL);
	virtual BOOL PreTranslateMessage(MSG*);
	virtual	void OnFrameCreated();
	virtual void OnUpdateFrameTitle	(BOOL bAddToTitle);
	virtual BOOL Create(LPCTSTR lpszClassName, LPCTSTR lpszWindowName, DWORD dwStyle = WS_OVERLAPPEDWINDOW, const RECT& rect = rectDefault, CWnd* pParentWnd = NULL, LPCTSTR lpszMenuName = NULL, DWORD dwExStyle = 0, CCreateContext* pContext = NULL);
	virtual BOOL PreCreateWindow(CREATESTRUCT& cs);
	// Generated message map functions
	//{{AFX_MSG(CRadarFrame)
	afx_msg void OnTBCBoxFontStyleChanged	();
	afx_msg void OnTBCBoxFontStyleCancel	() { SetFocus(); }

	afx_msg	int  OnCreate		(LPCREATESTRUCT);
	afx_msg BOOL OnSetCursor	(CWnd*, UINT, UINT);
	afx_msg void OnActivate(UINT nState, CWnd* pWndOther, BOOL bMinimized);
	afx_msg	void OnTimer		(UINT nUI);
	afx_msg	void OnDropDown		();

	afx_msg	void OnFixed			();
	afx_msg	void OnLinkRadar		();

	afx_msg	void OnUpdateLinkRadar	(CCmdUI*);
	afx_msg	void OnUpdateFixed		(CCmdUI*);
	afx_msg	void OnUpdatePaneText	(CCmdUI*);
	
	afx_msg LRESULT OnFontStyleChanged	(WPARAM, LPARAM);
	afx_msg LRESULT OnFormatStyleChanged(WPARAM, LPARAM);
	afx_msg LRESULT OnIsUnattendedWindow(WPARAM wParam, LPARAM lParam);

	afx_msg LRESULT OnFillAutoComple(WPARAM wp, LPARAM lp);
	afx_msg void OnGetMinMaxInfo(MINMAXINFO* lpMMI);

	afx_msg void OnSize(UINT nType, int cx, int cy);

	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()        

// Diagnostics
#ifdef _DEBUG
public:
	void Dump		(CDumpContext&)	const;
	void AssertValid()				const { CAbstractFrame::AssertValid(); }
#endif // _DEBUG
};

#include "endh.dex"
