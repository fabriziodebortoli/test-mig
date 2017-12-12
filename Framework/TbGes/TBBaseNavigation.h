#pragma once

// Components
#include <TbFrameworkImages\CommonImages.h>
#include <TbFrameworkImages\GeneralFunctions.h>
#include <TbNameSolver\PathFinder.h>

// TaskBuilder
#include <tbges\bodyedittree.h>
#include <TbGenlib\TBSplitterWnd.h>

#include "TBActivityDocument.h"
#include "TBBaseNavigation.hjson"

#include "beginh.dex"

enum E_BASE_NAVIGATION_SPLITTYPE {
	BASE_NAVIGATION_HORIZONTAL,		//HORIZONTAL
	BASE_NAVIGATION_VERTICAL		//VERTICAL
};

/////////////////////////////////////////////////////////////////////////////
//			Class TNodeDetail
/////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
class TNodeDetail : public SqlVirtualRecord
{
	DECLARE_DYNCREATE(TNodeDetail)

public:
	DataStr			l_FieldName;
	DataStr			l_FieldValue;
	DataBool		l_HasHyperlink;
	DataBool		l_IsSeparator;

public:
	TNodeDetail								(BOOL bCallInit = TRUE);

public:
	virtual void	BindRecord				();

public:
	static LPCTSTR   GetStaticName			();
};

/////////////////////////////////////////////////////////////////////////////
//			Class DBTNodeDetail
/////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
class DBTNodeDetail : public DBTSlaveBuffered
{
	DECLARE_DYNAMIC(DBTNodeDetail)

public:
	DBTNodeDetail		(CRuntimeClass* pClass, CAbstractFormDoc* pDoc);

};

///////////////////////////////////////////////////////////////////////////////
//					class CTreeViewManager definition
///////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
class CTreeViewManager
{

protected:
	enum E_SELECTIONMODE { F_SINGLE, F_MULTI, F_MULTISAMEPARENT };

private:
	TDisposablePtr<CTreeViewAdvCtrl>	m_pTreeView;

public:	//constructor
	CTreeViewManager()  {}
	~CTreeViewManager() {}

protected:
	void SetTreeViewObject(CTreeViewAdvCtrl* pTreeView) 
	{
		if (!m_pTreeView)
			m_pTreeView = pTreeView;

		ASSERT(m_pTreeView);
	}

public:		//wrapping public methods
	void ClearTree()
	{
		m_pTreeView->ClearTree();
	}

	void AddImage(const CString& imgKey, const CString& imgPath)
	{
		m_pTreeView->AddImage(imgKey, imgPath);
	}

	void SetImage(const CString& nodeKey, const CString& imgKey)
	{
		m_pTreeView->SetImage(nodeKey, imgKey);
	}

	void SetBackColorTreeView(COLORREF clrBkg)
	{
		m_pTreeView->SetBackColorTreeView(clrBkg);
	}

	void SetDblClickWithExpandCollapse(BOOL bSet = TRUE)
	{
		m_pTreeView->SetDblClickWithExpandCollapse(bSet);
	}

	void AddControls()
	{
		m_pTreeView->AddControls();
	}

	void SetViewContextMenu(BOOL bSet = TRUE)
	{
		m_pTreeView->SetViewContextMenu(bSet);
	}

	void SetNodeStateIcon(BOOL bSet = TRUE)
	{
		m_pTreeView->SetNodeStateIcon(bSet);
	}

	void SetAllowDrop(BOOL bSet = FALSE)
	{
		m_pTreeView->SetAllowDrop(bSet);
	}

	void SetAllowDragOver(BOOL bSet = FALSE)
	{
		m_pTreeView->SetAllowDragOver(bSet);
	}

	BOOL SetNodeAsSelected(const CString& nodeKey)
	{
		return m_pTreeView->SetNodeAsSelected(nodeKey);
	}

	BOOL IsExpandedSelectedNode()
	{
		return m_pTreeView->IsExpandedSelectedNode();
	}

	void CollapseAllFromSelectedNode()
	{
		m_pTreeView->CollapseAllFromSelectedNode();
	}

	void ExpandAllFromSelectedNode()
	{
		m_pTreeView->ExpandAllFromSelectedNode();
	}

	CString GetSelectedNodeKey()
	{
		return m_pTreeView->GetSelectedNodeKey();
	}

