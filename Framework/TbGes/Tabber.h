
#pragma once

#include <TbGenlib\parsobj.h>
#include <TbGenlib\tabcore.h>
#include <TbGeneric\TBThemeManager.h>

#include "extdoc.h"

//includere alla fine degli include del .H
#include "beginh.dex"

/////////////////////////////////////////////////////////////////////////////
class CTabManager;
class CTabDialog;
class ControlLinks;
class CWndObjDescriptionContainer;
class CDBTTreeEdit;
class CTileGroup;
class CBaseTileGroup;

/////////////////////////////////////////////////////////////////////////////
//					class CTabDialog definition
/////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT CTabDialog : public CBaseTabDialog
{
	friend class CTabManager;
	friend class CTileGroup;

	DECLARE_DYNAMIC(CTabDialog)
		
protected:
    CAbstractFormView*		m_pFormView;

	CTabManager*			m_pParentTabManager;

	TabManagers*			m_pChildTabManagers;		// contiene i tab dialog manager

	CTileGroup*				m_pTileGroup;				//eventuale TabManager figlio

public:
	CMap<CString, LPCTSTR, HWND, HWND> m_HWNDPositionsMap;//mappa ad uso e consumo di easybuilder per trovare gli handle delle finestre non registrate

	CTabDialog(const CString& sName, UINT nIDD = 0);
	virtual ~CTabDialog();

protected:
	BOOL BatchEnableTabDialogControls();
	void EnableTabDialogControls();
	
	void AttachParents(CAbstractFormDoc* pDoc, CAbstractFormView* pView, CTabManager* pTabber);
	void AttachParents(CAbstractFormDoc* pDoc, CParsedDialog* pDlg, CTabManager* pTabber);

public :
	CAbstractFormDoc*	GetDocument						() const	{ return (CAbstractFormDoc*) __super::GetDocument(); }

	CWnd*				GetWndLinkedCtrl				(UINT nIDC);
	virtual CWnd*		GetWndLinkedCtrl				(const CTBNamespace& aNS);
	CParsedCtrl*		GetLinkedParsedCtrl				(UINT nIDC);
	CParsedCtrl*		GetLinkedParsedCtrl				(const CTBNamespace& aNS);
	CParsedCtrl*		GetLinkedParsedCtrl				(DataObj* pDataObj);
	CBodyEdit*			GetBodyEdits					(int* pnStartIdx = NULL);	
	CBodyEdit*			GetBodyEdits					(const CTBNamespace& aNS);
	CBaseTileDialog*	GetTileDialog					(UINT nIDD);

	CWnd*				GetWndCtrl						(UINT nIDC);
	void				MoveControls					(CSize offset);
	BOOL				SetControlValue					(UINT nIDC, const DataObj& val);

	CBaseTabManager*	GetTabber						(UINT nIDC);
	CTabManager*		GetParentTabManager				()								const	{ return m_pParentTabManager; }

	BOOL				SetControlAutomaticExpression	(DataObj* , const CString& );

	TabManagers*		GetChildTabManagers		()	const	{ return m_pChildTabManagers; }

	virtual CBaseTileGroup*		GetChildTileGroup() const;
	virtual CBaseTabManager*	GetChildTabManager() const;

	static CWndObjDescription*			GetControlStructure			(CWndObjDescriptionContainer* pContainer, DlgInfoItem* pItem, CBaseTabDialog* pDialog, CTabManager* pParentTabManager);
	void				SetDefaultFocus			();

	void				Register				(CBodyEdit* pBody);

	// overridable
	virtual	void	BuildDataControlLinks	() {/* do nothing*/ }
	virtual void	CustomizeExternal		() {/* do nothing*/ }
	virtual BOOL	OnInitDialog			();
	virtual	void	EnableTabDialogControlLinks (BOOL bEnable = TRUE, BOOL bMustSetOSLReadOnly = FALSE);
	virtual	void	OnUpdateControls		(BOOL bParentIsVisible = TRUE);

	// implementabile per decidere lo stato dei control propri della tabdialog (utili per le
	// tab dialog aggiunte tramite ClientDoc )
	virtual void	OnDisableControlsForBatch	() {/* do nothing*/}
	virtual void	OnDisableControlsForAddNew	() {/* do nothing*/}
	virtual void	OnDisableControlsForEdit	() {/* do nothing*/}
	virtual void	OnEnableControlsForFind		() {/* do nothing*/}
	virtual void	OnDisableControlsAlways		() {/* do nothing*/}

	virtual	void	OnResetDataObjs				();

	virtual void	OnBeforeAttachParents		(CAbstractFormDoc* pDoc, CAbstractFormView* pView, CTabManager* pTabber) { }
	virtual void	OnAttachParents				(CAbstractFormDoc* pDoc, CAbstractFormView* pView, CTabManager* pTabber) { }

	//da reimplementare nelle proprie classi
	virtual BOOL	OnPrepareAuxData			() {return TRUE;}

	//chiamata dal framework per elaborazioni interne e dispatch alla classe derivata 
	//con la chiamata al metodo OnPrepareAuxData, ai TabManagers contenuti ed ai ClientDoc 
	virtual BOOL	PrepareAuxData				();

	virtual void	SyncExternalControllerInfo(BOOL bSave);

	virtual CBaseTabManager*	AddBaseTabManager(UINT nIDC, CRuntimeClass* pClass, const CString& sName, BOOL bCallOnInitialUpdate = TRUE)  { return (CBaseTabManager*)  AddTabManager(nIDC, pClass, sName, bCallOnInitialUpdate); }
	virtual CTabManager*	AddTabManager(UINT nIDC, CRuntimeClass* pClass, const CString& sName, BOOL bCallOnInitialUpdate = TRUE);
	virtual CTileGroup*		AddTileGroup(UINT nIDC, CRuntimeClass* pClass, const CString& sName, BOOL bCallOnInitialUpdate = TRUE, TileGroupInfoItem* pDlgInfoItem = NULL, CRect rectWnd = CRect(0, 0, 0, 0));

	virtual void EnableTabControls();
			void OnFindHotLinks();

	virtual CParsedCtrl*	AddLink
		(
			UINT			nIDC, 
			const CString&	sName,
			SqlRecord*		pRecord, 
			DataObj*		pDataObj, 
			CRuntimeClass*	pParsedCtrlClass,

			HotKeyLink*		pHotKeyLink			= NULL,
			UINT			nBtnID				= BTN_DEFAULT
		);
	virtual CParsedCtrl*	AddLink
		(
			UINT			nIDC, 
			const CString&	sName,
			SqlRecord*		pRecord, 
			DataObj*		pDataObj, 
			CRuntimeClass*	pParsedCtrlClass,

			CString			sNsHotKeyLink,
			UINT			nBtnID				= BTN_DEFAULT
		);

	virtual CExtButton*		AddLink
		(
			UINT			nIDC, 
			const CString&	sName,
			SqlRecord*		pRecord	= NULL, 
			DataObj*		pDataObj = NULL
		);
							
	virtual CBodyEdit*		AddLink
		(
			UINT				nIDC, 
			DBTSlaveBuffered*	pDBT, 
			CRuntimeClass*		pBodyEditClass,
			CRuntimeClass*		pRowFormViewClass = NULL,
			CString				strTitle = _T(""),
			CString				sName = _T("")
		);

	virtual CTBGridControl* AddLinkGrid
		(
		UINT				nIDC, 
		DBTSlaveBuffered*	pDBT, 
		CRuntimeClass*		pGridControlClass = NULL,
		CString				sTitle = _T(""),
		CString				sName = _T("")
		);

	virtual CDBTTreeEdit*		AddLink
		(
			UINT				nIDC, 
			DBTSlaveBuffered*	pDBT, 
			CString				sName = _T(""),
			CRuntimeClass*		pTreeClass = NULL
		);
	virtual CParsedPanel* AddLink
		(
					UINT			nIDC, 
					CRuntimeClass*	pParsedPanelClass, 
					CObject*		pPanelOwner,
			const	CString&		sName, 
			const	CString&		sCaption = _T(""), 
					BOOL			bCallOnInitialUpdate = TRUE
		);

	virtual CWnd* AddLink
		(
			UINT			nIDC, 
			const CString&	sName,
			CRuntimeClass*  prtCtrl
		);

	virtual CParsedCtrl* AddLinkAndCreateControl
		(
			const CString&	sName,
			DWORD			dwStyle, 
			const CRect&	rect,
			UINT			nIDC, 
			SqlRecord*		pRecord, 
			DataObj*		pDataObj, 
			CRuntimeClass*	pParsedCtrlClass,
			HotKeyLink*		pHotKeyLink			= NULL,
			UINT			nBtnID				= BTN_DEFAULT
		);

	virtual CBodyEdit* AddLinkAndCreateBodyEdit
		(
			CRect				rect,
			UINT				nIDC, 
			DBTSlaveBuffered*	pDBT, 
			CRuntimeClass*		pBodyEditClass,
			CRuntimeClass*		pRowFormViewClass	= NULL,
			CString				strRowFormViewTitle = _T(""),
			CString				sBodyName			= _T(""),
			CString				sRowViewName		= _T("")
		);

	 CLabelStatic* AddLabelLink		(UINT nIDC);
	 CLabelStatic* AddLabelLinkWithLine(UINT nIDC, int nSizePen = 1, int pos = CLabelStatic::LP_TOP);
	 CLabelStatic* AddSeparatorLink (UINT nIDC, COLORREF crBorder, int nSizePen = 1, BOOL  bVertical = FALSE, CLabelStatic::ELinePos pos = CLabelStatic::LP_VCENTER);
	 CGroupBoxBtn* AddGroupBoxLink	(UINT nIDC);

	 CParsedCtrl*   ReplaceAddLink
		(
			UINT			nIDC, 
			CRuntimeClass*	pParsedCtrlClass,
			HotKeyLink*		pHotKeyLink			 = NULL,
			UINT			nBtnID				 = BTN_DEFAULT
		);


	 CTBPropertyGrid*	AddLinkPropertyGrid
		 (
			 UINT				nIDC,
			 CString			sName,
			 CRuntimeClass*		pRuntimeClass = NULL
		 );

	virtual	BOOL OnCommand			(WPARAM wParam, LPARAM lParam);

protected:
	//{{AFX_MSG(CTabDialog)
	//gestione centratura controlli
	afx_msg void OnRButtonDown	(UINT nFlags, CPoint ptMousePos);
	afx_msg void OnSize			(UINT nType, int cx, int cy);
	afx_msg void OnDestroy		();
	afx_msg	BOOL OnEraseBkgnd	(CDC* pDC);

	LRESULT OnGetControlDescription(WPARAM wParam, LPARAM lParam);
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

#ifdef _DEBUG                                                     
public:
	virtual void AssertValid() const;
	virtual void Dump(CDumpContext& dc) const;
#endif	
	protected:
		virtual CJsonContextObj* GetJsonContext();
};

