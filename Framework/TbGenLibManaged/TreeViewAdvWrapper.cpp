/////////////////////////////////////////////////////////////////
//Customized version of TreeViewAdv control
//by Andrey Gliznetsov
/////////////////////////////////////////////////////////////////

#include "stdafx.h"

#include "afxwinforms.h"
#include "atlimage.h"

#include <TbNameSolver\FileSystemFunctions.h>
#include <TbGeneric\Globals.h>
#include <TbGeneric\FontsTable.h>
#include <TbFrameworkImages\CommonImages.h>
#include <TbFrameworkImages\GeneralFunctions.h>
#include <TbGeneric\WndObjDescription.h>
#include <TbGeneric\GeneralFunctions.h>
#include "TreeViewAdvWrapper.h"

#include "UserControlHandlers.h"

using namespace System;
using namespace System::ComponentModel;
using namespace System::Collections;
using namespace System::Windows::Forms;
using namespace System::Data;
using namespace System::Drawing;

using namespace Aga::Controls::Tree;

struct _IMAGELIST {};

//===========================================================================
//									Errors
//===========================================================================
static const TCHAR szTreeViewAdvNotInitialized		[]						= _T("Tree View Adv control not initialized. Cannot call requested method");
//===========================================================================
static const TCHAR szTreeViewAdvDefaultFont			[]						= _T("Verdana");
static const TCHAR szTreeViewNodeTextBoxProperty	[]						= _T("Text");
static const TCHAR szTreeViewNodeCheckBoxProperty	[]						= _T("CheckState");
//===========================================================================
//							UTreeViewAdvEventArgs
//===========================================================================
IMPLEMENT_DYNAMIC (UTreeViewAdvEventArgs, UnmanagedEventsArgs)
//---------------------------------------------------------------------------------------
UTreeViewAdvEventArgs::UTreeViewAdvEventArgs (const CString& sError)
	:
	UnmanagedEventsArgs (sError)
{
}

ref class CommandInfo
{
public:
	int Id;
	UINT AcceleratorKey;
	bool Ctrl;
	bool Shift;
	bool Alt;
};

//===========================================================================
//							TreeViewAdvEventsHandler
//===========================================================================
ref class TreeViewAdvEventsHandler : public ManagedEventsHandlerObj
{	
	CTreeViewAdvWrapper* m_pWrapper;
public:
	//---------------------------------------------------------------------------------------
	TreeViewAdvEventsHandler (CTreeViewAdvWrapper* p) : m_pWrapper(p)
	{
	}
	
	//------------------------------------------------------------------------------------------------------------
	virtual void MapEvents (Object^ pControl) override
	{
		TreeViewAdv^ pTreeControl = (TreeViewAdv^) pControl;

		pTreeControl->SelectionChanged			+=	gcnew EventHandler						(this, &TreeViewAdvEventsHandler::SelectionChanged);
		pTreeControl->ItemDrag					+=	gcnew ItemDragEventHandler				(this, &TreeViewAdvEventsHandler::ItemDrag);
		pTreeControl->DragOver					+=	gcnew DragEventHandler					(this, &TreeViewAdvEventsHandler::DragOver);
		pTreeControl->DragDrop					+=	gcnew DragEventHandler					(this, &TreeViewAdvEventsHandler::DragDrop);
		pTreeControl->MouseUp					+= 	gcnew MouseEventHandler					(this, &TreeViewAdvEventsHandler::MouseUp);
		pTreeControl->MouseDown					+=	gcnew MouseEventHandler					(this, &TreeViewAdvEventsHandler::MouseDown);
		pTreeControl->MouseClick				+=	gcnew MouseEventHandler					(this, &TreeViewAdvEventsHandler::MouseClick);
		pTreeControl->NodeChanged				+=	gcnew EventHandler<TreeModelEventArgs^>	(this, &TreeViewAdvEventsHandler::NodeChanged);
		pTreeControl->ContextMenuItemClick		+=	gcnew EventHandler						(this, &TreeViewAdvEventsHandler::ContextMenuItemClick);
		pTreeControl->MouseDoubleClick			+=	gcnew MouseEventHandler					(this, &TreeViewAdvEventsHandler::MouseDoubleClick);
		pTreeControl->KeyDown					+=  gcnew KeyEventHandler					(this, &TreeViewAdvEventsHandler::OnKeyDown);
		pTreeControl->NodeTextBox->LabelChanged +=  gcnew EventHandler						(this, &TreeViewAdvEventsHandler::LabelChanged);
		pTreeControl->SizeChanged				+=	gcnew EventHandler						(this, &TreeViewAdvEventsHandler::SizeChanged);
	}

	//------------------------------------------------------------------------------------------------------------
	void SelectionChanged(Object^ sender, EventArgs^ e)
	{
		if (m_pWrapper->IsIgnoreSelectionChanged())
			return;

		m_pWrapper->OnSelectionChanged();
		SendAsControl(UM_TREEVIEWADV_SELECTION_CHANGED);
	}
	//------------------------------------------------------------------------------------------------------------
	void SizeChanged(Object^ sender, EventArgs^ e)
	{
		m_pWrapper->OnSizeChanged();
	}

	//------------------------------------------------------------------------------------------------------------
	void ItemDrag(Object^ sender, ItemDragEventArgs^ e)
	{
		SendAsControl(UM_TREEVIEWADV_ITEM_DRAG);
	}

	//------------------------------------------------------------------------------------------------------------
	void DragOver(Object^ sender, DragEventArgs^ e)
	{
		SendAsControl(UM_TREEVIEWADV_DRAG_OVER);
	}

	//------------------------------------------------------------------------------------------------------------
	void DragDrop(Object^ sender, DragEventArgs^ e)
	{
		SendAsControl(UM_TREEVIEWADV_DRAG_DROP);
	}

	//------------------------------------------------------------------------------------------------------------
	void MouseUp(Object^ sender, MouseEventArgs^ e)
	{
		SendAsControl(UM_TREEVIEWADV_MOUSE_UP);
	}

	//------------------------------------------------------------------------------------------------------------
	void MouseDown(Object^ sender, MouseEventArgs^ e)
	{
		SendAsControl(UM_TREEVIEWADV_MOUSE_DOWN);
	}

	//------------------------------------------------------------------------------------------------------------
	void MouseClick(Object^ sender, MouseEventArgs^ e)
	{
		SendAsControl(UM_TREEVIEWADV_MOUSE_CLICK);
	}

	//------------------------------------------------------------------------------------------------------------
	void ContextMenuItemClick(Object^ sender, EventArgs^ e)
	{
		SendAsControl(UM_TREEVIEWADV_CONTEXT_MENU_ITEM_CLICK);
	}

	//-------------------------------------------------------------------------------------------------------------
	void LabelChanged(Object^ sender, EventArgs^ e)
	{
		SendAsControl(UM_TREEVIEWADV_LABEL_CHANGED);
	}

	//-------------------------------------------------------------------------------------------------------------
	void MouseDoubleClick(Object^ sender, MouseEventArgs^ e)
	{
		ReleaseCapture();
		SendAsControl(UM_TREEVIEWADV_MOUSE_DOUBLE_CLICK);
	}
	//-------------------------------------------------------------------------------------------------------------
	void OnToolBarCommand (Object^ sender, ToolBarButtonClickEventArgs^ e)
	{
		m_pWrapper->OnToolBarCommand(((CommandInfo^)e->Button->Tag)->Id);
	}
	//-------------------------------------------------------------------------------------------------------------
	void OnKeyDown(Object^ sender, KeyEventArgs^ e)
	{
		if (m_pWrapper->OnKeyCommand(e->KeyValue, e->Control, e->Alt, e->Shift))
			e->Handled = true;
	}
	//-------------------------------------------------------------------------------------------------------------
	void NodeChanged(Object^ sender, TreeModelEventArgs^ e)
	{
		m_pWrapper->OnStateNodeChanged();
		SendAsControl(UM_TREEVIEWADV_NODE_CHANGED);
	}
};

//===========================================================================
// CTreeNodeAdvArray class (Array of CTreeNodeAdvWrapper)
//===========================================================================
CInternalTreeNodeAdvArray::~CInternalTreeNodeAdvArray()
{
	for (int i = 0; i < GetCount(); i++)
		SAFE_DELETE(GetAt(i));
}

//===========================================================================
// CTreeNodeAdvMap class (Map of CTreeNodeAdvWrapper)
//===========================================================================
CTreeNodeAdvMap::~CTreeNodeAdvMap()
{
	Clear();
}

//-----------------------------------------------------------------------------------------------
void CTreeNodeAdvMap::Clear()
{
	POSITION				pos;
	CString					strKey;
	CTreeNodeAdvWrapperObj*	pElem;

	for (pos = GetStartPosition(); pos != NULL;)
	{
		GetNextAssoc(pos, strKey, (CObject*&)pElem);
		SAFE_DELETE(pElem);
	}
	RemoveAll();
}

//===========================================================================
// CTreeNodeAdvWrapper class (wrapper to node)
//===========================================================================
class CTreeNodeAdvWrapper : public CTreeNodeAdvWrapperObj
{
	gcroot<TreeNodeAdv^> m_pManagedNode;

public:	
	
	//-----------------------------------------------------------------------------------------------
	CTreeNodeAdvWrapper(TreeNodeAdv^ node)
	{
		m_pManagedNode = node;
	}

	//-----------------------------------------------------------------------------------------------
	TreeNodeAdv^ GetNode()
	{
		return m_pManagedNode;
	}

	//-----------------------------------------------------------------------------------------------
	CString GetKey()
	{
		if (!m_pManagedNode)
			return _T("");
		String^ key = ((Node^)(m_pManagedNode->Tag))->Key;
		return CString(key);
	}

	//-----------------------------------------------------------------------------------------------
	CString GetText()
	{
		if (!m_pManagedNode)
			return _T("");
		String^ text = ((Node^)m_pManagedNode->Tag)->Text;
		return CString(text);
	}

	//-----------------------------------------------------------------------------------------------
	void GetChildren(CTreeNodeAdvArray& arAllChildren)
	{
		if (!m_pManagedNode || m_pManagedNode->Children == nullptr)
			return;
		for (int i = 0; i < m_pManagedNode->Children->Count; i++)
		{
			CTreeNodeAdvWrapper* pChildNode = new CTreeNodeAdvWrapper(m_pManagedNode->Children[i]); 	
			arAllChildren.Add(pChildNode);
		}	
	}

	//-----------------------------------------------------------------------------------------------
	BOOL IsExpanded()
	{
		if (!m_pManagedNode)
			return FALSE;
		
		return (m_pManagedNode->IsExpanded == true); 	
	}

	//-----------------------------------------------------------------------------------------------
	BOOL IsSelected()
	{
		if (!m_pManagedNode)
			return FALSE;
		
		return (m_pManagedNode->IsSelected == true); 	
	}

	//-----------------------------------------------------------------------------------------------
	BOOL HasChildren()
	{
		if (!m_pManagedNode)
			return FALSE;
		
		return (m_pManagedNode->Children->Count > 0); 	
	}

	//-----------------------------------------------------------------------------------------------
	HBITMAP GetImageHandle()
	{
		if (!m_pManagedNode)
			return NULL;
		
		try
		{
			System::Drawing::Bitmap^ bitmap = ((Node^)m_pManagedNode->Tag)->Bitmap;
			return bitmap ? (HBITMAP)(HANDLE)bitmap->GetHbitmap() : NULL;
		}
		catch(...)
		{
			return NULL;
		}
		
		return NULL;
	}