	int GetSelectedNodeLevel()
	{
		return m_pTreeView->GetSelectedNodeLevel();
	}

	void GetAllChildrenFromSelectedNode(CArray<CString>& allChildren)
	{
		m_pTreeView->GetAllChildrenFromSelectedNode(allChildren);
	}

	void GetAllChildrenFromNodeKey(const CString& nodeKey, CArray<CString>& allChildren)
	{
		m_pTreeView->GetAllChildrenFromNodeKey(nodeKey, allChildren);
	}

	void GetAllNodesFromSelection(CArray<CString>& allNodesLeaf, CArray<CString>& allNodeNotLeaf)
	{
		m_pTreeView->GetAllNodesFromSelection(allNodesLeaf, allNodeNotLeaf);
	}

	void GetAllParentsTextNodeFromSelected(CArray<CString>& allParents)
	{
		m_pTreeView->GetAllParentsTextNodeFromSelected(allParents);
	}

	BOOL RemoveNode(const CString& nodeKeyToSearch)
	{
		return m_pTreeView->RemoveNode(nodeKeyToSearch);
	}

	void ExpandAll()
	{
		m_pTreeView->ExpandAll();
	}

	void SetSelectRoot()
	{
		m_pTreeView->SetSelectRoot();
	}

	void SetDragAndDropOnSameLevel(BOOL bSet = FALSE)
	{
		m_pTreeView->SetDragAndDropOnSameLevel(bSet);
	}

	void SetSelectionMode(E_SELECTIONMODE eMode)
	{
		CTreeViewAdvWrapper::E_SELECTIONMODE eTreeMode;
		
		switch (eMode)
		{
			case E_SELECTIONMODE::F_SINGLE:
				eTreeMode = CTreeViewAdvWrapper::E_SELECTIONMODE::F_SINGLE;
				break;
			case E_SELECTIONMODE::F_MULTI:
				eTreeMode = CTreeViewAdvWrapper::E_SELECTIONMODE::F_MULTI;
				break;
			case E_SELECTIONMODE::F_MULTISAMEPARENT:
				eTreeMode = CTreeViewAdvWrapper::E_SELECTIONMODE::F_MULTISAMEPARENT;
				break;
			default:
				eTreeMode = CTreeViewAdvWrapper::E_SELECTIONMODE::F_SINGLE;
		}
		m_pTreeView->SetSelectionMode(eTreeMode);
	}

	void SetCustomToolTip(COLORREF bkgColor, COLORREF foreColor)
	{
		m_pTreeView->SetCustomToolTip(bkgColor, foreColor);
	}

	void SetStyleForNode(const CString& nodeKey, const BOOL& bBold = FALSE, const BOOL& bItalic = FALSE, const BOOL& bStrikeOut = FALSE, const BOOL& bUnderline = FALSE, const COLORREF foreColor = AfxGetThemeManager()->GetTreeViewNodeForeColor())
	{
		m_pTreeView->SetStyleForNode(nodeKey, bBold, bItalic, bStrikeOut, bUnderline, foreColor);
	}

	void SetForeColorForNode(const CString& nodeKey, const COLORREF foreColor)
	{
		m_pTreeView->SetForeColorForNode(nodeKey, foreColor);
	}

	CString GetNodeText(const CString& sNodeKey)
	{
		CTreeNodeAdvWrapperObj* pNode = m_pTreeView->GetNode(sNodeKey);

		return pNode->GetText();
	}

	BOOL IsNodeChecked(const CString& sNodeKey)
	{
		CTreeNodeAdvWrapperObj* pNode = m_pTreeView->GetNode(sNodeKey);

		return pNode->IsChecked();
	}

	BOOL HasChildrenSelectedNode()
	{
		return m_pTreeView->HasChildrenSelectedNode();
	}

	CString GetParentKey(const CString& nodeKeyToSearch)
	{
		return m_pTreeView->GetParentKey(nodeKeyToSearch);
	}

	void AddContextMenuItem(const CString& menuItem)
	{
		m_pTreeView->AddContextMenuItem(menuItem);
	}

	void AddContextMenuItemDisabled(const CString& menuItem)
	{
		m_pTreeView->AddContextMenuItemDisabled(menuItem);
	}

	void AddContextMenuItemWithConfirm(const CString menuItem)
	{
		m_pTreeView->AddContextMenuItemWithConfirm(menuItem);
	}

