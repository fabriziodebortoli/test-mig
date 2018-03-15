// cannot use C++ managed namespaces in order to avoid stdafx.h conflicts
#include "stdafx.h"
#include "windows.h"

#include <TbGenlib\parsctrl.h>
#include <TbGes\extdoc.h>
#include <TbGes\dbt.h>

#include <TbGenlibManaged\UserControlHandlersMixed.h>
#include "MTreeView.h"

using namespace Microarea::Framework::TBApplicationWrapper;
using namespace Microarea::TaskBuilderNet::Core::NameSolver;
using namespace System::IO;
using namespace System::Text;
using namespace System;
using namespace System::Collections::Generic;
using namespace System::Drawing;

/////////////////////////////////////////////////////////////////////////////
// 						class MTreeView Implementation
/////////////////////////////////////////////////////////////////////////////

//----------------------------------------------------------------------------
MTreeView::MTreeView (IntPtr handleWndPtr)
	:
	BaseWindowWrapper(handleWndPtr)
{
	this->IsStretchable = true;
	InitializeTree();
}

//----------------------------------------------------------------------------
MTreeView::MTreeView (IWindowWrapperContainer^ parentWindow, System::String^ name, System::String^ controlClass, Point location, bool hasCodeBehind)
	:
	BaseWindowWrapper(parentWindow, name, controlClass, location, hasCodeBehind)
{	
	this->IsStretchable = true;
	InitializeTree();

	Location = location;
}

//----------------------------------------------------------------------------
void MTreeView::InitializeTree()
{
	m_pTreeView = (CTreeViewAdvCtrl*) GetWnd();
	m_pTreeView->SetUseNodesCache(FALSE);
	CManagedCtrl* pManCtrl = (CManagedCtrl*)GetWnd();
	CUserControlHandlerMixed* pHandler =  (CUserControlHandlerMixed*) pManCtrl->GetHandler();
	treeViewUC = (TreeViewAdv^)pHandler->GetWinControl();
	
	treeViewUC->SelectionChanged		+=	gcnew EventHandler (this, &MTreeView::ManagedSelectionChanged);
	treeViewUC->ContextMenuItemClick	+=	gcnew EventHandler (this, &MTreeView::ManagedContextMenuItemClick);
	treeViewUC->OnWndProc				+=	gcnew EventHandler<WndProcEventArgs^> (this, &MTreeView::OnWndProc);
	treeViewUC->OnAfterWndProc			+=	gcnew EventHandler<WndProcEventArgs^> (this, &MTreeView::OnAfterWndProc);
	treeViewUC->SetNodeStateIcon(true);
	treeViewUC->AddControls();
}


//-----------------------------------------------------------------------------
MTreeView::~MTreeView()
{
	this->!MTreeView ();
	GC::SuppressFinalize(this);
}

//-----------------------------------------------------------------------------
MTreeView::!MTreeView()
{
	if (!m_pTreeView)
		return;
	
	if (!HasCodeBehind)
	{
		CWnd* pParentWnd = m_pTreeView->GetParent();
		if (pParentWnd)
		{
			CParsedForm* pParsedForm = GetParsedForm(pParentWnd);
			if (pParsedForm)
				pParsedForm->GetControlLinks()->Remove(m_pTreeView);
		}
		m_pTreeView->DestroyWindow();
		delete m_pTreeView;	
	}
	treeViewUC->SelectionChanged		-=	gcnew EventHandler (this, &MTreeView::ManagedSelectionChanged);
	treeViewUC->ContextMenuItemClick	-=	gcnew EventHandler (this, &MTreeView::ManagedContextMenuItemClick);
	treeViewUC->OnWndProc				-=	gcnew EventHandler<WndProcEventArgs^> (this, &MTreeView::OnWndProc);
	treeViewUC->OnAfterWndProc			-=	gcnew EventHandler<WndProcEventArgs^> (this, &MTreeView::OnAfterWndProc);
	m_pTreeView = NULL;
}

//----------------------------------------------------------------------------
void MTreeView::Initialize ()
{
	BaseWindowWrapper::Initialize ();
	minSize = CUtility::GetIdealTreeViewSize();

	images = gcnew List<System::String^>();
}

//----------------------------------------------------------------------------
void MTreeView::ManagedSelectionChanged (System::Object^ sender, System::EventArgs^ args)
{
	EasyBuilderEventArgs^ ebArgs = gcnew EasyBuilderEventArgs();
	SelectionChanged(this, ebArgs);
}

//----------------------------------------------------------------------------
void MTreeView::ManagedContextMenuItemClick (System::Object^ sender, System::EventArgs^ args)
{
	EasyBuilderEventArgs^ ebArgs = gcnew EasyBuilderEventArgs();
	ContextMenuItemClick(this, ebArgs);
} 

