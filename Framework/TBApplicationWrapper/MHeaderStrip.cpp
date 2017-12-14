#include "stdafx.h"
#include "MHeaderStrip.h"
#include <TbGes\ExtDocView.h>
using namespace Microarea::Framework::TBApplicationWrapper;

using namespace System;
using namespace System::Collections::Generic;

using namespace ICSharpCode::NRefactory::CSharp;
using namespace ICSharpCode::NRefactory::PatternMatching;

/////////////////////////////////////////////////////////////////////////////
// 				class HeaderStripSerializer Implementation
/////////////////////////////////////////////////////////////////////////////
//----------------------------------------------------------------------------	

//----------------------------------------------------------------------------	
Statement^ HeaderStripSerializer::GetConstructor(IDesignerSerializationManager^ manager, EasyBuilderControl^ ebControl)
{
	Point currentLocation = GetLocationToSerialize(ebControl);
	String^ className = ebControl->SerializedType;
	IdentifierExpression^ variableDeclExpression = gcnew IdentifierExpression(ebControl->SerializedName);
	ObjectCreateExpression^ creationExpression =
		AstFacilities::GetObjectCreationExpression
		(
			gcnew SimpleType(className),
			gcnew ThisReferenceExpression(),
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
TypeDeclaration^ HeaderStripSerializer::SerializeClass(SyntaxTree^ syntaxTree, IComponent^ control)
{
	//Se la classe custom che devo generare esiste già, non devo creare niente
	String^ className = ((EasyBuilderComponent^)control)->SerializedType;

	TypeDeclaration^ controller = GetControllerTypeDeclaration(syntaxTree);
	String^ controllerName = controller->Name;

	TypeDeclaration^ aClass = gcnew TypeDeclaration();
	aClass->Modifiers = Modifiers::Public;
	aClass->Name = className;
	aClass->BaseTypes->Add(gcnew SimpleType(MHeaderStrip::typeid->FullName));

	//Costruttore a 6 parametri, per oggetti creati da zero in customizzazione
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

	//Cerca la proprieta' Document nella view (tanto e' uguale) e la aggiunge anche alla tab
	TypeDeclaration^ dec = FindClass(syntaxTree, ViewClassName);
	EntityDeclaration^ memberNode = nullptr;
	for each (EntityDeclaration^ current in dec->Members)
	{
		memberNode = dynamic_cast<EntityDeclaration^>(current);

		if (memberNode == nullptr || memberNode->Name != DocumentPropertyName)
			continue;

		memberNode = dynamic_cast<EntityDeclaration^>(memberNode->Clone());
		aClass->Members->Add(memberNode);
	}

	return aClass;
}

/////////////////////////////////////////////////////////////////////////////
// 				class MHeaderStrip Implementation
/////////////////////////////////////////////////////////////////////////////
//----------------------------------------------------------------------------	


//----------------------------------------------------------------------------
MHeaderStrip::MHeaderStrip(System::IntPtr handleWndPtr)
	:
	MTileGroup(handleWndPtr)
{
	ASSERT(GetWnd() && GetWnd()->IsKindOf(RUNTIME_CLASS(CHeaderStrip)));
	m_pHeader = (CHeaderStrip*)GetWnd();
	
}

//----------------------------------------------------------------------------
MHeaderStrip::MHeaderStrip(IWindowWrapperContainer^ parentWindow, System::String^ name, System::String^ controlClass, System::Drawing::Point location, bool hasCodeBehind)
	:
	MTileGroup(parentWindow, name, controlClass, location, hasCodeBehind)
{
}



//----------------------------------------------------------------------------
MHeaderStrip::~MHeaderStrip()
{
	this->!MHeaderStrip();
	GC::SuppressFinalize(this);
}

//-----------------------------------------------------------------------------
MHeaderStrip::!MHeaderStrip()
{
	if (!m_pHeader)
		return;

	if (!HasCodeBehind)
	{
		CWnd* pParentWnd = m_pHeader->GetParent();
		if (pParentWnd->IsKindOf(RUNTIME_CLASS(CAbstractFormView)))
		{
			CAbstractFormView* pView = (CAbstractFormView*)pParentWnd;
			pView->RemoveTileGroup(m_pHeader->GetDlgCtrlID());
		}
		m_pHeader = NULL;
		m_pTileGroup = NULL;
	}
	
	m_pHeader = NULL;
}

//----------------------------------------------------------------------------
int	MHeaderStrip::GetNamespaceType()
{
	return CTBNamespace::TABDLG;
}

//----------------------------------------------------------------------------
System::String^ MHeaderStrip::SerializedName::get()
{
	return System::String::Concat
		(
			"headerStrip_",
			EasyBuilderControlSerializer::Escape(
				String::IsNullOrEmpty(Name) ? Id : Name)
			);
}

//----------------------------------------------------------------------------
System::String^ MHeaderStrip::SerializedType::get()
{
	return System::String::Concat("HeaderStrip", EasyBuilderControlSerializer::Escape(Name));
}


//----------------------------------------------------------------------------
bool MHeaderStrip::Create(IWindowWrapperContainer^ parentWindow, System::Drawing::Point location, System::String^ className)
{
	CWnd* pParentWnd = ((BaseWindowWrapper^)parentWindow)->GetWnd();
	if (!pParentWnd || !pParentWnd->IsKindOf(RUNTIME_CLASS(CAbstractFormView)))
	{
		ASSERT(FALSE);
		return false;
	}

	CTBNamespace aNamespace = CreateNamespaceFromParent(parentWindow);

	CRect r(0, 0, parentWindow->Size.Width, 50);
	m_pHeader = ((CAbstractFormView*)pParentWnd)->AddHeaderStrip(AfxGetTBResourcesMap()->GetTbResourceID(CString(Id), TbControls), aNamespace.GetObjectName(), TRUE, r);
	m_pHeader->SetCaption(Text);
	//CTBNamespace::TABDLG
	m_pHeader->GetNamespace().SetNamespace(aNamespace);
	HasCodeBehind = false;
	m_pTileGroup = m_pHeader;
	Handle = (IntPtr)(int)m_pHeader->m_hWnd;
	return true;
}

//-----------------------------------------------------------------------------
void MHeaderStrip::OnAfterCreateComponents()
{

}

/*
//----------------------------------------------------------------------------
INameSpace^	MHeaderStrip::Namespace::get()
{
	return gcnew NameSpace(m_pHeader ? gcnew System::String(m_pHeader->GetNamespace().ToString()) : System::String::Empty);
}*/

//----------------------------------------------------------------------------
void MHeaderStrip::DataSource::set(String^ dataSource)
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
String^ MHeaderStrip::DataSource::get()
{
	CWndObjDescription* pDescri = GetWndObjDescription();
	if (pDescri && pDescri->m_pBindings)
		return gcnew String(pDescri->m_pBindings->m_strDataSource);

	return "";
}
