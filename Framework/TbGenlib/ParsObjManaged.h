
#pragma once

#include <afxhtml.h>

#include <TbGenlibManaged/ParsedUserControl.h>

#include <TbGenlibManaged/GanttWrapper.h>
#include <TbGenlibManaged/TreeViewAdvWrapper.h>
#include <TbGenlibManaged/TBPicViewerAdvWrapper.h>
#include <TbGenlib\ToolTipCtrl.h>

//includere alla fine degli include del .H
#include "beginh.dex"

//=============================================================================
//			Class CManagedParsedCtrl
//=============================================================================
class TB_EXPORT CManagedParsedCtrl : public CWnd, public CParsedCtrl, public ResizableCtrl, public IDisposingSourceImpl
{
	DECLARE_DYNAMIC (CManagedParsedCtrl)
protected:
	UINT		m_nID = 0;
	WNDPROC		m_ManagedWndProc = NULL;
public:
	CManagedParsedCtrl ();
	virtual ~CManagedParsedCtrl ();

	virtual	BOOL	SubclassEdit	(UINT, CWnd*, const CString& strName = _T(""));
	
	UINT	GetDlgID() const { return m_nID; }
	LRESULT WndProc(UINT msg, WPARAM wParam, LPARAM lParam);
protected:
	virtual	BOOL	Create			(DWORD dwStyle, const RECT& rect, CWnd* pParentWnd, UINT nID);
	virtual	void	SetValue		(const DataObj& aValue);
	virtual	void	GetValue		(DataObj& aValue);
	virtual void	InitSizeInfo	();
	virtual BOOL	EnableCtrl		(BOOL bEnable = TRUE);

	virtual DataType GetDataType		() const;

	// methods to manage Managed User control to override
	virtual	BOOL	OnCreateManaged		(DWORD dwStyle, const RECT& rect, CWnd* pParentWnd, UINT nID) = 0;
	virtual BOOL	OnEnableCtrlManaged	(BOOL bEnable = TRUE) = 0;
	virtual HWND	GetManagedHandle	() = 0;
	virtual void	OnAfterAttachControl();
	BOOL SubclassWindow(HWND hWnd);
	afx_msg	LRESULT	OnRecalcCtrlSize	(WPARAM, LPARAM);

	DECLARE_MESSAGE_MAP ();
};

//=============================================================================
//			Class CManagedCtrl
//=============================================================================
class AttachControlEventArg;

class TB_EXPORT CManagedCtrl : public CManagedParsedCtrl, public CParsedUserControlWrapper
{
	DECLARE_DYNCREATE (CManagedCtrl)

public:
	CManagedCtrl ();

public:
	virtual void AttachMangedCtrl	(AttachControlEventArg* pArg);
	virtual	BOOL OnCreateManaged	(DWORD dwStyle, const RECT& rect, CWnd* pParentWnd, UINT nID);
	virtual BOOL OnEnableCtrlManaged(BOOL bEnable = TRUE);
	virtual HWND GetManagedHandle	();

	virtual	void		SetValue		(const DataObj& aValue);
	virtual	void		GetValue		(DataObj& aValue);
	virtual BOOL		IsValid			();
	virtual BOOL		IsValid			(const DataObj& aValue);
	virtual DataType	GetDataType		() const;
	virtual void		OnAfterAttachControl	();
	// mappa eventi
	virtual void PerformLosingFocus ();
	virtual void PerformTextChanged ();
};

//=============================================================================
//			Class CGanttCtrl
//=============================================================================
class TB_EXPORT CGanttCtrl : public CManagedParsedCtrl, public CGanttWrapper
{
	DECLARE_DYNCREATE (CGanttCtrl)

public:
	CGanttCtrl ();

public:
	virtual	BOOL OnCreateManaged		(DWORD dwStyle, const RECT& rect, CWnd* pParentWnd, UINT nID);
	virtual BOOL OnEnableCtrlManaged	(BOOL bEnable = TRUE);
	virtual HWND GetManagedHandle		();
	virtual void OnAfterAttachControl	();
protected:      
	//{{AFX_MSG(CParsedBitmap)
	
