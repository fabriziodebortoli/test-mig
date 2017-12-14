#include "StdAfx.h"

//siccome sono in mixed mode, SendMessage non è mappata su SendMessageW e allora mi da errore di linking sulla SendMessage di CBaseDocument
#define SendMessage SendMessageW 


#include <TbGenlib\LayoutContainer.h>
#include <TbGes\TileManager.h>
#include "WindowWrapperContainer.h"
#include "MHeaderStrip.h"
#include "MTabber.h"
#include "MTileGroup.h"
#include "MLayoutContainer.h"

using namespace System;
using namespace Microarea::TaskBuilderNet::Interfaces;
using namespace Microarea::TaskBuilderNet::Core::Generic;
using namespace Microarea::TaskBuilderNet::Interfaces::View;
using namespace Microarea:: Framework::TBApplicationWrapper;

//================================================================================
//								MLayoutComponent
//================================================================================
MLayoutComponent::MLayoutComponent(IntPtr objectPtr)
{
	HasCodeBehind = true;
	m_pLayoutElement = (LayoutElement*) objectPtr.ToInt64();
}

//----------------------------------------------------------------------------
IComponent^ MLayoutComponent::LinkedComponent::get()
{
	return linkedComponent;
}

//----------------------------------------------------------------------------
void MLayoutComponent::LinkedComponent::set(IComponent^ value)
{
	linkedComponent = value;

	BaseWindowWrapper^ wrapper = AsBaseWindowWrapper();
	if (wrapper != nullptr)
		Namespace = wrapper->Namespace;
}

//----------------------------------------------------------------------------
String^ MLayoutComponent::GetNameFrom(INameSpace^ nameSpace)
{
	if (String::IsNullOrEmpty(nameSpace->FullNameSpace))
		return String::Empty;

	if (!String::IsNullOrEmpty(nameSpace->Leaf))
		return nameSpace->Leaf;

	int nPos = nameSpace->ToString()->LastIndexOf(".");
	if (nPos <= 0)
		return nameSpace->ToString();

	return nameSpace->ToString()->Substring(nPos + 1);
}

//----------------------------------------------------------------------------
INameSpace^ MLayoutComponent::Namespace::get()
{
	return nameSpace;
}

//----------------------------------------------------------------------------
void MLayoutComponent::Namespace::set(INameSpace^ value)
{
	nameSpace = value;
	Name = GetNameFrom(nameSpace);
}

//----------------------------------------------------------------------------
String^ MLayoutComponent::TypeDescription(Type^ type)
{
	if (MHeaderStrip::typeid == type || type->IsSubclassOf(MHeaderStrip::typeid))
		return "HeaderStrip";

	if (MTileGroup::typeid == type || type->IsSubclassOf(MTileGroup::typeid))
		return "TileGroup";

	if (MTilePanel::typeid == type || type->IsSubclassOf(MTilePanel::typeid))
		return "Panel";

	if (MTileDialog::typeid == type || type->IsSubclassOf(MTileDialog::typeid))
		return "Tile";

	if (MTilePanelTab::typeid == type || type->IsSubclassOf(MTilePanelTab::typeid))
		return "Panel Tab";

	if (MTabber::typeid == type || type->IsSubclassOf(MTabber::typeid))
		return "TileManager";
	
	return String::Empty;
}

//----------------------------------------------------------------------------
System::String^ MLayoutComponent::LayoutDescription::get()
{
	String^ linkedComponentDescri = Name;
	BaseWindowWrapper^ wrapper = AsBaseWindowWrapper();
	if (wrapper != nullptr)
		linkedComponentDescri = String::Concat(TypeDescription(wrapper->GetType()), " ", wrapper->ControlLabel);
		
	return linkedComponentDescri;
}

//----------------------------------------------------------------------------
IntPtr	MLayoutComponent::GetPtr()
{
	return (IntPtr) m_pLayoutElement;
}

//----------------------------------------------------------------------------
bool MLayoutComponent::Equals(System::Object^ obj)
{
	if (obj == nullptr || !(obj->GetType()->IsSubclassOf(MLayoutComponent::typeid) || MLayoutComponent::typeid->IsInstanceOfType(obj)))
		return false;

	MLayoutComponent^ comp = (MLayoutComponent^)obj;
	return GetPtr() == comp->GetPtr();
}

//----------------------------------------------------------------------------
BaseWindowWrapper^ MLayoutComponent::AsBaseWindowWrapper()
{
	if (LinkedComponent != nullptr && BaseWindowWrapper::typeid->IsInstanceOfType(LinkedComponent))
		return  (BaseWindowWrapper^)LinkedComponent;

	return nullptr;
}


//----------------------------------------------------------------------------
LayoutElement* MLayoutComponent::GetLayoutElement()
{
	return m_pLayoutElement;
}

