// cannot use C++ managed namespaces in order to avoid stdafx.h conflicts
#include "stdafx.h"
#include "windows.h"

#include <TbGes\BodyEdit.h>
#include <TbGes\FormMng.h>

#include <TbApplication\DocumentThread.h>
#include "MBodyEdit.h"
#include "MTileDialog.h"
#include "MDBTObjects.h"
#include "MParsedControlsExtenders.h"
#include "MDocument.h"
#include "MPanel.h"

using namespace Microarea::Framework::TBApplicationWrapper;
using namespace System::Text;
using namespace System;
using namespace System::Collections::Generic;
using namespace System::ComponentModel::Design::Serialization;
using namespace System::CodeDom;
using namespace System::Drawing;
using namespace System::Windows::Forms;
using namespace System::ComponentModel;

using namespace ICSharpCode::NRefactory::CSharp;
using namespace ICSharpCode::NRefactory::PatternMatching;
using namespace Microarea::TaskBuilderNet::Core::EasyBuilder::Refactor;

class CMyCrackingBodyEdit : public CBodyEdit
{
public:
	void DoMoveToPrevColCracked()
	{
		return DoMoveToPrevCol();
	}
	void DoMoveToNextColCracked()
	{
		return DoMoveToNextCol();
	}
};
/////////////////////////////////////////////////////////////////////////////
// 				class BodyEditSerializer Implementation
/////////////////////////////////////////////////////////////////////////////

//----------------------------------------------------------------------------	
Statement^ BodyEditSerializer::GetConstructor(IDesignerSerializationManager^ manager, EasyBuilderControl^ ebControl)
{
	if (ebControl == nullptr || (!ebControl->GetType()->IsSubclassOf(MBodyEdit::typeid) && ebControl->GetType() != MBodyEdit::typeid))
		return nullptr;

	((MBodyEdit^)ebControl)->IsSerialized = true;

	Point currentLocation = GetLocationToSerialize(ebControl);

	IdentifierExpression^ variableDeclExpression = gcnew IdentifierExpression(ebControl->SerializedName);
	ObjectCreateExpression^ creationExpression = AstFacilities::GetObjectCreationExpression
		(
			gcnew SimpleType(ebControl->SerializedType),
			GetParentWindowReference(),
			gcnew PrimitiveExpression(ebControl->FullId),
			gcnew PrimitiveExpression(ebControl->ClassName),
			AstFacilities::GetObjectCreationExpression
			(
				System::Drawing::Point::typeid->FullName,
				gcnew PrimitiveExpression(currentLocation.X),
				gcnew PrimitiveExpression(currentLocation.Y)
				),
			gcnew PrimitiveExpression(ebControl->HasCodeBehind)
			);

	SetExpression(manager, ebControl, variableDeclExpression, true);

	return AstFacilities::GetAssignmentStatement
		(
			variableDeclExpression,
			creationExpression
			);
}

//----------------------------------------------------------------------------	
TypeDeclaration^ BodyEditSerializer::SerializeClass(SyntaxTree^ syntaxTree, IComponent^ component)
{
	if (component == nullptr || (!component->GetType()->IsSubclassOf(MBodyEdit::typeid) && component->GetType() != MBodyEdit::typeid))
		return nullptr;

	MBodyEdit^ bodyEdit = (MBodyEdit^)component;
	String^ className = bodyEdit->SerializedType;
	RemoveClass(syntaxTree, className);

	TypeDeclaration^ controller = GetControllerTypeDeclaration(syntaxTree);
	String^ controllerName = controller->Name;

	TypeDeclaration^ aClass = gcnew TypeDeclaration();
	aClass->Modifiers = Modifiers::Public;
	aClass->Name = className;
	aClass->BaseTypes->Add(gcnew SimpleType(MBodyEdit::typeid->FullName));

	ConstructorDeclaration^ constr = gcnew ConstructorDeclaration();
	constr->Modifiers = Modifiers::Public;
	constr->Name = aClass->Name;
	aClass->Members->Add(constr);

	constr->Parameters->Add(gcnew ParameterDeclaration(gcnew SimpleType(IWindowWrapperContainer::typeid->FullName), ParentWindowVariableName, ICSharpCode::NRefactory::CSharp::ParameterModifier::None));
	constr->Parameters->Add(gcnew ParameterDeclaration(gcnew PrimitiveType("string"), ControlNameVariableName, ICSharpCode::NRefactory::CSharp::ParameterModifier::None));
	constr->Parameters->Add(gcnew ParameterDeclaration(gcnew PrimitiveType("string"), ControlClassVariableName, ICSharpCode::NRefactory::CSharp::ParameterModifier::None));
	constr->Parameters->Add(gcnew ParameterDeclaration(gcnew SimpleType(Point::typeid->FullName), LocationVariableName, ICSharpCode::NRefactory::CSharp::ParameterModifier::None));
	constr->Parameters->Add(gcnew ParameterDeclaration(gcnew PrimitiveType("bool"), HasCodeBehindVariableName, ICSharpCode::NRefactory::CSharp::ParameterModifier::None));

	constr->Body = gcnew BlockStatement();
	constr->Initializer = AstFacilities::GetConstructorInitializer(
		gcnew IdentifierExpression(ParentWindowVariableName),
		gcnew IdentifierExpression(ControlNameVariableName),
		gcnew IdentifierExpression(ControlClassVariableName),
		gcnew IdentifierExpression(LocationVariableName),
		gcnew IdentifierExpression(HasCodeBehindVariableName)
		);

	AttributeSection^ attr = AstFacilities::GetAttributeSection(PreserveFieldAttribute::typeid->FullName);

	// Cerca la proprieta' Document nella view (tanto e' uguale) e la aggiunge
	TypeDeclaration^ dec = FindClass(syntaxTree, ViewClassName);
	EntityDeclaration^ memberNode = nullptr;
	for each (EntityDeclaration^ current in dec->Members)
	{
		PropertyDeclaration^ viewDocProperty = dynamic_cast<PropertyDeclaration^>(current);
		if (viewDocProperty == nullptr)
			continue;

		// genera la proprietà document differente da quella di documentView
		PropertyDeclaration^ documentProperty = GenerateProperty
			(
				viewDocProperty->ReturnType->Clone(),  //viewDocProperty->Type->BaseType->Name //TODOLUCA
				DocumentPropertyName,
				gcnew CastExpression
				(
					viewDocProperty->ReturnType->Clone() ,//gcnew SimpleType(viewDocProperty->TypeReference->Type),
					gcnew MemberReferenceExpression(gcnew IdentifierExpression(StaticControllerVariableName), DocumentPropertyName)
				),
				false
				);


		documentProperty->Modifiers = viewDocProperty->Modifiers | Modifiers::New;
		documentProperty->Attributes->Add(attr);
		aClass->Members->Add(documentProperty);
		break;
	}

	return aClass;
}

/////////////////////////////////////////////////////////////////////////////
// 				class BodyEditColumnSerializer Implementation
/////////////////////////////////////////////////////////////////////////////

//----------------------------------------------------------------------------	
Statement^ BodyEditColumnSerializer::GetConstructor(IDesignerSerializationManager^ manager, EasyBuilderControl^ ebControl)
{
	if (ebControl == nullptr || (!ebControl->GetType()->IsSubclassOf(MBodyEditColumn::typeid) && ebControl->GetType() != MBodyEditColumn::typeid))
		return nullptr;

	MBodyEditColumn^ column = (MBodyEditColumn^)ebControl;
	System::String^ className = column->ClassType != nullptr ? column->ClassType->ClassName : System::String::Empty;
	int noLocation = 0;

	return AstFacilities::GetAssignmentStatement
		(
			gcnew IdentifierExpression(ebControl->SerializedName),
			AstFacilities::GetObjectCreationExpression
			(
				MBodyEditColumn::typeid->FullName,
				gcnew ThisReferenceExpression(),
				gcnew PrimitiveExpression(ebControl->Name),
				gcnew PrimitiveExpression(className),
				AstFacilities::GetObjectCreationExpression
				(
					System::Drawing::Point::typeid->FullName,
					gcnew PrimitiveExpression(noLocation),
					gcnew PrimitiveExpression(noLocation)
					),
				gcnew PrimitiveExpression(ebControl->HasCodeBehind)
				)
			);
}

/////////////////////////////////////////////////////////////////////////////
// 						class MBodyEditColumn Implementation
/////////////////////////////////////////////////////////////////////////////

//----------------------------------------------------------------------------
MBodyEditColumn::MBodyEditColumn(System::IntPtr colInfoPtr)
{
	HasCodeBehind = true;
	m_pColumnInfo = (ColumnInfo*)colInfoPtr.ToInt64();
	if (m_pColumnInfo && m_pColumnInfo->GetParsedCtrl())
	{
		CParsedCtrl* pCtrl = m_pColumnInfo->GetParsedCtrl();
		SetControl((MParsedControl^)BaseWindowWrapper::Create((System::IntPtr) pCtrl->GetCtrlCWnd()->m_hWnd));
	}
}

//----------------------------------------------------------------------------
MBodyEditColumn::MBodyEditColumn(MBodyEdit^ parentWindow, System::String^ name, System::String^ controlClass, Point location, bool hasCodeBehind)
{
	this->HasCodeBehind = hasCodeBehind;

	BodyEdit = parentWindow;

	if (bodyEdit == nullptr)
	{
		ASSERT(FALSE);
		return;
	}

	CBodyEdit* pBodyEdit = (CBodyEdit*)(long)bodyEdit->TbHandle;
	if (!bodyEdit)
	{
		ASSERT(FALSE);
		return;
	}
	if (hasCodeBehind)
	{
		if (BodyEdit->DesignModeType != EDesignMode::Static)
		{
			RenameChangeRequest^ request = gcnew RenameChangeRequest(Refactor::ChangeSubject::Class, BodyEdit->Namespace, BodyEdit->Document->Namespace, String::Empty, name, ((MDocument^)BodyEdit->Document)->Version);
			name = BaseCustomizationContext::ApplicationChanges->GetNewNameOf(request);
			delete request;
		}

		// mi copio le informazioni compreso il columninfo 
		m_pColumnInfo = pBodyEdit->GetColumn(name);
		if (!m_pColumnInfo)
		{
			ASSERT(FALSE);
			System::String^ message = gcnew System::String(
				"This customization is not compatibile with new document view model due to column: " + name + System::Environment::NewLine +
				"Parent Object Name : " + BodyEdit->Namespace->ToString() + System::Environment::NewLine +
				"Document : " + BodyEdit->Document->Namespace->ToString() + System::Environment::NewLine + 
				"Version Assembly: " + this->Version->ToString() + System::Environment::NewLine +
				"Current Version: " + System::Reflection::Assembly::GetExecutingAssembly()->GetName()->Version->ToString()
			);
			throw gcnew ApplicationException(message);
			return;
		}
		SetControl((MParsedControl^)BaseWindowWrapper::Create((System::IntPtr) m_pColumnInfo->GetParsedCtrl()->GetCtrlCWnd()->m_hWnd));
		return;
	}

	// default control class
	CString sControlClassName(controlClass);
	m_pColumnInfo = pBodyEdit->CreateDefaultColumn(sControlClassName, name);
	if (m_pColumnInfo)
		SetControl((MParsedControl^)BaseWindowWrapper::Create((System::IntPtr) m_pColumnInfo->GetParsedCtrl()->GetCtrlCWnd()->m_hWnd));
	else
		ASSERT(FALSE);

	// qualora venisse ripristinato l'ordine originale delle colonne,
	// le colonne aggiunge da EB vanno sempre in fondo
	if (!hasCodeBehind)
	{
		if (pBodyEdit->GetDocument() && pBodyEdit->GetDocument()->m_pFormManager)
		{
			BodyEditInfo* pBEInfo = pBodyEdit->GetDocument()->m_pFormManager->GetBodyEditInfo(pBodyEdit->GetNamespace());
			if (pBEInfo)
				pBodyEdit->AddColumnInFormManager(m_pColumnInfo, pBEInfo, pBodyEdit->GetAllColumnsInfoUpperBound());
		}
		else
			m_pColumnInfo->SetColPos(1000);
	}

	//if (!this->HasCodeBehind && !jsonColDescription)
	//	jsonColDescription = new CWndBodyColumnDescription(NULL);
}

//----------------------------------------------------------------------------
MBodyEditColumn::MBodyEditColumn(MBodyEdit^ parentWindow, System::Type^ controlType)
{
	this->HasCodeBehind = false;

	BodyEdit = (MBodyEdit^)parentWindow;

	if (bodyEdit == nullptr)
	{
		ASSERT(FALSE);
		return;
	}

	CBodyEdit* pBodyEdit = (CBodyEdit*)(long)bodyEdit->TbHandle;
	if (!bodyEdit)
	{
		ASSERT(FALSE);
		return;
	}

	CParsedCtrlFamily*	pFamily = AfxGetParsedControlsRegistry()->GetFamily(controlType->FullName);
	// default control class
	CString sControlClassName = pFamily->GetDefaultControl();

	CString sColName = cwsprintf(_T("IDC_%s%d"), sControlClassName, pBodyEdit->GetColumnsInfoNumber());

	m_pColumnInfo = pBodyEdit->CreateDefaultColumn(sControlClassName, sColName);
	if (m_pColumnInfo)
		SetControl((MParsedControl^)BaseWindowWrapper::Create((System::IntPtr) m_pColumnInfo->GetParsedCtrl()->GetCtrlCWnd()->m_hWnd));
	else
		ASSERT(FALSE);

	// qualora venisse ripristinato l'ordine originale delle colonne,
	// le colonne aggiunge da EB vanno sempre in fondo
	if (pBodyEdit->GetDocument() && pBodyEdit->GetDocument()->m_pFormManager)
	{
		BodyEditInfo* pBEInfo = pBodyEdit->GetDocument()->m_pFormManager->GetBodyEditInfo(pBodyEdit->GetNamespace());
		if (pBEInfo)
			pBodyEdit->AddColumnInFormManager(m_pColumnInfo, pBEInfo, pBEInfo->GetSize());
	}
	else
		m_pColumnInfo->SetColPos(1000);

	//if (!this->HasCodeBehind && !jsonColDescription)
	//	jsonColDescription = new CWndBodyColumnDescription(NULL);
}
//----------------------------------------------------------------------------
MBodyEditColumn::~MBodyEditColumn()
{
	this->!MBodyEditColumn();
	GC::SuppressFinalize(this);
}

//----------------------------------------------------------------------------
MBodyEditColumn::!MBodyEditColumn()
{
	m_pColumnInfo = NULL;
	// okkkio che con posso chiamare la set del DataBinding della colonna perchè gli oggetti
	// C++ sono già morti e non è in grado di lavorare. Ci penserà la SetControl a sistemare
	// le references
	SetControl(nullptr);
}

