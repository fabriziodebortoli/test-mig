
#pragma once

#include <atlimage.h>
#include <TbGeneric\array.h>
#include "parsobj.h"
#include "oslinfo.h"
#include "TabSelector.h" 

// Includere alla fine degli include del .H
#include "beginh.dex"

/////////////////////////////////////////////////////////////////////////////
class CBaseTabManager;
class CBaseTabDialog;
class DlgInfoArray;
class DlgInfoItem;
class CTabDialog;
class CGridControlCellPos;
class CJsonContextObj;
/////////////////////////////////////////////////////////////////////////////

//////////////////////////////////////////////////////////////////////////////
//							DlgInfoItem definition
//////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------

class TB_EXPORT DlgInfoItem : public CObject, public IOSLObjectManager 
{
	DECLARE_DYNAMIC (DlgInfoItem)
	friend class DlgInfoArray;
	friend class CBaseTabManager;
	template<class T> friend class TJsonTabManager;
	friend class CJsonTileManager;
	friend class CJsonFormEngine;

public:
	CStringList				m_strlistChildren;
	CString					m_strTitle;
	int						m_nIconIndex;
	CBaseTabDialog*			m_pBaseTabDlg;
	CString					m_strBkgndImage;	

protected:
	CRuntimeClass*			m_pDialogClass;
	HINSTANCE				m_hResourceModule;
	UINT					m_nDialogID;
	CGridControlCellPos*	m_pCellPos;	// posizione della cella (se il control di cui sopra
										// e` un CGridControl)
	int						m_nOrdPos;	// indice ordine di posizione

	BOOL					m_bEnabled;
	BOOL					m_bVisible;

private:
	UINT		m_nLastFocusIDC;
	CString		m_nsSelectorImage;
	CString		m_sSelectorTooltip;
	CJsonContextObj* m_pJsonContext;//puntatore alla struttura della dialog derivante da JSON

public:
	DlgInfoItem	(CRuntimeClass*	pClass, UINT nDialogID, int nOrdPos = -1 );
	DlgInfoItem	(CRuntimeClass*	pClass, UINT nDialogID, const CTBNamespace& sNamespace, const CString& sTitle, int nOrdPos = -1 );
	virtual ~DlgInfoItem ();

public:
	UINT					GetDialogID		() const	{ return m_nDialogID; }
	UINT					GetLastFocusIDC	() const	{ return m_nLastFocusIDC; }
	CGridControlCellPos*	GetCellPos		() const	{ return m_pCellPos; }
	__declspec(deprecated("\nUSE GetDialogID to obtain dialog ID, or m_strTitle to obtain dialog title")) UINT GetCaptionID	() const	{ return GetDialogID(); }
	int						GetOrdPos		() const	{ return m_nOrdPos; }
	CRuntimeClass*			GetDialogClass	() const	{ return m_pDialogClass; }
	CString					GetSelectorImage() const	{ return m_nsSelectorImage; }
	CString					GetSelectorTooltip() const	{ return m_sSelectorTooltip; }
	HINSTANCE				GetResourceModule() const	{ return m_hResourceModule; }
	void					SetResourceModule(HINSTANCE hInstance);
	BOOL			IsEnabled() const		{ return m_bEnabled; }
	BOOL			IsVisible() const		{ return m_bVisible; }		
	void			SetVisible(BOOL bSet) 	{ m_bVisible = bSet; }		

	CTBNamespace&	GetNamespace ()			{ return GetInfoOSL()->m_Namespace; }
	const CJsonContextObj* GetJsonContext() const { return m_pJsonContext; }
	void SetDialogID		(UINT nID);
	void SetLastFocusIDC	(UINT nIDC);
	void SetSelectorImage	(const CString& nsSelectorImage);
	void SetSelectorTooltip	(const CString& sSelectorTooltip);

