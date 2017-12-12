// cannot use C++ managed namespaces in order to avoid stdafx.h conflicts
#include "stdafx.h"
#include "windows.h"

#include <TbGes\BodyEdit.h>

#include "GenericControls.h"
#include "MTabber.h"

using namespace Microarea::Framework::TBApplicationWrapper;
using namespace System::CodeDom;
using namespace System::ComponentModel::Design::Serialization;
using namespace System::Drawing;
using namespace System::Windows::Forms;

/////////////////////////////////////////////////////////////////////////////
// 				class GenericWindowWrapperSerializer Implementation
/////////////////////////////////////////////////////////////////////////////

//----------------------------------------------------------------------------	
Statement^ GenericWindowWrapperSerializer::GetConstructor(IDesignerSerializationManager^ manager, EasyBuilderControl^ ebControl)
{
	Point originalPosition = ((GenericWindowWrapper^)ebControl)->OriginalPosition;
	Point currentLocation = GetLocationToSerialize(ebControl);

	IdentifierExpression^ variableDeclExpression = gcnew IdentifierExpression(ebControl->SerializedName);
	ObjectCreateExpression^ creationExpression = AstFacilities::GetObjectCreationExpression
		(
		ebControl->GetType()->ToString(),
		GetParentWindowReference(),
		gcnew PrimitiveExpression(ebControl->FullId),
		gcnew PrimitiveExpression(ebControl->ClassName),
		AstFacilities::GetObjectCreationExpression
		(
		System::Drawing::Point::typeid->FullName,
		gcnew PrimitiveExpression(currentLocation.X),
		gcnew PrimitiveExpression(currentLocation.Y)
		),
		AstFacilities::GetObjectCreationExpression
		(
		System::Drawing::Point::typeid->FullName,
		gcnew PrimitiveExpression(originalPosition.X),
		gcnew PrimitiveExpression(originalPosition.Y)
		),
		gcnew PrimitiveExpression(ebControl->HasCodeBehind)
		);

	SetExpression(manager, ebControl, variableDeclExpression, true);

	return gcnew ExpressionStatement(gcnew AssignmentExpression
		(
		variableDeclExpression,
		AssignmentOperatorType::Assign,
		creationExpression
		));
}

/////////////////////////////////////////////////////////////////////////////
// 				class GenericWindowWrapper Implementation
/////////////////////////////////////////////////////////////////////////////

//----------------------------------------------------------------------------
GenericWindowWrapper::GenericWindowWrapper(System::IntPtr handleWndPtr)
	:
	BaseWindowWrapper(handleWndPtr)
{
	System::Drawing::Point point = Rectangle.Location;
	CPoint aPt(point.X, point.Y);

	CWnd* pParent = GetWnd()->GetParent();
	if (!pParent)
		return;

	pParent->ScreenToClient(&aPt);
	originalPosition = Point(aPt.x, aPt.y);
}

//----------------------------------------------------------------------------
System::String^ GenericWindowWrapper::GetStaticAreaIDFromName(System::String^ name)
{
	if (System::String::Compare(name, MParsedControl::staticAreaName) == 0)
		return MParsedControl::staticAreaID;
	if (System::String::Compare(name, MParsedControl::staticArea1Name) == 0)
		return MParsedControl::staticArea1ID;
	if (System::String::Compare(name, MParsedControl::staticArea2Name) == 0)
		return MParsedControl::staticArea2ID;
	return name;
}

//----------------------------------------------------------------------------
GenericWindowWrapper::GenericWindowWrapper(IWindowWrapperContainer^ parentWindow, System::String^ name, System::String^ controlClass, Point location, Point originalPosition, bool hasCodeBehind)
	:
	BaseWindowWrapper(System::IntPtr::Zero)
{
	this->parent = parentWindow;
	this->name = name;
	this->id = CalcIdFromName(GetStaticAreaIDFromName(name));
	this->HasCodeBehind = hasCodeBehind;
	this->originalPosition = originalPosition;


	if (parentWindow != nullptr && this->HasCodeBehind)
	{
		// prima ci provo con il suo ID, poi con il name
		UINT nID = AfxGetTBResourcesMap()->GetExistingTbResourceID(CString(this->id), TbControls);
		if (nID == 0)
			nID = AfxGetTBResourcesMap()->GetExistingTbResourceID(CString(this->name), TbControls);

		if (nID != 0 && parentWindow->GetType()->IsSubclassOf(WindowWrapperContainer::typeid))
			Handle = ((WindowWrapperContainer^)parentWindow)->GetChildFromCtrlID(nID);

			//Se non trovo il control, do per scontato che il control originale sia stato spostato e 
			//NON applico la customizzazione
		else
			Handle = parentWindow->GetChildFromOriginalPos(originalPosition, controlClass);

		if (Handle == System::IntPtr::Zero)
			Handle = GetChildFromScaledArea((WindowWrapperContainer^)parentWindow, originalPosition);
	}
	else
		Create(parentWindow, location, controlClass);

	// anche se sembra un doppio rispetto alla create, il setting della 
	// proprietà di location fa eseguire il corretto scaling dei dpi
	this->Location = location;

	this->AddChangedProperty("Location");

}

