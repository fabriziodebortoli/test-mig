/////////////////////////////////////////////////////////////////
//Customized version of TreeViewAdv control
//by Andrey Gliznetsov
/////////////////////////////////////////////////////////////////

#pragma once

#include <TbGeneric\TBThemeManager.h>

#include "UserControlWrappers.h"

#include "beginh.dex"

class CUserControlHandlerObj;
class CToolBarWrapper;
class CResizeExtenderWrapper;
class CWndTreeCtrlDescription;

//Events
#define UM_TREEVIEWADV_SELECTION_CHANGED			0x9011
#define UM_TREEVIEWADV_ITEM_DRAG					0x9012
#define UM_TREEVIEWADV_DRAG_OVER					0x9013
#define UM_TREEVIEWADV_DRAG_DROP					0x9014
#define UM_TREEVIEWADV_MOUSE_UP						0x9015
#define UM_TREEVIEWADV_MOUSE_DOWN					0x9016
#define UM_TREEVIEWADV_MOUSE_CLICK					0x9017
#define UM_TREEVIEWADV_CONTEXT_MENU_ITEM_CLICK		0x9018
#define UM_TREEVIEWADV_MOUSE_DOUBLE_CLICK			0x9020
#define UM_TREEVIEWADV_NODE_CHANGED					0x9021
#define UM_TREEVIEWADV_LABEL_CHANGED				0x9022
#define SHOWING_COMMAND_ID							1717
#define HIDING_COMMAND_ID							1718

enum ToolBarButtonStyleWrapper { E_DROPDOWN_BUTTON, E_PUSH_BUTTON, E_SEPARATOR_BUTTON, E_TOGGLE_BUTTON};
		
// wrapper class to TreeViewAdv C# component
//===========================================================================
class TB_EXPORT UTreeViewAdvEventArgs : public UnmanagedEventsArgs
{
	DECLARE_DYNAMIC (UTreeViewAdvEventArgs);

public:
	UTreeViewAdvEventArgs	(const CString& sError);
};

class CTreeNodeAdvWrapperObj;

// Array dei nodi del tree   //vs2013 non sopportava la TB_EXPORT con la derivazione public CArray<CTreeNodeAdvWrapperObj*, CTreeNodeAdvWrapperObj*>
//quindi abbiamo splittato la classe in due parti, una ha la derivazione, l'altra il tbexport
//===========================================================================
class  CInternalTreeNodeAdvArray : public CArray<CTreeNodeAdvWrapperObj*, CTreeNodeAdvWrapperObj*>
{
public:
	~CInternalTreeNodeAdvArray();
};

class TB_EXPORT CTreeNodeAdvArray : public CInternalTreeNodeAdvArray
{};

// Mappa dei nodi del tree
//===========================================================================
class TB_EXPORT CTreeNodeAdvMap : public CMapStringToOb 
{
public:
	~CTreeNodeAdvMap();
	void Clear();
};

// wrapper class to CTreeNode 
//===========================================================================
class TB_EXPORT CTreeNodeAdvWrapperObj : public CObject
{

public:
	CTreeNodeAdvWrapperObj(){}
public:
	virtual CString			GetKey			()									= 0;
	virtual CString			GetText			()									= 0;
	virtual void			GetChildren		(CTreeNodeAdvArray& arAllChildren)	= 0;
	virtual	BOOL			IsExpanded		()									= 0;
	virtual	BOOL			IsSelected		()									= 0;
	virtual	BOOL			HasChildren		()									= 0;
	virtual HBITMAP			GetImageHandle	()									= 0;
	virtual	void			ToggleNode		()									= 0;
	virtual	void			SelectNode		()									= 0;
	virtual BOOL			IsChecked		()									= 0;
	virtual BOOL			ToggleCheck		(CString* errorMsg)					= 0;
	virtual BOOL			Check			(CString* errorMsg)					= 0;
	virtual BOOL			UnCheck			(CString* errorMsg)					= 0;
};

