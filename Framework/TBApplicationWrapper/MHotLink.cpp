#include "StdAfx.h"

#include <TbGeneric\ParametersSections.H>
#include <TbGeneric\ReferenceObjectsInfo.H>
#include <TbGenlib\TBCommandInterface.h>
#include <TbOleDb\sqlaccessor.h>
#include <TbOleDb\sqlrec.h>
#include <TbOleDb\wclause.h>
#include <TbWoormEngine\RepTable.h>
#include <TbGes\EXTDOC.H>

#include "MParsedControls.h"
#include "EBHotKeyLink.h"
#include "QueryController.h"
#include "MHotLink.h"
#include "MBodyedit.h"
#include "MDocument.h"

using namespace System;
using namespace Microarea::Framework::TBApplicationWrapper;
using namespace Microarea::TaskBuilderNet::Core::Generic;
using namespace Microarea::TaskBuilderNet::Core::EasyBuilder::Refactor;
using namespace ICSharpCode::NRefactory;
using namespace ICSharpCode::NRefactory::CSharp;


using namespace System::Runtime::InteropServices;
using namespace Microarea::TaskBuilderNet::Core::ComponentModel;
using namespace System::Collections::Generic;
using namespace System::ComponentModel::Design::Serialization;
using namespace System::ComponentModel;

typedef AstExpression ICSharpCode::NRefactory::CSharp::Expression;

#define szParam1  _T("EBP1")

//----------------------------------------------------------------------------	
bool SearchFieldConverter::CanConvertFrom(ITypeDescriptorContext^ context, Type^ sourceType)
{
    return (sourceType == System::String::typeid) || __super::CanConvertFrom(context, sourceType);
}

//----------------------------------------------------------------------------	
Object^ SearchFieldConverter::ConvertFrom(ITypeDescriptorContext^ context, System::Globalization::CultureInfo^ culture, Object^ value )
{
    System::String^ field = Convert::ToString(value);

	MHotLinkSearch^ search = (MHotLinkSearch^) context->Instance;
	if (search == nullptr)
		return nullptr;
	
	List<System::String^>^ fields = search->HotLink->GetCompatibleFieldNames(search->IsSearchByKey);

	for each (System::String^ fieldName in fields)
	{
		if (System::String::Compare(fieldName, field, StringComparison::InvariantCultureIgnoreCase) == 0)
			return fieldName;
	}
            
    return search->FieldName;
}

/////////////////////////////////////////////////////////////////////////////
// 				class MHotLinkParam Implementation
/////////////////////////////////////////////////////////////////////////////
//----------------------------------------------------------------------------	
MHotLinkParam::MHotLinkParam (System::String^ name, System::String^ description, MDataObj^ data)
{
	this->name = name;
	this->description = description;
	this->data = data;
}

//----------------------------------------------------------------------------
System::String^ MHotLinkParam::ToString()
{
	return Description;
}

/////////////////////////////////////////////////////////////////////////////
// 				class HotLinkSerializer Implementation
/////////////////////////////////////////////////////////////////////////////

//----------------------------------------------------------------------------	
Object^ HotLinkSerializer::Serialize(IDesignerSerializationManager^ manager, Object^ current)
{
	System::Collections::Generic::List<Statement^>^ newCollection = gcnew System::Collections::Generic::List<Statement^>();

	MHotLink^ hotLink = (MHotLink^) current;
	
	System::String^ className	= hotLink->SerializedType;
	System::String^ varName		= hotLink->SerializedName;

	//// comment
	//newCollection->Add(gcnew CodeCommentStatement(String::Empty));
	//newCollection->Add(gcnew CodeCommentStatement(String::Concat(" ", className)));
	//newCollection->Add(gcnew CodeCommentStatement(String::Empty));
	
	System::Collections::Generic::IList<Statement^>^ construction = SerializeConstruction(manager, hotLink, varName, className);
	//Se non serializza la creazione allora e` inutile continuare.
	if (construction == nullptr)
		return newCollection;

	newCollection->AddRange(construction);

	newCollection->Add
	(
		//this.Add(HKL...);
		AstFacilities::GetInvocationStatement	
		(
			gcnew ThisReferenceExpression(),
			EasyBuilderSerializer::AddMethodName,
			gcnew MemberReferenceExpression(gcnew ThisReferenceExpression(), varName),
			gcnew PrimitiveExpression(hotLink->IsChanged)
		)
	);

	// properties
	System::Collections::Generic::IList<Statement^>^ props = SerializeProperties(manager, hotLink, varName);
	if (props != nullptr)
		newCollection->AddRange(props);

	// events
	System::Collections::Generic::IList<Statement^>^ events = SerializeEvents(manager, hotLink, hotLink->SerializedName);
	if (events != nullptr)
		newCollection->AddRange(events);

	// parameter values
	System::Collections::Generic::IList<Statement^>^ paramValues = SerializeParametersValues(hotLink, varName);
	if (paramValues != nullptr)
		newCollection->AddRange(paramValues);
	return newCollection;
}

//----------------------------------------------------------------------------	
System::Collections::Generic::IList<Statement^>^ HotLinkSerializer::SerializeConstruction
	(
	IDesignerSerializationManager^ manager,
	MHotLink^ hotLink,
	System::String^ varName,
	System::String^ className
	)
{
	System::Collections::Generic::List<Statement^>^ newCollection = gcnew System::Collections::Generic::List<Statement^>();

	// construction
	IdentifierExpression^ varExpression	= gcnew IdentifierExpression(varName);
	newCollection->Add
		(
			AstFacilities::GetAssignmentStatement
			(
				varExpression,
				AstFacilities::GetObjectCreationExpression
				(
					className
				)
			)
		);
	SetExpression(manager, hotLink, varExpression, true);

	return newCollection;
}

//----------------------------------------------------------------------------	
TypeDeclaration^ HotLinkSerializer::SerializeClass (SyntaxTree^ syntaxTree, IComponent^ object)
{
	if (object == nullptr || (object->GetType() != MHotLink::typeid && !object->GetType()->IsSubclassOf(MHotLink::typeid)))
		return nullptr;

	NamespaceDeclaration^ ns = EasyBuilderSerializer::GetNamespaceDeclaration(syntaxTree);
	
	MHotLink^ hotlink = (MHotLink^) object; 
	
	String^ className = hotlink->SerializedType;
	RemoveClass(syntaxTree, className);

	TypeDeclaration^ aClass = gcnew TypeDeclaration();
	aClass->Modifiers = Modifiers::Public;
	aClass->Name = className;
	aClass->BaseTypes->Add(gcnew SimpleType(MHotLink::typeid->FullName));

	//private DocumentController controller;
	AttributeSection^ attr = AstFacilities::GetAttributeSection(PreserveFieldAttribute::typeid->FullName);

	// Costruttore 
	ConstructorDeclaration^ constr = gcnew ConstructorDeclaration();
	constr->Modifiers = Modifiers::Public;
	constr->Name = aClass->Name;
	aClass->Members->Add(constr);

	AstExpression^ parameterExpression = gcnew PrimitiveExpression(hotlink->Record->Name);

	if (hotlink->SourceNamespace != nullptr)
		parameterExpression = AstFacilities::GetObjectCreationExpression(
			NameSpace::typeid->FullName, gcnew PrimitiveExpression(hotlink->SourceNamespace->FullNameSpace)
			);

	constr->Initializer = AstFacilities::GetConstructorInitializer(
		parameterExpression,
		gcnew PrimitiveExpression(hotlink->Name),
		gcnew MemberReferenceExpression(
			gcnew IdentifierExpression(StaticControllerVariableName),
			EasyBuilderControlSerializer::DocumentPropertyName
			),
		gcnew PrimitiveExpression(hotlink->HasCodeBehind)
		);

	constr->Body = gcnew BlockStatement();

	// record
	SerializeRecord (syntaxTree, aClass, (MSqlRecord^) hotlink->Record);

	//TMyRecord CastToMyRecord(IRecord record) { return (TMyRecord) record; }
	MethodDeclaration^ castToMyRecordMethod = gcnew MethodDeclaration();
	castToMyRecordMethod->Name = CastToMyRecordMethodName;
	castToMyRecordMethod->Modifiers = Modifiers::Public | Modifiers::Virtual;
	castToMyRecordMethod->ReturnType = gcnew SimpleType (((MSqlRecord^)hotlink->Record)->SerializedType);
	castToMyRecordMethod->Parameters->Add (gcnew ParameterDeclaration(gcnew SimpleType(IRecord::typeid->FullName), RecordParamName, ParameterModifier::None));
	
	castToMyRecordMethod->Body = gcnew BlockStatement();
	castToMyRecordMethod->Body->Statements->Add
		(
			gcnew ReturnStatement
			(
				gcnew CastExpression
				(
					gcnew SimpleType(((MSqlRecord^)hotlink->Record)->SerializedType),
					gcnew IdentifierExpression (RecordParamName)
				)
			)
		);
	aClass->Members->Add(castToMyRecordMethod);

	for each (MHotLinkSearch^ search in hotlink->Searches)
		GenerateField(aClass, search);

	SerializeParameters (syntaxTree, aClass, hotlink);

	return aClass;
}