//----------------------------------------------------------------------------
const	LayoutElementArray*	MLayoutComponent::GetContainedElements()
{
	return m_pLayoutElement->GetContainedElements();
}

//----------------------------------------------------------------------------
MLayoutComponent^ MLayoutComponent::Create(LayoutElement* pElement)
{
	// se il component in realtà e' un container la istanzio già da subito
	bool isContainer = pElement->GetContainedElements() && pElement->GetContainedElements()->GetSize();
	if (isContainer)
	{
		CLayoutContainer* pAsContainer = dynamic_cast<CLayoutContainer*>(pElement->GetContainedElements()->GetAt(0)->GetParentElement());
		if (pAsContainer)
			return gcnew MLayoutContainer((IntPtr)pAsContainer);
	}

	return gcnew MLayoutComponent((IntPtr)pElement);
}

//================================================================================
//								MLayoutContainer								//
//================================================================================
MLayoutContainer::MLayoutContainer(IntPtr containerPtr)
	:
	MLayoutComponent(containerPtr)
{
	m_pLayoutContainer = (CLayoutContainer*)containerPtr.ToInt64();
	// inizializzo il namespace
	if (m_pLayoutContainer)
		Namespace = gcnew NameSpace(gcnew String(m_pLayoutContainer->GetElementNameSpace()));
}

//----------------------------------------------------------------------------
EContainerLayout MLayoutContainer::Layout::get()
{
	if (!m_pLayoutContainer)
		return EContainerLayout::Column;

	CLayoutContainer::LayoutType layout = m_pLayoutContainer->GetLayoutType();

	switch (layout)
	{
	case CLayoutContainer::HBOX:
		return EContainerLayout::Hbox;
	case CLayoutContainer::VBOX:
		return EContainerLayout::Vbox;
	case CLayoutContainer::STRIPE:
		return EContainerLayout::Stripe;
	default:
		return EContainerLayout::Column;
	}
}

//----------------------------------------------------------------------------
void MLayoutContainer::Layout::set(EContainerLayout layoutType)
{
	if (!m_pLayoutContainer)
		return;

	switch (layoutType)
	{
	case EContainerLayout::Column:
		m_pLayoutContainer->SetLayoutType(CLayoutContainer::COLUMN);
		break;
	case EContainerLayout::Hbox:
		m_pLayoutContainer->SetLayoutType(CLayoutContainer::HBOX);
		break;
	case EContainerLayout::Vbox:
		m_pLayoutContainer->SetLayoutType(CLayoutContainer::VBOX);
		break;
	case EContainerLayout::Stripe:
		m_pLayoutContainer->SetLayoutType(CLayoutContainer::STRIPE);
		break;
	default:
		break;
	}
}

//----------------------------------------------------------------------------
ELayoutAlign MLayoutContainer::LayoutAlign::get()
{
	if (!m_pLayoutContainer)
		return ELayoutAlign::NoAlign;

	CLayoutContainer::LayoutAlign layout = m_pLayoutContainer->GetLayoutAlign();

	switch (layout)
	{
	case CLayoutContainer::BEGIN:
		return ELayoutAlign::Begin;
	case CLayoutContainer::MIDDLE:
		return ELayoutAlign::Middle;
	case CLayoutContainer::END:
		return ELayoutAlign::End;
	case CLayoutContainer::STRETCHMAX:
		return ELayoutAlign::StretchMax;
	case CLayoutContainer::NO_ALIGN:
		return ELayoutAlign::NoAlign;
	default:
		return ELayoutAlign::Stretch;
	}
}

//----------------------------------------------------------------------------
void MLayoutContainer::LayoutAlign::set(ELayoutAlign layoutType)
{
	if (!m_pLayoutContainer)
		return;

	switch (layoutType)
	{

	case ELayoutAlign::Begin:
		m_pLayoutContainer->SetLayoutAlign(CLayoutContainer::BEGIN);
		break;
	case ELayoutAlign::Middle:
		m_pLayoutContainer->SetLayoutAlign(CLayoutContainer::MIDDLE);
		break;
	case ELayoutAlign::End:
		m_pLayoutContainer->SetLayoutAlign(CLayoutContainer::END);
		break;
	case ELayoutAlign::StretchMax:
		m_pLayoutContainer->SetLayoutAlign(CLayoutContainer::STRETCHMAX);
		break;
	case ELayoutAlign::NoAlign:
		m_pLayoutContainer->SetLayoutAlign(CLayoutContainer::NO_ALIGN);
		break;
	default:
	case ELayoutAlign::Stretch:
		m_pLayoutContainer->SetLayoutAlign(CLayoutContainer::STRETCH);
		break;
	}
}

