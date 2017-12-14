#include "stdafx.h"

#include <TbGeneric\JsonFormEngine.h>
#include <TbGenlib\basetiledialog.h>
#include "MPanel.h"
#include "MTilePanel.h"
#include "MTileGroup.h"
#include "MAccelerator.h"
#include "MBodyEdit.h"


/////////////////////////////////////////////////////////////////////////////
// 				class MPanel Implementation
/////////////////////////////////////////////////////////////////////////////

//----------------------------------------------------------------------------
MPanel::MPanel(IntPtr wrappedObject)
	:
	WindowWrapperContainer(wrappedObject)
{
}
//----------------------------------------------------------------------------
MPanel::MPanel(IWindowWrapperContainer^ parentWindow, System::String^ name, System::String^ className, System::Drawing::Point location, bool hasCodeBehind)
	:
	WindowWrapperContainer(parentWindow, name, className, location, hasCodeBehind)
{
}

//----------------------------------------------------------------------------
void MPanel::Initialize()
{
	minSize = CUtility::Get100x100Size();

}

//----------------------------------------------------------------------------
MPanel::~MPanel()
{
	this->!MPanel();
	System::GC::SuppressFinalize(this);
}

//----------------------------------------------------------------------------
MPanel::!MPanel()
{
	if (!HasCodeBehind)
	{
		CWnd* pWnd = GetWnd();
		if (pWnd)
		{
			pWnd->DestroyWindow();
		}
	}
}
//----------------------------------------------------------------------------
void MPanel::InitializeName(IWindowWrapperContainer^ parent)
{
	if (!parent)
		return;

	CTBNamespace aNamespace = CreateNamespaceFromParent(parent);
	nameSpace = gcnew NameSpace(gcnew System::String(aNamespace.ToString()));
}
//----------------------------------------------------------------------------
void MPanel::Parent::set(IWindowWrapperContainer^ value)
{
	__super::Parent = value;

	InitializeName(value);
}

#pragma region styles properties
//----------------------------------------------------------------------------
bool MPanel::Border::get()
{
	CWndObjDescription* pDescri = GetWndObjDescription();
	return pDescri ? pDescri->m_bBorder : (HasExStyle(WS_BORDER));
}

//----------------------------------------------------------------------------
void MPanel::Border::set(bool value)
{
	CWndObjDescription* pDescri = GetWndObjDescription();
	if (pDescri)
		pDescri->m_bBorder = value;

	SetExStyle(SET_STYLE_PARAMS(WS_BORDER));
}

//----------------------------------------------------------------------------
bool MPanel::ModalFrame::get()
{
	CWndObjDescription* pDescri = GetWndObjDescription();
	return pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CWndPanelDescription)) ?
		((CWndPanelDescription*)pDescri)->m_bModalFrame
		: HasStyle(DS_MODALFRAME);
}

//----------------------------------------------------------------------------
void MPanel::ModalFrame::set(bool value)
{
	CWndObjDescription* pDescri = GetWndObjDescription();
	if (pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CWndPanelDescription)))
		((CWndPanelDescription*)pDescri)->m_bModalFrame = value;

	SetStyle(SET_STYLE_PARAMS(DS_MODALFRAME));

}

//----------------------------------------------------------------------------
bool MPanel::Child::get()
{
	CWndObjDescription* pDescri = GetWndObjDescription();
	return pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CWndPanelDescription)) ?
		((CWndPanelDescription*)pDescri)->m_bChild
		: (HasStyle(WS_CHILD) && !HasStyle(WS_POPUP));
}

//----------------------------------------------------------------------------
void MPanel::Child::set(bool value)
{
	CWndObjDescription* pDescri = GetWndObjDescription();
	if (pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CWndPanelDescription)) && ((CWndPanelDescription*)pDescri)->m_bChild != value)
	{
		((CWndPanelDescription*)pDescri)->m_bChild = value;
		((CWndPanelDescription*)pDescri)->SetUpdated(&(((CWndPanelDescription*)pDescri)->m_bChild));
	}

	if (value)
		SetStyle(WS_CHILD, WS_POPUP);
	else
		SetStyle(WS_POPUP, WS_CHILD);

}
//----------------------------------------------------------------------------
bool MPanel::Center::get()
{
	CWndObjDescription* pDescri = GetWndObjDescription();
	return (pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CWndPanelDescription))) ?
		((CWndPanelDescription*)pDescri)->m_bCenter
		: (HasStyle(DS_CENTER));
}