//----------------------------------------------------------------------------	
void HotLinkSerializer::SerializeParameters (SyntaxTree^ syntaxTree, TypeDeclaration^ hklClass, IDataManager^ dataManager)
{
	MHotLink^ hotLink = (MHotLink^) dataManager;
	if (hotLink->Parameters->Count == 0)
		return;

	for each (MHotLinkParam^ param in hotLink->Parameters)
	{
		//	[LocalizedCategory("ParametersCategory", EBCategories::typeid)]
		AttributeSection^ catAttr = AstFacilities::GetAttributeSection
			(
				AstFacilities::GetAttribute(
					Microarea::TaskBuilderNet::Core::Localization::LocalizedCategoryAttribute::typeid->FullName,
					gcnew PrimitiveExpression(EBCategories::ParametersCategory),
					gcnew TypeOfExpression(gcnew SimpleType(EBCategories::typeid->FullName))
					)
			);

		PropertyDeclaration^ currentParamProperty = gcnew PropertyDeclaration();
		currentParamProperty->Modifiers = Modifiers::Public;
		currentParamProperty->Name = String::Format(ParamPropertyName, param->Name);
		currentParamProperty->Modifiers = Modifiers::Public;
		currentParamProperty->ReturnType = gcnew SimpleType (MHotLinkParam::typeid->FullName);
		
		currentParamProperty->Attributes->Add (catAttr);
		currentParamProperty->Getter = gcnew Accessor();
		currentParamProperty->Getter->Body = gcnew BlockStatement();
		currentParamProperty->Getter->Body->Statements->Add
		(
			gcnew ReturnStatement
			(
				AstFacilities::GetInvocationExpression
				(
					gcnew ThisReferenceExpression(), 
					GetParamByNameMethod, 
					gcnew PrimitiveExpression(param->Name)
				)
			)
		);

		hklClass->Members->Add(currentParamProperty);
	}
}

//----------------------------------------------------------------------------	
System::Collections::Generic::IList<Statement^>^ HotLinkSerializer::SerializeParametersValues (IDataManager^ dataManager, String^ hklName)
{
	MHotLink^ hotLink = (MHotLink^) dataManager;
	if (hotLink->Parameters->Count == 0)
		return nullptr;

	DesignerSerializationManager^ mng = gcnew DesignerSerializationManager();
	IDisposable^ session = mng->CreateSession();

	System::Collections::Generic::List<Statement^>^ newCollection = gcnew System::Collections::Generic::List<Statement^>();
	for each (MHotLinkParam^ param in hotLink->Parameters)
	{
		if (param == nullptr || param->Data == nullptr || param->Value == nullptr)
			continue;

		newCollection->Add
		(
			AstFacilities::GetAssignmentStatement
			(
				gcnew MemberReferenceExpression
				(
					gcnew MemberReferenceExpression
					(
						gcnew IdentifierExpression(hklName),
						String::Format(ParamPropertyName, param->Name)
					),
					ValuePropertyName
				),
				gcnew CastExpression 
				(
					gcnew SimpleType(param->Value->GetType()->Name),
					gcnew PrimitiveExpression(param->Value)
				)
			)
		);		
	}
	return newCollection;
}

//-----------------------------------------------------------------------------
System::String^ HotLinkSerializer::CreateSerializedType(System::String^ hotLinkName)
{
	return System::String::Concat(HKL, EasyBuilderSerializer::Escape(hotLinkName));
}

/////////////////////////////////////////////////////////////////////////////
// 				class HotLinkSerializerForModuleController Implementation
/////////////////////////////////////////////////////////////////////////////

//----------------------------------------------------------------------------	
Object^ HotLinkSerializerForModuleController::Serialize(IDesignerSerializationManager^ manager, Object^ current)
{
	System::Collections::Generic::List<Statement^>^ newCollection = gcnew System::Collections::Generic::List<Statement^>();

	MHotLink^ hotLink = (MHotLink^) current;
	
	System::String^ className	= hotLink->SerializedType;
	System::String^ varName		= hotLink->SerializedName;

	//// comment
	//newCollection->Add(gcnew CodeCommentStatement(String::Empty));
	//newCollection->Add(gcnew CodeCommentStatement(String::Concat(" ", className)));
	//newCollection->Add(gcnew CodeCommentStatement(String::Empty));

	//HKLBalanceReclassifications _HKLBalanceReclassifications = null;
	VariableDeclarationStatement^ varStmt = gcnew VariableDeclarationStatement(
		gcnew SimpleType(className),
		varName,
		gcnew PrimitiveExpression(nullptr)
	);

	newCollection->Add(varStmt);
	IdentifierExpression^ variableDeclExpression = gcnew IdentifierExpression(varName);

	//_HKLBalanceReclassifications = new HKLBalanceReclassifications(controller);
	newCollection->Add
	(
		AstFacilities::GetAssignmentStatement
		(
			variableDeclExpression,
			AstFacilities::GetObjectCreationExpression
				(
					className
				)
		)
	);
	SetExpression(manager, hotLink, variableDeclExpression, true);

	// properties
	System::Collections::Generic::IList<Statement^>^ props = SerializeProperties(manager, hotLink, varName);
	if (props != nullptr)
		newCollection->AddRange(props);

	// events
	System::Collections::Generic::IList<Statement^>^ events = SerializeEvents(manager, hotLink, varName);
	if (events != nullptr)
		newCollection->AddRange(events);

	// parameter values
	System::Collections::Generic::IList<Statement^>^ paramValues = SerializeParametersValues(hotLink, varName);
	if (paramValues != nullptr)
		newCollection->AddRange(paramValues);
	return newCollection;
}

//----------------------------------------------------------------------------	
AstExpression^ HotLinkSerializerForModuleController::GetEventHandlerOwner()
{
	return gcnew ThisReferenceExpression();
}

//----------------------------------------------------------------------------	
System::String^ HotLinkSerializerForModuleController::GetOwnerController()
{
	return gcnew System::String(EasyBuilderSerializer::ModuleControllerClassName);
}

//----------------------------------------------------------------------------	
AssignmentExpression^ HotLinkSerializerForModuleController::GenerateCodeAttachEventStatement(
			String^ varName,
            Microarea::TaskBuilderNet::Core::EasyBuilder::EventInfo^ changedEvent,
            AstExpression^ handlerExpression
            )
{
	return gcnew AssignmentExpression(
					gcnew MemberReferenceExpression(gcnew IdentifierExpression(varName), changedEvent->EventName),
					handlerExpression
					);
}

/////////////////////////////////////////////////////////////////////////////
// 				class HotLinkSerializerForSharedHotLinks Implementation
/////////////////////////////////////////////////////////////////////////////

//----------------------------------------------------------------------------	
Object^ HotLinkSerializerForSharedHotLinks::Serialize(IDesignerSerializationManager^ manager, Object^ current)
{
	System::Collections::Generic::List<Statement^>^ newCollection = gcnew System::Collections::Generic::List<Statement^>();

	MHotLink^ hotLink = (MHotLink^) current;
	
	System::String^ className	= hotLink->SerializedType;
	System::String^ varName		= hotLink->SerializedName;

	//// comment
	//newCollection->Add(gcnew CodeCommentStatement(String::Empty));
	//newCollection->Add(gcnew CodeCommentStatement(String::Concat(" ", className)));
	//newCollection->Add(gcnew CodeCommentStatement(String::Empty));
	
	System::Collections::Generic::IList<Statement^>^ construction = SerializeConstruction(manager, hotLink, varName, className);
	//Se non serializza la creazione allora e` inutile continuare.
	if (construction == nullptr)
		return newCollection;

	newCollection->AddRange(construction);

	newCollection->Add
	(
		//this.Add(HKL...);
		AstFacilities::GetInvocationStatement
		(
			gcnew ThisReferenceExpression(),
			EasyBuilderSerializer::AddMethodName,
			gcnew MemberReferenceExpression(gcnew ThisReferenceExpression(), varName),
			gcnew PrimitiveExpression(hotLink->IsChanged)
		)
	);

	// properties
	System::Collections::Generic::IList<Statement^>^ props = SerializeProperties(manager, hotLink, varName);
	if (props != nullptr)
		newCollection->AddRange(props);

	// events
	System::Collections::Generic::IList<Statement^>^ events = SerializeEvents(manager, hotLink, hotLink->SerializedName);
	if (events != nullptr)
		newCollection->AddRange(events);

	return newCollection;
}

//----------------------------------------------------------------------------	
System::Collections::Generic::IList<Statement^>^ HotLinkSerializerForSharedHotLinks::SerializeConstruction
	(
	IDesignerSerializationManager^ manager,
	MHotLink^ hotLink,
	System::String^ varName,
	System::String^ className
	)
{
	//_HKLBanks = Module.NewApplication1.NewModule1.ModuleController.Factory.CreteHKLBanks(controller);
	System::String^ hklClassFullName = hotLink->GetType()->FullName;
	System::String^ moduleDllNamespace = hklClassFullName->Substring(0, hklClassFullName->LastIndexOf('.'));
	System::String^ moduleControllerFullName = System::String::Concat(moduleDllNamespace, ".ModuleController");

	System::Collections::Generic::List<Statement^>^ newCollection = gcnew System::Collections::Generic::List<Statement^>();

	// construction
	IdentifierExpression^ varExpression	= gcnew IdentifierExpression(varName);

	IdentifierExpression^ moduleControllerReferenceExpression = gcnew IdentifierExpression(moduleControllerFullName);
	MemberReferenceExpression^ factoryPropertyReferenceExpression = gcnew MemberReferenceExpression(moduleControllerReferenceExpression, "Factory");
	InvocationExpression^ createInvokeExpression = AstFacilities::GetInvocationExpression
		(
		factoryPropertyReferenceExpression,
		EasyBuilderSerializer::GetFactoryMethodNameFromClassName(className)
		);
	newCollection->Add(AstFacilities::GetAssignmentStatement(varExpression, createInvokeExpression));

	SetExpression(manager, hotLink, varExpression, true);

	return newCollection;
}