	DECLARE_MESSAGE_MAP ();
};

#ifdef NEWTREE

class TreeNodeKey;
class TreeNodeMap;

//=============================================================================
//			Class CTreeViewAdvCtrl
//=============================================================================
class TB_EXPORT CTreeViewAdvCtrl : public CTBTreeCtrl, public CParsedCtrl, public CTreeViewAdvWrapper
{
	DECLARE_DYNCREATE (CTreeViewAdvCtrl)

public:
	CTreeViewAdvCtrl ();

	void	SelectNode			(CString keyNode);
	void	ToggleNode			(CString keyNode);

	virtual	void		SetValue(const DataObj& aValue);
	virtual	void		GetValue(DataObj& aValue);
	virtual BOOL		IsValid();
	virtual BOOL		IsValid(const DataObj& aValue);
	virtual DataType	GetDataType() const;
	virtual	BOOL		SubclassEdit(UINT, CWnd*, const CString& strName = _T(""));
	virtual BOOL		EnableCtrl(BOOL bEnable = TRUE);
	virtual	void		InitializeImageList();

private:
	INT				FindImage(const CString& sImageKey);
	TreeNodeKey*	FindNnode(const CString& sNodeKey);
	INT			GetIndentLevel(HTREEITEM hItem);

public:
	void	AddImage(const CString& sImageKey, const CString& sImagePath);
	void	SetImage(const CString& sNodeKey, const CString& sImageKey);
	void 	AddControls();
	void	AddHidingCommand();
	void	AddResizeControl();
	void	SetAllowDrop(const BOOL& bValue = FALSE);
	void 	SetViewContextMenu(const BOOL& bValue = FALSE);
	void 	SetDragAndDropOnSameLevel(const BOOL& bValue = FALSE);
	void 	ClearTree();
	void 	SetNodeStateIcon(const BOOL& bValue = FALSE);
	BOOL	AddNode(const CString& sNodeText, const CString& sNodeKey, const CString& sNodeKeyImage = _T(""), const COLORREF color = AfxGetThemeManager()->GetTreeViewNodeForeColor());
	BOOL	AddNode(const CString& sNodeText, const CString& sNodeKey, const CStringArray& sToolTipText, const CString& sNodeKeyImage = _T(""), const COLORREF color = AfxGetThemeManager()->GetTreeViewNodeForeColor());
	BOOL	AddNode(const CString& sNodeText, const CString& sNodeKey, const BOOL& checkBoxNode);
	BOOL	AddChild(const CString& sNodeText, const CString& sNodeKey, const CString& sNodeKeyImage = _T(""), const COLORREF color = AfxGetThemeManager()->GetTreeViewNodeForeColor());
	BOOL	AddChild(const CString& sNodeText, const CString& sNodeKey, const CStringArray& sToolTipText, const CString& sNodeKeyImage = _T(""), const COLORREF color = AfxGetThemeManager()->GetTreeViewNodeForeColor());
	BOOL	InsertChild(const CString& sParentKey, const CString& sNodeText, const CString& sNodeKey, const CString& sNodeKeyImage = _T(""), const COLORREF color = AfxGetThemeManager()->GetTreeViewNodeForeColor());
	BOOL	InsertChild(const CString& sParentKey, const CString& sNodeText, const CString& sNodeKey, const CStringArray& sToolTipText, const CString& sNodeKeyImage = _T(""), const COLORREF color = AfxGetThemeManager()->GetTreeViewNodeForeColor());
	void	SetSelectRoot();
	void	SetCustomToolTip(const COLORREF bkColor, const COLORREF foreColor);
	BOOL	HasChildrenSelectedNode(const BOOL& bValue = FALSE);
	void	SetSelectFirstChild();
	int		GetSelectedNodeLevel();
	void	GetAllParentsTextNodeFromSelected(CArray<CString>& m_AllParents);
	CString GetSelectedNodeKey();
	void	RemoveContextMenuItem(int idxMenuItem);
	CSize	GetSize();
	void	SetImageNoSel(const CString& sNodeKey, const CString& sImageKey);
	void	ExpandAll();
	void	CollapseAll();
	void	ExpandLevels(INT nLevel);
	BOOL	SetNodeAsSelected(const CString& sNodeKeyToSearch);
	void	EnableToolBarCommand(int id, bool bEnable);
	void	SetUpdateTextNode(const int& nrNode, const CString& text);
	void	SetUpdateTextNode(const CString& nodeKey, const CString& text);
	virtual void OnToolBarCommand(int cmdId);
	void	AddToolBarCommand(int id, const CString& sImage, CString sToolTip = L"", TCHAR nAccelCharCode = 0, BOOL bCtrlModifier = FALSE, BOOL bShiftModifier = FALSE, BOOL bAltModifier = FALSE, ToolBarButtonStyleWrapper eButtonStyle = E_PUSH_BUTTON);
	void	SetSelectionMode(const E_SELECTIONMODE eSelectionMode);
	HMENU	GetContextMenuHandle();
	void	AddContextSubMenuItem(const CString& itemMenu, CArray<CString>& subItems);
	void	SetToolbarButtonSize(int nButtonSize);
	void	SetToolBarButtonPushed(int id, bool bPush);
	bool	IsToolBarButtonPushed(int id);
	CTreeNodeAdvWrapperObj*	GetNode(const CString& sNodeKey);
	void	EnsureVisible(const CString& sNodeKey);
	void	ShowToolBarCommand(int id, bool bShow);
	void	SetUseNodesCache(BOOL bUseNodesCache);
	void	SetDblClickWithExpandCollapse(const BOOL& bValue = FALSE);
	BOOL	IsExpandedSelectedNode();
	void 	AddContextMenuItem(const CString& menuItem);
	void	AddContextMenuItemDisabled(const CString& menuItem);
	void	AddContextMenuItemWithConfirm(const CString& menuItem);
	void 	CollapseAllFromSelectedNode();
	void 	ExpandAllFromSelectedNode();
	BOOL 	ExistsSelectedNode();
	void	AddContextMenuSeparator();
	void	SetCancelDragDrop();
	CString GetParentKey(const CString& sNodeKeyToSearch);
	CString	GetNewParentKey();
	int	 	GetIdxContextMenuItemClicked();
	void	SetBackColorTreeView(const COLORREF color = AfxGetThemeManager()->GetTreeViewBkgColor());
	void	SetMenuItemCheck(const CString& itemMenu, BOOL check = TRUE);
	void	SetMenuItemEnable(const CString& itemMenu, BOOL enabled = TRUE);
	BOOL	ExistsNode(const CString& sNodeKeyToSearch);
	void	GetTextContextMenuItemClicked(CString& itemKey);
	BOOL	RemoveNode(const CString& sNodeKeyToSearch);
	void	SetStyleForNode(const CString& nodeKey, const BOOL& bBold = FALSE, const BOOL& bItalic = FALSE, const BOOL& bStrikeOut = FALSE, const BOOL& bUnderline = FALSE, const COLORREF foreColor = AfxGetThemeManager()->GetTreeViewNodeForeColor());
	void	SetStyleForSelectedNode(const BOOL& bBold = FALSE, const BOOL& bItalic = FALSE, const BOOL& bStrikeOut = FALSE, const BOOL& bUnderline = FALSE);
	void	AddToSelectedAndSetNodeAsCurrent(const CString& sNodeText, const CString& sNodeKey);
	void	AddToSelectedAndSetNodeAsCurrent(const CString& sNodeText, const CString& sNodeKey, const CString& sNodeKeyImage);
	void	AddToSelectedAndSetNodeAsCurrent(const CString& sNodeText, const CString& sNodeKey, const CStringArray& sToolTipText, const CString& sNodeKeyImage);
	void	AddAndSetNewNodeFromCurrent(const CString& sNodeText, const CString& sNodeKey);
	void	AddAndSetNewNodeFromCurrent(const CString& sNodeText, const CString& sNodeKey, const CString& sNodeKeyImage);
	void	AddAndSetNewNodeFromCurrent(const CString& sNodeText, const CString& sNodeKey, const CStringArray& sToolTipText, const CString& sNodeKeyImage);
	void	AddAndSetNewNodeFromActual(const CString& sNodeText, const CString& sNodeKey);
	void	AddAndSetNewNodeFromActual(const CString& sNodeText, const CString& sNodeKey, const CString& sNodeKeyImage);
	void	AddAndSetNewNodeFromActual(const CString& sNodeText, const CString& sNodeKey, const CStringArray& sToolTipText, const CString& sNodeKeyImage);
	void	SetForeColorForNode(const CString& nodeKey, const COLORREF foreColor);
	void	AddToCurrentNode();
	BOOL	GetContextMenuItemClickedResponse();
	CString	GetLastChangedNodeKey();
	int		GetOldNodesCount();