// wrapper class to TreeViewAdv C# component (Aga.Controls library)
//===========================================================================
#ifdef NEWTREE
class TB_EXPORT CTreeViewAdvWrapper
{
public:
	enum E_FONTSTYLE		{ F_BOLD, F_ITALIC, F_REGULAR, F_STRIKEOUT, F_UNDERLINE};
	enum E_SELECTIONMODE	{ F_SINGLE, F_MULTI, F_MULTISAMEPARENT };

	bool	IsIgnoreSelectionChanged () { return FALSE; }
	virtual void OnSelectionChanged	 ()						{}
	virtual void OnStateNodeChanged	 ()	{}
	virtual void OnSizeChanged		 ()	{}
	virtual void OnToolBarCommand(int cmdId) {};
	bool	OnKeyCommand(UINT nKey, bool ctrl, bool alt, bool shift) { return FALSE; }
};
#else
class TB_EXPORT CTreeViewAdvWrapper : public CUserControlWrapperObj
{
public:
	enum E_FONTSTYLE		{ F_BOLD, F_ITALIC, F_REGULAR, F_STRIKEOUT, F_UNDERLINE};
	enum E_SELECTIONMODE	{ F_SINGLE, F_MULTI, F_MULTISAMEPARENT };
	
private:
	CTreeNodeAdvMap				wrappedNodes;
	BOOL						m_bUseNodesCache;	//Dice se deve usare la cache dei nodi, impostato a false se wrappato da EasyBuilder, perche EB
													//chiama i metodi direttamente sul c#, e la cache sarebbe disallineata
	int							m_nButtonSize;	//determina la grandezza dei bottoni della toolbar
	CToolBarWrapper*			m_pToolbar;
	CResizeExtenderWrapper*		m_pExtender;
	int							m_nOriginalWidth;
	int							m_nOriginalHeight;
	bool						m_bIsVisible;
	bool						m_bAnimating;
	bool						m_bIgnoreSelectionChanged;

public:
	CTreeViewAdvWrapper		();
	~CTreeViewAdvWrapper	();

private:
	void	SetTreeViewDefaultFont						();

public:
	void	SetEditable									(const BOOL& bValue = TRUE);
	BOOL	IsEditable									();
	void	SetAllowDrop								(const BOOL& bValue = FALSE);
	void	SetFont										(const CString& sFontName, const int& fontSize, const E_FONTSTYLE eFontStyle);

	//********************************************************************************
	void	SetBackColorTreeView						(const COLORREF color = AfxGetThemeManager()->GetTreeViewBkgColor());		
	void	SetSelectedNodeBackColor(const COLORREF color = AfxGetThemeManager()->GetBERowSelectedBkgColor());
	void	SetSelectedNodeInactiveBkgColor(const COLORREF color = AfxGetThemeManager()->GetBERowSelectedBkgColor());
	void	SetSelectedNodeForeColor(const COLORREF color = AfxGetThemeManager()->GetBERowSelectedForeColor());
	void	SetScrollBarColor(const COLORREF color = AfxGetThemeManager()->GetScrollBarThumbNoPressedColor());
	//********************************************************************************