//----------------------------------------------------------------------------	
TypeDeclaration^ HotLinkSerializerForSharedHotLinks::SerializeClass(SyntaxTree^ syntaxTree, IComponent^ object)
{
	return nullptr;
}

/////////////////////////////////////////////////////////////////////////////
// 				class HotLinkSearchSerializer Implementation
/////////////////////////////////////////////////////////////////////////////

//----------------------------------------------------------------------------	
Object^ HotLinkSearchSerializer::Serialize(IDesignerSerializationManager^ manager, Object^ current)
{
	MHotLinkSearch^ search = (MHotLinkSearch^) current;
	if (search->HasCodeBehind)
		nullptr;

	System::Collections::Generic::List<Statement^>^ newCollection = gcnew System::Collections::Generic::List<Statement^>();

	String^ varName = search->SerializedName;
	IdentifierExpression^ variableDeclExpression = gcnew IdentifierExpression(varName);
	if (search->Site != nullptr)
		search->Site->Name = varName;
	newCollection->Add
	(
		AstFacilities::GetAssignmentStatement
		(
			variableDeclExpression,
			AstFacilities::GetObjectCreationExpression
			(
				gcnew SimpleType(MHotLinkSearch::typeid->FullName), 
				gcnew ThisReferenceExpression(), 
				gcnew PrimitiveExpression(search->HasCodeBehind)
			)
		)
	);
	SetExpression(manager, search, variableDeclExpression, true);
		
	System::Collections::Generic::IList<Statement^>^ props = SerializeProperties(manager, search, varName);
	if (props != nullptr)
		newCollection->AddRange(props);

	newCollection->Add
	(
		AstFacilities::GetInvocationStatement		//Add(src...);
		(
			gcnew ThisReferenceExpression(),
			EasyBuilderSerializer::AddMethodName,
			gcnew IdentifierExpression(varName),
			gcnew PrimitiveExpression(search->IsChanged)
		)
	);

	return newCollection;
}


/*//-----------------------------------------------------------------------------
bool SearchComboItem::Equals(System::Object^ obj)
{
	return System::String::Compare(Key, (String^)obj, StringComparison::InvariantCultureIgnoreCase) == 0;
}

//-----------------------------------------------------------------------------
int SearchComboItem::GetHashCode()
{
	return __super::GetHashCode();
}
*/

/////////////////////////////////////////////////////////////////////////////
// 				class MHotLinkSearch Implementation
/////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
MHotLinkSearch::MHotLinkSearch (MHotLink^ hotLink, bool hasCodeBehind)
{
	showInContextMenu = false;
	this->HasCodeBehind = hasCodeBehind;
	this->hotLink = hotLink;
}

//-----------------------------------------------------------------------------
System::String^ MHotLinkSearch::FieldName::get ()
{
	return fieldName;
}

//-----------------------------------------------------------------------------
void MHotLinkSearch::FieldName::set (System::String^ value)
{
	fieldName = value;
}

//-----------------------------------------------------------------------------
void MHotLinkSearch::AssociatedButton::set (MHotLinkSearch::ButtonAssociation value)
{
	associatedButton = value;
}

//-----------------------------------------------------------------------------
MHotLinkSearch::ButtonAssociation MHotLinkSearch::AssociatedButton::get ()
{
	return associatedButton;
}

//-----------------------------------------------------------------------------
void MHotLinkSearch::UseInComboBox::set (bool value)
{
	useInComboBox = value;
}

//-----------------------------------------------------------------------------
bool MHotLinkSearch::UseInComboBox::get ()
{
	return useInComboBox;
}

//-----------------------------------------------------------------------------
void MHotLinkSearch::ShowInContextMenu::set (bool value)
{
	showInContextMenu = value;
}

//-----------------------------------------------------------------------------
bool MHotLinkSearch::ShowInContextMenu::get ()
{
	return showInContextMenu;
}

//-----------------------------------------------------------------------------
bool MHotLinkSearch::CanChangeProperty	(System::String^ propertyName)
{
	return hotLink != nullptr && hotLink->IsDynamic;
}

//-----------------------------------------------------------------------------
System::String^ MHotLinkSearch::SerializedName::get () 
{ 
	return System::String::Concat(hotLink->SerializedType, "_", EasyBuilderSerializer::Escape(Name));
}

/////////////////////////////////////////////////////////////////////////////
// 				class MHotLinkSearches Implementation
/////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
MHotLinkSearches::MHotLinkSearches (EasyBuilderComponent^ parent)
	:
	EasyBuilderComponents(parent)
{
}

//-----------------------------------------------------------------------------
void MHotLinkSearches::Initialize ()
{
	MHotLink^ hotLink  = (MHotLink^) Parent;
	HotKeyLink* pHotLink = hotLink->GetHotLink();
	if (!pHotLink)
		return;

	MHotLinkSearch^ searchByKey = gcnew MHotLinkSearch(hotLink, hotLink->HasCodeBehind || !hotLink->IsDynamic);
	searchByKey->Name = MHotLinkSearch::SearchByKeyName;
	searchByKey->AddChangedProperty("Name");
	searchByKey->Description = gcnew System::String(cwsprintf(_TB("Search By Key")));
	searchByKey->AddChangedProperty("Description");

	::DataObj* pDataObj = pHotLink->GetDataObj();
	if (pDataObj)
		searchByKey->FieldName = gcnew System::String (pHotLink->GetAttachedRecord()->GetColumnName(pDataObj));
	else if (hotLink->IsDynamic)
	{
		IRecordField^ lastKeyField = (IRecordField^) hotLink->Record->PrimaryKeyFields[hotLink->Record->PrimaryKeyFields->Count-1];
		if (lastKeyField != nullptr)
			searchByKey->FieldName = lastKeyField->Name;
	}

	if (!System::String::IsNullOrEmpty(searchByKey->FieldName))
		searchByKey->AddChangedProperty("FieldName");

	searchByKey->AssociatedButton = MHotLinkSearch::ButtonAssociation::UpperButton;
	searchByKey->AddChangedProperty("AssociatedButton");
	if (hotLink->IsDynamic)
		searchByKey->UseInComboBox = true;
	else
	{
		CHotlinkDescription* pDescri = pHotLink->GetHotlinkDescription();
		searchByKey->UseInComboBox = pDescri ? pDescri->HasComboBox() == TRUE : false;
	}
	searchByKey->AddChangedProperty("UseInComboBox");

	hotLink->Add (searchByKey);
	if (searchByKey->Site != nullptr)
		PropertyChangingNotifier::OnComponentPropertyChanged(searchByKey->Site, searchByKey, "Description", System::String::Empty, ((MHotLinkSearch^)searchByKey)->Description);

	MHotLinkSearch^ search = gcnew MHotLinkSearch(hotLink, hotLink->HasCodeBehind || !hotLink->IsDynamic);
	search->Name = MHotLinkSearch::SearchByDescriptionName;
	search->AddChangedProperty("Name");

	search->Description = gcnew System::String(cwsprintf(_TB("Search By Description")));
	search->AddChangedProperty("Description");

	search->FieldName = gcnew System::String(pHotLink->GetDescriptionField());
	search->AddChangedProperty("FieldName");

	search->AssociatedButton = MHotLinkSearch::ButtonAssociation::LowerButton;
	search->AddChangedProperty("AssociatedButton");

	hotLink->Add (search);
	if (search->Site != nullptr)
		PropertyChangingNotifier::OnComponentPropertyChanged(search->Site, search, "Description", System::String::Empty, ((MHotLinkSearch^)search)->Description);
}

//----------------------------------------------------------------------------
EasyBuilderComponent^ MHotLinkSearches::CreateNewInstance ()
{
	MHotLink^ hotLink = (MHotLink^) this->Parent;

	MHotLinkSearch^ search = gcnew MHotLinkSearch(hotLink, false);
	search->Name = "New";
	search->AddChangedProperty("Name");
	search->ShowInContextMenu = true;
	search->AddChangedProperty("ShowInContextMenu");

	search->Site = (ITBSite^) ((ITBSite^) hotLink->Site)->Clone();
	search->Site->Name = search->SerializedName;
	
	return search;
}

