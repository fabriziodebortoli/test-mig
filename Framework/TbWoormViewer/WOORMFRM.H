
#pragma once

#ifndef __AFXEXT_H__
#include <afxext.h>
#endif                        

#include <TbGenlib\TBToolBar.h>
#include <TbGenlib\ParsCbx.h>
#include <TbGenlib\BaseFrm.h>
#include <TbGenlib\ExtStatusControlBar.h>

//includere alla fine degli include del .H
#include "beginh.dex"

#define NAME_MAIN_TOOLBAR		_T("Main")
#define NAME_MAIN_EDIT_TOOLBAR	_T("Editing")
#define NAME_SIZE_TOOLBAR		_T("Size")
#define NAME_BORDER_TOOLBAR		_T("Border")
#define NAME_OBJECT_TOOLBAR		_T("Object")
#define NAME_ALIGNMOVE_TOOLBAR	_T("Align move")

class CPaletteBar;
class CWoormDocMng;

class CRSDockPane;
	class CRSFullReportDockPane;
		class CRSLayoutDockPane;
		class CRSEngineDockPane;
	class CRSToolBoxDBDockPane;
	class CRSToolBoxDockPane;
	class CRSToolBarDockPane;
	class CRSObjectPropertyDockPane;
	class CRSEditorDockPane;

class CAbstractFormView;
	class CRSReportTreeView; 
		class CRSEngineTreeView;
		class CRSLayoutTreeView;
	class CRS_ObjectPropertyView;
	class CRSToolBoxObjectsView;
	class CRSToolBoxDBView;
	class CRSToolBarView;
	class CRSEditViewDocked;
class CRSEditView;
class CRSEditViewDebug;

//===========================================================================
class TB_EXPORT CMyStatusBar : public CTaskBuilderStatusBar
{                
protected:
	BOOL PreCreateWindow(CREATESTRUCT& cs);
	// Generated message map functions
	//{{AFX_MSG(CMyStatusBar)
	afx_msg void OnPaint();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

//===========================================================================
class TB_EXPORT CToolCbx: public CComboBoxExt
{                
public:
	BOOL m_bPassive;

	CToolCbx() : m_bPassive (FALSE) {}
};

//===========================================================================
class TB_EXPORT CToolCbxStr
{   
public:
	CStringArray m_arStringItemData;

	CToolCbxStr() {}
	virtual ~CToolCbxStr() { ResetContent(); }

	int AddEntry(const CString& sItemData);

	void ResetContent();
};

//===========================================================================
class TB_EXPORT CWoormFrame : public CAbstractWoormFrame
{
	DECLARE_DYNCREATE(CWoormFrame)
protected:
	CWoormDocMng*	m_pDoc = NULL;
	CPoint			m_ptAnchor;
private:
	BOOL	m_bCanUseReportEditor = FALSE;

protected:
	CWoormFrame();          // protected constructor used by dynamic creation

	void SetPaneInfo (UINT nId, UINT nStyle, int cwWidth);
	BOOL PreCreateWindow(CREATESTRUCT& cs);
	virtual BOOL IsRadarFrame() const { return FALSE; }

private:
	CTBTabbedToolbar* m_pWTabbedToolBar = NULL;
		CTBToolBar*	m_pWToolBar = NULL;
		CTBToolBar*	m_pWEditToolBar = NULL;
		CTBToolBar*	m_pWSizeToolBar = NULL;
		CTBToolBar*	m_pWBorderToolBar = NULL;
		CTBToolBar*	m_pWAlignMoveToolBar = NULL;
		CTBToolBar*	m_pWObjectToolBar = NULL;

public:  // control bar embedded members
	CToolCbxStr		m_cbxTemplatesStr;

	CMyStatusBar			m_RSStatusBar;
	CFont					m_StatusBarFont;

	CStringArray			m_arUICultures;
	CStringArray			m_arUICultureDescriptions;

	BOOL					m_bAlignToolbarFloating = FALSE;

// Operations
public:
	CWoormDocMng* GetDocument() { return (CWoormDocMng*) GetActiveDocument(); }

	BOOL	CreateToolBar		(CWoormDocMng* pDoc = NULL);
	BOOL	CreateObjectToolBar	(CWoormDocMng* pDoc = NULL);
	BOOL	CreateSizeToolBar		();
	BOOL	CreateAlignMoveToolBar	(const CPoint&);
	BOOL	CreateStatusBar		();

	void	ChangeFieldsButtons	(BOOL isActiveColumn);

	void	ShowRunButton		();
	void	ShowStopButton		();
	void	ShowPauseButton		();
	void	ShowResumeButton	();

	void	ShowReRunButton();
	void	ShowReStopButton();

	void 	SetAllowEditButtonInfo	(BOOL);
	void	SaveSettingsAndCleanUp	(BOOL RecalcLayout = TRUE);

	virtual BOOL	CreateTools(CWoormDocMng* pDoc = NULL);
	virtual BOOL	OnPopulatedDropDown    (UINT nIdCommand);
	virtual BOOL LoadFrame(UINT nIDResource, DWORD dwDefaultStyle = WS_OVERLAPPEDWINDOW | FWS_ADDTOTITLE, CWnd* pParentWnd = NULL, CCreateContext* pContext = NULL);

	void	ShowAlignToolbar	(BOOL bShow = TRUE);

	void	ShowDDLTemplates	(CWoormDocMng* pDoc = NULL);