//----------------------------------------------------------------------------
System::String^ MLayoutContainer::LayoutDescription::get()
{
	return String::Concat(__super::LayoutDescription, " (", Layout.ToString(), ", ", LayoutAlign.ToString(), ")");
}

//----------------------------------------------------------------------------
const LayoutElementArray*	MLayoutContainer::GetContainedElements()
{
	return m_pLayoutContainer->GetContainedElements();
}


//================================================================================
//								MLayoutObject								//
//================================================================================
//----------------------------------------------------------------------------
MLayoutObject::MLayoutObject(MLayoutComponent^ layoutObject)
{
	this->layoutObject = layoutObject;
	HasCodeBehind = layoutObject->HasCodeBehind;
}

//----------------------------------------------------------------------------
void MLayoutObject::OnAfterCreateComponents()
{
	if (layoutObject == nullptr || !layoutObject->GetLayoutElement())
		return;

	// se non faccio così la GetContainedElements() non mi punta alla classe giusta
	const LayoutElementArray* pElements = layoutObject->GetContainedElements();
	if (!pElements)
		return;

	for (int i = 0; i < pElements->GetCount(); i++)
	{
		LayoutElement* pElement = pElements->GetAt(i);
		if (!pElement)
			continue;


		INameSpace^ ns = gcnew NameSpace(gcnew String(pElement->GetElementNameSpace()));
		MLayoutObject^ component = gcnew MLayoutObject(MLayoutComponent::Create(pElement));
		
		if (!String::IsNullOrEmpty(ns->FullNameSpace) && (LinkedComponent == nullptr || IContainer::typeid->IsInstanceOfType(LinkedComponent)))
			component->LinkedComponent = FindLinkedComponent(ns, (IContainer^)LinkedComponent);
		if (component->LinkedComponent != nullptr)
		{
			WindowWrapperContainer^ linkedContainer = dynamic_cast<WindowWrapperContainer^>(component->LinkedComponent);
			if (linkedContainer != nullptr)
				linkedContainer->IntegrateLayout(component);
		}

		Add(component);
	}
}

//----------------------------------------------------------------------------
void MLayoutObject::RecursiveOnAfterCreateComponent()
{
	for each (IComponent^ component in Components)
	{
		if (MLayoutObject::typeid->IsInstanceOfType(component))
		{
			MLayoutObject ^ componentObject = (MLayoutObject^)component;
			componentObject->OnAfterCreateComponents();
			componentObject->RecursiveOnAfterCreateComponent();
		}
	}
}

//----------------------------------------------------------------------------
bool MLayoutObject::Equals(System::Object^ obj)
{
	if (obj == nullptr || !(obj->GetType()->IsSubclassOf(MLayoutObject::typeid) || MLayoutObject::typeid->IsInstanceOfType(obj)))
		return false;

	MLayoutObject^ comp = (MLayoutObject^) obj;
	return layoutObject->GetPtr() == comp->layoutObject->GetPtr();
}

//----------------------------------------------------------------------------
System::String^ MLayoutObject::LayoutDescription::get()
{
	return layoutObject->LayoutDescription;
}


//----------------------------------------------------------------------------
INameSpace^ MLayoutObject::Namespace::get()
{
	return layoutObject->Namespace;
}

//----------------------------------------------------------------------------
String^ MLayoutObject::Name::get()
{
	return layoutObject->Name;
}

//----------------------------------------------------------------------------
void MLayoutObject::Namespace::set(INameSpace^ value)
{
	layoutObject->Namespace = value;
}

//----------------------------------------------------------------------------
IComponent^ MLayoutObject::LinkedComponent::get()
{
	return layoutObject->LinkedComponent;
}

//----------------------------------------------------------------------------
void MLayoutObject::LinkedComponent::set(IComponent^ value)
{
	layoutObject->LinkedComponent = value;
}

//----------------------------------------------------------------------------
IComponent^ MLayoutObject::FindLinkedComponent(INameSpace^ ns, IContainer^ container)
{
	if (container == nullptr)
		container = GetParentWindow();

	if (container != nullptr && container->GetType()->IsSubclassOf(WindowWrapperContainer::typeid) || WindowWrapperContainer::typeid->IsInstanceOfType(container))
	{
		WindowWrapperContainer^ windowContainer = (WindowWrapperContainer^) container;
		return (IComponent^ ) windowContainer->GetControl(ns);
	}

	for each (IComponent^ component in container->Components)
	{
		if (component->GetType()->IsSubclassOf(BaseWindowWrapper::typeid) || BaseWindowWrapper::typeid->IsInstanceOfType(component))
		{
			BaseWindowWrapper^ wrapper = (BaseWindowWrapper^)component;
			if (String::Compare(wrapper->Namespace->FullNameSpace, ns->FullNameSpace) == 0)
				return wrapper;
		}
	}
	return nullptr;
}

