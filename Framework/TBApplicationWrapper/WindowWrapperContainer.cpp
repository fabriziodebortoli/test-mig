#include "stdafx.h"
#include "WindowWrapperContainer.h"
#include "MBodyEdit.h"
#include "MTileDialog.h"
#include "MTilePanel.h"
#include "MTileGroup.h"
#include "MView.h"

using namespace System;
using namespace System::Windows::Forms;
using namespace Microarea::Framework::TBApplicationWrapper;

/////////////////////////////////////////////////////////////////////////////
// 				class WindowWrapperContainer Implementation
/////////////////////////////////////////////////////////////////////////////
//----------------------------------------------------------------------------
WindowWrapperContainer::WindowWrapperContainer(System::IntPtr wrappedObject)
	:
	BaseWindowWrapper(wrappedObject)
{
	components = gcnew List<IComponent^>();
	flex = -1;
	lastEditDPI = 96;
}

//----------------------------------------------------------------------------
WindowWrapperContainer::WindowWrapperContainer(IWindowWrapperContainer^ parentWindow, System::String^ name, System::String^ className, Point location, bool hasCodeBehind)
	:
	BaseWindowWrapper(parentWindow, name, className, location, hasCodeBehind)
{
	components = gcnew List<IComponent^>();
	flex = -1;
	if (parentWindow != nullptr)
		lastEditDPI = ((WindowWrapperContainer^) parentWindow)->LastEditDPI;
}

//----------------------------------------------------------------------------
WindowWrapperContainer::~WindowWrapperContainer()
{
	ClearComponents();
	this->!WindowWrapperContainer();
	GC::SuppressFinalize(this);
}

//----------------------------------------------------------------------------
WindowWrapperContainer::!WindowWrapperContainer()
{

}

//----------------------------------------------------------------------------
INameSpace^ WindowWrapperContainer::Namespace::get()
{
	CParsedForm* pParsedForm = GetParsedForm(GetWnd());
	System::String^ nameSpace = System::String::Empty;
	if (pParsedForm)
		nameSpace = gcnew System::String(pParsedForm->GetNamespace().ToString());

	return gcnew NameSpace(nameSpace);
}

//----------------------------------------------------------------------------
Point WindowWrapperContainer::GetScrollPosition()
{
	CWnd* pWnd = GetWnd();
	if (pWnd && pWnd->IsKindOf(RUNTIME_CLASS(CScrollView)))
	{
		CPoint pt = ((CScrollView*)pWnd)->GetScrollPosition();
		return Point(pt.x, pt.y);
	}
	return Point::Empty;
}


//----------------------------------------------------------------------------
System::IntPtr WindowWrapperContainer::DocumentPtr::get()
{
	return System::IntPtr::Zero;
}

//----------------------------------------------------------------------------
System::String^ WindowWrapperContainer::Name::get()
{
	CParsedForm* pParsedForm = GetParsedForm(GetWnd());
	System::String^ name = System::String::Empty;
	if (pParsedForm)
		name = gcnew System::String(pParsedForm->GetNamespace().GetObjectName());

	return name;
}


//----------------------------------------------------------------------------
bool WindowWrapperContainer::WndProc(Message% m)
{
	switch (m.Msg)
	{
	case WM_DESTROY:
	{
		ClearComponents();
		break;
	}
	case WM_HSCROLL:
	case WM_VSCROLL:
		OnScrollChanged(gcnew WindowMessageEventArgs(m.Msg, m.WParam, m.LParam));
		break;
	case WM_COMMAND:
	{
		WPARAM wParam = (WPARAM)(int)m.WParam;
		LPARAM lParam = (LPARAM)(int)m.LParam;
		DECLARE_WM_COMMAND_PARAMS(wParam, lParam, nID, nCode, hwnd);
		switch (nCode)
		{
		case BEN_ROW_CHANGED:
		{
			EasyBuilderEventArgs^ args = gcnew EasyBuilderEventArgs();
			::SendMessage(hwnd, UM_EASYBUILDER_ACTION, RowChanged, (LPARAM)&args);
			if (args->Handled)
				return true;//mangio il messaggio solo se è stato gestito dal chiamato
			break;
			//return true; //mangio il messaggio
		}
		case BN_CLICKED:
		{
			EasyBuilderEventArgs^ args = gcnew EasyBuilderEventArgs();
			::SendMessage(hwnd, UM_EASYBUILDER_ACTION, Clicked, (LPARAM)&args);
			if (args->Handled)
				return true;//mangio il messaggio solo se è stato gestito dal chiamato
			break;
			//non mangio il messaggio perchè, per esempio nei wizard,
			//avveniva che, se presente una customizzazione, i click sui
			//pulsanti Next, Prev ecc, non sortiva alcun effetto.
		}
		}
		break;
	}
	}
	return __super::WndProc(m);
}


