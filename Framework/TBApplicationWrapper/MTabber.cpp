// cannot use C++ managed namespaces in order to avoid stdafx.h conflicts
#include "stdafx.h"
#include "windows.h"

#include <TbNameSolver/TBResourcesMap.h>
#include <TbOledb\SqlRec.h>
#include <TbGes\dbt.h>
#include <TbGes\Tabber.h>
#include <TbGes\TileManager.h>

#include "MToolbar.h"
#include "MBodyedit.h"
#include "MView.h"
#include "GenericControls.h"
#include "MTabber.h"

using namespace System;
using namespace System::Windows::Forms;
using namespace System::Drawing;
using namespace System::Collections::Generic;
using namespace System::Reflection;
using namespace System::CodeDom;
using namespace System::ComponentModel;
using namespace System::ComponentModel::Design;
using namespace System::ComponentModel::Design::Serialization;

using namespace ICSharpCode::NRefactory::CSharp;
using namespace ICSharpCode::NRefactory::PatternMatching;

/////////////////////////////////////////////////////////////////////////////
// 				class TabberSerializer Implementation
/////////////////////////////////////////////////////////////////////////////
//----------------------------------------------------------------------------	
Statement^ TabberSerializer::GetConstructor(IDesignerSerializationManager^ manager, EasyBuilderControl^ ebControl)
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
TypeDeclaration^ TabberSerializer::SerializeClass (SyntaxTree^ syntaxTree, IComponent^ control)
{
	//Se la classe custom che devo generare esiste già, non devo creare niente
	String^ className = ((EasyBuilderComponent^) control)->SerializedType;
	
	TypeDeclaration^ controller = GetControllerTypeDeclaration(syntaxTree);
	String^ controllerName = controller->Name;

	TypeDeclaration^ aClass = gcnew TypeDeclaration();
	aClass->Modifiers = Modifiers::Public;
	aClass->Name = className;
	aClass->BaseTypes->Add(gcnew SimpleType(MTabber::typeid->FullName));
		
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
// 				class TabSerializer Implementation
/////////////////////////////////////////////////////////////////////////////
//----------------------------------------------------------------------------	
Statement^ TabSerializer::GetConstructor(IDesignerSerializationManager^ manager, EasyBuilderControl^ ebControl)
{
	String^ className = ebControl->SerializedType;
	IdentifierExpression^ variableDeclExpression = gcnew IdentifierExpression(ebControl->SerializedName);
	
	//A seconda del fatto che la tab abbia code behind, viene invocato il costruttore a due parametri (tab esistenti)
	//o a sei parametri (tab customizzate);
	ObjectCreateExpression^ creationExpression = nullptr;
	if (ebControl->HasCodeBehind)
	{
		creationExpression =
			AstFacilities::GetObjectCreationExpression
			(
				gcnew SimpleType(className),
				AstFacilities::GetInvocationExpression
				(
					gcnew ThisReferenceExpression(),
					GetInfoPtrMethodName,
					gcnew PrimitiveExpression(ebControl->Name)
				)
			);
	}
	else
	{
		Point currentLocation = GetLocationToSerialize(ebControl);
		creationExpression = 
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
	}

	SetExpression(manager, ebControl, variableDeclExpression, true);

	return AstFacilities::GetAssignmentStatement
		(
			variableDeclExpression,
			creationExpression
		);
}

//----------------------------------------------------------------------------	
TypeDeclaration^ TabSerializer::SerializeClass (SyntaxTree^ syntaxTree, IComponent^ control)
{
	//Se la classe custom che devo generare esiste già, non devo creare niente
	String^ className = ((EasyBuilderComponent^) control)->SerializedType;

	TypeDeclaration^ controller = GetControllerTypeDeclaration(syntaxTree);
	String^ controllerName = controller->Name;

	TypeDeclaration^ aClass = gcnew TypeDeclaration();
	aClass->Modifiers = Modifiers::Public;
	aClass->Name = className;
	aClass->BaseTypes->Add(gcnew SimpleType(MTab::typeid->FullName));
		
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
		
	//Costruttore a 2 parametri, per oggetti wrappati su control di mago
	ConstructorDeclaration^ constr2 = gcnew ConstructorDeclaration();
	constr2->Modifiers = Modifiers::Public;
	constr2->Name = aClass->Name;
	aClass->Members->Add(constr2);
	constr2->Parameters->Add(gcnew ParameterDeclaration(gcnew SimpleType(IntPtr::typeid->FullName), WrappedObjectVariableName, ICSharpCode::NRefactory::CSharp::ParameterModifier::None));

	constr2->Body = gcnew BlockStatement();
	constr2->Initializer = AstFacilities::GetConstructorInitializer(			
		gcnew IdentifierExpression(WrappedObjectVariableName)
		);

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

/// <summary>
/// Internal Use
/// </summary>
void TabSerializer::SerializePropertiesAndAddMethod (
	IDesignerSerializationManager^ manager,
	EasyBuilderControl^ ebControl,
	System::Collections::Generic::IList<Statement^>^ collection
	)
{
	collection->Add
	(
		AstFacilities::GetInvocationStatement
		(
			gcnew ThisReferenceExpression(),
			AddMethodName,
			gcnew IdentifierExpression(ebControl->SerializedName),
			gcnew PrimitiveExpression(ebControl->IsChanged)
		)
	);
	System::Collections::Generic::IList<Statement^>^ props = SerializeProperties(manager, ebControl, ebControl->SerializedName);
	if (props != nullptr)
	{
		for each (Statement^ item in props)
			collection->Add(item);
	}
}


/////////////////////////////////////////////////////////////////////////////
// 						class MTab Implementation
/////////////////////////////////////////////////////////////////////////////

//----------------------------------------------------------------------------
MTab::MTab (IntPtr infoPtr)
	:
	WindowWrapperContainer(nullptr, nullptr, nullptr, Point::Empty, true),
	m_pTabManager			(NULL)
{
	m_pInfo = (DlgInfoItem*) infoPtr.ToInt64();
	if (m_pInfo && m_pInfo->m_pBaseTabDlg)
		Handle = (IntPtr)m_pInfo->m_pBaseTabDlg->m_hWnd;// se ho una tab attiva, ne prendo l'handle
}

//----------------------------------------------------------------------------
MTab::MTab (IWindowWrapperContainer^ parentWindow, System::String^ name, System::String^ controlClass, Point location, bool hasCodeBehind)
	:
	WindowWrapperContainer	(parentWindow, name, controlClass, location, hasCodeBehind),
	m_pInfo					(NULL),
	m_pTabManager			(NULL)
{
	if (Handle != IntPtr::Zero && !m_pInfo)
		m_pInfo = ((CBaseTabDialog*) GetWnd())->GetDlgInfoItem();
}

//----------------------------------------------------------------------------
MTab::~MTab  ()
{
	this->!MTab();
	m_pInfo = NULL;
	m_pTabManager =  NULL;
}

//----------------------------------------------------------------------------
MTab::!MTab  ()
{
	//Se non ha code behind non c'è niente da distruggere
    if (HasCodeBehind || !m_pInfo || !m_pTabManager )
		return;

	int nPos = m_pTabManager->GetTabDialogPos(m_pInfo->GetNamespace());
    if (nPos < 0)
		return;

	m_pTabManager->DeleteTab(m_pInfo, FALSE);
}

//-----------------------------------------------------------------------------
void MTab::Parent::set(IWindowWrapperContainer^ value)
{ 
	__super::Parent = value;

	if (value == nullptr || !MTabber::typeid->IsInstanceOfType(value))
		return;

	m_pTabManager = ((MTabber^) value)->GetInnerTabManager();
}


//-----------------------------------------------------------------------------
int	MTab::GetNamespaceType ()
{
	return CTBNamespace::TABDLG;
}

//----------------------------------------------------------------------------
void MTab::Activate ()
{
	MTabber^ tabber = dynamic_cast<MTabber^>(Parent);
	if (!tabber || !tabber->GetInnerTabManager())
		return;

	CBaseTabManager* pTabManager = tabber->GetInnerTabManager();
	if (m_pInfo->m_pBaseTabDlg == NULL || pTabManager->GetActiveDlg() != m_pInfo->m_pBaseTabDlg)
		pTabManager->TabDialogActivate(m_pInfo->GetDialogID());
}

//----------------------------------------------------------------------------
void MTab::CallCreateComponents ()
{
	__super::CallCreateComponents();

	if (!DesignMode)
		this->Displayed(this, EasyBuilderEventArgs::Empty);
}

//----------------------------------------------------------------------------
IntPtr MTab::Handle::get ()
{
	HWND hwnd = NULL;
	if (m_pInfo && m_pInfo->m_pBaseTabDlg)
		hwnd = m_pInfo->m_pBaseTabDlg->m_hWnd; //lazy initialization: se ho una tab attiva, ne prendo l'handle
	
	if ((HWND)(int)__super::Handle != hwnd)
		__super::Handle = (IntPtr)hwnd;

	return __super::Handle;
}

//----------------------------------------------------------------------------
bool MTab::CanCallCreateComponents()
{
	return m_pInfo && m_pInfo->m_pBaseTabDlg;
}

//----------------------------------------------------------------------------
System::String^	MTab::Name::get ()
{
	if (m_pInfo)
		return gcnew System::String (m_pInfo->GetNamespace().GetObjectName());
	
	return System::String::Empty;
}

//----------------------------------------------------------------------------
void MTab::Name::set (System::String^ value)
{
	//Per il momento se nella tab sono già stati droppati dei control, non posso rinominarla.
	if (Components && Components->Count == 0)
	{	
		CString sName = CString(value);
		m_pInfo->GetNamespace().SetObjectName(sName, TRUE);
		//La tabDialog associata al DlgInfoItem ha una variabile membro m_sName,
		//qui viene allineata con il nome impostato al namespace
		if (m_pInfo->m_pBaseTabDlg != nullptr)
			m_pInfo->m_pBaseTabDlg->SetFormName(sName);
		else
			Diagnostic->SetError((gcnew System::String(_TB("Unable to rename Tab Dialog: pointer m_pBaseTabDlg is null"))));
	}
	else
		Diagnostic->SetError((gcnew System::String(_TB("Unable to rename Tab Dialog: rename is enable only if the Tab does not contains controls"))));
}

//----------------------------------------------------------------------------
INameSpace^ MTab::Namespace::get ()
{
	System::String^ nameSpace = System::String::Empty;
	
	if (m_pInfo)
		nameSpace = gcnew System::String (m_pInfo->GetNamespace().ToString());
	
	return gcnew NameSpace(nameSpace);
}

//----------------------------------------------------------------------------
IntPtr MTab::DocumentPtr::get ()
{
	if (m_pInfo && m_pInfo->m_pBaseTabDlg)
		return (IntPtr) m_pInfo->m_pBaseTabDlg->GetDocument();

	return IntPtr::Zero;
}

//----------------------------------------------------------------------------
System::String^ MTab::Text::get ()
{
	if (!m_pInfo)
	{
		ASSERT(FALSE);
		return System::String::Empty;
	}	

	return gcnew System::String (m_pInfo->m_strTitle);
}

//----------------------------------------------------------------------------
System::String^ MTab::BackgroundImage::get ()
{ 
	return m_pInfo ? gcnew System::String(m_pInfo->m_strBkgndImage) : "";
}

//----------------------------------------------------------------------------
void MTab::BackgroundImage::set (System::String^ image) 
{ 
	if (!m_pInfo)
		return;
	m_pInfo->m_strBkgndImage = image;
	if (m_pInfo->m_pBaseTabDlg)
		m_pInfo->m_pBaseTabDlg->RefreshBackgroundImage();
}
//----------------------------------------------------------------------------
void MTab::Text::set (System::String^ value)
{
	if (!m_pInfo)
	{
		ASSERT(FALSE);
		return;
	}

	m_pInfo->m_strTitle = CString(value);

	CTabManager* pTabManager = NULL;

	if (m_pInfo->m_pBaseTabDlg)
		pTabManager = dynamic_cast<CTabManager*>(m_pInfo->m_pBaseTabDlg->GetParent());
	else if (!pTabManager && Parent != nullptr && Parent->GetType()->IsSubclassOf(MTabber::typeid))
		pTabManager = ((MTabber^)Parent)->GetInnerTabManager();

	if (pTabManager)
	{
		pTabManager->ChangeTabTitle(m_pInfo->GetDialogID(), m_pInfo->m_strTitle);
		pTabManager->GetNormalTabber()->SetTabLabel(m_pInfo->GetDialogID(), m_pInfo->m_strTitle);
	}
}

//----------------------------------------------------------------------------
bool MTab::CanCreate ()
{
	return !GetHandle();
}

//----------------------------------------------------------------------------
bool MTab::Equals (Object^ obj)
{
	if (
		obj == nullptr || 
		!(obj->GetType()->IsSubclassOf(MTab::typeid) || MTab::typeid->IsInstanceOfType(obj))
		)
		return false;
	
	MTab^ aTBTab = (MTab^)obj;

	if (m_pInfo && aTBTab->m_pInfo && m_pInfo == aTBTab->m_pInfo && m_pTabManager && aTBTab->m_pTabManager)
		return true;

	if (this->GetPtr() == aTBTab->GetPtr())
		return true;

	return __super::Equals(aTBTab);
}

//----------------------------------------------------------------------------
bool MTab::AreComponentsLoaded::get()
{
	// okkio che il tabber ha la gestione de
	if (m_pTabManager && !m_pInfo->m_pBaseTabDlg && IsChanged)
		return false;

	return __super::AreComponentsLoaded;
}

class CMyCrackingTabManager : public CTabManager
{
public:
	int InsertDlgInfoItemCracked(int pos, DlgInfoItem* item)
	{
		return InsertDlgInfoItem(pos, item);
	}
};

//----------------------------------------------------------------------------
bool MTab::Create (IWindowWrapperContainer^ parentWindow, Point location, System::String^ className)
{
	if (parentWindow == nullptr || !CanCreate())
		return false;

	CWnd* pParentWnd = ((BaseWindowWrapper^) parentWindow)->GetWnd();
	CPoint aPt(location.X, location.Y);

	CTabManager* pTabManager = NULL;
	CWnd* pTabManagerWnd = pParentWnd->IsKindOf(RUNTIME_CLASS(CTabDialog)) ? pParentWnd->GetParent() : pParentWnd;

	if (pTabManagerWnd->IsKindOf(RUNTIME_CLASS(CTabManager)))
		pTabManager = (CTabManager*) pTabManagerWnd;

	if (!pTabManager)
		return false;

	CSize aSize (minSize.Width, minSize.Height);
	SafeMapDialog(pParentWnd->m_hWnd, aSize);//trasformo in pixel
	CTBNamespace aNamespace = CString(CreateNamespaceFromParent(parentWindow));
	CString sName = CString(aNamespace.GetObjectName());
	UINT nIDC = AfxGetTBResourcesMap()->GetTbResourceID(sName, TbControls);
	
	m_pInfo = pTabManager->AddDialog 
		(
			IDD_EMPTY_TAB,
			aNamespace, 
			aNamespace.GetObjectName()
		);

	//la AddDialog non fa più la chiamata a InsertDlgInfoItem, la faccio qui
	//non posso mettere friend perché questa classe è managed, allora uso un trucchetto
	((CMyCrackingTabManager*)pTabManager)->InsertDlgInfoItemCracked(pTabManager->GetTabCount(), m_pInfo);

	if (m_pInfo)
	{
		m_pInfo->SetDialogID(nIDC);
		HasCodeBehind = false;
		m_pTabManager = pTabManager;

		this->Id = gcnew String(sName);
	}

	return m_pInfo != NULL;
}

//----------------------------------------------------------------------------
void MTab::Visible::set (bool value) 
{ 
	if (!m_pInfo)
		return;
	
	bool bCurrVisible = m_pInfo->IsVisible() == TRUE;

	if (bCurrVisible != value)
		m_pTabManager->TabDialogShow(m_pTabManager->GetDlgCtrlID(), m_pInfo->GetDialogID(), value);
}

//----------------------------------------------------------------------------
bool MTab::Visible::get () 
{ 
	return m_pInfo ? m_pInfo->IsVisible() == TRUE: false;
}

//----------------------------------------------------------------------------
bool MTab::Enabled::get () 
{ 
	return m_pInfo ? m_pInfo->IsEnabled() == TRUE : false;
}

//----------------------------------------------------------------------------
void MTab::Enabled::set (bool value) 
{ 
	if	(!m_pInfo || !m_pTabManager)
		return;

	bool bCurrEnabled = m_pInfo->IsEnabled() == TRUE;

	if (bCurrEnabled != value)
		m_pTabManager->TabDialogEnable(m_pTabManager->GetDlgCtrlID(), m_pInfo->GetDialogID(), value);
}

//----------------------------------------------------------------------------
int	MTab::TabIndex::get ()
{
	return	m_pTabManager ?
			m_pTabManager->GetTabDialogPos(m_pInfo->GetNamespace()) :
			NoActiveTabIndex;
}

//----------------------------------------------------------------------------
bool MTab::CanDropTarget (Type^ droppedObject)
{
	if (droppedObject->IsSubclassOf(MHotLink::typeid))
		return false;

	return MTab::typeid != droppedObject && 
			!droppedObject->IsSubclassOf(MTab::typeid) &&
			MTabber::typeid != droppedObject && 
			!droppedObject->IsSubclassOf(MTabber::typeid)  &&
			MToolbar::typeid != droppedObject;
}

//----------------------------------------------------------------------------
System::String^ MTab::SerializedName::get ()
{
	return System::String::Concat("tab_", EasyBuilderControlSerializer::Escape(Name));
}

//----------------------------------------------------------------------------
System::String^ MTab::SerializedType::get ()
{
	return System::String::Concat("Tab", EasyBuilderControlSerializer::Escape(Name));
}

//----------------------------------------------------------------------------
void MTab::Size::set(System::Drawing::Size value)
{
	__super::Size = value;
	if (m_pInfo && m_pInfo->m_pBaseTabDlg && DesignModeType != EDesignMode::Static)
	{
		m_pInfo->m_pBaseTabDlg->GetLayoutContainer()->RequestRelayout();
	}
}

//----------------------------------------------------------------------------
void MTab::OnControlsEnabled ()
{
	ControlsEnabled (this, gcnew EasyBuilderEventArgs());
}

//----------------------------------------------------------------------------
IntPtr MTab::GetChildFromOriginalPos (Point clientPosition, System::String^ controlClass)
{
	return m_pInfo && m_pInfo->m_pBaseTabDlg 
		? (IntPtr)(int)GetChildWindow(m_pInfo->m_pBaseTabDlg->m_hWnd, CString(controlClass), clientPosition, ((CTabDialog*)m_pInfo->m_pBaseTabDlg)->m_HWNDPositionsMap)
		: IntPtr::Zero;

}
//----------------------------------------------------------------------------
void MTab::SaveChildOriginalPos	(IntPtr hwndChild, Point clientPosition)
{
	if  (m_pInfo && m_pInfo->m_pBaseTabDlg)
		SaveChildWindowPos((HWND)hwndChild.ToInt64(), clientPosition, ((CTabDialog*)m_pInfo->m_pBaseTabDlg)->m_HWNDPositionsMap);
}


/////////////////////////////////////////////////////////////////////////////
// 						class MTabber Implementation
/////////////////////////////////////////////////////////////////////////////

//----------------------------------------------------------------------------
MTabber::MTabber (IntPtr handleWndPtr)
	: 
	WindowWrapperContainer(handleWndPtr)
{
	IsStretchable = true;
	HasCodeBehind = true;
	Handle = handleWndPtr;
	m_pTabber = (CTabManager*) GetWnd();
	if (m_pTabber)
		m_pTabber->InitTabNamespaces();
}

//----------------------------------------------------------------------------
MTabber::MTabber (IWindowWrapperContainer^ parentWindow, System::String^ name, System::String^ className, Point location, bool hasCodeBehind)
	:
	WindowWrapperContainer (parentWindow, name, className, location, hasCodeBehind)
{
	IsStretchable = true;
	m_pTabber = (CTabManager*) GetWnd();
	if (m_pTabber)
		m_pTabber->InitTabNamespaces();
}

//----------------------------------------------------------------------------
MTabber::~MTabber  ()
{
	this->!MTabber();
	m_pTabber = NULL;
}

//----------------------------------------------------------------------------
MTabber::!MTabber  ()
{
	if (HasCodeBehind || !m_pTabber)
		return;

	ClearComponents();
	
	CWnd* pParentWnd = m_pTabber->GetParent();
	if (pParentWnd->IsKindOf(RUNTIME_CLASS(CAbstractFormView)))
	{
		LayoutElement* pViewElement = m_pTabber->GetParentElement();
		if (pViewElement)
			pViewElement->RemoveChildElement(m_pTabber);

		CAbstractFormView* pView = (CAbstractFormView*)pParentWnd;
		//Rimuovo il tabber corrente dall'array di TabManagers
		int pos = pView->m_pTabManagers->GetTabManagerPos(CTBNamespace(this->Namespace->FullNameSpace));
		if (pos >= 0)
		{
			pView->m_pTabManagers->RemoveAt(pos);
		}
	}
	if (m_pTabber)
		m_pTabber->DestroyWindow();
}

//----------------------------------------------------------------------------
void MTabber::Initialize ()
{
	HasCodeBehind = true;
	minSize = CUtility::GetIdealTabberTileManagerSize();
}

//----------------------------------------------------------------------------
HWND MTabber::GetControlHandle (const CTBNamespace& aNamespace)
{
	CWnd* pWnd = m_pTabber 
		? m_pTabber->GetWndLinkedCtrl(aNamespace) 
		: NULL;
	return pWnd ? pWnd->m_hWnd : NULL;
}

//----------------------------------------------------------------------------
System::String^	MTabber::Name::get ()
{
	if (m_pTabber != nullptr)
		return gcnew System::String (m_pTabber->GetNamespace().GetObjectName());

	ASSERT(FALSE);
	return System::String::Empty;
}

//----------------------------------------------------------------------------
System::String^ MTabber::Text::get ()
{
	// no management
	return System::String::Empty;
}

//----------------------------------------------------------------------------
void MTabber::Text::set (System::String^ value)
{
	// no management
}

//----------------------------------------------------------------------------
INameSpace^	MTabber::Namespace::get () 
{ 
	if (m_pTabber != nullptr)
		return gcnew NameSpace(gcnew System::String (m_pTabber->GetNamespace().ToString()));

	ASSERT(FALSE);
	return nullptr;
} 

//----------------------------------------------------------------------------
void MTabber::Name::set (System::String^ value)
{
	if (!m_pTabber)
		return;

	if (Components->Count > 0)
		Diagnostic->SetError(gcnew System::String(_TB("Tabber cannot be renamed as it contains tab dialogs! Please remove all tabs.")));
	else
		m_pTabber->GetNamespace().SetObjectName(CString(value), TRUE);
}

//----------------------------------------------------------------------------
IWindowWrapper^	MTabber::GetControl (INameSpace^ nameSpace)
{
	for each (IComponent^ component in Components)
	{
		MTab^ tab = dynamic_cast<MTab^>(component);
		if (tab == nullptr)
			continue;

		if (System::String::Compare(tab->Namespace->ToString(), nameSpace->ToString(), true) == 0)
			return tab;
	}

	return WindowWrapperContainer::GetControl(nameSpace);
}

//----------------------------------------------------------------------------
bool MTabber::CanCreate ()
{
	return !GetHandle();
}

//----------------------------------------------------------------------------
bool MTabber::Equals(Object^ obj)
{
	if (
		obj == nullptr || 
		!(obj->GetType()->IsSubclassOf(MTabber::typeid) || MTabber::typeid->IsInstanceOfType(obj))
		)
		return false;

	MTabber^ aTBTabber = (MTabber^)obj;
	return this->GetPtr() == aTBTabber->GetPtr();
}

//----------------------------------------------------------------------------
bool MTabber::Create(IWindowWrapperContainer^ parentWindow, System::Drawing::Point location, System::String^ className)
{
	if (parentWindow == nullptr || !CanCreate())
		return false;

	CWnd* pParentWnd = ((BaseWindowWrapper^)parentWindow)->GetWnd();
	CPoint aPt(location.X, location.Y);

	CSize aSize(minSize.Width, minSize.Height);
	SafeMapDialog(pParentWnd->m_hWnd, aSize);//trasformo in pixel
	DWORD styles = WS_CHILD | WS_VISIBLE | WS_TABSTOP | WS_CLIPCHILDREN;

	CTBNamespace aNamespace = CString(CreateNamespaceFromParent(parentWindow));
	UINT nIDC = AfxGetTBResourcesMap()->GetTbResourceID(aNamespace.GetObjectName(), TbControls);

	CButton* pWnd = new CButton();
	if (!pWnd->Create
		(
		aNamespace.GetObjectName(),
		styles,
		CRect(aPt, aSize),
		pParentWnd,
		nIDC
		)
		)
	{
		delete pWnd;
		return FALSE;
	}

	

	if (pParentWnd->IsKindOf(RUNTIME_CLASS(CAbstractFormView)))
	{
		// tab manager destroys the window with all objects and and recreates it with another handle and default font!!
		m_pTabber = ((CAbstractFormView*)pParentWnd)->AddTabManager(nIDC, RUNTIME_CLASS(CTabManager), aNamespace.GetObjectName());
	
	}
	else 
	{
		CParsedForm* pDialog = GetParsedForm(pParentWnd);
		if (pDialog)
		{
			m_pTabber = (CTabManager*)((CMyCrackingDialog*)pDialog)->AddBaseTabManagerCracked(nIDC, RUNTIME_CLASS(CTabManager), aNamespace.GetObjectName());
		}
	}
	delete pWnd;
	if (!m_pTabber)
		return FALSE;

	// Il Runtime lo setta ad una dimensione piu' ragionevole
	// per evitare che lo dimensionino minuscolo
	if (DesignModeType == EDesignMode::Runtime)
	{
		m_pTabber->SetMinWidth(aSize.cx);
		m_pTabber->SetMaxWidth(LayoutElement::FREE);
		m_pTabber->SetMinHeight(aSize.cy);
	}

	Handle = (IntPtr)m_pTabber->m_hWnd;
	HasCodeBehind = false;
	m_pTabber->CreateNormalTabber();
	return TRUE;
}
//----------------------------------------------------------------------------
void MTabber::SwitchVisibility (bool bVisible)
{
	if (CurrentTab != nullptr)
		CurrentTab->SwitchVisibility(bVisible);
}

//----------------------------------------------------------------------------
bool MTabber::CreateWrappers(array<IntPtr>^ handlesToSkip)
{
	if (CurrentTab != nullptr)
		return CurrentTab->CreateWrappers(handlesToSkip);

	return false;
}

//----------------------------------------------------------------------------
//Inserisce in foundChildren tutti i controlli che contengono il punto screenPoint.
//Discrimina tra container e non container: i controlli NON container sono
//aggiunti per primi nella collezione, i controlli container sono aggiunti per ultimi.
void MTabber::GetChildrenFromPos(
	System::Drawing::Point screenPoint,
	IntPtr handleToSkip,
	System::Collections::Generic::ICollection<IWindowWrapper^>^ foundChildren
	)
{
	if (CurrentTab != nullptr)
	{
		HWND hHandleToSkip = (HWND)(int)handleToSkip;
		CPoint aPt(screenPoint.X, screenPoint.Y);
		HWND hChild = (HWND)CurrentTab->Handle.ToInt64();
		if (hChild != hHandleToSkip) 
		{
			CRect aRect;
			::GetWindowRect(hChild, &aRect);
			
			if (aRect.PtInRect(aPt))
			{
				// customized controls
				IWindowWrapper^ wrapper = GetControl((IntPtr) hChild);
				if (wrapper != nullptr)
					foundChildren->Add(wrapper);
			}
		}

		CurrentTab->GetChildrenFromPos(screenPoint, handleToSkip, foundChildren);
	}
}

//-----------------------------------------------------------------------------
int	MTabber::GetNamespaceType ()
{
	return CTBNamespace::TABBER;
}

//-----------------------------------------------------------------------------
void MTabber::AutoStretch::set(bool value)
{
	__super::AutoStretch = value;
}

//-----------------------------------------------------------------------------
bool MTabber::AutoFill::get()
{
	return __super::AutoFill;
}

//-----------------------------------------------------------------------------
void MTabber::AutoFill::set(bool value)
{
	if (!m_pTabber)
		return;

	__super::AutoFill = value;
}

//-----------------------------------------------------------------------------
System::Drawing::Size MTabber::Size::get()
{
	return __super::Size;
}

//-----------------------------------------------------------------------------
void MTabber::Size::set(System::Drawing::Size size)
{
	__super::Size = size;
}
//----------------------------------------------------------------------------
void MTabber::OnMouseDown (Point p)
{
	if (!m_pTabber)
		return;

	//se ho cliccato su una tab non faccio niente, è già gestita a monte, questo metodo serve solo 
	//per propagare il click ai pulsantini di scroll del tabber
	MTab^ tab = GetTabByPoint(gcnew Point(p.X, p.Y));
	if (tab)
		return;
		
	CPoint pt(p.X, p.Y);
	m_pTabber->ScreenToClient(&pt);
	m_pTabber->SendMessage(WM_LBUTTONDOWN, 0, MAKELPARAM(pt.x, pt.y));
}

//----------------------------------------------------------------------------
void MTabber::OnAfterCreateComponents()
{
	if (!m_pTabber || !m_pTabber->GetDlgInfoArray())
		return;

	DlgInfoArray* pArray = m_pTabber->GetDlgInfoArray();
	for (int i = 0; i < pArray->GetCount(); i++)
	{
		//i tilegroup li gestisce la classe derivata
		TileGroupInfoItem* pItem = dynamic_cast<TileGroupInfoItem*>(pArray->GetAt(i));
		if (pItem)
			continue;

		MTab^ tab = gcnew MTab((IntPtr)pArray->GetAt(i));
		tab->Parent = this;
		Add(tab);
	}
}

//-----------------------------------------------------------------------------
MTab^ MTabber::GetTabByName(System::String^ name)
{
	for each (IComponent^ component in Components)
	{
		MTab^ tab = dynamic_cast<MTab^>(component);
		if (tab == nullptr)
			continue;

		if (tab->Name == name)
			return tab;
	}

	return nullptr;
}
//-----------------------------------------------------------------------------
IntPtr MTabber::GetInfoPtr(System::String^ name)
{
	DlgInfoArray* pArray = 	m_pTabber->GetDlgInfoArray();
	for (int i = 0; i < pArray->GetCount(); i++)
	{
		DlgInfoItem* pItem = pArray->GetAt(i);
		if (pItem->GetNamespace().GetObjectName() == name)
			return (IntPtr)pItem;
	}
	return IntPtr::Zero;
}

//----------------------------------------------------------------------------
MTab^ MTabber::CurrentTab::get ()
{
	if (!m_pTabber)
		return nullptr;

	CBaseTabDialog* pActiveTab = m_pTabber->GetActiveDlg();
	if (!pActiveTab)
		return nullptr;
	
	System::String^ name = gcnew System::String (pActiveTab->GetFormName());
	for each (IComponent^ component in Components)
	{
		MTab^ tab = dynamic_cast<MTab^>(component);
		if (tab == nullptr)
			continue;

		if (tab->Name == name)
			return tab;
	}
	
	return nullptr;
}

//----------------------------------------------------------------------------
bool MTabber::KeepTabsAlive::get ()
{
	return m_pTabber ? m_pTabber->IsKeepingTabDlgAlive() == TRUE : false;
}

//----------------------------------------------------------------------------
void MTabber::KeepTabsAlive::set (bool value)
{
	if (m_pTabber)
		m_pTabber->SetKeepTabDlgAlive (value);
}

//----------------------------------------------------------------------------
int MTabber::MouseHitTestOnTab(Point^ mousePosition)
{
	TCHITTESTINFO hitTest;

	hitTest.pt = CPoint(mousePosition->X, mousePosition->Y);

	return m_pTabber-> HitTest(&hitTest);
}

//----------------------------------------------------------------------------
MTab^ MTabber::GetTabByPoint(Point^ p)
{
	TCHITTESTINFO hitTest;

	CPoint pt(p->X, p->Y);
	m_pTabber->ScreenToClient(&pt);
	hitTest.pt = pt;

	int hit = m_pTabber->HitTest(&hitTest);
	if (hit < 0)
		return nullptr;

	if (!m_pTabber->GetDlgInfoArray())
		return nullptr;
	
	DlgInfoItem* pItem = m_pTabber->GetDlgInfoArray()->GetAt(hit);
	if (!pItem)
		return nullptr;

	return GetTabByName(gcnew System::String(pItem->GetNamespace().GetObjectName()));
}

//----------------------------------------------------------------------------
bool MTabber::WndProc(Message% m)
{
	if (m.Msg == WM_DESTROY)
		m_pTabber = NULL;

	return __super::WndProc(m); 
}

//-----------------------------------------------------------------------------
void MTabber::SaveCurrentStatus (IDesignerCurrentStatus^ status)
{
	int nActiveTab = (CurrentTab == nullptr ? MTab::NoActiveTabIndex : CurrentTab->TabIndex);
	if (Namespace != nullptr)
		status[Namespace->FullNameSpace] = gcnew TabCurrentStatus(nActiveTab);
}

//-----------------------------------------------------------------------------
void MTabber::ApplyCurrentStatus (IDesignerCurrentStatus^ status)
{
	if (!m_pTabber)
		return;

	IDesignerCurrentStatusObject^ statusObject;
	if (
		!status->TryGetValue(Namespace->FullNameSpace, statusObject)
		||
		statusObject == nullptr 
		||
		TabCurrentStatus::typeid != statusObject->GetType())
		return;
	
	TabCurrentStatus^ currStatus = (TabCurrentStatus^) statusObject;

	if (currStatus != nullptr && currStatus->ActiveTab > MTab::NoActiveTabIndex)
	{
		UINT nID = m_pTabber->GetTabDialogIDD(currStatus->ActiveTab);
		m_pTabber->TabDialogActivate(m_pTabber->GetDlgCtrlID(), nID);
	}
}

//-----------------------------------------------------------------------------
void MTabber::OnDesignerControlCreated()
{
	KeepTabsAlive = true;
	SizeLU = AdjustMinSizeOnParent(this, Parent);
}

//----------------------------------------------------------------------------
bool MTabber::CanDropTarget (Type^ droppedObject)
{
	if (droppedObject->IsSubclassOf(MHotLink::typeid))
		return false;

	return	(MTab::typeid == droppedObject || droppedObject->IsSubclassOf(MTab::typeid));
}

//----------------------------------------------------------------------------
bool MTabber::CanDropData (IDataBinding^ dataBinding) 
{ 
	return false; //Il tabber non acetta drop di dati.
}

//----------------------------------------------------------------------------
System::String^ MTabber::SerializedName::get ()
{
	return System::String::Concat("tabber_", EasyBuilderControlSerializer::Escape(Name));
}

//----------------------------------------------------------------------------
System::String^ MTabber::SerializedType::get ()
{
	return System::String::Concat("Tabber", EasyBuilderControlSerializer::Escape(Name));
}

//----------------------------------------------------------------------------
void MTabber::CallCreateComponents ()
{
	if (!m_pTabber)
	{
		ASSERT(FALSE);
		return;
	}
	
	SuspendLayout(this, gcnew EasyBuilderEventArgs());

	// mi salvo dove e' posizionato il tabber
	m_nDefaultItemPos = GetActiveItemPos();
	
	if (DesignModeType == EDesignMode::Runtime)
		KeepTabsAlive = true;

	//creo i controlli del tabber
	CreateComponents();
	ApplyResources();

	if (DesignModeType == EDesignMode::Runtime)
		OnAfterCreateComponents();

	m_pTabber->InitTabNamespaces();

	bool atLeastOneCreated = false;
	//Creo i control per tutte le Tab, se attive
	for each (IComponent^ component in Components)
	{
		if (!IEasyBuilderContainer::typeid->IsInstanceOfType(component))
			continue;

		IEasyBuilderContainer^ ebContainer = (IEasyBuilderContainer^) component;
		
		// le tab devono essere alive
		CTabDialog* pDialog = m_pTabber->GetActiveDlg();
		if (!pDialog)
			continue;

		if (
				MTab::typeid->IsInstanceOfType(ebContainer) &&
				pDialog->GetDlgCtrlID() != ((MTab^) ebContainer)->GetWnd()->GetDlgCtrlID()
			)
			continue;

		// ora che ho il wrapping mi segno l'elemento di default
		if (pDialog && pDialog->GetCurrentTabPos() == m_nDefaultItemPos)
			DefaultItem = (BaseWindowWrapper^) component;

		ebContainer->CallCreateComponents();
		atLeastOneCreated = true;
	}

	//se non ho nemmeno una tab viva, vuol dire che non ho tab attiva
	//allora attivo la prima (la chiamata a CallCreateComponents mi arriverà
	//gratuitamente dal processo di attivazione)
	//questo codice è reso necessario dall'eliminazione della TabDialogActivate dalla create
	//della tab
	if (!atLeastOneCreated)
	{
		UINT nActiveTab = m_pTabber->GetDlgInfoArray()->GetSize() 
			? m_pTabber->GetDlgInfoArray()->GetAt(0)->GetDialogID()
			: 0;
		if (nActiveTab)
			m_pTabber->TabDialogActivate(m_pTabber->GetDlgCtrlID(), nActiveTab);
			m_pTabber->AdjustTabManager();
	}
}

//----------------------------------------------------------------------------
void MTabber::AfterTargetDrop(System::Type^ droppedType)
{
	m_pTabber->RequestRelayout();
}

//----------------------------------------------------------------------------
int MTabber::Flex::get()
{
	if (!m_pTabber)
		return -1;

	return  DesignerVisible ? flex :
		m_pTabber->GetFlex(LayoutElement::WIDTH);
}

//----------------------------------------------------------------------------
void MTabber::Flex::set(int value)
{
	if (!m_pTabber)
		return;
	flex = value;
	if (!DesignerVisible)
		m_pTabber->SetFlex(value);
}

//----------------------------------------------------------------------------
int MTabber::GetActiveItemPos()
{
	CTabDialog* pTabDialog = m_pTabber->GetActiveDlg();
	return pTabDialog ? pTabDialog->GetCurrentTabPos() : 0;
}

//----------------------------------------------------------------------------
void MTabber::ResumeDefaultLayout()
{
	ResumeLayout(this, gcnew EasyBuilderEventArgs());

	if (DefaultItem != nullptr)
	{
		DefaultItem->Activate();
		return;
	}

	if (m_nDefaultItemPos > m_pTabber->GetDlgInfoArray()->GetUpperBound())
		m_nDefaultItemPos = m_pTabber->GetDlgInfoArray()->GetUpperBound();

	if (m_nDefaultItemPos >= 0)
	{
		DlgInfoItem* pItem = m_pTabber->GetDlgInfoArray()->GetAt(m_nDefaultItemPos);
		m_pTabber->TabDialogActivate(pItem->GetNamespace().ToUnparsedString());
	}
	
}