//----------------------------------------------------------------------------
void MBodyEditColumn::Activate()
{
	WindowWrapperContainer^ container = (WindowWrapperContainer^)Parent;
	if (container != nullptr)
		container->Activate();
}

//-----------------------------------------------------------------------------
CWndObjDescription * MBodyEditColumn::UpdateAttrForJson(CWndBodyColumnDescription* pColumnDescription)
{
	if (!pColumnDescription)
		return NULL;

	pColumnDescription->m_X = NULL_COORD;
	pColumnDescription->m_Y = NULL_COORD;
	pColumnDescription->m_Width = NULL_COORD;
	pColumnDescription->m_Height = NULL_COORD;

	//binding
	if (pColumnDescription->m_pBindings)
	{
		//exists => clear
		if (pColumnDescription->m_pBindings->m_pHotLink)
		{
			delete pColumnDescription->m_pBindings->m_pHotLink;
			pColumnDescription->m_pBindings->m_pHotLink = NULL;
		}
		BindingInfo* pBindings = pColumnDescription->m_pBindings;
		delete pBindings;
		pColumnDescription->m_pBindings = NULL;
	}

	//manage data binding
	if (this->DataBinding != nullptr)
	{
		pColumnDescription->m_pBindings = new BindingInfo();
		NameSpace^ parent = (NameSpace^)this->DataBinding->Parent->Namespace;
		pColumnDescription->m_pBindings->m_strDataSource = CString(parent->Leaf) + _T(".") + CString(this->DataBinding->Name);
		if (this->HotLink != nullptr)
		{
			pColumnDescription->m_pBindings->m_pHotLink = new HotLinkInfo();
			pColumnDescription->m_pBindings->m_pHotLink->m_strName = CString(this->HotLink->Name);
			pColumnDescription->m_pBindings->m_pHotLink->m_strNamespace = CString(this->HotLink->Namespace->FullNameSpace);
			pColumnDescription->m_pBindings->m_pHotLink->m_bEnableAddOnFly = (Bool3)this->HotLink->CanAddOnFly;
			pColumnDescription->m_pBindings->m_pHotLink->m_bMustExistData = (Bool3)this->HotLink->DataMustExist;
		}
	}
	//grayed
	pColumnDescription->m_bStatusGrayed = this->IsGrayed;
	//hidden
	pColumnDescription->m_bStatusHidden = this->IsHidden;

	return pColumnDescription;
}

//----------------------------------------------------------------------------
System::IntPtr MBodyEditColumn::Handle::get()
{
	return m_pColumnInfo ? (IntPtr) m_pColumnInfo->GetParsedCtrl()->GetCtrlCWnd()->m_hWnd : IntPtr::Zero;
}
//----------------------------------------------------------------------------
System::Drawing::Rectangle MBodyEditColumn::Rectangle::get()
{
	if (!Visible)
		return System::Drawing::Rectangle::Empty;

	System::Drawing::Rectangle bodyRect = BodyEdit->Rectangle;
	CBodyEdit* pBody = (CBodyEdit*)BodyEdit->GetWnd();
	System::Drawing::Rectangle rect = System::Drawing::Rectangle(
		bodyRect.X + pBody->GetHorizontalOffset(m_pColumnInfo),
		bodyRect.Y,
		m_pColumnInfo->GetScreenWidth(), 
		bodyRect.Height);
	return rect;
}

//----------------------------------------------------------------------------
System::Drawing::Point MBodyEditColumn::Location::get()
{
	return Rectangle.Location;
}
//----------------------------------------------------------------------------
void MBodyEditColumn::Location::set(System::Drawing::Point value)
{
}

//----------------------------------------------------------------------------
System::Drawing::Size MBodyEditColumn::Size::get()
{
	return Rectangle.Size;
}
//----------------------------------------------------------------------------
void MBodyEditColumn::Size::set(System::Drawing::Size value)
{
	Width = value.Width;
}
//----------------------------------------------------------------------------
CWndBodyColumnDescription* MBodyEditColumn::GetWndObjDescription()
{
	if (!BodyEdit)
		return NULL;
	CWndObjDescription* pDesc = BodyEdit->GetWndObjDescription();
	if (!pDesc)
		return NULL;
	UINT nId = m_pColumnInfo->GetCtrlID();
	CString strColTitleId = nId ? AfxGetTBResourcesMap()->DecodeID(TbControls, nId).m_strName : _T("");

	return dynamic_cast<CWndBodyColumnDescription*>(pDesc->Find(strColTitleId));
}

//----------------------------------------------------------------------------
void MBodyEditColumn::DataSource::set(String^ dataSource)
{
	CWndObjDescription* pDescri = GetWndObjDescription();
	if (pDescri)
	{
		if (!pDescri->m_pBindings)
			pDescri->m_pBindings = new BindingInfo;
		pDescri->m_pBindings->m_strDataSource = dataSource;
	}

}
//----------------------------------------------------------------------------
String^ MBodyEditColumn::DataSource::get()
{
	CWndObjDescription* pDescri = GetWndObjDescription();
	if (pDescri && pDescri->m_pBindings)
		return gcnew String(pDescri->m_pBindings->m_strDataSource);
	return "";
}

//----------------------------------------------------------------------------
System::String^	MBodyEditColumn::Id::get()
{
	return m_pColumnInfo 
		? gcnew System::String(AfxGetTBResourcesMap()->DecodeID(TbControls, m_pColumnInfo->GetCtrlID()).m_strName)
		: "";
}
//----------------------------------------------------------------------------
void MBodyEditColumn::Id::set(System::String^ value)
{	
	if (!m_pColumnInfo)
	{
		ASSERT(FALSE);
		return;
	}
	CWndObjDescription* pDesc = GetWndObjDescription();
	if (pDesc)
	{
		pDesc->SetID(value, true);
	}
	m_pColumnInfo->SetCtrlID(AfxGetTBResourcesMap()->GetTbResourceID(CString(value), TbControls));
	CParsedCtrl* pCtrl = m_pColumnInfo->GetParsedCtrl();
	if (pCtrl)
		pCtrl->SetCtrlID(AfxGetTBResourcesMap()->GetTbResourceID(CString(value), TbControls));
}
//----------------------------------------------------------------------------
System::String^ MBodyEditColumn::Name::get()
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	if (pDesc && !pDesc->m_strName.IsEmpty())
		return gcnew System::String(pDesc->m_strName);
	return m_pColumnInfo
		? gcnew System::String(m_pColumnInfo->GetNamespace().GetObjectName())
		: __super::Name;
}

//----------------------------------------------------------------------------
void MBodyEditColumn::Name::set(System::String^ value)
{
	if (!m_pColumnInfo)
	{
		ASSERT(FALSE);
		return;
	}	

	CWndObjDescription* pDesc = GetWndObjDescription();
	if (pDesc)
		pDesc->m_strName = CString(value);

	m_pColumnInfo->GetNamespace().SetObjectName(CString(value), TRUE);
	CParsedCtrl* pCtrl = m_pColumnInfo->GetParsedCtrl();
	if (pCtrl)
	{
		if (pCtrl->GetNamespace().IsEmpty())
		{
			CTBNamespace aNs(CTBNamespace::CONTROL, m_pColumnInfo->GetNamespace().ToUnparsedString());
			pCtrl->SetNamespace(aNs.ToString());
		}
		else
			pCtrl->GetNamespace().SetObjectName(CString(value), TRUE);
	}
}
//----------------------------------------------------------------------------
System::String^ MBodyEditColumn::SerializedName::get()
{
	return GetSerializedName(Name);
}

//----------------------------------------------------------------------------
INameSpace^ MBodyEditColumn::Namespace::get()
{
	return m_pColumnInfo ? gcnew NameSpace(gcnew System::String(m_pColumnInfo->GetNamespace().ToString())) : nullptr;
}

//----------------------------------------------------------------------------
bool MBodyEditColumn::CanChangeProperty(System::String^ propertyName)
{
	//posso assegnare l'hotlink solo se ho prima impostato il databinding
	if (propertyName == MParsedControlSerializer::HotLinkPropertyName)
		return DataBinding != nullptr;

	//posso modificare il databinding solo se non ho l'hotlink agganciato
	if (propertyName == MParsedControlSerializer::DataBindingPropertyName)
		return HotLink == nullptr;

	//ESD: posso assegnare l'hotlinkNs solo se ho prima impostato l'hotlinkName perchè è chiave
	if (propertyName == MParsedControlSerializer::HotLinkNsPropName)
		return !System::String::IsNullOrEmpty(HotLinkName);

	if (propertyName == MParsedControlSerializer::ShowHotLinkButtonPropertyName)
		return HotLink != nullptr;

	if (propertyName == "Sort")
		return true;

	for (int i = 0; i < (MParsedControlSerializer::BEStatusPropertynames)->Length; i++) 
		if (MParsedControlSerializer::BEStatusPropertynames[i] == propertyName)
			return true;

	if (propertyName == MParsedControlSerializer::ItemsSourcePropertyName)
		return IsItemsSourceEditable;

	return __super::CanChangeProperty(propertyName);
}

//----------------------------------------------------------------------------
System::String^ MBodyEditColumn::ColumnTitle::get()
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	if (pDesc)
		return gcnew System::String(pDesc->m_strText);

	return m_pColumnInfo ? gcnew System::String(m_pColumnInfo->GetTitle()) : System::String::Empty;
}
//-----------------------------------------------------------------------------
void MBodyEditColumn::ColumnTitle::set(System::String^ title)
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	if (pDesc)
		pDesc->m_strText = title;

	if (!m_pColumnInfo)
	{
		ASSERT(FALSE);
		return;
	}

	m_pColumnInfo->SetTitle(CString(title));
	m_pColumnInfo->GetBodyEdit()->Invalidate();
	NotifyBodyEditChanged();
}

//----------------------------------------------------------------------------
bool MBodyEditColumn::Visible::get()
{
	return m_pColumnInfo ? (m_pColumnInfo->GetStatus() & STATUS_HIDDEN) != STATUS_HIDDEN : FALSE;
}

//----------------------------------------------------------------------------
void MBodyEditColumn::Visible::set(bool visible)
{
	if (!m_pColumnInfo)
	{
		ASSERT(FALSE);
		return;
	}

	m_pColumnInfo->SetVisible(visible == true);
	NotifyBodyEditChanged();
}

//----------------------------------------------------------------------------
System::String^ MBodyEditColumn::ClassName::get()
{
	return ClassType == nullptr ? System::String::Empty : ClassType->ClassName;
}

//----------------------------------------------------------------------------
IControlClass^ MBodyEditColumn::ClassType::get()
{
	if (control && MParsedControl::typeid->IsInstanceOfType(control))
		return ((MParsedControl^)control)->ClassType;

	return nullptr;
}

//----------------------------------------------------------------------------
void MBodyEditColumn::ClassType::set(IControlClass^ controlClass)
{
	if (!m_pColumnInfo)
	{
		ASSERT(FALSE);
		return;
	}
	CWndObjDescription* pDescri = GetWndObjDescription();
	if (pDescri)
		pDescri->m_strControlClass = controlClass->ClassName;

	CString sName(controlClass->ClassName);
	CString sFamilyName(controlClass->FamilyName);

	CRegisteredParsedCtrl* pOldRegisteredCtrl = control != nullptr && control->GetWnd() ?
		AfxGetParsedControlsRegistry()->GetRegisteredControl(control->GetWnd()) :
		NULL;

	CRegisteredParsedCtrl* pRegisteredCtrl = AfxGetParsedControlsRegistry()->GetRegisteredControl(sFamilyName, sName);
	CRuntimeClass*  pNewRTC = pRegisteredCtrl ? pRegisteredCtrl->GetClass() : NULL;

	if (!pOldRegisteredCtrl || pOldRegisteredCtrl == pRegisteredCtrl)
		return;

	// devo salvare il databiding esistente che verrà distrutto con la delete del control
	FieldDataBinding^ oldDataBinding = nullptr;
	if (control != nullptr)
	{
		if (control->GetType()->IsSubclassOf(MParsedControl::typeid))
		{
			MParsedControl^ ctrl = (MParsedControl^)control;
			oldDataBinding = ctrl->DataBinding ? (FieldDataBinding^)ctrl->DataBinding->Clone() : nullptr;
		}

		control->Handle = System::IntPtr::Zero;
		delete control;
		SetControl(nullptr);
	}

	m_pColumnInfo->ChangeParsedControlTo(pNewRTC, pOldRegisteredCtrl->GetNeededStyle(), pRegisteredCtrl->GetNeededStyle(), m_pColumnInfo->GetNamespace().GetObjectName());
	CParsedCtrl* pCtrl = m_pColumnInfo->GetParsedCtrl();
	if (pCtrl == NULL)
	{
		ASSERT(FALSE);
		return;
	}

	SetControl((MParsedControl^)BaseWindowWrapper::Create((System::IntPtr) pCtrl->GetCtrlCWnd()->m_hWnd));
	if (control != nullptr)
	{
		((ControlClass^)((MParsedControl^)control)->ClassType)->SetRegInfoPtr(pRegisteredCtrl);
		if (oldDataBinding != nullptr)
			((MParsedControl^)control)->DataBinding = oldDataBinding;
	}

	if (this->Formatter == nullptr || !this->Formatter->CompatibleType->Equals(controlClass->CompatibleType))
		InitializeDefaultFormatter(controlClass->CompatibleType);
}

//----------------------------------------------------------------------------
void MBodyEditColumn::InitializeDefaultFormatter(IDataType^ type)
{
	::DataType aDataType = ::DataType(type->Type, type->Tag);
	::Formatter* pFormatter = AfxGetFormatStyleTable()->GetFormatter(FromDataTypeToFormatName(aDataType), NULL);

	Formatter = gcnew FormatterStyle(pFormatter);

	if (Formatter != nullptr && Formatter->TypedInfo != nullptr && Formatter->TypedInfo->GetType() == FloatFormatterInfo::typeid)
		NumberOfDecimal = ((FloatFormatterInfo^)Formatter->TypedInfo)->NumberOfDecimals;
}

//----------------------------------------------------------------------------
void MBodyEditColumn::InitializeDefaultControlClass(IDataType^ type)
{
	::DataType aDataType(type->Type, type->Tag);

	const CParsedCtrlFamily* pFamily = AfxGetParsedControlsRegistry()->GetDefaultFamilyInfo(aDataType);
	if (!pFamily)
		return;

	CString sControlClass = pFamily->GetDefaultControl(aDataType);
	CRegisteredParsedCtrl* pNewCtrl = AfxGetParsedControlsRegistry()->GetRegisteredControl(pFamily->GetName(), sControlClass);
	CRegisteredParsedCtrl* pOldCtrl = ClassType == nullptr ? NULL : ((ControlClass^)ClassType)->GetRegInfoPtr();
	if (pOldCtrl != pNewCtrl)
		ClassType = gcnew ControlClass(pNewCtrl);
}

//----------------------------------------------------------------------------
ISite^ MBodyEditColumn::Site::get()
{
	return __super::Site;
}