//----------------------------------------------------------------------------
void MPanel::Center::set(bool value)
{
	CWndObjDescription* pDescri = GetWndObjDescription();
	if (pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CWndPanelDescription)))
		((CWndPanelDescription*)pDescri)->m_bCenter = value;	

	SetStyle(SET_STYLE_PARAMS(DS_CENTER));
}

//----------------------------------------------------------------------------
bool MPanel::CenterMouse::get()
{
	CWndObjDescription* pDescri = GetWndObjDescription();
	return pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CWndPanelDescription)) ?
		((CWndPanelDescription*)pDescri)->m_bCenterMouse
		: HasStyle(DS_CENTERMOUSE);
}

//----------------------------------------------------------------------------
void MPanel::CenterMouse::set(bool value)
{
	CWndObjDescription* pDescri = GetWndObjDescription();
	if (pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CWndPanelDescription)))
		((CWndPanelDescription*)pDescri)->m_bCenterMouse = value;

	SetStyle(SET_STYLE_PARAMS(DS_CENTERMOUSE));
}

//----------------------------------------------------------------------------
bool MPanel::UserControl::get()
{
	CWndObjDescription* pDescri = GetWndObjDescription();
	return pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CWndPanelDescription)) ?
		((CWndPanelDescription*)pDescri)->m_bUserControl
		: HasStyle(DS_CONTROL);
}

//----------------------------------------------------------------------------
void MPanel::UserControl::set(bool value)
{
	CWndObjDescription* pDescri = GetWndObjDescription();
	if (pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CWndPanelDescription)))
		((CWndPanelDescription*)pDescri)->m_bUserControl = value;
	
	SetStyle(SET_STYLE_PARAMS(DS_CONTROL));
}

//----------------------------------------------------------------------------
bool MPanel::Caption::get()
{
	CWndObjDescription* pDescri = GetWndObjDescription();
	return pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CWndPanelDescription)) ?
		((CWndPanelDescription*)pDescri)->m_bCaption
		: HasStyle(WS_CAPTION);
}

//----------------------------------------------------------------------------
void MPanel::Caption::set(bool value)
{
	CWndObjDescription* pDescri = GetWndObjDescription();
	if (pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CWndPanelDescription)))
		((CWndPanelDescription*)pDescri)->m_bCaption = value;

	SetStyle(SET_STYLE_PARAMS(WS_CAPTION));
}

//----------------------------------------------------------------------------
bool MPanel::SystemMenu::get()
{
	CWndObjDescription* pDescri = GetWndObjDescription();
	return pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CWndPanelDescription)) ?
		((CWndPanelDescription*)pDescri)->m_bSystemMenu
		: HasStyle(WS_SYSMENU);
}

//----------------------------------------------------------------------------
void MPanel::SystemMenu::set(bool value)
{
	CWndObjDescription* pDescri = GetWndObjDescription();
	if (pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CWndPanelDescription)))
		((CWndPanelDescription*)pDescri)->m_bSystemMenu = value;

	SetStyle(SET_STYLE_PARAMS(WS_SYSMENU));
}

//----------------------------------------------------------------------------
bool MPanel::ClipChildren::get()
{
	CWndObjDescription* pDescri = GetWndObjDescription();
	return pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CWndPanelDescription)) ?
		((CWndPanelDescription*)pDescri)->m_bClipChildren
		: HasStyle(WS_CLIPCHILDREN);
}

//----------------------------------------------------------------------------
void MPanel::ClipChildren::set(bool value)
{
	CWndObjDescription* pDescri = GetWndObjDescription();
	if (pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CWndPanelDescription)))
		((CWndPanelDescription*)pDescri)->m_bClipChildren = value;

	SetStyle(SET_STYLE_PARAMS(WS_CLIPCHILDREN));
}