//----------------------------------------------------------------------------
System::IntPtr GenericWindowWrapper::GetChildFromScaledArea(WindowWrapperContainer^ parentWindow, Point p)
{
	CWnd* pWnd = parentWindow->GetWnd();
	if (!pWnd)
		return System::IntPtr::Zero;

	CDC* pDC = pWnd->GetDC();
	if (!pDC)
		return System::IntPtr::Zero;

	double nScalingX = pDC->GetDeviceCaps(LOGPIXELSX) / SCALING_FACTOR;
	double nScalingY = pDC->GetDeviceCaps(LOGPIXELSY) / SCALING_FACTOR;

	if (nScalingX == 1.0 && nScalingY == 1.0)
	{
		pWnd->ReleaseDC(pDC);
		return System::IntPtr::Zero;
	}

	// prima calcolo il rapporto scalato considerando che sulle Y in generale è più preciso
	CPoint aScaledPoint(p.X, p.Y);
	CRect aRectArea(p.X - 10, p.Y - 3, p.X + 10, p.Y + 3);

	aScaledPoint.x = (LONG)(aScaledPoint.x * nScalingX);
	aScaledPoint.y = (LONG)(aScaledPoint.x * nScalingY);
	aRectArea.left = (LONG)(aRectArea.left  * nScalingX);
	aRectArea.right = (LONG)(aRectArea.right * nScalingX);
	aRectArea.top = (LONG)(aRectArea.top  * nScalingY);
	aRectArea.bottom = (LONG)(aRectArea.bottom * nScalingY);

	CWnd* pwndChild = pWnd->GetWindow(GW_CHILD);
	CRect r;
	while (pwndChild)
	{
		pwndChild->GetWindowRect(r);
		pWnd->ScreenToClient(r);
		if (r.left == aScaledPoint.x && r.top == aScaledPoint.y || aRectArea.PtInRect(CPoint(r.left, r.top)))
		{
			pWnd->ReleaseDC(pDC);
			return (System::IntPtr)(int)pwndChild->m_hWnd;
		}
		pwndChild = pwndChild->GetNextWindow();
	}

	pWnd->ReleaseDC(pDC);
	return System::IntPtr::Zero;
}

//----------------------------------------------------------------------------
void GenericWindowWrapper::Location::set(Point p)
{
	//forzo il salvataggio dell'handle di finestra nella mappa delle posizioni ante spostamento
	//così posso agganciarmi successivamente
	if (Parent)
		Parent->SaveChildOriginalPos(Handle, Location);
	__super::Location = p;
}
//----------------------------------------------------------------------------
void GenericWindowWrapper::InitializeName(IWindowWrapperContainer^ parent)
{
	if (!parent)
		return;

	CTBNamespace aNamespace = CreateNamespaceFromParent(parent);
	nameSpace = gcnew NameSpace(gcnew System::String(aNamespace.ToString()));
}

//----------------------------------------------------------------------------
GenericWindowWrapper::~GenericWindowWrapper()
{
	this->!GenericWindowWrapper();
	System::GC::SuppressFinalize(this);
}

//----------------------------------------------------------------------------
GenericWindowWrapper::!GenericWindowWrapper()
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
System::String^	GenericWindowWrapper::Name::get()
{
	if (DesignModeType == EDesignMode::Static)
	{
		CWndObjDescription* pDescri = GetWndObjDescription();
		if (pDescri)
			return gcnew System::String(pDescri->m_strName);

		return System::String::Empty;
	}
	else
	{
		if (nameSpace == nullptr)
			return System::String::Empty;

		return nameSpace->Leaf;
	}
}

//----------------------------------------------------------------------------
void GenericWindowWrapper::Name::set(System::String^ name)
{
	CWndObjDescription* pDescri = GetWndObjDescription();
	if (pDescri)
		pDescri->m_strName = name;

	int pos = nameSpace->FullNameSpace->LastIndexOf(NameSpace::TokenSeparator);
	if (nameSpace == nullptr || pos < 0)
		ASSERT(FALSE);
	else
		nameSpace = gcnew NameSpace(nameSpace->FullNameSpace->Substring(0, pos + 1) + name);
}