	void 	SetRowHeight								(int nHeight);
	void	AddImage									(const CString& sImageKey, const CString& sImagePath);
	void	SetImage									(const CString& sNodeKey,  const CString& sImageKey);
	void	SetImageNoSel								(const CString& sNodeKey,  const CString& sImageKey);
	void	SetSelectedNodeImage						(const CString& sNodeKey);
	BOOL	AddNode										(const CString& sNodeText, const CString& sNodeKey, const CString& sNodeKeyImage = _T(""), const COLORREF color = AfxGetThemeManager()->GetTreeViewNodeForeColor());
	BOOL	AddNode										(const CString& sNodeText, const CString& sNodeKey, const CStringArray& sToolTipText, const CString& sNodeKeyImage = _T(""), const COLORREF color = AfxGetThemeManager()->GetTreeViewNodeForeColor());
	BOOL	AddNode										(const CString& sNodeText, const CString& sNodeKey, const BOOL& checkBoxNode);
	BOOL	AddChild									(const CString& sNodeText, const CString& sNodeKey, const CString& sNodeKeyImage = _T(""), const COLORREF color = AfxGetThemeManager()->GetTreeViewNodeForeColor());
	BOOL	AddChild									(const CString& sNodeText, const CString& sNodeKey, const CStringArray& sToolTipText, const CString& sNodeKeyImage = _T(""), const COLORREF color = AfxGetThemeManager()->GetTreeViewNodeForeColor());
	BOOL	AddFastNodeToSelected						(const CString& sNodeText, const CString& sNodeKey, const CString& sToolTiptext = _T(""), const CString& sNodeKeyImage = _T(""));
	BOOL	AddFastRoot									(const CString& sNodeText, const CString& sNodeKey, const CString& sToolTipText = _T(""), const CString& sNodeKeyImage = _T(""));
	BOOL	InsertFastChild								(const CString& sParentKey, const CString& sNodeText, const CString& sNodeKey, const CString& sToolTipText = _T(""), const CString& sNodeKeyImage = _T(""));
	//********************************************************************************
	BOOL	SetNodeAsSelected							(const CString& sNodeKeyToSearch);
	BOOL	ExistsNode									(const CString& sNodeKeyToSearch);
	BOOL	InsertChild									(const CString& sParentKey, const CString& sNodeText, const CString& sNodeKey, const CString& sNodeKeyImage = _T(""), const COLORREF color = AfxGetThemeManager()->GetTreeViewNodeForeColor());
	BOOL	InsertChild									(const CString& sParentKey, const CString& sNodeText, const CString& sNodeKey, const CStringArray& sToolTipText, const CString& sNodeKeyImage = _T(""), const COLORREF color = AfxGetThemeManager()->GetTreeViewNodeForeColor());
	CString GetParentKey								(const CString& sNodeKeyToSearch);
	CString GetKeyByPosition							(const CPoint& pMouse);

	//********************************************************************************
	void 	ClearTree									();
	BOOL	RemoveNode									(const CString& sNodeKeyToSearch);
	
	//********************************************************************************
	//use these methods for editing nodes in your TreeView
	void	EditEnabled									(const BOOL& bValue = FALSE);
	//bValue = TRUE => every node can be edited pressing F2 key or after selection of node one more click
	//otherwise you can edit whether EditEnabled through the context menu or other custom ways
	void	CanDirtectlyEditing							(const BOOL& bValue = FALSE);
	//call this method when you begin to edit the selected node
	void	BeginEdit									();
	void	EndEdit										(bool bCancel);
	void	SetDefaultTextForEditing(const CString& sDefaultTextForEditing);
	CString	GetCurrentTextEdited						();

	void	BeginUpdate									();
	void	EndUpdate									();

