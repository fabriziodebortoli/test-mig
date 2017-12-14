#pragma once

#include <TbNameSolver\CallbackHandler.h>
#include <TbGenlib\OslInfo.h>
#include <TbGenlib\TBToolBar.h>
#include <TbGenlib\TBTabWnd.h>
#include "beginh.dex"

class CTBNamespace;

class CTaskBuilderDockPane;
//======================================================================
class TB_EXPORT CTaskBuilderDockPaneTabs : public CTaskBuilderTabWnd
{
	DECLARE_DYNCREATE(CTaskBuilderDockPaneTabs);
	
	friend class CTaskBuilderDockPane;

	CArray<CInfoOSLButton*,CInfoOSLButton*> m_arTabsOSLInfos;
	CTaskBuilderDockPane*	m_pParentPane;

public:
	CTaskBuilderDockPaneTabs();
	~CTaskBuilderDockPaneTabs();

	BOOL Create (CTaskBuilderDockPane* pParent);

	void AddTab			(CWnd* pWnd, LPCTSTR szName, LPCTSTR szTitle, UINT nImage = -1, BOOL bDetachable = TRUE);
	BOOL RemoveTabOf	(CWnd* pWnd);

	virtual void AttachDocument(CBaseDocument* pDoc) { m_pDocument = pDoc; }
	virtual COLORREF GetTabBkColor(int iTab) const;

private:
	void AttachTabOSLInfo(CInfoOSL* pParent, UINT nID, const CString& sName);
};

//======================================================================
class TB_EXPORT CTaskBuilderDockPaneCaptionButton : public CBCGPCaptionButton
{
	DECLARE_DYNAMIC(CTaskBuilderDockPaneCaptionButton);

	CString m_sName;
	CString m_sImageNameSpace;
	CString m_sTooltip;
	CBCGPToolBarImages	m_CustomImages;

public:
	CTaskBuilderDockPaneCaptionButton();
	CTaskBuilderDockPaneCaptionButton(const CString& sName, UINT nID, BOOL bLeftAlign = FALSE, int buttonSize = 16);
	CTaskBuilderDockPaneCaptionButton(const CString& sName, const CString& sImageNameSpace, BOOL bLeftAlign = FALSE, int buttonSize = 16);

	BOOL HasCustomIcon () const;
	const CString& GetName() const;
	const CString& GetNamespace() const { return m_sImageNameSpace; }

	void SetTooltip(const CString& strTooltip);

	virtual BOOL GetCustomToolTip(CString& strTipText);
	virtual void OnDraw (CDC* pDC, BOOL bActive, BOOL bHorz = TRUE, BOOL bMaximized = TRUE, BOOL bDisabled = FALSE);
};

//======================================================================
class TB_EXPORT CTaskBuilderDockPaneForm
{
	friend class CTaskBuilderDockPane;

	DECLARE_DYNAMIC(CTaskBuilderDockPaneForm);
private:
	CParsedForm*	m_pForm = NULL;
	CString			m_sFormTabPaneTitle;
	CObject*		m_pContext = NULL;
	BOOL			m_bValid = FALSE;
	CCreateContext*	m_pCreateContext = NULL;

public:
	CTaskBuilderDockPaneForm(CRuntimeClass* pWndClass, const CString& sTabPaneTitle /*_T("")*/, CObject* pContext = NULL, CCreateContext* pCreateContext = NULL);
	CTaskBuilderDockPaneForm(CParsedForm* pForm, const CString& sTabPaneTitle /*_T("")*/, CObject* pContext = NULL, CCreateContext* pCreateContext = NULL);
	~CTaskBuilderDockPaneForm();

	BOOL Create(CTaskBuilderDockPane* pPane, BOOL bCallOnInitialUpdate, CSize aSize, CCreateContext* pCreateContext);

public:
	BOOL  IsValid();
	CWnd* GetWnd();
	CString GetTitle() { return m_sFormTabPaneTitle;}
	CCreateContext* GetCreateContext() { return m_pCreateContext; }
};