//----------------------------------------------------------------------------
void MHotLinkSearches::ApplyChanges ()
{
	MHotLink^ hotLink = (MHotLink^) this->Parent;
	for each (IComponent^ search in originalCollection)
		hotLink->Remove(search);

	for each (IComponent^ search in this)
	{
		hotLink->Add(search);
		PropertyChangingNotifier::OnComponentAdded(hotLink, search, false);
		PropertyChangingNotifier::OnComponentPropertyChanged(hotLink->Site, search, "Description", nullptr, ((MHotLinkSearch^)search)->Description);
	}
}

//----------------------------------------------------------------------------
bool MHotLinkSearches::HasChanged()
{
	MHotLink^ hotLink = (MHotLink^) this->Parent;
	
	if (hotLink->Searches->Count != this->Count)
		return true;

	int i=0;
	for each (MHotLinkSearch^ search in this)
	{
		MHotLinkSearch^ original = (MHotLinkSearch^) originalCollection[i];
		if	(
				System::String::Compare(search->Name, original->Name, true) != 0 ||
				search->ChangedPropertiesCount > 0
			)
			return true;
		i++;
	}

	return false;
}

//----------------------------------------------------------------------------
bool MHotLinkSearches::IsEditable()
{
	return !Parent->HasCodeBehind;
}

//----------------------------------------------------------------------------
EasyBuilderComponents^ MHotLinkSearches::Clone()
{
	originalCollection = gcnew MHotLinkSearches((EasyBuilderComponent^)this->Parent);
	originalCollection->AddRange(this);
	return originalCollection;
}

//-----------------------------------------------------------------------------
MHotLinkSearch^ MHotLinkSearches::ByKey::get ()
{
	return Count > 0 ? (MHotLinkSearch^) default::get(MHotLinkSearch::SearchByKeyName) : nullptr;
}

//-----------------------------------------------------------------------------
MHotLinkSearch^ MHotLinkSearches::ByDescription::get ()
{
	return Count > 1 ? (MHotLinkSearch^) this[MHotLinkSearch::SearchByDescriptionName] : nullptr;
}

//-----------------------------------------------------------------------------
MHotLinkSearch^ MHotLinkSearches::UpperButton::get ()
{
	for each (MHotLinkSearch^ search in this)
		if (search->AssociatedButton == MHotLinkSearch::ButtonAssociation::UpperButton)
			return search;
	
	return nullptr;
}

//-----------------------------------------------------------------------------
MHotLinkSearch^ MHotLinkSearches::LowerButton::get ()
{
	for each (MHotLinkSearch^ search in this)
		if (search->AssociatedButton == MHotLinkSearch::ButtonAssociation::LowerButton)
			return search;

	return nullptr;
}

//-----------------------------------------------------------------------------
MHotLinkSearch^ MHotLinkSearches::ComboBox::get ()
{
	for each (MHotLinkSearch^ search in this)
		if (search->UseInComboBox)
			return search;

	return nullptr;
}

//-----------------------------------------------------------------------------
MHotLinkSearch^ MHotLinkSearches::default::get (System::String^ name)
{
	for each (MHotLinkSearch^ search in this)
		if (search->Name == name)
			return search;

	return nullptr;
}

//-----------------------------------------------------------------------------
MHotLinkSearch^ MHotLinkSearches::ContextMenu (int nMenuNr)
{
	int i=0;
	for each (MHotLinkSearch^ search in this)
		if (search->ShowInContextMenu)
		{
			i++;	
			if (i == nMenuNr)
				return search;
		}
		

	return nullptr;
}

//-----------------------------------------------------------------------------
bool MHotLinkSearches::HaveSearchesInContextMenu::get ()
{
	for each (MHotLinkSearch^ search in this)
		if (search->ShowInContextMenu)
			return true;
	
	return false;
}

//-----------------------------------------------------------------------------
void MHotLinkSearches::AfterCreateComponents ()
{
	MHotLink^ hotLink = (MHotLink^) this->Parent;

	hotLink->GetHotLink()->m_arContextMenuSearches.RemoveAll();
	for each (MHotLinkSearch^ search in this)
	{
		if (search->ShowInContextMenu)
			hotLink->GetHotLink()->m_arContextMenuSearches.Add(CString(System::String::IsNullOrEmpty(search->Description) ? search->Name : search->Description));
	}

	if (hotLink->IsDynamic)
		((EBHotKeyLink*)hotLink->GetHotLink())->InitializeXmlDescription();
}

/////////////////////////////////////////////////////////////////////////////
// 				class HotLinkEventArgs Implementation
/////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
HotLinkEventArgs::HotLinkEventArgs(IRecord^ record)
{ 
	this->record = record; 
}

//-----------------------------------------------------------------------------
IRecord^ HotLinkEventArgs::Record::get ()
{
	return record;
}

//-----------------------------------------------------------------------------
void HotLinkEventArgs::Record::set (IRecord^ record)
{
	this->record = record;
}

//-----------------------------------------------------------------------------
bool HotLinkEventArgs::Cancel::get ()
{
	return cancel;
}

//-----------------------------------------------------------------------------
void HotLinkEventArgs::Cancel::set (bool returnValue)
{
	this->cancel = returnValue;
}

/////////////////////////////////////////////////////////////////////////////
// 				class HotLinkComboEventArgs Implementation
/////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
HotLinkComboEventArgs::HotLinkComboEventArgs(IRecord^ record)
	:
	HotLinkEventArgs(record)
{ 
}

//-----------------------------------------------------------------------------
System::String^ HotLinkComboEventArgs::Result::get ()
{
	return result;
}

//-----------------------------------------------------------------------------
void HotLinkComboEventArgs::Result::set (System::String^ result)
{
	this->result = result;
}

/////////////////////////////////////////////////////////////////////////////
// 				class MHotLinkTable Implementation
/////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
class CHotLinkRecordDataFinder : public CRecordDataFinder
{
protected:
	SqlRecord* m_pTemplateRecord;

public:
	//-----------------------------------------------------------------------------
	CHotLinkRecordDataFinder (SqlTable* pSqlTable, SqlRecord* pTemplateRecord)
	{
		if (pSqlTable->GetRecord()->IsKindOf(pTemplateRecord->GetRuntimeClass()))
			m_pTemplateRecord = pTemplateRecord;
	}

	// per ottimizzare vado direttamente a farmi ritornare il SqlRecordItem
	// che tiene tutte le informazioni: dataobj, name, type e allocsize
	//-----------------------------------------------------------------------------
	virtual SqlRecordItem* GetRecordItem (SqlRecord* pRecord, ::DataObj* pDataObj) override
	{
		BOOL bSameRecord = m_pTemplateRecord && pRecord->GetSize() == m_pTemplateRecord->GetSize();

		SqlRecordItem* pItem;
		SqlRecordItem* pTemplateItem;
		int nSize =  pRecord->GetSizeEx() ;

		for (int i=0; i < nSize; i++)
		{
			pItem = pRecord->GetAt(i);
			if (pItem && pItem->GetDataObj() == pDataObj)
				return pItem;

			if (bSameRecord)
			{
				pTemplateItem = m_pTemplateRecord->GetAt(i);
				if (pTemplateItem && pTemplateItem->GetDataObj() == pDataObj)
					return pItem;
			}
		}

		return NULL;
	}
};


//-----------------------------------------------------------------------------
MHotLinkTable::MHotLinkTable(System::IntPtr tablePtr, MSqlRecord^ templateRecord)  
	: 
	MSqlTable (tablePtr)
{
	DataFinder = new CHotLinkRecordDataFinder(GetSqlTable(),  templateRecord->GetSqlRecord());
}

/////////////////////////////////////////////////////////////////////////////
// 				class MHotLink Implementation
/////////////////////////////////////////////////////////////////////////////

//-----------------------------------------------------------------------------
MHotLink::MHotLink(HotKeyLink* hotKeyLink)
	: 
		m_pHotKeyLink(hotKeyLink)
{
	Init();
	HasCodeBehind = true;
	isDynamic = false;
	sourceNamespace = gcnew NameSpace(gcnew System::String(m_pHotKeyLink->GetNamespace().ToString()));
	AfterConstruction();
}

//-----------------------------------------------------------------------------
MHotLink::MHotLink (System::String^ tableName, NameSpace^ hklNamespace)
{
	HasCodeBehind = false;
	isDynamic = true;

	Init();
	
	m_pHotKeyLink = new EBHotKeyLink (tableName, this);
	CTBNamespace aNs(hklNamespace);
	m_pHotKeyLink->SetNamespace(aNs);
	m_pHotKeyLink->SetName(CString(hklNamespace->Leaf));

	AfterConstruction();
}

