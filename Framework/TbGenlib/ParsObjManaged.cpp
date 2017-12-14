#include "stdafx.h"

#include <atlimage.h>

#include <TbGenlib\TbTreeCtrl.h>
#include <TbFrameworkImages\GeneralFunctions.h>
#include <TbFrameworkImages\CommonImages.h>

#include "BaseDoc.h"
#include "ParsObj.h"
#include "ParsObjManaged.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif

//=============================================================================
//			Class CManagedParsedCtrl
//=============================================================================
IMPLEMENT_DYNAMIC (CManagedParsedCtrl, CWnd)

//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CManagedParsedCtrl, CWnd)
	ON_MESSAGE	(UM_RECALC_CTRL_SIZE, OnRecalcCtrlSize)
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CManagedParsedCtrl::CManagedParsedCtrl()
	:
	IDisposingSourceImpl(this)
{
	CParsedCtrl::Attach(this);
}

//-----------------------------------------------------------------------------
CManagedParsedCtrl::~CManagedParsedCtrl ()
{
}

//-----------------------------------------------------------------------------
BOOL CManagedParsedCtrl::SubclassEdit (UINT nID, CWnd* pParentWnd, const CString& strName /*= _T("")*/)
{
	CRect aRect(0, 0, 0, 0);
	CWnd* wndPlaceHolder = pParentWnd->GetDlgItem(nID);
	if (wndPlaceHolder)
	{
		wndPlaceHolder->GetWindowRect(&aRect);
		pParentWnd->ScreenToClient(&aRect);
	}

	BOOL bOk = Create (WS_VISIBLE | WS_TABSTOP , aRect, pParentWnd, nID);
	//imposto lo stesso zorder del controllo che vado a distruggere
	SetWindowPos(wndPlaceHolder, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);
	if (wndPlaceHolder)
		wndPlaceHolder->DestroyWindow();
	return bOk;
}

//-----------------------------------------------------------------------------
void CManagedParsedCtrl::InitSizeInfo () 
{ 
	ResizableCtrl::InitSizeInfo(this); 
}

//-----------------------------------------------------------------------------
void CManagedParsedCtrl::SetValue (const DataObj& aValue)
{
}

//-----------------------------------------------------------------------------
void CManagedParsedCtrl::GetValue (DataObj& aValue)
{
}

//-----------------------------------------------------------------------------
void CManagedParsedCtrl::OnAfterAttachControl ()
{
}

//-----------------------------------------------------------------------------
DataType CManagedParsedCtrl::GetDataType () const
{
	return DATA_BOOL_TYPE;
}

//-----------------------------------------------------------------------------
LRESULT	CManagedParsedCtrl::OnRecalcCtrlSize (WPARAM wParam, LPARAM lParam)
{
	DoRecalcCtrlSize();
	return 0L;
}

//-----------------------------------------------------------------------------
LRESULT CALLBACK MyWndProc(HWND hwnd, UINT msg, WPARAM wParam, LPARAM lParam)
{
	CManagedParsedCtrl* pCtrl = (CManagedParsedCtrl*)CWnd::FromHandlePermanent(hwnd);
	
	return pCtrl ? pCtrl->WndProc(msg, wParam, lParam) : 0L;
}

