
#pragma once

#include <TbGenlib\TBToolBar.h>
#include <TbGenlib\TBCaptionBar.h>
#include <TbGenlib\TBDockPane.h>
#include <TbGenlib\ExtStatusControlBar.h>
#include <TbGenlib\TBParsedProgressBar.h>
#include "TbGenlib\TBBreadCrumb.h"
#include "TbGenlib\BaseFrm.h"
#include "TbGeneric\DockableFrame.h"
//------------------------------------------------------------------------------------------
//includere alla fine degli include del .H
#include "beginh.dex"

class CTaskBuilderStatusBar;
class CAbstractFormView;

TB_EXPORT extern const TCHAR szToolbarNameMain[];
TB_EXPORT extern const TCHAR szToolbarNameAux[];
TB_EXPORT extern const TCHAR szToolbarNameTools[];
TB_EXPORT extern const TCHAR szToolbarNameExport[];

//==========================================================================================
class TB_EXPORT CAbstractFrame : public CBaseFrame, public CSplittedForm
{
	DECLARE_DYNCREATE(CAbstractFrame)
	friend class CQueryManager;
	friend class CAbstractFormFrame;

private:
	BOOL						m_bDestroying = FALSE;
	
protected:
	// modifiche definitive
	CTaskBuilderCaptionBar*		m_pCaptionBar = NULL;
	CTBTabbedToolbar*			m_pTabbedToolBar = NULL;
	BOOL						m_bAddLoginDataInTitle;

protected:	//	Data Member
	CTaskBuilderStatusBar*	m_pStatusBar = NULL;
	CTBToolBar*				m_pToolBarActive = NULL;
	UINT 					m_nIcon; 		// icon ID to customize icons
	CString					m_strQueryName;	// contiene il nome della query corrente, se presente

public:		//	Constructor & Destructors
	CAbstractFrame ();
	virtual ~CAbstractFrame();

public:	
	const BOOL&				IsDestroying	() { return m_bDestroying; }
	void					SetDestroying	()		{ m_bDestroying = TRUE;  }
	CDockingPanes*			GetDockPane		()		 { return &m_DockingPanes; }
	const BOOL&				HasToolbar		() const { return m_bHasToolbar; }
	const BOOL&				HasStatusBar	() const { return m_bHasStatusBar; }
	virtual CTBTabbedToolbar* GetTabbedToolBar () { return m_pTabbedToolBar; }
	CTaskBuilderCaptionBar*	GetCaptionBar	();

	BOOL CreateCaptionBar(UINT nID, CString strName);

	afx_msg LRESULT OnCaptionBarHyperlinkClicked(WPARAM, LPARAM);
	BOOL OnDrawMenuImage (	CDC* pDC, const CBCGPToolbarMenuButton* pMenuButton, const CRect& rectImage);

	HICON					GetFrameIcon	();
	void					SetFrameIcon	();
	UINT 					GetIconID 		() const	{ return m_nIcon; }
	void 					SetIconID 		(UINT nIcon){ m_nIcon = nIcon; }
	void					SetQueryName	(const CString& strQueryName) { m_strQueryName = strQueryName; }

	void			SetAddLoginDataInTitle(BOOL bAdd);
	const BOOL&		GetAddLoginDataInTitle() const;

	CTaskBuilderStatusBar*	GetStatusBar() { return m_pStatusBar; }

public:	
	virtual BOOL	DestroyWindow();
	// per gestire la tabella  di accelerazione del documento dopo quella base
	virtual BOOL PreTranslateMessage(MSG* pMsg);
	virtual BOOL OnCommand(WPARAM wParam, LPARAM lParam);
	
	// per gestire dinamicamente i tips nella status bar (es.: flyby dei tooltips di toolbar)
	virtual void GetMessageString(UINT nID, CString& rMessage) const;
	virtual	HACCEL	GetDocumentAccelerator();
	TB_OLD_METHOD CDocument* GetActiveDocumentConst() const { return ((CAbstractFrame*)this)->GetActiveDocument(); }
	virtual void SetToolBarActive(CWnd* pCWnd);

	virtual BOOL OnCmdMsg(UINT nID, int nCode, void* pExtra, AFX_CMDHANDLERINFO* pHandlerInfo);
	virtual CString GetDocAccelText(WORD id);

public:

	/// Implementation
	//{{AFX_MSG(CAbstractFrame)
	afx_msg	HCURSOR	OnQueryDragIcon		();
	BOOL OnToolTipText					(UINT, NMHDR*, LRESULT*);
	afx_msg LRESULT OnGetSoapPort		(WPARAM wParam, LPARAM lParam);
	afx_msg LRESULT	OnGetNamespaceAndIcon		(WPARAM, LPARAM);
	afx_msg LRESULT	OnGetDocumentTitleInfo		(WPARAM, LPARAM);
	afx_msg LRESULT OnFetchRecord		(WPARAM wParam, LPARAM lParam);
	afx_msg void	OnUserHelpList		(UINT nID);
	afx_msg void	OnSize(UINT nType, int cx, int cy);
	afx_msg void	OnGetMinMaxInfo(MINMAXINFO* lpMMI);
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

// Diagnostics
#ifdef _DEBUG
public:
	void Dump (CDumpContext&) const;
	void AssertValid() const;
#endif // _DEBUG
};

class CTaskBuilderBreadcrumb;
class CTabManager;
class CAbstractFormFrame;
//=============================================================================
class TB_EXPORT CFrameStepper : public CBCGPDialogBar
{
	CTaskBuilderBreadcrumb*		m_pStepper;
	DataStr*					m_pStepperDataObj;
	CArray<CString, CString>	m_arSteps;
	CFont*						m_pFont;

public:
	CFrameStepper();
	~CFrameStepper();

public:
	static CString GetStepperName();

	CTaskBuilderBreadcrumb* GetStepper		();
	DataStr*				GetStepperDataObj();

	void CreateStepper	(CAbstractFormFrame* pParent, CRuntimeClass* pStepperClass);
	void UpdateStepper	(const CString& sRootDescri, CTabManager* pTabManager, int nDirection = 1);
	void SetHeight		(int nHeight);

	void	DoRecalcSize(int cx, int cy);

	static CString StripPath (const CString& strPath);

protected:
	afx_msg void OnSize (UINT nType, int cx, int cy);
	DECLARE_MESSAGE_MAP();
};

//=============================================================================
class TB_EXPORT CAbstractFormFrame : public CAbstractFrame
{
	DECLARE_DYNCREATE(CAbstractFormFrame)

	friend class CAbstractFormDoc;
	friend class CSingleExtDocTemplate;
	friend class CClientDoc;

	CFrameStepper*	m_pStepper = NULL;
	CJsonContextObj* m_pToolbarContext = NULL;
protected:
	CString				m_strSubTitle;
	CParsedProgressBar*		m_pProgressBar = NULL;

	//serve per associare le query con gli ID nel menù pop delle query
	CIdStringArray*	  m_pIDQueryArray = NULL;
	
public:		//	Implementation
	virtual	void SetFrameSize		(CSize csDialogSize);
	virtual	void SetSubTitle		(UINT nSubTitleID);
	virtual	void SetSubTitle		(const CString& strSubTitle);
	virtual void OnUpdateFrameTitle	(BOOL bAddToTitle);
	virtual void DoUpdateFrameTitle	(BOOL bAddToTitle, BOOL bOnlySubTitle = FALSE);

public:
	CWnd* GetMsgBar(){return GetMessageBar();}

public:	
	// per poter gestire anche la tabella di accelerazione apportata dai ClientDoc
	virtual BOOL			PreTranslateMessage				(MSG* pMsg);
	virtual	CTBNamespace    GetAuxToolBarButtonNameSpace	(int nID);
	virtual	int				GetAuxToolbarCommandID			(const CTBNamespace aNS);
	
	CFrameStepper*	GetFrameStepper() { return m_pStepper; }

public:		//	Constructor & Destructors
	CAbstractFormFrame ();
	virtual ~CAbstractFormFrame();
	
public:

	virtual CTBToolBar*			CreateEmptyToolBar	(LPCTSTR name, LPCTSTR text = NULL);
	
			CAbstractFormView*	GetViewByCtrlID		(UINT nIDC);
	
protected:
	virtual BOOL CreateJsonToolbar(UINT nID);
	BOOL CreateJsonToolbar();
	BOOL CreateJsonToolbar(CWndObjDescription* pDescription);
	BOOL CreateToolBar();
	void CreateToolbarFromDesc(CTBToolBar *pToolbar, CToolbarDescription* pToolBarDesc);
private:
	LRESULT OnEnable			(WPARAM wParam, LPARAM lParam);
	void	CreateQueryMenu		(BOOL bShowQueryManager = FALSE);
	CString GetQueryName		(UINT);
	void	OnActivateHandler	(BOOL bActivate, CView* pView);
	
	// return the status bar pane position, -1 not found
	INT  FindPane(INT nIDPane);

protected:	// Function Member