	//Metodo che espande o collapsa il nodo (chiama il metodo c# direttamente sul nodo cosi evita una ricerca del nodo che rallenta)
	//-----------------------------------------------------------------------------------------------
	void ToggleNode()
	{
		if (GetNode())
			GetNode()->ToggleNode();
	}

	//Metodo che seleziona il nodo (chiama il metodo c# direttamente sul nodo cosi evita una ricerca del nodo che rallenta)
	//-----------------------------------------------------------------------------------------------
	void SelectNode()
	{
		if (GetNode())
			GetNode()->SelectNode();
	}

	//-----------------------------------------------------------------------------------------------
	BOOL IsChecked()
	{
		if (!m_pManagedNode)
		{
			ASSERT(FALSE);
			return FALSE;
		}

		return m_pManagedNode->IsChecked;
	}

	//-----------------------------------------------------------------------------------------------
	BOOL ToggleCheck(CString* errorMsg)
	{
		if (!m_pManagedNode)
		{
			ASSERT(FALSE);
			return FALSE;
		}

		String^ mngErrorMsg = String::Empty;
		if (!m_pManagedNode->ToggleCheck(mngErrorMsg))
		{
			if (errorMsg)
				*errorMsg = *CString(mngErrorMsg);
			else
				errorMsg = new CString(mngErrorMsg);

			return false;
		}

		return true;
	}

	//-----------------------------------------------------------------------------------------------
	BOOL Check(CString* errorMsg)
	{
		if (!m_pManagedNode)
		{
			ASSERT(FALSE);
			return FALSE;
		}

		String^ mngErrorMsg = String::Empty;
		if (!m_pManagedNode->Check(mngErrorMsg))
		{
			if (errorMsg)
				*errorMsg = *CString(mngErrorMsg);
			else
				errorMsg = new CString(mngErrorMsg);

			return false;
		}

		return true;
	}

	//-----------------------------------------------------------------------------------------------
	BOOL UnCheck(CString* errorMsg)
	{
		if (!m_pManagedNode)
		{
			ASSERT(FALSE);
			return FALSE;
		}

		String^ mngErrorMsg = String::Empty;
		if (!m_pManagedNode->UnCheck(mngErrorMsg))
		{
			if (errorMsg)
				*errorMsg = *CString(mngErrorMsg);
			else
				errorMsg = new CString(mngErrorMsg);

			return false;
		}

		return true;
	}
};

//===========================================================================
// macro in order to identify Control used
//===========================================================================

#define ENSURE_TREEVIEWADV_CONTROL(r) ENSURE_USER_CONTROL(r, TreeViewAdv, treeViewUC, szTreeViewAdvNotInitialized)


#define VOID_ENSURE_TREEVIEWADV_CONTROL() VOID_ENSURE_USER_CONTROL(TreeViewAdv, treeViewUC, szTreeViewAdvNotInitialized)


#define DECLARE_TREEVIEWADV_HANDLER() DECLARE_CTRL_HANDLER(TreeViewAdv, pTreeViewHandler) 

//===========================================================================
// CToolBarWrapper class
//===========================================================================
class CToolBarWrapper
{
public:
	gcroot<ToolBar^> m_Toolbar;
	CToolBarWrapper(TreeViewAdvEventsHandler^ eventHandler, int nBottonSize)
	{
		m_Toolbar = gcnew ToolBar();
		m_Toolbar->ImageList = gcnew ImageList();
		m_Toolbar->ImageList->ColorDepth = ColorDepth::Depth32Bit;
		m_Toolbar->ImageList->ImageSize = System::Drawing::Size(nBottonSize, nBottonSize);
		m_Toolbar->BorderStyle = BorderStyle::FixedSingle;

		m_Toolbar->ButtonClick += gcnew ToolBarButtonClickEventHandler(eventHandler, &TreeViewAdvEventsHandler::OnToolBarCommand);
			
	}
	~CToolBarWrapper()
	{
		if (m_Toolbar)
		{
			delete m_Toolbar->ImageList; 
			delete m_Toolbar; 
		}
	}
};

//===========================================================================
// CResizeExtenderWrapper class
//===========================================================================
class CResizeExtenderWrapper
{
public:
	gcroot<Microarea::TaskBuilderNet::UI::WinControls::ResizeExtender^> m_Extender;
	CResizeExtenderWrapper()
	{
		
	}
	~CResizeExtenderWrapper()
	{
		if (m_Extender)
		{
			delete m_Extender; 
		}
	}
};

//===========================================================================
// MyToolBarButton class
//===========================================================================
ref class MyToolBarButton : public ToolBarButton
{
public:
	bool WasVisible;
};

//===========================================================================
//					CTreeViewAdvWrapper
//===========================================================================
TreeViewAdv^ GetUserControl(CUserControlWrapperObj* pWrapper)
{
	if (pWrapper == NULL || pWrapper->GetHandler() == NULL)
	{
		ASSERT_TRACE (FALSE, _T("Managed User Control not initialized!"));
		return nullptr;
	}
	CUserControlHandler<TreeViewAdv>* p =  (CUserControlHandler<TreeViewAdv>*) pWrapper->GetHandler();
	return p->GetControl();
}

#ifdef NEWTREE
#else
//---------------------------------------------------------------------------------------
CTreeViewAdvWrapper::CTreeViewAdvWrapper()
: 
	m_pToolbar(NULL), 
	m_nButtonSize(20), // default 16x16
	m_nOriginalWidth(0),
	m_nOriginalHeight(0),
	m_bIsVisible(true),
	m_bAnimating(false),
	m_bIgnoreSelectionChanged(false)
{
	m_pManHandler = new CUserControlHandler<TreeViewAdv>(gcnew TreeViewAdvEventsHandler(this));
	m_pExtender = new CResizeExtenderWrapper();
}


//---------------------------------------------------------------------------------------
CTreeViewAdvWrapper::~CTreeViewAdvWrapper()
{
	delete m_pToolbar;
	delete m_pManHandler;
	delete m_pExtender;
}

//---------------------------------------------------------------------------------------
bool CTreeViewAdvWrapper::IsAnimating()
{
	return m_bAnimating || (m_pExtender->m_Extender && m_pExtender->m_Extender->Resizing);
}

//---------------------------------------------------------------------------------------
bool CTreeViewAdvWrapper::OnKeyCommand(UINT nKey, bool ctrl, bool alt, bool shift)
{
	if (!m_pToolbar)
		return false;
	for each(MyToolBarButton^ btn in m_pToolbar->m_Toolbar->Buttons)
	{
		CommandInfo^ info = (CommandInfo^)btn->Tag;
		if (info->AcceleratorKey == nKey && info->Ctrl == ctrl && info->Shift == shift && info->Alt == alt)
		{
			if (btn->Enabled)
				OnToolBarCommand(info->Id);
			return true;
		}
	}
	return false;
}

//---------------------------------------------------------------------------------------
void CTreeViewAdvWrapper::OnToolBarCommand(int cmdId)
{
	switch(cmdId)
	{
		case HIDING_COMMAND_ID:
			Display(false);
			for each(MyToolBarButton^ btn in m_pToolbar->m_Toolbar->Buttons)
			{
				btn->WasVisible = btn->Visible; 
				btn->Visible = ((CommandInfo^)btn->Tag)->Id == SHOWING_COMMAND_ID;
			}
			if (m_pExtender->m_Extender)
				m_pExtender->m_Extender->Visible = false;
			break;

		case SHOWING_COMMAND_ID:
			Display(true);
			for each(MyToolBarButton^ btn in m_pToolbar->m_Toolbar->Buttons)
			{
				btn->Visible = btn->WasVisible && ((CommandInfo^)btn->Tag)->Id != SHOWING_COMMAND_ID;
			}
			if (m_pExtender->m_Extender)
				m_pExtender->m_Extender->Visible = true;
			break;
	}
}

//---------------------------------------------------------------------------------------
CSize CTreeViewAdvWrapper::GetSize()
{
	ENSURE_TREEVIEWADV_CONTROL(CSize(0,0));
	Size sz = treeViewUC->Size;
	
	return CSize(sz.Width, sz.Height);
}
//---------------------------------------------------------------------------------------
void CTreeViewAdvWrapper::Display(bool bShow)
{
	int minWidth = m_nButtonSize + 16;
	int delta = 0;
	VOID_ENSURE_TREEVIEWADV_CONTROL()
	m_bAnimating = true;
	if (bShow)
	{
		if (!m_bIsVisible)
		{
			m_pToolbar->m_Toolbar->AutoSize = true;
			treeViewUC->AutoScroll = true;
			treeViewUC->Width = m_nOriginalWidth;
			treeViewUC->Height = m_nOriginalHeight;

			while (treeViewUC->Width < m_nOriginalWidth || treeViewUC->Height < m_nOriginalHeight)
			{
				++delta;
				treeViewUC->Width = min(treeViewUC->Width + delta, m_nOriginalWidth);
				treeViewUC->Height = min(treeViewUC->Height + delta, m_nOriginalHeight);
				treeViewUC->Update();
			}
			m_bIsVisible = true;
		}
	}
	else
	{
		if (m_bIsVisible)
		{
			treeViewUC->AutoScroll = false;
			m_pToolbar->m_Toolbar->AutoSize = false;
			m_nOriginalWidth = treeViewUC->Width;
			m_nOriginalHeight = treeViewUC->Height;

			treeViewUC->Width = minWidth;
			treeViewUC->Height = minWidth;

			while (treeViewUC->Width > minWidth || treeViewUC->Height > minWidth)
			{
				++delta;
				treeViewUC->Width = max(treeViewUC->Width - delta, minWidth);
				treeViewUC->Height = max(treeViewUC->Height - delta, minWidth);
				treeViewUC->Update();
			}
			m_bIsVisible = false;
		}
	}
	m_bAnimating = false;
}
//---------------------------------------------------------------------------------------
void CTreeViewAdvWrapper::AddHidingCommand ()
{
	AddToolBarCommand(HIDING_COMMAND_ID, TBGlyph(szIconCollapse), _TB("Hide"), 'H', TRUE);
	AddToolBarCommand(SHOWING_COMMAND_ID, TBGlyph(szIconExpand), _TB("Show"), 'S', TRUE);
	ShowToolBarCommand(SHOWING_COMMAND_ID, false);
}