	void SetCellPos		(const CGridControlCellPos& cp);
	void SetCellPos		(CGridControlCellPos* pCellPos);
	void SetOrdPos		(const int& nOrdPos);

private:
	void InitJsonContext();
// diagnostics
#ifdef _DEBUG
public:	
	void Dump(CDumpContext&) const;
	void AssertValid() const;
#endif
};

//////////////////////////////////////////////////////////////////////////////
//							DlgInfoArray definition
//////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
class TB_EXPORT DlgInfoArray : public Array
{
	DECLARE_DYNAMIC (DlgInfoArray)
	friend class CBaseTabManager;
public:	
	BOOL		m_bPrepared;

public:
	DlgInfoArray() : m_bPrepared(FALSE) {}

	DlgInfoItem* 	GetAt		(int nIndex)const	{ return (DlgInfoItem*) Array::GetAt(nIndex);	}
	DlgInfoItem*&	ElementAt	(int nIndex)		{ return (DlgInfoItem*&) Array::ElementAt(nIndex); }
	
	DlgInfoItem* 	operator[](int nIndex)	const	{ return GetAt(nIndex);	}
	DlgInfoItem*& 	operator[](int nIndex)			{ return ElementAt(nIndex);	}
	
	int				Find		(const CString& sName) const;
	bool			Exists		(const CString& sName) const { return Find(sName) >= 0; }

// diagnostics
#ifdef _DEBUG
	void Dump(CDumpContext&) const;
	void AssertValid() const;
#endif
};

///////////////////////////////////////////////////////////////////////////////
//								ITabDialogTileGroup
///////////////////////////////////////////////////////////////////////////////
class TB_EXPORT ITabDialogTileGroup
{
public:
	ITabDialogTileGroup() {}
};

/////////////////////////////////////////////////////////////////////////////
//					class CBaseTabDialog definition
/////////////////////////////////////////////////////////////////////////////
//
/////////////////////////////////////////////////////////////////////////////
class TB_EXPORT CBaseTabDialog : public CParsedDialog
{
	friend class CBaseTabManager;
	DECLARE_DYNAMIC(CBaseTabDialog)
	
	Gdiplus::Bitmap* m_pBkgnd;

protected :
	int				m_nCurrTabPos;
	DlgInfoItem*	m_pDlgInfo;
	BOOL			m_bOwnsDlgInfo;

public :
	CBaseTabDialog (const CString& sName, UINT nIDD=0);
	~CBaseTabDialog ();
	virtual BOOL			Create					(CBaseTabManager* pParentWnd);

	// for custom cleanup after WM_NCDESTROY
	virtual void PostNcDestroy();

	DlgInfoItem*	GetDlgInfoItem	() const { return m_pDlgInfo; };
	void			SetLastFocusIDC	(UINT nIDC) { if (m_pDlgInfo) m_pDlgInfo->SetLastFocusIDC(nIDC); }
	
	void RefreshBackgroundImage();

	int	GetCurrentTabPos()					{ return m_nCurrTabPos; }

	void GetUsedRect(CRect &rectUsed);
	
protected:
	virtual	BOOL PreProcessMessage	(MSG* pMsg);
	virtual void OnOK				();
	virtual void OnCancel			();

protected:
	//{{AFX_MSG(CBaseTabDialog)
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

#ifdef _DEBUG                                                     
public:
	virtual void AssertValid() const;
	virtual void Dump(CDumpContext& dc) const;
#endif
};