//----------------------------------------------------------------------------
List<System::String^>^ MTreeView::GetImageKeys()
{
	return treeViewUC->GetImageKeys();
}

//----------------------------------------------------------------------------
void MTreeView::OnWndProc (System::Object^ sender, WndProcEventArgs^ args)
{
	if (WndProc(args->Msg))
		args->Handled = true;
}
//----------------------------------------------------------------------------
void MTreeView::OnAfterWndProc (System::Object^ sender, WndProcEventArgs^ args)
{
	AfterWndProc(args->Msg);
}
//----------------------------------------------------------------------------
INameSpace^	MTreeView::Namespace::get () 
{ 
	if (!m_pTreeView)
	{
		ASSERT(FALSE);
		return nullptr; 
	}

	return gcnew NameSpace(gcnew System::String(m_pTreeView->GetNamespace().ToString()));
} 

//----------------------------------------------------------------------------
System::String^	MTreeView::Name::get () 
{ 
	if (m_pTreeView)
		return gcnew System::String(m_pTreeView->GetNamespace().GetObjectName());

	return __super::Name;
} 

//-----------------------------------------------------------------------------
void MTreeView::Name::set (System::String^ value)
{
	if (HasCodeBehind || !m_pTreeView)
		return;

	m_pTreeView->GetNamespace().SetObjectName(value);
}

//----------------------------------------------------------------------------
bool MTreeView::CanCreate ()
{
	return !GetHandle();
}

//----------------------------------------------------------------------------
bool MTreeView::Create (IWindowWrapperContainer^ parentWindow, Point location, System::String^ className)
{
	if (parentWindow == nullptr || !CanCreate())
		return false;

	CWnd* pParentWnd = ((BaseWindowWrapper^) parentWindow)->GetWnd();

	CParsedForm* pParsedForm = GetParsedForm(pParentWnd);
	if (!pParsedForm)
		return false;
	
	CPoint aPt(location.X, location.Y);
	CSize aSize(minSize.Width, minSize.Height);
	SafeMapDialog(pParentWnd->m_hWnd, aSize);//trasformo in pixel

	CString sFamilyName (MTreeView::typeid->FullName);
	CString sControlClassName (className);

	if (sControlClassName.IsEmpty())
		sControlClassName = AfxGetParsedControlsRegistry()->GetFamilyDefaultControl(sFamilyName);

	CRegisteredParsedCtrl* pCtrl = AfxGetParsedControlsRegistry()->GetRegisteredControl(sFamilyName, sControlClassName);
	if (!pCtrl)
		return false;

	DWORD styles = WS_GROUP | WS_VISIBLE;

	CTBNamespace aNamespace = CString(CreateNamespaceFromParent(parentWindow));
	UINT nID = AfxGetTBResourcesMap()->GetTbResourceID(aNamespace.GetObjectName(), TbControls);

	CStatic* pButton = new CStatic();
	if (!pButton->Create 
			(
				_T(""), 
				styles, 
				CRect (aPt, aSize), 
				pParentWnd, 
				nID
			)
		)
	{
		delete pButton;
		pButton = NULL;
		return false;
	}
	pButton->UnsubclassWindow();
	delete pButton;
		
	m_pTreeView = (CTreeViewAdvCtrl*) AddLink
										(
											CString(name),
											pParentWnd,
											pParsedForm->GetControlLinks(), 
											nID,
											NULL,
											NULL,
											RUNTIME_CLASS(CTreeViewAdvCtrl)
										);
	

	if (m_pTreeView)
	{
		m_pTreeView->GetInfoOSL()->m_Namespace.SetChildNamespace(CTBNamespace::CONTROL, CString(name), pParsedForm->GetNamespace());
		Handle = (IntPtr) m_pTreeView->m_hWnd;
		HasCodeBehind = false;
		m_pTreeView->ShowWindow(SW_SHOW);
	}

	return m_pTreeView != NULL;
}

//----------------------------------------------------------------------------
System::String^ MTreeView::GetImageKeyFromIndex(int imageIndex)
{
	System::String^ imgKey = System::String::Empty;
	
	if (imageIndex < 0)
		return imgKey;

	if (imageIndex < images->Count)	
	{
		imgKey = images[imageIndex];
		AddImage(imgKey); //l'immagine viene aggiunta solo se non gia' esistente
	}
	else if (HasCodeBehind) //potrebbe gia essere caricata nell'imageList del treeView c# ma non nella proprieta' del mtreeview (caso di treeView codeBehind)
	{
		List<System::String^>^ keys =  GetImageKeys();
		if (imageIndex < keys->Count)
			return keys[imageIndex];
	}
	return imgKey;
}