	virtual BOOL LoadFrame(UINT nIDResource, DWORD dwDefaultStyle = WS_OVERLAPPEDWINDOW | FWS_ADDTOTITLE, CWnd* pParentWnd = NULL, CCreateContext* pContext = NULL);
	virtual BOOL CreateStatusBar		();
	virtual BOOL CreateAccelerator		();
	virtual BOOL RemovedAcceleratorList (CList<int, int>* pList);

	virtual	BOOL OnCreateClient			(LPCREATESTRUCT, CCreateContext*);
	virtual void OnCreateStepper		() {}

		virtual BOOL CreateTabbedToolBar		();
		virtual BOOL OnCustomizeJsonToolBar();
		virtual BOOL OnAddClientDocToolbarButtons() { return TRUE; }
		virtual BOOL OnCustomizeTabbedToolBar	(CTBTabbedToolbar* pTabbedBar );
		virtual void OnFrameCreated();
public:	
	virtual void SetStatusBarText	(const CString& strText);
	virtual BOOL OnPopulatedDropDown    (UINT nIdCommand);
	virtual CPoint GetPositionSwitchTo();
	void DoModal();
	//{{AFX_MSG(CAbstractFormFrame)
	afx_msg void OnActivate(UINT nState, CWnd* pWndOther, BOOL bMinimized);
	afx_msg BOOL OnHelpInfo			(HELPINFO* pHelpInfo);
	afx_msg void OnFormHelp			();
	afx_msg void OnUpdateFormHelp	(CCmdUI*);
	afx_msg void OnToolbarDropDown	(NMHDR* pnmh, LRESULT* plRes);

	afx_msg void	OnGoToProducerSite();
    afx_msg void	OnGoToSitePrivateArea();
	afx_msg LRESULT OnCloneDocument(WPARAM, LPARAM);
	afx_msg LRESULT OnIsRootDocument(WPARAM, LPARAM);
	afx_msg LRESULT OnHasInvalidView(WPARAM, LPARAM);

//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

// Diagnostics
#ifdef _DEBUG
public:
	void Dump (CDumpContext&) const;
	void AssertValid() const;
#endif // _DEBUG
};

//=============================================================================
class TB_EXPORT CCreateSlaveData
{
	friend class CMasterFrame;

	BOOL			m_bDestroyMe;
	CRuntimeClass*	m_pClass;
	CString			m_sTitle;
	HWND			m_hwndToClose;
	BOOL			m_bDisableDocument;

public:
	CCreateSlaveData(CRuntimeClass* pSlaveClass, CString& sTitle, HWND wndToClose = NULL, BOOL bDestroyMe = TRUE, BOOL bDisableDocument = FALSE);

};

//=============================================================================
class TB_EXPORT CMasterFrame : public CAbstractFormFrame 
{
	DECLARE_DYNCREATE(CMasterFrame)
protected:
	CExtButton* m_pAdminIcon = NULL;

public:		//	Constructor & Destructors
	CMasterFrame();
	virtual ~CMasterFrame();
	virtual void AdjustTabbedToolBar();

protected:
	virtual	BOOL OnToolTipText		(UINT nID, NMHDR* pNMHDR, LRESULT* pResult);
	virtual void OnAdjustFrameSize	(CSize& size);
	virtual BOOL OnAddClientDocToolbarButtons();
public:		// Implementation
	afx_msg void	OnClose			();

	afx_msg	LRESULT OnIsUnattendedWindow(WPARAM wParam, LPARAM lParam);
	afx_msg LRESULT OnDeferredCreateSlaveView(WPARAM wParam, LPARAM lParam);

	DECLARE_MESSAGE_MAP()

// Diagnostics
#ifdef _DEBUG
public:
	void Dump (CDumpContext&) const;
	void AssertValid() const;
#endif // _DEBUG
};

//=============================================================================
class TB_EXPORT CBatchFrame : public CMasterFrame
{
	DECLARE_DYNCREATE(CBatchFrame)
public:		
	// reimplementata per cambiare lo standard behaviour, la toolbar e` diversa
			void			SwitchBatchRunButtonState		();
	
protected:
		virtual BOOL OnCustomizeJsonToolBar();

protected:
	//{{AFX_MSG(CBatchFrame)
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

//=============================================================================
class TB_EXPORT CSlaveFrame : public CAbstractFormFrame
{
	DECLARE_DYNCREATE(CSlaveFrame)

public:		//	Constructor & Destructors
	CSlaveFrame();
	virtual ~CSlaveFrame();

protected:	// Function Member
	
