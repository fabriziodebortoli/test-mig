#include "stdafx.h"

#include "Utility.h"
#include "MTileDialog.h"
#include "MTileManager.h"
#include "MBodyEdit.h"
#include <TbGenlib\TilePanel.h>
#include <TbGes\TileDialog.h>

using namespace Microarea::Framework::TBApplicationWrapper;
using namespace Microarea::TaskBuilderNet::Core::Generic;
using namespace System;
using namespace System::Drawing;
using namespace System::Collections::Generic;
using namespace System::Reflection;

using namespace System::ComponentModel;
using namespace System::ComponentModel::Design;
using namespace System::ComponentModel::Design::Serialization;

using namespace ICSharpCode::NRefactory::CSharp;
using namespace ICSharpCode::NRefactory::PatternMatching;

/////////////////////////////////////////////////////////////////////////////
// 				class TileDialogSerializer Implementation
/////////////////////////////////////////////////////////////////////////////
//----------------------------------------------------------------------------	
const TCHAR szTileDesignStyleName[] = _T("DesignMode");

//----------------------------------------------------------------------------	
Statement^ TileDialogSerializer::GetConstructor(IDesignerSerializationManager^ manager, EasyBuilderControl^ ebControl)
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
TypeDeclaration^ TileDialogSerializer::SerializeClass(SyntaxTree^ syntaxTree, IComponent^ control)
{
	//Se la classe custom che devo generare esiste già, non devo creare niente
	String^ className = ((EasyBuilderComponent^)control)->SerializedType;

	TypeDeclaration^ controller = GetControllerTypeDeclaration(syntaxTree);
	String^ controllerName = controller->Name;

	TypeDeclaration^ aClass = gcnew TypeDeclaration();
	aClass->Modifiers = Modifiers::Public;
	aClass->Name = className;
	aClass->BaseTypes->Add(gcnew SimpleType(MTileDialog::typeid->FullName));

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
// 						class MTileDialog Implementation
/////////////////////////////////////////////////////////////////////////////

//----------------------------------------------------------------------------
MTileDialog::MTileDialog(IntPtr handleWndPtr)
	:
	WindowWrapperContainer(handleWndPtr),
	m_pTileDialog(NULL)
{
	HasCodeBehind = true;
	m_pTileDialog = (CBaseTileDialog*)GetWnd();

	ASSERT(m_pTileDialog);

	m_staticArea2 = nullptr;
}

//----------------------------------------------------------------------------
MTileDialog::MTileDialog(IWindowWrapperContainer^ parentWindow, System::String^ name, System::String^ className, Point location, bool hasCodeBehind)
	:
	WindowWrapperContainer(parentWindow, name, className, location, hasCodeBehind),
	m_pTileDialog(NULL)
{
	CWnd* pWnd = GetWnd();
	m_pTileDialog = dynamic_cast<CBaseTileDialog*>(pWnd);

	m_staticArea2 = nullptr;

	//if (!this->HasCodeBehind)
	//	jsonDescription = new CWndTileDescription(NULL);

}

//----------------------------------------------------------------------------
MTileDialog::~MTileDialog()
{
	this->!MTileDialog();
	m_pTileDialog = NULL;
}

//----------------------------------------------------------------------------
MTileDialog::!MTileDialog()
{
	if (HasCodeBehind || !m_pTileDialog)
		return;

	CBaseTileGroup* pParentGroup = m_pTileDialog->GetParentTileGroup();
	if (!pParentGroup)
		return;

	TileDialogArray* pTileDialogArray = pParentGroup->GetTileDialogs();
	pTileDialogArray->SetOwns(FALSE);
	for (int d = pTileDialogArray->GetCount() - 1; d >= 0; d--)
	{
		CBaseTileDialog* pDialog = pTileDialogArray->GetAt(d);
		if (pDialog && pDialog == m_pTileDialog)
		{
			pParentGroup->GetLayoutContainer()->RemoveChildElement(pDialog);
			pTileDialogArray->RemoveAt(d);
			break;
		}
	}

	pTileDialogArray->SetOwns(TRUE);

	if (IsWindow(m_pTileDialog->m_hWnd))
		m_pTileDialog->DestroyWindow();

	SAFE_DELETE(m_pTileDialog);

	//if (m_pTilePanel == NULL)
	//{
	//	m_pTileDialog->DestroyWindow();
	//	SAFE_DELETE(m_pTileDialog);
	//}
}

//----------------------------------------------------------------------------------
void MTileDialog::UpdateAttributesForJson(CWndObjDescription* pParentDescription)
{
	ASSERT(pParentDescription);
	if (!pParentDescription)
		return;

	if (!this->HasCodeBehind)
	{
		jsonDescription = pParentDescription->AddChildWindow(this->GetWnd(), this->Id);

		__super::UpdateAttributesForJson(pParentDescription);

		CWndTileDescription* pTileDescription = dynamic_cast<CWndTileDescription*>(jsonDescription);

		if (pTileDescription)
		{
			pTileDescription->m_X = 0;
			pTileDescription->m_Y = 0;
			pTileDescription->m_bHasStaticArea = (this->m_pTileDialog->HasStaticArea() == TRUE);
			switch (this->TileDialogType)
			{
			case ETileDialogSize::AutoFill:
				pTileDescription->m_Size = TileDialogSize::TILE_AUTOFILL;
				break;
			case ETileDialogSize::Standard:
				pTileDescription->m_Size = TileDialogSize::TILE_STANDARD;
				pTileDescription->m_Width = CUtility::GetIdealTileSizeLU(ETileDialogSize::Standard).Width;
				break;
			case ETileDialogSize::Wide:
				pTileDescription->m_Size = TileDialogSize::TILE_WIDE;
				pTileDescription->m_Width = CUtility::GetIdealTileSizeLU(ETileDialogSize::Wide).Width;
				break;
			case ETileDialogSize::Mini:
				pTileDescription->m_Size = TileDialogSize::TILE_MINI;
				pTileDescription->m_Width = CUtility::GetIdealTileSizeLU(ETileDialogSize::Mini).Width;
				break;
			}
		}
	}
	else
	{
		jsonDescription = new CDummyDescription();
		jsonDescription->m_Type = CWndObjDescription::WndObjType::Undefined;
		jsonDescription->m_strIds.Add(this->Id);
	}

}

//-------------------------------------------------------------------------------------------------------------
void MTileDialog::GenerateSerialization(CWndObjDescription* pParentDescription, List<System::Tuple<System::String^, System::String^>^>^ serialization)
{
	__super::GenerateSerialization(pParentDescription, serialization);

	//update parent description id with this (always)
	jsonDummyDescription = new CDummyDescription();
	jsonDummyDescription->m_Type = CWndObjDescription::WndObjType::Undefined;
	pParentDescription->m_Children.Add(jsonDummyDescription);
	jsonDummyDescription->m_arHrefHierarchy.Add(this->Id);

	if (!jsonDescription->IsKindOf(RUNTIME_CLASS(CDummyDescription)))
	{
		//serialize this => HasCodeBehind = false
		serialization->Add
		(
			gcnew Tuple<System::String^, System::String^>
			(
				gcnew String(this->Id),
				gcnew String(GetSerialization(jsonDescription))
				)
		);

	}
	else if (jsonDescription->m_Children.GetCount() > 0)    //serializza jsonDescription
	{
		serialization->Add
		(
			gcnew Tuple<System::String^, System::String^>
			(
				gcnew String(pParentDescription->m_strIds.GetAt(0) + _T("_") + this->Id),
				gcnew String(GetSerialization(jsonDescription))
				)
		);

		//non sono sul papà
		SAFE_DELETE(jsonDescription);
	}

	//clear parent
	for (int i = pParentDescription->m_Children.GetUpperBound(); i >= 0; i--)
	{
		if (pParentDescription->m_Children.GetAt(i) == jsonDescription)
		{
			SAFE_DELETE(pParentDescription->m_Children.GetAt(i));
			pParentDescription->m_Children.RemoveAt(i);
		}
	}
}

//----------------------------------------------------------------------------
HWND MTileDialog::GetControlHandle(const CTBNamespace& aNamespace)
{
	CWnd* pWnd = m_pTileDialog
		? m_pTileDialog->GetWndLinkedCtrl(aNamespace)
		: NULL;
	return pWnd ? pWnd->m_hWnd : NULL;
}

//----------------------------------------------------------------------------
TbResourceType MTileDialog::GetTbResourceType()
{
	return TbResources;
}

//----------------------------------------------------------------------------
bool MTileDialog::Create(IWindowWrapperContainer^ parentWindow, System::Drawing::Point location, System::String^ className)
{
	if (parentWindow == nullptr || !CanCreate())
		return false;

	CWnd* pParentWnd = ((BaseWindowWrapper^)parentWindow)->GetWnd();
	CPoint aPt(location.X, location.Y);

	CSize aSize(minSize.Width, minSize.Height);
	minSize.Width = CBaseTileDialog::GetTileWidth(TileDialogSize::TILE_STANDARD);
	SafeMapDialog(pParentWnd->m_hWnd, aSize);//trasformo in pixel

	CTBNamespace aNamespace = CString(CreateNamespaceFromParent(parentWindow));
	UINT nIDC = AfxGetTBResourcesMap()->GetTbResourceID(CString(Id), GetTbResourceType());

	if (pParentWnd->IsKindOf(RUNTIME_CLASS(CTilePanelTab)))
	{
		CTilePanelTab* pTab = (CTilePanelTab*)pParentWnd;
		CTilePanel* pTilePanel = pTab->GetTilePanel();
		m_pTileDialog = pTilePanel->AddTile(pTab, RUNTIME_CLASS(CEmptyTileDialog), IDD_EMPTY_TILE, aNamespace.GetObjectName(), ::TILE_STANDARD);
	}
	else if (pParentWnd->IsKindOf(RUNTIME_CLASS(CBaseTileGroup)))
	{
		CBaseTileGroup* pTileGroup = (CBaseTileGroup*)pParentWnd;
		m_pTileDialog = pTileGroup->AddTile(RUNTIME_CLASS(CEmptyTileDialog), IDD_EMPTY_TILE, aNamespace.GetObjectName(), ::TILE_STANDARD);
	}

	if (m_pTileDialog)
	{
		Parent = parentWindow;
		ParentComponent = (EasyBuilderComponent^)parentWindow;
		if (DesignModeType == EDesignMode::Runtime)
		{
			m_pTileDialog->SetMinWidth(minSize.Width);
			m_pTileDialog->SetMinHeight(minSize.Height);
			m_pTileDialog->SetMaxWidth(LayoutElement::ORIGINAL);
			m_pTileDialog->SetRequestedLastFlex(0);
			m_pTileDialog->SetFlex(0);
		}
		m_pTileDialog->GetNamespace() = aNamespace;
		m_pTileDialog->SetDlgCtrlID(nIDC);

		CSize currentSize(minSize.Width, minSize.Height);
		m_pTileDialog->ChangeSizeTo(currentSize, 1);

		Handle = (IntPtr)m_pTileDialog->m_hWnd;
	}

	if (this->DesignModeType == EDesignMode::Runtime && !m_pTileDialog->IsLayoutIntialized())
		m_pTileDialog->InitializeLayout();
	return m_pTileDialog != NULL;
}

//----------------------------------------------------------------------------
EContainerLayout	MTileDialog::ParentContainerLayout::get()
{
	if (!m_pTileDialog)
		return EContainerLayout::Column;

	LayoutElement* pElement = m_pTileDialog->GetParentElement();

	CLayoutContainer* pContainer = dynamic_cast<CLayoutContainer*>(pElement);
	if (!pContainer)
		return EContainerLayout::Column;

	CLayoutContainer::LayoutType layout = pContainer->GetLayoutType();

	switch (layout)
	{
	case CLayoutContainer::COLUMN:
		return EContainerLayout::Column;
	case CLayoutContainer::HBOX:
		return EContainerLayout::Hbox;
	case CLayoutContainer::VBOX:
		return EContainerLayout::Vbox;
	case CLayoutContainer::STRIPE:
		return EContainerLayout::Stripe;
	default:
		return EContainerLayout::Column;
	}
}

//----------------------------------------------------------------------------
void MTileDialog::ParentContainerLayout::set(EContainerLayout layoutType)
{
	if (!m_pTileDialog)
		return;

	LayoutElement* pElement = m_pTileDialog->GetParentElement();

	CLayoutContainer* pContainer = dynamic_cast<CLayoutContainer*>(pElement);
	if (!pContainer)
		return;

	switch (layoutType)
	{
	case EContainerLayout::Column:
		pContainer->SetLayoutType(CLayoutContainer::COLUMN);
		break;
	case EContainerLayout::Hbox:
		pContainer->SetLayoutType(CLayoutContainer::HBOX);
		break;
	case EContainerLayout::Vbox:
		pContainer->SetLayoutType(CLayoutContainer::VBOX);
		break;
	case EContainerLayout::Stripe:
		pContainer->SetLayoutType(CLayoutContainer::STRIPE);
		break;
	default:
		break;
	}
}


//----------------------------------------------------------------------------
ELayoutAlign MTileDialog::ParentLayoutAlign::get()
{
	if (!m_pTileDialog)
		return ELayoutAlign::Stretch;

	LayoutElement* pElement = m_pTileDialog->GetParentElement();

	CLayoutContainer* pContainer = dynamic_cast<CLayoutContainer*>(pElement);
	if (!pContainer)
		return ELayoutAlign::Stretch;

	if (DesignerVisible)
		return layoutAlignment;

	CLayoutContainer::LayoutAlign layout = pContainer->GetLayoutAlign();

	switch (layout)
	{
	case CLayoutContainer::BEGIN:
		return ELayoutAlign::Begin;
	case CLayoutContainer::MIDDLE:
		return ELayoutAlign::Middle;
	case CLayoutContainer::END:
		return ELayoutAlign::End;
	case CLayoutContainer::STRETCHMAX:
		return ELayoutAlign::StretchMax;
	case CLayoutContainer::NO_ALIGN:
		return ELayoutAlign::NoAlign;
	default:
		return ELayoutAlign::Stretch;
	}
}

//----------------------------------------------------------------------------
bool MTileDialog::HasOwnEditing::get()
{
	return (DesignModeType == EDesignMode::Runtime);
}

//----------------------------------------------------------------------------
void MTileDialog::AfterOwnEditingSwitching(bool onwEditingOn, bool IAmInEditing)
{
	if (!m_pTileDialog)
		return;

	inOwnEditing = IAmInEditing;
	bool visible = inOwnEditing;
	if (onwEditingOn)
		oldVisible = m_pTileDialog->IsVisible() == TRUE;
	else
		visible = oldVisible;

	m_pTileDialog->Show(visible);

	AfterSelectionChanged(IAmInEditing);
}

//----------------------------------------------------------------------------
void MTileDialog::AfterSelectionChanged(bool IAmSelected)
{
	TileStyle* pDesignStyle = AfxGetTileDialogStyle(_T("DesignMode"));
	if (m_pTileDialog && m_pTileDialog->GetTileStyle() && pDesignStyle)
	{
		m_pTileDialog->GetTileStyle()->UseAlternativeColorsOf(IAmSelected ? pDesignStyle : NULL);
		m_pTileDialog->Invalidate();
		m_pTileDialog->UpdateWindow();
	}
}

//-----------------------------------------------------------------------------------
void MTileDialog::Invalidate()
{
	IWindowWrapperContainer^ pParent = Parent;
	MTileGroup^ pTileGroup = dynamic_cast<MTileGroup^>(pParent);

	if (pTileGroup != nullptr)
	{
		__super::Invalidate();
		return;
	}

	pParent = pParent->Parent;
	BOOL bFound = FALSE;
	while (pParent && !bFound)
	{
		pTileGroup = dynamic_cast<MTileGroup^>(pParent);
		if (pTileGroup != nullptr)
		{
			pTileGroup->Invalidate();
			bFound = TRUE;
		}
		else
			pParent = pParent->Parent;
	}

	if (!bFound || (pParent == nullptr))
		__super::Invalidate();
}

//--------------------------------------------------------------------------------------
void MTileDialog::UpdateWindow()
{
	IWindowWrapperContainer^ pParent = Parent;
	MTileGroup^ pTileGroup = dynamic_cast<MTileGroup^>(pParent);

	if (pTileGroup != nullptr)
	{
		__super::UpdateWindow();
		return;
	}

	pParent = pParent->Parent;
	BOOL bFound = FALSE;
	while (pParent && !bFound)
	{
		pTileGroup = dynamic_cast<MTileGroup^>(pParent);
		if (pTileGroup != nullptr)
		{
			pTileGroup->UpdateWindow();
			bFound = TRUE;
		}
		else
			pParent = pParent->Parent;
	}

	if (!bFound || (pParent == nullptr))
		__super::UpdateWindow();
	
}

//----------------------------------------------------------------------------
void MTileDialog::ParentLayoutAlign::set(ELayoutAlign layoutType)
{
	if (!m_pTileDialog)
		return;

	LayoutElement* pElement = m_pTileDialog->GetParentElement();

	CLayoutContainer* pContainer = dynamic_cast<CLayoutContainer*>(pElement);
	if (!pContainer)
		return;

	layoutAlignment = layoutType;
	if (DesignerVisible)
		return;

	switch (layoutType)
	{

	case ELayoutAlign::Begin:
		pContainer->SetLayoutAlign(CLayoutContainer::BEGIN);
		break;
	case ELayoutAlign::Middle:
		pContainer->SetLayoutAlign(CLayoutContainer::MIDDLE);
		break;
	case ELayoutAlign::End:
		pContainer->SetLayoutAlign(CLayoutContainer::END);
		break;
	case ELayoutAlign::StretchMax:
		pContainer->SetLayoutAlign(CLayoutContainer::STRETCHMAX);
		break;
	case ELayoutAlign::NoAlign:
		pContainer->SetLayoutAlign(CLayoutContainer::NO_ALIGN);
		break;
	default:
	case ELayoutAlign::Stretch:
		pContainer->SetLayoutAlign(CLayoutContainer::STRETCH);
		break;
	}
}

//----------------------------------------------------------------------------
void MTileDialog::Name::set(System::String^ value)
{
	CString sName = CString(value);
	if (m_pTileDialog)
		m_pTileDialog->GetNamespace().SetObjectName(sName, TRUE);
}

//----------------------------------------------------------------------------
System::String^	MTileDialog::Name::get()
{
	if (m_pTileDialog)
		return gcnew System::String(m_pTileDialog->GetNamespace().GetObjectName());

	return System::String::Empty;
}

//----------------------------------------------------------------------------
bool MTileDialog::Equals(Object^ obj)
{
	if (
		obj == nullptr ||
		!(obj->GetType()->IsSubclassOf(MTileDialog::typeid) || MTileDialog::typeid->IsInstanceOfType(obj))
		)
		return false;

	MTileDialog^ aTBTab = (MTileDialog^)obj;
	// prima controllo lo stesso puntatore, 
	if (this->GetPtr() == aTBTab->GetPtr())
		return true;

	// ma qualora non corrispondesse potrei aver già costruito
	// la stessa dialog nella createComponents precedente come hasCodeBehind false
	if (!this->HasCodeBehind && !aTBTab->HasCodeBehind)
		return String::Compare(this->Namespace->FullNameSpace, aTBTab->Namespace->FullNameSpace) == 0;

	return false;
}


//----------------------------------------------------------------------------
System::String^ MTileDialog::SerializedName::get()
{
	return System::String::Concat
	(
		"tile_",
		EasyBuilderControlSerializer::Escape(String::IsNullOrEmpty(Name) ? Id : Name)
	);
}

//----------------------------------------------------------------------------
System::String^ MTileDialog::SerializedType::get()
{
	return System::String::Concat("Tile", EasyBuilderControlSerializer::Escape(Name));
}

//----------------------------------------------------------------------------
System::String^ MTileDialog::Text::get()
{
	if (!m_pTileDialog)
		return gcnew System::String("");

	return gcnew System::String(m_pTileDialog->GetTitle());
}

//----------------------------------------------------------------------------
void MTileDialog::Text::set(System::String^ value)
{
	if (m_pTileDialog)
		m_pTileDialog->SetTitle(value);
}


//----------------------------------------------------------------------------
ETileDialogSize MTileDialog::TileDialogType::get()
{
	if (!m_pTileDialog)
		return ETileDialogSize::Standard;

	return (ETileDialogSize)(int)m_pTileDialog->GetTileSize();
}

//----------------------------------------------------------------------------
void MTileDialog::TileDialogType::set(ETileDialogSize size)
{
	if (m_pTileDialog)
	{
		m_pTileDialog->SetTileSize((TileDialogSize)(int)size);
		System::Drawing::Size idealSize = CUtility::GetIdealTileSizeLU(size);

		idealSize.Width = CBaseTileDialog::GetTileWidth(CUtility::TransfertEnumTileSize(size));

		System::Drawing::Size value;
		if (HasCodeBehind)
			value = System::Drawing::Size
			(
				idealSize.Width > Size.Width ? idealSize.Width : SizeLU.Width,
				idealSize.Height > Size.Height ? idealSize.Height : SizeLU.Height
			);
		else
			value = idealSize;

		SizeLU = value;
		CSize aSize(value.Width, value.Height);
		m_pTileDialog->ChangeSizeTo(aSize, TRUE);

		if (DesignMode && this->Site != nullptr)
			AddStaticArea(this);

		ManageChildren(size);
	}
}

//--------------------------------------------------------------------------------
void MTileDialog::ManageChildren(ETileDialogSize size)
{
	if (HasCodeBehind || size != ETileDialogSize::AutoFill)
		return;

	bool bHasBrothers = false;
	System::Collections::Generic::List<BaseWindowWrapper^> stretchables;

	//verify bodyedit's existing
	for each (IWindowWrapper^ ctrl in Components)
	{
		BaseWindowWrapper^ stretchable = dynamic_cast<BaseWindowWrapper^>(ctrl);

		if (stretchable == nullptr)
			continue;

		if (stretchable->IsStretchable)
			stretchables.Add(stretchable);
		else
			if (!bHasBrothers)
				bHasBrothers = TRUE;
	}

	if (stretchables.Count == 0)
		return;

	bool bFillParent = false;
	bool bAutoStretch = false;

	if (stretchables.Count > 1)
	{
		bFillParent = false;
		bAutoStretch = false;
	}
	else
	{
		bFillParent = !bHasBrothers;
		bAutoStretch = bHasBrothers;
	}

	for each (BaseWindowWrapper^ stretchable in stretchables)
	{
		if (stretchable == nullptr)
			continue;

		bool bOldFillParent = stretchable->AutoFill;
		bool bOldBottomStretch = stretchable->BottomStretch;
		bool bOldRightStretch = stretchable->RightStretch;

		stretchable->AutoFill = bFillParent;
		stretchable->BottomStretch = bAutoStretch;
		stretchable->RightStretch = bAutoStretch;

		PropertyChangingNotifier::OnComponentPropertyChanged(Site, stretchable, "AutoFill", bOldFillParent, bFillParent);
		PropertyChangingNotifier::OnComponentPropertyChanged(Site, stretchable, "BottomStretch", bOldBottomStretch, bAutoStretch);
		PropertyChangingNotifier::OnComponentPropertyChanged(Site, stretchable, "RightStretch", bOldRightStretch, bAutoStretch);

		stretchable->AddChangedProperty("AutoFill");
		stretchable->AddChangedProperty("BottomStretch");
		stretchable->AddChangedProperty("RightStretch");
	}

	
}

//-----------------------------------------------------------------------------------
bool MTileDialog::HasBrothers(BaseWindowWrapper^ ctrl)
{
	return Components->Count > 1;
}

//----------------------------------------------------------------------------
void MTileDialog::Size::set(System::Drawing::Size value)
{
	__super::Size = value;
	if (!m_pTileDialog || DesignModeType == EDesignMode::Static)
		return;


	CSize aSize(Size.Width, Size.Height);
	//1 fa si che si cancellano le part e si rigenerano
	//in caso di WIDE, si devono cancellare e rigenerare le part, altrimenti la seconda part arriva con x = 572 ed in 
	//relayout della part di calcola offset della sua x attuale che e' 0 - rectActual.left (che e 572) => va fuori area
	m_pTileDialog->ChangeSizeTo(aSize, 1);	

	if (!m_pTileDialog->HasStaticArea())
		return;
		
	for each (IComponent^ component  in Components)
	{
		if (component->GetType() != GenericGroupBox::typeid)
			continue;

		GenericGroupBox^ groupBox = (GenericGroupBox^)component;
		UINT idStaticArea = groupBox->GetWnd()->GetDlgCtrlID();
		if (IsStaticArea(idStaticArea) && groupBox->Rectangle.Bottom > this->Rectangle.Bottom) 
		{
			groupBox->Size = System::Drawing::Size(
				groupBox->Size.Width, 
				groupBox->Size.Height - (groupBox->Rectangle.Bottom - this->Rectangle.Bottom));
		}
	}
}

//----------------------------------------------------------------------------
bool  MTileDialog::Collapsible::get()
{
	return m_pTileDialog && m_pTileDialog->GetTileStyle()->Collapsible() == TRUE;
}

//----------------------------------------------------------------------------
String^ MTileDialog::TileStyleName::get()
{
	if (!m_pTileDialog)
		return String::Empty;
	tileStyleName = gcnew String(m_pTileDialog->GetTileStyle()->GetName());
	return tileStyleName;
}

//----------------------------------------------------------------------------
void  MTileDialog::TileStyleName::set(String^ value)
{
	if (!m_pTileDialog)
		return;
	tileStyleName = value;
	if (!DesignerVisible)
		m_pTileDialog->SetTileStyleByName(CString(value));
}

//-----------------------------------------------------------------------
void  MTileDialog::Collapsible::set(bool value)
{
	if (m_pTileDialog)
		m_pTileDialog->SetCollapsible(value);
}

//----------------------------------------------------------------------------
bool  MTileDialog::Collapsed::get()
{
	if (!m_pTileDialog)
		return false;

	return  DesignerVisible ? collapsed :
		m_pTileDialog->IsCollapsed() == TRUE;
}

//----------------------------------------------------------------------------
void  MTileDialog::Collapsed::set(bool value)
{
	if (!m_pTileDialog)
		return;
	collapsed = value;

	//se imposto di avere una tile collapsed, in automatico la cambio in collapsible
	if (collapsed)
		Collapsible = true;

	if (!DesignerVisible)
		m_pTileDialog->SetCollapsed(value);
}


//----------------------------------------------------------------------------
int MTileDialog::Flex::get()
{
	if (!m_pTileDialog)
		return -1;

	return  DesignerVisible ? flex :
		((LayoutElement*)m_pTileDialog)->GetFlex(LayoutElement::WIDTH);
}

//----------------------------------------------------------------------------
void MTileDialog::Flex::set(int value)
{
	if (!m_pTileDialog)
		return;
	flex = value;
	if (!DesignerVisible)
		((LayoutElement*)m_pTileDialog)->SetFlex(value);
}

//----------------------------------------------------------------------------
System::Drawing::Size MTileDialog::MinSize::get()
{
	int widthCalc = CBaseTileDialog::GetTileWidth(CUtility::TransfertEnumTileSize(MTileDialog::TileDialogType));
	int width = Math::Max(widthCalc, ((BaseWindowWrapper^)this)->MinSize.Width);
	return  System::Drawing::Size(width, ((BaseWindowWrapper^)this)->MinSize.Height);
}

//----------------------------------------------------------------------------
bool MTileDialog::GroupCollapsible::get()
{
	return  m_pTileDialog && m_pTileDialog->IsGroupCollapsible() == TRUE;
}
//----------------------------------------------------------------------------
void MTileDialog::GroupCollapsible::set(bool value)
{
	if (m_pTileDialog)
		m_pTileDialog->SetGroupCollapsible(value);
}

//----------------------------------------------------------------------------
bool  MTileDialog::Pinnable::get()
{
	if (!m_pTileDialog)
		return false;

	return m_pTileDialog->GetTileStyle()->Pinnable() == TRUE;
}

//----------------------------------------------------------------------------
void  MTileDialog::Pinnable::set(bool value)
{
	if (!m_pTileDialog)
		return;

	m_pTileDialog->SetPinnable(value);
}

//----------------------------------------------------------------------------
bool  MTileDialog::Pinned::get()
{
	if (!m_pTileDialog)
		return false;

	return m_pTileDialog->IsPinned() == TRUE;
}

//----------------------------------------------------------------------------
void  MTileDialog::Pinned::set(bool value)
{
	if (!m_pTileDialog)
		return;

	m_pTileDialog->SetPinned(value);
}

//----------------------------------------------------------------------------
int MTileDialog::TabOrder::get() 
{ 
	MTileGroup^ tileGroup = dynamic_cast<MTileGroup^>(Parent);
	return tileGroup != nullptr ? tileGroup->GetTileIndex(this) : -1;
}

//----------------------------------------------------------------------------
void MTileDialog::TabOrder::set (int value)
{ 
	MTileGroup^ tileGroup = dynamic_cast<MTileGroup^>(Parent);
	if (tileGroup != nullptr)
		tileGroup->MoveTile(this, value);
}

//----------------------------------------------------------------------------
bool MTileDialog::CanCreate()
{
	return !GetHandle();
}

//----------------------------------------------------------------------------
IntPtr MTileDialog::Handle::get()
{
	HWND hwnd = NULL;
	if (m_pTileDialog)
		hwnd = m_pTileDialog->m_hWnd; //lazy initialization: se ho una tab attiva, ne prendo l'handle

	if (!hwnd)
		return __super::Handle;

	if ((HWND)(int)__super::Handle != hwnd)
		__super::Handle = (IntPtr)hwnd;

	return __super::Handle;
}

//----------------------------------------------------------------------------
bool MTileDialog::CanDropTarget(Type^ droppedObject)
{
	return !(WindowWrapperContainer::typeid == droppedObject || droppedObject->IsSubclassOf(WindowWrapperContainer::typeid));
}

//----------------------------------------------------------------------------
void MTileDialog::OnDesignerControlCreated() 
{ 
	SizeLU = AdjustMinSizeOnParent(this, Parent);
	AddStaticArea(this);
}

//----------------------------------------------------------------------------
void MTileDialog::Initialize()
{
	__super::Initialize();
	minSize = CUtility::GetIdealTileSizeLU(ETileDialogSize::Standard);
}

//-----------------------------------------------------------------------------
EditingMode MTileDialog::DesignerMovable::get()
{
	return EditingMode::ResizingMidBottom | EditingMode::ResizingMidRight | EditingMode::ResizingBottomRight;
}

//-----------------------------------------------------------------------------
void MTileDialog::Add(IComponent^ component, System::String^ name)
{
	__super::Add(component, name);
	GenericGroupBox^ groupBox = dynamic_cast<GenericGroupBox^>(component);
	if (groupBox && !groupBox->HasCodeBehind && groupBox->GetWnd())
	{
		CWnd* pWnd = groupBox->GetWnd();
		if (IsStaticArea(pWnd->GetDlgCtrlID()))
		{
			pWnd->SetWindowText(DesignModeType == EDesignMode::Runtime ? _T("Static Area") : _T(""));
			m_pTileDialog->RecalcParts();
			// se sono in design sicuramente il titolo e' già stato visualizzato e quindi 
			// e' già compreso nell'area di finestra, mentre se non e' ancora stato fatto il 
			// primo disegno della finestra, il titolo non e' ancora stato posizionato e tutto si 
			// sposterà successivamente. Senza questo aggiustamento, la static area e' sempre 
			// posizionata due volte sotto il titolo a seconda che sia stato disegnato o meno
			if (!m_pTileDialog->IsLayoutIntialized() && DesignModeType == EDesignMode::None)
				groupBox->Location = Point(groupBox->Location.X, 0);
		}
	}
}

//-----------------------------------------------------------------------------
void MTileDialog::AddStaticArea(MTileDialog^ tileDialog)
{
	if (!m_pTileDialog)
		return;

	switch (TileDialogType)
	{
	case ETileDialogSize::Standard:
		tileDialog->AddStaticArea(IDC_STATIC_AREA, 0);
		break;
	case ETileDialogSize::Wide:
		tileDialog->AddStaticArea(IDC_STATIC_AREA, 0);
		tileDialog->AddStaticArea(IDC_STATIC_AREA_2, 327);
		break;
	}
	m_pTileDialog->RecalcParts();

	if (TileDialogType != ETileDialogSize::Wide)
		return;

	CRect aRect = m_pTileDialog->GetStaticAreaRect(1);
	m_staticArea2->Location = Point(aRect.left, aRect.top);
}

//-----------------------------------------------------------------------------
BOOL MTileDialog::AddStaticArea(UINT nID, int nLeft)
{
	if (!m_pTileDialog)
		return FALSE;

	CString sName = AfxGetTBResourcesMap()->DecodeID(TbControls, nID).m_strName;
	String^ staticAreaName = gcnew String(sName);
	GenericGroupBox^ staticArea = nullptr;
	for each (IComponent^ component  in Components)
	{
		if (component->GetType() != GenericGroupBox::typeid)
			continue;

		GenericGroupBox^ groupBox = (GenericGroupBox^)component;
		if (String::Compare(groupBox->Name, staticAreaName) == 0)
		{
			staticArea = groupBox;
			break;
		}
	}
	if (staticArea != nullptr)
		return FALSE;

	Point location(nLeft, 0);

	staticArea = gcnew GenericGroupBox(this, staticAreaName, "", location, location, false);
	// evito di far scattare il changed, e' solo informativo
	int nTileHeight = m_pTileDialog->GetTitleHeight() + 1;
	staticArea->Size = System::Drawing::Size(CUtility::GetIdealStaticAreaSize().Width, CUtility::GetIdealStaticAreaSize().Height - nTileHeight);
	
	

	if (TileDialogType == ETileDialogSize::Wide)
	{
		m_staticArea2 = staticArea;
	}
	else
		staticArea->Location = Point(nLeft, nTileHeight);
	components->Add(staticArea);
	PropertyChangingNotifier::OnComponentAdded(this, staticArea, true);
	return TRUE;
}

//-----------------------------------------------------------------------------
bool MTileDialog::IsStaticArea(UINT nID)
{
	return CUtility::IsStaticArea(nID);
}

//-----------------------------------------------------------------------------
void MTileDialog::CallCreateComponents()
{
	__super::CallCreateComponents();
}

//-----------------------------------------------------------------------------
void MTileDialog::AfterWndProc(System::Windows::Forms::Message% m)
{
	__super::AfterWndProc(m);

	if (m_pTileDialog && m.Msg == UM_TILEPART_AFTER_RELAYOUT && m_pTileDialog->HasParts() && m_pTileDialog->IsLayoutIntialized())
		DelayedPartsAnchor();
}

//----------------------------------------------------------------------------
void MTileDialog::DelayedPartsAnchor()
{
	for each (IComponent^ component in this->Components)
	{
		BaseWindowWrapper^ wrapper = dynamic_cast<BaseWindowWrapper^>(component);
		if (wrapper != nullptr)
			wrapper->DelayedPartsAnchor();
	}
}