//-----------------------------------------------------------------------------
MHotLink::MHotLink (System::String^ tableName, System::String^ hklName, IDocumentDataManager^ document, bool hasCodeBehind)
{
	HasCodeBehind = hasCodeBehind;
	isDynamic = !hasCodeBehind;

	Init();
	MDocument^ doc = (MDocument^) document;
	if (hasCodeBehind)
	{
		if (doc != nullptr)
		{
			MHotLink^ mHotLink = doc->GetHotLink(hklName);
			if (mHotLink == nullptr)
			{
				ASSERT(FALSE);
				Diagnostic->SetError(gcnew System::String(cwsprintf(_TB("Hotlink with the name %s does not exists into the document!"), CString(hklName))));
			}
			else
				m_pHotKeyLink = mHotLink->GetHotLink();
		}
	}
	else
	{
		m_pHotKeyLink = new EBHotKeyLink (tableName, this);
		CString sName(hklName);
		m_pHotKeyLink->SetName(sName);

		if (doc != nullptr)
		{
			CTBNamespace aNs(CTBNamespace::LIBRARY, doc->GetDocument()->GetNamespace().Left(CTBNamespace::LIBRARY));
			aNs.AutoCompleteNamespace(CTBNamespace::HOTLINK, sName, aNs);
			m_pHotKeyLink->SetNamespace(aNs);

			//Passo il nome dell'hotlink all'Attach, sebbene lo abbia appena impostato subito prima di questo if,
			//perchè il codice dell'Attach reimposta il nome e, se non le viene passato come parametro, lo mette a stringa vuota.
			//Il nome dell'hotlink, infatti, è il secondo parametro del metodo Attach che ha stringa vuota come valore di default.
			doc->GetDocument()->Attach(m_pHotKeyLink, sName);
		}
	}
	AfterConstruction();
	InitializeParameters ();
}

//-----------------------------------------------------------------------------
MHotLink::MHotLink (NameSpace^ hlkNamespace, System::String^ hklName, IDocumentDataManager^ document, bool hasCodeBehind)
{
	HasCodeBehind = hasCodeBehind;
	isDynamic = false;

	Init();
	MDocument^ doc = (MDocument^) document;
	if (hasCodeBehind)
	{
		if (doc != nullptr)
		{
			m_pHotKeyLink = doc->GetDocument()->GetHotLink(CString(hklName));
			if (!m_pHotKeyLink)
			{
				ASSERT(FALSE);
				Diagnostic->SetError(gcnew System::String(cwsprintf(_TB("Hotlink with the name %s does not exists into the document!"), CString(hklName))));
			}
		}
	}
	else
	{
		m_pHotKeyLink = (HotKeyLink *)AfxGetTbCmdManager()->RunHotlink(CTBNamespace(hlkNamespace->FullNameSpace));
		m_pHotKeyLink->SetName(CString(hklName));
		if (doc != nullptr)
		{
			//Passo il nome dell'hotlink all'Attach, sebbene lo abbia appena impostato subito prima di questo if,
			//perchè il codice dell'Attach reimposta il nome e, se non le viene passato come parametro, lo mette a stringa vuota.
			//Il nome dell'hotlink, infatti, è il secondo parametro del metodo Attach che ha stringa vuota come valore di default.
			doc->GetDocument()->Attach(m_pHotKeyLink, CString(hklName));
		}
	}
	
	if (m_pHotKeyLink)
		sourceNamespace = gcnew NameSpace(gcnew System::String(m_pHotKeyLink->GetNamespace().ToString()));
	
	AfterConstruction();
	InitializeParameters ();
}

//-----------------------------------------------------------------------------
void MHotLink::AfterConstruction ()
{
	if (!m_pHotKeyLink)
		return;
	
	SqlRecord* pRecord = m_pHotKeyLink->GetAttachedRecord();
	
	// potrebber non avere record
	if (!pRecord)
		return;

	if (!HasCodeBehind)
	{
		DataMustExist = true;
		AddChangedProperty("DataMustExist");
	}
	
	AttachDefaultEvents();
	if (m_pHotKeyLink->GetDataObj() && m_pHotKeyLink->GetDataObj()->GetDataType().IsNumeric())
		m_pHotKeyLink->SetSkipEmptyDataObj(TRUE);
}

//-----------------------------------------------------------------------------
MHotLink::~MHotLink ()
{
	this->!MHotLink();
	GC::SuppressFinalize(this);
}

//-----------------------------------------------------------------------------
MHotLink::!MHotLink ()
{
	if (!HasCodeBehind && m_pHotKeyLink)
	{
		CAbstractFormDoc* pDoc = (CAbstractFormDoc*) m_pHotKeyLink->GetAttachedDocument();
		if (pDoc)
		{
			pDoc->RemoveHotLink(m_pHotKeyLink);
			m_pHotKeyLink = NULL;
		}
		else
		{
			//Gli hotlink di modulo non sono attach-ati ad alcun documento quando
			//sono istanziati per essere visualizzati nel moduleObjectsTree:
			//pDoc qui può essere nullo quando l'istanza di MHotLink in questione
			//è una di quelle appiccicate ad un nodo del suddetto treeview.
			SAFE_DELETE(m_pHotKeyLink);
		}
	}
}

//-----------------------------------------------------------------------------
void MHotLink::InitializeParameters ()
{
	if (!m_pHotKeyLink)
		return;
	
	CHotlinkDescription* pDescription = m_pHotKeyLink->GetHotlinkDescription();
	if (!pDescription)
		return;

	for (int i=0; i <= pDescription->GetParameters().GetUpperBound(); i++)
	{
		CDataObjDescription* pParamDescri = (CDataObjDescription*) pDescription->GetParameters().GetAt(i);
		if (!pParamDescri)
			continue;

		MHotLinkParam^ param = gcnew MHotLinkParam
			(
				gcnew System::String(pParamDescri->GetName()),
				gcnew System::String(pParamDescri->GetTitle()),
				MDataObj::Create(pParamDescri->GetValue())
			);
		parameters->Add(param);
	}

	if (!HasCodeBehind)
		m_pHotKeyLink->SetParamValueByName(TRUE);
}

//-----------------------------------------------------------------------------
void MHotLink::OnCallLink ()
{
	this->CallLink (this, nullptr);
}

//-----------------------------------------------------------------------------
System::String^ MHotLink::ToString()
{
	return ReflectionUtils::GetComponentFullPath(this); 
}
//-----------------------------------------------------------------------------
void MHotLink::Init()
{
	parameters = gcnew List<IMHotLinkParam^>();
	readOnDataLoad = isDynamic;
	lastWhereClauseTable = nullptr;
	searches = gcnew MHotLinkSearches(this);
}

//-----------------------------------------------------------------------------
MHotLinkSearches^ MHotLink::Searches::get ()
{
	return searches;
}

//-----------------------------------------------------------------------------
void MHotLink::Searches::set (MHotLinkSearches^ value)
{
	searches = value;
}

//-----------------------------------------------------------------------------
System::String^ MHotLink::DBFieldName::get ()
{
	return searches->ByKey->FieldName;
}

//-----------------------------------------------------------------------------
System::String^ MHotLink::ReturnType::get ()
{
	IDataType^ dataType = GetCodeDataObj()->DataTypeIDataObj;
	return dataType->ToString();
}

//-----------------------------------------------------------------------------
bool MHotLink::Enabled::get()
{
	return m_pHotKeyLink ? m_pHotKeyLink->IsHotLinkEnabled() == TRUE : false;
}

//-----------------------------------------------------------------------------
void MHotLink::Enabled::set(bool value)
{
	if (m_pHotKeyLink)
		m_pHotKeyLink->EnableHotLink(value == true);
}

//-----------------------------------------------------------------------------
bool MHotLink::Published::get()
{
	return m_bIsPublished;
}

//-----------------------------------------------------------------------------
void MHotLink::Published::set(bool value)
{
	m_bIsPublished = value;
}


//-----------------------------------------------------------------------------
System::String^ MHotLink::Name::get()
{
	return m_pHotKeyLink ? gcnew System::String(m_pHotKeyLink->GetName()) : System::String::Empty;
}

//-----------------------------------------------------------------------------
System::String^ MHotLink::Description::get()
{
	if (m_sHotlinkDescr)
		return m_sHotlinkDescr;
	return Namespace != nullptr ? Namespace->ToString() : gcnew System::String("Hotlink Description To Be Customized" + DateTime::Now);
}
//-----------------------------------------------------------------------------
void MHotLink::Description::set(System::String^ value)
{
	m_sHotlinkDescr = value;
}

//-----------------------------------------------------------------------------
INameSpace^ MHotLink::Namespace::get()
{
	return m_pHotKeyLink ? gcnew NameSpace(gcnew System::String(m_pHotKeyLink->GetNamespace().ToString())) : nullptr;
}

//-----------------------------------------------------------------------------
String^ MHotLink::PublicationNamespace::get()
{
	return String::IsNullOrEmpty(publicationNamespace) ? Namespace->FullNameSpace : publicationNamespace;
}

//-----------------------------------------------------------------------------
void MHotLink::PublicationNamespace::set(String^ ns)
{
	publicationNamespace = ns;
}

//-----------------------------------------------------------------------------
System::IntPtr	MHotLink::GetRecordPtr ()
{
	if (m_pHotKeyLink)
		return (System::IntPtr) m_pHotKeyLink->GetAttachedRecord(); 
	
	return System::IntPtr::Zero;
}

//-----------------------------------------------------------------------------
IRecord^ MHotLink::Record::get()
{
	if (record == nullptr)
	{
		MSqlRecord^ rec = m_pHotKeyLink ? CreateRecord(m_pHotKeyLink->GetAttachedRecord()) : nullptr;
		Add(rec);
	}

	return record;
}

//-----------------------------------------------------------------------------
System::String^ MHotLink::SerializedType::get ()
{ 
	return HotLinkSerializer::CreateSerializedType(Name);
}