	BOOL	AddFastRoot(const CString& sNodeText, const CString& sNodeKey, const CString& sToolTipText = _T(""), const CString& sNodeKeyImage = _T(""));
	BOOL	InsertFastChild(const CString& sParentKey, const CString& sNodeText, const CString& sNodeKey, const CString& sToolTipText = _T(""), const CString& sNodeKeyImage = _T(""));
	void	BeginUpdate();
	void	EndUpdate();
	void	GetAllChildrenFromSelectedNode(CArray<CString>& m_AllChildren);
	void	GetAllChildrenFromNodeKey(const CString& nodeKey, CArray<CString>& allChildren);
	void	CreateNewNode(const CString& sNodeText, const CString& sNodeKey);
	void	CreateNewNode(const CString& sNodeText, const CString& sNodeKey, const CString& sNodeKeyImage);
	void	CreateNewNode(const CString& sNodeText, const CString& sNodeKey, const CStringArray& sToolTipText, const CString& sNodeKeyImage);
	void	DeleteAllChildrenFromSelectedNode();
	bool	IsIgnoreSelectionChanged();
	void	SetIgnoreSelectionChanged(bool bIgnore);
	void	OldNodesClear();
	void	OldNodesPop();
	void	OldNodesPeek();
	void	OldNodesPush();
	void	SetAllowDragOver(const BOOL& bValue = FALSE);
	void	SetCaptionBoxConfirm(const CString& caption);
	void 	SetCheckBoxControls(const BOOL& bValue = FALSE);
	void	SetColorNewNode(const COLORREF color);
	void	SetColorCurrentNode(const COLORREF color);
	void	GetAllNodesFromSelection(CArray<CString>& m_AllNodesLeaf, CArray<CString>& m_AllNodesNotLeaf);
	void	SetSelectionOnlyOnLevel(const int& nLevelOnly);
	void	SetTextBoxConfirm(const CString& text);
	void	AddToSelectedNode();
	void	SetChecksBoxEditable(const BOOL& bValue = TRUE);
	void 	SetCurrentNodeParentSelected();
	void	SetNextNode();
	void	SetPrevNode();
	CString	GetTextNode(const int& nrNode);

protected:
	bool	IsAnimating();
	CWnd*	m_pParentWnd;
	TreeNodeMap* m_pTreeNodeMap;
	UINT m_nImage;
	CMap<CString, LPCTSTR, UINT, UINT> m_ImageMap;

//TODO: da eliminare
protected:
	virtual void OnInitControl() {}


protected:
	virtual	BOOL	Create(DWORD dwStyle, const RECT& rect, CWnd* pParentWnd, UINT nID);
	void			Enable(const BOOL bValue = TRUE);

protected:      
	//{{AFX_MSG(CTreeViewAdvCtrl)
	afx_msg LRESULT OnGetControlDescription		(WPARAM wParam, LPARAM lParam);
	afx_msg	LRESULT	OnRecalcCtrlSize(WPARAM, LPARAM);

