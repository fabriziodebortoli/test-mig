#include "StdAfx.h"
#include "MEasyBuilderContainer.h"
#include "MDocument.h"


using namespace Microarea::Framework::TBApplicationWrapper;

using namespace System;
using namespace System::Collections::Generic;
using namespace System::ComponentModel;

//-----------------------------------------------------------------------------
MEasyBuilderContainer::MEasyBuilderContainer(void)
{
	components = gcnew List<IComponent^>();
}

//-----------------------------------------------------------------------------
MEasyBuilderContainer::~MEasyBuilderContainer()
{
	this->!MEasyBuilderContainer();
	GC::SuppressFinalize(this);
}

//-----------------------------------------------------------------------------
MEasyBuilderContainer::!MEasyBuilderContainer()
{
	try
	{
		ClearComponents ();
	}
	catch(Exception^ ex)
	{
		System::Diagnostics::Debug::WriteLine(ex->ToString());
	}
}

//-----------------------------------------------------------------------------
void MEasyBuilderContainer::Add(IComponent^ component)
{
	if (component == nullptr)
		return;

	Add(component, nullptr);
}

//-----------------------------------------------------------------------------
void MEasyBuilderContainer::Add(System::ComponentModel::IComponent^ component, bool isChanged)
{
	if (component == nullptr)
		return;

	EasyBuilderComponent^ ebComp = dynamic_cast<EasyBuilderComponent^>(component);
	if (ebComp != nullptr)
		ebComp->IsChanged = isChanged;

	Add(component, nullptr);
}

//-----------------------------------------------------------------------------
void MEasyBuilderContainer::Add(IComponent^ component, System::String^ name)
{
	if (component == nullptr)
		return;

	if (this->components->Contains(component))
	{
		//delete component;
		return;
	}

	components->Add(component);

	if (component->GetType()->IsSubclassOf(EasyBuilderComponent::typeid) )
		((EasyBuilderComponent^) component)->ParentComponent = this;

	ITBComponentChangeService^ svc = nullptr; 
	
	if (Site != nullptr)
		svc = (ITBComponentChangeService^) Site->GetService(ITBComponentChangeService::typeid);
	
	if (svc != nullptr)
		svc->OnComponentAdded(this, component);

	//giro valido solo per le IEasyBuilderContainer, le view fanno il giro dall'altra parte
	IWindowWrapperContainer^ iWindowWrapperContainer = dynamic_cast<IWindowWrapperContainer^>(component);
	if (iWindowWrapperContainer != nullptr)
		return;
	
	IEasyBuilderContainer^ ebContainer = dynamic_cast<IEasyBuilderContainer^>(component);
	if (ebContainer != nullptr && ebContainer->CanCallCreateComponents())
		ebContainer->CallCreateComponents();
}

//-----------------------------------------------------------------------------
void MEasyBuilderContainer::Remove(IComponent^ component)
{
	if (component == nullptr)
		return;

	components->Remove(component);
}

//-----------------------------------------------------------------------------
void MEasyBuilderContainer::CreateComponents()
{
}

//-----------------------------------------------------------------------------
void MEasyBuilderContainer::ApplyResources ()
{
}

//-----------------------------------------------------------------------------
void MEasyBuilderContainer::ClearComponents ()
{
	for (int i = components->Count - 1; i >= 0; i--)
	{
		IComponent^ component = components[i];
		Remove(component);
		delete component;
	}
}

//-----------------------------------------------------------------------------
void MEasyBuilderContainer::CallCreateComponents()
{
	CreateComponents();
	ApplyResources();

	bool forceAfter = false;
	MDocument^ doc = dynamic_cast<MDocument^>(Document);
	if (doc != nullptr)
	{
		forceAfter = doc->WrapExistingObjectsInRunning;
	}
	if (forceAfter || DesignModeType == EDesignMode::Runtime)
		OnAfterCreateComponents();
}

//-----------------------------------------------------------------------------
ComponentCollection^ MEasyBuilderContainer::Components::get()
{
	return gcnew ComponentCollection(components->ToArray());
}

//-----------------------------------------------------------------------------
IComponent^ MEasyBuilderContainer::GetComponent (System::String^ controlName) 
{
	for each (IComponent^ current in components)
	{
		if (!EasyBuilderComponent::typeid->IsInstanceOfType(current))
			continue;
		if (System::String::Compare(controlName, ((EasyBuilderComponent^)current)->Name, StringComparison::InvariantCultureIgnoreCase) == 0)
			return current;
	}
	return nullptr;
}
		
//-----------------------------------------------------------------------------
bool MEasyBuilderContainer::HasComponent (System::String^ controlName) 
{
	return GetComponent(controlName) != nullptr;
}

//-----------------------------------------------------------------------------
void MEasyBuilderContainer::GetEasyBuilderComponents(List<Type^>^ requestedTypes, List<EasyBuilderComponent^>^ components)
{
	for each (IComponent^ component in Components)
	{
		if (!component->GetType()->IsSubclassOf(EasyBuilderComponent::typeid))
			continue;
		
		for each (Type^ requestedType in requestedTypes)
		{
			if (component->GetType() == requestedType || component->GetType()->IsSubclassOf(requestedType))
			{
				components->Add((EasyBuilderComponent^) component);
				break;
			}
		}

		if (component->GetType()->IsSubclassOf(MEasyBuilderContainer::typeid))
			((MEasyBuilderContainer^) component)->GetEasyBuilderComponents(requestedTypes, components);
	}
}
/* TODOBRUNA
//-----------------------------------------------------------------------------
void MEasyBuilderContainer::FireBehaviours (Object^ sender, EasyBuilderBehaviourEventArgs^ eventArg)
{
	__super::FireBehaviours(sender, eventArg);
	for each (EasyBuilderComponent^ component in Components)
		component->FireBehaviours(sender, eventArg);
}*/

//-----------------------------------------------------------------------------
bool MEasyBuilderContainer::IsReferenceableType::get()
{
	return true;
}