	virtual BOOL			CreateAccelerator	();
	virtual BOOL			Create				(LPCTSTR lpszClassName, LPCTSTR lpszWindowName, DWORD dwStyle = WS_OVERLAPPEDWINDOW, const RECT& rect = rectDefault, CWnd* pParentWnd = NULL, LPCTSTR lpszMenuName = NULL, DWORD dwExStyle = 0, CCreateContext* pContext = NULL);
	virtual BOOL			PreCreateWindow		(CREATESTRUCT& cs);
public:
	virtual BOOL			IsPopup()			{ return TRUE;}
	
protected:
	virtual BOOL OnCustomizeJsonToolBar();
	virtual BOOL OnCustomizeTabbedToolBar	(CTBTabbedToolbar* pTabbedBar);
		virtual void OnAdjustFrameSize			(CSize& size);
public:
	// Generated message map functions
	//{{AFX_MSG(CSlaveFrame)
	afx_msg LRESULT OnChangeVisualManager(WPARAM wParam, LPARAM lParam);
	afx_msg void OnGotoMaster	();
	afx_msg void OnClose		();
	afx_msg void OnCustomize	();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

// Diagnostics
#ifdef _DEBUG
public:
	void Dump (CDumpContext&) const;
	void AssertValid() const;
#endif // _DEBUG
};

//=============================================================================
class TB_EXPORT CRowFormFrame : public CSlaveFrame
{
	DECLARE_DYNCREATE(CRowFormFrame)

public:		//	Constructor & Destructors
	CRowFormFrame();
	virtual ~CRowFormFrame();

protected:	// Function Member
			
	virtual BOOL			CreateAccelerator	();
public:
	// Generated message map functions
	//{{AFX_MSG(CRowFormFrame)
	afx_msg void OnClose();
	afx_msg void OnGetMinMaxInfo(MINMAXINFO* lpMMI);
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

protected:
	virtual BOOL OnCustomizeJsonToolBar();
	virtual BOOL OnCustomizeTabbedToolBar	(CTBTabbedToolbar* pTabbedBar);
		virtual BOOL CreateStatusBar();

// Diagnostics
#ifdef _DEBUG
public:
	void Dump (CDumpContext&) const;
	void AssertValid() const;
#endif // _DEBUG
};

//=============================================================================
class TB_EXPORT CRowFormSimpleFrame : public CRowFormFrame
{
	DECLARE_DYNCREATE(CRowFormSimpleFrame)
public:
	CRowFormSimpleFrame() {}

protected:
	virtual BOOL OnCustomizeJsonToolBar();
	virtual BOOL OnCustomizeTabbedToolBar	(CTBTabbedToolbar* pTabbedBar);
};

//=============================================================================
class TB_EXPORT CRowFormEmbeddedSimpleFrame : public CRowFormFrame
{
	DECLARE_DYNCREATE(CRowFormEmbeddedSimpleFrame)
public:
	CRowFormEmbeddedSimpleFrame() {}

protected:	// Function Member
	virtual	BOOL			CreateStatusBar		() {return TRUE;}

	virtual	void			ActivateFrame		(int nCmdShow = -1) {}
	
protected:
	virtual BOOL OnCustomizeJsonToolBar();
	virtual BOOL OnCustomizeTabbedToolBar	(CTBTabbedToolbar* pTabbedBar);

	//{{AFX_MSG(CRowFormEmbeddedFrame)
	afx_msg LRESULT OnGetControlDescription(WPARAM, LPARAM);
	////}}AFX_MSG
	DECLARE_MESSAGE_MAP()
public:
	virtual BOOL			IsPopup() { return FALSE;}
	virtual void			OnFrameCreated ();
	virtual BOOL			PreCreateWindow(CREATESTRUCT& cs);
};

////=============================================================================
class TB_EXPORT CRowFormEmbeddedFrame : public CRowFormEmbeddedSimpleFrame
{
	DECLARE_DYNCREATE(CRowFormEmbeddedFrame)
public:
	CRowFormEmbeddedFrame() {}

protected:	
	virtual BOOL			CreateToolBar		() { return TRUE; }

protected:
	virtual BOOL OnCustomizeJsonToolBar() { return TRUE; }
	virtual BOOL OnCustomizeTabbedToolBar	(CTBTabbedToolbar* pTabbedBar) { return FALSE; }
};

//=============================================================================
class TB_EXPORT CSlaveFormEmbeddedFrame : public CSlaveFrame
{
	DECLARE_DYNCREATE(CSlaveFormEmbeddedFrame)
public:
	CSlaveFormEmbeddedFrame ();