	DECLARE_MESSAGE_MAP ();
};

//=============================================================================
class TB_EXPORT TreeNodeKey : public CObject
{
public:
	TreeNodeKey();
	TreeNodeKey(HTREEITEM hNode, const CString& sNodeKeyImage = _T(""), const COLORREF color = AfxGetThemeManager()->GetTreeViewNodeForeColor());

private:
	HTREEITEM	m_hNode;
	CString		m_sNodeKeyImage;
	COLORREF	m_cColor;

public:
	HTREEITEM GetHTreeItem() { return m_hNode; }
};


#else 
//=============================================================================
//			Class CTreeViewAdvCtrl
//=============================================================================
class TB_EXPORT CTreeViewAdvCtrl : public CManagedParsedCtrl, public CTreeViewAdvWrapper
{
	DECLARE_DYNCREATE(CTreeViewAdvCtrl)

public:
	CTreeViewAdvCtrl();

	void	SelectNode(CString keyNode);
	void	ToggleNode(CString keyNode);

private:
	CTBToolTipCtrl	m_ToolTip;
	CString			m_LastTextToolTip;
	BOOL			m_bToolTip;

public:
	virtual	BOOL OnCreateManaged(DWORD dwStyle, const RECT& rect, CWnd* pParentWnd, UINT nID);
	virtual BOOL OnEnableCtrlManaged(BOOL bEnable = TRUE);
	virtual HWND GetManagedHandle();
	virtual BOOL EnableCtrl(BOOL bEnable = TRUE);
	virtual void OnAfterAttachControl();
	virtual	BOOL PreTranslateMessage(MSG* pMsg);
	virtual void EnableToolTip();

protected:
	//{{AFX_MSG(CTreeViewAdvCtrl)
	afx_msg LRESULT OnGetControlDescription(WPARAM wParam, LPARAM lParam);
	afx_msg void	OnSetFocus(CWnd* /*pOldWnd*/);
	afx_msg void	OnMouseMove(UINT nFlags, CPoint point);