/////////////////////////////////////////////////////////////////////////////
//					class CRowTabDialog definition
/////////////////////////////////////////////////////////////////////////////
//
/////////////////////////////////////////////////////////////////////////////
class TB_EXPORT CRowTabDialog : public CTabDialog
{
	DECLARE_DYNAMIC(CRowTabDialog)
		
	friend class CTabManager;
	friend class CRowFormView;

protected:
	Array	m_DataToCtrlMap;

protected:
	virtual BOOL OnInitDialog			();

protected:
	void RebuildLinks			(SqlRecord*);

protected:
	virtual	void EnableTabDialogControlLinks	(BOOL bEnable = TRUE, BOOL bMustSetOSLReadOnly = FALSE);
	virtual	void OnUpdateControls				(BOOL bParentIsVisible = TRUE);

public:
	CRowTabDialog (const CString& sName, UINT nIDD = 0);
	virtual ~CRowTabDialog() {}

	virtual CParsedCtrl*	AddLink
		(
			UINT			nIDC, 
			const CString&	sName,
			SqlRecord*		pRecord, 
			DataObj*		pDataObj, 
			CRuntimeClass*	pParsedCtrlClass,
			HotKeyLink*		pHotKeyLink			= NULL,
			UINT			nBtnID				= BTN_DEFAULT
		);