//----------------------------------------------------------------------------
void MTreeView::AddNode(System::String^ sText, System::String^ sNodeKey, System::String^ sImage)	
{ 
	treeViewUC->AddTreeNode(sText, sNodeKey, sImage);
}

//----------------------------------------------------------------------------
void MTreeView::AddNode(System::String^ sText, System::String^ sNodeKey, int imageIndex)	
{ 
	System::String^ imgNamespace = GetImageKeyFromIndex(imageIndex);

	treeViewUC->AddTreeNode(sText, sNodeKey, imgNamespace);
}

//----------------------------------------------------------------------------
void MTreeView::ClearTree()
{ 
	if (treeViewUC->Model != nullptr && treeViewUC->Model->Nodes != nullptr)
		treeViewUC->Model->Nodes->Clear();
}

//----------------------------------------------------------------------------
void MTreeView::InsertChild(System::String^ parentKey, System::String^ text, System::String^ nodeKey, System::String^ image)
{
	InsertChild(parentKey, text, nodeKey, image, System::String::Empty);
}

//----------------------------------------------------------------------------
void MTreeView::InsertChild(System::String^ parentKey, System::String^ text, System::String^ nodeKey, System::String^ image, System::String^ toolTip)
{
	AddImage(image);
	treeViewUC->InsertChild(parentKey, text, nodeKey, image, System::Drawing::Color::Black, toolTip);
}

//----------------------------------------------------------------------------
void MTreeView::InsertChild(System::String^ parentKey, System::String^ text, System::String^ nodeKey, int imageIndex)
{
	InsertChild(parentKey, text, nodeKey, imageIndex, System::String::Empty);
}

//----------------------------------------------------------------------------
void MTreeView::InsertChild(System::String^ parentKey, System::String^ text, System::String^ nodeKey, int imageIndex, System::String^ toolTip)
{
	System::String^ imgNamespace = GetImageKeyFromIndex(imageIndex);
	
	treeViewUC->InsertChild(parentKey, text, nodeKey, imgNamespace, System::Drawing::Color::Black, toolTip);
}
		
//----------------------------------------------------------------------------
void MTreeView::AddContextMenuItem(System::String^ menuItem)
{
	treeViewUC->AddContextMenuItem(menuItem, false);
}

//----------------------------------------------------------------------------
void MTreeView::SetMenuItemCheck(System::String^ itemMenu, bool check)
{
	treeViewUC->SetMenuItemCheck(itemMenu, check);			
}

//----------------------------------------------------------------------------
void MTreeView::SetMenuItemEnable(System::String^ itemMenu, bool enabled)
{
	treeViewUC->SetMenuItemEnable(itemMenu, enabled);
}

//----------------------------------------------------------------------------
void MTreeView::AddContextSubMenuItem(System::String^ itemMenu, array<System::String^>^ subItems)
{
	treeViewUC->AddContextSubMenuItem(itemMenu, subItems, FALSE);
}

//-------------------------------------------------------------------------------------------------------------------
void MTreeView::SelectNode (System::String^ sNodeKey)
{
	treeViewUC->SelectNode(sNodeKey);
}
	
//-------------------------------------------------------------------------------------------------------------------
void MTreeView::ToggleNode(System::String^ sNodeKey)
{
	treeViewUC->ToggleNode(sNodeKey);
}

//-------------------------------------------------------------------------------------------------------------------
System::String^ MTreeView::GetSelectedNodeKey()
{
	if (treeViewUC->SelectedNode != nullptr)
	{
		Aga::Controls::Tree::Node^ node = (Aga::Controls::Tree::Node^)treeViewUC->SelectedNode->Tag;
		return node->Key;
	}
	else
		return System::String::Empty;
}

//-------------------------------------------------------------------------------------------------------------------
bool MTreeView::SetNodeAsSelected (System::String^ sNodeKey)
{
	return treeViewUC->SetNodeAsSelected(sNodeKey);
}
		
//-------------------------------------------------------------------------------------------------------------------
bool MTreeView::ExistsNode (System::String^ sNodeKey)
{
	return treeViewUC->ExistsNode(sNodeKey);
}

//-------------------------------------------------------------------------------------------------------------------
System::String^ MTreeView::GetParentKey (System::String^ sNodeKey)
{
	return treeViewUC->GetParentKey(sNodeKey);
}
	
//-------------------------------------------------------------------------------------------------------------------
void MTreeView::ExpandAll()
{
	treeViewUC->ExpandAll(true);
}

//-------------------------------------------------------------------------------------------------------------------
void MTreeView::CollapseAll()
{
	treeViewUC->CollapseAll(true);
}
	