//----------------------------------------------------------------------------
void MBodyEditColumn::Site::set(ISite^ site)
{
	__super::Site = site;

	if (Extensions != nullptr)
		Extensions->Service->AdjustSites();
}

//----------------------------------------------------------------------------
void MBodyEditColumn::SetControl(MParsedControl^ control)
{
	// devo gestire il rimpallo degli eventi del control di colonna
	if (this->control != nullptr)
	{
		this->control->SetParent(nullptr);
		((MParsedControl^) this->control)->ValueChanged -= gcnew EventHandler<EasyBuilderEventArgs^>(this, &MBodyEditColumn::OnValueChanged);
		//pulisce eventuali reference al databinding
		((MParsedControl^) this->control)->DataBinding = nullptr;
	}

	this->control = control;

	if (this->control != nullptr)
	{
		this->control->SetParent(this->parent);
		this->control->ParentComponent = this;
		if (Extensions != nullptr)
			Extensions->Service->Parent = this;

		((MParsedControl^) this->control)->ValueChanged += gcnew EventHandler<EasyBuilderEventArgs^>(this, &MBodyEditColumn::OnValueChanged);
	}
}

//----------------------------------------------------------------------------
System::IntPtr MBodyEditColumn::ColumnInfoPtr::get()
{
	return m_pColumnInfo ? (System::IntPtr) m_pColumnInfo : System::IntPtr::Zero;
}

//----------------------------------------------------------------------------
IDataBinding^ MBodyEditColumn::DataBinding::get()
{
	if (!m_pColumnInfo || control == nullptr || !MParsedControl::typeid->IsInstanceOfType(control))
		return nullptr;

	MParsedControl^ mControl = (MParsedControl^)control;

	if (mControl->DataBinding != nullptr)
		return mControl->DataBinding;

	if (BodyEdit == nullptr)
		return nullptr;

	CParsedCtrl* pCtrl = m_pColumnInfo->GetParsedCtrl();
	::DataObj* pDataObj = m_pColumnInfo->GetBaseDataObj();

	if (!pCtrl || !pDataObj || BodyEdit->DataBinding == nullptr)
		return nullptr;

	MDBTObject^ dbt = (MDBTObject^)BodyEdit->DataBinding->Data;
	MSqlRecord^ mSqlRecord = dbt == nullptr ? nullptr : (MSqlRecord^)dbt->Record;

	if (mSqlRecord == nullptr)
		mSqlRecord = gcnew MSqlRecord(pCtrl->m_pSqlRecord);

	System::String^ colName = gcnew System::String(pCtrl->m_pSqlRecord->GetColumnName(pDataObj));
	IRecordField^ recField = mSqlRecord->GetField(colName);
	MDataObj^ mDataObj = recField == nullptr ? nullptr : (MDataObj^)recField->DataObj;

	if (mDataObj == nullptr)
		mDataObj = MDataObj::Create(pDataObj);

	DataBinding = gcnew FieldDataBinding(mDataObj, dbt);

	EasyBuilderComponent^ ebComponent = (EasyBuilderComponent^)dbt;
	ebComponent->AddReferencedBy(this->SerializedName);

	return mControl->DataBinding;
}

//----------------------------------------------------------------------------
void MBodyEditColumn::DataBinding::set(IDataBinding^ dataBinding)
{
	if (control == nullptr || !MParsedControl::typeid->IsInstanceOfType(control))
		return;

	MParsedControl^ mControl = (MParsedControl^)control;

	int oldDataType = mControl->DataBinding != nullptr ? mControl->DataBinding->DataType->Type : -1;

	mControl->DataBinding = dataBinding;

	if (m_pColumnInfo)
		m_pColumnInfo->SetBaseDataObj(dataBinding == nullptr ? NULL : ((MDataObj^)dataBinding->Data)->GetDataObj());

	if (mControl->DataBinding == nullptr)
		return;

	FieldDataBinding^ fieldDataBinding = (FieldDataBinding^)dataBinding;
	MDataObj^ dataObj = (MDataObj^)fieldDataBinding->Data;

	if (dataObj != nullptr && dataObj->GetDataObj() && oldDataType != dataBinding->DataType->Type)
		SizeInChars = System::Drawing::Size(dataObj->GetDataObj()->GetColumnLen(), SizeInChars.Height);

	if (this->Formatter == nullptr || !this->Formatter->CompatibleType->Equals(dataBinding->DataType))
		InitializeDefaultFormatter(this->DataBinding->DataType);

	if (this->ClassType == nullptr || !this->ClassType->CompatibleType->Equals(this->DataBinding->DataType))
	{
		InitializeDefaultControlClass(this->DataBinding->DataType);

		// invalido l'eventuale lista di oggetti che dipendono dal data type
		if (IItemsSourceConsumer::typeid->IsInstanceOfType(this) && ((IItemsSourceConsumer^) this)->ItemsSource != nullptr)
			((IItemsSourceConsumer^) this)->ItemsSource->Clear();
	}

	// non devo notificare il cambio del bodyedit perchè il 
	// databinding è collegato solo alle colonne isCodeBehind a false
	// le quali già di fatto sono serializzate e notificate bene. Se lo
	// notifico, creo una ridondanza di notifica che fa scattare anche la 
	// doppia CreateComponents dato che si tratta di una property ondemand sulla get

}

//----------------------------------------------------------------------------
MHotLink^ MBodyEditColumn::HotLink::get()
{
	return (control && MParsedControl::typeid->IsInstanceOfType(control))
		? ((MParsedControl^)control)->HotLink
		: nullptr;
}

//----------------------------------------------------------------------------
void MBodyEditColumn::HotLink::set(MHotLink^ value)
{
	if (control && MParsedControl::typeid->IsInstanceOfType(control))
	{
		System::String^ error = nullptr;
		if (value != nullptr && !value->CanBeAttached((Microarea::TaskBuilderNet::Core::CoreTypes::DataType) control->CompatibleType, SizeInChars.Width, error))
		{
			Diagnostic->SetError(error);
			return;
		}

		((MParsedControl^)control)->HotLink = value;
	}
	NotifyBodyEditChanged();
}


//----------------------------------------------------------------------------
String^ MBodyEditColumn::HotLinkName::get()
{
	CWndObjDescription* pDescri = GetWndObjDescription();
	if (!pDescri || !pDescri->m_pBindings)
		return "";
	HotLinkInfo* pInfo = pDescri->m_pBindings->m_pHotLink;
	if (!pInfo)
		return "";
	return gcnew String(pInfo->m_strName);
}

//----------------------------------------------------------------------------
void MBodyEditColumn::HotLinkName::set(String^ value)
{
	CWndObjDescription* pDescri = GetWndObjDescription();
	if (!pDescri)
		return;
	if (!pDescri->m_pBindings)
		pDescri->m_pBindings = new BindingInfo();
	HotLinkInfo* pInfo = pDescri->m_pBindings->m_pHotLink;
	if (!pInfo)
	{
		pInfo = new HotLinkInfo;
		pDescri->m_pBindings->m_pHotLink = pInfo;
	}

	pInfo->m_strName = value;
	if (System::String::IsNullOrEmpty(value))
		HotLinkNs = value;
}

//----------------------------------------------------------------------------
void MBodyEditColumn::HotLinkNs::set(String^ value)
{
	CWndObjDescription* pDescri = GetWndObjDescription();
	if (pDescri)
	{
		if (!pDescri->m_pBindings)
			pDescri->m_pBindings = new BindingInfo;
		HotLinkInfo* pInfo = pDescri->m_pBindings->m_pHotLink;
		if (!pInfo)
		{
			pInfo = new HotLinkInfo;
			pDescri->m_pBindings->m_pHotLink = pInfo;
		}

		pInfo->m_strNamespace = value;
	}

}
//----------------------------------------------------------------------------
String^ MBodyEditColumn::HotLinkNs::get()
{
	CWndObjDescription* pDescri = GetWndObjDescription();
	if (pDescri && pDescri->m_pBindings)
	{
		HotLinkInfo* pInfo = pDescri->m_pBindings->m_pHotLink;
		if (!pInfo)
			return "";
		return gcnew String(pInfo->m_strNamespace);
	}

	return "";
}

//----------------------------------------------------------------------------
int MBodyEditColumn::Width::get()
{
	return m_pColumnInfo ? m_pColumnInfo->GetScreenWidth() : 0;
}

//----------------------------------------------------------------------------
void MBodyEditColumn::Width::set(int value)
{
	if (m_pColumnInfo && Width != value)
	{
		CAbstractFormDoc* pDoc = m_pColumnInfo->GetBodyEdit()->GetDocument();
		BOOL bOldFMModified = pDoc && pDoc->m_pFormManager && pDoc->m_pFormManager->m_bTBFModified;

		// per bypassare il vecchio formmanager devo settare anche la default
		m_pColumnInfo->SetDefaultScreenWidth(value);
		m_pColumnInfo->GetBodyEdit()->ResizeColumn(m_pColumnInfo, value, TRUE);
		// la ResizeColumn per eseguire il resize corretto deve avvisare anche le
		// strutture del vecchio form manager. Devo ricordarmi di resettarne lo stato 
		if (pDoc && pDoc->m_pFormManager)
			pDoc->m_pFormManager->m_bTBFModified = bOldFMModified;
		Rectangle.Width = value;
		NotifyBodyEditChanged();
	}
	CWndObjDescription* pDescri = GetWndObjDescription();
		if (pDescri)
			pDescri->m_Width = value;
}

//----------------------------------------------------------------------------
Color MBodyEditColumn::TitleBackColor::get()
{
	if (!m_pColumnInfo)
		return Color::Empty;

	COLORREF ref = m_pColumnInfo->GetBkgColor();
	if (ref == DefaultColor)
		return System::Drawing::Color::Transparent;

	return System::Drawing::ColorTranslator::FromWin32(ref);
}

//----------------------------------------------------------------------------
void MBodyEditColumn::TitleBackColor::set(Color value)
{
	if (!m_pColumnInfo)
	{
		ASSERT(FALSE);
		return;
	}

	if (value == Color::Empty)
		value = System::Drawing::Color::Transparent;

	// il default color del background purtroppo è lo 
	// stesso valore del Black quindi uso il transparent come chiave x il default
	COLORREF ref = value == System::Drawing::Color::Transparent ?
		DefaultColor :
		System::Drawing::ColorTranslator::ToWin32(value);
	m_pColumnInfo->SetBkgColor(ref);
	m_pColumnInfo->GetBodyEdit()->Invalidate();
	NotifyBodyEditChanged();
}

//----------------------------------------------------------------------------
Color MBodyEditColumn::TitleTextColor::get()
{
	if (!m_pColumnInfo)
		return Color::Empty;

	COLORREF ref = m_pColumnInfo->GetTextColor();
	if (ref == DefaultColor)
		return System::Drawing::Color::Black;

	return System::Drawing::ColorTranslator::FromWin32(ref);
}

//----------------------------------------------------------------------------
void MBodyEditColumn::TitleTextColor::set(Color color)
{
	if (!m_pColumnInfo)
	{
		ASSERT(FALSE);
		return;
	}

	if (color == Color::Empty && m_pColumnInfo->GetTextColor() == DefaultColor)
		return;

	COLORREF ref = System::Drawing::ColorTranslator::ToWin32(color);

	m_pColumnInfo->SetTextColor(ref);
	m_pColumnInfo->GetBodyEdit()->Invalidate();
	NotifyBodyEditChanged();
}

//----------------------------------------------------------------------------
IDataType^ MBodyEditColumn::CompatibleType::get()
{
	return ClassType->CompatibleType;
}

//----------------------------------------------------------------------------
bool MBodyEditColumn::Locked::get()
{
	return m_pColumnInfo ? (m_pColumnInfo->GetStatus() & STATUS_LOCKED) == STATUS_LOCKED : false;
}

//----------------------------------------------------------------------------
void MBodyEditColumn::Locked::set(bool value)
{
	WORD wCurrStatus = m_pColumnInfo->GetStatus();
	if (value)
		m_pColumnInfo->SetStatus(wCurrStatus | STATUS_LOCKED);
	else
		m_pColumnInfo->SetStatus(wCurrStatus & ~STATUS_LOCKED);
	NotifyBodyEditChanged();
}

//----------------------------------------------------------------------------
int MBodyEditColumn::NumberOfDecimal::get()
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	if (pDesc)
		return pDesc->m_nNumberDecimal;
	if (control && MParsedControl::typeid->IsInstanceOfType(control))
		return ((MParsedControl^)control)->NumberOfDecimal;

	return 0;
}

//----------------------------------------------------------------------------
void MBodyEditColumn::NumberOfDecimal::set(int nDec)
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	if (pDesc)
		pDesc->m_nNumberDecimal = nDec;
	if (control && MParsedControl::typeid->IsInstanceOfType(control))
		((MParsedControl^)control)->NumberOfDecimal = nDec;
	NotifyBodyEditChanged();
}

//----------------------------------------------------------------------------
FormatterStyle^ MBodyEditColumn::Formatter::get()
{
	if (control && MParsedControl::typeid->IsInstanceOfType(control))
		return ((MParsedControl^)control)->Formatter;

	return nullptr;
}

//----------------------------------------------------------------------------
void MBodyEditColumn::Formatter::set(FormatterStyle^ value)
{
	if (control && MParsedControl::typeid->IsInstanceOfType(control))
	{
		MParsedControl^ parsedControl = ((MParsedControl^)control);

		bool changed = Formatter != value;
		parsedControl->Formatter = value;
		if (changed && Formatter != nullptr && Formatter->TypedInfo != nullptr && Formatter->TypedInfo->GetType() == FloatFormatterInfo::typeid)
			NumberOfDecimal = ((FloatFormatterInfo^)Formatter->TypedInfo)->NumberOfDecimals;
		NotifyBodyEditChanged();
	}
}

//----------------------------------------------------------------------------
int MBodyEditColumn::ColPos::get()
{
	CBodyEdit* pBodyEdit = (CBodyEdit*)(long)bodyEdit->TbHandle;
	return m_pColumnInfo && pBodyEdit ? pBodyEdit->GetColumnIdx(m_pColumnInfo) : -1;
}

//----------------------------------------------------------------------------
void MBodyEditColumn::ColPos::set(int value)
{
	if (bodyEdit == nullptr || bodyEdit->TbHandle == 0)
		return;

	CBodyEdit* pBodyEdit = (CBodyEdit*)(long)bodyEdit->TbHandle;
	pBodyEdit->MoveColumnTo(m_pColumnInfo, value);

	CWndObjDescription* pDesc = GetWndObjDescription();
	CWndObjDescription* pBodyDesc = pDesc ? pDesc->GetParent() : NULL;
	if (pBodyDesc)
	{
		pBodyDesc->m_Children.RemoveItem(pDesc, FALSE);
		pBodyDesc->m_Children.InsertAt(value, pDesc);
	}
	NotifyBodyEditChanged();
}