	virtual CExtButton*		AddLink
		(
			UINT			nIDC, 
			const CString&	sName,
			SqlRecord*		pRecord	= NULL, 
			DataObj*		pDataObj = NULL
		);
							
	virtual	int				AddDataBoolToCheckLB
		(
			CBoolCheckListBox*	pBCLB,
			LPCTSTR				lpszAssoc,
			SqlRecord*			pRecord, 
			DataObj*			pDataObj
		);

#ifdef _DEBUG                                                     
public:
	virtual void AssertValid() const;
	virtual void Dump(CDumpContext& dc) const;
#endif
};

/////////////////////////////////////////////////////////////////////////////
//					class CTabDlgEmpty definition
/////////////////////////////////////////////////////////////////////////////
//
/////////////////////////////////////////////////////////////////////////////
class TB_EXPORT CTabDlgEmpty : public CTabDialog
{
	DECLARE_DYNCREATE(CTabDlgEmpty)

public:
	CTabDlgEmpty(CString sName = _T(" "), UINT nIDD = IDD_EMPTY_TAB) : CTabDialog(sName, nIDD) {}

public:
	virtual	void BuildDataControlLinks() {}
};

/////////////////////////////////////////////////////////////////////////////
//					class CMyComboButton definition
/////////////////////////////////////////////////////////////////////////////
class CTitleButton : public CButton
{
	//DECLARE_DYNAMIC(CTitleButton)
	//DECLARE_MESSAGE_MAP()

public:
	CString m_sTitle;
	COLORREF m_crTextColor;