	//********************************************************************************
	void	Display										(bool bShow);
	void 	AddControls									();
	void 	SetCheckBoxControls							(const BOOL& bValue = FALSE);
	void 	SetNodeStateIcon							(const BOOL& bValue = FALSE);
	//M. 5831
	void	SetChecksBoxEditable						(const BOOL& bValue = TRUE);
	void 	SetNodeCheckBoxProperty						(const CString& propertyName);
	void 	SetNodeCheckBoxThreeState					(const BOOL& threeState = FALSE);
	void 	SetNodeTextBoxProperty						(const CString& propertyName);
	void 	SetToolTip									();
	void	SetBalloonToolTip							(const BOOL& bIsBalloon = FALSE);
	void	SetCustomToolTip							(const COLORREF bkColor, const COLORREF foreColor);
	void	SetDblClickWithExpandCollapse				(const BOOL& bValue = FALSE);
	void 	SetCurrentNodeParentSelected				();
	void	SetMenuItemCheck							(const CString& itemMenu, BOOL check = TRUE);
	void	SetMenuItemEnable							(const CString& itemMenu, BOOL enabled = TRUE);
	BOOL 	ExistsSelectedNode							();
	void 	ExpandAllFromSelectedNode					();
	void	ExpandAll									();
	void	ExpandLevels								(int nLevel);
	void 	CollapseAllFromSelectedNode					();
	void	CollapseAll									();
	BOOL	IsExpandedSelectedNode						();
	void 	SetDragAndDropOnSameLevel					(const BOOL& bValue = FALSE);
	void 	AddContextMenuItem							(const CString& menuItem);
	void	AddContextMenuItemDisabled					(const CString& menuItem);
	void	AddContextSubMenuItem						(const CString& itemMenu, CArray<CString>& subItems);
	BOOL	GetContextMenuItemClickedResponse			();
	void	SetCaptionBoxConfirm						(const CString& caption);
	void	SetTextBoxConfirm							(const CString& text);
	void	AddContextMenuItemWithConfirm				(const CString& menuItem);
	void	AddContextMenuSeparator						();
	void	SetStyleForSelectedNode						(const BOOL& bBold = FALSE, const BOOL& bItalic = FALSE, const BOOL& bStrikeOut = FALSE, const BOOL& bUnderline = FALSE);
	void	SetStyleForNode								(const CString& nodeKey, const BOOL& bBold = FALSE, const BOOL& bItalic = FALSE, const BOOL& bStrikeOut = FALSE, const BOOL& bUnderline = FALSE, const COLORREF foreColor  = AfxGetThemeManager()->GetTreeViewNodeForeColor());
	void	SetForeColorForNode							(const CString& nodeKey, const COLORREF foreColor);
	void 	SetViewContextMenu							(const BOOL& bValue = FALSE);
	void	RemoveContextMenuItem						(int idxMenuItem);
	int	 	GetIdxContextMenuItemClicked				();
	void	GetTextContextMenuItemClicked				(CString& itemKey);
	int		GetSelectedNodeLevel						();
	CString GetSelectedNodeKey							();
	void	GetAllParentsTextNodeFromSelected			(CArray<CString>& m_AllParents);
	void	GetAllNodesFromSelection					(CArray<CString>& m_AllNodesLeaf, CArray<CString>& m_AllNodesNotLeaf);
	void	SetUpdateTextNode							(const int& nrNode, const CString& text);
	void	SetUpdateTextNode							(const CString& nodeKey, const CString& text);
	CString	GetTextNode									(const int& nrNode);
	void	GetAllChildrenFromSelectedNode				(CArray<CString>& m_AllChildren);
	void	GetAllChildrenFromNodeKey					(const CString& nodeKey, CArray<CString>& allChildren);
	void	SetSelectionMode							(const E_SELECTIONMODE eSelectionMode);
	void	SetSelectionOnlyOnLevel						(const int& nLevelOnly);
	void	SetAllowDragOver							(const BOOL& bValue = FALSE);
	CString	GetNewParentKey								();
	void	SetCancelDragDrop							();
	void	SetCancelDragOver							();
	BOOL	HasChildrenSelectedNode						();
	void	DeleteAllChildrenFromSelectedNode			();
	void	EnsureVisible								(const CString& sNodeKey);

	void	SetSelectRoot								();
	void	SetSelectFirstChild							();
	void	SetNextNode									();
	void	SetPrevNode									();
	//!!!!Use the following methods with stack management of old nodes
	void	CreateNewNode								(const CString& sNodeText, const CString& sNodeKey);
	void	CreateNewNode								(const CString& sNodeText, const CString& sNodeKey, const CString& sNodeKeyImage);
	void	CreateNewNode								(const CString& sNodeText, const CString& sNodeKey, const CStringArray& sToolTipText, const CString& sNodeKeyImage);
	int		GetOldNodesCount							();
	void	OldNodesPop									();
	void	OldNodesPeek								();
	void	OldNodesPush								();
	void	OldNodesClear								();
	//Use the following two methods when use CurrentNode and Actual Node
	void	SetColorNewNode								(const COLORREF color);
	void	SetColorCurrentNode							(const COLORREF color);
	//
	void	AddToSelectedNode							();
	void	AddToCurrentNode							();
	//