//---------------------------------------------------------------------------------------
void CTreeViewAdvWrapper::AddToolBarCommand(
	int id, 
	const CString& sImage, 
	CString sToolTip /*= L""*/, 
	TCHAR nAccelCharCode /*= 0*/,
	BOOL bCtrlModifier /*= FALSE*/,
	BOOL bShiftModifier /*= FALSE*/,
	BOOL bAltModifier /*= FALSE*/,
	ToolBarButtonStyleWrapper eButtonStyle /*= E_PUSH_BUTTON*/)
{
	bool bToolbarCreated = false;
	if (!m_pToolbar)
	{
		DECLARE_TREEVIEWADV_HANDLER()
		m_pToolbar = new CToolBarWrapper((TreeViewAdvEventsHandler^)pTreeViewHandler->GetEventsHandler(), m_nButtonSize);
		bToolbarCreated = true;
	}
	VOID_ENSURE_TREEVIEWADV_CONTROL()

	CTBNamespace aImageNs(CTBNamespace::IMAGE, sImage);
	
	CString sImagePath = aImageNs.IsValid() ? AfxGetPathFinder()->GetFileNameFromNamespace(aImageNs, AfxGetLoginInfos()->m_strUserName) : sImage;

	if (ExistFile(sImagePath))
		m_pToolbar->m_Toolbar->ImageList->Images->Add(Image::FromFile(gcnew String(sImagePath)));

	MyToolBarButton^ btn = gcnew MyToolBarButton();
	CommandInfo^ info = gcnew CommandInfo();
	info->Id = id;
	info->AcceleratorKey = nAccelCharCode;
	info->Ctrl = bCtrlModifier==TRUE;
	info->Shift = bShiftModifier==TRUE;
	info->Alt = bAltModifier==TRUE;
	info->Id = id;
	btn->Tag = info;
	btn->ImageIndex = m_pToolbar->m_Toolbar->ImageList->Images->Count - 1;
	if (!sToolTip.IsEmpty())
		sToolTip += _T(" ");
	sToolTip += _T("(");
	if (bCtrlModifier)
		sToolTip += _T("CTRL + ");
	if (bAltModifier)
		sToolTip += _T("ALT + ");
	if (bShiftModifier)
		sToolTip += _T("SHIFT + ");
	sToolTip += nAccelCharCode;
	sToolTip += _T(")");
	btn->ToolTipText = gcnew String(sToolTip);

	if (eButtonStyle != E_PUSH_BUTTON)
		btn->Style = (eButtonStyle == E_DROPDOWN_BUTTON) 
					? ToolBarButtonStyle::DropDownButton 
					: (eButtonStyle == E_SEPARATOR_BUTTON) ? ToolBarButtonStyle::Separator 
					: (eButtonStyle == E_TOGGLE_BUTTON) ? ToolBarButtonStyle::ToggleButton : ToolBarButtonStyle::PushButton;

	m_pToolbar->m_Toolbar->Buttons->Add(btn);
	m_pToolbar->m_Toolbar->Parent = treeViewUC;
	treeViewUC->TopMargin = m_pToolbar->m_Toolbar->Height + 5;
}

 //x i toogle button
//---------------------------------------------------------------------------------------
bool CTreeViewAdvWrapper::IsToolBarButtonPushed(int id)
{
	if (!m_pToolbar)
		return false;

	for each(MyToolBarButton^ btn in m_pToolbar->m_Toolbar->Buttons)
	{
		if (((CommandInfo^)btn->Tag)->Id == id)
		 return btn->Pushed;
	}
	return false;
}


//---------------------------------------------------------------------------------------
void CTreeViewAdvWrapper::SetToolBarButtonPushed(int id, bool bPush)
{	
	if (!m_pToolbar)
		return;

	for each(MyToolBarButton^ btn in m_pToolbar->m_Toolbar->Buttons)
	{
		if (((CommandInfo^)btn->Tag)->Id == id)
		{
			btn->Pushed = bPush;
			return;
		}
	}
}

//---------------------------------------------------------------------------------------
void CTreeViewAdvWrapper::EnableToolBarCommand(int id, bool bEnable)
{
	if (!m_pToolbar)
		return;
	for each(MyToolBarButton^ btn in m_pToolbar->m_Toolbar->Buttons)
	{
		if (((CommandInfo^)btn->Tag)->Id == id)
		{
			btn->Enabled = bEnable;
			break;
		}
	}
}

//---------------------------------------------------------------------------------------
void CTreeViewAdvWrapper::ShowToolBarCommand(int id, bool bShow)
{
	if (!m_pToolbar)
		return;
	for each(MyToolBarButton^ btn in m_pToolbar->m_Toolbar->Buttons)
	{
		if (((CommandInfo^)btn->Tag)->Id == id)
		{
			btn->Visible = bShow;
			break;
		}
	}
}


//---------------------------------------------------------------------------------------
void CTreeViewAdvWrapper::OnInitControl ()
{
	InitDefaultValues ();
	SetNodeCheckBoxProperty(szTreeViewNodeCheckBoxProperty);
	SetNodeCheckBoxThreeState(FALSE);
	SetNodeTextBoxProperty(szTreeViewNodeTextBoxProperty);
	SetToolTip();
	m_bUseNodesCache = TRUE;  //Disabilitato solo quando wrappato con EasyBuilder, perche EB lavora direttamente sul C#
	
}

//---------------------------------------------------------------------------------------
CString CTreeViewAdvWrapper::GetLastChangedNodeKey()
{
	ENSURE_TREEVIEWADV_CONTROL(NULL)

	System::String^ managedNodeKey = treeViewUC->LastChangedNodeKey;
	CString unmanagedNodeKey(managedNodeKey);
	return unmanagedNodeKey;
}

//---------------------------------------------------------------------------------------
void CTreeViewAdvWrapper::AddResizeControl()
{
	VOID_ENSURE_TREEVIEWADV_CONTROL()
	m_pExtender->m_Extender = gcnew Microarea::TaskBuilderNet::UI::WinControls::ResizeExtender();
	m_pExtender->m_Extender->ResizableControl = treeViewUC;
	m_pExtender->m_Extender->ResizeBorder = Microarea::TaskBuilderNet::UI::WinControls::ResizeExtender::ResizeBorderKind::Right;
}
//---------------------------------------------------------------------------------------
CTreeNodeAdvWrapperObj* CTreeViewAdvWrapper::GetNode(const CString& sNodeKey)
{
	ENSURE_TREEVIEWADV_CONTROL(NULL)

	CObject* pObject;
	if (m_bUseNodesCache && wrappedNodes.Lookup(sNodeKey, pObject) && pObject != NULL)
		return (CTreeNodeAdvWrapper*) pObject;

	TreeNodeAdv^ node = treeViewUC->GetNode(gcnew String(sNodeKey));
	if (node == nullptr)
		return NULL;

	CTreeNodeAdvWrapper* pWrappedNode = new CTreeNodeAdvWrapper (node);
	wrappedNodes[sNodeKey] = pWrappedNode;
	return pWrappedNode;
}

//---------------------------------------------------------------------------------------
HMENU CTreeViewAdvWrapper::GetContextMenuHandle()
{
	ENSURE_TREEVIEWADV_CONTROL(NULL)

	if (treeViewUC->IsContextMenuVisible)
		return (HMENU)(int)(treeViewUC->ContextMenu->Handle);
	else
		return 0;
}
//---------------------------------------------------------------------------------------
void CTreeViewAdvWrapper::SetEditable(const BOOL& bValue /*= TRUE*/)
{
	VOID_ENSURE_TREEVIEWADV_CONTROL()

	treeViewUC->IsEditable = (bValue == TRUE);
}

//---------------------------------------------------------------------------------------
BOOL CTreeViewAdvWrapper::IsEditable()
{
	ENSURE_TREEVIEWADV_CONTROL(FALSE)

	return treeViewUC->IsEditable;
}


//---------------------------------------------------------------------------------------
void CTreeViewAdvWrapper::SetAllowDrop(const BOOL& bValue /*= FALSE*/)
{
	VOID_ENSURE_TREEVIEWADV_CONTROL()

	treeViewUC->AllowDrop = (bValue == TRUE);
}

//---------------------------------------------------------------------------------------
void CTreeViewAdvWrapper::ExpandAllFromSelectedNode()
{
	VOID_ENSURE_TREEVIEWADV_CONTROL()

	treeViewUC->ExpandAll(FALSE);
}

//---------------------------------------------------------------------------------------
void CTreeViewAdvWrapper::ExpandLevels(int nLevel)
{
	VOID_ENSURE_TREEVIEWADV_CONTROL()

	treeViewUC->ExpandLevels(nLevel);
}

//---------------------------------------------------------------------------------------
void CTreeViewAdvWrapper::ExpandAll()
{
	VOID_ENSURE_TREEVIEWADV_CONTROL()

	treeViewUC->ExpandAll(TRUE);
}

//---------------------------------------------------------------------------------------
void CTreeViewAdvWrapper::CollapseAllFromSelectedNode()
{
	VOID_ENSURE_TREEVIEWADV_CONTROL()

	treeViewUC->CollapseAll(FALSE);
}

//---------------------------------------------------------------------------------------
void CTreeViewAdvWrapper::CollapseAll()
{
	VOID_ENSURE_TREEVIEWADV_CONTROL()

	treeViewUC->CollapseAll(TRUE);
}

//---------------------------------------------------------------------------------------
BOOL CTreeViewAdvWrapper::IsExpandedSelectedNode()
{
	ENSURE_TREEVIEWADV_CONTROL(FALSE)

	return (treeViewUC->SelectedNode != nullptr && treeViewUC->SelectedNode->IsExpanded);
}

//---------------------------------------------------------------------------------------
BOOL CTreeViewAdvWrapper::HasChildrenSelectedNode()
{
	ENSURE_TREEVIEWADV_CONTROL(FALSE)

	return (treeViewUC->SelectedNode != nullptr && treeViewUC->SelectedNode->Children->Count > 0);
}

//---------------------------------------------------------------------------------------
void CTreeViewAdvWrapper::DeleteAllChildrenFromSelectedNode	()
{
	VOID_ENSURE_TREEVIEWADV_CONTROL()

	if (treeViewUC->SelectedNode == nullptr || treeViewUC->SelectedNode->Children->Count == 0)
		return;

	for (int i = 0; i < treeViewUC->SelectedNode->Children->Count; i++)
		if (treeViewUC->SelectedNode->Children->Count > 0)
		{
			((Aga::Controls::Tree::Node^)treeViewUC->SelectedNode->Children[i]->Tag)->Parent = nullptr;
			i = -1;
		}
}

//---------------------------------------------------------------------------------------
void CTreeViewAdvWrapper::EnsureVisible(const CString& sNodeKey)
{
	VOID_ENSURE_TREEVIEWADV_CONTROL()

	TreeNodeAdv^ node = treeViewUC->GetNode(gcnew String(sNodeKey));
	if (node == nullptr)
		return;

	treeViewUC->EnsureVisible(node);
}

//---------------------------------------------------------------------------------------
void CTreeViewAdvWrapper::SetDragAndDropOnSameLevel(const BOOL& bValue /*= FALSE*/)
{
	VOID_ENSURE_TREEVIEWADV_CONTROL()

	treeViewUC->DragAndDropOnSameLevel = (bValue == TRUE);
}

//---------------------------------------------------------------------------------------
void CTreeViewAdvWrapper::SetChecksBoxEditable(const BOOL& bValue /*= TRUE*/)
{
	VOID_ENSURE_TREEVIEWADV_CONTROL()

	treeViewUC->SetChecksBoxEditable(bValue == TRUE);
}

//---------------------------------------------------------------------------------------
void CTreeViewAdvWrapper::SetNodeCheckBoxProperty(const CString& propertyName)
{
	VOID_ENSURE_TREEVIEWADV_CONTROL()

	String^ sPropertyName = gcnew String(propertyName);
	treeViewUC->SetNodeCheckBoxProperty(sPropertyName);
}

//---------------------------------------------------------------------------------------
void CTreeViewAdvWrapper::SetViewContextMenu(const BOOL& bValue /*= FALSE*/)
{
	//TODO rimuovere
}

//---------------------------------------------------------------------------------------
void CTreeViewAdvWrapper::RemoveContextMenuItem(int idxMenuItem)
{
	VOID_ENSURE_TREEVIEWADV_CONTROL()

	treeViewUC->RemoveContextMenuItem(idxMenuItem);
}