//-----------------------------------------------------------------------------
LRESULT CManagedParsedCtrl::WndProc(UINT msg, WPARAM wParam, LPARAM lParam)
{
	//prima vedo se il controllo gestisce il messaggio
	LRESULT res = m_ManagedWndProc(m_hWnd, msg, wParam, lParam);
	if (res)
		return res;
	//poi lo passo a MFC
	OnWndMsg(msg, wParam, lParam, &res);
	return res;
}
//-----------------------------------------------------------------------------
BOOL CManagedParsedCtrl::SubclassWindow(HWND hWnd)
{
	if (!__super::Attach(hWnd))
		return FALSE;

	// allow any other subclassing to occur
	PreSubclassWindow();

	// non metto la AFX WndProc, ma una mia procedura, che deve chiamare sia quella MFC, sia quella del controllo managed
	// se subclassassi normalmente, MFC sostituirebbe la sua window procedure a quela del controllo, e questo potrebbe non lavorare correttamente
	m_ManagedWndProc = (WNDPROC)::SetWindowLongPtr(hWnd, GWLP_WNDPROC, (INT_PTR)MyWndProc);
	ASSERT(m_ManagedWndProc != AfxGetAfxWndProc());

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CManagedParsedCtrl::Create (DWORD dwStyle, const RECT& rect, CWnd* pParentWnd, UINT nID)
{
	if (!pParentWnd)
	{
		CString sMsg = cwsprintf( _TB("Missing parent Window. Error creating Gantt control for ID %d"), nID);
		ASSERT_TRACE(FALSE, sMsg);
		AfxGetDiagnostic()->Add (sMsg, CDiagnostic::Warning);
		return FALSE;
	}

	if (!OnCreateManaged(dwStyle, rect, pParentWnd, nID))
	{
		CString sMsg = cwsprintf( _TB("Error creating Managed Parsed Control for ID %d"), nID);
		ASSERT_TRACE(FALSE, sMsg);
		AfxGetDiagnostic()->Add (sMsg, CDiagnostic::Warning);
		return FALSE;
	}

	m_nID = nID;

	SubclassWindow(GetManagedHandle());
	ResizableCtrl::InitSizeInfo (this);
	
	OnAfterAttachControl();
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CManagedParsedCtrl::EnableCtrl (BOOL bEnable /*TRUE*/)
{
	return OnEnableCtrlManaged(bEnable);
}

//=============================================================================
//			Class CManagedCtrl
//=============================================================================
IMPLEMENT_DYNCREATE (CManagedCtrl, CManagedParsedCtrl)

//-----------------------------------------------------------------------------
CManagedCtrl::CManagedCtrl()
{
}

//-----------------------------------------------------------------------------
BOOL CManagedCtrl::OnCreateManaged (DWORD dwStyle, const RECT& rect, CWnd* pParentWnd, UINT nID)
{
	return CParsedUserControlWrapper::CreateControl(dwStyle, rect, pParentWnd, nID, (CWnd*) this);
}

//-----------------------------------------------------------------------------
BOOL CManagedCtrl::OnEnableCtrlManaged(BOOL bEnable /*TRUE*/)
{
	CParsedUserControlWrapper::Enable(bEnable);
	return TRUE;
}

//-----------------------------------------------------------------------------
HWND CManagedCtrl::GetManagedHandle ()
{
	return CParsedUserControlWrapper::GetControlHandle();
}

//-----------------------------------------------------------------------------
void CManagedCtrl::SetValue(const DataObj& aValue)
{             
	ASSERT(CheckDataObjType(&aValue));
	CParsedUserControlWrapper::SetValue(aValue.Str());
}

//-----------------------------------------------------------------------------
void CManagedCtrl::GetValue(DataObj& aValue)
{
	ASSERT(CheckDataObjType(&aValue));
	aValue.Assign(CParsedUserControlWrapper::GetValue());
}

//-----------------------------------------------------------------------------
DataType CManagedCtrl::GetDataType () const
{
	return m_pData ? m_pData->GetDataType() : DATA_STR_TYPE;
}

//-----------------------------------------------------------------------------
void CManagedCtrl::PerformLosingFocus ()
{
	UpdateCtrlData(0,0);
}

//-----------------------------------------------------------------------------
void CManagedCtrl::PerformTextChanged ()
{
	m_pData->SetModified(TRUE);
	SetModifyFlag(TRUE);
	m_pDocument->SetModifiedFlag(TRUE);
}

//-----------------------------------------------------------------------------
BOOL CManagedCtrl::IsValid()
{ 
	return __super::IsValid(); 
}

//-----------------------------------------------------------------------------
BOOL CManagedCtrl::IsValid (const DataObj& aValue)
{ 
	return __super::IsValid(aValue);
}

//-----------------------------------------------------------------------------
void CManagedCtrl::AttachMangedCtrl (AttachControlEventArg* pArg)
{
	CParsedUserControlWrapper::AttachControl(pArg);
}

//-----------------------------------------------------------------------------
void CManagedCtrl::OnAfterAttachControl ()
{
	CParsedUserControlWrapper::OnAfterAttachControl();
}

//=============================================================================
//			Class CGanttCtrl
//=============================================================================
IMPLEMENT_DYNCREATE (CGanttCtrl, CManagedParsedCtrl)

//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CGanttCtrl, CManagedParsedCtrl)
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CGanttCtrl::CGanttCtrl()
{
}

//-----------------------------------------------------------------------------
BOOL CGanttCtrl::OnCreateManaged (DWORD dwStyle, const RECT& rect, CWnd* pParentWnd, UINT nID)
{
	return CGanttWrapper::CreateControl(dwStyle, rect, pParentWnd, nID, (CWnd*) this);
}

//-----------------------------------------------------------------------------
BOOL CGanttCtrl::OnEnableCtrlManaged(BOOL bEnable /*TRUE*/)
{
	CGanttWrapper::Enable(bEnable);
	return TRUE;
}

//-----------------------------------------------------------------------------
HWND CGanttCtrl::GetManagedHandle ()
{
	return CGanttWrapper::GetControlHandle();
}



//-----------------------------------------------------------------------------
void CGanttCtrl::OnAfterAttachControl ()
{
	CGanttWrapper::OnAfterAttachControl();
}

#ifdef NEWTREE
#define ASSERTTREE		/*ASSERT(FALSE);*/
//=============================================================================
class TreeNodeMap
{
	public:
		CMap<CString, LPCTSTR, TreeNodeKey*, TreeNodeKey*> m_TreeMap;
};

//=============================================================================
//			Class CTreeViewAdvCtrl
//=============================================================================
IMPLEMENT_DYNCREATE(CTreeViewAdvCtrl, CTBTreeCtrl)

//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CTreeViewAdvCtrl, CTBTreeCtrl)
	ON_MESSAGE	(UM_GET_CONTROL_DESCRIPTION, OnGetControlDescription)
	ON_MESSAGE  (UM_RECALC_CTRL_SIZE, OnRecalcCtrlSize)
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CTreeViewAdvCtrl::CTreeViewAdvCtrl() :
	m_pParentWnd(NULL),
	m_nImage(0)
{
	m_ImageMap.RemoveAll();
	m_pTreeNodeMap = new TreeNodeMap();
	m_pTreeNodeMap->m_TreeMap.RemoveAll();
}

//-----------------------------------------------------------------------------
BOOL CTreeViewAdvCtrl::Create(DWORD dwStyle, const RECT& rect, CWnd* pParentWnd, UINT nID)
{
	if (!CTBTreeCtrl::Create(dwStyle, rect, pParentWnd, nID))
		return FALSE;
	
	return TRUE;
}

//-----------------------------------------------------------------------------
void CTreeViewAdvCtrl::Enable(const BOOL bValue /*= TRUE*/)
{
	//TODO:
	ASSERTTREE
}

//-----------------------------------------------------------------------------
BOOL CTreeViewAdvCtrl::SubclassEdit(UINT nID, CWnd* pParent, const CString& strName)
{
	ASSERT(pParent);
	m_pParentWnd = pParent;

	CRect aRect(0, 0, 0, 0);
	CWnd* wndPlaceHolder = m_pParentWnd->GetDlgItem(nID);
	if (wndPlaceHolder)
	{
		wndPlaceHolder->GetWindowRect(&aRect);
		m_pParentWnd->ScreenToClient(&aRect);
	}

	if (!Create(WS_VISIBLE | WS_TABSTOP | WS_CHILD | WS_BORDER
		| TVS_HASBUTTONS | TVS_LINESATROOT | TVS_HASLINES
		/*| TVS_DISABLEDRAGDROP | TVS_NOTOOLTIPS*/ | TVS_EDITLABELS, aRect, pParent, nID)) {
		ASSERT(FALSE);
		return FALSE;
	}
	
	//imposto lo stesso zorder del controllo che vado a distruggere
	SetWindowPos(wndPlaceHolder, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);
	if (wndPlaceHolder)
		wndPlaceHolder->DestroyWindow();


	CParsedCtrl::Attach(this);
	ResizableCtrl::InitSizeInfo(this);
	return TRUE;
}

//------------------------------------------------------------------------------
void CTreeViewAdvCtrl::InitializeImageList()
{
	m_ImageList.Create(20, 20, ILC_COLOR32, 20, 20);
	m_ImageList.SetBkColor(AfxGetThemeManager()->GetTransBmpTransparentDefaultColor());
	SetImageList(&m_ImageList, TVSIL_NORMAL);

	// Incon 0
	m_ImageList.Add(TBLoadImage(TBGlyph(szIconFolder)));
}

//------------------------------------------------------------------------------
LRESULT CTreeViewAdvCtrl::OnRecalcCtrlSize(WPARAM wp, LPARAM lp)
{
	CRect rect;
	if (m_pParentWnd)
	{
		m_pParentWnd->GetWindowRect(rect);
	}

	return __super::OnRecalcCtrlSize(wp, lp);
}

//-----------------------------------------------------------------------------
void CTreeViewAdvCtrl::SelectNode(CString keyNode)
{
	//TODO:
	ASSERTTREE
}
//-----------------------------------------------------------------------------
void CTreeViewAdvCtrl::ToggleNode(CString keyNode)
{
	//TODO:
	ASSERTTREE
}
//-----------------------------------------------------------------------------
void CTreeViewAdvCtrl::SetValue(const DataObj& aValue)
{
	ASSERT(CheckDataObjType(&aValue));
}

//-----------------------------------------------------------------------------
void CTreeViewAdvCtrl::GetValue(DataObj& aValue)
{
	ASSERT(CheckDataObjType(&aValue));
}

//-----------------------------------------------------------------------------
BOOL CTreeViewAdvCtrl::EnableCtrl(BOOL bEnable /* = TRUE */)
{
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CTreeViewAdvCtrl::IsValid()
{
	return __super::IsValid();
}

//-----------------------------------------------------------------------------
BOOL CTreeViewAdvCtrl::IsValid(const DataObj& aValue)
{
	return __super::IsValid(aValue);
}

//-----------------------------------------------------------------------------
DataType CTreeViewAdvCtrl::GetDataType() const
{
	return DATA_BOOL_TYPE;
}

//-----------------------------------------------------------------------------
void CTreeViewAdvCtrl::AddImage(const CString& sImageKey, const CString& sImagePath)
{
	HICON	hIcon;
	//hIcon = TBLoadImageDosPath(sImagePath);
	hIcon = TBLoadImage(AfxGetPathFinder()->GetNamespaceFromPath(sImagePath).GetObjectName());

	m_nImage++;
	m_ImageList.Add(hIcon);
	::DeleteObject(hIcon);
	m_ImageMap.SetAt(sImageKey, m_nImage);
}

//-----------------------------------------------------------------------------
void CTreeViewAdvCtrl::SetImage(const CString& sNodeKey, const CString& sImageKey)
{
	//TODO:
	ASSERTTREE
}

//-----------------------------------------------------------------------------
void CTreeViewAdvCtrl::AddControls()
{
	//TODO:
}

//-----------------------------------------------------------------------------
void CTreeViewAdvCtrl::AddHidingCommand()
{
	//TODO:
	ASSERTTREE
}

//-----------------------------------------------------------------------------
void CTreeViewAdvCtrl::AddResizeControl()
{
	//TODO:
	ASSERTTREE
}

//-----------------------------------------------------------------------------
void CTreeViewAdvCtrl::SetAllowDrop(const BOOL& bValue /*= FALSE*/)
{
	//TODO:
	ASSERTTREE
}

//-----------------------------------------------------------------------------
void  CTreeViewAdvCtrl::SetViewContextMenu(const BOOL& bValue /*= FALSE*/)
{
	//TODO:
}

//-----------------------------------------------------------------------------
void CTreeViewAdvCtrl::SetDragAndDropOnSameLevel(const BOOL& bValue /*= FALSE*/)
{
	//TODO:
	ASSERTTREE
}

//-----------------------------------------------------------------------------
void CTreeViewAdvCtrl::ClearTree()
{
	if (!DeleteAllItems()) { 
		ASSERT(FALSE);
	}
}

//-----------------------------------------------------------------------------
void CTreeViewAdvCtrl::SetNodeStateIcon(const BOOL& bValue /*= FALSE*/)
{
	if (bValue)
	{
		InitializeImageList();
	}
	else
	{
		SetImageList(NULL, TVSIL_NORMAL);
	}
}

//-----------------------------------------------------------------------------
INT	CTreeViewAdvCtrl::FindImage(const CString& sImageKey)
{
	UINT iGet;
	if (m_ImageMap.Lookup(sImageKey, iGet))
	{
		return iGet;
	}
	return 0;
}

////-----------------------------------------------------------------------------
TreeNodeKey* CTreeViewAdvCtrl::FindNnode(const CString& sNodeKey)
{
	TreeNodeKey* pTreeNodeKey;
	if (!m_pTreeNodeMap->m_TreeMap.Lookup(sNodeKey, pTreeNodeKey))
		return NULL;
	return (TreeNodeKey*)pTreeNodeKey;
}

//-----------------------------------------------------------------------------
BOOL CTreeViewAdvCtrl::AddNode(const CString& sNodeText, const CString& sNodeKey, const CString& sNodeKeyImage /*= _T("")*/, const COLORREF color /*= AfxGetThemeManager()->GetTreeViewNodeForeColor()*/)
{
	HTREEITEM hNode = InsertItem(sNodeText, FindImage(sNodeKeyImage), FindImage(sNodeKeyImage));
	m_pTreeNodeMap->m_TreeMap.SetAt(sNodeKey, new TreeNodeKey(hNode, sNodeKeyImage, color));
	return FALSE;
}

//-----------------------------------------------------------------------------
BOOL CTreeViewAdvCtrl::AddNode(const CString& sNodeText, const CString& sNodeKey, const CStringArray& sToolTipText, const CString& sNodeKeyImage /*= _T("")*/, const COLORREF color /*= AfxGetThemeManager()->GetTreeViewNodeForeColor()*/)
{
	//TODO:
	ASSERTTREE
	return FALSE;
}

//-----------------------------------------------------------------------------
BOOL CTreeViewAdvCtrl::AddNode(const CString& sNodeText, const CString& sNodeKey, const BOOL& checkBoxNode)
{
	//TODO:
	ASSERTTREE
	return FALSE;
}

//-----------------------------------------------------------------------------
BOOL CTreeViewAdvCtrl::AddChild(const CString& sNodeText, const CString& sNodeKey, const CString& sNodeKeyImage /*= _T("")*/, const COLORREF color /*= AfxGetThemeManager()->GetTreeViewNodeForeColor()*/)
{
	ASSERTTREE
	return AddNode(sNodeText, sNodeKey, sNodeKeyImage, color);
}

//-----------------------------------------------------------------------------
BOOL CTreeViewAdvCtrl::AddChild(const CString& sNodeText, const CString& sNodeKey, const CStringArray& sToolTipText, const CString& sNodeKeyImage /*= _T("")*/, const COLORREF color /*= AfxGetThemeManager()->GetTreeViewNodeForeColor()*/)
{
	ASSERTTREE
	return AddNode(sNodeText, sNodeKey, sToolTipText, sNodeKeyImage, color);
}

//-----------------------------------------------------------------------------
BOOL CTreeViewAdvCtrl::InsertChild(const CString& sParentKey, const CString& sNodeText, const CString& sNodeKey, const CString& sNodeKeyImage /*= _T("")*/, const COLORREF color /*= AfxGetThemeManager()->GetTreeViewNodeForeColor()*/)
{
	TreeNodeKey* pTreeNodeKey = FindNnode(sParentKey);
	if (pTreeNodeKey == NULL)
		return FALSE;

	HTREEITEM hItem = pTreeNodeKey->GetHTreeItem();
	HTREEITEM hNode = InsertItem(sNodeText, FindImage(sNodeKeyImage), FindImage(sNodeKeyImage), hItem);
	m_pTreeNodeMap->m_TreeMap.SetAt(sNodeKey, new TreeNodeKey(hNode, sNodeKeyImage, color));
	return FALSE;
}

//-----------------------------------------------------------------------------
BOOL CTreeViewAdvCtrl::InsertChild(const CString& sParentKey, const CString& sNodeText, const CString& sNodeKey, const CStringArray& sToolTipText, const CString& sNodeKeyImage /*= _T("")*/, const COLORREF color /*= AfxGetThemeManager()->GetTreeViewNodeForeColor()*/)
{
	ASSERTTREE
	return FALSE;
}

//-----------------------------------------------------------------------------
void CTreeViewAdvCtrl::SetSelectRoot()
{
	//TODO:
	ASSERTTREE
}

//-----------------------------------------------------------------------------
void CTreeViewAdvCtrl::SetCustomToolTip(const COLORREF bkColor, const COLORREF foreColor)
{
	//TODO:
	ASSERTTREE
}

//-----------------------------------------------------------------------------
BOOL HasChildrenSelectedNode(const BOOL& bValue = FALSE)
{
	//TODO:
	ASSERTTREE
	return FALSE;
}

//-----------------------------------------------------------------------------
void CTreeViewAdvCtrl::SetSelectFirstChild()
{
	//TODO:
	ASSERTTREE
}

//-----------------------------------------------------------------------------
CString CTreeViewAdvCtrl::GetSelectedNodeKey()
{
	//TODO:
	return _T("");
}

//-----------------------------------------------------------------------------
int	CTreeViewAdvCtrl::GetSelectedNodeLevel()
{
	//TODO:
	ASSERTTREE
	return 0;
}

//-----------------------------------------------------------------------------
void CTreeViewAdvCtrl::GetAllParentsTextNodeFromSelected(CArray<CString>& m_AllParents)
{
	//TODO:
	ASSERTTREE
}

//-----------------------------------------------------------------------------
void CTreeViewAdvCtrl::RemoveContextMenuItem(int idxMenuItem)
{
	//TODO:
	ASSERTTREE
}

//-----------------------------------------------------------------------------
CSize CTreeViewAdvCtrl::GetSize()
{
	//TODO:
	ASSERTTREE
	CSize size;
	return size;
}

//-----------------------------------------------------------------------------
bool CTreeViewAdvCtrl::IsAnimating()
{
	//TODO:
	ASSERTTREE
	CSize size;
	return FALSE;
}

//-----------------------------------------------------------------------------
void CTreeViewAdvCtrl::SetImageNoSel(const CString& sNodeKey, const CString& sImageKey)
{
	//TODO:
	ASSERTTREE
}

//-----------------------------------------------------------------------------
void CTreeViewAdvCtrl::ExpandAll()
{
	HTREEITEM hItem;
	hItem = GetRootItem();
	while (hItem != NULL)
	{
		Expand(hItem, TVE_EXPAND);
		hItem = GetNextItem(hItem, TVGN_NEXTVISIBLE);
	}
}

//-----------------------------------------------------------------------------
void CTreeViewAdvCtrl::CollapseAll()
{
	HTREEITEM hItem;
	hItem = GetRootItem();
	while (hItem != NULL)
	{
		Expand(hItem, TVE_COLLAPSE);
		hItem = GetNextItem(hItem, TVGN_NEXTVISIBLE);
	}
}

//-----------------------------------------------------------------------------
INT CTreeViewAdvCtrl::GetIndentLevel(HTREEITEM hItem)
{
	INT iIndent = 0;
	while ((hItem = GetParentItem(hItem)) != NULL)
		iIndent++;
	return iIndent;
}

//-----------------------------------------------------------------------------
void CTreeViewAdvCtrl::ExpandLevels(INT nLevel)
{
	HTREEITEM hItem;
	hItem = GetFirstVisibleItem();
	INT nItemLev = GetIndentLevel(hItem);
	while (hItem != NULL)
	{
		if (GetIndentLevel(hItem) <= nItemLev + nLevel) {
			Expand(hItem, TVE_EXPAND);
		}
		hItem = GetNextItem(hItem, TVGN_NEXTVISIBLE);
	}
}

//-----------------------------------------------------------------------------
BOOL CTreeViewAdvCtrl::SetNodeAsSelected(const CString& sNodeKeyToSearch)
{
	TreeNodeKey* pTreeNodeKey = FindNnode(sNodeKeyToSearch);
	if (pTreeNodeKey == NULL) return FALSE;
	if (!SelectItem(pTreeNodeKey->GetHTreeItem()))
	{
		ASSERT(FALSE);
		return FALSE;
	}
	return TRUE;
}

//-----------------------------------------------------------------------------
void CTreeViewAdvCtrl::EnableToolBarCommand(int id, bool bEnable)
{
	//TODO:
	ASSERTTREE
}

//-----------------------------------------------------------------------------
void CTreeViewAdvCtrl::SetUpdateTextNode(const int& nrNode, const CString& text)
{
	//TODO:
	ASSERTTREE
}

//-----------------------------------------------------------------------------
void CTreeViewAdvCtrl::SetUpdateTextNode(const CString& nodeKey, const CString& text)
{
	//TODO:
	ASSERTTREE
}

//-----------------------------------------------------------------------------
void CTreeViewAdvCtrl::OnToolBarCommand(int cmdId)
{
	//TODO:
	ASSERTTREE
}

//-----------------------------------------------------------------------------
void CTreeViewAdvCtrl::AddToolBarCommand(int id, const CString& sImage, CString sToolTip /*= L""*/, TCHAR nAccelCharCode /*= 0*/, BOOL bCtrlModifier /*= FALSE*/, BOOL bShiftModifier /*= FALSE*/, BOOL bAltModifier /*= FALSE*/, ToolBarButtonStyleWrapper eButtonStyle /*= E_PUSH_BUTTON*/)
{
	//TODO:
	ASSERTTREE
}

//-----------------------------------------------------------------------------
void CTreeViewAdvCtrl::SetSelectionMode(const E_SELECTIONMODE eSelectionMode)
{
	//TODO:
	ASSERTTREE
}

//-----------------------------------------------------------------------------
HMENU CTreeViewAdvCtrl::GetContextMenuHandle()
{
	//TODO:
	ASSERTTREE
	return NULL;
}

//-----------------------------------------------------------------------------
void CTreeViewAdvCtrl::SetToolbarButtonSize(int nButtonSize)
{
	//TODO:
	ASSERTTREE
}

//-----------------------------------------------------------------------------
void CTreeViewAdvCtrl::SetToolBarButtonPushed(int id, bool bPush)
{
	//TODO:
	ASSERTTREE
}

//-----------------------------------------------------------------------------
bool CTreeViewAdvCtrl::IsToolBarButtonPushed(int id)
{
	//TODO:
	ASSERTTREE
	return FALSE;
}

//-----------------------------------------------------------------------------
CTreeNodeAdvWrapperObj*	CTreeViewAdvCtrl::GetNode(const CString& sNodeKey)
{
	//TODO:
	ASSERTTREE
	return NULL;
}

//-----------------------------------------------------------------------------
void CTreeViewAdvCtrl::EnsureVisible(const CString& sNodeKey)
{
	//TODO:
	ASSERTTREE
	ASSERT(FALSE);
}

//-----------------------------------------------------------------------------
void CTreeViewAdvCtrl::ShowToolBarCommand(int id, bool bShow)
{
	//TODO:
	ASSERTTREE
	ASSERT(FALSE);
}

//-----------------------------------------------------------------------------
void CTreeViewAdvCtrl::SetUseNodesCache(BOOL bUseNodesCache)
{ 
	//TODO:
	ASSERTTREE
}

//-----------------------------------------------------------------------------
void CTreeViewAdvCtrl::SetDblClickWithExpandCollapse(const BOOL& bValue /*= FALSE*/)
{
	//TODO:
	ASSERTTREE
}

//-----------------------------------------------------------------------------
BOOL CTreeViewAdvCtrl::IsExpandedSelectedNode()
{
	//TODO:
	ASSERTTREE
	return FALSE;
}

//-----------------------------------------------------------------------------
void CTreeViewAdvCtrl::AddContextMenuItem(const CString& menuItem)
{
	//TODO:
	ASSERTTREE
}

//-----------------------------------------------------------------------------
void CTreeViewAdvCtrl::AddContextMenuItemDisabled(const CString& menuItem)
{
	//TODO:
	ASSERTTREE
}

//-----------------------------------------------------------------------------
void CTreeViewAdvCtrl::AddContextMenuItemWithConfirm(const CString& menuItem)
{
	//TODO:
	ASSERTTREE
}

//-----------------------------------------------------------------------------
void CTreeViewAdvCtrl::CollapseAllFromSelectedNode()
{
	HTREEITEM hItem = GetSelectedItem();
	if (hItem)
		Expand(hItem, TVE_COLLAPSE);
}

//-----------------------------------------------------------------------------
void CTreeViewAdvCtrl::ExpandAllFromSelectedNode()
{
	//TODO:
	ASSERTTREE
}

//-----------------------------------------------------------------------------
BOOL CTreeViewAdvCtrl::ExistsSelectedNode()
{
	//TODO:
	ASSERTTREE
	return FALSE;
}

//-----------------------------------------------------------------------------
void CTreeViewAdvCtrl::AddContextMenuSeparator()
{
	//TODO:
	ASSERTTREE
}

//-----------------------------------------------------------------------------
void CTreeViewAdvCtrl::SetCancelDragDrop()
{
	//TODO:
	ASSERTTREE
}
//-----------------------------------------------------------------------------
CString CTreeViewAdvCtrl::GetParentKey(const CString& sNodeKeyToSearch)
{
	//TODO:
	ASSERTTREE
	return _T("");
}
//-----------------------------------------------------------------------------
CString	CTreeViewAdvCtrl::GetNewParentKey()
{
	//TODO:
	ASSERTTREE
	return _T("");
}
//-----------------------------------------------------------------------------
int	CTreeViewAdvCtrl::GetIdxContextMenuItemClicked()
{
	//TODO:
	ASSERTTREE
	return 0;
}
//-----------------------------------------------------------------------------
void CTreeViewAdvCtrl::SetBackColorTreeView(const COLORREF color /*= AfxGetThemeManager()->GetTreeViewBkgColor()*/)
{
	//TODO:
	ASSERTTREE
}

//-----------------------------------------------------------------------------
BOOL CTreeViewAdvCtrl::HasChildrenSelectedNode(const BOOL& bValue /*= FALSE*/)
{
	//TODO:
	ASSERTTREE
	return FALSE;
}

//-----------------------------------------------------------------------------
void CTreeViewAdvCtrl::AddContextSubMenuItem(const CString& itemMenu, CArray<CString>& subItems)
{
	//TODO:
	ASSERTTREE
}

//-----------------------------------------------------------------------------
void CTreeViewAdvCtrl::SetMenuItemCheck(const CString& itemMenu, BOOL check /*= TRUE*/)
{
	//TODO:
	ASSERTTREE
}

//-----------------------------------------------------------------------------
void CTreeViewAdvCtrl::SetMenuItemEnable(const CString& itemMenu, BOOL enabled /*= TRUE*/)
{
	//TODO:
	ASSERTTREE
}

//-----------------------------------------------------------------------------
BOOL CTreeViewAdvCtrl::ExistsNode(const CString& sNodeKeyToSearch)
{
	//TODO:
	ASSERTTREE
	return FALSE;
}

//-----------------------------------------------------------------------------
void CTreeViewAdvCtrl::GetTextContextMenuItemClicked(CString& itemKey)
{
	//TODO:
	ASSERTTREE
}

//-----------------------------------------------------------------------------
BOOL CTreeViewAdvCtrl::RemoveNode(const CString& sNodeKeyToSearch)
{
	//TODO:
	ASSERTTREE
	return FALSE;
}

//-----------------------------------------------------------------------------
void CTreeViewAdvCtrl::SetStyleForNode(const CString& nodeKey, const BOOL& bBold /*= FALSE*/, const BOOL& bItalic/* = FALSE*/, const BOOL& bStrikeOut /*= FALSE*/, const BOOL& bUnderline /*= FALSE*/, const COLORREF foreColor /*= AfxGetThemeManager()->GetTreeViewNodeForeColor()*/)
{
	//TODO:
	ASSERTTREE
}

//-----------------------------------------------------------------------------
void CTreeViewAdvCtrl::SetStyleForSelectedNode(const BOOL& bBold /*= FALSE*/, const BOOL& bItalic /*= FALSE*/, const BOOL& bStrikeOut /*= FALSE*/, const BOOL& bUnderline /*= FALSE*/)
{
	//TODO:
	ASSERTTREE
}


//-----------------------------------------------------------------------------
void CTreeViewAdvCtrl::AddToSelectedAndSetNodeAsCurrent(const CString& sNodeText, const CString& sNodeKey)
{
	//TODO:
	ASSERTTREE
}

//-----------------------------------------------------------------------------
void CTreeViewAdvCtrl::AddToSelectedAndSetNodeAsCurrent(const CString& sNodeText, const CString& sNodeKey, const CString& sNodeKeyImage)
{
	//TODO:
	ASSERTTREE
}

//-----------------------------------------------------------------------------
void CTreeViewAdvCtrl::AddToSelectedAndSetNodeAsCurrent(const CString& sNodeText, const CString& sNodeKey, const CStringArray& sToolTipText, const CString& sNodeKeyImage)
{
	//TODO:
	ASSERTTREE
}

//-----------------------------------------------------------------------------
void CTreeViewAdvCtrl::AddAndSetNewNodeFromCurrent(const CString& sNodeText, const CString& sNodeKey)
{
	//TODO:
	ASSERTTREE
}

//-----------------------------------------------------------------------------
void CTreeViewAdvCtrl::AddAndSetNewNodeFromCurrent(const CString& sNodeText, const CString& sNodeKey, const CString& sNodeKeyImage)
{
	//TODO:
	ASSERTTREE
}

//-----------------------------------------------------------------------------
void CTreeViewAdvCtrl::AddAndSetNewNodeFromCurrent(const CString& sNodeText, const CString& sNodeKey, const CStringArray& sToolTipText, const CString& sNodeKeyImage)
{
	//TODO:
	ASSERTTREE
}

//-----------------------------------------------------------------------------
void CTreeViewAdvCtrl::AddAndSetNewNodeFromActual(const CString& sNodeText, const CString& sNodeKey)
{
	//TODO:
	ASSERTTREE
}

//-----------------------------------------------------------------------------
void CTreeViewAdvCtrl::AddAndSetNewNodeFromActual(const CString& sNodeText, const CString& sNodeKey, const CString& sNodeKeyImage)
{
	//TODO:
	ASSERTTREE
}

//-----------------------------------------------------------------------------
void CTreeViewAdvCtrl::AddAndSetNewNodeFromActual(const CString& sNodeText, const CString& sNodeKey, const CStringArray& sToolTipText, const CString& sNodeKeyImage)
{
	//TODO:
	ASSERTTREE
}

//-----------------------------------------------------------------------------
void CTreeViewAdvCtrl::SetForeColorForNode(const CString& nodeKey, const COLORREF foreColor)
{
	//TODO:
	ASSERTTREE
}

//-----------------------------------------------------------------------------
void CTreeViewAdvCtrl::AddToCurrentNode()
{
	//TODO:
	ASSERTTREE
}

//-----------------------------------------------------------------------------
BOOL CTreeViewAdvCtrl::GetContextMenuItemClickedResponse()
{
	//TODO:
	ASSERTTREE
	return FALSE;
}

//-----------------------------------------------------------------------------
CString	CTreeViewAdvCtrl::GetLastChangedNodeKey()
{
	//TODO:
	ASSERTTREE
	return _T("");
}

//-----------------------------------------------------------------------------
int	CTreeViewAdvCtrl::GetOldNodesCount()
{
	//TODO:
	ASSERTTREE
	return 0;
}

//-----------------------------------------------------------------------------
BOOL CTreeViewAdvCtrl::AddFastRoot(const CString& sNodeText, const CString& sNodeKey, const CString& sToolTipText /*= _T("")*/, const CString& sNodeKeyImage /*= _T("")*/)
{
	//TODO:
	ASSERTTREE
	return FALSE;
}

//-----------------------------------------------------------------------------
BOOL CTreeViewAdvCtrl::InsertFastChild(const CString& sParentKey, const CString& sNodeText, const CString& sNodeKey, const CString& sToolTipText /*= _T("")*/, const CString& sNodeKeyImage /*= _T("")*/)
{
	//TODO:
	ASSERTTREE
	return FALSE;
}

//-----------------------------------------------------------------------------
void CTreeViewAdvCtrl::BeginUpdate()
{
	//TODO:
	ASSERTTREE
}

//-----------------------------------------------------------------------------
void CTreeViewAdvCtrl::EndUpdate()
{
	//TODO:
	ASSERTTREE
}
//-----------------------------------------------------------------------------
void CTreeViewAdvCtrl::GetAllChildrenFromSelectedNode(CArray<CString>& m_AllChildren)
{
	//TODO:
	ASSERTTREE
}

//-----------------------------------------------------------------------------
void CTreeViewAdvCtrl::GetAllChildrenFromNodeKey(const CString& nodeKey, CArray<CString>& allChildren)
{
	//TODO:
	ASSERTTREE
}

//-----------------------------------------------------------------------------
void CTreeViewAdvCtrl::CreateNewNode(const CString& sNodeText, const CString& sNodeKey)
{
	//TODO:
	ASSERTTREE
}

//-----------------------------------------------------------------------------
void CTreeViewAdvCtrl::CreateNewNode(const CString& sNodeText, const CString& sNodeKey, const CString& sNodeKeyImage)
{
	//TODO:
	ASSERTTREE
}

//-----------------------------------------------------------------------------
void CTreeViewAdvCtrl::CreateNewNode(const CString& sNodeText, const CString& sNodeKey, const CStringArray& sToolTipText, const CString& sNodeKeyImage)
{
	//TODO:
	ASSERTTREE
}

//-----------------------------------------------------------------------------
void CTreeViewAdvCtrl::DeleteAllChildrenFromSelectedNode()
{
	//TODO:
	ASSERTTREE
}

//-----------------------------------------------------------------------------
bool CTreeViewAdvCtrl::IsIgnoreSelectionChanged()
{
	//TODO:
	ASSERTTREE
	return FALSE;
}

//-----------------------------------------------------------------------------
void CTreeViewAdvCtrl::SetIgnoreSelectionChanged(bool bIgnore)
{ 
	//TODO:
	ASSERTTREE
}

//-----------------------------------------------------------------------------
void CTreeViewAdvCtrl::OldNodesClear()
{
	//TODO:
	ASSERTTREE
}

//-----------------------------------------------------------------------------
void CTreeViewAdvCtrl::OldNodesPop()
{
	//TODO:
	ASSERTTREE
}

//-----------------------------------------------------------------------------
void CTreeViewAdvCtrl::OldNodesPeek()
{
	//TODO:
	ASSERTTREE
}

//-----------------------------------------------------------------------------
void CTreeViewAdvCtrl::OldNodesPush()
{
	//TODO:
	ASSERTTREE
}

//-----------------------------------------------------------------------------
void CTreeViewAdvCtrl::SetAllowDragOver(const BOOL& bValue /*= FALSE*/)
{
	//TODO:
	ASSERTTREE
}

//-----------------------------------------------------------------------------
void CTreeViewAdvCtrl::SetCaptionBoxConfirm(const CString& caption)
{
	//TODO:
	ASSERTTREE
}

//-----------------------------------------------------------------------------
void CTreeViewAdvCtrl::SetCheckBoxControls(const BOOL& bValue /*= FALSE*/)
{
	//TODO:
	ASSERTTREE
}

//-----------------------------------------------------------------------------
void CTreeViewAdvCtrl::SetColorNewNode(const COLORREF color)
{
	//TODO:
	ASSERTTREE
}

//-----------------------------------------------------------------------------
void CTreeViewAdvCtrl::SetColorCurrentNode(const COLORREF color)
{
	//TODO:
	ASSERTTREE
}

//-----------------------------------------------------------------------------
void CTreeViewAdvCtrl::GetAllNodesFromSelection(CArray<CString>& m_AllNodesLeaf, CArray<CString>& m_AllNodesNotLeaf)
{
	//TODO:
	ASSERTTREE
}

//-----------------------------------------------------------------------------
void CTreeViewAdvCtrl::SetSelectionOnlyOnLevel(const int& nLevelOnly)
{
	//TODO:
	ASSERTTREE
}

//-----------------------------------------------------------------------------
void CTreeViewAdvCtrl::SetTextBoxConfirm(const CString& text)
{
	//TODO:
	ASSERTTREE
}

//-----------------------------------------------------------------------------
void CTreeViewAdvCtrl::AddToSelectedNode()
{
	//TODO:
	ASSERTTREE
}

//-----------------------------------------------------------------------------
void CTreeViewAdvCtrl::SetChecksBoxEditable(const BOOL& bValue /*= TRUE*/)
{
	//TODO:
	ASSERTTREE
}

//-----------------------------------------------------------------------------
void CTreeViewAdvCtrl::SetCurrentNodeParentSelected()
{
	//TODO:
	ASSERTTREE
}

//-----------------------------------------------------------------------------
void CTreeViewAdvCtrl::SetNextNode()
{
	//TODO:
	ASSERTTREE
}

//-----------------------------------------------------------------------------
void CTreeViewAdvCtrl::SetPrevNode()
{
	//TODO:
	ASSERTTREE
}

//-----------------------------------------------------------------------------
CString	CTreeViewAdvCtrl::GetTextNode(const int& nrNode)
{
	//TODO:
	ASSERTTREE
	return _T("");
}

//-----------------------------------------------------------------------------
LRESULT CTreeViewAdvCtrl::OnGetControlDescription(WPARAM wParam, LPARAM lParam)
{
	CWndObjDescriptionContainer* pContainer = (CWndObjDescriptionContainer*)wParam;

	/*Il metodo GetWindowDescription crea da zero una descrizione(del tipo della runtimeclass passata) se non esisteva gia,
	o ripesca quella gia creata nei round trip precedenti se esisteva.
	Nella creazione assegna un id alla descrizione (m_strId), che servira' da chiave per recuperarle.
	Questo id viene creato in modo standard sulla base dell'handle di finestra.
	In alcuni casi pero finestre "differenti" hanno lo stesso id (es. parsedbitmap del bodyedit). 
	In questi casi si puo' creare un ID disambiguo e passarlo al metodo GetWindowDescription.
	*/
	CString strId = (LPCTSTR)lParam;
	CWndTreeCtrlDescription* pTreeDesc = (CWndTreeCtrlDescription*)pContainer->GetWindowDescription(this, RUNTIME_CLASS(CWndTreeCtrlDescription), strId);

	//TDDO: da riscrivere

	return (LRESULT) pTreeDesc;
}


//=============================================================================
//			Class TreeNodeKey
//=============================================================================

//-----------------------------------------------------------------------------
TreeNodeKey::TreeNodeKey()
{
}

//-----------------------------------------------------------------------------
TreeNodeKey::TreeNodeKey(HTREEITEM hNode, const CString& sNodeKeyImage /*= _T("")*/, const COLORREF color /*= AfxGetThemeManager()->GetTreeViewNodeForeColor()*/) :
	m_hNode(hNode),
	m_sNodeKeyImage(sNodeKeyImage),
	m_cColor(color)
{
}

#else
//=============================================================================
//			Class CTreeViewAdvCtrl
//=============================================================================
IMPLEMENT_DYNCREATE (CTreeViewAdvCtrl, CManagedParsedCtrl)

//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CTreeViewAdvCtrl, CManagedParsedCtrl)
	ON_WM_SETFOCUS()
	ON_WM_MOUSEMOVE()
	ON_MESSAGE(UM_GET_CONTROL_DESCRIPTION, OnGetControlDescription)
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
LRESULT CTreeViewAdvCtrl::OnGetControlDescription(WPARAM wParam, LPARAM lParam)
{
	CWndObjDescriptionContainer* pContainer = (CWndObjDescriptionContainer*)wParam;

	/*Il metodo GetWindowDescription crea da zero una descrizione(del tipo della runtimeclass passata) se non esisteva gia,
	o ripesca quella gia creata nei round trip precedenti se esisteva.
	Nella creazione assegna un id alla descrizione (m_strId), che servira' da chiave per recuperarle.
	Questo id viene creato in modo standard sulla base dell'handle di finestra.
	In alcuni casi pero finestre "differenti" hanno lo stesso id (es. parsedbitmap del bodyedit).
	In questi casi si puo' creare un ID disambiguo e passarlo al metodo GetWindowDescription.
	*/
	CString strId = (LPCTSTR)lParam;
	CWndTreeCtrlDescription* pTreeDesc = (CWndTreeCtrlDescription*)pContainer->GetWindowDescription(this, RUNTIME_CLASS(CWndTreeCtrlDescription), strId);
	pTreeDesc->UpdateAttributes(this);
	pTreeDesc->m_Type = CWndObjDescription::TreeAdv;

	return (LRESULT)pTreeDesc;
}

//-----------------------------------------------------------------------------
CTreeViewAdvCtrl::CTreeViewAdvCtrl()
{
	m_LastTextToolTip = _T("");
	m_bToolTip = FALSE;

	m_ToolTip.Create(this);
	m_ToolTip.SetCWND(this);
}

//-----------------------------------------------------------------------------
BOOL CTreeViewAdvCtrl::OnCreateManaged (DWORD dwStyle, const RECT& rect, CWnd* pParentWnd, UINT nID)
{
	try
	{
		return CTreeViewAdvWrapper::CreateControl(dwStyle, rect, pParentWnd, nID, (CWnd*) this);
	}
	catch (...)
	{
		return FALSE;
	}
}

//-----------------------------------------------------------------------------
BOOL CTreeViewAdvCtrl::OnEnableCtrlManaged(BOOL bEnable /*TRUE*/)
{
	CTreeViewAdvWrapper::Enable(bEnable);
	return TRUE;
}

//-----------------------------------------------------------------------------
HWND CTreeViewAdvCtrl::GetManagedHandle ()
{
	return CTreeViewAdvWrapper::GetControlHandle();
}


//-----------------------------------------------------------------------------
void CTreeViewAdvCtrl::SelectNode(CString keyNode)
{
	CTreeViewAdvWrapper::SelectNode(keyNode);
}

//-----------------------------------------------------------------------------
void CTreeViewAdvCtrl::ToggleNode(CString keyNode)
{
	CTreeViewAdvWrapper::ToggleNode(keyNode);
}

//-----------------------------------------------------------------------------
BOOL CTreeViewAdvCtrl::EnableCtrl(BOOL bEnable /* = TRUE */)
{
	OnEnableCtrlManaged(bEnable);
	return TRUE;
}

//-----------------------------------------------------------------------------
void CTreeViewAdvCtrl::OnAfterAttachControl ()
{
	CTreeViewAdvWrapper::OnAfterAttachControl();
}

//-----------------------------------------------------------------------------
void CTreeViewAdvCtrl::OnSetFocus(CWnd* pOldWnd)
{
	//TODO verificare con calma
	//CTreeViewAdvWrapper::Focus();
	__super::OnSetFocus(pOldWnd);
}

//-----------------------------------------------------------------------------
void CTreeViewAdvCtrl::OnMouseMove(UINT nFlags, CPoint point)
{
	CString	textToolTip = GetKeyByPosition(point);
	if (m_LastTextToolTip.Compare(textToolTip) != 0)
	{
		m_LastTextToolTip = textToolTip;
		m_ToolTip.UpdateTipText(textToolTip, this);
		m_ToolTip.Activate(!m_LastTextToolTip.IsEmpty());
		m_ToolTip.Update();
	}
	
	__super::OnMouseMove(nFlags, point);
}

void CTreeViewAdvCtrl::EnableToolTip()
{
	if (m_bToolTip)
	{
		m_ToolTip.Activate(FALSE);
		return;
	}
		
	m_ToolTip.AddTool(this, _T("tree control!"));
	m_ToolTip.Activate(FALSE);
	m_bToolTip = TRUE;
}

//-----------------------------------------------------------------------------
BOOL CTreeViewAdvCtrl::PreTranslateMessage(LPMSG lpmsg)
{
	BOOL bHandleNow = FALSE;

	switch (lpmsg->message)
	{

	case WM_MOUSEMOVE:
		m_ToolTip.RelayEvent(lpmsg);
		break;

	case WM_KEYDOWN:

		switch (lpmsg->wParam)
		{
			case VK_UP:
				CTreeViewAdvWrapper::SetPrevNode();
				break;
			case VK_DOWN:
				CTreeViewAdvWrapper::SetNextNode();
				break;

			case VK_ADD:
				if (!CTreeViewAdvWrapper::IsExpandedSelectedNode())
				{
					CTreeViewAdvWrapper::ExpandAllFromSelectedNode();
				}
				else
				{
					CTreeViewAdvWrapper::CollapseAllFromSelectedNode();
				}
				break;
			case VK_F2:
			{
				if (CTreeViewAdvWrapper::IsEditable())
				{
					CTreeViewAdvWrapper::BeginEdit();
					return TRUE;
				}
				break;
			}
			case VK_ESCAPE:
			case VK_RETURN:
			{
				if (CTreeViewAdvWrapper::IsEditable())
				{
					CTreeViewAdvWrapper::EndEdit(lpmsg->wParam == VK_ESCAPE);
					return TRUE;
				}
				break;
			}
		}
	}
	return __super::PreTranslateMessage(lpmsg);
}
#endif
//=============================================================================
//			Class CTBPicViewerAdvCtrl
//=============================================================================
IMPLEMENT_DYNCREATE(CTBPicViewerAdvCtrl, CManagedParsedCtrl)

//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CTBPicViewerAdvCtrl, CManagedParsedCtrl)
	ON_MESSAGE	(UM_RECALC_CTRL_SIZE, OnRecalcCtrlSize)
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CTBPicViewerAdvCtrl::CTBPicViewerAdvCtrl()
	:
	m_bSkipRecalcCtrlSize(FALSE)
{
}

//-----------------------------------------------------------------------------
CTBPicViewerAdvCtrl::~CTBPicViewerAdvCtrl()
{
}

//-----------------------------------------------------------------------------
BOOL CTBPicViewerAdvCtrl::OnCreateManaged(DWORD dwStyle, const RECT& rect, CWnd* pParentWnd, UINT nID)
{
	return CTBPicViewerAdvWrapper::CreateControl(dwStyle, rect, pParentWnd, nID, (CWnd*) this);
}

//-----------------------------------------------------------------------------
BOOL CTBPicViewerAdvCtrl::OnEnableCtrlManaged(BOOL bEnable /*TRUE*/)
{
	CTBPicViewerAdvWrapper::Enabled(bEnable);
	return TRUE;
}

//-----------------------------------------------------------------------------
HWND CTBPicViewerAdvCtrl::GetManagedHandle()
{
	return CTBPicViewerAdvWrapper::GetControlHandle();
}

//------------------------------------------------------------------------------
BOOL CTBPicViewerAdvCtrl::SkipRecalcCtrlSize()
{
	if (!__super::SkipRecalcCtrlSize())
		return FALSE;

	return m_bSkipRecalcCtrlSize;
}

//------------------------------------------------------------------------------
LRESULT CTBPicViewerAdvCtrl::OnRecalcCtrlSize(WPARAM, LPARAM)
{
	DoRecalcCtrlSize();
	return 0L;
}

//-----------------------------------------------------------------------------
BOOL CTBPicViewerAdvCtrl::SubclassEdit(UINT IDC, CWnd* pParent, const CString& strName)
{
	if (!__super::SubclassEdit(IDC, pParent, strName))
		return FALSE;

	ResizableCtrl::InitSizeInfo(this);
	return TRUE;
}


//-----------------------------------------------------------------------------
void CTBPicViewerAdvCtrl::OnAfterAttachControl()
{
	CTBPicViewerAdvWrapper::OnAfterAttachControl();
}

// di default il controllo e' attrezzato per visualizzare l'anteprima di un file 
//-----------------------------------------------------------------------------
void CTBPicViewerAdvCtrl::SetValue(const DataObj& aValue)
{
	CString strValue = ((DataStr&)aValue).GetString();
	if (strValue.IsEmpty() || !ExistFile(strValue))
		return;

	CTBPicViewerAdvWrapper::DisplayFromFile(strValue);
}