//----------------------------------------------------------------------------
//Inserisce in foundChildren tutti i controlli che contengono il punto screenPoint.
//Discrimina tra container e non container: i controlli NON container sono
//aggiunti per primi nella collezione, i controlli container sono aggiunti per ultimi.
void WindowWrapperContainer::GetChildrenFromPos(
	System::Drawing::Point screenPoint,
	System::IntPtr handleToSkip,
	System::Collections::Generic::ICollection<IWindowWrapper^>^ foundChildren
	)
{
	CWnd* pWndEditingArea = GetWnd();
	if (pWndEditingArea == nullptr)
		return;

	HWND hHandleToSkip = (HWND)(int)handleToSkip;
	CPoint aPt(screenPoint.X, screenPoint.Y);

	CWnd* pChild = pWndEditingArea->GetWindow(GW_CHILD);
	while (pChild)
	{
		if (pChild->m_hWnd != hHandleToSkip)
		{
			CRect aRect;
			pChild->GetWindowRect(&aRect);

			if (aRect.PtInRect(aPt))
			{
				// customized controls
				IWindowWrapper^ wrapper = GetControl((System::IntPtr) pChild->m_hWnd);

				if (wrapper != nullptr)
				{
					if (IWindowWrapperContainer::typeid->IsInstanceOfType(wrapper))
						((WindowWrapperContainer^)wrapper)->GetChildrenFromPos(screenPoint, handleToSkip, foundChildren);
					if (MBodyEdit::typeid->IsInstanceOfType(wrapper))
						((MBodyEdit^)wrapper)->GetChildrenFromPos(screenPoint, handleToSkip, foundChildren);
					foundChildren->Add(wrapper);
				}
			}
		}

		pChild = pChild->GetNextWindow(GW_HWNDNEXT);
	}
}

/// <summary>
/// Internal Use
/// </summary>
//----------------------------------------------------------------------------
IntPtr WindowWrapperContainer::GetChildFromCtrlID(UINT nID)
{
	CWnd* pWndEditingArea = GetWnd();
	if (pWndEditingArea == nullptr)
		return IntPtr::Zero;

	CWnd* pChild = pWndEditingArea->GetWindow(GW_CHILD);
	while (pChild)
	{
		if (pChild->GetDlgCtrlID() == nID)
			return (IntPtr) (int) pChild->m_hWnd;
			
		// customized controls
		IWindowWrapper^ wrapper = GetControl((System::IntPtr) pChild->m_hWnd);

		if (wrapper != nullptr)
		{
			IntPtr subChild = IntPtr::Zero;
			if (IWindowWrapperContainer::typeid->IsInstanceOfType(wrapper))
				subChild = ((WindowWrapperContainer^)wrapper)->GetChildFromCtrlID(nID);
		//	if (subChild == IntPtr::Zero && MBodyEdit::typeid->IsInstanceOfType(wrapper))
		//		subChild = ((MBodyEdit^)wrapper)->GetChildFromCtrlID(nID);

			if (subChild != IntPtr::Zero)
				return subChild;
		}

		pChild = pChild->GetNextWindow(GW_HWNDNEXT);
	}
	return IntPtr::Zero;
}