//---------------------------------------------------------------------------------------
void CTreeViewAdvWrapper::SetNodeCheckBoxThreeState(const BOOL& threeState /*= FALSE*/)
{
	VOID_ENSURE_TREEVIEWADV_CONTROL()

	treeViewUC->SetNodeCheckBoxThreeState(threeState == TRUE);
}

//---------------------------------------------------------------------------------------
void CTreeViewAdvWrapper::SetAllowDragOver(const BOOL& bValue /*= FALSE*/)
{
	VOID_ENSURE_TREEVIEWADV_CONTROL()

	treeViewUC->AllowDragOver = (bValue == TRUE);
}

//Call this method only in OnDragDrop (usefull for cancel Drop operation)
//---------------------------------------------------------------------------------------
void CTreeViewAdvWrapper::SetCancelDragDrop()
{
	VOID_ENSURE_TREEVIEWADV_CONTROL()

	treeViewUC->CancelDragDrop = TRUE;
}

//Call this method only in OnDragOver (usefull for cancel Drag over operation)
void CTreeViewAdvWrapper::SetCancelDragOver()
{
	VOID_ENSURE_TREEVIEWADV_CONTROL()

	treeViewUC->CancelDragOver = TRUE;
}

//Call this method only in OnDragDrop (usefull for get old parent node on Drop operation)
//---------------------------------------------------------------------------------------
CString CTreeViewAdvWrapper::GetNewParentKey()
{
	ENSURE_TREEVIEWADV_CONTROL(_T(""))

	return CString(treeViewUC->NewParentKey);
}

//---------------------------------------------------------------------------------------
void CTreeViewAdvWrapper::SetNodeTextBoxProperty(const CString& propertyName)
{
	VOID_ENSURE_TREEVIEWADV_CONTROL()

	String^ sPropertyName = gcnew String(propertyName);
	treeViewUC->SetNodeTextBoxProperty(sPropertyName);
}

//---------------------------------------------------------------------------------------
void CTreeViewAdvWrapper::SetDblClickWithExpandCollapse(const BOOL& bValue /*= FALSE*/)
{
	VOID_ENSURE_TREEVIEWADV_CONTROL()

	treeViewUC->DblClickWithExpandCollapse = (bValue == TRUE);
}

//---------------------------------------------------------------------------------------
void CTreeViewAdvWrapper::SetToolTip()
{
	VOID_ENSURE_TREEVIEWADV_CONTROL()

	treeViewUC->ShowNodeToolTips = TRUE;
	treeViewUC->SetToolTip();
}

//----------------------------------------------------------------------------------------------
void CTreeViewAdvWrapper::SetBalloonToolTip(const BOOL& bIsBalloon /*FALSE*/)
{
	VOID_ENSURE_TREEVIEWADV_CONTROL()

	treeViewUC->SetBalloonToolTip(bIsBalloon == TRUE);
}
	
//----------------------------------------------------------------------------------------------
void CTreeViewAdvWrapper::SetCustomToolTip(const COLORREF bkColor, const COLORREF foreColor)
{
	VOID_ENSURE_TREEVIEWADV_CONTROL()
	DECLARE_TREEVIEWADV_HANDLER()

	treeViewUC->SetCustomToolTip
					(
						pTreeViewHandler->ConvertColor(bkColor), 
						pTreeViewHandler->ConvertColor(foreColor)
					);
}

//----------------------------------------------------------------------------------------------
void CTreeViewAdvWrapper::SetMenuItemCheck(const CString& itemMenu, BOOL bCheck)
{
	VOID_ENSURE_TREEVIEWADV_CONTROL()
	String^ sMenuItem = gcnew String(itemMenu);
	treeViewUC->SetMenuItemCheck(sMenuItem, bCheck == TRUE);
}

//----------------------------------------------------------------------------------------------
void CTreeViewAdvWrapper::SetMenuItemEnable(const CString& itemMenu, BOOL bEnable)
{
	VOID_ENSURE_TREEVIEWADV_CONTROL()
	String^ sMenuItem = gcnew String(itemMenu);
	treeViewUC->SetMenuItemEnable(sMenuItem, bEnable == TRUE);
}
	
//---------------------------------------------------------------------------------------
void CTreeViewAdvWrapper::AddContextMenuItem(const CString& menuItem)
{
	VOID_ENSURE_TREEVIEWADV_CONTROL()

	String^ sMenuItem = gcnew String(menuItem);
	treeViewUC->AddContextMenuItem(sMenuItem, FALSE);
}

//---------------------------------------------------------------------------------------
void CTreeViewAdvWrapper::AddContextSubMenuItem(const CString& menuItem, CArray<CString>& subItems)
{
	VOID_ENSURE_TREEVIEWADV_CONTROL()

	String^ sMenuItem = gcnew String(menuItem);
	System::Collections::Generic::List<String^> items = gcnew System::Collections::Generic::List<String^>();

	for(int i = 0; i < subItems.GetSize(); i++)
		items.Add(gcnew String(subItems[i]));

	treeViewUC->AddContextSubMenuItem(sMenuItem, items.ToArray(), FALSE);
}

//------------------------------------------------------------------------------------------
BOOL CTreeViewAdvWrapper::GetContextMenuItemClickedResponse()
{
	ENSURE_TREEVIEWADV_CONTROL(FALSE)

	return treeViewUC->ContextMenuItemClickResponse;
}

//-------------------------------------------------------------------------------------------
void CTreeViewAdvWrapper::SetCaptionBoxConfirm(const CString& caption)
{
	VOID_ENSURE_TREEVIEWADV_CONTROL()

	String^ sCaption = gcnew String(caption);
	treeViewUC->CaptionBoxConfirm = sCaption;
}

//---------------------------------------------------------------------------------------------
void CTreeViewAdvWrapper::SetTextBoxConfirm (const CString& text)
{
	VOID_ENSURE_TREEVIEWADV_CONTROL()

	String^ sText = gcnew String(text);
	treeViewUC->TextBoxConfirm = sText;
}

//----------------------------------------------------------------------------------------
void CTreeViewAdvWrapper::AddContextMenuItemWithConfirm(const CString& menuItem)
{
	VOID_ENSURE_TREEVIEWADV_CONTROL()

	String^ sMenuItem = gcnew String(menuItem);
	treeViewUC->AddContextMenuItem(sMenuItem, TRUE);
}

//----------------------------------------------------------------------------------------
void CTreeViewAdvWrapper::AddContextMenuItemDisabled(const CString& menuItem)
{
	VOID_ENSURE_TREEVIEWADV_CONTROL()

	String^ sMenuItem = gcnew String(menuItem);
	treeViewUC->AddContextMenuItemDisabled(sMenuItem);
}

//---------------------------------------------------------------------------------------
void CTreeViewAdvWrapper::AddContextMenuSeparator()
{
	VOID_ENSURE_TREEVIEWADV_CONTROL()

	treeViewUC->AddSeparatorMenuItem();
}

//---------------------------------------------------------------------------------------
int CTreeViewAdvWrapper::GetIdxContextMenuItemClicked()
{
	ENSURE_TREEVIEWADV_CONTROL(-1)

	return treeViewUC->IdxContextMenuItemClicked;
}

//---------------------------------------------------------------------------------------
void CTreeViewAdvWrapper::GetTextContextMenuItemClicked(CString& itemKey)
{
	VOID_ENSURE_TREEVIEWADV_CONTROL()

	itemKey = treeViewUC->TextContextMenuItemClicked;
}

//-------------------------------------------------------------------------------------------------
void CTreeViewAdvWrapper::SetStyleForSelectedNode(const BOOL& bBold /*FALSE*/, const BOOL& bItalic /*FALSE*/, const BOOL& bStrikeOut /*FALSE*/, const BOOL& bUnderline /*FALSE*/)
{
	VOID_ENSURE_TREEVIEWADV_CONTROL()

	treeViewUC->SetStyleForSelectedNode(bBold == TRUE, bItalic == TRUE, bStrikeOut == TRUE, bUnderline == TRUE);
}

//----------------------------------------------------------------------------------------------------
void CTreeViewAdvWrapper::SetStyleForNode(const CString& nodeKey, const BOOL& bBold /*FALSE*/, const BOOL& bItalic /*FALSE*/, const BOOL& bStrikeOut /*FALSE*/, const BOOL& bUnderline /*FALSE*/, const COLORREF foreColor)
{
	VOID_ENSURE_TREEVIEWADV_CONTROL()
	DECLARE_TREEVIEWADV_HANDLER()

	String^ sNodeKey = gcnew String(nodeKey);
	treeViewUC->SetStyleForNode(sNodeKey, bBold == TRUE, bItalic == TRUE, bStrikeOut == TRUE, bUnderline == TRUE, pTreeViewHandler->ConvertColor(foreColor));
}

//------------------------------------------------------------------------------------------------------------
void CTreeViewAdvWrapper::SetForeColorForNode(const CString& nodeKey, const COLORREF foreColor)	
{
	VOID_ENSURE_TREEVIEWADV_CONTROL()
	DECLARE_TREEVIEWADV_HANDLER()

	String^ sNodeKey = gcnew String(nodeKey);
	treeViewUC->SetForeColorForNode(sNodeKey, pTreeViewHandler->ConvertColor(foreColor));
}

//---------------------------------------------------------------------------------------
void CTreeViewAdvWrapper::SetFont
							(
								const CString& sFontName, 
								const int& fontSize, 
								const E_FONTSTYLE eFontStyle 
							)
{

	VOID_ENSURE_TREEVIEWADV_CONTROL()

	System::Drawing::FontStyle fontStyle;
	switch (eFontStyle)
	{
		case F_BOLD:
			fontStyle = System::Drawing::FontStyle::Bold;
			break;
		case F_ITALIC:
			fontStyle = System::Drawing::FontStyle::Italic;
			break;
		case F_REGULAR:
			fontStyle = System::Drawing::FontStyle::Regular;
			break;
		case F_STRIKEOUT:
			fontStyle = System::Drawing::FontStyle::Strikeout;
			break;
		case F_UNDERLINE:
			fontStyle = System::Drawing::FontStyle::Underline;
			break;
	}

	String^ fontName = gcnew String(sFontName);
	treeViewUC->Font = gcnew System::Drawing::Font(fontName, (float) fontSize, fontStyle);

}

//---------------------------------------------------------------------------------------------
void CTreeViewAdvWrapper::SetBackColorTreeView (const COLORREF color)
{
	VOID_ENSURE_TREEVIEWADV_CONTROL()
	DECLARE_TREEVIEWADV_HANDLER()

	if (treeViewUC == nullptr)
		return;

	treeViewUC->BackColor = pTreeViewHandler->ConvertColor(color);
}

//-----------------------------------------------------------------------------------------
void CTreeViewAdvWrapper::GetAllParentsTextNodeFromSelected(CArray<CString>& m_AllParents)
{
	VOID_ENSURE_TREEVIEWADV_CONTROL();

	if (treeViewUC->SelectedNode == nullptr)
		return;
	Aga::Controls::Tree::TreeNodeAdv^ node = treeViewUC->SelectedNode;
	while (node->Parent->Tag != nullptr)
	{
		m_AllParents.Add(((Aga::Controls::Tree::Node^)node->Parent->Tag)->Key);
		node = node->Parent;
	}
}