/////////////////////////////////////////////////////////////////////////////
//					class CBaseTabManager definition
/////////////////////////////////////////////////////////////////////////////
//
#define TABCTRL_ABORT_SELCHANGING TRUE
#define TABCTRL_CONFIRM_SELCHANGING FALSE
class TB_EXPORT CBaseTabManager : public CTabSelector, public ResizableCtrl, public LayoutElement, public IDisposingSourceImpl, public IOSLObjectManager 
{
	DECLARE_DYNAMIC(CBaseTabManager)

protected:
    DlgInfoArray*		m_pDlgInfoAr;
	CBaseTabDialog* 	m_pActiveDlg;
    CWnd*				m_pParent;
	DWORD				m_dwStyle;
	DWORD				m_dwStyleEx;
	COLORREF			m_crBkgColor;
	CBrush*				m_pBkgColorBrush;
	CImageList			m_imgList;
	BOOL				m_bKeepTabDlgAlive;
	BOOL				m_bDefaultSequence;
	CBaseDocument*      m_pDocument;
public:
	CTBNamespace&		GetNamespace () { return GetInfoOSL()->m_Namespace; }
	void				SetKeepTabDlgAlive(BOOL bSet);
	BOOL				IsKeepingTabDlgAlive () { return m_bKeepTabDlgAlive; }
	void				SetDefaultSequence(BOOL bSet) { m_bDefaultSequence = bSet; } //bugfix 21431 (vedi wizard)

public:
	CBaseTabManager();
	CBaseTabManager(DWORD dwStyle, DWORD dwStyleEx);
	virtual ~CBaseTabManager();
	void	RepositionCurrentDlg();

protected:
    void	AdjustCurrentDlgRect	(CRect* pRect);
	BOOL	CreateTabDialog			(int nPos);
	BOOL	SetCtrlFocus			(int nActiveTab);

	int		TabDialogShow		(UINT nIDD, BOOL bShow);
	int		TabDialogEnable		(UINT nIDD, BOOL bEnable);

public:
	int		GetTabDialogPos		(UINT nIDD);
	int		GetTabDialogPos		(const CTBNamespace& aNs);
	UINT	GetTabDialogIDD		(int npos);
	int		TabDialogRefresh	();

	int		TabDialogActivate	(UINT nIDD);

	void	OnInitialUpdate		(UINT nIDD, CWnd* pParentWnd, BOOL bCallCustomize = TRUE);
	int		AddDialog			(CRuntimeClass*, UINT nDialogID, int nOrdPos = -1, const CString nsSelectorImage = _T(""), const CString sSelectorTooltip = _T(""));
	int		AddDialog			(CRuntimeClass*, UINT nDialogID, const CTBNamespace& aNs, const CString& sTitle, int nOrdPos = -1, const CString nsSelectorImage = _T(""), const CString sSelectorTooltip = _T(""));
	void	SetTabDialogImage	(UINT nDialogID, const CString nsSelectorImage);
	
	CBaseDocument*	GetDocument		() { return m_pDocument; }
	void			AttachDocument	(CBaseDocument* pDocument) { m_pDocument = pDocument; }
	CWnd*			GetParent		()	const { return m_pParent; }

	DlgInfoArray*	GetDlgInfoArray	() const { return m_pDlgInfoAr; }

	UINT	GetActiveTabID();
	void	InitTabNamespaces();

protected:
	// overridable
	virtual	void	Customize			()				 	{ /* do nothing	*/ }
	virtual	void	CustomizeExternal	()				 	{ /* do nothing	*/ } // per i ClientDoc (vedi Tabber.h)
	virtual	void	OnAttachParents		(CBaseTabDialog*)	{ /* do nothing*/ }
	virtual	void	OnCustomize			();

	virtual DlgInfoItem* CreateDlgInfoItem(CRuntimeClass*	pDialogClass, UINT nDialogID, int nOrdPos = -1);

public:
	virtual	BOOL	PreTranslateMessage	(MSG* pMsg);

	virtual CBaseTabDialog*	GetActiveDlg() { return (m_pActiveDlg && (int(GetActiveTab()) == m_pActiveDlg->m_nCurrTabPos)) ? m_pActiveDlg : NULL;}
	void			ClearActiveDlg () { m_pActiveDlg = NULL; }

	
	COLORREF GetBackColor();
	void SetBackColor(COLORREF cr) ;

	int GetTabCount() ;

	UINT GetActiveTab();
	void SetActiveTab(int /*nActiveTab*/) ;

