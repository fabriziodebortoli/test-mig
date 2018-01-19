#include "StdAfx.h"
#include "EasyBuilderComponents.h"

using namespace Microarea::Framework::TBApplicationWrapper;
using namespace System;
using namespace System::ComponentModel;

/////////////////////////////////////////////////////////////////////////////
// 						class EasyBuilderComponents Implementation
/////////////////////////////////////////////////////////////////////////////
//----------------------------------------------------------------------------
EasyBuilderComponents::EasyBuilderComponents (EasyBuilderComponent^ parent)
{
	this->parent = parent;

	this->canBeExtendedByUI = true;
	this->canBeReducedByUI	= true;
	this->canBeModifiedByUI = false;
}

//----------------------------------------------------------------------------
EasyBuilderComponent^ EasyBuilderComponents::Parent::get ()
{
	return parent;
}

//----------------------------------------------------------------------------
EasyBuilderComponent^ EasyBuilderComponents::CreateNewInstance ()
{
	return nullptr;
}

//----------------------------------------------------------------------------
bool EasyBuilderComponents::CanBeExtendedByUI::get ()
{
	return canBeExtendedByUI;
}

//----------------------------------------------------------------------------
void EasyBuilderComponents::CanBeExtendedByUI::set (bool value)
{
	canBeExtendedByUI = value;
}

//----------------------------------------------------------------------------
bool EasyBuilderComponents::CanBeReducedByUI::get ()
{
	return canBeReducedByUI;
}

//----------------------------------------------------------------------------
void EasyBuilderComponents::CanBeReducedByUI::set (bool value)
{
	canBeReducedByUI = value;
}

//-----------------------------------------------------------------------------
void EasyBuilderComponents::CanBeModifiedByUI::set(bool value)
{
	canBeModifiedByUI = value;
}

//----------------------------------------------------------------------------------
bool EasyBuilderComponents::CanBeModifiedByUI::get()
{
	return canBeModifiedByUI;
}

//----------------------------------------------------------------------------
void EasyBuilderComponents::ApplyChanges ()
{
}

//----------------------------------------------------------------------------
void EasyBuilderComponents::InitializeForUI ()
{
}

//----------------------------------------------------------------------------
bool EasyBuilderComponents::HasChanged()
{
	return false;
}

//----------------------------------------------------------------------------
bool EasyBuilderComponents::IsEditable()
{
	return true;
}

//----------------------------------------------------------------------------
EasyBuilderComponents^ EasyBuilderComponents::Clone()
{
	EasyBuilderComponents^ newCollection = gcnew EasyBuilderComponents((EasyBuilderComponent^)this->Parent);

	newCollection->CanBeExtendedByUI = CanBeExtendedByUI;
	newCollection->CanBeReducedByUI = CanBeReducedByUI;
	newCollection->CanBeModifiedByUI = CanBeModifiedByUI;
	newCollection->AddRange(this);

	return newCollection;
}

//----------------------------------------------------------------------------
ComponentCollection^ EasyBuilderComponents::OriginalCollection::get()
{
	return nullptr;
}

//----------------------------------------------------------------------------
String^	EasyBuilderComponents::GetEditableError()
{
	return String::Empty;
}