	CTitleButton();

	virtual void DrawItem(LPDRAWITEMSTRUCT lpDrawItemStruct);
};

/////////////////////////////////////////////////////////////////////////////
//					class CTabManager definition
/////////////////////////////////////////////////////////////////////////////
//
/////////////////////////////////////////////////////////////////////////////
class TB_EXPORT CTabManager : public CBaseTabManager
{
	friend class CTabDialog;

	DECLARE_DYNCREATE(CTabManager)
	
	int							m_nLatestTabMadeVisible;
	int							m_FirstFocusedTab;

	CTitleButton				m_ctrlTitle;
	BOOL						m_bFirstTabAfterBatchCompleted;

public:
	CTabManager();
	CAbstractFormView*	GetFormView					();
	CParsedDialog*		GetParentParsedDialog		();
	CAbstractFormDoc*	GetDocument();

	CTabDialog* 		GetActiveDlg				() 	{ return (CTabDialog*) m_pActiveDlg; }
	CTileGroup* 		GetActiveTileGroup			();

	void 				SetTitle					(const CString& sText, COLORREF crTextColor = AfxGetThemeManager()->GetEnabledControlForeColor());

	void				PrepareTabDialogNamespaces	();

	BOOL	TabDialogActivate	(const CString& sNsTab);
	int		TabDialogActivate	(UINT nTabberID, UINT nIDD);
	int		TabDialogShow		(UINT nTabberID, UINT nIDD, BOOL /*= TRUE*/);
	int		TabDialogEnable		(UINT nTabberID, UINT nIDD, BOOL /*= TRUE*/);

	UINT	GetTabDialogID		(UINT nIDDTileGroup);
	void	DeleteTab			(DlgInfoItem* pInfo, BOOL bActivateNextTab = TRUE);

