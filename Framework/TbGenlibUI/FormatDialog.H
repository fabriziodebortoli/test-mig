#pragma once

#include <TbGeneric\DataTypesFormatters.h>
#include <TbGenlib\Parsctrl.h>
#include <TbGenlib\TbToolbar.h>
#include <TbGenlib\TbTreeCtrl.h>

//includere alla fine degli include del .H
#include "beginh.dex"

//===========================================================================
class CTreeItemRef : public CObject		// per localizzazione (parte non localizzata)
{
public: 
	CString m_strName;

public: 
	CTreeItemRef (CString strName);
};

//--------------------------------------------------------------------------
class CTreeFormatDialog : public CTBTreeCtrl 
{	
	DECLARE_DYNAMIC(CTreeFormatDialog)

protected:
	BOOL	m_bAfterCtrl;

public:
	void ExpandAll(UINT nCode);
	void ExpandAll(HTREEITEM hItem, UINT nCode);

public:
	CTreeFormatDialog();

protected:
	//{{AFX_MSG(CMultiSelTree)
	afx_msg void OnItemBeginEdit			(NMHDR*, LRESULT*);
	afx_msg void OnItemEndEdit				(NMHDR*, LRESULT*);
	afx_msg void OnRButtonDown				(UINT nFlags, CPoint point);
	afx_msg void OnContextMenu				(CWnd*, CPoint);
	afx_msg void OnKeyDown					(UINT nChar, UINT nRepCnt, UINT nFlags);
	afx_msg void OnKeyUp					(UINT nChar, UINT nRepCnt, UINT nFlags);

public:
	afx_msg void OnOpen						();
	afx_msg void OnDelete					();
	afx_msg void OnCopy						();
	afx_msg void OnPaste					();
	afx_msg void OnRename					();
	afx_msg void OnCut						();
	afx_msg void OnContextArea				();
	afx_msg void OnRefreshFormatter			();

	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()	
};

//===========================================================================
class TB_EXPORT CFormatDlg : public CParsedDialog
{
	DECLARE_DYNAMIC(CFormatDlg)
	friend class CTreeFormatDialog;
	friend class CFormatDlgRoot;

protected:
	CWnd*				m_pWndParent;

	FormatStyleTable&	m_SourceStyleTable;
	FormatStyleTable	m_StyleTable;
	FormatIdx&			m_FormatIdx;
	Formatter*			m_FormatCopy;
	DataType			m_ItemType;
	BOOL				m_bIgnoreIdx;
	BOOL				m_bModified;
	Array				m_arTreeItemRef;
	BOOL				m_bFormCut;
	CTBNamespace		m_NsForWoorm;
	BOOL				m_bFromTree;
	HTREEITEM			m_hSelCut;
	HTREEITEM			m_DefaultSel;

	CTreeFormatDialog	m_treeStyle;
	CImageList			m_ImageList;
	CBCGPComboBox		m_CmbStyle;
	CString				m_sFilterStyle;
	BOOL				m_bFilterTree;
	BOOL				m_bSelezionaAsOk;
	BOOL				m_bFontSave;

public:
	CFormatDlg (FormatStyleTable&, FormatIdx&, BOOL ignoreIdx = FALSE, CWnd* aParent = NULL, const CTBNamespace& NsForWoorm = CTBNamespace(), BOOL bSelezionaAsOk = FALSE);
	~CFormatDlg ();

protected:
	void	LoadImageList			();
	void	FillTreeCtrlStyle		();
	void	FillTreeAddModules		(const CString strApps, HTREEITEM hItemApp);
	void    FillTreeAddFormats		(const CString& strApp, const CString& strMod, HTREEITEM hParentItem);

	void	FillComboStyle			();

	BOOL	HasApplicationFormatters(const CString& strApps, const CString& strMod, const CString& sFilterStyle);
	BOOL	HasReportFormatters		(const CString& strApps, const CString& strMod, const CString& sFilterStyle);

	BOOL	CanCopy				();
	BOOL	CanPaste			();
	BOOL	CanApplyContextArea ();
	BOOL	CanDelete			();
	BOOL	CanOpen				()	{return CanCopy();}

	BOOL	IsTreeCtrlEditMessage	(WPARAM KeyCode);

	void    CopyStyle			();
	void	OpenStyle			();	
	void	ContextArea			();	
	void	DeleteStyle			();
	void	PasteStyle			();
	int		AddNewStyle			(CTBNamespace* Ns);
	void	SetLocalizedName	(Formatter* pFormatter);
	void	InsertInTree		(Formatter* pFormatter);
	BOOL	RemoveCustomStyle	(Formatter* pFormatter, BOOL bFromRename = FALSE);
	void	RenameStyle			(const CString& strSelItem);
	void	OnRemaneLabel		();
	CString	GetNewFormatName	(Formatter* pFormat);

	void	EnableDisableToolbar		();

	void	DeleteWoormFormatter		(FormatStyleTablePtr StyleTable);
	void	UpdateSourceStyleTable		();

	int			GetItemLevel			(HTREEITEM hItem);
	HTREEITEM	GetReportTreeItem		();

	BOOL		RefreshFormatsTable		();
	void		SetDefaultSelection		();

protected:
	virtual BOOL	OnInitDialog		();
	virtual void	OnOK				();
	virtual void	OnCancel			();
	virtual BOOL	PreTranslateMessage	(MSG* pMsg);
	virtual BOOL	GetToolTipProperties(CTooltipProperties& tp);
	virtual void	OnCustomizeToolbar();

private:
	BOOL OnOkFormatForSelect(Formatter* pSelFormat, BOOL bSave);
	void DoSaveFormatStyles	();

protected:
	// Generated message map functions
	//{{AFX_MSG(CFormatDlg)
	afx_msg	void OnNMDblclkTree				(NMHDR *pNMHDR, LRESULT *pResult);
	afx_msg void OnTreeSelchanged			(NMHDR *pNMHDR, LRESULT *pResult);
	afx_msg	void OnSaveFormatStyles			();
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
    //}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

#include "endh.dex"