//----------------------------------------------------------------------------
System::String^ GenericWindowWrapper::ClassName::get()
{
	TCHAR szClassName[MAX_CLASS_NAME + 1];
	if (!::GetClassName((HWND)Handle.ToInt64(), szClassName, MAX_CLASS_NAME))
		return System::String::Empty;

	return gcnew System::String(szClassName);
}

//----------------------------------------------------------------------------
void GenericWindowWrapper::Parent::set(IWindowWrapperContainer^ value)
{
	__super::Parent = value;
	InitializeName(value);
}

//----------------------------------------------------------------------------
Point GenericWindowWrapper::OriginalPosition::get()
{
	return originalPosition;
}

//----------------------------------------------------------------------------
void GenericWindowWrapper::OriginalPosition::set(Point value)
{
	originalPosition = value;
}

//----------------------------------------------------------------------------
bool GenericWindowWrapper::CanCreate()
{
	return !GetHandle();
}

//----------------------------------------------------------------------------
bool GenericWindowWrapper::CanDropTarget(System::Type^ droppedObject)
{
	return false;
}
//----------------------------------------------------------------------------
void GenericWindowWrapper::Initialize()
{
	__super::Initialize();
}

//----------------------------------------------------------------------------
bool GenericWindowWrapper::Create(IWindowWrapperContainer^ parentWindow, System::Drawing::Point location, System::String^ className)
{
	if (parentWindow == nullptr || !CanCreate())
		return false;

	CWnd* pParentWnd = ((BaseWindowWrapper^)parentWindow)->GetWnd();
	CPoint aPt(location.X, location.Y);

	CFont* pObjectFont = AfxGetThemeManager()->GetFormFont();
	CSize aSize(minSize.Width, minSize.Height);
	SafeMapDialog(pParentWnd->m_hWnd, aSize);//trasformo in pixel
	DWORD styles = WS_CHILD | WS_VISIBLE;
	DWORD exStyles = 0;
	OnCreateStyles(styles, exStyles);

	CTBNamespace aNamespace = CString(CreateNamespaceFromParent(parentWindow));

	HWND hwnd = ::CreateWindowEx(
		exStyles,
		GetWindowClass(),
		aNamespace.GetObjectName(),
		styles,
		aPt.x,
		aPt.y,
		aSize.cx,
		aSize.cy,
		pParentWnd->GetSafeHwnd(),
		NULL,
		NULL,
		NULL);
	if (!hwnd)
		return false;
	
	::SetWindowLong(hwnd, GWL_ID, AfxGetTBResourcesMap()->GetTbResourceID(CString(Id), TbControls));
	nameSpace = gcnew NameSpace(gcnew System::String(aNamespace.ToString()));
	Handle = (System::IntPtr)hwnd;
	::SendMessage(hwnd, WM_SETFONT, (WPARAM)pObjectFont->GetSafeHandle(), FALSE);
	//assegno il tema bcg
	//::SendMessage(hwnd, BCGM_CHANGEVISUALMANAGER, NULL, NULL);

	return true;
}

/////////////////////////////////////////////////////////////////////////////
// 				class MLabel Implementation
/////////////////////////////////////////////////////////////////////////////

//----------------------------------------------------------------------------
MLabel::MLabel(System::IntPtr handleWndPtr)
	:
	GenericWindowWrapper(handleWndPtr)
{
}

//----------------------------------------------------------------------------
MLabel::MLabel(IWindowWrapperContainer^ parentWindow, System::String^ name, System::String^ dummy, Point location, Point originalPosition, bool hasCodeBehind)
	:
	GenericWindowWrapper(parentWindow, name, dummy, location, originalPosition, hasCodeBehind)
{

}

//----------------------------------------------------------------------------
INameSpace^	MLabel::Namespace::get()
{
	return nameSpace;
}

/// <summary>
/// Internal Use
/// </summary>
[ExcludeFromIntellisense]
//----------------------------------------------------------------------------
void MLabel::Initialize()
{
	__super::Initialize();
	minSize = CUtility::GetIdealLabelSize();
	idPrefix = "IDC_STATIC";
}

//----------------------------------------------------------------------------
void MLabel::OnCreateStyles(DWORD& styles, DWORD& exStyles)
{
	styles |= SS_RIGHT;
}

