#include "stdafx.h"

#include <TbGes\TileManager.h>
#include <TbGes\TileDialog.h>
#include <TbGenlib\TilePanel.h>

#include "MTilePanel.h"
#include "MTileDialog.h"
#include "MTileManager.h"

using namespace Microarea::Framework::TBApplicationWrapper;
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
// 				class TilePanelSerializer Implementation
/////////////////////////////////////////////////////////////////////////////
//----------------------------------------------------------------------------	

//----------------------------------------------------------------------------	
Statement^ TilePanelSerializer::GetConstructor(IDesignerSerializationManager^ manager, EasyBuilderControl^ ebControl)
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
TypeDeclaration^ TilePanelSerializer::SerializeClass(SyntaxTree^ syntaxTree, IComponent^ control)
{
	//Se la classe custom che devo generare esiste già, non devo creare niente
	String^ className = ((EasyBuilderComponent^)control)->SerializedType;

	TypeDeclaration^ controller = GetControllerTypeDeclaration(syntaxTree);
	String^ controllerName = controller->Name;

	TypeDeclaration^ aClass = gcnew TypeDeclaration();
	aClass->Modifiers = Modifiers::Public;
	aClass->Name = className;
	aClass->BaseTypes->Add(gcnew SimpleType(MTilePanel::typeid->FullName));

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
// 				class TilePanelTabSerializer Implementation
/////////////////////////////////////////////////////////////////////////////
//----------------------------------------------------------------------------	

//----------------------------------------------------------------------------	
Statement^ TilePanelTabSerializer::GetConstructor(IDesignerSerializationManager^ manager, EasyBuilderControl^ ebControl)
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
TypeDeclaration^ TilePanelTabSerializer::SerializeClass(SyntaxTree^ syntaxTree, IComponent^ control)
{
	//Se la classe custom che devo generare esiste già, non devo creare niente
	String^ className = ((EasyBuilderComponent^)control)->SerializedType;
	
	TypeDeclaration^ controller = GetControllerTypeDeclaration(syntaxTree);
	String^ controllerName = controller->Name;

	TypeDeclaration^ aClass = gcnew TypeDeclaration();
	aClass->Modifiers = Modifiers::Public;
	aClass->Name = className;
	aClass->BaseTypes->Add(gcnew SimpleType(MTilePanelTab::typeid->FullName));

	//Costruttore a 6 parametri, per oggetti creati da zero in customizzazione
	ConstructorDeclaration^ constr = gcnew ConstructorDeclaration();
	constr->Modifiers = Modifiers::Public;
	constr->Name = aClass->Name,
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
// 						class MTilePanelTab Implementation
/////////////////////////////////////////////////////////////////////////////
MTilePanelTab::MTilePanelTab(System::IntPtr handlePtr)
	:
	WindowWrapperContainer(handlePtr),
	m_pPanelTabOwner (NULL)
{
	HasCodeBehind = true;
	m_pTilePanelTab = dynamic_cast<CTilePanelTab*>(GetWnd());
	if (m_pTilePanelTab)
		m_pPanelTabOwner = m_pTilePanelTab->GetTilePanel();
}

//----------------------------------------------------------------------------
MTilePanelTab::MTilePanelTab(IWindowWrapperContainer^ parentWindow, System::String^ name, System::String^ controlClass, Point location, bool hasCodeBehind)
	:
	WindowWrapperContainer(parentWindow, name, controlClass, location, hasCodeBehind),
	m_pPanelTabOwner(NULL),
	m_pTilePanelTab(NULL)
{
	m_pTilePanelTab = dynamic_cast<CTilePanelTab*>(GetWnd());
	if (m_pTilePanelTab)
		m_pPanelTabOwner = m_pTilePanelTab->GetTilePanel();
}

//----------------------------------------------------------------------------
MTilePanelTab::~MTilePanelTab()
{
	this->!MTilePanelTab();
}

//----------------------------------------------------------------------------
MTilePanelTab::!MTilePanelTab()
{
	if (HasCodeBehind || !m_pTilePanelTab || !m_pPanelTabOwner)
		return;

	m_pPanelTabOwner->RemoveTab(m_pTilePanelTab, FALSE);
	m_pTilePanelTab->DestroyWindow();
	SAFE_DELETE(m_pTilePanelTab);
}


//----------------------------------------------------------------------------
bool MTilePanelTab::Equals(Object^ obj)
{
	if (
		obj == nullptr ||
		!(obj->GetType()->IsSubclassOf(MTilePanelTab::typeid) || MTilePanelTab::typeid->IsInstanceOfType(obj))
		)
		return false;

	MTilePanelTab^ aTilePanel = (MTilePanelTab^)obj;
	return this->GetPtr() == aTilePanel->GetPtr();
}


//----------------------------------------------------------------------------
IntPtr MTilePanelTab::Handle::get()
{
	HWND hwnd = NULL;
	if (m_pTilePanelTab)
		hwnd = m_pTilePanelTab->m_hWnd; //lazy initialization: se ho una tab attiva, ne prendo l'handle

	if (!hwnd)
		return __super::Handle;

	if ((HWND)(int)__super::Handle != hwnd)
		__super::Handle = (IntPtr)hwnd;

	return __super::Handle;
}

//----------------------------------------------------------------------------
void MTilePanelTab::Name::set(System::String^ value)
{
	if (!m_pTilePanelTab)
		return;

	//TODOLUCA
	/*if (Tabs->Count > 0)
		Diagnostic->SetError(gcnew System::String(_TB("Tabber cannot be renamed as it contains tab dialogs! Please remove all tabs.")));
	else
		m_pTabber->GetNamespace().SetObjectName(CString(value), TRUE);*/

	m_pTilePanelTab->GetNamespace().SetObjectName(CString(value), TRUE);
}

//----------------------------------------------------------------------------
System::String^	MTilePanelTab::Name::get()
{
	if (m_pTilePanelTab)
		return gcnew System::String(m_pTilePanelTab->GetNamespace().GetObjectName());

	return System::String::Empty;
}

//----------------------------------------------------------------------------
void MTilePanelTab::AfterOwnEditingSwitching(bool onwEditingOn, bool IAmInEditing)
{
	if (m_pTilePanelTab)
		m_pTilePanelTab->GetTilePanel()->RequestRelayout();
}

//----------------------------------------------------------------------------
bool MTilePanelTab::Create(IWindowWrapperContainer^ parentWindow, Point location, System::String^ className)
{
	if (parentWindow == nullptr || !CanCreate())
		return false;

	CWnd* pParentWnd = ((BaseWindowWrapper^)parentWindow)->GetWnd();

	m_pPanelTabOwner = dynamic_cast<CTilePanel*>(pParentWnd);
	if (!m_pPanelTabOwner)
		return false;

	CSize aSize(minSize.Width, minSize.Height);
	SafeMapDialog(pParentWnd->m_hWnd, aSize);//trasformo in pixel
	CTBNamespace aNamespace = CString(CreateNamespaceFromParent(parentWindow));
	UINT nIDC = AfxGetTBResourcesMap()->GetTbResourceID(CString(aNamespace.GetObjectName()), TbControls);

	m_pTilePanelTab = m_pPanelTabOwner->AddTab(
		aNamespace.GetObjectName(),
		aNamespace.GetObjectName()
		);

	return m_pTilePanelTab != NULL;
}

//----------------------------------------------------------------------------
bool MTilePanelTab::CanCreate()
{
	return !GetHandle();
}

//----------------------------------------------------------------------------
void MTilePanelTab::OnAfterCreateComponents()
{
	if (!m_pTilePanelTab)
		return;

	CWnd* pCtrl = m_pTilePanelTab->GetWindow(GW_CHILD);
	for (; pCtrl; pCtrl = pCtrl->GetNextWindow())
	{
		CBaseTileDialog* pDialog = dynamic_cast<CBaseTileDialog*>(pCtrl);
		if (pDialog == nullptr)
			continue;

		MTileDialog^ tileDialog = gcnew MTileDialog((IntPtr)pDialog->m_hWnd);
		tileDialog->Parent = this;
		Add(tileDialog);
	}
}


//----------------------------------------------------------------------------
bool MTilePanelTab::CreateWrappers(array<IntPtr>^ handlesToSkip)
{
	bool changed = false;

	for each (IComponent^ component in Components)
	{
		MTileDialog^ tileDialog = dynamic_cast<MTileDialog^>(component);
		if (tileDialog == nullptr)
			continue;

		changed |= tileDialog->CreateWrappers(handlesToSkip);
	}

	return changed;
}

//----------------------------------------------------------------------------
bool MTilePanelTab::CanDropTarget(Type^ droppedObject)
{
	return
		(MTileDialog::typeid == droppedObject || droppedObject->IsSubclassOf(MTileDialog::typeid));
}


//-----------------------------------------------------------------------------
int	MTilePanelTab::GetNamespaceType()
{
	return CTBNamespace::TILEPANELTAB;
}

//----------------------------------------------------------------------------
System::String^ MTilePanelTab::SerializedName::get()
{
	
	return System::String::Concat("tilePanelTab_", EasyBuilderControlSerializer::Escape(Name));
}

//----------------------------------------------------------------------------
System::String^ MTilePanelTab::SerializedType::get()
{
	return System::String::Concat("TilePanelTab_", EasyBuilderControlSerializer::Escape(this->ParentComponent->Name), _T("_"), EasyBuilderControlSerializer::Escape(Name));
}

//----------------------------------------------------------------------------
INameSpace^ MTilePanelTab::Namespace::get()
{
	CTBNamespace aNs;
	if (m_pTilePanelTab)
		aNs = m_pTilePanelTab->GetNamespace();
	
	return gcnew NameSpace(gcnew System::String(aNs.ToString()));
}

//----------------------------------------------------------------------------
HWND MTilePanelTab::GetControlHandle(const CTBNamespace& aNamespace)
{
	CWnd* pWnd = m_pTilePanelTab
		? m_pTilePanelTab->GetWndLinkedCtrl(aNamespace)
		: NULL;
	return pWnd ? pWnd->m_hWnd : NULL;
}

//----------------------------------------------------------------------------
CLayoutContainer* MTilePanelTab::GetLayoutContainer()
{
	return m_pTilePanelTab ? m_pTilePanelTab->GetLayoutContainer() : NULL;
}

//-------------------------------------------------------------------------------
void MTilePanelTab::Invalidate()
{
	MTilePanel^ pParent = dynamic_cast<MTilePanel^>(Parent);

	if (pParent == nullptr)
		return;

	pParent->Invalidate();
}

//--------------------------------------------------------------------------------------
void MTilePanelTab::UpdateWindow()
{
	MTilePanel^ pParent = dynamic_cast<MTilePanel^>(Parent);

	if (pParent == nullptr)
		return;
	pParent->UpdateWindow();
}

/////////////////////////////////////////////////////////////////////////////
// 						class MTilePanel Implementation
/////////////////////////////////////////////////////////////////////////////

//----------------------------------------------------------------------------
MTilePanel::MTilePanel(IntPtr handleWndPtr)
	:
	WindowWrapperContainer(handleWndPtr),
	m_pTilePanel(NULL)
{
	HasCodeBehind = true;
	m_pTilePanel = dynamic_cast<CTilePanel*>(GetWnd());
}

//----------------------------------------------------------------------------
MTilePanel::MTilePanel(IWindowWrapperContainer^ parentWindow, System::String^ name, System::String^ className, Point location, bool hasCodeBehind)
	:
	WindowWrapperContainer(parentWindow, name, className, location, hasCodeBehind),
	m_pTilePanel(NULL)
{
	m_pTilePanel = dynamic_cast<CTilePanel*>(GetWnd());
}

//----------------------------------------------------------------------------
MTilePanel::~MTilePanel()
{
	this->!MTilePanel();
}

//----------------------------------------------------------------------------
MTilePanel::!MTilePanel()
{
	if (HasCodeBehind)
		return;

	if (m_pParentTileGroup)
	{
		TilePanelArray* pTilePanelArray = m_pParentTileGroup->GetTilePanels();
		for (int d = pTilePanelArray->GetCount() - 1; d >= 0; d--)
		{
			CTilePanel* pTilePanel = pTilePanelArray->GetAt(d);

			if (pTilePanel == m_pTilePanel)
			{
				m_pParentTileGroup->GetLayoutContainer()->RemoveChildElement(m_pTilePanel);
				pTilePanelArray->RemoveAt(d);
			}
		}
	}

	if (m_pTilePanel)
	{
		m_pTilePanel->DestroyWindow();
		SAFE_DELETE(m_pTilePanel);
	}
}

//-----------------------------------------------------------------------------
int	MTilePanel::GetNamespaceType()
{
	return CTBNamespace::TILEPANEL;
}

//----------------------------------------------------------------------------
INameSpace^ MTilePanel::Namespace::get()
{
	return gcnew NameSpace(gcnew System::String(m_pTilePanel ? m_pTilePanel->GetNamespace().ToString() : _T("")));
}

//----------------------------------------------------------------------------
bool MTilePanel::Create(IWindowWrapperContainer^ parentWindow, System::Drawing::Point location, System::String^ className)
{
	if (parentWindow == nullptr || !CanCreate())
		return false;

	CWnd* pParentWnd = ((BaseWindowWrapper^)parentWindow)->GetWnd();
	CPoint aPt(location.X, location.Y);

	CSize aSize(minSize.Width, minSize.Height);
	SafeMapDialog(pParentWnd->m_hWnd, aSize);//trasformo in pixel

	CTBNamespace aNamespace = CString(CreateNamespaceFromParent(parentWindow));
	UINT nIDC = AfxGetTBResourcesMap()->GetTbResourceID(aNamespace.GetObjectName(), TbControls);

	if (pParentWnd->IsKindOf(RUNTIME_CLASS(CBaseTileGroup)))
	{
		m_pParentTileGroup = ((CBaseTileGroup*)pParentWnd);
		m_pTilePanel = m_pParentTileGroup->AddPanel(aNamespace.GetObjectName(), aNamespace.GetObjectName());
		Size = parentWindow->Size;
		Handle = (IntPtr)m_pTilePanel->m_hWnd;
	}

	return m_pTilePanel != NULL;
}

//----------------------------------------------------------------------------
void MTilePanel::OnMouseDown(Point p)
{
	if (!m_pTilePanel)
		return;

	CPoint pt(p.X, p.Y);

	int nTab = m_pTilePanel->GetTabFromPoint(pt);
	m_pTilePanel->SetActiveTab(nTab);
}

//----------------------------------------------------------------------------
System::String^	MTilePanel::Name::get()
{
	if (m_pTilePanel)
		return gcnew System::String(m_pTilePanel->GetNamespace().GetObjectName());

	return System::String::Empty;
}

//----------------------------------------------------------------------------
void MTilePanel::Name::set(System::String^ value)
{
	CString sName = CString(value);
	m_pTilePanel->GetNamespace().SetObjectName(sName, TRUE);
}

//----------------------------------------------------------------------------
void MTilePanel::AfterOwnEditingSwitching(bool onwEditingOn, bool IAmInEditing)
{
	if (!m_pTilePanel)
		return;

	inOwnEditing = IAmInEditing;
	bool visible = inOwnEditing;
	if (onwEditingOn)
		oldVisible = m_pTilePanel->IsVisible() == TRUE;
	else
		visible = oldVisible;

	m_pTilePanel->Show(visible);

	AfterSelectionChanged(IAmInEditing);
}

//----------------------------------------------------------------------------
void MTilePanel::AfterSelectionChanged(bool IAmSelected)
{
	TileStyle* pDesignStyle = AfxGetTileDialogStyle(_T("DesignMode"));
	if (m_pTilePanel->GetTileStyle() && pDesignStyle)
	{
		m_pTilePanel->GetTileStyle()->UseAlternativeColorsOf(IAmSelected ? pDesignStyle : NULL);
		m_pTilePanel->Invalidate();
		m_pTilePanel->UpdateWindow();
	}
}


//----------------------------------------------------------------------------
System::String^ MTilePanel::Text::get()
{
	if (!m_pTilePanel)
		return gcnew System::String("");

	return gcnew System::String(m_pTilePanel->GetTitle());
}

//----------------------------------------------------------------------------
void MTilePanel::Text::set(System::String^ value)
{
	if (m_pTilePanel && m_pTilePanel->GetIsShowAsTile())
		m_pTilePanel->SetTitle(value);
}

//----------------------------------------------------------------------------
System::String^ MTilePanel::SerializedName::get()
{
	return System::String::Concat("tilePanel_", EasyBuilderControlSerializer::Escape(String::IsNullOrEmpty(Name) ? Id : Name));
}

//----------------------------------------------------------------------------
System::String^ MTilePanel::SerializedType::get()
{
	return System::String::Concat("TilePanel", EasyBuilderControlSerializer::Escape(Name));
}

//----------------------------------------------------------------------------
bool MTilePanel::CanCreate()
{
	return !GetHandle();
}

//----------------------------------------------------------------------------
bool MTilePanel::CanDropTarget(Type^ droppedObject)
{
	return	
		(MTileDialog::typeid == droppedObject || droppedObject->IsSubclassOf(MTileDialog::typeid)) ||
		(MTilePanelTab::typeid == droppedObject || droppedObject->IsSubclassOf(MTilePanelTab::typeid));
}


//----------------------------------------------------------------------------
void MTilePanel::Initialize()
{
	__super::Initialize();
	minSize = CUtility::Get200x200Size();
	HasCodeBehind = true;
}


//----------------------------------------------------------------------------
IntPtr MTilePanel::Handle::get()
{
	if (m_pTilePanel)
		return (IntPtr)m_pTilePanel->m_hWnd;

	return __super::Handle;
}

//-----------------------------------------------------------------------------------
void MTilePanel::Invalidate()
{
	MTileGroup^ pParent = dynamic_cast<MTileGroup^>(Parent);

	if (pParent == nullptr)
		return;
	pParent->Invalidate();
}

//--------------------------------------------------------------------------------------
void MTilePanel::UpdateWindow()
{
	MTileGroup^ pParent = dynamic_cast<MTileGroup^>(Parent);

	if (pParent == nullptr)
		return;
	pParent->UpdateWindow();
}

//----------------------------------------------------------------------------
MTilePanelTab^ MTilePanel::CurrentTab::get()
{
	if (m_pTilePanel == NULL)
		return nullptr;

	CTilePanelTab* pActiveTab = m_pTilePanel->GetActiveTab();
	if (!pActiveTab)
		return nullptr;

	CTBNamespace aNs(pActiveTab->GetElementNameSpace());
	System::String^ name = gcnew System::String(aNs.GetObjectName());

	for each (IComponent^ component in Components)
	{
		MTilePanelTab^ pPanelTab = dynamic_cast<MTilePanelTab^>(component);
		if (pPanelTab == nullptr)
			continue;

		if (pPanelTab->Name == name)
			return pPanelTab;
	}
	return nullptr;
}

//----------------------------------------------------------------------------
bool MTilePanel::CreateWrappers(array<IntPtr>^ handlesToSkip)
{
	//qua deve fare la create wrappers delle tile che contiene
	if (CurrentTab != nullptr)
		return CurrentTab->CreateWrappers(handlesToSkip);

	return false;
}

//----------------------------------------------------------------------------
void MTilePanel::OnAfterCreateComponents()
{
	if (!m_pTilePanel)
		return;

	for (int i = 0; i < m_pTilePanel->GetTabsNum(); i++)
	{
		MTilePanelTab^ tilePanelTab = gcnew MTilePanelTab((IntPtr)m_pTilePanel->GetTab(i)->m_hWnd);
		tilePanelTab->Parent = this;
		Add(tilePanelTab);
	}
}


//----------------------------------------------------------------------------
bool MTilePanel::Equals(Object^ obj)
{
	if (
		obj == nullptr ||
		!(obj->GetType()->IsSubclassOf(MTilePanel::typeid) || MTilePanel::typeid->IsInstanceOfType(obj))
		)
		return false;

	MTilePanel^ aTilePanel = (MTilePanel^)obj;
	return this->GetPtr() == aTilePanel->GetPtr();
}

//----------------------------------------------------------------------------
//Inserisce in foundChildren tutti i controlli che contengono il punto screenPoint.
//Discrimina tra container e non container: i controlli NON container sono
//aggiunti per primi nella collezione, i controlli container sono aggiunti per ultimi.
void MTilePanel::GetChildrenFromPos(
	System::Drawing::Point screenPoint,
	IntPtr handleToSkip,
	System::Collections::Generic::ICollection<IWindowWrapper^>^ foundChildren
	)
{
	if (components == nullptr)
		return;

	for each (IComponent^ component in Components)
	{
		MTilePanelTab^ pPanelTab = dynamic_cast<MTilePanelTab^>(component);
		HWND hHandleToSkip = (HWND)(int)handleToSkip;
		CPoint aPt(screenPoint.X, screenPoint.Y);
		HWND hChild = (HWND)pPanelTab->Handle.ToInt64();
		if (hChild != hHandleToSkip)
		{
			CRect aRect;
			::GetWindowRect(hChild, &aRect);

			if (aRect.PtInRect(aPt))
			{
				// customized controls
				IWindowWrapper^ wrapper = GetControl((IntPtr)hChild);
				if (wrapper != nullptr)
					foundChildren->Add(wrapper);
			}
		}

		pPanelTab->GetChildrenFromPos(screenPoint, handleToSkip, foundChildren);
	}

	//stessa cosa per i panel
}


//----------------------------------------------------------------------------
HWND MTilePanel::GetControlHandle(const CTBNamespace& aNamespace)
{
	CWnd* pWnd = m_pTilePanel
		? m_pTilePanel->GetWndLinkedCtrl(aNamespace)
		: NULL;
	return pWnd ? pWnd->m_hWnd : NULL;
}

//----------------------------------------------------------------------------
void MTilePanel::IntegrateLayout(ILayoutComponent^ layoutComponent)
{
	MLayoutObject ^ layoutObject = (MLayoutObject^) layoutComponent;
	if (layoutObject->Components->Count == Components->Count)
		return;

	layoutObject->RemoveLayoutObjectOn(this->Namespace);

	for each (IComponent^ component in Components)
	{
		if (!MTilePanelTab::typeid->IsInstanceOfType(component))
			continue;

		MTilePanelTab^ tab = (MTilePanelTab^)component;
			layoutObject->AddContainer(tab->GetLayoutContainer(), tab);
		}
}


//----------------------------------------------------------------------------
bool MTilePanel::HasOwnEditing::get()
{
	return (DesignModeType == EDesignMode::Runtime);
}
