#include "StdAfx.h"

#include "Attributes.h"

using namespace System;
using namespace Microarea::Framework::TBApplicationWrapper;

/////////////////////////////////////////////////////////////////////////////
// 				struct TableAttribute Implementation
/////////////////////////////////////////////////////////////////////////////


//----------------------------------------------------------------------------	
TableAttribute::TableAttribute(String^ tableName)
{
	this->tableName = tableName;
}

//----------------------------------------------------------------------------	
String^ TableAttribute::TableName::get()
{
	return tableName;
}

//----------------------------------------------------------------------------	
void TableAttribute::TableName::set(String^ value)
{
	tableName = value;
}

//----------------------------------------------------------------------------	
String^ TableAttribute::GetTableName(Type^ recordType)
{
	array<Object^>^ customAttributes =
		recordType->GetCustomAttributes(TableAttribute::typeid, true);

	if (customAttributes == nullptr || customAttributes->Length == 0)
	{
		return String::Empty;
	}

	if (customAttributes->Length > 1)
	{
		throw gcnew InvalidOperationException("Too many TableAttribute found for " + recordType->Name);
	}

	return ((TableAttribute^)customAttributes[0])->TableName;
}


/////////////////////////////////////////////////////////////////////////////
// 				struct TBBindableAttribute Implementation
/////////////////////////////////////////////////////////////////////////////


//----------------------------------------------------------------------------	
TBBindableAttribute::TBBindableAttribute(bool bindable)
{
	this->bindable = bindable;
}

//----------------------------------------------------------------------------	
bool TBBindableAttribute::Bindable::get()
{
	return bindable;
}

//----------------------------------------------------------------------------	
void TBBindableAttribute::Bindable::set(bool value)
{
	bindable = value;
}

/////////////////////////////////////////////////////////////////////////////
// 				FormTypeAttribute Implementation
/////////////////////////////////////////////////////////////////////////////


//----------------------------------------------------------------------------	
FormTypeAttribute::FormTypeAttribute(Type^ formType)
{
	this->formType = formType;
}

//----------------------------------------------------------------------------	
Type^ FormTypeAttribute::FormType::get()
{
	return formType;
}

//----------------------------------------------------------------------------	
void FormTypeAttribute::FormType::set(Type^ value)
{
	formType = value;
}

/////////////////////////////////////////////////////////////////////////////
// 				ADMGateExposed Implementation
/////////////////////////////////////////////////////////////////////////////
//----------------------------------------------------------------------------	
ADMGateExposed::ADMGateExposed()
{
	this->exposed = true;
}

//----------------------------------------------------------------------------	
ADMGateExposed::ADMGateExposed(bool exposed)
{
	this->exposed = exposed;
}

//----------------------------------------------------------------------------	
bool ADMGateExposed::Exposed::get()
{
	return this->exposed;
}

//----------------------------------------------------------------------------	
void ADMGateExposed::Exposed::set(bool value)
{
	this->exposed = value;
}
