// cannot use C++ managed namespaces in order to avoid stdafx.h conflicts
#include "stdafx.h"
#include "windows.h"
#include <TbGeneric\WndObjDescription.h>
#include <TbGeneric\VisualStylesXP.h>
#include <TbGenlib\TBToolbar.h>
#include <TbGenlib\generic.hjson> //JSON AUTOMATIC UPDATE
#include <TbGes\dbt.h>
#include <TbGes\tabber.h>
#include <TbGenLibManaged\\UserControlHandlersMixed.h>
#include <TbGenLib\OslBaseInterface.h>
#include <TbGenLib\OslInfo.h>
#include <TbGeneric\TBThemeManager.h>
#include "MParsedControls.h"
#include "GenericControls.h"
#include "MView.h"
#include "MToolbar.h"

using namespace Microarea::Framework::TBApplicationWrapper;
using namespace System::Windows::Forms;
using namespace System::IO;
using namespace System;
using namespace System::Collections::Generic;
using namespace System::Windows::Forms::Design;
using namespace System::CodeDom;
using namespace System::ComponentModel::Design::Serialization;
using namespace System::Drawing;
using namespace System::ComponentModel;

using namespace ICSharpCode::NRefactory::CSharp; 
using namespace ICSharpCode::NRefactory::PatternMatching;