//----------------------------------------------------------------------------
bool MPanel::ClipSiblings::get()
{
	CWndObjDescription* pDescri = GetWndObjDescription();
	return pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CWndPanelDescription)) ?
		((CWndPanelDescription*)pDescri)->m_bClipSiblings
		: HasStyle(WS_CLIPSIBLINGS);
}

//----------------------------------------------------------------------------
void MPanel::ClipSiblings::set(bool value)
{	
	CWndObjDescription* pDescri = GetWndObjDescription();
	if (pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CWndPanelDescription)))
		((CWndPanelDescription*)pDescri)->m_bClipSiblings = value;
	
	SetStyle(SET_STYLE_PARAMS(WS_CLIPSIBLINGS));
}

//----------------------------------------------------------------------------
bool MPanel::DialogFrame::get()
{
	CWndObjDescription* pDescri = GetWndObjDescription();
	return pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CWndPanelDescription)) ?
		((CWndPanelDescription*)pDescri)->m_bDialogFrame
		: HasStyle(WS_DLGFRAME);
}

//----------------------------------------------------------------------------
void MPanel::DialogFrame::set(bool value)
{
	CWndObjDescription* pDescri = GetWndObjDescription();
	if (pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CWndPanelDescription)))
		((CWndPanelDescription*)pDescri)->m_bDialogFrame = value;
	
	SetStyle(SET_STYLE_PARAMS(WS_DLGFRAME));
}

//----------------------------------------------------------------------------
bool MPanel::MaximizeBox::get()
{
	CWndObjDescription* pDescri = GetWndObjDescription();
	return pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CWndPanelDescription)) ?
		((CWndPanelDescription*)pDescri)->m_bMaximizeBox
		: HasStyle(WS_MAXIMIZEBOX);
}

//----------------------------------------------------------------------------
void MPanel::MaximizeBox::set(bool value)
{
	CWndObjDescription* pDescri = GetWndObjDescription();
	if (pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CWndPanelDescription)))
		((CWndPanelDescription*)pDescri)->m_bMaximizeBox = value;

	SetStyle(SET_STYLE_PARAMS(WS_MAXIMIZEBOX));
}

//----------------------------------------------------------------------------
bool MPanel::MinimizeBox::get()
{
	CWndObjDescription* pDescri = GetWndObjDescription();
	return pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CWndPanelDescription)) ?
		((CWndPanelDescription*)pDescri)->m_bMinimizeBox
		: HasStyle(WS_MINIMIZEBOX);
}

//----------------------------------------------------------------------------
void MPanel::MinimizeBox::set(bool value)
{
	CWndObjDescription* pDescri = GetWndObjDescription();
	if (pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CWndPanelDescription)))
		((CWndPanelDescription*)pDescri)->m_bMinimizeBox = value;

	SetStyle(SET_STYLE_PARAMS(WS_MINIMIZEBOX));
}

//----------------------------------------------------------------------------
bool MPanel::Overlapped::get()
{
	CWndObjDescription* pDescri = GetWndObjDescription();
	return pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CWndPanelDescription)) ?
		((CWndPanelDescription*)pDescri)->m_bOverlapped
		: HasStyle(WS_OVERLAPPED);
}

//----------------------------------------------------------------------------
void MPanel::Overlapped::set(bool value)
{
	CWndObjDescription* pDescri = GetWndObjDescription();
	if (pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CWndPanelDescription)))
		((CWndPanelDescription*)pDescri)->m_bOverlapped = value;

	SetStyle(SET_STYLE_PARAMS(WS_OVERLAPPED));
}

//----------------------------------------------------------------------------
bool MPanel::ThickFrame::get()
{
	CWndObjDescription* pDescri = GetWndObjDescription();
	return pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CWndPanelDescription)) ?
		((CWndPanelDescription*)pDescri)->m_bThickFrame
		: HasStyle(WS_THICKFRAME);
}

//----------------------------------------------------------------------------
void MPanel::ThickFrame::set(bool value)
{
	CWndObjDescription* pDescri = GetWndObjDescription();
	if (pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CWndPanelDescription)))
		((CWndPanelDescription*)pDescri)->m_bThickFrame = value;

	SetStyle(SET_STYLE_PARAMS(WS_THICKFRAME));
}