	virtual BOOL PreCreateWindow	(CREATESTRUCT& cs);

protected:	// Function Member
	virtual BOOL			CreateToolBar		() { return TRUE; }
	virtual	BOOL			CreateStatusBar		() { return TRUE; }
	virtual	void			ActivateFrame		(int nCmdShow = -1) {}

protected:
	virtual BOOL OnCustomizeJsonToolBar() { return TRUE; }
	virtual BOOL OnCustomizeTabbedToolBar	(CTBTabbedToolbar* pTabbedBar) { return FALSE; }

	//{{AFX_MSG(CRowFormEmbeddedFrame)
	afx_msg LRESULT OnGetControlDescription(WPARAM, LPARAM);
	////}}AFX_MSG
	DECLARE_MESSAGE_MAP()
public:
	virtual BOOL			IsPopup(){ return FALSE;}

	virtual void			OnFrameCreated();
};

//=============================================================================
class TB_EXPORT CWizardFrame : public CMasterFrame
{
	DECLARE_DYNCREATE(CWizardFrame)
public:	
	CWizardFrame();
	virtual BOOL CreateStatusBar	();

protected:

protected:
	//{{AFX_MSG(CWizardFrame)
	afx_msg void OnActivate(UINT nState, CWnd* pWndOther, BOOL bMinimized);
	afx_msg	LRESULT	OnBatchCompleted	(WPARAM wParam, LPARAM lParam);

	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

	void OnActivateHandler(BOOL bActivate, CWnd* pActivateWnd);
	virtual BOOL Create(LPCTSTR lpszClassName, LPCTSTR lpszWindowName, DWORD dwStyle = WS_OVERLAPPEDWINDOW, const RECT& rect = rectDefault, CWnd* pParentWnd = NULL, LPCTSTR lpszMenuName = NULL, DWORD dwExStyle = 0, CCreateContext* pContext = NULL);

// Diagnostics
#ifdef _DEBUG
public:
	void Dump (CDumpContext&) const;
	void AssertValid() const;
#endif // _DEBUG
};

//=============================================================================
class TB_EXPORT CWizardBatchFrame : public CBatchFrame
{
	DECLARE_DYNCREATE(CWizardBatchFrame)

public:	
	CWizardBatchFrame();
	
protected:
	virtual BOOL OnCustomizeJsonToolBar();
	virtual BOOL OnCustomizeTabbedToolBar(CTBTabbedToolbar* pTabbedBar);
		

public:
	virtual BOOL HasRestartButton			() { return FALSE; }
	
	afx_msg	LRESULT	OnBatchCompleted	(WPARAM wParam, LPARAM lParam);

protected:
	//{{AFX_MSG(CWizardBatchFrame)
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
	virtual BOOL Create(LPCTSTR lpszClassName, LPCTSTR lpszWindowName, DWORD dwStyle = WS_OVERLAPPEDWINDOW, const RECT& rect = rectDefault, CWnd* pParentWnd = NULL, LPCTSTR lpszMenuName = NULL, DWORD dwExStyle = 0, CCreateContext* pContext = NULL);

};

//=============================================================================
//		class CWizardStepperBatchFrame
//=============================================================================
class TB_EXPORT CWizardStepperBatchFrame : public CWizardBatchFrame
{
	DECLARE_DYNCREATE(CWizardStepperBatchFrame)

	CWizardStepperBatchFrame();

protected:
	virtual void			OnCreateStepper();
	virtual BOOL			HasRestartButton()		{ return TRUE; }
};



//=============================================================================
//		class CStepperBreadCrumb
//=============================================================================
class TB_EXPORT CStepperBreadCrumb : public CTaskBuilderBreadcrumb
{
	DECLARE_DYNCREATE(CStepperBreadCrumb)

public:
	CStepperBreadCrumb();

	virtual void							SetRoot(const CString& sRoot);
	virtual CTaskBuilderBreadcrumbItem*		AddItem(const CString& sName, const CString& sText, CTaskBuilderBreadcrumbItem* pParent = NULL);
	virtual void							UpdateBreadCrumb();

private:
	CArray<CString, CString>	m_arItemsList;

protected:
	virtual BOOL OnInitCtrl();
};

//==========================================================================================
class TB_EXPORT CBrowserFrame : public CAbstractFormFrame
{
	DECLARE_DYNCREATE(CBrowserFrame)
public:
	CBrowserFrame() { m_bHasToolbar = false; }
};


//==========================================================================================
#include "endh.dex"
