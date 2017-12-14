#pragma once

#include <TbGeneric\FontsTable.h>

#include <TbGenlib\parsctrl.h>
#include <TbGenlib\TbToolbar.h>
#include <TbGenlib\TbTreeCtrl.h>

//includere alla fine degli include del .H
#include "beginh.dex"

//===========================================================================
class CTreeFontItemRef : public CObject		// per localizzazione (parte non localizzata)
{
public: 
	CString m_strName;

public: 
	CTreeFontItemRef (CString strName);
};

//--------------------------------------------------------------------------
class CTreeFontDialog : public CTBTreeCtrl 
{	
	DECLARE_DYNAMIC(CTreeFontDialog)

public:
	void ExpandAll(UINT nCode);
	void ExpandAll(HTREEITEM hItem, UINT nCode);

protected:
	BOOL				m_bAfterCtrl;

public:
	CTreeFontDialog();

protected:
	//{{AFX_MSG(CMultiSelTree)
	afx_msg void OnItemBeginEdit		(NMHDR*, LRESULT*);
	afx_msg void OnItemEndEdit			(NMHDR*, LRESULT*);
	afx_msg void OnRButtonDown			(UINT nFlags, CPoint point);
	afx_msg void OnContextMenu			(CWnd*, CPoint);
	afx_msg void OnKeyDown				(UINT nChar, UINT nRepCnt, UINT nFlags);
	afx_msg void OnKeyUp				(UINT nChar, UINT nRepCnt, UINT nFlags);

public:
	afx_msg void OnOpen					();
	afx_msg void OnSetEscapement		();
	afx_msg void OnDelete				();
	afx_msg void OnCopy					();
	afx_msg void OnPaste				();
	afx_msg void OnRename				();
	afx_msg void OnCut					();
	afx_msg void OnContextArea			();
	afx_msg void OnRefreshFont			();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()	
};

//===========================================================================
class TB_EXPORT CFontStylesDlg : public CParsedDialog
{
	friend class CTreeFontDialog;
	friend class CFontDlgRoot;
	DECLARE_DYNAMIC(CFontStylesDlg)
public:
	BOOL				m_bEnableApplyAll;
	BOOL				m_bApplyAll;

protected:
	CWnd*				m_pWndParent;
	FontStyleTable&		m_SourceStyleTable;
	FontStyleTable		m_StyleTable;
	FontIdx&			m_FontIdx;
	FontStyle*			m_pFontCopy;
	BOOL				m_bIgnoreIdx;
	BOOL				m_bModified;
	Array				m_arTreeItemRef;
	BOOL				m_bFormCut;
	CTBNamespace		m_NsForWoorm;
	BOOL				m_bFromTree;
	BOOL				m_bShowPrinterFont;
	HTREEITEM			m_hSelCut;
	HTREEITEM			m_DefaultSel;

	CTreeFontDialog		m_treeStyle;
	CImageList			m_ImageList;
	CBCGPComboBox		m_CmbStyle;
	CString				m_sFilterStyle;
	BOOL				m_bFilterTree;
	BOOL				m_bSelezionaAsOk;
	CString				m_strPreferredPrinter;
	BOOL				m_bShowDefaultFont;

	BOOL				m_bFontSave;

public:
	CFontStylesDlg (FontStyleTable&, FontIdx&, BOOL ignoreIdx = FALSE, CWnd* aParent = NULL, const CTBNamespace& NsForWoorm = CTBNamespace(), BOOL bSelezionaAsOk = FALSE, BOOL	bShowDefaultFont = FALSE);
	~CFontStylesDlg ();

	virtual BOOL OnCmdMsg(UINT nID, int nCode, void* pExtra, AFX_CMDHANDLERINFO* pHandlerInfo);

protected:
	void	LoadImageList			();
	void	FillTreeCtrlStyle		();
	void	FillTreeAddModules		(const CString strApps, HTREEITEM hItemApp);
	void    FillTreeAddFonts		(const CString& strApp, const CString& strMod, HTREEITEM hParentItem);
	void    FillTreeAddReportFonts	();

	void	FillComboStyle			();

	BOOL	HasApplicationFonts		(const CString& strApps, const CString& strMod, const CString& sFilterStyle);
	BOOL	HasReportFonts			(const CString& strApps, const CString& strMod, const CString& sFilterStyle);

	BOOL	CanCopy				();
	BOOL	CanPaste			();
	BOOL	CanApplyContextArea	();
	BOOL	CanDelete			();
	BOOL	CanOpen				()	{return CanCopy();}

	BOOL	IsTreeCtrlEditMessage	(WPARAM KeyCode);

	void    CopyStyle			();
	void	OpenStyle			();	
	void	ContextArea			();	
	void	DeleteStyle			();
	void	PasteStyle			();
	int		AddNewStyle			(CTBNamespace* Ns);
	void	InsertInTree		(FontStyle* pFont);
	BOOL	RemoveCustomStyle	(FontStyle* pFont, BOOL bFromRename = FALSE);
	void	RenameStyle			(const CString& strSelItem);
	void	OnRemaneLabel		();
	CString	GetNewFontName		(FontStyle* pFont);

	void	EnableDisableToolbar();

	void	ApplyFontChange		(FontStyle* pNewFont, FontStyle* pOldFont);

	void	DeleteWoormFont			(FontStyleTablePtr StyleTable);
	void	UpdateSourceStyleTable	();

	int			GetItemLevel		(HTREEITEM hItem);
	HTREEITEM	GetReportTreeItem	();
	HTREEITEM	GetTplReportTreeItem	();

	BOOL	RefreshFontsTable		();
	void	SetDefaultSelection		();

protected:
	virtual BOOL	OnInitDialog		();
	virtual void	OnOK				();
	virtual void	OnCancel			();
	virtual BOOL	PreTranslateMessage	(MSG* pMsg);
	virtual BOOL	GetToolTipProperties(CTooltipProperties& tp);
	virtual void	OnCustomizeToolbar	();
private:
	BOOL OnOkFontForSelect	(FontStyle* pStyle, BOOL bSave);
	void DoSaveFontStyles	();

protected:
	// Generated message map functions
	//{{AFX_MSG(CFontStylesDlg)

	afx_msg void OnUpdatePrinterFont	(CCmdUI* pCmdUI);
	
	afx_msg	void OnNMDblclkTree				(NMHDR *pNMHDR, LRESULT *pResult);
	afx_msg void OnTreeSelchanged			(NMHDR *pNMHDR, LRESULT *pResult);
	afx_msg	void OnSaveFontStyles			();
	afx_msg void OnShowPrinterFont			();
	afx_msg	void OnComboStyleChanged		();
	afx_msg void OnOpen						();
	afx_msg void OnDelete					();
	afx_msg void OnCopy						();
	afx_msg void OnPaste					();
	afx_msg void OnRename					();
	afx_msg void OnCut						();
	afx_msg void OnContextArea				();
	afx_msg void OnFilterTree				();
	afx_msg void OnHelp						();
	afx_msg void OnApplyAllColum			();

    //}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

#include "endh.dex"