//======================================================================
class TB_EXPORT CTaskBuilderDockPane : public CBCGPDockingControlBar, public IDisposingSourceImpl, public IOSLObjectManager
{
	friend class CDockingPanes;
	friend class CTaskBuilderDockPaneForm;

	DECLARE_DYNCREATE(CTaskBuilderDockPane);

	CTaskBuilderDockPaneTabs	m_Tabber;
	COLORREF					m_BkgColor;
	COLORREF					m_TitleBkgColor;
	COLORREF					m_TitleForeColor;
	COLORREF					m_TitleHoveringForeColor;
	BOOL						m_bStretchOnFirstSlide;
	BOOL						m_bValid;
	int							m_BtnImageSize;
	CTBToolBar*					m_pToolBar = NULL;
	int							m_nToolbarHeight;
	BOOL						m_bEnabled;
	int							m_nMinWidth;
	BOOL						m_bUseTimer;

	CArray<CTaskBuilderDockPaneForm*> m_Forms;

	CBaseDocument*				m_pDocument = NULL;
	UINT						m_nID = 0;
public:
	CTaskBuilderDockPane();
	CTaskBuilderDockPane(CRuntimeClass* pWndClass, CString sTabPaneTitle = _T(""));
	virtual ~CTaskBuilderDockPane();

public:
	CTaskBuilderDockPaneTabs*	GetTabber();
	CTBToolBar*					GetToolBar() { return m_pToolBar; }
	CTaskBuilderDockPaneForm*	GetForm(int index);
	BOOL Create(CLocalizableFrame* pParent, UINT nID, const CString& sName, const CString& sTitle, DWORD wAlignment, BOOL bCallOnInitialUpdate, CSize aSize, CCreateContext* pCreateContext = NULL, DWORD dwBCGStyle = dwDefaultBCGDockingBarStyle, BOOL bVisible = TRUE);

	CBaseDocument* GetDocument();

	void	AddForm		(CRuntimeClass* pWndClass, CString sTabPaneTitle = _T(""), CObject* pContext = NULL);
	void	AddForm		(CTaskBuilderDockPaneForm* pForm);

	CWnd*	GetFormWnd	(CRuntimeClass* pWndClass) const;
	CWnd*	GetDerivedFormWnd(CRuntimeClass* pWndClass) const;

	void SetMinWidth(int nMinWidth);
	void SetUseTimer(BOOL bValue);

	// caption buttons
	void SetButtonImageSize(int imageSize) { m_BtnImageSize = imageSize; }
	CTaskBuilderDockPaneCaptionButton*  AddCaptionButton(UINT nIDImage, BOOL bLeftAlign = FALSE, int nPos = -1);
	CTaskBuilderDockPaneCaptionButton*  AddCaptionButton(const CString& sName, UINT nIDImage, BOOL bLeftAlign = FALSE, int nPos = -1);
	CTaskBuilderDockPaneCaptionButton*	AddCaptionButton(const CString& sName, const CString& sImageNameSpace, BOOL bLeftAlign /*FALSE*/, int nPos /*-1*/);

	void RemoveCaptionButton(UINT nID);
	void RemoveCaptionButton(const CString& sImageNameSpace);

	COLORREF GetTitleBkgColor()				{ return m_TitleBkgColor; }
	COLORREF GetTitleForeColor()			{ return m_TitleForeColor; }
	COLORREF GetTitleHoveringForeColor()	{ return m_TitleHoveringForeColor; }

	// colors
	void SetBkgColor(COLORREF color);
	void SetTitleBkgColor(COLORREF color);
	void SetTitleForeColor(COLORREF color);
	void SetTitleHoveringForeColor(COLORREF color);
	void SetUpperTabber(BOOL bValue);

	void DoStretch(int cx, int cy);
	void DoStretch();
	void EnablePane(BOOL bEnable);
	void ShowAutoHideButton(BOOL bShow);
	void HidePane(BOOL bSendInAutoHide = TRUE);

