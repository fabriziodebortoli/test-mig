#include "stdafx.h"

#include <TbGenlib\LayoutContainer.h>
#include <TbGes\TileManager.h>
#include <TbGes\TileDialog.h>

#include "MTileGroup.h"
#include "MTileDialog.h"
#include "MTilePanel.h"
#include "MTileManager.h"
#include "MView.h"

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

class CMyCrackingTileManager : public CTileManager
{
public:
	int InsertDlgInfoItemCracked(int pos, DlgInfoItem* item)
	{
		return __super::InsertDlgInfoItem(pos, item);
	}
};




/////////////////////////////////////////////////////////////////////////////
// 				class TileGroupSerializer Implementation
/////////////////////////////////////////////////////////////////////////////
//----------------------------------------------------------------------------	

//----------------------------------------------------------------------------	
Statement^ TileGroupSerializer::GetConstructor(IDesignerSerializationManager^ manager, EasyBuilderControl^ ebControl)
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
TypeDeclaration^ TileGroupSerializer::SerializeClass(SyntaxTree^ syntaxTree, IComponent^ control)
{
	//Se la classe custom che devo generare esiste già, non devo creare niente
	String^ className = ((EasyBuilderComponent^)control)->SerializedType;
	
	TypeDeclaration^ controller = GetControllerTypeDeclaration(syntaxTree);
	String^ controllerName = controller->Name;

	TypeDeclaration^ aClass = gcnew TypeDeclaration();
	aClass->Modifiers = Modifiers::Public;
	aClass->Name = className;
	aClass->BaseTypes->Add(gcnew SimpleType(MTileGroup::typeid->FullName));

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
// 						class MTileGroup Implementation
/////////////////////////////////////////////////////////////////////////////
//----------------------------------------------------------------------------
MTileGroup::MTileGroup(IntPtr handleWndPtr)
	:
	WindowWrapperContainer(handleWndPtr),
	m_pInfo(NULL),
	m_pTileManager(NULL),
	m_pTileGroup(NULL)
{
	HasCodeBehind = true;

	CWnd* pWnd = CWnd::FromHandle((HWND)(int)handleWndPtr.ToInt64());
	m_pTileGroup = dynamic_cast<CBaseTileGroup*> (pWnd);

	if (m_pTileGroup)
	{
		LayoutElement* pElement = m_pTileGroup->GetParentElement();
		CLayoutContainer* pContainer = dynamic_cast<CLayoutContainer*>(pElement);
		while (pContainer)
		{
			pElement = pContainer->GetParentElement();
			pContainer = dynamic_cast<CLayoutContainer*>(pElement);
		}
		CAbstractFormView* pView = dynamic_cast<CAbstractFormView*>(pElement);
		if (pView)
		{
			for (int i = 0; i < pView->m_pTileGroups->GetCount(); i++)
			{
				CBaseTileGroup* pTileGroup = pView->m_pTileGroups->GetAt(i);
				if (m_pTileGroup->GetDlgCtrlID() == pTileGroup->GetDlgCtrlID())
					break;
			}
		}

		m_pTileManager = dynamic_cast<CTileManager*>(pElement);
		if (m_pTileManager)
		{
			for (int i = 0; i < m_pTileManager->GetDlgInfoArray()->GetCount(); i++)
			{
				TileGroupInfoItem* pInfo = (TileGroupInfoItem*)m_pTileManager->GetDlgInfoArray();
				if (pInfo->GetDialogID() == m_pTileGroup->GetDlgCtrlID())
				{
					m_pInfo = pInfo;
					break;
				}
			}
		}
	}
}

//----------------------------------------------------------------------------
MTileGroup::MTileGroup(IWindowWrapperContainer^ parentWindow, System::String^ name, System::String^ className, Point location, bool hasCodeBehind)
	:
	m_pInfo(NULL),
	m_pTileManager(NULL),
	m_pTileGroup(NULL),
	WindowWrapperContainer(parentWindow, name, className, location, hasCodeBehind)
{
	if (parentWindow->GetType() == MTabber::typeid || parentWindow->GetType()->IsSubclassOf(MTabber::typeid))
	{
		MTabber^ tabber = dynamic_cast<MTabber^>(parentWindow);
		m_pTileManager = tabber->GetInnerTabManager();
		if (m_pTileManager && !m_pInfo)
		{
			for (int i = 0; i < m_pTileManager->GetDlgInfoArray()->GetCount(); i++)
			{
				TileGroupInfoItem* pInfo = dynamic_cast<TileGroupInfoItem*>(m_pTileManager->GetDlgInfoArray()->GetAt(i));
				if (pInfo->GetName().CompareNoCase(CString(name)) == 0)
				{
					m_pInfo = pInfo;
					break;
				}
			}
			if (m_pInfo && m_pInfo->m_pBaseTabDlg)
				m_pTileGroup = m_pInfo->m_pBaseTabDlg->GetChildTileGroup();
		}
	}
	else
		m_pTileGroup = dynamic_cast<CTileGroup*>(GetWnd());
}

//-------------------------------------------------------------------------------------------
void MTileGroup::UpdateAttributesForJson(CWndObjDescription* pParentDescription)
{
	ASSERT(pParentDescription);
	if (!pParentDescription)
		return;

	if (!this->HasCodeBehind)
	{
		jsonDescription = pParentDescription->AddChildWindow(this->GetWnd(), this->Id);

		ASSERT(jsonDescription);
		if (!jsonDescription)
			return;

		__super::UpdateAttributesForJson(pParentDescription);

		CWndLayoutContainerDescription* pTileGroupDescription = dynamic_cast<CWndLayoutContainerDescription*>(jsonDescription);

		if (pTileGroupDescription)
		{
			pTileGroupDescription->m_Type = CWndObjDescription::WndObjType::TileGroup;
			pTileGroupDescription->m_LayoutType = CLayoutContainer::COLUMN;	//its own default
			if (this->TileGroup && this->TileGroup->GetLayoutContainer())
				pTileGroupDescription->m_LayoutType = this->TileGroup->GetLayoutContainer()->GetLayoutType();
			if (this->TileGroup && this->TileGroup->GetLayoutContainer())
				pTileGroupDescription->m_LayoutAlign = this->TileGroup->GetLayoutContainer()->GetLayoutAlign() == CLayoutContainer::LayoutAlign::NO_ALIGN ?
				CLayoutContainer::LayoutAlign::STRETCH : //its own default
				this->TileGroup->GetLayoutContainer()->GetLayoutAlign();
		}
	}
	else
	{
		jsonDescription = new CDummyDescription();
		jsonDescription->m_Type = CWndObjDescription::WndObjType::Undefined;
		jsonDescription->m_strIds.Add(this->Id);
	}
}

//------------------------------------------------------------------------------------
void MTileGroup::GenerateSerialization(CWndObjDescription* pParentDescription, List<System::Tuple<System::String^, System::String^, System::Boolean>^>^ serialization)
{
	if (pParentDescription->IsKindOf(RUNTIME_CLASS(CDummyDescription)))
	{
		if (!jsonDescription->IsKindOf(RUNTIME_CLASS(CDummyDescription)))
		{
			//add dummy description to my father
			jsonDummyDescription = new CDummyDescription();
			jsonDummyDescription->m_Type = CWndObjDescription::WndObjType::Undefined;
			pParentDescription->m_Children.Add(jsonDummyDescription);
			jsonDummyDescription->m_arHrefHierarchy.Add(this->Id);
			//add json di jsonDescription.Serialize
			serialization->Add
			(
				gcnew Tuple<System::String^, System::String^, System::Boolean>
				(
					gcnew String(this->Id),
					gcnew String(GetSerialization(jsonDescription)),
					false
				)
			);

			//update parent => remove details for this from parent
			for (int i = pParentDescription->m_Children.GetCount() - 1; i >= 0; i--)
			{
				if (pParentDescription->m_Children.GetAt(i) == jsonDescription)
				{
					SAFE_DELETE(pParentDescription->m_Children.GetAt(i));
					pParentDescription->m_Children.RemoveAt(i);
				}
			}
		}
		else if (jsonDescription->m_Children.GetCount() > 0)
		{
			//ClientForms
			serialization->Add
			(
				gcnew Tuple<System::String^, System::String^, System::Boolean>
				(
					gcnew String(pParentDescription->m_strIds.GetAt(0) + _T("_") + this->Id),
					gcnew String(GetSerialization(jsonDescription)),
					true
				)
			);

			SAFE_DELETE(jsonDescription);
		}
	}
	
	__super::GenerateSerialization(pParentDescription, serialization);
}

//----------------------------------------------------------------------------
void MTileGroup::AttachTileManager(CTileManager* pManager, TileGroupInfoItem* pInfo)
{
	m_pTileManager = pManager;
	m_pInfo = pInfo;
}

//----------------------------------------------------------------------------
int MTileGroup::Flex::get()
{
	if (!m_pTileGroup)
		return -1;

	return  DesignerVisible ? flex :
		m_pTileGroup->GetFlex(LayoutElement::WIDTH);
}

//----------------------------------------------------------------------------
void MTileGroup::Flex::set(int value)
{
	if (!m_pTileGroup)
		return;
	flex = value;
	if (!DesignerVisible)
		m_pTileGroup->SetFlex(value);
}


//----------------------------------------------------------------------------
bool MTileGroup::GroupCollapsible::get()
{
	return  m_pTileGroup && m_pTileGroup->IsGroupCollapsible() == TRUE;
}
//----------------------------------------------------------------------------
void MTileGroup::GroupCollapsible::set(bool value)
{
	if (m_pTileGroup)
		m_pTileGroup->SetGroupCollapsible(value);
}


//----------------------------------------------------------------------------
MTileGroup::~MTileGroup()
{
	this->!MTileGroup();
	m_pInfo = NULL;
	m_pTileManager = NULL;
	m_pTileGroup = NULL;
	GC::SuppressFinalize(this);
}

//----------------------------------------------------------------------------
MTileGroup::!MTileGroup()
{
	if (HasCodeBehind)
		return;

	if (m_pTileManager)
	{
		if (!m_pTileManager->GetDlgInfoArray() || !m_pInfo)
			return;

		m_pTileManager->DeleteTab(m_pInfo, FALSE);
		return;
	}

	if (!m_pTileGroup)
		return;

	CAbstractFormView* pView = dynamic_cast<CAbstractFormView*>(m_pTileGroup->GetParent());
	if (pView)
	{
		pView->m_pTileGroups->SetOwns(FALSE);
		pView->RemoveTileGroup(m_pTileGroup->GetDlgCtrlID());
		pView->m_pTileGroups->SetOwns(TRUE);

		if (IsWindow(m_pTileGroup->m_hWnd))
			m_pTileGroup->DestroyWindow();

		SAFE_DELETE(m_pTileGroup);
	}
}

//-----------------------------------------------------------------------------
int MTileGroup::GetNamespaceType()
{
	return CTBNamespace::TABDLG;
}

//----------------------------------------------------------------------------
HWND MTileGroup::GetControlHandle(const CTBNamespace& aNamespace)
{
	CWnd* pWnd = m_pTileGroup
		? m_pTileGroup->GetWndLinkedCtrl(aNamespace)
		: NULL;
	return pWnd ? pWnd->m_hWnd : NULL;
}

//-----------------------------------------------------------------------------
void MTileGroup::Parent::set(IWindowWrapperContainer^ value)
{
	__super::Parent = value;

	if (value == nullptr || !MTabber::typeid->IsInstanceOfType(value))
		return;

	m_pTileManager = ((MTileManager^)value)->GetTileManager();
}

//----------------------------------------------------------------------------
int MTileGroup::TileGroupID::get()
{
	if (m_pInfo)
		return m_pInfo->m_nTileGroupID;
	
	if (m_pTileGroup)
		return m_pTileGroup->GetDlgCtrlID();
	
	return 0;
}


//----------------------------------------------------------------------------
void MTileGroup::Initialize()
{
	__super::Initialize();
	minSize = System::Drawing::Size(200, 600);//  CUtility::Get200x200Size();
}

//----------------------------------------------------------------------------
bool MTileGroup::Create(IWindowWrapperContainer^ parentWindow, System::Drawing::Point location, System::String^ className)
{
	if (parentWindow == nullptr || !CanCreate())
		return false;

	CWnd* pParentWnd = ((BaseWindowWrapper^)parentWindow)->GetWnd();
	CPoint aPt(location.X, location.Y);

	CSize aSize(minSize.Width, minSize.Height);

	SafeMapDialog(pParentWnd->m_hWnd, aSize);//trasformo in pixel

	DWORD styles = WS_CHILD | WS_VISIBLE | WS_TABSTOP | WS_CLIPCHILDREN;

	CTBNamespace aNamespace = CString(CreateNamespaceFromParent(parentWindow));
	UINT nIDC = AfxGetTBResourcesMap()->GetTbResourceID(CString(Id), TbControls);

	m_pTileManager = dynamic_cast<CTileManager*>(pParentWnd);
	// parent in caso di CTileManager
	if (m_pTileManager)
	{

		m_pInfo = ((CTileManager*)m_pTileManager)->AddTileGroup(RUNTIME_CLASS(CTileGroup), aNamespace.GetObjectName(), aNamespace.GetObjectName(), _T(""), _T(""), nIDC);

		//la AddDialog non fa più la chiamata a InsertDlgInfoItem, la faccio qui
		//non posso mettere friend perché questa classe è managed, allora uso un trucchetto
		int nPos = ((CMyCrackingTileManager*)m_pTileManager)->InsertDlgInfoItemCracked(m_pTileManager->GetTabCount(), m_pInfo);
		
		m_pTileManager->TabDialogActivate(m_pTileManager->GetDlgCtrlID(), m_pInfo->GetDialogID());

		Parent = parentWindow;
		ParentComponent = (EasyBuilderComponent^)parentWindow;
		CBaseTileGroup* pTileGroup = m_pTileManager->GetActiveTileGroup();
		if (DesignModeType == EDesignMode::Runtime)
			SetGroupInDesignMode(pTileGroup, LayoutElement::AUTO);

		m_pTileManager->AdjustTabManager();
		Handle = (IntPtr)pTileGroup->m_hWnd;
		return Handle != IntPtr::Zero;
	}

	// parent in caso di CBaseFormView
	CAbstractFormView* pView = dynamic_cast<CAbstractFormView*>(pParentWnd);
	if (pView)
	{
		int nFlex = pView->m_pTileGroups->GetSize() ? 0 : LayoutElement::AUTO;
		m_pTileGroup = pView->AddTileGroup(nIDC, RUNTIME_CLASS(CTileGroup), aNamespace.GetObjectName());
		if (m_pTileGroup)
		{
			Parent = parentWindow;
			ParentComponent = (EasyBuilderComponent^) parentWindow;
			if (DesignModeType == EDesignMode::Runtime)
				SetGroupInDesignMode(m_pTileGroup, nFlex);
			Handle = (IntPtr)m_pTileGroup->m_hWnd;
		}
	}


	return m_pTileGroup != NULL;
}

//----------------------------------------------------------------------------
CBaseTileGroup* MTileGroup::TileGroup::get()
{
	if (!this->m_pTileGroup)
	{
		if (m_pInfo)
		{
			CTabDialog* pDialog = ((CTabDialog*)this->m_pInfo->m_pBaseTabDlg);
			if (pDialog)
				this->m_pTileGroup = pDialog->GetChildTileGroup();
		}
	}

	return this->m_pTileGroup;
}

//----------------------------------------------------------------------------
bool MTileGroup::Equals(Object^ obj)
{
	if (
		obj == nullptr ||
		!(obj->GetType()->IsSubclassOf(MTileGroup::typeid) || MTileGroup::typeid->IsInstanceOfType(obj))
		)
		return false;

	MTileGroup^ aTileGroup = (MTileGroup^)obj;

	if (m_pInfo && aTileGroup->m_pInfo && m_pInfo == aTileGroup->m_pInfo && m_pTileManager && aTileGroup->m_pTileManager)
		return true;

	if (m_pTileGroup && aTileGroup->m_pTileGroup && m_pTileGroup == aTileGroup->m_pTileGroup)
		return true;
	
	return __super::Equals(aTileGroup);
}

//----------------------------------------------------------------------------
System::String^ MTileGroup::ParentName::get()
{
	if (m_pTileManager)
		return gcnew System::String(m_pTileManager->GetNamespace().GetObjectName());

	return String::Empty;
}

//----------------------------------------------------------------------------
System::String^	MTileGroup::Name::get()
{
	if (m_pInfo)
		return gcnew System::String(m_pInfo->GetNamespace().GetObjectName());
	
	if (m_pTileGroup)
		return gcnew System::String(m_pTileGroup->GetNamespace().GetObjectName());

	return System::String::Empty;
}

//----------------------------------------------------------------------------
void MTileGroup::Name::set(System::String^ value)
{
	//Per il momento se nella tab sono già stati droppati dei control, non posso rinominarla.
	if (Components && Components->Count == 0)
	{
		CString sName = CString(value);

		if (m_pInfo)
		{
			m_pInfo->GetNamespace().SetObjectName(sName, TRUE);
			//La tabDialog associata al DlgInfoItem ha una variabile membro m_sName,
			//qui viene allineata con il nome impostato al namespace
			if (m_pInfo->m_pBaseTabDlg != nullptr)
				m_pInfo->m_pBaseTabDlg->SetFormName(sName);
			else
				Diagnostic->SetError((gcnew System::String(_TB("Unable to rename Tile Group: pointer m_pInfo is null"))));
		}
		if (m_pTileGroup)
			m_pTileGroup->GetNamespace().SetObjectName(sName, TRUE);
	}
	else
		Diagnostic->SetError((gcnew System::String(_TB("Unable to rename Tile Group: rename is enable only if the Tile Group does not contains controls"))));
}

//----------------------------------------------------------------------------
INameSpace^ MTileGroup::Namespace::get()
{
	System::String^ nameSpace = System::String::Empty;
	if (m_pInfo)
		nameSpace = gcnew System::String(m_pInfo->GetNamespace().ToString());
	else if (m_pTileGroup)
		nameSpace = gcnew System::String(m_pTileGroup->GetNamespace().ToString());


	if (System::String::IsNullOrEmpty(nameSpace))
	{
		return __super::Namespace;
	}

	return gcnew NameSpace(nameSpace);
}

//----------------------------------------------------------------------------
void MTileGroup::Text::set(System::String^ value)
{
	if (m_pInfo)
		m_pInfo->m_strTitle = CString(value);

	if (m_pTileManager)
	{
		switch (m_pTileManager->GetShowMode())
		{
		case CTileManager::NORMAL:
			m_pTileManager->ChangeTabTitle(m_pInfo->GetDialogID(), m_pInfo->m_strTitle);
			break;
		case CTileManager::VERTICAL_TILE:
			m_pTileManager->UpdateSelector();
		default:
			break;
		}
	}
}


//----------------------------------------------------------------------------
System::String^ MTileGroup::Text::get()
{
	if (m_pInfo)
		return gcnew System::String(m_pInfo->m_strTitle);
	
	return String::Empty;
}

//----------------------------------------------------------------------------
System::String^ MTileGroup::SerializedName::get()
{
	return System::String::Concat
		(
			"tileGroup_", 
			EasyBuilderControlSerializer::Escape(
			String::IsNullOrEmpty(Name) ? Id : Name)
		);
}

//-----------------------------------------------------------------------------
EditingMode MTileGroup::DesignerMovable::get()
{
	EditingMode mode = EditingMode::ResizingMidBottom | EditingMode::ResizingMidRight | EditingMode::ResizingBottomRight;
	// e' movable solo se non si trova in un tilemanager
	if (!HasCodeBehind && m_pTileManager == nullptr && m_pInfo == nullptr)
		mode = mode | EditingMode::Moving;
	
	return mode;
}

//----------------------------------------------------------------------------
System::String^ MTileGroup::SerializedType::get()
{
	return System::String::Concat("TileGroup", EasyBuilderControlSerializer::Escape(Name));
}

//----------------------------------------------------------------------------
bool MTileGroup::CanCreate()
{
	return !GetHandle();
}

//----------------------------------------------------------------------------
bool MTileGroup::CanDropData(IDataBinding^ dataBinding)
{
	return false;
}

//----------------------------------------------------------------------------
bool MTileGroup::CanDropTarget(Type^ droppedObject)
{
	return
		(MTileDialog::typeid == droppedObject || droppedObject->IsSubclassOf(MTileDialog::typeid)) ||
		(MTilePanel::typeid == droppedObject || droppedObject->IsSubclassOf(MTilePanel::typeid));
}

//----------------------------------------------------------------------------
void MTileGroup::AfterTargetDrop(System::Type^ droppedType)
{

	if (m_pTileGroup)
		m_pTileGroup->RequestRelayout();
}

//----------------------------------------------------------------------------
void MTileGroup::Activate()
{
	if (m_pInfo == NULL)
		return;

	MTabber^ tabber = dynamic_cast<MTabber^>(Parent);
	if (!tabber)
		return;

	CTabManager* pTileManager = tabber->GetInnerTabManager();
	// sono già attiva, evito di andare in loop
	CTileGroup* pActiveTileGroup = pTileManager->GetActiveTileGroup();
	if (pActiveTileGroup && pActiveTileGroup == m_pTileGroup)
		return;

	pTileManager->TabDialogActivate(m_pInfo->GetNamespace().ToUnparsedString());
	if (m_pInfo->m_pBaseTabDlg)
		m_pTileGroup = ((CTabDialog*)this->m_pInfo->m_pBaseTabDlg)->GetChildTileGroup();
}

//----------------------------------------------------------------------------
void MTileGroup::SyncTileGroup()
{
	if (m_pInfo && m_pInfo->m_pBaseTabDlg)
	{
		m_pTileGroup = ((CTabDialog*)this->m_pInfo->m_pBaseTabDlg)->GetChildTileGroup();
		components->Clear(); 
	}
}

//----------------------------------------------------------------------------
IntPtr MTileGroup::Handle::get()
{
	HWND hwnd = NULL;
	if (m_pInfo && m_pInfo->m_pBaseTabDlg && m_pInfo->m_pBaseTabDlg->GetChildTileGroup())
		hwnd = m_pInfo->m_pBaseTabDlg->GetChildTileGroup()->m_hWnd; //lazy initialization: se ho una tab attiva, ne prendo l'handle

	if (!hwnd)
		return __super::Handle;

	if ((HWND)(int)__super::Handle != hwnd)
		__super::Handle = (IntPtr)hwnd;

	return __super::Handle;
}

//----------------------------------------------------------------------------
bool MTileGroup::CreateWrappers(array<IntPtr>^ handlesToSkip)
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
void MTileGroup::OnAfterCreateComponents()
{
	if (!TileGroup)
		return;

	for (int i = 0; i < TileGroup->GetTilePanels()->GetSize(); i++)
	{
		CTilePanel* pPanel = TileGroup->GetTilePanels()->GetAt(i);
		if (!pPanel || !pPanel->m_hWnd)
			continue;
		MTilePanel^ tilePanel = gcnew MTilePanel((IntPtr) pPanel->m_hWnd);
		tilePanel->Parent = this;
		Add(tilePanel);
	}

	for (int i = 0; i < TileGroup->GetTileDialogs()->GetSize(); i++)
	{
		CBaseTileDialog* pDialog = TileGroup->GetTileDialogs()->GetAt(i);

		CTilePanelTab* pTab = dynamic_cast<CTilePanelTab*>(pDialog->GetParent());
		if (pTab)
			continue;

		MTileDialog^ tileDialog = gcnew MTileDialog((IntPtr) pDialog->m_hWnd);
		tileDialog->Parent = this;
		Add(tileDialog);
	}
}

//----------------------------------------------------------------------------
//Inserisce in foundChildren tutti i controlli che contengono il punto screenPoint.
//Discrimina tra container e non container: i controlli NON container sono
//aggiunti per primi nella collezione, i controlli container sono aggiunti per ultimi.
void MTileGroup::GetChildrenFromPos(
	System::Drawing::Point screenPoint,
	IntPtr handleToSkip,
	System::Collections::Generic::ICollection<IWindowWrapper^>^ foundChildren
	)
{
	for each (IComponent^ component in Components)
	{
		WindowWrapperContainer^ container = dynamic_cast<WindowWrapperContainer^>(component);
		if (container == nullptr)
			continue;

		HWND hHandleToSkip = (HWND)(int)handleToSkip;
		CPoint aPt(screenPoint.X, screenPoint.Y);
		HWND hChild = (HWND)container->Handle.ToInt64();
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
		container->GetChildrenFromPos(screenPoint, handleToSkip, foundChildren);
	}
}

//----------------------------------------------------------------------------
void MTileGroup::AfterOwnEditingSwitching(bool onwEditingOn, bool IAmInEditing)
{
	if (m_pTileGroup)
		m_pTileGroup->RequestRelayout();
}

//----------------------------------------------------------------------------
void MTileGroup::SetGroupInDesignMode(CBaseTileGroup* pTileGroup, int nFlex)
{
	//if (!nFlex)
	//{
		pTileGroup->SetMinWidth(minSize.Width);
		pTileGroup->SetMinHeight(minSize.Height);
		pTileGroup->SetMaxWidth(LayoutElement::FREE);
	//}
	pTileGroup->SetRequestedLastFlex(nFlex);
	pTileGroup->SetFlex(nFlex);
	pTileGroup->GetLayoutContainer()->SetLayoutAlign(CLayoutContainer::NO_ALIGN);
}

//----------------------------------------------------------------------------
bool MTileGroup::CanCallCreateComponents() 
{ 
	return m_pTileManager == nullptr || (DesignModeType == EDesignMode::Runtime && !HasCodeBehind); 
}

//----------------------------------------------------------------------------
bool MTileGroup::AreComponentsLoaded::get()
{
	// okkio che il tabber ha la gestione de
	if (m_pTileManager && !m_pTileGroup && IsChanged)
		return false;

	return __super::AreComponentsLoaded;
}

//----------------------------------------------------------------------------
int	MTileGroup::GetTileIndex(MTileDialog^ tileDialog)
{
	return components != nullptr  ? components->IndexOf(tileDialog) : -1;
}

//----------------------------------------------------------------------------
void MTileGroup::LayoutChanged()
{
	IWindowWrapper^ parent = this;
	while (parent != nullptr)
	{
		parent = parent->Parent;
		MView^ view = dynamic_cast<MView^>(parent);
		if (view != nullptr)
		{
			view->LayoutChangedFor(Namespace);
			break;
		}
	}
}

//----------------------------------------------------------------------------
void MTileGroup::MoveTile(MTileDialog^ tileDialog, int nToIndex)
{
	if (m_pTileGroup && components && components->Contains(tileDialog))
	{
		int oldIndex = GetTileIndex(tileDialog);
		if (oldIndex != nToIndex)
		{
			CTileDialog* pTileDialog = dynamic_cast<CTileDialog*>(tileDialog->GetWnd());
			if (pTileDialog && pTileDialog->GetParentElement())
			{
				LayoutElement* pParentElement = pTileDialog->GetParentElement();
				pParentElement->RemoveChildElement(pTileDialog);
				pParentElement->InsertChildElement(pTileDialog, nToIndex);
				m_pTileGroup->RequestRelayout();
			}
			components->RemoveAt(oldIndex);
			components->Insert(nToIndex, tileDialog);
			LayoutChanged();
		}
	}
}

//----------------------------------------------------------------------------
int MTileGroup::TabOrder::get()
{
	if (m_pTileManager && m_pInfo)
	{
		for (int i = 0; i <= m_pTileManager->GetDlgInfoArray()->GetUpperBound(); i++)
		{
			TileGroupInfoItem* pItem = dynamic_cast<TileGroupInfoItem*>(m_pTileManager->GetDlgInfoArray()->GetAt(i));
			if (pItem && pItem->GetName().CompareNoCase(m_pInfo->GetName()) == 0)
				return i;
		}
	}
	else
	{
		if (m_pTileGroup)
		{
			CAbstractFormView* pView = dynamic_cast<CAbstractFormView*>(m_pTileGroup->GetParent());

			if (!pView)
				return -1;

			for (int i = 0; i < pView->m_pTileGroups->GetCount(); i++)
			{
				CBaseTileGroup* pGroup = pView->m_pTileGroups->GetAt(i);
				if (pGroup->GetNamespace().GetObjectName().CompareNoCase(CString(Name)) == 0)
					return i;
			}
		}
	}

	return -1;
}

//----------------------------------------------------------------------------
void MTileGroup::TabOrderSetForTileManager(int value)
{
	if (!m_pTileManager)
		return;

	CTileManager* pTileManager = dynamic_cast<CTileManager*>(m_pTileManager);
	if (pTileManager && m_pInfo)
	{
		pTileManager->MoveTileGroup(m_pInfo, value);

		MTileManager^ tileManager = dynamic_cast<MTileManager^>(__super::Parent);

		if (tileManager != nullptr)
			tileManager->MoveTileGroup(this, value);

	}

}

//----------------------------------------------------------------------------
void MTileGroup::TabOrderSetForView(int value)
{
	if (!m_pTileGroup)
		return;

	CAbstractFormView* pView = dynamic_cast<CAbstractFormView*>(m_pTileGroup->GetParent());

	if (!pView)
		return;

	pView->MoveTileGroup(m_pTileGroup, value);
	
	pView->RequestRelayout();

	MView^ view = dynamic_cast<MView^>(__super::Parent);
	if (view != nullptr)
		view->MoveTileGroup(this, value);
}

//----------------------------------------------------------------------------
void MTileGroup::TabOrder::set(int value)
{
	if (m_pTileManager)
		TabOrderSetForTileManager(value);
	else
		TabOrderSetForView(value);
}