//--------------------------------------------------------------------------------------------
void CTreeViewAdvWrapper::GetAllNodesFromSelection
							(
								CArray<CString>& m_AllNodesLeaf, 
								CArray<CString>& m_AllNodesNotLeaf
							)
{
	VOID_ENSURE_TREEVIEWADV_CONTROL()

	for (int i = 0; i < treeViewUC->SelectedNodes->Count; i++)
	{
		if (treeViewUC->SelectedNodes[i]->Children->Count > 0)
			m_AllNodesNotLeaf.Add(((Aga::Controls::Tree::Node^)treeViewUC->SelectedNodes[i]->Tag)->Key);
		else
			m_AllNodesLeaf.Add(((Aga::Controls::Tree::Node^)treeViewUC->SelectedNodes[i]->Tag)->Key);
	}
}

//-----------------------------------------------------------------------------------------------
void CTreeViewAdvWrapper::GetAllChildrenFromSelectedNode(CArray<CString>& m_AllChildren)
{
	VOID_ENSURE_TREEVIEWADV_CONTROL();

	if (treeViewUC->SelectedNode == nullptr)
		return;

	for (int i = 0; i < treeViewUC->SelectedNode->Children->Count; i++)
	{
		m_AllChildren.Add(((Aga::Controls::Tree::Node^)treeViewUC->SelectedNode->Children[i]->Tag)->Key);
	}
}

//-----------------------------------------------------------------------------------------------------------
void CTreeViewAdvWrapper::GetAllChildrenFromNodeKey(const CString& nodeKey, CArray<CString>& allChildren)
{
	VOID_ENSURE_TREEVIEWADV_CONTROL();

	TreeNodeAdv^ node = treeViewUC->GetNode(gcnew String(nodeKey));
	if (node == nullptr)
		return;

	for (int i = 0; i < node->Children->Count; i++)
		allChildren.Add(((Aga::Controls::Tree::Node^)node->Children[i]->Tag)->Key);

}

//-----------------------------------------------------------------------------------------------
void CTreeViewAdvWrapper::SetUpdateTextNode(const int& nrNode, const CString& text)
{
	VOID_ENSURE_TREEVIEWADV_CONTROL();

	if (treeViewUC->SelectedNode == nullptr)
		return;

	if (treeViewUC->SelectedNode->Children[nrNode] == nullptr)
		return;

	String^ sText = gcnew String(text);
	((Aga::Controls::Tree::Node^)treeViewUC->SelectedNode->Children[nrNode]->Tag)->Text = sText;
}

//---------------------------------------------------------------------------------------------------
void CTreeViewAdvWrapper::SetUpdateTextNode(const CString& nodeKey, const CString& text)
{
	VOID_ENSURE_TREEVIEWADV_CONTROL();

	String^ sNodeKey = gcnew String(nodeKey);
	String^ sText = gcnew String(text);

	treeViewUC->SetUpdateTextNode(sNodeKey, sText);
}

//------------------------------------------------------------------------------------------
CString CTreeViewAdvWrapper::GetTextNode(const int& nrNode)
{
	ENSURE_TREEVIEWADV_CONTROL(_T(""));

	if (treeViewUC->SelectedNode == nullptr)
		return _T("");

	if (treeViewUC->SelectedNode->Children == nullptr ||
		nrNode >= treeViewUC->SelectedNode->Children->Count || 
		treeViewUC->SelectedNode->Children[nrNode] == nullptr)
			return _T("");

	return CString(((Aga::Controls::Tree::Node^)treeViewUC->SelectedNode->Children[nrNode]->Tag)->Text);
}

//------------------------------------------------------------------------------------------
CRect CTreeViewAdvWrapper::GetNodeBounds(const CString& sNodeKey, int x, int y)
{
	String^ nodeKey	 = gcnew String(sNodeKey);
	return GetNodeBounds(GetNode(sNodeKey), x, y);
}

//------------------------------------------------------------------------------------------
CRect CTreeViewAdvWrapper::GetNodeBounds(CTreeNodeAdvWrapperObj* pNode, int x, int y)
{
	ENSURE_TREEVIEWADV_CONTROL(CRect(0, 0, 0, 0));

	CTreeNodeAdvWrapper* pInternalNode = (CTreeNodeAdvWrapper*)pNode;
	System::Drawing::Rectangle r = treeViewUC->GetNodeBounds(pInternalNode->GetNode());
	
	//sommo la posizione del tree (x,y) alle coordinate del nodo
	//perche il calcolo della poszione del nodo nel tree  differisce dal tree standard MFC
	CRect rectNode = CRect(r.Left, r.Top, r.Right, r.Bottom);
	rectNode.OffsetRect(x, y);
	return rectNode;
}

//------------------------------------------------------------------------------------------
void CTreeViewAdvWrapper::SetSelectionMode(const E_SELECTIONMODE eSelectionMode)
{
	VOID_ENSURE_TREEVIEWADV_CONTROL();

	switch (eSelectionMode)
	{
		case F_SINGLE:
			treeViewUC->SelectionMode = Aga::Controls::Tree::TreeSelectionMode::Single;
			break;
		case F_MULTI:
			treeViewUC->SelectionMode =  Aga::Controls::Tree::TreeSelectionMode::Multi;
			break;
		case F_MULTISAMEPARENT:
			treeViewUC->SelectionMode =  Aga::Controls::Tree::TreeSelectionMode::MultiSameParent;
			break;
	}
}

//---------------------------------------------------------------------------------------------
void CTreeViewAdvWrapper::SetSelectionOnlyOnLevel(const int& nLevelOnly)
{
	VOID_ENSURE_TREEVIEWADV_CONTROL()

	treeViewUC->SelectOnlyOnLevel = nLevelOnly;
}

//-----------------------------------------------------------------------------------------
void CTreeViewAdvWrapper::SetCheckBoxControls(const BOOL& bValue /*= FALSE*/)
{
	VOID_ENSURE_TREEVIEWADV_CONTROL()

	treeViewUC->SetCheckBoxControls(bValue == TRUE);
}

//-----------------------------------------------------------------------------------------
void CTreeViewAdvWrapper::SetNodeStateIcon(const BOOL& bValue)
{
	VOID_ENSURE_TREEVIEWADV_CONTROL()

	treeViewUC->SetNodeStateIcon(bValue == TRUE);
}

//-----------------------------------------------------------------------------------------
void CTreeViewAdvWrapper::AddControls()
{
	VOID_ENSURE_TREEVIEWADV_CONTROL()

	treeViewUC->AddControls();
}

//----------------------------------------------------------------------------------------
void CTreeViewAdvWrapper::SetTreeViewDefaultFont()
{
	VOID_ENSURE_TREEVIEWADV_CONTROL()

	SetFont(szTreeViewAdvDefaultFont, 9, F_REGULAR);
}

//----------------------------------------------------------------------------------------
void CTreeViewAdvWrapper::SetRowHeight(int nHeight)
{
	VOID_ENSURE_TREEVIEWADV_CONTROL()

	treeViewUC->RowHeight = ScalePix(nHeight);
}

//---------------------------------------------------------------------------------------
void CTreeViewAdvWrapper::SetSelectedNodeBackColor(const COLORREF color)
{
	VOID_ENSURE_TREEVIEWADV_CONTROL()
	DECLARE_TREEVIEWADV_HANDLER()
	treeViewUC->CustomColors->HightLightBkgColor->TheColor = pTreeViewHandler->ConvertColor(color);
}

//---------------------------------------------------------------------------------------
void CTreeViewAdvWrapper::SetSelectedNodeForeColor(const COLORREF color)
{
	VOID_ENSURE_TREEVIEWADV_CONTROL()
	DECLARE_TREEVIEWADV_HANDLER()
	treeViewUC->CustomColors->HightLightForeColor->TheColor = pTreeViewHandler->ConvertColor(color);
}
//---------------------------------------------------------------------------------------
void CTreeViewAdvWrapper::SetSelectedNodeInactiveBkgColor(const COLORREF color)
{
	VOID_ENSURE_TREEVIEWADV_CONTROL()
		DECLARE_TREEVIEWADV_HANDLER()
	treeViewUC->CustomColors->InactiveBorderBkgColor->TheColor = pTreeViewHandler->ConvertColor(color);
}

//---------------------------------------------------------------------------------------
void CTreeViewAdvWrapper::SetScrollBarColor(const COLORREF color)
{
	VOID_ENSURE_TREEVIEWADV_CONTROL()
		DECLARE_TREEVIEWADV_HANDLER()
	treeViewUC->CustomColors->ScrollbarColor->TheColor = pTreeViewHandler->ConvertColor(color);
}


//---------------------------------------------------------------------------------------
void CTreeViewAdvWrapper::InitDefaultValues ()
{
	SetTreeViewDefaultFont();
	SetRowHeight(23);
	SetSelectedNodeBackColor();
	SetSelectedNodeForeColor();
	SetSelectedNodeInactiveBkgColor();
	SetScrollBarColor();
}

//---------------------------------------------------------------------------------------
void CTreeViewAdvWrapper::Enable (const BOOL bValue /*TRUE*/)
{
	VOID_ENSURE_TREEVIEWADV_CONTROL()

	treeViewUC->Enabled = (bValue == TRUE);
}

//---------------------------------------------------------------------------------------------
void CTreeViewAdvWrapper::Focus()
{
	VOID_ENSURE_TREEVIEWADV_CONTROL()

	treeViewUC->Focus();
}

//---------------------------------------------------------------------------------------
void CTreeViewAdvWrapper::AddImage(const CString& sImageKey, const CString& sImagePath)
{
	VOID_ENSURE_TREEVIEWADV_CONTROL()

	String^ nodeKeyImage	= gcnew String(sImageKey);
	String^ nodeKeyPath		= gcnew String(sImagePath);

	treeViewUC->AddImage(nodeKeyImage, nodeKeyPath);
}

//----------------------------------------------------------------------------------------
void CTreeViewAdvWrapper::SetImage(const CString& sNodeKey,  const CString& sImageKey)
{
	VOID_ENSURE_TREEVIEWADV_CONTROL()

	String^ nodeKey	 = gcnew String(sNodeKey);
	String^ imageKey = gcnew String(sImageKey);

	treeViewUC->SetImage(nodeKey, imageKey);
}

// cambia l'immagine del nodo, ma al contrario della SetImage non seleziona il nodo
//----------------------------------------------------------------------------------------
void CTreeViewAdvWrapper::SetImageNoSel(const CString& sNodeKey,  const CString& sImageKey)
{
	VOID_ENSURE_TREEVIEWADV_CONTROL()

	String^ nodeKey	 = gcnew String(sNodeKey);
	String^ imageKey = gcnew String(sImageKey);

	treeViewUC->SetImageNoSel(nodeKey, imageKey);
}

//-------------------------------------------------------------------------------------------
void CTreeViewAdvWrapper::SetSelectedNodeImage(const CString& sNodeKey)
{
	VOID_ENSURE_TREEVIEWADV_CONTROL()

	String^ nodeKey	 = gcnew String(sNodeKey);
	treeViewUC->SetSelectedNodeImage(nodeKey);
}

//----------------------------------------------------------------------------------------
BOOL CTreeViewAdvWrapper::AddNode
								(
									const CString& sNodeText, 
									const CString& sNodeKey, 
									const CString& sNodeKeyImage /*= _T("")*/, 
									const COLORREF color
								)
{
	ENSURE_TREEVIEWADV_CONTROL(FALSE)
	DECLARE_TREEVIEWADV_HANDLER()

	String^ nodeText		= gcnew String(sNodeText);
	String^ nodeKey			= gcnew String(sNodeKey);
	String^ nodeKeyImage	= gcnew String(sNodeKeyImage);
	String^ toolTipText		= gcnew String(_T(""));
	return treeViewUC->AddTreeNode
							(
								nodeText, 
								nodeKey, 
								nodeKeyImage, 
								pTreeViewHandler->ConvertColor(color), 
								toolTipText
							);
}