//----------------------------------------------------------------------------
void MLabel::Text::set(System::String^ value)
{
	__super::Text = value;

	//Reimplementata per forzare un invalidate (altrimenti rimangono pezzi di stringhe più lunghe
	CWnd* pWnd = GetWnd();
	if (pWnd)
		pWnd->Invalidate();
}

/////////////////////////////////////////////////////////////////////////////
// 				class GenericButton Implementation
/////////////////////////////////////////////////////////////////////////////

//----------------------------------------------------------------------------
GenericButton::GenericButton(System::IntPtr handleWndPtr)
	:
	GenericWindowWrapper(handleWndPtr)
{
}

//----------------------------------------------------------------------------
GenericButton::GenericButton(IWindowWrapperContainer^ parentWindow, System::String^ name, System::String^ dummy, Point location, Point originalPosition, bool hasCodeBehind)
	:
	GenericWindowWrapper(parentWindow, name, dummy, location, originalPosition, hasCodeBehind)
{
}


//----------------------------------------------------------------------------
bool GenericButton::WndProc(Message% m)
{
	switch (m.Msg)
	{
	case BM_CLICK:
	{
		EasyBuilderEventArgs^ args = gcnew EasyBuilderEventArgs();
		Click(this, args);
		if (args->Handled)
			return true;//mangio il messaggio

		break;
	}

	case UM_EASYBUILDER_ACTION:
	{
		if (((EasyBuilderAction)(int)m.WParam) == Microarea::Framework::TBApplicationWrapper::Clicked)
		{
			EasyBuilderEventArgs^ args = *(EasyBuilderEventArgs^*)(LPARAM)m.LParam.ToInt64();
			Click(this, args);
			return true;//mangio il messaggio (è di easybuilder, non deve essere propagato)
		}
		break;
	}
	}
	return __super::WndProc(m);
}


/////////////////////////////////////////////////////////////////////////////
// 				class GenericEdit Implementation
/////////////////////////////////////////////////////////////////////////////

//----------------------------------------------------------------------------
GenericEdit::GenericEdit(System::IntPtr handleWndPtr)
	:
	GenericWindowWrapper(handleWndPtr)
{
}

//----------------------------------------------------------------------------
GenericEdit::GenericEdit(IWindowWrapperContainer^ parentWindow, System::String^ name, System::String^ dummy, Point location, Point originalPosition, bool hasCodeBehind)
	:
	GenericWindowWrapper(parentWindow, name, dummy, location, originalPosition, hasCodeBehind)
{
}

//----------------------------------------------------------------------------
void GenericEdit::OnCreateStyles(DWORD& styles, DWORD& exStyles) 
{
	styles |= WS_TABSTOP | ES_AUTOHSCROLL;
	exStyles |= WS_EX_CLIENTEDGE;
}
/////////////////////////////////////////////////////////////////////////////
// 				class GenericComboBox Implementation
/////////////////////////////////////////////////////////////////////////////

//----------------------------------------------------------------------------
GenericComboBox::GenericComboBox(System::IntPtr handleWndPtr)
	:
	GenericWindowWrapper(handleWndPtr)
{
}

//----------------------------------------------------------------------------
GenericComboBox::GenericComboBox(IWindowWrapperContainer^ parentWindow, System::String^ name, System::String^ dummy, Point location, Point originalPosition, bool hasCodeBehind)
	:
	GenericWindowWrapper(parentWindow, name, dummy, location, originalPosition, hasCodeBehind)
{
}
//----------------------------------------------------------------------------
void GenericComboBox::OnCreateStyles(DWORD& styles, DWORD& exStyles)
{
	styles |=  WS_TABSTOP | WS_VSCROLL | CBS_DROPDOWN | CBS_SORT;
	exStyles |= WS_EX_CLIENTEDGE;
}
//----------------------------------------------------------------------------
void GenericComboBox::Initialize()
{
	__super::Initialize();
	minSize = CUtility::GetIdeaGenericComboBoxSize();
}

/////////////////////////////////////////////////////////////////////////////
// 				class GenericListBox Implementation
/////////////////////////////////////////////////////////////////////////////

//----------------------------------------------------------------------------
GenericListBox::GenericListBox(System::IntPtr handleWndPtr)
	:
	GenericWindowWrapper(handleWndPtr)
{
	
}


//----------------------------------------------------------------------------
GenericListBox::GenericListBox(IWindowWrapperContainer^ parentWindow, System::String^ name, System::String^ dummy, Point location, Point originalPosition, bool hasCodeBehind)
	:
	GenericWindowWrapper(parentWindow, name, dummy, location, originalPosition, hasCodeBehind)
{
}