//----------------------------------------------------------------------------
void WindowWrapperContainer::SwitchVisibility(bool bVisible)
{
	//prima di tutto imposto il visible ai controlli customizzati così da poterli modificare
	for each (IComponent^ cmp in Components)
	{
		if (BaseWindowWrapper::typeid->IsInstanceOfType(cmp))
			((BaseWindowWrapper^)cmp)->SwitchVisibility(bVisible);
	}
}
//----------------------------------------------------------------------------
bool WindowWrapperContainer::CreateWrappers(array<System::IntPtr>^ handlesToSkip)
{
	HWND hEditingArea = GetHandle();
	HWND  hChild = GetWindow(hEditingArea, GW_CHILD);
	int index = 0;
	bool changedWrapper = false;

	while (hChild)
	{
		bool toSkip = false;
		for each(System::IntPtr h in handlesToSkip)
			if (hChild == (HWND)(int)h)
				toSkip = true;

		if (!toSkip)
		{
			// customized controls
			BaseWindowWrapper^ wrapper = (BaseWindowWrapper^)GetControl((System::IntPtr) hChild);

			if (wrapper == nullptr)
			{
				wrapper = (BaseWindowWrapper^)BaseWindowWrapper::Create((System::IntPtr) hChild);
				if (wrapper == nullptr)
				{
					hChild = GetNextWindow(hChild, GW_HWNDNEXT);
					continue;
				}

				changedWrapper = true;
				//wrapper->Parent = this; non serve: la fa già la Add
				Add(wrapper);
			}

			wrapper->CreateWrappers(handlesToSkip);
		}

		hChild = GetNextWindow(hChild, GW_HWNDNEXT);
	}
	return changedWrapper;
}

//-----------------------------------------------------------------------------
int WindowWrapperContainer::GetNamespaceType()
{
	return CTBNamespace::FORM;
}

//-----------------------------------------------------------------------------
IWindowWrapper^ WindowWrapperContainer::GetControl(System::String^ name)
{
	if (System::String::IsNullOrEmpty(name))
		return nullptr;

	for each (IWindowWrapper^ current in Components)
	{
		if (current->Name == name)
			return current;
	}
	return nullptr;
}
//-----------------------------------------------------------------------------
List<BaseWindowWrapper^>^ WindowWrapperContainer::GetChildrenByIdOrName(System::String^ id, System::String^ name)
{
	List<BaseWindowWrapper^>^ list = gcnew List<BaseWindowWrapper^>();
	if (System::String::IsNullOrEmpty(id) && System::String::IsNullOrEmpty(name))
		return list;

	for each (BaseWindowWrapper^ current in Components)
	{
		if ((!String::IsNullOrEmpty(id) && current->Id == id) ||
			(!String::IsNullOrEmpty(name) && current->Name->Equals(name, StringComparison::InvariantCultureIgnoreCase)))
			list->Add(current);
		if (WindowWrapperContainer::typeid->IsInstanceOfType(current))
		{
			list->AddRange(((WindowWrapperContainer^)current)->GetChildrenByIdOrName(id, name));
		}
	}
	return list;
}
//-----------------------------------------------------------------------------
IWindowWrapper^ WindowWrapperContainer::GetControl(System::IntPtr handle)
{
	if (handle == System::IntPtr::Zero)
		return nullptr;

	if (Handle == handle)
		return this;

	for each (IWindowWrapper^ current in Components)
	{
		if (current->Handle == handle)
			return current;

		if (IWindowWrapperContainer::typeid->IsInstanceOfType(current))
		{
			IWindowWrapper^ child = ((IWindowWrapperContainer^)current)->GetControl(handle);
			if (child != nullptr)
				return child;
		}
		if (MBodyEdit::typeid->IsInstanceOfType(current))
		{
			IWindowWrapper^ child = ((MBodyEdit^)current)->GetControl(handle);
			if (child != nullptr)
				return child;
		}
	}

	return nullptr;
}

//----------------------------------------------------------------------------
IWindowWrapper^	WindowWrapperContainer::GetControl(INameSpace^ nameSpace)
{
	if (System::String::Compare(Namespace->ToString(), nameSpace->ToString(), true) == 0)
		return this;

	for each (IWindowWrapper^ current in Components)
	{
		if (System::String::Compare(current->Namespace->ToString(), nameSpace->ToString(), true) == 0)
			return current;

		if (IWindowWrapperContainer::typeid->IsInstanceOfType(current))
		{
			IWindowWrapper^ child = ((IWindowWrapperContainer^)current)->GetControl(nameSpace);
			if (child != nullptr)
				return child;
		}
	}

	return nullptr;
}

//----------------------------------------------------------------------------
HWND WindowWrapperContainer::GetControlHandle(const CTBNamespace& aNamespace)
{
	if (!GetWnd())
		return NULL;

	CParsedForm* pParsedForm = GetParsedForm(GetWnd());
	if (!pParsedForm)
		return NULL;

	CWnd* pWnd = pParsedForm->GetWndLinkedCtrl(aNamespace);

	return pWnd ? pWnd->m_hWnd : NULL;
}