	DlgInfoItem*	AddDialog 
							(
								CRuntimeClass* pDialogClass, 
								UINT	nIDTitle, 
								int		nOrdPos = -1,
								UINT	nBeforeIDD = 0,
								const CString nsSelectorImage = _T(""), 
								const CString sSelectorTooltip = _T("")
							);
	DlgInfoItem*	AddDialog 
							(
								UINT	nIDC, 
								const	CTBNamespace& aNs,
								const	CString& sTitle,
								int		nOrdPos = -1,
								UINT	nBeforeIDD = 0,
								const CString nsSelectorImage = _T(""), 
								const CString sSelectorTooltip = _T("")
							);

public:
	virtual BOOL	PreTranslateMessage(MSG* pMsg);
	CWnd*			GetWndLinkedCtrl(const CTBNamespace& aNS);
	BOOL			PrepareAuxData();
	void SetFirstTabAfterBatchCompleted(BOOL bEnable);
	const BOOL& IsFirstTabAfterBatchCompletedEnabled() const;

	void	SetDefaultFocus();

protected:
	virtual BOOL			CreateEx		(_In_ DWORD dwExStyle, _In_ DWORD dwStyle, _In_ const RECT& rect, _In_ CWnd* pParentWnd, _In_ UINT nID);
	virtual	void			OnAttachParents	(CBaseTabDialog* pDlg);
	virtual	CBaseDocument*	GetSelectorDocument	()	{ return GetDocument(); }
		
	// serve per agganciare le TabDialog provenienti dai ClientDoc;
	virtual void	CustomizeExternal();		

	virtual BOOL	OnEnableTabSelChanging(UINT nBeforeTabIDD, UINT nAfterTabIDD) { return TRUE; }
	virtual void	OnTabSelChanged(UINT nTabIDD); 

	virtual BOOL	DispatchOnEnableTabSelChanging(UINT nBeforeTabIDD, UINT nAfterTabIDD);
	virtual void	DispatchOnAfterTabSelChanged(UINT nTabIDD);

	afx_msg	void	OnRButtonDown				(UINT nFlags, CPoint point);
	afx_msg	LRESULT	OnGetControlDescription		(WPARAM wParam, LPARAM lParam);
	afx_msg	LRESULT	OnBatchCompleted			(WPARAM wParam, LPARAM lParam);
	afx_msg void    OnSize						(UINT nType, int cx, int cy);
	afx_msg void	OnComboButtonClick			();
	afx_msg void	OnTabActivateFromMenu		(UINT nID);
	afx_msg	BOOL 	OnEraseBkgnd				(CDC* pDC);

	DECLARE_MESSAGE_MAP()
private:
	void GetVisibleTabs(int &start, int &end);
#ifdef _DEBUG
public:
	BOOL m_bCrtCheckMemoryFailed;
	virtual void AssertValid() const;
	virtual void Dump(CDumpContext& dc) const;
#endif
};

////////////////////////////////////////////////////////////////////////////////
//				class TabManagers definition
////////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT TabManagers : public Array
{
	DECLARE_DYNCREATE(TabManagers)

public:
	// overloaded operator helpers
	CTabManager*	GetAt			(int nIndex) const	{ return (CTabManager*) Array::GetAt(nIndex);}
	CTabDialog* 	GetActiveDlg	(int i) 			{ return GetAt(i)->GetActiveDlg(); }
	CTabManager*	Get				(int nIDC) const	
													{ 
														for (int i =0; i < GetSize(); i++)
															if (GetAt(i)->GetDlgCtrlID() == nIDC)
																return GetAt(i);
														return NULL;
													}
	int				GetTabManagerPos	(const CTBNamespace& aNs) const	
													{ 
														for (int i =0; i < GetSize(); i++)
															if (GetAt(i)->GetNamespace() == aNs)
																return i;
														return NULL;
													}

#ifdef _DEBUG
	virtual void AssertValid() const;
	virtual void Dump(CDumpContext& dc) const;
#endif
};                                  