//-----------------------------------------------------------------------------
MSqlRecord^	MHotLink::CreateRecord (SqlRecord* pRecord)
{
	MSqlRecord^ mRecord = nullptr;

	// il record appena nato può essere tipizzato o meno, ma deve entrare 
	// nella catena di parentele per poter navigare bene la propria struttura
	if (record == nullptr)
		mRecord = gcnew MSqlRecord(pRecord);
	else
		mRecord = (MSqlRecord^)Activator::CreateInstance(record->GetType(), (System::IntPtr) pRecord);

	mRecord->ParentComponent = this;

	mRecord->CallCreateComponents();

	//mRecord->UnmanagedObjectDisposing += gcnew EventHandler<EventArgs^>(this, &MDBTObject::OnUnmanagedRecordDisposing);
	return mRecord;
}

//-----------------------------------------------------------------------------
void MHotLink::Add(IComponent^ component, System::String^ name)
{
	if (component == nullptr)
		return;

	if (component->GetType()->IsSubclassOf(MSqlRecord::typeid)  || component->GetType() == MSqlRecord::typeid)
		record = (MSqlRecord^) component;

	if (component->GetType() == MHotLinkSearch::typeid)
	{
		MHotLinkSearch^ search = (MHotLinkSearch^) component;
		MHotLinkSearch^ existingSearch = Searches[search->Name];
		// already added
		if (existingSearch)
		{
			Searches->Remove(existingSearch);
			__super::Remove(existingSearch);
		}
		
		Searches->Add(search);
		PropertyChangingNotifier::OnComponentAdded(this, search, false);
	}

	__super::Add(component, name);
}

//-----------------------------------------------------------------------------
BOOL MHotLink::IsString(::DataObj* pDataObj)
{
	return pDataObj->GetDataType() == DATA_STR_TYPE || pDataObj->GetDataType() == DATA_TXT_TYPE;
}

//-----------------------------------------------------------------------------
void MHotLink::OnDefineQuery (HotKeyLink::SelectionType nQuerySelection)
{
	SqlTable* pTable = GetTable();
	
	pTable->SelectAll();

	if (nQuerySelection == HotKeyLink::CUSTOM_ACCESS)
		goto end;

	MHotLinkSearch^ search = nullptr;
	::DataObj* pDataObj;

	// il direct access è differente dalle altre
	if (nQuerySelection == HotKeyLink::DIRECT_ACCESS)
	{
		search = Searches->ByKey;
		if (search == nullptr)
			goto end;
		
		pDataObj =  m_pHotKeyLink->GetAttachedRecord()->GetDataObjFromColumnName(search->FieldName);
		pTable->AddFilterColumn(*pDataObj);
		pTable->AddParam(szParam1, *pDataObj);
		goto end;
	}

	// le altre ricerche alternative
	switch (nQuerySelection)
	{
		case HotKeyLink::UPPER_BUTTON:	search = Searches->UpperButton;					break;
		case HotKeyLink::LOWER_BUTTON:	search = Searches->LowerButton;					break;
		case HotKeyLink::COMBO_ACCESS:	search = Searches->ComboBox;					break;
	}

	if (search != nullptr && !System::String::IsNullOrEmpty(search->FieldName))
	{
		pDataObj =  m_pHotKeyLink->GetAttachedRecord()->GetDataObjFromColumnName(search->FieldName);
		if (IsString(pDataObj))
			pTable->AddFilterLike(*pDataObj);
		else
			pTable->AddFilterColumn(*pDataObj);
		
		pTable->AddParam(szParam1, *pDataObj);
		pTable->AddSortColumn(*pDataObj);
	}

end:
	HotLinkSelectionType newType = GetSelectionType(nQuerySelection);
	MHotLinkTable^ mTable = gcnew MHotLinkTable((System::IntPtr) pTable, (MSqlRecord^)Record);
	DefineQuery(mTable, newType);
}

//-----------------------------------------------------------------------------
void MHotLink::OnDefineSearchQuery (MSqlTable^ mTable, int nQuerySelection)
{ 
	if ((HotKeyLink::SelectionType) nQuerySelection != HotKeyLink::CUSTOM_ACCESS)
		return __super::DefineQuery(mTable);

	MHotLinkSearch^ search = (customSearch == nullptr) ? Searches->ContextMenu(m_pHotKeyLink->GetCustomSearch()) : customSearch ;
	SqlTable* pTable = mTable->GetSqlTable();
		
	if (search != nullptr && !System::String::IsNullOrEmpty(search->FieldName))
	{
		::DataObj* pDataObj =  m_pHotKeyLink->GetAttachedRecord()->GetDataObjFromColumnName(search->FieldName);
		if (IsString(pDataObj) )
			pTable->AddFilterLike(*pDataObj);
		else
			pTable->AddFilterColumn(*pDataObj);
		pTable->AddParam(szParam1, *pDataObj);
		pTable->AddSortColumn(*pDataObj);
	}

	__super::DefineQuery(mTable);
}

//-----------------------------------------------------------------------------
void MHotLink::OnPrepareQuery (::DataObj* aDataObj, HotKeyLink::SelectionType nQuerySelection)
{
	SqlTable* pTable = GetTable();

	if (nQuerySelection == HotKeyLink::CUSTOM_ACCESS)
	{
		goto end;
		return;
	}

	if (!pTable->ExistParam(szParam1))
	{
		goto end;
		return;
	}

	if (	nQuerySelection == HotKeyLink::DIRECT_ACCESS || 
			!IsString(aDataObj)
		)
		pTable->SetParamValue(szParam1, *aDataObj);
	else
		pTable->SetParamLike(szParam1, *aDataObj);

end:
	HotLinkSelectionType newType = GetSelectionType(nQuerySelection);
	MHotLinkTable^ mTable = gcnew MHotLinkTable((System::IntPtr) pTable, (MSqlRecord^)Record);
	MDataObj^ dataObj = MDataObj::Create(aDataObj);
	PrepareQuery(dataObj, mTable, newType);
}

//-----------------------------------------------------------------------------
System::String^ MHotLink::LinkedDocumentNamespace::get ()
{
	return m_pHotKeyLink ? gcnew System::String(m_pHotKeyLink->GetAddOnFlyNamespace()) : System::String::Empty;
}

//-----------------------------------------------------------------------------
void MHotLink::LinkedDocumentNamespace::set (System::String^ value)
{
	if (m_pHotKeyLink)
	{
		if (System::String::IsNullOrEmpty(value))
			m_pHotKeyLink->ClearAddOnFlyNamespace();
		else
			m_pHotKeyLink->SetAddOnFlyNamespace(CString(value));
	}
}

//-----------------------------------------------------------------------------
bool MHotLink::CanOpenLinkedDocument::get ()
{
	if (LinkedDocumentNamespace == nullptr)
		return false;

	return m_pHotKeyLink ? m_pHotKeyLink->IsDoCallLinkDisable() == FALSE : false;
}

//-----------------------------------------------------------------------------
void MHotLink::CanOpenLinkedDocument::set (bool value)
{
	if (LinkedDocumentNamespace == nullptr)
		return;

	if (m_pHotKeyLink)
		m_pHotKeyLink->SetDoCallLinkDisable(value == FALSE);
}

//-----------------------------------------------------------------------------
bool MHotLink::CanAddOnFly::get ()
{
	if (String::IsNullOrEmpty(LinkedDocumentNamespace))
		return false;

	return m_pHotKeyLink ? m_pHotKeyLink->IsEnabledAddOnFly() == TRUE : false;
}

//-----------------------------------------------------------------------------
void MHotLink::CanAddOnFly::set (bool value)
{
	if (LinkedDocumentNamespace == nullptr)
		return;

	if (m_pHotKeyLink)
		m_pHotKeyLink->EnableAddOnFly(value == TRUE);
}

//-----------------------------------------------------------------------------
bool MHotLink::CanSearch::get ()
{
	return m_pHotKeyLink ? m_pHotKeyLink->CanDoSearchOnLink() == TRUE : false;
}

//-----------------------------------------------------------------------------
void MHotLink::CanSearch::set (bool value)
{
	if (m_pHotKeyLink)
		m_pHotKeyLink->EnableSearchOnLink(value == TRUE);
}
//-----------------------------------------------------------------------------
MParsedControl^ MHotLink::AttachedControl::get() 
{
	return attachedControl;
}

//-----------------------------------------------------------------------------
void MHotLink::AttachedControl::set (MParsedControl^ value)
{
	if (attachedControl != nullptr)
	{
		RemoveReferencedBy(attachedControl->SerializedName);
		RestoreDataUpperCase();
	}
	
	attachedControl = value;
	
	if (attachedControl != nullptr)
	{
		AddReferencedBy(attachedControl->SerializedName);
		SetDataUpperCase();
	}
}

//-----------------------------------------------------------------------------
bool MHotLink::DataMustExist::get ()
{
	return m_pHotKeyLink ? m_pHotKeyLink->IsMustExistData() == TRUE : false;
}