//----------------------------------------------------------------------------
int MBodyEditColumn::Chars::get()
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	if (pDesc)
		return pDesc->m_nChars;
	return -1;
}

//----------------------------------------------------------------------------
void MBodyEditColumn::Chars::set(int value)
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	if (pDesc)
		pDesc->m_nChars = value;
	NotifyBodyEditChanged();
}

//----------------------------------------------------------------------------
System::Drawing::Size MBodyEditColumn::SizeInChars::get()
{
	CSize aSize(0, 0);

	if (m_pColumnInfo)
		m_pColumnInfo->GetCtrlSize(aSize);

	// in alcuni datatype il m_nCtrlLen ritornato è 0 che non è un valore ammesso
	return System::Drawing::Size(aSize.cy == 0 ? 1 : aSize.cy, aSize.cx);
}

//----------------------------------------------------------------------------
void MBodyEditColumn::SizeInChars::set(System::Drawing::Size value)
{
	if (
		m_pColumnInfo &&
		value != System::Drawing::Size::Empty &&
		value.Width > 0 &&
		value.Height > 0
		)
	{
		m_pColumnInfo->SetCtrlSize(value.Width, value.Height);
		NotifyBodyEditChanged();
	}
}

//----------------------------------------------------------------------------
System::String^ MBodyEditColumn::Tooltip::get()
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	if (pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndBodyColumnDescription)))
		return gcnew System::String(((CWndBodyColumnDescription*)pDesc)->m_strTooltip);
	return m_pColumnInfo ? gcnew System::String(m_pColumnInfo->GetToolTip()) : System::String::Empty;
}

//----------------------------------------------------------------------------
void MBodyEditColumn::Tooltip::set(System::String^ value)
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	if (pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndBodyColumnDescription)))
		((CWndBodyColumnDescription*)pDesc)->m_strTooltip = CString(value);
	if (m_pColumnInfo)
	{
		m_pColumnInfo->SetToolTip(CString(value));
		NotifyBodyEditChanged();
	}
}

//----------------------------------------------------------------------------
IDataType^ MBodyEditColumn::FilteredDataType::get()
{
	IDataType^ dataType = control != nullptr ? control->CompatibleType : nullptr;
	if (dataType == nullptr && DataBinding)
		dataType = DataBinding->DataType;

	return dataType;
}

//----------------------------------------------------------------------------
System::String^	MBodyEditColumn::GetSerializedName(System::String^ columnName)
{
	return System::String::Concat("col_", EasyBuilderSerializer::Escape(columnName));
}

//----------------------------------------------------------------------------
MBodyEdit^ MBodyEditColumn::BodyEdit::get()
{
	return bodyEdit;
}

//----------------------------------------------------------------------------
void MBodyEditColumn::BodyEdit::set(MBodyEdit^ value)
{
	bodyEdit = value; Parent = value->Parent;
}

//-----------------------------------------------------------------------------
void MBodyEditColumn::OnValueChanged(Object^ sender, EasyBuilderEventArgs^ args)
{
	this->ValueChanged(this, EasyBuilderEventArgs::Empty);
}

//----------------------------------------------------------------------------
bool MBodyEditColumn::TabStop::get()
{
	return true;
}

//----------------------------------------------------------------------------
void MBodyEditColumn::TabOrder::set(int value)
{
	int oldValue = ColPos;
	ColPos = value;
	if (Site != nullptr)
		PropertyChangingNotifier::OnComponentPropertyChanged(Site, this, "ColPos", oldValue, value);
}

//----------------------------------------------------------------------------
int MBodyEditColumn::TabOrder::get()
{
	return ColPos;
}

//----------------------------------------------------------------------------
void MBodyEditColumn::ShowHotLinkButton::set(bool value)
{
	if (control)
		((MParsedControl^)control)->ShowHotLinkButton = value;
}

//----------------------------------------------------------------------------
bool MBodyEditColumn::ShowHotLinkButton::get()
{
	return control ? ((MParsedControl^)control)->ShowHotLinkButton : true;
}

//----------------------------------------------------------------------------
void MBodyEditColumn::NotifyBodyEditChanged()
{
	// la notifica serve solo la prima volta per fa serializzare il bodyedit nella
	// CreateComponents del padre, quindi una volta fatto diventa ridondante
	if (BodyEdit != nullptr && BodyEdit->Site != nullptr && !BodyEdit->IsSerialized)
		PropertyChangingNotifier::OnComponentPropertyChanged(BodyEdit->Site, BodyEdit, "ColumnsCollection", nullptr, BodyEdit->Components);
}

//----------------------------------------------------------------------------
System::Collections::IList^ MBodyEditColumn::ItemsSource::get()
{
	return IsItemsSourceEditable ? ((IItemsSourceConsumer^)control)->ItemsSource : nullptr;
}

//----------------------------------------------------------------------------
void MBodyEditColumn::ItemsSource::set(System::Collections::IList^ value)
{
	if (IsItemsSourceEditable)
		((IItemsSourceConsumer^)control)->ItemsSource = value;
}

//----------------------------------------------------------------------------
void MBodyEditColumn::ItemSourceNs::set(String^ value)
{
	CWndBodyColumnDescription* pDescri = GetWndObjDescription();
	if (!pDescri)
		return;

	if (!pDescri->m_pItemSourceDescri)
		pDescri->m_pItemSourceDescri = new CItemSourceDescription;
	pDescri->m_pItemSourceDescri->m_strItemSourceNamespace = value;

}
//----------------------------------------------------------------------------
String^ MBodyEditColumn::ItemSourceNs::get()
{
	CWndBodyColumnDescription* pDescri = GetWndObjDescription();
	if (!pDescri)
		return "";

	return pDescri->m_pItemSourceDescri ? gcnew String(pDescri->m_pItemSourceDescri->m_strItemSourceNamespace) : "";
}

//----------------------------------------------------------------------------
String^ MBodyEditColumn::ItemSourceName::get()
{
	CWndBodyColumnDescription* pDescri = GetWndObjDescription();
	if (!pDescri)
		return "";

	return pDescri->m_pItemSourceDescri ? gcnew String(pDescri->m_pItemSourceDescri->m_strItemSourceName) : "";
}

//----------------------------------------------------------------------------
void MBodyEditColumn::ItemSourceName::set(String^ value)
{
	CWndBodyColumnDescription* pDescri = GetWndObjDescription();
	if (!pDescri)
		return;
	if (!pDescri->m_pItemSourceDescri)
		pDescri->m_pItemSourceDescri = new CItemSourceDescription;
	pDescri->m_pItemSourceDescri->m_strItemSourceName = value;
}

//----------------------------------------------------------------------------
bool MBodyEditColumn::IsItemsSourceEditable::get()
{
	return HasItemsSource ? ((IItemsSourceConsumer^)control)->IsItemsSourceEditable : false;
}

//----------------------------------------------------------------------------
void MBodyEditColumn::RefreshContentByDataType()
{
	if (HasItemsSource)
		((IItemsSourceConsumer^)control)->RefreshContentByDataType();
}

//----------------------------------------------------------------------------
bool MBodyEditColumn::HasItemsSource::get()
{
	return control != nullptr && IItemsSourceConsumer::typeid->IsInstanceOfType(control);
}

//----------------------------------------------------------------------------
bool MBodyEditColumn::ColumnHasFooter::get()
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	if (pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndBodyColumnDescription)))
		return ((CWndBodyColumnDescription*)pDesc)->m_bHasFooterDescr;
	if (m_pColumnInfo)
		return m_pColumnInfo->HasFooter() == TRUE;

	return	false;
}

//----------------------------------------------------------------------------
void MBodyEditColumn::ColumnHasFooter::set(bool value)
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	if (pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndBodyColumnDescription)))
		((CWndBodyColumnDescription*)pDesc)->m_bHasFooterDescr = value;
	if (m_pColumnInfo)
		m_pColumnInfo->SetHasFooter(value);
}
//----------------------------------------------------------------------------
bool MBodyEditColumn::AllowEnlarge::get()
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	if (pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndBodyColumnDescription)))
		return ((CWndBodyColumnDescription*)pDesc)->m_bAllowEnlarge;
	if (m_pColumnInfo)
		return m_pColumnInfo->GetAllowEnlarge() == TRUE;

	return	false;
}

//----------------------------------------------------------------------------
void MBodyEditColumn::AllowEnlarge::set(bool value)
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	if (pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndBodyColumnDescription)))
		((CWndBodyColumnDescription*)pDesc)->m_bAllowEnlarge = value;
	if (m_pColumnInfo)
		m_pColumnInfo->SetAllowEnlarge(value);
}


//----------------------------------------------------------------------------
bool MBodyEditColumn::Sort::get()
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	if (pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndBodyColumnDescription)))
		return ((CWndBodyColumnDescription*)pDesc)->m_bSort;
	return	false;
}

//----------------------------------------------------------------------------
void MBodyEditColumn::Sort::set(bool value)
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	if (pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndBodyColumnDescription)))
		((CWndBodyColumnDescription*)pDesc)->m_bSort = value;
}

//----------------------------------------------------------------------------
void MBodyEditColumn::AddOrRemoveStatus(int status, bool value)
{
	WORD wCurrStatus = m_pColumnInfo->GetStatus();
	if (value)
		wCurrStatus = wCurrStatus | status;
	else
		wCurrStatus = wCurrStatus & ~status;

	m_pColumnInfo->SetStatus(wCurrStatus);
	NotifyBodyEditChanged();
}

//----------------------------------------------------------------------------
bool MBodyEditColumn::IsGrayed::get()
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	if (pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndBodyColumnDescription)))
		return ((CWndBodyColumnDescription*)pDesc)->m_bStatusGrayed;
	if (m_pColumnInfo)
		return ((m_pColumnInfo->GetStatus() & STATUS_GRAYED) == STATUS_GRAYED);
	return	false;
}

//----------------------------------------------------------------------------
void MBodyEditColumn::IsGrayed::set(bool value)
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	if (pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndBodyColumnDescription)))
		((CWndBodyColumnDescription*)pDesc)->m_bStatusGrayed = value;
	else 
		AddOrRemoveStatus(STATUS_GRAYED, value);
}

//----------------------------------------------------------------------------
bool MBodyEditColumn::IsHidden::get()
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	if (pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndBodyColumnDescription)))
		return ((CWndBodyColumnDescription*)pDesc)->m_bStatusHidden;
	if (m_pColumnInfo)
		return ((m_pColumnInfo->GetStatus() & STATUS_HIDDEN) == STATUS_HIDDEN);
	return	false;
}

//----------------------------------------------------------------------------
void MBodyEditColumn::IsHidden::set(bool value)
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	if (pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndBodyColumnDescription)))
		((CWndBodyColumnDescription*)pDesc)->m_bStatusHidden = value;
	else
		AddOrRemoveStatus(STATUS_HIDDEN, value);
}

//----------------------------------------------------------------------------
bool MBodyEditColumn::IsNoChange_Grayed::get()
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	if (pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndBodyColumnDescription)))
		return ((CWndBodyColumnDescription*)pDesc)->m_bStatusNoChange_Grayed;
	if (m_pColumnInfo)
		return ((m_pColumnInfo->GetStatus() & STATUS_NOCHANGE_GRAYED) == STATUS_NOCHANGE_GRAYED);
	return	false;
}

//----------------------------------------------------------------------------
void MBodyEditColumn::IsNoChange_Grayed::set(bool value)
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	if (pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndBodyColumnDescription)))
		((CWndBodyColumnDescription*)pDesc)->m_bStatusNoChange_Grayed = value;
	else
		AddOrRemoveStatus(STATUS_NOCHANGE_GRAYED, value);
}

//----------------------------------------------------------------------------
bool MBodyEditColumn::IsNoChange_Hidden::get()
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	if (pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndBodyColumnDescription)))
		return ((CWndBodyColumnDescription*)pDesc)->m_bStatusNoChange_Hidden;
	if (m_pColumnInfo)
		return ((m_pColumnInfo->GetStatus() & STATUS_NOCHANGE_HIDDEN) == STATUS_NOCHANGE_HIDDEN);
	return	false;
}

//----------------------------------------------------------------------------
void MBodyEditColumn::IsNoChange_Hidden::set(bool value)
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	if (pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndBodyColumnDescription)))
		((CWndBodyColumnDescription*)pDesc)->m_bStatusNoChange_Hidden = value;
	else
		AddOrRemoveStatus(STATUS_NOCHANGE_HIDDEN, value);
}

//----------------------------------------------------------------------------
bool MBodyEditColumn::IsLocked::get()
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	if (pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndBodyColumnDescription)))
		return ((CWndBodyColumnDescription*)pDesc)->m_bStatusLocked;
	if (m_pColumnInfo)
		return ((m_pColumnInfo->GetStatus() & STATUS_LOCKED) == STATUS_LOCKED);
	return	false;
}

//----------------------------------------------------------------------------
void MBodyEditColumn::IsLocked::set(bool value)
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	if (pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndBodyColumnDescription)))
		((CWndBodyColumnDescription*)pDesc)->m_bStatusLocked = value;
	else
		AddOrRemoveStatus(STATUS_LOCKED, value);
}

//----------------------------------------------------------------------------
bool MBodyEditColumn::IsSortedDes::get()
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	if (pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndBodyColumnDescription)))
		return ((CWndBodyColumnDescription*)pDesc)->m_bStatusSortedDes;
	if (m_pColumnInfo)
		return ((m_pColumnInfo->GetStatus() & STATUS_SORTED_DESC) == STATUS_SORTED_DESC);
	return	false;
}

//----------------------------------------------------------------------------
void MBodyEditColumn::IsSortedDes::set(bool value)
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	if (pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndBodyColumnDescription)))
		((CWndBodyColumnDescription*)pDesc)->m_bStatusSortedDes = value;
	else
		AddOrRemoveStatus(STATUS_SORTED_DESC, value);
}

//----------------------------------------------------------------------------
bool MBodyEditColumn::IsSortedAsc::get()
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	if (pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndBodyColumnDescription)))
		return ((CWndBodyColumnDescription*)pDesc)->m_bStatusSortedAsc;
	if (m_pColumnInfo)
		return ((m_pColumnInfo->GetStatus() & STATUS_SORTED_ASC) == STATUS_SORTED_ASC);
	return	false;
}

//----------------------------------------------------------------------------
void MBodyEditColumn::IsSortedAsc::set(bool value)
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	if (pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndBodyColumnDescription)))
		((CWndBodyColumnDescription*)pDesc)->m_bStatusSortedAsc = value;
	else
		AddOrRemoveStatus(STATUS_SORTED_ASC, value);
}