//-----------------------------------------------------------------------------
bool WindowWrapperContainer::HasControl(System::IntPtr handle)
{
	if (handle == System::IntPtr::Zero)
		return false;

	return GetControl(handle) != nullptr;
}

//-----------------------------------------------------------------------------
void WindowWrapperContainer::Add(IComponent^ component)
{
	Add(component, nullptr);
}

//-----------------------------------------------------------------------------
void WindowWrapperContainer::Add(System::ComponentModel::IComponent^ component, bool isChanged)
{
	if (component == nullptr)
		return;

	EasyBuilderComponent^ ebComp = dynamic_cast<EasyBuilderComponent^>(component);
	if (ebComp != nullptr)
		ebComp->IsChanged = isChanged;

	Add(component, nullptr);
}


//-----------------------------------------------------------------------------
void WindowWrapperContainer::Add(IComponent^ component, System::String^ name)
{
	if (this->components->Contains(component))
	{
		delete component;
		return;
	}

	EasyBuilderControl^ control = (EasyBuilderControl^)component;
	if (control)
		control->Parent = this;

	if (name != nullptr && component->Site != nullptr)
		component->Site->Name = name;

	components->Add(component);
	ITBComponentChangeService^ svc = nullptr;

	if (Site != nullptr)
		svc = (ITBComponentChangeService^)Site->GetService(ITBComponentChangeService::typeid);

	if (svc != nullptr)
		svc->OnComponentAdded(this, component);

	IWindowWrapperContainer^ ebContainer = dynamic_cast<IWindowWrapperContainer^>(component);
	if (ebContainer != nullptr && ebContainer->CanCallCreateComponents())
		ebContainer->CallCreateComponents();
	else
	{
		// controllo anche che non si tratti di un oggetto tipo il bodyedit
		IEasyBuilderContainer^ baseContainer = dynamic_cast<IEasyBuilderContainer^>(component);
		if (baseContainer != nullptr && baseContainer->CanCallCreateComponents())
			baseContainer->CallCreateComponents();
	}
}

//-----------------------------------------------------------------------------
void WindowWrapperContainer::Remove(IComponent^ component)
{
	if (component == nullptr)
		return;

	//Rimuove il component attuale dalla lista dei components del container
	components->Remove(component);

	ITBComponentChangeService^ svc = nullptr;

	if (Site != nullptr)
		svc = (ITBComponentChangeService^)Site->GetService(ITBComponentChangeService::typeid);

	if (svc != nullptr)
		svc->OnComponentRemoved(this, component);
}

//-----------------------------------------------------------------------------
void WindowWrapperContainer::RemoveAll()
{
	for each (IComponent^ var in Components)
	{
		Remove(var);
	}
}

//----------------------------------------------------------------------------
void WindowWrapperContainer::CallCreateComponents()
{
	SuspendLayout(this, gcnew EasyBuilderEventArgs());
	CreateComponents();
	ApplyResources();
	if (DesignModeType == EDesignMode::Runtime)
		OnAfterCreateComponents();

	// Set curretn DPI
	lastEditDPI = GetCurrentLogPixels();
	
	ResumeLayout(this, gcnew EasyBuilderEventArgs());
}

//----------------------------------------------------------------------------
bool WindowWrapperContainer::CanDropTarget(Type^ droppedObject)
{
	if (droppedObject->IsSubclassOf(MHotLink::typeid))
		return false;
	return !(
		(MTileDialog::typeid == droppedObject || droppedObject->IsSubclassOf(MTileDialog::typeid)) ||
		(MTilePanel::typeid == droppedObject || droppedObject->IsSubclassOf(MTilePanel::typeid)) ||
		(MTilePanelTab::typeid == droppedObject || droppedObject->IsSubclassOf(MTilePanelTab::typeid)) ||
		(MTileGroup::typeid == droppedObject || droppedObject->IsSubclassOf(MTileGroup::typeid))
		);
}

//----------------------------------------------------------------------------
void WindowWrapperContainer::ClearComponents()
{
	for (int i = Components->Count - 1; i >= 0; i--)
	{
		IComponent^ component = Components[i];
	//	Remove(component);
		delete component;
	}
	RemoveAll();

}

//-----------------------------------------------------------------------------
void WindowWrapperContainer::SaveCurrentStatus(IDesignerCurrentStatus^ status)
{
	for each (EasyBuilderControl^ control in Components)
		control->SaveCurrentStatus(status);
}