//----------------------------------------------------------------------------
MLayoutObject^ MLayoutObject::FindLayoutObjectOn(INameSpace^ ns)
{
	if (String::Compare(this->Namespace->FullNameSpace, ns->FullNameSpace) == 0)
		return this;

	for each (IComponent^ component in Components)
	{
		if (!MLayoutObject::typeid->IsInstanceOfType(component))
			continue;

		MLayoutObject^ layoutObject = (MLayoutObject^) component;
		if (String::Compare(layoutObject->Namespace->FullNameSpace, ns->FullNameSpace) == 0)
			return layoutObject;
	}
	
	return nullptr;
}

//----------------------------------------------------------------------------
void MLayoutObject::RemoveLayoutObjectOn(INameSpace^ ns)
{
	if (String::Compare(this->Namespace->FullNameSpace, ns->FullNameSpace) == 0)
		return;
	
	for each (IComponent^ component in Components)
	{
		if (!MLayoutObject::typeid->IsInstanceOfType(component))
			continue;

		MLayoutObject^ layoutObject = (MLayoutObject^)component;
		if (String::Compare(layoutObject->Namespace->FullNameSpace, ns->FullNameSpace) == 0)
		{
			Remove(layoutObject);
			break;
		}
		
		// vado in ricorsione
		layoutObject->RemoveLayoutObjectOn(ns);
	}
}

//----------------------------------------------------------------------------
void MLayoutObject::AddContainer(CLayoutContainer* pContainer, IComponent^ linkedComponent)
{
	if (!pContainer)
		return;

	MLayoutObject^ container = gcnew MLayoutObject(gcnew MLayoutContainer((IntPtr)pContainer));
	container->LinkedComponent = linkedComponent;

	Add(container);
}

//----------------------------------------------------------------------------
void MLayoutObject::AddElement(LayoutElement* pElement, IComponent^ linkedComponent)
{
	if (!pElement)
		return;

	MLayoutObject^ element = gcnew MLayoutObject(gcnew MLayoutComponent((IntPtr)pElement));
	element->LinkedComponent = linkedComponent;

	Add(element);
}

//-----------------------------------------------------------------------------
bool MLayoutObject::CanCallCreateComponents()
{ 
	return false; 
}

//-----------------------------------------------------------------------------
void MLayoutObject::CallCreateComponents()
{
	components->Clear();
	// prima parte la generazione del ramo di oggetti
	OnAfterCreateComponents();
	RecursiveOnAfterCreateComponent();

	// quindi applico le differenze di customizzazione
	CreateComponents();
	ApplyResources();
}

//----------------------------------------------------------------------------
WindowWrapperContainer^ MLayoutObject::AsWindowWrapperContainer()
{
	if (LinkedComponent != nullptr && WindowWrapperContainer::typeid->IsInstanceOfType(LinkedComponent))
		return  (WindowWrapperContainer^)LinkedComponent;

	return nullptr;
}


//----------------------------------------------------------------------------
void MLayoutObject::Add(System::ComponentModel::IComponent^ component, System::String^ name)
{
	if (component == nullptr)
		return;

	MLayoutObject^ layoutObject = (MLayoutObject^)component;
	layoutObject->ParentComponent = this;
	// do il nome di default alla container ma questo codice va spostato nella Add
	if (layoutObject->LinkedComponent == nullptr && (layoutObject->Namespace == nullptr || String::IsNullOrEmpty(layoutObject->Namespace->FullNameSpace)))
	{
		layoutObject->Namespace = gcnew NameSpace(String::Concat(MLayoutContainer::defaultName, Components->Count.ToString()));
	}

	__super::Add(layoutObject, name);
}

//----------------------------------------------------------------------------
WindowWrapperContainer^ MLayoutObject::GetParentWindow()
{
	MLayoutObject^ layoutObject = this;
	while (layoutObject->ParentComponent != nullptr)
	{
		if (MLayoutObject::typeid != layoutObject->ParentComponent->GetType())
			continue;

		MLayoutObject^ parent = (MLayoutObject^)layoutObject->ParentComponent;
		if (parent->LinkedComponent != nullptr && parent->AsWindowWrapperContainer())
			return parent->AsWindowWrapperContainer();
		
		layoutObject = (MLayoutObject^) layoutObject->ParentComponent;
	}
	return nullptr;
}

//----------------------------------------------------------------------------
void MLayoutObject::LayoutChangedFor(INameSpace^ ns)
{
	// prima guardo se posso agire sul singolo oggetto
	MLayoutObject^ objectToChange = FindLayoutObjectOn(ns);
	// altrimenti rigenero l'intera catena
	if (objectToChange == nullptr)
		objectToChange = this;

	objectToChange->CallCreateComponents();
}