	void AddContextMenuSeparator()
	{
		m_pTreeView->AddContextMenuSeparator();
	}

	void AddContextMenuSubMenu(const CString& menuItem, CArray<CString> subItems)
	{
		m_pTreeView->AddContextSubMenuItem(menuItem, subItems);
	}

	int GetIdxContextMenuItemClicked()
	{
		return m_pTreeView->GetIdxContextMenuItemClicked();
	}

	void BeginUpdate()
	{
		m_pTreeView->BeginUpdate();
	}

	void EndUpdate()
	{
		m_pTreeView->EndUpdate();
	}

	void ExpandLevels(int nLevel)
	{
		m_pTreeView->ExpandLevels(nLevel);
	}

	BOOL ExistsSelectedNode()
	{
		return m_pTreeView->ExistsSelectedNode();
	}

	void OldNodesClear()
	{
		m_pTreeView->OldNodesClear();
	}

	int GetOldNodesCount()
	{
		return m_pTreeView->GetOldNodesCount();
	}

	void OldNodesPop()
	{
		m_pTreeView->OldNodesPop();
	}

	void OldNodesPush()
	{
		m_pTreeView->OldNodesPush();
	}

	void OldNodesPeek()
	{
		m_pTreeView->OldNodesPeek();
	}

	void SetUpdateTextnode(const CString& nodeKey, const CString& text)
	{
		m_pTreeView->SetUpdateTextNode(nodeKey, text);
	}

	void SetUpdateTextnode(const int& nrNode, const CString& text)
	{
		m_pTreeView->SetUpdateTextNode(nrNode, text);
	}

	CString GetTextNode(const int& nNode)
	{
		return m_pTreeView->GetTextNode(nNode);
	}

	void SetSelectFirstChild()
	{
		m_pTreeView->SetSelectFirstChild();
	}

	void SetNextNode()
	{
		m_pTreeView->SetNextNode();
	}

	void DeleteAllChildrenFromSelectedNode()
	{
		m_pTreeView->DeleteAllChildrenFromSelectedNode();
	}

	void SetSelectionOnlyOnLevel(int nLevelOnly)
	{
		m_pTreeView->SetSelectionOnlyOnLevel(nLevelOnly);
	}

	CString GetLastChangedNodeKey()
	{
		return m_pTreeView->GetLastChangedNodeKey();
	}

	void SetColorNewNode(const COLORREF color)
	{
		m_pTreeView->SetColorNewNode(color);
	}

	int GetContextMenuItemClickedResponse()
	{
		return m_pTreeView->GetContextMenuItemClickedResponse();
	}

	void SetCaptionBoxConfirm(const CString& caption)
	{
		m_pTreeView->SetCaptionBoxConfirm(caption);
	}

	void SetTextBoxConfirm(const CString& text)
	{
		m_pTreeView->SetTextBoxConfirm(text);
	}

	void SetCurrentNodeParentSelected()
	{
		m_pTreeView->SetCurrentNodeParentSelected();
	}

	BOOL ExistsNode(const CString& nodeKeyToSearch)
	{
		return m_pTreeView->ExistsNode(nodeKeyToSearch);
	}

	BOOL AddNode(const CString& nodeText, const CString& nodeKey, const CString& nodeKeyImage = _T(""), const COLORREF color = AfxGetThemeManager()->GetTreeViewNodeForeColor())
	{
		return m_pTreeView->AddNode(nodeText, nodeKey, nodeKeyImage, color);
	}
	BOOL AddNode(const CString& nodeText, const CString& nodeKey, const CStringArray& toolTipText, const CString& nodeKeyImage = _T(""), const COLORREF color = AfxGetThemeManager()->GetTreeViewNodeForeColor())
	{
		return m_pTreeView->AddNode(nodeText, nodeKey, toolTipText, nodeKeyImage, color);
	}
	BOOL AddNode(const CString& nodeText, const CString& nodeKey, const BOOL& checkBoxNode)
	{
		return m_pTreeView->AddNode(nodeText, nodeKey, checkBoxNode);
	}
	BOOL AddChild(const CString& nodeText, const CString& nodeKey, const CString& nodeKeyImage = _T(""), const COLORREF color = AfxGetThemeManager()->GetTreeViewNodeForeColor())
	{
		return m_pTreeView->AddChild(nodeText, nodeKey, nodeKeyImage, color);
	}