//-----------------------------------------------------------------------------
void MHotLink::DataMustExist::set (bool value)
{
	if (m_pHotKeyLink)
		m_pHotKeyLink->MustExistData(value == TRUE);
}
//----------------------------------------------------------------------------
bool MHotLink::CanBeAttached(Microarea::TaskBuilderNet::Core::CoreTypes::DataType dataType, int maxLength, System::String^% error)
{
	CString strError;
	if (isDynamic)
	{
		if (Searches->ByKey != nullptr && System::String::IsNullOrEmpty(Searches->ByKey->FieldName))
		{
			if (!strError.IsEmpty())
				strError.Append(_T("\r\n"));
			strError.Append(_TB("Missing property CodeField"));
		}

		if (Searches->ByDescription != nullptr && System::String::IsNullOrEmpty(Searches->ByDescription->FieldName))
		{
			if (!strError.IsEmpty())
				strError.Append(_T("\r\n"));
			strError.Append(_TB("Missing property DescriptionField"));
		}
	}


	MDataObj^ codeDataObj = GetCodeDataObj();
	// l'hotlink simulato non mi da parametri di controllo
	if (codeDataObj == nullptr && m_pHotKeyLink->IsKindOf(RUNTIME_CLASS(SimulatedHotKeyLink)))
		return TRUE;

	if (codeDataObj == nullptr ||
		codeDataObj->DataType != dataType || 
		(codeDataObj->DataType == Microarea::TaskBuilderNet::Core::CoreTypes::DataType::String && ((MDataStr^)codeDataObj)->MaxLength != maxLength))
	{
		if (!strError.IsEmpty())
			strError.Append(_T("\r\n"));
		strError.Append(_TB("CodeField data type is non compatible with the DataBinding one"));
	}

	// se sono su un bodyedit il reattach che avviene sul cambio tab e' a prescindere
	if (AttachedControl != nullptr && AttachedControl->ParentComponent != nullptr)
	{
		MBodyEditColumn^ column = dynamic_cast<MBodyEditColumn^>(AttachedControl->ParentComponent);
		if (column != nullptr)
			return TRUE;
	}

	if (AttachedControl != nullptr)
	{
		if (!strError.IsEmpty())
			strError.Append(_T("\r\n"));
		strError.Append(cwsprintf(_TB("Hotlink already attached to control {0-%s}"), CString(AttachedControl->Namespace->FullNameSpace)));
	}

	// gli hotlink propri che sono già attacch-ati sono false di ufficio
	if (HasCodeBehind && GetHotLink()->GetOwnerCtrl() != NULL)
	{
		if (!strError.IsEmpty())
			strError.Append(_T("\r\n"));
		strError.Append(cwsprintf(_TB("Hotlink already attached to an ERP control {0-%s}"), CString(GetHotLink()->GetOwnerCtrl()->GetNamespace().ToUnparsedString())));
	}

	if (!strError.IsEmpty())
	{
		error = (gcnew System::String(cwsprintf(_TB("The hotlink is not valid for this control:\r\n{0-%s}"), strError)));
		return FALSE;
	}
	return TRUE;
}
//-----------------------------------------------------------------------------
bool MHotLink::FindRecord (MDataObj^ mData)
{
	if (isDynamic && System::String::IsNullOrEmpty(Searches->ByKey->FieldName))
		return false;

	return m_pHotKeyLink && mData->GetDataObj() ? m_pHotKeyLink->DoFindRecord(mData->GetDataObj()) == TRUE : false;
}

//-------------------------------------------------------------------------------
MDataObj^ MHotLink::GetCodeDataObj ()
{
	if (Record == nullptr)
		return nullptr;
	if (isDynamic)
		return Searches->ByKey == nullptr ? nullptr : System::String::IsNullOrEmpty(Searches->ByKey->FieldName) ? nullptr : (MDataObj^) Record->GetData(Searches->ByKey->FieldName);

	return (MDataObj^) ((MSqlRecord^)Record)->GetData(m_pHotKeyLink->GetDataObj());
}

//----------------------------------------------------------------------------
MHotLinkParam^	MHotLink::GetParamByName (System::String^ name)
{
	RenameChangeRequest^ request = gcnew RenameChangeRequest(Refactor::ChangeSubject::HotLinkParameter, Namespace, this, name);
	String^ newName = BaseCustomizationContext::ApplicationChanges->GetNewNameOf(request);
	delete request;
	for each (MHotLinkParam^ param in parameters)
	{
		if (param->Name == name || param->Name == newName)
			return param;
	}

	System::String^ message = gcnew System::String
	(
		"This customization is not compatibile with new hotlink parameter : " + name + System::Environment::NewLine +
		"HotLink Type : " + GetType()->ToString() + System::Environment::NewLine +
		"Parent Object Name : " + Namespace->ToString() + System::Environment::NewLine +
		"Document : " + Document->Namespace->ToString() + System::Environment::NewLine
	);
	throw gcnew ApplicationException(message);

	return nullptr;
}

//----------------------------------------------------------------------------
bool MHotLink::CanChangeProperty (System::String^ propertyName) 
{
	if (propertyName == "Published")
		return this->isDynamic && parameters->Count == 0;

	if (propertyName == "CanAddOnFly" || propertyName == "CanOpenLinkedDocument")
		return LinkedDocumentNamespace != nullptr;

	if (propertyName == "PublicationNamespace")
		return false;

	return isDynamic;
}

//----------------------------------------------------------------------------
System::Collections::Generic::IList<IMHotLinkParam^>^ MHotLink::Parameters::get () 
{
	return parameters;
}

//----------------------------------------------------------------------------
SqlTable* MHotLink::GetTable()
{
	return m_pHotKeyLink ? m_pHotKeyLink->GetSqlTable() : NULL;
}

//----------------------------------------------------------------------------
void MHotLink::PrepareFilterQuery (MSqlTable^ mSqlTable, int nQuerySelection)
{
	SqlTable* pTable = mSqlTable->GetSqlTable();

	if (pTable && nQuerySelection == HotKeyLink::CUSTOM_ACCESS)
	{
		::DataObj* pDataObj = m_pHotKeyLink->GetAttachedData();
		if (!IsString(pDataObj))
			pTable->SetParamValue(szParam1, *pDataObj);
		else
			pTable->SetParamLike(szParam1,*pDataObj);
	}

	// non posso utilizzare la super::PreparingQuery perchè le condizioni di applicazione
	// della filterQuery sono un po' differenti rispetto al GenericDataManager
	this->OnQuerying ();

	if (pTable != NULL && !System::String::IsNullOrEmpty(FilterQuery) && (nQuerySelection != HotKeyLink::DIRECT_ACCESS || useFilterQueryInDirectAccess))
	{
		if (!pTable->IsOpen())
			pTable->Open();

		// okkio che il C++ in alcuni casi usa una nuova tabella nuova di pacca
		// aggiunti e quindi no si può fare altro che buttare via e rifare la query,
		// invece la DIRECT_ACCESS usa la stessa tabella dove i parametri ci sono già
		if (lastWhereClauseTable != nullptr && mSqlTable->GetSqlTable() != lastWhereClauseTable->GetSqlTable())
			SAFE_DELETE(whereClause);

		BOOL bAppend = FALSE;
		CString strError;
		
		if (!whereClause)
		{
			bAppend = TRUE;
			whereClause = CreateValidWhereClause(mSqlTable->GetSqlTable(), filterQuery, strError);
			lastWhereClauseTable = mSqlTable;
		}

		if (whereClause)
			whereClause->PrepareQuery(bAppend);
		else
			Diagnostic->SetError(gcnew System::String(strError));
	}

	PreparingQuery(this, gcnew DataManagerEventArgs(mSqlTable));
}

//----------------------------------------------------------------------------
bool MHotLink::ReadOnDataLoaded::get () 
{
	return readOnDataLoad;
}

//----------------------------------------------------------------------------
void MHotLink::ReadOnDataLoaded::set (bool value) 
{
	readOnDataLoad = value;
}

//----------------------------------------------------------------------------
void MHotLink::RestoreDataUpperCase ()
{
	if (attachedControl->DataBinding == nullptr)
		return;

	MDataObj^ mDataObj = (MDataObj^) attachedControl->DataBinding->Data;

	if (mDataObj == nullptr)
		return;
	
	::DataObj* pDataObj = mDataObj->GetDataObj();
	
	if (IsString(pDataObj))
	{
		pDataObj->SetUpperCase(wasDataUpperCase == true);
		// lo devo togliere anche dal control
		if (!wasDataUpperCase)
		{
			CParsedCtrl* pCtrl = ::GetParsedCtrl(attachedControl->GetWnd());
			if (pCtrl)
				pCtrl->SetCtrlStyle(pCtrl->GetCtrlStyle() & ~STR_STYLE_UPPERCASE);
		}
	}
}

//----------------------------------------------------------------------------
void MHotLink::SetDataUpperCase ()
{
	if (attachedControl->DataBinding == nullptr)
		return;

	MDataObj^ mDataObj = (MDataObj^) attachedControl->DataBinding->Data;

	if (mDataObj == nullptr)
		return;
	
	::DataObj* pDataObj = mDataObj->GetDataObj();
	
	if (IsString(pDataObj))
	{
		wasDataUpperCase = pDataObj->IsUpperCase() == TRUE;
		pDataObj->SetUpperCase(TRUE);
		// devo anche avvisare il control che nel frattempo è già nato senza lo stile
		CParsedCtrl* pCtrl = ::GetParsedCtrl(attachedControl->GetWnd());
		if (pCtrl)
			pCtrl->SetCtrlStyle(pCtrl->GetCtrlStyle() | STR_STYLE_UPPERCASE);
	}
}