	virtual BOOL CanFloat() const;
	virtual BOOL CanBeClosed() const;
	virtual	BOOL PreTranslateMessage	(MSG* pMsg);
	
	
	//Toolbar
	void EnableToolbar(int nToolbarHeight = 25, BOOL bWithTexts = FALSE);
	void AdjustToolbarHeight();
	virtual void	OnCustomizeToolbar() {};
	virtual BOOL	OnPopulatedDropDown(UINT nIdCommand) { return FALSE; }
	virtual void	OnUpdateCmdUI(class CFrameWnd *pTarget, int bDisableIfNoHndler);
	
	virtual void Slide(BOOL bSlideOut, BOOL bUseTimer = TRUE);
	virtual void GetMinSize(CSize& size) const;
	virtual CBCGPAutoHideToolBar* SetAutoHideMode(BOOL bMode, DWORD dwAlignment, CBCGPAutoHideToolBar* pCurrAutoHideBar = NULL, BOOL bUseTimer = TRUE);
	void InitialUpdate();
protected:
	afx_msg	void	OnWindowPosChanged		(WINDOWPOS* lpwndpos);
	afx_msg BOOL	OnEraseBkgnd			(CDC* pDC);
	afx_msg	LRESULT OnGetControlDescription	(WPARAM wParam, LPARAM lParam);

	DECLARE_MESSAGE_MAP()
	
	virtual void OnPressButtons					(UINT nHit);
	virtual void OnCustomCaptionButtonClicked	(CTaskBuilderDockPaneCaptionButton* pButton);
	virtual void OnCustomizePane				() {}
	virtual void OnAddForms						() {}
	virtual void OnSlide						(BOOL bSlideOut);
	virtual BOOL CheckStopSlideCondition		(BOOL bDirection);
	virtual void SetCaptionButtons				();

private:
	BOOL CheckPaneValidity		();
	BOOL UseTabber				();
	BOOL IsLayoutSuspended		();

	//void AttachOSLInfo(CInfoOSL* pParent, const CString& sName); manca il body

	void InitGraphics	();
	void AddForms(CBaseDocument* pDocument);
};

//======================================================================
class TB_EXPORT IDockingLayout : public CObject
{
public:
	enum ContainerArea	{ LEFT, TOP, RIGHT, BOTTOM };
public:
	virtual CSize GetSizeOf (CWnd* pContainer, ContainerArea area) { return CSize(0,0); }
};

//======================================================================
class TB_EXPORT CDockingPanes : public CArray<CTaskBuilderDockPane*>
{
	DECLARE_DYNCREATE(CDockingPanes);

	IDockingLayout*  m_pLayout;
	CCreateContext*  m_pMainCreateContext;
	BOOL			 m_bInCreateFrame;

public:
	CDockingPanes ();
	~CDockingPanes();

	CTaskBuilderDockPane*	CreatePane			(CLocalizableFrame* pParent, CTaskBuilderDockPane* pPane, UINT nID, const CString& sName, const CString& sTitle, DWORD wAlignment, CSize aSize = CSize(0,0), CCreateContext* pCreateContext = NULL, DWORD dwBCGStyle = dwDefaultBCGDockingBarStyle, BOOL bVisible = TRUE);
	CTaskBuilderDockPane*	CreatePane			(CLocalizableFrame* pParent, CRuntimeClass* pPaneClass, UINT nID, const CString& sName, const CString& sTitle, DWORD wAlignment, CSize aSize, CCreateContext* pCreateContext /*NULL*/, DWORD dwBCGStyle = dwDefaultBCGDockingBarStyle, BOOL bVisible = TRUE);
	BOOL					DestroyPane			(CTaskBuilderDockPane*);
	void					DestroyPanes		();

	void					EnableDockingLayout	(CRuntimeClass* pClass = NULL);

	CCreateContext*			GetMainCreateContenxt	();
	void			        BeginOnCreateFrame	    (CCreateContext* pContext);
	void			        EndOnCreateFrame	    ();
	const BOOL&				IsInCreateFrame			()  const;
	CTaskBuilderDockPane*	GetPane					(UINT nID) const;
private:
	IDockingLayout::ContainerArea	ToArea					(DWORD wAlignment); 
	void							DestroyInvalidObjects	();
	BOOL							IsDocumentInDesignMode	();
};

#include "endh.dex"