//----------------------------------------------------------------------------------------
BOOL CTreeViewAdvWrapper::AddNode
								(
									const CString& sNodeText, 
									const CString& sNodeKey,
									const CStringArray& sToolTipText,
									const CString& sNodeKeyImage /*= _T("")*/, 
									const COLORREF color
									)
{
	ENSURE_TREEVIEWADV_CONTROL(FALSE)
	DECLARE_TREEVIEWADV_HANDLER()

	String^ nodeText		= gcnew String(sNodeText);
	String^ nodeKey			= gcnew String(sNodeKey);
	String^ nodeKeyImage	= gcnew String(sNodeKeyImage);

	CString toolTip = _T("");
	for (int i = 0; i < sToolTipText.GetSize(); i++)
	{
		if (i > 0)
			toolTip += _T("\n");
		toolTip += sToolTipText.GetAt(i);
	}

	String^ toolTipText		= gcnew String(toolTip);
	EnableToolTip();
	return treeViewUC->AddTreeNode
							(
								nodeText, 
								nodeKey, 
								nodeKeyImage, 
								pTreeViewHandler->ConvertColor(color), 
								toolTipText
							);
}

//----------------------------------------------------------------------------------------
BOOL CTreeViewAdvWrapper::AddNode
								(
									const CString& sNodeText, 
									const CString& sNodeKey,
									const BOOL& checkBoxNode
									)
{
	ENSURE_TREEVIEWADV_CONTROL(FALSE)
	DECLARE_TREEVIEWADV_HANDLER()

	String^ nodeText		= gcnew String(sNodeText);
	String^ nodeKey			= gcnew String(sNodeKey);
	String^ toolTipText		= gcnew String(_T(""));
	
	return treeViewUC->AddTreeNode
							(
								nodeText, 
								nodeKey, 
								toolTipText,
								checkBoxNode == TRUE
							);
}

//-----------------------------------------------------------------------------------------
BOOL CTreeViewAdvWrapper::AddChild
								(
									const CString &sNodeText, 
									const CString& sNodeKey, 
									const CString& sNodeKeyImage /*= _T("")*/, 
									const COLORREF color
								)
{
	ENSURE_TREEVIEWADV_CONTROL(FALSE)
	DECLARE_TREEVIEWADV_HANDLER();

	String^ nodeText		= gcnew String(sNodeText);
	String^ nodeKey			= gcnew String(sNodeKey);
	String^ nodeKeyImage	= gcnew String(sNodeKeyImage);
	String^ toolTipText		= gcnew String(_T(""));

	return treeViewUC->AddChild
							(
								nodeText, 
								nodeKey, 
								nodeKeyImage, 
								pTreeViewHandler->ConvertColor(color), 
								toolTipText
							);
}

//-----------------------------------------------------------------------------------------
BOOL CTreeViewAdvWrapper::AddChild
								(
									const CString &sNodeText, 
									const CString& sNodeKey, 
									const CStringArray& sToolTipText,
									const CString& sNodeKeyImage /*= _T("")*/, 
									const COLORREF color 
								)
{
	ENSURE_TREEVIEWADV_CONTROL(FALSE)
	DECLARE_TREEVIEWADV_HANDLER();

	String^ nodeText		= gcnew String(sNodeText);
	String^ nodeKey			= gcnew String(sNodeKey);
	String^ nodeKeyImage	= gcnew String(sNodeKeyImage);

	CString toolTip = _T("");
	for (int i = 0; i < sToolTipText.GetSize(); i++)
	{
		if (i > 0)
			toolTip += _T("\n");
		toolTip += sToolTipText.GetAt(i);
	}

	String^ toolTipText		= gcnew String(toolTip);
	EnableToolTip();
	return treeViewUC->AddChild(nodeText, nodeKey, nodeKeyImage, pTreeViewHandler->ConvertColor(color), toolTipText);
}

//-----------------------------------------------------------------------------------------------------------------------------
BOOL CTreeViewAdvWrapper::AddFastNodeToSelected(const CString& sNodeText, const CString& sNodeKey, const CString& sToolTipText /*= _T("")*/, const CString& sNodeKeyImage /*= _T("")*/)
{
	ENSURE_TREEVIEWADV_CONTROL(FALSE)
	DECLARE_TREEVIEWADV_HANDLER();

	String^ nodeText		= gcnew String(sNodeText);
	String^ nodeKey			= gcnew String(sNodeKey);
	String^ nodeKeyImage	= gcnew String(sNodeKeyImage);
	String^ nodeToolTip		= gcnew String(sToolTipText);

	Aga::Controls::Tree::Node^ node = gcnew Aga::Controls::Tree::Node(nodeText, nodeKey, nodeKeyImage, nodeToolTip);

	if (node == nullptr)
		return FALSE;

	if (treeViewUC->SelectedNode != nullptr)
	{
		Aga::Controls::Tree::Node^ parent = (Aga::Controls::Tree::Node^)treeViewUC->SelectedNode->Tag;
		parent->Nodes->Add(node);
	}
	else
		treeViewUC->Model->Nodes->Add(node);

	return TRUE;
}

//------------------------------------------------------------------------------------------------------------------------------------------
BOOL CTreeViewAdvWrapper::AddFastRoot(const CString& sNodeText, const CString& sNodeKey, const CString& sToolTipText /*= _T("")*/, const CString& sNodeKeyImage /*= _T("")*/)
{
	ENSURE_TREEVIEWADV_CONTROL(FALSE)
	DECLARE_TREEVIEWADV_HANDLER();

	String^ nodeText		= gcnew String(sNodeText);
	String^ nodeKey			= gcnew String(sNodeKey);
	String^ nodeKeyImage	= gcnew String(sNodeKeyImage);
	String^ nodeToolTip		= gcnew String(sToolTipText);

	Aga::Controls::Tree::Node^ node = gcnew Aga::Controls::Tree::Node(nodeText, nodeKey, nodeKeyImage, nodeToolTip);

	if (node == nullptr)
		return FALSE;

	Aga::Controls::Tree::Node^ parent = treeViewUC->Model->Root;

	if (parent == nullptr)
		return FALSE;

	parent->Nodes->Add(node);
	
	return TRUE;
}

//------------------------------------------------------------------------------------------------------------------------------------------
BOOL CTreeViewAdvWrapper::InsertFastChild(const CString& sParentKey, const CString& sNodeText, const CString& sNodeKey, const CString& sToolTipText /*= _T("")*/, const CString& sNodeKeyImage /*= _T("")*/)
{
	ENSURE_TREEVIEWADV_CONTROL(FALSE)
	DECLARE_TREEVIEWADV_HANDLER();

	String^ parentKey = gcnew String(sParentKey);
	String^ nodeText = gcnew String(sNodeText);
	String^ nodeKey = gcnew String(sNodeKey);
	String^ nodeKeyImage = gcnew String(sNodeKeyImage);
	String^ nodeToolTip = gcnew String(sToolTipText);

	Aga::Controls::Tree::Node^ node = gcnew Aga::Controls::Tree::Node(nodeText, nodeKey, nodeKeyImage, nodeToolTip);

	if (node == nullptr)
		return FALSE;

	Aga::Controls::Tree::TreeNodeAdv^ parentAdv = treeViewUC->GetNode(parentKey);

	if (parentAdv == nullptr)
		return FALSE;

	Aga::Controls::Tree::Node^ parent = (Aga::Controls::Tree::Node^)parentAdv->Tag;
	parent->Nodes->Add(node);
	
	return TRUE;
}

//--------------------------------------------------------------------------------------------
BOOL CTreeViewAdvWrapper::SetNodeAsSelected(const CString& sNodeKeyToSearch)
{
	ENSURE_TREEVIEWADV_CONTROL(FALSE)

	String^ sKeyToSearch = gcnew String(sNodeKeyToSearch);
	return treeViewUC->SetNodeAsSelected(sKeyToSearch);
}

//----------------------------------------------------------------------------------------------
BOOL CTreeViewAdvWrapper::ExistsNode(const CString& sNodeKeyToSearch)
{
	ENSURE_TREEVIEWADV_CONTROL(FALSE)

	String^ sKeyToSearch = gcnew String(sNodeKeyToSearch);
	return treeViewUC->ExistsNode(sKeyToSearch);
}
	
//-----------------------------------------------------------------------------------------------
BOOL CTreeViewAdvWrapper::InsertChild(const CString& sParentKey, const CString& sNodeText, const CString& sNodeKey, const CString& sNodeKeyImage /*= _T("")*/, const COLORREF color)
{
	ENSURE_TREEVIEWADV_CONTROL(FALSE)
	DECLARE_TREEVIEWADV_HANDLER();

	String^ parentKey		= gcnew String(sParentKey);
	String^ nodeText		= gcnew String(sNodeText);
	String^ nodeKey			= gcnew String(sNodeKey);
	String^ nodeKeyImage	= gcnew String(sNodeKeyImage); 
	String^ toolTipText		= gcnew String(_T(""));
	return treeViewUC->InsertChild(parentKey, nodeText, nodeKey, nodeKeyImage, pTreeViewHandler->ConvertColor(color), toolTipText);
}

//---------------------------------------------------------------------------------------------------------------
BOOL CTreeViewAdvWrapper::InsertChild
								(
									const CString& sParentKey, 
									const CString& sNodeText, 
									const CString& sNodeKey, 
									const CStringArray& sToolTipText, 
									const CString& sNodeKeyImage /*_T("")*/, 
									const COLORREF color
								)
{
	ENSURE_TREEVIEWADV_CONTROL(FALSE)
	DECLARE_TREEVIEWADV_HANDLER();

	String^ parentKey		= gcnew String(sParentKey);
	String^ nodeText		= gcnew String(sNodeText);
	String^ nodeKey			= gcnew String(sNodeKey);
	String^ nodeKeyImage	= gcnew String(sNodeKeyImage); 

	CString toolTip = _T("");
	for (int i = 0; i < sToolTipText.GetSize(); i++)
	{
		if (i > 0)
			toolTip += _T("\n");
		toolTip += sToolTipText.GetAt(i);
	}

	String^ toolTipText		= gcnew String(toolTip);
	EnableToolTip();
	return treeViewUC->InsertChild
							(
								parentKey, 
								nodeText, 
								nodeKey, 
								nodeKeyImage, 
								pTreeViewHandler->ConvertColor(color), 
								toolTipText
							);
}

//------------------------------------------------------------------------------------------------
CString CTreeViewAdvWrapper::GetKeyByPosition(const CPoint& pMouse)
{
	ENSURE_TREEVIEWADV_CONTROL(_T(""))

	Point pm = Point(pMouse.x, pMouse.y);
	Aga::Controls::Tree::TreeNodeAdv^ nodeAdv = treeViewUC->GetNodeAt(pm);
	if (nodeAdv != nullptr)
	{
		Aga::Controls::Tree::Node^ node = (Aga::Controls::Tree::Node^)nodeAdv->Tag;
		return CString(node->ToolTipText);
	}

	return CString(_T(""));
}
//------------------------------------------------------------------------------------------------
CString CTreeViewAdvWrapper::GetParentKey(const CString& sNodeKeyToSearch)
{
	ENSURE_TREEVIEWADV_CONTROL(_T(""))
		
	String^ sKeyToSearch = gcnew String(sNodeKeyToSearch);
	return CString(treeViewUC->GetParentKey(sKeyToSearch));
}