//----------------------------------------------------------------------------
System::String^	MBodyEditColumn::Activation::get()
{
	CWndObjDescription* pDescri = GetWndObjDescription();
	if (!pDescri)
		return "";

	return gcnew String(pDescri->m_strActivation);
}
//----------------------------------------------------------------------------
void MBodyEditColumn::Activation::set(System::String^ value)
{
	CWndObjDescription* pDescri = GetWndObjDescription();
	if (!pDescri)
		return;
	pDescri->m_strActivation = value;
}

//----------------------------------------------------------------------------
MParsedControl^ MBodyEditColumn::Control::get()
{
	return control;
}

//----------------------------------------------------------------------------
void  MBodyEditColumn::Control::set(MParsedControl^ control)
{
	this->control = control;
}

//----------------------------------------------------------------------------
ComponentCollection^ MBodyEditColumn::Components::get()
{
	List<IComponent^>^ list = gcnew List<IComponent^>();

	if (Extensions != nullptr)
		for each (IComponent^ component in Extensions)
			if (component != nullptr)
				list->Add(component);

	return gcnew ComponentCollection(list->ToArray());
}

//----------------------------------------------------------------------------
IDataManager^ MBodyEditColumn::FixedDataManager::get()
{
	return bodyEdit && bodyEdit->DataBinding ? (IDataManager^)bodyEdit->DataBinding->Data : nullptr;
}

//----------------------------------------------------------------------------
IEasyBuilderComponentExtenders^ MBodyEditColumn::Extensions::get()
{
	if (DesignModeType == EDesignMode::Static)
		return nullptr;
	return control == nullptr ? nullptr : control->Extensions;
}


/////////////////////////////////////////////////////////////////////////////
// 						class MBodyEdit Implementation
/////////////////////////////////////////////////////////////////////////////

//----------------------------------------------------------------------------
MBodyEdit::MBodyEdit(System::IntPtr handleWndPtr)
	:
	BaseWindowWrapper(handleWndPtr)
{
	m_pBodyEdit = (CBodyEdit*)GetWnd();
	this->IsStretchable = true;
	components = gcnew List<MBodyEditColumn^>();
}

//----------------------------------------------------------------------------
MBodyEdit::MBodyEdit(IWindowWrapperContainer^ parentWindow, System::String^ name, System::String^ controlClass, Point location, bool hasCodeBehind)
	:
	BaseWindowWrapper(parentWindow, name, controlClass, location, hasCodeBehind)
{
	m_pBodyEdit = (CBodyEdit*)GetWnd();
	this->IsStretchable = true;
	components = gcnew List<MBodyEditColumn^>();
	// se passo da questo costruttore vuol dire che ne vengo dalla CreateComponents
	IsSerialized = true;

	Location = location;

	//if (!this->HasCodeBehind)
	//	jsonDescription = new CWndBodyDescription(NULL);
}

//-----------------------------------------------------------------------------
MBodyEdit::~MBodyEdit()
{
	this->!MBodyEdit();
	GC::SuppressFinalize(this);
}

//-----------------------------------------------------------------------------
MBodyEdit::!MBodyEdit()
{
	if (!m_pBodyEdit)
		return;

	m_pBodyEdit->SuspendLayout();
	// pulizia delle colonne per distruggere correttamente le nuove aggiunte
	for (int i = components->Count - 1; i >= 0; i--)
	{
		MBodyEditColumn^ column = (MBodyEditColumn^)components[i];
		Remove(column);
	}

	if (!HasCodeBehind)
	{
		CWnd* pParentWnd = m_pBodyEdit->GetParent();
		if (pParentWnd)
		{
			CParsedForm* pParsedForm = GetParsedForm(pParentWnd);
			if (pParsedForm)
				pParsedForm->GetControlLinks()->Remove(m_pBodyEdit);
		}
		m_pBodyEdit->DestroyWindow();
		delete m_pBodyEdit;
		m_pBodyEdit = NULL;
	}
	else //se devo distruggere il bodyedit non ha senso fare il resume del layout
	{
		m_pBodyEdit->ResumeLayout();
	}

	m_pBodyEdit = NULL;
}

//------------------------------------------------------------------------------------
void MBodyEdit::UpdateAttributesForJson(CWndObjDescription* pParentDescription)
{
	CWndBodyDescription* pBEDescription = NULL;
	bool hasNewColumns = false;

	ASSERT(pParentDescription);
	if (!pParentDescription)
		return;

	jsonDescription = pParentDescription->AddChildWindow(this->GetWnd(), this->Id);
	pBEDescription = dynamic_cast<CWndBodyDescription*>(jsonDescription);
	__super::UpdateAttributesForJson(pParentDescription);

	if (!jsonDescription)
		return;

	if (pBEDescription) //HasCodeBehind = FALSE
	{
		ASSERT(pBEDescription);
		if (!pBEDescription)
			return;

		pBEDescription->m_Width = this->Size.Width;
		pBEDescription->m_Height = this->Size.Height;
		//manage anchor
		pBEDescription->m_sAnchor = GetHorizontalIdAnchor();
		//TODO: clipChildren && ownerDraw
		//pBEDescription->Owne
		pBEDescription->m_bShowColumnHeaders = (Bool3)this->ShowColumnHeaders;
		pBEDescription->m_bShowHorizLines = (Bool3)this->ShowHorizLines;
		pBEDescription->m_bShowVertLines = (Bool3)this->ShowVertLines;
		pBEDescription->m_bShowHeaderToolbar = (Bool3)this->ShowHeaderToolbar;
		pBEDescription->m_bShowStatusBar = (Bool3)this->ShowStatusBar;
		pBEDescription->m_bAllowInsert = (Bool3)this->AllowInsert;
		pBEDescription->m_bAllowDelete = (Bool3)this->AllowDelete;

		//manage data binding
		if (pBEDescription->m_pBindings)
		{
			//exists => clear
			BindingInfo* pBindings = pBEDescription->m_pBindings;
			delete pBindings;
			pBEDescription->m_pBindings = nullptr;
		}
		if (this->DataBinding != nullptr)
		{
			//update databinding
			pBEDescription->m_pBindings = new BindingInfo();
			pBEDescription->m_pBindings->m_strDataSource = (NameSpace^)this->DataBinding->Name;//   Parent->Namespace;
		}

		//manage columns
		for (int i = pBEDescription->m_Children.GetUpperBound(); i >= 0; i--)
		{
			CWndBodyColumnDescription* pColumnDescription = dynamic_cast<CWndBodyColumnDescription*>(pBEDescription->m_Children.GetAt(i));
			if (!pColumnDescription)
				continue;

			MBodyEditColumn^ column = this->GetColumnByName(gcnew String(pColumnDescription->m_strName));
			if (column == nullptr)
				continue;

			if (!column->HasCodeBehind)
			{
				hasNewColumns = true;
				column->UpdateAttrForJson(pColumnDescription);
			}
			else //remove from parent
			{
				SAFE_DELETE(pBEDescription->m_Children.GetAt(i));
				pBEDescription->m_Children.RemoveAt(i);
			}

		}
	}

}

//-------------------------------------------------------------------------------------------------------------
void MBodyEdit::GenerateJsonForChildren(CWndObjDescription* pParentDescription, List<System::Tuple<System::String^, System::String^, System::Boolean>^>^ serialization)
{
	//no children container
}

//--------------------------------------------------------------------------------------------------------------
void MBodyEdit::GenerateSerialization(CWndObjDescription* pParentDescription, List<System::Tuple<System::String^, System::String^, System::Boolean>^>^ serialization)
{
	__super::GenerateSerialization(pParentDescription, serialization);

	bool hasNewColumns = false;
	for each (MBodyEditColumn^ column in this->ColumnsCollection)
	{
		if (!column->HasCodeBehind)
			hasNewColumns = true;

		column->GenerateJsonForEvents(serialization);
	}

	if (pParentDescription->IsKindOf(RUNTIME_CLASS(CDummyDescription))) //tile HasCodeBehind = true
	{
		//clear anyway from parent
		for (int i = pParentDescription->m_Children.GetUpperBound(); i >= 0; i--)
		{
			if (pParentDescription->m_Children.GetAt(i) == jsonDescription)
			{
				//SAFE_DELETE(pParentDescription->m_Children.GetAt(i));
				pParentDescription->m_Children.RemoveAt(i);
			}
		}

		if (!hasNewColumns)
		{
			for (int i = jsonDescription->m_Children.GetUpperBound(); i >= 0; i--)
				SAFE_DELETE(jsonDescription->m_Children.GetAt(i));
			jsonDescription->m_Children.RemoveAll();

			SAFE_DELETE(jsonDescription);
			return;
		}

		jsonDummyDescription = new CDummyDescription();
		jsonDummyDescription->m_Type = CWndObjDescription::WndObjType::Undefined;
		//manage differences
		if (!this->HasCodeBehind) //insert new BE
		{
			//all serialization
			jsonDummyDescription->m_strIds.Add(pParentDescription->m_strIds.GetAt(0));
			jsonDummyDescription->m_Children.Add(jsonDescription/*->DeepClone()*/);
			serialization->Add
			(
				gcnew Tuple<System::String^, System::String^, System::Boolean>
				(
					gcnew String(pParentDescription->m_strIds.GetAt(0) + _T("_") + this->Id),
					gcnew String(GetSerialization(jsonDummyDescription)),
					false
					)
			);

			for (int i = jsonDescription->m_Children.GetUpperBound(); i >= 0; i--)
				SAFE_DELETE(jsonDescription->m_Children.GetAt(i));
			jsonDescription->m_Children.RemoveAll();

			for (int i = jsonDummyDescription->m_Children.GetUpperBound(); i >= 0; i--)
				SAFE_DELETE(jsonDummyDescription->m_Children.GetAt(i));
			jsonDummyDescription->m_Children.RemoveAll();

			SAFE_DELETE(jsonDummyDescription);

		}
		else
		{
			//personalize existing BE => only columns - serialize them 
			jsonDummyDescription->m_strIds.Add(this->Id);
			for (int i = 0; i < jsonDescription->m_Children.GetCount(); i++)
			{
				CWndBodyColumnDescription* pColumnDescription = dynamic_cast<CWndBodyColumnDescription*>(jsonDescription->m_Children.GetAt(i));
				if (pColumnDescription)
					jsonDummyDescription->m_Children.Add(pColumnDescription->DeepClone());
			}
			
			//ClientForms
			serialization->Add
			(
				gcnew Tuple<System::String^, System::String^, System::Boolean>
				(
					gcnew String(pParentDescription->m_strIds.GetAt(0) + _T("_") + this->Id),
					gcnew String(GetSerialization(jsonDummyDescription)),
					true
					)
			);

			for (int i = jsonDescription->m_Children.GetUpperBound(); i >= 0; i--)
				SAFE_DELETE(jsonDescription->m_Children.GetAt(i));
			jsonDescription->m_Children.RemoveAll();
			SAFE_DELETE(jsonDescription);

			for (int i = jsonDummyDescription->m_Children.GetUpperBound(); i >= 0; i--)
				SAFE_DELETE(jsonDummyDescription->m_Children.GetAt(i));

			jsonDummyDescription->m_Children.RemoveAll();
			SAFE_DELETE(jsonDummyDescription);
		}
	}
}

//-----------------------------------------------------------------------------
void MBodyEdit::OnDesignerControlCreated()
{
	SizeLU = AdjustMinSizeOnParent(this, Parent);
}

//----------------------------------------------------------------------------
void MBodyEdit::OnMouseDown(Point p)
{
	if (!m_pBodyEdit)
		return;
	ScreenToClient(p);
	if (p.X < Size.Width/2)
		((CMyCrackingBodyEdit*)m_pBodyEdit)->DoMoveToPrevColCracked();
	else
		((CMyCrackingBodyEdit*)m_pBodyEdit)->DoMoveToNextColCracked();
}
//----------------------------------------------------------------------------
void MBodyEdit::Initialize()
{
	BaseWindowWrapper::Initialize();
	minSize = CUtility::GetIdealBodyEdiytSize();
}

//----------------------------------------------------------------------------
IWindowWrapper^ MBodyEdit::GetControl(System::IntPtr handle)
{
	for each (MBodyEditColumn^ column in components)
	{
		ColumnInfo* pColInfo = (ColumnInfo*)column->ColumnInfoPtr.ToInt64();
		if (pColInfo->GetParsedCtrl()->GetCtrlCWnd()->m_hWnd == (HWND)handle.ToInt64())
			return column;
	}
	return nullptr;
}
//----------------------------------------------------------------------------
void MBodyEdit::GetChildrenFromPos(
	System::Drawing::Point screenPoint,
	System::IntPtr handleToSkip,
	System::Collections::Generic::ICollection<IWindowWrapper^>^ foundChildren
	)
{
	ScreenToClient(screenPoint);
	CPoint point(screenPoint.X, screenPoint.Y);
	ColumnInfo* pCol = NULL;
	int nRowDBT = -1;
	DataObj* pCell = NULL;
	if (m_pBodyEdit->GetBodyItem(point, pCol, nRowDBT, pCell))
	{
		MBodyEditColumn^ col = GetColumnByInfo(pCol);
		if (col != nullptr)
			foundChildren->Add(col);
	}

}
//----------------------------------------------------------------------------
INameSpace^	MBodyEdit::Namespace::get()
{
	if (!m_pBodyEdit)
	{
		ASSERT(FALSE);
		return nullptr;
	}

	return gcnew NameSpace(gcnew System::String(m_pBodyEdit->GetNamespace().ToString()));
}

//----------------------------------------------------------------------------
System::String^	MBodyEdit::Name::get()
{
	if (!m_pBodyEdit)
	{
		ASSERT(FALSE);
		return nullptr;
	}

	return gcnew System::String(m_pBodyEdit->GetNamespace().GetObjectName());
}

//-----------------------------------------------------------------------------
void MBodyEdit::Name::set(System::String^ value)
{
	if (HasCodeBehind)
		return;

	// rinomina gli oggetti
	m_pBodyEdit->Rename(CString(value));
	CWndObjDescription* pDescri = GetWndObjDescription();
	if (pDescri && pDescri->m_strName)
		pDescri->m_strName= value;

	// riallinea i sites delle colonne con il nome corretto
	if (this->Site != nullptr && DesignMode)
		for each (EasyBuilderComponent^ column in components)
			PropertyChangingNotifier::OnComponentPropertyChanged(this->Site, column, EasyBuilderControlSerializer::ControlNamePropertyName, column->Name, column->Name);
}

//----------------------------------------------------------------------------
System::String^ MBodyEdit::Text::get()
{
	// no management
	return System::String::Empty;
}

//----------------------------------------------------------------------------
void MBodyEdit::Text::set(System::String^ value)
{
	// no management
}

//----------------------------------------------------------------------------
bool MBodyEdit::CanCreate()
{
	return !GetHandle();
}