struct _TREEITEM {};
struct _IMAGELIST {};
/////////////////////////////////////////////////////////////////////////////
// 						class MToolbar Implementation
/////////////////////////////////////////////////////////////////////////////
//----------------------------------------------------------------------------	
Statement^ MToolbarSerializer::GetConstructor(IDesignerSerializationManager^ manager, EasyBuilderControl^ ebControl)
{
	Point currentLocation = GetLocationToSerialize(ebControl);
	String^ className = ebControl->SerializedType;
	IdentifierExpression^ variableDeclExpression = gcnew IdentifierExpression(ebControl->SerializedName);
	ObjectCreateExpression^ creationExpression =
		AstFacilities::GetObjectCreationExpression
		(
			gcnew SimpleType(className),
			gcnew ThisReferenceExpression(),
			gcnew PrimitiveExpression(ebControl->Name),
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
TypeDeclaration^ MToolbarSerializer::SerializeClass (SyntaxTree^ syntaxTree, IComponent^ component)
{
	if (
		component == nullptr ||
		(component->GetType() != MToolbar::typeid && !component->GetType()->IsSubclassOf(MToolbar::typeid))
		)
		return nullptr;
	
	MToolbar^ toolbar = (MToolbar^) component; 	
	System::String^ className = toolbar->SerializedType;
	//Se la classe custom che devo generare esiste già, non devo creare niente
	TypeDeclaration^ aClass = FindClass(syntaxTree, className);
	if (aClass != nullptr)
		return aClass;

	TypeDeclaration^ controller = GetControllerTypeDeclaration(syntaxTree);
	String^ controllerName = controller->Name;

	aClass = gcnew TypeDeclaration();
	aClass->Modifiers = Modifiers::Public;
	aClass->Name = className;
	aClass->BaseTypes->Add(gcnew SimpleType(MToolbar::typeid->FullName));
	
	// Costruttore 
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
	
	return aClass;
}

/////////////////////////////////////////////////////////////////////////////
// 						class MToolbar Implementation
/////////////////////////////////////////////////////////////////////////////

//----------------------------------------------------------------------------
MToolbar::MToolbar (IWindowWrapperContainer^ parentWindow, System::String^ name, System::String^ controlClass, Point location, bool hasCodeBehind)
	:
	WindowWrapperContainer(parentWindow, name, controlClass, location, hasCodeBehind)
{
	minSize = CUtility::GetIdealToolbarSize(true);
}

//----------------------------------------------------------------------------
MToolbar::MToolbar (IntPtr handle)
	:
	WindowWrapperContainer(handle)
{
	m_bBefore = 0x00;
}

//----------------------------------------------------------------------------
MToolbar::~MToolbar  ()
{
	this->!MToolbar();
	GC::SuppressFinalize(this);
}

//----------------------------------------------------------------------------
MToolbar::!MToolbar ()
{
	// costringo la dispose del toolstrip
	// per farlo pulire bene ad ogni salvataggio di editing

	/*MView^ view = dynamic_cast<MView^>(this->parent);
	if (view == nullptr)
		return;

	CAbstractFormView* pView = (CAbstractFormView*)view->GetWnd();
	CAbstractFormFrame* pFrame = pView ? pView->GetFrame() : NULL;

	if (!pFrame)
		return;

	CTBTabbedToolbar* pTabbedToolBar = pFrame->GetTabbedToolBar();

	if (!pTabbedToolBar)
		return;

	BOOL bOk = pTabbedToolBar->RemoveTab(GetToolbarName());
	ASSERT(bOk);*/

	if (m_pTBToolbar)
	{
		m_pTBToolbar->DestroyWindow();
		delete m_pTBToolbar;
	}
	if (m_pInfoOSL)
		delete m_pInfoOSL;
}

//----------------------------------------------------------------------------
System::String^ MToolbar::SerializedType::get ()
{ 
	return System::String::Concat("MT", EasyBuilderSerializer::Escape(Name)); 
}

//----------------------------------------------------------------------------
bool MToolbar::CanCreate ()
{
	return true;
}

//----------------------------------------------------------------------------
bool MToolbar::Create (IWindowWrapperContainer^ parentWindow, System::Drawing::Point location, System::String^ className)
{
	parent = parentWindow;

	MView^ view = dynamic_cast<MView^>(parentWindow);
	if (view == nullptr)
		return false;

	CAbstractFormView* pView = (CAbstractFormView*) view->GetWnd();
	CAbstractFormFrame* pFrame = pView ? pView->GetFrame() : NULL;

	if (!pFrame)
		return false;

	//CRect rectMaster, recFrame;
	//pToolbar->GetClientRect(&rectMaster);
	//pFrame->GetClientRect(recFrame);

	CTBNamespace aNs = CreateNamespaceFromParent((MDocument^) view->Document);
	//
	// gestione grant di security
	m_pInfoOSL = new CInfoOSL(aNs, OSLType_Control);
	m_pInfoOSL->m_pParent = pView->GetDocument()->GetInfoOSL();
	AfxGetSecurityInterface()->GetObjectGrant (m_pInfoOSL);

	CTBTabbedToolbar* pTabbedToolBar = pFrame->GetTabbedToolBar();

	if (!pTabbedToolBar)
		return false;

	CString toolbarName = GetToolbarName();
	m_pTBToolbar = pFrame->CreateEmptyToolBar(toolbarName, toolbarName);

	if (!m_pTBToolbar)
		return false;

	/*if (!pTabbedToolBar->AddTab(m_pTBToolbar))
	{
		return false;
	}*/

	Handle = (IntPtr)m_pTBToolbar->m_hWnd;

	/*isUsingLargeButtons = pToolbar->IsUsingLargeButtons() == TRUE;

	toolStrip = gcnew System::Windows::Forms::ToolStrip();
	toolStrip->ImageScalingSize.Height = GetButtonSize();
	toolStrip->ImageScalingSize.Width = GetButtonSize();
	toolStrip->Dock = System::Windows::Forms::DockStyle::Top;
	
	CTaskBuilderToolBar* pToolbar = pFrame->GetAuxToolBar();
	

	if (view->NextTBPos == Point(0,0))
	{
		view->NextTBPos = Point(0, GetButtonHeight() -3 );
		if (pToolbar)
		{
			CRect rect;
			CToolBarCtrl& bar = pToolbar->GetToolBarCtrl();
			pToolbar->GetWindowRect(&rect);
			pToolbar->ScreenToClient(&rect);
			rect.OffsetRect( (bar.GetButtonCount()+1) * GetButtonHeight(), GetButtonHeight()); 
			view->NextTBPos = Point(rect.left , rect.top);	
		}
	}
	else
	{
		// add new line in toolbar container.!
		if (view->NextTBPos.X + GetButtonSize()*5 >= recFrame.right)
		{
			view->NextTBPos = Point(0, view->NextTBPos.Y + GetButtonHeight());
			// adjacent c# toolbar
			if (view->toolBarLevel > 0)
				view->NextTBPos.Y -= 10;
			view->toolBarLevel++;
		}
	}

	// append to the new position the ToolStrip
	toolStrip->Location = view->NextTBPos;
	// space between adjacent toolbar 
	view->NextTBPos = Point(view->NextTBPos.X + GetButtonSize(), view->NextTBPos.Y);
	// set init sizi of toolStrip - the toolStrip is clean
	toolStrip->Size = System::Drawing::Size(recFrame.Width() *2 , rectMaster.Height()) ;
	toolStrip->Stretch = true;

	// devo settare il colore di default del contesto di login
	COLORREF bkgColor = AfxGetThemeManager()->GetBackgroundColor();

	int r = GetRValue(bkgColor);
	int g = GetGValue(bkgColor);
	int b = GetBValue(bkgColor);
	toolStrip->BackColor = System::Drawing::Color::FromArgb(r,g,b);
	
	toolStrip->CreateControl();*/

	CWnd* pWnd = GetWnd();

	if (!pWnd)
		return false;

	pWnd->SetParent(pFrame);

	// chiamo la proprietà perchè deve controllare i grant
	Enabled = !DesignMode;
	return true;
}

//-----------------------------------------------------------------------------
CString MToolbar::GetToolbarName()
{
	CTBNamespace aNamespace = m_pInfoOSL->m_Namespace;
	return aNamespace.GetApplicationName();
}

//-----------------------------------------------------------------------------
// traslate c# Shortcut Enumeration to MFC VC++
/* http://msdn.microsoft.com/en-us/library/system.windows.forms.shortcut.aspx */

void MToolbar::PreTranslateMsgKey (UINT Msg, WPARAM wParam, LPARAM lParam)
{
	if (Msg == WM_KEYDOWN || WM_SYSKEYDOWN)
	{
		int wChar = (int)(wParam);

		if (wChar == VK_CONTROL) {
			m_bBefore = m_bBefore | 0x01;
			return;
		}

		if (wChar == VK_SHIFT) {
			m_bBefore = m_bBefore | 0x02;
			return;
		}

		if (wChar == VK_MENU) {
			m_bBefore = m_bBefore | 0x04;
			return;
		}

		System::String^ sShort = _T("");
		if ((m_bBefore & 0x01) == 0x01) // VK_CONTROL
			sShort += _T("Ctrl");
		if ((m_bBefore & 0x02) == 0x02) // VK_SHIFT
			sShort += _T("Shift");
		if ((m_bBefore & 0x04) == 0x04) // VK_MENU (Alt)
			sShort += _T("Alt");

		if (wChar >= VK_F1 && wChar <= VK_F24)
		{
			wChar = 1 + wChar - VK_F1;
			sShort += "F" + wChar.ToString();
		}
		else if (wChar == VK_INSERT)
			sShort += _T("Ins");
		else if (wChar == VK_DELETE)
			sShort += _T("Del");
		else if (wChar == VK_BACK)
			sShort += _T("Bksp");
		else			
			sShort += Convert::ToChar(wChar);

		for each (MToolbarItem^ item in Components)
			item->onKeyDown(sShort);
		
		m_bBefore = 0x00;
	}
}

//----------------------------------------------------------------------------
int	MToolbar::GetButtonSize	()
{
	return isUsingLargeButtons ? 24 : 16;
}

//----------------------------------------------------------------------------
int	MToolbar::GetButtonHeight	()
{
	return GetButtonSize() + 12;
}

//----------------------------------------------------------------------------
bool MToolbar::CreateWrappers(array<IntPtr>^ handlesToSkip)
{
	return true;
}

//----------------------------------------------------------------------------
INameSpace^ MToolbar::Namespace::get ()	
{ 
	return m_pInfoOSL ? gcnew NameSpace(gcnew System::String(m_pInfoOSL->m_Namespace.ToString())) : NameSpace::Empty;
} 

//-----------------------------------------------------------------------------
int	MToolbar::GetNamespaceType ()
{
	return CTBNamespace::TOOLBAR;
}

//----------------------------------------------------------------------------
System::String^ MToolbar::Name::get ()	
{ 
	return Namespace->Leaf;
} 

//----------------------------------------------------------------------------
System::String^	MToolbar::SerializedName::get () 
{ 
	return EasyBuilderSerializer::Escape(Name); 
}

//----------------------------------------------------------------------------
System::Drawing::Rectangle MToolbar::Rectangle::get()
{
	if (m_pTBToolbar)
	{
		return __super::Rectangle;
	}

	return System::Drawing::Rectangle(0, 0, 0, 0);
}

//----------------------------------------------------------------------------
bool MToolbar::CanUpdateTarget(Type^ droppedObject)
{
	return droppedObject == MPushButton::typeid  || droppedObject == MLabel::typeid ||  
		   droppedObject == MParsedCombo::typeid || droppedObject == MParsedStatic::typeid;
}

//----------------------------------------------------------------------------
bool MToolbar::CanDropTarget(Type^ droppedObject)
{
	return CanUpdateTarget(droppedObject);
}

//----------------------------------------------------------------------------
void MToolbar::UpdateTargetFromDrop (Type^ droppedType) 
{
	System::String^ type = MToolbarItem::TypeToControlClass(droppedType);
	MToolbarItem^ item = gcnew MToolbarItem(this, System::String::Format("New{0}{1}", type, Components->Count) , type, Point(0,0), false);
	Add(item);
	PropertyChangingNotifier::OnComponentAdded(this, item, true);
}

//----------------------------------------------------------------------------
void MToolbar::Add (IComponent^ component) 
{
	MToolbarItem^ item = ((MToolbarItem^) component);
	item->TabOrder = Components->Count;
	item->Enabled = true;

	// lo aggiungo dopo in modo da non far scattare il riordino del tabOrder
	__super::Add(component);

	// append new element, new width to add
	NextTBPosAddWidth(item->GetItemSize().Width);
}

//----------------------------------------------------------------------------
void MToolbar::MoveTo (System::String^ name, ToolStripItem^ item, int index) 
{
	EasyBuilderComponent^ component = (EasyBuilderComponent^) GetComponent(name);
	if (component == nullptr)
		return;
	
	//// prima agisco sul controllo grafico
	//m_pTBToolbar->Items->Remove(item);

	//if (index >= (m_pTBToolbar->Items->Count - 1))
	//	m_pTBToolbar->Items->Add(item);
	//else
	//	m_pTBToolbar->Items->Insert(index, item);

	// quindi devo risistemare i component
	List<IComponent^>^ newComponents = gcnew List<IComponent^>();
	
	for each (MToolbarItem^ tbItem in Components)
		if (tbItem != component)
			newComponents->Add(tbItem);

	if (index >= newComponents->Count)
		newComponents->Add(component);
	else
		newComponents->Insert(index, component);

	int i=0;
	for each (MToolbarItem^ tbItem in newComponents)
	{
		tbItem->tabOrder = i;
		i++;
	}
		
	components = newComponents;
}

//----------------------------------------------------------------------------
void MToolbar::NextTBPosAddWidth(int addWidth)
{
	MView^ view = (MView^) parent;
	view->NextTBPos = Point(view->NextTBPos.X + addWidth , view->NextTBPos.Y);
}

//----------------------------------------------------------------------------
bool MToolbar::LargeButtons::get () 
{ 
	return isUsingLargeButtons;
} 

//----------------------------------------------------------------------------
void MToolbar::LargeButtons::set (bool value) 
{ 
	isUsingLargeButtons = value;
}

//----------------------------------------------------------------------------
bool MToolbar::Enabled::get ()
{
	return m_pTBToolbar->IsWindowEnabled() == TRUE;
}

//----------------------------------------------------------------------------
void MToolbar::Enabled::set (bool value)
{
	if (m_pInfoOSL && !DesignMode && !OSL_CAN_DO(m_pInfoOSL, OSL_GRANT_EXECUTE))
	{
		value = false;
		//m_pTBToolbar->Text = gcnew System::String(_TB("User hasn't permission to execute this action. Please contact application administrator for obtain it."));
	}

	m_pTBToolbar->EnableWindow(value);
}

//----------------------------------------------------------------------------
System::String^ MToolbar::Text::get()
{ 
	//return m_pTBToolbar->Text;
	return String::Empty;
}

//----------------------------------------------------------------------------
void MToolbar::Text::set (System::String^ value) 
{ 
	//m_pTBToolbar->Text = value;
}

//----------------------------------------------------------------------------
void MToolbar::OnBuildingSecurityTree (IntPtr tree, IntPtr infoTreeItems)
{
	if (!m_pInfoOSL)
		return;

	CTBTreeCtrl* pTree = (CTBTreeCtrl*) tree.ToInt64();
	::Array* pInfoTreeItems = (::Array*) infoTreeItems.ToInt64();
	
	HTREEITEM hRoot = pTree->GetRootItem();
	HTREEITEM hAuxToolbar =  NULL;
	HTREEITEM hItem = pTree->GetChildItem(hRoot);
	while (hItem != NULL)
	{
		COslTreeItem* pItem = (COslTreeItem*) pTree->GetItemData (hItem);

		if (pItem && pItem->m_sNickName == _T("AuxiliaryToolbar"))
		{
			hAuxToolbar = hItem;
			break;
		}
		 hItem = pTree->GetNextSiblingItem(hItem);
	}

	COslTreeItem* pInfo = new COslTreeItem (NULL, m_pInfoOSL,  m_pInfoOSL->m_Namespace.GetObjectName());
	pInfoTreeItems->Add(pInfo);
	// OSLDLG_BMP_TOOLBAR
	BOOL bSameNamespace =	m_pInfoOSL->m_Namespace.GetApplicationName().CompareNoCase(m_pInfoOSL->m_pParent->m_Namespace.GetApplicationName())  == 0 &&
							m_pInfoOSL->m_Namespace.GetModuleName().CompareNoCase(m_pInfoOSL->m_pParent->m_Namespace.GetModuleName())  == 0 &&
							m_pInfoOSL->m_Namespace.GetObjectName(CTBNamespace::DOCUMENT).CompareNoCase(m_pInfoOSL->m_pParent->m_Namespace.GetObjectName(CTBNamespace::DOCUMENT))  == 0;
	
	CString sTitle = bSameNamespace ?  m_pInfoOSL->m_Namespace.GetTypeString() : m_pInfoOSL->m_Namespace.ToUnparsedString();
		
	HTREEITEM hToolbar = pTree->InsertItem (sTitle, 25, 25, hRoot, hAuxToolbar);
	pTree->SetItemData (hToolbar, (DWORD) pInfo); 

	for each (MToolbarItem^ item in Components)
	{
		COslTreeItem* pItemInfo = new COslTreeItem (pInfo, item->m_pInfoOSL,  item->m_pInfoOSL->m_Namespace.GetObjectName());
		pInfoTreeItems->Add(pItemInfo);
		// OSLDLG_BMP_CONTROL
		HTREEITEM hItem = pTree->InsertItem (item->m_pInfoOSL->m_Namespace.GetObjectName(), 22, 22, hToolbar);
		pTree->SetItemData (hItem, (DWORD) pItemInfo); 
	}

}

//----------------------------------------------------------------------------
void MToolbar::CallEvent (HWND hWnd, int commandId, UINT nEvent)
{
	int nID = 0;
	int nSubElem = -1;
	// vuol dire che potrebbe essere un codice composto nel commandID
	// handle della toolbar + sottoelemento
	if (hWnd == NULL && !nEvent)
	{
		CString sCommandID = cwsprintf(_T("%d"), commandId);
		CString sToolbarId = cwsprintf(_T("%d"), Handle);
		if (sCommandID.Left(sToolbarId.GetLength()) == sToolbarId)
		{
			hWnd = GetWnd()->m_hWnd;
			nEvent = BN_CLICKED;
			sCommandID.Replace(sToolbarId, _T(""));
			int nEl = _ttol(sCommandID); 
			nID = nEl / MToolbarItem::ItemEasyLookCode;
			nSubElem = nEl - (nID * MToolbarItem::ItemEasyLookCode);
		}
	}
	else
		nID = commandId;

	if (GetWnd()->m_hWnd != hWnd)
		return;

	for each (MToolbarItem^ item in Components)
	{
		if (item->TabOrder == nID)
		{
			if (nEvent == BN_CLICKED)
			{
				// si tratta di un sottoelemento della tooldropdown
				if (nSubElem >= 0)
					item->OnButtonClick(this, nSubElem);
				else
					item->OnButtonClick(this, nullptr);
			}
			else if (nEvent == TBN_DROPDOWN)
				item->ProcessOnDropDown ();
			break;
		}

	}
}
///////////////////////////////////////////////////////////////////////////////////////////
//								MToolbarItem
///////////////////////////////////////////////////////////////////////////////////////////
//----------------------------------------------------------------------------
MToolbarItem::MToolbarItem (IWindowWrapperContainer^ parentWindow, System::String^ name, System::String^ controlClass, Point location, bool hasCodeBehind)
	:
	BaseWindowWrapper(parentWindow, name, controlClass, location, hasCodeBehind)
{
	Handle = IntPtr::Zero;
}

//----------------------------------------------------------------------------
MToolbarItem::MToolbarItem (IntPtr handle)
	:
	BaseWindowWrapper(IntPtr::Zero)
{
	
}

//----------------------------------------------------------------------------
MToolbarItem::~MToolbarItem  ()
{
	this->!MToolbarItem();
	GC::SuppressFinalize(this);
}

//----------------------------------------------------------------------------
MToolbarItem::!MToolbarItem ()
{
	DeregisterEvents();
	//toolBar->Strip->Items->Remove(item);

	if (m_pInfoOSL)
		delete m_pInfoOSL;
}

//----------------------------------------------------------------------------
void MToolbarItem::DeregisterEvents ()
{
	if (ItemType == ToolStripComboBox::typeid)
		((ToolStripComboBox^)item)->SelectedIndexChanged -= gcnew System::EventHandler(this, &MToolbarItem::OnSelectedIndexChanged);
	else if (ItemType == ToolStripDropDownButton::typeid)
	{
		ToolStripDropDownButton^ tbd = (ToolStripDropDownButton^) item;
		tbd->DropDownItemClicked -= gcnew ToolStripItemClickedEventHandler(this, &MToolbarItem::OnItemSelected);
	}
	else
		item->Click -= gcnew System::EventHandler(this, &MToolbarItem::OnButtonClick);
}

//----------------------------------------------------------------------------
bool MToolbarItem::CanCreate ()
{
	return true;
}

//----------------------------------------------------------------------------
System::String^  MToolbarItem::GetFullPathImageFromNamespace(System::String^ INamespace)
{
	CString strPath = AfxGetPathFinder()->GetFileNameFromNamespace(CString(INamespace), AfxGetLoginInfos()->m_strUserName);
	System::String^ fullPath = gcnew System::String(strPath);
	if (!File::Exists(fullPath))
		return _T("");

	return fullPath;
}

//----------------------------------------------------------------------------
bool MToolbarItem::Create (IWindowWrapperContainer^ parentWindow, System::Drawing::Point location, System::String^ className)
{	
	Type^ itemType = ControlClassToType(className);
	if (itemType != MPushButton::typeid  && itemType != MLabel::typeid && 
		itemType != MParsedCombo::typeid && itemType != MParsedStatic::typeid)
		return false;

	toolBar = (MToolbar^) parentWindow;
	CTBNamespace aNs = CreateNamespaceFromParent(toolBar);
	// gestione grant di security
	m_pInfoOSL = new CInfoOSL(aNs, OSLType_Control);
	m_pInfoOSL->m_pParent = toolBar->m_pInfoOSL;
	AfxGetSecurityInterface()->GetObjectGrant (m_pInfoOSL);
	
	/*ShortcutValue = Shortcut::None;
	tooltip = System::String::Empty;*/

	if (itemType ==  MLabel::typeid)
	{
		item = gcnew ToolStripLabel();
		item->Text = "NewLabel";
	}
	else if(itemType ==  MParsedCombo::typeid)
	{
		item = gcnew ToolStripComboBox();
		item->Text = "NewCombo";
		((ToolStripComboBox^)item)->SelectedIndexChanged += gcnew System::EventHandler(this, &MToolbarItem::OnSelectedIndexChanged);
	}
	else if(itemType ==  MParsedStatic::typeid)
	{
		item = gcnew ToolStripSeparator();
		item->Text = "NewSeparator";
	}
	else
		item = MakeToolStripButton(false);

	/*itemSize = item->Size;
	itemSizeStart = itemSize;*/
	//toolBar->Strip->Items->Add(item);
	return true;
}

//----------------------------------------------------------------------------
ToolStripItem^ MToolbarItem::MakeToolStripButton(bool dropDown)
{
	if (item != nullptr)
	{
		// costringo a distruggere l'oggetto che
		// mi deregistra bene gli eventi
		DeregisterEvents ();
		delete item;
	}

	if (dropDown)
	{
		item = gcnew ToolStripDropDownButton();
		ToolStripDropDownButton^ tbd = (ToolStripDropDownButton^) item;

		tbd->DropDownItemClicked += gcnew ToolStripItemClickedEventHandler(this, &MToolbarItem::OnItemSelected);
		itemDropDown = gcnew ToolStripDropDown();
		if (!AfxIsRemoteInterface())
			itemDropDown->AutoClose = true;	
		tbd->DropDown = itemDropDown;
	}
	else
	{
		item = gcnew ToolStripButton();
		if (itemDropDown != nullptr)
		{
			delete itemDropDown;
			itemDropDown = nullptr;
		}
		item->Click += gcnew System::EventHandler(this, &MToolbarItem::OnButtonClick);
	}

	item->ImageTransparentColor = System::Drawing::Color::FromArgb(255,255,255);
	if (image == nullptr || image == System::String::Empty)
		Image = "Image.Extensions.EasyBuilder.Images.ToolbarButtonDefault" + (toolBar->GetButtonSize().ToString()) + ".png";
	else
		Image = image;

	item->Size = System::Drawing::Size(toolBar->GetButtonSize(), toolBar->GetButtonSize());

	return item;
}

//----------------------------------------------------------------------------
System::Drawing::Size MToolbarItem::GetItemSize()
{
	return itemSize;
}

//----------------------------------------------------------------------------
void MToolbarItem::OnButtonClick (Object^ sender, EventArgs^ e)
{
	Click(this, gcnew SelectedItemEventArgs(nullptr));
}

//----------------------------------------------------------------------------
void MToolbarItem::OnButtonClick (Object^ sender, int commandId)
{
	if (DropDown && commandId >= 0 && commandId < itemDropDown->Items->Count)
		Click(this, gcnew SelectedItemEventArgs(itemDropDown->Items[commandId]));
}

//----------------------------------------------------------------------------
INameSpace^ MToolbarItem::Namespace::get ()	
{ 
	return m_pInfoOSL ? gcnew NameSpace(gcnew System::String(m_pInfoOSL->m_Namespace.ToString())) : NameSpace::Empty;
} 

//-----------------------------------------------------------------------------
int	MToolbarItem::GetNamespaceType ()
{
	return CTBNamespace::TOOLBARBUTTON;
}

//----------------------------------------------------------------------------
System::String^ MToolbarItem::Name::get ()	
{ 
	return Namespace->Leaf;
} 

//----------------------------------------------------------------------------
void MToolbarItem::Name::set (System::String^ name) 
{ 
	if (m_pInfoOSL)
		m_pInfoOSL->m_Namespace.SetObjectName(name, TRUE);
} 

//----------------------------------------------------------------------------
System::String^	MToolbarItem::SerializedName::get () 
{ 
	return System::String::Concat("tbi_", EasyBuilderSerializer::Escape(Name)); 
}

//----------------------------------------------------------------------------
CTBNamespace MToolbarItem::CreateNamespaceFromParent (MToolbar^ toolbar)
{
	if (System::String::IsNullOrEmpty(name))
		CreateUniqueName(toolbar);

	CTBNamespace aNs(CString(toolbar->Namespace->FullNameSpace));
	aNs.SetObjectName(CTBNamespace::TOOLBARBUTTON, CString(name), TRUE, TRUE);
	return aNs;
}

//----------------------------------------------------------------------------
bool MToolbarItem::Enabled::get ()
{
	return item->Enabled;
}

//----------------------------------------------------------------------------
void MToolbarItem::Enabled::set (bool value)
{
	MDocument^ doc = (MDocument^) Document;

	if	(
			m_pInfoOSL && doc != nullptr && !DesignMode &&
			(
				!OSL_CAN_DO(m_pInfoOSL, OSL_GRANT_EXECUTE) ||
				(doc->FormMode == FormModeType::New && !OSL_CAN_DO(m_pInfoOSL, OSL_GRANT_NEW)) ||
				(doc->FormMode == FormModeType::Edit && !OSL_CAN_DO(m_pInfoOSL, OSL_GRANT_EDIT))
			)
		)
	{
		Tooltip = gcnew System::String(_TB("User hasn't permission to execute this action. Please contact application administrator for obtain it."));
		value = false;
	}
	
	if (item)
		item->Enabled = value;
}

//----------------------------------------------------------------------------
System::String^	MToolbarItem::ShortcutTooltip()
{
	if (ShortcutValue == Shortcut::None)
		return System::String::Empty;

	System::String^ sShortcut = "(" + ShortcutValue.ToString() + ")";
	int iCtrl = sShortcut->IndexOf("Ctrl");
	if (iCtrl >= 0)
		sShortcut = sShortcut->Insert( iCtrl + 4, " + ");
		
	int iShift = sShortcut->IndexOf("Shift");
	if (iShift >= 0)
		sShortcut = sShortcut->Insert( iShift + 5, " + ");

	int iAlt = sShortcut->IndexOf("Alt");
	if (iAlt >= 0) 
		sShortcut = sShortcut->Insert( iAlt + 3, " + ");

	return sShortcut;
}

//----------------------------------------------------------------------------
System::String^ MToolbarItem::Tooltip::get()
{
	return tooltip;
}

//----------------------------------------------------------------------------
void MToolbarItem::Tooltip::set(System::String^ value)
{
	tooltip = value;

	if (!DesignMode && ShortcutValue != Shortcut::None)
		item->ToolTipText = tooltip + ShortcutTooltip();
	else
		item->ToolTipText = tooltip;
}

//----------------------------------------------------------------------------
void MToolbarItem::onKeyDown(System::String^ sShort)
{
	try
	{
		Shortcut sc = (Shortcut) Enum::Parse(Shortcut::typeid, sShort);
		if (sc == ShortcutValue)
			OnButtonClick(this, EasyBuilderEventArgs::Empty);
	}
	catch(Exception^ e)
	{
		// key combination not supported!
		Exception^ b = e;
	}
	
}

//----------------------------------------------------------------------------
Shortcut MToolbarItem::ShortcutKey::get()
{
	return ShortcutValue;
}

//----------------------------------------------------------------------------
void MToolbarItem::ShortcutKey::set(Shortcut value)
{
	ShortcutValue = value;

	if (!DesignMode)
		item->ToolTipText = tooltip + ShortcutTooltip();
}

//----------------------------------------------------------------------------
System::String^	MToolbarItem::TypeToControlClass (Type^ type)
{
	if (type == MPushButton::typeid)
		return "Button";

	if (type == MLabel::typeid)
		return "Label";

	if (type == MParsedCombo::typeid)
		return "Combo";

	if (type == MParsedStatic::typeid)
		return "Separator";

	return System::String::Empty;
}

//----------------------------------------------------------------------------
System::String^ MToolbarItem::Image::get ()
{ 
	return image;
}

//----------------------------------------------------------------------------
void MToolbarItem::Image::set (System::String^ Image) 
{ 
	image =  System::String::Copy(Image);
	System::String^ fullPath = GetFullPathImageFromNamespace(image);
	if (!File::Exists(fullPath))
		return;

	System::IO::Stream^ stream = gcnew FileStream(fullPath, FileMode::Open, FileAccess::Read , FileShare::Read);
	if (stream != nullptr)
	{
		item->Image = (Bitmap^) Bitmap::FromStream(stream);
		stream->Close();
	}
}

//----------------------------------------------------------------------------
Type^ MToolbarItem::ControlClassToType (System::String^ className)
{
	if (System::String::Compare(className, "Button") == 0)
		return MPushButton::typeid;
	
	if (System::String::Compare(className, "Label") == 0)
		return MLabel::typeid;
	
	if (System::String::Compare(className, "Combo") == 0)
		return MParsedCombo::typeid;

	if (System::String::Compare(className, "Separator") == 0)
		return MParsedStatic::typeid;

	return nullptr;
}

//----------------------------------------------------------------------------
System::String^ MToolbarItem::Text::get () 
{ 
	return item->Text;
} 

//----------------------------------------------------------------------------
void MToolbarItem::Text::set (System::String^ value)
{ 
	item->Text = value;
	
	itemSize = System::Drawing::Size((item->Text->Length * 6), itemSize.Height) ;
	if (itemSizeStart != itemSize)
	{
		toolBar->NextTBPosAddWidth(itemSize.Width);
		itemSizeStart = itemSize;
	}
} 

//----------------------------------------------------------------------------
ToolStripItemAlignment MToolbarItem::Alignment::get () 
{ 
	return item->Alignment;
} 

//----------------------------------------------------------------------------
void MToolbarItem::Alignment::set (ToolStripItemAlignment value)
{ 
	item->Alignment = value;
} 

//----------------------------------------------------------------------------
System::String^ MToolbarItem::ClassName::get () 
{ 
	if (item->GetType() == ToolStripButton::typeid) 
		return TypeToControlClass(MPushButton::typeid);

	if (item->GetType() == ToolStripDropDownButton::typeid) 
		return TypeToControlClass(MPushButton::typeid);
	
	if (item->GetType() == ToolStripLabel::typeid)
		return TypeToControlClass(MLabel::typeid);

 	if (item->GetType() == ToolStripComboBox::typeid)
 		return TypeToControlClass(MParsedCombo::typeid);
	
	if (item->GetType() == ToolStripSeparator::typeid)
 		return TypeToControlClass(MParsedStatic::typeid);

	return System::String::Empty;
} 

//----------------------------------------------------------------------------
System::Drawing::Color MToolbarItem::BackColor::get()
{ 
	return item->BackColor;
}

//----------------------------------------------------------------------------
void MToolbarItem::BackColor::set (System::Drawing::Color value) 
{ 
	item->BackColor = value;
}

//----------------------------------------------------------------------------
System::Drawing::Color MToolbarItem::ForeColor::get()
{ 
	return item->ForeColor;
}

//----------------------------------------------------------------------------
void MToolbarItem::ForeColor::set (System::Drawing::Color value) 
{ 
	item->ForeColor = value;
}

//----------------------------------------------------------------------------
ToolStripItemDisplayStyle MToolbarItem::DisplayStyle::get()
{ 
	return item->DisplayStyle;
}

//----------------------------------------------------------------------------
void MToolbarItem::DisplayStyle::set (ToolStripItemDisplayStyle value) 
{ 
	item->DisplayStyle = value;
}

//----------------------------------------------------------------------------
bool MToolbarItem::Visible::get()
{ 
	return item->Visible;
}

//----------------------------------------------------------------------------
void MToolbarItem::Visible::set (bool value) 
{ 
	item->Visible = value;
}

//----------------------------------------------------------------------------
bool MToolbarItem::DropDown::get()
{ 
	return itemDropDown != nullptr;
}

//----------------------------------------------------------------------------
void MToolbarItem::DropDown::set (bool value) 
{ 
	if	(
			DropDown == value || 
			!item ||
			( ! ((item->GetType() == ToolStripButton::typeid) || (item->GetType() == ToolStripDropDownButton::typeid)) )
		)
		return;
		
	int iPos = -1;
	bool notFound = false;
	/* continue from iterators last position 
	for each (ToolStripItem^ itemCandidate in toolBar->Strip->Items)
	{
		iPos++;
		if (itemCandidate == item)
		{
			toolBar->Strip->Items->Remove(item);
			notFound = true;
			break;
		}
	}*/

	// item not found!
	if (!notFound)
	{
		ASSERT(FALSE);
		return;
	}

	// make new item
	item = MakeToolStripButton(value);
	//toolBar->Strip->Items->Insert(iPos, item);
}

//----------------------------------------------------------------------------
bool MToolbarItem::CanChangeProperty (System::String^ propertyName)
{
	if (propertyName == "Checked")
		return item->GetType() == ToolStripButton::typeid;

	if (propertyName == "ToolStripComboBoxItem")
		return item->GetType() == ToolStripComboBox::typeid;

	if (propertyName == "DropDown")
		return ((item->GetType() == ToolStripButton::typeid) || 
		        (item->GetType() == ToolStripDropDownButton::typeid));

	return __super::CanChangeProperty(propertyName);
}

//----------------------------------------------------------------------------
bool MToolbarItem::Checked::get()
{ 
	return ItemType == ToolStripButton::typeid ? ((ToolStripButton^) item)->Checked : false;
}

//----------------------------------------------------------------------------
void MToolbarItem::Checked::set (bool value) 
{ 
	if (ItemType == ToolStripButton::typeid)
		((ToolStripButton^) item)->Checked = value;
}

//----------------------------------------------------------------------------
void MToolbarItem::AddItem(Object^ itemToAdd)
{
	if (this->item->GetType() == ToolStripDropDownButton::typeid)
	{
		if (itemToAdd->GetType() == System::String::typeid)
			itemDropDown->Items->Add(gcnew ToolStripButton((System::String^) itemToAdd));
		else if (itemToAdd->GetType()->IsSubclassOf(ToolStripItem::typeid))
			itemDropDown->Items->Add((ToolStripItem^) itemToAdd);
	}

	if (this->item->GetType() == ToolStripComboBox::typeid)
		((ToolStripComboBox^) this->item)->Items->Add(itemToAdd);
}

//----------------------------------------------------------------------------
System::String^ MToolbarItem::CurrentItem::get()
{
	if ((item->GetType() == ToolStripComboBox::typeid) && (((ToolStripComboBox^) item)->SelectedItem))
		return ((ToolStripComboBox^) item)->SelectedItem->ToString();
	
	return System::String::Empty;
}

//----------------------------------------------------------------------------
System::Drawing::Rectangle MToolbarItem::ClientRectangle::get ()
{
	if (item == nullptr) 
		return System::Drawing::Rectangle::Empty;

	return item->Owner->RectangleToScreen(item->Bounds);
}

//----------------------------------------------------------------------------
void MToolbarItem::ShortcutToAccel(ACCEL* pAccel)
{
	System::String^ key = ShortcutKey.ToString();
	
	pAccel->fVirt = 0;
	
	if (key->Contains("Ctrl"))
	{
		pAccel->fVirt |= FCONTROL;
		key = key->Replace ("Ctrl", "");
	}
	if (key->Contains("Alt"))
	{
		pAccel->fVirt |= FALT;
		key = key->Replace ("Alt", "");
	}
	if (key->Contains("Shift"))
	{
		pAccel->fVirt |= FSHIFT;
		key = key->Replace ("Shift", "");
	}
		
	KeysConverter^ converter = gcnew KeysConverter();
	Keys baseKey = (Keys) converter->ConvertFromString(key);
	pAccel->key = (int) baseKey;
	pAccel->cmd = WM_COMMAND;
}

//----------------------------------------------------------------------------
void MToolbarItem::TabOrder::set(int value) 
{ 
	if (toolBar != nullptr && tabOrder != value)
		toolBar->MoveTo(Name, item, value);

	tabOrder = value;
}

//----------------------------------------------------------------------------
void MToolbarItem::OnSelectedIndexChanged (Object^ sender, EventArgs^ e)
{
	Click(this, gcnew SelectedItemEventArgs(((ToolStripComboBox^) sender)->SelectedItem));
}

//----------------------------------------------------------------------------
void MToolbarItem::OnItemSelected (Object^ sender, ToolStripItemClickedEventArgs^ e)
{
	Click(this, gcnew SelectedItemEventArgs(e->ClickedItem));
}

// i sotto elementi di un menu li identifico andando ad aumentare di il taborder
//----------------------------------------------------------------------------
void MToolbarItem::ProcessOnDropDown ()
{
	if (!DropDown)
		return;

	((ToolStripDropDownButton^) item)->ShowDropDown();
}