#pragma endregion

//----------------------------------------------------------------------------
bool MPanel::Create(IWindowWrapperContainer^ parentWindow, System::Drawing::Point location, System::String^ className)
{
	if (parentWindow == nullptr)
		return false;

	CWnd* pParentWnd = ((BaseWindowWrapper^)parentWindow)->GetWnd();
	CPoint aPt(location.X, location.Y);

	DWORD nStyle = WS_CHILD | WS_VISIBLE | DS_SETFONT;
	DWORD nExStyle = WS_EX_CONTROLPARENT;
	CSize aSize(minSize.Width, minSize.Height);
	SafeMapDialog(pParentWnd->m_hWnd, aSize);//trasformo in pixel
	CRect r(aPt, aSize);
	//trasformo le coordinate fisiche in coordinate logiche, per creare il template
	VERIFY(ReverseMapDialog(pParentWnd->m_hWnd, r));
	//la devo creare così, altrimenti non funziona la MapDialogRect!
	AutoDeletePtr<DLGTEMPLATE> pTemplate = (DLGTEMPLATE*)CJsonContextObj::CreateTemplate(r, L"", nStyle, nExStyle);

	CTBNamespace aNamespace = CString(CreateNamespaceFromParent(parentWindow));
	HWND hWnd = CreateDialogIndirect(NULL, pTemplate, pParentWnd->m_hWnd, NULL);
	if (!hWnd)
		return false;

	Handle = (System::IntPtr)hWnd;
	Id = gcnew String(aNamespace.GetObjectName());
	nameSpace = gcnew NameSpace(gcnew System::String(aNamespace.ToString()));

	return true;
}

/////////////////////////////////////////////////////////////////////////////
// 				class MEasyStudioPanel Implementation
/////////////////////////////////////////////////////////////////////////////

/// <summary>
/// Constructor: 
/// </summary>
MEasyStudioPanel::MEasyStudioPanel(System::IntPtr wrappedObject)
	:
	MPanel(wrappedObject)
{
	if (Size.IsEmpty)
		Size = minSize;
}
//----------------------------------------------------------------------------
void MEasyStudioPanel::Initialize()
{
	minSize = CUtility::Get100x100Size();

}
//----------------------------------------------------------------------------
System::String^	MEasyStudioPanel::Id::get()
{
	CWnd* pWnd = GetWnd();
	return pWnd
		? gcnew System::String(AfxGetTBResourcesMap()->DecodeID(TbResources, pWnd->GetDlgCtrlID()).m_strName)
		: "";
}
//----------------------------------------------------------------------------
void MEasyStudioPanel::Id::set(System::String^ value)
{
	CWnd* pWnd = GetWnd();
	if (pWnd)
		pWnd->SetDlgCtrlID(AfxGetTBResourcesMap()->GetTbResourceID(CString(value), TbResources));
	id = value;
}
//-----------------------------------------------------------------------------
EBool MEasyStudioPanel::Collapsible::get()
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	return pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndTileDescription))
		? (EBool)((CWndTileDescription*)pDesc)->m_bIsCollapsible
		: EBool::Undefined;
}

//-----------------------------------------------------------------------------
void MEasyStudioPanel::Collapsible::set(EBool value)
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	if (pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndTileDescription)))
		((CWndTileDescription*)pDesc)->m_bIsCollapsible = (Bool3)value;
}

//-----------------------------------------------------------------------------
EditingMode MEasyStudioPanel::DesignerMovable::get()
{
	return EditingMode::ResizingMidBottom | EditingMode::ResizingMidRight | EditingMode::ResizingBottomRight;
}

//-----------------------------------------------------------------------------
bool MEasyStudioPanel::Collapsed::get()
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	return pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndTileDescription)) ? ((CWndTileDescription*)pDesc)->m_bIsCollapsed : false;
}

//-----------------------------------------------------------------------------
void MEasyStudioPanel::Collapsed::set(bool value)
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	if (pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndTileDescription)))
		((CWndTileDescription*)pDesc)->m_bIsCollapsed = value;
}