//-----------------------------------------------------------------------------
void WindowWrapperContainer::ApplyCurrentStatus(IDesignerCurrentStatus^ status)
{
	for each (EasyBuilderControl^ control in Components)
		control->ApplyCurrentStatus(status);
}

//-----------------------------------------------------------------------------
void WindowWrapperContainer::OnScrollChanged(WindowMessageEventArgs^ e)
{
	this->ScrollChanged(this, e);
}

//----------------------------------------------------------------------------
void WindowWrapperContainer::OnDataLoaded()
{
	this->DataLoaded(this, EasyBuilderEventArgs::Empty);
	SyncHotLinks();
}

//-------------------------------------------------------------------------------
void WindowWrapperContainer::GenerateJsonForChildren(CWndObjDescription* pParentDescription, List<System::Tuple<System::String^, System::String^>^>^ serialization)
{
	for each (IWindowWrapper^ wrapper in this->Components)
	{
		BaseWindowWrapper^ child = dynamic_cast<BaseWindowWrapper^>(wrapper);
		if (child != nullptr && child->Handle != IntPtr::Zero)
		{
			//skip always StaticArea
			if (child->Id->CompareTo(gcnew String(staticAreaID)) == 0 && child->Id->CompareTo(gcnew String(staticArea1ID)) == 0 && child->Id->CompareTo(gcnew String(staticArea2ID)) == 0)
				continue;

			child->GenerateJson(pParentDescription, serialization);
		}
	}
}

//-----------------------------------------------------------------------------
void WindowWrapperContainer::SyncHotLinks()
{
	for each (IComponent^ component in Components)
	{
		if (component->GetType()->IsSubclassOf(WindowWrapperContainer::typeid))
		{
			((WindowWrapperContainer^)component)->SyncHotLinks();
			continue;
		}

		if (!component->GetType()->IsSubclassOf(MParsedControl::typeid))
			continue;

		MParsedControl^ control = (MParsedControl^)component;
		if (control->HotLink != nullptr && control->DataBinding != nullptr && control->HotLink->ReadOnDataLoaded)
			control->HotLink->FindRecord((MDataObj^)control->DataBinding->Data);
	}
}

//----------------------------------------------------------------------------
void WindowWrapperContainer::OnBuildingSecurityTree(System::IntPtr tree, System::IntPtr infoTreeItems)
{
	for each (IComponent^ component in Components)
		if (component->GetType()->IsSubclassOf(BaseWindowWrapper::typeid))
			((BaseWindowWrapper^)component)->OnBuildingSecurityTree(tree, infoTreeItems);
}

//----------------------------------------------------------------------------
void WindowWrapperContainer::ToLogicalUnits(Point% px)
{
	//trasformo in coordinate logiche
	CPoint pt(px.X, px.Y);
	ReverseMapDialog((HWND)Handle.ToInt64(), pt);
	px = Point(pt.x, pt.y);
}

//----------------------------------------------------------------------------
void WindowWrapperContainer::ToLogicalUnits(System::Drawing::Size% px)
{
	//trasformo in coordinate logiche
	CSize sz(px.Width, px.Height);
	ReverseMapDialog((HWND)Handle.ToInt64(), sz);
	px = System::Drawing::Size(sz.cx, sz.cy);
}

//----------------------------------------------------------------------------
void WindowWrapperContainer::ToPixels(Point% lu)
{
	//trasformo in coordinate fisiche
	CPoint pt(lu.X, lu.Y);
	SafeMapDialog((HWND)Handle.ToInt64(), pt);
	lu = Point(pt.x, pt.y);
}

//----------------------------------------------------------------------------
void WindowWrapperContainer::ToPixels(System::Drawing::Size% lu)
{
	//trasformo in coordinate fisiche
	CSize sz(lu.Width, lu.Height);
	SafeMapDialog((HWND)Handle.ToInt64(), sz);
	lu = System::Drawing::Size(sz.cx, sz.cy);
}

//----------------------------------------------------------------------------
int	WindowWrapperContainer::GetCurrentLogPixels()
{
	return ::GetLogPixels();
}

//----------------------------------------------------------------------------
int	WindowWrapperContainer::LastEditDPI::get()
{
	return lastEditDPI;
}

//----------------------------------------------------------------------------
void WindowWrapperContainer::LastEditDPI::set(int value)
{
	lastEditDPI = value;
}