//----------------------------------------------------------------------------
bool MBodyEdit::Create(IWindowWrapperContainer^ parentWindow, Point location, System::String^ className)
{
	if (parentWindow == nullptr || !CanCreate())
		return false;

	CWnd* pParentWnd = ((BaseWindowWrapper^)parentWindow)->GetWnd();

	CParsedForm* pParsedForm = GetParsedForm(pParentWnd);
	if (!pParsedForm)
		return false;

	CPoint aPt(
		abs(MulDiv(location.X, GetLogPixels(), SCALING_FACTOR)),
		abs(MulDiv(location.Y, GetLogPixels(), SCALING_FACTOR)) );

	CSize aSize(minSize.Width, minSize.Height);
	SafeMapDialog(pParentWnd->m_hWnd, aSize);//trasformo in pixel

	minSize.Width = abs(MulDiv(minSize.Width, GetLogPixels(), SCALING_FACTOR));
	minSize.Height = abs(MulDiv(minSize.Height, GetLogPixels(), SCALING_FACTOR));

	CString sFamilyName(MBodyEdit::typeid->FullName);
	CString sControlClassName(className);

	if (sControlClassName.IsEmpty())
		sControlClassName = AfxGetParsedControlsRegistry()->GetFamilyDefaultControl(sFamilyName);

	CRegisteredParsedCtrl* pCtrl = AfxGetParsedControlsRegistry()->GetRegisteredControl(sFamilyName, sControlClassName);
	if (!pCtrl)
		return false;

	DWORD styles = WS_CHILD | WS_VISIBLE | WS_TABSTOP | WS_CLIPCHILDREN | BS_OWNERDRAW;

	CTBNamespace aNamespace = CString(CreateNamespaceFromParent(parentWindow));
	UINT nID = AfxGetTBResourcesMap()->GetTbResourceID(aNamespace.GetObjectName(), TbControls);

	CButton* pButton = new CButton();
	if (!pButton->Create
		(
			_T(""),
			styles,
			CRect(aPt, aSize),
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

	m_pBodyEdit = ::AddLink
		(
			pParsedForm,
			pParentWnd,
			pParsedForm->GetControlLinks(),
			nID,
			NULL,
			RUNTIME_CLASS(CBodyEdit),
			NULL,
			_T(""),
			aNamespace.GetObjectName()
			);
	ASSERT(m_pBodyEdit->GetInfoOSL()->m_pParent == pParsedForm->GetInfoOSL());

	if (m_pBodyEdit)
	{
		Handle = (System::IntPtr) m_pBodyEdit->m_hWnd;
		HasCodeBehind = false;

		// without columns bodyedit auto-hides
		m_pBodyEdit->ShowWindow(SW_SHOW);
	}

	return m_pBodyEdit != NULL;
}

//----------------------------------------------------------------------------
System::String^ MBodyEdit::ClassName::get()
{
	return ClassType->ClassName;
}

//----------------------------------------------------------------------------
ControlClass^ MBodyEdit::ClassType::get()
{
	return gcnew ControlClass(this);
}

//----------------------------------------------------------------------------
int	MBodyEdit::GetNamespaceType()
{
	return CTBNamespace::GRID;
}

//----------------------------------------------------------------------------
IDataBinding^ MBodyEdit::DataBinding::get()
{
	if (dataBinding != nullptr || !m_pBodyEdit || !m_pBodyEdit->GetDBT() || Parent == nullptr)
		return dataBinding;

	MDBTObject^ dbt = nullptr;
	if (dataBinding == nullptr)
	{
		WindowWrapperContainer^ container = (WindowWrapperContainer^)Parent;
		if (container->Document == nullptr)
			return dataBinding;

		NameSpace^ nameSpace = gcnew NameSpace(gcnew System::String(m_pBodyEdit->GetDBT()->GetNamespace().ToString()));
		dbt = ((MDocument^)container->Document)->GetDBT(nameSpace);
		if (dbt != nullptr)
			dataBinding = gcnew DBTDataBinding(dbt);
	}
	else
		dbt = (MDBTObject^)dataBinding->Data;

	if (dbt != nullptr)
		dbt->AddReferencedBy(this->SerializedName);

	return dataBinding;
}

//----------------------------------------------------------------------------
void MBodyEdit::DataBinding::set(IDataBinding^ dataBinding)
{
	this->dataBinding = dataBinding;

	if (!m_pBodyEdit)
	{
		ASSERT(FALSE);
		return;
	}

	//se sto annullando il databinding, rimuovo eventuali componenti figli
	if (dataBinding == nullptr)
	{
		for (int i = components->Count - 1; i >= 0; i--)
			Remove(components[i]);

		m_pBodyEdit->SetDBT(NULL, FALSE);
		return;
	}

	DBTSlaveBuffered* pDBT = (DBTSlaveBuffered*)((MDBTObject^)dataBinding->Data)->GetDBTObject();
	if (!pDBT)
	{
		ASSERT(FALSE);
		return;
	}

	if (m_pBodyEdit->GetDBT() != pDBT)
	{
		// prima elimino il pre-esisente
		for (int i = components->Count - 1; i >= 0; i--)
			Remove(components[i]);

		m_pBodyEdit->SetDBT(pDBT, FALSE);
	}
}

//----------------------------------------------------------------------------
System::String^ MBodyEdit::SerializedType::get()
{
	return System::String::Concat("BE", EasyBuilderSerializer::Escape(Name));
}

//----------------------------------------------------------------------------
bool MBodyEdit::WndProc(Message% m)
{
	switch (m.Msg)
	{
	case UM_EASYBUILDER_ACTION:
	{
		EasyBuilderAction action = (EasyBuilderAction)(int)m.WParam;
		switch (action)
		{
		case Microarea::Framework::TBApplicationWrapper::RowChanged:
		{
			EasyBuilderEventArgs^ args = *(EasyBuilderEventArgs^*)(LPARAM)m.LParam.ToInt64();
			RowChanged(this, args);
			return true;//mangio il messaggio
		}
		}
		break;
	}
	}
	return __super::WndProc(m);
}

//-----------------------------------------------------------------------------
void MBodyEdit::Add(IComponent^ component)
{
	Add(component, nullptr);
}

//-----------------------------------------------------------------------------
void MBodyEdit::Add(System::ComponentModel::IComponent^ component, bool isChanged)
{
	if (component == nullptr)
		return;

	EasyBuilderComponent^ ebComp = dynamic_cast<EasyBuilderComponent^>(component);
	if(ebComp != nullptr)
		ebComp->IsChanged = isChanged;

	MBodyEditColumn^ column = dynamic_cast<MBodyEditColumn^>(component);
	if (column == nullptr)
	{
		Add(component, nullptr);
		return;
	}

	if (column->ColPos >= 0 && column->ColPos < components->Count)
		Insert(column->ColPos, column);
	else
		Add((IComponent^)column);

	column->ParentComponent = this;
	ColumnInfo* pColInfo = (ColumnInfo*)column->ColumnInfoPtr.ToInt64();
	ASSERT_VALID(pColInfo);
	pColInfo->GetInfoOSL()->m_pParent = m_pBodyEdit->GetInfoOSL();
}

//-----------------------------------------------------------------------------
void MBodyEdit::Insert(int nPos, MBodyEditColumn^ column)
{
	components->Insert(nPos, column);
	ITBComponentChangeService^ svc = nullptr;

	if (Site != nullptr)
		svc = (ITBComponentChangeService^)Site->GetService(ITBComponentChangeService::typeid);

	if (svc != nullptr)
		svc->OnComponentAdded(this, column);
}

//-----------------------------------------------------------------------------
void MBodyEdit::Add(IComponent^ component, System::String^ name)
{	
	if (name != nullptr)
		component->Site->Name = name;

	components->Add((MBodyEditColumn^)component);
	ITBComponentChangeService^ svc = nullptr;

	if (Site != nullptr)
		svc = (ITBComponentChangeService^)Site->GetService(ITBComponentChangeService::typeid);

	if (svc != nullptr)
		svc->OnComponentAdded(this, component);
}

//-----------------------------------------------------------------------------
MBodyEditColumn^ MBodyEdit::GetColumnByName(System::String^ name)
{
	for each (MBodyEditColumn^ column in components)
		if (System::String::Compare(column->Name, name, true) == 0)
			return column;

	return nullptr;
}

//-----------------------------------------------------------------------------
MBodyEditColumn^ MBodyEdit::GetColumnByInfo(ColumnInfo* pInfo)
{
	for each (MBodyEditColumn^ column in components)
	{
		ColumnInfo* pColInfo = (ColumnInfo*)column->ColumnInfoPtr.ToInt64();
		if (pColInfo == pInfo)
			return column;
	}
	return nullptr;
}

//-----------------------------------------------------------------------------
void MBodyEdit::Remove(IComponent^ component)
{
	if (!m_pBodyEdit)
		return;

	//Rimuove il component attuale dalla lista dei components del container
	MBodyEditColumn^ column = nullptr;
	if (component->GetType() == MBodyEditColumn::typeid)
	{
		column = (MBodyEditColumn^)component;

		ITBComponentChangeService^ svc = nullptr;

		if (!column->HasCodeBehind)
		{
			ColumnInfo* ci = (ColumnInfo*)column->ColumnInfoPtr.ToInt64();
			CParsedCtrl* pCtrl = ci->GetParsedCtrl();
			if (pCtrl->GetCtrlCWnd()->m_hWnd)
				pCtrl->GetCtrlCWnd()->DestroyWindow();
			m_pBodyEdit->RemoveColumn(ci);
		}
			

		//La remove è dentro l'if perchè components è una collection di MBodyEditColumn^
		//e quindi non ha senso rimuovere alcunchè e component non è MBodyEditColumn^
		components->Remove(column);

		// avverto della rimozione
		if (Site != nullptr)
			svc = (ITBComponentChangeService^)Site->GetService(ITBComponentChangeService::typeid);

		if (svc != nullptr)
			svc->OnComponentRemoved(this, component);

		delete column;
	}
}

//-----------------------------------------------------------------------------
System::Int32 MBodyEdit::TbHandle::get()
{
	return (long)m_pBodyEdit;
}

//-----------------------------------------------------------------------------
void MBodyEdit::AutoFillFromDataBinding(IDataBinding^ dataBinding, bool overrideExisting)
{
	if (!m_pBodyEdit || !dataBinding)
	{
		ASSERT(FALSE);
		return;
	}

	// controllo che il databinding sia 
	if (!overrideExisting && !IsDataBindingCompatibleForAutoFill(dataBinding))
		return;

	DBTDataBinding^		dbtBinding = nullptr;
	FieldDataBinding^	fieldBinding = nullptr;

	// prima mi occupo di assegnare le basi
	if (dataBinding->GetType() == DBTDataBinding::typeid)
	{
		dbtBinding = (DBTDataBinding^)dataBinding;
		DataBinding = dataBinding;
		((MDBTObject^)DataBinding->Data)->AddReferencedBy(this->SerializedName);
	}
	else if (dataBinding->GetType() == FieldDataBinding::typeid)
	{
		fieldBinding = (FieldDataBinding^)dataBinding;
		// il primo field aggancia anche il dbt
		if (DataBinding == nullptr)
		{
			// i controlli di correttezza li ha già eseguiti la IsDataBindingCompatibleForAutoFill
			DataBinding = gcnew DBTDataBinding((MDBTObject^)fieldBinding->Parent);
			((MDBTObject^)DataBinding->Data)->AddReferencedBy(this->SerializedName);
		}
	}

	if ((dbtBinding == nullptr && fieldBinding == nullptr) || DataBinding == nullptr || DataBinding->Data == nullptr)
		return;

	MDBTObject^ dbt = (MDBTObject^)DataBinding->Data;

	if (dbt->Record == nullptr)
		return;

	// ora genero le colonne
	MBodyEditColumn^ column = nullptr;

	if (fieldBinding != nullptr)
	{
		IRecordField^ field = dbt->Record->GetField((MDataObj^)fieldBinding->Data);
		if (field != nullptr && GetColumnByName(field->Name) == nullptr)
		{
			column = GenerateColumnFormField(field, false);
			if (column != nullptr)
				Add(column);
		}
	}
	else
	{
		components->Clear();
		// il dbt sostituisce tutte le colonne 
		int nCols = 0;
		for each (IRecordField^ field in dbt->Record->Fields)
		{
			column = GenerateColumnFormField(field, true);
			if (column != nullptr)
			{
				Add(column);
				nCols++;
			}

			// il drag&drop di tabelle da centinaia di colonne lo stronchiamo sul nascere
			if (nCols == 50)
				break;
		}
	}
}

//----------------------------------------------------------------------------
MBodyEditColumn^ MBodyEdit::GenerateColumnFormField(IRecordField^ field, bool isDroppingAllDbt)
{
	CString sFieldName(field->Name);

	if (
		isDroppingAllDbt &&
		(
			sFieldName.CompareNoCase(CREATED_COL_NAME) == 0 ||
			sFieldName.CompareNoCase(MODIFIED_COL_NAME) == 0 ||
			sFieldName.CompareNoCase(CREATED_ID_COL_NAME) == 0 ||
			sFieldName.CompareNoCase(MODIFIED_ID_COL_NAME) == 0
			)
		)
		return nullptr;

	MDataObj^ dataObj = (MDataObj^)field->DataObj;

	if (dataObj == nullptr || !dataObj->GetDataObj())
		return nullptr;

	MDBTObject^ dbt = (MDBTObject^)DataBinding->Data;

	// nascondo le colonne di FK solo se vengo dal drag&drop di un intero dbt
	bool isHidden = isDroppingAllDbt && ((MDBTSlaveBuffered^)dbt)->IsInForeignKey(field->Name);

	// il control deve nascere subito del datatype giusto x essere applicato alla colonna come si deve
	System::String^ controlName = System::String::Empty;
	Type^ type = MParsedControl::GetDefaultControlType(dataObj->DataType, false, controlName);

	MBodyEditColumn^ newCol = gcnew MBodyEditColumn(this, field->Name, controlName, Point(0, 0), false);
	newCol->Parent = this->Parent;
	newCol->DataBinding = gcnew FieldDataBinding(dataObj, dbt);
	newCol->AddChangedProperty("DataBinding");
	newCol->ColumnTitle = field->Name;
	newCol->AddChangedProperty("ColumnTitle");
	newCol->SizeInChars = System::Drawing::Size(dataObj->GetDataObj()->GetColumnLen(), 1);
	newCol->AddChangedProperty("SizeInChars");
	newCol->TitleBackColor = Color::Empty;
	newCol->AddChangedProperty("TitleBackColor");
	newCol->TitleTextColor = Color::Empty;
	newCol->AddChangedProperty("TitleTextColor");
	newCol->Visible = !isHidden;
	newCol->AddChangedProperty("Visible");
	newCol->ColPos = components->Count;
	newCol->AddChangedProperty("ColPos");

	PropertyChangingNotifier::OnComponentAdded(this, newCol, true);

	return newCol;
}

//----------------------------------------------------------------------------
void MBodyEdit::CallCreateComponents()
{
	// ripulisco e faccio scattare il caricamento delle colonne programmative
	// prima di iniziare il wrapping
	if (!HasCodeBehind && m_pBodyEdit && m_pBodyEdit->GetDocument()->m_pFormManager)
	{
		BodyEditInfo* pBEInfo = m_pBodyEdit->GetDocument()->m_pFormManager->GetBodyEditInfo(m_pBodyEdit->GetNamespace());
		if (!pBEInfo)
		{
			pBEInfo = new BodyEditInfo(m_pBodyEdit->GetNamespace(), m_pBodyEdit->GetNamespace().GetObjectName());
			pBEInfo->m_bModified = TRUE;
			pBEInfo->m_bValid = TRUE;

			m_pBodyEdit->GetDocument()->m_pFormManager->AddBodyEditInfo(pBEInfo);
		}
	}

	if (m_pBodyEdit)
		m_pBodyEdit->SuspendLayout();

	//metodo derivato
	CreateComponents();
	ApplyResources();
	OnAfterCreateComponents();

	//aggiungo quelle che mancano (le wrapper non customizzate)
	if (DesignMode)
		CreateColumns(true);

	if (m_pBodyEdit)
	{
		m_pBodyEdit->RecalculateAllColPos();
		m_pBodyEdit->ResumeLayout();
	}
}

//-----------------------------------------------------------------------------
void MBodyEdit::CreateComponents()
{
	CreateColumns(false);
}
//-----------------------------------------------------------------------------
bool MBodyEdit::CreateWrappers(array<System::IntPtr>^ handlesToSkip)
{
	return CreateColumns(true);
}

//-----------------------------------------------------------------------------
bool MBodyEdit::CreateColumns(bool skipDuplicates)
{
	bool oneCreated = false;
	if (m_pBodyEdit)
		m_pBodyEdit->SuspendLayout();
	ColumnInfo* pColInfo = NULL;
	for (int i = 0; i < m_pBodyEdit->GetAllColumnsInfoNumber(); i++)
	{
		pColInfo = m_pBodyEdit->GetColumnFromIdx(i);
		if (skipDuplicates && GetColumnByInfo(pColInfo))
			continue;//esiste già, è una customizzata ed è stata creata dalla CreateComponents
		MBodyEditColumn^ beColumn = gcnew MBodyEditColumn((System::IntPtr) pColInfo);
		beColumn->BodyEdit = this;
		Add(beColumn);

	}
	if (m_pBodyEdit)
		m_pBodyEdit->ResumeLayout();
	return oneCreated;
}

//-----------------------------------------------------------------------------
void MBodyEdit::ApplyResources()
{
}

//-----------------------------------------------------------------------------
void MBodyEdit::ClearComponents()
{
	
}

//-----------------------------------------------------------------------------
bool MBodyEdit::CanDropData(IDataBinding^ dataBinding)
{
	return IsDataBindingCompatibleForAutoFill(dataBinding);
}

//-----------------------------------------------------------------------------
bool MBodyEdit::IsDataBindingCompatibleForAutoFill(IDataBinding^ dataBinding)
{
	if (dataBinding == nullptr)
		return false;

	// drop di un DBT
	DBTDataBinding^ dbtBinding = nullptr;
	if (dataBinding->GetType() == DBTDataBinding::typeid)
		dbtBinding = (DBTDataBinding^)dataBinding;

	if (dbtBinding != nullptr)
		return DataBinding == nullptr
		&&
		dbtBinding->Data != nullptr
		&&
		dbtBinding->Data->GetType()->IsSubclassOf(MDBTSlaveBuffered::typeid);

	// drop di un campo di DBT
	FieldDataBinding^ fieldBinding = nullptr;
	if (dataBinding->GetType() == FieldDataBinding::typeid)
		fieldBinding = (FieldDataBinding^)dataBinding;

	IDataManager^ fieldDbt = fieldBinding->Parent;
	if (fieldDbt == nullptr || !MDBTSlaveBuffered::typeid->IsInstanceOfType(fieldDbt))
		return false;

	dbtBinding = (DBTDataBinding^)DataBinding;

	return dbtBinding == nullptr || (dbtBinding->Data != nullptr && dbtBinding->Data == fieldDbt);
}

//-----------------------------------------------------------------------------
bool MBodyEdit::BottomStretch::get()
{
	return m_pBodyEdit && __super::BottomStretch;
}

//-----------------------------------------------------------------------------
void MBodyEdit::BottomStretch::set(bool value)
{
	if (!m_pBodyEdit)
		return;

	__super::BottomStretch = value;
}

//-----------------------------------------------------------------------------
bool MBodyEdit::RightStretch::get()
{
	return m_pBodyEdit && __super::RightStretch;
}

//-----------------------------------------------------------------------------
void MBodyEdit::RightStretch::set(bool value)
{
	if (!m_pBodyEdit)
		return;

	__super::RightStretch = value;
}

//-----------------------------------------------------------------------------
bool MBodyEdit::AutoFill::get()
{
	return m_pBodyEdit && __super::AutoFill;
}

//-----------------------------------------------------------------------------
void MBodyEdit::AutoFill::set(bool value)
{
	if (!m_pBodyEdit)
		return;

	__super::AutoFill = value;

}

//-----------------------------------------------------------------------------
void MBodyEdit::StandardTabOrder::set(bool value)
{
	if (!m_pBodyEdit || !HasCodeBehind || Site == nullptr || !DesignMode)
		return;

	// per prima cosa riordina in origine
	m_pBodyEdit->RecalcColumnsOrder();

	// quindi crea un nuovo array di components con l'ordine nuovo ma le istanze uguali
	List<MBodyEditColumn^>^ newComponents = gcnew List<MBodyEditColumn^>();
	ColumnInfo* pColInfo = NULL;

	for (int i = 0; i < m_pBodyEdit->GetAllColumnsInfoNumber(); i++)
	{
		pColInfo = m_pBodyEdit->GetColumnFromIdx(i);
		MBodyEditColumn^ beColumn = GetColumnByInfo(pColInfo);
		if (beColumn != nullptr)
		{
			// notifico anche il ritorno all'originale nelle proprietà cambiate
			beColumn->RemoveChangedProperty("ColPos");
			newComponents->Add(beColumn);
		}
	}

	// lo sostituisce e serializza
	components = newComponents;
	PropertyChangingNotifier::OnComponentPropertyChanged(Site, this, "ColumnsCollection", nullptr, components);
}

//-----------------------------------------------------------------------------
bool MBodyEdit::StandardTabOrder::get()
{
	return false;
}


//-----------------------------------------------------------------------------
bool MBodyEdit::EnableInsertRow::get()
{
	return m_pBodyEdit ? m_pBodyEdit->CanInsertRow() == TRUE : false;

}

//-----------------------------------------------------------------------------
void MBodyEdit::EnableInsertRow::set(bool value)
{
	if (m_pBodyEdit)
		m_pBodyEdit->EnableInsertRow(value);
}
//-----------------------------------------------------------------------------
bool MBodyEdit::EnableAddRow::get()
{
	return m_pBodyEdit ? m_pBodyEdit->CanAddRow() == TRUE : false;

}

//-----------------------------------------------------------------------------
void MBodyEdit::EnableAddRow::set(bool value)
{
	if (m_pBodyEdit)
		m_pBodyEdit->EnableAddRow(value);
}
//-----------------------------------------------------------------------------
bool MBodyEdit::EnableDeleteRow::get()
{
	return m_pBodyEdit ? m_pBodyEdit->CanDeleteRow() == TRUE : false;
}

//-----------------------------------------------------------------------------
void MBodyEdit::EnableDeleteRow::set(bool value)
{
	if (m_pBodyEdit)
		m_pBodyEdit->EnableDeleteRow(value);
}

//-----------------------------------------------------------------------------
bool MBodyEdit::EnableFormViewCall::get()
{
	return m_pBodyEdit ? m_pBodyEdit->CanCallFormView() == TRUE : false;

}

//-----------------------------------------------------------------------------
void MBodyEdit::EnableFormViewCall::set(bool value)
{
	if (m_pBodyEdit)
		m_pBodyEdit->EnableFormViewCall(value);
}

//----------------------------------------------------------------------------
bool MBodyEdit::CanDropTarget(Type^ droppedObject)
{
	return CanUpdateTarget(droppedObject);
}
//----------------------------------------------------------------------------
bool MBodyEdit::CanUpdateTarget(Type^ droppedObject)
{
	if (DesignModeType == EDesignMode::Static)
		return MParsedControl::typeid->IsAssignableFrom(droppedObject);
	return false;
}
//----------------------------------------------------------------------------
void MBodyEdit::UpdateTargetFromDrop(Type^ droppedType)
{
	MBodyEditColumn^ newCol = gcnew MBodyEditColumn(this, droppedType);
	newCol->Parent = this->Parent;
	//newCol->AddChangedProperty("DataBinding");
	//newCol->ColumnTitle = c->ColumnTitle;
	//newCol->AddChangedProperty("ColumnTitle");
	//newCol->SizeInChars = c->SizeInChars;
	//newCol->AddChangedProperty("SizeInChars");
	newCol->Visible = true;
	newCol->AddChangedProperty("Visible");
	newCol->ColPos = components->Count;
	newCol->AddChangedProperty("ColPos");
	Add(newCol);

	PropertyChangingNotifier::OnComponentPropertyChanged(this->Site, this, "ColumnsCollection", nullptr, nullptr);
}

//----------------------------------------------------------------------------
void MBodyEdit::DataSource::set(String^ dataSource)
{
	CWndObjDescription* pDescri = GetWndObjDescription();
	if (pDescri)
	{
		if (!pDescri->m_pBindings)
			pDescri->m_pBindings = new BindingInfo;
		pDescri->m_pBindings->m_strDataSource = dataSource;
	}

}
//----------------------------------------------------------------------------
String^ MBodyEdit::DataSource::get()
{
	CWndObjDescription* pDescri = GetWndObjDescription();
	if (pDescri && pDescri->m_pBindings)
		return gcnew String(pDescri->m_pBindings->m_strDataSource);

	return "";
}

//----------------------------------------------------------------------------
String^ MBodyEdit::RowView::get()
{
	CWndObjDescription* pDescri = GetWndObjDescription();
	if (pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CWndBodyDescription)))
		return gcnew String(((CWndBodyDescription*)pDescri)->m_strRowView);

	return "";
}