	CTBTabbedToolbar*	GetTabbedToolBar	(BOOL bCheck = TRUE) { if (bCheck) ASSERT_VALID(m_pWTabbedToolBar); return m_pWTabbedToolBar;}
		CTBToolBar*			GetToolBar			(LPCTSTR lpszText) { ASSERT_VALID(m_pWTabbedToolBar); return m_pWTabbedToolBar->FindToolBar(lpszText); }
		CTBToolBar*			GetMainToolBar		() { ASSERT_VALID(m_pWToolBar); return m_pWToolBar; }
		CTBToolBar*			GetObjectToolBar	() { ASSERT_VALID(m_pWObjectToolBar); return m_pWObjectToolBar; }
		CTBToolBar*			GetEditorToolBar	() { ASSERT_VALID(m_pWEditToolBar); return m_pWEditToolBar; }

	BOOL OnDrawMenuImage(CDC* pDC, const CBCGPToolbarMenuButton* pMenuButton, const CRect& rectImage);

	CMyStatusBar*		GetStatusBar() { return &m_RSStatusBar; }
protected:
	//DDL: drop down list
	BOOL	CreateDDLTemplates(CWoormDocMng* pDoc = NULL);
	
private:
	//LPTSTR GetMenuCaption(UINT id, CString& strBuffer);
	void CreateMenu();
// Implementation
public:
	virtual void Dispose();
	virtual ~CWoormFrame();
	virtual BOOL DestroyWindow();

	virtual BOOL OnCreateClient(LPCREATESTRUCT lpcs, CCreateContext* pContext);
	virtual void OnUpdateFrameTitle(BOOL bAddToTitle);
	virtual BOOL OnCommand(WPARAM wParam, LPARAM lParam);
	virtual BOOL CreateAuxObjects(CCreateContext* pCreateContext);
	void ShowDockingPanels(BOOL bShow);
	virtual CPoint GetPositionSwitchTo();

	afx_msg BOOL OnBarCheck(UINT nID);
	afx_msg void OnUpdateControlBarMenu(CCmdUI* pCmdUI);
	afx_msg BOOL OnHelpInfo			(HELPINFO* pHelpInfo);
	afx_msg void OnWoormHelp		();
	afx_msg void OnUserHelpList		(UINT nID);
	afx_msg void OnSize(UINT nType, int cx, int cy);
	afx_msg void OnGoToProducerSite();
    afx_msg void OnGoToSitePrivateArea();
	
	afx_msg void OnUpdateWrmEasyReading (CCmdUI* pCmdUI);

	afx_msg void ToggleAlignToolbarFloating	();

	afx_msg void OnBtnFilePrint	();

	afx_msg void OnCultureSelected	(UINT nID);
	afx_msg void OnDropDownTemplate ();

	afx_msg LRESULT	OnGetNamespaceAndIcon	(WPARAM, LPARAM);
	afx_msg LRESULT	OnGetDocumentTitleInfo(WPARAM, LPARAM);

	afx_msg LRESULT OnCloneDocument(WPARAM, LPARAM);
	afx_msg LRESULT OnIsRootDocument(WPARAM /*wParam*/, LPARAM /*lParam*/);

	afx_msg LRESULT	OnGetSplittedString	(WPARAM, LPARAM);
	
	afx_msg void	OnGetMinMaxInfo(MINMAXINFO* lpMMI);
	afx_msg void	OnActivate(UINT nState, CWnd* pWndOther, BOOL bMinimized);

	LRESULT OnActivateTab(WPARAM wParam, LPARAM lParam);

	DECLARE_MESSAGE_MAP()

public:
	BOOL ShowNewEditor();
	BOOL ShowPanelToolbarText();

	void ShowPropertyPanel(BOOL bShow);
	void CreatePropertyPanel();

	void CreateAndShowPropertyPanel();

	BOOL CreateDockingPanel();

	void ClosePanels();
	void OnDeactivateFrame();

	CRSEditView*			CreateEditView();
	CRSEditViewDebug*		CreateEditViewDebug();

	CCreateContext								m_BornContext;

	TDisposablePtr<CRSFullReportDockPane>		m_pFullReportPane;
	TDisposablePtr<CRSEngineDockPane>			m_pEnginePane;
	TDisposablePtr<CRSLayoutDockPane>			m_pLayoutPane;

	TDisposablePtr<CRSObjectPropertyDockPane>	m_pPropertyPane;
	TDisposablePtr<CRSToolBoxDockPane>			m_pToolBoxPane;
	TDisposablePtr<CRSToolBoxDBDockPane>		m_pToolBoxDBPane;
	TDisposablePtr<CRSToolBarDockPane>			m_pToolBarPane;
	TDisposablePtr<CRSEditorDockPane>			m_pEditorPane;

	TDisposablePtr<CRS_ObjectPropertyView>		m_pObjectPropertyView ;
	TDisposablePtr<CRSToolBoxObjectsView>		m_pToolBoxTreeView;
	TDisposablePtr<CRSToolBoxDBView>			m_pToolBoxDBView;
	TDisposablePtr<CRSToolBarView>				m_pToolBarView;
	TDisposablePtr<CRSEditViewDocked>			m_pEditorDockedView;

	TDisposablePtr<CRSEditView>					m_pEditView;
	TDisposablePtr<CRSEditViewDebug>			m_pEditViewDebug;

	BOOL SelectLayoutObject(CObject* pObj, BOOL bPassive = TRUE);

	CRSReportTreeView* GetLayoutTreeView() { return m_pLayoutTreeView; }
	CRSReportTreeView* GetEngineTreeView() { return m_pEngineTreeView; }

	CRSReportTreeView* GetHiddenVariablesTreeView() { return  GetEngineTreeView(); }

	void SetReportTreeView(CRSReportTreeView* pView);

	void SetEngineTreeView(CRSReportTreeView* pView);
	void SetLayoutTreeView(CRSReportTreeView* pView);

private:
	TDisposablePtr<CRSReportTreeView>		m_pLayoutTreeView;
	TDisposablePtr<CRSReportTreeView>		m_pEngineTreeView;
};


#include "endh.dex"