//----------------------------------------------------------------------------
bool GenericListBox::WndProc(Message% m)
{
	if (m.Msg == WM_COMMAND)
	{
		WPARAM wParam = (WPARAM)(int)m.WParam;
		LPARAM lParam = (LPARAM)(int)m.LParam;
		DECLARE_WM_COMMAND_PARAMS(wParam, lParam, nID, nCode, hwnd);

		if (nCode == BM_SETCHECK)
		{
			EasyBuilderEventArgs^ args = gcnew EasyBuilderEventArgs();
			CheckedChanged(this, args);
			if (args->Handled)
				return true;//mangio il messaggio
		}
	}

	return __super::WndProc(m);
}

//----------------------------------------------------------------------------
void GenericListBox::OnCreateStyles(DWORD& styles, DWORD& exStyles)
{
	styles |= WS_TABSTOP;
	exStyles |= WS_EX_CLIENTEDGE;
}

/////////////////////////////////////////////////////////////////////////////
// 				class GenericCheckBox Implementation
/////////////////////////////////////////////////////////////////////////////

//----------------------------------------------------------------------------
GenericCheckBox::GenericCheckBox(System::IntPtr handleWndPtr)
	:
	GenericButton(handleWndPtr)
{
}

//----------------------------------------------------------------------------
GenericCheckBox::GenericCheckBox(IWindowWrapperContainer^ parentWindow, System::String^ name, System::String^ dummy, Point location, Point originalPosition, bool hasCodeBehind)
	:
	GenericButton(parentWindow, name, dummy, location, originalPosition, hasCodeBehind)
{
}

//----------------------------------------------------------------------------
bool GenericCheckBox::WndProc(Message% m)
{
	if (m.Msg == WM_COMMAND)
	{
		WPARAM wParam = (WPARAM)(int)m.WParam;
		LPARAM lParam = (LPARAM)(int)m.LParam;
		DECLARE_WM_COMMAND_PARAMS(wParam, lParam, nID, nCode, hwnd);

		if (nCode == CBN_SELCHANGE)
		{
			EasyBuilderEventArgs^ args = gcnew EasyBuilderEventArgs();
			SelectedIndexChanged(this, args);
		}
	}

	return __super::WndProc(m);
}

//----------------------------------------------------------------------------
void GenericCheckBox::Initialize()
{
	__super::Initialize();
	minSize = CUtility::GetIdealCheckRadioButtonSize();
}

//----------------------------------------------------------------------------
void GenericCheckBox::OnCreateStyles(DWORD& styles, DWORD& exStyles) 
{
	styles |= BS_AUTOCHECKBOX | WS_TABSTOP | BS_LEFTTEXT | BS_VCENTER | BS_RIGHT;
}

/////////////////////////////////////////////////////////////////////////////
// 				class GenericRadioButton Implementation
/////////////////////////////////////////////////////////////////////////////

//----------------------------------------------------------------------------
GenericRadioButton::GenericRadioButton(System::IntPtr handleWndPtr)
	:
	GenericButton(handleWndPtr)
{
}

//----------------------------------------------------------------------------
GenericRadioButton::GenericRadioButton(IWindowWrapperContainer^ parentWindow, System::String^ name, System::String^ dummy, Point location, Point originalPosition, bool hasCodeBehind)
	:
	GenericButton(parentWindow, name, dummy, location, originalPosition, hasCodeBehind)
{
}

//----------------------------------------------------------------------------
bool GenericRadioButton::WndProc(Message% m)
{
	if (m.Msg == WM_COMMAND)
	{
		WPARAM wParam = (WPARAM)(int)m.WParam;
		LPARAM lParam = (LPARAM)(int)m.LParam;
		DECLARE_WM_COMMAND_PARAMS(wParam, lParam, nID, nCode, hwnd);

		if (nCode == BM_SETCHECK)
		{
			EasyBuilderEventArgs^ args = gcnew EasyBuilderEventArgs();
			CheckedChanged(this, args);
			if (args->Handled)
				return true;//mangio il messaggio
		}
	}

	return __super::WndProc(m);
}

//----------------------------------------------------------------------------
void GenericRadioButton::Initialize()
{
	__super::Initialize();
	minSize = CUtility::GetIdealCheckRadioButtonSize();
}
//----------------------------------------------------------------------------
void GenericRadioButton::OnCreateStyles(DWORD& styles, DWORD& exStyles) 
{
	styles |= BS_AUTORADIOBUTTON | WS_TABSTOP | BS_RIGHT | BS_VCENTER | BS_LEFTTEXT;
}

/////////////////////////////////////////////////////////////////////////////
// 				class GenericGroupBox Implementation
/////////////////////////////////////////////////////////////////////////////