//----------------------------------------------------------------------------
void MBodyEdit::RowView::set(String^ value)
{
	CWndObjDescription* pDescri = GetWndObjDescription();
	if (pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CWndBodyDescription)))
	{
		((CWndBodyDescription*)pDescri)->m_strRowView = value;
	}

}


//-----------------------------------------------------------------------------
EBool MBodyEdit::AllowCallDialog::get()
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	return pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndBodyDescription)) ? (EBool)((CWndBodyDescription*)pDesc)->m_bAllowCallDialog : EBool::Undefined;
}
//-----------------------------------------------------------------------------                                                                                                                                                             
EBool MBodyEdit::AllowDelete::get()
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	return pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndBodyDescription)) ? (EBool)((CWndBodyDescription*)pDesc)->m_bAllowDelete : EBool::Undefined;
}
//-----------------------------------------------------------------------------                                                                                                                                                             
EBool MBodyEdit::AllowInsert::get()
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	return pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndBodyDescription)) ? (EBool)((CWndBodyDescription*)pDesc)->m_bAllowInsert : EBool::Undefined;
}

//-----------------------------------------------------------------------------
EBool MBodyEdit::AllowOrdering::get()
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	return pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndBodyDescription)) ? (EBool)((CWndBodyDescription*)pDesc)->m_bAllowOrdering : EBool::Undefined;
}
//-----------------------------------------------------------------------------                                                                                                                                                             
EBool MBodyEdit::AllowOrderingOnBrowse::get()
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	return pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndBodyDescription)) ? (EBool)((CWndBodyDescription*)pDesc)->m_bAllowOrderingOnBrowse : EBool::Undefined;
}
//-----------------------------------------------------------------------------                                                                                                                                                             
EBool MBodyEdit::AllowOrderingOnEdit::get()
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	return pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndBodyDescription)) ? (EBool)((CWndBodyDescription*)pDesc)->m_bAllowOrderingOnEdit : EBool::Undefined;
}
//-----------------------------------------------------------------------------                                                                                                                                                             
EBool MBodyEdit::AllowMultipleSel::get()
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	return pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndBodyDescription)) ? (EBool)((CWndBodyDescription*)pDesc)->m_bAllowMultipleSel : EBool::Undefined;
}
//-----------------------------------------------------------------------------                                                                                                                                                             
EBool MBodyEdit::AllowRemoveColumnInteractive::get()
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	return pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndBodyDescription)) ? (EBool)((CWndBodyDescription*)pDesc)->m_bAllowRemoveColumnInteractive : EBool::Undefined;
}
//-----------------------------------------------------------------------------                                                                                                                                                             
EBool MBodyEdit::ChangeColor::get()
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	return pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndBodyDescription)) ? (EBool)((CWndBodyDescription*)pDesc)->m_bChangeColor : EBool::Undefined;
}
//-----------------------------------------------------------------------------                                                                                                                                                             
EBool MBodyEdit::EnlargeAllStringColumns::get()
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	return pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndBodyDescription)) ? (EBool)((CWndBodyDescription*)pDesc)->m_bEnlargeAllStringColumns : EBool::Undefined;
}
//-----------------------------------------------------------------------------                                                                                                                                                             
EBool MBodyEdit::EnlargeCustom::get()
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	return pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndBodyDescription)) ? (EBool)((CWndBodyDescription*)pDesc)->m_bEnlargeCustom : EBool::Undefined;
}
//-----------------------------------------------------------------------------                                                                                                                                                             
EBool MBodyEdit::EnlargeLastColumn::get()
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	return pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndBodyDescription)) ? (EBool)((CWndBodyDescription*)pDesc)->m_bEnlargeLastColumn : EBool::Undefined;
}
//-----------------------------------------------------------------------------                                                                                                                                                             
EBool MBodyEdit::EnlargeLastStringColumn::get()
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	return pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndBodyDescription)) ? (EBool)((CWndBodyDescription*)pDesc)->m_bEnlargeLastStringColumn : EBool::Undefined;
}
//-----------------------------------------------------------------------------                                                                                                                                                             
EBool MBodyEdit::ShowColumnHeaders::get()
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	return pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndBodyDescription)) ? (EBool)((CWndBodyDescription*)pDesc)->m_bShowColumnHeaders : EBool::Undefined;
}
//-----------------------------------------------------------------------------                                                                                                                                                             
EBool MBodyEdit::ShowVertScrollbar::get()
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	return pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndBodyDescription)) ? (EBool)((CWndBodyDescription*)pDesc)->m_bShowVertScrollbar : EBool::Undefined;
}
//-----------------------------------------------------------------------------                                                                                                                                                             
EBool MBodyEdit::ShowHorizScrollbar::get()
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	return pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndBodyDescription)) ? (EBool)((CWndBodyDescription*)pDesc)->m_bShowHorizScrollbar : EBool::Undefined;
}
//-----------------------------------------------------------------------------                                                                                                                                                             
EBool MBodyEdit::ShowFooterToolbar::get()
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	return pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndBodyDescription)) ? (EBool)((CWndBodyDescription*)pDesc)->m_bShowFooterToolbar : EBool::Undefined;
}
//-----------------------------------------------------------------------------                                                                                                                                                             
EBool MBodyEdit::ShowHeaderToolbar::get()
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	return pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndBodyDescription)) ? (EBool)((CWndBodyDescription*)pDesc)->m_bShowHeaderToolbar : EBool::Undefined;
}
//-----------------------------------------------------------------------------                                                                                                                                                             
EBool MBodyEdit::ShowHorizLines::get()
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	return pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndBodyDescription)) ? (EBool)((CWndBodyDescription*)pDesc)->m_bShowHorizLines : EBool::Undefined;
}
//-----------------------------------------------------------------------------                                                                                                                                                             
EBool MBodyEdit::ShowVertLines::get()
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	return pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndBodyDescription)) ? (EBool)((CWndBodyDescription*)pDesc)->m_bShowVertLines : EBool::Undefined;
}
//-----------------------------------------------------------------------------                                                                                                                                                             
EBool MBodyEdit::ShowBorders::get()
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	return pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndBodyDescription)) ? (EBool)((CWndBodyDescription*)pDesc)->m_bShowBorders : EBool::Undefined;
}