//------------------------------------------------------------------------------------------------------
void CTreeViewAdvWrapper::EditEnabled(const BOOL& bValue /*= FALSE*/)
{
	VOID_ENSURE_TREEVIEWADV_CONTROL()

	treeViewUC->EditEnabled = (bValue == TRUE);
}

//----------------------------------------------------------------------------------------------------
void CTreeViewAdvWrapper::CanDirtectlyEditing(const BOOL& bValue /*= FALSE*/)
{
	VOID_ENSURE_TREEVIEWADV_CONTROL()

	treeViewUC->CanDirectlyEditing = (bValue == TRUE);
}

//----------------------------------------------------------------------------------------------------
void CTreeViewAdvWrapper::BeginEdit()
{
	VOID_ENSURE_TREEVIEWADV_CONTROL()

	treeViewUC->BeginEdit();
}

//----------------------------------------------------------------------------------------------------
void CTreeViewAdvWrapper::BeginUpdate()
{
	VOID_ENSURE_TREEVIEWADV_CONTROL()

	treeViewUC->BeginUpdate();
}

//----------------------------------------------------------------------------------------------------
void CTreeViewAdvWrapper::EndUpdate()
{
	VOID_ENSURE_TREEVIEWADV_CONTROL()

	treeViewUC->EndUpdate();
}

//----------------------------------------------------------------------------------------------------
void CTreeViewAdvWrapper::SetDefaultTextForEditing(const CString& sDefaultTextForEditing)
{
	VOID_ENSURE_TREEVIEWADV_CONTROL()

	String^ sText = gcnew String(sDefaultTextForEditing);
	treeViewUC->DefaultTextForEditing = sText;
}

//------------------------------------------------------------------------------------------
CString	CTreeViewAdvWrapper::GetCurrentTextEdited()
{
	ENSURE_TREEVIEWADV_CONTROL(_T(""))

	return CString(treeViewUC->CurrentTextEdited);
}

//----------------------------------------------------------------------------------------
void CTreeViewAdvWrapper::SetCurrentNodeParentSelected()
{
	VOID_ENSURE_TREEVIEWADV_CONTROL()

	treeViewUC->SelectedNode = treeViewUC->SelectedNode->Parent;
}

//----------------------------------------------------------------------------------------
BOOL CTreeViewAdvWrapper::ExistsSelectedNode()
{
	ENSURE_TREEVIEWADV_CONTROL(FALSE)

	return (treeViewUC->SelectedNode != nullptr);
}

//----------------------------------------------------------------------------------------
int CTreeViewAdvWrapper::GetSelectedNodeLevel()
{
	ENSURE_TREEVIEWADV_CONTROL(FALSE)

	if (treeViewUC->SelectedNode != nullptr)
		return treeViewUC->SelectedNode->Level;
	else
		return 0;
}

//----------------------------------------------------------------------------------------
CString CTreeViewAdvWrapper::GetSelectedNodeKey()
{
	ENSURE_TREEVIEWADV_CONTROL(_T(""))

	if (treeViewUC->SelectedNode != nullptr)
	{
		Aga::Controls::Tree::Node^ node = (Aga::Controls::Tree::Node^)treeViewUC->SelectedNode->Tag;
		return CString(node->Key);
	}
	else
		return _T("");
}

//Metodo che restitusce la root del Tree (un TreeNodeAdv)
//---------------------------------------------------------------------------------------------------------
CTreeNodeAdvWrapperObj* CTreeViewAdvWrapper::GetRoot()
{
	ENSURE_TREEVIEWADV_CONTROL(NULL)

	CObject* pObject;
	if (m_bUseNodesCache && wrappedNodes.Lookup(_T("root"), pObject) && pObject != NULL)
		return (CTreeNodeAdvWrapperObj*) pObject;

	if ( treeViewUC->Root == nullptr )
		return NULL;
	
	CTreeNodeAdvWrapper* pWrappedNode = new CTreeNodeAdvWrapper (treeViewUC->Root);
	wrappedNodes[_T("root")] = pWrappedNode;
	return pWrappedNode;
}

//---------------------------------------------------------------------------------------------------------
void CTreeViewAdvWrapper::SetSelectRoot()
{
	VOID_ENSURE_TREEVIEWADV_CONTROL()

	if (treeViewUC->Root == nullptr || treeViewUC->Root->Children == nullptr ||  treeViewUC->Root->Children->Count == 0)
		return;

    treeViewUC->SelectedNode = treeViewUC->Root->Children[0];
}

//----------------------------------------------------------------------------------------------------------
void CTreeViewAdvWrapper::SetSelectFirstChild()
{
	VOID_ENSURE_TREEVIEWADV_CONTROL()

	if (treeViewUC->SelectedNode != nullptr)
		treeViewUC->SelectedNode = treeViewUC->SelectedNode->Children[0];
}

//----------------------------------------------------------------------------------------------------------
void CTreeViewAdvWrapper::SetNextNode()
{
	VOID_ENSURE_TREEVIEWADV_CONTROL()

		if (treeViewUC->SelectedNode != nullptr)
		{
			if (HasChildrenSelectedNode() && IsExpandedSelectedNode())
			{
				SetSelectFirstChild();
				return;
			}
			
			Aga::Controls::Tree::TreeNodeAdv^  nodeSelect = treeViewUC->SelectedNode;
			Aga::Controls::Tree::TreeNodeAdv^  nodeParent = treeViewUC->SelectedNode->Parent;

			// per evitare il raise dell'evento di SelectionChanged
			Aga::Controls::Tree::TreeNodeAdv^  nodeCandidate = treeViewUC->SelectedNode->NextNode;
			while (nodeCandidate == nullptr && nodeParent != nullptr)
			{
				Aga::Controls::Tree::TreeNodeAdv^  nodeParentPrev = nodeParent->Parent;
				nodeCandidate = nodeParent->NextNode;
				if (nodeCandidate == nullptr)
					nodeParent = nodeParentPrev;
			}

			if (nodeCandidate == nullptr)
				treeViewUC->SelectedNode = nodeSelect;
			else
				treeViewUC->SelectedNode = nodeCandidate;
		}
}

//----------------------------------------------------------------------------------------------------------
void CTreeViewAdvWrapper::SetPrevNode()
{
	VOID_ENSURE_TREEVIEWADV_CONTROL()
	if (treeViewUC->SelectedNode != nullptr)
	{
		Aga::Controls::Tree::TreeNodeAdv^  nodeSelect = treeViewUC->SelectedNode;
		Aga::Controls::Tree::TreeNodeAdv^  nodeParent = treeViewUC->SelectedNode->Parent;
		Aga::Controls::Tree::Node^ node = (Aga::Controls::Tree::Node^)treeViewUC->SelectedNode->Tag;
		int index = node->Index;
		Aga::Controls::Tree::Node^ prevNode = nullptr;
		if (index == 0)
		{
			if (nodeParent != nullptr && treeViewUC->Root != nodeParent)
			{
				treeViewUC->SelectedNode = nodeParent;
			}
			else
			{
				treeViewUC->SelectedNode = nodeSelect;
			} 
			return;
		}
		else
		{
			prevNode = node->PreviousNode;
			TreeNodeAdv^ node = treeViewUC->GetNode(gcnew String(prevNode->Key));
			if (node != nullptr)
			{
				int nChildren = (node->Children)->Count;
				if (nChildren > 0 && node->IsExpanded)
				{
					treeViewUC->SelectedNode = node->Children[nChildren - 1];
					return;
				}
			}
		}

		if (prevNode != nullptr)
		{
			TreeNodeAdv^ node = treeViewUC->GetNode(gcnew String(prevNode->Key));
			treeViewUC->SelectedNode = node;
		}
	}
}

//-----------------------------------------------------------------------------------------------------------
void CTreeViewAdvWrapper::ClearTree()
{
	VOID_ENSURE_TREEVIEWADV_CONTROL()

	if (!treeViewUC)
		return;

	if (treeViewUC->Model != nullptr && treeViewUC->Model->Nodes != nullptr)
		treeViewUC->Model->Nodes->Clear();

	wrappedNodes.Clear();
}

//-----------------------------------------------------------------------------------------------------------
BOOL CTreeViewAdvWrapper::RemoveNode(const CString& sNodeKeyToSearch)
{
	ENSURE_TREEVIEWADV_CONTROL(FALSE)

	if (!treeViewUC)
		return FALSE;

	bool res = treeViewUC->RemoveNode(gcnew String(sNodeKeyToSearch));

	if (res == true)
		return TRUE;

	return FALSE;
}

//-------------------------------------------------------------------------------------------------------------------
//creates a new node (use this method with stack management of old nodes)
void CTreeViewAdvWrapper::CreateNewNode(const CString& sNodeText, const CString& sNodeKey)
{
	VOID_ENSURE_TREEVIEWADV_CONTROL()

	String^ nodeText	= gcnew String(sNodeText);
	String^ nodeKey		= gcnew String(sNodeKey);
	treeViewUC->CreateNewNode(nodeText, nodeKey);
}

//-------------------------------------------------------------------------------------------------------------------
//creates a new node (use this method with stack management of old nodes)
void CTreeViewAdvWrapper::CreateNewNode(const CString& sNodeText, const CString& sNodeKey, const CString& sNodeKeyImage)
{
	VOID_ENSURE_TREEVIEWADV_CONTROL()

	String^ nodeText		= gcnew String(sNodeText);
	String^ nodeKey			= gcnew String(sNodeKey);
	String^ nodeKeyImage	= gcnew String(sNodeKeyImage);
	treeViewUC->CreateNewNode(nodeText, nodeKey, nodeKeyImage);
}

//----------------------------------------------------------------------------------------------------------
void CTreeViewAdvWrapper::CreateNewNode
									(
										const CString& sNodeText, 
										const CString& sNodeKey, 
										const CStringArray& sToolTipText, 
										const CString& sNodeKeyImage
									)
{
	VOID_ENSURE_TREEVIEWADV_CONTROL()

	String^ nodeText		= gcnew String(sNodeText);
	String^ nodeKey			= gcnew String(sNodeKey);
	String^ nodeKeyImage	= gcnew String(sNodeKeyImage);

	CString toolTip = _T("");
	for (int i = 0; i < sToolTipText.GetSize(); i++)
	{
		if (i > 0)
			toolTip += _T("\n");
		toolTip += sToolTipText.GetAt(i);
	}
	
	String^ textToolTip		= gcnew String(toolTip);
	
	treeViewUC->CreateNewNode(nodeText, nodeKey, textToolTip, nodeKeyImage);
}

//-------------------------------------------------------------------------------------------------------------------
//returns the number of nodes memorized in the stack of old nodes (use this method with stack management of old nodes)
int CTreeViewAdvWrapper::GetOldNodesCount()
{
	ENSURE_TREEVIEWADV_CONTROL(0)

	return treeViewUC->OldNodesCount();
}

//-------------------------------------------------------------------------------------------------------------------
//removes and returns the node at the top (use this method with stack management of old nodes)
void CTreeViewAdvWrapper::OldNodesPop()
{
	VOID_ENSURE_TREEVIEWADV_CONTROL()

	treeViewUC->OldNodesPop();
}