	DECLARE_MESSAGE_MAP();
};
#endif

//=============================================================================
//			Class CTBPicViewerAdvCtrl
//=============================================================================
class TB_EXPORT CTBPicViewerAdvCtrl : public CManagedParsedCtrl, public CTBPicViewerAdvWrapper
{
	DECLARE_DYNCREATE(CTBPicViewerAdvCtrl)

public:
	CTBPicViewerAdvCtrl();
	~CTBPicViewerAdvCtrl();

public:
	BOOL m_bSkipRecalcCtrlSize;

public:
	virtual	BOOL OnCreateManaged		(DWORD dwStyle, const RECT& rect, CWnd* pParentWnd, UINT nID);
	virtual BOOL OnEnableCtrlManaged	(BOOL bEnable = TRUE);
	virtual HWND GetManagedHandle		();
	virtual void OnAfterAttachControl	();

	virtual	void		SetValue		(const DataObj& aValue);
	virtual DataType	GetDataType		() const { return DataType::String; }

	virtual	BOOL SubclassEdit		(UINT IDC, CWnd* pParent, const CString& strName = _T(""));
	virtual	BOOL SkipRecalcCtrlSize	();

			void InitSizeInfo		() { ResizableCtrl::InitSizeInfo(this); }
			void SetSkipRecalcCtrlSize(BOOL bEnable = FALSE) { m_bSkipRecalcCtrlSize = bEnable; }
	
protected:      
	//{{AFX_MSG(CTBPicViewerAdvCtrl)
	afx_msg	LRESULT	OnRecalcCtrlSize		(WPARAM, LPARAM);

	DECLARE_MESSAGE_MAP ();
};

#include "endh.dex"