//-------------------------------------------------------------------------------------------------------------------
void MTreeView::AddContextMenuSeparator()
{
	treeViewUC->AddSeparatorMenuItem();
}
	
//-------------------------------------------------------------------------------------------------------------------
System::String^ MTreeView::GetTextContextMenuItemClicked()
{
	return treeViewUC->TextContextMenuItemClicked;
}

//-------------------------------------------------------------------------------------------------------------------
void MTreeView::AddImage(System::String^ imageKey)
{
	CString strPath = AfxGetPathFinder()->GetFileNameFromNamespace(CString(imageKey), AfxGetLoginInfos()->m_strUserName);
	System::String^ fullPath = gcnew System::String(strPath);
	if (!File::Exists(fullPath))
		return;

	treeViewUC->SetNodeStateIcon(true);
	treeViewUC->AddImage(imageKey, fullPath);
	treeViewUC->AddControls();
}

//-------------------------------------------------------------------------------------------------------------------
void MTreeView::DeleteNode(System::String^ sNodeKey)
{
	TreeNodeAdv^ pNode = treeViewUC->GetNode(sNodeKey);
	if (pNode)
	{
		((Aga::Controls::Tree::Node^)pNode->Tag)->Parent = nullptr;
	}
}
	
//-------------------------------------------------------------------------------------------------------------------
void MTreeView::Enable(bool bValue)
{
	treeViewUC->Enabled = bValue;
}
	
//----------------------------------------------------------------------------
System::String^ MTreeView::ClassName::get ()	
{ 
	return ClassType->ClassName;
}  

//----------------------------------------------------------------------------
ControlClass^ MTreeView::ClassType::get()
{ 
	return gcnew ControlClass(this);
}

//----------------------------------------------------------------------------
int	MTreeView::GetNamespaceType ()
{
	return CTBNamespace::CONTROL;
}

//----------------------------------------------------------------------------
System::String^ MTreeView::SerializedType::get ()	
{ 
	return GetType()->Name; 
}

//----------------------------------------------------------------------------
void MTreeView::Handle::set (IntPtr handle)
{
	this->handle = handle;

}

//-----------------------------------------------------------------------------
Int32 MTreeView::TbHandle::get()
{
	return (long) m_pTreeView;
}

//-----------------------------------------------------------------------------
System::Drawing::Size MTreeView::Size::get()
{
	return __super::Size;
}

//-----------------------------------------------------------------------------
void MTreeView::Size::set(System::Drawing::Size size)
{
	__super::Size = size;
}

//--------------------------------------------------------------------------------------
void MTreeView::Location::set(System::Drawing::Point value)
{
	__super::Location = value;
}

//-----------------------------------------------------------------------------
void MTreeView::AutoStretch::set (bool value)
{
	if (!m_pTreeView)
		return;

	__super::AutoStretch = value;
}

//-----------------------------------------------------------------------------
bool MTreeView::BottomStretch::get()
{
	return m_pTreeView && __super::BottomStretch;
}

//-----------------------------------------------------------------------------
void MTreeView::BottomStretch::set(bool value)
{
	//Do nothing, the 'AutoFill' property now does the work
	//Left for backward compatibility for old EasyBuilder customization

	if (!m_pTreeView)
		return;

	__super::BottomStretch = value;
}

//-----------------------------------------------------------------------------
bool MTreeView::RightStretch::get()
{
	return m_pTreeView && __super::RightStretch;
}

//-----------------------------------------------------------------------------
void MTreeView::RightStretch::set(bool value)
{
	if (!m_pTreeView)
		return;

	__super::RightStretch = value;
}


//-----------------------------------------------------------------------------
bool MTreeView::AutoFill::get()
{
	return m_pTreeView && __super::AutoFill;
}

//-----------------------------------------------------------------------------
void MTreeView::AutoFill::set(bool value)
{
	if (!m_pTreeView)
		return;

	__super::AutoFill = value;
}

//-----------------------------------------------------------------------------
void MTreeView::Images::set(List<System::String^>^ value)
{
	images = value;
}

//-----------------------------------------------------------------------------
List<System::String^>^ MTreeView::Images::get()
{
	return images;
}

//----------------------------------------------------------------------------
bool MTreeView::Border::get()
{
	CWndObjDescription* pDescri = GetWndObjDescription();
	return pDescri ? pDescri->m_bBorder : HasExStyle(WS_EX_CLIENTEDGE);
}

//----------------------------------------------------------------------------
void MTreeView::Border::set(bool value)
{
	SetExStyle(SET_STYLE_PARAMS(WS_EX_CLIENTEDGE));

	CWndObjDescription* pDescri = GetWndObjDescription();
	if (pDescri)
		pDescri->m_bBorder = value;
}