//-----------------------------------------------------------------------------
EBool MEasyStudioPanel::HasTitle::get()
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	return pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndTileDescription))
		? (EBool)((CWndTileDescription*)pDesc)->m_bHasTitle
		: EBool::Undefined;
}

//-----------------------------------------------------------------------------
void MEasyStudioPanel::HasTitle::set(EBool value)
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	if (pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndTileDescription)))
		((CWndTileDescription*)pDesc)->m_bHasTitle = (Bool3)value;
}

//-----------------------------------------------------------------------------
EBool MEasyStudioPanel::Pinnable::get()
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	return pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndTileDescription))
		? (EBool)((CWndTileDescription*)pDesc)->m_bIsPinnable
		: EBool::Undefined;
}

//-----------------------------------------------------------------------------
void MEasyStudioPanel::Pinnable::set(EBool value)
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	if (pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndTileDescription)))
		((CWndTileDescription*)pDesc)->m_bIsPinnable = (Bool3)value;
}
//-----------------------------------------------------------------------------
bool MEasyStudioPanel::Pinned::get()
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	return pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndTileDescription)) ? ((CWndTileDescription*)pDesc)->m_bIsPinned : true;
}

//-----------------------------------------------------------------------------
void MEasyStudioPanel::Pinned::set(bool value)
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	if (pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndTileDescription)))
		((CWndTileDescription*)pDesc)->m_bIsPinned = value;
}

//-----------------------------------------------------------------------------
bool MEasyStudioPanel::WrapTileParts::get()
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	return pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndTileDescription)) ? ((CWndTileDescription*)pDesc)->m_bWrapTileParts : true;
}

//-----------------------------------------------------------------------------
void MEasyStudioPanel::WrapTileParts::set(bool value)
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	if (pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndTileDescription)))
		((CWndTileDescription*)pDesc)->m_bWrapTileParts = value;
}
//-----------------------------------------------------------------------------
bool MEasyStudioPanel::HasStaticArea::get()
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	return pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndTileDescription)) ? ((CWndTileDescription*)pDesc)->m_bHasStaticArea : true;
}

//-----------------------------------------------------------------------------
void MEasyStudioPanel::HasStaticArea::set(bool value)
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	if (pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndTileDescription)))
		((CWndTileDescription*)pDesc)->m_bHasStaticArea = value;
}
//-----------------------------------------------------------------------------
bool MEasyStudioPanel::Child::get()
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	return pDesc ? pDesc->m_bChild : false;
}

//-----------------------------------------------------------------------------
void MEasyStudioPanel::Child::set(bool value)
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	if (pDesc)
		pDesc->m_bChild = value;
}

//----------------------------------------------------------------------------
System::String^	MEasyStudioPanel::Name::get()
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	if (pDesc)
		return gcnew String(pDesc->m_strName);
	return __super::Name;
}
//----------------------------------------------------------------------------
void MEasyStudioPanel::Name::set(System::String^ value)
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	if (pDesc)
		pDesc->m_strName = value;
	__super::Name = value;
}
//----------------------------------------------------------------------------
EPanelType MEasyStudioPanel::PanelType::get()
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	return pDesc ? (EPanelType)pDesc->m_Type : EPanelType::Panel;
}
//----------------------------------------------------------------------------
void MEasyStudioPanel::PanelType::set(EPanelType value)
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	if (pDesc)
		pDesc->m_Type = (CWndObjDescription::WndObjType)value;
}

//----------------------------------------------------------------------------
ETileDialogStyle MEasyStudioPanel::TileDialogStyle::get()
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	if (pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndTileDescription)))
		return (ETileDialogStyle)((CWndTileDescription*)pDesc)->m_Style;
	return ETileDialogStyle::None;
}
//----------------------------------------------------------------------------
void MEasyStudioPanel::TileDialogStyle::set(ETileDialogStyle value)
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	if (pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndTileDescription)))
		((CWndTileDescription*)pDesc)->m_Style = (::TileDialogStyle)value;
}