//----------------------------------------------------------------------------
GenericGroupBox::GenericGroupBox(System::IntPtr handleWndPtr)
	:
	GenericButton(handleWndPtr)
{
}

//----------------------------------------------------------------------------
GenericGroupBox::GenericGroupBox(IWindowWrapperContainer^ parentWindow, System::String^ name, System::String^ dummy, Point location, Point originalPosition, bool hasCodeBehind)
	:
	GenericButton(parentWindow, name, dummy, location, originalPosition, hasCodeBehind)
{
}

//----------------------------------------------------------------------------
void GenericGroupBox::Initialize()
{
	__super::Initialize();
	minSize = CUtility::Get100x100Size();
}
//----------------------------------------------------------------------------
void GenericGroupBox::OnCreateStyles(DWORD& styles, DWORD& exStyles) 
{
	styles |= BS_GROUPBOX | BS_VCENTER | BS_CENTER;
}
//----------------------------------------------------------------------------
bool GenericGroupBox::CanDropTarget(System::Type^ droppedObject)
{
	if (droppedObject->IsSubclassOf(MHotLink::typeid))
		return false;

	return !droppedObject->IsSubclassOf(WindowWrapperContainer::typeid) ;
}

//----------------------------------------------------------------------------
bool GenericGroupBox::CanChangeProperty(System::String^ propertyName)
{
	CWnd* pWnd = GetWnd();
	if (propertyName != "Text" || !pWnd)
		return true;
	return !	CUtility::IsStaticArea(pWnd->GetDlgCtrlID());
}

//----------------------------------------------------------------------------
System::String^ GenericGroupBox::Text::get()
{
	CWndObjDescription* pDescri = GetWndObjDescription();
	if (pDescri && !pDescri->m_strText.IsEmpty())
		return gcnew System::String(pDescri->m_strText);

	CWnd* pWnd = GetWnd();
	if (!pWnd)
		return gcnew System::String("");

	CString str;
	pWnd->GetWindowText(str);
	return gcnew System::String(str);
}

//----------------------------------------------------------------------------
void GenericGroupBox::Text::set(System::String^ value)
{
	CWnd* pWnd = GetWnd();

	if (pWnd)
		pWnd->SetWindowText(CString(value));
	CWndObjDescription* pDescri = GetWndObjDescription();
	if (pDescri)
		pDescri->m_strText = value;
}



/////////////////////////////////////////////////////////////////////////////
// 				class GenericPushButton Implementation
/////////////////////////////////////////////////////////////////////////////

//----------------------------------------------------------------------------
GenericPushButton::GenericPushButton(System::IntPtr handleWndPtr)
	:
	GenericButton(handleWndPtr)
{
}

//----------------------------------------------------------------------------
GenericPushButton::GenericPushButton(IWindowWrapperContainer^ parentWindow, System::String^ name, System::String^ dummy, Point location, Point originalPosition, bool hasCodeBehind)
	:
	GenericButton(parentWindow, name, dummy, location, originalPosition, hasCodeBehind)
{
}

//----------------------------------------------------------------------------
void GenericPushButton::OnCreateStyles(DWORD& styles, DWORD& exStyles) 
{
	styles |= BS_PUSHBUTTON | WS_TABSTOP | BS_VCENTER | BS_CENTER ;
}



/////////////////////////////////////////////////////////////////////////////
// 				class GenericTreeView Implementation
/////////////////////////////////////////////////////////////////////////////

//----------------------------------------------------------------------------
GenericTreeView::GenericTreeView(System::IntPtr handleWndPtr)
	:
	GenericWindowWrapper(handleWndPtr)
{
}

//----------------------------------------------------------------------------
GenericTreeView::GenericTreeView(IWindowWrapperContainer^ parentWindow, System::String^ name, System::String^ dummy, Point location, Point originalPosition, bool hasCodeBehind)
	:
	GenericWindowWrapper(parentWindow, name, dummy, location, originalPosition, hasCodeBehind)
{
}

//----------------------------------------------------------------------------
void GenericTreeView::OnCreateStyles(DWORD& styles, DWORD& exStyles)
{
	styles |= WS_TABSTOP | SS_LEFT | WS_BORDER;
}

