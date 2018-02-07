#include "stdafx.h"

#include <TbGes\TileManager.h>
#include <TbGes\TileDialog.h>

#include "MView.h"
#include "MTileManager.h"
#include "MTileDialog.h"

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
// 				class TileManagerSerializer Implementation
/////////////////////////////////////////////////////////////////////////////
//----------------------------------------------------------------------------	

//----------------------------------------------------------------------------	
Statement^ TileManagerSerializer::GetConstructor(IDesignerSerializationManager^ manager, EasyBuilderControl^ ebControl)
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
TypeDeclaration^ TileManagerSerializer::SerializeClass(SyntaxTree^ syntaxTree, IComponent^ control)
{
	//Se la classe custom che devo generare esiste già, non devo creare niente
	String^ className = ((EasyBuilderComponent^)control)->SerializedType;

	TypeDeclaration^ controller = GetControllerTypeDeclaration(syntaxTree);
	String^ controllerName = controller->Name;

	TypeDeclaration^ aClass = gcnew TypeDeclaration();
	aClass->Modifiers = Modifiers::Public;
	aClass->Name = className;
	aClass->BaseTypes->Add(gcnew SimpleType(MTileManager::typeid->FullName));

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
// 						class MTileManager Implementation
/////////////////////////////////////////////////////////////////////////////

//----------------------------------------------------------------------------
MTileManager::MTileManager(IntPtr handleWndPtr)
	:
	MTabber(handleWndPtr)
{

}

//----------------------------------------------------------------------------
MTileManager::MTileManager(IWindowWrapperContainer^ parentWindow, System::String^ name, System::String^ className, Point location, bool hasCodeBehind)
	:
	MTabber(parentWindow, name, className, location, hasCodeBehind)
{
	//if (!this->HasCodeBehind)
	//	jsonDescription = new CTabberDescription(NULL);
}

//----------------------------------------------------------------------------
MTileManager::~MTileManager()
{
	if (HasCodeBehind)
		return;

	this->!MTileManager();
	GC::SuppressFinalize(this);
}

//----------------------------------------------------------------------------
MTileManager::!MTileManager()
{
}

//----------------------------------------------------------------------------------
void MTileManager::GenerateJson(CWndObjDescription* pParentDescription, List<System::Tuple<System::String^, System::String^>^>^ serialization)
{
	CDummyDescription* pDummyTMDescription = NULL;

	ASSERT(pParentDescription);
	if (!pParentDescription)
		return;

	if (!this->HasCodeBehind)
	{
		jsonDescription = pParentDescription->AddChildWindow(this->GetWnd(), this->Name);
		ASSERT(jsonDescription);
		if (!jsonDescription)
			return;
		__super::GenerateJson(pParentDescription, serialization);
		jsonDescription->m_Type = CWndObjDescription::WndObjType::TileManager;

		if (pParentDescription->IsKindOf(RUNTIME_CLASS(CDummyDescription)))
		{
			pDummyTMDescription = new CDummyDescription();
			pDummyTMDescription->m_Type = CWndObjDescription::WndObjType::Undefined;
			pParentDescription->m_Children.Add(pDummyTMDescription);
			pDummyTMDescription->m_arHrefHierarchy.Add(this->Id);
		}
	}
	else
	{
		jsonDescription = new CDummyDescription();
		jsonDescription->m_Type = CWndObjDescription::WndObjType::Undefined;
		jsonDescription->m_strIds.Add(this->Id);
	}

	for each (WindowWrapperContainer^ wrapper in this->Components)
	{
		if (wrapper == nullptr || wrapper->Handle == IntPtr::Zero)
			continue;

		wrapper->GenerateJson(jsonDescription, serialization);
	}

	if (pParentDescription->IsKindOf(RUNTIME_CLASS(CDummyDescription)))
	{
		if (!jsonDescription->IsKindOf(RUNTIME_CLASS(CDummyDescription))) //save json di jsonDescription.Serialize
		{
			serialization->Add
			(
				gcnew Tuple<System::String^, System::String^>
				(
					gcnew String(this->Id),
					gcnew String(GetSerialization(jsonDescription))
					)
			);
		}
		else if (jsonDescription->m_Children.GetCount() > 0)
		{
			serialization->Add
			(
				gcnew Tuple<System::String^, System::String^>
				(
					gcnew String(pParentDescription->m_strIds.GetAt(0) + _T("_") + this->Id),
					gcnew String(GetSerialization(jsonDescription))
					)
			);
		}

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

	GenerateJsonForEvents(serialization);
}

//----------------------------------------------------------------------------
HWND MTileManager::GetControlHandle(const CTBNamespace& aNamespace)
{
	CWnd* pWnd = m_pTabber
		? m_pTabber->GetWndLinkedCtrl(aNamespace)
		: NULL;
	return pWnd ? pWnd->m_hWnd : NULL;
}

//----------------------------------------------------------------------------
MTileGroup^ MTileManager::CurrentTileGroup::get ()
{
	if (!GetTileManager())
		return nullptr;

	CTileGroup* pGroup = GetTileManager()->GetActiveTileGroup();

	if (!pGroup)
		return nullptr;

	for each (IComponent^ component in Components)
	{
		MTileGroup^ tileGroup = dynamic_cast<MTileGroup^>(component);
		if (tileGroup != nullptr && tileGroup->Name == gcnew System::String(pGroup->GetNamespace().GetObjectName()))
			return  tileGroup;
	}
	return nullptr;
}

//-----------------------------------------------------------------------------
int	MTileManager::GetNamespaceType()
{
	return CTBNamespace::TABBER;
}

//----------------------------------------------------------------------------
bool MTileManager::CanCreate()
{
	return !GetHandle();
}

//----------------------------------------------------------------------------
System::String^ MTileManager::SerializedName::get()
{
	return System::String::Concat("tileManager_", EasyBuilderControlSerializer::Escape(Name));
}

//----------------------------------------------------------------------------
System::String^ MTileManager::SerializedType::get()
{
	return System::String::Concat("TileManager", EasyBuilderControlSerializer::Escape(Name));
}


//----------------------------------------------------------------------------
void MTileManager::Initialize()
{
	__super::Initialize();
	HasCodeBehind = true;
	minSize = CUtility::GetIdealTabberTileManagerSize();
}


//----------------------------------------------------------------------------
bool MTileManager::CanDropTarget(Type^ droppedObject)
{
	if (droppedObject->IsSubclassOf(MHotLink::typeid))
		return false;

	return	(MTileGroup::typeid == droppedObject || droppedObject->IsSubclassOf(MTileGroup::typeid));
}

//----------------------------------------------------------------------------
bool MTileManager::Create(IWindowWrapperContainer^ parentWindow, System::Drawing::Point location, System::String^ className)
{
	if (parentWindow == nullptr || !CanCreate())
		return false;

	CWnd* pParentWnd = ((BaseWindowWrapper^)parentWindow)->GetWnd();
	if (!pParentWnd->IsKindOf(RUNTIME_CLASS(CAbstractFormView)))
		return false;
	CPoint aPt(location.X, location.Y);

	CSize aSize(minSize.Width, minSize.Height);
	SafeMapDialog(pParentWnd->m_hWnd, aSize);//trasformo in pixel

	Parent = parentWindow;
	ParentComponent = (EasyBuilderComponent^)parentWindow;

	DWORD styles = WS_CHILD | WS_VISIBLE | WS_TABSTOP | WS_CLIPCHILDREN;

	CTBNamespace aNamespace = CString(CreateNamespaceFromParent(parentWindow));
	UINT nIDC = AfxGetTBResourcesMap()->GetTbResourceID(CString(Id), TbControls);

	CButton btn;
	if (!btn.Create
		(
		NULL,
		styles,
		CRect(aPt, aSize),
		pParentWnd,
		nIDC
		)
		)
	{
		return FALSE;
	}

	// tab manager destroys the window with all objects and and recreates it with another handle and default font!!
	m_pTabber = ((CAbstractFormView*)pParentWnd)->AddTileManager(nIDC, RUNTIME_CLASS(CTileManager), aNamespace.GetObjectName());
		
	if (GetTileManager())
	{
		// Il Runtime lo setta ad una dimensione piu' ragionevole
		// per evitare che lo dimensionino minuscolo
		if (DesignModeType == EDesignMode::Runtime)
		{
			GetTileManager()->SetMinWidth(aSize.cx);
			GetTileManager()->SetMaxWidth(LayoutElement::FREE);
			GetTileManager()->SetMinHeight(aSize.cy);
		}

		Handle = (IntPtr)GetTileManager()->m_hWnd;
	}

	return GetTileManager() != NULL;
}
	

//----------------------------------------------------------------------------
bool MTileManager::CreateWrappers(array<IntPtr>^ handlesToSkip)
{
	//qua deve fare la create wrappers delle tile che contiene
	bool changed = false;
	for each (IComponent^ component in Components)
	{
		MTileGroup^ tileGroup = dynamic_cast<MTileGroup^>(component);
		if (tileGroup == nullptr)
			continue;
		
		changed |= tileGroup->CreateWrappers(handlesToSkip);
	}

	return changed;
}

//----------------------------------------------------------------------------
bool MTileManager::Equals(Object^ obj)
{
	if (
		obj == nullptr ||
		!(obj->GetType()->IsSubclassOf(MTileManager::typeid) || MTileManager::typeid->IsInstanceOfType(obj))
		)
		return false;

	MTileManager^ aTileManager = (MTileManager^)obj;
	return this->GetPtr() == aTileManager->GetPtr();
}

//----------------------------------------------------------------------------
MTileGroup^ MTileManager::GetTabByPoint(Point^ p)
{
	TCHITTESTINFO hitTest;

	CPoint pt(p->X, p->Y);
	GetTileManager()->ScreenToClient(&pt);
	hitTest.pt = pt;

	int hit = GetTileManager()->HitTest(&hitTest);
	if (hit < 0)
		return nullptr;

	if (!GetTileManager()->GetDlgInfoArray())
		return nullptr;

	TileGroupInfoItem* pItem = (TileGroupInfoItem*)GetTileManager()->GetDlgInfoArray()->GetAt(hit);
	if (!pItem)
		return nullptr;

	return GetGroupByName(gcnew System::String(pItem->GetNamespace().GetObjectName()));
}


//-----------------------------------------------------------------------------
MTileGroup^ MTileManager::GetGroupByName(System::String^ name)
{
	for each (IComponent^ component in Components)
	{
		MTileGroup^ tileGroup = dynamic_cast<MTileGroup^>(component);
		if (tileGroup == nullptr)
			continue;

		if (tileGroup->Name == name)
			return tileGroup;
	}

	return nullptr;
}

//----------------------------------------------------------------------------
void MTileManager::OnAfterCreateComponents()
{
	if (DesignMode)
		KeepTabsAlive = true;

	if (!GetTileManager() || !GetTileManager()->GetDlgInfoArray())
		return;
	//questa serve per l'easybuilder designer
	DlgInfoArray* pArray = GetTileManager()->GetDlgInfoArray();

	for (int i = 0; i <= pArray->GetUpperBound(); i++)
	{
		TileGroupInfoItem* infoItem = (TileGroupInfoItem*)pArray->GetAt(i);
		if (!infoItem)
			continue;

		MTileGroup^ tileGroup = gcnew MTileGroup(IntPtr::Zero);
		tileGroup->AttachTileManager(GetTileManager(), infoItem);
		tileGroup->Parent = this;
		Add(tileGroup);
	}
}

//----------------------------------------------------------------------------
void MTileManager::CallCreateComponents()
{
	if (!GetTileManager())
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

	GetTileManager()->InitTabNamespaces();
	
	//Creo i control per tutte le Tab, se attive
	int nFirstCreated = -1;
	for (int i = 0; i <= GetTileManager()->GetDlgInfoArray()->GetUpperBound(); i++)
	{
		TileGroupInfoItem* pItem = dynamic_cast<TileGroupInfoItem*>(GetTileManager()->GetDlgInfoArray()->GetAt(i));
		if (!pItem || !pItem->m_pBaseTabDlg)
			continue;

		CTabDialog* pDialog = dynamic_cast<CTabDialog*>(pItem->m_pBaseTabDlg);
		if (!pDialog || !pDialog->GetChildTileGroup())
			continue;
		
		CBaseTileGroup* pGroup = pDialog->GetChildTileGroup();
		MTileGroup^ group = GetGroupByName(gcnew String(pItem->GetName()));
		if (group == nullptr)
			continue;
		
		// ora che ho il wrapping mi segno l'elemento di default
		if (pItem->m_pBaseTabDlg && pItem->m_pBaseTabDlg->GetCurrentTabPos() == m_nDefaultItemPos)
			DefaultItem = group;

		group->CallCreateComponents();
		if (nFirstCreated < 0)
			nFirstCreated = i;
	}

	//se non ho nemmeno una tab viva, vuol dire che non ho tab attiva
	//allora attivo la prima (la chiamata a CallCreateComponents mi arriverà
	//gratuitamente dal processo di attivazione)
	//questo codice è reso necessario dall'eliminazione della TabDialogActivate dalla create
	//della tab
	if (nFirstCreated < 0)
	{
		UINT nActiveTab = GetTileManager()->GetDlgInfoArray()->GetSize()
			? GetTileManager()->GetDlgInfoArray()->GetAt(0)->GetDialogID()
			: 0;
		if (nActiveTab)
		{
			GetTileManager()->TabDialogActivate(GetTileManager()->GetDlgCtrlID(), nActiveTab);
			GetTileManager()->AdjustTabManager();
		}
	}
}



//----------------------------------------------------------------------------
//Inserisce in foundChildren tutti i controlli che contengono il punto screenPoint.
//Discrimina tra container e non container: i controlli NON container sono
//aggiunti per primi nella collezione, i controlli container sono aggiunti per ultimi.
void MTileManager::GetChildrenFromPos(
	System::Drawing::Point screenPoint,
	IntPtr handleToSkip,
	System::Collections::Generic::ICollection<IWindowWrapper^>^ foundChildren
	)
{
	if (CurrentTileGroup != nullptr)
	{
		HWND hHandleToSkip = (HWND)(int)handleToSkip;
		CPoint aPt(screenPoint.X, screenPoint.Y);
		HWND hChild = (HWND)CurrentTileGroup->Handle.ToInt64();
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

		CurrentTileGroup->GetChildrenFromPos(screenPoint, handleToSkip, foundChildren);
	}
}

//----------------------------------------------------------------------------
void MTileManager::IntegrateLayout(ILayoutComponent^ layoutComponent)
{
	if (layoutComponent == nullptr)
		return;

	// prima ripulisco tutti i componenti precedenti
	MLayoutObject^ layoutObject = (MLayoutObject^)layoutComponent;
	layoutObject->ClearComponents();

	if (KeepTabsAlive)
	{
		// faccio il ricalcolo dell'intero tilemanager che a seconda dello
		// stato di keep alive o meno avrà uno o piu' tilegroup operativi
		for each (IComponent^ component in Components)
		{
			MTileGroup^ tileGroup = dynamic_cast<MTileGroup^>(component);
			if (tileGroup == nullptr || tileGroup->GetPtr() == IntPtr::Zero)
				continue;
			CTileGroup* pGroup = (CTileGroup*)tileGroup->GetPtr().ToInt32();
			if (pGroup)
				layoutObject->AddContainer(pGroup->GetLayoutContainer(), tileGroup);
		}
	}
	else
	{
		CTileGroup* pGroup = GetTileManager()->GetActiveTileGroup();
		if (pGroup)
			layoutObject->AddContainer(pGroup->GetLayoutContainer(), CurrentTileGroup);
	}
}

//----------------------------------------------------------------------------
void MTileManager::Add(System::ComponentModel::IComponent^ component, bool isChanged)
{
	__super::Add(component, isChanged);
}

//--------------------------------------
void MTileManager::LayoutChanged()
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

//-----------------------------------------------------------------------------------
void MTileManager::MoveTileGroup(MTileGroup^ tileGroup, int newIndex)
{
	if (components && components->Contains(tileGroup))
	{
		int oldIndex = components->IndexOf(tileGroup);

		if (oldIndex < 0)
			return;

		components->RemoveAt(oldIndex);
		components->Insert(newIndex, tileGroup);
		LayoutChanged();
	}
}