	//Added for MagoWEB
	void	SelectNode									(const CString& sNodeKey);
	void	ToggleNode									(const CString& sNodeKey);
	HMENU	GetContextMenuHandle						();
	void	SendMessageToControl						(UINT msg, WPARAM wParam, LPARAM lParam);
	CRect	GetNodeBounds								(const CString& sNodeKey, int x, int y);
	CRect	GetNodeBounds								(CTreeNodeAdvWrapperObj* pNode, int x, int y);
	
	//Metodi che fanno operazioni lavorando con puntatore a nodo (CTreeNodeAdvWrapperObj*)
	CTreeNodeAdvWrapperObj*	GetNode						(const CString& sNodeKey);
	CTreeNodeAdvWrapperObj* GetRoot						();

	CSize	GetSize										();
	//Use the following methods to construct the tree with unique nodes
	void	AddToSelectedAndSetNodeAsCurrent			(const CString& sNodeText, const CString& sNodeKey);
	void	AddToSelectedAndSetNodeAsCurrent			(const CString& sNodeText, const CString& sNodeKey, const CString& sNodeKeyImage);
	void	AddToSelectedAndSetNodeAsCurrent			(const CString& sNodeText, const CString& sNodeKey, const CStringArray& sToolTipText, const CString& sNodeKeyImage);
	void	AddAndSetNewNodeFromCurrent					(const CString& sNodeText, const CString& sNodeKey);
	void	AddAndSetNewNodeFromCurrent					(const CString& sNodeText, const CString& sNodeKey, const CString& sNodeKeyImage);
	void	AddAndSetNewNodeFromCurrent					(const CString& sNodeText, const CString& sNodeKey, const CStringArray& sToolTipText, const CString& sNodeKeyImage);
	void	AddAndSetNewNodeFromActual					(const CString& sNodeText, const CString& sNodeKey);
	void	AddAndSetNewNodeFromActual					(const CString& sNodeText, const CString& sNodeKey, const CString& sNodeKeyImage);
	void	AddAndSetNewNodeFromActual					(const CString& sNodeText, const CString& sNodeKey, const CStringArray& sToolTipText, const CString& sNodeKeyImage);
	//
	void	DeleteSelectedNode							();
	//metodi di spostamento dei nodi nel TreeView
	void	MoveChildrenOfSelectedNodeToChildrenOfRoot	(const BOOL& bAlsoLeaves = FALSE);
	void	MoveSelectedNodeToChildrenOfRoot			();

	void	SetUseNodesCache							(BOOL bUseNodesCache) {  m_bUseNodesCache = bUseNodesCache;}
	void	AddToolBarCommand							(int id, const CString& sImage, CString sToolTip = L"", TCHAR nAccelCharCode = 0, BOOL bCtrlModifier = FALSE, BOOL bShiftModifier = FALSE, BOOL bAltModifier = FALSE, ToolBarButtonStyleWrapper eButtonStyle = E_PUSH_BUTTON);
	void	AddHidingCommand							();
	void	AddResizeControl							();
	void	EnableToolBarCommand						(int id, bool bEnable);
	void	ShowToolBarCommand							(int id, bool bShow);
	bool	IsToolBarButtonPushed						(int id); //x i toogle button
	void	SetToolBarButtonPushed						(int id, bool bPush); //x i toogle button
	void	SetToolbarButtonSize						(int nButtonSize) { m_nButtonSize = nButtonSize; }
	CString	GetLastChangedNodeKey						();
	bool	OnKeyCommand								(UINT nKey, bool ctrl, bool alt, bool shift);

	void	SetIgnoreSelectionChanged					(bool bIgnore) { m_bIgnoreSelectionChanged = bIgnore; }
	bool	IsIgnoreSelectionChanged					() { return m_bIgnoreSelectionChanged; }

	virtual void OnSelectionChanged						()	{}
	virtual void OnStateNodeChanged						()	{}
	virtual void OnSizeChanged							()	{}
	virtual void OnToolBarCommand						(int cmdId);
	virtual void EnableToolTip							()	{}

protected:
	bool	IsAnimating();
	void	Enable	(const BOOL bValue = TRUE);
	virtual void OnInitControl ();

private:
	void	InitDefaultValues	();
public:
	void	Focus();

};
#endif
#include "endh.dex"