#pragma once
#include <TbNameSolver\ThreadContext.h>
#include <TbNameSolver\CallbackHandler.h>

//includere alla fine degli include del .H
#include "beginh.dex"

//*****************************************************************************
// CDockableFrame frame
//*****************************************************************************
class TB_EXPORT CDockableFrame : public CTBLockedFrame, public IDisposingSourceImpl
{
	DECLARE_DYNCREATE(CDockableFrame)


private:
	CRect					m_FreezedRect;
	CRect					m_FloatingRect;
	bool					m_bDocked;
	bool					m_bDockable;
	DWORD					m_dwAttachedThreadId;
	CArray<HWND>			m_arChilds;
	CDockableFrame*			m_pDockableParent; 
	bool					m_bIsFrameVisible;
	CSize					m_FrameSize;
	bool					m_bFirstActivation;
	BOOL					m_bEditorFrameVisible;
	BOOL					m_bFromExternalResizing;

protected:
	CDocument*				m_pDocument;
	HMENU					m_hMenu;
	//spostato da CBaseFrame - per poter testarlo senza aggiunta di altre variabili ausiliary
	//x EasyBuilder che la sua frame non ha toolbar
	BOOL					m_bHasToolbar;    //Il frame che contiene il CEF browser non necessita di Toolbar
	
public:
	BOOL					m_bChildManagement;
	BOOL					m_bDelayedLayoutSuspended;
	bool					m_bLayoutSuspended;
		
	BOOL					m_bAfterOnFrameCreated;

public:
	CDockableFrame();           // protected constructor used by dynamic creation
	virtual ~CDockableFrame();

	virtual void Dispose() {}
	
	CDocument*			GetDocument() { return m_pDocument; }
	void SuspendLayout();
	void ResumeLayout();
	void SetFrameVisible(bool bVisible) { m_bIsFrameVisible = bVisible; }
	CDockableFrame* GetDockableParent() { return m_pDockableParent; }
	bool IsFrameVisible() { return m_bIsFrameVisible; }

	void SetWoormEditorVisible(BOOL isVisible);
	void SetIDHelp(UINT nIDD) { m_nIDHelp = nIDD; }
	UINT GetIDHelp() { return m_nIDHelp; }

public:
	BOOL IsLayoutSuspended(BOOL bDelayed = FALSE) const;

protected:
	virtual	BOOL UseSplitters		() { return FALSE; }
	virtual	BOOL OnCreateClient		(LPCREATESTRUCT, CCreateContext*);
	virtual	void ActivateFrame		(int nCmdShow = -1);
	virtual BOOL DestroyWindow		();
	virtual BOOL PreCreateWindow	(CREATESTRUCT& cs);
	virtual	HACCEL	GetDocumentAccelerator() { return NULL; }
	
	virtual void PostNcDestroy();
	virtual BOOL IsEditingParamsFromExternalController() { return FALSE; }


	virtual BOOL GetToolbarButtonToolTipText(CBCGPToolbarButton* /*pButton*/, CString& /*strTTText*/);
	virtual BOOL OnCmdMsg(UINT nID, int nCode, void* pExtra, AFX_CMDHANDLERINFO* pHandlerInfo);
	virtual BOOL OnWndMsg(UINT message, WPARAM wParam, LPARAM lParam, LRESULT* pResult);

	afx_msg void	OnSize(UINT nType, int cx, int cy);
	afx_msg LRESULT OnChangeFrameStatus(WPARAM, LPARAM);
	afx_msg LRESULT OnUpdateExternalMenu(WPARAM, LPARAM);
	afx_msg LRESULT OnActivateTab(WPARAM, LPARAM);
	afx_msg LRESULT OnExecuteFunction(WPARAM, LPARAM);
	afx_msg LRESULT OnGetControlDescription(WPARAM, LPARAM);
	afx_msg LRESULT OnEnterSizeMove(WPARAM, LPARAM);
	afx_msg LRESULT OnExitSizeMove(WPARAM, LPARAM);
	afx_msg LRESULT OnMenuMngResizing(WPARAM, LPARAM);
	afx_msg LRESULT OnGetComponent(WPARAM wParam, LPARAM lParam);
	afx_msg int		OnCreate(LPCREATESTRUCT lpCreateStruct);
	void SendTitleUpdatesToMenu();
	virtual void AdjustClientArea ();
	virtual void AdjustTabbedToolBar() {}

	DECLARE_MESSAGE_MAP()

public:
	virtual HWND GetValidOwner();
	void SetOwner(CWnd* pWndOwner);
	void SetOwner(HWND hWndOwner);
	virtual BOOL Create(LPCTSTR lpszClassName, LPCTSTR lpszWindowName, DWORD dwStyle = WS_OVERLAPPEDWINDOW, const RECT& rect = rectDefault, CWnd* pParentWnd = NULL, LPCTSTR lpszMenuName = NULL, DWORD dwExStyle = 0, CCreateContext* pContext = NULL);
	afx_msg void OnDestroy();
	afx_msg void OnActivate(UINT nState, CWnd* pWndOther, BOOL bMinimized);
	virtual void OnFrameCreated();
	virtual BOOL PreTranslateMessage(MSG* pMsg);
	virtual void OnAdjustFrameSize(CSize& size) {}

	void SetDockable(BOOL bSet = TRUE) { m_bDockable = (bSet == TRUE); }
	void SetHasToolbar(BOOL bSet = TRUE) { m_bHasToolbar = (bSet == TRUE); }

	BOOL IsDockable() const { return m_bDockable; }
	static void GetPhantomBitmapFolderPaths(CStringArray& arPaths);
	
	CString GetScreenshotPath(CString ns);

	CArray<HWND>& GetChilds() { return m_arChilds; }
	void AddChild(HWND hwndChild) { m_arChilds.Add(hwndChild); }
	void RemoveChild(HWND hwndChild);
	bool IsDocked() { return m_bDocked; }
	CSize	GetCalcFrameSize();
	void	SetCalcFrameSize(CSize size);
private:
	void	AdjustFrameSize();
};


#include "endh.dex"