//----------------------------------------------------------------------------
ETileDialogSize MEasyStudioPanel::TileDialogSize::get()
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	if (pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndTileDescription)))
		return (ETileDialogSize)((CWndTileDescription*)pDesc)->m_Size;
	return ETileDialogSize::Standard;
}
//----------------------------------------------------------------------------
void MEasyStudioPanel::TileDialogSize::set(ETileDialogSize value)
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	if (pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndTileDescription)))
	{
		System::Drawing::Size sz = this->SizeLU;
		((CWndTileDescription*)pDesc)->m_Size = (::TileDialogSize)value;
		SizeLU = CUtility::GetIdealTileSizeLU(value);
	}
}

//----------------------------------------------------------------------------
int MEasyStudioPanel::Flex::get()
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	if (pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndTileDescription)))
		return ((CWndTileDescription*)pDesc)->m_nFlex;
	return -1;
}
//----------------------------------------------------------------------------
void MEasyStudioPanel::Flex::set(int value)
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	if (pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndTileDescription)))
		((CWndTileDescription*)pDesc)->m_nFlex = value;
}

//----------------------------------------------------------------------------
int MEasyStudioPanel::Col2Margin::get()
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	if (pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndTileDescription)))
		return ((CWndTileDescription*)pDesc)->m_nCol2Margin;
	return -1;
}
//----------------------------------------------------------------------------
void MEasyStudioPanel::Col2Margin::set(int value)
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	if (pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndTileDescription)))
		((CWndTileDescription*)pDesc)->m_nCol2Margin = value;
}
//----------------------------------------------------------------------------
Point MEasyStudioPanel::LocationLU::get()
{
	return __super::LocationLU;
}
//----------------------------------------------------------------------------
void MEasyStudioPanel::LocationLU::set(Point pt)
{
	__super::LocationLU = pt;
}

//-----------------------------------------------------------------------------
cli::array<MAccelerator^>^ MEasyStudioPanel::Accelerators::get()
{
	List<MAccelerator^>^ list = gcnew List<MAccelerator^>();
	CWndObjDescription* pDesc = GetWndObjDescription();
	if (pDesc && pDesc->m_pAccelerator)
	{
		for (int i = 0; i < pDesc->m_pAccelerator->m_arItems.GetCount(); i++)
		{
			MAccelerator^ acc = gcnew MAccelerator(pDesc->m_pAccelerator->m_arItems[i]);
			list->Add(acc);
		}
	}
	return list->ToArray();
}

//-----------------------------------------------------------------------------
void MEasyStudioPanel::Accelerators::set(cli::array<MAccelerator^>^ value)
{
	CWndObjDescription* pDesc = GetWndObjDescription();
	if (pDesc->m_pAccelerator)
		pDesc->m_pAccelerator->Clear();
	else
		pDesc->m_pAccelerator = new CAcceleratorDescription();
	for each (MAccelerator^ acc in value)
	{
		pDesc->m_pAccelerator->m_arItems.Add(acc->GetItem());
	}
}

//----------------------------------------------------------------------------
bool MEasyStudioPanel::CanChangeProperty(System::String^ propertyName)
{
	if (propertyName == "Id")
		return false;
	CWndObjDescription* pDesc = GetWndObjDescription();
	if (pDesc && pDesc->m_Type != (CWndObjDescription::Tile))
	{
		if (propertyName == "Collapsible" ||
			propertyName == "Collapsed" ||
			propertyName == "TileDialogStyle" ||
			propertyName == "TileDialogSize" ||
			propertyName == "Pinned" ||
			propertyName == "Flex" ||
			propertyName == "Pinnable" ||
			propertyName == "HasTitle" ||
			propertyName == "WrapTileParts"||
			propertyName == "HasStaticArea")
			return false;
	}

	return __super::CanChangeProperty(propertyName);
}

//----------------------------------------------------------------------------
bool MEasyStudioPanel::CanDropTarget(Type^ droppedObject)
{
	return !(
		MTab::typeid == droppedObject || droppedObject->IsSubclassOf(MTab::typeid) ||
		MTileDialog::typeid == droppedObject || droppedObject->IsSubclassOf(MTileDialog::typeid));
}