//----------------------------------------------------------------------------
bool MBodyEdit::Visible::get()
{
	return __super::Visible;
}

//----------------------------------------------------------------------------
void MBodyEdit::Visible::set(bool visible)
{
	__super::Visible = visible;
}

//-----------------------------------------------------------------------------                                                                                                                                                             
EBool MBodyEdit::AllowDrag::get()
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	return pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndBodyDescription)) ? (EBool)((CWndBodyDescription*)pDesc)->m_bAllowDrag : EBool::Undefined;
}
//-----------------------------------------------------------------------------                                                                                                                                                             
EBool MBodyEdit::AllowCopy::get()
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	return pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndBodyDescription)) ? (EBool)((CWndBodyDescription*)pDesc)->m_bAllowCopy : EBool::Undefined;
}
//-----------------------------------------------------------------------------                                                                                                                                                                  
EBool MBodyEdit::AllowDrop::get()
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	return pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndBodyDescription)) ? (EBool)((CWndBodyDescription*)pDesc)->m_bAllowDrop : EBool::Undefined;
}
//-----------------------------------------------------------------------------                                                                                                                                                                  
EBool MBodyEdit::AllowPaste::get()
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	return pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndBodyDescription)) ? (EBool)((CWndBodyDescription*)pDesc)->m_bAllowPaste : EBool::Undefined;
}
//-----------------------------------------------------------------------------                                                                                                                                                             
EBool MBodyEdit::AllowDragReadOnlyDoc::get()
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	return pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndBodyDescription)) ? (EBool)((CWndBodyDescription*)pDesc)->m_bAllowDragReadOnlyDoc : EBool::Undefined;
}
//-----------------------------------------------------------------------------                                                                                                                                                             
EBool MBodyEdit::AllowColumnLock::get()
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	return pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndBodyDescription)) ? (EBool)((CWndBodyDescription*)pDesc)->m_bAllowColumnLock : EBool::Undefined;
}
//-----------------------------------------------------------------------------                                                                                                                                                             
EBool MBodyEdit::AllowColumnLockInteractive::get()
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	return pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndBodyDescription)) ? (EBool)((CWndBodyDescription*)pDesc)->m_bAllowColumnLockInteractive : EBool::Undefined;
}
//-----------------------------------------------------------------------------                                                                                                                                                             
EBool MBodyEdit::AllowCustomize::get()
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	return pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndBodyDescription)) ? (EBool)((CWndBodyDescription*)pDesc)->m_bAllowCustomize : EBool::Undefined;
}
//-----------------------------------------------------------------------------                                                                                                                                                             
EBool MBodyEdit::AllowSearch::get()
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	return pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndBodyDescription)) ? (EBool)((CWndBodyDescription*)pDesc)->m_bAllowSearch : EBool::Undefined;
}

//-----------------------------------------------------------------------------                                                                                                                                                             
EBool MBodyEdit::ShowDataTip::get()
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	return pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndBodyDescription)) ? (EBool)((CWndBodyDescription*)pDesc)->m_bShowDataTip : EBool::Undefined;
}
//-----------------------------------------------------------------------------                                                                                                                                                                  
void MBodyEdit::ShowDataTip::set(EBool value)
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	if (pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndBodyDescription)))((CWndBodyDescription*)pDesc)->m_bShowDataTip = (Bool3)value;
}

//-----------------------------------------------------------------------------                                                                                                                                                             
EBool MBodyEdit::ShowStatusBar::get()
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	return pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndBodyDescription)) ? (EBool)((CWndBodyDescription*)pDesc)->m_bShowStatusBar : EBool::Undefined;
}
//-----------------------------------------------------------------------------                                                                                                                                                                  
void MBodyEdit::ShowStatusBar::set(EBool value)
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	if (pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndBodyDescription)))((CWndBodyDescription*)pDesc)->m_bShowStatusBar = (Bool3)value;
}


//-----------------------------------------------------------------------------
void MBodyEdit::AllowCallDialog::set(EBool value)
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	if (pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndBodyDescription)))((CWndBodyDescription*)pDesc)->m_bAllowCallDialog = (Bool3)value;
}
//-----------------------------------------------------------------------------                                                                                                                                                                  
void MBodyEdit::AllowDelete::set(EBool value)
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	if (pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndBodyDescription)))((CWndBodyDescription*)pDesc)->m_bAllowDelete = (Bool3)value;
}
//-----------------------------------------------------------------------------                                                                                                                                                                  
void MBodyEdit::AllowInsert::set(EBool value)
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	if (pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndBodyDescription)))((CWndBodyDescription*)pDesc)->m_bAllowInsert = (Bool3)value;
}
//-----------------------------------------------------------------------------                                                                                                                                                                  
void MBodyEdit::AllowOrdering::set(EBool value)
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	if (pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndBodyDescription)))((CWndBodyDescription*)pDesc)->m_bAllowOrdering = (Bool3)value;
}//-----------------------------------------------------------------------------                                                                                                                                                                  
void MBodyEdit::AllowOrderingOnBrowse::set(EBool value)
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	if (pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndBodyDescription)))((CWndBodyDescription*)pDesc)->m_bAllowOrderingOnBrowse = (Bool3)value;
}//-----------------------------------------------------------------------------                                                                                                                                                                  
void MBodyEdit::AllowOrderingOnEdit::set(EBool value)
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	if (pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndBodyDescription)))((CWndBodyDescription*)pDesc)->m_bAllowOrderingOnEdit = (Bool3)value;
}
//-----------------------------------------------------------------------------                                                                                                                                                                  
void MBodyEdit::AllowMultipleSel::set(EBool value)
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	if (pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndBodyDescription)))((CWndBodyDescription*)pDesc)->m_bAllowMultipleSel = (Bool3)value;
}
//-----------------------------------------------------------------------------                                                                                                                                                                  
void MBodyEdit::AllowRemoveColumnInteractive::set(EBool value)
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	if (pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndBodyDescription)))((CWndBodyDescription*)pDesc)->m_bAllowRemoveColumnInteractive = (Bool3)value;
}
//-----------------------------------------------------------------------------                                                                                                                                                                  
void MBodyEdit::ChangeColor::set(EBool value)
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	if (pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndBodyDescription)))((CWndBodyDescription*)pDesc)->m_bChangeColor = (Bool3)value;
}
//-----------------------------------------------------------------------------                                                                                                                                                                  
void MBodyEdit::EnlargeAllStringColumns::set(EBool value)
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	if (pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndBodyDescription)))((CWndBodyDescription*)pDesc)->m_bEnlargeAllStringColumns = (Bool3)value;
}
//-----------------------------------------------------------------------------                                                                                                                                                                  
void MBodyEdit::EnlargeCustom::set(EBool value)
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	if (pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndBodyDescription)))((CWndBodyDescription*)pDesc)->m_bEnlargeCustom = (Bool3)value;
}
//-----------------------------------------------------------------------------                                                                                                                                                                  
void MBodyEdit::EnlargeLastColumn::set(EBool value)
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	if (pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndBodyDescription)))((CWndBodyDescription*)pDesc)->m_bEnlargeLastColumn = (Bool3)value;
}
//-----------------------------------------------------------------------------                                                                                                                                                                  
void MBodyEdit::EnlargeLastStringColumn::set(EBool value)
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	if (pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndBodyDescription)))((CWndBodyDescription*)pDesc)->m_bEnlargeLastStringColumn = (Bool3)value;
}
//-----------------------------------------------------------------------------                                                                                                                                                                  
void MBodyEdit::ShowColumnHeaders::set(EBool value)
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	if (pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndBodyDescription)))((CWndBodyDescription*)pDesc)->m_bShowColumnHeaders = (Bool3)value;
}
//-----------------------------------------------------------------------------                                                                                                                                                                  
void MBodyEdit::ShowVertScrollbar::set(EBool value)
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	if (pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndBodyDescription)))((CWndBodyDescription*)pDesc)->m_bShowVertScrollbar = (Bool3)value;
}
//-----------------------------------------------------------------------------                                                                                                                                                                  
void MBodyEdit::ShowHorizScrollbar::set(EBool value)
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	if (pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndBodyDescription)))((CWndBodyDescription*)pDesc)->m_bShowHorizScrollbar = (Bool3)value;
}
//-----------------------------------------------------------------------------                                                                                                                                                                  
void MBodyEdit::ShowFooterToolbar::set(EBool value)
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	if (pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndBodyDescription)))((CWndBodyDescription*)pDesc)->m_bShowFooterToolbar = (Bool3)value;
}
//-----------------------------------------------------------------------------                                                                                                                                                                  
void MBodyEdit::ShowHeaderToolbar::set(EBool value)
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	if (pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndBodyDescription)))((CWndBodyDescription*)pDesc)->m_bShowHeaderToolbar = (Bool3)value;
}
//-----------------------------------------------------------------------------                                                                                                                                                                  
void MBodyEdit::ShowHorizLines::set(EBool value)
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	if (pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndBodyDescription)))((CWndBodyDescription*)pDesc)->m_bShowHorizLines = (Bool3)value;
}
//-----------------------------------------------------------------------------                                                                                                                                                                  
void MBodyEdit::ShowVertLines::set(EBool value)
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	if (pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndBodyDescription)))((CWndBodyDescription*)pDesc)->m_bShowVertLines = (Bool3)value;
}
//-----------------------------------------------------------------------------                                                                                                                                                                  
void MBodyEdit::ShowBorders::set(EBool value)
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	if (pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndBodyDescription)))((CWndBodyDescription*)pDesc)->m_bShowBorders = (Bool3)value;
}
//-----------------------------------------------------------------------------                                                                                                                                                                  
void MBodyEdit::AllowDrag::set(EBool value)
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	if (pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndBodyDescription)))((CWndBodyDescription*)pDesc)->m_bAllowDrag = (Bool3)value;
}
//-----------------------------------------------------------------------------                                                                                                                                                                  
void MBodyEdit::AllowCopy::set(EBool value)
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	if (pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndBodyDescription)))((CWndBodyDescription*)pDesc)->m_bAllowCopy = (Bool3)value;
}
//-----------------------------------------------------------------------------                                                                                                                                                                  
void MBodyEdit::AllowDrop::set(EBool value)
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	if (pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndBodyDescription)))((CWndBodyDescription*)pDesc)->m_bAllowDrop = (Bool3)value;
}
//-----------------------------------------------------------------------------                                                                                                                                                                  
void MBodyEdit::AllowPaste::set(EBool value)
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	if (pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndBodyDescription)))((CWndBodyDescription*)pDesc)->m_bAllowPaste = (Bool3)value;
}
//-----------------------------------------------------------------------------                                                                                                                                                                  
void MBodyEdit::AllowDragReadOnlyDoc::set(EBool value)
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	if (pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndBodyDescription)))((CWndBodyDescription*)pDesc)->m_bAllowDragReadOnlyDoc = (Bool3)value;
}
//-----------------------------------------------------------------------------                                                                                                                                                                  
void MBodyEdit::AllowColumnLock::set(EBool value)
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	if (pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndBodyDescription)))((CWndBodyDescription*)pDesc)->m_bAllowColumnLock = (Bool3)value;
}
//-----------------------------------------------------------------------------                                                                                                                                                                  
void MBodyEdit::AllowColumnLockInteractive::set(EBool value)
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	if (pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndBodyDescription)))((CWndBodyDescription*)pDesc)->m_bAllowColumnLockInteractive = (Bool3)value;
}
//-----------------------------------------------------------------------------                                                                                                                                                                  
void MBodyEdit::AllowCustomize::set(EBool value)
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	if (pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndBodyDescription)))((CWndBodyDescription*)pDesc)->m_bAllowCustomize = (Bool3)value;
}
//-----------------------------------------------------------------------------                                                                                                                                                                  
void MBodyEdit::AllowSearch::set(EBool value)
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	if (pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndBodyDescription)))((CWndBodyDescription*)pDesc)->m_bAllowSearch = (Bool3)value;
}

//----------------------------------------------------------------------------
double MBodyEdit::TitleRows::get()
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	if (pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndBodyDescription)))
		return ((CWndBodyDescription*)pDesc)->m_dTitleRows;

	return m_pBodyEdit ? m_pBodyEdit->GetUITitlesRows() : DEFAULT_TITLE_ROWS;
}

//----------------------------------------------------------------------------
void MBodyEdit::TitleRows::set(double value)
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	if (pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndBodyDescription)))
		((CWndBodyDescription*)pDesc)->m_dTitleRows = value;
	if (m_pBodyEdit)
		m_pBodyEdit->SetUITitlesRows(value);
}
//----------------------------------------------------------------------------
int MBodyEdit::MaxRecords::get()
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	if (pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndBodyDescription)))
		return ((CWndBodyDescription*)pDesc)->m_nMaxRecords;

	return m_pBodyEdit ? m_pBodyEdit->GetMaxRecords() : MAX_BODY_RECORDS;
}

//----------------------------------------------------------------------------
void MBodyEdit::MaxRecords::set(int value)
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	if (pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndBodyDescription)))
		((CWndBodyDescription*)pDesc)->m_nMaxRecords = value;
	if (m_pBodyEdit)
		m_pBodyEdit->SetMaxRecords(value);
}