	void ChangeTabTitle(UINT nIDDTab,  const CString& sTitle, int nIconIndex = -1);
	void AdjustTabManager();


protected:
	// Generated message map functions
	virtual	BOOL	OnCommand		(WPARAM wParam, LPARAM lParam);

	virtual BOOL	DispatchOnEnableTabSelChanging(UINT nBeforeTab, UINT nAfterTab) { return TRUE; }
	virtual void	DispatchOnAfterTabSelChanged(UINT nTab) { }

	virtual BOOL	OnEnableTabSelChanging(UINT nBeforeTab, UINT nAfterTab) { return TRUE; }
	virtual void	OnAfterTabSelChanged(UINT nTab) { }
			
	virtual	int		InsertDlgInfoItem(int, DlgInfoItem*);
	virtual int		GetFirstTab(int nStartPos);

			void	ShowTab(int nPos, BOOL bEnable);

	
private:
			DlgInfoItem*	CreateDialogObj	(UINT nPos);
			BOOL			DoTabSelChanging	(); 
			void			DoTabSelChange		();
		
protected:
			int GetDlgItemPos(int nVisiblePos);
			CString GetTabIconSource(UINT iDialogID);
public:
			int GetDlgItemNextPos(int nVisiblePos, BOOL OnlyEnabled = FALSE);
			int GetDlgItemPrevPos(int nVisiblePos, BOOL OnlyEnabled = FALSE);

			int GetTabIndexFromItemPos(int nPos);

public:
	// LayoutElement interface implementation
	virtual const	CString				GetElementNameSpace	() { return GetInfoOSL()->m_Namespace.ToString(); }
	virtual const	LayoutElementArray*	GetContainedElements()										{ return NULL;}
	virtual			BOOL				IsVisible			()										{ return TRUE; }
	virtual			int					GetRequiredHeight	(CRect &rectAvail);
	virtual			int					GetRequiredWidth	(CRect &rectAvail);
	virtual			void				GetAvailableRect	(CRect &rectAvail);
	virtual			void				Relayout			(CRect &rectNew, HDWP hDWP = NULL);
	virtual			void				GetUsedRect			(CRect &rectUsed);

public:
	// Accessibility - Method used to uniquely identify an object by Ranorex Spy
	virtual CString GetRanorexNamespace();

protected:
	virtual	BOOL	CanDoLastFlex	(FlexDim fd);
	virtual	int		GetMinHeight	(CRect& rect = CRect(0, 0, 0, 0));

private:
   //afx_msg LRESULT OnTabActivate (WPARAM wParam, LPARAM lParam);
	afx_msg BOOL OnTabSelChanging(NMHDR* pNMHDR, LRESULT* pResult); //ON_NOTIFY_REFLECT_EX(TCN_SELCHANGING, ...

	//afx_msg LRESULT OnTabShown (WPARAM wParam, LPARAM lParam);
	afx_msg BOOL OnTabSelChange ( NMHDR* pNMHDR, LRESULT* result );

	afx_msg	void OnLButtonDown (UINT nFlags, CPoint point);

protected:
	afx_msg LRESULT	OnCtrlFocused		(WPARAM, LPARAM);
	afx_msg LRESULT OnValueChanged		(WPARAM, LPARAM);
	afx_msg	void 	OnSetFocus			(CWnd*);
	afx_msg	void	OnWindowPosChanged	(WINDOWPOS FAR*);
	afx_msg	LRESULT	OnRecalcCtrlSize	(WPARAM, LPARAM);
	afx_msg	LRESULT	OnActivateTabPage	(WPARAM, LPARAM);
	
	afx_msg void OnMouseMove(UINT nFlags, CPoint point);
	afx_msg void	OnSize				(UINT nType, int cx, int cy);


	DECLARE_MESSAGE_MAP()

#ifdef _DEBUG
	virtual void AssertValid() const;
	virtual void Dump(CDumpContext& dc) const;
#endif
};


#include "endh.dex"