	BOOL AddChild(const CString& nodeText, const CString& nodeKey, const CStringArray& toolTipText, const CString& nodeKeyImage = _T(""), const COLORREF color = AfxGetThemeManager()->GetTreeViewNodeForeColor())
	{
		return m_pTreeView->AddChild(nodeText, nodeKey, toolTipText, nodeKeyImage, color);
	}

	BOOL AddFastNodeToSelected(const CString& nodeText, const CString& nodeKey, const CString& toolTiptext = _T(""), const CString& nodeKeyImage = _T(""))
	{
		return m_pTreeView->AddFastNodeToSelected(nodeText, nodeKey, toolTiptext, nodeKeyImage);
	}

	BOOL AddFastRoot(const CString& nodeText, const CString& nodeKey, const CString& toolTipText = _T(""), const CString& nodeKeyImage = _T(""))
	{
		return m_pTreeView->AddFastRoot(nodeText, nodeKey, toolTipText, nodeKeyImage);
	}

	BOOL InsertFastChild(const CString& parentKey, const CString& nodeText, const CString& nodeKey, const CString& toolTipText = _T(""), const CString& nodeKeyImage = _T(""))
	{
		return m_pTreeView->InsertFastChild(parentKey, nodeText, nodeKey, toolTipText, nodeKeyImage);
	}

	void AddToSelectedNode()
	{
		m_pTreeView->AddToSelectedNode();
	}

	void AddToCurrentNode()
	{
		m_pTreeView->AddToCurrentNode();
	}

	void SetColorCurrentNode(const COLORREF color)
	{
		m_pTreeView->SetColorCurrentNode(color);
	}

	void AddToSelectedAndSetNodeAsCurrent(const CString& nodeText, const CString& nodeKey)
	{
		m_pTreeView->AddToSelectedAndSetNodeAsCurrent(nodeText, nodeKey);
	}
	void AddToSelectedAndSetNodeAsCurrent(const CString& nodeText, const CString& nodeKey, const CString& nodeKeyImage)
	{
		m_pTreeView->AddToSelectedAndSetNodeAsCurrent(nodeText, nodeKey, nodeKeyImage);
	}

	void AddToSelectedAndSetNodeAsCurrent(const CString& nodeText, const CString& nodeKey, const CStringArray& toolTipText, const CString& nodeKeyImage)
	{
		m_pTreeView->AddToSelectedAndSetNodeAsCurrent(nodeText, nodeKey, toolTipText, nodeKeyImage);
	}

	void AddAndSetNewNodeFromCurrent(const CString& nodeText, const CString& nodeKey)
	{
		m_pTreeView->AddAndSetNewNodeFromCurrent(nodeText, nodeKey);
	}

	void AddAndSetNewNodeFromCurrent(const CString& nodeText, const CString& nodeKey, const CString& nodeKeyImage)
	{
		m_pTreeView->AddAndSetNewNodeFromCurrent(nodeText, nodeKey, nodeKeyImage);
	}

	void AddAndSetNewNodeFromCurrent(const CString& nodeText, const CString& nodeKey, const CStringArray& toolTipText, const CString& nodeKeyImage)
	{
		m_pTreeView->AddAndSetNewNodeFromCurrent(nodeText, nodeKey, toolTipText, nodeKeyImage);
	}

	void AddAndSetNewNodeFromActual(const CString& nodeText, const CString& nodeKey)
	{
		m_pTreeView->AddAndSetNewNodeFromActual(nodeText, nodeKey);
	}

	void AddAndSetNewNodeFromActual(const CString& nodeText, const CString& nodeKey, const CString& nodeKeyImage)
	{
		m_pTreeView->AddAndSetNewNodeFromActual(nodeText, nodeKey, nodeKeyImage);
	}

	void AddAndSetNewNodeFromActual(const CString& nodeText, const CString& nodeKey, const CStringArray& toolTipText, const CString& nodeKeyImage)
	{
		m_pTreeView->AddAndSetNewNodeFromActual(nodeText, nodeKey, toolTipText, nodeKeyImage);
	}

	void DeleteSelectedNode()
	{
		m_pTreeView->DeleteSelectedNode();
	}

	void CreateNewNode(const CString& nodeText, const CString& nodeKey)
	{
		m_pTreeView->CreateNewNode(nodeText, nodeKey);
	}