//----------------------------------------------------------------------------
bool GenericTreeView::WndProc(Message% m)
{
	switch (m.Msg)
	{
	case BM_CLICK:
	{
		EasyBuilderEventArgs^ args = gcnew EasyBuilderEventArgs();
		Click(this, args);
		if (args->Handled)
			return true;//mangio il messaggio

		break;
	}

	case UM_EASYBUILDER_ACTION:
	{
		if (((EasyBuilderAction)(int)m.WParam) == Microarea::Framework::TBApplicationWrapper::Clicked)
		{
			EasyBuilderEventArgs^ args = *(EasyBuilderEventArgs^*)(LPARAM)m.LParam.ToInt64();
			Click(this, args);
			return true;//mangio il messaggio (è di easybuilder, non deve essere propagato)
		}
		break;
	}
	}
	return __super::WndProc(m);
}

//----------------------------------------------------------------------------
void GenericTreeView::Initialize()
{
	BaseWindowWrapper::Initialize();
	minSize = CUtility::Get200x200Size();
}

/////////////////////////////////////////////////////////////////////////////
// 				class GenericListCtrl Implementation
/////////////////////////////////////////////////////////////////////////////

//----------------------------------------------------------------------------
GenericListCtrl::GenericListCtrl(System::IntPtr handleWndPtr)
	:
	GenericWindowWrapper(handleWndPtr)
{
}

//----------------------------------------------------------------------------
GenericListCtrl::GenericListCtrl(IWindowWrapperContainer^ parentWindow, System::String^ name, System::String^ dummy, Point location, Point originalPosition, bool hasCodeBehind)
	:
	GenericWindowWrapper(parentWindow, name, dummy, location, originalPosition, hasCodeBehind)
{
}

//----------------------------------------------------------------------------
void GenericListCtrl::OnCreateStyles(DWORD& styles, DWORD& exStyles)
{
	styles |= WS_TABSTOP | LVS_ALIGNLEFT;
	exStyles |= WS_EX_CLIENTEDGE;
}

//----------------------------------------------------------------------------
void GenericListCtrl::Initialize()
{
	BaseWindowWrapper::Initialize();
	minSize = CUtility::Get200x200Size();
}

//----------------------------------------------------------------------------
bool GenericListCtrl::WndProc(Message% m)
{
	switch (m.Msg)
	{
	case BM_CLICK:
	{
		EasyBuilderEventArgs^ args = gcnew EasyBuilderEventArgs();
		Click(this, args);
		if (args->Handled)
			return true;//mangio il messaggio

		break;
	}

	case UM_EASYBUILDER_ACTION:
	{
		if (((EasyBuilderAction)(int)m.WParam) == Microarea::Framework::TBApplicationWrapper::Clicked)
		{
			EasyBuilderEventArgs^ args = *(EasyBuilderEventArgs^*)(LPARAM)m.LParam.ToInt64();
			Click(this, args);
			return true;//mangio il messaggio (è di easybuilder, non deve essere propagato)
		}
		break;
	}
	}
	return __super::WndProc(m);
}

//----------------------------------------------------------------------------
EListCtrlAlign GenericListCtrl::Alignment::get()
{
	CWndObjDescription* pDescri = GetWndObjDescription();
	if (pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CListCtrlDescription)))
		return (EListCtrlAlign)((CListCtrlDescription*)pDescri)->m_nAlignment;

	return this->HasStyle(LVS_ALIGNLEFT) ? EListCtrlAlign::Left : EListCtrlAlign::Top;			
}

//----------------------------------------------------------------------------
void GenericListCtrl::Alignment::set(EListCtrlAlign value)
{
	CWndObjDescription* pDescri = GetWndObjDescription();
	if (pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CListCtrlDescription)) && ((CListCtrlDescription*)pDescri)->m_nAlignment != (::ListCtrlAlign)value)
	{
		((CListCtrlDescription*)pDescri)->m_nAlignment = (::ListCtrlAlign)value;
		((CListCtrlDescription*)pDescri)->SetUpdated(&(((CListCtrlDescription*)pDescri)->m_nAlignment));
	}

	switch (value) {
		case EListCtrlAlign::Left:
			this->SetStyle(LVS_ALIGNTOP , LVS_ALIGNLEFT); 			break;
		case EListCtrlAlign::Top:
			this->SetStyle(LVS_ALIGNLEFT , LVS_ALIGNTOP); 			break;
	}

}

//----------------------------------------------------------------------------
EListCtrlViewMode GenericListCtrl::View::get()
{
	CWndObjDescription* pDescri = GetWndObjDescription();
	if (pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CListCtrlDescription)))
		return (EListCtrlViewMode)((CListCtrlDescription*)pDescri)->m_nView;

	if (this->HasStyle(LVS_LIST)) 		return EListCtrlViewMode::List;
	if (this->HasStyle(LVS_REPORT)) 	return EListCtrlViewMode::Report;
	if (this->HasStyle(LVS_SMALLICON)) 	return EListCtrlViewMode::SmallIcon;
	return EListCtrlViewMode::Icon;
}