//==============================================================================
//	CTabWizard definition
//==============================================================================
//
//-----------------------------------------------------------------------------
class TB_EXPORT CTabWizard : public CTabManager
{
	DECLARE_DYNCREATE(CTabWizard)

protected:
	CTabWizard();

	virtual int	 GetFirstTab(int nStartPos);
	virtual void Customize();
	virtual void OnCustomize();
	virtual BOOL PreTranslateMessage(MSG* pMsg);
			BOOL HandleAcceleratorInCtrl(const CString& strPattern, UINT nIDC);
			
public:
	virtual DlgInfoItem* AddDialog
					(
						CRuntimeClass* pDialogClass, 
						UINT	nIDTitle, 
						int		nOrdPos = -1,
						UINT	nBeforeIDD = 0
					);

	void SetWizardButtons(DWORD dwFlags);
	virtual BOOL CreateEx(_In_ DWORD dwExStyle, _In_ DWORD dwStyle, _In_ const RECT& rect, _In_ CWnd* pParentWnd, _In_ UINT nID);
	virtual void GetUsedRect(CRect &rectUsed);

	// Generated message map functions
	//{{AFX_MSG(CTabWizard)
	afx_msg void OnSize(UINT nType, int cx, int cy);
	afx_msg	void OnWindowPosChanging(WINDOWPOS FAR* wndPos);
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

/////////////////////////////////////////////////////////////////////////////
//classe da derivare per generare la tab dialog dei wizard documents
/////////////////////////////////////////////////////////////////////////////
//					CWizardTabDialog
/////////////////////////////////////////////////////////////////////////////
//

class TB_EXPORT CWizardTabDialog : public CTabDialog
{
	DECLARE_DYNAMIC(CWizardTabDialog)
private:
	BOOL m_bLast;

public:
	CWizardTabDialog(const CString& sName, UINT nIDD = 0);

public:
	void	Deactivate();
	void	Activate  ();

	LRESULT GetBitmapID();

	BOOL	IsLast();
	void	SetLast(BOOL bLast = TRUE);

public:
	virtual LRESULT OnWizardNext	();
	virtual LRESULT OnWizardBack	();
	virtual LRESULT OnWizardFinish	();
	virtual LRESULT OnWizardCancel	();

	virtual LRESULT OnGetBitmapID	();

	virtual void	OnUpdateWizardButtons() {}

	virtual void	OnActivate		(){}
	virtual void	OnDeactivate	(){}

	// Generated message map functions
	//{{AFX_MSG(CWizardTabDialog)
	afx_msg void OnSize(UINT nType, int cx, int cy);
	afx_msg void OnWindowPosChanged( WINDOWPOS* lpwndpos);
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

public:
	virtual void CustomizeExternal	();
	virtual BOOL OnInitDialog		();

protected:
	void	SetWizardButtons			(DWORD dwFlags) {((CTabWizard*)m_pParentTabManager)->SetWizardButtons(dwFlags);}
};


// classe da cui derivare le tabdialog contenenti i campi per l'esportazione
// in XML del documento. I valori di tali campi sono utilizzati per le selezioni 
// di esportazione cablate programmativamente
//----------------------------------------------------------------
//class CAppExportCriteriaTabDlg 
//----------------------------------------------------------------
class TB_EXPORT CXMLAppCriteriaTabDlg : public CWizardTabDialog
{
	DECLARE_DYNCREATE(CXMLAppCriteriaTabDlg)

private:
	UINT	m_nDialogID;

public:
	CXMLBaseAppCriteria*	m_pExportCriteria;

public:
	CXMLAppCriteriaTabDlg (const CString& sName = _T(""), UINT = -1);

public:
	UINT		GetIDD() const		{ return m_nDialogID; };
		
public:
	virtual BOOL OnOkWizardNext () { return TRUE; };
	virtual BOOL OnOkWizardBack () { return TRUE; };

	virtual LRESULT	OnGetBitmapID();
	
public:
	virtual BOOL OnInitDialog		();

};	

#include "endh.dex"