	void CreateNewNode(const CString& nodeText, const CString& nodeKey, const CString& nodeKeyImage)
	{
		m_pTreeView->CreateNewNode(nodeText, nodeKey, nodeKeyImage);
	}
	
	void CreateNewNode(const CString& nodeText, const CString& nodeKey, const CStringArray& toolTipText, const CString& nodeKeyImage)
	{
		m_pTreeView->CreateNewNode(nodeText, nodeKey, toolTipText, nodeKeyImage);
	}

	BOOL InsertChild(const CString& parentKey, const CString& nodeText, const CString& nodeKey, const CString& nodeKeyImage = _T(""), const COLORREF color = AfxGetThemeManager()->GetTreeViewNodeForeColor())
	{
		return m_pTreeView->InsertChild(parentKey, nodeText, nodeKey, nodeKeyImage, color);
	}

	BOOL InsertChild(const CString& parentKey, const CString& nodeText, const CString& nodeKey, const CStringArray& toolTipText, const CString& nodeKeyImage = _T(""), const COLORREF color = AfxGetThemeManager()->GetTreeViewNodeForeColor())
	{
		return m_pTreeView->InsertChild(parentKey, nodeText, nodeKey, toolTipText, nodeKeyImage, color);
	}

	void SetCancelDragDrop()
	{
		m_pTreeView->SetCancelDragDrop();
	}

	CString GetNewParentKey()
	{
		return m_pTreeView->GetNewParentKey();
	}
};

/////////////////////////////////////////////////////////////////////////////
//					class CTBBaseNavigationFrame definition
/////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
class TB_EXPORT CTBBaseNavigationFrame : public CTBActivityFrame
{
	DECLARE_DYNCREATE(CTBBaseNavigationFrame)

public:
	CTBBaseNavigationFrame();

protected:
	virtual BOOL OnCustomizeJsonToolBar		();
	virtual	BOOL CreateAuxObjects			(CCreateContext*	pCreateContext);
	virtual BOOL OnPopulatedDropDown		(UINT nIDCommand);
	virtual BOOL UseSplitters()				{ return TRUE; };

private:
	virtual BOOL OnCustomizeTabbedToolBar	(CTBTabbedToolbar* pTabbedBar) { return TRUE; }//metodo che va a morire => da non potterlo reimplementare nelle classi derivate
};

/////////////////////////////////////////////////////////////////////////////
//					class CTBBaseNavigationDocument definition
/////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
class TB_EXPORT CTBBaseNavigationDocument : public CTBActivityDocument, public CTreeViewManager
{
	DECLARE_DYNCREATE(CTBBaseNavigationDocument)

	friend class CTBBaseNavigationFrame;
	
public:
	CTBBaseNavigationDocument();
	virtual ~CTBBaseNavigationDocument();

private:
	//!!!!variabili privatissime	
	CTaskBuilderSplitterWnd*		m_pSplitter;
	E_BASE_NAVIGATION_SPLITTYPE		m_eSplitType;
	DBTNodeDetail*					m_pDBTNodeDetail;
	BOOL							m_bTreeViewLoaded;
	BOOL							m_bShowDetails;
	float							m_nSplitterRatio;
	CRuntimeClass*					m_pFirstView;
	CRuntimeClass*					m_pSecondView;
	BOOL							m_bShowDetailsSettings;
	CString							m_LastSelectedNode;
	CStrStatic*						m_pControlFieldValue;

public:
			void					ManageDescriptionDetailHKL		(HotKeyLinkObj* pHKLObj);
			void					RemoveNodeDetails				();
			CString					GetLastSelectedNode				() { return m_LastSelectedNode;}
			BOOL					LoadTree						();
			void					ClearTreeAndEmptyDetails		();
			void					SetSplitterRatio				(float nRatio) { m_nSplitterRatio = nRatio; }
			void					AddDetail						(const DataStr& fieldName, const DataObj& fieldValue, BOOL hasHyperLink);
			void					AddSeparator					(const DataStr& fieldName);
			BOOL					IsTreeViewLoaded				() { return m_bTreeViewLoaded; }
			BOOL					GetShowDetails					() { return m_bShowDetailsSettings; }
			void					SetShowDetails					(BOOL bSet) { m_bShowDetailsSettings = bSet; }
			//deprecated - DO NOT USE ANYMORE - for now: just for compilation
			//describe your splitter in your json
			void					SetViews						(CRuntimeClass* pFirstView, CRuntimeClass* pSecondView) 
			{ 
				m_pFirstView = pFirstView; 
				m_pSecondView = pSecondView; 
			}
			