//----------------------------------------------------------------------------
void GenericListCtrl::View::set(EListCtrlViewMode value)
{

	CWndObjDescription* pDescri = GetWndObjDescription();
	if (pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CListCtrlDescription)) && ((CListCtrlDescription*)pDescri)->m_nView != (::ListCtrlViewMode)value)
	{
		((CListCtrlDescription*)pDescri)->m_nView = (::ListCtrlViewMode)value;
		((CListCtrlDescription*)pDescri)->SetUpdated(&(((CListCtrlDescription*)pDescri)->m_nView));
	}

	switch (value) {
		case EListCtrlViewMode::Icon:
			this->SetStyle(LVS_SMALLICON | LVS_LIST| LVS_REPORT, LVS_ICON);
			break;
		case EListCtrlViewMode::List:
			this->SetStyle(LVS_SMALLICON | LVS_REPORT, LVS_LIST);
			break;
		case EListCtrlViewMode::Report:
			this->SetStyle(LVS_SMALLICON | LVS_LIST, LVS_REPORT);
			break;
		case EListCtrlViewMode::SmallIcon:
			this->SetStyle(LVS_LIST| LVS_REPORT , LVS_SMALLICON);
			break;
	}
}




/////////////////////////////////////////////////////////////////////////////
// 				class GenericActiveX Implementation
/////////////////////////////////////////////////////////////////////////////

//----------------------------------------------------------------------------
GenericActiveX::GenericActiveX(System::IntPtr handleWndPtr)
	:
	BaseWindowWrapper(handleWndPtr)
{
}

//----------------------------------------------------------------------------
GenericActiveX::GenericActiveX(IWindowWrapperContainer^ parentWindow, System::String^ name, System::String^ dummy, Point location, Point originalPosition, bool hasCodeBehind)
	:
	BaseWindowWrapper(System::IntPtr::Zero)
{
}

//----------------------------------------------------------------------------
void GenericActiveX::Initialize()
{
	__super::Initialize();
	minSize = CUtility::Get100x100Size();
}
//----------------------------------------------------------------------------
System::String^ GenericActiveX::ControlClass::get()
{
	CWnd* pWnd = GetWnd();
	if (!pWnd)
		return "";
	COleControlSite* pSite = pWnd->GetControlSite();
	if (!pSite)
		return "";
	return "";
}
//----------------------------------------------------------------------------
void GenericActiveX::ControlClass::set(System::String^ value)
{ 
	CWnd* pParentWnd = Parent ? (CWnd*)Parent->GetWndPtr().ToInt64() : NULL;
	Point pt = Location;
	System::Drawing::Size sz = Size;

	CWnd* pWnd = GetWnd();
	if (pWnd)
		pWnd->DestroyWindow();
	CreateActiveX(CString(value), pParentWnd, CRect(CPoint(pt.X, pt.Y), CSize(sz.Width, sz.Height)));
}

//----------------------------------------------------------------------------
bool GenericActiveX::Create(IWindowWrapperContainer^ parentWindow, System::Drawing::Point location, System::String^ className)
{
	if (parentWindow == nullptr || !CanCreate())
		return false;

	CWnd* pParentWnd = ((BaseWindowWrapper^)parentWindow)->GetWnd();
	CPoint aPt(location.X, location.Y);

	CSize aSize(minSize.Width, minSize.Height);
	SafeMapDialog(pParentWnd->m_hWnd, aSize);//trasformo in pixel
	
	CreateNamespaceFromParent(parentWindow);

	return CreateActiveX(_T("{CD6C7868 - 5864 - 11D0 - ABF0 - 0020AF6B0B7A}"), pParentWnd, CRect(aPt, aSize));
}

//----------------------------------------------------------------------------
bool GenericActiveX::CreateActiveX(CString sClass, CWnd* pParentWnd, CRect rect)
{
	IID clsId;
	if (SUCCEEDED(CLSIDFromString(sClass, &clsId)))
	{
		DWORD styles = WS_CHILD | WS_VISIBLE;
		DWORD exStyles = 0;
		OnCreateStyles(styles, exStyles);
		CWnd wnd;
		if (!(wnd.CreateControl(clsId, NULL, styles, rect, pParentWnd, AfxGetTBResourcesMap()->GetTbResourceID(CString(Id), TbControls))))
		{
			ASSERT(FALSE);
			return false;
		}
		Handle = (System::IntPtr)wnd.Detach();
		return true;
	}	
	return false;

}