//-------------------------------------------------------------------------------------------------------------------
//returns the Node at the top without removing it (use this method with stack management of old nodes)
void CTreeViewAdvWrapper::OldNodesPeek()
{
	VOID_ENSURE_TREEVIEWADV_CONTROL()

	treeViewUC->OldNodesPeek();
}

//-------------------------------------------------------------------------------------------------------------------
//inserts the Node created with CreateNewNode method at the top (use this method with stack management of old nodes)
void CTreeViewAdvWrapper::OldNodesPush()
{
	VOID_ENSURE_TREEVIEWADV_CONTROL()

	treeViewUC->OldNodesPush();
}

//-------------------------------------------------------------------------------------------------------------------
//emptying the collection of old nodes(use this method with stack management of old nodes)
void CTreeViewAdvWrapper::OldNodesClear()
{
	VOID_ENSURE_TREEVIEWADV_CONTROL()

	treeViewUC->OldNodesClear();
}

//-------------------------------------------------------------------------------------------------------------------
//sets the color of the new node created with CreateNewNode method or managing CurrentNode 
void CTreeViewAdvWrapper::SetColorNewNode(const COLORREF color)
{
	VOID_ENSURE_TREEVIEWADV_CONTROL()
	DECLARE_TREEVIEWADV_HANDLER();

	treeViewUC->SetColorNewNode(pTreeViewHandler->ConvertColor(color));
}

//-------------------------------------------------------------------------------------------------------------------
//sets the color of the new node created with CreateNewNode method or managing NewNode 
void CTreeViewAdvWrapper::SetColorCurrentNode(const COLORREF color)
{
	VOID_ENSURE_TREEVIEWADV_CONTROL()
	DECLARE_TREEVIEWADV_HANDLER();

	treeViewUC->SetColorCurrentNode(pTreeViewHandler->ConvertColor(color));
}

//-------------------------------------------------------------------------------------------------------------------
//add the new node created with CreateNewNode method to the selected node (use this method with stack management of old nodes)
void CTreeViewAdvWrapper::AddToSelectedNode()
{
	VOID_ENSURE_TREEVIEWADV_CONTROL()

	treeViewUC->AddToSelectedNode();
}

//-------------------------------------------------------------------------------------------------------------------
//add the new node created with CreateNewNode method to the current node (use this method with stack management of old nodes)
void CTreeViewAdvWrapper::AddToCurrentNode()
{
	VOID_ENSURE_TREEVIEWADV_CONTROL()

	treeViewUC->AddToCurrentNode();
}

//Select the node with specified key
//-------------------------------------------------------------------------------------------------------------------
void CTreeViewAdvWrapper::SelectNode (const CString& sNodeKey)
{
	ASSERT(AfxIsRemoteInterface());

	CTreeNodeAdvWrapperObj* pNode = GetNode(sNodeKey);
	if (pNode)
	{
		pNode->SelectNode();
	}
}
	
//Toggle (expand if it's collapsed, collapse if it's expanded) the node with specified key
//-------------------------------------------------------------------------------------------------------------------
void CTreeViewAdvWrapper::ToggleNode(const CString& sNodeKey)
{
	CTreeNodeAdvWrapperObj* pNode = GetNode(sNodeKey);
	if (pNode)
	{
		pNode->ToggleNode();
	}
}

//----------------------------------------------------------------------------------------------------------------------
void CTreeViewAdvWrapper::AddToSelectedAndSetNodeAsCurrent(const CString& sNodeText, const CString& sNodeKey)
{
	VOID_ENSURE_TREEVIEWADV_CONTROL()

	String^ nodeText	= gcnew String(sNodeText);
	String^ nodeKey		= gcnew String(sNodeKey);
	treeViewUC->AddToSelectedAndSetNodeAsCurrent(nodeText, nodeKey);
}

//----------------------------------------------------------------------------------------------------------------------
void CTreeViewAdvWrapper::AddToSelectedAndSetNodeAsCurrent
													(
														const CString& sNodeText, 
														const CString& sNodeKey, 
														const CString& sNodeKeyImage
													)
{
	VOID_ENSURE_TREEVIEWADV_CONTROL()

	String^ nodeText		= gcnew String(sNodeText);
	String^ nodeKey			= gcnew String(sNodeKey);
	String^ nodeKeyImage	= gcnew String(sNodeKeyImage);
	String^ toolTipText		= gcnew String(_T(""));
	treeViewUC->AddToSelectedAndSetNodeAsCurrent(nodeText, nodeKey, nodeKeyImage, toolTipText);
}

//----------------------------------------------------------------------------------------------------------------------
void CTreeViewAdvWrapper::AddToSelectedAndSetNodeAsCurrent
													(
														const CString& sNodeText, 
														const CString& sNodeKey, 
														const CStringArray& sToolTipText, 
														const CString& sNodeKeyImage
													)
{
	VOID_ENSURE_TREEVIEWADV_CONTROL()

	String^ nodeText		= gcnew String(sNodeText);
	String^ nodeKey			= gcnew String(sNodeKey);
	String^ nodeKeyImage	= gcnew String(sNodeKeyImage);

	CString toolTip = _T("");
	for (int i = 0; i < sToolTipText.GetSize(); i++)
	{
		if (i > 0)
			toolTip += _T("\n");
		toolTip += sToolTipText.GetAt(i);
	}

	String^ toolTipText		= gcnew String(toolTip);
	EnableToolTip();
	treeViewUC->AddToSelectedAndSetNodeAsCurrent(nodeText, nodeKey, nodeKeyImage, toolTipText);
}

//----------------------------------------------------------------------------------------------------------------------
void CTreeViewAdvWrapper::AddAndSetNewNodeFromCurrent(const CString& sNodeText, const CString& sNodeKey)
{
	VOID_ENSURE_TREEVIEWADV_CONTROL()

	String^ nodeText	= gcnew String(sNodeText);
	String^ nodeKey		= gcnew String(sNodeKey);
	treeViewUC->AddAndSetNewNodeFromCurrent(nodeText, nodeKey);
}

//----------------------------------------------------------------------------------------------------------------------
void CTreeViewAdvWrapper::AddAndSetNewNodeFromCurrent
													(
														const CString& sNodeText, 
														const CString& sNodeKey, 
														const CString& sNodeKeyImage
													)
{
	VOID_ENSURE_TREEVIEWADV_CONTROL()

	String^ nodeText		= gcnew String(sNodeText);
	String^ nodeKey			= gcnew String(sNodeKey);
	String^ nodeKeyImage	= gcnew String(sNodeKeyImage);
	String^ toolTipText		= gcnew String(_T(""));
	treeViewUC->AddAndSetNewNodeFromCurrent(nodeText, nodeKey, nodeKeyImage, toolTipText);

}

//----------------------------------------------------------------------------------------------------------------------
void CTreeViewAdvWrapper::AddAndSetNewNodeFromCurrent
													(
														const CString& sNodeText, 
														const CString& sNodeKey, 
														const CStringArray& sToolTipText,
														const CString& sNodeKeyImage
													)
{
	VOID_ENSURE_TREEVIEWADV_CONTROL()

	String^ nodeText		= gcnew String(sNodeText);
	String^ nodeKey			= gcnew String(sNodeKey);
	String^ nodeKeyImage	= gcnew String(sNodeKeyImage);

	CString toolTip = _T("");
	for (int i = 0; i < sToolTipText.GetSize(); i++)
	{
		if (i > 0)
			toolTip += _T("\n");
		toolTip += sToolTipText.GetAt(i);
	}

	String^ toolTipText		= gcnew String(toolTip);
	EnableToolTip();
	treeViewUC->AddAndSetNewNodeFromCurrent(nodeText, nodeKey, nodeKeyImage, toolTipText);

}

//----------------------------------------------------------------------------------------------------------------------
void CTreeViewAdvWrapper::AddAndSetNewNodeFromActual(const CString& sNodeText, const CString& sNodeKey)
{
	VOID_ENSURE_TREEVIEWADV_CONTROL()

	String^ nodeText	= gcnew String(sNodeText);
	String^ nodeKey		= gcnew String(sNodeKey);
	treeViewUC->AddAndSetNewNodeFromActual(nodeText, nodeKey);
}

//-------------------------------------------------------------------------------------------------------------------------------------------
void CTreeViewAdvWrapper::AddAndSetNewNodeFromActual
													(
														const CString& sNodeText, 
														const CString& sNodeKey, 
														const CString& sNodeKeyImage
													)
{
	VOID_ENSURE_TREEVIEWADV_CONTROL()

	String^ nodeText	= gcnew String(sNodeText);
	String^ nodeKey		= gcnew String(sNodeKey);
	String^ nodeKeyImage	= gcnew String(sNodeKeyImage);
	String^ toolTipText		= gcnew String(_T(""));
	treeViewUC->AddAndSetNewNodeFromActual(nodeText, nodeKey, nodeKeyImage, toolTipText);
}

//-------------------------------------------------------------------------------------------------------------------------------------------
void CTreeViewAdvWrapper::AddAndSetNewNodeFromActual
													(
														const CString& sNodeText, 
														const CString& sNodeKey, 
														const CStringArray& sToolTipText,
														const CString& sNodeKeyImage 
													)
{
	VOID_ENSURE_TREEVIEWADV_CONTROL()

	String^ nodeText	= gcnew String(sNodeText);
	String^ nodeKey		= gcnew String(sNodeKey);
	String^ nodeKeyImage	= gcnew String(sNodeKeyImage);

	CString toolTip = _T("");
	for (int i = 0; i < sToolTipText.GetSize(); i++)
	{
		if (i > 0)
			toolTip += _T("\n");
		toolTip += sToolTipText.GetAt(i);
	}

	String^ toolTipText		= gcnew String(toolTip);
	EnableToolTip();
	treeViewUC->AddAndSetNewNodeFromActual(nodeText, nodeKey, nodeKeyImage, toolTipText);
	
}
	
//-------------------------------------------------------------------------------------------------------------------------------------------
void CTreeViewAdvWrapper::DeleteSelectedNode()
{
	VOID_ENSURE_TREEVIEWADV_CONTROL()

	if (treeViewUC->SelectedNode == nullptr)
		return;

	((Aga::Controls::Tree::Node^)treeViewUC->SelectedNode->Tag)->Parent = nullptr;
}

//-------------------------------------------------------------------------------------------------------------------------------------------------
void CTreeViewAdvWrapper::MoveChildrenOfSelectedNodeToChildrenOfRoot(const BOOL& bAlsoLeaves /*= FALSE*/)
{
	VOID_ENSURE_TREEVIEWADV_CONTROL()

	treeViewUC->MoveChildrenOfSelectedNodeToChildrenOfRoot(bAlsoLeaves == TRUE);
}

//-------------------------------------------------------------------------------------------------------------------------------------------------
void CTreeViewAdvWrapper::MoveSelectedNodeToChildrenOfRoot()
{
	VOID_ENSURE_TREEVIEWADV_CONTROL()

	treeViewUC->MoveSelectedNodeToChildrenOfRoot();
}

//----------------------------------------------------------------------------------------------------
void CTreeViewAdvWrapper::EndEdit(bool bCancel)
{
	VOID_ENSURE_TREEVIEWADV_CONTROL()

		if (treeViewUC->SelectedNode == nullptr)
			return;

	treeViewUC->NodeTextBox->EndEdit(bCancel);
}
#endif