//-----------------------------------------------------------------------------
List<System::String^>^	MHotLink::GetCompatibleFieldNames (bool exactMatch)
{
	List<System::String^>^ list = gcnew List<System::String^>();
	for each (MSqlRecordItem^ field in Record->Fields)
	{
		if (!exactMatch || AttachedControl == nullptr)
		{
			list->Add(field->Name);
			continue;
		}

		if ((Microarea::TaskBuilderNet::Core::CoreTypes::DataType) AttachedControl->CompatibleType != (Microarea::TaskBuilderNet::Core::CoreTypes::DataType)field->DataObjType)
			continue;

		if	(
				exactMatch &&
				(Microarea::TaskBuilderNet::Core::CoreTypes::DataType) field->DataObjType == Microarea::TaskBuilderNet::Core::CoreTypes::DataType::String && 
				AttachedControl->MaxLength != ((MDataStr^)field->DataObj)->MaxLength
			)
			continue;
			
		list->Add(field->Name);
	}

	return list;
}

//-----------------------------------------------------------------------------
void MHotLink::CallCreateComponents ()
{
	__super::CallCreateComponents ();
	Searches->AfterCreateComponents();
}

//-----------------------------------------------------------------------------
bool MHotLink::ReceiveValidate (System::IntPtr recordPtr)
{
	SqlRecord* pRecord = (SqlRecord*) recordPtr.ToInt64();
	if (!pRecord)
		return true;

	HotLinkEventArgs^ eventArgs = gcnew HotLinkEventArgs(CreateRecord(pRecord));
	this->Validated (this, eventArgs);
	if (eventArgs->Cancel)
		return false;
	
	return true;
}

//-----------------------------------------------------------------------------
CString MHotLink::FormatComboItem (SqlRecord* pRecord)
{
	if (!pRecord)
		return _T("");

	HotLinkComboEventArgs^ eventArgs = gcnew HotLinkComboEventArgs(CreateRecord(pRecord));
	this->ComboItemPrepared (this, eventArgs);
	return CString(eventArgs->Result);
}

//-----------------------------------------------------------------------------
void MHotLink::SetError (System::String^ message)
{
	if (m_pHotKeyLink)
		m_pHotKeyLink->SetErrorString(CString(message));
}

//-----------------------------------------------------------------------------
void MHotLink::SetWarning (System::String^ message)
{
	if (m_pHotKeyLink)
		m_pHotKeyLink->SetWarningString(CString(message));
}

//-----------------------------------------------------------------------------
void MHotLink::AttachDefaultEvents ()
{
	if (!m_pHotKeyLink || m_pHotKeyLink->GetOnGoodRecordFunPtr())
		return;
	
	if (onGoodRecordHandle.IsAllocated)
		onGoodRecordHandle.Free();

	onGoodRecordCallBack = gcnew OnGoodRecordCallBack(this, &MHotLink::OnGoodRecord);
	onGoodRecordHandle = GCHandle::Alloc(onGoodRecordCallBack);
	System::IntPtr funPtr = Marshal::GetFunctionPointerForDelegate(onGoodRecordCallBack);
	m_pHotKeyLink->SetOnGoodRecordFunPtr (static_cast<GOOD_REC_FUNC>(funPtr.ToPointer()));
}

//-----------------------------------------------------------------------------
void MHotLink::OnGoodRecord ()
{
	if (Record == nullptr)
		return;

	OnQueried();
	//m_pHotKeyLink->GetAttachedDocument()->UpdateDataView();
}

//-----------------------------------------------------------------------------
bool MHotLink::UseFilterQueryInDirectAccess::get ()
{
	return useFilterQueryInDirectAccess;
}

//-----------------------------------------------------------------------------
void MHotLink::UseFilterQueryInDirectAccess::set (bool value)
{
	useFilterQueryInDirectAccess = value;
}

//-----------------------------------------------------------------------------
void MHotLink::FireHotLinkDestroyed()
{
	m_pHotKeyLink = NULL;
	HotLinkDestroyed(this, gcnew EasyBuilderEventArgs());
}

//-----------------------------------------------------------------------------
bool MHotLink::SearchOnLinkUpper()
{
	return ((EBHotKeyLink*)m_pHotKeyLink)->SearchOnLinkUpperProxy();
}

//-----------------------------------------------------------------------------
bool MHotLink::SearchOnLinkLower()
{
	return ((EBHotKeyLink*)m_pHotKeyLink)->SearchOnLinkLowerProxy();
}

//-----------------------------------------------------------------------------
HotLinkSelectionType MHotLink::GetSelectionType(HotKeyLink::SelectionType type)
{
	switch (type)
	{
	case HotKeyLink::NO_SEL:
		return HotLinkSelectionType::None;
	case HotKeyLink::DIRECT_ACCESS:
		return HotLinkSelectionType::DirectAccess;
	case HotKeyLink::UPPER_BUTTON:
		return HotLinkSelectionType::UpperButton;
	case HotKeyLink::LOWER_BUTTON:
		return HotLinkSelectionType::LowerButton;
	case HotKeyLink::COMBO_ACCESS:
		return HotLinkSelectionType::ComboAccess;
	case HotKeyLink::CUSTOM_ACCESS:
		return HotLinkSelectionType::CustomAccess;
	default:
		return HotLinkSelectionType::None;
	}
}

//-----------------------------------------------------------------------------
void MHotLink::DefineQuery(MSqlTable^ mTable, HotLinkSelectionType selectionType)
{
	__super::DefineQuery(mTable);
}

//-----------------------------------------------------------------------------
void MHotLink::PrepareQuery(MDataObj^ dataObj, MSqlTable^ mTable,  HotLinkSelectionType selectionType)
{					
	PrepareFilterQuery(mTable, (int)selectionType);
}



//-----------------------------------------------------------------------------
void MHotLink::SearchOnLink(MHotLinkSearch^ search) 
{
	IRecordField^ field = Record->GetField(search->FieldName);

	EBHotKeyLink* ebHotLink =  (EBHotKeyLink*)m_pHotKeyLink;
	if (ebHotLink)
		ebHotLink->SetRunningMode(RADAR_FROM_CTRL);

	customSearch = search;
	m_pHotKeyLink->SearchOnLink(((MDataObj^)field->DataObj)->GetDataObj(), HotKeyLink::CUSTOM_ACCESS);
	customSearch = nullptr;
	
}

//-----------------------------------------------------------------------------
MHotLinkSearch^ MHotLink::CreateHotLinkSearch(System::String^ searchField, System::String^ description)
{
	MHotLinkSearch^ temp = gcnew MHotLinkSearch(this, false);
    temp->UseInComboBox = true;
    temp->FieldName = searchField; //ISOCountryCodes.f_ISOCountryCode.Name;
	temp->Description = description; 
	return temp;
}

//-----------------------------------------------------------------------------
void MHotLink::AddUpperSearch(System::String^ searchField, System::String^ description) 
{
	MHotLinkSearch^ HKLISOCountryCodes_ByKey = CreateHotLinkSearch(searchField, description);
    HKLISOCountryCodes_ByKey->AssociatedButton = MHotLinkSearch::ButtonAssociation::UpperButton;
    HKLISOCountryCodes_ByKey->Name = "ByKey";
	Add(HKLISOCountryCodes_ByKey);
}

//-----------------------------------------------------------------------------
void MHotLink::AddLowerSearch(System::String^ searchField, System::String^ description) 
{
	MHotLinkSearch^ HKLISOCountryCodes_ByDescription = CreateHotLinkSearch(searchField, description);
    HKLISOCountryCodes_ByDescription->AssociatedButton = MHotLinkSearch::ButtonAssociation::LowerButton;
    HKLISOCountryCodes_ByDescription->Name = "ByDescription";
	Add(HKLISOCountryCodes_ByDescription);
}

//-----------------------------------------------------------------------------
void MHotLink::AddCustomSearch(System::String^ searchField, System::String^ description) 
{
	MHotLinkSearch^ HKLCustomSearch = CreateHotLinkSearch(searchField, description);
    HKLCustomSearch->Name = "By" + searchField;
	Add(HKLCustomSearch);
}

//-----------------------------------------------------------------------------
bool MHotLink::ExistData(IDataObj^ dataObj)
{
	return m_pHotKeyLink->ExistData(((MDataObj^)dataObj)->GetDataObj()) == TRUE;
}


//-----------------------------------------------------------------------------
void MHotLink::ClearRunningMode ()
{
	if (m_pHotKeyLink && IsDynamic)
		((EBHotKeyLink*) m_pHotKeyLink)->SetRunningMode(0);
}

//-----------------------------------------------------------------------------
void MHotLink::OnRadarRecordAvailable	()
{
}

//-----------------------------------------------------------------------------
bool MHotLink::IsValid	()
{
	return m_pHotKeyLink && IsDynamic ?
			((EBHotKeyLink*) m_pHotKeyLink)->IsValid() == TRUE:
			true;
}

//-----------------------------------------------------------------------------
bool MHotLink::IsToDelete()
{
	return false;
}