			DataStr					GetCurrentFieldName				() 
			{ 
				ASSERT(m_pDBTNodeDetail->GetCurrentRow());

				if (m_pDBTNodeDetail->GetCurrentRow())
					return ((TNodeDetail*)m_pDBTNodeDetail->GetCurrentRow())->l_FieldName;

				return _T("");
			}

			DataStr					GetCurrentFieldValue			()
			{
				ASSERT(m_pDBTNodeDetail->GetCurrentRow());

				if (m_pDBTNodeDetail->GetCurrentRow())
					return ((TNodeDetail*)m_pDBTNodeDetail->GetCurrentRow())->l_FieldValue;

				return _T("");
			}

			BOOL					HasCurrentFieldValueHyperlink	()
			{
				ASSERT(m_pDBTNodeDetail->GetCurrentRow());

				if (m_pDBTNodeDetail->GetCurrentRow())
					return ((TNodeDetail*)m_pDBTNodeDetail->GetCurrentRow())->l_HasHyperlink;

				return FALSE;
			}

			BOOL					IsCurrentFieldSeparator			()
			{
				ASSERT(m_pDBTNodeDetail->GetCurrentRow());

				if (m_pDBTNodeDetail->GetCurrentRow())
					return ((TNodeDetail*)m_pDBTNodeDetail->GetCurrentRow())->l_IsSeparator;

				return FALSE;
			}
						
protected:	//from CAbstractFormDoc
	virtual BOOL					OnAttachData					();
	virtual void					OnParsedControlCreated			(CParsedCtrl* pCtrl);
	virtual void					OnColumnInfoCreated				(ColumnInfo* pColInfo);
	virtual	void					OnFrameCreated					();
protected:	//virtual methods
	virtual void					ShowEmptyDetails				();
	virtual BOOL					PopulateTree					() { return TRUE; }
	virtual void					InitTree						() {}
	virtual void					DoSelectionNodeChanged			(CString nodeKey) {}
	virtual void					DoDragDrop						() {}
	virtual void					DoNodeChanged					(CString nodeKey, BOOL bIsChecked) {}
	virtual void					DoNodeDoubleClick				(CString nodeKey) {}
	virtual void					DoCtxMenuItemClick				(CString nodeKey) {}
	virtual void					DoBuildActions					(CString nodeKey, CTBToolBarMenu& menu){}
	virtual void					DoShowHideDetails				(CString nodeKey);
	virtual void					DoExpandCollapse				();
	virtual void					DoFind							(){}
	virtual BOOL					DoEnableActions					();
	virtual BOOL					DoEnableExpand					();
	virtual BOOL					DoEnableDetails					();
	virtual BOOL					DoEnableFind					();
	virtual void					ManageExtractData				();
	virtual void					ManageUndoExtraction			();
	virtual void					DoDetailGridRowChanged			(){}

private:	//virtual private methods
	virtual BOOL					OnLoadDBT						();	//non esposta più alle classi derivate
	
private: //private methods
			void					BuildActions					(CTBToolBarMenu& menu);
			void					RecalculateHorizontalSplitter	(BOOL bSplit);	//adesso viene gestito solo orizzontale
			void					ManageSplitter					(const BOOL& bShowDetails);

private:
	//{{AFX_MSG(CTBBaseNavigation)
		afx_msg void OnSelectionNodeChanged							();
		afx_msg void OnCtxMenuItemClick								();
		afx_msg void OnDragDrop										();
		afx_msg void OnNodeChanged									();
		afx_msg void OnNodeDoubleClick								();
		afx_msg	void OnDetailGridRowChanged							();
		afx_msg void OnExpandCollapse								();
		afx_msg void OnShowHideDetails								();
		afx_msg void OnFind											();
		afx_msg void OnEnableActions								(CCmdUI*);
		afx_msg void OnEnableExpand									(CCmdUI*);
		afx_msg void OnEnableShowDetails							(CCmdUI*);
		afx_msg void OnEnableFind									(CCmdUI*);
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

#include "endh.dex"
